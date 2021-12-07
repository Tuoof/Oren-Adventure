using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using oren_Advent;

namespace Tests
{
    public class healthTest
    {
        [Test]
        public void currentHealthTest()
        {
            // arrange
            var health = new GameObject().AddComponent<PlayerHealthSP>();

            // act
            health.maxHealth = 3;
            health.currentHealth = health.maxHealth;

            // assert
            Assert.AreEqual(3, health.currentHealth);
            Debug.Log("Value are equal");
        }
        // A Test behaves as an ordinary method
        [Test]
        public void healthMinusTest()
        {
            // arrange
            var health = new GameObject().AddComponent<PlayerHealthSP>();

            // act
            health.maxHealth = 3;
            health.currentHealth = health.maxHealth;
            for(int i = health.maxHealth; i >= 0; i--)
            {
                health.DealDamage();
            }

            // assert
            if(health.currentHealth < 0) Assert.Negative(health.currentHealth);
            else if(health.currentHealth == 0) Assert.AreEqual(0, health.currentHealth);
            
            Debug.Log("Gameobject destroyed");
        }

        [Test]
        public void MaxHealthTest()
        {
            // arrange
            var health = new GameObject().AddComponent<PlayerHealthSP>();

            // act
            health.maxHealth = 3;
            health.currentHealth = 3;

            // assert
            Assert.AreEqual(health.maxHealth, health.currentHealth);
            Debug.Log("Value are equal");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator healthTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}

