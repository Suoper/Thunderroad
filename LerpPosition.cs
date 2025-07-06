using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002DA RID: 730
	public class LerpPosition : MonoBehaviour
	{
		// Token: 0x06002332 RID: 9010 RVA: 0x000F11A7 File Offset: 0x000EF3A7
		private void OnEnable()
		{
			this.orgPosition = this.reference.position;
			this.orgRotation = this.reference.rotation;
		}

		// Token: 0x06002333 RID: 9011 RVA: 0x000F11CB File Offset: 0x000EF3CB
		public void SetRatio(float ratio)
		{
			this.ratio = ratio;
		}

		// Token: 0x06002334 RID: 9012 RVA: 0x000F11D4 File Offset: 0x000EF3D4
		private void Update()
		{
			if (this.reference && this.target)
			{
				this.reference.position = Vector3.Lerp(this.orgPosition, this.target.transform.position, this.curve.Evaluate(this.ratio));
				this.reference.rotation = Quaternion.Lerp(this.orgRotation, this.target.transform.rotation, this.curve.Evaluate(this.ratio));
			}
		}

		// Token: 0x06002335 RID: 9013 RVA: 0x000F1269 File Offset: 0x000EF469
		private void OnDrawGizmosSelected()
		{
			if (this.reference && this.target)
			{
				Gizmos.DrawLine(this.orgPosition, this.target.transform.position);
			}
		}

		// Token: 0x0400223F RID: 8767
		public Transform reference;

		// Token: 0x04002240 RID: 8768
		public Transform target;

		// Token: 0x04002241 RID: 8769
		public AnimationCurve curve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04002242 RID: 8770
		[Range(0f, 1f)]
		public float ratio;

		// Token: 0x04002243 RID: 8771
		protected Vector3 orgPosition;

		// Token: 0x04002244 RID: 8772
		protected Quaternion orgRotation;
	}
}
