using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ThunderRoad
{
	// Token: 0x020002D6 RID: 726
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ItemSpawner.html")]
	[AddComponentMenu("ThunderRoad/Levels/Spawners/Item Spawner")]
	public class ItemSpawner : MonoBehaviour, ICheckAsset, IToolControllable
	{
		// Token: 0x17000229 RID: 553
		// (get) Token: 0x0600230D RID: 8973 RVA: 0x000F059F File Offset: 0x000EE79F
		// (set) Token: 0x0600230E RID: 8974 RVA: 0x000F05A7 File Offset: 0x000EE7A7
		public string itemId
		{
			get
			{
				return this.referenceId;
			}
			set
			{
				this.referenceId = value;
			}
		}

		// Token: 0x0600230F RID: 8975 RVA: 0x000F05B0 File Offset: 0x000EE7B0
		public List<ValueDropdownItem<string>> GetAllItemOrLootTableID()
		{
			if (this.referenceType == ItemSpawner.ReferenceType.Item)
			{
				return Catalog.GetDropdownAllID(Category.Item, "None");
			}
			return Catalog.GetDropdownAllID(Category.LootTable, "None");
		}

		// Token: 0x06002310 RID: 8976 RVA: 0x000F05D1 File Offset: 0x000EE7D1
		public bool IsCopyable()
		{
			return false;
		}

		// Token: 0x06002311 RID: 8977 RVA: 0x000F05D4 File Offset: 0x000EE7D4
		public void CopyTo(UnityEngine.Object other)
		{
			Debug.LogError("Copying ItemSpawner is not currently supported! Invoking this method does nothing.");
		}

		// Token: 0x06002312 RID: 8978 RVA: 0x000F05E0 File Offset: 0x000EE7E0
		public void CopyFrom(IToolControllable original)
		{
			Debug.LogError("Copying ItemSpawner is not currently supported! Invoking this method does nothing.");
		}

		// Token: 0x06002313 RID: 8979 RVA: 0x000F05EC File Offset: 0x000EE7EC
		public void ReparentAlign(Component other)
		{
			((IToolControllable)this).ReparentAlignTransform(other);
		}

		// Token: 0x06002314 RID: 8980 RVA: 0x000F05F5 File Offset: 0x000EE7F5
		public void Remove()
		{
			UnityEngine.Object.Destroy(this);
		}

		// Token: 0x06002315 RID: 8981 RVA: 0x000F05FD File Offset: 0x000EE7FD
		public Transform GetTransform()
		{
			return base.transform;
		}

		// Token: 0x06002316 RID: 8982 RVA: 0x000F0605 File Offset: 0x000EE805
		private Vector3 MakeLocalForce(Vector3 inputForce, Transform reference)
		{
			return reference.right * inputForce.x + reference.up * inputForce.y + reference.forward * inputForce.z;
		}

		// Token: 0x06002317 RID: 8983 RVA: 0x000F0644 File Offset: 0x000EE844
		private void Awake()
		{
			if (this.spawnOnLevelLoad && this.spawnOnStart)
			{
				this.spawnOnStart = false;
			}
			if (this.spawnOnLevelLoad)
			{
				EventManager.onLevelLoad += this.HandleLevelLoaded;
			}
			if (this.parentSpawner)
			{
				this.parentSpawner.SubscribeChildSpawner(this);
			}
			this.itemSpawnerLimiter = base.GetComponentInParent<ItemSpawnerLimiter>();
		}

		// Token: 0x06002318 RID: 8984 RVA: 0x000F06A6 File Offset: 0x000EE8A6
		protected void Start()
		{
			if (this.spawnOnStart && !this.spawnOnLevelLoad)
			{
				this.Spawn();
			}
		}

		// Token: 0x06002319 RID: 8985 RVA: 0x000F06BE File Offset: 0x000EE8BE
		private void OnDestroy()
		{
			if (this.spawnOnLevelLoad)
			{
				EventManager.onLevelLoad -= this.HandleLevelLoaded;
			}
		}

		// Token: 0x0600231A RID: 8986 RVA: 0x000F06D9 File Offset: 0x000EE8D9
		private void OnDisable()
		{
			if (this.parentSpawner)
			{
				this.parentSpawner.UnSubscribeChildSpawner(this);
			}
		}

		// Token: 0x0600231B RID: 8987 RVA: 0x000F06F4 File Offset: 0x000EE8F4
		private bool IsClosedGraph()
		{
			ItemSpawner parent = this.parentSpawner;
			bool firstCheck = true;
			while (parent != null)
			{
				if (parent == this || (!firstCheck && parent == this.parentSpawner))
				{
					Debug.LogError("ItemSpawner " + base.gameObject.GetPathFromRoot() + " has a self reference loop in parent spawner chain");
					return true;
				}
				firstCheck = false;
				parent = parent.parentSpawner;
			}
			return false;
		}

		// Token: 0x0600231C RID: 8988 RVA: 0x000F075A File Offset: 0x000EE95A
		public void SubscribeChildSpawner(ItemSpawner child)
		{
			this.childSpawners.Add(child);
		}

		// Token: 0x0600231D RID: 8989 RVA: 0x000F0768 File Offset: 0x000EE968
		public void UnSubscribeChildSpawner(ItemSpawner child)
		{
			this.childSpawners.Remove(child);
		}

		// Token: 0x0600231E RID: 8990 RVA: 0x000F0778 File Offset: 0x000EE978
		public bool IsCurrentlySpawning()
		{
			if (!this.IsClosedGraph())
			{
				for (int i = 0; i < this.childSpawners.Count; i++)
				{
					if (this.childSpawners[i].IsCurrentlySpawning())
					{
						return true;
					}
				}
				return this.currentlySpawning > 0;
			}
			return false;
		}

		// Token: 0x0600231F RID: 8991 RVA: 0x000F07C5 File Offset: 0x000EE9C5
		public void Spawn()
		{
			this.Spawn(true, true, true, null, -1, true);
		}

		// Token: 0x06002320 RID: 8992 RVA: 0x000F07D4 File Offset: 0x000EE9D4
		public int Spawn(bool checkParentSpawner = true, bool checkIgnoreOnAndroid = true, bool checkSpawnLimiter = true, System.Random randomGen = null, int maxItemCount = -1, bool allowDuplicates = true)
		{
			ItemSpawner.<>c__DisplayClass55_0 CS$<>8__locals1 = new ItemSpawner.<>c__DisplayClass55_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.randomGen = randomGen;
			CS$<>8__locals1.allowDuplicates = allowDuplicates;
			CS$<>8__locals1.checkIgnoreOnAndroid = checkIgnoreOnAndroid;
			CS$<>8__locals1.checkSpawnLimiter = checkSpawnLimiter;
			CS$<>8__locals1.maxItemCount = maxItemCount;
			if (!base.enabled || (checkParentSpawner && this.parentSpawner) || (CS$<>8__locals1.checkIgnoreOnAndroid && this.priority == ItemSpawner.Priority.IgnoreOnAndroid && Common.GetQualityLevel(false) == QualityLevel.Android) || (CS$<>8__locals1.checkSpawnLimiter && this.itemSpawnerLimiter))
			{
				return 0;
			}
			List<ItemData> itemDataList = this.GetItemData(CS$<>8__locals1.randomGen);
			if (itemDataList.IsNullOrEmpty())
			{
				return 0;
			}
			LootTableBase isLootTable = this.currentLoadedData as LootTableBase;
			int currentSpawn = 0;
			if (this.spawnerType != ItemSpawner.SpawnerType.UseReferenceId)
			{
				this.spawnCount = itemDataList.Count;
			}
			for (int i = 0; i < this.spawnCount; i++)
			{
				ItemData itemData = itemDataList[0];
				if (this.spawnerType != ItemSpawner.SpawnerType.UseReferenceId)
				{
					itemData = itemDataList[i];
				}
				if (CS$<>8__locals1.maxItemCount > 0 && currentSpawn >= CS$<>8__locals1.maxItemCount)
				{
					break;
				}
				if (!CS$<>8__locals1.allowDuplicates)
				{
					for (int j = 0; j < this.spawnedItems.Count; j++)
					{
						if (isLootTable != null || this.spawnedItems[j].data.id == ((itemData != null) ? itemData.id : null))
						{
							return currentSpawn;
						}
					}
				}
				if (itemData != null)
				{
					this.currentlySpawning++;
					ItemData itemData2 = itemData;
					Action<Item> callback;
					if ((callback = CS$<>8__locals1.<>9__0) == null)
					{
						callback = (CS$<>8__locals1.<>9__0 = delegate(Item item)
						{
							ItemSpawner.<>c__DisplayClass55_1 CS$<>8__locals2 = new ItemSpawner.<>c__DisplayClass55_1();
							CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
							CS$<>8__locals2.item = item;
							if (CS$<>8__locals2.item)
							{
								if (!Application.isPlaying)
								{
									CS$<>8__locals1.<>4__this.AlignAndPlaceItem(CS$<>8__locals2.item, CS$<>8__locals1.randomGen);
									CS$<>8__locals2.item.gameObject.hideFlags = HideFlags.DontSaveInEditor;
									return;
								}
								CS$<>8__locals1.<>4__this.spawnedItems.Add(CS$<>8__locals2.item);
								CS$<>8__locals2.item.OnDespawnEvent += CS$<>8__locals2.<Spawn>g__OnItemDespawn|1;
								CS$<>8__locals2.item.spawner = CS$<>8__locals1.<>4__this;
								CS$<>8__locals2.item.SetOwner(CS$<>8__locals1.<>4__this.defaultOwner);
								CS$<>8__locals2.item.DisallowDespawn = !CS$<>8__locals1.<>4__this.allowDespawn;
								Holder holder = null;
								if (CS$<>8__locals1.<>4__this.holderObject)
								{
									holder = CS$<>8__locals1.<>4__this.holderObject.GetComponent<Holder>();
								}
								if (!holder)
								{
									holder = CS$<>8__locals1.<>4__this.GetComponent<Holder>();
								}
								if (holder)
								{
									holder.Snap(CS$<>8__locals2.item, false);
								}
								else
								{
									CS$<>8__locals1.<>4__this.AlignAndPlaceItem(CS$<>8__locals2.item, CS$<>8__locals1.randomGen);
									if (CS$<>8__locals1.<>4__this.ignoreCollisionCollider != null)
									{
										CS$<>8__locals2.item.IgnoreColliderCollision(CS$<>8__locals1.<>4__this.ignoreCollisionCollider);
									}
									if (CS$<>8__locals1.<>4__this.ignoreCollisionItem != null)
									{
										CS$<>8__locals2.item.IgnoreObjectCollision(CS$<>8__locals1.<>4__this.ignoreCollisionItem);
									}
									if (CS$<>8__locals1.<>4__this.ignoreCollisionRagdoll != null)
									{
										CS$<>8__locals2.item.IgnoreRagdollCollision(CS$<>8__locals1.<>4__this.ignoreCollisionRagdoll);
									}
									if (CS$<>8__locals1.<>4__this.ignoredPlayerParts != (RagdollPart.Type)0)
									{
										Creature currentCreature = Player.currentCreature;
										if (((currentCreature != null) ? currentCreature.ragdoll : null) != null)
										{
											CS$<>8__locals2.item.IgnoreRagdollCollision(Player.currentCreature.ragdoll, ~CS$<>8__locals1.<>4__this.ignoredPlayerParts);
										}
									}
									if (CS$<>8__locals1.<>4__this.forceThrow)
									{
										CS$<>8__locals2.item.Throw(1f, Item.FlyDetection.Forced);
									}
									PhysicBody pb = null;
									if (CS$<>8__locals1.<>4__this.linearVelocityMode == ItemSpawner.VelocityReference.InheritWorldSpace || CS$<>8__locals1.<>4__this.angularVelocityMode == ItemSpawner.VelocityReference.InheritWorldSpace)
									{
										pb = CS$<>8__locals1.<>4__this.transform.GetPhysicBodyInParent();
									}
									switch (CS$<>8__locals1.<>4__this.linearVelocityMode)
									{
									case ItemSpawner.VelocityReference.WorldSpace:
										CS$<>8__locals2.item.physicBody.velocity = CS$<>8__locals1.<>4__this.linearVelocity;
										break;
									case ItemSpawner.VelocityReference.SpawnerSpace:
										CS$<>8__locals2.item.physicBody.velocity = CS$<>8__locals1.<>4__this.MakeLocalForce(CS$<>8__locals1.<>4__this.linearVelocity, CS$<>8__locals1.<>4__this.transform);
										break;
									case ItemSpawner.VelocityReference.ItemSpace:
										CS$<>8__locals2.item.physicBody.velocity = CS$<>8__locals1.<>4__this.MakeLocalForce(CS$<>8__locals1.<>4__this.linearVelocity, CS$<>8__locals2.item.transform);
										break;
									case ItemSpawner.VelocityReference.InheritWorldSpace:
										CS$<>8__locals2.item.physicBody.velocity = ((pb != null) ? pb.velocity : CS$<>8__locals1.<>4__this.linearVelocity);
										break;
									}
									switch (CS$<>8__locals1.<>4__this.angularVelocityMode)
									{
									case ItemSpawner.VelocityReference.WorldSpace:
										CS$<>8__locals2.item.physicBody.angularVelocity = CS$<>8__locals1.<>4__this.angularVelocity;
										break;
									case ItemSpawner.VelocityReference.SpawnerSpace:
										CS$<>8__locals2.item.physicBody.angularVelocity = CS$<>8__locals1.<>4__this.MakeLocalForce(CS$<>8__locals1.<>4__this.angularVelocity, CS$<>8__locals1.<>4__this.transform);
										break;
									case ItemSpawner.VelocityReference.ItemSpace:
										CS$<>8__locals2.item.physicBody.angularVelocity = CS$<>8__locals1.<>4__this.MakeLocalForce(CS$<>8__locals1.<>4__this.angularVelocity, CS$<>8__locals2.item.transform);
										break;
									case ItemSpawner.VelocityReference.InheritWorldSpace:
										CS$<>8__locals2.item.physicBody.angularVelocity = ((pb != null) ? pb.angularVelocity : CS$<>8__locals1.<>4__this.angularVelocity);
										break;
									}
									if (CS$<>8__locals1.<>4__this.setPlayerLastHandler && Player.currentCreature != null)
									{
										CS$<>8__locals2.item.lastHandler = Player.currentCreature.handRight;
									}
								}
								UnityEvent<Item> unityEvent = CS$<>8__locals1.<>4__this.onSpawnEvent;
								if (unityEvent != null)
								{
									unityEvent.Invoke(CS$<>8__locals2.item);
								}
								for (int k = 0; k < CS$<>8__locals1.<>4__this.childSpawners.Count; k++)
								{
									if (CS$<>8__locals1.<>4__this.itemSpawnerLimiter)
									{
										CS$<>8__locals1.<>4__this.itemSpawnerLimiter.SpawnChild(CS$<>8__locals1.<>4__this.childSpawners[k], CS$<>8__locals1.allowDuplicates);
									}
									else if (CS$<>8__locals1.<>4__this.childSpawners[k])
									{
										CS$<>8__locals1.<>4__this.childSpawners[k].Spawn(false, CS$<>8__locals1.checkIgnoreOnAndroid, CS$<>8__locals1.checkSpawnLimiter, CS$<>8__locals1.randomGen, CS$<>8__locals1.maxItemCount, CS$<>8__locals1.allowDuplicates);
									}
								}
							}
							CS$<>8__locals1.<>4__this.currentlySpawning = CS$<>8__locals1.<>4__this.currentlySpawning - 1;
						});
					}
					itemData2.SpawnAsync(callback, null, null, null, true, null, Item.Owner.None);
					currentSpawn++;
				}
			}
			return currentSpawn;
		}

		/// <summary>
		/// Returns the item data linked to this ID int the catalog.
		/// If we use a loot table, use the random Pick() method.
		/// </summary>
		/// <returns>The item data in the catalog, null if not found</returns>
		// Token: 0x06002321 RID: 8993 RVA: 0x000F098C File Offset: 0x000EEB8C
		private List<ItemData> GetItemData(System.Random randomGen = null)
		{
			this.currentLoadedData = null;
			LootConfigData lootConfigData = null;
			string lootConfigId;
			if (!Level.TryGetCurrentLevelOption(LevelOption.LootConfig.Get(), out lootConfigId) || !Catalog.TryGetData<LootConfigData>(lootConfigId, out lootConfigData, true) || this.spawnerType == ItemSpawner.SpawnerType.UseReferenceId)
			{
				if (this.referenceType == ItemSpawner.ReferenceType.Item)
				{
					ItemData itemData;
					if (Catalog.TryGetData<ItemData>(this.itemId, out itemData, true))
					{
						this.currentLoadedData = itemData;
						return new List<ItemData>
						{
							this.currentLoadedData as ItemData
						};
					}
					Debug.LogError("ItemSpawner " + base.gameObject.GetPathFromRoot() + " could not find item data with ID " + this.itemId, this);
				}
				else if (this.referenceType == ItemSpawner.ReferenceType.LootTable)
				{
					LootTableBase lootTable;
					if (Catalog.TryGetData<LootTableBase>(this.itemId, out lootTable, true))
					{
						this.currentLoadedData = lootTable;
						return lootTable.Pick(0, 0, randomGen);
					}
					Debug.LogError("ItemSpawner " + base.gameObject.GetPathFromRoot() + " could not find loot table with ID " + this.itemId, this);
				}
				return null;
			}
			switch (this.spawnerType)
			{
			case ItemSpawner.SpawnerType.SideRoom:
				this.currentLoadedData = ((lootConfigData != null) ? lootConfigData.sideRoomLootTable : null);
				break;
			case ItemSpawner.SpawnerType.EnemyDrop:
				this.currentLoadedData = ((lootConfigData != null) ? lootConfigData.enemyDropLootTable : null);
				break;
			case ItemSpawner.SpawnerType.Treasure:
				this.currentLoadedData = ((lootConfigData != null) ? lootConfigData.treasureLootTable : null);
				break;
			case ItemSpawner.SpawnerType.Reward:
				this.currentLoadedData = ((lootConfigData != null) ? lootConfigData.rewardLootTable : null);
				this.spawnCount = 1;
				break;
			case ItemSpawner.SpawnerType.AltSideRoom:
				this.currentLoadedData = ((lootConfigData != null) ? lootConfigData.altSideRoomLootTable : null);
				break;
			case ItemSpawner.SpawnerType.AltTreasure:
				this.currentLoadedData = ((lootConfigData != null) ? lootConfigData.altTreasureLootTable : null);
				break;
			}
			if (this.currentLoadedData == null)
			{
				Debug.LogError("LootConfigData " + lootConfigId + " was not found in the catalog. Unable to spawn items on " + base.name);
				return null;
			}
			LootTableBase lootTable2 = this.currentLoadedData as LootTableBase;
			if (lootTable2 != null)
			{
				return lootTable2.Pick(0, 0, randomGen);
			}
			Debug.LogError("LootConfigData " + lootConfigId + " was not a LootTable. Unable to spawn items on " + base.name);
			return null;
		}

		/// <summary>
		/// Places the item.
		/// Using a separate method allows for correctly placed gizmos
		/// </summary>
		/// <param name="item">item to place and align.</param>
		// Token: 0x06002322 RID: 8994 RVA: 0x000F0B84 File Offset: 0x000EED84
		private void AlignAndPlaceItem(Item item, System.Random randomGen = null)
		{
			if (item.spawnPoint == null)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Item ",
					item.data.id,
					" has no spawn point. Unable to spawn item ",
					base.name,
					" properly, defaulting to transform position"
				}));
				item.spawnPoint = item.transform;
			}
			item.transform.MoveAlign(item.spawnPoint, base.transform, null);
			Vector3 insideUnitSphere;
			if (randomGen != null)
			{
				insideUnitSphere = new Vector3((float)randomGen.NextDouble(), (float)randomGen.NextDouble(), (float)randomGen.NextDouble());
				insideUnitSphere = insideUnitSphere.normalized * (float)randomGen.NextDouble();
			}
			else
			{
				insideUnitSphere = UnityEngine.Random.insideUnitSphere;
			}
			item.transform.position += Vector3.ProjectOnPlane(insideUnitSphere * this.randomRadius, base.transform.up);
			if (this.randomRotate)
			{
				float randomValue = (randomGen != null) ? ((float)randomGen.NextDouble()) : UnityEngine.Random.value;
				item.transform.rotation *= Quaternion.AngleAxis(randomValue * 360f, base.transform.up);
			}
			item.CacheSpawnTransformation();
		}

		/// <summary>
		/// Resets "unused" items transformations.
		/// Useful for resetting props, as if they were spawning again
		/// </summary>
		/// <param name="forceDisallowDespawn">If true, ignore disallow despawn and spawn times equal to zero. Used by item spawners</param>
		// Token: 0x06002323 RID: 8995 RVA: 0x000F0CBC File Offset: 0x000EEEBC
		public void ResetSpawnedItems(bool forceDisallowDespawn = true)
		{
			for (int i = 0; i < this.spawnedItems.Count; i++)
			{
				Item item = this.spawnedItems[i];
				if (item && LevelModuleCleaner.ShouldBeCleanedUp(item, forceDisallowDespawn))
				{
					item.ResetToSpawningTransformation();
				}
			}
		}

		/// <summary>
		/// Remove item from spawned items list (Used to keep track of what has been spawned).
		/// Automatically called on item despawn
		/// </summary>
		/// <param name="item">Item to remove from the list</param>
		// Token: 0x06002324 RID: 8996 RVA: 0x000F0D03 File Offset: 0x000EEF03
		public void UnloadFromSpawner(Item item)
		{
			this.spawnedItems.Remove(item);
			item.spawner = null;
		}

		// Token: 0x06002325 RID: 8997 RVA: 0x000F0D19 File Offset: 0x000EEF19
		private void HandleLevelLoaded(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			EventManager.onLevelLoad -= this.HandleLevelLoaded;
		}

		// Token: 0x04002208 RID: 8712
		[Tooltip("The ID of the item/table specified in the Catelog")]
		[FormerlySerializedAs("itemId")]
		public string referenceId;

		// Token: 0x04002209 RID: 8713
		[Tooltip("When \"Item\" is selected, only Items in the catelog appear in the Reference Id.\nWhen \"LootTable\" is selected, LootTables appear in the catelog.")]
		public ItemSpawner.ReferenceType referenceType;

		// Token: 0x0400220A RID: 8714
		[Tooltip("Mainly used for Dungeons, depicts how the item order is picked.\n\nMandatory: Will ALWAYS spawn no matter the limiter.\nDefault: Will be used by the limiter, and can be removed if not chosen by the limiter.\n\nIgnore on Android: Is default for PCVR, but will never spawn on Android.")]
		public ItemSpawner.Priority priority = ItemSpawner.Priority.Default;

		// Token: 0x0400220B RID: 8715
		[Tooltip("This determines the type of the Item Spawner.\n\nUse Reference ID: Will use the Reference ID/Item ID listed at the top. +\nSide Room: Will instead spawn items of which are side-room loot in dungeons.\nEnemy Drop: The loot is dropped by enemies. +\nReward: The item spawner will spawn items that would spawn at the end of an Outpost dungeon. +\nAlt Side Room: Item spawner spawns items that are side-room loot for Dalgarian dungeons. +\nAlt Treasure: Will spawn guaranteed Dalgarian treasure.")]
		public ItemSpawner.SpawnerType spawnerType;

		// Token: 0x0400220C RID: 8716
		[Tooltip("(Optional) If this spawner is a child of a parent, place parent in to this field. The parent will spawn first to prevent this item from spawning inside the parent.")]
		public ItemSpawner parentSpawner;

		// Token: 0x0400220D RID: 8717
		[Tooltip("When ticked, will use items stored inside pool.")]
		public bool pooled;

		// Token: 0x0400220E RID: 8718
		[Tooltip("Spawns items upon start.")]
		public bool spawnOnStart;

		// Token: 0x0400220F RID: 8719
		[Tooltip("Spawns items upon level load.")]
		public bool spawnOnLevelLoad;

		// Token: 0x04002210 RID: 8720
		[Tooltip("When ticked, items spawned by this spawner will be despawned on wave start or after some time.")]
		public bool allowDespawn;

		// Token: 0x04002211 RID: 8721
		[Tooltip("How many items spawn when spawner is initiated")]
		public int spawnCount = 1;

		// Token: 0x04002212 RID: 8722
		[Tooltip("The default owner of this item when it spawns")]
		public Item.Owner defaultOwner;

		// Token: 0x04002213 RID: 8723
		[Tooltip("Radius of which items can spawn in, in random locations inside the radius.")]
		public float randomRadius;

		// Token: 0x04002214 RID: 8724
		[Tooltip("When enabled, items will be randomly rotated when spawned.")]
		public bool randomRotate;

		// Token: 0x04002215 RID: 8725
		[Tooltip("(Optional) When Item is spawned, they will spawn inside referenced Holder.")]
		public Transform holderObject;

		// Token: 0x04002216 RID: 8726
		[Tooltip("(Optional) When Item is spawned, they will spawn on referenced Rope or RopeSimple component.")]
		public RopeSimple ropeTemplate;

		// Token: 0x04002217 RID: 8727
		public UnityEvent<Item> onSpawnEvent = new UnityEvent<Item>();

		// Token: 0x04002218 RID: 8728
		[NonSerialized]
		public ItemSpawnerLimiter itemSpawnerLimiter;

		// Token: 0x04002219 RID: 8729
		[Header("Spawn in Motion")]
		[Tooltip("Sets the reference transform for the item's linear velocity on spawn.\nWorld Space applies force in the world directions (X/Y/Z direction is world-based).\nSpawner Space applies force in the item spawner's transform directions (X/Y/Z direction is based on spawner rotation).\nItem Space applies force based on the item's rotation. (X/Y/Z direction is local to the item)\nInherit World Space will make the spawned item match the motion of the item spawner, if it's on a moving physics body.")]
		public ItemSpawner.VelocityReference linearVelocityMode;

		// Token: 0x0400221A RID: 8730
		[Tooltip("Measured in meters per second")]
		public Vector3 linearVelocity;

		// Token: 0x0400221B RID: 8731
		[Tooltip("Sets the reference transform for the item's angular velocity on spawn.\nWorld Space applies torque in the world directions (X/Y/Z direction is world-based).\nSpawner Space applies torque in the item spawner's transform directions (X/Y/Z direction is based on spawner rotation).\nItem Space applies force torque on the item's rotation. (X/Y/Z direction is local to the item)\nInherit World Space will make the spawned item match the motion of the item spawner, if it's on a moving physics body.")]
		public ItemSpawner.VelocityReference angularVelocityMode;

		// Token: 0x0400221C RID: 8732
		[Tooltip("Measured in radians per second")]
		public Vector3 angularVelocity;

		// Token: 0x0400221D RID: 8733
		[Tooltip("Set this to true to force the item to fly as though thrown.")]
		public bool forceThrow;

		// Token: 0x0400221E RID: 8734
		[Tooltip("Use this to prevent the item from colliding with something else on spawn.")]
		public Collider ignoreCollisionCollider;

		// Token: 0x0400221F RID: 8735
		[Tooltip("Use this to prevent the item from colliding with another item on spawn.")]
		public Item ignoreCollisionItem;

		// Token: 0x04002220 RID: 8736
		[Tooltip("Use this to prevent the item from colliding with an input ragdoll on spawn.")]
		public Ragdoll ignoreCollisionRagdoll;

		// Token: 0x04002221 RID: 8737
		[Tooltip("Set this to true if this item should act as though the player was the last handler. This means the item will not collide with the player when spawned.")]
		public bool setPlayerLastHandler;

		// Token: 0x04002222 RID: 8738
		[Tooltip("Use this field to prevent the spawned item(s) from colliding with specific parts of the player body.")]
		public RagdollPart.Type ignoredPlayerParts;

		// Token: 0x04002223 RID: 8739
		private CatalogData currentLoadedData;

		// Token: 0x04002224 RID: 8740
		private List<Item> spawnedItems = new List<Item>();

		// Token: 0x04002225 RID: 8741
		private List<ItemSpawner> childSpawners = new List<ItemSpawner>();

		// Token: 0x04002226 RID: 8742
		private int currentlySpawning;

		// Token: 0x020009B4 RID: 2484
		public enum SpawnerType
		{
			// Token: 0x04004590 RID: 17808
			UseReferenceId,
			// Token: 0x04004591 RID: 17809
			SideRoom,
			// Token: 0x04004592 RID: 17810
			EnemyDrop,
			// Token: 0x04004593 RID: 17811
			Treasure,
			// Token: 0x04004594 RID: 17812
			Reward,
			// Token: 0x04004595 RID: 17813
			AltSideRoom,
			// Token: 0x04004596 RID: 17814
			AltTreasure
		}

		// Token: 0x020009B5 RID: 2485
		public enum Priority
		{
			// Token: 0x04004598 RID: 17816
			Mandatory,
			// Token: 0x04004599 RID: 17817
			Default,
			// Token: 0x0400459A RID: 17818
			IgnoreOnAndroid
		}

		// Token: 0x020009B6 RID: 2486
		public enum ReferenceType
		{
			// Token: 0x0400459C RID: 17820
			Item,
			// Token: 0x0400459D RID: 17821
			LootTable
		}

		// Token: 0x020009B7 RID: 2487
		public enum VelocityReference
		{
			// Token: 0x0400459F RID: 17823
			WorldSpace,
			// Token: 0x040045A0 RID: 17824
			SpawnerSpace,
			// Token: 0x040045A1 RID: 17825
			ItemSpace,
			// Token: 0x040045A2 RID: 17826
			InheritWorldSpace
		}
	}
}
