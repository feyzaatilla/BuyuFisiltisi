namespace BuyuFisiltisi.Companion.Barks
{
    public interface ICompanionBarkProvider
    {
        bool TryGetBark(BarkTrigger trigger, float time, out string text);
    }
}
