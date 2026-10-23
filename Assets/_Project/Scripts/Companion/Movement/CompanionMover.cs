using BuyuFisiltisi.Core.Navigation;
using UnityEngine;
using UnityEngine.AI;

namespace BuyuFisiltisi.Companion.Movement
{
    [RequireComponent(typeof(NavMeshAgent))]
    public sealed class CompanionMover : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float followDistance = 1.5f;
        [SerializeField] private float arriveTolerance = 0.2f;
        [SerializeField] private float repathInterval = 0.25f;
        [SerializeField] private float sampleRadius = 1f;

        private NavMeshAgent agent;
        private Transform followTarget;
        private float nextRepathTime;

        public bool IsFollowing => followTarget != null;
        public float FollowDistance => followDistance;
        public float CurrentSpeed => agent != null && agent.isOnNavMesh ? agent.velocity.magnitude : 0f;

        public bool HasArrived =>
            agent != null && agent.isOnNavMesh && !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + arriveTolerance;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;

            if (AgentTypeLookup.TryFindId(AgentTypes.Fisil.Name, out var id))
            {
                if (agent.agentTypeID != id)
                {
                    agent.agentTypeID = id;
                    agent.Warp(transform.position);
                }
            }
            else
            {
                Debug.LogWarning($"[Fısıl] '{AgentTypes.Fisil.Name}' Agent Type bulunamadı.");
            }
        }

        public bool MoveTo(Vector3 destination)
        {
            followTarget = null;
            return SetDestination(destination);
        }

        public void Follow(Transform target)
        {
            followTarget = target;
            nextRepathTime = 0f;
        }

        public void Stop()
        {
            followTarget = null;
            if (agent == null || !agent.isOnNavMesh)
                return;

            agent.isStopped = true;
            agent.ResetPath();
        }

        private void Update()
        {
            if (followTarget == null || Time.time < nextRepathTime)
                return;

            nextRepathTime = Time.time + repathInterval;
            var goal = FollowPoint.Calculate(followTarget.position, transform.position, followDistance);
            if (!agent.hasPath || (goal - agent.destination).sqrMagnitude > 0.04f)
                SetDestination(goal);
        }

        private bool SetDestination(Vector3 destination)
        {
            if (agent == null || !agent.isOnNavMesh)
                return false;

            var filter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
            if (!NavMesh.SamplePosition(destination, out var hit, sampleRadius, filter))
                return false;

            agent.isStopped = false;
            return agent.SetDestination(hit.position);
        }
    }
}
