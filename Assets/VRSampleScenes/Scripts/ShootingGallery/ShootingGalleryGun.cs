using System.Collections;
using UnityEngine;
using UnityEngine.VR;
using VRStandardAssets.Utils;

namespace VRStandardAssets.ShootingGallery
{
    // This script controls the gun for the shooter
    // scenes, including it's movement and shooting.
    public class ShootingGalleryGun : MonoBehaviour
    {
        [SerializeField] private float m_DefaultLineLength = 70f;                       // How far the line renderer will reach if a target isn't hit.
        [SerializeField] private float m_Damping = 0.5f;                                // The damping with which this gameobject follows the camera.
        [SerializeField] private float m_GunFlareVisibleSeconds = 0.07f;                // How long, in seconds, the line renderer and flare are visible for with each shot.
        [SerializeField] private float m_GunContainerSmoothing = 10f;                   // How fast the gun arm follows the reticle.
        [SerializeField] private AudioSource m_GunAudio;                                // The audio source which plays the sound of the gun firing.
        [SerializeField] private ShootingGalleryController m_ShootingGalleryController; // Reference to the controller so the gun cannot fire whilst the game isn't playing.
        [SerializeField] private VREyeRaycaster m_EyeRaycaster;                         // Used to detect whether the gun is currently aimed at something.
        [SerializeField] private VRInput m_VRInput;                                     // Used to tell the gun when to fire.
        [SerializeField] private Transform m_CameraTransform;                           // Used as a reference to move this gameobject towards.
        [SerializeField] private Transform m_GunContainer;                              // This contains the gun arm needs to be moved smoothly.
        [SerializeField] private Transform m_GunEnd;                                    // This is where the line renderer should start from.
        [SerializeField] private LineRenderer m_GunFlare;                               // This is used to display the gun as a laser.
        [SerializeField] private Reticle m_Reticle;                                     // This is what the gun arm should be aiming at.
        [SerializeField] private ParticleSystem m_FlareParticles;                       // This particle system plays when the gun fires.
        [SerializeField] private GameObject[] m_FlareMeshes;                            // These are meshes of which one is randomly activated when the gun fires.


        private const float k_DampingCoef = -20f;                                       // This is the coefficient used to ensure smooth damping of this gameobject.


        private void Awake()
        {
            m_GunFlare.enabled = false;
        }


        private void OnEnable ()
        {
            m_VRInput.OnDown += HandleDown;
        }


        private void OnDisable ()
        {
            m_VRInput.OnDown -= HandleDown;
        }


        private void Update()
        {
            // Smoothly interpolate this gameobject's rotation towards that of the user/camera.
            transform.rotation = Quaternion.Slerp(transform.rotation, InputTracking.GetLocalRotation(VRNode.Head),
                m_Damping * (1 - Mathf.Exp(k_DampingCoef * Time.deltaTime)));
            
            // Move this gameobject to the camera.
            transform.position = m_CameraTransform.position;

            // Find a rotation for the gun to be pointed at the reticle.
            Quaternion lookAtRotation = Quaternion.LookRotation (m_Reticle.ReticleTransform.position - m_GunContainer.position);

            // Smoothly interpolate the gun's rotation towards that rotation.
            m_GunContainer.rotation = Quaternion.Slerp (m_GunContainer.rotation, lookAtRotation,
                m_GunContainerSmoothing * Time.deltaTime);
        }


        private void HandleDown ()
        {
            // If the game isn't playing don't do anything.
            if (!m_ShootingGalleryController.IsPlaying)
                return;
            
            // Otherwise, if there is an interactible currently being looked at, try to find it's ShootingTarget component.
            ShootingTarget shootingTarget = m_EyeRaycaster.CurrentInteractible ? m_EyeRaycaster.CurrentInteractible.GetComponent<ShootingTarget>() : null;

            // If there is a ShootingTarget component get it's transform as the target for shooting at.
            Transform target = shootingTarget ? shootingTarget.transform : null;

            // Start shooting at the target.
            StartCoroutine (Fire (target));
        }


        private IEnumerator Fire(Transform target)
        {
            // Play the sound of the gun firing.
            m_GunAudio.Play();

            // Set the length of the line renderer to the default.
            float lineLength = m_DefaultLineLength;

            // If there is a target, the line renderer's length is instead the distance from the gun to the target.
            if (target)
                lineLength = Vector3.Distance (m_GunEnd.position, target.position);

            // Chose an index for a random flare mesh.
            int randomFlareIndex = Random.Range (0, m_FlareMeshes.Length);

            // Store the rotation of that random flare and set it randomly rotate around the z axis.
            Vector3 randomEulerRotation = m_FlareMeshes[randomFlareIndex].transform.eulerAngles;
            randomEulerRotation.z = Random.Range (0f, 360f);

            // Set the random rotation that has been stored back to the flare and turn it on.
            m_FlareMeshes[randomFlareIndex].transform.eulerAngles = randomEulerRotation;
            m_FlareMeshes[randomFlareIndex].SetActive (true);

            // Play the particle system for the gun.
            m_FlareParticles.Play();

            // Turn the line renderer on.
            m_GunFlare.enabled = true;
            
            // Whilst the line renderer is on move it with the gun.
            yield return StartCoroutine (MoveLineRenderer (lineLength));
            
            // Turn the line renderer off again.
            m_GunFlare.enabled = false;

            // Turn the random flare mesh off.
            m_FlareMeshes[randomFlareIndex].SetActive(false);

        }


        private IEnumerator MoveLineRenderer (float lineLength)
        {
            // Create a timer.
            float timer = 0f;

            // While that timer has not yet reached the length of time that the gun effects should be visible for...
            while (timer < m_GunFlareVisibleSeconds)
            {
                // ... set the line renderer to start at the gun and finish forward of the gun the determined distance.
                m_GunFlare.SetPosition(0, m_GunEnd.position);
                m_GunFlare.SetPosition(1, m_GunEnd.position + m_GunEnd.forward * lineLength);

                // Wait for the next frame.
                yield return null;

                // Increment the timer by the amount of time waited.
                timer += Time.deltaTime;
            }
        }
    }
}