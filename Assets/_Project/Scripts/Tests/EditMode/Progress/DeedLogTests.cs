using BuyuFisiltisi.Core.Progress;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.Progress
{
    public class DeedLogTests
    {
        private const string Deed = "firefly_healed";

        [Test]
        public void NewLog_IsEmpty()
        {
            var log = new DeedLog();

            Assert.AreEqual(0, log.Count);
            Assert.IsFalse(log.Has(Deed));
        }

        [Test]
        public void Record_AddsDeedWithDescription()
        {
            var log = new DeedLog();

            var added = log.Record(Deed, "Ateş böceği iyileştirildi.");

            Assert.IsTrue(added);
            Assert.IsTrue(log.Has(Deed));
            Assert.AreEqual("Ateş böceği iyileştirildi.", log.GetDescription(Deed));
        }

        [Test]
        public void Record_SameDeedTwice_IsIgnored()
        {
            var log = new DeedLog();
            log.Record(Deed, "ilk");

            var added = log.Record(Deed, "ikinci");

            Assert.IsFalse(added);
            Assert.AreEqual(1, log.Count);
            Assert.AreEqual("ilk", log.GetDescription(Deed));
        }

        [Test]
        public void Record_EmptyId_IsIgnored()
        {
            var log = new DeedLog();

            Assert.IsFalse(log.Record("", "x"));
            Assert.IsFalse(log.Record(null, "x"));
            Assert.AreEqual(0, log.Count);
        }

        [Test]
        public void GetDescription_UnknownDeed_ReturnsEmpty()
        {
            var log = new DeedLog();

            Assert.AreEqual(string.Empty, log.GetDescription("yok"));
        }
    }
}