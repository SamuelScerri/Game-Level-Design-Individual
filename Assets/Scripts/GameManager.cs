using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

/*
	This Is The Game Manager, To Be Honest This Could Have Been Seperated Into Multiple Classes Like The Player,
	But It Has Became A Bit Unmaintanable & Seperating Would Be A Waste Of Time
*/

public class GameManager : MonoBehaviour
{
	public static GameManager s_gameManager;
	public static GameObject s_player;

	private SaveManager _saveManager;

	private Animator _animator;
	private AudioSource _dialogueAudio;

	[SerializeField]
	private GameObject _dialogueBox, _objectiveText, _healthText, _currencyText, _inventory;

	[SerializeField]
	private List<Item> _equippedItems;
	private GameObject[] _inventoryItems;

	private bool _paused;
	private bool _requestDone;

	[SerializeField]
	private float _dialogueTextDelay;

	[SerializeField]
	private bool _resetSave;
	private bool _hasControl;

	[SerializeField]
	private byte _currency;

	[SerializeField]
	private Dialogue _issueDialogue;

	private NPC _currentCharacter;

	private bool _yesPressed, _noPressed, _continueRequest;

	[SerializeField]
	private AudioClip _showDialogueSound, _hideDialogueSound, _textSound, _collectSound;

	private Coroutine _textCoroutine;

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

		_inventoryItems = new GameObject[7];
		_saveManager = new SaveManager();

		UpdateInventory();

		//This Will Load The Game
		if (!_resetSave)
			LoadGame();
		else
			ObtainCurrency(0);

