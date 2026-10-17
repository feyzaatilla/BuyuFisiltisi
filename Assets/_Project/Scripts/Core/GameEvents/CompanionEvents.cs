namespace BuyuFisiltisi.Core.GameEvents
{
    /// <summary>
    /// Fısıl'ın durum makinesi durum değiştirdiğinde yayınlanır.
    /// Durum adları metin olarak taşınır; böylece Core, Companion assembly'sine bağımlı olmaz.
    /// </summary>
    public readonly struct CompanionStateChangedEvent
    {
        public readonly string PreviousState;
        public readonly string CurrentState;

        public CompanionStateChangedEvent(string previousState, string currentState)
        {
            PreviousState = previousState;
            CurrentState = currentState;
        }
    }

    /// <summary>Fısıl bir replik söylediğinde yayınlanır (atmosferik replik havuzundan, LLM'den değil).</summary>
    public readonly struct CompanionBarkEvent
    {
        public readonly string Text;
        public CompanionBarkEvent(string text) { Text = text; }
    }

    /// <summary>Fısıl'a verilen bir görevin nasıl sonuçlandığı.</summary>
    public enum CompanionTaskOutcome
    {
        Succeeded,
        Failed,
        Cancelled,
        TimedOut
    }

    /// <summary>
    /// Fısıl bir görevi bitirdiğinde yayınlanır.
    /// Aşama 5'teki CompanionTestRunner başarı oranı ve ortalama süreyi bu olaydan hesaplar.
    /// </summary>
    public readonly struct CompanionTaskFinishedEvent
    {
        public readonly string TaskName;
        public readonly CompanionTaskOutcome Outcome;
        public readonly float DurationSeconds;

        public CompanionTaskFinishedEvent(string taskName, CompanionTaskOutcome outcome, float durationSeconds)
        {
            TaskName = taskName;
            Outcome = outcome;
            DurationSeconds = durationSeconds;
        }
    }
}
