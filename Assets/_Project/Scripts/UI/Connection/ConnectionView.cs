namespace BuyuFisiltisi.UI.Connection
{
    public enum PanelVisibility
    {
        Unchanged,
        Hidden,
        Shown
    }

    public enum ConnectionTone
    {
        Neutral,
        Good,
        Bad
    }

    public readonly struct ConnectionView
    {
        public ConnectionView(string indicator, ConnectionTone tone, PanelVisibility panel, string detail, bool busy)
        {
            Indicator = indicator;
            Tone = tone;
            Panel = panel;
            Detail = detail;
            Busy = busy;
        }

        public string Indicator { get; }
        public ConnectionTone Tone { get; }
        public PanelVisibility Panel { get; }
        public string Detail { get; }
        public bool Busy { get; }
    }
}
