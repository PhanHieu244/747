using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_KillSwitch : MonoBehaviour
	{
		private Animator anim;

		private void Start()
		{
			anim = GetComponent<Animator>();
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				anim.SetBool("isDead", true);
			}
		}
	}
}
