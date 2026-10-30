using System.Collections;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.World
{
    public sealed class SealedDoor : MonoBehaviour, IRequiresEventBus
    {
        public const string DefaultId = "MuhurluKapi";

        [SerializeField] private string _doorId = DefaultId;
        [SerializeField] private Vector3 _openOffset = new Vector3(0f, -4.5f, 0f);
        [SerializeField] private float _openSeconds = 2.5f;

        private DoorLatch _latch;
        private IEventBus _eventBus;

        private DoorLatch Latch => _latch ??= new DoorLatch(_doorId);

        public bool IsOpen => Latch.IsOpen;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus?.Unsubscribe<DoorOpenRequestedEvent>(OnOpenRequested);
            _eventBus = eventBus;
            _eventBus?.Subscribe<DoorOpenRequestedEvent>(OnOpenRequested);
        }

        private void OnDestroy()
        {
            _eventBus?.Unsubscribe<DoorOpenRequestedEvent>(OnOpenRequested);
        }

        private void OnOpenRequested(DoorOpenRequestedEvent request)
        {
            if (!Latch.TryOpen(request.DoorId))
                return;

            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.enabled = false;

            StartCoroutine(Open());
        }

        private IEnumerator Open()
        {
            var start = transform.position;
            var end = start + _openOffset;
            var duration = Mathf.Max(0.01f, _openSeconds);
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            transform.position = end;
            Debug.Log("[Kapı] Mühürlü kapı açıldı.", this);
            _eventBus?.Publish(new DoorOpenedEvent(Latch.Id));
        }
    }
}
