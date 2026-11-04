using UnityEngine;

namespace BuyuFisiltisi.UI.Menu
{
    public sealed class MenuCameraDrift : MonoBehaviour
    {
        [SerializeField] private Transform _lookAt;
        [SerializeField] private float _sway = 1.2f;
        [SerializeField] private float _speed = 0.12f;

        private Vector3 _origin;

        private void Awake()
        {
            _origin = transform.position;
        }

        private void LateUpdate()
        {
            transform.position = _origin + MenuBackdropLayout.CameraOffset(Time.unscaledTime, _sway, _speed);
            if (_lookAt != null)
                transform.LookAt(_lookAt.position);
        }
    }
}
