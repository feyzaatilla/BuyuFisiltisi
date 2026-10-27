using BuyuFisiltisi.Gameplay.Forest;
using BuyuFisiltisi.Gameplay.Items;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BuyuFisiltisi.EditorTools
{
    public static class FlowerTestBuilder
    {
        private const string TestSceneFolder = "Assets/_Project/Scenes/Test/";
        private const string MaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Etkilesim.mat";
        private const string Title = "Çiçek Testi";

        private static readonly Vector3[] FlowerPositions =
        {
            new Vector3(-4f, 0.4f, 6f),
            new Vector3(3f, 0.4f, 10f),
            new Vector3(-3f, 0.4f, 15f)
        };

        [MenuItem("Büyü Fısıltısı/Çiçek Test Düzenini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (!scene.path.StartsWith(TestSceneFolder))
            {
                EditorUtility.DisplayDialog(Title, "Bu sahne test sahnesi değil, çiçekler eklenmedi.", "Tamam");
                return;
            }

            var witch = GameObject.Find("Cadi");
            if (witch == null)
            {
                EditorUtility.DisplayDialog(Title, "Sahnede 'Cadi' bulunamadı.", "Tamam");
                return;
            }

            Clear(witch);

            var inventory = witch.AddComponent<PlayerInventory>();
            witch.AddComponent<InventoryHud>();
            witch.AddComponent<BrewKeyboardInput>();

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            var root = new GameObject("Ciceker").transform;
            var flowers = new CollectibleFlower[FlowerPositions.Length];

            for (var i = 0; i < FlowerPositions.Length; i++)
                flowers[i] = CreateFlower(root, i, inventory, material);

            var input = new GameObject("CicekEmri").AddComponent<FlowerFetchKeyboardInput>();
            var inputData = new SerializedObject(input);
            var list = inputData.FindProperty("_flowers");
            list.arraySize = flowers.Length;
            for (var i = 0; i < flowers.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = flowers[i];
            inputData.ApplyModifiedPropertiesWithoutUndo();

            CreateForest(inventory, material);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Çiçek] {flowers.Length} çiçek ve çanta {scene.name} sahnesine eklendi. Play'e bas, V, T ve H tuşlarını dene.");
        }

        private static void Clear(GameObject witch)
        {
            DestroyNamed("Ciceker");
            DestroyNamed("CicekEmri");
            DestroyNamed("AtesBocegi");
            DestroyNamed("AtesBocegiEmri");
            DestroyNamed("OrmanIsigi");
            DestroyNamed("KaranlikPerde");

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

        private static void CreateForest(PlayerInventory inventory, Material material)
        {
            var position = new Vector3(2f, 0.8f, 19f);

            var fireflyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            fireflyObject.name = "AtesBocegi";
            fireflyObject.transform.position = position;
            fireflyObject.transform.localScale = Vector3.one * 0.4f;
            Object.DestroyImmediate(fireflyObject.GetComponent<SphereCollider>());
            fireflyObject.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            if (material != null)
                fireflyObject.GetComponent<Renderer>().sharedMaterial = material;

            var point = new GameObject("DurmaNoktasi").transform;
            point.SetParent(fireflyObject.transform, false);
            point.position = new Vector3(position.x, 0f, position.z);

            var firefly = fireflyObject.AddComponent<Firefly>();
            var fireflyData = new SerializedObject(firefly);
            fireflyData.FindProperty("_inventory").objectReferenceValue = inventory;
            fireflyData.FindProperty("_interactionPoint").objectReferenceValue = point;
            fireflyData.ApplyModifiedPropertiesWithoutUndo();

            var curtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
            curtain.name = "KaranlikPerde";
            curtain.transform.position = new Vector3(0f, 1.5f, 28f);
            curtain.transform.localScale = new Vector3(10f, 3f, 0.5f);
            Object.DestroyImmediate(curtain.GetComponent<BoxCollider>());
            curtain.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            if (material != null)
                curtain.GetComponent<Renderer>().sharedMaterial = material;

            var forestLight = new GameObject("OrmanIsigi");
            var lightObject = new GameObject("KaranlikIsik");
            lightObject.transform.SetParent(forestLight.transform, false);
            lightObject.transform.position = new Vector3(0f, 4f, 24f);
            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.5f);
            light.intensity = 40f;
            light.range = 25f;

            var controller = forestLight.AddComponent<ForestLightController>();
            var controllerData = new SerializedObject(controller);
            var lights = controllerData.FindProperty("_lights");
            lights.arraySize = 1;
            lights.GetArrayElementAtIndex(0).objectReferenceValue = light;
            var darkness = controllerData.FindProperty("_darknessObjects");
            darkness.arraySize = 1;
            darkness.GetArrayElementAtIndex(0).objectReferenceValue = curtain;
            controllerData.ApplyModifiedPropertiesWithoutUndo();

            var input = new GameObject("AtesBocegiEmri").AddComponent<FireflyKeyboardInput>();
            var inputData = new SerializedObject(input);
            inputData.FindProperty("_firefly").objectReferenceValue = firefly;
            inputData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void DestroyNamed(string objectName)
        {
            var found = GameObject.Find(objectName);
            if (found != null)
                Object.DestroyImmediate(found);
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
    }
}
