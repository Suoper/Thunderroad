using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000222 RID: 546
	[Serializable]
	public class SkillTreeData : CatalogData
	{
		// Token: 0x06001701 RID: 5889 RVA: 0x0009A674 File Offset: 0x00098874
		public List<ValueDropdownItem<string>> GetAllItemIds()
		{
			return Catalog.GetDropdownAllID(Category.Item, "None");
		}

		// Token: 0x06001702 RID: 5890 RVA: 0x0009A681 File Offset: 0x00098881
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			this.crystalItemData = Catalog.GetData<ItemData>(this.crystalItemId, true);
		}

		// Token: 0x0400162A RID: 5674
		public string localizationGroupID;

		// Token: 0x0400162B RID: 5675
		public string displayName;

		// Token: 0x0400162C RID: 5676
		public string description;

		// Token: 0x0400162D RID: 5677
		public int maxTier = 3;

		// Token: 0x0400162E RID: 5678
		public bool showInInfuser = true;

		// Token: 0x0400162F RID: 5679
		public Color color = Color.white;

		// Token: 0x04001630 RID: 5680
		[ColorUsage(true, true)]
		public Color emissionColor = Color.white;

		// Token: 0x04001631 RID: 5681
		public float orbPitch = 1f;

		// Token: 0x04001632 RID: 5682
		public float costMultiplier = 1f;

		// Token: 0x04001633 RID: 5683
		public string crystalItemId;

		// Token: 0x04001634 RID: 5684
		public string orbIconAddress;

		// Token: 0x04001635 RID: 5685
		public string infuserTopParticleAddress;

		// Token: 0x04001636 RID: 5686
		public string infuserBeamAudioAddress;

		// Token: 0x04001637 RID: 5687
		public string musicAddress;

		// Token: 0x04001638 RID: 5688
		public string videoAddress;

		// Token: 0x04001639 RID: 5689
		public string iconEnabledAddress;

		// Token: 0x0400163A RID: 5690
		public string iconDisabledAddress;

		// Token: 0x0400163B RID: 5691
		[NonSerialized]
		public ItemData crystalItemData;
	}
}
