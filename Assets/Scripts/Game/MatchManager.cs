using System.Collections;
using System.Linq;
using FishNet;
using FishNet.Object;
using Game.Player;
using UnityEngine;

namespace Game
{
    public class MatchManager : NetworkBehaviour
    {
        /// <summary>
        /// Prefab to spawn for the player.
        /// </summary>
        [Tooltip("Prefab to spawn for the player.")]
        [SerializeField]
        private NetworkObject _playerPrefab;
        
        public static MatchManager Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
            InstanceFinder.TimeManager.OnTick += TimeManager_OnTick;
        }

        private void OnDestroy()
        {
            if (InstanceFinder.TimeManager != null) 
                InstanceFinder.TimeManager.OnTick -= TimeManager_OnTick;
        }

        private void TimeManager_OnTick()
        {
            
        }

        [ServerRpc(RequireOwnership = false)]
        public void Hit(int source, int target)
        {
            var players = FindObjectsOfType<PlayerControl>();
            var sourcePlayer = players.FirstOrDefault(x => x.OwnerId == source);
            var targetPlayer = players.FirstOrDefault(x => x.OwnerId == target);
            
            if (sourcePlayer == null || targetPlayer == null)
            {
                return;
            }
            
            if(Vector3.Distance(sourcePlayer.transform.position, targetPlayer.transform.position) > 2)
            {
                return;
            }

            targetPlayer.TakeDamage(10, sourcePlayer.transform.position);
        }

        [ServerRpc(RequireOwnership = false)]
        public void Kill(NetworkObject networkObject)
        {
            StartCoroutine(KillAndRespawn(networkObject));
        }

        private IEnumerator KillAndRespawn(NetworkObject player)
        {
            var playerControl = FindObjectsOfType<PlayerControl>()
                .FirstOrDefault(x => x.OwnerId == player.OwnerId);
            
            if (playerControl == null || playerControl.HitPoints > 0)
                yield break;
            
            var conn = player.Owner;
            InstanceFinder.ServerManager.Despawn(player);

            yield return new WaitForSeconds(2);

            PlayerSpawner.Instance.Spawn(conn, IsServer);
        }
    }
}