using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200036F RID: 879
	public class ShopDisplay : ThunderBehaviour
	{
		// Token: 0x060029D3 RID: 10707 RVA: 0x0011C329 File Offset: 0x0011A529
		private void OnValidate()
		{
			if (!this.shop)
			{
				this.shop = base.GetComponentInParent<Shop>();
			}
		}

		// Token: 0x060029D4 RID: 10708 RVA: 0x0011C344 File Offset: 0x0011A544
		private void Awake()
		{
			if (this.shop == null)
			{
				Debug.LogError("Can't use a shop display if no shop present!");
				base.enabled = false;
				return;
			}
			this.shop.onShopContentChanged += this.ShopLoadedInventory;
		}

		// Token: 0x060029D5 RID: 10709 RVA: 0x0011C37D File Offset: 0x0011A57D
		private void ShopLoadedInventory(EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.SetUpDisplay();
			}
		}

		// Token: 0x060029D6 RID: 10710 RVA: 0x0011C38C File Offset: 0x0011A58C
		private void SetUpDisplay()
		{
			this.includeTransforms.Sort((Transform x, Transform y) => UnityEngine.Random.Range(-1, 2));
			switch (this.displayType)
			{
			case ShopDisplay.DisplayType.Holders:
				this.holders = new List<Holder>();
				this.holders.AddRange(base.GetComponentsInChildren<Holder>());
				for (int h = 0; h < this.includeTransforms.Count; h++)
				{
					this.holders.AddRange(this.includeTransforms[h].GetComponentsInChildren<Holder>());
				}
				break;
			case ShopDisplay.DisplayType.Shelf:
				this.shelves = new List<ItemShelf>();
				this.shelves.AddRange(base.GetComponentsInChildren<ItemShelf>());
				for (int s = 0; s < this.includeTransforms.Count; s++)
				{
					this.shelves.AddRange(this.includeTransforms[s].GetComponentsInChildren<ItemShelf>());
				}
				break;
			case ShopDisplay.DisplayType.Basket:
				this.baskets = new List<ItemBasket>();
				this.baskets.AddRange(base.GetComponentsInChildren<ItemBasket>());
				for (int b = 0; b < this.includeTransforms.Count; b++)
				{
					this.baskets.AddRange(this.includeTransforms[b].GetComponentsInChildren<ItemBasket>());
				}
				break;
			}
			if (this.shop.data.shopStockDictionary.TryGetValue(this.displayCategory, out this.connectedCategory))
			{
				this.displayItems = new List<Item>();
				switch (this.displayType)
				{
				case ShopDisplay.DisplayType.Holders:
					this.HolderSpawnItems();
					break;
				case ShopDisplay.DisplayType.Shelf:
					base.StartCoroutine(this.ShelfSpawnItems());
					break;
				case ShopDisplay.DisplayType.Basket:
					this.BasketSpawnItems();
					break;
				}
				if (this.forceSpawnCount > 0)
				{
					this.ForceSpawn();
				}
			}
		}

		// Token: 0x060029D7 RID: 10711 RVA: 0x0011C548 File Offset: 0x0011A748
		private void ForceSpawn()
		{
			int displayCount = 0;
			for (int i = this.shop.shopContents.contents.Count - 1; i >= 0; i--)
			{
				ItemContent itemContent = this.shop.shopContents.contents[i] as ItemContent;
				if (itemContent != null && this.connectedCategory.allAcceptedIDs.Contains(itemContent.data.id))
				{
					displayCount++;
					this.SpawnNewDisplayItem(itemContent, null);
					if (displayCount >= this.forceSpawnCount)
					{
						break;
					}
				}
			}
		}

		// Token: 0x060029D8 RID: 10712 RVA: 0x0011C5CC File Offset: 0x0011A7CC
		private void HolderSpawnItems()
		{
			if (this.holders.Count <= 0)
			{
				return;
			}
			List<string> keys = new List<string>();
			Dictionary<string, List<Holder>> holderSizeCounts = new Dictionary<string, List<Holder>>();
			for (int h = 0; h < this.holders.Count; h++)
			{
				Holder holder = this.holders[h];
				if (holder.data == null)
				{
					holder.TryLoadFromID();
				}
				for (int s = 0; s < holder.data.slots.Count; s++)
				{
					string slot = holder.data.slots[s];
					if (!holderSizeCounts.ContainsKey(slot))
					{
						holderSizeCounts.Add(slot, new List<Holder>());
						keys.Add(slot);
					}
					holderSizeCounts[slot].Add(holder);
				}
			}
			for (int i = 0; i < keys.Count; i++)
			{
				holderSizeCounts[keys[i]] = (from l in holderSizeCounts[keys[i]]
				orderby l.data.slots.Count
				select l).ToList<Holder>();
			}
			int displayCount = 0;
			for (int j = this.shop.shopContents.contents.Count - 1; j >= 0; j--)
			{
				ItemContent itemContent = this.shop.shopContents.contents[j] as ItemContent;
				List<Holder> fittingHolders;
				if (itemContent != null && this.connectedCategory.allAcceptedIDs.Contains(itemContent.data.id) && !string.IsNullOrEmpty(itemContent.data.slot) && holderSizeCounts.TryGetValue(itemContent.data.slot, out fittingHolders))
				{
					Holder selectedHolder = fittingHolders[0];
					foreach (string allowedSlot in selectedHolder.data.slots)
					{
						List<Holder> holderList;
						if (holderSizeCounts.TryGetValue(allowedSlot, out holderList))
						{
							holderList.Remove(selectedHolder);
						}
					}
					displayCount++;
					this.SpawnNewDisplayItem(itemContent, delegate(Item item)
					{
						selectedHolder.Snap(item, true);
					});
					if (displayCount >= this.holders.Count)
					{
						break;
					}
				}
			}
		}

		// Token: 0x060029D9 RID: 10713 RVA: 0x0011C834 File Offset: 0x0011AA34
		private void BasketSpawnItems()
		{
			this.baskets = this.baskets.OrderBy(delegate(ItemBasket b)
			{
				if (b.allowedItems.Count != 0)
				{
					return (float)b.allowedItems.Count;
				}
				return float.PositiveInfinity;
			}).ToList<ItemBasket>();
			int[] basketItemCounts = new int[this.baskets.Count];
			for (int i = this.shop.shopContents.contents.Count - 1; i >= 0; i--)
			{
				ItemContent itemContent = this.shop.shopContents.contents[i] as ItemContent;
				if (itemContent != null && this.connectedCategory.allAcceptedIDs.Contains(itemContent.data.id))
				{
					int b2;
					int b;
					for (b = 0; b < this.baskets.Count; b = b2 + 1)
					{
						if (this.baskets[b].allowedItems.Contains(itemContent.data.id) || this.baskets[b].allowedItems.Count == 0)
						{
							int height = basketItemCounts[b];
							this.SpawnNewDisplayItem(itemContent, delegate(Item item)
							{
								item.transform.position = this.baskets[b].GetRandomPositionAtHeight(height);
								item.physicBody.velocity = Vector3.zero;
							});
							basketItemCounts[b]++;
							break;
						}
						b2 = b;
					}
				}
			}
		}

		// Token: 0x060029DA RID: 10714 RVA: 0x0011C9C8 File Offset: 0x0011ABC8
		private IEnumerator ShelfSpawnItems()
		{
			List<List<Transform>> shelfSpots = new List<List<Transform>>();
			int totalSpots = 0;
			this.shelves = (from s in this.shelves
			orderby s.shelfSpotSize.sqrMagnitude descending
			select s).ToList<ItemShelf>();
			this.shelves[0].itemMustFitInBounds = false;
			for (int s3 = 0; s3 < this.shelves.Count; s3++)
			{
				shelfSpots.Add(this.shelves[s3].shelfSpots.ToList<Transform>());
				totalSpots += this.shelves[s3].shelfSpots.Count;
			}
			if (shelfSpots.Count == 0)
			{
				yield break;
			}
			int spawningItems = 0;
			int totalChosen = 0;
			List<KeyValuePair<Item, Bounds>> itemBoundsList = new List<KeyValuePair<Item, Bounds>>();
			Action<Item> <>9__2;
			for (int i = this.shop.shopContents.contents.Count - 1; i >= 0; i--)
			{
				ItemContent itemContent = this.shop.shopContents.contents[i] as ItemContent;
				if (itemContent != null && this.connectedCategory.allAcceptedIDs.Contains(itemContent.data.id))
				{
					int k = spawningItems;
					spawningItems = k + 1;
					totalChosen++;
					ItemContent content = itemContent;
					Action<Item> callback;
					if ((callback = <>9__2) == null)
					{
						callback = (<>9__2 = delegate(Item item)
						{
							Rigidbody[] componentsInChildren2 = item.GetComponentsInChildren<Rigidbody>();
							int l;
							for (l = 0; l < componentsInChildren2.Length; l++)
							{
								componentsInChildren2[l].isKinematic = true;
							}
							itemBoundsList.Add(new KeyValuePair<Item, Bounds>(item, Utils.GetCombinedBoundingBox(item.transform)));
							l = spawningItems;
							spawningItems = l - 1;
						});
					}
					this.SpawnNewDisplayItem(content, callback);
					if (totalChosen >= totalSpots)
					{
						IL_1E8:
						while (spawningItems > 0)
						{
							yield return Yielders.EndOfFrame;
						}
						itemBoundsList = (from ibb in itemBoundsList
						orderby ibb.Value.size.sqrMagnitude
						select ibb).ToList<KeyValuePair<Item, Bounds>>();
						for (int j = 0; j < itemBoundsList.Count; j++)
						{
							Item item2 = itemBoundsList[j].Key;
							Bounds value = itemBoundsList[j].Value;
							for (int s2 = this.shelves.Count - 1; s2 >= 0; s2--)
							{
								if (shelfSpots[s2].Count != 0)
								{
									ItemShelf shelf = this.shelves[s2];
									int chosenIndex = shelf.displayAtRandomSpots ? UnityEngine.Random.Range(0, shelfSpots[s2].Count) : 0;
									Transform chosenSpot = shelfSpots[s2][chosenIndex];
									Bounds? itemBounds = null;
									for (int r = 0; r < item2.renderers.Count; r++)
									{
										Renderer renderer = item2.renderers[r];
										if (renderer.gameObject.activeInHierarchy)
										{
											if (itemBounds == null)
											{
												itemBounds = new Bounds?(renderer.bounds);
											}
											else
											{
												itemBounds.Value.Encapsulate(renderer.bounds);
											}
										}
									}
									Transform shelfAligner = item2.transform.FindOrAddTransform("ShelfAligner", itemBounds.Value.center, null, null);
									item2.transform.MoveAlign(shelfAligner, chosenSpot, null);
									UnityEngine.Object.Destroy(shelfAligner.gameObject);
									foreach (Rigidbody rigidbody in item2.GetComponentsInChildren<Rigidbody>())
									{
										rigidbody.isKinematic = false;
										rigidbody.velocity = Vector3.zero;
									}
									if (!shelf.itemMustFitInBounds || shelf.shelfSpotSize.sqrMagnitude >= itemBounds.Value.size.sqrMagnitude)
									{
										shelfSpots[s2].RemoveAt(chosenIndex);
										break;
									}
								}
							}
						}
						yield break;
					}
				}
			}
			goto IL_1E8;
		}

		// Token: 0x060029DB RID: 10715 RVA: 0x0011C9D8 File Offset: 0x0011ABD8
		private void SpawnNewDisplayItem(ItemContent content, Action<Item> callback)
		{
			this.shop.SpawnItemFromInventory(content, delegate(Item item)
			{
				item.isUsed = false;
				this.shop.AddPriceTags(item);
				this.displayItems.Add(item);
				Action<Item> callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(item);
			});
		}

		// Token: 0x04002796 RID: 10134
		public string displayCategory;

		// Token: 0x04002797 RID: 10135
		[NonSerialized]
		public ShopData.ShopStockCategory connectedCategory;

		// Token: 0x04002798 RID: 10136
		public ShopDisplay.DisplayType displayType;

		// Token: 0x04002799 RID: 10137
		public List<Transform> includeTransforms = new List<Transform>();

		// Token: 0x0400279A RID: 10138
		public int forceSpawnCount;

		// Token: 0x0400279B RID: 10139
		[NonSerialized]
		public List<Holder> holders;

		// Token: 0x0400279C RID: 10140
		[NonSerialized]
		public List<ItemShelf> shelves;

		// Token: 0x0400279D RID: 10141
		[NonSerialized]
		public List<ItemBasket> baskets;

		// Token: 0x0400279E RID: 10142
		public Shop shop;

		// Token: 0x0400279F RID: 10143
		[NonSerialized]
		public List<Item> displayItems;

		// Token: 0x02000A80 RID: 2688
		public enum DisplayType
		{
			// Token: 0x0400487B RID: 18555
			Holders,
			// Token: 0x0400487C RID: 18556
			Shelf,
			// Token: 0x0400487D RID: 18557
			Basket
		}
	}
}
