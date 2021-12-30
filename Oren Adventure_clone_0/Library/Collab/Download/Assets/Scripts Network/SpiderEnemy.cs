using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class SpiderEnemy : NetworkBehaviour
    {
        // public NetworkVariable<int> health = new NetworkVariable<int>();
        public int health = 100;
        public GameObject deathEffect;

        public void TakeDamage(int Damage)
        {
            health -= Damage;
            if (health <= 0)
            {
                Die();
            }
        }
        void Die()
        {
            Destroy(gameObject);
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

