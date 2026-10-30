using System.Collections;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ForestDarkness : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField, Range(0.05f, 1f)] private float _darkFactor = 0.3f;
        [SerializeField] private float _brightenSeconds = 4f;

        private IEventBus _eventBus;
        private Light _sun;
        private float _sunIntensity;
        private float _ambientIntensity;
        private bool _brightened;

        private void Awake()
        {
            _sun = RenderSettings.sun;
            if (_sun == null)
                _sun = FindSun();

            _ambientIntensity = RenderSettings.ambientIntensity;
            if (_sun != null)
                _sunIntensity = _sun.intensity;

            Apply(_darkFactor);
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDestroy()
        {
            Apply(1f);
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus?.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
            _eventBus = eventBus;
            if (_eventBus != null && isActiveAndEnabled)
                _eventBus.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private static Light FindSun()
        {
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
            {
                if (light.type == LightType.Directional)
                    return light;
            }

            return null;
        }

        private void OnDeedRecorded(DeedRecordedEvent deed)
        {
            if (_brightened || deed.DeedId != Firefly.HealedDeedId)
                return;

            _brightened = true;
            StartCoroutine(Brighten());
        }

        private IEnumerator Brighten()
        {
            var elapsed = 0f;
            while (elapsed < _brightenSeconds)
            {
                elapsed += Time.deltaTime;
                Apply(Mathf.Lerp(_darkFactor, 1f, Mathf.Clamp01(elapsed / _brightenSeconds)));
                yield return null;
            }

            Apply(1f);
        }

        private void Apply(float factor)
        {
            RenderSettings.ambientIntensity = _ambientIntensity * factor;
            if (_sun != null)
                _sun.intensity = _sunIntensity * factor;
        }
    }
}
