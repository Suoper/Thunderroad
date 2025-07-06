using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200039C RID: 924
	public class UIWorldMapLevelPresentation : MonoBehaviour
	{
		// Token: 0x06002C06 RID: 11270 RVA: 0x0012880C File Offset: 0x00126A0C
		public void SetLocationSelected()
		{
			LevelInstance levelInstance = this.worldMapBoard.currentLevelInstance;
			this.SetEnemyConfig();
			this.SetupLevelInfo();
			this.ToggleRightPanelContent(levelInstance.instanceData == null);
			this.levelModeIndex = 0;
			List<LevelData.Mode> levelDataModes = levelInstance.LevelData.GetGameModeAllowedModes();
			this.levelModeCount = levelDataModes.Count;
			this.startIndexOption = 0;
			this.currentIndexOption = 0;
			this.modeSelector.UpdateValues(levelInstance);
			this.modePrevious.SetActive(levelDataModes.Count > 1);
			this.modeNext.SetActive(levelDataModes.Count > 1);
			this.BuildOptions();
			this.ShowOption();
			this.BuildAttributeGroups();
		}

		// Token: 0x06002C07 RID: 11271 RVA: 0x001288B4 File Offset: 0x00126AB4
		public void UpdateLocalizedFields()
		{
			if (this.worldMapBoard.currentLevelInstance == null)
			{
				return;
			}
			LevelData levelData = this.worldMapBoard.currentLevelInstance.LevelData;
			StringBuilder titleBuilder = new StringBuilder();
			titleBuilder.Append(LocalizationManager.Instance.GetLocalizedString("Levels", levelData.name, false) ?? levelData.name);
			string modeDescription = LocalizationManager.Instance.GetLocalizedString("Levels", levelData.descriptionLocalizationId, false) ?? levelData.description;
			if (this.enemyConfig != null)
			{
				titleBuilder.Append(" - ");
				titleBuilder.Append(LocalizationManager.Instance.GetLocalizedString("EnemyConfig", this.enemyConfig.nameLocalizationId, false) ?? this.enemyConfig.name);
				string description;
				if (this.enemyConfig.TryGetLevelDescription(levelData.id, out description))
				{
					modeDescription = (LocalizationManager.Instance.GetLocalizedString("EnemyConfig", description, false) ?? description);
				}
			}
			this.levelTitleText.text = titleBuilder.ToString();
			this.modeDescriptionText.text = modeDescription;
		}

		/// <summary>
		/// Load the level for the selected location and mode
		/// </summary>
		// Token: 0x06002C08 RID: 11272 RVA: 0x001289BC File Offset: 0x00126BBC
		public void Travel()
		{
			Dictionary<string, string> options = new Dictionary<string, string>();
			LevelInstance levelInstance = this.worldMapBoard.currentLevelInstance;
			if (levelInstance.IsUserConfigurable())
			{
				for (int i = 0; i < this.levelModeOptions.Count; i++)
				{
					UISelectionListButtonsLevelModeOption buttonOption = this.levelModeOptions[i];
					OptionBase currentOption = buttonOption.CurrentOption;
					if (currentOption.IsLevelOption())
					{
						if (!(currentOption is Option) && !(currentOption is OptionBoolean))
						{
							OptionEnumInt optionEnum = currentOption as OptionEnumInt;
							if (optionEnum == null)
							{
								OptionStringBase optionString = currentOption as OptionStringBase;
								if (optionString != null)
								{
									int index = buttonOption.GetValue();
									optionString.SetValue(new OptionIntValue(index));
									string stringValue = optionString.CurrentStringValue().value;
									if (!string.IsNullOrEmpty(stringValue))
									{
										options.Add(optionString.name, stringValue);
									}
								}
							}
							else
							{
								int index2 = buttonOption.GetValue();
								optionEnum.SetValue(new OptionIntValue(index2));
								Type enumType = optionEnum.GetEnumType();
								object enumValue = Enum.ToObject(enumType, index2);
								if (Enum.IsDefined(enumType, enumValue))
								{
									string stringValue2 = enumValue.ToString();
									if (!string.IsNullOrEmpty(stringValue2))
									{
										options.Add(optionEnum.name, stringValue2);
									}
								}
							}
						}
						else
						{
							options.Add(currentOption.name, buttonOption.GetValue().ToString());
						}
					}
				}
			}
			else
			{
				options = levelInstance.instanceData.PreBuildOptions();
			}
			if (levelInstance.LevelData.id == Player.characterData.mode.data.levelHome)
			{
				options.Add(LevelOption.PlayerSpawnerId.Get(), Player.characterData.mode.data.levelHomeTravelSpawnerId);
			}
			GameManager.local.StartCoroutine(this.TravelCoroutine(levelInstance.LevelData.id, levelInstance.GetMode(), options));
		}

		// Token: 0x06002C09 RID: 11273 RVA: 0x00128B7B File Offset: 0x00126D7B
		private IEnumerator TravelCoroutine(string levelId, LevelData.Mode mode, Dictionary<string, string> options)
		{
			Pointer pointer = Pointer.GetActive();
			bool playedSound = false;
			if (pointer != null)
			{
				IResourceLocation travelAudio = this.worldMapBoard.currentLevelInstance.GetTravelAudio();
				if (travelAudio != null)
				{
					playedSound = true;
					pointer.PlayOneShot(travelAudio, 1f);
				}
			}
			if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				this.worldMapBoard.worldMapCanvasCuller.enabled = false;
				yield return CameraEffects.local.ProgressiveAction(1f, delegate(float p)
				{
					this.worldMapBoard.worldMapCanvasCuller.canvasGroup.alpha = 1f - p;
				});
			}
			yield return CameraEffects.DoTimedEffectCoroutine(Color.black, CameraEffects.TimedEffect.FadeIn, 1f);
			while (playedSound && pointer.IsPlaying())
			{
				yield return Yielders.ForSeconds(0.1f);
			}
			yield return LevelManager.LoadLevelCoroutine(levelId, mode, options, LoadingCamera.State.Enabled).WrapSafely(delegate(Exception e)
			{
				Debug.LogError("Error while loading level: " + e.Message + "\n" + e.StackTrace);
			});
			yield break;
		}

		/// <summary>
		/// Change to the next mode for the selected map
		/// </summary>
		// Token: 0x06002C0A RID: 11274 RVA: 0x00128B9F File Offset: 0x00126D9F
		public void NextMode()
		{
			this.levelModeIndex++;
			if (this.levelModeIndex >= this.levelModeCount)
			{
				this.levelModeIndex = 0;
			}
			this.startIndexOption = 0;
			this.currentIndexOption = 0;
			this.BuildOptions();
			this.ShowOption();
		}

		/// <summary>
		/// Change to the previous mode for the selected map
		/// </summary>
		// Token: 0x06002C0B RID: 11275 RVA: 0x00128BE0 File Offset: 0x00126DE0
		public void PreviousMode()
		{
			this.levelModeIndex--;
			if (this.levelModeIndex < 0)
			{
				this.levelModeIndex = this.levelModeCount - 1;
			}
			this.startIndexOption = 0;
			this.currentIndexOption = 0;
			this.BuildOptions();
			this.ShowOption();
		}

		/// <summary>
		/// Change to the next options page for the selected map and selected mode
		/// </summary>
		// Token: 0x06002C0C RID: 11276 RVA: 0x00128C2C File Offset: 0x00126E2C
		public void NextOptions()
		{
			this.startIndexOption += 3;
			this.optionIndex++;
			this.ShowOption();
		}

		/// <summary>
		/// Change to the previous options page for the selected map and selected mode
		/// </summary>
		// Token: 0x06002C0D RID: 11277 RVA: 0x00128C50 File Offset: 0x00126E50
		public void PreviousOptions()
		{
			this.startIndexOption -= 3;
			this.optionIndex--;
			this.ShowOption();
		}

		// Token: 0x06002C0E RID: 11278 RVA: 0x00128C74 File Offset: 0x00126E74
		private void OnDestroy()
		{
			this.ClearBanner();
		}

		/// <summary>
		/// Find if there is an enemy config in level options (from level instance).
		/// </summary>
		// Token: 0x06002C0F RID: 11279 RVA: 0x00128C7C File Offset: 0x00126E7C
		private void SetEnemyConfig()
		{
			this.enemyConfig = null;
			LevelInstance levelInstance = this.worldMapBoard.currentLevelInstance;
			if (levelInstance != null && levelInstance.instanceData != null)
			{
				Dictionary<string, string> options = levelInstance.instanceData.PreBuildOptions();
				string enemyConfigId;
				if (options != null && options.TryGetValue(LevelOption.EnemyConfig.Get(), out enemyConfigId))
				{
					this.enemyConfig = Catalog.GetData<EnemyConfig>(enemyConfigId, true);
				}
			}
		}

		/// <summary>
		/// SetUp the correct Level Title, description and banner.
		/// They can change according to the faction;
		/// </summary>
		// Token: 0x06002C10 RID: 11280 RVA: 0x00128CD4 File Offset: 0x00126ED4
		private void SetupLevelInfo()
		{
			Sprite mapPreviewTexture = this.worldMapBoard.currentLevelInstance.LevelData.mapPreviewImage;
			if (mapPreviewTexture != null)
			{
				this.mapPreview.gameObject.SetActive(true);
				this.mapPreview.sprite = mapPreviewTexture;
			}
			else
			{
				this.mapPreview.gameObject.SetActive(false);
			}
			this.UpdateLocalizedFields();
			this.ClearBanner();
			if (this.enemyConfig != null)
			{
				this.enemyBanner.gameObject.SetActive(true);
				this.bannerlevelIcon.sprite = this.worldMapBoard.currentLevelInstance.LevelData.mapLocationIcon;
				this.enemyBannerBackground.color = this.enemyConfig.color;
				this.enemyBannerIcon.gameObject.SetActive(false);
				Catalog.LoadAssetAsync<Sprite>(this.enemyConfig.iconAddress, new Action<Sprite>(this.OnEnemyIconLoaded), typeof(UIWorldMapLocation).Name);
				this.levelTitleBox.offsetMin = Vector2.right * this.titleBannerOffset + Vector2.up * this.levelTitleBox.offsetMin.y;
				this.levelTitleText.rectTransform.offsetMin = Vector2.right * this.titleBannerOffset;
				this.levelTitleText.alignment = TextAlignmentOptions.MidlineLeft;
				return;
			}
			this.levelTitleBox.offsetMin = Vector2.up * this.levelTitleBox.offsetMin.y;
			this.levelTitleText.rectTransform.offsetMin = Vector2.zero;
			this.levelTitleText.alignment = TextAlignmentOptions.Midline;
		}

		// Token: 0x06002C11 RID: 11281 RVA: 0x00128E7F File Offset: 0x0012707F
		private void OnEnemyIconLoaded(Sprite obj)
		{
			this.enemyBannerIcon.sprite = obj;
			this.bannerlevelIcon.color = this.enemyConfig.iconColor;
			this.enemyBannerIcon.gameObject.SetActive(true);
		}

		/// <summary>
		/// Release texture from banner and reset the banner data
		/// </summary>
		// Token: 0x06002C12 RID: 11282 RVA: 0x00128EB4 File Offset: 0x001270B4
		private void ClearBanner()
		{
			this.enemyBanner.gameObject.SetActive(false);
			this.bannerlevelIcon.sprite = null;
			if (this.enemyBannerIcon.sprite != null)
			{
				Sprite sprite = this.enemyBannerIcon.sprite;
				this.enemyBannerIcon.sprite = null;
				Catalog.ReleaseAsset<Sprite>(sprite);
			}
		}

		/// <summary>
		/// Toggle each panels active state.
		/// </summary>
		// Token: 0x06002C13 RID: 11283 RVA: 0x00128F0D File Offset: 0x0012710D
		private void ToggleRightPanelContent(bool configurationState)
		{
			this.mapRightPanelAttrubteDisplay.SetActive(!configurationState);
			this.mapRightPanelConfigurable.SetActive(configurationState);
		}

		/// <summary>
		/// Create the options selection panel
		/// </summary>
		// Token: 0x06002C14 RID: 11284 RVA: 0x00128F2C File Offset: 0x0012712C
		private void BuildOptions()
		{
			LevelInstance levelInstance = this.worldMapBoard.currentLevelInstance;
			for (int i = 0; i < this.levelModeOptions.Count; i++)
			{
				UnityEngine.Object.Destroy(this.levelModeOptions[i].gameObject);
			}
			this.levelModeOptions.Clear();
			List<LevelData.Mode> levelDataModes = levelInstance.LevelData.GetGameModeAllowedModes();
			if (levelDataModes.Count == 0)
			{
				return;
			}
			if (!levelInstance.IsUserConfigurable())
			{
				return;
			}
			levelInstance.LevelDataMode = levelDataModes[this.levelModeIndex];
			levelInstance.levelDataModeId = levelDataModes[this.levelModeIndex].name;
			this.currentOptions = levelDataModes[this.levelModeIndex].availableOptions;
			for (int j = 0; j < this.currentOptions.Count; j++)
			{
				UISelectionListButtonsLevelModeOption buttonOption = UnityEngine.Object.Instantiate<UISelectionListButtonsLevelModeOption>(this.LevelOptionPrefab, this.optionsPanel);
				buttonOption.Setup(this.currentOptions[j], this.worldMapBoard.levelOptionDescriptionText);
				buttonOption.gameObject.SetActive(false);
				this.levelModeOptions.Add(buttonOption);
			}
			this.optionMaxPage = Mathf.CeilToInt((float)this.currentOptions.Count / 3f);
			this.optionIndex = 1;
			if (this.optionMaxPage > 1)
			{
				this.optionsPageText.gameObject.SetActive(true);
				this.nextOptionsButton.gameObject.SetActive(true);
				this.previousOptionsButton.gameObject.SetActive(true);
				return;
			}
			this.optionsPageText.gameObject.SetActive(false);
			this.nextOptionsButton.gameObject.SetActive(false);
			this.previousOptionsButton.gameObject.SetActive(false);
		}

		/// <summary>
		/// Update all options informations
		/// </summary>
		// Token: 0x06002C15 RID: 11285 RVA: 0x001290D0 File Offset: 0x001272D0
		private void ShowOption()
		{
			this.optionsPageText.text = this.optionIndex.ToString() + "/" + this.optionMaxPage.ToString();
			for (int i = 0; i < this.levelModeOptions.Count; i++)
			{
				this.levelModeOptions[i].gameObject.SetActive(false);
			}
			if (!this.worldMapBoard.currentLevelInstance.IsUserConfigurable())
			{
				return;
			}
			if (this.currentOptions.Count > this.startIndexOption + 3)
			{
				this.currentIndexOption = this.startIndexOption + 3;
				this.nextOptionsButton.IsInteractable = true;
			}
			else if (this.currentOptions.Count == this.startIndexOption + 3)
			{
				this.currentIndexOption = this.startIndexOption + 3;
				this.nextOptionsButton.IsInteractable = false;
			}
			else
			{
				this.currentIndexOption = this.currentOptions.Count;
				this.nextOptionsButton.IsInteractable = false;
			}
			this.previousOptionsButton.IsInteractable = (this.startIndexOption > 0);
			float spacing = 0f;
			for (int j = this.startIndexOption; j < this.currentIndexOption; j++)
			{
				if (!this.levelModeOptions[j].CurrentOption.IsHidden())
				{
					this.levelModeOptions[j].GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, spacing);
					this.levelModeOptions[j].gameObject.SetActive(true);
					spacing -= this.optionSpacingSize;
				}
			}
		}

		/// <summary>
		/// Create level attribute for specific level instance
		/// </summary>
		// Token: 0x06002C16 RID: 11286 RVA: 0x00129250 File Offset: 0x00127450
		private void BuildAttributeGroups()
		{
			this.ClearAttributeDisplayGroups();
			LevelInstance levelInstance = this.worldMapBoard.currentLevelInstance;
			if (levelInstance.IsUserConfigurable())
			{
				return;
			}
			bool needSeparator = false;
			foreach (UIAttributeData data in levelInstance.instanceData.GetAttributes())
			{
				this.CreateAttributeDisplay(data, this.attributeDisplayGroupAnchor, needSeparator);
				needSeparator = true;
			}
		}

		// Token: 0x06002C17 RID: 11287 RVA: 0x001292D0 File Offset: 0x001274D0
		public GameObject CreateAttributeDisplay(UIAttributeData data, Transform anchor, bool withSeparator = false)
		{
			UIAttributeGroupDisplay display = this.CreateAttributeGroupDisplay(anchor).WithTitle(data.name, "Default").WithDescription(data.value, "Default");
			if (withSeparator)
			{
				display.WithSeparator();
			}
			if (!string.IsNullOrEmpty(data.backgroundAddress))
			{
				Sprite background;
				if (!this.worldMapBoard.attributeTexture.TryGetValue(data.backgroundAddress, out background))
				{
					Debug.LogError("Map No background for address : " + data.backgroundAddress);
				}
				else
				{
					display.WithBackground(background, data.backgroundColor, 1f);
				}
			}
			if (data.isIconProgression)
			{
				Sprite textureIcon3;
				Sprite textureBgIcon;
				if (data.iconList != null && data.bgIconList != null)
				{
					List<ValueTuple<Sprite, Color>> icons = new List<ValueTuple<Sprite, Color>>();
					int count = data.iconList.Count;
					for (int i = 0; i < count; i++)
					{
						string address = data.iconList[i].Item1;
						Sprite textureIcon;
						if (!this.worldMapBoard.attributeTexture.TryGetValue(address, out textureIcon))
						{
							Debug.LogError("Map No Icon for address : " + address);
						}
						else
						{
							icons.Add(new ValueTuple<Sprite, Color>(textureIcon, data.iconList[i].Item2));
						}
					}
					List<ValueTuple<Sprite, Color>> bgIcons = new List<ValueTuple<Sprite, Color>>();
					count = data.bgIconList.Count;
					for (int j = 0; j < count; j++)
					{
						string address2 = data.bgIconList[j].Item1;
						Sprite textureIcon2;
						if (!this.worldMapBoard.attributeTexture.TryGetValue(address2, out textureIcon2))
						{
							Debug.LogError("Map No Icon for address : " + address2);
						}
						else
						{
							bgIcons.Add(new ValueTuple<Sprite, Color>(textureIcon2, data.bgIconList[j].Item2));
						}
					}
					if (icons.Count != bgIcons.Count)
					{
						Debug.LogError("icon list and bg icon list not same length");
					}
					else
					{
						display.WithProgressionIcons(icons, bgIcons, data.numberIcon, 1f);
					}
				}
				else if (!this.worldMapBoard.attributeTexture.TryGetValue(data.iconAddressId, out textureIcon3))
				{
					Debug.LogError("Map No Icon for address : " + data.iconAddressId);
				}
				else if (!this.worldMapBoard.attributeTexture.TryGetValue(data.iconBgProgressionAddressId, out textureBgIcon))
				{
					Debug.LogError("Map No Icon for address : " + data.iconBgProgressionAddressId);
				}
				else
				{
					display.WithProgressionIcons(textureIcon3, data.iconColor, textureBgIcon, data.iconBgProgressionColor, data.iconProgressionMax, data.numberIcon, 1f);
				}
			}
			else if (data.iconList != null)
			{
				List<ValueTuple<Sprite, Color>> icons2 = new List<ValueTuple<Sprite, Color>>();
				int count2 = data.iconList.Count;
				for (int k = 0; k < count2; k++)
				{
					string address3 = data.iconList[k].Item1;
					Sprite textureIcon4;
					if (!this.worldMapBoard.attributeTexture.TryGetValue(address3, out textureIcon4))
					{
						Debug.LogError("Map No Icon for address : " + address3);
					}
					else
					{
						icons2.Add(new ValueTuple<Sprite, Color>(textureIcon4, data.iconList[k].Item2));
					}
				}
				display.WithIcons(icons2, 1f);
			}
			else if (!string.IsNullOrEmpty(data.iconAddressId) && data.numberIcon > 0)
			{
				Sprite textureIcon5;
				if (this.worldMapBoard.attributeTexture.TryGetValue(data.iconAddressId, out textureIcon5))
				{
					display.WithIcons(textureIcon5, data.iconColor, data.numberIcon, 1f);
				}
				else
				{
					Debug.LogError("Map No Icon for address : " + data.iconAddressId);
				}
			}
			return display.gameObject;
		}

		/// <summary>
		/// Create an attribute group display on the board.
		/// This is for the right-panel to display attribute data visually.
		/// </summary>
		// Token: 0x06002C18 RID: 11288 RVA: 0x00129650 File Offset: 0x00127850
		private UIAttributeGroupDisplay CreateAttributeGroupDisplay(Transform anchor)
		{
			Transform transform = UnityEngine.Object.Instantiate<GameObject>(this.attributeDisplayGroupPrefab).transform;
			transform.SetParent(anchor);
			(transform as RectTransform).sizeDelta = Vector2.up * this.attributeDisplayGroupHeight;
			transform.localPosition = Vector3.zero;
			transform.localRotation = Quaternion.identity;
			transform.localScale = Vector3.one;
			return transform.GetComponent<UIAttributeGroupDisplay>();
		}

		/// <summary>
		/// Clear all attribute display groups from the group anchor.
		/// </summary>
		// Token: 0x06002C19 RID: 11289 RVA: 0x001296B8 File Offset: 0x001278B8
		private void ClearAttributeDisplayGroups()
		{
			for (int i = this.attributeDisplayGroupAnchor.childCount - 1; i >= 0; i--)
			{
				UnityEngine.Object.Destroy(this.attributeDisplayGroupAnchor.GetChild(i).gameObject);
			}
		}

		// Token: 0x04002982 RID: 10626
		public UIWorldMapBoard worldMapBoard;

		// Token: 0x04002983 RID: 10627
		[Header("Map Infos")]
		public RectTransform levelTitleBox;

		// Token: 0x04002984 RID: 10628
		public TextMeshProUGUI levelTitleText;

		// Token: 0x04002985 RID: 10629
		public Image mapPreview;

		// Token: 0x04002986 RID: 10630
		public TextMeshProUGUI modeDescriptionText;

		// Token: 0x04002987 RID: 10631
		[Header("Faction Banner")]
		public GameObject enemyBanner;

		// Token: 0x04002988 RID: 10632
		public Image bannerlevelIcon;

		// Token: 0x04002989 RID: 10633
		public Image enemyBannerIcon;

		// Token: 0x0400298A RID: 10634
		public Image enemyBannerBackground;

		// Token: 0x0400298B RID: 10635
		public float titleBannerOffset;

		// Token: 0x0400298C RID: 10636
		[Header("Level Options selection")]
		public Transform optionsPanel;

		// Token: 0x0400298D RID: 10637
		public UIMapLevelMode modeSelector;

		// Token: 0x0400298E RID: 10638
		public GameObject modePrevious;

		// Token: 0x0400298F RID: 10639
		public GameObject modeNext;

		// Token: 0x04002990 RID: 10640
		public UICustomisableButton nextOptionsButton;

		// Token: 0x04002991 RID: 10641
		public UICustomisableButton previousOptionsButton;

		// Token: 0x04002992 RID: 10642
		public TextMeshProUGUI optionsPageText;

		// Token: 0x04002993 RID: 10643
		public UISelectionListButtonsLevelModeOption LevelOptionPrefab;

		// Token: 0x04002994 RID: 10644
		public List<UISelectionListButtonsLevelModeOption> levelModeOptions = new List<UISelectionListButtonsLevelModeOption>();

		// Token: 0x04002995 RID: 10645
		[Header("Level instance configuration")]
		public GameObject mapRightPanelConfigurable;

		// Token: 0x04002996 RID: 10646
		public GameObject mapRightPanelAttrubteDisplay;

		// Token: 0x04002997 RID: 10647
		[Space]
		public GameObject attributeDisplayGroupPrefab;

		// Token: 0x04002998 RID: 10648
		public Transform attributeDisplayGroupAnchor;

		// Token: 0x04002999 RID: 10649
		public float attributeDisplayGroupHeight = 50f;

		// Token: 0x0400299A RID: 10650
		private EnemyConfig enemyConfig;

		// Token: 0x0400299B RID: 10651
		private int levelModeIndex;

		// Token: 0x0400299C RID: 10652
		private int levelModeCount;

		// Token: 0x0400299D RID: 10653
		private int currentIndexOption;

		// Token: 0x0400299E RID: 10654
		private int startIndexOption;

		// Token: 0x0400299F RID: 10655
		private int optionMaxPage;

		// Token: 0x040029A0 RID: 10656
		private int optionIndex = 1;

		// Token: 0x040029A1 RID: 10657
		private float optionSpacingSize = 0.1f;

		// Token: 0x040029A2 RID: 10658
		private List<OptionBase> currentOptions;
	}
}
