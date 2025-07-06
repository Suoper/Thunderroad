using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002D3 RID: 723
	[RequireComponent(typeof(Collider))]
	public class ItemPlaceableZone : ThunderBehaviour
	{
		// Token: 0x17000226 RID: 550
		// (get) Token: 0x060022EF RID: 8943 RVA: 0x000EFFAE File Offset: 0x000EE1AE
		// (set) Token: 0x060022F0 RID: 8944 RVA: 0x000EFFB6 File Offset: 0x000EE1B6
		public ItemPlaceableZone.ContentStatus contentStatus { get; protected set; }

		// Token: 0x17000227 RID: 551
		// (get) Token: 0x060022F1 RID: 8945 RVA: 0x000EFFBF File Offset: 0x000EE1BF
		public List<Item> items
		{
			get
			{
				return this.itemsInZone.Keys.ToList<Item>();
			}
		}

		// Token: 0x17000228 RID: 552
		// (get) Token: 0x060022F2 RID: 8946 RVA: 0x000EFFD1 File Offset: 0x000EE1D1
		// (set) Token: 0x060022F3 RID: 8947 RVA: 0x000EFFD9 File Offset: 0x000EE1D9
		public Container container { get; protected set; }

		// Token: 0x060022F4 RID: 8948 RVA: 0x000EFFE2 File Offset: 0x000EE1E2
		protected void OnValidate()
		{
			this.triggerCollider = base.GetComponent<Collider>();
			this.triggerCollider.isTrigger = true;
			this.triggerCollider.gameObject.layer = GameManager.GetLayer(LayerName.Zone);
		}

		// Token: 0x060022F5 RID: 8949 RVA: 0x000F0014 File Offset: 0x000EE214
		protected void Setup()
		{
			EventManager.onLevelUnload += this.LevelUnload;
			Container container;
			if (!this.placeableContainers.TryGetValue(this.containerID, out container))
			{
				GameObject gameObject = new GameObject("Placeable_Container_" + this.containerID);
				gameObject.SetActive(false);
				container = gameObject.AddComponent<Container>();
				container.loadOnStart = false;
				container.transform.SetParent(Level.current.transform, false);
				gameObject.SetActive(true);
				this.placeableContainers.Add(this.containerID, container);
				this.container = container;
				container.OnContentLoadedEvent += this.ContainerLoaded;
				container.Load();
				return;
			}
			this.container = container;
		}

		// Token: 0x060022F6 RID: 8950 RVA: 0x000F00C6 File Offset: 0x000EE2C6
		protected void Awake()
		{
		}

		// Token: 0x060022F7 RID: 8951 RVA: 0x000F00C8 File Offset: 0x000EE2C8
		protected void Start()
		{
			this.Setup();
		}

		// Token: 0x060022F8 RID: 8952 RVA: 0x000F00D0 File Offset: 0x000EE2D0
		protected void ContainerLoaded()
		{
			if (this.contentStatus == ItemPlaceableZone.ContentStatus.WaitingForContent)
			{
				this.SpawnAndPositionItems();
				return;
			}
			if (this.contentStatus != ItemPlaceableZone.ContentStatus.Spawned)
			{
				this.contentStatus = ItemPlaceableZone.ContentStatus.ContentReady;
			}
		}

		// Token: 0x060022F9 RID: 8953 RVA: 0x000F00F4 File Offset: 0x000EE2F4
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			if (this.container && this.container.contentLoaded)
			{
				this.contentStatus = ItemPlaceableZone.ContentStatus.ContentReady;
			}
			if (this.contentStatus == ItemPlaceableZone.ContentStatus.ContentReady)
			{
				this.SpawnAndPositionItems();
				return;
			}
			if (this.contentStatus != ItemPlaceableZone.ContentStatus.Spawned)
			{
				this.contentStatus = ItemPlaceableZone.ContentStatus.WaitingForContent;
			}
		}

		// Token: 0x060022FA RID: 8954 RVA: 0x000F0148 File Offset: 0x000EE348
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			this.StoreAllItems();
		}

		// Token: 0x060022FB RID: 8955 RVA: 0x000F0156 File Offset: 0x000EE356
		protected void LevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			EventManager.onLevelUnload -= this.LevelUnload;
			this.StoreAllItems();
		}

		// Token: 0x060022FC RID: 8956 RVA: 0x000F0170 File Offset: 0x000EE370
		protected void OnTriggerEnter(Collider other)
		{
			Item item;
			if (!this.IsStorableItem(other, out item))
			{
				return;
			}
			List<Collider> colliders;
			if (this.itemsInZone.TryGetValue(item, out colliders))
			{
				colliders.Add(other);
				return;
			}
			this.itemsInZone[item] = new List<Collider>
			{
				other
			};
		}

		// Token: 0x060022FD RID: 8957 RVA: 0x000F01BC File Offset: 0x000EE3BC
		protected void OnTriggerExit(Collider other)
		{
			Item item;
			if (!this.IsStorableItem(other, out item))
			{
				return;
			}
			List<Collider> colliders;
			if (this.itemsInZone.TryGetValue(item, out colliders))
			{
				colliders.Remove(other);
				if (colliders.Count == 0)
				{
					this.itemsInZone.Remove(item);
				}
			}
		}

		// Token: 0x060022FE RID: 8958 RVA: 0x000F0204 File Offset: 0x000EE404
		protected bool IsStorableItem(Collider collider, out Item item)
		{
			item = null;
			if (!collider.attachedRigidbody && !collider.attachedArticulationBody)
			{
				return false;
			}
			item = collider.GetComponentInParent<Item>();
			return !(item == null) && !item.isBrokenPiece && (!item.spawner || (this.allowItemsFromItemSpawners && !this.ignoredItemSpawners.Contains(item.spawner))) && (!item.holder || !item.holder.creature);
		}

		// Token: 0x060022FF RID: 8959 RVA: 0x000F02A0 File Offset: 0x000EE4A0
		protected void SpawnAndPositionItems()
		{
			for (int i = this.container.contents.Count - 1; i >= 0; i--)
			{
				ItemContent content = this.container.contents[i] as ItemContent;
				this.container.RemoveContent(content);
				content.Spawn(delegate(Item item)
				{
					ContentCustomDataRelativePosRot posRot;
					if (content.TryGetCustomData<ContentCustomDataRelativePosRot>(out posRot))
					{
						item.transform.position = this.transform.TransformPoint(posRot.localPos);
						item.transform.rotation = this.transform.TransformRotation(posRot.localRot);
					}
					item.DisallowDespawn = true;
				}, this.container.spawnOwner, true);
			}
		}

		// Token: 0x06002300 RID: 8960 RVA: 0x000F0328 File Offset: 0x000EE528
		protected void StoreAllItems()
		{
			if (this.itemsInZone.Count == 0)
			{
				return;
			}
			List<Item> items = this.items;
			this.itemsInZone.Clear();
			for (int i = items.Count - 1; i >= 0; i--)
			{
				if (items[i] == null)
				{
					Debug.LogWarning("An item from placeable zone [ " + base.name + " ] unexpectedly despawned. This is a problem.");
				}
				else
				{
					ContentCustomDataRelativePosRot posRot = new ContentCustomDataRelativePosRot(items[i].transform, base.transform);
					this.container.AddItemContent(items[i], true, null, new List<ContentCustomData>
					{
						posRot
					});
				}
			}
			this.contentStatus = ItemPlaceableZone.ContentStatus.ContentReady;
		}

		// Token: 0x06002301 RID: 8961 RVA: 0x000F03D3 File Offset: 0x000EE5D3
		public void SetItemsKinematic(bool active)
		{
		}

		// Token: 0x040021F9 RID: 8697
		public string containerID;

		// Token: 0x040021FA RID: 8698
		public bool allowItemsFromItemSpawners = true;

		// Token: 0x040021FB RID: 8699
		public List<ItemSpawner> ignoredItemSpawners = new List<ItemSpawner>();

		// Token: 0x040021FE RID: 8702
		protected Dictionary<string, Container> placeableContainers = new Dictionary<string, Container>();

		// Token: 0x040021FF RID: 8703
		protected Collider triggerCollider;

		// Token: 0x04002200 RID: 8704
		protected Dictionary<Item, List<Collider>> itemsInZone = new Dictionary<Item, List<Collider>>();

		// Token: 0x020009B2 RID: 2482
		public enum ContentStatus
		{
			// Token: 0x04004589 RID: 17801
			Prespawn,
			// Token: 0x0400458A RID: 17802
			ContentReady,
			// Token: 0x0400458B RID: 17803
			WaitingForContent,
			// Token: 0x0400458C RID: 17804
			Spawned
		}
	}
}
