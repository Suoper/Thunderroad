using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Manikin;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000256 RID: 598
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Equipment.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Equipment")]
	public class Equipment : MonoBehaviour
	{
		/// <summary>
		/// Invoked when armour has been equipped.
		/// </summary>
		// Token: 0x1700018F RID: 399
		// (get) Token: 0x06001A04 RID: 6660 RVA: 0x000AD569 File Offset: 0x000AB769
		// (set) Token: 0x06001A05 RID: 6661 RVA: 0x000AD571 File Offset: 0x000AB771
		public Equipment.OnArmourEquipped OnArmourEquippedEvent { get; set; }

		/// <summary>
		/// Invoked when armour has been unequipped.
		/// </summary>
		// Token: 0x17000190 RID: 400
		// (get) Token: 0x06001A06 RID: 6662 RVA: 0x000AD57A File Offset: 0x000AB77A
		// (set) Token: 0x06001A07 RID: 6663 RVA: 0x000AD582 File Offset: 0x000AB782
		public Equipment.OnArmourUnEquipped OnArmourUnEquippedEvent { get; set; }

		// Token: 0x06001A08 RID: 6664 RVA: 0x000AD58B File Offset: 0x000AB78B
		private void Awake()
		{
			EventManager.onItemSpawnEquip += this.OnItemSpawnEquip;
		}

		// Token: 0x06001A09 RID: 6665 RVA: 0x000AD5A0 File Offset: 0x000AB7A0
		public void Init(Creature creature)
		{
			this.creature = creature;
			creature.container.OnContentRemoveEvent += this.OnContainerContentRemoved;
			if (creature.manikinParts)
			{
				creature.manikinParts.UpdateParts_Completed += this.OnUpdatePartCompleted;
			}
			this.ManageHolsterHandlers(true);
		}

		// Token: 0x06001A0A RID: 6666 RVA: 0x000AD5F6 File Offset: 0x000AB7F6
		public void Load(bool reEquipAllWardrobes = false)
		{
			if (this.equipWardrobesOnLoad)
			{
				this.EquipAllWardrobes(false, reEquipAllWardrobes);
			}
		}

		// Token: 0x06001A0B RID: 6667 RVA: 0x000AD608 File Offset: 0x000AB808
		private void OnItemSpawnEquip(Item item)
		{
			if (!this.creature.isPlayer)
			{
				return;
			}
			if (item.data.type != ItemData.Type.Wardrobe)
			{
				if (item.data.HasModule<ItemModuleWardrobe>())
				{
					Debug.LogWarning("Cannot equip " + item.data.id + " because it's type is not Wardrobe");
				}
				return;
			}
			ItemModuleWardrobe.CreatureWardrobe creatureWardrobe = item.data.GetModule<ItemModuleWardrobe>().GetWardrobe(this.creature);
			if (creatureWardrobe.creatureName != this.creature.data.name)
			{
				return;
			}
			for (int i = 0; i < this.wearableSlots.Count; i++)
			{
				Wearable wearableSlot = this.wearableSlots[i];
				if (wearableSlot.IsItemCompatible(creatureWardrobe))
				{
					wearableSlot.EquipItem(item);
					return;
				}
			}
		}

		// Token: 0x06001A0C RID: 6668 RVA: 0x000AD6C8 File Offset: 0x000AB8C8
		public void OnDespawn()
		{
			this.ManageHolsterHandlers(false);
			if (this.GetHeldItem(Side.Left))
			{
				this.GetHeldItem(Side.Left).Despawn();
			}
			if (this.GetHeldItem(Side.Right))
			{
				this.GetHeldItem(Side.Right).Despawn();
			}
			foreach (Holder holder in this.creature.holders)
			{
				for (int i = 0; i < holder.items.Count; i++)
				{
					holder.items[i].Despawn();
				}
			}
			EventManager.onItemSpawnEquip -= this.OnItemSpawnEquip;
			this.UnequipAllWardrobes(false, true);
		}

		// Token: 0x06001A0D RID: 6669 RVA: 0x000AD794 File Offset: 0x000AB994
		protected void OnContainerContentRemoved(ContainerContent content, EventTime eventTime)
		{
			ItemContent itemContent = content as ItemContent;
			if (itemContent == null)
			{
				return;
			}
			if (itemContent.data.type == ItemData.Type.Wardrobe && eventTime == EventTime.OnStart && itemContent.state is ContentStateWorn)
			{
				this.UnequipWardrobe(itemContent, true);
			}
		}

		// Token: 0x06001A0E RID: 6670 RVA: 0x000AD7D4 File Offset: 0x000AB9D4
		public void SetWearablesState(bool active)
		{
			for (int i = 0; i < this.wearableSlots.Count; i++)
			{
				this.wearableSlots[i].SetTouch(active);
			}
		}

		// Token: 0x06001A0F RID: 6671 RVA: 0x000AD80C File Offset: 0x000ABA0C
		public void UnequipAllWardrobes(bool updateParts = false, bool updateWornState = true)
		{
			if (!this.creature.manikinParts)
			{
				return;
			}
			this.creature.manikinParts.disableRenderersDuringUpdate = true;
			if (updateParts && this.waitPartUpdateCoroutine != null)
			{
				base.StopCoroutine(this.waitPartUpdateCoroutine);
			}
			if (updateWornState)
			{
				List<ContainerContent> contents = this.creature.container.contents;
				bool checkDataNotNull = true;
				Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
				array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
				foreach (ItemContent itemContent in contents.GetEnumerableContentsOfType(checkDataNotNull, array))
				{
					itemContent.state = null;
				}
			}
			this.creature.manikinLocations.FromJson(this.creature.orgWardrobeLocations);
			if (updateParts)
			{
				this.UpdateParts();
			}
		}

		// Token: 0x06001A10 RID: 6672 RVA: 0x000AD8F4 File Offset: 0x000ABAF4
		public void EquipAllWardrobes(bool bodyOnly, bool reEquipAllWardrobes = true)
		{
			if (!this.creature.manikinParts)
			{
				return;
			}
			this.creature.manikinParts.disableRenderersDuringUpdate = true;
			if (reEquipAllWardrobes)
			{
				this.UnequipAllWardrobes(false, !reEquipAllWardrobes);
			}
			List<ContainerContent> contents = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			int num = 0;
			Func<ItemContent, bool> <>9__0;
			Func<ItemContent, bool> func;
			if ((func = <>9__0) == null)
			{
				func = (<>9__0 = delegate(ItemContent content)
				{
					ItemModuleWardrobe itemModuleWardrobe;
					return content.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe) && (!bodyOnly || itemModuleWardrobe.category == Equipment.WardRobeCategory.Body);
				});
			}
			array[num] = func;
			foreach (ItemContent itemContent in contents.GetEnumerableContentsOfType(checkDataNotNull, array))
			{
				if (itemContent.HasState<ContentStateWorn>())
				{
					this.EquipWardrobe(itemContent, false);
				}
			}
			this.UpdateParts();
		}

		// Token: 0x06001A11 RID: 6673 RVA: 0x000AD9C8 File Offset: 0x000ABBC8
		public void EquipWardrobe(ItemContent itemContent, bool updateParts = true)
		{
			if (!this.creature.manikinParts)
			{
				return;
			}
			if (itemContent.data.type != ItemData.Type.Wardrobe)
			{
				Debug.LogError("Cannot wear " + itemContent.referenceID + " because it's not a Wardrobe");
				return;
			}
			ItemModuleWardrobe itemModuleWardrobe;
			if (!itemContent.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe))
			{
				Debug.LogError("Cannot wear " + itemContent.referenceID + " because it doesn't have an itemModuleWardrobe");
				return;
			}
			ItemModuleWardrobe.CreatureWardrobe wardrobeData;
			if (!itemModuleWardrobe.TryGetWardrobe(this.creature, out wardrobeData) || wardrobeData.manikinWardrobeData == null)
			{
				return;
			}
			if (updateParts)
			{
				this.creature.manikinParts.disableRenderersDuringUpdate = false;
			}
			for (int i = 0; i < wardrobeData.manikinWardrobeData.channels.Length; i++)
			{
				ItemContent wornContent = this.GetWornContent(wardrobeData.manikinWardrobeData.channels[i], wardrobeData.manikinWardrobeData.layers[i]);
				if (wornContent != null)
				{
					this.UnequipWardrobe(wornContent, false);
				}
			}
			foreach (ItemModule itemModule in itemContent.data.modules)
			{
				ItemModuleApparel apparelModule = itemModule as ItemModuleApparel;
				ApparelModuleType equippedOn;
				if (apparelModule != null && Enum.TryParse<ApparelModuleType>(wardrobeData.manikinWardrobeData.channels[0], true, out equippedOn))
				{
					apparelModule.OnEquip(this.creature, equippedOn, wardrobeData);
				}
			}
			itemContent.state = new ContentStateWorn();
			this.creature.manikinLocations.AddPart(wardrobeData.manikinWardrobeData);
			if (updateParts)
			{
				this.UpdateParts();
			}
			Mana mana = this.creature.mana;
			if (mana != null)
			{
				mana.OnApparelChangeEvent();
			}
			ArmorSFX armorSFX = this.creature.armorSFX;
			if (armorSFX == null)
			{
				return;
			}
			armorSFX.CalculateEffectData();
		}

		// Token: 0x06001A12 RID: 6674 RVA: 0x000ADB80 File Offset: 0x000ABD80
		public void UnequipWardrobe(ItemContent itemContent, bool updateParts = true)
		{
			if (!this.creature.manikinParts)
			{
				return;
			}
			if (itemContent.data.type != ItemData.Type.Wardrobe)
			{
				Debug.LogError("Cannot unwear " + itemContent.referenceID + " because it's not a Wardrobe");
				return;
			}
			ItemModuleWardrobe itemModuleWardrobe;
			if (!itemContent.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe))
			{
				Debug.LogError("Cannot unwear " + itemContent.referenceID + " because it doesn't have an itemModuleWardrobe");
				return;
			}
			ItemModuleWardrobe.CreatureWardrobe wardrobeData;
			if (!itemModuleWardrobe.TryGetWardrobe(this.creature, out wardrobeData) || wardrobeData.manikinWardrobeData == null)
			{
				Debug.LogError("Cannot unwear " + itemContent.referenceID + " because it doesn't have a wardrobeData or wardrobeData.manikinWardrobeData");
				return;
			}
			if (updateParts)
			{
				this.creature.manikinParts.disableRenderersDuringUpdate = false;
			}
			bool wasWorn = false;
			for (int i = 0; i < wardrobeData.manikinWardrobeData.channels.Length; i++)
			{
				if (this.GetWornContent(wardrobeData.manikinWardrobeData.channels[i], wardrobeData.manikinWardrobeData.layers[i]) != null)
				{
					this.creature.manikinLocations.RemovePart(wardrobeData.manikinWardrobeData.channels[i], wardrobeData.manikinWardrobeData.layers[i]);
					wasWorn = true;
				}
			}
			itemContent.state = null;
			if (wasWorn)
			{
				using (List<ItemModule>.Enumerator enumerator = itemContent.data.modules.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ItemModule itemModule = enumerator.Current;
						ItemModuleApparel apparelModule = itemModule as ItemModuleApparel;
						ApparelModuleType equippedOn;
						if (apparelModule != null && Enum.TryParse<ApparelModuleType>(wardrobeData.manikinWardrobeData.channels[0], true, out equippedOn))
						{
							apparelModule.OnUnequip(this.creature, equippedOn, wardrobeData);
						}
					}
					goto IL_1A5;
				}
			}
			Debug.LogWarning("Cannot unwear " + itemContent.referenceID + " because it was already not worn");
			IL_1A5:
			if (updateParts)
			{
				this.UpdateParts();
			}
			Mana mana = this.creature.mana;
			if (mana != null)
			{
				mana.OnApparelChangeEvent();
			}
			ArmorSFX armorSFX = this.creature.armorSFX;
			if (armorSFX == null)
			{
				return;
			}
			armorSFX.CalculateEffectData();
		}

		// Token: 0x06001A13 RID: 6675 RVA: 0x000ADD78 File Offset: 0x000ABF78
		public ItemContent[] GetWornContents()
		{
			List<ContainerContent> contents = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
			return contents.GetEnumerableContentsOfType(checkDataNotNull, array).ToArray<ItemContent>();
		}

		// Token: 0x06001A14 RID: 6676 RVA: 0x000ADDC8 File Offset: 0x000ABFC8
		public ItemContent GetWornContent(string channel, int layer)
		{
			List<ContainerContent> contents = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
			foreach (ItemContent itemContent in contents.GetEnumerableContentsOfType(checkDataNotNull, array))
			{
				ItemModuleWardrobe itemModuleWardrobe = itemContent.data.GetModule<ItemModuleWardrobe>();
				if (itemModuleWardrobe != null)
				{
					ItemModuleWardrobe.CreatureWardrobe wardrobeData = itemModuleWardrobe.GetWardrobe(this.creature);
					if (wardrobeData != null)
					{
						ManikinWardrobeData manikinWardrobeData = wardrobeData.manikinWardrobeData;
						if (!(manikinWardrobeData == null))
						{
							for (int i = 0; i < manikinWardrobeData.channels.Length; i++)
							{
								string a = manikinWardrobeData.channels[i];
								int itemWardrobeLayer = manikinWardrobeData.layers[i];
								if (a == channel && itemWardrobeLayer == layer)
								{
									return itemContent;
								}
							}
						}
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Get the lowest layer of equipment on the target manikin channel.
		/// </summary>
		// Token: 0x06001A15 RID: 6677 RVA: 0x000ADEBC File Offset: 0x000AC0BC
		public ItemContent GetWornContentLowerLayer(string channel, params string[] onlyLayers)
		{
			int[] onlyLayersInt = new int[onlyLayers.Length];
			for (int i = 0; i < onlyLayers.Length; i++)
			{
				onlyLayersInt[i] = ItemModuleWardrobe.GetLayer(channel, onlyLayers[i]);
			}
			int lowerLayer = int.MaxValue;
			ItemContent resultContent = null;
			List<ContainerContent> contents = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
			foreach (ItemContent itemContent in contents.GetEnumerableContentsOfType(checkDataNotNull, array))
			{
				ItemModuleWardrobe itemModuleWardrobe;
				if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe))
				{
					ItemModuleWardrobe.CreatureWardrobe wardrobeData = itemModuleWardrobe.GetWardrobe(this.creature);
					if (wardrobeData != null && wardrobeData.manikinWardrobeData != null)
					{
						for (int j = 0; j < wardrobeData.manikinWardrobeData.channels.Length; j++)
						{
							if (wardrobeData.manikinWardrobeData.channels[j] == channel && (onlyLayers == null || onlyLayers.Length == 0 || onlyLayersInt.Contains(wardrobeData.manikinWardrobeData.layers[j])) && wardrobeData.manikinWardrobeData.layers[j] < lowerLayer)
							{
								lowerLayer = wardrobeData.manikinWardrobeData.layers[j];
								resultContent = itemContent;
							}
						}
					}
				}
			}
			return resultContent;
		}

		/// <summary>
		/// Get all equipment on a specific manikin channel, filtered by layers
		/// </summary>
		// Token: 0x06001A16 RID: 6678 RVA: 0x000AE024 File Offset: 0x000AC224
		public ItemContent[] GetWornContentsLowerLayer(string channel, params string[] onlyLayers)
		{
			int[] onlyLayersInt = new int[onlyLayers.Length];
			for (int i = 0; i < onlyLayers.Length; i++)
			{
				onlyLayersInt[i] = ItemModuleWardrobe.GetLayer(channel, onlyLayers[i]);
			}
			List<ItemContent> results = new List<ItemContent>();
			List<ContainerContent> contents = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
			foreach (ItemContent itemContent in contents.GetEnumerableContentsOfType(checkDataNotNull, array))
			{
				ItemModuleWardrobe itemModuleWardrobe;
				if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe))
				{
					ItemModuleWardrobe.CreatureWardrobe wardrobeData = itemModuleWardrobe.GetWardrobe(this.creature);
					if (wardrobeData != null && wardrobeData.manikinWardrobeData != null)
					{
						for (int j = 0; j < wardrobeData.manikinWardrobeData.channels.Length; j++)
						{
							if (wardrobeData.manikinWardrobeData.channels[j] == channel && (onlyLayers == null || onlyLayers.Length == 0 || onlyLayersInt.Contains(wardrobeData.manikinWardrobeData.layers[j])))
							{
								results.Add(itemContent);
								break;
							}
						}
					}
				}
			}
			return results.ToArray();
		}

		/// <summary>
		/// Get all equipment on a specific manikin channel.
		/// </summary>
		// Token: 0x06001A17 RID: 6679 RVA: 0x000AE168 File Offset: 0x000AC368
		public ItemContent[] GetWornContentsLowerLayer(string channel)
		{
			List<ItemContent> results = new List<ItemContent>();
			List<ContainerContent> contents = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
			foreach (ItemContent itemContent in contents.GetEnumerableContentsOfType(checkDataNotNull, array))
			{
				ItemModuleWardrobe itemModuleWardrobe;
				if (itemContent.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe))
				{
					ItemModuleWardrobe.CreatureWardrobe wardrobeData = itemModuleWardrobe.GetWardrobe(this.creature);
					if (wardrobeData != null && wardrobeData.manikinWardrobeData != null)
					{
						for (int i = 0; i < wardrobeData.manikinWardrobeData.channels.Length; i++)
						{
							if (wardrobeData.manikinWardrobeData.channels[i] == channel)
							{
								results.Add(itemContent);
								break;
							}
						}
					}
				}
			}
			return results.ToArray();
		}

		/// <summary>
		/// Get the equipment on a specific part.
		/// </summary>
		// Token: 0x06001A18 RID: 6680 RVA: 0x000AE260 File Offset: 0x000AC460
		public ItemContent[] GetEquipmentOnPart(RagdollPart.Type partType)
		{
			Wearable part = this.creature.ragdoll.GetPart(partType).wearable;
			if (part == null)
			{
				Debug.LogWarning(string.Format("Part {0} on {1} does not contain a wearable!", partType, this.creature.name));
				return null;
			}
			return this.GetWornContentsLowerLayer(part.wardrobeChannel);
		}

		// Token: 0x06001A19 RID: 6681 RVA: 0x000AE2BB File Offset: 0x000AC4BB
		public void UpdateParts()
		{
			if (this.waitPartUpdateCoroutine != null)
			{
				base.StopCoroutine(this.waitPartUpdateCoroutine);
			}
			this.waitPartUpdateCoroutine = base.StartCoroutine(this.WaitPartUpdateCoroutine());
		}

		// Token: 0x06001A1A RID: 6682 RVA: 0x000AE2E3 File Offset: 0x000AC4E3
		protected IEnumerator WaitPartUpdateCoroutine()
		{
			Equipment.OnCreaturePartChanged onCreaturePartChanged = this.onCreaturePartChanged;
			if (onCreaturePartChanged != null)
			{
				onCreaturePartChanged(EventTime.OnStart);
			}
			Equipment.OnCreaturePartChanged onCreaturePartChanged2 = Equipment.onAnyCreaturePartChanged;
			if (onCreaturePartChanged2 != null)
			{
				onCreaturePartChanged2(EventTime.OnStart);
			}
			while (this.GetPendingApparelLoading() > 0)
			{
				yield return Yielders.EndOfFrame;
			}
			this.creature.manikinLocations.UpdateParts();
			yield break;
		}

		// Token: 0x06001A1B RID: 6683 RVA: 0x000AE2F4 File Offset: 0x000AC4F4
		public int GetPendingApparelLoading()
		{
			Creature creature = this.creature;
			int? num;
			if (creature == null)
			{
				num = null;
			}
			else
			{
				ManikinPartList manikinParts = creature.manikinParts;
				num = ((manikinParts != null) ? new int?(manikinParts.PendingHandles()) : null);
			}
			int? num2 = num;
			return num2.GetValueOrDefault();
		}

		// Token: 0x06001A1C RID: 6684 RVA: 0x000AE33C File Offset: 0x000AC53C
		protected void OnUpdatePartCompleted(ManikinPart[] partsAdded)
		{
			Equipment.OnCreaturePartChanged onCreaturePartChanged = this.onCreaturePartChanged;
			if (onCreaturePartChanged != null)
			{
				onCreaturePartChanged(EventTime.OnEnd);
			}
			Equipment.OnCreaturePartChanged onCreaturePartChanged2 = Equipment.onAnyCreaturePartChanged;
			if (onCreaturePartChanged2 == null)
			{
				return;
			}
			onCreaturePartChanged2(EventTime.OnEnd);
		}

		// Token: 0x140000AF RID: 175
		// (add) Token: 0x06001A1D RID: 6685 RVA: 0x000AE360 File Offset: 0x000AC560
		// (remove) Token: 0x06001A1E RID: 6686 RVA: 0x000AE398 File Offset: 0x000AC598
		public event Equipment.HeldWeaponReceivedHit OnHeldWeaponHitEvent;

		// Token: 0x06001A1F RID: 6687 RVA: 0x000AE3CD File Offset: 0x000AC5CD
		public void HeldWeaponHit(CollisionInstance collisionInstance)
		{
			if (this.OnHeldWeaponHitEvent != null)
			{
				this.OnHeldWeaponHitEvent(collisionInstance);
			}
		}

		// Token: 0x140000B0 RID: 176
		// (add) Token: 0x06001A20 RID: 6688 RVA: 0x000AE3E4 File Offset: 0x000AC5E4
		// (remove) Token: 0x06001A21 RID: 6689 RVA: 0x000AE41C File Offset: 0x000AC61C
		public event Equipment.HeldItemsChanged OnHeldItemsChangeEvent;

		// Token: 0x06001A22 RID: 6690 RVA: 0x000AE454 File Offset: 0x000AC654
		public void HeldItemsChange()
		{
			if (this.OnHeldItemsChangeEvent != null)
			{
				Equipment.HeldItemsChanged onHeldItemsChangeEvent = this.OnHeldItemsChangeEvent;
				Handle handle = this.heldRight;
				Item oldRightHand = (handle != null) ? handle.item : null;
				Handle handle2 = this.heldLeft;
				onHeldItemsChangeEvent(oldRightHand, (handle2 != null) ? handle2.item : null, this.GetHeldItem(Side.Right), this.GetHeldItem(Side.Left));
			}
			this.creature.animator.SetInteger(Creature.hashFreeHands, ((this.GetHeldHandle(Side.Right) == null) ? 1 : 0) + ((this.GetHeldHandle(Side.Left) == null) ? 2 : 0));
		}

		// Token: 0x140000B1 RID: 177
		// (add) Token: 0x06001A23 RID: 6691 RVA: 0x000AE4E4 File Offset: 0x000AC6E4
		// (remove) Token: 0x06001A24 RID: 6692 RVA: 0x000AE51C File Offset: 0x000AC71C
		public event Equipment.HolsterInteracted OnHolsterInteractedEvent;

		// Token: 0x06001A25 RID: 6693 RVA: 0x000AE554 File Offset: 0x000AC754
		protected void ManageHolsterHandlers(bool add)
		{
			using (List<Holder>.Enumerator enumerator = this.creature.holders.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Equipment.<>c__DisplayClass63_0 CS$<>8__locals1 = new Equipment.<>c__DisplayClass63_0();
					CS$<>8__locals1.<>4__this = this;
					CS$<>8__locals1.holder = enumerator.Current;
					this.holderSnapHandlers[CS$<>8__locals1.holder] = new Holder.HolderDelegate(CS$<>8__locals1.<ManageHolsterHandlers>g__HolderSnap|0);
					this.holderUnSnapHandlers[CS$<>8__locals1.holder] = new Holder.HolderDelegate(CS$<>8__locals1.<ManageHolsterHandlers>g__HolderUnsnap|1);
					if (add)
					{
						CS$<>8__locals1.holder.Snapped += this.holderSnapHandlers[CS$<>8__locals1.holder];
						CS$<>8__locals1.holder.UnSnapped += this.holderUnSnapHandlers[CS$<>8__locals1.holder];
					}
					else
					{
						CS$<>8__locals1.holder.Snapped -= this.holderSnapHandlers[CS$<>8__locals1.holder];
						CS$<>8__locals1.holder.UnSnapped -= this.holderUnSnapHandlers[CS$<>8__locals1.holder];
					}
				}
			}
		}

		// Token: 0x06001A26 RID: 6694 RVA: 0x000AE66C File Offset: 0x000AC86C
		public Handle GetHeldHandle(Side handSide)
		{
			if (handSide == Side.Left)
			{
				RagdollHand handLeft = this.creature.handLeft;
				if (handLeft == null)
				{
					return null;
				}
				return handLeft.grabbedHandle;
			}
			else
			{
				if (handSide != Side.Right)
				{
					return null;
				}
				RagdollHand handRight = this.creature.handRight;
				if (handRight == null)
				{
					return null;
				}
				return handRight.grabbedHandle;
			}
		}

		// Token: 0x06001A27 RID: 6695 RVA: 0x000AE6A4 File Offset: 0x000AC8A4
		public Item GetHeldItem(Side handSide)
		{
			Handle heldHandle = this.GetHeldHandle(handSide);
			if (heldHandle == null)
			{
				return null;
			}
			return heldHandle.item;
		}

		// Token: 0x06001A28 RID: 6696 RVA: 0x000AE6B8 File Offset: 0x000AC8B8
		public Item GetHeldWeapon(Side handSide)
		{
			Item item = this.GetHeldItem(handSide);
			if (item != null)
			{
				ItemData data = item.data;
				ItemData.Type? type = (data != null) ? new ItemData.Type?(data.type) : null;
				ItemData.Type type2 = ItemData.Type.Weapon;
				if (type.GetValueOrDefault() == type2 & type != null)
				{
					return item;
				}
			}
			if (item != null)
			{
				ItemData data2 = item.data;
				ItemData.Type? type = (data2 != null) ? new ItemData.Type?(data2.type) : null;
				ItemData.Type type2 = ItemData.Type.Shield;
				if (type.GetValueOrDefault() == type2 & type != null)
				{
					return item;
				}
			}
			return null;
		}

		// Token: 0x06001A29 RID: 6697 RVA: 0x000AE744 File Offset: 0x000AC944
		public bool CheckWeapon(Item weapon, Equipment.WeaponDrawInfo info, Transform target = null)
		{
			if (weapon == null)
			{
				return false;
			}
			if (weapon.data.moduleAI == null)
			{
				return false;
			}
			if (info.checkAmmo)
			{
				Item leftHand = this.GetHeldWeapon(Side.Left);
				bool flag;
				if (leftHand)
				{
					string slot = leftHand.data.slot;
					ItemModuleAI.RangedWeaponData rangedWeaponData = weapon.data.moduleAI.rangedWeaponData;
					flag = (slot == ((rangedWeaponData != null) ? rangedWeaponData.ammoType : null));
				}
				else
				{
					flag = false;
				}
				bool leftIsAmmo = flag;
				Item rightHand = this.GetHeldWeapon(Side.Right);
				bool flag2;
				if (rightHand)
				{
					string slot2 = rightHand.data.slot;
					ItemModuleAI.RangedWeaponData rangedWeaponData2 = weapon.data.moduleAI.rangedWeaponData;
					flag2 = (slot2 == ((rangedWeaponData2 != null) ? rangedWeaponData2.ammoType : null));
				}
				else
				{
					flag2 = false;
				}
				bool rightIsAmmo = flag2;
				ItemModuleAI.RangedWeaponData rangedWeaponData3 = weapon.data.moduleAI.rangedWeaponData;
				if (!this.GetQuiverAmmo((rangedWeaponData3 != null) ? rangedWeaponData3.ammoType : null, true) && !leftIsAmmo && !rightIsAmmo)
				{
					return false;
				}
			}
			return (weapon.data.moduleAI.rangedWeaponData == null || !(target != null) || (target.position - this.creature.transform.position).magnitude >= weapon.data.moduleAI.rangedWeaponData.tooCloseDistance) && (weapon.data.moduleAI.primaryClass == info.weaponClass || weapon.data.moduleAI.secondaryClass == info.weaponClass) && (weapon.data.moduleAI.weaponHandling == info.weaponHandling || weapon.data.moduleAI.secondaryHandling == info.weaponHandling);
		}

		// Token: 0x06001A2A RID: 6698 RVA: 0x000AE8E4 File Offset: 0x000ACAE4
		public List<Item> GetAllHolsteredItems()
		{
			List<Item> items = new List<Item>();
			List<Holder> allHolders = new List<Holder>();
			allHolders.AddRange(this.creature.holders);
			while (allHolders.Count > 0)
			{
				foreach (Item item in allHolders[0].items)
				{
					items.Add(item);
					if (item.childHolders.Count > 0)
					{
						allHolders.AddRange(item.childHolders);
					}
				}
				allHolders.RemoveAt(0);
			}
			return items;
		}

		// Token: 0x06001A2B RID: 6699 RVA: 0x000AE988 File Offset: 0x000ACB88
		public List<Item> GetHolsterWeapons()
		{
			List<Item> interactiveObjects = new List<Item>();
			foreach (Holder holder in this.creature.holders)
			{
				foreach (Item holdObject in holder.items)
				{
					if (holdObject.data != null && (holdObject.data.type == ItemData.Type.Weapon || holdObject.data.type == ItemData.Type.Weapon))
					{
						interactiveObjects.Add(holdObject);
					}
				}
			}
			return interactiveObjects;
		}

		// Token: 0x06001A2C RID: 6700 RVA: 0x000AEA44 File Offset: 0x000ACC44
		public void GetBestMatch(Equipment.WeaponDrawInfo info, Side side, out Item set, Item disallowed = null, Transform target = null)
		{
			float greatestReach = 0f;
			set = null;
			int holdersCount = this.creature.holders.Count;
			for (int i = 0; i < holdersCount; i++)
			{
				Holder holder = this.creature.holders[i];
				int itemsCount = holder.items.Count;
				for (int j = 0; j < itemsCount; j++)
				{
					Item item = holder.items[j];
					if (!(disallowed == item))
					{
						if (this.CheckWeapon(item, info, target))
						{
							Handle handle = item.GetMainHandle(side);
							if (handle.reach > greatestReach)
							{
								greatestReach = handle.reach;
								set = item;
							}
						}
						if (item.childHolders.Count > 0 && info.checkInHolder)
						{
							foreach (Holder holder2 in item.childHolders)
							{
								foreach (Item quiverItem in holder2.items)
								{
									if (!(disallowed == quiverItem) && this.CheckWeapon(quiverItem, info, target))
									{
										Handle handle2 = quiverItem.GetMainHandle(side);
										if (handle2.reach > greatestReach)
										{
											greatestReach = handle2.reach;
											set = quiverItem;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06001A2D RID: 6701 RVA: 0x000AEBD0 File Offset: 0x000ACDD0
		public Holder GetQuiverAmmo(string ammoType, bool requireContents = true)
		{
			foreach (Holder holder in this.creature.holders)
			{
				foreach (Item holdItem in holder.items)
				{
					if (holdItem.data.type == ItemData.Type.Quiver && holdItem.childHolders.Count > 0)
					{
						Holder childHolder = holdItem.childHolders[0];
						if (childHolder.data.slots.Contains(ammoType) && (!requireContents || childHolder.items.Count > 0))
						{
							return childHolder;
						}
					}
				}
			}
			return null;
		}

		// Token: 0x06001A2E RID: 6702 RVA: 0x000AECB8 File Offset: 0x000ACEB8
		public Holder GetHolder(Holder.DrawSlot drawSlot)
		{
			foreach (Holder holder in this.creature.holders)
			{
				if (holder.drawSlot == drawSlot)
				{
					return holder;
				}
			}
			return null;
		}

		// Token: 0x06001A2F RID: 6703 RVA: 0x000AED1C File Offset: 0x000ACF1C
		public Holder GetFreeDrawHolder(Side side, bool hipsIsPriority, string slot = null, Holder ignoreHolder = null)
		{
			if (side == Side.Right)
			{
				Holder holder = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.HipsRight : Holder.DrawSlot.BackRight);
				if (holder && holder.HasSlotFree() && holder != ignoreHolder && (slot == null || holder.data.SlotAllowed(slot)))
				{
					return holder;
				}
				holder = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.BackRight : Holder.DrawSlot.HipsRight);
				if (holder && holder.HasSlotFree() && holder != ignoreHolder && (slot == null || holder.data.SlotAllowed(slot)))
				{
					return holder;
				}
			}
			else
			{
				Holder holder2 = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.HipsLeft : Holder.DrawSlot.BackLeft);
				if (holder2 && holder2.HasSlotFree() && holder2 != ignoreHolder && (slot == null || holder2.data.SlotAllowed(slot)))
				{
					return holder2;
				}
				holder2 = this.GetHolder(hipsIsPriority ? Holder.DrawSlot.BackLeft : Holder.DrawSlot.HipsLeft);
				if (holder2 && holder2.HasSlotFree() && holder2 != ignoreHolder && (slot == null || holder2.data.SlotAllowed(slot)))
				{
					return holder2;
				}
			}
			return null;
		}

		// Token: 0x06001A30 RID: 6704 RVA: 0x000AEE24 File Offset: 0x000AD024
		public Holder GetFirstFreeHolder(string slot = null, Holder ignoreHolder = null)
		{
			foreach (Holder holder in this.creature.holders)
			{
				if (!(holder == ignoreHolder) && holder.HasSlotFree() && (slot == null || holder.data.SlotAllowed(slot)))
				{
					return holder;
				}
			}
			return null;
		}

		// Token: 0x040018D3 RID: 6355
		public bool canSwapExistingArmour = true;

		// Token: 0x040018D4 RID: 6356
		public bool equipWardrobesOnLoad = true;

		// Token: 0x040018D5 RID: 6357
		public bool armourEditModeEnabled;

		// Token: 0x040018D6 RID: 6358
		[NonSerialized]
		public Creature creature;

		// Token: 0x040018D7 RID: 6359
		[NonSerialized]
		public Renderer leftSelectedPart;

		// Token: 0x040018D8 RID: 6360
		[NonSerialized]
		public Renderer rightSelectedPart;

		// Token: 0x040018D9 RID: 6361
		[NonSerialized]
		public Wearable lastSelectedWearable;

		// Token: 0x040018DA RID: 6362
		[NonSerialized]
		public readonly List<Wearable> wearableSlots = new List<Wearable>();

		// Token: 0x040018DD RID: 6365
		public Equipment.OnCreaturePartChanged onCreaturePartChanged;

		// Token: 0x040018DE RID: 6366
		public static Equipment.OnCreaturePartChanged onAnyCreaturePartChanged;

		// Token: 0x040018DF RID: 6367
		protected Handle heldLeft;

		// Token: 0x040018E0 RID: 6368
		protected Handle heldRight;

		// Token: 0x040018E1 RID: 6369
		protected Coroutine waitPartUpdateCoroutine;

		// Token: 0x040018E4 RID: 6372
		private Dictionary<Holder, Holder.HolderDelegate> holderSnapHandlers = new Dictionary<Holder, Holder.HolderDelegate>();

		// Token: 0x040018E5 RID: 6373
		private Dictionary<Holder, Holder.HolderDelegate> holderUnSnapHandlers = new Dictionary<Holder, Holder.HolderDelegate>();

		// Token: 0x0200089B RID: 2203
		// (Invoke) Token: 0x060040CD RID: 16589
		public delegate void OnArmourEquipped(Wearable slot, Item item);

		// Token: 0x0200089C RID: 2204
		// (Invoke) Token: 0x060040D1 RID: 16593
		public delegate void OnArmourUnEquipped(Wearable slot, Item item);

		// Token: 0x0200089D RID: 2205
		public class WeaponDrawInfo
		{
			// Token: 0x0400420C RID: 16908
			public ItemModuleAI.WeaponClass weaponClass;

			// Token: 0x0400420D RID: 16909
			public ItemModuleAI.WeaponHandling weaponHandling;

			// Token: 0x0400420E RID: 16910
			public bool checkAmmo;

			// Token: 0x0400420F RID: 16911
			public bool checkInHolder;
		}

		// Token: 0x0200089E RID: 2206
		public enum WardRobeCategory
		{
			// Token: 0x04004211 RID: 16913
			Apparel,
			// Token: 0x04004212 RID: 16914
			Body
		}

		// Token: 0x0200089F RID: 2207
		// (Invoke) Token: 0x060040D6 RID: 16598
		public delegate void OnCreaturePartChanged(EventTime eventTime);

		// Token: 0x020008A0 RID: 2208
		// (Invoke) Token: 0x060040DA RID: 16602
		public delegate void HeldWeaponReceivedHit(CollisionInstance collisionInstance);

		// Token: 0x020008A1 RID: 2209
		// (Invoke) Token: 0x060040DE RID: 16606
		public delegate void HeldItemsChanged(Item oldRightHand, Item oldLeftHand, Item newRightHand, Item newLeftHand);

		// Token: 0x020008A2 RID: 2210
		// (Invoke) Token: 0x060040E2 RID: 16610
		public delegate void HolsterInteracted(Holder holder, Item holsteredItem, bool added);
	}
}
