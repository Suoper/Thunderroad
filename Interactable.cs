using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000349 RID: 841
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Misc/Interactable.html")]
	public class Interactable : ThunderBehaviour
	{
		// Token: 0x06002747 RID: 10055 RVA: 0x0010F55C File Offset: 0x0010D75C
		protected virtual void OnDrawGizmosSelected()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			if (this.axisLength > 0f)
			{
				Gizmos.color = Color.white;
				Common.DrawGizmoArrow(Vector3.zero, Vector3.up * (this.axisLength / 2f), Common.HueColourValue(HueColorName.White), 0.05f, 20f, false);
				Common.DrawGizmoArrow(Vector3.zero, -Vector3.up * (this.axisLength / 2f), Common.HueColourValue(HueColorName.White), 0.05f, 20f, false);
				Common.DrawGizmoCapsule(base.transform.position, base.transform.up, this.axisLength + this.touchRadius, this.touchRadius, Common.HueColourValue(HueColorName.White));
				Common.DrawGizmoCapsule(base.transform.position, base.transform.up, this.axisLength + this.touchRadius, this.touchRadius, Common.HueColourValue(HueColorName.White));
				return;
			}
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = Color.white;
			Gizmos.DrawWireSphere(this.touchCenter, this.touchRadius);
		}

		// Token: 0x06002748 RID: 10056 RVA: 0x0010F6A6 File Offset: 0x0010D8A6
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			this.TouchEndPlayerHands(false);
		}

		// Token: 0x06002749 RID: 10057 RVA: 0x0010F6B5 File Offset: 0x0010D8B5
		public virtual void SetTouch(bool active)
		{
			this.touchCollider.enabled = (this.touchActive && active);
			this.TouchEndPlayerHands(active);
		}

		// Token: 0x0600274A RID: 10058 RVA: 0x0010F6D4 File Offset: 0x0010D8D4
		protected void TouchEndPlayerHands(bool active)
		{
			if (active)
			{
				return;
			}
			if (!Player.local)
			{
				return;
			}
			if (Player.local.handLeft.ragdollHand)
			{
				Player.local.handLeft.ragdollHand.InteractableStoppedTouching(this, true);
			}
			if (Player.local.handRight.ragdollHand)
			{
				Player.local.handRight.ragdollHand.InteractableStoppedTouching(this, true);
			}
		}

		// Token: 0x0600274B RID: 10059 RVA: 0x0010F74C File Offset: 0x0010D94C
		public virtual void SetTouchPersistent(bool active)
		{
			this.touchActive = active;
			this.touchCollider.enabled = active;
			this.TouchEndPlayerHands(active);
		}

		// Token: 0x1400012D RID: 301
		// (add) Token: 0x0600274C RID: 10060 RVA: 0x0010F768 File Offset: 0x0010D968
		// (remove) Token: 0x0600274D RID: 10061 RVA: 0x0010F7A0 File Offset: 0x0010D9A0
		public event Interactable.ActionDelegate OnTouchActionEvent;

		// Token: 0x0600274E RID: 10062 RVA: 0x0010F7D5 File Offset: 0x0010D9D5
		public List<ValueDropdownItem<string>> GetAllInteractableID()
		{
			return Catalog.GetDropdownAllID(Category.Interactable, "None");
		}

		// Token: 0x0600274F RID: 10063 RVA: 0x0010F7E4 File Offset: 0x0010D9E4
		protected virtual void Awake()
		{
			if (!GameManager.local)
			{
				base.enabled = false;
				return;
			}
			this.name = base.name;
			this.touchCollider = base.GetComponent<Collider>();
			if (!this.touchCollider)
			{
				if (this.axisLength > 0f)
				{
					this.touchCollider = base.gameObject.AddComponent<CapsuleCollider>();
					this.touchCollider.isTrigger = true;
				}
				else
				{
					this.touchCollider = base.gameObject.AddComponent<SphereCollider>();
					this.touchCollider.isTrigger = true;
				}
			}
			this.SetTouchRadius(this.touchRadius, true);
			base.gameObject.layer = GameManager.GetLayer(LayerName.Touch);
		}

		// Token: 0x06002750 RID: 10064 RVA: 0x0010F891 File Offset: 0x0010DA91
		protected virtual void Start()
		{
			this.TryLoadFromID();
		}

		// Token: 0x06002751 RID: 10065 RVA: 0x0010F899 File Offset: 0x0010DA99
		public virtual float GetNearestAxisPosition(Vector3 position)
		{
			return Mathf.Clamp(base.transform.InverseTransformPoint(position).y, -(this.axisLength / 2f), this.axisLength / 2f);
		}

		// Token: 0x06002752 RID: 10066 RVA: 0x0010F8CA File Offset: 0x0010DACA
		public virtual Vector3 GetNearestPositionAlongAxis(Vector3 position)
		{
			return base.transform.TransformPoint(new Vector3(0f, this.GetNearestAxisPosition(position), 0f));
		}

		// Token: 0x06002753 RID: 10067 RVA: 0x0010F8F0 File Offset: 0x0010DAF0
		public void SetTouchRadius(float radius, bool force = false)
		{
			if (!force && this.touchRadius.IsApproximately(radius))
			{
				return;
			}
			this.touchRadius = radius;
			CapsuleCollider capsuleCollider = this.touchCollider as CapsuleCollider;
			if (capsuleCollider != null)
			{
				capsuleCollider.radius = this.touchRadius;
				capsuleCollider.center = this.touchCenter;
				capsuleCollider.height = this.axisLength + this.touchRadius;
				capsuleCollider.direction = 1;
			}
			SphereCollider sphereCollider = this.touchCollider as SphereCollider;
			if (sphereCollider != null)
			{
				sphereCollider.radius = this.touchRadius;
				sphereCollider.center = this.touchCenter;
			}
		}

		// Token: 0x06002754 RID: 10068 RVA: 0x0010F980 File Offset: 0x0010DB80
		public virtual void TryLoadFromID()
		{
			if (this.data == null && !string.IsNullOrEmpty(this.interactableId) && this.interactableId != "None")
			{
				InteractableData interactableData;
				if (Catalog.TryGetData<InteractableData>(this.interactableId, out interactableData, true))
				{
					if (interactableData != null)
					{
						this.Load(interactableData.Clone() as InteractableData);
						return;
					}
				}
				else
				{
					Debug.LogError("Interactable ID not found: " + this.interactableId + " on interactable " + base.gameObject.GetPathFromRoot(), this);
				}
			}
		}

		// Token: 0x06002755 RID: 10069 RVA: 0x0010F9FF File Offset: 0x0010DBFF
		public virtual void Load(InteractableData interactableData)
		{
			this.interactableId = interactableData.id;
			this.data = interactableData;
			this.initialized = true;
		}

		// Token: 0x06002756 RID: 10070 RVA: 0x0010FA1C File Offset: 0x0010DC1C
		public virtual bool CanTouch(RagdollHand ragdollHand)
		{
			return (this.allowedHandSide != Interactable.HandSide.Left || ragdollHand.side != Side.Right) && (this.allowedHandSide != Interactable.HandSide.Right || ragdollHand.side != Side.Left) && this.data != null && !this.data.disableTouch;
		}

		// Token: 0x06002757 RID: 10071 RVA: 0x0010FA68 File Offset: 0x0010DC68
		public virtual void OnTouchStay(RagdollHand ragdollHand)
		{
		}

		// Token: 0x06002758 RID: 10072 RVA: 0x0010FA6A File Offset: 0x0010DC6A
		public virtual void OnTouchStart(RagdollHand ragdollHand)
		{
			if (Highlighter.isEnabled && Interactable.showTouchHighlighter)
			{
				this.ShowHint(ragdollHand);
			}
		}

		// Token: 0x06002759 RID: 10073 RVA: 0x0010FA81 File Offset: 0x0010DC81
		public virtual void OnTouchEnd(RagdollHand ragdollHand)
		{
			Highlighter.GetSide(ragdollHand.side).Hide();
		}

		// Token: 0x0600275A RID: 10074 RVA: 0x0010FA94 File Offset: 0x0010DC94
		public virtual Interactable.InteractionResult CheckInteraction(RagdollHand ragdollHand)
		{
			new List<string>();
			return new Interactable.InteractionResult(ragdollHand, false, true, LocalizationManager.Instance.GetLocalizedString("Default", "Error", false), "Cannot use base class", null, null, null);
		}

		// Token: 0x0600275B RID: 10075 RVA: 0x0010FAD4 File Offset: 0x0010DCD4
		public virtual void ShowHint(RagdollHand ragdollHand)
		{
			Interactable.InteractionResult interactionResult = this.CheckInteraction(ragdollHand);
			if (interactionResult.showHint)
			{
				Highlighter side = Highlighter.GetSide(ragdollHand.side);
				side.Show(this.highlighterTransform ? this.highlighterTransform : base.transform, interactionResult.hintTitle, interactionResult.hintDesignation, Highlighter.Style.Default, 1f, false, "");
				side.SetOutlineColor(interactionResult.hintColor);
			}
		}

		// Token: 0x0600275C RID: 10076 RVA: 0x0010FB40 File Offset: 0x0010DD40
		public virtual bool TryTouchAction(RagdollHand ragdollHand, Interactable.Action action)
		{
			Interactable.ActionDelegate onTouchActionEvent = this.OnTouchActionEvent;
			if (onTouchActionEvent != null)
			{
				onTouchActionEvent(ragdollHand, action);
			}
			return false;
		}

		// Token: 0x0600275D RID: 10077 RVA: 0x0010FB58 File Offset: 0x0010DD58
		public void HapticRumble(PlayerHand playerHand)
		{
			float centerDistance = Vector3.Distance(base.transform.position, playerHand.transform.position);
			centerDistance.ToString("F2").Equals(this.previousCenterDistance.ToString("F2"));
			this.previousCenterDistance = centerDistance;
		}

		// Token: 0x0400266C RID: 9836
		[Tooltip("(Only needed for non-json handles)\nInsert Interactable ID here.")]
		public string interactableId;

		// Token: 0x0400266D RID: 9837
		[Tooltip("What hand is allowed to grab the handle.")]
		public Interactable.HandSide allowedHandSide;

		// Token: 0x0400266E RID: 9838
		public Transform highlighterTransform;

		// Token: 0x0400266F RID: 9839
		[Tooltip("The length of which the player can grab along.\nIf >0, a button will appear and allow you to adjust the length along its points")]
		public float axisLength;

		// Token: 0x04002670 RID: 9840
		[Tooltip("The radius of which the player can grab the handle")]
		public float touchRadius = 0.1f;

		// Token: 0x04002671 RID: 9841
		[Tooltip("When the player's hand is within the range of multiple interactables, the closest one is prioritized. \n \nArtifical distance is a fake distance added to the player's hand while checking which interactable is the nearest.\nSetting this to a high value gives it a low priority when working out which interactable to use, while a low (or negative) value will give it a high priority compared to other interactables. \n \n Generally, you can leave this value at 0.")]
		public float artificialDistance;

		// Token: 0x04002672 RID: 9842
		[Tooltip("Determines the center of the touchRadius")]
		public Vector3 touchCenter;

		// Token: 0x04002673 RID: 9843
		public static bool showTouchHighlighter = true;

		// Token: 0x04002674 RID: 9844
		[NonSerialized]
		public InteractableData data;

		// Token: 0x04002675 RID: 9845
		[NonSerialized]
		public Collider touchCollider;

		// Token: 0x04002676 RID: 9846
		[NonSerialized]
		public bool touchActive = true;

		// Token: 0x04002677 RID: 9847
		private float previousCenterDistance = float.PositiveInfinity;

		// Token: 0x04002678 RID: 9848
		protected bool initialized;

		// Token: 0x0400267A RID: 9850
		public new string name;

		// Token: 0x02000A38 RID: 2616
		public enum HandSide
		{
			// Token: 0x0400476D RID: 18285
			Both,
			// Token: 0x0400476E RID: 18286
			Right,
			// Token: 0x0400476F RID: 18287
			Left
		}

		// Token: 0x02000A39 RID: 2617
		// (Invoke) Token: 0x0600459D RID: 17821
		public delegate void ActionDelegate(RagdollHand ragdollHand, Interactable.Action action);

		// Token: 0x02000A3A RID: 2618
		public enum Action
		{
			// Token: 0x04004771 RID: 18289
			UseStart,
			// Token: 0x04004772 RID: 18290
			UseStop,
			// Token: 0x04004773 RID: 18291
			AlternateUseStart,
			// Token: 0x04004774 RID: 18292
			AlternateUseStop,
			// Token: 0x04004775 RID: 18293
			Grab,
			// Token: 0x04004776 RID: 18294
			Ungrab
		}

		// Token: 0x02000A3B RID: 2619
		public class InteractionResult
		{
			// Token: 0x060045A0 RID: 17824 RVA: 0x0019637C File Offset: 0x0019457C
			public InteractionResult(RagdollHand ragdollHand, bool isInteractable, bool showHint = false, string hintTitle = null, string hintDesignation = null, Color? hintColor = null, AudioClip audioClip = null, string altHintDesignation = null)
			{
				this.ragdollHand = ragdollHand;
				this.isInteractable = isInteractable;
				this.showHint = showHint;
				this.hintTitle = (hintTitle ?? LocalizationManager.Instance.GetLocalizedString("Default", "Error", false));
				this.hintDesignation = hintDesignation;
				this.altHintDesignation = altHintDesignation;
				this.hintColor = (hintColor ?? Color.white);
				this.audioClip = audioClip;
			}

			// Token: 0x04004777 RID: 18295
			public RagdollHand ragdollHand;

			// Token: 0x04004778 RID: 18296
			public bool isInteractable;

			// Token: 0x04004779 RID: 18297
			public bool showHint;

			// Token: 0x0400477A RID: 18298
			public string hintTitle;

			// Token: 0x0400477B RID: 18299
			public string hintDesignation;

			// Token: 0x0400477C RID: 18300
			public string altHintDesignation;

			// Token: 0x0400477D RID: 18301
			public Color hintColor;

			// Token: 0x0400477E RID: 18302
			public AudioClip audioClip;
		}

		// Token: 0x02000A3C RID: 2620
		[Serializable]
		public class HighlightParams
		{
			// Token: 0x060045A1 RID: 17825 RVA: 0x001963FD File Offset: 0x001945FD
			public Interactable.HighlightParams Copy()
			{
				return (Interactable.HighlightParams)base.MemberwiseClone();
			}

			// Token: 0x0400477F RID: 18303
			public Color touchColor = Color.yellow;

			// Token: 0x04004780 RID: 18304
			public Color proximityColor = Color.white;

			// Token: 0x04004781 RID: 18305
			public float proximityDistance = 2f;
		}
	}
}
