using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Gameplay.Commands;
using Unity.AI.Navigation;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.DebugTools
{
    [DefaultExecutionOrder(-500)]
    public sealed class WitchTestRig : MonoBehaviour
    {
        [SerializeField] private NavMeshSurface[] _surfaces = new NavMeshSurface[0];
        [SerializeField] private GameObject[] _activateAfterBake = new GameObject[0];
        [SerializeField] private CompanionCommandKeyboardInput _commandInput;

        private EventBus _eventBus;
        private string _companionState = "Bekliyor";
        private string _lastTask = "Yok";
        private string _bridgeStatus = "Kalkık";

        private void Awake()
        {
            _eventBus = new EventBus();
            _eventBus.Subscribe<CompanionStateChangedEvent>(OnCompanionStateChanged);
            _eventBus.Subscribe<CompanionTaskFinishedEvent>(OnCompanionTaskFinished);
            _eventBus.Subscribe<SwitchPulledEvent>(OnSwitchPulled);
            _eventBus.Subscribe<BridgeLoweredEvent>(OnBridgeLowered);

            var behaviours = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            ICompanionCommandReceiver receiver = null;
            foreach (var behaviour in behaviours)
            {
                if (behaviour is ICompanionCommandReceiver found)
                {
                    receiver = found;
                    break;
                }
            }

            foreach (var behaviour in behaviours)
            {
                if (behaviour is IRequiresEventBus needsBus)
                    needsBus.InjectEventBus(_eventBus);

                if (receiver != null && behaviour is ICompanionCommandIssuer issuer)
                    issuer.SetCommandReceiver(receiver);
            }

            if (receiver == null)
                Debug.LogWarning("[Cadı Testi] Sahnede Fısıl bulunamadı, emirler gitmeyecek.", this);
        }

        private void Start()
        {
            foreach (var surface in _surfaces)
            {
                if (surface != null)
                    surface.BuildNavMesh();
            }

            foreach (var target in _activateAfterBake)
            {
                if (target != null)
                    target.SetActive(true);
            }
        }

        private void OnCompanionStateChanged(CompanionStateChangedEvent gameEvent)
        {
            _companionState = gameEvent.CurrentState;
        }

        private void OnCompanionTaskFinished(CompanionTaskFinishedEvent gameEvent)
        {
            _lastTask = gameEvent.TaskName + " → " + gameEvent.Outcome + " (" + gameEvent.DurationSeconds.ToString("0.0") + " sn)";
        }

        private void OnSwitchPulled(SwitchPulledEvent gameEvent)
        {
            _bridgeStatus = "İniyor...";
        }

        private void OnBridgeLowered(BridgeLoweredEvent gameEvent)
        {
            _bridgeStatus = "İndi, karşıya geçebilirsin";
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 430, 190), GUI.skin.box);
            GUILayout.Label("WASD / oklar: yürü   Shift: koş   Space: zıpla");
            GUILayout.Label("Sağ tık basılı + fare: kamerayı çevir   Tekerlek: yakınlaş");
            GUILayout.Label("F: Fısıl yanıma gel   G: bekle   B: şalteri çek");
            GUILayout.Label("Son emir: " + (_commandInput != null ? _commandInput.LastCommand : "-"));
            GUILayout.Label("Fısıl: " + _companionState);
            GUILayout.Label("Son görev: " + _lastTask);
            GUILayout.Label("Köprü: " + _bridgeStatus);
            GUILayout.EndArea();
        }
    }
}
