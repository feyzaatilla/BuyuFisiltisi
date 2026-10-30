using System.Linq;
using BuyuFisiltisi.Gameplay.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuyuFisiltisi.EditorTools
{
    public static class SealedDoorBuilder
    {
        private const string SceneName = "ForestScene";
        private const string Title = "Mühürlü Kapı";

        [MenuItem("Büyü Fısıltısı/Mühürlü Kapıyı Bağla")]
        public static void Wire()
        {
            if (EditModeRunner.RunInEditMode(Wire))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                Dialog($"Önce {SceneName} sahnesini aç.");
                return;
            }

            var door = scene.GetRootGameObjects()
                .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                .FirstOrDefault(child => child.name == SealedDoor.DefaultId);

            if (door == null)
            {
                Dialog("Sahnede MuhurluKapi bulunamadı. Önce Graybox Sahnesini Kur menüsünü çalıştır.");
                return;
            }

            if (door.GetComponent<SealedDoor>() == null)
                Undo.AddComponent<SealedDoor>(door.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Kapı] Mühürlü kapıya açılma bileşeni eklendi. Ctrl+S ile kaydet.");
        }

        private static void Dialog(string message)
        {
            EditorUtility.DisplayDialog(Title, message, "Tamam");
        }
    }
}
