using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
	//This Is Used For Better Readibility In Later Code, A Simple Number Press Will Correspond To The Active Item
	private KeyCode[] _codes = new KeyCode[]
	{
		KeyCode.Alpha1,
		KeyCode.Alpha2,
		KeyCode.Alpha3,
		KeyCode.Alpha4,
		KeyCode.Alpha5,
		KeyCode.Alpha6,
		KeyCode.Alpha7,
	};

	private void Update()
	{
		for (byte i = 0; i < _codes.Length; i++)
			if (Input.GetKeyDown(_codes[i]))
				GameManager.s_gameManager.SetActiveItem(i);
	}
}
