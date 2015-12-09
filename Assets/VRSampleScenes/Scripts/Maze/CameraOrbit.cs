using System.Collections;
using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Maze
{
    // In the maze scene, the camera rotates around
    // the maze in response to the user swiping.  This
    // class handles how the camera moves in response
    // to the swipe.  This script is placed on a parent
    // of the camera such that the camera pivots around
    // as this gameobject is rotated.
    public class CameraOrbit : MonoBehaviour
    {
        // This enum represents the way in which the camera will rotate around the maze.
        public enum OrbitStyle
        {
            Smooth, Step, StepWithFade,
        }


        [SerializeField] private OrbitStyle m_OrbitStyle;
        [SerializeField] private float m_RotationIncrement = 45f;           // The amount the camera rotates in response to a swipe.
        [SerializeField] private float m_RotationFadeDuration = 0.2f;       // If fading is enabled, this is the duration of the the fade.
        [SerializeField] private VRCameraFade m_CameraFade;                 // Optional reference to the camera fade script, only required if fading is enabled.
        [SerializeField] private VRInput m_VrInput;                         // Reference to the VRInput to subscribe to swipe events.
        [SerializeField] private MazeGameController m_MazeGameController;   // Reference to the game controller so swiping will only be handled while the game is playing.
        [SerializeField] private Rigidbody m_Rigidbody;                     // Reference to the camera's rigidbody.


        private Quaternion m_StartRotation;                                 // The rotation of the camera at the start of the scene, used for reseting.


        private void Awake ()
        {
            // Store the start rotation.
            m_StartRotation = m_Rigidbody.rotation;
        }


        private void OnEnable ()
        {
            m_VrInput.OnSwipe += HandleSwipe;
        }


        private void OnDisable ()
        {
            m_VrInput.OnSwipe -= HandleSwipe;
        }

      
        private void HandleSwipe(VRInput.SwipeDirection swipeDirection)
        {
            // If the game isn't playing or the camera is fading, return and don't handle the swipe.
            if (!m_MazeGameController.Playing)
                return;

            if (m_CameraFade.IsFading)
                return;

            // Otherwise start rotating the camera with either a positive or negative increment.
            switch (swipeDirection)
            {
                case VRInput.SwipeDirection.LEFT:
                    StartCoroutine(RotateCamera(m_RotationIncrement));
                    break;

                case VRInput.SwipeDirection.RIGHT:
                    StartCoroutine(RotateCamera(-m_RotationIncrement));
                    break;
            }
        }

       
        private IEnumerator RotateCamera(float increment)
        {
            // Determine how the camera should rotate base on it's orbit style.
            switch (m_OrbitStyle)
            {
                // If the style is smooth add a torque to the camera's rigidbody.
                case OrbitStyle.Smooth:
                    m_Rigidbody.AddTorque (transform.up * increment);
                    break;
                
                // If the style is step then rotate the camera's transform by a set amount.
                case OrbitStyle.Step:
                    transform.Rotate(0, increment, 0);
                    break;

                // If the style is step with a fade, wait for the camera to fade out, then step the rotation around, the wait for the camera to fade in.
                case OrbitStyle.StepWithFade:
                    yield return StartCoroutine(m_CameraFade.BeginFadeOut(m_RotationFadeDuration, false));
                    transform.Rotate(0, increment, 0);
                    yield return StartCoroutine(m_CameraFade.BeginFadeIn(m_RotationFadeDuration, false));
                    break;
            }
        }


        public void Restart ()
        {
            // To restart, make sure the rotation is reset and the camera is not moving or rotating.
            m_Rigidbody.rotation = m_StartRotation;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.velocity = Vector3.zero;
        }
    }
}