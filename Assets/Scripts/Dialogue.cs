using UnityEngine;

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

	public DialogueSection(string speech)
	{
		Speech = speech;
	}
}