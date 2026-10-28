using BuyuFisiltisi.Core.Actions;

namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakTurn
    {
        public OakTurn(OakDialogueStatus status, string reply, IGameAction action, int previousTrust, int currentTrust)
        {
            Status = status;
            Reply = reply;
            Action = action ?? NoAction.Instance;
            PreviousTrust = previousTrust;
            CurrentTrust = currentTrust;
        }

        public OakDialogueStatus Status { get; }
        public string Reply { get; }
        public IGameAction Action { get; }
        public int PreviousTrust { get; }
        public int CurrentTrust { get; }
        public bool IsOk => Status == OakDialogueStatus.Ok;
        public bool TrustChanged => CurrentTrust != PreviousTrust;
    }
}
