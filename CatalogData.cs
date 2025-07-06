using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200016E RID: 366
	[Serializable]
	public class CatalogData
	{
		// Token: 0x0600124C RID: 4684 RVA: 0x00082F7C File Offset: 0x0008117C
		public virtual string SortKey()
		{
			return this.id;
		}

		// Token: 0x0600124D RID: 4685 RVA: 0x00082F84 File Offset: 0x00081184
		public string GetLatestOverrideFolder()
		{
			if (string.IsNullOrEmpty(this.sourceFolders))
			{
				return GameSettings.loadDefaultFolders.Last<string>();
			}
			return this.sourceFolders.Split('+', StringSplitOptions.None).Last<string>();
		}

		// Token: 0x17000125 RID: 293
		// (get) Token: 0x0600124E RID: 4686 RVA: 0x00082FB1 File Offset: 0x000811B1
		private bool showOwnershipFields
		{
			get
			{
				return this.standaloneData && ModManager.gameModsLoaded;
			}
		}

		// Token: 0x0600124F RID: 4687 RVA: 0x00082FC4 File Offset: 0x000811C4
		public List<ValueDropdownItem<string>> GetJsonFolders()
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>();
			foreach (string folderName in FileManager.GetFolderNames(FileManager.Type.JSONCatalog, FileManager.Source.Default, ""))
			{
				if (!(folderName == ".git"))
				{
					dropdownList.Add(new ValueDropdownItem<string>(folderName, folderName));
				}
			}
			return dropdownList;
		}

		// Token: 0x06001250 RID: 4688 RVA: 0x00083011 File Offset: 0x00081211
		public virtual int GetCurrentVersion()
		{
			return 0;
		}

		// Token: 0x06001251 RID: 4689 RVA: 0x00083014 File Offset: 0x00081214
		public virtual void Init()
		{
			this.hashId = Animator.StringToHash(this.id.ToLower());
		}

		// Token: 0x06001252 RID: 4690 RVA: 0x0008302C File Offset: 0x0008122C
		public virtual void OnEditorSave()
		{
		}

		// Token: 0x06001253 RID: 4691 RVA: 0x0008302E File Offset: 0x0008122E
		public virtual void OnCatalogRefresh()
		{
			this.version = this.GetCurrentVersion();
			this.standaloneData = true;
		}

		// Token: 0x06001254 RID: 4692 RVA: 0x00083043 File Offset: 0x00081243
		public virtual CatalogData Clone()
		{
			return base.MemberwiseClone() as CatalogData;
		}

		// Token: 0x06001255 RID: 4693 RVA: 0x00083050 File Offset: 0x00081250
		public virtual IEnumerator OnCatalogRefreshCoroutine()
		{
			return null;
		}

		// Token: 0x06001256 RID: 4694 RVA: 0x00083053 File Offset: 0x00081253
		public virtual IEnumerator LoadAddressableAssetsCoroutine()
		{
			return null;
		}

		// Token: 0x06001257 RID: 4695 RVA: 0x00083056 File Offset: 0x00081256
		public virtual void ReleaseAddressableAssets()
		{
		}

		// Token: 0x06001258 RID: 4696 RVA: 0x00083058 File Offset: 0x00081258
		public virtual void OnLanguageChanged(string language)
		{
		}

		// Token: 0x0400104D RID: 4173
		[NonSerialized]
		public bool standaloneData;

		// Token: 0x0400104E RID: 4174
		[JsonProperty(Order = -2)]
		public string id;

		// Token: 0x0400104F RID: 4175
		[JsonProperty(Order = -2)]
		public BuildSettings.ContentFlag sensitiveContent;

		// Token: 0x04001050 RID: 4176
		[JsonProperty(Order = -2)]
		public BuildSettings.ContentFlagBehaviour sensitiveFilterBehaviour;

		// Token: 0x04001051 RID: 4177
		[JsonProperty(Order = -2)]
		[NonSerialized]
		public string sourceFolders;

		// Token: 0x04001052 RID: 4178
		[JsonProperty(Order = -2)]
		[NonSerialized]
		public int hashId;

		// Token: 0x04001053 RID: 4179
		[JsonProperty(Order = -2)]
		public int version;

		// Token: 0x04001054 RID: 4180
		public string groupPath;

		// Token: 0x04001055 RID: 4181
		[NonSerialized]
		public string filePath;

		// Token: 0x04001056 RID: 4182
		[NonSerialized]
		public ModManager.ModData owner;

		// Token: 0x04001057 RID: 4183
		[NonSerialized]
		public List<ModManager.ModData> changers = new List<ModManager.ModData>();
	}
}
