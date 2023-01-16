using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
	[SerializeField]
	private float _cameraSensitivity, _cameraMaxSpeed;

	[SerializeField]
	private Vector3 _cameraPosition;

	[SerializeField]
	private GameObject _cameraPrefab;

	private Vector2 _cameraRotation, _cameraDampedPosition, _cameraDampedRotation;

	private void Start()
	{
		Instantiate(_cameraPrefab);
	}

	private void LateUpdate()
	{
		//We Will Only Move The Camera If The Player Has Control
		if (GetComponent<PlayerMovement>().HasControl())
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
}
