using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000265 RID: 613
	[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Self-imbue config")]
	[Serializable]
	public class GolemImbue : GolemAbility
	{
		// Token: 0x06001B90 RID: 7056 RVA: 0x000B6B83 File Offset: 0x000B4D83
		public override bool Allow(GolemController golem)
		{
			return base.Allow(golem);
		}

		// Token: 0x06001B91 RID: 7057 RVA: 0x000B6B8C File Offset: 0x000B4D8C
		public override void Begin(GolemController golem)
		{
			base.Begin(golem);
			if (this.startEffectData == null)
			{
				this.startEffectData = Catalog.GetData<EffectData>(this.startEffectID, true);
			}
			if (this.bodyEffectData == null)
			{
				this.bodyEffectData = Catalog.GetData<EffectData>(this.bodyEffectID, true);
			}
			golem.PerformAttackMotion(GolemController.AttackMotion.SelfImbue, new Action(base.End));
			EffectData effectData = this.startEffectData;
			if (effectData != null)
			{
				effectData.Spawn(golem.transform, true, null, false).Play(0, false, false);
			}
			if (this.addedHandlers)
			{
				return;
			}
			foreach (Rigidbody component in golem.bodyParts)
			{
				component.GetOrAddComponent<CollisionListener>().OnCollisionEnterEvent += this.GolemTouch;
			}
		}

		// Token: 0x06001B92 RID: 7058 RVA: 0x000B6C68 File Offset: 0x000B4E68
		private void GolemTouch(Collision other)
		{
			if (this.bodyEffects.IsNullOrEmpty())
			{
				return;
			}
			Creature toucher = null;
			Rigidbody rigidbody = other.rigidbody;
			RagdollPart part = (rigidbody != null) ? rigidbody.GetComponent<RagdollPart>() : null;
			if (part != null)
			{
				toucher = part.ragdoll.creature;
			}
			else
			{
				Rigidbody rigidbody2 = other.rigidbody;
				Creature creature = (rigidbody2 != null) ? rigidbody2.GetComponent<Creature>() : null;
				if (creature != null)
				{
					toucher = creature;
				}
				else
				{
					Rigidbody rigidbody3 = other.rigidbody;
					Item item = (rigidbody3 != null) ? rigidbody3.GetComponent<Item>() : null;
					if (item != null && this.disarmAllowed)
					{
						for (int i = item.handlers.Count - 1; i >= 0; i--)
						{
							RagdollHand handler = item.handlers[i];
							this.Affect(handler.creature);
						}
					}
				}
			}
			if (toucher != null)
			{
				this.Affect(toucher);
			}
		}

		// Token: 0x06001B93 RID: 7059 RVA: 0x000B6D29 File Offset: 0x000B4F29
		public override void AbilityStep(int step)
		{
			base.AbilityStep(step);
			this.golem.StartCoroutine(this.EffectCoroutine());
		}

		// Token: 0x06001B94 RID: 7060 RVA: 0x000B6D44 File Offset: 0x000B4F44
		private IEnumerator EffectCoroutine()
		{
			this.ApplyEffects();
			if (this.golem.isClimbed)
			{
				for (int i = this.golem.climbers.Count - 1; i >= 0; i--)
				{
					Creature creature = this.golem.climbers[i];
					this.Affect(creature);
				}
			}
			this.golem.OnDamageDealt += this.GolemHit;
			float endTime = Time.time + this.selfImbueDuration;
			while (Time.time < endTime)
			{
				if (this.golem.isClimbed && !this.bodyEffects.IsNullOrEmpty())
				{
					for (int j = this.golem.climbers.Count - 1; j >= 0; j--)
					{
						Creature creature2 = this.golem.climbers[j];
						this.Affect(creature2);
					}
				}
				yield return Yielders.EndOfFrame;
			}
			this.golem.OnDamageDealt -= this.GolemHit;
			this.RemoveEffects();
			yield break;
		}

		// Token: 0x06001B95 RID: 7061 RVA: 0x000B6D53 File Offset: 0x000B4F53
		private void GolemHit(Creature target, float damage)
		{
			if (this.bodyEffects.IsNullOrEmpty())
			{
				return;
			}
			this.Affect(target);
		}

		// Token: 0x06001B96 RID: 7062 RVA: 0x000B6D6C File Offset: 0x000B4F6C
		protected void ApplyEffects()
		{
			if (this.bodyEffects.IsNullOrEmpty())
			{
				this.bodyEffects = new List<EffectInstance>();
				HumanBodyBones[] array = new HumanBodyBones[9];
				RuntimeHelpers.InitializeArray(array, fieldof(<PrivateImplementationDetails>.4F623F379121DB6F7857E314ACB4645C8678F849FEC38CAF8915F5C96CCA9DB8).FieldHandle);
				foreach (HumanBodyBones bone in array)
				{
					this.<ApplyEffects>g__SpawnOnBone|18_0(this.golem.animator.GetBoneTransform(bone));
				}
			}
		}

		// Token: 0x06001B97 RID: 7063 RVA: 0x000B6DD0 File Offset: 0x000B4FD0
		protected void RemoveEffects()
		{
			if (this.bodyEffects.IsNullOrEmpty())
			{
				return;
			}
			foreach (EffectInstance effectInstance in this.bodyEffects)
			{
				effectInstance.Stop(0);
			}
			this.bodyEffects = null;
		}

		// Token: 0x06001B98 RID: 7064 RVA: 0x000B6E38 File Offset: 0x000B5038
		protected void Affect(Creature target)
		{
			if (Time.time < target.lastDamageTime + this.timeBetweenDamage)
			{
				return;
			}
			if (this.ungripOnDamage)
			{
				RagdollHand hand = target.GetHand((Side)UnityEngine.Random.Range(0, 2));
				if (!this.ReleaseHand(hand))
				{
					this.ReleaseHand(hand.otherHand);
				}
			}
			target.Damage(this.contactDamage);
			foreach (Golem.InflictedStatus status in this.appliedStatuses)
			{
				target.Inflict(status.data, this, status.duration, status.parameter, true);
			}
		}

		// Token: 0x06001B99 RID: 7065 RVA: 0x000B6EF0 File Offset: 0x000B50F0
		private bool ReleaseHand(RagdollHand hand)
		{
			if (Time.time < hand.creature.lastDamageTime + this.timeBetweenDamage)
			{
				return false;
			}
			if (!this.golem.grabbedParts.ContainsKey(hand) && !this.disarmAllowed)
			{
				return false;
			}
			if (hand.grabbedHandle != null)
			{
				hand.UnGrab(false);
			}
			RagdollHandClimb climb = hand.climb;
			if (((climb != null) ? climb.gripPhysicBody : null) != null)
			{
				hand.climb.UnGrip();
			}
			RagdollHandClimb climb2 = hand.climb;
			if (climb2 != null)
			{
				climb2.DisableGripTemp(0.5f);
			}
			return true;
		}

		// Token: 0x06001B9B RID: 7067 RVA: 0x000B6FBC File Offset: 0x000B51BC
		[CompilerGenerated]
		private void <ApplyEffects>g__SpawnOnBone|18_0(Transform bone)
		{
			EffectInstance bodyEffect = this.bodyEffectData.Spawn(bone, true, null, false);
			bodyEffect.Play(0, false, false);
			this.bodyEffects.Add(bodyEffect);
		}

		// Token: 0x04001A5A RID: 6746
		public float selfImbueDuration = 10f;

		// Token: 0x04001A5B RID: 6747
		public float contactDamage = 5f;

		// Token: 0x04001A5C RID: 6748
		public float timeBetweenDamage = 0.5f;

		// Token: 0x04001A5D RID: 6749
		public bool ungripOnDamage;

		// Token: 0x04001A5E RID: 6750
		public bool disarmAllowed;

		// Token: 0x04001A5F RID: 6751
		public string bodyEffectID;

		// Token: 0x04001A60 RID: 6752
		public string startEffectID;

		// Token: 0x04001A61 RID: 6753
		public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();

		// Token: 0x04001A62 RID: 6754
		[NonSerialized]
		public EffectData startEffectData;

		// Token: 0x04001A63 RID: 6755
		[NonSerialized]
		public EffectData bodyEffectData;

		// Token: 0x04001A64 RID: 6756
		protected List<EffectInstance> bodyEffects;

		// Token: 0x04001A65 RID: 6757
		protected bool addedHandlers;
	}
}
