using UnityEngine;
using UnityEngine.VR;

namespace VRStandardAssets.Examples
{
    // This script shows how gameobjects can react to
    // the rotation of the user's head.  It tilts the
    // gameobject's transform so it's front edge is 
    // perpendicular to the user's line of sight.
    public class ExampleRotation : MonoBehaviour
    {
        [SerializeField] private float m_Damping = 0.2f;        // Used to smooth the rotation of the transform.
        [SerializeField] private float m_MaxYRotation = 20f;    // The maximum amount the transform can rotate around the y axis.
        [SerializeField] private float m_MinYRotation = -20f;   // The maximum amount the transform can rotate around the y axis in the opposite direction.


        private const float k_ExpDampCoef = -20f;               // Coefficient used to damp the rotation.


        private void Update()
        {
            // Store the Euler rotation of the gameobject.
            var eulerRotation = transform.rotation.eulerAngles;

            // Set the rotation to be the same as the user's in the y axis.
            eulerRotation.x = 0;
            eulerRotation.z = 0;
            eulerRotation.y = InputTracking.GetLocalRotation(VRNode.Head).eulerAngles.y;

            // Add 360 to the rotation so that it can effectively be clamped.
            if (eulerRotation.y < 270)
                eulerRotation.y += 360;

            // Clamp the rotation between the minimum and maximum.
            eulerRotation.y = Mathf.Clamp(eulerRotation.y, 360 + m_MinYRotation, 360 + m_MaxYRotation);

            // Smoothly damp the rotation towards the newly calculated rotation.
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(eulerRotation),
                m_Damping * (1 - Mathf.Exp(k_ExpDampCoef * Time.deltaTime)));
        }
    }
}