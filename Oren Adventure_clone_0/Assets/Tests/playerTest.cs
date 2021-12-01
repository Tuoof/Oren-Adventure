using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Advent;

namespace Tests
{
    public class playerTest
    {
        private PlayerControllerSP _controller;
        // A Test behaves as an ordinary method
        [Test]
        public void playerJumpTest()
        {
            var gameObject = new GameObject();
            _controller = gameObject.AddComponent<PlayerControllerSP>();

            _controller.isGrounded = true;
            _controller.jumpForce = 10;
            _controller.extraJumpValue = 1;
            _controller.Jump();

            Assert.That(() => _controller.Jump(), Throws.TypeOf<UnityException>());
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator playerTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}

