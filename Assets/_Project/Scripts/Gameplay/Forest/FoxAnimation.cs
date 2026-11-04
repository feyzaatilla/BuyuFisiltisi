using System;
using BuyuFisiltisi.Core.Creatures;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public static class FoxAnimation
    {
        public const string Idle = "Idle";
        public const string Walk = "Walk";
        public const string Gallop = "Gallop";
        public const string HitReact = "Idle_HitReact_Left";
        public const string Rest = "Idle_2";

        public static readonly Color ShadowTint = new Color(0.32f, 0.2f, 0.45f, 1f);

        private static readonly string[] LoopingClips = { Idle, Walk, Gallop, Rest, "Idle_2_HeadLow", "Eating" };

        public static string StateFor(FoxState state)
        {
            switch (state)
            {
                case FoxState.Patrol:
                    return Walk;
                case FoxState.Chase:
                case FoxState.Flee:
                    return Gallop;
                case FoxState.Stunned:
                    return HitReact;
                case FoxState.Calm:
                    return Rest;
                default:
                    return Idle;
            }
        }

        public static string ClipName(string importedName)
        {
            if (string.IsNullOrEmpty(importedName))
                return string.Empty;

            var index = importedName.LastIndexOf('|');
            return index >= 0 ? importedName.Substring(index + 1) : importedName;
        }

        public static bool Loops(string clipName)
        {
            return Array.IndexOf(LoopingClips, clipName) >= 0;
        }

        public static Color Tint(Color original, float calm)
        {
            var shadow = new Color(original.r * ShadowTint.r, original.g * ShadowTint.g, original.b * ShadowTint.b, original.a);
            return Color.Lerp(shadow, original, Mathf.Clamp01(calm));
        }
    }
}
