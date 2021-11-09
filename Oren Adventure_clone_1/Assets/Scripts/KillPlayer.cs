using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillPlayer : MonoBehaviour
{
    public StageManager stageManager;
    // Start is called before the first frame update
    void Start()
    {
        stageManager = FindObjectOfType<StageManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.name=="Player")
        {
            stageManager.RespawnPlayer();
        }
    }
}
