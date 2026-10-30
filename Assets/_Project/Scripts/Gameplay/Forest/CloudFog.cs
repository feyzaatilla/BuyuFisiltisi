using System.Collections;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class CloudFog : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private Color _fogColor = new Color(0.45f, 0.47f, 0.52f);
        [SerializeField] private float _startDensity = 0.06f;
        [SerializeField] private float _fadeSeconds = 1.5f;

        private IEventBus _eventBus;
        private bool _originalFog;
        private Color _originalColor;
        private FogMode _originalMode;
        private float _originalDensity;
        private Coroutine _fade;

        private void Awake()
        {
            _originalFog = RenderSettings.fog;
            _originalColor = RenderSettings.fogColor;
            _originalMode = RenderSettings.fogMode;
            _originalDensity = RenderSettings.fogDensity;

            RenderSettings.fog = true;
            RenderSettings.fogColor = _fogColor;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogDensity = _startDensity;
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<CloudDispersedEvent>(OnCloudDispersed);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<CloudDispersedEvent>(OnCloudDispersed);
        }

        private void OnDestroy()
        {
            RenderSettings.fog = _originalFog;
            RenderSettings.fogColor = _originalColor;
            RenderSettings.fogMode = _originalMode;
            RenderSettings.fogDensity = _originalDensity;
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus?.Unsubscribe<CloudDispersedEvent>(OnCloudDispersed);
            _eventBus = eventBus;
            if (_eventBus != null && isActiveAndEnabled)
                _eventBus.Subscribe<CloudDispersedEvent>(OnCloudDispersed);
        }

        private void OnCloudDispersed(CloudDispersedEvent dispersed)
        {
            if (dispersed.Total <= 0)
                return;

            var remaining = 1f - (float)dispersed.Dispersed / dispersed.Total;
            var target = _startDensity * remaining;

            if (_fade != null)
                StopCoroutine(_fade);

            _fade = StartCoroutine(FadeTo(target));
        }

        private IEnumerator FadeTo(float target)
        {
            var start = RenderSettings.fogDensity;
            var elapsed = 0f;
            while (elapsed < _fadeSeconds)
            {
                elapsed += Time.deltaTime;
                RenderSettings.fogDensity = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / _fadeSeconds));
                yield return null;
            }

            RenderSettings.fogDensity = target;
        }
    }
}
