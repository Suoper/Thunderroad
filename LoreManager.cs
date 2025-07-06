using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002E1 RID: 737
	public class LoreManager : MonoBehaviour
	{
		// Token: 0x06002367 RID: 9063 RVA: 0x000F1E27 File Offset: 0x000F0027
		private void Awake()
		{
			LoreManager.instance = this;
			EventManager.onLevelLoad += this.HandleLevelLoaded;
			this.rng = new System.Random();
		}

		// Token: 0x06002368 RID: 9064 RVA: 0x000F1E4B File Offset: 0x000F004B
		private void OnDestroy()
		{
			LoreManager.instance = null;
			EventManager.onLevelLoad -= this.HandleLevelLoaded;
		}

		// Token: 0x06002369 RID: 9065 RVA: 0x000F1E64 File Offset: 0x000F0064
		public void HandleLevelLoaded(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.PopulateLoreAreas();
			}
		}

		// Token: 0x0600236A RID: 9066 RVA: 0x000F1E70 File Offset: 0x000F0070
		public void PopulateLoreAreas()
		{
			foreach (LoreArea loreArea in this.loreAreas)
			{
				this.activeLoreSpawners.AddRange(loreArea.loreSpawners);
			}
			this.Shuffle<LoreSpawner>(this.activeLoreSpawners, this.rng);
			foreach (LoreSpawner loreSpawner in this.activeLoreSpawners)
			{
				loreSpawner.Init();
				loreSpawner.PopulateLore();
			}
		}

		// Token: 0x0600236B RID: 9067 RVA: 0x000F1F24 File Offset: 0x000F0124
		public void Shuffle<T>(IList<T> list, System.Random randomGen)
		{
			int i = list.Count;
			while (i > 1)
			{
				i--;
				int j = randomGen.Next(i + 1);
				T value = list[j];
				list[j] = list[i];
				list[i] = value;
			}
		}

		// Token: 0x0600236C RID: 9068 RVA: 0x000F1F6C File Offset: 0x000F016C
		public void MarkAllLoreRead()
		{
			int count = 0;
			foreach (LoreSpawner loreSpawner in this.activeLoreSpawners)
			{
				count += loreSpawner.loreItems.Count;
				loreSpawner.MarkAsRead(false);
			}
			Debug.Log(string.Format("collected <color=red>{0}</color>", count));
		}

		// Token: 0x0400226C RID: 8812
		public List<LoreArea> loreAreas = new List<LoreArea>();

		// Token: 0x0400226D RID: 8813
		public List<LoreSpawner> activeLoreSpawners = new List<LoreSpawner>();

		// Token: 0x0400226E RID: 8814
		private float delayTime = 2f;

		// Token: 0x0400226F RID: 8815
		private System.Random rng;

		// Token: 0x04002270 RID: 8816
		public static LoreManager instance;
	}
}
