using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class MovingEnemySP : MonoBehaviour
    {
        public float speed;
        public int startingPoint;
        public Transform[] points;
        public bool facingRight;

        public int i;
        // Start is called before the first frame update
        void Start()
        {
            transform.position = points[startingPoint].position;
        }

        // Update is called once per frame
        void Update()
        {
            if (Vector2.Distance(transform.position, points[i].position) < 0.2f)
            {
                i++;
                if (i == points.Length)
                {
                    Flip();
                    i = 0;
                }
            }
            //  moving platform to the point position with index"i"
            transform.position = Vector2.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);
        }
        void Flip()
        {
            facingRight = !facingRight;

            transform.Rotate(0f, 180f, 0f);
        }

    }
}


