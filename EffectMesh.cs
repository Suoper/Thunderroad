using System;
using System.Collections;
using ThunderRoad.Pools;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200028A RID: 650
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectMesh.html")]
	public class EffectMesh : Effect
	{
		// Token: 0x06001EA8 RID: 7848 RVA: 0x000D08A8 File Offset: 0x000CEAA8
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.materialPropertyBlock = new MaterialPropertyBlock();
			this.meshRenderer = base.GetComponent<MeshRenderer>();
		}

		// Token: 0x06001EA9 RID: 7849 RVA: 0x000D08D0 File Offset: 0x000CEAD0
		private void Awake()
		{
			this.materialPropertyBlock = new MaterialPropertyBlock();
			this.meshFilter = base.GetComponent<MeshFilter>();
			if (!this.meshFilter)
			{
				this.meshFilter = base.gameObject.AddComponent<MeshFilter>();
			}
			this.meshRenderer = base.GetComponent<MeshRenderer>();
			if (!this.meshRenderer)
			{
				this.meshRenderer = base.gameObject.AddComponent<MeshRenderer>();
			}
			this.meshRenderer.enabled = false;
		}

		// Token: 0x06001EAA RID: 7850 RVA: 0x000D0948 File Offset: 0x000CEB48
		private IEnumerator MeshSizeFadeCoroutine(bool fadeIn = true)
		{
			this.meshSizeFading = true;
			float time = 0f;
			while (time < this.meshSizeFadeDuration)
			{
				if (this.meshSizeFromIntensity)
				{
					float meshSizeValue = this.curveMeshSize.Evaluate(this.currentValue);
					if (fadeIn)
					{
						base.transform.localScale = Vector3.Lerp(Vector3.zero, new Vector3(meshSizeValue, meshSizeValue, meshSizeValue), time / this.meshSizeFadeDuration);
					}
					else
					{
						base.transform.localScale = Vector3.Lerp(new Vector3(meshSizeValue, meshSizeValue, meshSizeValue), Vector3.zero, time / this.meshSizeFadeDuration);
					}
				}
				else if (fadeIn)
				{
					base.transform.localScale = Vector3.Lerp(Vector3.zero, this.meshSize, time / this.meshSizeFadeDuration);
				}
				else
				{
					base.transform.localScale = Vector3.Lerp(this.meshSize, Vector3.zero, time / this.meshSizeFadeDuration);
				}
				time += Time.deltaTime;
				yield return Yielders.EndOfFrame;
			}
			base.transform.localScale = this.meshSize;
			this.meshSizeFading = false;
			yield return true;
			yield break;
		}

		// Token: 0x06001EAB RID: 7851 RVA: 0x000D095E File Offset: 0x000CEB5E
		private IEnumerator MeshRotationFadeCoroutine(bool fadeIn = true)
		{
			this.meshRotationFading = true;
			float time = 0f;
			while (time < this.meshRotationFadeDuration)
			{
				if (this.meshRotationFromIntensity)
				{
					float meshRotYValue = this.curveMeshrotY.Evaluate(this.currentValue);
					if (fadeIn)
					{
						base.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(base.transform.localEulerAngles.x, meshRotYValue, base.transform.localEulerAngles.x), time / this.meshSizeFadeDuration);
					}
					base.transform.localEulerAngles = Vector3.Lerp(new Vector3(base.transform.localEulerAngles.x, meshRotYValue, base.transform.localEulerAngles.x), Vector3.zero, time / this.meshSizeFadeDuration);
				}
				else
				{
					if (fadeIn)
					{
						base.transform.localEulerAngles = Vector3.Lerp(Vector3.zero, this.meshRotation, time / this.meshSizeFadeDuration);
					}
					base.transform.localEulerAngles = Vector3.Lerp(this.meshRotation, Vector3.zero, time / this.meshSizeFadeDuration);
				}
				time += Time.deltaTime;
				yield return Yielders.EndOfFrame;
			}
			this.meshRotationFading = false;
			yield return true;
			yield break;
		}

		// Token: 0x06001EAC RID: 7852 RVA: 0x000D0974 File Offset: 0x000CEB74
		private float GetLastTime(AnimationCurve animationCurve)
		{
			if (animationCurve.length != 0)
			{
				return animationCurve[animationCurve.length - 1].time;
			}
			return 0f;
		}

		// Token: 0x06001EAD RID: 7853 RVA: 0x000D09A8 File Offset: 0x000CEBA8
		public override void Play()
		{
			base.CancelInvoke();
			this.playTime = Time.time;
			if (this.meshRenderer != null)
			{
				this.meshRenderer.enabled = true;
			}
			if (this.linkEmissionColor != EffectTarget.None && this.meshRenderer != null)
			{
				Material[] materials = this.meshRenderer.materials;
				for (int i = 0; i < materials.Length; i++)
				{
					materials[i].EnableKeyword("_EMISSION");
				}
			}
			if ((this.step == Effect.Step.Start || this.step == Effect.Step.End) && this.lifeTime > 0f)
			{
				base.InvokeRepeating("UpdateLifeTime", 0f, this.refreshSpeed);
			}
			if (this.meshSizeFadeDuration > 0f)
			{
				this.meshSizeFadeCoroutine = base.StartCoroutine(this.MeshSizeFadeCoroutine(true));
			}
			if (this.meshRotationFadeDuration > 0f)
			{
				this.meshRotationFadeCoroutine = base.StartCoroutine(this.MeshRotationFadeCoroutine(true));
			}
			base.transform.localPosition = this.meshPosition;
		}

		// Token: 0x06001EAE RID: 7854 RVA: 0x000D0AA2 File Offset: 0x000CECA2
		public override void Stop()
		{
			if (this.meshRenderer != null)
			{
				this.meshRenderer.enabled = false;
			}
		}

		// Token: 0x06001EAF RID: 7855 RVA: 0x000D0AC0 File Offset: 0x000CECC0
		public override void End(bool loopOnly = false)
		{
			if (this.meshSizeFadeDuration > 0f)
			{
				base.StopCoroutine(this.meshSizeFadeCoroutine);
				this.meshSizeFadeCoroutine = base.StartCoroutine(this.MeshSizeFadeCoroutine(false));
			}
			if (this.meshRotationFadeDuration > 0f)
			{
				base.StopCoroutine(this.meshRotationFadeCoroutine);
				this.meshRotationFadeCoroutine = base.StartCoroutine(this.MeshRotationFadeCoroutine(false));
			}
			if (this.meshSizeFadeDuration > 0f || this.meshRotationFadeDuration > 0f)
			{
				base.Invoke("Despawn", this.meshSizeFadeDuration);
				return;
			}
			if (this.meshRenderer != null)
			{
				this.meshRenderer.enabled = false;
			}
			this.Despawn();
		}

		// Token: 0x06001EB0 RID: 7856 RVA: 0x000D0B74 File Offset: 0x000CED74
		protected void UpdateLifeTime()
		{
			float value = Mathf.Clamp01(1f - (Time.time - this.playTime) / this.lifeTime);
			this.SetIntensity(value, false);
			if (value == 0f)
			{
				this.Despawn();
			}
		}

		// Token: 0x06001EB1 RID: 7857 RVA: 0x000D0BB8 File Offset: 0x000CEDB8
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				this.currentValue = this.intensityCurve.Evaluate(value);
				if (this.meshSizeFromIntensity && !this.meshSizeFading)
				{
					float meshSizeValue = this.curveMeshSize.Evaluate(this.currentValue);
					base.transform.localScale = new Vector3(meshSizeValue, meshSizeValue, meshSizeValue);
				}
				if (this.meshRotationFromIntensity && !this.meshRotationFading)
				{
					float meshRotYValue = this.curveMeshrotY.Evaluate(this.currentValue);
					base.transform.localEulerAngles = new Vector3(base.transform.localEulerAngles.x, meshRotYValue, base.transform.localEulerAngles.z);
				}
				bool updatePropertyBlock = false;
				if (this.linkTintColor == EffectTarget.Main && this.currentMainGradient != null)
				{
					this.materialPropertyBlock.SetColor("_TintColor", this.currentMainGradient.Evaluate(this.currentValue));
					updatePropertyBlock = true;
				}
				else if (this.linkTintColor == EffectTarget.Secondary && this.currentSecondaryGradient != null)
				{
					this.materialPropertyBlock.SetColor("_TintColor", this.currentSecondaryGradient.Evaluate(this.currentValue));
					updatePropertyBlock = true;
				}
				else if (this.linkBaseColor == EffectTarget.Main && this.currentMainGradient != null)
				{
					this.materialPropertyBlock.SetColor("_BaseColor", this.currentMainGradient.Evaluate(this.currentValue));
					updatePropertyBlock = true;
				}
				else if (this.linkBaseColor == EffectTarget.Secondary && this.currentSecondaryGradient != null)
				{
					this.materialPropertyBlock.SetColor("_BaseColor", this.currentSecondaryGradient.Evaluate(this.currentValue));
					updatePropertyBlock = true;
				}
				if (this.linkEmissionColor == EffectTarget.Main && this.currentMainGradient != null)
				{
					this.materialPropertyBlock.SetColor("_EmissionColor", this.currentMainGradient.Evaluate(this.currentValue));
					updatePropertyBlock = true;
				}
				else if (this.linkEmissionColor == EffectTarget.Secondary && this.currentSecondaryGradient != null)
				{
					this.materialPropertyBlock.SetColor("_EmissionColor", this.currentSecondaryGradient.Evaluate(this.currentValue));
					updatePropertyBlock = true;
				}
				if (this.meshRenderer != null && updatePropertyBlock)
				{
					this.meshRenderer.SetPropertyBlock(this.materialPropertyBlock);
				}
			}
		}

		// Token: 0x06001EB2 RID: 7858 RVA: 0x000D0DE2 File Offset: 0x000CEFE2
		public override void SetMainGradient(Gradient gradient)
		{
			this.currentMainGradient = gradient;
			this.SetIntensity(this.currentValue, false);
		}

		// Token: 0x06001EB3 RID: 7859 RVA: 0x000D0DF8 File Offset: 0x000CEFF8
		public override void SetSecondaryGradient(Gradient gradient)
		{
			this.currentSecondaryGradient = gradient;
			this.SetIntensity(this.currentValue, false);
		}

		// Token: 0x06001EB4 RID: 7860 RVA: 0x000D0E10 File Offset: 0x000CF010
		public override void Despawn()
		{
			base.StopCoroutine(this.MeshSizeFadeCoroutine(true));
			base.StopCoroutine(this.MeshRotationFadeCoroutine(true));
			this.meshSizeFading = false;
			this.meshRotationFading = false;
			base.CancelInvoke();
			if (this.meshRenderer != null)
			{
				this.meshRenderer.enabled = false;
			}
			if (Application.isPlaying)
			{
				EffectModuleMesh.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001D44 RID: 7492
		[NonSerialized]
		public EffectMeshPoolManager poolManager;

		// Token: 0x04001D45 RID: 7493
		public int poolCount = 20;

		// Token: 0x04001D46 RID: 7494
		public float lifeTime = 5f;

		// Token: 0x04001D47 RID: 7495
		public float refreshSpeed = 0.1f;

		// Token: 0x04001D48 RID: 7496
		public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f, 1f, 1f),
			new Keyframe(1f, 1f, 1f, 1f)
		});

		// Token: 0x04001D49 RID: 7497
		[NonSerialized]
		public float playTime;

		// Token: 0x04001D4A RID: 7498
		[Header("Color Gradient")]
		public EffectTarget linkBaseColor;

		// Token: 0x04001D4B RID: 7499
		public EffectTarget linkTintColor;

		// Token: 0x04001D4C RID: 7500
		public EffectTarget linkEmissionColor;

		// Token: 0x04001D4D RID: 7501
		public Vector3 meshSize = Vector3.one;

		// Token: 0x04001D4E RID: 7502
		public float meshSizeFadeDuration;

		// Token: 0x04001D4F RID: 7503
		protected bool meshSizeFading;

		// Token: 0x04001D50 RID: 7504
		protected Coroutine meshSizeFadeCoroutine;

		// Token: 0x04001D51 RID: 7505
		public Vector3 meshRotation;

		// Token: 0x04001D52 RID: 7506
		public Vector3 meshPosition;

		// Token: 0x04001D53 RID: 7507
		public float meshRotationFadeDuration;

		// Token: 0x04001D54 RID: 7508
		protected bool meshRotationFading;

		// Token: 0x04001D55 RID: 7509
		protected Coroutine meshRotationFadeCoroutine;

		// Token: 0x04001D56 RID: 7510
		[Header("Intensity to mesh size")]
		public bool meshSizeFromIntensity;

		// Token: 0x04001D57 RID: 7511
		public AnimationCurve curveMeshSize;

		// Token: 0x04001D58 RID: 7512
		[Header("Intensity to mesh rotation Y")]
		public bool meshRotationFromIntensity;

		// Token: 0x04001D59 RID: 7513
		public AnimationCurve curveMeshrotY;

		// Token: 0x04001D5A RID: 7514
		[NonSerialized]
		public float currentValue;

		// Token: 0x04001D5B RID: 7515
		[GradientUsage(true)]
		[NonSerialized]
		public Gradient currentMainGradient;

		// Token: 0x04001D5C RID: 7516
		[GradientUsage(true)]
		[NonSerialized]
		public Gradient currentSecondaryGradient;

		// Token: 0x04001D5D RID: 7517
		[NonSerialized]
		public MaterialPropertyBlock materialPropertyBlock;

		// Token: 0x04001D5E RID: 7518
		[NonSerialized]
		public MeshFilter meshFilter;

		// Token: 0x04001D5F RID: 7519
		[NonSerialized]
		public MeshRenderer meshRenderer;
	}
}
