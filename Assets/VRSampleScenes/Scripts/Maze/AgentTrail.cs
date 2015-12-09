using System.Collections;
using UnityEngine;

namespace VRStandardAssets.Maze
{
    // This script simply shows the path of the
    // NavMeshAgent in the maze scene.
    public class AgentTrail : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent m_Agent;  // Reference to the nav mesh agent who's path will be drawn.
        [SerializeField] private LineRenderer m_Line;   // Reference to the line renderer used to draw the path.


        private const float k_PathDelay = 0.3f;         // NavMeshAgents need a small delay before they are ready with a path.


        private void Update()
        {
            // If the agent still has some distance to go, return.
            if (m_Agent.remainingDistance > m_Agent.stoppingDistance)
                return;

            // Otherwise turn off the line renderer.
            m_Line.enabled = false;
        }


        public void SetDestination()
        {
            // When the destination is set, update the path that is drawn.
            StartCoroutine(UpdatePath());
        }


        private IEnumerator UpdatePath()
        {
            // Wait for the path to be ready.
            yield return new WaitForSeconds(k_PathDelay);
            while (m_Agent.pathPending)
            {
                yield return null;
            }

            // Turn the line renderer on.
            m_Line.enabled = true;
            
            // Get the path from the NavMeshAgent.
            NavMeshPath path = m_Agent.path;

            // Set the LineRenderer to have as many points as the path has corners.
            m_Line.SetVertexCount(path.corners.Length);

            // Go through all the corners and set the line's points to the corners' positions.
            for (int i = 0; i < path.corners.Length; i++)
            {
                m_Line.SetPosition(i, path.corners[i]);
            }
        }
    }
}