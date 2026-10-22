using System;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    [Serializable]
    public sealed class ObstacleSensorSettings
    {
        [Min(0.1f)] public float Range = 1.5f;
        [Range(5f, 80f)] public float SideAngle = 35f;
        [Min(0f)] public float Height = 0.3f;
    }
}
