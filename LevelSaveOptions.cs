using System;

namespace ThunderRoad
{
	// Token: 0x0200032B RID: 811
	[Flags]
	public enum LevelSaveOptions
	{
		// Token: 0x040025CB RID: 9675
		PlayerHolsters = 1,
		// Token: 0x040025CC RID: 9676
		PlayerGrabbedItems = 2,
		// Token: 0x040025CD RID: 9677
		PlayerRacks = 4,
		// Token: 0x040025CE RID: 9678
		LevelCreatures = 8,
		// Token: 0x040025CF RID: 9679
		PlayerGrabbedCreatures = 16
	}
}
