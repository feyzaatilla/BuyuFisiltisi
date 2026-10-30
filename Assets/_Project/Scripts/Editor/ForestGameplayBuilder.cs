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
        private const string CloudMaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Sinir.mat";
        private const string Title = "Orman Oynanışı";
        private const string WitchPath = "_Characters/Cadi";
        private const string RootName = "_Oynanis";

        private static readonly Vector3[] FlowerPositions =
        {
            new Vector3(-4f, 0.4f, 38f),
            new Vector3(3f, 0.4f, 42f),
            new Vector3(0f, 0.4f, 52f),
            new Vector3(-5f, 4.1f, 64.9f),
            new Vector3(5f, 0.4f, 44f)
        };

        private static readonly Vector3[] DecoySpots =
        {
            new Vector3(5f, 0.4f, 44f),
            new Vector3(-5f, 0.4f, 44f),
            new Vector3(5f, 0.4f, 40f)
        };

        private const int MountainFlowerIndex = 3;
        private const int DecoyFlowerIndex = 4;

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

            flowers[MountainFlowerIndex].gameObject.AddComponent<FlowerPickupOnTouch>();
            CreateMountain(root, material);

            flowers[DecoyFlowerIndex].gameObject.AddComponent<FlowerPickupOnTouch>();
            var body = witch.transform.Find("Cadi_Govde");
            CreateDecoy(root, flowers[DecoyFlowerIndex], body != null ? body : witch.transform);

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

            CreateClouds(root, body != null ? body : witch.transform);

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
            Debug.Log("[Orman] Çiçekler, ateş böceği ve orman ışığı ForestScene sahnesine eklendi. R, V, T ve H tuşlarını dene.");
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

        private static void CreateClouds(Transform root, Transform player)
        {
            var cloudMaterial = AssetDatabase.LoadAssetAtPath<Material>(CloudMaterialPath);
            var clouds = new GameObject("KaraBulutlar").transform;
            clouds.SetParent(root, false);

            var positions = new[]
            {
                new Vector3(-3f, 3f, 0f),
                new Vector3(4f, 3.2f, 6f),
                new Vector3(-2f, 3.4f, 13f)
            };

            var list = new DarkCloud[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "KaraBulut_" + (i + 1);
                go.transform.SetParent(clouds, false);
                go.transform.position = positions[i];
                go.transform.localScale = new Vector3(3f, 1.2f, 2.5f);
                Object.DestroyImmediate(go.GetComponent<SphereCollider>());

                if (cloudMaterial != null)
                    go.GetComponent<Renderer>().sharedMaterial = cloudMaterial;

                list[i] = go.AddComponent<DarkCloud>();
            }

            var clearer = clouds.gameObject.AddComponent<CloudClearer>();
            var data = new SerializedObject(clearer);
            data.FindProperty("_player").objectReferenceValue = player;
            var cloudList = data.FindProperty("_clouds");
            cloudList.arraySize = list.Length;
            for (var i = 0; i < list.Length; i++)
                cloudList.GetArrayElementAtIndex(i).objectReferenceValue = list[i];
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateMountain(Transform root, Material material)
        {
            var mountain = new GameObject("Dag").transform;
            mountain.SetParent(root, false);

            for (var i = 0; i < 6; i++)
            {
                var height = 0.6f * (i + 1);
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name = "Basamak_" + (i + 1);
                go.transform.SetParent(mountain, false);
                go.transform.position = new Vector3(-5f, height / 2f, 55f + 1.8f * i + 0.9f);
                go.transform.localScale = new Vector3(2.5f, height, 1.8f);
                go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

                if (material != null)
                    go.GetComponent<Renderer>().sharedMaterial = material;
            }
        }

        private static void CreateDecoy(Transform root, CollectibleFlower flower, Transform player)
        {
            var spotsRoot = new GameObject("YalanciCicekNoktalari").transform;
            spotsRoot.SetParent(root, false);

            var spots = new Transform[DecoySpots.Length];
            for (var i = 0; i < DecoySpots.Length; i++)
            {
                spots[i] = new GameObject("Nokta_" + (i + 1)).transform;
                spots[i].SetParent(spotsRoot, false);
                spots[i].position = DecoySpots[i];
            }

            var elusive = flower.gameObject.AddComponent<ElusiveFlower>();
            var data = new SerializedObject(elusive);
            data.FindProperty("_player").objectReferenceValue = player;
            var list = data.FindProperty("_spots");
            list.arraySize = spots.Length;
            for (var i = 0; i < spots.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = spots[i];
            data.ApplyModifiedPropertiesWithoutUndo();
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
