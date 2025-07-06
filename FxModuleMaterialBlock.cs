using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200029C RID: 668
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/FxModuleMaterial")]
	public class FxModuleMaterialBlock : FxModule
	{
		// Token: 0x06001F57 RID: 8023 RVA: 0x000D55B0 File Offset: 0x000D37B0
		private void OnValidate()
		{
			if (!Application.isPlaying)
			{
				if (this.meshRenderers == null)
				{
					this.meshRenderers = new List<MeshRenderer>(base.GetComponents<MeshRenderer>());
				}
				if (this.propertyBlock == null)
				{
					this.propertyBlock = new MaterialPropertyBlock();
					foreach (FxModuleMaterialBlock.Property.Float @float in this.floatProperties)
					{
						@float.Init(this.propertyBlock);
					}
					foreach (FxModuleMaterialBlock.Property.Color color in this.colorProperties)
					{
						color.Init(this.propertyBlock);
					}
				}
			}
		}

		// Token: 0x06001F58 RID: 8024 RVA: 0x000D5684 File Offset: 0x000D3884
		private void Awake()
		{
			this.propertyBlock = new MaterialPropertyBlock();
			foreach (FxModuleMaterialBlock.Property.Float @float in this.floatProperties)
			{
				@float.Init(this.propertyBlock);
			}
			foreach (FxModuleMaterialBlock.Property.Color color in this.colorProperties)
			{
				color.Init(this.propertyBlock);
			}
		}

		// Token: 0x06001F59 RID: 8025 RVA: 0x000D572C File Offset: 0x000D392C
		public override void SetIntensity(float intensity)
		{
			foreach (FxModuleMaterialBlock.Property.Float propertyFloat in this.floatProperties)
			{
				if (propertyFloat.link == EffectLink.Intensity)
				{
					propertyFloat.UpdatePropertyBlock(intensity);
				}
			}
			foreach (FxModuleMaterialBlock.Property.Color propertyColor in this.colorProperties)
			{
				if (propertyColor.link == EffectLink.Intensity)
				{
					propertyColor.UpdatePropertyBlock(intensity);
				}
			}
			foreach (MeshRenderer meshRenderer in this.meshRenderers)
			{
				meshRenderer.SetPropertyBlock(this.propertyBlock);
			}
		}

		// Token: 0x06001F5A RID: 8026 RVA: 0x000D5818 File Offset: 0x000D3A18
		public override void SetSpeed(float speed)
		{
			foreach (FxModuleMaterialBlock.Property.Float propertyFloat in this.floatProperties)
			{
				if (propertyFloat.link == EffectLink.Speed)
				{
					propertyFloat.UpdatePropertyBlock(speed);
				}
			}
			foreach (FxModuleMaterialBlock.Property.Color propertyColor in this.colorProperties)
			{
				if (propertyColor.link == EffectLink.Speed)
				{
					propertyColor.UpdatePropertyBlock(speed);
				}
			}
			foreach (MeshRenderer meshRenderer in this.meshRenderers)
			{
				meshRenderer.SetPropertyBlock(this.propertyBlock);
			}
		}

		// Token: 0x06001F5B RID: 8027 RVA: 0x000D5908 File Offset: 0x000D3B08
		public override void Stop(bool playStopEffect = true)
		{
			this.SetIntensity(0f);
			this.SetSpeed(0f);
		}

		// Token: 0x04001E6B RID: 7787
		public List<MeshRenderer> meshRenderers;

		// Token: 0x04001E6C RID: 7788
		public List<FxModuleMaterialBlock.Property.Float> floatProperties = new List<FxModuleMaterialBlock.Property.Float>();

		// Token: 0x04001E6D RID: 7789
		public List<FxModuleMaterialBlock.Property.Color> colorProperties = new List<FxModuleMaterialBlock.Property.Color>();

		// Token: 0x04001E6E RID: 7790
		protected MaterialPropertyBlock propertyBlock;

		// Token: 0x0200093B RID: 2363
		[Serializable]
		public class Property
		{
			// Token: 0x060042E7 RID: 17127 RVA: 0x0018E3CB File Offset: 0x0018C5CB
			public void Init(MaterialPropertyBlock materialPropertyBlock)
			{
				this.id = Shader.PropertyToID(this.name);
				this.materialPropertyBlock = materialPropertyBlock;
			}

			// Token: 0x060042E8 RID: 17128 RVA: 0x0018E3E5 File Offset: 0x0018C5E5
			public virtual void UpdatePropertyBlock(float value)
			{
			}

			// Token: 0x0400441C RID: 17436
			public string name;

			// Token: 0x0400441D RID: 17437
			public EffectLink link;

			// Token: 0x0400441E RID: 17438
			protected int id;

			// Token: 0x0400441F RID: 17439
			protected MaterialPropertyBlock materialPropertyBlock;

			// Token: 0x02000BE7 RID: 3047
			[Serializable]
			public class Float : FxModuleMaterialBlock.Property
			{
				// Token: 0x06004A7B RID: 19067 RVA: 0x001A67C0 File Offset: 0x001A49C0
				public override void UpdatePropertyBlock(float value)
				{
					this.materialPropertyBlock.SetFloat(this.id, this.curve.Evaluate(value));
				}

				// Token: 0x04004D44 RID: 19780
				public AnimationCurve curve;
			}

			// Token: 0x02000BE8 RID: 3048
			[Serializable]
			public class Color : FxModuleMaterialBlock.Property
			{
				// Token: 0x06004A7D RID: 19069 RVA: 0x001A67E7 File Offset: 0x001A49E7
				public override void UpdatePropertyBlock(float value)
				{
					this.materialPropertyBlock.SetVector(this.id, this.gradient.Evaluate(value));
				}

				// Token: 0x04004D45 RID: 19781
				[GradientUsage(true, ColorSpace.Linear)]
				public Gradient gradient;
			}
		}
	}
}
