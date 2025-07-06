using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ThunderRoad
{
	// Token: 0x02000287 RID: 647
	[Serializable]
	public class EffectBundle
	{
		// Token: 0x06001E8D RID: 7821 RVA: 0x000CFC37 File Offset: 0x000CDE37
		public EffectBundle Clone()
		{
			EffectBundle effectBundle = base.MemberwiseClone() as EffectBundle;
			effectBundle.ignoredEffects = new List<string>(this.ignoredEffects);
			return effectBundle;
		}

		// Token: 0x06001E8E RID: 7822 RVA: 0x000CFC58 File Offset: 0x000CDE58
		public void OnCatalogRefresh()
		{
			if (this.effectId != null && this.effectId != "")
			{
				this.effectData = Catalog.GetData<EffectData>(this.effectId, true);
			}
			if (this.ignoredEffects != null)
			{
				List<Type> types = new List<Type>();
				foreach (string ignoreEffect in this.ignoredEffects)
				{
					types.Add(Type.GetType(ignoreEffect));
				}
				this.ignoredEffectTypes = types.ToArray();
			}
		}

		// Token: 0x06001E8F RID: 7823 RVA: 0x000CFCF8 File Offset: 0x000CDEF8
		public List<ValueDropdownItem<string>> GetAllEffetModuleTypes()
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>();
			dropdownList.Add(new ValueDropdownItem<string>("None", null));
			foreach (Type type in typeof(EffectModule).Assembly.GetTypes())
			{
				if (type.IsSubclassOf(typeof(EffectModule)))
				{
					dropdownList.Add(new ValueDropdownItem<string>(type.ToString(), type.ToString()));
				}
			}
			return dropdownList;
		}

		// Token: 0x06001E90 RID: 7824 RVA: 0x000CFD6D File Offset: 0x000CDF6D
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x04001D1A RID: 7450
		public string effectId;

		// Token: 0x04001D1B RID: 7451
		[NonSerialized]
		public EffectData effectData;

		// Token: 0x04001D1C RID: 7452
		public List<string> ignoredEffects;

		// Token: 0x04001D1D RID: 7453
		[NonSerialized]
		public Type[] ignoredEffectTypes;
	}
}
