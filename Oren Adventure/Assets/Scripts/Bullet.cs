using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] float speed;
    public int Damage = 10;
    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.right * speed;
        // rb.velocity = new Vector2(speed * Time.deltaTime, rb.velocity.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D (Collider2D hitInfo)
    {
        Debug.Log(hitInfo.name);
        SpiderEnemy spiderEnemy = hitInfo.GetComponent<SpiderEnemy>();
        if(spiderEnemy != null)
        {
            spiderEnemy.TakeDamage(Damage);
        }
        Destroy(gameObject);
    }
}
