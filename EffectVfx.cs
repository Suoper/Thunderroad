using System;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace ThunderRoad
{
	// Token: 0x02000290 RID: 656
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectVfx.html")]
	[ExecuteInEditMode]
	public class EffectVfx : Effect
	{
		// Token: 0x06001EE9 RID: 7913 RVA: 0x000D29B2 File Offset: 0x000D0BB2
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.vfx = base.GetComponent<VisualEffect>();
			this.SetTarget(this.targetTransform);
		}

		// Token: 0x06001EEA RID: 7914 RVA: 0x000D29DA File Offset: 0x000D0BDA
		private void Awake()
		{
			if (!base.TryGetComponent<VisualEffect>(out this.vfx))
			{
				this.vfx = base.gameObject.AddComponent<VisualEffect>();
			}
			this.vfx.enabled = false;
		}

		// Token: 0x06001EEB RID: 7915 RVA: 0x000D2A08 File Offset: 0x000D0C08
		public override void Play()
		{
			this.vfx.enabled = true;
			if (this.vfx.HasInt(EffectVfx.p_Seed))
			{
				this.vfx.SetInt(EffectVfx.p_Seed, UnityEngine.Random.Range(0, 10000));
			}
			this.vfx.Play();
			if (this.step == Effect.Step.Start || this.step == Effect.Step.End)
			{
				base.Invoke("Despawn", this.lifeTime);
			}
			this.playTime = Time.time;
		}

		// Token: 0x06001EEC RID: 7916 RVA: 0x000D2A90 File Offset: 0x000D0C90
		public override void Stop()
		{
			this.vfx.Stop();
		}

		// Token: 0x06001EED RID: 7917 RVA: 0x000D2A9D File Offset: 0x000D0C9D
		public override void End(bool loopOnly = false)
		{
			this.vfx.Stop();
			this.stopping = true;
			if (this.despawnOnEnd)
			{
				base.Invoke("Despawn", this.despawnDelay);
			}
		}

		// Token: 0x06001EEE RID: 7918 RVA: 0x000D2ACA File Offset: 0x000D0CCA
		public override void SetSize(float value)
		{
			base.SetSize(value);
			if (this.vfx.HasFloat(EffectVfx.p_Size))
			{
				this.vfx.SetFloat(EffectVfx.p_Size, value);
			}
		}

		// Token: 0x06001EEF RID: 7919 RVA: 0x000D2B00 File Offset: 0x000D0D00
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if ((!loopOnly || (loopOnly && this.step == Effect.Step.Loop)) && this.vfx.HasFloat(EffectVfx.p_Intensity))
			{
				this.vfx.SetFloat(EffectVfx.p_Intensity, this.intensityCurve.Evaluate(value));
			}
			if (this.useScaleCurve)
			{
				float scale = this.scaleCurve.Evaluate(value);
				base.transform.localScale = new Vector3(scale, scale, scale) * this.effectSize;
			}
			if (this.emitSize)
			{
				this.vfx.SetFloat(EffectVfx.p_Emitter_Size, this.curveEmitSize.Evaluate(this.intensityCurve.Evaluate(value)));
			}
		}

		// Token: 0x06001EF0 RID: 7920 RVA: 0x000D2BC4 File Offset: 0x000D0DC4
		public override void SetMainGradient(Gradient gradient)
		{
			if (gradient != null && this.vfx.HasGradient(EffectVfx.p_MainGradient))
			{
				this.vfx.SetGradient(EffectVfx.p_MainGradient, gradient);
				return;
			}
			this.vfx.ResetOverride(EffectVfx.p_MainGradient);
		}

		// Token: 0x06001EF1 RID: 7921 RVA: 0x000D2C18 File Offset: 0x000D0E18
		public override void SetSecondaryGradient(Gradient gradient)
		{
			if (gradient != null && this.vfx.HasGradient(EffectVfx.p_SecondaryGradient))
			{
				this.vfx.SetGradient(EffectVfx.p_SecondaryGradient, gradient);
				return;
			}
			this.vfx.ResetOverride(EffectVfx.p_SecondaryGradient);
		}

		// Token: 0x06001EF2 RID: 7922 RVA: 0x000D2C6C File Offset: 0x000D0E6C
		public override void SetMesh(Mesh mesh)
		{
			if (this.usePointCache)
			{
				if (!mesh.isReadable)
				{
					Debug.LogError("Cannot access vertices on mesh " + mesh.name + " for generating point cache (isReadable is false; Read/Write must be enabled in import settings)");
					return;
				}
				this.pCache = PointCacheGenerator.ComputePCacheFromMesh(mesh, this.pointCacheMapSize, this.pointCachePointCount, this.pointCacheSeed, this.pointCacheDistribution, this.pointCacheBakeMode);
				if (this.vfx.HasTexture(EffectVfx.p_PositionMap))
				{
					this.vfx.SetTexture(EffectVfx.p_PositionMap, this.pCache.positionMap);
				}
				if (this.vfx.HasTexture(EffectVfx.p_NormalMap))
				{
					this.vfx.SetTexture(EffectVfx.p_NormalMap, this.pCache.normalMap);
				}
				if (!this.pointCacheSkinnedMeshUpdate)
				{
					this.pCache.Dispose();
					this.pCache = null;
					return;
				}
			}
			else if (this.vfx.HasMesh(EffectVfx.p_Mesh))
			{
				this.vfx.SetMesh(EffectVfx.p_Mesh, mesh);
			}
		}

		// Token: 0x06001EF3 RID: 7923 RVA: 0x000D2D8C File Offset: 0x000D0F8C
		public override void SetRenderer(Renderer renderer, bool secondary)
		{
			if ((this.useSecondaryRenderer && secondary) || (!this.useSecondaryRenderer && !secondary))
			{
				SkinnedMeshRenderer meshRenderer = renderer as SkinnedMeshRenderer;
				Mesh mesh2;
				if (meshRenderer == null)
				{
					MeshFilter component = renderer.GetComponent<MeshFilter>();
					mesh2 = ((component != null) ? component.sharedMesh : null);
				}
				else
				{
					mesh2 = meshRenderer.sharedMesh;
				}
				Mesh mesh = mesh2;
				if (mesh == null)
				{
					return;
				}
				if (this.usePointCache)
				{
					if (!mesh.isReadable)
					{
						Debug.LogError("Cannot access vertices on mesh " + mesh.name + " for generating point cache (isReadable is false; Read/Write must be enabled in import settings)");
						return;
					}
					if (renderer is SkinnedMeshRenderer)
					{
						mesh = new Mesh();
						(renderer as SkinnedMeshRenderer).BakeMesh(mesh);
						this.pointCacheSkinnedMeshRenderer = (renderer as SkinnedMeshRenderer);
					}
					this.pCache = PointCacheGenerator.ComputePCacheFromMesh(mesh, this.pointCacheMapSize, this.pointCachePointCount, this.pointCacheSeed, this.pointCacheDistribution, this.pointCacheBakeMode);
					if (this.vfx.HasTexture(EffectVfx.p_PositionMap))
					{
						this.vfx.SetTexture(EffectVfx.p_PositionMap, this.pCache.positionMap);
					}
					if (this.vfx.HasTexture(EffectVfx.p_NormalMap))
					{
						this.vfx.SetTexture(EffectVfx.p_NormalMap, this.pCache.normalMap);
					}
					if (!this.pointCacheSkinnedMeshUpdate)
					{
						this.pCache.Dispose();
						this.pCache = null;
						return;
					}
				}
				else
				{
					SkinnedMeshRenderer smr = renderer as SkinnedMeshRenderer;
					if (smr != null)
					{
						this.vfx.SetSkinnedMeshRenderer(EffectVfx.p_SkinnedMeshRenderer, smr);
					}
					if (this.vfx.HasMesh(EffectVfx.p_Mesh))
					{
						this.vfx.SetMesh(EffectVfx.p_Mesh, mesh);
					}
				}
			}
		}

		// Token: 0x06001EF4 RID: 7924 RVA: 0x000D2F3A File Offset: 0x000D113A
		public override void SetCollider(Collider collider)
		{
		}

		// Token: 0x06001EF5 RID: 7925 RVA: 0x000D2F3C File Offset: 0x000D113C
		public override void SetSource(Transform source)
		{
			this.sourceTransform = source;
			if (source && this.vfx.HasVector3(EffectVfx.p_Source_position))
			{
				this.hasSource = true;
				this.UpdateSource();
				return;
			}
			this.hasSource = false;
		}

		// Token: 0x06001EF6 RID: 7926 RVA: 0x000D2F79 File Offset: 0x000D1179
		public override void SetTarget(Transform target)
		{
			this.targetTransform = target;
			if (target && this.vfx.HasVector3(EffectVfx.p_Target_position))
			{
				this.hasTarget = true;
				this.UpdateTarget();
				return;
			}
			this.hasTarget = false;
		}

		// Token: 0x170001EA RID: 490
		// (get) Token: 0x06001EF7 RID: 7927 RVA: 0x000D2FB6 File Offset: 0x000D11B6
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update | ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x06001EF8 RID: 7928 RVA: 0x000D2FBC File Offset: 0x000D11BC
		protected internal override void ManagedUpdate()
		{
			if (this.lookAtTarget)
			{
				base.transform.LookAt(2f * base.transform.position - this.targetTransform.position, Vector3.up);
			}
			this.UpdateSource();
			this.UpdateTarget();
			if (this.stopping && this.vfx.aliveParticleCount == 0)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001EF9 RID: 7929 RVA: 0x000D302D File Offset: 0x000D122D
		protected internal override void ManagedLateUpdate()
		{
			if (this.pointCacheSkinnedMeshUpdate)
			{
				this.pCache.Update(this.pointCacheSkinnedMeshRenderer);
			}
		}

		// Token: 0x06001EFA RID: 7930 RVA: 0x000D3048 File Offset: 0x000D1248
		public void UpdateSource()
		{
			if (this.hasSource && this.sourceTransform)
			{
				if (this.vfx.HasVector3(EffectVfx.p_Source_position))
				{
					this.vfx.SetVector3(EffectVfx.p_Source_position, this.sourceTransform.position);
				}
				if (this.vfx.HasVector3(EffectVfx.p_Source_angles))
				{
					this.vfx.SetVector3(EffectVfx.p_Source_angles, this.sourceTransform.eulerAngles);
				}
				if (this.vfx.HasVector3(EffectVfx.p_Source_scale))
				{
					this.vfx.SetVector3(EffectVfx.p_Source_scale, this.sourceTransform.localScale);
				}
				if (this.spawnOn == SpawnTarget.Source)
				{
					this.vfx.transform.SetPositionAndRotation(this.sourceTransform.position, this.sourceTransform.rotation);
				}
			}
		}

		// Token: 0x06001EFB RID: 7931 RVA: 0x000D3144 File Offset: 0x000D1344
		public void UpdateTarget()
		{
			if (this.hasTarget && this.targetTransform)
			{
				if (this.vfx.HasVector3(EffectVfx.p_Target_position))
				{
					this.vfx.SetVector3(EffectVfx.p_Target_position, this.targetTransform.position);
				}
				if (this.vfx.HasVector3(EffectVfx.p_Target_angles))
				{
					this.vfx.SetVector3(EffectVfx.p_Target_angles, this.targetTransform.eulerAngles);
				}
				if (this.vfx.HasVector3(EffectVfx.p_Target_scale))
				{
					this.vfx.SetVector3(EffectVfx.p_Target_scale, this.targetTransform.localScale);
				}
				if (this.spawnOn == SpawnTarget.Target)
				{
					this.vfx.transform.SetPositionAndRotation(this.targetTransform.position, this.targetTransform.rotation);
				}
			}
		}

		// Token: 0x06001EFC RID: 7932 RVA: 0x000D3240 File Offset: 0x000D1440
		public override void Despawn()
		{
			if (this.pCache != null)
			{
				this.pCache.Dispose();
				this.pCache = null;
			}
			this.pointCacheSkinnedMeshRenderer = null;
			base.CancelInvoke();
			this.stopping = false;
			this.vfx.Stop();
			this.lookAtTarget = false;
			this.vfx.enabled = false;
			if (Application.isPlaying)
			{
				EffectModuleVfx.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x06001EFD RID: 7933 RVA: 0x000D32AC File Offset: 0x000D14AC
		public void SetProperty<T>(string key, T value)
		{
			if (value is bool)
			{
				bool boolValue = value as bool;
				if (this.vfx.HasBool(key))
				{
					this.vfx.SetBool(key, boolValue);
					return;
				}
			}
			else if (value is float)
			{
				float floatValue = value as float;
				if (this.vfx.HasFloat(key))
				{
					this.vfx.SetFloat(key, floatValue);
					return;
				}
			}
			else
			{
				Gradient gradientValue = value as Gradient;
				if (gradientValue == null)
				{
					if (value is int)
					{
						int intValue = value as int;
						if (this.vfx.HasInt(key))
						{
							this.vfx.SetInt(key, intValue);
							return;
						}
					}
					else if (value is Matrix4x4)
					{
						Matrix4x4 matrixValue = value as Matrix4x4;
						if (this.vfx.HasMatrix4x4(key))
						{
							this.vfx.SetMatrix4x4(key, matrixValue);
							return;
						}
					}
					else
					{
						Texture textureValue = value as Texture;
						if (textureValue == null)
						{
							if (value is Vector3)
							{
								Vector3 vector3Value = value as Vector3;
								if (this.vfx.HasVector3(key))
								{
									this.vfx.SetVector3(key, vector3Value);
									return;
								}
							}
							else
							{
								Mesh meshValue = value as Mesh;
								if (meshValue == null)
								{
									if (value is Vector2)
									{
										Vector2 vector2Value = value as Vector2;
										if (this.vfx.HasVector2(key))
										{
											this.vfx.SetVector2(key, vector2Value);
											return;
										}
									}
									else if (value is Vector4)
									{
										Vector4 vector4Value = value as Vector4;
										if (this.vfx.HasVector4(key))
										{
											this.vfx.SetVector4(key, vector4Value);
											return;
										}
									}
									else
									{
										AnimationCurve curveValue = value as AnimationCurve;
										if (curveValue == null)
										{
											GraphicsBuffer bufferValue = value as GraphicsBuffer;
											if (bufferValue == null)
											{
												if (value is uint)
												{
													uint uintValue = value as uint;
													if (this.vfx.HasUInt(key))
													{
														this.vfx.SetUInt(key, uintValue);
														return;
													}
												}
												else
												{
													SkinnedMeshRenderer smrValue = value as SkinnedMeshRenderer;
													if (smrValue == null)
													{
														return;
													}
													if (this.vfx.HasSkinnedMeshRenderer(key))
													{
														this.vfx.SetSkinnedMeshRenderer(key, smrValue);
													}
												}
											}
											else if (this.vfx.HasGraphicsBuffer(key))
											{
												this.vfx.SetGraphicsBuffer(key, bufferValue);
												return;
											}
										}
										else if (this.vfx.HasAnimationCurve(key))
										{
											this.vfx.SetAnimationCurve(key, curveValue);
											return;
										}
									}
								}
								else if (this.vfx.HasMesh(key))
								{
									this.vfx.SetMesh(key, meshValue);
									return;
								}
							}
						}
						else if (this.vfx.HasTexture(key))
						{
							this.vfx.SetTexture(key, textureValue);
							return;
						}
					}
				}
				else if (this.vfx.HasGradient(key))
				{
					this.vfx.SetGradient(key, gradientValue);
					return;
				}
			}
		}

		// Token: 0x06001EFE RID: 7934 RVA: 0x000D35FC File Offset: 0x000D17FC
		public bool TryGetProperty<T>(string key, out T value) where T : class
		{
			value = default(T);
			if (typeof(T) == typeof(bool))
			{
				if (this.vfx.HasBool(key))
				{
					value = (this.vfx.GetBool(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(float))
			{
				if (this.vfx.HasFloat(key))
				{
					value = (this.vfx.GetFloat(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Gradient))
			{
				if (this.vfx.HasGradient(key))
				{
					value = (this.vfx.GetGradient(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(int))
			{
				if (this.vfx.HasInt(key))
				{
					value = (this.vfx.GetInt(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Matrix4x4))
			{
				if (this.vfx.HasMatrix4x4(key))
				{
					value = (this.vfx.GetMatrix4x4(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Texture))
			{
				if (this.vfx.HasTexture(key))
				{
					value = (this.vfx.GetTexture(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Vector3))
			{
				if (this.vfx.HasVector3(key))
				{
					value = (this.vfx.GetVector3(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Mesh))
			{
				if (this.vfx.HasMesh(key))
				{
					value = (this.vfx.GetMesh(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Vector2))
			{
				if (this.vfx.HasVector2(key))
				{
					value = (this.vfx.GetVector2(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(Vector4))
			{
				if (this.vfx.HasVector4(key))
				{
					value = (this.vfx.GetVector4(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(AnimationCurve))
			{
				if (this.vfx.HasAnimationCurve(key))
				{
					value = (this.vfx.GetAnimationCurve(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(uint))
			{
				if (this.vfx.HasUInt(key))
				{
					value = (this.vfx.GetUInt(key) as T);
					return true;
				}
			}
			else if (typeof(T) == typeof(SkinnedMeshRenderer) && this.vfx.HasSkinnedMeshRenderer(key))
			{
				value = (this.vfx.GetSkinnedMeshRenderer(key) as T);
				return true;
			}
			return false;
		}

		// Token: 0x04001DD3 RID: 7635
		public VisualEffect vfx;

		// Token: 0x04001DD4 RID: 7636
		public float lifeTime = 5f;

		// Token: 0x04001DD5 RID: 7637
		public Transform sourceTransform;

		// Token: 0x04001DD6 RID: 7638
		public Transform targetTransform;

		// Token: 0x04001DD7 RID: 7639
		[NonSerialized]
		public EffectVfxPoolManager poolManager;

		// Token: 0x04001DD8 RID: 7640
		public SpawnTarget spawnOn;

		// Token: 0x04001DD9 RID: 7641
		public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		});

		// Token: 0x04001DDA RID: 7642
		public bool useScaleCurve;

		// Token: 0x04001DDB RID: 7643
		public AnimationCurve scaleCurve;

		// Token: 0x04001DDC RID: 7644
		public bool lookAtTarget;

		// Token: 0x04001DDD RID: 7645
		public bool useSecondaryRenderer;

		// Token: 0x04001DDE RID: 7646
		public bool despawnOnEnd;

		// Token: 0x04001DDF RID: 7647
		public float despawnDelay;

		// Token: 0x04001DE0 RID: 7648
		public bool usePointCache;

		// Token: 0x04001DE1 RID: 7649
		public bool pointCacheSkinnedMeshUpdate;

		// Token: 0x04001DE2 RID: 7650
		public int pointCacheMapSize = 512;

		// Token: 0x04001DE3 RID: 7651
		public int pointCachePointCount = 4096;

		// Token: 0x04001DE4 RID: 7652
		public int pointCacheSeed;

		// Token: 0x04001DE5 RID: 7653
		public PointCacheGenerator.Distribution pointCacheDistribution = PointCacheGenerator.Distribution.RandomUniformArea;

		// Token: 0x04001DE6 RID: 7654
		public PointCacheGenerator.MeshBakeMode pointCacheBakeMode = PointCacheGenerator.MeshBakeMode.Triangle;

		// Token: 0x04001DE7 RID: 7655
		protected PointCacheGenerator.PCache pCache;

		// Token: 0x04001DE8 RID: 7656
		protected SkinnedMeshRenderer pointCacheSkinnedMeshRenderer;

		// Token: 0x04001DE9 RID: 7657
		[NonSerialized]
		public float playTime;

		// Token: 0x04001DEA RID: 7658
		[Header("Intensity to Emitter Size")]
		public bool emitSize;

		// Token: 0x04001DEB RID: 7659
		public AnimationCurve curveEmitSize;

		// Token: 0x04001DEC RID: 7660
		protected bool stopping;

		// Token: 0x04001DED RID: 7661
		protected bool hasTarget;

		// Token: 0x04001DEE RID: 7662
		protected bool hasSource;

		// Token: 0x04001DEF RID: 7663
		public static readonly ExposedProperty p_Seed = "Seed";

		// Token: 0x04001DF0 RID: 7664
		public static readonly ExposedProperty p_Size = "Size";

		// Token: 0x04001DF1 RID: 7665
		public static readonly ExposedProperty p_Intensity = "Intensity";

		// Token: 0x04001DF2 RID: 7666
		public static readonly ExposedProperty p_Emitter_Size = "Emitter Size";

		// Token: 0x04001DF3 RID: 7667
		public static readonly ExposedProperty p_MainGradient = "MainGradient";

		// Token: 0x04001DF4 RID: 7668
		public static readonly ExposedProperty p_SecondaryGradient = "SecondaryGradient";

		// Token: 0x04001DF5 RID: 7669
		public static readonly ExposedProperty p_PositionMap = "PositionMap";

		// Token: 0x04001DF6 RID: 7670
		public static readonly ExposedProperty p_NormalMap = "NormalMap";

		// Token: 0x04001DF7 RID: 7671
		public static readonly ExposedProperty p_Mesh = "Mesh";

		// Token: 0x04001DF8 RID: 7672
		public static readonly ExposedProperty p_SkinnedMeshRenderer = "SkinnedMeshRenderer";

		// Token: 0x04001DF9 RID: 7673
		public static readonly ExposedProperty p_Source_position = "Source_position";

		// Token: 0x04001DFA RID: 7674
		public static readonly ExposedProperty p_Source_angles = "Source_angles";

		// Token: 0x04001DFB RID: 7675
		public static readonly ExposedProperty p_Source_scale = "Source_scale";

		// Token: 0x04001DFC RID: 7676
		public static readonly ExposedProperty p_Target_position = "Target_position";

		// Token: 0x04001DFD RID: 7677
		public static readonly ExposedProperty p_Target_angles = "Target_angles";

		// Token: 0x04001DFE RID: 7678
		public static readonly ExposedProperty p_Target_scale = "Target_scale";
	}
}
