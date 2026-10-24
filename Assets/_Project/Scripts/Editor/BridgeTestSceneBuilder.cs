using BuyuFisiltisi.Core.Navigation;
using BuyuFisiltisi.Gameplay.DebugTools;
using BuyuFisiltisi.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

namespace BuyuFisiltisi.EditorTools
{
    public static class BridgeTestSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Test/InciBridgeTest.unity";
        private const string MaterialFolder = "Assets/_Project/Materials/Graybox/";

        [MenuItem("Büyü Fısıltısı/Köprü Test Sahnesini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            if (!TryFindAgentType(AgentTypes.Cadi.Name, out var cadiId))
            {
                EditorUtility.DisplayDialog("Köprü Testi", $"'{AgentTypes.Cadi.Name}' Agent Type bulunamadı. Önce Proje Kurulum Kontrolü'nü çalıştır.", "Tamam");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null &&
                !EditorUtility.DisplayDialog("Köprü Testi", "InciBridgeTest sahnesi zaten var. Üzerine yazılsın mı?", "Evet", "Hayır"))
                return;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            Box(null, "Zemin_Yakin", new Vector3(0f, -0.1f, 10f), new Vector3(12f, 0.2f, 20f), "Graybox_Zemin", GameLayers.Ground);
            Box(null, "Zemin_Uzak", new Vector3(0f, -0.1f, 29f), new Vector3(12f, 0.2f, 10f), "Graybox_Zemin", GameLayers.Ground);

            var respawn = new GameObject("DonusNoktasi").transform;
            respawn.position = new Vector3(0f, 0f, 15f);

            var killZone = new GameObject("UcurumBolgesi");
            killZone.transform.position = new Vector3(0f, -6f, 22f);
            var killCollider = killZone.AddComponent<BoxCollider>();
            killCollider.isTrigger = true;
            killCollider.size = new Vector3(40f, 2f, 8f);
            killZone.AddComponent<ChasmKillZone>().RespawnPoint = respawn;

            var surfaceObject = new GameObject("NavMesh_Cadi");
            var surface = surfaceObject.AddComponent<NavMeshSurface>();
            surface.agentTypeID = cadiId;
            surface.layerMask = ~(GameLayers.Mask(GameLayers.Player) | GameLayers.Mask(GameLayers.Companion));

            var bridge = new GameObject("Kopru");
            bridge.transform.SetPositionAndRotation(new Vector3(0f, 0f, 24f), Quaternion.Euler(90f, 0f, 0f));
            Box(bridge.transform, "Kopru_Tabla", new Vector3(0f, -0.1f, -2.2f), new Vector3(4f, 0.2f, 4.4f), "Graybox_Kopru", GameLayers.Obstacle);
            var modifier = bridge.AddComponent<NavMeshModifier>();
            modifier.ignoreFromBuild = true;
            modifier.applyToChildren = true;
            var controller = bridge.AddComponent<BridgeController>();
            var controllerData = new SerializedObject(controller);
            controllerData.FindProperty("_pivot").objectReferenceValue = bridge.transform;
            controllerData.FindProperty("_navMeshModifier").objectReferenceValue = modifier;
            var surfaces = controllerData.FindProperty("_surfaces");
            surfaces.arraySize = 1;
            surfaces.GetArrayElementAtIndex(0).objectReferenceValue = surface;
            controllerData.ApplyModifiedPropertiesWithoutUndo();

            var switchRoot = new GameObject("BridgeSwitch");
            switchRoot.transform.position = new Vector3(-4f, 0f, 15f);
            Box(switchRoot.transform, "Salter_Govde", new Vector3(0f, 0.6f, 0f), new Vector3(0.3f, 1.2f, 0.3f), "Graybox_Etkilesim", GameLayers.Interactable);
            var handle = new GameObject("Salter_Kol").transform;
            handle.SetParent(switchRoot.transform, false);
            handle.localPosition = new Vector3(0f, 1.2f, 0f);
            handle.localRotation = Quaternion.Euler(0f, 0f, 30f);
            Box(handle, "Salter_Kol_Cubuk", new Vector3(0f, 0.3f, 0f), new Vector3(0.1f, 0.6f, 0.1f), "Graybox_Etkilesim", GameLayers.Interactable);
            var interactionPoint = new GameObject("EtkilesimNoktasi").transform;
            interactionPoint.SetParent(switchRoot.transform, false);
            interactionPoint.localPosition = new Vector3(0.9f, 0f, 0f);
            var bridgeSwitch = switchRoot.AddComponent<BridgeSwitch>();
            var switchData = new SerializedObject(bridgeSwitch);
            switchData.FindProperty("_interactionPoint").objectReferenceValue = interactionPoint;
            switchData.FindProperty("_focusPoint").objectReferenceValue = handle;
            switchData.FindProperty("_handle").objectReferenceValue = handle;
            switchData.ApplyModifiedPropertiesWithoutUndo();

            var walker = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            walker.name = "Yuruyucu";
            walker.layer = GameLayers.Player;
            walker.transform.position = new Vector3(0f, 1f, 12f);
            ApplyMaterial(walker, "Graybox_Cadi");
            var agent = walker.AddComponent<NavMeshAgent>();
            agent.agentTypeID = cadiId;
            agent.radius = AgentTypes.Cadi.Radius;
            agent.height = AgentTypes.Cadi.Height;
            agent.baseOffset = 1f;
            agent.speed = 3.5f;
            walker.SetActive(false);

            var goal = new GameObject("Hedef").transform;
            goal.position = new Vector3(0f, 0f, 30f);

            var rig = new GameObject("TestDuzenegi").AddComponent<BridgeTestRig>();
            var rigData = new SerializedObject(rig);
            rigData.FindProperty("_switch").objectReferenceValue = bridgeSwitch;
            rigData.FindProperty("_surface").objectReferenceValue = surface;
            rigData.FindProperty("_walker").objectReferenceValue = agent;
            rigData.FindProperty("_walkerGoal").objectReferenceValue = goal;
            rigData.ApplyModifiedPropertiesWithoutUndo();

            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = new Vector3(14f, 9f, 8f);
                camera.transform.LookAt(new Vector3(0f, 1f, 20f));
            }

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log("[Köprü Testi] Sahne kuruldu: " + ScenePath + ". Play'e bas, sonra B tuşuna bas.");
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
