using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class Winbox : NetworkBehaviour
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
                // var player = NetworkManager.Singleton.ConnectedClients.Values;
                StageManager.Singleton.Winstate = true;
            }
        }
    }
}

