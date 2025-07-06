using System;

namespace ThunderRoad
{
	// Token: 0x02000392 RID: 914
	public class UISelectionListButtonsSwitchMeterImperial : UISelectionListButtons
	{
		// Token: 0x06002B91 RID: 11153 RVA: 0x001260C0 File Offset: 0x001242C0
		protected override void Awake()
		{
			base.Awake();
			this.minValue = 0;
			this.maxValue = 1;
			this.currentValue = this.minValue;
			if (this.value != null)
			{
				this.value.text = ((this.currentValue == 0) ? LocalizationManager.Instance.GetLocalizedString("Default", "Metric", false).ToUpper() : LocalizationManager.Instance.GetLocalizedString("Default", "Imperial", false).ToUpper());
			}
		}

		// Token: 0x06002B92 RID: 11154 RVA: 0x00126144 File Offset: 0x00124344
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
		}

		// Token: 0x06002B93 RID: 11155 RVA: 0x00126157 File Offset: 0x00124357
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002B94 RID: 11156 RVA: 0x0012616A File Offset: 0x0012436A
		private void OnLanguageChanged(string language)
		{
			this.RefreshDisplay();
		}

		// Token: 0x06002B95 RID: 11157 RVA: 0x00126174 File Offset: 0x00124374
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
			if (this.currentValue == 1)
			{
				this.characterSelection.ConvertMeterToImperial();
				this.imperialFeetInput.SetCurrentValue(this.characterSelection.currentFeet);
				this.imperialInchInput.SetCurrentValue(this.characterSelection.currentInch);
				return;
			}
			this.characterSelection.ConvertImperialToMeter();
			this.meterInput.SetCurrentValue(this.characterSelection.currentSize);
		}

		// Token: 0x06002B96 RID: 11158 RVA: 0x001261F0 File Offset: 0x001243F0
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.currentValue == 0)
			{
				this.meterInput.gameObject.SetActive(true);
				this.imperialFeetInput.gameObject.SetActive(false);
				this.imperialInchInput.gameObject.SetActive(false);
			}
			else
			{
				this.meterInput.gameObject.SetActive(false);
				this.imperialFeetInput.gameObject.SetActive(true);
				this.imperialInchInput.gameObject.SetActive(true);
			}
			if (this.value != null)
			{
				this.value.text = ((this.currentValue == 0) ? LocalizationManager.Instance.GetLocalizedString("Default", "Metric", false).ToUpper() : LocalizationManager.Instance.GetLocalizedString("Default", "Imperial", false).ToUpper());
			}
		}

		// Token: 0x0400291E RID: 10526
		public UISelectionListButtonsHeight meterInput;

		// Token: 0x0400291F RID: 10527
		public UISelectionListButtonsHeightImperial imperialFeetInput;

		// Token: 0x04002920 RID: 10528
		public UISelectionListButtonsHeightImperial imperialInchInput;
	}
}
