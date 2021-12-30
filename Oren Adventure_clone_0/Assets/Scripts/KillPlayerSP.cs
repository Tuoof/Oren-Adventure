using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class KillPlayerSP : MonoBehaviour
    {

        // Start is called before the first frame update
        private void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.tag == "Enemy")
            {
                var player = this.gameObject.GetComponent<PlayerControllerSP>();
                PlayerHealthSP.singleton.DealDamage();
                player.RespawnPlayer();
                // StageManagerSP.Singleton.MovePlayerToCheckpoint();
            }
        }
    }
}

