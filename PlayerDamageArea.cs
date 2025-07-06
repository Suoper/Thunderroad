using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002E8 RID: 744
	public class PlayerDamageArea : MonoBehaviour
	{
		// Token: 0x060023CE RID: 9166 RVA: 0x000F529C File Offset: 0x000F349C
		private void OnValidate()
		{
			this.ConfigureAudioSource();
			base.gameObject.layer = Common.GetLayer(LayerName.Zone);
			this.triggerCollider = base.GetComponent<Collider>();
			if (!this.triggerCollider)
			{
				Debug.LogErrorFormat(this, "PlayerDamageArea need a trigger collider!", Array.Empty<object>());
				return;
			}
			this.triggerCollider.isTrigger = true;
		}

		// Token: 0x060023CF RID: 9167 RVA: 0x000F52F7 File Offset: 0x000F34F7
		private void Awake()
		{
			this.ConfigureAudioSource();
			this.playerlayer = Common.GetLayer(LayerName.PlayerLocomotion);
			base.gameObject.layer = Common.GetLayer(LayerName.Zone);
		}

		// Token: 0x060023D0 RID: 9168 RVA: 0x000F5320 File Offset: 0x000F3520
		private void ConfigureAudioSource()
		{
			this.audioSource = base.GetComponent<AudioSource>();
			if (!this.audioSource)
			{
				this.audioSource = base.gameObject.AddComponent<AudioSource>();
			}
			this.audioSource.hideFlags = HideFlags.HideInInspector;
			this.audioSource.playOnAwake = false;
			this.audioSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Voice);
		}

		// Token: 0x060023D1 RID: 9169 RVA: 0x000F5380 File Offset: 0x000F3580
		private void OnDisable()
		{
			base.CancelInvoke();
		}

		// Token: 0x060023D2 RID: 9170 RVA: 0x000F5388 File Offset: 0x000F3588
		private void OnTriggerEnter(Collider other)
		{
			if (!this.playerDetected && other.gameObject.layer == this.playerlayer)
			{
				this.playerHead = other.GetComponentInParent<PlayerHead>();
				if (this.playerHead)
				{
					this.playerCollider = other;
					this.playerDetected = true;
					base.CancelInvoke();
					base.InvokeRepeating("UpdateDamage", this.damageStartupDelay, this.damageInterval);
					if (this.attractionForce.magnitude > 0f)
					{
						base.StartCoroutine(this.AttractionCoroutine());
					}
				}
			}
		}

		// Token: 0x060023D3 RID: 9171 RVA: 0x000F5413 File Offset: 0x000F3613
		private void OnTriggerExit(Collider other)
		{
			if (this.playerDetected && other == this.playerCollider)
			{
				this.playerDetected = false;
				base.CancelInvoke();
				if (this.attractionForce.magnitude > 0f)
				{
					base.StopAllCoroutines();
				}
			}
		}

		// Token: 0x060023D4 RID: 9172 RVA: 0x000F5450 File Offset: 0x000F3650
		private void UpdateDamage()
		{
			if (Player.local && Player.local.hasCreature)
			{
				Player.local.creature.Damage(this.damageValue, this.damageType);
				if (this.damageAudioContainer)
				{
					this.audioSource.PlayOneShot(this.damageAudioContainer.PickAudioClip());
				}
				if (Player.local.creature.eyesUnderwater)
				{
					EffectData drownEffectData = Player.local.creature.data.drownEffectData;
					EffectInstance effectInstance = (drownEffectData != null) ? drownEffectData.Spawn(Player.local.creature.jaw.position, Player.local.creature.jaw.rotation, Player.local.creature.jaw, null, true, null, false, 1f, 1f, Array.Empty<Type>()) : null;
					if (effectInstance == null)
					{
						return;
					}
					effectInstance.Play(0, false, false);
					return;
				}
				else
				{
					if (this.hurtMaleAudioContainer && Player.local.creature.data.gender == CreatureData.Gender.Male)
					{
						this.audioSource.PlayOneShot(this.hurtMaleAudioContainer.PickAudioClip());
					}
					if (this.hurtFemaleAudioContainer && Player.local.creature.data.gender == CreatureData.Gender.Female)
					{
						this.audioSource.PlayOneShot(this.hurtFemaleAudioContainer.PickAudioClip());
					}
				}
			}
		}

		// Token: 0x060023D5 RID: 9173 RVA: 0x000F55B6 File Offset: 0x000F37B6
		private IEnumerator AttractionCoroutine()
		{
			while (this.playerDetected)
			{
				Player.local.locomotion.physicBody.AddForce(this.attractionForce * Time.deltaTime, ForceMode.VelocityChange);
				yield return null;
			}
			yield break;
		}

		// Token: 0x040022F1 RID: 8945
		public Collider triggerCollider;

		// Token: 0x040022F2 RID: 8946
		[Header("General")]
		public float damageStartupDelay = 2f;

		// Token: 0x040022F3 RID: 8947
		public float damageInterval = 2f;

		// Token: 0x040022F4 RID: 8948
		public DamageType damageType;

		// Token: 0x040022F5 RID: 8949
		public float damageValue = 5f;

		// Token: 0x040022F6 RID: 8950
		public Vector3 attractionForce;

		// Token: 0x040022F7 RID: 8951
		[Header("Audio")]
		public AudioContainer damageAudioContainer;

		// Token: 0x040022F8 RID: 8952
		public AudioContainer hurtMaleAudioContainer;

		// Token: 0x040022F9 RID: 8953
		public AudioContainer hurtFemaleAudioContainer;

		// Token: 0x040022FA RID: 8954
		protected AudioSource audioSource;

		// Token: 0x040022FB RID: 8955
		[Header("Debug")]
		[NonSerialized]
		public bool playerDetected;

		// Token: 0x040022FC RID: 8956
		protected PlayerHead playerHead;

		// Token: 0x040022FD RID: 8957
		protected Collider playerCollider;

		// Token: 0x040022FE RID: 8958
		protected int playerlayer;
	}
}
