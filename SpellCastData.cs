using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000226 RID: 550
	[Serializable]
	public class SpellCastData : SpellData
	{
		// Token: 0x17000160 RID: 352
		// (get) Token: 0x0600173A RID: 5946 RVA: 0x0009C450 File Offset: 0x0009A650
		[HideInInspector]
		public int? Order
		{
			get
			{
				if (!this.hasOrder)
				{
					return null;
				}
				return new int?(this.order);
			}
		}

		// Token: 0x0600173B RID: 5947 RVA: 0x0009C47A File Offset: 0x0009A67A
		public new SpellCastData Clone()
		{
			return base.MemberwiseClone() as SpellCastData;
		}

		// Token: 0x0600173C RID: 5948 RVA: 0x0009C487 File Offset: 0x0009A687
		public virtual void Load(SpellCaster spellCaster)
		{
			this.CountAndLoadSkillPassives(spellCaster.ragdollHand.creature);
		}

		// Token: 0x0600173D RID: 5949 RVA: 0x0009C49A File Offset: 0x0009A69A
		public void CountAndLoadSkillPassives(Creature creature)
		{
			if (creature != null)
			{
				this.LoadSkillPassives(creature.CountSkillsOfTree(this.primarySkillTreeId, false, false));
			}
		}

		// Token: 0x0600173E RID: 5950 RVA: 0x0009C4B9 File Offset: 0x0009A6B9
		public virtual void LoadSkillPassives(int skillCount)
		{
		}

		// Token: 0x0600173F RID: 5951 RVA: 0x0009C4BB File Offset: 0x0009A6BB
		public virtual void FixedUpdateCaster()
		{
		}

		// Token: 0x06001740 RID: 5952 RVA: 0x0009C4BD File Offset: 0x0009A6BD
		public virtual void UpdateCaster()
		{
		}

		// Token: 0x06001741 RID: 5953 RVA: 0x0009C4BF File Offset: 0x0009A6BF
		public virtual void Fire(bool active)
		{
		}

		// Token: 0x06001742 RID: 5954 RVA: 0x0009C4C1 File Offset: 0x0009A6C1
		public virtual void FireAxis(float value)
		{
		}

		// Token: 0x06001743 RID: 5955 RVA: 0x0009C4C3 File Offset: 0x0009A6C3
		public virtual void Unload()
		{
			base.ClearModifiers();
		}

		// Token: 0x06001744 RID: 5956 RVA: 0x0009C4CB File Offset: 0x0009A6CB
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			if (!string.IsNullOrEmpty(this.iconEffectId))
			{
				this.iconEffectData = Catalog.GetData<EffectData>(this.iconEffectId, true);
			}
		}

		// Token: 0x06001745 RID: 5957 RVA: 0x0009C4F2 File Offset: 0x0009A6F2
		public override int GetCurrentVersion()
		{
			return 0;
		}

		// Token: 0x040016AA RID: 5802
		public string wheelDisplayName;

		// Token: 0x040016AB RID: 5803
		public bool hasOrder;

		// Token: 0x040016AC RID: 5804
		public int order;

		// Token: 0x040016AD RID: 5805
		public string iconEffectId;

		// Token: 0x040016AE RID: 5806
		[NonSerialized]
		public EffectData iconEffectData;

		// Token: 0x040016AF RID: 5807
		public SpellCastData.CastType aiCastType;

		// Token: 0x040016B0 RID: 5808
		public float loopMaxDuration = 5f;

		// Token: 0x040016B1 RID: 5809
		public float aiCastMinDistance = 1f;

		// Token: 0x040016B2 RID: 5810
		public float aiCastMaxDistance = 20f;

		// Token: 0x040016B3 RID: 5811
		public float aiCastGestureLength = 0.7f;

		// Token: 0x040016B4 RID: 5812
		public float minMana = 5f;

		// Token: 0x0200083E RID: 2110
		public enum CastType
		{
			// Token: 0x040040F6 RID: 16630
			CastSimple,
			// Token: 0x040040F7 RID: 16631
			CastLoop
		}
	}
}
