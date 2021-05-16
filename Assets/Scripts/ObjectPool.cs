using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    // Shared instance handling the object pool.
    public static ObjectPool SharedInstance;
    // List of pooled objects.
    public List<GameObject> pooledObjects;
    // Prefab of the object to pool.
    public GameObject objectToPool;

    // User-specified amount of pooling.
    public int amountToPool;


    private void Awake()
    {
        // Init shared instance.
        SharedInstance = this;
    }

    private void Start()
    {
        // Init the list of pooled objects.
        pooledObjects = new List<GameObject>();

        // Instantiate the list of objects and de-activate them.
        GameObject tmp;
        for (int i = 0; i < amountToPool; i++)
        {
            tmp = Instantiate(objectToPool);
            tmp.SetActive(false);
            pooledObjects.Add(tmp);
        }
    }

    // Get an object from the pool.
    public GameObject GetPooledObject()
    {
        // Return the first inactive object from the pool.
        for (int i = 0; i < amountToPool; i++)
        {
            if (pooledObjects[i] != null)
            {
                if (!pooledObjects[i].activeInHierarchy)
                {
                    return pooledObjects[i];
                }
            }
        }
        return null;
    }
}
