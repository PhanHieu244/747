using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_AnimSpeed : MonoBehaviour
	{
		private Animator anim;

		private float Speed;

		private void Start()
		{
			Speed = Random.Range(0.85f, 1.25f);
			anim = GetComponent<Animator>();
		}

		private void Update()
		{
			anim.speed = Speed;
		}
	}
}
