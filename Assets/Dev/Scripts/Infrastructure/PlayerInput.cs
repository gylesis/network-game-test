using Fusion;
using UnityEngine;

namespace Dev.Infrastructure
{
    public struct PlayerInput : INetworkInput
    {
        public Vector2 MoveDirection;
        public Vector3 Rotation;
        public NetworkButtons Buttons;
    }

    public enum InputButtons
    {
        Jump = 0,
        Run = 1
    }
}