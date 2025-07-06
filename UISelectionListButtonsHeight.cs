using System;

namespace ThunderRoad
{
	// Token: 0x0200038E RID: 910
	public class UISelectionListButtonsHeight : UISelectionListButtons
	{
		// Token: 0x06002B63 RID: 11107 RVA: 0x0012509A File Offset: 0x0012329A
		protected override void Awake()
		{
			base.Awake();
			this.minValue = 130;
			this.maxValue = 230;
			this.Refresh();
		}

		// Token: 0x06002B64 RID: 11108 RVA: 0x001250BE File Offset: 0x001232BE
		private void OnEnable()
		{
			this.Refresh();
		}

		// Token: 0x06002B65 RID: 11109 RVA: 0x001250C6 File Offset: 0x001232C6
		public override void LoadValue()
		{
			if (!this.characterSelection.initialized)
			{
				return;
			}
			this.currentValue = (int)(this.characterSelection.GetCharacterData().calibration.height * 100f);
		}

		// Token: 0x06002B66 RID: 11110 RVA: 0x001250F8 File Offset: 0x001232F8
		public void SetCurrentValue(float height)
		{
			this.currentValue = (int)height;
			this.characterSelection.currentSize = (float)this.currentValue;
			this.RefreshDisplay();
		}

		// Token: 0x06002B67 RID: 11111 RVA: 0x0012511A File Offset: 0x0012331A
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
			this.characterSelection.currentSize = (float)this.currentValue;
			this.characterSelection.CalibrateCharacterHeightManually(false);
			this.characterSelection.ConvertMeterToImperial();
		}

		// Token: 0x06002B68 RID: 11112 RVA: 0x00125154 File Offset: 0x00123354
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.value != null)
			{
				this.value.text = ((float)this.currentValue / 100f).ToString() + "m";
			}
		}
	}
}
