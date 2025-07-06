using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020001DE RID: 478
	public class LevelTestAreaModule : LevelModule
	{
		// Token: 0x06001581 RID: 5505 RVA: 0x00095C5B File Offset: 0x00093E5B
		public override IEnumerator OnLoadCoroutine()
		{
			AreaManager areaManager = AreaManager.Instance;
			if (areaManager == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(new GameObject());
				gameObject.transform.position = Vector3.zero;
				gameObject.transform.rotation = Quaternion.identity;
				areaManager = gameObject.gameObject.AddComponent<AreaManager>();
				gameObject.name = "AreaManager";
			}
			areaManager.AreaFullMemoryDepth = 100;
			areaManager.AreaLiteMemoryDepth = 100;
			areaManager.AreaCullDepth = 2;
			int dungeonLength = 0;
			string dungeonLengthString;
			if (this.level.options.TryGetValue(LevelOption.DungeonLength.Get(), out dungeonLengthString))
			{
				int.TryParse(dungeonLengthString, out dungeonLength);
				if (dungeonLength < 0)
				{
					dungeonLength = 0;
				}
			}
			int dungeonDifficulty = 0;
			string dungeonDifficultyString;
			if (this.level.options.TryGetValue(LevelOption.Difficulty.Get(), out dungeonDifficultyString))
			{
				int.TryParse(dungeonDifficultyString, out dungeonDifficulty);
				if (dungeonDifficulty < 0)
				{
					dungeonDifficulty = 0;
				}
			}
			string areaId;
			if (!this.level.options.TryGetValue(LevelOption.DungeonRoom.Get(), out areaId))
			{
				Debug.LogError("Need DungeonRoom option");
				yield break;
			}
			IAreaBlueprintGenerator bp = Catalog.GetData(Category.Area, areaId, false) as IAreaBlueprintGenerator;
			if (bp == null)
			{
				bp = (Catalog.GetData(Category.AreaCollection, areaId, false) as IAreaBlueprintGenerator);
			}
			if (bp == null)
			{
				Debug.LogError("Can not find area blueprint spawner for id : " + areaId);
				yield break;
			}
			SpawnableArea root;
			if (dungeonLength == 0)
			{
				root = bp.GetSpawnableArea(AreaRotationHelper.Rotation.Front, -1, Vector3.zero, dungeonDifficulty, false);
				yield return areaManager.Init(root);
				yield break;
			}
			root = bp.GetSpawnableArea(AreaRotationHelper.Rotation.Front, -1, Vector3.zero, dungeonDifficulty, false);
			List<AreaData.AreaConnection> connections = bp.GetConnections();
			int connectionCount = connections.Count;
			System.Random rng = new System.Random(Level.seed);
			List<IAreaBlueprintGenerator> areaListToConnect = this.GetAllAreaConnectable(bp);
			int areaToConnectCount = areaListToConnect.Count;
			for (int indexConnection = 0; indexConnection < connectionCount; indexConnection++)
			{
				AreaData.AreaConnection connection = connections[indexConnection];
				AreaRotationHelper.Face entranceFace = AreaRotationHelper.GetOppositeFace(connection.face);
				Vector3 exitPosition = root.GetConnectionPosition(bp.GetId(), indexConnection);
				int otherIndex = 0;
				string otherBpId = null;
				SpawnableArea spawnableArea = this.GetConnectSpawnableArea(rng, areaListToConnect, areaToConnectCount, connection, entranceFace, exitPosition, out otherIndex, out otherBpId);
				if (spawnableArea != null)
				{
					SpawnableArea.ConnectAreas(root, bp.GetId(), indexConnection, spawnableArea, otherBpId, otherIndex, false);
				}
			}
			yield return areaManager.Init(root);
			yield break;
		}

		// Token: 0x06001582 RID: 5506 RVA: 0x00095C6C File Offset: 0x00093E6C
		private List<IAreaBlueprintGenerator> GetAllAreaConnectable(IAreaBlueprintGenerator bp)
		{
			List<IAreaBlueprintGenerator> allAreaConnectable = new List<IAreaBlueprintGenerator>();
			List<AreaData.AreaConnection> connections = bp.GetConnections();
			int connectionCount = connections.Count;
			HashSet<string> allConnectionTypeID = new HashSet<string>();
			for (int indexConnection = 0; indexConnection < connectionCount; indexConnection++)
			{
				List<AreaData.AreaConnectionTypeIdContainer> listConnectionTypes = connections[indexConnection].connectionTypeIdContainerList;
				int connectionTypesCount = listConnectionTypes.Count;
				for (int indexConnectionType = 0; indexConnectionType < connectionTypesCount; indexConnectionType++)
				{
					allConnectionTypeID.Add(listConnectionTypes[indexConnectionType].dataId);
				}
			}
			List<CatalogData> allArea = Catalog.GetCategoryData(Category.Area).catalogDatas;
			int allAreaCount = allArea.Count;
			for (int indexArea = 0; indexArea < allAreaCount; indexArea++)
			{
				AreaData area = allArea[indexArea] as AreaData;
				if (area != null && area.IsSpawnable() && (area.areaGlobalParameters == null || area.areaGlobalParameters.Length == 0))
				{
					List<AreaData.AreaConnection> tempAreaConnections = area.GetConnections();
					int tempAreaConnectionCount = tempAreaConnections.Count;
					for (int indexConnection2 = 0; indexConnection2 < tempAreaConnectionCount; indexConnection2++)
					{
						List<AreaData.AreaConnectionTypeIdContainer> listConnectionTypes2 = tempAreaConnections[indexConnection2].connectionTypeIdContainerList;
						int connectionTypesCount2 = listConnectionTypes2.Count;
						for (int indexConnectionType2 = 0; indexConnectionType2 < connectionTypesCount2; indexConnectionType2++)
						{
							if (allConnectionTypeID.Contains(listConnectionTypes2[indexConnectionType2].dataId))
							{
								allAreaConnectable.Add(area);
								break;
							}
						}
					}
				}
			}
			return allAreaConnectable;
		}

		// Token: 0x06001583 RID: 5507 RVA: 0x00095DB4 File Offset: 0x00093FB4
		private SpawnableArea GetConnectSpawnableArea(System.Random rng, List<IAreaBlueprintGenerator> areaListToConnect, int areaToConnectCount, AreaData.AreaConnection connection, AreaRotationHelper.Face entranceFace, Vector3 exitPosition, out int indexConnection, out string bpId)
		{
			List<int> indexList = new List<int>();
			for (int indexArea = 0; indexArea < areaToConnectCount; indexArea++)
			{
				indexList.Add(indexArea);
			}
			while (indexList.Count > 0)
			{
				int rngIndex = rng.Next(0, indexList.Count);
				int indexArea2 = indexList[rngIndex];
				IAreaBlueprintGenerator tempAreaBp = areaListToConnect[indexArea2];
				if (tempAreaBp != null)
				{
					List<AreaData.AreaConnection> otherConnectionList = tempAreaBp.GetConnections();
					int otherConnectionCount = otherConnectionList.Count;
					for (int indexOtherConnection = 0; indexOtherConnection < otherConnectionCount; indexOtherConnection++)
					{
						AreaData.AreaConnection otherConnection = otherConnectionList[indexOtherConnection];
						if (connection.IsConnectionValid(otherConnection))
						{
							List<AreaRotationHelper.Rotation> rotationList = tempAreaBp.GetAllowedRotation();
							int rotationCount = rotationList.Count;
							for (int indexRotation = 0; indexRotation < rotationCount; indexRotation++)
							{
								AreaRotationHelper.Rotation tempRotation = rotationList[indexRotation];
								if (AreaRotationHelper.RotateFace(otherConnection.face, tempRotation) == entranceFace)
								{
									indexConnection = indexOtherConnection;
									bpId = tempAreaBp.GetId();
									return tempAreaBp.GetSpawnableArea(tempRotation, indexConnection, exitPosition, 0, false);
								}
							}
						}
					}
				}
				indexList.RemoveAt(rngIndex);
			}
			indexConnection = -1;
			bpId = string.Empty;
			return null;
		}

		// Token: 0x0400156A RID: 5482
		private const string AREA_MANAGER_ROOT_NAME = "AreaManager";
	}
}
