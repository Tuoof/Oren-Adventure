using Unity.Netcode;
using TMPro;
using Unity.Netcode.Transports.UNET;
using UnityEngine;
using UnityEngine.UI;

namespace oren_Network
{
    public class MenuControl : NetworkBehaviour
    {
        [SerializeField]
        private Text m_HostIpInput;

        [SerializeField]
        private string m_SceneName;
        public GameObject settingsPanel;

        // [SerializeField]
        // private TextMeshProUGUI playersInGameText;

        // [SerializeField]
        // private TMP_InputField joinCodeInput;

        [SerializeField]
        private Button executePhysicsButton;

        private bool hasServerStarted;

        public GameObject playMode;
        public GameObject coopMode;
        public GameObject backButton;

        public void GotoLobby()
        {
            SceneTransitionHandler.sceneTransitionHandler.SwitchScene(m_SceneName);
        }
        async public void StartHostGame()
        {
            if (RelayManager.Instance.IsRelayEnabled)
            {
                await RelayManager.Instance.SetupRelay();
                Logger.Instance.LogInfo("start host...");
                LobbyControl.isHosting = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)               
            }
        }
        public void StartLocalHostGame()
        {
            Logger.Instance.LogInfo("Start Local host...");

            var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (utpTransport) { m_HostIpInput.text = "127.0.0.1"; }

            LobbyControl.isHosting = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)

            // Update the current HostNameInput with whatever we have set in the NetworkConfig as default
            // var unetTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
        }

        async public void StartClientGame()
        {
            if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(m_HostIpInput.text))
            {
                await RelayManager.Instance.JoinRelay(m_HostIpInput.text);
                Logger.Instance.LogInfo("Client started...");
                LobbyControl.isHosting = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)
            }
        }
        public void StartLocalClientGame()
        {
            Logger.Instance.LogInfo("start local client...");
            ushort portAddress = 9998;

            if (m_HostIpInput.text != "Hostname")
            {
                // var unetTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
                var utpTransport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

                if (utpTransport)
                {
                    utpTransport.SetConnectionData(m_HostIpInput.text, portAddress );
                    // utpTransport.ConnectAddress = m_HostIpInput.text;
                    // utpTransport.ConnectPort = 9998;
                }
                LobbyControl.isHosting = false; //This is a work around to handle proper instantiation of a scene for the first time.  (See LobbyControl.cs)
            }
        }

        public void PlayButtonLocalGame()
        {
            m_SceneName = "LevelTutorial";
            SceneTransitionHandler.sceneTransitionHandler.SwitchScene(m_SceneName);
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

