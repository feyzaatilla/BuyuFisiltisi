namespace BuyuFisiltisi.Core.Companion
{
    /// <summary>Emir alabilen taraf (Fısıl'ın CompanionController'ı bunu uygular).</summary>
    public interface ICompanionCommandReceiver
    {
        void IssueCommand(CompanionCommand command);
    }
}
