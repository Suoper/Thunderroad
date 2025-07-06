using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002A9 RID: 681
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Exclude/AIFireball.html")]
	public class AIFireable : MonoBehaviour
	{
		// Token: 0x170001EE RID: 494
		// (get) Token: 0x06001F9E RID: 8094 RVA: 0x000D6F51 File Offset: 0x000D5151
		// (set) Token: 0x06001F9F RID: 8095 RVA: 0x000D6F59 File Offset: 0x000D5159
		public Item item { get; protected set; }

		// Token: 0x04001ED6 RID: 7894
		public Transform aimTransform;

		// Token: 0x04001ED7 RID: 7895
		[Header("Fireable events")]
		public UnityEvent aimEvent;

		// Token: 0x04001ED8 RID: 7896
		public UnityEvent fireEvent;

		// Token: 0x04001ED9 RID: 7897
		public UnityEvent reloadEvent;
	}
}
