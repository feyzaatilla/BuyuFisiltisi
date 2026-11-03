using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class ForestMusic : MonoBehaviour
    {
        private const int SampleRate = 22050;

        [SerializeField] private float _volume = 0.22f;
        [SerializeField] private float _chordSeconds = 6f;
        [SerializeField] private int _seed = 7;

        private static readonly float[][] Chords =
        {
            new[] { 57f, 60f, 64f, 69f },
            new[] { 53f, 57f, 60f, 65f },
            new[] { 48f, 55f, 60f, 64f },
            new[] { 55f, 59f, 62f, 67f }
        };

        private static readonly float[] Bells = { 69f, 72f, 74f, 76f, 79f, 81f, 84f };

        private void Start()
        {
            var source = GetComponent<AudioSource>();
            source.clip = CreateClip();
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = _volume;
            source.playOnAwake = false;
            source.Play();
        }

        private AudioClip CreateClip()
        {
            var chordSamples = Mathf.RoundToInt(_chordSeconds * SampleRate);
            var total = chordSamples * Chords.Length;
            var data = new float[total];
            var random = new System.Random(_seed);

            for (var c = 0; c < Chords.Length; c++)
            {
                var start = c * chordSamples;
                foreach (var note in Chords[c])
                {
                    var frequency = Frequency(note);
                    for (var i = 0; i < chordSamples; i++)
                    {
                        var t = (float)i / SampleRate;
                        var fade = Mathf.Clamp01(t / 1.5f) * Mathf.Clamp01((_chordSeconds - t) / 1.5f);
                        var wave = Mathf.Sin(2f * Mathf.PI * frequency * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t);
                        data[start + i] += wave * fade * 0.06f;
                    }
                }

                var bellCount = 3 + random.Next(3);
                for (var b = 0; b < bellCount; b++)
                {
                    var frequency = Frequency(Bells[random.Next(Bells.Length)]);
                    var offset = start + random.Next(chordSamples - SampleRate * 2);
                    var length = SampleRate * 2;
                    for (var i = 0; i < length && offset + i < total; i++)
                    {
                        var t = (float)i / SampleRate;
                        var envelope = Mathf.Exp(-t * 2.5f) * Mathf.Clamp01(t * 200f);
                        data[offset + i] += Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.08f;
                    }
                }
            }

            var peak = 0.0001f;
            for (var i = 0; i < total; i++)
                peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            for (var i = 0; i < total; i++)
                data[i] = data[i] / peak * 0.8f;

            var clip = AudioClip.Create("OrmanMuzigi", total, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float Frequency(float midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69f) / 12f);
        }
    }
}
