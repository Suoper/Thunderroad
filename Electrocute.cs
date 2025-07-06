using System;

namespace ThunderRoad
{
	// Token: 0x02000238 RID: 568
	public class Electrocute : Status
	{
		// Token: 0x06001803 RID: 6147 RVA: 0x0009FF9A File Offset: 0x0009E19A
		public override void Spawn(StatusData data, ThunderEntity entity)
		{
			base.Spawn(data, entity);
			this.data = (data as StatusDataElectrocute);
		}

		// Token: 0x06001804 RID: 6148 RVA: 0x0009FFB0 File Offset: 0x0009E1B0
		public override void Apply()
		{
			base.Apply();
			Creature creature = this.entity as Creature;
			if (creature == null)
			{
				return;
			}
			if (creature.isPlayer)
			{
				creature.currentLocomotion.globalMoveSpeedMultiplier.Add(this, this.data.playerSpeedModifier);
				creature.mana.chargeSpeedMult.Add(this, this.data.chargeSpeedModifier);
			}
			creature.brain.AddNoStandUpModifier(this);
			creature.TryElectrocute(1f, float.PositiveInfinity, true, true, null);
		}

		// Token: 0x06001805 RID: 6149 RVA: 0x000A0034 File Offset: 0x0009E234
		public override void Remove()
		{
			base.Remove();
			Creature creature = this.entity as Creature;
			if (creature == null)
			{
				return;
			}
			if (creature.isPlayer)
			{
				creature.currentLocomotion.globalMoveSpeedMultiplier.Remove(this);
				creature.mana.chargeSpeedMult.Remove(this);
			}
			creature.brain.RemoveNoStandUpModifier(this);
			creature.brain.instance.GetModule<BrainModuleElectrocute>(true).StopElectrocute(true);
		}

		// Token: 0x06001806 RID: 6150 RVA: 0x000A00A8 File Offset: 0x0009E2A8
		public override void FullRemove()
		{
			base.FullRemove();
			Creature creature = this.entity as Creature;
			if (creature == null)
			{
				return;
			}
			if (Electrocute.knockdownOnEnd && !creature.isKilled && !creature.isPlayer)
			{
				creature.ragdoll.SetState(Ragdoll.State.Destabilized);
			}
		}

		// Token: 0x04001739 RID: 5945
		public static bool knockdownOnEnd;

		// Token: 0x0400173A RID: 5946
		public new StatusDataElectrocute data;
	}
}
