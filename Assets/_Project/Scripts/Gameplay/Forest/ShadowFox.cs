using System.Collections.Generic;
using BuyuFisiltisi.Core.Creatures;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Core.Navigation;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ShadowFox : MonoBehaviour, IRequiresEventBus
    {
        public const string CalmedDeedId = "shadow_fox_calmed";

        [SerializeField] private Transform _player;
        [SerializeField] private PlayerInventory _inventory;
        [SerializeField] private Transform[] _patrolPoints = new Transform[0];
        [SerializeField] private Renderer[] _eyes = new Renderer[0];
        [SerializeField] private Material _calmEyes;
        [SerializeField] private Renderer[] _fur = new Renderer[0];
        [SerializeField] private Material _calmFur;
        [SerializeField] private Light _glow;
        [SerializeField] private float _patrolSpeed = 2f;
        [SerializeField] private float _chaseSpeed = 4.6f;
        [SerializeField] private float _fleeSpeed = 6f;
        [SerializeField] private float _viewRange = 9f;
        [SerializeField] private float _viewAngle = 70f;
        [SerializeField] private float _spellRange = 7f;
        [SerializeField] private float _petRadius = 1.8f;
        [SerializeField] private Vector2 _minBounds = new Vector2(-8.5f, 33f);
        [SerializeField] private Vector2 _maxBounds = new Vector2(8.5f, 58f);

        private readonly FoxBrain _brain = new FoxBrain();
        private readonly List<string> _stolen = new List<string>();
        private IEventBus _eventBus;
        private int _patrolIndex;
        private bool _warned;
        private float _bobTime;

        public FoxState State => _brain.State;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Update()
        {
            if (_player == null || Time.timeScale == 0f)
                return;

            var toPlayer = _player.position - transform.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;
            var sees = CanSeePlayer(toPlayer, distance);

            if (sees && !_warned && _brain.State == FoxState.Patrol)
            {
                _warned = true;
                GameMessage.Show("Fısıl: Gölge Tilki seni gördü! Koş (Shift) ya da sol tıkla, büyünle sersemlet!", new Color(1f, 0.6f, 0.6f), 3.5f);
            }

            HandleSpell(distance);

            if (_brain.Tick(Time.deltaTime, sees, distance))
                Steal();

            if (_brain.State == FoxState.Stunned && distance <= _petRadius && _brain.Calm())
                BecomeCalm();

            Move(toPlayer);
        }

        private bool CanSeePlayer(Vector3 toPlayer, float distance)
        {
            if (_brain.State == FoxState.Calm || _brain.State == FoxState.Stunned || distance > _viewRange)
                return false;

            if (_brain.State != FoxState.Chase && Vector3.Angle(transform.forward, toPlayer) > _viewAngle)
                return false;

            var eye = transform.position + Vector3.up * 0.5f;
            var target = _player.position + Vector3.up * 1f;
            return !Physics.Linecast(eye, target, 1 << GameLayers.Obstacle, QueryTriggerInteraction.Ignore);
        }

        private void HandleSpell(float distance)
        {
            if (distance > _spellRange || _brain.State == FoxState.Calm || _brain.State == FoxState.Stunned)
                return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            var cast = (mouse != null && mouse.leftButton.wasPressedThisFrame) || (keyboard != null && keyboard.rKey.wasPressedThisFrame);
            if (cast && _brain.Stun())
                GameMessage.Show("Büyün Gölge Tilki'yi sersemletti! Hemen yanına git ve onu sev.", new Color(0.75f, 0.9f, 1f), 3.5f);
        }

        private void Steal()
        {
            if (_inventory == null)
                return;

            foreach (var flower in ItemCatalog.Flowers)
            {
                if (!_inventory.Inventory.TryConsume(flower))
                    continue;

                _stolen.Add(flower);
                GameMessage.Show("Gölge Tilki " + ItemCatalog.GetName(flower) + " çiçeğini kaptı ve kaçtı! Onu sersemletip sevmeden geri alamazsın.", new Color(1f, 0.55f, 0.5f), 4f);
                return;
            }

            GameMessage.Show("Gölge Tilki üstüne atladı ama çalacak çiçeğin yoktu.", new Color(1f, 0.55f, 0.5f), 3f);
        }

        private void BecomeCalm()
        {
            foreach (var flower in _stolen)
                _inventory?.Inventory.Add(flower);

            var returned = _stolen.Count;
            _stolen.Clear();

            foreach (var eye in _eyes)
                if (eye != null && _calmEyes != null)
                    eye.sharedMaterial = _calmEyes;
            foreach (var fur in _fur)
                if (fur != null && _calmFur != null)
                    fur.sharedMaterial = _calmFur;
            if (_glow != null)
                _glow.color = new Color(0.55f, 0.75f, 1f);

            var text = returned > 0
                ? "Gölge Tilki sakinleşti ve çaldığı " + returned + " çiçeği geri verdi."
                : "Gölge Tilki sakinleşti, artık seni kovalamayacak.";
            GameMessage.Show(text, new Color(0.7f, 0.9f, 1f), 4f);
            _eventBus?.Publish(new DeedRecordedEvent(CalmedDeedId, "Gölge Tilki'ye zarar vermek yerine onu sakinleştirip sevdi."));
        }

        private void Move(Vector3 toPlayer)
        {
            var state = _brain.State;
            if (state == FoxState.Stunned || state == FoxState.Calm)
            {
                transform.Rotate(0f, state == FoxState.Stunned ? 220f * Time.deltaTime : 0f, 0f);
                return;
            }

            Vector3 direction;
            float speed;
            if (state == FoxState.Chase)
            {
                direction = toPlayer;
                speed = _chaseSpeed;
            }
            else if (state == FoxState.Flee)
            {
                direction = -toPlayer;
                speed = _fleeSpeed;
            }
            else
            {
                if (_patrolPoints.Length == 0)
                    return;

                var point = _patrolPoints[_patrolIndex % _patrolPoints.Length].position - transform.position;
                point.y = 0f;
                if (point.magnitude < 0.5f)
                    _patrolIndex++;
                direction = point;
                speed = _patrolSpeed;
            }

            if (direction.sqrMagnitude < 0.001f)
                return;

            direction.Normalize();
            var next = transform.position + direction * (speed * Time.deltaTime);
            next.x = Mathf.Clamp(next.x, _minBounds.x, _maxBounds.x);
            next.z = Mathf.Clamp(next.z, _minBounds.y, _maxBounds.y);
            _bobTime += Time.deltaTime * speed * 3f;
            next.y = Mathf.Abs(Mathf.Sin(_bobTime)) * 0.08f;
            transform.position = next;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(direction, Vector3.up), 540f * Time.deltaTime);
        }
    }
}
