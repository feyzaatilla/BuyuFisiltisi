namespace BuyuFisiltisi.Companion.Sensors
{
    public interface ISensor
    {
        void Sense(float deltaTime);
        void Reset();
    }

    public interface ISensor<out TReading> : ISensor
    {
        TReading Current { get; }
    }
}
