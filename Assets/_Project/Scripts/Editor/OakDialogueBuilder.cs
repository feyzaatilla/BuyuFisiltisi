using System.Linq;
using BuyuFisiltisi.Gameplay.Commands;
using BuyuFisiltisi.Gameplay.Dialogue;
using BuyuFisiltisi.Gameplay.Player;
using BuyuFisiltisi.UI.Connection;
using BuyuFisiltisi.UI.Oak;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BuyuFisiltisi.EditorTools
{
    public static class OakDialogueBuilder
    {
        private const string SceneName = "ForestScene";
        private const string Title = "Yaşlı Meşe Sohbeti";
        private const string ConnectionPrefabPath = "Assets/_Project/Prefabs/UI/BaglantiPaneli.prefab";
        private const string CanvasName = "MeseSohbeti";
        private const string SystemName = "MeseSohbetSistemi";

        private static readonly Color BoxColor = new Color(0.1f, 0.16f, 0.11f, 0.92f);
        private static readonly Color InputColor = new Color(0f, 0f, 0f, 0.45f);
        private static readonly Color TextColor = new Color(0.93f, 0.9f, 0.8f, 1f);
        private static readonly Color ReplyColor = new Color(0.78f, 0.95f, 0.7f, 1f);
        private static readonly Color PlayerColor = new Color(0.75f, 0.85f, 1f, 1f);
        private static readonly Color ThinkingColor = new Color(0.95f, 0.85f, 0.5f, 1f);

        [MenuItem("Büyü Fısıltısı/Yaşlı Meşe Sohbetini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != SceneName)
            {
                Dialog($"Önce {SceneName} sahnesini aç.");
                return;
            }

            var oak = Find("_Characters/YasliMese");
            var witch = Find("_Characters/Cadi/Cadi_Govde");
            var systems = Find("_Systems");
            var ui = Find("_UI");

            if (oak == null || witch == null || systems == null)
            {
                Dialog("Sahnede Yaşlı Meşe, Cadı ya da _Systems bulunamadı. Önce Orman Sahnesini Bağla menüsünü çalıştır.");
                return;
            }

            if (ui == null)
            {
                var created = new GameObject("_UI");
                Undo.RegisterCreatedObjectUndo(created, "_UI");
                ui = created.transform;
            }

            Remove(ui, CanvasName);
            Remove(systems, SystemName);

            var view = BuildCanvas(ui);
            var connection = EnsureConnectionPanel(ui);
            BuildSystem(systems, oak, witch, view, connection);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[Meşe] Sohbet kutusu ve sistemi sahneye eklendi. Ctrl+S ile kaydet.");
        }

        private static OakChatView BuildCanvas(Transform ui)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var root = new GameObject(CanvasName, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            Undo.RegisterCreatedObjectUndo(root, CanvasName);
            root.transform.SetParent(ui, false);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var view = root.AddComponent<OakChatView>();

            var prompt = Label(root.transform, "Istem", font, 34, TextAnchor.MiddleCenter, TextColor);
            prompt.fontStyle = FontStyle.Bold;
            prompt.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.85f);
            Place(prompt.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 150f), new Vector2(900f, 60f), new Vector2(0.5f, 0f));

            var trust = Label(root.transform, "Guven", font, 28, TextAnchor.UpperLeft, ThinkingColor);
            trust.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.85f);
            Place(trust.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(720f, 50f), new Vector2(0f, 1f));

            var panel = Panel(root.transform, "Kutu", BoxColor);
            Place(panel.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(1500f, 380f), new Vector2(0.5f, 0f));

            var player = Label(panel.transform, "CadiSozu", font, 28, TextAnchor.UpperLeft, PlayerColor);
            Place(player.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -20f), new Vector2(-60f, 50f), new Vector2(0.5f, 1f));

            var reply = Label(panel.transform, "MeseSozu", font, 34, TextAnchor.UpperLeft, ReplyColor);
            Place(reply.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -75f), new Vector2(-60f, 170f), new Vector2(0.5f, 1f));

            var thinking = Label(panel.transform, "Dusunuyor", font, 30, TextAnchor.MiddleLeft, ThinkingColor);
            thinking.fontStyle = FontStyle.Italic;
            Place(thinking.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 100f), new Vector2(-60f, 44f), new Vector2(0.5f, 0f));

            var inputBox = Panel(panel.transform, "YaziKutusu", InputColor);
            Place(inputBox.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 20f), new Vector2(-40f, 64f), new Vector2(0.5f, 0f));

            var input = Label(inputBox.transform, "Yazi", font, 32, TextAnchor.MiddleLeft, TextColor);
            input.horizontalOverflow = HorizontalWrapMode.Overflow;
            Stretch(input.rectTransform, 16f);

            var data = new SerializedObject(view);
            data.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            data.FindProperty("_playerText").objectReferenceValue = player;
            data.FindProperty("_replyText").objectReferenceValue = reply;
            data.FindProperty("_thinkingText").objectReferenceValue = thinking;
            data.FindProperty("_inputText").objectReferenceValue = input;
            data.FindProperty("_promptText").objectReferenceValue = prompt;
            data.FindProperty("_trustText").objectReferenceValue = trust;
            data.ApplyModifiedPropertiesWithoutUndo();

            panel.gameObject.SetActive(false);
            prompt.gameObject.SetActive(false);
            thinking.gameObject.SetActive(false);
            return view;
        }

        private static ConnectionStatusView EnsureConnectionPanel(Transform ui)
        {
            var existing = Object.FindObjectsByType<ConnectionStatusView>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existing.Length > 0)
                return existing[0];

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ConnectionPrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning("[Meşe] BaglantiPaneli.prefab bulunamadı, bağlantı paneli eklenmedi.");
                return null;
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, ui);
            Undo.RegisterCreatedObjectUndo(instance, "Bağlantı paneli");
            return instance.GetComponent<ConnectionStatusView>();
        }

        private static void BuildSystem(Transform systems, Transform oak, Transform witch, OakChatView view, ConnectionStatusView connection)
        {
            var holder = new GameObject(SystemName);
            Undo.RegisterCreatedObjectUndo(holder, SystemName);
            holder.transform.SetParent(systems, false);

            var controller = holder.AddComponent<OakDialogueController>();
            var data = new SerializedObject(controller);
            data.FindProperty("_oak").objectReferenceValue = oak;
            data.FindProperty("_player").objectReferenceValue = witch;
            data.FindProperty("_view").objectReferenceValue = view;
            data.FindProperty("_connection").objectReferenceValue = connection;
            data.ApplyModifiedPropertiesWithoutUndo();

            var inputLock = holder.AddComponent<DialogueInputLock>();
            var locked = new Behaviour[]
            {
                witch.GetComponent<WitchMotor>(),
                Object.FindFirstObjectByType<CompanionCommandKeyboardInput>()
            }.Where(behaviour => behaviour != null).ToArray();

            var lockData = new SerializedObject(inputLock);
            var list = lockData.FindProperty("_lockedBehaviours");
            list.arraySize = locked.Length;
            for (var i = 0; i < locked.Length; i++)
                list.GetArrayElementAtIndex(i).objectReferenceValue = locked[i];
            lockData.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void Remove(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null)
                Undo.DestroyObjectImmediate(child.gameObject);
        }

        private static Text Label(Transform parent, string name, Font font, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = color;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Image Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static void Place(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, Vector2 pivot)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float margin)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(margin, 0f);
            rect.offsetMax = new Vector2(-margin, 0f);
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
            EditorUtility.DisplayDialog(Title, message, "Tamam");
        }
    }
}
