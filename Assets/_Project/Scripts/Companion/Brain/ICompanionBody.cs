using UnityEngine;

namespace BuyuFisiltisi.Companion.Brain
{
    public interface ICompanionBody
    {
        GameObject Self { get; }
        bool HasArrived { get; }
        bool IsStuck { get; }
        void FollowPlayer();
        bool MoveTo(Vector3 destination);
        void Stop();
    }
}
