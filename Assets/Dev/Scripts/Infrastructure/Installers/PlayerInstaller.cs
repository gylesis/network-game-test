using Dev.PlayerLogic;
using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class PlayerInstaller : MonoInstaller
    {
        
        public override void InstallBindings()
        {
            Container.Bind<PlayerController>().FromComponentOnRoot().AsSingle();
            //Container.Bind<PlayerCameraController>().FromInstance(_playerCameraController);
        }

    }
}