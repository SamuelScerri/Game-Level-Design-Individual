using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	[SerializeField]
	private float _cameraSensitivity, _cameraMaxSpeed, _playerAcceleration, _playerSpeed, _footstepDelay;

	[SerializeField]
	private Vector3 _cameraPosition;

	[SerializeField]
	private GameObject _cameraPrefab;

	[SerializeField]
	private SoundVariety _footsteps;

	[SerializeField]
	private bool _hasControl, _previousControlState;

	[SerializeField]
	private byte _interactionRadius;

	private CharacterController _characterController;
	private Animator _animator;
	private AudioSource _source;

	private Vector2 _cameraRotation, _cameraDampedPosition, _cameraDampedRotation;
	private Vector3 _playerVelocity, _playerDampedVelocity, _previousDirection;

	private float _playerRotation, _playerDampedRotation;

	private Coroutine _footstepCoroutine;
	private GameObject _nearestNPC;

	private void Start()
	{
		//Get The Character Controller & Animator
		_characterController = GetComponent<CharacterController>();
		_animator = GetComponent<Animator>();
		_source = GetComponent<AudioSource>();

		_previousControlState = true;

		Instantiate(_cameraPrefab);
		GameManager.s_player = this;
		GiveControl();
	}

	private void Update()
	{
		//Get The Direction Vector For Adding Velocity Later
		Vector3 direction = Vector3.zero;

		if (_hasControl)
		{
			direction = GetDirection();
			Interact();
		}

		//When The Player Is Moving, They Will Look Towards The Direction Vector
		if (direction != Vector3.zero)
		{
			_playerRotation = Quaternion.LookRotation(direction, Vector2.up).eulerAngles.y;

			if (_footstepCoroutine == null)
				_footstepCoroutine = StartCoroutine(Footstep());
			_animator.SetTrigger("Run");
		}
		else _animator.SetTrigger("Idle");

		//When The Player Stops Moving, The Desired Rotation Is Set To The Velocity, This Is To Prevent The Character From Turning in 90 Degree Angles
		if (_previousDirection != Vector3.zero && direction == Vector3.zero)
			_playerRotation = Quaternion.LookRotation(_playerVelocity, Vector2.up).eulerAngles.y;

		//The Velocity & Rotation Will Smoothly Change To The Desired Target
		_playerVelocity = Vector3.SmoothDamp(_playerVelocity, direction * _playerSpeed, ref _playerDampedVelocity, _playerAcceleration);
		transform.rotation = Quaternion.Euler(0, Mathf.SmoothDampAngle(transform.eulerAngles.y, _playerRotation, ref _playerDampedRotation, .1f), 0);

		//Finally The Character Controller Is Used To Move The Player
		_characterController.Move(_playerVelocity * Time.deltaTime);
		_previousDirection = direction;

		//Here We Get The Nearest NPC So That The Player Can Interact With
		_nearestNPC = GetNearestGameObject(GameObject.FindGameObjectsWithTag("NPC"));
	}

	private void LateUpdate()
	{
		if (_hasControl)
			UpdateCamera();

		//This Is Responsible For Smoothly Rotating The Camera
		Camera.main.transform.rotation = Quaternion.Euler(
			Mathf.SmoothDampAngle(Camera.main.transform.eulerAngles.x, _cameraRotation.x, ref _cameraDampedRotation.x, .1f),
			Mathf.SmoothDampAngle(Camera.main.transform.eulerAngles.y, _cameraRotation.y, ref _cameraDampedRotation.y, .1f),
		0);

		//This Will Ensure That The Camera Will Rotate Around The Player
		Camera.main.transform.position = transform.position + Camera.main.transform.rotation * _cameraPosition;

		//This Will Ensure That The Camera Won't Be Behind An Object
		RaycastHit hit;

		if (Physics.Raycast(transform.position + Vector3.up * 2, Camera.main.transform.position - (transform.position + Vector3.up * 2), out hit, Mathf.Abs(_cameraPosition.z)))
			Camera.main.transform.position = hit.point;

		Debug.DrawLine(transform.position + Vector3.up * 2, Camera.main.transform.position);
		Camera.main.transform.position += Camera.main.transform.forward * .5f;
	}

	private void UpdateCamera()
	{
		//A Seperate Vector Is Used To Ensure That The Player Won't Be Able To Flip The Camera Upside Down
		if (Mathf.Abs(_cameraDampedRotation.y) < _cameraMaxSpeed)
			_cameraRotation += new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * _cameraSensitivity;
		_cameraRotation = new Vector2(Mathf.Clamp(_cameraRotation.x, -15, 15), _cameraRotation.y);
	}

	//Returns The Direction Based Off The Camera Direction
	private Vector3 GetDirection()
	{
		Vector3 direction = Camera.main.transform.TransformDirection(Vector3.right) * Input.GetAxisRaw("Horizontal") +
			Camera.main.transform.TransformDirection(Vector3.forward) * Input.GetAxisRaw("Vertical");
		direction = new Vector3(direction.x, 0, direction.z).normalized;

		return direction;
	}

	private IEnumerator Footstep()
	{
		_source.clip = _footsteps.GetRandomSoundVariation();
		_source.Play();

		yield return new WaitForSeconds(_footstepDelay);
		_footstepCoroutine = null;
	}

	private void Interact()
	{
		if (Input.GetKeyDown("space") && _nearestNPC != null)
		{
			Vector3 lookPosition = _nearestNPC.transform.position - transform.position;
			_playerRotation = Quaternion.LookRotation(lookPosition).eulerAngles.y;

			_nearestNPC.GetComponent<NPC>().TalkTo(gameObject);
		}
	}

	//Gets The Closest Ray Position From Multiple Raycast Hits
	private Vector3 GetClosestRayPoint(RaycastHit[] hits)
	{
		Vector3 closestRaycast = Vector3.zero;
		float closestDistance = 0;

		foreach (RaycastHit hit in hits)
		{
			float distance = Vector3.Distance(transform.position, hit.point);

			if (closestDistance == 0 || distance < closestDistance)
			{
				closestRaycast = hit.point;
				closestDistance = distance;
			}
		}


		return closestRaycast;
	}

	private GameObject GetNearestGameObject(GameObject[] objects)
	{
		GameObject closestGameObject = null;
		float closestDistance = 0;

		foreach (GameObject currentObject in objects)
		{
			float distance = Vector3.Distance(transform.position, currentObject.transform.position);

			if (closestDistance == 0 || distance < closestDistance)
				if (distance < _interactionRadius)
				{
					closestGameObject = currentObject;
					closestDistance = distance;

			}
		}

		return closestGameObject;
	}

	public void GiveControl()
	{
		_hasControl = _previousControlState;

		if (_hasControl)
			Cursor.lockState = CursorLockMode.Locked;

		_previousControlState = true;
	}

	public void TakeControl()
	{
		_playerDampedRotation = 0;

		_previousControlState = _hasControl;
		Cursor.lockState = CursorLockMode.None;
		_hasControl = false;
	}

	public bool HasControl()
	{
		return _hasControl;
	}
}
