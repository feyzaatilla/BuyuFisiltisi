using BuyuFisiltisi.Companion.Movement;
using BuyuFisiltisi.Companion.Sensors;
using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Brain
{
    [RequireComponent(typeof(CompanionMover))]
    public sealed class CompanionController : MonoBehaviour, ICompanionCommandReceiver, IRequiresEventBus, ICompanionBody
    {
        [SerializeField] private Transform player;
        [SerializeField] private float taskTimeout = 20f;
        [SerializeField] private bool followOnStart = true;
        [SerializeField] private StuckDetectorSettings stuckSettings = new StuckDetectorSettings();

        private CompanionMover mover;
        private CompanionBrain brain;
        private StuckDetector stuckDetector;
        private IEventBus eventBus;

        public CompanionStateId State => brain != null ? brain.State : CompanionStateId.Waiting;

        public GameObject Self => gameObject;
        public bool HasArrived => mover.HasArrived;
        public bool IsStuck => stuckDetector.Current;

        private void Awake()
        {
            mover = GetComponent<CompanionMover>();
            stuckDetector = new StuckDetector(() => mover.CurrentSpeed, stuckSettings);
            brain = new CompanionBrain(this, taskTimeout);
            brain.StateChanged += OnStateChanged;
            brain.TaskFinished += OnTaskFinished;
        }

        private void Start()
        {
            if (followOnStart && player != null)
                brain.Issue(CompanionCommand.FollowPlayer());
        }

        private void OnDestroy()
        {
            if (brain == null)
                return;

            brain.StateChanged -= OnStateChanged;
            brain.TaskFinished -= OnTaskFinished;
        }

        private void Update()
        {
            if (brain.State == CompanionStateId.GoingToTarget)
                stuckDetector.Sense(Time.deltaTime);

            brain.Tick(Time.deltaTime);
        }

        public void SetPlayer(Transform target)
        {
            player = target;
        }

        public void InjectEventBus(IEventBus bus)
        {
            eventBus = bus;
        }

        public void IssueCommand(CompanionCommand command)
        {
            if (command == null)
                return;

            Debug.Log($"[Fısıl] Emir: {command}", this);
            brain.Issue(command);
        }

        public void FollowPlayer()
        {
            if (player == null)
            {
                Debug.LogWarning("[Fısıl] Takip edilecek oyuncu atanmamış.", this);
                mover.Stop();
                return;
            }

            mover.Follow(player);
        }

        public bool MoveTo(Vector3 destination)
        {
            stuckDetector.Reset();
            return mover.MoveTo(destination);
        }

        public void Stop()
        {
            stuckDetector.Reset();
            mover.Stop();
        }

        private void OnStateChanged(CompanionStateId previous, CompanionStateId current)
        {
            eventBus?.Publish(new CompanionStateChangedEvent(previous.ToString(), current.ToString()));
        }

        private void OnTaskFinished(string taskName, CompanionTaskOutcome outcome, float duration)
        {
            Debug.Log($"[Fısıl] Görev bitti: {taskName} → {outcome} ({duration:0.0} sn)", this);
            eventBus?.Publish(new CompanionTaskFinishedEvent(taskName, outcome, duration));
        }
    }
}
