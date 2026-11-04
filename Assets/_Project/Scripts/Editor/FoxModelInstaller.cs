using System.Linq;
using BuyuFisiltisi.Gameplay.Forest;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuyuFisiltisi.EditorTools
{
    public static class FoxModelInstaller
    {
        public const string FoxModelPath = "Assets/_Project/Art/Quaternius_Animals/Fox.fbx";
        public const string ControllerPath = "Assets/_Project/Art/Tilki.controller";
        public const string FoxName = "GolgeTilki";
        public const string ModelName = "Tilki_Model";
        private const string OldBodyName = "Gorunum";
        private const float FoxLength = 1.3f;
        private const float ModelYaw = 0f;

        [MenuItem("Büyü Fısıltısı/Gölge Tilki Modelini Yerleştir")]
        private static void InstallInOpenScene()
        {
            var fox = GameObject.Find(FoxName);
            if (fox == null)
            {
                EditorUtility.DisplayDialog("Gölge Tilki", "Önce ForestScene sahnesini aç.", "Tamam");
                return;
            }

            if (!Install(fox))
            {
                EditorUtility.DisplayDialog("Gölge Tilki", "Fox.fbx bulunamadı: " + FoxModelPath, "Tamam");
                return;
            }

            EditorSceneManager.MarkSceneDirty(fox.scene);
            EditorSceneManager.SaveScene(fox.scene);
            Debug.Log("[Tilki] Gölge Tilki yeni modelle yerleştirildi.");
        }

        public static bool Install(GameObject fox)
        {
            if (fox == null)
                return false;

            PrepareImport();
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(FoxModelPath);
            if (asset == null)
            {
                Debug.LogWarning("[Tilki] Model bulunamadı: " + FoxModelPath);
                return false;
            }

            var controller = GetController();
            Remove(fox.transform, OldBodyName);
            Remove(fox.transform, ModelName);

            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset, fox.transform);
            model.name = ModelName;
            model.transform.localPosition = Vector3.zero;
            model.transform.localRotation = Quaternion.Euler(0f, ModelYaw, 0f);
            model.transform.localScale = Vector3.one;
            Fit(fox.transform, model.transform);

            var animator = model.GetComponent<Animator>();
            if (animator == null)
                animator = model.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var look = fox.GetComponent<FoxLook>();
            if (look == null)
                look = fox.AddComponent<FoxLook>();
            var lookData = new SerializedObject(look);
            lookData.FindProperty("_animator").objectReferenceValue = animator;
            lookData.ApplyModifiedPropertiesWithoutUndo();

            var shadow = fox.GetComponent<ShadowFox>();
            if (shadow != null)
            {
                var data = new SerializedObject(shadow);
                data.FindProperty("_eyes").arraySize = 0;
                data.FindProperty("_fur").arraySize = 0;
                data.ApplyModifiedPropertiesWithoutUndo();
            }

            return true;
        }

        private static void PrepareImport()
        {
            var importer = AssetImporter.GetAtPath(FoxModelPath) as ModelImporter;
            if (importer == null)
                return;

            if (importer.animationType != ModelImporterAnimationType.Generic || !importer.isReadable)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.isReadable = true;
                importer.SaveAndReimport();
            }

            var clips = importer.clipAnimations;
            if (clips == null || clips.Length == 0)
                clips = importer.defaultClipAnimations;

            var changed = false;
            foreach (var clip in clips)
            {
                var loop = FoxAnimation.Loops(FoxAnimation.ClipName(clip.name));
                if (clip.loopTime == loop)
                    continue;

                clip.loopTime = loop;
                changed = true;
            }

            if (!changed)
                return;

            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static RuntimeAnimatorController GetController()
        {
            var existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
            if (existing != null)
                return existing;

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            var machine = controller.layers[0].stateMachine;
            var clips = AssetDatabase.LoadAllAssetsAtPath(FoxModelPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__"));

            foreach (var clip in clips)
            {
                var state = machine.AddState(FoxAnimation.ClipName(clip.name));
                state.motion = clip;
                if (state.name == FoxAnimation.Idle)
                    machine.defaultState = state;
            }

            AssetDatabase.SaveAssets();
            return controller;
        }

        private static void Remove(Transform parent, string childName)
        {
            var child = parent.Find(childName);
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }

        private static void Fit(Transform fox, Transform model)
        {
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = Measure(renderers);
            var length = Mathf.Max(bounds.size.x, bounds.size.z);
            if (length > 0.0001f)
                model.localScale = Vector3.one * (FoxLength / length);

            bounds = Measure(renderers);
            model.position += Vector3.up * (fox.position.y - bounds.min.y);
        }

        private static Bounds Measure(Renderer[] renderers)
        {
            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            return bounds;
        }
    }
}
