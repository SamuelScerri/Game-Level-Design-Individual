using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject _zombie;

	private Coroutine _spawnZombiesCoroutine;

	private void Update()
	{
		if (_spawnZombiesCoroutine == null)
			_spawnZombiesCoroutine = StartCoroutine(SpawnZombies());
	}

	private IEnumerator SpawnZombies()
	{
		GameObject newZombie = Instantiate(_zombie, transform.position, Quaternion.identity) as GameObject;
		newZombie.GetComponent<Zombie>()._currentState = State.Chasing;

		yield return new WaitForSeconds(30);
		_spawnZombiesCoroutine = null;
	}
}
