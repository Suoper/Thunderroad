using System;

namespace ThunderRoad
{
	// Token: 0x0200023B RID: 571
	public class Slowed : Status
	{
		// Token: 0x17000178 RID: 376
		// (get) Token: 0x06001814 RID: 6164 RVA: 0x000A07AA File Offset: 0x0009E9AA
		public override bool ReapplyOnValueChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06001815 RID: 6165 RVA: 0x000A07AD File Offset: 0x0009E9AD
		public override void Spawn(StatusData data, ThunderEntity entity)
		{
			base.Spawn(data, entity);
			this.data = (data as StatusDataSlowed);
		}

		// Token: 0x06001816 RID: 6166 RVA: 0x000A07C4 File Offset: 0x0009E9C4
		protected override object GetValue()
		{
			float output = 1f;
			foreach (ValueTuple<float, object> valueTuple in this.handlers.Values)
			{
				object parameter = valueTuple.Item2;
				if (parameter is float)
				{
					float multiplier = (float)parameter;
					output *= multiplier;
				}
			}
			return output;
		}

		// Token: 0x06001817 RID: 6167 RVA: 0x000A083C File Offset: 0x0009EA3C
		public override void Apply()
		{
			base.Apply();
			float speed = this.data.baseSlowMult;
			object value = this.value;
			if (value is float)
			{
				float mult = (float)value;
				speed *= mult;
			}
			Creature player = this.entity as Creature;
			if (player != null && player.isPlayer)
			{
				player.mana.focusConsumptionMult.Add(this, speed);
				return;
			}
			this.entity.SetPhysicModifier(this, new float?(speed), 1f, 3f / speed);
			Golem golem = this.entity as Golem;
			if (golem != null)
			{
				golem.speed.Add(this, speed);
			}
			Creature creature = this.entity as Creature;
			if (creature == null)
			{
				return;
			}
			creature.locomotion.SetAllSpeedModifiers(this, speed);
			creature.animator.speed = speed;
			BrainModuleSpeak module = creature.brain.instance.GetModule<BrainModuleSpeak>(false);
			if (module == null)
			{
				return;
			}
			module.SetPitchMultiplier(speed);
		}

		// Token: 0x06001818 RID: 6168 RVA: 0x000A0928 File Offset: 0x0009EB28
		public override void Remove()
		{
			base.Remove();
			this.entity.RemovePhysicModifier(this);
			Creature player = this.entity as Creature;
			if (player != null && player.isPlayer)
			{
				player.mana.focusConsumptionMult.Remove(this);
			}
			Golem golem = this.entity as Golem;
			if (golem != null)
			{
				golem.speed.Remove(this);
			}
			Creature creature = this.entity as Creature;
			if (creature == null)
			{
				return;
			}
			creature.locomotion.RemoveSpeedModifier(this);
			creature.animator.speed = 1f;
			BrainModuleSpeak module = creature.brain.instance.GetModule<BrainModuleSpeak>(true);
			if (module == null)
			{
				return;
			}
			module.ClearPitchMultiplier();
		}

		// Token: 0x04001741 RID: 5953
		public new StatusDataSlowed data;
	}
}
