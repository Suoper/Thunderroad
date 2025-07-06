using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002B0 RID: 688
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Handle.html")]
	[AddComponentMenu("ThunderRoad/Handle")]
	public class Handle : Interactable
	{
		// Token: 0x140000F2 RID: 242
		// (add) Token: 0x0600203E RID: 8254 RVA: 0x000DB150 File Offset: 0x000D9350
		// (remove) Token: 0x0600203F RID: 8255 RVA: 0x000DB188 File Offset: 0x000D9388
		public event Handle.GrabEvent Grabbed;

		// Token: 0x140000F3 RID: 243
		// (add) Token: 0x06002040 RID: 8256 RVA: 0x000DB1C0 File Offset: 0x000D93C0
		// (remove) Token: 0x06002041 RID: 8257 RVA: 0x000DB1F8 File Offset: 0x000D93F8
		public event Handle.GrabEvent UnGrabbed;

		// Token: 0x140000F4 RID: 244
		// (add) Token: 0x06002042 RID: 8258 RVA: 0x000DB230 File Offset: 0x000D9430
		// (remove) Token: 0x06002043 RID: 8259 RVA: 0x000DB268 File Offset: 0x000D9468
		public event Handle.TkEvent TkGrabbed;

		// Token: 0x140000F5 RID: 245
		// (add) Token: 0x06002044 RID: 8260 RVA: 0x000DB2A0 File Offset: 0x000D94A0
		// (remove) Token: 0x06002045 RID: 8261 RVA: 0x000DB2D8 File Offset: 0x000D94D8
		public event Handle.TkEvent TkUnGrabbed;

		// Token: 0x140000F6 RID: 246
		// (add) Token: 0x06002046 RID: 8262 RVA: 0x000DB310 File Offset: 0x000D9510
		// (remove) Token: 0x06002047 RID: 8263 RVA: 0x000DB348 File Offset: 0x000D9548
		public event Handle.SlideEvent SlidingStateChange;

		// Token: 0x140000F7 RID: 247
		// (add) Token: 0x06002048 RID: 8264 RVA: 0x000DB380 File Offset: 0x000D9580
		// (remove) Token: 0x06002049 RID: 8265 RVA: 0x000DB3B8 File Offset: 0x000D95B8
		public event Handle.SwitchHandleEvent SlideToOtherHandle;

		// Token: 0x140000F8 RID: 248
		// (add) Token: 0x0600204A RID: 8266 RVA: 0x000DB3F0 File Offset: 0x000D95F0
		// (remove) Token: 0x0600204B RID: 8267 RVA: 0x000DB428 File Offset: 0x000D9628
		public event Interactable.ActionDelegate OnHeldActionEvent;

		// Token: 0x17000208 RID: 520
		// (get) Token: 0x0600204C RID: 8268 RVA: 0x000DB460 File Offset: 0x000D9660
		public bool IsTkGrabbed
		{
			get
			{
				List<SpellCaster> list = this.telekinesisHandlers;
				return list != null && list.Count > 0;
			}
		}

		// Token: 0x17000209 RID: 521
		// (get) Token: 0x0600204D RID: 8269 RVA: 0x000DB482 File Offset: 0x000D9682
		public SpellCaster MainTkHandler
		{
			get
			{
				if (!this.IsTkGrabbed)
				{
					return null;
				}
				return this.telekinesisHandlers[0];
			}
		}

		// Token: 0x0600204E RID: 8270 RVA: 0x000DB49C File Offset: 0x000D969C
		public virtual void CheckOrientations()
		{
			this.orientations = new List<HandlePose>(base.GetComponentsInChildren<HandlePose>());
			if (this.orientations.Count == 0)
			{
				if (this.allowedOrientations.Count > 0)
				{
					using (List<Handle.Orientation>.Enumerator enumerator = this.allowedOrientations.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Handle.Orientation allowedOrientation = enumerator.Current;
							if (allowedOrientation.allowedHand == Handle.HandSide.Both || allowedOrientation.allowedHand == Handle.HandSide.Right)
							{
								HandlePose handleOrientation = this.AddOrientation(Side.Right, allowedOrientation.positionOffset, Quaternion.Euler(allowedOrientation.rotation));
								if (this.orientationDefaultRight == null && (allowedOrientation.isDefault == Handle.HandSide.Both || allowedOrientation.isDefault == Handle.HandSide.Right))
								{
									this.orientationDefaultRight = handleOrientation;
								}
							}
							if (allowedOrientation.allowedHand == Handle.HandSide.Both || allowedOrientation.allowedHand == Handle.HandSide.Left)
							{
								HandlePose handleOrientation2 = this.AddOrientation(Side.Left, allowedOrientation.positionOffset, Quaternion.Euler(allowedOrientation.rotation));
								if (this.orientationDefaultLeft == null && (allowedOrientation.isDefault == Handle.HandSide.Both || allowedOrientation.isDefault == Handle.HandSide.Left))
								{
									this.orientationDefaultLeft = handleOrientation2;
								}
							}
						}
						return;
					}
				}
				this.orientationDefaultRight = this.AddOrientation(Side.Right, Vector3.zero, Quaternion.identity);
				this.orientationDefaultLeft = this.AddOrientation(Side.Left, Vector3.zero, Quaternion.identity);
			}
		}

		// Token: 0x0600204F RID: 8271 RVA: 0x000DB5F4 File Offset: 0x000D97F4
		public virtual HandlePose AddOrientation(Side side, Vector3 position, Quaternion rotation)
		{
			GameObject gameObject = new GameObject("Orient");
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localPosition = position;
			gameObject.transform.localRotation = rotation;
			gameObject.transform.localScale = Vector3.one;
			HandlePose handleOrientation = gameObject.AddComponent<HandlePose>();
			handleOrientation.side = side;
			this.orientations.Add(handleOrientation);
			return handleOrientation;
		}

		// Token: 0x06002050 RID: 8272 RVA: 0x000DB65E File Offset: 0x000D985E
		public virtual float GetDefaultAxisLocalPosition()
		{
			if (this.axisLength == 0f)
			{
				return 0f;
			}
			return this.defaultGrabAxisRatio * (this.axisLength / 2f);
		}

		// Token: 0x06002051 RID: 8273 RVA: 0x000DB686 File Offset: 0x000D9886
		public virtual Vector3 GetDefaultAxisPosition(Side side)
		{
			return base.transform.TransformPoint(0f, this.GetDefaultAxisLocalPosition(), 0f);
		}

		// Token: 0x06002052 RID: 8274 RVA: 0x000DB6A4 File Offset: 0x000D98A4
		public virtual HandlePose GetDefaultOrientation(Side side)
		{
			if (side == Side.Right && this.orientationDefaultRight)
			{
				return this.orientationDefaultRight;
			}
			if (side == Side.Left && this.orientationDefaultLeft)
			{
				return this.orientationDefaultLeft;
			}
			Debug.LogError("No default orientation found! Please check the prefab " + base.transform.parent.name + "/" + this.name);
			return null;
		}

		// Token: 0x06002053 RID: 8275 RVA: 0x000DB70C File Offset: 0x000D990C
		public virtual HandlePose GetNearestOrientation(Transform grip, Side side)
		{
			float higherDot = float.NegativeInfinity;
			HandlePose orientationResult = null;
			foreach (HandlePose orientation in this.orientations)
			{
				if (orientation.side == side)
				{
					float dot = Vector3.Dot(grip.forward, orientation.transform.rotation * Vector3.forward) + Vector3.Dot(grip.up, orientation.transform.rotation * Vector3.up);
					if (dot > higherDot)
					{
						higherDot = dot;
						orientationResult = orientation;
					}
				}
			}
			return orientationResult;
		}

		// Token: 0x06002054 RID: 8276 RVA: 0x000DB7B8 File Offset: 0x000D99B8
		public virtual bool IsAllowed(Side side)
		{
			foreach (HandlePose orientation in this.orientations)
			{
				if (side == orientation.side)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002055 RID: 8277 RVA: 0x000DB814 File Offset: 0x000D9A14
		public virtual void CalculateReach()
		{
			float farthestDamagerDist = 0f;
			ColliderGroup[] componentsInChildren = base.GetComponentInParent<Item>().GetComponentsInChildren<ColliderGroup>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Collider[] componentsInChildren2 = componentsInChildren[i].GetComponentsInChildren<Collider>();
				for (int j = 0; j < componentsInChildren2.Length; j++)
				{
					Vector3 farthestPoint = componentsInChildren2[j].ClosestPointOnBounds(base.transform.position + base.transform.up.normalized * 10f);
					float dist = base.transform.InverseTransformPoint(farthestPoint).y;
					if (dist > farthestDamagerDist)
					{
						farthestDamagerDist = dist;
					}
				}
			}
			this.reach = farthestDamagerDist - this.GetDefaultAxisLocalPosition();
		}

		// Token: 0x06002056 RID: 8278 RVA: 0x000DB8BF File Offset: 0x000D9ABF
		public virtual void SetUpdatePoses(bool active)
		{
			this.updatePosesAutomatically = active;
		}

		// Token: 0x06002057 RID: 8279 RVA: 0x000DB8C8 File Offset: 0x000D9AC8
		public virtual void Release()
		{
			for (int i = this.handlersCount - 1; i >= 0; i--)
			{
				this.handlers[i].UnGrab(false);
			}
		}

		// Token: 0x06002058 RID: 8280 RVA: 0x000DB8FC File Offset: 0x000D9AFC
		protected virtual void ForcePlayerGrab()
		{
			if (Player.local)
			{
				PlayerHand playerhand = null;
				if (!Player.local.handRight.ragdollHand.grabbedHandle)
				{
					playerhand = Player.local.handRight;
				}
				if (!Player.local.handLeft.ragdollHand.grabbedHandle)
				{
					playerhand = Player.local.handLeft;
				}
				if (playerhand && playerhand.ragdollHand)
				{
					playerhand.ragdollHand.Grab(this);
				}
			}
		}

		// Token: 0x06002059 RID: 8281 RVA: 0x000DB984 File Offset: 0x000D9B84
		protected override void Awake()
		{
			this.CheckOrientations();
			base.Awake();
			this.physicBody = base.gameObject.GetPhysicBodyInParent();
			if (this.physicBody == null)
			{
				Debug.LogError("Handle could not find a physic body in parent! " + base.transform.parent.name + "/" + this.name, base.gameObject);
			}
			base.gameObject.layer = GameManager.GetLayer(LayerName.TouchObject);
			this.handlers = new List<RagdollHand>();
			this.telekinesisHandlers = new List<SpellCaster>();
			this.item = base.GetComponentInParent<Item>();
			if (this.activateHandle)
			{
				this.SetActivateHandle(this.activateHandle);
			}
		}

		// Token: 0x0600205A RID: 8282 RVA: 0x000DBA3C File Offset: 0x000D9C3C
		public void SetActivateHandle(Handle newActivateHandle)
		{
			if (this.activateHandle)
			{
				this.activateHandle.Grabbed -= this.OnActivateHandleGrabbed;
				this.activateHandle.UnGrabbed -= this.OnActivateHandleUnGrabbed;
				this.SetTouchPersistent(true);
			}
			if (newActivateHandle)
			{
				this.SetTouchPersistent(false);
				newActivateHandle.Grabbed += this.OnActivateHandleGrabbed;
				newActivateHandle.UnGrabbed += this.OnActivateHandleUnGrabbed;
				this.activateHandle = newActivateHandle;
			}
		}

		// Token: 0x0600205B RID: 8283 RVA: 0x000DBAC5 File Offset: 0x000D9CC5
		protected void OnActivateHandleGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.SetTouchPersistent(true);
			}
		}

		// Token: 0x0600205C RID: 8284 RVA: 0x000DBAD2 File Offset: 0x000D9CD2
		protected void OnActivateHandleUnGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.SetTouchPersistent(false);
			}
		}

		// Token: 0x0600205D RID: 8285 RVA: 0x000DBAE0 File Offset: 0x000D9CE0
		public override void Load(InteractableData interactableData)
		{
			if (!(interactableData is HandleData))
			{
				Debug.LogError("Trying to load wrong data type");
				return;
			}
			base.Load(interactableData as HandleData);
			this.data = (this.data as HandleData);
			if (this.data.disableOnStart)
			{
				this.SetTouchPersistent(false);
				this.SetTelekinesis(false);
			}
			else
			{
				this.SetTelekinesis(this.data.allowTelekinesis);
			}
			Action<HandleData> action = this.onDataLoaded;
			if (action == null)
			{
				return;
			}
			action(this.data as HandleData);
		}

		// Token: 0x0600205E RID: 8286 RVA: 0x000DBB68 File Offset: 0x000D9D68
		public virtual void SetTelekinesis(bool active)
		{
			if (active)
			{
				if (this.IsAllowed(Side.Left) && this.IsAllowed(Side.Right))
				{
					if (this is HandleRagdoll)
					{
						base.gameObject.tag = HandleData.tagTkRagdoll;
						return;
					}
					base.gameObject.tag = HandleData.tagTkDefault;
					return;
				}
				else
				{
					if (!this.IsAllowed(Side.Left) && !this.IsAllowed(Side.Right))
					{
						base.gameObject.tag = "Untagged";
						return;
					}
					if (!this.IsAllowed(Side.Left))
					{
						if (this is HandleRagdoll)
						{
							base.gameObject.tag = HandleData.tagTkRagdollLeft;
							return;
						}
						base.gameObject.tag = HandleData.tagTkDefaultLeft;
						return;
					}
					else if (!this.IsAllowed(Side.Right))
					{
						if (this is HandleRagdoll)
						{
							base.gameObject.tag = HandleData.tagTkRagdollRight;
							return;
						}
						base.gameObject.tag = HandleData.tagTkDefaultRight;
						return;
					}
				}
			}
			else
			{
				if (this.IsTkGrabbed)
				{
					for (int i = this.telekinesisHandlers.Count - 1; i >= 0; i--)
					{
						SpellCaster spellCaster = this.telekinesisHandlers[i];
						if (spellCaster != null)
						{
							SpellTelekinesis telekinesis = spellCaster.telekinesis;
							if (telekinesis != null)
							{
								telekinesis.TryRelease(false, false);
							}
						}
					}
				}
				if (this.telekinesisHandlers.Count == 0)
				{
					base.gameObject.tag = "Untagged";
				}
			}
		}

		// Token: 0x0600205F RID: 8287 RVA: 0x000DBCA5 File Offset: 0x000D9EA5
		protected virtual bool HoldGripToGrab()
		{
			return this.data.forceHoldGripToGrab || Handle.holdGrip;
		}

		// Token: 0x06002060 RID: 8288 RVA: 0x000DBCBC File Offset: 0x000D9EBC
		public override void OnTouchStart(RagdollHand ragdollHand)
		{
			if (this.item && this.item.holder && !this.item.holder.GrabFromHandle())
			{
				this.item.holder.OnTouchStart(ragdollHand);
				return;
			}
			base.OnTouchStart(ragdollHand);
		}

		// Token: 0x06002061 RID: 8289 RVA: 0x000DBD13 File Offset: 0x000D9F13
		public override void OnTouchStay(RagdollHand ragdollHand)
		{
			base.OnTouchStay(ragdollHand);
			if (Highlighter.isEnabled && this.axisLength > 0f)
			{
				Highlighter.GetSide(ragdollHand.side).axisPosition = this.GetNearestAxisPosition(ragdollHand.grip.position);
			}
		}

		// Token: 0x06002062 RID: 8290 RVA: 0x000DBD54 File Offset: 0x000D9F54
		public override void OnTouchEnd(RagdollHand ragdollHand)
		{
			if (this.item && this.item.holder && !this.item.holder.GrabFromHandle())
			{
				this.item.holder.OnTouchEnd(ragdollHand);
				return;
			}
			base.OnTouchEnd(ragdollHand);
		}

		// Token: 0x06002063 RID: 8291 RVA: 0x000DBDAC File Offset: 0x000D9FAC
		public override Interactable.InteractionResult CheckInteraction(RagdollHand ragdollHand)
		{
			if (this.item && !this.item.gameObject.activeInHierarchy && this.item.holder == null)
			{
				return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
			}
			if (ragdollHand.grabbedHandle)
			{
				return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
			}
			if (!this.IsAllowed(ragdollHand.side))
			{
				return new Interactable.InteractionResult(ragdollHand, false, this.data.warnIfNotAllowed, LocalizationManager.Instance.GetLocalizedString("Default", "NotAllowed", false), LocalizationManager.Instance.GetLocalizedString("Default", "NotAllowedHand", false), new Color?(Color.red), null, null);
			}
			if (this.data.disabledOnSnap)
			{
				Item item = this.item;
				if ((item != null) ? item.holder : null)
				{
					return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
				}
			}
			HandleRagdoll handleRagdoll = this as HandleRagdoll;
			if (handleRagdoll != null && handleRagdoll.ragdollPart == ragdollHand)
			{
				return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
			}
			if (this.item && this.item.IsHanded())
			{
				TextData.Item itemLocalization = LocalizationManager.Instance.GetLocalizedTextItem(this.item.data.localizationId);
				string itemName = (itemLocalization != null) ? itemLocalization.name : this.item.data.displayName;
				string itemHint = (this.item.data.value > 0f) ? (this.item.data.tierString + " / " + this.item.OwnerString) : this.item.data.tierString;
				if (this.IsHanded())
				{
					if (this.axisLength > 0f || this.data.forceAllowTwoHanded)
					{
						if (this.item.leftPlayerHand || this.item.rightPlayerHand)
						{
							return new Interactable.InteractionResult(ragdollHand, true, true, itemName, itemHint, null, null, null);
						}
						if (this.data.allowSteal)
						{
							return new Interactable.InteractionResult(ragdollHand, true, true, itemName, itemHint, null, null, null);
						}
					}
					else if (this.data.allowSwap)
					{
						RagdollHand otherHand = ragdollHand.otherHand;
						Handle otherHandle = (otherHand != null) ? otherHand.grabbedHandle : null;
						if (otherHandle && otherHandle.item == this.item)
						{
							return new Interactable.InteractionResult(ragdollHand, true, true, itemName, itemHint, null, null, null);
						}
					}
				}
				else
				{
					if (this.item.leftPlayerHand || this.item.rightPlayerHand)
					{
						return new Interactable.InteractionResult(ragdollHand, true, true, itemName, itemHint, null, null, null);
					}
					if (this.data.allowSteal)
					{
						return new Interactable.InteractionResult(ragdollHand, true, true, itemName, itemHint, null, null, null);
					}
				}
				return new Interactable.InteractionResult(ragdollHand, false, false, null, null, null, null, null);
			}
			string hintTitle;
			string hintDesignation;
			if (this.item != null)
			{
				if (string.IsNullOrEmpty(this.item.data.localizationId))
				{
					Debug.LogWarning("Item " + this.item.data.id + " has no localization ID set!");
				}
				TextData.Item itemLocalization2 = LocalizationManager.Instance.GetLocalizedTextItem(this.item.data.localizationId);
				hintTitle = ((itemLocalization2 != null) ? itemLocalization2.name : this.item.data.displayName);
				hintDesignation = this.item.data.tierString + " / " + this.item.OwnerString;
			}
			else
			{
				if (!string.IsNullOrEmpty(this.data.localizationId))
				{
					TextData.Item itemLocalization3 = LocalizationManager.Instance.GetLocalizedTextItem(this.data.localizationId);
					if (itemLocalization3 != null)
					{
						hintTitle = itemLocalization3.name;
					}
					else
					{
						hintTitle = (LocalizationManager.Instance.GetLocalizedString("Default", this.data.localizationId, false) ?? this.data.highlightDefaultTitle);
					}
				}
				else
				{
					hintTitle = this.data.highlightDefaultTitle;
				}
				hintDesignation = this.data.highlightDefaultDesignation;
			}
			return new Interactable.InteractionResult(ragdollHand, true, true, hintTitle, hintDesignation, null, null, null);
		}

		// Token: 0x06002064 RID: 8292 RVA: 0x000DC234 File Offset: 0x000DA434
		public override bool TryTouchAction(RagdollHand ragdollHand, Interactable.Action action)
		{
			if (this.item)
			{
				this.item.OnTouchAction(ragdollHand, this, action);
			}
			HandleRagdoll handleRagdoll = this as HandleRagdoll;
			if (handleRagdoll != null)
			{
				handleRagdoll.ragdollPart.OnTouchAction(ragdollHand, this, action);
			}
			base.TryTouchAction(ragdollHand, action);
			if (action == Interactable.Action.Grab)
			{
				if (this.data.disablePinchGrab && PlayerControl.GetHand(ragdollHand.side).pinchPressed && !PlayerControl.GetHand(ragdollHand.side).gripPressed)
				{
					return false;
				}
				if (ragdollHand.grabbedHandle)
				{
					return false;
				}
				Interactable.InteractionResult handleResult = this.CheckInteraction(ragdollHand);
				if (handleResult.isInteractable)
				{
					Item item = this.item;
					if (!((item != null) ? item.holder : null))
					{
						ragdollHand.GrabRelative(this, false);
						this.justGrabbed = true;
						return true;
					}
					Interactable.InteractionResult holderResult = this.item.holder.CheckInteraction(ragdollHand);
					if (holderResult.isInteractable)
					{
						ragdollHand.GrabRelative(this, false);
						this.justGrabbed = true;
						return true;
					}
					if (holderResult.showHint)
					{
						return true;
					}
				}
				else if (handleResult.showHint)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002065 RID: 8293 RVA: 0x000DC340 File Offset: 0x000DA540
		public virtual bool HeldActionAvailable(RagdollHand ragdollHand, Interactable.Action action)
		{
			if (action == Interactable.Action.Grab)
			{
				this.justGrabbed = false;
			}
			if (action == Interactable.Action.Ungrab && this.justGrabbed && !this.HoldGripToGrab())
			{
				this.justGrabbed = false;
				return false;
			}
			return true;
		}

		// Token: 0x06002066 RID: 8294 RVA: 0x000DC36C File Offset: 0x000DA56C
		public virtual void HeldAction(RagdollHand ragdollHand, Interactable.Action action)
		{
			if (action == Interactable.Action.Ungrab)
			{
				ragdollHand.UnGrab(true);
			}
			if (this.item)
			{
				this.item.OnHeldAction(ragdollHand, this, action);
			}
			HandleRagdoll handleRagdoll = this as HandleRagdoll;
			if (handleRagdoll != null)
			{
				handleRagdoll.ragdollPart.OnHeldAction(ragdollHand, handleRagdoll, action);
			}
			Interactable.ActionDelegate onHeldActionEvent = this.OnHeldActionEvent;
			if (onHeldActionEvent != null)
			{
				onHeldActionEvent(ragdollHand, action);
			}
			Interactable.Action configuredStartAction = GameManager.options.invertUseAndSlide ? Interactable.Action.UseStart : Interactable.Action.AlternateUseStart;
			Interactable.Action configuredStopAction = GameManager.options.invertUseAndSlide ? Interactable.Action.UseStop : Interactable.Action.AlternateUseStop;
			if (this.axisLength > 0f)
			{
				if (action == configuredStartAction && this.slideBehavior != Handle.SlideBehavior.DisallowSlide)
				{
					this.SetSliding(ragdollHand, true);
				}
				if (action == configuredStopAction)
				{
					this.SetSliding(ragdollHand, false);
					return;
				}
			}
			else if (this.moveToHandle && action == configuredStartAction && this.handlersCount > 0)
			{
				ragdollHand.UnGrab(false);
				ragdollHand.Grab(this.moveToHandle, this.moveToHandle.GetDefaultOrientation(ragdollHand.side), this.moveToHandleAxisPos, false, false);
			}
		}

		// Token: 0x06002067 RID: 8295 RVA: 0x000DC464 File Offset: 0x000DA664
		public virtual void SetSliding(RagdollHand ragdollHand, bool active)
		{
			if (this.SlidingStateChange != null)
			{
				this.SlidingStateChange(ragdollHand, active, this, ragdollHand.gripInfo.axisPosition, EventTime.OnStart);
			}
			if (active && !ragdollHand.gripInfo.isSliding)
			{
				if (!this.data.allowSlidingWithBothHand)
				{
					RagdollHand otherHand = ragdollHand.otherHand;
					if (((otherHand != null) ? otherHand.gripInfo : null) != null && ragdollHand.otherHand.grabbedHandle == this && ragdollHand.otherHand.gripInfo.isSliding)
					{
						return;
					}
				}
				ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.grabbedHandle.transform.position);
				ragdollHand.gripInfo.isSliding = true;
				ragdollHand.gripInfo.lastSlidePosition = ragdollHand.gripInfo.transform.position;
				this.slideForce = 0f;
				this.RefreshAllJointDrives();
				this.UpdateSliding();
			}
			else if (ragdollHand.gripInfo.isSliding)
			{
				if (this.SlidingStateChange != null)
				{
					this.SlidingStateChange(ragdollHand, false, this, ragdollHand.gripInfo.axisPosition, EventTime.OnStart);
				}
				ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
				ragdollHand.gripInfo.isSliding = false;
				ragdollHand.lastSliding = null;
				this.RefreshAllJointDrives();
				this.UpdateSliding();
			}
			if (this.SlidingStateChange != null)
			{
				this.SlidingStateChange(ragdollHand, active, this, ragdollHand.gripInfo.axisPosition, EventTime.OnEnd);
			}
		}

		// Token: 0x06002068 RID: 8296 RVA: 0x000DC600 File Offset: 0x000DA800
		public virtual void FixedUpdateHandle(RagdollHand ragdollHand)
		{
			if (!this.initialized)
			{
				return;
			}
			Handle.GripInfo gripInfo = ragdollHand.gripInfo;
			if (gripInfo != null && gripInfo.hasPlayerJoint)
			{
				ConfigurableJoint playerJoint = gripInfo.playerJoint;
				PlayerHand playerHand = ragdollHand.playerHand;
				Player player = playerHand.player;
				Transform playerTransform = player.transform;
				playerJoint.targetPosition = playerTransform.InverseTransformPoint(playerHand.grip.position) - playerJoint.anchor;
				playerJoint.targetRotation = Quaternion.Inverse(playerTransform.rotation) * playerHand.grip.rotation;
				playerJoint.anchor = playerTransform.InverseTransformPoint(player.GetShoulderCenter());
			}
			this.UpdateSliding(ragdollHand);
		}

		// Token: 0x06002069 RID: 8297 RVA: 0x000DC6A4 File Offset: 0x000DA8A4
		public virtual void UpdateHandle(RagdollHand ragdollHand)
		{
			if (!this.initialized)
			{
				return;
			}
			if (!this.handlers.Contains(ragdollHand))
			{
				return;
			}
			this.UpdateAutoRotate(ragdollHand);
			this.UpdatePoses(ragdollHand);
			this.UpdateRedirectControllerAxis(ragdollHand);
		}

		// Token: 0x0600206A RID: 8298 RVA: 0x000DC6D4 File Offset: 0x000DA8D4
		protected virtual void UpdateRedirectControllerAxis(RagdollHand ragdollHand)
		{
			if (!this.initialized)
			{
				return;
			}
			if (!this.redirectControllerAxis)
			{
				return;
			}
			if (!ragdollHand.playerHand)
			{
				return;
			}
			PlayerControl.Hand hand = PlayerControl.GetHand(ragdollHand.side);
			UnityEvent<float> unityEvent = this.controllerAxisOutputX;
			if (unityEvent != null)
			{
				unityEvent.Invoke(hand.JoystickAxis.x);
			}
			UnityEvent<float> unityEvent2 = this.controllerAxisOutputY;
			if (unityEvent2 == null)
			{
				return;
			}
			unityEvent2.Invoke(hand.JoystickAxis.y);
		}

		// Token: 0x0600206B RID: 8299 RVA: 0x000DC744 File Offset: 0x000DA944
		protected virtual void UpdateAutoRotate(RagdollHand ragdollHand)
		{
			if (!this.initialized)
			{
				return;
			}
			if (!this.data.rotateAroundAxis)
			{
				return;
			}
			Vector3 handGripToHandBoneDirection = Vector3.ProjectOnPlane(ragdollHand.grip.transform.position - ragdollHand.bone.animation.position, ragdollHand.gripInfo.transform.up);
			float handGripToHandBoneAngle = Vector3.SignedAngle(ragdollHand.grip.forward, handGripToHandBoneDirection, ragdollHand.grip.up);
			Vector3 gripToUpperArmDirection = Vector3.ProjectOnPlane(ragdollHand.gripInfo.transform.position - ragdollHand.lowerArmPart.bone.animation.position, ragdollHand.gripInfo.transform.up);
			ragdollHand.gripInfo.transform.rotation = Quaternion.LookRotation(gripToUpperArmDirection, ragdollHand.gripInfo.transform.up);
			ragdollHand.gripInfo.transform.Rotate(0f, -handGripToHandBoneAngle, 0f, Space.Self);
		}

		// Token: 0x0600206C RID: 8300 RVA: 0x000DC844 File Offset: 0x000DAA44
		protected virtual void UpdateSliding()
		{
			if (this.axisLength == 0f)
			{
				return;
			}
			int count = this.handlersCount;
			for (int i = 0; i < count; i++)
			{
				RagdollHand ragdollHand = this.handlers[i];
				this.UpdateSliding(ragdollHand);
			}
		}

		// Token: 0x0600206D RID: 8301 RVA: 0x000DC888 File Offset: 0x000DAA88
		protected virtual void UpdateSliding(RagdollHand ragdollHand)
		{
			if (!this.initialized)
			{
				return;
			}
			if (this.axisLength == 0f)
			{
				return;
			}
			if (!ragdollHand.playerHand)
			{
				return;
			}
			if (ragdollHand.gripInfo.hasEffect)
			{
				float ratio = Mathf.InverseLerp(this.data.slideFxMinVelocity, this.data.slideFxMaxVelocity, (ragdollHand.gripInfo.transform.position - ragdollHand.gripInfo.lastSlidePosition).magnitude / Time.fixedDeltaTime);
				ragdollHand.gripInfo.effectInstance.SetSpeed(ratio);
				ragdollHand.gripInfo.lastSlidePosition = ragdollHand.gripInfo.transform.position;
			}
			if (ragdollHand.gripInfo.isSliding)
			{
				Vector3 handlerOrientationLocal = ragdollHand.gripInfo.orientation.transform.position - base.transform.position;
				float currentAxisPos = this.GetNearestAxisPosition(ragdollHand.playerHand.grip.position - handlerOrientationLocal);
				float damper = Utils.CalculateRatio(GameManager.options.invertUseAndSlide ? PlayerControl.GetHand(ragdollHand.side).useAxis : PlayerControl.GetHand(ragdollHand.side).alternateUseAxis, 0.2f, 1f, this.data.slideMaxDamper, this.data.slideMinDamper);
				ragdollHand.gripInfo.transform.position = this.GetNearestPositionAlongAxis(ragdollHand.playerHand.grip.position - handlerOrientationLocal) + handlerOrientationLocal;
				ragdollHand.gripInfo.joint.connectedAnchor = (ragdollHand.gripInfo.playerJoint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position));
				JointDrive jointDrive = ragdollHand.gripInfo.joint.yDrive;
				if (currentAxisPos >= this.axisLength / 2f || currentAxisPos <= -(this.axisLength / 2f))
				{
					jointDrive.positionSpring = ragdollHand.creature.GetPositionJointConfig().x * this.data.positionSpringMultiplier;
					jointDrive.positionDamper = ragdollHand.creature.GetPositionJointConfig().y * this.data.positionDamperMultiplier;
				}
				else
				{
					jointDrive.positionSpring = 0f;
					jointDrive.positionDamper = damper;
				}
				ragdollHand.gripInfo.joint.yDrive = jointDrive;
				if (this.data.slideMotorMaxForce > 0f && ragdollHand.playerHand)
				{
					Vector3 slideVector = (base.transform.up.y < 0f) ? base.transform.up : (-base.transform.up);
					if (this.data.slideMotorDir == HandleData.SlideMotorDirection.Down)
					{
						slideVector = -base.transform.up;
					}
					if (this.data.slideMotorDir == HandleData.SlideMotorDirection.Up)
					{
						slideVector = base.transform.up;
					}
					this.slideForce = Mathf.Clamp(this.slideForce + this.data.slideMotorAcceleration, 0f, this.data.slideMotorMaxForce);
					ragdollHand.playerHand.player.locomotion.physicBody.AddForce(slideVector * this.slideForce, ForceMode.Force);
				}
				if (currentAxisPos > this.axisLength / 2f - this.slideToHandleOffset)
				{
					if (this.slideToUpHandle && ragdollHand.lastSliding != this.slideToUpHandle)
					{
						this.SlideHandToOtherHandle(ragdollHand, this.slideToUpHandle, true);
						return;
					}
				}
				else if (currentAxisPos < -(this.axisLength / 2f) + this.slideToHandleOffset)
				{
					if (this.slideToBottomHandle && ragdollHand.lastSliding != this.slideToBottomHandle)
					{
						this.SlideHandToOtherHandle(ragdollHand, this.slideToBottomHandle, true);
						return;
					}
				}
				else
				{
					ragdollHand.lastSliding = null;
				}
			}
		}

		// Token: 0x0600206E RID: 8302 RVA: 0x000DCC78 File Offset: 0x000DAE78
		protected virtual void UpdatePoses(RagdollHand ragdollHand)
		{
			if (!this.initialized)
			{
				return;
			}
			if (!this.updatePosesAutomatically)
			{
				return;
			}
			HandlePose currentPose = ragdollHand.gripInfo.orientation;
			if (!Mathf.Approximately(currentPose.targetWeight, currentPose.lastTargetWeight))
			{
				ragdollHand.poser.SetTargetWeight(currentPose.targetWeight, false);
				currentPose.lastTargetWeight = currentPose.targetWeight;
			}
		}

		// Token: 0x0600206F RID: 8303 RVA: 0x000DCCD4 File Offset: 0x000DAED4
		public virtual void SlideHandToOtherHandle(RagdollHand hand, Handle target, bool silent = true)
		{
			if (this.SlideToOtherHandle != null)
			{
				this.SlideToOtherHandle(hand, this, target, EventTime.OnStart);
			}
			hand.UnGrab(false);
			hand.lastSliding = this;
			bool orgSilent = target.silentGrab;
			if (silent)
			{
				target.silentGrab = true;
			}
			hand.GrabRelative(target, false);
			target.silentGrab = orgSilent;
			target.SetSliding(hand, target.slideBehavior == Handle.SlideBehavior.KeepSlide);
			if (this.SlideToOtherHandle != null)
			{
				this.SlideToOtherHandle(hand, this, target, EventTime.OnEnd);
			}
		}

		// Token: 0x06002070 RID: 8304 RVA: 0x000DCD4D File Offset: 0x000DAF4D
		public virtual bool IsHanded()
		{
			return this.handlers != null && this.handlersCount > 0;
		}

		// Token: 0x06002071 RID: 8305 RVA: 0x000DCD64 File Offset: 0x000DAF64
		public virtual void OnTelekinesisGrab(SpellTelekinesis spellTelekinesis)
		{
			this.telekinesisHandlers.Add(spellTelekinesis.spellCaster);
			if (this.item)
			{
				if (this.item.holder)
				{
					this.item.holder.UnSnap(this.item, false);
				}
				foreach (CollisionHandler collisionHandler in this.item.collisionHandlers)
				{
					collisionHandler.SetPhysicModifier(this, new float?((float)(spellTelekinesis.gravity ? 1 : 0)), 1f, spellTelekinesis.drag, spellTelekinesis.angularDrag, -1f, null);
				}
				this.item.physicBody.sleepThreshold = 0f;
				this.item.StopThrowing();
				this.item.StopFlying();
				this.item.IgnoreIsMoving();
				this.item.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
				this.item.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.telekinesis;
				this.item.SetCenterOfMass(base.transform.localPosition + new Vector3(0f, this.GetDefaultAxisLocalPosition(), 0f));
				this.item.isTelekinesisGrabbed = true;
				if (this.IsTkGrabbed)
				{
					this.item.tkHandlers.Add(spellTelekinesis.spellCaster);
					this.item.OnTelekinesisGrab(this, spellTelekinesis);
					this.item.lastHandler = spellTelekinesis.spellCaster.ragdollHand;
				}
			}
			Handle.TkEvent tkGrabbed = this.TkGrabbed;
			if (tkGrabbed == null)
			{
				return;
			}
			tkGrabbed(this, spellTelekinesis);
		}

		// Token: 0x06002072 RID: 8306 RVA: 0x000DCF24 File Offset: 0x000DB124
		public virtual void OnTelekinesisRelease(SpellTelekinesis spellTelekinesis, bool tryThrow, out bool throwing, bool isGrabbing)
		{
			throwing = false;
			this.telekinesisHandlers.Remove(spellTelekinesis.spellCaster);
			if (this.item)
			{
				this.item.tkHandlers.Remove(spellTelekinesis.spellCaster);
				bool lastHandler = true;
				using (List<Handle>.Enumerator enumerator = this.item.handles.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.IsTkGrabbed)
						{
							lastHandler = false;
							break;
						}
					}
				}
				if (lastHandler)
				{
					foreach (CollisionHandler collisionHandler in this.item.collisionHandlers)
					{
						foreach (Handle handle in this.item.handles)
						{
							collisionHandler.RemovePhysicModifier(handle);
						}
					}
					this.item.ResetCenterOfMass();
					this.item.isTelekinesisGrabbed = false;
					this.item.OnTelekinesisRelease(this, spellTelekinesis, tryThrow, isGrabbing);
					if (tryThrow)
					{
						Vector3 controllerVelocity = Player.local.transform.rotation * PlayerControl.GetHand(spellTelekinesis.spellCaster.ragdollHand.side).GetHandVelocity() * (1f / Time.timeScale);
						bool unPenetrated = false;
						if (controllerVelocity.sqrMagnitude > SpellCaster.throwMinHandVelocity * SpellCaster.throwMinHandVelocity)
						{
							if (this.item.isPenetrating)
							{
								for (int i = this.item.mainCollisionHandler.collisions.Length - 1; i >= 0; i--)
								{
									CollisionInstance collision = this.item.mainCollisionHandler.collisions[i];
									if (!(((collision != null) ? collision.damageStruct.penetrationJoint : null) == null))
									{
										Vector3 direction = -collision.damageStruct.damager.transform.forward;
										if (collision.damageStruct.damager.type == Damager.Type.Pierce && Vector3.Angle(controllerVelocity, direction) < 40f)
										{
											unPenetrated = true;
											collision.damageStruct.damager.UnPenetrate(collision, false);
										}
									}
								}
							}
							this.item.physicBody.velocity = controllerVelocity.normalized * 0.01f;
							this.item.physicBody.AddForce(controllerVelocity.normalized * (spellTelekinesis.pushDefaultForce * this.item.distantGrabThrowRatio * (unPenetrated ? 0.5f : 1f)), ForceMode.VelocityChange);
							this.item.Throw(spellTelekinesis.throwMultiplier, (this.item.HasFlag(ItemFlags.Throwable) || !spellTelekinesis.spinMode) ? Item.FlyDetection.Forced : Item.FlyDetection.Disabled);
							if (spellTelekinesis.clearFloatingOnThrow)
							{
								this.item.Clear("Floating");
							}
							throwing = true;
						}
					}
				}
			}
			Handle.TkEvent tkUnGrabbed = this.TkUnGrabbed;
			if (tkUnGrabbed == null)
			{
				return;
			}
			tkUnGrabbed(this, spellTelekinesis);
		}

		// Token: 0x06002073 RID: 8307 RVA: 0x000DD250 File Offset: 0x000DB450
		public void ReleaseAllTkHandlers()
		{
			if (!this.IsTkGrabbed)
			{
				return;
			}
			for (int i = this.telekinesisHandlers.Count - 1; i >= 0; i--)
			{
				this.telekinesisHandlers[i].telekinesis.TryRelease(false, false);
			}
		}

		// Token: 0x06002074 RID: 8308 RVA: 0x000DD298 File Offset: 0x000DB498
		public virtual void OnGrab(RagdollHand ragdollHand, float axisPosition, HandlePose handlePose, bool teleportToHand = false)
		{
			if (this.physicBody == null)
			{
				Debug.LogError("Handle " + this.name + " has no physic body", base.gameObject);
				return;
			}
			if (handlePose == null)
			{
				Debug.LogError("There is no handle pose assigned to " + this.name + "! This causes arms to break.");
				return;
			}
			if (handlePose.handle == null)
			{
				Debug.LogError("Handle is not assigned on HandlePose " + handlePose.name + ", or the handle reference was set incorrectly and has been despawned! This causes arms to break.");
				return;
			}
			Handle.GrabEvent grabbed = this.Grabbed;
			if (grabbed != null)
			{
				grabbed(ragdollHand, this, EventTime.OnStart);
			}
			Item item = this.item;
			if ((item != null) ? item.holder : null)
			{
				if (this.item.holder.data.grabTeleport == HolderData.GrabTeleport.Enabled)
				{
					teleportToHand = true;
				}
				else if (this.item.holder.data.grabTeleport == HolderData.GrabTeleport.IfParentHolder && this.item.holder.parentHolder)
				{
					teleportToHand = true;
				}
				this.item.holder.UnSnap(this.item, false);
			}
			if (this.item && this.item.isTelekinesisGrabbed)
			{
				foreach (Handle handle in this.item.handles)
				{
					handle.ReleaseAllTkHandlers();
				}
			}
			if (this.releaseHandle)
			{
				Handle grabbableToRelease = this.releaseHandle.GetComponent<Handle>();
				for (int i = grabbableToRelease.handlersCount - 1; i >= 0; i--)
				{
					grabbableToRelease.handlers[i].TryRelease();
				}
			}
			if (this.axisLength == 0f && !this.data.forceAllowTwoHanded)
			{
				this.Release();
			}
			if (handlePose.defaultHandPoseData == null || handlePose.targetHandPoseData == null)
			{
				handlePose.LoadHandPosesData();
			}
			ragdollHand.poser.SetGripFromPose(handlePose.defaultHandPoseData);
			ragdollHand.poser.SetDefaultPose(handlePose.defaultHandPoseData);
			ragdollHand.poser.SetTargetPose(handlePose.targetHandPoseData, false, false, false, false, false);
			ragdollHand.poser.SetTargetWeight(handlePose.targetWeight, false);
			Physics.IgnoreCollision(ragdollHand.touchCollider, this.touchCollider, true);
			if (this.data.disableHandCollider)
			{
				ragdollHand.simplifiedCollider.enabled = false;
				if (ragdollHand.ForeArmColliders != null)
				{
					for (int j = 0; j < ragdollHand.ForeArmColliders.Count; j++)
					{
						if (ragdollHand.ForeArmColliders[j])
						{
							ragdollHand.ForeArmColliders[j].enabled = false;
						}
					}
				}
			}
			Highlighter.GetSide(ragdollHand.side).Hide();
			ragdollHand.gripInfo = this.CreateGripPoint(ragdollHand, axisPosition, handlePose);
			if (ragdollHand.playerHand && (ragdollHand.creature.ragdoll.state == Ragdoll.State.NoPhysic || ragdollHand.creature.ragdoll.state == Ragdoll.State.Kinematic))
			{
				Vector3 orgObjectPosition = this.physicBody.transform.position;
				Quaternion orgObjectRotation = this.physicBody.transform.rotation;
				ragdollHand.creature.ragdoll.ik.SetHandAnchor(ragdollHand.side, ragdollHand.gripInfo.ikAnchor);
				ragdollHand.gripInfo.ikAnchor.position = ragdollHand.gripInfo.transform.TransformPointUnscaled(ragdollHand.grip.InverseTransformPointUnscaled(ragdollHand.transform.position) + new Vector3((ragdollHand.side == Side.Right) ? this.ikAnchorOffset.x : (-this.ikAnchorOffset.x), this.ikAnchorOffset.y, this.ikAnchorOffset.z));
				ragdollHand.gripInfo.ikAnchor.localRotation = Quaternion.Inverse(ragdollHand.grip.rotation) * ragdollHand.transform.rotation;
				this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.playerHand.grip.transform, null);
				if (ragdollHand.gripInfo.joint)
				{
					Debug.LogError("gripInfo.joint already exist");
					UnityEngine.Object.Destroy(ragdollHand.gripInfo.joint);
				}
				ragdollHand.gripInfo.joint = ragdollHand.playerHand.grip.gameObject.AddComponent<ConfigurableJoint>();
				ragdollHand.gripInfo.joint.anchor = Vector3.zero;
				ragdollHand.gripInfo.joint.autoConfigureConnectedAnchor = false;
				ragdollHand.gripInfo.joint.rotationDriveMode = ((this.data.rotationDrive == HandleData.RotationDrive.Slerp && !Handle.globalForceXYZ) ? RotationDriveMode.Slerp : RotationDriveMode.XYAndZ);
				if (this.customRigidBody)
				{
					ragdollHand.gripInfo.joint.connectedAnchor = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
					ragdollHand.gripInfo.joint.connectedBody = this.customRigidBody;
				}
				else
				{
					ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
					ragdollHand.gripInfo.joint.SetConnectedPhysicBody(this.physicBody);
				}
				this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.playerHand.player.locomotion.physicBody.transform, null);
				ragdollHand.gripInfo.playerJoint = ragdollHand.playerHand.player.locomotion.physicBody.gameObject.AddComponent<ConfigurableJoint>();
				ragdollHand.gripInfo.playerJoint.enableCollision = true;
				ragdollHand.gripInfo.playerJoint.autoConfigureConnectedAnchor = false;
				ragdollHand.gripInfo.playerJoint.anchor = Vector3.zero;
				if (this.customRigidBody)
				{
					ragdollHand.gripInfo.playerJoint.connectedAnchor = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
					ragdollHand.gripInfo.playerJoint.connectedBody = this.customRigidBody;
				}
				else
				{
					ragdollHand.gripInfo.playerJoint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
					ragdollHand.gripInfo.playerJoint.SetConnectedPhysicBody(this.physicBody);
				}
				this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.playerHand.grip.transform, null);
				ragdollHand.gripInfo.hasPlayerJoint = true;
				if (!teleportToHand)
				{
					this.physicBody.transform.position = orgObjectPosition;
					this.physicBody.transform.rotation = orgObjectRotation;
				}
				ragdollHand.gripInfo.type = Handle.GripInfo.Type.PlayerJoint;
			}
			else
			{
				RagdollHand otherHand = ragdollHand.otherHand;
				if (((otherHand != null) ? otherHand.grabbedHandle : null) && ragdollHand.otherHand.grabbedHandle.item == this.item)
				{
					ragdollHand.creature.ragdoll.ik.SetHandAnchor(ragdollHand.side, ragdollHand.gripInfo.ikAnchor);
					ragdollHand.gripInfo.ikAnchor.position = ragdollHand.gripInfo.transform.TransformPointUnscaled(ragdollHand.grip.InverseTransformPointUnscaled(ragdollHand.transform.position) + new Vector3((ragdollHand.side == Side.Right) ? this.ikAnchorOffset.x : (-this.ikAnchorOffset.x), this.ikAnchorOffset.y, this.ikAnchorOffset.z));
					ragdollHand.gripInfo.ikAnchor.localRotation = Quaternion.Inverse(ragdollHand.grip.rotation) * ragdollHand.transform.rotation;
					ragdollHand.gripInfo.type = Handle.GripInfo.Type.IKOnly;
				}
				else if (ragdollHand.creature.ragdoll.state == Ragdoll.State.NoPhysic || ragdollHand.creature.ragdoll.state == Ragdoll.State.Kinematic || ragdollHand.creature.ragdoll.state == Ragdoll.State.Disabled)
				{
					this.Attach(ragdollHand, false);
				}
				else
				{
					this.Attach(ragdollHand, true);
				}
			}
			if (this.data.setCenterOfMassTohandle)
			{
				if (this.customRigidBody)
				{
					this.customRigidBody.centerOfMass = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
				}
				else
				{
					this.physicBody.centerOfMass = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
				}
			}
			if (ragdollHand.playerHand)
			{
				ragdollHand.playerHand.player.locomotion.OnGroundEvent += this.OnPlayerLocomotionGroundEvent;
				ragdollHand.playerHand.player.locomotion.OnFlyEvent += this.OnPlayerLocomotionFlyEvent;
			}
			ragdollHand.creature.currentLocomotion.SetSpeedModifier(this.data.handlerLocomotionSpeedMultiplier, 1f, 1f, 1f, 1f, 1f, 1f);
			if (this.data.grabEffectData != null)
			{
				ragdollHand.gripInfo.effectInstance = this.data.grabEffectData.Spawn(ragdollHand.gripInfo.transform.position, ragdollHand.gripInfo.transform.rotation, ragdollHand.gripInfo.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
				ragdollHand.gripInfo.effectInstance.SetIntensity(0f);
				ragdollHand.gripInfo.effectInstance.Play(0, this.silentGrab, false);
				ragdollHand.gripInfo.hasEffect = true;
			}
			if (this.redirectControllerAxis)
			{
				if (ragdollHand.side == PlayerControl.local.locomotionController)
				{
					Player.local.locomotion.allowMove = false;
				}
				else
				{
					Player.local.locomotion.allowTurn = false;
					Player.local.locomotion.allowJump = false;
					Player.local.locomotion.allowCrouch = false;
				}
			}
			if (!this.handlers.Contains(ragdollHand))
			{
				this.handlers.Add(ragdollHand);
			}
			this.handlersCount = this.handlers.Count;
			ragdollHand.grabbedHandle = this;
			ragdollHand.lastHandle = this;
			if (this.item)
			{
				this.item.OnGrab(this, ragdollHand);
			}
			this.RefreshAllJointDrives();
			this.UpdateAutoRotate(ragdollHand);
			if (this.Grabbed != null)
			{
				this.Grabbed(ragdollHand, this, EventTime.OnEnd);
			}
		}

		// Token: 0x06002075 RID: 8309 RVA: 0x000DDD64 File Offset: 0x000DBF64
		public virtual void Attach(RagdollHand ragdollHand, bool usePhysic)
		{
			if (ragdollHand.gripInfo.joint)
			{
				UnityEngine.Object.Destroy(ragdollHand.gripInfo.joint);
			}
			ragdollHand.gripInfo.physicBody = ragdollHand.physicBody;
			if (this.item && this.physicBody == this.item.physicBody)
			{
				this.physicBody.transform.SetParent(null, true);
			}
			if (usePhysic)
			{
				this.physicBody.isKinematic = false;
				this.MoveAndAlignToHand(ragdollHand);
				ragdollHand.gripInfo.joint = ragdollHand.physicBody.gameObject.AddComponent<ConfigurableJoint>();
				ragdollHand.gripInfo.joint.autoConfigureConnectedAnchor = false;
				ragdollHand.gripInfo.joint.anchor = ragdollHand.transform.InverseTransformPoint(ragdollHand.grip.position);
				if (this.customRigidBody)
				{
					ragdollHand.gripInfo.joint.connectedBody = this.customRigidBody;
					ragdollHand.gripInfo.joint.connectedAnchor = this.customRigidBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
				}
				else
				{
					ragdollHand.gripInfo.joint.SetConnectedPhysicBody(this.physicBody);
					ragdollHand.gripInfo.joint.connectedAnchor = this.physicBody.transform.InverseTransformPoint(ragdollHand.gripInfo.transform.position);
				}
				ragdollHand.gripInfo.joint.xMotion = ConfigurableJointMotion.Locked;
				ragdollHand.gripInfo.joint.yMotion = ConfigurableJointMotion.Locked;
				ragdollHand.gripInfo.joint.zMotion = ConfigurableJointMotion.Locked;
				ragdollHand.gripInfo.joint.angularXMotion = ConfigurableJointMotion.Locked;
				ragdollHand.gripInfo.joint.angularYMotion = ConfigurableJointMotion.Locked;
				ragdollHand.gripInfo.joint.angularZMotion = ConfigurableJointMotion.Locked;
				ragdollHand.gripInfo.type = Handle.GripInfo.Type.HandJoint;
				return;
			}
			this.physicBody.isKinematic = true;
			ragdollHand.gripInfo.type = Handle.GripInfo.Type.HandSync;
		}

		// Token: 0x06002076 RID: 8310 RVA: 0x000DDF6C File Offset: 0x000DC16C
		public virtual void MoveAndAlignToHand(RagdollHand ragdollHand)
		{
			Vector3 localPosition = ragdollHand.transform.InverseTransformPointUnscaled(ragdollHand.grip.position);
			Quaternion localRotation = ragdollHand.transform.InverseTransformRotation(ragdollHand.grip.rotation);
			this.physicBody.transform.MoveAlign(ragdollHand.gripInfo.transform, ragdollHand.transform.TransformPoint(localPosition), ragdollHand.transform.TransformRotation(localRotation), null);
		}

		// Token: 0x06002077 RID: 8311 RVA: 0x000DDFDB File Offset: 0x000DC1DB
		protected void OnPlayerLocomotionGroundEvent(Locomotion locomotion, Vector3 groundPoint, Vector3 velocity, Collider groundCollider)
		{
			this.RefreshJointDrive();
		}

		// Token: 0x06002078 RID: 8312 RVA: 0x000DDFE3 File Offset: 0x000DC1E3
		protected void OnPlayerLocomotionFlyEvent(Locomotion locomotion)
		{
			this.RefreshJointDrive();
		}

		// Token: 0x06002079 RID: 8313 RVA: 0x000DDFEC File Offset: 0x000DC1EC
		public virtual void OnUnGrab(RagdollHand ragdollHand, bool throwing)
		{
			if (this.UnGrabbed != null)
			{
				this.UnGrabbed(ragdollHand, this, EventTime.OnStart);
			}
			ragdollHand.poser.ResetDefaultPose();
			ragdollHand.poser.ResetTargetPose();
			ragdollHand.poser.SetTargetWeight(0f, false);
			ragdollHand.ResetGripPositionAndRotation();
			Physics.IgnoreCollision(ragdollHand.touchCollider, this.touchCollider, false);
			if (this.data.disableHandCollider && !ragdollHand.playerHand)
			{
				ragdollHand.simplifiedCollider.enabled = true;
				if (ragdollHand.ForeArmColliders != null)
				{
					for (int i = 0; i < ragdollHand.ForeArmColliders.Count; i++)
					{
						if (ragdollHand.ForeArmColliders[i])
						{
							ragdollHand.ForeArmColliders[i].enabled = true;
						}
					}
				}
			}
			if (ragdollHand.playerHand)
			{
				ragdollHand.playerHand.player.locomotion.OnGroundEvent -= this.OnPlayerLocomotionGroundEvent;
				ragdollHand.playerHand.player.locomotion.OnFlyEvent -= this.OnPlayerLocomotionFlyEvent;
			}
			if (this.item && !this.item.IsHanded(this) && this.physicBody.rigidBody)
			{
				this.item.transform.SetParent(null, true);
			}
			bool disableArmCollision = false;
			if (ragdollHand.gripInfo.type == Handle.GripInfo.Type.PlayerJoint)
			{
				ragdollHand.creature.ragdoll.ik.SetHandAnchor(ragdollHand.side, ragdollHand.transform);
			}
			else if (ragdollHand.gripInfo.type == Handle.GripInfo.Type.HandSync)
			{
				this.physicBody.isKinematic = false;
				disableArmCollision = true;
			}
			if (this.data.setCenterOfMassTohandle)
			{
				if (this.customRigidBody)
				{
					this.customRigidBody.ResetCenterOfMass();
				}
				else if (this.item)
				{
					this.item.ResetCenterOfMass();
				}
			}
			RagdollHand otherHand = ragdollHand.otherHand;
			if (((otherHand != null) ? otherHand.grabbedHandle : null) == null)
			{
				ragdollHand.creature.currentLocomotion.RemoveSpeedModifier(this);
			}
			if (this.redirectControllerAxis)
			{
				if (ragdollHand.side == PlayerControl.local.locomotionController)
				{
					Player.local.locomotion.allowMove = true;
				}
				else
				{
					Player.local.locomotion.allowTurn = true;
					Player.local.locomotion.allowJump = true;
					Player.local.locomotion.allowCrouch = true;
				}
				UnityEvent<float> unityEvent = this.controllerAxisOutputX;
				if (unityEvent != null)
				{
					unityEvent.Invoke(0f);
				}
				UnityEvent<float> unityEvent2 = this.controllerAxisOutputY;
				if (unityEvent2 != null)
				{
					unityEvent2.Invoke(0f);
				}
			}
			ragdollHand.gripInfo.Destroy();
			this.handlers.Remove(ragdollHand);
			this.handlersCount = this.handlers.Count;
			ragdollHand.gripInfo = null;
			ragdollHand.lastHandle = this;
			ragdollHand.grabbedHandle = null;
			if (this.item)
			{
				this.item.OnUnGrab(this, ragdollHand, throwing);
				if (disableArmCollision)
				{
					this.item.IgnoreRagdollCollision(ragdollHand.ragdoll, RagdollPart.Type.LeftArm | RagdollPart.Type.RightArm | RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand);
				}
			}
			this.RefreshAllJointDrives();
			if (this.UnGrabbed != null)
			{
				this.UnGrabbed(ragdollHand, this, EventTime.OnEnd);
			}
		}

		// Token: 0x0600207A RID: 8314 RVA: 0x000DE314 File Offset: 0x000DC514
		public virtual Handle.GripInfo CreateGripPoint(RagdollHand ragdollHand, float axisPosition, HandlePose orientation)
		{
			Handle.GripInfo gripInfo = new Handle.GripInfo(base.transform);
			gripInfo.ragdollHand = ragdollHand;
			gripInfo.orientation = orientation;
			gripInfo.axisPosition = axisPosition;
			gripInfo.transform.position = orientation.transform.position + orientation.handle.transform.up * (axisPosition * orientation.handle.transform.lossyScale.y);
			gripInfo.transform.rotation = orientation.transform.rotation;
			gripInfo.ikAnchor = new GameObject("Anchor").transform;
			gripInfo.ikAnchor.SetParentOrigin(gripInfo.transform);
			return gripInfo;
		}

		// Token: 0x0600207B RID: 8315 RVA: 0x000DE3C8 File Offset: 0x000DC5C8
		public virtual void RefreshAllJointDrives()
		{
			if (this.item)
			{
				using (List<Handle>.Enumerator enumerator = this.item.handles.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Handle handle = enumerator.Current;
						handle.RefreshJointDrive();
					}
					return;
				}
			}
			this.RefreshJointDrive();
		}

		// Token: 0x0600207C RID: 8316 RVA: 0x000DE430 File Offset: 0x000DC630
		public virtual void RefreshJointDrive()
		{
			if (!this.IsHanded())
			{
				return;
			}
			if (this.handlersCount == 1)
			{
				RagdollHand ragdollHand = this.handlers[0];
				if (this.item && this.item.IsTwoHanded(null))
				{
					Vector2 forceRotationSpringDamper2hMult = new Vector2(ragdollHand.creature.data.forceRotationSpringDamper2HMult.x * this.data.rotationSpring2hMultiplier, ragdollHand.creature.data.forceRotationSpringDamper2HMult.y * this.data.rotationDamper2hMultiplier);
					RagdollHand otherHand = ragdollHand.otherHand;
					if (((otherHand != null) ? otherHand.grabbedHandle : null) && ragdollHand.otherHand.grabbedHandle.item && ragdollHand.otherHand.grabbedHandle.item == this.item)
					{
						if (this.data.dominantWhenTwoHanded && ragdollHand.otherHand.grabbedHandle.data.dominantWhenTwoHanded)
						{
							this.SetJointConfig(ragdollHand, ragdollHand.creature.data.forcePositionSpringDamper2HMult, (ragdollHand.side == Handle.dominantHand) ? forceRotationSpringDamper2hMult : Vector2.zero, this.data.rotationDrive);
						}
						else
						{
							this.SetJointConfig(ragdollHand, ragdollHand.creature.data.forcePositionSpringDamper2HMult, this.data.dominantWhenTwoHanded ? forceRotationSpringDamper2hMult : Vector2.zero, this.data.rotationDrive);
						}
					}
					else
					{
						this.SetJointConfig(ragdollHand, ragdollHand.creature.data.forcePositionSpringDamper2HMult, this.data.dominantWhenTwoHanded ? forceRotationSpringDamper2hMult : Vector2.zero, this.data.rotationDrive);
					}
				}
				else
				{
					this.SetJointConfig(ragdollHand, Vector2.one, Vector2.one, this.data.rotationDrive);
				}
			}
			if (this.handlersCount == 2)
			{
				RagdollHand firstHandler = this.handlers[0];
				RagdollHand secondHandler = this.handlers[1];
				if (this.data.dominantWhenTwoHanded && Handle.twoHandedMode != Handle.TwoHandedMode.Position)
				{
					if (Vector3.Dot(firstHandler.gripInfo.transform.up, secondHandler.gripInfo.transform.up) > 0f)
					{
						if (Vector3.Dot(firstHandler.gripInfo.transform.up, base.transform.up) > 0f)
						{
							if (Handle.twoHandedMode == Handle.TwoHandedMode.AutoFront)
							{
								if (this.GetNearestAxisPosition(firstHandler.gripInfo.transform.position) > this.GetNearestAxisPosition(secondHandler.gripInfo.transform.position))
								{
									this.SetJointToTwoHanded(firstHandler.side, 1f);
								}
								else
								{
									this.SetJointToTwoHanded(secondHandler.side, 1f);
								}
							}
							else if (Handle.twoHandedMode == Handle.TwoHandedMode.AutoRear)
							{
								if (this.GetNearestAxisPosition(firstHandler.gripInfo.transform.position) > this.GetNearestAxisPosition(secondHandler.gripInfo.transform.position))
								{
									this.SetJointToTwoHanded(secondHandler.side, 1f);
								}
								else
								{
									this.SetJointToTwoHanded(firstHandler.side, 1f);
								}
							}
							else
							{
								this.SetJointToTwoHanded(Handle.dominantHand, 1f);
							}
						}
						else if (Handle.twoHandedMode == Handle.TwoHandedMode.AutoFront)
						{
							if (this.GetNearestAxisPosition(firstHandler.gripInfo.transform.position) < this.GetNearestAxisPosition(secondHandler.gripInfo.transform.position))
							{
								this.SetJointToTwoHanded(firstHandler.side, 1f);
							}
							else
							{
								this.SetJointToTwoHanded(secondHandler.side, 1f);
							}
						}
						else if (Handle.twoHandedMode == Handle.TwoHandedMode.AutoRear)
						{
							if (this.GetNearestAxisPosition(firstHandler.gripInfo.transform.position) < this.GetNearestAxisPosition(secondHandler.gripInfo.transform.position))
							{
								this.SetJointToTwoHanded(secondHandler.side, 1f);
							}
							else
							{
								this.SetJointToTwoHanded(firstHandler.side, 1f);
							}
						}
						else
						{
							this.SetJointToTwoHanded(Handle.dominantHand, 1f);
						}
					}
					else
					{
						this.SetJointToTwoHanded(Handle.dominantHand, 0.1f);
					}
				}
				else
				{
					this.SetJointToTwoHanded(Handle.dominantHand, 0.1f);
				}
			}
			if (this.handlersCount > 2)
			{
				Debug.LogError("More than 2 handler is not supported right now");
			}
		}

		// Token: 0x0600207D RID: 8317 RVA: 0x000DE884 File Offset: 0x000DCA84
		public virtual void SetJointToTwoHanded(Side dominantSide, float rotationMultiplier = 1f)
		{
			RagdollHand firstHandler = this.handlers.First<RagdollHand>();
			RagdollHand secondHandler = this.handlers.Last<RagdollHand>();
			this.SetJointConfig(firstHandler, firstHandler.creature.data.forceSpringDamper2HNoDomMult, (firstHandler.side == dominantSide) ? (firstHandler.creature.data.forceRotationSpringDamper2HMult * rotationMultiplier) : Vector2.zero, this.data.useWYZWhenTwoHanded ? HandleData.RotationDrive.WYZ : this.data.rotationDrive);
			this.SetJointConfig(secondHandler, secondHandler.creature.data.forceSpringDamper2HNoDomMult, (secondHandler.side == dominantSide) ? (secondHandler.creature.data.forceRotationSpringDamper2HMult * rotationMultiplier) : Vector2.zero, this.data.useWYZWhenTwoHanded ? HandleData.RotationDrive.WYZ : this.data.rotationDrive);
		}

		// Token: 0x0600207E RID: 8318 RVA: 0x000DE95C File Offset: 0x000DCB5C
		public virtual void SetJointDrive(Vector2 positionMultiplier, Vector2 rotationMultiplier)
		{
			foreach (RagdollHand handler in this.handlers)
			{
				this.SetJointConfig(handler, positionMultiplier, rotationMultiplier, this.data.rotationDrive);
			}
		}

		// Token: 0x0600207F RID: 8319 RVA: 0x000DE9BC File Offset: 0x000DCBBC
		public virtual void SetForcePlayerJointModifier(object handler, bool active)
		{
			if (active)
			{
				if (!this.forcePlayerJointHandlers.Contains(handler))
				{
					this.forcePlayerJointHandlers.Add(handler);
				}
				if (!this.forcePlayerJoint)
				{
					this.forcePlayerJoint = true;
					this.RefreshJointDrive();
					return;
				}
			}
			else
			{
				for (int i = 0; i < this.forcePlayerJointHandlers.Count; i++)
				{
					if (this.forcePlayerJointHandlers[i] == handler)
					{
						this.forcePlayerJointHandlers.RemoveAtIgnoreOrder(i);
						i--;
					}
				}
				if (this.forcePlayerJointHandlers.Count == 0 && this.forcePlayerJoint != active && this.forcePlayerJoint)
				{
					this.forcePlayerJoint = false;
					this.RefreshJointDrive();
				}
			}
		}

		// Token: 0x06002080 RID: 8320 RVA: 0x000DEA5C File Offset: 0x000DCC5C
		public virtual void SetJointModifier(object handler, float positionSpringMultiplier = 1f, float positionDamperMultiplier = 1f, float rotationSpringMultiplier = 1f, float rotationDamperMultiplier = 1f)
		{
			Handle.JointModifier jointModifier = null;
			for (int i = 0; i < this.jointModifiers.Count; i++)
			{
				if (this.jointModifiers[i].handler == handler)
				{
					jointModifier = this.jointModifiers[i];
					break;
				}
			}
			if (jointModifier == null)
			{
				jointModifier = new Handle.JointModifier
				{
					handler = handler
				};
				this.jointModifiers.Add(jointModifier);
			}
			jointModifier.positionSpringMultiplier = positionSpringMultiplier;
			jointModifier.positionDamperMultiplier = positionDamperMultiplier;
			jointModifier.rotationSpringMultiplier = rotationSpringMultiplier;
			jointModifier.rotationDamperMultiplier = rotationDamperMultiplier;
			this.RefreshJointModifiers();
		}

		// Token: 0x06002081 RID: 8321 RVA: 0x000DEAE4 File Offset: 0x000DCCE4
		public virtual void RemoveJointModifier(object handler)
		{
			for (int i = 0; i < this.jointModifiers.Count; i++)
			{
				if (this.jointModifiers[i].handler == handler)
				{
					this.jointModifiers.RemoveAtIgnoreOrder(i);
					i--;
				}
			}
			this.RefreshJointModifiers();
		}

		// Token: 0x06002082 RID: 8322 RVA: 0x000DEB31 File Offset: 0x000DCD31
		public virtual void ClearJointModifiers()
		{
			this.jointModifiers.Clear();
			this.RefreshJointModifiers();
		}

		// Token: 0x06002083 RID: 8323 RVA: 0x000DEB44 File Offset: 0x000DCD44
		public virtual void RefreshJointModifiers()
		{
			this.positionSpringMultiplier = 1f;
			this.positionDamperMultiplier = 1f;
			this.rotationSpringMultiplier = 1f;
			this.rotationDamperMultiplier = 1f;
			foreach (Handle.JointModifier jointModifier in this.jointModifiers)
			{
				if (jointModifier.positionSpringMultiplier >= 0f && jointModifier.positionSpringMultiplier != 1f)
				{
					this.positionSpringMultiplier *= jointModifier.positionSpringMultiplier;
				}
				if (jointModifier.positionDamperMultiplier >= 0f && jointModifier.positionDamperMultiplier != 1f)
				{
					this.positionDamperMultiplier *= jointModifier.positionDamperMultiplier;
				}
				if (jointModifier.rotationSpringMultiplier >= 0f && jointModifier.rotationSpringMultiplier != 1f)
				{
					this.rotationSpringMultiplier *= jointModifier.rotationSpringMultiplier;
				}
				if (jointModifier.rotationDamperMultiplier >= 0f && jointModifier.rotationDamperMultiplier != 1f)
				{
					this.rotationDamperMultiplier *= jointModifier.rotationDamperMultiplier;
				}
			}
			this.RefreshJointDrive();
		}

		// Token: 0x06002084 RID: 8324 RVA: 0x000DEC7C File Offset: 0x000DCE7C
		public virtual void SetJointConfig(RagdollHand handler, Vector2 positionMultiplier, Vector2 rotationMultiplier, HandleData.RotationDrive rotationDrive)
		{
			if (!handler.creature.player)
			{
				return;
			}
			if (handler.creature.player.locomotion.isGrounded && !this.data.forceClimbing && !this.forcePlayerJoint && (!this.item || !this.item.data.grabAndGripClimb))
			{
				if (handler.gripInfo.joint)
				{
					this.SetJointConfig(handler.gripInfo.joint, handler, positionMultiplier, rotationMultiplier, handler.creature.data.forceMaxPosition, handler.creature.data.forceMaxRotation, rotationDrive, 0f);
				}
				if (handler.gripInfo.playerJoint)
				{
					this.SetJointConfig(handler.gripInfo.playerJoint, handler, Vector2.zero, Vector2.zero, 0f, 0f, rotationDrive, 0f);
				}
				this.playerJointActive = false;
				return;
			}
			if (handler.gripInfo.joint)
			{
				this.SetJointConfig(handler.gripInfo.joint, handler, Vector2.zero, Vector2.zero, 0f, 0f, rotationDrive, 0f);
			}
			if (handler.gripInfo.playerJoint)
			{
				if (this.ignoreClimbingForceOverride)
				{
					this.SetJointConfig(handler.gripInfo.playerJoint, handler, positionMultiplier, rotationMultiplier, handler.creature.data.forceMaxPosition, handler.creature.data.forceMaxRotation, rotationDrive, handler.playerHand.player.creature.morphology.GetArmCenterToFingerTipLenght());
				}
				else
				{
					this.SetJointConfig(handler.gripInfo.playerJoint, handler, handler.creature.data.climbingForcePositionSpringDamperMult * positionMultiplier, rotationMultiplier, handler.creature.data.climbingForceMaxPosition, handler.creature.data.climbingForceMaxRotation, rotationDrive, handler.playerHand.player.creature.morphology.GetArmCenterToFingerTipLenght());
				}
			}
			this.playerJointActive = true;
		}

		// Token: 0x06002085 RID: 8325 RVA: 0x000DEEA4 File Offset: 0x000DD0A4
		public virtual void SetJointConfig(ConfigurableJoint joint, RagdollHand handler, Vector2 positionMultiplier, Vector2 rotationMultiplier, float maxPositionForce, float maxRotationForce, HandleData.RotationDrive rotationDrive, float limit = 0f)
		{
			JointDrive jointDrive = default(JointDrive);
			JointDrive rotationJointDrive = default(JointDrive);
			JointDrive rotationJointDrive2 = default(JointDrive);
			jointDrive.positionSpring = handler.creature.GetPositionJointConfig().x * this.data.positionSpringMultiplier * positionMultiplier.x * Handle.globalPositionSpringMultiplier * this.positionSpringMultiplier;
			jointDrive.positionDamper = handler.creature.GetPositionJointConfig().y * this.data.positionDamperMultiplier * positionMultiplier.y * Handle.globalPositionDamperMultiplier * this.positionDamperMultiplier;
			jointDrive.maximumForce = maxPositionForce * positionMultiplier.x * handler.creature.GetRotationJointConfig().z;
			joint.xDrive = jointDrive;
			joint.yDrive = jointDrive;
			joint.zDrive = jointDrive;
			rotationJointDrive.positionSpring = handler.creature.GetRotationJointConfig().x * this.data.rotationSpringMultiplier * rotationMultiplier.x * Handle.globalRotationSpringMultiplier * this.rotationSpringMultiplier;
			rotationJointDrive.positionDamper = handler.creature.GetRotationJointConfig().y * this.data.rotationDamperMultiplier * rotationMultiplier.y * Handle.globalRotationDamperMultiplier * this.rotationDamperMultiplier;
			rotationJointDrive.maximumForce = maxRotationForce * rotationMultiplier.x * handler.creature.GetRotationJointConfig().z;
			joint.angularXDrive = ((rotationDrive == HandleData.RotationDrive.X || rotationDrive == HandleData.RotationDrive.WYZ || Handle.globalForceXYZ) ? rotationJointDrive : rotationJointDrive2);
			joint.angularYZDrive = ((rotationDrive == HandleData.RotationDrive.YZ || rotationDrive == HandleData.RotationDrive.WYZ || Handle.globalForceXYZ) ? rotationJointDrive : rotationJointDrive2);
			joint.slerpDrive = ((rotationDrive == HandleData.RotationDrive.Slerp && !Handle.globalForceXYZ) ? rotationJointDrive : rotationJointDrive2);
			joint.rotationDriveMode = ((rotationDrive == HandleData.RotationDrive.Slerp && !Handle.globalForceXYZ) ? RotationDriveMode.Slerp : RotationDriveMode.XYAndZ);
			if (limit > 0f)
			{
				joint.xMotion = ConfigurableJointMotion.Limited;
				joint.yMotion = ConfigurableJointMotion.Limited;
				joint.zMotion = ConfigurableJointMotion.Limited;
				SoftJointLimit softJointLimit = new SoftJointLimit
				{
					limit = limit
				};
				joint.linearLimit = softJointLimit;
			}
			else
			{
				joint.xMotion = ConfigurableJointMotion.Free;
				joint.yMotion = ConfigurableJointMotion.Free;
				joint.zMotion = ConfigurableJointMotion.Free;
			}
			joint.angularXMotion = ConfigurableJointMotion.Free;
			joint.angularYMotion = ConfigurableJointMotion.Free;
			joint.angularZMotion = ConfigurableJointMotion.Free;
			this.lastPositionMultiplier = positionMultiplier;
			this.lastRotationMultiplier = rotationMultiplier;
		}

		// Token: 0x06002086 RID: 8326 RVA: 0x000DF0D4 File Offset: 0x000DD2D4
		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(new Vector3(0f, this.GetDefaultAxisLocalPosition(), 0f), this.reach);
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(new Vector3(0f, this.GetDefaultAxisLocalPosition(), 0f), 0.03f);
		}

		// Token: 0x04001F46 RID: 8006
		[Range(-1f, 1f)]
		[Tooltip("Define where the handle is automatically grabbed along the axis length")]
		public float defaultGrabAxisRatio;

		// Token: 0x04001F47 RID: 8007
		public Vector3 ikAnchorOffset;

		// Token: 0x04001F48 RID: 8008
		[NonSerialized]
		public List<HandlePose> orientations = new List<HandlePose>();

		// Token: 0x04001F49 RID: 8009
		[Tooltip("The default handpose to be grabbed (Right Hand)")]
		public HandlePose orientationDefaultRight;

		// Token: 0x04001F4A RID: 8010
		[Tooltip("The default handpose to be grabbed (Left Hand)")]
		public HandlePose orientationDefaultLeft;

		// Token: 0x04001F4B RID: 8011
		[Tooltip("When linked handle is grabbed, ungrip this handle.")]
		public Handle releaseHandle;

		// Token: 0x04001F4C RID: 8012
		[Tooltip("Handle will only activate when the linked handle is grabbed.")]
		public Handle activateHandle;

		// Token: 0x04001F4D RID: 8013
		[Tooltip("NPCs will try to grab this handle if their brain logic tells them to.")]
		public Handle AIGrabHandle;

		// Token: 0x04001F4E RID: 8014
		[Tooltip("When ticked, no sound will play when the handle is grabbed")]
		public bool silentGrab;

		// Token: 0x04001F4F RID: 8015
		[Tooltip("When ticked, the player will ungrab the handle when grounded (touching the ground)")]
		public bool forceAutoDropWhenGrounded;

		// Token: 0x04001F50 RID: 8016
		[Tooltip("The default handpose to be grabbed (Right Hand)")]
		public bool ignoreClimbingForceOverride;

		// Token: 0x04001F51 RID: 8017
		[Tooltip("If the player can cast spells while holding this handle, this transform defines where the orb will appear.")]
		public Transform spellOrbTarget;

		// Token: 0x04001F52 RID: 8018
		[Tooltip("Lets AI know how far the item is away from the player. You can use the button to calculate this automatically, so long as a ColliderGroup is set up with sufficient colliders.")]
		public float reach = 0.5f;

		// Token: 0x04001F53 RID: 8019
		[Tooltip("(Optional)Disables listed colliders once the handle is grabbed.")]
		public List<Collider> handOverlapColliders;

		// Token: 0x04001F54 RID: 8020
		[Tooltip("(Optional) Allows you to add a custom rigidbody to the handle. (Do not reference item!)")]
		public Rigidbody customRigidBody;

		// Token: 0x04001F55 RID: 8021
		[Tooltip("(Optional) When player hand reaches the top of the handle via slide, it will switch to listed handle once the top is reached.")]
		public Handle slideToUpHandle;

		// Token: 0x04001F56 RID: 8022
		[Tooltip("(Optional) When player hand reaches the bottom of the handle via slide, it will switch to listed handle once the bottom is reached.")]
		public Handle slideToBottomHandle;

		// Token: 0x04001F57 RID: 8023
		[Tooltip("(Optional) Offset of the bottom and top slide up handles. Can switch handle when reaching 0.2 meters away from the top/bottom, for example.")]
		public float slideToHandleOffset = 0.01f;

		// Token: 0x04001F58 RID: 8024
		[Tooltip("Allows you to enable/disable handle sliding (Only works on handles with a length greater than 0!)")]
		public Handle.SlideBehavior slideBehavior;

		// Token: 0x04001F59 RID: 8025
		[Tooltip("(Optional) When you slide up the handle, and the axis length is 0, sliding will instead snap to the referenced handle.")]
		public Handle moveToHandle;

		// Token: 0x04001F5A RID: 8026
		[Tooltip("Axis Position for the \"Move To Handle\" handle")]
		public float moveToHandleAxisPos;

		// Token: 0x04001F5B RID: 8027
		[Tooltip("When this box is checked, hand poses will update whenever the target weight changes or whenever the pose data changes.")]
		public bool updatePosesAutomatically;

		// Token: 0x04001F5C RID: 8028
		[Header("Here, you can add a list of orientations for the handle. Once done, you can click the update button for the new orientations, and it will do it automatically.\n\nThis field is old/obsolete, and is only optional.")]
		public List<Handle.Orientation> allowedOrientations = new List<Handle.Orientation>();

		// Token: 0x04001F5D RID: 8029
		[Header("Controller axis override")]
		public bool redirectControllerAxis;

		// Token: 0x04001F5E RID: 8030
		public UnityEvent<float> controllerAxisOutputX = new UnityEvent<float>();

		// Token: 0x04001F5F RID: 8031
		public UnityEvent<float> controllerAxisOutputY = new UnityEvent<float>();

		// Token: 0x04001F60 RID: 8032
		[NonSerialized]
		public new HandleData data;

		// Token: 0x04001F61 RID: 8033
		public static bool holdGrip = false;

		// Token: 0x04001F62 RID: 8034
		public static Side dominantHand = Side.Right;

		// Token: 0x04001F63 RID: 8035
		public static Handle.TwoHandedMode twoHandedMode = Handle.TwoHandedMode.AutoFront;

		// Token: 0x04001F64 RID: 8036
		public static float globalPositionSpringMultiplier = 1f;

		// Token: 0x04001F65 RID: 8037
		public static float globalPositionDamperMultiplier = 1f;

		// Token: 0x04001F66 RID: 8038
		public static float globalRotationSpringMultiplier = 1f;

		// Token: 0x04001F67 RID: 8039
		public static float globalRotationDamperMultiplier = 1f;

		// Token: 0x04001F68 RID: 8040
		public static bool globalForceXYZ = false;

		// Token: 0x04001F69 RID: 8041
		protected Vector2 lastPositionMultiplier = new Vector2(1f, 1f);

		// Token: 0x04001F6A RID: 8042
		protected Vector2 lastRotationMultiplier = new Vector2(1f, 1f);

		// Token: 0x04001F6B RID: 8043
		[NonSerialized]
		public Item item;

		// Token: 0x04001F6C RID: 8044
		[NonSerialized]
		public PhysicBody physicBody;

		// Token: 0x04001F6D RID: 8045
		[NonSerialized]
		public bool playerJointActive;

		// Token: 0x04001F6E RID: 8046
		protected bool forcePlayerJoint;

		// Token: 0x04001F6F RID: 8047
		protected List<object> forcePlayerJointHandlers = new List<object>();

		// Token: 0x04001F74 RID: 8052
		protected float positionSpringMultiplier = 1f;

		// Token: 0x04001F75 RID: 8053
		protected float positionDamperMultiplier = 1f;

		// Token: 0x04001F76 RID: 8054
		protected float rotationSpringMultiplier = 1f;

		// Token: 0x04001F77 RID: 8055
		protected float rotationDamperMultiplier = 1f;

		// Token: 0x04001F78 RID: 8056
		[NonSerialized]
		public List<Handle.JointModifier> jointModifiers = new List<Handle.JointModifier>();

		// Token: 0x04001F7C RID: 8060
		public List<RagdollHand> handlers;

		// Token: 0x04001F7D RID: 8061
		protected int handlersCount;

		// Token: 0x04001F7E RID: 8062
		[NonSerialized]
		public List<SpellCaster> telekinesisHandlers = new List<SpellCaster>();

		// Token: 0x04001F7F RID: 8063
		[NonSerialized]
		public Action<HandleData> onDataLoaded;

		// Token: 0x04001F80 RID: 8064
		[NonSerialized]
		public bool justGrabbed;

		// Token: 0x04001F81 RID: 8065
		protected float slideForce;

		// Token: 0x0200094A RID: 2378
		public new enum HandSide
		{
			// Token: 0x04004448 RID: 17480
			None,
			// Token: 0x04004449 RID: 17481
			Right,
			// Token: 0x0400444A RID: 17482
			Left,
			// Token: 0x0400444B RID: 17483
			Both
		}

		// Token: 0x0200094B RID: 2379
		public enum SlideBehavior
		{
			// Token: 0x0400444D RID: 17485
			CanSlide,
			// Token: 0x0400444E RID: 17486
			KeepSlide,
			// Token: 0x0400444F RID: 17487
			DisallowSlide
		}

		// Token: 0x0200094C RID: 2380
		[Serializable]
		public class Orientation
		{
			// Token: 0x06004312 RID: 17170 RVA: 0x0018E7C2 File Offset: 0x0018C9C2
			public Orientation(Vector3 position, Vector3 rotation, Handle.HandSide allowedHand, Handle.HandSide isDefault)
			{
				this.rotation = rotation;
				this.positionOffset = position;
				this.allowedHand = allowedHand;
				this.isDefault = isDefault;
			}

			// Token: 0x04004450 RID: 17488
			public Vector3 rotation;

			// Token: 0x04004451 RID: 17489
			public Vector3 positionOffset;

			// Token: 0x04004452 RID: 17490
			public Handle.HandSide allowedHand = Handle.HandSide.Both;

			// Token: 0x04004453 RID: 17491
			public Handle.HandSide isDefault;
		}

		// Token: 0x0200094D RID: 2381
		public enum TwoHandedMode
		{
			// Token: 0x04004455 RID: 17493
			Position,
			// Token: 0x04004456 RID: 17494
			Dominant,
			// Token: 0x04004457 RID: 17495
			AutoFront,
			// Token: 0x04004458 RID: 17496
			AutoRear
		}

		// Token: 0x0200094E RID: 2382
		// (Invoke) Token: 0x06004314 RID: 17172
		public delegate void GrabEvent(RagdollHand ragdollHand, Handle handle, EventTime eventTime);

		// Token: 0x0200094F RID: 2383
		// (Invoke) Token: 0x06004318 RID: 17176
		public delegate void TkEvent(Handle handle, SpellTelekinesis spellTelekinesis);

		// Token: 0x02000950 RID: 2384
		[Serializable]
		public class JointModifier
		{
			// Token: 0x0600431B RID: 17179 RVA: 0x0018E7EE File Offset: 0x0018C9EE
			public JointModifier()
			{
			}

			// Token: 0x0600431C RID: 17180 RVA: 0x0018E7F6 File Offset: 0x0018C9F6
			public JointModifier(object handler, float positionSpringMultiplier, float positionDamperMultiplier, float rotationSpringMultiplier, float rotationDamperMultiplier)
			{
				this.handler = handler;
				this.positionSpringMultiplier = positionSpringMultiplier;
				this.positionDamperMultiplier = positionDamperMultiplier;
				this.rotationSpringMultiplier = rotationSpringMultiplier;
				this.rotationDamperMultiplier = rotationDamperMultiplier;
			}

			// Token: 0x04004459 RID: 17497
			[NonSerialized]
			public object handler;

			// Token: 0x0400445A RID: 17498
			public float positionSpringMultiplier;

			// Token: 0x0400445B RID: 17499
			public float positionDamperMultiplier;

			// Token: 0x0400445C RID: 17500
			public float rotationSpringMultiplier;

			// Token: 0x0400445D RID: 17501
			public float rotationDamperMultiplier;
		}

		// Token: 0x02000951 RID: 2385
		// (Invoke) Token: 0x0600431E RID: 17182
		public delegate void SlideEvent(RagdollHand ragdollHand, bool sliding, Handle handle, float position, EventTime eventTime);

		// Token: 0x02000952 RID: 2386
		// (Invoke) Token: 0x06004322 RID: 17186
		public delegate void SwitchHandleEvent(RagdollHand ragdollHand, Handle oldHandle, Handle newHandle, EventTime eventTime);

		// Token: 0x02000953 RID: 2387
		[Serializable]
		public class GripInfo
		{
			// Token: 0x06004325 RID: 17189 RVA: 0x0018E824 File Offset: 0x0018CA24
			public GripInfo(Transform parent)
			{
				this.gameObject = new GameObject("GripPoint");
				this.transform = this.gameObject.transform;
				this.transform.SetParent(parent);
				this.transform.localPosition = Vector3.zero;
				this.transform.localRotation = Quaternion.identity;
				this.transform.localScale = Vector3.one;
			}

			// Token: 0x1700056F RID: 1391
			// (get) Token: 0x06004326 RID: 17190 RVA: 0x0018E894 File Offset: 0x0018CA94
			public Transform SpellOrbTarget
			{
				get
				{
					HandlePose handlePose = this.orientation;
					if (((handlePose != null) ? handlePose.spellOrbTarget : null) != null)
					{
						return this.orientation.spellOrbTarget;
					}
					HandlePose handlePose2 = this.orientation;
					if (!(((handlePose2 != null) ? handlePose2.handle.spellOrbTarget : null) != null))
					{
						return null;
					}
					return this.orientation.handle.spellOrbTarget;
				}
			}

			// Token: 0x06004327 RID: 17191 RVA: 0x0018E8F8 File Offset: 0x0018CAF8
			public void Destroy()
			{
				if (this.joint)
				{
					UnityEngine.Object.Destroy(this.joint);
				}
				if (this.playerJoint)
				{
					UnityEngine.Object.Destroy(this.playerJoint);
				}
				this.hasPlayerJoint = false;
				if (this.hasEffect)
				{
					this.effectInstance.onEffectFinished += this.OnEffectFinishedEvent;
					this.effectInstance.End(false, -1f);
					this.hasEffect = false;
					return;
				}
				UnityEngine.Object.Destroy(this.gameObject);
			}

			// Token: 0x06004328 RID: 17192 RVA: 0x0018E97F File Offset: 0x0018CB7F
			private void OnEffectFinishedEvent(EffectInstance effectInstance)
			{
				if (this.effectInstance == effectInstance)
				{
					effectInstance.onEffectFinished -= this.OnEffectFinishedEvent;
					UnityEngine.Object.Destroy(this.gameObject);
				}
			}

			// Token: 0x0400445E RID: 17502
			public GameObject gameObject;

			// Token: 0x0400445F RID: 17503
			public Transform transform;

			// Token: 0x04004460 RID: 17504
			public Transform ikAnchor;

			// Token: 0x04004461 RID: 17505
			public Vector3 lastSlidePosition;

			// Token: 0x04004462 RID: 17506
			public RagdollHand ragdollHand;

			// Token: 0x04004463 RID: 17507
			public float axisPosition;

			// Token: 0x04004464 RID: 17508
			public HandlePose orientation;

			// Token: 0x04004465 RID: 17509
			public PhysicBody physicBody;

			// Token: 0x04004466 RID: 17510
			public ConfigurableJoint joint;

			// Token: 0x04004467 RID: 17511
			public ConfigurableJoint playerJoint;

			// Token: 0x04004468 RID: 17512
			public EffectInstance effectInstance;

			// Token: 0x04004469 RID: 17513
			public bool hasEffect;

			// Token: 0x0400446A RID: 17514
			public bool isSliding;

			// Token: 0x0400446B RID: 17515
			public bool hasPlayerJoint;

			// Token: 0x0400446C RID: 17516
			public Handle.GripInfo.Type type;

			// Token: 0x02000BEB RID: 3051
			public enum Type
			{
				// Token: 0x04004D49 RID: 19785
				None,
				// Token: 0x04004D4A RID: 19786
				PlayerJoint,
				// Token: 0x04004D4B RID: 19787
				HandJoint,
				// Token: 0x04004D4C RID: 19788
				HandSync,
				// Token: 0x04004D4D RID: 19789
				IKOnly
			}
		}
	}
}
