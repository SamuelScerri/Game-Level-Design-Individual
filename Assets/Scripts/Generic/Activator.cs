using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
	private void OnTriggerEnter(Collider c)
	{
		if (c.gameObject.tag == "Player")
		{
			foreach (GameObject item in GameObject.FindGameObjectsWithTag("Zombie Spawner"))
				item.GetComponent<MonoBehaviour>().enabled = true;
		}
	}
}
