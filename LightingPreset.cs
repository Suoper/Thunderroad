using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000301 RID: 769
	[CreateAssetMenu(menuName = "ThunderRoad/Level/Lighting Data")]
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/LightingPreset.html")]
	public class LightingPreset : ScriptableObject
	{
		/// <summary>
		/// Call this method when releasing preset so it destroys/release instanciated object in it
		/// </summary>
		// Token: 0x060024CA RID: 9418 RVA: 0x000FC8D3 File Offset: 0x000FAAD3
		public void OnReleasePreset()
		{
			if (this.skyBoxMaterialProp != null)
			{
				this.skyBoxMaterialProp.ReleaseInstancedMaterial();
				return;
			}
			Debug.LogError("[LightingPreset][" + base.name + "] Cannot release skyboxMaterial. skyBoxMaterialProp is null");
		}

		// Token: 0x060024CB RID: 9419 RVA: 0x000FC904 File Offset: 0x000FAB04
		public void UpdateFrom(LightingPreset source, out bool needRebake)
		{
			needRebake = false;
			if (source.ambientIntensity != this.ambientIntensity)
			{
				this.ambientIntensity = source.ambientIntensity;
				needRebake = true;
			}
			this.shadowColor = source.shadowColor;
			if (source.applyAtRuntime != this.applyAtRuntime)
			{
				this.applyAtRuntime = source.applyAtRuntime;
				needRebake = true;
			}
			if (source.dirLightColor != this.dirLightColor)
			{
				this.dirLightColor = source.dirLightColor;
				needRebake = true;
			}
			if (source.dirLightIntensity != this.dirLightIntensity)
			{
				this.dirLightIntensity = source.dirLightIntensity;
				needRebake = true;
			}
			if (source.dirLightIndirectMultiplier != this.dirLightIndirectMultiplier)
			{
				this.dirLightIndirectMultiplier = source.dirLightIndirectMultiplier;
				needRebake = true;
			}
			if (source.directionalLightLocalRotation != this.directionalLightLocalRotation)
			{
				this.directionalLightLocalRotation = source.directionalLightLocalRotation;
				needRebake = true;
			}
			this.fog = source.fog;
			this.fogColor = source.fogColor;
			this.fogStartDistance = source.fogStartDistance;
			this.fogEndDistance = source.fogEndDistance;
			if (source.skybox != this.skybox)
			{
				this.skybox = source.skybox;
				needRebake = true;
			}
			if (this.skyBoxMaterialProp.CopyFrom(source.skyBoxMaterialProp))
			{
				needRebake = true;
			}
			this.clouds = source.clouds;
			this.cloudsSoftness = source.cloudsSoftness;
			this.cloudsSpeed = source.cloudsSpeed;
			this.cloudsSize = source.cloudsSize;
			this.cloudsAlpha = source.cloudsAlpha;
			this.cloudsColor = source.cloudsColor;
		}

		// Token: 0x060024CC RID: 9420 RVA: 0x000FCA83 File Offset: 0x000FAC83
		private void OnValidate()
		{
			this.ValidateFogParameters();
		}

		// Token: 0x060024CD RID: 9421 RVA: 0x000FCA8C File Offset: 0x000FAC8C
		public void ValidateFogParameters()
		{
			if (this.fogStartDistance < 0f)
			{
				this.fogStartDistance = 0f;
			}
			if (this.fogEndDistance < 0f)
			{
				this.fogEndDistance = 0f;
			}
			if (this.fogStartDistance > this.fogEndDistance)
			{
				this.fogEndDistance = this.fogStartDistance + 0.01f;
			}
		}

		// Token: 0x060024CE RID: 9422 RVA: 0x000FCAE9 File Offset: 0x000FACE9
		private string GetPresetName()
		{
			return base.name.Replace("_LightingPreset", "").Replace("_Android", "");
		}

		// Token: 0x060024CF RID: 9423 RVA: 0x000FCB10 File Offset: 0x000FAD10
		public bool TryGetSerializedLightmapData(Texture2D color, out LightingPreset.SerializedLightmapData serializedLightmapData)
		{
			for (int i = 0; i < this.serializedLightmaps.Count; i++)
			{
				if (this.serializedLightmaps[i].color == color)
				{
					serializedLightmapData = this.serializedLightmaps[i];
					return true;
				}
			}
			serializedLightmapData = null;
			return false;
		}

		// Token: 0x060024D0 RID: 9424 RVA: 0x000FCB60 File Offset: 0x000FAD60
		public bool TryGetSerializedLightmapData(Texture2D color, out LightingPreset.SerializedLightmapData serializedLightmapData, out int index)
		{
			for (int i = 0; i < this.serializedLightmaps.Count; i++)
			{
				if (this.serializedLightmaps[i].color == color)
				{
					serializedLightmapData = this.serializedLightmaps[i];
					index = i;
					return true;
				}
			}
			serializedLightmapData = null;
			index = -1;
			return false;
		}

		// Token: 0x060024D1 RID: 9425 RVA: 0x000FCBB8 File Offset: 0x000FADB8
		public bool TryGetReflectionProbeData(Texture texture, out LightingPreset.ReflectionProbeData reflectionProbeData)
		{
			for (int i = 0; i < this.reflectionProbes.Count; i++)
			{
				if (this.reflectionProbes[i].texture == texture)
				{
					reflectionProbeData = this.reflectionProbes[i];
					return true;
				}
			}
			reflectionProbeData = null;
			return false;
		}

		// Token: 0x060024D2 RID: 9426 RVA: 0x000FCC08 File Offset: 0x000FAE08
		protected bool TryGetLightingGroupInScene(out LightingGroup lightingGroup)
		{
			lightingGroup = UnityEngine.Object.FindObjectOfType<LightingGroup>();
			return lightingGroup;
		}

		// Token: 0x04002446 RID: 9286
		[Header("Runtime")]
		[Header("General")]
		public float ambientIntensity = 1f;

		// Token: 0x04002447 RID: 9287
		public Color shadowColor = new Color(0.7f, 0.7f, 0.7f);

		// Token: 0x04002448 RID: 9288
		[Header("Directional light")]
		public bool applyAtRuntime;

		// Token: 0x04002449 RID: 9289
		public Color dirLightColor = new Color(1f, 0.8687521f, 0.7122642f);

		// Token: 0x0400244A RID: 9290
		public float dirLightIntensity = 2f;

		// Token: 0x0400244B RID: 9291
		public float dirLightIndirectMultiplier = 1f;

		// Token: 0x0400244C RID: 9292
		public Quaternion directionalLightLocalRotation;

		// Token: 0x0400244D RID: 9293
		[Header("Fog")]
		public LightingPreset.State fog;

		// Token: 0x0400244E RID: 9294
		public Color fogColor;

		// Token: 0x0400244F RID: 9295
		public float fogStartDistance;

		// Token: 0x04002450 RID: 9296
		public float fogEndDistance;

		// Token: 0x04002451 RID: 9297
		[Header("Skybox")]
		public LightingPreset.State skybox;

		// Token: 0x04002452 RID: 9298
		public SerializedMaterialProperties skyBoxMaterialProp = new SerializedMaterialProperties();

		// Token: 0x04002453 RID: 9299
		[Header("Clouds")]
		public LightingPreset.State clouds;

		// Token: 0x04002454 RID: 9300
		public float cloudsSoftness = 1f;

		// Token: 0x04002455 RID: 9301
		public float cloudsSpeed = 1f;

		// Token: 0x04002456 RID: 9302
		public float cloudsSize = 1f;

		// Token: 0x04002457 RID: 9303
		public float cloudsAlpha = 1f;

		// Token: 0x04002458 RID: 9304
		public Color cloudsColor;

		// Token: 0x04002459 RID: 9305
		[Header("Lightmaps")]
		public List<LightingPreset.SerializedLightmapData> serializedLightmaps = new List<LightingPreset.SerializedLightmapData>();

		// Token: 0x0400245A RID: 9306
		public List<LightingPreset.MeshRendererData> rendererDataListForLightmaps = new List<LightingPreset.MeshRendererData>();

		// Token: 0x0400245B RID: 9307
		public List<int> indexLightmapsRendererMeshCount = new List<int>();

		// Token: 0x0400245C RID: 9308
		public List<LightingPreset.LightmapLightData> lightmapLights = new List<LightingPreset.LightmapLightData>();

		// Token: 0x0400245D RID: 9309
		public List<LightingPreset.LightProbeVolumeData> lightProbeVolumes = new List<LightingPreset.LightProbeVolumeData>();

		// Token: 0x0400245E RID: 9310
		public List<LightingPreset.ReflectionProbeData> reflectionProbes = new List<LightingPreset.ReflectionProbeData>();

		// Token: 0x0400245F RID: 9311
		public List<SphericalHarmonicsL2> bakedProbes;

		// Token: 0x04002460 RID: 9312
		public List<Vector3> probePositions;

		// Token: 0x04002461 RID: 9313
		public UDateTime lastSave = DateTime.Now;

		// Token: 0x020009F9 RID: 2553
		public enum State
		{
			// Token: 0x04004691 RID: 18065
			NoChange,
			// Token: 0x04004692 RID: 18066
			Disabled,
			// Token: 0x04004693 RID: 18067
			Enabled
		}

		// Token: 0x020009FA RID: 2554
		[Serializable]
		public class SerializedLightmapData
		{
			// Token: 0x06004507 RID: 17671 RVA: 0x00194FFE File Offset: 0x001931FE
			public SerializedLightmapData(Texture2D color, Texture2D directional, Texture2D shadowMask)
			{
				this.color = color;
				this.directional = directional;
				this.shadowMask = shadowMask;
			}

			// Token: 0x06004508 RID: 17672 RVA: 0x0019501B File Offset: 0x0019321B
			public SerializedLightmapData(LightmapData lightmapData)
			{
				this.color = lightmapData.lightmapColor;
				this.directional = lightmapData.lightmapDir;
				this.shadowMask = lightmapData.shadowMask;
			}

			// Token: 0x06004509 RID: 17673 RVA: 0x00195047 File Offset: 0x00193247
			public LightmapData ToLightmapData()
			{
				return new LightmapData
				{
					lightmapColor = this.color,
					lightmapDir = this.directional,
					shadowMask = this.shadowMask
				};
			}

			// Token: 0x0600450A RID: 17674 RVA: 0x00195072 File Offset: 0x00193272
			public LightingPreset.SerializedLightmapData Clone()
			{
				return base.MemberwiseClone() as LightingPreset.SerializedLightmapData;
			}

			// Token: 0x04004694 RID: 18068
			public Texture2D color;

			// Token: 0x04004695 RID: 18069
			public Texture2D directional;

			// Token: 0x04004696 RID: 18070
			public Texture2D shadowMask;
		}

		// Token: 0x020009FB RID: 2555
		[Serializable]
		public class LightmapMeshRendererData
		{
			// Token: 0x0600450B RID: 17675 RVA: 0x0019507F File Offset: 0x0019327F
			public LightmapMeshRendererData(LightingGroup.MeshRendererReference meshRendererReference, LightingPreset.SerializedLightmapData serializedLightmapData)
			{
				this.meshRendererGuid = meshRendererReference.guid;
				this.offsetScale = meshRendererReference.meshRenderer.lightmapScaleOffset;
				this.colorReference = serializedLightmapData.color;
			}

			// Token: 0x04004697 RID: 18071
			public string meshRendererGuid;

			// Token: 0x04004698 RID: 18072
			public Vector4 offsetScale;

			// Token: 0x04004699 RID: 18073
			public bool generateSecondaryUV;

			// Token: 0x0400469A RID: 18074
			public Texture2D colorReference;
		}

		// Token: 0x020009FC RID: 2556
		[Serializable]
		public class MeshRendererData
		{
			// Token: 0x0600450C RID: 17676 RVA: 0x001950B0 File Offset: 0x001932B0
			public MeshRendererData(LightingGroup.MeshRendererReference meshRendererReference)
			{
				this.meshRendererGuid = meshRendererReference.guid;
				this.offsetScale = meshRendererReference.meshRenderer.lightmapScaleOffset;
			}

			// Token: 0x0600450D RID: 17677 RVA: 0x001950D5 File Offset: 0x001932D5
			public MeshRendererData(LightingPreset.LightmapMeshRendererData data)
			{
				this.meshRendererGuid = data.meshRendererGuid;
				this.offsetScale = data.offsetScale;
				this.generateSecondaryUV = data.generateSecondaryUV;
			}

			// Token: 0x0400469B RID: 18075
			public string meshRendererGuid;

			// Token: 0x0400469C RID: 18076
			public Vector4 offsetScale;

			// Token: 0x0400469D RID: 18077
			public bool generateSecondaryUV;
		}

		// Token: 0x020009FD RID: 2557
		[Serializable]
		public class LightmapLightData
		{
			// Token: 0x0600450E RID: 17678 RVA: 0x00195104 File Offset: 0x00193304
			public LightmapLightData(LightingGroup.LightReference lightReference, int mixedLightingMode)
			{
				this.lightGuid = lightReference.guid;
				this.probeOcclusionLightIndex = lightReference.light.bakingOutput.probeOcclusionLightIndex;
				this.occlusionMaskChannel = lightReference.light.bakingOutput.occlusionMaskChannel;
				this.mixedLightingMode = mixedLightingMode;
			}

			// Token: 0x0400469E RID: 18078
			public string lightGuid;

			// Token: 0x0400469F RID: 18079
			public int baketype;

			// Token: 0x040046A0 RID: 18080
			public int mixedLightingMode;

			// Token: 0x040046A1 RID: 18081
			public int probeOcclusionLightIndex;

			// Token: 0x040046A2 RID: 18082
			public int occlusionMaskChannel;
		}

		// Token: 0x020009FE RID: 2558
		[Serializable]
		public class LightProbeVolumeData
		{
			// Token: 0x0600450F RID: 17679 RVA: 0x00195158 File Offset: 0x00193358
			public LightProbeVolumeData(LightingGroup.LightProbeVolumeReference lightProbeVolumeReference)
			{
				this.lightProbeVolumeGuid = lightProbeVolumeReference.guid;
				this.SHAr = lightProbeVolumeReference.lightProbeVolume.SHAr;
				this.SHAg = lightProbeVolumeReference.lightProbeVolume.SHAg;
				this.SHAb = lightProbeVolumeReference.lightProbeVolume.SHAb;
				this.occ = lightProbeVolumeReference.lightProbeVolume.occ;
			}

			// Token: 0x040046A3 RID: 18083
			public string lightProbeVolumeGuid;

			// Token: 0x040046A4 RID: 18084
			public Texture3D SHAr;

			// Token: 0x040046A5 RID: 18085
			public Texture3D SHAg;

			// Token: 0x040046A6 RID: 18086
			public Texture3D SHAb;

			// Token: 0x040046A7 RID: 18087
			public Texture3D occ;
		}

		// Token: 0x020009FF RID: 2559
		[Serializable]
		public class ReflectionProbeData
		{
			// Token: 0x06004510 RID: 17680 RVA: 0x001951BB File Offset: 0x001933BB
			public ReflectionProbeData(LightingGroup.ReflectionProbeReference reflectionProbeReference)
			{
				this.reflectionProbeGuid = reflectionProbeReference.guid;
				this.texture = reflectionProbeReference.reflectionProbe.bakedTexture;
			}

			// Token: 0x040046A8 RID: 18088
			public string reflectionProbeGuid;

			// Token: 0x040046A9 RID: 18089
			public Texture texture;
		}
	}
}
