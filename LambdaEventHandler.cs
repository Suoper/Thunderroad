using System;
using System.Collections.Generic;

namespace ThunderRoad
{
	// Token: 0x0200034E RID: 846
	public abstract class LambdaEventHandler : ThunderBehaviour
	{
		// Token: 0x06002788 RID: 10120 RVA: 0x00110C54 File Offset: 0x0010EE54
		public virtual void CheckConfiguredEvents()
		{
			this.UnsubscribeAllAnonymous();
			if (this.anonymousHandlerUnsubscribers == null)
			{
				this.anonymousHandlerUnsubscribers = new List<Action>();
			}
		}

		// Token: 0x06002789 RID: 10121 RVA: 0x00110C70 File Offset: 0x0010EE70
		protected void UnsubscribeAllAnonymous()
		{
			if (this.anonymousHandlerUnsubscribers.IsNullOrEmpty())
			{
				return;
			}
			foreach (Action action in this.anonymousHandlerUnsubscribers)
			{
				action();
			}
			this.anonymousHandlerUnsubscribers.Clear();
		}

		// Token: 0x0600278A RID: 10122 RVA: 0x00110CDC File Offset: 0x0010EEDC
		protected virtual void OnDestroy()
		{
			this.UnsubscribeAllAnonymous();
		}

		// Token: 0x040026A5 RID: 9893
		protected List<Action> anonymousHandlerUnsubscribers;
	}
}
