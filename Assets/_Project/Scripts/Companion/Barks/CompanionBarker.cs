using System;
using BuyuFisiltisi.Companion.Brain;
using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Companion.Barks
{
    public sealed class CompanionBarker : MonoBehaviour, IRequiresEventBus
    {
        [SerializeField] private float _cooldown = 4f;

        private IEventBus _eventBus;
        private ICompanionBarkProvider _provider;

        private void Awake()
        {
            EnsureProvider();
        }

        private void EnsureProvider()
        {
            _provider ??= new BarkPool(DefaultBarkLines.Create(), _cooldown, new System.Random());
        }

        public void SetProvider(ICompanionBarkProvider provider)
        {
            _provider = provider;
        }

        public void InjectEventBus(IEventBus eventBus)
        {
            Unsubscribe();
            _eventBus = eventBus;
            if (_eventBus == null)
                return;

            _eventBus.Subscribe<CompanionStateChangedEvent>(OnStateChanged);
            _eventBus.Subscribe<CompanionTaskFinishedEvent>(OnTaskFinished);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<CompanionStateChangedEvent>(OnStateChanged);
            _eventBus.Unsubscribe<CompanionTaskFinishedEvent>(OnTaskFinished);
        }

        private void OnStateChanged(CompanionStateChangedEvent gameEvent)
        {
            if (!Enum.TryParse(gameEvent.CurrentState, out CompanionStateId state))
                return;

            switch (state)
            {
                case CompanionStateId.Following:
                    Say(BarkTrigger.Following);
                    break;
                case CompanionStateId.Waiting:
                    Say(BarkTrigger.Waiting);
                    break;
                case CompanionStateId.GoingToTarget:
                    Say(BarkTrigger.GoingToTarget);
                    break;
            }
        }

        private void OnTaskFinished(CompanionTaskFinishedEvent gameEvent)
        {
            if (gameEvent.Outcome == CompanionTaskOutcome.Cancelled)
                return;

            Say(gameEvent.Outcome == CompanionTaskOutcome.Succeeded ? BarkTrigger.TaskSucceeded : BarkTrigger.TaskFailed);
        }

        private void Say(BarkTrigger trigger)
        {
            if (_eventBus == null)
                return;

            EnsureProvider();

            if (_provider.TryGetBark(trigger, Time.time, out var text))
                _eventBus.Publish(new CompanionBarkEvent(text));
        }
    }
}
