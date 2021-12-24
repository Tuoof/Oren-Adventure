using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class KillPlayer : NetworkBehaviour
    {
        private ClientRpcParams m_OwnerRPCParams;
        // Start is called before the first frame update
        public override void OnNetworkSpawn()
        {
            if (IsServer) m_OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.tag == "Enemy")
            {
                var player = gameObject.GetComponent<ClientPlayerController>();
                player.RespawnPlayerServerRpc(m_OwnerRPCParams);
                // PlayerHealth.Singleton.DealDamage();
            }
        }
    }
}

