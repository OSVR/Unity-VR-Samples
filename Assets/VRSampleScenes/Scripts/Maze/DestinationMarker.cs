using UnityEngine;

namespace VRStandardAssets.Maze
{
    // As a visual aid to where the character in the maze scene
    // is headed, this class shows a small pin on the ground at
    // the character's destination.
    public class DestinationMarker : MonoBehaviour
    {
        [SerializeField] private MazeTargetSetting m_MazeTargetSetting;                                 // The script that tells the NavMeshAgent where to go.
        [SerializeField] private AudioSource m_MarkerMoveAudio;                                         // The audio source that will play when the marker is played.
        [SerializeField] private Renderer m_Renderer;                                                   // The renderer needs to be turned on and off when the marker is needed or not.
		[SerializeField] private Animator m_Animator;                                                   // The marker has a pop-up animation that needs to play.


        private readonly int m_HashMazeNavMarkerAnimState = Animator.StringToHash("MazeNavMarkerAnim"); // Used to reference the state to be played.


        private void Awake()
        {
            // There's no destination at the start so hide the marker.
            Hide();
        }


        private void OnEnable ()
        {
            m_MazeTargetSetting.OnTargetSet += HandleTargetSet;
        }


        private void OnDisable ()
        {
            m_MazeTargetSetting.OnTargetSet -= HandleTargetSet;
        }


        public void Hide()
        {
            m_Renderer.enabled = false;
        }


        private void Show()
        {
            m_Renderer.enabled = true;
        }


        public void Restart()
        {
            // This is called when the game restarts so the marker needs to be hidden again.
            Hide();
        }


        private void HandleTargetSet(Transform target)
        {
            // When the target is set show the marker.
            Show();

            // Set the marker's position to the target position.
            transform.position = target.position;

            // Play the audio.
            m_MarkerMoveAudio.Play();

            // Play the animation on whichever layer it is on, with no time offset.
            m_Animator.Play(m_HashMazeNavMarkerAnimState, -1, 0.0f);
        }
    }
}