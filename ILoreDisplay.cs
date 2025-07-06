using System;
using System.Collections.Generic;
using ThunderRoad.Modules;

namespace ThunderRoad
{
	// Token: 0x02000308 RID: 776
	public interface ILoreDisplay
	{
		// Token: 0x0600251B RID: 9499
		void SetLore(LoreModule loreModule, LoreSpawner spawner, LoreScriptableObject.LoreData loreData);

		// Token: 0x0600251C RID: 9500
		void SetMultipleLore(LoreModule loreModule, LoreSpawner spawner, List<LoreScriptableObject.LoreData> loreDatas);

		// Token: 0x0600251D RID: 9501
		void ReleaseLore();
	}
}
