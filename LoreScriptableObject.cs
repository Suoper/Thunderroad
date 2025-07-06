using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200030C RID: 780
	[CreateAssetMenu(menuName = "ThunderRoad/Lore/Lore")]
	public class LoreScriptableObject : ScriptableObject
	{
		// Token: 0x06002529 RID: 9513 RVA: 0x000FEF18 File Offset: 0x000FD118
		public LoreScriptableObject.LorePack GetPack(int hashId)
		{
			if (this._hashIdToLorePack == null)
			{
				this._hashIdToLorePack = new Dictionary<int, LoreScriptableObject.LorePack>();
				for (int i = 0; i < this.allLorePacks.Length; i++)
				{
					this._hashIdToLorePack.Add(this.allLorePacks[i].hashId, this.allLorePacks[i]);
				}
			}
			LoreScriptableObject.LorePack value;
			if (this._hashIdToLorePack.TryGetValue(hashId, out value))
			{
				return value;
			}
			return null;
		}

		/// <summary>
		/// Get the lore hash id of a specific lore pack from the lore tree 
		/// </summary>
		/// <param name="lorePackName">Lore pack name must be composed by the lore pack folder name</param>
		/// <returns></returns>
		// Token: 0x0600252A RID: 9514 RVA: 0x000FEF7E File Offset: 0x000FD17E
		public static int GetLoreHashId(string lorePackName)
		{
			return Animator.StringToHash(lorePackName);
		}

		// Token: 0x040024AA RID: 9386
		public LoreDisplayTypeScriptableObject displayType;

		// Token: 0x040024AB RID: 9387
		public LoreScriptableObject.LorePack[] allLorePacks;

		// Token: 0x040024AC RID: 9388
		public List<int> rootLoreHashIds;

		// Token: 0x040024AD RID: 9389
		public int loreId;

		// Token: 0x040024AE RID: 9390
		public int changeCheckHash;

		// Token: 0x040024AF RID: 9391
		public HashSet<string> uniqueRequiredParamsInPack = new HashSet<string>();

		// Token: 0x040024B0 RID: 9392
		public HashSet<string> uniqueLevelOptionsInPack = new HashSet<string>();

		// Token: 0x040024B1 RID: 9393
		private Dictionary<int, LoreScriptableObject.LorePack> _hashIdToLorePack;

		// Token: 0x02000A07 RID: 2567
		public enum LoreType
		{
			// Token: 0x040046C3 RID: 18115
			text,
			// Token: 0x040046C4 RID: 18116
			texture
		}

		// Token: 0x02000A08 RID: 2568
		[Serializable]
		public class LoreData
		{
			// Token: 0x040046C5 RID: 18117
			public string groupId;

			// Token: 0x040046C6 RID: 18118
			public string titleId;

			// Token: 0x040046C7 RID: 18119
			public string loreId;

			// Token: 0x040046C8 RID: 18120
			public string itemId;

			// Token: 0x040046C9 RID: 18121
			public LoreScriptableObject.LoreType loreType;

			// Token: 0x040046CA RID: 18122
			public string contentAddress;

			// Token: 0x040046CB RID: 18123
			public bool displayGraphicsInJournal;
		}

		// Token: 0x02000A09 RID: 2569
		[Serializable]
		public class LorePack
		{
			// Token: 0x040046CC RID: 18124
			public int hashId;

			// Token: 0x040046CD RID: 18125
			[Header("Group name translate")]
			public string groupId;

			// Token: 0x040046CE RID: 18126
			public string nameId;

			// Token: 0x040046CF RID: 18127
			[Header("LoreRequirement")]
			public List<int> loreRequirement;

			// Token: 0x040046D0 RID: 18128
			[Header("Conditions")]
			public LorePackCondition lorePackCondition;

			// Token: 0x040046D1 RID: 18129
			public bool spawnPackAsOneItem;

			// Token: 0x040046D2 RID: 18130
			[Header("Lore Data")]
			public List<LoreScriptableObject.LoreData> loreData;
		}
	}
}
