using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Player
{
    public interface IPlayerInput
    {
        Vector2 Move { get; }
        bool Sprint { get; }
        bool JumpPressed { get; }
        Vector2 Look { get; }
        bool IsLooking { get; }
        float Zoom { get; }
    }
}
