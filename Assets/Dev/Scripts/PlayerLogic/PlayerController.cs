using Dev.Infrastructure;
using Dev.Infrastructure.Installers;
using Fusion;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private LayerMask _groundLayerMask;
        [SerializeField] private PlayerFacade _player;
        
        private float _maxHeightFromFall;

        private PlayersDataService _playersDataService;
        private GameStaticData _gameStaticData;
            
        [Networked] private NetworkButtons ButtonsPrevious { get; set; }
        [Networked] private float VerticalInput { get; set; }
        [Networked] private float HorizontalInput { get; set; }
        [Networked] private Vector3 Rotation { get; set; }
        [Networked] private Vector3 Move { get; set; }

        [Networked] private NetworkBool AllowToMove { get; set; } = false;
        [Networked] private NetworkBool IsGrounded { get; set; }
        
        [Inject]
        private void Construct(PlayersDataService playersDataService, GameStaticData gameStaticData)
        {
            _gameStaticData = gameStaticData;
            _playersDataService = playersDataService;
        }

        public override void FixedUpdateNetwork()
        {
            if (GetInput<PlayerInput>(out var input))
            {
                var toJump = input.Buttons.WasPressed(ButtonsPrevious,InputButtons.Jump);
                bool toRun = input.Buttons.IsSet(InputButtons.Run);
                
                ButtonsPrevious = input.Buttons;
                
                VerticalInput = input.MoveDirection.y;
                HorizontalInput = input.MoveDirection.x;
                Rotation = input.Rotation;
                
                HandleJump(toJump);
                HandleMoveAndRotation(toRun);
            }
            
            if (HasStateAuthority)
            {
                HandleGrounded();
            }
        }

        public void SetAllowToMove(bool allow)
        {
            AllowToMove = allow;
            _player.NetworkRigidbody.Rigidbody.isKinematic = !allow;
            _player.NetworkRigidbody.Rigidbody.useGravity = allow;
        }

        public override void Render()
        {
            if(_player.PlayerInstance == null) return;
            
            _player.PlayerInstance.Animator.SetFloat("Vertical",VerticalInput);
            _player.PlayerInstance.Animator.SetFloat("Horizontal", HorizontalInput);
            _player.PlayerInstance.Animator.SetBool("IsGrounded", IsGrounded);
            _player.PlayerInstance.Animator.SetBool("IsFalling", _player.NetworkRigidbody.Rigidbody.velocity.y < -1f || IsGrounded == false);
        }

        private void HandleMoveAndRotation(bool toRun)
        {
            if(AllowToMove == false) return;
            
            float moveY = VerticalInput;    
            float moveX = HorizontalInput * _gameStaticData.HorizontalMoveSpeedMultiplier;
            
            if (moveY < 0)
            {
                moveY *= _gameStaticData.BackwardMoveSpeedMultiplier;
            }
            else
            {
                if (toRun && IsGrounded)
                {
                    moveY *= _gameStaticData.SprintModifier;
                }
            }
           
            
            Vector3 move =  _player.transform.forward * moveY +
                            _player.transform.right * moveX;

            Vector3 prevVelocity = _player.NetworkRigidbody.Rigidbody.velocity;

            Vector3 moveVector = move * _gameStaticData.MoveSpeed * Runner.DeltaTime;
            moveVector.y = prevVelocity.y;

            Move = moveVector;
            
            _player.NetworkRigidbody.Rigidbody.velocity = Move;
            _player.NetworkRigidbody.RBRotation =
                Quaternion.LookRotation(Rotation);
        }

        private void HandleJump(bool toJump)
        {
            if(AllowToMove == false) return;

            if (toJump)
            {
                if (IsGrounded)
                {
                    IsGrounded = false;
                    _player.NetworkRigidbody.Rigidbody.velocity += Vector3.up * _gameStaticData.JumpPower;
                }
            }
        }


        private void HandleGrounded()
        {
            bool isGrounded = Physics.SphereCast(_player.transform.position + Vector3.up * 0.2f, 0.08f, Vector3.down, out var hit,
                _gameStaticData.GroundCheckDistance, _groundLayerMask);
            
            var notGroundedNow = isGrounded == false;
            
            if (notGroundedNow)
            {
                if (IsGrounded)
                {
                    Debug.Log($"Left from ground");
                }

                if (_player.transform.position.y > _maxHeightFromFall)
                {
                    _maxHeightFromFall = _player.transform.position.y;
                }
            }
            else
            {
                if (IsGrounded == false)
                {
                    float currentHeight = _player.transform.position.y;
                    float difference = _maxHeightFromFall - currentHeight;

                    if (difference >= _gameStaticData.FallDamageDistance)
                    {
                        ApplyDamageFromFall(difference);
                        Debug.Log($"Fall damage applied");
                    }
                    else
                    {
                        Debug.Log($"Fall distance too small to apply damage");
                    }

                    Debug.Log($"Landed, diff: {difference}");

                    _maxHeightFromFall = 0;
                }
            }

            IsGrounded = isGrounded;
        }

        private void ApplyDamageFromFall(float difference)
        {
            int heightStepDamage = 10;

            int totalDamage = (Mathf.CeilToInt(difference) - (int) _gameStaticData.FallDamageDistance) * heightStepDamage;

            _playersDataService.ApplyDamage(Object.InputAuthority, totalDamage);

            Debug.Log($"Total damage applied {totalDamage}");
        }
    }
}