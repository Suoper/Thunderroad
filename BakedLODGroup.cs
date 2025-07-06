using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002FD RID: 765
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/BakedLodGroup.html")]
	[ExecuteInEditMode]
	public class BakedLODGroup : MonoBehaviour, ICheckAsset
	{
		// Token: 0x0600249A RID: 9370 RVA: 0x000FB324 File Offset: 0x000F9524
		private void Awake()
		{
			this.lightingGroup = base.GetComponentInParent<LightingGroup>();
			foreach (MeshRenderer targetMeshRenderer in this.targetMeshRenderers)
			{
				if (this.meshRenderer && targetMeshRenderer && !targetMeshRenderer.isPartOfStaticBatch)
				{
					targetMeshRenderer.lightmapScaleOffset = this.meshRenderer.lightmapScaleOffset;
				}
			}
		}

		// Token: 0x0600249B RID: 9371 RVA: 0x000FB384 File Offset: 0x000F9584
		private void Start()
		{
			if (!this.lightingGroup)
			{
				this.ApplyLightmaps();
			}
		}

		// Token: 0x0600249C RID: 9372 RVA: 0x000FB39C File Offset: 0x000F959C
		public void ApplyLightmaps()
		{
			if (this.applySourceLightmapOnTarget)
			{
				if (this.meshRenderer == null)
				{
					Debug.LogWarningFormat(this, "BakeLODGroup - Can't apply lightmap because meshRenderer is null on " + base.gameObject.GetPathFromRoot(), Array.Empty<object>());
					return;
				}
				foreach (MeshRenderer targetMeshRenderer in this.targetMeshRenderers)
				{
					if (targetMeshRenderer == null)
					{
						Debug.LogWarningFormat(this, "BakeLODGroup - Can't apply lightmap to a targetMeshRenderer because it's null on " + base.gameObject.GetPathFromRoot(), Array.Empty<object>());
					}
					else
					{
						targetMeshRenderer.lightmapIndex = this.meshRenderer.lightmapIndex;
						targetMeshRenderer.realtimeLightmapIndex = this.meshRenderer.realtimeLightmapIndex;
						targetMeshRenderer.lightProbeUsage = this.meshRenderer.lightProbeUsage;
						targetMeshRenderer.realtimeLightmapScaleOffset = this.meshRenderer.realtimeLightmapScaleOffset;
						if (!targetMeshRenderer.isPartOfStaticBatch)
						{
							targetMeshRenderer.lightmapScaleOffset = this.meshRenderer.lightmapScaleOffset;
						}
					}
				}
			}
		}

		// Token: 0x0400242F RID: 9263
		[Tooltip("This will be the head mesh renderer, which will be used for the lightmap, especially if you share lightmaps with other LODs.")]
		public MeshRenderer meshRenderer;

		// Token: 0x04002430 RID: 9264
		[Tooltip("To save lightmap space, you can put other LODs of this object here, a recommendation if they share the same lightmap UVs.")]
		public bool applySourceLightmapOnTarget;

		// Token: 0x04002431 RID: 9265
		[Tooltip("Put LODs you want to share lightmap scales with here.")]
		public MeshRenderer[] targetMeshRenderers = new MeshRenderer[0];

		// Token: 0x04002432 RID: 9266
		protected LightingGroup lightingGroup;

		// Token: 0x04002433 RID: 9267
		protected float orgLightmapScale;
	}
}
