using System;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure.Networking
{
    public class ConnectionManager : MonoBehaviour
    {
        [SerializeField] private bool _autoConnect;
        [SerializeField] private NetworkRunner _networkRunner;
        [SerializeField] private float _offsetBetweenContainers = 50;

        private string _inputNickname = String.Empty;
        private bool _isConnectingToSession;
        
        private NetworkEventsDispatcher _networkEventsDispatcher;
        private LocalPlayerData _localPlayerData;

        [Inject]
        private void Construct(NetworkEventsDispatcher networkEventsDispatcher, LocalPlayerData localPlayerData)
        {
            _localPlayerData = localPlayerData;
            _networkEventsDispatcher = networkEventsDispatcher;
        }

        private void Start()
        {
            if (_autoConnect)
            {
                JoinHostClient();
            }
        }

        private void OnGUI()
        {
            float height = 50;
            float width = 200;
            
            float xPos = Screen.width / 2 - (width / 2);
            float yPos = Screen.height / 2;
            
            Rect rect = new Rect(xPos, yPos, width, height);
            
            if (_isConnectingToSession)
            {
                GUI.Label(rect, "Connecting...");
            }
            else
            {
                GUI.Label(rect, "Welcome to test task");
                yPos += _offsetBetweenContainers + height;
                rect = new Rect(xPos, yPos, width, height);
                _inputNickname = GUI.TextField(rect, _inputNickname);
                yPos += _offsetBetweenContainers + height;
                rect = new Rect(xPos, yPos, width, height);
                if (_inputNickname == String.Empty)
                {
                    GUI.Label(rect, "Enter nickname to be able to join/host session");
                }
                else
                {
                    if (GUI.Button(rect, "Host or join session"))
                    {
                        JoinHostClient();
                    }
                }
            }
        }

        private async void JoinHostClient()
        {
            if (_inputNickname == String.Empty)
            {
                _inputNickname = $"Player";
            }

            _localPlayerData.Nickname = _inputNickname;
            
            _networkRunner.AddCallbacks(_networkEventsDispatcher);
            
            _isConnectingToSession = true;
            
            StartGameArgs startGameArgs = new StartGameArgs();

            startGameArgs.GameMode = GameMode.AutoHostOrClient;
            startGameArgs.SessionName = "MyGame";
            startGameArgs.SceneManager = _networkRunner.gameObject.AddComponent<NetworkSceneManagerDefault>();
            startGameArgs.Scene = SceneRef.FromIndex(1);
            startGameArgs.PlayerCount = 4;

            StartGameResult startGameResult = await _networkRunner.StartGame(startGameArgs);

            if (startGameResult.Ok == false)
            {
                Debug.LogError($"Failed to start game, error msg: {startGameResult.ErrorMessage}, Reason: {startGameResult.ShutdownReason}");
                _isConnectingToSession = false;
            }
        }
        
    }
}