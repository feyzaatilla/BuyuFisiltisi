using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Gameplay.Player;
using BuyuFisiltisi.Gameplay.World;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class RootParkour : MonoBehaviour, IRequiresEventBus
    {
        public const string CompletedDeedId = "root_parkour_completed";

        [SerializeField] private Transform _player;
        [SerializeField] private Transform[] _checkpoints = new Transform[0];
        [SerializeField] private Transform _exit;
        [SerializeField] private Transform _surfacePoint;
        [SerializeField] private float _fallDepth = 7f;
        [SerializeField] private float _reachRadius = 1.6f;
        [SerializeField] private RunePuzzle _puzzle;
        [SerializeField] private float _jumpBoost = 1.8f;

        private IEventBus _eventBus;
        private int _current;
        private bool _completed;
        private WitchMotor _motor;

        public void InjectEventBus(IEventBus eventBus)
        {
            _eventBus = eventBus;
        }

        private void Update()
        {
            UpdateJumpBoost();
            if (_player == null || _checkpoints.Length == 0 || _checkpoints[0] == null)
                return;

            var inside = Mathf.Abs(_player.position.x - transform.position.x) < 30f && Mathf.Abs(_player.position.z - transform.position.z) < 60f && _player.position.y < transform.position.y + 15f;
            if (!inside)
                return;

            for (var i = _current + 1; i < _checkpoints.Length; i++)
            {
                if (_checkpoints[i] != null && Near(_checkpoints[i].position, 2f))
                {
                    _current = i;
                    GameSound.Play(SoundKind.Checkpoint);
                    Show("Kök ışığı parladı. Düşersen buradan devam edeceksin.");
                }
            }

            if (_player.position.y < transform.position.y - _fallDepth)
            {
                Teleport(_checkpoints[_current]);
                GameSound.Play(SoundKind.Fail);
                ScreenFlash.Play();
                Show("Köklerin arasına düştün! Bir daha dene.");
                return;
            }

            if (_exit != null && _surfacePoint != null && (_puzzle == null || _puzzle.IsSolved) && Near(_exit.position, _reachRadius))
            {
                Teleport(_surfacePoint);
                _current = 0;
                if (!_completed)
                {
                    _completed = true;
                    _eventBus?.Publish(new DeedRecordedEvent(CompletedDeedId, "Gizli girdabın altındaki tuzaklı mağarayı geçip taş bulmacasını çözdü."));
                }
                Show("Yeryüzüne döndün! Fısıl seni bekliyordu. Şimdi Yaşlı Meşe'ye git.");
            }
        }

        private void UpdateJumpBoost()
        {
            if (_player == null)
                return;
            if (_motor == null)
                _motor = _player.GetComponentInParent<WitchMotor>();
            if (_motor == null)
                return;

            var local = _player.position - transform.position;
            var inside = Mathf.Abs(local.x) < 30f && Mathf.Abs(local.z) < 60f && local.y < 15f;
            _motor.JumpMultiplier = inside ? _jumpBoost : 1f;
        }

        public void Fail(string message)
        {
            if (_player == null || _checkpoints.Length == 0)
                return;

            Teleport(_checkpoints[_current]);
            GameSound.Play(SoundKind.Fail);
            ScreenFlash.Play();
            Show(message);
        }

        private bool Near(Vector3 point, float radius)
        {
            return (_player.position - point).sqrMagnitude <= radius * radius;
        }

        private void Teleport(Transform point)
        {
            var respawnable = _player.GetComponentInParent<IRespawnable>();
            respawnable?.Respawn(point.position, point.rotation);
        }

        private static void Show(string message)
        {
            GameMessage.Show(message, new Color(0.65f, 1f, 0.75f), 3f);
        }
    }
}
