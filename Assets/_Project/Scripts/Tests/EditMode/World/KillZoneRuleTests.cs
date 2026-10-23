using BuyuFisiltisi.Core.Navigation;
using BuyuFisiltisi.Gameplay.World;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.World
{
    public class KillZoneRuleTests
    {
        [Test]
        public void PlayerLayer_IsAffected()
        {
            var mask = GameLayers.Mask(GameLayers.Player);

            Assert.IsTrue(KillZoneRule.Affects(mask, GameLayers.Player));
        }

        [Test]
        public void OtherLayers_AreIgnored()
        {
            var mask = GameLayers.Mask(GameLayers.Player);

            Assert.IsFalse(KillZoneRule.Affects(mask, GameLayers.Companion));
            Assert.IsFalse(KillZoneRule.Affects(mask, -1));
            Assert.IsFalse(KillZoneRule.Affects(mask, 32));
        }
    }
}
