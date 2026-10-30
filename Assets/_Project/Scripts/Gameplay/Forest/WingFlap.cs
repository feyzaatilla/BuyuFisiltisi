using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class WingFlap : MonoBehaviour
    {
        [SerializeField] private float _side = 1f;
        [SerializeField] private float _speed = 22f;
        [SerializeField] private float _angle = 35f;

        private void Update()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, _side * Mathf.Sin(Time.time * _speed) * _angle);
        }
    }
}
