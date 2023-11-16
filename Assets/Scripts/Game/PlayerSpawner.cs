using System;
using System.Collections.Generic;
using Extensions;
using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using UnityEngine;

namespace Game
{
    //mostly copied from FishNet.Component.Spawning.PlayerSpawner
    //only to expose the actual spawning method to allow respawning
    public class PlayerSpawner : MonoBehaviour
    {
        public event Action<NetworkObject> OnSpawned;
        [Tooltip("Prefab to spawn for the player.")]
        [SerializeField]
        private List<NetworkObject> _playerPrefabs = new();
        [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
        [SerializeField]
        private bool _addToDefaultScene = true;
        [Tooltip("Areas in which players may spawn.")]
        public Transform[] Spawns = new Transform[0];
        private NetworkManager _networkManager;
        private int _nextSpawn;
        
        //Singleton access
        public static PlayerSpawner Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            InitializeOnce();
        }

        private void OnDestroy()
        {
            if (_networkManager != null)
                _networkManager.SceneManager.OnClientLoadedStartScenes -= Spawn;
        }
        
        private void InitializeOnce()
        {
            _networkManager = InstanceFinder.NetworkManager;
            if (_networkManager == null)
            {
                Debug.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
                return;
            }

            _networkManager.SceneManager.OnClientLoadedStartScenes += Spawn;
        }
        
        public void Spawn(NetworkConnection conn, bool asServer)
        {
            if (!asServer)
                return;
            if (_playerPrefabs == null || _playerPrefabs.Count == 0)
            {
                Debug.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
                return;
            }

            Vector3 position;
            Quaternion rotation;
            SetSpawn(out position, out rotation);

            NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefabs.Random(), position, rotation, true);
            _networkManager.ServerManager.Spawn(nob, conn);

            //If there are no global scenes 
            if (_addToDefaultScene)
                _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            OnSpawned?.Invoke(nob);
        }
        private void SetSpawn(out Vector3 pos, out Quaternion rot)
        {
           pos = Vector3.zero;
           rot = Quaternion.identity;

            Transform result = Spawns[_nextSpawn];
            if (result != null)
            {
                pos = result.position;
                rot = result.rotation;
            }

            _nextSpawn++;
            if (_nextSpawn >= Spawns.Length)
                _nextSpawn = 0;
        }
    }
}