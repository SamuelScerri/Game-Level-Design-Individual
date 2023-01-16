using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
	[SerializeField]
	private byte _attackCooldown;

	private Coroutine _attackCoroutine;
	private Animator _animator;

	private void Start()
	{
		_animator = transform.GetChild(0).GetComponent<Animator>();
	}

	private void Update()
	{
		//Get The Direction Vector For Adding Velocity Later
		Vector3 direction = Vector3.zero;

		if (GetComponent<PlayerMovement>().HasControl())
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
		yield return new WaitForSeconds(_attackCooldown);

		_attackCoroutine = null;
	}
}
