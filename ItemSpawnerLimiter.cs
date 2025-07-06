using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002D7 RID: 727
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Areas/ItemSpawnerLimiter.html")]
	public class ItemSpawnerLimiter : MonoBehaviour
	{
		// Token: 0x06002327 RID: 8999 RVA: 0x000F0D63 File Offset: 0x000EEF63
		private void Awake()
		{
			if (this.spawnOnLevelLoad)
			{
				EventManager.onLevelLoad += this.HandleLevelLoaded;
			}
		}

		// Token: 0x06002328 RID: 9000 RVA: 0x000F0D80 File Offset: 0x000EEF80
		private void Start()
		{
			if (!base.enabled)
			{
				return;
			}
			this.itemSpawners = base.GetComponentsInChildren<ItemSpawner>();
			foreach (ItemSpawner itemSpawner in this.itemSpawners)
			{
				if (itemSpawner.spawnerType == ItemSpawner.SpawnerType.SideRoom)
				{
					this.sideRoomSpawners.Add(itemSpawner);
				}
				if (itemSpawner.spawnerType == ItemSpawner.SpawnerType.Treasure)
				{
					this.treasureSpawners.Add(itemSpawner);
				}
				if (itemSpawner.spawnerType == ItemSpawner.SpawnerType.Reward)
				{
					this.rewardSpawners.Add(itemSpawner);
				}
			}
			this.totalSideRoomLootSpawners = this.sideRoomSpawners.Count;
			if (this.totalSideRoomLootSpawners > 0)
			{
				this.sideRoomLootSpawnersToActivate = UnityEngine.Random.Range(1, this.totalSideRoomLootSpawners + 1);
			}
			string dungeonLengthStr;
			Level.current.options.TryGetValue(LevelOption.DungeonLength.Get(), out dungeonLengthStr);
			int dungeonLength;
			if (dungeonLengthStr != null && int.TryParse(dungeonLengthStr, out dungeonLength))
			{
				this.treasureSpawnersToActivate = dungeonLength;
			}
			if (this.spawnOnStart && !this.spawnOnLevelLoad)
			{
				this.randomGen = new System.Random(Level.seed);
				this.SpawnAll();
			}
		}

		// Token: 0x06002329 RID: 9001 RVA: 0x000F0E84 File Offset: 0x000EF084
		public void SpawnAll()
		{
			this.spawnCount = 0;
			this.spawnChildCount = 0;
			this.itemSpawners = base.GetComponentsInChildren<ItemSpawner>();
			this.Shuffle<ItemSpawner>(this.itemSpawners);
			int countMax = (Common.GetQualityLevel(false) == QualityLevel.Android) ? this.androidMaxSpawn : this.maxSpawn;
			ItemSpawner[] array = this.itemSpawners;
			int i = 0;
			while (i < array.Length)
			{
				ItemSpawner itemSpawner = array[i];
				ItemSpawner.SpawnerType spawnerType = itemSpawner.spawnerType;
				if (spawnerType != ItemSpawner.SpawnerType.Treasure)
				{
					goto IL_7C;
				}
				if (this.activeTreasureSpawners < this.treasureSpawnersToActivate)
				{
					this.activeTreasureSpawners++;
					goto IL_7C;
				}
				IL_102:
				i++;
				continue;
				IL_7C:
				if (spawnerType == ItemSpawner.SpawnerType.SideRoom)
				{
					if (this.activeSideRoomLootSpawners >= this.sideRoomLootSpawnersToActivate)
					{
						goto IL_102;
					}
					this.activeSideRoomLootSpawners++;
				}
				if (itemSpawner.priority == ItemSpawner.Priority.Mandatory)
				{
					itemSpawner.Spawn(true, true, false, this.randomGen, -1, true);
					goto IL_102;
				}
				if (this.spawnCount < countMax)
				{
					int max = countMax - this.spawnCount;
					if (max > 1)
					{
						max = this.randomGen.Next(1, max + 1);
					}
					this.spawnCount += itemSpawner.Spawn(true, true, false, this.randomGen, max, true);
					goto IL_102;
				}
				goto IL_102;
			}
		}

		// Token: 0x0600232A RID: 9002 RVA: 0x000F0FA0 File Offset: 0x000EF1A0
		public void SpawnChild(ItemSpawner itemSpawner, bool allowDuplicates)
		{
			if (itemSpawner.priority == ItemSpawner.Priority.Mandatory)
			{
				itemSpawner.Spawn(false, true, false, this.randomGen, -1, allowDuplicates);
				return;
			}
			int countMax = (Common.GetQualityLevel(false) == QualityLevel.Android) ? this.androidMaxChildSpawn : this.maxChildSpawn;
			if (this.spawnChildCount >= countMax)
			{
				return;
			}
			int max = countMax - this.spawnChildCount;
			if (max > 1)
			{
				max = this.randomGen.Next(1, max + 1);
			}
			this.spawnChildCount += itemSpawner.Spawn(false, true, false, this.randomGen, max, allowDuplicates);
		}

		// Token: 0x0600232B RID: 9003 RVA: 0x000F1028 File Offset: 0x000EF228
		public void Shuffle<T>(IList<T> list)
		{
			int i = list.Count;
			while (i > 1)
			{
				i--;
				int j = this.randomGen.Next(i + 1);
				T value = list[j];
				list[j] = list[i];
				list[i] = value;
			}
		}

		// Token: 0x0600232C RID: 9004 RVA: 0x000F1074 File Offset: 0x000EF274
		public bool IsItemSpawning()
		{
			if (this.itemSpawners == null)
			{
				return false;
			}
			for (int i = 0; i < this.itemSpawners.Length; i++)
			{
				if (this.itemSpawners[i].IsCurrentlySpawning())
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600232D RID: 9005 RVA: 0x000F10B0 File Offset: 0x000EF2B0
		private void HandleLevelLoaded(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			EventManager.onLevelLoad -= this.HandleLevelLoaded;
			this.SpawnAll();
		}

		// Token: 0x04002227 RID: 8743
		[Tooltip("Maximum amount of items that can spawn in a room")]
		public int maxSpawn = 10;

		// Token: 0x04002228 RID: 8744
		[Tooltip("The maximum amount of child items in a room (items that have parent spawners)")]
		public int maxChildSpawn = 6;

		// Token: 0x04002229 RID: 8745
		[Tooltip("Maximum amount of items that can spawn in a room on the Android Variant")]
		public int androidMaxSpawn = 4;

		// Token: 0x0400222A RID: 8746
		[Tooltip("Maximum amount of child items that can spawn in a room on the Android Variant")]
		public int androidMaxChildSpawn = 2;

		// Token: 0x0400222B RID: 8747
		[Tooltip("When set to true, the items will spawn on start.")]
		public bool spawnOnStart = true;

		// Token: 0x0400222C RID: 8748
		[Tooltip("When set to true, the items will spawn on level load.")]
		public bool spawnOnLevelLoad;

		// Token: 0x0400222D RID: 8749
		[NonSerialized]
		public int spawnCount;

		// Token: 0x0400222E RID: 8750
		[NonSerialized]
		public int spawnChildCount;

		// Token: 0x0400222F RID: 8751
		[NonSerialized]
		public ItemSpawner[] itemSpawners;

		// Token: 0x04002230 RID: 8752
		[NonSerialized]
		public System.Random randomGen;

		// Token: 0x04002231 RID: 8753
		private int treasureSpawnersToActivate;

		// Token: 0x04002232 RID: 8754
		private int activeTreasureSpawners;

		// Token: 0x04002233 RID: 8755
		private int sideRoomLootSpawnersToActivate;

		// Token: 0x04002234 RID: 8756
		private int activeSideRoomLootSpawners;

		// Token: 0x04002235 RID: 8757
		private int totalSideRoomLootSpawners;

		// Token: 0x04002236 RID: 8758
		private List<ItemSpawner> sideRoomSpawners = new List<ItemSpawner>();

		// Token: 0x04002237 RID: 8759
		private List<ItemSpawner> treasureSpawners = new List<ItemSpawner>();

		// Token: 0x04002238 RID: 8760
		private List<ItemSpawner> rewardSpawners = new List<ItemSpawner>();
	}
}
