using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class KillPlayerSP : MonoBehaviour
    {
        public StageManagerSP stageManagerSP;
        // Start is called before the first frame update
        void Start()
        {
            stageManagerSP = FindObjectOfType<StageManagerSP>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.tag == "Player")
            {
                stageManagerSP.RespawnPlayer();
                // FindObjectOfType<PlayerHealth>().DealDamage();
                PlayerHealthSP.instance.DealDamage();
            }
        }
    }
}

