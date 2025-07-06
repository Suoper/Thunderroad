using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.ResourceProviders;

namespace ThunderRoad
{
	// Token: 0x020001D5 RID: 469
	[Serializable]
	public class LevelData : CatalogData
	{
		// Token: 0x17000154 RID: 340
		// (get) Token: 0x0600151D RID: 5405 RVA: 0x000939EC File Offset: 0x00091BEC
		[JsonIgnore]
		public ItemData throwableData
		{
			get
			{
				if (this.throwableItem != null)
				{
					return this.throwableItem;
				}
				if (this.throwableTable != null)
				{
					return this.throwableTable.PickOne(0, 0, null);
				}
				if (!string.IsNullOrEmpty(this.improvisedThrowableID))
				{
					LevelData.ThrowableReference throwableReference = this.throwableRefType;
					if (throwableReference != LevelData.ThrowableReference.Item)
					{
						if (throwableReference == LevelData.ThrowableReference.Table)
						{
							this.throwableTable = Catalog.GetData<LootTableBase>(this.improvisedThrowableID, true);
						}
					}
					else
					{
						this.throwableItem = Catalog.GetData<ItemData>(this.improvisedThrowableID, true);
					}
					return this.throwableData;
				}
				if (Level.master.data != this)
				{
					return Level.master.data.throwableData;
				}
				Debug.LogError("Could not find any improvised throwable data: Level_Master.json may have data overrides which clear improvised throwable by mistake.");
				return null;
			}
		}

		// Token: 0x0600151E RID: 5406 RVA: 0x00093A91 File Offset: 0x00091C91
		public override int GetCurrentVersion()
		{
			return 3;
		}

