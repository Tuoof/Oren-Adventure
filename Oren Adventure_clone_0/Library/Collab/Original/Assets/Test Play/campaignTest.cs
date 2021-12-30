using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Network;
using Unity.Netcode;

public class campaignTest
{
    private MenuControl _controller;
    
    // A Test behaves as an ordinary method
    [Test]
    public void campaignTestSimplePasses()
    {   
        var localPlay = GameObject.Find("StartLocalGame");
        _controller.PlayButtonLocalGame();
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator campaignTestWithEnumeratorPasses()
    {
        yield return null;
    }
}
