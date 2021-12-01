using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace oren_Advent
{
    public class UIController : MonoBehaviour
    {
        PlayerHealth playerHealth;
        public static UIController instance;
        [SerializeField] Text healthText;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            healthText.text = $"=   {PlayerHealth.instance.currentHealth}";
            // healthText.text = $"=   {playerHealth.currentHealth}";
        }
    }
}