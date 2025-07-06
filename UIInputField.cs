using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200037E RID: 894
	public class UIInputField : MonoBehaviour
	{
		// Token: 0x06002A8A RID: 10890 RVA: 0x0011FA0C File Offset: 0x0011DC0C
		public void SetupStyle(UIPlayerMenu.Style style, Color defaultColor, Color highlightColor, Dictionary<UIPlayerMenu.Style, Material> outlineFontMaterials)
		{
			this.title.color = defaultColor;
			this.placeHolderText.color = defaultColor;
			this.inputText.color = defaultColor;
			if (this.framesSelected == null)
			{
				this.framesSelected = new Dictionary<UIPlayerMenu.Style, Sprite>
				{
					{
						UIPlayerMenu.Style.Medieval,
						this.medievalFrameSelected
					},
					{
						UIPlayerMenu.Style.Modern,
						this.modernFrameSelected
					}
				};
			}
			this.frameSelected.sprite = this.framesSelected[style];
			if (this.inputButton == null)
			{
				this.inputButton = base.GetComponent<UICustomisableButton>();
			}
			this.inputButton.SetupStyle(style, defaultColor, highlightColor, outlineFontMaterials);
		}

		// Token: 0x04002842 RID: 10306
		[SerializeField]
		private TextMeshProUGUI title;

		// Token: 0x04002843 RID: 10307
		[SerializeField]
		private TextMeshProUGUI placeHolderText;

		// Token: 0x04002844 RID: 10308
		[SerializeField]
		private TextMeshProUGUI inputText;

		// Token: 0x04002845 RID: 10309
		[SerializeField]
		private Image frameSelected;

		// Token: 0x04002846 RID: 10310
		[SerializeField]
		private Sprite medievalFrameSelected;

		// Token: 0x04002847 RID: 10311
		[SerializeField]
		private Sprite modernFrameSelected;

		// Token: 0x04002848 RID: 10312
		private UICustomisableButton inputButton;

		// Token: 0x04002849 RID: 10313
		private Dictionary<UIPlayerMenu.Style, Sprite> framesSelected;
	}
}
