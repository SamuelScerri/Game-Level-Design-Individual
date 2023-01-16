using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum State
{
	Idling,
	Chasing,
	Attacking
};

public class Zombie : MonoBehaviour
{
	[SerializeField]
	private State _currentState;

	[SerializeField]
	private byte _attackCooldown;

	[SerializeField]
	private float _attackDelay;

	[SerializeField]
	private float _attackRadius, _chaseRadius;

	private float _desiredRotation, _desiredDampedRotation;

	private NavMeshAgent _agent;
	private Animator _animator;

	private Coroutine _attackCoroutine;

	private void Start()
	{
		_agent = GetComponent<NavMeshAgent>();
		_animator = transform.GetChild(0).GetComponent<Animator>();

		_animator.SetTrigger("Idle");
	}

	private void Update()
	{
		if (_currentState == State.Idling)
			if (Vector3.Distance(transform.position, GameManager.s_player.transform.position) < _chaseRadius)
				_currentState = State.Chasing;

		if (_currentState == State.Chasing)
		{
			if (_agent.velocity.magnitude < .1f)
				_animator.SetTrigger("Idle");
			else _animator.SetTrigger("Walk");

			_agent.SetDestination(GameManager.s_player.transform.position);
		}

		if (_currentState == State.Attacking)
		{
			_animator.SetTrigger("Idle");

			Vector3 lookPosition = GameManager.s_player.transform.position - transform.position;
			_desiredRotation = Quaternion.LookRotation(lookPosition).eulerAngles.y;

			transform.eulerAngles = Vector2.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _desiredRotation, ref _desiredDampedRotation, .1f);
		}

		if (Vector3.Distance(transform.position, GameManager.s_player.transform.position) < _attackRadius && _attackCoroutine == null)
			_attackCoroutine = StartCoroutine(AttackCoroutine());
	}

	private IEnumerator AttackCoroutine()
	{
		_currentState = State.Attacking;

		_animator.SetTrigger("Idle");
		_animator.SetTrigger("Attack");

		_agent.ResetPath();

		yield return new WaitForSeconds(_attackDelay);

		//Check If The Player Is Still Within Reach
		if (Vector3.Distance(transform.position, GameManager.s_player.transform.position) < _attackRadius)
			GameManager.s_player.GetComponent<HealthManager>().Damage();

		_animator.SetTrigger("Walk");
		_currentState = State.Chasing;
		yield return new WaitForSeconds(_attackCooldown);

		_attackCoroutine = null;
	}
}
