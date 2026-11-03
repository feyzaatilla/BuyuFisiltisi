using BuyuFisiltisi.Gameplay.Book;
using BuyuFisiltisi.Gameplay.Forest;
using BuyuFisiltisi.UI.Pause;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuyuFisiltisi.Bootstrap
{
    public static class SceneExtras
    {
        public const string RootName = "_Ekstralar";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
            Attach();
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Attach();
        }

        private static void Attach()
        {
            var bootstrap = Object.FindFirstObjectByType<GameBootstrap>();
            if (bootstrap == null || bootstrap.Bus == null)
                return;

            if (Object.FindFirstObjectByType<PauseMenuView>() != null)
                return;

            var root = new GameObject(RootName);
            var pause = root.AddComponent<PauseMenuView>();
            pause.InjectEventBus(bootstrap.Bus);

            var book = Object.FindFirstObjectByType<SpellBookView>();
            var cauldron = Object.FindFirstObjectByType<CauldronStation>();
            pause.SetWindowCheck(() => (book != null && book.IsOpen) || (cauldron != null && cauldron.IsOpen));

            var oak = GameObject.Find(OakMoodTint.TargetName);
            if (oak != null && oak.GetComponent<OakMoodTint>() == null)
                oak.AddComponent<OakMoodTint>().InjectEventBus(bootstrap.Bus);
        }
    }
}
