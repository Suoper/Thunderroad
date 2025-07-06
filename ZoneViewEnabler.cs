using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x020002FC RID: 764
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/ZoneViewEnabler")]
	[AddComponentMenu("ThunderRoad/Levels/Zone view")]
	public class ZoneViewEnabler : MonoBehaviour
	{
		// Token: 0x06002495 RID: 9365 RVA: 0x000FB1DC File Offset: 0x000F93DC
		private void OnEnable()
		{
			RenderPipelineManager.beginCameraRendering += this.ExecuteBeforeCameraRender;
		}

		// Token: 0x06002496 RID: 9366 RVA: 0x000FB1EF File Offset: 0x000F93EF
		private void OnDisable()
		{
			RenderPipelineManager.beginCameraRendering -= this.ExecuteBeforeCameraRender;
		}

		// Token: 0x06002497 RID: 9367 RVA: 0x000FB204 File Offset: 0x000F9404
		private void Awake()
		{
			this.workingZones = new List<BoxCollider>(base.GetComponentsInChildren<BoxCollider>());
			foreach (BoxCollider boxCollider in this.workingZones)
			{
				boxCollider.isTrigger = true;
				boxCollider.gameObject.layer = 2;
			}
		}

		// Token: 0x06002498 RID: 9368 RVA: 0x000FB274 File Offset: 0x000F9474
		public void ExecuteBeforeCameraRender(ScriptableRenderContext context, Camera camera)
		{
			if (camera.gameObject.layer == 21)
			{
				using (List<BoxCollider>.Enumerator enumerator = this.workingZones.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current.bounds.Contains(camera.transform.position))
						{
							if (!this.target.activeSelf)
							{
								this.target.SetActive(true);
							}
							return;
						}
					}
				}
				if (this.target.activeSelf)
				{
					this.target.SetActive(false);
				}
			}
		}

		// Token: 0x0400242D RID: 9261
		public GameObject target;

		// Token: 0x0400242E RID: 9262
		protected List<BoxCollider> workingZones;
	}
}
