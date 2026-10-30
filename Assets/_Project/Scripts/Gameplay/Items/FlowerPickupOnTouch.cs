using BuyuFisiltisi.Core.Navigation;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Items
{
    [RequireComponent(typeof(CollectibleFlower))]
    public sealed class FlowerPickupOnTouch : MonoBehaviour
    {
        [SerializeField] private float _radius = 1.2f;

        private CollectibleFlower _flower;

        private void Awake()
        {
            _flower = GetComponent<CollectibleFlower>();

            var trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = _radius / Mathf.Max(0.01f, transform.lossyScale.x);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.layer != GameLayers.Player)
                return;

            if (_flower.CanInteract(other.gameObject))
                _flower.Interact(other.gameObject);
        }
    }
}
