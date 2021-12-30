using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace oren_Advent
{
    public enum GameOverReason : byte
    {
        Win = 0,
        TimeUp = 1,
        Death = 2,
        Max,
    }

    public class StageManagerSP : MonoBehaviour
    {
        public static StageManagerSP Singleton;
        public Transform currentCheckpoint;
        public bool hasGameStarted = false;
        public bool isGameOver = false;

        [Header("UI Settings")]
        public TMP_Text gameTimerText;
        public TMP_Text scoreText;
        public Text livesText;
        public TMP_Text gameOverText;

        [Header("UI Menu")]
        public GameObject winMenu;
        public GameObject loseMenu;
        public GameObject pauseMenu;

        //[Header("GameMode Settings")]

        // Start is called before the first frame update
        void Awake()
        {
            Assert.IsNull(Singleton, $"Multiple instances of {nameof(StageManagerSP)} detected. This should not happen.");
            Singleton = this;

            OnSingletonReady?.Invoke();

            hasGameStarted = false;

        }

        // Update is called once per frame
        void Update()
        {
            if(livesText.text=="0")
            {
                loseMenu.gameObject.SetActive(true);
            }
        }

        void FixedUpdate()
        {
            // MovePlayerToCheckpoint();
        }
        private void Start()
        {
            hasGameStarted = true;
        }
        
        private bool IsCurrentGameOver()
        {
            return isGameOver;
        }
        private bool HasGameStarted()
        {
            return hasGameStarted;
        }
        
        private void OnGameStarted()
        {
            // gameTimerText.gameObject.SetActive(false);
        }
        public void SetLives(int lives)
        {
            livesText.text = lives.ToString();
        }
        public void DisplayGameOverText(string message)
        {
            if (gameOverText)
            {
                gameOverText.SetText(message);
                gameOverText.gameObject.SetActive(true);
            }
        }
        public void SetGameEnd(GameOverReason reason)
        {
            // We should only end the game if all the player's are dead
            if (reason != GameOverReason.Death)
            {
                this.isGameOver = true;
                return;
            }

            this.isGameOver = true;
        }

        public void ExitGame()
        {
            SceneManager.LoadScene("StartMenu");
        }

        internal static event Action OnSingletonReady;
    }
}
