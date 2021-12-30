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
        private void OnTriggerEnter2D(Collider2D collider)
        {
            if (collider.tag == "Stone")
            {
                Debug.LogFormat("button pressed with collider object tag {0}", collider.tag);
                
                if (this.gameObject.name == "Button-Puzzle1") 
                {
                    Debug.LogFormat("button pressed with this object name {0}", this.gameObject.name);
                    onlinePuzzle.Puzzle1ButtonActivated = true;
                }
            }
            //puzzle3 buttons
            if(collider.tag == "Stone")
            {
                Debug.LogFormat("button pressed with collision object name {0}", collider.tag);

                if (this.gameObject.name == "Button1-Puzzle3")
                {
                    Debug.LogFormat("button pressed with this object name {0}", this.gameObject.name);
                    onlinePuzzle.puzzle3button1Activated = true;
                }
            }
            if (collider.tag == "Stone")
            {
                Debug.LogFormat("button pressed with collision object name {0}", collider.gameObject.name);
                
                if (this.gameObject.name == "Button2-Puzzle3")
                {
                    Debug.LogFormat("button pressed with this object name {0}", this.gameObject.name);
                    onlinePuzzle.puzzle3button2Activated = true;
                }
            }
            //puzzle4 buttons
            if (collider.gameObject.name == "Stone-Puzzle4")
            {
                onlinePuzzle.puzzle4ButtonActivated = true;
            }
        }
    }

}




