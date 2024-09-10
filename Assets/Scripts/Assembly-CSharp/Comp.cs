using UnityEngine;

public class Comp : Creature
{
	public Transform Spine0;

	public Transform Spine1;

	public Transform Spine2;

	public Transform Spine3;

	public Transform Spine4;

	public Transform Spine5;

	public Transform Neck0;

	public Transform Neck1;

	public Transform Neck2;

	public Transform Neck3;

	public Transform Tail0;

	public Transform Tail1;

	public Transform Tail2;

	public Transform Tail3;

	public Transform Tail4;

	public Transform Tail5;

	public Transform Tail6;

	public Transform Tail7;

	public Transform Tail8;

	public Transform Arm1;

	public Transform Arm2;

	public Transform Left_Hips;

	public Transform Right_Hips;

	public Transform Left_Leg;

	public Transform Right_Leg;

	public Transform Left_Foot0;

	public Transform Right_Foot0;

	public AudioClip Waterflush;

	public AudioClip Hit_jaw;

	public AudioClip Hit_head;

	public AudioClip Hit_tail;

	public AudioClip Smallstep;

	public AudioClip Smallsplash;

	public AudioClip Bite;

	public AudioClip Comp1;

	public AudioClip Comp2;

	public AudioClip Comp3;

	public AudioClip Comp4;

	public AudioClip Comp5;

	private Vector3 dir = Vector3.zero;

	private void OnCollisionStay(Collision col)
	{
		int num = Random.Range(0, 4);
		AudioClip pain = null;
		switch (num)
		{
		case 0:
			pain = Comp1;
			break;
		case 1:
			pain = Comp2;
			break;
		case 2:
			pain = Comp3;
			break;
		case 3:
			pain = Comp4;
			break;
		}
		ManageCollision(col, Pitch_Max, Crouch_Max, source, pain, Hit_jaw, Hit_head, Hit_tail);
	}

	private void PlaySound(string name, int time)
	{
		if ((float)time != currframe || lastframe == currframe)
		{
			return;
		}
		switch (name)
		{
		case "Step":
			source[1].pitch = Random.Range(0.75f, 1.25f);
			if (IsInWater)
			{
				source[1].PlayOneShot(Waterflush, Random.Range(0.25f, 0.5f));
			}
			else if (IsOnWater)
			{
				source[1].PlayOneShot(Smallsplash, Random.Range(0.25f, 0.5f));
			}
			else if (IsOnGround)
			{
				source[1].PlayOneShot(Smallstep, Random.Range(0.25f, 0.5f));
			}
			lastframe = currframe;
			break;
		case "Bite":
			source[1].pitch = Random.Range(1f, 1.25f);
			source[1].PlayOneShot(Bite, 0.5f);
			lastframe = currframe;
			break;
		case "Die":
			source[1].pitch = Random.Range(0.8f, 1f);
			source[1].PlayOneShot((IsOnWater | IsInWater) ? Smallsplash : Smallstep, 1f);
			lastframe = currframe;
			IsDead = true;
			break;
		case "Call":
			source[0].pitch = Random.Range(1f, 1.25f);
			source[0].PlayOneShot(Comp4, 1f);
			lastframe = currframe;
			break;
		case "Atk":
		{
			int num2 = Random.Range(0, 3);
			source[0].pitch = Random.Range(1f, 1.75f);
			switch (num2)
			{
			case 0:
				source[0].PlayOneShot(Comp2, 1f);
				break;
			case 1:
				source[0].PlayOneShot(Comp3, 1f);
				break;
			case 2:
				source[0].PlayOneShot(Comp5, 1f);
				break;
			}
			lastframe = currframe;
			break;
		}
		case "Growl":
		{
			int num = Random.Range(0, 5);
			source[0].pitch = Random.Range(1f, 1.75f);
			switch (num)
			{
			case 0:
				source[0].PlayOneShot(Comp1, 1f);
				break;
			case 1:
				source[0].PlayOneShot(Comp2, 1f);
				break;
			case 2:
				source[0].PlayOneShot(Comp3, 1f);
				break;
			case 3:
				source[0].PlayOneShot(Comp4, 1f);
				break;
			case 4:
				source[0].PlayOneShot(Comp5, 1f);
				break;
			}
			lastframe = currframe;
			break;
		}
		}
	}