		//Here We Update The Inventory
		SetActiveItem(0);
		
	}

	private static void UpdateInventory()
	{
		//Here We Will Get Every Item Box & Reset It For Later
		for (byte i = 0; i < s_gameManager._inventory.transform.childCount; i ++)
		{
			s_gameManager._inventoryItems[i] = s_gameManager._inventory.transform.GetChild(i).gameObject;

			//Here We Reset Every Item In The Inventory
			s_gameManager._inventoryItems[i].transform.GetChild(0).GetComponent<Image>().sprite = null;
			s_gameManager._inventoryItems[i].transform.GetChild(0).GetComponent<Image>().color = new Color(0, 0, 0, 0);
		}

		//Here We Will Update The UI According To The Equipped Items In Order
		for (byte i = 0; i < s_gameManager._equippedItems.Count; i++)
		{
			s_gameManager._inventoryItems[i].transform.GetChild(0).GetComponent<Image>().sprite = s_gameManager._equippedItems[i].Image;
			s_gameManager._inventoryItems[i].transform.GetChild(0).GetComponent<Image>().color = Color.white;
		}
	}

	public static void CollectSound()
	{
		s_gameManager._dialogueAudio.clip = s_gameManager._collectSound;
		s_gameManager._dialogueAudio.Play();
	}

	public static void ObtainCurrency(byte amount)
	{
		s_gameManager._currency += amount;
		CollectSound();

		UpdateCurrencyUI();
	}

	public static void YesRequest()
	{
		s_gameManager._yesPressed = true;
	}

	public static void NoRequest()
	{
		s_gameManager._noPressed = true;
	}

	public static void SpendCurrency(byte amount)
	{
		if (s_gameManager._currency < amount)
			Debug.Log("Too Poor!");
		else s_gameManager._currency -= amount;
	}

	private static void UpdateCurrencyUI()
	{
		s_gameManager._currencyText.GetComponent<Text>().text = "Currency: " + s_gameManager._currency;
	}

	public static void GiveItem(Item item)
	{
		if (s_gameManager._equippedItems.Count < s_gameManager._inventoryItems.Length)
			s_gameManager._equippedItems.Add(item);

		CollectSound();


		UpdateInventory();
	}

	public static void SellItem(Item item)
	{
		if (s_gameManager._currency >= item.Value && s_gameManager._equippedItems.Count < s_gameManager._inventoryItems.Length)
		{
			s_gameManager._currency -= item.Value;
			s_gameManager._equippedItems.Add(item);
			CollectSound();
			UpdateInventory();
			UpdateCurrencyUI();
		}

		else
			ContinueDialogue(s_gameManager._issueDialogue);
	}

	public static Item GetActiveItem()
	{
		return s_gameManager._equippedItems[s_gameManager._activeItem];
	}

	public static void RemoveActiveItem()
	{
		s_gameManager._equippedItems.RemoveAt(s_gameManager._activeItem);
		SetActiveItem(s_gameManager._activeItem);
		UpdateInventory();
	}

	public static void SetActiveItem(byte item)
	{
		//First We Clamp The Value To Avoid Any Errors
		item = (byte) Mathf.Clamp(item, 0, s_gameManager._equippedItems.Count - 1);

		//Here We Will Reset The Colour Of The Previous Active Item's Box & Set The New Item Box
		s_gameManager._inventoryItems[s_gameManager._activeItem].GetComponent<Image>().color = Color.white;
		s_gameManager._inventoryItems[item].GetComponent<Image>().color = new Color(.8f, .8f, .8f);

		//Now We Just Change The Active Item
		s_gameManager._activeItem = item;
	}

	public static void SetObjective(string objective)
	{
		s_gameManager._objectiveText.GetComponent<Text>().text = "Current Objective: " + objective;
	}

	public static void ShowDialogue(Dialogue dialogue, NPC character)
	{
		s_gameManager._textCoroutine = s_gameManager.StartCoroutine(s_gameManager.DisplayTextCoroutine(dialogue, character));
	}

	public static void ContinueDialogue(Dialogue dialogue)
	{
		s_gameManager.StopCoroutine(s_gameManager._textCoroutine);

		s_gameManager._textCoroutine = s_gameManager.StartCoroutine(s_gameManager.DisplayTextCoroutine(dialogue, null));
	}

	public static void GiveControl()
	{
		s_gameManager._hasControl = true;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public static void TakeControl()
	{
		Cursor.lockState = CursorLockMode.None;
		s_gameManager._hasControl = false;
	}

	public static bool HasControl()
	{
		return s_gameManager._hasControl;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && _textCoroutine == null)
			TogglePause();
	}

	//This Is Responsible For Pausing The Game, When The Game Is Paused The Player Won't Be Able To Control Their Character
	public static void TogglePause()
	{
		s_gameManager._animator.SetTrigger("Toggle Pause Menu");

		s_gameManager._paused = s_gameManager._paused ? false : true;
		Time.timeScale = s_gameManager._paused ? 0 : 1;

		if (!s_gameManager._paused)
			GiveControl();
		else TakeControl();
	}

	public static void QuitGame()
	{
		Debug.Log("Game Has Ended");
		Application.Quit();
	}

	//This Will Save The Game By Converting The Save Manager Data To A Json File
	public static void SaveGame()
	{
		Debug.Log("Saving Data...");

		s_gameManager._saveManager.PlayerPosition = s_player.transform.position;
		s_gameManager._saveManager.PlayerInventory = s_gameManager._equippedItems.ToArray();
		s_gameManager._saveManager.PlayerCurrency = s_gameManager._currency;

		string jsonData = JsonUtility.ToJson(s_gameManager._saveManager);
		s_gameManager.StartCoroutine(s_gameManager.PostRequest("samuelscerrig1.pythonanywhere.com/api/savedata", jsonData));
	}

	//A Helper Function For Readability
	public static void LoadGame()
	{
		s_gameManager.StartCoroutine(s_gameManager.GetRequest("samuelscerrig1.pythonanywhere.com/api/getdata"));
	}

	public static void ContinueRequest()
	{
		s_gameManager._continueRequest = true;
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

	
		if (_saveManager.PlayerPosition != null)
			s_player.transform.position = _saveManager.PlayerPosition;

		if (_saveManager.PlayerInventory != null)
			s_gameManager._equippedItems = new List<Item>(_saveManager.PlayerInventory);

		s_gameManager._currency = _saveManager.PlayerCurrency;

		ObtainCurrency(0);
		UpdateCurrencyUI();
		UpdateInventory();
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
				yield return new WaitUntil(() => _yesPressed || _noPressed);

				if (_yesPressed)
				{
					section.Event.Action.Invoke();
					//Reset Behaviours
					_yesPressed = false;
					_noPressed = false;

					yield return new WaitForSeconds(1);
					s_gameManager._dialogueAudio.clip = s_gameManager._textSound;
				}
			}

			else
			{
				//Stop Entire Coroutine Until The User Has Pressed The Space Button
				yield return new WaitUntil(() => Input.GetKeyDown("space") || _continueRequest);
				section.Event.Action.Invoke();

				if (section.Event.Action.GetPersistentEventCount() > 0)
				{
					yield return new WaitForSeconds(1);
					s_gameManager._dialogueAudio.clip = s_gameManager._textSound;
				}
			}

							//Reset Behaviours
			_yesPressed = false;
			_noPressed = false;
			_continueRequest = false;
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

		_yesPressed = false;
		_noPressed = false;
		_continueRequest = false;
	}

	public void SetHealth(byte health)
	{
		_healthText.GetComponent<Text>().text = "Health: " + health.ToString();
	}
}
