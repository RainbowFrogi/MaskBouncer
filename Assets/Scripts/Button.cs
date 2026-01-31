using UnityEngine;

public class Button : MonoBehaviour
{
	[SerializeField] private RandomPrefabSpawner randomPrefabSpawner;

	public void Yes()
	{
		randomPrefabSpawner.ReplaceWithRandom();
	}

	public void No()
	{
		
	}
}
