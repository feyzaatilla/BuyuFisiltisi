using System.Linq;
using BuyuFisiltisi.UI.Connection;
using BuyuFisiltisi.UI.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuyuFisiltisi.EditorTools
{
    public static class MainMenuBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string MenuScenePath = SceneFolder + "/MainMenu.unity";
        private const string GameScenePath = SceneFolder + "/" + MainMenuContent.GameSceneName + ".unity";
        private const string Title = "Ana Menü";

        private static readonly Color Background = new Color(0.05f, 0.1f, 0.07f, 1f);

        [MenuItem("Büyü Fısıltısı/Ana Menü Sahnesini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(GameScenePath) == null)
            {
                EditorUtility.DisplayDialog(Title, $"Oyun sahnesi bulunamadı: {GameScenePath}", "Tamam");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CreateCamera();
            CreateMenu();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
            AssetDatabase.Refresh();

            ApplyBuildSettings();
            Debug.Log($"[Ana Menü] {MenuScenePath} kuruldu. Build sırası: {string.Join(", ", EditorBuildSettings.scenes.Select(s => s.path))}");
        }

        private static void CreateCamera()
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Background;
            cameraObject.transform.position = new Vector3(0f, 1f, -10f);
        }

        private static void CreateMenu()
        {
            var root = new GameObject("AnaMenu");
            var monitor = root.AddComponent<OllamaConnectionMonitor>();
            var view = root.AddComponent<MainMenuView>();

            var serialized = new SerializedObject(view);
            serialized.FindProperty("_monitor").objectReferenceValue = monitor;
            serialized.FindProperty("_gameScene").stringValue = MainMenuContent.GameSceneName;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ApplyBuildSettings()
        {
            var current = EditorBuildSettings.scenes.Select(s => s.path);
            var ordered = MenuSceneOrder.Arrange(MenuScenePath, GameScenePath, current);
            EditorBuildSettings.scenes = ordered.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        }
    }
}
