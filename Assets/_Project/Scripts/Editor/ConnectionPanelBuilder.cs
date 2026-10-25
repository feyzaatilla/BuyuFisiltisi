using BuyuFisiltisi.UI.Connection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BuyuFisiltisi.EditorTools
{
    public static class ConnectionPanelBuilder
    {
        private const string PrefabFolder = "Assets/_Project/Prefabs/UI";
        private const string PrefabPath = PrefabFolder + "/BaglantiPaneli.prefab";
        private const string TestSceneFolder = "Assets/_Project/Scenes/Test/";
        private const string Title = "Bağlantı Paneli";

        private static readonly Color PanelShade = new Color(0f, 0f, 0f, 0.65f);
        private static readonly Color BoxColor = new Color(0.12f, 0.18f, 0.13f, 0.95f);
        private static readonly Color ButtonColor = new Color(0.32f, 0.45f, 0.3f, 1f);
        private static readonly Color TextColor = new Color(0.93f, 0.9f, 0.8f, 1f);

        [MenuItem("Büyü Fısıltısı/Bağlantı Panelini Kur")]
        public static void Build()
        {
            if (EditModeRunner.RunInEditMode(Build))
                return;

            var prefab = CreatePrefab();
            var scene = EditorSceneManager.GetActiveScene();

            if (!scene.path.StartsWith(TestSceneFolder))
            {
                EditorUtility.DisplayDialog(Title, $"Prefab hazır: {PrefabPath}\nBu sahne test sahnesi olmadığı için içine eklenmedi.", "Tamam");
                return;
            }

            foreach (var existing in Object.FindObjectsByType<ConnectionStatusView>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Object.DestroyImmediate(existing.gameObject);

            PrefabUtility.InstantiatePrefab(prefab, scene);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"[Bağlantı] Panel {scene.name} sahnesine eklendi.");
        }

        private static GameObject CreatePrefab()
        {
            if (!AssetDatabase.IsValidFolder(PrefabFolder))
                AssetDatabase.CreateFolder("Assets/_Project/Prefabs", "UI");

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var root = new GameObject("BaglantiPaneli", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            var monitor = root.AddComponent<OllamaConnectionMonitor>();
            var view = root.AddComponent<ConnectionStatusView>();

            var indicator = Label(root.transform, "Gosterge", font, 36, TextAnchor.UpperRight);
            indicator.fontStyle = FontStyle.Bold;
            indicator.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
            Place(indicator.rectTransform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-24f, -20f), new Vector2(720f, 56f), new Vector2(1f, 1f));

            var panel = Panel(root.transform, "Panel", PanelShade);
            Stretch(panel.rectTransform);

            var box = Panel(panel.transform, "Kutu", BoxColor);
            Place(box.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 360f), new Vector2(0.5f, 0.5f));

            var title = Label(box.transform, "Baslik", font, 48, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            title.text = ConnectionMessages.PanelTitle;
            Place(title.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -30f), new Vector2(-60f, 70f), new Vector2(0.5f, 1f));

            var detail = Label(box.transform, "Aciklama", font, 26, TextAnchor.MiddleCenter);
            detail.text = ConnectionMessages.UnreachableDetail;
            Place(detail.rectTransform, new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, 10f), new Vector2(-80f, 130f), new Vector2(0.5f, 0.5f));

            var button = Panel(box.transform, "TekrarDene", ButtonColor);
            Place(button.rectTransform, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 36f), new Vector2(280f, 64f), new Vector2(0.5f, 0f));
            var retry = button.gameObject.AddComponent<Button>();
            retry.targetGraphic = button;

            var retryLabel = Label(button.transform, "Yazi", font, 28, TextAnchor.MiddleCenter);
            retryLabel.text = ConnectionMessages.RetryLabel;
            Stretch(retryLabel.rectTransform);

            var data = new SerializedObject(view);
            data.FindProperty("_monitor").objectReferenceValue = monitor;
            data.FindProperty("_indicator").objectReferenceValue = indicator;
            data.FindProperty("_panel").objectReferenceValue = panel.gameObject;
            data.FindProperty("_title").objectReferenceValue = title;
            data.FindProperty("_detail").objectReferenceValue = detail;
            data.FindProperty("_retryButton").objectReferenceValue = retry;
            data.FindProperty("_retryLabel").objectReferenceValue = retryLabel;
            data.ApplyModifiedPropertiesWithoutUndo();

            panel.gameObject.SetActive(false);

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static Text Label(Transform parent, string name, Font font, int size, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.color = TextColor;
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

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
