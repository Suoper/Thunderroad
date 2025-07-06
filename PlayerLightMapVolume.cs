using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000307 RID: 775
	[ExecuteInEditMode]
	[RequireComponent(typeof(BoxCollider))]
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/PlayerLightmapVolume.html")]
	public class PlayerLightMapVolume : MonoBehaviour
	{
		// Token: 0x06002515 RID: 9493 RVA: 0x000FEA80 File Offset: 0x000FCC80
		private void OnEnable()
		{
			if (Application.isPlaying)
			{
				base.gameObject.SetActive(false);
				return;
			}
			this.boxCollider = base.GetComponent<BoxCollider>();
			this.boxCollider.isTrigger = true;
			PlayerLightMapVolume.playerLightMapVolumes.Add(this);
			base.gameObject.tag = "EditorOnly";
		}

		// Token: 0x06002516 RID: 9494 RVA: 0x000FEAD4 File Offset: 0x000FCCD4
		private void OnValidate()
		{
		}

		// Token: 0x06002517 RID: 9495 RVA: 0x000FEAD6 File Offset: 0x000FCCD6
		private void OnDisable()
		{
			PlayerLightMapVolume.playerLightMapVolumes.Remove(this);
		}

		// Token: 0x06002518 RID: 9496 RVA: 0x000FEAE4 File Offset: 0x000FCCE4
		private void OnDestroy()
		{
			PlayerLightMapVolume.playerLightMapVolumes.Remove(this);
		}

		// Token: 0x04002495 RID: 9365
		public static List<PlayerLightMapVolume> playerLightMapVolumes = new List<PlayerLightMapVolume>();

		// Token: 0x04002496 RID: 9366
		[Tooltip("Depicts what Player Light Map Volume has priority over others.")]
		[Range(0f, 10f)]
		public int priority = 1;

		// Token: 0x04002497 RID: 9367
		[Range(0f, 5f)]
		[Tooltip("Adjusts the light map scale of Baked LOD Groups inside the box.")]
		public float lightMapScaleMultiplier = 1f;

		// Token: 0x04002498 RID: 9368
		[Tooltip("The distance from the closest point of the bounding box of the volume where the lightmap scale multiplier will be 1. The lightmap scale multiplier will be interpolated between 1 and 0 from the bounding box to the falloff distance.")]
		public float lightMapFalloffDistance = 10f;

		// Token: 0x04002499 RID: 9369
		[Tooltip("The box collider used to calculate Baked LOD groups.")]
		public BoxCollider boxCollider;
	}
}
