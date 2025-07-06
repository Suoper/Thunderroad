using System;
using System.Collections.Generic;
using ThunderRoad.Modules;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002E2 RID: 738
	public class LoreSpawner : MonoBehaviour
	{
		// Token: 0x1700022D RID: 557
		// (get) Token: 0x0600236E RID: 9070 RVA: 0x000F200D File Offset: 0x000F020D
		public bool IsPopulated
		{
			get
			{
				return this.isPopulated;
			}
		}

		// Token: 0x0600236F RID: 9071 RVA: 0x000F2015 File Offset: 0x000F0215
		private void Awake()
		{
			this.Init();
			EventManager.OnLevelInstanceLoaded += this.HandleInstanceLoaded;
			EventManager.onLevelLoad += this.HandleLevelLoaded;
		}

		// Token: 0x06002370 RID: 9072 RVA: 0x000F203F File Offset: 0x000F023F
		private void Start()
		{
			this.RegisterToLoreArea();
		}

		// Token: 0x06002371 RID: 9073 RVA: 0x000F2048 File Offset: 0x000F0248
		private void Update()
		{
			if (!this.ManualCulling || Player.local == null)
			{
				return;
			}
			if (UpdateManager.frameCount % 60 != 0)
			{
				return;
			}
			for (int i = 0; i < this.loreItems.Count; i++)
			{
				Item item = this.loreItems[i];
				if (Vector3.Distance(item.transform.position, Player.local.transform.position) > this.CullingDistance)
				{
					item.gameObject.SetActive(false);
				}
				else
				{
					item.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x06002372 RID: 9074 RVA: 0x000F20DC File Offset: 0x000F02DC
		public void Init()
		{
			if (this.initialized)
			{
				return;
			}
			if (GameModeManager.instance == null || GameModeManager.instance.currentGameMode == null || !GameModeManager.instance.currentGameMode.TryGetModule<LoreModule>(out this.loreModule))
			{
				return;
			}
			if (this.loreConditionRequiredParameters == null)
			{
				this.loreConditionRequiredParameters = new List<string>();
			}
			if (this.loreConditionOptionalParameters == null)
			{
				this.loreConditionOptionalParameters = new List<string>();
			}
			this.LoreFoundSfxID = this.loreModule.LoreFoundSfxID;
			this.loreFoundEffectData = Catalog.GetData<EffectData>(this.LoreFoundSfxID, true);
			if (!this.isGroupLoreSpawn)
			{
				this.spawnPoints.Clear();
				this.spawnPoints.Add(base.transform);
			}
			if (!this.ForceEnable)
			{
				this.RollToDisableLoreSpawner(this.loreModule.loreSpawnProbabiltyWeight, this.loreModule.VisibilityProbabilityWeights[this.visibility]);
			}
			this.initialized = true;
		}

		// Token: 0x06002373 RID: 9075 RVA: 0x000F21C8 File Offset: 0x000F03C8
		public void RollToDisableLoreSpawner(float globalProbabilityWeight, float visibilityProbabilityWeight)
		{
			float num = UnityEngine.Random.Range(0f, 1f);
			float spawnChance = globalProbabilityWeight * visibilityProbabilityWeight;
			if (num > spawnChance)
			{
				base.enabled = false;
			}
		}

		// Token: 0x06002374 RID: 9076 RVA: 0x000F21F2 File Offset: 0x000F03F2
		public void HandleInstanceLoaded()
		{
			this.BuildLoreManagerAndPopulate();
		}

		// Token: 0x06002375 RID: 9077 RVA: 0x000F21FA File Offset: 0x000F03FA
		public void HandleLevelLoaded(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.BuildLoreManagerAndPopulate();
			}
		}

		// Token: 0x06002376 RID: 9078 RVA: 0x000F2206 File Offset: 0x000F0406
		private void OnDestroy()
		{
			EventManager.OnLevelInstanceLoaded -= this.HandleInstanceLoaded;
			EventManager.onLevelLoad -= this.HandleLevelLoaded;
			this.Clear();
		}

		// Token: 0x06002377 RID: 9079 RVA: 0x000F2230 File Offset: 0x000F0430
		public void BuildLoreManagerAndPopulate()
		{
			if (!Level.IsDungeon && !LoreManager.instance)
			{
				LoreManager loreManager = Level.current.gameObject.AddComponent<LoreManager>();
				loreManager.loreAreas.Add(this.loreArea);
				loreManager.PopulateLoreAreas();
			}
		}

		// Token: 0x06002378 RID: 9080 RVA: 0x000F226C File Offset: 0x000F046C
		public void RegisterToLoreArea()
		{
			Level level = Level.current;
			if (level == null)
			{
				Debug.LogError("LoreSpawner unable to get current level, unable to register to lore area.");
				return;
			}
			if (!level.TryGetComponent<LoreArea>(out this.loreArea))
			{
				this.loreArea = level.gameObject.AddComponent<LoreArea>();
			}
			this.loreArea.loreSpawners.Add(this);
		}

		// Token: 0x06002379 RID: 9081 RVA: 0x000F22C4 File Offset: 0x000F04C4
		private void OnItemSpawn(Item item, int index, Action<Item> callback)
		{
			if (item == null)
			{
				Debug.LogError("LoreSpawner : " + base.name + " failed to spawn item");
				return;
			}
			item.gameObject.name = "Note - " + this.loreData[index].groupId + " / " + this.loreData[index].loreId;
			item.DisallowDespawn = true;
			ILoreDisplay loreDisplay = item.transform.GetComponentInChildren<ILoreDisplay>();
			this.loreDisplayers.Add(loreDisplay);
			this.loreItems.Add(item);
			item.OnDespawnEvent += this.onItemDespawn;
			if (this.lorePack.spawnPackAsOneItem)
			{
				loreDisplay.SetMultipleLore(this.loreModule, this, this.loreData);
			}
			else
			{
				loreDisplay.SetLore(this.loreModule, this, this.loreData[index]);
			}
			if (callback != null)
			{
				callback(item);
			}
		}

		// Token: 0x0600237A RID: 9082 RVA: 0x000F23B4 File Offset: 0x000F05B4
		public void PopulateLore()
		{
			if (!this.initialized)
			{
				if (this.loreModule != null)
				{
					string str = "Cannot populate lore: ";
					string name = base.name;
					string str2 = " area : ";
					LoreArea loreArea = this.loreArea;
					Debug.LogError(str + name + str2 + ((loreArea != null) ? loreArea.ToString() : null));
				}
				return;
			}
			if (this.isPopulated)
			{
				return;
			}
			if (this.loreModule == null)
			{
				Debug.LogError("Cannot populate lore: " + base.name + " - Lore Module not found");
				return;
			}
			List<LoreScriptableObject.LoreData> data;
			if ((this.useSpecificLorePack && this.loreModule.TryPickSpecificLore(this.specificLorePackName, out this.lorePackId, out data, out this.lorePack)) || (!this.useSpecificLorePack && this.loreModule.TryGetLorePack(this.spawnPoints.Count, this.visibility, this.loreConditionRequiredParameters.ToArray(), this.loreConditionOptionalParameters.ToArray(), out this.lorePackId, out data, out this.lorePack)))
			{
				this.loreData = data;
				if (!this.loreData.IsNullOrEmpty())
				{
					Debug.Log(string.Format("loreSpawner {0} populated with lore id {1}, {2}, and lorePackID {3}", new object[]
					{
						base.gameObject.name,
						this.loreData[0].loreId,
						this.loreData[0].titleId,
						this.lorePackId
					}));
				}
				else
				{
					Debug.Log("LoreSpawner: " + base.gameObject.name + " had null or empty loreData");
				}
				if (this.parentSpawner != null)
				{
					this.parentSpawner.onSpawnEvent.AddListener(new UnityAction<Item>(this.ParentSpawned));
				}
				else if (this.SpawnOnStart)
				{
					this.Spawn(null, null);
				}
				this.isPopulated = true;
				return;
			}
			if (this.DEBUG)
			{
				string requiredParam = "";
				if (!this.loreConditionRequiredParameters.IsNullOrEmpty())
				{
					requiredParam = this.loreConditionRequiredParameters[0];
				}
				Debug.LogError(string.Concat(new string[]
				{
					"lore module did not return lore, you are likely out of lore under the current conditions.\n",
					string.Format("Visbility is {0}\n", this.visibility),
					"Required Param is ",
					requiredParam,
					" \nEnemy Config is ",
					Level.current.options["EnemyConfig"],
					"\nCH progression level is ",
					Level.current.options["CrystalHuntLevelProgression"]
				}));
			}
		}

		// Token: 0x0600237B RID: 9083 RVA: 0x000F2624 File Offset: 0x000F0824
		public void Spawn(float delay, Vector3? position = null, Action<Item> callback = null)
		{
			LoreSpawner.<>c__DisplayClass40_0 CS$<>8__locals1 = new LoreSpawner.<>c__DisplayClass40_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.callback = callback;
			base.StartCoroutine(CS$<>8__locals1.<Spawn>g__SpawnCoroutine|0(delay, position));
		}

		// Token: 0x0600237C RID: 9084 RVA: 0x000F2654 File Offset: 0x000F0854
		public void Spawn(Vector3? position = null, Action<Item> callback = null)
		{
			if (this.spawnPoints == null)
			{
				Debug.LogError("LoreSpawner : " + base.name + " has no spawn point set");
				return;
			}
			if (this.loreData == null)
			{
				if (!this.loreModule.HasLoreBeenRead(this.lorePackId))
				{
					Debug.LogError("LoreSpawner : " + base.name + string.Format(" has no lore datas set, pack id is {0}, has been read is {1}", this.lorePackId, this.loreModule.HasLoreBeenRead(this.lorePackId)));
				}
				return;
			}
			if (!this.loreData.IsNullOrEmpty())
			{
				Debug.Log(string.Format("LoreSpawner: {0} spawning lore with id {1}, and lorePackID {2}", base.gameObject.name, this.loreData[0].loreId, this.lorePackId));
			}
			else
			{
				Debug.Log("LoreSpawner: " + base.gameObject.name + " had null or empty loreData");
			}
			if (this.lorePack.spawnPackAsOneItem)
			{
				ItemData itemData;
				if (Catalog.TryGetData<ItemData>(this.loreData[0].itemId, out itemData, true))
				{
					if (position != null)
					{
						itemData.SpawnAsync(delegate(Item item)
						{
							this.OnItemSpawn(item, 0, callback);
						}, position, new Quaternion?(this.spawnPoints[0].rotation), null, true, null, Item.Owner.None);
						return;
					}
					itemData.SpawnAsync(delegate(Item item)
					{
						this.OnItemSpawn(item, 0, callback);
					}, new Vector3?(this.spawnPoints[0].position), new Quaternion?(this.spawnPoints[0].rotation), null, true, null, Item.Owner.None);
					return;
				}
			}
			else
			{
				for (int i = this.loreData.Count - 1; i >= 0; i--)
				{
					if (this.spawnPoints[i] == null)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"LoreSpawner : ",
							base.name,
							" spawn point ",
							i.ToString(),
							" is null"
						}));
						return;
					}
					ItemData itemData2;
					if (Catalog.TryGetData<ItemData>(this.loreData[i].itemId, out itemData2, true))
					{
						int index = i;
						if (position != null)
						{
							itemData2.SpawnAsync(delegate(Item item)
							{
								this.OnItemSpawn(item, index, callback);
							}, position, new Quaternion?(this.spawnPoints[i].rotation), null, true, null, Item.Owner.None);
						}
						else
						{
							itemData2.SpawnAsync(delegate(Item item)
							{
								this.OnItemSpawn(item, index, callback);
							}, new Vector3?(this.spawnPoints[i].position), new Quaternion?(this.spawnPoints[i].rotation), null, true, null, Item.Owner.None);
						}
					}
				}
			}
		}

		// Token: 0x0600237D RID: 9085 RVA: 0x000F2920 File Offset: 0x000F0B20
		public void ParentSpawned(Item item)
		{
			this.parentSpawner.onSpawnEvent.RemoveListener(new UnityAction<Item>(this.ParentSpawned));
			this.Spawn(null, null);
		}

		// Token: 0x0600237E RID: 9086 RVA: 0x000F295C File Offset: 0x000F0B5C
		public void TestSpawn()
		{
			if (this.loreData.IsNullOrEmpty())
			{
				Debug.Log("TestSpawn() lore datas is null, make sure you are on a CH character!");
				return;
			}
			if (this.loreData.Count > 0)
			{
				this.hasRead = false;
				this.MarkAsRead(true);
				this.TestDespawn();
			}
			this.PopulateLore();
		}

		// Token: 0x0600237F RID: 9087 RVA: 0x000F29A9 File Offset: 0x000F0BA9
		public void Debug_SetVisible()
		{
			Debug.Log("Setting test spawner visibility to Very Visibile");
			this.visibility = LorePackCondition.Visibility.VeryVisibile;
		}

		// Token: 0x06002380 RID: 9088 RVA: 0x000F29BC File Offset: 0x000F0BBC
		public void Debug_SetHidden()
		{
			Debug.Log("Setting test spawner visibility to Hidden");
			this.visibility = LorePackCondition.Visibility.Hidden;
		}

		// Token: 0x06002381 RID: 9089 RVA: 0x000F29CF File Offset: 0x000F0BCF
		public void Debug_SetEnemyConfig(string s)
		{
			Level.current.options["EnemyConfig"] = s;
		}

		// Token: 0x06002382 RID: 9090 RVA: 0x000F29E8 File Offset: 0x000F0BE8
		public void TestDespawn()
		{
			if (this.loreData.Count > 0)
			{
				this.isPopulated = true;
				this.ReleaseLore(true);
			}
			for (int i = 0; i < this.loreItems.Count; i++)
			{
				UnityEngine.Object.Destroy(this.loreItems[i].gameObject);
			}
			this.loreData.Clear();
		}

		// Token: 0x06002383 RID: 9091 RVA: 0x000F2A48 File Offset: 0x000F0C48
		public void ReleaseLore(bool forceRelease = false)
		{
			if (this.isPopulated && (forceRelease || !this.hasRead))
			{
				this.loreModule.ReleaseLore(this.lorePackId, this.hasRead);
				for (int i = 0; i < this.loreDisplayers.Count; i++)
				{
					ILoreDisplay loreDisplayer = this.loreDisplayers[i];
					Item item = this.loreItems[i];
					if (loreDisplayer != null && item != null)
					{
						loreDisplayer.ReleaseLore();
					}
				}
				this.isPopulated = false;
			}
		}

		// Token: 0x06002384 RID: 9092 RVA: 0x000F2AC8 File Offset: 0x000F0CC8
		private void Clear()
		{
			this.ReleaseLore(true);
			for (int i = 0; i < this.loreDisplayers.Count; i++)
			{
				Item item = this.loreItems[i];
				if (item != null)
				{
					item.OnDespawnEvent -= this.onItemDespawn;
					item.Despawn();
				}
			}
			this.loreItems.Clear();
			this.loreDisplayers.Clear();
		}

		// Token: 0x06002385 RID: 9093 RVA: 0x000F2B36 File Offset: 0x000F0D36
		private void onItemDespawn(EventTime eventTime)
		{
			this.Clear();
		}

		// Token: 0x06002386 RID: 9094 RVA: 0x000F2B40 File Offset: 0x000F0D40
		[Tooltip("WARNING: this requires your lore state be reset. All lore will progress will be lost if used.")]
		public void EditorSpawn()
		{
			this.Clear();
			this.loreModule.Debug_MarkAllLoreUnread();
			this.PopulateLore();
			if (!this.SpawnOnStart)
			{
				this.Spawn(null, null);
			}
		}

		// Token: 0x06002387 RID: 9095 RVA: 0x000F2B7C File Offset: 0x000F0D7C
		public void EditorClearLore()
		{
			this.Clear();
		}

		// Token: 0x06002388 RID: 9096 RVA: 0x000F2B84 File Offset: 0x000F0D84
		public void MarkAsRead(bool playEffect)
		{
			if (!this.hasRead)
			{
				if (playEffect)
				{
					this.loreFoundEffectInstance = this.loreFoundEffectData.Spawn(base.transform, true, null, false);
					this.loreFoundEffectInstance.Play(0, false, false);
				}
				this.hasRead = true;
				this.loreModule.ReleaseLore(this.lorePackId, this.hasRead);
				if (!this.loreData.IsNullOrEmpty())
				{
					Debug.Log(string.Concat(new string[]
					{
						"Marking lore pack ",
						this.loreData[0].loreId,
						" ",
						this.loreData[0].titleId,
						" as read"
					}));
				}
			}
		}

		// Token: 0x04002271 RID: 8817
		public LorePackCondition.Visibility visibility;

		// Token: 0x04002272 RID: 8818
		public List<string> loreConditionRequiredParameters;

		// Token: 0x04002273 RID: 8819
		public List<string> loreConditionOptionalParameters;

		// Token: 0x04002274 RID: 8820
		public bool isGroupLoreSpawn;

		// Token: 0x04002275 RID: 8821
		public List<Transform> spawnPoints = new List<Transform>();

		// Token: 0x04002276 RID: 8822
		public List<ILoreDisplay> loreDisplayers = new List<ILoreDisplay>();

		// Token: 0x04002277 RID: 8823
		public List<Item> loreItems = new List<Item>();

		// Token: 0x04002278 RID: 8824
		public bool ForceEnable;

		// Token: 0x04002279 RID: 8825
		public bool SpawnOnStart = true;

		// Token: 0x0400227A RID: 8826
		public bool ManualCulling;

		// Token: 0x0400227B RID: 8827
		public float CullingDistance = 50f;

		// Token: 0x0400227C RID: 8828
		public bool useSpecificLorePack;

		// Token: 0x0400227D RID: 8829
		[Tooltip("The lore pack name starts with the local path of the pack folder name")]
		public string specificLorePackName;

		// Token: 0x0400227E RID: 8830
		public ItemSpawner parentSpawner;

		// Token: 0x0400227F RID: 8831
		public bool DEBUG;

		// Token: 0x04002280 RID: 8832
		public LoreArea loreArea;

		// Token: 0x04002281 RID: 8833
		private LoreModule loreModule;

		// Token: 0x04002282 RID: 8834
		private int lorePackId = -1;

		// Token: 0x04002283 RID: 8835
		private bool hasRead;

		// Token: 0x04002284 RID: 8836
		private bool isPopulated;

		// Token: 0x04002285 RID: 8837
		private List<LoreScriptableObject.LoreData> loreData;

		// Token: 0x04002286 RID: 8838
		private LoreScriptableObject.LorePack lorePack;

		// Token: 0x04002287 RID: 8839
		private EffectData loreFoundEffectData;

		// Token: 0x04002288 RID: 8840
		private EffectInstance loreFoundEffectInstance;

		// Token: 0x04002289 RID: 8841
		private string LoreFoundSfxID = "LoreFound";

		// Token: 0x0400228A RID: 8842
		private bool initialized;
	}
}
