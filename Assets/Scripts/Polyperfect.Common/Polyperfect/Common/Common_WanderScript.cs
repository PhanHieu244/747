using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace Polyperfect.Common
{
	[RequireComponent(typeof(Animator))]
	[RequireComponent(typeof(CharacterController))]
	public class Common_WanderScript : MonoBehaviour
	{
		public enum WanderState
		{
			Idle,
			Wander,
			Chase,
			Evade,
			Attack,
			Dead
		}

		private const float contingencyDistance = 1f;

		[SerializeField]
		public IdleState[] idleStates;

		[SerializeField]
		private MovementState[] movementStates;

		[SerializeField]
		private AIState[] attackingStates;

		[SerializeField]
		private AIState[] deathStates;

		[SerializeField]
		public string species = "NA";

		[SerializeField]
		[Tooltip("This specific animal stats asset, create a new one from the asset menu under (LowPolyAnimals/NewAnimalStats)")]
		public AIStats stats;

		[SerializeField]
		[Tooltip("How far away from it's origin this animal will wander by itself.")]
		private float wanderZone = 10f;

		private int dominance = 1;

		private int originalDominance;

		[SerializeField]
		[Tooltip("How far this animal can sense a predator.")]
		private float awareness = 30f;

		[SerializeField]
		[Tooltip("How far this animal can sense it's prey.")]
		private float scent = 30f;

		private float originalScent;

		private float stamina = 10f;

		private float power = 10f;

		private float toughness = 5f;

		private float aggression;

		private float originalAggression;

		private float attackSpeed = 0.5f;

		private bool territorial;

		private bool stealthy;

		[SerializeField]
		[Tooltip("If true, this animal will never leave it's zone, even if it's chasing or running away from another animal.")]
		private bool constainedToWanderZone;

		[SerializeField]
		[Tooltip("This animal will be peaceful towards species in this list.")]
		private string[] nonAgressiveTowards;

		private static List<Common_WanderScript> allAnimals = new List<Common_WanderScript>();

		[SerializeField]
		[Tooltip("If true, this animal will rotate to match the terrain. Ensure you have set the layer of the terrain as 'Terrain'.")]
		private bool matchSurfaceRotation;

		[SerializeField]
		[Tooltip("How fast the animnal rotates to match the surface rotation.")]
		private float surfaceRotationSpeed = 2f;

		[SerializeField]
		[Tooltip("If true, AI changes to this animal will be logged in the console.")]
		private bool logChanges;

		[SerializeField]
		[Tooltip("If true, gizmos will be drawn in the editor.")]
		private bool showGizmos;

		[SerializeField]
		private bool drawWanderRange = true;

		[SerializeField]
		private bool drawScentRange = true;

		[SerializeField]
		private bool drawAwarenessRange = true;

		public UnityEvent deathEvent;

		public UnityEvent attackingEvent;

		public UnityEvent idleEvent;

		public UnityEvent movementEvent;

		private Color distanceColor = new Color(0f, 0f, 205f);

		private Color awarnessColor = new Color(1f, 0f, 1f, 1f);

		private Color scentColor = new Color(1f, 0f, 0f, 1f);

		private Animator animator;

		private CharacterController characterController;

		private NavMeshAgent navMeshAgent;

		private Vector3 origin;

		private int totalIdleStateWeight;

		private bool useNavMesh;

		private Vector3 targetLocation = Vector3.zero;

		private float turnSpeed;

		private float attackTimer;

		public WanderState CurrentState;

		private Common_WanderScript primaryPrey;

		private Common_WanderScript primaryPursuer;

		private Common_WanderScript attackTarget;

		private float moveSpeed;

		private float attackReach = 2f;

		private bool forceUpdate;

		private float idleStateDuration;

		private Vector3 startPosition;

		private Vector3 wanderTarget;

		private IdleState currentIdleState;

		private float idleUpdateTime;

		private bool started;

		private readonly HashSet<string> animatorParameters = new HashSet<string>();

		public float MaxDistance
		{
			get
			{
				return wanderZone;
			}
			set
			{
				wanderZone = value;
			}
		}

		public static List<Common_WanderScript> AllAnimals
		{
			get
			{
				return allAnimals;
			}
		}

		private float MinimumStaminaForAggression
		{
			get
			{
				return stats.stamina * 0.9f;
			}
		}

		private float MinimumStaminaForFlee
		{
			get
			{
				return stats.stamina * 0.1f;
			}
		}

		private IEnumerable<AIState> AllStates
		{
			get
			{
				IdleState[] array = idleStates;
				for (int i = 0; i < array.Length; i++)
				{
					yield return array[i];
				}
				MovementState[] array2 = movementStates;
				for (int i = 0; i < array2.Length; i++)
				{
					yield return array2[i];
				}
				AIState[] array3 = attackingStates;
				for (int i = 0; i < array3.Length; i++)
				{
					yield return array3[i];
				}
				array3 = deathStates;
				for (int i = 0; i < array3.Length; i++)
				{
					yield return array3[i];
				}
			}
		}

		public void OnDrawGizmosSelected()
		{
			if (!showGizmos)
			{
				return;
			}
			if (drawWanderRange)
			{
				Gizmos.color = distanceColor;
				Gizmos.DrawWireSphere((origin == Vector3.zero) ? base.transform.position : origin, wanderZone);
				Gizmos.DrawIcon(new Vector3(base.transform.position.x, base.transform.position.y + wanderZone, base.transform.position.z), "ico-wander", true);
			}
			if (drawAwarenessRange)
			{
				Gizmos.color = awarnessColor;
				Gizmos.DrawWireSphere(base.transform.position, awareness);
				Gizmos.DrawIcon(new Vector3(base.transform.position.x, base.transform.position.y + awareness, base.transform.position.z), "ico-awareness", true);
			}
			if (drawScentRange)
			{
				Gizmos.color = scentColor;
				Gizmos.DrawWireSphere(base.transform.position, scent);
				Gizmos.DrawIcon(new Vector3(base.transform.position.x, base.transform.position.y + scent, base.transform.position.z), "ico-scent", true);
			}
			if (!Application.isPlaying)
			{
				return;
			}
			if (useNavMesh)
			{
				if (navMeshAgent.remainingDistance > 1f)
				{
					Gizmos.DrawSphere(navMeshAgent.destination + new Vector3(0f, 0.1f, 0f), 0.2f);
					Gizmos.DrawLine(base.transform.position, navMeshAgent.destination);
				}
			}
			else if (targetLocation != Vector3.zero)
			{
				Gizmos.DrawSphere(targetLocation + new Vector3(0f, 0.1f, 0f), 0.2f);
				Gizmos.DrawLine(base.transform.position, targetLocation);
			}
		}

		private void Awake()
		{
			if (!stats)
			{
				Debug.LogError(string.Format("No stats attached to {0}'s Wander Script.", base.gameObject.name));
				base.enabled = false;
				return;
			}
			animator = GetComponent<Animator>();
			RuntimeAnimatorController runtimeAnimatorController = animator.runtimeAnimatorController;
			if ((bool)animator)
			{
				animatorParameters.UnionWith(animator.parameters.Select((AnimatorControllerParameter p) => p.name));
			}
			if (logChanges)
			{
				if (runtimeAnimatorController == null)
				{
					Debug.LogError(string.Format("{0} has no animator controller, make sure you put one in to allow the character to walk. See documentation for more details (1)", base.gameObject.name));
					base.enabled = false;
					return;
				}
				if (animator.avatar == null)
				{
					Debug.LogError(string.Format("{0} has no avatar, make sure you put one in to allow the character to animate. See documentation for more details (2)", base.gameObject.name));
					base.enabled = false;
					return;
				}
				if (animator.hasRootMotion)
				{
					Debug.LogError(string.Format("{0} has root motion applied, consider turning this off as our script will deactivate this on play as we do not use it (3)", base.gameObject.name));
					animator.applyRootMotion = false;
				}
				if (idleStates.Length == 0 || movementStates.Length == 0)
				{
					Debug.LogError(string.Format("{0} has no idle or movement states, make sure you fill these out. See documentation for more details (4)", base.gameObject.name));
					base.enabled = false;
					return;
				}
				if (idleStates.Length != 0)
				{
					for (int i = 0; i < idleStates.Length; i++)
					{
						if (idleStates[i].animationBool == "")
						{
							Debug.LogError(string.Format("{0} has " + idleStates.Length + " Idle states, you need to make sure that each state has an animation boolean. See documentation for more details (4)", base.gameObject.name));
							base.enabled = false;
							return;
						}
					}
				}
				if (movementStates.Length != 0)
				{
					for (int j = 0; j < movementStates.Length; j++)
					{
						if (movementStates[j].animationBool == "")
						{
							Debug.LogError(string.Format("{0} has " + movementStates.Length + " Movement states, you need to make sure that each state has an animation boolean to see the character walk. See documentation for more details (4)", base.gameObject.name));
							base.enabled = false;
							return;
						}
						if (movementStates[j].moveSpeed <= 0f)
						{
							Debug.LogError(string.Format("{0} has a movement state with a speed of 0 or less, you need to set the speed higher than 0 to see the character move. See documentation for more details (4)", base.gameObject.name));
							base.enabled = false;
							return;
						}
						if (movementStates[j].turnSpeed <= 0f)
						{
							Debug.LogError(string.Format("{0} has a turn speed state with a speed of 0 or less, you need to set the speed higher than 0 to see the character turn. See documentation for more details (4)", base.gameObject.name));
							base.enabled = false;
							return;
						}
					}
				}
				if (attackingStates.Length == 0)
				{
					Debug.Log(string.Format("{0} has " + attackingStates.Length + " this character will not be able to attack. See documentation for more details (4)", base.gameObject.name));
				}
				if (attackingStates.Length != 0)
				{
					for (int k = 0; k < attackingStates.Length; k++)
					{
						if (attackingStates[k].animationBool == "")
						{
							Debug.LogError(string.Format("{0} has " + attackingStates.Length + " attacking states, you need to make sure that each state has an animation boolean. See documentation for more details (4)", base.gameObject.name));
							base.enabled = false;
							return;
						}
					}
				}
				if (stats == null)
				{
					Debug.LogError(string.Format("{0} has no AI stats, make sure you assign one to the wander script. See documentation for more details (5)", base.gameObject.name));
					base.enabled = false;
					return;
				}
				if ((bool)animator)
				{
					foreach (AIState allState in AllStates)
					{
						if (!animatorParameters.Contains(allState.animationBool))
						{
							Debug.LogError(string.Format("{0} did not contain {1}. Make sure you set it in the Animation States on the character, and have a matching parameter in the Animator Controller assigned.", base.gameObject.name, allState.animationBool));
							base.enabled = false;
							return;
						}
					}
				}
			}
			IdleState[] array = idleStates;
			foreach (IdleState idleState in array)
			{
				totalIdleStateWeight += idleState.stateWeight;
			}
			origin = base.transform.position;
			animator.applyRootMotion = false;
			characterController = GetComponent<CharacterController>();
			navMeshAgent = GetComponent<NavMeshAgent>();
			originalDominance = stats.dominance;
			dominance = originalDominance;
			toughness = stats.toughness;
			territorial = stats.territorial;
			stamina = stats.stamina;
			originalAggression = stats.agression;
			aggression = originalAggression;
			attackSpeed = stats.attackSpeed;
			stealthy = stats.stealthy;
			originalScent = scent;
			scent = originalScent;
			if ((bool)navMeshAgent)
			{
				useNavMesh = true;
				navMeshAgent.stoppingDistance = 1f;
			}
			if (matchSurfaceRotation && base.transform.childCount > 0)
			{
				base.transform.GetChild(0).gameObject.AddComponent<Common_SurfaceRotation>().SetRotationSpeed(surfaceRotationSpeed);
			}
		}

		private void OnEnable()
		{
			allAnimals.Add(this);
		}

		private void OnDisable()
		{
			allAnimals.Remove(this);
			StopAllCoroutines();
		}

		private void Start()
		{
			startPosition = base.transform.position;
			if (Common_WanderManager.Instance != null && Common_WanderManager.Instance.PeaceTime)
			{
				SetPeaceTime(true);
			}
			StartCoroutine(RandomStartingDelay());
		}

		private void Update()
		{
			if (!started)
			{
				return;
			}
			if (forceUpdate)
			{
				UpdateAI();
				forceUpdate = false;
			}
			if (CurrentState == WanderState.Attack)
			{
				if (!attackTarget || attackTarget.CurrentState == WanderState.Dead)
				{
					Common_WanderScript common_WanderScript = attackTarget;
					UpdateAI();
					if ((bool)common_WanderScript && common_WanderScript == attackTarget)
					{
						Debug.LogError(string.Format("Target was same {0}", common_WanderScript.gameObject.name));
					}
				}
				attackTimer += Time.deltaTime;
			}
			if (attackTimer > attackSpeed)
			{
				attackTimer -= attackSpeed;
				if ((bool)attackTarget)
				{
					attackTarget.TakeDamage(power);
				}
				if (attackTarget.CurrentState == WanderState.Dead)
				{
					UpdateAI();
				}
			}
			Vector3 position = base.transform.position;
			Vector3 targetPos = position;
			switch (CurrentState)
			{
			case WanderState.Attack:
				FaceDirection((attackTarget.transform.position - position).normalized);
				targetPos = position;
				break;
			case WanderState.Chase:
				if (!primaryPrey || primaryPrey.CurrentState == WanderState.Dead)
				{
					primaryPrey = null;
					SetState(WanderState.Idle);
					goto case WanderState.Idle;
				}
				targetPos = primaryPrey.transform.position;
				ValidatePosition(ref targetPos);
				if (!IsValidLocation(targetPos))
				{
					SetState(WanderState.Idle);
					targetPos = position;
					UpdateAI();
					break;
				}
				FaceDirection((targetPos - position).normalized);
				stamina -= Time.deltaTime;
				if (stamina <= 0f)
				{
					UpdateAI();
				}
				break;
			case WanderState.Evade:
				targetPos = position + Vector3.ProjectOnPlane(position - primaryPursuer.transform.position, Vector3.up);
				if (!IsValidLocation(targetPos))
				{
					targetPos = startPosition;
				}
				ValidatePosition(ref targetPos);
				FaceDirection((targetPos - position).normalized);
				stamina -= Time.deltaTime;
				if (stamina <= 0f)
				{
					UpdateAI();
				}
				break;
			case WanderState.Wander:
				stamina = Mathf.MoveTowards(stamina, stats.stamina, Time.deltaTime);
				targetPos = wanderTarget;
				Debug.DrawLine(position, targetPos, Color.yellow);
				FaceDirection((targetPos - position).normalized);
				if (Vector3.ProjectOnPlane(targetPos - base.transform.position, Vector3.up).magnitude < 1f)
				{
					SetState(WanderState.Idle);
					UpdateAI();
				}
				break;
			case WanderState.Idle:
				stamina = Mathf.MoveTowards(stamina, stats.stamina, Time.deltaTime);
				if (Time.time >= idleUpdateTime)
				{
					SetState(WanderState.Wander);
					UpdateAI();
				}
				break;
			}
			if ((bool)navMeshAgent)
			{
				navMeshAgent.destination = targetPos;
				navMeshAgent.speed = moveSpeed;
				navMeshAgent.angularSpeed = turnSpeed;
			}
			else
			{
				characterController.SimpleMove(moveSpeed * Vector3.ProjectOnPlane(targetPos - position, Vector3.up).normalized);
			}
		}

		private void FaceDirection(Vector3 facePosition)
		{
			base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(Vector3.RotateTowards(base.transform.forward, facePosition, turnSpeed * Time.deltaTime * ((float)Math.PI / 180f), 0f), Vector3.up), Vector3.up);
		}

		public void TakeDamage(float damage)
		{
			toughness -= damage;
			if (toughness <= 0f)
			{
				Die();
			}
		}

		public void Die()
		{
			SetState(WanderState.Dead);
		}

		public void SetPeaceTime(bool peace)
		{
			if (peace)
			{
				dominance = 0;
				scent = 0f;
				aggression = 0f;
			}
			else
			{
				dominance = originalDominance;
				scent = originalScent;
				aggression = originalAggression;
			}
		}

		private void UpdateAI()
		{
			if (CurrentState == WanderState.Dead)
			{
				Debug.LogError("Trying to update the AI of a dead animal, something probably went wrong somewhere.");
				return;
			}
			Vector3 position = base.transform.position;
			primaryPursuer = null;
			if (awareness > 0f)
			{
				float num = awareness;
				if (allAnimals.Count > 0)
				{
					foreach (Common_WanderScript allAnimal in allAnimals)
					{
						if ((!(allAnimal.primaryPrey != this) || !(allAnimal.attackTarget != this)) && allAnimal.CurrentState != WanderState.Dead)
						{
							float num2 = Vector3.Distance(position, allAnimal.transform.position);
							if ((!(allAnimal.attackTarget != this) || !allAnimal.stealthy) && allAnimal.dominance > dominance && !(num2 > num))
							{
								num = num2;
								primaryPursuer = allAnimal;
							}
						}
					}
				}
			}
			bool flag = false;
			if ((bool)primaryPrey)
			{
				if (primaryPrey.CurrentState == WanderState.Dead)
				{
					primaryPrey = null;
				}
				else if (Vector3.Distance(position, primaryPrey.transform.position) > scent)
				{
					primaryPrey = null;
				}
				else
				{
					flag = true;
				}
			}
			if (!primaryPrey)
			{
				primaryPrey = null;
				if (dominance > 0 && attackingStates.Length != 0)
				{
					float num3 = aggression * 0.01f;
					num3 *= num3;
					float num4 = scent;
					foreach (Common_WanderScript allAnimal2 in allAnimals)
					{
						if (allAnimal2.CurrentState == WanderState.Dead)
						{
							Debug.LogError(string.Format("Dead animal found: {0}", allAnimal2.gameObject.name));
						}
						if (allAnimal2 == this || (allAnimal2.species == species && !territorial) || allAnimal2.dominance > dominance || allAnimal2.stealthy || nonAgressiveTowards.Contains(allAnimal2.species) || UnityEngine.Random.Range(0f, 0.99999f) >= num3)
						{
							continue;
						}
						Vector3 position2 = allAnimal2.transform.position;
						if (!IsValidLocation(position2))
						{
							continue;
						}
						float num5 = Vector3.Distance(position, position2);
						if (!(num5 > num4))
						{
							if (logChanges)
							{
								Debug.Log(string.Format("{0}: Found prey ({1}), chasing.", base.gameObject.name, allAnimal2.gameObject.name));
							}
							num4 = num5;
							primaryPrey = allAnimal2;
						}
					}
				}
			}
			bool flag2 = false;
			if ((bool)primaryPrey)
			{
				if ((flag && stamina > 0f) || stamina > MinimumStaminaForAggression)
				{
					flag2 = true;
				}
				else
				{
					primaryPrey = null;
				}
			}
			bool flag3 = false;
			if ((bool)primaryPursuer && !flag2 && stamina > MinimumStaminaForFlee)
			{
				flag3 = true;
			}
			bool flag4 = false;
			bool flag5 = flag2 && Vector3.Distance(position, primaryPrey.transform.position) < CalcAttackRange(primaryPrey);
			bool flag6 = flag3 && Vector3.Distance(position, primaryPursuer.transform.position) < CalcAttackRange(primaryPursuer);
			if (flag6)
			{
				attackTarget = primaryPursuer;
			}
			else if (flag5)
			{
				attackTarget = primaryPrey;
				if (!attackTarget.attackTarget == (bool)this)
				{
					flag4 = true;
				}
			}
			else
			{
				attackTarget = null;
			}
			int num6;
			if (attackingStates.Length != 0)
			{
				num6 = ((flag5 || flag6) ? 1 : 0);
				if (num6 != 0)
				{
					SetState(WanderState.Attack);
					goto IL_040b;
				}
			}
			else
			{
				num6 = 0;
			}
			if (flag2)
			{
				SetState(WanderState.Chase);
			}
			else if (flag3)
			{
				SetState(WanderState.Evade);
			}
			else if (CurrentState != 0 && CurrentState != WanderState.Wander)
			{
				SetState(WanderState.Idle);
			}
			goto IL_040b;
			IL_040b:
			if (((uint)num6 & (flag4 ? 1u : 0u)) != 0)
			{
				attackTarget.forceUpdate = true;
			}
		}

		private bool IsValidLocation(Vector3 targetPosition)
		{
			if (!constainedToWanderZone)
			{
				return true;
			}
			return Vector3.Distance(startPosition, targetPosition) < wanderZone;
		}

		private float CalcAttackRange(Common_WanderScript other)
		{
			float num = (navMeshAgent ? navMeshAgent.radius : characterController.radius);
			float num2 = (other.navMeshAgent ? other.navMeshAgent.radius : other.characterController.radius);
			return attackReach + num + num2;
		}

		private void SetState(WanderState state)
		{
			if (CurrentState == WanderState.Dead)
			{
				Debug.LogError("Attempting to set a state to a dead animal.");
				return;
			}
			CurrentState = state;
			switch (CurrentState)
			{
			case WanderState.Idle:
				HandleBeginIdle();
				break;
			case WanderState.Chase:
				HandleBeginChase();
				break;
			case WanderState.Evade:
				HandleBeginEvade();
				break;
			case WanderState.Attack:
				HandleBeginAttack();
				break;
			case WanderState.Dead:
				HandleBeginDeath();
				break;
			case WanderState.Wander:
				HandleBeginWander();
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		private void ClearAnimatorBools()
		{
			IdleState[] array = idleStates;
			foreach (IdleState idleState in array)
			{
				TrySetBool(idleState.animationBool, false);
			}
			MovementState[] array2 = movementStates;
			foreach (MovementState movementState in array2)
			{
				TrySetBool(movementState.animationBool, false);
			}
			AIState[] array3 = attackingStates;
			foreach (AIState aIState in array3)
			{
				TrySetBool(aIState.animationBool, false);
			}
			array3 = deathStates;
			foreach (AIState aIState2 in array3)
			{
				TrySetBool(aIState2.animationBool, false);
			}
		}

		private void TrySetBool(string parameterName, bool value)
		{
			if (!string.IsNullOrEmpty(parameterName) && (logChanges || animatorParameters.Contains(parameterName)))
			{
				animator.SetBool(parameterName, value);
			}
		}

		private void HandleBeginDeath()
		{
			ClearAnimatorBools();
			if (deathStates.Length != 0)
			{
				TrySetBool(deathStates[UnityEngine.Random.Range(0, deathStates.Length)].animationBool, true);
			}
			deathEvent.Invoke();
			if ((bool)navMeshAgent && navMeshAgent.isOnNavMesh)
			{
				navMeshAgent.destination = base.transform.position;
			}
			base.enabled = false;
		}

		private void HandleBeginAttack()
		{
			int num = UnityEngine.Random.Range(0, attackingStates.Length);
			turnSpeed = 120f;
			ClearAnimatorBools();
			TrySetBool(attackingStates[num].animationBool, true);
			attackingEvent.Invoke();
		}

		private void HandleBeginEvade()
		{
			SetMoveFast();
			movementEvent.Invoke();
		}

		private void HandleBeginChase()
		{
			SetMoveFast();
			movementEvent.Invoke();
		}

		private void SetMoveFast()
		{
			MovementState movementState = null;
			float num = 0f;
			MovementState[] array = movementStates;
			foreach (MovementState movementState2 in array)
			{
				float num2 = movementState2.moveSpeed;
				if (num2 > num)
				{
					movementState = movementState2;
					num = num2;
				}
			}
			turnSpeed = movementState.turnSpeed;
			moveSpeed = num;
			ClearAnimatorBools();
			TrySetBool(movementState.animationBool, true);
		}

		private void SetMoveSlow()
		{
			MovementState movementState = null;
			float num = float.MaxValue;
			MovementState[] array = movementStates;
			foreach (MovementState movementState2 in array)
			{
				float num2 = movementState2.moveSpeed;
				if (num2 < num)
				{
					movementState = movementState2;
					num = num2;
				}
			}
			turnSpeed = movementState.turnSpeed;
			moveSpeed = num;
			ClearAnimatorBools();
			TrySetBool(movementState.animationBool, true);
		}

		private void HandleBeginIdle()
		{
			primaryPrey = null;
			int num = UnityEngine.Random.Range(0, totalIdleStateWeight);
			int num2 = 0;
			IdleState[] array = idleStates;
			foreach (IdleState idleState in array)
			{
				num2 += idleState.stateWeight;
				if (num <= num2)
				{
					idleUpdateTime = Time.time + UnityEngine.Random.Range(idleState.minStateTime, idleState.maxStateTime);
					ClearAnimatorBools();
					TrySetBool(idleState.animationBool, true);
					moveSpeed = 0f;
					break;
				}
			}
			idleEvent.Invoke();
		}

		private void HandleBeginWander()
		{
			primaryPrey = null;
			Vector3 vector = UnityEngine.Random.insideUnitSphere * wanderZone;
			Vector3 targetPos = startPosition + vector;
			ValidatePosition(ref targetPos);
			wanderTarget = targetPos;
			SetMoveSlow();
		}

		private void ValidatePosition(ref Vector3 targetPos)
		{
			if ((bool)navMeshAgent)
			{
				NavMeshHit hit;
				if (!NavMesh.SamplePosition(targetPos, out hit, float.PositiveInfinity, 1 << NavMesh.GetAreaFromName("Walkable")))
				{
					Debug.LogError("Unable to sample nav mesh. Please ensure there's a Nav Mesh layer with the name Walkable");
					base.enabled = false;
				}
				else
				{
					targetPos = hit.position;
				}
			}
		}

		private IEnumerator RandomStartingDelay()
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(0f, 2f));
			started = true;
			StartCoroutine(ConstantTicking(UnityEngine.Random.Range(0.7f, 1f)));
		}

		private IEnumerator ConstantTicking(float delay)
		{
			while (true)
			{
				UpdateAI();
				yield return new WaitForSeconds(delay);
			}
		}

		[ContextMenu("This will delete any states you have set, and replace them with the default ones, you can't undo!")]
		public void BasicWanderSetUp()
		{
			MovementState movementState = new MovementState();
			MovementState movementState2 = new MovementState();
			IdleState idleState = new IdleState();
			AIState aIState = new AIState();
			AIState aIState2 = new AIState();
			movementState.stateName = "Walking";
			movementState.animationBool = "isWalking";
			movementState2.stateName = "Running";
			movementState2.animationBool = "isRunning";
			movementStates = new MovementState[2];
			movementStates[0] = movementState;
			movementStates[1] = movementState2;
			idleState.stateName = "Idle";
			idleState.animationBool = "isIdling";
			idleStates = new IdleState[1];
			idleStates[0] = idleState;
			aIState.stateName = "Attacking";
			aIState.animationBool = "isAttacking";
			attackingStates = new AIState[1];
			attackingStates[0] = aIState;
			aIState2.stateName = "Dead";
			aIState2.animationBool = "isDead";
			deathStates = new AIState[1];
			deathStates[0] = aIState2;
		}
	}
}
