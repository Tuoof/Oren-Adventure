using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    [DisallowMultipleComponent]
    public class KillPlayer : NetworkBehaviour
    {
        
        private ClientRpcParams m_OwnerRPCParams;
        private PlayerHealth playerHealth;
        private ClientPlayerController clientPlayerController;
        // Start is called before the first frame update
        public override void OnNetworkSpawn()
        {
            if (IsServer) m_OwnerRPCParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } } };
            clientPlayerController = gameObject.GetComponent<ClientPlayerController>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.tag == "Enemy")
            {
                var respawn = true;
                var player = clientPlayerController;
                playerHealth = gameObject.GetComponent<PlayerHealth>();
                RespawnPlayerServerRpc(respawn);
                // player.RespawnPlayerServerRpc();
                // playerHealth.DealDamageServerRpc();
            }
        }

        [ServerRpc]
        private void RespawnPlayerServerRpc(bool respawned)
        {
            clientPlayerController.PlayerRespawned.Value = respawned;
        }

        private void OnTriggerExit2D(Collider2D collider)
        {
            if (collider.tag == "Enemy")
            {
                var respawn = false;
                RespawnPlayerServerRpc(respawn);
            }
        }
    }
}

