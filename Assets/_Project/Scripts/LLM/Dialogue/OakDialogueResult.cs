namespace BuyuFisiltisi.LLM.Dialogue
{
    public sealed class OakDialogueResult
    {
        private OakDialogueResult(OakDialogueStatus status, OakResponse response, string rawContent)
        {
            Status = status;
            Response = response ?? OakResponse.Fallback;
            RawContent = rawContent;
        }

        public OakDialogueStatus Status { get; }
        public OakResponse Response { get; }
        public string RawContent { get; }
        public bool IsOk => Status == OakDialogueStatus.Ok;

        public static OakDialogueResult Ok(OakResponse response, string rawContent) => new OakDialogueResult(OakDialogueStatus.Ok, response, rawContent);
        public static OakDialogueResult Fail(OakDialogueStatus status, string rawContent = null) => new OakDialogueResult(status, OakResponse.Fallback, rawContent);
    }
}
