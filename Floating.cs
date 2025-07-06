using System;
using System.Collections;
using ThunderRoad.Skill.Spell;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000239 RID: 569
	public class Floating : Status
	{
		// Token: 0x06001808 RID: 6152 RVA: 0x000A00F6 File Offset: 0x0009E2F6
		public override void Spawn(StatusData data, ThunderEntity entity)
		{
			base.Spawn(data, entity);
			this.data = (data as StatusDataFloating);
		}

		// Token: 0x06001809 RID: 6153 RVA: 0x000A010C File Offset: 0x0009E30C
		protected override object GetValue()
		{
			FloatingParams output = FloatingParams.Identity;
			bool? blockSlam = null;
			foreach (ValueTuple<float, object> valueTuple in this.handlers.Values)
			{
				object parameter = valueTuple.Item2;
				if (parameter is FloatingParams)
				{
					FloatingParams multiplier = (FloatingParams)parameter;
					output *= multiplier;
					bool? flag;
					if (!multiplier.noSlamAtEnd)
					{
						flag = new bool?(false);
					}
					else if (blockSlam == null)
					{
						flag = new bool?(true);
					}
					else
					{
						flag = blockSlam;
					}
					blockSlam = flag;
				}
			}
			output.noSlamAtEnd = blockSlam.GetValueOrDefault();
			return output;
		}

		// Token: 0x17000176 RID: 374
		// (get) Token: 0x0600180A RID: 6154 RVA: 0x000A01CC File Offset: 0x0009E3CC
		public override bool ReapplyOnValueChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x0600180B RID: 6155 RVA: 0x000A01D0 File Offset: 0x0009E3D0
		public override void FirstApply()
		{
			base.FirstApply();
			ThunderEntity entity = this.entity;
			Creature creature = entity as Creature;
			if (creature != null)
			{
				if (!creature.isPlayer)
				{
					Creature creature2 = creature;
					creature2.brain.AddNoStandUpModifier(this);
					creature2.ragdoll.rootPart.physicBody.AddForce(Vector3.up * this.data.upwardsForceCreature, this.data.upwardsForceMode);
					creature2.ragdoll.rootPart.physicBody.AddTorque(Vector3.Cross((creature2.transform.position - Player.local.transform.position).normalized, Vector3.up) * this.data.creatureTorqueMult, this.data.torqueForceMode);
					return;
				}
				creature.currentLocomotion.physicBody.AddForce(Vector3.up * this.data.upwardsForcePlayer, this.data.upwardsForceMode);
				creature.hitEnvironmentDamageModifier.Add(this, this.data.environmentDamageMult);
				return;
			}
			else
			{
				Item item = entity as Item;
				if (item == null)
				{
					return;
				}
				item.physicBody.AddForce(Vector3.up * this.data.upwardsForceItem, ForceMode.VelocityChange);
				item.physicBody.AddTorque(Vector3.Cross((item.transform.position - Player.local.transform.position).normalized, Vector3.up) * this.data.itemTorqueMult, this.data.torqueForceMode);
				return;
			}
		}

		// Token: 0x0600180C RID: 6156 RVA: 0x000A0374 File Offset: 0x0009E574
		public override void Apply()
		{
			base.Apply();
			object value = this.value;
			FloatingParams floatingParams;
			if (value is FloatingParams)
			{
				FloatingParams floatingParam = (FloatingParams)value;
				floatingParams = floatingParam;
			}
			else
			{
				floatingParams = FloatingParams.Identity;
			}
			FloatingParams multiplier = floatingParams;
			ThunderEntity entity = this.entity;
			Creature creature = entity as Creature;
			if (creature != null)
			{
				if (!creature.isKilled && !creature.isPlayer && (this.data.destabilizeCreatures || creature.brain.currentStagger == Brain.Stagger.LightAndMedium))
				{
					creature.ragdoll.SetState(Ragdoll.State.Destabilized);
				}
				if (creature.brain.isDying)
				{
					BrainModuleDeath module = creature.brain.instance.GetModule<BrainModuleDeath>(true);
					if (module != null)
					{
						module.StopDying();
					}
				}
				creature.ragdoll.SetPhysicModifier(this, new float?((creature.isPlayer ? this.data.gravityMultPlayer : this.data.gravityMult) * multiplier.gravity), this.data.massRatio * multiplier.mass, (creature.isPlayer ? this.data.dragRatioPlayer : this.data.dragRatio) * multiplier.drag, this.data.angularDragRatio * multiplier.drag, null);
				creature.currentLocomotion.SetPhysicModifier(this, new float?((creature.isPlayer ? this.data.gravityMultPlayer : this.data.gravityMult) * multiplier.gravity), this.data.massRatio * multiplier.mass, (creature.isPlayer ? this.data.dragRatioPlayer : this.data.dragRatio) * multiplier.drag, -1);
				return;
			}
			Item item = entity as Item;
			if (item == null)
			{
				return;
			}
			item.SetPhysicModifier(this, new float?(this.data.gravityMult * multiplier.gravity), this.data.massRatio * multiplier.mass, this.data.dragRatio * multiplier.drag, this.data.angularDragRatio * multiplier.drag, -1f, null);
		}

		// Token: 0x0600180D RID: 6157 RVA: 0x000A0580 File Offset: 0x0009E780
		public override void Remove()
		{
			base.Remove();
			Creature creature = this.entity as Creature;
			if (creature != null)
			{
				creature.brain.RemoveNoStandUpModifier(this);
				creature.ragdoll.RemovePhysicModifier(this);
				creature.currentLocomotion.RemovePhysicModifier(this);
				return;
			}
			Item item = this.entity as Item;
			if (item != null)
			{
				item.RemovePhysicModifier(this);
			}
		}

		// Token: 0x0600180E RID: 6158 RVA: 0x000A05E0 File Offset: 0x0009E7E0
		public override void FullRemove()
		{
			base.FullRemove();
			Creature creature2 = this.entity as Creature;
			if (creature2 != null)
			{
				creature2.hitEnvironmentDamageModifier.Remove(this);
			}
			Creature creature;
			bool flag;
			if (this.data.allowSlamOnEnd && Floating.slamOnEnd)
			{
				object value = this.value;
				if (!(value is FloatingParams) || !((FloatingParams)value).noSlamAtEnd)
				{
					creature = (this.entity as Creature);
					if (creature != null)
					{
						flag = (creature.state == Creature.State.Alive);
						goto IL_6E;
					}
				}
			}
			flag = true;
			IL_6E:
			bool flag2 = flag;
			if (!flag2)
			{
				Ragdoll ragdoll = creature.ragdoll;
				bool flag3;
				if (ragdoll != null && ragdoll.isGrabbed)
				{
					Ragdoll.State state = ragdoll.state;
					if (state == Ragdoll.State.Inert || state == Ragdoll.State.Standing)
					{
						flag3 = true;
						goto IL_9D;
					}
				}
				flag3 = false;
				IL_9D:
				flag2 = flag3;
			}
			if (flag2 || creature.isPlayer || this.data.lithobrakeData == null)
			{
				return;
			}
			EffectData slamEffectData = this.data.slamEffectData;
			if (slamEffectData != null)
			{
				EffectInstance effectInstance = slamEffectData.Spawn(this.entity.Center + Vector3.up * 1f, Quaternion.LookRotation(Vector3.down), null, null, true, null, false, 1f, 1f, Array.Empty<Type>());
				if (effectInstance != null)
				{
					effectInstance.Play(0, false, false);
				}
			}
			creature.StartCoroutine(this.HighGravityRoutine(this.data.lithobrakeData, creature));
		}

		// Token: 0x0600180F RID: 6159 RVA: 0x000A0720 File Offset: 0x0009E920
		public IEnumerator HighGravityRoutine(SkillHoverSlam skill, Creature creature)
		{
			creature.ragdoll.SetPhysicModifier(this, new float?(skill.slamGravityRatio), 1f, -1f, -1f, null);
			yield return new WaitForSeconds(skill.slamDuration);
			creature.ragdoll.RemovePhysicModifier(this);
			yield break;
		}

		// Token: 0x0400173B RID: 5947
		public static bool slamOnEnd;

		// Token: 0x0400173C RID: 5948
		public new StatusDataFloating data;
	}
}
