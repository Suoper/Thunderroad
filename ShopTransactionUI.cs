using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000370 RID: 880
	public class ShopTransactionUI : MonoBehaviour
	{
		// Token: 0x060029DD RID: 10717 RVA: 0x0011CA24 File Offset: 0x0011AC24
		private void OnValidate()
		{
			if (!this.shop)
			{
				this.shop = base.GetComponentInParent<Shop>();
			}
		}

		// Token: 0x060029DE RID: 10718 RVA: 0x0011CA40 File Offset: 0x0011AC40
		private void Awake()
		{
			this.totalOrignalColor = this.totalText.color;
			this.shopReceptLinePrefab.gameObject.SetActive(false);
			Player.onSpawn += this.OnPlayerSpawn;
			this.lines = new List<ShopTransactionUILine>();
			this.shop.onZoneItemChanged += this.OnZoneItemChange;
			this.shop.onTransactionCompleted += this.OnTransactionCompleted;
			this.buyOrSellButton.labels[0].text = this.type.ToString();
			if (this.type == ShopTransactionUI.ReceptType.Buy)
			{
				UIText component = this.buyOrSellButton.labels[0].GetComponent<UIText>();
				component.text = this.buyString;
				component.Refresh(false);
				this.buyOrSellButton.onPointerClick.AddListener(new UnityAction(this.OnBuyClick));
				this.sellAllButton.gameObject.SetActive(false);
				this.titleUiText.text = this.boughtItemsString;
				this.emptyMessage.text = this.emptyBuyString;
			}
			else if (this.type == ShopTransactionUI.ReceptType.Sell)
			{
				UIText component2 = this.buyOrSellButton.labels[0].GetComponent<UIText>();
				component2.text = this.sellString;
				component2.Refresh(false);
				this.buyOrSellButton.onPointerClick.AddListener(new UnityAction(this.OnSellClick));
				this.sellAllButton.onPointerClick.AddListener(new UnityAction(this.OnSellAllClick));
				this.titleUiText.text = this.SoldItemsString;
				this.emptyMessage.text = this.emptySellString;
			}
			this.titleUiText.Refresh(false);
			this.emptyMessage.Refresh(false);
		}

		// Token: 0x060029DF RID: 10719 RVA: 0x0011CBFB File Offset: 0x0011ADFB
		private void OnDestroy()
		{
			Player.onSpawn -= this.OnPlayerSpawn;
		}

		// Token: 0x060029E0 RID: 10720 RVA: 0x0011CC0E File Offset: 0x0011AE0E
		private void OnPlayerSpawn(Player player)
		{
			this.Refresh();
		}

		// Token: 0x060029E1 RID: 10721 RVA: 0x0011CC16 File Offset: 0x0011AE16
		protected void OnBuyClick()
		{
			if (this.shop.BuyItems())
			{
				UnityEvent unityEvent = this.onBuySuccess;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
				return;
			}
			else
			{
				UnityEvent unityEvent2 = this.onBuyFail;
				if (unityEvent2 == null)
				{
					return;
				}
				unityEvent2.Invoke();
				return;
			}
		}

		// Token: 0x060029E2 RID: 10722 RVA: 0x0011CC46 File Offset: 0x0011AE46
		protected void OnSellClick()
		{
			if (this.shop.SellItems())
			{
				UnityEvent unityEvent = this.onSellSuccess;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
				return;
			}
			else
			{
				UnityEvent unityEvent2 = this.onSellFail;
				if (unityEvent2 == null)
				{
					return;
				}
				unityEvent2.Invoke();
				return;
			}
		}

		// Token: 0x060029E3 RID: 10723 RVA: 0x0011CC78 File Offset: 0x0011AE78
		protected void OnSellAllClick()
		{
			int spawnCount = 0;
			int count = 0;
			for (int i = Player.currentCreature.container.contents.Count - 1; i >= 0; i--)
			{
				ItemContent content = Player.currentCreature.container.contents[i] as ItemContent;
				if (content != null && content.data.type == ItemData.Type.Valuable)
				{
					Action<Item> <>9__0;
					for (int j = 0; j < content.quantity; j++)
					{
						ItemContent content2 = content;
						Action<Item> callback;
						if ((callback = <>9__0) == null)
						{
							callback = (<>9__0 = delegate(Item item)
							{
								Player.currentCreature.container.RemoveContent(content);
								int spawnCount;
								item.transform.position = this.autoDropPoints[spawnCount].position;
								item.transform.rotation = UnityEngine.Random.rotation;
								item.DisallowDespawn = true;
								spawnCount = spawnCount;
								spawnCount++;
							});
						}
						content2.Spawn(callback, Item.Owner.Player, false);
						count++;
						if (count >= this.autoDropPoints.Length)
						{
							return;
						}
					}
				}
			}
		}

		// Token: 0x060029E4 RID: 10724 RVA: 0x0011CD63 File Offset: 0x0011AF63
		protected void OnZoneItemChange(Shop.Transaction transaction, Item item, bool added)
		{
			this.Refresh();
		}

		// Token: 0x060029E5 RID: 10725 RVA: 0x0011CD6B File Offset: 0x0011AF6B
		protected void OnTransactionCompleted(Shop.Transaction transaction, bool success)
		{
			this.Refresh();
		}

		// Token: 0x060029E6 RID: 10726 RVA: 0x0011CD74 File Offset: 0x0011AF74
		public void Refresh()
		{
			this.playerMoneyText.text = Player.characterData.inventory.GetCurrencyString(this.shop.data.tradeCurrency);
			foreach (ShopTransactionUILine shopTransactionUILine in this.lines)
			{
				UnityEngine.Object.Destroy(shopTransactionUILine.gameObject);
			}
			this.lines.Clear();
			List<Item> items = null;
			if (this.type == ShopTransactionUI.ReceptType.Buy)
			{
				items = this.shop.buyingItems;
			}
			else if (this.type == ShopTransactionUI.ReceptType.Sell)
			{
				items = this.shop.sellingItems;
			}
			this.emptyMessage.gameObject.SetActive(items.Count == 0);
			new List<ShopTransactionUILine>();
			foreach (Item item in items)
			{
				bool foundExistingShopReceptLine = false;
				foreach (ShopTransactionUILine existingShopReceptLine in this.lines)
				{
					if (existingShopReceptLine.itemHashId == item.data.hashId && existingShopReceptLine.unitPrice == this.shop.GetItemValue(item))
					{
						ShopTransactionUILine shopTransactionUILine2 = existingShopReceptLine;
						int quantity = shopTransactionUILine2.quantity;
						shopTransactionUILine2.quantity = quantity + 1;
						foundExistingShopReceptLine = true;
					}
				}
				if (!foundExistingShopReceptLine)
				{
					ShopTransactionUILine shopReceptLine = UnityEngine.Object.Instantiate<ShopTransactionUILine>(this.shopReceptLinePrefab, this.rootLines);
					shopReceptLine.gameObject.SetActive(true);
					shopReceptLine.itemHashId = item.data.hashId;
					shopReceptLine.itemName = LocalizationManager.GetLocalizedItemName(item.data);
					shopReceptLine.quantity = 1;
					shopReceptLine.unitPrice = this.shop.GetItemValue(item);
					this.lines.Add(shopReceptLine);
				}
			}
			if (this.type != ShopTransactionUI.ReceptType.Buy)
			{
				if (this.type == ShopTransactionUI.ReceptType.Sell)
				{
					if (this.shop.sellingValue > 0)
					{
						this.totalText.text = this.shop.sellingValue.ToString();
						return;
					}
					this.totalText.text = "0";
					this.totalText.color = this.totalOrignalColor;
				}
				return;
			}
			if ((float)this.shop.buyingValue > Player.characterData.inventory.GetCurrencyValue(this.shop.data.tradeCurrency))
			{
				this.totalText.text = this.shop.buyingValue.ToString();
				this.totalText.color = this.notEnoughMoneyTotalColor;
				return;
			}
			if (this.shop.buyingValue > 0)
			{
				this.totalText.text = this.shop.buyingValue.ToString();
				this.totalText.color = this.totalOrignalColor;
				return;
			}
			this.totalText.text = "0";
			this.totalText.color = this.totalOrignalColor;
		}

		// Token: 0x040027A0 RID: 10144
		public Shop shop;

		// Token: 0x040027A1 RID: 10145
		public ShopTransactionUI.ReceptType type;

		// Token: 0x040027A2 RID: 10146
		public Transform rootLines;

		// Token: 0x040027A3 RID: 10147
		public ShopTransactionUILine shopReceptLinePrefab;

		// Token: 0x040027A4 RID: 10148
		public TextMeshProUGUI playerMoneyText;

		// Token: 0x040027A5 RID: 10149
		public TextMeshProUGUI totalText;

		// Token: 0x040027A6 RID: 10150
		public Color notEnoughMoneyTotalColor = Color.red;

		// Token: 0x040027A7 RID: 10151
		public UICustomisableButton buyOrSellButton;

		// Token: 0x040027A8 RID: 10152
		public UICustomisableButton sellAllButton;

		// Token: 0x040027A9 RID: 10153
		public UIText titleUiText;

		// Token: 0x040027AA RID: 10154
		public UIText emptyMessage;

		// Token: 0x040027AB RID: 10155
		public string buyString = "{Buy}";

		// Token: 0x040027AC RID: 10156
		public string sellString = "{Sell}";

		// Token: 0x040027AD RID: 10157
		public string boughtItemsString = "{BoughtItems}";

		// Token: 0x040027AE RID: 10158
		public string SoldItemsString = "{SoldItems}";

		// Token: 0x040027AF RID: 10159
		public string emptyBuyString = "{EmptyBuy}";

		// Token: 0x040027B0 RID: 10160
		public string emptySellString = "{EmptySell}";

		// Token: 0x040027B1 RID: 10161
		protected List<ShopTransactionUILine> lines;

		// Token: 0x040027B2 RID: 10162
		public Transform[] autoDropPoints;

		// Token: 0x040027B3 RID: 10163
		public UnityEvent onBuySuccess;

		// Token: 0x040027B4 RID: 10164
		public UnityEvent onSellSuccess;

		// Token: 0x040027B5 RID: 10165
		public UnityEvent onBuyFail;

		// Token: 0x040027B6 RID: 10166
		public UnityEvent onSellFail;

		// Token: 0x040027B7 RID: 10167
		protected Color totalOrignalColor;

		// Token: 0x02000A88 RID: 2696
		public enum ReceptType
		{
			// Token: 0x04004894 RID: 18580
			Buy,
			// Token: 0x04004895 RID: 18581
			Sell
		}
	}
}
