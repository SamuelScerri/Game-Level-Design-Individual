using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
	[SerializeField]
	private byte _attackCooldown;

	[SerializeField]
	private float _attackDelay, _attackRadius;

	[SerializeField]
	private SoundVariety _attackSounds;

	private Coroutine _attackCoroutine;
	private Animator _animator;

	private AudioSource _source;

	private void Start()
	{
		_animator = transform.GetChild(0).GetComponent<Animator>();
		_source = GetComponents<AudioSource>()[1];
	}

	private void Update()
	{
		//Get The Direction Vector For Adding Velocity Later
		Vector3 direction = Vector3.zero;

		if (GameManager.s_gameManager.HasControl())
		{
			direction = GetComponent<PlayerMovement>().GetDirection();
			CheckAttack();
		}
	}

	private void CheckAttack()
	{
		if (Input.GetMouseButtonDown(0) && _attackCoroutine == null)
			_attackCoroutine = StartCoroutine(AttackCoroutine());
	}

	private IEnumerator AttackCoroutine()
	{
		_animator.SetTrigger("Attack");
		_source.clip = _attackSounds.GetRandomSoundVariation();
		_source.Play();

		yield return new WaitForSeconds(_attackDelay);

		RaycastHit hit;

		//The Enemy's Collider Is Really Thin, So A Sphere Cast Should Help The Player Damage The Zombies Easier
		if (Physics.SphereCast(transform.position + Vector3.up * 2, 1, transform.forward, out hit, _attackRadius))
			if (hit.collider.tag == "Enemy")
				hit.transform.GetComponent<HealthManager>().Damage();

		yield return new WaitForSeconds(_attackCooldown);

		_attackCoroutine = null;
	}
}
