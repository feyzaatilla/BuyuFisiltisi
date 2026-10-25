using UnityEngine;

namespace BuyuFisiltisi.Companion.Visual
{
    public sealed class HoverVisual : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 0.08f;
        [SerializeField] private float _frequency = 1.2f;
        [SerializeField] private float _swayAngle = 6f;

        private Vector3 _baseLocalPosition;
        private Quaternion _baseLocalRotation;
        private float _phase;

        private void Awake()
        {
            _baseLocalPosition = transform.localPosition;
            _baseLocalRotation = transform.localRotation;
            _phase = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            var wave = Mathf.Sin(Time.time * _frequency * Mathf.PI * 2f + _phase);
            transform.localPosition = _baseLocalPosition + Vector3.up * (wave * _amplitude);
            transform.localRotation = _baseLocalRotation * Quaternion.Euler(0f, 0f, wave * _swayAngle);
        }
    }
}
