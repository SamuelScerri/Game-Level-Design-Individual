using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
	[SerializeField]
	private byte _currencyAmount;

	[SerializeField]
	private Item _itemToGive;

	private void Update()
	{
		transform.Rotate(Vector2.up * Time.deltaTime * 256);
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (collider.gameObject.tag == "Player")
		{
			if (_itemToGive != null)
			{
				GameManager.GiveItem(_itemToGive);
				GameManager.ObtainCurrency(_itemToGive.Value);
			}

			else GameManager.ObtainCurrency(_currencyAmount);

			Destroy(this.gameObject);
		}
	}
}
