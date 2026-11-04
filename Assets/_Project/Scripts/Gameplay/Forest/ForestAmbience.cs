using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class ForestAmbience : MonoBehaviour
    {
        private const int SampleRate = 22050;
        private const float LoopSeconds = 24f;

        [SerializeField] private float _volume = 0.3f;

        private void Start()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = CreateClip();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = _volume;
            source.Play();
        }

        private static AudioClip CreateClip()
        {
            var total = Mathf.RoundToInt(LoopSeconds * SampleRate);
            var data = new float[total];
            var random = new System.Random(21);

            var low = 0f;
            var lower = 0f;
            for (var i = 0; i < total; i++)
            {
                var t = (float)i / SampleRate;
                var noise = (float)random.NextDouble() * 2f - 1f;
                low = Mathf.Lerp(low, noise, 0.04f);
                lower = Mathf.Lerp(lower, low, 0.08f);
                var swell = 0.55f + 0.3f * Mathf.Sin(2f * Mathf.PI * t / LoopSeconds) + 0.15f * Mathf.Sin(2f * Mathf.PI * t / (LoopSeconds / 3f));
                data[i] += lower * swell * 3.5f;
            }

            for (var group = 0; group < 14; group++)
            {
                var start = (float)random.NextDouble() * (LoopSeconds - 1f);
                var carrier = 4000f + (float)random.NextDouble() * 600f;
                for (var pulse = 0; pulse < 3; pulse++)
                {
                    var offset = Mathf.RoundToInt((start + pulse * 0.07f) * SampleRate);
                    var length = Mathf.RoundToInt(0.045f * SampleRate);
                    for (var i = 0; i < length && offset + i < total; i++)
                    {
                        var t = (float)i / SampleRate;
                        var envelope = Mathf.Sin(Mathf.PI * i / length);
                        data[offset + i] += Mathf.Sin(2f * Mathf.PI * carrier * t) * envelope * 0.05f;
                    }
                }
            }

            for (var bird = 0; bird < 7; bird++)
            {
                var start = (float)random.NextDouble() * (LoopSeconds - 2f);
                var from = 1800f + (float)random.NextDouble() * 900f;
                var to = from * (1.3f + (float)random.NextDouble() * 0.5f);
                var repeats = 2 + random.Next(3);
                for (var r = 0; r < repeats; r++)
                {
                    var offset = Mathf.RoundToInt((start + r * 0.16f) * SampleRate);
                    var length = Mathf.RoundToInt(0.11f * SampleRate);
                    var phase = 0f;
                    for (var i = 0; i < length && offset + i < total; i++)
                    {
                        var progress = (float)i / length;
                        var frequency = Mathf.Lerp(from, to, progress) + Mathf.Sin(progress * 40f) * 120f;
                        phase += frequency / SampleRate;
                        var envelope = Mathf.Sin(Mathf.PI * progress);
                        data[offset + i] += Mathf.Sin(2f * Mathf.PI * phase) * envelope * 0.12f;
                    }
                }
            }

            var peak = 0.0001f;
            for (var i = 0; i < total; i++)
                peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            var fade = SampleRate / 2;
            for (var i = 0; i < total; i++)
            {
                var edge = Mathf.Clamp01(Mathf.Min(i, total - 1 - i) / (float)fade);
                data[i] = data[i] / peak * 0.8f * Mathf.Lerp(0.85f, 1f, edge);
            }

            var clip = AudioClip.Create("OrmanSesleri", total, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
