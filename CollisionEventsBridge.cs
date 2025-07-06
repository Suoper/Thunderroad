using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000316 RID: 790
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Event-Linkers/CollisionEventsBridge.html")]
	public class CollisionEventsBridge : MonoBehaviour
	{
		// Token: 0x06002596 RID: 9622 RVA: 0x00102681 File Offset: 0x00100881
		private void OnCollisionEnter(Collision collision)
		{
			this.onCollisionEnter.Invoke(collision);
		}

		// Token: 0x06002597 RID: 9623 RVA: 0x0010268F File Offset: 0x0010088F
		private void OnCollisionStay(Collision collision)
		{
			this.onCollisionStay.Invoke(collision);
		}

		// Token: 0x06002598 RID: 9624 RVA: 0x0010269D File Offset: 0x0010089D
		private void OnCollisionExit(Collision collision)
		{
			this.onCollisionExit.Invoke(collision);
		}

		// Token: 0x06002599 RID: 9625 RVA: 0x001026AB File Offset: 0x001008AB
		private void OnDestroy()
		{
			this.onCollisionEnter.RemoveAllListeners();
			this.onCollisionExit.RemoveAllListeners();
			this.onCollisionStay.RemoveAllListeners();
		}

		// Token: 0x04002513 RID: 9491
		public UnityEvent<Collision> onCollisionEnter = new UnityEvent<Collision>();

		// Token: 0x04002514 RID: 9492
		public UnityEvent<Collision> onCollisionExit = new UnityEvent<Collision>();

		// Token: 0x04002515 RID: 9493
		public UnityEvent<Collision> onCollisionStay = new UnityEvent<Collision>();
	}
}
