using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200028F RID: 655
	public class EffectSpawner : MonoBehaviour
	{
		// Token: 0x06001EE0 RID: 7904 RVA: 0x000D27C5 File Offset: 0x000D09C5
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06001EE1 RID: 7905 RVA: 0x000D27D4 File Offset: 0x000D09D4
		private void OnValidate()
		{
			if (!this.InPrefabScene() && this.effectInstance != null)
			{
				this.effectInstance.SetIntensity(this.intensity);
				this.effectInstance.SetSpeed(this.speed);
				if (this.useMainGradient)
				{
					this.effectInstance.SetMainGradient(this.mainGradient);
				}
				if (this.useSecondaryGradient)
				{
					this.effectInstance.SetSecondaryGradient(this.secondaryGradient);
				}
			}
		}

		// Token: 0x06001EE2 RID: 7906 RVA: 0x000D2845 File Offset: 0x000D0A45
		private void Awake()
		{
		}

		// Token: 0x06001EE3 RID: 7907 RVA: 0x000D2847 File Offset: 0x000D0A47
		protected void Start()
		{
			if (this.spawnOnStart)
			{
				this.Spawn();
			}
		}

		// Token: 0x06001EE4 RID: 7908 RVA: 0x000D2858 File Offset: 0x000D0A58
		protected void Update()
		{
			if (this.autoIntensity && this.effectInstance != null && this.effectInstance.effects.Count > 0)
			{
				this.intensity = Mathf.Clamp01(this.intensityCurve.Evaluate(Time.time - this.playTime));
				this.effectInstance.SetIntensity(this.intensity);
			}
		}

		// Token: 0x06001EE5 RID: 7909 RVA: 0x000D28BB File Offset: 0x000D0ABB
		public IEnumerator SpawnCoroutine(EffectData effectData)
		{
			List<Coroutine> coroutines = new List<Coroutine>();
			foreach (EffectModule effectModule in effectData.modules)
			{
				if (effectModule.CheckQualityLevel())
				{
					effectModule.OnCatalogRefresh(effectData, this.editorLoad);
					coroutines.Add(base.StartCoroutine(effectModule.RefreshCoroutine(effectData, this.editorLoad)));
				}
			}
			foreach (Coroutine refreshCoroutine in coroutines)
			{
				yield return refreshCoroutine;
			}
			List<Coroutine>.Enumerator enumerator2 = default(List<Coroutine>.Enumerator);
			if (this.autoIntensity)
			{
				this.intensity = Mathf.Clamp01(this.intensityCurve.Evaluate(0f));
			}
			this.effectInstance = effectData.Spawn(base.transform.position, base.transform.rotation, base.transform, null, true, null, false, this.intensity, this.speed, Array.Empty<Type>());
			if (this.useMainGradient)
			{
				this.effectInstance.SetMainGradient(this.mainGradient);
			}
			if (this.useSecondaryGradient)
			{
				this.effectInstance.SetSecondaryGradient(this.secondaryGradient);
			}
			if (this.source)
			{
				this.effectInstance.SetSource(this.source);
			}
			if (this.target)
			{
				this.effectInstance.SetTarget(this.target);
			}
			if (this.mesh)
			{
				this.effectInstance.SetMesh(this.mesh);
			}
			if (this.mainRenderer)
			{
				this.effectInstance.SetRenderer(this.mainRenderer, false);
			}
			if (this.secondaryRenderer)
			{
				this.effectInstance.SetRenderer(this.secondaryRenderer, true);
			}
			if (this.collider)
			{
				this.effectInstance.SetCollider(this.collider);
			}
			this.effectInstance.Play(0, false, false);
			this.playTime = Time.time;
			yield break;
			yield break;
		}

		// Token: 0x06001EE6 RID: 7910 RVA: 0x000D28D1 File Offset: 0x000D0AD1
		public void Stop()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (this.effectInstance != null)
			{
				this.effectInstance.End(false, -1f);
			}
			this.effectInstance = null;
		}

		// Token: 0x06001EE7 RID: 7911 RVA: 0x000D28FC File Offset: 0x000D0AFC
		public void Spawn()
		{
			if (!Application.isPlaying)
			{
				Debug.LogError("Press play to use the effect Spawner!");
				return;
			}
			this.Stop();
			if (this.effectId != "" && this.effectId != null)
			{
				base.StartCoroutine(this.SpawnCoroutine(Catalog.GetData<EffectData>(this.effectId, true)));
			}
		}

		// Token: 0x04001DC0 RID: 7616
		public string effectId;

		// Token: 0x04001DC1 RID: 7617
		public bool autoIntensity;

		// Token: 0x04001DC2 RID: 7618
		public AnimationCurve intensityCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001DC3 RID: 7619
		protected float playTime;

		// Token: 0x04001DC4 RID: 7620
		public bool spawnOnStart = true;

		// Token: 0x04001DC5 RID: 7621
		public bool editorLoad = true;

		// Token: 0x04001DC6 RID: 7622
		[Range(0f, 1f)]
		public float intensity;

		// Token: 0x04001DC7 RID: 7623
		[Range(0f, 1f)]
		public float speed;

		// Token: 0x04001DC8 RID: 7624
		public bool useMainGradient;

		// Token: 0x04001DC9 RID: 7625
		[GradientUsage(true)]
		public Gradient mainGradient;

		// Token: 0x04001DCA RID: 7626
		public bool useSecondaryGradient;

		// Token: 0x04001DCB RID: 7627
		[GradientUsage(true)]
		public Gradient secondaryGradient;

		// Token: 0x04001DCC RID: 7628
		public Transform source;

		// Token: 0x04001DCD RID: 7629
		public Transform target;

		// Token: 0x04001DCE RID: 7630
		public Mesh mesh;

		// Token: 0x04001DCF RID: 7631
		public Renderer mainRenderer;

		// Token: 0x04001DD0 RID: 7632
		public Renderer secondaryRenderer;

		// Token: 0x04001DD1 RID: 7633
		public Collider collider;

		// Token: 0x04001DD2 RID: 7634
		protected EffectInstance effectInstance;
	}
}
