using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002A2 RID: 674
	public class TestEffect : MonoBehaviour
	{
		// Token: 0x06001F7E RID: 8062 RVA: 0x000D64B8 File Offset: 0x000D46B8
		public void OnValidate()
		{
			this.rootParticleSystem = base.GetComponent<ParticleSystem>();
			this.effects = new List<Effect>(base.GetComponentsInChildren<Effect>());
			if (!this.rootParticleSystem)
			{
				this.rootParticleSystem = base.gameObject.AddComponent<ParticleSystem>();
				this.rootParticleSystem.emission.enabled = false;
				this.rootParticleSystem.shape.enabled = false;
				this.rootParticleSystem.GetComponent<ParticleSystemRenderer>().enabled = false;
			}
			this.Refresh();
		}

		// Token: 0x06001F7F RID: 8063 RVA: 0x000D6540 File Offset: 0x000D4740
		public void Refresh()
		{
			foreach (Effect effect in this.effects)
			{
				if (this.target)
				{
					effect.SetTarget(this.target);
				}
				if (this.mesh)
				{
					effect.SetMesh(this.mesh);
				}
				if (this.mainRenderer)
				{
					effect.SetRenderer(this.mainRenderer, false);
				}
				if (this.secondaryRenderer)
				{
					effect.SetRenderer(this.secondaryRenderer, true);
				}
				if (this.collider)
				{
					effect.SetCollider(this.collider);
				}
				if (this.useMainGradient)
				{
					effect.SetMainGradient(this.mainGradient);
				}
				if (this.useSecondaryGradient)
				{
					effect.SetSecondaryGradient(this.secondaryGradient);
				}
				effect.SetIntensity(this.intensity, false);
				effect.SetSpeed(this.speed, false);
			}
		}

		// Token: 0x06001F80 RID: 8064 RVA: 0x000D6654 File Offset: 0x000D4854
		public void Play()
		{
			this.Refresh();
			this.rootParticleSystem.Play();
			foreach (Effect effect in this.effects)
			{
				if (effect.step != Effect.Step.End)
				{
					effect.Play();
				}
			}
		}

		// Token: 0x06001F81 RID: 8065 RVA: 0x000D66C0 File Offset: 0x000D48C0
		public void TestIntensity(float duration)
		{
			this.Play();
			base.StartCoroutine(this.TestIntensityAction(duration));
		}

		// Token: 0x06001F82 RID: 8066 RVA: 0x000D66D6 File Offset: 0x000D48D6
		private IEnumerator TestIntensityAction(float duration)
		{
			float startTime = Time.time;
			float t = 0f;
			while (t <= 1f)
			{
				t = (Time.time - startTime) / duration;
				this.intensity = Mathf.Lerp(0f, 1f, t);
				this.Refresh();
				yield return null;
			}
			yield break;
		}

		// Token: 0x06001F83 RID: 8067 RVA: 0x000D66EC File Offset: 0x000D48EC
		public void Stop()
		{
			this.rootParticleSystem.Stop();
			foreach (Effect effect in this.effects)
			{
				if (effect.step == Effect.Step.End)
				{
					effect.Play();
				}
				else
				{
					effect.End(false);
				}
			}
		}

		// Token: 0x04001EAA RID: 7850
		[Range(0f, 1f)]
		public float intensity;

		// Token: 0x04001EAB RID: 7851
		[Range(0f, 1f)]
		public float speed;

		// Token: 0x04001EAC RID: 7852
		public bool useMainGradient;

		// Token: 0x04001EAD RID: 7853
		[GradientUsage(true)]
		public Gradient mainGradient;

		// Token: 0x04001EAE RID: 7854
		public bool useSecondaryGradient;

		// Token: 0x04001EAF RID: 7855
		[GradientUsage(true)]
		public Gradient secondaryGradient;

		// Token: 0x04001EB0 RID: 7856
		public Transform target;

		// Token: 0x04001EB1 RID: 7857
		public Mesh mesh;

		// Token: 0x04001EB2 RID: 7858
		public Renderer mainRenderer;

		// Token: 0x04001EB3 RID: 7859
		public Renderer secondaryRenderer;

		// Token: 0x04001EB4 RID: 7860
		public Collider collider;

		// Token: 0x04001EB5 RID: 7861
		protected ParticleSystem rootParticleSystem;

		// Token: 0x04001EB6 RID: 7862
		protected List<Effect> effects = new List<Effect>();
	}
}
