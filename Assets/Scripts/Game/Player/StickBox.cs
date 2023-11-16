using System;
using UnityEngine;

namespace Game.Player
{
    //used to detect a player has been hit, the suscriptor to the event is PlayerControl
    public class StickBox : MonoBehaviour
    {
        public event Action<int> PlayerHit = _ => {};
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
