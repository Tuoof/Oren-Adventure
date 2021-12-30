using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level1Puzzle : MonoBehaviour
{
    public int activatedBtnAmt;
    public GameObject level1Door;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(activatedBtnAmt==2)
        {
            level1Door.gameObject.SetActive(false);
        }
    }
}
