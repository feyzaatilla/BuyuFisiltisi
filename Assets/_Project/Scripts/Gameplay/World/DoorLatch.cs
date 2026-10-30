using System;

namespace BuyuFisiltisi.Gameplay.World
{
    public sealed class DoorLatch
    {
        public DoorLatch(string id)
        {
            Id = id;
        }

        public string Id { get; }
        public bool IsOpen { get; private set; }

        public bool Matches(string id)
        {
            return id != null && string.Equals(Id, id, StringComparison.Ordinal);
        }

        public bool TryOpen(string id)
        {
            if (IsOpen || !Matches(id))
                return false;

            IsOpen = true;
            return true;
        }
    }
}
