using System;

namespace ThunderRoad
{
	// Token: 0x0200023C RID: 572
	public class Speed : Status
	{
		// Token: 0x0600181A RID: 6170 RVA: 0x000A09DB File Offset: 0x0009EBDB
		public override void Spawn(StatusData data, ThunderEntity entity)
		{
			base.Spawn(data, entity);
			this.data = (data as StatusDataSpeed);
		}

		// Token: 0x0600181B RID: 6171 RVA: 0x000A09F4 File Offset: 0x0009EBF4
		public override void Apply()
		{
			base.Apply();
			float multiplier = (float)this.value;
			Creature creature = this.entity as Creature;
			if (creature != null)
			{
				creature.currentLocomotion.SetSpeedModifier(this, multiplier, multiplier, multiplier, multiplier, 1f, 1f);
			}
		}

		// Token: 0x0600181C RID: 6172 RVA: 0x000A0A3C File Offset: 0x0009EC3C
		public override void OnValueChange()
		{
			base.OnValueChange();
			float multiplier = (float)this.value;
			Creature creature = this.entity as Creature;
			if (creature != null)
			{
				creature.currentLocomotion.SetSpeedModifier(this, multiplier, multiplier, multiplier, multiplier, 1f, 1f);
			}
		}

		// Token: 0x0600181D RID: 6173 RVA: 0x000A0A84 File Offset: 0x0009EC84
		protected override object GetValue()
		{
			float output = 1f;
			foreach (ValueTuple<float, object> valueTuple in this.handlers.Values)
			{
				object item = valueTuple.Item2;
				if (item is float)
				{
					float param = (float)item;
					output *= param;
				}
			}
			return output;
		}

		// Token: 0x0600181E RID: 6174 RVA: 0x000A0AFC File Offset: 0x0009ECFC
		public override void Remove()
		{
			base.Remove();
			Creature creature = this.entity as Creature;
			if (creature != null)
			{
				creature.currentLocomotion.RemoveSpeedModifier(this);
			}
		}

		// Token: 0x04001742 RID: 5954
		public new StatusDataSpeed data;
	}
}
