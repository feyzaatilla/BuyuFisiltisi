using System.Linq;
using BuyuFisiltisi.Bootstrap;
using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Gameplay.Commands;
using BuyuFisiltisi.Gameplay.Player;
using BuyuFisiltisi.Gameplay.World;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BuyuFisiltisi.EditorTools
{
    public static class ForestSceneWiring
    {
        private const string SceneName = "ForestScene";
        private const string FisilPrefabPath = "Assets/_Project/Prefabs/Fisil.prefab";

        private static readonly Vector3 InteractionPointPosition = new Vector3(8.3f, 0f, 15f);
        private static readonly Vector3 FisilStartPosition = new Vector3(1.5f, 0f, -5f);
        private static readonly Vector3 KillZonePosition = new Vector3(0f, -6f, 40f);
        private static readonly Vector3 KillZoneSize = new Vector3(200f, 2f, 200f);

        [MenuItem("Büyü Fısıltısı/Orman Sahnesini Bağla")]
        public static void Wire()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                Dialog($"Önce {SceneName} sahnesini aç.");
                return;
            }

            var bootstrap = Find("_Systems/Bootstrap");
            var switchRoot = Find("_Interactables/BridgeSwitch");
            var handle = Find("_Interactables/BridgeSwitch/Salter");
            var zone = Find("_Environment/02_KokDuvari_Kopru");
            var bridge = Find("_Environment/02_KokDuvari_Kopru/Kopru");
            var characters = Find("_Characters");
            var witchBody = Find("_Characters/Cadi/Cadi_Govde");
            var systems = Find("_Systems");

            if (bootstrap == null || switchRoot == null || handle == null || zone == null ||
                bridge == null || characters == null || witchBody == null || systems == null)
            {
                Dialog("Sahnede beklenen nesneler eksik. Önce Graybox Sahnesini Kur menüsünü çalıştır.");
                return;
            }

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FisilPrefabPath);
            if (prefab == null || prefab.GetComponent<CompanionController>() == null)
            {
                Dialog("Fisil.prefab bulunamadı ya da eski. Önce Fısıl Prefab'ını Oluştur menüsünü çalıştır.");
                return;
            }

            AddOnce<GameBootstrap>(bootstrap.gameObject);
            var player = WireWitch(witchBody);
            var bridgeSwitch = WireSwitch(switchRoot, handle);
            WireCommands(systems, bridgeSwitch);
            WireBridge(bridge);
            WireChasm(zone, player.position);
            WireFisil(characters, player, prefab);
            DisableStaticBatching(scene);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Orman] Cadı, kamera, emir tuşları, şalter, köprü, uçurum ve Fısıl sahneye bağlandı. Ctrl+S ile kaydet.");
        }

        private static Transform WireWitch(Transform body)
        {
            var capsule = body.GetComponent<CapsuleCollider>();
            if (capsule != null)
                Undo.DestroyObjectImmediate(capsule);

            var characterController = body.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = Undo.AddComponent<CharacterController>(body.gameObject);
                characterController.radius = 0.4f;
                characterController.height = 2f;
                characterController.center = Vector3.zero;
            }

            var motor = AddOnce<WitchMotor>(body.gameObject);

            var camera = Camera.main;
            if (camera != null)
            {
                var cameraRig = AddOnce<ThirdPersonCamera>(camera.gameObject);
                SetReference(cameraRig, "_target", body);
                SetReference(motor, "_cameraTransform", camera.transform);
            }
            else
            {
                Debug.LogWarning("[Orman] Main Camera bulunamadı, kamera bağlanmadı.");
            }

            return body;
        }

        private static void WireCommands(Transform systems, BridgeSwitch bridgeSwitch)
        {
            var holder = Child(systems, "FisilEmirleri", systems.position);
            var commands = AddOnce<CompanionCommandKeyboardInput>(holder.gameObject);
            SetReference(commands, "_switchTarget", bridgeSwitch);
        }

        private static BridgeSwitch WireSwitch(Transform root, Transform handle)
        {
            var point = Child(root, "EtkilesimNoktasi", InteractionPointPosition);
            var bridgeSwitch = AddOnce<BridgeSwitch>(root.gameObject);

            var serialized = new SerializedObject(bridgeSwitch);
            serialized.FindProperty("_interactionPoint").objectReferenceValue = point;
            serialized.FindProperty("_focusPoint").objectReferenceValue = handle;
            serialized.FindProperty("_handle").objectReferenceValue = handle;
            serialized.ApplyModifiedProperties();
            return bridgeSwitch;
        }

        private static void WireBridge(Transform bridge)
        {
            var modifier = IgnoreInNavMesh(bridge.gameObject);
            var controller = AddOnce<BridgeController>(bridge.gameObject);
            var surfaces = Object.FindObjectsByType<NavMeshSurface>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("_pivot").objectReferenceValue = bridge;
            serialized.FindProperty("_navMeshModifier").objectReferenceValue = modifier;
            var list = serialized.FindProperty("_surfaces");
            list.arraySize = surfaces.Length;
            for (var i = 0; i < surfaces.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = surfaces[i];
            serialized.ApplyModifiedProperties();
        }

        private static void WireChasm(Transform zone, Vector3 respawnPosition)
        {
            var killZone = Child(zone, "UcurumBolgesi", KillZonePosition);
            var box = killZone.GetComponent<BoxCollider>();
            if (box == null)
                box = Undo.AddComponent<BoxCollider>(killZone.gameObject);

            Undo.RecordObject(box, "Uçurum bölgesi");
            box.isTrigger = true;
            box.center = Vector3.zero;
            box.size = KillZoneSize;

            IgnoreInNavMesh(killZone.gameObject);

            var respawn = Child(zone, "DonusNoktasi", respawnPosition);
            var chasm = AddOnce<ChasmKillZone>(killZone.gameObject);
            Undo.RecordObject(chasm, "Uçurum bölgesi");
            chasm.RespawnPoint = respawn;
            EditorUtility.SetDirty(chasm);
        }

        private static void WireFisil(Transform characters, Transform player, GameObject prefab)
        {
            var existing = characters.Find("Fisil");
            CompanionController controller = null;

            if (existing != null)
            {
                controller = existing.GetComponent<CompanionController>();
                if (controller == null)
                    Undo.DestroyObjectImmediate(existing.gameObject);
            }

            if (controller == null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, characters);
                Undo.RegisterCreatedObjectUndo(instance, "Fısıl");
                instance.name = "Fisil";
                instance.transform.position = FisilStartPosition;
                controller = instance.GetComponent<CompanionController>();
            }

            IgnoreInNavMesh(controller.gameObject);

            var serialized = new SerializedObject(controller);
            serialized.FindProperty("player").objectReferenceValue = player;
            serialized.ApplyModifiedProperties();
        }

        private static void DisableStaticBatching(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                {
                    var flags = GameObjectUtility.GetStaticEditorFlags(child.gameObject);
                    if ((flags & StaticEditorFlags.BatchingStatic) == 0)
                    {
                        continue;
                    }

                    Undo.RecordObject(child.gameObject, "Statik birleştirmeyi kapat");
                    GameObjectUtility.SetStaticEditorFlags(child.gameObject, flags & ~StaticEditorFlags.BatchingStatic);
                }
            }
        }

        private static void SetReference(Object target, string property, Object value)
        {
            var serialized = new SerializedObject(target);
            serialized.FindProperty(property).objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }

        private static NavMeshModifier IgnoreInNavMesh(GameObject target)
        {
            var modifier = target.GetComponent<NavMeshModifier>();
            if (modifier == null)
                modifier = Undo.AddComponent<NavMeshModifier>(target);

            Undo.RecordObject(modifier, "NavMesh dışı");
            modifier.ignoreFromBuild = true;
            modifier.applyToChildren = true;
            return modifier;
        }

        private static Transform Child(Transform parent, string name, Vector3 worldPosition)
        {
            var child = parent.Find(name);
            if (child == null)
            {
                var created = new GameObject(name);
                Undo.RegisterCreatedObjectUndo(created, name);
                created.transform.SetParent(parent, false);
                child = created.transform;
            }

            Undo.RecordObject(child, name);
            child.position = worldPosition;
            return child;
        }

        private static T AddOnce<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : Undo.AddComponent<T>(target);
        }

        private static Transform Find(string path)
        {
            var parts = path.Split(new[] { '/' }, 2);
            var root = EditorSceneManager.GetActiveScene().GetRootGameObjects().FirstOrDefault(g => g.name == parts[0]);
            if (root == null)
                return null;

            return parts.Length == 1 ? root.transform : root.transform.Find(parts[1]);
        }

        private static void Dialog(string message)
        {
            EditorUtility.DisplayDialog("Orman Sahnesi", message, "Tamam");
        }
    }
}
