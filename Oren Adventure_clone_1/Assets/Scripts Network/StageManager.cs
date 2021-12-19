using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine.Assertions;

namespace oren_Network
{
    public class StageManager : NetworkBehaviour
    {
        public static StageManager Singleton { get; private set; }
        public GameObject currentCheckpoint;
        public GameObject player;
        // Start is called before the first frame update
        void Start()
        {
            // player = GameObject.FindGameObjectWithTag("Player");
        }

        // Update is called once per frame
        void Update()
        {

        }

        void FixedUpdate()
        {
            if (!NetworkManager.Singleton.IsConnectedClient) { return; }
            player = GameObject.FindGameObjectWithTag("Player");
        }
        // public void RespawnPlayer()
        // {
        //     Debug.Log("player respawn");
        //     player.transform.position = currentCheckpoint.transform.position;
        // }
    }
}
