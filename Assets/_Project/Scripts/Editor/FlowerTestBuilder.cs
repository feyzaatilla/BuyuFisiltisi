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

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Çiçek] {flowers.Length} çiçek ve çanta {scene.name} sahnesine eklendi. Play'e bas, V tuşuna bas.");
        }

        private static void Clear(GameObject witch)
        {
            DestroyNamed("Ciceker");
            DestroyNamed("CicekEmri");

            var inventory = witch.GetComponent<PlayerInventory>();
            if (inventory != null)
                Object.DestroyImmediate(inventory);

            var hud = witch.GetComponent<InventoryHud>();
            if (hud != null)
                Object.DestroyImmediate(hud);
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
