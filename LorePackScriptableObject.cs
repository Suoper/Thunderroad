using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200030B RID: 779
	[CreateAssetMenu(menuName = "ThunderRoad/Lore/Lore Part pack")]
	public class LorePackScriptableObject : ScriptableObject
	{
		// Token: 0x06002525 RID: 9509 RVA: 0x000FEE00 File Offset: 0x000FD000
		public List<ValueDropdownItem<string>> GetAllItemId()
		{
			return new List<ValueDropdownItem<string>>
			{
				new ValueDropdownItem<string>("Note", "Note"),
				new ValueDropdownItem<string>("DalgarianTablet", "DalgTablet"),
				new ValueDropdownItem<string>("Notepad", "Notepad"),
				new ValueDropdownItem<string>("NotepadOutlaw", "NotepadOutlaw"),
				new ValueDropdownItem<string>("NotepadWildfolk", "NotepadWildfolk"),
				new ValueDropdownItem<string>("NotepadEraden", "NotepadEradenCommon"),
				new ValueDropdownItem<string>("NotepadEradenRoyal", "NotepadEradenRoyal"),
				new ValueDropdownItem<string>("NotepadTheEye", "NotepadTheEyeCommon"),
				new ValueDropdownItem<string>("NotepadTheEyePropaganda", "NotepadTheEyePropaganda"),
				new ValueDropdownItem<string>("NotepadDalgarianSociety", "NotepadDalgarianSociety")
			};
		}

		// Token: 0x06002526 RID: 9510 RVA: 0x000FEEE4 File Offset: 0x000FD0E4
		public List<ValueDropdownItem<string>> GetAllTextGroupID()
		{
			return Catalog.GetTextData().GetDropdownAllTextGroups();
		}

		// Token: 0x06002527 RID: 9511 RVA: 0x000FEEF0 File Offset: 0x000FD0F0
		public List<ValueDropdownItem<string>> GetAllTextId()
		{
			return Catalog.GetTextData().GetDropdownAllTexts(this.groupId);
		}

		// Token: 0x040024A2 RID: 9378
		[Header("Localization Ids")]
		public string groupId;

		// Token: 0x040024A3 RID: 9379
		public string nameId;

		// Token: 0x040024A4 RID: 9380
		[Tooltip("index use to define the position of thhe pack in the journal")]
		public int journalIndex;

		// Token: 0x040024A5 RID: 9381
		[Header("Requirement")]
		[Tooltip("lore that need to be read before this one")]
		public List<LorePackScriptableObject> loreRequirements;

		// Token: 0x040024A6 RID: 9382
		[Header("Conditions")]
		public LorePackCondition condition;

		// Token: 0x040024A7 RID: 9383
		public string itemId = "Note";

		// Token: 0x040024A8 RID: 9384
		public bool spawnPackAsSingleItem;

		// Token: 0x040024A9 RID: 9385
		[Header("Lore Item Data")]
		public List<LorePackScriptableObject.LoreItemData> loreData;

		// Token: 0x02000A06 RID: 2566
		[Serializable]
		public class LoreItemData
		{
			// Token: 0x06004517 RID: 17687 RVA: 0x001952AD File Offset: 0x001934AD
			public List<ValueDropdownItem<string>> GetAllTextGroupID()
			{
				return Catalog.GetTextData().GetDropdownAllTextGroups();
			}

			// Token: 0x06004518 RID: 17688 RVA: 0x001952B9 File Offset: 0x001934B9
			public List<ValueDropdownItem<string>> GetAllTextId()
			{
				return Catalog.GetTextData().GetDropdownAllTexts(this.groupId);
			}

			// Token: 0x040046BC RID: 18108
			public string groupId;

			// Token: 0x040046BD RID: 18109
			public string titleId;

			// Token: 0x040046BE RID: 18110
			public string loreId;

			// Token: 0x040046BF RID: 18111
			public LoreScriptableObject.LoreType loreType;

			// Token: 0x040046C0 RID: 18112
			public string contentType = "Bas.UI.DefaultNoteContent";

			// Token: 0x040046C1 RID: 18113
			[Tooltip("Display any sprites from the pack in the journal?")]
			public bool displayGraphicsInJournal;
		}
	}
}
