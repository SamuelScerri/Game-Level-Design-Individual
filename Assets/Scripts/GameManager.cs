using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public static GameManager s_gameManager;
	public static Player s_player;

	private Animator _animator;
	private GameObject _dialogueBox;

	private bool _paused;

	private void Awake()
	{
		if (s_gameManager == null)
			s_gameManager = this;
		else
			Destroy(this.gameObject);
	}

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_dialogueBox = transform.GetChild(0).gameObject;
	}

	public void ShowDialogue(Dialogue[] dialogue)
	{
		_animator.SetTrigger("Toggle Dialogue Box");
		StartCoroutine(DisplayTextCoroutine(dialogue));
	}

	private void Update()
	{
		if (Input.GetKeyDown("p"))
			TogglePause();
	}

	public void TogglePause()
	{
		_animator.SetTrigger("Toggle Pause Menu");

		_paused = _paused ? false : true;
		Time.timeScale = _paused ? 0 : 1;

		if (!_paused)
			s_player.GiveControl();
		else s_player.TakeControl();
	}

	public void QuitGame()
	{
		Debug.Log("Game Has Ended");
		Application.Quit();
	}

	private IEnumerator DisplayTextCoroutine(Dialogue[] dialogue)
	{
		s_player.TakeControl();

		Text text = _dialogueBox.transform.GetChild(0).GetComponent<Text>();
		text.text = "";

		yield return new WaitForSeconds(1);

		for (int d = 0; d < dialogue.Length; d++)
		{
			for (int i = 0; i < dialogue[d].Speech.Length; i++)
			{
				text.text += dialogue[d].Speech[i];
				yield return new WaitForSeconds(.02f);
			}

			//Stop Entire Coroutine Until The User Has Pressed The Space Button
			yield return new WaitUntil(() => Input.GetKeyDown("space"));

			text.text = "";
		}

		_animator.SetTrigger("Toggle Dialogue Box");
		
		yield return new WaitForSeconds(1);
		s_player.GiveControl();
	}
}
