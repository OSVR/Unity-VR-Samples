using UnityEngine;
using VRStandardAssets.Utils;

namespace VRStandardAssets.Flyer
{
    // This script handles getting the laser instances from
    // the object pool and firing them.
    public class FlyerLaserController : MonoBehaviour
    {
        [SerializeField] private VRInput m_VRInput;                     // Reference to the VRInput so when the fire button is pressed it can be handled.
        [SerializeField] private FlyerGameController m_GameController;  // Reference to the game controller so firing can be limited to when the game is running.
        [SerializeField] private ObjectPool m_LaserObjectPool;          // Reference to the object pool the lasers belong to.
        [SerializeField] private Transform m_LaserSpawnPosLeft;         // The positions the lasers should spawn from.
        [SerializeField] private Transform m_LaserSpawnPosRight;
        [SerializeField] private AudioSource m_LaserAudio;              // The audio source that should play firing sounds.


        private void OnEnable()
        {
            m_VRInput.OnDown += HandleDown;
        }


        private void OnDisable()
        {
            m_VRInput.OnDown -= HandleDown;
        }


        private void HandleDown()
        {
            // If the game isn't running return.
            if (!m_GameController.IsGameRunning)
                return;

            // Fire laser from each position.
            SpawnLaser(m_LaserSpawnPosLeft);
            SpawnLaser(m_LaserSpawnPosRight);
        }


        private void SpawnLaser(Transform gunPos)
        {
            // Get a laser from the pool.
            GameObject laserGameObject = m_LaserObjectPool.GetGameObjectFromPool();

            // Set it's position and rotation based on the gun positon.
            laserGameObject.transform.position = gunPos.position;
            laserGameObject.transform.rotation = gunPos.rotation;

            // Find the FlyerLaser component of the laser instance.
            FlyerLaser flyerLaser = laserGameObject.GetComponent<FlyerLaser>();

            // Set it's object pool so it knows where to return to.
            flyerLaser.ObjectPool = m_LaserObjectPool;

            // Restart the laser.
            flyerLaser.Restart();

            // Play laser audio.
            m_LaserAudio.Play();
        }
    }
}