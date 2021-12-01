using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth instance;
    public int currentHealth, maxHealth;
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DealDamage()
    {
        currentHealth--;

        if(currentHealth <= 0)
        {
            gameObject.SetActive(false);
        }
    }
}
}
