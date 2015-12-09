using UnityEngine;
using System.Collections;

namespace VRStandardAssets.Flyer
{
    // This script approximates which ring
    // the flyer is in line with and lets it
    // know so it can change colour.
    public class FlyerAlignmentChecker : MonoBehaviour
    {
        [SerializeField] private float m_Radius = 10f;  // The radius of the sphere cast.


        private Ring m_CurrentRing;                     // The ring that the flyer is currently or was most recently aligned with.
        private bool m_IsGameRunning;                   // Whether the game is currently running.


        public void StartGame ()
        {
            // At the start of the game, it is running.
            m_IsGameRunning = true;

            // Start checking for aligned rings.
            StartCoroutine (CheckAlignment ());
        }


        public void StopGame ()
        {
            // At the end of the game it is no longer running.
            m_IsGameRunning = false;
        }



        private IEnumerator CheckAlignment()
        {
            // Continue looping until the game is no longer running.
            while (m_IsGameRunning)
            {
                // If there is a current ring, set it to be unaligned by default.
                if (m_CurrentRing)
                    m_CurrentRing.ShipAligned = false;

                // Create a ray forward from the flyer's current position.
                Ray ray = new Ray (transform.position, Vector3.forward);
                RaycastHit hit;

                // Spherecast along the ray.
                if (Physics.SphereCast (ray, m_Radius, out hit))
                {
                    // Try to find a ring on the hit object.
                    Ring ring = hit.transform.GetComponent<Ring> ();

                    // If it is a ring...
                    if (ring)
                    {
                        // ...  set it as the current ring and the flyer is aligned with it.
                        m_CurrentRing = ring;
                        m_CurrentRing.ShipAligned = true;
                    }
                }

                // Wait until next frame.
                yield return null;
            }
        }
    }
}