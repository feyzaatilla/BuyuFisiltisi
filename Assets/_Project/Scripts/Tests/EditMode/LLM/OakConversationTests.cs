using System;
using BuyuFisiltisi.LLM.Dialogue;
using BuyuFisiltisi.LLM.Ollama;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakConversationTests
    {
        [Test]
        public void BuildRequest_PutsSystemFirstAndPlayerLast()
        {
            var conversation = new OakConversation();

            var messages = conversation.BuildRequest("kurallar", "  Merhaba  ");

            Assert.AreEqual(2, messages.Count);
            Assert.AreEqual(ChatMessage.SystemRole, messages[0].role);
            Assert.AreEqual("kurallar", messages[0].content);
            Assert.AreEqual(ChatMessage.UserRole, messages[1].role);
            Assert.AreEqual("Merhaba", messages[1].content);
        }

        [Test]
        public void Record_AddsPlayerAndOakAsJson()
        {
            var conversation = new OakConversation();

            conversation.Record("Merhaba", new OakResponse("Hoş geldin.", OakAction.GiveHint, 1));

            Assert.AreEqual(2, conversation.Count);
            Assert.AreEqual(ChatMessage.AssistantRole, conversation.History[1].role);
            Assert.IsTrue(OakResponseParser.TryParse(conversation.History[1].content, out var parsed));
            Assert.AreEqual("Hoş geldin.", parsed.Reply);
            Assert.AreEqual(OakAction.GiveHint, parsed.Action);
            Assert.AreEqual(1, parsed.TrustDelta);
        }

        [Test]
        public void History_IsIncludedInNextRequest()
        {
            var conversation = new OakConversation();
            conversation.Record("Merhaba", new OakResponse("Selam.", OakAction.None, 0));

            var messages = conversation.BuildRequest("kurallar", "Kapıyı açar mısın?");

            Assert.AreEqual(4, messages.Count);
            Assert.AreEqual("Merhaba", messages[1].content);
            Assert.AreEqual("Kapıyı açar mısın?", messages[3].content);
        }

        [Test]
        public void History_DropsOldestPairs()
        {
            var conversation = new OakConversation(4);

            for (var i = 0; i < 5; i++)
                conversation.Record("Söz " + i, new OakResponse("Cevap " + i, OakAction.None, 0));

            Assert.AreEqual(4, conversation.Count);
            Assert.AreEqual("Söz 3", conversation.History[0].content);
        }

        [Test]
        public void Normalize_TrimsJoinsLinesAndCutsLongText()
        {
            Assert.AreEqual(string.Empty, OakConversation.Normalize(null));
            Assert.AreEqual("bir iki", OakConversation.Normalize(" bir\niki "));
            Assert.AreEqual(OakConversation.MaxPlayerTextLength, OakConversation.Normalize(new string('a', 500)).Length);
        }

        [Test]
        public void EmptyPlayerText_Throws_AndIsNotRecorded()
        {
            var conversation = new OakConversation();

            Assert.Throws<ArgumentException>(() => conversation.BuildRequest("kurallar", "   "));
            conversation.Record("   ", new OakResponse("x", OakAction.None, 0));
            Assert.AreEqual(0, conversation.Count);
        }
    }
}
