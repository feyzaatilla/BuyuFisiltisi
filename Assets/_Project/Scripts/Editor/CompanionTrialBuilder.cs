using System.Linq;
using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Gameplay.Player;
using BuyuFisiltisi.Gameplay.World;
using BuyuFisiltisi.Testing;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuyuFisiltisi.EditorTools
{
    public static class CompanionTrialBuilder
    {
        private const string SceneName = "ForestScene";
        private const string Title = "Fısıl Başarı Testi";
        private const string RootName = "FisilBasariTesti";
        private const string TargetName = "FisilTestHedefi";
        private const string SystemsName = "_Systems";
        private const string StartPrefix = "FisilTestBaslangic_";

        [MenuItem("Büyü Fısıltısı/Fısıl Başarı Testini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                Dialog($"Önce {SceneName} sahnesini aç.");
                return;
            }

            var companion = Object.FindFirstObjectByType<CompanionController>(FindObjectsInactive.Include);
            if (companion == null)
            {
                Dialog("Sahnede Fısıl bulunamadı.");
                return;
            }

            var bridgeSwitch = Object.FindFirstObjectByType<BridgeSwitch>(FindObjectsInactive.Include);
            if (bridgeSwitch == null)
            {
                Dialog("Sahnede köprü şalteri bulunamadı.");
                return;
            }

            var systems = scene.GetRootGameObjects().FirstOrDefault(root => root.name == SystemsName);
            if (systems == null)
            {
                systems = new GameObject(SystemsName);
                Undo.RegisterCreatedObjectUndo(systems, Title);
            }

            var old = systems.transform.Find(RootName);
            if (old != null)
                Undo.DestroyObjectImmediate(old.gameObject);

            var root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, Title);
            root.transform.SetParent(systems.transform, false);

            var targetObject = new GameObject(TargetName);
            Undo.RegisterCreatedObjectUndo(targetObject, Title);
            targetObject.transform.SetParent(root.transform, false);
            targetObject.transform.position = bridgeSwitch.InteractionPoint.position;
            var target = targetObject.AddComponent<CompanionTrialTarget>();

            var switchPoint = bridgeSwitch.InteractionPoint.position;
            var witch = Object.FindFirstObjectByType<WitchMotor>(FindObjectsInactive.Include);
            var witchPoint = witch != null ? witch.transform.position : companion.transform.position;
            var positions = new[]
            {
                companion.transform.position,
                witchPoint,
                Vector3.Lerp(witchPoint, switchPoint, 0.5f)
            };

            var starts = new Transform[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                var start = new GameObject(StartPrefix + (i + 1));
                Undo.RegisterCreatedObjectUndo(start, Title);
                start.transform.SetParent(root.transform, false);
                start.transform.position = positions[i];
                starts[i] = start.transform;
            }

            var runner = root.AddComponent<CompanionTrialRunner>();
            var serialized = new SerializedObject(runner);
            serialized.FindProperty("_companion").objectReferenceValue = companion;
            serialized.FindProperty("_target").objectReferenceValue = target;
            var startsProperty = serialized.FindProperty("_startPoints");
            startsProperty.arraySize = starts.Length;
            for (var i = 0; i < starts.Length; i++)
                startsProperty.GetArrayElementAtIndex(i).objectReferenceValue = starts[i];
            serialized.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log($"[Fısıl Testi] Test kuruldu, {starts.Length} başlangıç noktası eklendi. Oyunda K tuşu 10 deneme başlatır. Ctrl+S ile kaydet.");
        }

        private static void Dialog(string message)
        {
            EditorUtility.DisplayDialog(Title, message, "Tamam");
        }
    }
}
