using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Common;
using VRStandardAssets.Utils;

namespace VRStandardAssets.ShootingGallery
{
    // This class controls the flow of the shooter games.  It
    // includes the introduction, spawning of targets and
    // outro.  Targets are spawned with a system that makes
    // spawnning more likely when there are fewer.
    public class ShootingGalleryController : MonoBehaviour
    {
        [SerializeField] private SessionData.GameType m_GameType;       // Whether this is a 180 or 360 shooter.
        [SerializeField] private int m_IdealTargetNumber = 5;           // How many targets aim to be on screen at once.
        [SerializeField] private float m_BaseSpawnProbability = 0.7f;   // When there are the ideal number of targets, this is the probability another will spawn.
        [SerializeField] private float m_GameLength = 60f;              // Time a game lasts in seconds.
        [SerializeField] private float m_SpawnInterval = 1f;            // How frequently a target could spawn.
        [SerializeField] private float m_EndDelay = 1.5f;               // The time the user needs to wait between the outro UI and being able to continue.
        [SerializeField] private float m_SphereSpawnInnerRadius = 5f;   // For the 360 shooter, the nearest targets can spawn.
        [SerializeField] private float m_SphereSpawnOuterRadius = 10f;  // For the 360 shooter, the furthest targets can spawn.
        [SerializeField] private float m_SphereSpawnMaxHeight = 15f;    // For the 360 shooter, the highest targets can spawn.
        [SerializeField] private SelectionSlider m_SelectionSlider;     // Used to confirm the user has understood the intro UI.
        [SerializeField] private Transform m_Camera;                    // Used to determine where targets can spawn.
        [SerializeField] private SelectionRadial m_SelectionRadial;     // Used to continue past the outro.
        [SerializeField] private Reticle m_Reticle;                     // This is turned on and off when it is required and not.
        [SerializeField] private Image m_TimerBar;                      // The time remaining is shown on the UI for the gun, this is a reference to the image showing the time remaining.
        [SerializeField] private ObjectPool m_TargetObjectPool;         // The object pool that stores all the targets.
        [SerializeField] private BoxCollider m_SpawnCollider;           // For the 180 shooter, the volume that the targets can spawn within.
        [SerializeField] private UIController m_UIController;           // Used to encapsulate the UI.
        [SerializeField] private InputWarnings m_InputWarnings;         // Tap warnings need to be on for the intro and outro but off for the game itself.


        private float m_SpawnProbability;                               // The current probability that a target will spawn at the next interval.
        private float m_ProbabilityDelta;                               // The difference to the probability caused by a target spawning or despawning.


        public bool IsPlaying { get; private set; }                     // Whether or not the game is currently playing.


        private IEnumerator Start()
        {
            // Set the game type for the score to be recorded correctly.
            SessionData.SetGameType(m_GameType);

            // The probability difference for each spawn is difference between 100% and the base probabilty per the number or targets wanted.
            // That means that if the ideal number of targets was 5, the base probability was 0.7 then the delta is 0.06.
            // So if there are no targets, the probability of one spawning will be 1, then 0.94, then 0.88, etc.
            m_ProbabilityDelta = (1f - m_BaseSpawnProbability) / m_IdealTargetNumber;

            // Continue looping through all the phases.
            while (true)
            {
                yield return StartCoroutine (StartPhase ());
                yield return StartCoroutine (PlayPhase ());
                yield return StartCoroutine (EndPhase ());
            }
        }


        private IEnumerator StartPhase ()
        {
            // Wait for the intro UI to fade in.
            yield return StartCoroutine (m_UIController.ShowIntroUI ());

            // Show the reticle (since there is now a selection slider) and hide the radial.
            m_Reticle.Show ();
            m_SelectionRadial.Hide ();

            // Turn on the tap warnings for the selection slider.
            m_InputWarnings.TurnOnDoubleTapWarnings ();
            m_InputWarnings.TurnOnSingleTapWarnings ();

            // Wait for the selection slider to finish filling.
            yield return StartCoroutine (m_SelectionSlider.WaitForBarToFill ());

            // Turn off the tap warnings since it will now be tap to fire.
            m_InputWarnings.TurnOffDoubleTapWarnings ();
            m_InputWarnings.TurnOffSingleTapWarnings ();

            // Wait for the intro UI to fade out.
            yield return StartCoroutine (m_UIController.HideIntroUI ());
        }


        private IEnumerator PlayPhase ()
        {
            // Wait for the UI on the player's gun to fade in.
            yield return StartCoroutine(m_UIController.ShowPlayerUI());

            // The game is now playing.
            IsPlaying = true;

            // Make sure the reticle is being shown.
            m_Reticle.Show ();

            // Reset the score.
            SessionData.Restart ();

            // Wait for the play updates to finish.
            yield return StartCoroutine (PlayUpdate ());

            // Wait for the gun's UI to fade.
            yield return StartCoroutine(m_UIController.HidePlayerUI());

            // The game is no longer playing.
            IsPlaying = false;
        }


