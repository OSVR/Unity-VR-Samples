using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace VRStandardAssets.Flyer
{
    public class FlyerHealthController : MonoBehaviour
    {
        [SerializeField] private float m_StartingHealth = 100f;         // The amount of health the flyer starts with.
        [SerializeField] private GameObject m_FlyerExplosionPrefab;     // A prefab of the flyer exploded into parts.
        [SerializeField] private Image m_HealthBar;                     // Reference to the image used as a health bar.
        [SerializeField] private AudioSource m_ExplosionAudio;          // Reference to the audio source used to play the explosion sound.
        [SerializeField] private AudioSource m_ThrusterAudio;           // Reference to the audio source used to play the sound of the flyer engines.
        [SerializeField] private GameObject[] m_FlyerUIGameObjects;     // All the gameobjects containing UI for the flyer (to be turned off on death).
        [SerializeField] private Renderer[] m_FlyerRenderers;           // All the renderers for the flyer (to be turned off on death).
        [SerializeField] private Collider[] m_FlyerColliders;           // All the colliders for the flyer (to be turned off on death).


        private float m_CurrentHealth;                                  // How much health the flyer currently has.
        private bool m_IsDead;                                          // Whether the flyer is currently dead.


        private const float k_WaitForExplosion = 3f;                    // How long to wait for the explosion to finish before destroying it.


        public bool IsDead { get { return m_IsDead; } }


        public void StartGame ()
        {
            // Turn all the visual and physical components of the flyer on.
            ShowFlyer (true);

            // The flyer is not dead and it's health is reset.
            m_IsDead = false;
            m_CurrentHealth = m_StartingHealth;
            m_HealthBar.fillAmount = 1f;
        }


        public void StopGame ()
        {
            // Turn all the visual and physical components of the flyer off.
            ShowFlyer (false);
        }


        private void ShowFlyer(bool show)
        {
            // Go through all the renderers, colliders and gameobjects and turn them on or off as appropriate.
            foreach (Renderer r in m_FlyerRenderers)
            {
                r.enabled = show;
            }

            foreach (Collider c in m_FlyerColliders)
            {
                c.enabled = show;
            }

            foreach (GameObject flyerUIGameObject in m_FlyerUIGameObjects)
            {
                flyerUIGameObject.SetActive (show);
            }

            // Play the thrusters if the flyer is being turned on and stop them if not.
            if (show)
                m_ThrusterAudio.Play();
            else
                m_ThrusterAudio.Stop();
        }


        public void TakeDamage(int damage)
        {
            // If the flyer is already dead no need to do anything.
            if (m_IsDead)
                return;

            // Decrement the current health by the damage but make sure it stays between the min and max.
            m_CurrentHealth -= damage;
            m_CurrentHealth = Mathf.Clamp(m_CurrentHealth, 0f, m_StartingHealth);

            // Set the health bar to show the normalised health amount.
            m_HealthBar.fillAmount = m_CurrentHealth / m_StartingHealth;

            // If the current health is approximately equal to zero the flyer is dead so destroy it.
            if (Mathf.Abs(m_CurrentHealth) < float.Epsilon)
            {
                m_IsDead = true;
                StartCoroutine(DestroyFlyer());
            }
        }


        private IEnumerator DestroyFlyer()
        {
            // Play the explosion audio.
            m_ExplosionAudio.Play();

            // Instantiate the explosion.
            GameObject flyerExplosion =
                Instantiate(m_FlyerExplosionPrefab, transform.position, Quaternion.identity) as GameObject;

            // Turn all the visual and physical components of the flyer off.
            ShowFlyer(false);

            // Wait for the explosion to finish.
            yield return new WaitForSeconds(k_WaitForExplosion);

            // Destroy the explosion gameobject.
            Destroy(flyerExplosion);
        }
    }
}