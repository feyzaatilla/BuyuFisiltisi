using BuyuFisiltisi.Gameplay.World;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public abstract class ForestTrap : MonoBehaviour
    {
        [SerializeField] protected Transform _player;
        [SerializeField] protected Transform _respawnPoint;
        [SerializeField] private float _warningRadius = 7f;
        [SerializeField] private string _warning = "Fısıl: Dikkat et, burada bir tuzak var!";

        private bool _warned;

        protected float FlatDistanceToPlayer()
        {
            if (_player == null)
                return float.MaxValue;

            var offset = _player.position - transform.position;
            offset.y = 0f;
            return offset.magnitude;
        }

        protected virtual void Update()
        {
            if (_warned || _player == null)
                return;

            if (FlatDistanceToPlayer() <= _warningRadius)
            {
                _warned = true;
                GameMessage.Show(_warning, 3f);
            }
        }

        protected void SendPlayerBack(string message)
        {
            GameMessage.Show(message, 3f);
            if (_player == null || _respawnPoint == null)
                return;

            var respawnable = _player.GetComponentInParent<IRespawnable>();
            if (respawnable != null)
                respawnable.Respawn(_respawnPoint.position, _respawnPoint.rotation);
        }
    }
}
