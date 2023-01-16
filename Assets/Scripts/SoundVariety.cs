using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SoundVariety
{
	public AudioClip[] Clips;

	public SoundVariety(AudioClip[] clips)
	{
		Clips = clips;
	}

	public AudioClip GetRandomSoundVariation()
	{
		return Clips[Random.Range(0, Clips.Length)];
	}
}
