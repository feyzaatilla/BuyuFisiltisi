using BuyuFisiltisi.Companion.Barks;
using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Companion.Visual;
using BuyuFisiltisi.Core.Navigation;
using BuyuFisiltisi.Gameplay.Commands;
using BuyuFisiltisi.Gameplay.DebugTools;
using BuyuFisiltisi.Gameplay.Player;
using BuyuFisiltisi.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace BuyuFisiltisi.EditorTools
{
    public static class WitchTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Test/InciCadiTest.unity";
        private const string FisilPrefabPath = "Assets/_Project/Prefabs/Fisil.prefab";
        private const string MaterialFolder = "Assets/_Project/Materials/Graybox/";
        private const string Title = "Cadı Testi";

        private const float WallHeight = 2f;
        private const float WallThickness = 0.3f;

        private static readonly Vector3 WitchStart = new Vector3(0f, 1f, 0f);

        [MenuItem("Büyü Fısıltısı/Cadı Test Sahnesini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            if (!TryFindAgentType(AgentTypes.Cadi.Name, out var cadiId) || !TryFindAgentType(AgentTypes.Fisil.Name, out var fisilId))
            {
                EditorUtility.DisplayDialog(Title, "Cadi ve Fisil Agent Type'ları bulunamadı. Önce Proje Kurulum Kontrolü'nü çalıştır.", "Tamam");
                return;
            }

            var fisilPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FisilPrefabPath);
            if (fisilPrefab == null || fisilPrefab.GetComponent<CompanionController>() == null)
            {
                EditorUtility.DisplayDialog(Title, "Fisil.prefab bulunamadı. Önce Fısıl Prefab'ını Oluştur menüsünü çalıştır.", "Tamam");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog(Title, "InciCadiTest sahnesi zaten var. Üzerine yazılsın mı?", "Evet", "Hayır"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Box(null, "Zemin_Yakin", new Vector3(0f, -0.1f, 8f), new Vector3(16f, 0.2f, 24f), "Graybox_Zemin", GameLayers.Ground);
            Box(null, "Zemin_Uzak", new Vector3(0f, -0.1f, 29f), new Vector3(16f, 0.2f, 10f), "Graybox_Zemin", GameLayers.Ground);

            var nook = new GameObject("SalterKosesi").transform;
            BuildNook(nook);

            var respawn = new GameObject("BaslangicNoktasi").transform;
            respawn.position = WitchStart;

            var killZone = new GameObject("UcurumBolgesi");
            killZone.transform.position = new Vector3(0f, -6f, 15f);
            var killCollider = killZone.AddComponent<BoxCollider>();
            killCollider.isTrigger = true;
            killCollider.size = new Vector3(200f, 2f, 200f);
            killZone.AddComponent<ChasmKillZone>().RespawnPoint = respawn;

            var excluded = ~(GameLayers.Mask(GameLayers.Player) | GameLayers.Mask(GameLayers.Companion));
            var cadiSurface = Surface("NavMesh_Cadi", cadiId, excluded);
            var fisilSurface = Surface("NavMesh_Fisil", fisilId, excluded);

            var bridge = new GameObject("Kopru");
            bridge.transform.SetPositionAndRotation(new Vector3(0f, 0f, 24f), Quaternion.Euler(90f, 0f, 0f));
            Box(bridge.transform, "Kopru_Tabla", new Vector3(0f, -0.1f, -2.2f), new Vector3(4f, 0.2f, 4.4f), "Graybox_Kopru", GameLayers.Obstacle);
            var modifier = bridge.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;
            modifier.applyToChildren = true;
            var bridgeData = new SerializedObject(bridge.AddComponent<BridgeController>());
            bridgeData.FindProperty("_pivot").objectReferenceValue = bridge.transform;
            bridgeData.FindProperty("_navMeshModifier").objectReferenceValue = modifier;
            var surfaces = bridgeData.FindProperty("_surfaces");
            surfaces.arraySize = 2;
            surfaces.GetArrayElementAtIndex(0).objectReferenceValue = cadiSurface;
            surfaces.GetArrayElementAtIndex(1).objectReferenceValue = fisilSurface;
            bridgeData.ApplyModifiedPropertiesWithoutUndo();

            var bridgeSwitch = BuildSwitch(new Vector3(7.8f, 0f, 14f));

            var witch = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            witch.name = "Cadi";
            witch.layer = GameLayers.Player;
            witch.transform.position = WitchStart;
            Object.DestroyImmediate(witch.GetComponent<CapsuleCollider>());
            ApplyMaterial(witch, "Graybox_Cadi");
            var characterController = witch.AddComponent<CharacterController>();
            characterController.radius = 0.4f;
            characterController.height = 2f;
            characterController.center = Vector3.zero;
            var motor = witch.AddComponent<WitchMotor>();

            var camera = Camera.main;
            if (camera != null)
            {
                var cameraRig = camera.gameObject.AddComponent<ThirdPersonCamera>();
                new SerializedObject(cameraRig).Apply("_target", witch.transform);
                new SerializedObject(motor).Apply("_cameraTransform", camera.transform);
                camera.transform.position = new Vector3(0f, 4f, -7f);
                camera.transform.LookAt(witch.transform.position);
            }

            var fisil = (GameObject)PrefabUtility.InstantiatePrefab(fisilPrefab);
            fisil.name = "Fisil";
            fisil.transform.position = new Vector3(1.5f, 0f, -1.5f);
            new SerializedObject(fisil.GetComponent<CompanionController>()).Apply("player", witch.transform);
            fisil.AddComponent<CompanionBarker>();
            var book = fisil.transform.Find("Fisil_Kitap");
            if (book != null)
                book.gameObject.AddComponent<HoverVisual>();
            fisil.SetActive(false);

            var commands = new GameObject("FisilEmirleri").AddComponent<CompanionCommandKeyboardInput>();
            new SerializedObject(commands).Apply("_switchTarget", bridgeSwitch);

            var rigData = new SerializedObject(new GameObject("TestDuzenegi").AddComponent<WitchTestRig>());
            var rigSurfaces = rigData.FindProperty("_surfaces");
            rigSurfaces.arraySize = 2;
            rigSurfaces.GetArrayElementAtIndex(0).objectReferenceValue = cadiSurface;
            rigSurfaces.GetArrayElementAtIndex(1).objectReferenceValue = fisilSurface;
            var activate = rigData.FindProperty("_activateAfterBake");
            activate.arraySize = 1;
            activate.GetArrayElementAtIndex(0).objectReferenceValue = fisil;
            rigData.FindProperty("_commandInput").objectReferenceValue = commands;
            rigData.FindProperty("_companion").objectReferenceValue = fisil.transform;
            rigData.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Cadı Testi] Sahne kuruldu: " + ScenePath + ". Play'e bas, WASD ile yürü, B ile Fısıl'a şalteri çektir.");
        }

        private static void BuildNook(Transform parent)
        {
            const float left = 5.5f;
            const float right = 8.7f;
            const float back = 12f;
            const float front = 16f;
            var gap = AgentTypes.RootWallGapWidth;
            var centerZ = (back + front) * 0.5f;
            var halfThickness = WallThickness * 0.5f;
            var width = right - left;
            var depth = front - back;

            Wall(parent, "Duvar_Arka", new Vector3(right + halfThickness, 0f, centerZ), new Vector3(WallThickness, WallHeight, depth + WallThickness * 2f));
            Wall(parent, "Duvar_Sol", new Vector3(left + width * 0.5f, 0f, back - halfThickness), new Vector3(width, WallHeight, WallThickness));
            Wall(parent, "Duvar_Sag", new Vector3(left + width * 0.5f, 0f, front + halfThickness), new Vector3(width, WallHeight, WallThickness));

            var segment = (depth + WallThickness * 2f - gap) * 0.5f;
            var frontX = left - halfThickness;
            Wall(parent, "Duvar_On_A", new Vector3(frontX, 0f, back - WallThickness + segment * 0.5f), new Vector3(WallThickness, WallHeight, segment));
            Wall(parent, "Duvar_On_B", new Vector3(frontX, 0f, front + WallThickness - segment * 0.5f), new Vector3(WallThickness, WallHeight, segment));
        }

        private static void Wall(Transform parent, string name, Vector3 basePosition, Vector3 size)
        {
            Box(parent, name, basePosition + Vector3.up * (size.y * 0.5f), size, "Graybox_KokDuvari", GameLayers.Obstacle);
        }

        private static BridgeSwitch BuildSwitch(Vector3 position)
        {
            var root = new GameObject("BridgeSwitch");
            root.transform.position = position;
            Box(root.transform, "Salter_Govde", new Vector3(0f, 0.6f, 0f), new Vector3(0.3f, 1.2f, 0.3f), "Graybox_Etkilesim", GameLayers.Interactable);
            var handle = new GameObject("Salter_Kol").transform;
            handle.SetParent(root.transform, false);
            handle.localPosition = new Vector3(0f, 1.2f, 0f);
            handle.localRotation = Quaternion.Euler(0f, 0f, 30f);
            Box(handle, "Salter_Kol_Cubuk", new Vector3(0f, 0.3f, 0f), new Vector3(0.1f, 0.6f, 0.1f), "Graybox_Etkilesim", GameLayers.Interactable);
            var point = new GameObject("EtkilesimNoktasi").transform;
            point.SetParent(root.transform, false);
            point.localPosition = new Vector3(-0.8f, 0f, 0f);

            var bridgeSwitch = root.AddComponent<BridgeSwitch>();
            var data = new SerializedObject(bridgeSwitch);
            data.FindProperty("_interactionPoint").objectReferenceValue = point;
            data.FindProperty("_focusPoint").objectReferenceValue = handle;
            data.FindProperty("_handle").objectReferenceValue = handle;
            data.ApplyModifiedPropertiesWithoutUndo();
            return bridgeSwitch;
        }

        private static NavMeshSurface Surface(string name, int agentTypeId, int layerMask)
        {
            var surface = new GameObject(name).AddComponent<NavMeshSurface>();
            surface.agentTypeID = agentTypeId;
            surface.layerMask = layerMask;
            return surface;
        }

        private static void Apply(this SerializedObject data, string property, Object value)
        {
            data.FindProperty(property).objectReferenceValue = value;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject Box(Transform parent, string name, Vector3 localPosition, Vector3 size, string material, int layer)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.layer = layer;
            if (parent != null)
                go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = size;
            ApplyMaterial(go, material);
            return go;
        }

        private static void ApplyMaterial(GameObject go, string material)
        {
            var asset = AssetDatabase.LoadAssetAtPath<Material>(MaterialFolder + material + ".mat");
            if (asset != null)
                go.GetComponent<Renderer>().sharedMaterial = asset;
        }

        private static bool TryFindAgentType(string agentTypeName, out int agentTypeId)
        {
            var count = NavMesh.GetSettingsCount();
            for (var i = 0; i < count; i++)
            {
                var id = NavMesh.GetSettingsByIndex(i).agentTypeID;
                if (NavMesh.GetSettingsNameFromID(id) == agentTypeName)
                {
                    agentTypeId = id;
                    return true;
                }
            }

            agentTypeId = 0;
            return false;
        }
    }
}
