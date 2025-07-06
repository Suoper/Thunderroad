using System;

namespace ThunderRoad
{
	// Token: 0x02000366 RID: 870
	public class BoolHandler : ValueHandler<bool>
	{
		// Token: 0x06002917 RID: 10519 RVA: 0x00117678 File Offset: 0x00115878
		public BoolHandler(bool defaultValue)
		{
			this.baseValue = defaultValue;
		}

		// Token: 0x06002918 RID: 10520 RVA: 0x00117688 File Offset: 0x00115888
		protected override void Refresh()
		{
			this.value = this.baseValue;
			foreach (bool each in this.handlers.Values)
			{
				if (each != this.baseValue)
				{
					this.value = each;
					break;
				}
			}
		}

		// Token: 0x06002919 RID: 10521 RVA: 0x001176F8 File Offset: 0x001158F8
		public void Add(object handler)
		{
			base.Add(handler, !this.baseValue);
		}
	}
}
