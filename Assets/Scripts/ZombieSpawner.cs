using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
	[SerializeField]
	private GameObject _zombie;

	private Coroutine _spawnZombiesCoroutine;

	private byte _spawnTimer = 60;

	private void Update()
	{
		if (_spawnZombiesCoroutine == null)
			_spawnZombiesCoroutine = StartCoroutine(SpawnZombies());
	}

	private IEnumerator SpawnZombies()
	{
		GameObject newZombie = Instantiate(_zombie, transform.position, Quaternion.identity) as GameObject;
		newZombie.GetComponent<Zombie>()._currentState = State.Chasing;

		yield return new WaitForSeconds((float) _spawnTimer);

		_spawnTimer = (byte) Mathf.Clamp(_spawnTimer - 10, 15, Mathf.Infinity);
		_spawnZombiesCoroutine = null;
	}
}
