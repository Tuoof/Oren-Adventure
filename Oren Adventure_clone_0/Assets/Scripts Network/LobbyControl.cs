using System;
using TMPro;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace oren_Network
{
    public class LobbyControl : NetworkBehaviour
    {
        public static LobbyControl Singleton { get; set; }
        [HideInInspector]
        public static bool isHosting;
        public static bool isServer;
        public static bool isClient;

        [SerializeField]
        private string m_SceneName = "Level1";

        // Minimum player count required to transition to next level
        [SerializeField]
        private int m_MinimumPlayerCount = 1;
        [SerializeField]
        private int m_MaximumPlayerCount = 4;

        public Text LobbyText;
        public Text PlayerName;
        public Text codeText;
        private TMP_InputField Code;
        private bool m_AllPlayersInLobby;

        private Dictionary<ulong, bool> m_ClientsInLobby;
        private Dictionary<ulong, string> m_ClientsNameInLobby;
        private string m_UserLobbyStatusText, m_UserLobbyNameText;
        public GameObject LobbyCanvas;
        public GameObject inGameCanvas;
        public GameObject stageManager;

        public Button ExitButton;

        public void StartSession()
        {
            if (isHosting)
            {
                NetworkManager.Singleton.StartHost(); //Spin up the host
            }
            else if(isServer)
            {
                NetworkManager.Singleton.StartServer();
            }
            else if (isClient)
            {
                NetworkManager.Singleton.StartClient(); //Spin up the client
            }
        }

        /// <summary>
        ///     Awake
        ///     This is one way to kick off a multiplayer session
        /// </summary>
        private void Awake()
        {
            m_ClientsInLobby = new Dictionary<ulong, bool>();
            m_ClientsNameInLobby = new Dictionary<ulong, string>();

            //We added this information to tell us if we are going to host a game or join an the game session
            StartSession();

            if (NetworkManager.Singleton.IsListening)
            {
                //Always add ourselves to the list at first
                if (IsHost || IsClient) m_ClientsInLobby.Add(NetworkManager.Singleton.LocalClientId, false);

                //If we are hosting, then handle the server side for detecting when clients have connected
                //and when their lobby scenes are finished loading.
                if (IsServer)
                {
                    m_AllPlayersInLobby = false;

                    //Server will be notified when a client connects
                    NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
                    SceneTransitionHandler.singleton.OnClientLoadedScene += ClientLoadedScene;
                }

                //Update our lobby
                GenerateUserStatsForLobby();
            }

            SceneTransitionHandler.singleton.SetSceneState(SceneTransitionHandler.SceneStates.Lobby);
            // SceneTransitionHandler.sceneTransitionHandler.SetSceneState(SceneTransitionHandler.SceneStates.Level1);
        }

        private void Start()
        {
            // Button btn = ExitButton.GetComponent<Button>();
            // btn.onClick.AddListener(delegate{ExitGame(NetworkManager.Singleton.LocalClientId);});
        }

        private void OnGUI()
        {
            if (LobbyText != null) LobbyText.text = m_UserLobbyStatusText;
        }

        /// <summary>
        ///     GenerateUserStatsForLobby
        ///     Psuedo code for setting player state
        ///     Just updating a text field, this could use a lot of "refactoring"  :)
        /// </summary>
        public void GenerateUserStatsForLobby()
        {
            m_UserLobbyStatusText = string.Empty;
            m_UserLobbyNameText = string.Empty;
            foreach (var clientLobbyStatus in m_ClientsInLobby)
            {
                m_UserLobbyStatusText += "        " + "  ( " + PlayerName.text + clientLobbyStatus.Key + " )         ";
                if (clientLobbyStatus.Value)
                    m_UserLobbyStatusText += "(Ready)\n";
                else
                    m_UserLobbyStatusText += "(Not Ready)\n";
            }
        }

        /// <summary>
        ///     UpdateAndCheckPlayersInLobby
        ///     Checks to see if we have at least 2 or more people to start
        /// </summary>
        private void UpdateAndCheckPlayersInLobby()
        {
            m_AllPlayersInLobby = m_ClientsInLobby.Count >= m_MinimumPlayerCount && m_ClientsInLobby.Count <= m_MaximumPlayerCount;
            Debug.LogFormat("we were notified the All player in lobby was {0}", m_ClientsInLobby.Count);

            foreach (var clientLobbyStatus in m_ClientsInLobby)
            {
                SendClientReadyStatusUpdatesClientRpc(clientLobbyStatus.Key, clientLobbyStatus.Value);
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientLobbyStatus.Key))

                    //If some clients are still loading into the lobby scene then this is false
                    m_AllPlayersInLobby = false;
            }

            CheckForAllPlayersReady();
        }

        /// <summary>
        ///     ClientLoadedScene
        ///     Invoked when a client has loaded this scene
        /// </summary>
        /// <param name="clientId"></param>
        private void ClientLoadedScene(ulong clientId)
        {
            if (IsServer)
            {
                if (!m_ClientsInLobby.ContainsKey(clientId))
                {
                    m_ClientsInLobby.Add(clientId, false);
                    GenerateUserStatsForLobby();
                }

                UpdateAndCheckPlayersInLobby();
            }
        }

        /// <summary>
        ///     OnClientConnectedCallback
        ///     Since we are entering a lobby and MLAPI NetowrkingManager is spawning the player,
        ///     the server can be configured to only listen for connected clients at this stage.
        /// </summary>
        /// <param name="clientId">client that connected</param>
        private void OnClientConnectedCallback(ulong clientId)
        {
            if (IsServer)
            {
                if (!m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Add(clientId, false);
                GenerateUserStatsForLobby();
                UpdateAndCheckPlayersInLobby();
            }
        }

        /// <summary>
        ///     SendClientReadyStatusUpdatesClientRpc
        ///     Sent from the server to the client when a player's status is updated.
        ///     This also populates the connected clients' (excluding host) player state in the lobby
        /// </summary>
        /// <param name="clientId"></param>
        /// <param name="isReady"></param>
        [ClientRpc]
        private void SendClientReadyStatusUpdatesClientRpc(ulong clientId, bool isReady)
        {
            if (!IsServer)
            {
                if (!m_ClientsInLobby.ContainsKey(clientId))
                    m_ClientsInLobby.Add(clientId, isReady);
                else
                    m_ClientsInLobby[clientId] = isReady;
                GenerateUserStatsForLobby();
            }
        }

        /// <summary>
        ///     CheckForAllPlayersReady
        ///     Checks to see if all players are ready, and if so launches the game
        /// </summary>
        private void CheckForAllPlayersReady()
        {
            Debug.LogFormat("we were notified the All player in lobby was {0}", m_AllPlayersInLobby);
            if (m_AllPlayersInLobby)
            {
                var allPlayersAreReady = true;
                foreach (var clientLobbyStatus in m_ClientsInLobby)
                    if (!clientLobbyStatus.Value)

                        //If some clients are still loading into the lobby scene then this is false
                        allPlayersAreReady = false;

                //Only if all players are ready
                if (allPlayersAreReady)
                {
                    //Remove our client connected callback
                    NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;

                    //Remove our scene loaded callback
                    SceneTransitionHandler.singleton.OnClientLoadedScene -= ClientLoadedScene;

                    //Transition to the ingame scene
                    /*LobbyCanvas.SetActive(false);
                    inGameCanvas.SetActive(true);
                    stageManager.SetActive(true);
                    this.gameObject.SetActive(false);*/

                    SceneTransitionHandler.singleton.SwitchScene(m_SceneName);
                    Debug.LogFormat("we were notified the All player Ready Status was {0}", allPlayersAreReady);
                    SwitchSceneServerRpc();
                }
            }
        }

        [ServerRpc]
        private void SwitchSceneServerRpc()
        {
            SceneTransitionHandler.singleton.SwitchScene(m_SceneName);
            Debug.LogFormat("Server was notified the All player Ready Status was {0}", SceneTransitionHandler.singleton.m_SceneState);
        }
        [ClientRpc]
        private void SwitchSceneClientRpc()
        {
            SceneTransitionHandler.singleton.SwitchScene(m_SceneName);
            Debug.LogFormat("Client were notified the All player Ready Status was {0}", SceneTransitionHandler.singleton.m_SceneState);
        }

        /// <summary>
        ///     PlayerIsReady
        ///     Tied to the Ready button in the InvadersLobby scene
        /// </summary>
        public void PlayerIsReady()
        {
            if (IsServer)
            {
                m_ClientsInLobby[NetworkManager.Singleton.ServerClientId] = true;
                UpdateAndCheckPlayersInLobby();
            }
            else
            {
                m_ClientsInLobby[NetworkManager.Singleton.LocalClientId] = true;
                OnClientIsReadyServerRpc(NetworkManager.Singleton.LocalClientId);
            }

            GenerateUserStatsForLobby();
        }

        /// <summary>
        ///     OnClientIsReadyServerRpc
        ///     Sent to the server when the player clicks the ready button
        /// </summary>
        /// <param name="clientid">clientId that is ready</param>
        [ServerRpc(RequireOwnership = false)]
        private void OnClientIsReadyServerRpc(ulong clientid)
        {
            if (m_ClientsInLobby.ContainsKey(clientid))
            {
                m_ClientsInLobby[clientid] = true;
                UpdateAndCheckPlayersInLobby();
                GenerateUserStatsForLobby();
            }
        }

        public void ExitGame(ulong clientId)
        {
            if (IsServer)
            {
                foreach (var clientLobbyStatus in m_ClientsInLobby)
                {
                    m_UserLobbyStatusText = string.Empty;
                }

                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
                SceneTransitionHandler.singleton.OnClientLoadedScene -= ClientLoadedScene;

                if (m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Remove(clientId);
            }

            else if (!IsServer)
            {
                if (m_ClientsInLobby.ContainsKey(clientId)) m_ClientsInLobby.Remove(clientId);
            }

            NetworkManager.Singleton.Shutdown();
            SceneTransitionHandler.singleton.ExitAndLoadStartMenu();
        }

        public void SetCode(string code)
        {
            codeText.text = code;
        }
    }
}
