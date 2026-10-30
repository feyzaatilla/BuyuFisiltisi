using BuyuFisiltisi.Core.Items;
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
        private const string CaveMaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Magara.mat";
        private const string ModelFolder = "Assets/_Project/Art/kenney_nature-kit/Models/FBX format/";
        private const string GlowMaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Isilti.mat";
        private const string LavaMaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Lav.mat";
        private const string Title = "Orman Oynanışı";
        private const string WitchPath = "_Characters/Cadi";
        private const string RootName = "_Oynanis";

        private static readonly Vector3[] FlowerPositions =
        {
            new Vector3(-4f, 0.4f, 38f),
            new Vector3(3f, 0.4f, 42f),
            new Vector3(0f, 0.9f, 52f),
            new Vector3(-5f, 4.1f, 64.9f),
            new Vector3(5f, 0.4f, 44f),
            new Vector3(-9f, 0.4f, 44f)
        };

        private static readonly string[] FlowerItems =
        {
            ItemCatalog.SilverHyacinth, ItemCatalog.RootPomegranate, ItemCatalog.SilverHyacinth,
            ItemCatalog.NightViolet, ItemCatalog.RedWhistleWeed, ItemCatalog.MistFlower
        };

        private static readonly string[] FlowerModels =
        {
            "flower_purpleB", "flower_redC", "flower_purpleB", "flower_purpleC", "flower_redA", "flower_yellowC"
        };

        private static readonly Vector3 RootPosition = new Vector3(-1.2f, 0f, 80f);

        private static readonly Vector3[] DecoySpots =
        {
            new Vector3(5f, 0.4f, 44f),
            new Vector3(-5f, 0.4f, 44f),
            new Vector3(5f, 0.4f, 40f)
        };

        private const int MountainFlowerIndex = 3;
        private const int DecoyFlowerIndex = 4;
        private const int NookFlowerIndex = 5;

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

            flowers[0].gameObject.AddComponent<FlowerPickupOnTouch>();
            flowers[1].gameObject.AddComponent<FlowerPickupOnTouch>();
            var reachable = new[] { flowers[NookFlowerIndex] };

            var fetch = new GameObject("CicekEmri").AddComponent<FlowerFetchKeyboardInput>();
            fetch.transform.SetParent(root, false);
            var fetchData = new SerializedObject(fetch);
            var list = fetchData.FindProperty("_flowers");
            list.arraySize = reachable.Length;
            for (var i = 0; i < reachable.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = reachable[i];
            fetchData.ApplyModifiedPropertiesWithoutUndo();

            CreateClouds(root, body != null ? body : witch.transform);
            CreateCave(root);
            CreateForestDecor(root);
            CreateDeepForest(root);
            TuneCamera();
            new GameObject("SisKontrol").transform.SetParent(root, false);
            root.Find("SisKontrol").gameObject.AddComponent<CloudFog>();
            new GameObject("KaranlikKontrol").transform.SetParent(root, false);
            root.Find("KaranlikKontrol").gameObject.AddComponent<ForestDarkness>();

            new GameObject("GorevRehberi").transform.SetParent(root, false);
            root.Find("GorevRehberi").gameObject.AddComponent<QuestTracker>();

            var firefly = CreateFirefly(root, inventory, material);
            CreateForestLight(root);

            CreateWoundedRoot(root, inventory);

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
            DressFlower(go.transform, index, position);

            var point = new GameObject("DurmaNoktasi").transform;
            point.SetParent(go.transform, false);
            point.position = new Vector3(position.x, 0f, position.z);

            var flower = go.AddComponent<CollectibleFlower>();
            var data = new SerializedObject(flower);
            data.FindProperty("_interactionId").stringValue = "cicek_" + (index + 1);
            data.FindProperty("_itemId").stringValue = FlowerItems[index];
            data.FindProperty("_displayName").stringValue = ItemCatalog.GetName(FlowerItems[index]);
            data.FindProperty("_inventory").objectReferenceValue = inventory;
            data.FindProperty("_interactionPoint").objectReferenceValue = point;
            data.ApplyModifiedPropertiesWithoutUndo();
            return flower;
        }

        private static void DressFlower(Transform flower, int index, Vector3 position)
        {
            Object.DestroyImmediate(flower.GetComponent<MeshRenderer>());
            Object.DestroyImmediate(flower.GetComponent<MeshFilter>());

            PlaceModel(flower, FlowerModels[index], new Vector3(position.x, position.y - 0.4f, position.z), index * 47f, 0.9f);

            var glow = new GameObject("Isilti");
            glow.transform.SetParent(flower, false);
            glow.transform.position = position + Vector3.up * 0.4f;
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.9f, 0.45f);
            light.intensity = 3f;
            light.range = 2.5f;
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

        private static Material GetCaveMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(CaveMaterialPath);
            if (material != null)
                return material;

            var source = AssetDatabase.LoadAssetAtPath<Material>(CloudMaterialPath);
            material = source != null ? new Material(source) : new Material(Shader.Find("Universal Render Pipeline/Lit"));
            var color = new Color(0.12f, 0.12f, 0.15f);
            material.color = color;
            material.SetColor("_BaseColor", color);
            AssetDatabase.CreateAsset(material, CaveMaterialPath);
            return material;
        }

        private static void CreateCave(Transform root)
        {
            var cave = new GameObject("Magara").transform;
            cave.SetParent(root, false);

            PlaceModel(cave, "cliff_cave_rock", new Vector3(10.3f, 0f, 15f), -90f, 6.4f);
            PlaceModel(cave, "cliff_large_rock", new Vector3(9f, 0f, 11.4f), 90f, 4.2f);
            PlaceModel(cave, "cliff_large_rock", new Vector3(9f, 0f, 18.6f), 90f, 4.2f);
            PlaceModel(cave, "rock_tallA", new Vector3(7.2f, 0f, 11.6f), 20f, 2.6f);
            PlaceModel(cave, "rock_tallC", new Vector3(7.2f, 0f, 18.4f), 160f, 2.6f);
            PlaceModel(cave, "rock_largeB", new Vector3(6.6f, 0f, 13.4f), 40f, 1.4f);
            PlaceModel(cave, "rock_smallA", new Vector3(7.4f, 0f, 16.6f), 75f, 0.8f);
            PlaceModel(cave, "stone_tallB", new Vector3(5.8f, 0f, 10.6f), 10f, 1.8f);
            PlaceModel(cave, "stone_tallD", new Vector3(5.8f, 0f, 19.4f), 200f, 1.8f);

            CreateCaveLight(cave, "MagaraIsigi", new Vector3(8.5f, 1.8f, 15f), new Color(0.35f, 0.5f, 1f), 12f, 7f, 0.25f);
            CreateCaveLight(cave, "MesaleIsigi", new Vector3(9.4f, 1.6f, 13f), new Color(1f, 0.55f, 0.2f), 14f, 6f, 0.5f);
        }

        private static void CreateForestDecor(Transform root)
        {
            var decor = new GameObject("Suslemeler").transform;
            decor.SetParent(root, false);

            var random = new System.Random(7);
            var trees = new[] { "tree_default_dark", "tree_detailed_dark", "tree_oak_dark", "tree_fat_darkh", "tree_pineTallA", "tree_pineTallC" };
            var props = new[] { "stump_old", "stump_oldTall", "log", "log_large", "mushroom_redGroup", "mushroom_tanGroup" };

            for (var z = 2f; z < 84f; z += 4.5f)
            {
                foreach (var side in new[] { -1f, 1f })
                {
                    var x = side * (8.6f + (float)random.NextDouble() * 1.2f);
                    var zz = z + (float)random.NextDouble() * 2f;
                    if (x > 5f && zz > 9f && zz < 21f)
                        continue;

                    PlaceModel(decor, trees[random.Next(trees.Length)], new Vector3(x, 0f, zz), random.Next(360), 4f + (float)random.NextDouble() * 2.5f);
                }
            }

            for (var z = 6f; z < 80f; z += 7f)
            {
                var side = random.Next(2) == 0 ? -1f : 1f;
                var x = side * (6.8f + (float)random.NextDouble() * 1.4f);
                var zz = z + (float)random.NextDouble() * 3f;
                if (x > 5f && zz > 9f && zz < 21f)
                    continue;

                PlaceModel(decor, props[random.Next(props.Length)], new Vector3(x, 0f, zz), random.Next(360), 1.1f);
            }
        }

        private static void CreateDeepForest(Transform root)
        {
            var forest = new GameObject("DerinOrman").transform;
            forest.SetParent(root, false);

            foreach (var name in new[] { "Sinir_Sol", "Sinir_Sag", "Sinir_Bas", "Sinir_Son", "AtesBocegiAgaci_Govde", "YasliMese_Govde", "KapiDuvari_Sol", "KapiDuvari_Sag", "Cicek_GumusSumbul", "Cicek_GeceMenekse", "Cicek_KizilDudukOtu", "Cicek_SisCicegi_Girintide" })
                HideRenderer(name);
            foreach (var wall in Object.FindObjectsByType<Transform>(FindObjectsSortMode.None))
            {
                if (wall.name.StartsWith("KokDuvari_") && wall.GetComponent<Renderer>() != null)
                    wall.GetComponent<Renderer>().enabled = false;
            }

            CreateHedges(forest);
            CreateFireflySwarm(forest);

            var groundSource = GameObject.Find("Zemin");
            var outer = GameObject.CreatePrimitive(PrimitiveType.Plane);
            outer.name = "DisZemin";
            outer.transform.SetParent(forest, false);
            outer.transform.position = new Vector3(0f, -0.02f, 44f);
            outer.transform.localScale = new Vector3(8f, 1f, 14f);
            Object.DestroyImmediate(outer.GetComponent<Collider>());
            if (groundSource != null && groundSource.GetComponent<Renderer>() != null)
                outer.GetComponent<Renderer>().sharedMaterial = groundSource.GetComponent<Renderer>().sharedMaterial;

            var random = new System.Random(21);
            var trees = new[] { "tree_pineTallA", "tree_pineTallB", "tree_pineTallC", "tree_pineTallD", "tree_default_dark", "tree_detailed_dark", "tree_oak_dark", "tree_fat_darkh", "tree_pineRoundC" };
            var ground = new[] { "grass_large", "grass_leafsLarge", "plant_bushLarge", "plant_bushDetailed", "plant_flatTall", "mushroom_tanTall", "rock_smallA", "stump_round" };

            for (var row = 0; row < 4; row++)
            {
                for (var z = -6f; z < 96f; z += 2.6f)
                {
                    foreach (var side in new[] { -1f, 1f })
                    {
                        var x = side * (10.8f + row * 2.4f + (float)random.NextDouble() * 1.4f);
                        var zz = z + (float)random.NextDouble() * 2f;
                        PlaceModel(forest, trees[random.Next(trees.Length)], new Vector3(x, 0f, zz), random.Next(360), 5.5f + row * 1.2f + (float)random.NextDouble() * 3f);
                    }
                }
            }

            for (var row = 0; row < 3; row++)
            {
                for (var x = -16f; x <= 16f; x += 2.4f)
                {
                    PlaceModel(forest, trees[random.Next(trees.Length)], new Vector3(x + (float)random.NextDouble(), 0f, -6f - row * 2.4f), random.Next(360), 6f + (float)random.NextDouble() * 3f);
                    PlaceModel(forest, trees[random.Next(trees.Length)], new Vector3(x + (float)random.NextDouble(), 0f, 92f + row * 2.4f), random.Next(360), 6f + (float)random.NextDouble() * 3f);
                }
            }

            for (var i = 0; i < 140; i++)
            {
                var x = -9.5f + (float)random.NextDouble() * 19f;
                var z = -3f + (float)random.NextDouble() * 90f;
                if (Mathf.Abs(x) < 3.5f)
                    continue;
                if (x > 6.5f && z > 10f && z < 20f)
                    continue;
                if (z > 19f && z < 31f)
                    continue;

                PlaceModel(forest, ground[random.Next(ground.Length)], new Vector3(x, 0f, z), random.Next(360), 0.5f + (float)random.NextDouble() * 0.7f);
            }

            PlaceModel(forest, "tree_oak_dark", new Vector3(5f, 0f, 62f), 30f, 9f);
            PlaceModel(forest, "tree_fat_darkh", new Vector3(-4f, 0f, 82f), 0f, 10f);
        }

        private static Material GetGlowMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(GlowMaterialPath);
            if (material != null)
                return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = new Color(1f, 0.9f, 0.35f);
            AssetDatabase.CreateAsset(material, GlowMaterialPath);
            return material;
        }

        private static void CreateFireflySwarm(Transform root)
        {
            var swarm = new GameObject("AtesBocekleri").transform;
            swarm.SetParent(root, false);

            var random = new System.Random(5);
            var material = GetGlowMaterial();
            for (var i = 0; i < 40; i++)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                go.name = "Bocek_" + (i + 1);
                go.transform.SetParent(swarm, false);
                var x = -9f + (float)random.NextDouble() * 18f;
                var z = -2f + (float)random.NextDouble() * 88f;
                go.transform.position = new Vector3(x, 0.6f + (float)random.NextDouble() * 2.2f, z);
                go.transform.localScale = Vector3.one * 0.07f;
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.GetComponent<Renderer>().sharedMaterial = material;
                go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                go.AddComponent<DriftMotion>();
            }
        }

        private static void CreateHedges(Transform root)
        {
            var hedge = new GameObject("Calilar").transform;
            hedge.SetParent(root, false);

            var random = new System.Random(11);
            var bushes = new[] { "plant_bushLarge", "plant_bushDetailed", "plant_bush", "rock_largeA", "rock_largeC" };
            for (var z = 41f; z <= 47f; z += 0.9f)
            {
                if (Mathf.Abs(z - 44f) < 0.6f)
                    continue;
                PlaceModel(hedge, bushes[random.Next(bushes.Length)], new Vector3(-7f, 0f, z), random.Next(360), 2.2f);
            }

            for (var x = -10f; x <= -7f; x += 0.9f)
            {
                PlaceModel(hedge, bushes[random.Next(bushes.Length)], new Vector3(x, 0f, 41f), random.Next(360), 2.2f);
                PlaceModel(hedge, bushes[random.Next(bushes.Length)], new Vector3(x, 0f, 47f), random.Next(360), 2.2f);
            }

            for (var x = -10f; x <= 10f; x += 1.1f)
            {
                if (Mathf.Abs(x) < 2.8f)
                    continue;
                PlaceModel(hedge, "cliff_block_rock", new Vector3(x, 0f, 89.6f), random.Next(4) * 90f, 2.6f);
            }
        }

        private static void TuneCamera()
        {
            var camera = Object.FindFirstObjectByType<BuyuFisiltisi.Gameplay.Player.ThirdPersonCamera>();
            if (camera == null)
                return;

            var data = new SerializedObject(camera);
            data.FindProperty("_targetHeight").floatValue = 1.6f;
            data.FindProperty("_startPitch").floatValue = 14f;
            data.FindProperty("_startDistance").floatValue = 4.5f;
            data.FindProperty("_maxDistance").floatValue = 9f;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateWoundedRoot(Transform root, PlayerInventory inventory)
        {
            var go = new GameObject("YaraliKok");
            go.transform.SetParent(root, false);
            go.transform.position = RootPosition;
            PlaceModel(go.transform, "log_large", RootPosition, 70f, 2.4f);

            var glow = new GameObject("YaraIsigi");
            glow.transform.SetParent(go.transform, false);
            glow.transform.position = RootPosition + Vector3.up * 0.8f;
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.9f, 0.15f, 0.2f);
            light.intensity = 5f;
            light.range = 3.5f;
            glow.AddComponent<FlickerLight>();

            var wound = go.AddComponent<WoundedRoot>();
            var data = new SerializedObject(wound);
            data.FindProperty("_inventory").objectReferenceValue = inventory;
            data.FindProperty("_woundLight").objectReferenceValue = light;
            data.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void HideRenderer(string name)
        {
            var target = GameObject.Find(name);
            if (target != null && target.GetComponent<Renderer>() != null)
                target.GetComponent<Renderer>().enabled = false;
        }

        private static void PlaceModel(Transform parent, string modelName, Vector3 position, float yaw, float targetSize)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + modelName + ".fbx");
            if (asset == null)
            {
                Debug.LogWarning("[Orman] Model bulunamadı: " + modelName);
                return;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = modelName;
            instance.transform.position = position;
            instance.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            var largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largest > 0.0001f)
                instance.transform.localScale = Vector3.one * (targetSize / largest);

            bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            instance.transform.position += new Vector3(0f, position.y - bounds.min.y, 0f);
        }

        private static void CreateCaveLight(Transform parent, string name, Vector3 position, Color color, float intensity, float range, float flicker)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var light = go.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = color;
            light.intensity = intensity;
            light.range = range;

            var flickerLight = go.AddComponent<FlickerLight>();
            var data = new SerializedObject(flickerLight);
            data.FindProperty("_amount").floatValue = flicker;
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
            var pool = new GameObject("AtesHavuzu").transform;
            pool.SetParent(root, false);

            var respawn = new GameObject("AlevDonusNoktasi").transform;
            respawn.SetParent(pool, false);
            respawn.position = FlameRespawn;

            var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lava.name = "Lav";
            lava.transform.SetParent(pool, false);
            lava.transform.position = new Vector3(center.x, 0f, center.z - 0.6f);
            lava.transform.localScale = new Vector3(6f, 0.3f, 5f);
            lava.GetComponent<Renderer>().sharedMaterial = GetLavaMaterial();
            lava.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lava.GetComponent<BoxCollider>().isTrigger = true;
            lava.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            lava.AddComponent<ChasmKillZone>().RespawnPoint = respawn;

            var stone = GetCaveMaterial();
            CreateStone(pool, "Tas_1", new Vector3(center.x - 1.1f, 0.25f, center.z - 2.2f), new Vector3(1f, 0.5f, 1f), stone);
            CreateStone(pool, "Tas_2", new Vector3(center.x + 0.9f, 0.25f, center.z - 1.1f), new Vector3(0.9f, 0.5f, 0.9f), stone);
            CreateStone(pool, "Ada", new Vector3(center.x, 0.25f, center.z), new Vector3(1.4f, 0.5f, 1.4f), stone);

            var glow = new GameObject("LavIsigi");
            glow.transform.SetParent(pool, false);
            glow.transform.position = new Vector3(center.x, 1.2f, center.z - 0.6f);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.4f, 0.1f);
            light.intensity = 10f;
            light.range = 7f;
            var flicker = new SerializedObject(glow.AddComponent<FlickerLight>());
            flicker.FindProperty("_amount").floatValue = 0.4f;
            flicker.ApplyModifiedPropertiesWithoutUndo();

            var random = new System.Random(3);
            for (var i = 0; i < 14; i++)
            {
                var angle = i / 14f * Mathf.PI * 2f;
                var edge = new Vector3(center.x + Mathf.Cos(angle) * 3.4f, 0f, center.z - 0.6f + Mathf.Sin(angle) * 2.9f);
                if (Mathf.Sin(angle) < -0.7f && Mathf.Abs(Mathf.Cos(angle)) < 0.5f)
                    continue;
                PlaceModel(pool, random.Next(2) == 0 ? "rock_smallA" : "rock_smallC", edge, random.Next(360), 0.7f);
            }
        }

        private static void CreateStone(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            go.transform.localScale = size;
            go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;
            if (material != null)
                go.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Material GetLavaMaterial()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(LavaMaterialPath);
            if (material != null)
                return material;

            material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            material.color = new Color(1f, 0.35f, 0.05f);
            AssetDatabase.CreateAsset(material, LavaMaterialPath);
            return material;
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

            go.transform.localScale = Vector3.one * 0.18f;
            go.GetComponent<Renderer>().sharedMaterial = GetGlowMaterial();

            var glow = new GameObject("Isilti");
            glow.transform.SetParent(go.transform, false);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.3f);
            light.intensity = 4f;
            light.range = 3f;
            var flicker = new SerializedObject(glow.AddComponent<FlickerLight>());
            flicker.FindProperty("_amount").floatValue = 0.6f;
            flicker.ApplyModifiedPropertiesWithoutUndo();

            var drift = new SerializedObject(go.AddComponent<DriftMotion>());
            drift.FindProperty("_amplitude").floatValue = 0.12f;
            drift.ApplyModifiedPropertiesWithoutUndo();

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
