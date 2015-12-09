using System;
using System.Collections;
using UnityEngine;

namespace VRStandardAssets.Flyer
{
    // This script looks after the explosion after an asteroid has been hit.
    public class AsteroidExplosion : MonoBehaviour
    {
        public event Action<AsteroidExplosion> OnExplosionEnded;    // This event is triggered after the last particle system has finished.


        private ParticleSystem[] m_ParticleSystems;                 // The particle systems for the explosion and their children.
        private float m_Duration;                                   // The longest duration of all the particle systems.


        private void Awake()
        {
            // Find all the particle systems.
            m_ParticleSystems = GetComponentsInChildren<ParticleSystem>(true);

            // By default the duration is zero.
            m_Duration = 0f;

            // Go through all the particle systems and if their duration is longer use that instead.
            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                if (m_ParticleSystems[i].duration > m_Duration)
                    m_Duration = m_ParticleSystems[i].duration;
            }
        }


        private void OnDestroy()
        {
            // Ensure the event is completely unsubscribed when the explosion is destroyed.
            OnExplosionEnded = null;
        }


        public void Restart()
        {
            // Go through all the particle systems and clear their current particles then play them.
            for (int i = 0; i < m_ParticleSystems.Length; i++)
            {
                m_ParticleSystems[i].Clear();
                m_ParticleSystems[i].Play();                
            }

            // Start the time out.
            StartCoroutine (Timeout ());
        }


        private IEnumerator Timeout()
        {
            // Wait for the longest particle system to finish.
            yield return new WaitForSeconds (m_Duration);

            // If anything is subscribed to OnExplosionEnded call it.
            if (OnExplosionEnded != null)
                OnExplosionEnded(this);
        }
    }
}