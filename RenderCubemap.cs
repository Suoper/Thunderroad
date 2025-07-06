using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThunderRoad
{
	// Token: 0x020002ED RID: 749
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Misc/RenderCubemap.html")]
	[AddComponentMenu("ThunderRoad/Levels/Preview Cubemap")]
	public class RenderCubemap : MonoBehaviour
	{
		// Token: 0x060023F1 RID: 9201 RVA: 0x000F5DAC File Offset: 0x000F3FAC
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.assetBundleName == null || this.assetBundleName == "")
			{
				this.assetBundleName = base.gameObject.scene.name + "Preview";
			}
		}

		// Token: 0x060023F2 RID: 9202 RVA: 0x000F5E04 File Offset: 0x000F4004
		public void CreateCubemap(bool skillCubemap = false)
		{
			Cubemap cubemap = new Cubemap(this.size, this.defaultFormat, this.textureCreationFlags);
			Camera camera = base.gameObject.AddComponent<Camera>();
			UniversalAdditionalCameraData cameraData = camera.GetOrAddComponent<UniversalAdditionalCameraData>();
			if (skillCubemap)
			{
				cameraData.SetRenderer(1);
			}
			camera.RenderToCubemap(cubemap);
			if (cameraData)
			{
				UnityEngine.Object.DestroyImmediate(cameraData);
			}
			UnityEngine.Object.DestroyImmediate(camera);
			Debug.Log("Cubemap created!");
		}

		// Token: 0x0400231C RID: 8988
		public string assetBundleName;

		// Token: 0x0400231D RID: 8989
		public int size = 128;

		// Token: 0x0400231E RID: 8990
		public DefaultFormat defaultFormat;

		// Token: 0x0400231F RID: 8991
		public TextureCreationFlags textureCreationFlags;
	}
}
