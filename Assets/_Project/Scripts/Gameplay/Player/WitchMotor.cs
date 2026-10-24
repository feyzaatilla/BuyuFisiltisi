using BuyuFisiltisi.Gameplay.World;
using UnityEngine;

namespace BuyuFisiltisi.Gameplay.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class WitchMotor : MonoBehaviour, IRespawnable
    {
        [SerializeField] private Transform _cameraTransform;
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _sprintSpeed = 6.5f;
        [SerializeField] private float _turnSpeed = 720f;
        [SerializeField] private float _jumpHeight = 0.7f;
        [SerializeField] private float _gravity = -20f;
        [SerializeField] private float _groundStick = 2f;

        private CharacterController _controller;
        private IPlayerInput _input;
        private float _verticalSpeed;

        public Vector3 Velocity { get; private set; }
        public bool IsGrounded => _controller != null && _controller.isGrounded;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _input ??= new KeyboardMousePlayerInput();
            if (_cameraTransform == null && Camera.main != null)
                _cameraTransform = Camera.main.transform;
        }

        public void SetInput(IPlayerInput input)
        {
            _input = input;
        }

        public void SetCamera(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        private void Update()
        {
            var cameraForward = _cameraTransform != null ? _cameraTransform.forward : Vector3.forward;
            var direction = MovementMath.CameraRelative(_input.Move, cameraForward);
            var grounded = _controller.isGrounded;
            var speed = grounded && _input.Sprint ? _sprintSpeed : _walkSpeed;

            _verticalSpeed = MovementMath.NextVerticalSpeed(_verticalSpeed, grounded, _gravity, Time.deltaTime, _groundStick);
            if (grounded && _input.JumpPressed)
                _verticalSpeed = MovementMath.JumpSpeed(_jumpHeight, _gravity);
            var horizontal = direction * speed;
            Velocity = new Vector3(horizontal.x, _verticalSpeed, horizontal.z);
            _controller.Move(Velocity * Time.deltaTime);

            if (direction.sqrMagnitude > 0.0001f)
            {
                var target = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turnSpeed * Time.deltaTime);
            }
        }

        public void Respawn(Vector3 position, Quaternion rotation)
        {
            _controller.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            _verticalSpeed = 0f;
            Velocity = Vector3.zero;
            _controller.enabled = true;
        }
    }
}
