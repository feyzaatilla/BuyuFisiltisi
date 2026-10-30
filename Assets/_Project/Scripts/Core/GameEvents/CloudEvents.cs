namespace BuyuFisiltisi.Core.GameEvents
{
    public readonly struct CloudDispersedEvent
    {
        public readonly int Dispersed;
        public readonly int Total;

        public CloudDispersedEvent(int dispersed, int total)
        {
            Dispersed = dispersed;
            Total = total;
        }
    }
}
