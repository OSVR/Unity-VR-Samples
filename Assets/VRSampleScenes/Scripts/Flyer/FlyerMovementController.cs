using UnityEngine;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;
using UnityEngine.VR;
using VRStandardAssets.Common;

namespace VRStandardAssets.Flyer
{
    // This class handles the movement of the flyer ship.
    public class FlyerMovementController : MonoBehaviour
    {
        [SerializeField] private float m_DistanceFromCamera = 75f;  // The distance from the camera the ship aims to be.
        [SerializeField] private float m_Speed = 100f;              // The speed the ship moves forward.
        [SerializeField] private float m_Damping = 0.5f;            // The amount of damping applied to the movement of the ship.
        [SerializeField] private Transform m_Flyer;                 // Reference to the transform of the flyer.
        [SerializeField] private Transform m_TargetMarker;          // The transform the flyer is moving towards.
        [SerializeField] private Transform m_Camera;                // Reference to the camera's transform.
        [SerializeField] private Transform m_CameraContainer;       // Reference to the transform containing the camera.
        [SerializeField] private Text m_CurrentScore;               // Reference to the Text component that will display the user's score.


        private bool m_IsGameRunning;                               // Whether the game is running.
        private Vector3 m_FlyerStartPos;                            // These positions and rotations are stored at Start so the flyer can be reset each game.
        private Quaternion m_FlyerStartRot;
        private Vector3 m_TargetMarkerStartPos;
        private Quaternion m_TargetMarkerStartRot;
        private Vector3 m_CameraContainerStartPos;


        private const float k_ExpDampingCoef = -20f;                // The coefficient used to damp the movement of the flyer.
        private const float k_BankingCoef = 3f;                     // How much the ship banks when it moves.


        private void Start ()
        {
            // Store all the starting positions and rotations.
            m_FlyerStartPos = m_Flyer.position;
            m_FlyerStartRot = m_Flyer.rotation;
            m_TargetMarkerStartPos = m_TargetMarker.position;
            m_TargetMarkerStartRot = m_TargetMarker.rotation;
            m_CameraContainerStartPos = m_CameraContainer.position;
        }


        public void StartGame ()
        {
            // The game is now running.
            m_IsGameRunning = true;

            // Start the flyer moving.
            StartCoroutine (MoveFlyer ());
        }


        public void StopGame ()
        {
            // The game is no longer running.
            m_IsGameRunning = false;

            // Reset all the positions and rotations that were store.
            m_Flyer.position = m_FlyerStartPos;
            m_Flyer.rotation = m_FlyerStartRot;
            m_TargetMarker.position = m_TargetMarkerStartPos;
            m_TargetMarker.rotation = m_TargetMarkerStartRot;
            m_CameraContainer.position = m_CameraContainerStartPos;
        }


        private IEnumerator MoveFlyer ()
        {
            while (m_IsGameRunning)
            {
                // Set the target marker position to a point forward of the camera multiplied by the distance from the camera.
                Quaternion headRotation = InputTracking.GetLocalRotation (VRNode.Head);
                m_TargetMarker.position = m_Camera.position + (headRotation * Vector3.forward) * m_DistanceFromCamera;

                // Move the camera container forward.
                m_CameraContainer.Translate (Vector3.forward * Time.deltaTime * m_Speed);

                // Move the flyer towards the target marker.
                m_Flyer.position = Vector3.Lerp(m_Flyer.position, m_TargetMarker.position,
                    m_Damping * (1f - Mathf.Exp (k_ExpDampingCoef * Time.deltaTime)));

                // Calculate the vector from the target marker to the flyer.
                Vector3 dist = m_Flyer.position - m_TargetMarker.position;

                // Base the target markers pitch (x rotation) on the distance in the y axis and it's roll (z rotation) on the distance in the x axis.
                m_TargetMarker.eulerAngles = new Vector3 (dist.y, 0f, dist.x) * k_BankingCoef;

                // Make the flyer bank towards the marker.
                m_Flyer.rotation = Quaternion.Lerp(m_Flyer.rotation, m_TargetMarker.rotation,
                    m_Damping * (1f - Mathf.Exp (k_ExpDampingCoef * Time.deltaTime)));

                // Update the score text.
                m_CurrentScore.text = "Score: " + SessionData.Score;

                // Wait until next frame.
                yield return null;
            }
        }
    }
}