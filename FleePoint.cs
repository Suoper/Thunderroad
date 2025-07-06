using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002CD RID: 717
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/FleePoint.html")]
	[AddComponentMenu("ThunderRoad/Levels/Flee point")]
	public class FleePoint : MonoBehaviour
	{
		// Token: 0x060022B4 RID: 8884 RVA: 0x000EE426 File Offset: 0x000EC626
		protected virtual void OnEnable()
		{
			FleePoint.list.Add(this);
		}

		// Token: 0x060022B5 RID: 8885 RVA: 0x000EE433 File Offset: 0x000EC633
		protected virtual void OnDisable()
		{
			FleePoint.list.Remove(this);
		}

		// Token: 0x040021C8 RID: 8648
		public static List<FleePoint> list = new List<FleePoint>();
	}
}
