using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002CE RID: 718
	public class ForcePusher : ThunderBehaviour
	{
		// Token: 0x060022B8 RID: 8888 RVA: 0x000EE455 File Offset: 0x000EC655
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x17000224 RID: 548
		// (get) Token: 0x060022B9 RID: 8889 RVA: 0x000EE462 File Offset: 0x000EC662
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x060022BA RID: 8890 RVA: 0x000EE465 File Offset: 0x000EC665
		private void Awake()
		{
			this.entities = new List<ThunderEntity>();
		}

		// Token: 0x060022BB RID: 8891 RVA: 0x000EE472 File Offset: 0x000EC672
		public void Start()
		{
			this.effectData = Catalog.GetData<EffectData>(this.effectId, true);
		}

		// Token: 0x060022BC RID: 8892 RVA: 0x000EE486 File Offset: 0x000EC686
		protected internal override void ManagedUpdate()
		{
			base.ManagedUpdate();
			if (this.radiusParticles)
			{
				this.radiusParticles.transform.localScale = Vector3.one * this.radius;
			}
		}

		// Token: 0x060022BD RID: 8893 RVA: 0x000EE4BB File Offset: 0x000EC6BB
		public void Push()
		{
			if (this.delay > 0f)
			{
				base.StartCoroutine(this.DelayedPush(this.delay));
				return;
			}
			this.ForcePush();
		}

		// Token: 0x060022BE RID: 8894 RVA: 0x000EE4E4 File Offset: 0x000EC6E4
		public IEnumerator DelayedPush(float delay)
		{
			yield return new WaitForSeconds(delay);
			this.ForcePush();
			yield break;
		}

		// Token: 0x060022BF RID: 8895 RVA: 0x000EE4FC File Offset: 0x000EC6FC
		public void ForcePush()
		{
			this.effectData.Spawn(base.transform, true, null, false).Play(0, false, false);
			this.entities.Clear();
			ThunderEntity.InRadius(base.transform.position, this.radius, null, this.entities);
			for (int i = 0; i < this.entities.Count; i++)
			{
				ThunderEntity entity = this.entities[i];
				Creature creature = entity as Creature;
				if (creature != null && !creature.isPlayer)
				{
					creature.MaxPush(Creature.PushType.Magic, creature.ragdoll.targetPart.transform.position - base.transform.position, (RagdollPart.Type)0);
				}
				ForcePusher.RadialPushMode radialPushMode = this.pushMode;
				if (radialPushMode != ForcePusher.RadialPushMode.Proximity)
				{
					if (radialPushMode == ForcePusher.RadialPushMode.Absolute)
					{
						entity.AddRadialForce(this.force, base.transform.position, this.upwardsModifier, this.forceMode, null);
					}
				}
				else
				{
					entity.AddExplosionForce(this.force, base.transform.position, this.radius, this.upwardsModifier, this.forceMode, null);
				}
			}
		}

		// Token: 0x060022C0 RID: 8896 RVA: 0x000EE616 File Offset: 0x000EC816
		private void OnDrawGizmos()
		{
			Gizmos.DrawWireSphere(base.transform.position, this.radius);
		}

		// Token: 0x040021C9 RID: 8649
		public float radius = 4f;

		// Token: 0x040021CA RID: 8650
		public float force = 10f;

		// Token: 0x040021CB RID: 8651
		public float upwardsModifier = 1f;

		// Token: 0x040021CC RID: 8652
		public ForceMode forceMode = ForceMode.VelocityChange;

		// Token: 0x040021CD RID: 8653
		public GameObject radiusParticles;

		// Token: 0x040021CE RID: 8654
		public float delay = 0.5f;

		// Token: 0x040021CF RID: 8655
		[Tooltip("How force is applied based on distance from the center. Proximity has force falloff, Absolute will apply the same force regardless of distance.")]
		public ForcePusher.RadialPushMode pushMode = ForcePusher.RadialPushMode.Absolute;

		// Token: 0x040021D0 RID: 8656
		public string effectId = "Shockwave";

		// Token: 0x040021D1 RID: 8657
		protected EffectData effectData;

		// Token: 0x040021D2 RID: 8658
		protected List<ThunderEntity> entities;

		// Token: 0x020009AF RID: 2479
		public enum RadialPushMode
		{
			// Token: 0x0400457E RID: 17790
			Proximity,
			// Token: 0x0400457F RID: 17791
			Absolute
		}
	}
}
