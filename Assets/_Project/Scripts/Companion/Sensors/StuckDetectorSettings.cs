using System;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    [Serializable]
    public sealed class StuckDetectorSettings
    {
        [Min(0f)] public float SpeedThreshold = 0.05f;
        [Min(0f)] public float TimeThreshold = 1.5f;
    }
}
