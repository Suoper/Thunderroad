using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200033E RID: 830
	[RequireComponent(typeof(Light))]
	public class Flicker : MonoBehaviour
	{
		// Token: 0x060026DF RID: 9951 RVA: 0x0010C43A File Offset: 0x0010A63A
		public void On(bool lerped = true)
		{
			this.lightActive = true;
			if (!lerped)
			{
				this.lightSource.intensity = this.targetIntensity;
			}
		}

		// Token: 0x060026E0 RID: 9952 RVA: 0x0010C457 File Offset: 0x0010A657
		public void Off(bool lerped = true)
		{
			this.lightActive = false;
			if (!lerped)
			{
				this.lightSource.intensity = 0f;
			}
		}

		// Token: 0x060026E1 RID: 9953 RVA: 0x0010C473 File Offset: 0x0010A673
		private void Start()
		{
			this.lightSource = base.GetComponent<Light>();
			this.seed = (float)UnityEngine.Random.Range(0, 10000);
		}

		// Token: 0x060026E2 RID: 9954 RVA: 0x0010C494 File Offset: 0x0010A694
		private void Update()
		{
			float target = this.lightActive ? this.targetIntensity : 0f;
			this.actualIntensity = Mathf.Lerp(this.actualIntensity, target, Time.deltaTime * this.lerpRate);
			this.flicker = 1f + this.flickerAmount * (2f * Mathf.Clamp01(Mathf.PerlinNoise(Time.time * this.flickerRate, this.seed)) - 1f);
			this.lightSource.intensity = this.actualIntensity * this.flicker;
		}

		// Token: 0x0400262A RID: 9770
		public float targetIntensity = 1f;

		// Token: 0x0400262B RID: 9771
		public float lerpRate = 10f;

		// Token: 0x0400262C RID: 9772
		public float flickerRate = 10f;

		// Token: 0x0400262D RID: 9773
		public float flickerAmount = 0.5f;

		// Token: 0x0400262E RID: 9774
		public bool lightActive;

		// Token: 0x0400262F RID: 9775
		public float flicker;

		// Token: 0x04002630 RID: 9776
		private Light lightSource;

		// Token: 0x04002631 RID: 9777
		private float actualIntensity;

		// Token: 0x04002632 RID: 9778
		private float seed;
	}
}
