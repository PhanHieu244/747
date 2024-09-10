using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
	public static AudioManager instance;

	public AudioMixerGroup mixerGroup;

	public Sound[] sounds;

	private void Awake()
	{
		if (instance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		else
		{
			instance = this;
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}
		Sound[] array = sounds;
		foreach (Sound sound in array)
		{
			sound.source = base.gameObject.AddComponent<AudioSource>();
			sound.source.clip = sound.clip;
			sound.source.loop = sound.loop;
			sound.source.outputAudioMixerGroup = mixerGroup;
		}
	}

	private void Start()
	{
	}

	public void Play(string sound)
	{
		Sound sound2 = Array.Find(sounds, (Sound item) => item.name == sound);
		if (sound2 == null)
		{
			Debug.LogWarning("Sound: " + base.name + " not found!");
			return;
		}
		sound2.source.volume = sound2.volume * (1f + UnityEngine.Random.Range((0f - sound2.volumeVariance) / 2f, sound2.volumeVariance / 2f));
		sound2.source.pitch = sound2.pitch * (1f + UnityEngine.Random.Range((0f - sound2.pitchVariance) / 2f, sound2.pitchVariance / 2f));
		sound2.source.Play();
	}

	public void Pause(string sound)
	{
		Sound sound2 = Array.Find(sounds, (Sound item) => item.name == sound);
		if (sound2 == null)
		{
			Debug.LogWarning("Sound: " + base.name + " not found!");
			return;
		}
		sound2.source.volume = sound2.volume * (1f + UnityEngine.Random.Range((0f - sound2.volumeVariance) / 2f, sound2.volumeVariance / 2f));
		sound2.source.pitch = sound2.pitch * (1f + UnityEngine.Random.Range((0f - sound2.pitchVariance) / 2f, sound2.pitchVariance / 2f));
		sound2.source.Pause();
	}
}
