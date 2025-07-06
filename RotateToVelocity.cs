using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000357 RID: 855
	public class RotateToVelocity : MonoBehaviour
	{
		// Token: 0x06002805 RID: 10245 RVA: 0x001120A8 File Offset: 0x001102A8
		private void Update()
		{
			Vector3 velocityAtPoint = this.rigidbody.GetPointVelocity(base.transform.position);
			float velocityRatio = Mathf.InverseLerp(this.minSpeed, this.maxSpeed, velocityAtPoint.magnitude);
			base.transform.rotation = Quaternion.Lerp(Quaternion.LookRotation(this.defaultDirection, this.upwards), Quaternion.LookRotation(this.revert ? (-velocityAtPoint.normalized) : velocityAtPoint.normalized, this.upwards), velocityRatio);
		}

		// Token: 0x040026E9 RID: 9961
		public Rigidbody rigidbody;

		// Token: 0x040026EA RID: 9962
		public Vector3 defaultDirection = Vector3.up;

		// Token: 0x040026EB RID: 9963
		public Vector3 upwards = Vector3.up;

		// Token: 0x040026EC RID: 9964
		public float minSpeed;

		// Token: 0x040026ED RID: 9965
		public float maxSpeed = 3f;

		// Token: 0x040026EE RID: 9966
		public bool revert;
	}
}
