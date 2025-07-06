using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000380 RID: 896
	public class UiItemSpawnerCategoryElement : UIItemSpawnerGridElement
	{
		// Token: 0x06002AD0 RID: 10960 RVA: 0x00121C20 File Offset: 0x0011FE20
		protected override void SetLocalizedFields()
		{
			if (this.categoryData == null || base.gameObject == null || this.elementName == null)
			{
				return;
			}
			if (LocalizationManager.Instance == null)
			{
				Debug.LogError("LocalizationManager is null when setting localized fields!");
				return;
			}
			string categoryName = LocalizationManager.Instance.GetLocalizedString("Default", this.categoryData.name, false);
			if (base.gameObject)
			{
				base.gameObject.name = (categoryName ?? this.categoryData.name);
			}
			if (this.elementName)
			{
				this.elementName.text = (categoryName ?? this.categoryData.name);
			}
		}

		// Token: 0x06002AD1 RID: 10961 RVA: 0x00121CD8 File Offset: 0x0011FED8
		public void SetCategory(UIItemSpawner itemSpawner, GameData.Category categoryData, Sprite icon, ToggleGroup group)
		{
			this.categoryData = categoryData;
			this.SetLocalizedFields();
			this.itemCount.text = categoryData.itemsCount.ToString();
			if (icon != null)
			{
				this.iconSprite.sprite = icon;
			}
			base.Button.toggle.onValueChanged.AddListener(delegate(bool <p0>)
			{
				itemSpawner.OnCategoryChanged(categoryData.name, this);
			});
		}

		// Token: 0x06002AD2 RID: 10962 RVA: 0x00121D63 File Offset: 0x0011FF63
		public void SetInteractable(bool canInteract)
		{
			base.Button.IsInteractable = canInteract;
			base.Button.toggle.interactable = canInteract;
		}

		// Token: 0x06002AD3 RID: 10963 RVA: 0x00121D82 File Offset: 0x0011FF82
		public void SetCount(int count)
		{
			this.itemCount.text = count.ToString();
		}

		// Token: 0x04002890 RID: 10384
		[SerializeField]
		private TextMeshProUGUI itemCount;

		// Token: 0x04002891 RID: 10385
		public GameData.Category categoryData;
	}
}
