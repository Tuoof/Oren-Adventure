using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public GameObject currentCheckpoint;
    private PlayerControllerSP player;
    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerControllerSP>();
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