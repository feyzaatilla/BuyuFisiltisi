namespace BuyuFisiltisi.Gameplay.World
{
    public sealed class SwitchLatch
    {
        public SwitchLatch(string id)
        {
            Id = id;
        }

        public string Id { get; }
        public bool IsPulled { get; private set; }

        public bool TryPull()
        {
            if (IsPulled)
                return false;

            IsPulled = true;
            return true;
        }

        public void Reset()
        {
            IsPulled = false;
        }
    }
}
