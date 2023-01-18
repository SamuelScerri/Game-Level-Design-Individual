using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "Data", menuName = "Character/Create Item", order = 1)]
public class Item : ScriptableObject
{
	public Sprite Image;

	public bool IsWeapon, IsCraftable;
	public int Strength;
	public byte Value;
	public byte HandsNeeded;

	public Item(Sprite image, bool isCraftable, bool isWeapon, byte strength, byte newValue, byte handsNeeded)
	{
		IsWeapon = isWeapon;
		IsCraftable = isCraftable;
		Strength = strength;
		Image = image;
		Value = newValue;
		HandsNeeded = handsNeeded;
	}
}