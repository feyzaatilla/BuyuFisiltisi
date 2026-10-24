using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Companion.Movement;
using BuyuFisiltisi.Core.Navigation;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BuyuFisiltisi.EditorTools
{
    public static class CompanionPrefabBuilder
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs";
        private const string PrefabPath = PrefabFolder + "/Fisil.prefab";
        private const string MaterialPath = "Assets/_Project/Materials/Graybox/Graybox_Fisil.mat";
        private const float HoverHeight = 1.2f;

        [MenuItem("Büyü Fısıltısı/Fısıl Prefab'ını Oluştur")]
        public static void Build()
        {
            if (!AgentTypeLookup.TryFindId(AgentTypes.Fisil.Name, out var agentTypeId))
            {
                EditorUtility.DisplayDialog("Fısıl", $"'{AgentTypes.Fisil.Name}' Agent Type bulunamadı. Önce Proje Kurulum Kontrolü'nü çalıştır.", "Tamam");
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) != null &&
                !EditorUtility.DisplayDialog("Fısıl", "Fisil.prefab zaten var. Üzerine yazılsın mı?", "Evet", "Hayır"))
                return;

            var root = new GameObject("Fisil") { layer = GameLayers.Companion };

            var agent = root.AddComponent<NavMeshAgent>();
            agent.agentTypeID = agentTypeId;
            agent.radius = AgentTypes.Fisil.Radius;
            agent.height = AgentTypes.Fisil.Height;
            agent.baseOffset = HoverHeight;
            agent.speed = 3.5f;
            agent.angularSpeed = 720f;
            agent.acceleration = 12f;
            agent.stoppingDistance = 0.1f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;

            root.AddComponent<CompanionMover>();
            root.AddComponent<CompanionController>();

            var book = GameObject.CreatePrimitive(PrimitiveType.Cube);
            book.name = "Fisil_Kitap";
            book.layer = GameLayers.Companion;
            book.transform.SetParent(root.transform, false);
            book.transform.localScale = new Vector3(0.3f, 0.4f, 0.1f);
            Object.DestroyImmediate(book.GetComponent<Collider>());

            var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (material != null)
                book.GetComponent<Renderer>().sharedMaterial = material;

            if (!AssetDatabase.IsValidFolder(PrefabFolder))
                AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");

            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.SaveAssets();

            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Debug.Log("[Fısıl] Fisil.prefab oluşturuldu: " + PrefabPath);
        }
    }
}
