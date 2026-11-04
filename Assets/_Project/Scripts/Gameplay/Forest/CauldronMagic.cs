using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class CauldronMagic : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private Transform[] _motes = new Transform[0];
        [SerializeField] private Transform[] _bubbles = new Transform[0];
        [SerializeField] private Transform[] _wisps = new Transform[0];
        [SerializeField] private float _orbitRadius = 2.4f;
        [SerializeField] private Renderer _surface;
        [SerializeField] private Light _light;
        [SerializeField] private float _radius = 1.1f;
        [SerializeField] private float _riseHeight = 2.6f;
        [SerializeField] private float _surfaceHeight = 1.6f;

        private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

        private readonly Color _green = new Color(0.42f, 1f, 0.54f);
        private readonly Color _purple = new Color(0.7f, 0.35f, 1f);

        private IEventBus _eventBus;
        private MaterialPropertyBlock _block;
        private float _burst;
        private float _baseIntensity;

        public void InjectEventBus(IEventBus eventBus)
        {
            if (_eventBus != null)
                _eventBus.Unsubscribe<RecipeCompletedEvent>(OnRecipeCompleted);
            _eventBus = eventBus;
            _eventBus?.Subscribe<RecipeCompletedEvent>(OnRecipeCompleted);
        }

        private void Awake()
        {
            _block = new MaterialPropertyBlock();
            _baseIntensity = _light != null ? _light.intensity : 0f;
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<RecipeCompletedEvent>(OnRecipeCompleted);
        }

        private void OnRecipeCompleted(RecipeCompletedEvent gameEvent)
        {
            _burst = 1f;
        }

        private void Update()
        {
            var time = Time.time;
            _burst = Mathf.MoveTowards(_burst, 0f, Time.deltaTime * 0.5f);
            var speed = 1f + _burst * 3f;

            for (var i = 0; i < _motes.Length; i++)
            {
                if (_motes[i] == null)
                    continue;

                var phase = Mathf.Repeat(time * 0.25f * speed + (float)i / _motes.Length, 1f);
                var angle = (i * 137.5f + time * 60f * speed) * Mathf.Deg2Rad;
                var radius = _radius * (1f - phase * 0.6f) * (1f + _burst * 0.5f);
                _motes[i].localPosition = new Vector3(Mathf.Cos(angle) * radius, _surfaceHeight + phase * _riseHeight * (1f + _burst), Mathf.Sin(angle) * radius);
                _motes[i].localScale = Vector3.one * (0.12f * Mathf.Sin(phase * Mathf.PI) + 0.02f) * (1f + _burst);
            }

            for (var i = 0; i < _wisps.Length; i++)
            {
                if (_wisps[i] == null)
                    continue;

                var angle = (time * 35f * speed + i * 360f / _wisps.Length) * Mathf.Deg2Rad;
                var wobble = Mathf.Sin(time * 1.7f + i * 1.3f);
                var radius = _orbitRadius + wobble * 0.3f;
                _wisps[i].localPosition = new Vector3(Mathf.Cos(angle) * radius, 0.9f + wobble * 0.6f + (i % 3) * 0.5f, Mathf.Sin(angle) * radius);
                _wisps[i].localScale = Vector3.one * (0.18f + 0.06f * Mathf.Sin(time * 3f + i)) * (1f + _burst);
            }

            for (var i = 0; i < _bubbles.Length; i++)
            {
                if (_bubbles[i] == null)
                    continue;

                var phase = Mathf.Repeat(time * (0.6f + i * 0.13f) * speed + i * 0.37f, 1f);
                _bubbles[i].localScale = Vector3.one * (0.25f * phase);
            }

            var pulse = 0.5f + 0.5f * Mathf.Sin(time * 1.3f);
            var color = Color.Lerp(_green, _purple, pulse);
            if (_surface != null)
            {
                _surface.GetPropertyBlock(_block);
                _block.SetColor(BaseColor, color);
                _block.SetColor(EmissionColor, color * (1.5f + _burst * 3f));
                _surface.SetPropertyBlock(_block);
            }

            if (_light != null)
            {
                _light.color = color;
                _light.intensity = _baseIntensity * (1f + _burst * 2f);
            }
        }
    }
}
