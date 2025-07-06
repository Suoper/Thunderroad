using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.UnityConverters.Geometry;
using Newtonsoft.Json.UnityConverters.Math;
using Newtonsoft.Json.UnityConverters.Scripting;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace ThunderRoad
{
	// Token: 0x0200016C RID: 364
	public static class Catalog
	{
		/// <summary>
		/// Init the catalog on game start
		/// </summary>
		// Token: 0x060011FE RID: 4606 RVA: 0x00080BAC File Offset: 0x0007EDAC
		public static void Init()
		{
			EventManager.OnLanguageChanged += Catalog.OnLanguageChanged;
		}

		// Token: 0x060011FF RID: 4607 RVA: 0x00080BBF File Offset: 0x0007EDBF
		public static bool TryGetCategory<T>(out Category category)
		{
			category = Category.Custom;
			return Catalog.TryGetCategory(typeof(T), out category);
		}

		// Token: 0x06001200 RID: 4608 RVA: 0x00080BD8 File Offset: 0x0007EDD8
		public static bool TryGetCategory(Type type, out Category category)
		{
			if (Catalog.typeCategories.TryGetValue(type, out category))
			{
				return true;
			}
			if (!Catalog.IsSameOrSubclass(typeof(CatalogData), type))
			{
				Debug.LogError(string.Format("Type: {0} is not a subclass of CatalogData!", type));
				return false;
			}
			for (int i = 0; i < Catalog.baseTypeCategories.Length; i++)
			{
				ValueTuple<Type, Category> tuple = Catalog.baseTypeCategories[i];
				if (Catalog.IsSameOrSubclass(tuple.Item1, type))
				{
					Catalog.typeCategories.Add(type, tuple.Item2);
					category = tuple.Item2;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001201 RID: 4609 RVA: 0x00080C64 File Offset: 0x0007EE64
		public static Category GetCategory(Type type)
		{
			Category category;
			if (Catalog.typeCategories.TryGetValue(type, out category))
			{
				return category;
			}
			if (!Catalog.IsSameOrSubclass(typeof(CatalogData), type))
			{
				Debug.LogError(string.Format("Type: {0} is not a subclass of CatalogData!", type));
				return Category.Custom;
			}
			for (int i = 0; i < Catalog.baseTypeCategories.Length; i++)
			{
				ValueTuple<Type, Category> tuple = Catalog.baseTypeCategories[i];
				if (Catalog.IsSameOrSubclass(tuple.Item1, type))
				{
					Catalog.typeCategories.Add(type, tuple.Item2);
					return tuple.Item2;
				}
			}
			return Category.Custom;
		}

		// Token: 0x06001202 RID: 4610 RVA: 0x00080CED File Offset: 0x0007EEED
		public static bool IsSameOrSubclass(Type baseClass, Type subClass)
		{
			return subClass.IsSubclassOf(baseClass) || subClass == baseClass;
		}

		// Token: 0x06001203 RID: 4611 RVA: 0x00080D01 File Offset: 0x0007EF01
		public static JsonSerializer GetJsonNetSerializer()
		{
			JsonSerializer result;
			if ((result = Catalog.jsonSerializer) == null)
			{
				result = (Catalog.jsonSerializer = JsonSerializer.CreateDefault(Catalog.GetJsonNetSerializerSettings()));
			}
			return result;
		}

		// Token: 0x06001204 RID: 4612 RVA: 0x00080D1C File Offset: 0x0007EF1C
		public static JsonSerializerSettings GetJsonNetSerializerSettings()
		{
			if (Catalog.jsonSerializerSettings != null)
			{
				return Catalog.jsonSerializerSettings;
			}
			Catalog.jsonSerializerSettings = new JsonSerializerSettings();
			Catalog.jsonSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
			Catalog.jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
			Catalog.jsonSerializerSettings.Formatting = Formatting.Indented;
			Catalog.jsonSerializerSettings.MaxDepth = new int?(50);
			Catalog.jsonSerializerSettings.ContractResolver = JsonSerializer.CreateDefault().ContractResolver;
			Catalog.jsonSerializerSettings.Converters.Add(new StringEnumConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new Vector2Converter());
			Catalog.jsonSerializerSettings.Converters.Add(new Vector2IntConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new Vector3Converter());
			Catalog.jsonSerializerSettings.Converters.Add(new Vector3IntConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new Vector4Converter());
			Catalog.jsonSerializerSettings.Converters.Add(new QuaternionConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new ColorConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new LayerMaskConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new BoundsConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new BoundsIntConverter());
			Catalog.jsonSerializerSettings.Converters.Add(new KeyedListMergeConverter(Catalog.jsonSerializerSettings.ContractResolver));
			return Catalog.jsonSerializerSettings;
		}

		// Token: 0x06001205 RID: 4613 RVA: 0x00080E85 File Offset: 0x0007F085
		public static bool IsJsonLoaded()
		{
			return Catalog.gameData != null;
		}

		// Token: 0x06001206 RID: 4614 RVA: 0x00080E90 File Offset: 0x0007F090
		private static void LoadJsonDbFile(string zipPath, string folderName, ModManager.ModData modData = null)
		{
			string logTag = (modData != null) ? ("[ModManager][Catalog][" + folderName + "]") : ("[Catalog][" + folderName + "]");
			FileInfo fileInfo = new FileInfo(zipPath);
			string localPathFile = fileInfo.Directory.Name + "/" + fileInfo.Name;
			Debug.Log(logTag + " Loading file: " + localPathFile);
			ZipFile zip = null;
			try
			{
				zip = ZipFile.Read(zipPath);
			}
			catch (Exception ex)
			{
				Debug.LogError(logTag + " Error: Exception reading .jsondb file: " + localPathFile);
				if (modData != null)
				{
					modData.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.JSON, "Exception reading .jsondb file", "ModErrorExceptionReadingJSONDB", string.Empty, ex.Message, localPathFile));
					ModManager.loadedMods.Add(modData);
				}
				return;
			}
			if (zip == null)
			{
				Debug.LogError(logTag + " Error: Can't read .jsondb file: " + localPathFile);
				if (modData != null)
				{
					modData.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.JSON, "Can't read .jsondb file", "ModErrorCantReadJSONDB", string.Empty, "", localPathFile));
					ModManager.loadedMods.Add(modData);
				}
				return;
			}
			int zipCount = zip.Count;
			List<string> inputJsons = new List<string>(zipCount);
			List<string> inputJsonPaths = new List<string>(zipCount);
			for (int i = 0; i < zipCount; i++)
			{
				ZipEntry zipEntry = zip[i];
				if (!(Path.GetExtension(zipEntry.FileName) != ".json") && !zipEntry.FileName.Contains("catalog_", StringComparison.OrdinalIgnoreCase) && !zipEntry.FileName.Equals("manifest.json", StringComparison.OrdinalIgnoreCase))
				{
					string jsonText = new StreamReader(zipEntry.OpenReader()).ReadToEnd();
					string entryName = localPathFile + "/" + zipEntry.FileName;
					inputJsons.Add(jsonText);
					inputJsonPaths.Add(entryName);
				}
			}
			Catalog.LoadJsonLooseFiles(folderName, inputJsons, inputJsonPaths, modData);
			Debug.Log(logTag + "- Loaded file: " + localPathFile);
		}

		// Token: 0x06001207 RID: 4615 RVA: 0x00081090 File Offset: 0x0007F290
		private static void LoadJsonLooseFiles(string folderName, List<string> inputJsons, List<string> inputJsonPaths, ModManager.ModData modData = null)
		{
			string basePath = (modData != null) ? FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, "") : FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Default, "");
			basePath = Path.Combine(basePath, folderName);
			string logTag = (modData != null) ? ("[ModManager][Catalog][" + folderName + "]") : ("[Catalog][" + folderName + "]");
			object[] deserializedObjects = Catalog.DeserializeJsons(inputJsons, inputJsonPaths, modData);
			for (int i = 0; i < deserializedObjects.Length; i++)
			{
				object catalogObject = deserializedObjects[i];
				if (catalogObject != null && (catalogObject is CatalogData || catalogObject is GameData))
				{
					if (new FileInfo(inputJsonPaths[i]).Directory == null)
					{
						Debug.LogError(logTag + " Failed to load file: " + inputJsonPaths[i]);
					}
					else
					{
						string localPath = Path.GetRelativePath(basePath, inputJsonPaths[i]);
						if (!Catalog.LoadJson(catalogObject, inputJsons[i], inputJsonPaths[i], folderName, modData))
						{
							Debug.LogError(logTag + " Failed to load file: " + localPath);
						}
					}
				}
			}
		}

		// Token: 0x06001208 RID: 4616 RVA: 0x00081194 File Offset: 0x0007F394
		private static void ReadCatalogJsonFiles(FileManager.Source fileSource, string folder, out List<string> inputJsons, out List<string> inputJsonPaths)
		{
			string[] paths = FileManager.GetFullFilePaths(FileManager.Type.JSONCatalog, fileSource, folder, "*.json");
			inputJsons = new List<string>(paths.Length);
			inputJsonPaths = new List<string>(paths.Length);
			foreach (string path in paths)
			{
				if (!path.Contains("catalog_", StringComparison.OrdinalIgnoreCase) && !path.Contains("manifest.json", StringComparison.OrdinalIgnoreCase))
				{
					string json = File.ReadAllText(path);
					inputJsons.Add(json);
					inputJsonPaths.Add(path);
				}
			}
		}

		// Token: 0x06001209 RID: 4617 RVA: 0x00081208 File Offset: 0x0007F408
		public static bool LoadJson(object jsonObj, string jsonText, string jsonPath, string folder, ModManager.ModData modData = null)
		{
			string logTag = (modData != null) ? ("[ModManager][Catalog][" + folder + "]") : ("[Catalog][" + folder + "]");
			CatalogData catalogData = jsonObj as CatalogData;
			if (catalogData == null)
			{
				GameData newGameData = jsonObj as GameData;
				if (newGameData == null)
				{
					Debug.LogWarning(logTag + " Custom Json File or malformed CatalogData skipped: " + jsonPath);
					return false;
				}
				Catalog.LoadGameData(newGameData, jsonText, jsonPath, folder, modData);
				return true;
			}
			else
			{
				if (!Catalog.DoesCatalogVersionMatch(catalogData))
				{
					Debug.LogWarning(string.Format("{0} Version mismatch (file {1}, current {2}) ignoring file: {3}", new object[]
					{
						logTag,
						catalogData.version,
						catalogData.GetCurrentVersion(),
						jsonPath
					}));
					if (modData != null)
					{
						modData.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.JSON, "Catalog version incompatible", "ModErrorIncompatibleCatalogVersion", string.Empty, string.Format("[{0}] Version mismatch (file {1}, current {2}) ignoring file: {3}", new object[]
						{
							folder,
							catalogData.version,
							catalogData.GetCurrentVersion(),
							jsonPath
						}), jsonPath));
						ModManager.loadedMods.Add(modData);
					}
					return false;
				}
				return Catalog.LoadCatalogData(catalogData, jsonText, jsonPath, folder, modData);
			}
		}

		// Token: 0x0600120A RID: 4618 RVA: 0x00081340 File Offset: 0x0007F540
		public static void LoadGameData(GameData newGameData, string jsonText, string jsonPath, string folder, ModManager.ModData modData = null)
		{
			if (Catalog.gameData != null)
			{
				if (!ThunderRoadSettings.current.overrideData)
				{
					return;
				}
				JsonConvert.PopulateObject(jsonText, Catalog.gameData, Catalog.jsonSerializerSettings);
				if (modData != null)
				{
					string relativePath = jsonPath.Substring(jsonPath.IndexOf(folder, StringComparison.Ordinal));
					Debug.Log("[ModManager][Catalog][" + folder + "] Overriding: [GameData] with: " + relativePath);
				}
				if (Application.isEditor)
				{
					GameData gameData = Catalog.gameData;
					gameData.sourceFolders = gameData.sourceFolders + "+" + folder;
					return;
				}
			}
			else
			{
				Catalog.gameData = newGameData;
				if (Application.isEditor)
				{
					Catalog.gameData.sourceFolders = folder;
				}
			}
		}

		// Token: 0x0600120B RID: 4619 RVA: 0x000813DC File Offset: 0x0007F5DC
		public static bool LoadCatalogData(CatalogData catalogData, string jsonText, string jsonPath, string folder, ModManager.ModData modData = null)
		{
			string logTag = (modData != null) ? ("[ModManager][Catalog][" + folder + "]") : ("[Catalog][" + folder + "]");
			Category category;
			if (!Catalog.TryGetCategory(catalogData.GetType(), out category))
			{
				Debug.LogError(string.Format("{0} CatalogData: [{1}][{2}] JSON: {3} does not map to a valid category. Please subclass CustomData to extend catalog data types", new object[]
				{
					logTag,
					catalogData.GetType(),
					catalogData.id,
					jsonPath
				}));
				return false;
			}
			CatalogData existingData = Catalog.GetData(category, catalogData.id, false);
			if (existingData != null)
			{
				if (!ThunderRoadSettings.current.overrideData)
				{
					return false;
				}
				if (modData != null)
				{
					string relativePath = jsonPath.Substring(jsonPath.IndexOf(folder, StringComparison.Ordinal));
					Debug.Log(string.Format("{0} Overriding: [{1}][{2}][{3}] with: {4}", new object[]
					{
						logTag,
						category.ToString(),
						existingData.GetType(),
						existingData.id,
						relativePath
					}));
				}
				JsonConvert.PopulateObject(jsonText, existingData, Catalog.jsonSerializerSettings);
				if (Application.isEditor)
				{
					Debug.LogWarning(string.Concat(new string[]
					{
						"WARNING: Catalog Data ",
						existingData.id,
						" (",
						existingData.filePath,
						") is being overridden by file ",
						jsonPath,
						"!"
					}));
					CatalogData catalogData2 = existingData;
					catalogData2.sourceFolders = catalogData2.sourceFolders + "+" + folder;
				}
				if (modData != null)
				{
					existingData.changers.Add(modData);
					modData.changedDatas.Add(existingData);
				}
			}
			else
			{
				if (Application.isEditor)
				{
					catalogData.sourceFolders = folder;
					catalogData.filePath = jsonPath;
				}
				catalogData.Init();
				int categoryIndex = (int)category;
				CatalogCategory[] array = Catalog.data;
				int num = categoryIndex;
				if (array[num] == null)
				{
					array[num] = new CatalogCategory(category);
				}
				if (Catalog.data[categoryIndex].AddCatalogData(catalogData) && modData != null)
				{
					catalogData.owner = modData;
					modData.ownedDatas.Add(catalogData);
				}
			}
			return true;
		}

		// Token: 0x0600120C RID: 4620 RVA: 0x000815D4 File Offset: 0x0007F7D4
		public static object[] DeserializeJsons(List<string> jsons, List<string> jsonPaths, ModManager.ModData modData = null)
		{
			int jsonsCount = jsons.Count;
			object[] outputs = new object[jsonsCount];
			object lockObj = new object();
			Parallel.For(0, jsonsCount, delegate(int index)
			{
				object obj = null;
				try
				{
					obj = JsonConvert.DeserializeObject(jsons[index], Catalog.jsonSerializerSettings);
				}
				catch (Exception ex)
				{
					Debug.LogError("[Catalog] Error loading json: " + jsonPaths[index] + ". " + ex.Message);
					if (!false)
					{
						string[] array = new string[7];
						array[0] = "LoadJson : Cannot read json file ";
						array[1] = jsonPaths[index];
						array[2] = " (";
						array[3] = ex.Message;
						array[4] = ") \n (";
						int num = 5;
						Exception innerException = ex.InnerException;
						array[num] = ((innerException != null) ? innerException.Message : null);
						array[6] = ")";
						Debug.LogError(string.Concat(array));
					}
					if (modData != null)
					{
						object lockObj = lockObj;
						lock (lockObj)
						{
							HashSet<ModManager.ModData.Error> errors = modData.errors;
							ModManager.ModData.ErrorType type = ModManager.ModData.ErrorType.JSON;
							string description = "Cannot read json file: " + ex.Message;
							string descriptionLocalizationId = "ModErrorCantReadJSON";
							string message = ex.Message;
							Exception innerException2 = ex.InnerException;
							errors.Add(new ModManager.ModData.Error(type, description, descriptionLocalizationId, message, (innerException2 != null) ? innerException2.Message : null, jsonPaths[index]));
							ModManager.loadedMods.Add(modData);
						}
					}
				}
				outputs[index] = obj;
			});
			return outputs;
		}

		// Token: 0x0600120D RID: 4621 RVA: 0x0008163C File Offset: 0x0007F83C
		public static void SaveAllJson()
		{
			if (!Application.isPlaying)
			{
				Catalog.jsonSerializerSettings = Catalog.GetJsonNetSerializerSettings();
			}
			Category[] array = (Category[])Enum.GetValues(typeof(Category));
			for (int i = 0; i < array.Length; i++)
			{
				foreach (CatalogData catalogData in Catalog.GetDataList(array[i]))
				{
					Catalog.SaveToJson(catalogData);
				}
			}
			Catalog.SaveGameData();
			Debug.Log("All Json Saved");
		}

		// Token: 0x0600120E RID: 4622 RVA: 0x000816D4 File Offset: 0x0007F8D4
		public static void SaveGameData()
		{
			string fileName = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Default, Catalog.gameData.GetLatestOverrideFolder()) + "/Game.json";
			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}
			File.WriteAllText(fileName, JsonConvert.SerializeObject(Catalog.gameData, typeof(GameData), Catalog.GetJsonNetSerializerSettings()));
		}

		// Token: 0x0600120F RID: 4623 RVA: 0x0008172C File Offset: 0x0007F92C
		public static void SaveToJson(CatalogData catalogData)
		{
			catalogData.OnEditorSave();
			string fileName = "";
			if (!Application.isPlaying && File.Exists(catalogData.filePath))
			{
				fileName = catalogData.filePath;
				File.Delete(catalogData.filePath);
			}
			string savePath = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Default, catalogData.GetLatestOverrideFolder());
			if (!Directory.Exists(savePath))
			{
				Directory.CreateDirectory(savePath);
			}
			Category category;
			if (Catalog.TryGetCategory(catalogData.GetType(), out category))
			{
				string saveCategoryPath = string.Format("{0}/{1}s/", savePath, category);
				string type = catalogData.GetType().ToString();
				string[] array = type.Split('.', StringSplitOptions.None);
				type = array[array.Length - 1];
				if (category == Category.AreaCollection)
				{
					saveCategoryPath = savePath + "/" + type + "s/";
				}
				if (!Directory.Exists(saveCategoryPath))
				{
					Directory.CreateDirectory(saveCategoryPath);
				}
				if (fileName == "")
				{
					fileName = string.Format("{0}{1}_{2}.json", saveCategoryPath, category, catalogData.id);
					if (catalogData is ItemData)
					{
						fileName = string.Format("{0}/{1}s/{2}_{3}.json", new object[]
						{
							savePath,
							category,
							category,
							(catalogData as ItemData).type.ToString() + "_" + catalogData.id
						});
					}
				}
				File.WriteAllText(fileName, JsonConvert.SerializeObject(catalogData, typeof(CatalogData), Catalog.jsonSerializerSettings));
				return;
			}
			Debug.Log(string.Format("Cannot get valid category for CatalogData: [{0}][{1}]. Unable to save to json", catalogData.id, catalogData.GetType()));
		}

		// Token: 0x06001210 RID: 4624 RVA: 0x000818A8 File Offset: 0x0007FAA8
		public static void Clear()
		{
			if (Catalog.data == null)
			{
				Catalog.data = new CatalogCategory[Enum.GetNames(typeof(Category)).Length];
			}
			for (int i = 0; i < Catalog.data.Length; i++)
			{
				if (Catalog.data[i] == null)
				{
					Catalog.data[i] = new CatalogCategory((Category)i);
				}
				else
				{
					Catalog.data[i].Clear();
				}
			}
			Catalog.gameData = null;
			Catalog.jsonSerializerSettings = Catalog.GetJsonNetSerializerSettings();
		}

		// Token: 0x06001211 RID: 4625 RVA: 0x0008191D File Offset: 0x0007FB1D
		public static void EditorLoadAllJson(bool requiresRefresh = false, bool force = false, bool includeMods = true)
		{
		}

		// Token: 0x06001212 RID: 4626 RVA: 0x00081920 File Offset: 0x0007FB20
		public static void LoadDefaultCatalogs()
		{
			Catalog.Clear();
			Debug.Log("Loading default catalogs");
			Catalog.gameData = null;
			int defaultFolderCount = GameSettings.loadDefaultFolders.Count;
			for (int i = 0; i < defaultFolderCount; i++)
			{
				string defaultFolder = GameSettings.loadDefaultFolders[i];
				Debug.Log("Loading default catalog in folder: " + defaultFolder);
				string jsondbPath = FileManager.GetFullPath(FileManager.Type.AddressableAssets, FileManager.Source.Default, defaultFolder + ".jsondb");
				if (File.Exists(jsondbPath))
				{
					Catalog.LoadJsonDbFile(jsondbPath, defaultFolder, null);
				}
				Debug.Log("Loaded catalog in folder: " + defaultFolder);
			}
			Debug.Log("Finished loading default catalogs");
		}

		// Token: 0x06001213 RID: 4627 RVA: 0x000819B4 File Offset: 0x0007FBB4
		public static void LoadModCatalog(ModManager.ModData mod)
		{
			if (mod.Incompatible)
			{
				return;
			}
			string[] fullFilePaths = FileManager.GetFullFilePaths(FileManager.Type.JSONCatalog, FileManager.Source.Mods, mod.folderName, "*.jsondb");
			for (int i = 0; i < fullFilePaths.Length; i++)
			{
				Catalog.LoadJsonDbFile(fullFilePaths[i], mod.folderName, mod);
			}
			List<string> inputJsons;
			List<string> inputJsonPaths;
			Catalog.ReadCatalogJsonFiles(FileManager.Source.Mods, mod.folderName, out inputJsons, out inputJsonPaths);
			Catalog.LoadJsonLooseFiles(mod.folderName, inputJsons, inputJsonPaths, mod);
		}

		// Token: 0x06001214 RID: 4628 RVA: 0x00081A18 File Offset: 0x0007FC18
		public static bool DoesCatalogVersionMatch(CatalogData catalogData)
		{
			return !Catalog.checkFileVersion || catalogData.version == catalogData.GetCurrentVersion();
		}

		// Token: 0x06001215 RID: 4629 RVA: 0x00081A34 File Offset: 0x0007FC34
		public static void Refresh()
		{
			Catalog.gameData.OnCatalogRefresh();
			for (int i = 0; i < Catalog.data.Length; i++)
			{
				List<CatalogData> catalogDatas = Catalog.data[i].catalogDatas;
				if (catalogDatas != null)
				{
					for (int d = 0; d < catalogDatas.Count; d++)
					{
						catalogDatas[d].OnCatalogRefresh();
					}
				}
			}
		}

		// Token: 0x06001216 RID: 4630 RVA: 0x00081A8A File Offset: 0x0007FC8A
		public static IEnumerator RefreshCoroutine()
		{
			Catalog.RefreshingCatalog = true;
			float refreshTimer = Time.realtimeSinceStartup;
			Debug.Log("[Catalog] Refresh catalog...");
			EventManager.InvokeCatalogRefresh(EventTime.OnStart);
			LoadingCamera.SetLoadingStage(LoadingCamera.Stage.CatalogRefresh, false);
			yield return null;
			float gameDataTimer = Time.realtimeSinceStartup;
			Catalog.gameData.OnCatalogRefresh();
			yield return Catalog.gameData.OnCatalogRefreshCoroutine();
			Debug.Log(string.Format("[Catalog] Game data refreshed in {0:F2} sec", Time.realtimeSinceStartup - gameDataTimer));
			int totalCount = 0;
			List<IEnumerator> coroutines = new List<IEnumerator>();
			int num;
			for (int i = 0; i < Catalog.data.Length; i = num + 1)
			{
				float timer = Time.realtimeSinceStartup;
				Category category = (Category)i;
				yield return LoadingCamera.SetAdditionalLoadingTextInfoYield(string.Format(": {0}", category));
				List<CatalogData> catalogDatas = Catalog.data[i].catalogDatas;
				if (catalogDatas != null)
				{
					int catalogDatasCount = catalogDatas.Count;
					coroutines.Clear();
					for (int d = 0; d < catalogDatasCount; d++)
					{
						CatalogData catalogData = catalogDatas[d];
						catalogData.OnCatalogRefresh();
						if (!(GameManager.local == null))
						{
							IEnumerator catalogRefreshCoroutine = catalogData.OnCatalogRefreshCoroutine();
							if (catalogRefreshCoroutine != null)
							{
								coroutines.Add(catalogRefreshCoroutine);
								num = totalCount;
								totalCount = num + 1;
							}
						}
					}
					if (coroutines.Count > 0)
					{
						yield return coroutines.YieldParallel();
					}
					string categoryName = Enum.GetName(typeof(Category), i);
					Debug.Log(string.Format("[Catalog] {0} refreshed {1} entries in {2:F2} sec", categoryName, catalogDatasCount, Time.realtimeSinceStartup - timer));
				}
				num = i;
			}
			EventManager.InvokeCatalogRefresh(EventTime.OnEnd);
			Debug.Log(string.Format("[Catalog] Refreshed {0} entries in {1:F2} sec", totalCount, Time.realtimeSinceStartup - refreshTimer));
			Catalog.RefreshingCatalog = false;
			yield break;
		}

		// Token: 0x06001217 RID: 4631 RVA: 0x00081A92 File Offset: 0x0007FC92
		public static IEnumerator LoadAddressableAssetsCoroutine()
		{
			float refreshTimer = Time.realtimeSinceStartup;
			Debug.Log("[Catalog] Load Addressable Assets...");
			yield return null;
			float gameDataTimer = Time.realtimeSinceStartup;
			yield return Catalog.gameData.LoadAddressableAssetsCoroutine();
			Debug.Log(string.Format("[Catalog] Game data Addressable Assets loaded in {0:F2} sec", Time.realtimeSinceStartup - gameDataTimer));
			int totalCount = 0;
			List<IEnumerator> coroutines = new List<IEnumerator>();
			int num;
			for (int i = 0; i < Catalog.data.Length; i = num + 1)
			{
				float timer = Time.realtimeSinceStartup;
				Category category = (Category)i;
				yield return LoadingCamera.SetAdditionalLoadingTextInfoYield(string.Format(": {0}", category));
				List<CatalogData> catalogDatas = Catalog.data[i].catalogDatas;
				if (catalogDatas != null)
				{
					int catalogDatasCount = catalogDatas.Count;
					coroutines.Clear();
					for (int d = 0; d < catalogDatasCount; d++)
					{
						CatalogData catalogData = catalogDatas[d];
						catalogData.OnCatalogRefresh();
						if (!(GameManager.local == null))
						{
							IEnumerator loadAddressableAssetsCoroutine = catalogData.LoadAddressableAssetsCoroutine();
							if (loadAddressableAssetsCoroutine != null)
							{
								coroutines.Add(loadAddressableAssetsCoroutine);
								num = totalCount;
								totalCount = num + 1;
							}
						}
					}
					if (coroutines.Count > 0)
					{
						yield return coroutines.YieldParallel();
					}
					string categoryName = Enum.GetName(typeof(Category), i);
					Debug.Log(string.Format("[Catalog] {0} loaded addressable assets for {1} entries in {2:F2} sec", categoryName, catalogDatasCount, Time.realtimeSinceStartup - timer));
				}
				num = i;
			}
			Debug.Log(string.Format("[Catalog] Loaded Addressable Assets for {0} entries in {1:F2} sec", totalCount, Time.realtimeSinceStartup - refreshTimer));
			yield break;
		}

		// Token: 0x06001218 RID: 4632 RVA: 0x00081A9C File Offset: 0x0007FC9C
		public static void ReleaseAddressableAssets()
		{
			for (int i = 0; i < Catalog.data.Length; i++)
			{
				CatalogCategory catalogCategory = Catalog.data[i];
				if (catalogCategory != null)
				{
					int catalogDatasCount = catalogCategory.catalogDatas.Count;
					for (int j = 0; j < catalogDatasCount; j++)
					{
						catalogCategory.catalogDatas[j].ReleaseAddressableAssets();
					}
				}
			}
		}

		// Token: 0x06001219 RID: 4633 RVA: 0x00081AEF File Offset: 0x0007FCEF
		public static IEnumerator AddressablesInitializeAsync()
		{
			AddressLocationCache.Clear();
			while (!Addressables.InitializeAsync().IsDone)
			{
				yield return null;
			}
			yield break;
		}

		/// <summary>
		/// Method to update data when the language changes
		/// </summary>
		/// <param name="language">Language key</param>
		// Token: 0x0600121A RID: 4634 RVA: 0x00081AF8 File Offset: 0x0007FCF8
		public static void OnLanguageChanged(string language)
		{
			for (int i = 0; i < Catalog.data.Length; i++)
			{
				CatalogCategory catalogCategory = Catalog.data[i];
				if (catalogCategory != null)
				{
					int catalogDatasCount = catalogCategory.catalogDatas.Count;
					for (int j = 0; j < catalogDatasCount; j++)
					{
						catalogCategory.catalogDatas[j].OnLanguageChanged(language);
					}
				}
			}
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x00081B4C File Offset: 0x0007FD4C
		public static TextData GetTextData()
		{
			if (!Catalog.IsJsonLoaded())
			{
				return null;
			}
			if (LocalizationManager.Instance != null)
			{
				return Catalog.GetData<TextData>(LocalizationManager.Instance.Language, false) ?? Catalog.GetData<TextData>(SystemLanguage.English.ToString(), true);
			}
			return Catalog.GetData<TextData>(SystemLanguage.English.ToString(), true);
		}

		/// <summary>
		/// Use this just for strings that need to be displayed in the UI when the game starts and before the catalog is loaded.
		/// Note: Add new entries (some are in Unicode) when new languages are added
		/// </summary>
		/// <param name="stringId">String id from the Default text group</param>
		/// <returns>Localized string</returns>
		// Token: 0x0600121C RID: 4636 RVA: 0x00081BB0 File Offset: 0x0007FDB0
		public static string GetPreCatalogLoadText(string stringId)
		{
			string text = stringId;
			string language = (GameManager.options != null) ? GameManager.options.language : Application.systemLanguage.ToString();
			if (stringId == "Loading")
			{
				uint num = <PrivateImplementationDetails>.ComputeStringHash(language);
				if (num <= 2092788901U)
				{
					if (num <= 1065318370U)
					{
						if (num != 286263347U)
						{
							if (num != 463134907U)
							{
								if (num != 1065318370U)
								{
									goto IL_24B;
								}
								if (!(language == "ChineseSimplified"))
								{
									goto IL_24B;
								}
							}
							else
							{
								if (!(language == "English"))
								{
									goto IL_24B;
								}
								return "Loading";
							}
						}
						else
						{
							if (!(language == "German"))
							{
								goto IL_24B;
							}
							return "Laden";
						}
					}
					else if (num != 1615110235U)
					{
						if (num != 1724680745U)
						{
							if (num != 2092788901U)
							{
								goto IL_24B;
							}
							if (!(language == "French"))
							{
								goto IL_24B;
							}
							return "Chargement";
						}
						else
						{
							if (!(language == "ChineseTraditional"))
							{
								goto IL_24B;
							}
							return Utils.UnicodeToInternationalCharacters("載入中");
						}
					}
					else
					{
						if (!(language == "Thai"))
						{
							goto IL_24B;
						}
						return Utils.UnicodeToInternationalCharacters("กำลังโหลด");
					}
				}
				else if (num <= 2909847329U)
				{
					if (num != 2115103848U)
					{
						if (num != 2483826186U)
						{
							if (num != 2909847329U)
							{
								goto IL_24B;
							}
							if (!(language == "Korean"))
							{
								goto IL_24B;
							}
							return Utils.UnicodeToInternationalCharacters("로딩 중");
						}
						else
						{
							if (!(language == "Japanese"))
							{
								goto IL_24B;
							}
							return Utils.UnicodeToInternationalCharacters("ロード中");
						}
					}
					else if (!(language == "Chinese"))
					{
						goto IL_24B;
					}
				}
				else if (num != 3088679515U)
				{
					if (num != 3872816476U)
					{
						if (num != 4030305579U)
						{
							goto IL_24B;
						}
						if (!(language == "Italian"))
						{
							goto IL_24B;
						}
						return "Caricamento";
					}
					else
					{
						if (!(language == "Portuguese"))
						{
							goto IL_24B;
						}
						return "Carregando";
					}
				}
				else
				{
					if (!(language == "Spanish"))
					{
						goto IL_24B;
					}
					return "Cargando";
				}
				return Utils.UnicodeToInternationalCharacters("正在载入");
				IL_24B:
				text = "Loading";
			}
			return text;
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x00081E0F File Offset: 0x0008000F
		public static bool TryGetCategoryData(Category category, out CatalogCategory catalogCategory)
		{
			catalogCategory = Catalog.GetCategoryData(category);
			return catalogCategory != null;
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x00081E1E File Offset: 0x0008001E
		public static CatalogCategory GetCategoryData(Category category)
		{
			CatalogCategory[] array = Catalog.data;
			if (array == null)
			{
				return null;
			}
			return array[(int)category];
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x00081E2D File Offset: 0x0008002D
		public static bool TryGetData<T>(string id, out T outputData, bool logWarning = true) where T : CatalogData
		{
			outputData = default(T);
			if (string.IsNullOrEmpty(id))
			{
				return false;
			}
			outputData = Catalog.GetData<T>(id, logWarning);
			return outputData != null;
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x00081E5C File Offset: 0x0008005C
		public static T GetData<T>(string id, bool logWarning = true) where T : CatalogData
		{
			if (string.IsNullOrEmpty(id))
			{
				return default(T);
			}
			Category category;
			if (Catalog.TryGetCategory<T>(out category))
			{
				CatalogData catalogData = Catalog.GetData(category, id, logWarning);
				if (catalogData is T)
				{
					return catalogData as T;
				}
			}
			if (logWarning)
			{
				Debug.LogWarning(string.Format("Data [{0} | {1}] of type [{2} | {3}] cannot be found in catalog or is not the correct type", new object[]
				{
					id,
					Animator.StringToHash(id.ToLower()),
					typeof(T),
					category
				}));
			}
			return default(T);
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x00081EF0 File Offset: 0x000800F0
		public static CatalogData GetData(Category category, string id, bool logWarning = true)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}
			CatalogCategory categoryData;
			CatalogData catalogData;
			if (Catalog.TryGetCategoryData(category, out categoryData) && categoryData.TryGetCatalogData(id, out catalogData))
			{
				return catalogData;
			}
			if (logWarning)
			{
				Debug.LogWarning(string.Format("Data [{0} | {1}] of type [{2}] cannot be found in catalog", id, Animator.StringToHash(id.ToLower()), category));
			}
			return null;
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x00081F48 File Offset: 0x00080148
		public static IEnumerable<CatalogData> GetDataEnumerable(IEnumerable<Category> categories)
		{
			using (IEnumerator<Category> enumerator = categories.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					CatalogCategory catalogCategory;
					if (Catalog.TryGetCategoryData(enumerator.Current, out catalogCategory))
					{
						int catalogDatasCount = catalogCategory.catalogDatas.Count;
						int num;
						for (int i = 0; i < catalogDatasCount; i = num + 1)
						{
							CatalogData catalogData = catalogCategory.catalogDatas[i];
							yield return catalogData;
							num = i;
						}
						catalogCategory = null;
					}
				}
			}
			IEnumerator<Category> enumerator = null;
			yield break;
			yield break;
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x00081F58 File Offset: 0x00080158
		public static List<CatalogData> GetDataList(Category category)
		{
			CatalogCategory categoryData;
			if (!Catalog.TryGetCategoryData(category, out categoryData))
			{
				return new List<CatalogData>();
			}
			return categoryData.catalogDatas;
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x00081F7C File Offset: 0x0008017C
		public static List<T> GetDataList<T>() where T : CatalogData
		{
			Category category;
			if (!Catalog.TryGetCategory<T>(out category))
			{
				return new List<T>(0);
			}
			return (from catalogData in Catalog.GetDataList(category)
			where catalogData is T
			select (T)((object)catalogData)).ToList<T>();
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x00081FEC File Offset: 0x000801EC
		public static IEnumerable<T> GetDataEnumerable<T>() where T : CatalogData
		{
			Category category;
			if (!Catalog.TryGetCategory<T>(out category))
			{
				return new T[0];
			}
			return from catalogData in Catalog.GetDataList(category)
			where catalogData is T
			select (T)((object)catalogData);
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x00082058 File Offset: 0x00080258
		public static List<ValueDropdownItem<string>> GetDropdownAllID(Category category, string noneText = "None")
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>();
			if (noneText != null)
			{
				dropdownList.Add(new ValueDropdownItem<string>(noneText, ""));
			}
			foreach (string id in Catalog.GetAllID(category))
			{
				dropdownList.Add(new ValueDropdownItem<string>(id, id));
			}
			return dropdownList;
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x000820CC File Offset: 0x000802CC
		public static List<ValueDropdownItem<string>> GetDropdownAllID<T>(string noneText = "None") where T : CatalogData
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>();
			if (noneText != null)
			{
				dropdownList.Add(new ValueDropdownItem<string>(noneText, ""));
			}
			foreach (string id in Catalog.GetAllID<T>())
			{
				dropdownList.Add(new ValueDropdownItem<string>(id, id));
			}
			return dropdownList;
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x00082140 File Offset: 0x00080340
		public static List<ValueDropdownItem<string>> GetDropdownHolderSlots(string noneText = "None")
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>();
			if (noneText != null)
			{
				dropdownList.Add(new ValueDropdownItem<string>(noneText, ""));
			}
			foreach (string id in Catalog.gameData.holderSlots)
			{
				dropdownList.Add(new ValueDropdownItem<string>(id, id));
			}
			return dropdownList;
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x000821B8 File Offset: 0x000803B8
		public static List<string> GetAllID(Category category)
		{
			return (from x in Catalog.GetDataList(category)
			select x.id).ToList<string>();
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x000821EC File Offset: 0x000803EC
		public static List<string> GetAllID<T>() where T : CatalogData
		{
			List<T> dataList = Catalog.GetDataList<T>();
			if (dataList.IsNullOrEmpty())
			{
				return new List<string>(0);
			}
			List<string> results = new List<string>();
			for (int i = 0; i < dataList.Count; i++)
			{
				results.Add(dataList[i].id);
			}
			return results;
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x0008223D File Offset: 0x0008043D
		public static float GetCollisionStayRatio(float velocityMagnitude)
		{
			return Mathf.InverseLerp(Catalog.gameData.collisionStayVelocityRange.x, Catalog.gameData.collisionStayVelocityRange.y, velocityMagnitude);
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x00082264 File Offset: 0x00080464
		public static void LoadLocationAsync<T>(string address, Action<IResourceLocation> result, string requestName)
		{
			AsyncOperationHandle<IList<IResourceLocation>> operation = Catalog.LoadResourceLocationsAsync<T>(address);
			if (operation.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError(string.Format("Addressable operation failed load location for address: {0} - Please check the bundles have been built correctly. {1}", address, operation.OperationException));
				if (operation.OperationException == null)
				{
					Catalog.ReleaseAsset<IList<IResourceLocation>>(operation);
				}
			}
			operation.Completed += delegate(AsyncOperationHandle<IList<IResourceLocation>> operationHandle)
			{
				Catalog.OnLoadResourceLocations<T>(address, result, operationHandle, requestName);
			};
		}

		// Token: 0x0600122D RID: 4653 RVA: 0x000822E0 File Offset: 0x000804E0
		public static IEnumerator LoadLocationCoroutine<T>(string address, Action<IResourceLocation> result, string requestName)
		{
			if (!Application.isPlaying || string.IsNullOrEmpty(address))
			{
				if (result != null)
				{
					result(null);
				}
				yield break;
			}
			IResourceLocation location;
			if (AddressLocationCache.TryGetAddressLocation<T>(address, out location))
			{
				if (result != null)
				{
					result(location);
				}
				yield break;
			}
			AsyncOperationHandle<IList<IResourceLocation>> handle = Catalog.LoadResourceLocationsAsync<T>(address);
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError(string.Format("Addressable operation failed load location for address: {0} - Please check the bundles have been built correctly. {1}", address, handle.OperationException));
				if (handle.OperationException == null)
				{
					Catalog.ReleaseAsset<IList<IResourceLocation>>(handle);
				}
				if (result != null)
				{
					result(null);
				}
				yield break;
			}
			if (!handle.IsDone)
			{
				yield return handle;
			}
			Catalog.OnLoadResourceLocations<T>(address, result, handle, requestName);
			yield break;
		}

		// Token: 0x0600122E RID: 4654 RVA: 0x000822FD File Offset: 0x000804FD
		private static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync<T>(string address)
		{
			return Catalog.LoadResourceLocationsAsync(address, typeof(T));
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x00082310 File Offset: 0x00080510
		private static AsyncOperationHandle<IList<IResourceLocation>> LoadResourceLocationsAsync(string address, Type type = null)
		{
			string addressOnly;
			string subAddress;
			Catalog.GetAddressAndSubAddress(address, out addressOnly, out subAddress);
			if (subAddress != null)
			{
				return Addressables.LoadResourceLocationsAsync(address, type);
			}
			return Addressables.LoadResourceLocationsAsync(new string[]
			{
				address,
				Common.GetQualityLevel(false).ToString()
			}, Addressables.MergeMode.Intersection, type);
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x00082360 File Offset: 0x00080560
		private static void GetAddressAndSubAddress(string address, out string addressOnly, out string subAddress)
		{
			if (address.Contains("["))
			{
				string[] split = address.Split(new char[]
				{
					'[',
					']'
				});
				addressOnly = split[0];
				subAddress = split[1];
				return;
			}
			addressOnly = address;
			subAddress = null;
		}

		// Token: 0x06001231 RID: 4657 RVA: 0x000823A2 File Offset: 0x000805A2
		private static void OnLoadResourceLocations<T>(string address, Action<IResourceLocation> callback, AsyncOperationHandle<IList<IResourceLocation>> handle, string requestName)
		{
			Catalog.OnLoadResourceLocations(address, callback, handle, requestName, typeof(T));
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x000823B8 File Offset: 0x000805B8
		private static void OnLoadResourceLocations(string address, Action<IResourceLocation> callback, AsyncOperationHandle<IList<IResourceLocation>> handle, string requestName, Type type)
		{
			if (handle.Status == AsyncOperationStatus.Succeeded && handle.Result.Count > 0)
			{
				IList<IResourceLocation> locations = handle.Result;
				IResourceLocation resultLocation = handle.Result[0];
				if (locations.Count > 1)
				{
					string addressOnly;
					string subAddress;
					Catalog.GetAddressAndSubAddress(address, out addressOnly, out subAddress);
					if (!string.IsNullOrEmpty(subAddress))
					{
						resultLocation = locations.First((IResourceLocation location) => location.InternalId.Contains(subAddress));
					}
					else
					{
						Debug.LogWarning("Multiple addressable locations found for address: " + address + ". There could be duplicate assets with the same address. Using first one: " + resultLocation.InternalId);
					}
				}
				AddressLocationCache.TryAddAddressLocation(address, type, resultLocation);
				if (callback != null)
				{
					callback(resultLocation);
					return;
				}
			}
			else
			{
				Debug.LogWarning(string.Format("Address [{0}] of type {1} not found for [{2}]", address, type, requestName));
				Addressables.Release<IList<IResourceLocation>>(handle);
				if (callback != null)
				{
					callback(null);
				}
			}
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x00082490 File Offset: 0x00080690
		public static void LoadAssetAsync<T>(string address, Action<T> callback, string handlerName) where T : UnityEngine.Object
		{
			if (!Application.isPlaying || string.IsNullOrEmpty(address))
			{
				Action<T> callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(default(T));
				return;
			}
			else
			{
				IResourceLocation cachedLocation;
				if (AddressLocationCache.TryGetAddressLocation<T>(address, out cachedLocation))
				{
					Catalog.LoadAssetAsync<T>(cachedLocation, callback, handlerName);
					return;
				}
				Catalog.LoadLocationAsync<T>(address, delegate(IResourceLocation location)
				{
					Catalog.LoadAssetAsync<T>(location, callback, handlerName);
				}, handlerName);
				return;
			}
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x00082510 File Offset: 0x00080710
		public static void LoadAssetAsync<T>(object location, Action<T> callback, string handlerName) where T : UnityEngine.Object
		{
			if (Catalog.RefreshingCatalog)
			{
				Debug.LogError(string.Format("Attempting to load asset [{0}] while the catalog is refreshing. Please use LoadAddressableAssetsCoroutine instead.", location));
			}
			if (!Application.isPlaying || location == null)
			{
				Action<T> callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(default(T));
				return;
			}
			else
			{
				IResourceLocation resourceLocation = null;
				AsyncOperationHandle<T> handle;
				if (location is IResourceLocation)
				{
					resourceLocation = (location as IResourceLocation);
					handle = Addressables.LoadAssetAsync<T>(resourceLocation);
				}
				else
				{
					handle = Addressables.LoadAssetAsync<T>(location);
				}
				if (handle.Status != AsyncOperationStatus.Failed)
				{
					handle.Completed += delegate(AsyncOperationHandle<T> operationHandle)
					{
						if (operationHandle.Status == AsyncOperationStatus.Succeeded)
						{
							Action<T> callback4 = callback;
							if (callback4 == null)
							{
								return;
							}
							callback4(operationHandle.Result);
							return;
						}
						else
						{
							string format = "Unable to find asset at resource location [{0}][{1}][{2}] for object [{3}]";
							object[] array = new object[4];
							array[0] = typeof(T).Name;
							int num = 1;
							IResourceLocation resourceLocation = resourceLocation;
							array[num] = ((resourceLocation != null) ? resourceLocation.ResourceType : null);
							int num2 = 2;
							IResourceLocation resourceLocation2 = resourceLocation;
							array[num2] = ((resourceLocation2 != null) ? resourceLocation2.PrimaryKey : null);
							array[3] = handlerName;
							Debug.LogWarning(string.Format(format, array));
							if (handle.OperationException == null)
							{
								Catalog.ReleaseAsset<T>(operationHandle);
							}
							Action<T> callback5 = callback;
							if (callback5 == null)
							{
								return;
							}
							callback5(default(T));
							return;
						}
					};
					return;
				}
				Debug.LogError(string.Format("Addressable operation failed load location for handlerName: {0} - Please check the bundles have been built correctly. {1}", handlerName, handle.OperationException));
				if (handle.OperationException == null)
				{
					Catalog.ReleaseAsset<T>(handle);
				}
				Action<T> callback3 = callback;
				if (callback3 == null)
				{
					return;
				}
				callback3(default(T));
				return;
			}
		}

		// Token: 0x06001235 RID: 4661 RVA: 0x0008261D File Offset: 0x0008081D
		public static IEnumerator LoadAssetCoroutine<T>(string address, Action<T> callback, string handlerName) where T : UnityEngine.Object
		{
			if (!Application.isPlaying || string.IsNullOrEmpty(address))
			{
				if (callback != null)
				{
					callback(default(T));
				}
				yield break;
			}
			IResourceLocation location;
			if (!AddressLocationCache.TryGetAddressLocation<T>(address, out location))
			{
				yield return Catalog.LoadLocationCoroutine<T>(address, delegate(IResourceLocation value)
				{
					location = value;
				}, handlerName);
			}
			yield return Catalog.LoadAssetCoroutine<T>(location, callback, handlerName);
			yield break;
		}

		// Token: 0x06001236 RID: 4662 RVA: 0x0008263A File Offset: 0x0008083A
		public static IEnumerator LoadAssetCoroutine<T>(IResourceLocation location, Action<T> callback, string handlerName) where T : UnityEngine.Object
		{
			if (Catalog.RefreshingCatalog)
			{
				Debug.LogError(string.Format("Attempting to load asset [{0}] while the catalog is refreshing. Please use LoadAddressableAssetsCoroutine instead.", location));
			}
			if (location == null)
			{
				if (callback != null)
				{
					callback(default(T));
				}
				yield break;
			}
			AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(location);
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError(string.Format("Addressable operation failed load location for handlerName: {0} - Please check the bundles have been built correctly. {1}", handlerName, handle.OperationException));
				if (handle.OperationException == null)
				{
					Catalog.ReleaseAsset<T>(handle);
				}
				if (callback != null)
				{
					callback(default(T));
				}
				yield break;
			}
			yield return handle;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				callback(handle.Result);
				yield break;
			}
			Debug.LogWarning(string.Format("Unable to find asset at resource location [{0}][{1}][{2}] for object [{3}]", new object[]
			{
				typeof(T).Name,
				location.ResourceType,
				location.PrimaryKey,
				handlerName
			}));
			if (handle.OperationException == null)
			{
				Catalog.ReleaseAsset<T>(handle);
			}
			if (callback != null)
			{
				callback(default(T));
			}
			yield break;
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x00082658 File Offset: 0x00080858
		public static void InstantiateAsync(string address, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> callback, string handlerName)
		{
			if (!Application.isPlaying || string.IsNullOrEmpty(address))
			{
				Action<GameObject> callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(null);
				return;
			}
			else
			{
				IResourceLocation cachedLocation;
				if (AddressLocationCache.TryGetAddressLocation<GameObject>(address, out cachedLocation))
				{
					Catalog.InstantiateAsync(cachedLocation, position, rotation, parent, callback, handlerName);
					return;
				}
				Catalog.LoadLocationAsync<GameObject>(address, delegate(IResourceLocation location)
				{
					Catalog.InstantiateAsync(location, position, rotation, parent, callback, handlerName);
				}, handlerName);
				return;
			}
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x000826F8 File Offset: 0x000808F8
		public static void InstantiateAsync(object location, Vector3 position, Quaternion rotation, Transform parent, Action<GameObject> callback, string handlerName)
		{
			if (!Application.isPlaying || location == null)
			{
				Debug.LogWarning("Prefab location is null, unable to instantiate gameobject for object " + handlerName);
				Action<GameObject> callback2 = callback;
				if (callback2 == null)
				{
					return;
				}
				callback2(null);
				return;
			}
			else
			{
				IResourceLocation resourceLocation = null;
				AsyncOperationHandle<GameObject> handle;
				if (location is IResourceLocation)
				{
					resourceLocation = (location as IResourceLocation);
					handle = Addressables.InstantiateAsync(resourceLocation, position, rotation, parent, true);
				}
				else
				{
					handle = Addressables.InstantiateAsync(location, position, rotation, parent, true);
				}
				if (handle.Status != AsyncOperationStatus.Failed)
				{
					handle.Completed += delegate(AsyncOperationHandle<GameObject> operationHandle)
					{
						if (operationHandle.Status == AsyncOperationStatus.Succeeded)
						{
							Action<GameObject> callback4 = callback;
							if (callback4 == null)
							{
								return;
							}
							callback4(operationHandle.Result);
							return;
						}
						else
						{
							string str = "Unable to instantiate gameObject from location ";
							IResourceLocation resourceLocation = resourceLocation;
							Debug.LogWarning(str + ((resourceLocation != null) ? resourceLocation.PrimaryKey : null) + " for object " + handlerName);
							if (handle.OperationException == null)
							{
								Addressables.ReleaseInstance(operationHandle);
							}
							Action<GameObject> callback5 = callback;
							if (callback5 == null)
							{
								return;
							}
							callback5(null);
							return;
						}
					};
					return;
				}
				Debug.LogError(string.Format("Addressable operation failed InstantiateAsync for handlerName: {0} - Please check the bundles have been built correctly. {1}", handlerName, handle.OperationException));
				if (handle.OperationException == null)
				{
					Catalog.ReleaseAsset<GameObject>(handle);
				}
				Action<GameObject> callback3 = callback;
				if (callback3 == null)
				{
					return;
				}
				callback3(null);
				return;
			}
		}

		// Token: 0x06001239 RID: 4665 RVA: 0x000827FD File Offset: 0x000809FD
		public static IEnumerator InstantiateCoroutine<T>(string address, Action<T> callback, string handlerName) where T : UnityEngine.Object
		{
			if (!Application.isPlaying || string.IsNullOrEmpty(address))
			{
				if (callback != null)
				{
					callback(default(T));
				}
				yield break;
			}
			IResourceLocation location;
			if (!AddressLocationCache.TryGetAddressLocation<T>(address, out location))
			{
				yield return Catalog.LoadLocationCoroutine<GameObject>(address, delegate(IResourceLocation value)
				{
					location = value;
				}, handlerName);
			}
			yield return Catalog.InstantiateCoroutine<T>(location, callback, handlerName);
			yield break;
		}

		/// <summary>
		/// Instantiates a prefab and returns either the prefab itself if the Type param is GameObject or a component on the gameobject of type T
		/// </summary>
		/// <param name="location"></param>
		/// <param name="callback"></param>
		/// <param name="handlerName"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		// Token: 0x0600123A RID: 4666 RVA: 0x0008281A File Offset: 0x00080A1A
		public static IEnumerator InstantiateCoroutine<T>(object location, Action<T> callback, string handlerName) where T : UnityEngine.Object
		{
			if (location == null)
			{
				if (callback != null)
				{
					callback(default(T));
				}
				yield break;
			}
			IResourceLocation resourceLocation = null;
			AsyncOperationHandle<GameObject> handle;
			if (location is IResourceLocation)
			{
				resourceLocation = (location as IResourceLocation);
				handle = Addressables.InstantiateAsync(resourceLocation, null, false, true);
			}
			else
			{
				handle = Addressables.InstantiateAsync(location, null, false, true);
			}
			if (handle.Status == AsyncOperationStatus.Failed)
			{
				Debug.LogError(string.Format("Addressable operation failed InstantiateAsync for handlerName: {0} - Please check the bundles have been built correctly. {1}", handlerName, handle.OperationException));
				if (handle.OperationException == null)
				{
					Catalog.ReleaseAsset<GameObject>(handle);
				}
				if (callback != null)
				{
					callback(default(T));
				}
				yield break;
			}
			yield return handle;
			if (handle.Status == AsyncOperationStatus.Succeeded)
			{
				if (typeof(T) == typeof(GameObject))
				{
					if (callback != null)
					{
						callback(handle.Result as T);
					}
				}
				else if (callback != null)
				{
					callback(handle.Result.GetComponent<T>());
				}
				yield break;
			}
			string[] array = new string[7];
			array[0] = "Unable to find asset at location [";
			int num = 1;
			IResourceLocation resourceLocation2 = resourceLocation;
			array[num] = ((resourceLocation2 != null) ? resourceLocation2.PrimaryKey : null);
			array[2] = "] of type [";
			array[3] = typeof(T).Name;
			array[4] = "] for object [";
			array[5] = handlerName;
			array[6] = "]";
			Debug.LogWarning(string.Concat(array));
			Addressables.Release<GameObject>(handle);
			if (callback != null)
			{
				callback(default(T));
			}
			yield break;
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x00082837 File Offset: 0x00080A37
		public static void ReleaseAsset<TObject>(AsyncOperationHandle<TObject> handle)
		{
			Addressables.Release<TObject>(handle);
		}

		// Token: 0x0600123C RID: 4668 RVA: 0x0008283F File Offset: 0x00080A3F
		public static void ReleaseAsset<TObject>(TObject obj)
		{
			if (obj != null)
			{
				Addressables.Release<TObject>(obj);
			}
		}

		// Token: 0x0600123D RID: 4669 RVA: 0x0008284F File Offset: 0x00080A4F
		public static void ReleaseAsset(AsyncOperationHandle handle)
		{
			Addressables.Release(handle);
		}

		// Token: 0x0600123E RID: 4670 RVA: 0x00082858 File Offset: 0x00080A58
		public static T EditorLoad<T>(string address, Catalog.PlatformSelection platformSelection = Catalog.PlatformSelection.Auto) where T : UnityEngine.Object
		{
			if (string.IsNullOrEmpty(address))
			{
				return default(T);
			}
			Debug.LogError("Can't use Catalog.EditorLoad() when game is played!");
			return default(T);
		}

		// Token: 0x0600123F RID: 4671 RVA: 0x0008288A File Offset: 0x00080A8A
		public static bool EditorExist<T>(string address)
		{
			if (string.IsNullOrEmpty(address))
			{
				return false;
			}
			Debug.LogError("Can't use Catalog.EditorExist() when game is played!");
			return false;
		}

		// Token: 0x06001240 RID: 4672 RVA: 0x000828A4 File Offset: 0x00080AA4
		public static string GetImagePlatformAddress(string imageAddress)
		{
			if (imageAddress == null)
			{
				return string.Empty;
			}
			string updatedAddress = imageAddress;
			if (imageAddress.Contains("{Controller}"))
			{
				if (imageAddress.Contains(".Menu") && PlayerControl.controllerDiagram == PlayerControl.ControllerDiagram.OculusTouch && PlayerControl.loader == PlayerControl.Loader.OpenVR)
				{
					updatedAddress = imageAddress.Replace("{Controller}", PlayerControl.loader.ToString() + PlayerControl.controllerDiagram.ToString());
				}
				else
				{
					updatedAddress = imageAddress.Replace("{Controller}", PlayerControl.controllerDiagram.ToString());
				}
			}
			return updatedAddress;
		}

		// Token: 0x06001241 RID: 4673 RVA: 0x00082938 File Offset: 0x00080B38
		public static string GetVideoPlatformAddress(string videoAddress)
		{
			if (videoAddress == null)
			{
				return string.Empty;
			}
			string updatedAddress = videoAddress;
			if (videoAddress.Contains("{Controller}"))
			{
				updatedAddress = videoAddress.Replace("{Controller}", PlayerControl.controllerDiagram.ToString());
			}
			return updatedAddress;
		}

		// Token: 0x0400103E RID: 4158
		public static List<string> loadModFolders = new List<string>();

		// Token: 0x0400103F RID: 4159
		public static CatalogCategory[] data = new CatalogCategory[Enum.GetNames(typeof(Category)).Length];

		// Token: 0x04001040 RID: 4160
		public static GameData gameData;

		// Token: 0x04001041 RID: 4161
		public static bool loadMods = true;

		// Token: 0x04001042 RID: 4162
		public static bool checkFileVersion = true;

		// Token: 0x04001043 RID: 4163
		public static JsonSerializerSettings jsonSerializerSettings;

		// Token: 0x04001044 RID: 4164
		public static JsonSerializer jsonSerializer;

		/// <summary>
		/// A mapping of all of the base types to their catalog Categories
		/// </summary>
		// Token: 0x04001045 RID: 4165
		[TupleElementNames(new string[]
		{
			"Type",
			"Category"
		})]
		public static ValueTuple<Type, Category>[] baseTypeCategories = new ValueTuple<Type, Category>[]
		{
			new ValueTuple<Type, Category>(typeof(WaveData), Category.Wave),
			new ValueTuple<Type, Category>(typeof(CreatureData), Category.Creature),
			new ValueTuple<Type, Category>(typeof(ItemData), Category.Item),
			new ValueTuple<Type, Category>(typeof(EffectData), Category.Effect),
			new ValueTuple<Type, Category>(typeof(TextData), Category.Text),
			new ValueTuple<Type, Category>(typeof(InteractableData), Category.Interactable),
			new ValueTuple<Type, Category>(typeof(HandPoseData), Category.HandPose),
			new ValueTuple<Type, Category>(typeof(ExpressionData), Category.Expression),
			new ValueTuple<Type, Category>(typeof(LevelData), Category.Level),
			new ValueTuple<Type, Category>(typeof(DamagerData), Category.Damager),
			new ValueTuple<Type, Category>(typeof(MaterialData), Category.Material),
			new ValueTuple<Type, Category>(typeof(ColliderGroupData), Category.ColliderGroup),
			new ValueTuple<Type, Category>(typeof(CreatureTable), Category.CreatureTable),
			new ValueTuple<Type, Category>(typeof(LootTableBase), Category.LootTable),
			new ValueTuple<Type, Category>(typeof(ContainerData), Category.Container),
			new ValueTuple<Type, Category>(typeof(BrainData), Category.Brain),
			new ValueTuple<Type, Category>(typeof(SkillData), Category.Skill),
			new ValueTuple<Type, Category>(typeof(EffectGroupData), Category.EffectGroup),
			new ValueTuple<Type, Category>(typeof(DamageModifierData), Category.DamageModifier),
			new ValueTuple<Type, Category>(typeof(LiquidData), Category.Liquid),
			new ValueTuple<Type, Category>(typeof(MusicGroup), Category.MusicGroup),
			new ValueTuple<Type, Category>(typeof(Music), Category.Music),
			new ValueTuple<Type, Category>(typeof(AnimationData), Category.Animation),
			new ValueTuple<Type, Category>(typeof(VoiceData), Category.Voice),
			new ValueTuple<Type, Category>(typeof(AreaConnectionTypeData), Category.AreaConnectionType),
			new ValueTuple<Type, Category>(typeof(AreaData), Category.Area),
			new ValueTuple<Type, Category>(typeof(AreaTable), Category.AreaTable),
			new ValueTuple<Type, Category>(typeof(AreaCollectionData), Category.AreaCollection),
			new ValueTuple<Type, Category>(typeof(ShopData), Category.Shop),
			new ValueTuple<Type, Category>(typeof(MenuData), Category.Menu),
			new ValueTuple<Type, Category>(typeof(BehaviorTreeData), Category.BehaviorTree),
			new ValueTuple<Type, Category>(typeof(CustomData), Category.Custom),
			new ValueTuple<Type, Category>(typeof(KeyboardData), Category.Keyboard),
			new ValueTuple<Type, Category>(typeof(GameModeData), Category.GameMode),
			new ValueTuple<Type, Category>(typeof(StatusData), Category.Status),
			new ValueTuple<Type, Category>(typeof(StanceData), Category.Stance),
			new ValueTuple<Type, Category>(typeof(EntityModule), Category.EntityModule),
			new ValueTuple<Type, Category>(typeof(SkillTreeData), Category.SkillTree),
			new ValueTuple<Type, Category>(typeof(EnemyConfig), Category.EnemyConfig),
			new ValueTuple<Type, Category>(typeof(LootConfigData), Category.LootConfig)
		};

		// Token: 0x04001046 RID: 4166
		public static Dictionary<Type, Category> typeCategories = new Dictionary<Type, Category>();

		// Token: 0x04001047 RID: 4167
		public static bool RefreshingCatalog;

		// Token: 0x0200074A RID: 1866
		public enum PlatformSelection
		{
			// Token: 0x04003CC2 RID: 15554
			Auto,
			// Token: 0x04003CC3 RID: 15555
			Windows,
			// Token: 0x04003CC4 RID: 15556
			Android
		}
	}
}
