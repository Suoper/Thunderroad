using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000382 RID: 898
	public class UiItemSpawnerItemElement : UIItemSpawnerGridElement
	{
		// Token: 0x06002ADD RID: 10973 RVA: 0x00121DFB File Offset: 0x0011FFFB
		protected new void Awake()
		{
			base.Awake();
			this.noItemIcon = this.iconSprite.sprite;
			this.noItemIconColor = this.iconSprite.color;
		}

		// Token: 0x06002ADE RID: 10974 RVA: 0x00121E28 File Offset: 0x00120028
		protected override void SetLocalizedFields()
		{
			if (this.itemData == null)
			{
				return;
			}
			TextData.Item itemLocalization = LocalizationManager.Instance.GetLocalizedTextItem(this.itemData.localizationId);
			base.gameObject.name = ((itemLocalization != null) ? itemLocalization.name : this.itemData.displayName);
			this.elementName.text = ((itemLocalization != null) ? itemLocalization.name : this.itemData.displayName);
		}

		// Token: 0x06002ADF RID: 10975 RVA: 0x00121E98 File Offset: 0x00120098
		public virtual void SetItem(UIItemSpawner itemSpawner, UIItemSpawner.ItemInfo itemInfo, Sprite icon, bool isSelected, bool showCount)
		{
			this.itemData = itemInfo.data;
			this.SetLocalizedFields();
			if (icon == null)
			{
				icon = this.noItemIcon;
			}
			this.iconSprite.sprite = icon;
			this.iconSprite.color = Color.white;
			this.iconSprite.enabled = true;
			if (string.IsNullOrEmpty(this.itemData.tierString))
			{
				this.tier.text = "-";
				this.tierColor.gameObject.SetActive(false);
			}
			else
			{
				this.tier.text = this.itemData.tierString;
				if (itemSpawner != null)
				{
					this.tierColor.color = itemSpawner.GetTierColor(this.itemData.tier);
				}
				this.tierColor.gameObject.SetActive(true);
			}
			if (itemInfo.inHolder)
			{
				this.droppedIcon.enabled = true;
			}
			else
			{
				this.droppedIcon.enabled = false;
			}
			this.counter.SetActive(showCount);
			this.counterText.text = itemInfo.quantity.ToString();
			base.Button.toggle.onValueChanged.AddListener(delegate(bool <p0>)
			{
				itemSpawner.OnItemChanged(itemInfo, this);
			});
		}

		// Token: 0x06002AE0 RID: 10976 RVA: 0x00122010 File Offset: 0x00120210
		public void ResetItem()
		{
			this.itemData = null;
			this.iconSprite.sprite = this.noItemIcon;
			this.iconSprite.color = this.noItemIconColor;
			this.tier.text = "-";
			this.tierColor.gameObject.SetActive(false);
			this.counter.SetActive(false);
			base.gameObject.name = "Not Used Item";
			this.elementName.text = "-";
			base.Button.toggle.onValueChanged.RemoveAllListeners();
		}

		// Token: 0x04002895 RID: 10389
		[SerializeField]
		private TextMeshProUGUI tier;

		// Token: 0x04002896 RID: 10390
		[SerializeField]
		private Image tierColor;

		// Token: 0x04002897 RID: 10391
		[SerializeField]
		private GameObject counter;

		// Token: 0x04002898 RID: 10392
		[SerializeField]
		private TextMeshProUGUI counterText;

		// Token: 0x04002899 RID: 10393
		[SerializeField]
		private Image droppedIcon;

		// Token: 0x0400289A RID: 10394
		protected ItemData itemData;

		// Token: 0x0400289B RID: 10395
		private Sprite noItemIcon;

		// Token: 0x0400289C RID: 10396
		private Color noItemIconColor;
	}
}
