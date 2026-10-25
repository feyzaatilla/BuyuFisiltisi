using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using BuyuFisiltisi.UI.Connection;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class ConnectionMessagesTests
    {
        [Test]
        public void Ready_HidesPanelAndShowsConnected()
        {
            var view = ConnectionMessages.For(OllamaHealth.Ready, "gemma2:2b");

            Assert.AreEqual(PanelVisibility.Hidden, view.Panel);
            Assert.AreEqual(ConnectionTone.Good, view.Tone);
            Assert.AreEqual(ConnectionMessages.ConnectedIndicator, view.Indicator);
        }

        [Test]
        public void Unreachable_ShowsPanelWithConnectionMessage()
        {
            var view = ConnectionMessages.For(OllamaHealth.Unreachable, "gemma2:2b");

            Assert.AreEqual(PanelVisibility.Shown, view.Panel);
            Assert.AreEqual(ConnectionTone.Bad, view.Tone);
            StringAssert.Contains("Bağlantı kurulamadı", view.Detail);
        }

        [Test]
        public void ModelMissing_TellsWhichModelToPull()
        {
            var view = ConnectionMessages.For(OllamaHealth.ModelMissing, "gemma2:2b");

            Assert.AreEqual(PanelVisibility.Shown, view.Panel);
            StringAssert.Contains("ollama pull gemma2:2b", view.Detail);
        }

        [Test]
        public void Checking_KeepsPanelAndDisablesRetry()
        {
            var view = ConnectionMessages.For(OllamaHealth.Checking, "gemma2:2b");

            Assert.AreEqual(PanelVisibility.Unchanged, view.Panel);
            Assert.IsTrue(view.Busy);
        }

        [Test]
        public void DialogueTimeout_ShowsPanel()
        {
            var view = ConnectionMessages.For(OakDialogueStatus.Timeout);

            Assert.AreEqual(PanelVisibility.Shown, view.Panel);
            StringAssert.Contains("zaman aşımı", view.Detail);
        }

        [Test]
        public void DialogueServerUnavailable_ShowsPanel()
        {
            Assert.AreEqual(PanelVisibility.Shown, ConnectionMessages.For(OakDialogueStatus.ServerUnavailable).Panel);
        }

        [Test]
        public void DialogueInvalidResponse_DoesNotShowPanel()
        {
            Assert.AreEqual(PanelVisibility.Unchanged, ConnectionMessages.For(OakDialogueStatus.InvalidResponse).Panel);
        }
    }
}
