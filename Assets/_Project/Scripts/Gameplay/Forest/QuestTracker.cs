using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Core.Items;
using BuyuFisiltisi.Core.Progress;
using BuyuFisiltisi.Gameplay.Items;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class QuestTracker : MonoBehaviour, IRequiresEventBus
    {
        private const string CloudsStep = "bulutlar";
        private const string BridgeStep = "kopru";
        private const string FlowersStep = "cicekler";
        private const string TeaStep = "cay";
        private const string FireflyStep = "ates_bocegi";
        private const string RootStep = "kok";
        private const string TalkStep = "mese";
        private const string DoorStep = "kapi";
        private const int FlowersNeeded = 6;
        private const float ToastSeconds = 4f;

        private bool _talking;

        private readonly QuestLog _log = new QuestLog(new[]
        {
            new QuestStep(CloudsStep, "Kara bulutlara yaklaş, sol tıkla dağıt"),
            new QuestStep(BridgeStep, "Şaltere yaklaş ya da tıkla, Fısıl çeksin ve köprü insin"),
            new QuestStep(FlowersStep, "Altı çiçeği topla (girintidekini kitaptan Fısıl'a getirt)"),
            new QuestStep(TeaStep, "Büyü kazanına tıkla, üç tarifi demle"),
            new QuestStep(FireflyStep, "Işık Çayıyla ateş böceğine git, ona dokun"),
            new QuestStep(RootStep, "Meşe'nin yaralı köküne merhem sür"),
            new QuestStep(TalkStep, "Yaşlı Meşe'ye git, E ile konuş"),
            new QuestStep(DoorStep, "Meşe'nin güvenini kazan, kapıyı açtır")
        });

        private IEventBus _eventBus;
        private int _flowers;
        private readonly System.Collections.Generic.HashSet<string> _recipes = new System.Collections.Generic.HashSet<string>();
        private int _clouds;
        private int _cloudsTotal;
        private string _toast = string.Empty;
        private float _toastUntil;
        private GUIStyle _taskStyle;
        private GUIStyle _toastStyle;

        public QuestLog Log => _log;

        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            Unsubscribe();
            _eventBus = eventBus;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void Subscribe()
        {
            if (_eventBus == null)
                return;

            _eventBus.Subscribe<CloudDispersedEvent>(OnCloudDispersed);
            _eventBus.Subscribe<SwitchPulledEvent>(OnSwitchPulled);
            _eventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            _eventBus.Subscribe<RecipeCompletedEvent>(OnRecipeCompleted);
            _eventBus.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
            _eventBus.Subscribe<DialogueFocusChangedEvent>(OnDialogueFocusChanged);
            _eventBus.Subscribe<DoorOpenRequestedEvent>(OnDoorOpenRequested);
        }

        private void Unsubscribe()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<CloudDispersedEvent>(OnCloudDispersed);
            _eventBus.Unsubscribe<SwitchPulledEvent>(OnSwitchPulled);
            _eventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
            _eventBus.Unsubscribe<RecipeCompletedEvent>(OnRecipeCompleted);
            _eventBus.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
            _eventBus.Unsubscribe<DialogueFocusChangedEvent>(OnDialogueFocusChanged);
            _eventBus.Unsubscribe<DoorOpenRequestedEvent>(OnDoorOpenRequested);
        }

        private void OnCloudDispersed(CloudDispersedEvent dispersed)
        {
            _clouds = dispersed.Dispersed;
            _cloudsTotal = dispersed.Total;
            if (_clouds >= _cloudsTotal)
                Complete(CloudsStep);
        }

        private void OnSwitchPulled(SwitchPulledEvent pulled)
        {
            Complete(BridgeStep);
        }

        private void OnItemCollected(ItemCollectedEvent collected)
        {
            if (!ItemCatalog.IsFlower(collected.ItemId))
                return;

            _flowers += collected.Amount;
            if (_flowers >= FlowersNeeded)
                Complete(FlowersStep);
        }

        private void OnRecipeCompleted(RecipeCompletedEvent completed)
        {
            _recipes.Add(completed.RecipeId);
            if (_recipes.Count >= Recipe.All.Count)
                Complete(TeaStep);
        }

        private void OnDeedRecorded(DeedRecordedEvent deed)
        {
            if (deed.DeedId == Firefly.HealedDeedId)
                Complete(FireflyStep);

            if (deed.DeedId == WoundedRoot.HealedDeedId)
                Complete(RootStep);
        }

        private void OnDialogueFocusChanged(DialogueFocusChangedEvent focus)
        {
            if (focus.IsActive)
                _talking = true;
            else if (_talking)
                Complete(TalkStep);
        }

        private void OnDoorOpenRequested(DoorOpenRequestedEvent requested)
        {
            Complete(TalkStep);
            Complete(DoorStep);
        }

        private void Complete(string stepId)
        {
            var text = _log.GetText(stepId);
            if (!_log.Complete(stepId))
                return;

            _toast = _log.IsFinished
                ? "Tebrikler! Orman Kalbi'ne ulaştın."
                : "Görev tamamlandı: " + text;
            _toastUntil = Time.unscaledTime + ToastSeconds;
            Debug.Log("[Görev] " + _toast);
        }

        private string CurrentText()
        {
            var current = _log.Current;
            if (!current.HasValue)
                return "Tüm görevler tamamlandı";

            if (current.Value.Id == CloudsStep && _cloudsTotal > 0)
                return current.Value.Text + ": " + _clouds + "/" + _cloudsTotal;

            if (current.Value.Id == FlowersStep)
                return current.Value.Text + ": " + Mathf.Min(_flowers, FlowersNeeded) + "/" + FlowersNeeded;

            if (current.Value.Id == TeaStep)
                return current.Value.Text + ": " + _recipes.Count + "/" + Recipe.All.Count;

            return current.Value.Text;
        }

        private void OnGUI()
        {
            if (Time.timeScale == 0f)
                return;

            if (_taskStyle == null)
            {
                _taskStyle = new GUIStyle(GUI.skin.label) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter, wordWrap = true };
                _taskStyle.normal.textColor = Color.white;
                _toastStyle = new GUIStyle(_taskStyle) { fontSize = 26 };
                _toastStyle.normal.textColor = new Color(0.6f, 1f, 0.6f);
            }

            var width = Screen.width * 0.5f;
            var x = Screen.width * 0.25f;
            DrawOnPanel(new Rect(x, 18f, width, 56f), "Görev: " + CurrentText(), _taskStyle);

            if (Time.unscaledTime < _toastUntil)
                DrawOnPanel(new Rect(x, 70f, width, 70f), _toast, _toastStyle);
        }

        private void DrawOnPanel(Rect rect, string text, GUIStyle style)
        {
            var content = new GUIContent(text);
            var size = style.CalcSize(content);
            var textWidth = Mathf.Min(size.x, rect.width);
            var textHeight = style.CalcHeight(content, textWidth);
            var back = new Rect(rect.x + (rect.width - textWidth) * 0.5f - 14f, rect.y - 4f, textWidth + 28f, textHeight + 8f);
            var old = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.45f);
            GUI.DrawTexture(back, Texture2D.whiteTexture);
            GUI.color = old;
            GUI.Label(new Rect(back.x + 14f, rect.y, textWidth, textHeight), text, style);
        }
    }
}
