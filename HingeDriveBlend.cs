using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002D1 RID: 721
	public class HingeDriveBlend : MonoBehaviour
	{
		// Token: 0x060022E2 RID: 8930 RVA: 0x000EFA78 File Offset: 0x000EDC78
		private void OnDisable()
		{
			if (this.disableBehavior != HingeDriveBlend.DisableBehavior.OutputLastCurveValue)
			{
				if (this.disableBehavior == HingeDriveBlend.DisableBehavior.OutputFirstCurveValue)
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

		// Token: 0x060022E3 RID: 8931 RVA: 0x000EFAD0 File Offset: 0x000EDCD0
		private void Update()
		{
			if (this.hingeDrive.HingeAngle >= this.referenceAngle)
			{
				this.hingeAngleRatio = Mathf.InverseLerp(this.referenceAngle, this.hingeDrive.maxAngle, this.hingeDrive.HingeAngle);
			}
			else
			{
				this.hingeAngleRatio = Mathf.InverseLerp(this.referenceAngle, this.hingeDrive.minAngle, this.hingeDrive.HingeAngle);
			}
			UnityEvent<float> unityEvent = this.output;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(this.curve.Evaluate(this.hingeAngleRatio));
		}

		// Token: 0x040021EC RID: 8684
		public HingeDrive hingeDrive;

		// Token: 0x040021ED RID: 8685
		public float referenceAngle;

		// Token: 0x040021EE RID: 8686
		public AnimationCurve curve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x040021EF RID: 8687
		public HingeDriveBlend.DisableBehavior disableBehavior = HingeDriveBlend.DisableBehavior.OutputLastCurveValue;

		// Token: 0x040021F0 RID: 8688
		public UnityEvent<float> output = new UnityEvent<float>();

		// Token: 0x040021F1 RID: 8689
		private float hingeAngleRatio;

		// Token: 0x020009B1 RID: 2481
		public enum DisableBehavior
		{
			// Token: 0x04004585 RID: 17797
			None,
			// Token: 0x04004586 RID: 17798
			OutputLastCurveValue,
			// Token: 0x04004587 RID: 17799
			OutputFirstCurveValue
		}
	}
}
