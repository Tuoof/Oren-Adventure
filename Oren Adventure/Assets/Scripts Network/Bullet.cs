using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class Bullet : MonoBehaviour
    {
        [SerializeField] float speed;
        public PlayerController owner;
        public ClientPlayerController clientOwner;

        public int Damage = 10;
        public float maxDistanceBullet = 10;
        public float liveTime = 1;
        public Vector3 initialPosition;
        public Rigidbody2D rb;
        // Start is called before the first frame update
        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            initialPosition = transform.position;
        }

        // Update is called once per frame
        void Update()
        {
            if (!NetworkManager.Singleton.IsServer) return;
            rb.velocity = transform.right * speed;

            liveTime -= Time.deltaTime;
            if (Vector3.Distance(initialPosition, transform.position) >= maxDistanceBullet)
            {
                Destroy(this.gameObject);
            }
        }


        void OnTriggerEnter2D(Collider2D hitInfo)
        {
            if (!NetworkManager.Singleton.IsServer)
            return;

            Debug.Log(hitInfo.name);
            var Enemy = hitInfo.GetComponent<Enemy>();
            if (Enemy != null)
            {
                Destroy(this.gameObject);
                Enemy.TakeDamage(Damage);
                return;
            }
        }
    }
}
