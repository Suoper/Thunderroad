using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ThunderRoad
{
	// Token: 0x0200025B RID: 603
	public class Golem : GolemController
	{
		// Token: 0x1700019D RID: 413
		// (get) Token: 0x06001A7D RID: 6781 RVA: 0x000B0E54 File Offset: 0x000AF054
		public Golem.CrystalConfig activeCrystalConfig
		{
			get
			{
				switch (this.tier)
				{
				case Golem.Tier.Tier1:
					return this.tier1Config;
				case Golem.Tier.Tier2:
					return this.tier2Config;
				case Golem.Tier.Tier3:
					return this.tier3Config;
				}
				return this.defaultConfig;
			}
		}

		// Token: 0x06001A7E RID: 6782 RVA: 0x000B0E9D File Offset: 0x000AF09D
		public List<ValueDropdownItem<string>> GetAllItemID()
		{
			return Catalog.GetDropdownAllID(Category.Item, "None");
		}

		// Token: 0x1700019E RID: 414
		// (get) Token: 0x06001A7F RID: 6783 RVA: 0x000B0EAA File Offset: 0x000AF0AA
		public bool isProtected
		{
			get
			{
				List<GolemCrystal> list = this.linkedArenaCrystals;
				return ((list != null) ? list.Count : 0) > 0;
			}
		}

		// Token: 0x1700019F RID: 415
		// (get) Token: 0x06001A80 RID: 6784 RVA: 0x000B0EC1 File Offset: 0x000AF0C1
		public int crystalsLeft
		{
			get
			{
				return this.crystals.Count;
			}
		}

		// Token: 0x140000B7 RID: 183
		// (add) Token: 0x06001A81 RID: 6785 RVA: 0x000B0ED0 File Offset: 0x000AF0D0
		// (remove) Token: 0x06001A82 RID: 6786 RVA: 0x000B0F08 File Offset: 0x000AF108
		public event Golem.GolemCrystalBreak OnGolemCrystalBreak;

		// Token: 0x140000B8 RID: 184
		// (add) Token: 0x06001A83 RID: 6787 RVA: 0x000B0F40 File Offset: 0x000AF140
		// (remove) Token: 0x06001A84 RID: 6788 RVA: 0x000B0F78 File Offset: 0x000AF178
		public event Golem.GolemCrystalBreak OnArenaCrystalBreak;

		// Token: 0x140000B9 RID: 185
		// (add) Token: 0x06001A85 RID: 6789 RVA: 0x000B0FB0 File Offset: 0x000AF1B0
		// (remove) Token: 0x06001A86 RID: 6790 RVA: 0x000B0FE4 File Offset: 0x000AF1E4
		public static event Action OnLocalGolemSet;

		// Token: 0x06001A87 RID: 6791 RVA: 0x000B1017 File Offset: 0x000AF217
		public override void SetAwake(bool awake)
		{
			base.SetAwake(awake);
			this.awakeTime = Time.time;
		}

		// Token: 0x06001A88 RID: 6792 RVA: 0x000B102B File Offset: 0x000AF22B
		public virtual void SetAttackTarget(Transform targetTransform)
		{
			if (base.isAwake && !base.isStunned)
			{
				this.LookAt(targetTransform);
			}
			this.attackTarget = targetTransform;
		}

		// Token: 0x06001A89 RID: 6793 RVA: 0x000B104B File Offset: 0x000AF24B
		public void Rampage()
		{
			this.Rampage(this.TargetInMeleeRange() ? RampageType.Melee : RampageType.Ranged);
		}

		// Token: 0x06001A8A RID: 6794 RVA: 0x000B1060 File Offset: 0x000AF260
		public void Rampage(RampageType type = RampageType.Melee)
		{
			if (type == RampageType.None)
			{
				Debug.LogWarning("Can't rampage with type none!");
				return;
			}
			this.StopStun();
			this.StopDeploy();
			this.hitDamageMultiplier = this.rampageDamageMult;
			this.hitForceMultiplier = this.rampageForceMult;
			if (type == RampageType.Melee)
			{
				this.RampageMelee(delegate
				{
					this.hitDamageMultiplier = 1f;
					this.hitForceMultiplier = 1f;
				});
			}
			else
			{
				this.RampageRanged(delegate
				{
					this.hitDamageMultiplier = 1f;
					this.hitForceMultiplier = 1f;
				});
			}
			this.navMeshAgent.updateRotation = false;
		}

		// Token: 0x06001A8B RID: 6795 RVA: 0x000B10D8 File Offset: 0x000AF2D8
		public virtual void Defeat()
		{
			if (this.crystals.IsNullOrEmpty())
			{
				return;
			}
			this.BreakArenaCrystals(this.linkedArenaCrystals.Count);
			this.BreakCrystals(this.crystalsLeft);
			GolemSpawner spawner = this.spawner;
			if (spawner == null)
			{
				return;
			}
			UnityEvent onGolemDefeat = spawner.onGolemDefeat;
			if (onGolemDefeat == null)
			{
				return;
			}
			onGolemDefeat.Invoke();
		}

		// Token: 0x06001A8C RID: 6796 RVA: 0x000B112C File Offset: 0x000AF32C
		public void BreakCrystals(int num = 1)
		{
			if (this.crystals.IsNullOrEmpty())
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				this.crystals[this.crystals.Count - 1].Break();
			}
		}

		// Token: 0x06001A8D RID: 6797 RVA: 0x000B1170 File Offset: 0x000AF370
		public void BreakArenaCrystals(int num = 1)
		{
			if (this.linkedArenaCrystals.IsNullOrEmpty())
			{
				return;
			}
			for (int i = 0; i < num; i++)
			{
				this.linkedArenaCrystals[this.linkedArenaCrystals.Count - 1].Break();
			}
		}

		// Token: 0x06001A8E RID: 6798 RVA: 0x000B11B4 File Offset: 0x000AF3B4
		protected override void OnValidate()
		{
			base.OnValidate();
			if (!this.navMeshAgent)
			{
				this.navMeshAgent = base.GetComponentInChildren<NavMeshAgent>();
			}
			if (!this.golemCrystalRandomizer)
			{
				this.golemCrystalRandomizer = base.GetComponentInChildren<WeakPointRandomizer>();
			}
		}

		// Token: 0x06001A8F RID: 6799 RVA: 0x000B11F0 File Offset: 0x000AF3F0
		protected override void Awake()
		{
			Golem.local = this;
			string optionValue;
			Golem.Tier tier;
			switch (int.Parse((Level.current.options != null && Level.current.options.TryGetValue(LevelOption.GolemTier.Get(), out optionValue)) ? optionValue : "4"))
			{
			case 1:
				tier = Golem.Tier.Tier1;
				break;
			case 2:
				tier = Golem.Tier.Tier2;
				break;
			case 3:
				tier = Golem.Tier.Tier3;
				break;
			case 4:
				tier = Golem.Tier.Any;
				break;
			default:
				tier = this.tier;
				break;
			}
			this.tier = tier;
			Action onLocalGolemSet = Golem.OnLocalGolemSet;
			if (onLocalGolemSet != null)
			{
				onLocalGolemSet();
			}
			EventManager.onPossess += this.OnPlayerPossess;
			this.nextMeleeAttackTime = Time.time;
			base.lastAbilityTime = Time.time;
			this.climbTime = Time.time;
			this.awakeTime = Time.time;
			this.navMeshAgent.updatePosition = false;
			this.navMeshAgent.updateRotation = false;
			this.RandomizeCrystals();
			base.Awake();
			this.shardItemData = Catalog.GetData<ItemData>(this.shardItemId, true);
			if (this.autoDefeatOnStart || this.autoDefeatAndKillOnStart)
			{
				this.DelayedAction(0.5f, new Action(this.Defeat));
				if (this.autoDefeatAndKillOnStart)
				{
					this.DelayedAction(7f, delegate
					{
						Time.timeScale = 0.2f;
						this.Kill();
					});
				}
			}
		}

		// Token: 0x06001A90 RID: 6800 RVA: 0x000B133C File Offset: 0x000AF53C
		public GolemAbilityType GetValidAbilityType()
		{
			if (base.isClimbed && this.allowClimbReact)
			{
				return GolemAbilityType.Climb;
			}
			if (this.allowMelee && this.TargetInMeleeRange())
			{
				return GolemAbilityType.Melee;
			}
			return GolemAbilityType.Ranged;
		}

		// Token: 0x06001A91 RID: 6801 RVA: 0x000B1364 File Offset: 0x000AF564
		public GolemAbility GetRandomAbilityOfType(GolemAbilityType type)
		{
			GolemAbility result;
			if (this.abilities.WeightedFilteredSelectInPlace((GolemAbility ability) => ability.Allow(this) && ability.type == type, (GolemAbility ability) => ability.weight, out result))
			{
				return result;
			}
			if (!this.abilities.WeightedFilteredSelectInPlace((GolemAbility ability) => ability.Allow(this) && ability.type == type, (GolemAbility ability) => ability.weight, out result))
			{
				return null;
			}
			return result;
		}

		// Token: 0x06001A92 RID: 6802 RVA: 0x000B1400 File Offset: 0x000AF600
		private void RandomizeCrystals()
		{
			this.shardsToDrop = UnityEngine.Random.Range((int)this.activeCrystalConfig.minMaxShardsDropped.x, (int)this.activeCrystalConfig.minMaxShardsDropped.y + 1);
			foreach (Transform weakPoint in this.golemCrystalRandomizer.RandomizeWeakPoints(this.activeCrystalConfig.golemCrystals, false).Shuffle<Transform>())
			{
				GolemCrystal crystal = weakPoint.GetComponentInChildren<GolemCrystal>();
				if (crystal == null)
				{
					Debug.LogError("Weak point " + weakPoint.GetPathFrom(weakPoint.root) + " is not a golem crystal!");
				}
				else
				{
					UnityEvent onBreak = crystal.onBreak;
					if (onBreak != null)
					{
						onBreak.AddListener(delegate()
						{
							this.CrystalBreak(crystal);
						});
					}
					this.crystals.Add(crystal);
					Rigidbody crystalRB;
					if (crystal.TryGetComponent<Rigidbody>(out crystalRB))
					{
						this.bodyParts.Add(crystalRB);
					}
				}
			}
			this.RefreshWeakPoints();
		}

		// Token: 0x06001A93 RID: 6803 RVA: 0x000B1534 File Offset: 0x000AF734
		public void RandomizeCrystalProtection()
		{
			if (this.arenaCrystalRandomizer != null)
			{
				int maxLinks = Mathf.Min(this.activeCrystalConfig.golemCrystals, this.activeCrystalConfig.arenaCrystals);
				int index = 0;
				foreach (Transform weakPoint in this.arenaCrystalRandomizer.RandomizeWeakPoints(this.activeCrystalConfig.arenaCrystals, false).Shuffle<Transform>())
				{
					GolemCrystal arenaCrystal = weakPoint.GetComponentInChildren<GolemCrystal>();
					if (arenaCrystal == null)
					{
						Debug.LogError("Arena crystal " + weakPoint.GetPathFrom(weakPoint.root) + " is not a simple breakable!");
					}
					else if (index < maxLinks)
					{
						GolemCrystal crystal = this.crystals[index];
						arenaCrystal.onShieldDisable.AddListener(delegate()
						{
							crystal.linkEffect.gameObject.SetActive(true);
						});
						arenaCrystal.onBreak.AddListener(delegate()
						{
							this.linkedArenaCrystals.Remove(arenaCrystal);
							crystal.DisableShield();
							Golem.GolemCrystalBreak onArenaCrystalBreak = this.OnArenaCrystalBreak;
							if (onArenaCrystalBreak != null)
							{
								onArenaCrystalBreak(arenaCrystal);
							}
							this.Stun(this.activeCrystalConfig.arenaCrystalMaxStun);
						});
						arenaCrystal.EnableShield();
						crystal.linkEffectTarget.SetParentOrigin(arenaCrystal.transform, new Vector3?(Vector3.zero), new Quaternion?(Quaternion.identity), null);
						this.linkedArenaCrystals.Add(arenaCrystal);
						index++;
					}
				}
			}
			for (int i = 0; i < this.crystals.Count; i++)
			{
				this.crystals[i].EnableShield();
			}
		}

		// Token: 0x06001A94 RID: 6804 RVA: 0x000B1708 File Offset: 0x000AF908
		protected override void Start()
		{
			base.Start();
			this.SetAttackTarget(this.attackTarget);
		}

		// Token: 0x06001A95 RID: 6805 RVA: 0x000B171C File Offset: 0x000AF91C
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			base.InvokeRepeating("OnCycle", this.cycleTime, this.cycleTime);
		}

		// Token: 0x06001A96 RID: 6806 RVA: 0x000B173B File Offset: 0x000AF93B
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			base.CancelInvoke("OnCycle");
		}

		// Token: 0x06001A97 RID: 6807 RVA: 0x000B174E File Offset: 0x000AF94E
		private void OnDestroy()
		{
			EventManager.onPossess -= this.OnPlayerPossess;
		}

		// Token: 0x06001A98 RID: 6808 RVA: 0x000B1762 File Offset: 0x000AF962
		protected virtual void OnPlayerPossess(Creature creature, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			this.SetAttackTarget(creature.ragdoll.targetPart.transform);
		}

		// Token: 0x06001A99 RID: 6809 RVA: 0x000B177F File Offset: 0x000AF97F
		protected override IEnumerator WakeCoroutine()
		{
			yield return base.WakeCoroutine();
			for (int i = 0; i < this.crystals.Count; i++)
			{
				bool arenaCrystal = i < this.linkedArenaCrystals.Count;
				if (!arenaCrystal || i <= 0)
				{
					(arenaCrystal ? this.linkedArenaCrystals[i] : this.crystals[i]).DisableShield();
				}
			}
			yield break;
		}

		// Token: 0x06001A9A RID: 6810 RVA: 0x000B178E File Offset: 0x000AF98E
		public void RemoveNextShield(float delay)
		{
			if (this.linkedArenaCrystals.Count > 0)
			{
				if (delay <= 0f)
				{
					this.linkedArenaCrystals[0].DisableShield();
					return;
				}
				this.DelayedAction(delay, delegate
				{
					this.linkedArenaCrystals[0].DisableShield();
				});
			}
		}

		// Token: 0x06001A9B RID: 6811 RVA: 0x000B17CC File Offset: 0x000AF9CC
		public override void RefreshWeakPoints()
		{
			base.RefreshWeakPoints();
			this.weakpoints = new List<Transform>();
			for (int i = 0; i < this.crystals.Count; i++)
			{
				this.weakpoints.Add(this.crystals[i].transform.Find("Target") ?? this.crystals[i].transform);
			}
		}

		// Token: 0x06001A9C RID: 6812 RVA: 0x000B183C File Offset: 0x000AFA3C
		public virtual void TargetPlayer()
		{
			Creature currentCreature = Player.currentCreature;
			Transform transform;
			if (currentCreature == null)
			{
				transform = null;
			}
			else
			{
				Ragdoll ragdoll = currentCreature.ragdoll;
				if (ragdoll == null)
				{
					transform = null;
				}
				else
				{
					RagdollPart targetPart = ragdoll.targetPart;
					transform = ((targetPart != null) ? targetPart.transform : null);
				}
			}
			Transform target = transform;
			if (target == null)
			{
				return;
			}
			this.SetAttackTarget(target);
		}

		// Token: 0x06001A9D RID: 6813 RVA: 0x000B1880 File Offset: 0x000AFA80
		protected internal override void ManagedUpdate()
		{
			base.ManagedUpdate();
			if (base.isKilled)
			{
				return;
			}
			bool targetCloseDistance = this.attackTarget != null && this.attackTarget.position.PointInRadius(base.transform.position + new Vector3(0f, this.stopMoveSphereHeight, 0f), this.stopMoveDistance);
			if (!targetCloseDistance)
			{
				this.climbReactIntensity = 0;
			}
			if (base.isStunned && this.waitStunApproach && targetCloseDistance)
			{
				this.stunEndTime = Time.time + this.activeCrystalConfig.arenaCrystalNearbyStun;
				this.waitStunApproach = false;
			}
			this.navMeshAgent.updateRotation = false;
			if (base.isDefeated && !base.stunInProgress)
			{
				this.Stun(0f, null, delegate()
				{
					this.UnlockFacePlate(true);
				}, null);
				return;
			}
			if (base.isAwake && !base.isBusy && this.attackTarget)
			{
				Vector3 worldDeltaPosition = this.navMeshAgent.nextPosition - base.transform.position;
				worldDeltaPosition.y = 0f;
				float dx = Vector3.Dot(base.transform.right, worldDeltaPosition);
				float dy = Vector3.Dot(base.transform.forward, worldDeltaPosition);
				Vector2 deltaPosition = new Vector2(dx, dy);
				float smooth = Mathf.Min(1f, Time.deltaTime / 0.1f);
				this.smoothDeltaPosition = Vector2.Lerp(this.smoothDeltaPosition, deltaPosition, smooth);
				this.velocity = this.smoothDeltaPosition / Time.deltaTime;
				if (this.navMeshAgent.remainingDistance <= this.navMeshAgent.stoppingDistance)
				{
					this.velocity = Vector2.Lerp(Vector2.zero, this.velocity, this.navMeshAgent.remainingDistance / this.navMeshAgent.stoppingDistance);
				}
				bool navmeshReached = this.navMeshAgent.remainingDistance <= this.navMeshAgent.stoppingDistance;
				bool shouldMove = this.velocity.magnitude > 0.5f && !navmeshReached && !targetCloseDistance;
				bool canSee = base.IsSightable(this.attackTarget, 1000f, this.forwardAngle);
				if (this.navMeshAgent.destination.ToXZ().DistanceSqr(this.attackTarget.position.ToXZ()) > this.navMeshAgent.stoppingDistance)
				{
					shouldMove = (!canSee || !this.eyeTransform.position.ToXZ().PointInRadius(this.attackTarget.position.ToXZ(), this.minHeadToTargetDistance));
				}
				bool bothFeetPlanted = this.animatorEvent.rightFootPlanted && this.animatorEvent.leftFootPlanted;
				if (!shouldMove && !bothFeetPlanted)
				{
					shouldMove = true;
				}
				if (base.isClimbed)
				{
					shouldMove = false;
					if (!this.wasClimbed)
					{
						this.climbTime = Time.time;
					}
					if (Time.time > this.climbTime + (this.climbReactTime - this.climbReactWarningTime) && this.lastReactWarningTime < this.climbTime)
					{
						this.lastReactWarningTime = Time.time;
						this.climbReactWarningAudio.Play();
					}
					this.wasClimbed = true;
				}
				else if (Time.time > this.climbTime + this.resetClimbedTime)
				{
					this.wasClimbed = false;
				}
				this.SetMove(shouldMove);
				if (worldDeltaPosition.magnitude > this.navMeshAgent.radius / 2f)
				{
					base.transform.position = Vector3.Lerp(this.animator.rootPosition, this.navMeshAgent.nextPosition, smooth);
				}
				if (shouldMove && !base.isBusy)
				{
					this.navMeshAgent.updateRotation = !bothFeetPlanted;
				}
			}
			GolemAbility currentAbility = this.currentAbility;
			if (currentAbility != null)
			{
				currentAbility.OnUpdate();
			}
			if (base.isAwake && this.attackTarget)
			{
				base.UpdateSwingEffects();
			}
		}

		// Token: 0x06001A9E RID: 6814 RVA: 0x000B1C59 File Offset: 0x000AFE59
		public override void OnAnimatorMove()
		{
			base.OnAnimatorMove();
			if (base.isKilled)
			{
				return;
			}
			this.navMeshAgent.nextPosition = this.animator.rootPosition;
		}

		// Token: 0x06001A9F RID: 6815 RVA: 0x000B1C80 File Offset: 0x000AFE80
		public override void Kill()
		{
			this.Defeat();
			base.Kill();
			GolemSpawner spawner = this.spawner;
			if (spawner == null)
			{
				return;
			}
			UnityEvent onGolemKill = spawner.onGolemKill;
			if (onGolemKill == null)
			{
				return;
			}
			onGolemKill.Invoke();
		}

		// Token: 0x06001AA0 RID: 6816 RVA: 0x000B1CA8 File Offset: 0x000AFEA8
		public override void Stun(float duration = 0f, Action onStunStart = null, Action onStunnedBegin = null, Action onStunnedEnd = null)
		{
			base.Stun(duration, onStunStart, onStunnedBegin, onStunnedEnd);
			this.crystalsBrokenDuringStun = 0;
			this.navMeshAgent.updateRotation = false;
			this.LookAt(null);
			GolemSpawner spawner = this.spawner;
			if (spawner == null)
			{
				return;
			}
			UnityEvent onGolemStun = spawner.onGolemStun;
			if (onGolemStun == null)
			{
				return;
			}
			onGolemStun.Invoke();
		}

		// Token: 0x06001AA1 RID: 6817 RVA: 0x000B1CF4 File Offset: 0x000AFEF4
		public void RampageRanged(Action callback)
		{
			GolemAbility ability;
			if (this.crystalBreakReactions.WeightedFilteredSelectInPlace((GolemAbility each) => each.rampageType == RampageType.Ranged && (each.abilityTier == Golem.Tier.Any || this.tier == Golem.Tier.Any || each.abilityTier.HasFlagNoGC(this.tier)), (GolemAbility _) => 1f, out ability))
			{
				base.StartAbility(ability, callback);
				return;
			}
			this.RampageMelee(callback);
		}

		// Token: 0x06001AA2 RID: 6818 RVA: 0x000B1D4C File Offset: 0x000AFF4C
		public void RampageMelee(Action callback)
		{
			if (UnityEngine.Random.value > this.normalRampageChance)
			{
				GolemAbility ability;
				if (this.crystalBreakReactions.WeightedFilteredSelectInPlace((GolemAbility each) => each.rampageType == RampageType.Melee && (each.abilityTier == Golem.Tier.Any || this.tier == Golem.Tier.Any || each.abilityTier.HasFlagNoGC(this.tier)), (GolemAbility _) => 1f, out ability))
				{
					base.StartAbility(ability, callback);
					return;
				}
			}
			this.PerformAttackMotion(GolemController.AttackMotion.Rampage, null);
		}

		// Token: 0x06001AA3 RID: 6819 RVA: 0x000B1DB4 File Offset: 0x000AFFB4
		protected void CrystalBreak(GolemCrystal crystal)
		{
			float shardsPerCrystal = (float)this.shardsToDrop / (float)this.crystals.Count;
			int shardCount = UnityEngine.Random.Range(Mathf.FloorToInt(shardsPerCrystal), Mathf.CeilToInt(shardsPerCrystal) + 1);
			this.shardsToDrop -= shardCount;
			for (int i = 0; i < shardCount; i++)
			{
				ItemData itemData = this.shardItemData;
				if (itemData != null)
				{
					itemData.SpawnAsync(new Action<Item>(this.OnShardSpawn), new Vector3?(crystal.transform.position), new Quaternion?(crystal.transform.rotation), null, true, null, Item.Owner.None);
				}
			}
			Player.currentCreature.mana.RegenFocus(Player.currentCreature.mana.MaxFocus);
			this.crystals.Remove(crystal);
			if (this.crystals.IsNullOrEmpty())
			{
				base.EndAbility();
				base.isDefeated = true;
				base.ChangeState(GolemController.State.Defeated);
				this.characterController.enabled = false;
				this.targetHeadEmissionColor = this.defeatedEmissionColor;
				if (!this.animator.GetBool(GolemController.stunHash) || !base.stunInProgress)
				{
					this.Stun(0f, null, delegate()
					{
						this.UnlockFacePlate(true);
					}, null);
				}
				else
				{
					this.UnlockFacePlate(true);
				}
			}
			else
			{
				if (this.animator.GetBool(GolemController.stunStartedHash))
				{
					int num = this.crystalsBrokenDuringStun + 1;
					this.crystalsBrokenDuringStun = num;
					if (num >= this.crystalsBrokenToWake)
					{
						this.StopStun();
						base.StartCoroutine(this.RampageAfterStun());
					}
				}
				else if (base.state != GolemController.State.Rampage)
				{
					base.InvokeRampageState();
					this.Rampage();
				}
				UnityEvent crystalBreakEvent = this.crystalBreakEvent;
				if (crystalBreakEvent != null)
				{
					crystalBreakEvent.Invoke();
				}
				Golem.GolemCrystalBreak onGolemCrystalBreak = this.OnGolemCrystalBreak;
				if (onGolemCrystalBreak != null)
				{
					onGolemCrystalBreak(crystal);
				}
			}
			this.RefreshWeakPoints();
			this.RemoveNextShield(this.shieldDisableDelay);
		}

		// Token: 0x06001AA4 RID: 6820 RVA: 0x000B1F75 File Offset: 0x000B0175
		public void OnShardSpawn(Item item)
		{
			SkillTreeShard component = item.GetComponent<SkillTreeShard>();
			item.SetOwner(Item.Owner.Player);
			component.OnAbsorbEvent -= this.OnShardAbsorb;
			component.OnAbsorbEvent += this.OnShardAbsorb;
			component.FlyToPlayer();
		}

		// Token: 0x06001AA5 RID: 6821 RVA: 0x000B1FB0 File Offset: 0x000B01B0
		public void OnShardAbsorb(SkillTreeShard shard)
		{
			ValueTuple<string, float> value2 = ItemModuleConvertToCurrency.GetValue(shard.item);
			string type = value2.Item1;
			float value = value2.Item2;
			Player.characterData.inventory.AddCurrencyValue(type, value);
		}

		// Token: 0x06001AA6 RID: 6822 RVA: 0x000B1FE6 File Offset: 0x000B01E6
		private IEnumerator RampageAfterStun()
		{
			while (this.animator.GetBool(GolemController.stunStartedHash))
			{
				yield return null;
			}
			if (base.isDefeated)
			{
				yield break;
			}
			this.Rampage();
			yield break;
		}

		// Token: 0x06001AA7 RID: 6823 RVA: 0x000B1FF5 File Offset: 0x000B01F5
		public override void Deploy(float duration = 0f, Action onDeployStart = null, Action onDeployedBegin = null, Action onDeployedEnd = null)
		{
			base.Deploy(duration, onDeployStart, onDeployedBegin, onDeployedEnd);
			this.navMeshAgent.updateRotation = false;
			this.LookAt(this.attackTarget);
		}

		// Token: 0x06001AA8 RID: 6824 RVA: 0x000B201C File Offset: 0x000B021C
		public void OnCycle()
		{
			if (base.isKilled)
			{
				return;
			}
			if (this.attackTarget)
			{
				if (this.awakeWhenTargetClose && !base.isAwake)
				{
					if (!base.transform.position.PointInRadius(this.attackTarget.position, this.awakeTargetDistance))
					{
						return;
					}
					this.LookAt(this.attackTarget);
					base.lastAbilityTime = Time.time;
					this.nextMeleeAttackTime = Time.time;
					this.headAimConstraint.weight = 1f;
					this.SetAwake(true);
				}
				this.navMeshAgent.SetDestination(this.attackTarget.position);
				bool allowNonClimbReact = true;
				if (this.wasClimbed)
				{
					if (this.blockActionWhileClimbed)
					{
						allowNonClimbReact = false;
					}
					if (this.allowClimbReact && !base.isBusy && base.isClimbed && Time.time > this.climbTime + this.climbReactTime)
					{
						allowNonClimbReact = false;
						this.TryClimbReact();
					}
				}
				if (allowNonClimbReact && this.currentAbility == null && !base.isBusy && !base.inAttackMotion && !base.deployInProgress && !base.stunInProgress)
				{
					GolemAbilityType abilityType = this.GetValidAbilityType();
					if (this.allowMelee && this.TargetInMeleeRange())
					{
						if (this.setMeleeCooldown)
						{
							this.nextMeleeAttackTime = Time.time + UnityEngine.Random.Range(this.meleeMinMaxCooldown.x, this.meleeMinMaxCooldown.y);
							this.setMeleeCooldown = false;
						}
						if (abilityType == GolemAbilityType.Melee && Time.time - base.lastAbilityTime > this.abilityCooldown)
						{
							GolemAbility ability = this.GetRandomAbilityOfType(GolemAbilityType.Melee);
							if (ability != null)
							{
								base.StartAbility(ability, null);
								goto IL_202;
							}
						}
						if (Time.time >= this.nextMeleeAttackTime && this.TryMeleeAttackTarget())
						{
							this.setMeleeCooldown = true;
							this.LookAt(null);
							this.navMeshAgent.updateRotation = false;
						}
					}
					else if (Time.time - base.lastAbilityTime > this.abilityCooldown)
					{
						GolemAbility ability2 = this.GetRandomAbilityOfType(abilityType);
						if (ability2 != null)
						{
							base.StartAbility(ability2, null);
						}
					}
				}
				IL_202:
				GolemAbility currentAbility = this.currentAbility;
				if (currentAbility != null)
				{
					currentAbility.OnCycle(this.cycleTime);
				}
				if (!base.isBusy)
				{
					this.LookAt(this.attackTarget);
					return;
				}
			}
			else
			{
				this.SetMove(false);
			}
		}

		// Token: 0x06001AA9 RID: 6825 RVA: 0x000B225E File Offset: 0x000B045E
		private bool TargetInMeleeRange()
		{
			return base.transform.position.PointInRadius(this.attackTarget.position, this.meleeMaxAttackDistance);
		}

		// Token: 0x06001AAA RID: 6826 RVA: 0x000B2284 File Offset: 0x000B0484
		public void TryClimbReact()
		{
			if (this.climbReacts.IsNullOrEmpty())
			{
				Debug.LogError("No climb reacts!");
				return;
			}
			GolemAbility climbReaction = null;
			int steps = this.climbReactIntensity + 1;
			if (steps > 0)
			{
				for (int i = 0; i < this.climbReacts.Count; i++)
				{
					if (this.climbReacts[i].Allow(this))
					{
						climbReaction = this.climbReacts[i];
						steps--;
					}
					if (steps == 0)
					{
						break;
					}
				}
			}
			if (climbReaction == null)
			{
				Debug.LogError("No available climb react!");
				return;
			}
			this.climbReactIntensity++;
			base.StartAbility(climbReaction, delegate
			{
				if (!base.isClimbed)
				{
					this.climbReactIntensity--;
				}
			});
			this.wasClimbed = false;
			this.climbTime += this.climbReactTime;
		}

		// Token: 0x06001AAB RID: 6827 RVA: 0x000B2348 File Offset: 0x000B0548
		public bool TryMeleeAttackTarget()
		{
			float targetDistance = Vector3.Distance(base.transform.position.ToXZ(), this.attackTarget.position.ToXZ());
			float targetAngle = Vector3.SignedAngle(base.transform.forward, this.attackTarget.position.ToXZ() - base.transform.position.ToXZ(), Vector3.up);
			bool foundAttack = false;
			GolemController.AttackMotion attack = GolemController.AttackMotion.Rampage;
			foreach (Golem.MeleeAttackRange meleeAttackRange in this.attackRanges)
			{
				foundAttack = meleeAttackRange.TryUseRange(targetAngle, targetDistance, out attack, base.lastAttackMotion);
				if (foundAttack)
				{
					break;
				}
			}
			if (foundAttack)
			{
				this.PerformAttackMotion(attack, null);
				this.climbReactIntensity = 0;
			}
			return foundAttack;
		}

		// Token: 0x04001932 RID: 6450
		public static Golem local;

		// Token: 0x04001933 RID: 6451
		[Header("AI")]
		public NavMeshAgent navMeshAgent;

		// Token: 0x04001934 RID: 6452
		public float cycleTime = 0.5f;

		// Token: 0x04001935 RID: 6453
		public float forwardAngle = 30f;

		// Token: 0x04001936 RID: 6454
		public float stopMoveDistance = 5.5f;

		// Token: 0x04001937 RID: 6455
		public float stopMoveSphereHeight = 1f;

		// Token: 0x04001938 RID: 6456
		public float minHeadToTargetDistance = 20f;

		// Token: 0x04001939 RID: 6457
		private Vector2 velocity;

		// Token: 0x0400193A RID: 6458
		private Vector2 smoothDeltaPosition;

		// Token: 0x0400193B RID: 6459
		private float animationDampTime = 0.1f;

		// Token: 0x0400193C RID: 6460
		[Header("Awake")]
		public bool awakeWhenTargetClose = true;

		// Token: 0x0400193D RID: 6461
		public float awakeTargetDistance = 30f;

		// Token: 0x0400193E RID: 6462
		protected float awakeTime;

		// Token: 0x0400193F RID: 6463
		[Header("Melee")]
		public bool allowMelee = true;

		// Token: 0x04001940 RID: 6464
		public float meleeMaxAttackDistance = 7f;

		// Token: 0x04001941 RID: 6465
		[FormerlySerializedAs("abilityCooldownInMelee")]
		public float abilityCooldown = 15f;

		// Token: 0x04001942 RID: 6466
		public Vector2 meleeMinMaxCooldown = new Vector2(1f, 3f);

		// Token: 0x04001943 RID: 6467
		public List<Golem.MeleeAttackRange> attackRanges;

		// Token: 0x04001944 RID: 6468
		private bool setMeleeCooldown;

		// Token: 0x04001945 RID: 6469
		private float nextMeleeAttackTime;

		// Token: 0x04001946 RID: 6470
		public bool blockActionWhileClimbed = true;

		// Token: 0x04001947 RID: 6471
		public bool allowClimbReact;

		// Token: 0x04001948 RID: 6472
		public AudioSource climbReactWarningAudio;

		// Token: 0x04001949 RID: 6473
		public float climbReactWarningTime = 2f;

		// Token: 0x0400194A RID: 6474
		public float climbReactTime = 5f;

		// Token: 0x0400194B RID: 6475
		public float resetClimbedTime = 1f;

		// Token: 0x0400194C RID: 6476
		public List<GolemAbility> climbReacts;

		// Token: 0x0400194D RID: 6477
		private bool wasClimbed;

		// Token: 0x0400194E RID: 6478
		private float climbTime;

		// Token: 0x0400194F RID: 6479
		private int climbReactIntensity;

		// Token: 0x04001950 RID: 6480
		private float lastReactWarningTime;

		// Token: 0x04001951 RID: 6481
		[Header("Crystals")]
		public WeakPointRandomizer golemCrystalRandomizer;

		// Token: 0x04001952 RID: 6482
		public WeakPointRandomizer arenaCrystalRandomizer;

		// Token: 0x04001953 RID: 6483
		public Golem.CrystalConfig defaultConfig;

		// Token: 0x04001954 RID: 6484
		public Golem.CrystalConfig tier1Config;

		// Token: 0x04001955 RID: 6485
		public Golem.CrystalConfig tier2Config;

		// Token: 0x04001956 RID: 6486
		public Golem.CrystalConfig tier3Config;

		// Token: 0x04001957 RID: 6487
		public bool rampageOnCrystalBreak = true;

		// Token: 0x04001958 RID: 6488
		public List<GolemAbility> crystalBreakReactions = new List<GolemAbility>();

		// Token: 0x04001959 RID: 6489
		public float normalRampageChance = 0.4f;

		// Token: 0x0400195A RID: 6490
		public float rampageDamageMult = 2f;

		// Token: 0x0400195B RID: 6491
		public float rampageForceMult = 1.5f;

		// Token: 0x0400195C RID: 6492
		public string shardItemId = "CrystalShard";

		// Token: 0x0400195D RID: 6493
		protected ItemData shardItemData;

		// Token: 0x0400195E RID: 6494
		protected int shardsToDrop;

		// Token: 0x0400195F RID: 6495
		[Range(1f, 10f)]
		public int crystalsBrokenToWake = 1;

		// Token: 0x04001960 RID: 6496
		private int crystalsBrokenDuringStun;

		// Token: 0x04001961 RID: 6497
		public float shieldDisableDelay;

		// Token: 0x04001962 RID: 6498
		[ColorUsage(true, true)]
		public Color defeatedEmissionColor;

		// Token: 0x04001963 RID: 6499
		public bool autoDefeatOnStart;

		// Token: 0x04001964 RID: 6500
		public bool autoDefeatAndKillOnStart;

		// Token: 0x04001965 RID: 6501
		[NonSerialized]
		public List<GolemCrystal> crystals = new List<GolemCrystal>();

		// Token: 0x04001966 RID: 6502
		[NonSerialized]
		public List<GolemCrystal> linkedArenaCrystals = new List<GolemCrystal>();

		// Token: 0x04001967 RID: 6503
		protected int disableArenaCrystalShieldIndex;

		// Token: 0x04001968 RID: 6504
		private Quaternion climbInitialRelativeRotation;

		// Token: 0x020008AF RID: 2223
		// (Invoke) Token: 0x0600411A RID: 16666
		public delegate void GolemCrystalBreak(GolemCrystal crystal);

		// Token: 0x020008B0 RID: 2224
		[Serializable]
		public class CrystalConfig
		{
			// Token: 0x04004245 RID: 16965
			public int golemCrystals = 8;

			// Token: 0x04004246 RID: 16966
			public int arenaCrystals = 3;

			// Token: 0x04004247 RID: 16967
			public float arenaCrystalNearbyStun = 10f;

			// Token: 0x04004248 RID: 16968
			public float arenaCrystalMaxStun = 30f;

			// Token: 0x04004249 RID: 16969
			public Vector2 minMaxShardsDropped = new Vector2(8f, 16f);
		}

		// Token: 0x020008B1 RID: 2225
		[Serializable]
		public class AttackRange
		{
			// Token: 0x17000530 RID: 1328
			// (get) Token: 0x0600411E RID: 16670 RVA: 0x00189F3C File Offset: 0x0018813C
			public virtual string dynamicTitle
			{
				get
				{
					return "Attack Range";
				}
			}

			// Token: 0x0600411F RID: 16671 RVA: 0x00189F43 File Offset: 0x00188143
			public bool CheckAngleDistance(float targetAngle, float targetDistance)
			{
				return targetAngle >= this.angleMinMax.x && targetAngle <= this.angleMinMax.y && targetDistance >= this.distanceMinMax.x && targetDistance <= this.distanceMinMax.y;
			}

			// Token: 0x0400424A RID: 16970
			public Vector2 angleMinMax;

			// Token: 0x0400424B RID: 16971
			public Vector2 distanceMinMax;
		}

		// Token: 0x020008B2 RID: 2226
		[Flags]
		public enum Tier
		{
			// Token: 0x0400424D RID: 16973
			Any = 0,
			// Token: 0x0400424E RID: 16974
			Tier1 = 1,
			// Token: 0x0400424F RID: 16975
			Tier2 = 2,
			// Token: 0x04004250 RID: 16976
			Tier3 = 4
		}

		// Token: 0x020008B3 RID: 2227
		[Serializable]
		public class MeleeAttackRange : Golem.AttackRange
		{
			// Token: 0x17000531 RID: 1329
			// (get) Token: 0x06004121 RID: 16673 RVA: 0x00189F8C File Offset: 0x0018818C
			public override string dynamicTitle
			{
				get
				{
					string result = this.attackOptions.IsNullOrEmpty() ? "None" : "";
					int i = 0;
					for (;;)
					{
						int num = i;
						Golem.MeleeAttackRange.WeightedAttack[] array = this.attackOptions;
						if (num >= ((array != null) ? array.Length : 0))
						{
							break;
						}
						Golem.MeleeAttackRange.WeightedAttack attack = this.attackOptions[i];
						result = string.Concat(new string[]
						{
							result,
							attack.attack.ToString(),
							" (",
							attack.weight.ToString("0.00"),
							")"
						});
						if (i != this.attackOptions.Length - 1)
						{
							result += ", ";
						}
						i++;
					}
					return result;
				}
			}

			// Token: 0x06004122 RID: 16674 RVA: 0x0018A038 File Offset: 0x00188238
			public bool TryUseRange(float targetAngle, float targetDistance, out GolemController.AttackMotion attack, GolemController.AttackMotion lastAttack = GolemController.AttackMotion.Rampage)
			{
				attack = this.attackOptions[UnityEngine.Random.Range(0, this.attackOptions.Length)].attack;
				Golem.MeleeAttackRange.WeightedAttack weightedAttack;
				if (!this.attackOptions.WeightedFilteredSelectInPlace((Golem.MeleeAttackRange.WeightedAttack at) => at.attack != lastAttack, (Golem.MeleeAttackRange.WeightedAttack at) => at.weight, out weightedAttack))
				{
					return false;
				}
				attack = weightedAttack.attack;
				return base.CheckAngleDistance(targetAngle, targetDistance);
			}

			// Token: 0x04004251 RID: 16977
			[Space]
			public Golem.MeleeAttackRange.WeightedAttack[] attackOptions;

			// Token: 0x02000BE1 RID: 3041
			[Serializable]
			public class WeightedAttack
			{
				// Token: 0x04004D39 RID: 19769
				public GolemController.AttackMotion attack;

				// Token: 0x04004D3A RID: 19770
				public float weight = 1f;
			}
		}

		// Token: 0x020008B4 RID: 2228
		[Serializable]
		public class InflictedStatus
		{
			// Token: 0x17000532 RID: 1330
			// (get) Token: 0x06004124 RID: 16676 RVA: 0x0018A0C3 File Offset: 0x001882C3
			private List<ValueDropdownItem<string>> GetAllStatuses
			{
				get
				{
					return Catalog.GetDropdownAllID<StatusData>("None");
				}
			}

			// Token: 0x04004252 RID: 16978
			public string data;

			// Token: 0x04004253 RID: 16979
			public float duration = 3f;

			// Token: 0x04004254 RID: 16980
			public float parameter;
		}
	}
}
