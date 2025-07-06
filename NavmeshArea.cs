using System;

namespace ThunderRoad
{
	// Token: 0x02000321 RID: 801
	[Flags]
	public enum NavmeshArea
	{
		// Token: 0x04002595 RID: 9621
		Walkable = 1,
		// Token: 0x04002596 RID: 9622
		NotWalkable = 2,
		// Token: 0x04002597 RID: 9623
		Jump = 4,
		// Token: 0x04002598 RID: 9624
		Door = 8,
		// Token: 0x04002599 RID: 9625
		Edge = 16
	}
}
