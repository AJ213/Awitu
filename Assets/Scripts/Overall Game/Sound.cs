using UnityEngine.Audio;
using UnityEngine;

[System.Serializable]
public class Sound 
{

	public string name;

	public AudioClip clip;

	[Range(0f, 1f)]
	public float volume = .75f;
	[Range(0f, 1f)]
	public float volumeVariance = .1f;

	[Range(.1f, 3f)]
	public float pitch = 1f;
	[Range(0f, 1f)]
	public float pitchVariance = .1f;
	[Range(0, 256)]
	public int priority = 128;

	public bool loop = false;

	[SerializeField] AudioMixerGroup mixerGroup;

	[HideInInspector]
	public AudioSource source;

}
