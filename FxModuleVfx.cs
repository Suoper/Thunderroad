using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace ThunderRoad
{
	// Token: 0x020002A0 RID: 672
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModuleVfx.html")]
	public class FxModuleVfx : FxModule
	{
		// Token: 0x06001F72 RID: 8050 RVA: 0x000D6178 File Offset: 0x000D4378
		private void Awake()
		{
			this.vfx = base.GetComponent<VisualEffect>();
			List<FxModuleVfx.Property> list = new List<FxModuleVfx.Property>();
			list.AddRange(this.floatProperties);
			list.AddRange(this.colorProperties);
			foreach (FxModuleVfx.Property property in list)
			{
				property.id = Shader.PropertyToID(property.name);
				if (property.link == EffectLink.Intensity)
				{
					this.intensityProperties.Add(property);
				}
				else if (property.link == EffectLink.Speed)
				{
					this.speedProperties.Add(property);
				}
			}
		}

		// Token: 0x06001F73 RID: 8051 RVA: 0x000D6224 File Offset: 0x000D4424
		public override bool IsPlaying()
		{
			return this.vfx.aliveParticleCount > 0;
		}

		// Token: 0x06001F74 RID: 8052 RVA: 0x000D6237 File Offset: 0x000D4437
		public override void Play()
		{
			base.Play();
			this.vfx.Play();
		}

		// Token: 0x06001F75 RID: 8053 RVA: 0x000D624C File Offset: 0x000D444C
		public override void SetIntensity(float intensity)
		{
			foreach (FxModuleVfx.Property property in this.intensityProperties)
			{
				property.Update(this.vfx, intensity);
			}
		}

		// Token: 0x06001F76 RID: 8054 RVA: 0x000D62A4 File Offset: 0x000D44A4
		public override void SetSpeed(float speed)
		{
			foreach (FxModuleVfx.Property property in this.speedProperties)
			{
				property.Update(this.vfx, speed);
			}
		}

		// Token: 0x06001F77 RID: 8055 RVA: 0x000D62FC File Offset: 0x000D44FC
		public override void Stop(bool playStopEffect = true)
		{
			this.vfx.Stop();
		}

		// Token: 0x04001E9C RID: 7836
		public List<FxModuleVfx.Property.Float> floatProperties = new List<FxModuleVfx.Property.Float>();

		// Token: 0x04001E9D RID: 7837
		public List<FxModuleVfx.Property.Color> colorProperties = new List<FxModuleVfx.Property.Color>();

		// Token: 0x04001E9E RID: 7838
		protected List<FxModuleVfx.Property> intensityProperties = new List<FxModuleVfx.Property>();

		// Token: 0x04001E9F RID: 7839
		protected List<FxModuleVfx.Property> speedProperties = new List<FxModuleVfx.Property>();

		// Token: 0x04001EA0 RID: 7840
		protected VisualEffect vfx;

		// Token: 0x0200093C RID: 2364
		public class Property
		{
			// Token: 0x060042EA RID: 17130 RVA: 0x0018E3EF File Offset: 0x0018C5EF
			public virtual void Update(VisualEffect vfx, float value)
			{
			}

			// Token: 0x04004420 RID: 17440
			public string name;

			// Token: 0x04004421 RID: 17441
			public EffectLink link;

			// Token: 0x04004422 RID: 17442
			[NonSerialized]
			public int id;

			// Token: 0x02000BE9 RID: 3049
			[Serializable]
			public class Float : FxModuleVfx.Property
			{
				// Token: 0x06004A7F RID: 19071 RVA: 0x001A6813 File Offset: 0x001A4A13
				public override void Update(VisualEffect vfx, float value)
				{
					vfx.SetFloat(this.id, this.curve.Evaluate(value));
				}

				// Token: 0x04004D46 RID: 19782
				public AnimationCurve curve;
			}

			// Token: 0x02000BEA RID: 3050
			[Serializable]
			public class Color : FxModuleVfx.Property
			{
				// Token: 0x06004A81 RID: 19073 RVA: 0x001A6835 File Offset: 0x001A4A35
				public override void Update(VisualEffect vfx, float value)
				{
					vfx.SetVector4(this.id, this.gradient.Evaluate(value));
				}

				// Token: 0x04004D47 RID: 19783
				[GradientUsage(true)]
				public Gradient gradient;
			}
		}
	}
}
