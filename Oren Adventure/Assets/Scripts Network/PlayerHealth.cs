using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class PlayerHealth : NetworkBehaviour
    {
        public static PlayerHealth Singleton { get; private set; }
        public NetworkVariable<int> currentHealth = new NetworkVariable<int>();
        private NetworkVariable<int> m_Lives = new NetworkVariable<int>(3);
        public int maxHealth;
        private bool m_IsAlive = true;
        // Start is called before the first frame update
        private void Awake()
        {
            Singleton = this;
        }
        public override void OnNetworkSpawn()
        {
            currentHealth.Value = maxHealth;
        }

        private void OnLivesChanged(int previousAmount, int currentAmount)
        {
            // Hide graphics client side upon death
            if (currentAmount <= 0 && IsClient && TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                spriteRenderer.enabled = false;

            if (!IsOwner) return;
            Debug.LogFormat("Lives {0} ", currentAmount);
            if (StageManager.Singleton != null) StageManager.Singleton.SetLives(m_Lives.Value);

            if (m_Lives.Value <= 0)
            {
                m_IsAlive = false;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DealDamage()
        {
            currentHealth.Value--;

            if (currentHealth.Value <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
