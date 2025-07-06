using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000368 RID: 872
	[Serializable]
	public class Bezier
	{
		// Token: 0x0600291F RID: 10527 RVA: 0x00117830 File Offset: 0x00115A30
		public Bezier(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
		{
			this.p0 = v0;
			this.p1 = v1;
			this.p2 = v2;
			this.p3 = v3;
		}

		// Token: 0x06002920 RID: 10528 RVA: 0x0011788C File Offset: 0x00115A8C
		public Vector3 Evaluate(float t)
		{
			this.CheckConstant();
			float t2 = t * t;
			float t3 = t * t * t;
			float x = this.Ax * t3 + this.Bx * t2 + this.Cx * t + this.p0.x;
			float y = this.Ay * t3 + this.By * t2 + this.Cy * t + this.p0.y;
			float z = this.Az * t3 + this.Bz * t2 + this.Cz * t + this.p0.z;
			return new Vector3(x, y, z);
		}

		// Token: 0x06002921 RID: 10529 RVA: 0x00117924 File Offset: 0x00115B24
		private void SetConstant()
		{
			this.Cx = 3f * (this.p0.x + this.p1.x - this.p0.x);
			this.Bx = 3f * (this.p3.x + this.p2.x - (this.p0.x + this.p1.x)) - this.Cx;
			this.Ax = this.p3.x - this.p0.x - this.Cx - this.Bx;
			this.Cy = 3f * (this.p0.y + this.p1.y - this.p0.y);
			this.By = 3f * (this.p3.y + this.p2.y - (this.p0.y + this.p1.y)) - this.Cy;
			this.Ay = this.p3.y - this.p0.y - this.Cy - this.By;
			this.Cz = 3f * (this.p0.z + this.p1.z - this.p0.z);
			this.Bz = 3f * (this.p3.z + this.p2.z - (this.p0.z + this.p1.z)) - this.Cz;
			this.Az = this.p3.z - this.p0.z - this.Cz - this.Bz;
		}

		// Token: 0x06002922 RID: 10530 RVA: 0x00117B08 File Offset: 0x00115D08
		private void CheckConstant()
		{
			if (this.p0 != this.b0 || this.p1 != this.b1 || this.p2 != this.b2 || this.p3 != this.b3)
			{
				this.SetConstant();
				this.b0 = this.p0;
				this.b1 = this.p1;
				this.b2 = this.p2;
				this.b3 = this.p3;
			}
		}

		// Token: 0x04002730 RID: 10032
		public Vector3 p0;

		// Token: 0x04002731 RID: 10033
		public Vector3 p1;

		// Token: 0x04002732 RID: 10034
		public Vector3 p2;

		// Token: 0x04002733 RID: 10035
		public Vector3 p3;

		// Token: 0x04002734 RID: 10036
		public float ti;

		// Token: 0x04002735 RID: 10037
		private Vector3 b0 = Vector3.zero;

		// Token: 0x04002736 RID: 10038
		private Vector3 b1 = Vector3.zero;

		// Token: 0x04002737 RID: 10039
		private Vector3 b2 = Vector3.zero;

		// Token: 0x04002738 RID: 10040
		private Vector3 b3 = Vector3.zero;

		// Token: 0x04002739 RID: 10041
		private float Ax;

		// Token: 0x0400273A RID: 10042
		private float Ay;

		// Token: 0x0400273B RID: 10043
		private float Az;

		// Token: 0x0400273C RID: 10044
		private float Bx;

		// Token: 0x0400273D RID: 10045
		private float By;

		// Token: 0x0400273E RID: 10046
		private float Bz;

		// Token: 0x0400273F RID: 10047
		private float Cx;

		// Token: 0x04002740 RID: 10048
		private float Cy;

		// Token: 0x04002741 RID: 10049
		private float Cz;
	}
}
