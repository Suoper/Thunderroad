using System;

namespace ThunderRoad
{
	// Token: 0x0200023A RID: 570
	public struct FloatingParams
	{
		// Token: 0x06001811 RID: 6161 RVA: 0x000A0745 File Offset: 0x0009E945
		public FloatingParams(float gravity = 1f, float drag = 1f, float mass = 1f, bool noSlamAtEnd = false)
		{
			this.gravity = gravity;
			this.drag = drag;
			this.mass = mass;
			this.noSlamAtEnd = noSlamAtEnd;
		}

		// Token: 0x17000177 RID: 375
		// (get) Token: 0x06001812 RID: 6162 RVA: 0x000A0764 File Offset: 0x0009E964
		public static FloatingParams Identity
		{
			get
			{
				return new FloatingParams(1f, 1f, 1f, false);
			}
		}

		// Token: 0x06001813 RID: 6163 RVA: 0x000A077B File Offset: 0x0009E97B
		public static FloatingParams operator *(FloatingParams a, FloatingParams b)
		{
			return new FloatingParams(a.gravity * b.gravity, a.drag * b.drag, a.mass * b.mass, false);
		}

		// Token: 0x0400173D RID: 5949
		public float gravity;

		// Token: 0x0400173E RID: 5950
		public float drag;

		// Token: 0x0400173F RID: 5951
		public float mass;

		// Token: 0x04001740 RID: 5952
		public bool noSlamAtEnd;
	}
}
