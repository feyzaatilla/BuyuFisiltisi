using BuyuFisiltisi.Core.Events;
using BuyuFisiltisi.Core.GameEvents;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class GameSoundListener : MonoBehaviour, IRequiresEventBus
    {
        private IEventBus _eventBus;

        public void InjectEventBus(IEventBus eventBus)
        {
            Unsubscribe();
            _eventBus = eventBus;
            if (_eventBus == null)
                return;

            _eventBus.Subscribe<ItemCollectedEvent>(OnItemCollected);
            _eventBus.Subscribe<RecipeCompletedEvent>(OnRecipeCompleted);
            _eventBus.Subscribe<CloudDispersedEvent>(OnCloudDispersed);
            _eventBus.Subscribe<BridgeLoweredEvent>(OnBridgeLowered);
            _eventBus.Subscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Unsubscribe()
        {
            if (_eventBus == null)
                return;

            _eventBus.Unsubscribe<ItemCollectedEvent>(OnItemCollected);
            _eventBus.Unsubscribe<RecipeCompletedEvent>(OnRecipeCompleted);
            _eventBus.Unsubscribe<CloudDispersedEvent>(OnCloudDispersed);
            _eventBus.Unsubscribe<BridgeLoweredEvent>(OnBridgeLowered);
            _eventBus.Unsubscribe<DeedRecordedEvent>(OnDeedRecorded);
        }

        private void OnItemCollected(ItemCollectedEvent gameEvent)
        {
            GameSound.Play(SoundKind.Pickup);
        }

        private void OnRecipeCompleted(RecipeCompletedEvent gameEvent)
        {
            GameSound.Play(SoundKind.Brew);
        }

        private void OnCloudDispersed(CloudDispersedEvent gameEvent)
        {
            GameSound.Play(SoundKind.Cloud);
        }

        private void OnBridgeLowered(BridgeLoweredEvent gameEvent)
        {
            GameSound.Play(SoundKind.Bridge, 0.8f);
        }

        private void OnDeedRecorded(DeedRecordedEvent gameEvent)
        {
            GameSound.Play(SoundKind.Deed, 0.45f);
        }
    }
}
