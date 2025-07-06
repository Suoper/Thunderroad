using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002F9 RID: 761
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/WaveSpawner.html")]
	[AddComponentMenu("ThunderRoad/Levels/Spawners/Waves Spawner")]
	public class WaveSpawner : MonoBehaviour
	{
		// Token: 0x06002448 RID: 9288 RVA: 0x000F7B4C File Offset: 0x000F5D4C
		public void StartWave(string waveId)
		{
			WaveData waveData = null;
			string text;
			if (Level.TryGetCurrentLevelOption(LevelOption.EnemyConfig, out text))
			{
				Debug.Log("[Wave Spawner]: " + base.name + " | StartWave() - level has enemy tier => Replace wave Id");
				waveData = this.GetWaveData();
				if (waveData != null)
				{
					Debug.Log(string.Concat(new string[]
					{
						"[Wave Spawner]: ",
						base.name,
						" | StartWave() - old id: ",
						waveId,
						" -> new id: ",
						waveData.id
					}));
					waveId = waveData.id;
				}
			}
			if (waveData == null)
			{
				waveData = Catalog.GetData<WaveData>(waveId, true);
			}
			this.StartWave(waveData, 0f, true);
		}

		// Token: 0x06002449 RID: 9289 RVA: 0x000F7BE7 File Offset: 0x000F5DE7
		public void CancelWave()
		{
			this.StopWave(false);
		}

		// Token: 0x0600244A RID: 9290 RVA: 0x000F7BF0 File Offset: 0x000F5DF0
		public void StopWave(bool success)
		{
			if (this.isRunning)
			{
				EventManager.onCreatureSpawn -= this.CreatureSpawn;
				EventManager.onCreatureAttacking -= this.CreatureAttack;
				EventManager.onCreatureHit -= this.CreatureHit;
				EventManager.onCreatureKill -= this.CreatureKill;
				EventManager.onCreatureDespawn -= this.CreatureDespawn;
				this.delayedStartTimeLeft = 0f;
				if (this.updateCoroutine != null)
				{
					base.StopCoroutine(this.updateCoroutine);
				}
				if (this.startWaveCoroutine != null)
				{
					base.StopCoroutine(this.startWaveCoroutine);
				}
				this.groupCoroutineRunning = false;
				this.isRunning = false;
				for (int i = this.spawnedCreatures.Count - 1; i >= 0; i--)
				{
					Creature creature = this.spawnedCreatures[i];
					creature.brain.instance.tree.blackboard.UpdateVariable<bool>("ForceFlee", true);
					this.RemoveFromWave(creature);
				}
				if (!Catalog.gameData.useDynamicMusic && this.audioMusic.clip)
				{
					Level.current.StartCoroutine(Utils.FadeOut(this.audioMusic, 2f));
				}
				if (!Catalog.gameData.useDynamicMusic && this.audioGroupWave)
				{
					if (success)
					{
						this.audioStep.clip = this.audioGroupWave.PickAudioClip(0);
						this.audioStep.PlayDelayed(1f);
					}
					else
					{
						this.audioStep.clip = this.audioGroupWave.PickAudioClip(1);
						this.audioStep.PlayDelayed(1f);
					}
				}
				if (this.OnWaveAnyEndEvent != null)
				{
					this.OnWaveAnyEndEvent.Invoke();
				}
				if (success)
				{
					if (this.OnWaveWinEvent != null)
					{
						this.OnWaveWinEvent.Invoke();
						return;
					}
				}
				else if (Player.currentCreature == null || Player.currentCreature.state == Creature.State.Dead)
				{
					if (this.OnWaveLossEvent != null)
					{
						this.OnWaveLossEvent.Invoke();
						return;
					}
				}
				else if (this.OnWaveCancelEvent != null)
				{
					this.OnWaveCancelEvent.Invoke();
				}
			}
		}

		// Token: 0x0600244B RID: 9291 RVA: 0x000F7DFC File Offset: 0x000F5FFC
		public void Clean()
		{
			for (int i = Item.allActive.Count - 1; i >= 0; i--)
			{
				if (Item.allActive[i].spawnTime != 0f && !Item.allActive[i].holder && !Item.allActive[i].isTelekinesisGrabbed && !Item.allActive[i].isThrowed && !Item.allActive[i].IsHanded() && Item.allActive[i].owner == Item.Owner.None && !Item.allActive[i].DisallowDespawn && !Item.allActive[i].isGripped)
				{
					Item.allActive[i].Despawn();
				}
			}
			for (int j = Creature.allActive.Count - 1; j >= 0; j--)
			{
				if (!Creature.allActive[j].isPlayer)
				{
					if (Creature.allActive[j].state != Creature.State.Dead)
					{
						Brain brain = Creature.allActive[j].brain;
						bool flag;
						if (brain == null)
						{
							flag = true;
						}
						else
						{
							BrainData instance = brain.instance;
							bool? flag2 = (instance != null) ? new bool?(instance.isActive) : null;
							bool flag3 = true;
							flag = !(flag2.GetValueOrDefault() == flag3 & flag2 != null);
						}
						if (!flag)
						{
							goto IL_160;
						}
					}
					Creature.allActive[j].Despawn();
				}
				IL_160:;
			}
		}

		// Token: 0x0600244C RID: 9292 RVA: 0x000F7F74 File Offset: 0x000F6174
		public List<ValueDropdownItem<int>> GetAllFactionID()
		{
			if (Catalog.gameData == null)
			{
				return null;
			}
			return Catalog.gameData.GetFactions();
		}

		// Token: 0x0600244D RID: 9293 RVA: 0x000F7F89 File Offset: 0x000F6189
		public List<ValueDropdownItem<string>> GetAllWaveID()
		{
			return Catalog.GetDropdownAllID(Category.Wave, "None");
		}

		// Token: 0x17000232 RID: 562
		// (get) Token: 0x0600244E RID: 9294 RVA: 0x000F7F96 File Offset: 0x000F6196
		// (set) Token: 0x0600244F RID: 9295 RVA: 0x000F7F9D File Offset: 0x000F619D
		public static Transform creatureStart { get; protected set; }

		// Token: 0x06002450 RID: 9296 RVA: 0x000F7FA8 File Offset: 0x000F61A8
		private void Awake()
		{
			if (!this.ignoreDungeonLimits)
			{
				this.area = base.GetComponentInParent<Area>();
			}
			if (this.addAsFleepointOnStart)
			{
				foreach (Transform point in this.spawns)
				{
					if (!point.gameObject.GetComponent<FleePoint>())
					{
						point.gameObject.AddComponent<FleePoint>();
					}
				}
			}
			this.creatureQueue = new List<WaveData.SpawnData>();
			this.spawnedCreatures = new List<Creature>();
		}

		// Token: 0x06002451 RID: 9297 RVA: 0x000F8044 File Offset: 0x000F6244
		private void OnDestroy()
		{
			if (this.audioGroupWave)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioGroupWave);
			}
			if (!Catalog.gameData.useDynamicMusic && this.audioMusic.clip)
			{
				Catalog.ReleaseAsset<AudioClip>(this.audioMusic.clip);
			}
		}

		// Token: 0x06002452 RID: 9298 RVA: 0x000F8097 File Offset: 0x000F6297
		private void Start()
		{
			if (this.beginWaveOnStart)
			{
				this.StartWave(this.startWaveId, this.beginWaveOnStartDelay, true);
			}
		}

		// Token: 0x06002453 RID: 9299 RVA: 0x000F80B4 File Offset: 0x000F62B4
		private void OnEnable()
		{
			WaveSpawner.instances.Add(this);
			UnityEvent<WaveSpawner> onWaveSpawnerEnabledEvent = WaveSpawner.OnWaveSpawnerEnabledEvent;
			if (onWaveSpawnerEnabledEvent != null)
			{
				onWaveSpawnerEnabledEvent.Invoke(this);
			}
			if (WaveSpawner.creatureStart == null)
			{
				WaveSpawner.creatureStart = new GameObject("CreatureStart").transform;
				WaveSpawner.creatureStart.position = new Vector3(1000f, 1000f, 1000f);
				BoxCollider boxCollider = new GameObject("StartFloor").AddComponent<BoxCollider>();
				boxCollider.transform.parent = WaveSpawner.creatureStart.transform;
				boxCollider.transform.localPosition = new Vector3(0f, -0.25f, 0f);
				boxCollider.transform.localScale = new Vector3(3f, 0.5f, 3f);
			}
		}

		// Token: 0x06002454 RID: 9300 RVA: 0x000F8180 File Offset: 0x000F6380
		public void OnDisable()
		{
			WaveSpawner.instances.Remove(this);
			UnityEvent<WaveSpawner> onWaveSpawnerDisabledEvent = WaveSpawner.OnWaveSpawnerDisabledEvent;
			if (onWaveSpawnerDisabledEvent != null)
			{
				onWaveSpawnerDisabledEvent.Invoke(this);
			}
			if (this.isRunning)
			{
				this.delayedStartTimeLeft = 0f;
				if (this.updateCoroutine != null)
				{
					base.StopCoroutine(this.updateCoroutine);
				}
				if (this.startWaveCoroutine != null)
				{
					base.StopCoroutine(this.startWaveCoroutine);
				}
				this.groupCoroutineRunning = false;
				this.isRunning = false;
				UnityEvent<WaveSpawner> onWaveSpawnerStopRunningEvent = WaveSpawner.OnWaveSpawnerStopRunningEvent;
				if (onWaveSpawnerStopRunningEvent != null)
				{
					onWaveSpawnerStopRunningEvent.Invoke(this);
				}
				foreach (Creature creature in this.spawnedCreatures)
				{
					creature.SetFaction(this.ignoredFactionId);
				}
				if (!Catalog.gameData.useDynamicMusic)
				{
					this.audioMusic.Stop();
				}
			}
			if (WaveSpawner.instances.Count == 0 && WaveSpawner.creatureStart != null && WaveSpawner.creatureStart.gameObject != null)
			{
				UnityEngine.Object.Destroy(WaveSpawner.creatureStart.gameObject);
			}
		}

		// Token: 0x06002455 RID: 9301 RVA: 0x000F82A4 File Offset: 0x000F64A4
		public static bool TryGetRunningInstance(out WaveSpawner waveSpawner)
		{
			int instancesCount = WaveSpawner.instances.Count;
			for (int i = 0; i < instancesCount; i++)
			{
				WaveSpawner ws = WaveSpawner.instances[i];
				if (ws.isRunning)
				{
					waveSpawner = ws;
					return true;
				}
			}
			waveSpawner = null;
			return false;
		}

		/// <summary>
		/// Returns the wave data linked to this ID in the catalog.
		/// </summary>
		/// <returns>The wave data in the catalog, null if not found</returns>
		// Token: 0x06002456 RID: 9302 RVA: 0x000F82E8 File Offset: 0x000F64E8
		private WaveData GetWaveData()
		{
			this.currentLoadedData = null;
			if (this.referenceType == WaveSpawner.ReferenceType.EnemyConfig)
			{
				string enemyConfigId;
				if (!Level.TryGetCurrentLevelOption(LevelOption.EnemyConfig, out enemyConfigId))
				{
					Debug.LogError("[Wave spawner] EnemyConfig level option not found.");
					return null;
				}
				EnemyConfig loadedEnemyConfig;
				if (!Catalog.TryGetData<EnemyConfig>(enemyConfigId, out loadedEnemyConfig, true))
				{
					Debug.LogError("[Wave spawner] GetWaveData() could not fetch the Enemy Config data from the level options.");
					return null;
				}
				this.currentLoadedData = loadedEnemyConfig.GetWave(this.enemyConfigType);
				if (this.currentLoadedData == null)
				{
					Debug.LogWarning(string.Format("[Wave Spawner] GetWaveData() could not fetch the Wave for the EnemyConfigType: {0} from the Enemy Config: {1}", this.enemyConfigType, loadedEnemyConfig.id));
				}
				else
				{
					Debug.Log(string.Format("[Wave Spawner] GetWaveData() Loaded {0} - Wave Id: {1}  to replace the EnemyConfigType: {2}", loadedEnemyConfig.id, this.currentLoadedData.id, this.enemyConfigType));
				}
			}
			return this.currentLoadedData;
		}

		// Token: 0x06002457 RID: 9303 RVA: 0x000F83A4 File Offset: 0x000F65A4
		public void StartWave(string waveId, float delay = 0f, bool resetWaveCount = true)
		{
			WaveData waveData = null;
			string text;
			if (Level.TryGetCurrentLevelOption(LevelOption.EnemyConfig, out text))
			{
				Debug.Log("[Wave Spawner]: " + base.name + " | StartWave() - level has enemy tier => Replace wave Id");
				waveData = this.GetWaveData();
				if (waveData != null)
				{
					Debug.Log(string.Concat(new string[]
					{
						"[Creature Spawner]: ",
						base.name,
						" | StartWave() - old id: ",
						waveId,
						" -> new id: ",
						waveData.id
					}));
					waveId = waveData.id;
				}
			}
			if (waveData == null)
			{
				waveData = Catalog.GetData<WaveData>(waveId, true);
			}
			this.StartWave(waveData, delay, resetWaveCount);
		}

		// Token: 0x06002458 RID: 9304 RVA: 0x000F843C File Offset: 0x000F663C
		public void StartWave(WaveData waveData, float delay = 0f, bool resetWaveCount = true)
		{
			if (waveData == null)
			{
				Debug.LogError("No WaveData was provided, wave not started.");
				return;
			}
			if (!this.isRunning)
			{
				this.waveData = waveData;
				this.spawnPoints = new List<WaveSpawner.SpawnPoint>();
				Area areaComponent = base.GetComponentInParent<Area>();
				foreach (Transform spawnPoint in this.spawns)
				{
					this.spawnPoints.Add(new WaveSpawner.SpawnPoint(spawnPoint, areaComponent));
				}
				this.availableSpawnPoints = new WaveSpawner.SpawnPoint[this.spawnPoints.Count];
				if (this.cleanBodiesAndItemsOnWaveStart)
				{
					this.Clean();
				}
				this.creatureQueue.Clear();
				this.spawnedCreatures.Clear();
				this.waveFactionInfos.Clear();
				foreach (WaveData.WaveFaction waveFaction in waveData.factions)
				{
					this.waveFactionInfos.Add(waveFaction.factionID, new WaveSpawner.FactionInfo
					{
						data = waveFaction,
						aliveCount = 0
					});
				}
				this.isRunning = true;
				UnityEvent<WaveSpawner> onWaveSpawnerStartRunningEvent = WaveSpawner.OnWaveSpawnerStartRunningEvent;
				if (onWaveSpawnerStartRunningEvent != null)
				{
					onWaveSpawnerStartRunningEvent.Invoke(this);
				}
				if (!Catalog.gameData.useDynamicMusic && this.audioMusic != null && this.audioMusic.clip)
				{
					this.audioMusic.Play();
				}
				if (resetWaveCount)
				{
					this.waveCount = 0;
				}
				base.StartCoroutine(this.PopulateQueue());
				if (delay > 0f)
				{
					if (this.startWaveCoroutine != null)
					{
						base.StopCoroutine(this.startWaveCoroutine);
					}
					this.startWaveCoroutine = base.StartCoroutine(this.StartWaveCoroutine(delay));
				}
				else
				{
					this.updateCoroutine = base.StartCoroutine(this.UpdateSpawner());
				}
				if (this.OnWaveBeginEvent != null)
				{
					this.OnWaveBeginEvent.Invoke();
				}
			}
		}

		// Token: 0x06002459 RID: 9305 RVA: 0x000F8630 File Offset: 0x000F6830
		protected IEnumerator StartWaveCoroutine(float delay)
		{
			this.delayedStartTimeLeft = delay;
			while (this.delayedStartTimeLeft > 0f)
			{
				yield return Yielders.ForSeconds(1f);
				this.delayedStartTimeLeft -= 1f;
			}
			this.delayedStartTimeLeft = 0f;
			this.updateCoroutine = base.StartCoroutine(this.UpdateSpawner());
			yield break;
		}

		// Token: 0x0600245A RID: 9306 RVA: 0x000F8646 File Offset: 0x000F6846
		protected IEnumerator PopulateQueue()
		{
			this.populating = true;
			HashSet<WaveData.Group> stillActive = new HashSet<WaveData.Group>();
			for (int i = 0; i < this.creatureQueue.Count + this.spawnedCreatures.Count; i++)
			{
				WaveData.Group group = (i < this.creatureQueue.Count) ? this.creatureQueue[i].spawnGroup : this.spawnedCreatures[i - this.creatureQueue.Count].spawnGroup;
				stillActive.Add(group);
			}
			int orgWaveCount = this.waveCount;
			int num;
			for (int j = 0; j < this.waveData.groups.Count; j = num + 1)
			{
				WaveData.Group group2 = this.waveData.groups[j];
				group2.FindPrerequisiteGroup();
				if (!stillActive.Contains(group2))
				{
					if (this.groupAliveCounts.ContainsKey(group2))
					{
						this.groupAliveCounts.Remove(group2);
					}
					yield return group2.GetCreatures(this.creatureQueue);
					if (this.waveCount == orgWaveCount)
					{
						this.waveCount++;
					}
				}
				num = j;
			}
			for (int k = this.creatureQueue.Count - 1; k >= 0; k--)
			{
				if (this.creatureQueue[k].data == null)
				{
					this.creatureQueue.RemoveAt(k);
				}
			}
			this.populating = false;
			yield break;
		}

		// Token: 0x0600245B RID: 9307 RVA: 0x000F8655 File Offset: 0x000F6855
		protected IEnumerator UpdateSpawner()
		{
			this.waveStartTime = Time.time;
			this.lastActionTime = Time.time;
			EventManager.onCreatureSpawn += this.CreatureSpawn;
			EventManager.onCreatureAttacking += this.CreatureAttack;
			EventManager.onCreatureHit += this.CreatureHit;
			EventManager.onCreatureKill += this.CreatureKill;
			EventManager.onCreatureDespawn += this.CreatureDespawn;
			for (;;)
			{
				int totalAlive = this.CountAlive();
				WaveData.SpawnData toSpawn = null;
				int spawnQueueCount = this.creatureQueue.Count;
				for (int i = 0; i < spawnQueueCount; i++)
				{
					WaveData.SpawnData spawnData = this.creatureQueue[i];
					int alive;
					if (spawnData.data != null && (spawnData.spawnGroup.prereqGroup == null || (this.groupAliveCounts.TryGetValue(spawnData.spawnGroup.prereqGroup, out alive) && alive <= spawnData.spawnGroup.prereqMaxRemainingAlive)) && totalAlive + 1 <= this.waveData.GetMaxAlive() && this.waveFactionInfos[spawnData.data.factionId].aliveCount + 1 <= this.waveFactionInfos[spawnData.data.factionId].data.factionMaxAlive)
					{
						toSpawn = spawnData;
						break;
					}
				}
				if (toSpawn != null)
				{
					WaveSpawner.SpawnPoint spawnPoint = this.PickSpawnPoint(toSpawn.spawnGroup.spawnPointIndex);
					if (spawnPoint != null)
					{
						base.StartCoroutine(toSpawn.data.SpawnCoroutine(spawnPoint.transform.position, spawnPoint.transform.eulerAngles.y, null, delegate(Creature spawned)
						{
							this.spawnedCreatures.Add(spawned);
							int alive2;
							if (this.groupAliveCounts.TryGetValue(toSpawn.spawnGroup, out alive2))
							{
								this.groupAliveCounts[toSpawn.spawnGroup] = alive2 + 1;
							}
							else
							{
								this.groupAliveCounts.Add(toSpawn.spawnGroup, 1);
							}
							spawned.spawnGroup = toSpawn.spawnGroup;
							this.creatureQueue.Remove(toSpawn);
						}, true, null));
						this.lastActionTime = Time.time;
					}
					else
					{
						toSpawn = null;
					}
				}
				this.CheckSpawnedList();
				bool queueStuck = toSpawn == null && this.creatureQueue.Count > 0 && totalAlive < this.waveData.GetMaxAlive();
				if (this.CheckEnded(queueStuck) && !this.populating)
				{
					if (this.waveData.loopBehavior != WaveData.LoopBehavior.NoLoop)
					{
						base.StartCoroutine(this.PopulateQueue());
						if (this.OnWaveLoopEvent != null)
						{
							this.OnWaveLoopEvent.Invoke();
						}
					}
					else
					{
						this.StopWave(true);
					}
				}
				if (this.inactivityEndTimer > 0f)
				{
					if (Time.time > this.lastActionTime + this.inactivityEndTimer)
					{
						foreach (Creature creature in Creature.allActive)
						{
							if (!creature.isPlayer && creature.IsVisible())
							{
								this.lastActionTime = Time.time;
								break;
							}
						}
					}
					if (Time.time > this.lastActionTime + this.inactivityEndTimer)
					{
						this.StopWave(false);
					}
				}
				yield return Yielders.ForSeconds(this.updateRate);
			}
			yield break;
		}

		// Token: 0x0600245C RID: 9308 RVA: 0x000F8664 File Offset: 0x000F6864
		private void CreatureSpawn(Creature creature)
		{
			this.lastActionTime = Time.time;
		}

		// Token: 0x0600245D RID: 9309 RVA: 0x000F8671 File Offset: 0x000F6871
		private void CreatureAttack(Creature attacker, Creature targetCreature, Transform targetTransform, BrainModuleAttack.AttackType type, BrainModuleAttack.AttackStage stage)
		{
			if (stage == BrainModuleAttack.AttackStage.Attack)
			{
				this.lastActionTime = Time.time;
			}
		}

		// Token: 0x0600245E RID: 9310 RVA: 0x000F8683 File Offset: 0x000F6883
		private void CreatureHit(Creature creature, CollisionInstance collisionInstance, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.lastActionTime = Time.time;
			}
		}

		// Token: 0x0600245F RID: 9311 RVA: 0x000F8694 File Offset: 0x000F6894
		private void CreatureKill(Creature creature, Player player, CollisionInstance collisionInstance, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				this.RemoveFromWave(creature);
			}
			if (eventTime == EventTime.OnEnd)
			{
				this.lastActionTime = Time.time;
			}
		}

		// Token: 0x06002460 RID: 9312 RVA: 0x000F86B1 File Offset: 0x000F68B1
		private void CreatureDespawn(Creature creature, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				this.RemoveFromWave(creature);
			}
			if (eventTime == EventTime.OnEnd)
			{
				this.lastActionTime = Time.time;
			}
		}

		// Token: 0x06002461 RID: 9313 RVA: 0x000F86CC File Offset: 0x000F68CC
		private int CountAlive()
		{
			foreach (KeyValuePair<int, WaveSpawner.FactionInfo> factionInfo in this.waveFactionInfos)
			{
				factionInfo.Value.aliveCount = 0;
			}
			int totalAlive = 0;
			for (int i = 0; i < Creature.allActive.Count; i++)
			{
				if (!(Creature.allActive[i] == null) && Creature.allActive[i].state != Creature.State.Dead && !Creature.allActive[i].player && Creature.allActive[i].loaded && Creature.allActive[i].countTowardsMaxAlive && (!this.area || this.area.spawnableArea == Creature.allActive[i].currentArea))
				{
					WaveSpawner.FactionInfo factionInfo2;
					if (this.waveFactionInfos.TryGetValue(Creature.allActive[i].factionId, out factionInfo2))
					{
						factionInfo2.aliveCount++;
					}
					totalAlive++;
				}
			}
			return totalAlive;
		}

		// Token: 0x06002462 RID: 9314 RVA: 0x000F8808 File Offset: 0x000F6A08
		private void CheckSpawnedList()
		{
			for (int i = this.spawnedCreatures.Count - 1; i >= 0; i--)
			{
				Creature creature = this.spawnedCreatures[i];
				if (creature.isKilled || !Creature.allActive.Contains(creature))
				{
					this.RemoveFromWave(creature);
				}
			}
		}

		// Token: 0x06002463 RID: 9315 RVA: 0x000F8858 File Offset: 0x000F6A58
		private bool CheckEnded(bool queueStuck)
		{
			bool enemiesAlive = this.IsEnemiesAlive();
			if (this.waveData.loopBehavior == WaveData.LoopBehavior.NoLoop)
			{
				return !enemiesAlive;
			}
			if (this.waveData.loopBehavior == WaveData.LoopBehavior.LoopSeamless)
			{
				return !enemiesAlive || this.creatureQueue.Count == 0 || queueStuck;
			}
			return this.creatureQueue.Count == 0;
		}

		// Token: 0x06002464 RID: 9316 RVA: 0x000F88B4 File Offset: 0x000F6AB4
		private bool IsEnemiesAlive()
		{
			if (this.spawnedCreatures.Count == 0 && this.creatureQueue.Count == 0)
			{
				return false;
			}
			CreatureData compareData = (Player.currentCreature != null && Player.currentCreature.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Passive && Player.currentCreature.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored) ? Player.currentCreature.data : null;
			for (int i = 0; i < this.spawnedCreatures.Count + this.creatureQueue.Count; i++)
			{
				CreatureData creatureData = (i < this.spawnedCreatures.Count) ? this.spawnedCreatures[i].data : this.creatureQueue[i - this.spawnedCreatures.Count].data;
				if (compareData == null)
				{
					GameData.Faction creatureFaction = Catalog.gameData.GetFaction(creatureData.factionId);
					if (creatureFaction.attackBehaviour != GameData.Faction.AttackBehaviour.Passive && creatureFaction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored)
					{
						compareData = creatureData;
					}
				}
				else if (this.AreEnemies(creatureData, compareData))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002465 RID: 9317 RVA: 0x000F89B8 File Offset: 0x000F6BB8
		public bool AreEnemies(CreatureData dataA, CreatureData dataB)
		{
			if (dataA == null || dataB == null)
			{
				return false;
			}
			GameData.Faction faction = Catalog.gameData.GetFaction(dataA.factionId);
			GameData.Faction otherFaction = Catalog.gameData.GetFaction(dataB.factionId);
			return faction.attackBehaviour != GameData.Faction.AttackBehaviour.Passive && faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && otherFaction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && (faction.attackBehaviour == GameData.Faction.AttackBehaviour.Agressive || faction.id != otherFaction.id);
		}

		// Token: 0x06002466 RID: 9318 RVA: 0x000F8A28 File Offset: 0x000F6C28
		private void RemoveFromWave(Creature waveCreature)
		{
			if (!this.spawnedCreatures.Contains(waveCreature))
			{
				return;
			}
			int alive;
			if (waveCreature.spawnGroup != null && this.groupAliveCounts.TryGetValue(waveCreature.spawnGroup, out alive))
			{
				this.groupAliveCounts[waveCreature.spawnGroup] = alive - 1;
			}
			waveCreature.spawnGroup = null;
			this.spawnedCreatures.Remove(waveCreature);
			if (this.spawnedCreatures.Count + this.creatureQueue.Count == 0)
			{
				this.groupAliveCounts.Clear();
			}
		}

		// Token: 0x06002467 RID: 9319 RVA: 0x000F8AB0 File Offset: 0x000F6CB0
		public WaveSpawner.SpawnPoint PickSpawnPoint(int index = -1)
		{
			int layerMask = 1 << GameManager.GetLayer(LayerName.BodyLocomotion);
			if (index < 0)
			{
				int i = 0;
				foreach (WaveSpawner.SpawnPoint spawnPoint in this.spawnPoints)
				{
					if (Time.time - spawnPoint.lastSpawnTime > this.sameSpawnDelay && !Physics.CheckSphere(spawnPoint.transform.position, 0.35f, layerMask, QueryTriggerInteraction.Collide))
					{
						this.availableSpawnPoints[i] = spawnPoint;
						i++;
					}
				}
				return this.availableSpawnPoints[UnityEngine.Random.Range(0, i)];
			}
			if (this.spawnPoints.Count <= index)
			{
				Debug.LogError(string.Format("Spawn point index {0} do not exist!", index));
				return null;
			}
			if (Time.time - this.spawnPoints[index].lastSpawnTime > this.sameSpawnDelay && !Physics.CheckSphere(this.spawnPoints[index].transform.position, 0.35f, layerMask, QueryTriggerInteraction.Collide))
			{
				return this.spawnPoints[index];
			}
			return null;
		}

		// Token: 0x04002395 RID: 9109
		public static List<WaveSpawner> instances = new List<WaveSpawner>();

		// Token: 0x04002396 RID: 9110
		public WaveSpawner.ReferenceType referenceType = WaveSpawner.ReferenceType.EnemyConfig;

		// Token: 0x04002397 RID: 9111
		public WaveSpawner.EnemyConfigType enemyConfigType;

		// Token: 0x04002398 RID: 9112
		[Tooltip("When ticked, the wave spawner will ignore the dungeon limits and spawn NPCs regardless of the dungeon limit. This is useful for playing a dungeon room in sandbox mode")]
		public bool ignoreDungeonLimits;

		// Token: 0x04002399 RID: 9113
		[Tooltip("List of spawn points, where NPC will spawn during a wave start.")]
		public List<Transform> spawns = new List<Transform>();

		// Token: 0x0400239A RID: 9114
		[Tooltip("Adds FleePoint to the Spawn Point locations. When wave ends, alive NPC will move to these spots to despawn.")]
		public bool addAsFleepointOnStart = true;

		// Token: 0x0400239B RID: 9115
		[Tooltip("Begins wave apon loading in to the level.")]
		public bool beginWaveOnStart;

		// Token: 0x0400239C RID: 9116
		[Tooltip("Delay of Wave activation on start.")]
		public float beginWaveOnStartDelay;

		// Token: 0x0400239D RID: 9117
		[Tooltip("ID of wave that begins on start")]
		public string startWaveId;

		// Token: 0x0400239E RID: 9118
		[Tooltip("When ticked, items and dead bodies will despawn when the wave starts. When disabled, bodies and items do not despawn on start.")]
		public bool cleanBodiesAndItemsOnWaveStart = true;

		// Token: 0x0400239F RID: 9119
		[Tooltip("ID of the faction that is ignored by the NPCs of this wave.")]
		public int ignoredFactionId = -1;

		// Token: 0x040023A0 RID: 9120
		[Tooltip("Determines how often the spawner checks to see if it can spawn a new creature.")]
		public float updateRate = 2f;

		// Token: 0x040023A1 RID: 9121
		[Tooltip("Determines the delay between spawning creatures at the same spawn point.")]
		public float sameSpawnDelay = 3f;

		// Token: 0x040023A2 RID: 9122
		[Tooltip("Delay between NPC spawns.")]
		public float spawnDelay = 2f;

		// Token: 0x040023A3 RID: 9123
		[Tooltip("Automatically end the wave if no action after a specific duration.")]
		public float inactivityEndTimer = 60f;

		// Token: 0x040023A4 RID: 9124
		[Header("Event")]
		public UnityEvent OnWaveBeginEvent = new UnityEvent();

		// Token: 0x040023A5 RID: 9125
		public UnityEvent OnWaveAnyEndEvent = new UnityEvent();

		// Token: 0x040023A6 RID: 9126
		public UnityEvent OnWaveWinEvent = new UnityEvent();

		// Token: 0x040023A7 RID: 9127
		public UnityEvent OnWaveLossEvent = new UnityEvent();

		// Token: 0x040023A8 RID: 9128
		public UnityEvent OnWaveCancelEvent = new UnityEvent();

		// Token: 0x040023A9 RID: 9129
		public UnityEvent OnWaveLoopEvent = new UnityEvent();

		// Token: 0x040023AA RID: 9130
		public static UnityEvent<WaveSpawner> OnWaveSpawnerEnabledEvent = new UnityEvent<WaveSpawner>();

		// Token: 0x040023AB RID: 9131
		public static UnityEvent<WaveSpawner> OnWaveSpawnerDisabledEvent = new UnityEvent<WaveSpawner>();

		// Token: 0x040023AC RID: 9132
		public static UnityEvent<WaveSpawner> OnWaveSpawnerStartRunningEvent = new UnityEvent<WaveSpawner>();

		// Token: 0x040023AD RID: 9133
		public static UnityEvent<WaveSpawner> OnWaveSpawnerStopRunningEvent = new UnityEvent<WaveSpawner>();

		// Token: 0x040023AE RID: 9134
		[NonSerialized]
		public List<WaveSpawner.SpawnPoint> spawnPoints;

		// Token: 0x040023AF RID: 9135
		[NonSerialized]
		public WaveData waveData;

		// Token: 0x040023B0 RID: 9136
		[NonSerialized]
		public bool isRunning;

		// Token: 0x040023B1 RID: 9137
		[NonSerialized]
		public float waveStartTime;

		// Token: 0x040023B2 RID: 9138
		[NonSerialized]
		public float delayedStartTimeLeft;

		// Token: 0x040023B3 RID: 9139
		[NonSerialized]
		public float lastActionTime;

		// Token: 0x040023B4 RID: 9140
		[NonSerialized]
		public int waveCount;

		// Token: 0x040023B5 RID: 9141
		[NonSerialized]
		public List<WaveData.SpawnData> creatureQueue;

		// Token: 0x040023B6 RID: 9142
		[NonSerialized]
		public List<Creature> spawnedCreatures;

		// Token: 0x040023B8 RID: 9144
		protected AudioSource audioMusic;

		// Token: 0x040023B9 RID: 9145
		protected AudioSource audioStep;

		// Token: 0x040023BA RID: 9146
		protected AudioContainer audioGroupWave;

		// Token: 0x040023BB RID: 9147
		protected bool populating;

		// Token: 0x040023BC RID: 9148
		protected Coroutine startWaveCoroutine;

		// Token: 0x040023BD RID: 9149
		protected Coroutine updateCoroutine;

		// Token: 0x040023BE RID: 9150
		protected WaveSpawner.SpawnPoint[] availableSpawnPoints;

		// Token: 0x040023BF RID: 9151
		protected bool groupCoroutineRunning;

		// Token: 0x040023C0 RID: 9152
		protected Area area;

		// Token: 0x040023C1 RID: 9153
		protected Dictionary<int, WaveSpawner.FactionInfo> waveFactionInfos = new Dictionary<int, WaveSpawner.FactionInfo>();

		// Token: 0x040023C2 RID: 9154
		protected Dictionary<WaveData.Group, int> groupAliveCounts = new Dictionary<WaveData.Group, int>();

		// Token: 0x040023C3 RID: 9155
		private WaveData currentLoadedData;

		// Token: 0x020009E4 RID: 2532
		public enum ReferenceType
		{
			// Token: 0x04004645 RID: 17989
			None,
			// Token: 0x04004646 RID: 17990
			EnemyConfig
		}

		// Token: 0x020009E5 RID: 2533
		public enum EnemyConfigType
		{
			// Token: 0x04004648 RID: 17992
			MeleeOnlyStd,
			// Token: 0x04004649 RID: 17993
			MeleeFocusedStd,
			// Token: 0x0400464A RID: 17994
			MixedStd,
			// Token: 0x0400464B RID: 17995
			RangedFocusedStd,
			// Token: 0x0400464C RID: 17996
			MeleeOnlyArena,
			// Token: 0x0400464D RID: 17997
			MeleeFocusedArena,
			// Token: 0x0400464E RID: 17998
			MixedArena,
			// Token: 0x0400464F RID: 17999
			RangedFocusedArena,
			// Token: 0x04004650 RID: 18000
			MeleeOnlyEnd,
			// Token: 0x04004651 RID: 18001
			MeleeFocusedEnd,
			// Token: 0x04004652 RID: 18002
			MixedEnd,
			// Token: 0x04004653 RID: 18003
			RangedFocusedEnd
		}

		// Token: 0x020009E6 RID: 2534
		[Serializable]
		public class SpawnPoint
		{
			// Token: 0x060044D9 RID: 17625 RVA: 0x001943AA File Offset: 0x001925AA
			public SpawnPoint(Transform transform, Area area = null)
			{
				this.transform = transform;
				this.lastSpawnTime = 0f;
				this.area = area;
			}

			// Token: 0x04004654 RID: 18004
			public Transform transform;

			// Token: 0x04004655 RID: 18005
			public float lastSpawnTime;

			// Token: 0x04004656 RID: 18006
			public Area area;
		}

		// Token: 0x020009E7 RID: 2535
		[Serializable]
		public class FactionInfo
		{
			// Token: 0x04004657 RID: 18007
			public WaveData.WaveFaction data;

			// Token: 0x04004658 RID: 18008
			public int aliveCount;
		}
	}
}
