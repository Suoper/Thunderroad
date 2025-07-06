using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ThunderRoad
{
	// Token: 0x02000194 RID: 404
	public class EffectModuleMesh : EffectModule
	{
		// Token: 0x17000139 RID: 313
		// (get) Token: 0x0600135F RID: 4959 RVA: 0x00088CF0 File Offset: 0x00086EF0
		// (set) Token: 0x06001360 RID: 4960 RVA: 0x00088E90 File Offset: 0x00087090
		protected Gradient MainGradient
		{
			get
			{
				if (this.mainGradient != null)
				{
					this.mainColorStart = this.mainGradient.colorKeys[0].color;
					this.mainColorStart.a = this.mainGradient.alphaKeys[0].alpha;
					this.mainColorEnd = this.mainGradient.colorKeys[this.mainGradient.colorKeys.Length - 1].color;
					this.mainColorEnd.a = this.mainGradient.alphaKeys[this.mainGradient.alphaKeys.Length - 1].alpha;
					return this.mainGradient;
				}
				if (this.mainColorStart != Color.clear || this.mainColorEnd != Color.clear)
				{
					this.mainGradient = new Gradient();
					GradientColorKey[] colorKeys = new GradientColorKey[2];
					GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
					colorKeys[0].color = this.mainColorStart;
					colorKeys[0].time = 0f;
					alphaKeys[0].alpha = this.mainColorStart.a;
					alphaKeys[0].time = 0f;
					colorKeys[1].color = this.mainColorEnd;
					colorKeys[1].time = 1f;
					alphaKeys[1].alpha = this.mainColorEnd.a;
					alphaKeys[1].time = 1f;
					this.mainGradient.SetKeys(colorKeys, alphaKeys);
				}
				return this.mainGradient;
			}
			set
			{
				this.mainGradient = value;
				if (value != null)
				{
					this.mainColorStart = value.colorKeys[0].color;
					this.mainColorStart.a = value.alphaKeys[0].alpha;
					this.mainColorEnd = value.colorKeys[value.colorKeys.Length - 1].color;
					this.mainColorEnd.a = value.alphaKeys[value.alphaKeys.Length - 1].alpha;
				}
			}
		}

		// Token: 0x1700013A RID: 314
		// (get) Token: 0x06001361 RID: 4961 RVA: 0x00088F20 File Offset: 0x00087120
		// (set) Token: 0x06001362 RID: 4962 RVA: 0x000890C0 File Offset: 0x000872C0
		protected Gradient SecondaryGradient
		{
			get
			{
				if (this.secondaryGradient != null)
				{
					this.secondaryColorStart = this.secondaryGradient.colorKeys[0].color;
					this.secondaryColorStart.a = this.secondaryGradient.alphaKeys[0].alpha;
					this.secondaryColorEnd = this.secondaryGradient.colorKeys[this.secondaryGradient.colorKeys.Length - 1].color;
					this.secondaryColorEnd.a = this.secondaryGradient.alphaKeys[this.secondaryGradient.alphaKeys.Length - 1].alpha;
					return this.secondaryGradient;
				}
				if (this.secondaryColorStart != Color.clear || this.secondaryColorEnd != Color.clear)
				{
					this.secondaryGradient = new Gradient();
					GradientColorKey[] colorKeys = new GradientColorKey[2];
					GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
					colorKeys[0].color = this.secondaryColorStart;
					colorKeys[0].time = 0f;
					alphaKeys[0].alpha = this.secondaryColorStart.a;
					alphaKeys[0].time = 0f;
					colorKeys[1].color = this.secondaryColorEnd;
					colorKeys[1].time = 1f;
					alphaKeys[1].alpha = this.secondaryColorEnd.a;
					alphaKeys[1].time = 1f;
					this.secondaryGradient.SetKeys(colorKeys, alphaKeys);
				}
				return this.secondaryGradient;
			}
			set
			{
				this.secondaryGradient = value;
				if (value != null)
				{
					this.secondaryColorStart = value.colorKeys[0].color;
					this.secondaryColorStart.a = value.alphaKeys[0].alpha;
					this.secondaryColorEnd = value.colorKeys[value.colorKeys.Length - 1].color;
					this.secondaryColorEnd.a = value.alphaKeys[value.alphaKeys.Length - 1].alpha;
				}
			}
		}

		// Token: 0x1700013B RID: 315
		// (get) Token: 0x06001363 RID: 4963 RVA: 0x00089150 File Offset: 0x00087350
		// (set) Token: 0x06001364 RID: 4964 RVA: 0x000892F0 File Offset: 0x000874F0
		protected Gradient MainGradientNoHdr
		{
			get
			{
				if (this.mainGradientNoHdr != null)
				{
					this.mainNoHdrColorStart = this.mainGradientNoHdr.colorKeys[0].color;
					this.mainNoHdrColorStart.a = this.mainGradientNoHdr.alphaKeys[0].alpha;
					this.mainNoHdrColorEnd = this.mainGradientNoHdr.colorKeys[this.mainGradientNoHdr.colorKeys.Length - 1].color;
					this.mainNoHdrColorEnd.a = this.mainGradientNoHdr.alphaKeys[this.mainGradientNoHdr.alphaKeys.Length - 1].alpha;
					return this.mainGradientNoHdr;
				}
				if (this.mainNoHdrColorStart != Color.clear || this.mainNoHdrColorEnd != Color.clear)
				{
					this.mainGradientNoHdr = new Gradient();
					GradientColorKey[] colorKeys = new GradientColorKey[2];
					GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
					colorKeys[0].color = this.mainNoHdrColorStart;
					colorKeys[0].time = 0f;
					alphaKeys[0].alpha = this.mainNoHdrColorStart.a;
					alphaKeys[0].time = 0f;
					colorKeys[1].color = this.mainNoHdrColorEnd;
					colorKeys[1].time = 1f;
					alphaKeys[1].alpha = this.mainNoHdrColorEnd.a;
					alphaKeys[1].time = 1f;
					this.mainGradientNoHdr.SetKeys(colorKeys, alphaKeys);
				}
				return this.mainGradientNoHdr;
			}
			set
			{
				this.mainGradientNoHdr = value;
				if (value != null)
				{
					this.mainNoHdrColorStart = value.colorKeys[0].color;
					this.mainNoHdrColorStart.a = value.alphaKeys[0].alpha;
					this.mainNoHdrColorEnd = value.colorKeys[value.colorKeys.Length - 1].color;
					this.mainNoHdrColorEnd.a = value.alphaKeys[value.alphaKeys.Length - 1].alpha;
				}
			}
		}

		// Token: 0x1700013C RID: 316
		// (get) Token: 0x06001365 RID: 4965 RVA: 0x00089380 File Offset: 0x00087580
		// (set) Token: 0x06001366 RID: 4966 RVA: 0x00089520 File Offset: 0x00087720
		protected Gradient SecondaryGradientNoHdr
		{
			get
			{
				if (this.secondaryGradientNoHdr != null)
				{
					this.secondaryNoHdrColorStart = this.secondaryGradientNoHdr.colorKeys[0].color;
					this.secondaryNoHdrColorStart.a = this.secondaryGradientNoHdr.alphaKeys[0].alpha;
					this.secondaryNoHdrColorEnd = this.secondaryGradientNoHdr.colorKeys[this.secondaryGradientNoHdr.colorKeys.Length - 1].color;
					this.secondaryNoHdrColorEnd.a = this.secondaryGradientNoHdr.alphaKeys[this.secondaryGradientNoHdr.alphaKeys.Length - 1].alpha;
					return this.secondaryGradientNoHdr;
				}
				if (this.secondaryNoHdrColorStart != Color.clear || this.secondaryNoHdrColorEnd != Color.clear)
				{
					this.secondaryGradientNoHdr = new Gradient();
					GradientColorKey[] colorKeys = new GradientColorKey[2];
					GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
					colorKeys[0].color = this.secondaryNoHdrColorStart;
					colorKeys[0].time = 0f;
					alphaKeys[0].alpha = this.secondaryNoHdrColorStart.a;
					alphaKeys[0].time = 0f;
					colorKeys[1].color = this.secondaryNoHdrColorEnd;
					colorKeys[1].time = 1f;
					alphaKeys[1].alpha = this.secondaryNoHdrColorEnd.a;
					alphaKeys[1].time = 1f;
					this.secondaryGradientNoHdr.SetKeys(colorKeys, alphaKeys);
				}
				return this.secondaryGradientNoHdr;
			}
			set
			{
				this.secondaryGradientNoHdr = value;
				if (value != null)
				{
					this.secondaryNoHdrColorStart = value.colorKeys[0].color;
					this.secondaryNoHdrColorStart.a = value.alphaKeys[0].alpha;
					this.secondaryNoHdrColorEnd = value.colorKeys[value.colorKeys.Length - 1].color;
					this.secondaryNoHdrColorEnd.a = value.alphaKeys[value.alphaKeys.Length - 1].alpha;
				}
			}
		}

		// Token: 0x06001367 RID: 4967 RVA: 0x000895AF File Offset: 0x000877AF
		public override void OnCatalogRefresh(EffectData effectData, bool editorLoad = false)
		{
			base.OnCatalogRefresh(effectData, editorLoad);
			Gradient gradient = this.MainGradient;
			Gradient gradient2 = this.SecondaryGradient;
			Gradient gradient3 = this.MainGradientNoHdr;
			Gradient gradient4 = this.SecondaryGradientNoHdr;
		}

		// Token: 0x06001368 RID: 4968 RVA: 0x000895D5 File Offset: 0x000877D5
		public override IEnumerator RefreshCoroutine(EffectData effectData, bool editorLoad = false)
		{
			if (editorLoad)
			{
				this.mesh = base.EditorLoad<Mesh>(this.meshAddress);
				yield return null;
			}
			else
			{
				if (this.mesh)
				{
					Catalog.ReleaseAsset<Mesh>(this.mesh);
				}
				yield return Catalog.LoadAssetCoroutine<Mesh>(this.meshAddress, delegate(Mesh value)
				{
					this.mesh = value;
				}, effectData.id + " (EffectModuleMesh)");
			}
			if (this.materials == null)
			{
				yield break;
			}
			int materialsCount = this.materials.Count;
			int num;
			for (int i = 0; i < materialsCount; i = num + 1)
			{
				EffectModuleMesh.<>c__DisplayClass48_0 CS$<>8__locals1 = new EffectModuleMesh.<>c__DisplayClass48_0();
				EffectModuleMesh.Materials mat = this.materials[i];
				if (!string.IsNullOrEmpty(mat.materialAddress))
				{
					EffectModuleMesh.Materials.Material material = mat as EffectModuleMesh.Materials.Material;
					if (material != null && material.value && !editorLoad && !EffectModuleMesh.pools.IsNullOrEmpty())
					{
						Catalog.ReleaseAsset<Material>(material.value);
					}
					CS$<>8__locals1.m = null;
					if (editorLoad)
					{
						CS$<>8__locals1.m = base.EditorLoad<Material>(mat.materialAddress);
						yield return null;
					}
					else
					{
						yield return Catalog.LoadAssetCoroutine<Material>(mat.materialAddress, delegate(Material value)
						{
							CS$<>8__locals1.m = value;
						}, effectData.id + " (EffectModuleMesh)");
					}
					if (CS$<>8__locals1.m != null)
					{
						this.materials[i] = new EffectModuleMesh.Materials.Material
						{
							materialAddress = mat.materialAddress,
							value = new Material(CS$<>8__locals1.m)
						};
					}
					CS$<>8__locals1 = null;
					mat = null;
				}
				num = i;
			}
			yield break;
		}

		// Token: 0x06001369 RID: 4969 RVA: 0x000895F4 File Offset: 0x000877F4
		public override void Clean()
		{
			if (this.intensityCurve != null && this.intensityCurve.keys.Length == 0)
			{
				this.intensityCurve = null;
			}
			if (this.sizeCurve != null && this.sizeCurve.keys.Length == 0)
			{
				this.sizeCurve = null;
			}
			if (this.rotationYCurve != null && this.rotationYCurve.keys.Length == 0)
			{
				this.rotationYCurve = null;
			}
			if (this.mainGradient != null && this.mainGradient.alphaKeys.Length == 2 && this.mainGradient.colorKeys.Length == 2 && this.mainGradient.alphaKeys[0].alpha == 1f && this.mainGradient.alphaKeys[1].alpha == 1f && this.mainGradient.alphaKeys[0].time == 0f && this.mainGradient.alphaKeys[1].time == 1f && this.mainGradient.colorKeys[0].color == Color.white && this.mainGradient.colorKeys[1].color == Color.white && this.mainGradient.colorKeys[0].time == 0f && this.mainGradient.colorKeys[1].time == 1f)
			{
				this.mainGradient = null;
			}
			if (this.secondaryGradient != null && this.secondaryGradient.alphaKeys.Length == 2 && this.secondaryGradient.colorKeys.Length == 2 && this.secondaryGradient.alphaKeys[0].alpha == 1f && this.secondaryGradient.alphaKeys[1].alpha == 1f && this.secondaryGradient.alphaKeys[0].time == 0f && this.secondaryGradient.alphaKeys[1].time == 1f && this.secondaryGradient.colorKeys[0].color == Color.white && this.secondaryGradient.colorKeys[1].color == Color.white && this.secondaryGradient.colorKeys[0].time == 0f && this.secondaryGradient.colorKeys[1].time == 1f)
			{
				this.secondaryGradient = null;
			}
		}

		// Token: 0x0600136A RID: 4970 RVA: 0x000898C4 File Offset: 0x00087AC4
		public override void CopyHDRToNonHDR()
		{
			this.mainGradientNoHdr = this.mainGradient;
			this.secondaryGradientNoHdr = this.secondaryGradient;
		}

		/// <summary>
		/// This will get or create the EffectMeshPoolManager for this EffectModuleMesh
		/// </summary>
		// Token: 0x0600136B RID: 4971 RVA: 0x000898E0 File Offset: 0x00087AE0
		public static EffectMeshPoolManager GetOrCreateEffectMeshPoolManager(EffectModuleMesh effectModuleMesh)
		{
			EffectMeshPoolManager poolManager;
			if (EffectModuleMesh.pools.TryGetValue(effectModuleMesh, out poolManager))
			{
				return poolManager;
			}
			poolManager = new EffectMeshPoolManager(EffectModuleMesh.poolRoot, effectModuleMesh, PoolType.Stack, GameManager.local.PoolCollectionChecks, Catalog.gameData.platformParameters.poolingMeshCount, Catalog.gameData.platformParameters.poolingMeshCount, true);
			EffectModuleMesh.pools.Add(effectModuleMesh, poolManager);
			return poolManager;
		}

		// Token: 0x0600136C RID: 4972 RVA: 0x00089941 File Offset: 0x00087B41
		public static IEnumerator GeneratePool()
		{
			yield return EffectModuleMesh.ClearPool();
			if (!Catalog.gameData.platformParameters.enableEffectMesh)
			{
				yield break;
			}
			List<CatalogData> effectDatas = Catalog.GetDataList(Category.Effect);
			int count = effectDatas.Count;
			for (int i = 0; i < count; i++)
			{
				EffectData effectData = effectDatas[i] as EffectData;
				if (effectData != null)
				{
					for (int j = 0; j < effectData.modules.Count; j++)
					{
						EffectModuleMesh effectModuleMesh = effectData.modules[j] as EffectModuleMesh;
						if (effectModuleMesh != null && effectModuleMesh.CheckQualityLevel())
						{
							if (string.IsNullOrEmpty(effectModuleMesh.meshAddress))
							{
								Debug.LogWarning("EffectData: " + effectData.id + "'s effectModuleMesh meshAddress is null or empty. Has it not been set?");
							}
							else
							{
								EffectModuleMesh.GetOrCreateEffectMeshPoolManager(effectModuleMesh);
							}
						}
					}
				}
			}
			yield break;
		}

		// Token: 0x0600136D RID: 4973 RVA: 0x00089949 File Offset: 0x00087B49
		public static IEnumerator DespawnAllOutOfPool()
		{
			EffectModuleMesh.poolRoot = GameManager.poolTransform.Find("Meshes");
			if (!EffectModuleMesh.poolRoot)
			{
				EffectModuleMesh.poolRoot = new GameObject("Meshes").transform;
				EffectModuleMesh.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectMeshPoolManager effectMeshPoolManager in EffectModuleMesh.pools.Values)
			{
				effectMeshPoolManager.DespawnOutOfPool();
			}
			yield return null;
			yield break;
		}

		// Token: 0x0600136E RID: 4974 RVA: 0x00089951 File Offset: 0x00087B51
		public static IEnumerator ClearPool()
		{
			EffectModuleMesh.poolRoot = GameManager.poolTransform.Find("Meshes");
			if (!EffectModuleMesh.poolRoot)
			{
				EffectModuleMesh.poolRoot = new GameObject("Meshes").transform;
				EffectModuleMesh.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectMeshPoolManager effectMeshPoolManager in EffectModuleMesh.pools.Values)
			{
				effectMeshPoolManager.Dispose();
				yield return null;
			}
			Dictionary<EffectModuleMesh, EffectMeshPoolManager>.ValueCollection.Enumerator enumerator = default(Dictionary<EffectModuleMesh, EffectMeshPoolManager>.ValueCollection.Enumerator);
			EffectModuleMesh.pools.Clear();
			yield break;
			yield break;
		}

		// Token: 0x0600136F RID: 4975 RVA: 0x0008995C File Offset: 0x00087B5C
		public static void Despawn(EffectMesh effect)
		{
			try
			{
				if (effect.isPooled && EffectModuleMesh.poolRoot && effect.poolManager != null)
				{
					effect.poolManager.Release(effect);
				}
				else
				{
					if (effect.poolManager == null)
					{
						Debug.LogWarning("EffectMesh " + effect.name + " has no poolManager, but is marked as pooled!");
					}
					UnityEngine.Object.Destroy(effect.gameObject);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error despawning effect: {0}", e));
			}
		}

		// Token: 0x06001370 RID: 4976 RVA: 0x000899E8 File Offset: 0x00087BE8
		public override bool Spawn(EffectData effectData, Vector3 position, Quaternion rotation, out Effect effect, float intensity = 0f, float speed = 0f, Transform parent = null, CollisionInstance collisionInstance = null, bool pooled = true, ColliderGroup colliderGroup = null)
		{
			if (!base.Spawn(effectData, position, rotation, out effect, intensity, speed, parent, collisionInstance, pooled, colliderGroup))
			{
				return false;
			}
			if (!Catalog.gameData.platformParameters.enableEffectMesh || !this.mesh)
			{
				return false;
			}
			EffectMeshPoolManager poolManager = EffectModuleMesh.GetOrCreateEffectMeshPoolManager(this);
			if (poolManager == null)
			{
				Debug.LogWarning("Could not get an instance of an EffectMeshPoolManager for " + effectData.id + " - " + this.meshAddress);
				return false;
			}
			effect = ((pooled && GameManager.local.UsePooledEffectMesh) ? poolManager.Get() : this.CreateEffectMesh());
			if (effect == null)
			{
				Debug.LogWarning("No Meshes left available in the pool! for " + effectData.id + " - " + this.meshAddress);
				return false;
			}
			if (parent)
			{
				effect.transform.SetParent(parent, false);
			}
			else if (Level.current)
			{
				effect.transform.SetParent(Level.current.transform, false);
			}
			effect.transform.SetPositionAndRotation(position, rotation);
			effect.SetIntensity(intensity, false);
			effect.SetSpeed(speed, false);
			effect.gameObject.SetActive(true);
			LightVolumeReceiver lightVolumeReceiver = effect.lightVolumeReceiver;
			if (lightVolumeReceiver != null)
			{
				lightVolumeReceiver.UpdateRenderers();
			}
			return true;
		}

		/// <summary>
		/// Creates a configured, non pooled EffectMesh instance
		/// </summary>
		/// <returns></returns>
		// Token: 0x06001371 RID: 4977 RVA: 0x00089B30 File Offset: 0x00087D30
		public EffectMesh CreateEffectMesh()
		{
			GameObject gameObject = new GameObject("Mesh_" + this.meshAddress);
			gameObject.transform.SetParent(EffectModuleMesh.poolRoot);
			EffectMesh effectMesh = gameObject.AddComponent<EffectMesh>();
			effectMesh = this.Configure(effectMesh);
			effectMesh.isPooled = false;
			effectMesh.isOutOfPool = false;
			effectMesh.gameObject.SetActive(false);
			return effectMesh;
		}

		/// <summary>
		/// Configures an EffectMesh with values from this EffectModuleMesh
		/// </summary>
		/// <param name="effect"></param>
		/// <returns></returns>
		// Token: 0x06001372 RID: 4978 RVA: 0x00089B8C File Offset: 0x00087D8C
		public EffectMesh Configure(EffectMesh effect)
		{
			if (effect.lightVolumeReceiver == null)
			{
				effect.lightVolumeReceiver = effect.GetComponent<LightVolumeReceiver>();
			}
			effect.gameObject.SetActive(true);
			if (this.step == Effect.Step.Custom)
			{
				effect.stepCustomHashId = this.stepCustomIdHash;
			}
			effect.meshSize = this.localScale;
			effect.meshPosition = this.localPosition;
			effect.meshRotation = this.localRotation;
			if (effect.meshSizeFromIntensity)
			{
				float value = effect.curveMeshSize.Evaluate(0f);
				effect.transform.localScale = new Vector3(value, value, value);
			}
			else
			{
				effect.transform.localScale = this.localScale;
			}
			effect.meshFilter.mesh = this.mesh;
			int materialCount = this.materials.Count;
			Material[] meshMaterials = new Material[materialCount];
			for (int i = 0; i < materialCount; i++)
			{
				EffectModuleMesh.Materials j = this.materials[i];
				EffectModuleMesh.Materials.Material material = j as EffectModuleMesh.Materials.Material;
				if (material != null)
				{
					meshMaterials[i] = material.value;
				}
				else
				{
					Debug.LogError(string.Concat(new string[]
					{
						"EffectModuleMesh: ",
						this.meshAddress,
						" has null or invalid materials for material address: ",
						j.materialAddress,
						" "
					}));
				}
			}
			effect.meshRenderer.materials = meshMaterials;
			effect.meshSizeFromIntensity = this.useSizeCurve;
			effect.curveMeshSize = this.sizeCurve;
			effect.meshRotationFromIntensity = this.useRotationYCurve;
			effect.curveMeshrotY = this.rotationYCurve;
			effect.meshSizeFadeDuration = this.sizeFadeDuration;
			effect.meshRotationFadeDuration = this.rotationFadeDuration;
			int propCount = this.materialProperties.Count;
			for (int index = 0; index < propCount; index++)
			{
				EffectModuleMesh.MaterialProperty materialProperty = this.materialProperties[index];
				EffectModuleMesh.MaterialProperty.Float f = materialProperty as EffectModuleMesh.MaterialProperty.Float;
				if (f == null)
				{
					EffectModuleMesh.MaterialProperty.Int k = materialProperty as EffectModuleMesh.MaterialProperty.Int;
					if (k == null)
					{
						EffectModuleMesh.MaterialProperty.Vector vector = materialProperty as EffectModuleMesh.MaterialProperty.Vector;
						if (vector == null)
						{
							EffectModuleMesh.MaterialProperty.Color color = materialProperty as EffectModuleMesh.MaterialProperty.Color;
							if (color != null)
							{
								effect.materialPropertyBlock.SetColor(materialProperty.name, color.value);
							}
						}
						else
						{
							effect.materialPropertyBlock.SetVector(materialProperty.name, vector.value);
						}
					}
					else
					{
						effect.materialPropertyBlock.SetInt(materialProperty.name, k.value);
					}
				}
				else
				{
					effect.materialPropertyBlock.SetFloat(materialProperty.name, f.value);
				}
			}
			AnimationCurve animationCurve;
			if ((animationCurve = this.intensityCurve) == null)
			{
				animationCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 0f, 1f, 1f),
					new Keyframe(1f, 1f, 1f, 1f)
				});
			}
			effect.intensityCurve = animationCurve;
			effect.linkBaseColor = this.linkBaseColor;
			effect.linkTintColor = this.linkTintColor;
			effect.linkEmissionColor = this.linkEmissionColor;
			effect.lifeTime = this.lifeTime;
			effect.refreshSpeed = this.refreshSpeed;
			if ((QualitySettings.renderPipeline as UniversalRenderPipelineAsset).supportsHDR)
			{
				effect.SetMainGradient(this.mainGradient);
				effect.SetSecondaryGradient(this.secondaryGradient);
			}
			else
			{
				effect.SetMainGradient(this.mainGradientNoHdr);
				effect.SetSecondaryGradient(this.secondaryGradientNoHdr);
			}
			return effect;
		}

		// Token: 0x04001204 RID: 4612
		public new AnimationCurve intensityCurve;

		// Token: 0x04001205 RID: 4613
		public float lifeTime = 5f;

		// Token: 0x04001206 RID: 4614
		public float refreshSpeed = 0.1f;

		// Token: 0x04001207 RID: 4615
		public EffectTarget linkBaseColor;

		// Token: 0x04001208 RID: 4616
		public EffectTarget linkTintColor;

		// Token: 0x04001209 RID: 4617
		public EffectTarget linkEmissionColor;

		// Token: 0x0400120A RID: 4618
		[GradientUsage(true)]
		[NonSerialized]
		private Gradient mainGradient;

		// Token: 0x0400120B RID: 4619
		[GradientUsage(true)]
		[NonSerialized]
		private Gradient secondaryGradient;

		// Token: 0x0400120C RID: 4620
		[NonSerialized]
		private Gradient mainGradientNoHdr;

		// Token: 0x0400120D RID: 4621
		[NonSerialized]
		private Gradient secondaryGradientNoHdr;

		// Token: 0x0400120E RID: 4622
		[ColorUsage(true, true)]
		[SerializeField]
		public Color mainColorStart;

		// Token: 0x0400120F RID: 4623
		[ColorUsage(true, true)]
		[SerializeField]
		public Color mainColorEnd;

		// Token: 0x04001210 RID: 4624
		[ColorUsage(true, true)]
		[SerializeField]
		public Color secondaryColorStart;

		// Token: 0x04001211 RID: 4625
		[ColorUsage(true, true)]
		[SerializeField]
		public Color secondaryColorEnd;

		// Token: 0x04001212 RID: 4626
		[SerializeField]
		public Color mainNoHdrColorStart;

		// Token: 0x04001213 RID: 4627
		[SerializeField]
		public Color mainNoHdrColorEnd;

		// Token: 0x04001214 RID: 4628
		[SerializeField]
		public Color secondaryNoHdrColorStart;

		// Token: 0x04001215 RID: 4629
		[SerializeField]
		public Color secondaryNoHdrColorEnd;

		// Token: 0x04001216 RID: 4630
		public Vector3 localScale = Vector3.one;

		// Token: 0x04001217 RID: 4631
		public bool useSizeCurve;

		// Token: 0x04001218 RID: 4632
		public AnimationCurve sizeCurve;

		// Token: 0x04001219 RID: 4633
		public float sizeFadeDuration;

		// Token: 0x0400121A RID: 4634
		public Vector3 localPosition = Vector3.zero;

		// Token: 0x0400121B RID: 4635
		public Vector3 localRotation = Vector3.zero;

		// Token: 0x0400121C RID: 4636
		public bool useRotationYCurve;

		// Token: 0x0400121D RID: 4637
		public AnimationCurve rotationYCurve;

		// Token: 0x0400121E RID: 4638
		public float rotationFadeDuration;

		// Token: 0x0400121F RID: 4639
		public string meshAddress;

		// Token: 0x04001220 RID: 4640
		[NonSerialized]
		public Mesh mesh;

		// Token: 0x04001221 RID: 4641
		public List<EffectModuleMesh.Materials> materials = new List<EffectModuleMesh.Materials>();

		// Token: 0x04001222 RID: 4642
		public List<EffectModuleMesh.MaterialProperty> materialProperties = new List<EffectModuleMesh.MaterialProperty>();

		/// <summary>
		/// A pool of each EffectMesh objects for each effectModule configuration. 
		/// </summary>
		// Token: 0x04001223 RID: 4643
		public static Dictionary<EffectModuleMesh, EffectMeshPoolManager> pools = new Dictionary<EffectModuleMesh, EffectMeshPoolManager>();

		// Token: 0x04001224 RID: 4644
		public static Transform poolRoot;

		// Token: 0x0200079A RID: 1946
		public class Materials
		{
			// Token: 0x04003E7E RID: 15998
			public string materialAddress;

			// Token: 0x02000BC2 RID: 3010
			public class Material : EffectModuleMesh.Materials
			{
				// Token: 0x04004CCF RID: 19663
				[NonSerialized]
				public UnityEngine.Material value;
			}
		}

		// Token: 0x0200079B RID: 1947
		public class MaterialProperty
		{
			// Token: 0x04003E7F RID: 15999
			public string name;

			// Token: 0x02000BC3 RID: 3011
			[Serializable]
			public class Vector : EffectModuleMesh.MaterialProperty
			{
				// Token: 0x04004CD0 RID: 19664
				public Vector4 value;
			}

			// Token: 0x02000BC4 RID: 3012
			[Serializable]
			public class Float : EffectModuleMesh.MaterialProperty
			{
				// Token: 0x04004CD1 RID: 19665
				public float value;
			}

			// Token: 0x02000BC5 RID: 3013
			[Serializable]
			public class Int : EffectModuleMesh.MaterialProperty
			{
				// Token: 0x04004CD2 RID: 19666
				public int value;
			}

			// Token: 0x02000BC6 RID: 3014
			[Serializable]
			public class Color : EffectModuleMesh.MaterialProperty
			{
				// Token: 0x04004CD3 RID: 19667
				public UnityEngine.Color value;
			}
		}
	}
}
