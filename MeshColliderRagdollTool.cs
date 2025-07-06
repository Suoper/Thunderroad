using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000276 RID: 630
	public class MeshColliderRagdollTool : MonoBehaviour
	{
		// Token: 0x06001CC4 RID: 7364 RVA: 0x000C1430 File Offset: 0x000BF630
		public void SetColliders()
		{
			SkinnedMeshRenderer[] smrs = base.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer smr in smrs)
			{
				MeshCollider meshCollider;
				if (!smr.TryGetComponent<MeshCollider>(out meshCollider))
				{
					meshCollider = smr.gameObject.AddComponent<MeshCollider>();
				}
				meshCollider.sharedMesh = smr.sharedMesh;
				meshCollider.convex = true;
				meshCollider.material = this.defaultMaterial;
			}
			for (int i = smrs.Length - 1; i >= 0; i--)
			{
				UnityEngine.Object.Destroy(smrs[i]);
			}
		}

		// Token: 0x04001B8B RID: 7051
		public PhysicMaterial defaultMaterial;
	}
}
