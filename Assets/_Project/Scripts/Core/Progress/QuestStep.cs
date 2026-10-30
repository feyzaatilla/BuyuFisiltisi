namespace BuyuFisiltisi.Core.Progress
{
    public readonly struct QuestStep
    {
        public readonly string Id;
        public readonly string Text;

        public QuestStep(string id, string text)
        {
            Id = id;
            Text = text;
        }
    }
}
