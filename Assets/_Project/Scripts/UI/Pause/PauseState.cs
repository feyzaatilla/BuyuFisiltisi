namespace BuyuFisiltisi.UI.Pause
{
    public sealed class PauseState
    {
        public bool IsPaused { get; private set; }
        public bool ShowHelp { get; private set; }

        public bool OnKey(bool escape, bool blocked, bool frozenByOther)
        {
            if (IsPaused)
            {
                if (ShowHelp && escape)
                    ShowHelp = false;
                else
                    Resume();
                return true;
            }

            if (blocked || frozenByOther)
                return false;

            IsPaused = true;
            ShowHelp = false;
            return true;
        }

        public void OpenHelp()
        {
            if (IsPaused)
                ShowHelp = true;
        }

        public void CloseHelp()
        {
            ShowHelp = false;
        }

        public void Resume()
        {
            IsPaused = false;
            ShowHelp = false;
        }
    }
}
