using System;
using UnityEngine;

namespace VRStandardAssets.Maze
{
    // This script controls what happens when
    // the character reaches the exit of the maze.
    public class ExitArea : MonoBehaviour
    {
        public event Action OnGameComplete;                     // This event is called when the the player reaches the exit area for the first time.


        [SerializeField] private Transform m_PlayerTransform;   // Reference to the character in the scene to determine what has entered the trigger.


        private Player m_Player;                                // Reference to the Player script so the player knows when the game is complete.
        private bool m_IsShowingGameOver;                       // To make sure the gameover event is only called once.


        private void Awake ()
        {
            // Find the Player component from the player's transform.
            m_Player = m_PlayerTransform.GetComponent<Player> ();
        }


        private void OnTriggerEnter(Collider other)
        {
            // If the triggering transform is not the player then return.
            if (other.transform != m_PlayerTransform)
                return;

            // Otherwise tell the player the game is complete and the game is over.
            m_Player.GameComplete();
            ShowGameOver();
        }


        public void Restart()
        {
            // When the game restarts the game is no longer over.
            m_IsShowingGameOver = false;
        }


        private void ShowGameOver()
        {
            // Make sure this is only called once by checking this bool and setting it afterwards.
            if (m_IsShowingGameOver)
                return;

            m_IsShowingGameOver = true;

            // If OnGameComplete has any subscribers call it.
            if (OnGameComplete != null)
                OnGameComplete();
        }
    }
}