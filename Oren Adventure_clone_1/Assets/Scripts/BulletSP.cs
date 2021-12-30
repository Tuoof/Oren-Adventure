using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class BulletSP : MonoBehaviour
    {
        [SerializeField] float speed;
        public PlayerControllerSP owner;

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
            rb.velocity = transform.right * speed;

            liveTime -= Time.deltaTime;
            if (Vector3.Distance(initialPosition, transform.position) >= maxDistanceBullet)
            {
                Destroy(this.gameObject);
            }
        }


        void OnTriggerEnter2D(Collider2D hitInfo)
        {
            Debug.Log(hitInfo.name);
            var EnemySP = hitInfo.GetComponent<EnemySP>();
            
            if (EnemySP != null)
            {
                Destroy(this.gameObject);
                EnemySP.TakeDamage(Damage);
                return;
            }
        }
    }
}
