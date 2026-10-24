using BuyuFisiltisi.Core.Companion;
using BuyuFisiltisi.Core.Events;
using UnityEngine;

namespace BuyuFisiltisi.Bootstrap
{
    [DefaultExecutionOrder(-1000)]
    public sealed class GameBootstrap : MonoBehaviour
    {
        public IEventBus Bus { get; private set; }

        private void Awake()
        {
            Bus = new EventBus();

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

            var injected = 0;
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IRequiresEventBus needsBus)
                {
                    needsBus.InjectEventBus(Bus);
                    injected++;
                }

                if (receiver != null && behaviour is ICompanionCommandIssuer issuer)
                    issuer.SetCommandReceiver(receiver);
            }

            Debug.Log($"[Bootstrap] Olay yolu kuruldu, {injected} bileşene bağlandı.", this);
        }
    }
}
