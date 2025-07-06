using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200028E RID: 654
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectShader.html")]
	public class EffectShader : Effect
	{
		// Token: 0x06001ED4 RID: 7892 RVA: 0x000D2424 File Offset: 0x000D0624
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.Awake();
		}

		// Token: 0x06001ED5 RID: 7893 RVA: 0x000D243C File Offset: 0x000D063C
		private void Awake()
		{
			if (EffectShader.colorPropertyID == 0)
			{
				EffectShader.colorPropertyID = Shader.PropertyToID("_Color");
			}
			if (EffectShader.emissionPropertyID == 0)
			{
				EffectShader.emissionPropertyID = Shader.PropertyToID("_EmissionColor");
			}
			if (EffectShader.useEmissionPropertyID == 0)
			{
				EffectShader.useEmissionPropertyID = Shader.PropertyToID("_UseEmission");
			}
		}

		// Token: 0x06001ED6 RID: 7894 RVA: 0x000D248C File Offset: 0x000D068C
		public override void Play()
		{
			base.CancelInvoke();
			Effect.Step step = this.step;
			if ((step == Effect.Step.Start || step == Effect.Step.End) && this.lifeTime > 0f)
			{
				base.InvokeRepeating("UpdateLifeTime", 0f, this.refreshSpeed);
			}
			this.SetIntensity(this.currentValue, false);
			this.playTime = Time.time;
		}

		// Token: 0x06001ED7 RID: 7895 RVA: 0x000D24E8 File Offset: 0x000D06E8
		public override void Stop()
		{
			this.SetIntensity(0f, false);
		}

		// Token: 0x06001ED8 RID: 7896 RVA: 0x000D24F6 File Offset: 0x000D06F6
		public override void End(bool loopOnly = false)
		{
			this.Despawn();
		}

		// Token: 0x06001ED9 RID: 7897 RVA: 0x000D2500 File Offset: 0x000D0700
		protected void UpdateLifeTime()
		{
			float value = Mathf.Clamp01(1f - (Time.time - this.playTime) / this.lifeTime);
			this.SetIntensity(value, false);
			if (value == 0f)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001EDA RID: 7898 RVA: 0x000D2542 File Offset: 0x000D0742
		public override void SetRenderer(Renderer renderer, bool secondary)
		{
			if ((this.useSecondaryRenderer && secondary) || (!this.useSecondaryRenderer && !secondary))
			{
				this.materialInstance = renderer.GetComponent<MaterialInstance>();
			}
		}

		// Token: 0x06001EDB RID: 7899 RVA: 0x000D2568 File Offset: 0x000D0768
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if (loopOnly && this.step != Effect.Step.Loop)
			{
				return;
			}
			this.currentValue = value;
			if (!this.materialInstance || !this.materialInstance.CachedRenderer || (!this.materialInstance.CachedRenderer.isVisible && value != 0f))
			{
				return;
			}
			EffectTarget effectTarget = this.linkBaseColor;
			if (effectTarget != EffectTarget.Main)
			{
				if (effectTarget == EffectTarget.Secondary)
				{
					if (this.currentSecondaryGradient != null)
					{
						this.materialInstance.material.SetColor(EffectShader.colorPropertyID, this.currentSecondaryGradient.Evaluate(value));
					}
				}
			}
			else if (this.currentMainGradient != null)
			{
				this.materialInstance.material.SetColor(EffectShader.colorPropertyID, this.currentMainGradient.Evaluate(value));
			}
			effectTarget = this.linkEmissionColor;
			if (effectTarget != EffectTarget.Main)
			{
				if (effectTarget == EffectTarget.Secondary)
				{
					if (this.currentSecondaryGradient != null)
					{
						this.materialInstance.material.SetFloat(EffectShader.useEmissionPropertyID, 1f);
						this.materialInstance.material.SetColor(EffectShader.emissionPropertyID, this.currentSecondaryGradient.Evaluate(value));
					}
				}
			}
			else if (this.currentMainGradient != null)
			{
				this.materialInstance.material.SetFloat(EffectShader.useEmissionPropertyID, 1f);
				this.materialInstance.material.SetColor(EffectShader.emissionPropertyID, this.currentMainGradient.Evaluate(value));
			}
			effectTarget = this.linkExtraColor;
			if (effectTarget != EffectTarget.Main)
			{
				if (effectTarget != EffectTarget.Secondary)
				{
					return;
				}
				if (this.currentSecondaryGradient != null)
				{
					this.materialInstance.material.SetColor(this.extraPropertyId, this.currentSecondaryGradient.Evaluate(value));
				}
			}
			else if (this.currentMainGradient != null)
			{
				this.materialInstance.material.SetColor(this.extraPropertyId, this.currentMainGradient.Evaluate(value));
				return;
			}
		}

		// Token: 0x06001EDC RID: 7900 RVA: 0x000D2734 File Offset: 0x000D0934
		public override void SetMainGradient(Gradient gradient)
		{
			this.currentMainGradient = gradient;
			this.SetIntensity(this.currentValue, false);
		}

		// Token: 0x06001EDD RID: 7901 RVA: 0x000D274A File Offset: 0x000D094A
		public override void SetSecondaryGradient(Gradient gradient)
		{
			this.currentSecondaryGradient = gradient;
			this.SetIntensity(this.currentValue, false);
		}

		// Token: 0x06001EDE RID: 7902 RVA: 0x000D2760 File Offset: 0x000D0960
		public override void Despawn()
		{
			base.CancelInvoke();
			this.SetIntensity(0f, false);
			this.materialInstance = null;
			this.currentValue = 0f;
			this.currentMainGradient = null;
			this.currentSecondaryGradient = null;
			if (Application.isPlaying)
			{
				EffectModuleShader.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001DB1 RID: 7601
		public EffectTarget linkBaseColor;

		// Token: 0x04001DB2 RID: 7602
		public EffectTarget linkEmissionColor;

		// Token: 0x04001DB3 RID: 7603
		public EffectTarget linkExtraColor;

		// Token: 0x04001DB4 RID: 7604
		public float lifeTime;

		// Token: 0x04001DB5 RID: 7605
		public float refreshSpeed = 0.1f;

		// Token: 0x04001DB6 RID: 7606
		public bool useSecondaryRenderer;

		// Token: 0x04001DB7 RID: 7607
		public int extraPropertyId;

		// Token: 0x04001DB8 RID: 7608
		[NonSerialized]
		public float playTime;

		// Token: 0x04001DB9 RID: 7609
		protected MaterialInstance materialInstance;

		// Token: 0x04001DBA RID: 7610
		protected static int colorPropertyID;

		// Token: 0x04001DBB RID: 7611
		protected static int emissionPropertyID;

		// Token: 0x04001DBC RID: 7612
		protected static int useEmissionPropertyID;

		// Token: 0x04001DBD RID: 7613
		protected float currentValue;

		// Token: 0x04001DBE RID: 7614
		protected Gradient currentMainGradient;

		// Token: 0x04001DBF RID: 7615
		protected Gradient currentSecondaryGradient;
	}
}
