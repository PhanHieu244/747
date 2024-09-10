using UnityEngine;

namespace Polyperfect.Common
{
	public class Common_RandomInt : StateMachineBehaviour
	{
		public string intparameterName;

		public Vector2 minMax;

		public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
		{
			animator.SetInteger(intparameterName, 0);
			animator.SetInteger(intparameterName, (int)Random.Range(minMax.x, minMax.y));
		}
	}
}
