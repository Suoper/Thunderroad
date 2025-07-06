using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IngameDebugConsole;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200036E RID: 878
	public class Shop : ThunderBehaviour
	{
		// Token: 0x1700027C RID: 636
		// (get) Token: 0x06002994 RID: 10644 RVA: 0x0011AEA9 File Offset: 0x001190A9
		// (set) Token: 0x06002995 RID: 10645 RVA: 0x0011AEB0 File Offset: 0x001190B0
		public static Shop local { get; protected set; }

		// Token: 0x06002996 RID: 10646 RVA: 0x0011AEB8 File Offset: 0x001190B8
		public List<ValueDropdownItem<string>> GetAllShopID()
		{
			return Catalog.GetDropdownAllID(Category.Shop, "None");
		}

		// Token: 0x1700027D RID: 637
		// (get) Token: 0x06002997 RID: 10647 RVA: 0x0011AEC6 File Offset: 0x001190C6
		// (set) Token: 0x06002998 RID: 10648 RVA: 0x0011AECE File Offset: 0x001190CE
		public List<ShopDisplay> displayStands { get; protected set; }

		// Token: 0x1400013B RID: 315
		// (add) Token: 0x06002999 RID: 10649 RVA: 0x0011AED8 File Offset: 0x001190D8
		// (remove) Token: 0x0600299A RID: 10650 RVA: 0x0011AF10 File Offset: 0x00119110
		public event Action<EventTime> onShopContentChanged;

		// Token: 0x1400013C RID: 316
		// (add) Token: 0x0600299B RID: 10651 RVA: 0x0011AF48 File Offset: 0x00119148
		// (remove) Token: 0x0600299C RID: 10652 RVA: 0x0011AF80 File Offset: 0x00119180
		public event Action<Shop.ShopZone, bool> onPlayerChangeZone;

		// Token: 0x1400013D RID: 317
		// (add) Token: 0x0600299D RID: 10653 RVA: 0x0011AFB8 File Offset: 0x001191B8
		// (remove) Token: 0x0600299E RID: 10654 RVA: 0x0011AFF0 File Offset: 0x001191F0
		public event Action<Shop.Transaction, Item, bool> onZoneItemChanged;

		// Token: 0x1400013E RID: 318
		// (add) Token: 0x0600299F RID: 10655 RVA: 0x0011B028 File Offset: 0x00119228
		// (remove) Token: 0x060029A0 RID: 10656 RVA: 0x0011B060 File Offset: 0x00119260
		public event Action<Shop.Transaction, Item> onItemEnterZone;

		// Token: 0x1400013F RID: 319
		// (add) Token: 0x060029A1 RID: 10657 RVA: 0x0011B098 File Offset: 0x00119298
		// (remove) Token: 0x060029A2 RID: 10658 RVA: 0x0011B0D0 File Offset: 0x001192D0
		public event Action<Shop.Transaction> onTransactionStart;

		// Token: 0x14000140 RID: 320
		// (add) Token: 0x060029A3 RID: 10659 RVA: 0x0011B108 File Offset: 0x00119308
		// (remove) Token: 0x060029A4 RID: 10660 RVA: 0x0011B140 File Offset: 0x00119340
		public event Action<Shop.Transaction, bool> onTransactionCompleted;

		// Token: 0x060029A5 RID: 10661 RVA: 0x0011B178 File Offset: 0x00119378
		public void RefreshBuyingAndSellingValues()
		{
			this.sellingValue = 0;
			foreach (Item sellingItem in this.sellingItems)
			{
				this.sellingValue += this.GetItemValue(sellingItem);
			}
			this.buyingValue = 0;
			foreach (Item buyingItem in this.buyingItems)
			{
				this.buyingValue += this.GetItemValue(buyingItem);
			}
		}

		// Token: 0x060029A6 RID: 10662 RVA: 0x0011B238 File Offset: 0x00119438
		public bool BuyItems()
		{
			if (this.buyingItems.Count <= 0)
			{
				return false;
			}
			Action<Shop.Transaction> action = this.onTransactionStart;
			if (action != null)
			{
				action(Shop.Transaction.Buy);
			}
			this.RefreshBuyingAndSellingValues();
			if (Player.characterData.inventory.GetCurrencyValue(this.data.tradeCurrency) < (float)this.buyingValue)
			{
				Debug.Log("Not enough money to buy");
				Action<Shop.Transaction, bool> action2 = this.onTransactionCompleted;
				if (action2 != null)
				{
					action2(Shop.Transaction.Buy, false);
				}
				return false;
			}
			for (int i = this.buyingItems.Count - 1; i >= 0; i--)
			{
				Item item = this.buyingItems[i];
				item.SetOwner(Item.Owner.Player);
				item.RemoveNonStorableModifier(this);
				this.RemovePriceTags(item);
				EventManager.InvokePlayerBoughtItem(item);
			}
			Debug.Log("Buy items for " + this.buyingValue.ToString());
			Player.characterData.inventory.AddCurrencyValue(this.data.tradeCurrency, (float)(-(float)this.buyingValue));
			this.buyingItems.Clear();
			this.buyingValue = 0;
			Action<Shop.Transaction, bool> action3 = this.onTransactionCompleted;
			if (action3 != null)
			{
				action3(Shop.Transaction.Buy, true);
			}
			return true;
		}

		// Token: 0x060029A7 RID: 10663 RVA: 0x0011B354 File Offset: 0x00119554
		public bool SellItems()
		{
			if (this.sellingItems.Count <= 0)
			{
				return false;
			}
			Action<Shop.Transaction> action = this.onTransactionStart;
			if (action != null)
			{
				action(Shop.Transaction.Sell);
			}
			this.RefreshBuyingAndSellingValues();
			if (this.sellingValue == 0)
			{
				Action<Shop.Transaction, bool> action2 = this.onTransactionCompleted;
				if (action2 != null)
				{
					action2(Shop.Transaction.Sell, false);
				}
				return false;
			}
			int orgSellingValue = this.sellingValue;
			for (int i = this.sellingItems.Count - 1; i >= 0; i--)
			{
				Item item = this.sellingItems[i];
				item.SetOwner(Item.Owner.Shopkeeper);
				if (this.GetItemShopCategory(item.data).despawnOnTrade)
				{
					item.Despawn();
				}
				else
				{
					item.AddNonStorableModifier(this);
					this.AddPriceTags(item);
				}
			}
			Debug.Log("Sold items for " + orgSellingValue.ToString());
			Player.characterData.inventory.AddCurrencyValue(this.data.tradeCurrency, (float)orgSellingValue);
			this.sellingItems.Clear();
			this.sellingValue = 0;
			Action<Shop.Transaction, bool> action3 = this.onTransactionCompleted;
			if (action3 != null)
			{
				action3(Shop.Transaction.Sell, true);
			}
			return true;
		}

		// Token: 0x060029A8 RID: 10664 RVA: 0x0011B460 File Offset: 0x00119660
		public void AddPriceTags(Item item)
		{
			this.RemovePriceTags(item);
			List<PriceTagPoint> priceTagPoints = new List<PriceTagPoint>(item.GetComponentsInChildren<PriceTagPoint>(true));
			if (priceTagPoints.Count > 0)
			{
				foreach (PriceTagPoint priceTagPoint in priceTagPoints)
				{
					this.SpawnPriceTag(item, priceTagPoint);
				}
			}
		}

		// Token: 0x060029A9 RID: 10665 RVA: 0x0011B4CC File Offset: 0x001196CC
		protected void SpawnPriceTag(Item item, PriceTagPoint priceTagPoint)
		{
			UnityEngine.Object.Instantiate<GameObject>(this.priceTagPrefab, priceTagPoint.transform.position, priceTagPoint.transform.rotation, priceTagPoint.transform).GetComponent<ShopPriceTag>().Set((float)this.GetItemValue(item), item.data.tier);
		}

		// Token: 0x060029AA RID: 10666 RVA: 0x0011B520 File Offset: 0x00119720
		public void RemovePriceTags(Item item)
		{
			foreach (ShopPriceTag shopPriceTag in new List<ShopPriceTag>(item.GetComponentsInChildren<ShopPriceTag>()))
			{
				UnityEngine.Object.Destroy(shopPriceTag.gameObject);
			}
		}

		// Token: 0x060029AB RID: 10667 RVA: 0x0011B57C File Offset: 0x0011977C
		public float GetItemUsedValueMultiplier(ItemData itemData)
		{
			ShopData.ShopStockCategory itemShopCategory = this.GetItemShopCategory(itemData);
			if (itemShopCategory == null)
			{
				return this.data.defaultUsedValueMultiplier;
			}
			return itemShopCategory.usedValueMultiplier;
		}

		// Token: 0x060029AC RID: 10668 RVA: 0x0011B59C File Offset: 0x0011979C
		public ShopData.ShopStockCategory GetItemShopCategory(ItemData itemData)
		{
			foreach (ShopData.ShopStockCategory category in this.data.shopStockCategories)
			{
				if (category.allAcceptedIDs.Contains(itemData.id))
				{
					return category;
				}
			}
			return null;
		}

		// Token: 0x060029AD RID: 10669 RVA: 0x0011B608 File Offset: 0x00119808
		public float GetItemRealPrice(ItemContent content)
		{
			ContentCustomDataValueOverride containedValue;
			if (!content.TryGetCustomData<ContentCustomDataValueOverride>(out containedValue))
			{
				return content.data.value;
			}
			return containedValue.value;
		}

		// Token: 0x060029AE RID: 10670 RVA: 0x0011B631 File Offset: 0x00119831
		public int GetItemValue(Item item)
		{
			if (item == null)
			{
				return -1;
			}
			return this.GetItemValue(item.GetValue(false).Item2, item.data, item.isUsed);
		}

		// Token: 0x060029AF RID: 10671 RVA: 0x0011B65C File Offset: 0x0011985C
		public int GetItemValue(ItemContent content)
		{
			if (content == null)
			{
				return -1;
			}
			return this.GetItemValue(this.GetItemRealPrice(content), content.data, false);
		}

		// Token: 0x060029B0 RID: 10672 RVA: 0x0011B677 File Offset: 0x00119877
		private int GetItemValue(float realPrice, ItemData itemData, bool isUsed = false)
		{
			return this.RoundUsingShopMethod(realPrice * (isUsed ? this.GetItemUsedValueMultiplier(itemData) : 1f));
		}

		// Token: 0x060029B1 RID: 10673 RVA: 0x0011B694 File Offset: 0x00119894
		private int RoundUsingShopMethod(float unrounded)
		{
			switch (this.data.roundingMethod)
			{
			case ShopData.RoundMethod.RoundNormal:
				return Mathf.RoundToInt(unrounded);
			case ShopData.RoundMethod.RoundUp:
				return Mathf.CeilToInt(unrounded);
			case ShopData.RoundMethod.RoundDown:
				return Mathf.FloorToInt(unrounded);
			default:
				return 0;
			}
		}

		// Token: 0x060029B2 RID: 10674 RVA: 0x0011B6D7 File Offset: 0x001198D7
		[ConsoleMethod("ShopRestock", "Force shop to restock", new string[]
		{

		})]
		public static void ClearAndRestock()
		{
			Shop.local.StoreShopkeeperOwnedItems();
			Shop.local.shopContents.contents.Clear();
			Shop.local.OnShopContentChanged();
		}

		// Token: 0x060029B3 RID: 10675 RVA: 0x0011B701 File Offset: 0x00119901
		[ConsoleMethod("ShopRefresh", "Refresh shop content", new string[]
		{

		})]
		public static void RefreshContent()
		{
			Shop.local.StoreShopkeeperOwnedItems();
			Shop.local.OnShopContentChanged();
		}

		// Token: 0x060029B4 RID: 10676 RVA: 0x0011B718 File Offset: 0x00119918
		public void StoreShopkeeperOwnedItems()
		{
			for (int i = Item.allActive.Count - 1; i >= 0; i--)
			{
				Item item = Item.allActive[i];
				if ((!(item.holder != null) || !(item.holder.parentItem != null)) && item.owner == Item.Owner.Shopkeeper)
				{
					this.shopContents.AddItemContent(item, true, null, null);
				}
			}
		}

		// Token: 0x060029B5 RID: 10677 RVA: 0x0011B784 File Offset: 0x00119984
		public void AddStocksFromTable(int stockCount, LootTableBase table, int upgradeLevel)
		{
			for (int i = 0; i < stockCount; i++)
			{
				for (int pickCount = 0; pickCount < 1000; pickCount++)
				{
					ItemData pickedItemData = table.PickOne(upgradeLevel, 0, null);
					if (pickedItemData != null)
					{
						ItemModuleAI itemModuleAI = pickedItemData.GetModule<ItemModuleAI>();
						int existingCount = this.shopContents.contents.Count((ContainerContent c) => c.catalogData.hashId == pickedItemData.hashId);
						if (pickedItemData.type != ItemData.Type.Weapon && pickedItemData.type != ItemData.Type.Shield && pickedItemData.type != ItemData.Type.Quiver && pickedItemData.type != ItemData.Type.Wardrobe)
						{
							this.shopContents.AddItemContent(pickedItemData, null, null);
							Debug.Log("Added " + pickedItemData.id + " to shop contents!");
							break;
						}
						if (itemModuleAI != null && itemModuleAI.weaponHandling == ItemModuleAI.WeaponHandling.OneHanded && pickedItemData.type != ItemData.Type.Shield)
						{
							if (existingCount < 2)
							{
								this.shopContents.AddItemContent(pickedItemData, null, null);
								Debug.Log("Added " + pickedItemData.id + " to shop contents!");
								break;
							}
						}
						else if (existingCount < 1)
						{
							this.shopContents.AddItemContent(pickedItemData, null, null);
							Debug.Log("Added " + pickedItemData.id + " to shop contents!");
							break;
						}
					}
				}
			}
		}

		// Token: 0x060029B6 RID: 10678 RVA: 0x0011B904 File Offset: 0x00119B04
		public void SpawnItemFromInventory(ItemContent stockItem, Action<Item> callback)
		{
			stockItem.Spawn(delegate(Item item)
			{
				this.shopContents.RemoveContent(stockItem);
				item.transform.position = this.transform.position;
				item.transform.rotation = this.transform.rotation;
				Action<Item> callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(item);
			}, Item.Owner.Shopkeeper, false);
		}

		// Token: 0x060029B7 RID: 10679 RVA: 0x0011B948 File Offset: 0x00119B48
		protected void ItemEnterBuyZone(UnityEngine.Object itemObj)
		{
			Item item = itemObj as Item;
			if (item == null)
			{
				return;
			}
			if (item.owner != Item.Owner.Shopkeeper)
			{
				return;
			}
			Action<Shop.Transaction, Item> action = this.onItemEnterZone;
			if (action != null)
			{
				action(Shop.Transaction.Buy, item);
			}
			if (!this.buyingItems.Contains(item))
			{
				this.buyingItems.Add(item);
				this.RefreshBuyingAndSellingValues();
				Action<Shop.Transaction, Item, bool> action2 = this.onZoneItemChanged;
				if (action2 == null)
				{
					return;
				}
				action2(Shop.Transaction.Buy, item, true);
			}
		}

		// Token: 0x060029B8 RID: 10680 RVA: 0x0011B9B0 File Offset: 0x00119BB0
		protected void ItemExitBuyZone(UnityEngine.Object itemObj)
		{
			Item item = itemObj as Item;
			if (item == null)
			{
				return;
			}
			if (this.buyingItems.Contains(item))
			{
				this.buyingItems.Remove(item);
				this.RefreshBuyingAndSellingValues();
				Action<Shop.Transaction, Item, bool> action = this.onZoneItemChanged;
				if (action == null)
				{
					return;
				}
				action(Shop.Transaction.Buy, item, false);
			}
		}

		// Token: 0x060029B9 RID: 10681 RVA: 0x0011B9FC File Offset: 0x00119BFC
		protected void ItemEnterSellZone(UnityEngine.Object itemObj)
		{
			Item item = itemObj as Item;
			if (item == null)
			{
				return;
			}
			if (item.owner == Item.Owner.Shopkeeper)
			{
				return;
			}
			Action<Shop.Transaction, Item> action = this.onItemEnterZone;
			if (action != null)
			{
				action(Shop.Transaction.Sell, item);
			}
			if (item.data.value <= 0f || this.GetItemValue(item) <= 0)
			{
				return;
			}
			if (!this.sellingItems.Contains(item))
			{
				this.sellingItems.Add(item);
				this.RefreshBuyingAndSellingValues();
				Action<Shop.Transaction, Item, bool> action2 = this.onZoneItemChanged;
				if (action2 == null)
				{
					return;
				}
				action2(Shop.Transaction.Sell, item, true);
			}
		}

		// Token: 0x060029BA RID: 10682 RVA: 0x0011BA84 File Offset: 0x00119C84
		protected void ItemExitSellZone(UnityEngine.Object itemObj)
		{
			Item item = itemObj as Item;
			if (item == null)
			{
				return;
			}
			if (this.sellingItems.Contains(item))
			{
				this.sellingItems.Remove(item);
				this.RefreshBuyingAndSellingValues();
				Action<Shop.Transaction, Item, bool> action = this.onZoneItemChanged;
				if (action == null)
				{
					return;
				}
				action(Shop.Transaction.Sell, item, false);
			}
		}

		// Token: 0x060029BB RID: 10683 RVA: 0x0011BAD0 File Offset: 0x00119CD0
		protected void Awake()
		{
			Shop.local = this;
			this.displayStands = new List<ShopDisplay>();
			this.displayStands.AddRange(base.GetComponentsInChildren<ShopDisplay>());
			this.playerBuyZone.playerEnterEvent.AddListener(delegate(UnityEngine.Object A)
			{
				Action<Shop.ShopZone, bool> action = this.onPlayerChangeZone;
				if (action == null)
				{
					return;
				}
				action(Shop.ShopZone.Buy, true);
			});
			this.playerSellZone.playerEnterEvent.AddListener(delegate(UnityEngine.Object A)
			{
				Action<Shop.ShopZone, bool> action = this.onPlayerChangeZone;
				if (action == null)
				{
					return;
				}
				action(Shop.ShopZone.Sell, true);
			});
			this.playerStoreZone.playerEnterEvent.AddListener(delegate(UnityEngine.Object A)
			{
				Action<Shop.ShopZone, bool> action = this.onPlayerChangeZone;
				if (action == null)
				{
					return;
				}
				action(Shop.ShopZone.Store, true);
			});
			this.playerBuyZone.playerExitEvent.AddListener(delegate(UnityEngine.Object A)
			{
				Action<Shop.ShopZone, bool> action = this.onPlayerChangeZone;
				if (action == null)
				{
					return;
				}
				action(Shop.ShopZone.Buy, false);
			});
			this.playerSellZone.playerExitEvent.AddListener(delegate(UnityEngine.Object A)
			{
				Action<Shop.ShopZone, bool> action = this.onPlayerChangeZone;
				if (action == null)
				{
					return;
				}
				action(Shop.ShopZone.Sell, false);
			});
			this.playerStoreZone.playerExitEvent.AddListener(delegate(UnityEngine.Object A)
			{
				Action<Shop.ShopZone, bool> action = this.onPlayerChangeZone;
				if (action == null)
				{
					return;
				}
				action(Shop.ShopZone.Store, false);
			});
			this.itemBuyZone.itemEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ItemEnterBuyZone));
			this.itemBuyZone.itemExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ItemExitBuyZone));
			this.itemSellZone.itemEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ItemEnterSellZone));
			this.itemSellZone.itemExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ItemExitSellZone));
		}

		// Token: 0x060029BC RID: 10684 RVA: 0x0011BC18 File Offset: 0x00119E18
		public void Init()
		{
			this.data = Catalog.GetData<ShopData>(this.shopDataID, true);
			Transform spawnPoint = this.defaultShopkeeperSpawnPoint;
			if (!Player.characterData.tutorial.IsTutorialCompleted(TutorialManager.TutorialType.Shop))
			{
				spawnPoint = this.tutorialShopkeeperSpawnPoint;
			}
			base.StartCoroutine(Catalog.GetData<CreatureData>(this.data.shopKeeperCreatureID, true).SpawnCoroutine(spawnPoint.position, spawnPoint.eulerAngles.y, null, delegate(Creature creature)
			{
				this.shopkeeper = creature;
				this.moduleShopkeeper = this.shopkeeper.brain.instance.GetModule<BrainModuleShopkeeper>(true);
				this.moduleShopkeeper.SetShop(this);
				this.shopkeeper.OnKillEvent += this.OnShopKeeperKilled;
				if (Player.characterData.tutorial.IsTutorialCompleted(TutorialManager.TutorialType.Shop))
				{
					this.moduleShopkeeper.SetWayPoints(this.idleWaypoints);
					return;
				}
				this.tutorialRunning = true;
				this.shopkeeper.PlayAnimation(this.tutorialOverThereAnimationId, null);
				Player.characterData.tutorial.CompleteTutorial(TutorialManager.TutorialType.Shop);
			}, false, null));
			if (this.shopContents == null)
			{
				this.shopContents = this.GetOrAddComponent<Container>();
			}
			this.shopContents.loadOnStart = false;
			this.shopContents.loadContent = Container.LoadContent.ContainerID;
			this.shopContents.containerID = "Empty";
			if (this.shopContents.contents.IsNullOrEmpty())
			{
				this.shopContents.OnContentLoadedEvent += this.OnShopContentChanged;
				this.shopContents.Load();
				return;
			}
			this.OnShopContentChanged();
		}

		// Token: 0x060029BD RID: 10685 RVA: 0x0011BD11 File Offset: 0x00119F11
		public void StartTutorialSequence()
		{
			if (this.tutorialRunning && this.tutorialCoroutine == null)
			{
				this.tutorialCoroutine = base.StartCoroutine(this.TutorialCoroutine());
			}
		}

		// Token: 0x060029BE RID: 10686 RVA: 0x0011BD35 File Offset: 0x00119F35
		protected IEnumerator TutorialCoroutine()
		{
			this.shopkeeper.StopAnimation(false);
			this.moduleShopkeeper.SpeakTutorial1();
			while (this.moduleShopkeeper.IsInDialog())
			{
				yield return new WaitForSeconds(1f);
			}
			this.moduleShopkeeper.SetWayPoints(this.idleWaypoints);
			yield return new WaitForSeconds(10f);
			this.moduleShopkeeper.SpeakTutorial2();
			while (this.moduleShopkeeper.IsInDialog())
			{
				yield return new WaitForSeconds(1f);
			}
			yield return new WaitForSeconds(1f);
			this.moduleShopkeeper.SpeakTutorial3();
			while (this.moduleShopkeeper.IsInDialog())
			{
				yield return new WaitForSeconds(1f);
			}
			yield return new WaitForSeconds(10f);
			this.tutorialRunning = false;
			this.tutorialCoroutine = null;
			yield break;
		}

		// Token: 0x060029BF RID: 10687 RVA: 0x0011BD44 File Offset: 0x00119F44
		private void OnShopKeeperKilled(CollisionInstance collisionInstance, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				if (this.tutorialCoroutine != null)
				{
					base.StopCoroutine(this.tutorialCoroutine);
					this.tutorialRunning = false;
				}
				base.Invoke("FadeIn", this.shopKeeperKillReloadDuration - 2f);
				base.Invoke("ReloadLevel", this.shopKeeperKillReloadDuration);
			}
		}

		// Token: 0x060029C0 RID: 10688 RVA: 0x0011BD98 File Offset: 0x00119F98
		private void FadeIn()
		{
			CameraEffects.DoFadeEffect(true, 2f);
		}

		// Token: 0x060029C1 RID: 10689 RVA: 0x0011BDA5 File Offset: 0x00119FA5
		private void ReloadLevel()
		{
			LevelManager.ReloadLevel();
		}

		// Token: 0x060029C2 RID: 10690 RVA: 0x0011BDAC File Offset: 0x00119FAC
		private int GetUpgradeLevel(string checkpoints)
		{
			if (checkpoints.IsNullOrEmptyOrWhitespace())
			{
				return 0;
			}
			checkpoints.Replace(" ", string.Empty);
			int[] checkpointInts = (from n in checkpoints.Split(",", StringSplitOptions.None)
			select Convert.ToInt32(n)).ToArray<int>();
			Array.Sort<int>(checkpointInts);
			int upgradeLevel = 0;
			while (upgradeLevel < checkpointInts.Length && this.GetPlayerProgressLevel() >= checkpointInts[upgradeLevel])
			{
				upgradeLevel++;
			}
			return upgradeLevel;
		}

		// Token: 0x060029C3 RID: 10691 RVA: 0x0011BE2C File Offset: 0x0011A02C
		private int GetPlayerProgressLevel()
		{
			CrystalHuntSaveData saveData;
			if (GameModeManager.instance.currentGameMode.TryGetGameModeSaveData<CrystalHuntSaveData>(out saveData))
			{
				return saveData.levelProgression;
			}
			Debug.LogError("Cannot load CrystalHuntSaveData");
			return 0;
		}

		// Token: 0x060029C4 RID: 10692 RVA: 0x0011BE60 File Offset: 0x0011A060
		private bool ShouldRestock(out int day, out int level)
		{
			day = 0;
			level = 0;
			CrystalHuntSaveData saveData;
			if (!GameModeManager.instance.currentGameMode.TryGetGameModeSaveData<CrystalHuntSaveData>(out saveData))
			{
				Debug.LogError("Cannot load CrystalHuntSaveData");
				return false;
			}
			if (saveData.levelProgression > saveData.lastShopRestockLevel || saveData.days - saveData.lastShopRestockDay >= this.data.restockDayInterval || saveData.days < saveData.lastShopRestockDay)
			{
				Debug.Log(string.Format("Restock required!\nGame day is {0} and player level is {1}; last restock day was {2} at level {3}", new object[]
				{
					saveData.days,
					saveData.levelProgression,
					saveData.lastShopRestockDay,
					saveData.lastShopRestockLevel
				}));
				day = saveData.days;
				level = saveData.levelProgression;
				return true;
			}
			return false;
		}

		// Token: 0x060029C5 RID: 10693 RVA: 0x0011BF2C File Offset: 0x0011A12C
		private void SetLastShopRestockInfo(int day, int level)
		{
			CrystalHuntSaveData saveData;
			if (GameModeManager.instance.currentGameMode.TryGetGameModeSaveData<CrystalHuntSaveData>(out saveData))
			{
				saveData.lastShopRestockDay = day;
				saveData.lastShopRestockLevel = level;
				return;
			}
			Debug.LogError("Cannot load CrystalHuntSaveData");
		}

		// Token: 0x060029C6 RID: 10694 RVA: 0x0011BF68 File Offset: 0x0011A168
		protected void FullyStock()
		{
			foreach (ShopData.ShopStockCategory stockCategory in this.data.shopStockCategories)
			{
				int upgradeLevel = this.GetUpgradeLevel(stockCategory.upgradeCheckpoints);
				Debug.Log(string.Format("Pick {0}, item level {1}", stockCategory.stockCategory, upgradeLevel));
				this.AddStocksFromTable(UnityEngine.Random.Range(stockCategory.minMaxStockQuantity.x, stockCategory.minMaxStockQuantity.y + 1), stockCategory.restockTable, upgradeLevel);
			}
		}

		// Token: 0x060029C7 RID: 10695 RVA: 0x0011C00C File Offset: 0x0011A20C
		private void OnShopContentChanged()
		{
			Action<EventTime> action = this.onShopContentChanged;
			if (action != null)
			{
				action(EventTime.OnStart);
			}
			if (this.shopContents.contents == null)
			{
				this.shopContents.contents = new List<ContainerContent>();
			}
			int dayPassed;
			int currentLevel;
			if (this.shopContents.contents.Count == 0)
			{
				Debug.Log("Not shop content in save, restocking...");
				this.FullyStock();
			}
			else if (this.ShouldRestock(out dayPassed, out currentLevel))
			{
				this.shopContents.contents.Clear();
				this.FullyStock();
				this.SetLastShopRestockInfo(dayPassed, currentLevel);
			}
			if (this.data.displaySorting < ShopData.DisplaySorting.Oldest)
			{
				this.shopContents.contents = this.shopContents.contents.OrderByDescending(delegate(ContainerContent c)
				{
					ItemContent itemContent = c as ItemContent;
					return this.GetItemValue((itemContent != null) ? itemContent : null);
				}).ToList<ContainerContent>();
			}
			if (this.data.displaySorting == ShopData.DisplaySorting.Random)
			{
				this.shopContents.contents = this.shopContents.contents.Shuffle<ContainerContent>();
			}
			if (this.data.displaySorting == ShopData.DisplaySorting.Newest)
			{
				this.shopContents.contents = this.shopContents.contents.OrderBy(delegate(ContainerContent c)
				{
					ItemContent itemContent = c as ItemContent;
					return this.GetItemValue((itemContent != null) ? itemContent : null);
				}).ToList<ContainerContent>();
			}
			if (this.data.displaySorting == ShopData.DisplaySorting.Oldest)
			{
				this.shopContents.contents = this.shopContents.contents.OrderByDescending(delegate(ContainerContent c)
				{
					ItemContent itemContent = c as ItemContent;
					return this.GetItemValue((itemContent != null) ? itemContent : null);
				}).ToList<ContainerContent>();
			}
			Action<EventTime> action2 = this.onShopContentChanged;
			if (action2 == null)
			{
				return;
			}
			action2(EventTime.OnEnd);
		}

		// Token: 0x04002776 RID: 10102
		public string shopDataID;

		// Token: 0x04002777 RID: 10103
		[NonSerialized]
		public ShopData data;

		// Token: 0x04002778 RID: 10104
		public Transform defaultShopkeeperSpawnPoint;

		// Token: 0x04002779 RID: 10105
		public Transform tutorialShopkeeperSpawnPoint;

		// Token: 0x0400277A RID: 10106
		[NonSerialized]
		public Creature shopkeeper;

		// Token: 0x0400277B RID: 10107
		[HideInInspector]
		[NonSerialized]
		public BrainModuleShopkeeper moduleShopkeeper;

		// Token: 0x0400277C RID: 10108
		public Container shopContents;

		// Token: 0x0400277D RID: 10109
		public GameObject priceTagPrefab;

		// Token: 0x0400277E RID: 10110
		public string tutorialOverThereAnimationId;

		// Token: 0x0400277F RID: 10111
		public float shopKeeperKillReloadDuration = 10f;

		// Token: 0x04002780 RID: 10112
		[NonSerialized]
		public List<Item> buyingItems = new List<Item>();

		// Token: 0x04002781 RID: 10113
		[NonSerialized]
		public List<Item> sellingItems = new List<Item>();

		// Token: 0x04002782 RID: 10114
		[NonSerialized]
		public int buyingValue;

		// Token: 0x04002783 RID: 10115
		[NonSerialized]
		public int sellingValue;

		// Token: 0x04002784 RID: 10116
		public Transform shopkeeperBuyWaypoint;

		// Token: 0x04002785 RID: 10117
		public Transform shopkeeperSellWaypoint;

		// Token: 0x04002786 RID: 10118
		public Transform shopkeeperStoreWaypoint;

		// Token: 0x04002787 RID: 10119
		public WayPoint[] idleWaypoints;

		// Token: 0x04002788 RID: 10120
		public Zone playerBuyZone;

		// Token: 0x04002789 RID: 10121
		public Zone playerSellZone;

		// Token: 0x0400278A RID: 10122
		public Zone playerStoreZone;

		// Token: 0x0400278B RID: 10123
		public Zone itemBuyZone;

		// Token: 0x0400278C RID: 10124
		public Zone itemSellZone;

		// Token: 0x0400278E RID: 10126
		protected Coroutine tutorialCoroutine;

		// Token: 0x04002795 RID: 10133
		[NonSerialized]
		public bool tutorialRunning;

		// Token: 0x02000A7A RID: 2682
		public enum Transaction
		{
			// Token: 0x0400486B RID: 18539
			Buy,
			// Token: 0x0400486C RID: 18540
			Sell
		}

		// Token: 0x02000A7B RID: 2683
		public enum ShopZone
		{
			// Token: 0x0400486E RID: 18542
			Buy,
			// Token: 0x0400486F RID: 18543
			Sell,
			// Token: 0x04004870 RID: 18544
			Store
		}
	}
}
