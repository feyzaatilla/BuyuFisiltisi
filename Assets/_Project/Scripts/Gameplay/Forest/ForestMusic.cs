using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class ForestMusic : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private const float ChordSeconds = 2.8f;

        [SerializeField] private float _volume = 0.22f;

        private static readonly float[][] Chords =
        {
            new[] { 57f, 60f, 64f, 69f },
            new[] { 53f, 57f, 60f, 65f },
            new[] { 48f, 55f, 60f, 64f },
            new[] { 55f, 59f, 62f, 67f }
        };

        private static readonly float[][] Melody =
        {
            new[] { 69f, 72f, 76f, 74f, 72f, 71f, 69f, 64f },
            new[] { 69f, 72f, 77f, 76f, 74f, 72f, 69f, 72f },
            new[] { 67f, 72f, 76f, 79f, 77f, 76f, 72f, 74f },
            new[] { 74f, 71f, 67f, 71f, 74f, 76f, 79f, 76f }
        };

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
            var chordSamples = Mathf.RoundToInt(ChordSeconds * SampleRate);
            var total = chordSamples * Chords.Length;
            var data = new float[total];
            var random = new System.Random(7);

            for (var c = 0; c < Chords.Length; c++)
            {
                var start = c * chordSamples;
                foreach (var note in Chords[c])
                {
                    var frequency = Frequency(note);
                    for (var i = 0; i < chordSamples; i++)
                    {
                        var t = (float)i / SampleRate;
                        var fade = Mathf.Clamp01(t / 0.6f) * Mathf.Clamp01((ChordSeconds - t) / 0.6f);
                        var wave = Mathf.Sin(2f * Mathf.PI * frequency * t) + 0.3f * Mathf.Sin(2f * Mathf.PI * frequency * 2f * t);
                        data[start + i] += wave * fade * 0.035f;
                    }
                }

                var beat = ChordSeconds / 12f;
                for (var n = 0; n < 8; n++)
                {
                    var phrase = n / 4;
                    var step = n % 4;
                    var longNote = step == 3;
                    var offset = start + Mathf.RoundToInt((phrase * 6 + step) * beat * SampleRate);
                    var seconds = longNote ? beat * 2.8f : beat * 0.9f;
                    Pluck(data, offset, seconds, Frequency(Melody[c][n]), longNote ? 1.2f : 6f, longNote ? 0.12f : 0.1f);
                    Sax(data, offset, longNote ? beat * 3f : beat * 0.95f, Frequency(Melody[c][n] - 12f), random, 0.11f);
                }

                for (var b = 0; b < 12; b++)
                {
                    var offset = start + Mathf.RoundToInt(b * beat * SampleRate);
                    var bassNote = Chords[c][0] - 12f + (b % 3 == 2 ? 7f : 0f);
                    Pluck(data, offset, beat * 0.8f, Frequency(bassNote), 9f, 0.11f);
                    if (b % 3 == 0)
                        Kick(data, offset);
                    if (b % 6 == 3)
                        Snare(data, offset, random);
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

        private static void Sax(float[] data, int offset, float seconds, float frequency, System.Random random, float gain)
        {
            var length = Mathf.RoundToInt(seconds * SampleRate);
            var phase = 0f;
            var smooth = 0f;
            for (var i = 0; i < length && offset + i < data.Length; i++)
            {
                var t = (float)i / SampleRate;
                var vibrato = 1f + 0.012f * Mathf.Sin(2f * Mathf.PI * 5.5f * t) * Mathf.Clamp01((t - 0.12f) * 6f);
                phase += frequency * vibrato / SampleRate;
                var wave = 0f;
                for (var k = 1; k <= 8; k++)
                    wave += Mathf.Sin(2f * Mathf.PI * phase * k) * (k == 1 ? 1f : (k % 2 == 0 ? 0.7f : 0.5f) / k * 2f);
                smooth = Mathf.Lerp(smooth, wave, 0.35f);
                var breath = ((float)random.NextDouble() * 2f - 1f) * 0.08f;
                var envelope = Mathf.Clamp01(t * 25f) * Mathf.Clamp01((seconds - t) * 12f) * (0.85f + 0.15f * Mathf.Exp(-t * 6f));
                data[offset + i] += (smooth + breath) * envelope * gain;
            }
        }

        private static void Kick(float[] data, int offset)
        {
            var length = Mathf.RoundToInt(0.25f * SampleRate);
            var phase = 0f;
            for (var i = 0; i < length && offset + i < data.Length; i++)
            {
                var t = (float)i / SampleRate;
                phase += Mathf.Lerp(120f, 45f, Mathf.Clamp01(t * 8f)) / SampleRate;
                data[offset + i] += Mathf.Sin(2f * Mathf.PI * phase) * Mathf.Exp(-t * 14f) * 0.3f;
            }
        }

        private static void Snare(float[] data, int offset, System.Random random)
        {
            var length = Mathf.RoundToInt(0.18f * SampleRate);
            var previous = 0f;
            for (var i = 0; i < length && offset + i < data.Length; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)random.NextDouble() * 2f - 1f;
                var high = noise - previous;
                previous = noise;
                data[offset + i] += (high * 0.5f + Mathf.Sin(2f * Mathf.PI * 190f * t) * 0.4f) * Mathf.Exp(-t * 22f) * 0.18f;
            }
        }

        private static void Pluck(float[] data, int offset, float seconds, float frequency, float decay, float gain)
        {
            var length = Mathf.RoundToInt(seconds * SampleRate);
            for (var i = 0; i < length && offset + i < data.Length; i++)
            {
                var t = (float)i / SampleRate;
                var envelope = Mathf.Exp(-t * decay) * Mathf.Clamp01(t * 300f) * Mathf.Clamp01((seconds - t) * 20f);
                var wave = Mathf.Sin(2f * Mathf.PI * frequency * t)
                    + 0.45f * Mathf.Sin(4f * Mathf.PI * frequency * t) * Mathf.Exp(-t * 4f)
                    + 0.2f * Mathf.Sin(6f * Mathf.PI * frequency * t) * Mathf.Exp(-t * 8f);
                data[offset + i] += wave * envelope * gain;
            }
        }

        private static float Frequency(float midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69f) / 12f);
        }
    }
}
