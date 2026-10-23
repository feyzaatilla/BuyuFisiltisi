namespace BuyuFisiltisi.Companion.Sensors
{
    public readonly struct TargetReading
    {
        public static readonly TargetReading None = new TargetReading(false, float.PositiveInfinity, 0f, false);

        public TargetReading(bool hasTarget, float distance, float angle, bool arrived)
        {
            HasTarget = hasTarget;
            Distance = distance;
            Angle = angle;
            Arrived = arrived;
        }

        public bool HasTarget { get; }
        public float Distance { get; }
        public float Angle { get; }
        public bool Arrived { get; }
    }
}
