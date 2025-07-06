using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000288 RID: 648
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectDecal.html")]
	[ExecuteInEditMode]
	public class EffectDecal : Effect
	{
		// Token: 0x06001E92 RID: 7826 RVA: 0x000CFD84 File Offset: 0x000CDF84
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.materialPropertyBlock = new MaterialPropertyBlock();
			this.meshRenderer = base.GetComponentInChildren<MeshRenderer>();
			if (EffectDecal.colorPropertyID == 0)
			{
				EffectDecal.colorPropertyID = Shader.PropertyToID("_Color");
			}
			if (EffectDecal.emissionPropertyID == 0)
			{
				EffectDecal.emissionPropertyID = Shader.PropertyToID("_EmissionColor");
			}
			if (EffectDecal.intensityPropertyID == 0)
			{
				EffectDecal.intensityPropertyID = Shader.PropertyToID("_Intensity");
			}
		}

		// Token: 0x06001E93 RID: 7827 RVA: 0x000CFDF8 File Offset: 0x000CDFF8
		private void Awake()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
			MeshFilter meshFilter = base.GetComponentInChildren<MeshFilter>();
			if (!meshFilter)
			{
				if (!EffectDecal.defaultCubeMesh)
				{
					MeshFilter component = GameObject.CreatePrimitive(PrimitiveType.Cube).GetComponent<MeshFilter>();
					EffectDecal.defaultCubeMesh = component.sharedMesh;
					UnityEngine.Object.Destroy(component.gameObject);
				}
				Transform meshTransform = base.transform.Find("Mesh");
				if (!meshTransform)
				{
					meshTransform = new GameObject("Mesh").transform;
					meshTransform.SetParent(base.transform);
					meshTransform.localPosition = Vector3.zero;
					meshTransform.rotation = Quaternion.LookRotation(base.transform.up, base.transform.forward);
					meshTransform.localScale = Vector3.one;
				}
				meshFilter = meshTransform.gameObject.AddComponent<MeshFilter>();
				meshFilter.mesh = EffectDecal.defaultCubeMesh;
			}
			this.meshRenderer = base.GetComponentInChildren<MeshRenderer>();
			if (!this.meshRenderer)
			{
				this.meshRenderer = meshFilter.gameObject.AddComponent<MeshRenderer>();
			}
			if (EffectDecal.colorPropertyID == 0)
			{
				EffectDecal.colorPropertyID = Shader.PropertyToID("_Color");
			}
			if (EffectDecal.emissionPropertyID == 0)
			{
				EffectDecal.emissionPropertyID = Shader.PropertyToID("_EmissionColor");
			}
			this.meshRenderer.enabled = false;
		}

		// Token: 0x06001E94 RID: 7828 RVA: 0x000CFF34 File Offset: 0x000CE134
		public override void Play()
		{
			this.playTime = Time.time;
			base.CancelInvoke();
			Transform transform = this.meshRenderer.transform;
			transform.localScale = Vector3.one;
			Vector3 lossyScale = transform.lossyScale;
			if (this.useSizeCurve)
			{
				float eval = this.sizeCurve.Evaluate(0f);
				lossyScale = new Vector3(eval / lossyScale.x * this.size.x, eval / lossyScale.y * this.size.y, eval / lossyScale.z * this.size.z);
			}
			else
			{
				lossyScale = new Vector3(this.size.x / lossyScale.x, this.size.y / lossyScale.y, this.size.z / lossyScale.z);
				float randomRange = UnityEngine.Random.Range(-this.sizeRandomRange, this.sizeRandomRange);
				lossyScale += new Vector3(randomRange, randomRange, randomRange);
			}
			transform.localScale = lossyScale;
			if (this.step == Effect.Step.Start || this.step == Effect.Step.End)
			{
				base.InvokeRepeating("UpdateLifeTime", 0f, this.fadeRefreshSpeed);
			}
			if (AreaManager.Instance)
			{
				LightVolumeReceiver.ApplyProbeVolume(this.meshRenderer, this.materialPropertyBlock);
			}
			else
			{
				LightVolumeReceiver.DisableProbeVolume(this.meshRenderer);
			}
			this.meshRenderer.enabled = true;
		}

		// Token: 0x06001E95 RID: 7829 RVA: 0x000D0090 File Offset: 0x000CE290
		public override void Stop()
		{
			this.meshRenderer.enabled = false;
		}

		// Token: 0x06001E96 RID: 7830 RVA: 0x000D009E File Offset: 0x000CE29E
		public override void End(bool loopOnly = false)
		{
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				this.Despawn();
			}
		}

		// Token: 0x06001E97 RID: 7831 RVA: 0x000D00B8 File Offset: 0x000CE2B8
		protected void UpdateLifeTime()
		{
			float baseValue = Mathf.Clamp01(1f - (Time.time - this.playTime) / this.baseLifeTime);
			if (this.meshRenderer.isVisible)
			{
				if (this.linkBaseColor != EffectTarget.None)
				{
					this.materialPropertyBlock.SetColor(EffectDecal.colorPropertyID, this.baseColorGradient.Evaluate(baseValue));
				}
				if (this.linkEmissionColor != EffectTarget.None)
				{
					float emissionValue = Mathf.Clamp01(1f - (Time.time - this.playTime) / this.emissionLifeTime);
					this.materialPropertyBlock.SetColor(EffectDecal.emissionPropertyID, this.emissionColorGradient.Evaluate(emissionValue));
				}
				if (this.linkBaseColor != EffectTarget.None || this.linkEmissionColor != EffectTarget.None)
				{
					this.meshRenderer.SetPropertyBlock(this.materialPropertyBlock);
				}
				if (this.useSizeCurve)
				{
					float eval = this.sizeCurve.Evaluate(Time.time - this.playTime);
					this.meshRenderer.transform.localScale = Vector3.one;
					this.meshRenderer.transform.localScale = new Vector3(eval / this.meshRenderer.transform.lossyScale.x * this.size.x, eval / this.meshRenderer.transform.lossyScale.y * this.size.y, eval / this.meshRenderer.transform.lossyScale.z * this.size.z);
				}
			}
			if (baseValue == 0f)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001E98 RID: 7832 RVA: 0x000D0240 File Offset: 0x000CE440
		public override void SetMainGradient(Gradient gradient)
		{
			if (this.linkBaseColor == EffectTarget.Main)
			{
				this.baseColorGradient = gradient;
			}
			if (this.linkEmissionColor == EffectTarget.Main)
			{
				this.emissionColorGradient = gradient;
			}
		}

		// Token: 0x06001E99 RID: 7833 RVA: 0x000D0262 File Offset: 0x000CE462
		public override void SetSecondaryGradient(Gradient gradient)
		{
			if (this.linkBaseColor == EffectTarget.Secondary)
			{
				this.baseColorGradient = gradient;
			}
			if (this.linkEmissionColor == EffectTarget.Secondary)
			{
				this.emissionColorGradient = gradient;
			}
		}

		// Token: 0x06001E9A RID: 7834 RVA: 0x000D0284 File Offset: 0x000CE484
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				if (this.linkBaseColor != EffectTarget.None)
				{
					this.materialPropertyBlock.SetColor(EffectDecal.colorPropertyID, this.baseColorGradient.Evaluate(value));
				}
				if (this.linkEmissionColor != EffectTarget.None)
				{
					this.materialPropertyBlock.SetColor(EffectDecal.emissionPropertyID, this.emissionColorGradient.Evaluate(value));
				}
				if (this.linkBaseColor != EffectTarget.None || this.linkEmissionColor != EffectTarget.None)
				{
					this.meshRenderer.SetPropertyBlock(this.materialPropertyBlock);
				}
				if (this.useSizeCurve)
				{
					float eval = this.sizeCurve.Evaluate(value);
					this.meshRenderer.transform.localScale = Vector3.one;
					this.meshRenderer.transform.localScale = new Vector3(eval / this.meshRenderer.transform.lossyScale.x * this.size.x, eval / this.meshRenderer.transform.lossyScale.y * this.size.y, eval / this.meshRenderer.transform.lossyScale.z * this.size.z);
				}
				if (this.meshRenderer.material.HasFloat(EffectDecal.intensityPropertyID))
				{
					this.meshRenderer.material.SetFloat(EffectDecal.intensityPropertyID, value);
				}
			}
		}

		// Token: 0x06001E9B RID: 7835 RVA: 0x000D03EF File Offset: 0x000CE5EF
		public override void CollisionStay(Vector3 position, Quaternion rotation, float intensity)
		{
		}

		// Token: 0x06001E9C RID: 7836 RVA: 0x000D03F1 File Offset: 0x000CE5F1
		public override void Despawn()
		{
			base.CancelInvoke();
			this.meshRenderer.enabled = false;
			if (Application.isPlaying)
			{
				EffectModuleDecal.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001D1E RID: 7454
		[NonSerialized]
		public MeshRenderer meshRenderer;

		// Token: 0x04001D1F RID: 7455
		protected static int colorPropertyID;

		// Token: 0x04001D20 RID: 7456
		protected static int emissionPropertyID;

		// Token: 0x04001D21 RID: 7457
		protected static int intensityPropertyID;

		// Token: 0x04001D22 RID: 7458
		protected static Mesh defaultCubeMesh;

		// Token: 0x04001D23 RID: 7459
		protected MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04001D24 RID: 7460
		public float baseLifeTime = 60f;

		// Token: 0x04001D25 RID: 7461
		public float emissionLifeTime = 10f;

		// Token: 0x04001D26 RID: 7462
		public float fadeRefreshSpeed = 0.1f;

		// Token: 0x04001D27 RID: 7463
		[NonSerialized]
		public float playTime;

		// Token: 0x04001D28 RID: 7464
		[Header("Size")]
		public Vector3 size = Vector3.one;

		// Token: 0x04001D29 RID: 7465
		public float sizeRandomRange;

		// Token: 0x04001D2A RID: 7466
		public bool useSizeCurve;

		// Token: 0x04001D2B RID: 7467
		public AnimationCurve sizeCurve;

		// Token: 0x04001D2C RID: 7468
		[Header("Gradient")]
		public EffectTarget linkBaseColor;

		// Token: 0x04001D2D RID: 7469
		public EffectTarget linkEmissionColor;

		// Token: 0x04001D2E RID: 7470
		[GradientUsage(true)]
		public Gradient baseColorGradient;

		// Token: 0x04001D2F RID: 7471
		[GradientUsage(true)]
		public Gradient emissionColorGradient;
	}
}
