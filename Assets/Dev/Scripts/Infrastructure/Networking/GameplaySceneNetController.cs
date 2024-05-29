using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace Dev.Infrastructure.Networking
{
    public class GameplaySceneNetController : MonoBehaviour
    {
        private NetworkEventsDispatcher _networkEventsDispatcher;

        [Inject]
        private void Construct(NetworkEventsDispatcher networkEventsDispatcher)
        {
            _networkEventsDispatcher = networkEventsDispatcher;
        }

        private void Start()
        {
            _networkEventsDispatcher.Shutdown += OnShutdown;
        }

        private void OnDestroy()
        {
            _networkEventsDispatcher.Shutdown -= OnShutdown;
        }

        private void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
            Debug.Log($"Shutdown");
            SceneManager.LoadScene(0);
        }   
    }
}