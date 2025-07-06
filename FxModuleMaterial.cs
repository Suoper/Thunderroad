using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200029B RID: 667
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModuleMaterial.html")]
	public class FxModuleMaterial : FxModule
	{
		// Token: 0x06001F52 RID: 8018 RVA: 0x000D53C0 File Offset: 0x000D35C0
		private void Awake()
		{
			this.meshRenderer = base.GetComponent<MeshRenderer>();
			if (!this.meshRenderer)
			{
				this.meshRenderer = base.GetComponent<SkinnedMeshRenderer>();
			}
			List<FxModuleMaterial.Property> list = new List<FxModuleMaterial.Property>();
			list.AddRange(this.floatProperties);
			list.AddRange(this.colorProperties);
			foreach (FxModuleMaterial.Property property in list)
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

		// Token: 0x06001F53 RID: 8019 RVA: 0x000D5484 File Offset: 0x000D3684
		public override void SetIntensity(float intensity)
		{
			foreach (FxModuleMaterial.Property property in this.intensityProperties)
			{
				foreach (Material material in this.meshRenderer.MaterialInstances())
				{
					property.Update(material, intensity);
				}
			}
		}

		// Token: 0x06001F54 RID: 8020 RVA: 0x000D54F8 File Offset: 0x000D36F8
		public override void SetSpeed(float speed)
		{
			foreach (FxModuleMaterial.Property property in this.speedProperties)
			{
				foreach (Material material in this.meshRenderer.MaterialInstances())
				{
					property.Update(material, speed);
				}
			}
		}

		// Token: 0x06001F55 RID: 8021 RVA: 0x000D556C File Offset: 0x000D376C
		public override void Stop(bool playStopEffect = true)
		{
			this.SetIntensity(0f);
		}

		// Token: 0x04001E66 RID: 7782
		public List<FxModuleMaterial.Property.Float> floatProperties = new List<FxModuleMaterial.Property.Float>();

		// Token: 0x04001E67 RID: 7783
		public List<FxModuleMaterial.Property.Color> colorProperties = new List<FxModuleMaterial.Property.Color>();

		// Token: 0x04001E68 RID: 7784
		protected List<FxModuleMaterial.Property> intensityProperties = new List<FxModuleMaterial.Property>();

		// Token: 0x04001E69 RID: 7785
		protected List<FxModuleMaterial.Property> speedProperties = new List<FxModuleMaterial.Property>();

		// Token: 0x04001E6A RID: 7786
		protected Renderer meshRenderer;

		// Token: 0x0200093A RID: 2362
		public class Property
		{
			// Token: 0x060042E5 RID: 17125 RVA: 0x0018E3C1 File Offset: 0x0018C5C1
			public virtual void Update(Material material, float value)
			{
			}

			// Token: 0x04004419 RID: 17433
			public string name;

			// Token: 0x0400441A RID: 17434
			public EffectLink link;

			// Token: 0x0400441B RID: 17435
			[NonSerialized]
			public int id;

			// Token: 0x02000BE5 RID: 3045
			[Serializable]
			public class Float : FxModuleMaterial.Property
			{
				// Token: 0x06004A77 RID: 19063 RVA: 0x001A677C File Offset: 0x001A497C
				public override void Update(Material material, float value)
				{
					material.SetFloat(this.id, this.curve.Evaluate(value));
				}

				// Token: 0x04004D42 RID: 19778
				public AnimationCurve curve;
			}

			// Token: 0x02000BE6 RID: 3046
			[Serializable]
			public class Color : FxModuleMaterial.Property
			{
				// Token: 0x06004A79 RID: 19065 RVA: 0x001A679E File Offset: 0x001A499E
				public override void Update(Material material, float value)
				{
					material.SetColor(this.id, this.gradient.Evaluate(value));
				}

				// Token: 0x04004D43 RID: 19779
				[GradientUsage(true)]
				public Gradient gradient;
			}
		}
	}
}
