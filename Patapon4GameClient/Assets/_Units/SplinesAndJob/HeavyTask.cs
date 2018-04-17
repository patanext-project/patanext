using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class HeavyTask : MonoBehaviour {

	public GameObject splinePrefab;
	public List<GameObject> instantiedGameObjects = new List<GameObject>();
	public int perCount;

	// Use this for initialization
	/*IEnumerator Start () {
		while (true)
		{
			var random = Random.Range(1, 2);
			if (random == 1)
			{ _Create(); _Destroy(); }
			else
			{ _Destroy(); _Create(); }
			yield return new WaitForSeconds(2);
			var newGameObject = Instantiate(splinePrefab, Random.insideUnitCircle * 2.5f, Quaternion.identity);
		}
	}*/

	private void Update()
	{
		perCount += Mathf.FloorToInt(Input.mouseScrollDelta.y * 10);
		if (Input.GetMouseButtonDown(0))
		{
			for (int i = 0; i < perCount || i < instantiedGameObjects.Count; i++)
			{
				var go = instantiedGameObjects[i];
				Destroy(go);
				instantiedGameObjects.Remove(go);
			}
		}
		if (Input.GetMouseButtonDown(1))
		{
			for (int i = 0; i < perCount; i++)
			{
				var newGameObject = Instantiate(splinePrefab, Random.insideUnitCircle * 2.5f, Quaternion.identity);
				instantiedGameObjects.Add(newGameObject);
			}			
		}
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(0, 0, 220, 20), "Spawn Size: " + perCount + ", entities: " + instantiedGameObjects.Count);
	}
}
