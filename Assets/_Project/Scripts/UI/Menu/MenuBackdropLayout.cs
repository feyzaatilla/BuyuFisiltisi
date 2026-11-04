using System.Collections.Generic;
using UnityEngine;

namespace BuyuFisiltisi.UI.Menu
{
    public static class MenuBackdropLayout
    {
        public const float CorridorHalfWidth = 3.5f;
        public static readonly Vector3 OakPosition = new Vector3(0f, 0f, 14f);
        public static readonly Vector3 CameraPosition = new Vector3(0f, 2.2f, -4f);

        public static List<Vector3> TreePositions(int seed, int count, float minRadius, float maxRadius)
        {
            var random = new System.Random(seed);
            var result = new List<Vector3>();
            var attempts = 0;
            while (result.Count < count && attempts < count * 50)
            {
                attempts++;
                var angle = (float)(random.NextDouble() * Mathf.PI);
                var radius = minRadius + (float)random.NextDouble() * (maxRadius - minRadius);
                var position = new Vector3(Mathf.Cos(angle) * radius, 0f, OakPosition.z - 6f + Mathf.Sin(angle) * radius);
                if (InCorridor(position) || TooClose(result, position, 2.2f))
                    continue;

                result.Add(position);
            }

            return result;
        }

        public static bool InCorridor(Vector3 position)
        {
            return Mathf.Abs(position.x) < CorridorHalfWidth && position.z < OakPosition.z + 3f;
        }

        public static Vector3 CameraOffset(float time, float sway, float speed)
        {
            var t = time * speed;
            return new Vector3(Mathf.Sin(t) * sway, Mathf.Sin(t * 0.7f) * sway * 0.25f, Mathf.Sin(t * 0.5f) * sway * 0.5f);
        }

        private static bool TooClose(List<Vector3> placed, Vector3 position, float distance)
        {
            foreach (var other in placed)
            {
                if ((other - position).sqrMagnitude < distance * distance)
                    return true;
            }

            return false;
        }
    }
}
