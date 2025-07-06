using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderRoad
{
	// Token: 0x0200028B RID: 651
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectParticle.html")]
	public class EffectParticle : Effect
	{
		// Token: 0x06001EB6 RID: 7862 RVA: 0x000D0F05 File Offset: 0x000CF105
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (!Application.isPlaying)
			{
				this.Init();
			}
		}

		// Token: 0x06001EB7 RID: 7863 RVA: 0x000D0F22 File Offset: 0x000CF122
		private void Awake()
		{
			this.Init();
		}

		// Token: 0x06001EB8 RID: 7864 RVA: 0x000D0F2C File Offset: 0x000CF12C
		private void Init()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
			this.rootParticleSystem = base.GetComponent<ParticleSystem>();
			if (!this.rootParticleSystem)
			{
				this.rootParticleSystem = base.gameObject.AddComponent<ParticleSystem>();
				this.rootParticleSystem.emission.enabled = false;
				this.rootParticleSystem.shape.enabled = false;
				this.rootParticleSystem.GetComponent<ParticleSystemRenderer>().enabled = false;
			}
			this.childs = new List<EffectParticleChild>(base.GetComponentsInChildren<EffectParticleChild>());
			foreach (EffectParticleChild child in this.childs)
			{
				child.particleSystem = child.GetComponent<ParticleSystem>();
				ParticleSystem.MainModule particleSystemMain = child.particleSystem.main;
				if (this.step == Effect.Step.Custom)
				{
					particleSystemMain.playOnAwake = false;
				}
				child.particleRenderer = child.particleSystem.GetComponent<ParticleSystemRenderer>();
				child.materialPropertyBlock = new MaterialPropertyBlock();
				if (Application.isPlaying && child.sendCollisionEvents)
				{
					child.particleCollisionDetector = child.particleSystem.gameObject.AddComponent<ParticleCollisionDetector>();
				}
				if (Application.isPlaying && child.emitEffectOnCollision)
				{
					child.particleCollisionSpawner = child.particleSystem.gameObject.AddComponent<ParticleCollisionSpawner>();
				}
			}
		}

		// Token: 0x06001EB9 RID: 7865 RVA: 0x000D1090 File Offset: 0x000CF290
		public override void Play()
		{
			int childsCount = this.childs.Count;
			for (int index = 0; index < childsCount; index++)
			{
				EffectParticleChild p = this.childs[index];
				if (!(p == null))
				{
					ParticleSystem.ShapeModule shapeModule = p.particleSystem.shape;
					ParticleSystemShapeType shapeType = shapeModule.shapeType;
					bool enabled;
					if (shapeType != ParticleSystemShapeType.Mesh)
					{
						if (shapeType != ParticleSystemShapeType.MeshRenderer)
						{
							if (shapeType != ParticleSystemShapeType.SkinnedMeshRenderer || !(shapeModule.skinnedMeshRenderer == null))
							{
								goto IL_A5;
							}
							enabled = false;
						}
						else
						{
							if (!(shapeModule.meshRenderer == null))
							{
								goto IL_A5;
							}
							enabled = false;
						}
					}
					else
					{
						if (!(shapeModule.mesh == null))
						{
							goto IL_A5;
						}
						enabled = false;
					}
					IL_AE:
					shapeModule.enabled = enabled;
					if (AreaManager.Instance)
					{
						LightVolumeReceiver.ApplyProbeVolume(p.particleRenderer, this.materialPropertyBlock);
						goto IL_E2;
					}
					LightVolumeReceiver.DisableProbeVolume(p.particleRenderer);
					goto IL_E2;
					IL_A5:
					enabled = shapeModule.enabled;
					goto IL_AE;
				}
				Debug.LogError(base.name + " have an EffectParticleChild that has been destroyed! (this could happen if a effectParticle component has been added to one of the childs)");
				IL_E2:;
			}
			foreach (EffectParticleChild child in this.childs)
			{
				if (Application.isPlaying && child.sendCollisionEvents)
				{
					child.particleCollisionDetector.OnCollisionEvent -= this.containingInstance.InvokeCollisionEvent;
					child.particleCollisionDetector.OnCollisionEvent += this.containingInstance.InvokeCollisionEvent;
				}
			}
			this.rootParticleSystem.Play(true);
			if (this.step == Effect.Step.Start || this.step == Effect.Step.End)
			{
				base.Invoke("Despawn", this.lifeTime);
			}
			this.playTime = Time.time;
		}

		// Token: 0x06001EBA RID: 7866 RVA: 0x000D1250 File Offset: 0x000CF450
		public IEnumerator TimedDespawn(float lifeTime)
		{
			yield return Yielders.ForSeconds(lifeTime);
			this.Despawn();
			yield break;
		}

		// Token: 0x06001EBB RID: 7867 RVA: 0x000D1266 File Offset: 0x000CF466
		public override void Stop()
		{
			this.rootParticleSystem.Stop();
		}

		// Token: 0x06001EBC RID: 7868 RVA: 0x000D1273 File Offset: 0x000CF473
		public override void End(bool loopOnly = false)
		{
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				base.CancelInvoke();
				this.rootParticleSystem.Stop();
				base.Invoke("Despawn", this.lifeTime);
			}
		}

		// Token: 0x170001E9 RID: 489
		// (get) Token: 0x06001EBD RID: 7869 RVA: 0x000D12A6 File Offset: 0x000CF4A6
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				if (!this.renderInLateUpdate)
				{
					return (ManagedLoops)0;
				}
				return ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x06001EBE RID: 7870 RVA: 0x000D12B4 File Offset: 0x000CF4B4
		protected internal override void ManagedLateUpdate()
		{
			if (this.renderInLateUpdate && this.playTime > 0f)
			{
				int childsCount = this.childs.Count;
				for (int i = 0; i < childsCount; i++)
				{
					EffectParticleChild p = this.childs[i];
					if (!(p == null))
					{
						p.particleSystem.Simulate(Time.deltaTime, true, false, false);
					}
				}
			}
		}

		// Token: 0x06001EBF RID: 7871 RVA: 0x000D1317 File Offset: 0x000CF517
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if ((!loopOnly || (loopOnly && this.step == Effect.Step.Loop)) && this.effectLink == EffectLink.Intensity)
			{
				this.SetVariation(value, loopOnly);
			}
		}

		// Token: 0x06001EC0 RID: 7872 RVA: 0x000D1340 File Offset: 0x000CF540
		public override void SetSpeed(float value, bool loopOnly = false)
		{
			base.SetSpeed(value, loopOnly);
			if ((!loopOnly || (loopOnly && this.step == Effect.Step.Loop)) && this.effectLink == EffectLink.Speed)
			{
				this.SetVariation(value, loopOnly);
			}
		}

		// Token: 0x06001EC1 RID: 7873 RVA: 0x000D136A File Offset: 0x000CF56A
		public override void SetSize(float size)
		{
			base.SetSize(size);
			this.SetVariation(this.effectIntensity, false);
		}

		// Token: 0x06001EC2 RID: 7874 RVA: 0x000D1380 File Offset: 0x000CF580
		public void SetVariation(float value, bool loopOnly = false)
		{
			this.currentValue = this.intensityCurve.Evaluate(value);
			if (this.useScaleCurve)
			{
				float scale = this.scaleCurve.Evaluate(value);
				base.transform.localScale = new Vector3(scale, scale, scale) * this.effectSize;
			}
			int childsCount = this.childs.Count;
			for (int i = 0; i < childsCount; i++)
			{
				EffectParticleChild p = this.childs[i];
				if (p == null)
				{
					Debug.LogError(base.name + " has an EffectParticleChild that has been destroyed! (this could happen if a effectParticle component has been added to one of the childs)");
				}
				else
				{
					ParticleSystem.MainModule mainModule = p.particleSystem.main;
					if (p.duration && !p.particleSystem.isPlaying)
					{
						mainModule.duration = p.curveDuration.Evaluate(this.currentValue);
					}
					if (p.lifeTime)
					{
						this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
						float lifeTime = p.curveLifeTime.Evaluate(this.currentValue);
						this.minMaxCurve.constantMin = Mathf.Clamp(lifeTime - p.randomRangeLifeTime, 0f, float.PositiveInfinity);
						this.minMaxCurve.constantMax = Mathf.Clamp(lifeTime, 0f, float.PositiveInfinity);
						mainModule.startLifetime = this.minMaxCurve;
					}
					if (p.speed)
					{
						this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
						float speed = p.curveSpeed.Evaluate(this.currentValue);
						this.minMaxCurve.constantMin = (this.minMaxCurve.constantMin = Mathf.Clamp(speed - p.randomRangeSpeed, 0f, float.PositiveInfinity));
						this.minMaxCurve.constantMax = speed;
						mainModule.startSpeed = this.minMaxCurve;
					}
					if (p.size)
					{
						this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
						float size = p.curveSize.Evaluate(this.currentValue);
						this.minMaxCurve.constantMin = Mathf.Clamp(size - p.randomRangeSize, 0f, float.PositiveInfinity);
						this.minMaxCurve.constantMax = Mathf.Clamp(size, 0f, float.PositiveInfinity);
						mainModule.startSize = this.minMaxCurve;
					}
					if (p.shapeRadius)
					{
						p.particleSystem.shape.radius = p.curveShapeRadius.Evaluate(this.currentValue);
					}
					if (p.shapeArc)
					{
						p.particleSystem.shape.arc = p.curveShapeArc.Evaluate(this.currentValue);
					}
					if (p.rate)
					{
						this.minMaxCurve.mode = ParticleSystemCurveMode.TwoConstants;
						float rate = p.curveRate.Evaluate(this.currentValue);
						this.minMaxCurve.constantMin = Mathf.Clamp(rate - p.randomRangeRate, 0f, float.PositiveInfinity);
						this.minMaxCurve.constantMax = Mathf.Clamp(rate, 0f, float.PositiveInfinity);
						p.particleSystem.emission.rateOverTime = this.minMaxCurve;
					}
					if (p.rateOverDistance)
					{
						p.particleSystem.emission.rateOverDistanceMultiplier = p.curveDistanceRate.Evaluate(this.currentValue);
					}
					if (p.burst)
					{
						short burst = (short)p.curveBurst.Evaluate(value);
						ParticleSystem.Burst particleBurst = new ParticleSystem.Burst(0f, (short)Mathf.Clamp((float)(burst - p.randomRangeBurst), 0f, float.PositiveInfinity), (short)Mathf.Clamp((float)burst, 0f, float.PositiveInfinity));
						p.particleSystem.emission.SetBurst(0, particleBurst);
					}
					if (p.velocityOverLifetime)
					{
						float rate2 = p.curvevelocityOverLifetime.Evaluate(this.currentValue);
						this.minMaxCurve.constantMin = Mathf.Clamp(rate2, 0f, float.PositiveInfinity);
						this.minMaxCurve.constantMax = Mathf.Clamp(rate2, 0f, float.PositiveInfinity);
						p.particleSystem.velocityOverLifetime.speedModifier = this.minMaxCurve;
					}
					if (p.lightIntensity)
					{
						p.particleSystem.lights.intensityMultiplier = p.curveLightIntensity.Evaluate(this.currentValue);
					}
					ParticleSystem.MinMaxGradient minMaxGradient = mainModule.startColor;
					if (p.linkStartGradient == EffectTarget.Main && this.currentMainGradient != null)
					{
						minMaxGradient.mode = ParticleSystemGradientMode.Gradient;
						minMaxGradient.gradient = this.currentMainGradient;
					}
					if (p.linkStartGradient == EffectTarget.Secondary && this.currentSecondaryGradient != null)
					{
						minMaxGradient.mode = ParticleSystemGradientMode.Gradient;
						minMaxGradient.gradient = this.currentSecondaryGradient;
					}
					EffectTarget linkStartColor = p.linkStartColor;
					if (linkStartColor != EffectTarget.Main)
					{
						if (linkStartColor != EffectTarget.Secondary)
						{
							goto IL_633;
						}
						if (this.currentSecondaryGradient == null)
						{
							goto IL_633;
						}
						minMaxGradient.mode = ParticleSystemGradientMode.Color;
						if (p.alpha)
						{
							float alpha = p.curveAlpha.Evaluate(this.currentValue);
							Color newColor = this.currentMainGradient.Evaluate(this.currentValue);
							minMaxGradient.color = new Color(newColor.r, newColor.g, newColor.b, alpha);
						}
						else if (p.ignoreAlpha)
						{
							Color newColor2 = this.currentSecondaryGradient.Evaluate(this.currentValue);
							minMaxGradient.color = new Color(newColor2.r, newColor2.g, newColor2.b, minMaxGradient.color.a);
						}
						else
						{
							minMaxGradient.color = this.currentSecondaryGradient.Evaluate(this.currentValue);
						}
					}
					else
					{
						if (this.currentMainGradient == null)
						{
							goto IL_633;
						}
						minMaxGradient.mode = ParticleSystemGradientMode.Color;
						if (p.alpha)
						{
							float alpha2 = p.curveAlpha.Evaluate(this.currentValue);
							Color newColor3 = this.currentMainGradient.Evaluate(this.currentValue);
							minMaxGradient.color = new Color(newColor3.r, newColor3.g, newColor3.b, alpha2);
						}
						else if (p.ignoreAlpha)
						{
							Color newColor4 = this.currentMainGradient.Evaluate(this.currentValue);
							minMaxGradient.color = new Color(newColor4.r, newColor4.g, newColor4.b, minMaxGradient.color.a);
						}
						else
						{
							minMaxGradient.color = this.currentMainGradient.Evaluate(this.currentValue);
						}
					}
					IL_690:
					mainModule.startColor = minMaxGradient;
					bool updatePropertyBlock = false;
					if (p.linkBaseColor == EffectTarget.Main && this.currentMainGradient != null)
					{
						p.materialPropertyBlock.SetColor(EffectParticle.BaseColor, this.currentMainGradient.Evaluate(this.currentValue));
						updatePropertyBlock = true;
					}
					else if (p.linkBaseColor == EffectTarget.Secondary && this.currentSecondaryGradient != null)
					{
						p.materialPropertyBlock.SetColor(EffectParticle.BaseColor, this.currentSecondaryGradient.Evaluate(this.currentValue));
						updatePropertyBlock = true;
					}
					if (p.linkTintColor == EffectTarget.Main && this.currentMainGradient != null)
					{
						p.materialPropertyBlock.SetColor(EffectParticle.TintColor, this.currentMainGradient.Evaluate(this.currentValue));
						updatePropertyBlock = true;
					}
					else if (p.linkTintColor == EffectTarget.Secondary && this.currentSecondaryGradient != null)
					{
						p.materialPropertyBlock.SetColor(EffectParticle.TintColor, this.currentSecondaryGradient.Evaluate(this.currentValue));
						updatePropertyBlock = true;
					}
					if (p.linkEmissionColor == EffectTarget.Main && this.currentMainGradient != null)
					{
						p.materialPropertyBlock.SetColor(EffectParticle.EmissionColor, this.currentMainGradient.Evaluate(this.currentValue));
						updatePropertyBlock = true;
					}
					else if (p.linkEmissionColor == EffectTarget.Secondary && this.currentSecondaryGradient != null)
					{
						p.materialPropertyBlock.SetColor(EffectParticle.EmissionColor, this.currentSecondaryGradient.Evaluate(this.currentValue));
						updatePropertyBlock = true;
					}
					if (updatePropertyBlock)
					{
						p.particleRenderer.SetPropertyBlock(p.materialPropertyBlock);
						goto IL_7F5;
					}
					goto IL_7F5;
					IL_633:
					if (!p.alpha)
					{
						goto IL_690;
					}
					minMaxGradient.mode = ParticleSystemGradientMode.Color;
					if (p.alpha)
					{
						float alpha3 = p.curveAlpha.Evaluate(this.currentValue);
						minMaxGradient.color = new Color(minMaxGradient.color.r, minMaxGradient.color.g, minMaxGradient.color.b, alpha3);
						goto IL_690;
					}
					goto IL_690;
				}
				IL_7F5:;
			}
		}

		// Token: 0x06001EC3 RID: 7875 RVA: 0x000D1B8D File Offset: 0x000CFD8D
		public override void SetMainGradient(Gradient gradient)
		{
			this.currentMainGradient = gradient;
			this.SetIntensity(this.currentValue, false);
		}

		// Token: 0x06001EC4 RID: 7876 RVA: 0x000D1BA3 File Offset: 0x000CFDA3
		public override void SetSecondaryGradient(Gradient gradient)
		{
			this.currentSecondaryGradient = gradient;
			this.SetIntensity(this.currentValue, false);
		}

		// Token: 0x06001EC5 RID: 7877 RVA: 0x000D1BBC File Offset: 0x000CFDBC
		public override void SetMesh(Mesh mesh)
		{
			if (mesh == null || mesh.vertexCount == 0)
			{
				return;
			}
			int childsCount = this.childs.Count;
			for (int i = 0; i < childsCount; i++)
			{
				EffectParticleChild p = this.childs[i];
				if (p == null)
				{
					Debug.LogError(base.name + " has an EffectParticleChild that has been destroyed! (this could happen if a effectParticle component has been added to one of the childs)");
				}
				else if (p.mesh)
				{
					ParticleSystem.ShapeModule shapeModule = p.particleSystem.shape;
					shapeModule.shapeType = ParticleSystemShapeType.Mesh;
					shapeModule.mesh = mesh;
					shapeModule.enabled = true;
				}
			}
		}

		// Token: 0x06001EC6 RID: 7878 RVA: 0x000D1C4C File Offset: 0x000CFE4C
		public override void SetRenderer(Renderer renderer, bool secondary)
		{
			if (renderer == null)
			{
				return;
			}
			int childsCount = this.childs.Count;
			for (int i = 0; i < childsCount; i++)
			{
				EffectParticleChild p = this.childs[i];
				if (p == null)
				{
					Debug.LogError(base.name + " has an EffectParticleChild that has been destroyed! (this could happen if a effectParticle component has been added to one of the childs)");
				}
				else
				{
					ParticleSystem.ShapeModule shapeModule = p.particleSystem.shape;
					if ((p.useRenderer == EffectTarget.Main && !secondary) || (p.useRenderer == EffectTarget.Secondary && secondary))
					{
						if (renderer is MeshRenderer)
						{
							shapeModule.shapeType = ParticleSystemShapeType.MeshRenderer;
							shapeModule.meshRenderer = (renderer as MeshRenderer);
							shapeModule.enabled = true;
						}
						if (renderer is SkinnedMeshRenderer)
						{
							shapeModule.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
							shapeModule.skinnedMeshRenderer = (renderer as SkinnedMeshRenderer);
							shapeModule.enabled = true;
						}
					}
				}
			}
		}

		// Token: 0x06001EC7 RID: 7879 RVA: 0x000D1D20 File Offset: 0x000CFF20
		public override void SetCollider(Collider collider)
		{
			int childsCount = this.childs.Count;
			for (int i = 0; i < childsCount; i++)
			{
				EffectParticleChild p = this.childs[i];
				if (p == null)
				{
					Debug.LogError(base.name + " has an EffectParticleChild that has been destroyed! (this could happen if a effectParticle component has been added to one of the childs)");
				}
				else if (p.collider)
				{
					p.particleSystem.main.scalingMode = ParticleSystemScalingMode.Hierarchy;
					ParticleSystem.ShapeModule shapeModule = p.particleSystem.shape;
					Transform colliderTransform = collider.transform;
					Vector3 colliderLossyScale = colliderTransform.lossyScale;
					if (collider is SphereCollider)
					{
						shapeModule.shapeType = ParticleSystemShapeType.Sphere;
						shapeModule.radius = (collider as SphereCollider).radius * colliderLossyScale.magnitude;
						shapeModule.position = p.transform.InverseTransformPoint(colliderTransform.TransformPoint((collider as SphereCollider).center));
					}
					else if (collider is CapsuleCollider)
					{
						shapeModule.shapeType = ParticleSystemShapeType.Box;
						float height = (collider as CapsuleCollider).height;
						float radius = (collider as CapsuleCollider).radius;
						if ((collider as CapsuleCollider).direction == 0)
						{
							shapeModule.scale = new Vector3(height * colliderLossyScale.x, radius * Mathf.Max(colliderLossyScale.y, colliderLossyScale.z), radius * Mathf.Max(colliderLossyScale.y, colliderLossyScale.z));
						}
						if ((collider as CapsuleCollider).direction == 1)
						{
							shapeModule.scale = new Vector3(radius * Mathf.Max(colliderLossyScale.x, colliderLossyScale.z), height * colliderLossyScale.y, radius * Mathf.Max(colliderLossyScale.x, colliderLossyScale.z));
						}
						if ((collider as CapsuleCollider).direction == 2)
						{
							shapeModule.scale = new Vector3(radius * Mathf.Max(colliderLossyScale.x, colliderLossyScale.y), radius * Mathf.Max(colliderLossyScale.x, colliderLossyScale.y), height * colliderLossyScale.z);
						}
						shapeModule.position = p.transform.InverseTransformPoint(colliderTransform.TransformPoint((collider as CapsuleCollider).center));
					}
					else if (collider is BoxCollider)
					{
						shapeModule.shapeType = ParticleSystemShapeType.Box;
						shapeModule.scale = new Vector3((collider as BoxCollider).size.x * colliderLossyScale.x, (collider as BoxCollider).size.y * colliderLossyScale.y, (collider as BoxCollider).size.z * colliderLossyScale.z);
						shapeModule.position = p.transform.InverseTransformPoint(colliderTransform.TransformPoint((collider as BoxCollider).center));
						shapeModule.scale = (collider as BoxCollider).size;
					}
					shapeModule.rotation = (Quaternion.Inverse(p.transform.rotation) * colliderTransform.rotation).eulerAngles;
				}
			}
		}

		/// <summary>
		/// This is used by the pooling system to sort of fake despawn the effect without returning it to the pool, because we are going to use it again right away
		/// </summary>
		// Token: 0x06001EC8 RID: 7880 RVA: 0x000D201C File Offset: 0x000D021C
		public void FakeDespawn()
		{
			try
			{
				this.playTime = 0f;
				if (this.rootParticleSystem != null)
				{
					this.rootParticleSystem.Stop();
					base.CancelInvoke();
				}
				if (Application.isPlaying)
				{
					base.InvokeDespawnCallback();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in EffectParticle.FakeDespawn: {0}", e));
			}
		}

		// Token: 0x06001EC9 RID: 7881 RVA: 0x000D2088 File Offset: 0x000D0288
		public override void Despawn()
		{
			this.playTime = 0f;
			try
			{
				if (this.rootParticleSystem != null)
				{
					this.rootParticleSystem.Stop();
					base.CancelInvoke();
				}
				for (int i = 0; i < this.childs.Count; i++)
				{
					if (this.childs[i].sendCollisionEvents)
					{
						ParticleCollisionDetector detector = this.childs[i].particleCollisionDetector;
						if (detector != null)
						{
							detector.OnCollisionEvent -= this.containingInstance.InvokeCollisionEvent;
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in EffectParticle.Despawn: {0}", e));
			}
			if (Application.isPlaying)
			{
				EffectModuleParticle.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001D60 RID: 7520
		[NonSerialized]
		public EffectParticlePoolManager poolManager;

		// Token: 0x04001D61 RID: 7521
		public int poolCount = 50;

		// Token: 0x04001D62 RID: 7522
		public float lifeTime = 5f;

		// Token: 0x04001D63 RID: 7523
		public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		});

		// Token: 0x04001D64 RID: 7524
		[NonSerialized]
		public float playTime;

		// Token: 0x04001D65 RID: 7525
		public EffectLink effectLink;

		// Token: 0x04001D66 RID: 7526
		public bool renderInLateUpdate;

		// Token: 0x04001D67 RID: 7527
		public bool useScaleCurve;

		// Token: 0x04001D68 RID: 7528
		public AnimationCurve scaleCurve;

		// Token: 0x04001D69 RID: 7529
		[NonSerialized]
		public List<EffectParticleChild> childs = new List<EffectParticleChild>();

		// Token: 0x04001D6A RID: 7530
		[NonSerialized]
		public ParticleSystem rootParticleSystem;

		// Token: 0x04001D6B RID: 7531
		protected ParticleSystem.MinMaxCurve minMaxCurve;

		// Token: 0x04001D6C RID: 7532
		[NonSerialized]
		public float currentValue;

		// Token: 0x04001D6D RID: 7533
		[GradientUsage(true)]
		[NonSerialized]
		public Gradient currentMainGradient;

		// Token: 0x04001D6E RID: 7534
		[GradientUsage(true)]
		[NonSerialized]
		public Gradient currentSecondaryGradient;

		// Token: 0x04001D6F RID: 7535
		protected MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04001D70 RID: 7536
		private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");

		// Token: 0x04001D71 RID: 7537
		private static readonly int TintColor = Shader.PropertyToID("_TintColor");

		// Token: 0x04001D72 RID: 7538
		private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

		// Token: 0x04001D73 RID: 7539
		private Coroutine despawnCoroutine;

		// Token: 0x04001D74 RID: 7540
		[FormerlySerializedAs("customStepIsLoop")]
		public bool loopCustomStep;
	}
}
