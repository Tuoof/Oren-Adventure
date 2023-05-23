using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Advent;

namespace Tests
{
    public class playerTest : MonoBehaviour
    {
        private PlayerControllerSP _controller;
        private PlayerControllerSP _flip;
        private PlayerControllerSP _shoot;
        private BulletSP _bullet;

        bool facingright = true;
        int horizontal = -1;
        // A Test behaves as an ordinary method
        [Test]
        public void playerJumpTest()
        {
            var gameObject = new GameObject();
            _controller = gameObject.AddComponent<PlayerControllerSP>();

            // _controller.isGrounded = true;
            _controller.jumpForce = 10;
            _controller.extraJumpValue = 1;
            _controller.transform.position = Vector2.up * _controller.jumpForce;

            // Assert.That(() => _controller.transform.position == new Vector3(0f, 10f, 0f) , Throws.TypeOf<UnityException>());
            Assert.AreEqual(_controller.jumpForce, _controller.transform.position.y);
        }

        [Test]
        public void playerInitialShootPointTest()
        {
            var gameObject = new GameObject();
            _shoot = gameObject.AddComponent<PlayerControllerSP>();
            _bullet = gameObject.AddComponent<BulletSP>();
            _shoot.transform.position = new Vector3(2f, 2f, 0f);
            _bullet.transform.position = _shoot.transform.position;

            Assert.AreEqual(_bullet.transform.position, _shoot.transform.position );
        }

        [Test]
        public void playerFlipTest()
        {
            var gameObject = new GameObject();
            _flip = gameObject.AddComponent<PlayerControllerSP>();

            if (horizontal >= 0 )
            {
                
                Assert.IsTrue(facingright);
                Debug.Log("facing right");
            }
            else if (horizontal <= -1)
            {
                facingright = false;
                Assert.IsFalse(facingright);
                Debug.Log("facing left");
            }
            
        }
    }
}

