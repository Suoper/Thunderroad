using System;
using System.Collections;
using System.Collections.Generic;
using Koenigz.PerfectCulling;
using Shadowood;
using ThunderRoad.Pools;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace ThunderRoad
{
	// Token: 0x0200012A RID: 298
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Areas/Area.html")]
	[RequireComponent(typeof(AreaLightingGroupLiteMemoryToggle))]
	public class Area : MonoBehaviour, ICheckAsset
	{
		// Token: 0x170000CE RID: 206
		// (get) Token: 0x06000E4C RID: 3660 RVA: 0x0006571A File Offset: 0x0006391A
		public bool IsInitialized
		{
			get
			{
				return this.initialized;
			}
		}

		// Token: 0x1400006D RID: 109
		// (add) Token: 0x06000E4D RID: 3661 RVA: 0x00065724 File Offset: 0x00063924
		// (remove) Token: 0x06000E4E RID: 3662 RVA: 0x0006575C File Offset: 0x0006395C
		public event Area.CullChangeEvent onCullChange;

		// Token: 0x1400006E RID: 110
		// (add) Token: 0x06000E4F RID: 3663 RVA: 0x00065794 File Offset: 0x00063994
		// (remove) Token: 0x06000E50 RID: 3664 RVA: 0x000657CC File Offset: 0x000639CC
		public event Area.HideChangeEvent onHideChange;

		// Token: 0x06000E51 RID: 3665 RVA: 0x00065801 File Offset: 0x00063A01
		public IEnumerator Init(SpawnableArea spawnableArea, bool inLoadingMenu)
		{
			JobHandle bakeMeshHandle;
			bool bakeMeshRunning = this.BakeMeshCollider(out bakeMeshHandle);
			this.spawnableArea = spawnableArea;
			System.Random rng = new System.Random(Level.seed + spawnableArea.managedId);
			this.DisableAllPlayerSpawner();
			this.blendAudioSources = new List<Area.BlendAudioSource>();
			int audioSourcesToBlendCount = this.audioSourcesToBlend.Count;
			for (int i = 0; i < audioSourcesToBlendCount; i++)
			{
				AudioSource audioSource = this.audioSourcesToBlend[i];
				if (audioSource)
				{
					Area.BlendAudioSource blendAudioSource = new Area.BlendAudioSource(audioSource);
					this.blendAudioSources.Add(blendAudioSource);
					blendAudioSource.ApplyVolume(0f);
				}
			}
			if (this.gateways != null)
			{
				for (int indexGateway = 0; indexGateway < this.gateways.Length; indexGateway++)
				{
					SpawnableArea.ConnectedArea connectedAreaInfo = spawnableArea.GetConnectedArea(spawnableArea.AreaDataId, indexGateway);
					if (connectedAreaInfo == null)
					{
						this.gateways[indexGateway].gameObject.SetActive(false);
						GameObject blockerPrefab = null;
						if (spawnableArea.connectionBlockerPrefab.TryGetValue(indexGateway, out blockerPrefab) && blockerPrefab != null)
						{
							GameObject blocker = UnityEngine.Object.Instantiate<GameObject>(blockerPrefab, this.gateways[indexGateway].transform.position, this.gateways[indexGateway].transform.rotation, base.transform);
							this.blockers.Add(blocker);
						}
					}
					else
					{
						this.gateways[indexGateway].Init(spawnableArea, indexGateway, connectedAreaInfo.connectedArea, connectedAreaInfo.connectedAreaConnectionIndex);
						this.connectedGateways.Add(indexGateway, this.gateways[indexGateway]);
						SpawnableArea connectedSpawnableArea = connectedAreaInfo.connectedArea;
						if (connectedSpawnableArea.IsSpawned)
						{
							if (!connectedSpawnableArea.connectionGateSpawn.ContainsKey(connectedAreaInfo.connectedAreaConnectionIndex))
							{
								GameObject gatePrefab = null;
								if (spawnableArea.connectionGatePrefab.TryGetValue(indexGateway, out gatePrefab) && gatePrefab != null)
								{
									GameObject spawnGate = UnityEngine.Object.Instantiate<GameObject>(gatePrefab, this.gateways[indexGateway].transform.position, this.gateways[indexGateway].transform.rotation, this.rootNoCulling.transform);
									spawnableArea.connectionGateSpawn.Add(indexGateway, spawnGate);
								}
							}
							if (!connectedSpawnableArea.connectionLinkNavMesh.ContainsKey(connectedAreaInfo.connectedAreaConnectionIndex))
							{
								AreaConnectionTypeData connectionType = Catalog.GetData<AreaConnectionTypeData>(connectedAreaInfo.connectionTypeId, true);
								GameObject navMeshLinkRoot = UnityEngine.Object.Instantiate<GameObject>(new GameObject(), this.gateways[indexGateway].transform.position, this.gateways[indexGateway].transform.rotation, this.rootNoCulling.transform);
								NavMeshLink navMeshLink = navMeshLinkRoot.AddComponent<NavMeshLink>();
								navMeshLink.bidirectional = true;
								navMeshLink.agentTypeID = 0;
								navMeshLink.area = 0;
								navMeshLink.costModifier = -1;
								navMeshLink.width = connectionType.size.x;
								navMeshLink.startPoint = Vector3.forward * 1f;
								navMeshLink.endPoint = Vector3.back * 1f;
								navMeshLinkRoot.name = string.Format("NavMeshLink connection_{0} type : {1}", indexGateway, connectionType.id);
								spawnableArea.connectionLinkNavMesh.Add(indexGateway, navMeshLinkRoot);
							}
						}
					}
				}
			}
			else
			{
				Debug.LogError("No Gateway in area : " + spawnableArea.AreaDataId);
			}
			if (this.rootNoCulling == null)
			{
				Debug.LogError("Area " + spawnableArea.AreaDataId + " has no RootNoCulling. \nSomething went wrong with the area import");
				yield break;
			}
			if (this.cullingVolume)
			{
				this.cullingVolume.enabled = false;
			}
			if (this.lightingGroup != null)
			{
				if (this.lightingGroup.lightingPreset == null)
				{
					Debug.LogError("Area initialization : lightingGroup group has no lighting preset");
				}
				else
				{
					Debug.LogFormat(this.lightingGroup, "Waiting LightingGroup " + this.lightingGroup.name + " to initialize...", Array.Empty<object>());
					while (!this.lightingGroup.initialized)
					{
						yield return null;
					}
					Debug.LogFormat(this.lightingGroup, "LightingGroup " + this.lightingGroup.name + " initialized!", Array.Empty<object>());
				}
			}
			float time = Time.realtimeSinceStartup;
			Tonemapping.TonemappingBrieflyDisable();
			List<int> renderIds = this.RenderReflectionProbes();
			int num;
			for (int j = 0; j < this.reflectionProbes.Length; j = num + 1)
			{
				int renderId = renderIds[j];
				if (renderId != -1)
				{
					ReflectionProbe reflectionProbe = this.reflectionProbes[j];
					while (!reflectionProbe.IsFinishedRendering(renderId) && !reflectionProbe.realtimeTexture)
					{
						yield return Yielders.EndOfFrame;
					}
					reflectionProbe.customBakedTexture = reflectionProbe.realtimeTexture;
					reflectionProbe.realtimeTexture = null;
					reflectionProbe.mode = ReflectionProbeMode.Custom;
					reflectionProbe = null;
				}
				num = j;
			}
			LazyListPool<int>.Instance.Return(renderIds);
			Tonemapping.TonemappingBrieflyReEnable();
			Debug.Log(string.Format("[Area] Rendered {0} reflection probes in {1} seconds", this.reflectionProbes.Length, Time.realtimeSinceStartup - time));
			Debug.LogFormat(this.lightingGroup, "Area " + this.lightingGroup.name + " start static batching...", Array.Empty<object>());
			yield return this.StaticBatchCoroutine();
			Debug.LogFormat(this.lightingGroup, string.Format("Static batching ended for {0} in {1} seconds", this.lightingGroup.name, Time.realtimeSinceStartup - time), Array.Empty<object>());
			if (bakeMeshRunning)
			{
				bakeMeshHandle.Complete();
			}
			int colliderCount = this.colliderList.Count;
			for (int k = 0; k < colliderCount; k++)
			{
				if (this.colliderList[k] == null)
				{
					Debug.LogError("A collider is null in area " + spawnableArea.AreaDataId);
				}
				else
				{
					this.colliderList[k].enabled = true;
				}
			}
			if (this.cullingVolume)
			{
				this.cullingVolume.enabled = true;
			}
			if (this.itemLimiterSpawner != null)
			{
				this.itemLimiterSpawner.randomGen = rng;
				this.itemLimiterSpawner.SpawnAll();
			}
			else
			{
				Debug.LogError("No itemLimiterSpawner in area " + spawnableArea.AreaDataId);
			}
			yield return this.InitCreature(inLoadingMenu, rng);
			if (this.itemLimiterSpawner != null)
			{
				while (this.itemLimiterSpawner.IsItemSpawning())
				{
					yield return null;
				}
			}
			foreach (KeyValuePair<int, AreaGateway> pair in this.connectedGateways)
			{
				if (pair.Value.isFakeviewData)
				{
					while (!pair.Value.isFakeviewDataInUse)
					{
						yield return null;
					}
				}
				pair = default(KeyValuePair<int, AreaGateway>);
			}
			Dictionary<int, AreaGateway>.Enumerator enumerator = default(Dictionary<int, AreaGateway>.Enumerator);
			yield return Yielders.EndOfFrame;
			Debug.LogFormat(this, "Started InitCullCoroutine for " + base.name, Array.Empty<object>());
			yield return this.InitCullCoroutine(spawnableArea.IsCulled);
			this.ForceHide(this.isHidden);
			this.initialized = true;
			this.SetLiteMemoryState(spawnableArea.IsLiteMemoryState);
			LoreArea newLoreArea = base.gameObject.AddComponent<LoreArea>();
			newLoreArea.SetRoomId(spawnableArea.AreaDataId);
			AreaManager.Instance.LoreManager.loreAreas.Add(newLoreArea);
			UnityEvent unityEvent = this.onInitialized;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			yield break;
			yield break;
		}

		// Token: 0x06000E52 RID: 3666 RVA: 0x00065820 File Offset: 0x00063A20
		private List<int> RenderReflectionProbes()
		{
			List<int> renderIds = LazyListPool<int>.Instance.Get(this.reflectionProbes.Length);
			for (int i = 0; i < this.reflectionProbes.Length; i++)
			{
				ReflectionProbe reflectionProbe = this.reflectionProbes[i];
				this.InitReflectionProbeRotation(reflectionProbe);
				reflectionProbe.mode = ReflectionProbeMode.Realtime;
				reflectionProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
				reflectionProbe.timeSlicingMode = ReflectionProbeTimeSlicingMode.AllFacesAtOnce;
				if (Common.GetQualityLevel(false) == QualityLevel.Android)
				{
					reflectionProbe.hdr = false;
				}
				int reflectionLayers = this.GetReflectionLayers();
				reflectionProbe.cullingMask = reflectionLayers;
				if (reflectionProbe.enabled)
				{
					reflectionProbe.RenderProbe();
					int renderId = reflectionProbe.RenderProbe();
					renderIds.Add(renderId);
				}
				else
				{
					Debug.LogError("Reflection probe " + reflectionProbe.gameObject.GetPathFromRoot() + " is disabled. Cannot render it.");
					renderIds.Add(-1);
				}
			}
			return renderIds;
		}

		// Token: 0x06000E53 RID: 3667 RVA: 0x000658E4 File Offset: 0x00063AE4
		private IEnumerator InitCreature(bool inLoadingMenu, System.Random rng)
		{
			int ignoreMaxCount = this.creatureNoLimiteSpawners.Count;
			int num;
			for (int indexCreatureSpawnerNoLimit = 0; indexCreatureSpawnerNoLimit < ignoreMaxCount; indexCreatureSpawnerNoLimit = num + 1)
			{
				this.creatureNoLimiteSpawners[indexCreatureSpawnerNoLimit].Spawn(null, rng);
				if (this.creatureNoLimiteSpawners[indexCreatureSpawnerNoLimit].CurrentState != CreatureSpawner.State.Init)
				{
					while (this.creatureNoLimiteSpawners[indexCreatureSpawnerNoLimit].CurrentState != CreatureSpawner.State.Spawned)
					{
						yield return null;
					}
					Creature spawnedCreature = this.creatureNoLimiteSpawners[indexCreatureSpawnerNoLimit].GetSpawnedCreature(0);
					if (!(spawnedCreature == null))
					{
						while (!spawnedCreature.loaded)
						{
							yield return null;
						}
						List<Holder> creatureholders = spawnedCreature.holders;
						if (creatureholders != null && creatureholders.Count > 0)
						{
							for (int i = 0; i < creatureholders.Count; i = num + 1)
							{
								Holder holder = creatureholders[i];
								while (holder.spawningItem)
								{
									yield return null;
								}
								holder = null;
								num = i;
							}
						}
						this.RegisterCreature(spawnedCreature);
						spawnedCreature.SetCull(this.spawnableArea.IsCulled);
						spawnedCreature = null;
						creatureholders = null;
					}
				}
				num = indexCreatureSpawnerNoLimit;
			}
			if (this.spawnableArea.NumberCreature > 0)
			{
				int indexCreatureSpawnerNoLimit = Math.Min(this.spawnableArea.NumberCreature, this.creatureSpawners.Count);
				if (this.spawnableArea.isCreatureSpawnedExist == null)
				{
					this.spawnableArea.isCreatureSpawnedExist = new bool[indexCreatureSpawnerNoLimit];
				}
				if (!this.spawnableArea.ResapawnDeadCreature && this.spawnableArea.isCreatureDead == null)
				{
					this.spawnableArea.isCreatureDead = new bool[indexCreatureSpawnerNoLimit];
				}
				this.Shuffle<CreatureSpawner>(this.creatureSpawners, rng);
				for (int i = 0; i < indexCreatureSpawnerNoLimit; i = num + 1)
				{
					if (!this.spawnableArea.isCreatureSpawnedExist[i] && (this.spawnableArea.ResapawnDeadCreature || !this.spawnableArea.isCreatureDead[i]))
					{
						this.creatureSpawners[i].Spawn(null, rng);
						if (this.creatureSpawners[i].CurrentState != CreatureSpawner.State.Init)
						{
							while (this.creatureSpawners[i].CurrentState != CreatureSpawner.State.Spawned)
							{
								yield return null;
							}
							Creature spawnedCreature = this.creatureSpawners[i].GetSpawnedCreature(0);
							this.spawnableArea.isCreatureSpawnedExist[i] = true;
							spawnedCreature.currentArea = this.spawnableArea;
							spawnedCreature.initialArea = this.spawnableArea;
							spawnedCreature.areaSpawnerIndex = i;
							this.RegisterCreature(spawnedCreature);
							while (!spawnedCreature.loaded)
							{
								yield return null;
							}
							List<Holder> creatureholders = spawnedCreature.holders;
							if (creatureholders != null && creatureholders.Count > 0)
							{
								for (int j = 0; j < creatureholders.Count; j = num + 1)
								{
									Holder holder = creatureholders[j];
									while (holder.spawningItem)
									{
										yield return null;
									}
									holder = null;
									num = j;
								}
							}
							spawnedCreature.SetCull(this.spawnableArea.IsCulled);
							spawnedCreature = null;
							creatureholders = null;
						}
					}
					num = i;
				}
			}
			yield break;
		}

		// Token: 0x06000E54 RID: 3668 RVA: 0x000658FC File Offset: 0x00063AFC
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

		// Token: 0x06000E55 RID: 3669 RVA: 0x00065944 File Offset: 0x00063B44
		public bool BakeMeshCollider(out JobHandle jobHandle)
		{
			jobHandle = default(JobHandle);
			if (this.meshColliderRefList == null || this.meshColliderRefList.Count == 0)
			{
				return false;
			}
			int meshColliderCount = this.meshColliderRefList.Count;
			NativeArray<int> meshIds = new NativeArray<int>(meshColliderCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			NativeArray<bool> meshConvex = new NativeArray<bool>(meshColliderCount, Allocator.Persistent, NativeArrayOptions.ClearMemory);
			for (int i = 0; i < meshColliderCount; i++)
			{
				if (this.meshColliderRefList[i] == null || this.meshColliderRefList[i].mesh == null)
				{
					jobHandle = default(JobHandle);
					return false;
				}
				meshIds[i] = this.meshColliderRefList[i].mesh.GetInstanceID();
				meshConvex[i] = this.meshColliderRefList[i].convex;
			}
			Area.BakeJob job = new Area.BakeJob(meshIds, meshConvex);
			jobHandle = job.Schedule(meshIds.Length, 10, default(JobHandle));
			meshIds.Dispose(jobHandle);
			meshConvex.Dispose(jobHandle);
			return true;
		}

		// Token: 0x06000E56 RID: 3670 RVA: 0x00065A54 File Offset: 0x00063C54
		private void OnDestroy()
		{
			if (this.reflectionProbes != null)
			{
				int reflectionProbeCount = this.reflectionProbes.Length;
				for (int i = 0; i < reflectionProbeCount; i++)
				{
					ReflectionProbe reflectionProbe = this.reflectionProbes[i];
					RenderTexture realtimeTexture = reflectionProbe.realtimeTexture;
					if (realtimeTexture != null)
					{
						realtimeTexture.Release();
						UnityEngine.Object.Destroy(realtimeTexture);
						reflectionProbe.realtimeTexture = null;
					}
					RenderTexture bakedRenderTexture = reflectionProbe.bakedTexture as RenderTexture;
					if (bakedRenderTexture != null)
					{
						bakedRenderTexture.Release();
						UnityEngine.Object.Destroy(bakedRenderTexture);
						reflectionProbe.bakedTexture = null;
					}
					RenderTexture customBakedRenderTexture = reflectionProbe.customBakedTexture as RenderTexture;
					if (customBakedRenderTexture != null)
					{
						customBakedRenderTexture.Release();
						UnityEngine.Object.Destroy(customBakedRenderTexture);
						reflectionProbe.customBakedTexture = null;
					}
				}
			}
			while (this.items.Count > 0)
			{
				Item item = this.items[0];
				this.items.RemoveAt(0);
				if (item != null)
				{
					item.Despawn();
				}
			}
			while (this.creatures.Count > 0)
			{
				Creature creature = this.creatures[0];
				this.creatures.RemoveAt(0);
				if (creature != null)
				{
					creature.Despawn();
				}
			}
			this.SetLiteMemoryState(true);
		}

		// Token: 0x06000E57 RID: 3671 RVA: 0x00065B70 File Offset: 0x00063D70
		public void DisableAllPlayerSpawner()
		{
			if (this.playerSpawners == null)
			{
				return;
			}
			int count = this.playerSpawners.Count;
			for (int i = 0; i < count; i++)
			{
				this.playerSpawners[i].gameObject.SetActive(false);
			}
		}

		// Token: 0x06000E58 RID: 3672 RVA: 0x00065BB8 File Offset: 0x00063DB8
		public PlayerSpawner GetPlayerSpawner(string id)
		{
			PlayerSpawner selectedSpawner = PlayerSpawner.Get(id, this.playerSpawners);
			if (selectedSpawner == null)
			{
				Debug.LogError("PlayerSpawner " + id + " not found in area " + this.spawnableArea.AreaDataId);
				return null;
			}
			if (selectedSpawner.gameObject.activeInHierarchy)
			{
				return selectedSpawner;
			}
			selectedSpawner.transform.parent = base.transform;
			selectedSpawner.gameObject.SetActive(true);
			return selectedSpawner;
		}

		// Token: 0x06000E59 RID: 3673 RVA: 0x00065C2C File Offset: 0x00063E2C
		public void InitReflectionProbeRotation(ReflectionProbe reflectionProbe)
		{
			Bounds bounds = new Bounds(reflectionProbe.center, reflectionProbe.size);
			Matrix4x4 mat = Matrix4x4.TRS(Vector3.zero, base.transform.rotation, base.transform.localScale);
			Vector3 newMax = mat.MultiplyPoint3x4(bounds.max);
			Vector3 newMin = mat.MultiplyPoint3x4(bounds.min);
			bounds = default(Bounds);
			bounds.Encapsulate(newMax);
			bounds.Encapsulate(newMin);
			reflectionProbe.center = bounds.center;
			reflectionProbe.size = bounds.size;
		}

		// Token: 0x06000E5A RID: 3674 RVA: 0x00065CBD File Offset: 0x00063EBD
		public IEnumerator StaticBatchCoroutine()
		{
			if (!Catalog.gameData.platformParameters.dungeonStaticBatching)
			{
				yield break;
			}
			int totalVertexCount = 0;
			int totalCombinedMeshes = 0;
			if (this.meshInfos.IsNullOrEmpty())
			{
				Debug.LogError("No meshInfos in area " + this.spawnableArea.AreaDataId + ", cannot static batch, the room needs reimported.");
				yield break;
			}
			this.meshInfos = MeshBatcher.SortMeshInfo(this.meshInfos, this.spawnableArea.AreaData.Bounds, 10f);
			Dictionary<int, List<MeshBatcher.StaticBatchGroup>> staticBatchGroups = MeshBatcher.CreateStaticBatchGroups(this.meshInfos);
			foreach (KeyValuePair<int, List<MeshBatcher.StaticBatchGroup>> staticBatchGroup in staticBatchGroups)
			{
				foreach (MeshBatcher.StaticBatchGroup group in staticBatchGroup.Value)
				{
					yield return null;
					group.StaticBatch();
					int num = totalCombinedMeshes;
					totalCombinedMeshes = num + 1;
					totalVertexCount += group.vertexCount;
					foreach (MeshBatcher.MeshInfo meshInfo in group.meshInfos)
					{
						this.meshes.Add(meshInfo.MeshFilter.sharedMesh);
					}
					group = null;
				}
				List<MeshBatcher.StaticBatchGroup>.Enumerator enumerator2 = default(List<MeshBatcher.StaticBatchGroup>.Enumerator);
			}
			Dictionary<int, List<MeshBatcher.StaticBatchGroup>>.Enumerator enumerator = default(Dictionary<int, List<MeshBatcher.StaticBatchGroup>>.Enumerator);
			Debug.Log(string.Format("[Area] Static batched [Obj:{0}][Vert:{1}][CombinedMeshes:{2}] for room {3}", new object[]
			{
				this.meshes.Count,
				totalVertexCount,
				totalCombinedMeshes,
				this.spawnableArea.AreaDataId
			}));
			yield break;
			yield break;
		}

		// Token: 0x06000E5B RID: 3675 RVA: 0x00065CCC File Offset: 0x00063ECC
		public void StaticBatch()
		{
			if (Catalog.gameData.platformParameters.dungeonStaticBatching)
			{
				List<GameObject> objectsToBatch = LazyListPool<GameObject>.Instance.Get(2048);
				int meshRendererCount = this.activeMeshRenderers.Length;
				for (int i = 0; i < meshRendererCount; i++)
				{
					MeshRenderer meshRenderer = this.activeMeshRenderers[i];
					if (meshRenderer.gameObject.CompareTag("RoomStaticBatch") || (meshRenderer.lightmapIndex >= 0 && !meshRenderer.gameObject.CompareTag("NoRoomStaticBatching") && !meshRenderer.gameObject.CompareTag("PointerActive")))
					{
						objectsToBatch.Add(meshRenderer.gameObject);
					}
					MeshFilter meshFilter;
					if (meshRenderer.TryGetComponent<MeshFilter>(out meshFilter))
					{
						this.meshes.Add(meshFilter.sharedMesh);
					}
				}
				StaticBatchingUtility.Combine(objectsToBatch.ToArray(), base.gameObject);
				Debug.Log(string.Format("[Area] Static batched {0} objects for room {1}", objectsToBatch.Count, this.spawnableArea.AreaDataId));
				LazyListPool<GameObject>.Instance.Return(objectsToBatch);
			}
		}

		// Token: 0x06000E5C RID: 3676 RVA: 0x00065DC4 File Offset: 0x00063FC4
		public void OnPlayerEnter(Area previousArea)
		{
			this.IsActive = true;
			this.lightingGroup.ApplySceneSettings(true);
			if (this.blendAudioSources != null)
			{
				int blendAudioSourceCount = this.blendAudioSources.Count;
				for (int i = 0; i < blendAudioSourceCount; i++)
				{
					this.blendAudioSources[i].ApplyVolume(1f);
				}
			}
			this.onPlayerEnter.Invoke();
			if (this.connectedGateways != null)
			{
				foreach (KeyValuePair<int, AreaGateway> pair in this.connectedGateways)
				{
					pair.Value.OnPlayerChangeArea(previousArea, this);
				}
			}
			Transform playerTransform = PlayerTest.local ? PlayerTest.local.transform : Player.local.head.transform;
			if (!this.CheckActiveGateways(playerTransform.position))
			{
				this.spawnableArea.ApplyGlobalParameters(true, false, null);
			}
		}

		// Token: 0x06000E5D RID: 3677 RVA: 0x00065EC0 File Offset: 0x000640C0
		public void OnPlayerExit(Area newArea)
		{
			this.IsActive = false;
			this.onPlayerExit.Invoke();
			foreach (KeyValuePair<int, AreaGateway> pair in this.connectedGateways)
			{
				pair.Value.OnPlayerChangeArea(this, newArea);
			}
		}

		// Token: 0x06000E5E RID: 3678 RVA: 0x00065F2C File Offset: 0x0006412C
		public bool CheckActiveGateways(Vector3 playerPosition)
		{
			AreaGateway activeGateway = null;
			foreach (KeyValuePair<int, AreaGateway> pair in this.connectedGateways)
			{
				if (pair.Value.CheckActive(playerPosition))
				{
					if (activeGateway != null)
					{
						Debug.LogError("Multiple active gateway in area : " + base.gameObject.name + "\n This can cause issues with global parameter sun skybox etc");
					}
					activeGateway = pair.Value;
				}
				else
				{
					pair.Value.OnPlayerExit();
				}
			}
			if (activeGateway != null)
			{
				activeGateway.OnPlayerEnter();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Blend audio with parameter t
		/// </summary>
		/// <param name="t"> when t = 0 volume is 0, when t is 1 volume will be the original one</param>
		// Token: 0x06000E5F RID: 3679 RVA: 0x00065FDC File Offset: 0x000641DC
		public void BlendAudio(float t)
		{
			foreach (Area.BlendAudioSource blendAudioSource in this.blendAudioSources)
			{
				blendAudioSource.ApplyVolume(t);
			}
		}

		/// <summary>
		/// Blend between Current light preset and the area preset
		/// This does not change the current preset
		/// </summary>
		/// <param name="t">When t is 0 current level preset is fully apply, when t is 1 this area preset is fully apply</param>
		// Token: 0x06000E60 RID: 3680 RVA: 0x00066030 File Offset: 0x00064230
		public void BlendLight(float t)
		{
			this.lightingGroup.BlendSceneSettingsWithCurrent(t, false, false);
		}

		// Token: 0x06000E61 RID: 3681 RVA: 0x00066040 File Offset: 0x00064240
		public void RegisterItem(Item item)
		{
			if (!this.items.Contains(item))
			{
				this.items.Add(item);
			}
			if (!this.initialized)
			{
				item.Hide(true);
				item.SetCull(false, true);
				return;
			}
			item.Hide(this.isHidden);
			item.SetCull(this.isCulled, true);
		}

		// Token: 0x06000E62 RID: 3682 RVA: 0x00066098 File Offset: 0x00064298
		public void UnRegisterItem(Item item)
		{
			if (this.items.Contains(item))
			{
				this.items.Remove(item);
			}
			item.Hide(false);
			item.SetCull(false, true);
		}

		// Token: 0x06000E63 RID: 3683 RVA: 0x000660C4 File Offset: 0x000642C4
		public void RegisterCreature(Creature creature)
		{
			if (!this.creatures.Contains(creature))
			{
				this.creatures.Add(creature);
			}
		}

		// Token: 0x06000E64 RID: 3684 RVA: 0x000660E0 File Offset: 0x000642E0
		public void UnRegisterCreature(Creature creature)
		{
			if (this.creatures.Contains(creature))
			{
				this.creatures.Remove(creature);
			}
		}

		// Token: 0x06000E65 RID: 3685 RVA: 0x00066100 File Offset: 0x00064300
		public void SetLiteMemoryState(bool isLiteMemoryState)
		{
			if (!this.initialized)
			{
				return;
			}
			int count = this.liteMemoryToggles.Count;
			for (int index = 0; index < count; index++)
			{
				ILiteMemoryToggle memoryToggle;
				if (this.liteMemoryToggles.TryGetAtIndex(index, out memoryToggle))
				{
					memoryToggle.SetLiteMemory(isLiteMemoryState);
				}
			}
		}

		// Token: 0x06000E66 RID: 3686 RVA: 0x00066148 File Offset: 0x00064348
		public void SetCull(bool cull)
		{
			if (!this.initialized)
			{
				return;
			}
			if (cull && !this.isCulled)
			{
				this.isCulled = true;
				this.HideUnseenGateObject();
				foreach (object obj in base.transform)
				{
					Transform transform = (Transform)obj;
					if (!(transform == this.rootNoCulling.transform))
					{
						transform.gameObject.SetActive(false);
					}
				}
				if (this.toggleRoomObjectCoroutine != null)
				{
					base.StopCoroutine(this.toggleRoomObjectCoroutine);
				}
				if (this.spawnRoomObjectsAcrossFrames > 0f)
				{
					this.toggleRoomObjectCoroutine = base.StartCoroutine(this.ToggleRoomObjectCoroutine(false));
				}
				else
				{
					int itemCount = this.items.Count;
					for (int i = 0; i < itemCount; i++)
					{
						this.items[i].SetCull(true, true);
					}
					int creatureCount = this.creatures.Count;
					for (int j = 0; j < creatureCount; j++)
					{
						this.creatures[j].SetCull(true);
					}
				}
				if (this.onCullChange != null)
				{
					this.onCullChange(true);
					return;
				}
			}
			else if (!cull && this.isCulled)
			{
				this.isCulled = false;
				this.HideUnseenGateObject();
				foreach (object obj2 in base.transform)
				{
					Transform transform2 = (Transform)obj2;
					if (!(transform2 == this.rootNoCulling.transform))
					{
						transform2.gameObject.SetActive(true);
					}
				}
				int blockerCount = this.blockers.Count;
				for (int k = 0; k < blockerCount; k++)
				{
					this.blockers[k].SetActive(!this.isHidden);
				}
				if (this.toggleRoomObjectCoroutine != null)
				{
					base.StopCoroutine(this.toggleRoomObjectCoroutine);
				}
				if (this.spawnRoomObjectsAcrossFrames > 0f)
				{
					this.toggleRoomObjectCoroutine = base.StartCoroutine(this.ToggleRoomObjectCoroutine(true));
				}
				else
				{
					int itemCount2 = this.items.Count;
					for (int l = 0; l < itemCount2; l++)
					{
						this.items[l].SetCull(false, true);
					}
					int creatureCount2 = this.creatures.Count;
					for (int m = 0; m < creatureCount2; m++)
					{
						this.creatures[m].SetCull(false);
					}
				}
				if (this.onCullChange != null)
				{
					this.onCullChange(false);
				}
			}
		}

		// Token: 0x06000E67 RID: 3687 RVA: 0x000663F8 File Offset: 0x000645F8
		public IEnumerator InitCullCoroutine(bool cull)
		{
			if (cull && !this.isCulled)
			{
				this.isCulled = true;
				this.HideUnseenGateObject();
				foreach (object obj in base.transform)
				{
					Transform transform = (Transform)obj;
					if (!(transform == this.rootNoCulling.transform))
					{
						transform.gameObject.SetActive(false);
					}
				}
				if (this.spawnRoomObjectsAcrossFrames > 0f)
				{
					yield return this.ToggleRoomObjectCoroutine(false);
				}
				else
				{
					int itemCount = this.items.Count;
					for (int i = 0; i < itemCount; i++)
					{
						this.items[i].SetCull(true, true);
					}
					int creatureCount = this.creatures.Count;
					for (int j = 0; j < creatureCount; j++)
					{
						this.creatures[j].SetCull(true);
					}
				}
				if (this.onCullChange != null)
				{
					this.onCullChange(true);
				}
			}
			else if (!cull && this.isCulled)
			{
				this.isCulled = false;
				this.HideUnseenGateObject();
				foreach (object obj2 in base.transform)
				{
					Transform transform2 = (Transform)obj2;
					if (!(transform2 == this.rootNoCulling.transform))
					{
						transform2.gameObject.SetActive(true);
					}
				}
				if (this.spawnRoomObjectsAcrossFrames > 0f)
				{
					yield return this.ToggleRoomObjectCoroutine(true);
				}
				else
				{
					int itemCount2 = this.items.Count;
					for (int k = 0; k < itemCount2; k++)
					{
						this.items[k].SetCull(false, true);
					}
					int creatureCount2 = this.creatures.Count;
					for (int l = 0; l < creatureCount2; l++)
					{
						this.creatures[l].SetCull(false);
					}
				}
				if (this.onCullChange != null)
				{
					this.onCullChange(false);
				}
			}
			yield break;
		}

		// Token: 0x06000E68 RID: 3688 RVA: 0x0006640E File Offset: 0x0006460E
		public void Hide(bool hide)
		{
			if (!this.initialized)
			{
				this.isHidden = hide;
				return;
			}
			if (hide != this.isHidden)
			{
				this.ForceHide(hide);
			}
		}

		// Token: 0x06000E69 RID: 3689 RVA: 0x00066430 File Offset: 0x00064630
		private void ForceHide(bool hide)
		{
			if (this.cullingVolume)
			{
				int bakeGroupCount = this.cullingVolume.bakeGroups.Length;
				for (int indexBakeGroup = 0; indexBakeGroup < bakeGroupCount; indexBakeGroup++)
				{
					Renderer[] renderers = this.cullingVolume.bakeGroups[indexBakeGroup].renderers;
					int rendererCount = renderers.Length;
					for (int indexRenderer = 0; indexRenderer < rendererCount; indexRenderer++)
					{
						Renderer renderer = renderers[indexRenderer];
						if (!(renderer == null))
						{
							renderer.enabled = !hide;
						}
					}
				}
				this.cullingVolume.enabled = !hide;
			}
			else if (!this.meshRenderers.IsNullOrEmpty())
			{
				for (int i = 0; i < this.meshRenderers.Length; i++)
				{
					this.meshRenderers[i].enabled = !hide;
				}
			}
			int itemCount = this.items.Count;
			for (int j = 0; j < itemCount; j++)
			{
				this.items[j].Hide(hide);
			}
			int creatureCount = this.creatures.Count;
			for (int k = 0; k < creatureCount; k++)
			{
				this.creatures[k].Hide(hide);
			}
			if (!this.isCulled)
			{
				int blockerCount = this.blockers.Count;
				for (int l = 0; l < blockerCount; l++)
				{
					this.blockers[l].SetActive(!hide);
				}
			}
			foreach (KeyValuePair<int, AreaGateway> pair in this.connectedGateways)
			{
				pair.Value.gameObject.SetActive(!hide);
			}
			if (this.visualEffects != null)
			{
				for (int m = 0; m < this.visualEffects.Length; m++)
				{
					this.visualEffects[m].enabled = !hide;
				}
			}
			if (this.particlesSystem != null)
			{
				for (int n = 0; n < this.particlesSystem.Length; n++)
				{
					this.particlesSystem[n].gameObject.SetActive(!hide);
				}
			}
			this.isHidden = hide;
			this.HideUnseenGateObject();
			if (this.onHideChange != null)
			{
				this.onHideChange(this.isHidden);
			}
		}

		// Token: 0x06000E6A RID: 3690 RVA: 0x00066670 File Offset: 0x00064870
		private IEnumerator ToggleRoomObjectCoroutine(bool enabled)
		{
			int num;
			if (enabled)
			{
				for (int i = 0; i < this.items.Count; i = num + 1)
				{
					this.items[i].SetCull(false, true);
					this.items[i].physicBody.ForceFreeze();
					int i2 = 0;
					while ((float)i2 < this.spawnRoomObjectsAcrossFrames)
					{
						yield return Yielders.EndOfFrame;
						num = i2;
						i2 = num + 1;
					}
					this.items[i].physicBody.UnFreeze();
					i2 = 0;
					while ((float)i2 < this.spawnRoomObjectsAcrossFrames)
					{
						yield return Yielders.EndOfFrame;
						num = i2;
						i2 = num + 1;
					}
					num = i;
				}
			}
			else
			{
				for (int j = 0; j < this.items.Count; j++)
				{
					this.items[j].SetCull(true, true);
				}
			}
			for (int i = 0; i < this.creatures.Count; i = num + 1)
			{
				this.creatures[i].SetCull(!enabled);
				int i2 = 0;
				while ((float)i2 < this.spawnRoomObjectsAcrossFrames)
				{
					yield return Yielders.EndOfFrame;
					num = i2;
					i2 = num + 1;
				}
				num = i;
			}
			this.toggleRoomObjectCoroutine = null;
			yield break;
		}

		// Token: 0x06000E6B RID: 3691 RVA: 0x00066688 File Offset: 0x00064888
		private void HideUnseenGateObject()
		{
			for (int i = 0; i < this.gateways.Length; i++)
			{
				GameObject gate;
				if (this.spawnableArea.connectionGateSpawn.TryGetValue(i, out gate))
				{
					SpawnableArea.ConnectedArea connectedAreaInfo = this.spawnableArea.GetConnectedArea(this.spawnableArea.AreaDataId, i);
					bool isAreaVisible = !this.isCulled && !this.isHidden;
					bool isConnectedAreaVisible = connectedAreaInfo.connectedArea.IsSpawned && connectedAreaInfo.connectedArea.SpawnedArea.initialized && !connectedAreaInfo.connectedArea.SpawnedArea.isCulled && !connectedAreaInfo.connectedArea.SpawnedArea.isHidden;
					gate.SetActive(isAreaVisible || isConnectedAreaVisible);
				}
			}
		}

		/// <summary>
		/// The layers which will be rendered by the reflection probes
		/// </summary>
		/// <returns></returns>
		// Token: 0x06000E6C RID: 3692 RVA: 0x00066748 File Offset: 0x00064948
		private int GetReflectionLayers()
		{
			return 0 | 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Water") | 1 << LayerMask.NameToLayer("LocomotionOnly") | 1 << LayerMask.NameToLayer("NoLocomotion") | 1 << LayerMask.NameToLayer("SkyDome");
		}

		// Token: 0x06000E6D RID: 3693 RVA: 0x000667A8 File Offset: 0x000649A8
		protected bool LODHaveRenderers(LOD lod)
		{
			int rendererCount = lod.renderers.Length;
			for (int i = 0; i < rendererCount; i++)
			{
				if (lod.renderers[i] != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000E6E RID: 3694 RVA: 0x000667E0 File Offset: 0x000649E0
		protected bool IsAdditionalNonLOD0Collider(LODGroup lODGroup, Collider collider)
		{
			LOD[] lods = lODGroup.GetLODs();
			if (lods.Length == 0)
			{
				return false;
			}
			if (!this.LODHaveRenderers(lods[0]))
			{
				return false;
			}
			for (int indexLods = 1; indexLods < lods.Length; indexLods++)
			{
				Renderer[] renderers = lods[indexLods].renderers;
				int rendererCount = renderers.Length;
				for (int indexRenderer = 0; indexRenderer < rendererCount; indexRenderer++)
				{
					Renderer renderer = renderers[indexRenderer];
					if (renderer)
					{
						Collider[] lodColliders = renderer.gameObject.GetComponentsInChildren<Collider>();
						int lodColliderCount = lodColliders.Length;
						for (int indexLodCollider = 0; indexLodCollider < lodColliderCount; indexLodCollider++)
						{
							if (lodColliders[indexLodCollider] == collider)
							{
								return true;
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x04000C8B RID: 3211
		[Header("Custom References")]
		public List<Area.CustomReference> customReferences;

		// Token: 0x04000C8C RID: 3212
		[Header("Culling")]
		[Tooltip("Spawns the area across a number of frames during load time (Recommended 2)")]
		public float spawnRoomObjectsAcrossFrames = 2f;

		// Token: 0x04000C8D RID: 3213
		[Header("Audio blend")]
		[Tooltip("Lists looping audio sources which will blend between eachother.")]
		public List<AudioSource> audioSourcesToBlend = new List<AudioSource>();

		// Token: 0x04000C8E RID: 3214
		[Tooltip("This is the root for everyting that should not be disabled when culled")]
		public GameObject rootNoCulling;

		// Token: 0x04000C8F RID: 3215
		[HideInInspector]
		public PerfectCullingVolume cullingVolume;

		// Token: 0x04000C90 RID: 3216
		[HideInInspector]
		public LightingGroup lightingGroup;

		// Token: 0x04000C91 RID: 3217
		[HideInInspector]
		public ListMonoBehaviourReference<ILiteMemoryToggle> liteMemoryToggles;

		// Token: 0x04000C92 RID: 3218
		[NonSerialized]
		public bool lightingPresetFromData;

		// Token: 0x04000C93 RID: 3219
		[HideInInspector]
		public VisualEffect[] visualEffects;

		// Token: 0x04000C94 RID: 3220
		[HideInInspector]
		public ParticleSystem[] particlesSystem;

		// Token: 0x04000C95 RID: 3221
		[HideInInspector]
		public AreaGateway[] gateways;

		// Token: 0x04000C96 RID: 3222
		[HideInInspector]
		public List<PlayerSpawner> playerSpawners = new List<PlayerSpawner>();

		// Token: 0x04000C97 RID: 3223
		[HideInInspector]
		public ItemSpawnerLimiter itemLimiterSpawner;

		// Token: 0x04000C98 RID: 3224
		[HideInInspector]
		public List<CreatureSpawner> creatureSpawners = new List<CreatureSpawner>();

		// Token: 0x04000C99 RID: 3225
		[HideInInspector]
		public List<CreatureSpawner> creatureNoLimiteSpawners = new List<CreatureSpawner>();

		// Token: 0x04000C9A RID: 3226
		[HideInInspector]
		public ReflectionProbe[] reflectionProbes;

		// Token: 0x04000C9B RID: 3227
		[HideInInspector]
		public MeshRenderer[] meshRenderers;

		// Token: 0x04000C9C RID: 3228
		[HideInInspector]
		public MeshRenderer[] activeMeshRenderers;

		// Token: 0x04000C9D RID: 3229
		public List<MeshBatcher.MeshInfo> meshInfos;

		// Token: 0x04000C9E RID: 3230
		[HideInInspector]
		public List<Area.MeshColliderRef> meshColliderRefList = new List<Area.MeshColliderRef>();

		// Token: 0x04000C9F RID: 3231
		[HideInInspector]
		public List<Collider> colliderList = new List<Collider>();

		// Token: 0x04000CA0 RID: 3232
		[NonSerialized]
		public Dictionary<int, AreaGateway> connectedGateways = new Dictionary<int, AreaGateway>();

		// Token: 0x04000CA1 RID: 3233
		[NonSerialized]
		public List<GameObject> blockers = new List<GameObject>();

		// Token: 0x04000CA2 RID: 3234
		[NonSerialized]
		public bool initialized;

		// Token: 0x04000CA3 RID: 3235
		[Header("Culling")]
		[NonSerialized]
		public List<Area.BlendAudioSource> blendAudioSources;

		// Token: 0x04000CA4 RID: 3236
		[NonSerialized]
		public List<Item> items = new List<Item>();

		// Token: 0x04000CA5 RID: 3237
		[NonSerialized]
		public List<Creature> creatures = new List<Creature>();

		// Token: 0x04000CA6 RID: 3238
		[NonSerialized]
		public bool isCulled;

		// Token: 0x04000CA7 RID: 3239
		[NonSerialized]
		public bool isHidden;

		// Token: 0x04000CA8 RID: 3240
		[NonSerialized]
		public SpawnableArea spawnableArea;

		// Token: 0x04000CA9 RID: 3241
		[NonSerialized]
		public bool IsActive;

		// Token: 0x04000CAA RID: 3242
		protected Coroutine toggleRoomObjectCoroutine;

		// Token: 0x04000CAB RID: 3243
		[NonSerialized]
		public List<Mesh> meshes = LazyListPool<Mesh>.Instance.Get(2048);

		// Token: 0x04000CAC RID: 3244
		[Header("Event")]
		public UnityEvent onPlayerEnter = new UnityEvent();

		// Token: 0x04000CAD RID: 3245
		public UnityEvent onPlayerExit = new UnityEvent();

		// Token: 0x04000CAE RID: 3246
		public UnityEvent onInitialized = new UnityEvent();

		// Token: 0x020006B3 RID: 1715
		[Serializable]
		public class CustomReference
		{
			// Token: 0x04003A1A RID: 14874
			public string name;

			// Token: 0x04003A1B RID: 14875
			public List<Transform> transforms;
		}

		// Token: 0x020006B4 RID: 1716
		public class BlendAudioSource
		{
			// Token: 0x06003A34 RID: 14900 RVA: 0x00172B20 File Offset: 0x00170D20
			public BlendAudioSource(AudioSource audioSource)
			{
				FxModuleAudio audioFx = audioSource.GetComponentInParent<FxModuleAudio>();
				if (audioFx != null && audioFx.audioSource == audioSource)
				{
					this.audioFx = audioFx;
					this.orgVolume = EffectAudio.DecibelToLinear(audioFx.volumeDb);
					this.audioSource = null;
					return;
				}
				this.audioFx = null;
				this.audioSource = audioSource;
				this.orgVolume = audioSource.volume;
			}

			/// <summary>
			/// Apply volume as lerp between 0 and original volume
			/// </summary>
			/// <param name="t"> t the lerp parameter (t = 1.0f will apply original volume)</param>
			// Token: 0x06003A35 RID: 14901 RVA: 0x00172B8C File Offset: 0x00170D8C
			public void ApplyVolume(float t)
			{
				float newVolume = Mathf.Lerp(0f, this.orgVolume, t);
				if (this.audioFx != null)
				{
					this.audioFx.volumeDb = EffectAudio.LinearToDecibel(newVolume);
					this.audioFx.Refresh();
					return;
				}
				this.audioSource.volume = newVolume;
			}

			// Token: 0x04003A1C RID: 14876
			private AudioSource audioSource;

			// Token: 0x04003A1D RID: 14877
			private FxModuleAudio audioFx;

			// Token: 0x04003A1E RID: 14878
			private float orgVolume;
		}

		// Token: 0x020006B5 RID: 1717
		[Serializable]
		public class MeshColliderRef
		{
			// Token: 0x04003A1F RID: 14879
			public bool convex;

			// Token: 0x04003A20 RID: 14880
			public Mesh mesh;
		}

		// Token: 0x020006B6 RID: 1718
		public struct BakeJob : IJobParallelFor
		{
			// Token: 0x06003A37 RID: 14903 RVA: 0x00172BEA File Offset: 0x00170DEA
			public BakeJob(NativeArray<int> meshIds, NativeArray<bool> meshConvex)
			{
				this.meshIds = meshIds;
				this.meshConvex = meshConvex;
			}

			// Token: 0x06003A38 RID: 14904 RVA: 0x00172BFA File Offset: 0x00170DFA
			public void Execute(int index)
			{
				Physics.BakeMesh(this.meshIds[index], this.meshConvex[index]);
			}

			// Token: 0x04003A21 RID: 14881
			public const int MESH_PER_JOB = 10;

			// Token: 0x04003A22 RID: 14882
			private NativeArray<int> meshIds;

			// Token: 0x04003A23 RID: 14883
			private NativeArray<bool> meshConvex;
		}

		// Token: 0x020006B7 RID: 1719
		// (Invoke) Token: 0x06003A3A RID: 14906
		public delegate void CullChangeEvent(bool isCulled);

		// Token: 0x020006B8 RID: 1720
		// (Invoke) Token: 0x06003A3E RID: 14910
		public delegate void HideChangeEvent(bool isHide);
	}
}
