using System;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000387 RID: 903
	public class UIScrollController : ScrollRect
	{
		// Token: 0x06002B07 RID: 11015 RVA: 0x00123008 File Offset: 0x00121208
		protected new void Awake()
		{
			base.Awake();
			this.contentLayoutGroup = base.content.GetComponent<LayoutGroup>();
			if (this.contentLayoutGroup != null)
			{
				RectOffset defaultPadding = this.contentLayoutGroup.padding;
				this.defaultGridOffset = new RectOffset(defaultPadding.left, defaultPadding.right, defaultPadding.top, defaultPadding.bottom);
			}
		}

		// Token: 0x06002B08 RID: 11016 RVA: 0x00123069 File Offset: 0x00121269
		protected new void Start()
		{
			base.Start();
			if (base.verticalScrollbar != null)
			{
				base.onValueChanged.AddListener(delegate(Vector2 <p0>)
				{
					this.SetVerticalScrollBarSize(this.verticalScrollSize);
				});
			}
			this.UpdateResetPosition();
		}

		// Token: 0x06002B09 RID: 11017 RVA: 0x0012309C File Offset: 0x0012129C
		protected new void OnEnable()
		{
			base.OnEnable();
			this.previousGridPosition = base.content.localPosition.y;
		}

		// Token: 0x06002B0A RID: 11018 RVA: 0x001230BA File Offset: 0x001212BA
		protected new void LateUpdate()
		{
			base.LateUpdate();
			if (this.adjustContentPosition && this.contentLayoutGroup != null)
			{
				this.AdjustContentPosition();
			}
		}

		/// <summary>
		/// Unity UI default behavior changes the scrollbar size while we are scrolling, which messes up the scroll handle
		/// position, so we need to set the scrollbar size, each time the scroll value changes, to keep its size constant.
		/// </summary>
		/// <param name="size">Size of the vertical scroll</param>
		// Token: 0x06002B0B RID: 11019 RVA: 0x001230DE File Offset: 0x001212DE
		private void SetVerticalScrollBarSize(float size)
		{
			if (base.verticalScrollbar == null)
			{
				Debug.LogError("No vertical scroll bar assigned to the scroll: " + base.name);
				return;
			}
			base.verticalScrollbar.size = size;
		}

		/// <summary>
		/// Adjust the scroll content position, according to the scrollbars visibility,
		/// if the visibility setting is set to Auto-hide
		/// </summary>
		// Token: 0x06002B0C RID: 11020 RVA: 0x00123110 File Offset: 0x00121310
		private void AdjustContentPosition()
		{
			bool updatedPosition = false;
			if (base.verticalScrollbar != null)
			{
				if (base.verticalScrollbar.isActiveAndEnabled)
				{
					if (this.contentLayoutGroup.padding.left != this.defaultGridOffset.left)
					{
						updatedPosition = true;
						this.contentLayoutGroup.padding.left = this.defaultGridOffset.left;
						this.contentLayoutGroup.padding.right = this.defaultGridOffset.right;
					}
				}
				else if (this.contentLayoutGroup.padding.left != this.noScrollbarsPadding.left)
				{
					updatedPosition = true;
					this.contentLayoutGroup.padding.left = this.noScrollbarsPadding.left;
					this.contentLayoutGroup.padding.right = this.noScrollbarsPadding.right;
				}
			}
			if (base.horizontalScrollbar != null)
			{
				if (base.horizontalScrollbar.isActiveAndEnabled)
				{
					if (this.contentLayoutGroup.padding.top != this.defaultGridOffset.top)
					{
						updatedPosition = true;
						this.contentLayoutGroup.padding.top = this.defaultGridOffset.top;
						this.contentLayoutGroup.padding.bottom = this.defaultGridOffset.bottom;
					}
				}
				else if (this.contentLayoutGroup.padding.top != this.noScrollbarsPadding.top)
				{
					updatedPosition = true;
					this.contentLayoutGroup.padding.top = this.noScrollbarsPadding.top;
					this.contentLayoutGroup.padding.bottom = this.noScrollbarsPadding.bottom;
				}
			}
			if (updatedPosition)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(base.content);
			}
		}

		/// <summary>
		/// ScrollRect.LateUpdate calls SetContentAnchoredPosition with very tiny values every frame,
		/// only if scrolling is not needed and even when velocity is zero.
		/// SetContentAnchoredPosition makes text jitter. Check before setting position.
		/// </summary>
		// Token: 0x06002B0D RID: 11021 RVA: 0x001232C8 File Offset: 0x001214C8
		protected override void SetContentAnchoredPosition(Vector2 position)
		{
			if (Application.isPlaying && base.verticalScrollbar != null && !base.verticalScrollbar.IsActive())
			{
				if (base.content.anchoredPosition == Vector2.zero)
				{
					return;
				}
				position = Vector2.zero;
			}
			if (position != Vector2.zero && UIScrollController.Approximately(base.content.anchoredPosition, position, 0.01f) && UIScrollController.Approximately(position, Vector2.zero, 0.01f))
			{
				position = Vector2.zero;
			}
			base.SetContentAnchoredPosition(position);
		}

		/// <summary>
		/// Called when scrolling would occur.
		/// Prevent setting when vertical scrollbar is disabled and scrolling is not needed to prevent jittering.
		/// </summary>
		// Token: 0x06002B0E RID: 11022 RVA: 0x0012335B File Offset: 0x0012155B
		protected override void SetNormalizedPosition(float value, int axis)
		{
			if (Application.isPlaying && base.verticalScrollbar != null && !base.verticalScrollbar.IsActive())
			{
				return;
			}
			base.SetNormalizedPosition(value, axis);
			this.LayoutGroupValueChanged();
		}

		// Token: 0x06002B0F RID: 11023 RVA: 0x00123390 File Offset: 0x00121590
		private static bool Approximately(Vector2 vec1, Vector2 vec2, float threshold = 0.01f)
		{
			return ((vec1.x < vec2.x) ? (vec2.x - vec1.x) : (vec1.x - vec2.x)) <= threshold && ((vec1.y < vec2.y) ? (vec2.y - vec1.y) : (vec1.y - vec2.y)) <= threshold;
		}

		/// <summary>
		/// Control scroll max speed. The scroll rect default behavior is to increase the scroll speed proportionally
		/// to the amount of content present in the scroll. This means that if the scroll has a lot of content, it is
		/// almost impossible to scroll small portions at a time.
		/// </summary>
		// Token: 0x06002B10 RID: 11024 RVA: 0x001233FC File Offset: 0x001215FC
		public void LayoutGroupValueChanged()
		{
			if (this.controlVerticalSpeed)
			{
				float displacementAttempt = this.previousGridPosition - base.content.localPosition.y;
				float absoluteDisplacement = Math.Abs(displacementAttempt);
				if (absoluteDisplacement > 0.05f && absoluteDisplacement > this.maxVerticalDisplacement)
				{
					float displacement = (displacementAttempt < 0f) ? (-this.maxVerticalDisplacement) : this.maxVerticalDisplacement;
					base.content.localPosition = new Vector2(this.resetGridPosition.x, this.previousGridPosition - displacement);
				}
				this.previousGridPosition = base.content.transform.localPosition.y;
			}
			base.content.localPosition = new Vector2(this.resetGridPosition.x, base.content.transform.localPosition.y);
		}

		// Token: 0x06002B11 RID: 11025 RVA: 0x001234D4 File Offset: 0x001216D4
		public void ResetPosition()
		{
			base.content.localPosition = this.resetGridPosition;
			this.previousGridPosition = base.content.localPosition.y;
		}

		// Token: 0x06002B12 RID: 11026 RVA: 0x00123502 File Offset: 0x00121702
		public void UpdateResetPosition()
		{
			this.resetGridPosition = new Vector2(base.content.localPosition.x, base.content.localPosition.y);
		}

		// Token: 0x040028CA RID: 10442
		public float verticalScrollSize;

		// Token: 0x040028CB RID: 10443
		[Tooltip("Adjust the position of the content if the auto-hide option is enabled for the vertical and/or the horizontal scrollbars.")]
		public bool adjustContentPosition;

		// Token: 0x040028CC RID: 10444
		[Tooltip("Set the left/right values for the vertical scrollbar and the top/bottom values for the horizontal scrollbar.")]
		public RectOffset noScrollbarsPadding;

		// Token: 0x040028CD RID: 10445
		public bool controlVerticalSpeed;

		// Token: 0x040028CE RID: 10446
		[Tooltip("Max vertical scroll speed: Maximum vertical pixels that can be displaced in one frame.")]
		public float maxVerticalDisplacement = 30f;

		// Token: 0x040028CF RID: 10447
		private LayoutGroup contentLayoutGroup;

		// Token: 0x040028D0 RID: 10448
		private float previousGridPosition;

		// Token: 0x040028D1 RID: 10449
		private Vector2 resetGridPosition;

		// Token: 0x040028D2 RID: 10450
		private RectOffset defaultGridOffset;
	}
}
