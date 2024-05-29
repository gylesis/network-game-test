using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;

namespace Dev.Infrastructure.Networking
{
    public class NetworkEventsDispatcher : INetworkRunnerCallbacks
    {
        public event Action<NetworkRunner, PlayerRef> PlayerJoined;
        public event Action<NetworkRunner, PlayerRef> PlayerLeft;
        public event Action<NetworkRunner, NetworkInput> Input;
        public event Action<NetworkRunner, ShutdownReason> Shutdown;
        public event Action<NetworkRunner> SceneLoadingStarted;
        public event Action<NetworkRunner> SceneLoadingDone;


        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) => PlayerJoined?.Invoke(runner, player);

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) => PlayerLeft?.Invoke(runner, player);

        public void OnInput(NetworkRunner runner, NetworkInput input) => Input?.Invoke(runner, input);

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Shutdown?.Invoke(runner, shutdownReason);
            
            PlayerJoined = null;
            PlayerLeft = null;
            Input = null;
            SceneLoadingDone = null;
            SceneLoadingStarted = null;
            Shutdown = null;
        }

        public void OnSceneLoadDone(NetworkRunner runner) => SceneLoadingDone?.Invoke(runner);

        public void OnSceneLoadStart(NetworkRunner runner) => SceneLoadingStarted?.Invoke(runner);

        public void OnConnectedToServer(NetworkRunner runner) { }

        public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }

        public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
        

        public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request,
                                     byte[] token) { }

        public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }

        public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }

        public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key,
                                           ArraySegment<byte> data) { }

        public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    }
}