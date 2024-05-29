using System;
using System.Collections.Generic;
using System.Linq;
using Dev.Infrastructure.Installers;
using Dev.Infrastructure.Networking;
using Dev.Utils;
using DG.Tweening;
using Fusion;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Dev.PlayerLogic
{
    public class PlayerSpawner : NetworkBehaviour
    {
        [SerializeField] private BoxCollider _spawnBounds;
        [SerializeField] private Player _playerPrefab;
        [SerializeField] private PlayerFacade _playerFacadePrefab;
        [SerializeField] private Transform[] _spawnPoints;
        
        private NetworkEventsDispatcher _networkEventsDispatcher;
        private SceneCameraController _sceneCameraController;
        
        [Networked, Capacity(4)] public NetworkDictionary<PlayerRef, PlayerFacade> Players { get; }

        public List<PlayerFacade> PlayersList => Players.Select(x => x.Value).ToList();
        public Subject<PlayerRef> PlayerSpawned { get; } = new Subject<PlayerRef>();
        public Subject<PlayerRef> PlayerDeSpawned { get; } = new Subject<PlayerRef>();

        [Inject]    
        private void Construct(NetworkEventsDispatcher networkEventsDispatcher,
                              SceneCameraController sceneCameraController)
        {
            _sceneCameraController = sceneCameraController;
            _networkEventsDispatcher = networkEventsDispatcher;
        }

        private void Start()
        {
            _networkEventsDispatcher.PlayerJoined += OnPlayerJoined;
            _networkEventsDispatcher.PlayerLeft += OnPlayerLeft;
        }

        private void OnDestroy()
        {
            _networkEventsDispatcher.PlayerJoined -= OnPlayerJoined;
            _networkEventsDispatcher.PlayerLeft -= OnPlayerLeft;
        }

        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            RPC_SetCameraState(player);

            SpawnPlayerFacade(runner, player);
        }

        public void SpawnPlayerFacade(NetworkRunner runner, PlayerRef playerRef)
        {
            if (runner.IsServer == false) return;

            RPC_SetCursorState(playerRef, false);
            
            Vector3 spawnPos = GetSpawnPosForPlayer();

            PlayerFacade spawnedPlayer = runner.Spawn(_playerFacadePrefab, spawnPos, inputAuthority: playerRef);

            Players.Add(playerRef, spawnedPlayer);
            spawnedPlayer.PlayerController.SetAllowToMove(false);
            
            runner.SetPlayerObject(playerRef, spawnedPlayer.Object);
            spawnedPlayer.Object.AssignInputAuthority(playerRef);
         
            SpawnPlayer(playerRef);
        }

        [Rpc]
        private void RPC_SetCursorState([RpcTarget] PlayerRef playerRef, bool isOn)
        {
            Cursor.visible = isOn;
            Cursor.lockState = isOn ? CursorLockMode.None : CursorLockMode.Locked;
        }

        private void SpawnPlayer(PlayerRef playerRef)
        {
            PlayerFacade playerFacade = Players.Get(playerRef);

            Player player = Runner.Spawn(_playerPrefab, position: playerFacade.transform.position + Vector3.up * 0.5f, onBeforeSpawned: (runner, o) =>
            {
                o.transform.parent = playerFacade.transform;
            });
            
            player.Object.AssignInputAuthority(playerRef);
            
            playerFacade.SetPlayerInstance(player);
            playerFacade.PlayerController.SetAllowToMove(true);
            
            PlayerSpawned.OnNext(playerRef);
        }
        
        private Vector3 GetSpawnPosForPlayer()
        {
            Vector3 spawnPos = _spawnPoints[Random.Range(0, _spawnPoints.Length)].position;

            return Extensions.GetClosestGroundPos(spawnPos);
        }
        
        public void DespawnPlayer(PlayerRef playerRef)
        {
            if (Runner.IsServer)
            {
                if (Players.ContainsKey(playerRef))
                {
                    Runner.Despawn(Players[playerRef].Object);

                    Players.Remove(playerRef);
                    
                    PlayerDeSpawned.OnNext(playerRef);
                }
            }
        }

        public void RespawnPlayer(PlayerRef playerRef, float delay = 0, Action onRespawned = null)
        {
            bool hasPlayer = TryGetPlayer(playerRef, out var player);

            if (hasPlayer)
            {
                var sequence = DOTween.Sequence();

                sequence
                    .AppendInterval(delay)  
                    .AppendCallback((() =>
                    {
                        Vector3 spawnPos = GetSpawnPosForPlayer();

                        player.NetworkRigidbody.Teleport(spawnPos);
                        RPC_SetCameraForcePos(playerRef, spawnPos);
                    }))
                    .AppendInterval(0.5f)
                    .AppendCallback((() =>
                    {
                        player.PlayerController.SetAllowToMove(true);
                        player.PlayerInstance.RPC_SetActive(true);
                        
                        onRespawned?.Invoke();
                    }));
            }
        }

        [Rpc]
        private void RPC_SetCameraForcePos([RpcTarget] PlayerRef playerRef, Vector3 pos)
        {
            Players[playerRef].PlayerCameraController.SetForcedPos(pos);
        }
        
        public bool HasPlayerInstance(PlayerRef playerRef) => Players.ContainsKey(playerRef);

        public bool TryGetPlayer(PlayerRef playerRef, out PlayerFacade player)
        {
            player = null;

            bool hasPlayerInstance = HasPlayerInstance(playerRef);

            if (hasPlayerInstance)
            {
                player = Players[playerRef];
            }
            else
            {
                Debug.Log($"Player {playerRef} doesn't have instance");
            }
                
            
            return hasPlayerInstance;
        }
        
        [Rpc]
        private void RPC_SetCameraState([RpcTarget] PlayerRef playerRef)
        {
            _sceneCameraController.SetState(false);
            
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
           DespawnPlayer(player);
        }
    }
}