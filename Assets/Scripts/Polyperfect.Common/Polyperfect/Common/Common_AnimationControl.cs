using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_AnimationControl : MonoBehaviour
	{
		private string currentAnimation = "";

		private void Start()
		{
		}

		private void Update()
		{
		}

		public void SetAnimation(string animationName)
		{
			if (currentAnimation != "")
			{
				GetComponent<Animator>().SetBool(currentAnimation, false);
			}
			GetComponent<Animator>().SetBool(animationName, true);
			currentAnimation = animationName;
		}

		public void SetAnimationIdle()
		{
			if (currentAnimation != "")
			{
				GetComponent<Animator>().SetBool(currentAnimation, false);
			}
		}

		public void SetDeathAnimation(int numOfClips)
		{
			int num = Random.Range(0, numOfClips);
			string text = "Death";
			Debug.Log(num);
			GetComponent<Animator>().SetInteger(text, num);
		}
	}
}
