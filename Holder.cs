using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using ThunderRoad.Reveal;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000341 RID: 833
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Holder.html")]
	[AddComponentMenu("ThunderRoad/Holder")]
	public class Holder : Interactable
	{
		// Token: 0x17000253 RID: 595
		// (get) Token: 0x060026F1 RID: 9969 RVA: 0x0010C773 File Offset: 0x0010A973
		// (set) Token: 0x060026F2 RID: 9970 RVA: 0x0010C780 File Offset: 0x0010A980
		public new HolderData data
		{
			get
			{
				return (HolderData)this.data;
			}
			set
			{
				this.data = value;
			}
		}

		// Token: 0x1400012B RID: 299
		// (add) Token: 0x060026F3 RID: 9971 RVA: 0x0010C78C File Offset: 0x0010A98C
		// (remove) Token: 0x060026F4 RID: 9972 RVA: 0x0010C7C4 File Offset: 0x0010A9C4
		public event Holder.HolderDelegate Snapped;

		// Token: 0x1400012C RID: 300
		// (add) Token: 0x060026F5 RID: 9973 RVA: 0x0010C7FC File Offset: 0x0010A9FC
		// (remove) Token: 0x060026F6 RID: 9974 RVA: 0x0010C834 File Offset: 0x0010AA34
		public event Holder.HolderDelegate UnSnapped;

		// Token: 0x17000254 RID: 596
		// (get) Token: 0x060026F7 RID: 9975 RVA: 0x0010C86C File Offset: 0x0010AA6C
		public Vector3 expectedPosition
		{
			get
			{
				if (this.creature == null)
				{
					return Vector3.zero;
				}
				if (!this.creature.isPlayer)
				{
					return this.creature.transform.TransformPoint(this.creature.transform.InverseTransformPoint(this.creature.ragdoll.headPart.transform.position) + this.headRelativePosition);
				}
				return this.creature.player.transform.TransformPoint(this.creature.player.transform.InverseTransformPoint(this.creature.ragdoll.headPart.transform.position) + this.headRelativePosition * this.creature.ragdoll.transform.lossyScale.y);
			}
		}

		// Token: 0x060026F8 RID: 9976 RVA: 0x0010C94E File Offset: 0x0010AB4E
		protected virtual void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.slots.Count == 0)
			{
				this.slots.Add(base.transform);
			}
		}

		// Token: 0x060026F9 RID: 9977 RVA: 0x0010C97C File Offset: 0x0010AB7C
		public void FillWithDefault()
		{
			for (int i = this.items.Count - 1; i >= 0; i--)
			{
				if (this.items[i].DisallowDespawn)
				{
					this.items[i].DisallowDespawn = false;
				}
				this.items[i].Despawn();
			}
			this.extraContents.Clear();
			this.currentQuantity = 0;
			ItemData spawnItemData;
			if (Catalog.TryGetData<ItemData>(this.data.spawnItemID, out spawnItemData, true))
			{
				for (int j = 0; j < this.data.spawnQuantity; j++)
				{
					if (j < this.slots.Count)
					{
						spawnItemData.SpawnAsync(delegate(Item item)
						{
							Holder.<>c__DisplayClass35_0 CS$<>8__locals1 = new Holder.<>c__DisplayClass35_0();
							CS$<>8__locals1.<>4__this = this;
							CS$<>8__locals1.item = item;
							try
							{
								this.Snap(CS$<>8__locals1.item, true);
								if (this.data.itemDespawnCondition != HolderData.DespawnProtection.Normal)
								{
									CS$<>8__locals1.item.DisallowDespawn = true;
									if (this.data.itemDespawnCondition != HolderData.DespawnProtection.Never)
									{
										if (this.data.itemDespawnCondition == HolderData.DespawnProtection.ParentItem && this.parentItem)
										{
											this.parentItem.OnDespawnEvent += CS$<>8__locals1.<FillWithDefault>g__ParentDespawn|1;
										}
										else if (this.data.itemDespawnCondition == HolderData.DespawnProtection.ParentCreature && this.parentPart)
										{
											this.parentPart.ragdoll.creature.OnDespawnEvent += CS$<>8__locals1.<FillWithDefault>g__ParentDespawn|1;
										}
									}
								}
							}
							catch (Exception ex)
							{
								Debug.LogError("[Holder] Error spawning item for fillWithDefault: " + ex.Message);
							}
						}, null, null, null, true, null, Item.Owner.None);
					}
					else
					{
						this.extraContents.Add(new ItemContent(spawnItemData, null, null, 1));
						this.currentQuantity++;
					}
				}
				return;
			}
			Debug.LogError("[Holder] Cannot fill with default - ItemData not found: " + this.data.spawnItemID + " on " + base.gameObject.GetPathFromRoot());
		}

		// Token: 0x060026FA RID: 9978 RVA: 0x0010CA9C File Offset: 0x0010AC9C
		public virtual void AlignObject(Item item)
		{
			string targetAnchor = this.editorTargetAnchor;
			if (Application.isPlaying && this.data != null)
			{
				targetAnchor = this.data.targetAnchor;
			}
			Item.HolderPoint hp = item.GetHolderPoint(targetAnchor);
			ItemData.CustomSnap cs = item.GetCustomSnap(this.name);
			Transform itemHolderPoint = (hp != null) ? hp.anchor : item.transform;
			Transform alignmentPoint = this.useAnchor ? this.slots[0].transform : this.slots[this.items.IndexOf(item)].transform;
			Quaternion holderStartLocal = itemHolderPoint.localRotation;
			itemHolderPoint.LocalEulerRotation((cs != null) ? (-cs.localRotation) : Vector3.zero, item.transform);
			Vector3 transformedHolderPoint = item.transform.TransformPoint(item.transform.InverseTransformPoint(itemHolderPoint.position) + ((cs != null) ? cs.localPosition : Vector3.zero));
			Vector3 resultPoint = alignmentPoint.TransformPoint(itemHolderPoint.InverseTransformPoint(transformedHolderPoint));
			item.transform.MoveAlign(itemHolderPoint, resultPoint, alignmentPoint.rotation, alignmentPoint);
			itemHolderPoint.localRotation = holderStartLocal;
		}

		// Token: 0x060026FB RID: 9979 RVA: 0x0010CBB8 File Offset: 0x0010ADB8
		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			if (this.slots == null)
			{
				return;
			}
			if (this.slots.Count == 0)
			{
				Gizmos.matrix = base.transform.localToWorldMatrix;
				Common.DrawGizmoArrow(Vector3.zero, Vector3.forward * 0.1f, Common.HueColourValue(HueColorName.Purple), 0.1f, 10f, false);
				Common.DrawGizmoArrow(Vector3.zero, Vector3.up * 0.05f, Common.HueColourValue(HueColorName.Green), 0.05f, 20f, false);
				return;
			}
			foreach (Transform slot in this.slots)
			{
				if (!(slot == null))
				{
					Gizmos.matrix = slot.localToWorldMatrix;
					Common.DrawGizmoArrow(Vector3.zero, Vector3.forward * 0.1f / 2f, Common.HueColourValue(HueColorName.Purple), 0.05f, 5f, false);
					Common.DrawGizmoArrow(Vector3.zero, Vector3.up * 0.05f / 2f, Common.HueColourValue(HueColorName.Green), 0.025f, 20f, false);
				}
			}
		}

		// Token: 0x060026FC RID: 9980 RVA: 0x0010CD1C File Offset: 0x0010AF1C
		private void OnDrawGizmos()
		{
			Creature creature = this.creature;
			if ((creature != null) ? creature.player : null)
			{
				Gizmos.DrawLine(this.expectedPosition, this.creature.ragdoll.headPart.transform.position);
			}
		}

		// Token: 0x060026FD RID: 9981 RVA: 0x0010CD5C File Offset: 0x0010AF5C
		protected override void Awake()
		{
			base.Awake();
			if (this.slots.Count == 0)
			{
				this.slots.Add(base.transform);
			}
			this.creature = base.GetComponentInParent<Creature>();
			if (this.creature)
			{
				this.parentPart = base.GetComponentInParent<RagdollPart>();
				this.headRelativePosition = this.creature.transform.InverseTransformPointUnscaled(base.transform.position) - this.creature.transform.InverseTransformPointUnscaled(this.creature.ragdoll.headPart.transform.position);
			}
			this.initialTouchRadius = this.touchRadius;
			this.parentItem = base.GetComponentInParent<Item>();
			this.RefreshChildAndParentHolder();
		}

		// Token: 0x060026FE RID: 9982 RVA: 0x0010CE20 File Offset: 0x0010B020
		protected override void Start()
		{
			base.Start();
			using (List<Collider>.Enumerator enumerator = this.ignoredColliders.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == null)
					{
						UnityEngine.Object gameObject = base.gameObject;
						string name = this.name;
						string str = " have ignoredColliders being null | ";
						Transform parent = base.transform.parent;
						Debug.LogErrorFormat(gameObject, name + str + ((parent != null) ? parent.ToString() : null), Array.Empty<object>());
					}
				}
			}
			foreach (Item startObject in this.startObjects)
			{
				this.Snap(startObject.GetComponent<Item>(), true);
			}
		}

		// Token: 0x060026FF RID: 9983 RVA: 0x0010CEF8 File Offset: 0x0010B0F8
		public void RefreshChildAndParentHolder()
		{
			this.parentHolder = (base.transform.parent ? base.transform.parent.gameObject.GetComponentInParent<Holder>() : null);
			foreach (Item item in this.items)
			{
				foreach (Holder holder in item.childHolders)
				{
					holder.RefreshChildAndParentHolder();
				}
			}
		}

		// Token: 0x06002700 RID: 9984 RVA: 0x0010CFB4 File Offset: 0x0010B1B4
		public Holder GetRootHolder()
		{
			if (!this.parentHolder)
			{
				return this;
			}
			return this.parentHolder.GetRootHolder();
		}

		// Token: 0x06002701 RID: 9985 RVA: 0x0010CFD0 File Offset: 0x0010B1D0
		public override void Load(InteractableData interactableData)
		{
			HolderData holderData = interactableData as HolderData;
			if (holderData == null)
			{
				Debug.LogError("Trying to load wrong data type");
				return;
			}
			base.Load(holderData);
			if (this.audioContainer)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioContainer);
			}
			Catalog.LoadAssetAsync<AudioContainer>(holderData.audioContainerLocation, delegate(AudioContainer value)
			{
				this.audioContainer = value;
			}, holderData.id);
			if (!string.IsNullOrEmpty(this.data.spawnItemID) && this.data.spawnQuantity > 0)
			{
				this.FillWithDefault();
			}
		}

		// Token: 0x06002702 RID: 9986 RVA: 0x0010D054 File Offset: 0x0010B254
		private void OnDestroy()
		{
			if (this.audioContainer)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioContainer);
				this.audioContainer = null;
			}
		}

		// Token: 0x06002703 RID: 9987 RVA: 0x0010D075 File Offset: 0x0010B275
		protected override void ManagedOnDisable()
		{
			if (this.cleanItemsCoroutine != null)
			{
				base.StopCoroutine(this.cleanItemsCoroutine);
			}
		}

		// Token: 0x06002704 RID: 9988 RVA: 0x0010D08B File Offset: 0x0010B28B
		public override bool CanTouch(RagdollHand ragdollHand)
		{
			return (this.data.forceAllowTouchOnPlayer && this.creature != null && this.creature.isPlayer) || base.CanTouch(ragdollHand);
		}

		// Token: 0x06002705 RID: 9989 RVA: 0x0010D0BE File Offset: 0x0010B2BE
		public virtual bool GrabFromHandle()
		{
			if (this.creature != null && this.creature.isPlayer)
			{
				return this.data.playerGrabFromHandle;
			}
			return this.data.grabFromHandle;
		}

		// Token: 0x06002706 RID: 9990 RVA: 0x0010D0F2 File Offset: 0x0010B2F2
		public virtual bool ObjectAllowed(Item item)
		{
			return this.data.ObjectAllowed(item);
		}

		// Token: 0x06002707 RID: 9991 RVA: 0x0010D100 File Offset: 0x0010B300
		public virtual int GetMaxQuantity()
		{
			return this.data.maxQuantity;
		}

		// Token: 0x06002708 RID: 9992 RVA: 0x0010D10D File Offset: 0x0010B30D
		public override void OnTouchStart(RagdollHand ragdollHand)
		{
			if (Holder.showHudHighlighter && this.data.highlighterPlayerView && this.creature != null && this.creature.isPlayer)
			{
				this.ShowHint(ragdollHand);
				return;
			}
			base.OnTouchStart(ragdollHand);
		}

		// Token: 0x06002709 RID: 9993 RVA: 0x0010D150 File Offset: 0x0010B350
		public override void OnTouchEnd(RagdollHand ragdollHand)
		{
			base.OnTouchEnd(ragdollHand);
			if (Holder.showHudHighlighter && this.data.highlighterPlayerView && this.creature != null && this.creature.isPlayer)
			{
				if (this.drawSlot == Holder.DrawSlot.BackRight || this.drawSlot == Holder.DrawSlot.HipsRight)
				{
					Highlighter.right.Hide();
					return;
				}
				if (this.drawSlot == Holder.DrawSlot.BackLeft || this.drawSlot == Holder.DrawSlot.HipsLeft)
				{
					Highlighter.left.Hide();
				}
			}
		}

		// Token: 0x0600270A RID: 9994 RVA: 0x0010D1CC File Offset: 0x0010B3CC
		public override void ShowHint(RagdollHand ragdollHand)
		{
			if (Holder.showHudHighlighter && this.creature != null && this.creature.isPlayer)
			{
				Interactable.InteractionResult interactionResult = this.CheckInteraction(ragdollHand);
				if (interactionResult.showHint)
				{
					Holder.DrawSlot drawSlot = this.drawSlot;
					if (drawSlot == Holder.DrawSlot.BackRight || drawSlot == Holder.DrawSlot.HipsRight)
					{
						if (!this.data.highlighterViewHandFreeOnly || (this.data.highlighterViewHandFreeOnly && !ragdollHand.grabbedHandle))
						{
							Highlighter.right.Show(this.data.highlighterPlayerView ? Player.local.head.highlighterRootRight : (this.highlighterTransform ? this.highlighterTransform : base.transform), interactionResult.hintTitle, interactionResult.hintDesignation, Highlighter.Style.Lines, 1.5f, false, interactionResult.altHintDesignation);
							Highlighter.right.SetOutlineColor(interactionResult.hintColor);
							return;
						}
					}
					else
					{
						drawSlot = this.drawSlot;
						if ((drawSlot == Holder.DrawSlot.BackLeft || drawSlot == Holder.DrawSlot.HipsLeft) && (!this.data.highlighterViewHandFreeOnly || (this.data.highlighterViewHandFreeOnly && !ragdollHand.grabbedHandle)))
						{
							Highlighter.left.Show(this.data.highlighterPlayerView ? Player.local.head.highlighterRootLeft : (this.highlighterTransform ? this.highlighterTransform : base.transform), interactionResult.hintTitle, interactionResult.hintDesignation, Highlighter.Style.Lines, 1.5f, false, interactionResult.altHintDesignation);
							Highlighter.left.SetOutlineColor(interactionResult.hintColor);
							return;
						}
					}
				}
			}
			else
			{
				base.ShowHint(ragdollHand);
			}
		}

		// Token: 0x0600270B RID: 9995 RVA: 0x0010D374 File Offset: 0x0010B574
		public string GetQuantityText()
		{
			int maxQuantity = this.GetMaxQuantity();
			if (maxQuantity == 2147483647)
			{
				return this.currentQuantity.ToString();
			}
			return this.currentQuantity.ToString() + " / " + maxQuantity.ToString();
		}

		// Token: 0x0600270C RID: 9996 RVA: 0x0010D3B8 File Offset: 0x0010B5B8
		public virtual bool HasSlotFree()
		{
			return this.currentQuantity < this.GetMaxQuantity();
		}

		// Token: 0x0600270D RID: 9997 RVA: 0x0010D3C8 File Offset: 0x0010B5C8
		public override Interactable.InteractionResult CheckInteraction(RagdollHand ragdollHand)
		{
			if (this.data.delegateToParentHolder && this.parentHolder && !this.parentHolder.hasTriggeredFromParent)
			{
				return this.parentHolder.CheckInteraction(ragdollHand);
			}
			if (this.data.locked)
			{
				return new Interactable.InteractionResult(ragdollHand, false, this.data.warnIfLocked, LocalizationManager.Instance.GetLocalizedString("Default", "Locked", false), LocalizationManager.Instance.GetLocalizedString("Default", "UnableToGrab", false), new Color?(Color.red), null, null);
			}
			this.hasTriggeredFromParent = true;
			bool canGrabParentWithTrigger = this.data.delegateToParentHolder && this.parentHolder && !this.data.grabWithTrigger;
			foreach (Item item in this.items)
			{
				if (item.childHolders.Count != 0 && (!(ragdollHand.grabbedHandle == null) || this.ShouldTouchChildHolder(ragdollHand, item.childHolders[0], null)))
				{
					Interactable.InteractionResult result = item.childHolders[0].CheckInteraction(ragdollHand);
					this.hasTriggeredFromParent = false;
					return result;
				}
			}
			this.hasTriggeredFromParent = false;
			if (ragdollHand.grabbedHandle && ragdollHand.grabbedHandle.item)
			{
				if (ragdollHand.grabbedHandle.data.ignoreSnap)
				{
					return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
				}
				if (!this.HasSlotFree())
				{
					if (this.GetMaxQuantity() > 1)
					{
						TextData.Item itemLocalization = LocalizationManager.Instance.GetLocalizedTextItem(this.data.localizationId);
						string hintTitle;
						if (itemLocalization != null)
						{
							hintTitle = itemLocalization.name;
						}
						else
						{
							hintTitle = (LocalizationManager.Instance.GetLocalizedString("Default", this.data.localizationId, false) ?? this.data.highlightDefaultTitle);
						}
						return new Interactable.InteractionResult(ragdollHand, false, this.data.warnIfInUse, hintTitle, this.GetQuantityText(), new Color?(Color.red), Catalog.gameData.errorAudioGroup.PickAudioClip(0), null);
					}
					return new Interactable.InteractionResult(ragdollHand, false, this.data.warnIfInUse, LocalizationManager.Instance.GetLocalizedString("Default", "AlreadyUsed", false), "", new Color?(Color.red), Catalog.gameData.errorAudioGroup.PickAudioClip(0), null);
				}
				else
				{
					if (ragdollHand.grabbedHandle.item.disableSnap)
					{
						return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
					}
					if (this.ObjectAllowed(ragdollHand.grabbedHandle.item))
					{
						TextData.Item itemLocalization2 = LocalizationManager.Instance.GetLocalizedTextItem(ragdollHand.grabbedHandle.item.data.localizationId);
						string hintTitle2;
						if (itemLocalization2 != null)
						{
							hintTitle2 = itemLocalization2.name;
						}
						else
						{
							hintTitle2 = (LocalizationManager.Instance.GetLocalizedString("Default", this.data.localizationId, false) ?? this.data.highlightDefaultTitle);
						}
						return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle2, (this.GetMaxQuantity() > 1) ? this.GetQuantityText() : (string.IsNullOrEmpty(this.data.highlightDefaultDesignation) ? "" : LocalizationManager.Instance.TryGetLocalization("Default", "{" + this.data.highlightDefaultDesignation + "}", null, false)), null, null, null);
					}
					return new Interactable.InteractionResult(ragdollHand, false, this.data.warnIfNotAllowed, LocalizationManager.Instance.GetLocalizedString("Default", "NotAllowed", false), LocalizationManager.Instance.GetLocalizedString("Default", "SlotNotCompatible", false), new Color?(Color.red), Catalog.gameData.errorAudioGroup.PickAudioClip(1), null);
				}
			}
			else if (this.items.Count > 0)
			{
				Item holdObject = this.items.Last<Item>();
				Interactable.InteractionResult handleResult = holdObject.GetMainHandle(ragdollHand.side).CheckInteraction(ragdollHand);
				if (handleResult.isInteractable)
				{
					if (this.GetMaxQuantity() > 1)
					{
						TextData.Item itemLocalization3 = LocalizationManager.Instance.GetLocalizedTextItem(holdObject.data.localizationId);
						if (itemLocalization3 != null)
						{
							handleResult.hintTitle = itemLocalization3.name;
						}
						else
						{
							string defaultText = LocalizationManager.Instance.GetLocalizedString("Default", this.data.localizationId, false);
							handleResult.hintTitle = (defaultText ?? this.data.highlightDefaultTitle);
						}
						handleResult.hintDesignation = this.GetQuantityText();
						Interactable.InteractionResult interactionResult = handleResult;
						string altHintDesignation;
						if (!canGrabParentWithTrigger)
						{
							altHintDesignation = "";
						}
						else
						{
							TextData.Item localizedTextItem = LocalizationManager.Instance.GetLocalizedTextItem(this.parentItem.data.localizationId);
							if ((altHintDesignation = ((localizedTextItem != null) ? localizedTextItem.name : null)) == null)
							{
								altHintDesignation = (LocalizationManager.Instance.GetLocalizedString("Default", this.parentHolder.data.localizationId, false) ?? this.parentHolder.data.highlightDefaultTitle);
							}
						}
						interactionResult.altHintDesignation = altHintDesignation;
					}
					using (List<Item>.Enumerator enumerator = this.items.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							if (enumerator.Current.childHolders.Count > 0 && string.IsNullOrEmpty(handleResult.hintDesignation))
							{
								handleResult.hintDesignation = LocalizationManager.Instance.GetLocalizedString("Default", "Grab", false);
							}
						}
					}
					return handleResult;
				}
				return handleResult;
			}
			else
			{
				if (this.GetMaxQuantity() > 1)
				{
					TextData.Item itemLocalization4 = LocalizationManager.Instance.GetLocalizedTextItem(this.data.localizationId);
					string hintTitle3;
					if (itemLocalization4 != null)
					{
						hintTitle3 = itemLocalization4.name;
					}
					else
					{
						hintTitle3 = (LocalizationManager.Instance.GetLocalizedString("Default", this.data.localizationId, false) ?? this.data.highlightDefaultTitle);
					}
					return new Interactable.InteractionResult(ragdollHand, false, true, hintTitle3, this.GetQuantityText(), new Color?(Color.gray), null, null);
				}
				return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
			}
			Interactable.InteractionResult result2;
			return result2;
		}

		// Token: 0x0600270E RID: 9998 RVA: 0x0010D9F8 File Offset: 0x0010BBF8
		public bool ShouldTouchChildHolder(RagdollHand hand, Holder child, Interactable.Action? action = null)
		{
			if (child != null && child.data.disableWhenHolstered)
			{
				return false;
			}
			if (child != null && child.data.grabWithTrigger)
			{
				return !(action > Interactable.Action.UseStart);
			}
			PlayerHand playerHand = hand.playerHand;
			return playerHand == null || !playerHand.controlHand.usePressed;
		}

		// Token: 0x0600270F RID: 9999 RVA: 0x0010DA5C File Offset: 0x0010BC5C
		public override bool TryTouchAction(RagdollHand ragdollHand, Interactable.Action action)
		{
			if (this.data.delegateToParentHolder && this.parentHolder && !this.parentHolder.hasTriggeredFromParent)
			{
				return this.parentHolder.TryTouchAction(ragdollHand, action);
			}
			if (this.parentItem)
			{
				this.parentItem.OnTouchAction(ragdollHand, this, action);
			}
			if (this.parentPart)
			{
				this.parentPart.OnTouchAction(ragdollHand, this, action);
			}
			base.TryTouchAction(ragdollHand, action);
			this.hasTriggeredFromParent = true;
			foreach (Item item in this.items)
			{
				foreach (Holder childHolder in item.childHolders)
				{
					if (childHolder.data.delegateToParentHolder && (!(ragdollHand.grabbedHandle == null) || this.ShouldTouchChildHolder(ragdollHand, childHolder, new Interactable.Action?(action))))
					{
						bool result = childHolder.TryTouchAction(ragdollHand, action);
						this.hasTriggeredFromParent = false;
						return result;
					}
				}
			}
			this.hasTriggeredFromParent = false;
			bool withTrigger = this.data.grabWithTrigger && action == Interactable.Action.UseStart;
			if (action != Interactable.Action.Grab)
			{
				if (!withTrigger)
				{
					if (action <= Interactable.Action.UseStop)
					{
						ragdollHand.RefreshTouch();
						return false;
					}
					if (action != Interactable.Action.Ungrab)
					{
						return false;
					}
					if ((ragdollHand.grabbedHandle && !Handle.holdGrip && ragdollHand.grabbedHandle.justGrabbed) || (ragdollHand.grabbedHandle && ragdollHand.grabbedHandle.item && ragdollHand.grabbedHandle.item.IsTwoHanded(null)) || (ragdollHand.playerHand && PlayerControl.GetHand(ragdollHand.side).GetHandVelocity().magnitude > Catalog.gameData.throwVelocity))
					{
						return false;
					}
					Interactable.InteractionResult interactionResult = this.CheckInteraction(ragdollHand);
					if (interactionResult.isInteractable)
					{
						if (ragdollHand.grabbedHandle && ragdollHand.grabbedHandle.item)
						{
							this.Snap(ragdollHand.grabbedHandle.item, false);
							return true;
						}
						return false;
					}
					else
					{
						if (ragdollHand.creature.equipment.lastSelectedWearable != null && ragdollHand.creature.equipment.lastSelectedWearable.TryTouchAction(ragdollHand, action))
						{
							return false;
						}
						if (interactionResult.showHint && interactionResult.audioClip)
						{
							AudioSource.PlayClipAtPoint(interactionResult.audioClip, base.transform.position);
						}
						return true;
					}
				}
			}
			else
			{
				if (this.GrabFromHandle() || ragdollHand.grabbedHandle)
				{
					return false;
				}
				if (!withTrigger)
				{
				}
			}
			Interactable.InteractionResult interactionResult2 = this.CheckInteraction(ragdollHand);
			if (interactionResult2.isInteractable)
			{
				if (this.items.Count > 0)
				{
					Item orgHoldItem = this.items.Last<Item>();
					if (orgHoldItem.GetMainHandle(ragdollHand.side).CheckInteraction(ragdollHand).isInteractable)
					{
						if (orgHoldItem.GetMainHandle(ragdollHand.side).data.useDefaultGripForHolder)
						{
							ragdollHand.Grab(orgHoldItem.GetMainHandle(ragdollHand.side), true, withTrigger);
						}
						else
						{
							ragdollHand.GrabRelative(orgHoldItem.GetMainHandle(ragdollHand.side), withTrigger);
						}
						orgHoldItem.GetMainHandle(ragdollHand.side).justGrabbed = true;
						return true;
					}
				}
			}
			else if (interactionResult2.showHint)
			{
				return true;
			}
			return false;
		}

		// Token: 0x06002710 RID: 10000 RVA: 0x0010DDF4 File Offset: 0x0010BFF4
		public virtual void Snap(Item item, bool silent = false)
		{
			try
			{
				if (this.HasSlotFree())
				{
					bool grabFromHandle = this.GrabFromHandle();
					item.waterHandler.Reset();
					item.ClearZones();
					this.currentQuantity++;
					if (this.items.Count >= this.slots.Count)
					{
						this.extraContents.Add(new ItemContent(item.data, null, null, 1));
						if (!silent && (item.audioContainerSnap != null || this.audioContainer != null))
						{
							(item.audioContainerSnap ? item.audioContainerSnap.PickAudioClip(0) : this.audioContainer.PickAudioClip(0)).PlayClipAtPoint(base.transform.position, 0f, AudioMixerName.Master);
						}
						item.Despawn();
					}
					else
					{
						foreach (Handle handle in item.handles)
						{
							handle.Release();
							handle.SetTouch(grabFromHandle);
							if (handle.data.allowTelekinesis)
							{
								handle.SetTelekinesis(this.data.allowTeleGrab);
							}
						}
						item.physicBody.isEnabled = false;
						item.RefreshCollision(false);
						item.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem), false);
						item.physicBody.collisionDetectionMode = CollisionDetectionMode.Discrete;
						this.items.Add(item);
						this.AlignObject(item);
						this.SetTouch(!grabFromHandle);
						foreach (ColliderGroup colliderGroup in item.colliderGroups)
						{
							foreach (Collider collider in colliderGroup.colliders)
							{
								if (!(collider == null))
								{
									foreach (Collider collider2 in this.ignoredColliders)
									{
										if (!(collider2 == null))
										{
											Physics.IgnoreCollision(collider, collider2, true);
										}
									}
									if (this.ignoredColliders.Count == 0 && !collider.isTrigger)
									{
										collider.enabled = false;
									}
								}
							}
						}
						this.RefreshChildAndParentHolder();
						foreach (Item item2 in this.items)
						{
							foreach (Holder childHolder in item2.childHolders)
							{
								if (!grabFromHandle)
								{
									childHolder.SetTouch(false);
								}
							}
						}
						item.OnSnap(this, silent);
						this.RefreshFPVVisibilityRecursive(item, !this.GetHighestParentHolder().data.hideFromFpv || !this.GetHighestParentHolder().creature || !this.GetHighestParentHolder().creature.player);
						if (this.GetRootHolder().creature && this.GetRootHolder().creature.hidden)
						{
							foreach (Item item3 in this.items)
							{
								item3.Hide(true);
							}
						}
						foreach (RevealDecal revealDecal in item.revealDecals)
						{
							if (revealDecal.revealMaterialController && revealDecal.revealMaterialController.Activated)
							{
								if (this.cleanItemsCoroutine != null)
								{
									base.StopCoroutine(this.cleanItemsCoroutine);
								}
								if (this.data.cleanItemsDuration > 0f)
								{
									this.cleanItemsCoroutine = base.StartCoroutine(this.CleanItemsCoroutine());
									break;
								}
								break;
							}
						}
						Holder.HolderDelegate snapped = this.Snapped;
						if (snapped != null)
						{
							snapped(item);
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("[Holder] Error snapping item: {0}", e));
			}
		}

		/// <summary>
		/// Refresh any child items of the holstered item.
		/// </summary>
		// Token: 0x06002711 RID: 10001 RVA: 0x0010E2E8 File Offset: 0x0010C4E8
		public void RefreshChildItems()
		{
			bool grabFromHandle = this.GrabFromHandle();
			this.SetTouch(!grabFromHandle);
			for (int i = 0; i < this.items.Count; i++)
			{
				Item item = this.items[i];
				foreach (Handle handle in item.handles)
				{
					handle.Release();
					handle.SetTouch(grabFromHandle);
					handle.SetTelekinesis(this.data.allowTeleGrab);
				}
				foreach (Holder childHolder in item.childHolders)
				{
					if (!grabFromHandle)
					{
						childHolder.SetTouch(false);
					}
				}
				this.RefreshFPVVisibilityRecursive(item, !this.GetHighestParentHolder().data.hideFromFpv || !this.GetHighestParentHolder().creature || !this.GetHighestParentHolder().creature.player);
			}
		}

		// Token: 0x06002712 RID: 10002 RVA: 0x0010E418 File Offset: 0x0010C618
		public Holder GetHighestParentHolder()
		{
			if (!(this.parentHolder == null))
			{
				return this.parentHolder.GetHighestParentHolder();
			}
			return this;
		}

		// Token: 0x06002713 RID: 10003 RVA: 0x0010E438 File Offset: 0x0010C638
		public void RefreshFPVVisibilityRecursive(Item affect, bool visible)
		{
			affect.SetMeshLayer(GameManager.GetLayer(visible ? LayerName.DroppedItem : LayerName.FPVHide));
			for (int h = affect.childHolders.Count - 1; h >= 0; h--)
			{
				Holder childHolder = affect.childHolders[h];
				for (int i = childHolder.items.Count - 1; i >= 0; i--)
				{
					this.RefreshFPVVisibilityRecursive(childHolder.items[i], visible);
				}
			}
		}

		// Token: 0x06002714 RID: 10004 RVA: 0x0010E4A8 File Offset: 0x0010C6A8
		private IEnumerator CleanItemsCoroutine()
		{
			float time = 0f;
			float multiplier = this.data.cleanItemsStep / this.data.cleanItemsDuration;
			while (time < this.data.cleanItemsDuration)
			{
				List<RevealMaterialController> revealMaterialController = new List<RevealMaterialController>();
				foreach (Item item in this.items)
				{
					foreach (RevealDecal revealDecal in item.revealDecals)
					{
						if (revealDecal.revealMaterialController && revealDecal.revealMaterialController.Activated)
						{
							revealMaterialController.Add(revealDecal.revealMaterialController);
						}
					}
				}
				RevealMaskProjection.BlitColor(revealMaterialController, new Vector4(multiplier, multiplier, multiplier, multiplier), BlendOp.ReverseSubtract, ColorWriteMask.All);
				time += this.data.cleanItemsStep;
				yield return Yielders.ForSeconds(this.data.cleanItemsStep);
			}
			using (List<Item>.Enumerator enumerator = this.items.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item2 = enumerator.Current;
					foreach (RevealDecal revealDecal2 in item2.revealDecals)
					{
						if (revealDecal2.revealMaterialController && revealDecal2.revealMaterialController.Activated)
						{
							revealDecal2.Reset();
						}
					}
				}
				yield break;
			}
			yield break;
		}

		// Token: 0x06002715 RID: 10005 RVA: 0x0010E4B8 File Offset: 0x0010C6B8
		public virtual void UnSnap(Item item, bool silent = false)
		{
			try
			{
				this.currentQuantity--;
				item.physicBody.isEnabled = true;
				item.transform.SetParent(null, true);
				foreach (Handle handle in item.handles)
				{
					handle.SetTouch(true);
				}
				item.OnUnSnap(this, silent);
				this.RefreshFPVVisibilityRecursive(item, true);
				this.items.Remove(item);
				if (this.items.Count == 0)
				{
					if (this.cleanItemsCoroutine != null)
					{
						base.StopCoroutine(this.cleanItemsCoroutine);
					}
					if (this.GrabFromHandle())
					{
						this.SetTouch(true);
					}
				}
				foreach (Holder holder in item.GetComponentsInChildren<Holder>())
				{
					holder.SetTouch(true);
					holder.RefreshChildAndParentHolder();
				}
				this.RefreshChildAndParentHolder();
				foreach (ColliderGroup colliderGroup in item.colliderGroups)
				{
					foreach (Collider collider in colliderGroup.colliders)
					{
						if (collider)
						{
							foreach (Collider collider2 in this.ignoredColliders)
							{
								if (collider2)
								{
									Physics.IgnoreCollision(collider, collider2, false);
								}
							}
							if (this.ignoredColliders.Count == 0 && !collider.isTrigger)
							{
								collider.enabled = true;
							}
						}
					}
				}
				if (this.GetRootHolder().creature && this.GetRootHolder().creature.hidden)
				{
					item.Hide(false);
				}
				item.RefreshAllowTelekinesis();
				if (this.UnSnapped != null)
				{
					this.UnSnapped(item);
				}
				if (this.extraContents.Count > 0 && base.gameObject.activeInHierarchy)
				{
					base.StartCoroutine(this.RespawnUnSnappedItemContentCoroutine());
				}
				if (Holder.infiniteSupply && this.currentQuantity == 0 && !string.IsNullOrEmpty(this.data.spawnItemID) && this.data.spawnQuantity > 0 && this.data.allowInfiniteSupplyCheat && !item.despawning)
				{
					this.FillWithDefault();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("[Holder] Error unsnapping item: {0}", e));
			}
		}

		// Token: 0x06002716 RID: 10006 RVA: 0x0010E7B0 File Offset: 0x0010C9B0
		private IEnumerator RespawnUnSnappedItemContentCoroutine()
		{
			yield return Yielders.EndOfFrame;
			if (this.extraContents.IsNullOrEmpty())
			{
				yield break;
			}
			ItemContent itemContents = this.extraContents[0];
			itemContents.Spawn(delegate(Item spawnItem)
			{
				this.extraContents.Remove(itemContents);
				this.currentQuantity--;
				this.Snap(spawnItem, true);
			}, Item.Owner.None, true);
			yield break;
		}

		// Token: 0x06002717 RID: 10007 RVA: 0x0010E7C0 File Offset: 0x0010C9C0
		public void UnSnapOneItem(bool destroy)
		{
			Item unsnapped = this.UnSnapOne(true);
			if (unsnapped != null && destroy)
			{
				unsnapped.Despawn();
			}
		}

		// Token: 0x06002718 RID: 10008 RVA: 0x0010E7E8 File Offset: 0x0010C9E8
		public Item UnSnapOne(bool silent = true)
		{
			Item item = this.items.LastOrDefault<Item>();
			if (item)
			{
				this.UnSnap(item, silent);
			}
			return item;
		}

		// Token: 0x06002719 RID: 10009 RVA: 0x0010E814 File Offset: 0x0010CA14
		public virtual void UnSnapAll()
		{
			for (int i = this.items.Count - 1; i >= 0; i--)
			{
				this.UnSnap(this.items[i], true);
			}
		}

		// Token: 0x04002638 RID: 9784
		[Tooltip("If enabled, when the hand is near the holder, it will display a HUD Highlighter")]
		public static bool showHudHighlighter = true;

		// Token: 0x04002639 RID: 9785
		[Tooltip("If set to true, this holder will ignore the JSON item limit, and spawn an infinite supply of the item")]
		public static bool infiniteSupply = false;

		// Token: 0x0400263A RID: 9786
		[Tooltip("This is the position of the holder, assuming this holder has been placed on a creature.")]
		public Holder.DrawSlot drawSlot;

		// Token: 0x0400263B RID: 9787
		[Tooltip("If useAnchor is set to true, the holder will only align the items to the first slot.")]
		public bool useAnchor = true;

		// Token: 0x0400263C RID: 9788
		[Tooltip("Determines the amount of slots in the holder. The number of items inside the holder can bypass the amount of slots, but it will not make new slots for the items to sit in.")]
		public List<Transform> slots = new List<Transform>();

		// Token: 0x0400263D RID: 9789
		[Tooltip("This is a list of items that the holder will start with. For this to take effect, the item must exist in the scene that the holder is in (for example, an item rack)")]
		public List<Item> startObjects = new List<Item>();

		// Token: 0x0400263E RID: 9790
		[Tooltip("GameObjects placed in this holder will not have any physics interactions with any colliders in this list, when the item is taken out (For example, an arrow will not collide with its' quiver)")]
		public List<Collider> ignoredColliders = new List<Collider>();

		// Token: 0x0400263F RID: 9791
		[Tooltip("This value is used to define the target anchor in editor only. This is ignored in game, and is a testing tool only.")]
		public string editorTargetAnchor;

		// Token: 0x04002640 RID: 9792
		[NonSerialized]
		public bool spawningItem;

		// Token: 0x04002641 RID: 9793
		[NonSerialized]
		public List<Item> items = new List<Item>();

		// Token: 0x04002642 RID: 9794
		public int currentQuantity;

		// Token: 0x04002643 RID: 9795
		public List<ItemContent> extraContents = new List<ItemContent>();

		// Token: 0x04002646 RID: 9798
		[NonSerialized]
		public float initialTouchRadius;

		// Token: 0x04002647 RID: 9799
		[NonSerialized]
		public Creature creature;

		// Token: 0x04002648 RID: 9800
		[NonSerialized]
		public RagdollPart parentPart;

		// Token: 0x04002649 RID: 9801
		[NonSerialized]
		public Item parentItem;

		// Token: 0x0400264A RID: 9802
		[NonSerialized]
		public Holder parentHolder;

		// Token: 0x0400264B RID: 9803
		[NonSerialized]
		public Vector3 headRelativePosition;

		// Token: 0x0400264C RID: 9804
		public AudioContainer audioContainer;

		// Token: 0x0400264D RID: 9805
		protected Coroutine cleanItemsCoroutine;

		// Token: 0x0400264E RID: 9806
		[NonSerialized]
		public bool hasTriggeredFromParent;

		// Token: 0x02000A30 RID: 2608
		public enum DrawSlot
		{
			// Token: 0x04004756 RID: 18262
			None,
			// Token: 0x04004757 RID: 18263
			BackRight,
			// Token: 0x04004758 RID: 18264
			BackLeft,
			// Token: 0x04004759 RID: 18265
			HipsRight,
			// Token: 0x0400475A RID: 18266
			HipsLeft
		}

		// Token: 0x02000A31 RID: 2609
		[Serializable]
		public class HolderSize
		{
			// Token: 0x06004580 RID: 17792 RVA: 0x00195EBD File Offset: 0x001940BD
			public List<ValueDropdownItem<string>> GetAllHolderSlots()
			{
				return Catalog.GetDropdownHolderSlots("None");
			}

			// Token: 0x0400475B RID: 18267
			public string slot;
		}

		// Token: 0x02000A32 RID: 2610
		// (Invoke) Token: 0x06004583 RID: 17795
		public delegate void HolderDelegate(Item item);
	}
}
