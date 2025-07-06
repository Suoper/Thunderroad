using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002DD RID: 733
	public class LodFade : MonoBehaviour
	{
		// Token: 0x0600234E RID: 9038 RVA: 0x000F18A5 File Offset: 0x000EFAA5
		private void Awake()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
		}

		// Token: 0x0600234F RID: 9039 RVA: 0x000F18B4 File Offset: 0x000EFAB4
		public void Set(float fade)
		{
			foreach (MeshRenderer meshRenderer in this.meshRenderers)
			{
				meshRenderer.GetPropertyBlock(this.materialPropertyBlock);
				this.materialPropertyBlock.SetVector("unity_LODFade", new Vector4(fade - 1f, 0f));
				meshRenderer.SetPropertyBlock(this.materialPropertyBlock);
			}
		}

		// Token: 0x0400225D RID: 8797
		public List<MeshRenderer> meshRenderers;

		// Token: 0x0400225E RID: 8798
		protected MaterialPropertyBlock materialPropertyBlock;
	}
}
