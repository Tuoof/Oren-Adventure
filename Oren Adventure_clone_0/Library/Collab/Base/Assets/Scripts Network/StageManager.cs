using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Unity.Netcode;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace oren_Network
{
    public enum GameOverReason : byte
    {
        Win = 0,
        TimeUp = 1,
        Death = 2,
        Max,
    }

    public class StageManager : NetworkBehaviour
    {
        public static StageManager Singleton { get; set; }
        public GameObject currentCheckpoint;
        public GameObject ActionButton;
        public GameObject WinPanel;
        public GameObject LosePanel;
        public GameObject PauseMenu;
        public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> playerRespawned { get; } = new NetworkVariable<bool>(false);

        public bool Winstate = false;
        

        [Header("UI Settings")]
        public Text RemainingTimeText;
        public TMP_Text livesText;
        public TMP_Text gameTimerText;

        [Header("GameMode Settings")]
        [Tooltip("Delay Time before the game starts")]
        public float m_DelayedStartTime;
        [Tooltip("Time remaining until the game ends")]
        public float m_TimeRemaining;

        //These help to simplify checking server vs client
        //[NSS]: This would also be a great place to add a state machine and use networked vars for this
        private bool m_ClientGameOver;
        private bool m_ClientGameStarted;
        private bool m_ClientStartCountdown;
        private NetworkVariable<bool> m_CountdownStarted = new NetworkVariable<bool>(false);

        // the timer should only be synced at the beginning and then let the client to update it in a predictive manner
        private bool m_ReplicatedTimeSent = false;
        private float m_DelayedGameTimer;
        // private float m_GameTimer;

        // Start is called before the first frame update
        void Awake()
        {
            if (Singleton != null)
            {
                Destroy(Singleton);
                Singleton = this;
                Assert.IsNull(Singleton, $"Multiple instances of {nameof(StageManager)} detected. This should not happen.");
            }
            else
            {
                Singleton = this;
            }
            OnSingletonReady?.Invoke();

            


        }

        // Update is called once per frame
        void Update()
        {
            //Is the game over?
            if (IsCurrentGameOver()) return;

            //Update game timer (if the game hasn't started)
            UpdateGameTimer();
            TimeRemainingUpdate();

        }

        void FixedUpdate()
        {
            if (Winstate) { hasGameStarted.Value = false; }
        }

        void TimeRemainingUpdate()
        {
            if (m_DelayedStartTime <= 0) { m_TimeRemaining -= Time.deltaTime; }
            RemainingTimeText.text = m_TimeRemaining.ToString();

            if (IsServer && m_TimeRemaining <= 0.0f) // Only the server should be updating this
            {
                m_TimeRemaining = 0.0f;
                hasGameStarted.Value = false;
                this.isGameOver.Value = true;
            }
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                hasGameStarted.Value = true;

                m_DelayedStartTime = 0f;
                m_TimeRemaining = 60.0f;
                //Set our time remaining locally
                // m_DelayedGameTimer = m_DelayedStartTime;

                //Set for server side
                m_ReplicatedTimeSent = false;
            }
            else
            {
                //We do a check for the client side value upon instantiating the class (should be zero)
                Debug.LogFormat("Client side we started with a Delay timer value of {0}", m_DelayedGameTimer);
            }
            currentCheckpoint = GameObject.FindGameObjectWithTag("Checkpoint");

            if (IsClient && !IsServer)
            {
                m_ClientGameOver = false;
                m_ClientStartCountdown = false;
                m_ClientGameStarted = false;

                m_CountdownStarted.OnValueChanged += (oldValue, newValue) =>
                {
                    m_ClientStartCountdown = newValue;
                    Debug.LogFormat("Client side we were notified the start count down state was {0}", newValue);
                };

                hasGameStarted.OnValueChanged += (oldValue, newValue) =>
                {
                    m_ClientGameStarted = newValue;
                    Debug.LogFormat("Client side we were notified the game started state was {0}", newValue);
                };

                isGameOver.OnValueChanged += (oldValue, newValue) =>
                {
                    m_ClientGameOver = newValue;
                    Debug.LogFormat("Client side we were notified the game over state was {0}", newValue);
                };
            }

            //Both client and host/server will set the scene state to "ingame" which places the PlayerControl into the SceneTransitionHandler.SceneStates.INGAME
            //and in turn makes the players visible and allows for the players to be controlled.
            SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Level1);

            base.OnNetworkSpawn();
        }
        private bool ShouldStartCountDown()
        {
            //If the game has started, then don't both with the rest of the count down checks.
            if (HasGameStarted()) return false;
            if (IsServer)
            {
                m_CountdownStarted.Value = SceneTransitionHandler.sceneTransitionHandler.AllClientsAreLoaded();

                //While we are counting down, continually set the m_ReplicatedTimeRemaining.Value (client should only receive the update once)
                if (m_CountdownStarted.Value && !m_ReplicatedTimeSent)
                {
                    SetReplicatedDelayedGameTimerClientRPC(m_DelayedStartTime);
                    // SetReplicatedGameTimerClientRPC(m_TimeRemaining);
                    m_ReplicatedTimeSent = true;
                }

                return m_CountdownStarted.Value;
            }

            return m_ClientStartCountdown;
        }

        [ClientRpc]
        private void SetReplicatedDelayedGameTimerClientRPC(float delayedStartTime)
        {
            // See the ShouldStartCountDown method for when the server updates the value
            if (m_DelayedGameTimer == 0)
            {
                Debug.LogFormat("Client side our first timer update value is {0}", delayedStartTime);
                m_DelayedGameTimer = delayedStartTime;
            }
            else
            {
                Debug.LogFormat("Client side we got an update for a timer value of {0} when we shouldn't", delayedStartTime);
            }
        }
        private bool IsCurrentGameOver()
        {
            if (IsServer)
                return isGameOver.Value;
            return m_ClientGameOver;
        }
        private bool HasGameStarted()
        {
            if (IsServer)
                return hasGameStarted.Value;
            return m_ClientGameStarted;
        }
        private void UpdateGameTimer()
        {
            if (!ShouldStartCountDown()) return;
            if (!HasGameStarted() && m_DelayedStartTime > 0.0f)
            {
                m_DelayedStartTime -= Time.deltaTime;

                if (IsServer && m_DelayedStartTime <= 0.0f) // Only the server should be updating this
                {
                    m_DelayedStartTime = 0.0f;
                    hasGameStarted.Value = true;

                    OnGameStarted();
                }

                if (m_DelayedStartTime > 0.1f) { gameTimerText.SetText("{00}", Mathf.FloorToInt(m_DelayedStartTime)); }
            }
        }
        private void OnGameStarted()
        {
            gameTimerText.gameObject.SetActive(false);
            RemainingTimeText.gameObject.SetActive(true);
        }
        public void SetLives(int lives)
        {
            livesText.SetText("0{0}", lives);
        }
        public void DisplayLoseGameOver()
        {
            ActionButton.SetActive(false);
            LosePanel.SetActive(true);
        }
        public void DisplayWinGameOver()
        {
            ActionButton.SetActive(false);
            WinPanel.SetActive(true);
        }
        [ServerRpc]
        public void SetGameEndServerRpc(GameOverReason reason)
        {
            if (!IsServer) return;
            Assert.IsTrue(IsServer, "SetGameEnd should only be called server side!");

            // We should only end the game if all the player's are dead
            if (reason != GameOverReason.Death)
            {
                this.isGameOver.Value = true;
                BroadcastGameOverClientRpc(reason);
                return;
            }

            foreach (NetworkClient networkedClient in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = networkedClient.PlayerObject;
                if (playerObject == null) continue;

                // We should just early out if any of the player's are still alive
                if (playerObject.GetComponent<ClientPlayerController>().IsAlive)
                    return;
            }
            this.isGameOver.Value = true;
        }
        public void MovePlayerToCheckpoint()
        {
            foreach (NetworkClient networkedClient in NetworkManager.Singleton.ConnectedClientsList)
            {
                var playerObject = networkedClient.PlayerObject;
                if (playerObject == null) continue;

                // We should just early out if any of the player's are still alive
                if (playerObject.GetComponent<ClientPlayerController>().transform.position == currentCheckpoint.transform.position)
                    BroadcastPlayerToCheckpointClientRpc();
                return;
            }
        }

        [ClientRpc]
        public void BroadcastPlayerToCheckpointClientRpc()
        {
            var localPlayerObject = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
            Assert.IsNotNull(localPlayerObject);

            if (localPlayerObject.TryGetComponent<ClientPlayerController>(out var PlayerController))
            {
                // PlayerController.RespawnPlayerClientRpc();
            }

        }

        [ClientRpc]
        public void BroadcastGameOverClientRpc(GameOverReason reason)
        {
            var localPlayerObject = NetworkManager.Singleton.ConnectedClients[NetworkManager.Singleton.LocalClientId].PlayerObject;
            Assert.IsNotNull(localPlayerObject);

            if (localPlayerObject.TryGetComponent<ClientPlayerController>(out var PlayerController))
            {
                PlayerController.NotifyGameOver(reason);
            }
        }
        public void ExitGame()
        {
            NetworkManager.Singleton.Shutdown();
            SceneTransitionHandler.sceneTransitionHandler.ExitAndLoadStartMenu();
        }

        internal static event Action OnSingletonReady;
    }
}
