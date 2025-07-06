using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200038D RID: 909
	public class UISelectionListButtonsGameModeInventoryStart : UISelectionListButtons
	{
		// Token: 0x06002B58 RID: 11096 RVA: 0x00124E40 File Offset: 0x00123040
		public void SetGameMode(GameModeData gameMode)
		{
			this.gameMode = gameMode;
			if (gameMode == null || gameMode.playerInventoryStart == null || gameMode.playerInventoryStart.Count < 2)
			{
				base.gameObject.SetActive(false);
				this.currentValue = 0;
				return;
			}
			base.gameObject.SetActive(true);
			this.maxValue = gameMode.playerInventoryStart.Count - 1;
			this.minValue = 0;
			this.SetValue(0, false);
			this.RefreshDisplay();
		}

		// Token: 0x06002B59 RID: 11097 RVA: 0x00124EB5 File Offset: 0x001230B5
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

		// Token: 0x06002B5A RID: 11098 RVA: 0x00124EEF File Offset: 0x001230EF
		private void Start()
		{
			this.LoadValue();
			this.OnUpdateValue(false);
		}

		// Token: 0x06002B5B RID: 11099 RVA: 0x00124EFE File Offset: 0x001230FE
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.RefreshDisplay();
		}

		// Token: 0x06002B5C RID: 11100 RVA: 0x00124F17 File Offset: 0x00123117
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002B5D RID: 11101 RVA: 0x00124F2A File Offset: 0x0012312A
		private void OnLanguageChanged(string language)
		{
			this.RefreshDisplay();
		}

		// Token: 0x06002B5E RID: 11102 RVA: 0x00124F32 File Offset: 0x00123132
		public override void LoadValue()
		{
		}

		// Token: 0x06002B5F RID: 11103 RVA: 0x00124F34 File Offset: 0x00123134
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B60 RID: 11104 RVA: 0x00124F44 File Offset: 0x00123144
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.gameMode == null)
			{
				return;
			}
			if (this.gameMode.playerInventoryStart == null)
			{
				return;
			}
			if (this.currentValue >= this.gameMode.playerInventoryStart.Count)
			{
				return;
			}
			InventoryStart inventory = this.gameMode.playerInventoryStart[this.currentValue];
			this.value.text = (string.IsNullOrEmpty(inventory.titleTextID) ? inventory.titleText : LocalizationManager.Instance.GetLocalizedString(inventory.textGroupId, inventory.titleTextID, false));
			inventory.onSpriteTitleImageLoade -= this.OnValueImageLoaded;
			if (inventory.titleImageSprite == null)
			{
				inventory.onSpriteTitleImageLoade += this.OnValueImageLoaded;
			}
			else
			{
				this.valueImage.sprite = inventory.titleImageSprite;
			}
			this.valueImage.gameObject.SetActive(inventory.titleImageSprite != null);
			this.valueDescription.text = (string.IsNullOrEmpty(inventory.descriptionTextID) ? inventory.descriptionText : LocalizationManager.Instance.GetLocalizedString(inventory.textGroupId, inventory.descriptionTextID, false));
		}

		// Token: 0x06002B61 RID: 11105 RVA: 0x0012506D File Offset: 0x0012326D
		private void OnValueImageLoaded(Sprite obj)
		{
			this.valueImage.sprite = obj;
			this.valueImage.gameObject.SetActive(obj != null);
		}

		// Token: 0x04002908 RID: 10504
		public Image valueImage;

		// Token: 0x04002909 RID: 10505
		[SerializeField]
		protected TextMeshProUGUI valueDescription;

		// Token: 0x0400290A RID: 10506
		protected GameModeData gameMode;
	}
}
