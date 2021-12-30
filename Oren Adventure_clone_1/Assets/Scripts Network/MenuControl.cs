using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

namespace oren_Network
{
    public class MenuControl : NetworkBehaviour
    {
        public static MenuControl Singleton { get; private set; }
        [SerializeField]
        private Text m_HostIpInput;
        public bool isServerBuild;
        ushort portAddress;

        [SerializeField]
        private string m_SceneName;
        public GameObject settingsPanel;

        // [SerializeField]
        // private TMP_InputField joinCodeInput;

        private bool hasServerStarted;

        public GameObject playMode;
        public GameObject coopMode;
        public GameObject backButton;

        private void Awake()
        {
            if (isServerBuild)
            {
                StartServerGame();
            }
        }
        public void GotoLobby()
        {
            SceneTransitionHandler.singleton.SwitchScene(m_SceneName);
        }
        public void StartServerGame()
        {
            Logger.Instance.LogInfo("Start Local host...");

            // Update the current HostNameInput with whatever we have set in the NetworkConfig as default
            // var unetTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            // if (utpTransport) { m_HostIpInput.text = "127.0.0.1"; }

            LobbyControl.isServer = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)
            GotoLobby();

        }
        async public void StartHostGame()
        {
            if (RelayManager.Instance.IsRelayEnabled)
            {
                await RelayManager.Instance.SetupRelay();
                Logger.Instance.LogInfo("start host...");
                LobbyControl.isHosting = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)
                GotoLobby();            
            }
        }
        public void StartLocalHostGame()
        {
            Logger.Instance.LogInfo("Start Local host...");

            // Update the current HostNameInput with whatever we have set in the NetworkConfig as default
            // var unetTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport) { m_HostIpInput.text = "127.0.0.1"; }

            LobbyControl.isHosting = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)
            GotoLobby();

            
        }

        async public void StartClientGame()
        {
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(m_HostIpInput.text))
            {
                await RelayManager.Instance.JoinRelay(m_HostIpInput.text);
                Logger.Instance.LogInfo("Client started...");
                LobbyControl.isHosting = false; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)
                GotoLobby();
            }
        }
        public void StartLocalClientGame()
        {
            Logger.Instance.LogInfo("start local client...");
            portAddress = 7777;

            if (m_HostIpInput.text != "Hostname")
            {
                // var unetTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
                var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

                if (utpTransport)
                {
                    // utpTransport.SetConnectionData(m_HostIpInput.text, portAddress );
                    // unetTransport.ConnectAddress = m_HostIpInput.text;
                    // unetTransport.ConnectPort = portAddress;
                }
                LobbyControl.isHosting = false; //This is a work around to handle proper instantiation of a scene for the first time.  (See LobbyControl.cs)
                LobbyControl.isServer = false;
                LobbyControl.isClient = true;
                GotoLobby();
            }
        }

        public void PlayButtonLocalGame()
        {
            m_SceneName = "Level1SP";
            SceneTransitionHandler.singleton.SwitchScene(m_SceneName);
        }

        // Setting Button Function
        public void SettingsMenuOpen()
        {
            settingsPanel.SetActive(true);
            playMode.SetActive(false);
            coopMode.SetActive(false);
        }
        public void SettingsMenuClose()
        {
            settingsPanel.SetActive(false);
            playMode.SetActive(true);
            coopMode.SetActive(false);
        }

        // Coop Button Function
        public void CoopModeButton()
        {
            settingsPanel.SetActive(false);
            playMode.SetActive(false);
            coopMode.SetActive(true);
            backButton.SetActive(true);
        }

        public void BackButton()
        {
            playMode.SetActive(true);
            coopMode.SetActive(false);
            backButton.SetActive(false);
        }
    }
}

