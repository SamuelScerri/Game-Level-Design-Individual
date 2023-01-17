using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HealthManager : MonoBehaviour
{
	[SerializeField]
	private byte _health, _invisibilityTime;

	[SerializeField]
	private SoundVariety _impactSounds;

	[SerializeField]
	private GameObject[] _droppableObjects;

	private Vector3 _scaleDamped, _colourDamped;
	private Coroutine _healthCoroutine;
	private Material _material;
	private AudioSource _source;
	private Animator _animator;

	private void Start()
	{
		_source = GetComponent<AudioSource>();
		_material = transform.GetChild(0).GetComponent<Renderer>().material;
		_animator = transform.GetChild(0).GetComponent<Animator>();

		if (gameObject.tag == "Player")
			GameManager.s_gameManager.SetHealth(_health);
	}

	// Update is called once per frame
	private void Update()
	{
		transform.GetChild(0).localScale = Vector3.SmoothDamp(transform.GetChild(0).localScale, Vector3.one, ref _scaleDamped, .1f);
		_material.color = new Color(
			Mathf.SmoothDamp(_material.color.r, 1, ref _colourDamped.x, .1f),
			Mathf.SmoothDamp(_material.color.g, 1, ref _colourDamped.y, .1f),
			Mathf.SmoothDamp(_material.color.b, 1, ref _colourDamped.z, .1f));
	}

	public void GainHealth(byte amount)
	{
		_health = (byte) Mathf.Clamp(_health + amount, 0, 255);
		GameManager.s_gameManager.SetHealth(_health);
	}

	public void Damage()
	{
		if (_healthCoroutine == null)
			_healthCoroutine = StartCoroutine(HealthCoroutine());
	}

	private void Die()
	{
		_animator.SetTrigger("Die");

		//Here We Will Basically Get Rid Of All The Logic
		MonoBehaviour[] scripts = GetComponents<MonoBehaviour>();

		foreach (MonoBehaviour script in scripts)
			if (script != this)
				Destroy(script);
				
		Destroy(GetComponent<Collider>());
		
		if (gameObject.tag == "Enemy")
			Destroy(GetComponent<NavMeshAgent>());
	}

	private IEnumerator HealthCoroutine()
	{
		_source.clip = _impactSounds.GetRandomSoundVariation();
		_source.Play();

		transform.GetChild(0).localScale = Vector3.one * 1.1f;
		_material.color = Color.red;

		_health = (byte) Mathf.Clamp(_health - 1, 0, 255);

		if (gameObject.tag == "Player")
			GameManager.s_gameManager.SetHealth(_health);

		if (_health == 0)
		{
			Die();

			yield return new WaitForSeconds(1);

			if (_droppableObjects.Length > 0)
				DropItem();
		}

		yield return new WaitForSeconds(_invisibilityTime);

		_healthCoroutine = null;
	}

	private void DropItem()
	{
		Instantiate(_droppableObjects[Random.Range(0, _droppableObjects.Length)], transform.position + Vector3.up * 2, Quaternion.identity);
	}

	public byte GetHealth()
	{
		return _health;
	}
}
