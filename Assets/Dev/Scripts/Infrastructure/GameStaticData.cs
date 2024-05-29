using UnityEngine;

namespace Dev.Infrastructure
{
    [CreateAssetMenu(fileName = "GameStaticData", menuName = "StaticDat/GameStaticData  ", order = 0)]
    public class GameStaticData : ScriptableObject
    {
        [Header("Player")]
        
        [SerializeField] private int _playerStartHealth = 100;
        [SerializeField] private float _cameraRotationSpeed = 2f;
        [SerializeField] private float _cameraFollowSpeed = 2f;
        [SerializeField] private float _moveSpeed = 300;
        [SerializeField] private float _sprintModifier = 1.2f;
        [SerializeField] private float _jumpPower = 5;
        [SerializeField] private float _fallDamageDistance = 3f;
        [SerializeField] private float _horizontalMoveSpeedMultiplier = 0.5f;
        [SerializeField] private float _backwardMoveSpeedMultiplier = 0.4f;
        [SerializeField] private float _groundCheckDistance = 0.1f;
        
        [Header("Health Boxes")]
        [SerializeField] private int _maxHealthBoxesAtScene = 5;
        [SerializeField] private int _healthRestoreGained = 30;
        [SerializeField] private float _spawnRate = 8;

        public int MaxHealthBoxesAtScene => _maxHealthBoxesAtScene;
        public int HealthBoxRestoreGained => _healthRestoreGained;
        public float HealthBoxesSpawnRate => _spawnRate;

        public float CameraFollowSpeed => _cameraFollowSpeed;

        public float SprintModifier => _sprintModifier;

        public float GroundCheckDistance => _groundCheckDistance;

        public float MoveSpeed => _moveSpeed;

        public float JumpPower => _jumpPower;

        public float FallDamageDistance => _fallDamageDistance;

        public float HorizontalMoveSpeedMultiplier => _horizontalMoveSpeedMultiplier;

        public float BackwardMoveSpeedMultiplier => _backwardMoveSpeedMultiplier;

        public int PlayerStartHealth => _playerStartHealth;

        public float CameraRotationSpeed => _cameraRotationSpeed;
    }
}