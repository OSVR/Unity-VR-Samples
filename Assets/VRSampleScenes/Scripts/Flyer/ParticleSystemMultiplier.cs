using System;
using UnityEngine;

namespace UnityStandardAssets.Effects
{
    public class ParticleSystemMultiplier : MonoBehaviour
    {
        // a simple script to scale the size, speed and lifetime of a particle system

        public float multiplier = 1;


        private void Start()
        {
            var systems = GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem system in systems)
            {
				ParticleSystem.MainModule main = system.main;

				main.startSizeMultiplier = multiplier;
				main.startSpeedMultiplier = multiplier;
				main.startLifetimeMultiplier = Mathf.Lerp(multiplier, 1, 0.5f);

                system.Clear();
                system.Play();
            }
        }
    }
}
