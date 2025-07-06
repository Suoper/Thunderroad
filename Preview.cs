using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000353 RID: 851
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Preview.html")]
	[AddComponentMenu("ThunderRoad/Items/Preview")]
	public class Preview : MonoBehaviour
	{
		// Token: 0x060027EB RID: 10219 RVA: 0x00111B60 File Offset: 0x0010FD60
		protected virtual void OnDrawGizmosSelected()
		{
			this.size = base.transform.lossyScale.x;
			Gizmos.color = Common.HueColourValue(HueColorName.Green);
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Transform thisTransform = base.transform;
			Gizmos.DrawWireCube(Vector3.zero, new Vector3(this.size / thisTransform.lossyScale.x, this.size / thisTransform.lossyScale.y, 0f));
			Common.DrawGizmoArrow(Vector3.zero, Vector3.back * 0.3f / thisTransform.lossyScale.z, Color.blue, 0.15f / thisTransform.lossyScale.z, 20f, false);
			Common.DrawGizmoArrow(Vector3.zero, Vector3.up * 0.3f / thisTransform.lossyScale.y, Common.HueColourValue(HueColorName.Green), 0.15f / thisTransform.lossyScale.y, 20f, false);
			Bounds bounds = default(Bounds);
			Item parentItem = base.GetComponentInParent<Item>();
			if (parentItem != null)
			{
				foreach (Renderer renderer in parentItem.GetComponentsInChildren<Renderer>())
				{
					if (!(renderer == null))
					{
						bounds.Encapsulate(renderer.bounds);
					}
				}
			}
			Vector3 topLightPos = thisTransform.forward * 2f;
			Common.DrawGizmoArrow(topLightPos, -thisTransform.forward, Color.yellow, Vector3.Distance(topLightPos, Vector3.zero), 20f, false);
			Vector3 leftLightPos = -thisTransform.right * 2f;
			Common.DrawGizmoArrow(leftLightPos, thisTransform.right, Color.yellow, Vector3.Distance(leftLightPos, Vector3.zero), 20f, false);
			Vector3 rightLightPos = thisTransform.right * 2f;
			Common.DrawGizmoArrow(rightLightPos, -thisTransform.right, Color.yellow, Vector3.Distance(rightLightPos, Vector3.zero), 20f, false);
		}

		// Token: 0x040026DB RID: 9947
		[Tooltip("If ticked, will generate a separate preview with \"Close-up\", which is used for close-up preview in the item spawner")]
		public bool closeUpPreview;

		// Token: 0x040026DC RID: 9948
		[Tooltip("Determine size of the preview. Scales with X Scale")]
		public float size = 1f;

		// Token: 0x040026DD RID: 9949
		[Tooltip("Default resolution of the generated preview")]
		public int iconResolution = 512;

		// Token: 0x040026DE RID: 9950
		[Tooltip("Temporarily changes layer when taking a picture. Change this if the layers are already in use/changed.")]
		public int tempLayer = 2;

		// Token: 0x040026DF RID: 9951
		[Tooltip("Point light intensity")]
		public float pointLightIntensity = 1f;

		// Token: 0x040026E0 RID: 9952
		[Tooltip("Ambient light intensity")]
		public float ambientIntensity = 1f;

		// Token: 0x040026E1 RID: 9953
		[Tooltip("Try to update the catalog with the addressable path of the generated icon.")]
		public bool updateCatalog;

		// Token: 0x040026E2 RID: 9954
		[Tooltip("List of renderers which can be used in the preview. Not neccessary for weapon mesh.")]
		public List<Renderer> renderers;

		// Token: 0x040026E3 RID: 9955
		[NonSerialized]
		public Texture2D generatedIcon;
	}
}
