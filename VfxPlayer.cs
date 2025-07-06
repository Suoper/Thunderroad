using System;
using UnityEngine;
using UnityEngine.VFX;

namespace ThunderRoad
{
	// Token: 0x020002A5 RID: 677
	public class VfxPlayer : MonoBehaviour
	{
		// Token: 0x06001F8A RID: 8074 RVA: 0x000D683C File Offset: 0x000D4A3C
		private void Awake()
		{
			if (Common.GetQualityLevel(false) == QualityLevel.Windows)
			{
				if (this.defaultParticleSystem)
				{
					this.defaultParticleSystem.gameObject.SetActive(true);
				}
				if (this.defaultVisualEffect)
				{
					this.defaultVisualEffect.gameObject.SetActive(true);
				}
				if (this.mobileParticleSystem)
				{
					this.mobileParticleSystem.gameObject.SetActive(false);
				}
				if (this.mobileVisualEffect)
				{
					this.mobileVisualEffect.gameObject.SetActive(false);
					return;
				}
			}
			else if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				if (this.defaultParticleSystem)
				{
					this.defaultParticleSystem.gameObject.SetActive(false);
				}
				if (this.defaultVisualEffect)
				{
					this.defaultVisualEffect.gameObject.SetActive(false);
				}
				if (this.mobileParticleSystem)
				{
					this.mobileParticleSystem.gameObject.SetActive(true);
				}
				if (this.mobileVisualEffect)
				{
					this.mobileVisualEffect.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x06001F8B RID: 8075 RVA: 0x000D694E File Offset: 0x000D4B4E
		private void Start()
		{
			if (this.playOnStart)
			{
				this.Play();
			}
		}

		// Token: 0x06001F8C RID: 8076 RVA: 0x000D6960 File Offset: 0x000D4B60
		public void ToggleActivePlatform()
		{
			if (this.mobileParticleSystem.gameObject.activeInHierarchy)
			{
				if (this.defaultParticleSystem)
				{
					this.defaultParticleSystem.gameObject.SetActive(true);
				}
				if (this.defaultVisualEffect)
				{
					this.defaultVisualEffect.gameObject.SetActive(true);
				}
				if (this.mobileParticleSystem)
				{
					this.mobileParticleSystem.gameObject.SetActive(false);
				}
				if (this.mobileVisualEffect)
				{
					this.mobileVisualEffect.gameObject.SetActive(false);
					return;
				}
			}
			else if (this.defaultParticleSystem.gameObject.activeInHierarchy)
			{
				if (this.defaultParticleSystem)
				{
					this.defaultParticleSystem.gameObject.SetActive(false);
				}
				if (this.defaultVisualEffect)
				{
					this.defaultVisualEffect.gameObject.SetActive(false);
				}
				if (this.mobileParticleSystem)
				{
					this.mobileParticleSystem.gameObject.SetActive(true);
				}
				if (this.mobileVisualEffect)
				{
					this.mobileVisualEffect.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x06001F8D RID: 8077 RVA: 0x000D6A88 File Offset: 0x000D4C88
		public void Play()
		{
			if (Common.GetQualityLevel(false) == QualityLevel.Windows)
			{
				if (this.defaultParticleSystem)
				{
					this.defaultParticleSystem.Play();
				}
				if (this.defaultVisualEffect)
				{
					this.defaultVisualEffect.Play();
					return;
				}
			}
			else if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				if (this.mobileParticleSystem)
				{
					this.mobileParticleSystem.Play();
				}
				if (this.mobileVisualEffect)
				{
					this.mobileVisualEffect.Play();
				}
			}
		}

		// Token: 0x06001F8E RID: 8078 RVA: 0x000D6B08 File Offset: 0x000D4D08
		public void Stop()
		{
			if (Common.GetQualityLevel(false) == QualityLevel.Windows)
			{
				if (this.defaultParticleSystem)
				{
					ParticleSystem particleSystem = this.defaultParticleSystem;
					if (particleSystem != null)
					{
						particleSystem.Stop();
					}
				}
				if (this.defaultVisualEffect)
				{
					VisualEffect visualEffect = this.defaultVisualEffect;
					if (visualEffect == null)
					{
						return;
					}
					visualEffect.Stop();
					return;
				}
			}
			else if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				if (this.mobileParticleSystem)
				{
					this.mobileParticleSystem.Stop();
				}
				if (this.mobileVisualEffect)
				{
					this.mobileVisualEffect.Stop();
				}
			}
		}

		// Token: 0x04001EB9 RID: 7865
		public bool playOnStart;

		// Token: 0x04001EBA RID: 7866
		[Header("Default")]
		[InspectorName("ParticleSystem")]
		public ParticleSystem defaultParticleSystem;

		// Token: 0x04001EBB RID: 7867
		[InspectorName("VisualEffect")]
		public VisualEffect defaultVisualEffect;

		// Token: 0x04001EBC RID: 7868
		[Header("Mobile")]
		[InspectorName("ParticleSystem")]
		public ParticleSystem mobileParticleSystem;

		// Token: 0x04001EBD RID: 7869
		[InspectorName("VisualEffect")]
		public VisualEffect mobileVisualEffect;
	}
}
