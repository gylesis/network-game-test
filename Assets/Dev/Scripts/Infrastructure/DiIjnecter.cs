using UnityEngine;
using Zenject;

namespace Dev.Infrastructure
{
    public class DiInjecter 
    {
        private DiContainer _diContainer;

        public static DiInjecter Instance { get; private set; }

        public DiInjecter(DiContainer diContainer)
        {
            Instance = this;
            _diContainer = diContainer;
        }
        
        public void Inject(GameObject gameObject)
        {
            _diContainer.InjectGameObject(gameObject);
        }
    }
}