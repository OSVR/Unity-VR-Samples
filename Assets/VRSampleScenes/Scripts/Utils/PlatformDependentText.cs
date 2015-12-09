using System;
using UnityEngine;
using UnityEngine.UI;

namespace VRStandardAssets.Utils
{
    // This script is to make the content of a Text
    // component different depending on the platform.
    public class PlatformDependentText : MonoBehaviour
    {
        // This class is used to group together the platform and text specific to it.
        [Serializable]
        public class PlatformTextPair
        {
            public RuntimePlatform m_Platform;
            [Multiline] public string m_Text;


            public PlatformTextPair (RuntimePlatform platform)
            {
                m_Platform = platform;
            }
        }


        [SerializeField] private Text m_TextComponent;                      // Refernce to the component that is going to display the text.
        [SerializeField] private PlatformTextPair[] m_PlatformTextPairs =   // Collection of platforms with associated strings.
        {
            new PlatformTextPair (RuntimePlatform.OSXEditor),               // By default this collection has instances for editor on
            new PlatformTextPair (RuntimePlatform.WindowsEditor),           // both editor platforms.
            new PlatformTextPair (RuntimePlatform.WindowsPlayer),           // For occulus deployment there needs to be a WindowsPlayer platform.
            new PlatformTextPair (RuntimePlatform.Android),                 // For GearVR deployment there needs to be an Android platform.
        };


        // OnValidate is called whenever anything changes in the inspector for this script.
        // It is only executed in the editor.  This is used to make setting up this script less time consuming.
        private void OnValidate ()
        {
            // Only continue with the function if there is a Text component reference...
            if (!m_TextComponent)
                return;

            // ...and the text component has some text written in it...
            if (m_TextComponent.text == string.Empty)
                return;

            // ...and the PlatformTextPair collection has been intialised...
            if (m_PlatformTextPairs == null)
                return;

            // ...and there are instances of PlatformTextPair in the collection...
            if (m_PlatformTextPairs.Length == 0)
                return;

            // ...and there's nothing written in the first entry.
            if (m_PlatformTextPairs[0].m_Text != string.Empty)
                return;

            // If the checks are passed, set the first PlatformTextPair to have the same text as the text component.
            m_PlatformTextPairs[0].m_Text = m_TextComponent.text;
        }
        
        
        private void Awake ()
        {
            // Go through all the PlatformTextPairs and if they have the current platform set the text appropriately.
            for (int i = 0; i < m_PlatformTextPairs.Length; i++)
            {
                if (m_PlatformTextPairs[i].m_Platform == Application.platform)
                    m_TextComponent.text = m_PlatformTextPairs[i].m_Text;
            }
        }
    }
}