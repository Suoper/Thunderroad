using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200025A RID: 602
	public class CollisionListener : MonoBehaviour
	{
		// Token: 0x140000B5 RID: 181
		// (add) Token: 0x06001A76 RID: 6774 RVA: 0x000B0D48 File Offset: 0x000AEF48
		// (remove) Token: 0x06001A77 RID: 6775 RVA: 0x000B0D80 File Offset: 0x000AEF80
		public event CollisionListener.CollisionEvent OnCollisionEnterEvent;

		// Token: 0x140000B6 RID: 182
		// (add) Token: 0x06001A78 RID: 6776 RVA: 0x000B0DB8 File Offset: 0x000AEFB8
		// (remove) Token: 0x06001A79 RID: 6777 RVA: 0x000B0DF0 File Offset: 0x000AEFF0
		public event CollisionListener.CollisionEvent OnCollisionExitEvent;

		// Token: 0x06001A7A RID: 6778 RVA: 0x000B0E25 File Offset: 0x000AF025
		private void OnCollisionEnter(Collision other)
		{
			CollisionListener.CollisionEvent onCollisionEnterEvent = this.OnCollisionEnterEvent;
			if (onCollisionEnterEvent == null)
			{
				return;
			}
			onCollisionEnterEvent(other);
		}

		// Token: 0x06001A7B RID: 6779 RVA: 0x000B0E38 File Offset: 0x000AF038
		private void OnCollisionExit(Collision other)
		{
			CollisionListener.CollisionEvent onCollisionExitEvent = this.OnCollisionExitEvent;
			if (onCollisionExitEvent == null)
			{
				return;
			}
			onCollisionExitEvent(other);
		}

		// Token: 0x020008AE RID: 2222
		// (Invoke) Token: 0x06004116 RID: 16662
		public delegate void CollisionEvent(Collision other);
	}
}
