using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace oren_Advent
{
    public class PlayerHealthSP : MonoBehaviour
{
    public static PlayerHealthSP singleton { get; private set; }
    public int currentHealth, maxHealth;
    // Start is called before the first frame update
    private void Awake()
    {
        singleton = this;
    }
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        StageManagerSP.Singleton.SetLives(currentHealth);
    }

    public void DealDamage()
    {
        currentHealth--;

        if(currentHealth <= 0)
        {
            // gameObject.SetActive(false);
        }
    }
}
}
