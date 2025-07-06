using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000311 RID: 785
	[RequireComponent(typeof(SkinnedMeshRenderer))]
	public class BlendShapeSyncer : ThunderBehaviour
	{
		// Token: 0x1700023F RID: 575
		// (get) Token: 0x06002542 RID: 9538 RVA: 0x000FF575 File Offset: 0x000FD775
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate;
			}
		}

		// Token: 0x06002543 RID: 9539 RVA: 0x000FF578 File Offset: 0x000FD778
		private void Start()
		{
			this.SetBlendShapeDictionary();
		}

		// Token: 0x06002544 RID: 9540 RVA: 0x000FF580 File Offset: 0x000FD780
		private void SetBlendShapeDictionary()
		{
			this.skinnedMeshRenderer = base.GetComponent<SkinnedMeshRenderer>();
			this.blendShapeDictionary = new Dictionary<int, int>();
			this.selfCleanedNames = new List<string>();
			this.driverCleanedNames = new List<string>();
			List<int> matchedIndices = new List<int>();
			for (int i = 0; i < this.skinnedMeshRenderer.sharedMesh.blendShapeCount; i++)
			{
				string shapeName = this.skinnedMeshRenderer.sharedMesh.GetBlendShapeName(i);
				foreach (BlendShapeSyncer.ShapeNameCleaner shapeNameCleaner in this.selfNameCleaning)
				{
					shapeName = shapeNameCleaner.Clean(shapeName);
				}
				this.selfCleanedNames.Add(shapeName);
			}
			for (int j = 0; j < this.driverSMR.sharedMesh.blendShapeCount; j++)
			{
				string cleanedName = this.driverSMR.sharedMesh.GetBlendShapeName(j);
				foreach (BlendShapeSyncer.ShapeNameCleaner shapeNameCleaner2 in this.driverNameCleaning)
				{
					cleanedName = shapeNameCleaner2.Clean(cleanedName);
				}
				this.driverCleanedNames.Add(cleanedName);
				if (this.selfCleanedNames.Contains(cleanedName) && !this.blendShapeDictionary.ContainsKey(j))
				{
					int matchedIndex = this.selfCleanedNames.IndexOf(cleanedName);
					this.blendShapeDictionary.Add(j, matchedIndex);
					matchedIndices.Add(matchedIndex);
				}
			}
			for (int k = 0; k < this.selfCleanedNames.Count; k++)
			{
				if (!matchedIndices.Contains(k))
				{
					Debug.LogWarning(string.Concat(new string[]
					{
						"Missed blendshape ",
						this.skinnedMeshRenderer.sharedMesh.GetBlendShapeName(k),
						" (",
						this.selfCleanedNames[k],
						") in the matching process: You may need to adjust your name cleaning process if this blendshape is important!"
					}));
				}
			}
		}

		// Token: 0x06002545 RID: 9541 RVA: 0x000FF77C File Offset: 0x000FD97C
		protected internal override void ManagedFixedUpdate()
		{
			foreach (KeyValuePair<int, int> blendShapePairing in this.blendShapeDictionary)
			{
				this.skinnedMeshRenderer.SetBlendShapeWeight(blendShapePairing.Value, this.driverSMR.GetBlendShapeWeight(blendShapePairing.Key));
			}
		}

		// Token: 0x040024C5 RID: 9413
		public List<BlendShapeSyncer.ShapeNameCleaner> selfNameCleaning = new List<BlendShapeSyncer.ShapeNameCleaner>();

		// Token: 0x040024C6 RID: 9414
		public SkinnedMeshRenderer driverSMR;

		// Token: 0x040024C7 RID: 9415
		public List<BlendShapeSyncer.ShapeNameCleaner> driverNameCleaning = new List<BlendShapeSyncer.ShapeNameCleaner>();

		// Token: 0x040024C8 RID: 9416
		private SkinnedMeshRenderer skinnedMeshRenderer;

		// Token: 0x040024C9 RID: 9417
		private List<string> selfCleanedNames;

		// Token: 0x040024CA RID: 9418
		private List<string> driverCleanedNames;

		// Token: 0x040024CB RID: 9419
		private Dictionary<int, int> blendShapeDictionary;

		// Token: 0x02000A0C RID: 2572
		[Serializable]
		public class ShapeNameCleaner
		{
			// Token: 0x06004523 RID: 17699 RVA: 0x00195430 File Offset: 0x00193630
			public string Clean(string input)
			{
				switch (this.method)
				{
				case BlendShapeSyncer.ShapeNameCleaner.StringMethod.ToLower:
					return input.ToLower();
				case BlendShapeSyncer.ShapeNameCleaner.StringMethod.ToUpper:
					return input.ToUpper();
				case BlendShapeSyncer.ShapeNameCleaner.StringMethod.Split:
					return input.Split(new char[]
					{
						this.splitChar
					}, 2)[(this.splitSide == Side.Left) ? 0 : 1];
				case BlendShapeSyncer.ShapeNameCleaner.StringMethod.Replace:
					return input.Replace(this.stringToReplace, this.replacement);
				default:
					return input;
				}
			}

			// Token: 0x040046DE RID: 18142
			public BlendShapeSyncer.ShapeNameCleaner.StringMethod method;

			// Token: 0x040046DF RID: 18143
			public char splitChar;

			// Token: 0x040046E0 RID: 18144
			public Side splitSide;

			// Token: 0x040046E1 RID: 18145
			public string stringToReplace;

			// Token: 0x040046E2 RID: 18146
			public string replacement;

			// Token: 0x02000BF0 RID: 3056
			public enum StringMethod
			{
				// Token: 0x04004D65 RID: 19813
				ToLower,
				// Token: 0x04004D66 RID: 19814
				ToUpper,
				// Token: 0x04004D67 RID: 19815
				Split,
				// Token: 0x04004D68 RID: 19816
				Replace
			}
		}
	}
}
