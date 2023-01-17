using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;

[CreateAssetMenu(fileName = "Data", menuName = "Character/Create Item", order = 1)]
[Serializable]
public class Item : ScriptableObject
{
	public Sprite Image;

	public bool IsWeapon;
	public byte Strength;
	public byte Value;

	public Item(Sprite image, bool isWeapon, byte strength, byte newValue)
	{
		IsWeapon = isWeapon;
		Strength = strength;
		Image = image;
		Value = newValue;
	}
}