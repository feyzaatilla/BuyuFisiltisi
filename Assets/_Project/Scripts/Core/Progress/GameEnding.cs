namespace BuyuFisiltisi.Core.Progress
{
    public enum GameOutcome
    {
        None,
        Good,
        Bad
    }

    public sealed class EndingState
    {
        public GameOutcome Outcome { get; private set; } = GameOutcome.None;

        public bool IsOver => Outcome != GameOutcome.None;

        public bool TryEnd(GameOutcome outcome)
        {
            if (IsOver || outcome == GameOutcome.None)
                return false;

            Outcome = outcome;
            return true;
        }

        public string Title
        {
            get
            {
                switch (Outcome)
                {
                    case GameOutcome.Good: return "Orman Kalbi Açıldı";
                    case GameOutcome.Bad: return "Ormandan Sürüldün";
                    default: return string.Empty;
                }
            }
        }

        public string Story
        {
            get
            {
                switch (Outcome)
                {
                    case GameOutcome.Good:
                        return "Yaşlı Meşe sana güvendi ve mühürlü kapıyı açtı. Orman yeniden fısıldamaya başladı.";
                    case GameOutcome.Bad:
                        return "Yaşlı Meşe öfkeyle köklerini savurdu. Cadı ve Fısıl ormanın dışına fırlatıldı.";
                    default:
                        return string.Empty;
                }
            }
        }
    }
}
