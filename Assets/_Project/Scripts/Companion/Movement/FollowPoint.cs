using UnityEngine;

namespace BuyuFisiltisi.Companion.Movement
{
    public static class FollowPoint
    {
        public static Vector3 Calculate(Vector3 leader, Vector3 follower, float distance)
        {
            if (IsWithin(leader, follower, distance))
                return follower;

            var offset = follower - leader;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.0001f)
                offset = Vector3.back;

            return leader + offset.normalized * distance;
        }

        public static bool IsWithin(Vector3 a, Vector3 b, float distance)
        {
            var delta = a - b;
            delta.y = 0f;
            return delta.sqrMagnitude <= distance * distance;
        }
    }
}
