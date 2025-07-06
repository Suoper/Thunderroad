using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200036D RID: 877
	public static class ModManager
	{
		// Token: 0x14000139 RID: 313
		// (add) Token: 0x06002971 RID: 10609 RVA: 0x00119E34 File Offset: 0x00118034
		// (remove) Token: 0x06002972 RID: 10610 RVA: 0x00119E68 File Offset: 0x00118068
		public static event ModManager.ModLoadEvent OnModLoad;

		// Token: 0x1400013A RID: 314
		// (add) Token: 0x06002973 RID: 10611 RVA: 0x00119E9C File Offset: 0x0011809C
		// (remove) Token: 0x06002974 RID: 10612 RVA: 0x00119ED0 File Offset: 0x001180D0
		public static event ModManager.ModLoadEvent OnModUnload;

		// Token: 0x06002975 RID: 10613 RVA: 0x00119F03 File Offset: 0x00118103
		public static void InvokeOnModLoadEvent(EventTime eventTime, ModManager.ModLoadEventType eventType, ModManager.ModData modData = null)
		{
			if (ModManager.OnModLoad != null)
			{
				ModManager.OnModLoad(eventTime, eventType, modData);
			}
		}

		// Token: 0x06002976 RID: 10614 RVA: 0x00119F19 File Offset: 0x00118119
		public static IEnumerator LoadAndRefreshCatalog(bool loadJsonOnly = false)
		{
			ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.ModManager, null);
			if (ThunderRoadSettings.current.build.allowModManager || ThunderRoadSettings.current.build.allowJsonMods || ThunderRoadSettings.current.build.allowScriptedMods)
			{
				LoadingCamera.StartLoading(LoadingCamera.LoadingType.LoadMods);
				if (!ModManager.gameModsLoaded)
				{
					yield return ModManager.LoadCoroutine(loadJsonOnly);
				}
				if (ModManager.gameModsLoaded && !ModManager.isGameModsCatalogRefreshed)
				{
					yield return Catalog.RefreshCoroutine();
					ModManager.isGameModsCatalogRefreshed = true;
					yield return GameManager.options.ApplyModOptions();
				}
				Debug.Log("[ModManager] Reloading localization after mods loaded");
				LocalizationManager.Instance.OnLanguageChanged(LocalizationManager.Instance.Language);
				LoadingCamera.FinishLoading(LoadingCamera.LoadingType.LoadMods);
			}
			ModManager.gameModsLoaded = true;
			yield return PlayerSaveData.LoadCharacterSaves(null);
			ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.ModManager, null);
			yield break;
		}

		// Token: 0x06002977 RID: 10615 RVA: 0x00119F28 File Offset: 0x00118128
		public static IEnumerator LoadCoroutine(bool loadJsonOnly = false)
		{
			LoadingCamera.SetLoadingStage(LoadingCamera.Stage.LoadMods, false);
			List<ModManager.ModData> orderedMods = ModManager.GetOrderedMods();
			orderedMods = (from m in orderedMods
			where !ModManager.loadedMods.Contains(m) && ModManager.ManifestVersionCheck(m)
			select m).ToList<ModManager.ModData>();
			if (Application.isPlaying || loadJsonOnly)
			{
				if (!loadJsonOnly)
				{
					yield return ModManager.LoadModAssemblies(orderedMods);
					yield return ModManager.LoadModAddressables(orderedMods);
				}
				yield return ModManager.LoadModCatalogs(orderedMods);
				if (!loadJsonOnly)
				{
					yield return ModManager.LoadAddressableAssets(orderedMods);
				}
				if (!loadJsonOnly)
				{
					yield return ModManager.LoadModOptions(orderedMods);
					yield return ModManager.LoadModThunderScripts(orderedMods);
				}
			}
			if (ModManager.loadedMods.Any((ModManager.ModData mod) => !mod.Incompatible) || ModManager.loadedMods == null || ModManager.loadedMods.Count == 0)
			{
				ModManager.gameModsLoaded = true;
			}
			yield break;
		}

		// Token: 0x06002978 RID: 10616 RVA: 0x00119F37 File Offset: 0x00118137
		public static IEnumerator LoadModAssemblies(List<ModManager.ModData> modDatas)
		{
			if (modDatas.IsNullOrEmpty())
			{
				yield break;
			}
			int modCount = modDatas.Count;
			Debug.Log(string.Format("{0}[{1}] Loading Mod Assemblies", "[ModManager]", ModManager.ModLoadEventType.Assembly));
			int num;
			for (int i = 0; i < modCount; i = num + 1)
			{
				ModManager.ModData mod = modDatas[i];
				if (!mod.Incompatible)
				{
					string[] array;
					if (ModManager.TryGetModAssemblyPaths(mod, out array) && !ThunderRoadSettings.current.build.allowScriptedMods)
					{
						Debug.LogWarning(string.Format("{0}[{1}][{2}] Mod: {3} contains scripts, but this platform does not allow scripts. Skipping mod", new object[]
						{
							"[ModManager]",
							ModManager.ModLoadEventType.Assembly,
							mod.folderName,
							mod.Name
						}));
						mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Could not load mod: " + mod.Name, "ModErrorCouldNotLoadMod", mod.Name, "This mod: " + mod.Name + " contains scripts but the game does not support scripts", string.Empty));
						mod.Incompatible = true;
						ModManager.loadedMods.Add(mod);
					}
					else
					{
						ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.Assembly, mod);
						LoadingCamera.SetAdditionalLoadingTextInfo(string.Format(": {0} - {1}", mod.Name, ModManager.ModLoadEventType.Assembly));
						if (Application.isPlaying)
						{
							if (mod.assemblies.Count == 0)
							{
								ModManager.LoadAssembly(mod);
							}
							else
							{
								Debug.LogError(string.Format("{0}[{1}][{2}] Assemblies already loaded for: {3}", new object[]
								{
									"[ModManager]",
									ModManager.ModLoadEventType.Assembly,
									mod.folderName,
									mod.Name
								}));
							}
						}
						ModManager.loadedMods.Add(mod);
						ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.Assembly, mod);
						yield return LoadingCamera.SetPercentageYield((i + 1) * 100 / modCount);
					}
				}
				num = i;
			}
			Debug.Log(string.Format("{0}[{1}] Loaded Mod Assemblies", "[ModManager]", ModManager.ModLoadEventType.Assembly));
			yield break;
		}

		// Token: 0x06002979 RID: 10617 RVA: 0x00119F46 File Offset: 0x00118146
		public static IEnumerator LoadModAddressables(List<ModManager.ModData> modDatas)
		{
			if (modDatas.IsNullOrEmpty())
			{
				yield break;
			}
			int modCount = modDatas.Count;
			Debug.Log(string.Format("{0}[{1}] Loading Mod Addressables", "[ModManager]", ModManager.ModLoadEventType.Addressable));
			int num;
			for (int i = 0; i < modCount; i = num + 1)
			{
				ModManager.ModData mod = modDatas[i];
				if (!mod.Incompatible)
				{
					ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.Addressable, mod);
					LoadingCamera.SetAdditionalLoadingTextInfo(string.Format(": {0} - {1}", mod.Name, ModManager.ModLoadEventType.Addressable));
					if (mod.contentCatalogPaths.Count == 0)
					{
						yield return AddressableAssetManager.LoadMod(mod);
					}
					else
					{
						Debug.LogError(string.Format("{0}[{1}][{2}] Content catalogs already loaded for: {3}", new object[]
						{
							"[ModManager]",
							ModManager.ModLoadEventType.Addressable,
							mod.folderName,
							mod.Name
						}));
					}
					ModManager.loadedMods.Add(mod);
					ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.Addressable, mod);
					yield return LoadingCamera.SetPercentageYield((i + 1) * 100 / modCount);
					mod = null;
				}
				num = i;
			}
			Debug.Log(string.Format("{0}[{1}] Loaded Mod Addressables", "[ModManager]", ModManager.ModLoadEventType.Addressable));
			yield break;
		}

		// Token: 0x0600297A RID: 10618 RVA: 0x00119F55 File Offset: 0x00118155
		public static IEnumerator LoadModCatalogs(List<ModManager.ModData> modDatas)
		{
			if (modDatas.IsNullOrEmpty())
			{
				yield break;
			}
			int modCount = modDatas.Count;
			Debug.Log(string.Format("{0}[{1}] Loading Mod Catalogs", "[ModManager]", ModManager.ModLoadEventType.Catalog));
			int num;
			for (int i = 0; i < modCount; i = num + 1)
			{
				ModManager.ModData mod = modDatas[i];
				if (!mod.Incompatible)
				{
					ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.Catalog, mod);
					LoadingCamera.SetAdditionalLoadingTextInfo(string.Format(": {0} - {1}", mod.Name, ModManager.ModLoadEventType.Catalog));
					if (ThunderRoadSettings.current.build.allowJsonMods)
					{
						Catalog.LoadModCatalog(mod);
					}
					ModManager.loadedMods.Add(mod);
					ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.Catalog, mod);
					yield return LoadingCamera.SetPercentageYield((i + 1) * 100 / modCount);
				}
				num = i;
			}
			Debug.Log(string.Format("{0}[{1}] Loaded Mod Catalogs", "[ModManager]", ModManager.ModLoadEventType.Catalog));
			yield break;
		}

		// Token: 0x0600297B RID: 10619 RVA: 0x00119F64 File Offset: 0x00118164
		public static IEnumerator LoadAddressableAssets()
		{
			yield return ModManager.LoadAddressableAssets(ModManager.loadedMods.ToList<ModManager.ModData>());
			yield break;
		}

		// Token: 0x0600297C RID: 10620 RVA: 0x00119F6C File Offset: 0x0011816C
		public static IEnumerator LoadAddressableAssets(List<ModManager.ModData> modDatas)
		{
			if (ModManager.modCatalogAddressablesLoaded)
			{
				yield break;
			}
			if (modDatas.IsNullOrEmpty())
			{
				yield break;
			}
			int modCount = modDatas.Count;
			Debug.Log(string.Format("{0}[{1}] Loading Mod Addressable Assets", "[ModManager]", ModManager.ModLoadEventType.AddressableAsset));
			int num;
			for (int i = 0; i < modCount; i = num + 1)
			{
				ModManager.ModData mod = modDatas[i];
				if (!mod.Incompatible)
				{
					ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.AddressableAsset, mod);
					LoadingCamera.SetAdditionalLoadingTextInfo(string.Format(": {0} - {1}", mod.Name, ModManager.ModLoadEventType.AddressableAsset));
					foreach (CatalogData catalogData in mod.ownedDatas)
					{
						yield return catalogData.LoadAddressableAssetsCoroutine();
					}
					List<CatalogData>.Enumerator enumerator = default(List<CatalogData>.Enumerator);
					foreach (CatalogData catalogData2 in mod.changedDatas)
					{
						yield return catalogData2.LoadAddressableAssetsCoroutine();
					}
					enumerator = default(List<CatalogData>.Enumerator);
					ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.AddressableAsset, mod);
					mod = null;
				}
				num = i;
			}
			yield return Catalog.gameData.LoadAddressableAssetsCoroutine();
			ModManager.modCatalogAddressablesLoaded = true;
			Debug.Log(string.Format("{0}[{1}] Loaded Mod Addressable Assets", "[ModManager]", ModManager.ModLoadEventType.AddressableAsset));
			yield break;
			yield break;
		}

		// Token: 0x0600297D RID: 10621 RVA: 0x00119F7B File Offset: 0x0011817B
		public static IEnumerator LoadModOptions(List<ModManager.ModData> modDatas)
		{
			if (modDatas.IsNullOrEmpty())
			{
				yield break;
			}
			int modCount = modDatas.Count;
			Debug.Log(string.Format("{0}[{1}] Loading Mod Options", "[ModManager]", ModManager.ModLoadEventType.ModOption));
			int k;
			for (int i = 0; i < modCount; i = k + 1)
			{
				ModManager.<>c__DisplayClass25_0 CS$<>8__locals1;
				CS$<>8__locals1.mod = modDatas[i];
				if (!CS$<>8__locals1.mod.Incompatible)
				{
					ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.ModOption, CS$<>8__locals1.mod);
					LoadingCamera.SetAdditionalLoadingTextInfo(string.Format(": {0} - {1}", CS$<>8__locals1.mod.Name, ModManager.ModLoadEventType.ModOption));
					if (CS$<>8__locals1.mod.modOptions.Count == 0)
					{
						using (List<Assembly>.Enumerator enumerator = CS$<>8__locals1.mod.assemblies.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Assembly assembly = enumerator.Current;
								ModManager.<>c__DisplayClass25_1 CS$<>8__locals2;
								CS$<>8__locals2.assembly = assembly;
								try
								{
									Type[] types = CS$<>8__locals2.assembly.GetTypes();
									for (int j = 0; j < types.Length; j++)
									{
										ModManager.GetModAttributes(types[j], CS$<>8__locals1.mod, CS$<>8__locals2.assembly);
									}
								}
								catch (ReflectionTypeLoadException ex)
								{
									Exception[] loaderExceptions = ex.LoaderExceptions;
									for (k = 0; k < loaderExceptions.Length; k++)
									{
										ModManager.<LoadModOptions>g__LogError|25_0(loaderExceptions[k], ref CS$<>8__locals1, ref CS$<>8__locals2);
									}
									break;
								}
								catch (Exception ex2)
								{
									ModManager.<LoadModOptions>g__LogError|25_0(ex2, ref CS$<>8__locals1, ref CS$<>8__locals2);
									break;
								}
							}
							goto IL_1B9;
						}
						goto IL_177;
					}
					goto IL_177;
					IL_1B9:
					ModManager.loadedMods.Add(CS$<>8__locals1.mod);
					if (!CS$<>8__locals1.mod.modOptions.IsNullOrEmpty())
					{
						Debug.Log(string.Format("{0}[{1}][{2}] Loaded {3} ModOptions", new object[]
						{
							"[ModManager]",
							ModManager.ModLoadEventType.ModOption,
							CS$<>8__locals1.mod.folderName,
							CS$<>8__locals1.mod.modOptions.Count
						}));
					}
					ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.ModOption, CS$<>8__locals1.mod);
					yield return LoadingCamera.SetPercentageYield((i + 1) * 100 / modCount);
					goto IL_262;
					IL_177:
					Debug.LogError(string.Format("{0}[{1}][{2}] ModOptions already loaded for: {3}", new object[]
					{
						"[ModManager]",
						ModManager.ModLoadEventType.ModOption,
						CS$<>8__locals1.mod.folderName,
						CS$<>8__locals1.mod.Name
					}));
					goto IL_1B9;
				}
				IL_262:
				k = i;
			}
			Debug.Log(string.Format("{0}[{1}] Loaded Mod Options", "[ModManager]", ModManager.ModLoadEventType.ModOption));
			yield break;
		}

		// Token: 0x0600297E RID: 10622 RVA: 0x00119F8A File Offset: 0x0011818A
		public static IEnumerator LoadModThunderScripts(List<ModManager.ModData> modDatas)
		{
			if (modDatas.IsNullOrEmpty())
			{
				yield break;
			}
			int modCount = modDatas.Count;
			Debug.Log(string.Format("{0}[{1}] Loading Mod ThunderScripts", "[ModManager]", ModManager.ModLoadEventType.ThunderScript));
			int k;
			for (int i = 0; i < modCount; i = k + 1)
			{
				ModManager.<>c__DisplayClass26_0 CS$<>8__locals1;
				CS$<>8__locals1.mod = modDatas[i];
				if (!CS$<>8__locals1.mod.Incompatible)
				{
					ModManager.InvokeOnModLoadEvent(EventTime.OnStart, ModManager.ModLoadEventType.ThunderScript, CS$<>8__locals1.mod);
					LoadingCamera.SetAdditionalLoadingTextInfo(string.Format(": {0} - {1}", CS$<>8__locals1.mod.Name, ModManager.ModLoadEventType.ThunderScript));
					if (CS$<>8__locals1.mod.thunderScripts.Count == 0)
					{
						using (List<Assembly>.Enumerator enumerator = CS$<>8__locals1.mod.assemblies.GetEnumerator())
						{
							while (enumerator.MoveNext())
							{
								Assembly assembly = enumerator.Current;
								ModManager.<>c__DisplayClass26_1 CS$<>8__locals2;
								CS$<>8__locals2.assembly = assembly;
								try
								{
									foreach (Type type in CS$<>8__locals2.assembly.GetTypes())
									{
										if (type.IsClass)
										{
											ModManager.LoadThunderScripts(type, CS$<>8__locals1.mod, CS$<>8__locals2.assembly);
										}
									}
								}
								catch (ReflectionTypeLoadException ex)
								{
									Exception[] loaderExceptions = ex.LoaderExceptions;
									for (k = 0; k < loaderExceptions.Length; k++)
									{
										ModManager.<LoadModThunderScripts>g__LogError|26_0(loaderExceptions[k], ref CS$<>8__locals1, ref CS$<>8__locals2);
									}
									break;
								}
								catch (Exception ex2)
								{
									ModManager.<LoadModThunderScripts>g__LogError|26_0(ex2, ref CS$<>8__locals1, ref CS$<>8__locals2);
									break;
								}
							}
							goto IL_1C9;
						}
						goto IL_187;
					}
					goto IL_187;
					IL_1C9:
					ModManager.loadedMods.Add(CS$<>8__locals1.mod);
					ModManager.InvokeOnModLoadEvent(EventTime.OnEnd, ModManager.ModLoadEventType.ThunderScript, CS$<>8__locals1.mod);
					yield return LoadingCamera.SetPercentageYield((i + 1) * 100 / modCount);
					goto IL_214;
					IL_187:
					Debug.LogError(string.Format("{0}[{1}][{2}] ThunderScripts already loaded for: {3}", new object[]
					{
						"[ModManager]",
						ModManager.ModLoadEventType.ThunderScript,
						CS$<>8__locals1.mod.folderName,
						CS$<>8__locals1.mod.Name
					}));
					goto IL_1C9;
				}
				IL_214:
				k = i;
			}
			Debug.Log(string.Format("{0}[{1}] Loaded Mod ThunderScripts", "[ModManager]", ModManager.ModLoadEventType.ThunderScript));
			yield break;
		}

		// Token: 0x0600297F RID: 10623 RVA: 0x00119F99 File Offset: 0x00118199
		public static void Load(bool loadJsonOnly = false)
		{
			ModManager.LoadCoroutine(loadJsonOnly).AsSynchronous();
		}

		// Token: 0x06002980 RID: 10624 RVA: 0x00119FA8 File Offset: 0x001181A8
		public static void RefreshModOptionsUI()
		{
			if (ModManager.loadedMods == null || ModManager.loadedMods.Count == 0)
			{
				return;
			}
			foreach (ModOption modOption in ModManager.loadedMods.Where(delegate(ModManager.ModData modData)
			{
				List<ModOption> modOptions = modData.modOptions;
				return modOptions != null && modOptions.Count > 0;
			}).SelectMany((ModManager.ModData modData) => modData.modOptions))
			{
				modOption.RefreshUI();
			}
		}

		// Token: 0x06002981 RID: 10625 RVA: 0x0011A050 File Offset: 0x00118250
		public static void ReloadJson()
		{
			foreach (ModManager.ModData mod in ModManager.loadedMods)
			{
				Debug.Log("[ModManager] - Reloading mod json: " + mod.Name);
				mod.errors.RemoveWhere((ModManager.ModData.Error e) => e.type == ModManager.ModData.ErrorType.JSON);
				if (ThunderRoadSettings.current.build.allowJsonMods)
				{
					Catalog.LoadModCatalog(mod);
				}
				Debug.Log("[ModManager] - Mod json: " + mod.Name + " reloaded");
			}
		}

		// Token: 0x06002982 RID: 10626 RVA: 0x0011A110 File Offset: 0x00118310
		internal static List<ModManager.ModData> GetOrderedMods()
		{
			List<ModManager.ModData> modList = new List<ModManager.ModData>();
			string[] topLevelModFolders = FileManager.GetFolderNames(FileManager.Type.JSONCatalog, FileManager.Source.Mods, "");
			topLevelModFolders = ModManager.GetLoadOrderedMods(topLevelModFolders);
			for (int i = 0; i < topLevelModFolders.Length; i++)
			{
				string topLevelModFolder = topLevelModFolders[i];
				if (ModManager.IsEnabledFolder(topLevelModFolder))
				{
					bool flag = ModManager.ContainsManifest(topLevelModFolder);
					bool hasNestedManifest = Directory.GetDirectories(FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, topLevelModFolder)).Select(new Func<string, string>(Path.GetFileName)).Any(delegate(string p)
					{
						string nestedModFolder = topLevelModFolder + "/" + p;
						return ModManager.IsEnabledFolder(p) && ModManager.ContainsManifest(nestedModFolder);
					});
					if (!flag && !hasNestedManifest)
					{
						Debug.LogWarning("[ModManager] No manifest found in " + topLevelModFolder);
					}
					else if (hasNestedManifest)
					{
						Debug.LogWarning("[ModManager] Unable to load " + topLevelModFolder + ". This mod may have been extracted incorrectly");
					}
					else if (ModManager.<GetOrderedMods>g__AddValidMod|30_0(topLevelModFolder, modList))
					{
						Debug.Log("[ModManager] Loaded mod folder " + topLevelModFolder);
					}
				}
			}
			return modList;
		}

		/// <summary>
		/// Checks if a given mod is compatible with the current game version
		/// </summary>
		/// <param name="modData"></param>
		/// <returns></returns>
		// Token: 0x06002983 RID: 10627 RVA: 0x0011A20C File Offset: 0x0011840C
		public static bool ManifestVersionCheck(ModManager.ModData modData)
		{
			bool result;
			try
			{
				Version minModVersion = new Version(ThunderRoadSettings.current.game.minModVersion);
				if (new Version(modData.GameVersion).CompareTo(minModVersion) != 0)
				{
					Debug.LogWarning(string.Format("{0} - Mod {1} for ({2}) is not compatible with current minimum mod version {3}", new object[]
					{
						"[ModManager]",
						modData.Name,
						modData.GameVersion,
						minModVersion
					}));
					modData.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Manifest, "Incompatible with game version", "ModErrorIncompatibleWithGameVersion", string.Empty, string.Format("Mod {0} for ({1}) is not compatible with current minimum mod version {2}", modData.Name, modData.GameVersion, minModVersion), modData.fullPath + "/manifest.json"));
					ModManager.loadedMods.Add(modData);
					result = false;
				}
				else
				{
					result = true;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		/// <summary>
		/// Tries to read the manifest, returns false if it was unable to read it
		/// </summary>
		/// <param name="modFolder"></param>
		/// <param name="modData"></param>
		/// <returns></returns>
		// Token: 0x06002984 RID: 10628 RVA: 0x0011A2EC File Offset: 0x001184EC
		public static bool TryReadManifest(string modFolder, out ModManager.ModData modData)
		{
			modData = null;
			if (!ModManager.ContainsManifest(modFolder))
			{
				return false;
			}
			try
			{
				string json = FileManager.ReadAllText(FileManager.Type.JSONCatalog, FileManager.Source.Mods, modFolder + "/manifest.json");
				modData = JsonConvert.DeserializeObject<ModManager.ModData>(json, Catalog.GetJsonNetSerializerSettings());
				modData.folderName = modFolder;
				modData.fullPath = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, modFolder);
			}
			catch (Exception e)
			{
				Debug.Log("[ModManager] Unable to read manifest from " + modFolder + " : " + e.Message);
				return false;
			}
			return true;
		}

		// Token: 0x06002985 RID: 10629 RVA: 0x0011A374 File Offset: 0x00118574
		private static bool IsEnabledFolder(string modFolder)
		{
			return (Catalog.loadModFolders.Count > 0 && !Catalog.loadModFolders.Contains(modFolder.ToLower())) || !modFolder.StartsWith("_");
		}

		// Token: 0x06002986 RID: 10630 RVA: 0x0011A3A5 File Offset: 0x001185A5
		private static bool ContainsManifest(string modFolder)
		{
			return FileManager.FileExist(FileManager.Type.JSONCatalog, FileManager.Source.Mods, modFolder + "/manifest.json");
		}

		/// <summary>
		/// Given a list of modFolders, read the loadorder.json and reorder the array of modFolders
		/// </summary>
		/// <param name="modFolders"></param>
		/// <returns></returns>
		// Token: 0x06002987 RID: 10631 RVA: 0x0011A3BC File Offset: 0x001185BC
		private static string[] GetLoadOrderedMods(string[] modFolders)
		{
			string loadOrderPath = FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, "loadorder.json");
			if (File.Exists(loadOrderPath))
			{
				ModManager.LoadOrder loadOrder = JsonUtility.FromJson<ModManager.LoadOrder>(File.ReadAllText(loadOrderPath));
				if (loadOrder != null)
				{
					List<string> modFoldersOrdered = new List<string>();
					for (int i = 0; i < modFolders.Length; i++)
					{
						if (!loadOrder.modNames.Contains(modFolders[i]))
						{
							modFoldersOrdered.Add(modFolders[i]);
						}
					}
					foreach (string loadOrderModName in loadOrder.modNames)
					{
						if (modFolders.Contains(loadOrderModName))
						{
							modFoldersOrdered.Add(loadOrderModName);
						}
					}
					modFolders = modFoldersOrdered.ToArray();
				}
			}
			return modFolders;
		}

		// Token: 0x06002988 RID: 10632 RVA: 0x0011A47C File Offset: 0x0011867C
		public static ModManager.ModData GetModDataFromAssembly(string assemblyFullName)
		{
			Predicate<Assembly> <>9__0;
			foreach (ModManager.ModData modData in ModManager.loadedMods)
			{
				List<Assembly> assemblies = modData.assemblies;
				Predicate<Assembly> match;
				if ((match = <>9__0) == null)
				{
					match = (<>9__0 = ((Assembly a) => a.FullName == assemblyFullName));
				}
				if (assemblies.Exists(match))
				{
					return modData;
				}
			}
			return null;
		}

		/// <summary>
		/// Checks if a mod has dll files in its folder, and if so returns the paths to them.
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="paths"></param>
		/// <returns></returns>
		// Token: 0x06002989 RID: 10633 RVA: 0x0011A50C File Offset: 0x0011870C
		internal static bool TryGetModAssemblyPaths(ModManager.ModData mod, out string[] paths)
		{
			paths = FileManager.GetFullFilePaths(FileManager.Type.JSONCatalog, FileManager.Source.Mods, mod.folderName, "*.dll");
			return paths.Length != 0;
		}

		// Token: 0x0600298A RID: 10634 RVA: 0x0011A528 File Offset: 0x00118728
		private static void LoadAssembly(ModManager.ModData mod)
		{
			mod.assemblies.Clear();
			if (mod.Incompatible)
			{
				return;
			}
			string[] dllPaths;
			if (!ModManager.TryGetModAssemblyPaths(mod, out dllPaths))
			{
				return;
			}
			foreach (string dllPath in dllPaths)
			{
				FileInfo dllFileInfo = new FileInfo(dllPath);
				string dllLocalPath = dllFileInfo.Directory.Name + "/" + dllFileInfo.Name;
				Debug.Log(string.Format("{0}[{1}][{2}] Loading Assembly: {3}", new object[]
				{
					"[ModManager]",
					ModManager.ModLoadEventType.Assembly,
					mod.folderName,
					dllLocalPath
				}));
				byte[] dllBytes = File.ReadAllBytes(dllPath);
				string pdbPath = dllPath.Replace(".dll", ".pdb");
				byte[] pdbBytes = null;
				if (File.Exists(pdbPath))
				{
					FileInfo pdbFileInfo = new FileInfo(pdbPath);
					string pdbLocalPath = pdbFileInfo.Directory.Name + "/" + pdbFileInfo.Name;
					Debug.Log(string.Format("{0}[{1}][{2}] Loading Assembly Debug Symbols: {3}", new object[]
					{
						"[ModManager]",
						ModManager.ModLoadEventType.Assembly,
						mod.folderName,
						pdbLocalPath
					}));
					pdbBytes = File.ReadAllBytes(pdbPath);
				}
				Assembly assembly;
				if (ModManager.TryLoadAssembly(mod, dllBytes, pdbBytes, dllLocalPath, out assembly))
				{
					mod.assemblies.Add(assembly);
					Debug.Log(string.Format("{0}[{1}][{2}] Loaded Assembly: {3}", new object[]
					{
						"[ModManager]",
						ModManager.ModLoadEventType.Assembly,
						mod.folderName,
						dllLocalPath
					}));
				}
			}
			bool incompatible = mod.Incompatible;
		}

		/// <summary>
		/// Tries to load an assembly from a byte array, returns false if it failed
		/// </summary>
		/// <param name="mod"></param>
		/// <param name="dllBytes"></param>
		/// <param name="pdbBytes"></param>
		/// <param name="dllLocalPath"></param>
		/// <param name="assembly"></param>
		/// <returns></returns>
		// Token: 0x0600298B RID: 10635 RVA: 0x0011A6A8 File Offset: 0x001188A8
		private static bool TryLoadAssembly(ModManager.ModData mod, byte[] dllBytes, byte[] pdbBytes, string dllLocalPath, out Assembly assembly)
		{
			assembly = null;
			try
			{
				if (pdbBytes.IsNullOrEmpty())
				{
					assembly = Assembly.Load(dllBytes);
				}
				else
				{
					assembly = Assembly.Load(dllBytes, pdbBytes);
				}
			}
			catch (ReflectionTypeLoadException ex2)
			{
				foreach (Exception inner in ex2.LoaderExceptions)
				{
					Debug.LogError("[ModManager] - Error loading assembly: " + dllLocalPath + " | " + inner.Message);
					mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Could not load assembly", "ModErrorCouldNotLoadAssembly", string.Empty, inner.Message, FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, dllLocalPath)));
					mod.Incompatible = true;
					ModManager.loadedMods.Add(mod);
				}
				return false;
			}
			catch (Exception ex)
			{
				Debug.LogError(string.Format("{0} - Error loading assembly: {1} | {2}", "[ModManager]", dllLocalPath, ex));
				mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Could not load assembly", "ModErrorCouldNotLoadAssembly", string.Empty, ex.Message, FileManager.GetFullPath(FileManager.Type.JSONCatalog, FileManager.Source.Mods, dllLocalPath)));
				mod.Incompatible = true;
				ModManager.loadedMods.Add(mod);
				return false;
			}
			return true;
		}

		// Token: 0x0600298C RID: 10636 RVA: 0x0011A7D4 File Offset: 0x001189D4
		private static void LoadThunderScripts(Type type, ModManager.ModData mod, Assembly assembly)
		{
			if (!type.IsSubclassOf(typeof(ThunderScript)))
			{
				return;
			}
			if (type.IsAbstract)
			{
				return;
			}
			ThunderScript thunderScript = null;
			try
			{
				thunderScript = (ThunderScript)Activator.CreateInstance(type, true);
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("{0}[{1}][{2}] Unable to create instance of ThunderScript for: {3} on mod: {4} in assembly: {5}, {6}", new object[]
				{
					"[ModManager]",
					ModManager.ModLoadEventType.ThunderScript,
					mod.folderName,
					type,
					mod.Name,
					assembly.FullName,
					e
				}));
				mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, string.Format("Unable to create instance of ThunderScript: {0}", type), "ModErrorUnableToInstantiateThunderScript", type.ToString(), e.Message, mod.fullPath));
				return;
			}
			try
			{
				thunderScript.ScriptLoaded(mod);
			}
			catch (Exception e2)
			{
				Debug.LogError(string.Format("{0}[{1}][{2}] Exception during ThunderScript ScriptLoaded for: {3} on mod: {4} in assembly: {5}, {6}", new object[]
				{
					"[ModManager]",
					ModManager.ModLoadEventType.ThunderScript,
					mod.folderName,
					type,
					mod.Name,
					assembly.FullName,
					e2
				}));
				mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, string.Format("Exception during ScriptLoaded on ThunderScript: {0}", type), "ModErrorExceptionThunderScriptLoad", type.ToString(), e2.Message, mod.fullPath));
				return;
			}
			try
			{
				thunderScript.Enable();
			}
			catch (Exception e3)
			{
				Debug.LogError(string.Format("{0}[{1}][{2}] Exception during ThunderScript Enable for: {3} on mod: {4} in assembly: {5}, {6}", new object[]
				{
					"[ModManager]",
					ModManager.ModLoadEventType.ThunderScript,
					mod.folderName,
					type,
					mod.Name,
					assembly.FullName,
					e3
				}));
				mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, string.Format("Exception during Enable on ThunderScript: {0}", type), "ModErrorExceptionThunderScriptEnable", type.ToString(), e3.Message, mod.fullPath));
				return;
			}
			mod.thunderScripts.Add(thunderScript);
			Debug.Log(string.Format("{0}[{1}][{2}] Loaded ThunderScript: {3} on mod: {4} in assembly: {5}", new object[]
			{
				"[ModManager]",
				ModManager.ModLoadEventType.ThunderScript,
				mod.folderName,
				thunderScript.ThunderScriptType,
				mod.Name,
				assembly.FullName
			}));
		}

		/// <summary>
		/// Processes our custom mod attributes, such as ModOption
		/// </summary>
		/// <param name="type"></param>
		/// <param name="mod"></param>
		/// <param name="assembly"></param>
		// Token: 0x0600298D RID: 10637 RVA: 0x0011AA30 File Offset: 0x00118C30
		private static void GetModAttributes(Type type, ModManager.ModData mod, Assembly assembly)
		{
			try
			{
				foreach (MemberInfo member in type.GetMembers((BindingFlags)(-1)))
				{
					ModOption[] options = member.GetCustomAttributes(typeof(ModOption), false) as ModOption[];
					if (options != null && options.Length != 0 && member.IsStatic())
					{
						ModManager.AddModOption(type, mod, assembly, member, options[0]);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("{0}[{1}] Exception when getting mod attributions for Mod: {2} for Type: {3} in assembly: {4}. {5} ", new object[]
				{
					"[ModManager]",
					ModManager.ModLoadEventType.ModOption,
					mod.Name,
					type,
					assembly.FullName,
					e
				}));
				mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Exception when getting mod attributes", "ModErrorExceptionGetAttributes", string.Empty, e.Message, mod.fullPath));
			}
		}

		// Token: 0x0600298E RID: 10638 RVA: 0x0011AB10 File Offset: 0x00118D10
		private static void AddModOption(Type type, ModManager.ModData mod, Assembly assembly, MemberInfo member, ModOption modOption)
		{
			try
			{
				if (member.IsStatic())
				{
					if (string.IsNullOrEmpty(modOption.name))
					{
						modOption.name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Regex.Replace(member.Name, "(\\B[A-Z])", " $1").ToLower());
					}
					modOption.member = member;
					modOption.memberClassType = type;
					modOption.modData = mod;
					modOption.memberDataType = ModOption.GetMemberType(member);
					foreach (ModOptionAttribute modOptionAttribute in (from a in member.GetCustomAttributes(true)
					where a.GetType().IsSubclassOf(typeof(ModOptionAttribute))
					select a).Cast<ModOptionAttribute>())
					{
						if (!(modOptionAttribute.GetType() == typeof(ModOption)))
						{
							modOptionAttribute.modOption = modOption;
							modOptionAttribute.member = member;
							modOptionAttribute.Process();
						}
					}
					mod.modOptions.Add(modOption);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("{0}[{1}] Exception when adding mod option for Mod: {2} for Type: {3} in assembly: {4}. {5} ", new object[]
				{
					"[ModManager]",
					ModManager.ModLoadEventType.ModOption,
					mod.Name,
					type,
					assembly.FullName,
					e
				}));
				mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Exception when adding mod option", "ModErrorExceptionAddModOption", string.Empty, e.Message, mod.fullPath));
			}
		}

		/// <summary>
		/// Tries to find the modData which the given assembly is part of
		/// </summary>
		/// <param name="assembly"></param>
		/// <param name="modData"></param>
		/// <returns>true if it found the modData</returns>
		// Token: 0x0600298F RID: 10639 RVA: 0x0011ACA8 File Offset: 0x00118EA8
		public static bool TryGetModData(Assembly assembly, out ModManager.ModData modData)
		{
			modData = null;
			foreach (ModManager.ModData loadedMod in ModManager.loadedMods)
			{
				if (loadedMod.assemblies.Contains(assembly))
				{
					modData = loadedMod;
					break;
				}
			}
			return modData != null;
		}

		// Token: 0x06002991 RID: 10641 RVA: 0x0011AD30 File Offset: 0x00118F30
		[CompilerGenerated]
		internal static void <LoadModOptions>g__LogError|25_0(Exception ex, ref ModManager.<>c__DisplayClass25_0 A_1, ref ModManager.<>c__DisplayClass25_1 A_2)
		{
			Debug.LogError(string.Format("{0}[{1}][{2}] Error getting types in assembly: {3} | {4}", new object[]
			{
				"[ModManager]",
				ModManager.ModLoadEventType.ModOption,
				A_1.mod.folderName,
				A_2.assembly.Location,
				ex.Message
			}));
			A_1.mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Could not get types in assembly", "ModErrorCouldNotGetTypesInAssembly", string.Empty, ex.Message, A_2.assembly.Location));
			A_1.mod.Incompatible = true;
			ModManager.loadedMods.Add(A_1.mod);
		}

		// Token: 0x06002992 RID: 10642 RVA: 0x0011ADDC File Offset: 0x00118FDC
		[CompilerGenerated]
		internal static void <LoadModThunderScripts>g__LogError|26_0(Exception ex, ref ModManager.<>c__DisplayClass26_0 A_1, ref ModManager.<>c__DisplayClass26_1 A_2)
		{
			Debug.LogError(string.Format("{0}[{1}][{2}] Error processing ThunderScripts in assembly: {3} | {4}", new object[]
			{
				"[ModManager]",
				ModManager.ModLoadEventType.ThunderScript,
				A_1.mod.folderName,
				A_2.assembly.Location,
				ex.Message
			}));
			A_1.mod.errors.Add(new ModManager.ModData.Error(ModManager.ModData.ErrorType.Assembly, "Error processing ThunderScripts in assembly", "ModErrorCouldNotProcessThunderscriptsInAssembly", string.Empty, ex.Message, A_2.assembly.Location));
			A_1.mod.Incompatible = true;
			ModManager.loadedMods.Add(A_1.mod);
		}

		// Token: 0x06002993 RID: 10643 RVA: 0x0011AE88 File Offset: 0x00119088
		[CompilerGenerated]
		internal static bool <GetOrderedMods>g__AddValidMod|30_0(string topLevelModFolder, List<ModManager.ModData> modDatas)
		{
			ModManager.ModData modData;
			if (ModManager.TryReadManifest(topLevelModFolder, out modData))
			{
				modDatas.Add(modData);
				return true;
			}
			return false;
		}

		// Token: 0x0400276C RID: 10092
		public const string debugLine = "[ModManager]";

		// Token: 0x0400276D RID: 10093
		public static string modFolderName = "Mods";

		// Token: 0x0400276E RID: 10094
		public static bool editorLoadAddressableBundles = false;

		// Token: 0x0400276F RID: 10095
		public static bool modCatalogAddressablesLoaded;

		// Token: 0x04002770 RID: 10096
		public static readonly HashSet<ModManager.ModData> loadedMods = new HashSet<ModManager.ModData>();

		// Token: 0x04002771 RID: 10097
		public static bool gameModsLoaded;

		// Token: 0x04002774 RID: 10100
		public static bool isGameModsCatalogRefreshed;

		// Token: 0x02000A66 RID: 2662
		// (Invoke) Token: 0x06004615 RID: 17941
		public delegate void ModLoadEvent(EventTime eventTime, ModManager.ModLoadEventType eventType, ModManager.ModData modData = null);

		// Token: 0x02000A67 RID: 2663
		public enum ModLoadEventType
		{
			// Token: 0x04004819 RID: 18457
			ModManager,
			// Token: 0x0400481A RID: 18458
			Assembly,
			// Token: 0x0400481B RID: 18459
			Addressable,
			// Token: 0x0400481C RID: 18460
			Catalog,
			// Token: 0x0400481D RID: 18461
			ModOption,
			// Token: 0x0400481E RID: 18462
			ThunderScript,
			// Token: 0x0400481F RID: 18463
			AddressableAsset
		}

		// Token: 0x02000A68 RID: 2664
		public class ModData
		{
			// Token: 0x06004618 RID: 17944 RVA: 0x00197814 File Offset: 0x00195A14
			public bool TryGetModOption(string optionName, out ModOption modOption)
			{
				modOption = null;
				if (string.IsNullOrEmpty(optionName))
				{
					return false;
				}
				int modOptionsCount = this.modOptions.Count;
				for (int i = 0; i < modOptionsCount; i++)
				{
					ModOption option = this.modOptions[i];
					if (option.name == optionName)
					{
						modOption = option;
						break;
					}
				}
				return modOption != null;
			}

			// Token: 0x06004619 RID: 17945 RVA: 0x0019786C File Offset: 0x00195A6C
			protected bool Equals(ModManager.ModData other)
			{
				return this.Name == other.Name && this.Description == other.Description && this.Author == other.Author && this.ModVersion == other.ModVersion && this.GameVersion == other.GameVersion && this.Thumbnail == other.Thumbnail;
			}

			// Token: 0x0600461A RID: 17946 RVA: 0x001978EB File Offset: 0x00195AEB
			public override bool Equals(object obj)
			{
				return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((ModManager.ModData)obj)));
			}

			// Token: 0x0600461B RID: 17947 RVA: 0x00197919 File Offset: 0x00195B19
			public override int GetHashCode()
			{
				return HashCode.Combine<string, string, string, string, string, string>(this.Name, this.Description, this.Author, this.ModVersion, this.GameVersion, this.Thumbnail);
			}

			// Token: 0x0600461C RID: 17948 RVA: 0x00197944 File Offset: 0x00195B44
			public static bool operator ==(ModManager.ModData left, ModManager.ModData right)
			{
				return object.Equals(left, right);
			}

			// Token: 0x0600461D RID: 17949 RVA: 0x0019794D File Offset: 0x00195B4D
			public static bool operator !=(ModManager.ModData left, ModManager.ModData right)
			{
				return !object.Equals(left, right);
			}

			// Token: 0x04004820 RID: 18464
			public string Name;

			// Token: 0x04004821 RID: 18465
			public string Description;

			// Token: 0x04004822 RID: 18466
			public string Author;

			// Token: 0x04004823 RID: 18467
			public string ModVersion;

			// Token: 0x04004824 RID: 18468
			public string GameVersion;

			// Token: 0x04004825 RID: 18469
			public string Thumbnail;

			// Token: 0x04004826 RID: 18470
			[NonSerialized]
			public string folderName;

			// Token: 0x04004827 RID: 18471
			[NonSerialized]
			public string fullPath;

			// Token: 0x04004828 RID: 18472
			[NonSerialized]
			public bool Incompatible;

			// Token: 0x04004829 RID: 18473
			[NonSerialized]
			public List<Assembly> assemblies = new List<Assembly>();

			// Token: 0x0400482A RID: 18474
			[NonSerialized]
			public UIModsMenu.ModMenu menu;

			// Token: 0x0400482B RID: 18475
			[NonSerialized]
			public List<ThunderScript> thunderScripts = new List<ThunderScript>();

			// Token: 0x0400482C RID: 18476
			[NonSerialized]
			public List<ModOption> modOptions = new List<ModOption>();

			// Token: 0x0400482D RID: 18477
			[NonSerialized]
			public List<string> contentCatalogPaths = new List<string>();

			// Token: 0x0400482E RID: 18478
			[NonSerialized]
			public List<CatalogData> ownedDatas = new List<CatalogData>();

			// Token: 0x0400482F RID: 18479
			[NonSerialized]
			public List<CatalogData> changedDatas = new List<CatalogData>();

			// Token: 0x04004830 RID: 18480
			[NonSerialized]
			public HashSet<ModManager.ModData.Error> errors = new HashSet<ModManager.ModData.Error>();

			// Token: 0x02000BF4 RID: 3060
			public class Error
			{
				// Token: 0x06004A9B RID: 19099 RVA: 0x001A6BC1 File Offset: 0x001A4DC1
				public Error(ModManager.ModData.ErrorType type, string description, string descriptionLocalizationId, string descriptionExtraInfo, string innerMessage, string filePath)
				{
					this.type = type;
					this.description = description;
					this.descriptionLocalizationId = descriptionLocalizationId;
					this.descriptionExtraInfo = descriptionExtraInfo;
					this.innerMessage = innerMessage;
					this.filePath = filePath;
				}

				// Token: 0x06004A9C RID: 19100 RVA: 0x001A6BF8 File Offset: 0x001A4DF8
				protected bool Equals(ModManager.ModData.Error other)
				{
					return this.type == other.type && this.description == other.description && this.innerMessage == other.innerMessage && this.filePath == other.filePath;
				}

				// Token: 0x06004A9D RID: 19101 RVA: 0x001A6C4C File Offset: 0x001A4E4C
				public override bool Equals(object obj)
				{
					return obj != null && (this == obj || (!(obj.GetType() != base.GetType()) && this.Equals((ModManager.ModData.Error)obj)));
				}

				// Token: 0x06004A9E RID: 19102 RVA: 0x001A6C7A File Offset: 0x001A4E7A
				public override int GetHashCode()
				{
					return HashCode.Combine<int, string, string, string>((int)this.type, this.description, this.innerMessage, this.filePath);
				}

				// Token: 0x06004A9F RID: 19103 RVA: 0x001A6C99 File Offset: 0x001A4E99
				public static bool operator ==(ModManager.ModData.Error left, ModManager.ModData.Error right)
				{
					return object.Equals(left, right);
				}

				// Token: 0x06004AA0 RID: 19104 RVA: 0x001A6CA2 File Offset: 0x001A4EA2
				public static bool operator !=(ModManager.ModData.Error left, ModManager.ModData.Error right)
				{
					return !object.Equals(left, right);
				}

				// Token: 0x04004D76 RID: 19830
				public ModManager.ModData.ErrorType type;

				// Token: 0x04004D77 RID: 19831
				public string description;

				// Token: 0x04004D78 RID: 19832
				public string descriptionLocalizationId;

				// Token: 0x04004D79 RID: 19833
				public string descriptionExtraInfo;

				// Token: 0x04004D7A RID: 19834
				public string innerMessage;

				// Token: 0x04004D7B RID: 19835
				public string filePath;
			}

			// Token: 0x02000BF5 RID: 3061
			public enum ErrorType
			{
				// Token: 0x04004D7D RID: 19837
				JSON,
				// Token: 0x04004D7E RID: 19838
				Catalog,
				// Token: 0x04004D7F RID: 19839
				Assembly,
				// Token: 0x04004D80 RID: 19840
				Manifest,
				// Token: 0x04004D81 RID: 19841
				Option,
				// Token: 0x04004D82 RID: 19842
				ThunderScript
			}
		}

		// Token: 0x02000A69 RID: 2665
		public class LoadOrder
		{
			// Token: 0x04004831 RID: 18481
			public List<string> modNames;
		}
	}
}
