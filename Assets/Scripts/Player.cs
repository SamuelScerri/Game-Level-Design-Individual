using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField]
	private float _cameraSensitivity, _cameraMaxSpeed, _playerAcceleration, _playerSpeed;

	[SerializeField]
	private Vector3 _cameraPosition;

	[SerializeField]
	private GameObject _cameraPrefab;

	private CharacterController _characterController;
	private Animator _animator;

	private Vector2 _cameraRotation, _cameraDampedPosition, _cameraDampedRotation;
	private Vector3 _playerVelocity, _playerDampedVelocity, _previousDirection;

	private float _playerRotation, _playerDampedRotation;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;

		//Get The Character Controller & Animator
		_characterController = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();

		Instantiate(_cameraPrefab);
	}

	private void Update()
	{
		//Get The Direction Vector For Adding Velocity Later
		Vector3 direction = Camera.main.transform.TransformDirection(Vector3.right) * Input.GetAxisRaw("Horizontal") +
			Camera.main.transform.TransformDirection(Vector3.forward) * Input.GetAxisRaw("Vertical");
		direction = new Vector3(direction.x, 0, direction.z).normalized;

		//When The Player Is Moving, They Will Look Towards The Direction Vector
		if (direction != Vector3.zero)
		{
			_playerRotation = Quaternion.LookRotation(direction, Vector2.up).eulerAngles.y;
			_animator.SetTrigger("Run");
		}
		else _animator.SetTrigger("Idle");

		//When The Player Stops Moving, The Desired Rotation Is Set To The Velocity, This Is To Prevent The Character From Turning in 90 Degree Angles
		if (_previousDirection != Vector3.zero && direction == Vector3.zero)
			_playerRotation = Quaternion.LookRotation(_playerVelocity, Vector2.up).eulerAngles.y;

		//The Velocity & Rotation Will Smoothly Change To The Desired Target
		_playerVelocity = Vector3.SmoothDamp(_playerVelocity, direction * _playerSpeed, ref _playerDampedVelocity, _playerAcceleration);
		transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.eulerAngles.y, _playerRotation, ref _playerDampedRotation, .2f), 0);

		//Finally The Character Controller Is Used To Move The Player
		_characterController.Move(_playerVelocity * Time.deltaTime);
		_previousDirection = direction;
	}

	private void LateUpdate()
	{
		//A Seperate Vector Is Used To Ensure That The Player Won't Be Able To Flip The Camera Upside Down
		if (Mathf.Abs(_cameraDampedRotation.y) < _cameraMaxSpeed)
			_cameraRotation += new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * _cameraSensitivity;
		_cameraRotation = new Vector2(Mathf.Clamp(_cameraRotation.x, -30, 30), _cameraRotation.y);

		//This Is Responsible For Smoothly Rotating The Camera
		Camera.main.transform.rotation = Quaternion.Euler(
			Mathf.SmoothDampAngle(Camera.main.transform.eulerAngles.x, _cameraRotation.x, ref _cameraDampedRotation.x, .1f),
			Mathf.SmoothDampAngle(Camera.main.transform.eulerAngles.y, _cameraRotation.y, ref _cameraDampedRotation.y, .1f),
		0);

		//This Will Ensure That The Camera Will Rotate Around The Player
		Camera.main.transform.position = transform.position + Camera.main.transform.rotation * _cameraPosition;

		//This Will Ensure That The Camera Won't Be Behind An Object
		RaycastHit[] walls = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, Mathf.Abs(_cameraPosition.z));

		if (walls.Length > 0)
			Camera.main.transform.position = GetClosestRayPoint(walls);
		Camera.main.transform.position += Camera.main.transform.forward * .1f;
	}

	private Vector3 GetClosestRayPoint(RaycastHit[] hits)
	{
		Vector3 closestRaycast = Vector3.zero;
		float closestDistance = 0;

		foreach (RaycastHit hit in hits)
			if (Vector3.Distance(transform.position, hit.point) < closestDistance || closestDistance == 0)
			{
				closestRaycast = hit.point;
				closestDistance = Vector3.Distance(transform.position, hit.point);
			}

		return closestRaycast;
	}
}
