using System;
using System.Collections.Generic;

namespace ThunderRoad
{
	// Token: 0x020001E5 RID: 485
	[Serializable]
	public abstract class LootTableBase : CatalogData
	{
		// Token: 0x060015AE RID: 5550 RVA: 0x0009715C File Offset: 0x0009535C
		public virtual ItemData PickOne(int level = 0, int searchDepth = 0, Random randomGen = null)
		{
			return null;
		}

		// Token: 0x060015AF RID: 5551 RVA: 0x0009715F File Offset: 0x0009535F
		public virtual List<ItemData> Pick(int level = 0, int searchDepth = 0, Random randomGen = null)
		{
			return null;
		}

		// Token: 0x060015B0 RID: 5552 RVA: 0x00097162 File Offset: 0x00095362
		public virtual List<ItemData> GetAll(int level = -1, int pickCount = 0)
		{
			return new List<ItemData>();
		}

		// Token: 0x060015B1 RID: 5553 RVA: 0x00097169 File Offset: 0x00095369
		public virtual bool DoesLootTableContainItemID(string id, int level = -1, int depth = 0)
		{
			return false;
		}

		// Token: 0x060015B2 RID: 5554 RVA: 0x0009716C File Offset: 0x0009536C
		public virtual void RenameItem(string itemId, string newName)
		{
		}
	}
}
