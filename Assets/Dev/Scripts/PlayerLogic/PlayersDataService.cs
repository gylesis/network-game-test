using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Dev.Infrastructure;
using Dev.Infrastructure.Installers;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;

namespace Dev.PlayerLogic
{
    public class PlayersDataService : NetworkBehaviour
    {
        [SerializeField] private int _startHealth = 100;

        private LocalPlayerData _localPlayerData;
        private PlayerSpawner _playerSpawner;

        [Networked, Capacity(4)] public NetworkDictionary<PlayerRef, PlayerData> Data { get; }
        [Networked, Capacity(4)] public NetworkDictionary<PlayerRef, PlayerSessionData> SessionData { get; }
        public List<PlayerFacade> Players => _playerSpawner.PlayersList;

        public Subject<PlayerRef> PlayerSpawned => _playerSpawner.PlayerSpawned;
        public Subject<PlayerRef> PlayerDeSpawned => _playerSpawner.PlayerDeSpawned;
        public Subject<PlayerRef> NicknameChanged { get; } = new();

        [Inject]
        private void Construct(PlayerSpawner playerSpawner, LocalPlayerData localPlayerData)
        {
            _localPlayerData = localPlayerData;
            _playerSpawner = playerSpawner;
        }

        private void Start()
        {
            PlayerSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerSpawned));
            PlayerDeSpawned.TakeUntilDestroy(this).Subscribe((OnPlayerDespawned));
        }

        public override void Spawned()
        {
            RPC_AddNickname(Runner.LocalPlayer, _localPlayerData.Nickname);
        }

        [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
        private async void RPC_AddNickname(PlayerRef playerRef, NetworkString<_32> nickname)
        {
            if(Data.ContainsKey(playerRef))
            {
               if( Data.Get(playerRef).Nickname != String.Empty)
                return;
            }
            
            Stopwatch stopwatch = Stopwatch.StartNew();

            float timeoutCooldown = 15 * 1000;

            while (stopwatch.ElapsedMilliseconds < timeoutCooldown)
            {
                await Task.Yield();
                if (Data.ContainsKey(playerRef)) break;
            }

            PlayerData data = Data.Get(playerRef);
            data.SetNickname(nickname.ToString());

            Data.Set(playerRef, data);

            NicknameChanged.OnNext(playerRef);
        }

        private void OnPlayerSpawned(PlayerRef playerRef)
        {
            PlayerData playerData = new PlayerData();

            PlayerSessionData sessionData = new PlayerSessionData();
            sessionData.Health = 100;

            SessionData.Add(playerRef, sessionData);
            Data.Add(playerRef, playerData);
        }

        private void OnPlayerDespawned(PlayerRef playerRef)
        {
            Data.Remove(playerRef);
            SessionData.Remove(playerRef);
        }

        public void ApplyDamage(PlayerRef playerRef, int damage)
        {
            var sessionData = SessionData.Get(playerRef);

            sessionData.Health -= damage;
    
            if (sessionData.Health <= 0)
            {
                sessionData.Health = Mathf.Clamp(sessionData.Health, 0, _startHealth);
                OnPlayerDied(playerRef);
            }

            SessionData.Set(playerRef, sessionData);
        }

        public bool IsPlayerHasFullHealth(PlayerRef playerRef) => GetPlayerHealth(playerRef) == _startHealth;

        public bool TryGetPlayer(PlayerRef playerRef, out PlayerFacade player)
        {
            return _playerSpawner.TryGetPlayer(playerRef, out player);
        }
        
        public void GainHealthToPlayer(PlayerRef playerRef, int health)
        {
            PlayerSessionData sessionData = SessionData.Get(playerRef);

            sessionData.Health += health;
            sessionData.Health = Mathf.Clamp(sessionData.Health, 0, _startHealth);
            
            SessionData.Set(playerRef, sessionData);
        }

        public int GetPlayerHealth(PlayerRef playerRef)
        {
            return SessionData.Get(playerRef).Health;
        }

        public void RestoreHealth(PlayerRef playerRef)
        {
            GainHealthToPlayer(playerRef, _startHealth);
        }

        private void OnPlayerDied(PlayerRef playerRef)
        {
            bool hasPlayerInstance = _playerSpawner.TryGetPlayer(playerRef, out var player);

            if (hasPlayerInstance == false) return;

            player.PlayerInstance.RPC_SetActive(false);
            player.PlayerController.SetAllowToMove(false);

            _playerSpawner.RespawnPlayer(playerRef, 1, (() => { RestoreHealth(playerRef); }));
        }

        public string GetNickname(PlayerRef playerRef)
        {
            return Data.Get(playerRef).Nickname;
        }
    }
}