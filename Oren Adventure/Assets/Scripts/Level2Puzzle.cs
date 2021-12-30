using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level2Puzzle : MonoBehaviour
{
    public int activatedBtnAmt;
    public GameObject level2Door;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (activatedBtnAmt == 4)
        {
            level2Door.gameObject.SetActive(false);
        }
    }
}
