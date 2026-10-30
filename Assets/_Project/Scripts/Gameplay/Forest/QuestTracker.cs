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

        private readonly QuestLog _log = new QuestLog(new[]
        {
            new QuestStep(CloudsStep, "Kara bulutlara yaklaş, sol tıkla dağıt"),
            new QuestStep(BridgeStep, "Kitaptan (Tab) Fısıl'a şalteri çektir, köprüyü indir"),
            new QuestStep(FlowersStep, "Altı çiçeği topla (girintidekini kitaptan Fısıl'a getirt)"),
            new QuestStep(TeaStep, "Büyü kazanına tıkla, üç tarifi demle"),
            new QuestStep(FireflyStep, "Kitaptan Fısıl'la ateş böceğine çay götür"),
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
            _eventBus.Subscribe<TrustChangedEvent>(OnTrustChanged);
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
            _eventBus.Unsubscribe<TrustChangedEvent>(OnTrustChanged);
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

        private void OnTrustChanged(TrustChangedEvent changed)
        {
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

            var width = Screen.width;
            GUI.Label(new Rect(0f, Screen.height - 190f, width, 70f), "Görev: " + CurrentText(), _taskStyle);

            if (Time.unscaledTime < _toastUntil || _log.IsFinished)
                GUI.Label(new Rect(0f, Screen.height - 240f, width, 50f), _toast, _toastStyle);
        }
    }
}
