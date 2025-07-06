using System;
using System.Collections.Generic;

namespace ThunderRoad
{
	// Token: 0x02000227 RID: 551
	[Serializable]
	public class SpellData : SkillData, IContainerLoadable<SpellData>
	{
		// Token: 0x06001747 RID: 5959 RVA: 0x0009C534 File Offset: 0x0009A734
		public virtual void DrawGizmos()
		{
		}

		// Token: 0x06001748 RID: 5960 RVA: 0x0009C536 File Offset: 0x0009A736
		public virtual void DrawGizmosSelected()
		{
		}

		// Token: 0x06001749 RID: 5961 RVA: 0x0009C538 File Offset: 0x0009A738
		public override ContainerContent InstanceContent()
		{
			return new SpellContent(this);
		}

		// Token: 0x0600174A RID: 5962 RVA: 0x0009C540 File Offset: 0x0009A740
		public override void Init()
		{
			base.Init();
			this.SetupModifiers();
		}

		// Token: 0x0600174B RID: 5963 RVA: 0x0009C54E File Offset: 0x0009A74E
		public override CatalogData Clone()
		{
			CatalogData result = base.Clone();
			this.SetupModifiers();
			return result;
		}

		// Token: 0x0600174C RID: 5964 RVA: 0x0009C55C File Offset: 0x0009A75C
		public string SkillPassiveLabel(string name, float value)
		{
			return string.Format("{0} Mult. per Skill (max {1}%)", name, value * 3f * 100f);
		}

		// Token: 0x0600174D RID: 5965 RVA: 0x0009C57B File Offset: 0x0009A77B
		public void SetupModifiers()
		{
			this.modifiers = new Dictionary<int, Dictionary<object, float>>();
			this.multipliers = new Dictionary<int, float>();
		}

		/// <summary>
		/// Add a default modifier to a spell.
		/// </summary>
		/// <param name="handler">Owner of this modifier</param>
		/// <param name="modifier">One of the three basic modifier types</param>
		/// <param name="value">Modifier value</param>
		// Token: 0x0600174E RID: 5966 RVA: 0x0009C593 File Offset: 0x0009A793
		public void AddModifier(object handler, Modifier modifier, float value)
		{
			if (!this.modifiers.ContainsKey((int)modifier))
			{
				this.modifiers[(int)modifier] = new Dictionary<object, float>();
			}
			this.modifiers[(int)modifier][handler] = value;
			this.RefreshMultiplier((int)modifier);
		}

		/// <summary>
		/// Add a custom modifier to a spell.
		/// </summary>
		/// <param name="handler">Owner of this modifier</param>
		/// <param name="modifier">Hash of the modifier name</param>
		/// <param name="value">Modifier value</param>
		// Token: 0x0600174F RID: 5967 RVA: 0x0009C5CE File Offset: 0x0009A7CE
		public void AddModifier(object handler, int modifier, float value)
		{
			if (!this.modifiers.ContainsKey(modifier))
			{
				this.modifiers[modifier] = new Dictionary<object, float>();
			}
			this.modifiers[modifier][handler] = value;
			this.RefreshMultiplier(modifier);
		}

		// Token: 0x06001750 RID: 5968 RVA: 0x0009C60C File Offset: 0x0009A80C
		public void RefreshMultiplier(int modifier)
		{
			if (!this.modifiers.ContainsKey(modifier))
			{
				this.multipliers[modifier] = 1f;
				return;
			}
			float output = 1f;
			foreach (float mult in this.modifiers[modifier].Values)
			{
				output *= mult;
			}
			this.multipliers[modifier] = output;
		}

		// Token: 0x06001751 RID: 5969 RVA: 0x0009C69C File Offset: 0x0009A89C
		public void RemoveModifier(object handler, Modifier modifier)
		{
			Dictionary<object, float> values;
			if (this.modifiers.TryGetValue((int)modifier, out values) && values.Remove(handler))
			{
				this.RefreshMultiplier((int)modifier);
			}
		}

		// Token: 0x06001752 RID: 5970 RVA: 0x0009C6CC File Offset: 0x0009A8CC
		public void RemoveModifier(object handler, int modifier)
		{
			Dictionary<object, float> values;
			if (this.modifiers.TryGetValue(modifier, out values) && values.Remove(handler))
			{
				this.RefreshMultiplier(modifier);
			}
		}

		// Token: 0x06001753 RID: 5971 RVA: 0x0009C6FC File Offset: 0x0009A8FC
		public void RemoveModifiers(object handler)
		{
			foreach (KeyValuePair<int, Dictionary<object, float>> kvp in this.modifiers)
			{
				if (kvp.Value.Remove(handler))
				{
					this.RefreshMultiplier(kvp.Key);
				}
			}
		}

		// Token: 0x06001754 RID: 5972 RVA: 0x0009C764 File Offset: 0x0009A964
		public float GetModifier(int modifierHash)
		{
			float value;
			if (!this.multipliers.TryGetValue(modifierHash, out value))
			{
				return 1f;
			}
			return value;
		}

		// Token: 0x06001755 RID: 5973 RVA: 0x0009C788 File Offset: 0x0009A988
		public float GetModifier(Modifier modifier)
		{
			float value;
			if (!this.multipliers.TryGetValue((int)modifier, out value))
			{
				return 1f;
			}
			return value;
		}

		// Token: 0x06001756 RID: 5974 RVA: 0x0009C7AC File Offset: 0x0009A9AC
		public void ClearModifiers()
		{
			this.modifiers.Clear();
			this.multipliers.Clear();
		}

		// Token: 0x040016B5 RID: 5813
		[NonSerialized]
		public Dictionary<int, Dictionary<object, float>> modifiers;

		// Token: 0x040016B6 RID: 5814
		[NonSerialized]
		public Dictionary<int, float> multipliers;
	}
}
