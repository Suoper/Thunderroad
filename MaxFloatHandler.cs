using System;

namespace ThunderRoad
{
	// Token: 0x02000364 RID: 868
	public class MaxFloatHandler : ValueHandler<float>
	{
		// Token: 0x06002911 RID: 10513 RVA: 0x00117560 File Offset: 0x00115760
		public MaxFloatHandler()
		{
			this.baseValue = 1f;
		}

		// Token: 0x06002912 RID: 10514 RVA: 0x00117573 File Offset: 0x00115773
		public MaxFloatHandler(float baseValue)
		{
			this.baseValue = baseValue;
		}

		// Token: 0x06002913 RID: 10515 RVA: 0x00117584 File Offset: 0x00115784
		protected override void Refresh()
		{
			this.value = this.baseValue;
			foreach (float each in this.handlers.Values)
			{
				if (each > this.value)
				{
					this.value = each;
				}
			}
		}

		// Token: 0x06002914 RID: 10516 RVA: 0x001175F4 File Offset: 0x001157F4
		public static implicit operator MaxFloatHandler(float value)
		{
			return new MaxFloatHandler(value);
		}
	}
}
