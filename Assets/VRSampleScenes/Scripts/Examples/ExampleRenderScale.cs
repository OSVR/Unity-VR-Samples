using UnityEngine;
using UnityEngine.VR;

namespace VRStandardAssets.Examples
{
    // This script shows how renderscale affects the visuals
    // and performance of a scene.
    public class ExampleRenderScale : MonoBehaviour
    {
        [SerializeField] private float m_RenderScale = 1.5f;              //The render scale. Higher numbers = better quality, but trades performance
	 

	    void Start ()
        {
            VRSettings.renderScale = m_RenderScale;
	    }
    }
}