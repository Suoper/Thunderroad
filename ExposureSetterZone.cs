using System;
using System.Collections;
using Shadowood;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ThunderRoad
{
	// Token: 0x020002C9 RID: 713
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/ExposureTransitionZone")]
	[RequireComponent(typeof(BoxCollider))]
	public class ExposureSetterZone : ThunderBehaviour
	{
		/// <summary>
		/// Reset the player exposure to 0
		/// </summary>
		// Token: 0x0600228B RID: 8843 RVA: 0x000EDBAC File Offset: 0x000EBDAC
		public static void ResetPlayerExposure()
		{
			Shadowood.Tonemapping.SetExposureStatic(0f);
			if (Player.local)
			{
				Volume processingVolume = Player.local.GetComponentInChildren<Volume>();
				ColorAdjustments colorAdjustments;
				if (processingVolume && processingVolume.profile && processingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
				{
					colorAdjustments.postExposure.value = 0f;
				}
			}
		}

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x0600228C RID: 8844 RVA: 0x000EDC10 File Offset: 0x000EBE10
		public Volume postProcessingVolume
		{
			get
			{
				if (this.localPostProcessingVolume == null && Player.local != null)
				{
					this.localPostProcessingVolume = Player.local.GetComponentInChildren<Volume>();
				}
				if (this.localPostProcessingVolume)
				{
					return this.localPostProcessingVolume;
				}
				return null;
			}
		}

		// Token: 0x0600228D RID: 8845 RVA: 0x000EDC5D File Offset: 0x000EBE5D
		public void SetExposureOnEnter()
		{
			this.SetTargetExposure(this.exposureOnEnter);
		}

		// Token: 0x0600228E RID: 8846 RVA: 0x000EDC6B File Offset: 0x000EBE6B
		public void ResetExposure()
		{
			if (this.exposureAdjustMode == ExposureSetterZone.ExposureAdjustMode.ShaderToneMapping)
			{
				this.SetTargetExposure(1f);
				return;
			}
			this.SetTargetExposure(0f);
		}

		// Token: 0x0600228F RID: 8847 RVA: 0x000EDC90 File Offset: 0x000EBE90
		protected virtual void Awake()
		{
			base.gameObject.layer = Common.zoneLayer;
			this.mainCollider = base.GetComponent<BoxCollider>();
			this.mainCollider.isTrigger = true;
			this.playerMask = 1 << GameManager.GetLayer(LayerName.PlayerLocomotion);
			PlayerSpawner.onSpawn = (Action<PlayerSpawner, EventTime>)Delegate.Combine(PlayerSpawner.onSpawn, new Action<PlayerSpawner, EventTime>(this.OnPlayerSpawnerEvent));
			QualityLevel qualityLevel = Common.GetQualityLevel(false);
			ExposureSetterZone.ExposureAdjustMode exposureAdjustMode;
			if (qualityLevel != QualityLevel.Windows)
			{
				if (qualityLevel == QualityLevel.Android)
				{
					exposureAdjustMode = ExposureSetterZone.ExposureAdjustMode.ShaderToneMapping;
				}
				else
				{
					exposureAdjustMode = this.exposureAdjustMode;
				}
			}
			else
			{
				exposureAdjustMode = ExposureSetterZone.ExposureAdjustMode.PostProcessingVolume;
			}
			this.exposureAdjustMode = exposureAdjustMode;
		}

		// Token: 0x06002290 RID: 8848 RVA: 0x000EDD1B File Offset: 0x000EBF1B
		private void OnPlayerSpawnerEvent(PlayerSpawner playerSpawner, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				return;
			}
			if (Player.local.creature)
			{
				this.localPostProcessingVolume = Player.local.GetComponentInChildren<Volume>();
			}
		}

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x06002291 RID: 8849 RVA: 0x000EDD42 File Offset: 0x000EBF42
		// (set) Token: 0x06002292 RID: 8850 RVA: 0x000EDD4A File Offset: 0x000EBF4A
		public bool playerInZone { get; protected set; }

		// Token: 0x06002293 RID: 8851 RVA: 0x000EDD54 File Offset: 0x000EBF54
		private void OnValidate()
		{
			BoxCollider collider;
			if (base.TryGetComponent<BoxCollider>(out collider))
			{
				this.mainCollider = collider;
			}
			if (this.mainCollider != null)
			{
				this.mainCollider.size = this.size;
			}
		}

		// Token: 0x06002294 RID: 8852 RVA: 0x000EDD91 File Offset: 0x000EBF91
		protected override void ManagedOnDisable()
		{
			this.mainCollider.size = Vector3.zero;
		}

		// Token: 0x06002295 RID: 8853 RVA: 0x000EDDA3 File Offset: 0x000EBFA3
		protected override void ManagedOnEnable()
		{
			this.mainCollider = base.GetComponent<BoxCollider>();
			this.mainCollider.size = this.size;
		}

		// Token: 0x06002296 RID: 8854 RVA: 0x000EDDC2 File Offset: 0x000EBFC2
		public void CancelTransition()
		{
			ExposureSetterZone.isBusy = false;
			base.StopAllCoroutines();
			this.transitionCoroutine = null;
		}

		// Token: 0x06002297 RID: 8855 RVA: 0x000EDDD8 File Offset: 0x000EBFD8
		protected float GetCurrentExposure()
		{
			if (this.exposureAdjustMode == ExposureSetterZone.ExposureAdjustMode.ShaderToneMapping)
			{
				return Mathf.Log(Shader.GetGlobalVector(ExposureSetterZone.TonemappingSettings).x, 2f);
			}
			Volume volume = this.postProcessingVolume;
			ColorAdjustments colorAdjustments;
			if (volume && volume.profile && volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
			{
				return colorAdjustments.postExposure.value;
			}
			return 0f;
		}

		// Token: 0x06002298 RID: 8856 RVA: 0x000EDE44 File Offset: 0x000EC044
		protected void SetTargetExposure(float exposure)
		{
			if (this.exposureAdjustMode == ExposureSetterZone.ExposureAdjustMode.ShaderToneMapping)
			{
				Shadowood.Tonemapping.SetExposureStatic(exposure);
				return;
			}
			Volume processingVolume = this.postProcessingVolume;
			if (processingVolume == null)
			{
				Debug.LogError("No post processing volume found on the player or the zone, cannot set exposure: " + base.gameObject.GetPathFromRoot());
				return;
			}
			ColorAdjustments colorAdjustments;
			if (processingVolume && processingVolume.profile && processingVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
			{
				colorAdjustments.postExposure.value = exposure;
			}
		}

		// Token: 0x06002299 RID: 8857 RVA: 0x000EDEBC File Offset: 0x000EC0BC
		protected IEnumerator TransitionExposure()
		{
			ExposureSetterZone.isBusy = true;
			float currentExposure = this.GetCurrentExposure();
			while (Math.Abs(currentExposure - this.targetExposure) > 0.01f)
			{
				float speed = (this.targetExposure > currentExposure) ? this.speedDown : this.speedUp;
				this.SetTargetExposure(Mathf.Lerp(currentExposure, this.targetExposure, speed * Time.deltaTime));
				currentExposure = this.GetCurrentExposure();
				yield return null;
			}
			this.SetTargetExposure(this.targetExposure);
			this.transitionCoroutine = null;
			ExposureSetterZone.isBusy = false;
			yield break;
		}

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x0600229A RID: 8858 RVA: 0x000EDECB File Offset: 0x000EC0CB
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return (ManagedLoops)0;
			}
		}

		// Token: 0x0600229B RID: 8859 RVA: 0x000EDECE File Offset: 0x000EC0CE
		private void OnTriggerEnter(Collider other)
		{
			this.HandleTriggerChange(other, true);
		}

		// Token: 0x0600229C RID: 8860 RVA: 0x000EDED8 File Offset: 0x000EC0D8
		private void OnTriggerExit(Collider other)
		{
			this.HandleTriggerChange(other, false);
		}

		// Token: 0x0600229D RID: 8861 RVA: 0x000EDEE2 File Offset: 0x000EC0E2
		protected bool IsInLayerMask(int layerMask, int layer)
		{
			return layerMask == (layerMask | 1 << layer);
		}

		// Token: 0x0600229E RID: 8862 RVA: 0x000EDEF0 File Offset: 0x000EC0F0
		protected virtual void HandleTriggerChange(Collider collider, bool enter)
		{
			PhysicBody physicBody = collider.GetPhysicBody();
			int objectLayer = (physicBody != null) ? physicBody.gameObject.layer : collider.gameObject.layer;
			if (!this.IsInLayerMask(this.playerMask, objectLayer))
			{
				return;
			}
			Vector3 pos = collider.gameObject.transform.position;
			Vector3 forward = base.transform.forward;
			Vector3 toCollider = base.transform.position - pos;
			ExposureSetterZone.Direction direction = (Vector3.Dot(forward, toCollider) > 0f) ? ExposureSetterZone.Direction.Exit : ExposureSetterZone.Direction.Enter;
			this.PlayerZoneChange(enter, direction);
		}

		// Token: 0x0600229F RID: 8863 RVA: 0x000EDF78 File Offset: 0x000EC178
		protected virtual void PlayerZoneChange(bool enter, ExposureSetterZone.Direction direction)
		{
			if (enter && this.playerInZone)
			{
				return;
			}
			this.playerInZone = enter;
			if (!enter)
			{
				return;
			}
			ExposureSetterZone.activeZone = this;
			this.targetExposure = this.exposureOnEnter;
			if (this.transitionCoroutine != null)
			{
				base.StopCoroutine(this.transitionCoroutine);
			}
			if (this.transitionCoroutine == null)
			{
				this.transitionCoroutine = base.StartCoroutine(this.TransitionExposure());
			}
		}

		// Token: 0x040021B0 RID: 8624
		public static bool isBusy;

		// Token: 0x040021B1 RID: 8625
		public static ExposureSetterZone activeZone;

		// Token: 0x040021B2 RID: 8626
		public static readonly int TonemappingSettings = Shader.PropertyToID("_TonemappingSettings");

		// Token: 0x040021B3 RID: 8627
		[Header("Portal")]
		public Vector3 size = Vector3.one;

		// Token: 0x040021B4 RID: 8628
		[Header("Exposure")]
		[Tooltip("The exposure adjust mode to use.")]
		public ExposureSetterZone.ExposureAdjustMode exposureAdjustMode;

		// Token: 0x040021B5 RID: 8629
		[Tooltip("The post processing volume to use for exposure adjustment.")]
		public Volume localPostProcessingVolume;

		// Token: 0x040021B6 RID: 8630
		[Tooltip("The exposure rate of change when transitioning to a brighter zone")]
		public float speedUp = 3f;

		// Token: 0x040021B7 RID: 8631
		[Tooltip("The exposure rate of change  when transitioning to a darker zone")]
		public float speedDown = 1f;

		// Token: 0x040021B8 RID: 8632
		[Tooltip("The exposure to set when the player enters zone via green portal.")]
		public float exposureOnEnter = 1f;

		// Token: 0x040021B9 RID: 8633
		protected BoxCollider mainCollider;

		// Token: 0x040021BA RID: 8634
		protected int playerMask;

		// Token: 0x040021BC RID: 8636
		protected Coroutine transitionCoroutine;

		// Token: 0x040021BD RID: 8637
		protected float targetExposure;

		// Token: 0x020009AB RID: 2475
		public enum ExposureAdjustMode
		{
			// Token: 0x04004570 RID: 17776
			PostProcessingVolume,
			// Token: 0x04004571 RID: 17777
			ShaderToneMapping
		}

		// Token: 0x020009AC RID: 2476
		protected enum Direction
		{
			// Token: 0x04004573 RID: 17779
			Exit,
			// Token: 0x04004574 RID: 17780
			Enter
		}
	}
}
