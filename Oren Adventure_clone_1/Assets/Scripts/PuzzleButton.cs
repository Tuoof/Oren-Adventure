using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleButton : MonoBehaviour
{
    public Level1Puzzle lv1Puzzle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Stone1")
        {
            lv1Puzzle.activatedBtnAmt += 1;
            Debug.Log("Button pressed!");
        }
        if (collision.gameObject.name == "Stone2")
        {
            lv1Puzzle.activatedBtnAmt += 1;
            Debug.Log("Button pressed!");
        }
        if (collision.gameObject.name == "Stone3")
        {
            lv1Puzzle.activatedBtnAmt += 1;
            Debug.Log("Button pressed!");
        }
        if (collision.gameObject.name == "Stone4")
        {
            lv1Puzzle.activatedBtnAmt += 1;
            Debug.Log("Button pressed!");
        }
    }

}
