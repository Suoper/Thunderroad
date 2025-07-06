using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002C0 RID: 704
	public class DistanceEvent : MonoBehaviour
	{
		// Token: 0x06002214 RID: 8724 RVA: 0x000EB0A9 File Offset: 0x000E92A9
		private void OnEnable()
		{
			base.InvokeRepeating("CheckDistance", this.checkInterval + UnityEngine.Random.value * this.checkInterval, this.checkInterval);
		}

		// Token: 0x06002215 RID: 8725 RVA: 0x000EB0CF File Offset: 0x000E92CF
		private void OnDisable()
		{
			base.CancelInvoke();
		}

		// Token: 0x06002216 RID: 8726 RVA: 0x000EB0D8 File Offset: 0x000E92D8
		private void CheckDistance()
		{
			float distanceRatio2;
			if (this.usePlayerHeadAsReference)
			{
				float distanceRatio;
				if (Player.local && Player.local.head.transform.position.PointInRadius(base.transform.position, this.radius, out distanceRatio))
				{
					if (!this.inRadius)
					{
						UnityEvent unityEvent = this.onRadiusEnter;
						if (unityEvent != null)
						{
							unityEvent.Invoke();
						}
						this.inRadius = true;
						return;
					}
				}
				else if (this.inRadius)
				{
					UnityEvent unityEvent2 = this.onRadiusExit;
					if (unityEvent2 != null)
					{
						unityEvent2.Invoke();
					}
					this.inRadius = false;
					return;
				}
			}
			else if (this.reference.transform.position.PointInRadius(base.transform.position, this.radius, out distanceRatio2))
			{
				if (!this.inRadius)
				{
					UnityEvent unityEvent3 = this.onRadiusEnter;
					if (unityEvent3 != null)
					{
						unityEvent3.Invoke();
					}
					this.inRadius = true;
					return;
				}
			}
			else if (this.inRadius)
			{
				UnityEvent unityEvent4 = this.onRadiusExit;
				if (unityEvent4 != null)
				{
					unityEvent4.Invoke();
				}
				this.inRadius = false;
			}
		}

		// Token: 0x06002217 RID: 8727 RVA: 0x000EB1DD File Offset: 0x000E93DD
		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(base.transform.position, this.radius);
		}

		// Token: 0x04002105 RID: 8453
		public bool usePlayerHeadAsReference;

		// Token: 0x04002106 RID: 8454
		public Transform reference;

		// Token: 0x04002107 RID: 8455
		public float radius = 2f;

		// Token: 0x04002108 RID: 8456
		public float checkInterval = 1f;

		// Token: 0x04002109 RID: 8457
		public UnityEvent onRadiusEnter;

		// Token: 0x0400210A RID: 8458
		public UnityEvent onRadiusExit;

		// Token: 0x0400210B RID: 8459
		public bool inRadius;

		// Token: 0x02000999 RID: 2457
		public enum DisableBehavior
		{
			// Token: 0x04004522 RID: 17698
			None,
			// Token: 0x04004523 RID: 17699
			OutputLastCurveValue,
			// Token: 0x04004524 RID: 17700
			OutputFirstCurveValue
		}
	}
}
