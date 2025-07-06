using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002DE RID: 734
	[RequireComponent(typeof(LODGroup))]
	public class LODGroupOverride : MonoBehaviour
	{
		// Token: 0x06002351 RID: 9041 RVA: 0x000F1940 File Offset: 0x000EFB40
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.Refresh();
		}

		// Token: 0x06002352 RID: 9042 RVA: 0x000F1956 File Offset: 0x000EFB56
		private void Start()
		{
			this.Refresh();
		}

		// Token: 0x06002353 RID: 9043 RVA: 0x000F1960 File Offset: 0x000EFB60
		public void Refresh()
		{
			LODGroup lODGroup = base.GetComponent<LODGroup>();
			if (lODGroup)
			{
				lODGroup.size = this.lodSize;
				lODGroup.localReferencePoint = this.localReference;
				return;
			}
			Debug.LogErrorFormat(this, "LODGroupOverride cannot apply as there is no LODGroup attached", Array.Empty<object>());
		}

		// Token: 0x0400225F RID: 8799
		public float lodSize = 1f;

		// Token: 0x04002260 RID: 8800
		public Vector3 localReference = Vector3.zero;
	}
}
