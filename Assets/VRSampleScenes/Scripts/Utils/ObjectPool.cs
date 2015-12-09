using System.Collections.Generic;
using UnityEngine;

namespace VRStandardAssets.Utils
{
    // This is a simple object pooling script that
    // allows for random variation in prefabs.
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private GameObject[] m_Prefabs;            // These are prefabs which are all variations of the same (for example various asteroids).
        [SerializeField] private int m_NumberInPool;                // The number of prefabs to be initially instanced for the pool.


        private List<GameObject> m_Pool = new List<GameObject> ();  // The list of instantiated prefabs making up the pool.


        private void Awake ()
        {
            // Add as many random variations to the pool as initially determined.
            for (int i = 0; i < m_NumberInPool; i++)
            {
                AddToPool ();
            }
        }


        private void AddToPool ()
        {
            // Select a random prefab.
            int randomIndex = Random.Range (0, m_Prefabs.Length);

            // Instantiate the prefab.
            GameObject instance = Instantiate(m_Prefabs[randomIndex]);

            // Make the instance a child of this pool and turn it off.
            instance.transform.parent = transform;
            instance.SetActive (false);

            // Add the instance to the pool for later use.
            m_Pool.Add (instance);
        }


        public GameObject GetGameObjectFromPool ()
        {
            // If there aren't any instances left in the pool, add one.
            if (m_Pool.Count == 0)
                AddToPool ();
            
            // Get a reference to the first gameobject in the pool.
            GameObject ret = m_Pool[0];

            // Remove that gameobject from the pool list.
            m_Pool.RemoveAt(0);

            // Activate the instance.
            ret.SetActive (true);

            // Put it in the root of the hierarchy.
            ret.transform.parent = null;

            // Return the unpooled instance.
            return ret;
        }


        public void ReturnGameObjectToPool (GameObject go)
        {
            // Add the gameobject to the pool list.
            m_Pool.Add (go);

            // Deactivate the gameobject and make it a child of the pool.
            go.SetActive (false);
            go.transform.parent = transform;
        }
    }
}