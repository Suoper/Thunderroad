using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200029E RID: 670
	public class FxModuleParticleCollision : MonoBehaviour
	{
		// Token: 0x06001F6B RID: 8043 RVA: 0x000D5EAE File Offset: 0x000D40AE
		private void Awake()
		{
			this.particle = base.GetComponent<ParticleSystem>();
			this.collisionEvents = new List<ParticleCollisionEvent>();
		}

		// Token: 0x06001F6C RID: 8044 RVA: 0x000D5EC8 File Offset: 0x000D40C8
		private void OnParticleCollision(GameObject other)
		{
			if (!base.enabled)
			{
				return;
			}
			this.aliveEvents = this.particle.GetCollisionEvents(other, this.collisionEvents);
			if (this.aliveEvents > 0 && Time.time - this.lastEmitTime > this.emitRate)
			{
				FxModuleParticle fxModuleParticle = UnityEngine.Object.Instantiate<FxModuleParticle>(this.spawnFxModuleParticle);
				fxModuleParticle.transform.position = this.collisionEvents[0].intersection;
				fxModuleParticle.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0f, 360f), this.collisionEvents[0].normal) * Quaternion.LookRotation(this.collisionEvents[0].normal);
				fxModuleParticle.gameObject.SetActive(true);
				fxModuleParticle.SetIntensity(1f);
				fxModuleParticle.Play();
				this.lastEmitTime = Time.time;
			}
		}

		// Token: 0x04001E89 RID: 7817
		public FxModuleParticle spawnFxModuleParticle;

		// Token: 0x04001E8A RID: 7818
		public float maxGroundAngle = 45f;

		// Token: 0x04001E8B RID: 7819
		public float emitRate = 0.2f;

		// Token: 0x04001E8C RID: 7820
		public float minIntensity;

		// Token: 0x04001E8D RID: 7821
		public float maxIntensity = 1f;

		// Token: 0x04001E8E RID: 7822
		public bool useMainGradient;

		// Token: 0x04001E8F RID: 7823
		public bool useSecondaryGradient;

		// Token: 0x04001E90 RID: 7824
		protected ParticleSystem particle;

		// Token: 0x04001E91 RID: 7825
		protected List<ParticleCollisionEvent> collisionEvents;

		// Token: 0x04001E92 RID: 7826
		private float lastEmitTime;

		// Token: 0x04001E93 RID: 7827
		private int aliveEvents;
	}
}