	private void FixedUpdate()
	{
		StatusUpdate();
		if (!IsActive | (AnimSpeed == 0f))
		{
			body.Sleep();
			return;
		}
		OnReset = false;
		OnAttack = false;
		IsConstrained = false;
		if (UseAI && Health != 0f)
		{
			AICore(1, 2, 3, 4, 5, 6, 7);
		}
		else if (Health != 0f)
		{
			GetUserInputs(1, 2, 3, 4, 5, 6, 7);
		}
		else
		{
			anm.SetBool("Attack", false);
			anm.SetInteger("Move", 0);
			anm.SetInteger("Idle", -1);
		}
		if (IsOnGround | IsInWater | IsOnWater)
		{
			if (!IsOnGround)
			{
				body.drag = 1f;
				body.angularDrag = 1f;
			}
			else
			{
				body.drag = 4f;
				body.angularDrag = 4f;
			}
			ApplyYPos();
			anm.SetBool("OnGround", true);
			dir = new Vector3(base.transform.forward.x, 0f, base.transform.forward.z);
		}
		else
		{
			ApplyGravity(0.5f);
			anm.SetBool("OnGround", false);
		}
		if (OnAnm.IsName("Comp|IdleA") | OnAnm.IsName("Comp|Die"))
		{
			Move(Vector3.zero);
			if (OnAnm.IsName("Comp|Die"))
			{
				OnReset = true;
				if (!IsDead)
				{
					PlaySound("AtkB", 2);
					PlaySound("Die", 12);
				}
			}
		}
		else if (OnAnm.IsName("Comp|IdleJumpStart") | OnAnm.IsName("Comp|RunJumpStart") | OnAnm.IsName("Comp|JumpIdle") | OnAnm.IsName("Comp|IdleJumpEnd") | OnAnm.IsName("Comp|RunJumpEnd") | OnAnm.IsName("Comp|JumpAtk"))
		{
			if (OnAnm.IsName("Comp|IdleJumpStart") | OnAnm.IsName("Comp|RunJumpStart"))
			{
				if ((double)OnAnm.normalizedTime > 0.4)
				{
					Move(Vector3.up, 1.5f, true);
				}
				else
				{
					OnJump = true;
				}
				if (anm.GetInteger("Move").Equals(2))
				{
					Move(dir, 80f);
				}
				else if (anm.GetInteger("Move").Equals(1))
				{
					Move(dir, 32f);
				}
				PlaySound("Step", 1);
				PlaySound("Step", 2);
			}
			else if (OnAnm.IsName("Comp|IdleJumpEnd") | OnAnm.IsName("Comp|RunJumpEnd"))
			{
				if (OnAnm.IsName("Comp|RunJumpEnd"))
				{
					Move(dir, 80f);
				}
				body.velocity = new Vector3(body.velocity.x, 0f, body.velocity.z);
				OnJump = false;
				PlaySound("Step", 3);
				PlaySound("Step", 4);
			}
			else if (OnAnm.IsName("Comp|JumpAtk"))
			{
				if (anm.GetInteger("Move").Equals(1) | anm.GetInteger("Move").Equals(2))
				{
					Move(Vector3.Lerp(dir, Vector3.zero, 0.5f), 80f);
				}
				OnAttack = true;
				PlaySound("AtkB", 1);
				PlaySound("Bite", 9);
				body.velocity = new Vector3(body.velocity.x, (body.velocity.y > 0f) ? body.velocity.y : 0f, body.velocity.z);
			}
			else if (!anm.GetInteger("Move").Equals(0))
			{
				Move(Vector3.Lerp(dir, Vector3.zero, 0.5f), 80f);
			}
		}
		else if (OnAnm.IsName("Comp|Walk"))
		{
			roll = Mathf.Clamp(Mathf.Lerp(roll, spineX * 15f, 0.1f), -30f, 30f);
			Move(base.transform.forward, 20f);
			PlaySound("Step", 8);
			PlaySound("Step", 9);
		}
		else if (OnAnm.IsName("Comp|Run") | OnAnm.IsName("Comp|RunGrowl") | OnAnm.IsName("Comp|RunAtk1") | (OnAnm.IsName("Comp|RunAtk2") && (double)OnAnm.normalizedTime < 0.9) | (OnAnm.IsName("Comp|IdleAtk3") && (double)OnAnm.normalizedTime > 0.5 && (double)OnAnm.normalizedTime < 0.9))
		{
			roll = Mathf.Clamp(Mathf.Lerp(roll, spineX * 15f, 0.1f), -30f, 30f);
			Move(base.transform.forward, 80f);
			if (OnAnm.IsName("Comp|Run"))
			{
				PlaySound("Step", 4);
				PlaySound("Step", 12);
			}
			else if (OnAnm.IsName("Comp|RunGrowl"))
			{
				PlaySound("Atk", 2);
				PlaySound("Step", 4);
				PlaySound("Step", 12);
			}
			else if (OnAnm.IsName("Comp|RunAtk1"))
			{
				OnAttack = true;
				PlaySound("Atk", 2);
				PlaySound("Step", 4);
				PlaySound("Step", 12);
			}
			else if (OnAnm.IsName("Comp|RunAtk2") | OnAnm.IsName("Comp|IdleAtk3"))
			{
				OnAttack = true;
				PlaySound("Atk", 2);
				PlaySound("Step", 4);
				PlaySound("Bite", 9);
				PlaySound("Step", 12);
			}
		}
		else if (OnAnm.IsName("Comp|Walk-"))
		{
			Move(-base.transform.forward, 16f);
			PlaySound("Step", 8);
			PlaySound("Step", 9);
		}
		else if (OnAnm.IsName("Comp|Strafe-"))
		{
			Move(base.transform.right, 16f);
			PlaySound("Step", 8);
			PlaySound("Step", 9);
		}
		else if (OnAnm.IsName("Comp|Strafe+"))
		{
			Move(-base.transform.right, 16f);
			PlaySound("Step", 8);
			PlaySound("Step", 9);
		}
		else if (OnAnm.IsName("Comp|IdleAtk3"))
		{
			OnAttack = true;
			Move(Vector3.zero);
			PlaySound("Atk", 1);
		}
		else if (OnAnm.IsName("Comp|GroundAtk"))
		{
			OnAttack = true;
			PlaySound("Atk", 2);
			PlaySound("Bite", 4);
		}
		else if (OnAnm.IsName("Comp|IdleAtk1") | OnAnm.IsName("Comp|IdleAtk2"))
		{
			OnAttack = true;
			Move(Vector3.zero);
			PlaySound("Atk", 2);
			PlaySound("Bite", 9);
		}
		else if (OnAnm.IsName("Comp|ToSleep"))
		{
			OnReset = true;
			IsConstrained = true;
		}
		else if (OnAnm.IsName("Comp|Sleep"))
		{
			OnReset = true;
			IsConstrained = true;
			PlaySound("Repose", 1);
		}
		else if (OnAnm.IsName("Comp|EatA"))
		{
			OnReset = true;
			IsConstrained = true;
		}
		else if (OnAnm.IsName("Comp|EatB"))
		{
			OnReset = true;
			IsConstrained = true;
			PlaySound("Bite", 3);
		}
		else if (OnAnm.IsName("Comp|EatC"))
		{
			OnReset = true;
		}
		else if (OnAnm.IsName("Comp|IdleB"))
		{
			OnReset = true;
			PlaySound("Atk", 1);
		}
		else if (OnAnm.IsName("Comp|IdleC"))
		{
			OnReset = true;
			IsConstrained = true;
			PlaySound("Step", 2);
		}
		else if (OnAnm.IsName("Comp|IdleD"))
		{
			PlaySound("Growl", 1);
		}
		else if (OnAnm.IsName("Comp|IdleE"))
		{
			PlaySound("Call", 1);
			PlaySound("Call", 4);
			PlaySound("Call", 8);
		}
		else if (OnAnm.IsName("Comp|Die-"))
		{
			OnReset = true;
			PlaySound("Atk", 1);
			IsDead = false;
		}
		RotateBone(IkType.SmBiped, 60f);
	}

