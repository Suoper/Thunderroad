using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200016D RID: 365
	public class CatalogCategory
	{
		// Token: 0x06001243 RID: 4675 RVA: 0x00082DA8 File Offset: 0x00080FA8
		public CatalogCategory(Category category)
		{
			this.category = category;
		}

		// Token: 0x06001244 RID: 4676 RVA: 0x00082DF5 File Offset: 0x00080FF5
		public void Clear()
		{
			this.catalogDatas.Clear();
			this.idToHashId.Clear();
			this.idToHashIdCaseSensitive.Clear();
			this.hashIdToCatalogData.Clear();
		}

		// Token: 0x06001245 RID: 4677 RVA: 0x00082E24 File Offset: 0x00081024
		public bool AddCatalogData(CatalogData catalogData)
		{
			this.idToHashId.TryAdd(catalogData.id, catalogData.hashId);
			this.idToHashIdCaseSensitive.TryAdd(catalogData.id, catalogData.hashId);
			if (this.hashIdToCatalogData.TryAdd(catalogData.hashId, catalogData))
			{
				this.catalogDatas.Add(catalogData);
				return true;
			}
			Debug.Log(string.Format("CatalogData: {0} - {1} has already been added to catalog.", catalogData.id, catalogData.hashId));
			return false;
		}

		/// <summary>
		/// Returns a hash of the lower case version of the ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		// Token: 0x06001246 RID: 4678 RVA: 0x00082EA4 File Offset: 0x000810A4
		public int GetHashId(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return -1;
			}
			int value;
			if (!this.idToHashIdCaseSensitive.TryGetValue(id, out value))
			{
				if (!this.idToHashId.TryGetValue(id, out value))
				{
					value = Animator.StringToHash(id.ToLower());
					this.idToHashId.Add(id, value);
				}
				this.idToHashIdCaseSensitive.Add(id, value);
			}
			return value;
		}

		// Token: 0x06001247 RID: 4679 RVA: 0x00082F02 File Offset: 0x00081102
		public bool TryGetHashId(string id, out int hashId)
		{
			hashId = this.GetHashId(id);
			return hashId != -1;
		}

		// Token: 0x06001248 RID: 4680 RVA: 0x00082F15 File Offset: 0x00081115
		public bool TryGetCatalogData(int hashId, out CatalogData catalogData)
		{
			catalogData = this.GetCatalogData(hashId);
			return catalogData != null;
		}

		// Token: 0x06001249 RID: 4681 RVA: 0x00082F25 File Offset: 0x00081125
		public bool TryGetCatalogData(string id, out CatalogData catalogData)
		{
			catalogData = this.GetCatalogData(id);
			return catalogData != null;
		}

		// Token: 0x0600124A RID: 4682 RVA: 0x00082F38 File Offset: 0x00081138
		public CatalogData GetCatalogData(string id)
		{
			int hashId;
			if (this.TryGetHashId(id, out hashId))
			{
				return this.GetCatalogData(hashId);
			}
			return null;
		}

		// Token: 0x0600124B RID: 4683 RVA: 0x00082F5C File Offset: 0x0008115C
		public CatalogData GetCatalogData(int hashId)
		{
			CatalogData catalogData;
			if (this.hashIdToCatalogData.TryGetValue(hashId, out catalogData))
			{
				return catalogData;
			}
			return null;
		}

		// Token: 0x04001048 RID: 4168
		public Category category;

		// Token: 0x04001049 RID: 4169
		public List<CatalogData> catalogDatas = new List<CatalogData>(100);

		// Token: 0x0400104A RID: 4170
		private Dictionary<string, int> idToHashId = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

		// Token: 0x0400104B RID: 4171
		private Dictionary<string, int> idToHashIdCaseSensitive = new Dictionary<string, int>();

		// Token: 0x0400104C RID: 4172
		private Dictionary<int, CatalogData> hashIdToCatalogData = new Dictionary<int, CatalogData>();
	}
}
