using System.Collections;
using System.Collections.Generic;
using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Testing
{
    public sealed class CompanionTrialRunner : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private CompanionController _companion;
        [SerializeField] private CompanionTrialTarget _target;
        [SerializeField] private Transform[] _startPoints = new Transform[0];
        [SerializeField] private float _startSampleRadius = 3f;
        [SerializeField] private int _trialCount = 10;
        [SerializeField] private float _pauseSeconds = 1f;
        [SerializeField] private float _trialTimeoutSeconds = 30f;
        [SerializeField] private float _summarySeconds = 20f;
        [SerializeField] private Key _startKey = Key.K;

        private IEventBus _bus;
        private CompanionTrialSession _session;
        private bool _dialogueActive;
        private bool _waiting;
        private string _currentStart;
        private float _hideAt;
        private GUIStyle _style;

        public bool IsRunning { get; private set; }
        public CompanionTrialSession Session => _session;

        public void InjectEventBus(IEventBus eventBus)
        {
            Unsubscribe();
            _bus = eventBus;
            if (_bus == null)
                return;

            _bus.Subscribe<CompanionTaskFinishedEvent>(OnTaskFinished);
            _bus.Subscribe<DialogueFocusChangedEvent>(OnDialogueFocusChanged);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_bus == null)
                return;

            _bus.Unsubscribe<CompanionTaskFinishedEvent>(OnTaskFinished);
            _bus.Unsubscribe<DialogueFocusChangedEvent>(OnDialogueFocusChanged);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null || IsRunning || _dialogueActive)
                return;

            if (keyboard[_startKey].wasPressedThisFrame)
                StartTrials();
        }

        public void StartTrials()
        {
            if (IsRunning)
                return;

            if (_companion == null || _target == null)
            {
                Debug.LogWarning("[Fısıl Testi] Fısıl ya da test hedefi atanmamış.", this);
                return;
            }

            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            IsRunning = true;
            _session = new CompanionTrialSession(_trialCount, _target.TaskName);
            var agent = _companion.GetComponent<NavMeshAgent>();
            var origin = _companion.transform.position;
            var starts = ValidStartPoints();

            Debug.Log($"[Fısıl Testi] Başladı: {_trialCount} deneme, {Mathf.Max(1, starts.Count)} başlangıç noktası, hedef: {_target.DisplayName}", this);

            while (!_session.IsComplete)
            {
                var index = _session.Attempts;
                var startPoint = starts.Count > 0 ? starts[index % starts.Count] : null;
                _currentStart = startPoint != null ? startPoint.name : "Başlangıç";
                var start = startPoint != null ? startPoint.position : origin;

                _companion.IssueCommand(CompanionCommand.Wait());
                if (agent != null && agent.isOnNavMesh)
                    agent.Warp(Snap(agent, start));

                _target.ResetForTrial();
                yield return null;

                _waiting = true;
                var startedAt = Time.time;
                _companion.IssueCommand(CompanionCommand.InteractWith(_target));

                while (_waiting && Time.time - startedAt < _trialTimeoutSeconds)
                    yield return null;

                if (_waiting)
                {
                    _waiting = false;
                    _session.RecordTimeout(_trialTimeoutSeconds, _currentStart);
                    _companion.IssueCommand(CompanionCommand.Wait());
                }

                Debug.Log($"[Fısıl Testi] {_session.Attempts}/{_trialCount} ({_currentStart}): {_session.LastOutcome} ({_session.LastSeconds:0.0} sn)", this);
                yield return new WaitForSeconds(_pauseSeconds);
            }

            if (agent != null && agent.isOnNavMesh)
                agent.Warp(Snap(agent, origin));

            _companion.IssueCommand(CompanionCommand.FollowPlayer());
            Debug.Log($"[Fısıl Testi] Sonuç: {_session.Summary()} ({_session.Details()})", this);
            if (starts.Count > 0)
                Debug.Log($"[Fısıl Testi] Başlangıç noktalarına göre: {_session.StartBreakdown()}", this);
            _hideAt = Time.time + _summarySeconds;
            IsRunning = false;
        }

        private void OnTaskFinished(CompanionTaskFinishedEvent gameEvent)
        {
            if (!_waiting || _session == null)
                return;

            if (_session.Record(gameEvent.TaskName, gameEvent.Outcome, gameEvent.DurationSeconds, _currentStart))
                _waiting = false;
        }

        private List<Transform> ValidStartPoints()
        {
            var result = new List<Transform>();
            if (_startPoints == null)
                return result;

            foreach (var point in _startPoints)
            {
                if (point != null)
                    result.Add(point);
            }

            return result;
        }

        private Vector3 Snap(NavMeshAgent agent, Vector3 position)
        {
            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            return NavMesh.SamplePosition(position, out var hit, _startSampleRadius, filter) ? hit.position : position;
        }

        private void OnDialogueFocusChanged(DialogueFocusChangedEvent gameEvent)
        {
            _dialogueActive = gameEvent.IsActive;
        }

        private void OnGUI()
        {
            if (_session == null || (!IsRunning && Time.time > _hideAt))
                return;

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.box)
                {
                    fontSize = 22,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
            }

            var text = IsRunning
                ? $"Fısıl testi: deneme {_session.Attempts + 1}/{_trialCount}, başarılı {_session.Successes}"
                : $"Fısıl testi bitti: {_session.Summary()}";

            var width = Mathf.Min(760f, Screen.width - 40f);
            GUI.Box(new Rect((Screen.width - width) * 0.5f, 60f, width, 56f), text, _style);
        }
    }
}
