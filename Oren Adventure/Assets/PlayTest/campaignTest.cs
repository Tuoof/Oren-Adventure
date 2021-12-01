using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Advent;

public class campaignTest
{
    private PlayerControllerSP _controller;
    
    // A Test behaves as an ordinary method
    [Test]
    public void campaignTestSimplePasses()
    {

    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator campaignTestWithEnumeratorPasses()
    {
        var gameObject = new GameObject();
        _controller = gameObject.AddComponent<PlayerControllerSP>();
        
        _controller.isGrounded = true;
        // _controller.IsGrounded() = true;
        _controller.Jump();
        yield return null;
    }
}
