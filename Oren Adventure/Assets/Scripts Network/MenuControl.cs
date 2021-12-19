using Unity.Netcode;
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
        public GameObject playMode;
        public GameObject coopMode;
        public GameObject backButton;
        public void StartHostGame()
        {
            // Update the current HostNameInput with whatever we have set in the NetworkConfig as default
            var unetTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            if (unetTransport) m_HostIpInput.text = "127.0.0.1";
            LobbyControl.isHosting = true; //This is a work around to handle proper instantiation of a scene for the first time.(See LobbyControl.cs)
            SceneTransitionHandler.sceneTransitionHandler.SwitchScene(m_SceneName);
        }

        public void JoinLocalGame()
        {
            if (m_HostIpInput.text != "Hostname")
            {
                var utpTransport = (UNetTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
                if (utpTransport)
                {
                    //utpTransport.SetConnectionData(m_HostIpInput.text, 7777);
                    utpTransport.ConnectAddress = m_HostIpInput.text;
                    utpTransport.ConnectPort = 7777;
                }
                LobbyControl.isHosting = false; //This is a work around to handle proper instantiation of a scene for the first time.  (See LobbyControl.cs)
                SceneTransitionHandler.sceneTransitionHandler.SwitchScene(m_SceneName);
            }
        }

        // Play Button Function
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

