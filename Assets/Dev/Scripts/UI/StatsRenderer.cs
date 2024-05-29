using Fusion;
using TMPro;
using UnityEngine;

namespace Dev
{
    public class StatsRenderer : NetworkBehaviour
    {
        [SerializeField] private TMP_Text _text;

        [Networked] private NetworkString<_32> _nickname { get; set; }
        
        public void UpdateNickname(string nickname)
        {
            _nickname = nickname;
        }
        
        public void UpdateHealth(int health)
        {
            _text.text = $"{_nickname} \nHealth: {health}";
        }   
        
    }
}