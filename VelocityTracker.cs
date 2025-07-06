using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200026C RID: 620
	public class VelocityTracker : ThunderBehaviour
	{
		// Token: 0x170001BD RID: 445
		// (get) Token: 0x06001BCE RID: 7118 RVA: 0x000B81DB File Offset: 0x000B63DB
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x170001BE RID: 446
		// (get) Token: 0x06001BCF RID: 7119 RVA: 0x000B81DE File Offset: 0x000B63DE
		// (set) Token: 0x06001BD0 RID: 7120 RVA: 0x000B81EB File Offset: 0x000B63EB
		public Vector3 position
		{
			get
			{
				return base.transform.position;
			}
			set
			{
				base.transform.position = value;
			}
		}

		// Token: 0x170001BF RID: 447
		// (get) Token: 0x06001BD1 RID: 7121 RVA: 0x000B81F9 File Offset: 0x000B63F9
		// (set) Token: 0x06001BD2 RID: 7122 RVA: 0x000B8206 File Offset: 0x000B6406
		public Quaternion rotation
		{
			get
			{
				return base.transform.rotation;
			}
			set
			{
				base.transform.rotation = value;
			}
		}

		// Token: 0x06001BD3 RID: 7123 RVA: 0x000B8214 File Offset: 0x000B6414
		public static implicit operator Transform(VelocityTracker tracker)
		{
			return tracker.transform;
		}

		// Token: 0x06001BD4 RID: 7124 RVA: 0x000B821C File Offset: 0x000B641C
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			this.lastPos = base.transform.position;
			this.lastRot = base.transform.eulerAngles;
		}

		// Token: 0x06001BD5 RID: 7125 RVA: 0x000B8248 File Offset: 0x000B6448
		protected internal override void ManagedUpdate()
		{
			base.ManagedUpdate();
			this.velocity = (base.transform.position - this.lastPos) / Time.fixedDeltaTime;
			this.angularVelocity = (base.transform.eulerAngles - this.lastRot) / Time.fixedDeltaTime;
			this.lastPos = base.transform.position;
			this.lastRot = base.transform.eulerAngles;
		}

		// Token: 0x04001AB0 RID: 6832
		public Vector3 velocity;

		// Token: 0x04001AB1 RID: 6833
		public Vector3 angularVelocity;

		// Token: 0x04001AB2 RID: 6834
		private Vector3 lastPos;

		// Token: 0x04001AB3 RID: 6835
		private Vector3 lastRot;
	}
}
