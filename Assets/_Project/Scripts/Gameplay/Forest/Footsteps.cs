using BuyuFisiltisi.Gameplay.Player;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class Footsteps : MonoBehaviour
    {
        private const int SampleRate = 22050;

        [SerializeField] private float _stepDistance = 1.1f;
        [SerializeField] private float _volume = 0.35f;

        private WitchMotor _motor;
        private AudioSource _source;
        private AudioClip[] _clips;
        private Vector3 _lastPosition;
        private float _distance;
        private float _airTime;
        private System.Random _random = new System.Random(11);

        private void Start()
        {
            _motor = GetComponent<WitchMotor>();
            _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _clips = new AudioClip[4];
            for (var i = 0; i < _clips.Length; i++)
                _clips[i] = CreateStep(i);
            _lastPosition = transform.position;
        }

        private void Update()
        {
            if (_motor == null || Time.timeScale == 0f)
                return;

            var position = transform.position;
            var moved = position - _lastPosition;
            moved.y = 0f;
            _lastPosition = position;

            if (!_motor.IsGrounded)
            {
                _airTime += Time.deltaTime;
                return;
            }

            if (_airTime > 0.35f)
            {
                Step(1.6f, 0.85f);
                _distance = 0f;
            }
            _airTime = 0f;

            if (moved.magnitude > 2f)
            {
                _distance = 0f;
                return;
            }

            _distance += moved.magnitude;
            if (_distance >= _stepDistance)
            {
                _distance = 0f;
                Step(1f, 0.9f + (float)_random.NextDouble() * 0.2f);
            }
        }

        private void Step(float loudness, float pitch)
        {
            _source.pitch = pitch;
            _source.PlayOneShot(_clips[_random.Next(_clips.Length)], _volume * loudness);
        }

        private AudioClip CreateStep(int variant)
        {
            var length = Mathf.RoundToInt(0.12f * SampleRate);
            var data = new float[length];
            var low = 0f;
            var smoothing = 0.12f + variant * 0.04f;
            for (var i = 0; i < length; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)_random.NextDouble() * 2f - 1f;
                low = Mathf.Lerp(low, noise, smoothing);
                var crunch = low * Mathf.Exp(-t * 35f) * 1.6f;
                var thump = Mathf.Sin(2f * Mathf.PI * (85f + variant * 8f) * t) * Mathf.Exp(-t * 45f);
                data[i] = (crunch + thump * 0.8f) * Mathf.Clamp01(t * 800f);
            }

            var clip = AudioClip.Create("Adim_" + variant, length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
