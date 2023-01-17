using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
	private SpriteRenderer _equippedItemSprite;
	private AudioSource _source;

	[SerializeField]
	private AudioClip _consumeClip;

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

	private void Start()
	{
		_equippedItemSprite = transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();
		_source = GetComponents<AudioSource>()[1];

		_equippedItemSprite.sprite = GameManager.GetActiveItem().Image;
	}

	private void Update()
	{
		if (GameManager.HasControl())
		{
			for (byte i = 0; i < _codes.Length; i++)
				if (Input.GetKeyDown(_codes[i]))
				{
					GameManager.SetActiveItem(i);
					_equippedItemSprite.sprite = GameManager.GetActiveItem().Image;
				}

			if (Input.GetMouseButtonDown(0) && !GameManager.GetActiveItem().IsWeapon)
			{
				GetComponent<HealthManager>().GainHealth(GameManager.GetActiveItem().Strength);
				GameManager.RemoveActiveItem();
				_equippedItemSprite.sprite = GameManager.GetActiveItem().Image;

				_source.clip = _consumeClip;
				_source.Play();
			}

			else GetComponent<PlayerAttack>().CheckAttack(GameManager.GetActiveItem().Strength);
		}
	}
}
