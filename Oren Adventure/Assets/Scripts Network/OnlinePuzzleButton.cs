using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace oren_Network
{
    public class OnlinePuzzleButton : NetworkBehaviour
    {
        public OnlinePuzzle onlinePuzzle;
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
            if (collision.gameObject.name == "StonePuzzle1")
            {
                onlinePuzzle.Puzzle1ButtonActivated = true;
                Debug.Log("Button pressed!");
            }
            //puzzle3 buttons
            if(collision.gameObject.name=="Button1-Puzzle3")
            {
                onlinePuzzle.puzzle3button1Activated = true;
            }
            if (collision.gameObject.name == "Button2-Puzzle3")
            {
                onlinePuzzle.puzzle3button2Activated = true;
            }
            //puzzle4 buttons
            if (collision.gameObject.name == "Button-Puzzle4")
            {
                onlinePuzzle.puzzle4ButtonActivated = true;
            }
        }
    }

}




