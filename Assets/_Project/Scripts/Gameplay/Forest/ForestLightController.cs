using System.Collections;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ForestLightController : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private Light[] _lights = new Light[0];
        [SerializeField] private GameObject[] _darknessObjects = new GameObject[0];
        [SerializeField] private float _duration = 3f;

        private IEventBus _eventBus;
        private float[] _targetIntensities;
        private bool _lit;

        public bool IsLit => _lit;

        private void Awake()
        {
            _targetIntensities = new float[_lights.Length];
            for (int i = 0; i < _lights.Length; i++)
            {
                _targetIntensities[i] = _lights[i].intensity;
                _lights[i].intensity = 0f;
            }
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus?.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
            _eventBus = eventBus;
            if (_eventBus != null && isActiveAndEnabled)
                _eventBus.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDeedRecorded(DeedRecordedEvent deed)
        {
            if (_lit || deed.DeedId != Firefly.HealedDeedId)
                return;

            _lit = true;
            StartCoroutine(LightUp());
        }

        private IEnumerator LightUp()
        {
            float elapsed = 0f;
            while (elapsed < _duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _duration);
                for (int i = 0; i < _lights.Length; i++)
                    _lights[i].intensity = _targetIntensities[i] * t;
                yield return null;
            }

            for (int i = 0; i < _darknessObjects.Length; i++)
                _darknessObjects[i].SetActive(false);
        }
    }
}