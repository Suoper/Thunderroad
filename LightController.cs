using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000292 RID: 658
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/LightController")]
	public class LightController : MonoBehaviour
	{
		// Token: 0x06001F08 RID: 7944 RVA: 0x000D3D0E File Offset: 0x000D1F0E
		public virtual void SetActive(bool active)
		{
			Light component = base.GetComponent<Light>();
			component.enabled = active;
			LightManager.AddLight(component);
		}

		// Token: 0x06001F09 RID: 7945 RVA: 0x000D3D24 File Offset: 0x000D1F24
		public virtual void SetIntensity(float value)
		{
			Light light = base.GetComponent<Light>();
			if (this.maxIntensity > 0f)
			{
				light.intensity = Mathf.Lerp(this.minIntensity, this.maxIntensity, value);
			}
			if (this.maxRange > 0f)
			{
				light.range = Mathf.Lerp(this.minRange, this.maxRange, value);
			}
		}

		// Token: 0x06001F0A RID: 7946 RVA: 0x000D3D82 File Offset: 0x000D1F82
		public virtual void SetColor(Color mainColor)
		{
			base.GetComponent<Light>().color = mainColor;
		}

		// Token: 0x04001E07 RID: 7687
		[Header("Intensity to intensity (0 = disabled)")]
		public float minIntensity;

		// Token: 0x04001E08 RID: 7688
		public float maxIntensity;

		// Token: 0x04001E09 RID: 7689
		public float randomRangeIntensity;

		// Token: 0x04001E0A RID: 7690
		[Header("Intensity to range (0 = disabled)")]
		public float minRange;

		// Token: 0x04001E0B RID: 7691
		public float maxRange;

		// Token: 0x04001E0C RID: 7692
		public float randomRange;
	}
}
