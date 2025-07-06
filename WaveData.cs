using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200024A RID: 586
	[Serializable]
	public class WaveData : CatalogData
	{
		// Token: 0x1700017F RID: 383
		// (set) Token: 0x0600188F RID: 6287 RVA: 0x000A249E File Offset: 0x000A069E
		[JsonProperty("loop")]
		private bool loopBehaviorSetter
		{
			set
			{
				if (value)
				{
					this.loopBehavior = WaveData.LoopBehavior.LoopSeamless;
					return;
				}
				this.loopBehavior = WaveData.LoopBehavior.NoLoop;
			}
		}

		// Token: 0x17000180 RID: 384
		// (set) Token: 0x06001890 RID: 6288 RVA: 0x000A24B2 File Offset: 0x000A06B2
		[JsonProperty("maxAlive")]
		private int maxAliveAlternateSetter
		{
			set
			{
				this.totalMaxAlive = value;
			}
		}

		// Token: 0x06001891 RID: 6289 RVA: 0x000A24BB File Offset: 0x000A06BB
		public override int GetCurrentVersion()
		{
			return 2;
		}

		// Token: 0x06001892 RID: 6290 RVA: 0x000A24C0 File Offset: 0x000A06C0
		private void OnBeginCheckGroup(int index)
		{
			this.groups[index].index = index;
			if (string.IsNullOrEmpty(this.groups[index].referenceID))
			{
				GUI.color = Color.red;
				return;
			}
			WaveData.Group.Reference reference = this.groups[index].reference;
			if (reference != WaveData.Group.Reference.Creature)
			{
				if (reference != WaveData.Group.Reference.Table)
				{
					return;
				}
				if (Catalog.GetData<CreatureTable>(this.groups[index].referenceID, false) != null)
				{
					return;
				}
			}
			else if (Catalog.GetData<CreatureData>(this.groups[index].referenceID, false) != null)
			{
				return;
			}
			GUI.color = Color.red;
		}

		// Token: 0x06001893 RID: 6291 RVA: 0x000A2559 File Offset: 0x000A0759
		private void OnEndCheckGroup(int index)
		{
			GUI.color = Color.white;
		}

		// Token: 0x06001894 RID: 6292 RVA: 0x000A2568 File Offset: 0x000A0768
		public override CatalogData Clone()
		{
			WaveData waveData = base.MemberwiseClone() as WaveData;
			waveData.groups = (from item in waveData.groups
			select item.Clone()).ToList<WaveData.Group>();
			return waveData;
		}

		// Token: 0x06001895 RID: 6293 RVA: 0x000A25B5 File Offset: 0x000A07B5
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			this.RefreshInfo();
		}

		// Token: 0x06001896 RID: 6294 RVA: 0x000A25C4 File Offset: 0x000A07C4
		private void RefreshInfo()
		{
			this.minTotalCount = 0;
			this.maxTotalCount = 0f;
			Dictionary<int, WaveData.WaveFaction> foundFactions = new Dictionary<int, WaveData.WaveFaction>();
			foreach (WaveData.WaveFaction faction in this.factions)
			{
				faction.factionName = Catalog.gameData.GetFaction(faction.factionID).name;
				if (!foundFactions.TryAdd(faction.factionID, faction))
				{
					Debug.LogError(string.Format("Duplicate faction {0} in wave {1}", faction.factionID, this.id));
				}
			}
			List<int> factionsInWave = new List<int>();
			List<WaveData.WaveFaction> newFactions = new List<WaveData.WaveFaction>();
			if (this.groups != null)
			{
				for (int i = 0; i < this.groups.Count; i++)
				{
					this.groups[i].waveData = this;
					this.minTotalCount += this.groups[i].minMaxCount.x;
					this.maxTotalCount += (float)this.groups[i].minMaxCount.y;
					this.groups[i].OnCatalogRefresh();
					this.groups[i].GetFactions(factionsInWave);
				}
			}
			factionsInWave.Sort();
			if (factionsInWave.Count > 0)
			{
				foreach (int factionID in factionsInWave)
				{
					WaveData.WaveFaction existingFaction;
					if (foundFactions.TryGetValue(factionID, out existingFaction))
					{
						newFactions.Add(existingFaction);
						foundFactions.Remove(factionID);
					}
					else
					{
						newFactions.Add(new WaveData.WaveFaction(Catalog.gameData.GetFaction(factionID), Mathf.RoundToInt((float)(this.totalMaxAlive / factionsInWave.Count))));
					}
				}
			}
			this.factions = newFactions;
		}

		// Token: 0x06001897 RID: 6295 RVA: 0x000A27C8 File Offset: 0x000A09C8
		public int GetMaxAlive()
		{
			return Mathf.Min(this.totalMaxAlive, Catalog.gameData.platformParameters.maxWaveAlive);
		}

		// Token: 0x040017A1 RID: 6049
		public string category = "Misc";

		// Token: 0x040017A2 RID: 6050
		public string localizationId;

		// Token: 0x040017A3 RID: 6051
		public string title = "Unknown";

		// Token: 0x040017A4 RID: 6052
		[Multiline]
		public string description;

		// Token: 0x040017A5 RID: 6053
		public WaveData.LoopBehavior loopBehavior;

		// Token: 0x040017A6 RID: 6054
		public int totalMaxAlive = 5;

		// Token: 0x040017A7 RID: 6055
		public bool alwaysAvailable;

		// Token: 0x040017A8 RID: 6056
		public List<string> waveSelectors;

		// Token: 0x040017A9 RID: 6057
		[NonSerialized]
		public int minTotalCount;

		// Token: 0x040017AA RID: 6058
		[NonSerialized]
		public float maxTotalCount;

		// Token: 0x040017AB RID: 6059
		public List<WaveData.WaveFaction> factions = new List<WaveData.WaveFaction>();

		// Token: 0x040017AC RID: 6060
		public List<WaveData.Group> groups;

		// Token: 0x02000863 RID: 2147
		public enum LoopBehavior
		{
			// Token: 0x04004177 RID: 16759
			NoLoop,
			// Token: 0x04004178 RID: 16760
			LoopSeamless,
			// Token: 0x04004179 RID: 16761
			Loop
		}

		// Token: 0x02000864 RID: 2148
		[Serializable]
		public class WaveFaction
		{
			// Token: 0x06004007 RID: 16391 RVA: 0x00188999 File Offset: 0x00186B99
			public WaveFaction()
			{
			}

			// Token: 0x06004008 RID: 16392 RVA: 0x001889B3 File Offset: 0x00186BB3
			public WaveFaction(GameData.Faction faction, int factionDefaultMax)
			{
				this.factionID = faction.id;
				this.factionName = faction.name;
				this.factionMaxAlive = factionDefaultMax;
			}

			// Token: 0x0400417A RID: 16762
			public int factionID;

			// Token: 0x0400417B RID: 16763
			[NonSerialized]
			public string factionName;

			// Token: 0x0400417C RID: 16764
			public float factionHealthMultiplier = 1f;

			// Token: 0x0400417D RID: 16765
			public int factionMaxAlive = 5;
		}

		// Token: 0x02000865 RID: 2149
		public class SpawnData
		{
			// Token: 0x06004009 RID: 16393 RVA: 0x001889EC File Offset: 0x00186BEC
			public SpawnData(CreatureData d, WaveData.Group g)
			{
				this.data = d;
				this.spawnGroup = g;
			}

			// Token: 0x0400417E RID: 16766
			public CreatureData data;

			// Token: 0x0400417F RID: 16767
			public WaveData.Group spawnGroup;
		}

		// Token: 0x02000866 RID: 2150
		[Serializable]
		public class Group
		{
			// Token: 0x1700050F RID: 1295
			// (set) Token: 0x0600400A RID: 16394 RVA: 0x00188A02 File Offset: 0x00186C02
			[JsonProperty("creatureID")]
			private string creatureIDAlternateSetter
			{
				set
				{
					this.creatureID = value;
				}
			}

			// Token: 0x17000510 RID: 1296
			// (set) Token: 0x0600400B RID: 16395 RVA: 0x00188A0B File Offset: 0x00186C0B
			[JsonProperty("creatureTableID")]
			private string creatureTableIDAlternateSetter
			{
				set
				{
					this.creatureTableID = value;
				}
			}

			// Token: 0x0600400C RID: 16396 RVA: 0x00188A14 File Offset: 0x00186C14
			private bool IsSet()
			{
				return this.set;
			}

			// Token: 0x17000511 RID: 1297
			// (set) Token: 0x0600400D RID: 16397 RVA: 0x00188A1C File Offset: 0x00186C1C
			[JsonProperty("conditionStepIndex")]
			private int conditionAlternateSetter
			{
				set
				{
					this.prereqGroupIndex = value;
				}
			}

			// Token: 0x17000512 RID: 1298
			// (set) Token: 0x0600400E RID: 16398 RVA: 0x00188A25 File Offset: 0x00186C25
			[JsonProperty("conditionThreshold")]
			private int conditionThreshAlternateSetter
			{
				set
				{
					this.prereqMaxRemainingAlive = value;
				}
			}

			// Token: 0x0600400F RID: 16399 RVA: 0x00188A2E File Offset: 0x00186C2E
			public WaveData.Group Clone()
			{
				return base.MemberwiseClone() as WaveData.Group;
			}

			// Token: 0x06004010 RID: 16400 RVA: 0x00188A3C File Offset: 0x00186C3C
			public int GetMaxThreshold()
			{
				if (this.waveData != null)
				{
					WaveData.Group conditionStep = this.waveData.groups.ElementAtOrDefault(this.prereqGroupIndex);
					if (conditionStep != null)
					{
						return conditionStep.minMaxCount.x - 1;
					}
				}
				return 4;
			}

			// Token: 0x06004011 RID: 16401 RVA: 0x00188A7A File Offset: 0x00186C7A
			public void SetRandSpawn()
			{
				if (!this.set)
				{
					this.spawnPointIndex = -1;
					return;
				}
				if (this.spawnPointIndex == -1)
				{
					this.spawnPointIndex = 0;
				}
			}

			// Token: 0x06004012 RID: 16402 RVA: 0x00188A9C File Offset: 0x00186C9C
			private void RefreshInfo()
			{
				WaveData waveData = this.waveData;
				if (waveData == null)
				{
					return;
				}
				waveData.RefreshInfo();
			}

			// Token: 0x06004013 RID: 16403 RVA: 0x00188AB0 File Offset: 0x00186CB0
			public void GetFactions(List<int> existing)
			{
				if (this.overrideFaction)
				{
					if (!existing.Contains(this.factionID))
					{
						existing.Add(this.factionID);
						return;
					}
				}
				else
				{
					if (this.referenceID == "None")
					{
						return;
					}
					if (this.reference == WaveData.Group.Reference.Creature)
					{
						CreatureData creatureData = Catalog.GetData<CreatureData>(this.referenceID, true);
						if (!existing.Contains(creatureData.factionId))
						{
							existing.Add(creatureData.factionId);
							return;
						}
					}
					else
					{
						CreatureTable creatureTable;
						if (Catalog.TryGetData<CreatureTable>(this.referenceID, out creatureTable, true))
						{
							using (List<int>.Enumerator enumerator = creatureTable.GetFactionIDs().GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									int factionID = enumerator.Current;
									if (!existing.Contains(factionID))
									{
										existing.Add(factionID);
									}
								}
								return;
							}
						}
						Debug.LogError("Could not find creature table " + this.referenceID + " for wave " + this.waveData.id);
					}
				}
			}

			// Token: 0x06004014 RID: 16404 RVA: 0x00188BB0 File Offset: 0x00186DB0
			public List<ValueDropdownItem<string>> GetAllCreatureOrTableID()
			{
				return Catalog.GetDropdownAllID((this.reference == WaveData.Group.Reference.Creature) ? Category.Creature : Category.CreatureTable, "None");
			}

			// Token: 0x06004015 RID: 16405 RVA: 0x00188BC8 File Offset: 0x00186DC8
			public List<ValueDropdownItem<string>> GetAllContainerID()
			{
				return Catalog.GetDropdownAllID(Category.Container, "None");
			}

			// Token: 0x06004016 RID: 16406 RVA: 0x00188BD5 File Offset: 0x00186DD5
			public List<ValueDropdownItem<string>> GetAllBrainID()
			{
				return Catalog.GetDropdownAllID(Category.Brain, "None");
			}

			// Token: 0x06004017 RID: 16407 RVA: 0x00188BE2 File Offset: 0x00186DE2
			public List<ValueDropdownItem<int>> GetAllFactionID()
			{
				return Catalog.gameData.GetFactions();
			}

			// Token: 0x06004018 RID: 16408 RVA: 0x00188BEE File Offset: 0x00186DEE
			public void OnCatalogRefresh()
			{
				if (string.IsNullOrEmpty(this.referenceID))
				{
					if (this.reference == WaveData.Group.Reference.Creature)
					{
						this.referenceID = this.creatureID;
					}
					if (this.reference == WaveData.Group.Reference.Table)
					{
						this.referenceID = this.creatureTableID;
					}
				}
			}

			// Token: 0x06004019 RID: 16409 RVA: 0x00188C26 File Offset: 0x00186E26
			public void FindPrerequisiteGroup()
			{
				if (this.prereqGroupIndex == -1)
				{
					this.prereqGroup = null;
				}
				this.prereqGroup = this.waveData.groups.ElementAtOrDefault(this.prereqGroupIndex);
			}

			// Token: 0x0600401A RID: 16410 RVA: 0x00188C54 File Offset: 0x00186E54
			public IEnumerator GetCreatures(List<WaveData.SpawnData> toFill)
			{
				float count = 1f;
				if (this.minMaxCount.x == this.minMaxCount.y)
				{
					count = (float)this.minMaxCount.x;
				}
				if (this.minMaxCount.x > 0 && this.minMaxCount.x < this.minMaxCount.y)
				{
					count = (float)UnityEngine.Random.Range(this.minMaxCount.x, this.minMaxCount.y + 1);
				}
				CreatureData creatureData = null;
				CreatureTable creatureTable = null;
				if (this.reference == WaveData.Group.Reference.Creature)
				{
					creatureData = Catalog.GetData<CreatureData>(this.referenceID, true);
				}
				else
				{
					creatureTable = Catalog.GetData<CreatureTable>(this.referenceID, true);
				}
				if (creatureData == null && creatureTable == null)
				{
					yield break;
				}
				int i = 0;
				while ((float)i < count)
				{
					if (creatureTable != null)
					{
						creatureTable.TryPick(out creatureData, null);
					}
					CreatureData overrideData = creatureData;
					if (this.overrideFaction || this.overrideBrain || this.overrideContainer)
					{
						WaveData.Group.<>c__DisplayClass42_0 CS$<>8__locals1 = new WaveData.Group.<>c__DisplayClass42_0();
						CS$<>8__locals1.task = overrideData.CloneJsonAsync<CreatureData>();
						yield return new WaitUntil(() => CS$<>8__locals1.task.IsCompleted);
						overrideData = CS$<>8__locals1.task.Result;
						overrideData.Init();
						overrideData.OnCatalogRefresh();
						yield return overrideData.OnCatalogRefreshCoroutine();
						if (this.overrideFaction)
						{
							overrideData.factionId = this.factionID;
						}
						if (this.overrideBrain)
						{
							overrideData.brainId = this.overrideBrainID;
						}
						if (this.overrideContainer)
						{
							overrideData.containerID = this.overrideContainerID;
						}
						CS$<>8__locals1 = null;
					}
					if (overrideData != null)
					{
						toFill.Add(new WaveData.SpawnData(overrideData, this));
					}
					overrideData = null;
					int num = i;
					i = num + 1;
				}
				yield break;
			}

			// Token: 0x04004180 RID: 16768
			[NonSerialized]
			public WaveData waveData;

			// Token: 0x04004181 RID: 16769
			[NonSerialized]
			public int index;

			// Token: 0x04004182 RID: 16770
			public WaveData.Group.Reference reference;

			// Token: 0x04004183 RID: 16771
			public string referenceID;

			// Token: 0x04004184 RID: 16772
			[JsonIgnore]
			[HideInInspector]
			public string creatureID;

			// Token: 0x04004185 RID: 16773
			[JsonIgnore]
			[HideInInspector]
			public string creatureTableID;

			// Token: 0x04004186 RID: 16774
			public bool overrideFaction;

			// Token: 0x04004187 RID: 16775
			public int factionID;

			// Token: 0x04004188 RID: 16776
			public bool overrideContainer;

			// Token: 0x04004189 RID: 16777
			public string overrideContainerID;

			// Token: 0x0400418A RID: 16778
			public bool overrideBrain;

			// Token: 0x0400418B RID: 16779
			public string overrideBrainID;

			// Token: 0x0400418C RID: 16780
			public bool overrideMaxMelee;

			// Token: 0x0400418D RID: 16781
			public int overrideMaxMeleeCount;

			// Token: 0x0400418E RID: 16782
			public float groupHealthMultiplier = 1f;

			// Token: 0x0400418F RID: 16783
			public Vector2Int minMaxCount = new Vector2Int(1, 1);

			// Token: 0x04004190 RID: 16784
			[NonSerialized]
			public bool set;

			// Token: 0x04004191 RID: 16785
			public int spawnPointIndex = -1;

			// Token: 0x04004192 RID: 16786
			public int prereqGroupIndex = -1;

			// Token: 0x04004193 RID: 16787
			[NonSerialized]
			public WaveData.Group prereqGroup;

			// Token: 0x04004194 RID: 16788
			public int prereqMaxRemainingAlive;

			// Token: 0x02000BDC RID: 3036
			public enum Reference
			{
				// Token: 0x04004D23 RID: 19747
				Creature,
				// Token: 0x04004D24 RID: 19748
				Table
			}
		}
	}
}
