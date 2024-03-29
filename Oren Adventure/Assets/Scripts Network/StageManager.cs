using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
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
    [DisallowMultipleComponent]
    public class StageManager : NetworkBehaviour
    {
        public GameObject Playerprefab;
        
        public static StageManager Singleton { get; set; }
        public GameObject currentCheckpoint;

        [Header("Panel Settings")]
        public GameObject actionButton;
        public GameObject winPanel;
        public GameObject losePanel;
        public GameObject pausePanel;
        public GameObject timePanel;
        public GameObject pauseButton;
        public GameObject disconnectedPanel;
        public List<GameObject> stones;
        private GameObject NM;

        public NetworkVariable<bool> hasGameStarted { get; } = new NetworkVariable<bool>(false);
        public NetworkVariable<bool> isGameOver { get; } = new NetworkVariable<bool>(false);

        public Dictionary<ulong, bool> m_ClientsInStage;

        public bool Winstate = false;
        private bool m_AllPlayersInStage;


        [Header("UI Settings")]
        public Text RemainingTimeText;
        public TMP_Text livesText;
        public TMP_Text gameTimerText;

        [Header("GameMode Settings")]
        [Tooltip("Delay Time before the game starts")]
        public float m_DelayedStartTime;
        [Tooltip("Time remaining until the game ends")]
        [SerializeField]
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
            NM = GameObject.FindGameObjectWithTag("NetworkManager");
            m_ClientsInStage = NM.GetComponent<PlayerDictionary>().m_Clients;

            m_TimeRemaining = 120.0f;

            if (IsServer)
            {
                m_AllPlayersInStage = true;

                //Server will be notified when a client disconnects
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

                Debug.LogFormat("All player in Stage was {0}", m_ClientsInStage.Count);
                Debug.LogFormat("All PlayerDictionary in stage was {0}", NM.GetComponent<PlayerDictionary>().m_Clients.Count);
            }
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
            Debug.LogFormat("All player in Stage was {0}", m_ClientsInStage.Count);
            Debug.LogFormat("All PlayerDictionary in stage was {0}", NM.GetComponent<PlayerDictionary>().m_Clients.Count);
        }

        void SetStonesActive()
        {
            for (int i = 0; i < stones.Count; i++)
            {
                stones[i].SetActive(true);
            }
        }

        private void UpdateAndCheckPlayersInStage()
        {
            Debug.LogFormat("we were notified the player count in Stage was {0}", m_ClientsInStage.Count);
            Debug.LogFormat("we were notified the All player in Stage was {0}", m_AllPlayersInStage);
            if (m_ClientsInStage.Count <= 0)
            {
                m_AllPlayersInStage = false;
            }
            CheckForAllPlayersLeave();
        }

        private void OnClientDisconnectCallback(ulong clientId)
        {
            if (IsServer)
            {
                if (m_ClientsInStage.ContainsKey(clientId))
                {
                    m_ClientsInStage.Remove(clientId);
                }
                UpdateAndCheckPlayersInStage();
                SendDisconnectedGameClientRpc();
            }
            else if(IsClient)
            {
                foreach (var client in m_ClientsInStage.ToList())
                {
                    m_ClientsInStage.Remove(client.Key);
                }
            }
        }

        public void TimeRemainingUpdate()
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

            if (IsServer)
            {
                hasGameStarted.Value = true;

                m_DelayedStartTime = 0f;

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
            SetStonesActive();

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
            SceneTransitionHandler.singleton.SetSceneState(SceneTransitionHandler.SceneStates.Level1);
            base.OnNetworkSpawn();
        }
        private bool ShouldStartCountDown()
        {
            //If the game has started, then don't both with the rest of the count down checks.
            if (HasGameStarted()) return false;
            if (IsServer)
            {
                m_CountdownStarted.Value = SceneTransitionHandler.singleton.AllClientsAreLoaded();

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
        private void CheckForAllPlayersLeave()
        {
            Debug.LogFormat("we were notified the All player in Stage was {0}", m_AllPlayersInStage);
            if (!m_AllPlayersInStage)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;

                NetworkManager.Singleton.Shutdown();
                SceneTransitionHandler.singleton.ExitAndLoadStartMenu();
            }
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
            actionButton.SetActive(false);
            timePanel.SetActive(false);
            pauseButton.SetActive(false);
            losePanel.SetActive(true);
        }
        public void DisplayWinGameOver()
        {
            actionButton.SetActive(false);
            timePanel.SetActive(false);
            pauseButton.SetActive(false);
            winPanel.SetActive(true);
        }
        [ServerRpc]
        public void SetGameEndServerRpc(GameOverReason reason)
        {
            if (!IsOwner) return;
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
        public void PauseGame()
        {
            actionButton.SetActive(false);
            timePanel.SetActive(false);
            pauseButton.SetActive(false);
            pausePanel.SetActive(true);
        }

        [ClientRpc]
        public void SendDisconnectedGameClientRpc()
        {
            if (IsServer) return;

            actionButton.SetActive(false);
            timePanel.SetActive(false);
            pauseButton.SetActive(false);
            disconnectedPanel.SetActive(true);
        }
        public void ExitGame()
        {
            foreach (var client in m_ClientsInStage.ToList())
            {
                m_ClientsInStage.Remove(client.Key);
            }
            NetworkManager.Singleton.Shutdown();
            SceneTransitionHandler.singleton.ExitAndLoadStartMenu();
        }
        public void ExitPanel()
        {
            actionButton.SetActive(true);
            timePanel.SetActive(true);
            pauseButton.SetActive(true);
            pausePanel.SetActive(false);
        }

        internal static event Action OnSingletonReady;
    }
}
