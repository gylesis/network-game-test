using System;
using Dev.PlayerLogic;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class GameplayInstaller : MonoInstaller
    {
        [SerializeField] private SceneCameraController _sceneCameraController;
        [SerializeField] private PlayerCameraController _playerCameraController;

        [SerializeField] private PlayersStatsDrawer _playersStatsDrawer;
        [SerializeField] private PlayersDataService _playersDataService;
        [SerializeField] private PlayerSpawner _playerSpawner;
        
        public override void InstallBindings()
        {
            Debug.Log($"Install bindings");
            Container.Bind<PlayerSpawner>().FromInstance(_playerSpawner).AsSingle();            
            Container.Bind<PlayersStatsDrawer>().FromInstance(_playersStatsDrawer).AsSingle();
            Container.Bind<PlayersDataService>().FromInstance(_playersDataService).AsSingle();
            
            Container.Bind<SceneCameraController>().FromInstance(_sceneCameraController).AsSingle();

            Container.Rebind<PlayerCameraController>().FromComponentInNewPrefab(_playerCameraController).AsSingle();

            Container.Bind<DiInjecter>().AsSingle().NonLazy();
        }

        private void OnDestroy()
        {
            Debug.Log($"Flush bindings");
            Container.FlushBindings();
        }
    }
}