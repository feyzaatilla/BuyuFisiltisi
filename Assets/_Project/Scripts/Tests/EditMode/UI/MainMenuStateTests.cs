using BuyuFisiltisi.UI.Menu;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class MainMenuStateTests
    {
        [Test]
        public void StartsOnMainPage()
        {
            Assert.AreEqual(MainMenuPage.Main, new MainMenuState().Page);
        }

        [Test]
        public void OpenHowTo_ThenBack_ReturnsToMain()
        {
            var state = new MainMenuState();

            state.OpenHowTo();
            Assert.AreEqual(MainMenuPage.HowTo, state.Page);

            state.Back();
            Assert.AreEqual(MainMenuPage.Main, state.Page);
        }

        [Test]
        public void Escape_OnHowTo_GoesBack()
        {
            var state = new MainMenuState();
            state.OpenHowTo();

            Assert.IsTrue(state.HandleEscape());
            Assert.AreEqual(MainMenuPage.Main, state.Page);
        }

        [Test]
        public void Escape_OnMain_DoesNothing()
        {
            var state = new MainMenuState();

            Assert.IsFalse(state.HandleEscape());
            Assert.AreEqual(MainMenuPage.Main, state.Page);
        }
    }
}
