using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace VRStandardAssets.Utils
{
    // This script is used to show messages when the user
    // seems to be using the wrong type of input.
    public class InputWarnings : MonoBehaviour
    {
        [SerializeField] private float m_WarningDisplayTime = 2f;                       // How long the warning is displayed for.
        [SerializeField] private bool m_ShowDoubleTapWarnings;                          // Whether to show warnings by default when the user double taps.
        [SerializeField] private string m_DoubleTapWarningMessage = "HOLD, DON'T TAP!"; // The message to display when the user uses a double tap when they shouldn't.
        [SerializeField] private bool m_ShowSingleTapWarnings;                          // Whether to show warnings by default when the user taps.
        [SerializeField] private string m_SingleTapWarningMessage = "HOLD, DON'T TAP!"; // The message to display when the user taps when they shouldn't.
        [SerializeField] private Text m_WarningText;                                    // Reference to the Text component that will hold the messages.
        [SerializeField] private Image m_BackgroundImage;                               // Reference to the image that makes the background for the warning.
        [SerializeField] private Transform m_TextTransform;                             // Reference to the transform of the Text component, used to move the warning to the location of the click.
        [SerializeField] private Transform m_Camera;                                    // Reference to the camera's transform so the text knows which way to face.
        [SerializeField] private Reticle m_Reticle;                                     // Reference to the reticle in order to set the position of the warning.
        [SerializeField] private VRInput m_VRInput;                                     // Reference to the VRInput to detect input.


        private Coroutine m_WarningCoroutine;                                           // Reference to the coroutine that displays the warning message so it can be stopped prematurely.
        private Coroutine m_SingleClickDelayCoroutine;                                  // It isn't clear whether a click will be a double for a short time, this delays the check.
        private bool m_DisplayingWarning;                                               // Whether the warning is currently being displayed.
        private VRInput.SwipeDirection m_CurrentSwipe;                                  // The swipe being used this frame, this is used to determine whether a click is a swipe.
        private float m_DownTime;                                                       // This is used to determine whether a click is actually a hold.
        private Vector3 m_WarningPosition;                                              // The position that the warning should stick to.
        private float m_ScaleMultiplier;                                                // The warning needs to have an appropriate size based on the Reticle's scale.


        private const float k_ClickIsHoldTime = 0.5f;                                   // How long Fire1 has to be down to be considered a hold.


        private void Awake()
        {
            // Approximate the difference in scale of the reticle and text to be the same across all axes.
            m_ScaleMultiplier = m_TextTransform.localScale.x / m_Reticle.ReticleTransform.localScale.x;

            // Display nothing on the start of the scene.
            m_WarningText.text = string.Empty;
            m_BackgroundImage.enabled = false;
        }


        private void OnEnable ()
        {
            m_VRInput.OnDoubleClick += HandleDoubleClick;
            m_VRInput.OnClick += HandleClick;
            m_VRInput.OnSwipe += HandleSwipe;
            m_VRInput.OnDown += HandleDown;
        }


        private void OnDisable ()
        {
            m_VRInput.OnDoubleClick -= HandleDoubleClick;
            m_VRInput.OnClick -= HandleClick;
            m_VRInput.OnSwipe -= HandleSwipe;
            m_VRInput.OnDown -= HandleDown;
        }


        private void HandleDoubleClick ()
        {
            // If the coroutine to check whether a click is a double is running, stop it.
            if(m_SingleClickDelayCoroutine != null)
                StopCoroutine (m_SingleClickDelayCoroutine);

            // If warnings should be shown for double clicks show one with the double tap message and keep a reference to it.
            if (m_ShowDoubleTapWarnings)
                m_WarningCoroutine = StartCoroutine (DisplayWarning (m_DoubleTapWarningMessage));
        }


        private void HandleClick ()
        {
            // If the difference in time between now and the time when the button went down is greater than the constant, it's a 'hold' so return.
            if (Time.time - m_DownTime >= k_ClickIsHoldTime)
                return;

            // If warning should be shown for single clicks and there is no current swipe direction, start a coroutine to check it's not a double click.
            if (m_ShowSingleTapWarnings && m_CurrentSwipe == VRInput.SwipeDirection.NONE)
                m_SingleClickDelayCoroutine = StartCoroutine(SingleClickCheckDelay ());
        }


        private void HandleSwipe (VRInput.SwipeDirection swipe)
        {
            // Store the swipe this frame.
            m_CurrentSwipe = swipe;
        }


        private void HandleDown ()
        {
            // Store the time when the button is pressed.
            m_DownTime = Time.time;
        }


        private IEnumerator SingleClickCheckDelay ()
        {
            // Wait for the time before it can be a double click.
            yield return new WaitForSeconds (m_VRInput.DoubleClickTime);

            // If this coroutine hasn't been stopped by another HandleClick function then display the single tap warning message.
            m_WarningCoroutine = StartCoroutine (DisplayWarning (m_SingleTapWarningMessage));
        }


        private IEnumerator DisplayWarning (string message)
        {
            // If a warning is already being displayed, quit.
            if(m_DisplayingWarning)
                yield break;

            // Otherwise we are now displaying a warning.
            m_DisplayingWarning = true;

            // Display the message.
            m_WarningText.text = message;
            m_BackgroundImage.enabled = true;

            // Set the position of the warning to that of the Reticle.
            m_TextTransform.position = m_Reticle.ReticleTransform.position;

            // Set the rotation of the warning to facing the camera but oriented so it's up is along the global y axis.
            m_TextTransform.rotation = Quaternion.LookRotation (m_Camera.forward);
            
            // Set the scale of the warning based on the scale of the Reticle but taking the difference in scale into account.
            m_TextTransform.localScale = m_Reticle.ReticleTransform.localScale * m_ScaleMultiplier;
            
            // Wait until the time is up.
            yield return new WaitForSeconds (m_WarningDisplayTime);

            // Display nothing.
            m_WarningText.text = string.Empty;
            m_BackgroundImage.enabled = false;

            // A warning is no longer being displayed.
            m_DisplayingWarning = false;
        }


        public void TurnOnDoubleTapWarnings ()
        {
            // If double tap warnings are already being shown nothing needs to be done so return.
            if (m_ShowDoubleTapWarnings)
                return;

            // Double tap warnings can now be shown.
            m_ShowDoubleTapWarnings = true;

            // If there is currently a single tap warning being shown stop it.
            if(m_WarningCoroutine != null)
                StopCoroutine (m_WarningCoroutine);

            // Set the component to display nothing.
            m_WarningText.text = string.Empty;
        }


        public void TurnOffDoubleTapWarnings ()
        {
            // No longer show double tap warnings.
            m_ShowDoubleTapWarnings = false;
        }


        public void TurnOnSingleTapWarnings ()
        {
            // If single taps warnings are already being shown nothing needs to be done so return.
            if (m_ShowSingleTapWarnings)
                return;

            // Single tap warnings can now be shown.
            m_ShowSingleTapWarnings = true;

            // If there is a double tap warning currently being shown stop it.
            if (m_WarningCoroutine != null)
                StopCoroutine(m_WarningCoroutine);

            // Set the component to display nothing.
            m_WarningText.text = string.Empty;
        }


        public void TurnOffSingleTapWarnings ()
        {
            // No longer show single tap warnings.
            m_ShowSingleTapWarnings = false;
        }
    }
}