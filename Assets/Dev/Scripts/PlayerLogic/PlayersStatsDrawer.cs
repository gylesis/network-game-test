using System.Linq;
using Dev.Infrastructure;
using Dev.Infrastructure.Installers;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersStatsDrawer : NetworkBehaviour
    {
        [SerializeField] private StatsRenderer _statsRendererPrefab;
        [Networked, Capacity(4)] private NetworkDictionary<PlayerRef, StatsRenderer> _statsRenderers { get; }

        private PlayerCameraController _playerCameraController;
        private PlayersDataService _playersDataService;
        
        [Inject]
        private void Construct(PlayersDataService playersDataService, PlayerCameraController playerCameraController)
        {
            _playerCameraController = playerCameraController;
            _playersDataService = playersDataService;
        }

        public override void Spawned()
        {
            _playersDataService.NicknameChanged.TakeUntilDestroy(this).Subscribe((OnPlayerNicknameChanged));
            _playersDataService.PlayerSpawned.TakeUntilDestroy(this).Subscribe(OnPlayerSpawned);
            _playersDataService.PlayerDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        private void OnPlayerNicknameChanged(PlayerRef playerRef)
        {
            StatsRenderer statsRenderer = _statsRenderers.Get(playerRef);

            statsRenderer.UpdateNickname(_playersDataService.GetNickname(playerRef));
        }

        private void OnPlayerSpawned(PlayerRef playerRef)
        {
            if (Runner.IsServer == false) return;

            StatsRenderer statsRenderer = Runner.Spawn(_statsRendererPrefab, inputAuthority: playerRef);

            statsRenderer.UpdateNickname(_playersDataService.GetNickname(playerRef));
            _statsRenderers.Add(playerRef,statsRenderer);
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            if (Runner.IsServer == false) return;

            StatsRenderer statsRenderer = _statsRenderers.First(x => x.Key == playerRef).Value;

            Runner.Despawn(statsRenderer.Object);

            _statsRenderers.Remove(playerRef);
        }

        public override void Render()
        {
            for (var index = _playersDataService.Players.Count - 1; index >= 0; index--)
            {
                PlayerFacade player = _playersDataService.Players[index];
                
                if(player == null) return;
                
                if(player.PlayerInstance == null) return;
                
                PlayerRef playerRef = player.Object.InputAuthority;

                PlayerSessionData playerSessionData = _playersDataService.SessionData.Get(playerRef);

                Vector3 pos = player.transform.position + Vector3.up * 2.5f;

                StatsRenderer statsRenderer = _statsRenderers.Get(playerRef);

                int health = playerSessionData.Health;
                
                statsRenderer.UpdateHealth(health);

                statsRenderer.transform.position = pos;
                statsRenderer.transform.rotation =
                    Quaternion.LookRotation(_playerCameraController.CameraForwardDirection);
            }
        }
    }
}