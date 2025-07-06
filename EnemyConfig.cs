using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200019A RID: 410
	[Serializable]
	public class EnemyConfig : CatalogData
	{
		// Token: 0x060013C6 RID: 5062 RVA: 0x0008D958 File Offset: 0x0008BB58
		public CreatureTable GetCreatureTable(CreatureSpawner.EnemyConfigType enemyConfigType)
		{
			string overrideTableId = string.Empty;
			switch (enemyConfigType)
			{
			case CreatureSpawner.EnemyConfigType.PatrolMix:
				overrideTableId = this.patrolMixId;
				break;
			case CreatureSpawner.EnemyConfigType.PatrolMelee:
				overrideTableId = this.patrolMeleeId;
				break;
			case CreatureSpawner.EnemyConfigType.PatrolRanged:
				overrideTableId = this.patrolRangedId;
				break;
			case CreatureSpawner.EnemyConfigType.AlertMix:
				overrideTableId = this.alertMixId;
				break;
			case CreatureSpawner.EnemyConfigType.AlertMelee:
				overrideTableId = this.alertMeleeId;
				break;
			case CreatureSpawner.EnemyConfigType.AlertRanged:
				overrideTableId = this.alertRangedId;
				break;
			case CreatureSpawner.EnemyConfigType.RareMix:
				overrideTableId = this.rareMixId;
				break;
			case CreatureSpawner.EnemyConfigType.RareMelee:
				overrideTableId = this.rareMeleeId;
				break;
			case CreatureSpawner.EnemyConfigType.RareRanged:
				overrideTableId = this.rareRangedId;
				break;
			}
			return Catalog.GetData<CreatureTable>(overrideTableId, true);
		}

		// Token: 0x060013C7 RID: 5063 RVA: 0x0008D9F0 File Offset: 0x0008BBF0
		public WaveData GetWave(WaveSpawner.EnemyConfigType enemyConfigType)
		{
			string overrideWaveId = string.Empty;
			switch (enemyConfigType)
			{
			case WaveSpawner.EnemyConfigType.MeleeOnlyStd:
				overrideWaveId = this.meleeOnlyStd;
				break;
			case WaveSpawner.EnemyConfigType.MeleeFocusedStd:
				overrideWaveId = this.meleeFocusedStd;
				break;
			case WaveSpawner.EnemyConfigType.MixedStd:
				overrideWaveId = this.mixedStd;
				break;
			case WaveSpawner.EnemyConfigType.RangedFocusedStd:
				overrideWaveId = this.rangedFocusedStd;
				break;
			case WaveSpawner.EnemyConfigType.MeleeOnlyArena:
				overrideWaveId = this.meleeOnlyArena;
				break;
			case WaveSpawner.EnemyConfigType.MeleeFocusedArena:
				overrideWaveId = this.meleeFocusedArena;
				break;
			case WaveSpawner.EnemyConfigType.MixedArena:
				overrideWaveId = this.mixedArena;
				break;
			case WaveSpawner.EnemyConfigType.RangedFocusedArena:
				overrideWaveId = this.rangedFocusedArena;
				break;
			case WaveSpawner.EnemyConfigType.MeleeOnlyEnd:
				overrideWaveId = this.meleeOnlyEnd;
				break;
			case WaveSpawner.EnemyConfigType.MeleeFocusedEnd:
				overrideWaveId = this.meleeFocusedEnd;
				break;
			case WaveSpawner.EnemyConfigType.MixedEnd:
				overrideWaveId = this.mixedEnd;
				break;
			case WaveSpawner.EnemyConfigType.RangedFocusedEnd:
				overrideWaveId = this.rangedFocusedEnd;
				break;
			}
			return Catalog.GetData<WaveData>(overrideWaveId, true);
		}

		// Token: 0x060013C8 RID: 5064 RVA: 0x0008DAAC File Offset: 0x0008BCAC
		public bool TryGetLevelDescription(string levelId, out string description)
		{
			description = string.Empty;
			if (this.descriptionsByLevelId == null)
			{
				return false;
			}
			int count = this.descriptionsByLevelId.Count;
			for (int i = 0; i < count; i++)
			{
				if (string.Equals(this.descriptionsByLevelId[i].levelId, levelId))
				{
					description = this.descriptionsByLevelId[i].description;
					return true;
				}
			}
			return false;
		}

		// Token: 0x060013C9 RID: 5065 RVA: 0x0008DB14 File Offset: 0x0008BD14
		public List<ValueDropdownItem<string>> GetAllCreatureTableID()
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>
			{
				new ValueDropdownItem<string>("None", "")
			};
			foreach (string id in (from x in Catalog.GetDataList(Category.CreatureTable).OfType<CreatureTable>()
			select x.id).ToList<string>())
			{
				dropdownList.Add(new ValueDropdownItem<string>(id, id));
			}
			return dropdownList;
		}

		// Token: 0x060013CA RID: 5066 RVA: 0x0008DBB8 File Offset: 0x0008BDB8
		public List<ValueDropdownItem<string>> GetAllWavesID()
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>
			{
				new ValueDropdownItem<string>("None", "")
			};
			foreach (string id in (from x in Catalog.GetDataList(Category.Wave).OfType<WaveData>()
			select x.id).ToList<string>())
			{
				dropdownList.Add(new ValueDropdownItem<string>(id, id));
			}
			return dropdownList;
		}

		// Token: 0x04001299 RID: 4761
		public const string GroupId = "EnemyConfig";

		// Token: 0x0400129A RID: 4762
		[Header("UI")]
		public string iconAddress;

		// Token: 0x0400129B RID: 4763
		public bool customIconColor;

		// Token: 0x0400129C RID: 4764
		[Tooltip("Icon color")]
		public Color iconColor = Color.white;

		// Token: 0x0400129D RID: 4765
		[Tooltip("Banner color")]
		public Color color;

		// Token: 0x0400129E RID: 4766
		public string name;

		// Token: 0x0400129F RID: 4767
		public string nameLocalizationId;

		// Token: 0x040012A0 RID: 4768
		public List<EnemyConfig.DescriptionByLevelId> descriptionsByLevelId;

		// Token: 0x040012A1 RID: 4769
		[Header("CREATURE TABLES")]
		[Header("Patrol Enemies")]
		public string patrolMixId;

		// Token: 0x040012A2 RID: 4770
		public string patrolMeleeId;

		// Token: 0x040012A3 RID: 4771
		public string patrolRangedId;

		// Token: 0x040012A4 RID: 4772
		[Header("Alert Enemies")]
		public string alertMixId;

		// Token: 0x040012A5 RID: 4773
		public string alertMeleeId;

		// Token: 0x040012A6 RID: 4774
		public string alertRangedId;

		// Token: 0x040012A7 RID: 4775
		[Header("Rare Enemies")]
		public string rareMixId;

		// Token: 0x040012A8 RID: 4776
		public string rareMeleeId;

		// Token: 0x040012A9 RID: 4777
		public string rareRangedId;

		// Token: 0x040012AA RID: 4778
		[Header("WAVES")]
		[Header("Standard Waves")]
		public string meleeOnlyStd;

		// Token: 0x040012AB RID: 4779
		public string meleeFocusedStd;

		// Token: 0x040012AC RID: 4780
		public string mixedStd;

		// Token: 0x040012AD RID: 4781
		public string rangedFocusedStd;

		// Token: 0x040012AE RID: 4782
		[Header("Arena Waves")]
		public string meleeOnlyArena;

		// Token: 0x040012AF RID: 4783
		public string meleeFocusedArena;

		// Token: 0x040012B0 RID: 4784
		public string mixedArena;

		// Token: 0x040012B1 RID: 4785
		public string rangedFocusedArena;

		// Token: 0x040012B2 RID: 4786
		[Header("Ending Waves")]
		public string meleeOnlyEnd;

		// Token: 0x040012B3 RID: 4787
		public string meleeFocusedEnd;

		// Token: 0x040012B4 RID: 4788
		public string mixedEnd;

		// Token: 0x040012B5 RID: 4789
		public string rangedFocusedEnd;

		// Token: 0x020007B2 RID: 1970
		[Serializable]
		public class DescriptionByLevelId
		{
			// Token: 0x04003EC0 RID: 16064
			public string levelId;

			// Token: 0x04003EC1 RID: 16065
			public string description;
		}
	}
}
