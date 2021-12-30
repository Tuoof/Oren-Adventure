using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class WinPlayer : NetworkBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerStay2D(Collider2D collider)
        {
            if (collider.tag == "Player")
            {
                foreach (NetworkClient networkedClient in NetworkManager.Singleton.ConnectedClientsList)
                {
                    StageManager.Singleton.SetGameEnd(GameOverReason.Win);
                    StageManager.Singleton.ExitGame();
                }
            }
        }
    }
}

