using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000260 RID: 608
	public abstract class GolemAbility : ScriptableObject
	{
		/// <summary>
		/// Whether a headshot should interrupt and <c>End()</c> this ability.
		/// </summary>
		// Token: 0x170001B8 RID: 440
		// (get) Token: 0x06001B66 RID: 7014 RVA: 0x000B5710 File Offset: 0x000B3910
		public virtual bool HeadshotInterruptable
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Whether the golem should ask this ability where to look.
		/// Runs <c>GolemAbility.LookAt()</c> during the golem's <c>LookAtCoroutine</c> if true.
		/// </summary>
		// Token: 0x170001B9 RID: 441
		// (get) Token: 0x06001B67 RID: 7015 RVA: 0x000B5713 File Offset: 0x000B3913
		public virtual bool OverrideLook
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		/// Force-run this ability, cancelling whatever the golem was doing before. Not a particularly safe method.
		/// </summary>
		// Token: 0x06001B68 RID: 7016 RVA: 0x000B5716 File Offset: 0x000B3916
		public virtual void ForceRun()
		{
			this.golem.StartAbility(this, null);
		}

		/// <summary>
		/// If <c>OverrideLook</c> is true, this is called during the golem's <c>LookAtCoroutine</c> instead of the default behaviour.
		/// </summary>
		// Token: 0x06001B69 RID: 7017 RVA: 0x000B5725 File Offset: 0x000B3925
		public virtual void LookAt()
		{
		}

		/// <summary>
		/// Whether this ability should be allowed to run.
		/// </summary>
		/// <param name="golem"></param>
		/// <returns></returns>
		// Token: 0x06001B6A RID: 7018 RVA: 0x000B5727 File Offset: 0x000B3927
		public virtual bool Allow(GolemController golem)
		{
			return (this.type == GolemAbilityType.Climb || golem.lastAbility != this) && (this.abilityTier == Golem.Tier.Any || golem.tier == Golem.Tier.Any || this.abilityTier.HasFlagNoGC(golem.tier));
		}

		// Token: 0x06001B6B RID: 7019 RVA: 0x000B5764 File Offset: 0x000B3964
		public void Begin(GolemController golem, Action endCallback = null)
		{
			this.golem = golem;
			this.active = true;
			this.endCallback = endCallback;
			this.Begin(golem);
		}

		/// <summary>
		/// Called when the ability first starts. Load data and tell the golem to play animations here.
		/// </summary>
		/// <param name="golem"></param>
		// Token: 0x06001B6C RID: 7020 RVA: 0x000B5782 File Offset: 0x000B3982
		public virtual void Begin(GolemController golem)
		{
		}

		/// <summary>
		/// Called when the golem's current animation triggers an AbilityStep event.
		/// </summary>
		/// <param name="step"></param>
		// Token: 0x06001B6D RID: 7021 RVA: 0x000B5784 File Offset: 0x000B3984
		public void TryAbilityStep(AnimationEvent e)
		{
			if (e.stringParameter != this.stepMotion.ToString())
			{
				return;
			}
			this.AbilityStep(e.intParameter);
		}

		/// <summary>
		/// Called when the golem's current animation triggers an AbilityStep event.
		/// </summary>
		/// <param name="step"></param>
		// Token: 0x06001B6E RID: 7022 RVA: 0x000B57B1 File Offset: 0x000B39B1
		public virtual void AbilityStep(int step)
		{
		}

		/// <summary>
		/// This is called once every Golem update cycle (every 0.5 seconds by default).
		/// </summary>
		/// <param name="delta"></param>
		// Token: 0x06001B6F RID: 7023 RVA: 0x000B57B3 File Offset: 0x000B39B3
		public virtual void OnCycle(float delta)
		{
		}

		/// <summary>
		/// This is called once every ManagedUpdate.
		/// </summary>
		// Token: 0x06001B70 RID: 7024 RVA: 0x000B57B5 File Offset: 0x000B39B5
		public virtual void OnUpdate()
		{
		}

		/// <summary>
		/// This is called when the ability is interrupted by a crystal being broken.
		/// </summary>
		// Token: 0x06001B71 RID: 7025 RVA: 0x000B57B7 File Offset: 0x000B39B7
		public virtual void Interrupt()
		{
			this.End(true);
		}

		/// <summary>
		/// Call this to kindly ask the GolemController to safely end this ability.
		/// </summary>
		// Token: 0x06001B72 RID: 7026 RVA: 0x000B57C0 File Offset: 0x000B39C0
		public void End(bool early)
		{
			if (this.golem.currentAbility == this)
			{
				this.golem.EndAbility();
			}
			else
			{
				this.OnEnd();
			}
			if (this.stunOnExit && !early)
			{
				this.golem.Stun(this.stunDuration);
			}
		}

		// Token: 0x06001B73 RID: 7027 RVA: 0x000B580F File Offset: 0x000B3A0F
		public void End()
		{
			this.End(false);
		}

		/// <summary>
		/// Called when the ability should end. Put code to end and clean up your ability here,
		/// but do not call this directly - use <c>GolemAbility.End()</c> instead.
		/// </summary>
		// Token: 0x06001B74 RID: 7028 RVA: 0x000B5818 File Offset: 0x000B3A18
		public virtual void OnEnd()
		{
			this.active = false;
			Action action = this.endCallback;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x04001A0C RID: 6668
		public GolemAbilityType type = GolemAbilityType.Ranged;

		// Token: 0x04001A0D RID: 6669
		public float weight = 1f;

		// Token: 0x04001A0E RID: 6670
		public RampageType rampageType;

		// Token: 0x04001A0F RID: 6671
		public Golem.Tier abilityTier;

		// Token: 0x04001A10 RID: 6672
		public bool stunOnExit;

		// Token: 0x04001A11 RID: 6673
		public float stunDuration;

		// Token: 0x04001A12 RID: 6674
		[NonSerialized]
		public bool active;

		// Token: 0x04001A13 RID: 6675
		[NonSerialized]
		public GolemController golem;

		// Token: 0x04001A14 RID: 6676
		protected Action endCallback;

		// Token: 0x04001A15 RID: 6677
		[NonSerialized]
		public GolemController.AttackMotion stepMotion;
	}
}
