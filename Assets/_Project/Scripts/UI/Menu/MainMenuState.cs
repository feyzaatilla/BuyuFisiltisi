namespace BuyuFisiltisi.UI.Menu
{
    public enum MainMenuPage
    {
        Main,
        HowTo
    }

    public sealed class MainMenuState
    {
        public MainMenuPage Page { get; private set; } = MainMenuPage.Main;

        public void OpenHowTo()
        {
            Page = MainMenuPage.HowTo;
        }

        public void Back()
        {
            Page = MainMenuPage.Main;
        }

        public bool HandleEscape()
        {
            if (Page == MainMenuPage.Main)
                return false;

            Back();
            return true;
        }
    }
}
