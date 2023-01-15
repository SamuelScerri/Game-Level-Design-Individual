using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
	private CharacterController _characterController;
	private Vector3 _cameraPosition;

	private Vector2 _cameraRotation, _cameraDampedPosition, _cameraDampedRotation;

	[SerializeField]
	private float _cameraSensitivity, _cameraMaxSpeed, _playerAcceleration, _playerSpeed;

	private void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;

		_characterController = GetComponent<CharacterController>();
		_cameraPosition = Camera.main.transform.localPosition;
	}

	private void Update()
	{

	}

	private void LateUpdate()
	{
		if (Mathf.Abs(_cameraDampedRotation.y) < _cameraMaxSpeed)
			_cameraRotation += new Vector2(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X")) * _cameraSensitivity;
		_cameraRotation = new Vector2(Mathf.Clamp(_cameraRotation.x, -30, 30), _cameraRotation.y);

		Camera.main.transform.rotation = Quaternion.Euler(
			Mathf.SmoothDampAngle(Camera.main.transform.localEulerAngles.x, _cameraRotation.x, ref _cameraDampedRotation.x, .1f),
			Mathf.SmoothDampAngle(Camera.main.transform.localEulerAngles.y, _cameraRotation.y, ref _cameraDampedRotation.y, .1f),
		0);

		Camera.main.transform.localPosition = Camera.main.transform.rotation * _cameraPosition;
	}
}
