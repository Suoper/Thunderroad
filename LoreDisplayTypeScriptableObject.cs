using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000309 RID: 777
	[CreateAssetMenu(menuName = "ThunderRoad/Lore/LoreDisplayType")]
	public class LoreDisplayTypeScriptableObject : ScriptableObject
	{
		// Token: 0x0600251E RID: 9502 RVA: 0x000FEB23 File Offset: 0x000FCD23
		public string GetNameKey()
		{
			return this.groupID + "/" + this.displayName;
		}

		/// <summary>
		/// Can be deleted after we have some specified, just used to generate dummy data.
		/// See: LoreModule @ 233 (GetAllDisplayType)
		/// If it's not using dummy data this can be deleted.
		/// </summary>
		// Token: 0x0600251F RID: 9503 RVA: 0x000FEB3B File Offset: 0x000FCD3B
		public static LoreDisplayTypeScriptableObject Create(string displayName, Color32 colour)
		{
			LoreDisplayTypeScriptableObject loreDisplayTypeScriptableObject = ScriptableObject.CreateInstance<LoreDisplayTypeScriptableObject>();
			loreDisplayTypeScriptableObject.groupID = "Default";
			loreDisplayTypeScriptableObject.displayName = displayName;
			loreDisplayTypeScriptableObject.colour = colour;
			return loreDisplayTypeScriptableObject;
		}

		// Token: 0x06002520 RID: 9504 RVA: 0x000FEB5B File Offset: 0x000FCD5B
		public List<ValueDropdownItem<string>> GetAllTextGroupID()
		{
			return Catalog.GetTextData().GetDropdownAllTextGroups();
		}

		// Token: 0x06002521 RID: 9505 RVA: 0x000FEB67 File Offset: 0x000FCD67
		public List<ValueDropdownItem<string>> GetAllTextId()
		{
			return Catalog.GetTextData().GetDropdownAllTexts(this.groupID);
		}

		// Token: 0x0400249A RID: 9370
		public string groupID;

		// Token: 0x0400249B RID: 9371
		public string displayName;

		// Token: 0x0400249C RID: 9372
		public Color32 colour;

		// Token: 0x0400249D RID: 9373
		[Tooltip("Display this pack as dalgarian in the journal?")]
		public bool displayInJournalAsDalgarian;

		// Token: 0x0400249E RID: 9374
		public int factionOrderIndex;
	}
}
