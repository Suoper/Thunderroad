using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200038B RID: 907
	public class UISelectionListButtonsGameMode : UISelectionListButtons
	{
		// Token: 0x06002B3F RID: 11071 RVA: 0x001245D9 File Offset: 0x001227D9
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

		// Token: 0x06002B40 RID: 11072 RVA: 0x00124613 File Offset: 0x00122813
		private void Start()
		{
			this.LoadValue();
			this.OnUpdateValue(true);
		}

		// Token: 0x06002B41 RID: 11073 RVA: 0x00124622 File Offset: 0x00122822
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.RefreshDisplay();
		}

		// Token: 0x06002B42 RID: 11074 RVA: 0x0012463B File Offset: 0x0012283B
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002B43 RID: 11075 RVA: 0x00124650 File Offset: 0x00122850
		private void OnDestroy()
		{
			this.inventoryStartSelectionButton.valueImage = null;
			for (int i = 0; i < this.availableGameModes.Count; i++)
			{
				GameModeData tempMode = this.availableGameModes[i];
				if (tempMode != null && tempMode.state != GameModeData.State.Hidden && tempMode.playerInventoryStart != null)
				{
					int count = tempMode.playerInventoryStart.Count;
					for (int j = 0; j < count; j++)
					{
						tempMode.playerInventoryStart[j].UnloadTitleImage();
					}
				}
			}
		}

		// Token: 0x06002B44 RID: 11076 RVA: 0x001246C9 File Offset: 0x001228C9
		private void OnLanguageChanged(string language)
		{
			this.RefreshDisplay();
		}

		// Token: 0x06002B45 RID: 11077 RVA: 0x001246D4 File Offset: 0x001228D4
		public override void LoadValue()
		{
			this.availableGameModes = new List<GameModeData>();
			List<GameModeData> gameModes = (from gm in Catalog.GetDataList<GameModeData>()
			orderby gm.order
			select gm).ToList<GameModeData>();
			for (int i = 0; i < gameModes.Count; i++)
			{
				GameModeData tempMode = gameModes[i];
				if (tempMode != null && tempMode.state != GameModeData.State.Hidden)
				{
					this.availableGameModes.Add(tempMode);
					if (tempMode.playerInventoryStart != null)
					{
						int count = tempMode.playerInventoryStart.Count;
						for (int j = 0; j < count; j++)
						{
							tempMode.playerInventoryStart[j].LoadTitleImage();
						}
					}
				}
			}
			this.maxValue = this.availableGameModes.Count - 1;
			this.RefreshDisplay();
		}

		// Token: 0x06002B46 RID: 11078 RVA: 0x0012479B File Offset: 0x0012299B
		public GameModeData GetCurrentGameMode()
		{
			return this.availableGameModes[this.currentValue];
		}

		// Token: 0x06002B47 RID: 11079 RVA: 0x001247B0 File Offset: 0x001229B0
		public override void OnUpdateValue(bool silent = false)
		{
			GameModeData gameMode = this.availableGameModes[this.currentValue];
			this.inventoryStartSelectionButton.SetGameMode(gameMode);
			if (gameMode.hasTutorial)
			{
				this.tutorialToggle.SetValue(1, false);
				this.tutorialToggle.gameObject.SetActive(true);
				this.gameModeConfigPanel.SetActive(true);
			}
			else
			{
				this.tutorialToggle.gameObject.SetActive(false);
				this.gameModeConfigPanel.SetActive(this.inventoryStartSelectionButton.gameObject.activeSelf);
			}
			this.difficultySelectionButton.SetGameMode(gameMode);
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B48 RID: 11080 RVA: 0x00124854 File Offset: 0x00122A54
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.availableGameModes == null)
			{
				return;
			}
			if (this.currentValue > this.availableGameModes.Count)
			{
				return;
			}
			if (this.currentValue < 0)
			{
				return;
			}
			GameModeData selectedGameMode = this.availableGameModes[this.currentValue];
			this.gameModeImage.texture = selectedGameMode.iconLoaded;
			this.value.text = (selectedGameMode.nameLocalizationId.IsNullOrEmptyOrWhitespace() ? selectedGameMode.name : LocalizationManager.Instance.GetLocalizedString("Default", selectedGameMode.nameLocalizationId, false));
			this.gameModeDescription.text = (selectedGameMode.descriptionLocalizationId.IsNullOrEmptyOrWhitespace() ? selectedGameMode.description : LocalizationManager.Instance.GetLocalizedString("Default", selectedGameMode.descriptionLocalizationId, false));
			this.gameModeWarning.text = (selectedGameMode.warningId.IsNullOrEmptyOrWhitespace() ? selectedGameMode.warning : LocalizationManager.Instance.TryGetLocalization("Default", selectedGameMode.warningId, null, false));
			this.gameModeWarning.gameObject.SetActive(!this.gameModeWarning.text.IsNullOrEmptyOrWhitespace());
			this.selectButton.gameObject.SetActive(selectedGameMode.state != GameModeData.State.Disabled);
		}

		// Token: 0x040028F4 RID: 10484
		public RawImage gameModeImage;

		// Token: 0x040028F5 RID: 10485
		public TextMeshProUGUI gameModeDescription;

		// Token: 0x040028F6 RID: 10486
		public UICustomisableButton selectButton;

		// Token: 0x040028F7 RID: 10487
		public TextMeshProUGUI gameModeWarning;

		// Token: 0x040028F8 RID: 10488
		public GameObject gameModeConfigPanel;

		// Token: 0x040028F9 RID: 10489
		public UISelectionListButtonsBool tutorialToggle;

		// Token: 0x040028FA RID: 10490
		public UISelectionListButtonsGameModeInventoryStart inventoryStartSelectionButton;

		// Token: 0x040028FB RID: 10491
		public UISelectionListButtonsGameModeDifficultyPreset difficultySelectionButton;

		// Token: 0x040028FC RID: 10492
		protected List<GameModeData> availableGameModes;
	}
}
