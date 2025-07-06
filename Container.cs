using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200024F RID: 591
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Container.html")]
	[AddComponentMenu("ThunderRoad/Container")]
	public class Container : MonoBehaviour
	{
		// Token: 0x060018DB RID: 6363 RVA: 0x000A3BB6 File Offset: 0x000A1DB6
		public List<ValueDropdownItem<string>> GetAllContainerID()
		{
			return Catalog.GetDropdownAllID(Category.Container, "None");
		}

		// Token: 0x14000093 RID: 147
		// (add) Token: 0x060018DC RID: 6364 RVA: 0x000A3BC4 File Offset: 0x000A1DC4
		// (remove) Token: 0x060018DD RID: 6365 RVA: 0x000A3BFC File Offset: 0x000A1DFC
		public event Container.ContentLoadedEvent OnContentLoadedEvent;

		// Token: 0x14000094 RID: 148
		// (add) Token: 0x060018DE RID: 6366 RVA: 0x000A3C34 File Offset: 0x000A1E34
		// (remove) Token: 0x060018DF RID: 6367 RVA: 0x000A3C6C File Offset: 0x000A1E6C
		public event Container.ContentChangeEvent OnContentAddEvent;

		// Token: 0x14000095 RID: 149
		// (add) Token: 0x060018E0 RID: 6368 RVA: 0x000A3CA4 File Offset: 0x000A1EA4
		// (remove) Token: 0x060018E1 RID: 6369 RVA: 0x000A3CDC File Offset: 0x000A1EDC
		public event Container.ContentChangeEvent OnContentRemoveEvent;

		/// <summary>
		/// Called when the quantity field of a container is set to a higher one
		/// </summary>
		// Token: 0x14000096 RID: 150
		// (add) Token: 0x060018E2 RID: 6370 RVA: 0x000A3D14 File Offset: 0x000A1F14
		// (remove) Token: 0x060018E3 RID: 6371 RVA: 0x000A3D4C File Offset: 0x000A1F4C
		public event Container.ContentChangeEvent OnContentQuantityIncreaseEvent;

		/// <summary>
		/// Called when the quantity field of a container is set to a lower one
		/// </summary>
		// Token: 0x14000097 RID: 151
		// (add) Token: 0x060018E4 RID: 6372 RVA: 0x000A3D84 File Offset: 0x000A1F84
		// (remove) Token: 0x060018E5 RID: 6373 RVA: 0x000A3DBC File Offset: 0x000A1FBC
		public event Container.ContentChangeEvent OnContentQuantityDecreaseEvent;

		// Token: 0x060018E6 RID: 6374 RVA: 0x000A3DF1 File Offset: 0x000A1FF1
		protected void Awake()
		{
			if (!GameManager.local)
			{
				base.enabled = false;
				return;
			}
			this.creature = base.GetComponentInParent<Creature>();
		}

		// Token: 0x060018E7 RID: 6375 RVA: 0x000A3E13 File Offset: 0x000A2013
		protected void Start()
		{
			if (!this.creature && this.loadOnStart)
			{
				EventManager.onLevelLoad += this.OnLevelLoad;
			}
		}

		// Token: 0x060018E8 RID: 6376 RVA: 0x000A3E3C File Offset: 0x000A203C
		private void OnLevelLoad(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				return;
			}
			try
			{
				if (base.enabled && base.gameObject.activeInHierarchy)
				{
					this.Load();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error loading container on level load: {0}", e));
			}
			EventManager.onLevelLoad -= this.OnLevelLoad;
		}

		// Token: 0x060018E9 RID: 6377 RVA: 0x000A3EA0 File Offset: 0x000A20A0
		public void Load()
		{
			List<ContainerContent> savedContents;
			if (!string.IsNullOrEmpty(this.loadPlayerContainerID) && Player.characterData.TryGetContainer(this.loadPlayerContainerID, out savedContents))
			{
				this.Load(savedContents);
				return;
			}
			Container.LoadContent loadContent = this.loadContent;
			if (loadContent == Container.LoadContent.ContainerID)
			{
				this.LoadFromContainerId();
				return;
			}
			if (loadContent != Container.LoadContent.PlayerInventory)
			{
				this.contents = new List<ContainerContent>();
				this.LoadContents();
				return;
			}
			this.LoadFromPlayerInventory();
		}

		// Token: 0x060018EA RID: 6378 RVA: 0x000A3F08 File Offset: 0x000A2108
		public void Load(List<ContainerContent> contents)
		{
			List<ContainerContent> cloneContents = Container.CloneContents(contents);
			if (!this.allowStackItem)
			{
				this.contents = ((cloneContents != null) ? cloneContents : new List<ContainerContent>());
			}
			else
			{
				this.contents = new List<ContainerContent>();
				if (cloneContents != null)
				{
					int count = cloneContents.Count;
					for (int i = 0; i < count; i++)
					{
						this.AddContent<ContainerContent>(cloneContents[i], true);
					}
				}
			}
			this.LoadContents();
		}

		// Token: 0x060018EB RID: 6379 RVA: 0x000A3F70 File Offset: 0x000A2170
		public void LoadFromContainerId()
		{
			ContainerData containerData;
			if (!string.IsNullOrEmpty(this.containerID) && this.containerID != "None" && Catalog.TryGetData<ContainerData>(this.containerID, out containerData, true))
			{
				this.contents = (containerData.GetClonedContents() ?? new List<ContainerContent>());
			}
			this.LoadContents();
		}

		// Token: 0x060018EC RID: 6380 RVA: 0x000A3FC7 File Offset: 0x000A21C7
		public void LoadFromPlayerInventory()
		{
			if (Player.characterData != null)
			{
				this.contents = (Player.characterData.CloneInventory() ?? new List<ContainerContent>());
			}
			this.LoadContents();
		}

		// Token: 0x060018ED RID: 6381 RVA: 0x000A3FF0 File Offset: 0x000A21F0
		public static bool ItemExist(string containerID, ItemData itemData)
		{
			List<ContainerContent> containerContents;
			if (Container.TryGetContent(containerID, out containerContents))
			{
				foreach (ContainerContent containerContent in containerContents)
				{
					if (itemData == containerContent.catalogData)
					{
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x060018EE RID: 6382 RVA: 0x000A4054 File Offset: 0x000A2254
		public static bool TryGetContent(string containerID, out List<ContainerContent> containerContents)
		{
			List<ContainerContent> stashContainerContents;
			if (Player.characterData.TryGetContainer(containerID, out stashContainerContents))
			{
				containerContents = stashContainerContents;
				return true;
			}
			ContainerData stashContainerData;
			if (Catalog.TryGetData<ContainerData>(containerID, out stashContainerData, true))
			{
				containerContents = stashContainerData.containerContents;
				return true;
			}
			containerContents = null;
			return false;
		}

		// Token: 0x060018EF RID: 6383 RVA: 0x000A4090 File Offset: 0x000A2290
		public void ClearLinkedHolders()
		{
			foreach (Holder holder in this.linkedHolders)
			{
				for (int i = holder.items.Count - 1; i >= 0; i--)
				{
					holder.items[i].Despawn();
				}
			}
		}

		// Token: 0x060018F0 RID: 6384 RVA: 0x000A4108 File Offset: 0x000A2308
		private void LoadContents()
		{
			List<ItemContent> placedContentOrderedList = new List<ItemContent>();
			int placedItemCount = 0;
			using (List<ItemContent>.Enumerator enumerator = this.contents.GetContentsOfType(false, Array.Empty<Func<ItemContent, bool>>()).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ItemContent content = enumerator.Current;
					if (content != null)
					{
						ContentStateHolder contentStateHolder = content.state as ContentStateHolder;
						if (contentStateHolder != null)
						{
							using (List<Holder>.Enumerator enumerator2 = this.linkedHolders.GetEnumerator())
							{
								while (enumerator2.MoveNext())
								{
									Holder holder = enumerator2.Current;
									if (holder.name == contentStateHolder.holderName)
									{
										content.Spawn(delegate(Item item)
										{
											item.linkedContainer = this;
											holder.Snap(item, true);
											this.contents.Remove(content);
										}, this.spawnOwner, true);
										placedItemCount++;
										break;
									}
								}
							}
						}
						ContentStatePlaced contentStatePlaced2 = content.state as ContentStatePlaced;
						if (contentStatePlaced2 != null)
						{
							Level current = Level.current;
							string a;
							if (current == null)
							{
								a = null;
							}
							else
							{
								LevelData data = current.data;
								a = ((data != null) ? data.id : null);
							}
							if (a == contentStatePlaced2.levelId)
							{
								placedContentOrderedList.Add(content);
							}
						}
					}
				}
			}
			if (!placedContentOrderedList.IsNullOrEmpty())
			{
				placedContentOrderedList = (from c in placedContentOrderedList
				orderby (c.state as ContentStatePlaced).lastSpawnTime
				select c).ToList<ItemContent>();
				StringBuilder sb = new StringBuilder();
				sb.AppendLine(string.Format("Spawning persistent items from container {0}. Max items: {1}", base.name, Catalog.gameData.platformParameters.maxHomeItem));
				using (List<ItemContent>.Enumerator enumerator = placedContentOrderedList.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ItemContent content = enumerator.Current;
						ContentState state = content.state;
						ContentStatePlaced contentStatePlaced = state as ContentStatePlaced;
						if (contentStatePlaced != null)
						{
							if (PlayerSaveData.playerStashContainerName != this.loadPlayerContainerID || contentStatePlaced.levelId != Player.characterData.mode.data.levelHome || placedItemCount < Catalog.gameData.platformParameters.maxHomeItem)
							{
								content.Spawn(delegate(Item item)
								{
									item.linkedContainer = this;
									item.DisallowDespawn = true;
									item.transform.SetPositionAndRotation(contentStatePlaced.position, contentStatePlaced.rotation);
									item.physicBody.isKinematic = contentStatePlaced.kinematic;
									this.contents.Remove(content);
								}, this.spawnOwner, true);
								placedItemCount++;
								sb.AppendLine(string.Format("[{0}] {1}", placedItemCount, content.data.id));
							}
							else
							{
								sb.AppendLine("[Max Items Hit] " + content.data.id + " cannot spawn");
								content.state = null;
							}
						}
					}
				}
				Debug.Log(sb.ToString());
			}
			this.contentLoaded = true;
			Container.ContentLoadedEvent onContentLoadedEvent = this.OnContentLoadedEvent;
			if (onContentLoadedEvent == null)
			{
				return;
			}
			onContentLoadedEvent();
		}

		// Token: 0x060018F1 RID: 6385 RVA: 0x000A4480 File Offset: 0x000A2680
		public List<ContainerContent> CloneContents()
		{
			return Container.CloneContents(this.contents);
		}

		// Token: 0x060018F2 RID: 6386 RVA: 0x000A4490 File Offset: 0x000A2690
		public static List<ContainerContent> CloneContents(List<ContainerContent> contents)
		{
			List<ContainerContent> clonedContents = new List<ContainerContent>();
			foreach (ContainerContent content in contents)
			{
				if (content != null)
				{
					clonedContents.Add(content.Clone());
				}
			}
			return clonedContents;
		}

		// Token: 0x060018F3 RID: 6387 RVA: 0x000A44F0 File Offset: 0x000A26F0
		public ContainerContent AddDataContent<T>(T data, string type = "data") where T : CatalogData, IContainerLoadable<T>
		{
			if (data == null)
			{
				Debug.LogWarning("Can't add null " + type + " to inventory!");
				return null;
			}
			return this.AddContent<ContainerContent>(data.InstanceContent(), false);
		}

		// Token: 0x060018F4 RID: 6388 RVA: 0x000A4524 File Offset: 0x000A2724
		public bool HasDataContent<T>(T data) where T : CatalogData, IContainerLoadable<T>
		{
			return data != null && this.contents.Exists((ContainerContent content) => content.referenceID == data.id);
		}

		// Token: 0x060018F5 RID: 6389 RVA: 0x000A4564 File Offset: 0x000A2764
		public bool TryGetDataContent<T>(string id, out T instance) where T : CatalogData
		{
			instance = default(T);
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}
			ContainerContent containerContent = this.contents.Find((ContainerContent content) => content.referenceID == id);
			instance = (((containerContent != null) ? containerContent.catalogData : null) as T);
			return instance != null;
		}

		// Token: 0x060018F6 RID: 6390 RVA: 0x000A45D8 File Offset: 0x000A27D8
		public List<ItemData> GetAllWardrobe()
		{
			List<ItemData> wearables = new List<ItemData>();
			for (int i = 0; i < this.contents.Count; i++)
			{
				ItemData data = this.contents[i].catalogData as ItemData;
				if (data != null && data.type == ItemData.Type.Wardrobe)
				{
					wearables.Add(data);
				}
			}
			return wearables;
		}

		// Token: 0x060018F7 RID: 6391 RVA: 0x000A462C File Offset: 0x000A282C
		public T AddDataContent<T, J>(J data, string type = "data") where T : ContainerContent<J, T> where J : CatalogData, IContainerLoadable<J>
		{
			return (T)((object)this.AddDataContent<J>(data, type));
		}

		// Token: 0x060018F8 RID: 6392 RVA: 0x000A463B File Offset: 0x000A283B
		public ItemContent AddItemContent(Item item, bool despawnItem, ContentState state = null, List<ContentCustomData> customDataList = null)
		{
			item.InvokeOnContainerAddEvent(this);
			ItemContent result = this.AddItemContent(item.data, state, (customDataList != null) ? customDataList : item.contentCustomData);
			if (despawnItem)
			{
				item.Despawn();
			}
			return result;
		}

		// Token: 0x060018F9 RID: 6393 RVA: 0x000A4668 File Offset: 0x000A2868
		public ItemContent AddItemContent(string itemId, ContentState state = null, List<ContentCustomData> customDataList = null)
		{
			return this.AddItemContent(Catalog.GetData<ItemData>(itemId, true), state, customDataList);
		}

		// Token: 0x060018FA RID: 6394 RVA: 0x000A4679 File Offset: 0x000A2879
		public ItemContent AddItemContent(ItemData itemData, ContentState state = null, List<ContentCustomData> customDataList = null)
		{
			if (itemData == null)
			{
				Debug.LogWarning("Can't add null item to inventory!");
				return null;
			}
			return this.AddContent<ItemContent>(new ItemContent(itemData.id, state, customDataList, 1), false);
		}

		// Token: 0x060018FB RID: 6395 RVA: 0x000A469F File Offset: 0x000A289F
		public ItemContent AddItemContent(ItemContent itemContent)
		{
			return this.AddContent<ItemContent>(itemContent, false);
		}

		// Token: 0x060018FC RID: 6396 RVA: 0x000A46A9 File Offset: 0x000A28A9
		public bool HasSkillContent(string skillID)
		{
			return this.HasSkillContent(Catalog.GetData<SkillData>(skillID, true));
		}

		// Token: 0x060018FD RID: 6397 RVA: 0x000A46B8 File Offset: 0x000A28B8
		public bool HasSkillContent(SkillData skillData)
		{
			return this.HasDataContent<SkillData>(skillData);
		}

		// Token: 0x060018FE RID: 6398 RVA: 0x000A46C1 File Offset: 0x000A28C1
		public bool TryGetSkillContent<T>(string skillID, out T skillData) where T : SkillData
		{
			return this.TryGetDataContent<T>(skillID, out skillData);
		}

		// Token: 0x060018FF RID: 6399 RVA: 0x000A46CB File Offset: 0x000A28CB
		public bool TryGetSkillContent<T>(SkillData skill, out T skillData) where T : SkillData
		{
			return this.TryGetDataContent<T>(skill.id, out skillData);
		}

		// Token: 0x06001900 RID: 6400 RVA: 0x000A46DA File Offset: 0x000A28DA
		public SpellContent AddSpellContent(string spellID)
		{
			return this.AddSpellContent(Catalog.GetData<SpellData>(spellID, true));
		}

		// Token: 0x06001901 RID: 6401 RVA: 0x000A46E9 File Offset: 0x000A28E9
		public SpellContent AddSpellContent(SpellData spellData)
		{
			return (SpellContent)this.AddDataContent<SpellData>(spellData, "spell");
		}

		// Token: 0x06001902 RID: 6402 RVA: 0x000A46FC File Offset: 0x000A28FC
		public SkillContent AddSkillContent(string skillID)
		{
			return this.AddSkillContent(Catalog.GetData<SkillData>(skillID, true));
		}

		// Token: 0x06001903 RID: 6403 RVA: 0x000A470B File Offset: 0x000A290B
		public SkillContent AddSkillContent(SkillData skillData)
		{
			return (SkillContent)this.AddDataContent<SkillData>(skillData, "skill");
		}

		// Token: 0x06001904 RID: 6404 RVA: 0x000A4720 File Offset: 0x000A2920
		protected T AddContent<T>(T content, bool isSilent = false) where T : ContainerContent
		{
			if (content == null)
			{
				return default(T);
			}
			if (!isSilent)
			{
				Container.ContentChangeEvent onContentAddEvent = this.OnContentAddEvent;
				if (onContentAddEvent != null)
				{
					onContentAddEvent(content, EventTime.OnStart);
				}
			}
			if (this.allowStackItem)
			{
				ItemContent item = content as ItemContent;
				if (item != null && item.data.isStackable && item.state == null)
				{
					ItemContent existingItem = null;
					int count = this.contents.Count;
					for (int i = 0; i < count; i++)
					{
						ItemContent tempItem = this.contents[i] as ItemContent;
						if (tempItem != null && tempItem.referenceID == item.referenceID && tempItem.state == null)
						{
							existingItem = tempItem;
							break;
						}
					}
					if (existingItem != null)
					{
						existingItem.quantity += item.quantity;
						goto IL_F9;
					}
					this.contents.Add(content);
					goto IL_F9;
				}
			}
			this.contents.Add(content);
			IL_F9:
			if (!isSilent)
			{
				Container.ContentChangeEvent onContentAddEvent2 = this.OnContentAddEvent;
				if (onContentAddEvent2 != null)
				{
					onContentAddEvent2(content, EventTime.OnEnd);
				}
			}
			return content;
		}

		/// <summary>
		/// Used to set the quantity field of some content. Calls the increase / decrease hooks if value is different</summary>
		/// <param name="content">Content to change</param>
		/// <param name="newQuantity">Quantity to set</param>
		// Token: 0x06001905 RID: 6405 RVA: 0x000A4844 File Offset: 0x000A2A44
		public void SetContentQuantity(ContainerContent content, int newQuantity)
		{
			int previousQuantity = 0;
			ItemContent itemContent = content as ItemContent;
			if (itemContent != null)
			{
				previousQuantity = itemContent.quantity;
				if (previousQuantity > newQuantity)
				{
					Container.ContentChangeEvent onContentQuantityDecreaseEvent = this.OnContentQuantityDecreaseEvent;
					if (onContentQuantityDecreaseEvent != null)
					{
						onContentQuantityDecreaseEvent(content, EventTime.OnStart);
					}
				}
				if (previousQuantity < newQuantity)
				{
					Container.ContentChangeEvent onContentQuantityIncreaseEvent = this.OnContentQuantityIncreaseEvent;
					if (onContentQuantityIncreaseEvent != null)
					{
						onContentQuantityIncreaseEvent(content, EventTime.OnStart);
					}
				}
				itemContent.SetQuantity(newQuantity);
			}
			TableContent tableContent = content as TableContent;
			if (tableContent != null)
			{
				previousQuantity = tableContent.quantity;
				if (previousQuantity > newQuantity)
				{
					Container.ContentChangeEvent onContentQuantityDecreaseEvent2 = this.OnContentQuantityDecreaseEvent;
					if (onContentQuantityDecreaseEvent2 != null)
					{
						onContentQuantityDecreaseEvent2(content, EventTime.OnStart);
					}
				}
				if (previousQuantity < newQuantity)
				{
					Container.ContentChangeEvent onContentQuantityIncreaseEvent2 = this.OnContentQuantityIncreaseEvent;
					if (onContentQuantityIncreaseEvent2 != null)
					{
						onContentQuantityIncreaseEvent2(content, EventTime.OnStart);
					}
				}
				tableContent.SetQuantity(newQuantity);
			}
			if (previousQuantity > newQuantity)
			{
				Container.ContentChangeEvent onContentQuantityDecreaseEvent3 = this.OnContentQuantityDecreaseEvent;
				if (onContentQuantityDecreaseEvent3 != null)
				{
					onContentQuantityDecreaseEvent3(content, EventTime.OnEnd);
				}
			}
			if (previousQuantity < newQuantity)
			{
				Container.ContentChangeEvent onContentQuantityIncreaseEvent3 = this.OnContentQuantityIncreaseEvent;
				if (onContentQuantityIncreaseEvent3 == null)
				{
					return;
				}
				onContentQuantityIncreaseEvent3(content, EventTime.OnEnd);
			}
		}

		// Token: 0x06001906 RID: 6406 RVA: 0x000A490C File Offset: 0x000A2B0C
		public void RemoveContent(string referenceID, int count = 0, bool removeIfQuantityNull = true)
		{
			int removalRemaining = count;
			for (int i = this.contents.Count - 1; i >= 0; i--)
			{
				ContainerContent containerContent = this.contents[i];
				if (!(containerContent.referenceID != referenceID))
				{
					Container.ContentChangeEvent onContentRemoveEvent = this.OnContentRemoveEvent;
					if (onContentRemoveEvent != null)
					{
						onContentRemoveEvent(containerContent, EventTime.OnStart);
					}
					ItemContent itemContent = containerContent as ItemContent;
					if (itemContent != null && removalRemaining > 0)
					{
						if (itemContent.quantity > removalRemaining)
						{
							itemContent.quantity -= removalRemaining;
							removalRemaining = 0;
						}
						else
						{
							removalRemaining -= itemContent.quantity;
							if (removeIfQuantityNull)
							{
								this.contents.RemoveAt(i);
							}
							else
							{
								itemContent.quantity = 0;
							}
						}
					}
					if (count <= 0)
					{
						this.contents.RemoveAt(i);
					}
					Container.ContentChangeEvent onContentRemoveEvent2 = this.OnContentRemoveEvent;
					if (onContentRemoveEvent2 != null)
					{
						onContentRemoveEvent2(containerContent, EventTime.OnEnd);
					}
					if (count > 0 && removalRemaining == 0)
					{
						break;
					}
				}
			}
		}

		// Token: 0x06001907 RID: 6407 RVA: 0x000A49E0 File Offset: 0x000A2BE0
		public void RemoveContent(ContainerContent content)
		{
			int i = this.contents.Count - 1;
			while (i >= 0)
			{
				if (this.contents[i] == content)
				{
					Container.ContentChangeEvent onContentRemoveEvent = this.OnContentRemoveEvent;
					if (onContentRemoveEvent != null)
					{
						onContentRemoveEvent(content, EventTime.OnStart);
					}
					this.contents.RemoveAt(i);
					Container.ContentChangeEvent onContentRemoveEvent2 = this.OnContentRemoveEvent;
					if (onContentRemoveEvent2 == null)
					{
						return;
					}
					onContentRemoveEvent2(content, EventTime.OnEnd);
					return;
				}
				else
				{
					i--;
				}
			}
		}

		// Token: 0x06001908 RID: 6408 RVA: 0x000A4A46 File Offset: 0x000A2C46
		public bool IsPlayersContainer()
		{
			return this.creature != null && this.creature == Player.currentCreature;
		}

		// Token: 0x040017F4 RID: 6132
		public Container.LoadContent loadContent;

		// Token: 0x040017F5 RID: 6133
		public string loadPlayerContainerID;

		// Token: 0x040017F6 RID: 6134
		public string containerID;

		// Token: 0x040017F7 RID: 6135
		public bool loadOnStart = true;

		// Token: 0x040017F8 RID: 6136
		public Item.Owner spawnOwner;

		// Token: 0x040017F9 RID: 6137
		public List<Holder> linkedHolders = new List<Holder>();

		// Token: 0x040017FA RID: 6138
		public List<ContainerContent> contents = new List<ContainerContent>();

		// Token: 0x040017FB RID: 6139
		public bool allowStackItem;

		// Token: 0x04001801 RID: 6145
		[NonSerialized]
		public bool contentLoaded;

		// Token: 0x04001802 RID: 6146
		[NonSerialized]
		public Creature creature;

		// Token: 0x02000871 RID: 2161
		public enum LoadContent
		{
			// Token: 0x040041B2 RID: 16818
			None,
			// Token: 0x040041B3 RID: 16819
			ContainerID,
			// Token: 0x040041B4 RID: 16820
			PlayerInventory
		}

		// Token: 0x02000872 RID: 2162
		// (Invoke) Token: 0x06004037 RID: 16439
		public delegate void ContentLoadedEvent();

		// Token: 0x02000873 RID: 2163
		// (Invoke) Token: 0x0600403B RID: 16443
		public delegate void ContentChangeEvent(ContainerContent content, EventTime eventTime);
	}
}
