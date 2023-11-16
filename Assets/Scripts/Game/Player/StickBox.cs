using System;
using UnityEngine;

namespace Game.Player
{
    public class StickBox : MonoBehaviour
    {
        public event Action<int> PlayerHit = player => {};
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                var playerId = other.GetComponent<PlayerControl>().OwnerId;
                PlayerHit.Invoke(playerId);
            }
        }
    }
}
