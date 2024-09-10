using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_AudioManager : MonoBehaviour
	{
		private static Common_AudioManager instance;

		[SerializeField]
		private bool muteSound;

		[SerializeField]
		private int objectPoolLength = 20;

		[SerializeField]
		private float soundDistance = 7f;

		[SerializeField]
		private bool logSounds;

		private List<AudioSource> pool = new List<AudioSource>();

		private void Awake()
		{
			instance = this;
			for (int i = 0; i < objectPoolLength; i++)
			{
				GameObject obj = new GameObject();
				obj.transform.SetParent(instance.transform);
				obj.name = "Sound Effect";
				AudioSource audioSource = obj.AddComponent<AudioSource>();
				audioSource.spatialBlend = 1f;
				audioSource.minDistance = instance.soundDistance;
				audioSource.gameObject.SetActive(false);
				pool.Add(audioSource);
			}
		}

		public static void PlaySound(AudioClip clip, Vector3 pos)
		{
			if (!instance)
			{
				Debug.LogError("No Audio Manager found in the scene.");
			}
			else
			{
				if (instance.muteSound)
				{
					return;
				}
				if (!clip)
				{
					Debug.LogError("Clip is null");
					return;
				}
				if (instance.logSounds)
				{
					Debug.Log("Playing Audio: " + clip.name);
				}
				for (int i = 0; i < instance.pool.Count; i++)
				{
					if (!instance.pool[i].gameObject.activeInHierarchy)
					{
						instance.pool[i].clip = clip;
						instance.pool[i].transform.position = pos;
						instance.pool[i].gameObject.SetActive(true);
						instance.pool[i].Play();
						instance.StartCoroutine(instance.ReturnToPool(instance.pool[i].gameObject, clip.length));
						return;
					}
				}
				GameObject gameObject = new GameObject();
				gameObject.transform.SetParent(instance.transform);
				gameObject.name = "Sound Effect";
				AudioSource audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.spatialBlend = 1f;
				audioSource.minDistance = instance.soundDistance;
				instance.pool.Add(audioSource);
				audioSource.clip = clip;
				gameObject.transform.position = pos;
				audioSource.Play();
				instance.StartCoroutine(instance.ReturnToPool(gameObject, clip.length));
			}
		}

		private IEnumerator ReturnToPool(GameObject obj, float delay)
		{
			yield return new WaitForSeconds(delay);
			obj.SetActive(false);
		}
	}
}
