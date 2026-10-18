using System.Collections.Generic;
using System.Linq;
using BuyuFisiltisi.Core.Navigation;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuyuFisiltisi.EditorTools
{
    
    public static class GrayboxSceneBuilder
    {
        private const string MaterialFolder = "Assets/_Project/Materials/Graybox";
        private const float LevelHalfWidth = 10f;
        private const float WallThickness = 0.4f;
        private const float RootWallHeight = 2.5f;

        
        private const float ChasmStartZ = 20f;
        private const float ChasmEndZ = 24f;

        [MenuItem("Büyü Fısıltısı/Graybox Sahnesini Kur")]
        public static void Build()
        {
            var zones = FindZones();
            if (zones == null)
                return;

            if (zones.Values.Any(z => z.childCount > 0))
            {
                EditorUtility.DisplayDialog("Graybox",
                    "_Environment altındaki bölgelerde zaten nesne var. Graybox yalnızca boş bölgelere kurulur.", "Tamam");
                return;
            }

            var mats = new Materials();

            BuildEntryMeadow(zones["01_GirisCayiri"], mats);
            BuildRootWallAndBridge(zones["02_KokDuvari_Kopru"], mats);
            BuildFlowerValley(zones["03_CicekVadisi"], mats);
            BuildFireflyTree(zones["04_AtesBocegiAgaci"], mats);
            BuildForestHeart(zones["05_OrmanKalbi"], mats);
            BuildBoundary(zones["01_GirisCayiri"].parent, mats);
            PlaceCharacters(mats);
            ExcludeDynamicObjectsFromNavMesh();
            AddSceneToBuildSettings();

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("[Graybox] Sahne kuruldu. Şimdi _NavMesh altındaki iki yüzeyde Bake'e bas.");
        }

        private static Dictionary<string, Transform> FindZones()
        {
            var scene = EditorSceneManager.GetActiveScene();
            var environment = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "_Environment");
            if (environment == null)
            {
                EditorUtility.DisplayDialog("Graybox",
                    "_Environment bulunamadı. Önce Büyü Fısıltısı → Proje Kurulum Kontrolü → Sahne hiyerarşisini kur.", "Tamam");
                return null;
            }

            var names = new[] { "01_GirisCayiri", "02_KokDuvari_Kopru", "03_CicekVadisi", "04_AtesBocegiAgaci", "05_OrmanKalbi" };
            var result = new Dictionary<string, Transform>();
            foreach (var name in names)
            {
                var zone = environment.transform.Find(name);
                if (zone == null)
                {
                    EditorUtility.DisplayDialog("Graybox", $"_Environment/{name} bulunamadı. Hiyerarşiyi yeniden kur.", "Tamam");
                    return null;
                }
                result[name] = zone;
            }
            return result;
        }

        
        private static void BuildEntryMeadow(Transform zone, Materials mats)
        {
            Ground(zone, "Zemin", new Vector3(0, 0, 0), new Vector2(LevelHalfWidth * 2, 20), mats.Ground);
        }

        
        private static void BuildRootWallAndBridge(Transform zone, Materials mats)
        {
            Ground(zone, "Zemin_Yakin", new Vector3(0, 0, 15), new Vector2(LevelHalfWidth * 2, 10), mats.Ground);
            Ground(zone, "Zemin_Uzak", new Vector3(0, 0, 29), new Vector2(LevelHalfWidth * 2, 10), mats.Ground);

            
            var bridge = new GameObject("Kopru");
            Undo.RegisterCreatedObjectUndo(bridge, "Create Kopru");
            bridge.transform.SetParent(zone, false);
            bridge.transform.localPosition = new Vector3(0, 0, ChasmEndZ);
            bridge.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var deckLength = ChasmEndZ - ChasmStartZ + 0.4f;
            Box(bridge.transform, "Kopru_Tabla", new Vector3(0, -0.1f, -deckLength / 2f), new Vector3(4, 0.2f, deckLength),
                mats.Bridge, GameLayers.Obstacle);

            
            var nook = Group(zone, "SalterGirintisi");
            const float nookMinX = 7f, nookMaxX = LevelHalfWidth, nookMinZ = 12f, nookMaxZ = 18f;
            var gap = AgentTypes.RootWallGapWidth;
            var gapCenterZ = (nookMinZ + nookMaxZ) / 2f;

            
            var lowerLen = gapCenterZ - gap / 2f - nookMinZ;
            var upperLen = nookMaxZ - (gapCenterZ + gap / 2f);
            Box(nook, "KokDuvari_A", new Vector3(nookMinX, RootWallHeight / 2f, nookMinZ + lowerLen / 2f),
                new Vector3(WallThickness, RootWallHeight, lowerLen), mats.RootWall, GameLayers.Obstacle);
            Box(nook, "KokDuvari_B", new Vector3(nookMinX, RootWallHeight / 2f, nookMaxZ - upperLen / 2f),
                new Vector3(WallThickness, RootWallHeight, upperLen), mats.RootWall, GameLayers.Obstacle);

            
            var depth = nookMaxX - nookMinX + WallThickness;
            Box(nook, "KokDuvari_On", new Vector3((nookMinX + nookMaxX) / 2f, RootWallHeight / 2f, nookMinZ),
                new Vector3(depth, RootWallHeight, WallThickness), mats.RootWall, GameLayers.Obstacle);
            Box(nook, "KokDuvari_Arka", new Vector3((nookMinX + nookMaxX) / 2f, RootWallHeight / 2f, nookMaxZ),
                new Vector3(depth, RootWallHeight, WallThickness), mats.RootWall, GameLayers.Obstacle);

            var switchRoot = FindSceneObject("_Interactables/BridgeSwitch");
            Box(switchRoot != null ? switchRoot : nook, "Salter", new Vector3(9f, 0.6f, gapCenterZ),
                new Vector3(0.3f, 1.2f, 0.3f), mats.Interactable, GameLayers.Interactable);
        }

        
        private static void BuildFlowerValley(Transform zone, Materials mats)
        {
            Ground(zone, "Zemin", new Vector3(0, 0, 44), new Vector2(LevelHalfWidth * 2, 20), mats.Ground);

            var nook = Group(zone, "CicekGirintisi");
            const float nookX = -7f, minZ = 41f, maxZ = 47f;
            var gap = AgentTypes.RootWallGapWidth;
            var mid = (minZ + maxZ) / 2f;
            var len = mid - gap / 2f - minZ;
            Box(nook, "KokDuvari_A", new Vector3(nookX, RootWallHeight / 2f, minZ + len / 2f),
                new Vector3(WallThickness, RootWallHeight, len), mats.RootWall, GameLayers.Obstacle);
            Box(nook, "KokDuvari_B", new Vector3(nookX, RootWallHeight / 2f, maxZ - len / 2f),
                new Vector3(WallThickness, RootWallHeight, len), mats.RootWall, GameLayers.Obstacle);
            var depth = LevelHalfWidth - 7f + WallThickness;
            Box(nook, "KokDuvari_On", new Vector3(-8.5f, RootWallHeight / 2f, minZ),
                new Vector3(depth, RootWallHeight, WallThickness), mats.RootWall, GameLayers.Obstacle);
            Box(nook, "KokDuvari_Arka", new Vector3(-8.5f, RootWallHeight / 2f, maxZ),
                new Vector3(depth, RootWallHeight, WallThickness), mats.RootWall, GameLayers.Obstacle);

            var flowers = FindSceneObject("_Interactables/Flowers");
            if (flowers == null)
                flowers = zone;
            Box(flowers, "Cicek_GumusSumbul", new Vector3(3f, 0.25f, 38f), new Vector3(0.3f, 0.5f, 0.3f), mats.Interactable, GameLayers.Interactable);
            Box(flowers, "Cicek_GeceMenekse", new Vector3(-3f, 0.25f, 50f), new Vector3(0.3f, 0.5f, 0.3f), mats.Interactable, GameLayers.Interactable);
            Box(flowers, "Cicek_KizilDudukOtu", new Vector3(6f, 0.25f, 46f), new Vector3(0.3f, 0.5f, 0.3f), mats.Interactable, GameLayers.Interactable);
            Box(flowers, "Cicek_SisCicegi_Girintide", new Vector3(-9f, 0.25f, mid), new Vector3(0.3f, 0.5f, 0.3f), mats.Interactable, GameLayers.Interactable);
        }

        
        private static void BuildFireflyTree(Transform zone, Materials mats)
        {
            Ground(zone, "Zemin", new Vector3(0, 0, 62), new Vector2(LevelHalfWidth * 2, 16), mats.Ground);
            Box(zone, "AtesBocegiAgaci_Govde", new Vector3(5f, 2.5f, 62f), new Vector3(1.2f, 5f, 1.2f), mats.Tree, GameLayers.Obstacle);
        }

        
        private static void BuildForestHeart(Transform zone, Materials mats)
        {
            Ground(zone, "Zemin", new Vector3(0, 0, 80), new Vector2(LevelHalfWidth * 2, 20), mats.Ground);

            var meseRoot = FindSceneObject("_Characters/YasliMese");
            if (meseRoot == null)
                meseRoot = zone;
            Box(meseRoot, "YasliMese_Govde", new Vector3(-4f, 3f, 82f), new Vector3(3f, 6f, 3f), mats.Tree, GameLayers.Obstacle);

            const float doorZ = 88f, doorWidth = 3f, wallHeight = 4f;
            var sideLen = LevelHalfWidth - doorWidth / 2f;
            Box(zone, "KapiDuvari_Sol", new Vector3(-(doorWidth / 2f + sideLen / 2f), wallHeight / 2f, doorZ),
                new Vector3(sideLen, wallHeight, WallThickness), mats.RootWall, GameLayers.Obstacle);
            Box(zone, "KapiDuvari_Sag", new Vector3(doorWidth / 2f + sideLen / 2f, wallHeight / 2f, doorZ),
                new Vector3(sideLen, wallHeight, WallThickness), mats.RootWall, GameLayers.Obstacle);
            Box(zone, "MuhurluKapi", new Vector3(0, wallHeight / 2f, doorZ), new Vector3(doorWidth, wallHeight, WallThickness),
                mats.Door, GameLayers.Obstacle);
        }

        
        private static void BuildBoundary(Transform environment, Materials mats)
        {
            var boundary = Group(environment, "Sinir");
            const float minZ = -10f, maxZ = 90f, height = 1.5f;
            var length = maxZ - minZ;
            var midZ = (minZ + maxZ) / 2f;
            var x = LevelHalfWidth + WallThickness / 2f;
            Box(boundary, "Sinir_Sol", new Vector3(-x, height / 2f, midZ), new Vector3(WallThickness, height, length), mats.Boundary, GameLayers.Obstacle);
            Box(boundary, "Sinir_Sag", new Vector3(x, height / 2f, midZ), new Vector3(WallThickness, height, length), mats.Boundary, GameLayers.Obstacle);
            Box(boundary, "Sinir_Bas", new Vector3(0, height / 2f, minZ - WallThickness / 2f), new Vector3(x * 2, height, WallThickness), mats.Boundary, GameLayers.Obstacle);
            Box(boundary, "Sinir_Son", new Vector3(0, height / 2f, maxZ + WallThickness / 2f), new Vector3(x * 2, height, WallThickness), mats.Boundary, GameLayers.Obstacle);
        }

        
        private static void PlaceCharacters(Materials mats)
        {
            var cadi = FindSceneObject("_Characters/Cadi");
            if (cadi != null && cadi.childCount == 0)
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                Undo.RegisterCreatedObjectUndo(body, "Create Cadi_Govde");
                body.name = "Cadi_Govde";
                body.transform.SetParent(cadi, false);
                body.transform.localPosition = new Vector3(0, 1f, -5f);
                body.layer = GameLayers.Player;
                body.GetComponent<Renderer>().sharedMaterial = mats.Character;
            }

            var fisil = FindSceneObject("_Characters/Fisil");
            if (fisil != null && fisil.childCount == 0)
            {
                Box(fisil, "Fisil_Kitap", new Vector3(1.5f, 1.2f, -5f), new Vector3(0.3f, 0.4f, 0.1f), mats.Companion, GameLayers.Companion);
            }
        }

        
        private static void ExcludeDynamicObjectsFromNavMesh()
        {
            foreach (var path in new[] { "_Characters/Cadi", "_Characters/Fisil", "_Interactables", "_Environment/02_KokDuvari_Kopru/Kopru" })
            {
                var target = FindSceneObject(path);
                if (target == null)
                    continue;
                var modifier = target.GetComponent<NavMeshModifier>();
                if (modifier == null)
                    modifier = Undo.AddComponent<NavMeshModifier>(target.gameObject);
                modifier.ignoreFromBuild = true;
                modifier.applyToChildren = true;
            }
        }

        private static void AddSceneToBuildSettings()
        {
            var path = EditorSceneManager.GetActiveScene().path;
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[Graybox] Sahne henüz kaydedilmemiş; Build Settings'e eklenmedi. File → Build Profiles üzerinden elle ekle.");
                return;
            }

            var scenes = EditorBuildSettings.scenes.Where(s => s.path != path).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        

        private static void Ground(Transform parent, string name, Vector3 center, Vector2 size, Material mat)
        {
            
            Box(parent, name, new Vector3(center.x, -0.1f, center.z), new Vector3(size.x, 0.2f, size.y), mat, GameLayers.Ground);
        }

        private static GameObject Box(Transform parent, string name, Vector3 localPosition, Vector3 size, Material mat, int layer)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            go.layer = layer;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic);
            return go;
        }

        private static Transform Group(Transform parent, string name)
        {
            var go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        private static Transform FindSceneObject(string path)
        {
            var parts = path.Split('/');
            var root = EditorSceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == parts[0]);
            if (root == null)
                return null;
            return parts.Length == 1 ? root.transform : root.transform.Find(string.Join("/", parts.Skip(1)));
        }

        
        private sealed class Materials
        {
            public readonly Material Ground = Get("Graybox_Zemin", new Color(0.35f, 0.45f, 0.3f));
            public readonly Material RootWall = Get("Graybox_KokDuvari", new Color(0.3f, 0.2f, 0.15f));
            public readonly Material Bridge = Get("Graybox_Kopru", new Color(0.6f, 0.45f, 0.25f));
            public readonly Material Interactable = Get("Graybox_Etkilesim", new Color(0.9f, 0.75f, 0.2f));
            public readonly Material Door = Get("Graybox_Kapi", new Color(0.45f, 0.2f, 0.5f));
            public readonly Material Tree = Get("Graybox_Agac", new Color(0.25f, 0.35f, 0.2f));
            public readonly Material Boundary = Get("Graybox_Sinir", new Color(0.5f, 0.5f, 0.5f));
            public readonly Material Character = Get("Graybox_Cadi", new Color(0.2f, 0.3f, 0.8f));
            public readonly Material Companion = Get("Graybox_Fisil", new Color(0.85f, 0.85f, 1f));

            private static Material Get(string name, Color color)
            {
                var path = $"{MaterialFolder}/{name}.mat";
                var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (existing != null)
                    return existing;

                if (!AssetDatabase.IsValidFolder(MaterialFolder))
                    AssetDatabase.CreateFolder("Assets/_Project/Materials", "Graybox");

                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null)
                    shader = Shader.Find("Standard");
                var mat = new Material(shader) { name = name };
                mat.SetColor("_BaseColor", color);
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
                return mat;
            }
        }
    }
}
