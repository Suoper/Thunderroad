using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000234 RID: 564
	[Serializable]
	public class MeleeIdle : IdlePose
	{
		// Token: 0x1700016A RID: 362
		// (get) Token: 0x060017C8 RID: 6088 RVA: 0x0009EB88 File Offset: 0x0009CD88
		public override bool showDifficulty
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060017C9 RID: 6089 RVA: 0x0009EB8B File Offset: 0x0009CD8B
		public override StanceNode CreateNew()
		{
			return new MeleeIdle();
		}

		// Token: 0x060017CA RID: 6090 RVA: 0x0009EB92 File Offset: 0x0009CD92
		public override bool CheckValid(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0)
		{
			return !this.requireThrustWeapon || ((mainHand != null && mainHand.AttackTypeMatches(ItemModuleAI.AttackType.Thrust)) || (offHand != null && offHand.AttackTypeMatches(ItemModuleAI.AttackType.Thrust)));
		}

		// Token: 0x060017CB RID: 6091 RVA: 0x0009EBBC File Offset: 0x0009CDBC
		protected override void PopulateLists()
		{
			base.PopulateLists();
			this.attackMotions = new List<AttackMotion>();
			this.minRange = float.PositiveInfinity;
			this.maxRange = float.NegativeInfinity;
			this.minReachRange = float.PositiveInfinity;
			this.maxReachRange = float.NegativeInfinity;
			MeleeStanceData meleeStanceData = this.stanceData as MeleeStanceData;
			if (meleeStanceData != null)
			{
				for (int i = 0; i < meleeStanceData.openingAttacks.Count; i++)
				{
					AttackMotion motion = meleeStanceData.openingAttacks[i];
					if (motion.sourceIdle == "Any" || motion.sourceIdle == this.id)
					{
						this.attackMotions.Add(motion);
						if (motion.includeInMeleeRange)
						{
							if (motion.includeWeaponReach)
							{
								if (motion.minRange < this.minReachRange)
								{
									this.minReachRange = motion.minRange;
								}
								if (motion.minRange > this.maxReachRange)
								{
									this.maxReachRange = motion.minRange;
								}
							}
							else
							{
								if (motion.minRange < this.minRange)
								{
									this.minRange = motion.minRange;
								}
								if (motion.minRange > this.maxRange)
								{
									this.maxRange = motion.minRange;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060017CC RID: 6092 RVA: 0x0009ECF0 File Offset: 0x0009CEF0
		public AttackMotion RandomWeightedMotion()
		{
			AttackMotion newMotion;
			this.attackMotions.WeightedFilteredSelectInPlace((AttackMotion motion) => motion.skipChance.IsApproximately(0f) || UnityEngine.Random.value > motion.skipChance, (AttackMotion motion) => motion.weight, out newMotion);
			if (newMotion != null)
			{
				this.lastPick = newMotion;
			}
			return newMotion;
		}

		// Token: 0x060017CD RID: 6093 RVA: 0x0009ED54 File Offset: 0x0009CF54
		public AttackMotion PickBestMotion(BrainModuleMelee attacker, Transform target, float maxdelta)
		{
			MeleeIdle.<>c__DisplayClass13_0 CS$<>8__locals1 = new MeleeIdle.<>c__DisplayClass13_0();
			CS$<>8__locals1.attacker = attacker;
			CS$<>8__locals1.maxdelta = maxdelta;
			CS$<>8__locals1.<>4__this = this;
			Locomotion targetLocomotion = target.GetComponent<Locomotion>();
			float? num;
			if (targetLocomotion == null)
			{
				num = null;
			}
			else
			{
				CapsuleCollider capsuleCollider = targetLocomotion.capsuleCollider;
				num = ((capsuleCollider != null) ? new float?(capsuleCollider.radius) : null);
			}
			float? num2 = num;
			num2.GetValueOrDefault();
			float radius = CS$<>8__locals1.attacker.creature.locomotion.capsuleCollider.radius;
			MeleeIdle.<>c__DisplayClass13_0 CS$<>8__locals2 = CS$<>8__locals1;
			Vector3 targetVelocity;
			if (!(targetLocomotion != null))
			{
				PhysicBody pb = target.GetPhysicBodyInParent();
				targetVelocity = ((pb != null) ? pb.velocity.ToXZ() : Vector3.zero);
			}
			else
			{
				targetVelocity = targetLocomotion.physicBody.velocity.ToXZ();
			}
			CS$<>8__locals2.targetVelocity = targetVelocity;
			CS$<>8__locals1.predictedDistances = new float[5];
			CS$<>8__locals1.deltaTimes = new float[5];
			CS$<>8__locals1.directionDelta = 0f;
			CS$<>8__locals1.deltaGood = false;
			CS$<>8__locals1.deltaRatio = 1f;
			Transform selfTransform = CS$<>8__locals1.attacker.creature.transform;
			CS$<>8__locals1.selfStartPosition = selfTransform.position.ToXZ();
			CS$<>8__locals1.targetStart = target.position.ToXZ();
			CS$<>8__locals1.heightRatio = selfTransform.lossyScale.y;
			AttackMotion newMotion;
			this.attackMotions.WeightedFilteredSelectInPlace(new Func<AttackMotion, bool>(CS$<>8__locals1.<PickBestMotion>g__AttackWithinDelta|0), new Func<AttackMotion, float>(CS$<>8__locals1.<PickBestMotion>g__GetAttackWeight|1), out newMotion);
			if (newMotion != null)
			{
				this.lastPick = newMotion;
			}
			return newMotion;
		}

		// Token: 0x04001715 RID: 5909
		public bool requireThrustWeapon;

		// Token: 0x04001716 RID: 5910
		[NonSerialized]
		public List<AttackMotion> attackMotions;

		// Token: 0x04001717 RID: 5911
		[NonSerialized]
		public AttackMotion lastPick;

		// Token: 0x04001718 RID: 5912
		[NonSerialized]
		public float minRange;

		// Token: 0x04001719 RID: 5913
		[NonSerialized]
		public float maxRange;

		// Token: 0x0400171A RID: 5914
		[NonSerialized]
		public float minReachRange;

		// Token: 0x0400171B RID: 5915
		[NonSerialized]
		public float maxReachRange;
	}
}
