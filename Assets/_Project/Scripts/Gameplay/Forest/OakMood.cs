using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public static class OakMood
    {
        public const int MinTrust = -5;
        public const int MaxTrust = 5;

        public static readonly Color WitheredLeaves = new Color(0.55f, 0.38f, 0.2f);
        public static readonly Color BloomingLeaves = new Color(0.45f, 1f, 0.35f);
        public static readonly Color ColdLight = new Color(0.45f, 0.55f, 1f);
        public static readonly Color WarmLight = new Color(1f, 0.95f, 0.55f);

        public static float Mood(int trust)
        {
            var clamped = Mathf.Clamp(trust, MinTrust, MaxTrust);
            return clamped >= 0 ? (float)clamped / MaxTrust : -(float)clamped / MinTrust;
        }

        public static Color Leaves(Color original, int trust)
        {
            return Blend(original, WitheredLeaves, BloomingLeaves, Mood(trust));
        }

        public static Color Light(Color original, int trust)
        {
            return Blend(original, ColdLight, WarmLight, Mood(trust));
        }

        public static Color Blend(Color original, Color low, Color high, float mood)
        {
            return mood >= 0f ? Color.Lerp(original, high, mood) : Color.Lerp(original, low, -mood);
        }
    }
}
