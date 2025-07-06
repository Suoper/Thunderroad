using System;

namespace ThunderRoad
{
	// Token: 0x0200022A RID: 554
	[Serializable]
	public class SpellPowerData : SpellData
	{
		// Token: 0x0600176E RID: 5998 RVA: 0x0009D6C3 File Offset: 0x0009B8C3
		public new SpellPowerData Clone()
		{
			return base.MemberwiseClone() as SpellPowerData;
		}

		// Token: 0x0600176F RID: 5999 RVA: 0x0009D6D0 File Offset: 0x0009B8D0
		public virtual void Load(Mana mana)
		{
			this.mana = mana;
		}

		// Token: 0x06001770 RID: 6000 RVA: 0x0009D6D9 File Offset: 0x0009B8D9
		public virtual void Use(bool active)
		{
		}

		// Token: 0x06001771 RID: 6001 RVA: 0x0009D6DB File Offset: 0x0009B8DB
		public virtual void Update()
		{
		}

		// Token: 0x06001772 RID: 6002 RVA: 0x0009D6DD File Offset: 0x0009B8DD
		public virtual void Unload()
		{
		}

		// Token: 0x06001773 RID: 6003 RVA: 0x0009D6DF File Offset: 0x0009B8DF
		public override int GetCurrentVersion()
		{
			return 0;
		}

		// Token: 0x040016E3 RID: 5859
		[NonSerialized]
		public Mana mana;
	}
}
