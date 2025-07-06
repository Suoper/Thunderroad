using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200029D RID: 669
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModuleParticle.html")]
	public class FxModuleParticle : FxModule
	{
		// Token: 0x06001F5D RID: 8029 RVA: 0x000D593E File Offset: 0x000D3B3E
		private void OnValidate()
		{
			this.Awake();
		}

		// Token: 0x06001F5E RID: 8030 RVA: 0x000D5948 File Offset: 0x000D3B48
		private void Awake()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
			if (base.TryGetComponent<ParticleSystem>(out this.particleSystem))
			{
				this.particleSystemMain = this.particleSystem.main;
				this.particleSystemRenderer = this.particleSystem.GetComponent<ParticleSystemRenderer>();
				return;
			}
			Debug.LogError("FXModuleParticle " + base.gameObject.GetPathFromRoot() + " is missing its particleSYstem");
		}

		// Token: 0x06001F5F RID: 8031 RVA: 0x000D59B0 File Offset: 0x000D3BB0
		public override bool IsPlaying()
		{
			return this.particleSystem.isPlaying;
		}

		// Token: 0x06001F60 RID: 8032 RVA: 0x000D59BD File Offset: 0x000D3BBD
		public override void Play()
		{
			base.Play();
			this.particleSystem.Play();
			if (LightProbeVolume.Exists)
			{
				LightVolumeReceiver.ApplyProbeVolume(this.particleSystemRenderer, this.materialPropertyBlock);
				return;
			}
			LightVolumeReceiver.DisableProbeVolume(this.particleSystemRenderer);
		}

		// Token: 0x06001F61 RID: 8033 RVA: 0x000D59F8 File Offset: 0x000D3BF8
		public override void SetIntensity(float intensity)
		{
			if (this.emissionRateOverTimeLink == FxModule.Link.Intensity)
			{
				this.SetParticleEmissionRateOverTime(intensity);
			}
			if (this.emissionRateOverDistanceLink == FxModule.Link.Intensity)
			{
				this.SetParticleEmissionRateOverDistance(intensity);
			}
			if (this.lifeTimeLink == FxModule.Link.Intensity)
			{
				this.SetParticleLifetime(intensity);
			}
			if (this.emissionBurstLink == FxModule.Link.Intensity)
			{
				this.SetParticleBurst(intensity);
			}
			if (this.scaleLink == FxModule.Link.Intensity)
			{
				this.SetParticleScale(intensity);
			}
			if (this.startSpeedLink == FxModule.Link.Intensity)
			{
				this.SetParticleSpeed(intensity);
			}
		}

		// Token: 0x06001F62 RID: 8034 RVA: 0x000D5A68 File Offset: 0x000D3C68
		public override void SetSpeed(float speed)
		{
			if (this.emissionRateOverTimeLink == FxModule.Link.Speed)
			{
				this.SetParticleEmissionRateOverTime(speed);
			}
			if (this.emissionRateOverDistanceLink == FxModule.Link.Speed)
			{
				this.SetParticleEmissionRateOverDistance(speed);
			}
			if (this.lifeTimeLink == FxModule.Link.Speed)
			{
				this.SetParticleLifetime(speed);
			}
			if (this.emissionBurstLink == FxModule.Link.Speed)
			{
				this.SetParticleBurst(speed);
			}
			if (this.scaleLink == FxModule.Link.Speed)
			{
				this.SetParticleScale(speed);
			}
			if (this.startSpeedLink == FxModule.Link.Speed)
			{
				this.SetParticleSpeed(speed);
			}
		}

		// Token: 0x06001F63 RID: 8035 RVA: 0x000D5AD8 File Offset: 0x000D3CD8
		protected void SetParticleEmissionRateOverTime(float value)
		{
			this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
			float rate = Mathf.Lerp(this.emissionRateOverTimeRange.x, this.emissionRateOverTimeRange.y, this.emissionRateOverTimeCurve.Evaluate(value));
			this.minMaxCurve.constantMin = Mathf.Clamp(rate - this.emissionRateOverTimeReduction, this.emissionRateOverTimeRange.x, this.emissionRateOverTimeRange.y);
			this.minMaxCurve.constantMax = rate;
			this.particleSystem.emission.rateOverTime = this.minMaxCurve;
		}

		// Token: 0x06001F64 RID: 8036 RVA: 0x000D5B6C File Offset: 0x000D3D6C
		protected void SetParticleEmissionRateOverDistance(float value)
		{
			this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
			float rate = Mathf.Lerp(this.emissionRateOverDistanceRange.x, this.emissionRateOverDistanceRange.y, this.emissionRateOverDistanceCurve.Evaluate(value));
			this.minMaxCurve.constantMin = Mathf.Clamp(rate - this.emissionRateOverDistanceReduction, this.emissionRateOverDistanceRange.x, this.emissionRateOverDistanceRange.y);
			this.minMaxCurve.constantMax = rate;
			this.particleSystem.emission.rateOverDistance = this.minMaxCurve;
		}

		// Token: 0x06001F65 RID: 8037 RVA: 0x000D5C00 File Offset: 0x000D3E00
		protected void SetParticleLifetime(float value)
		{
			this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
			float lifeTime = Mathf.Lerp(this.lifeTimeRange.x, this.lifeTimeRange.y, this.curveLifeTime.Evaluate(value));
			this.minMaxCurve.constantMin = Mathf.Clamp(lifeTime - this.lifeTimeReduction, this.lifeTimeRange.x, this.lifeTimeRange.y);
			this.minMaxCurve.constantMax = lifeTime;
			this.particleSystem.main.startLifetime = this.minMaxCurve;
		}

		// Token: 0x06001F66 RID: 8038 RVA: 0x000D5C94 File Offset: 0x000D3E94
		protected void SetParticleBurst(float value)
		{
			short burst = (short)this.emissionBurstCurve.Evaluate(value);
			ParticleSystem.Burst particleBurst = new ParticleSystem.Burst(0f, (short)Mathf.Clamp((float)(burst - this.emissionBurstRandomRange), 0f, float.PositiveInfinity), (short)Mathf.Clamp((float)burst, 0f, float.PositiveInfinity));
			this.particleSystem.emission.SetBurst(0, particleBurst);
		}

		// Token: 0x06001F67 RID: 8039 RVA: 0x000D5CFC File Offset: 0x000D3EFC
		protected void SetParticleScale(float value)
		{
			float size = this.scaleCurve.Evaluate(value);
			base.transform.localScale = new Vector3(this.scaleMultiplier.x * size, this.scaleMultiplier.y * size, this.scaleMultiplier.z * size);
		}

		// Token: 0x06001F68 RID: 8040 RVA: 0x000D5D50 File Offset: 0x000D3F50
		protected void SetParticleSpeed(float value)
		{
			this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
			float speed = this.startSpeedLinkCurve.Evaluate(value);
			this.minMaxCurve.constantMin = Mathf.Lerp(this.startSpeedLinkCurve.GetFirstValue(), speed, this.startSpeedMinSpeedRatio);
			this.minMaxCurve.constantMax = speed;
			this.particleSystem.main.startSpeed = this.minMaxCurve;
		}

		// Token: 0x06001F69 RID: 8041 RVA: 0x000D5DBD File Offset: 0x000D3FBD
		public override void Stop(bool playStopEffect = true)
		{
			this.particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
		}

		// Token: 0x04001E6F RID: 7791
		[Header("Emission Rate Over Time")]
		public FxModule.Link emissionRateOverTimeLink;

		// Token: 0x04001E70 RID: 7792
		public AnimationCurve emissionRateOverTimeCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001E71 RID: 7793
		public Vector2 emissionRateOverTimeRange = new Vector2(0f, 10f);

		// Token: 0x04001E72 RID: 7794
		public float emissionRateOverTimeReduction;

		// Token: 0x04001E73 RID: 7795
		[Header("Emission Rate Over Distance")]
		public FxModule.Link emissionRateOverDistanceLink;

		// Token: 0x04001E74 RID: 7796
		public AnimationCurve emissionRateOverDistanceCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001E75 RID: 7797
		public Vector2 emissionRateOverDistanceRange = new Vector2(0f, 10f);

		// Token: 0x04001E76 RID: 7798
		public float emissionRateOverDistanceReduction;

		// Token: 0x04001E77 RID: 7799
		[Header("Life time")]
		public FxModule.Link lifeTimeLink;

		// Token: 0x04001E78 RID: 7800
		public AnimationCurve curveLifeTime;

		// Token: 0x04001E79 RID: 7801
		public Vector2 lifeTimeRange = new Vector2(0f, 10f);

		// Token: 0x04001E7A RID: 7802
		public float lifeTimeReduction;

		// Token: 0x04001E7B RID: 7803
		[Header("Emission burst")]
		public FxModule.Link emissionBurstLink;

		// Token: 0x04001E7C RID: 7804
		public AnimationCurve emissionBurstCurve;

		// Token: 0x04001E7D RID: 7805
		public short emissionBurstRandomRange;

		// Token: 0x04001E7E RID: 7806
		[Header("Scale")]
		public FxModule.Link scaleLink;

		// Token: 0x04001E7F RID: 7807
		public AnimationCurve scaleCurve;

		// Token: 0x04001E80 RID: 7808
		public Vector3 scaleMultiplier = Vector3.one;

		// Token: 0x04001E81 RID: 7809
		[Header("Start speed")]
		public FxModule.Link startSpeedLink;

		// Token: 0x04001E82 RID: 7810
		public AnimationCurve startSpeedLinkCurve;

		// Token: 0x04001E83 RID: 7811
		[Range(0f, 1f)]
		public float startSpeedMinSpeedRatio = 1f;

		// Token: 0x04001E84 RID: 7812
		protected MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04001E85 RID: 7813
		protected ParticleSystem particleSystem;

		// Token: 0x04001E86 RID: 7814
		protected ParticleSystemRenderer particleSystemRenderer;

		// Token: 0x04001E87 RID: 7815
		protected ParticleSystem.MainModule particleSystemMain;

		// Token: 0x04001E88 RID: 7816
		protected ParticleSystem.MinMaxCurve minMaxCurve;
	}
}
