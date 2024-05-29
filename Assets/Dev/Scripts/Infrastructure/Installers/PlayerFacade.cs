using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;
using Zenject;

namespace Dev.Infrastructure.Installers
{
    public class PlayerFacade : NetworkContext
    {
        [SerializeField] private NetworkRigidbody3D _networkRigidbody;
        [SerializeField] private PlayerController _playerController;
        [SerializeField] private InputService _inputService;
       
        private PlayerCameraController _playerCameraController;

        [Networked] public Player PlayerInstance { get; private set; }
        
        public NetworkRigidbody3D NetworkRigidbody => _networkRigidbody;
        public PlayerCameraController PlayerCameraController => _playerCameraController;
        public InputService InputService => _inputService;
        public PlayerController PlayerController => _playerController;

        [Inject]
        private void Construct(PlayerCameraController playerCameraController)
        {
            _playerCameraController = playerCameraController;
        }

        public override void Spawned()
        {
            base.Spawned();

            if (HasInputAuthority)
            {
                _playerCameraController.SetupPlayer(this);
            }
        }

        public void SetPlayerInstance(Player player)
        {
            PlayerInstance = player;
        }

        public override void Render()
        {
            if (PlayerInstance == null) return;

            if (NetworkRigidbody.InterpolationTarget == null)
            {
                _networkRigidbody.SetInterpolationTarget(PlayerInstance.transform);
            }
        }
    }
}