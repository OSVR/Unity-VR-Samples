using System;
using System.Collections.Generic;
using UnityEngine;
using VRStandardAssets.Common;

namespace VRStandardAssets.Flyer
{
    // This script handles the behaviour of the gates
    // in the flyer scene including changing their colour
    // and adding to the player's score.
    public class Ring : MonoBehaviour
    {
        public event Action<Ring> OnRingRemove;
        

        [SerializeField] private int m_Score = 100;                         // The amount added to the player's score when the ring is activated.
        [SerializeField] private AudioSource m_AudioSource;                 // Reference to the audio source that plays a clip when the player activates the ring.
        [SerializeField] private Color m_BaseColor = Color.blue;            // The colour the ring is by defalt.
        [SerializeField] private Color m_ShipAlignedColor = Color.yellow;   // The colour the ring is when the ship is aligned with it.
        [SerializeField] private Color m_ActivatedColor = Color.green;      // The colour the ring is when it has been activated.


        private bool m_HasTriggered;
        private Transform m_Cam;
        private GameObject m_Flyer;
        private List<Material> m_Materials;
        private bool m_ShipAligned;


        private const float k_RemovalDistance = 50f;


        // This property is used choose a colour for the ring based on the flyer's alignment.
        public bool ShipAligned
        {
            set
            {
                m_ShipAligned = value;

                // If this ring has already been triggered it should be the triggered colour so return.
                if (m_HasTriggered)
                    return;

                // Otherwise set the ring's colour based on whether the ship 
                SetRingColour (m_ShipAligned ? m_ShipAlignedColor : m_BaseColor);
            }

            get { return m_ShipAligned; }
        }


        private void Awake()
        {
            // Create a list of materials and add the main material on each child renderer to it.
            m_Materials = new List<Material>();
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            for (int i = 0; i < renderers.Length; i++)
            {
                m_Materials.Add(renderers[i].material);
            }

            // Set references to the camera and flyer.
            m_Cam = Camera.main.transform;
            m_Flyer = GameObject.FindGameObjectWithTag ("Player");
        }


        private void Update()
        {
            // If the ring is far enough behind the camera and something is subscribed to OnRingRemove call it.
            if (transform.position.z < m_Cam.position.z - k_RemovalDistance)
                if (OnRingRemove != null)
                    OnRingRemove(this);
        }


        private void OnTriggerEnter(Collider other)
        {
            // If this ring has already triggered or the ring has not collided with the flyer return.
            if (m_HasTriggered || other.gameObject != m_Flyer)
                return;

            // Otherwise the ring has been triggered.
            m_HasTriggered = true;

            // Play audio.
            m_AudioSource.Play();

            // Add to the score.
            SessionData.AddScore(m_Score);

            // Set the ring's colour.
            SetRingColour (m_ActivatedColor);
        }


        private void OnDestroy()
        {
            // Ensure the event is completely unsubscribed when the ring is destroyed.
            OnRingRemove = null;
        }


        public void Restart()
        {
            // Reset the colour to it's original colour.
            SetRingColour (m_BaseColor);

            // The ring has no longer been triggered.
            m_HasTriggered = false;
        }


        private void SetRingColour (Color color)
        {
            // Go through all the materials and set their colour appropriately.
            for (int i = 0; i < m_Materials.Count; i++)
            {
                m_Materials[i].color = color;
            }
        }
    }
}