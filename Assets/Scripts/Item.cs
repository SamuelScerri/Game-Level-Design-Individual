using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Character/Create Item", order = 1)]
public class Item : ScriptableObject
{
	public Sprite Image;

	public bool IsWeapon;
	public byte Strength;

	public Item(Sprite image, bool isWeapon, byte strength)
	{
		IsWeapon = isWeapon;
		Strength = strength;
		Image = image;
	}
}
