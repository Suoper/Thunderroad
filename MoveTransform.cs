using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002E4 RID: 740
	public class MoveTransform : MonoBehaviour
	{
		// Token: 0x060023AB RID: 9131 RVA: 0x000F43AE File Offset: 0x000F25AE
		private void Awake()
		{
			this.originalPosition = base.transform.position;
		}

		// Token: 0x060023AC RID: 9132 RVA: 0x000F43C4 File Offset: 0x000F25C4
		private void Update()
		{
			if (this.target)
			{
				if (this.movementType == MoveTransform.MovementType.Translate)
				{
					base.transform.Translate((this.target.position - base.transform.position) * Time.deltaTime * this.speed, Space.World);
					return;
				}
				if (this.movementType == MoveTransform.MovementType.Lerp)
				{
					base.transform.position = Vector3.Lerp(base.transform.position, this.target.position, Time.deltaTime * this.speed);
				}
			}
		}

		// Token: 0x060023AD RID: 9133 RVA: 0x000F4464 File Offset: 0x000F2664
		private void OnDrawGizmosSelected()
		{
			if (this.target == null)
			{
				return;
			}
			object obj = Application.isPlaying ? this.originalPosition : base.transform.position;
			Gizmos.color = Color.green;
			object obj2 = obj;
			Gizmos.DrawSphere(obj2, 0.15f);
			Gizmos.color = Color.green;
			Gizmos.DrawLine(obj2, this.target.position);
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(this.target.position, 0.15f);
		}

		// Token: 0x040022B3 RID: 8883
		public Transform target;

		// Token: 0x040022B4 RID: 8884
		public float speed = 1f;

		// Token: 0x040022B5 RID: 8885
		public MoveTransform.MovementType movementType;

		// Token: 0x040022B6 RID: 8886
		protected Vector3 originalPosition;

		// Token: 0x020009CD RID: 2509
		public enum MovementType
		{
			// Token: 0x040045EF RID: 17903
			Translate,
			// Token: 0x040045F0 RID: 17904
			Lerp
		}
	}
}
