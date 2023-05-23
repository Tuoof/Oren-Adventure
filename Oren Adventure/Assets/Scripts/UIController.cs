using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace oren_Advent
{
    public class UIController : MonoBehaviour
    {
        PlayerHealthSP playerHealthSP;
        public static UIController singleton;
        [SerializeField] Text healthText;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            healthText.text = $"{PlayerHealthSP.singleton.currentHealth}";
            // healthText.text = $"=   {playerHealth.currentHealth}";
        }
        public void exit2Main()
        {
            SceneManager.LoadScene(1);
        }
        public void retryLevel1SP()
        {
            SceneManager.LoadScene(5);
        }
        public void toLevelSelector()
        {
            SceneManager.LoadScene(4);
        }
    }
}