using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000242 RID: 578
	public class StatusDataBurning : StatusData
	{
		// Token: 0x0600185E RID: 6238 RVA: 0x000A15BC File Offset: 0x0009F7BC
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			this.heatAnimationData = Catalog.GetData<AnimationData>(this.heatAnimationId, true);
			this.igniteEffectData = Catalog.GetData<EffectData>(this.igniteEffectId, true);
			this.smokingMainEffectData = Catalog.GetData<EffectData>(this.smokingMainEffectId, true);
			this.smokingLimbEffectData = Catalog.GetData<EffectData>(this.smokingLimbEffectId, true);
			this.burningMainEffectData = Catalog.GetData<EffectData>(this.burningMainEffectId, true);
			this.burningLimbEffectData = Catalog.GetData<EffectData>(this.burningLimbEffectId, true);
		}

		// Token: 0x0600185F RID: 6239 RVA: 0x000A163C File Offset: 0x0009F83C
		public override bool SpawnEffect(ThunderEntity entity, out EffectInstance effect)
		{
			effect = null;
			Creature creature = entity as Creature;
			if (creature == null)
			{
				Item item = entity as Item;
				if (item != null)
				{
					if (this.itemEffectData != null)
					{
						EffectData itemEffectData = this.itemEffectData;
						effect = ((itemEffectData != null) ? itemEffectData.Spawn(item.transform, true, null, false) : null);
						if (effect == null)
						{
							return false;
						}
						for (int i = 0; i < item.colliderGroups.Count; i++)
						{
							if (item.colliderGroups[i].imbueEmissionRenderer)
							{
								effect.SetRenderer(item.colliderGroups[0].imbueEmissionRenderer, false);
								break;
							}
						}
					}
				}
			}
			else if (this.creatureEffectData != null)
			{
				EffectData creatureEffectData = this.creatureEffectData;
				effect = ((creatureEffectData != null) ? creatureEffectData.Spawn(creature.ragdoll.targetPart.meshBone.transform, true, null, false) : null);
				if (effect == null)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06001860 RID: 6240 RVA: 0x000A171C File Offset: 0x0009F91C
		public bool SpawnEffects(ThunderEntity entity, EffectData torsoEffect, EffectData limbEffect, out List<EffectInstance> effects)
		{
			effects = new List<EffectInstance>();
			Creature creature = entity as Creature;
			if (creature == null)
			{
				return false;
			}
			EffectInstance mainEffect = (torsoEffect != null) ? torsoEffect.Spawn(creature.ragdoll.targetPart.meshBone.transform, true, null, false) : null;
			if (mainEffect != null)
			{
				effects.Add(mainEffect);
			}
			if (creature.data.type != CreatureType.Human)
			{
				return true;
			}
			EffectInstance leftLegEffect = (limbEffect != null) ? limbEffect.Spawn(creature.ragdoll.GetPart(RagdollPart.Type.LeftLeg, RagdollPart.Section.Upper).meshBone.transform, true, null, false) : null;
			EffectInstance rightLegEffect = (limbEffect != null) ? limbEffect.Spawn(creature.ragdoll.GetPart(RagdollPart.Type.RightLeg, RagdollPart.Section.Upper).meshBone.transform, true, null, false) : null;
			EffectInstance leftArmEffect = (limbEffect != null) ? limbEffect.Spawn(creature.ragdoll.GetPart(RagdollPart.Type.LeftArm, RagdollPart.Section.Upper).meshBone.transform, true, null, false) : null;
			EffectInstance rightArmEffect = (limbEffect != null) ? limbEffect.Spawn(creature.ragdoll.GetPart(RagdollPart.Type.RightArm, RagdollPart.Section.Upper).meshBone.transform, true, null, false) : null;
			if (leftArmEffect != null)
			{
				leftArmEffect.SetSize(0.6f);
			}
			if (rightArmEffect != null)
			{
				rightArmEffect.SetSize(0.6f);
			}
			if (leftArmEffect != null)
			{
				effects.Add(leftArmEffect);
			}
			if (rightArmEffect != null)
			{
				effects.Add(rightArmEffect);
			}
			if (leftLegEffect != null)
			{
				effects.Add(leftLegEffect);
			}
			if (rightLegEffect != null)
			{
				effects.Add(rightLegEffect);
			}
			return true;
		}

		// Token: 0x0400175D RID: 5981
		public const string Heat = "Heat";

		// Token: 0x0400175E RID: 5982
		public const string HeatGainMult = "HeatGainMult";

		// Token: 0x0400175F RID: 5983
		public const string HeatLossMult = "HeatLossMult";

		// Token: 0x04001760 RID: 5984
		public const string Char = "CharAmount";

		// Token: 0x04001761 RID: 5985
		public float burnDelay = 0.5f;

		// Token: 0x04001762 RID: 5986
		public float damagePerTick = 0.75f;

		// Token: 0x04001763 RID: 5987
		public float damagePerTickPlayer = 0.75f;

		// Token: 0x04001764 RID: 5988
		public float heatReductionPerSecond = 1f;

		// Token: 0x04001765 RID: 5989
		public float heatReductionPerSecondKilled = 2f;

		// Token: 0x04001766 RID: 5990
		public float heatReductionPerSecondPlayer = 4f;

		// Token: 0x04001767 RID: 5991
		public string heatAnimationId = "HumanFireDeaths";

		// Token: 0x04001768 RID: 5992
		[NonSerialized]
		public AnimationData heatAnimationData;

		// Token: 0x04001769 RID: 5993
		public float maxHeat = 100f;

		// Token: 0x0400176A RID: 5994
		public float animationStartTimeMax = 0.5f;

		// Token: 0x0400176B RID: 5995
		public float fullCharTime = 10f;

		// Token: 0x0400176C RID: 5996
		public float charRevealStep = 0.02f;

		// Token: 0x0400176D RID: 5997
		public AnimationCurve charResistanceCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);

		// Token: 0x0400176E RID: 5998
		public bool allowVisualCharring = true;

		// Token: 0x0400176F RID: 5999
		public bool freezeOnFullChar = true;

		// Token: 0x04001770 RID: 6000
		public bool weakenJointsOnFullChar = true;

		// Token: 0x04001771 RID: 6001
		public float fullCharCharJointMultiplier = 1f;

		// Token: 0x04001772 RID: 6002
		public string igniteEffectId = "SpellFireBurningSpread";

		// Token: 0x04001773 RID: 6003
		public string smokingMainEffectId;

		// Token: 0x04001774 RID: 6004
		[NonSerialized]
		public EffectData smokingMainEffectData;

		// Token: 0x04001775 RID: 6005
		public string smokingLimbEffectId;

		// Token: 0x04001776 RID: 6006
		[NonSerialized]
		public EffectData smokingLimbEffectData;

		// Token: 0x04001777 RID: 6007
		public string burningMainEffectId;

		// Token: 0x04001778 RID: 6008
		[NonSerialized]
		public EffectData burningMainEffectData;

		// Token: 0x04001779 RID: 6009
		public string burningLimbEffectId;

		// Token: 0x0400177A RID: 6010
		[NonSerialized]
		public EffectData burningLimbEffectData;

		// Token: 0x0400177B RID: 6011
		[NonSerialized]
		public EffectData igniteEffectData;
	}
}
