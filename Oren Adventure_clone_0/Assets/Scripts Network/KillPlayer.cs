using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class KillPlayer : NetworkBehaviour
    {
        public StageManager stageManager;
        // Start is called before the first frame update
        void Start()
        {
            stageManager = FindObjectOfType<StageManager>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                var respanwPlayer = collision.gameObject.GetComponent<ClientPlayerController>();
                respanwPlayer.RespawnPlayer();
                // FindObjectOfType<PlayerHealth>().DealDamage();
                PlayerHealth.instance.DealDamage();
            }
        }
    }
}

