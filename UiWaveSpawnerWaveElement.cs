using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000399 RID: 921
	public class UiWaveSpawnerWaveElement : MonoBehaviour
	{
		// Token: 0x06002BD5 RID: 11221 RVA: 0x0012758C File Offset: 0x0012578C
		private void Awake()
		{
			this.waveName = base.GetComponent<TextMeshProUGUI>();
			this.toggle = base.GetComponent<Toggle>();
		}

		// Token: 0x06002BD6 RID: 11222 RVA: 0x001275A6 File Offset: 0x001257A6
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.SetLocalizedFields();
		}

		// Token: 0x06002BD7 RID: 11223 RVA: 0x001275BF File Offset: 0x001257BF
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002BD8 RID: 11224 RVA: 0x001275D2 File Offset: 0x001257D2
		private void OnLanguageChanged(string language)
		{
			this.SetLocalizedFields();
		}

		// Token: 0x06002BD9 RID: 11225 RVA: 0x001275DC File Offset: 0x001257DC
		private void SetLocalizedFields()
		{
			if (this.waveData == null)
			{
				return;
			}
			TextData.Wave waveLocalization = LocalizationManager.Instance.GetLocalizedTextWave(this.waveData.localizationId);
			base.name = ((waveLocalization != null) ? waveLocalization.title : (char.IsNumber(this.waveData.title[0]) ? this.waveData.title.Substring(1, this.waveData.title.Length - 1) : this.waveData.title));
			this.waveName.text = base.name;
		}

		// Token: 0x06002BDA RID: 11226 RVA: 0x00127674 File Offset: 0x00125874
		public void SetupWave(UIWaveSpawner waveSpawner, WaveData waveData)
		{
			this.waveData = waveData;
			this.SetLocalizedFields();
			this.toggle.onValueChanged.AddListener(delegate(bool <p0>)
			{
				waveSpawner.OnWaveSelectedChanged(this.toggle, waveData);
			});
		}

		// Token: 0x04002953 RID: 10579
		private TextMeshProUGUI waveName;

		// Token: 0x04002954 RID: 10580
		private Toggle toggle;

		// Token: 0x04002955 RID: 10581
		private WaveData waveData;
	}
}
