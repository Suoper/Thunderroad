using System;

namespace ThunderRoad
{
	// Token: 0x02000389 RID: 905
	public class UISelectionListButtonsBool : UISelectionListButtons
	{
		// Token: 0x06002B25 RID: 11045 RVA: 0x0012396C File Offset: 0x00121B6C
		protected override void Awake()
		{
			base.Awake();
			this.minValue = 0;
			this.maxValue = 1;
			if (!this.isValueInitialized)
			{
				this.currentValue = 0;
				if (this.value != null)
				{
					this.value.text = LocalizationManager.Instance.GetLocalizedString("Default", "False", false);
				}
			}
		}

		// Token: 0x06002B26 RID: 11046 RVA: 0x001239CA File Offset: 0x00121BCA
		public override void LoadValue()
		{
		}

		// Token: 0x06002B27 RID: 11047 RVA: 0x001239CC File Offset: 0x00121BCC
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B28 RID: 11048 RVA: 0x001239DC File Offset: 0x00121BDC
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.value != null)
			{
				this.value.text = LocalizationManager.Instance.GetLocalizedString("Default", (base.GetCurrentValue() == 0) ? "Disabled" : "Enabled", false);
			}
		}

		// Token: 0x06002B29 RID: 11049 RVA: 0x00123A2C File Offset: 0x00121C2C
		public bool GetCurrentBoolValue()
		{
			return base.GetCurrentValue() != 0;
		}

		// Token: 0x06002B2A RID: 11050 RVA: 0x00123A37 File Offset: 0x00121C37
		public override void SetValue(int value, bool silent = false)
		{
			base.SetValue(value, silent);
			this.isValueInitialized = true;
		}

		// Token: 0x040028DF RID: 10463
		private bool isValueInitialized;
	}
}
