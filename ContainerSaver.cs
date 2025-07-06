using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002BC RID: 700
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/ContainerSaver.html")]
	public class ContainerSaver : MonoBehaviour
	{
		// Token: 0x060021EB RID: 8683 RVA: 0x000E9B8C File Offset: 0x000E7D8C
		public void TransferCurrentGrabbedOrHolsteredNpcItemToPlayer()
		{
			if (!Player.local)
			{
				return;
			}
			if (!Player.local.creature)
			{
				return;
			}
			List<Item> playerItems = Player.local.creature.equipment.GetAllHolsteredItems();
			Item handLeftItem = Player.local.creature.equipment.GetHeldItem(Side.Left);
			if (handLeftItem)
			{
				playerItems.Add(handLeftItem);
			}
			Item handRightItem = Player.local.creature.equipment.GetHeldItem(Side.Right);
			if (handRightItem)
			{
				playerItems.Add(handRightItem);
			}
			foreach (Item item in playerItems)
			{
				if (item.owner == Item.Owner.None && !item.worldAttached && (this.transferIncludeItemFromSpawner || !item.spawner))
				{
					item.SetOwner(Item.Owner.Player);
				}
			}
		}

		// Token: 0x060021EC RID: 8684 RVA: 0x000E9C84 File Offset: 0x000E7E84
		public void Save()
		{
			Debug.Log("ContainerSaver - Saving...");
			UnityEvent unityEvent = this.onPreSave;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			for (int i = Item.all.Count - 1; i >= 0; i--)
			{
				ItemAlwaysReturnsInInventory returnInInventory;
				if (Item.all[i].TryGetComponent<ItemAlwaysReturnsInInventory>(out returnInInventory))
				{
					returnInInventory.ReturnToPlayerInventory();
				}
			}
			foreach (ContainerSaver.ContainerSave saveContainer in this.saveContainers)
			{
				if (saveContainer.container)
				{
					Player.characterData.ClearContainer(saveContainer.playerContainerID);
					Player.characterData.AddToContainer(saveContainer.playerContainerID, saveContainer.container.contents);
				}
				for (int j = Item.all.Count - 1; j >= 0; j--)
				{
					Item item = Item.all[j];
					if (item.loaded && (saveContainer.ownerFilter != ContainerSaver.OwnerFilter.Player || item.owner == Item.Owner.Player) && (saveContainer.ownerFilter != ContainerSaver.OwnerFilter.LinkedContainer || !(item.linkedContainer != saveContainer.container)) && (saveContainer.retrieveFilterTypes.Count <= 0 || saveContainer.retrieveFilterTypes.Contains(item.data.type)) && !saveContainer.excludedTypes.Contains(item.data.type) && (!item.holder || !(item.holder.parentItem != null)) && (!item.holder || !item.holder.parentPart || !item.holder.parentPart.ragdoll.creature.isPlayer) && (!(item.breakable != null) || !item.breakable.IsBroken) && (!saveContainer.retrieveAllowedStorageItemOnly || saveContainer.saveBehavior != ContainerSaver.SaveBehavior.RetrieveToContainer || item.data.allowedStorage.HasFlag(ItemData.Storage.Container) || item.data.allowedStorage.HasFlag(ItemData.Storage.Inventory)))
					{
						if (saveContainer.saveBehavior == ContainerSaver.SaveBehavior.SaveCurrentPosition)
						{
							if (Level.current == null || Level.current.data == null || string.IsNullOrEmpty(Level.current.data.id))
							{
								Debug.LogError("[Container] Level ID is null or empty, can't save item position. Item will retrieved instead");
								this.RetrieveToContainer(item, saveContainer);
							}
							else if (item.holder && saveContainer.container.linkedHolders.Contains(item.holder))
							{
								Debug.Log(string.Concat(new string[]
								{
									"Save current item ",
									item.name,
									" in holder ",
									item.holder.name,
									" to container ",
									saveContainer.playerContainerID
								}));
								Player.characterData.AddToContainer(saveContainer.playerContainerID, new ItemContent(item, new ContentStateHolder(item.holder.name), item.contentCustomData, 1));
							}
							else
							{
								Debug.Log("Save current item position " + item.name + " to container " + saveContainer.playerContainerID);
								Player.characterData.AddToContainer(saveContainer.playerContainerID, new ItemContent(item, new ContentStatePlaced(item, Level.current.data.id), item.contentCustomData, 1));
							}
						}
						else if (saveContainer.saveBehavior == ContainerSaver.SaveBehavior.RetrieveToContainer)
						{
							this.RetrieveToContainer(item, saveContainer);
						}
					}
				}
			}
			foreach (ItemContent itemContent in Item.potentialLostItems)
			{
				Player.characterData.AddToContainer(PlayerSaveData.playerStashContainerName, itemContent);
			}
			Item.potentialLostItems.Clear();
			Player.characterData.SaveCoroutine(this.savePlayerInventory, null).AsSynchronous();
		}

		// Token: 0x060021ED RID: 8685 RVA: 0x000EA0CC File Offset: 0x000E82CC
		private void OnEnable()
		{
			EventManager.onLevelUnload += this.OnLevelUnload;
			EventManager.OnItemGrab += this.OnItemGrab;
		}

		// Token: 0x060021EE RID: 8686 RVA: 0x000EA0F0 File Offset: 0x000E82F0
		private void OnDisable()
		{
			EventManager.onLevelUnload -= this.OnLevelUnload;
			EventManager.OnItemGrab -= this.OnItemGrab;
		}

		// Token: 0x060021EF RID: 8687 RVA: 0x000EA114 File Offset: 0x000E8314
		private void Awake()
		{
			ContainerSaver.instance = this;
		}

		// Token: 0x060021F0 RID: 8688 RVA: 0x000EA11C File Offset: 0x000E831C
		private void Start()
		{
			Item.potentialLostItems.Clear();
		}

		// Token: 0x060021F1 RID: 8689 RVA: 0x000EA128 File Offset: 0x000E8328
		private void OnItemGrab(Handle handle, RagdollHand ragdollHand)
		{
			if (ragdollHand.playerHand && handle.item && handle.item.linkedContainer)
			{
				foreach (ContainerSaver.ContainerSave saveContainer in this.saveContainers)
				{
					if (saveContainer.transferToPlayerOnGrab && saveContainer.container == handle.item.linkedContainer)
					{
						handle.item.SetOwner(Item.Owner.Player);
						handle.item.linkedContainer = null;
					}
				}
			}
			if (this.transferUnOwnedItemToPlayerOnGrab && handle.item && handle.item.owner == Item.Owner.None && !handle.item.worldAttached && (this.transferIncludeItemFromSpawner || !handle.item.spawner))
			{
				handle.item.SetOwner(Item.Owner.Player);
			}
		}

		// Token: 0x060021F2 RID: 8690 RVA: 0x000EA230 File Offset: 0x000E8430
		private void RetrieveToContainer(Item item, ContainerSaver.ContainerSave saveContainer)
		{
			if (item.data.allowedStorage.HasFlag(ItemData.Storage.Container))
			{
				Debug.Log("Retrieve item " + item.name + " to container " + saveContainer.playerContainerID);
				Player.characterData.AddToContainer(saveContainer.playerContainerID, new ItemContent(item, null, item.contentCustomData, 1));
				return;
			}
			if (Player.currentCreature != null)
			{
				Debug.Log("Retrieve item " + item.name + " to player inventory as it's not allowed in container");
				Player.currentCreature.container.AddItemContent(item, true, null, null);
			}
		}

		// Token: 0x060021F3 RID: 8691 RVA: 0x000EA2D4 File Offset: 0x000E84D4
		private void OnLevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				return;
			}
			if (this.saveIfPlayerAliveOnly && Player.local.creature == null)
			{
				return;
			}
			if (!this.saveOnLevelUnload)
			{
				return;
			}
			this.Save();
		}

		// Token: 0x060021F4 RID: 8692 RVA: 0x000EA305 File Offset: 0x000E8505
		private void OnApplicationQuit()
		{
			if (!this.saveOnAppQuit)
			{
				return;
			}
			if (this.saveIfPlayerAliveOnly && Player.local.creature == null)
			{
				return;
			}
			this.Save();
		}

		// Token: 0x040020D5 RID: 8405
		public static ContainerSaver instance;

		// Token: 0x040020D6 RID: 8406
		[Tooltip("Do you want to save the player inventory?")]
		public bool savePlayerInventory;

		// Token: 0x040020D7 RID: 8407
		[Tooltip("Do you want to save the container when the level unloads?")]
		public bool saveOnLevelUnload;

		// Token: 0x040020D8 RID: 8408
		[Tooltip("When enabled, the container will only save if the player is alive.")]
		public bool saveIfPlayerAliveOnly = true;

		// Token: 0x040020D9 RID: 8409
		[Tooltip("Saves the container when the game is closed.")]
		public bool saveOnAppQuit;

		// Token: 0x040020DA RID: 8410
		[Tooltip("When enabled, unowned items will become owned by player when grabbed.")]
		public bool transferUnOwnedItemToPlayerOnGrab;

		// Token: 0x040020DB RID: 8411
		[Tooltip("When enabled, items inside spawners are transferred to player.")]
		public bool transferIncludeItemFromSpawner;

		// Token: 0x040020DC RID: 8412
		[Tooltip("List of saved containers.")]
		public List<ContainerSaver.ContainerSave> saveContainers;

		// Token: 0x040020DD RID: 8413
		[Header("Events")]
		public UnityEvent onPreSave;

		// Token: 0x0200098B RID: 2443
		[Serializable]
		public class ContainerSave
		{
			// Token: 0x040044EA RID: 17642
			[Tooltip("States the Player Container which the container saver saves to.\n\nCrystal Hunt default container is \"PlayerStash\"")]
			public string playerContainerID;

			// Token: 0x040044EB RID: 17643
			[Tooltip("(Optional) Can select a container placed in scene.")]
			public Container container;

			// Token: 0x040044EC RID: 17644
			[Tooltip("Who owns the container?")]
			public ContainerSaver.OwnerFilter ownerFilter;

			// Token: 0x040044ED RID: 17645
			[Tooltip("Depicts the save behaviour of the container.\n\nRetrieve to Container: Items will go to the Container (like the stash).\nSave Current Position: Will save the item position in the scene (like Level Persistence).")]
			public ContainerSaver.SaveBehavior saveBehavior;

			// Token: 0x040044EE RID: 17646
			[Tooltip("Filter the types of items that are affected by the container saver.")]
			public List<ItemData.Type> retrieveFilterTypes;

			// Token: 0x040044EF RID: 17647
			[Tooltip("Specific excluded item types from container saver")]
			public List<ItemData.Type> excludedTypes;

			// Token: 0x040044F0 RID: 17648
			[Tooltip("When enabled, items grabbed by player will automatically become owned by the player.")]
			public bool transferToPlayerOnGrab;

			// Token: 0x040044F1 RID: 17649
			[Tooltip("When true, only allowed items will be stored to the container.")]
			public bool retrieveAllowedStorageItemOnly = true;
		}

		// Token: 0x0200098C RID: 2444
		public enum OwnerFilter
		{
			// Token: 0x040044F3 RID: 17651
			Player,
			// Token: 0x040044F4 RID: 17652
			LinkedContainer
		}

		// Token: 0x0200098D RID: 2445
		public enum SaveBehavior
		{
			// Token: 0x040044F6 RID: 17654
			RetrieveToContainer,
			// Token: 0x040044F7 RID: 17655
			SaveCurrentPosition
		}
	}
}
