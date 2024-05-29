using Dev.Infrastructure;
using Dev.Infrastructure.Installers;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayerCameraController : MonoBehaviour
    {
        [SerializeField] private Transform _cameraParent;
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _cameraFollowSpeed = 2;
        
        [SerializeField] private PlayerFacade _localPlayer;
        private GameStaticData _gameStaticData;
        private PlayersDataService _playersDataService;

        public Vector3 CameraForwardDirection => Vector3.ProjectOnPlane(_playerCamera.transform.forward, Vector3.up).normalized;
        public Vector3 CameraRightDirection => Vector3.ProjectOnPlane(_playerCamera.transform.right, Vector3.up).normalized;
        
        public void SetupPlayer(PlayerFacade player)
        {
            _localPlayer = player;
            Debug.Log($"Local player {_localPlayer.Object.InputAuthority}");
        }

        public void SetForcedPos(Vector3 pos)
        {
            _cameraParent.transform.position = pos;
        }

        [Inject]
        private void Construct(GameStaticData gameStaticData)
        {
            _gameStaticData = gameStaticData;
        }
        
        private void Update()
        {
            if(_localPlayer == null) return;
            
            _cameraParent.transform.position = Vector3.Lerp(_cameraParent.transform.position, _localPlayer.gameObject.transform.position , Time.deltaTime * _gameStaticData.CameraFollowSpeed);
            Vector3 eulerAngles = _cameraParent.transform.rotation.eulerAngles;

            eulerAngles.y += _localPlayer.InputService.LocalPlayerInput.MouseDelta * _gameStaticData.CameraRotationSpeed;
                
            _cameraParent.transform.rotation = Quaternion.Euler(eulerAngles);
        }
    }
}