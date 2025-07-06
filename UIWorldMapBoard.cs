using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ThunderRoad.Modules;
using TMPro;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200039B RID: 923
	public class UIWorldMapBoard : ThunderBehaviour
	{
		// Token: 0x170002A2 RID: 674
		// (get) Token: 0x06002BE8 RID: 11240 RVA: 0x00127DBE File Offset: 0x00125FBE
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x170002A3 RID: 675
		// (get) Token: 0x06002BE9 RID: 11241 RVA: 0x00127DC1 File Offset: 0x00125FC1
		// (set) Token: 0x06002BEA RID: 11242 RVA: 0x00127DCC File Offset: 0x00125FCC
		public int worldMapIndex
		{
			get
			{
				return this._worldMapIndex;
			}
			set
			{
				this.worldMaps[this._worldMapIndex].Show(false);
				if (!this.worldMapsCustomInfos.IsNullOrEmpty() && this.worldMapsCustomInfos[this._worldMapIndex] != null)
				{
					this.worldMapsCustomInfos[this._worldMapIndex].SetActive(false);
				}
				this._worldMapIndex = value;
				if (this._worldMapIndex >= this.worldMaps.Count)
				{
					this._worldMapIndex = 0;
				}
				if (this._worldMapIndex < 0)
				{
					this._worldMapIndex = this.worldMaps.Count - 1;
				}
				this.worldMaps[this._worldMapIndex].Show(true);
				if (!this.worldMapsCustomInfos.IsNullOrEmpty() && this.worldMapsCustomInfos[this._worldMapIndex] != null && GameModeManager.instance.currentGameMode != null && GameModeManager.instance.currentGameMode.name.Equals("CrystalHunt") && Player.characterData.tutorial.state != TutorialManager.TutorialState.InProgress)
				{
					this.worldMapCustomInfosAnchor.parent.gameObject.SetActive(true);
					this.worldMapsCustomInfos[this._worldMapIndex].SetActive(true);
				}
				else
				{
					this.worldMapCustomInfosAnchor.parent.gameObject.SetActive(false);
				}
				this.locationIndex = 0;
			}
		}

		// Token: 0x06002BEB RID: 11243 RVA: 0x00127F2F File Offset: 0x0012612F
		protected override void ManagedOnEnable()
		{
			EventManager.OnUpdateMap += this.HandleUpdateMap;
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.UpdateLocalizedFields();
		}

		// Token: 0x06002BEC RID: 11244 RVA: 0x00127F59 File Offset: 0x00126159
		protected override void ManagedOnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
			EventManager.OnUpdateMap -= this.HandleUpdateMap;
		}

		// Token: 0x06002BED RID: 11245 RVA: 0x00127F80 File Offset: 0x00126180
		protected internal override void ManagedLateUpdate()
		{
			if (UpdateManager.frameCount % 60 != 0)
			{
				return;
			}
			bool playerClose = false;
			if (Player.local != null)
			{
				this.playerDistanceToBoard = Vector3.Distance(base.transform.position, Player.local.transform.position);
				playerClose = (this.playerDistanceToBoard <= this.playerUpdateDistance);
			}
			if (Spectator.local != null)
			{
				float spectatorDistanceToBoard = Vector3.Distance(base.transform.position, Spectator.local.transform.position);
				playerClose = (playerClose || spectatorDistanceToBoard <= this.playerUpdateDistance);
			}
			if (playerClose && this.playerTimeInBoardRadius == 0f)
			{
				this.playerTimeInBoardRadius = Time.time + this.updateTime;
			}
			if (this.wantsToUpdate && Time.time >= this.playerTimeInBoardRadius && (playerClose || this.<ManagedLateUpdate>g__IsVisibleToCamera|42_0()))
			{
				this.RefreshBoard();
			}
		}

		// Token: 0x06002BEE RID: 11246 RVA: 0x00128066 File Offset: 0x00126266
		private void Init(bool tryUseLastAttributeCache = false)
		{
			if (this._initCoroutine != null)
			{
				base.StopCoroutine(this._initCoroutine);
				this._initCoroutine = null;
			}
			this.Clear();
			this._initCoroutine = base.StartCoroutine(this.InitCoroutine(tryUseLastAttributeCache));
		}

		// Token: 0x06002BEF RID: 11247 RVA: 0x0012809C File Offset: 0x0012629C
		private IEnumerator InitCoroutine(bool tryUseLastAttributeCache = false)
		{
			this.validLevelInstancesList = new List<LevelInstance>();
			this.levelInstanceLocationLocked = new List<LevelInstance>();
			this.levelInstanceLocationNotLocked = new List<LevelInstance>();
			LevelInstancesModule levelInstancesModule;
			if (GameModeManager.instance.currentGameMode == null || !GameModeManager.instance.currentGameMode.TryGetModule<LevelInstancesModule>(out levelInstancesModule))
			{
				Debug.Log("Current game mode does not have a valid LevelInstancesModule. Reverting to default");
				levelInstancesModule = new LevelInstancesModule();
			}
			string worldMapInfoPrefabAddress = levelInstancesModule.GetWorldMapInfoPrefabAddress();
			if (!string.IsNullOrEmpty(worldMapInfoPrefabAddress))
			{
				yield return Catalog.LoadAssetCoroutine<GameObject>(worldMapInfoPrefabAddress, new Action<GameObject>(this.OnWorldMapInfoOrefabLoaded), "UIWorldMapBoard");
			}
			if (this.attributeTexture == null)
			{
				this.attributeTexture = new Dictionary<string, Sprite>();
			}
			List<LevelInstance> allLevelInstances = levelInstancesModule.LevelInstances;
			foreach (LevelInstance levelInstance in allLevelInstances)
			{
				LevelData levelData2 = levelInstance.LevelData;
				if (!(levelData2.id == "Master"))
				{
					if (levelData2.sceneLocation == null)
					{
						Debug.LogError("WorldmapBoard - Could not find scene for level: " + levelData2.id + ", unable to put a location on the map");
					}
					else if (levelData2.worldmapPrefabLocation == null)
					{
						Debug.LogError("Could not load world map for level: " + levelData2.id + ", unable to put a location on the map");
					}
					else if ((!levelData2.showOnlyDevMode || GameManager.DevMode) && levelData2.showOnMap && (!levelData2.hideOnAndroid || Common.GetQualityLevel(false) != QualityLevel.Android))
					{
						levelData2.PreloadMapData();
						while (!levelData2.isMapDataPreloaded())
						{
							yield return null;
						}
						LevelInstanceData instanceData = levelInstance.instanceData;
						List<UIAttributeData> attributes = (instanceData != null) ? instanceData.GetAttributes() : null;
						if (attributes != null && attributes.Count > 0)
						{
							int attributeCount = attributes.Count;
							for (int i = 0; i < attributeCount; i++)
							{
								UIAttributeData data = attributes[i];
								string backgroundAddress = data.backgroundAddress;
								if (!string.IsNullOrEmpty(backgroundAddress) && this.attributeTexture.TryAdd(backgroundAddress, null))
								{
									Catalog.LoadAssetAsync<Sprite>(backgroundAddress, delegate(Sprite icon)
									{
										if (icon == null)
										{
											this.attributeTexture.Remove(backgroundAddress);
											return;
										}
										this.attributeTexture[backgroundAddress] = icon;
									}, "World Map Init");
								}
								List<ValueTuple<string, Color>> iconAdresses = data.iconList;
								if (iconAdresses == null)
								{
									iconAdresses = new List<ValueTuple<string, Color>>();
									iconAdresses.Add(new ValueTuple<string, Color>(data.iconAddressId, Color.white));
								}
								int count = iconAdresses.Count;
								for (int indexAddress = 0; indexAddress < count; indexAddress++)
								{
									string iconAddress = iconAdresses[indexAddress].Item1;
									if (!string.IsNullOrEmpty(iconAddress) && this.attributeTexture.TryAdd(iconAddress, null))
									{
										Catalog.LoadAssetAsync<Sprite>(iconAddress, delegate(Sprite icon)
										{
											if (icon == null)
											{
												this.attributeTexture.Remove(iconAddress);
												return;
											}
											if (!this.attributeTexture.IsNullOrEmpty())
											{
												this.attributeTexture[iconAddress] = icon;
												return;
											}
											Debug.LogError("Attribute textures dictionary is null or empty, cannot set icon for icon address " + iconAddress);
										}, "World Map Init");
									}
								}
								List<ValueTuple<string, Color>> bgIconAdresses = data.iconList;
								if (bgIconAdresses == null)
								{
									bgIconAdresses = new List<ValueTuple<string, Color>>();
									bgIconAdresses.Add(new ValueTuple<string, Color>(data.iconAddressId, Color.white));
								}
								count = bgIconAdresses.Count;
								for (int indexAdress = 0; indexAdress < count; indexAdress++)
								{
									string iconBgProgressionAddressId = bgIconAdresses[indexAdress].Item1;
									if (!string.IsNullOrEmpty(iconBgProgressionAddressId) && this.attributeTexture.TryAdd(iconBgProgressionAddressId, null))
									{
										Catalog.LoadAssetAsync<Sprite>(iconBgProgressionAddressId, delegate(Sprite icon)
										{
											if (icon == null)
											{
												this.attributeTexture.Remove(iconBgProgressionAddressId);
												return;
											}
											this.attributeTexture[iconBgProgressionAddressId] = icon;
										}, "World Map Init");
									}
								}
							}
						}
						this.validLevelInstancesList.Add(levelInstance);
						if (levelInstance.isMapLocationLocked)
						{
							this.levelInstanceLocationLocked.Add(levelInstance);
						}
						else
						{
							this.levelInstanceLocationNotLocked.Add(levelInstance);
						}
						levelData2 = null;
						levelInstance = null;
					}
				}
			}
			List<LevelInstance>.Enumerator enumerator = default(List<LevelInstance>.Enumerator);
			int mapCount = 0;
			using (List<LevelInstance>.Enumerator enumerator2 = this.validLevelInstancesList.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					LevelInstance levelInstance2 = enumerator2.Current;
					LevelData levelData = levelInstance2.LevelData;
					UIWorldMap worldMap = this.worldMaps.Find((UIWorldMap w) => w.worldMapHash == levelData.worldmapHash);
					if (!worldMap)
					{
						GameObject worldMapGo = UnityEngine.Object.Instantiate<GameObject>(levelData.worldmapPrefab, this.worldMapAnchor);
						worldMap = worldMapGo.GetComponent<UIWorldMap>();
						if (!worldMap)
						{
							Debug.LogError("World map prefab don't have any WorldMap component for level: " + levelData.id);
							UnityEngine.Object.Destroy(worldMapGo);
						}
						else
						{
							worldMap.worldMapHash = levelData.worldmapHash;
							if (!string.IsNullOrEmpty(levelData.worldmapLabel))
							{
								worldMap.label = levelData.worldmapLabel;
							}
							worldMap.texture = levelData.worldmapTexture2D;
							worldMap.worldMapBoard = this;
							worldMap.SetUpOverlapLocation();
							this.worldMaps.Add(worldMap);
							int num = mapCount;
							mapCount = num + 1;
						}
					}
				}
				goto IL_602;
			}
			IL_5E6:
			yield return null;
			IL_602:
			if (this.IsAllAttributeIconLoaded())
			{
				if (this.worldMapCustomInfosPrefab)
				{
					for (int indexMap = 0; indexMap < mapCount; indexMap++)
					{
						GameObject customInfos = UnityEngine.Object.Instantiate<GameObject>(this.worldMapCustomInfosPrefab, this.worldMapCustomInfosAnchor);
						customInfos.GetComponent<MapInfosDisplay>().Init(levelInstancesModule);
						this.worldMapsCustomInfos.Add(customInfos);
					}
				}
				foreach (UIWorldMap worldMap2 in this.worldMaps)
				{
					int count2 = this.levelInstanceLocationLocked.Count;
					for (int j = 0; j < count2; j++)
					{
						LevelInstance levelInstance3 = this.levelInstanceLocationLocked[j];
						if (levelInstance3.LevelData.worldmapHash == worldMap2.worldMapHash)
						{
							worldMap2.AddLocation(levelInstance3, this.attributeTexture, 1);
						}
					}
					for (int k = 0; k < this.levelInstanceLocationNotLocked.Count; k++)
					{
						LevelInstance levelInstance4 = this.levelInstanceLocationNotLocked[k];
						if (levelInstance4.LevelData.worldmapHash == worldMap2.worldMapHash)
						{
							int indexLocation = worldMap2.AddLocation(levelInstance4, this.attributeTexture, levelInstance4.mapLocationRandomNearest);
							if (indexLocation >= 0)
							{
								levelInstance4.mapLocationIndex = indexLocation;
								levelInstance4.isMapLocationLocked = true;
								this.levelInstanceLocationLocked.Add(levelInstance4);
								this.levelInstanceLocationNotLocked.RemoveAt(k);
								k--;
							}
						}
					}
					worldMap2.Show(false);
				}
				this.worldMapCustomInfosAnchor.parent.gameObject.SetActive(false);
				this.mapSelectorPrevious.SetActive(this.worldMaps.Count > 1);
				this.mapSelectorNext.SetActive(this.worldMaps.Count > 1);
				for (int l = 0; l < this.worldMaps.Count; l++)
				{
					if (this.worldMaps[l].isDefault)
					{
						this.worldMapIndex = l;
						IL_823:
						while (!Player.local)
						{
							yield return Yielders.ForSeconds(1f);
						}
						this.SetPageNone();
						PointerInputModule.SetUICameraToAllCanvas();
						CrystalHuntLevelInstancesModule levelInstanceModule;
						if (GameModeManager.instance.currentGameMode.TryGetModule<CrystalHuntLevelInstancesModule>(out levelInstanceModule))
						{
							levelInstanceModule.needsToUpdateMap = false;
						}
						if (this.worldMap == null)
						{
							this.worldMap = base.GetComponentInChildren<UIWorldMap>();
						}
						this._initCoroutine = null;
						this.isInitialized = true;
						yield break;
					}
				}
				goto IL_823;
			}
			goto IL_5E6;
			yield break;
		}

		// Token: 0x06002BF0 RID: 11248 RVA: 0x001280AB File Offset: 0x001262AB
		private void OnWorldMapInfoOrefabLoaded(GameObject obj)
		{
			if (obj != null)
			{
				this.worldMapCustomInfosPrefab = obj;
			}
		}

		// Token: 0x06002BF1 RID: 11249 RVA: 0x001280C0 File Offset: 0x001262C0
		private bool IsAllAttributeIconLoaded()
		{
			if (this.attributeTexture == null)
			{
				return false;
			}
			foreach (KeyValuePair<string, Sprite> icon in this.attributeTexture)
			{
				if (icon.Value == null)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002BF2 RID: 11250 RVA: 0x0012812C File Offset: 0x0012632C
		private void Clear()
		{
			if (this.levelInstanceLocationLocked != null)
			{
				this.levelInstanceLocationLocked.Clear();
				this.levelInstanceLocationLocked = null;
			}
			if (this.levelInstanceLocationNotLocked != null)
			{
				this.levelInstanceLocationNotLocked.Clear();
				this.levelInstanceLocationNotLocked = null;
			}
			if (this.validLevelInstancesList != null)
			{
				foreach (LevelInstance levelInstance in this.validLevelInstancesList)
				{
					levelInstance.LevelData.ReleaseMapData();
				}
				this.validLevelInstancesList.Clear();
				this.validLevelInstancesList = null;
			}
			if (this.attributeTexture != null)
			{
				List<KeyValuePair<string, Sprite>> releaseTexture = new List<KeyValuePair<string, Sprite>>();
				foreach (KeyValuePair<string, Sprite> icon in this.attributeTexture)
				{
					if (icon.Value != null)
					{
						releaseTexture.Add(icon);
					}
				}
				int count = releaseTexture.Count;
				for (int i = 0; i < count; i++)
				{
					KeyValuePair<string, Sprite> icon2 = releaseTexture[i];
					this.attributeTexture[icon2.Key] = null;
					Catalog.ReleaseAsset<Sprite>(icon2.Value);
				}
				this.attributeTexture.Clear();
				this.attributeTexture = null;
			}
			if (this.worldMaps != null)
			{
				foreach (GameObject obj in this.worldMapsCustomInfos)
				{
					UnityEngine.Object.Destroy(obj);
				}
				foreach (UIWorldMap uiworldMap in this.worldMaps)
				{
					UnityEngine.Object.Destroy(uiworldMap.gameObject);
				}
			}
			if (this.worldMaps == null)
			{
				this.worldMaps = new List<UIWorldMap>();
			}
			this.worldMaps.Clear();
			if (this.worldMapsCustomInfos == null)
			{
				this.worldMapsCustomInfos = new List<GameObject>();
			}
			this.worldMapsCustomInfos.Clear();
			Catalog.ReleaseAsset<GameObject>(this.worldMapCustomInfosPrefab);
			this.worldMapCustomInfosPrefab = null;
			this.isInitialized = false;
		}

		// Token: 0x06002BF3 RID: 11251 RVA: 0x00128368 File Offset: 0x00126568
		private void OnDestroy()
		{
			if (this._initCoroutine != null)
			{
				base.StopCoroutine(this._initCoroutine);
				this._initCoroutine = null;
			}
			this.Clear();
		}

		// Token: 0x06002BF4 RID: 11252 RVA: 0x0012838C File Offset: 0x0012658C
		public void ToggleContentVisibility(bool isVisible)
		{
			this.worldMapSelectorCanvas.enabled = isVisible;
			if (!this.worldMaps.IsNullOrEmpty())
			{
				this.worldMaps[this.worldMapIndex].Show(isVisible);
			}
			this.levelDetails.enabled = isVisible;
			this.mapCollider.gameObject.SetActive(isVisible);
			this.levelCollider.gameObject.SetActive(isVisible);
		}

		// Token: 0x06002BF5 RID: 11253 RVA: 0x001283F7 File Offset: 0x001265F7
		public void SetLocationSelected(LevelInstance levelInstance)
		{
			if (this.selectedLevelInstance == levelInstance)
			{
				return;
			}
			this.selectedLevelInstance = levelInstance;
			this.currentLevelInstance = levelInstance;
			this.canvasDetails.gameObject.SetActive(true);
			this.levelPresentation.SetLocationSelected();
			EventManager.InvokeMapLevelSelected();
		}

		// Token: 0x06002BF6 RID: 11254 RVA: 0x00128432 File Offset: 0x00126632
		public void SetPageNone()
		{
			this.currentLevelInstance = null;
			this.canvasDetails.gameObject.SetActive(false);
		}

		// Token: 0x06002BF7 RID: 11255 RVA: 0x0012844C File Offset: 0x0012664C
		public void SetLocationHover(LevelInstance levelInstance)
		{
			this.currentLevelInstance = levelInstance;
			this.canvasDetails.gameObject.SetActive(true);
			this.levelPresentation.SetLocationSelected();
		}

		// Token: 0x06002BF8 RID: 11256 RVA: 0x00128471 File Offset: 0x00126671
		public void ResetLocationSelected()
		{
			if (this.selectedLevelInstance == null)
			{
				this.SetPageNone();
				return;
			}
			this.currentLevelInstance = this.selectedLevelInstance;
			this.canvasDetails.gameObject.SetActive(true);
			this.levelPresentation.SetLocationSelected();
		}

		// Token: 0x06002BF9 RID: 11257 RVA: 0x001284AA File Offset: 0x001266AA
		public UIWorldMap[] GetAllMaps()
		{
			List<UIWorldMap> list = this.worldMaps;
			return ((list != null) ? list.ToArray() : null) ?? Array.Empty<UIWorldMap>();
		}

		/// <summary>
		/// Switches to the next map page
		/// </summary>
		// Token: 0x06002BFA RID: 11258 RVA: 0x001284C8 File Offset: 0x001266C8
		public void NextWorldMap()
		{
			int worldMapIndex = this.worldMapIndex;
			this.worldMapIndex = worldMapIndex + 1;
		}

		/// <summary>
		/// Switches to the previous map page
		/// </summary>
		// Token: 0x06002BFB RID: 11259 RVA: 0x001284E8 File Offset: 0x001266E8
		public void PreviousWorldMap()
		{
			int worldMapIndex = this.worldMapIndex;
			this.worldMapIndex = worldMapIndex - 1;
		}

		/// <summary>
		/// Update the level option description text according to the option being pointed
		/// </summary>
		// Token: 0x06002BFC RID: 11260 RVA: 0x00128505 File Offset: 0x00126705
		public void SetLevelOptionDescription(string description)
		{
			this.levelOptionDescriptionText.text = description;
		}

		/// <summary>
		/// Change to the next location on the currently selected map page
		/// </summary>
		// Token: 0x06002BFD RID: 11261 RVA: 0x00128514 File Offset: 0x00126714
		public void SetNextLocation()
		{
			this.locationIndex++;
			if (this.locationIndex >= this.worldMaps[this.worldMapIndex].locations.Count)
			{
				this.locationIndex = 0;
			}
			this.worldMaps[this.worldMapIndex].locations[this.locationIndex].button.onPointerClick.Invoke();
		}

		/// <summary>
		/// Change to the previous location on the currently selected map page
		/// </summary>
		// Token: 0x06002BFE RID: 11262 RVA: 0x0012858C File Offset: 0x0012678C
		public void SetPreviousLocation()
		{
			this.locationIndex--;
			if (this.locationIndex < 0)
			{
				this.locationIndex = this.worldMaps[this.worldMapIndex].locations.Count - 1;
			}
			this.worldMaps[this.worldMapIndex].locations[this.locationIndex].button.onPointerClick.Invoke();
		}

		// Token: 0x06002BFF RID: 11263 RVA: 0x00128603 File Offset: 0x00126803
		private void OnLanguageChanged(string language)
		{
			this.UpdateLocalizedFields();
		}

		// Token: 0x06002C00 RID: 11264 RVA: 0x0012860C File Offset: 0x0012680C
		private void UpdateLocalizedFields()
		{
			if (this.worldMaps != null && this.worldMapIndex >= 0 && this.worldMapIndex < this.worldMaps.Count)
			{
				this.worldMapLabel.text = (LocalizationManager.Instance.GetLocalizedString("Levels", this.worldMaps[this.worldMapIndex].label, false) ?? this.worldMaps[this.worldMapIndex].label);
				for (int i = 0; i < this.worldMaps[this.worldMapIndex].locations.Count; i++)
				{
					this.worldMaps[this.worldMapIndex].locations[i].UpdateLocalizedField();
				}
			}
			this.levelPresentation.UpdateLocalizedFields();
		}

		// Token: 0x06002C01 RID: 11265 RVA: 0x001286E4 File Offset: 0x001268E4
		private void HandleUpdateMap(EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			if (GameModeManager.instance.currentGameMode.refreshMapOnlyWhenPlayerNear && (Time.time < this.playerTimeInBoardRadius || this.playerDistanceToBoard > this.playerUpdateDistance || !base.transform.IsVisibleToCamera(Player.local.head.cam)))
			{
				this.wantsToUpdate = true;
				return;
			}
			this.RefreshBoard();
		}

		// Token: 0x06002C02 RID: 11266 RVA: 0x0012874C File Offset: 0x0012694C
		private void RefreshBoard()
		{
			this.wantsToUpdate = false;
			this.playerTimeInBoardRadius = 0f;
			this.Init(false);
		}

		// Token: 0x06002C05 RID: 11269 RVA: 0x00128798 File Offset: 0x00126998
		[CompilerGenerated]
		private bool <ManagedLateUpdate>g__IsVisibleToCamera|42_0()
		{
			bool isVisible = false;
			if (Player.local != null && Player.local.head != null)
			{
				isVisible = base.transform.IsVisibleToCamera(Player.local.head.cam);
			}
			if (Spectator.local != null)
			{
				isVisible = (isVisible || base.transform.IsVisibleToCamera(Spectator.local.cam));
			}
			return isVisible;
		}

		// Token: 0x0400295F RID: 10591
		public Canvas worldMapSelectorCanvas;

		// Token: 0x04002960 RID: 10592
		public CanvasCuller worldMapCanvasCuller;

		// Token: 0x04002961 RID: 10593
		public Transform worldMapAnchor;

		// Token: 0x04002962 RID: 10594
		public Transform worldMapCustomInfosAnchor;

		// Token: 0x04002963 RID: 10595
		public Canvas levelDetails;

		// Token: 0x04002964 RID: 10596
		public BoxCollider mapCollider;

		// Token: 0x04002965 RID: 10597
		public BoxCollider levelCollider;

		// Token: 0x04002966 RID: 10598
		public Transform canvasDetails;

		// Token: 0x04002967 RID: 10599
		public UIWorldMap worldMap;

		// Token: 0x04002968 RID: 10600
		[Header("World map Icon")]
		public TextMeshProUGUI worldMapLabel;

		// Token: 0x04002969 RID: 10601
		public UIWorldMapLocation mapCardLocationPrefab;

		// Token: 0x0400296A RID: 10602
		public MeshRenderer mapRenderer;

		// Token: 0x0400296B RID: 10603
		[Space]
		public UIWorldMapLevelPresentation levelPresentation;

		// Token: 0x0400296C RID: 10604
		[Space]
		public UIText levelOptionDescriptionText;

		// Token: 0x0400296D RID: 10605
		[Space]
		public GameObject mapSelectorPrevious;

		// Token: 0x0400296E RID: 10606
		public GameObject mapSelectorNext;

		// Token: 0x0400296F RID: 10607
		private float playerUpdateDistance = 4f;

		// Token: 0x04002970 RID: 10608
		private float playerDistanceToBoard;

		// Token: 0x04002971 RID: 10609
		private float playerTimeInBoardRadius;

		// Token: 0x04002972 RID: 10610
		private float updateTime = 1.5f;

		// Token: 0x04002973 RID: 10611
		private bool wantsToUpdate;

		// Token: 0x04002974 RID: 10612
		private List<UIWorldMap> worldMaps;

		// Token: 0x04002975 RID: 10613
		private List<GameObject> worldMapsCustomInfos;

		// Token: 0x04002976 RID: 10614
		private GameObject worldMapCustomInfosPrefab;

		// Token: 0x04002977 RID: 10615
		private int _worldMapIndex;

		// Token: 0x04002978 RID: 10616
		private int locationIndex;

		// Token: 0x04002979 RID: 10617
		private List<LevelInstance> validLevelInstancesList;

		// Token: 0x0400297A RID: 10618
		private List<LevelInstance> levelInstanceLocationLocked;

		// Token: 0x0400297B RID: 10619
		private List<LevelInstance> levelInstanceLocationNotLocked;

		// Token: 0x0400297C RID: 10620
		[NonSerialized]
		public LevelInstance currentLevelInstance;

		// Token: 0x0400297D RID: 10621
		[NonSerialized]
		public LevelInstance selectedLevelInstance;

		// Token: 0x0400297E RID: 10622
		[NonSerialized]
		private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

		// Token: 0x0400297F RID: 10623
		private Coroutine _initCoroutine;

		// Token: 0x04002980 RID: 10624
		[NonSerialized]
		public bool isInitialized;

		// Token: 0x04002981 RID: 10625
		[NonSerialized]
		public Dictionary<string, Sprite> attributeTexture;
	}
}
