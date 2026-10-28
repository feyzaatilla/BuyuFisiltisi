using System;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakTrust
    {
        public const int DefaultMin = -5;
        public const int DefaultMax = 5;
        public const int DefaultDoorThreshold = 3;
        public const int DefaultAttackThreshold = -3;

        public OakTrust(int start = 0, int min = DefaultMin, int max = DefaultMax,
            int doorThreshold = DefaultDoorThreshold, int attackThreshold = DefaultAttackThreshold)
        {
            if (min > max)
                throw new ArgumentException("En düşük güven en yüksekten büyük olamaz.", nameof(min));
            if (doorThreshold < min || doorThreshold > max)
                throw new ArgumentOutOfRangeException(nameof(doorThreshold));
            if (attackThreshold < min || attackThreshold > max)
                throw new ArgumentOutOfRangeException(nameof(attackThreshold));

            Min = min;
            Max = max;
            DoorThreshold = doorThreshold;
            AttackThreshold = attackThreshold;
            Current = Clamp(start);
        }

        public int Current { get; private set; }
        public int Min { get; }
        public int Max { get; }
        public int DoorThreshold { get; }
        public int AttackThreshold { get; }

        public bool CanOpenDoor => Current >= DoorThreshold;
        public bool MayAttack => Current <= AttackThreshold;

        public int Apply(int delta)
        {
            var previous = Current;
            Current = Clamp(Current + delta);
            return Current - previous;
        }

        private int Clamp(int value)
        {
            return value < Min ? Min : value > Max ? Max : value;
        }
    }
}
