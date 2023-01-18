using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
	public void StartGame()
	{
		SceneManager.LoadScene(1);
	}

	public void LoadGame()
	{
		GameManager.s_loadWithData = true;
		StartGame();
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
