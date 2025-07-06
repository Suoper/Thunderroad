using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace ThunderRoad
{
	// Token: 0x02000199 RID: 409
	public class EffectModuleVfx : EffectModule
	{
		// Token: 0x060013AD RID: 5037 RVA: 0x0008C6F4 File Offset: 0x0008A8F4
		protected List<ValueDropdownItem<LayerName>> LayerOverrideOptions()
		{
			List<ValueDropdownItem<LayerName>> options = new List<ValueDropdownItem<LayerName>>();
			foreach (LayerName lName in (LayerName[])Enum.GetValues(typeof(LayerName)))
			{
				string text = (lName == LayerName.None) ? "Don't override" : ("Override: " + lName.ToString());
				options.Add(new ValueDropdownItem<LayerName>(text, lName));
			}
			return options;
		}

		// Token: 0x17000145 RID: 325
		// (get) Token: 0x060013AE RID: 5038 RVA: 0x0008C760 File Offset: 0x0008A960
		// (set) Token: 0x060013AF RID: 5039 RVA: 0x0008C900 File Offset: 0x0008AB00
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

		// Token: 0x17000146 RID: 326
		// (get) Token: 0x060013B0 RID: 5040 RVA: 0x0008C990 File Offset: 0x0008AB90
		// (set) Token: 0x060013B1 RID: 5041 RVA: 0x0008CB30 File Offset: 0x0008AD30
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

		// Token: 0x17000147 RID: 327
		// (get) Token: 0x060013B2 RID: 5042 RVA: 0x0008CBC0 File Offset: 0x0008ADC0
		// (set) Token: 0x060013B3 RID: 5043 RVA: 0x0008CD60 File Offset: 0x0008AF60
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

		// Token: 0x17000148 RID: 328
		// (get) Token: 0x060013B4 RID: 5044 RVA: 0x0008CDF0 File Offset: 0x0008AFF0
		// (set) Token: 0x060013B5 RID: 5045 RVA: 0x0008CF90 File Offset: 0x0008B190
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

		// Token: 0x060013B6 RID: 5046 RVA: 0x0008D01F File Offset: 0x0008B21F
		public override void OnCatalogRefresh(EffectData effectData, bool editorLoad = false)
		{
			base.OnCatalogRefresh(effectData, editorLoad);
			Gradient gradient = this.MainGradient;
			Gradient gradient2 = this.SecondaryGradient;
			Gradient gradient3 = this.MainGradientNoHdr;
			Gradient gradient4 = this.SecondaryGradientNoHdr;
		}

		// Token: 0x060013B7 RID: 5047 RVA: 0x0008D045 File Offset: 0x0008B245
		public override IEnumerator RefreshCoroutine(EffectData effectData, bool editorLoad = false)
		{
			if (this.vfxAsset)
			{
				Catalog.ReleaseAsset<VisualEffectAsset>(this.vfxAsset);
			}
			yield return Catalog.LoadAssetCoroutine<VisualEffectAsset>(this.vfxAddress, delegate(VisualEffectAsset value)
			{
				this.vfxAsset = value;
			}, effectData.id + " (EffectModuleVfx)");
			if (this.meshAsset)
			{
				Catalog.ReleaseAsset<Mesh>(this.meshAsset);
			}
			yield return Catalog.LoadAssetCoroutine<Mesh>(this.meshAddress, delegate(Mesh value)
			{
				this.meshAsset = value;
			}, effectData.id + " (EffectModuleVfx)");
			yield break;
		}

		// Token: 0x060013B8 RID: 5048 RVA: 0x0008D05C File Offset: 0x0008B25C
		public override void Clean()
		{
			if (this.intensityCurve != null && this.intensityCurve.keys.Length == 0)
			{
				this.intensityCurve = null;
			}
			if (this.scaleCurve != null && this.scaleCurve.keys.Length == 0)
			{
				this.scaleCurve = null;
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

		// Token: 0x060013B9 RID: 5049 RVA: 0x0008D30F File Offset: 0x0008B50F
		public override void CopyHDRToNonHDR()
		{
			this.mainGradientNoHdr = this.mainGradient;
			this.secondaryGradientNoHdr = this.secondaryGradient;
		}

		/// <summary>
		/// This will get or create the EffectAudioPoolContainer for a particular effect and audioClip from this EffectModuleAudios audioContainer
		/// </summary>
		// Token: 0x060013BA RID: 5050 RVA: 0x0008D32C File Offset: 0x0008B52C
		public static EffectVfxPoolManager GetOrCreateEffectVfxPoolManager(EffectModuleVfx effectModuleVfx)
		{
			EffectVfxPoolManager poolManager;
			if (EffectModuleVfx.pools.TryGetValue(effectModuleVfx, out poolManager))
			{
				return poolManager;
			}
			poolManager = new EffectVfxPoolManager(EffectModuleVfx.poolRoot, effectModuleVfx, PoolType.Stack, GameManager.local.PoolCollectionChecks, Catalog.gameData.platformParameters.poolingVfxCount, Catalog.gameData.platformParameters.poolingVfxCount / 2, true);
			EffectModuleVfx.pools.Add(effectModuleVfx, poolManager);
			return poolManager;
		}

		// Token: 0x060013BB RID: 5051 RVA: 0x0008D38F File Offset: 0x0008B58F
		public static IEnumerator GeneratePool()
		{
			yield return EffectModuleVfx.ClearPool();
			if (!Catalog.gameData.platformParameters.enableEffectVfx)
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
						EffectModuleVfx effectModuleVfx = effectData.modules[j] as EffectModuleVfx;
						if (effectModuleVfx != null && effectModuleVfx.CheckQualityLevel())
						{
							if (string.IsNullOrEmpty(effectModuleVfx.vfxAddress) && string.IsNullOrEmpty(effectModuleVfx.meshAddress))
							{
								Debug.LogWarning("EffectData: " + effectData.id + "'s effectModuleVfx does not have a valid vfxAddress or meshAddress.");
							}
							else
							{
								EffectModuleVfx.GetOrCreateEffectVfxPoolManager(effectModuleVfx);
							}
						}
					}
				}
			}
			yield break;
		}

		// Token: 0x060013BC RID: 5052 RVA: 0x0008D397 File Offset: 0x0008B597
		public static IEnumerator DespawnAllOutOfPool()
		{
			EffectModuleVfx.poolRoot = GameManager.poolTransform.Find("Vfx");
			if (!EffectModuleVfx.poolRoot)
			{
				EffectModuleVfx.poolRoot = new GameObject("Vfx").transform;
				EffectModuleVfx.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectVfxPoolManager effectVfxPoolManager in EffectModuleVfx.pools.Values)
			{
				effectVfxPoolManager.DespawnOutOfPool();
			}
			yield return null;
			yield break;
		}

		// Token: 0x060013BD RID: 5053 RVA: 0x0008D39F File Offset: 0x0008B59F
		public static IEnumerator ClearPool()
		{
			EffectModuleVfx.poolRoot = GameManager.poolTransform.Find("Vfx");
			if (!EffectModuleVfx.poolRoot)
			{
				EffectModuleVfx.poolRoot = new GameObject("Vfx").transform;
				EffectModuleVfx.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectVfxPoolManager effectVfxPoolManager in EffectModuleVfx.pools.Values)
			{
				effectVfxPoolManager.Dispose();
				yield return null;
			}
			Dictionary<EffectModuleVfx, EffectVfxPoolManager>.ValueCollection.Enumerator enumerator = default(Dictionary<EffectModuleVfx, EffectVfxPoolManager>.ValueCollection.Enumerator);
			EffectModuleVfx.pools.Clear();
			yield break;
			yield break;
		}

		// Token: 0x060013BE RID: 5054 RVA: 0x0008D3A8 File Offset: 0x0008B5A8
		public static void Despawn(EffectVfx effect)
		{
			try
			{
				if (effect.isPooled && EffectModuleVfx.poolRoot && effect.poolManager != null)
				{
					effect.poolManager.Release(effect);
				}
				else
				{
					if (effect.poolManager == null)
					{
						Debug.LogWarning("EffectVfx " + effect.name + " has no poolManager, but is marked as pooled!");
					}
					UnityEngine.Object.Destroy(effect.gameObject);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error despawning effect: {0}", e));
			}
		}

		// Token: 0x060013BF RID: 5055 RVA: 0x0008D434 File Offset: 0x0008B634
		public override bool Spawn(EffectData effectData, Vector3 position, Quaternion rotation, out Effect effect, float intensity = 0f, float speed = 0f, Transform parent = null, CollisionInstance collisionInstance = null, bool pooled = true, ColliderGroup colliderGroup = null)
		{
			if (!base.Spawn(effectData, position, rotation, out effect, intensity, speed, parent, collisionInstance, pooled, colliderGroup))
			{
				return false;
			}
			if (!Catalog.gameData.platformParameters.enableEffectVfx || !this.vfxAsset)
			{
				return false;
			}
			EffectVfxPoolManager poolManager = EffectModuleVfx.GetOrCreateEffectVfxPoolManager(this);
			if (poolManager == null)
			{
				Debug.LogWarning("Could not get an instance of an EffectVfxPoolManager for " + effectData.id + " - " + this.vfxAddress);
				return false;
			}
			effect = ((pooled && GameManager.local.UsePooledEffectVfx) ? poolManager.Get() : this.CreateEffectVfx());
			if (effect == null)
			{
				Debug.LogWarning("No VFXs left available in the pool! for " + effectData.id + " - " + this.vfxAddress);
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
			if (this.layerOverride != LayerName.None)
			{
				effect.gameObject.SetLayerRecursively(Common.GetLayer(this.layerOverride));
			}
			return true;
		}

		/// <summary>
		/// Creates an configured, non pooled EffectVfx instance
		/// </summary>
		/// <returns></returns>
		// Token: 0x060013C0 RID: 5056 RVA: 0x0008D59C File Offset: 0x0008B79C
		public EffectVfx CreateEffectVfx()
		{
			GameObject gameObject = new GameObject("Vfx-" + this.vfxAddress);
			gameObject.transform.SetParent(EffectModuleVfx.poolRoot);
			EffectVfx effectVfx = gameObject.AddComponent<EffectVfx>();
			effectVfx = this.Configure(effectVfx);
			effectVfx.isPooled = false;
			effectVfx.isOutOfPool = false;
			effectVfx.gameObject.SetActive(false);
			return effectVfx;
		}

		/// <summary>
		/// Configures an EffectVFX with values from this EffectModuleVfx
		/// </summary>
		/// <param name="effectVfx"></param>
		/// <returns></returns>
		// Token: 0x060013C1 RID: 5057 RVA: 0x0008D5F8 File Offset: 0x0008B7F8
		public EffectVfx Configure(EffectVfx effectVfx)
		{
			if (effectVfx.lightVolumeReceiver == null)
			{
				effectVfx.lightVolumeReceiver = effectVfx.GetComponent<LightVolumeReceiver>();
			}
			if (this.step == Effect.Step.Custom)
			{
				effectVfx.stepCustomHashId = this.stepCustomIdHash;
			}
			effectVfx.transform.localScale = this.localScale;
			effectVfx.useScaleCurve = this.useScaleCurve;
			effectVfx.scaleCurve = this.scaleCurve;
			AnimationCurve animationCurve;
			if ((animationCurve = this.intensityCurve) == null)
			{
				animationCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 0f, 1f, 1f),
					new Keyframe(1f, 1f, 1f, 1f)
				});
			}
			effectVfx.intensityCurve = animationCurve;
			effectVfx.lifeTime = this.lifeTime;
			effectVfx.despawnOnEnd = this.despawnOnEnd;
			effectVfx.despawnDelay = this.despawnDelay;
			effectVfx.lookAtTarget = this.lookAtTarget;
			effectVfx.spawnOn = this.spawnOn;
			effectVfx.vfx.visualEffectAsset = this.vfxAsset;
			effectVfx.usePointCache = this.usePointCache;
			effectVfx.useSecondaryRenderer = this.useSecondaryRenderer;
			if (this.usePointCache)
			{
				effectVfx.pointCacheSkinnedMeshUpdate = this.pointCacheSkinnedMeshUpdate;
				effectVfx.pointCacheMapSize = this.pointCacheMapSize;
				effectVfx.pointCachePointCount = this.pointCachePointCount;
				effectVfx.pointCacheSeed = this.pointCacheSeed;
				effectVfx.pointCacheBakeMode = this.pointCacheBakeMode;
				effectVfx.pointCacheDistribution = this.pointCacheDistribution;
			}
			for (int index = 0; index < this.materialProperties.Count; index++)
			{
				EffectModuleVfx.VfxProperty vfxProperty = this.materialProperties[index];
				EffectModuleVfx.VfxProperty.Float f = vfxProperty as EffectModuleVfx.VfxProperty.Float;
				if (f == null)
				{
					EffectModuleVfx.VfxProperty.Int i = vfxProperty as EffectModuleVfx.VfxProperty.Int;
					if (i == null)
					{
						EffectModuleVfx.VfxProperty.Vector2 vector2 = vfxProperty as EffectModuleVfx.VfxProperty.Vector2;
						if (vector2 == null)
						{
							EffectModuleVfx.VfxProperty.Vector3 vector3 = vfxProperty as EffectModuleVfx.VfxProperty.Vector3;
							if (vector3 != null)
							{
								if (effectVfx.vfx.HasVector3(vfxProperty.name))
								{
									effectVfx.vfx.SetVector3(vfxProperty.name, vector3.value);
								}
							}
						}
						else if (effectVfx.vfx.HasVector2(vfxProperty.name))
						{
							effectVfx.vfx.SetVector2(vfxProperty.name, vector2.value);
						}
					}
					else if (effectVfx.vfx.HasInt(vfxProperty.name))
					{
						effectVfx.vfx.SetInt(vfxProperty.name, i.value);
					}
				}
				else if (effectVfx.vfx.HasFloat(vfxProperty.name))
				{
					effectVfx.vfx.SetFloat(vfxProperty.name, f.value);
				}
			}
			if (this.meshAsset)
			{
				effectVfx.SetMesh(this.meshAsset);
			}
			if ((QualitySettings.renderPipeline as UniversalRenderPipelineAsset).supportsHDR)
			{
				effectVfx.SetMainGradient(this.mainGradient);
				effectVfx.SetSecondaryGradient(this.secondaryGradient);
			}
			else
			{
				effectVfx.SetMainGradient(this.mainGradientNoHdr);
				effectVfx.SetSecondaryGradient(this.secondaryGradientNoHdr);
			}
			return effectVfx;
		}

		// Token: 0x04001273 RID: 4723
		public string vfxAddress;

		// Token: 0x04001274 RID: 4724
		[NonSerialized]
		public VisualEffectAsset vfxAsset;

		// Token: 0x04001275 RID: 4725
		public bool despawnOnEnd;

		// Token: 0x04001276 RID: 4726
		public float despawnDelay;

		// Token: 0x04001277 RID: 4727
		public new AnimationCurve intensityCurve;

		// Token: 0x04001278 RID: 4728
		public float lifeTime = 5f;

		// Token: 0x04001279 RID: 4729
		public bool useSecondaryRenderer;

		// Token: 0x0400127A RID: 4730
		public bool lookAtTarget;

		// Token: 0x0400127B RID: 4731
		public SpawnTarget spawnOn;

		// Token: 0x0400127C RID: 4732
		public LayerName layerOverride;

		// Token: 0x0400127D RID: 4733
		[GradientUsage(true)]
		[NonSerialized]
		private Gradient mainGradient;

		// Token: 0x0400127E RID: 4734
		[GradientUsage(true)]
		[NonSerialized]
		private Gradient secondaryGradient;

		// Token: 0x0400127F RID: 4735
		[NonSerialized]
		private Gradient mainGradientNoHdr;

		// Token: 0x04001280 RID: 4736
		[NonSerialized]
		private Gradient secondaryGradientNoHdr;

		// Token: 0x04001281 RID: 4737
		[ColorUsage(true, true)]
		[SerializeField]
		public Color mainColorStart;

		// Token: 0x04001282 RID: 4738
		[ColorUsage(true, true)]
		[SerializeField]
		public Color mainColorEnd;

		// Token: 0x04001283 RID: 4739
		[ColorUsage(true, true)]
		[SerializeField]
		public Color secondaryColorStart;

		// Token: 0x04001284 RID: 4740
		[ColorUsage(true, true)]
		[SerializeField]
		public Color secondaryColorEnd;

		// Token: 0x04001285 RID: 4741
		[SerializeField]
		public Color mainNoHdrColorStart;

		// Token: 0x04001286 RID: 4742
		[SerializeField]
		public Color mainNoHdrColorEnd;

		// Token: 0x04001287 RID: 4743
		[SerializeField]
		public Color secondaryNoHdrColorStart;

		// Token: 0x04001288 RID: 4744
		[SerializeField]
		public Color secondaryNoHdrColorEnd;

		// Token: 0x04001289 RID: 4745
		public Vector3 localScale = Vector3.one;

		// Token: 0x0400128A RID: 4746
		public bool useScaleCurve;

		// Token: 0x0400128B RID: 4747
		public AnimationCurve scaleCurve;

		// Token: 0x0400128C RID: 4748
		public bool usePointCache;

		// Token: 0x0400128D RID: 4749
		public bool pointCacheSkinnedMeshUpdate;

		// Token: 0x0400128E RID: 4750
		public int pointCacheMapSize = 512;

		// Token: 0x0400128F RID: 4751
		public int pointCachePointCount = 4096;

		// Token: 0x04001290 RID: 4752
		public int pointCacheSeed;

		// Token: 0x04001291 RID: 4753
		public PointCacheGenerator.Distribution pointCacheDistribution = PointCacheGenerator.Distribution.RandomUniformArea;

		// Token: 0x04001292 RID: 4754
		public PointCacheGenerator.MeshBakeMode pointCacheBakeMode = PointCacheGenerator.MeshBakeMode.Triangle;

		// Token: 0x04001293 RID: 4755
		public bool useMesh;

		// Token: 0x04001294 RID: 4756
		public string meshAddress;

		// Token: 0x04001295 RID: 4757
		[NonSerialized]
		public Mesh meshAsset;

		// Token: 0x04001296 RID: 4758
		public List<EffectModuleVfx.VfxProperty> materialProperties = new List<EffectModuleVfx.VfxProperty>();

		/// <summary>
		/// A pool of each EffectVfx objects for each effectModule configuration. 
		/// </summary>
		// Token: 0x04001297 RID: 4759
		public static Dictionary<EffectModuleVfx, EffectVfxPoolManager> pools = new Dictionary<EffectModuleVfx, EffectVfxPoolManager>();

		// Token: 0x04001298 RID: 4760
		public static Transform poolRoot;

		// Token: 0x020007AD RID: 1965
		[Serializable]
		public class VfxProperty
		{
			// Token: 0x04003EB4 RID: 16052
			public string name;

			// Token: 0x02000BC7 RID: 3015
			public class Vector2 : EffectModuleVfx.VfxProperty
			{
				// Token: 0x04004CD4 RID: 19668
				public UnityEngine.Vector2 value;
			}

			// Token: 0x02000BC8 RID: 3016
			public class Vector3 : EffectModuleVfx.VfxProperty
			{
				// Token: 0x04004CD5 RID: 19669
				public UnityEngine.Vector3 value;
			}

			// Token: 0x02000BC9 RID: 3017
			public class Float : EffectModuleVfx.VfxProperty
			{
				// Token: 0x04004CD6 RID: 19670
				public float value;
			}

			// Token: 0x02000BCA RID: 3018
			public class Int : EffectModuleVfx.VfxProperty
			{
				// Token: 0x04004CD7 RID: 19671
				public int value;
			}

			// Token: 0x02000BCB RID: 3019
			public class Gradient : EffectModuleVfx.VfxProperty
			{
				// Token: 0x04004CD8 RID: 19672
				public UnityEngine.Gradient value;
			}
		}
	}
}
