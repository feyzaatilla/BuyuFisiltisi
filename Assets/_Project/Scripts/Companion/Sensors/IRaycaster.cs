using UnityEngine;

namespace BuyuFisiltisi.Companion.Sensors
{
    public interface IRaycaster
    {
        bool Cast(Vector3 origin, Vector3 direction, float maxDistance, out float hitDistance);
    }
}
