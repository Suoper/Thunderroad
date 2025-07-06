using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200025C RID: 604
	public class GolemAnimatorEvent : MonoBehaviour
	{
		// Token: 0x06001AB7 RID: 6839 RVA: 0x000B2628 File Offset: 0x000B0828
		private void OnValidate()
		{
			if (!this.animator)
			{
				this.animator = base.GetComponent<Animator>();
			}
			if (!this.golem)
			{
				this.golem = base.GetComponentInParent<GolemController>();
			}
			if (this.audioSources.IsNullOrEmpty())
			{
				this.audioSources = new List<AudioSource>();
				this.audioSources.AddRange(this.golem.GetComponentsInChildren<AudioSource>());
			}
			if (this.particleSystems.IsNullOrEmpty())
			{
				this.particleSystems = new List<ParticleSystem>();
				this.particleSystems.AddRange(this.golem.GetComponentsInChildren<ParticleSystem>());
			}
		}

		// Token: 0x06001AB8 RID: 6840 RVA: 0x000B26C4 File Offset: 0x000B08C4
		private void Awake()
		{
			this.InitAnimationParametersHashes();
			for (int i = 0; i < this.audioSources.Count; i++)
			{
				if (!this.keyedAudios.ContainsKey(this.audioSources[i].name) && !(this.audioSources[i].GetComponentInParent<GolemCrystal>() != null))
				{
					this.keyedAudios.Add(this.audioSources[i].name, this.audioSources[i]);
				}
			}
			for (int j = 0; j < this.particleSystems.Count; j++)
			{
				if (!this.keyedAudios.ContainsKey(this.particleSystems[j].name) && !(this.particleSystems[j].GetComponentInParent<GolemCrystal>() != null))
				{
					this.keyedParticles[this.particleSystems[j].name] = this.particleSystems[j];
				}
			}
		}

		// Token: 0x06001AB9 RID: 6841 RVA: 0x000B27C1 File Offset: 0x000B09C1
		private void InitAnimationParametersHashes()
		{
			GolemAnimatorEvent.blendIdHash = Animator.StringToHash("BlendID");
			GolemAnimatorEvent.rightFootHash = Animator.StringToHash("RightFoot");
		}

		// Token: 0x06001ABA RID: 6842 RVA: 0x000B27E1 File Offset: 0x000B09E1
		private void OnAnimatorMove()
		{
			if (!this.golem.animatorIsRoot)
			{
				this.golem.OnAnimatorMove();
			}
		}

		// Token: 0x06001ABB RID: 6843 RVA: 0x000B27FC File Offset: 0x000B09FC
		public void RightPlant(AnimationEvent e)
		{
			float @float = this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
			float thisParameter = (e != null) ? e.floatParameter : 0f;
			float forwardRound = Mathf.Round(@float);
			if ((e == null || e.animatorStateInfo.fullPathHash == this.animator.GetCurrentAnimatorStateInfo(0).fullPathHash) && Mathf.Abs(thisParameter - forwardRound) <= Mathf.Epsilon)
			{
				this.animator.SetBool(GolemAnimatorEvent.rightFootHash, false);
				this.rightFootPlanted = true;
				UnityEvent unityEvent = this.onRightFootPlant;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x06001ABC RID: 6844 RVA: 0x000B2894 File Offset: 0x000B0A94
		public void RightUnplant(AnimationEvent e)
		{
			float blendAnimId = this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
			float num = (e != null) ? e.floatParameter : 0f;
			float forwardRound = Mathf.Round(blendAnimId);
			if (Mathf.Abs(num - forwardRound) <= Mathf.Epsilon)
			{
				this.rightFootPlanted = false;
			}
		}

		// Token: 0x06001ABD RID: 6845 RVA: 0x000B28E0 File Offset: 0x000B0AE0
		public void LeftPlant(AnimationEvent e)
		{
			float @float = this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
			float thisParameter = (e != null) ? e.floatParameter : 0f;
			float forwardRound = Mathf.Round(@float);
			if ((e == null || e.animatorStateInfo.fullPathHash == this.animator.GetCurrentAnimatorStateInfo(0).fullPathHash) && Mathf.Abs(thisParameter - forwardRound) <= Mathf.Epsilon)
			{
				this.animator.SetBool(GolemAnimatorEvent.rightFootHash, true);
				this.leftFootPlanted = true;
				UnityEvent unityEvent = this.onLeftFootPlant;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			}
		}

		// Token: 0x06001ABE RID: 6846 RVA: 0x000B2978 File Offset: 0x000B0B78
		public void LeftUnplant(AnimationEvent e)
		{
			float blendAnimId = this.animator.GetFloat(GolemAnimatorEvent.blendIdHash);
			float num = (e != null) ? e.floatParameter : 0f;
			float forwardRound = Mathf.Round(blendAnimId);
			if (Mathf.Abs(num - forwardRound) <= Mathf.Epsilon)
			{
				this.leftFootPlanted = false;
			}
		}

		// Token: 0x06001ABF RID: 6847 RVA: 0x000B29C2 File Offset: 0x000B0BC2
		private void ActivateAbilityStep(AnimationEvent e)
		{
			GolemAbility currentAbility = this.golem.currentAbility;
			if (currentAbility == null)
			{
				return;
			}
			currentAbility.TryAbilityStep(e);
		}

		// Token: 0x06001AC0 RID: 6848 RVA: 0x000B29DA File Offset: 0x000B0BDA
		private void StartTurnTo(AnimationEvent e)
		{
		}

		// Token: 0x06001AC1 RID: 6849 RVA: 0x000B29DC File Offset: 0x000B0BDC
		private void StopTurnTo(AnimationEvent e)
		{
		}

		// Token: 0x06001AC2 RID: 6850 RVA: 0x000B29DE File Offset: 0x000B0BDE
		private void EnableHitbox(AnimationEvent e)
		{
			Action<bool> action = this.onEnableHitbox;
			if (action == null)
			{
				return;
			}
			action(true);
		}

		// Token: 0x06001AC3 RID: 6851 RVA: 0x000B29F1 File Offset: 0x000B0BF1
		private void DisableHitbox(AnimationEvent e)
		{
			Action<bool> action = this.onEnableHitbox;
			if (action == null)
			{
				return;
			}
			action(false);
		}

		// Token: 0x06001AC4 RID: 6852 RVA: 0x000B2A04 File Offset: 0x000B0C04
		private void EffectOn(AnimationEvent e)
		{
		}

		// Token: 0x06001AC5 RID: 6853 RVA: 0x000B2A06 File Offset: 0x000B0C06
		private void EffectOff(AnimationEvent e)
		{
			Debug.Log("EffectOff");
		}

		// Token: 0x06001AC6 RID: 6854 RVA: 0x000B2A14 File Offset: 0x000B0C14
		private void PlayAudioSource(AnimationEvent e)
		{
			AudioSource audioSource;
			if (this.keyedAudios.TryGetValue(e.stringParameter, out audioSource))
			{
				audioSource.Play();
			}
		}

		// Token: 0x06001AC7 RID: 6855 RVA: 0x000B2A3C File Offset: 0x000B0C3C
		private void PlayParticleEffect(AnimationEvent e)
		{
			ParticleSystem system;
			if (!this.keyedParticles.TryGetValue(e.stringParameter, out system))
			{
				Debug.LogWarning(string.Concat(new string[]
				{
					"Golem attempted to ",
					(e.intParameter == 0) ? "stop" : "play",
					" particle effect ",
					e.stringParameter,
					", but no particle system of that name could be found."
				}));
				return;
			}
			int intParameter = e.intParameter;
			if (intParameter == 0)
			{
				system.Stop();
				return;
			}
			if (intParameter != 1)
			{
				return;
			}
			system.Play();
		}

		// Token: 0x0400196C RID: 6508
		[Header("References")]
		public GolemController golem;

		// Token: 0x0400196D RID: 6509
		public Animator animator;

		// Token: 0x0400196E RID: 6510
		public List<AudioSource> audioSources;

		// Token: 0x0400196F RID: 6511
		public List<ParticleSystem> particleSystems;

		// Token: 0x04001970 RID: 6512
		[Header("Feet")]
		public bool rightFootPlanted;

		// Token: 0x04001971 RID: 6513
		public bool leftFootPlanted;

		// Token: 0x04001972 RID: 6514
		[Header("Events")]
		public UnityEvent onLeftFootPlant;

		// Token: 0x04001973 RID: 6515
		public UnityEvent onRightFootPlant;

		// Token: 0x04001974 RID: 6516
		public Action<bool> onEnableHitbox;

		// Token: 0x04001975 RID: 6517
		public static int blendIdHash;

		// Token: 0x04001976 RID: 6518
		public static int rightFootHash;

		// Token: 0x04001977 RID: 6519
		protected Dictionary<string, AudioSource> keyedAudios = new Dictionary<string, AudioSource>();

		// Token: 0x04001978 RID: 6520
		protected Dictionary<string, ParticleSystem> keyedParticles = new Dictionary<string, ParticleSystem>();
	}
}
