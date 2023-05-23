using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Advent;

namespace Tests
{
    public class bulletTest
    {
        private Rigidbody2D rb;
        private BulletSP bullet;
        private EnemySP enemy;
        private bool maxDistancePassed = false;
        // A Test behaves as an ordinary method
        [Test]
        public void bulletCollisionTest()
        {
            var gameObject = new GameObject();  
            enemy = gameObject.AddComponent<EnemySP>();
            Assert.IsNotNull(enemy);

            if (enemy != null )
            enemy.TakeDamage(0);

            Assert.NotZero(enemy.health);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator bulletMoveDistanceTest()
        {
            // var bullet = MonoBehaviour.Instantiate(Resources.Load<GameObject>("Bullet"));
            var gameObject = new GameObject();            
            bullet = gameObject.AddComponent<BulletSP>();
            // var bullet.rb = AddComponent<Rigidbody2D>();
            
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            // act
            bullet.maxDistanceBullet = 5;

            for(int i = 0; i < 20; i++)
            {
                bullet.transform.position += new Vector3(1.0f, 0f, 0f);

                if ((bullet.initialPosition + bullet.transform.position).magnitude == bullet.maxDistanceBullet)
                {
                    maxDistancePassed = true;
                    Assert.IsTrue(maxDistancePassed);
                }
            }

            if(Vector3.Distance(bullet.initialPosition, bullet.transform.position) >= bullet.maxDistanceBullet)
            {
                Assert.Greater(bullet.transform.position.x, bullet.maxDistanceBullet);
                Debug.Log("bullet passed max distance");
            }
            yield return null;
        }
    }
}