		/// <summary>
		/// Returns true if this levelData has any mode which is allowed in the specified gameMode
		/// </summary>
		/// <param name="gameModeData"></param>
		/// <returns></returns>
		// Token: 0x0600151F RID: 5407 RVA: 0x00093A94 File Offset: 0x00091C94
		public bool IsLevelDataAllowedInGameMode(GameModeData gameModeData)
		{
			List<LevelData.Mode> levelModes = this.modes;
			int modesCount = levelModes.Count;
			for (int i = 0; i < modesCount; i++)
			{
				if (levelModes[i].allowGameModes.Contains(gameModeData.id, StringComparer.OrdinalIgnoreCase))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001520 RID: 5408 RVA: 0x00093ADC File Offset: 0x00091CDC
		public bool TryGetMode(string modeName, out LevelData.Mode mode)
		{
			mode = null;
			if (string.IsNullOrEmpty(modeName))
			{
				return false;
			}
			mode = this.GetMode(modeName);
			return mode != null && mode.name.Equals(modeName, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001521 RID: 5409 RVA: 0x00093B08 File Offset: 0x00091D08
		public bool HasMode(string modeName)
		{
			LevelData.Mode mode = this.GetMode(modeName);
			return mode != null && mode.name.Equals(modeName, StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// get all LevelModes which are valid for the current game mode 
		/// </summary>
		/// <returns></returns>
		// Token: 0x06001522 RID: 5410 RVA: 0x00093B30 File Offset: 0x00091D30
		public List<LevelData.Mode> GetGameModeAllowedModes()
		{
			List<LevelData.Mode> allowedModes = new List<LevelData.Mode>();
			string currentGameMode = null;
			if (GameModeManager.instance != null && GameModeManager.instance.currentGameMode != null)
			{
				currentGameMode = GameModeManager.instance.currentGameMode.id;
			}
			List<LevelData.Mode> levelModes = this.modes;
			int modesCount = levelModes.Count;
			for (int i = 0; i < modesCount; i++)
			{
				LevelData.Mode mode = levelModes[i];
				if (mode.allowGameModes == null || mode.allowGameModes.Contains(currentGameMode))
				{
					allowedModes.Add(mode);
				}
			}
			return allowedModes;
		}

		// Token: 0x06001523 RID: 5411 RVA: 0x00093BB8 File Offset: 0x00091DB8
		public LevelData.Mode GetMode(string modeName = null)
		{
			LevelData.Mode levelMode = null;
			string currentGameMode = null;
			if (GameModeManager.instance != null && GameModeManager.instance.currentGameMode != null)
			{
				currentGameMode = GameModeManager.instance.currentGameMode.id;
			}
			List<LevelData.Mode> levelModes = this.modes;
			int modesCount = levelModes.Count;
			for (int i = 0; i < modesCount; i++)
			{
				LevelData.Mode mode = levelModes[i];
				if (modeName != null && mode.name.Equals(modeName, StringComparison.OrdinalIgnoreCase))
				{
					if (string.IsNullOrEmpty(currentGameMode))
					{
						levelMode = mode;
						break;
					}
					if (mode.allowGameModes != null && mode.allowGameModes.Contains(currentGameMode))
					{
						levelMode = mode;
						break;
					}
				}
			}
			if (levelMode == null && levelModes != null && levelModes.Count > 0)
			{
				levelMode = levelModes[0];
				string modeNameStr = string.IsNullOrEmpty(modeName) ? "Null" : modeName;
				string gameModeStr = string.IsNullOrEmpty(currentGameMode) ? "Null" : currentGameMode;
				if (!string.IsNullOrEmpty(modeName))
				{
					Debug.LogError(string.Format("Could not find a level mode {0} on gameMode {1}, fallback to {2}", modeNameStr, gameModeStr, levelMode));
				}
			}
			return levelMode;
		}

		// Token: 0x06001524 RID: 5412 RVA: 0x00093CB0 File Offset: 0x00091EB0
		public bool DoesIDMatchThrowableData(string id)
		{
			if (this.improvisedThrowableID == null && this != Level.master.data)
			{
				return Level.master.data.DoesIDMatchThrowableData(id);
			}
			if (this.throwableRefType == LevelData.ThrowableReference.Item)
			{
				return id == this.improvisedThrowableID;
			}
			if (this.throwableTable == null)
			{
				this.throwableTable = Catalog.GetData<LootTableBase>(this.improvisedThrowableID, true);
			}
			return this.throwableTable.DoesLootTableContainItemID(id, -1, 0);
		}

		// Token: 0x06001525 RID: 5413 RVA: 0x00093D20 File Offset: 0x00091F20
		public override IEnumerator OnCatalogRefreshCoroutine()
		{
			if (this.modes == null)
			{
				this.modes = new List<LevelData.Mode>();
			}
			if (this.modes.Count == 0)
			{
				this.modes.Add(new LevelData.Mode());
			}
			yield return Catalog.LoadLocationCoroutine<SceneInstance>(this.sceneAddress, delegate(IResourceLocation value)
			{
				this.sceneLocation = value;
			}, this.id);
			yield return Catalog.LoadLocationCoroutine<GameObject>(this.worldmapPrefabAddress, delegate(IResourceLocation value)
			{
				this.worldmapPrefabLocation = value;
			}, this.id);
			yield return Catalog.LoadLocationCoroutine<Texture2D>(this.worldmapTextureAddress, delegate(IResourceLocation value)
			{
				this.worldmapTextureLocation = value;
			}, this.id);
			yield return Catalog.LoadLocationCoroutine<Sprite>(this.mapLocationIconAddress, delegate(IResourceLocation value)
			{
				this.mapLocationIconLocation = value;
			}, this.id);
			yield return Catalog.LoadLocationCoroutine<Sprite>(this.mapLocationIconHoverAddress, delegate(IResourceLocation value)
			{
				this.mapLocationIconHoverLocation = value;
			}, this.id);
			yield return Catalog.LoadLocationCoroutine<Sprite>(this.mapPreviewImageAddress, delegate(IResourceLocation value)
			{
				this.mapPreviewImageLocation = value;
			}, this.id);
			yield return Catalog.LoadLocationCoroutine<AudioContainer>(this.worldMapTravelAudioContainerAddress, delegate(IResourceLocation value)
			{
				this.worldMapTravelAudioContainerLocation = value;
			}, this.id);
			this.worldmapHash = this.CalculateWorldMapHash();
			yield break;
		}

		/// <summary>
		/// The worldmap hash is what determines the uniqueness of a worldmap, it could be a combination of different properties.
		/// This allows us and modders to reuse the same worldmap prefab with a different texture for example.
		/// </summary>
		// Token: 0x06001526 RID: 5414 RVA: 0x00093D2F File Offset: 0x00091F2F
		public virtual int CalculateWorldMapHash()
		{
			return Animator.StringToHash(this.worldmapPrefabAddress + this.worldmapTextureAddress + this.worldmapLabel);
		}

		// Token: 0x06001527 RID: 5415 RVA: 0x00093D50 File Offset: 0x00091F50
		public void PreloadMapData()
		{
			if (this.worldmapPrefabLocation != null)
			{
				Catalog.LoadAssetAsync<GameObject>(this.worldmapPrefabLocation, delegate(GameObject value)
				{
					this.worldmapPrefab = value;
				}, "worldmap");
			}
			if (this.worldmapTextureLocation != null)
			{
				Catalog.LoadAssetAsync<Texture2D>(this.worldmapTextureLocation, delegate(Texture2D value)
				{
					this.worldmapTexture2D = value;
				}, "worldmapTexture");
			}
			if (this.mapPreviewImageLocation != null)
			{
				Catalog.LoadAssetAsync<Sprite>(this.mapPreviewImageLocation, delegate(Sprite value)
				{
					this.mapPreviewImage = value;
				}, "mapPreviewImage");
			}
			if (this.mapLocationIconLocation != null)
			{
				Catalog.LoadAssetAsync<Sprite>(this.mapLocationIconLocation, delegate(Sprite value)
				{
					this.mapLocationIcon = value;
				}, "mapIcon");
			}
			if (this.mapLocationIconHoverLocation != null)
			{
				Catalog.LoadAssetAsync<Sprite>(this.mapLocationIconHoverLocation, delegate(Sprite value)
				{
					this.mapLocationIconHover = value;
				}, "mapIconHover");
			}
		}

		// Token: 0x06001528 RID: 5416 RVA: 0x00093E14 File Offset: 0x00092014
		public bool isMapDataPreloaded()
		{
			return (this.worldmapPrefabLocation == null || !(this.worldmapPrefab == null)) && (this.mapPreviewImageLocation == null || !(this.mapPreviewImage == null)) && (this.mapLocationIconLocation == null || !(this.mapLocationIcon == null)) && (this.mapLocationIconHoverLocation == null || !(this.mapLocationIconHover == null));
		}

		// Token: 0x06001529 RID: 5417 RVA: 0x00093E82 File Offset: 0x00092082
		public void ReleaseMapData()
		{
			Catalog.ReleaseAsset<GameObject>(this.worldmapPrefab);
			Catalog.ReleaseAsset<Texture2D>(this.worldmapTexture2D);
			Catalog.ReleaseAsset<Sprite>(this.mapPreviewImage);
			Catalog.ReleaseAsset<Sprite>(this.mapLocationIcon);
			Catalog.ReleaseAsset<Sprite>(this.mapLocationIconHover);
		}

		// Token: 0x0400150D RID: 5389
		public string name;

		// Token: 0x0400150E RID: 5390
		[TextArea]
		public string description;

		// Token: 0x0400150F RID: 5391
		public string descriptionLocalizationId;

		// Token: 0x04001510 RID: 5392
		public string sceneAddress;

		// Token: 0x04001511 RID: 5393
		[NonSerialized]
		public IResourceLocation sceneLocation;

		// Token: 0x04001512 RID: 5394
		public bool showOnlyDevMode;

		// Token: 0x04001513 RID: 5395
		public bool showInLevelSelection = true;

		// Token: 0x04001514 RID: 5396
		public string worldmapPrefabAddress = "Bas.Image.Map.Default";

		// Token: 0x04001515 RID: 5397
		public string worldmapTextureAddress = "Bas.Worldmap.Eraden";

		// Token: 0x04001516 RID: 5398
		public string worldmapLabel;

		// Token: 0x04001517 RID: 5399
		[Tooltip("The audio container that will play when the player clicks to travel to this location from the worldmap")]
		public string worldMapTravelAudioContainerAddress = "Bas.AudioGroup.UI.LightClick";

		// Token: 0x04001518 RID: 5400
		[NonSerialized]
		public IResourceLocation worldMapTravelAudioContainerLocation;

		// Token: 0x04001519 RID: 5401
		[NonSerialized]
		public IResourceLocation worldmapPrefabLocation;

		// Token: 0x0400151A RID: 5402
		[NonSerialized]
		public GameObject worldmapPrefab;

		// Token: 0x0400151B RID: 5403
		[NonSerialized]
		public IResourceLocation worldmapTextureLocation;

		// Token: 0x0400151C RID: 5404
		[NonSerialized]
		public Texture2D worldmapTexture2D;

		// Token: 0x0400151D RID: 5405
		[NonSerialized]
		public int worldmapHash;

		// Token: 0x0400151E RID: 5406
		public int mapLocationIndex;

		// Token: 0x0400151F RID: 5407
		public bool showOnMap = true;

		// Token: 0x04001520 RID: 5408
		public bool hideOnAndroid;

		// Token: 0x04001521 RID: 5409
		public string mapLocationIconAddress = "Bas.Icon.Location.Default";

		// Token: 0x04001522 RID: 5410
		[NonSerialized]
		public IResourceLocation mapLocationIconLocation;

		// Token: 0x04001523 RID: 5411
		[NonSerialized]
		public Sprite mapLocationIcon;

		// Token: 0x04001524 RID: 5412
		public string mapLocationIconHoverAddress = "Bas.Icon.Location.DefaultHover";

		// Token: 0x04001525 RID: 5413
		[NonSerialized]
		public IResourceLocation mapLocationIconHoverLocation;

		// Token: 0x04001526 RID: 5414
		[NonSerialized]
		public Sprite mapLocationIconHover;

		// Token: 0x04001527 RID: 5415
		public string mapPreviewImageAddress;

		// Token: 0x04001528 RID: 5416
		[NonSerialized]
		public IResourceLocation mapPreviewImageLocation;

		// Token: 0x04001529 RID: 5417
		[NonSerialized]
		public Sprite mapPreviewImage;

		// Token: 0x0400152A RID: 5418
		public List<LevelData.Mode> modes;

		// Token: 0x0400152B RID: 5419
		public int modePickCountPerRank = 2;

		// Token: 0x0400152C RID: 5420
		public List<LevelData.CustomCameras> customCameras;

		// Token: 0x0400152D RID: 5421
		public LevelData.ThrowableReference throwableRefType;

		// Token: 0x0400152E RID: 5422
		public string improvisedThrowableID;

		// Token: 0x0400152F RID: 5423
		private ItemData throwableItem;

		// Token: 0x04001530 RID: 5424
		private LootTableBase throwableTable;

		// Token: 0x02000805 RID: 2053
		public enum ThrowableReference
		{
			// Token: 0x0400402B RID: 16427
			Item,
			// Token: 0x0400402C RID: 16428
			Table
		}

		// Token: 0x02000806 RID: 2054
		[Serializable]
		public struct CustomCameras
		{
			// Token: 0x0400402D RID: 16429
			public Vector3 position;

			// Token: 0x0400402E RID: 16430
			public Quaternion rotation;
		}

		// Token: 0x02000807 RID: 2055
		[Serializable]
		public class Mode
		{
			// Token: 0x06003EAA RID: 16042 RVA: 0x0018468A File Offset: 0x0018288A
			public List<ValueDropdownItem<string>> GetAllGameModeID()
			{
				return Catalog.GetDropdownAllID(Category.GameMode, "None");
			}

			// Token: 0x06003EAB RID: 16043 RVA: 0x00184698 File Offset: 0x00182898
			public bool HasOption(string optionName)
			{
				if (this.availableOptions.IsNullOrEmpty())
				{
					return false;
				}
				int availableOptionsCount = this.availableOptions.Count;
				for (int i = 0; i < availableOptionsCount; i++)
				{
					if (this.availableOptions[i].name.Equals(optionName, StringComparison.OrdinalIgnoreCase))
					{
						return true;
					}
				}
				return false;
			}

			// Token: 0x06003EAC RID: 16044 RVA: 0x001846EC File Offset: 0x001828EC
			public T GetModule<T>() where T : LevelModule
			{
				int modulesCount = this.modules.Count;
				for (int i = 0; i < modulesCount; i++)
				{
					LevelModule levelModule = this.modules[i];
					if (levelModule is T)
					{
						return levelModule as T;
					}
				}
				return default(T);
			}

			// Token: 0x06003EAD RID: 16045 RVA: 0x0018473C File Offset: 0x0018293C
			public LevelModule GetModule(Type type)
			{
				int modulesCount = this.modules.Count;
				for (int i = 0; i < modulesCount; i++)
				{
					LevelModule levelModule = this.modules[i];
					if (levelModule.GetType() == type)
					{
						return levelModule;
					}
				}
				return null;
			}

			// Token: 0x06003EAE RID: 16046 RVA: 0x0018477F File Offset: 0x0018297F
			public bool HasModule<T>() where T : LevelModule
			{
				return this.GetModule<T>() != null;
			}

			// Token: 0x06003EAF RID: 16047 RVA: 0x0018478F File Offset: 0x0018298F
			public bool HasModule(Type type)
			{
				return this.GetModule(type) != null;
			}

			// Token: 0x06003EB0 RID: 16048 RVA: 0x0018479B File Offset: 0x0018299B
			public bool TryGetModule(Type type, out LevelModule module)
			{
				module = this.GetModule(type);
				return module != null;
			}

			// Token: 0x06003EB1 RID: 16049 RVA: 0x001847AB File Offset: 0x001829AB
			public bool TryGetModule<T>(out T module) where T : LevelModule
			{
				module = this.GetModule<T>();
				return module != null;
			}

			// Token: 0x0400402F RID: 16431
			[JsonMergeKey]
			public string name = "Default";

			// Token: 0x04004030 RID: 16432
			public string displayName = "{Default}";

			// Token: 0x04004031 RID: 16433
			[TextArea]
			public string description = "{NoDescription}";

			// Token: 0x04004032 RID: 16434
			public List<string> allowGameModes;

			// Token: 0x04004033 RID: 16435
			public int mapOrder;

			// Token: 0x04004034 RID: 16436
			public LevelData.Mode.PlayerDeathAction playerDeathAction = LevelData.Mode.PlayerDeathAction.AskReload;

			// Token: 0x04004035 RID: 16437
			public List<LevelModule> modules = new List<LevelModule>();

			// Token: 0x04004036 RID: 16438
			public List<OptionBase> availableOptions = new List<OptionBase>();

			// Token: 0x02000BCF RID: 3023
			public enum PlayerDeathAction
			{
				// Token: 0x04004CE9 RID: 19689
				None,
				// Token: 0x04004CEA RID: 19690
				AskReload,
				// Token: 0x04004CEB RID: 19691
				ReloadLevel,
				// Token: 0x04004CEC RID: 19692
				LoadHome,
				// Token: 0x04004CED RID: 19693
				PermaDeath
			}
		}

		// Token: 0x02000808 RID: 2056
		public enum MusicType
		{
			// Token: 0x04004038 RID: 16440
			Background,
			// Token: 0x04004039 RID: 16441
			Combat
		}
	}
}
