using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020001D6 RID: 470
	public class LevelAreaModule : LevelModule
	{
		// Token: 0x06001537 RID: 5431 RVA: 0x00093F87 File Offset: 0x00092187
		public List<ValueDropdownItem<string>> GetAllAreaID()
		{
			return Catalog.GetDropdownAllID(Category.Area, "None");
		}

		// Token: 0x06001538 RID: 5432 RVA: 0x00093F95 File Offset: 0x00092195
		public override IEnumerator OnLoadCoroutine()
		{
			AreaManager areaManager = AreaManager.Instance;
			if (areaManager == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(new GameObject("AreaManager"));
				gameObject.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
				areaManager = gameObject.gameObject.AddComponent<AreaManager>();
			}
			areaManager.AreaFullMemoryDepth = this.areaFullMemoryDepth;
			areaManager.AreaLiteMemoryDepth = this.areaLiteMemoryDepth;
			areaManager.AreaCullDepth = this.areaCullDepth;
			int dungeonLength = this.ParseDungeonLength();
			Dictionary<string, Tuple<int, float>> dictionaryTableIdIndeDropWeight = null;
			bool isValidId = false;
			string dungeonRoomId;
			if (this.level.options.TryGetValue(LevelOption.DungeonRoom.Get(), out dungeonRoomId))
			{
				if (Catalog.GetData<AreaData>(dungeonRoomId, false) != null)
				{
					isValidId = true;
				}
				else if (Catalog.GetData<AreaCollectionData>(dungeonRoomId, false) != null)
				{
					isValidId = true;
				}
				if (isValidId)
				{
					dictionaryTableIdIndeDropWeight = LevelAreaModule.IncreaseAreaIdSpawnChance(dungeonRoomId);
				}
			}
			LevelAreaModule.AreaSpawnerData areaSpawnerData = this.areaByLength[dungeonLength];
			SpawnableArea root = Catalog.GetData<AreaCollectionData>(areaSpawnerData.areaCollectionId, true).GetSpawnableArea(AreaRotationHelper.Rotation.Front, -1, Vector3.zero, areaSpawnerData.numberCreature, areaSpawnerData.isSharedNPCAlert);
			if (isValidId)
			{
				SpawnableArea area = LevelAreaModule.GetRoomFromTree(dungeonRoomId, root);
				if (area != null)
				{
					root = area;
				}
				else
				{
					Debug.LogError("AreaCollection : " + areaSpawnerData.areaCollectionId + " fail to create with specific area id : " + dungeonRoomId);
				}
			}
			LevelAreaModule.ResetAreaIdSpawnChance(dictionaryTableIdIndeDropWeight);
			bool terminate = false;
			yield return areaManager.Init(root).WrapSafely(delegate(Exception error)
			{
				terminate = true;
				Debug.LogError(string.Format("Error while initializing AreaManager : {0}", error));
			});
			if (terminate)
			{
				throw new Exception("Error while initializing AreaManager");
			}
			yield break;
		}

		// Token: 0x06001539 RID: 5433 RVA: 0x00093FA4 File Offset: 0x000921A4
		private static SpawnableArea GetRoomFromTree(string dungeonRoomId, SpawnableArea root)
		{
			if (string.IsNullOrEmpty(dungeonRoomId))
			{
				return null;
			}
			List<int> list;
			List<SpawnableArea> tree = root.GetBreadthTree(out list);
			int treeCount = tree.Count;
			for (int indexTree = 0; indexTree < treeCount; indexTree++)
			{
				if (tree[indexTree].AreaDataId.Equals(dungeonRoomId))
				{
					return tree[indexTree];
				}
			}
			return null;
		}

		// Token: 0x0600153A RID: 5434 RVA: 0x00093FF4 File Offset: 0x000921F4
		private static void ResetAreaIdSpawnChance(Dictionary<string, Tuple<int, float>> dictionaryTableIdIndeDropWeight)
		{
			if (dictionaryTableIdIndeDropWeight != null)
			{
				foreach (KeyValuePair<string, Tuple<int, float>> item in dictionaryTableIdIndeDropWeight)
				{
					string key = item.Key;
					int dropIndex = item.Value.Item1;
					float dropPreviousWeight = item.Value.Item2;
					Catalog.GetData<AreaTable>(key, true).areaSettingTable.drops[dropIndex].probabilityWeight = dropPreviousWeight;
				}
			}
		}

		// Token: 0x0600153B RID: 5435 RVA: 0x0009407C File Offset: 0x0009227C
		private static Dictionary<string, Tuple<int, float>> IncreaseAreaIdSpawnChance(string dungeonRoomId)
		{
			Dictionary<string, Tuple<int, float>> dictionaryTableIdIndeDropWeight = new Dictionary<string, Tuple<int, float>>();
			List<CatalogData> tableDataList = Catalog.GetDataList(Category.AreaTable);
			if (tableDataList != null)
			{
				int tableCount = tableDataList.Count;
				for (int indexTableData = 0; indexTableData < tableCount; indexTableData++)
				{
					AreaTable tempAreaTable = tableDataList[indexTableData] as AreaTable;
					if (tempAreaTable != null)
					{
						List<DropTable<AreaTable.AreaSettings>.Drop> dropList = tempAreaTable.areaSettingTable.drops;
						int dropCount = dropList.Count;
						for (int indexDrop = 0; indexDrop < dropCount; indexDrop++)
						{
							if (dropList[indexDrop].dropItem.bpGeneratorIdContainer.dataId.Equals(dungeonRoomId))
							{
								Tuple<int, float> tuple = new Tuple<int, float>(indexDrop, dropList[indexDrop].probabilityWeight);
								dropList[indexDrop].probabilityWeight = 1000000f;
								dictionaryTableIdIndeDropWeight.Add(tempAreaTable.id, tuple);
							}
						}
					}
				}
			}
			return dictionaryTableIdIndeDropWeight;
		}

		// Token: 0x0600153C RID: 5436 RVA: 0x00094150 File Offset: 0x00092350
		private int ParseDungeonLength()
		{
			int dungeonLength = 0;
			string dungeonLengthString;
			if (this.level.options.TryGetValue(LevelOption.DungeonLength.Get(), out dungeonLengthString))
			{
				int.TryParse(dungeonLengthString, out dungeonLength);
				dungeonLength--;
				if (dungeonLength < 0)
				{
					dungeonLength = 0;
				}
			}
			if (dungeonLength >= this.areaByLength.Count)
			{
				dungeonLength = this.areaByLength.Count - 1;
			}
			return dungeonLength;
		}

		// Token: 0x04001531 RID: 5425
		private const string AREA_MANAGER_ROOT_NAME = "AreaManager";

		// Token: 0x04001532 RID: 5426
		public int areaFullMemoryDepth = 100;

		// Token: 0x04001533 RID: 5427
		public int areaLiteMemoryDepth = 100;

		// Token: 0x04001534 RID: 5428
		public int areaCullDepth = 2;

		// Token: 0x04001535 RID: 5429
		public List<LevelAreaModule.AreaSpawnerData> areaByLength;

		// Token: 0x0200080A RID: 2058
		[Serializable]
		public class AreaSpawnerData
		{
			// Token: 0x06003EB9 RID: 16057 RVA: 0x00184A2F File Offset: 0x00182C2F
			public List<ValueDropdownItem<string>> GetAllAreaCollectionID()
			{
				return Catalog.GetDropdownAllID(Category.AreaCollection, "None");
			}

			// Token: 0x0400403D RID: 16445
			public string areaCollectionId = "";

			// Token: 0x0400403E RID: 16446
			public int numberCreature;

			// Token: 0x0400403F RID: 16447
			public bool isSharedNPCAlert;
		}
	}
}
