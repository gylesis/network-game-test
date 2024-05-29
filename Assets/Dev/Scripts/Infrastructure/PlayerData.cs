using Fusion;

namespace Dev.Infrastructure
{
    public struct PlayerData : INetworkStruct
    {
        [Networked] private NetworkString<_32> _nickname { get; set; }

        public string Nickname => _nickname.ToString();

        public void SetNickname(string nickname)
        {
            _nickname = nickname;
        }
    }
}