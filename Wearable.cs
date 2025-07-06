using System;
using System.Collections.Generic;
using ThunderRoad.Manikin;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThunderRoad
{
	// Token: 0x02000369 RID: 873
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Wearable.html")]
	[AddComponentMenu("ThunderRoad/Wearable")]
	public class Wearable : Interactable
	{
		/// <summary>
		/// The creature this wearable is targeting.
		/// </summary>
		// Token: 0x17000276 RID: 630
		// (get) Token: 0x06002923 RID: 10531 RVA: 0x00117B97 File Offset: 0x00115D97
		// (set) Token: 0x06002924 RID: 10532 RVA: 0x00117B9F File Offset: 0x00115D9F
		public Creature Creature { get; private set; }

		/// <summary>
		/// The target part this wearable is attached to.
		/// </summary>
		// Token: 0x17000277 RID: 631
		// (get) Token: 0x06002925 RID: 10533 RVA: 0x00117BA8 File Offset: 0x00115DA8
		// (set) Token: 0x06002926 RID: 10534 RVA: 0x00117BB0 File Offset: 0x00115DB0
		public RagdollPart Part { get; private set; }

		/// <summary>
		/// Invoked when an item has been equipped.
		/// </summary>
		// Token: 0x14000136 RID: 310
		// (add) Token: 0x06002927 RID: 10535 RVA: 0x00117BBC File Offset: 0x00115DBC
		// (remove) Token: 0x06002928 RID: 10536 RVA: 0x00117BF4 File Offset: 0x00115DF4
		public event Wearable.OnItemEquipped OnItemEquippedEvent;

		/// <summary>
		/// Invoked when an item has been equipped.
		/// </summary>
		// Token: 0x14000137 RID: 311
		// (add) Token: 0x06002929 RID: 10537 RVA: 0x00117C2C File Offset: 0x00115E2C
		// (remove) Token: 0x0600292A RID: 10538 RVA: 0x00117C64 File Offset: 0x00115E64
		public event Wearable.OnItemUnEquipped OnItemUnEquippedEvent;

		/// <summary>
		/// Invoked when edit mode has changed its state.
		/// </summary>
		// Token: 0x14000138 RID: 312
		// (add) Token: 0x0600292B RID: 10539 RVA: 0x00117C9C File Offset: 0x00115E9C
		// (remove) Token: 0x0600292C RID: 10540 RVA: 0x00117CD4 File Offset: 0x00115ED4
		public event Wearable.OnEditModeChanged OnEditModeChangedEvent;

		/// <summary>
		/// This wearable layers. Layers can be obtained through ItemToLayerName/IndexToLayerName.
		/// </summary>
		// Token: 0x17000278 RID: 632
		// (get) Token: 0x0600292D RID: 10541 RVA: 0x00117D09 File Offset: 0x00115F09
		private Dictionary<string, int> WardrobeLayersInt { get; } = new Dictionary<string, int>();

		// Token: 0x0600292E RID: 10542 RVA: 0x00117D14 File Offset: 0x00115F14
		protected override void Awake()
		{
			this.Creature = base.GetComponentInParent<Creature>();
			this.Part = base.GetComponentInParent<RagdollPart>();
			if (this.Creature.manikinParts)
			{
				this.Creature.manikinParts.UpdateParts_Completed += this.OnManikinPartsRefreshed;
			}
			this.ConvertLayersToInts();
			this.internalLayerArray = this.GetAllLayers();
			Equipment equipment = this.Creature.equipment;
			if (equipment != null)
			{
				equipment.wearableSlots.Add(this);
			}
			base.Awake();
			this.SetTouch(false);
		}

		// Token: 0x0600292F RID: 10543 RVA: 0x00117DA4 File Offset: 0x00115FA4
		private void ConvertLayersToInts()
		{
			if (this.Creature == null)
			{
				this.Creature = base.GetComponentInParent<Creature>();
			}
			if (this.Part == null)
			{
				this.Part = base.GetComponentInParent<RagdollPart>();
			}
			this.WardrobeLayersInt.Clear();
			for (int i = 0; i < this.wardrobeLayers.Length; i++)
			{
				string layerName = this.wardrobeLayers[i].layer;
				if (!this.WardrobeLayersInt.ContainsKey(layerName))
				{
					int intLayer = ItemModuleWardrobe.GetLayer(this.wardrobeChannel, layerName);
					if (intLayer == -1)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"Layer ",
							layerName,
							" is not valid for channel ",
							this.wardrobeChannel,
							"."
						}));
					}
					this.WardrobeLayersInt.Add(layerName, intLayer);
				}
			}
		}

		/// <summary>
		/// Is the target item wardrobe compatible with this wearable?
		/// </summary>
		// Token: 0x06002930 RID: 10544 RVA: 0x00117E70 File Offset: 0x00116070
		public bool IsItemCompatible(ItemModuleWardrobe.CreatureWardrobe creatureWardrobe)
		{
			if (creatureWardrobe == null)
			{
				Debug.LogWarning("Item has no valid creature wardrobe.");
				return false;
			}
			for (int i = 0; i < creatureWardrobe.manikinWardrobeData.channels.Length; i++)
			{
				if (string.CompareOrdinal(creatureWardrobe.manikinWardrobeData.channels[i], this.wardrobeChannel) == 0)
				{
					for (int j = 0; j < creatureWardrobe.manikinWardrobeData.layers.Length; j++)
					{
						int layer = creatureWardrobe.manikinWardrobeData.layers[j];
						string indexToLayerName = this.IndexToLayerName(layer);
						if (this.WardrobeLayersInt.ContainsKey(indexToLayerName))
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Returns renderers from the first available lod for for the given layer.
		/// </summary>
		/// <param name="layer"></param>
		/// <returns></returns>
		// Token: 0x06002931 RID: 10545 RVA: 0x00117F00 File Offset: 0x00116100
		public List<Renderer> GetWornPartRenderers(int layer)
		{
			ManikinPart mPart = this.Creature.manikinLocations.GetPartAtLocation(this.wardrobeChannel, layer);
			if (mPart == null)
			{
				return null;
			}
			ManikinGroupPart manikinGroupPart = mPart as ManikinGroupPart;
			if (manikinGroupPart)
			{
				foreach (ManikinGroupPart.PartLOD partLOD in manikinGroupPart.partLODs)
				{
					if (partLOD.renderers.Count > 0)
					{
						return partLOD.renderers;
					}
				}
				Debug.LogError("Could not find any renderer in " + manikinGroupPart.name);
				return new List<Renderer>();
			}
			ManikinSmrPart manikinSmrPart = mPart as ManikinSmrPart;
			if (manikinSmrPart != null)
			{
				return new List<Renderer>
				{
					manikinSmrPart.GetSkinnedMeshRenderer()
				};
			}
			return new List<Renderer>();
		}

		/// <summary>
		/// Returns renderers from the first available lod for either the worn armour or base mesh part.
		/// </summary>
		// Token: 0x06002932 RID: 10546 RVA: 0x00117FD8 File Offset: 0x001161D8
		public List<Renderer> GetWornPartRenderers()
		{
			List<ManikinPart> locations = this.Creature.manikinLocations.GetPartsAtChannel(this.wardrobeChannel, null);
			if (locations.Count == 0)
			{
				return null;
			}
			bool isEmpty = this.IsEmpty();
			RagdollPart.Type type = this.Part.type;
			ManikinPart mPart;
			if (type <= RagdollPart.Type.LeftArm)
			{
				if (type != RagdollPart.Type.Head)
				{
					if (type == RagdollPart.Type.Torso)
					{
						int torso = (this.Creature.data.gender == CreatureData.Gender.Female) ? this.FindRendererWithName(locations, "Torso", isEmpty ? "Chest" : null) : this.FindRendererWithName(locations, "Chest", isEmpty ? "_Chest" : null);
						mPart = locations[(this.Part.section == RagdollPart.Section.Upper && !isEmpty) ? this.FindRendererWithName(locations, "Shoulder", null) : ((this.Part.section == RagdollPart.Section.Upper && isEmpty) ? torso : ((this.Part.section == RagdollPart.Section.Mid && !isEmpty) ? this.FindRendererWithName(locations, "_Chest", "_Shirt") : ((this.Part.section == RagdollPart.Section.Mid && isEmpty) ? torso : ((this.Part.section == RagdollPart.Section.Lower && isEmpty) ? this.FindRendererWithName(locations, "Underwear", null) : ((this.Part.section == RagdollPart.Section.Lower && !isEmpty) ? this.FindRendererWithName(locations, "_Chest", null) : 0)))))];
						goto IL_1CB;
					}
					if (type != RagdollPart.Type.LeftArm)
					{
						goto IL_1C3;
					}
				}
				else
				{
					if (isEmpty)
					{
						mPart = locations[(this.Creature.data.gender == CreatureData.Gender.Male) ? 1 : 0];
						goto IL_1CB;
					}
					mPart = locations[locations.Count - 1];
					goto IL_1CB;
				}
			}
			else if (type != RagdollPart.Type.RightArm && type != RagdollPart.Type.LeftLeg && type != RagdollPart.Type.RightLeg)
			{
				goto IL_1C3;
			}
			mPart = locations[locations.Count - 1];
			goto IL_1CB;
			IL_1C3:
			mPart = locations[0];
			IL_1CB:
			ManikinGroupPart manikinGroupPart = mPart as ManikinGroupPart;
			if (manikinGroupPart)
			{
				foreach (ManikinGroupPart.PartLOD partLOD in manikinGroupPart.partLODs)
				{
					if (partLOD.renderers.Count > 0)
					{
						return partLOD.renderers;
					}
				}
				Debug.LogError("Could not find any renderer in " + manikinGroupPart.name);
				return new List<Renderer>();
			}
			ManikinSmrPart manikinSmrPart = mPart as ManikinSmrPart;
			return new List<Renderer>
			{
				manikinSmrPart.GetSkinnedMeshRenderer()
			};
		}

		/// <summary>
		/// Equip an item to this wearable slot, the Item must be wearable and compatible.
		/// </summary>
		/// <param name="onItemUnequipped">If an existing item is equipped this is invoked when it is removed.</param>
		/// <returns>If the item could be worn.</returns>
		// Token: 0x06002933 RID: 10547 RVA: 0x00118254 File Offset: 0x00116454
		public bool EquipItem(Item item)
		{
			if (item != null && item.data != null && item.data.type == ItemData.Type.Wardrobe)
			{
				for (int i = 0; i < item.handlers.Count; i++)
				{
					item.handlers[i].TryRelease();
				}
				if (!this.IsEmpty())
				{
					ItemModuleWardrobe.CreatureWardrobe creatureWardrobe = item.data.GetModule<ItemModuleWardrobe>().GetWardrobe(this.Creature);
					string layer = this.ItemToLayerName(creatureWardrobe);
					ItemContent worn = this.GetEquipmentOnLayer(layer);
					if (worn != null)
					{
						this.UnEquip(worn, null, true);
					}
				}
				ItemContent content = this.Creature.container.AddItemContent(item, true, new ContentStateWorn(), item.contentCustomData);
				this.Creature.equipment.EquipWardrobe(content, true);
				this.PlayAudioFromLocation(content.data.snapAudioContainerLocation, content.data.snapAudioVolume_dB);
				Mirror.ClearRenderQueue();
				Equipment.OnArmourEquipped onArmourEquippedEvent = this.Creature.equipment.OnArmourEquippedEvent;
				if (onArmourEquippedEvent != null)
				{
					onArmourEquippedEvent(this, item);
				}
				Wearable.OnItemEquipped onItemEquippedEvent = this.OnItemEquippedEvent;
				if (onItemEquippedEvent != null)
				{
					onItemEquippedEvent(item);
				}
				this.hasHandExited = false;
				return true;
			}
			return false;
		}

		/// <summary>
		/// Unequip a WARDROBE container item from the player.
		/// </summary>
		/// <param name="itemContent">The item to unequip.</param>
		/// <param name="onItemUnequipped">If an existing item is equipped this is invoked when it is removed.</param>
		/// <param name="grabReleasedItem">If the item should be grabbed after being unequipped.</param>
		/// <returns>If the item could be unequipped.</returns>
		// Token: 0x06002934 RID: 10548 RVA: 0x0011837C File Offset: 0x0011657C
		public bool UnEquip(ItemContent itemContent, Action<Item> onItemUnequipped = null, bool grabReleasedItem = false)
		{
			if (itemContent == null)
			{
				return false;
			}
			ItemModuleWardrobe wardrobeModule;
			if (!itemContent.data.TryGetModule<ItemModuleWardrobe>(out wardrobeModule))
			{
				return false;
			}
			ItemModuleWardrobe.CreatureWardrobe creatureWardrobe;
			if (!wardrobeModule.TryGetWardrobe(this.Creature, out creatureWardrobe))
			{
				return false;
			}
			this.Creature.container.RemoveContent(itemContent);
			Wearable.WearableEntry entry = this.GetLayerEntry(this.ItemToLayerName(creatureWardrobe));
			itemContent.Spawn(delegate(Item item)
			{
				if (grabReleasedItem && item != null)
				{
					this.lastInteractor = ((this.Creature.handLeft.grabbedHandle == null) ? this.Creature.handLeft : ((this.Creature.handRight.grabbedHandle == null) ? this.Creature.handRight : null));
					if (this.lastInteractor != null)
					{
						Handle handle = item.GetMainHandle(this.lastInteractor.side);
						if (handle != null)
						{
							item.physicBody.rigidBody.velocity = Vector3.zero;
							item.transform.SetPositionAndRotation(this.lastInteractor.transform.position, Quaternion.identity);
							this.lastInteractor.Grab(handle, true, false);
							goto IL_132;
						}
					}
					item.physicBody.rigidBody.velocity = Vector3.zero;
					item.transform.SetPositionAndRotation(this.transform.position, Quaternion.identity);
				}
				IL_132:
				Equipment.OnArmourUnEquipped onArmourUnEquippedEvent = this.Creature.equipment.OnArmourUnEquippedEvent;
				if (onArmourUnEquippedEvent != null)
				{
					onArmourUnEquippedEvent(this, item);
				}
				Wearable.OnItemUnEquipped onItemUnEquippedEvent = this.OnItemUnEquippedEvent;
				if (onItemUnEquippedEvent != null)
				{
					onItemUnEquippedEvent(entry.layer, item);
				}
				Action<Item> onItemUnequipped2 = onItemUnequipped;
				if (onItemUnequipped2 == null)
				{
					return;
				}
				onItemUnequipped2(item);
			}, this.Creature.container.spawnOwner, true);
			this.PlayAudioFromLocation(itemContent.data.snapAudioContainerLocation, itemContent.data.snapAudioVolume_dB);
			this.SetHintState(null);
			return true;
		}

		/// <summary>
		/// Unequip the current worn item.
		/// </summary>
		/// <param name="onItemUnequipped">If an existing item is equipped this is invoked when it is removed.</param>
		/// <returns>If the item could be unequipped.</returns>
		// Token: 0x06002935 RID: 10549 RVA: 0x00118437 File Offset: 0x00116637
		public bool UnEquip(string layer, Action<Item> onItemUnequipped = null)
		{
			return this.UnEquip(this.GetEquipmentOnLayer(layer), onItemUnequipped, false);
		}

		// Token: 0x06002936 RID: 10550 RVA: 0x00118448 File Offset: 0x00116648
		public bool TryGetLayerIndex(string layer, out int index)
		{
			index = -1;
			int value;
			if (this.WardrobeLayersInt.TryGetValue(layer, out value))
			{
				index = value;
			}
			return index != -1;
		}

		/// <summary>
		/// Get the current worn item on the target layer.
		/// </summary>
		// Token: 0x06002937 RID: 10551 RVA: 0x00118474 File Offset: 0x00116674
		public ItemContent GetEquipmentOnLayer(string layer)
		{
			int value;
			if (this.TryGetLayerIndex(layer, out value))
			{
				return this.Creature.equipment.GetWornContent(this.wardrobeChannel, value);
			}
			return null;
		}

		/// <summary>
		/// Does any of the target layers have equipment on them?
		/// </summary>
		// Token: 0x06002938 RID: 10552 RVA: 0x001184A8 File Offset: 0x001166A8
		public bool HasEquipmentOnAnyLayers(params string[] layers)
		{
			for (int i = 0; i < layers.Length; i++)
			{
				if (this.WardrobeLayersInt.ContainsKey(layers[i]))
				{
					return this.Creature.equipment.GetWornContent(this.wardrobeChannel, this.WardrobeLayersInt[layers[i]]) != null;
				}
			}
			return false;
		}

		/// <summary>
		/// Is this wearable fully empty?
		/// </summary>
		// Token: 0x06002939 RID: 10553 RVA: 0x001184FC File Offset: 0x001166FC
		public bool IsEmpty()
		{
			foreach (Wearable.WearableEntry entry in this.wardrobeLayers)
			{
				if (this.GetEquipmentOnLayer(entry.layer) != null)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Get the layer name of the target item.
		/// This will get the name of the layer from the target item.
		/// </summary>
		// Token: 0x0600293A RID: 10554 RVA: 0x00118534 File Offset: 0x00116734
		public string ItemToLayerName(ItemModuleWardrobe.CreatureWardrobe wardrobe)
		{
			if (wardrobe == null)
			{
				return string.Empty;
			}
			if (wardrobe.manikinWardrobeData == null)
			{
				return string.Empty;
			}
			if (wardrobe.manikinWardrobeData.layers == null)
			{
				return string.Empty;
			}
			if (wardrobe.manikinWardrobeData.layers.Length == 0)
			{
				return string.Empty;
			}
			return this.IndexToLayerName(wardrobe.manikinWardrobeData.layers[0]);
		}

		/// <summary>
		/// Get the name of the target layer.
		/// This takes the index of the layer and returns its name.
		/// </summary>
		// Token: 0x0600293B RID: 10555 RVA: 0x00118598 File Offset: 0x00116798
		public string IndexToLayerName(int layer)
		{
			foreach (KeyValuePair<string, int> key in this.WardrobeLayersInt)
			{
				if (key.Value == layer)
				{
					return key.Key;
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Can this be touched by the target hand?
		/// </summary>
		// Token: 0x0600293C RID: 10556 RVA: 0x00118600 File Offset: 0x00116800
		public override bool CanTouch(RagdollHand ragdollHand)
		{
			this.lastInteractor = ragdollHand;
			return this.Creature.isPlayer && base.CanTouch(ragdollHand) && (this.CanEditFromMirror() || this.Creature.equipment.armourEditModeEnabled);
		}

		/// <summary>
		/// Invoked when a hand has started touching this interactable.
		/// </summary>
		// Token: 0x0600293D RID: 10557 RVA: 0x00118640 File Offset: 0x00116840
		public override void OnTouchStart(RagdollHand ragdollHand)
		{
			this.lastInteractor = ragdollHand;
			this.Creature.equipment.lastSelectedWearable = this;
			Player.local.GetHand(ragdollHand.side).ragdollHand.caster.DisableSpellWheel(this);
			WheelMenuSpell wheel = (ragdollHand.side == Side.Left) ? WheelMenuSpell.left : WheelMenuSpell.right;
			if (wheel.selectorActive)
			{
				wheel.Hide();
			}
			this.SetHintState(null);
			base.OnTouchStart(ragdollHand);
		}

		// Token: 0x0600293E RID: 10558 RVA: 0x001186B8 File Offset: 0x001168B8
		public override void OnTouchStay(RagdollHand ragdollHand)
		{
			this.lastInteractor = ragdollHand;
			base.OnTouchStay(ragdollHand);
			if (this.Creature.isPlayer && ragdollHand.nearestInteractable == this)
			{
				if (this.CanEditFromMirror() || this.Creature.equipment.armourEditModeEnabled)
				{
					this.SetHintState(ragdollHand);
					return;
				}
				this.SetHintState(null);
			}
		}

		/// <summary>
		/// Invoked when a hand has stopped touching this interactable.
		/// </summary>
		// Token: 0x0600293F RID: 10559 RVA: 0x00118718 File Offset: 0x00116918
		public override void OnTouchEnd(RagdollHand ragdollHand)
		{
			this.lastInteractor = ragdollHand;
			base.OnTouchEnd(ragdollHand);
			if (this.Creature.isPlayer)
			{
				this.SetHintState(null);
			}
			this.hasHandExited = true;
			this.SetHintState(null);
			Player.local.GetHand(ragdollHand.side).ragdollHand.caster.AllowSpellWheel(this);
		}

		/// <summary>
		/// Show the visual hint to the player.
		/// </summary>
		// Token: 0x06002940 RID: 10560 RVA: 0x00118775 File Offset: 0x00116975
		public override void ShowHint(RagdollHand ragdollHand)
		{
		}

		/// <summary>
		/// Can the current ragdoll target interact with this slot?
		/// </summary>
		// Token: 0x06002941 RID: 10561 RVA: 0x00118778 File Offset: 0x00116978
		public override Interactable.InteractionResult CheckInteraction(RagdollHand ragdollHand)
		{
			if (!this.CanEditFromMirror() && !this.Creature.equipment.armourEditModeEnabled)
			{
				return this.NoInteraction(ragdollHand);
			}
			if (ragdollHand.grabbedHandle != null && ragdollHand.grabbedHandle.item != null && ragdollHand.grabbedHandle.item.data != null)
			{
				if (ragdollHand.grabbedHandle.item.owner == Item.Owner.Shopkeeper)
				{
					return this.NoInteraction(ragdollHand);
				}
				if (ragdollHand.grabbedHandle.item.data.type != ItemData.Type.Wardrobe)
				{
					return this.NoInteraction(ragdollHand);
				}
				ItemModuleWardrobe module = ragdollHand.grabbedHandle.item.data.GetModule<ItemModuleWardrobe>();
				ItemModuleWardrobe.CreatureWardrobe wardrobeData = (module != null) ? module.GetWardrobe(this.Creature) : null;
				if (wardrobeData != null && wardrobeData.manikinWardrobeData != null)
				{
					int i = 0;
					while (i < wardrobeData.manikinWardrobeData.channels.Length)
					{
						if (string.CompareOrdinal(wardrobeData.manikinWardrobeData.channels[i], this.wardrobeChannel) == 0 && this.WardrobeLayersInt.ContainsKey(this.IndexToLayerName(wardrobeData.manikinWardrobeData.layers[i])))
						{
							if (this.Creature.equipment.GetWornContent(this.wardrobeChannel, wardrobeData.manikinWardrobeData.layers[i]) == null || this.Creature.equipment.canSwapExistingArmour)
							{
								return new Interactable.InteractionResult(ragdollHand, true, true, null, null, null, null, null);
							}
							return new Interactable.InteractionResult(ragdollHand, false, true, null, null, null, null, null);
						}
						else
						{
							i++;
						}
					}
				}
				return this.NoInteraction(ragdollHand);
			}
			else
			{
				ItemContent wardrobeContent = null;
				for (int j = 0; j < this.internalLayerArray.Length; j++)
				{
					wardrobeContent = this.Creature.equipment.GetWornContent(this.wardrobeChannel, this.WardrobeLayersInt[this.internalLayerArray[j]]);
					if (wardrobeContent != null)
					{
						break;
					}
				}
				if (wardrobeContent != null)
				{
					TextData.Item itemLocalization = LocalizationManager.Instance.GetLocalizedTextItem(wardrobeContent.data.localizationId);
					string itemName = (itemLocalization != null) ? itemLocalization.name : wardrobeContent.data.displayName;
					return new Interactable.InteractionResult(ragdollHand, true, true, itemName, this.wardrobeChannel, null, null, null);
				}
				return this.NoInteraction(ragdollHand);
			}
		}

		// Token: 0x06002942 RID: 10562 RVA: 0x001189C4 File Offset: 0x00116BC4
		public override bool TryTouchAction(RagdollHand ragdollHand, Interactable.Action action)
		{
			if (!this.CanEditFromMirror() && !this.Creature.equipment.armourEditModeEnabled)
			{
				return false;
			}
			if (ragdollHand.grabbedHandle != null && action != Interactable.Action.Ungrab)
			{
				action = Interactable.Action.Ungrab;
			}
			if (action != Interactable.Action.Grab)
			{
				if (action == Interactable.Action.Ungrab)
				{
					if ((ragdollHand.grabbedHandle && !Handle.holdGrip && ragdollHand.grabbedHandle.justGrabbed) || (ragdollHand.grabbedHandle && ragdollHand.grabbedHandle.item && ragdollHand.grabbedHandle.item.IsTwoHanded(null)) || (ragdollHand.playerHand && PlayerControl.GetHand(ragdollHand.side).GetHandVelocity().magnitude > Catalog.gameData.throwVelocity))
					{
						return false;
					}
					if (ragdollHand.grabbedHandle && ragdollHand.grabbedHandle.item && ragdollHand.grabbedHandle.item.owner == Item.Owner.Shopkeeper)
					{
						return false;
					}
					Interactable.InteractionResult interactionResult = this.CheckInteraction(ragdollHand);
					if (interactionResult.isInteractable && ragdollHand.grabbedHandle != null && ragdollHand.grabbedHandle.item != null)
					{
						this.RefreshOtherInteractor(ragdollHand);
						this.EquipItem(ragdollHand.grabbedHandle.item);
						return true;
					}
					if (interactionResult.showHint)
					{
						if (interactionResult.audioClip)
						{
							interactionResult.audioClip.PlayClipAtPoint(base.transform.position, 1f, AudioMixerName.Effect);
						}
						return true;
					}
				}
			}
			else
			{
				Interactable.InteractionResult interactionResult = this.CheckInteraction(ragdollHand);
				if (interactionResult.isInteractable)
				{
					ItemContent content = this.Creature.equipment.GetWornContent(this.wardrobeChannel, this.GetWornLayerInt());
					this.UnEquip(content, delegate(Item item)
					{
						if (item != null)
						{
							item.physicBody.rigidBody.velocity = Vector3.zero;
							item.transform.SetPositionAndRotation(this.lastInteractor.transform.position, Quaternion.identity);
							ragdollHand.Grab(item.GetMainHandle(ragdollHand.side), true, false);
						}
					}, false);
				}
				else if (interactionResult.showHint)
				{
					if (interactionResult.audioClip)
					{
						interactionResult.audioClip.PlayClipAtPoint(base.transform.position, 1f, AudioMixerName.Effect);
					}
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Get the top-most layer as its layer int.
		/// </summary>
		// Token: 0x06002943 RID: 10563 RVA: 0x00118C34 File Offset: 0x00116E34
		private int GetWornLayerInt()
		{
			for (int i = 0; i < this.internalLayerArray.Length; i++)
			{
				if (this.Creature.equipment.GetWornContent(this.wardrobeChannel, this.WardrobeLayersInt[this.internalLayerArray[i]]) != null)
				{
					return this.WardrobeLayersInt[this.internalLayerArray[i]];
				}
			}
			return -1;
		}

		/// <summary>
		/// Loop through parts in the target list and return whichever matches the base/alt name.
		/// </summary>
		// Token: 0x06002944 RID: 10564 RVA: 0x00118C94 File Offset: 0x00116E94
		private int FindRendererWithName(List<ManikinPart> mPartList, string baseName, string altName = null)
		{
			if (altName != null)
			{
				for (int i = 0; i < mPartList.Count; i++)
				{
					if (mPartList[i].name.Contains(altName))
					{
						return i;
					}
				}
			}
			for (int j = 0; j < mPartList.Count; j++)
			{
				if (mPartList[j].name.Contains(baseName))
				{
					return j;
				}
			}
			return 0;
		}

		/// <summary>
		/// Queue into the mirror render queue and return the outlining part.
		/// </summary>
		// Token: 0x06002945 RID: 10565 RVA: 0x00118CF4 File Offset: 0x00116EF4
		private Renderer GetAndOutlinePart()
		{
			Renderer part = this.GetWornPartRenderers()[0];
			Mirror.QueueRenderObjects(new Renderer[]
			{
				part
			});
			return part;
		}

		/// <summary>
		/// Default result for readability.
		/// </summary>
		// Token: 0x06002946 RID: 10566 RVA: 0x00118D20 File Offset: 0x00116F20
		private Interactable.InteractionResult NoInteraction(RagdollHand hand)
		{
			return new Interactable.InteractionResult(hand, false, false, null, null, null, null, null);
		}

		/// <summary>
		/// Checks if the current world mirror allows armour to be edited.
		/// </summary>
		// Token: 0x06002947 RID: 10567 RVA: 0x00118D42 File Offset: 0x00116F42
		private bool CanEditFromMirror()
		{
			return !(Mirror.local == null) && Mirror.local.allowArmourEditing && Mirror.local.mirrorMesh.isVisible && Mirror.local.isRendering;
		}

		/// <summary>
		/// Get all layers.
		/// This is to avoid LINQ.
		/// </summary>
		// Token: 0x06002948 RID: 10568 RVA: 0x00118D80 File Offset: 0x00116F80
		private string[] GetAllLayers()
		{
			List<string> layers = new List<string>();
			foreach (Wearable.WearableEntry entry in this.wardrobeLayers)
			{
				layers.Add(entry.layer);
			}
			return layers.ToArray();
		}

		/// <summary>
		/// This is used to retrieve an entry from the layer array.
		/// </summary>
		// Token: 0x06002949 RID: 10569 RVA: 0x00118DC0 File Offset: 0x00116FC0
		private Wearable.WearableEntry GetLayerEntry(string layer)
		{
			foreach (Wearable.WearableEntry entry in this.wardrobeLayers)
			{
				if (string.CompareOrdinal(entry.layer, layer) == 0)
				{
					return entry;
				}
			}
			return null;
		}

		/// <summary>
		/// Play audio from a target location.
		/// </summary>
		// Token: 0x0600294A RID: 10570 RVA: 0x00118DF8 File Offset: 0x00116FF8
		private void PlayAudioFromLocation(IResourceLocation location, float volume_dB)
		{
			if (location == null)
			{
				Debug.LogWarning(this.data.id + " can't play audio, location does not exist.");
				return;
			}
			Catalog.LoadAssetAsync<AudioContainer>(location, delegate(AudioContainer audioContainer)
			{
				if (audioContainer != null)
				{
					audioContainer.PlayClipAtPoint(this.transform.position, volume_dB, AudioMixerName.Master);
					return;
				}
				Debug.LogWarning(location.InternalId + " container not found.");
			}, this.data.id);
		}

		/// <summary>
		/// This updates the interactor hint display state on the opposite hand
		/// if that hand is currently touching the target item, if so and the
		/// item is equipped the hint bubble will float, this is triggered when
		/// an item is equipped, forcing a refresh on the other hands state.
		/// </summary>
		// Token: 0x0600294B RID: 10571 RVA: 0x00118E65 File Offset: 0x00117065
		private void RefreshOtherInteractor(RagdollHand hand)
		{
			Handle grabbedHandle = hand.grabbedHandle;
			if (((grabbedHandle != null) ? grabbedHandle.item : null) != null && hand.otherHand != null)
			{
				this.SetHintState(null);
			}
		}

		/// <summary>
		/// Toggle the hint state.
		/// </summary>
		/// <param name="target">Target hand (if null hint will be hidden).</param>
		// Token: 0x0600294C RID: 10572 RVA: 0x00118E98 File Offset: 0x00117098
		private void SetHintState(RagdollHand target)
		{
			if (!(target != null) || ((!this.CheckInteraction(target).showHint || this.data.highlighterViewHandFreeOnly) && (!this.data.highlighterViewHandFreeOnly || target.grabbedHandle)))
			{
				this.Creature.equipment.leftSelectedPart = null;
				this.Creature.equipment.rightSelectedPart = null;
				Mirror.ClearRenderQueue();
				return;
			}
			Renderer partRend = this.GetAndOutlinePart();
			if (target.side == Side.Left)
			{
				this.Creature.equipment.leftSelectedPart = partRend;
				return;
			}
			this.Creature.equipment.rightSelectedPart = partRend;
		}

		// Token: 0x0600294D RID: 10573 RVA: 0x00118F3E File Offset: 0x0011713E
		private void OnManikinPartsRefreshed(ManikinPart[] partsAdded)
		{
			this.hasHandExited = true;
		}

		// Token: 0x04002742 RID: 10050
		public string wardrobeChannel;

		// Token: 0x04002743 RID: 10051
		public Wearable.WearableEntry[] wardrobeLayers;

		// Token: 0x04002744 RID: 10052
		public Side hudSide;

		// Token: 0x04002745 RID: 10053
		private string[] internalLayerArray;

		// Token: 0x04002746 RID: 10054
		internal bool hasHandExited = true;

		// Token: 0x04002747 RID: 10055
		private RagdollHand lastInteractor;

		// Token: 0x02000A5B RID: 2651
		// (Invoke) Token: 0x060045FA RID: 17914
		public delegate void OnItemEquipped(Item item);

		// Token: 0x02000A5C RID: 2652
		// (Invoke) Token: 0x060045FE RID: 17918
		public delegate void OnItemUnEquipped(string layer, Item item);

		// Token: 0x02000A5D RID: 2653
		// (Invoke) Token: 0x06004602 RID: 17922
		public delegate void OnEditModeChanged(bool state);

		// Token: 0x02000A5E RID: 2654
		[Serializable]
		public class WearableEntry
		{
			// Token: 0x04004800 RID: 18432
			public string layer;

			// Token: 0x04004801 RID: 18433
			public int layerIndex;
		}
	}
}
