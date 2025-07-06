using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200037B RID: 891
	public class UIDropdown : TMP_Dropdown
	{
		/// <summary>
		/// Show the dropdown.
		/// </summary>
		// Token: 0x06002A6A RID: 10858 RVA: 0x0011F4B3 File Offset: 0x0011D6B3
		public override bool Show()
		{
			if (!base.Show())
			{
				return false;
			}
			if (this.updateTitlePosition)
			{
				this.title.transform.localPosition = this.openedTitlePosition;
			}
			UnityEvent onDropdownShow = this.OnDropdownShow;
			if (onDropdownShow != null)
			{
				onDropdownShow.Invoke();
			}
			return true;
		}

		/// <summary>
		/// Hide the dropdown list. I.e. close it.
		/// </summary>
		// Token: 0x06002A6B RID: 10859 RVA: 0x0011F4EF File Offset: 0x0011D6EF
		public override void Hide()
		{
			base.Hide();
			if (this.updateTitlePosition)
			{
				this.title.transform.localPosition = this.closedTitlePosition;
			}
			UnityEvent onDropdownHide = this.OnDropdownHide;
			if (onDropdownHide == null)
			{
				return;
			}
			onDropdownHide.Invoke();
		}

		// Token: 0x06002A6C RID: 10860 RVA: 0x0011F528 File Offset: 0x0011D728
		public void SetupStyle(UIPlayerMenu.Style style, Color defaultColor, Color highlightColor, Dictionary<UIPlayerMenu.Style, Color> dropdownBackgroundColors, Dictionary<UIPlayerMenu.Style, Material> outlineFontMaterials)
		{
			this.title.color = defaultColor;
			base.captionText.color = defaultColor;
			this.arrowImage.color = defaultColor;
			if (this.backgroundImage == null)
			{
				this.backgroundImage = base.template.GetComponent<Image>();
			}
			this.backgroundImage.color = dropdownBackgroundColors[style];
			if (this.dropdownButton == null)
			{
				this.dropdownButton = base.GetComponent<UICustomisableButton>();
			}
			this.dropdownButton.SetupStyle(style, defaultColor, highlightColor, outlineFontMaterials);
			this.itemButton.toggle.graphic.color = defaultColor;
			this.itemButton.toggle.targetGraphic.color = dropdownBackgroundColors[style];
			this.itemButton.SetupStyle(style, defaultColor, highlightColor, outlineFontMaterials);
		}

		// Token: 0x0400282A RID: 10282
		[Header("References")]
		public TMP_Text title;

		// Token: 0x0400282B RID: 10283
		[SerializeField]
		private Image arrowImage;

		// Token: 0x0400282C RID: 10284
		[SerializeField]
		private UICustomisableButton itemButton;

		// Token: 0x0400282D RID: 10285
		[Space]
		[Header("Settings")]
		[SerializeField]
		private bool updateTitlePosition;

		// Token: 0x0400282E RID: 10286
		[SerializeField]
		private Vector3 openedTitlePosition;

		// Token: 0x0400282F RID: 10287
		[SerializeField]
		private Vector3 closedTitlePosition;

		// Token: 0x04002830 RID: 10288
		[Space]
		[Header("Events")]
		public UnityEvent OnDropdownShow;

		// Token: 0x04002831 RID: 10289
		public UnityEvent OnDropdownHide;

		// Token: 0x04002832 RID: 10290
		private UICustomisableButton dropdownButton;

		// Token: 0x04002833 RID: 10291
		private Image backgroundImage;
	}
}
