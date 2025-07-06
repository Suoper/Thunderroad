using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000386 RID: 902
	public class UIMenuNavBarToggle : MonoBehaviour
	{
		// Token: 0x06002B03 RID: 11011 RVA: 0x00122BD0 File Offset: 0x00120DD0
		private void Init()
		{
			this.frames = new Dictionary<UIPlayerMenu.Style, Sprite>
			{
				{
					UIPlayerMenu.Style.Medieval,
					this.medievalFrame
				},
				{
					UIPlayerMenu.Style.Modern,
					this.modernFrame
				}
			};
			this.framesSelected = new Dictionary<UIPlayerMenu.Style, Sprite>
			{
				{
					UIPlayerMenu.Style.Medieval,
					this.medievalFrameSelected
				},
				{
					UIPlayerMenu.Style.Modern,
					this.modernFrameSelected
				}
			};
			this.icons = new Dictionary<UIPlayerMenu.Style, Sprite>
			{
				{
					UIPlayerMenu.Style.Medieval,
					this.medievalIcon
				},
				{
					UIPlayerMenu.Style.Modern,
					this.modernIcon
				}
			};
			this.iconsSelected = new Dictionary<UIPlayerMenu.Style, Sprite>
			{
				{
					UIPlayerMenu.Style.Medieval,
					this.medievalIconSelected
				},
				{
					UIPlayerMenu.Style.Modern,
					this.modernIconSelected
				}
			};
		}

		// Token: 0x06002B04 RID: 11012 RVA: 0x00122C74 File Offset: 0x00120E74
		public void Setup(UIMenu menu, MenuData menuData)
		{
			this.Init();
			this.menu = menu;
			base.name = menuData.id;
			this.title.SetLocalizationIds("Default", "{" + menuData.nameId + "}");
			Catalog.LoadAssetAsync<Sprite>(menuData.iconAddress, delegate(Sprite value)
			{
				if (value == null)
				{
					Debug.LogError("Failed to load icon for menu: " + menuData.id);
					return;
				}
				Dictionary<UIPlayerMenu.Style, Sprite> dictionary = this.icons;
				UIPlayerMenu.Style key = UIPlayerMenu.Style.Medieval;
				this.medievalIcon = value;
				dictionary[key] = value;
				Dictionary<UIPlayerMenu.Style, Sprite> dictionary2 = this.iconsSelected;
				UIPlayerMenu.Style key2 = UIPlayerMenu.Style.Medieval;
				this.medievalIconSelected = value;
				dictionary2[key2] = value;
				Dictionary<UIPlayerMenu.Style, Sprite> dictionary3 = this.icons;
				UIPlayerMenu.Style key3 = UIPlayerMenu.Style.Modern;
				this.modernIcon = value;
				dictionary3[key3] = value;
				UIPlayerMenu.Style style2 = UIPlayerMenu.instance.currentStyle;
				menu.icon.sprite = this.icons[style2];
				this.icon.sprite = this.icons[style2];
				this.iconSelected.sprite = this.iconsSelected[style2];
				UIPlayerMenu.instance.AddAddressableTexture(value);
			}, "UIPlayerMenu");
			Catalog.LoadAssetAsync<Sprite>(menuData.iconRollhoverAddress, delegate(Sprite value)
			{
				if (value == null)
				{
					Debug.LogError("Failed to load rollover icon for menu: " + menuData.id);
					return;
				}
				Dictionary<UIPlayerMenu.Style, Sprite> dictionary = this.iconsSelected;
				UIPlayerMenu.Style key = UIPlayerMenu.Style.Modern;
				this.modernIconSelected = value;
				dictionary[key] = value;
				UIPlayerMenu.Style style2 = UIPlayerMenu.instance.currentStyle;
				this.icon.sprite = this.icons[style2];
				this.iconSelected.sprite = this.iconsSelected[style2];
				UIPlayerMenu.instance.AddAddressableTexture(value);
			}, "UIPlayerMenu");
			this.button.toggle.isOn = false;
			this.button.toggle.onValueChanged.AddListener(delegate(bool <p0>)
			{
				UIPlayerMenu.instance.OnNavButtonValueChanged(this.button.toggle, menuData);
				menu.Toggled(this.button.toggle.isOn);
			});
			UIPlayerMenu.Style style = UIPlayerMenu.instance.currentStyle;
			this.SetupStyle(style, UIPlayerMenu.instance.defaultColors[style], UIPlayerMenu.instance.highlightColors[style]);
		}

		// Token: 0x06002B05 RID: 11013 RVA: 0x00122D90 File Offset: 0x00120F90
		public void SetupStyle(UIPlayerMenu.Style style, Color defaultColor, Color highlightColor)
		{
			this.title.textComponent.rectTransform.localPosition = new Vector3(this.title.textComponent.rectTransform.localPosition.x, (float)this.titleYPositions[style], this.title.textComponent.rectTransform.localPosition.z);
			if (this.title.textComponent.font == UIPlayerMenu.instance.outlineFontAsset)
			{
				this.title.textComponent.fontMaterial = UIPlayerMenu.instance.outlineFontMaterials[style];
			}
			this.frame.rectTransform.sizeDelta = new Vector2((float)this.frameSizes[style], (float)this.frameSizes[style]);
			this.frame.sprite = this.frames[style];
			this.frameSelected.sprite = this.framesSelected[style];
			this.menu.icon.sprite = this.icons[style];
			this.icon.sprite = this.icons[style];
			this.iconSelected.sprite = this.iconsSelected[style];
			this.warning.sprite = UIPlayerMenu.instance.warningIcons[style];
			this.warning.color = UIPlayerMenu.instance.warningColors[style];
			this.button.defaultColor = defaultColor;
			this.button.hoverColor = highlightColor;
			this.button.pressedColor = highlightColor;
			for (int i = 0; i < this.button.buttonGraphics.Length; i++)
			{
				this.button.buttonGraphics[i].color = defaultColor;
			}
			for (int j = 0; j < this.button.buttonGraphicsSwap.Length; j++)
			{
				this.button.buttonGraphicsSwap[j].color = highlightColor;
			}
			this.button.SetButtonState(this.button.toggle.isOn);
		}

		// Token: 0x040028B4 RID: 10420
		private readonly Dictionary<UIPlayerMenu.Style, int> titleYPositions = new Dictionary<UIPlayerMenu.Style, int>
		{
			{
				UIPlayerMenu.Style.Medieval,
				-150
			},
			{
				UIPlayerMenu.Style.Modern,
				-140
			}
		};

		// Token: 0x040028B5 RID: 10421
		private readonly Dictionary<UIPlayerMenu.Style, int> frameSizes = new Dictionary<UIPlayerMenu.Style, int>
		{
			{
				UIPlayerMenu.Style.Medieval,
				230
			},
			{
				UIPlayerMenu.Style.Modern,
				260
			}
		};

		// Token: 0x040028B6 RID: 10422
		public UICustomisableButton button;

		// Token: 0x040028B7 RID: 10423
		[SerializeField]
		private UIText title;

		// Token: 0x040028B8 RID: 10424
		[SerializeField]
		private Image frame;

		// Token: 0x040028B9 RID: 10425
		[SerializeField]
		private Image frameSelected;

		// Token: 0x040028BA RID: 10426
		[SerializeField]
		private Sprite medievalFrame;

		// Token: 0x040028BB RID: 10427
		[SerializeField]
		private Sprite medievalFrameSelected;

		// Token: 0x040028BC RID: 10428
		[SerializeField]
		private Sprite modernFrame;

		// Token: 0x040028BD RID: 10429
		[SerializeField]
		private Sprite modernFrameSelected;

		// Token: 0x040028BE RID: 10430
		[SerializeField]
		private Image icon;

		// Token: 0x040028BF RID: 10431
		[SerializeField]
		private Image iconSelected;

		// Token: 0x040028C0 RID: 10432
		[SerializeField]
		private Image warning;

		// Token: 0x040028C1 RID: 10433
		private UIMenu menu;

		// Token: 0x040028C2 RID: 10434
		private Sprite medievalIcon;

		// Token: 0x040028C3 RID: 10435
		private Sprite medievalIconSelected;

		// Token: 0x040028C4 RID: 10436
		private Sprite modernIcon;

		// Token: 0x040028C5 RID: 10437
		private Sprite modernIconSelected;

		// Token: 0x040028C6 RID: 10438
		private Dictionary<UIPlayerMenu.Style, Sprite> frames;

		// Token: 0x040028C7 RID: 10439
		private Dictionary<UIPlayerMenu.Style, Sprite> framesSelected;

		// Token: 0x040028C8 RID: 10440
		private Dictionary<UIPlayerMenu.Style, Sprite> icons;

		// Token: 0x040028C9 RID: 10441
		private Dictionary<UIPlayerMenu.Style, Sprite> iconsSelected;
	}
}
