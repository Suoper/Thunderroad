using System;
using UnityEngine.UI;

namespace ThunderRoad
{
	/// <summary>
	/// Special class used as a empty UI graphic component so it can be used as a raycast target
	/// </summary>
	// Token: 0x02000376 RID: 886
	public class RaycastTarget : Graphic
	{
		// Token: 0x06002A13 RID: 10771 RVA: 0x0011D9AE File Offset: 0x0011BBAE
		public override void SetMaterialDirty()
		{
		}

		// Token: 0x06002A14 RID: 10772 RVA: 0x0011D9B0 File Offset: 0x0011BBB0
		public override void SetVerticesDirty()
		{
		}

		// Token: 0x06002A15 RID: 10773 RVA: 0x0011D9B2 File Offset: 0x0011BBB2
		protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();
		}
	}
}
