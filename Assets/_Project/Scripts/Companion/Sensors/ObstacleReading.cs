namespace BuyuFisiltisi.Companion.Sensors
{
    public readonly struct ObstacleReading
    {
        public static readonly ObstacleReading Clear = new ObstacleReading(false, false, false, float.PositiveInfinity);

        public ObstacleReading(bool left, bool center, bool right, float nearestDistance)
        {
            Left = left;
            Center = center;
            Right = right;
            NearestDistance = nearestDistance;
        }

        public bool Left { get; }
        public bool Center { get; }
        public bool Right { get; }
        public float NearestDistance { get; }
        public bool Any => Left || Center || Right;
    }
}
