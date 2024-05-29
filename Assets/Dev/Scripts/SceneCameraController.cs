using UnityEngine;

namespace Dev
{
    public class SceneCameraController : MonoBehaviour
    {
        [SerializeField] private Camera _mainCamera;

        public void SetState(bool isOn)
        {
            _mainCamera.gameObject.SetActive(isOn);
        }
    }
}