using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000388 RID: 904
	public class UISelectionListButtons : MonoBehaviour
	{
		// Token: 0x14000143 RID: 323
		// (add) Token: 0x06002B15 RID: 11029 RVA: 0x00123550 File Offset: 0x00121750
		// (remove) Token: 0x06002B16 RID: 11030 RVA: 0x00123588 File Offset: 0x00121788
		public event Action<UISelectionListButtons> onUpdateValueEvent;

		// Token: 0x1700029B RID: 667
		// (get) Token: 0x06002B17 RID: 11031 RVA: 0x001235BD File Offset: 0x001217BD
		public virtual bool Loop
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700029C RID: 668
		// (get) Token: 0x06002B18 RID: 11032 RVA: 0x001235C0 File Offset: 0x001217C0
		public virtual int stepValue
		{
			get
			{
				return 1;
			}
		}

		// Token: 0x06002B19 RID: 11033 RVA: 0x001235C3 File Offset: 0x001217C3
		protected virtual void OnValidate()
		{
		}

		// Token: 0x06002B1A RID: 11034 RVA: 0x001235C8 File Offset: 0x001217C8
		protected virtual void Awake()
		{
			this.characterSelection = base.GetComponentInParent<CharacterSelection>();
			if (!this.colorViewer && base.transform.Find("Color"))
			{
				Transform transform = base.transform.Find("Color");
				this.colorViewer = ((transform != null) ? transform.GetComponent<RawImage>() : null);
			}
			if (!this.colorViewer2 && base.transform.Find("Color2"))
			{
				Transform transform2 = base.transform.Find("Color2");
				this.colorViewer2 = ((transform2 != null) ? transform2.GetComponent<RawImage>() : null);
			}
			if (this.nextButton)
			{
				this.nextButton.onPointerClick.AddListener(new UnityAction(this.NextValue));
			}
			if (this.previousButton)
			{
				this.previousButton.onPointerClick.AddListener(new UnityAction(this.PreviousValue));
			}
		}

		// Token: 0x06002B1B RID: 11035 RVA: 0x001236C1 File Offset: 0x001218C1
		public virtual void LoadValue()
		{
		}

		// Token: 0x06002B1C RID: 11036 RVA: 0x001236C3 File Offset: 0x001218C3
		public virtual void Refresh()
		{
			this.LoadValue();
			this.RefreshDisplay();
		}

		// Token: 0x06002B1D RID: 11037 RVA: 0x001236D4 File Offset: 0x001218D4
		public virtual void NextValue()
		{
			if (this.currentValue + this.stepValue <= this.maxValue)
			{
				this.currentValue += this.stepValue;
				this.OnUpdateValue(false);
				return;
			}
			if (this.Loop)
			{
				this.currentValue = this.minValue;
				this.OnUpdateValue(false);
			}
		}

		// Token: 0x06002B1E RID: 11038 RVA: 0x0012372C File Offset: 0x0012192C
		public virtual void PreviousValue()
		{
			if (this.currentValue - this.stepValue >= this.minValue)
			{
				this.currentValue -= this.stepValue;
				this.OnUpdateValue(false);
				return;
			}
			if (this.Loop)
			{
				this.currentValue = this.maxValue;
				this.OnUpdateValue(false);
			}
		}

		// Token: 0x06002B1F RID: 11039 RVA: 0x00123784 File Offset: 0x00121984
		public virtual void SetValue(int value, bool silent = false)
		{
			if (this.currentValue != value)
			{
				this.currentValue = value;
				this.OnUpdateValue(silent);
			}
		}

		// Token: 0x06002B20 RID: 11040 RVA: 0x0012379D File Offset: 0x0012199D
		public int GetCurrentValue()
		{
			return this.currentValue;
		}

		// Token: 0x06002B21 RID: 11041 RVA: 0x001237A8 File Offset: 0x001219A8
		public virtual void RefreshDisplay()
		{
			if (this.value != null)
			{
				this.value.text = this.currentValue.ToString();
			}
			if (this.nextButton != null)
			{
				this.nextButton.IsInteractable = (this.Loop || this.currentValue != this.maxValue);
			}
			if (this.previousButton != null)
			{
				this.previousButton.IsInteractable = (this.Loop || this.currentValue != this.minValue);
			}
		}

		// Token: 0x06002B22 RID: 11042 RVA: 0x00123844 File Offset: 0x00121A44
		public virtual void SetInteractable(bool active)
		{
			this.isInteractable = active;
			if (this.nextButton)
			{
				this.nextButton.IsInteractable = active;
			}
			if (this.previousButton)
			{
				this.previousButton.IsInteractable = active;
			}
			if (this.value)
			{
				this.value.color = (active ? Color.white : Color.gray);
			}
			if (this.title)
			{
				this.title.color = (active ? Color.white : Color.gray);
			}
			if (active)
			{
				return;
			}
			if (this.value)
			{
				this.value.text = "N/A";
			}
			this.maxValue = 0;
			this.currentValue = 0;
			if (this.colorViewer)
			{
				this.colorViewer.color = Color.gray;
			}
			if (this.colorViewer2)
			{
				this.colorViewer.color = Color.gray;
			}
		}

		// Token: 0x06002B23 RID: 11043 RVA: 0x00123941 File Offset: 0x00121B41
		public virtual void OnUpdateValue(bool silent = false)
		{
			if (!silent && this.onUpdateValueEvent != null)
			{
				this.onUpdateValueEvent(this);
			}
		}

		// Token: 0x040028D3 RID: 10451
		[SerializeField]
		protected UICustomisableButton nextButton;

		// Token: 0x040028D4 RID: 10452
		[SerializeField]
		protected UICustomisableButton previousButton;

		// Token: 0x040028D5 RID: 10453
		[SerializeField]
		protected TextMeshProUGUI title;

		// Token: 0x040028D6 RID: 10454
		[SerializeField]
		protected TextMeshProUGUI value;

		// Token: 0x040028D8 RID: 10456
		protected RawImage colorViewer;

		// Token: 0x040028D9 RID: 10457
		protected RawImage colorViewer2;

		// Token: 0x040028DA RID: 10458
		protected int currentValue;

		// Token: 0x040028DB RID: 10459
		protected int minValue;

		// Token: 0x040028DC RID: 10460
		protected int maxValue;

		// Token: 0x040028DD RID: 10461
		protected bool isInteractable = true;

		// Token: 0x040028DE RID: 10462
		protected CharacterSelection characterSelection;
	}
}
