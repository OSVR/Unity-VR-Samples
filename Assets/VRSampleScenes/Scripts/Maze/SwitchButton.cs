using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Maze
{
    // This class is used to control the power to the
    // turret in the maze scene.  It uses a SelectionSlider
    // to turn off the power which is only activated when
    // the player is in front of the switch itself.
    public class SwitchButton : MonoBehaviour
    {
        [SerializeField] private AudioSource m_SwitchAudio;             // The audio source that plays the clips for when the UI appears and is filled.
        [SerializeField] private AudioClip m_SwitchPressClip;           // The audio clip for when the selection slider is filled.
        [SerializeField] private AudioClip m_ActivateGUIClip;           // The audio for when the selection slider first appears.
        [SerializeField] private SelectionSlider m_SelectionSlider;     // The selection slider that needs to be filled for the power to be turned off.
        [SerializeField] private UIFader m_SelectionSliderFader;        // The fader that controls the appearance and disappearance of the slider.
        [SerializeField] private Transform m_Character;                 // Used to check it is the player that has entered the trigger zone.
        [SerializeField] private Turret m_Turret;                       // Reference to the turret that will be turned off by the switch.


        private bool m_PowerOn;                                         // Whether or not power is going to the turret.


        private void OnEnable ()
        {
            m_SelectionSlider.OnBarFilled += HandleBarFilled;
        }


        private void OnDisable ()
        {
            m_SelectionSlider.OnBarFilled -= HandleBarFilled;
        }


        private void OnTriggerEnter (Collider other)
        {
            // If the triggering transform is not the character or the power is already off, return.
            if (other.transform != m_Character)
                return;

            if(!m_PowerOn)
                return;

            // Play the audio for when the UI gets turned on.
            m_SwitchAudio.clip = m_ActivateGUIClip;
            m_SwitchAudio.Play();

            // Fade in the selection slider.
            StartCoroutine(m_SelectionSliderFader.InteruptAndFadeIn());
        }


        private void OnTriggerExit (Collider other)
        {
            // If the triggering transform is not the character or the power is already off, return.
            if (other.transform != m_Character)
                return;

            if (!m_PowerOn)
                return;

            // Fade out the selection slider.
            StartCoroutine(m_SelectionSliderFader.CheckAndFadeOut());
        }


        public void Restart ()
        {
            // When the game restarts the power is on.
            m_PowerOn = true;

            // The selection slider should be invisible.
            m_SelectionSliderFader.SetInvisible ();
        }


        private void HandleBarFilled ()
        {
            // When the selection slider is filled, the power should be turned off.
            m_PowerOn = false;

            // Play the audio for the power being turned off.
            m_SwitchAudio.clip = m_SwitchPressClip;
            m_SwitchAudio.Play();

            // Deactivate the turret.
            m_Turret.Deactivate();

            // Fade out the selection slider.
            StartCoroutine (m_SelectionSliderFader.CheckAndFadeOut ());
        }
    }
}