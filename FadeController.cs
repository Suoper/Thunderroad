using System;
using Shadowood;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x020002CC RID: 716
	public class FadeController : MonoBehaviour
	{
		// Token: 0x060022AF RID: 8879 RVA: 0x000EE273 File Offset: 0x000EC473
		private void OnValidate()
		{
			if (this.sphereMesh)
			{
				this.propertyBlock = new MaterialPropertyBlock();
			}
			this.Update();
		}

		// Token: 0x060022B0 RID: 8880 RVA: 0x000EE293 File Offset: 0x000EC493
		private void Awake()
		{
			if (this.sphereMesh)
			{
				this.propertyBlock = new MaterialPropertyBlock();
			}
		}

		// Token: 0x060022B1 RID: 8881 RVA: 0x000EE2B0 File Offset: 0x000EC4B0
		private void Update()
		{
			if (this.currentWeight != this.weight)
			{
				this.currentWeight = this.weight;
				if (Common.IsAndroid)
				{
					Tonemapping.SetExposureStatic(-10f * this.weight);
					if (this.sphereMesh)
					{
						this.propertyBlock.SetColor("_BaseColor", new Color(0f, 0f, 0f, this.weight));
						this.sphereMesh.SetPropertyBlock(this.propertyBlock);
						return;
					}
				}
				else
				{
					if (this.volume.enabled)
					{
						if (this.volume.weight == 0f)
						{
							this.volume.enabled = false;
						}
					}
					else if (this.volume.weight > 0f)
					{
						this.volume.enabled = true;
					}
					this.volume.weight = this.weight;
				}
			}
		}

		// Token: 0x060022B2 RID: 8882 RVA: 0x000EE39C File Offset: 0x000EC59C
		private void OnDisable()
		{
			if (Common.IsAndroid)
			{
				Tonemapping.SetExposureStatic(0f);
				if (this.sphereMesh)
				{
					this.propertyBlock.SetColor("_Color", new Color(0f, 0f, 0f, 0f));
					this.sphereMesh.SetPropertyBlock(this.propertyBlock);
					return;
				}
			}
			else
			{
				this.volume.weight = 0f;
				this.volume.enabled = false;
			}
		}

		// Token: 0x040021C3 RID: 8643
		public float weight;

		// Token: 0x040021C4 RID: 8644
		public Volume volume;

		// Token: 0x040021C5 RID: 8645
		public MeshRenderer sphereMesh;

		// Token: 0x040021C6 RID: 8646
		protected float currentWeight;

		// Token: 0x040021C7 RID: 8647
		protected MaterialPropertyBlock propertyBlock;
	}
}
