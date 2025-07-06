using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002D4 RID: 724
	public class ContentCustomDataRelativePosRot : ContentCustomData
	{
		// Token: 0x06002303 RID: 8963 RVA: 0x000F0405 File Offset: 0x000EE605
		public ContentCustomDataRelativePosRot()
		{
		}

		// Token: 0x06002304 RID: 8964 RVA: 0x000F040D File Offset: 0x000EE60D
		public ContentCustomDataRelativePosRot(Transform item, Transform relative)
		{
			this.localPos = relative.InverseTransformPoint(item.position);
			this.localRot = relative.InverseTransformRotation(item.rotation);
		}

		// Token: 0x04002201 RID: 8705
		public Vector3 localPos;

		// Token: 0x04002202 RID: 8706
		public Quaternion localRot;
	}
}
