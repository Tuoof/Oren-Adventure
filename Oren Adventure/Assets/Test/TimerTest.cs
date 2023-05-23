using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using oren_Network;
using Unity.Netcode;
using UnityEngine.TestTools;

public class TimerTest : NetworkBehaviour
{
    private StageManager _gameManager;
    private bool timerStarted = true;
    // public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);

    [Test]
    public void TimeVariableTest()
    {
        var gameObject = new GameObject();
        _gameManager = gameObject.AddComponent<StageManager>();
        _gameManager.m_TimeRemaining = 0.0f;

        if (_gameManager.m_TimeRemaining == 0.0f)
        _gameManager.isGameOver.Value = true;

        Assert.IsTrue(_gameManager.isGameOver.Value);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator GameTimerTest()
    {
        var gameObject = new GameObject();
        _gameManager = gameObject.AddComponent<StageManager>();

        _gameManager.m_TimeRemaining = 10.0f;
        if (timerStarted) 
        {
            for (float i = _gameManager.m_TimeRemaining; i > 0; i--)
            _gameManager.m_TimeRemaining --; 
        }

        yield return null;
        Assert.AreEqual( 0f, _gameManager.m_TimeRemaining);
    }
}
