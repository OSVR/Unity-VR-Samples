using System.Collections;
using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Maze
{
    // This is the game controller for the maze scene.
    // It manages turning things on and off during the
    // different phases of the game.
    public class MazeGameController : MonoBehaviour
    {
        [SerializeField] private SelectionSlider m_SelectionSlider;         // This is the selection slider that must be filled to get into the play phase.
        [SerializeField] private UIFader m_InstructionsFader;               // The fader that is in control of the UI for the start of the game.
        [SerializeField] private UIFader m_WinFader;                        // The fader that is in control of the UI for when the player wins the game.
        [SerializeField] private UIFader m_LoseFader;                       // The fader that is in control of the UI for when the player loses the game.
        [SerializeField] private Player m_Player;                           // Reference to the player to determine whether the game is lost.
        [SerializeField] private ExitArea m_ExitArea;                       // Reference to the exit area to determine whether the game is won.
        [SerializeField] private MazeTargetSetting m_MazeTargetSetting;     // This needs to be turned on and off when the game is running and not.
        [SerializeField] private ParticleSystem m_WinParticles;             // The particle system that will play when the game is won.
        [SerializeField] private Reticle m_Reticle;                         // This needs to be turned on and off and it's settings altered to be appropriate for target setting.
        [SerializeField] private SwitchButton m_MazeSwitch;                 // This needs to be reset for each new game.
        [SerializeField] private Turret m_Turret;                           // This needs to be reset for each new game.
        [SerializeField] private DestinationMarker m_DestinationMarker;     // This needs to know when to be visible and not.
        [SerializeField] private AudioSource m_GameOverAudioSource;         // The audio source that plays the audio for when the player loses the game.
        [SerializeField] private VRCameraFade m_CameraFade;                 // Used for fading in and out between plays of the game.
        [SerializeField] private SelectionRadial m_SelectionRadial;         // Used for restarting the game at the end.
        [SerializeField] private CameraOrbit m_CameraOrbit;                 // This needs to be reset for each new game.
        [SerializeField] private InputWarnings m_InputWarnings;             // Different warnings need to be displayed at different points through the maze game.


        private bool m_Playing;                                             // Whether or not the game is currently playing.
        private bool m_Win;                                                 // Whether the player won.


        public bool Playing { get { return m_Playing; } }


        private void OnEnable ()
        {
            m_ExitArea.OnGameComplete += HandleGameComplete;
            m_Player.OnPlayerShot += HandlePlayerShot;
        }


        private void OnDisable ()
        {
            m_ExitArea.OnGameComplete -= HandleGameComplete;
            m_Player.OnPlayerShot -= HandlePlayerShot;
        }


        private IEnumerator Start()
        {
            // When the game is started for the first time, everything should be reset.
            Restart();

            while (true)
            {
                // Keep looping through the Start, Play and End phases, waiting for each to finish.
                yield return StartCoroutine(StartPhase());
                yield return StartCoroutine(PlayPhase());
                yield return StartCoroutine(EndPhase());
            }
        }


        private IEnumerator StartPhase ()
        {
            // Wait for the instructions to fade in.
            yield return StartCoroutine (m_InstructionsFader.InteruptAndFadeIn ());

            // Turn the reticle on and make it flat to the screen so it can be used with the selection slider.
            m_Reticle.Show();
            m_Reticle.UseNormal = false;

            // The user should hold Fire1 at this point so warn against tapping.
            m_InputWarnings.TurnOnDoubleTapWarnings ();

            // Wait for the selection bar to fill indicating the user has read the instructions.
            yield return StartCoroutine (m_SelectionSlider.WaitForBarToFill ());

            // Turn off the double tap warnings since the user will need to use double tap to move the character.
            m_InputWarnings.TurnOffDoubleTapWarnings ();

            // Wait for the instructions to fade out.
            yield return StartCoroutine (m_InstructionsFader.InteruptAndFadeOut ());
        }


        private IEnumerator PlayPhase()
        {
            // The game is now playing.
            m_Playing = true;

            // Turn on the ability to set the character's target.
            m_MazeTargetSetting.Activate();

            // Make the reticle flat to the ground.
            m_Reticle.UseNormal = true;

            // The user must use double taps to move so warn against single taps.
            m_InputWarnings.TurnOnSingleTapWarnings ();

            // While the game is playing keep coming back next frame.
            while (m_Playing)
            {
                yield return null;
            }

            // Turn tap warnings back off.
            m_InputWarnings.TurnOffSingleTapWarnings ();
        }


        private IEnumerator EndPhase()
        {
            // Turn off the ability to set targets for the character.
            m_MazeTargetSetting.Deactivate ();

            // Hide the reticle as it's not required at the moment.
            m_Reticle.Hide();

            // The reticle also controls the rotation for the selection radial so set it to use default rotation.
            m_Reticle.UseNormal = false;

            // Hide the destination marker as targets are no longer being set.
            m_DestinationMarker.Hide();

            // If the player won...
            if (m_Win)
            {
                // ... play the win particles and audio.
                m_WinParticles.Play(true);
                m_GameOverAudioSource.Play();

                // Wait for the particles to finish.
                yield return new WaitForSeconds(m_WinParticles.duration);

                // Wait for the win UI to fade in.
                yield return StartCoroutine (m_WinFader.InteruptAndFadeIn ());
            }
            else
            {
                // If the player didn't win wait for the lose UI to fade in.
                yield return StartCoroutine (m_LoseFader.InteruptAndFadeIn ());
            }

            // The user needs to hold Fire1 to pass the UI so turn on double tap warnings.
            m_InputWarnings.TurnOnDoubleTapWarnings ();

            // Wait for the radial to fill.
            yield return StartCoroutine (m_SelectionRadial.WaitForSelectionRadialToFill());

            // Turn the tap warnings back off.
            m_InputWarnings.TurnOffDoubleTapWarnings ();

            // In order wait for the win and lose UI to fade out (only one should be faded in) then wait for the camera to fade out.
            yield return StartCoroutine (m_WinFader.InteruptAndFadeOut ());
            yield return StartCoroutine (m_LoseFader.InteruptAndFadeOut ());
            yield return StartCoroutine (m_CameraFade.BeginFadeOut(true));

            // Restart all the dependent scripts.
            Restart();

            // Wait for the screen tot fade back in.
            yield return StartCoroutine(m_CameraFade.BeginFadeIn(true));
        }


        private void Restart()
        {
            // Restart everything so it's ready for the start of the game.
            m_CameraOrbit.Restart();
            m_Player.Restart();
            m_MazeSwitch.Restart();
            m_Turret.Activate();
            m_ExitArea.Restart();
            m_WinParticles.Stop(true);
            m_DestinationMarker.Restart();
        }
        

        private void HandlePlayerShot()
        {
            // If the player is shot the game is no longer playing and the player didn't win.
            m_Playing = false;
            m_Win = false;
        }


        private void HandleGameComplete()
        {
            // If the game is complete it is no longer playing and the player won.
            m_Playing = false;
            m_Win = true;
        }
    }
}