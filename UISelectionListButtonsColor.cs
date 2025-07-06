using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200038A RID: 906
	public class UISelectionListButtonsColor : UISelectionListButtons
	{
		// Token: 0x06002B2C RID: 11052 RVA: 0x00123A50 File Offset: 0x00121C50
		protected override void Awake()
		{
			base.Awake();
			base.name = this.color.ToString() + "Color";
			this.minValue = 0;
			if (this.value)
			{
				this.value.text = "";
			}
		}

		// Token: 0x06002B2D RID: 11053 RVA: 0x00123AA8 File Offset: 0x00121CA8
		private void OnEnable()
		{
			if (this.hairToggle != null)
			{
				base.StartCoroutine(this.<OnEnable>g__ResetUISelection|23_0());
			}
		}

		// Token: 0x06002B2E RID: 11054 RVA: 0x00123AC8 File Offset: 0x00121CC8
		public override void LoadValue()
		{
			if (!this.characterSelection)
			{
				return;
			}
			if (!this.characterSelection.initialized)
			{
				return;
			}
			Creature creature = this.characterSelection.GetCharacterCreature();
			CreatureData.EthnicGroup ethnicGroup = this.characterSelection.GetCharacterCreatureEthnicGroup();
			switch (this.color)
			{
			case UISelectionListButtonsColor.PartColor.Hair:
			{
				UISelectionListButtonsColor.PartColorType partColorType = this.colorType;
				if (partColorType != UISelectionListButtonsColor.PartColorType.Primary)
				{
					if (partColorType == UISelectionListButtonsColor.PartColorType.Secondary)
					{
						this.availableColors = (ethnicGroup.hairColorsSpecular ?? new List<Color>());
						if (this.useSharedColors)
						{
							this.availableColors = this.availableColors.Concat(creature.data.hairColorsSpecularShared).Distinct<Color>().ToList<Color>();
						}
						this.currentValue = this.ClosestColor(this.availableColors, creature.GetColor(Creature.ColorModifier.HairSpecular));
					}
				}
				else
				{
					this.availableColors = (ethnicGroup.hairColorsPrimary ?? new List<Color>());
					if (this.useSharedColors)
					{
						this.availableColors = this.availableColors.Concat(creature.data.hairColorsPrimaryShared).Distinct<Color>().ToList<Color>();
					}
					this.currentValue = this.ClosestColor(this.availableColors, creature.GetColor(Creature.ColorModifier.Hair));
				}
				this.maxValue = this.availableColors.Count - 1;
				break;
			}
			case UISelectionListButtonsColor.PartColor.Eyes:
			{
				UISelectionListButtonsColor.PartColorType partColorType = this.colorType;
				if (partColorType != UISelectionListButtonsColor.PartColorType.Primary)
				{
					if (partColorType == UISelectionListButtonsColor.PartColorType.Secondary)
					{
						this.availableColors = (ethnicGroup.eyesColorsSclera ?? new List<Color>());
						if (this.useSharedColors)
						{
							this.availableColors = this.availableColors.Concat(creature.data.eyesColorsScleraShared).Distinct<Color>().ToList<Color>();
						}
						this.currentValue = this.ClosestColor(this.availableColors, creature.GetColor(Creature.ColorModifier.EyesSclera));
					}
				}
				else
				{
					this.availableColors = (ethnicGroup.eyesColorsIris ?? new List<Color>());
					if (this.useSharedColors)
					{
						this.availableColors = this.availableColors.Concat(creature.data.eyesColorsIrisShared).Distinct<Color>().ToList<Color>();
					}
					this.currentValue = this.ClosestColor(this.availableColors, creature.GetColor(Creature.ColorModifier.EyesIris));
				}
				this.maxValue = this.availableColors.Count - 1;
				break;
			}
			case UISelectionListButtonsColor.PartColor.Skin:
				this.availableColors = (ethnicGroup.skinColors ?? new List<Color>());
				if (this.useSharedColors)
				{
					this.availableColors = this.availableColors.Concat(creature.data.skinColorsShared).Distinct<Color>().ToList<Color>();
				}
				this.currentValue = this.ClosestColor(this.availableColors, creature.GetColor(Creature.ColorModifier.Skin));
				this.maxValue = this.availableColors.Count - 1;
				break;
			}
			if (this.currentValue < this.minValue || this.currentValue > this.maxValue)
			{
				this.currentValue = 0;
			}
			for (int i = 0; i < this.colorSelectorButtons.Count; i++)
			{
				UnityEngine.Object.Destroy(this.colorSelectorButtons[i]);
			}
			for (int j = 0; j < this.colorSelectorDeactivatedFillers.Count; j++)
			{
				UnityEngine.Object.Destroy(this.colorSelectorDeactivatedFillers[j]);
			}
			this.colorSelectorButtons.Clear();
			this.colorSelectorDeactivatedFillers.Clear();
			if (this.availableColors != null)
			{
				for (int k = 0; k < this.availableColors.Count; k++)
				{
					GameObject colorSelector = UnityEngine.Object.Instantiate<GameObject>(this.colorSelectorButtonPrefab, this.colorSelectorButtonHolder);
					int currentIndex = k;
					colorSelector.GetComponent<Toggle>().onValueChanged.AddListener(delegate(bool toggled)
					{
						if (toggled)
						{
							this.SetCurrentValue(currentIndex);
							return;
						}
						if (currentIndex == this.currentValue)
						{
							colorSelector.GetComponent<Toggle>().isOn = true;
						}
					});
					colorSelector.GetComponent<Graphic>().color = this.availableColors[k];
					this.colorSelectorButtons.Add(colorSelector);
				}
			}
			for (int l = 0; l < this.colorSelectorDeactivatedFillerNumber; l++)
			{
				GameObject colorSelector2 = UnityEngine.Object.Instantiate<GameObject>(this.colorSelectorDeactivatedFillerPrefab, this.colorSelectorButtonHolder);
				this.colorSelectorDeactivatedFillers.Add(colorSelector2);
			}
			this.SetCurrentValue(this.currentValue);
		}

		/// <summary>
		/// Makes sure current colors are allowed by the current ethnic group.
		/// If the current color isn't available in the ethnic group ones, pick the closest one.
		/// </summary>
		/// <param name="creature">Current creature.</param>
		/// <param name="ethnicGroup">Current ethnic group.</param>
		// Token: 0x06002B2F RID: 11055 RVA: 0x00123EE0 File Offset: 0x001220E0
		private void CheckEthnicGroupValues(Creature creature, CreatureData.EthnicGroup ethnicGroup)
		{
			List<Color> hairPrimaryColors = creature.data.GetPrimaryHairColors(ethnicGroup, this.useSharedColors);
			List<Color> hairSpecularColors = creature.data.GetSpecularHairColors(ethnicGroup, this.useSharedColors);
			List<Color> eyesColorsIris = creature.data.GetIrisColors(ethnicGroup, this.useSharedColors);
			List<Color> eyesColorsSclera = creature.data.GetScleraColors(ethnicGroup, this.useSharedColors);
			List<Color> skinColors = creature.data.GetSkinColors(ethnicGroup, this.useSharedColors);
			if (!hairPrimaryColors.Contains(this.currentPrimaryHairColor))
			{
				creature.SetColor(creature.data.PickHairColorPrimary(ethnicGroup, this.ClosestColor(hairPrimaryColors, this.currentPrimaryHairColor), true), Creature.ColorModifier.Hair, true);
			}
			if (!hairSpecularColors.Contains(this.currentSecondaryHairColor))
			{
				creature.SetColor(creature.data.PickHairColorSpecular(ethnicGroup, this.ClosestColor(hairSpecularColors, this.currentSecondaryHairColor), true), Creature.ColorModifier.HairSpecular, true);
			}
			if (!eyesColorsIris.Contains(this.currentEyeIrisColor))
			{
				creature.SetColor(creature.data.PickEyesColorIris(ethnicGroup, this.ClosestColor(eyesColorsIris, this.currentEyeIrisColor), true), Creature.ColorModifier.EyesIris, true);
			}
			if (!eyesColorsSclera.Contains(this.currentEyeScleraColor))
			{
				creature.SetColor(creature.data.PickEyesColorSclera(ethnicGroup, this.ClosestColor(eyesColorsSclera, this.currentEyeScleraColor), true), Creature.ColorModifier.EyesSclera, true);
			}
			if (!skinColors.Contains(this.currentSkinColor))
			{
				creature.SetColor(creature.data.PickSkinColor(ethnicGroup, -1, true, true), Creature.ColorModifier.Skin, true);
			}
		}

		// Token: 0x06002B30 RID: 11056 RVA: 0x0012402F File Offset: 0x0012222F
		public void SetUseSharedColor(bool active)
		{
			this.useSharedColors = active;
			this.LoadValue();
		}

		// Token: 0x06002B31 RID: 11057 RVA: 0x0012403E File Offset: 0x0012223E
		public void SetHairModifier()
		{
			this.SetCurrentPartColor(UISelectionListButtonsColor.PartColor.Hair);
			this.SetCurrentPartColorType(UISelectionListButtonsColor.PartColorType.Primary);
			this.primaryToggle.isOn = true;
			this.colorSelectorScrollController.ResetPosition();
			this.colorTypeTabs.gameObject.SetActive(true);
		}

		// Token: 0x06002B32 RID: 11058 RVA: 0x00124076 File Offset: 0x00122276
		public void SetSkinModifier()
		{
			this.SetCurrentPartColor(UISelectionListButtonsColor.PartColor.Skin);
			this.SetCurrentPartColorType(UISelectionListButtonsColor.PartColorType.Primary);
			this.primaryToggle.isOn = true;
			this.colorSelectorScrollController.ResetPosition();
			this.colorTypeTabs.gameObject.SetActive(false);
		}

		// Token: 0x06002B33 RID: 11059 RVA: 0x001240AE File Offset: 0x001222AE
		public void SetEyesModifier()
		{
			this.SetCurrentPartColor(UISelectionListButtonsColor.PartColor.Eyes);
			this.SetCurrentPartColorType(UISelectionListButtonsColor.PartColorType.Primary);
			this.primaryToggle.isOn = true;
			this.colorSelectorScrollController.ResetPosition();
			this.colorTypeTabs.gameObject.SetActive(true);
		}

		// Token: 0x06002B34 RID: 11060 RVA: 0x001240E6 File Offset: 0x001222E6
		public void SetCurrentPartColor(UISelectionListButtonsColor.PartColor part)
		{
			this.color = part;
			this.LoadValue();
		}

		// Token: 0x06002B35 RID: 11061 RVA: 0x001240F5 File Offset: 0x001222F5
		public void SetCurrentPartPrimaryColorType()
		{
			this.SetCurrentPartColorType(UISelectionListButtonsColor.PartColorType.Primary);
			this.colorSelectorScrollController.ResetPosition();
		}

		// Token: 0x06002B36 RID: 11062 RVA: 0x00124109 File Offset: 0x00122309
		public void SetCurrentPartSecondaryColorType()
		{
			this.SetCurrentPartColorType(UISelectionListButtonsColor.PartColorType.Secondary);
			this.colorSelectorScrollController.ResetPosition();
		}

		// Token: 0x06002B37 RID: 11063 RVA: 0x0012411D File Offset: 0x0012231D
		public void SetCurrentPartColorType(UISelectionListButtonsColor.PartColorType partType)
		{
			this.colorType = partType;
			this.LoadValue();
		}

		// Token: 0x06002B38 RID: 11064 RVA: 0x0012412C File Offset: 0x0012232C
		public void SetCurrentValue(int colorIndex)
		{
			this.currentValue = Mathf.Clamp(colorIndex, this.minValue, this.maxValue);
			for (int i = 0; i < this.colorSelectorButtons.Count; i++)
			{
				this.colorSelectorButtons[i].GetComponent<Toggle>().isOn = (this.currentValue == i);
			}
			this.OnUpdateValue(false);
		}

		// Token: 0x06002B39 RID: 11065 RVA: 0x00124190 File Offset: 0x00122390
		public override void OnUpdateValue(bool silent = false)
		{
			Creature creature = this.characterSelection.GetCharacterCreature();
			switch (this.color)
			{
			case UISelectionListButtonsColor.PartColor.Hair:
			{
				UISelectionListButtonsColor.PartColorType partColorType = this.colorType;
				if (partColorType != UISelectionListButtonsColor.PartColorType.Primary)
				{
					if (partColorType == UISelectionListButtonsColor.PartColorType.Secondary)
					{
						creature.SetColor(this.availableColors[this.currentValue], Creature.ColorModifier.HairSpecular, true);
					}
				}
				else
				{
					creature.SetColor(this.availableColors[this.currentValue], Creature.ColorModifier.Hair, true);
				}
				break;
			}
			case UISelectionListButtonsColor.PartColor.Eyes:
			{
				UISelectionListButtonsColor.PartColorType partColorType = this.colorType;
				if (partColorType != UISelectionListButtonsColor.PartColorType.Primary)
				{
					if (partColorType == UISelectionListButtonsColor.PartColorType.Secondary)
					{
						creature.SetColor(this.availableColors[this.currentValue], Creature.ColorModifier.EyesSclera, true);
					}
				}
				else
				{
					creature.SetColor(this.availableColors[this.currentValue], Creature.ColorModifier.EyesIris, true);
				}
				break;
			}
			case UISelectionListButtonsColor.PartColor.Skin:
				creature.SetColor(this.availableColors[this.currentValue], Creature.ColorModifier.Skin, true);
				if (this.colorViewer)
				{
					this.colorViewer.color = this.availableColors[this.currentValue];
				}
				break;
			}
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B3A RID: 11066 RVA: 0x001242AC File Offset: 0x001224AC
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (!this.characterSelection)
			{
				return;
			}
			if (!this.characterSelection.initialized || !this.isInteractable)
			{
				return;
			}
			switch (this.color)
			{
			case UISelectionListButtonsColor.PartColor.Hair:
				if (this.colorViewer)
				{
					this.colorViewer.color = this.availableColors[this.currentValue];
				}
				if (this.colorViewer2)
				{
					this.colorViewer2.color = this.availableColors[this.currentValue];
					this.colorViewer2.enabled = true;
					return;
				}
				break;
			case UISelectionListButtonsColor.PartColor.Eyes:
				if (this.colorViewer)
				{
					this.colorViewer.color = this.availableColors[this.currentValue];
				}
				if (this.colorViewer2)
				{
					this.colorViewer2.color = this.availableColors[this.currentValue];
					this.colorViewer2.enabled = true;
					return;
				}
				break;
			case UISelectionListButtonsColor.PartColor.Skin:
				if (this.colorViewer)
				{
					this.colorViewer.color = this.availableColors[this.currentValue];
				}
				if (this.colorViewer2)
				{
					this.colorViewer2.enabled = false;
				}
				break;
			default:
				return;
			}
		}

		// Token: 0x06002B3B RID: 11067 RVA: 0x00124400 File Offset: 0x00122600
		public override void Refresh()
		{
			base.Refresh();
			Creature creature = this.characterSelection.GetCharacterCreature();
			this.currentPrimaryHairColor = creature.GetColor(Creature.ColorModifier.Hair);
			this.currentSecondaryHairColor = creature.GetColor(Creature.ColorModifier.HairSpecular);
			this.currentEyeIrisColor = creature.GetColor(Creature.ColorModifier.EyesIris);
			this.currentEyeScleraColor = creature.GetColor(Creature.ColorModifier.EyesSclera);
			this.currentSkinColor = creature.GetColor(Creature.ColorModifier.Skin);
			this.CheckEthnicGroupValues(creature, this.characterSelection.GetCharacterCreatureEthnicGroup());
		}

		// Token: 0x06002B3C RID: 11068 RVA: 0x00124474 File Offset: 0x00122674
		private int ClosestColor(List<Color> colors, Color target)
		{
			Color nearest_color = Color.black;
			double dbl_input_red = Convert.ToDouble(target.r);
			double dbl_input_green = Convert.ToDouble(target.g);
			double dbl_input_blue = Convert.ToDouble(target.b);
			double distance = 500.0;
			foreach (Color color in colors)
			{
				double dbl_test_red = Math.Pow(Convert.ToDouble(color.r) - dbl_input_red, 2.0);
				double dbl_test_green = Math.Pow(Convert.ToDouble(color.g) - dbl_input_green, 2.0);
				double temp = Math.Sqrt(Math.Pow(Convert.ToDouble(color.b) - dbl_input_blue, 2.0) + dbl_test_green + dbl_test_red);
				if (temp == 0.0)
				{
					nearest_color = color;
					break;
				}
				if (temp < distance)
				{
					distance = temp;
					nearest_color = color;
				}
			}
			return colors.FindIndex((Color n) => n == nearest_color);
		}

		// Token: 0x06002B3E RID: 11070 RVA: 0x001245CA File Offset: 0x001227CA
		[CompilerGenerated]
		private IEnumerator <OnEnable>g__ResetUISelection|23_0()
		{
			yield return null;
			this.SetHairModifier();
			this.hairToggle.toggle.isOn = true;
			this.hairToggle.SetButtonState(true);
			yield break;
		}

		// Token: 0x040028E0 RID: 10464
		public GameObject colorSelectorButtonPrefab;

		// Token: 0x040028E1 RID: 10465
		public GameObject colorSelectorDeactivatedFillerPrefab;

		// Token: 0x040028E2 RID: 10466
		public int colorSelectorDeactivatedFillerNumber = 30;

		// Token: 0x040028E3 RID: 10467
		public Transform colorSelectorButtonHolder;

		// Token: 0x040028E4 RID: 10468
		public UIScrollController colorSelectorScrollController;

		// Token: 0x040028E5 RID: 10469
		public Transform colorTypeTabs;

		// Token: 0x040028E6 RID: 10470
		public Toggle primaryToggle;

		// Token: 0x040028E7 RID: 10471
		public Toggle secondaryToggle;

		// Token: 0x040028E8 RID: 10472
		public UISelectionListButtonsColor.PartColor color;

		// Token: 0x040028E9 RID: 10473
		public UISelectionListButtonsColor.PartColorType colorType;

		// Token: 0x040028EA RID: 10474
		public bool useSharedColors;

		// Token: 0x040028EB RID: 10475
		public UICustomisableButton hairToggle;

		// Token: 0x040028EC RID: 10476
		protected List<Color> availableColors;

		// Token: 0x040028ED RID: 10477
		private readonly List<GameObject> colorSelectorButtons = new List<GameObject>();

		// Token: 0x040028EE RID: 10478
		private readonly List<GameObject> colorSelectorDeactivatedFillers = new List<GameObject>();

		// Token: 0x040028EF RID: 10479
		private Color currentPrimaryHairColor;

		// Token: 0x040028F0 RID: 10480
		private Color currentSecondaryHairColor;

		// Token: 0x040028F1 RID: 10481
		private Color currentSkinColor;

		// Token: 0x040028F2 RID: 10482
		private Color currentEyeScleraColor;

		// Token: 0x040028F3 RID: 10483
		private Color currentEyeIrisColor;

		// Token: 0x02000AA3 RID: 2723
		public enum PartColor
		{
			// Token: 0x040048FF RID: 18687
			Hair,
			// Token: 0x04004900 RID: 18688
			Eyes,
			// Token: 0x04004901 RID: 18689
			Skin
		}

		// Token: 0x02000AA4 RID: 2724
		public enum PartColorType
		{
			// Token: 0x04004903 RID: 18691
			Primary,
			// Token: 0x04004904 RID: 18692
			Secondary
		}
	}
}
