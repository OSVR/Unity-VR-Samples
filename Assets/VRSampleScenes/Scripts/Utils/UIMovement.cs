using UnityEngine;

namespace VRStandardAssets.Utils
{
    // This class is used to move UI elements in ways that are
    // generally useful when using VR, specifically looking at
    // the camera and rotating so they're always in front of
    // the camera.
    public class UIMovement : MonoBehaviour
    {
        [SerializeField] private bool m_LookatCamera = true;    // Whether the UI element should rotate to face the camera.
        [SerializeField] private Transform m_UIElement;         // The transform of the UI to be affected.
        [SerializeField] private Transform m_Camera;            // The transform of the camera.
        [SerializeField] private bool m_RotateWithCamera;       // Whether the UI should rotate with the camera so it is always in front.
        [SerializeField] private float m_FollowSpeed = 10f;     // The speed with which the UI should follow the camera.


        private float m_DistanceFromCamera;                     // The distance the UI should stay from the camera when rotating with it.


        private void Start ()
        {
            // Find the distance from the UI to the camera so the UI can remain at that distance.
            m_DistanceFromCamera = Vector3.Distance (m_UIElement.position, m_Camera.position);
        }


        private void Update()
        {
            // If the UI should look at the camera set it's rotation to point from the UI to the camera.
            if(m_LookatCamera)
                m_UIElement.rotation = Quaternion.LookRotation(m_UIElement.position - m_Camera.position);

            // If the UI should rotate with the camera...
            if (m_RotateWithCamera)
            {
                // Find the direction the camera is looking but on a flat plane.
                Vector3 targetDirection = Vector3.ProjectOnPlane (m_Camera.forward, Vector3.up).normalized;

                // Calculate a target position from the camera in the direction at the same distance from the camera as it was at Start.
                Vector3 targetPosition = m_Camera.position + targetDirection * m_DistanceFromCamera;

                // Set the target position  to be an interpolation of itself and the UI's position.
                targetPosition = Vector3.Lerp(m_UIElement.position, targetPosition, m_FollowSpeed * Time.deltaTime);

                // Since the UI is only following on the XZ plane, negate any y movement.
                targetPosition.y = m_UIElement.position.y;

                // Set the UI's position to the calculated target position.
                m_UIElement.position = targetPosition;
            }
        }
    }
}