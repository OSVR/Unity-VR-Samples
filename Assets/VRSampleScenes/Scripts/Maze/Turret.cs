using System.Collections;
using UnityEngine;

namespace VRStandardAssets.Maze
{
    // This script is used to control the gun
    // turret that prevents the character escaping
    // the maze.  It must be turned off by the
    // player using the switch.
    public class Turret : MonoBehaviour
    {
        [SerializeField] private float m_BarrelSpinSpeed = 1000f;   // The barrel of the gun is rotated manually, this is its speed.
        [SerializeField] private float m_AimTime = 0.5f;            // This is the amount of time the turret should take to aim at the player.
        [SerializeField] private float m_Range = 20f;               // How far the turret raycasts and so how far away the player can be shot.
        [SerializeField] private float m_CeaseFireTime = 0.2f;      // The length of time the gun will shoot for when it starts.
        [SerializeField] private Transform m_PlayerTransform;       // Used to aim at the player.
        [SerializeField] private Player m_Player;                   // Used to tell the player it has been shot.
        [SerializeField] private Transform m_TurretRotator;         // A child transform that is rotated to aim at the player.
        [SerializeField] private Transform m_TurretBarrel;          // Used to rotate the barrel.
        [SerializeField] private Animator m_Animator;               // This must be turned on and off when animation needs to be overridden.
        [SerializeField] private ParticleSystem m_GunParticles;     // This is turned on while the gun is shooting.
        [SerializeField] private AudioSource m_BulletAudio;         // The audio source that plays the sound of the bullets being fired.
        [SerializeField] private AudioSource m_GunAudio;            // The audio source that plays the sound of the gun's barrel spinning up and spinning down.
        [SerializeField] private AudioSource m_PowerAudio;          // The audio source that plays the sound of the turret powering up and powering down.
        [SerializeField] private AudioClip m_GunSpinUpClip;         // The clip of the gun's barrel spinning up.
        [SerializeField] private AudioClip m_GunSpinDownClip;       // The clip of the gun's barrel spinning down.
        [SerializeField] private AudioClip m_PowerUpClip;           // The clip of the turret powering up.
        [SerializeField] private AudioClip m_PowerDownClip;         // The clip of the turret powering down.


        private bool m_PlayerInSight;                               // Whether the player is currently in sight.
        private bool m_Firing;                                      // Whether the gun is currently firing.
        private float m_AimTimer;                                   // A normalised time used to smooth between animation and manually aiming at the player.
        private bool m_IsTurretActive;                              // Whether the turret is currently powered up.


        private readonly int m_HashPowerUpPara = Animator.StringToHash("PowerUp");          // Used to reference the different animator parameters.
        private readonly int m_HashPowerDownPara = Animator.StringToHash ("PowerDown");


        private const float k_PowerUpWaitTime = 2.033f;             // The amount of time it takes for the turret to power up (based on the animation).


        private void AimAtPlayer()
        {
            // Find the players position but at the rotators height.
            Vector3 playerAtTurretHeight = m_PlayerTransform.position;
            playerAtTurretHeight.y = m_TurretRotator.position.y;

            // Find a rotation of the turret rotating to facing the player.
            Quaternion newRotation = Quaternion.LookRotation(playerAtTurretHeight - m_TurretRotator.position);

            // If the normalised time hasn't reached 1 yet, increment it.
            if (m_AimTimer < 1f)
                m_AimTimer += Time.deltaTime / m_AimTime;
            else
                m_AimTimer = 1f;

            // Use the normalised time to rotate the turret smoothly so that it's facing the player.
            m_TurretRotator.rotation = Quaternion.Slerp(m_TurretRotator.rotation, newRotation, m_AimTimer);
            
            // Create a ray from the turret in the direction of the turret to the player.
            Ray ray = new Ray(m_TurretRotator.position, playerAtTurretHeight - m_TurretRotator.position);
            RaycastHit hit;

            // The player is in sight if the raycast hits something and the transform of what's been hit is the player's transform.
            m_PlayerInSight = Physics.Raycast(ray, out hit, m_Range) && hit.transform == m_PlayerTransform;

            // However the player only counts as being in sight if it's not dead.
            m_PlayerInSight &= !m_Player.Dead;
        }


