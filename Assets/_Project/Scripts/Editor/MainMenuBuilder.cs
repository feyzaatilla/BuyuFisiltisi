using System.Linq;
using BuyuFisiltisi.Gameplay.Forest;
using BuyuFisiltisi.UI.Connection;
using BuyuFisiltisi.UI.Menu;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace BuyuFisiltisi.EditorTools
{
    public static class MainMenuBuilder
    {
        private const string SceneFolder = "Assets/_Project/Scenes";
        private const string MenuScenePath = SceneFolder + "/MainMenu.unity";
        private const string GameScenePath = SceneFolder + "/" + MainMenuContent.GameSceneName + ".unity";
        private const string ModelFolder = "Assets/_Project/Art/kenney_nature-kit/Models/FBX format/";
        private const string GroundMaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Zemin.mat";
        private const string GlowMaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Isilti.mat";
        private const string KayKitFolder = "Assets/_Project/Art/KayKit_Adventurers_2.0_FREE/";
        private const string WitchControllerPath = "Assets/_Project/Art/Cadi.controller";
        private const string Title = "Ana Menü";

        private static readonly Vector3 WitchPosition = new Vector3(-3.2f, 0f, 3.2f);
        private static readonly Vector3 BookPosition = new Vector3(-1.8f, 1.9f, 2.9f);
        private static readonly Color FogColor = new Color(0.06f, 0.12f, 0.1f, 1f);
        private static readonly string[] TreeModels = { "tree_pineRoundA", "tree_pineRoundC", "tree_oak_dark", "tree_default_dark", "tree_cone_dark", "tree_detailed_dark" };
        private static readonly string[] GroundModels = { "grass_large", "grass_leafsLarge", "flower_purpleA", "flower_yellowB", "mushroom_redGroup", "mushroom_tanGroup", "rock_largeB" };

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
            SetAtmosphere();
            var backdrop = new GameObject("Arka_Plan").transform;
            var focus = CreateBackdrop(backdrop);
            CreateCharacters(backdrop);
            CreateCamera(focus);
            CreateMenu();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, MenuScenePath);
            AssetDatabase.Refresh();

            ApplyBuildSettings();
            Debug.Log($"[Ana Menü] {MenuScenePath} kuruldu. Build sırası: {string.Join(", ", EditorBuildSettings.scenes.Select(s => s.path))}");
        }

        private static void SetAtmosphere()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = FogColor;
            RenderSettings.fogDensity = 0.045f;
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.16f, 0.24f, 0.22f);

            var moon = new GameObject("Ay_Isigi").AddComponent<Light>();
            moon.type = LightType.Directional;
            moon.color = new Color(0.55f, 0.65f, 0.9f);
            moon.intensity = 0.35f;
            moon.transform.rotation = Quaternion.Euler(35f, -30f, 0f);
        }

        private static Transform CreateBackdrop(Transform root)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Zemin";
            ground.transform.SetParent(root, false);
            ground.transform.position = new Vector3(0f, 0f, 10f);
            ground.transform.localScale = new Vector3(8f, 1f, 8f);
            var groundMaterial = AssetDatabase.LoadAssetAtPath<Material>(GroundMaterialPath);
            if (groundMaterial != null)
                ground.GetComponent<Renderer>().sharedMaterial = groundMaterial;

            var oakPosition = MenuBackdropLayout.OakPosition;
            PlaceModel(root, "tree_fat_darkh", oakPosition, 0f, 11f);
            var focus = new GameObject("Mese_Odak").transform;
            focus.SetParent(root, false);
            focus.position = oakPosition + new Vector3(0f, 3.2f, 0f);

            var aura = new GameObject("Mese_Isigi").AddComponent<Light>();
            aura.transform.SetParent(root, false);
            aura.transform.position = oakPosition + new Vector3(0f, 3.5f, -2.5f);
            aura.type = LightType.Point;
            aura.color = new Color(1f, 0.85f, 0.5f);
            aura.intensity = 6f;
            aura.range = 12f;
            aura.gameObject.AddComponent<FlickerLight>();

            var random = new System.Random(5);
            foreach (var position in MenuBackdropLayout.TreePositions(11, 22, 7f, 22f))
            {
                var model = TreeModels[random.Next(TreeModels.Length)];
                PlaceModel(root, model, position, random.Next(360), 5f + (float)random.NextDouble() * 4f);
            }

            for (var i = 0; i < 26; i++)
            {
                var x = -9f + (float)random.NextDouble() * 18f;
                var z = 1f + (float)random.NextDouble() * 14f;
                var spot = new Vector3(x, 0f, z);
                if (MenuBackdropLayout.InCorridor(spot) && Mathf.Abs(x) < 1.5f)
                    continue;
                if ((spot - WitchPosition).sqrMagnitude < 2.5f)
                    continue;

                PlaceModel(root, GroundModels[random.Next(GroundModels.Length)], spot, random.Next(360), 0.6f + (float)random.NextDouble() * 0.6f);
            }

            var glow = AssetDatabase.LoadAssetAtPath<Material>(GlowMaterialPath);
            for (var i = 0; i < 36; i++)
            {
                var mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mote.name = "Ates_Bocegi";
                mote.transform.SetParent(root, false);
                mote.transform.position = new Vector3(-8f + (float)random.NextDouble() * 16f, 0.6f + (float)random.NextDouble() * 4.5f, 2f + (float)random.NextDouble() * 14f);
                mote.transform.localScale = Vector3.one * 0.07f;
                Object.DestroyImmediate(mote.GetComponent<Collider>());
                if (glow != null)
                    mote.GetComponent<Renderer>().sharedMaterial = glow;
                var drift = new SerializedObject(mote.AddComponent<DriftMotion>());
                drift.FindProperty("_amplitude").floatValue = 0.5f;
                drift.FindProperty("_speed").floatValue = 0.25f;
                drift.ApplyModifiedPropertiesWithoutUndo();
            }

            return focus;
        }

        private static void CreateCharacters(Transform root)
        {
            var mage = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitFolder + "Characters/fbx/Mage.fbx");
            if (mage != null)
            {
                var witch = (GameObject)PrefabUtility.InstantiatePrefab(mage, root);
                witch.name = "Cadi";
                FitToHeight(witch.transform, WitchPosition, 1.9f);
                witch.transform.rotation = Quaternion.Euler(0f, 160f, 0f);
                var animator = witch.GetComponent<Animator>();
                if (animator == null)
                    animator = witch.AddComponent<Animator>();
                animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(WitchControllerPath);
                animator.applyRootMotion = false;
            }
            else
            {
                Debug.LogWarning("[Ana Menü] Cadı modeli bulunamadı.");
            }

            var bookAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitFolder + "Assets/fbx(unity)/spellbook_open.fbx");
            if (bookAsset == null)
            {
                Debug.LogWarning("[Ana Menü] Fısıl'ın kitap modeli bulunamadı.");
                return;
            }

            var fisil = new GameObject("Fisil").transform;
            fisil.SetParent(root, false);
            fisil.position = BookPosition;
            fisil.rotation = Quaternion.Euler(0f, 200f, 0f);

            var book = (GameObject)PrefabUtility.InstantiatePrefab(bookAsset, fisil);
            book.name = "spellbook_open";
            book.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f) * bookAsset.transform.localRotation;
            var renderers = book.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                    bounds.Encapsulate(renderer.bounds);
                var largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (largest > 0.0001f)
                    book.transform.localScale = Vector3.one * (1.1f / largest);
            }

            var glow = new GameObject("Fisil_Isigi").AddComponent<Light>();
            glow.transform.SetParent(fisil, false);
            glow.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            glow.type = LightType.Point;
            glow.color = new Color(0.5f, 0.9f, 1f);
            glow.intensity = 4f;
            glow.range = 4.5f;
            glow.gameObject.AddComponent<FlickerLight>();

            var drift = new SerializedObject(fisil.gameObject.AddComponent<DriftMotion>());
            drift.FindProperty("_amplitude").floatValue = 0.15f;
            drift.FindProperty("_speed").floatValue = 0.5f;
            drift.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void FitToHeight(Transform model, Vector3 feet, float height)
        {
            model.position = feet;
            model.localScale = Vector3.one;

            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            if (bounds.size.y > 0.0001f)
                model.localScale = Vector3.one * (height / bounds.size.y);

            bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            model.position += Vector3.up * (feet.y - bounds.min.y);
        }

        private static void CreateCamera(Transform focus)
        {
            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = FogColor;
            camera.fieldOfView = 55f;
            cameraObject.transform.position = MenuBackdropLayout.CameraPosition;
            cameraObject.transform.LookAt(focus.position);

            var drift = cameraObject.AddComponent<MenuCameraDrift>();
            var serialized = new SerializedObject(drift);
            serialized.FindProperty("_lookAt").objectReferenceValue = focus;
            serialized.ApplyModifiedPropertiesWithoutUndo();
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

        private static GameObject PlaceModel(Transform parent, string modelName, Vector3 position, float yaw, float targetSize)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + modelName + ".fbx");
            if (asset == null)
            {
                Debug.LogWarning("[Ana Menü] Model bulunamadı: " + modelName);
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = modelName;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return instance;

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            var largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largest > 0.0001f)
                instance.transform.localScale = Vector3.one * (targetSize / largest);

            return instance;
        }

        private static void ApplyBuildSettings()
        {
            var current = EditorBuildSettings.scenes.Select(s => s.path);
            var ordered = MenuSceneOrder.Arrange(MenuScenePath, GameScenePath, current);
            EditorBuildSettings.scenes = ordered.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
        }
    }
}
