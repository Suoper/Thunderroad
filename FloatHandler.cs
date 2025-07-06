using System;

namespace ThunderRoad
{
	// Token: 0x02000363 RID: 867
	public class FloatHandler : ValueHandler<float>
	{
		// Token: 0x0600290D RID: 10509 RVA: 0x001174C8 File Offset: 0x001156C8
		public FloatHandler()
		{
			this.baseValue = 1f;
		}

		// Token: 0x0600290E RID: 10510 RVA: 0x001174DB File Offset: 0x001156DB
		public FloatHandler(float baseValue)
		{
			this.baseValue = baseValue;
		}

		// Token: 0x0600290F RID: 10511 RVA: 0x001174EC File Offset: 0x001156EC
		protected override void Refresh()
		{
			this.value = this.baseValue;
			foreach (float each in this.handlers.Values)
			{
				this.value *= each;
			}
		}

		// Token: 0x06002910 RID: 10512 RVA: 0x00117558 File Offset: 0x00115758
		public static implicit operator FloatHandler(float value)
		{
			return new FloatHandler(value);
		}
	}
}
