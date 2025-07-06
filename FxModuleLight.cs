using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200029A RID: 666
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModuleLight.html")]
	[RequireComponent(typeof(Light))]
	public class FxModuleLight : FxModule
	{
		// Token: 0x06001F49 RID: 8009 RVA: 0x000D51FD File Offset: 0x000D33FD
		private void OnValidate()
		{
			this.light = base.GetComponent<Light>();
		}

		// Token: 0x06001F4A RID: 8010 RVA: 0x000D520C File Offset: 0x000D340C
		private void Awake()
		{
			this.light = base.GetComponent<Light>();
			this.orgLightIntensity = this.light.intensity;
			this.currentLightIntensity = 0f;
			if (this.flicker)
			{
				base.StartCoroutine(this.FlickerCoroutine());
			}
			LightManager.AddLight(this.light);
		}

		// Token: 0x06001F4B RID: 8011 RVA: 0x000D5261 File Offset: 0x000D3461
		private IEnumerator FlickerCoroutine()
		{
			for (;;)
			{
				this.RefreshLightIntensity();
				yield return new WaitForSeconds(UnityEngine.Random.Range(this.flickerMinSpeed, this.flickerMaxSpeed));
			}
			yield break;
		}

		// Token: 0x06001F4C RID: 8012 RVA: 0x000D5270 File Offset: 0x000D3470
		protected void RefreshLightIntensity()
		{
			if (this.light == null)
			{
				return;
			}
			if (this.flicker)
			{
				this.light.intensity = Mathf.Clamp(this.currentLightIntensity - UnityEngine.Random.Range(0f, this.flickerIntensityReduction), 0f, float.PositiveInfinity);
			}
			else
			{
				this.light.intensity = this.currentLightIntensity;
			}
			this.light.enabled = (this.light.intensity > 0f);
		}

		// Token: 0x06001F4D RID: 8013 RVA: 0x000D52F5 File Offset: 0x000D34F5
		public override void Play()
		{
			base.Play();
			if (this.duration > 0f)
			{
				base.Invoke("Stop", this.duration);
			}
		}

		// Token: 0x06001F4E RID: 8014 RVA: 0x000D531B File Offset: 0x000D351B
		public override void SetIntensity(float intensity)
		{
			this.currentLightIntensity = this.orgLightIntensity * this.intensityCurve.Evaluate(intensity);
			this.RefreshLightIntensity();
		}

		// Token: 0x06001F4F RID: 8015 RVA: 0x000D533C File Offset: 0x000D353C
		public override void SetSpeed(float speed)
		{
		}

		// Token: 0x06001F50 RID: 8016 RVA: 0x000D533E File Offset: 0x000D353E
		public override void Stop(bool playStopEffect = true)
		{
			this.light.enabled = false;
		}

		// Token: 0x04001E5D RID: 7773
		public float duration;

		// Token: 0x04001E5E RID: 7774
		public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001E5F RID: 7775
		public bool flicker;

		// Token: 0x04001E60 RID: 7776
		public float flickerIntensityReduction = 0.2f;

		// Token: 0x04001E61 RID: 7777
		public float flickerMinSpeed = 0.01f;

		// Token: 0x04001E62 RID: 7778
		public float flickerMaxSpeed = 0.1f;

		// Token: 0x04001E63 RID: 7779
		protected Light light;

		// Token: 0x04001E64 RID: 7780
		protected float orgLightIntensity;

		// Token: 0x04001E65 RID: 7781
		protected float currentLightIntensity;
	}
}
