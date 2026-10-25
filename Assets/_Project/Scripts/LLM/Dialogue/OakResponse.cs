namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakResponse
    {
        public const int MinTrustDelta = -2;
        public const int MaxTrustDelta = 2;
        public const string FallbackReply = "Yaşlı Meşe bir süre sessizce yapraklarını hışırdatıyor.";

        public OakResponse(string reply, OakAction action, int trustDelta)
        {
            Reply = string.IsNullOrWhiteSpace(reply) ? FallbackReply : reply.Trim();
            Action = action;
            TrustDelta = trustDelta < MinTrustDelta ? MinTrustDelta : trustDelta > MaxTrustDelta ? MaxTrustDelta : trustDelta;
        }

        public string Reply { get; }
        public OakAction Action { get; }
        public int TrustDelta { get; }

        public static OakResponse Fallback => new OakResponse(FallbackReply, OakAction.None, 0);
    }
}
