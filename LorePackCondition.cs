using System;
using System.Collections.Generic;

namespace ThunderRoad
{
	// Token: 0x0200030A RID: 778
	[Serializable]
	public class LorePackCondition
	{
		// Token: 0x06002523 RID: 9507 RVA: 0x000FEB84 File Offset: 0x000FCD84
		public LorePackCondition(LorePackCondition condition)
		{
			if (condition.visibilityRequired != null)
			{
				int count = condition.visibilityRequired.Count;
				this.visibilityRequired = new List<LorePackCondition.Visibility>(count);
				for (int i = 0; i < count; i++)
				{
					this.visibilityRequired.Add(condition.visibilityRequired[i]);
				}
			}
			else
			{
				this.visibilityRequired = null;
			}
			if (condition.levelOptions != null)
			{
				this.levelOptions = new LorePackCondition.LoreLevelOptionCondition[condition.levelOptions.Length];
				for (int j = 0; j < this.levelOptions.Length; j++)
				{
					this.levelOptions[j] = condition.levelOptions[j];
				}
			}
			else
			{
				this.levelOptions = null;
			}
			if (condition.requiredParameters != null)
			{
				this.requiredParameters = new string[condition.requiredParameters.Length];
				for (int k = 0; k < this.requiredParameters.Length; k++)
				{
					this.requiredParameters[k] = condition.requiredParameters[k];
				}
				return;
			}
			this.requiredParameters = null;
		}

		// Token: 0x06002524 RID: 9508 RVA: 0x000FEC70 File Offset: 0x000FCE70
		public bool IsValid(LorePackCondition.Visibility visibility, string[] validationRequiredParameters, string[] validationOptionalParameters)
		{
			if (this.visibilityRequired != null && this.visibilityRequired.Count > 0)
			{
				int count = this.visibilityRequired.Count;
				bool isVisibilityValid = false;
				for (int i = 0; i < count; i++)
				{
					if (this.visibilityRequired[i] == visibility)
					{
						isVisibilityValid = true;
						break;
					}
				}
				if (!isVisibilityValid)
				{
					return false;
				}
			}
			if (this.levelOptions != null && this.levelOptions.Length != 0)
			{
				if (Level.current == null)
				{
					return false;
				}
				Dictionary<string, string> options = Level.current.options;
				if (options == null)
				{
					return false;
				}
				for (int j = 0; j < this.levelOptions.Length; j++)
				{
					string levelOptionValue;
					if (!options.TryGetValue(this.levelOptions[j].key, out levelOptionValue))
					{
						return false;
					}
					if (!this.levelOptions[j].CheckValue(levelOptionValue))
					{
						return false;
					}
				}
			}
			List<string> requiredParametersCopy = new List<string>(this.requiredParameters);
			if (validationRequiredParameters != null && validationRequiredParameters.Length != 0)
			{
				foreach (string requiredParam in validationRequiredParameters)
				{
					int indexFound = -1;
					int count2 = requiredParametersCopy.Count;
					for (int l = 0; l < count2; l++)
					{
						if (string.Equals(requiredParam, requiredParametersCopy[l]))
						{
							indexFound = l;
							break;
						}
					}
					if (indexFound < 0)
					{
						return false;
					}
					requiredParametersCopy.RemoveAt(indexFound);
				}
			}
			int countParameters = requiredParametersCopy.Count;
			for (int m = 0; m < countParameters; m++)
			{
				string requiredParam2 = requiredParametersCopy[m];
				bool isParam = false;
				for (int n = 0; n < validationOptionalParameters.Length; n++)
				{
					if (string.Equals(requiredParam2, validationOptionalParameters[n]))
					{
						isParam = true;
						break;
					}
				}
				if (!isParam)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0400249F RID: 9375
		public List<LorePackCondition.Visibility> visibilityRequired;

		// Token: 0x040024A0 RID: 9376
		public LorePackCondition.LoreLevelOptionCondition[] levelOptions;

		// Token: 0x040024A1 RID: 9377
		public string[] requiredParameters;

		// Token: 0x02000A04 RID: 2564
		public enum Visibility
		{
			// Token: 0x040046B5 RID: 18101
			Hidden,
			// Token: 0x040046B6 RID: 18102
			PartiallyHidden,
			// Token: 0x040046B7 RID: 18103
			Visibile,
			// Token: 0x040046B8 RID: 18104
			VeryVisibile
		}

		// Token: 0x02000A05 RID: 2565
		[Serializable]
		public class LoreLevelOptionCondition
		{
			// Token: 0x06004515 RID: 17685 RVA: 0x001951E0 File Offset: 0x001933E0
			public bool CheckValue(string levelOptionValue)
			{
				switch (this.comparison)
				{
				case LorePackCondition.LoreLevelOptionCondition.ComparisonType.StringEquals:
					return string.Equals(this.value, levelOptionValue);
				case LorePackCondition.LoreLevelOptionCondition.ComparisonType.StringNotEquals:
					return !string.Equals(this.value, levelOptionValue);
				case LorePackCondition.LoreLevelOptionCondition.ComparisonType.StringContains:
					return levelOptionValue != null && levelOptionValue.Contains(this.value);
				default:
				{
					int intValue;
					if (!int.TryParse(this.value, out intValue))
					{
						return false;
					}
					int levelOptionIntValue;
					if (!int.TryParse(levelOptionValue, out levelOptionIntValue))
					{
						return false;
					}
					switch (this.comparison)
					{
					case LorePackCondition.LoreLevelOptionCondition.ComparisonType.IntEqual:
						return levelOptionIntValue == intValue;
					case LorePackCondition.LoreLevelOptionCondition.ComparisonType.IntNotEqual:
						return levelOptionIntValue != intValue;
					case LorePackCondition.LoreLevelOptionCondition.ComparisonType.IntGreater:
						return levelOptionIntValue > intValue;
					case LorePackCondition.LoreLevelOptionCondition.ComparisonType.IntLesser:
						return levelOptionIntValue < intValue;
					case LorePackCondition.LoreLevelOptionCondition.ComparisonType.IntGreaterOrEqual:
						return levelOptionIntValue >= intValue;
					case LorePackCondition.LoreLevelOptionCondition.ComparisonType.IntLesserOrEqual:
						return levelOptionIntValue <= intValue;
					default:
						return false;
					}
					break;
				}
				}
			}

			// Token: 0x040046B9 RID: 18105
			public string key;

			// Token: 0x040046BA RID: 18106
			public LorePackCondition.LoreLevelOptionCondition.ComparisonType comparison;

			// Token: 0x040046BB RID: 18107
			public string value;

			// Token: 0x02000BEF RID: 3055
			public enum ComparisonType
			{
				// Token: 0x04004D5B RID: 19803
				StringEquals,
				// Token: 0x04004D5C RID: 19804
				StringNotEquals,
				// Token: 0x04004D5D RID: 19805
				StringContains,
				// Token: 0x04004D5E RID: 19806
				IntEqual,
				// Token: 0x04004D5F RID: 19807
				IntNotEqual,
				// Token: 0x04004D60 RID: 19808
				IntGreater,
				// Token: 0x04004D61 RID: 19809
				IntLesser,
				// Token: 0x04004D62 RID: 19810
				IntGreaterOrEqual,
				// Token: 0x04004D63 RID: 19811
				IntLesserOrEqual
			}
		}
	}
}
