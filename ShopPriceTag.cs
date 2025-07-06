using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x020002F3 RID: 755
	public class ShopPriceTag : MonoBehaviour
	{
		// Token: 0x06002416 RID: 9238 RVA: 0x000F6C18 File Offset: 0x000F4E18
		public void Set(float priceValue, int tier)
		{
			this.textPrice.text = string.Format("{0}", priceValue);
			this.tierUiText.text = "{Tier} ";
			this.tierNumberText.text = tier.ToString();
			this.tierUiText.Refresh(false);
			if (Catalog.gameData != null && !Catalog.gameData.tierColors.IsNullOrEmpty())
			{
				Color tierColor = Catalog.gameData.tierColors[tier];
				this.tierBackground.color = tierColor;
			}
		}

		// Token: 0x04002362 RID: 9058
		public TMP_Text textPrice;

		// Token: 0x04002363 RID: 9059
		public TMP_Text tierNumberText;

		// Token: 0x04002364 RID: 9060
		public UIText tierUiText;

		// Token: 0x04002365 RID: 9061
		public Image tierBackground;
	}
}
