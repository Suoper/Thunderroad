using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200038C RID: 908
	public class UISelectionListButtonsGameModeDifficultyPreset : UISelectionListButtons
	{
		// Token: 0x06002B4A RID: 11082 RVA: 0x0012499B File Offset: 0x00122B9B
		public List<ValueDropdownItem<string>> GetAllTextGroupID()
		{
			return Catalog.GetTextData().GetDropdownAllTextGroups();
		}

		// Token: 0x06002B4B RID: 11083 RVA: 0x001249A7 File Offset: 0x00122BA7
		public List<ValueDropdownItem<string>> GetAllTextId()
		{
			return Catalog.GetTextData().GetDropdownAllTexts(this.presetCustomLocalizedGroupId);
		}

		// Token: 0x06002B4C RID: 11084 RVA: 0x001249BC File Offset: 0x00122BBC
		public void SetGameMode(GameModeData gameMode)
		{
			this.gameMode = gameMode;
			this.BuildOption();
			this.currentValue = -1;
			int defaultPreset = gameMode.defaultDifficultyPreset + 1;
			if (this.presets.Count > defaultPreset)
			{
				this.SetValue(defaultPreset, false);
				return;
			}
			if (this.presets.Count > 1)
			{
				this.SetValue(1, false);
				return;
			}
			this.SetValue(0, false);
		}

		// Token: 0x06002B4D RID: 11085 RVA: 0x00124A1C File Offset: 0x00122C1C
		protected override void Awake()
		{
			base.Awake();
			this.currentValue = 0;
			this.minValue = 0;
			if (!this.value)
			{
				this.value.text = "";
			}
			this.maxValue = 0;
		}

		// Token: 0x06002B4E RID: 11086 RVA: 0x00124A56 File Offset: 0x00122C56
		private void Start()
		{
			this.LoadValue();
			this.OnUpdateValue(false);
		}

		// Token: 0x06002B4F RID: 11087 RVA: 0x00124A65 File Offset: 0x00122C65
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.RefreshDisplay();
		}

		// Token: 0x06002B50 RID: 11088 RVA: 0x00124A7E File Offset: 0x00122C7E
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002B51 RID: 11089 RVA: 0x00124A91 File Offset: 0x00122C91
		private void OnLanguageChanged(string language)
		{
			this.RefreshDisplay();
		}

		// Token: 0x06002B52 RID: 11090 RVA: 0x00124A99 File Offset: 0x00122C99
		public override void LoadValue()
		{
		}

		// Token: 0x06002B53 RID: 11091 RVA: 0x00124A9C File Offset: 0x00122C9C
		public override void OnUpdateValue(bool silent = false)
		{
			if (this.gameMode == null)
			{
				return;
			}
			DifficultyPreset currentPreset = this.presets[this.currentValue];
			int buttonCount = this.optionButtons.Count;
			for (int i = 0; i < buttonCount; i++)
			{
				UISelectionListButtonsLevelModeOption tempButton = this.optionButtons[i];
				if (currentPreset.optionPresets.ContainsKey(tempButton.CurrentOption.name))
				{
					tempButton.SetValue(currentPreset.optionPresets[tempButton.CurrentOption.name], true);
				}
			}
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B54 RID: 11092 RVA: 0x00124B2C File Offset: 0x00122D2C
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.presets == null)
			{
				return;
			}
			if (this.currentValue > this.presets.Count)
			{
				return;
			}
			DifficultyPreset preset = this.presets[this.currentValue];
			string presetName = preset.localizationId.IsNullOrEmptyOrWhitespace() ? this.presets[this.currentValue].name : LocalizationManager.Instance.GetLocalizedString(preset.localizationGroupId, preset.localizationId, false);
			this.value.text = presetName;
			this.presetDescriptionText.text = LocalizationManager.Instance.GetLocalizedString(preset.localizationGroupId, preset.localizationDescriptionId, false);
			int buttonCount = this.optionButtons.Count;
			for (int i = 0; i < buttonCount; i++)
			{
				this.optionButtons[i].Refresh();
			}
		}

		// Token: 0x06002B55 RID: 11093 RVA: 0x00124C04 File Offset: 0x00122E04
		private void BuildOption()
		{
			for (int i = 0; i < this.optionButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(this.optionButtons[i].gameObject);
			}
			this.optionButtons.Clear();
			List<OptionBase> difficultyOptions = this.gameMode.GetDifficultyOptions();
			int optionCount = (difficultyOptions == null) ? 0 : difficultyOptions.Count;
			if (optionCount == 0)
			{
				this.difficultyParent.SetActive(false);
				return;
			}
			this.difficultyParent.SetActive(true);
			this.presets = new List<DifficultyPreset>();
			DifficultyPreset customPreset = new DifficultyPreset();
			customPreset.gameModeData = this.gameMode;
			customPreset.localizationGroupId = this.presetCustomLocalizedGroupId;
			customPreset.localizationId = this.presetCustomLocalizedName;
			customPreset.localizationDescriptionId = this.presetCustomLocalizedDescription;
			customPreset.name = "Custom";
			customPreset.optionPresets = new OptionPresets();
			this.presets.Add(customPreset);
			this.presets.AddRange(this.gameMode.difficultyPresets);
			for (int j = 0; j < optionCount; j++)
			{
				OptionBase tempOption = difficultyOptions[j];
				UISelectionListButtonsLevelModeOption buttonOption = UnityEngine.Object.Instantiate<UISelectionListButtonsLevelModeOption>(this.LevelOptionPrefab, this.optionsPanel);
				buttonOption.Setup(tempOption, this.optionDescriptionText);
				buttonOption.onUpdateValueEvent += this.ForceUpdateCustomValue;
				this.optionButtons.Add(buttonOption);
				customPreset.optionPresets[tempOption.name] = tempOption.DefaultValue();
			}
			this.maxValue = this.presets.Count - 1;
			this.minValue = 0;
		}

		// Token: 0x06002B56 RID: 11094 RVA: 0x00124D88 File Offset: 0x00122F88
		public void ForceUpdateCustomValue(UISelectionListButtons button)
		{
			if (this.currentValue == 0)
			{
				UISelectionListButtonsLevelModeOption buttonModeOption = (UISelectionListButtonsLevelModeOption)button;
				this.presets[0].optionPresets[buttonModeOption.CurrentOption.name] = buttonModeOption.CurrentOption.CurrentValue();
				return;
			}
			int optionCount = this.optionButtons.Count;
			for (int i = 0; i < optionCount; i++)
			{
				UISelectionListButtonsLevelModeOption buttonModeOption2 = this.optionButtons[i];
				this.presets[0].optionPresets[buttonModeOption2.CurrentOption.name] = buttonModeOption2.CurrentOption.CurrentValue();
			}
			this.SetValue(0, false);
		}

		// Token: 0x040028FD RID: 10493
		public GameObject difficultyParent;

		// Token: 0x040028FE RID: 10494
		public TextMeshProUGUI presetDescriptionText;

		// Token: 0x040028FF RID: 10495
		public UISelectionListButtonsLevelModeOption LevelOptionPrefab;

		// Token: 0x04002900 RID: 10496
		public Transform optionsPanel;

		// Token: 0x04002901 RID: 10497
		public UIText optionDescriptionText;

		// Token: 0x04002902 RID: 10498
		[Header("Custom text")]
		public string presetCustomLocalizedGroupId;

		// Token: 0x04002903 RID: 10499
		public string presetCustomLocalizedName;

		// Token: 0x04002904 RID: 10500
		public string presetCustomLocalizedDescription;

		// Token: 0x04002905 RID: 10501
		protected List<UISelectionListButtonsLevelModeOption> optionButtons = new List<UISelectionListButtonsLevelModeOption>();

		// Token: 0x04002906 RID: 10502
		protected GameModeData gameMode;

		// Token: 0x04002907 RID: 10503
		protected List<DifficultyPreset> presets;
	}
}
