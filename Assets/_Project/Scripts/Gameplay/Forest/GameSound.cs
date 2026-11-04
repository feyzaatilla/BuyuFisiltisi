using System.Collections.Generic;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public enum SoundKind
    {
        Pickup,
        Brew,
        Fail,
        Vortex,
        StoneLit,
        StoneWrong,
        Solved,
        Checkpoint,
        Cloud,
        Bridge,
        Deed
    }

    public static class GameSound
    {
        private const int SampleRate = 22050;

        private static readonly Dictionary<SoundKind, AudioClip> Clips = new Dictionary<SoundKind, AudioClip>();
        private static AudioSource _source;
        private static System.Random _random = new System.Random(3);

        public static void Play(SoundKind kind, float volume = 0.6f)
        {
            if (_source == null)
            {
                var host = new GameObject("OyunSesleri") { hideFlags = HideFlags.HideAndDontSave };
                _source = host.AddComponent<AudioSource>();
                _source.playOnAwake = false;
                _source.spatialBlend = 0f;
            }

            if (!Clips.TryGetValue(kind, out var clip) || clip == null)
            {
                clip = Create(kind);
                Clips[kind] = clip;
            }

            _source.PlayOneShot(clip, volume);
        }

        private static AudioClip Create(SoundKind kind)
        {
            float[] data;
            switch (kind)
            {
                case SoundKind.Pickup:
                    data = Mix(0.4f, Tone(88f, 0f, 0.25f, 9f), Tone(95f, 0.08f, 0.3f, 8f));
                    break;
                case SoundKind.Brew:
                    data = Brew();
                    break;
                case SoundKind.Fail:
                    data = Mix(0.6f, Slide(50f, 38f, 0f, 0.45f, 0.6f), Noise(0f, 0.25f, 0.25f, 12f));
                    break;
                case SoundKind.Vortex:
                    data = Mix(1.4f, Slide(70f, 40f, 0f, 1.3f, 0.35f), Noise(0f, 1.3f, 0.35f, 1.5f));
                    break;
                case SoundKind.StoneLit:
                    data = Mix(1.2f, Tone(81f, 0f, 1.1f, 3f), Tone(93f, 0f, 0.8f, 4f, 0.3f));
                    break;
                case SoundKind.StoneWrong:
                    data = Mix(0.6f, Tone(55f, 0f, 0.25f, 7f), Tone(51f, 0.18f, 0.35f, 6f));
                    break;
                case SoundKind.Solved:
                    data = Mix(1.4f, Tone(72f, 0f, 0.9f, 3f), Tone(76f, 0.12f, 0.9f, 3f), Tone(79f, 0.24f, 0.9f, 3f), Tone(84f, 0.36f, 1f, 2.5f));
                    break;
                case SoundKind.Checkpoint:
                    data = Mix(0.9f, Tone(79f, 0f, 0.6f, 4f), Tone(86f, 0.1f, 0.7f, 4f));
                    break;
                case SoundKind.Cloud:
                    data = Mix(1f, Noise(0f, 0.6f, 0.5f, 5f), Tone(96f, 0.05f, 0.8f, 4f, 0.4f), Tone(100f, 0.15f, 0.8f, 4f, 0.3f));
                    break;
                case SoundKind.Bridge:
                    data = Mix(1.4f, Slide(40f, 36f, 0f, 1.3f, 0.7f), Noise(0f, 1.3f, 0.3f, 2f));
                    break;
                default:
                    data = Mix(1f, Tone(84f, 0f, 0.7f, 4f), Tone(88f, 0.15f, 0.7f, 4f), Tone(91f, 0.3f, 0.8f, 3.5f));
                    break;
            }

            Normalize(data);
            var clip = AudioClip.Create("Ses_" + kind, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float[] Brew()
        {
            var parts = new List<float[]>();
            for (var i = 0; i < 9; i++)
            {
                var start = i * 0.09f + (float)_random.NextDouble() * 0.04f;
                var note = 62f + _random.Next(14);
                parts.Add(Slide(note, note + 5f, start, 0.08f, 0.5f));
            }
            parts.Add(Slide(67f, 86f, 0.4f, 0.7f, 0.5f));
            return Mix(1.2f, parts.ToArray());
        }

        private static float[] Tone(float midi, float start, float length, float decay, float gain = 1f)
        {
            return Slide(midi, midi, start, length, gain, decay);
        }

        private static float[] Slide(float fromMidi, float toMidi, float start, float length, float gain, float decay = 0f)
        {
            var offset = Mathf.RoundToInt(start * SampleRate);
            var count = Mathf.RoundToInt(length * SampleRate);
            var data = new float[offset + count];
            var phase = 0f;
            for (var i = 0; i < count; i++)
            {
                var t = (float)i / SampleRate;
                var progress = (float)i / count;
                var frequency = Frequency(Mathf.Lerp(fromMidi, toMidi, progress));
                phase += 2f * Mathf.PI * frequency / SampleRate;
                var envelope = Mathf.Clamp01(t * 300f) * (decay > 0f ? Mathf.Exp(-t * decay) : Mathf.Sin(progress * Mathf.PI));
                data[offset + i] = Mathf.Sin(phase) * envelope * gain;
            }
            return data;
        }

        private static float[] Noise(float start, float length, float gain, float decay)
        {
            var offset = Mathf.RoundToInt(start * SampleRate);
            var count = Mathf.RoundToInt(length * SampleRate);
            var data = new float[offset + count];
            var smooth = 0f;
            for (var i = 0; i < count; i++)
            {
                var t = (float)i / SampleRate;
                smooth = Mathf.Lerp(smooth, (float)_random.NextDouble() * 2f - 1f, 0.15f);
                var envelope = Mathf.Clamp01(t * 20f) * Mathf.Exp(-t * decay);
                data[offset + i] = smooth * envelope * gain;
            }
            return data;
        }

        private static float[] Mix(float seconds, params float[][] parts)
        {
            var data = new float[Mathf.RoundToInt(seconds * SampleRate)];
            foreach (var part in parts)
                for (var i = 0; i < part.Length && i < data.Length; i++)
                    data[i] += part[i];
            return data;
        }

        private static void Normalize(float[] data)
        {
            var peak = 0.0001f;
            foreach (var sample in data)
                peak = Mathf.Max(peak, Mathf.Abs(sample));
            for (var i = 0; i < data.Length; i++)
                data[i] = data[i] / peak * 0.7f;
        }

        private static float Frequency(float midi)
        {
            return 440f * Mathf.Pow(2f, (midi - 69f) / 12f);
        }
    }
}
