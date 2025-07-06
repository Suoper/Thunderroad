using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002FB RID: 763
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ZonePortal.html")]
	public class ZonePortal : MonoBehaviour
	{
		// Token: 0x06002492 RID: 9362 RVA: 0x000FB04C File Offset: 0x000F924C
		public bool IsInside(Vector3 position)
		{
			Vector3 localPosition = base.transform.InverseTransformPoint(Player.local.head.transform.position);
			return localPosition.z > 0f && localPosition.z < this.size.z && localPosition.x < this.size.x / 2f && localPosition.x > -this.size.x / 2f && localPosition.y < this.size.y / 2f && localPosition.y > -this.size.y / 2f;
		}

		// Token: 0x06002493 RID: 9363 RVA: 0x000FB104 File Offset: 0x000F9304
		private void OnDrawGizmos()
		{
			Gizmos.matrix = base.transform.localToWorldMatrix;
			Gizmos.color = new Color(0f, 0f, 0.5f, 0.25f);
			Gizmos.DrawCube(new Vector3(0f, 0f, this.size.z / 2f), this.size);
			Gizmos.color = new Color(0f, 0f, 0.5f, 1f);
			Gizmos.DrawWireCube(new Vector3(0f, 0f, this.size.z / 2f), this.size);
		}

		// Token: 0x04002428 RID: 9256
		public Vector3 size = Vector3.one;

		// Token: 0x04002429 RID: 9257
		public bool preventZoneEnterEvent;

		// Token: 0x0400242A RID: 9258
		public bool preventZoneExitEvent;

		// Token: 0x0400242B RID: 9259
		public UnityEvent<UnityEngine.Object> enterEvent = new UnityEvent<UnityEngine.Object>();

		// Token: 0x0400242C RID: 9260
		public UnityEvent<UnityEngine.Object> exitEvent = new UnityEvent<UnityEngine.Object>();
	}
}
