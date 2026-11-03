using BuyuFisiltisi.UI.Menu;
using NUnit.Framework;

namespace BuyuFisiltisi.Tests.EditMode.UI
{
    public class MenuSceneOrderTests
    {
        private const string Menu = "Assets/_Project/Scenes/MainMenu.unity";
        private const string Game = "Assets/_Project/Scenes/ForestScene.unity";

        [Test]
        public void PutsMenuFirstAndGameSecond()
        {
            var order = MenuSceneOrder.Arrange(Menu, Game, new[] { Game });

            CollectionAssert.AreEqual(new[] { Menu, Game }, order);
        }

        [Test]
        public void RemovesSampleScene()
        {
            var order = MenuSceneOrder.Arrange(Menu, Game, new[] { Game, "Assets/Scenes/SampleScene.unity" });

            CollectionAssert.AreEqual(new[] { Menu, Game }, order);
        }

        [Test]
        public void DoesNotDuplicateWhenRunTwice()
        {
            var first = MenuSceneOrder.Arrange(Menu, Game, new[] { Game });
            var second = MenuSceneOrder.Arrange(Menu, Game, first);

            CollectionAssert.AreEqual(new[] { Menu, Game }, second);
        }

        [Test]
        public void KeepsOtherScenesAfterGame()
        {
            const string other = "Assets/_Project/Scenes/Extra.unity";
            var order = MenuSceneOrder.Arrange(Menu, Game, new[] { other, Game });

            CollectionAssert.AreEqual(new[] { Menu, Game, other }, order);
        }

        [Test]
        public void HandlesEmptyCurrentList()
        {
            CollectionAssert.AreEqual(new[] { Menu, Game }, MenuSceneOrder.Arrange(Menu, Game, null));
        }
    }
}
