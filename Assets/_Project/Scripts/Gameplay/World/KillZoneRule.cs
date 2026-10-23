namespace BuyuFisiltisi.Gameplay.World
{
    public static class KillZoneRule
    {
        public static bool Affects(int layerMask, int layer)
        {
            if (layer < 0 || layer > 31)
                return false;

            return (layerMask & (1 << layer)) != 0;
        }
    }
}
