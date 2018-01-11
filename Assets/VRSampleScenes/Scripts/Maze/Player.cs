using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace VRStandardAssets.Maze
{
    // The other scripts on the player character in the maze
    // scene are from Unity's Standard Assets.  This script is
    // designed to work with them to control the specifics of
    // the character for the maze scene such as the player being
    // shot and celebrating.
    public class Player : MonoBehaviour
    {
        public event Action OnPlayerShot;                                               // This event is triggered when the player is shot.


        [SerializeField] private MazeTargetSetting m_MazeTargetSetting;                 // This triggers an event when the target is set. 
        [SerializeField] private AgentTrail m_AgentTrail;                               // This needs to know when a target has been set.
        [SerializeField] private AudioSource m_PlayerAudio;                             // The audio source that plays the sounds of the player.
        [SerializeField] private AudioClip m_PlayerHitClip;                             // The sound to play as the player is shot.
        [SerializeField] private AudioClip m_PlayerDieClip;                             // The sound to play when the player dies.
        
        
        private NavMeshAgent m_Agent;                                                   // Needs to be used to reset the character's position and stop the player.
        private AICharacterControl m_AiCharacter;                                       // Used to actually set the destination of the player.
        private Animator m_Animator;                                                    // Used to trigger various states playing.
        private Collider m_Collider;                                                    // Turned off when the player dies.
        private Rigidbody m_RigidBody;                                                  // Used to stop the character when the game is complete.
        private bool m_IsDying;                                                         // Whether the player is dying.
        private bool m_IsGameOver;                                                      // Whether the game is complete.
        private Vector3 m_OriginPosition;                                               // The position the player should move to at the start.


        private readonly int m_HashResetPara = Animator.StringToHash ("Reset");         // Used to trigger a reset of the player's animation.
        private readonly int m_HashDyingPara = Animator.StringToHash ("Dying");         // Used to trigger the animation of the player dying when shot.
        private readonly int m_HashWinningPara = Animator.StringToHash("Winning");      // Used to trigger the animation of the player celebrating when winning.


        public bool Dead { get { return m_IsDying; } }


        private void Awake()
        {
            // Setup references.
            m_RigidBody = GetComponent<Rigidbody>();
            m_Collider = GetComponent<Collider>();
            m_AiCharacter = GetComponent<AICharacterControl>();
            m_Agent = GetComponent<NavMeshAgent>();
            m_Animator = GetComponent<Animator>();

            // Set the position that the player will be reset to.
            m_OriginPosition = transform.position;
        }


        private void OnEnable ()
        {
            m_MazeTargetSetting.OnTargetSet += HandleSetTarget;
        }


        private void OnDisable()
        {
            m_MazeTargetSetting.OnTargetSet -= HandleSetTarget;
        }


        public void Restart()
        {
            // When the game starts the character should be able to move and the collider should be on but the player is not dead.
            m_AiCharacter.enabled = true;
            m_Collider.enabled = true;
            m_IsDying = false;

            // Move the character back to the start position.
            m_Agent.Warp(m_OriginPosition);

            // Make sure the character is already at the position that is trying to be reached.
            m_AiCharacter.SetTarget(transform.position);

            // Reset the animator.
            m_Animator.SetTrigger(m_HashResetPara);
            
            // The game is not over.
            m_IsGameOver = false;
        }


        public void GameComplete()
        {
            // To make sure this can only be called once, check if the game is already over.
            if (m_IsGameOver)
                return;

            // The game is now over.
            m_IsGameOver = true;

            // Set the character's rigidbody to stop it from moving.
            m_RigidBody.isKinematic = true;

            // Play the winning animation.
            m_Animator.SetTrigger(m_HashWinningPara);
        }


        public void TurretHit()
        {
            // To make sure this is only called once, check if the player is already dying.
            if (m_IsDying)
                return;

            // The player is now dying.
            m_IsDying = true;

            // Start the player dying.
            StartCoroutine(DyingSequence());
        }


        private IEnumerator DyingSequence()
        {
            // Wait a frame to avoid any conflicts with other scripts.
            yield return null;

            // Turn off the character control and collider.
            m_AiCharacter.enabled = false;
            m_Collider.enabled = false;

            // Start the dying animation playing.
            m_Animator.SetTrigger(m_HashDyingPara);

            // Stop the NavMeshAgent from moving the character.
            m_Agent.isStopped = true;

            // In order play the clips of the player being hit and then dying.
            yield return StartCoroutine (PlayClipAndWait (m_PlayerHitClip));
            yield return StartCoroutine (PlayClipAndWait (m_PlayerDieClip));

            // If there are any subscribers to OnPlayerShot, call it.
            if (OnPlayerShot != null)
                OnPlayerShot();
        }


        private IEnumerator PlayClipAndWait (AudioClip clip)
        {
            // Set the audio to be the given clip and play it.
            m_PlayerAudio.clip = clip;
            m_PlayerAudio.Play();

            // Return after the clip has finished.
            yield return new WaitForSeconds (clip.length);
        }


        private void HandleSetTarget(Transform target)
        {
            // If the game isn't over set the destination of the AI controlling the character and the trail showing its path.
            if (m_IsGameOver)
                return;
            
            m_AiCharacter.SetTarget(target.position);
            m_AgentTrail.SetDestination();
        }
    }
}