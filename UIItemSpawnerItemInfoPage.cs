using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000383 RID: 899
	public class UIItemSpawnerItemInfoPage : MonoBehaviour
	{
		// Token: 0x1700029A RID: 666
		// (get) Token: 0x06002AE2 RID: 10978 RVA: 0x001220B0 File Offset: 0x001202B0
		// (set) Token: 0x06002AE3 RID: 10979 RVA: 0x001220B8 File Offset: 0x001202B8
		public ItemData CurrentItem { get; private set; }

		// Token: 0x06002AE4 RID: 10980 RVA: 0x001220C1 File Offset: 0x001202C1
		private void Awake()
		{
			this.noItemIcon = this.iconSprite.sprite;
			this.noItemIconColor = this.iconSprite.color;
		}

		// Token: 0x06002AE5 RID: 10981 RVA: 0x001220E5 File Offset: 0x001202E5
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
		}

		// Token: 0x06002AE6 RID: 10982 RVA: 0x001220F8 File Offset: 0x001202F8
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
			if (this.addressableTexture)
			{
				Catalog.ReleaseAsset<Sprite>(this.addressableTexture);
			}
		}

		// Token: 0x06002AE7 RID: 10983 RVA: 0x00122123 File Offset: 0x00120323
		private void OnLanguageChanged(string language)
		{
			this.SetLocalizedFields();
		}

		// Token: 0x06002AE8 RID: 10984 RVA: 0x0012212C File Offset: 0x0012032C
		private void SetLocalizedFields()
		{
			TextData.Item itemLocalization = LocalizationManager.Instance.GetLocalizedTextItem(this.CurrentItem.localizationId);
			this.itemName.text = ((itemLocalization != null) ? itemLocalization.name : ((!this.CurrentItem.displayName.IsNullOrEmptyOrWhitespace()) ? this.CurrentItem.displayName : this.notAvailableFallbackString));
			this.description.text = ((itemLocalization != null) ? itemLocalization.description : ((!this.CurrentItem.description.IsNullOrEmptyOrWhitespace()) ? this.CurrentItem.description : this.notAvailableFallbackString));
			this.blacksmith.text = (string.IsNullOrEmpty(this.CurrentItem.author) ? this.notAvailableFallbackString : this.CurrentItem.author);
		}

		// Token: 0x06002AE9 RID: 10985 RVA: 0x001221F5 File Offset: 0x001203F5
		public void ToggleInfoPage(bool show)
		{
			base.gameObject.SetActive(show);
		}

		// Token: 0x06002AEA RID: 10986 RVA: 0x00122204 File Offset: 0x00120404
		public void SetItemInfo(ItemData itemData)
		{
			if (itemData == null)
			{
				Debug.LogError("Cannot display null item data!");
				return;
			}
			this.CurrentItem = itemData;
			this.SetLocalizedFields();
			if (this.CurrentItem.icon != null)
			{
				this.iconSprite.sprite = this.CurrentItem.icon;
				this.iconSprite.color = Color.white;
				return;
			}
			this.iconSprite.sprite = this.noItemIcon;
			this.iconSprite.color = this.noItemIconColor;
			this.CurrentItem.LoadIconAsync(false, delegate(Sprite texture)
			{
				if (texture != null)
				{
					this.iconSprite.sprite = texture;
					this.iconSprite.color = Color.white;
					this.addressableTexture = texture;
				}
			});
		}

		// Token: 0x0400289D RID: 10397
		[SerializeField]
		private TextMeshProUGUI itemName;

		// Token: 0x0400289E RID: 10398
		[SerializeField]
		private TextMeshProUGUI blacksmith;

		// Token: 0x0400289F RID: 10399
		[SerializeField]
		private TextMeshProUGUI description;

		// Token: 0x040028A0 RID: 10400
		[SerializeField]
		private Image iconSprite;

		// Token: 0x040028A1 RID: 10401
		private Sprite addressableTexture;

		// Token: 0x040028A2 RID: 10402
		private Sprite noItemIcon;

		// Token: 0x040028A3 RID: 10403
		private Color noItemIconColor;

		// Token: 0x040028A4 RID: 10404
		private string notAvailableFallbackString = "---";
	}
}
