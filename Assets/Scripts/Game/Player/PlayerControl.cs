using FishNet;
using FishNet.Object;
using FishNet.Object.Prediction;
using FishNet.Transporting;
using UnityEngine;

namespace Game.Player
{
    public class PlayerControl : NetworkBehaviour
    {
        [SerializeField]
        [Range(0.1f,1f)]
        private float _moveRate = 0.2f;
        private float _nextHitTime;
        private bool _hit;
        
        [SerializeField]
        private GameObject[] colorElements;
        
        [SerializeField]
        private NetworkObject tombstonePrefab;

        private Rigidbody _rigidbody;
        private Animator _animator;
        private static readonly int hitTrigger = Animator.StringToHash("Hit");
        public int HitPoints = 30;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _animator = GetComponentInChildren<Animator>();
            GetComponentInChildren<StickBox>().PlayerHit += OnPlayerHit;
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
            InstanceFinder.TimeManager.OnPostTick += TimeManager_OnPostTick;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null)
            {
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
                InstanceFinder.TimeManager.OnPostTick -= TimeManager_OnPostTick;
            }
        }

        public override void OnStartClient()
        {
            PredictionManager.OnPreReplicateReplay += PredictionManager_OnPreReplicateReplay;
            Recolor();
        }
        public override void OnStopClient()
        {            
            PredictionManager.OnPreReplicateReplay -= PredictionManager_OnPreReplicateReplay;
        }

        private void Update()
        {
            if (IsOwner && Input.GetKeyDown(KeyCode.Space) && Time.time > _nextHitTime)
            {
                _animator.SetTrigger(hitTrigger);
                Hit();
            }
        }
        
        [ServerRpc]
        private void Hit()
        {
            if (Time.time > _nextHitTime)
            {
                _nextHitTime = Time.time + .3f;
                AnimateHit();
            }
        }

        [ObserversRpc(ExcludeOwner = true)]
        private void AnimateHit()
        {
            _animator.SetTrigger(hitTrigger);
        }

        /// <summary>
        /// Called every time any predicted object is replaying. Replays only occur for owner.
        /// Currently owners may only predict one object at a time.
        /// </summary>
        private void PredictionManager_OnPreReplicateReplay(uint arg1, PhysicsScene arg2, PhysicsScene2D arg3)
        {
            /* Server does not replay so it does
             * not need to add gravity. */
            if (!IsServer)
                AddGravity();
        }


        private void TimeManager_OnTick()
        {
            if (IsOwner)
            {
                Reconciliation(default, false);
                BuildMoveData(out MoveData md);
                Move(md, false);
            }
            if (IsServer)
            {
                Move(default, true);
            }

            /* Server and all clients must add the additional gravity.
             * Adding gravity is not necessarily required in general but
             * to make jumps more snappy extra gravity is added per tick.
             * All clients and server need to simulate the gravity to keep
             * prediction equal across the network. */
            AddGravity();
        }

        private void TimeManager_OnPostTick()
        {
            /* Reconcile is sent during PostTick because we
             * want to send the rb data AFTER the simulation. */
            if (IsServer)
            {
                ReconcileData rd = new ReconcileData(transform.position, transform.rotation, _rigidbody.velocity, _rigidbody.angularVelocity);
                Reconciliation(rd, true);
            }
        }


        /// <summary>
        /// Builds a MoveData to use within replicate.
        /// </summary>
        /// <param name="md"></param>
        private void BuildMoveData(out MoveData md)
        {
            md = default;

            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            if (horizontal == 0f && vertical == 0f)
                return;

            md = new MoveData(_hit, horizontal, vertical);
        }

        /// <summary>
        /// Adds gravity to the rigidbody.
        /// </summary>
        private void AddGravity()
        {
            _rigidbody.AddForce(Physics.gravity * 2f);
        }

        [Replicate]
        private void Move(MoveData md, bool asServer, Channel channel = Channel.Unreliable, bool replaying = false)
        {
            var movement = new Vector2(md.Horizontal, md.Vertical);
            if (movement.magnitude == 0)
                return;
            
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            
            var rbTransform = _rigidbody.transform;
            var position = rbTransform.position + rbTransform.forward * (movement.y * _moveRate);
            var deltaRotation = Quaternion.Euler(rbTransform.up * (movement.x * 5f));
            var rotation = rbTransform.rotation * deltaRotation;
            _rigidbody.Move(position, rotation);
        }

        [Reconcile]
        private void Reconciliation(ReconcileData rd, bool asServer, Channel channel = Channel.Unreliable)
        {
            transform.position = rd.Position;
            transform.rotation = rd.Rotation;
            _rigidbody.velocity = rd.Velocity;
            _rigidbody.angularVelocity = rd.AngularVelocity;
        }

        private void Recolor()
        {
            for (var index = 0; index < colorElements.Length; index++)
                colorElements[index].GetComponent<Renderer>().material.color = Extensions.Color.Random(OwnerId);
        }
        
        private void OnPlayerHit(int playerId)
        {
            if (IsOwner)
                MatchManager.Instance.Hit(OwnerId, playerId);
        }

        public void TakeDamage(int damage, Vector3 from)
        {
            if (IsServer)
            {
                HitPoints -= damage;
                if (HitPoints <= 0)
                    Die();
                else
                    _rigidbody.AddForce((transform.position - from) * 5f, ForceMode.Impulse);
            }
        }

        private void Die()
        {
            if (IsServer)
            {
                NetworkObject tombstone = Instantiate(tombstonePrefab, transform.position, Quaternion.identity);
                InstanceFinder.ServerManager.Spawn(tombstone);
                MatchManager.Instance.Kill(NetworkObject);
            }
        }
    }
}

