using BuyuFisiltisi.UI.Oak;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class ChatInputBufferTests
    {
        [Test]
        public void Append_AddsCharactersInOrder()
        {
            var buffer = new ChatInputBuffer();

            buffer.Append('M');
            buffer.Append('e');

            Assert.AreEqual("Me", buffer.Text);
        }

        [Test]
        public void Append_IgnoresControlCharacters()
        {
            var buffer = new ChatInputBuffer();

            Assert.IsFalse(buffer.Append('\r'));
            Assert.IsFalse(buffer.Append('\b'));
            Assert.IsFalse(buffer.Append('\n'));
            Assert.AreEqual(string.Empty, buffer.Text);
        }

        [Test]
        public void Append_StopsAtMaxLength()
        {
            var buffer = new ChatInputBuffer(3);

            buffer.Append('a');
            buffer.Append('b');
            buffer.Append('c');

            Assert.IsFalse(buffer.Append('d'));
            Assert.AreEqual("abc", buffer.Text);
        }

        [Test]
        public void Backspace_RemovesLastCharacter()
        {
            var buffer = new ChatInputBuffer();
            buffer.Append('a');
            buffer.Append('b');

            Assert.IsTrue(buffer.Backspace());
            Assert.AreEqual("a", buffer.Text);
        }

        [Test]
        public void Backspace_OnEmptyBufferDoesNothing()
        {
            var buffer = new ChatInputBuffer();

            Assert.IsFalse(buffer.Backspace());
            Assert.AreEqual(string.Empty, buffer.Text);
        }

        [Test]
        public void IsEmpty_IsTrueForSpacesOnly()
        {
            var buffer = new ChatInputBuffer();
            buffer.Append(' ');
            buffer.Append(' ');

            Assert.IsTrue(buffer.IsEmpty);
        }

        [Test]
        public void Take_ReturnsTrimmedTextAndClears()
        {
            var buffer = new ChatInputBuffer();
            foreach (var character in " Selam ")
                buffer.Append(character);

            Assert.AreEqual("Selam", buffer.Take());
            Assert.AreEqual(string.Empty, buffer.Text);
        }
    }
}
