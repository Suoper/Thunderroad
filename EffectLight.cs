using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000289 RID: 649
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectLight.html")]
	[ExecuteInEditMode]
	public class EffectLight : Effect
	{
		// Token: 0x06001E9E RID: 7838 RVA: 0x000D044C File Offset: 0x000CE64C
		private void Awake()
		{
			this.pointLight = base.GetComponent<Light>();
			if (!this.pointLight)
			{
				this.pointLight = base.gameObject.AddComponent<Light>();
				LightManager.AddLight(this.pointLight);
			}
			this.pointLight.range = 0f;
			this.pointLight.intensity = 0f;
			this.pointLight.type = LightType.Point;
			this.pointLight.enabled = false;
		}

		// Token: 0x06001E9F RID: 7839 RVA: 0x000D04C8 File Offset: 0x000CE6C8
		public override void Play()
		{
			base.Play();
			this.pointLight.enabled = true;
			this.playTime = Time.time;
			this.stopTime = Time.time;
			this.stopping = (this.step != Effect.Step.Loop);
			this.stopIntensity = this.intensity;
		}

		// Token: 0x06001EA0 RID: 7840 RVA: 0x000D051B File Offset: 0x000CE71B
		public override void Stop()
		{
			this.pointLight.enabled = false;
		}

		// Token: 0x06001EA1 RID: 7841 RVA: 0x000D0529 File Offset: 0x000CE729
		public override void End(bool loopOnly = false)
		{
			this.stopIntensity = this.intensity;
			this.stopTime = Time.time;
			this.stopping = true;
			if (this.loopFadeDelay == 0f)
			{
				this.Despawn();
			}
		}

		// Token: 0x170001E8 RID: 488
		// (get) Token: 0x06001EA2 RID: 7842 RVA: 0x000D055C File Offset: 0x000CE75C
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001EA3 RID: 7843 RVA: 0x000D0560 File Offset: 0x000CE760
		protected internal override void ManagedUpdate()
		{
			if (this.intensitySmoothFactor > 0f)
			{
				this.intensity = Mathf.Lerp(this.intensity, this.targetIntensity, Time.deltaTime * this.intensitySmoothFactor);
			}
			if (this.rangeSmoothFactor > 0f)
			{
				this.range = Mathf.Lerp(this.range, this.targetRange, Time.deltaTime * this.rangeSmoothFactor);
			}
			if (this.colorSmoothFactor > 0f)
			{
				this.color = Color.Lerp(this.color, this.targetColor, Time.deltaTime * this.colorSmoothFactor);
			}
			this.pointLight.intensity = this.intensity * this.flickerIntensityCurve.Evaluate(Mathf.PerlinNoise(Time.time * this.flickerRateCurve.Evaluate(this.intensity), 0f));
			this.pointLight.range = this.range;
			this.pointLight.color = this.color;
			if (this.stopping)
			{
				float amount;
				if ((amount = Time.time - this.stopTime) < this.loopFadeDelay)
				{
					this.SetIntensity((1f - amount / this.loopFadeDelay) * this.stopIntensity, false);
					return;
				}
				this.stopping = false;
				this.Despawn();
			}
		}

		// Token: 0x06001EA4 RID: 7844 RVA: 0x000D06A8 File Offset: 0x000CE8A8
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				this.targetIntensity = this.intensityCurve.Evaluate(value);
				if (this.intensitySmoothFactor == 0f)
				{
					this.intensity = this.targetIntensity;
				}
				this.targetColor = this.colorCurve.Evaluate(value);
				if (this.colorSmoothFactor == 0f)
				{
					this.color = this.targetColor;
				}
				this.targetRange = this.rangeCurve.Evaluate(value);
				if (this.rangeSmoothFactor == 0f)
				{
					this.range = this.targetRange;
				}
			}
		}

		// Token: 0x06001EA5 RID: 7845 RVA: 0x000D0753 File Offset: 0x000CE953
		public override void SetMainGradient(Gradient gradient)
		{
			base.SetMainGradient(gradient);
			this.colorCurve = gradient;
		}

		// Token: 0x06001EA6 RID: 7846 RVA: 0x000D0763 File Offset: 0x000CE963
		public override void Despawn()
		{
			base.Despawn();
			this.pointLight.enabled = false;
			if (Application.isPlaying)
			{
				EffectModuleLight.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001D30 RID: 7472
		public Light pointLight;

		// Token: 0x04001D31 RID: 7473
		public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		});

		// Token: 0x04001D32 RID: 7474
		public AnimationCurve rangeCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		});

		// Token: 0x04001D33 RID: 7475
		public Gradient colorCurve = new Gradient();

		// Token: 0x04001D34 RID: 7476
		public float intensitySmoothFactor;

		// Token: 0x04001D35 RID: 7477
		public float rangeSmoothFactor;

		// Token: 0x04001D36 RID: 7478
		public float colorSmoothFactor;

		// Token: 0x04001D37 RID: 7479
		public AnimationCurve flickerIntensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0.5f),
			new Keyframe(1f, 1.5f)
		});

		// Token: 0x04001D38 RID: 7480
		public AnimationCurve flickerRateCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		// Token: 0x04001D39 RID: 7481
		public float loopFadeDelay;

		// Token: 0x04001D3A RID: 7482
		public float playTime;

		// Token: 0x04001D3B RID: 7483
		public float stopTime;

		// Token: 0x04001D3C RID: 7484
		private bool stopping;

		// Token: 0x04001D3D RID: 7485
		private float stopIntensity;

		// Token: 0x04001D3E RID: 7486
		private float intensity;

		// Token: 0x04001D3F RID: 7487
		private float range;

		// Token: 0x04001D40 RID: 7488
		private Color color;

		// Token: 0x04001D41 RID: 7489
		private float targetIntensity;

		// Token: 0x04001D42 RID: 7490
		private float targetRange;

		// Token: 0x04001D43 RID: 7491
		private Color targetColor;
	}
}
