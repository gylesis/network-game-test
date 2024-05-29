using Fusion;

namespace Dev.Infrastructure
{
    public struct PlayerSessionData : INetworkStruct
    {
        [Networked] public int Health { get; set; }
    }
}