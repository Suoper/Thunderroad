using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002D9 RID: 729
	public class LerpModifier : MonoBehaviour
	{
		// Token: 0x06002330 RID: 9008 RVA: 0x000F112C File Offset: 0x000EF32C
		public void SetValue(float value)
		{
			float newValue = Mathf.Lerp(this.outputMin, this.outputMax, Mathf.InverseLerp(this.inputMin, this.inputMax, value));
			if (this.invertOutputRatio)
			{
				newValue = 1f - newValue;
			}
			UnityEvent<float> unityEvent = this.output;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(newValue);
		}

		// Token: 0x04002239 RID: 8761
		public float inputMin;

		// Token: 0x0400223A RID: 8762
		public float inputMax = 1f;

		// Token: 0x0400223B RID: 8763
		public bool invertOutputRatio;

		// Token: 0x0400223C RID: 8764
		public float outputMin;

		// Token: 0x0400223D RID: 8765
		public float outputMax = 1f;

		// Token: 0x0400223E RID: 8766
		public UnityEvent<float> output = new UnityEvent<float>();
	}
}
