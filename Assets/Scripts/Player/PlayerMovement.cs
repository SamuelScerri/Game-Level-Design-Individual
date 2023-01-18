using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
	[SerializeField]
	private float _playerAcceleration, _playerSpeed, _footstepDelay;

	[SerializeField]
	private SoundVariety _footsteps;

	private CharacterController _characterController;
	private Animator _animator;
	private AudioSource _source;

	private Vector3 _playerVelocity, _playerDampedVelocity, _previousDirection, _playerScaleDamped;

	private float _playerRotation, _playerDampedRotation;

	private Coroutine _footstepCoroutine, _attackCoroutine;
	private GameObject _nearestNPC;

	public Vector3 WarpPosition;

	private void Start()
	{
		//Get The Character Controller & Animator
		_characterController = GetComponent<CharacterController>();
		_animator = transform.GetChild(0).GetComponent<Animator>();
		_source = GetComponents<AudioSource>()[2];



		//Here We Will Set This Player As The Main Player, This Will Allow Us To Access The Player More Easily
		GameManager.s_player = gameObject;
		GameManager.GiveControl();
	}

	private void Update()
	{
		_characterController.enabled = true;

		//Get The Direction Vector For Adding Velocity Later
		Vector3 direction = Vector3.zero;

		if (GameManager.HasControl())
			direction = GetDirection();

		if (Time.timeScale != 0)
		{
			//When The Player Is Moving, They Will Look Towards The Direction Vector
			if (direction != Vector3.zero)
			{
				_playerRotation = Quaternion.LookRotation(direction, Vector2.up).eulerAngles.y;

				if (_footstepCoroutine == null)
					_footstepCoroutine = StartCoroutine(FootstepCoroutine());
				_animator.SetTrigger("Run");
			}
			else _animator.SetTrigger("Idle");

			//When The Player Stops Moving, The Desired Rotation Is Set To The Velocity, This Is To Prevent The Character From Turning in 90 Degree Angles
			if (_previousDirection != Vector3.zero && direction == Vector3.zero)
				_playerRotation = Quaternion.LookRotation(_playerVelocity, Vector2.up).eulerAngles.y;

			//The Velocity & Rotation Will Smoothly Change To The Desired Target
			_playerVelocity = Vector3.SmoothDamp(_playerVelocity, direction * _playerSpeed, ref _playerDampedVelocity, _playerAcceleration);
			transform.eulerAngles = new Vector3(0, Mathf.SmoothDampAngle(transform.eulerAngles.y, _playerRotation, ref _playerDampedRotation, .1f), 0);

			//Finally The Character Controller Is Used To Move The Player
			_characterController.Move(_playerVelocity * Time.deltaTime);
			_previousDirection = direction;	
		}

		if (WarpPosition != Vector3.zero)
		{
			_characterController.enabled = false;

			transform.position = WarpPosition;
			WarpPosition = Vector3.zero;
		}
	}

	//Returns The Direction Based Off The Camera Direction
	public Vector3 GetDirection()
	{
		Vector3 direction = Camera.main.transform.TransformDirection(Vector3.right) * Input.GetAxisRaw("Horizontal") +
			Camera.main.transform.TransformDirection(Vector3.forward) * Input.GetAxisRaw("Vertical");
		direction = new Vector3(direction.x, 0, direction.z).normalized;

		return direction;
	}

	private IEnumerator FootstepCoroutine()
	{
		_source.clip = _footsteps.GetRandomSoundVariation();
		_source.Play();

		yield return new WaitForSeconds(_footstepDelay);
		_footstepCoroutine = null;
	}



	public void SetPlayerRotation(float rotation)
	{
		_playerRotation = rotation;
	}
}
