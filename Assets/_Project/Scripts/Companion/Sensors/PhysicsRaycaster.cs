using BuyuFisiltisi.Core.Navigation;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    public sealed class PhysicsRaycaster : IRaycaster
    {
        private readonly int _layerMask;

        public PhysicsRaycaster() : this(GameLayers.ObstacleSensorMask)
        {
        }

        public PhysicsRaycaster(int layerMask)
        {
            _layerMask = layerMask;
        }

        public bool Cast(Vector3 origin, Vector3 direction, float maxDistance, out float hitDistance)
        {
            if (Physics.Raycast(origin, direction, out var hit, maxDistance, _layerMask, QueryTriggerInteraction.Ignore))
            {
                hitDistance = hit.distance;
                return true;
            }

            hitDistance = float.PositiveInfinity;
            return false;
        }
    }
}
