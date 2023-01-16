using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum State
{
	Idling,
	Attacking
};

public class Zombie : MonoBehaviour
{
	[SerializeField]
	private State _currentState;

	private UnityEngine.AI.NavMeshAgent _agent;

	private void Start()
	{
		_agent = GetComponent<NavMeshAgent>();
	}
}
