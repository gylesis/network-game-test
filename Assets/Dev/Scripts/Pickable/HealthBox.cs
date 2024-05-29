using Dev.Infrastructure.Installers;
using Dev.Infrastructure.Networking;
using Dev.PlayerLogic;
using Fusion;
using UniRx;
using UnityEngine;

namespace Dev
{
    public class HealthBox : NetworkContext
    {
        [SerializeField] private Transform _view;

        public Subject<PlayerRef> PlayerPickedUp { get; } = new Subject<PlayerRef>();

        protected override void Start()
        {
            base.Start();
        
            Vector3 eulerAngles = _view.transform.eulerAngles;
            eulerAngles.y = Random.Range(0, 360);
            _view.transform.rotation = Quaternion.Euler(eulerAngles);
        }
        
        private void Update()
        {
            Vector3 eulerAngles = _view.transform.eulerAngles;
            eulerAngles.y += Time.deltaTime * 25;
            _view.transform.rotation = Quaternion.Euler(eulerAngles);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                bool isPlayer = other.TryGetComponent<PlayerFacade>(out var player);

                if (isPlayer)
                {
                    PlayerRef playerRef = player.Object.InputAuthority;
                    Debug.Log($"Player {playerRef} is about to pickup health");                    
                    
                    PlayerPickedUp.OnNext(playerRef);
                }
            }
        }
    }   
}