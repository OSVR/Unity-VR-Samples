using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Flyer
{
    // This script controls the flow of the flyer
    // game and how all the other controllers work
    // together.
    public class FlyerGameController : MonoBehaviour
    {
        [SerializeField] private int m_GameDuration = 30;                                   // The duration of the game
        [SerializeField] private float m_IntroOutroFadeDuration = 0.5f;                     // The duration of the fade before and after the intro
        [SerializeField] private SelectionSlider m_SelectionSlider;                         // The slider needed to start the game.
        [SerializeField] private Reticle m_Reticle;                                         // The reticle so it can be turned on to aim at the selection slider.
        [SerializeField] private FlyerHealthController m_HealthController;                  // The flyer's health so it can be reset.
        [SerializeField] private FlyerAlignmentChecker m_AlignmentChecker;                  // The script to check ring alignments, it should only be on whilst the game runs.
        [SerializeField] private FlyerMovementController m_FlyerMovementController;         // The script controlling the movement of the flyer.
        [SerializeField] private EnvironmentController m_EnvironmentController;             // This needs to know when to start and stop spawning the environment.
        [SerializeField] private UIController m_UIController;                               // This needs to know when specific pieces of UI should be shown.
        [SerializeField] private GUIArrows m_GuiArrows;                                     // The GUI Arrows shown at the start.
        [SerializeField] private Image m_TimerBar;                                          // Timer slider to indicate time remaining.
        [SerializeField] private InputWarnings m_InputWarnings;                             // This needs to know when to show different warnings.
        [SerializeField] private VRCameraFade m_CameraFade;                                 // This is used to fade out and back in again as the game starts.
        [SerializeField] private SelectionRadial m_SelectionRadial;                         // Used to restart the game.
        

        private float m_EndTime;                                                            // The time at the point the game should end.
        private float m_TimeRemaining;                                                      // The time until the game should end.
        private bool m_IsGameRunning;                                                       // Whether the game is currently running.


        public bool IsGameRunning { get { return m_IsGameRunning; } }


        private IEnumerator Start()
        {
            while (true)
            {
                yield return StartCoroutine (StartPhase ());
                yield return StartCoroutine (PlayPhase ());
                yield return StartCoroutine (EndPhase ());
            }
        }


        private IEnumerator StartPhase ()
        {
            // Make sure the Outro UI is not being shown and the intro UI is.
            StartCoroutine(m_UIController.ShowIntroUI());
            StartCoroutine(m_UIController.HideOutroUI ());

            // To make sure the user is facing the right way show the arrows.
            m_GuiArrows.Show ();

            // Turn off the fog whilst showing the intro.
            RenderSettings.fog = false;

            // Make sure the game is stopped for the flyer's health.
            m_HealthController.StopGame ();

            // Since a selection slider is being used, hide the radial and show the reticle.
            m_SelectionRadial.Hide ();
            m_Reticle.Show ();

            // The user should hold Fire1 for the selection slider so turn on warnings for tapping.
            m_InputWarnings.TurnOnDoubleTapWarnings ();
            m_InputWarnings.TurnOnSingleTapWarnings ();

            // In order wait for the selection slider to fill, then the intro UI to hide, then the camera to fade out.
            yield return StartCoroutine (m_SelectionSlider.WaitForBarToFill ());
            yield return StartCoroutine (m_UIController.HideIntroUI ());
            yield return StartCoroutine(m_CameraFade.BeginFadeOut(m_IntroOutroFadeDuration, false));

            // Once the screen has faded out, we can hide UI elements we don't want to see anymore like the arrows and reticle.
            m_GuiArrows.Hide ();
            m_Reticle.Hide ();
            
            // Turn the fog back on so spawned objects won't appear suddenly.
            RenderSettings.fog = true;

            // The user now needs to tap to fire so turn off the warnings.
            m_InputWarnings.TurnOffDoubleTapWarnings ();
            m_InputWarnings.TurnOffSingleTapWarnings ();

            // Now wait for the screen to fade back in.
            yield return StartCoroutine(m_CameraFade.BeginFadeIn(m_IntroOutroFadeDuration, false));
        }


        private IEnumerator PlayPhase ()
        {
            // The game is now running.
            m_IsGameRunning = true;

            // Start the various controllers.
            m_AlignmentChecker.StartGame ();
            m_HealthController.StartGame ();
            m_FlyerMovementController.StartGame ();
            m_EnvironmentController.StartEnvironment();

            // The end of the game is the current time + the length of the game.
            m_EndTime = Time.time + m_GameDuration;

            // Each frame while the flyer is alive and there is time remaining...
            do
            {
                // Calculate the time remaining set the timer bar to fill by the normalised time remaining.
                m_TimeRemaining = m_EndTime - Time.time;
                m_TimerBar.fillAmount = m_TimeRemaining / m_GameDuration;

                // Wait until the next frame.
                yield return null;
            }
            while (m_TimeRemaining > 0f && !m_HealthController.IsDead);
            
            // Upon reaching this point either the time has run out or the flyer is dead, either way the game is no longer running.
            m_IsGameRunning = false;
        }


        private IEnumerator EndPhase ()
        {
            // Wait for the camera to fade out.
            yield return StartCoroutine(m_CameraFade.BeginFadeOut(m_IntroOutroFadeDuration, false));

            // Show the required UI like the arrows and the radial.
            m_GuiArrows.Show ();
            m_SelectionRadial.Show ();

            // Turn off the fog.
            RenderSettings.fog = false;

            // Show the outro UI.
            StartCoroutine(m_UIController.ShowOutroUI());

            // Stop the various controllers.
            m_AlignmentChecker.StopGame();
            m_HealthController.StopGame();
            m_FlyerMovementController.StopGame();
            m_EnvironmentController.StopEnvironment();

            // The user needs to fill the radial to continue so turn the tap warnings back on.
            m_InputWarnings.TurnOnDoubleTapWarnings();
            m_InputWarnings.TurnOnSingleTapWarnings();

            // In order, wait for the screen to fade in, then wait for the user to fill the radial.
            yield return StartCoroutine(m_CameraFade.BeginFadeIn(m_IntroOutroFadeDuration, false));
            yield return StartCoroutine(m_SelectionRadial.WaitForSelectionRadialToFill());

            // Turn the warnings off now the user has filled the radial.
            m_InputWarnings.TurnOffDoubleTapWarnings();
            m_InputWarnings.TurnOffSingleTapWarnings();

            // Hide the arrows.
            m_GuiArrows.Hide();
            
            // Wait for the outro UI to hide.
            yield return StartCoroutine(m_UIController.HideOutroUI());

            // Turn the fog back on.
            RenderSettings.fog = true;
        }
    }
}