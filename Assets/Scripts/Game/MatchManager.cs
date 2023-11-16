using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FishNet;
using FishNet.Object;
using Game.Player;
using UnityEngine;

namespace Game
{
    //This class exists to validate player's interaction, for now handles Hits, Deaths and Respawns
    public class MatchManager : NetworkBehaviour
    {
        //Singleton access
        public static MatchManager Instance { get; private set; }

        private Dictionary<int, float> nextHitTime = new Dictionary<int, float>();
        
        private void Awake()
        {
            Instance = this;
        }

        [ServerRpc(RequireOwnership = false)]
        public void Hit(int source, int target)
        {
            var players = FindObjectsOfType<PlayerControl>();
            var sourcePlayer = players.FirstOrDefault(x => x.OwnerId == source);
            var targetPlayer = players.FirstOrDefault(x => x.OwnerId == target);
            
            if (sourcePlayer == null || targetPlayer == null)
                return;

            //don't allow hit from too far away
            if(Vector3.Distance(sourcePlayer.transform.position, targetPlayer.transform.position) > 3)
                return;
            
            //don't allow too fast hits
            if (nextHitTime.TryGetValue(source, out var time))
            {
                if (Time.time < time)
                {
                    return;
                }
            }
            
            nextHitTime[source] = Time.time + .3f;
            
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
            
            //don't kill a player with hitpoints remaining
            if (playerControl == null || playerControl.HitPoints > 0)
                yield break;
            
            var conn = player.Owner;
            InstanceFinder.ServerManager.Despawn(player);

            yield return new WaitForSeconds(2);

            PlayerSpawner.Instance.Spawn(conn, IsServer);
        }
    }
}