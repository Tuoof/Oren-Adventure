using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class BulletSP : MonoBehaviour
    {
        [SerializeField] float speed;
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
            rb.velocity = transform.right * speed;
            // rb.velocity = new Vector2(speed * Time.deltaTime, rb.velocity.y);
        }

        // Update is called once per frame
        void Update()
        {
            liveTime -= Time.deltaTime;
            if (Vector3.Distance(initialPosition, transform.position) >= maxDistanceBullet)
            {
                Destroy(this.gameObject);
            }
        }


        void OnTriggerEnter2D(Collider2D hitInfo)
        {
            Debug.Log(hitInfo.name);
            SpiderEnemySP spiderEnemySP = hitInfo.GetComponent<SpiderEnemySP>();
            if (spiderEnemySP != null)
            {
                spiderEnemySP.TakeDamage(Damage);
            }
            Destroy(this.gameObject);
        }
    }
}
