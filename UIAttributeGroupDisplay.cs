using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000378 RID: 888
	public class UIAttributeGroupDisplay : MonoBehaviour
	{
		// Token: 0x06002A1D RID: 10781 RVA: 0x0011DDB4 File Offset: 0x0011BFB4
		private void Awake()
		{
			this.ClearIcons();
			this.backgroundImage.gameObject.SetActive(false);
			this.title.gameObject.SetActive(false);
			this.description.gameObject.SetActive(false);
			this.separator.SetActive(false);
			this.iconLayout.gameObject.SetActive(false);
		}

		// Token: 0x06002A1E RID: 10782 RVA: 0x0011DE17 File Offset: 0x0011C017
		private void OnDestroy()
		{
			this.ClearIcons();
			this.iconPool.Clear();
			this.iconPool = null;
		}

		// Token: 0x06002A1F RID: 10783 RVA: 0x0011DE31 File Offset: 0x0011C031
		private void OnEnable()
		{
			if (!this.iconSet)
			{
				base.StartCoroutine(this.SetIconCoroutine());
			}
		}

		// Token: 0x06002A20 RID: 10784 RVA: 0x0011DE48 File Offset: 0x0011C048
		public UIAttributeGroupDisplay WithBackground(Sprite backgroundImage, Color color, float aspectRatio = 1f)
		{
			this.backgroundImage.sprite = backgroundImage;
			this.backgroundImage.color = color;
			AspectRatioFitter aspectRatioFitter;
			if (aspectRatio != 1f && this.backgroundImage.TryGetComponent<AspectRatioFitter>(out aspectRatioFitter))
			{
				aspectRatioFitter.aspectRatio = aspectRatio;
			}
			this.backgroundImage.gameObject.SetActive(true);
			return this;
		}

		/// <summary>
		/// Set the attribute display title.
		/// </summary>
		// Token: 0x06002A21 RID: 10785 RVA: 0x0011DE9D File Offset: 0x0011C09D
		public UIAttributeGroupDisplay WithTitle(string textId, string groupId = "Default")
		{
			this.title.textGroupId = groupId;
			this.title.text = "{" + textId + "}";
			this.title.gameObject.SetActive(true);
			return this;
		}

		/// <summary>
		/// Set the attribute description.
		/// </summary>
		// Token: 0x06002A22 RID: 10786 RVA: 0x0011DED8 File Offset: 0x0011C0D8
		public UIAttributeGroupDisplay WithDescription(string textId, string groupId = "Default")
		{
			this.description.textGroupId = groupId;
			this.description.text = "{" + textId + "}";
			this.description.gameObject.SetActive(true);
			return this;
		}

		// Token: 0x06002A23 RID: 10787 RVA: 0x0011DF14 File Offset: 0x0011C114
		public UIAttributeGroupDisplay WithIcons(Sprite iconTexture, Color iconColor, int count = 1, float iconAspectRatio = 1f)
		{
			if (iconTexture == null)
			{
				return this;
			}
			List<ValueTuple<Sprite, Color>> tempList = new List<ValueTuple<Sprite, Color>>();
			for (int i = 0; i < count; i++)
			{
				tempList.Add(new ValueTuple<Sprite, Color>(iconTexture, iconColor));
			}
			return this.WithIcons(tempList, iconAspectRatio);
		}

		// Token: 0x06002A24 RID: 10788 RVA: 0x0011DF54 File Offset: 0x0011C154
		public UIAttributeGroupDisplay WithIcons([TupleElementNames(new string[]
		{
			"iconCache",
			"iconColor"
		})] List<ValueTuple<Sprite, Color>> icons, float iconAspectRatio = 1f)
		{
			if (icons == null)
			{
				return this;
			}
			this.iconLayout.gameObject.SetActive(true);
			this.icons = icons;
			this.iconAspectRatio = iconAspectRatio;
			this.bgIcons = null;
			this.iconProgession = this.icons.Count;
			this.SetIconCount();
			return this;
		}

		// Token: 0x06002A25 RID: 10789 RVA: 0x0011DFA4 File Offset: 0x0011C1A4
		public UIAttributeGroupDisplay WithProgressionIcons([TupleElementNames(new string[]
		{
			"iconCache",
			"iconColor"
		})] List<ValueTuple<Sprite, Color>> icons, [TupleElementNames(new string[]
		{
			"iconCache",
			"iconColor"
		})] List<ValueTuple<Sprite, Color>> bgIcons, int iconProgession, float iconAspectRatio = 1f)
		{
			if (icons == null || bgIcons == null)
			{
				return this;
			}
			this.iconLayout.gameObject.SetActive(true);
			this.icons = icons;
			this.iconAspectRatio = iconAspectRatio;
			this.bgIcons = bgIcons;
			this.iconProgession = iconProgession;
			this.SetIconCount();
			return this;
		}

		// Token: 0x06002A26 RID: 10790 RVA: 0x0011DFE4 File Offset: 0x0011C1E4
		public UIAttributeGroupDisplay WithProgressionIcons(Sprite icon, Color color, Sprite bgIcon, Color bgColor, int count = 1, int progression = 1, float iconAspectRatio = 1f)
		{
			if (icon == null || bgIcon == null)
			{
				return this;
			}
			List<ValueTuple<Sprite, Color>> tempListIcons = new List<ValueTuple<Sprite, Color>>();
			List<ValueTuple<Sprite, Color>> tempListBgIcons = new List<ValueTuple<Sprite, Color>>();
			for (int i = 0; i < count; i++)
			{
				tempListIcons.Add(new ValueTuple<Sprite, Color>(icon, color));
				tempListBgIcons.Add(new ValueTuple<Sprite, Color>(bgIcon, bgColor));
			}
			return this.WithProgressionIcons(tempListIcons, tempListBgIcons, progression, iconAspectRatio);
		}

		/// <summary>
		/// Set active the separator.
		/// </summary>
		// Token: 0x06002A27 RID: 10791 RVA: 0x0011E045 File Offset: 0x0011C245
		public UIAttributeGroupDisplay WithSeparator()
		{
			this.separator.SetActive(true);
			return this;
		}

		/// <summary>
		/// Set the amount of icons to display.
		/// </summary>
		// Token: 0x06002A28 RID: 10792 RVA: 0x0011E054 File Offset: 0x0011C254
		private void SetIconCount()
		{
			this.iconSet = false;
			if (base.gameObject.activeInHierarchy)
			{
				base.StartCoroutine(this.SetIconCoroutine());
			}
		}

		// Token: 0x06002A29 RID: 10793 RVA: 0x0011E077 File Offset: 0x0011C277
		private IEnumerator SetIconCoroutine()
		{
			yield return null;
			RectTransform layoutTransform = this.iconLayout.transform as RectTransform;
			float iconHeight = layoutTransform.sizeDelta.y - (float)(this.iconLayout.padding.top + this.iconLayout.padding.bottom);
			int totalIconCount = this.bgIcons.IsNullOrEmpty() ? this.icons.Count : this.bgIcons.Count;
			float iconWidth = (layoutTransform.sizeDelta.x - ((float)(this.iconLayout.padding.left + this.iconLayout.padding.right) + this.iconLayout.spacing * (float)(totalIconCount - 1))) / (float)totalIconCount;
			this.ClearIcons();
			if (this.iconPool == null || this.iconPool.Count == 0)
			{
				Debug.LogError("No Icon model present in the icon pool");
				this.iconSet = true;
				yield break;
			}
			float scale = 1f;
			AspectRatioFitter.AspectMode aspectMode;
			if (iconHeight > iconWidth)
			{
				aspectMode = AspectRatioFitter.AspectMode.WidthControlsHeight;
				this.iconLayout.childControlHeight = false;
				this.iconLayout.childControlWidth = true;
				this.iconLayout.childForceExpandHeight = false;
				this.iconLayout.childForceExpandWidth = true;
				float height = iconWidth / this.iconAspectRatio;
				if (height > iconHeight)
				{
					scale = iconHeight / height;
				}
			}
			else
			{
				aspectMode = AspectRatioFitter.AspectMode.HeightControlsWidth;
				this.iconLayout.childControlHeight = true;
				this.iconLayout.childControlWidth = false;
				this.iconLayout.childForceExpandHeight = true;
				this.iconLayout.childForceExpandWidth = false;
				float width = iconHeight * this.iconAspectRatio;
				if (width > iconWidth)
				{
					scale = iconWidth / width;
				}
			}
			int poolCount = this.iconPool.Count;
			for (int index = 0; index < this.iconProgession; index++)
			{
				Image tempImage = (index < poolCount) ? this.iconPool[index] : this.CreateIcon();
				tempImage.sprite = this.icons[index].Item1;
				tempImage.color = this.icons[index].Item2;
				AspectRatioFitter ratioFitter;
				if (tempImage.TryGetComponent<AspectRatioFitter>(out ratioFitter))
				{
					ratioFitter.aspectRatio = this.iconAspectRatio;
					ratioFitter.aspectMode = aspectMode;
					tempImage.transform.localScale = Vector3.one * scale;
				}
				tempImage.gameObject.SetActive(true);
			}
			if (this.iconProgession < totalIconCount)
			{
				float scaleBgProgression = 1f;
				if (iconHeight > iconWidth)
				{
					float height2 = iconWidth / this.iconAspectRatio;
					if (height2 > iconHeight)
					{
						scaleBgProgression = iconHeight / height2;
					}
				}
				else
				{
					float width2 = iconHeight * this.iconAspectRatio;
					if (width2 > iconWidth)
					{
						scaleBgProgression = iconWidth / width2;
					}
				}
				for (int index2 = this.iconProgession; index2 < totalIconCount; index2++)
				{
					Image tempImage2 = (index2 < poolCount) ? this.iconPool[index2] : this.CreateIcon();
					tempImage2.sprite = this.bgIcons[index2].Item1;
					tempImage2.color = this.bgIcons[index2].Item2;
					AspectRatioFitter ratioFitter2;
					if (tempImage2.TryGetComponent<AspectRatioFitter>(out ratioFitter2))
					{
						ratioFitter2.aspectRatio = this.iconAspectRatio;
						ratioFitter2.aspectMode = aspectMode;
						tempImage2.transform.localScale = Vector3.one * scaleBgProgression;
					}
					tempImage2.gameObject.SetActive(true);
				}
			}
			LayoutRebuilder.ForceRebuildLayoutImmediate(layoutTransform);
			this.iconSet = true;
			yield break;
		}

		/// <summary>
		/// Clears all currently listed icons.
		/// </summary>
		// Token: 0x06002A2A RID: 10794 RVA: 0x0011E088 File Offset: 0x0011C288
		private void ClearIcons()
		{
			if (this.iconPool == null)
			{
				return;
			}
			int poolCount = this.iconPool.Count;
			for (int i = 0; i < poolCount; i++)
			{
				this.iconPool[i].sprite = null;
				this.iconPool[i].gameObject.SetActive(false);
			}
		}

		/// <summary>
		/// Create an icon from the cache address.
		/// </summary>
		// Token: 0x06002A2B RID: 10795 RVA: 0x0011E0E0 File Offset: 0x0011C2E0
		private Image CreateIcon()
		{
			Image newIcon = UnityEngine.Object.Instantiate<Image>(this.iconPool[0], this.iconLayout.transform);
			this.iconPool.Add(newIcon);
			return newIcon;
		}

		// Token: 0x040027E7 RID: 10215
		[SerializeField]
		private Image backgroundImage;

		// Token: 0x040027E8 RID: 10216
		[SerializeField]
		private UIText title;

		// Token: 0x040027E9 RID: 10217
		[SerializeField]
		private UIText description;

		// Token: 0x040027EA RID: 10218
		[SerializeField]
		private HorizontalOrVerticalLayoutGroup iconLayout;

		// Token: 0x040027EB RID: 10219
		[SerializeField]
		private List<Image> iconPool;

		// Token: 0x040027EC RID: 10220
		[SerializeField]
		private GameObject separator;

		// Token: 0x040027ED RID: 10221
		[TupleElementNames(new string[]
		{
			"iconCache",
			"iconColor"
		})]
		private List<ValueTuple<Sprite, Color>> icons;

		// Token: 0x040027EE RID: 10222
		public float iconAspectRatio;

		// Token: 0x040027EF RID: 10223
		[TupleElementNames(new string[]
		{
			"iconCache",
			"iconColor"
		})]
		private List<ValueTuple<Sprite, Color>> bgIcons;

		// Token: 0x040027F0 RID: 10224
		private int iconProgession;

		// Token: 0x040027F1 RID: 10225
		private bool iconSet = true;
	}
}
