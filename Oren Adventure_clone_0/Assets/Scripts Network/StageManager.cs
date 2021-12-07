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
        public GameObject currentCheckpoint;
        private ClientPlayerController player;
        // Start is called before the first frame update
        void Start()
        {
            player = FindObjectOfType<ClientPlayerController>();
        }

        // Update is called once per frame
        void Update()
        {

        }
        public void RespawnPlayer()
        {
            Debug.Log("player respawn");
            player.transform.position = currentCheckpoint.transform.position;
        }
    }
}
