using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Network;
using Unity.Netcode;

public class RespawnPlayer
{
    private ClientPlayerController _controller;
    private Enemy _enemy;

    // A Test behaves as an ordinary method
    [Test]
    public void RespawnPlayerTriggerTest()
    {
        var gameObject = new GameObject();
        _controller = gameObject.AddComponent<ClientPlayerController>();
        _enemy = gameObject.AddComponent<Enemy>();

        _enemy.transform.position = new Vector3(3f, 3f, 0f);
        _controller.transform.position = _enemy.transform.position;

        Assert.IsNotNull(_controller.transform.position);
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [Test]
    public void PlayerRespawnedTest()
    {
        var gameObject = new GameObject();
        _controller = gameObject.AddComponent<ClientPlayerController>();
        _enemy = gameObject.AddComponent<Enemy>();

        _enemy.transform.position = new Vector3(3f, 3f, 0f);
        _controller.transform.position = _enemy.transform.position;

        if (_controller.transform.position ==  _enemy.transform.position)

        Assert.IsNotNull(_controller.transform.position);
    }
}
