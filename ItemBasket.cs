using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200034A RID: 842
	public class ItemBasket : ThunderBehaviour
	{
		// Token: 0x06002760 RID: 10080 RVA: 0x0010FBD7 File Offset: 0x0010DDD7
		public int GetMaxItems()
		{
			return Mathf.FloorToInt((this.maxSpawnHeight - this.startSpawnHeight) / this.spawnHeightIncrease) + 1;
		}

		// Token: 0x06002761 RID: 10081 RVA: 0x0010FBF4 File Offset: 0x0010DDF4
		public Vector3 GetRandomPositionAtHeight(int heightIndex)
		{
			return base.transform.position + new Vector3(0f, this.startSpawnHeight + (float)heightIndex * this.spawnHeightIncrease, 0f) + new Vector3(UnityEngine.Random.Range(-1f, 1f), 0f, UnityEngine.Random.Range(-1f, 1f)).normalized * this.spawnRadius;
		}

		// Token: 0x0400267B RID: 9851
		public float spawnRadius;

		// Token: 0x0400267C RID: 9852
		public float startSpawnHeight;

		// Token: 0x0400267D RID: 9853
		public float spawnHeightIncrease;

		// Token: 0x0400267E RID: 9854
		public float maxSpawnHeight;

		// Token: 0x0400267F RID: 9855
		public List<string> allowedItems = new List<string>();
	}
}
