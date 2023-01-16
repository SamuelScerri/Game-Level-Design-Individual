using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
	[SerializeField]
	private byte _health, _invisibilityTime;

	private Vector3 _scaleDamped, _colourDamped;
	private Coroutine _healthCoroutine;
	private Material _material;


	private void Start()
	{
		_material = transform.GetChild(0).GetComponent<Renderer>().material;
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

	public void Damage()
	{
		if (_healthCoroutine == null)
			_healthCoroutine = StartCoroutine(HealthCoroutine());
	}

	private IEnumerator HealthCoroutine()
	{
		transform.GetChild(0).localScale = Vector3.one * 1.1f;
		_material.color = Color.red;
		_health --;

		yield return new WaitForSeconds(_invisibilityTime);

		_healthCoroutine = null;
	}
}
