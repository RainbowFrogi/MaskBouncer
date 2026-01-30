using System.Collections.Generic;
using UnityEngine;

public class RandomPrefabSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private List<GameObject> prefabs = new List<GameObject>();

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private bool spawnAtCameraCenter = true;
    [SerializeField] private Camera targetCamera;

    [Header("Runtime")]
    [SerializeField] private GameObject spawnedInstance;

	void Start()
	{
		SpawnRandom();
	}

    public GameObject SpawnRandom()
    {
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning($"{nameof(RandomPrefabSpawner)}: No prefabs assigned.", this);
            return null;
        }

        int index = Random.Range(0, prefabs.Count);
        GameObject prefab = prefabs[index];
        if (prefab == null)
        {
            Debug.LogWarning($"{nameof(RandomPrefabSpawner)}: Prefab at index {index} is null.", this);
            return null;
        }

        Vector3 position = ResolveSpawnPosition();
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        spawnedInstance = Instantiate(prefab, position, rotation);
        return spawnedInstance;
    }

    public void ReplaceWithRandom()
    {
        if (spawnedInstance != null)
        {
            Destroy(spawnedInstance);
            spawnedInstance = null;
        }

        SpawnRandom();
    }

    public GameObject GetSpawnedInstance()
    {
        return spawnedInstance;
    }

    public void ClearSpawnedInstance()
    {
        if (spawnedInstance == null)
        {
            return;
        }

        Destroy(spawnedInstance);
        spawnedInstance = null;
    }

    private Vector3 ResolveSpawnPosition()
    {
        if (spawnPoint != null)
        {
            return spawnPoint.position;
        }

        if (spawnAtCameraCenter)
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam != null)
            {
                Vector3 center = new Vector3(0.5f, 0.5f, cam.nearClipPlane + 1f);
                return cam.ViewportToWorldPoint(center);
            }
        }

        return transform.position;
    }
}
