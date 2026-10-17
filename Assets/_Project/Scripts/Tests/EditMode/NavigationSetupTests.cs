using System.Linq;
using BuyuFisiltisi.Core.Navigation;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode
{
    /// <summary>Katman ve Agent Type tanımlarının şartnameyle tutarlı olduğunu doğrulayan testler.</summary>
    public class NavigationSetupTests
    {
        [Test]
        public void RootWallGap_IsWithinSpecRange()
        {
            Assert.Greater(AgentTypes.RootWallGapWidth, 0.3f);
            Assert.Less(AgentTypes.RootWallGapWidth, 0.5f);
        }

        [Test]
        public void RootWallGap_OnlyFisilFits()
        {
            Assert.IsTrue(AgentTypes.Fisil.FitsThrough(AgentTypes.RootWallGapWidth));
            Assert.IsFalse(AgentTypes.Cadi.FitsThrough(AgentTypes.RootWallGapWidth));
        }

        [Test]
        public void GameLayers_AreUniqueUserLayers()
        {
            var indices = GameLayers.All.Select(l => l.Index).ToArray();
            var names = GameLayers.All.Select(l => l.Name).ToArray();

            Assert.AreEqual(indices.Length, indices.Distinct().Count());
            Assert.AreEqual(names.Length, names.Distinct().Count());
            // 0-5 Unity'nin yerleşik katmanları, 31'den sonrası yok.
            Assert.IsTrue(indices.All(i => i >= 6 && i <= 31));
        }
    }
}
