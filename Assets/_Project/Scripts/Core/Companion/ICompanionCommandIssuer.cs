namespace BuyuFisiltisi.Core.Companion
{
    /// <summary>
    /// Emir veren taraf (önce geçici klavye girişi, Aşama 4'te kitabın Emir sayfası).
    /// Bootstrap, emir verenleri emir alana bu metotla bağlar; ikisi birbirini doğrudan tanımaz.
    /// </summary>
    public interface ICompanionCommandIssuer
    {
        void SetCommandReceiver(ICompanionCommandReceiver receiver);
    }
}
