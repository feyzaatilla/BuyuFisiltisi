using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class DriftMotion : MonoBehaviour
    {
        [SerializeField] private float _amplitude = 0.5f;
        [SerializeField] private float _speed = 0.4f;

        private Vector3 _origin;
        private float _seed;

        private void Awake()
        {
            _origin = transform.localPosition;
            _seed = Random.value * 100f;
        }

        private void Update()
        {
            var t = Time.time * _speed;
            var offset = new Vector3(
                Mathf.PerlinNoise(_seed, t) - 0.5f,
                Mathf.PerlinNoise(_seed + 10f, t) - 0.5f,
                Mathf.PerlinNoise(_seed + 20f, t) - 0.5f);
            transform.localPosition = _origin + offset * (_amplitude * 2f);
        }
    }
}
