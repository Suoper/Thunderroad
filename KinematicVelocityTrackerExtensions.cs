using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200026D RID: 621
	public static class KinematicVelocityTrackerExtensions
	{
		// Token: 0x06001BD7 RID: 7127 RVA: 0x000B82D4 File Offset: 0x000B64D4
		public static VelocityTracker GetVelocityTracker(this Rigidbody rigidbody)
		{
			VelocityTracker velocityTracker;
			if (!rigidbody.gameObject.TryGetOrAddComponent(out velocityTracker))
			{
				return null;
			}
			return velocityTracker;
		}
	}
}
