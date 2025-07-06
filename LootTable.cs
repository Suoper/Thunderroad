using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020001E4 RID: 484
	[Serializable]
	public class LootTable : LootTableBase, IContainerLoadable<LootTable>
	{
		// Token: 0x17000155 RID: 341
		// (set) Token: 0x06001599 RID: 5529 RVA: 0x000963DC File Offset: 0x000945DC
		[JsonProperty("drops")]
		protected List<LootTable.Drop> dropsSetter
		{
			set
			{
				if (this.levelledDrops == null)
				{
					this.levelledDrops = new List<LootTable.DropLevel>();
				}
				this.levelledDrops.Insert(0, new LootTable.DropLevel
				{
					lootTable = this,
					drops = value
				});
			}
		}

		// Token: 0x0600159A RID: 5530 RVA: 0x00096410 File Offset: 0x00094610
		public override CatalogData Clone()
		{
			LootTable lootTable = base.MemberwiseClone() as LootTable;
			lootTable.levelledDrops = (from item in lootTable.levelledDrops
			select item.Clone()).ToList<LootTable.DropLevel>();
			return lootTable;
		}

		// Token: 0x0600159B RID: 5531 RVA: 0x00096460 File Offset: 0x00094660
		public override void RenameItem(string itemId, string newName)
		{
			foreach (LootTable.DropLevel dropLevel in this.levelledDrops)
			{
				foreach (LootTable.Drop drop in dropLevel.drops)
				{
					if (drop.reference == LootTable.Drop.Reference.Item && drop.referenceID == this.id)
					{
						drop.referenceID = newName;
					}
				}
			}
		}

		// Token: 0x0600159C RID: 5532 RVA: 0x00096508 File Offset: 0x00094708
		public override int GetCurrentVersion()
		{
			return 1;
		}

		// Token: 0x0600159D RID: 5533 RVA: 0x0009650C File Offset: 0x0009470C
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			if (this.levelledDrops.IsNullOrEmpty())
			{
				return;
			}
			foreach (LootTable.DropLevel dropLevel in this.levelledDrops)
			{
				if (!dropLevel.drops.IsNullOrEmpty())
				{
					foreach (LootTable.Drop drop in dropLevel.drops)
					{
						drop.OnCatalogRefresh(this);
					}
				}
			}
			this.AssessTable(false);
		}

		// Token: 0x0600159E RID: 5534 RVA: 0x000965C0 File Offset: 0x000947C0
		public void AssessTable(bool getMinMax = true)
		{
			if (this.levelledDrops.IsNullOrEmpty())
			{
				return;
			}
			foreach (LootTable.DropLevel dropLevel in this.levelledDrops)
			{
				dropLevel.lootTable = this;
				dropLevel.AssessLevel(getMinMax);
			}
		}

		// Token: 0x0600159F RID: 5535 RVA: 0x00096628 File Offset: 0x00094828
		public override bool DoesLootTableContainItemID(string id, int level = -1, int depth = 0)
		{
			LootTable.<>c__DisplayClass11_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.id = id;
			CS$<>8__locals1.depth = depth;
			int depth2 = CS$<>8__locals1.depth;
			CS$<>8__locals1.depth = depth2 + 1;
			if (CS$<>8__locals1.depth > LootTable.maxPickCount)
			{
				Debug.LogError(string.Concat(new string[]
				{
					this.id,
					" | Max search depth for ID [ ",
					CS$<>8__locals1.id,
					" ] reached! ( ",
					LootTable.maxPickCount.ToString(),
					") Please check if there is any loop in the drop table..."
				}));
				return false;
			}
			if (level == -1)
			{
				int i = 0;
				for (;;)
				{
					int num = i;
					List<LootTable.DropLevel> list = this.levelledDrops;
					if (num >= ((list != null) ? list.Count : 0))
					{
						return false;
					}
					if (this.<DoesLootTableContainItemID>g__CheckLevel|11_0(i, ref CS$<>8__locals1))
					{
						break;
					}
					i++;
				}
				return true;
			}
			return this.<DoesLootTableContainItemID>g__CheckLevel|11_0(level, ref CS$<>8__locals1);
		}

		// Token: 0x060015A0 RID: 5536 RVA: 0x000966F0 File Offset: 0x000948F0
		public override ItemData PickOne(int level = 0, int searchDepth = 0, System.Random randomGen = null)
		{
			this.AssessTable(true);
			level = Math.Clamp(level, 0, this.levelledDrops.Count - 1);
			if (level < 0 || level >= this.levelledDrops.Count)
			{
				Debug.LogError("Can't pick a drop from a level that doesn't exist!");
				return null;
			}
			searchDepth++;
			if (searchDepth > LootTable.maxPickCount)
			{
				Debug.LogError(this.id + " | Max search depth reached! ( " + LootTable.maxPickCount.ToString() + ") Please check if there is any loop in the drop table...");
				return null;
			}
			LootTable.DropLevel dropLevel = this.levelledDrops[level];
			float pickedNumber = (randomGen != null) ? ((float)randomGen.NextDouble() * dropLevel.probabilityTotalWeight) : UnityEngine.Random.Range(0f, dropLevel.probabilityTotalWeight);
			foreach (LootTable.Drop drop in dropLevel.drops)
			{
				if (pickedNumber >= drop.probabilityRangeFrom && pickedNumber < drop.probabilityRangeTo)
				{
					if (string.IsNullOrEmpty(drop.referenceID))
					{
						return null;
					}
					if (drop.reference == LootTable.Drop.Reference.Item)
					{
						return drop.itemData;
					}
					if (drop.lootTable != null)
					{
						return drop.lootTable.PickOne(level, searchDepth, randomGen);
					}
					return null;
				}
			}
			return null;
		}

		// Token: 0x060015A1 RID: 5537 RVA: 0x00096834 File Offset: 0x00094A34
		public override List<ItemData> Pick(int level = 0, int searchDepth = 0, System.Random randomGen = null)
		{
			this.AssessTable(true);
			if (level < 0 || level >= this.levelledDrops.Count)
			{
				Debug.LogError("Can't pick a drop from a level that doesn't exist!");
				return null;
			}
			searchDepth++;
			if (searchDepth > LootTable.maxPickCount)
			{
				Debug.LogError(this.id + " | Max search depth reached! ( " + LootTable.maxPickCount.ToString() + ") Please check if there is any loop in the drop table...");
				return null;
			}
			LootTable.DropLevel dropLevel = this.levelledDrops[level];
			float pickedNumber = (randomGen != null) ? ((float)randomGen.NextDouble() * dropLevel.probabilityTotalWeight) : UnityEngine.Random.Range(0f, dropLevel.probabilityTotalWeight);
			List<ItemData> result = new List<ItemData>();
			foreach (LootTable.Drop drop in dropLevel.drops)
			{
				if (pickedNumber > drop.probabilityRangeFrom && pickedNumber < drop.probabilityRangeTo)
				{
					if (string.IsNullOrEmpty(drop.referenceID))
					{
						return null;
					}
					if (randomGen == null)
					{
						randomGen = new System.Random();
					}
					float rand = Mathf.Lerp(drop.minMaxRand.x, drop.minMaxRand.y, (float)randomGen.NextDouble());
					if (drop.reference == LootTable.Drop.Reference.Item)
					{
						switch (drop.randMode)
						{
						case LootTable.Drop.RandMode.ItemCount:
						{
							rand = (float)((int)Math.Round((double)rand, 0));
							int i = 0;
							while ((float)i < rand)
							{
								result.Add(drop.itemData);
								i++;
							}
							break;
						}
						case LootTable.Drop.RandMode.MoneyValue:
							while (rand >= drop.itemData.value)
							{
								result.Add(drop.itemData);
								rand -= drop.itemData.value;
							}
							break;
						case LootTable.Drop.RandMode.RewardValue:
							while (rand >= drop.itemData.rewardValue)
							{
								result.Add(drop.itemData);
								rand -= drop.itemData.rewardValue;
							}
							break;
						}
						return result;
					}
					if (drop.lootTable == null)
					{
						return null;
					}
					switch (drop.randMode)
					{
					case LootTable.Drop.RandMode.ItemCount:
					{
						rand = (float)((int)rand);
						int j = 0;
						while ((float)j < rand)
						{
							result.Add(drop.lootTable.PickOne(level, searchDepth, randomGen));
							j++;
						}
						return result;
					}
					case LootTable.Drop.RandMode.MoneyValue:
						return drop.lootTable.PickUpToMoneyValue(rand, level, searchDepth, randomGen);
					case LootTable.Drop.RandMode.RewardValue:
						return drop.lootTable.PickUpToRewardValue(rand, level, searchDepth, randomGen);
					}
				}
			}
			return null;
		}

		// Token: 0x060015A2 RID: 5538 RVA: 0x00096AD0 File Offset: 0x00094CD0
		private LootTable.DropLevel ValuePickInit(int level = 0, int searchDepth = 0, bool clamp = false)
		{
			this.AssessTable(true);
			if (clamp)
			{
				level = Mathf.Clamp(level, 0, this.levelledDrops.Count - 1);
			}
			if (level < 0 || level >= this.levelledDrops.Count)
			{
				Debug.LogError("Can't pick a drop from a level that doesn't exist!");
				return null;
			}
			searchDepth++;
			if (searchDepth > LootTable.maxPickCount)
			{
				Debug.LogError(this.id + " | Max search depth reached! ( " + LootTable.maxPickCount.ToString() + ") Please check if there is any loop in the drop table...");
				return null;
			}
			LootTable.DropLevel dropLevel = this.levelledDrops[level];
			dropLevel.drops.Shuffle<LootTable.Drop>();
			return dropLevel;
		}

		// Token: 0x060015A3 RID: 5539 RVA: 0x00096B68 File Offset: 0x00094D68
		public List<ItemData> PickUpToMoneyValue(float maxValue, int level = 0, int searchDepth = 0, System.Random randomGen = null)
		{
			LootTable.DropLevel dropLevel = this.ValuePickInit(level, searchDepth, false);
			if (dropLevel == null)
			{
				return null;
			}
			searchDepth++;
			List<ItemData> result = new List<ItemData>();
			int startIndex = randomGen.Next(0, dropLevel.drops.Count - 1);
			float valueRemaining = maxValue;
			while (valueRemaining >= dropLevel.minMoneyValue)
			{
				for (int i = 0; i < dropLevel.drops.Count; i++)
				{
					int index = (startIndex + i) % dropLevel.drops.Count;
					LootTable.Drop drop = dropLevel.drops[index];
					if (drop.reference == LootTable.Drop.Reference.Item)
					{
						if (drop.itemData.value <= valueRemaining)
						{
							result.Add(drop.itemData);
							valueRemaining -= drop.itemData.value;
						}
					}
					else
					{
						ItemData picked = drop.lootTable.PickOneUnderMoneyValue(valueRemaining, level, searchDepth, randomGen);
						if (picked != null)
						{
							result.Add(picked);
							valueRemaining -= picked.value;
						}
					}
					if (valueRemaining < dropLevel.minMoneyValue)
					{
						break;
					}
				}
				if (valueRemaining < dropLevel.minMoneyValue || dropLevel.minMoneyValue.IsApproximately(0f))
				{
					break;
				}
			}
			return result;
		}

		// Token: 0x060015A4 RID: 5540 RVA: 0x00096C80 File Offset: 0x00094E80
		public ItemData PickOneUnderMoneyValue(float value, int level = 0, int searchDepth = 0, System.Random randomGen = null)
		{
			LootTable.DropLevel dropLevel = this.ValuePickInit(level, searchDepth, true);
			if (dropLevel == null)
			{
				return null;
			}
			if (dropLevel.minMoneyValue > value)
			{
				return null;
			}
			searchDepth++;
			int startIndex = randomGen.Next(0, dropLevel.drops.Count - 1);
			for (int i = 0; i < dropLevel.drops.Count; i++)
			{
				int index = (startIndex + i) % dropLevel.drops.Count;
				LootTable.Drop drop = dropLevel.drops[index];
				if (drop.reference != LootTable.Drop.Reference.Item)
				{
					return drop.lootTable.PickOneUnderMoneyValue(value, level, searchDepth, randomGen);
				}
				if (drop.itemData.value <= value)
				{
					return drop.itemData;
				}
			}
			return null;
		}

		// Token: 0x060015A5 RID: 5541 RVA: 0x00096D2C File Offset: 0x00094F2C
		public List<ItemData> PickUpToRewardValue(float maxValue, int level = 0, int searchDepth = 0, System.Random randomGen = null)
		{
			LootTable.DropLevel dropLevel = this.ValuePickInit(level, searchDepth, false);
			if (dropLevel == null)
			{
				return null;
			}
			searchDepth++;
			List<ItemData> result = new List<ItemData>();
			int startIndex = randomGen.Next(0, dropLevel.drops.Count - 1);
			float valueRemaining = maxValue;
			while (valueRemaining >= dropLevel.minRewardValue)
			{
				for (int i = 0; i < dropLevel.drops.Count; i++)
				{
					int index = (startIndex + i) % dropLevel.drops.Count;
					LootTable.Drop drop = dropLevel.drops[index];
					if (drop.reference == LootTable.Drop.Reference.Item)
					{
						if (drop.itemData.rewardValue <= valueRemaining)
						{
							result.Add(drop.itemData);
							valueRemaining -= drop.itemData.rewardValue;
						}
					}
					else
					{
						ItemData picked = drop.lootTable.PickOneUnderRewardValue(valueRemaining, level, searchDepth, randomGen);
						if (picked != null)
						{
							result.Add(picked);
							valueRemaining -= picked.rewardValue;
						}
					}
					if (valueRemaining < dropLevel.minRewardValue)
					{
						break;
					}
				}
				if (valueRemaining < dropLevel.minRewardValue || dropLevel.minRewardValue.IsApproximately(0f))
				{
					break;
				}
			}
			return result;
		}

		// Token: 0x060015A6 RID: 5542 RVA: 0x00096E44 File Offset: 0x00095044
		public ItemData PickOneUnderRewardValue(float value, int level = 0, int searchDepth = 0, System.Random randomGen = null)
		{
			LootTable.DropLevel dropLevel = this.ValuePickInit(level, searchDepth, true);
			if (dropLevel == null)
			{
				return null;
			}
			if (dropLevel.minRewardValue > value)
			{
				return null;
			}
			searchDepth++;
			int startIndex = randomGen.Next(0, dropLevel.drops.Count - 1);
			for (int i = 0; i < dropLevel.drops.Count; i++)
			{
				int index = (startIndex + i) % dropLevel.drops.Count;
				LootTable.Drop drop = dropLevel.drops[index];
				if (drop.reference != LootTable.Drop.Reference.Item)
				{
					return drop.lootTable.PickOneUnderRewardValue(value, level, searchDepth, randomGen);
				}
				if (drop.itemData.rewardValue <= value)
				{
					return drop.itemData;
				}
			}
			return null;
		}

		// Token: 0x060015A7 RID: 5543 RVA: 0x00096EF0 File Offset: 0x000950F0
		public override List<ItemData> GetAll(int level = -1, int pickCount = 0)
		{
			LootTable.<>c__DisplayClass19_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.pickCount = pickCount;
			List<ItemData> itemDataList = new List<ItemData>();
			int pickCount2 = CS$<>8__locals1.pickCount;
			CS$<>8__locals1.pickCount = pickCount2 + 1;
			if (CS$<>8__locals1.pickCount > LootTable.maxPickCount)
			{
				Debug.LogError(this.id + " | Max search depth reached! ( " + LootTable.maxPickCount.ToString() + ") Please check if there is any loop in the drop table...");
				return itemDataList;
			}
			if (level == -1)
			{
				int i = 0;
				for (;;)
				{
					int num = i;
					List<LootTable.DropLevel> list = this.levelledDrops;
					if (num >= ((list != null) ? list.Count : 0))
					{
						break;
					}
					this.<GetAll>g__GetLevelDrops|19_0(i, itemDataList, ref CS$<>8__locals1);
					i++;
				}
			}
			else
			{
				this.<GetAll>g__GetLevelDrops|19_0(level, itemDataList, ref CS$<>8__locals1);
			}
			return itemDataList;
		}

		// Token: 0x060015A8 RID: 5544 RVA: 0x00096F8F File Offset: 0x0009518F
		public void OnLoadedFromContainer(Container container)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060015A9 RID: 5545 RVA: 0x00096F96 File Offset: 0x00095196
		public ContainerContent InstanceContent()
		{
			return new TableContent(this, null, null, 1);
		}

		// Token: 0x060015AC RID: 5548 RVA: 0x00096FC0 File Offset: 0x000951C0
		[CompilerGenerated]
		private bool <DoesLootTableContainItemID>g__CheckLevel|11_0(int level, ref LootTable.<>c__DisplayClass11_0 A_2)
		{
			if (level < 0 || this.levelledDrops.Count >= level)
			{
				Debug.LogError("Can't pick a drop from a level that doesn't exist!");
				return false;
			}
			LootTable.DropLevel dropLevel = this.levelledDrops[level];
			for (int r = 0; r < 2; r++)
			{
				for (int i = 0; i < dropLevel.drops.Count; i++)
				{
					if (dropLevel.drops[i].reference != ((r == 0) ? LootTable.Drop.Reference.Table : LootTable.Drop.Reference.Item) && ((r == 0) ? (A_2.id == dropLevel.drops[i].referenceID) : Catalog.GetData<LootTable>(dropLevel.drops[i].referenceID, true).DoesLootTableContainItemID(A_2.id, A_2.depth, 0)))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060015AD RID: 5549 RVA: 0x0009708C File Offset: 0x0009528C
		[CompilerGenerated]
		private void <GetAll>g__GetLevelDrops|19_0(int level, List<ItemData> addList, ref LootTable.<>c__DisplayClass19_0 A_3)
		{
			if (level < 0 || level >= this.levelledDrops.Count)
			{
				level = Mathf.Clamp(level, 0, this.levelledDrops.Count - 1);
			}
			foreach (LootTable.Drop drop in this.levelledDrops[level].drops)
			{
				if (!(drop.referenceID == "") && drop.referenceID != null)
				{
					if (drop.reference == LootTable.Drop.Reference.Item)
					{
						addList.Add(drop.itemData);
					}
					else if (drop.lootTable != null)
					{
						addList.AddRange(drop.lootTable.GetAll(A_3.pickCount, 0));
					}
				}
			}
		}

		// Token: 0x04001580 RID: 5504
		public List<LootTable.DropLevel> levelledDrops = new List<LootTable.DropLevel>();

		// Token: 0x04001581 RID: 5505
		private static int maxPickCount = 20;

		// Token: 0x02000823 RID: 2083
		[Serializable]
		public class DropLevel
		{
			// Token: 0x170004E9 RID: 1257
			// (get) Token: 0x06003F22 RID: 16162 RVA: 0x00185F77 File Offset: 0x00184177
			[JsonIgnore]
			public string elementName
			{
				get
				{
					return string.Format("Drop level {0}", this.dropLevel);
				}
			}

			/// <summary>
			/// Calculates the percentage and asigns the probabilities how many times
			/// the items can be picked. Function used also to validate data when tweaking numbers in editor.
			/// </summary>	
			// Token: 0x06003F23 RID: 16163 RVA: 0x00185F90 File Offset: 0x00184190
			public void AssessLevel(bool getMinMax)
			{
				if (this.drops == null || this.drops.Count == 0)
				{
					return;
				}
				this.minMoneyValue = float.PositiveInfinity;
				this.maxMoneyValue = float.NegativeInfinity;
				this.minRewardValue = float.PositiveInfinity;
				this.maxRewardValue = float.NegativeInfinity;
				float currentProbabilityWeightMaximum = 0f;
				foreach (LootTable.Drop line in this.drops)
				{
					if (line.probabilityWeight < 0f)
					{
						line.probabilityWeight = 0f;
					}
					else
					{
						line.probabilityRangeFrom = currentProbabilityWeightMaximum;
						currentProbabilityWeightMaximum += line.probabilityWeight;
						line.probabilityRangeTo = currentProbabilityWeightMaximum;
					}
					if (getMinMax && line.reference == LootTable.Drop.Reference.Item)
					{
						if (line.itemData == null)
						{
							line.itemData = Catalog.GetData<ItemData>(line.referenceID, true);
						}
						if (line.itemData != null)
						{
							this.minMoneyValue = Mathf.Min(this.minMoneyValue, line.itemData.value);
							this.maxMoneyValue = Mathf.Max(this.maxMoneyValue, line.itemData.value);
							this.minRewardValue = Mathf.Min(this.minRewardValue, line.itemData.rewardValue);
							this.maxRewardValue = Mathf.Max(this.maxRewardValue, line.itemData.rewardValue);
						}
					}
				}
				this.probabilityTotalWeight = currentProbabilityWeightMaximum;
				foreach (LootTable.Drop drop in this.drops)
				{
					drop.probabilityPercent = drop.probabilityWeight / this.probabilityTotalWeight * 100f;
				}
			}

			// Token: 0x06003F24 RID: 16164 RVA: 0x00186158 File Offset: 0x00184358
			private void AddEntryForEachInCategory()
			{
				foreach (ItemData itemData in Catalog.GetDataList<ItemData>())
				{
					if (itemData.type == this.itemCategory)
					{
						this.drops.Add(new LootTable.Drop
						{
							reference = LootTable.Drop.Reference.Item,
							referenceID = itemData.id,
							probabilityWeight = 1f
						});
					}
				}
			}

			// Token: 0x06003F25 RID: 16165 RVA: 0x001861E0 File Offset: 0x001843E0
			public LootTable.DropLevel Clone()
			{
				LootTable.DropLevel dropLevel = new LootTable.DropLevel();
				dropLevel.drops = (from item in dropLevel.drops
				select item.Clone()).ToList<LootTable.Drop>();
				return dropLevel;
			}

			// Token: 0x0400408A RID: 16522
			[JsonMergeKey]
			public int dropLevel = -1;

			// Token: 0x0400408B RID: 16523
			[NonSerialized]
			public ItemData.Type itemCategory;

			// Token: 0x0400408C RID: 16524
			[Space]
			public List<LootTable.Drop> drops;

			// Token: 0x0400408D RID: 16525
			[NonSerialized]
			public LootTable lootTable;

			// Token: 0x0400408E RID: 16526
			[NonSerialized]
			public float probabilityTotalWeight;

			// Token: 0x0400408F RID: 16527
			[NonSerialized]
			public float minMoneyValue;

			// Token: 0x04004090 RID: 16528
			[NonSerialized]
			public float maxMoneyValue;

			// Token: 0x04004091 RID: 16529
			[NonSerialized]
			public float minRewardValue;

			// Token: 0x04004092 RID: 16530
			[NonSerialized]
			public float maxRewardValue;
		}

		// Token: 0x02000824 RID: 2084
		[Serializable]
		public class Drop
		{
			// Token: 0x06003F27 RID: 16167 RVA: 0x0018622B File Offset: 0x0018442B
			public List<ValueDropdownItem<string>> GetAllItemOrLootTableID()
			{
				if (this.reference == LootTable.Drop.Reference.Item)
				{
					return Catalog.GetDropdownAllID(Category.Item, "None");
				}
				return Catalog.GetDropdownAllID(Category.LootTable, "None");
			}

			// Token: 0x06003F28 RID: 16168 RVA: 0x0018624C File Offset: 0x0018444C
			public void OnCatalogRefresh(LootTable sourceLootTable)
			{
				if (string.IsNullOrEmpty(this.referenceID))
				{
					return;
				}
				LootTable.Drop.Reference reference = this.reference;
				if (reference != LootTable.Drop.Reference.Item)
				{
					if (reference != LootTable.Drop.Reference.Table)
					{
						return;
					}
					if (!Catalog.TryGetData<LootTable>(this.referenceID, out this.lootTable, true))
					{
						Debug.LogWarning("LootTable " + sourceLootTable.id + " cannot find LootTable " + this.referenceID);
					}
				}
				else if (!Catalog.TryGetData<ItemData>(this.referenceID, out this.itemData, true))
				{
					Debug.LogError("Loot table: " + sourceLootTable.id + " cannot find Item:" + this.referenceID);
					return;
				}
			}

			// Token: 0x06003F29 RID: 16169 RVA: 0x001862DF File Offset: 0x001844DF
			public LootTable.Drop Clone()
			{
				return base.MemberwiseClone() as LootTable.Drop;
			}

			// Token: 0x04004093 RID: 16531
			[JsonMergeKey]
			public string referenceID;

			// Token: 0x04004094 RID: 16532
			public LootTable.Drop.Reference reference;

			// Token: 0x04004095 RID: 16533
			[NonSerialized]
			public ItemData itemData;

			// Token: 0x04004096 RID: 16534
			[NonSerialized]
			public LootTable lootTable;

			// Token: 0x04004097 RID: 16535
			public LootTable.Drop.RandMode randMode;

			// Token: 0x04004098 RID: 16536
			public Vector2 minMaxRand = Vector2.one;

			// Token: 0x04004099 RID: 16537
			public float probabilityWeight;

			// Token: 0x0400409A RID: 16538
			[NonSerialized]
			public float probabilityPercent;

			// Token: 0x0400409B RID: 16539
			[NonSerialized]
			public float probabilityRangeFrom;

			// Token: 0x0400409C RID: 16540
			[NonSerialized]
			public float probabilityRangeTo;

			// Token: 0x02000BD1 RID: 3025
			public enum RandMode
			{
				// Token: 0x04004CF1 RID: 19697
				ItemCount,
				// Token: 0x04004CF2 RID: 19698
				MoneyValue,
				// Token: 0x04004CF3 RID: 19699
				RewardValue
			}

			// Token: 0x02000BD2 RID: 3026
			public enum Reference
			{
				// Token: 0x04004CF5 RID: 19701
				Item,
				// Token: 0x04004CF6 RID: 19702
				Table
			}
		}
	}
}
