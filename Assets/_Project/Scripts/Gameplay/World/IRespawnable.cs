using UnityEngine;

namespace BuyuFisiltisi.Gameplay.World
{
    public interface IRespawnable
    {
        void Respawn(Vector3 position, Quaternion rotation);
    }
}
