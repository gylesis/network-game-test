using Dev.Infrastructure.Networking;
using UnityEngine;

namespace Dev.PlayerLogic
{
    public class Player : NetworkContext
    {
        [SerializeField] private Animator _animator;
        
        public Animator Animator => _animator;
    }
}