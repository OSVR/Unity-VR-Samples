using UnityEngine;
using UnityEngine.UI;
using VRStandardAssets.Common;

namespace VRStandardAssets.ShootingGallery
{
    // This script displays the player's score during the
    // shooter scenes.
    public class ShootingGalleryScore : MonoBehaviour
    {
        [SerializeField] private Text m_ScoreText;


        private void Update()
        {
            m_ScoreText.text = SessionData.Score.ToString();
        }
    }
}