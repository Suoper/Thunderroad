using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002BE RID: 702
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/CreatureSpawner.html")]
	[AddComponentMenu("ThunderRoad/Levels/Spawners/CreatureTable Spawner")]
	public class CreatureSpawner : MonoBehaviour
	{
		// Token: 0x17000217 RID: 535
		// (get) Token: 0x06002200 RID: 8704 RVA: 0x000EA8F7 File Offset: 0x000E8AF7
		// (set) Token: 0x06002201 RID: 8705 RVA: 0x000EA8FF File Offset: 0x000E8AFF
		public CreatureSpawner.State CurrentState { get; private set; }

		// Token: 0x06002202 RID: 8706 RVA: 0x000EA908 File Offset: 0x000E8B08
		public List<ValueDropdownItem<string>> GetAllCreatureTableID()
		{
			return Catalog.GetDropdownAllID(Category.CreatureTable, "None");
		}

		// Token: 0x06002203 RID: 8707 RVA: 0x000EA915 File Offset: 0x000E8B15
		public void Spawn()
		{
			this.Spawn(null, null);
		}

		// Token: 0x06002204 RID: 8708 RVA: 0x000EA920 File Offset: 0x000E8B20
		public void Spawn(Action completeCallback = null, System.Random randomGen = null)
		{
			CreatureSpawner.<>c__DisplayClass30_0 CS$<>8__locals1 = new CreatureSpawner.<>c__DisplayClass30_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.completeCallback = completeCallback;
			string text;
			bool hasEnemyConfig = Level.TryGetCurrentLevelOption(LevelOption.EnemyConfig, out text);
			CreatureSpawner.State currentState = this.CurrentState;
			if (((currentState == CreatureSpawner.State.Spawning || currentState == CreatureSpawner.State.Spawned) && !this.respawnOnDeath) || (string.IsNullOrEmpty(this.creatureTableID) && !hasEnemyConfig))
			{
				return;
			}
			CS$<>8__locals1.creatureTableData = null;
			if (hasEnemyConfig)
			{
				CS$<>8__locals1.creatureTableData = this.GetCreatureTableData();
				if (CS$<>8__locals1.creatureTableData != null)
				{
					this.creatureTableID = CS$<>8__locals1.creatureTableData.id;
				}
			}
			if (CS$<>8__locals1.creatureTableData == null)
			{
				CS$<>8__locals1.creatureTableData = Catalog.GetData<CreatureTable>(this.creatureTableID, true);
			}
			CreatureData creatureData;
			if (CS$<>8__locals1.creatureTableData != null && CS$<>8__locals1.creatureTableData.TryPick(out creatureData, randomGen))
			{
				Vector3 spawnPosition = base.transform.position;
				Quaternion spawnRotation = base.transform.rotation;
				WayPoint[] waypointArray = this.waypointsRoot ? this.waypointsRoot.GetComponentsInChildren<WayPoint>() : base.GetComponentsInChildren<WayPoint>();
				if (waypointArray != null && this.spawnAtRandomWaypoint && waypointArray.Length != 0)
				{
					int randomChildIdx = (randomGen != null) ? randomGen.Next(waypointArray.Length) : UnityEngine.Random.Range(0, waypointArray.Length);
					spawnPosition = waypointArray[randomChildIdx].transform.position;
					spawnRotation = waypointArray[randomChildIdx].transform.rotation;
				}
				NavMeshHit navMeshHit;
				if (this.spawnOnNavMesh && NavMesh.SamplePosition(spawnPosition, out navMeshHit, 2f, -1))
				{
					spawnPosition = navMeshHit.position;
				}
				this.CurrentState = CreatureSpawner.State.Spawning;
				if (this.asyncSpawn)
				{
					creatureData.SpawnAsync(spawnPosition, spawnRotation.eulerAngles.y, null, true, waypointArray, new Action<Creature>(CS$<>8__locals1.<Spawn>g__FinishSpawn|0));
					return;
				}
				base.StartCoroutine(creatureData.SpawnCoroutine(spawnPosition, spawnRotation.eulerAngles.y, null, new Action<Creature>(CS$<>8__locals1.<Spawn>g__FinishSpawn|0), true, waypointArray));
			}
		}

		// Token: 0x06002205 RID: 8709 RVA: 0x000EAAE8 File Offset: 0x000E8CE8
		private void OnCreatureKill(CollisionInstance hit, EventTime time)
		{
			ColliderGroup targetColliderGroup = hit.targetColliderGroup;
			Creature creature2;
			if (targetColliderGroup == null)
			{
				creature2 = null;
			}
			else
			{
				CollisionHandler collisionHandler = targetColliderGroup.collisionHandler;
				if (collisionHandler == null)
				{
					creature2 = null;
				}
				else
				{
					RagdollPart ragdollPart = collisionHandler.ragdollPart;
					if (ragdollPart == null)
					{
						creature2 = null;
					}
					else
					{
						Ragdoll ragdoll = ragdollPart.ragdoll;
						creature2 = ((ragdoll != null) ? ragdoll.creature : null);
					}
				}
			}
			Creature creature = creature2;
			if (creature != null)
			{
				creature.OnKillEvent -= this.OnCreatureKill;
			}
			if (time == EventTime.OnEnd && !LevelManager.isLoadingLocked)
			{
				this.RunAfter(new Action(this.Spawn), this.respawnDelay, false);
			}
		}

		// Token: 0x06002206 RID: 8710 RVA: 0x000EAB68 File Offset: 0x000E8D68
		public void SpawnCreature(CreatureData creatureData, Action completeCallback = null)
		{
			CreatureSpawner.<>c__DisplayClass32_0 CS$<>8__locals1 = new CreatureSpawner.<>c__DisplayClass32_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.completeCallback = completeCallback;
			if (this.CurrentState == CreatureSpawner.State.Spawning || this.CurrentState == CreatureSpawner.State.Spawned || string.IsNullOrEmpty(this.creatureTableID))
			{
				return;
			}
			Vector3 spawnPosition = base.transform.position;
			Quaternion spawnRotation = base.transform.rotation;
			WayPoint[] waypointArray = this.waypointsRoot ? this.waypointsRoot.GetComponentsInChildren<WayPoint>() : base.GetComponentsInChildren<WayPoint>();
			if (waypointArray != null && this.spawnAtRandomWaypoint && waypointArray.Length != 0)
			{
				int randomChildIdx = UnityEngine.Random.Range(0, waypointArray.Length);
				spawnPosition = waypointArray[randomChildIdx].transform.position;
				spawnRotation = waypointArray[randomChildIdx].transform.rotation;
			}
			NavMeshHit navMeshHit;
			if (this.spawnOnNavMesh && NavMesh.SamplePosition(spawnPosition, out navMeshHit, 2f, -1))
			{
				spawnPosition = navMeshHit.position;
			}
			this.CurrentState = CreatureSpawner.State.Spawning;
			if (this.asyncSpawn)
			{
				creatureData.SpawnAsync(spawnPosition, spawnRotation.eulerAngles.y, null, true, waypointArray, new Action<Creature>(CS$<>8__locals1.<SpawnCreature>g__FinishSpawn|0));
				return;
			}
			base.StartCoroutine(creatureData.SpawnCoroutine(spawnPosition, spawnRotation.eulerAngles.y, null, new Action<Creature>(CS$<>8__locals1.<SpawnCreature>g__FinishSpawn|0), true, waypointArray));
		}

		/// <summary>
		/// Returns the creature data linked to this ID in the catalog.
		/// </summary>
		/// <returns>The creature data in the catalog, null if not found</returns>
		// Token: 0x06002207 RID: 8711 RVA: 0x000EAC98 File Offset: 0x000E8E98
		private CreatureTable GetCreatureTableData()
		{
			this.currentLoadedData = null;
			if (this.referenceType == CreatureSpawner.ReferenceType.EnemyConfig)
			{
				EnemyConfig loadedEnemyConfig = null;
				string enemyConfigId;
				if (Level.TryGetCurrentLevelOption(LevelOption.EnemyConfig, out enemyConfigId))
				{
					loadedEnemyConfig = Catalog.GetData<EnemyConfig>(enemyConfigId, true);
				}
				if (loadedEnemyConfig == null)
				{
					Debug.LogError("[Creature spawner] GetCreatureTableData() could not fetch the Enemy Config data from the level options.");
					return null;
				}
				this.currentLoadedData = loadedEnemyConfig.GetCreatureTable(this.enemyConfigType);
				if (this.currentLoadedData == null)
				{
					Debug.LogWarning(string.Format("[Creature Spawner] GetCreatureTableData() could not fetch the Creature Table for the EnemyConfigType: {0} from the Enemy Config: {1}", this.enemyConfigType, loadedEnemyConfig.id));
				}
				else
				{
					Debug.Log(string.Format("[Creature Spawner] GetCreatureTableData() Loaded {0} - Creature Table Id: {1}  to replace the EnemyConfigType: {2}", loadedEnemyConfig.id, this.currentLoadedData.id, this.enemyConfigType));
				}
			}
			return this.currentLoadedData;
		}

		// Token: 0x06002208 RID: 8712 RVA: 0x000EAD48 File Offset: 0x000E8F48
		public void ResetSpawner()
		{
			base.StartCoroutine(this.ResetSpawnerCoroutine());
		}

		// Token: 0x06002209 RID: 8713 RVA: 0x000EAD58 File Offset: 0x000E8F58
		public void SetCreaturesToWaveNPCS()
		{
			foreach (CreatureSpawner.SpawnedCreature spawnedCreature in this.spawnedCreatures)
			{
				spawnedCreature.creature.spawnGroup = this.spawnGroup;
			}
		}

		// Token: 0x0600220A RID: 8714 RVA: 0x000EADB4 File Offset: 0x000E8FB4
		private IEnumerator ResetSpawnerCoroutine()
		{
			yield return this.DespawnCreaturesCoroutine(true);
			this.CurrentState = CreatureSpawner.State.Init;
			this.Spawn();
			yield break;
		}

		/// <summary>
		/// Despawn creatures
		/// </summary>
		/// <param name="allCreatures">True if the dead creatures should also be despawned</param>
		/// <returns></returns>
		// Token: 0x0600220B RID: 8715 RVA: 0x000EADC3 File Offset: 0x000E8FC3
		private IEnumerator DespawnCreaturesCoroutine(bool allCreatures)
		{
			while (this.CurrentState == CreatureSpawner.State.Spawning)
			{
				yield return null;
			}
			List<CreatureSpawner.SpawnedCreature> spawnedCreaturesCopy = new List<CreatureSpawner.SpawnedCreature>(this.spawnedCreatures);
			for (int i = 0; i < spawnedCreaturesCopy.Count; i++)
			{
				spawnedCreaturesCopy[i].creature.Despawn();
			}
			if (allCreatures)
			{
				for (int j = 0; j < this.deadCreatures.Count; j++)
				{
					this.deadCreatures[j].creature.Despawn();
				}
				this.deadCreatures.Clear();
			}
			yield break;
		}

		// Token: 0x0600220C RID: 8716 RVA: 0x000EADD9 File Offset: 0x000E8FD9
		public Creature GetSpawnedCreature(int index)
		{
			if (index < 0)
			{
				return null;
			}
			if (index >= this.spawnedCreatures.Count)
			{
				return null;
			}
			return this.spawnedCreatures[index].creature;
		}

		// Token: 0x0600220D RID: 8717 RVA: 0x000EAE02 File Offset: 0x000E9002
		protected void Start()
		{
			UnityEvent onStart = this.OnStart;
			if (onStart != null)
			{
				onStart.Invoke();
			}
			if (Level.current != null && Level.current.loaded && this.spawnOnStart)
			{
				this.Spawn();
			}
		}

		// Token: 0x0600220E RID: 8718 RVA: 0x000EAE3C File Offset: 0x000E903C
		public void OnDrawGizmos()
		{
			WayPoint.SpawnerDrawGizmos(base.transform, this.waypointsRoot);
		}

		// Token: 0x040020E8 RID: 8424
		public string creatureTableID;

		// Token: 0x040020E9 RID: 8425
		public CreatureSpawner.ReferenceType referenceType = CreatureSpawner.ReferenceType.EnemyConfig;

		// Token: 0x040020EA RID: 8426
		public CreatureSpawner.EnemyConfigType enemyConfigType;

		// Token: 0x040020EB RID: 8427
		[Tooltip("Spawns creatures in faster/immediately, but can break some of them. If your creatures are behaving weirdly, uncheck this box.")]
		public bool asyncSpawn = true;

		// Token: 0x040020EC RID: 8428
		[Tooltip("The creature spawner is ignored by areas(?)")]
		public bool ignoredByAreas;

		// Token: 0x040020ED RID: 8429
		[Tooltip("When set to true, the creature will spawn on start")]
		public bool spawnOnStart = true;

		// Token: 0x040020EE RID: 8430
		[Tooltip("When enabled, creature will respawn when the killed.")]
		public bool respawnOnDeath;

		// Token: 0x040020EF RID: 8431
		[Tooltip("Sets a delay for the respawn.")]
		public float respawnDelay = 1f;

		// Token: 0x040020F0 RID: 8432
		[Tooltip("If set to true, if this spawner is called on Level load, it'll delay the end of the loading screen until the creature is done spawning.")]
		public bool blockLoad;

		// Token: 0x040020F1 RID: 8433
		[Tooltip("Spawns the creature on a navmesh if it isn't already.")]
		public bool spawnOnNavMesh = true;

		// Token: 0x040020F2 RID: 8434
		[Tooltip("When enabled, will ignore the Dungeon rooms' max NPC count.")]
		public bool ignoreRoomMaxNPC;

		// Token: 0x040020F3 RID: 8435
		[Tooltip("When enabled, will spawn the creature on a random waypoint.")]
		public bool spawnAtRandomWaypoint = true;

		// Token: 0x040020F4 RID: 8436
		[Tooltip("Specify the waypoint the creature spawns on.")]
		public Transform waypointsRoot;

		// Token: 0x040020F5 RID: 8437
		[Header("Events")]
		public List<CreatureSpawner.BrainStateEvent> brainStateChangeEvents = new List<CreatureSpawner.BrainStateEvent>();

		// Token: 0x040020F6 RID: 8438
		public UnityEvent OnStart = new UnityEvent();

		// Token: 0x040020F7 RID: 8439
		public UnityEvent OnKill = new UnityEvent();

		// Token: 0x040020F8 RID: 8440
		public UnityEvent<Creature> OnSpawn = new UnityEvent<Creature>();

		// Token: 0x040020F9 RID: 8441
		public UnityEvent OnDespawn = new UnityEvent();

		// Token: 0x040020FB RID: 8443
		private CreatureTable currentLoadedData;

		// Token: 0x040020FC RID: 8444
		[Header("Instance")]
		protected List<CreatureSpawner.SpawnedCreature> spawnedCreatures = new List<CreatureSpawner.SpawnedCreature>();

		// Token: 0x040020FD RID: 8445
		[Header("Instance")]
		protected List<CreatureSpawner.SpawnedCreature> deadCreatures = new List<CreatureSpawner.SpawnedCreature>();

		// Token: 0x040020FE RID: 8446
		[Header("Instance")]
		protected WaveData.Group spawnGroup;

		// Token: 0x0200098E RID: 2446
		public enum State
		{
			// Token: 0x040044F9 RID: 17657
			Init,
			// Token: 0x040044FA RID: 17658
			Spawning,
			// Token: 0x040044FB RID: 17659
			Spawned
		}

		// Token: 0x0200098F RID: 2447
		public enum ReferenceType
		{
			// Token: 0x040044FD RID: 17661
			None,
			// Token: 0x040044FE RID: 17662
			EnemyConfig
		}

		// Token: 0x02000990 RID: 2448
		public enum EnemyConfigType
		{
			// Token: 0x04004500 RID: 17664
			PatrolMix,
			// Token: 0x04004501 RID: 17665
			PatrolMelee,
			// Token: 0x04004502 RID: 17666
			PatrolRanged,
			// Token: 0x04004503 RID: 17667
			AlertMix,
			// Token: 0x04004504 RID: 17668
			AlertMelee,
			// Token: 0x04004505 RID: 17669
			AlertRanged,
			// Token: 0x04004506 RID: 17670
			RareMix,
			// Token: 0x04004507 RID: 17671
			RareMelee,
			// Token: 0x04004508 RID: 17672
			RareRanged
		}

		// Token: 0x02000991 RID: 2449
		[Serializable]
		public class CreatureEvent : UnityEvent<UnityEngine.Object>
		{
		}

		// Token: 0x02000992 RID: 2450
		[Serializable]
		public class BrainStateEvent
		{
			// Token: 0x04004509 RID: 17673
			public Brain.State triggerState = Brain.State.Combat;

			// Token: 0x0400450A RID: 17674
			public float timeDelay;

			// Token: 0x0400450B RID: 17675
			[Tooltip("This should be toggled on if this brain state change event starts a wave")]
			public bool addCreatureTargetShareDelay;

			// Token: 0x0400450C RID: 17676
			public bool checkAlive = true;

			// Token: 0x0400450D RID: 17677
			public bool checkNotMuffled = true;

			// Token: 0x0400450E RID: 17678
			public bool checkSameState = true;

			// Token: 0x0400450F RID: 17679
			public CreatureSpawner.CreatureEvent onChange;
		}

		// Token: 0x02000993 RID: 2451
		public class SpawnedCreature
		{
			// Token: 0x060043EA RID: 17386 RVA: 0x001901F0 File Offset: 0x0018E3F0
			public SpawnedCreature(Creature creature)
			{
				this.creature = creature;
				creature.brain.OnStateChangeEvent += this.OnBrainStateChangeEvent;
				creature.OnDespawnEvent += this.OnCreatureDespawn;
				creature.OnKillEvent += this.OnCreatureKill;
			}

			// Token: 0x060043EB RID: 17387 RVA: 0x00190245 File Offset: 0x0018E445
			public IEnumerator DelayedStateCheck(CreatureSpawner.BrainStateEvent brainEvent)
			{
				float delay = brainEvent.timeDelay;
				if (brainEvent.addCreatureTargetShareDelay)
				{
					if (delay == 0f)
					{
						delay = 0.1f;
					}
					delay += this.creature.brain.instance.GetModule<BrainModuleDetection>(true).acquireSharedTargetDelay;
				}
				yield return Yielders.ForSeconds(delay);
				if ((this.creature.state != Creature.State.Dead || !brainEvent.checkAlive) && (!this.creature.brain.isMuffled || !brainEvent.checkNotMuffled) && (this.creature.brain.state == brainEvent.triggerState || !brainEvent.checkSameState))
				{
					brainEvent.onChange.Invoke(this.creature.brain.currentTarget);
				}
				yield break;
			}

			// Token: 0x060043EC RID: 17388 RVA: 0x0019025C File Offset: 0x0018E45C
			protected void OnBrainStateChangeEvent(Brain.State newState)
			{
				foreach (CreatureSpawner.BrainStateEvent brainEvent in this.creature.creatureSpawner.brainStateChangeEvents)
				{
					if (newState == brainEvent.triggerState)
					{
						if (brainEvent.timeDelay > 0f)
						{
							this.creature.creatureSpawner.StartCoroutine(this.DelayedStateCheck(brainEvent));
						}
						else
						{
							brainEvent.onChange.Invoke(this.creature.brain.currentTarget);
						}
					}
				}
			}

			// Token: 0x060043ED RID: 17389 RVA: 0x00190300 File Offset: 0x0018E500
			protected void OnCreatureDespawn(EventTime eventTime)
			{
				if (eventTime == EventTime.OnStart)
				{
					if (this.creature.state != Creature.State.Dead)
					{
						this.creature.creatureSpawner.OnDespawn.Invoke();
					}
					this.creature.brain.OnStateChangeEvent -= this.OnBrainStateChangeEvent;
					this.creature.OnDespawnEvent -= this.OnCreatureDespawn;
					this.creature.OnKillEvent -= this.OnCreatureKill;
					this.creature.creatureSpawner.spawnedCreatures.Remove(this);
				}
			}

			// Token: 0x060043EE RID: 17390 RVA: 0x00190398 File Offset: 0x0018E598
			protected void OnCreatureKill(CollisionInstance collisionInstance, EventTime eventTime)
			{
				this.creature.creatureSpawner.OnKill.Invoke();
				this.creature.brain.OnStateChangeEvent -= this.OnBrainStateChangeEvent;
				this.creature.OnDespawnEvent -= this.OnCreatureDespawn;
				this.creature.OnKillEvent -= this.OnCreatureKill;
				this.creature.creatureSpawner.spawnedCreatures.Remove(this);
				this.creature.creatureSpawner.deadCreatures.Add(this);
			}

			// Token: 0x04004510 RID: 17680
			public Creature creature;
		}
	}
}
