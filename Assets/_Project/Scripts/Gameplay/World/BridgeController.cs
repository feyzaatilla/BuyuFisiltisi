using System.Collections;
using System.Collections.Generic;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using Unity.AI.Navigation;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.World
{
    public sealed class BridgeController : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private string _switchId = BridgeSwitch.DefaultId;
        [SerializeField] private Transform _pivot;
        [SerializeField] private float _raisedAngle = 90f;
        [SerializeField] private float _loweredAngle = 0f;
        [SerializeField] private float _duration = 2f;
        [SerializeField] private NavMeshModifier _navMeshModifier;
        [SerializeField] private NavMeshSurface[] _surfaces = new NavMeshSurface[0];

        private IEventBus _eventBus;
        private BridgeLowering _lowering;
        private Vector3 _baseEuler;

        public bool IsLowered { get; private set; }

        private void Awake()
        {
            if (_pivot == null)
                _pivot = transform;

            _baseEuler = _pivot.localEulerAngles;
            _lowering = new BridgeLowering(_raisedAngle, _loweredAngle, _duration);
            ApplyAngle(_raisedAngle);
        }

        private void OnEnable()
        {
            _eventBus?.Subscribe<SwitchPulledEvent>(OnSwitchPulled);
        }

        private void OnDisable()
        {
            _eventBus?.Unsubscribe<SwitchPulledEvent>(OnSwitchPulled);
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus?.Unsubscribe<SwitchPulledEvent>(OnSwitchPulled);
            _eventBus = eventBus;
            if (_eventBus != null && isActiveAndEnabled)
                _eventBus.Subscribe<SwitchPulledEvent>(OnSwitchPulled);
        }

        private void OnSwitchPulled(SwitchPulledEvent gameEvent)
        {
            if (gameEvent.SwitchId != _switchId)
                return;

            if (_lowering.Begin())
                StartCoroutine(LowerRoutine());
        }

        private IEnumerator LowerRoutine()
        {
            while (!_lowering.Advance(Time.deltaTime))
            {
                ApplyAngle(_lowering.CurrentAngle);
                yield return null;
            }

            ApplyAngle(_loweredAngle);

            if (_navMeshModifier != null)
                _navMeshModifier.ignoreFromBuild = false;

            var operations = new List<AsyncOperation>();
            foreach (var surface in _surfaces)
            {
                if (surface == null || surface.navMeshData == null)
                    continue;

                var data = Instantiate(surface.navMeshData);
                surface.RemoveData();
                surface.navMeshData = data;
                surface.AddData();
                operations.Add(surface.UpdateNavMesh(data));
            }

            foreach (var operation in operations)
            {
                while (operation != null && !operation.isDone)
                    yield return null;
            }

            IsLowered = true;
            _eventBus?.Publish(new BridgeLoweredEvent());
        }

        private void ApplyAngle(float angle)
        {
            _pivot.localRotation = Quaternion.Euler(angle, _baseEuler.y, _baseEuler.z);
        }
    }
}