        public void Activate()
        {
            // When the turret is activated start it powering up.
            StartCoroutine (PowerUp ());
        }


        public void Deactivate()
        {
            // When the turret is deactivated, reset the turret active and aim timer fields.
            m_IsTurretActive = false;
            m_AimTimer = 0f;
        }


        private IEnumerator PowerUp ()
        {
            // When powering up turn the animator on and set it to play the power up animation.
            m_Animator.enabled = true;
            m_Animator.SetTrigger (m_HashPowerUpPara);

            // Play the power up clip.
            m_PowerAudio.clip = m_PowerUpClip;
            m_PowerAudio.Play();

            // Wait for the animation to finish.
            yield return new WaitForSeconds (k_PowerUpWaitTime);

            // The turret is now active.
            m_IsTurretActive = true;

            // Start a loop that lasts whilst the turret is active.
            StartCoroutine (ActiveLoop ());
        }

        
        private IEnumerator ActiveLoop()
        {
            // The turret's transforms should not be controlled by the animator whilst aiming at the player.
            m_Animator.enabled = false;

            // This loop should continue until the turret is powered down and is made inactive.
            while (m_IsTurretActive)
            {
                // Rotate so the player is in front of the turret and detect if the player is in sight.
                AimAtPlayer();

                // If the player has been spotted but the turret is not yet firing, start spinning up and then firing.
                if (m_PlayerInSight && !m_Firing)
                {
                    StartCoroutine (SpinUpAndFire ());
                }

                // If the player is no longer in sight and the turret is still firing, stop fiing and spin down.
                if (!m_PlayerInSight && m_Firing)
                {
                    StartCoroutine (SpinDown ());
                }

                // Return next frame.
                yield return null;
            }

            // If the turret is no longer active then turn the animator back on.
            m_Animator.enabled = true;

            // Since the turret is not active, power down the turret.
            PowerDown();
        }


        private IEnumerator SpinUpAndFire ()
        {
            // Set firing to true so this coroutine is only started once.
            m_Firing = true;

            // Play the clip of the barrels spinning up.
            m_GunAudio.clip = m_GunSpinUpClip;
            m_GunAudio.Play();

            // While the player is in sight for the duration of the spinning up clip...
            float timer = 0f;
            while (m_PlayerInSight && timer < m_GunSpinUpClip.length)
            {
                timer += Time.deltaTime;

                // ... rotate the gun barrels with a speed based on the normalised time of the spinning up clip.
                float normalizedTime = timer / m_GunSpinUpClip.length;
                m_TurretBarrel.Rotate (Vector3.forward * m_BarrelSpinSpeed * Time.deltaTime * normalizedTime);

                yield return null;
            }

            // Now the gun has finished spinning up, if the player is still in sight hit the player and play the effects.
            if (m_PlayerInSight)
            {
                m_Player.TurretHit();
                m_GunParticles.Play();
                m_BulletAudio.Play();
            }
        }


        private IEnumerator SpinDown ()
        {
            // The player is no longer in sight but the turret is firing so it should stop.
            m_Firing = false;

            // Wait for the gun to stop firing.
            yield return new WaitForSeconds (m_CeaseFireTime);

            // Stop the particles and audio effects.
            m_GunParticles.Stop();
            m_BulletAudio.Stop ();

            // Play the audio of the gun spinning down.
            m_GunAudio.clip = m_GunSpinDownClip;
            m_GunAudio.Play();

            // While the player is not in sight and for the length of the spinning down clip...
            float timer = 0f;
            while (!m_PlayerInSight && timer < m_GunSpinDownClip.length)
            {
                timer += Time.deltaTime;

                // ... rotate the gun barrels with a speed inversely based on the normalised time of the spinning down clip.
                float normalizedTime = 1 - timer / m_GunSpinDownClip.length;
                m_TurretBarrel.Rotate(Vector3.forward * m_BarrelSpinSpeed * Time.deltaTime * normalizedTime);

                yield return null;
            }
        }


        private void PowerDown ()
        {
            // Play the audio and animation of the turret powering down.
            m_Animator.SetTrigger (m_HashPowerDownPara);
            m_PowerAudio.clip = m_PowerDownClip;
            m_PowerAudio.Play();
        }
    }
}