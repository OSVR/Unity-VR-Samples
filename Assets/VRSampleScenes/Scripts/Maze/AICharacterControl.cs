using UnityEngine;
using UnityEngine.AI;
using VRStandardAssets.Maze;

namespace VRStandardAssets.Maze
{
    [RequireComponent(typeof(UnityEngine.AI.NavMeshAgent))]
    [RequireComponent(typeof(ThirdPersonCharacter))]
    public class AICharacterControl : MonoBehaviour
    {
        public UnityEngine.AI.NavMeshAgent agent { get; private set; }             // Navmesh agent required for the path finding
        public ThirdPersonCharacter character { get; private set; } // Character we are controlling


        private Rigidbody m_Rigidbody;
        private Player m_Player;
        private Vector3 m_TargetPosition;


        private void Start()
        {
            // get the components on the object we need ( should not be null due to require component so no need to check )
            agent = GetComponentInChildren<NavMeshAgent>();
            character = GetComponent<ThirdPersonCharacter>();
            m_Rigidbody = GetComponent<Rigidbody>();
            m_Player = GetComponent<Player>();

            agent.updateRotation = false;
            agent.updatePosition = true;

            m_TargetPosition = transform.position;
        }


        private void Update()
        {
            agent.SetDestination(m_TargetPosition);

            if (agent.remainingDistance > agent.stoppingDistance)
            {
                character.Move(agent.desiredVelocity, false, false);
                m_Rigidbody.isKinematic = false;
            }
            else
            {
                character.Move(Vector3.zero, false, false);
                m_Rigidbody.isKinematic = true;
            }

            if (m_Player.Dead)
                m_Rigidbody.isKinematic = false;
        }


        public void SetTarget(Vector3 targetPosition)
        {
            m_TargetPosition = targetPosition;
        }
    }
}
