using BuyuFisiltisi.Gameplay.Forest;
using BuyuFisiltisi.Gameplay.Items;
using BuyuFisiltisi.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuyuFisiltisi.EditorTools
{
    public static class ForestGameplayBuilder
    {
        private const string ForestSceneName = "ForestScene";
        private const string MaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Etkilesim.mat";
        private const string Title = "Orman Oynanışı";
        private const string WitchPath = "_Characters/Cadi";
        private const string RootName = "_Oynanis";

        private static readonly Vector3[] FlowerPositions =
        {
            new Vector3(-4f, 0.4f, 38f),
            new Vector3(3f, 0.4f, 42f),
            new Vector3(0f, 0.4f, 52f)
        };

        private const int HiddenFlowerIndex = 2;
        private static readonly Vector3 FlameRespawn = new Vector3(0f, 0.1f, 46f);

        private static readonly Vector3 FireflyPosition = new Vector3(2f, 0.8f, 60f);
        private static readonly Vector3 LightPosition = new Vector3(0f, 4f, 76f);

        [MenuItem("Büyü Fısıltısı/Orman Oynanış Düzenini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != ForestSceneName)
            {
                EditorUtility.DisplayDialog(Title, "Bu işlem sadece ForestScene sahnesinde çalışır.", "Tamam");
                return;
            }

            var witch = GameObject.Find(WitchPath);
            if (witch == null)
            {
                EditorUtility.DisplayDialog(Title, "Sahnede '" + WitchPath + "' bulunamadı.", "Tamam");
                return;
            }

            Clear(witch);

            var inventory = witch.AddComponent<PlayerInventory>();
            witch.AddComponent<InventoryHud>();
            witch.AddComponent<BrewKeyboardInput>();

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var root = new GameObject(RootName).transform;

            var flowers = new CollectibleFlower[FlowerPositions.Length];
            for (var i = 0; i < FlowerPositions.Length; i++)
                flowers[i] = CreateFlower(root, i, inventory, material);

            flowers[HiddenFlowerIndex].gameObject.AddComponent<FlowerPickupOnTouch>();
            CreateFlames(root, FlowerPositions[HiddenFlowerIndex], material);

            var reachable = new CollectibleFlower[HiddenFlowerIndex];
            for (var i = 0; i < reachable.Length; i++)
                reachable[i] = flowers[i];

            var fetch = new GameObject("CicekEmri").AddComponent<FlowerFetchKeyboardInput>();
            fetch.transform.SetParent(root, false);
            var fetchData = new SerializedObject(fetch);
            var list = fetchData.FindProperty("_flowers");
            list.arraySize = reachable.Length;
            for (var i = 0; i < reachable.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = reachable[i];
            fetchData.ApplyModifiedPropertiesWithoutUndo();

            new GameObject("GorevRehberi").transform.SetParent(root, false);
            root.Find("GorevRehberi").gameObject.AddComponent<QuestTracker>();

            var firefly = CreateFirefly(root, inventory, material);
            CreateForestLight(root);

            var fireflyInput = new GameObject("AtesBocegiEmri").AddComponent<FireflyKeyboardInput>();
            fireflyInput.transform.SetParent(root, false);
            var inputData = new SerializedObject(fireflyInput);
            inputData.FindProperty("_firefly").objectReferenceValue = firefly;
            inputData.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Orman] Çiçekler, ateş böceği ve orman ışığı ForestScene sahnesine eklendi. V, T ve H tuşlarını dene.");
        }

        private static void Clear(GameObject witch)
        {
            var old = GameObject.Find(RootName);
            if (old != null)
                Object.DestroyImmediate(old);

            var brew = witch.GetComponent<BrewKeyboardInput>();
            if (brew != null)
                Object.DestroyImmediate(brew);

            var hud = witch.GetComponent<InventoryHud>();
            if (hud != null)
                Object.DestroyImmediate(hud);

            var inventory = witch.GetComponent<PlayerInventory>();
            if (inventory != null)
                Object.DestroyImmediate(inventory);
        }

        private static CollectibleFlower CreateFlower(Transform root, int index, PlayerInventory inventory, Material material)
        {
            var position = FlowerPositions[index];

            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "IsikCicegi_" + (index + 1);
            go.transform.SetParent(root, false);
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.5f;
            Object.DestroyImmediate(go.GetComponent<SphereCollider>());

            if (material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;

            go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            var point = new GameObject("DurmaNoktasi").transform;
            point.SetParent(go.transform, false);
            point.position = new Vector3(position.x, 0f, position.z);

            var flower = go.AddComponent<CollectibleFlower>();
            var data = new SerializedObject(flower);
            data.FindProperty("_interactionId").stringValue = "isik_cicegi_" + (index + 1);
            data.FindProperty("_inventory").objectReferenceValue = inventory;
            data.FindProperty("_interactionPoint").objectReferenceValue = point;
            data.ApplyModifiedPropertiesWithoutUndo();
            return flower;
        }

        private static void CreateFlames(Transform root, Vector3 center, Material material)
        {
            var flames = new GameObject("AlevHalkasi").transform;
            flames.SetParent(root, false);

            var respawn = new GameObject("AlevDonusNoktasi").transform;
            respawn.SetParent(flames, false);
            respawn.position = FlameRespawn;

            CreateFlameBar(flames, "Alev_On", center + new Vector3(0f, 0.125f, -2.5f), new Vector3(6f, 0.25f, 0.8f), respawn, material);
            CreateFlameBar(flames, "Alev_Arka", center + new Vector3(0f, 0.125f, 2.5f), new Vector3(6f, 0.25f, 0.8f), respawn, material);
            CreateFlameBar(flames, "Alev_Sol", center + new Vector3(-2.5f, 0.125f, 0f), new Vector3(0.8f, 0.25f, 4f), respawn, material);
            CreateFlameBar(flames, "Alev_Sag", center + new Vector3(2.5f, 0.125f, 0f), new Vector3(0.8f, 0.25f, 4f), respawn, material);
        }

        private static void CreateFlameBar(Transform parent, string name, Vector3 position, Vector3 size, Transform respawn, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = size;
            go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            if (material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;

            go.GetComponent<BoxCollider>().isTrigger = true;
            var zone = go.AddComponent<ChasmKillZone>();
            zone.RespawnPoint = respawn;
        }

        private static Firefly CreateFirefly(Transform root, PlayerInventory inventory, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "AtesBocegi";
            go.transform.SetParent(root, false);
            go.transform.position = FireflyPosition;
            go.transform.localScale = Vector3.one * 0.4f;
            Object.DestroyImmediate(go.GetComponent<SphereCollider>());
            go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            if (material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;

            var point = new GameObject("DurmaNoktasi").transform;
            point.SetParent(go.transform, false);
            point.position = new Vector3(FireflyPosition.x, 0f, FireflyPosition.z);

            var firefly = go.AddComponent<Firefly>();
            var data = new SerializedObject(firefly);
            data.FindProperty("_inventory").objectReferenceValue = inventory;
            data.FindProperty("_interactionPoint").objectReferenceValue = point;
            data.ApplyModifiedPropertiesWithoutUndo();
            return firefly;
        }

        private static void CreateForestLight(Transform root)
        {
            var forestLight = new GameObject("OrmanIsigi");
            forestLight.transform.SetParent(root, false);

            var lightObject = new GameObject("KaranlikIsik");
            lightObject.transform.SetParent(forestLight.transform, false);
            lightObject.transform.position = LightPosition;

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.5f);
            light.intensity = 60f;
            light.range = 30f;

            var controller = forestLight.AddComponent<ForestLightController>();
            var data = new SerializedObject(controller);
            var lights = data.FindProperty("_lights");
            lights.arraySize = 1;
            lights.GetArrayElementAtIndex(0).objectReferenceValue = light;
            data.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
