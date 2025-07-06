using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200037F RID: 895
	public class UIItemSpawner : MonoBehaviour
	{
		// Token: 0x17000298 RID: 664
		// (get) Token: 0x06002A8C RID: 10892 RVA: 0x0011FAB3 File Offset: 0x0011DCB3
		// (set) Token: 0x06002A8D RID: 10893 RVA: 0x0011FABB File Offset: 0x0011DCBB
		public UIItemSpawner.ItemInfo selectedItemInfo { get; protected set; }

		// Token: 0x06002A8E RID: 10894 RVA: 0x0011FAC4 File Offset: 0x0011DCC4
		public void ToggleActivateCanvas()
		{
			base.gameObject.SetActive(this.activateCanvas);
			this.activateCanvas = !this.activateCanvas;
		}

		// Token: 0x06002A8F RID: 10895 RVA: 0x0011FAE8 File Offset: 0x0011DCE8
		public void ToggleCanvas()
		{
			CanvasGroup canvasGroup = base.GetComponentInParent<CanvasGroup>();
			if (canvasGroup)
			{
				this.showCanvas = !this.showCanvas;
				canvasGroup.alpha = (float)(this.showCanvas ? 1 : 0);
			}
		}

		// Token: 0x06002A90 RID: 10896 RVA: 0x0011FB28 File Offset: 0x0011DD28
		private void OnEnable()
		{
			Item.OnItemSpawn = (Action<Item>)Delegate.Combine(Item.OnItemSpawn, new Action<Item>(this.OnItemChange));
			Item.OnItemDespawn = (Action<Item>)Delegate.Combine(Item.OnItemDespawn, new Action<Item>(this.OnItemChange));
			Item.OnItemSnap = (Action<Item, Holder>)Delegate.Combine(Item.OnItemSnap, new Action<Item, Holder>(this.OnItemSnap));
			Item.OnItemGrab = (Action<Item, Handle, RagdollHand>)Delegate.Combine(Item.OnItemGrab, new Action<Item, Handle, RagdollHand>(this.OnItemGrabOrUngrab));
			Item.OnItemUngrab = (Action<Item, Handle, RagdollHand>)Delegate.Combine(Item.OnItemUngrab, new Action<Item, Handle, RagdollHand>(this.OnItemGrabOrUngrab));
			Item.onAnyOwnerChange = (Action<Item, Item.Owner, Item.Owner>)Delegate.Combine(Item.onAnyOwnerChange, new Action<Item, Item.Owner, Item.Owner>(this.OnItemOwnerChange));
			this.container.OnContentLoadedEvent += this.OnContentLoadedEvent;
			this.container.OnContentAddEvent += this.OnContentChanged;
			this.container.OnContentRemoveEvent += this.OnContentChanged;
			Zone zone = this.retrieveZone;
			if (zone != null)
			{
				zone.itemEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.OnItemEnterOrExitRetrieveZone));
			}
			Zone zone2 = this.retrieveZone;
			if (zone2 == null)
			{
				return;
			}
			zone2.lastItemExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.OnItemEnterOrExitRetrieveZone));
		}

		// Token: 0x06002A91 RID: 10897 RVA: 0x0011FC80 File Offset: 0x0011DE80
		private void OnDisable()
		{
			Debug.LogFormat(this, "Item spawner - OnDisable", Array.Empty<object>());
			Item.OnItemSpawn = (Action<Item>)Delegate.Remove(Item.OnItemSpawn, new Action<Item>(this.OnItemChange));
			Item.OnItemDespawn = (Action<Item>)Delegate.Remove(Item.OnItemDespawn, new Action<Item>(this.OnItemChange));
			Item.OnItemSnap = (Action<Item, Holder>)Delegate.Remove(Item.OnItemSnap, new Action<Item, Holder>(this.OnItemSnap));
			Item.OnItemGrab = (Action<Item, Handle, RagdollHand>)Delegate.Remove(Item.OnItemGrab, new Action<Item, Handle, RagdollHand>(this.OnItemGrabOrUngrab));
			Item.OnItemUngrab = (Action<Item, Handle, RagdollHand>)Delegate.Remove(Item.OnItemUngrab, new Action<Item, Handle, RagdollHand>(this.OnItemGrabOrUngrab));
			Item.onAnyOwnerChange = (Action<Item, Item.Owner, Item.Owner>)Delegate.Remove(Item.onAnyOwnerChange, new Action<Item, Item.Owner, Item.Owner>(this.OnItemOwnerChange));
			this.container.OnContentLoadedEvent -= this.OnContentLoadedEvent;
			this.container.OnContentAddEvent -= this.OnContentChanged;
			this.container.OnContentRemoveEvent -= this.OnContentChanged;
			Zone zone = this.retrieveZone;
			if (zone != null)
			{
				zone.itemEnterEvent.RemoveListener(new UnityAction<UnityEngine.Object>(this.OnItemEnterOrExitRetrieveZone));
			}
			Zone zone2 = this.retrieveZone;
			if (zone2 == null)
			{
				return;
			}
			zone2.lastItemExitEvent.RemoveListener(new UnityAction<UnityEngine.Object>(this.OnItemEnterOrExitRetrieveZone));
		}

		// Token: 0x06002A92 RID: 10898 RVA: 0x0011FDE5 File Offset: 0x0011DFE5
		private void OnItemOwnerChange(Item item, Item.Owner previousOwner, Item.Owner newOwner)
		{
			if (this.currentTab == UIItemSpawner.Tab.Placed && (previousOwner == Item.Owner.Player || newOwner == Item.Owner.Player))
			{
				this.SetDirty(null);
			}
		}

		// Token: 0x06002A93 RID: 10899 RVA: 0x0011FDFF File Offset: 0x0011DFFF
		private void OnItemGrabOrUngrab(Item item, Handle handle, RagdollHand ragdollHand)
		{
			this.OnItemChange(item);
		}

		// Token: 0x06002A94 RID: 10900 RVA: 0x0011FE08 File Offset: 0x0011E008
		private void OnItemSnap(Item item, Holder holder)
		{
			this.OnItemChange(item);
		}

		// Token: 0x06002A95 RID: 10901 RVA: 0x0011FE11 File Offset: 0x0011E011
		private void OnItemChange(Item item)
		{
			if (this.currentTab == UIItemSpawner.Tab.Placed && item.owner == Item.Owner.Player && (!item.holder || !item.holder.parentPart))
			{
				this.SetDirty(null);
			}
		}

		// Token: 0x06002A96 RID: 10902 RVA: 0x0011FE4C File Offset: 0x0011E04C
		private void Awake()
		{
			this.categoryItemsCount = new Dictionary<string, int>();
			this.categoryGroups = new Dictionary<string, List<GameData.Category>>();
			this.categoryElementDictionary = new Dictionary<GameData.Category, UiItemSpawnerCategoryElement>();
			this.addressableCategoryTextures = new Dictionary<string, Sprite>();
			this.addressableItemTextures = new Dictionary<string, Sprite>();
			this.categoriesCanvases = new List<UIGridRow>();
			this.itemsCanvases = new List<UIGridRow>();
			this.usedItems = new List<UiItemSpawnerItemElement>();
			this.notUsedItems = new List<UiItemSpawnerItemElement>();
			this.usedItemRows = new List<UIGridRow>();
			this.notUsedItemRows = new List<UIGridRow>();
			this.categoriesLayoutCachedTransform = this.categoriesLayout.transform;
			this.categoriesGridRectHeight = this.categoriesScroll.GetComponent<RectTransform>().rect.height;
			this.categoriesGridCellHeight = this.categoriesLayout.GetComponent<GridLayoutGroup>().cellSize.y;
			this.itemsLayoutCachedTransform = this.itemsLayout.transform;
			this.itemsGridRectHeight = this.itemsScroll.GetComponent<RectTransform>().rect.height;
			this.itemsGridCellHeight = this.itemsLayout.GetComponent<GridLayoutGroup>().cellSize.y;
			this.itemElement.gameObject.SetActive(false);
			this.categoriesPage.SetActive(true);
			this.itemsPage.SetActive(true);
			this.itemInfoPage.ToggleInfoPage(false);
			this.placedTabButton.gameObject.SetActive(false);
			this.containerTabButton.gameObject.SetActive(false);
			this.sandboxTabButton.gameObject.SetActive(false);
			this.sandboxTabButton.SetButtonState(false);
			this.containerTabButton.SetButtonState(false);
			this.placedTabButton.SetButtonState(false);
			this.initialized = true;
		}

		// Token: 0x06002A97 RID: 10903 RVA: 0x0011FFFC File Offset: 0x0011E1FC
		private void Start()
		{
			if (this.autoEnableSandboxTab)
			{
				this.showSandboxTab = (GameModeManager.instance.currentGameMode.id == "Sandbox");
			}
			this.placedTabButton.gameObject.SetActive(this.showPlacedTab);
			this.containerTabButton.gameObject.SetActive(this.showContainerTab);
			this.sandboxTabButton.gameObject.SetActive(this.showSandboxTab);
			if (this.showSandboxTab)
			{
				this.sandboxTabButton.toggle.isOn = true;
				this.sandboxTabButton.SetButtonState(true);
				this.SetTab(UIItemSpawner.Tab.Sandbox);
			}
			else if (this.showContainerTab)
			{
				this.containerTabButton.toggle.isOn = true;
				this.containerTabButton.SetButtonState(true);
				this.SetTab(UIItemSpawner.Tab.Container);
			}
			else if (this.showPlacedTab)
			{
				this.placedTabButton.toggle.isOn = true;
				this.placedTabButton.SetButtonState(true);
				this.SetTab(UIItemSpawner.Tab.Placed);
			}
			this.ToggleItemButtons(false);
		}

		// Token: 0x06002A98 RID: 10904 RVA: 0x00120101 File Offset: 0x0011E301
		public void SetSandboxTabVisibility(bool active)
		{
			this.SetTabVisibility(UIItemSpawner.Tab.Sandbox, active);
		}

		// Token: 0x06002A99 RID: 10905 RVA: 0x0012010B File Offset: 0x0011E30B
		public void SetContainerTabVisibility(bool active)
		{
			this.SetTabVisibility(UIItemSpawner.Tab.Container, active);
		}

		// Token: 0x06002A9A RID: 10906 RVA: 0x00120115 File Offset: 0x0011E315
		public void SetPlacedTabVisibility(bool active)
		{
			this.SetTabVisibility(UIItemSpawner.Tab.Placed, active);
		}

		// Token: 0x06002A9B RID: 10907 RVA: 0x00120120 File Offset: 0x0011E320
		public void SetTabVisibility(UIItemSpawner.Tab tab, bool active)
		{
			if (tab == UIItemSpawner.Tab.Sandbox)
			{
				this.showSandboxTab = active;
				this.sandboxTabButton.gameObject.SetActive(active);
				this.sandboxTabButton.toggle.isOn = active;
				if (this.initialized && active)
				{
					this.sandboxTabButton.SetButtonState(true);
					this.SetTab(tab);
					return;
				}
			}
			else if (tab == UIItemSpawner.Tab.Container)
			{
				this.showContainerTab = active;
				this.containerTabButton.gameObject.SetActive(active);
				this.containerTabButton.toggle.isOn = active;
				if (this.initialized && active)
				{
					this.containerTabButton.SetButtonState(true);
					this.SetTab(tab);
					return;
				}
			}
			else if (tab == UIItemSpawner.Tab.Placed)
			{
				this.showPlacedTab = active;
				this.placedTabButton.gameObject.SetActive(active);
				this.placedTabButton.toggle.isOn = active;
				if (this.initialized && active)
				{
					this.placedTabButton.SetButtonState(true);
					this.SetTab(tab);
				}
			}
		}

		// Token: 0x06002A9C RID: 10908 RVA: 0x00120216 File Offset: 0x0011E416
		private void OnDestroy()
		{
			this.ReleaseAddressableCategoryTextures();
			this.ReleaseAddressableItemTextures();
		}

		// Token: 0x06002A9D RID: 10909 RVA: 0x00120224 File Offset: 0x0011E424
		protected void OnDrawGizmos()
		{
			if (this.spawnPoint)
			{
				Gizmos.DrawWireSphere(this.spawnPoint.position, 0.1f);
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(0.402f, 0.29f, 0f));
		}

		/// <summary>
		/// Hacky workaround to reset this nested prefab anchors to make sure
		/// the UI content is rendered inside the book mesh of the parent prefab
		/// </summary>
		// Token: 0x06002A9E RID: 10910 RVA: 0x00120281 File Offset: 0x0011E481
		private IEnumerator ResetAnchors()
		{
			yield return null;
			base.gameObject.SetActive(false);
			base.gameObject.SetActive(true);
			yield break;
		}

		// Token: 0x06002A9F RID: 10911 RVA: 0x00120290 File Offset: 0x0011E490
		public void OnContentLoadedEvent()
		{
			this.UpdateItemsPageTitle("All");
			this.SetDirty(null);
		}

		// Token: 0x06002AA0 RID: 10912 RVA: 0x001202A4 File Offset: 0x0011E4A4
		public void OnContentChanged(ContainerContent content, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.SetDirty(null);
			}
		}

		/// <summary>
		/// Enable and disable the categories' grid elements canvas when they enter and leave the book page while being scrolled.
		/// This method is called from the Categories scroll rect OnValueChanged callback
		/// </summary>
		// Token: 0x06002AA1 RID: 10913 RVA: 0x001202B4 File Offset: 0x0011E4B4
		public void CheckCategoriesCanvasesVisibility()
		{
			Vector3 localPosition = this.categoriesLayoutCachedTransform.localPosition;
			for (int i = 0; i < this.categoriesCanvases.Count; i++)
			{
				UIGridRow categoryCanvas = this.categoriesCanvases[i];
				if (categoryCanvas.CachedTransform != null)
				{
					Vector3 cachedTransformLocalPosition = categoryCanvas.CachedTransform.localPosition;
					if (localPosition.y + cachedTransformLocalPosition.y > this.categoriesGridCellHeight * 7f || localPosition.y + cachedTransformLocalPosition.y < -this.categoriesGridRectHeight - this.categoriesGridCellHeight * 7f)
					{
						categoryCanvas.ToggleComponents(false);
					}
					else
					{
						categoryCanvas.ToggleComponents(true);
					}
				}
			}
		}

		/// <summary>
		/// Enable and disable the items' grid elements canvas when they enter and leave the book page while being scrolled.
		/// This method is called from the Items scroll rect OnValueChanged callback
		/// </summary>
		// Token: 0x06002AA2 RID: 10914 RVA: 0x0012035C File Offset: 0x0011E55C
		public void CheckItemsCanvasesVisibility()
		{
			Vector3 localPosition = this.itemsLayoutCachedTransform.localPosition;
			for (int i = 0; i < this.itemsCanvases.Count; i++)
			{
				UIGridRow itemCanvas = this.itemsCanvases[i];
				Vector3 cachedTransformLocalPosition = itemCanvas.CachedTransform.localPosition;
				if (localPosition.y + cachedTransformLocalPosition.y > this.itemsGridCellHeight * 2f || localPosition.y + cachedTransformLocalPosition.y < -this.itemsGridRectHeight - this.itemsGridCellHeight * 2f)
				{
					itemCanvas.ToggleComponents(false);
				}
				else
				{
					itemCanvas.ToggleComponents(true);
				}
			}
		}

		// Token: 0x06002AA3 RID: 10915 RVA: 0x001203F4 File Offset: 0x0011E5F4
		public bool HasSameCategoryNames(List<string> categoryNames)
		{
			return (from x in this.currentCategoryNames
			orderby x
			select x).SequenceEqual(from x in categoryNames
			orderby x
			select x);
		}

		// Token: 0x06002AA4 RID: 10916 RVA: 0x00120458 File Offset: 0x0011E658
		public UiItemSpawnerCategoryElement RefreshCategories()
		{
			List<string> containerCategoryNames = new List<string>();
			HashSet<UIItemSpawner.ItemInfo> filteredContent = this.GetFilteredContent();
			Dictionary<string, int> numberItemPerCategory = new Dictionary<string, int>();
			foreach (UIItemSpawner.ItemInfo itemInfo in filteredContent)
			{
				string category = itemInfo.data.category;
				if (category == null)
				{
					Debug.LogError("cannot add item " + itemInfo.data.id + " to UIItemSpawner, its category is null.");
				}
				else
				{
					if (!containerCategoryNames.Contains(category))
					{
						containerCategoryNames.Add(category);
					}
					numberItemPerCategory.TryAdd(category, 0);
					Dictionary<string, int> dictionary = numberItemPerCategory;
					string key = category;
					int num = dictionary[key];
					dictionary[key] = num + 1;
				}
			}
			if (this.currentCategoryNames.Count > 0 && this.HasSameCategoryNames(containerCategoryNames))
			{
				foreach (KeyValuePair<GameData.Category, UiItemSpawnerCategoryElement> pair in this.categoryElementDictionary)
				{
					if (!(pair.Value == null))
					{
						int count;
						if (numberItemPerCategory.TryGetValue(pair.Key.name, out count))
						{
							pair.Value.SetCount(count);
							pair.Key.itemsCount = count;
						}
						else
						{
							pair.Value.SetCount(0);
							pair.Key.itemsCount = 0;
						}
					}
				}
				return this.selectedCategoryElement;
			}
			this.currentCategoryNames = containerCategoryNames;
			this.categoryGroups.Clear();
			foreach (GameData.Category category2 in Catalog.gameData.categories)
			{
				bool exist = this.currentCategoryNames.Contains(category2.name);
				if ((!this.showExistingOnly || exist) && (!(category2.name == "Apparels") || this.showArmors))
				{
					if (!this.categoryGroups.ContainsKey(category2.group))
					{
						this.categoryGroups.Add(category2.group, new List<GameData.Category>());
					}
					this.categoryGroups[category2.group].Add(category2);
				}
			}
			this.categoriesCanvases.Clear();
			foreach (object obj in this.categoriesLayout.transform)
			{
				Transform child = (Transform)obj;
				if (!(child.gameObject == this.categoryElement.gameObject) && !(child.gameObject == this.categoriesTitle.gameObject) && !(child.gameObject == this.categoriesRow.gameObject) && !(child.gameObject == this.categoriesSpace.gameObject))
				{
					UnityEngine.Object.Destroy(child.gameObject);
				}
			}
			this.categoryElement.gameObject.SetActive(true);
			this.categoriesTitle.gameObject.SetActive(true);
			this.categoriesRow.gameObject.SetActive(true);
			this.categoryElementDictionary.Clear();
			Dictionary<string, List<GameData.Category>>.KeyCollection keys = this.categoryGroups.Keys;
			int rowCount = 0;
			UiItemSpawnerCategoryElement firstCategoryElement = null;
			foreach (string categoryGroup in keys)
			{
				List<GameObject> groupElementList = new List<GameObject>();
				UIGridRow titleRow = UnityEngine.Object.Instantiate<UIGridRow>(this.categoriesTitle);
				titleRow.name = "Title Row " + this.categoriesCanvases.Count.ToString();
				titleRow.transform.SetParent(this.categoriesLayout.transform, false);
				titleRow.GetComponentInChildren<UIText>().SetLocalizationIds("Default", "{" + categoryGroup + "}");
				this.categoriesCanvases.Add(titleRow);
				groupElementList.Add(titleRow.gameObject);
				this.AddCategoriesGridSpaces(3, ref groupElementList);
				UIGridRow row = this.AddGridRow(true);
				groupElementList.Add(row.gameObject);
				rowCount++;
				if (rowCount > 3)
				{
					row.ToggleComponents(false);
				}
				for (int i = 0; i < this.categoryGroups[categoryGroup].Count; i++)
				{
					if (row.IsRowFull())
					{
						this.AddCategoriesGridSpaces(5, ref groupElementList);
						row = this.AddGridRow(true);
						groupElementList.Add(row.gameObject);
					}
					GameData.Category category3 = this.categoryGroups[categoryGroup][i];
					UiItemSpawnerCategoryElement element = this.InstantiateCategory(category3.name, row.gameObject);
					bool exist2 = this.currentCategoryNames.Contains(category3.name);
					this.SetCategory(element, category3);
					element.SetInteractable(exist2);
					if (exist2 && firstCategoryElement == null)
					{
						firstCategoryElement = element;
					}
					row.AddElement(element.gameObject);
					this.categoryElementDictionary.Add(category3, element);
				}
				this.AddCategoriesGridSpaces(4, ref groupElementList);
			}
			foreach (KeyValuePair<GameData.Category, UiItemSpawnerCategoryElement> pair2 in this.categoryElementDictionary)
			{
				if (!(pair2.Value == null))
				{
					int count2;
					if (numberItemPerCategory.TryGetValue(pair2.Key.name, out count2))
					{
						pair2.Value.SetCount(count2);
						pair2.Key.itemsCount = count2;
					}
					else
					{
						pair2.Value.SetCount(0);
						pair2.Key.itemsCount = 0;
					}
				}
			}
			this.categoryElement.gameObject.SetActive(false);
			this.categoriesTitle.gameObject.SetActive(false);
			this.categoriesRow.gameObject.SetActive(false);
			this.categoriesLayout.allowSwitchOff = false;
			this.categoriesScroll.ResetPosition();
			return firstCategoryElement;
		}

		// Token: 0x06002AA5 RID: 10917 RVA: 0x00120ACC File Offset: 0x0011ECCC
		private IEnumerator WaitToSelectCategory(UiItemSpawnerCategoryElement categoryElement)
		{
			yield return new WaitForEndOfFrame();
			if (categoryElement)
			{
				categoryElement.Button.toggle.isOn = true;
				categoryElement.Button.SetButtonState(true);
			}
			yield break;
		}

		/// <summary>
		/// Instantiate a new grid row
		/// </summary>
		/// <param name="isCategory">True if the row on the categories left page and false if the row is on the items right page</param>
		/// <returns>The canvas of the category row</returns>
		// Token: 0x06002AA6 RID: 10918 RVA: 0x00120ADC File Offset: 0x0011ECDC
		private UIGridRow AddGridRow(bool isCategory)
		{
			UIGridRow row = UnityEngine.Object.Instantiate<UIGridRow>(isCategory ? this.categoriesRow : this.itemsRow);
			row.transform.SetParent(isCategory ? this.categoriesLayout.transform : this.itemsLayout.transform, false);
			row.Setup(3);
			if (isCategory)
			{
				row.name = "Row " + this.categoriesCanvases.Count.ToString();
				this.categoriesCanvases.Add(row);
			}
			else
			{
				row.name = "Row " + this.itemsCanvases.Count.ToString();
				this.itemsCanvases.Add(row);
				this.usedItemRows.Add(row);
			}
			return row;
		}

		/// <summary>
		/// Add empty space elements to give the grid a nice layout with dynamics row's height (title height != categories row height)
		/// </summary>
		/// <param name="spaces">Number of spaces to add</param>
		// Token: 0x06002AA7 RID: 10919 RVA: 0x00120BA0 File Offset: 0x0011EDA0
		private void AddCategoriesGridSpaces(int spaces, ref List<GameObject> list)
		{
			for (int i = 1; i <= spaces; i++)
			{
				list.Add(UnityEngine.Object.Instantiate<GameObject>(this.categoriesSpace, this.categoriesLayout.transform));
			}
		}

		// Token: 0x06002AA8 RID: 10920 RVA: 0x00120BD8 File Offset: 0x0011EDD8
		private UiItemSpawnerCategoryElement InstantiateCategory(string category, GameObject row)
		{
			UiItemSpawnerCategoryElement uiItemSpawnerCategoryElement = UnityEngine.Object.Instantiate<UiItemSpawnerCategoryElement>(this.categoryElement);
			string localizedName = LocalizationManager.Instance.GetLocalizedString("Default", category, false);
			uiItemSpawnerCategoryElement.name = (localizedName ?? category);
			uiItemSpawnerCategoryElement.gameObject.SetActive(true);
			uiItemSpawnerCategoryElement.transform.SetParent(row.transform, false);
			return uiItemSpawnerCategoryElement;
		}

		// Token: 0x06002AA9 RID: 10921 RVA: 0x00120C2C File Offset: 0x0011EE2C
		private void ReleaseAddressableCategoryTextures()
		{
			foreach (KeyValuePair<string, Sprite> entry in this.addressableCategoryTextures)
			{
				if (entry.Value)
				{
					Catalog.ReleaseAsset<Sprite>(entry.Value);
				}
			}
			this.addressableCategoryTextures.Clear();
		}

		// Token: 0x06002AAA RID: 10922 RVA: 0x00120CA0 File Offset: 0x0011EEA0
		private void ReleaseAddressableItemTextures()
		{
			foreach (KeyValuePair<string, Sprite> entry in this.addressableItemTextures)
			{
				if (entry.Value)
				{
					Catalog.ReleaseAsset<Sprite>(entry.Value);
				}
			}
			this.addressableItemTextures.Clear();
		}

		// Token: 0x06002AAB RID: 10923 RVA: 0x00120D14 File Offset: 0x0011EF14
		private void SetCategory(UiItemSpawnerCategoryElement categoryElement, GameData.Category categoryData)
		{
			categoryData.itemsCount = this.categoryItemsCount.GetValueOrDefault(categoryData.name, 0);
			Sprite texture;
			if (this.addressableCategoryTextures.TryGetValue(categoryData.name, out texture))
			{
				categoryElement.SetCategory(this, categoryData, texture, this.categoriesLayout);
				return;
			}
			Catalog.LoadAssetAsync<Sprite>(categoryData.iconLocation, delegate(Sprite sprite)
			{
				if (!sprite && !this.addressableCategoryTextures.TryAdd(categoryData.name, sprite))
				{
					Catalog.ReleaseAsset<Sprite>(sprite);
				}
				if (categoryElement)
				{
					categoryElement.SetCategory(this, categoryData, sprite, this.categoriesLayout);
				}
			}, "Inventory");
		}

		// Token: 0x06002AAC RID: 10924 RVA: 0x00120DB4 File Offset: 0x0011EFB4
		public void OnCategoryChanged(string name, UiItemSpawnerCategoryElement categoryElement)
		{
			if (this.isDirty)
			{
				return;
			}
			this.selectedCategoryElement = categoryElement;
			if (this.selectedCategoryElement.Button.toggle.isOn)
			{
				this.SetDirty(name);
			}
			else if (!categoryElement.Button.toggle.group.ActiveToggles().FirstOrDefault<Toggle>())
			{
				this.UpdateItemsPageTitle("All");
				this.SetDirty(null);
			}
			this.selectedItemInfo = null;
			this.HidePageItemInfo();
			this.ToggleItemButtons(false);
			this.itemsScroll.ResetPosition();
		}

		// Token: 0x06002AAD RID: 10925 RVA: 0x00120E43 File Offset: 0x0011F043
		private void UpdateItemsPageTitle(string itemsCategory)
		{
			this.itemsPageTitle.SetLocalizationIds("Default", "{" + itemsCategory + "}");
		}

		// Token: 0x06002AAE RID: 10926 RVA: 0x00120E68 File Offset: 0x0011F068
		private HashSet<UIItemSpawner.ItemInfo> GetFilteredContent()
		{
			HashSet<UIItemSpawner.ItemInfo> itemInfoList = new HashSet<UIItemSpawner.ItemInfo>();
			if (this.currentTab == UIItemSpawner.Tab.Placed)
			{
				int count = this.container.contents.Count;
				for (int i = 0; i < count; i++)
				{
					ItemContent itemContent = this.container.contents[i] as ItemContent;
					if (itemContent != null && ((itemContent != null && itemContent.state is ContentStatePlaced) || (itemContent != null && itemContent.state is ContentStateHolder)))
					{
						itemInfoList.Add(new UIItemSpawner.ItemInfo(itemContent));
					}
				}
				if (this.showInstancedPlacedItem)
				{
					for (int j = Item.allActive.Count - 1; j >= 0; j--)
					{
						Item item = Item.allActive[j];
						if (item.data != null && item.owner == Item.Owner.Player && !item.IsHanded() && (!item.holder || !item.holder.parentPart))
						{
							itemInfoList.Add(new UIItemSpawner.ItemInfo(item));
						}
					}
				}
			}
			else if (this.currentTab == UIItemSpawner.Tab.Container)
			{
				int count2 = this.container.contents.Count;
				for (int k = 0; k < count2; k++)
				{
					ItemContent itemContent2 = this.container.contents[k] as ItemContent;
					if (itemContent2 != null && (itemContent2 == null || !(itemContent2.state is ContentStatePlaced)) && (itemContent2 == null || !(itemContent2.state is ContentStateHolder)))
					{
						itemInfoList.Add(new UIItemSpawner.ItemInfo(itemContent2));
					}
				}
			}
			else if (this.currentTab == UIItemSpawner.Tab.Sandbox)
			{
				foreach (CatalogData catalogData in Catalog.GetDataList(Category.Item))
				{
					ItemData itemData = catalogData as ItemData;
					if (itemData != null && itemData.allowedStorage.HasFlag(ItemData.Storage.SandboxAllItems) && itemData != null && itemData.prefabLocation != null)
					{
						itemInfoList.Add(new UIItemSpawner.ItemInfo(new ItemContent(itemData, null, null, 1)));
					}
				}
			}
			return itemInfoList;
		}

		// Token: 0x06002AAF RID: 10927 RVA: 0x0012107C File Offset: 0x0011F27C
		public void RefreshItems()
		{
			this.SetDirty(null);
		}

		// Token: 0x06002AB0 RID: 10928 RVA: 0x00121085 File Offset: 0x0011F285
		public IEnumerator RefreshItemsCoroutine(string storageCategoryFilter = null)
		{
			UIItemSpawner.ItemInfo selectedItemInfo = this.selectedItemInfo;
			this.selectedItemInfo = null;
			yield return this.ReleaseItems();
			this.itemsCanvases.Clear();
			UIGridRow row = this.AddGridRow(false);
			int rowCount = 1;
			HashSet<UIItemSpawner.ItemInfo> itemInfoList = this.GetFilteredContent();
			this.textPlacedCount.text = string.Format("{0} / {1}", itemInfoList.Count, Catalog.gameData.platformParameters.maxHomeItem);
			this.textPlacedCount.color = ((itemInfoList.Count > Catalog.gameData.platformParameters.maxHomeItem) ? Color.red : Color.black);
			int index = 0;
			foreach (UIItemSpawner.ItemInfo itemInfo in from content in itemInfoList
			orderby content.data.tier, content.data.mass
			select content)
			{
				string category = itemInfo.data.category;
				if (!string.IsNullOrEmpty(category) && (!(category == "Apparels") || this.showArmors) && (storageCategoryFilter == null || !(storageCategoryFilter != category)))
				{
					int num;
					if (row.IsRowFull())
					{
						row = this.AddGridRow(false);
						num = rowCount;
						rowCount = num + 1;
						if (rowCount > 3)
						{
							row.ToggleComponents(false);
						}
					}
					UiItemSpawnerItemElement item = this.GetItem();
					this.usedItems.Add(item);
					this.SetItem(item, itemInfo, selectedItemInfo == itemInfo);
					row.AddElement(item.gameObject);
					if (index % 10 == 0)
					{
						yield return null;
					}
					num = index;
					index = num + 1;
				}
			}
			IEnumerator<UIItemSpawner.ItemInfo> enumerator = null;
			if (this.selectedItemInfo == null)
			{
				this.ToggleItemButtons(false);
			}
			yield break;
			yield break;
		}

		/// <summary>
		/// Release previous category items and rows
		/// </summary>
		// Token: 0x06002AB1 RID: 10929 RVA: 0x0012109B File Offset: 0x0011F29B
		private IEnumerator ReleaseItems()
		{
			foreach (UiItemSpawnerItemElement item in this.usedItems)
			{
				item.gameObject.SetActive(false);
				item.transform.SetParent(this.itemObjectsPool.transform);
				item.ResetItem();
				if (this.notUsedItems.Count < 45)
				{
					this.notUsedItems.Add(item);
				}
				else
				{
					UnityEngine.Object.Destroy(item.gameObject);
				}
			}
			this.usedItems.Clear();
			yield return null;
			foreach (UIGridRow row in this.usedItemRows)
			{
				row.gameObject.SetActive(false);
				row.transform.SetParent(this.itemObjectsPool.transform);
				if (this.notUsedItemRows.Count < 15)
				{
					this.notUsedItemRows.Add(row);
				}
				else
				{
					UnityEngine.Object.Destroy(row.gameObject);
				}
			}
			this.usedItemRows.Clear();
			yield return null;
			yield break;
		}

		// Token: 0x06002AB2 RID: 10930 RVA: 0x001210AC File Offset: 0x0011F2AC
		private UiItemSpawnerItemElement GetItem()
		{
			UiItemSpawnerItemElement item;
			if (this.notUsedItems.Count > 0)
			{
				item = this.notUsedItems[0];
				this.notUsedItems.RemoveAt(0);
			}
			else
			{
				item = UnityEngine.Object.Instantiate<UiItemSpawnerItemElement>(this.itemElement);
			}
			item.gameObject.SetActive(true);
			item.transform.SetParent(this.itemsLayout.transform, false);
			return item;
		}

		// Token: 0x06002AB3 RID: 10931 RVA: 0x00121114 File Offset: 0x0011F314
		private void SetItem(UiItemSpawnerItemElement itemElement, UIItemSpawner.ItemInfo itemInfo, bool isSelected)
		{
			bool showCount = false;
			if (this.currentTab != UIItemSpawner.Tab.Sandbox)
			{
				showCount = (this.container.allowStackItem && itemInfo.data.isStackable && !itemInfo.isItem && itemInfo.itemContent.quantity > 1);
			}
			Sprite texture2;
			if (this.addressableItemTextures.TryGetValue(itemInfo.data.id, out texture2))
			{
				itemElement.SetItem(this, itemInfo, texture2, isSelected, showCount);
				return;
			}
			itemInfo.data.LoadIconAsync(true, delegate(Sprite texture)
			{
				if (!texture && !this.addressableItemTextures.TryAdd(itemInfo.data.id, texture))
				{
					Catalog.ReleaseAsset<Sprite>(texture);
				}
				if (this.itemElement)
				{
					itemElement.SetItem(this, itemInfo, texture, isSelected, showCount);
				}
			});
		}

		// Token: 0x06002AB4 RID: 10932 RVA: 0x001211FC File Offset: 0x0011F3FC
		public virtual void OnItemChanged(UIItemSpawner.ItemInfo itemInfo, UiItemSpawnerItemElement itemElement)
		{
			if (itemElement.Button.toggle.isOn)
			{
				if (this.selectedItemElement != null && this.selectedItemElement != itemElement)
				{
					this.selectedItemElement.Button.toggle.isOn = false;
				}
				this.selectedItemInfo = itemInfo;
				this.selectedItemElement = itemElement;
				this.ToggleItemButtons(true);
			}
			else if (!itemElement.Button.toggle.group.ActiveToggles().FirstOrDefault<Toggle>())
			{
				this.selectedItemInfo = null;
				this.ToggleItemButtons(false);
			}
			this.RefreshRetrieveAndDespawnButtons();
		}

		// Token: 0x06002AB5 RID: 10933 RVA: 0x00121299 File Offset: 0x0011F499
		protected void OnItemEnterOrExitRetrieveZone(UnityEngine.Object itemObj)
		{
			this.RefreshRetrieveAndDespawnButtons();
		}

		// Token: 0x06002AB6 RID: 10934 RVA: 0x001212A4 File Offset: 0x0011F4A4
		protected void RefreshTabs()
		{
			if (this.currentTab == UIItemSpawner.Tab.Placed)
			{
				this.placedViewTitle.SetActive(true);
				this.storageViewTitle.SetActive(false);
				this.sandboxViewTitle.SetActive(false);
				this.textPlacedCount.transform.parent.gameObject.SetActive(true);
				this.retrieveAllButton.gameObject.SetActive(false);
				this.despawnAllButton.gameObject.SetActive(false);
			}
			else if (this.currentTab == UIItemSpawner.Tab.Container)
			{
				this.placedViewTitle.SetActive(false);
				this.storageViewTitle.SetActive(true);
				this.sandboxViewTitle.SetActive(false);
				this.textPlacedCount.transform.parent.gameObject.SetActive(false);
				this.retrieveAllButton.gameObject.SetActive(!this.frozeContents);
				this.despawnAllButton.gameObject.SetActive(false);
			}
			else if (this.currentTab == UIItemSpawner.Tab.Sandbox)
			{
				this.placedViewTitle.SetActive(false);
				this.storageViewTitle.SetActive(false);
				this.sandboxViewTitle.SetActive(true);
				this.textPlacedCount.transform.parent.gameObject.SetActive(false);
				this.retrieveAllButton.gameObject.SetActive(false);
				this.despawnAllButton.gameObject.SetActive(true);
			}
			this.RefreshRetrieveAndDespawnButtons();
		}

		// Token: 0x06002AB7 RID: 10935 RVA: 0x00121408 File Offset: 0x0011F608
		protected void RefreshRetrieveAndDespawnButtons()
		{
			this.retrieveAllButton.IsInteractable = false;
			this.despawnAllButton.IsInteractable = false;
			if (this.retrieveZone)
			{
				foreach (KeyValuePair<Item, int> itemDic in this.retrieveZone.itemsInZone)
				{
					if (this.ItemCanBeDespawned(itemDic.Key))
					{
						this.despawnAllButton.IsInteractable = true;
					}
					if (this.ItemCanBeRetrieved(itemDic.Key))
					{
						this.retrieveAllButton.IsInteractable = true;
					}
				}
			}
		}

		// Token: 0x06002AB8 RID: 10936 RVA: 0x001214B4 File Offset: 0x0011F6B4
		public Color GetTierColor(int tier)
		{
			if (tier < 0 || tier >= Catalog.gameData.tierColors.Length)
			{
				return Color.white;
			}
			return Catalog.gameData.tierColors[tier];
		}

		// Token: 0x06002AB9 RID: 10937 RVA: 0x001214E0 File Offset: 0x0011F6E0
		public void ShowPageItemInfo()
		{
			if (this.selectedItemInfo == null)
			{
				return;
			}
			this.categoriesPage.SetActive(false);
			this.itemInfoPage.SetItemInfo(this.selectedItemInfo.data);
			this.itemInfoPage.ToggleInfoPage(true);
			UICustomisableButton uicustomisableButton = this.infoButton;
			if (uicustomisableButton != null)
			{
				uicustomisableButton.gameObject.SetActive(false);
			}
			UICustomisableButton uicustomisableButton2 = this.backButton;
			if (uicustomisableButton2 == null)
			{
				return;
			}
			uicustomisableButton2.gameObject.SetActive(true);
		}

		// Token: 0x06002ABA RID: 10938 RVA: 0x00121554 File Offset: 0x0011F754
		public void HidePageItemInfo()
		{
			this.categoriesPage.SetActive(true);
			this.itemInfoPage.ToggleInfoPage(false);
			UICustomisableButton uicustomisableButton = this.backButton;
			if (uicustomisableButton != null)
			{
				uicustomisableButton.gameObject.SetActive(false);
			}
			UICustomisableButton uicustomisableButton2 = this.infoButton;
			if (uicustomisableButton2 == null)
			{
				return;
			}
			uicustomisableButton2.gameObject.SetActive(true);
		}

		// Token: 0x06002ABB RID: 10939 RVA: 0x001215A6 File Offset: 0x0011F7A6
		public void SetPlayerCanGrab(bool active)
		{
			Player.local.creature.GetHand(Side.Right).SetBlockGrab(!active, true);
			Player.local.creature.GetHand(Side.Left).SetBlockGrab(!active, true);
		}

		// Token: 0x06002ABC RID: 10940 RVA: 0x001215DC File Offset: 0x0011F7DC
		public void EquipSelectedItem()
		{
			if (this.selectedItemInfo.isItem)
			{
				this.HandleItemSpawn(this.selectedItemInfo.item, true);
			}
			else
			{
				this.selectedItemInfo.itemContent.Spawn(delegate(Item item)
				{
					this.HandleItemSpawn(item, true);
				}, this.frozeContents ? Item.Owner.None : this.container.spawnOwner, true);
			}
			if (this.selectedItemInfo.itemContent != null && this.selectedItemInfo.itemContent.quantity <= 1)
			{
				this.HidePageItemInfo();
			}
		}

		// Token: 0x06002ABD RID: 10941 RVA: 0x00121664 File Offset: 0x0011F864
		public void SpawnSelectedItem()
		{
			if (this.selectedItemInfo.isItem)
			{
				this.HandleItemSpawn(this.selectedItemInfo.item, false);
			}
			else
			{
				this.selectedItemInfo.itemContent.Spawn(delegate(Item item)
				{
					this.HandleItemSpawn(item, false);
				}, this.frozeContents ? Item.Owner.None : this.container.spawnOwner, true);
			}
			if (this.selectedItemInfo.itemContent != null && this.selectedItemInfo.itemContent.quantity <= 1)
			{
				this.HidePageItemInfo();
			}
		}

		// Token: 0x06002ABE RID: 10942 RVA: 0x001216EC File Offset: 0x0011F8EC
		public void RetrieveItems()
		{
			if (this.container && this.retrieveZone && !this.frozeContents)
			{
				bool retrievedSomething = false;
				for (int i = this.retrieveZone.itemsInZone.Count - 1; i >= 0; i--)
				{
					KeyValuePair<Item, int> itemDic = this.retrieveZone.itemsInZone.ElementAt(i);
					if (this.ItemCanBeRetrieved(itemDic.Key))
					{
						this.container.AddItemContent(itemDic.Key, true, null, null);
						retrievedSomething = true;
					}
				}
				if (retrievedSomething)
				{
					this.SetDirty(null);
				}
			}
		}

		// Token: 0x06002ABF RID: 10943 RVA: 0x00121780 File Offset: 0x0011F980
		public void DespawnItems()
		{
			for (int i = this.retrieveZone.itemsInZone.Count - 1; i >= 0; i--)
			{
				KeyValuePair<Item, int> itemDic = this.retrieveZone.itemsInZone.ElementAt(i);
				if (this.ItemCanBeDespawned(itemDic.Key))
				{
					itemDic.Key.Despawn();
				}
			}
			this.RefreshRetrieveAndDespawnButtons();
		}

		// Token: 0x06002AC0 RID: 10944 RVA: 0x001217DD File Offset: 0x0011F9DD
		protected bool ItemCanBeDespawned(Item item)
		{
			return !item.holder && !item.DisallowDespawn;
		}

		// Token: 0x06002AC1 RID: 10945 RVA: 0x001217F9 File Offset: 0x0011F9F9
		protected bool ItemCanBeRetrieved(Item item)
		{
			return !item.holder && item.owner == Item.Owner.Player && item.data.allowedStorage.HasFlag(ItemData.Storage.Container);
		}

		// Token: 0x06002AC2 RID: 10946 RVA: 0x00121835 File Offset: 0x0011FA35
		public void SetSandboxTab()
		{
			this.SetTab(UIItemSpawner.Tab.Sandbox);
		}

		// Token: 0x06002AC3 RID: 10947 RVA: 0x0012183E File Offset: 0x0011FA3E
		public void SetContainerTab()
		{
			this.SetTab(UIItemSpawner.Tab.Container);
		}

		// Token: 0x06002AC4 RID: 10948 RVA: 0x00121847 File Offset: 0x0011FA47
		public void SetPlacedTab()
		{
			this.SetTab(UIItemSpawner.Tab.Placed);
		}

		// Token: 0x06002AC5 RID: 10949 RVA: 0x00121850 File Offset: 0x0011FA50
		public void SetTab(UIItemSpawner.Tab tab)
		{
			if (this.currentTab != tab)
			{
				this.currentTab = tab;
				this.SetDirty(null);
				this.RefreshTabs();
			}
		}

		// Token: 0x06002AC6 RID: 10950 RVA: 0x0012186F File Offset: 0x0011FA6F
		public void SetDirty(string storageCategoryFilter = null)
		{
			if (this.isDirty)
			{
				return;
			}
			if (this.refreshCoroutine != null)
			{
				base.StopCoroutine(this.refreshCoroutine);
			}
			this.refreshCoroutine = base.StartCoroutine(this.RefreshCoroutine(storageCategoryFilter));
			this.isDirty = true;
		}

		// Token: 0x06002AC7 RID: 10951 RVA: 0x001218A8 File Offset: 0x0011FAA8
		private IEnumerator RefreshCoroutine(string storageCategoryFilter = null)
		{
			yield return null;
			while (!Catalog.IsJsonLoaded() || !LocalizationManager.Instance.IsTextDataParsed)
			{
				yield return null;
			}
			UiItemSpawnerCategoryElement firstCategoryElement = this.RefreshCategories();
			if (string.IsNullOrEmpty(storageCategoryFilter))
			{
				if (firstCategoryElement)
				{
					if (firstCategoryElement != this.selectedCategoryElement && !firstCategoryElement.Button.toggle.isOn)
					{
						this.selectedCategoryElement = firstCategoryElement;
						this.isDirty = false;
						this.refreshCoroutine = null;
						yield return this.WaitToSelectCategory(this.selectedCategoryElement);
					}
					storageCategoryFilter = this.selectedCategoryElement.categoryData.name;
				}
				else
				{
					storageCategoryFilter = "";
				}
			}
			Debug.Log("Refreshing Item Spawner with category: " + storageCategoryFilter);
			this.UpdateItemsPageTitle(storageCategoryFilter);
			yield return this.RefreshItemsCoroutine(storageCategoryFilter);
			this.categoriesScroll.UpdateResetPosition();
			this.itemsScroll.UpdateResetPosition();
			this.isDirty = false;
			this.refreshCoroutine = null;
			yield break;
		}

		// Token: 0x06002AC8 RID: 10952 RVA: 0x001218C0 File Offset: 0x0011FAC0
		public UICustomisableButton AddCustomButton(string textGroupId, string textKey, UnityAction call)
		{
			UICustomisableButton uicustomisableButton = UnityEngine.Object.Instantiate<UICustomisableButton>(this.infoButton, this.infoButton.transform.parent);
			uicustomisableButton.onPointerClick = new UnityEvent();
			UIText component = uicustomisableButton.labels[0].GetComponent<UIText>();
			component.textGroupId = textGroupId;
			component.text = "{" + textKey + "}";
			component.Refresh(false);
			uicustomisableButton.onPointerClick.AddListener(call);
			uicustomisableButton.IsInteractable = true;
			uicustomisableButton.gameObject.SetActive(true);
			return uicustomisableButton;
		}

		// Token: 0x06002AC9 RID: 10953 RVA: 0x00121942 File Offset: 0x0011FB42
		private void EnableSpawnButton()
		{
			this.spawnButton.IsInteractable = true;
		}

		// Token: 0x06002ACA RID: 10954 RVA: 0x00121950 File Offset: 0x0011FB50
		private void EnableEquipButton()
		{
			this.equipButton.IsInteractable = true;
		}

		// Token: 0x06002ACB RID: 10955 RVA: 0x0012195E File Offset: 0x0011FB5E
		protected virtual void ToggleItemButtons(bool enable)
		{
			this.infoButton.IsInteractable = enable;
			this.spawnButton.IsInteractable = enable;
			this.equipButton.IsInteractable = enable;
		}

		// Token: 0x06002ACC RID: 10956 RVA: 0x00121984 File Offset: 0x0011FB84
		private void HandleItemSpawn(Item item, bool tryEquip)
		{
			UIItemSpawner.<>c__DisplayClass137_0 CS$<>8__locals1 = new UIItemSpawner.<>c__DisplayClass137_0();
			CS$<>8__locals1.item = item;
			if (CS$<>8__locals1.item == null)
			{
				return;
			}
			EventManager.InvokeStashItemSpawn(CS$<>8__locals1.item);
			if (tryEquip)
			{
				bool startedBlocked = Player.local.creature.GetHand(Side.Right).grabBlocked || Player.local.creature.GetHand(Side.Left).grabBlocked;
				this.SetPlayerCanGrab(true);
				CS$<>8__locals1.despawned = false;
				if (CS$<>8__locals1.item.data.HasModule<ItemModuleWardrobe>())
				{
					CS$<>8__locals1.item.OnDespawnEvent += CS$<>8__locals1.<HandleItemSpawn>g__ItemDespawn|0;
					EventManager.InvokeItemSpawnEquip(CS$<>8__locals1.item);
				}
				if (CS$<>8__locals1.despawned)
				{
					if (this.currentTab != UIItemSpawner.Tab.Sandbox)
					{
						this.container.RemoveContent(CS$<>8__locals1.item.itemId, 1, true);
						this.SetDirty(null);
					}
					return;
				}
				RagdollHand hand = Player.local.creature.GetHand(Pointer.activeSide);
				if (hand.IsGrabbingOrTK())
				{
					hand = hand.otherHand;
				}
				bool grabbed = false;
				if (!hand.IsGrabbingOrTK())
				{
					hand.Grab(CS$<>8__locals1.item.GetMainHandle(hand.side), true, false);
					grabbed = true;
				}
				if (startedBlocked)
				{
					this.SetPlayerCanGrab(false);
				}
				if (grabbed)
				{
					if (this.currentTab != UIItemSpawner.Tab.Sandbox && !this.frozeContents)
					{
						this.container.RemoveContent(CS$<>8__locals1.item.itemId, 1, true);
					}
					return;
				}
			}
			if (CS$<>8__locals1.item.holder)
			{
				CS$<>8__locals1.item.holder.UnSnap(CS$<>8__locals1.item, true);
			}
			CS$<>8__locals1.item.transform.MoveAlign(CS$<>8__locals1.item.spawnPoint, this.spawnPoint, null);
			if (Catalog.gameData.platformParameters.itemSpawnerDelay > 0)
			{
				this.spawnButton.IsInteractable = false;
				this.equipButton.IsInteractable = false;
				base.Invoke("EnableSpawnButton", (float)Catalog.gameData.platformParameters.itemSpawnerDelay);
				base.Invoke("EnableEquipButton", (float)Catalog.gameData.platformParameters.itemSpawnerDelay);
			}
			if (this.currentTab != UIItemSpawner.Tab.Sandbox && !this.frozeContents)
			{
				this.container.RemoveContent(CS$<>8__locals1.item.itemId, 1, true);
				this.SetDirty(null);
			}
		}

		// Token: 0x0400284A RID: 10314
		private const int PAGE_COLUMNS = 3;

		// Token: 0x0400284B RID: 10315
		private const int ITEM_ROWS_POOL_COUNT = 15;

		// Token: 0x0400284C RID: 10316
		private const int ITEM_POOL_COUNT = 45;

		// Token: 0x0400284D RID: 10317
		private const string ARMOR_CATEGORY = "Apparels";

		// Token: 0x0400284E RID: 10318
		[Header("References")]
		public Container container;

		// Token: 0x0400284F RID: 10319
		[SerializeField]
		private Transform spawnPoint;

		// Token: 0x04002850 RID: 10320
		[SerializeField]
		private UiItemSpawnerCategoryElement categoryElement;

		// Token: 0x04002851 RID: 10321
		[SerializeField]
		private UiItemSpawnerItemElement itemElement;

		// Token: 0x04002852 RID: 10322
		[SerializeField]
		private ToggleGroup categoriesLayout;

		// Token: 0x04002853 RID: 10323
		[SerializeField]
		private ToggleGroup itemsLayout;

		// Token: 0x04002854 RID: 10324
		[SerializeField]
		private UIGridRow itemsRow;

		// Token: 0x04002855 RID: 10325
		[SerializeField]
		private UIGridRow categoriesRow;

		// Token: 0x04002856 RID: 10326
		[SerializeField]
		private UIGridRow categoriesTitle;

		// Token: 0x04002857 RID: 10327
		[SerializeField]
		private GameObject categoriesSpace;

		// Token: 0x04002858 RID: 10328
		[SerializeField]
		private GameObject itemObjectsPool;

		// Token: 0x04002859 RID: 10329
		[SerializeField]
		private Zone retrieveZone;

		// Token: 0x0400285A RID: 10330
		[SerializeField]
		private TMP_Text textPlacedCount;

		// Token: 0x0400285B RID: 10331
		[SerializeField]
		private GameObject placedViewTitle;

		// Token: 0x0400285C RID: 10332
		[SerializeField]
		private GameObject storageViewTitle;

		// Token: 0x0400285D RID: 10333
		[SerializeField]
		private GameObject sandboxViewTitle;

		// Token: 0x0400285E RID: 10334
		[SerializeField]
		private UICustomisableButton containerTabButton;

		// Token: 0x0400285F RID: 10335
		[SerializeField]
		private UICustomisableButton placedTabButton;

		// Token: 0x04002860 RID: 10336
		[SerializeField]
		private UICustomisableButton sandboxTabButton;

		// Token: 0x04002861 RID: 10337
		[SerializeField]
		private GameObject categoriesPage;

		// Token: 0x04002862 RID: 10338
		[SerializeField]
		private UIText itemsPageTitle;

		// Token: 0x04002863 RID: 10339
		[SerializeField]
		private GameObject itemsPage;

		// Token: 0x04002864 RID: 10340
		[SerializeField]
		private UIItemSpawnerItemInfoPage itemInfoPage;

		// Token: 0x04002865 RID: 10341
		[SerializeField]
		private UIScrollController categoriesScroll;

		// Token: 0x04002866 RID: 10342
		[SerializeField]
		private UIScrollController itemsScroll;

		// Token: 0x04002867 RID: 10343
		[SerializeField]
		private UICustomisableButton infoButton;

		// Token: 0x04002868 RID: 10344
		[SerializeField]
		private UICustomisableButton spawnButton;

		// Token: 0x04002869 RID: 10345
		[SerializeField]
		private UICustomisableButton equipButton;

		// Token: 0x0400286A RID: 10346
		[SerializeField]
		private UICustomisableButton backButton;

		// Token: 0x0400286B RID: 10347
		[SerializeField]
		private UICustomisableButton retrieveAllButton;

		// Token: 0x0400286C RID: 10348
		[SerializeField]
		private UICustomisableButton despawnAllButton;

		// Token: 0x0400286D RID: 10349
		[Header("Setup")]
		[SerializeField]
		private bool showExistingOnly = true;

		// Token: 0x0400286E RID: 10350
		[SerializeField]
		private bool showArmors = true;

		// Token: 0x0400286F RID: 10351
		[SerializeField]
		private bool showSandboxTab = true;

		// Token: 0x04002870 RID: 10352
		[SerializeField]
		private bool showPlacedTab = true;

		// Token: 0x04002871 RID: 10353
		[SerializeField]
		private bool showContainerTab = true;

		// Token: 0x04002872 RID: 10354
		[SerializeField]
		private bool autoEnableSandboxTab = true;

		// Token: 0x04002873 RID: 10355
		[SerializeField]
		private bool showInstancedPlacedItem;

		// Token: 0x04002874 RID: 10356
		[SerializeField]
		private bool frozeContents;

		// Token: 0x04002875 RID: 10357
		private UIItemSpawner.Tab currentTab;

		// Token: 0x04002876 RID: 10358
		private Dictionary<string, int> categoryItemsCount;

		// Token: 0x04002877 RID: 10359
		private Dictionary<string, List<GameData.Category>> categoryGroups;

		// Token: 0x04002878 RID: 10360
		private Dictionary<GameData.Category, UiItemSpawnerCategoryElement> categoryElementDictionary;

		// Token: 0x04002879 RID: 10361
		private Dictionary<string, Sprite> addressableCategoryTextures;

		// Token: 0x0400287A RID: 10362
		private Dictionary<string, Sprite> addressableItemTextures;

		// Token: 0x0400287B RID: 10363
		private List<UIGridRow> categoriesCanvases;

		// Token: 0x0400287C RID: 10364
		private List<UIGridRow> itemsCanvases;

		// Token: 0x0400287D RID: 10365
		private Transform categoriesLayoutCachedTransform;

		// Token: 0x0400287E RID: 10366
		private float categoriesGridRectHeight;

		// Token: 0x0400287F RID: 10367
		private float categoriesGridCellHeight;

		// Token: 0x04002880 RID: 10368
		private Transform itemsLayoutCachedTransform;

		// Token: 0x04002881 RID: 10369
		private float itemsGridRectHeight;

		// Token: 0x04002882 RID: 10370
		private float itemsGridCellHeight;

		// Token: 0x04002883 RID: 10371
		private List<UiItemSpawnerItemElement> usedItems;

		// Token: 0x04002884 RID: 10372
		private List<UiItemSpawnerItemElement> notUsedItems;

		// Token: 0x04002885 RID: 10373
		private List<UIGridRow> usedItemRows;

		// Token: 0x04002886 RID: 10374
		private List<UIGridRow> notUsedItemRows;

		// Token: 0x04002887 RID: 10375
		private List<string> currentCategoryNames = new List<string>();

		// Token: 0x04002888 RID: 10376
		private Coroutine refreshCoroutine;

		// Token: 0x04002889 RID: 10377
		private UiItemSpawnerCategoryElement selectedCategoryElement;

		// Token: 0x0400288A RID: 10378
		private bool isDirty;

		// Token: 0x0400288B RID: 10379
		private bool initialized;

		// Token: 0x0400288D RID: 10381
		private UiItemSpawnerItemElement selectedItemElement;

		// Token: 0x0400288E RID: 10382
		private bool activateCanvas = true;

		// Token: 0x0400288F RID: 10383
		private bool showCanvas;

		// Token: 0x02000A93 RID: 2707
		public enum Tab
		{
			// Token: 0x040048BF RID: 18623
			None,
			// Token: 0x040048C0 RID: 18624
			Sandbox,
			// Token: 0x040048C1 RID: 18625
			Container,
			// Token: 0x040048C2 RID: 18626
			Placed
		}

		// Token: 0x02000A94 RID: 2708
		public class ItemInfo
		{
			// Token: 0x17000607 RID: 1543
			// (get) Token: 0x060046B1 RID: 18097 RVA: 0x00199EE7 File Offset: 0x001980E7
			public ItemData data
			{
				get
				{
					if (!this.isItem)
					{
						return this.itemContent.data;
					}
					return this.item.data;
				}
			}

			// Token: 0x17000608 RID: 1544
			// (get) Token: 0x060046B2 RID: 18098 RVA: 0x00199F08 File Offset: 0x00198108
			public bool inHolder
			{
				get
				{
					if (!this.isItem)
					{
						return this.itemContent.state is ContentStateHolder;
					}
					return this.item.holder;
				}
			}

			// Token: 0x17000609 RID: 1545
			// (get) Token: 0x060046B3 RID: 18099 RVA: 0x00199F36 File Offset: 0x00198136
			public int quantity
			{
				get
				{
					if (!this.isItem)
					{
						return this.itemContent.quantity;
					}
					return 1;
				}
			}

			// Token: 0x060046B4 RID: 18100 RVA: 0x00199F4D File Offset: 0x0019814D
			public ItemInfo(Item item)
			{
				this.item = item;
				this.isItem = true;
			}

			// Token: 0x060046B5 RID: 18101 RVA: 0x00199F63 File Offset: 0x00198163
			public ItemInfo(ItemContent itemContent)
			{
				this.itemContent = itemContent;
				this.isItem = false;
			}

			// Token: 0x040048C3 RID: 18627
			public Item item;

			// Token: 0x040048C4 RID: 18628
			public ItemContent itemContent;

			// Token: 0x040048C5 RID: 18629
			public bool isItem;
		}
	}
}
