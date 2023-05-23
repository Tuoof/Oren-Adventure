using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class PlayerHealth : NetworkBehaviour
    {
        // public static PlayerHealth Singleton { get; private set; }
        public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
        public int maxHealth;
        public bool m_IsAlive = true;
        // Start is called before the first frame update
        private void Awake()
        {
            currentHealth.Value = maxHealth;
        }

        public override void OnNetworkSpawn()
        {
            currentHealth.OnValueChanged += OnLivesChanged;

            if (!StageManager.Singleton)
                StageManager.OnSingletonReady += SubscribeToDelegatesAndUpdateValues;
            else
                SubscribeToDelegatesAndUpdateValues();
        }
        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (IsClient)
            {
                currentHealth.OnValueChanged -= OnLivesChanged;
            }
        }

        private void SubscribeToDelegatesAndUpdateValues()
        {
            if (IsClient && IsOwner)
            {
                StageManager.Singleton.SetLives(currentHealth.Value);
            }
        }

        private void OnLivesChanged(int previousAmount, int currentAmount)
        {
            // Hide graphics client side upon death
            if (currentAmount <= 0 && IsClient && TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                // spriteRenderer.enabled = false;

                if (!IsOwner) return;
            Debug.LogFormat("Lives {0} ", currentAmount);
            if (StageManager.Singleton != null) StageManager.Singleton.SetLives(currentHealth.Value);

            if (currentHealth.Value <= 0)
            {
                m_IsAlive = false;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        [ServerRpc]
        public void DealDamageServerRpc()
        {
            currentHealth.Value--;
        }
    }
}
