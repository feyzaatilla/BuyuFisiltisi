using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    [RequireComponent(typeof(Light))]
    public sealed class FlickerLight : MonoBehaviour
    {
        [SerializeField] private float _amount = 0.35f;
        [SerializeField] private float _speed = 6f;

        private Light _light;
        private float _baseIntensity;
        private float _offset;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _baseIntensity = _light.intensity;
            _offset = Random.value * 100f;
        }

        private void Update()
        {
            var noise = Mathf.PerlinNoise(_offset, Time.time * _speed);
            _light.intensity = _baseIntensity * (1f - _amount + noise * _amount * 2f);
        }
    }
}
