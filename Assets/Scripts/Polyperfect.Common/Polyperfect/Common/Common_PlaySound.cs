using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_PlaySound : MonoBehaviour
	{
		[SerializeField]
		private AudioClip animalSound;

		[SerializeField]
		private AudioClip walking;

		[SerializeField]
		private AudioClip eating;

		[SerializeField]
		private AudioClip running;

		[SerializeField]
		private AudioClip attacking;

		[SerializeField]
		private AudioClip death;

		[SerializeField]
		private AudioClip sleeping;

		private void AnimalSound()
		{
			if ((bool)animalSound)
			{
				Common_AudioManager.PlaySound(animalSound, base.transform.position);
			}
		}

		private void Walking()
		{
			if ((bool)walking)
			{
				Common_AudioManager.PlaySound(walking, base.transform.position);
			}
		}

		private void Eating()
		{
			if ((bool)eating)
			{
				Common_AudioManager.PlaySound(eating, base.transform.position);
			}
		}

		private void Running()
		{
			if ((bool)running)
			{
				Common_AudioManager.PlaySound(running, base.transform.position);
			}
		}

		private void Attacking()
		{
			if ((bool)attacking)
			{
				Common_AudioManager.PlaySound(attacking, base.transform.position);
			}
		}

		private void Death()
		{
			if ((bool)death)
			{
				Common_AudioManager.PlaySound(death, base.transform.position);
			}
		}

		private void Sleeping()
		{
			if ((bool)sleeping)
			{
				Common_AudioManager.PlaySound(sleeping, base.transform.position);
			}
		}
	}
}
