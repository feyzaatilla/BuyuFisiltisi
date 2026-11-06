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

        private void Start()
        {
            PaintPlatforms();
        }

        private void PaintPlatforms()
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            var moss = shader != null ? new Material(shader) : null;
            if (moss != null)
                moss.color = new Color(0.36f, 0.62f, 0.42f);

            var block = new MaterialPropertyBlock();
            foreach (Transform child in transform)
            {
                if (child.gameObject.layer != BuyuFisiltisi.Core.Navigation.GameLayers.Ground || child.GetComponent<BoxCollider>() == null)
                    continue;

                var renderer = child.GetComponent<MeshRenderer>();
                if (renderer == null)
                    continue;

                var isWood = child.name.StartsWith("Kayan") || child.name.StartsWith("Kok_");
                renderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", isWood ? new Color(0.62f, 0.4f, 0.22f) : new Color(0.46f, 0.4f, 0.34f));
                renderer.SetPropertyBlock(block);

                if (isWood || moss == null)
                    continue;

                var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                top.name = "Yosun_Ust";
                Destroy(top.GetComponent<Collider>());
                top.transform.SetParent(child, false);
                var scale = child.lossyScale;
                top.transform.localPosition = new Vector3(0f, 0.5f + 0.01f / Mathf.Max(scale.y, 0.01f), 0f);
                top.transform.localScale = new Vector3(1f - 0.3f / Mathf.Max(scale.x, 0.01f), 0.02f / Mathf.Max(scale.y, 0.01f), 1f - 0.3f / Mathf.Max(scale.z, 0.01f));
                var topRenderer = top.GetComponent<MeshRenderer>();
                topRenderer.sharedMaterial = moss;
                topRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
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
                GameSound.Play(SoundKind.Ouch, 0.9f);
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
            GameSound.Play(SoundKind.Ouch, 0.9f);
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
