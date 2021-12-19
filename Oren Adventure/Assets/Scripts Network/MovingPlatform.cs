using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace oren_Network
{
    public class MovingPlatform : NetworkBehaviour
    {
        public float speed;
        public int startingPoint;
        public Transform[] points;

        public int i;
        // Start is called before the first frame update
        void Start()
        {
            transform.position = points[startingPoint].position;
        }

        // Update is called once per frame
        void Update()
        {
            if(Vector2.Distance(transform.position, points[i].position)<0.2f)
            {
                i++;
                if(i== points.Length)
                {
                    i = 0;
                }
            }
            //  moving platform to the point position with index"i"
            transform.position = Vector2.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
        }
        public void OnCollisionEnter2D(Collision2D collision)
        {
            collision.transform.SetParent(transform);
        }
        public void OnCollisionExit2D(Collision2D collision)
        {
            collision.transform.SetParent(null);
        }

    }
}


