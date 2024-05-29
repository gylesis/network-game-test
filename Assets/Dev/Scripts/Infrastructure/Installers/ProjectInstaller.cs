using Dev.Infrastructure.Networking;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class ProjectInstaller : MonoInstaller
    {
        [SerializeField] private GameStaticData _gameStaticData;
        
        public override void InstallBindings()
        {
            Container.Bind<GameStaticData>().FromInstance(_gameStaticData).AsSingle();
            
            Container.Bind<NetworkEventsDispatcher>().AsSingle().NonLazy();
            Container.Bind<LocalPlayerData>().AsSingle().NonLazy();
        }
    }
}