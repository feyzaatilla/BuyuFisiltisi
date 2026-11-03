using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class SwingingLog : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private RootParkour _parkour;
        [SerializeField] private Transform _log;
        [SerializeField] private float _angle = 60f;
        [SerializeField] private float _speed = 1.4f;
        [SerializeField] private float _hitRadius = 1f;

        private void Update()
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * _speed) * _angle);
            if (_player == null || _log == null || _parkour == null)
                return;

            if (Vector3.Distance(_player.position, _log.position) <= _hitRadius)
                _parkour.Fail("Sallanan kütük sana çarptı! Zamanlamanı ayarla.");
        }
    }
}
