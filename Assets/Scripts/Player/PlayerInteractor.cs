using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
	[SerializeField]
	private byte _interactionRadius;
	private GameObject _nearestNPC;
	private GameObject _nearestInteractable;

	private void Update()
	{
		//We Will Only Be Able To Interact If The Player Has Control
		if (GameManager.HasControl())
			Interact();
	}

	private void Interact()
	{
		//This Will Ensure That There Will Be No Null Exception Error
		if (Input.GetKeyDown("space") && _nearestNPC != null)
		{
			//Here We Set The Desired Rotation Of The Player To Look At The NPC
			Vector3 lookPosition = _nearestNPC.transform.position - transform.position;
			GetComponent<PlayerMovement>().SetPlayerRotation(Quaternion.LookRotation(lookPosition).eulerAngles.y);

			//The NPC Will Then Proceed To Talk To Us
			_nearestNPC.GetComponent<NPC>().TalkTo(gameObject);
		}

		else if (Input.GetKeyDown("space") && _nearestInteractable != null)
		{
			//The NPC Will Then Proceed To Talk To Us
			_nearestInteractable.GetComponent<Animation>().Play();
		}


		//Here We Get The Nearest NPC So That The Player Can Interact With
		_nearestNPC = GetNearestGameObject(GameObject.FindGameObjectsWithTag("NPC"));

		//Here We Get The Nearest Interactable So That The Player Can Interact With
		_nearestInteractable = GetNearestGameObject(GameObject.FindGameObjectsWithTag("Interactable"));
	}

	//This Function Will Get The Nearest Game Object That Is Closest To The Player
	private GameObject GetNearestGameObject(GameObject[] objects)
	{
		GameObject closestGameObject = null;
		float closestDistance = 0;

		//We Iterate Over Every Object Given
		foreach (GameObject currentObject in objects)
		{
			float distance = Vector3.Distance(transform.position, currentObject.transform.position);

			//If This Object Is Closer, Then This Is The Closest Object
			if (closestDistance == 0 || distance < closestDistance)
				if (distance < _interactionRadius)
				{
					closestGameObject = currentObject;
					closestDistance = distance;
				}
		}

		//We Will Then Return The Closest Game Object
		return closestGameObject;
	}
}
