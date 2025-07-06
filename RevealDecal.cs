using System;
using System.Collections.Generic;
using ThunderRoad.Reveal;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000356 RID: 854
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/RevealDecal.html")]
	[AddComponentMenu("ThunderRoad/Reveal Decal")]
	public class RevealDecal : MonoBehaviour
	{
		// Token: 0x060027F8 RID: 10232 RVA: 0x00111E75 File Offset: 0x00110075
		public void SetMaskResolutionFull()
		{
			this.SetMaskResolution(1f);
		}

		// Token: 0x060027F9 RID: 10233 RVA: 0x00111E82 File Offset: 0x00110082
		public void SetMaskResolutionHalf()
		{
			this.SetMaskResolution(0.5f);
		}

		// Token: 0x060027FA RID: 10234 RVA: 0x00111E8F File Offset: 0x0011008F
		public void SetMaskResolutionQuarter()
		{
			this.SetMaskResolution(0.25f);
		}

		// Token: 0x060027FB RID: 10235 RVA: 0x00111E9C File Offset: 0x0011009C
		public void SetMaskResolutionEighth()
		{
			this.SetMaskResolution(0.125f);
		}

		// Token: 0x060027FC RID: 10236 RVA: 0x00111EAC File Offset: 0x001100AC
		public void SetMaskResolution(float scale = 1f)
		{
			Renderer renderer = base.GetComponent<Renderer>();
			if (renderer != null)
			{
				Material mat = renderer.sharedMaterial;
				if (mat != null)
				{
					Texture baseMap = mat.GetTexture("_BaseMap");
					if (baseMap != null)
					{
						this.maskWidth = (RevealDecal.RevealMaskResolution)Mathf.ClosestPowerOfTwo((int)((float)baseMap.width * scale));
						this.maskHeight = (RevealDecal.RevealMaskResolution)Mathf.ClosestPowerOfTwo((int)((float)baseMap.height * scale));
						if (this.maskWidth != this.maskHeight)
						{
							Debug.Log(base.gameObject.name);
						}
					}
				}
			}
		}

		// Token: 0x060027FD RID: 10237 RVA: 0x00111F38 File Offset: 0x00110138
		private void Awake()
		{
			if (!Level.master || !Catalog.gameData.platformParameters.enableEffectReveal)
			{
				base.enabled = false;
				return;
			}
			if (!base.gameObject.GetComponent<MaterialInstance>())
			{
				base.gameObject.AddComponent<MaterialInstance>();
			}
			this.revealMaterialController = base.gameObject.AddComponent<RevealMaterialController>();
			this.revealMaterialController.width = (int)this.maskWidth;
			this.revealMaterialController.height = (int)this.maskHeight;
			this.revealMaterialController.maskPropertyName = "_RevealMask";
			this.revealMaterialController.restoreMaterialsOnReset = false;
			this.revealMaterialController.renderTextureFormat = RenderTextureFormat.ARGB64;
		}

		// Token: 0x060027FE RID: 10238 RVA: 0x00111FE4 File Offset: 0x001101E4
		public void ActivateReveal()
		{
			if (this.revealMaterialController)
			{
				this.revealMaterialController.ActivateRevealMaterials();
			}
		}

		// Token: 0x060027FF RID: 10239 RVA: 0x00111FFF File Offset: 0x001101FF
		public void Reset()
		{
			if (this.revealMaterialController)
			{
				this.revealMaterialController.Reset();
			}
		}

		// Token: 0x06002800 RID: 10240 RVA: 0x00112019 File Offset: 0x00110219
		public void Blit(float multiplier = 0.1f)
		{
			RevealMaskProjection.BlitColor(new List<RevealMaterialController>
			{
				this.revealMaterialController
			}, new Vector4(multiplier, multiplier, multiplier, multiplier), BlendOp.ReverseSubtract, ColorWriteMask.All);
		}

		// Token: 0x06002801 RID: 10241 RVA: 0x0011203D File Offset: 0x0011023D
		public void Blit(Vector4 color, BlendOp blendMode)
		{
			RevealMaskProjection.BlitColor(new List<RevealMaterialController>
			{
				this.revealMaterialController
			}, color, blendMode, ColorWriteMask.All);
		}

		// Token: 0x06002802 RID: 10242 RVA: 0x00112059 File Offset: 0x00110259
		public void BlitTexture(Color color, Texture2D texture, BlendOp blendMode)
		{
			RevealMaskProjection.BlitTexture(new List<RevealMaterialController>
			{
				this.revealMaterialController
			}, color, texture, blendMode, ColorWriteMask.All);
		}

		// Token: 0x06002803 RID: 10243 RVA: 0x0011207B File Offset: 0x0011027B
		public bool UpdateOvertime()
		{
			return this.revealMaterialController.UpdateBlitsOverTime();
		}

		// Token: 0x040026E5 RID: 9957
		[Tooltip("Resolution of the width of the reveal mask")]
		public RevealDecal.RevealMaskResolution maskWidth = RevealDecal.RevealMaskResolution.Size_512;

		// Token: 0x040026E6 RID: 9958
		[Tooltip("Resolution of the height of the reveal mask")]
		public RevealDecal.RevealMaskResolution maskHeight = RevealDecal.RevealMaskResolution.Size_512;

		// Token: 0x040026E7 RID: 9959
		[Tooltip("Specifies what type of reveal is used.\nDefault is for Items and Weapons.\nOutfit is for clothing/armor.\nBody is for NPC/Player.\n\nThe Body type will make it so blood/reveal is removed/fades when the Player/NPC drinks a potion, but only on the body, not on Outfit.")]
		public RevealDecal.Type type;

		// Token: 0x040026E8 RID: 9960
		[NonSerialized]
		public RevealMaterialController revealMaterialController;

		// Token: 0x02000A42 RID: 2626
		public enum Type
		{
			// Token: 0x0400478C RID: 18316
			Default,
			// Token: 0x0400478D RID: 18317
			Body,
			// Token: 0x0400478E RID: 18318
			Outfit
		}

		// Token: 0x02000A43 RID: 2627
		public enum RevealMaskResolution
		{
			// Token: 0x04004790 RID: 18320
			Size_32 = 32,
			// Token: 0x04004791 RID: 18321
			Size_64 = 64,
			// Token: 0x04004792 RID: 18322
			Size_128 = 128,
			// Token: 0x04004793 RID: 18323
			Size_256 = 256,
			// Token: 0x04004794 RID: 18324
			Size_512 = 512,
			// Token: 0x04004795 RID: 18325
			Size_1024 = 1024,
			// Token: 0x04004796 RID: 18326
			Size_2048 = 2048,
			// Token: 0x04004797 RID: 18327
			Size_4096 = 4096
		}
	}
}
