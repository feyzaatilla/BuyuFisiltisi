using System.Linq;
using BuyuFisiltisi.LLM.Ollama;
using BuyuFisiltisi.UI.Connection;
using BuyuFisiltisi.UI.Menu;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class MainMenuContentTests
    {
        [Test]
        public void GameScene_IsForestScene()
        {
            Assert.AreEqual("ForestScene", MainMenuContent.GameSceneName);
        }

        [Test]
        public void Ready_ShowsConnectedIndicatorWithoutNote()
        {
            Assert.AreEqual(ConnectionMessages.ConnectedIndicator, MainMenuContent.Indicator(OllamaHealth.Ready, "gemma2:2b"));
            Assert.AreEqual(ConnectionTone.Good, MainMenuContent.Tone(OllamaHealth.Ready, "gemma2:2b"));
            Assert.IsNull(MainMenuContent.Note(OllamaHealth.Ready, "gemma2:2b"));
            Assert.IsFalse(MainMenuContent.CanRetry(OllamaHealth.Ready));
        }

        [Test]
        public void Unreachable_ShowsOfflineNoteAndRetry()
        {
            Assert.AreEqual(ConnectionMessages.DisconnectedIndicator, MainMenuContent.Indicator(OllamaHealth.Unreachable, "gemma2:2b"));
            Assert.AreEqual(ConnectionTone.Bad, MainMenuContent.Tone(OllamaHealth.Unreachable, "gemma2:2b"));
            Assert.AreEqual(MainMenuContent.OfflineNote, MainMenuContent.Note(OllamaHealth.Unreachable, "gemma2:2b"));
            Assert.IsTrue(MainMenuContent.CanRetry(OllamaHealth.Unreachable));
        }

        [Test]
        public void ModelMissing_NamesModelInNote()
        {
            StringAssert.Contains("ollama pull gemma2:2b", MainMenuContent.Note(OllamaHealth.ModelMissing, "gemma2:2b"));
            Assert.IsTrue(MainMenuContent.CanRetry(OllamaHealth.ModelMissing));
        }

        [Test]
        public void Checking_CannotRetry()
        {
            Assert.AreEqual(ConnectionMessages.CheckingIndicator, MainMenuContent.Indicator(OllamaHealth.Checking, "gemma2:2b"));
            Assert.IsFalse(MainMenuContent.CanRetry(OllamaHealth.Checking));
        }

        [Test]
        public void Controls_IncludeTalkAndBookKeys()
        {
            var keys = MainMenuContent.Controls.Select(c => c.Key).ToList();

            CollectionAssert.Contains(keys, "E");
            CollectionAssert.Contains(keys, "Tab");
            Assert.IsTrue(MainMenuContent.Controls.All(c => !string.IsNullOrWhiteSpace(c.Action)));
        }

        [Test]
        public void UiScale_GrowsWithScreenHeight()
        {
            Assert.AreEqual(1f, MainMenuContent.UiScale(1080f), 0.001f);
            Assert.AreEqual(2f, MainMenuContent.UiScale(2160f), 0.001f);
            Assert.AreEqual(1f, MainMenuContent.UiScale(0f), 0.001f);
        }

        [Test]
        public void Goals_MentionOakAndFox()
        {
            var all = string.Join(" ", MainMenuContent.Goals);

            StringAssert.Contains("Yaşlı Meşe", all);
            StringAssert.Contains("Gölge Tilki", all);
        }
    }
}
