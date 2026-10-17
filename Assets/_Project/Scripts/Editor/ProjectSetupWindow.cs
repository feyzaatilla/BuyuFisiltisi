using System.Collections.Generic;
using BuyuFisiltisi.Core.Navigation;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace BuyuFisiltisi.EditorTools
{
    /// <summary>
    /// "Büyü Fısıltısı → Proje Kurulum Kontrolü" penceresi.
    /// Unity sürümünü, klasörleri, katmanları ve iki Agent Type'ı kontrol eder;
    /// eksik katmanları ekler ve açık sahnede standart hiyerarşiyi kurar.
    /// </summary>
    public class ProjectSetupWindow : EditorWindow
    {
        private const string ExpectedUnityVersion = "6000.3.25f1";
        private const float RadiusTolerance = 0.01f;

        private static readonly string[] RequiredFolders =
        {
            "Assets/_Project/Scripts",
            "Assets/_Project/Scenes",
            "Assets/_Project/Prefabs",
            "Assets/_Project/Materials",
            "Assets/_Project/ScriptableObjects/Recipes",
        };

        // Sahne hiyerarşisi: kök nesne → alt nesneler
        private static readonly (string Root, string[] Children)[] SceneHierarchy =
        {
            ("_Systems", new[] { "Bootstrap" }),
            ("_Lighting", new string[0]),
            ("_Environment", new[] { "01_GirisCayiri", "02_KokDuvari_Kopru", "03_CicekVadisi", "04_AtesBocegiAgaci", "05_OrmanKalbi" }),
            ("_NavMesh", new string[0]),
            ("_Characters", new[] { "Cadi", "Fisil", "YasliMese" }),
            ("_Interactables", new[] { "BridgeSwitch", "Flowers" }),
            ("_UI", new string[0]),
        };

        // Unity'nin varsayılan sahnede oluşturduğu ışık nesneleri _Lighting altına taşınır.
        private static readonly string[] LightingObjects = { "Directional Light", "Global Volume" };

        private Vector2 _scroll;

        [MenuItem("Büyü Fısıltısı/Proje Kurulum Kontrolü")]
        public static void Open()
        {
            var window = GetWindow<ProjectSetupWindow>("Kurulum Kontrolü");
            window.minSize = new Vector2(420, 480);
        }

        private void OnGUI()
        {
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawSection("Unity sürümü");
            DrawCheck(Application.unityVersion == ExpectedUnityVersion,
                $"Beklenen {ExpectedUnityVersion}, açık olan {Application.unityVersion}");

            DrawSection("Klasörler");
            foreach (var folder in RequiredFolders)
                DrawCheck(AssetDatabase.IsValidFolder(folder), folder);

            DrawSection("Katmanlar (Tags and Layers)");
            var allLayersOk = true;
            foreach (var (index, name) in GameLayers.All)
            {
                var ok = LayerMask.LayerToName(index) == name;
                allLayersOk &= ok;
                DrawCheck(ok, $"{index}: {name}");
            }
            using (new EditorGUI.DisabledScope(allLayersOk))
            {
                if (GUILayout.Button("Eksik katmanları ekle"))
                    AddMissingLayers();
            }

            DrawSection("NavMesh Agent Types");
            foreach (var spec in AgentTypes.All)
            {
                var found = TryFindAgentType(spec.Name, out var settings);
                var radiusOk = found && Mathf.Abs(settings.agentRadius - spec.Radius) <= RadiusTolerance;
                DrawCheck(found && radiusOk,
                    found
                        ? $"{spec.Name}: yarıçap {settings.agentRadius:0.###} (beklenen {spec.Radius:0.###})"
                        : $"{spec.Name}: bulunamadı. Window → AI → Navigation → Agents → + ile ekle " +
                          $"(Radius {spec.Radius}, Height {spec.Height}, Step Height {spec.StepHeight}, Max Slope {spec.MaxSlope})");
            }
            EditorGUILayout.HelpBox(
                $"Kök duvarındaki yarık {AgentTypes.RootWallGapWidth} m olmalı: " +
                $"Fısıl geçer = {AgentTypes.Fisil.FitsThrough(AgentTypes.RootWallGapWidth)}, " +
                $"Cadı geçer = {AgentTypes.Cadi.FitsThrough(AgentTypes.RootWallGapWidth)}.",
                MessageType.Info);

            DrawSection("Açık sahne");
            EditorGUILayout.LabelField("Sahne", EditorSceneManager.GetActiveScene().name);
            if (GUILayout.Button("Sahne hiyerarşisini kur"))
                BuildSceneHierarchy();

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSection(string title)
        {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        private static void DrawCheck(bool ok, string label)
        {
            var previous = GUI.color;
            GUI.color = ok ? new Color(0.55f, 0.9f, 0.55f) : new Color(1f, 0.55f, 0.5f);
            EditorGUILayout.LabelField((ok ? "✔ " : "✘ ") + label, EditorStyles.wordWrappedLabel);
            GUI.color = previous;
        }

        /// <summary>Boş olan katman yuvalarına proje katmanlarını yazar; dolu ve farklı adlı bir yuvaya dokunmaz.</summary>
        private static void AddMissingLayers()
        {
            var tagManagerAssets = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAssets == null || tagManagerAssets.Length == 0)
            {
                Debug.LogError("[Kurulum] TagManager.asset okunamadı.");
                return;
            }

            var tagManager = new SerializedObject(tagManagerAssets[0]);
            var layers = tagManager.FindProperty("layers");

            foreach (var (index, name) in GameLayers.All)
            {
                var slot = layers.GetArrayElementAtIndex(index);
                if (slot.stringValue == name)
                    continue;

                if (!string.IsNullOrEmpty(slot.stringValue))
                {
                    Debug.LogWarning($"[Kurulum] {index}. katman '{slot.stringValue}' olarak dolu; '{name}' yazılmadı. Elle düzelt.");
                    continue;
                }

                slot.stringValue = name;
                Debug.Log($"[Kurulum] {index}. katman '{name}' eklendi.");
            }

            tagManager.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
        }

        /// <summary>Adı verilen Agent Type'ı NavMesh ayarlarında arar.</summary>
        private static bool TryFindAgentType(string name, out NavMeshBuildSettings settings)
        {
            var count = NavMesh.GetSettingsCount();
            for (var i = 0; i < count; i++)
            {
                var candidate = NavMesh.GetSettingsByIndex(i);
                if (NavMesh.GetSettingsNameFromID(candidate.agentTypeID) == name)
                {
                    settings = candidate;
                    return true;
                }
            }

            settings = default;
            return false;
        }

        /// <summary>
        /// Açık sahnede standart kök nesneleri ve iki NavMeshSurface'i oluşturur. Var olanlara dokunmaz,
        /// bu yüzden birden çok kez basmak güvenlidir. NavMesh'i pişirmez (Bake ayrı adım).
        /// </summary>
        private static void BuildSceneHierarchy()
        {
            var created = new List<string>();
            var roots = new Dictionary<string, Transform>();

            foreach (var (rootName, children) in SceneHierarchy)
            {
                var root = FindOrCreate(rootName, null, created);
                roots[rootName] = root.transform;
                foreach (var child in children)
                    FindOrCreate(child, root.transform, created);
            }

            foreach (var rootObject in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (System.Array.IndexOf(LightingObjects, rootObject.name) >= 0)
                    Undo.SetTransformParent(rootObject.transform, roots["_Lighting"], $"Move {rootObject.name}");
            }

            var navMeshRoot = roots["_NavMesh"];
            foreach (var spec in AgentTypes.All)
            {
                var surfaceObject = FindOrCreate($"NavMesh_{spec.Name}", navMeshRoot, created);
                var surface = surfaceObject.GetComponent<NavMeshSurface>();
                if (surface == null)
                    surface = Undo.AddComponent<NavMeshSurface>(surfaceObject);

                if (TryFindAgentType(spec.Name, out var settings))
                {
                    surface.agentTypeID = settings.agentTypeID;
                }
                else
                {
                    Debug.LogWarning($"[Kurulum] '{spec.Name}' Agent Type bulunamadı; NavMesh_{spec.Name} için Agent Type'ı sonra elle seç.");
                }

                // Fısıl çok ince; yarıktan geçen şeridin oluşması için voxel boyutunu küçültüyoruz.
                if (spec == AgentTypes.Fisil)
                {
                    surface.overrideVoxelSize = true;
                    surface.voxelSize = 0.03f;
                }
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log(created.Count == 0
                ? "[Kurulum] Hiyerarşi zaten tam, yeni nesne oluşturulmadı."
                : $"[Kurulum] Oluşturulan nesneler: {string.Join(", ", created)}");
        }

        private static GameObject FindOrCreate(string name, Transform parent, List<string> created)
        {
            if (parent != null)
            {
                var existingChild = parent.Find(name);
                if (existingChild != null)
                    return existingChild.gameObject;
            }
            else
            {
                foreach (var rootObject in EditorSceneManager.GetActiveScene().GetRootGameObjects())
                    if (rootObject.name == name)
                        return rootObject;
            }

            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            if (parent != null)
                go.transform.SetParent(parent, false);
            created.Add(name);
            return go;
        }
    }
}
