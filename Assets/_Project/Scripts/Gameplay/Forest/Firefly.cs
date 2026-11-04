using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Interaction;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class Firefly : MonoBehaviour, IInteractable, IRequiresEventBus
    {
        public const string HealedDeedId = "firefly_healed";

        [SerializeField] private string _interactionId = "firefly";
        [SerializeField] private string _displayName = "Ateş Böceği";
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private Transform _focusPoint;
        [SerializeField] private Transform _player;
        [SerializeField] private float _touchRadius = 1.8f;
        [SerializeField] private Light _glow;

        private IEventBus _eventBus;
        private bool _healed;
        private bool _touching;
        private float _health;
        private float _baseIntensity = 7f;
        private float _baseRange = 5f;
        private Transform _body;
        private Vector3 _bodyPosition;
        private Quaternion _bodyRotation;
        private WingFlap[] _wings;
        private Renderer[] _glowParts;
        private MaterialPropertyBlock _block;

        public bool IsHealed => _healed;
        public string InteractionId => _interactionId;
        public string DisplayName => _displayName;
        public Transform InteractionPoint => _interactionPoint != null ? _interactionPoint : transform;
        public Transform FocusPoint => _focusPoint != null ? _focusPoint : transform;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Awake()
        {
            if (_glow != null)
            {
                var flicker = _glow.GetComponent<FlickerLight>();
                if (flicker != null)
                    flicker.enabled = false;
                _baseIntensity = _glow.intensity;
                _baseRange = _glow.range;
            }

            _body = transform.Find("Gorunum");
            if (_body != null)
            {
                _bodyPosition = _body.localPosition;
                _bodyRotation = _body.localRotation;
            }

            _wings = GetComponentsInChildren<WingFlap>();
            var parts = new System.Collections.Generic.List<Renderer>();
            foreach (var renderer in GetComponentsInChildren<Renderer>())
                if (renderer.name == "Kuyruk" || renderer.name == "Anten_Ucu")
                    parts.Add(renderer);
            _glowParts = parts.ToArray();
            _block = new MaterialPropertyBlock();
        }

        private void Update()
        {
            Animate();

            if (_player == null || _healed)
                return;

            var offset = _player.position - transform.position;
            offset.y = 0f;
            var touching = offset.magnitude <= _touchRadius;
            if (touching && !_touching)
            {
                if (_inventory != null && _inventory.Inventory.Count(Recipe.LightTeaId) > 0)
                    Interact(_player.gameObject);
                else
                    GameMessage.Show("Ateş böceği yaralı, ışığı sönmek üzere. Ona kazanda demlenmiş bir Işık Çayı getir.", 3.5f);
            }

            _touching = touching;
        }

        private void Animate()
        {
            _health = Mathf.MoveTowards(_health, _healed ? 1f : 0f, Time.deltaTime / 2.5f);
            var h = Mathf.SmoothStep(0f, 1f, _health);
            var t = Time.time;

            if (_glow != null)
            {
                var sputter = Mathf.PerlinNoise(t * 7f, 3.1f);
                var sick = _baseIntensity * 0.25f * (sputter > 0.4f ? sputter : 0.08f);
                var well = _baseIntensity * 2.2f * (0.85f + 0.3f * Mathf.PerlinNoise(t * 2.5f, 7.7f));
                _glow.intensity = Mathf.Lerp(sick, well, h);
                _glow.range = Mathf.Lerp(_baseRange * 0.6f, _baseRange * 1.6f, h);
            }

            if (_body != null)
            {
                var circle = t * 1.6f;
                var offset = new Vector3(
                    Mathf.Sin(circle) * 0.45f * h,
                    Mathf.Lerp(-0.25f, 0.7f, h) + Mathf.Sin(t * (1f + 2f * h)) * Mathf.Lerp(0.03f, 0.1f, h),
                    Mathf.Cos(circle) * 0.45f * h);
                _body.localPosition = _bodyPosition + offset;
                var turn = Quaternion.Euler(0f, h * (circle * Mathf.Rad2Deg + 90f), (1f - h) * 28f);
                _body.localRotation = Quaternion.Slerp(_bodyRotation * Quaternion.Euler(0f, 0f, 28f), turn * _bodyRotation, h);
            }

            if (_wings != null)
                foreach (var wing in _wings)
                    if (wing != null)
                        wing.enabled = h > 0.3f;

            if (_glowParts != null)
            {
                var color = Color.Lerp(new Color(0.3f, 0.26f, 0.18f), new Color(1f, 0.9f, 0.35f) * 1.6f, h);
                _block.SetColor("_BaseColor", color);
                _block.SetColor("_Color", color);
                foreach (var part in _glowParts)
                    if (part != null)
                        part.SetPropertyBlock(_block);
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            return !_healed && _inventory != null;
        }

        public void Interact(GameObject interactor)
        {
            if (!CanInteract(interactor))
                return;

            if (!_inventory.Inventory.TryConsume(Recipe.LightTeaId))
            {
                Debug.Log("[Ateş Böceği] Işık Çayı yok, iyileştiremedim.");
                return;
            }

            _healed = true;
            GameSound.Play(SoundKind.Solved, 0.7f);
            GameMessage.Show("Işık Çayını ateş böceğine içirdin. Kanatları yeniden ışıl ışıl parlıyor!", new Color(1f, 0.9f, 0.45f), 3.5f);
            _eventBus?.Publish(new DeedRecordedEvent(HealedDeedId, "Yaralı ateş böceği Işık Çayı ile iyileştirildi."));
            Debug.Log("[Ateş Böceği] İyileşti.");
        }
    }
}