using System.Collections;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using BuyuFisiltisi.Gameplay.World;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.DebugTools
{
    [DefaultExecutionOrder(-500)]
    public sealed class BridgeTestRig : MonoBehaviour
    {
        [SerializeField] private BridgeSwitch _switch;
        [SerializeField] private NavMeshSurface _surface;
        [SerializeField] private NavMeshAgent _walker;
        [SerializeField] private Transform _walkerGoal;

        private EventBus _eventBus;
        private string _switchStatus = "Bekliyor";
        private string _bridgeStatus = "Kalkık";
        private string _walkerStatus = "Uçurumun önünde bekliyor";

        private void Awake()
        {
            _eventBus = new EventBus();
            _eventBus.Subscribe<SwitchPulledEvent>(OnSwitchPulled);
            _eventBus.Subscribe<BridgeLoweredEvent>(OnBridgeLowered);

            foreach (var behaviour in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
            {
                if (behaviour is IRequiresEventBus receiver)
                    receiver.InjectEventBus(_eventBus);
            }
        }

        private void Start()
        {
            if (_surface != null)
                _surface.BuildNavMesh();

            if (_walker != null)
                _walker.gameObject.SetActive(true);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || _switch == null)
                return;

            if (keyboard.bKey.wasPressedThisFrame && _switch.CanInteract(gameObject))
                _switch.Interact(gameObject);

            if (_walker != null && _walker.isOnNavMesh && _walker.hasPath && !_walker.pathPending &&
                _walker.remainingDistance <= _walker.stoppingDistance + 0.1f)
                _walkerStatus = "Karşıya geçti";
        }

        private void OnSwitchPulled(SwitchPulledEvent gameEvent)
        {
            _switchStatus = "Çekildi (" + gameEvent.SwitchId + ")";
            _bridgeStatus = "İniyor...";
        }

        private void OnBridgeLowered(BridgeLoweredEvent gameEvent)
        {
            _bridgeStatus = "İndi, NavMesh güncellendi";
            if (_walker != null && _walkerGoal != null)
                StartCoroutine(SendWalker());
        }

        private IEnumerator SendWalker()
        {
            yield return null;
            yield return null;

            if (!_walker.isOnNavMesh)
            {
                _walkerStatus = "NavMesh üzerinde değil";
                yield break;
            }

            var path = new NavMeshPath();
            _walker.CalculatePath(_walkerGoal.position, path);
            if (path.status != NavMeshPathStatus.PathComplete)
            {
                _walkerStatus = "Yol bulunamadı (" + path.status + ")";
                yield break;
            }

            _walker.isStopped = false;
            _walker.SetDestination(_walkerGoal.position);
            _walkerStatus = "Köprüden geçiyor";
        }

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20, 20, 380, 150), GUI.skin.box);
            GUILayout.Label("B tuşu: şalteri çek");
            GUILayout.Label("Şalter: " + _switchStatus);
            GUILayout.Label("Köprü: " + _bridgeStatus);
            GUILayout.Label("Yürüyücü: " + _walkerStatus);
            if (_walker != null && _walker.isActiveAndEnabled)
                GUILayout.Label("Hız: " + _walker.velocity.magnitude.ToString("0.0") + " m/sn  Yol: " + _walker.pathStatus);
            GUILayout.EndArea();
        }
    }
}
