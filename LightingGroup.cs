using System;
using System.Collections.Generic;
using Shadowood;
using ThunderRoad.Pools;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000300 RID: 768
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Areas/LightingGroup.html")]
	public class LightingGroup : MonoBehaviour, ICheckAsset
	{
		// Token: 0x1400011E RID: 286
		// (add) Token: 0x060024A4 RID: 9380 RVA: 0x000FB51A File Offset: 0x000F971A
		// (remove) Token: 0x060024A3 RID: 9379 RVA: 0x000FB503 File Offset: 0x000F9703
		public static event Action UpdateRenderSettingEvent
		{
			add
			{
				LightingGroup._updateRenderSettingEvent = (Action)Delegate.Remove(LightingGroup._updateRenderSettingEvent, value);
				LightingGroup._updateRenderSettingEvent = (Action)Delegate.Combine(LightingGroup._updateRenderSettingEvent, value);
			}
			remove
			{
				LightingGroup._updateRenderSettingEvent = (Action)Delegate.Remove(LightingGroup._updateRenderSettingEvent, value);
			}
		}

		// Token: 0x060024A5 RID: 9381 RVA: 0x000FB546 File Offset: 0x000F9746
		public static void FreeLightmapIndexHelper()
		{
			LightingGroup.lightmapIndexHelper = null;
		}

		// Token: 0x060024A6 RID: 9382 RVA: 0x000FB550 File Offset: 0x000F9750
		private void OnValidate()
		{
			if (!base.enabled)
			{
				return;
			}
			if (Application.isPlaying)
			{
				return;
			}
			if (this.InPrefabScene())
			{
				return;
			}
			if (this.currentLightingPreset != this.lightingPreset)
			{
				if (this.currentLightingPreset)
				{
					this.ClearAll(this.currentLightingPreset);
				}
				if (this.lightingPreset)
				{
					this.bakedLODGroups = new List<BakedLODGroup>(base.GetComponentsInChildren<BakedLODGroup>());
				}
			}
		}

		// Token: 0x060024A7 RID: 9383 RVA: 0x000FB5C4 File Offset: 0x000F97C4
		private void Awake()
		{
			this.GetBakedLODGroups();
			if (Application.isPlaying && this.lightingPreset)
			{
				foreach (LightingPreset.MeshRendererData meshRendererData in this.lightingPreset.rendererDataListForLightmaps)
				{
					MeshRenderer meshRenderer;
					if (this.TryGetMeshRenderer(meshRendererData.meshRendererGuid, out meshRenderer) && !(meshRenderer == null) && !meshRenderer.isPartOfStaticBatch)
					{
						meshRenderer.lightmapScaleOffset = meshRendererData.offsetScale;
					}
				}
			}
		}

		// Token: 0x060024A8 RID: 9384 RVA: 0x000FB65C File Offset: 0x000F985C
		public void Start()
		{
			if (this.lightingPreset != null)
			{
				this.ApplyAll();
			}
			this.isStarted = true;
		}

		// Token: 0x060024A9 RID: 9385 RVA: 0x000FB679 File Offset: 0x000F9879
		private void OnEnable()
		{
			if (!this.isStarted)
			{
				return;
			}
			if (this.lightingPreset != null)
			{
				this.ApplyAll();
			}
		}

		// Token: 0x060024AA RID: 9386 RVA: 0x000FB698 File Offset: 0x000F9898
		private void OnDisable()
		{
			LightingGroup.allActive.Remove(this);
			if (GameManager.isQuitting)
			{
				return;
			}
			this.ClearAll();
		}

		// Token: 0x060024AB RID: 9387 RVA: 0x000FB6B4 File Offset: 0x000F98B4
		private void OnDestroy()
		{
			if (Application.isPlaying)
			{
				GlobalSkyCube.SetDefaults();
				this.ClearAll();
			}
		}

		// Token: 0x060024AC RID: 9388 RVA: 0x000FB6C8 File Offset: 0x000F98C8
		private void GetBakedLODGroups()
		{
			if (this.bakedLODGroups == null)
			{
				this.bakedLODGroups = new List<BakedLODGroup>();
			}
			if (this.bakedLODGroups.Count == 0)
			{
				base.GetComponentsInChildren<BakedLODGroup>(this.bakedLODGroups);
			}
		}

		// Token: 0x060024AD RID: 9389 RVA: 0x000FB6F8 File Offset: 0x000F98F8
		[ContextMenu("ApplyAll")]
		public void ApplyAll()
		{
			if (this.initialized)
			{
				return;
			}
			try
			{
				if (LightingGroup.allActive != null)
				{
					LightingGroup.allActive.Add(this);
				}
				this.ApplyLightmaps(false);
				this.ApplyLightProbeVolumes(false);
				this.ApplySceneSettings(false);
				if (this.lightingPreset)
				{
					Debug.LogFormat(this, "Apply lightmaps, lightProbe and Scene settings from " + this.lightingPreset.name + " on lightingGroup " + base.name, Array.Empty<object>());
				}
				this.initialized = true;
			}
			catch (Exception message)
			{
				Debug.LogError(message);
			}
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x000FB790 File Offset: 0x000F9990
		public void ApplyPresetWithoutSceneSettings(LightingPreset preset)
		{
			if (this.lightingPreset != null)
			{
				return;
			}
			this.lightingPreset = preset;
			this.ApplyLightmaps(true);
			this.ApplyLightProbeVolumes(true);
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x000FB7B8 File Offset: 0x000F99B8
		public void ApplyLightmaps(bool showDebugLine = true)
		{
			if (this.lightingPreset == null)
			{
				Debug.LogWarningFormat(this, "Cannot apply lighting data as LightingPreset field is null", Array.Empty<object>());
				return;
			}
			if (showDebugLine)
			{
				Debug.LogFormat(this, "Apply lightmaps from " + this.lightingPreset.name + " on lightingGroup " + base.name, Array.Empty<object>());
			}
			if (LightingGroup.lightmapIndexHelper == null)
			{
				LightingGroup.lightmapIndexHelper = new LightingGroup.LightmapIndexHelper();
			}
			int meshCount = this.lightingPreset.rendererDataListForLightmaps.Count;
			int indexLightmap = -1;
			int indexNextLightmapMesh = -1;
			while (indexNextLightmapMesh == -1 && indexLightmap < this.lightingPreset.indexLightmapsRendererMeshCount.Count - 1)
			{
				indexLightmap++;
				indexNextLightmapMesh = this.lightingPreset.indexLightmapsRendererMeshCount[indexLightmap] - 1;
			}
			if (indexNextLightmapMesh >= 0)
			{
				LightingGroup.lightmapIndexHelper.InitFromLightmap();
				List<MeshRenderer> meshList = LazyListPool<MeshRenderer>.Instance.Get(0);
				for (int indexMesh = 0; indexMesh < meshCount; indexMesh++)
				{
					MeshRenderer meshRenderer = null;
					if (this.TryGetMeshRenderer(this.lightingPreset.rendererDataListForLightmaps[indexMesh].meshRendererGuid, out meshRenderer))
					{
						meshList.Add(meshRenderer);
						if (!meshRenderer.isPartOfStaticBatch)
						{
							meshRenderer.lightmapScaleOffset = this.lightingPreset.rendererDataListForLightmaps[indexMesh].offsetScale;
						}
					}
					if (indexMesh == indexNextLightmapMesh)
					{
						if (meshList.Count > 0 && this.lightingPreset.serializedLightmaps.Count > 0)
						{
							LightingGroup.lightmapIndexHelper.SetMeshLightmapIndex(meshList, this.lightingPreset.serializedLightmaps[indexLightmap].ToLightmapData());
						}
						meshList.Clear();
						while (indexNextLightmapMesh == indexMesh && indexLightmap < this.lightingPreset.indexLightmapsRendererMeshCount.Count - 1)
						{
							indexLightmap++;
							indexNextLightmapMesh = this.lightingPreset.indexLightmapsRendererMeshCount[indexLightmap] - 1;
						}
					}
				}
				LazyListPool<MeshRenderer>.Instance.Return(meshList);
				LightingGroup.lightmapIndexHelper.SetLightmap();
			}
			this.GetBakedLODGroups();
			int count = this.bakedLODGroups.Count;
			for (int i = 0; i < count; i++)
			{
				this.bakedLODGroups[i].ApplyLightmaps();
			}
		}

		// Token: 0x060024B0 RID: 9392 RVA: 0x000FB9BC File Offset: 0x000F9BBC
		public void ApplyLightProbeVolumes(bool showDebugLine = true)
		{
			if (this.lightingPreset == null)
			{
				Debug.LogWarningFormat(this, "Cannot apply lighting data as LightingPreset field is null", Array.Empty<object>());
				return;
			}
			if (showDebugLine)
			{
				Debug.LogFormat(this, "Apply lightProbeVolumes from " + this.lightingPreset.name + " on lightingGroup " + base.name, Array.Empty<object>());
			}
			this.currentLightingPreset = this.lightingPreset;
			foreach (LightingPreset.LightmapLightData lightmapLightData in this.lightingPreset.lightmapLights)
			{
				Light light;
				if (this.TryGetLight(lightmapLightData.lightGuid, out light) && !(light == null))
				{
					LightBakingOutput bakingOutput = new LightBakingOutput
					{
						isBaked = true,
						lightmapBakeType = (LightmapBakeType)lightmapLightData.baketype,
						mixedLightingMode = (MixedLightingMode)lightmapLightData.mixedLightingMode,
						probeOcclusionLightIndex = lightmapLightData.probeOcclusionLightIndex,
						occlusionMaskChannel = lightmapLightData.occlusionMaskChannel
					};
					light.bakingOutput = bakingOutput;
				}
			}
			foreach (LightingPreset.ReflectionProbeData reflectionProbeData in this.lightingPreset.reflectionProbes)
			{
				ReflectionProbe reflectionProbe;
				if (this.TryGetReflectionProbe(reflectionProbeData.reflectionProbeGuid, out reflectionProbe) && !(reflectionProbe == null))
				{
					reflectionProbe.bakedTexture = reflectionProbeData.texture;
				}
			}
			foreach (LightingPreset.LightProbeVolumeData lightProbeVolumeData in this.lightingPreset.lightProbeVolumes)
			{
				LightProbeVolume lightProbeVolume;
				if (this.TryGetLightProbeVolume(lightProbeVolumeData.lightProbeVolumeGuid, out lightProbeVolume) && lightProbeVolume != null)
				{
					lightProbeVolume.SetTexture(lightProbeVolumeData.SHAr, lightProbeVolumeData.SHAg, lightProbeVolumeData.SHAb, lightProbeVolumeData.occ);
				}
			}
		}

		/// <summary>
		/// Apply only scene settings
		/// </summary>
		// Token: 0x060024B1 RID: 9393 RVA: 0x000FBBB8 File Offset: 0x000F9DB8
		public void ApplySceneSettings(bool showDebugLine = true)
		{
			this.ApplySceneSettings(this.lightingPreset, showDebugLine);
		}

		/// <summary>
		/// Apply only sky scene settings
		/// </summary>
		// Token: 0x060024B2 RID: 9394 RVA: 0x000FBBC7 File Offset: 0x000F9DC7
		public void ApplySkySceneSettings()
		{
			this.ApplySkySceneSettings(this.lightingPreset);
		}

		/// <summary>
		/// Apply only scene settings
		/// </summary>
		/// <param name="lightingPreset">Reference of the lightingPreset asset where the settings should retrieved</param>
		// Token: 0x060024B3 RID: 9395 RVA: 0x000FBBD8 File Offset: 0x000F9DD8
		public void ApplySceneSettings(LightingPreset lightingPreset, bool showDebugLine = true)
		{
			if (lightingPreset == null)
			{
				Debug.LogWarningFormat(this, "Cannot apply scene settings, no lightingPreset is referenced", Array.Empty<object>());
				return;
			}
			if (showDebugLine)
			{
				Debug.LogFormat(this, "Apply Scene Settings from " + lightingPreset.name, Array.Empty<object>());
			}
			LightingPreset copyLevelCurrentPreset;
			if (Application.isPlaying && Level.current != null)
			{
				if (Level.current.currentLightingPreset == null)
				{
					copyLevelCurrentPreset = ScriptableObject.CreateInstance<LightingPreset>();
					Level.current.currentLightingPreset = copyLevelCurrentPreset;
					copyLevelCurrentPreset.applyAtRuntime = false;
					copyLevelCurrentPreset.fog = LightingPreset.State.Disabled;
					copyLevelCurrentPreset.skybox = LightingPreset.State.Disabled;
					copyLevelCurrentPreset.clouds = LightingPreset.State.Disabled;
				}
				else
				{
					copyLevelCurrentPreset = Level.current.currentLightingPreset;
				}
			}
			else
			{
				copyLevelCurrentPreset = ScriptableObject.CreateInstance<LightingPreset>();
			}
			RenderSettings.ambientIntensity = lightingPreset.ambientIntensity;
			copyLevelCurrentPreset.ambientIntensity = lightingPreset.ambientIntensity;
			RenderSettings.subtractiveShadowColor = lightingPreset.shadowColor;
			copyLevelCurrentPreset.shadowColor = lightingPreset.shadowColor;
			if (lightingPreset.fog == LightingPreset.State.Enabled)
			{
				lightingPreset.ValidateFogParameters();
				RenderSettings.fog = true;
				copyLevelCurrentPreset.fog = LightingPreset.State.Enabled;
				RenderSettings.fogColor = lightingPreset.fogColor;
				copyLevelCurrentPreset.fogColor = lightingPreset.fogColor;
				RenderSettings.fogStartDistance = lightingPreset.fogStartDistance;
				copyLevelCurrentPreset.fogStartDistance = lightingPreset.fogStartDistance;
				RenderSettings.fogEndDistance = lightingPreset.fogEndDistance;
				copyLevelCurrentPreset.fogEndDistance = lightingPreset.fogEndDistance;
			}
			else if (lightingPreset.fog == LightingPreset.State.Disabled)
			{
				RenderSettings.fog = false;
				copyLevelCurrentPreset.fog = LightingPreset.State.Disabled;
			}
			this.ApplySkySceneSettings(lightingPreset);
			LightingGroup.sceneSettingsMaster = this;
			if (LightingGroup._updateRenderSettingEvent != null)
			{
				LightingGroup._updateRenderSettingEvent();
			}
		}

		/// <summary>
		/// Apply only sky scene settings. (directional lights, skybox and fog) 
		/// </summary>
		/// <param name="lightingPreset">Reference of the lightingPreset asset where the settings should retrieved</param>
		// Token: 0x060024B4 RID: 9396 RVA: 0x000FBD4C File Offset: 0x000F9F4C
		public void ApplySkySceneSettings(LightingPreset lightingPreset)
		{
			if (lightingPreset == null)
			{
				Debug.LogWarningFormat(this, "Cannot apply sky scene settings, no lightingPreset is referenced", Array.Empty<object>());
				return;
			}
			LightingPreset copyLevelCurrentPreset;
			if (Application.isPlaying && Level.current != null)
			{
				if (Level.current.currentLightingPreset == null)
				{
					copyLevelCurrentPreset = ScriptableObject.CreateInstance<LightingPreset>();
					Level.current.currentLightingPreset = copyLevelCurrentPreset;
					copyLevelCurrentPreset.applyAtRuntime = false;
					copyLevelCurrentPreset.fog = LightingPreset.State.Disabled;
					copyLevelCurrentPreset.skybox = LightingPreset.State.Disabled;
					copyLevelCurrentPreset.clouds = LightingPreset.State.Disabled;
				}
				else
				{
					copyLevelCurrentPreset = Level.current.currentLightingPreset;
				}
			}
			else
			{
				copyLevelCurrentPreset = ScriptableObject.CreateInstance<LightingPreset>();
			}
			if (!Application.isPlaying || lightingPreset.applyAtRuntime)
			{
				if (RenderSettings.sun)
				{
					copyLevelCurrentPreset.applyAtRuntime = true;
					RenderSettings.sun.color = lightingPreset.dirLightColor;
					copyLevelCurrentPreset.dirLightColor = lightingPreset.dirLightColor;
					RenderSettings.sun.intensity = lightingPreset.dirLightIntensity;
					copyLevelCurrentPreset.dirLightIntensity = lightingPreset.dirLightIntensity;
					RenderSettings.sun.bounceIntensity = lightingPreset.dirLightIndirectMultiplier;
					copyLevelCurrentPreset.dirLightIndirectMultiplier = lightingPreset.dirLightIndirectMultiplier;
					RenderSettings.sun.transform.rotation = base.transform.TransformRotation(lightingPreset.directionalLightLocalRotation);
					copyLevelCurrentPreset.directionalLightLocalRotation = base.transform.TransformRotation(lightingPreset.directionalLightLocalRotation);
				}
				else
				{
					Debug.LogError("Scene have no sun source set in lighting parameters");
				}
			}
			if (lightingPreset.skybox == LightingPreset.State.Enabled)
			{
				copyLevelCurrentPreset.skyBoxMaterialProp.CopyFrom(lightingPreset.skyBoxMaterialProp);
				if (lightingPreset.skyBoxMaterialProp.Mat)
				{
					RenderSettings.skybox = lightingPreset.skyBoxMaterialProp.GetInstancedMat(0f);
				}
				GlobalSkyCube.SetCubemapRotation((base.transform.eulerAngles.y > 0f) ? (360f - base.transform.eulerAngles.y) : (-base.transform.eulerAngles.y));
			}
			else if (lightingPreset.skybox == LightingPreset.State.Disabled)
			{
				RenderSettings.skybox = null;
				RenderSettings.ambientSkyColor = Color.black;
				copyLevelCurrentPreset.skybox = LightingPreset.State.Disabled;
			}
			if (Clouds.instance && Clouds.instance.meshRenderer)
			{
				if (lightingPreset.clouds == LightingPreset.State.Enabled)
				{
					copyLevelCurrentPreset.clouds = LightingPreset.State.Enabled;
					Clouds.instance.meshRenderer.enabled = true;
					MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
					Clouds.instance.meshRenderer.GetPropertyBlock(materialPropertyBlock);
					materialPropertyBlock.SetFloat("_CloudSoftness", lightingPreset.cloudsSoftness);
					copyLevelCurrentPreset.cloudsSoftness = lightingPreset.cloudsSoftness;
					materialPropertyBlock.SetFloat("_Speed", lightingPreset.cloudsSpeed);
					copyLevelCurrentPreset.cloudsSpeed = lightingPreset.cloudsSpeed;
					materialPropertyBlock.SetFloat("_Size", lightingPreset.cloudsSize);
					copyLevelCurrentPreset.cloudsSize = lightingPreset.cloudsSize;
					materialPropertyBlock.SetColor("_Color", lightingPreset.cloudsColor);
					copyLevelCurrentPreset.cloudsColor = lightingPreset.cloudsColor;
					materialPropertyBlock.SetFloat("_Alpha", lightingPreset.cloudsAlpha);
					copyLevelCurrentPreset.cloudsAlpha = lightingPreset.cloudsAlpha;
					Clouds.instance.meshRenderer.SetPropertyBlock(materialPropertyBlock);
					return;
				}
				if (lightingPreset.clouds == LightingPreset.State.Disabled)
				{
					Clouds.instance.meshRenderer.enabled = false;
					copyLevelCurrentPreset.clouds = LightingPreset.State.Disabled;
				}
			}
		}

		/// <summary>
		/// Apply only scene settings
		/// </summary>
		/// <param name="t">When t is 0 current level preset is fully apply, when t is 1 this lighting preset is fully apply</param>
		// Token: 0x060024B5 RID: 9397 RVA: 0x000FC068 File Offset: 0x000FA268
		public void BlendSceneSettingsWithCurrent(float t, bool showDebugLine = true, bool applySun = true)
		{
			if (LightingGroup.sceneSettingsMaster == this)
			{
				return;
			}
			if (Level.current == null)
			{
				Debug.LogWarningFormat(this, "Cannot blend scene settings, no current level found", Array.Empty<object>());
				return;
			}
			LightingPreset currentLightingPreset = Level.current.currentLightingPreset;
			if (currentLightingPreset == null)
			{
				Debug.LogWarningFormat(this, "Cannot blend scene settings, level currentLightingPreset is not set", Array.Empty<object>());
				return;
			}
			if (this.lightingPreset == null)
			{
				Debug.LogWarningFormat(this, "Cannot blend scene settings, no lightingPreset is referenced", Array.Empty<object>());
				return;
			}
			RenderSettings.ambientIntensity = Mathf.Lerp(currentLightingPreset.ambientIntensity, this.lightingPreset.ambientIntensity, t);
			RenderSettings.subtractiveShadowColor = Color.Lerp(currentLightingPreset.shadowColor, this.lightingPreset.shadowColor, t);
			if (this.lightingPreset.fog != LightingPreset.State.NoChange)
			{
				if (currentLightingPreset.fog == LightingPreset.State.Disabled && this.lightingPreset.fog == LightingPreset.State.Disabled)
				{
					RenderSettings.fog = false;
				}
				else
				{
					Color fogColor = currentLightingPreset.fogColor;
					float fogStartDistance = currentLightingPreset.fogStartDistance;
					float fogEndDistance = currentLightingPreset.fogEndDistance;
					if (currentLightingPreset.fog == LightingPreset.State.Disabled)
					{
						fogColor = Color.white;
						fogStartDistance = Camera.main.farClipPlane;
						fogEndDistance = Camera.main.farClipPlane;
					}
					if (this.lightingPreset.fog == LightingPreset.State.Enabled)
					{
						fogColor = Color.Lerp(fogColor, this.lightingPreset.fogColor, t);
						fogStartDistance = this.FogDistanceSmoothStep(fogStartDistance, this.lightingPreset.fogStartDistance, t);
						fogEndDistance = this.FogDistanceSmoothStep(fogEndDistance, this.lightingPreset.fogEndDistance, t);
					}
					else
					{
						Color tempColor = Color.white;
						fogColor = Color.Lerp(fogColor, tempColor, t);
						fogStartDistance = this.FogDistanceSmoothStep(fogStartDistance, Camera.main.farClipPlane, t);
						fogEndDistance = this.FogDistanceSmoothStep(fogEndDistance, Camera.main.farClipPlane, t);
					}
					if (fogStartDistance > fogEndDistance)
					{
						fogStartDistance = fogEndDistance - 0.01f;
					}
					RenderSettings.fog = true;
					RenderSettings.fogColor = fogColor;
					RenderSettings.fogStartDistance = fogStartDistance;
					RenderSettings.fogEndDistance = fogEndDistance;
				}
				if (LightingGroup._updateRenderSettingEvent != null)
				{
					LightingGroup._updateRenderSettingEvent();
				}
			}
		}

		// Token: 0x060024B6 RID: 9398 RVA: 0x000FC240 File Offset: 0x000FA440
		private float FogDistanceSmoothStep(float from, float to, float t)
		{
			float smoothRatioStopValue = 500f;
			float min = from;
			float tSmooth = t;
			if (to < min)
			{
				min = to;
				tSmooth = 1f - t;
			}
			float smoothRatio = 1f;
			if (min < smoothRatioStopValue)
			{
				smoothRatio = Mathf.Lerp(min, smoothRatioStopValue, tSmooth) / smoothRatioStopValue;
				smoothRatio *= smoothRatio;
			}
			float factor = to - from;
			return from + t * smoothRatio * factor;
		}

		// Token: 0x060024B7 RID: 9399 RVA: 0x000FC28C File Offset: 0x000FA48C
		private void FogDistanceSmoothStep(float startDistanceFrom, float endDistanceFrom, float startDistanceTo, float endDistanceTo, float t, out float startDistance, out float endDistance)
		{
			float a = endDistanceFrom - startDistanceFrom;
			float diffTo = endDistanceTo - startDistanceTo;
			float diff = Mathf.Lerp(a, diffTo, t);
			startDistance = Mathf.Lerp(startDistanceFrom, startDistanceTo, t);
			endDistance = startDistance + diff;
		}

		/// <summary>
		/// Clear all lightingPreset settings except scene settings
		/// </summary>
		// Token: 0x060024B8 RID: 9400 RVA: 0x000FC2BE File Offset: 0x000FA4BE
		public void ClearAll()
		{
			if (!this.initialized)
			{
				return;
			}
			this.ClearAll(this.lightingPreset);
			this.initialized = false;
		}

		// Token: 0x060024B9 RID: 9401 RVA: 0x000FC2DC File Offset: 0x000FA4DC
		public void ClearLightingPreset()
		{
			if (!this.initialized)
			{
				return;
			}
			if (this.lightingPreset == null)
			{
				return;
			}
			this.ClearLightmaps(this.lightingPreset);
			this.ClearLightProbeVolumes(this.lightingPreset);
			this.lightingPreset.OnReleasePreset();
			this.lightingPreset = null;
			this.currentLightingPreset = null;
		}

		// Token: 0x060024BA RID: 9402 RVA: 0x000FC332 File Offset: 0x000FA532
		public void ClearAll(LightingPreset lightingPreset)
		{
			this.ClearLightmaps(lightingPreset);
			this.ClearLightProbeVolumes(lightingPreset);
			this.ClearSceneSettings();
		}

		// Token: 0x060024BB RID: 9403 RVA: 0x000FC348 File Offset: 0x000FA548
		public void ClearLightmaps(LightingPreset lightingPreset = null)
		{
			string lightingPresetName = (lightingPreset != null) ? lightingPreset.name : "Unknown - Lightning Preset already Destroyed";
			Debug.LogFormat(this, "Clear lightmaps from {0}", new object[]
			{
				lightingPresetName
			});
			LightingGroup.LightmapIndexHelper lightmapIndexHelper = LightingGroup.lightmapIndexHelper;
			if (lightmapIndexHelper != null)
			{
				lightmapIndexHelper.InitFromLightmap();
			}
			foreach (LightingGroup.MeshRendererReference meshRendererReference in this.meshRendererReferences)
			{
				if (!(meshRendererReference.meshRenderer == null) && meshRendererReference.meshRenderer.lightmapIndex >= 0)
				{
					LightingGroup.LightmapIndexHelper lightmapIndexHelper2 = LightingGroup.lightmapIndexHelper;
					if (lightmapIndexHelper2 != null)
					{
						lightmapIndexHelper2.RemoveLightmap(meshRendererReference.meshRenderer);
					}
				}
			}
			LightingGroup.LightmapIndexHelper lightmapIndexHelper3 = LightingGroup.lightmapIndexHelper;
			if (lightmapIndexHelper3 == null)
			{
				return;
			}
			lightmapIndexHelper3.SetLightmap();
		}

		// Token: 0x060024BC RID: 9404 RVA: 0x000FC414 File Offset: 0x000FA614
		public void ClearLightProbeVolumes(LightingPreset lightingPreset = null)
		{
			int count = this.lightProbeVolumeReferences.Count;
			for (int i = 0; i < count; i++)
			{
				LightProbeVolume lightProbeVolume = this.lightProbeVolumeReferences[i].lightProbeVolume;
				if (lightProbeVolume != null)
				{
					lightProbeVolume.SetTexture(null, null, null, null);
				}
			}
		}

		// Token: 0x060024BD RID: 9405 RVA: 0x000FC460 File Offset: 0x000FA660
		public void ClearSceneSettings()
		{
			if (LightingGroup.sceneSettingsMaster != this)
			{
				return;
			}
			Debug.LogFormat(this, "Clear scene settings", Array.Empty<object>());
			LightingGroup.sceneSettingsMaster = null;
			if (LightingGroup.allActive.Count > 0)
			{
				LightingGroup.allActive[0].ApplySceneSettings(true);
			}
		}

		// Token: 0x060024BE RID: 9406 RVA: 0x000FC4B0 File Offset: 0x000FA6B0
		public bool HasMeshRendererReference(MeshRenderer meshRenderer)
		{
			using (List<LightingGroup.MeshRendererReference>.Enumerator enumerator = this.meshRendererReferences.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.meshRenderer == meshRenderer)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060024BF RID: 9407 RVA: 0x000FC510 File Offset: 0x000FA710
		public bool TryGetMeshRendererGuid(MeshRenderer meshRenderer, out string meshRendererGuid)
		{
			foreach (LightingGroup.MeshRendererReference meshRendererReference in this.meshRendererReferences)
			{
				if (meshRendererReference.meshRenderer == meshRenderer)
				{
					meshRendererGuid = meshRendererReference.guid;
					return true;
				}
			}
			meshRendererGuid = null;
			return false;
		}

		// Token: 0x060024C0 RID: 9408 RVA: 0x000FC57C File Offset: 0x000FA77C
		public bool TryGetMeshRenderer(string meshRendererGuid, out MeshRenderer meshRenderer)
		{
			if (this._meshRendererByGuid == null)
			{
				int count = this.meshRendererReferences.Count;
				this._meshRendererByGuid = new Dictionary<string, MeshRenderer>(count);
				for (int i = 0; i < count; i++)
				{
					LightingGroup.MeshRendererReference meshRendererReference = this.meshRendererReferences[i];
					if (!(meshRendererReference.meshRenderer == null))
					{
						this._meshRendererByGuid.Add(meshRendererReference.guid, meshRendererReference.meshRenderer);
					}
				}
			}
			return this._meshRendererByGuid.TryGetValue(meshRendererGuid, out meshRenderer);
		}

		// Token: 0x060024C1 RID: 9409 RVA: 0x000FC5F4 File Offset: 0x000FA7F4
		public bool HasLightReference(Light light)
		{
			using (List<LightingGroup.LightReference>.Enumerator enumerator = this.lightReferences.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.light == light)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060024C2 RID: 9410 RVA: 0x000FC654 File Offset: 0x000FA854
		public bool TryGetLight(string lightGuid, out Light light)
		{
			foreach (LightingGroup.LightReference lightReference in this.lightReferences)
			{
				if (lightReference.guid == lightGuid)
				{
					light = lightReference.light;
					return true;
				}
			}
			light = null;
			return false;
		}

		// Token: 0x060024C3 RID: 9411 RVA: 0x000FC6C0 File Offset: 0x000FA8C0
		public bool HasLightProbeVolumeReference(LightProbeVolume lightProbeVolume)
		{
			using (List<LightingGroup.LightProbeVolumeReference>.Enumerator enumerator = this.lightProbeVolumeReferences.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.lightProbeVolume == lightProbeVolume)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060024C4 RID: 9412 RVA: 0x000FC720 File Offset: 0x000FA920
		public bool TryGetLightProbeVolume(string lightGuid, out LightProbeVolume lightProbeVolume)
		{
			foreach (LightingGroup.LightProbeVolumeReference lightProbeVolumeReference in this.lightProbeVolumeReferences)
			{
				if (lightProbeVolumeReference.guid == lightGuid)
				{
					lightProbeVolume = lightProbeVolumeReference.lightProbeVolume;
					return true;
				}
			}
			lightProbeVolume = null;
			return false;
		}

		// Token: 0x060024C5 RID: 9413 RVA: 0x000FC78C File Offset: 0x000FA98C
		public bool HasReflectionProbeReference(ReflectionProbe reflectionProbe)
		{
			using (List<LightingGroup.ReflectionProbeReference>.Enumerator enumerator = this.reflectionProbeReferences.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.reflectionProbe == reflectionProbe)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060024C6 RID: 9414 RVA: 0x000FC7EC File Offset: 0x000FA9EC
		public bool TryGetReflectionProbe(string reflectionProbeGuid, out ReflectionProbe reflectionProbe)
		{
			foreach (LightingGroup.ReflectionProbeReference reflectionProbeReference in this.reflectionProbeReferences)
			{
				if (reflectionProbeReference.guid == reflectionProbeGuid)
				{
					reflectionProbe = reflectionProbeReference.reflectionProbe;
					return true;
				}
			}
			reflectionProbe = null;
			return false;
		}

		// Token: 0x060024C7 RID: 9415 RVA: 0x000FC858 File Offset: 0x000FAA58
		public bool TryGetLightmapIndex(Texture2D lightmapColor, List<LightmapData> lightmaps, out int lightmapIndex)
		{
			for (int i = 0; i < lightmaps.Count; i++)
			{
				if (lightmaps[i].lightmapColor == lightmapColor)
				{
					lightmapIndex = i;
					return true;
				}
			}
			lightmapIndex = -1;
			return false;
		}

		// Token: 0x04002436 RID: 9270
		public static List<LightingGroup> allActive = new List<LightingGroup>();

		// Token: 0x04002437 RID: 9271
		public static LightingGroup sceneSettingsMaster;

		// Token: 0x04002438 RID: 9272
		private static Action _updateRenderSettingEvent;

		// Token: 0x04002439 RID: 9273
		public LightingPreset lightingPreset;

		// Token: 0x0400243A RID: 9274
		public bool changeLightmapFormatForAndroid;

		// Token: 0x0400243B RID: 9275
		public List<LightingGroup.MeshRendererReference> meshRendererReferences = new List<LightingGroup.MeshRendererReference>();

		// Token: 0x0400243C RID: 9276
		public List<LightingGroup.LightReference> lightReferences = new List<LightingGroup.LightReference>();

		// Token: 0x0400243D RID: 9277
		public List<LightingGroup.LightProbeVolumeReference> lightProbeVolumeReferences = new List<LightingGroup.LightProbeVolumeReference>();

		// Token: 0x0400243E RID: 9278
		public List<LightingGroup.ReflectionProbeReference> reflectionProbeReferences = new List<LightingGroup.ReflectionProbeReference>();

		// Token: 0x0400243F RID: 9279
		protected LightingPreset currentLightingPreset;

		// Token: 0x04002440 RID: 9280
		protected List<BakedLODGroup> bakedLODGroups;

		// Token: 0x04002441 RID: 9281
		private bool isStarted;

		// Token: 0x04002442 RID: 9282
		[NonSerialized]
		public bool initialized;

		// Token: 0x04002443 RID: 9283
		public bool initializedDebug;

		// Token: 0x04002444 RID: 9284
		private Dictionary<string, MeshRenderer> _meshRendererByGuid;

		// Token: 0x04002445 RID: 9285
		private static LightingGroup.LightmapIndexHelper lightmapIndexHelper;

		// Token: 0x020009F4 RID: 2548
		[Serializable]
		public class MeshRendererReference
		{
			// Token: 0x060044FE RID: 17662 RVA: 0x00194BAC File Offset: 0x00192DAC
			public MeshRendererReference(MeshRenderer meshRenderer)
			{
				this.guid = Guid.NewGuid().ToString();
				this.meshRenderer = meshRenderer;
			}

			// Token: 0x04004684 RID: 18052
			public string guid;

			// Token: 0x04004685 RID: 18053
			public MeshRenderer meshRenderer;
		}

		// Token: 0x020009F5 RID: 2549
		[Serializable]
		public class LightReference
		{
			// Token: 0x060044FF RID: 17663 RVA: 0x00194BE0 File Offset: 0x00192DE0
			public LightReference(Light light)
			{
				this.guid = Guid.NewGuid().ToString();
				this.light = light;
			}

			// Token: 0x04004686 RID: 18054
			public string guid;

			// Token: 0x04004687 RID: 18055
			public Light light;
		}

		// Token: 0x020009F6 RID: 2550
		[Serializable]
		public class LightProbeVolumeReference
		{
			// Token: 0x06004500 RID: 17664 RVA: 0x00194C14 File Offset: 0x00192E14
			public LightProbeVolumeReference(LightProbeVolume lightProbeVolume)
			{
				this.guid = Guid.NewGuid().ToString();
				this.lightProbeVolume = lightProbeVolume;
			}

			// Token: 0x04004688 RID: 18056
			public string guid;

			// Token: 0x04004689 RID: 18057
			public LightProbeVolume lightProbeVolume;
		}

		// Token: 0x020009F7 RID: 2551
		[Serializable]
		public class ReflectionProbeReference
		{
			// Token: 0x06004501 RID: 17665 RVA: 0x00194C48 File Offset: 0x00192E48
			public ReflectionProbeReference(ReflectionProbe reflectionProbe)
			{
				this.guid = Guid.NewGuid().ToString();
				this.reflectionProbe = reflectionProbe;
			}

			// Token: 0x0400468A RID: 18058
			public string guid;

			// Token: 0x0400468B RID: 18059
			public ReflectionProbe reflectionProbe;
		}

		// Token: 0x020009F8 RID: 2552
		public class LightmapIndexHelper
		{
			// Token: 0x06004502 RID: 17666 RVA: 0x00194C7C File Offset: 0x00192E7C
			public LightmapIndexHelper()
			{
				this._freeIndex = new Queue<int>();
				this._lightmapIndexCountMapping = new Dictionary<int, int>();
				this._defaultEmptyLightmap = new LightmapData();
				bool allLightmapNull = true;
				LightmapData[] lightmaps = LightmapSettings.lightmaps;
				for (int i = 0; i < lightmaps.Length; i++)
				{
					if (lightmaps[i].lightmapColor == null && lightmaps[i].lightmapDir == null && lightmaps[i].shadowMask == null)
					{
						lightmaps[i] = this._defaultEmptyLightmap;
						this._freeIndex.Enqueue(i);
					}
					else
					{
						allLightmapNull = false;
					}
				}
				if (allLightmapNull)
				{
					this._freeIndex.Clear();
					LightmapSettings.lightmaps = null;
					return;
				}
				LightmapSettings.lightmaps = lightmaps;
			}

			// Token: 0x06004503 RID: 17667 RVA: 0x00194D2A File Offset: 0x00192F2A
			public void InitFromLightmap()
			{
				this._lightmaps = new List<LightmapData>(LightmapSettings.lightmaps);
			}

			// Token: 0x06004504 RID: 17668 RVA: 0x00194D3C File Offset: 0x00192F3C
			public void SetLightmap()
			{
				if (this._lightmaps == null)
				{
					return;
				}
				LightmapSettings.lightmaps = this._lightmaps.ToArray();
				this._lightmaps = null;
			}

			// Token: 0x06004505 RID: 17669 RVA: 0x00194D60 File Offset: 0x00192F60
			public void SetMeshLightmapIndex(List<MeshRenderer> meshList, LightmapData lightmapData)
			{
				if (this._lightmaps == null)
				{
					return;
				}
				int meshCount = meshList.Count;
				for (int i = 0; i < this._lightmaps.Count; i++)
				{
					if (this._lightmaps[i] != this._defaultEmptyLightmap && this._lightmapIndexCountMapping.ContainsKey(i) && this._lightmaps[i].lightmapColor == lightmapData.lightmapColor)
					{
						for (int indexMesh = 0; indexMesh < meshCount; indexMesh++)
						{
							meshList[indexMesh].lightmapIndex = i;
							Dictionary<int, int> lightmapIndexCountMapping = this._lightmapIndexCountMapping;
							int num = i;
							int num2 = lightmapIndexCountMapping[num];
							lightmapIndexCountMapping[num] = num2 + 1;
						}
						return;
					}
				}
				int index = this._lightmaps.Count;
				while (this._freeIndex.Count > 0)
				{
					index = this._freeIndex.Dequeue();
					if (index >= 0 && index < this._lightmaps.Count && this._lightmaps[index] == this._defaultEmptyLightmap)
					{
						this._lightmaps[index] = lightmapData;
						this._lightmapIndexCountMapping.Add(index, 0);
						for (int indexMesh2 = 0; indexMesh2 < meshCount; indexMesh2++)
						{
							meshList[indexMesh2].lightmapIndex = index;
							Dictionary<int, int> lightmapIndexCountMapping2 = this._lightmapIndexCountMapping;
							int num2 = index;
							int num = lightmapIndexCountMapping2[num2];
							lightmapIndexCountMapping2[num2] = num + 1;
						}
						return;
					}
				}
				index = this._lightmaps.Count;
				this._lightmaps.Add(lightmapData);
				this._lightmapIndexCountMapping.Add(index, 0);
				for (int indexMesh3 = 0; indexMesh3 < meshCount; indexMesh3++)
				{
					meshList[indexMesh3].lightmapIndex = index;
					Dictionary<int, int> lightmapIndexCountMapping3 = this._lightmapIndexCountMapping;
					int num = index;
					int num2 = lightmapIndexCountMapping3[num];
					lightmapIndexCountMapping3[num] = num2 + 1;
				}
			}

			// Token: 0x06004506 RID: 17670 RVA: 0x00194F1C File Offset: 0x0019311C
			public void RemoveLightmap(MeshRenderer mesh)
			{
				if (this._lightmaps == null)
				{
					return;
				}
				int index = mesh.lightmapIndex;
				if (this._lightmapIndexCountMapping.ContainsKey(index))
				{
					Dictionary<int, int> lightmapIndexCountMapping = this._lightmapIndexCountMapping;
					int lightmapIndex = mesh.lightmapIndex;
					int num = lightmapIndexCountMapping[lightmapIndex];
					lightmapIndexCountMapping[lightmapIndex] = num - 1;
					if (this._lightmapIndexCountMapping[index] <= 0)
					{
						this._lightmaps[index].lightmapColor = null;
						this._lightmaps[index].lightmapDir = null;
						this._lightmaps[index].shadowMask = null;
						this._lightmaps[index] = this._defaultEmptyLightmap;
						this._freeIndex.Enqueue(index);
						this._lightmapIndexCountMapping.Remove(index);
						if (this._lightmapIndexCountMapping.Count == 0)
						{
							this._freeIndex.Clear();
							this._lightmaps.Clear();
						}
					}
				}
			}

			// Token: 0x0400468C RID: 18060
			private List<LightmapData> _lightmaps;

			// Token: 0x0400468D RID: 18061
			private Queue<int> _freeIndex;

			// Token: 0x0400468E RID: 18062
			private Dictionary<int, int> _lightmapIndexCountMapping;

			// Token: 0x0400468F RID: 18063
			private LightmapData _defaultEmptyLightmap;
		}
	}
}
