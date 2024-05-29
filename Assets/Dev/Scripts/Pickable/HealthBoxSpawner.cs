using Dev.Infrastructure;
using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev
{
    public class HealthBoxSpawner : NetworkContext
    {
        [SerializeField] private HealthBox _healthBoxPrefab;
        [SerializeField] private Transform[] _spawnPoints;
        
        private int _spawnedAmount = 0;
        private TickTimer _spawnTimer;

        private PlayersDataService _playersDataService;
        private GameStaticData _gameStaticData;

        [Inject]
        private void Construct(PlayersDataService playersDataService, GameStaticData gameStaticData)
        {
            _gameStaticData = gameStaticData;
            _playersDataService = playersDataService;
        }

        public override void Spawned()
        {
            base.Spawned();

            if (Runner.IsServer)
            {
                _spawnTimer = TickTimer.CreateFromSeconds(Runner, _gameStaticData.HealthBoxesSpawnRate);
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (Runner.IsServer)
            {
                if(_spawnedAmount >= _gameStaticData.MaxHealthBoxesAtScene) return;
                
                if (_spawnTimer.ExpiredOrNotRunning(Runner))
                {
                    _spawnTimer = TickTimer.CreateFromSeconds(Runner, _gameStaticData.HealthBoxesSpawnRate);

                    Transform spawnPoint = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

                    HealthBox healthBox = Runner.Spawn(_healthBoxPrefab, spawnPoint.position);
                    healthBox.PlayerPickedUp.TakeUntilDestroy(this)
                        .Subscribe((player => OnPlayerPickedUpHealthBox(player, healthBox)));
                    
                    _spawnedAmount++;
                }
            }
        }

        private void OnPlayerPickedUpHealthBox(PlayerRef playerRef, HealthBox healthBox)
        {
            int playerHealth = _playersDataService.GetPlayerHealth(playerRef);
            Debug.Log($"Health {playerHealth}");

            if (_playersDataService.IsPlayerHasFullHealth(playerRef))
            {
                Debug.Log($"Player {playerRef} has full health, skippin health box");
                return; 
            }
            
            _spawnedAmount--;
            Runner.Despawn(healthBox.Object);
            _playersDataService.GainHealthToPlayer(playerRef, _gameStaticData.HealthBoxRestoreGained);
        }
    }
}