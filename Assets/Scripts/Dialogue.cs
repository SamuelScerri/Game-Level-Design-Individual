using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Data", menuName = "Character/Create Dialogue", order = 1)]
public class Dialogue : ScriptableObject
{
	public DialogueSection[] DialogueSections;

	public Dialogue(DialogueSection[] dialogueSections)
	{
		DialogueSections = dialogueSections;
	}
}

[System.Serializable]
public class DialogueSection
{
	public string Speech;
	public ActionEvent Event;

	public DialogueSection(string speech, ActionEvent newEvent)
	{
		Speech = speech;
		newEvent = Event;
	}
}

[System.Serializable]
public class ActionEvent
{
	public bool Question;
	public UnityEvent Action;

	public ActionEvent(bool question, UnityEvent action)
	{
		Question = question;
		Action = action;
	}
}