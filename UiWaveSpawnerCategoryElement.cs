using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000398 RID: 920
	public class UiWaveSpawnerCategoryElement : MonoBehaviour
	{
		// Token: 0x06002BCF RID: 11215 RVA: 0x00127479 File Offset: 0x00125679
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.SetLocalizedFields();
		}

		// Token: 0x06002BD0 RID: 11216 RVA: 0x00127492 File Offset: 0x00125692
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002BD1 RID: 11217 RVA: 0x001274A5 File Offset: 0x001256A5
		private void OnLanguageChanged(string language)
		{
			this.SetLocalizedFields();
		}

		// Token: 0x06002BD2 RID: 11218 RVA: 0x001274AD File Offset: 0x001256AD
		private void SetLocalizedFields()
		{
			if (string.IsNullOrEmpty(this.category))
			{
				return;
			}
			this.categoryList.SetLocalizationIds("Default", "{" + this.category + "}");
		}

		// Token: 0x06002BD3 RID: 11219 RVA: 0x001274E4 File Offset: 0x001256E4
		public void SetCategory(UIWaveSpawner waveSpawner, string category, List<CatalogData> wavesData)
		{
			this.category = category;
			this.categoryList.Init(category);
			base.gameObject.name = "Category - " + category;
			this.SetLocalizedFields();
			for (int i = 0; i < wavesData.Count; i++)
			{
				WaveData waveData = (WaveData)wavesData[i];
				UiWaveSpawnerWaveElement newWave = UnityEngine.Object.Instantiate<UiWaveSpawnerWaveElement>(this.waveElement, base.transform);
				newWave.SetupWave(waveSpawner, waveData);
				this.categoryList.AddElement(newWave.gameObject);
			}
			UnityEngine.Object.Destroy(this.waveElement.gameObject);
			this.categoryList.CollapseList();
		}

		// Token: 0x04002950 RID: 10576
		[SerializeField]
		private UIExpandableList categoryList;

		// Token: 0x04002951 RID: 10577
		[SerializeField]
		private UiWaveSpawnerWaveElement waveElement;

		// Token: 0x04002952 RID: 10578
		private string category;
	}
}
