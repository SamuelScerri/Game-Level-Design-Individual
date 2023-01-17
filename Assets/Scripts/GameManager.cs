using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
	public static GameManager s_gameManager;
	public static GameObject s_player;

	private SaveManager _saveManager;

	private Animator _animator;
	private AudioSource _dialogueAudio;

	[SerializeField]
	private GameObject _dialogueBox, _objectiveText, _healthText, _inventory;

	private List<GameObject> _inventoryItems;

	private bool _paused;
	private bool _requestDone;

	[SerializeField]
	private float _dialogueTextDelay;

	[SerializeField]
	private bool _resetSave;
	private bool _hasControl;

	private NPC _currentCharacter;

	[SerializeField]
	private AudioClip _showDialogueSound, _hideDialogueSound, _textSound;

	private static Coroutine _textCoroutine;

	private byte _activeItem;

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
		_dialogueAudio = GetComponent<AudioSource>();
		_hasControl = true;

		_inventoryItems = new List<GameObject>();

		//Here We Will Get Every Item Box
		for (byte i = 0; i < _inventory.transform.childCount; i ++)
			_inventoryItems.Add(_inventory.transform.GetChild(i).gameObject);

		_saveManager = new SaveManager();
		SetActiveItem(0);

		//This Will Load The Game
		if (!_resetSave)
			LoadGame();
	}

	public void SetActiveItem(byte item)
	{
		//First We Clamp The Value To Avoid Any Errors
		item = (byte) Mathf.Clamp(item, 0, _inventoryItems.Count - 1);

		//Here We Will Reset The Colour Of The Previous Active Item's Box & Set The New Item Box
		_inventoryItems[_activeItem].GetComponent<Image>().color = Color.white;
		_inventoryItems[item].GetComponent<Image>().color = Color.grey;

		//Now We Just Change The Active Item
		_activeItem = item;
	}

	public void SetObjective(string objective)
	{
		s_gameManager._objectiveText.GetComponent<Text>().text = "Current Objective: " + objective;
	}

	public void ShowDialogue(Dialogue dialogue, NPC character)
	{
		_textCoroutine = s_gameManager.StartCoroutine(s_gameManager.DisplayTextCoroutine(dialogue, character));
	}

	public void ContinueDialogue(Dialogue dialogue)
	{
		StopCoroutine(_textCoroutine);

		_textCoroutine = s_gameManager.StartCoroutine(s_gameManager.DisplayTextCoroutine(dialogue, null));
	}

	public void GiveControl()
	{
		_hasControl = true;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void TakeControl()
	{
		Cursor.lockState = CursorLockMode.None;
		_hasControl = false;
	}

	public bool HasControl()
	{
		return _hasControl;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && _textCoroutine == null)
			TogglePause();
	}

	//This Is Responsible For Pausing The Game, When The Game Is Paused The Player Won't Be Able To Control Their Character
	public void TogglePause()
	{
		_animator.SetTrigger("Toggle Pause Menu");

		_paused = _paused ? false : true;
		Time.timeScale = _paused ? 0 : 1;

		if (!_paused)
			GiveControl();
		else TakeControl();
	}

	public void QuitGame()
	{
		Debug.Log("Game Has Ended");
		Application.Quit();
	}

	//This Will Save The Game By Converting The Save Manager Data To A Json File
	public void SaveGame()
	{
		Debug.Log("Saving Data...");

		_saveManager.PlayerPosition = s_player.transform.position;

		string jsonData = JsonUtility.ToJson(_saveManager);
		StartCoroutine(PostRequest("samuelscerrig1.pythonanywhere.com/api/savedata", jsonData));
	}

	//A Helper Function For Readability
	public void LoadGame()
	{
		StartCoroutine(GetRequest("samuelscerrig1.pythonanywhere.com/api/getdata"));
	}

	/*
		Code Adapted From:
		http://syedakbar.co/connecting-unity-with-a-database-using-python-flask-rest-webservice/
	*/
	private IEnumerator PostRequest(string url, string json)
	{
		var uwr = new UnityWebRequest(url, "POST");
		byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
		uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
		uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
		uwr.SetRequestHeader("Content-Type", "application/json");

		yield return uwr.SendWebRequest();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
			Debug.Log("Error While Sending: " + uwr.error);
		else
			Debug.Log("Received: " + uwr.downloadHandler.text);
	}

	/*
		Code Adapted From:
		http://syedakbar.co/connecting-unity-with-a-database-using-python-flask-rest-webservice/
	*/
	private IEnumerator GetRequest(string url)
	{
		UnityWebRequest uwr = UnityWebRequest.Get(url);
		yield return uwr.SendWebRequest();

		if (uwr.result == UnityWebRequest.Result.ConnectionError)
			Debug.Log("Error While Sending: " + uwr.error);
		else
			Debug.Log("Received: Data Successfully!");

		_saveManager = JsonUtility.FromJson<SaveManager>(uwr.downloadHandler.text);

		s_player.transform.position = _saveManager.PlayerPosition;
	}

	//This Will Process The Dialogue Object & Display In Sections
	private IEnumerator DisplayTextCoroutine(Dialogue dialogue, NPC character)
	{
		//First The Player Will Not Be Able To Move Anymore
		TakeControl();

		//We Then Show The Dialogue Box
		if (character != null)
		{
			_currentCharacter = character;
			_animator.SetTrigger("Toggle Dialogue Box");
		}
		_dialogueAudio.clip = _showDialogueSound;
		_dialogueAudio.Play();

		//The Text Is Cleared To Ensure That No Other Dialogue Is Present
		Text text = _dialogueBox.transform.GetChild(0).GetComponent<Text>();
		text.text = "";

		yield return new WaitForSeconds(1);
		_dialogueAudio.clip = _textSound;

		//Here We Will Iterate Over Every Dialogue Section
		foreach (DialogueSection section in dialogue.DialogueSections)
		{
			//Clear For The Next Section
			text.text = "";

			//Here We Will Display The Entire Text Character By Character, Mimicing Most Dialogue System In Other Games
			for (int i = 0; i < section.Speech.Length; i++)
			{
				text.text += section.Speech[i];
				_dialogueAudio.Play();

				//We Wait Here So That We Won't Show The Entire Text Immediately
				yield return new WaitForSeconds(_dialogueTextDelay);
			}

			//This Uses The Unity Event, Essentially Allowing Us To Be Able To Add Any Event We Want
			if (section.Event.Question)
			{
				yield return new WaitUntil(() => Input.GetKeyDown("space"));
				section.Event.Action.Invoke();
			}

			else
			{
				//Stop Entire Coroutine Until The User Has Pressed The Space Button
				yield return new WaitUntil(() => Input.GetKeyDown("space"));
				section.Event.Action.Invoke();

				if (section.Event.Action.GetPersistentEventCount() > 0)
					yield return new WaitForSeconds(1);
			}
		}

		//Hide The Dialogue Box & Reset The Character Back To Its Original State
		_animator.SetTrigger("Toggle Dialogue Box");

		_currentCharacter.ResetRotation();

		_dialogueAudio.clip = _hideDialogueSound;
		_dialogueAudio.Play();
		
		//Finally The Player Will Be Able To Move Again
		yield return new WaitForSeconds(1);
		GiveControl();

		//Clear Here, There Isn't Any Big Reason But It Could Free Up A Bit Of Memory
		text.text = "";

		_currentCharacter = null;
		_textCoroutine = null;
	}

	public void SetHealth(byte health)
	{
		_healthText.GetComponent<Text>().text = "Health: " + health.ToString();
	}
}
