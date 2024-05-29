using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class InputService : NetworkContext
    {
        private Vector3 _prevMousePos;
            
        private NetworkEventsDispatcher _networkEventsDispatcher;
        private PlayerCameraController _playerCameraController;

        public LocalPlayerInput LocalPlayerInput { get; private set; }


        [Inject]
        private void Construct(NetworkEventsDispatcher networkEventsDispatcher, PlayerCameraController playerCameraController)
        {
            _playerCameraController = playerCameraController;
            _networkEventsDispatcher = networkEventsDispatcher;
        }

        protected override void Start()
        {
            base.Start();
            _networkEventsDispatcher.Input += OnInput;
        }

        private void OnDisable()
        {
            _networkEventsDispatcher.Input -= OnInput;
        }

        public override void Render()
        {
            if (HasInputAuthority)
            {
                Vector3 currentMousePos = Input.mousePosition;
                
                LocalPlayerInput localPlayerInput = new LocalPlayerInput();
                
                localPlayerInput.MouseDelta = Input.GetAxis("Mouse X");
                
                LocalPlayerInput = localPlayerInput;
                
                _prevMousePos = currentMousePos;
            }
        }

        private void OnInput(NetworkRunner runner, NetworkInput input)
        {
            var x = Input.GetAxisRaw("Horizontal");
            var y = Input.GetAxisRaw("Vertical");

            bool jump = Input.GetAxis("Jump") > 0;
            bool sprint = Input.GetKey(KeyCode.LeftShift);

            var keyBoardInput = new Vector2(x, y);

            PlayerInput playerInput = new PlayerInput();

            playerInput.MoveDirection = keyBoardInput.normalized;
            playerInput.Rotation = _playerCameraController.CameraForwardDirection;
            playerInput.Buttons.Set(InputButtons.Jump, jump);
            playerInput.Buttons.Set(InputButtons.Run, sprint);

            input.Set(playerInput);
        }
    }
}