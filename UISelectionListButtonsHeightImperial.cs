using System;

namespace ThunderRoad
{
	// Token: 0x0200038F RID: 911
	public class UISelectionListButtonsHeightImperial : UISelectionListButtons
	{
		// Token: 0x06002B6A RID: 11114 RVA: 0x001251A8 File Offset: 0x001233A8
		protected override void Awake()
		{
			base.Awake();
			if (this.imperialType == UISelectionListButtonsHeightImperial.ImperialType.INCH)
			{
				this.minValue = 0;
				this.maxValue = 11;
			}
			else
			{
				this.minValue = 3;
				this.maxValue = 7;
			}
			this.currentValue = this.minValue;
			string subFix = "ft";
			if (this.imperialType == UISelectionListButtonsHeightImperial.ImperialType.INCH)
			{
				subFix = "in";
			}
			if (this.value != null)
			{
				this.value.text = this.currentValue.ToString() + subFix;
			}
		}

		// Token: 0x06002B6B RID: 11115 RVA: 0x0012522E File Offset: 0x0012342E
		public override void LoadValue()
		{
			bool initialized = this.characterSelection.initialized;
		}

		// Token: 0x06002B6C RID: 11116 RVA: 0x0012523C File Offset: 0x0012343C
		public void SetCurrentValue(float height)
		{
			this.currentValue = (int)height;
			this.RefreshDisplay();
			string subFix = "ft";
			if (this.imperialType == UISelectionListButtonsHeightImperial.ImperialType.INCH)
			{
				subFix = "in";
			}
			if (this.value != null)
			{
				this.value.text = this.currentValue.ToString() + subFix;
			}
		}

		// Token: 0x06002B6D RID: 11117 RVA: 0x00125298 File Offset: 0x00123498
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
			if (this.imperialType == UISelectionListButtonsHeightImperial.ImperialType.FEET)
			{
				this.characterSelection.currentFeet = (float)this.currentValue;
				this.characterSelection.currentInch = (float)this.otherHeightSection.GetCurrentValue();
			}
			else
			{
				this.characterSelection.currentInch = (float)this.currentValue;
				this.characterSelection.currentFeet = (float)this.otherHeightSection.GetCurrentValue();
			}
			this.characterSelection.CalibrateCharacterHeightManually(true);
			string subFix = "ft";
			if (this.imperialType == UISelectionListButtonsHeightImperial.ImperialType.INCH)
			{
				subFix = "in";
			}
			if (this.value != null)
			{
				this.value.text = this.currentValue.ToString() + subFix;
			}
			this.characterSelection.ConvertImperialToMeter();
		}

		// Token: 0x0400290B RID: 10507
		public UISelectionListButtonsHeightImperial otherHeightSection;

		// Token: 0x0400290C RID: 10508
		public UISelectionListButtonsHeightImperial.ImperialType imperialType;

		// Token: 0x02000AA9 RID: 2729
		public enum ImperialType
		{
			// Token: 0x0400490F RID: 18703
			FEET,
			// Token: 0x04004910 RID: 18704
			INCH
		}
	}
}
