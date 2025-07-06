using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ThunderRoad
{
	// Token: 0x0200024E RID: 590
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Brain.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Brain")]
	[RequireComponent(typeof(NavMeshAgent))]
	public class Brain : ThunderBehaviour
	{
		// Token: 0x17000183 RID: 387
		// (get) Token: 0x060018AD RID: 6317 RVA: 0x000A30FE File Offset: 0x000A12FE
		// (set) Token: 0x060018AE RID: 6318 RVA: 0x000A3106 File Offset: 0x000A1306
		public Creature lastTarget { get; protected set; }

		// Token: 0x14000090 RID: 144
		// (add) Token: 0x060018AF RID: 6319 RVA: 0x000A3110 File Offset: 0x000A1310
		// (remove) Token: 0x060018B0 RID: 6320 RVA: 0x000A3148 File Offset: 0x000A1348
		public event Action<Brain.State> OnStateChangeEvent;

		// Token: 0x14000091 RID: 145
		// (add) Token: 0x060018B1 RID: 6321 RVA: 0x000A3180 File Offset: 0x000A1380
		// (remove) Token: 0x060018B2 RID: 6322 RVA: 0x000A31B8 File Offset: 0x000A13B8
		public event Brain.PushEvent OnPushEvent;

		// Token: 0x14000092 RID: 146
		// (add) Token: 0x060018B3 RID: 6323 RVA: 0x000A31F0 File Offset: 0x000A13F0
		// (remove) Token: 0x060018B4 RID: 6324 RVA: 0x000A3228 File Offset: 0x000A1428
		public event Brain.AttackEvent OnAttackEvent;

		// Token: 0x060018B5 RID: 6325 RVA: 0x000A325D File Offset: 0x000A145D
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				this.navMeshAgent = base.GetComponent<NavMeshAgent>();
				if (this.navMeshAgent)
				{
					this.navMeshAgent.enabled = false;
				}
			}
		}

		// Token: 0x060018B6 RID: 6326 RVA: 0x000A3299 File Offset: 0x000A1499
		public void OnCreatureEnable()
		{
			if (this.instance != null)
			{
				this.instance.Start();
				this.instance.Update(true);
			}
		}

		// Token: 0x060018B7 RID: 6327 RVA: 0x000A32BA File Offset: 0x000A14BA
		public void OnCreatureDisable()
		{
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.Stop();
		}

		// Token: 0x060018B8 RID: 6328 RVA: 0x000A32CC File Offset: 0x000A14CC
		protected virtual void Awake()
		{
			if (!Brain.hashInitialized)
			{
				Brain.hashAim = Animator.StringToHash("Aim");
				Brain.hashBlock = Animator.StringToHash("Block");
				Brain.hashHitDirX = Animator.StringToHash("HitDirX");
				Brain.hashHitDirY = Animator.StringToHash("HitDirY");
				Brain.hashIsReloading = Animator.StringToHash("IsReloading");
				Brain.hashIsShooting = Animator.StringToHash("IsShooting");
				Brain.hashElectrocute = Animator.StringToHash("Electrocute");
				Brain.hashChoke = Animator.StringToHash("Choke");
				Brain.hashCarry = Animator.StringToHash("Carry");
				Brain.hashReload = Animator.StringToHash("Reload");
				Brain.hashGrabRock = Animator.StringToHash("GrabRock");
				Brain.hashThrowOverhand = Animator.StringToHash("ThrowOverhand");
				Brain.hashShoot = Animator.StringToHash("Shoot");
				Brain.hashHit = Animator.StringToHash("Hit");
				Brain.hashIsCastingLeft = Animator.StringToHash("IsCastingLeft");
				Brain.hashIsCastingRight = Animator.StringToHash("IsCastingRight");
				Brain.hashCast = Animator.StringToHash("Cast");
				Brain.hashCastSide = Animator.StringToHash("CastSide");
				Brain.hashCastTime = Animator.StringToHash("CastTime");
				Brain.hashParryMagic = Animator.StringToHash("ParryMagic");
				Brain.hashHitType = Animator.StringToHash("HitType");
				Brain.hashInjured = Animator.StringToHash("Injured");
				Brain.hashDodge = Animator.StringToHash("Dodge");
				Brain.hashDodgeType = Animator.StringToHash("DodgeType");
				Brain.hashDodgeSpeed = Animator.StringToHash("DodgeSpeed");
				Brain.hashCastCurve = Animator.StringToHash("CastCurve");
				Brain.hashSwapHands = Animator.StringToHash("SwapLeftToRight");
				Brain.hashGrappleEscape = Animator.StringToHash("GrappleEscape");
				Brain.hashInitialized = true;
			}
			this.navMeshAgent = base.GetComponent<NavMeshAgent>();
			this.navMeshAgent.updatePosition = false;
			this.navMeshAgent.updateRotation = false;
			this.navMeshAgent.speed = 0f;
			this.navMeshAgent.stoppingDistance = 0f;
			this.navMeshAgent.autoBraking = false;
			this.slowZones = new List<Zone>();
		}

		// Token: 0x060018B9 RID: 6329 RVA: 0x000A34E8 File Offset: 0x000A16E8
		public void Init(Creature creature)
		{
			this.creature = creature;
			creature.ragdoll.OnStateChange += this.OnRagdollStateChange;
			creature.OnZoneEvent += this.OnZoneEvent;
			creature.OnDespawnEvent += this.OnCreatureDespawn;
		}

		// Token: 0x060018BA RID: 6330 RVA: 0x000A3538 File Offset: 0x000A1738
		public virtual IEnumerator LoadCoroutine(string brainId)
		{
			BrainData brainData;
			if (Catalog.TryGetData<BrainData>(brainId, out brainData, true))
			{
				BrainData brainData2 = this.instance;
				if (brainData2 != null)
				{
					brainData2.Unload();
				}
				this.instance = brainData.TakeFromPool();
				if (this.instance == null)
				{
					Brain.<>c__DisplayClass76_0 CS$<>8__locals1 = new Brain.<>c__DisplayClass76_0();
					CS$<>8__locals1.task = brainData.CloneJsonAsync<BrainData>();
					yield return new WaitUntil(() => CS$<>8__locals1.task.IsCompleted);
					this.instance = CS$<>8__locals1.task.Result;
					CS$<>8__locals1 = null;
				}
				if (this.instance != null)
				{
					yield return this.instance.LoadCoroutine(this.creature);
					this.instance.Start();
				}
				else
				{
					Debug.LogError("BrainData instance is null");
				}
			}
			yield break;
		}

		// Token: 0x060018BB RID: 6331 RVA: 0x000A3550 File Offset: 0x000A1750
		public virtual void Load(string brainId)
		{
			BrainData brainData;
			if (Catalog.TryGetData<BrainData>(brainId, out brainData, true))
			{
				BrainData brainData2 = this.instance;
				if (brainData2 != null)
				{
					brainData2.Unload();
				}
				this.instance = brainData.TakeFromPool();
				if (this.instance == null)
				{
					this.instance = brainData.Instantiate();
				}
				if (this.instance != null)
				{
					this.instance.Load(this.creature);
					this.instance.Start();
					return;
				}
				Debug.LogError("BrainData instance is null");
			}
		}

		// Token: 0x060018BC RID: 6332 RVA: 0x000A35C8 File Offset: 0x000A17C8
		public virtual void Stop()
		{
			this.isIncapacitated = false;
			this.isUnconscious = false;
			if (this.instance != null)
			{
				this.instance.Stop();
				this.ClearNoStandUpModifiers();
			}
		}

		// Token: 0x060018BD RID: 6333 RVA: 0x000A35F1 File Offset: 0x000A17F1
		public void ResetBrain()
		{
			BrainData brainData = this.instance;
			if (brainData != null)
			{
				BehaviorTreeData tree = brainData.tree;
				if (tree != null)
				{
					tree.Reset();
				}
			}
			this.isMuffled = false;
		}

		// Token: 0x17000184 RID: 388
		// (get) Token: 0x060018BE RID: 6334 RVA: 0x000A3616 File Offset: 0x000A1816
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update | ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x060018BF RID: 6335 RVA: 0x000A3619 File Offset: 0x000A1819
		protected internal override void ManagedFixedUpdate()
		{
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.FixedUpdate();
		}

		// Token: 0x060018C0 RID: 6336 RVA: 0x000A362C File Offset: 0x000A182C
		protected internal override void ManagedUpdate()
		{
			this.isDodging = (this.creature.animator.GetInteger(Brain.hashDodgeType) > 0);
			bool flag = this.currentTarget != null;
			if ((flag && this.targetAcquireTime == null) || this.currentTarget != this.lastTarget)
			{
				this.targetAcquireTime = new float?(Time.time);
				this.lastTarget = this.currentTarget;
			}
			if (!flag && this.targetAcquireTime != null)
			{
				this.targetAcquireTime = null;
			}
			if (this.navMeshAgent.enabled)
			{
				this.navMeshAgent.nextPosition = this.creature.transform.position;
			}
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.Update(false);
		}

		// Token: 0x060018C1 RID: 6337 RVA: 0x000A36F6 File Offset: 0x000A18F6
		protected internal override void ManagedLateUpdate()
		{
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.LateUpdate();
		}

		// Token: 0x060018C2 RID: 6338 RVA: 0x000A3708 File Offset: 0x000A1908
		public void SetState(Brain.State newState)
		{
			if (this.state != newState)
			{
				Action<Brain.State> onStateChangeEvent = this.OnStateChangeEvent;
				if (onStateChangeEvent != null)
				{
					onStateChangeEvent(newState);
				}
				EventManager.InvokeCreatureBrainStateChange(this.creature, newState);
				this.state = newState;
			}
		}

		// Token: 0x060018C3 RID: 6339 RVA: 0x000A3738 File Offset: 0x000A1938
		public void InvokePushEvent(Creature.PushType type, Brain.Stagger stagger)
		{
			Brain.PushEvent onPushEvent = this.OnPushEvent;
			if (onPushEvent == null)
			{
				return;
			}
			onPushEvent(type, stagger);
		}

		// Token: 0x060018C4 RID: 6340 RVA: 0x000A374C File Offset: 0x000A194C
		public void InvokeAttackEvent(Brain.AttackType attackType, bool strong, Creature target)
		{
			Brain.AttackEvent onAttackEvent = this.OnAttackEvent;
			if (onAttackEvent == null)
			{
				return;
			}
			onAttackEvent(attackType, strong, target);
		}

		// Token: 0x060018C5 RID: 6341 RVA: 0x000A3761 File Offset: 0x000A1961
		protected void OnCreatureDespawn(EventTime eventTime)
		{
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.Unload();
		}

		// Token: 0x060018C6 RID: 6342 RVA: 0x000A3773 File Offset: 0x000A1973
		protected virtual void OnRagdollStateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicStateChange, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd && this.creature.initialized && this.creature.loaded)
			{
				this.navMeshAgent.enabled = (newState == Ragdoll.State.Standing || newState == Ragdoll.State.Kinematic || newState == Ragdoll.State.NoPhysic);
			}
		}

		// Token: 0x060018C7 RID: 6343 RVA: 0x000A37B0 File Offset: 0x000A19B0
		protected void OnZoneEvent(Zone zone, bool enter)
		{
			if (!zone.navSpeedModifier)
			{
				return;
			}
			if (enter)
			{
				if (!this.slowZones.Contains(zone))
				{
					this.slowZones.Add(zone);
					return;
				}
			}
			else if (this.slowZones.Contains(zone))
			{
				this.slowZones.Remove(zone);
			}
		}

		// Token: 0x060018C8 RID: 6344 RVA: 0x000A3800 File Offset: 0x000A1A00
		public virtual int GetFreeAvoidancePriority(int avoidancePriority)
		{
			foreach (Creature creature in Creature.allActive)
			{
				if (creature.state != Creature.State.Dead && creature.brain && creature.brain.navMeshAgent.avoidancePriority == avoidancePriority)
				{
					avoidancePriority = this.GetFreeAvoidancePriority(avoidancePriority + 1);
				}
			}
			return avoidancePriority;
		}

		// Token: 0x060018C9 RID: 6345 RVA: 0x000A3880 File Offset: 0x000A1A80
		public float GetHorizontalDistance(Transform target)
		{
			return this.GetHorizontalDistance(target.position);
		}

		// Token: 0x060018CA RID: 6346 RVA: 0x000A388E File Offset: 0x000A1A8E
		public float GetHorizontalDistance(Vector3 targetPosition)
		{
			return Vector3.Distance(this.creature.transform.position.ToXZ(), targetPosition.ToXZ());
		}

		// Token: 0x060018CB RID: 6347 RVA: 0x000A38B0 File Offset: 0x000A1AB0
		public float GetDistance(Transform target)
		{
			return this.GetDistance(target.position);
		}

		// Token: 0x060018CC RID: 6348 RVA: 0x000A38BE File Offset: 0x000A1ABE
		public float GetDistance(Vector3 targetPosition)
		{
			return Vector3.Distance(this.creature.transform.position, targetPosition);
		}

		// Token: 0x060018CD RID: 6349 RVA: 0x000A38D6 File Offset: 0x000A1AD6
		public float GetHeight(Transform target)
		{
			return target.position.y - this.creature.transform.position.y;
		}

		// Token: 0x060018CE RID: 6350 RVA: 0x000A38FC File Offset: 0x000A1AFC
		public float GetHorizontalAngle(Transform target)
		{
			return Vector3.Angle(this.creature.transform.forward.ToXZ(), target.position.ToXZ() - this.creature.transform.position.ToXZ());
		}

		// Token: 0x060018CF RID: 6351 RVA: 0x000A3948 File Offset: 0x000A1B48
		public bool CanSight(Vector3 targetPosition, float sightThickness, float minDistance, float maxDistance, bool showDebugLines = false)
		{
			return Brain.InSightRange(this.creature.centerEyes.position, targetPosition, sightThickness, minDistance, maxDistance, showDebugLines);
		}

		// Token: 0x060018D0 RID: 6352 RVA: 0x000A396C File Offset: 0x000A1B6C
		public bool CanSight(Creature targetCreature, bool useDetectionMaxDistance = false, bool showDebugLines = false)
		{
			RagdollPart sightedRagdollPart;
			return this.CanSight(targetCreature, out sightedRagdollPart, useDetectionMaxDistance, showDebugLines);
		}

		// Token: 0x060018D1 RID: 6353 RVA: 0x000A398C File Offset: 0x000A1B8C
		public bool CanSight(Creature targetCreature, out RagdollPart sightedRagdollPart, bool useDetectionMaxDistance = false, bool showDebugLines = false)
		{
			BrainData brainData = targetCreature.brain.instance;
			BrainModuleSightable moduleSightable = (brainData != null) ? brainData.GetModule<BrainModuleSightable>(true) : null;
			if (moduleSightable != null && moduleSightable.IsSightable(this.creature.centerEyes.position, moduleSightable.sightThickness, 0f, useDetectionMaxDistance ? moduleSightable.sightDetectionMaxDistance : moduleSightable.sightMaxDistance, out sightedRagdollPart, showDebugLines))
			{
				return true;
			}
			sightedRagdollPart = null;
			return false;
		}

		// Token: 0x060018D2 RID: 6354 RVA: 0x000A39F4 File Offset: 0x000A1BF4
		public bool CanSight(Creature targetCreature, float sightThickness, float minDistance, float maxDistance, bool showDebugLines = false)
		{
			RagdollPart sightedRagdollPart;
			return this.CanSight(targetCreature, sightThickness, minDistance, maxDistance, out sightedRagdollPart, showDebugLines);
		}

		// Token: 0x060018D3 RID: 6355 RVA: 0x000A3A18 File Offset: 0x000A1C18
		public bool CanSight(Creature targetCreature, float sightThickness, float minDistance, float maxDistance, out RagdollPart sightedRagdollPart, bool showDebugLines = false)
		{
			BrainData brainData = targetCreature.brain.instance;
			BrainModuleSightable moduleSightable = (brainData != null) ? brainData.GetModule<BrainModuleSightable>(true) : null;
			if (moduleSightable != null && moduleSightable.IsSightable(this.creature.centerEyes.position, sightThickness, minDistance, maxDistance, out sightedRagdollPart, showDebugLines))
			{
				return true;
			}
			sightedRagdollPart = null;
			return false;
		}

		// Token: 0x060018D4 RID: 6356 RVA: 0x000A3A68 File Offset: 0x000A1C68
		public static bool InSightRange(Vector3 fromPosition, Vector3 toPosition, float sightThickness, float minDistance, float maxDistance, bool showDebugLines = false)
		{
			float targetDistance = Vector3.Distance(fromPosition, toPosition);
			if (targetDistance < minDistance)
			{
				return false;
			}
			if (targetDistance > maxDistance)
			{
				return false;
			}
			Vector3 rayDirection = (toPosition - fromPosition).normalized;
			LayerMask sightObstacleMask = 1 << GameManager.GetLayer(LayerName.Default) | 1 << GameManager.GetLayer(LayerName.NoLocomotion);
			RaycastHit raycastHit;
			if (!((sightThickness > 0f) ? Physics.SphereCast(fromPosition, sightThickness, rayDirection, out raycastHit, maxDistance, sightObstacleMask) : Physics.Raycast(fromPosition, rayDirection, out raycastHit, maxDistance, sightObstacleMask)))
			{
				if (showDebugLines)
				{
					Debug.DrawRay(fromPosition, rayDirection * targetDistance, Color.green);
				}
				return true;
			}
			if (raycastHit.distance < targetDistance)
			{
				if (showDebugLines)
				{
					Debug.DrawLine(fromPosition, raycastHit.point, Color.red);
				}
				return false;
			}
			if (showDebugLines)
			{
				Debug.DrawRay(fromPosition, rayDirection * targetDistance, Color.green);
			}
			return true;
		}

		// Token: 0x060018D5 RID: 6357 RVA: 0x000A3B39 File Offset: 0x000A1D39
		public void AddNoStandUpModifier(object handler)
		{
			if (!this.noStandupModifiers.Contains(handler))
			{
				this.noStandupModifiers.Add(handler);
			}
		}

		// Token: 0x060018D6 RID: 6358 RVA: 0x000A3B55 File Offset: 0x000A1D55
		public void RemoveNoStandUpModifier(object handler)
		{
			if (this.noStandupModifiers.Contains(handler))
			{
				this.noStandupModifiers.Remove(handler);
			}
		}

		// Token: 0x060018D7 RID: 6359 RVA: 0x000A3B72 File Offset: 0x000A1D72
		public void ClearNoStandUpModifiers()
		{
			this.noStandupModifiers.Clear();
		}

		// Token: 0x060018D8 RID: 6360 RVA: 0x000A3B7F File Offset: 0x000A1D7F
		protected void OnDrawGizmosSelected()
		{
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.OnDrawGizmosSelected();
		}

		// Token: 0x060018D9 RID: 6361 RVA: 0x000A3B91 File Offset: 0x000A1D91
		protected void OnDrawGizmos()
		{
			BrainData brainData = this.instance;
			if (brainData == null)
			{
				return;
			}
			brainData.OnDrawGizmos();
		}

		// Token: 0x040017BB RID: 6075
		[NonSerialized]
		public bool canDamage;

		// Token: 0x040017BC RID: 6076
		[NonSerialized]
		public bool isElectrocuted;

		// Token: 0x040017BD RID: 6077
		[NonSerialized]
		public bool isDying;

		// Token: 0x040017BE RID: 6078
		[NonSerialized]
		public bool isAttacking;

		// Token: 0x040017BF RID: 6079
		[NonSerialized]
		public bool isShooting;

		// Token: 0x040017C0 RID: 6080
		[NonSerialized]
		public bool isCasting;

		// Token: 0x040017C1 RID: 6081
		[NonSerialized]
		public bool isChoke;

		// Token: 0x040017C2 RID: 6082
		[NonSerialized]
		public bool isMuffled;

		// Token: 0x040017C3 RID: 6083
		[NonSerialized]
		public bool isCarried;

		// Token: 0x040017C4 RID: 6084
		[NonSerialized]
		public bool isDodging;

		// Token: 0x040017C5 RID: 6085
		[NonSerialized]
		public bool isDefending;

		// Token: 0x040017C6 RID: 6086
		[NonSerialized]
		public bool isIncapacitated;

		// Token: 0x040017C7 RID: 6087
		[NonSerialized]
		public bool isUnconscious;

		// Token: 0x040017C8 RID: 6088
		[NonSerialized]
		public bool onNavmesh;

		// Token: 0x040017C9 RID: 6089
		public bool isManuallyControlled;

		// Token: 0x040017CA RID: 6090
		[NonSerialized]
		public Creature creature;

		// Token: 0x040017CB RID: 6091
		[NonSerialized]
		public NavMeshAgent navMeshAgent;

		// Token: 0x040017CC RID: 6092
		public static int hashAim;

		// Token: 0x040017CD RID: 6093
		public static int hashBlock;

		// Token: 0x040017CE RID: 6094
		public static int hashHit;

		// Token: 0x040017CF RID: 6095
		public static int hashHitDirX;

		// Token: 0x040017D0 RID: 6096
		public static int hashHitDirY;

		// Token: 0x040017D1 RID: 6097
		public static int hashElectrocute;

		// Token: 0x040017D2 RID: 6098
		public static int hashChoke;

		// Token: 0x040017D3 RID: 6099
		public static int hashCarry;

		// Token: 0x040017D4 RID: 6100
		public static int hashIsCastingLeft;

		// Token: 0x040017D5 RID: 6101
		public static int hashIsCastingRight;

		// Token: 0x040017D6 RID: 6102
		public static int hashCast;

		// Token: 0x040017D7 RID: 6103
		public static int hashCastCurve;

		// Token: 0x040017D8 RID: 6104
		public static int hashCastSide;

		// Token: 0x040017D9 RID: 6105
		public static int hashCastTime;

		// Token: 0x040017DA RID: 6106
		public static int hashIsReloading;

		// Token: 0x040017DB RID: 6107
		public static int hashIsShooting;

		// Token: 0x040017DC RID: 6108
		public static int hashShoot;

		// Token: 0x040017DD RID: 6109
		public static int hashReload;

		// Token: 0x040017DE RID: 6110
		public static int hashGrabRock;

		// Token: 0x040017DF RID: 6111
		public static int hashThrowOverhand;

		// Token: 0x040017E0 RID: 6112
		public static int hashParryMagic;

		// Token: 0x040017E1 RID: 6113
		public static int hashHitType;

		// Token: 0x040017E2 RID: 6114
		public static int hashInjured;

		// Token: 0x040017E3 RID: 6115
		public static int hashDodge;

		// Token: 0x040017E4 RID: 6116
		public static int hashDodgeType;

		// Token: 0x040017E5 RID: 6117
		public static int hashDodgeSpeed;

		// Token: 0x040017E6 RID: 6118
		public static int hashSwapHands;

		// Token: 0x040017E7 RID: 6119
		public static int hashGrappleEscape;

		// Token: 0x040017E8 RID: 6120
		public static bool hashInitialized;

		// Token: 0x040017E9 RID: 6121
		[NonSerialized]
		public Creature currentTarget;

		// Token: 0x040017EB RID: 6123
		public float? targetAcquireTime;

		// Token: 0x040017EC RID: 6124
		[NonSerialized]
		public Brain.Stagger currentStagger;

		// Token: 0x040017EE RID: 6126
		[NonSerialized]
		public Brain.State state;

		// Token: 0x040017EF RID: 6127
		[NonSerialized]
		public BrainData instance;

		// Token: 0x040017F0 RID: 6128
		[NonSerialized]
		public List<Zone> slowZones;

		// Token: 0x040017F3 RID: 6131
		[NonSerialized]
		public List<object> noStandupModifiers = new List<object>();

		// Token: 0x0200086A RID: 2154
		public enum State
		{
			// Token: 0x0400419A RID: 16794
			Idle,
			// Token: 0x0400419B RID: 16795
			Follow,
			// Token: 0x0400419C RID: 16796
			Patrol,
			// Token: 0x0400419D RID: 16797
			Investigate,
			// Token: 0x0400419E RID: 16798
			Alert,
			// Token: 0x0400419F RID: 16799
			Combat,
			// Token: 0x040041A0 RID: 16800
			Grappled,
			// Token: 0x040041A1 RID: 16801
			Custom
		}

		// Token: 0x0200086B RID: 2155
		// (Invoke) Token: 0x06004027 RID: 16423
		public delegate void PushEvent(Creature.PushType type, Brain.Stagger stagger);

		// Token: 0x0200086C RID: 2156
		// (Invoke) Token: 0x0600402B RID: 16427
		public delegate void AttackEvent(Brain.AttackType attackType, bool strong, Creature target);

		// Token: 0x0200086D RID: 2157
		public enum AttackType
		{
			// Token: 0x040041A3 RID: 16803
			Melee,
			// Token: 0x040041A4 RID: 16804
			Bow,
			// Token: 0x040041A5 RID: 16805
			Cast,
			// Token: 0x040041A6 RID: 16806
			Throw
		}

		// Token: 0x0200086E RID: 2158
		public enum Stagger
		{
			// Token: 0x040041A8 RID: 16808
			None,
			// Token: 0x040041A9 RID: 16809
			LightAndMedium,
			// Token: 0x040041AA RID: 16810
			Full
		}
	}
}
