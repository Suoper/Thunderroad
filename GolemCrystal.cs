using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200025F RID: 607
	public class GolemCrystal : SimpleBreakable
	{
		// Token: 0x06001B5F RID: 7007 RVA: 0x000B543B File Offset: 0x000B363B
		protected override void Awake()
		{
			base.Awake();
			this.originalAllowedTypes = this.allowedDamageTypes;
		}

		// Token: 0x06001B60 RID: 7008 RVA: 0x000B5450 File Offset: 0x000B3650
		private void Start()
		{
			VfxPlayer vfxPlayer = this.passiveVfxPlayer;
			if (vfxPlayer != null)
			{
				vfxPlayer.Stop();
			}
			this.hitEffectData = Catalog.GetData<EffectData>(this.hitEffectID, false);
			this.emissiveEffectData = Catalog.GetData<EffectData>(this.emissiveEffectID, false);
			for (int i = 0; i < this.emissiveRenderers.Count; i++)
			{
				if (!(this.emissiveRenderers[i] == null))
				{
					EffectInstance emissiveEffect = this.emissiveEffectData.Spawn(base.transform.position, base.transform.rotation, base.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					emissiveEffect.SetRenderer(this.emissiveRenderers[i], true);
					emissiveEffect.SetIntensity(1f);
					emissiveEffect.Play(0, false, false);
					this.emissiveEffects.Add(emissiveEffect);
				}
			}
		}

		// Token: 0x06001B61 RID: 7009 RVA: 0x000B5530 File Offset: 0x000B3730
		public virtual void EnableShield()
		{
			if (this.shieldActive)
			{
				return;
			}
			this.shieldActive = true;
			UnityEvent unityEvent = this.onShieldEnable;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			GameObject gameObject = this.shield;
			if (gameObject != null)
			{
				gameObject.SetActive(true);
			}
			this.allowedDamageTypes = SimpleBreakable.DamageType.None;
			VfxPlayer vfxPlayer = this.passiveVfxPlayer;
			if (vfxPlayer != null)
			{
				vfxPlayer.Stop();
			}
			this.targetEmissive = 0f;
		}

		// Token: 0x06001B62 RID: 7010 RVA: 0x000B5594 File Offset: 0x000B3794
		public virtual void DisableShield()
		{
			if (!this.shieldActive)
			{
				return;
			}
			this.shieldActive = false;
			UnityEvent unityEvent = this.onShieldDisable;
			if (unityEvent != null)
			{
				unityEvent.Invoke();
			}
			GameObject gameObject = this.shield;
			if (gameObject != null)
			{
				gameObject.SetActive(false);
			}
			this.allowedDamageTypes = this.originalAllowedTypes;
			VfxPlayer vfxPlayer = this.passiveVfxPlayer;
			if (vfxPlayer != null)
			{
				vfxPlayer.Play();
			}
			this.targetEmissive = 1f;
		}

		// Token: 0x06001B63 RID: 7011 RVA: 0x000B55FC File Offset: 0x000B37FC
		protected override bool EnterCollision(Collision collision)
		{
			if (!base.EnterCollision(collision))
			{
				return false;
			}
			if (this.hitEffectData != null)
			{
				this.hitEffectData.Spawn(collision.GetContact(0).point, Quaternion.LookRotation(UnityEngine.Random.insideUnitSphere), null, null, true, null, false, 1f, 1f, Array.Empty<Type>()).Play(0, false, false);
			}
			return true;
		}

		// Token: 0x06001B64 RID: 7012 RVA: 0x000B5660 File Offset: 0x000B3860
		private void LateUpdate()
		{
			this.currentEmissive = Mathf.MoveTowards(this.currentEmissive, this.targetEmissive, Time.deltaTime / this.emissiveToggleTime);
			foreach (EffectInstance effectInstance in this.emissiveEffects)
			{
				effectInstance.SetIntensity(this.currentEmissive);
			}
		}

		// Token: 0x040019FB RID: 6651
		[Header("Protection")]
		public GameObject shield;

		// Token: 0x040019FC RID: 6652
		public UnityEvent onShieldEnable;

		// Token: 0x040019FD RID: 6653
		public UnityEvent onShieldDisable;

		// Token: 0x040019FE RID: 6654
		public Transform linkEffect;

		// Token: 0x040019FF RID: 6655
		public Transform linkEffectTarget;

		// Token: 0x04001A00 RID: 6656
		public VfxPlayer passiveVfxPlayer;

		// Token: 0x04001A01 RID: 6657
		public string hitEffectID;

		// Token: 0x04001A02 RID: 6658
		public string emissiveEffectID;

		// Token: 0x04001A03 RID: 6659
		public float emissiveToggleTime = 0.5f;

		// Token: 0x04001A04 RID: 6660
		public List<Renderer> emissiveRenderers = new List<Renderer>();

		// Token: 0x04001A05 RID: 6661
		private bool shieldActive;

		// Token: 0x04001A06 RID: 6662
		private SimpleBreakable.DamageType originalAllowedTypes;

		// Token: 0x04001A07 RID: 6663
		private EffectData hitEffectData;

		// Token: 0x04001A08 RID: 6664
		private EffectData emissiveEffectData;

		// Token: 0x04001A09 RID: 6665
		private List<EffectInstance> emissiveEffects = new List<EffectInstance>();

		// Token: 0x04001A0A RID: 6666
		private float currentEmissive;

		// Token: 0x04001A0B RID: 6667
		private float targetEmissive = 1f;
	}
}
