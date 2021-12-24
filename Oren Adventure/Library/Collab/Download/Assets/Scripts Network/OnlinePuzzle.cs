using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

namespace oren_Network
{
    public class OnlinePuzzle : NetworkBehaviour
    {
        [Header("Puzzle 1")]
        public bool Puzzle1ButtonActivated;
        [SerializeField] private GameObject puzzle1Spike;

        [Header("Puzzle 3")]
        public bool puzzle3button1Activated;
        public bool puzzle3button2Activated;
        [SerializeField] private GameObject puzzle3Door;

        [Header("Puzzle 4")]
        public bool puzzle4ButtonActivated;
        [SerializeField] private GameObject puzzle4Door;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            //puzzle no.1 activate button to disable spike
            if (Puzzle1ButtonActivated == true)
            {
                puzzle1Spike.SetActive(false);
            }
            else
                puzzle1Spike.SetActive(true);
            //puzzle no.3 activate 2 buttons to open gate
            if (puzzle3button1Activated && puzzle3button2Activated)
            {
                puzzle3Door.SetActive(false);
            }
            //puzle no. 4 activate 2 button to open the next gate.
            if(puzzle4ButtonActivated)
            {
                puzzle4Door.SetActive(false);
            }
        }
    }
}

