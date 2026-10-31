using BuyuFisiltisi.Gameplay.Player;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Forest
{
    public sealed class JumpMushroom : MonoBehaviour
    {
        [SerializeField] private Transform _player;
        [SerializeField] private float _radius = 0.9f;
        [SerializeField] private float _launchSpeed = 13f;
        [SerializeField] private Transform _cap;

        private WitchMotor _motor;
        private float _squash;

        private void Update()
        {
            if (_player == null)
                return;

            if (_motor == null)
                _motor = _player.GetComponentInParent<WitchMotor>();

            var offset = _player.position - transform.position;
            var height = offset.y;
            offset.y = 0f;
            if (_motor != null && offset.magnitude <= _radius && height > -0.3f && height < 0.9f && _motor.Velocity.y <= 0.1f)
            {
                _motor.Launch(_launchSpeed);
                _squash = 1f;
            }

            if (_cap != null)
            {
                _squash = Mathf.MoveTowards(_squash, 0f, Time.deltaTime * 4f);
                _cap.localScale = new Vector3(1f + _squash * 0.3f, 1f - _squash * 0.4f, 1f + _squash * 0.3f);
            }
        }
    }
}
