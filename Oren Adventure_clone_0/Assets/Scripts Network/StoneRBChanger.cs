using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class StoneRBChanger : NetworkBehaviour
{
    void Awake()
    {

        this.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        
        Debug.LogFormat("This stone has set to dynamic rigidbody");
    }
    // Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        this.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
