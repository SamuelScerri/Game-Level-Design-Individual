using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPC : MonoBehaviour
{
	[SerializeField]
	private Dialogue _dialogue;

	[SerializeField]
	private bool _talkAutomatically;

	private float _originalRotation, _desiredRotation, _desiredDampedRotation;

	private void Start()
	{
		_originalRotation = transform.eulerAngles.y;
		_desiredRotation = _originalRotation;

		if (_talkAutomatically)
			TalkTo(GameManager.s_player.gameObject);
	}

	public void TalkTo(GameObject character)
	{
		GameManager.ShowDialogue(_dialogue, this);
		
		Vector3 lookPosition = character.transform.position - transform.position;
		_desiredRotation = Quaternion.LookRotation(lookPosition).eulerAngles.y;
	}

	public void ResetRotation()
	{
		_desiredRotation = _originalRotation;
	}

	private void Update()
	{
		transform.eulerAngles = Vector2.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, _desiredRotation, ref _desiredDampedRotation, .1f);
	}
}
