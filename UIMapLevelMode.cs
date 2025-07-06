using System;
using System.Collections.Generic;
using TMPro;

namespace ThunderRoad
{
	// Token: 0x02000385 RID: 901
	public class UIMapLevelMode : UISelectionListButtons
	{
		// Token: 0x06002AF9 RID: 11001 RVA: 0x001228E0 File Offset: 0x00120AE0
		protected override void Awake()
		{
			base.Awake();
			this.currentValue = 0;
			this.minValue = 0;
			if (this.value != null)
			{
				this.value.text = "";
			}
			this.maxValue = 0;
		}

		// Token: 0x06002AFA RID: 11002 RVA: 0x0012291B File Offset: 0x00120B1B
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
		}

		// Token: 0x06002AFB RID: 11003 RVA: 0x0012292E File Offset: 0x00120B2E
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002AFC RID: 11004 RVA: 0x00122941 File Offset: 0x00120B41
		private void OnLanguageChanged(string language)
		{
			this.SetTexts();
		}

		// Token: 0x06002AFD RID: 11005 RVA: 0x00122949 File Offset: 0x00120B49
		public string GetCurrentLevelMode()
		{
			return this.modeSelected;
		}

		// Token: 0x06002AFE RID: 11006 RVA: 0x00122951 File Offset: 0x00120B51
		public override void OnUpdateValue(bool silent = false)
		{
			if (this.modes == null || this.modes.Count == 0 || this.currentValue >= this.modes.Count)
			{
				return;
			}
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002AFF RID: 11007 RVA: 0x0012298C File Offset: 0x00120B8C
		public void UpdateValues(LevelInstance levelInstance)
		{
			LevelData levelData = levelInstance.LevelData;
			if (levelData == null)
			{
				return;
			}
			this.modes = new List<LevelData.Mode>();
			if (levelInstance.IsUserConfigurable())
			{
				this.modes = levelData.GetGameModeAllowedModes();
			}
			else
			{
				this.modes.Add(levelInstance.GetMode());
			}
			this.currentValue = 0;
			this.maxValue = this.modes.Count - 1;
			if (this.modes.Count > 0)
			{
				base.gameObject.SetActive(true);
				this.modeSelected = this.modes[0].name;
			}
			else
			{
				base.gameObject.SetActive(false);
			}
			this.OnUpdateValue(false);
		}

		// Token: 0x06002B00 RID: 11008 RVA: 0x00122A38 File Offset: 0x00120C38
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.modes == null || this.modes.Count == 0 || this.currentValue >= this.modes.Count)
			{
				return;
			}
			this.SetTexts();
			this.modeSelected = this.modes[this.currentValue].name;
		}

		// Token: 0x06002B01 RID: 11009 RVA: 0x00122A98 File Offset: 0x00120C98
		public void SetTexts()
		{
			if (this.modes == null || this.modes.Count == 0 || this.currentValue >= this.modes.Count)
			{
				return;
			}
			if (this.value != null)
			{
				this.value.text = (LocalizationManager.Instance.GetLocalizedString("Default", this.modes[this.currentValue].name, false) ?? this.modes[this.currentValue].name);
			}
			this.levelModeDescription.text = (LocalizationManager.Instance.GetLocalizedString("Default", this.modes[this.currentValue].description, false) ?? this.modes[this.currentValue].description);
			this.levelModeTitle.text = (LocalizationManager.Instance.GetLocalizedString("Default", this.modes[this.currentValue].displayName, false) ?? this.modes[this.currentValue].displayName);
		}

		// Token: 0x040028B0 RID: 10416
		public TextMeshProUGUI levelModeDescription;

		// Token: 0x040028B1 RID: 10417
		public TextMeshProUGUI levelModeTitle;

		// Token: 0x040028B2 RID: 10418
		private string modeSelected = "";

		// Token: 0x040028B3 RID: 10419
		private List<LevelData.Mode> modes;
	}
}
