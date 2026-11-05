using BuyuFisiltisi.LLM.Dialogue;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.LLM
{
    public class OakDeedCatalogTests
    {
        internal static readonly string[] AllDeeds =
        {
            "Ormanın üzerindeki kara bulutları dağıttı.",
            "Yaralı ateş böceği Işık Çayı ile iyileştirildi.",
            "Yaşlı Meşe'nin yaralı kökü Sessizlik Merhemi ile iyileştirildi.",
            "Gölge Tilki'ye zarar vermek yerine onu sakinleştirip sevdi.",
            "Gizli girdabın altındaki tuzaklı mağarayı geçip taş bulmacasını çözdü."
        };

        [Test]
        public void NoDeeds_AllHintsMissing()
        {
            Assert.AreEqual(OakDeedCatalog.Count, OakDeedCatalog.MissingHints(null).Count);
            Assert.AreEqual(OakDeedCatalog.Count, OakDeedCatalog.MissingHints(new string[0]).Count);
        }

        [Test]
        public void AllGameDeeds_NothingMissing()
        {
            Assert.AreEqual(0, OakDeedCatalog.MissingHints(AllDeeds).Count);
        }

        [Test]
        public void FireflyDone_FireflyHintRemoved()
        {
            var missing = OakDeedCatalog.MissingHints(new[] { AllDeeds[1] });

            Assert.AreEqual(OakDeedCatalog.Count - 1, missing.Count);
            foreach (var hint in missing)
                StringAssert.DoesNotContain("ateş böceği", hint);
        }

        [Test]
        public void FoxDone_OnlyFoxHintRemoved()
        {
            var missing = OakDeedCatalog.MissingHints(new[] { AllDeeds[3] });

            Assert.AreEqual(OakDeedCatalog.Count - 1, missing.Count);
            foreach (var hint in missing)
                StringAssert.DoesNotContain("Tilki", hint);
        }

        [Test]
        public void HintsKeepGameOrder()
        {
            var missing = OakDeedCatalog.MissingHints(null);

            StringAssert.Contains("kara bulut", missing[0]);
            StringAssert.Contains("mağara", missing[missing.Count - 1]);
        }
    }
}
