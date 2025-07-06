using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200029F RID: 671
	public class FxModuleParticleTimeSpeed : MonoBehaviour
	{
		// Token: 0x06001F6E RID: 8046 RVA: 0x000D5FE4 File Offset: 0x000D41E4
		private void Awake()
		{
			this.particleSystem = base.GetComponent<ParticleSystem>();
			this.particleSystemMain = this.particleSystem.main;
			this.particles = new ParticleSystem.Particle[this.particleSystem.main.maxParticles];
		}

		// Token: 0x06001F6F RID: 8047 RVA: 0x000D602C File Offset: 0x000D422C
		private void Update()
		{
			if (Time.timeScale != this.previousTimeScale)
			{
				this.SetSpeed(Mathf.Lerp(this.slowTimeMultiplier, 1f, Mathf.InverseLerp(this.minTimeScale, 1f, Time.timeScale)));
				this.previousTimeScale = Time.timeScale;
			}
		}

		// Token: 0x06001F70 RID: 8048 RVA: 0x000D607C File Offset: 0x000D427C
		public void SetSpeed(float speedRatio)
		{
			this.particleSystemMain.startSpeed = new ParticleSystem.MinMaxCurve(this.startSpeed);
			int aliveParticlesCount = this.particleSystem.GetParticles(this.particles);
			for (int i = 0; i < aliveParticlesCount; i++)
			{
				this.particles[i].velocity = this.particles[i].velocity.normalized * (this.startSpeed * speedRatio);
			}
			this.particleSystem.SetParticles(this.particles, aliveParticlesCount);
			this.particleSystemMain.startSpeed = new ParticleSystem.MinMaxCurve(this.startSpeed * speedRatio);
			this.particleSystemMain.gravityModifier = new ParticleSystem.MinMaxCurve(this.gravity * speedRatio);
		}

		// Token: 0x04001E94 RID: 7828
		public float startSpeed = 300f;

		// Token: 0x04001E95 RID: 7829
		public float gravity = 1f;

		// Token: 0x04001E96 RID: 7830
		public float minTimeScale = 0.25f;

		// Token: 0x04001E97 RID: 7831
		public float slowTimeMultiplier = 0.1f;

		// Token: 0x04001E98 RID: 7832
		protected ParticleSystem particleSystem;

		// Token: 0x04001E99 RID: 7833
		protected ParticleSystem.MainModule particleSystemMain;

		// Token: 0x04001E9A RID: 7834
		private ParticleSystem.Particle[] particles;

		// Token: 0x04001E9B RID: 7835
		protected float previousTimeScale = 1f;
	}
}
