using System;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour
{
	[HideInInspector]
	public enum PathType
	{
		Walk,
		Run
	}

	[HideInInspector]
	public enum TargetAction
	{
		None,
		Sleep,
		Eat,
		Drink
	}

	[Serializable]
	public struct _PathEditor
	{
		[Tooltip("Place your waypoint gameobject in a reacheable position.\nDon't put a waypoint in air if the creature are not able to fly")]
		public GameObject _Waypoint;

		public PathType _PathType;

		public TargetAction _TargetAction;

		[Tooltip("Using a priority of 100% will disable all autonomous AI for this waypoint\nObstacle avoid AI and custom targets search still enabled")]
		[Range(1f, 100f)]
		public int _Priority;

		public _PathEditor(GameObject Waypoint, PathType PathType, TargetAction TargetAction, int Priority)
		{
			_Waypoint = Waypoint;
			_PathType = PathType;
			_TargetAction = TargetAction;
			_Priority = Priority;
		}
	}

	[HideInInspector]
	public enum TargetType
	{
		Enemy,
		Friend
	}

	[Serializable]
	public struct _TargetEditor
	{
		public GameObject _GameObject;

		public TargetType _TargetType;

		[Tooltip("If MaxRange is zero, range is infinite. \nCreature will start his attack/tracking once in range.")]
		public int MaxRange;
	}

	public enum Skin
	{
		SkinA,
		SkinB,
		SkinC
	}

	public enum Eyes
	{
		Type0,
		Type1,
		Type2,
		Type3,
		Type4,
		Type5,
		Type6,
		Type7,
		Type8,
		Type9,
		Type10,
		Type11,
		Type12,
		Type13,
		Type14,
		Type15
	}

	public enum IkType
	{
		None,
		Convex,
		Quad,
		Flying,
		SmBiped,
		LgBiped
	}

	[Space(10f)]
	[Header("ARTIFICIAL INTELLIGENCE")]
	public bool UseAI;

	private const string PathHelp = "Use gameobjects as waypoints to define a path for this creature by \ntaking into account the priority between autonomous AI and its path.";

	private const string WaypointHelp = "Place your waypoint gameobject in a reacheable position.\nDon't put a waypoint in air if the creature are not able to fly";

	private const string PriorityPathHelp = "Using a priority of 100% will disable all autonomous AI for this waypoint\nObstacle avoid AI and custom targets search still enabled";

	private const string TargetHelp = "Use gameobjects to assign a custom enemy/friend for this creature\nCan be any kind of gameobject e.g : player, other creature.\nThe creature will include friend/enemy goals in its search. \nEnemy: triggered if the target is in range. \nFriend: triggered when the target moves away.";

	private const string MaxRangeHelp = "If MaxRange is zero, range is infinite. \nCreature will start his attack/tracking once in range.";

	[Space(10f)]
	[Tooltip("Use gameobjects as waypoints to define a path for this creature by \ntaking into account the priority between autonomous AI and its path.")]
	public List<_PathEditor> PathEditor;

	[HideInInspector]
	public int nextPath;

	[Space(10f)]
	[Tooltip("Use gameobjects to assign a custom enemy/friend for this creature\nCan be any kind of gameobject e.g : player, other creature.\nThe creature will include friend/enemy goals in its search. \nEnemy: triggered if the target is in range. \nFriend: triggered when the target moves away.")]
	public List<_TargetEditor> TargetEditor;

	[Space(10f)]
	[Header("CREATURE SETTINGS")]
	public Skin bodyTexture;

	public Eyes eyesTexture;

	[Space(5f)]
	[Range(0f, 100f)]
	public float Health = 100f;

	[Range(0f, 100f)]
	public float Water = 100f;

	[Range(0f, 100f)]
	public float Food = 100f;

	[Range(0f, 100f)]
	public float Stamina = 100f;

	[Space(5f)]
	[Range(1f, 10f)]
	public float DamageMultiplier = 1f;

	[Range(1f, 10f)]
	public float ArmorMultiplier = 1f;

	[Range(0f, 2f)]
	public float AnimSpeed = 1f;

	public bool Herbivorous;

	public bool CanAttack;

	public bool CanHeadAttack;

	public bool CanTailAttack;

	public bool CanWalk;

	public bool CanJump;

	public bool CanFly;

	public bool CanSwim;

	public bool LowAltitude;

	public bool CanInvertBody;

	public float BaseMass = 1f;

	public float Ang_T = 0.025f;

	public float Crouch_Max;

	public float Yaw_Max;

	public float Pitch_Max;

	[Space(20f)]
	[Header("COMPONENTS AND TEXTURES")]
	public Rigidbody body;

	public LODGroup lod;

	public Animator anm;

	public AudioSource[] source;

	public SkinnedMeshRenderer[] rend;

	public Texture[] skin;

	public Texture[] eyes;

	[Space(20f)]
	[Header("TRANSFORMS AND SOUNDS")]
	public Transform Head;

	[HideInInspector]
	public Manager main;

	[HideInInspector]
	public AnimatorStateInfo OnAnm;

	[HideInInspector]
	public bool IsActive;

	[HideInInspector]
	public bool IsVisible;

	[HideInInspector]
	public bool IsDead;

	[HideInInspector]
	public bool IsOnGround;

	[HideInInspector]
	public bool IsOnWater;

	[HideInInspector]
	public bool IsInWater;

	[HideInInspector]
	public bool IsConstrained;

	[HideInInspector]
	public bool IsOnLevitation;

	[HideInInspector]
	public bool OnAttack;

	[HideInInspector]
	public bool OnJump;

	[HideInInspector]
	public bool OnCrouch;

	[HideInInspector]
	public bool OnReset;

	[HideInInspector]
	public bool OnInvert;

	[HideInInspector]
	public bool OnHeadMove;

	[HideInInspector]
	public bool OnAutoLook;

	[HideInInspector]
	public bool OnTailAttack;

	[HideInInspector]
	public int rndX;

	[HideInInspector]
	public int rndY;

	[HideInInspector]
	public int rndMove;

	[HideInInspector]
	public int rndIdle;

	[HideInInspector]
	public int loop;

	[HideInInspector]
	public string behavior;

	[HideInInspector]
	public string specie;

	[HideInInspector]
	public GameObject objTGT;

	[HideInInspector]
	public GameObject objCOL;

	[HideInInspector]
	public Vector3 HeadPos;

	[HideInInspector]
	public Vector3 posCOL = Vector3.zero;

	[HideInInspector]
	public Vector3 posTGT = Vector3.zero;

	[HideInInspector]
	public Vector3 lookTGT = Vector3.zero;

	[HideInInspector]
	public Vector3 boxscale = Vector3.zero;

	[HideInInspector]
	public Vector3 normal = Vector3.zero;

	[HideInInspector]
	public Quaternion angTGT = Quaternion.identity;

	[HideInInspector]
	public Quaternion normAng = Quaternion.identity;

	[HideInInspector]
	public float currframe;

	[HideInInspector]
	public float lastframe;

	[HideInInspector]
	public float lastHit;

	[HideInInspector]
	public float crouch;

	[HideInInspector]
	public float spineX;

	[HideInInspector]
	public float spineY;

	[HideInInspector]
	public float headX;

	[HideInInspector]
	public float headY;

	[HideInInspector]
	public float pitch;

	[HideInInspector]
	public float roll;

	[HideInInspector]
	public float reverse;

	[HideInInspector]
	public float posY;

	[HideInInspector]
	public float waterY;

	[HideInInspector]
	public float withersSize;

	[HideInInspector]
	public float size;

	[HideInInspector]
	public float speed;

	[HideInInspector]
	public float behaviorCount;

	[HideInInspector]
	public float distTGT;

	[HideInInspector]
	public float delta;

	[HideInInspector]
	public float actionDist;

	[HideInInspector]
	public float angleAdd;

	[HideInInspector]
	public float avoidDelta;

	[HideInInspector]
	public float avoidAdd;

	private const int enemyMaxRange = 50;

	private const int waterMaxRange = 200;

	private const int foodMaxRange = 200;

	private const int friendMaxRange = 200;

	private const int preyMaxRange = 200;

	private const int MoveX = 0;

	private const int MoveY = 1;

	private const int Attack = 2;

	private const int Interact = 3;

	private const int Sleep = 4;

	private const int MoveZ = 5;

	private const int Run = 6;

	private const int CamX = 7;

	private const int CamY = 8;

	private const int CamZ = 9;

	private const int Focus = 10;

	private const int Target = 11;

	private const int Map = 12;

	private const int YesNo = 13;

	private const int Menu = 14;

	private Vector3 FR_HIT;

	private Vector3 FL_HIT;

	private Vector3 BR_HIT;

	private Vector3 BL_HIT;

	private Vector3 FR_Norm = Vector3.up;

	private Vector3 FL_Norm = Vector3.up;

	private Vector3 BR_Norm = Vector3.up;

	private Vector3 BL_Norm = Vector3.up;

	private float BR1;

	private float BR2;

	private float BR3;

	private float BR_Add;

	private float BL1;

	private float BL2;

	private float BL3;

	private float BL_Add;

	private float alt1;

	private float alt2;

	private float a1;

	private float a2;

	private float b1;

	private float b2;

	private float c1;

	private float c2;

	private float FR1;

	private float FR2;

	private float FR3;

	private float FR_Add;

	private float FL1;

	private float FL2;

	private float FL3;

	private float FL_Add;

	private float alt3;

	private float alt4;

	private float a3;

	private float a4;

	private float b3;

	private float b4;

	private float c3;

	private float c4;

	private void Start()
	{
		main = Camera.main.transform.GetComponent<Manager>();
		SetScale(base.transform.localScale.x);
		SetMaterials(bodyTexture.GetHashCode(), eyesTexture.GetHashCode());
		loop = UnityEngine.Random.Range(0, 100);
		specie = base.transform.GetChild(0).name;
	}

	public void SetAI(bool UseAI)
	{
		this.UseAI = UseAI;
		if (!this.UseAI)
		{
			posTGT = Vector3.zero;
			objTGT = null;
			objCOL = null;
			behaviorCount = 0f;
		}
	}

	public void SetMaterials(int bodyindex, int eyesindex)
	{
		bodyTexture = (Skin)bodyindex;
		eyesTexture = (Eyes)eyesindex;
		SkinnedMeshRenderer[] array = rend;
		foreach (SkinnedMeshRenderer obj in array)
		{
			obj.materials[0].mainTexture = skin[bodyindex];
			obj.materials[1].mainTexture = eyes[eyesindex];
		}
	}

	public void SetScale(float resize)
	{
		size = resize;
		base.transform.localScale = new Vector3(resize, resize, resize);
		body.mass = BaseMass * size;
		withersSize = (base.transform.GetChild(0).GetChild(0).position - base.transform.position).magnitude;
		boxscale = rend[0].bounds.extents;
		source[0].maxDistance = Mathf.Lerp(50f, 300f, size);
		source[1].maxDistance = Mathf.Lerp(50f, 150f, size);
	}

	public void StatusUpdate()
	{
		IsVisible = false;
		SkinnedMeshRenderer[] array = rend;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].isVisible)
			{
				IsVisible = true;
			}
		}
		if (!main.RealtimeGame)
		{
			float magnitude = (main.transform.position - base.transform.position).magnitude;
			if (!IsVisible && magnitude > 100f)
			{
				IsActive = false;
				anm.cullingMode = AnimatorCullingMode.CullCompletely;
				return;
			}
			IsActive = true;
			anm.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		else
		{
			IsActive = true;
			anm.cullingMode = AnimatorCullingMode.AlwaysAnimate;
		}
		anm.speed = AnimSpeed;
		if (anm.GetNextAnimatorClipInfo(0).Length != 0)
		{
			OnAnm = anm.GetNextAnimatorStateInfo(0);
		}
		else if (anm.GetCurrentAnimatorClipInfo(0).Length != 0)
		{
			OnAnm = anm.GetCurrentAnimatorStateInfo(0);
		}
		if ((currframe == 15f) | ((double)anm.GetAnimatorTransitionInfo(0).normalizedTime > 0.5))
		{
			currframe = 0f;
			lastframe = -1f;
		}
		else
		{
			currframe = Mathf.Round(OnAnm.normalizedTime % 1f * 15f);
		}
		if (Health > 0f)
		{
			if (loop > 100)
			{
				if (CanSwim)
				{
					if (anm.GetInteger("Move") != 0)
					{
						Food = Mathf.Clamp(Food - 0.01f, 0f, 100f);
					}
					if (IsInWater | IsOnWater)
					{
						Stamina = Mathf.Clamp(Stamina + 1f, 0f, 100f);
						Water = Mathf.Clamp(Water + 1f, 0f, 100f);
					}
					else if (CanWalk)
					{
						Stamina = Mathf.Clamp(Stamina - 0.01f, 0f, 100f);
						Water = Mathf.Clamp(Water - 0.01f, 0f, 100f);
					}
					else
					{
						Stamina = Mathf.Clamp(Stamina - 1f, 0f, 100f);
						Water = Mathf.Clamp(Water - 1f, 0f, 100f);
						Health = Mathf.Clamp(Health - 1f, 0f, 100f);
					}
				}
				else
				{
					if (anm.GetInteger("Move") != 0)
					{
						Stamina = Mathf.Clamp(Stamina - 0.01f, 0f, 100f);
						Water = Mathf.Clamp(Water - 0.01f, 0f, 100f);
						Food = Mathf.Clamp(Herbivorous ? (Food - 0.1f) : (Food - 0.01f), 0f, 100f);
					}
					if (IsInWater)
					{
						Stamina = Mathf.Clamp(Stamina - 1f, 0f, 100f);
						Health = Mathf.Clamp(Health - 1f, 0f, 100f);
					}
				}
				if ((Food == 0f) | (Stamina == 0f) | (Water == 0f))
				{
					Health = Mathf.Clamp(Health - 0.1f, 0f, 100f);
				}
				else
				{
					Health = Mathf.Clamp(Health + 0.1f, 0f, 100f);
				}
				loop = 0;
			}
			else
			{
				loop++;
			}
			return;
		}
		Water = 0f;
		Food = 0f;
		Stamina = 0f;
		behavior = "Dead";
		if (main.TimeAfterDead == 0)
		{
			return;
		}
		if (behaviorCount > 0f)
		{
			behaviorCount = 0f;
		}
		else if (behaviorCount == (float)(-main.TimeAfterDead))
		{
			if (main.selected >= main.creaturesList.IndexOf(base.transform.gameObject) && main.selected > 0)
			{
				main.selected--;
			}
			main.creaturesList.Remove(base.transform.gameObject);
			UnityEngine.Object.Destroy(base.transform.gameObject);
		}
		else
		{
			behaviorCount -= 1f;
		}
	}

	private void SpawnBlood(Vector3 position)
	{
		ParticleSystem particleSystem = UnityEngine.Object.Instantiate(main.blood, position, Quaternion.Euler(-90f, 0f, 0f));
		particleSystem.transform.localScale = new Vector3(boxscale.z / 10f, boxscale.z / 10f, boxscale.z / 10f);
		UnityEngine.Object.Destroy(particleSystem.gameObject, 1f);
	}

	private void OnCollisionExit()
	{
		objCOL = null;
	}

	public void ManageCollision(Collision col, float pitch_max, float crouch_max, AudioSource[] source, AudioClip pain, AudioClip Hit_jaw, AudioClip Hit_head, AudioClip Hit_tail)
	{
		if (col.transform.root.tag.Equals("Creature"))
		{
			Creature component = col.gameObject.GetComponent<Creature>();
			objCOL = component.gameObject;
			if (!UseAI && OnAttack)
			{
				objTGT = component.gameObject;
				component.objTGT = base.transform.gameObject;
				behaviorCount = 500f;
				component.behaviorCount = 500f;
				if (component.specie == specie)
				{
					behavior = "Contest";
					component.behavior = "Contest";
				}
				else if (component.CanAttack)
				{
					behavior = "Battle";
					component.behavior = "Battle";
				}
				else
				{
					behavior = "Battle";
					component.behavior = "ToFlee";
				}
			}
			if (IsDead && lastHit == 0f && component.IsConstrained)
			{
				SpawnBlood(col.GetContact(0).point);
				body.AddForce(-component.transform.forward, ForceMode.Acceleration);
				lastHit = 25f;
				return;
			}
			if (lastHit == 0f && component.OnAttack)
			{
				float num = Mathf.Clamp(component.BaseMass * component.DamageMultiplier / (BaseMass * ArmorMultiplier), 10f, 100f);
				if (col.collider.gameObject.name.StartsWith("jaw"))
				{
					SpawnBlood(col.GetContact(0).point);
					if (!IsInWater)
					{
						body.AddForce(-col.GetContact(0).normal * component.body.mass / 4f, ForceMode.Acceleration);
					}
					lastHit = 50f;
					if (IsDead)
					{
						return;
					}
					source[0].pitch = UnityEngine.Random.Range(1f, 1.5f);
					source[0].PlayOneShot(pain, 1f);
					source[1].PlayOneShot(Hit_jaw, UnityEngine.Random.Range(0.1f, 0.4f));
					Health = Mathf.Clamp(Health - num, 0f, 100f);
				}
				else if (col.collider.gameObject.name.Equals("head"))
				{
					SpawnBlood(col.GetContact(0).point);
					if (!IsInWater)
					{
						body.AddForce(col.GetContact(0).normal * component.body.mass / 4f, ForceMode.Acceleration);
					}
					lastHit = 50f;
					if (IsDead)
					{
						return;
					}
					source[0].pitch = UnityEngine.Random.Range(1f, 1.5f);
					source[0].PlayOneShot(pain, 1f);
					source[1].PlayOneShot(Hit_head, UnityEngine.Random.Range(0.1f, 0.4f));
					if (!Herbivorous)
					{
						Health = Mathf.Clamp(Health - num, 0f, 100f);
					}
					else
					{
						Health = Mathf.Clamp(Health - num / 10f, 0f, 100f);
					}
				}
				else if (!col.collider.gameObject.name.Equals("root"))
				{
					SpawnBlood(col.GetContact(0).point);
					if (!IsInWater)
					{
						body.AddForce(col.GetContact(0).normal * component.body.mass / 4f, ForceMode.Acceleration);
					}
					lastHit = 50f;
					if (IsDead)
					{
						return;
					}
					source[0].pitch = UnityEngine.Random.Range(1f, 1.5f);
					source[0].PlayOneShot(pain, 1f);
					source[1].PlayOneShot(Hit_tail, UnityEngine.Random.Range(0.1f, 0.4f));
					if (!Herbivorous)
					{
						Health = Mathf.Clamp(Health - num, 0f, 100f);
					}
					else
					{
						Health -= (Health = Mathf.Clamp(Health - num / 10f, 0f, 100f));
					}
				}
			}
			if (objTGT != objCOL)
			{
				lookTGT = component.Head.position;
				posCOL = col.GetContact(0).point;
			}
		}
		else if (col.gameObject != objTGT)
		{
			objCOL = col.gameObject;
			posCOL = col.GetContact(0).point;
		}
	}

	public void GetGroundPos(IkType ikType, Transform RLeg1 = null, Transform RLeg2 = null, Transform RLeg3 = null, Transform LLeg1 = null, Transform LLeg2 = null, Transform LLeg3 = null, Transform RArm1 = null, Transform RArm2 = null, Transform RArm3 = null, Transform LArm1 = null, Transform LArm2 = null, Transform LArm3 = null, float FeetOffset = 0f)
	{
		posY = 0f - base.transform.position.y;
		if (main.UseRaycast)
		{
			if ((ikType == IkType.None) | IsDead | IsInWater | !IsOnGround)
			{
				RaycastHit hitInfo;
				if (Physics.Raycast(base.transform.position + Vector3.up * withersSize, -Vector3.up, out hitInfo, withersSize * 1.5f, 1))
				{
					posY = hitInfo.point.y;
					normal = hitInfo.normal;
					IsOnGround = true;
				}
				else
				{
					IsOnGround = false;
				}
			}
			else if (ikType >= IkType.SmBiped)
			{
				RaycastHit hitInfo2;
				if (Physics.Raycast(base.transform.position + base.transform.forward * 2f + Vector3.up, -Vector3.up, out hitInfo2, withersSize * 2f, 1))
				{
					posY = hitInfo2.point.y;
					normal = hitInfo2.normal;
				}
				RaycastHit hitInfo3;
				if (Physics.Raycast(RLeg3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo3, withersSize * 2f, 1))
				{
					IsOnGround = true;
					BR_HIT = hitInfo3.point;
					BR_Norm = hitInfo3.normal;
				}
				else
				{
					BR_HIT.y = 0f - base.transform.position.y;
				}
				RaycastHit hitInfo4;
				if (Physics.Raycast(LLeg3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo4, withersSize * 2f, 1))
				{
					IsOnGround = true;
					BL_HIT = hitInfo4.point;
					BL_Norm = hitInfo4.normal;
				}
				else
				{
					BL_HIT.y = 0f - base.transform.position.y;
				}
				if (posY > BL_HIT.y && posY > BR_HIT.y)
				{
					posY = Mathf.Max(BL_HIT.y, BR_HIT.y);
				}
				else
				{
					posY = Mathf.Min(BL_HIT.y, BR_HIT.y);
				}
				normal = (BL_Norm + BR_Norm + normal) / 3f;
			}
			else if (ikType == IkType.Flying)
			{
				IsOnGround = false;
				RaycastHit hitInfo5;
				if (Physics.Raycast(base.transform.position + Vector3.up * withersSize, -Vector3.up, out hitInfo5, withersSize * 4f, 1))
				{
					normal = hitInfo5.normal;
					IsOnGround = true;
					RaycastHit hitInfo6;
					if (Physics.Raycast(RArm3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo6, withersSize * 4f, 1))
					{
						FR_HIT = hitInfo6.point;
						FR_Norm = hitInfo6.normal;
					}
					else
					{
						FR_Norm = hitInfo5.normal;
						FR_HIT.y = 0f - base.transform.position.y;
					}
					RaycastHit hitInfo7;
					if (Physics.Raycast(LArm3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo7, withersSize * 4f, 1))
					{
						FL_HIT = hitInfo7.point;
						FL_Norm = hitInfo7.normal;
					}
					else
					{
						FL_Norm = hitInfo5.normal;
						FL_HIT.y = 0f - base.transform.position.y;
					}
					RaycastHit hitInfo8;
					if (Physics.Raycast(RLeg3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo8, withersSize * 4f, 1))
					{
						BR_HIT = hitInfo8.point;
						BR_Norm = hitInfo8.normal;
					}
					else
					{
						BR_Norm = hitInfo5.normal;
						BR_HIT.y = 0f - base.transform.position.y;
					}
					RaycastHit hitInfo9;
					if (Physics.Raycast(LLeg3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo9, withersSize * 4f, 1))
					{
						BL_HIT = hitInfo9.point;
						BL_Norm = hitInfo9.normal;
					}
					else
					{
						BL_Norm = hitInfo5.normal;
						BL_HIT.y = 0f - base.transform.position.y;
					}
					posY = hitInfo5.point.y;
				}
			}
			else
			{
				IsOnGround = false;
				RaycastHit hitInfo10;
				if (Physics.Raycast(RArm3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo10, withersSize * 2f, 1))
				{
					FR_HIT = hitInfo10.point;
					FR_Norm = hitInfo10.normal;
					IsOnGround = true;
				}
				else
				{
					FR_HIT.y = 0f - base.transform.position.y;
				}
				RaycastHit hitInfo11;
				if (Physics.Raycast(LArm3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo11, withersSize * 2f, 1))
				{
					FL_HIT = hitInfo11.point;
					FL_Norm = hitInfo11.normal;
					IsOnGround = true;
				}
				else
				{
					FL_HIT.y = 0f - base.transform.position.y;
				}
				RaycastHit hitInfo12;
				if (Physics.Raycast(RLeg3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo12, withersSize * 2f, 1))
				{
					BR_HIT = hitInfo12.point;
					BR_Norm = hitInfo12.normal;
					IsOnGround = true;
				}
				else
				{
					BR_HIT.y = 0f - base.transform.position.y;
				}
				RaycastHit hitInfo13;
				if (Physics.Raycast(LLeg3.position + Vector3.up * withersSize, -Vector3.up, out hitInfo13, withersSize * 2f, 1))
				{
					BL_HIT = hitInfo13.point;
					BL_Norm = hitInfo13.normal;
					IsOnGround = true;
				}
				else
				{
					BL_HIT.y = 0f - base.transform.position.y;
				}
				if (ikType == IkType.Convex)
				{
					if (IsConstrained)
					{
						posY = Mathf.Min(BR_HIT.y, BL_HIT.y, FR_HIT.y, FL_HIT.y);
					}
					else
					{
						posY = (BR_HIT.y + BL_HIT.y + FR_HIT.y + FL_HIT.y) / 4f;
					}
				}
				else if (IsConstrained | !main.UseIK)
				{
					posY = Mathf.Min(BR_HIT.y, BL_HIT.y, FR_HIT.y, FL_HIT.y);
				}
				else
				{
					posY = (BR_HIT.y + BL_HIT.y + FR_HIT.y + FL_HIT.y - size) / 4f;
				}
				normal = Vector3.Cross(FR_HIT - BL_HIT, BR_HIT - FL_HIT).normalized;
			}
		}
		else if ((ikType == IkType.None) | IsDead | IsInWater | !IsOnGround)
		{
			float num = (base.transform.position.x - main.T.transform.position.x) / main.T.terrainData.size.x * main.tres;
			float num2 = (base.transform.position.z - main.T.transform.position.z) / main.T.terrainData.size.z * main.tres;
			normal = main.T.terrainData.GetInterpolatedNormal(num / main.tres, num2 / main.tres);
			posY = main.T.SampleHeight(base.transform.position) + main.T.GetPosition().y;
		}
		else if (ikType >= IkType.SmBiped)
		{
			BR_HIT = new Vector3(RLeg3.position.x, main.T.SampleHeight(RLeg3.position) + main.tpos.y, RLeg3.position.z);
			float num3 = (RLeg3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			float num4 = (RLeg3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			BR_Norm = main.tdata.GetInterpolatedNormal(num3 / main.tres, num4 / main.tres);
			BL_HIT = new Vector3(LLeg3.position.x, main.T.SampleHeight(LLeg3.position) + main.tpos.y, LLeg3.position.z);
			num3 = (LLeg3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num4 = (LLeg3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			BL_Norm = main.tdata.GetInterpolatedNormal(num3 / main.tres, num4 / main.tres);
			if (posY > BL_HIT.y && posY > BR_HIT.y)
			{
				posY = Mathf.Max(BL_HIT.y, BR_HIT.y);
			}
			else
			{
				posY = Mathf.Min(BL_HIT.y, BR_HIT.y);
			}
			normal = (BL_Norm + BR_Norm + normal) / 3f;
		}
		else if (ikType == IkType.Flying)
		{
			float num5 = (base.transform.position.x - main.T.transform.position.x) / main.T.terrainData.size.x * main.tres;
			float num6 = (base.transform.position.z - main.T.transform.position.z) / main.T.terrainData.size.z * main.tres;
			normal = main.T.terrainData.GetInterpolatedNormal(num5 / main.tres, num6 / main.tres);
			posY = main.T.SampleHeight(base.transform.position) + main.T.GetPosition().y;
			BR_HIT = new Vector3(RLeg3.position.x, main.T.SampleHeight(RLeg3.position) + main.tpos.y, RLeg3.position.z);
			num5 = (RLeg3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num6 = (RLeg3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			BR_Norm = main.tdata.GetInterpolatedNormal(num5 / main.tres, num6 / main.tres);
			BL_HIT = new Vector3(LLeg3.position.x, main.T.SampleHeight(LLeg3.position) + main.tpos.y, LLeg3.position.z);
			num5 = (LLeg3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num6 = (LLeg3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			BL_Norm = main.tdata.GetInterpolatedNormal(num5 / main.tres, num6 / main.tres);
			FR_HIT = new Vector3(RArm3.position.x, main.T.SampleHeight(RArm3.position) + main.tpos.y, RArm3.position.z);
			num5 = (RArm3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num6 = (RArm3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			FR_Norm = main.tdata.GetInterpolatedNormal(num5 / main.tres, num6 / main.tres);
			FL_HIT = new Vector3(LArm3.position.x, main.T.SampleHeight(LArm3.position) + main.tpos.y, LArm3.position.z);
			num5 = (LArm3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num6 = (LArm3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			FL_Norm = main.tdata.GetInterpolatedNormal(num5 / main.tres, num6 / main.tres);
		}
		else
		{
			BR_HIT = new Vector3(RLeg3.position.x, main.T.SampleHeight(RLeg3.position) + main.tpos.y, RLeg3.position.z);
			float num7 = (RLeg3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			float num8 = (RLeg3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			BR_Norm = main.tdata.GetInterpolatedNormal(num7 / main.tres, num8 / main.tres);
			BL_HIT = new Vector3(LLeg3.position.x, main.T.SampleHeight(LLeg3.position) + main.tpos.y, LLeg3.position.z);
			num7 = (LLeg3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num8 = (LLeg3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			BL_Norm = main.tdata.GetInterpolatedNormal(num7 / main.tres, num8 / main.tres);
			FR_HIT = new Vector3(RArm3.position.x, main.T.SampleHeight(RArm3.position) + main.tpos.y, RArm3.position.z);
			num7 = (RArm3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num8 = (RArm3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			FR_Norm = main.tdata.GetInterpolatedNormal(num7 / main.tres, num8 / main.tres);
			FL_HIT = new Vector3(LArm3.position.x, main.T.SampleHeight(LArm3.position) + main.tpos.y, LArm3.position.z);
			num7 = (LArm3.position.x - main.tpos.x) / main.tdata.size.x * main.tres;
			num8 = (LArm3.position.z - main.tpos.z) / main.tdata.size.z * main.tres;
			FL_Norm = main.tdata.GetInterpolatedNormal(num7 / main.tres, num8 / main.tres);
			if (ikType == IkType.Convex)
			{
				if (IsConstrained)
				{
					posY = Mathf.Min(BR_HIT.y, BL_HIT.y, FR_HIT.y, FL_HIT.y);
				}
				else
				{
					posY = (BR_HIT.y + BL_HIT.y + FR_HIT.y + FL_HIT.y) / 4f;
				}
			}
			else if (IsConstrained | !main.UseIK)
			{
				posY = Mathf.Min(BR_HIT.y, BL_HIT.y, FR_HIT.y, FL_HIT.y);
			}
			else
			{
				posY = (BR_HIT.y + BL_HIT.y + FR_HIT.y + FL_HIT.y - size) / 4f;
			}
			normal = Vector3.Cross(FR_HIT - BL_HIT, BR_HIT - FL_HIT).normalized;
		}
		if (base.transform.position.y - size <= posY)
		{
			IsOnGround = true;
		}
		else
		{
			IsOnGround = false;
		}
		waterY = main.WaterAlt - crouch;
		if (base.transform.position.y < waterY && body.worldCenterOfMass.y > waterY)
		{
			IsOnWater = true;
		}
		else
		{
			IsOnWater = false;
		}
		if (body.worldCenterOfMass.y < waterY)
		{
			IsInWater = true;
		}
		else
		{
			IsInWater = false;
		}
		if (IsDead)
		{
			body.maxDepenetrationVelocity = 0.25f;
			body.constraints = RigidbodyConstraints.None;
		}
		else if (IsConstrained)
		{
			body.maxDepenetrationVelocity = 0f;
			crouch = 0f;
			body.constraints = (RigidbodyConstraints)122;
		}
		else
		{
			body.maxDepenetrationVelocity = 5f;
			if (lastHit == 0f)
			{
				body.constraints = RigidbodyConstraints.FreezeRotationZ;
			}
			else
			{
				body.constraints = RigidbodyConstraints.None;
			}
		}
		if (IsOnGround && !IsInWater)
		{
			Quaternion quaternion = Quaternion.LookRotation(Vector3.Cross(base.transform.right, normal), normal);
			if (!CanFly)
			{
				float value = Mathf.DeltaAngle(quaternion.eulerAngles.x, 0f);
				float value2 = Mathf.DeltaAngle(quaternion.eulerAngles.z, 0f);
				float num9 = Mathf.Clamp(value, -45f, 45f);
				float num10 = Mathf.Clamp(value2, -10f, 10f);
				normAng = Quaternion.Euler(0f - num9, anm.GetFloat("Turn"), 0f - num10);
			}
			else
			{
				normAng = Quaternion.Euler(quaternion.eulerAngles.x, anm.GetFloat("Turn"), quaternion.eulerAngles.z);
			}
			posY -= crouch;
		}
		else if (IsInWater | IsOnWater)
		{
			normAng = Quaternion.Euler(0f, anm.GetFloat("Turn"), 0f);
			posY = waterY - body.centerOfMass.y;
		}
		else
		{
			normAng = Quaternion.Euler(0f, anm.GetFloat("Turn"), 0f);
			posY = 0f - base.transform.position.y;
		}
	}

	public void ApplyGravity(float multiplier = 1f)
	{
		body.AddForce(Vector3.up * size * ((body.velocity.y > 0f) ? (-20f * body.drag) : (-50f * body.drag)) * multiplier, ForceMode.Acceleration);
	}

	public void ApplyYPos()
	{
		if (IsOnGround && ((Mathf.Abs(normal.x) > main.MaxSlope) | (Mathf.Abs(normal.z) > main.MaxSlope)))
		{
			body.AddForce(new Vector3(normal.x, 0f - normal.y, normal.z) * 64f, ForceMode.Acceleration);
			behaviorCount = 0f;
		}
		body.AddForce(Vector3.up * Mathf.Clamp(posY - base.transform.position.y, 0f - size, size), ForceMode.VelocityChange);
	}

	public void Move(Vector3 dir, float force = 0f, bool jump = false)
	{
		if (CanAttack && anm.GetBool("Attack").Equals(true))
		{
			force *= 1.5f;
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, normAng, Ang_T * 2f);
		}
		else
		{
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, normAng, Ang_T);
		}
		if (dir != Vector3.zero)
		{
			force = ((CanSwim || IsOnGround) ? (force / (4f / body.drag)) : ((IsInWater | IsOnWater) ? (force / 8f) : ((CanFly || OnJump) ? (force / (4f / body.drag)) : (force / 8f))));
			body.AddForce(dir * force * speed, jump ? ForceMode.VelocityChange : ForceMode.Acceleration);
		}
	}

	public void RotateBone(IkType ikType, float maxX, float maxY = 0f, bool CanMoveHead = true, float t = 0.5f)
	{
		if (AnimSpeed == 0f)
		{
			return;
		}
		if (!OnAttack && !OnJump)
		{
			speed = size * anm.speed * (1f - Mathf.Abs(Mathf.DeltaAngle(base.transform.eulerAngles.y, anm.GetFloat("Turn"))) / 135f);
		}
		if (main.UseIK)
		{
			main.message = 2;
			ikType = IkType.None;
			main.UseIK = false;
		}
		if (lastHit != 0f)
		{
			if (!IsDead && CanWalk)
			{
				crouch = Mathf.Lerp(crouch, Crouch_Max * size / 2f, 1f);
			}
			lastHit -= 1f;
		}
		if (OnReset)
		{
			pitch = Mathf.Lerp(pitch, 0f, t / 10f);
			roll = Mathf.Lerp(roll, 0f, t / 10f);
			headX = Mathf.LerpAngle(headX, 0f, t / 10f);
			headY = Mathf.LerpAngle(headY, 0f, t / 10f);
			crouch = Mathf.Lerp(crouch, 0f, t / 10f);
			spineX = Mathf.LerpAngle(spineX, 0f, t / 10f);
			spineY = Mathf.LerpAngle(spineY, 0f, t / 10f);
			return;
		}
		if (avoidDelta != 0f)
		{
			if (Mathf.Abs(avoidAdd) > 90f)
			{
				avoidDelta = 0f;
			}
			avoidAdd = Mathf.MoveTowardsAngle(avoidAdd, (avoidDelta > 0f) ? 135f : (-135f), t);
		}
		else
		{
			avoidAdd = Mathf.MoveTowardsAngle(avoidAdd, 0f, t);
		}
		if ((bool)objTGT)
		{
			if (behavior.EndsWith("Hunt") | behavior.Equals("Battle") | behavior.EndsWith("Contest"))
			{
				lookTGT = objTGT.transform.position;
			}
			else if (Herbivorous && behavior.Equals("Food"))
			{
				lookTGT = posTGT;
			}
			else if (loop == 0)
			{
				lookTGT = Vector3.zero;
			}
		}
		else if (loop == 0)
		{
			lookTGT = Vector3.zero;
		}
		if (CanMoveHead)
		{
			if (!OnTailAttack && !anm.GetInteger("Move").Equals(0))
			{
				spineX = Mathf.MoveTowardsAngle(spineX, Mathf.DeltaAngle(anm.GetFloat("Turn"), base.transform.eulerAngles.y) / 360f * maxX, t);
				spineY = Mathf.LerpAngle(spineY, 0f, t / 10f);
			}
			else
			{
				spineX = Mathf.MoveTowardsAngle(spineX, 0f, t / 10f);
				spineY = Mathf.LerpAngle(spineY, 0f, t / 10f);
			}
			if ((!CanFly && !CanSwim && anm.GetInteger("Move") != 2) | !IsOnGround)
			{
				roll = Mathf.Lerp(roll, 0f, Ang_T);
			}
			crouch = Mathf.Lerp(crouch, 0f, t / 10f);
			if (OnHeadMove)
			{
				return;
			}
			if (lookTGT != Vector3.zero && (lookTGT - base.transform.position).magnitude > boxscale.z)
			{
				Quaternion quaternion = ((!objTGT || !objTGT.tag.Equals("Creature")) ? Quaternion.LookRotation(lookTGT - HeadPos) : Quaternion.LookRotation(objTGT.GetComponent<Rigidbody>().worldCenterOfMass - HeadPos));
				headX = Mathf.MoveTowardsAngle(headX, Mathf.DeltaAngle(quaternion.eulerAngles.y, base.transform.eulerAngles.y) / (180f - Yaw_Max) * Yaw_Max, t);
				headY = Mathf.MoveTowardsAngle(headY, Mathf.DeltaAngle(quaternion.eulerAngles.x, base.transform.eulerAngles.x) / (90f - Pitch_Max) * Pitch_Max, t);
			}
			else if (Mathf.RoundToInt(anm.GetFloat("Turn")) == Mathf.RoundToInt(base.transform.eulerAngles.y))
			{
				if (loop == 0 && Mathf.RoundToInt(headX * 100f) == Mathf.RoundToInt(rndX * 100) && Mathf.RoundToInt(headY * 100f) == Mathf.RoundToInt(rndY * 100))
				{
					rndX = UnityEngine.Random.Range((int)(0f - Yaw_Max) / 2, (int)Yaw_Max / 2);
					rndY = UnityEngine.Random.Range((int)(0f - Pitch_Max) / 2, (int)Pitch_Max / 2);
				}
				headX = Mathf.LerpAngle(headX, rndX, t / 10f);
				headY = Mathf.LerpAngle(headY, rndY, t / 10f);
			}
			else
			{
				headX = Mathf.LerpAngle(headX, spineX, t / 10f);
				headY = Mathf.LerpAngle(headY, 0f, t / 10f);
			}
			return;
		}
		spineX = Mathf.LerpAngle(spineX, Mathf.DeltaAngle(anm.GetFloat("Turn"), base.transform.eulerAngles.y) / 360f * maxX, Ang_T);
		if (IsOnGround && !IsInWater)
		{
			spineY = Mathf.LerpAngle(spineY, 0f, t / 10f);
			roll = Mathf.LerpAngle(roll, 0f, t / 10f);
			pitch = Mathf.Lerp(pitch, 0f, t / 10f);
		}
		else if (CanFly)
		{
			if (anm.GetInteger("Move") >= 2 && anm.GetInteger("Move") < 3)
			{
				spineY = Mathf.LerpAngle(spineY, Mathf.DeltaAngle(anm.GetFloat("Pitch") * 90f, pitch) / 180f * maxY, Ang_T);
			}
			roll = Mathf.LerpAngle(roll, 0f - spineX, t / 10f);
		}
		else
		{
			spineY = Mathf.LerpAngle(spineY, Mathf.DeltaAngle(anm.GetFloat("Pitch") * 90f, pitch) / 180f * maxY, Ang_T);
			roll = Mathf.LerpAngle(roll, 0f - spineX, t / 10f);
		}
		headX = Mathf.LerpAngle(headX, spineX, t);
		headY = Mathf.LerpAngle(headY, spineY, t);
	}

	public void GetUserInputs(int idle1 = 0, int idle2 = 0, int idle3 = 0, int idle4 = 0, int eat = 0, int drink = 0, int sleep = 0, int rise = 0)
	{
		if (behavior == "Repose" && anm.GetInteger("Move") != 0)
		{
			behavior = "Player";
		}
		else if (behaviorCount <= 0f)
		{
			objTGT = null;
			behavior = "Player";
			behaviorCount = 0f;
		}
		else
		{
			behaviorCount -= 1f;
		}
		if (base.transform.gameObject == main.creaturesList[main.selected].gameObject && main.CameraMode != 0)
		{
			bool flag = (Input.GetKey(KeyCode.LeftShift) ? true : false);
			if (CanAttack)
			{
				if (Input.GetKey(KeyCode.Mouse0))
				{
					behaviorCount = 500f;
					behavior = "Hunt";
					anm.SetBool("Attack", true);
				}
				else
				{
					anm.SetBool("Attack", false);
				}
			}
			if (main.UseIK && Input.GetKey(KeyCode.LeftControl))
			{
				crouch = Crouch_Max * size;
				OnCrouch = true;
			}
			else
			{
				OnCrouch = false;
			}
			if (CanFly | CanSwim)
			{
				if (Input.GetKey(KeyCode.Mouse1))
				{
					anm.SetFloat("Turn", base.transform.eulerAngles.y + Input.GetAxis("Mouse X") * 22.5f);
					if (Input.GetAxis("Mouse Y") != 0f && anm.GetInteger("Move") == 3)
					{
						anm.SetFloat("Pitch", Input.GetAxis("Mouse Y"));
					}
					else if (Input.GetKey(KeyCode.LeftControl))
					{
						anm.SetFloat("Pitch", 1f);
					}
					else if (Input.GetKey(KeyCode.Space))
					{
						anm.SetFloat("Pitch", -1f);
					}
				}
				else if (Input.GetKey(KeyCode.LeftControl))
				{
					anm.SetFloat("Pitch", 1f);
				}
				else if (Input.GetKey(KeyCode.Space))
				{
					anm.SetFloat("Pitch", -1f);
				}
				else
				{
					anm.SetFloat("Pitch", 0f);
				}
			}
			if (CanJump && Input.GetKey(KeyCode.Space) && !OnJump)
			{
				anm.SetInteger("Move", 3);
			}
			else if ((Input.GetAxis("Horizontal") != 0f) | (Input.GetAxis("Vertical") != 0f))
			{
				if (CanSwim | (CanFly && !IsOnGround))
				{
					if (Input.GetKey(KeyCode.Mouse1))
					{
						if (Input.GetAxis("Vertical") < 0f)
						{
							anm.SetInteger("Move", -1);
						}
						else if (Input.GetAxis("Vertical") > 0f)
						{
							anm.SetInteger("Move", 3);
						}
						else if (Input.GetAxis("Horizontal") > 0f)
						{
							anm.SetInteger("Move", -10);
						}
						else if (Input.GetAxis("Horizontal") < 0f)
						{
							anm.SetInteger("Move", 10);
						}
						else
						{
							anm.SetInteger("Move", 0);
						}
					}
					else
					{
						if (flag)
						{
							anm.SetInteger("Move", (!CanSwim) ? 1 : 2);
						}
						else
						{
							anm.SetInteger("Move", CanSwim ? 1 : 2);
						}
						float value = main.transform.eulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * 57.29578f;
						anm.SetFloat("Turn", value);
					}
				}
				else if (Input.GetKey(KeyCode.Mouse1))
				{
					if (Input.GetAxis("Vertical") > 0f && !flag)
					{
						anm.SetInteger("Move", 1);
					}
					else if (Input.GetAxis("Vertical") > 0f)
					{
						anm.SetInteger("Move", 2);
					}
					else if (Input.GetAxis("Vertical") < 0f)
					{
						anm.SetInteger("Move", -1);
					}
					else if (Input.GetAxis("Horizontal") > 0f)
					{
						anm.SetInteger("Move", -10);
					}
					else if (Input.GetAxis("Horizontal") < 0f)
					{
						anm.SetInteger("Move", 10);
					}
					anm.SetFloat("Turn", base.transform.eulerAngles.y + Input.GetAxis("Mouse X") * 22.5f);
				}
				else
				{
					float value2 = main.transform.eulerAngles.y + Mathf.Atan2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * 57.29578f;
					anm.SetInteger("Move", (!flag) ? 1 : 2);
					anm.SetFloat("Turn", value2);
				}
			}
			else if ((CanSwim | CanFly) && !IsOnGround)
			{
				if (CanSwim && anm.GetFloat("Pitch") != 0f && !Input.GetKey(KeyCode.Mouse1))
				{
					anm.SetInteger("Move", (!flag) ? 1 : 2);
				}
				else
				{
					anm.SetInteger("Move", 0);
				}
			}
			else if (Input.GetKey(KeyCode.Mouse1))
			{
				if (Input.GetAxis("Mouse X") > 0f)
				{
					anm.SetInteger("Move", 10);
				}
				else if (Input.GetAxis("Mouse X") < 0f)
				{
					anm.SetInteger("Move", -10);
				}
				else
				{
					anm.SetInteger("Move", 0);
				}
				anm.SetFloat("Turn", base.transform.eulerAngles.y + Input.GetAxis("Mouse X") * 22.5f);
			}
			else
			{
				anm.SetInteger("Move", 0);
			}
			if (CanInvertBody && Input.GetKeyDown(KeyCode.R))
			{
				if (OnInvert)
				{
					OnInvert = false;
				}
				else
				{
					OnInvert = true;
				}
			}
			if (Input.GetKey(KeyCode.E))
			{
				int num = 0;
				if (idle1 > 0)
				{
					num++;
				}
				if (idle2 > 0)
				{
					num++;
				}
				if (idle3 > 0)
				{
					num++;
				}
				if (idle4 > 0)
				{
					num++;
				}
				rndIdle = UnityEngine.Random.Range(1, num + 1);
				switch (rndIdle)
				{
				case 1:
					anm.SetInteger("Idle", idle1);
					break;
				case 2:
					anm.SetInteger("Idle", idle2);
					break;
				case 3:
					anm.SetInteger("Idle", idle3);
					break;
				case 4:
					anm.SetInteger("Idle", idle4);
					break;
				}
			}
			else if (Input.GetKey(KeyCode.F))
			{
				if (posTGT == Vector3.zero)
				{
					FindPlayerFood();
				}
				if (IsOnWater)
				{
					anm.SetInteger("Idle", drink);
					if (Water < 100f)
					{
						behavior = "Water";
						Water = Mathf.Clamp(Water + 0.05f, 0f, 100f);
					}
					if (Input.GetKeyUp(KeyCode.F))
					{
						posTGT = Vector3.zero;
					}
					else
					{
						posTGT = base.transform.position;
					}
				}
				else if (posTGT != Vector3.zero)
				{
					anm.SetInteger("Idle", eat);
					behavior = "Food";
					if (Food < 100f)
					{
						Food = Mathf.Clamp(Food + 0.05f, 0f, 100f);
					}
					if (Water < 25f)
					{
						Water += 0.05f;
					}
					if (Input.GetKeyUp(KeyCode.F))
					{
						posTGT = Vector3.zero;
					}
				}
				else
				{
					main.message = 1;
				}
			}
			else if (Input.GetKey(KeyCode.Q))
			{
				anm.SetInteger("Idle", sleep);
				if (anm.GetInteger("Move") != 0)
				{
					anm.SetInteger("Idle", 0);
				}
			}
			else if (rise != 0 && Input.GetKey(KeyCode.Space))
			{
				anm.SetInteger("Idle", rise);
			}
			else
			{
				anm.SetInteger("Idle", 0);
				posTGT = Vector3.zero;
			}
			if (Input.GetKey(KeyCode.Mouse2))
			{
				OnHeadMove = true;
				headX = Mathf.Lerp(headX, Mathf.Clamp(headX - Input.GetAxis("Mouse X"), 0f - Yaw_Max, Yaw_Max), 0.5f);
				headY = Mathf.Lerp(headY, Mathf.Clamp(headY + Input.GetAxis("Mouse Y"), 0f - Pitch_Max, Pitch_Max), 0.5f);
			}
			else
			{
				OnHeadMove = false;
			}
			delta = Mathf.DeltaAngle(main.transform.eulerAngles.y, anm.GetFloat("Turn"));
			if (OnAnm.IsName(specie + "|Sleep"))
			{
				behavior = "Repose";
				Stamina = Mathf.Clamp(Stamina + 0.05f, 0f, 100f);
			}
		}
		else
		{
			anm.SetInteger("Move", 0);
			anm.SetInteger("Idle", 0);
			if (CanAttack)
			{
				anm.SetBool("Attack", false);
			}
			if (CanFly | CanSwim)
			{
				anm.SetFloat("Pitch", 0f);
			}
		}
	}

	private bool FindPlayerFood()
	{
		if (!Herbivorous)
		{
			GameObject[] array = main.creaturesList.ToArray();
			foreach (GameObject gameObject in array)
			{
				if (!((gameObject.transform.position - Head.position).magnitude > boxscale.z))
				{
					Creature component = gameObject.GetComponent<Creature>();
					if (component.IsDead)
					{
						objTGT = component.gameObject;
						posTGT = component.body.worldCenterOfMass;
						return true;
					}
				}
			}
		}
		else if ((bool)main.T)
		{
			if (withersSize > 8f)
			{
				if (Physics.CheckSphere(Head.position, withersSize, main.treeLayer))
				{
					posTGT = Head.position;
					return true;
				}
				return false;
			}
			int num = 0;
			float num2 = (base.transform.position.x - main.T.transform.position.x) / main.tdata.size.z * main.tres;
			float num3 = (base.transform.position.z - main.T.transform.position.z) / main.tdata.size.x * main.tres;
			for (num = 0; num < main.tdata.detailPrototypes.Length; num++)
			{
				if (main.tdata.GetDetailLayer((int)num2, (int)num3, 1, 1, num)[0, 0] > 0)
				{
					posTGT.x = main.tdata.size.x / main.tres * num2 + main.T.transform.position.x;
					posTGT.z = main.tdata.size.z / main.tres * num3 + main.T.transform.position.z;
					posTGT.y = main.T.SampleHeight(new Vector3(posTGT.x, 0f, posTGT.z));
					objTGT = null;
					return true;
				}
			}
		}
		objTGT = null;
		posTGT = Vector3.zero;
		return false;
	}

	public void AICore(int idle1 = 0, int idle2 = 0, int idle3 = 0, int idle4 = 0, int eat = 0, int drink = 0, int sleep = 0)
	{
		main.message = 2;
		UseAI = false;
	}
}
