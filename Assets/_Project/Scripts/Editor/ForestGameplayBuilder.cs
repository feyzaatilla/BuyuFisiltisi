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
        private const string KayKitFolder = "Assets/_Project/Art/KayKit_Adventurers_2.0_FREE/";
        private const string WitchControllerPath = "Assets/_Project/Art/Cadi.controller";
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
            new Vector3(-8.3f, 0.4f, 44f)
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

        private static readonly Vector3 NookFetchPoint = new Vector3(-7.5f, 0f, 44f);

        private static readonly Vector3 CauldronPosition = new Vector3(2.5f, 0f, 55.5f);

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

        private static readonly Vector3 OakPosition = new Vector3(-4f, 0f, 82f);

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

            CreateClouds(root, body != null ? body : witch.transform);
            CreateCave(root);
            CreateForestDecor(root);
            CreateDeepForest(root);
            TuneCamera();
            DressCharacters(body != null ? body : witch.transform);
            new GameObject("SisKontrol").transform.SetParent(root, false);
            root.Find("SisKontrol").gameObject.AddComponent<CloudFog>();
            new GameObject("KaranlikKontrol").transform.SetParent(root, false);
            root.Find("KaranlikKontrol").gameObject.AddComponent<ForestDarkness>();

            var intro = new GameObject("Giris").AddComponent<BuyuFisiltisi.UI.Intro.IntroView>();
            intro.transform.SetParent(root, false);
            var fisilObject = GameObject.Find("Fisil");
            var bookVisual = fisilObject != null ? fisilObject.transform.Find("Fisil_Kitap/KitapGorseli") : null;
            var introData = new SerializedObject(intro);
            introData.FindProperty("_companionVisual").objectReferenceValue = bookVisual;
            introData.FindProperty("_witch").objectReferenceValue = body != null ? body : witch.transform;
            introData.FindProperty("_cameraRig").objectReferenceValue = Object.FindFirstObjectByType<BuyuFisiltisi.Gameplay.Player.ThirdPersonCamera>();
            var introBook = CreateIntroBook(intro.transform);
            if (introBook != null)
            {
                introData.FindProperty("_introBook").objectReferenceValue = introBook;
                introData.FindProperty("_closedBook").objectReferenceValue = introBook.Find("Kapali")?.gameObject;
                introData.FindProperty("_openBook").objectReferenceValue = introBook.Find("Acik")?.gameObject;
            }
            introData.ApplyModifiedPropertiesWithoutUndo();

            new GameObject("BitisEkrani").transform.SetParent(root, false);
            root.Find("BitisEkrani").gameObject.AddComponent<BuyuFisiltisi.UI.Ending.EndScreenView>();

            new GameObject("GorevRehberi").transform.SetParent(root, false);
            root.Find("GorevRehberi").gameObject.AddComponent<QuestTracker>();

            var firefly = CreateFirefly(root, inventory, material);
            CreateForestLight(root);

            CreateWoundedRoot(root, inventory);

            var book = new GameObject("BuyuKitabi").AddComponent<BuyuFisiltisi.Gameplay.Book.SpellBookView>();
            book.transform.SetParent(root, false);
            var bookData = new SerializedObject(book);
            bookData.FindProperty("_inventory").objectReferenceValue = inventory;
            bookData.FindProperty("_fireflyTarget").objectReferenceValue = firefly;
            var commands = Object.FindFirstObjectByType<BuyuFisiltisi.Gameplay.Commands.CompanionCommandKeyboardInput>();
            if (commands != null)
                bookData.FindProperty("_switchTarget").objectReferenceValue = new SerializedObject(commands).FindProperty("_switchTarget").objectReferenceValue;
            var bookFlowers = bookData.FindProperty("_fetchFlowers");
            bookFlowers.arraySize = 1;
            bookFlowers.GetArrayElementAtIndex(0).objectReferenceValue = flowers[NookFlowerIndex];
            bookData.ApplyModifiedPropertiesWithoutUndo();

            CreateCauldron(root, book, body != null ? body : witch.transform, inventory);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Orman] Çiçekler, ateş böceği ve orman ışığı ForestScene sahnesine eklendi. Tab ile Büyü Kitabını aç.");
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
            point.position = index == NookFlowerIndex ? NookFetchPoint : new Vector3(position.x, 0f, position.z);

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
                new Vector3(-3f, 4.4f, 0f),
                new Vector3(4f, 4.6f, 6f),
                new Vector3(-2f, 4.8f, 13f)
            };

            var list = new DarkCloud[positions.Length];
            for (var i = 0; i < positions.Length; i++)
            {
                var go = new GameObject("KaraBulut_" + (i + 1));
                go.transform.SetParent(clouds, false);
                go.transform.position = positions[i];

                var random = new System.Random(40 + i);
                for (var p = 0; p < 7; p++)
                {
                    var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    puff.name = "Bulut_Parca";
                    puff.transform.SetParent(go.transform, false);
                    var angle = p * Mathf.PI * 2f / 7f;
                    var spread = p == 0 ? 0f : 1f + (float)random.NextDouble() * 0.5f;
                    puff.transform.localPosition = new Vector3(Mathf.Cos(angle) * spread * 1.2f, (float)random.NextDouble() * 0.5f - (p == 0 ? -0.3f : 0.1f), Mathf.Sin(angle) * spread * 0.8f);
                    var size = p == 0 ? 2.2f : 1.2f + (float)random.NextDouble() * 0.7f;
                    puff.transform.localScale = new Vector3(size, size * 0.75f, size);
                    Object.DestroyImmediate(puff.GetComponent<SphereCollider>());
                    if (cloudMaterial != null)
                        puff.GetComponent<Renderer>().sharedMaterial = cloudMaterial;
                }

                var flash = new GameObject("Simsek");
                flash.transform.SetParent(go.transform, false);
                flash.transform.localPosition = new Vector3(0f, -0.6f, 0f);
                var flashLight = flash.AddComponent<Light>();
                flashLight.type = LightType.Point;
                flashLight.color = new Color(0.6f, 0.45f, 1f);
                flashLight.intensity = 0.4f;
                flashLight.range = 7f;

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
            var groundMaterial = groundSource != null && groundSource.GetComponent<Renderer>() != null ? groundSource.GetComponent<Renderer>().sharedMaterial : null;
            CreateOuterGround(forest, "DisZemin_Guney", new Vector3(0f, -0.03f, -3f), new Vector3(80f, 0.02f, 46f), groundMaterial);
            CreateOuterGround(forest, "DisZemin_Kuzey", new Vector3(0f, -0.03f, 69f), new Vector3(80f, 0.02f, 90f), groundMaterial);
            CreateOuterGround(forest, "DisZemin_Bati", new Vector3(-25f, -0.03f, 22f), new Vector3(30f, 0.02f, 4f), groundMaterial);
            CreateOuterGround(forest, "DisZemin_Dogu", new Vector3(25f, -0.03f, 22f), new Vector3(30f, 0.02f, 4f), groundMaterial);
            CreateChasm(forest);

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
            CreateOldOak(forest);
        }

        private static void CreateOldOak(Transform parent)
        {
            var oak = new GameObject("YasliMese_Gorunum").transform;
            oak.SetParent(parent, false);
            oak.position = OakPosition;

            var tree = PlaceModel(oak, "tree_fat_darkh", OakPosition, 0f, 14f);
            var front = OakPosition.z - 1f;
            if (tree != null)
            {
                front = float.MaxValue;
                foreach (var filter in tree.GetComponentsInChildren<MeshFilter>())
                {
                    if (filter.sharedMesh == null)
                        continue;
                    foreach (var vertex in filter.sharedMesh.vertices)
                    {
                        var world = filter.transform.TransformPoint(vertex);
                        if (world.y > 1.8f && world.y < 2.8f && Mathf.Abs(world.x - OakPosition.x) < 0.8f && world.z < front)
                            front = world.z;
                    }
                }
                if (front == float.MaxValue)
                    front = OakPosition.z - 1f;
            }

            var eyeMaterial = GetColorMaterial("FFD966_Goz", new Color(1f, 0.85f, 0.4f), true);
            foreach (var side in new[] { -0.35f, 0.35f })
            {
                var eye = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                eye.name = "Mese_Goz";
                eye.transform.SetParent(oak, false);
                eye.transform.position = new Vector3(OakPosition.x + side, 2.4f, front - 0.02f);
                eye.transform.localScale = new Vector3(0.28f, 0.16f, 0.1f);
                Object.DestroyImmediate(eye.GetComponent<Collider>());
                eye.GetComponent<Renderer>().sharedMaterial = eyeMaterial;
            }

            var random = new System.Random(7);
            for (var i = 0; i < 8; i++)
            {
                var angle = i * Mathf.PI * 2f / 8f;
                var offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * 1.9f;
                PlaceModel(oak, "log", OakPosition + offset, -angle * Mathf.Rad2Deg, 2.2f + (float)random.NextDouble() * 0.6f);
            }

            foreach (var (model, offset) in new[] { ("mushroom_redGroup", new Vector3(2.4f, 0f, -1.2f)), ("mushroom_tanGroup", new Vector3(-2.2f, 0f, -1.6f)), ("mushroom_redTall", new Vector3(1.4f, 0f, -2.4f)) })
                PlaceModel(oak, model, OakPosition + offset, random.Next(360), 0.8f);

            var aura = new GameObject("Mese_Isigi");
            aura.transform.SetParent(oak, false);
            aura.transform.position = OakPosition + new Vector3(0f, 3.5f, -2f);
            var light = aura.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.8f, 0.45f);
            light.intensity = 5f;
            light.range = 9f;
            aura.AddComponent<FlickerLight>();

            var glow = GetGlowMaterial();
            for (var i = 0; i < 14; i++)
            {
                var mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mote.name = "Mese_Isiltisi";
                mote.transform.SetParent(oak, false);
                var angle = (float)random.NextDouble() * Mathf.PI * 2f;
                var radius = 1.8f + (float)random.NextDouble() * 2.5f;
                mote.transform.position = OakPosition + new Vector3(Mathf.Cos(angle) * radius, 1f + (float)random.NextDouble() * 4f, Mathf.Sin(angle) * radius);
                mote.transform.localScale = Vector3.one * 0.08f;
                Object.DestroyImmediate(mote.GetComponent<Collider>());
                mote.GetComponent<Renderer>().sharedMaterial = glow;
                var drift = new SerializedObject(mote.AddComponent<DriftMotion>());
                drift.FindProperty("_amplitude").floatValue = 0.3f;
                drift.ApplyModifiedPropertiesWithoutUndo();
            }
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
                if (Mathf.Abs(z - 44f) < 1.4f)
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

        private static void DressCharacters(Transform witchBody)
        {
            var old = witchBody.Find("CadiGorseli");
            if (old != null)
                Object.DestroyImmediate(old.gameObject);

            if (DressWithModels(witchBody))
                return;

            var witch = new GameObject("CadiGorseli").transform;
            witch.SetParent(witchBody, false);
            var hatColor = new Color(0.18f, 0.08f, 0.28f);
            AddPart(witch, "Sapka_Kenar", PrimitiveType.Cylinder, new Vector3(0f, 1.0f, 0f), new Vector3(1.1f, 0.03f, 1.1f), hatColor);
            AddPart(witch, "Sapka_1", PrimitiveType.Cylinder, new Vector3(0f, 1.18f, 0f), new Vector3(0.6f, 0.16f, 0.6f), hatColor);
            AddPart(witch, "Sapka_2", PrimitiveType.Cylinder, new Vector3(0f, 1.45f, -0.04f), new Vector3(0.42f, 0.14f, 0.42f), hatColor);
            AddPart(witch, "Sapka_3", PrimitiveType.Cylinder, new Vector3(0f, 1.68f, -0.1f), new Vector3(0.26f, 0.12f, 0.26f), hatColor);
            AddPart(witch, "Sapka_Uc", PrimitiveType.Sphere, new Vector3(0f, 1.84f, -0.18f), new Vector3(0.12f, 0.12f, 0.12f), hatColor);
            AddPart(witch, "Sapka_Bant", PrimitiveType.Cylinder, new Vector3(0f, 1.07f, 0f), new Vector3(0.62f, 0.04f, 0.62f), new Color(0.9f, 0.7f, 0.2f));
            AddPart(witch, "Pelerin", PrimitiveType.Cube, new Vector3(0f, 0.05f, -0.42f), new Vector3(0.8f, 1.6f, 0.06f), new Color(0.25f, 0.05f, 0.12f));

            var fisil = GameObject.Find("Fisil");
            var book = fisil != null ? fisil.transform.Find("Fisil_Kitap") : null;
            if (book == null)
                return;

            var oldBook = book.Find("KitapGorseli");
            if (oldBook != null)
                Object.DestroyImmediate(oldBook.gameObject);

            var bookRenderer = book.GetComponent<Renderer>();
            if (bookRenderer != null)
                bookRenderer.enabled = false;

            var visual = new GameObject("KitapGorseli").transform;
            visual.SetParent(book, false);
            visual.localScale = new Vector3(1f / Mathf.Max(book.lossyScale.x, 0.01f), 1f / Mathf.Max(book.lossyScale.y, 0.01f), 1f / Mathf.Max(book.lossyScale.z, 0.01f));
            visual.localRotation = Quaternion.Euler(-70f, 0f, 0f);

            var cover = new Color(0.45f, 0.1f, 0.12f);
            AddPart(visual, "Kapak_Alt", PrimitiveType.Cube, new Vector3(0f, -0.06f, 0f), new Vector3(0.5f, 0.03f, 0.65f), cover);
            AddPart(visual, "Kapak_Ust", PrimitiveType.Cube, new Vector3(0f, 0.06f, 0f), new Vector3(0.5f, 0.03f, 0.65f), cover);
            AddPart(visual, "Sirt", PrimitiveType.Cube, new Vector3(-0.25f, 0f, 0f), new Vector3(0.04f, 0.15f, 0.65f), new Color(0.3f, 0.06f, 0.08f));
            AddPart(visual, "Sayfalar", PrimitiveType.Cube, new Vector3(0.01f, 0f, 0f), new Vector3(0.46f, 0.09f, 0.6f), new Color(0.95f, 0.9f, 0.75f));
            AddPart(visual, "Goz_Sol", PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, 0.12f), new Vector3(0.07f, 0.03f, 0.07f), new Color(0.6f, 1f, 0.9f), true);
            AddPart(visual, "Goz_Sag", PrimitiveType.Sphere, new Vector3(0.08f, 0.08f, -0.12f), new Vector3(0.07f, 0.03f, 0.07f), new Color(0.6f, 1f, 0.9f), true);

            var glow = new GameObject("KitapIsigi");
            glow.transform.SetParent(visual, false);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.9f, 1f);
            light.intensity = 2f;
            light.range = 2.5f;
        }

        private static bool DressWithModels(Transform witchBody)
        {
            var mage = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitFolder + "Characters/fbx/Mage.fbx");
            var bookAsset = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitFolder + "Assets/fbx(unity)/spellbook_open.fbx");
            if (mage == null || bookAsset == null)
                return false;

            var bodyRenderer = witchBody.GetComponent<Renderer>();
            if (bodyRenderer != null)
                bodyRenderer.enabled = false;

            var witch = (GameObject)PrefabUtility.InstantiatePrefab(mage, witchBody);
            witch.name = "CadiGorseli";
            FitToHeight(witch.transform, witchBody.position + Vector3.down * 1f, witchBody.rotation, 1.9f);

            var animator = witch.GetComponent<Animator>();
            if (animator == null)
                animator = witch.AddComponent<Animator>();
            animator.runtimeAnimatorController = GetWitchController();
            animator.applyRootMotion = false;
            witch.AddComponent<BuyuFisiltisi.Gameplay.Player.SpeedAnimator>();

            var fisil = GameObject.Find("Fisil");
            var book = fisil != null ? fisil.transform.Find("Fisil_Kitap") : null;
            if (book == null)
                return true;

            var oldBook = book.Find("KitapGorseli");
            if (oldBook != null)
                Object.DestroyImmediate(oldBook.gameObject);

            var bookRenderer = book.GetComponent<Renderer>();
            if (bookRenderer != null)
                bookRenderer.enabled = false;

            var visual = new GameObject("KitapGorseli").transform;
            visual.SetParent(book, false);
            visual.localScale = new Vector3(1f / Mathf.Max(book.lossyScale.x, 0.01f), 1f / Mathf.Max(book.lossyScale.y, 0.01f), 1f / Mathf.Max(book.lossyScale.z, 0.01f));

            var model = (GameObject)PrefabUtility.InstantiatePrefab(bookAsset, visual);
            model.name = "spellbook_open";
            model.transform.localRotation = Quaternion.Euler(-25f, 0f, 0f) * bookAsset.transform.localRotation;
            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                var bounds = renderers[0].bounds;
                foreach (var renderer in renderers)
                    bounds.Encapsulate(renderer.bounds);
                var largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
                if (largest > 0.0001f)
                    model.transform.localScale = Vector3.one * (0.6f / largest);
            }

            var glow = new GameObject("KitapIsigi");
            glow.transform.SetParent(visual, false);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.9f, 1f);
            light.intensity = 2f;
            light.range = 2.5f;
            return true;
        }

        private static Transform CreateIntroBook(Transform parent)
        {
            var closed = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitFolder + "Assets/fbx(unity)/spellbook_closed.fbx");
            var open = AssetDatabase.LoadAssetAtPath<GameObject>(KayKitFolder + "Assets/fbx(unity)/spellbook_open.fbx");
            if (closed == null || open == null)
                return null;

            var root = new GameObject("GirisKitabi").transform;
            root.SetParent(parent, false);
            AddBookModel(root, closed, "Kapali", 1.1f);
            AddBookModel(root, open, "Acik", 1.4f);

            var glow = new GameObject("Isilti");
            glow.transform.SetParent(root, false);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.5f, 0.9f, 1f);
            light.intensity = 6f;
            light.range = 6f;
            return root;
        }

        private static void AddBookModel(Transform root, GameObject asset, string name, float size)
        {
            var holder = new GameObject(name).transform;
            holder.SetParent(root, false);
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset, holder);
            model.transform.localRotation = Quaternion.Euler(-20f, 0f, 0f) * asset.transform.localRotation;

            var renderers = model.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            var largest = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            if (largest > 0.0001f)
                model.transform.localScale = Vector3.one * (size / largest);

            bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            model.transform.position -= bounds.center - holder.position;
        }

        private static void FitToHeight(Transform model, Vector3 feet, Quaternion rotation, float height)
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
                model.localScale = model.localScale * (height / bounds.size.y);

            bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);
            model.position += Vector3.up * (feet.y - bounds.min.y);
        }

        private static RuntimeAnimatorController GetWitchController()
        {
            var existing = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(WitchControllerPath);
            if (existing != null)
                return existing;

            var idle = LoadClip(KayKitFolder + "Animations/fbx/Rig_Medium/Rig_Medium_General.fbx", "Idle_A");
            var walk = LoadClip(KayKitFolder + "Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx", "Walking_A");
            var run = LoadClip(KayKitFolder + "Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx", "Running_A");
            if (idle == null || walk == null || run == null)
                return null;

            var controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(WitchControllerPath);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            var state = controller.CreateBlendTreeInController("Hareket", out var tree);
            tree.blendType = UnityEditor.Animations.BlendTreeType.Simple1D;
            tree.blendParameter = "Speed";
            tree.useAutomaticThresholds = false;
            tree.AddChild(idle, 0f);
            tree.AddChild(walk, 4f);
            tree.AddChild(run, 6.5f);
            controller.layers[0].stateMachine.defaultState = state;
            AssetDatabase.SaveAssets();
            return controller;
        }

        private static AnimationClip LoadClip(string path, string clipName)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer != null)
            {
                var clips = importer.clipAnimations.Length > 0 ? importer.clipAnimations : importer.defaultClipAnimations;
                var changed = false;
                foreach (var clip in clips)
                {
                    if (clip.loopTime)
                        continue;
                    clip.loopTime = true;
                    changed = true;
                }

                if (changed)
                {
                    importer.clipAnimations = clips;
                    importer.SaveAndReimport();
                }
            }

            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                if (asset is AnimationClip clip && clip.name == clipName)
                    return clip;
            }

            return null;
        }

        private static void AddPart(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color, bool glow = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().sharedMaterial = GetColorMaterial(ColorUtility.ToHtmlStringRGB(color) + (glow ? "_Isik" : string.Empty), color, glow);
        }

        private static Material GetColorMaterial(string key, Color color, bool glow)
        {
            var path = "Assets/_Project/Materials/Graybox/Karakter_" + key + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material != null)
                return material;

            material = new Material(Shader.Find(glow ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit"));
            material.color = color;
            AssetDatabase.CreateAsset(material, path);
            return material;
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

        private static void CreateOuterGround(Transform parent, string name, Vector3 position, Vector3 size, Material material)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube);
            piece.name = name;
            piece.transform.SetParent(parent, false);
            piece.transform.position = position;
            piece.transform.localScale = size;
            Object.DestroyImmediate(piece.GetComponent<Collider>());
            if (material != null)
                piece.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static void CreateChasm(Transform parent)
        {
            var chasm = new GameObject("Ucurum").transform;
            chasm.SetParent(parent, false);
            var dark = GetColorMaterial("140C1E_Ucurum", new Color(0.08f, 0.05f, 0.12f), false);
            var parts = new[]
            {
                (new Vector3(0f, -9f, 22f), new Vector3(20.4f, 0.2f, 4.4f)),
                (new Vector3(-10.1f, -4.5f, 22f), new Vector3(0.2f, 9f, 4.4f)),
                (new Vector3(10.1f, -4.5f, 22f), new Vector3(0.2f, 9f, 4.4f)),
                (new Vector3(0f, -4.6f, 19.9f), new Vector3(20.4f, 9f, 0.2f)),
                (new Vector3(0f, -4.6f, 24.1f), new Vector3(20.4f, 9f, 0.2f))
            };

            foreach (var (position, size) in parts)
            {
                var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wall.name = "Ucurum_Duvar";
                wall.transform.SetParent(chasm, false);
                wall.transform.position = position;
                wall.transform.localScale = size;
                Object.DestroyImmediate(wall.GetComponent<Collider>());
                wall.GetComponent<Renderer>().sharedMaterial = dark;
            }

            var mist = new GameObject("Ucurum_Isigi");
            mist.transform.SetParent(chasm, false);
            mist.transform.position = new Vector3(0f, -6f, 22f);
            var light = mist.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.55f, 0.3f, 0.9f);
            light.intensity = 3f;
            light.range = 8f;
        }

        private static void CreateCauldron(Transform root, BuyuFisiltisi.Gameplay.Book.SpellBookView book, Transform player, PlayerInventory inventory)
        {
            var cauldron = new GameObject("BuyuKazani").transform;
            cauldron.SetParent(root, false);
            cauldron.position = CauldronPosition;

            PlaceModel(cauldron, "campfire_stones", CauldronPosition, 0f, 1.6f);
            PlaceModel(cauldron, "campfire_logs", CauldronPosition, 30f, 1.1f);
            PlaceModel(cauldron, "pot_large", CauldronPosition + Vector3.up * 0.25f, 0f, 1.1f);

            var brew = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            brew.name = "Iksir";
            brew.transform.SetParent(cauldron, false);
            brew.transform.position = CauldronPosition + Vector3.up * 1.05f;
            brew.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
            Object.DestroyImmediate(brew.GetComponent<Collider>());
            brew.GetComponent<Renderer>().sharedMaterial = GetColorMaterial("6CFF8A_Isik", new Color(0.42f, 1f, 0.54f), true);

            var glow = new GameObject("KazanIsigi");
            glow.transform.SetParent(cauldron, false);
            glow.transform.position = CauldronPosition + Vector3.up * 1.6f;
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(0.45f, 1f, 0.5f);
            light.intensity = 6f;
            light.range = 5f;
            glow.AddComponent<FlickerLight>();

            var clickArea = cauldron.gameObject.AddComponent<BoxCollider>();
            clickArea.center = new Vector3(0f, 0.8f, 0f);
            clickArea.size = new Vector3(1.8f, 1.6f, 1.8f);

            var station = cauldron.gameObject.AddComponent<BuyuFisiltisi.Gameplay.Book.CauldronStation>();
            var data = new SerializedObject(station);
            data.FindProperty("_book").objectReferenceValue = book;
            data.FindProperty("_player").objectReferenceValue = player;
            data.FindProperty("_inventory").objectReferenceValue = inventory;
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

        private static GameObject PlaceModel(Transform parent, string modelName, Vector3 position, float yaw, float targetSize)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + modelName + ".fbx");
            if (asset == null)
            {
                Debug.LogWarning("[Orman] Model bulunamadı: " + modelName);
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

            bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            instance.transform.position += new Vector3(0f, position.y - bounds.min.y, 0f);
            return instance;
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

                go.GetComponent<Renderer>().enabled = false;
                PlaceModelFit(mountain, "cliff_block_rock", go.transform.position, new Vector3(2.7f, height, 1.95f));
            }

            PlaceModel(mountain, "rock_tallB", new Vector3(-6.6f, 0f, 63.5f), 30f, 4.6f);
            PlaceModel(mountain, "rock_tallE", new Vector3(-3.4f, 0f, 64.5f), 200f, 3.4f);
            PlaceModel(mountain, "cliff_large_rock", new Vector3(-7.4f, 0f, 59f), 90f, 4f);
            PlaceModel(mountain, "rock_largeD", new Vector3(-3.6f, 0f, 57f), 10f, 1.2f);
            PlaceModel(mountain, "plant_bushSmall", new Vector3(-4.2f, 3.6f, 65.2f), 0f, 0.6f);
        }

        private static void PlaceModelFit(Transform parent, string modelName, Vector3 center, Vector3 size)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + modelName + ".fbx");
            if (asset == null)
                return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            instance.name = modelName;
            instance.transform.position = Vector3.zero;
            instance.transform.rotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var renderers = instance.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return;

            var bounds = renderers[0].bounds;
            foreach (var renderer in renderers)
                bounds.Encapsulate(renderer.bounds);

            var scale = new Vector3(
                size.x / Mathf.Max(bounds.size.x, 0.0001f),
                size.y / Mathf.Max(bounds.size.y, 0.0001f),
                size.z / Mathf.Max(bounds.size.z, 0.0001f));
            instance.transform.localScale = scale;
            var offset = Vector3.Scale(bounds.center, scale);
            instance.transform.position = center - offset;
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

        private static void BugPart(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 euler, Vector3 scale, Material material)
        {
            var part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localRotation = Quaternion.Euler(euler);
            part.transform.localScale = scale;
            Object.DestroyImmediate(part.GetComponent<Collider>());
            part.GetComponent<Renderer>().sharedMaterial = material;
        }

        private static Firefly CreateFirefly(Transform root, PlayerInventory inventory, Material material)
        {
            var go = new GameObject("AtesBocegi");
            go.transform.SetParent(root, false);
            go.transform.position = FireflyPosition;
            go.transform.rotation = Quaternion.Euler(0f, 160f, 0f);
            go.AddComponent<NavMeshModifier>().ignoreFromBuild = true;

            var body = new GameObject("Gorunum").transform;
            body.SetParent(go.transform, false);
            body.localScale = Vector3.one * 0.5f;

            var shellMaterial = GetColorMaterial("3A2448_Bocek", new Color(0.23f, 0.14f, 0.28f), false);
            var whiteMaterial = GetColorMaterial("FFFFFF_Goz", Color.white, true);
            var pupilMaterial = GetColorMaterial("111111_Gozbebegi", new Color(0.07f, 0.07f, 0.07f), false);
            var wingMaterial = GetColorMaterial("CFE6FF_Kanat", new Color(0.8f, 0.9f, 1f), true);

            BugPart(body, "Kuyruk", PrimitiveType.Sphere, new Vector3(0f, 0f, -0.45f), Vector3.zero, new Vector3(0.62f, 0.55f, 0.8f), GetGlowMaterial());
            BugPart(body, "Govde", PrimitiveType.Sphere, new Vector3(0f, 0.05f, 0.1f), Vector3.zero, new Vector3(0.48f, 0.42f, 0.5f), shellMaterial);
            BugPart(body, "Bas", PrimitiveType.Sphere, new Vector3(0f, 0.12f, 0.48f), Vector3.zero, new Vector3(0.44f, 0.4f, 0.4f), shellMaterial);
            foreach (var side in new[] { -1f, 1f })
            {
                BugPart(body, "Goz", PrimitiveType.Sphere, new Vector3(side * 0.11f, 0.2f, 0.64f), Vector3.zero, new Vector3(0.15f, 0.17f, 0.1f), whiteMaterial);
                BugPart(body, "Gozbebegi", PrimitiveType.Sphere, new Vector3(side * 0.1f, 0.2f, 0.69f), Vector3.zero, new Vector3(0.07f, 0.09f, 0.04f), pupilMaterial);
                BugPart(body, "Anten", PrimitiveType.Cylinder, new Vector3(side * 0.1f, 0.42f, 0.6f), new Vector3(20f, 0f, side * -25f), new Vector3(0.02f, 0.14f, 0.02f), shellMaterial);
                BugPart(body, "Anten_Ucu", PrimitiveType.Sphere, new Vector3(side * 0.16f, 0.55f, 0.65f), Vector3.zero, Vector3.one * 0.06f, GetGlowMaterial());

                var hinge = new GameObject(side < 0f ? "Kanat_Sol" : "Kanat_Sag").transform;
                hinge.SetParent(body, false);
                hinge.localPosition = new Vector3(side * 0.12f, 0.28f, 0.1f);
                BugPart(hinge, "Kanat", PrimitiveType.Sphere, new Vector3(side * 0.32f, 0f, -0.12f), new Vector3(0f, side * 20f, 0f), new Vector3(0.62f, 0.03f, 0.32f), wingMaterial);
                BugPart(hinge, "Kanat_Arka", PrimitiveType.Sphere, new Vector3(side * 0.26f, -0.02f, -0.36f), new Vector3(0f, side * -15f, 0f), new Vector3(0.48f, 0.03f, 0.24f), wingMaterial);
                var flap = new SerializedObject(hinge.gameObject.AddComponent<WingFlap>());
                flap.FindProperty("_side").floatValue = side;
                flap.ApplyModifiedPropertiesWithoutUndo();
            }

            PlaceModel(root, "stump_round", new Vector3(FireflyPosition.x, 0f, FireflyPosition.z), 0f, 0.9f);

            var glow = new GameObject("Isilti");
            glow.transform.SetParent(go.transform, false);
            var light = glow.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.85f, 0.3f);
            light.intensity = 7f;
            light.range = 5f;
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
