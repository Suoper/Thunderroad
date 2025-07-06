using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000390 RID: 912
	public class UISelectionListButtonsLevelModeOption : UISelectionListButtons, IPointerEnterHandler, IEventSystemHandler
	{
		// Token: 0x06002B6F RID: 11119 RVA: 0x0012536C File Offset: 0x0012356C
		public void OnPointerEnter(PointerEventData eventData)
		{
			this.SetLevelOptionDescription();
		}

		// Token: 0x1700029D RID: 669
		// (get) Token: 0x06002B70 RID: 11120 RVA: 0x00125374 File Offset: 0x00123574
		public override bool Loop
		{
			get
			{
				return !(this.CurrentOption is Option) || this.numberOfValues > this.maxIcon;
			}
		}

		// Token: 0x1700029E RID: 670
		// (get) Token: 0x06002B71 RID: 11121 RVA: 0x00125393 File Offset: 0x00123593
		// (set) Token: 0x06002B72 RID: 11122 RVA: 0x0012539B File Offset: 0x0012359B
		public UIText optionDescriptionText { get; private set; }

		// Token: 0x1700029F RID: 671
		// (get) Token: 0x06002B73 RID: 11123 RVA: 0x001253A4 File Offset: 0x001235A4
		// (set) Token: 0x06002B74 RID: 11124 RVA: 0x001253AC File Offset: 0x001235AC
		public OptionBase CurrentOption { get; private set; }

		// Token: 0x170002A0 RID: 672
		// (get) Token: 0x06002B75 RID: 11125 RVA: 0x001253B8 File Offset: 0x001235B8
		public override int stepValue
		{
			get
			{
				Option option = this.CurrentOption as Option;
				if (option == null)
				{
					return 1;
				}
				return option.stepValue;
			}
		}

		// Token: 0x06002B76 RID: 11126 RVA: 0x001253DC File Offset: 0x001235DC
		private void Start()
		{
			if (!this.valueInitialised)
			{
				this.LoadValue();
				this.InitValues();
				this.OnUpdateValue(true);
			}
		}

		// Token: 0x06002B77 RID: 11127 RVA: 0x001253F9 File Offset: 0x001235F9
		private void OnDestroy()
		{
			if (this.optionDescriptionText != null)
			{
				this.optionDescriptionText.text = string.Empty;
			}
		}

		// Token: 0x06002B78 RID: 11128 RVA: 0x0012541C File Offset: 0x0012361C
		private void SetLevelOptionDescription()
		{
			if (string.IsNullOrEmpty(this.descriptionText))
			{
				this.RemoveLevelOptionDescription();
			}
			if (this.optionDescriptionText != null)
			{
				this.optionDescriptionText.SetLocalizationIds("Default", this.descriptionText);
				if (!this.optionDescriptionText.gameObject.activeInHierarchy)
				{
					this.optionDescriptionText.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x06002B79 RID: 11129 RVA: 0x00125483 File Offset: 0x00123683
		private void RemoveLevelOptionDescription()
		{
			if (this.optionDescriptionText != null)
			{
				this.optionDescriptionText.gameObject.SetActive(false);
			}
		}

		// Token: 0x06002B7A RID: 11130 RVA: 0x001254A4 File Offset: 0x001236A4
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.SetTexts();
			this.RefreshDisplay();
		}

		// Token: 0x06002B7B RID: 11131 RVA: 0x001254C3 File Offset: 0x001236C3
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002B7C RID: 11132 RVA: 0x001254D8 File Offset: 0x001236D8
		public void Setup(OptionBase option, UIText optionDescriptionText)
		{
			this.CurrentOption = option;
			this.optionDescriptionText = optionDescriptionText;
			this.RemoveLevelOptionDescription();
			this.LoadValue();
			this.InitValues();
			this.OnUpdateValue(false);
			this.toggle.onUpdateValueEvent += this.OnUpdateToggle;
			this.toggle.LoadValue();
			this.toggle.OnUpdateValue(false);
		}

		// Token: 0x06002B7D RID: 11133 RVA: 0x0012553C File Offset: 0x0012373C
		public void InitValues()
		{
			this.minValue = 0;
			if (this.value != null)
			{
				this.value.text = "";
				this.value.gameObject.SetActive(false);
			}
			if (this.CurrentOption == null)
			{
				return;
			}
			this.SetTexts();
			OptionBase currentOption = this.CurrentOption;
			Option option = currentOption as Option;
			if (option == null)
			{
				OptionBoolean optionBoolean = currentOption as OptionBoolean;
				if (optionBoolean == null)
				{
					OptionStringBase optionStringList = currentOption as OptionStringBase;
					if (optionStringList == null)
					{
						OptionEnumInt optionEnum = currentOption as OptionEnumInt;
						if (optionEnum == null)
						{
							return;
						}
						this.currentValue = optionEnum.GetDefaultEnumIndex();
						this.minValue = 0;
						this.maxValue = optionEnum.GetEnumValueCount() - 1;
						this.numberOfValues = this.maxValue + 1 - this.minValue;
						this.value.gameObject.SetActive(true);
						this.toggle.gameObject.SetActive(false);
						this.starPlace.gameObject.SetActive(false);
						if (this.nextButton != null)
						{
							this.nextButton.gameObject.SetActive(true);
						}
						if (this.previousButton != null)
						{
							this.previousButton.gameObject.SetActive(true);
						}
						this.value.text = optionEnum.CurrentValue().Value().ToString();
					}
					else
					{
						OptionIntValue optionIntValue = optionStringList.DefaultValue() as OptionIntValue;
						if (optionIntValue != null)
						{
							this.toggle.gameObject.SetActive(false);
							this.starPlace.gameObject.SetActive(false);
							if (optionStringList.StringCount() == 0)
							{
								this.value.gameObject.SetActive(false);
								if (this.nextButton != null)
								{
									this.nextButton.gameObject.SetActive(false);
								}
								if (this.previousButton != null)
								{
									this.previousButton.gameObject.SetActive(false);
								}
								this.numberOfValues = 0;
								this.minValue = 0;
								this.maxValue = 0;
								return;
							}
							this.value.gameObject.SetActive(true);
							if (this.nextButton != null)
							{
								this.nextButton.gameObject.SetActive(true);
							}
							if (this.previousButton != null)
							{
								this.previousButton.gameObject.SetActive(true);
							}
							this.minValue = 0;
							this.maxValue = optionStringList.StringCount() - 1;
							this.numberOfValues = this.maxValue + 1 - this.minValue;
							this.currentValue = optionIntValue.value;
							return;
						}
					}
				}
				else
				{
					OptionBooleanValue optionBooleanValue = optionBoolean.DefaultValue() as OptionBooleanValue;
					if (optionBooleanValue != null)
					{
						this.minValue = 0;
						this.maxValue = 1;
						this.numberOfValues = 2;
						this.value.gameObject.SetActive(false);
						this.toggle.gameObject.SetActive(true);
						this.starPlace.gameObject.SetActive(false);
						this.SetValue(optionBooleanValue.value ? 1 : 0, true);
						if (this.nextButton != null)
						{
							this.nextButton.gameObject.SetActive(false);
						}
						if (this.previousButton != null)
						{
							this.previousButton.gameObject.SetActive(false);
							return;
						}
					}
				}
			}
			else
			{
				OptionIntValue optionIntValue2 = option.DefaultValue() as OptionIntValue;
				if (optionIntValue2 != null)
				{
					this.minValue = option.MinValue();
					this.maxValue = option.MaxValue();
					this.currentValue = Mathf.Clamp(optionIntValue2.value, this.minValue, this.maxValue);
					this.numberOfValues = this.maxValue + 1 - this.minValue;
					if (this.numberOfValues > this.maxIcon)
					{
						this.value.gameObject.SetActive(true);
						this.toggle.gameObject.SetActive(false);
						this.starPlace.gameObject.SetActive(false);
						if (this.nextButton != null)
						{
							this.nextButton.gameObject.SetActive(true);
						}
						if (this.previousButton != null)
						{
							this.previousButton.gameObject.SetActive(true);
						}
						this.value.text = this.currentValue.ToString();
						return;
					}
					this.value.gameObject.SetActive(false);
					this.toggle.gameObject.SetActive(false);
					this.starPlace.gameObject.SetActive(true);
					if (this.nextButton != null)
					{
						this.nextButton.gameObject.SetActive(true);
					}
					if (this.previousButton != null)
					{
						this.previousButton.gameObject.SetActive(true);
					}
					if (this.images != null)
					{
						for (int i = 0; i < this.images.Length; i++)
						{
							UnityEngine.Object.Destroy(this.images[i].gameObject);
						}
						this.images = null;
					}
					this.images = new Image[this.numberOfValues];
					for (int j = 0; j < this.numberOfValues; j++)
					{
						this.images[j] = UnityEngine.Object.Instantiate<Image>(this.starImagePrefab, this.starPlace);
						this.images[j].transform.SetParent(this.starPlace);
					}
					if (this.images.Length != 0)
					{
						for (int k = 0; k < this.currentValue; k++)
						{
							this.images[k].sprite = this.starImageFill;
						}
						return;
					}
				}
			}
		}

		// Token: 0x06002B7E RID: 11134 RVA: 0x00125A9B File Offset: 0x00123C9B
		private void OnLanguageChanged(string language)
		{
			this.SetTexts();
			this.RefreshDisplay();
		}

		// Token: 0x06002B7F RID: 11135 RVA: 0x00125AAC File Offset: 0x00123CAC
		private void SetTexts()
		{
			if (this.CurrentOption == null)
			{
				return;
			}
			if (this.title != null)
			{
				this.title.text = (LocalizationManager.Instance.GetLocalizedString("Default", this.CurrentOption.displayName, false) ?? this.CurrentOption.displayName);
			}
			this.descriptionText = "{" + this.CurrentOption.description + "}";
		}

		// Token: 0x06002B80 RID: 11136 RVA: 0x00125B25 File Offset: 0x00123D25
		public override void LoadValue()
		{
		}

		// Token: 0x06002B81 RID: 11137 RVA: 0x00125B28 File Offset: 0x00123D28
		public int GetValue()
		{
			OptionBase<OptionIntValue> intOption = this.CurrentOption as OptionBase<OptionIntValue>;
			if (intOption != null)
			{
				return (intOption.CurrentValue() as OptionIntValue).value;
			}
			if (this.CurrentOption is OptionBoolean)
			{
				return this.toggle.GetCurrentValue();
			}
			return base.GetCurrentValue();
		}

		// Token: 0x06002B82 RID: 11138 RVA: 0x00125B74 File Offset: 0x00123D74
		public void SetValue(OptionValue value, bool silent = false)
		{
			int newValue = 0;
			OptionIntValue intValue = value as OptionIntValue;
			if (intValue != null)
			{
				newValue = intValue.value;
				OptionEnumInt optionEnum = this.CurrentOption as OptionEnumInt;
				if (optionEnum != null)
				{
					newValue = optionEnum.GetEnumIndex(newValue);
				}
			}
			OptionBooleanValue booleanValue = value as OptionBooleanValue;
			if (booleanValue != null)
			{
				newValue = (booleanValue.value ? 1 : 0);
			}
			this.SetValue(newValue, silent);
		}

		// Token: 0x06002B83 RID: 11139 RVA: 0x00125BCA File Offset: 0x00123DCA
		public override void SetValue(int value, bool silent = false)
		{
			base.SetValue(value, silent);
			if (this.CurrentOption is OptionBoolean)
			{
				this.toggle.SetValue(value, true);
				this.toggle.RefreshDisplay();
			}
			this.valueInitialised = true;
		}

		// Token: 0x06002B84 RID: 11140 RVA: 0x00125C00 File Offset: 0x00123E00
		public override void OnUpdateValue(bool silent = false)
		{
			OptionBase currentOption = this.CurrentOption;
			OptionEnumInt optionEnum = currentOption as OptionEnumInt;
			if (optionEnum == null)
			{
				OptionBoolean optionBoolean = currentOption as OptionBoolean;
				if (optionBoolean == null)
				{
					OptionBase<OptionIntValue> optionBaseInt = currentOption as OptionBase<OptionIntValue>;
					if (optionBaseInt != null)
					{
						optionBaseInt.SetValue(new OptionIntValue(this.currentValue));
					}
				}
				else
				{
					optionBoolean.SetValue(new OptionBooleanValue
					{
						value = (this.currentValue > 0)
					});
				}
			}
			else
			{
				optionEnum.SetEnumValueFromIndex(this.currentValue);
			}
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B85 RID: 11141 RVA: 0x00125C7C File Offset: 0x00123E7C
		private void OnUpdateToggle(UISelectionListButtons obj)
		{
			if (this.toggle.gameObject.activeSelf)
			{
				this.SetValue(this.toggle.GetCurrentValue(), false);
			}
		}

		// Token: 0x06002B86 RID: 11142 RVA: 0x00125CA4 File Offset: 0x00123EA4
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			OptionBase currentOption = this.CurrentOption;
			OptionBoolean optionBool = currentOption as OptionBoolean;
			if (optionBool != null)
			{
				int tempBoolValue = (optionBool.CurrentValue() as OptionBooleanValue).value ? 1 : 0;
				this.toggle.SetValue(tempBoolValue, true);
				this.toggle.RefreshDisplay();
				return;
			}
			Option option = currentOption as Option;
			if (option == null)
			{
				OptionStringBase optionStringList = currentOption as OptionStringBase;
				if (optionStringList == null)
				{
					OptionEnumInt optionEnumInt = currentOption as OptionEnumInt;
					if (optionEnumInt == null)
					{
						return;
					}
					this.value.text = optionEnumInt.GetCurrentValueLabel();
				}
				else if (this.minValue != this.maxValue)
				{
					this.value.text = optionStringList.GetCurrentValueLabel();
					return;
				}
				return;
			}
			int tempIntValue = (option.CurrentValue() as OptionIntValue).value;
			if (this.numberOfValues <= this.maxIcon)
			{
				for (int i = 0; i < this.numberOfValues; i++)
				{
					this.images[i].sprite = this.starImage;
				}
				for (int j = 0; j < tempIntValue; j++)
				{
					this.images[j].sprite = this.starImageFill;
				}
				return;
			}
			this.value.text = option.GetCurrentValueLabel();
		}

		// Token: 0x0400290D RID: 10509
		public Image starImagePrefab;

		// Token: 0x0400290E RID: 10510
		public Sprite starImage;

		// Token: 0x0400290F RID: 10511
		public Sprite starImageFill;

		// Token: 0x04002910 RID: 10512
		public UISelectionListButtonsBool toggle;

		// Token: 0x04002911 RID: 10513
		public Transform starPlace;

		// Token: 0x04002912 RID: 10514
		public int maxIcon = 5;

		// Token: 0x04002913 RID: 10515
		private string descriptionText;

		// Token: 0x04002914 RID: 10516
		private Image[] images;

		// Token: 0x04002915 RID: 10517
		private int numberOfValues;

		// Token: 0x04002916 RID: 10518
		private bool valueInitialised;
	}
}
