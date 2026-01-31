using UnityEngine;

public class Button : MonoBehaviour
{
	[SerializeField] private RandomPrefabSpawner randomPrefabSpawner;

	public void Yes()
	{
		MaskProperties properties = randomPrefabSpawner.spawnedInstance.GetComponent<MaskProperties>();
		randomPrefabSpawner.ReplaceWithRandom();
	}

	public void No()
	{
		
	}
}
