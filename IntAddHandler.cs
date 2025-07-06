using System;

namespace ThunderRoad
{
	// Token: 0x02000365 RID: 869
	public class IntAddHandler : ValueHandler<int>
	{
		// Token: 0x06002915 RID: 10517 RVA: 0x001175FC File Offset: 0x001157FC
		public IntAddHandler()
		{
			this.baseValue = 0;
		}

		// Token: 0x06002916 RID: 10518 RVA: 0x0011760C File Offset: 0x0011580C
		protected override void Refresh()
		{
			this.value = this.baseValue;
			foreach (int each in this.handlers.Values)
			{
				this.value += each;
			}
		}
	}
}
