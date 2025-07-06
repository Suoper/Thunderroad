using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000264 RID: 612
	[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Blast config")]
	public class GolemBlast : GolemAbility
	{
		// Token: 0x06001B8A RID: 7050 RVA: 0x000B6648 File Offset: 0x000B4848
		private void OnValidate()
		{
			Golem.MeleeAttackRange meleeRange = this.attackRange as Golem.MeleeAttackRange;
			if (meleeRange != null)
			{
				this.attackRange = new Golem.AttackRange
				{
					angleMinMax = meleeRange.angleMinMax,
					distanceMinMax = meleeRange.distanceMinMax
				};
			}
		}

		// Token: 0x06001B8B RID: 7051 RVA: 0x000B6688 File Offset: 0x000B4888
		public override bool Allow(GolemController golem)
		{
			float targetDistance = Vector3.Distance(golem.transform.position.ToXZ(), golem.attackTarget.position.ToXZ());
			float targetAngle = Vector3.SignedAngle(golem.transform.forward, golem.attackTarget.position.ToXZ() - golem.transform.position.ToXZ(), Vector3.up);
			return base.Allow(golem) && (this.type == GolemAbilityType.Climb || golem.lastAttackMotion != this.motion) && (this.type != GolemAbilityType.Melee || this.attackRange.CheckAngleDistance(targetAngle, targetDistance));
		}

		// Token: 0x06001B8C RID: 7052 RVA: 0x000B6730 File Offset: 0x000B4930
		public override void Begin(GolemController golem)
		{
			base.Begin(golem);
			this.blastReference = golem.transform;
			if (this.attachToBone)
			{
				this.blastReference = golem.animator.GetBoneTransform(this.blastLinkedBone);
			}
			if (this.blastEffectData == null)
			{
				this.blastEffectData = Catalog.GetData<EffectData>(this.blastEffectID, true);
			}
			golem.PerformAttackMotion(this.motion, new Action(base.End));
		}

		// Token: 0x06001B8D RID: 7053 RVA: 0x000B67A4 File Offset: 0x000B49A4
		public override void AbilityStep(int step)
		{
			base.AbilityStep(step);
			Vector3 position = this.blastReference.TransformPoint(this.blastLocalPosition);
			if (this.kickPlayerOff && this.golem.isClimbed)
			{
				this.golem.ForceUngripClimbers(true, true);
			}
			this.blastEffectData.Spawn(position, Quaternion.Euler(this.effectEulers), null, null, true, null, false, 1f, 1f, Array.Empty<Type>()).Play(0, false, false);
			GolemBlast.Explosion(position, this.blastRadius, this.blastMask, this.blastDamage, this.appliedStatuses, this.blastForce, this.blastForceUpwardMult, this.blastForceMode, this.damageBreakables, null);
		}

		// Token: 0x06001B8E RID: 7054 RVA: 0x000B6858 File Offset: 0x000B4A58
		public static void Explosion(Vector3 position, float radius, LayerMask layerMask, float damage, List<Golem.InflictedStatus> statuses, float force, float upwardsMult, ForceMode forceMode, bool hitBreakables, Action endCallback)
		{
			GolemBlast.<>c__DisplayClass22_0 CS$<>8__locals1 = new GolemBlast.<>c__DisplayClass22_0();
			CS$<>8__locals1.force = force;
			CS$<>8__locals1.position = position;
			CS$<>8__locals1.radius = radius;
			CS$<>8__locals1.upwardsMult = upwardsMult;
			CS$<>8__locals1.forceMode = forceMode;
			CS$<>8__locals1.breakHandler = null;
			EventManager.OnItemBrokenEnd += CS$<>8__locals1.<Explosion>g__BreakableBroken|0;
			HashSet<GameObject> affectedObjects = new HashSet<GameObject>();
			Collider[] array = Physics.OverlapSphere(CS$<>8__locals1.position, CS$<>8__locals1.radius, layerMask, QueryTriggerInteraction.Ignore);
			for (int i = 0; i < array.Length; i++)
			{
				PhysicBody pb = array[i].GetPhysicBody();
				if (pb != null)
				{
					ThunderEntity hitEntity = null;
					RagdollPart part;
					if (pb.gameObject == Player.local.gameObject)
					{
						hitEntity = Player.currentCreature;
					}
					else if (pb.gameObject.TryGetComponent<RagdollPart>(out part))
					{
						hitEntity = part.ragdoll.creature;
					}
					else
					{
						pb.gameObject.TryGetComponent<ThunderEntity>(out hitEntity);
					}
					if (!(hitEntity == null) && !affectedObjects.Contains(hitEntity.gameObject))
					{
						if (hitEntity != null)
						{
							affectedObjects.Add(hitEntity.gameObject);
							hitEntity.AddExplosionForce(CS$<>8__locals1.force, CS$<>8__locals1.position, CS$<>8__locals1.radius, CS$<>8__locals1.upwardsMult, CS$<>8__locals1.forceMode, null);
							Creature hitCreature = hitEntity as Creature;
							if (hitCreature != null)
							{
								hitCreature.Damage(damage);
								foreach (Golem.InflictedStatus status in statuses)
								{
									hitCreature.Inflict(status.data, "Golem explosion", status.duration, status.parameter, true);
								}
							}
							Item item = hitEntity as Item;
							bool flag;
							Breakable breakable;
							if (item != null)
							{
								breakable = item.breakable;
								flag = (breakable != null);
							}
							else
							{
								flag = false;
							}
							if (flag && hitBreakables && !breakable.contactBreakOnly && CS$<>8__locals1.force * CS$<>8__locals1.force >= breakable.instantaneousBreakDamage)
							{
								CS$<>8__locals1.breakHandler = delegate(Breakable broken, PhysicBody[] pieces)
								{
									if (broken != breakable)
									{
										return;
									}
									for (int j = 0; j < pieces.Length; j++)
									{
										pieces[j].AddExplosionForce(CS$<>8__locals1.force, CS$<>8__locals1.position, CS$<>8__locals1.radius, CS$<>8__locals1.upwardsMult, CS$<>8__locals1.forceMode);
									}
								};
								breakable.Break();
							}
						}
						else
						{
							affectedObjects.Add(pb.gameObject);
							pb.AddExplosionForce(CS$<>8__locals1.force, CS$<>8__locals1.position, CS$<>8__locals1.radius, CS$<>8__locals1.upwardsMult, CS$<>8__locals1.forceMode);
						}
					}
				}
			}
			if (endCallback != null)
			{
				endCallback();
			}
		}

		// Token: 0x04001A48 RID: 6728
		public GolemController.AttackMotion motion;

		// Token: 0x04001A49 RID: 6729
		public Vector3 blastLocalPosition;

		// Token: 0x04001A4A RID: 6730
		public bool attachToBone;

		// Token: 0x04001A4B RID: 6731
		public HumanBodyBones blastLinkedBone = HumanBodyBones.UpperChest;

		// Token: 0x04001A4C RID: 6732
		public float blastRadius = 5f;

		// Token: 0x04001A4D RID: 6733
		public string blastEffectID = "";

		// Token: 0x04001A4E RID: 6734
		public Vector3 effectEulers = Vector3.zero;

		// Token: 0x04001A4F RID: 6735
		public bool kickPlayerOff;

		// Token: 0x04001A50 RID: 6736
		public bool damageBreakables;

		// Token: 0x04001A51 RID: 6737
		public LayerMask blastMask;

		// Token: 0x04001A52 RID: 6738
		public float blastDamage = 5f;

		// Token: 0x04001A53 RID: 6739
		public float blastForce = 5f;

		// Token: 0x04001A54 RID: 6740
		public float blastForceUpwardMult = 1.5f;

		// Token: 0x04001A55 RID: 6741
		public ForceMode blastForceMode = ForceMode.VelocityChange;

		// Token: 0x04001A56 RID: 6742
		public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();

		// Token: 0x04001A57 RID: 6743
		public Golem.AttackRange attackRange;

		// Token: 0x04001A58 RID: 6744
		[NonSerialized]
		public EffectData blastEffectData;

		// Token: 0x04001A59 RID: 6745
		[NonSerialized]
		public Transform blastReference;
	}
}