        private IEnumerator EndPhase ()
        {
            // Hide the reticle since the radial is about to be used.
            m_Reticle.Hide ();
            
            // In order, wait for the outro UI to fade in then wait for an additional delay.
            yield return StartCoroutine (m_UIController.ShowOutroUI ());
            yield return new WaitForSeconds(m_EndDelay);
            
            // Turn on the tap warnings.
            m_InputWarnings.TurnOnDoubleTapWarnings();
            m_InputWarnings.TurnOnSingleTapWarnings();

            // Wait for the radial to fill (this will show and hide the radial automatically).
            yield return StartCoroutine(m_SelectionRadial.WaitForSelectionRadialToFill());

            // The radial is now filled so stop the warnings.
            m_InputWarnings.TurnOffDoubleTapWarnings();
            m_InputWarnings.TurnOffSingleTapWarnings();

            // Wait for the outro UI to fade out.
            yield return StartCoroutine(m_UIController.HideOutroUI());
        }


        private IEnumerator PlayUpdate ()
        {
            // When the updates start, the probability of a target spawning is 100%.
            m_SpawnProbability = 1f;

            // The time remaining is the full length of the game length.
            float gameTimer = m_GameLength;

            // The amount of time before the next spawn is the full interval.
            float spawnTimer = m_SpawnInterval;

            // While there is still time remaining...
            while (gameTimer > 0f)
            {
                // ... check if the timer for spawning has reached zero.
                if (spawnTimer <= 0f)
                {
                    // If it's time to spawn, check if a spawn should happen based on the probability.
                    if (Random.value < m_SpawnProbability)
                    {
                        // If a spawn should happen, restart the timer for spawning.
                        spawnTimer = m_SpawnInterval;

                        // Decrease the probability of a spawn next time because there are now more targets.
                        m_SpawnProbability -= m_ProbabilityDelta;

                        // Spawn a target.
                        Spawn (gameTimer);
                    }
                }

                // Wait for the next frame.
                yield return null;

                // Decrease the timers by the time that was waited.
                gameTimer -= Time.deltaTime;
                spawnTimer -= Time.deltaTime;

                // Set the timer bar to be filled by the amount 
                m_TimerBar.fillAmount = gameTimer / m_GameLength;
            }
        }


        private void Spawn (float timeRemaining)
        {
            // Get a reference to a target instance from the object pool.
            GameObject target = m_TargetObjectPool.GetGameObjectFromPool ();

            // Set the target's position to a random position. 
            target.transform.position = SpawnPosition();

            // Find a reference to the ShootingTarget script on the target gameobject and call it's Restart function.
            ShootingTarget shootingTarget = target.GetComponent<ShootingTarget>();
            shootingTarget.Restart(timeRemaining);

            // Subscribe to the OnRemove event.
            shootingTarget.OnRemove += HandleTargetRemoved;
        }


        private Vector3 SpawnPosition ()
        {
            // If this game is a 180 game then the random spawn position should be within the given collider.
            if (m_GameType == SessionData.GameType.SHOOTER180)
            {
                // Find the centre and extents of the spawn collider.
                Vector3 center = m_SpawnCollider.bounds.center;
                Vector3 extents = m_SpawnCollider.bounds.extents;

                // Get a random value between the extents on each axis.
                float x = Random.Range(center.x - extents.x, center.x + extents.x);
                float y = Random.Range(center.y - extents.y, center.y + extents.y);
                float z = Random.Range(center.z - extents.z, center.z + extents.z);
                
                // Return the point these random values make.
                return new Vector3(x, y, z);
            }

            // Otherwise the game is 360 and the spawn should be within a cylinder shape.
            // Find a random point on a unit circle and give it a radius between the inner and outer radii.
            Vector2 randomCirclePoint = Random.insideUnitCircle.normalized *
                                  Random.Range (m_SphereSpawnInnerRadius, m_SphereSpawnOuterRadius);

            // Find a random height between the camera's height and the maximum.
            float randomHeight = Random.Range (m_Camera.position.y, m_SphereSpawnMaxHeight);

            // The the random point on the circle is on the XZ plane and the random height is the Y axis.
            return new Vector3(randomCirclePoint.x, randomHeight, randomCirclePoint.y);
        }


        private void HandleTargetRemoved(ShootingTarget target)
        {
            // Now that the event has been hit, unsubscribe from it.
            target.OnRemove -= HandleTargetRemoved;

            // Return the target to it's object pool.
            m_TargetObjectPool.ReturnGameObjectToPool (target.gameObject);

            // Increase the likelihood of a spawn next time because there are fewer targets now.
            m_SpawnProbability += m_ProbabilityDelta;
        }
    }
}