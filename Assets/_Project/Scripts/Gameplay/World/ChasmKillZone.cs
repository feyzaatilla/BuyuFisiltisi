using BuyuFisiltisi.Core.Navigation;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.World
{
    [RequireComponent(typeof(Collider))]
    public sealed class ChasmKillZone : MonoBehaviour
    {
        [SerializeField] private Transform _respawnPoint;
        [SerializeField] private LayerMask _affectedLayers = 1 << GameLayers.Player;

        public Transform RespawnPoint
        {
            get => _respawnPoint;
            set => _respawnPoint = value;
        }

        private void Reset()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!KillZoneRule.Affects(_affectedLayers.value, other.gameObject.layer))
                return;

            if (_respawnPoint == null)
            {
                Debug.LogWarning("[Uçurum] Dönüş noktası atanmamış.", this);
                return;
            }

            var respawnable = other.GetComponentInParent<IRespawnable>();
            if (respawnable != null)
            {
                respawnable.Respawn(_respawnPoint.position, _respawnPoint.rotation);
                return;
            }

            var controller = other.GetComponentInParent<CharacterController>();
            var target = controller != null ? controller.transform : other.transform;
            if (controller != null)
                controller.enabled = false;

            target.SetPositionAndRotation(_respawnPoint.position, _respawnPoint.rotation);

            var body = other.attachedRigidbody;
            if (body != null && !body.isKinematic)
                body.linearVelocity = Vector3.zero;

            if (controller != null)
                controller.enabled = true;
        }
    }
}
