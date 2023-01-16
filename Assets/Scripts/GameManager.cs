using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class GameManager : MonoBehaviour
{
	public static GameManager s_gameManager;
	public static PlayerMovement s_player;

	private SaveManager _saveManager;

	private Animator _animator;
	private AudioSource _dialogueAudio;
	private GameObject _dialogueBox;

	private bool _paused;
	private bool _requestDone;

	[SerializeField]
	private float _dialogueTextDelay;

	[SerializeField]
	private AudioClip _showDialogueSound, _hideDialogueSound, _textSound;

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

		//Get The Dialogue Box & Create A New Save Manager
		_dialogueBox = transform.GetChild(1).gameObject;
		_saveManager = new SaveManager();

		//This Will Load The Game
		LoadGame();
	}

	public void ShowDialogue(Dialogue dialogue, NPC character)
	{
		StartCoroutine(DisplayTextCoroutine(dialogue, character));
	}

	private void Update()
	{
		if (Input.GetKeyDown("p") && s_player.HasControl())
			TogglePause();
	}

	//This Is Responsible For Pausing The Game, When The Game Is Paused The Player Won't Be Able To Control Their Character
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
		s_player.TakeControl();

		//We Then Show The Dialogue Box
		_animator.SetTrigger("Toggle Dialogue Box");
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

			//Stop Entire Coroutine Until The User Has Pressed The Space Button
			yield return new WaitUntil(() => Input.GetKeyDown("space"));
		}

		//Hide The Dialogue Box & Reset The Character Back To Its Original State
		_animator.SetTrigger("Toggle Dialogue Box");
		character.ResetRotation();

		_dialogueAudio.clip = _hideDialogueSound;
		_dialogueAudio.Play();
		
		//Finally The Player Will Be Able To Move Again
		yield return new WaitForSeconds(1);
		s_player.GiveControl();

		//Clear Here, There Isn't Any Big Reason But It Could Free Up A Bit Of Memory
		text.text = "";
	}
}
