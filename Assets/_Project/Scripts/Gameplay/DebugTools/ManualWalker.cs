using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace BuyuFisiltisi.Gameplay.DebugTools
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class ManualWalker : MonoBehaviour
    {
        [SerializeField] private float _speed = 4f;
        [SerializeField] private float _turnSpeed = 720f;

        private NavMeshAgent _agent;

        public bool IsMoving { get; private set; }

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            IsMoving = false;
            var keyboard = Keyboard.current;
            if (keyboard == null || !_agent.isOnNavMesh)
                return;

            var input = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;

            if (input == Vector2.zero)
                return;

            var direction = ToWorld(input.normalized);
            if (_agent.hasPath)
                _agent.ResetPath();

            _agent.Move(direction * (_speed * Time.deltaTime));
            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, _turnSpeed * Time.deltaTime);
            IsMoving = true;
        }

        private static Vector3 ToWorld(Vector2 input)
        {
            var camera = Camera.main;
            if (camera == null)
                return new Vector3(input.x, 0f, input.y);

            var forward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
            var right = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;
            return (forward * input.y + right * input.x).normalized;
        }
    }
}