	private void LateUpdate()
	{
		if (IsActive)
		{
			HeadPos = Head.GetChild(0).GetChild(0).position;
			float y = (0f - headY) * headX / Yaw_Max;
			Spine0.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Spine1.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Spine2.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Spine3.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Spine4.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Spine5.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Arm1.rotation *= Quaternion.Euler(headY * 8f, 0f, 0f);
			Arm2.rotation *= Quaternion.Euler(0f, headY * 8f, 0f);
			Neck0.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Neck1.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Neck2.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Neck3.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Head.rotation *= Quaternion.Euler(0f - headY, y, headX);
			Tail0.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail1.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail2.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail3.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail4.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail5.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail6.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail7.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Tail8.rotation *= Quaternion.Euler(0f, 0f, 0f - spineX);
			Right_Hips.rotation *= Quaternion.Euler(0f - roll, 0f, 0f);
			Left_Hips.rotation *= Quaternion.Euler(0f, roll, 0f);
			if (!IsDead)
			{
				Head.GetChild(0).transform.rotation *= Quaternion.Euler(lastHit, 0f, 0f);
			}
			GetGroundPos(IkType.SmBiped, Right_Hips, Right_Leg, Right_Foot0, Left_Hips, Left_Leg, Left_Foot0);
		}
	}
}
