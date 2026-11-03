using BuyuFisiltisi.UI.Pause;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class PauseStateTests
    {
        [Test]
        public void Key_WhenFree_Pauses()
        {
            var state = new PauseState();

            Assert.IsTrue(state.OnKey(true, false, false));
            Assert.IsTrue(state.IsPaused);
        }

        [Test]
        public void Key_WhenAnotherWindowOpen_DoesNotPause()
        {
            var state = new PauseState();

            Assert.IsFalse(state.OnKey(true, true, false));
            Assert.IsFalse(state.IsPaused);
        }

        [Test]
        public void Key_WhenGameFrozenByIntro_DoesNotPause()
        {
            var state = new PauseState();

            Assert.IsFalse(state.OnKey(false, false, true));
            Assert.IsFalse(state.IsPaused);
        }

        [Test]
        public void Key_WhenPaused_Resumes()
        {
            var state = new PauseState();
            state.OnKey(false, false, false);

            Assert.IsTrue(state.OnKey(false, false, false));
            Assert.IsFalse(state.IsPaused);
        }

        [Test]
        public void Escape_OnHelp_ReturnsToPauseMenu()
        {
            var state = new PauseState();
            state.OnKey(true, false, false);
            state.OpenHelp();

            state.OnKey(true, false, false);

            Assert.IsTrue(state.IsPaused);
            Assert.IsFalse(state.ShowHelp);
        }

        [Test]
        public void P_OnHelp_ResumesGame()
        {
            var state = new PauseState();
            state.OnKey(false, false, false);
            state.OpenHelp();

            state.OnKey(false, false, false);

            Assert.IsFalse(state.IsPaused);
            Assert.IsFalse(state.ShowHelp);
        }

        [Test]
        public void OpenHelp_WhenNotPaused_DoesNothing()
        {
            var state = new PauseState();

            state.OpenHelp();

            Assert.IsFalse(state.ShowHelp);
        }
    }
}
