using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002BF RID: 703
	public class DistanceBlend : MonoBehaviour
	{
		// Token: 0x06002210 RID: 8720 RVA: 0x000EAEE0 File Offset: 0x000E90E0
		private void OnDisable()
		{
			if (this.disableBehavior != DistanceBlend.DisableBehavior.OutputLastCurveValue)
			{
				if (this.disableBehavior == DistanceBlend.DisableBehavior.OutputFirstCurveValue)
				{
					UnityEvent<float> unityEvent = this.output;
					if (unityEvent == null)
					{
						return;
					}
					unityEvent.Invoke(this.curve.GetFirstValue());
				}
				return;
			}
			UnityEvent<float> unityEvent2 = this.output;
			if (unityEvent2 == null)
			{
				return;
			}
			unityEvent2.Invoke(this.curve.GetLastValue());
		}

		// Token: 0x06002211 RID: 8721 RVA: 0x000EAF38 File Offset: 0x000E9138
		private void Update()
		{
			float distanceRatio2;
			if (this.usePlayerHeadAsReference)
			{
				float distanceRatio;
				if (Player.local && Player.local.head.transform.position.PointInRadius(base.transform.position, this.radius, out distanceRatio))
				{
					UnityEvent<float> unityEvent = this.output;
					if (unityEvent == null)
					{
						return;
					}
					unityEvent.Invoke(this.curve.Evaluate(distanceRatio));
					return;
				}
				else
				{
					UnityEvent<float> unityEvent2 = this.output;
					if (unityEvent2 == null)
					{
						return;
					}
					unityEvent2.Invoke(this.curve.GetFirstValue());
					return;
				}
			}
			else if (this.reference.transform.position.PointInRadius(base.transform.position, this.radius, out distanceRatio2))
			{
				UnityEvent<float> unityEvent3 = this.output;
				if (unityEvent3 == null)
				{
					return;
				}
				unityEvent3.Invoke(this.curve.Evaluate(distanceRatio2));
				return;
			}
			else
			{
				UnityEvent<float> unityEvent4 = this.output;
				if (unityEvent4 == null)
				{
					return;
				}
				unityEvent4.Invoke(this.curve.GetFirstValue());
				return;
			}
		}

		// Token: 0x06002212 RID: 8722 RVA: 0x000EB022 File Offset: 0x000E9222
		private void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(base.transform.position, this.radius);
		}

		// Token: 0x040020FF RID: 8447
		public bool usePlayerHeadAsReference;

		// Token: 0x04002100 RID: 8448
		public Transform reference;

		// Token: 0x04002101 RID: 8449
		public float radius = 2f;

		// Token: 0x04002102 RID: 8450
		public AnimationCurve curve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04002103 RID: 8451
		public DistanceBlend.DisableBehavior disableBehavior = DistanceBlend.DisableBehavior.OutputLastCurveValue;

		// Token: 0x04002104 RID: 8452
		public UnityEvent<float> output = new UnityEvent<float>();

		// Token: 0x02000998 RID: 2456
		public enum DisableBehavior
		{
			// Token: 0x0400451E RID: 17694
			None,
			// Token: 0x0400451F RID: 17695
			OutputLastCurveValue,
			// Token: 0x04004520 RID: 17696
			OutputFirstCurveValue
		}
	}
}
