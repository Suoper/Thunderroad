using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ThunderRoad
{
	// Token: 0x02000195 RID: 405
	public class EffectModuleParticle : EffectModule
	{
		// Token: 0x1700013D RID: 317
		// (get) Token: 0x06001376 RID: 4982 RVA: 0x00089F48 File Offset: 0x00088148
		// (set) Token: 0x06001377 RID: 4983 RVA: 0x0008A0E8 File Offset: 0x000882E8
		protected internal Gradient MainGradient
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

		// Token: 0x1700013E RID: 318
		// (get) Token: 0x06001378 RID: 4984 RVA: 0x0008A178 File Offset: 0x00088378
		// (set) Token: 0x06001379 RID: 4985 RVA: 0x0008A318 File Offset: 0x00088518
		protected internal Gradient SecondaryGradient
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

		// Token: 0x1700013F RID: 319
		// (get) Token: 0x0600137A RID: 4986 RVA: 0x0008A3A8 File Offset: 0x000885A8
		// (set) Token: 0x0600137B RID: 4987 RVA: 0x0008A548 File Offset: 0x00088748
		protected internal Gradient MainGradientNoHdr
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

		// Token: 0x17000140 RID: 320
		// (get) Token: 0x0600137C RID: 4988 RVA: 0x0008A5D8 File Offset: 0x000887D8
		// (set) Token: 0x0600137D RID: 4989 RVA: 0x0008A778 File Offset: 0x00088978
		protected internal Gradient SecondaryGradientNoHdr
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

		// Token: 0x0600137E RID: 4990 RVA: 0x0008A807 File Offset: 0x00088A07
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x0600137F RID: 4991 RVA: 0x0008A814 File Offset: 0x00088A14
		public override void OnCatalogRefresh(EffectData effectData, bool editorLoad = false)
		{
			base.OnCatalogRefresh(effectData, editorLoad);
			Gradient gradient = this.MainGradient;
			Gradient gradient2 = this.SecondaryGradient;
			Gradient gradient3 = this.MainGradientNoHdr;
			Gradient gradient4 = this.SecondaryGradientNoHdr;
		}

		// Token: 0x06001380 RID: 4992 RVA: 0x0008A83A File Offset: 0x00088A3A
		public override IEnumerator RefreshCoroutine(EffectData effectData, bool editorLoad = false)
		{
			if (editorLoad)
			{
				GameObject gameObject = base.EditorLoad<GameObject>(this.effectParticleAddress);
				this.effectParticlePrefab = ((gameObject != null) ? gameObject.GetComponent<EffectParticle>() : null);
				yield return null;
			}
			else
			{
				if (this.effectParticlePrefab && !EffectModuleParticle.pools.IsNullOrEmpty())
				{
					Catalog.ReleaseAsset<EffectParticle>(this.effectParticlePrefab);
				}
				yield return Catalog.LoadAssetCoroutine<GameObject>(this.effectParticleAddress, delegate(GameObject value)
				{
					this.effectParticlePrefab = ((value != null) ? value.GetComponent<EffectParticle>() : null);
				}, effectData.id + " (EffectModuleParticle)");
			}
			this.collisionEffectData = Catalog.GetData<EffectData>(this.collisionEffectId, true);
			yield break;
		}

		// Token: 0x06001381 RID: 4993 RVA: 0x0008A857 File Offset: 0x00088A57
		public override void CopyHDRToNonHDR()
		{
			this.mainGradientNoHdr = this.mainGradient;
			this.secondaryGradientNoHdr = this.secondaryGradient;
		}

		// Token: 0x06001382 RID: 4994 RVA: 0x0008A874 File Offset: 0x00088A74
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

		/// <summary>
		/// This will get or create the EffectParticlePoolManager for this EffectModuleParticle
		/// </summary>
		// Token: 0x06001383 RID: 4995 RVA: 0x0008AB28 File Offset: 0x00088D28
		public static EffectParticlePoolManager GetOrCreateEffectParticlePoolManager(EffectModuleParticle effectModuleParticle)
		{
			EffectParticlePoolManager poolManager;
			if (EffectModuleParticle.pools.TryGetValue(effectModuleParticle, out poolManager))
			{
				return poolManager;
			}
			poolManager = new EffectParticlePoolManager(EffectModuleParticle.poolRoot, effectModuleParticle, PoolType.Stack, GameManager.local.PoolCollectionChecks, effectModuleParticle.effectParticlePrefab.poolCount, 1, true);
			EffectModuleParticle.pools.Add(effectModuleParticle, poolManager);
			return poolManager;
		}

		// Token: 0x06001384 RID: 4996 RVA: 0x0008AB77 File Offset: 0x00088D77
		public static IEnumerator GeneratePool()
		{
			yield return EffectModuleParticle.ClearPool();
			if (!Catalog.gameData.platformParameters.enableEffectParticle)
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
						EffectModuleParticle effectModuleParticle = effectData.modules[j] as EffectModuleParticle;
						if (effectModuleParticle != null && effectModuleParticle.CheckQualityLevel())
						{
							if (effectModuleParticle.effectParticlePrefab == null)
							{
								Debug.LogWarning(string.Concat(new string[]
								{
									"EffectData: ",
									effectData.id,
									"'s effectModuleParticle: ",
									effectModuleParticle.effectParticleAddress,
									" has a null effectParticlePrefab. Did it not get loaded?"
								}));
							}
							else
							{
								EffectModuleParticle.GetOrCreateEffectParticlePoolManager(effectModuleParticle);
							}
						}
					}
				}
			}
			yield break;
		}

		// Token: 0x06001385 RID: 4997 RVA: 0x0008AB7F File Offset: 0x00088D7F
		public static IEnumerator DespawnAllOutOfPool()
		{
			EffectModuleParticle.poolRoot = GameManager.poolTransform.Find("Particles");
			if (!EffectModuleParticle.poolRoot)
			{
				EffectModuleParticle.poolRoot = new GameObject("Particles").transform;
				EffectModuleParticle.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectParticlePoolManager effectParticlePoolManager in EffectModuleParticle.pools.Values)
			{
				effectParticlePoolManager.DespawnOutOfPool();
			}
			yield return null;
			yield break;
		}

		// Token: 0x06001386 RID: 4998 RVA: 0x0008AB87 File Offset: 0x00088D87
		public static IEnumerator ClearPool()
		{
			EffectModuleParticle.poolRoot = GameManager.poolTransform.Find("Particles");
			if (!EffectModuleParticle.poolRoot)
			{
				EffectModuleParticle.poolRoot = new GameObject("Particles").transform;
				EffectModuleParticle.poolRoot.SetParent(GameManager.poolTransform, false);
			}
			foreach (EffectParticlePoolManager effectParticlePoolManager in EffectModuleParticle.pools.Values)
			{
				effectParticlePoolManager.Dispose();
				yield return null;
			}
			Dictionary<EffectModuleParticle, EffectParticlePoolManager>.ValueCollection.Enumerator enumerator = default(Dictionary<EffectModuleParticle, EffectParticlePoolManager>.ValueCollection.Enumerator);
			EffectModuleParticle.pools.Clear();
			yield break;
			yield break;
		}

		// Token: 0x06001387 RID: 4999 RVA: 0x0008AB90 File Offset: 0x00088D90
		public static void Despawn(EffectParticle effect)
		{
			try
			{
				if (effect.isPooled && EffectModuleParticle.poolRoot && effect.poolManager != null)
				{
					effect.poolManager.Release(effect);
				}
				else
				{
					if (effect.poolManager == null && effect.isPooled)
					{
						Debug.LogWarning("EffectParticle " + effect.name + " has no poolManager, but is marked as pooled! If its using a FXController, remove EffectParticle scripts");
					}
					UnityEngine.Object.Destroy(effect.gameObject);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error despawning effect: {0}", e));
			}
		}

		// Token: 0x06001388 RID: 5000 RVA: 0x0008AC24 File Offset: 0x00088E24
		public override bool Spawn(EffectData effectData, Vector3 position, Quaternion rotation, out Effect effect, float intensity = 0f, float speed = 0f, Transform parent = null, CollisionInstance collisionInstance = null, bool pooled = true, ColliderGroup colliderGroup = null)
		{
			if (!base.Spawn(effectData, position, rotation, out effect, intensity, speed, parent, collisionInstance, pooled, colliderGroup))
			{
				return false;
			}
			if (!Catalog.gameData.platformParameters.enableEffectParticle || !this.effectParticlePrefab)
			{
				return false;
			}
			Effect.Step step = this.step;
			if (step == Effect.Step.Start || step == Effect.Step.End)
			{
				EffectLink effectLink = this.effectLink;
				if (effectLink != EffectLink.Intensity)
				{
					if (effectLink != EffectLink.Speed)
					{
						goto IL_74;
					}
					if (speed >= this.cullMinSpeed)
					{
						goto IL_74;
					}
				}
				else if (intensity >= this.cullMinIntensity)
				{
					goto IL_74;
				}
				return false;
				IL_74:
				if (Player.local)
				{
					Transform transform = Player.local.head.transform;
					Vector3 headPos = transform.position;
					Vector3 headDir = transform.forward;
					Vector3 dirToEffect = position - headPos;
					float dotProduct = Vector3.Dot(dirToEffect.normalized, headDir);
					if (dirToEffect.sqrMagnitude > this.insideParticleRadius * this.insideParticleRadius && dotProduct < 0f)
					{
						return false;
					}
				}
			}
			EffectParticlePoolManager poolManager = EffectModuleParticle.GetOrCreateEffectParticlePoolManager(this);
			if (poolManager == null)
			{
				Debug.LogWarning("Could not get an instance of an EffectParticlePoolManager for " + effectData.id + " - " + this.effectParticleAddress);
				return false;
			}
			effect = ((pooled && GameManager.local.UsePooledEffectParticle) ? poolManager.Get() : this.CreateEffectParticle());
			if (effect == null)
			{
				Debug.LogWarning("No particles left available in the pool! for " + effectData.id + " - " + this.effectParticleAddress);
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
			effect.transform.SetPositionAndRotation(position, rotation * Quaternion.Euler(this.localRotation));
			effect.gameObject.SetActive(true);
			effect.SetIntensity(intensity, false);
			effect.SetSpeed(speed, false);
			LightVolumeReceiver lightVolumeReceiver = effect.lightVolumeReceiver;
			if (lightVolumeReceiver != null)
			{
				lightVolumeReceiver.UpdateRenderers();
			}
			return true;
		}

		/// <summary>
		/// Creates an configured, non pooled EffectParticle instance
		/// </summary>
		/// <returns></returns>
		// Token: 0x06001389 RID: 5001 RVA: 0x0008AE18 File Offset: 0x00089018
		public EffectParticle CreateEffectParticle()
		{
			EffectParticle effectParticle = UnityEngine.Object.Instantiate<EffectParticle>(this.effectParticlePrefab, EffectModuleParticle.poolRoot);
			effectParticle = this.Configure(effectParticle);
			effectParticle.isPooled = false;
			effectParticle.isOutOfPool = false;
			effectParticle.gameObject.SetActive(false);
			return effectParticle;
		}

		/// <summary>
		/// Configures an effectParticle with values from this EffectModuleParticle
		/// </summary>
		/// <param name="effectParticle"></param>
		/// <returns></returns>
		// Token: 0x0600138A RID: 5002 RVA: 0x0008AE5C File Offset: 0x0008905C
		public EffectParticle Configure(EffectParticle effectParticle)
		{
			if (effectParticle.lightVolumeReceiver == null)
			{
				effectParticle.lightVolumeReceiver = effectParticle.GetComponent<LightVolumeReceiver>();
			}
			if (this.step == Effect.Step.Custom)
			{
				effectParticle.stepCustomHashId = this.stepCustomIdHash;
			}
			effectParticle.loopCustomStep = this.loopCustomStep;
			effectParticle.transform.localScale = this.localScale;
			effectParticle.useScaleCurve = this.useScaleCurve;
			effectParticle.scaleCurve = this.scaleCurve;
			effectParticle.renderInLateUpdate = this.renderInLateUpdate;
			effectParticle.effectLink = this.effectLink;
			AnimationCurve animationCurve;
			if ((animationCurve = this.intensityCurve) == null)
			{
				animationCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 0f, 1f, 1f),
					new Keyframe(1f, 1f, 1f, 1f)
				});
			}
			effectParticle.intensityCurve = animationCurve;
			foreach (EffectParticleChild effectParticleChild in effectParticle.childs)
			{
				if (effectParticleChild.emitEffectOnCollision)
				{
					if (this.collisionEffectData != null)
					{
						effectParticleChild.particleCollisionSpawner.effectData = this.collisionEffectData;
						effectParticleChild.particleCollisionSpawner.active = true;
					}
					else
					{
						effectParticleChild.particleCollisionSpawner.effectData = null;
						effectParticleChild.particleCollisionSpawner.active = false;
					}
				}
			}
			if ((QualitySettings.renderPipeline as UniversalRenderPipelineAsset).supportsHDR)
			{
				effectParticle.SetMainGradient(this.mainGradient);
				effectParticle.SetSecondaryGradient(this.secondaryGradient);
			}
			else
			{
				effectParticle.SetMainGradient(this.mainGradientNoHdr);
				effectParticle.SetSecondaryGradient(this.secondaryGradientNoHdr);
			}
			return effectParticle;
		}

		// Token: 0x04001225 RID: 4645
		public EffectLink effectLink;

		// Token: 0x04001226 RID: 4646
		public float insideParticleRadius;

		// Token: 0x04001227 RID: 4647
		public float cullMinIntensity = 0.05f;

		// Token: 0x04001228 RID: 4648
		public float cullMinSpeed = 0.05f;

		// Token: 0x04001229 RID: 4649
		public string effectParticleAddress;

		// Token: 0x0400122A RID: 4650
		[NonSerialized]
		public EffectParticle effectParticlePrefab;

		// Token: 0x0400122B RID: 4651
		public new AnimationCurve intensityCurve;

		// Token: 0x0400122C RID: 4652
		public bool renderInLateUpdate;

		// Token: 0x0400122D RID: 4653
		[GradientUsage(true)]
		[NonSerialized]
		private Gradient mainGradient;

		// Token: 0x0400122E RID: 4654
		[GradientUsage(true)]
		[NonSerialized]
		private Gradient secondaryGradient;

		// Token: 0x0400122F RID: 4655
		[NonSerialized]
		private Gradient mainGradientNoHdr;

		// Token: 0x04001230 RID: 4656
		[NonSerialized]
		private Gradient secondaryGradientNoHdr;

		// Token: 0x04001231 RID: 4657
		[ColorUsage(true, true)]
		[SerializeField]
		public Color mainColorStart;

		// Token: 0x04001232 RID: 4658
		[ColorUsage(true, true)]
		[SerializeField]
		public Color mainColorEnd;

		// Token: 0x04001233 RID: 4659
		[ColorUsage(true, true)]
		[SerializeField]
		public Color secondaryColorStart;

		// Token: 0x04001234 RID: 4660
		[ColorUsage(true, true)]
		[SerializeField]
		public Color secondaryColorEnd;

		// Token: 0x04001235 RID: 4661
		[SerializeField]
		public Color mainNoHdrColorStart;

		// Token: 0x04001236 RID: 4662
		[SerializeField]
		public Color mainNoHdrColorEnd;

		// Token: 0x04001237 RID: 4663
		[SerializeField]
		public Color secondaryNoHdrColorStart;

		// Token: 0x04001238 RID: 4664
		[SerializeField]
		public Color secondaryNoHdrColorEnd;

		// Token: 0x04001239 RID: 4665
		public Vector3 localScale = Vector3.one;

		// Token: 0x0400123A RID: 4666
		public bool useScaleCurve;

		// Token: 0x0400123B RID: 4667
		public AnimationCurve scaleCurve;

		// Token: 0x0400123C RID: 4668
		public Vector3 localRotation = Vector3.zero;

		// Token: 0x0400123D RID: 4669
		public string collisionEffectId;

		// Token: 0x0400123E RID: 4670
		[NonSerialized]
		public EffectData collisionEffectData;

		// Token: 0x0400123F RID: 4671
		public LayerMask collisionLayerMask = -1;

		// Token: 0x04001240 RID: 4672
		public float collisionMaxGroundAngle = 45f;

		// Token: 0x04001241 RID: 4673
		public float collisionEmitRate = 0.2f;

		// Token: 0x04001242 RID: 4674
		public float collisionMinIntensity;

		// Token: 0x04001243 RID: 4675
		public float collisionMaxIntensity = 1f;

		// Token: 0x04001244 RID: 4676
		public bool collisionUseMainGradient;

		// Token: 0x04001245 RID: 4677
		public bool collisionUseSecondaryGradient;

		/// <summary>
		/// A pool of each EffectParticle objects for each effectModule configuration. 
		/// </summary>
		// Token: 0x04001246 RID: 4678
		public static Dictionary<EffectModuleParticle, EffectParticlePoolManager> pools = new Dictionary<EffectModuleParticle, EffectParticlePoolManager>();

		// Token: 0x04001247 RID: 4679
		public static Transform poolRoot;
	}
}
