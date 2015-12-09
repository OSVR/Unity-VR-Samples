using UnityEngine;
using UnityEngine.UI;

namespace VRStandardAssets.Utils
{
    // This is a simple script to tint UI Images a colour when
    // a VRInteractiveItem is looked at.  Note that the Images
    // to tint don't have to be on the same gameobject as the
    // VRInteractiveItem but the user will be looking at the
    // VRInteractiveItem when the Images are tinted.
    // Also note that this should be used to tint/highlight
    // images, not change their colour entirely.
    public class UITint : MonoBehaviour
    {
        [SerializeField] private Color m_Tint;                                  // The colour to tint the Images.
        [Range (0f, 1f)] [SerializeField] private float m_TintPercent = 0.5f;   // How much the colour should be used.
        [SerializeField] private Image[] m_ImagesToTint;                        // References to the images which will be tinted.
        [SerializeField] private VRInteractiveItem m_InteractiveItem;           // Reference to the VRInteractiveItem which must be looked at to tint the images.


        private void OnEnable ()
        {
            m_InteractiveItem.OnOver += HandleOver;
            m_InteractiveItem.OnOut += HandleOut;
        }


        private void OnDisable ()
        {
            m_InteractiveItem.OnOver -= HandleOver;
            m_InteractiveItem.OnOut -= HandleOut;
        }


        private void HandleOver ()
        {
            // When the user looks at the VRInteractiveItem go through all the images...
            for (int i = 0; i < m_ImagesToTint.Length; i++)
            {
                // and ADD to their colour by an amount based on the tint percentage.  Note this will push the colour closer to white.
                m_ImagesToTint[i].color += m_Tint * m_TintPercent;
            }
        }


        private void HandleOut ()
        {
            // When the user looks away from the VRInteractiveItem go through all the images...
            for (int i = 0; i < m_ImagesToTint.Length; i++)
            {
                // ...and subtract the same amount.
                m_ImagesToTint[i].color -= m_Tint * m_TintPercent;
            }
        }
    }
}