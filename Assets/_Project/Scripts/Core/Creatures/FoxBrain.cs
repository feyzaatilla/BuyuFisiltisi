namespace BuyuFisiltisi.Core.Creatures
{
    public enum FoxState
    {
        Patrol,
        Chase,
        Flee,
        Stunned,
        Calm
    }

    public sealed class FoxBrain
    {
        private readonly float _catchDistance;
        private readonly float _loseSightSeconds;
        private readonly float _fleeSeconds;
        private readonly float _stunSeconds;
        private float _stateTime;
        private float _unseenTime;

        public FoxBrain(float catchDistance = 1.2f, float loseSightSeconds = 2.5f, float fleeSeconds = 4f, float stunSeconds = 4f)
        {
            _catchDistance = catchDistance;
            _loseSightSeconds = loseSightSeconds;
            _fleeSeconds = fleeSeconds;
            _stunSeconds = stunSeconds;
        }

        public FoxState State { get; private set; } = FoxState.Patrol;

        public bool Tick(float deltaTime, bool canSeePlayer, float distanceToPlayer)
        {
            _stateTime += deltaTime;
            _unseenTime = canSeePlayer ? 0f : _unseenTime + deltaTime;

            switch (State)
            {
                case FoxState.Patrol:
                    if (canSeePlayer)
                        Enter(FoxState.Chase);
                    break;
                case FoxState.Chase:
                    if (distanceToPlayer <= _catchDistance)
                    {
                        Enter(FoxState.Flee);
                        return true;
                    }
                    if (_unseenTime >= _loseSightSeconds)
                        Enter(FoxState.Patrol);
                    break;
                case FoxState.Flee:
                    if (_stateTime >= _fleeSeconds)
                        Enter(FoxState.Patrol);
                    break;
                case FoxState.Stunned:
                    if (_stateTime >= _stunSeconds)
                        Enter(FoxState.Patrol);
                    break;
            }

            return false;
        }

        public bool Stun()
        {
            if (State == FoxState.Calm)
                return false;

            Enter(FoxState.Stunned);
            return true;
        }

        public bool Calm()
        {
            if (State != FoxState.Stunned)
                return false;

            Enter(FoxState.Calm);
            return true;
        }

        private void Enter(FoxState state)
        {
            State = state;
            _stateTime = 0f;
        }
    }
}
