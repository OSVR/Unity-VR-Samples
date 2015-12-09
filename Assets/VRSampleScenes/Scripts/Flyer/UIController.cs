using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Common;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Flyer
{
    // The script controls the UI for the flyer scene.
    public class UIController : MonoBehaviour
    {
        [SerializeField] private UIFader m_IntroUI;     // Reference to the script that controls the fading of the intro UI.
        [SerializeField] private UIFader m_OutroUI;     // Reference to the script that controls the fading of the outro UI.
        [SerializeField] private Text m_TotalScore;     // The text component used to display the score for this session.
        [SerializeField] private Text m_HighScore;      // The text component used to display the high score.


        public IEnumerator ShowIntroUI()
        {
            // Interupt any fading the intro UI is already doing and fade in, return when finished.
            yield return StartCoroutine(m_IntroUI.InteruptAndFadeIn());
        }


        public IEnumerator HideIntroUI()
        {
            // Interupt any fading the outro UI is already doing and fade out, return when finished.
            yield return StartCoroutine(m_IntroUI.InteruptAndFadeOut());
        }


        public IEnumerator ShowOutroUI()
        {
            // Set the text to show the various scores.
            m_TotalScore.text = SessionData.Score.ToString();
            m_HighScore.text = SessionData.HighScore.ToString();

            // Wait for the outro to fade in.
            yield return StartCoroutine(m_OutroUI.InteruptAndFadeIn());
        }


        public IEnumerator HideOutroUI()
        {
            // Wait for the outro to fade out.
            yield return StartCoroutine(m_OutroUI.InteruptAndFadeOut());
        }
    }
}