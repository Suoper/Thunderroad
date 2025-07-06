using System;
using ThunderRoad.Manikin;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000277 RID: 631
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/MeshPart.html")]
	[DisallowMultipleComponent]
	public class MeshPart : MonoBehaviour
	{
		// Token: 0x06001CC6 RID: 7366 RVA: 0x000C14B6 File Offset: 0x000BF6B6
		private void OnValidate()
		{
			this.scale = ((this.scale < 1) ? 1 : Mathf.ClosestPowerOfTwo(this.scale));
		}

		// Token: 0x06001CC7 RID: 7367 RVA: 0x000C14D5 File Offset: 0x000BF6D5
		private void Start()
		{
			this.manikinPart = base.GetComponentInParent<ManikinPart>();
			if (this.defaultPhysicMaterial)
			{
				this.defaultPhysicMaterialHash = Animator.StringToHash(this.defaultPhysicMaterial.name + " (Instance)");
			}
		}

		// Token: 0x04001B8C RID: 7052
		[Tooltip("References the Skinned Mesh Renderer that creates the collider for the part.\n\nIt is recommended, if your parts have LODs, to use the lowest LOD for this, as it will be more performant.")]
		public SkinnedMeshRenderer skinnedMeshRenderer;

		// Token: 0x04001B8D RID: 7053
		[Tooltip("This defines the default physics material if your part does not have an ID mesh. If it does have an ID mesh, it is recommended to set this to \"Flesh\".")]
		public PhysicMaterial defaultPhysicMaterial;

		// Token: 0x04001B8E RID: 7054
		[Tooltip("The ID map texture. This is used to determine the physics material used for armor detection for the part.")]
		public Texture2D idMap;

		// Token: 0x04001B8F RID: 7055
		[Tooltip("For better performance, it is recommended to use the convert button to set the ID map to an ID Map array, for better performance.")]
		public IdMapArray idMapArray;

		// Token: 0x04001B90 RID: 7056
		[Tooltip("The factor to scale the ID map down by.")]
		public int scale = 4;

		// Token: 0x04001B91 RID: 7057
		[NonSerialized]
		public ManikinPart manikinPart;

		// Token: 0x04001B92 RID: 7058
		[NonSerialized]
		public int defaultPhysicMaterialHash;
	}
}
