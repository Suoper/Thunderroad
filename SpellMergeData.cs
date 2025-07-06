using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000229 RID: 553
	[Serializable]
	public class SpellMergeData : SpellData
	{
		// Token: 0x14000088 RID: 136
		// (add) Token: 0x06001759 RID: 5977 RVA: 0x0009C7D4 File Offset: 0x0009A9D4
		// (remove) Token: 0x0600175A RID: 5978 RVA: 0x0009C80C File Offset: 0x0009AA0C
		public event SpellMergeData.MergeEvent OnMergeUpdateEvent;

		// Token: 0x14000089 RID: 137
		// (add) Token: 0x0600175B RID: 5979 RVA: 0x0009C844 File Offset: 0x0009AA44
		// (remove) Token: 0x0600175C RID: 5980 RVA: 0x0009C87C File Offset: 0x0009AA7C
		public event SpellMergeData.MergeEvent OnMergeFixedUpdate;

		// Token: 0x1400008A RID: 138
		// (add) Token: 0x0600175D RID: 5981 RVA: 0x0009C8B4 File Offset: 0x0009AAB4
		// (remove) Token: 0x0600175E RID: 5982 RVA: 0x0009C8EC File Offset: 0x0009AAEC
		public event SpellMergeData.MergeEvent OnMergeStartEvent;

		// Token: 0x1400008B RID: 139
		// (add) Token: 0x0600175F RID: 5983 RVA: 0x0009C924 File Offset: 0x0009AB24
		// (remove) Token: 0x06001760 RID: 5984 RVA: 0x0009C95C File Offset: 0x0009AB5C
		public event SpellMergeData.MergeEvent OnMergeEndEvent;

		// Token: 0x06001761 RID: 5985 RVA: 0x0009C991 File Offset: 0x0009AB91
		public virtual void Load(Mana mana)
		{
			this.mana = mana;
			this.CountAndLoadSkillPassives(mana.creature);
			this.overrideChargeEffect = null;
			this.overrideFingerEffect = null;
		}

		// Token: 0x06001762 RID: 5986 RVA: 0x0009C9B4 File Offset: 0x0009ABB4
		public void CountAndLoadSkillPassives(Creature creature)
		{
			if (creature != null)
			{
				this.LoadSkillPassives(creature.CountSkillsOfTree(this.primarySkillTreeId, false, false));
			}
		}

		// Token: 0x06001763 RID: 5987 RVA: 0x0009C9D3 File Offset: 0x0009ABD3
		public virtual void LoadSkillPassives(int skillCount)
		{
			base.AddModifier(this, Modifier.ChargeSpeed, 1f + (float)skillCount * this.chargeSpeedPerSkill);
		}

		// Token: 0x06001764 RID: 5988 RVA: 0x0009C9EC File Offset: 0x0009ABEC
		public virtual bool CanMerge()
		{
			return true;
		}

		// Token: 0x06001765 RID: 5989 RVA: 0x0009C9F0 File Offset: 0x0009ABF0
		public virtual void FixedUpdate()
		{
			if (this.mana.mergeCompleted)
			{
				float handLeftForceMultiplier = this.handForceMultiplierDistanceCurve.Evaluate(Vector3.Distance(this.mana.mergePoint.position, this.mana.casterLeft.magicSource.position));
				this.mana.casterLeft.ragdollHand.physicBody.AddForce((this.mana.casterLeft.magicSource.position - this.mana.mergePoint.position).normalized * (this.handForceCurve.Evaluate(Time.time) * handLeftForceMultiplier * this.handForceChargeCurve.Evaluate(this.currentCharge)), ForceMode.Force);
				float handRightForceMultiplier = this.handForceMultiplierDistanceCurve.Evaluate(Vector3.Distance(this.mana.mergePoint.position, this.mana.casterRight.magicSource.position));
				this.mana.casterRight.ragdollHand.physicBody.AddForce((this.mana.casterRight.magicSource.position - this.mana.mergePoint.position).normalized * (this.handForceCurve.Evaluate(Time.time) * handRightForceMultiplier * this.handForceChargeCurve.Evaluate(this.currentCharge)), ForceMode.Force);
				SpellMergeData.MergeEvent onMergeFixedUpdate = this.OnMergeFixedUpdate;
				if (onMergeFixedUpdate == null)
				{
					return;
				}
				onMergeFixedUpdate(this);
			}
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x0009CB78 File Offset: 0x0009AD78
		public virtual void Update()
		{
			if (this.mana.mergeCompleted)
			{
				float chargeIncrease = Mathf.Clamp(this.chargeSpeed * this.mana.GetHandsIntensity() * base.GetModifier(Modifier.ChargeSpeed) * Time.deltaTime, 0f, 1f - this.currentCharge);
				this.currentCharge = Mathf.Clamp01(this.currentCharge + chargeIncrease);
				if (this.currentCharge > this.minCharge)
				{
					if (!this.reachedMinCharge)
					{
						this.mana.creature.ragdoll.ForBothHands(delegate(RagdollHand hand)
						{
							hand.HapticTick(1f, true);
						});
						this.reachedMinCharge = true;
						this.mana.InvokeMergeStep(this, Mana.CastStep.MergeCharged);
					}
				}
				else
				{
					this.reachedMinCharge = false;
				}
				EffectInstance effectInstance = this.chargeEffect;
				if (effectInstance != null)
				{
					effectInstance.SetIntensity(this.currentCharge);
				}
				this.mana.casterRight.ragdollHand.playerHand.controlHand.HapticLoop(this, this.hapticIntensityCurve.Evaluate(this.currentCharge) * this.hapticCurveModifier, 0.01f);
				this.mana.casterLeft.ragdollHand.playerHand.controlHand.HapticLoop(this, this.hapticIntensityCurve.Evaluate(this.currentCharge) * this.hapticCurveModifier, 0.01f);
				this.mana.casterRight.SetFingersEffect(this.currentCharge);
				this.mana.casterRight.ragdollHand.poser.SetTargetWeight(1f - this.currentCharge, false);
				this.mana.casterLeft.SetFingersEffect(this.currentCharge);
				this.mana.casterLeft.ragdollHand.poser.SetTargetWeight(1f - this.currentCharge, false);
				SpellMergeData.MergeEvent onMergeUpdateEvent = this.OnMergeUpdateEvent;
				if (onMergeUpdateEvent == null)
				{
					return;
				}
				onMergeUpdateEvent(this);
			}
		}

		// Token: 0x06001767 RID: 5991 RVA: 0x0009CD64 File Offset: 0x0009AF64
		public virtual void Merge(bool active)
		{
			this.mana.InvokeMergeStep(this, active ? Mana.CastStep.MergeStart : Mana.CastStep.MergeStop);
			if (active)
			{
				this.mana.mergePoint.position = Vector3.Lerp(this.mana.casterLeft.magicSource.position, this.mana.casterRight.magicSource.position, 0.5f);
				this.currentCharge = (((SpellCastCharge)this.mana.casterLeft.spellInstance).currentCharge + ((SpellCastCharge)this.mana.casterRight.spellInstance).currentCharge) / 2f * this.chargeStartHandsRatio;
				this.mana.casterLeft.ragdollHand.poser.SetDefaultPose(this.openHandPoseData);
				this.mana.casterLeft.ragdollHand.poser.SetTargetPose(this.closeHandPoseData, false, false, false, false, false);
				this.mana.casterRight.ragdollHand.poser.SetDefaultPose(this.openHandPoseData);
				this.mana.casterRight.ragdollHand.poser.SetTargetPose(this.closeHandPoseData, false, false, false, false, false);
				this.mana.casterLeft.ragdollHand.playerHand.controlHand.HapticLoop(this, this.hapticIntensityCurve.Evaluate(this.currentCharge) * this.hapticCurveModifier, 0.01f);
				this.mana.casterRight.ragdollHand.playerHand.controlHand.HapticLoop(this, this.hapticIntensityCurve.Evaluate(this.currentCharge) * this.hapticCurveModifier, 0.01f);
				this.mana.casterLeft.SpawnFingersEffect(this.overrideFingerEffect ?? this.fingerEffectData, true, 0f, this.mana.casterLeft.Orb);
				this.mana.casterRight.SpawnFingersEffect(this.overrideFingerEffect ?? this.fingerEffectData, true, 0f, this.mana.casterRight.Orb);
				EffectData effectData = this.overrideChargeEffect ?? this.chargeEffectData;
				this.chargeEffect = ((effectData != null) ? effectData.Spawn(this.mana.mergePoint, true, null, false) : null);
				EffectInstance effectInstance = this.chargeEffect;
				if (effectInstance != null)
				{
					effectInstance.Play(0, false, false);
				}
				EffectInstance effectInstance2 = this.chargeEffect;
				if (effectInstance2 != null)
				{
					effectInstance2.SetIntensity(0f);
				}
				this.mana.casterLeft.ragdollHand.playerHand.link.SetJointModifier(this, this.handPositionSpringMultiplier, this.handPositionDamperMultiplier, 1f, 1f, this.handLocomotionVelocityCorrectionMultiplier);
				this.mana.casterRight.ragdollHand.playerHand.link.SetJointModifier(this, this.handPositionSpringMultiplier, this.handPositionDamperMultiplier, 1f, 1f, this.handLocomotionVelocityCorrectionMultiplier);
				SpellMergeData.MergeEvent onMergeStartEvent = this.OnMergeStartEvent;
				if (onMergeStartEvent == null)
				{
					return;
				}
				onMergeStartEvent(this);
				return;
			}
			else
			{
				this.mana.casterLeft.StopFingersEffect();
				this.mana.casterRight.StopFingersEffect();
				if (!this.mana.casterLeft.ragdollHand.grabbedHandle)
				{
					this.mana.casterLeft.ragdollHand.poser.ResetDefaultPose();
					this.mana.casterLeft.ragdollHand.poser.ResetTargetPose();
				}
				if (!this.mana.casterRight.ragdollHand.grabbedHandle)
				{
					this.mana.casterRight.ragdollHand.poser.ResetDefaultPose();
					this.mana.casterRight.ragdollHand.poser.ResetTargetPose();
				}
				this.mana.casterLeft.ragdollHand.playerHand.controlHand.StopHapticLoop(this);
				this.mana.casterRight.ragdollHand.playerHand.controlHand.StopHapticLoop(this);
				this.mana.casterLeft.ragdollHand.playerHand.link.RemoveJointModifier(this);
				this.mana.casterRight.ragdollHand.playerHand.link.RemoveJointModifier(this);
				EffectInstance effectInstance3 = this.chargeEffect;
				if (effectInstance3 != null)
				{
					effectInstance3.SetParent(null, false);
				}
				EffectInstance effectInstance4 = this.chargeEffect;
				if (effectInstance4 != null)
				{
					effectInstance4.End(false, -1f);
				}
				if (this.allowThrow && this.mana.creature.player)
				{
					Vector3 leftVel = Player.currentCreature.handLeft.Velocity();
					Vector3 rightVel = Player.currentCreature.handRight.Velocity();
					Vector3 velocity = Vector3.Slerp(leftVel.normalized, rightVel.normalized, 0.5f) * Mathf.Lerp(leftVel.magnitude, rightVel.magnitude, 0.5f);
					if (velocity.magnitude > this.throwMinHandVelocity && this.currentCharge > this.minCharge)
					{
						this.Throw(velocity);
					}
				}
				SpellMergeData.MergeEvent onMergeEndEvent = this.OnMergeEndEvent;
				if (onMergeEndEvent == null)
				{
					return;
				}
				onMergeEndEvent(this);
				return;
			}
		}

		// Token: 0x06001768 RID: 5992 RVA: 0x0009D281 File Offset: 0x0009B481
		public virtual void FireAxis(float value, Side side)
		{
			if (this.mana.mergeCompleted)
			{
				if (side == Side.Right)
				{
					this.mana.casterRight.intensity = value;
					return;
				}
				if (side == Side.Left)
				{
					this.mana.casterLeft.intensity = value;
				}
			}
		}

		// Token: 0x06001769 RID: 5993 RVA: 0x0009D2BC File Offset: 0x0009B4BC
		public virtual void Unload()
		{
			this.mana.casterLeft.StopFingersEffect();
			this.mana.casterRight.StopFingersEffect();
			if (!this.mana.casterLeft.ragdollHand.grabbedHandle)
			{
				this.mana.casterLeft.ragdollHand.poser.ResetDefaultPose();
				this.mana.casterLeft.ragdollHand.poser.ResetTargetPose();
			}
			if (!this.mana.casterRight.ragdollHand.grabbedHandle)
			{
				this.mana.casterRight.ragdollHand.poser.ResetDefaultPose();
				this.mana.casterRight.ragdollHand.poser.ResetTargetPose();
			}
			this.mana.casterLeft.ragdollHand.playerHand.controlHand.StopHapticLoop(this);
			this.mana.casterRight.ragdollHand.playerHand.controlHand.StopHapticLoop(this);
			this.mana.casterLeft.ragdollHand.playerHand.link.RemoveJointModifier(this);
			this.mana.casterRight.ragdollHand.playerHand.link.RemoveJointModifier(this);
			EffectInstance effectInstance = this.chargeEffect;
			if (effectInstance != null)
			{
				effectInstance.SetParent(null, false);
			}
			EffectInstance effectInstance2 = this.chargeEffect;
			if (effectInstance2 == null)
			{
				return;
			}
			effectInstance2.End(false, -1f);
		}

		// Token: 0x0600176A RID: 5994 RVA: 0x0009D432 File Offset: 0x0009B632
		public virtual void Throw(Vector3 velocity)
		{
			this.mana.InvokeMergeStep(this, Mana.CastStep.MergeFireStart);
		}

		// Token: 0x0600176B RID: 5995 RVA: 0x0009D444 File Offset: 0x0009B644
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			if (this.chargeEffectId != null && this.chargeEffectId != "")
			{
				this.chargeEffectData = Catalog.GetData<EffectData>(this.chargeEffectId, true);
			}
			if (this.closeHandPoseId != null && this.closeHandPoseId != "")
			{
				this.closeHandPoseData = Catalog.GetData<HandPoseData>(this.closeHandPoseId, true);
			}
			if (this.openHandPoseId != null && this.openHandPoseId != "")
			{
				this.openHandPoseData = Catalog.GetData<HandPoseData>(this.openHandPoseId, true);
			}
			if (this.fingerEffectId != null && this.fingerEffectId != "")
			{
				this.fingerEffectData = Catalog.GetData<EffectData>(this.fingerEffectId, true);
			}
			this.handForceCurve.postWrapMode = WrapMode.Loop;
		}

		// Token: 0x0600176C RID: 5996 RVA: 0x0009D513 File Offset: 0x0009B713
		public override int GetCurrentVersion()
		{
			return 0;
		}

		// Token: 0x040016B7 RID: 5815
		[JsonMergeKey]
		public string leftSpellId;

		// Token: 0x040016B8 RID: 5816
		[JsonMergeKey]
		public string rightSpellId;

		// Token: 0x040016B9 RID: 5817
		public float chargeSpeed = 0.2f;

		// Token: 0x040016BA RID: 5818
		public float chargeSpeedPerSkill = 0.05f;

		// Token: 0x040016BB RID: 5819
		public float chargeStartHandsRatio = 0.5f;

		// Token: 0x040016BC RID: 5820
		public float stopSpeed = 0.6f;

		// Token: 0x040016BD RID: 5821
		public bool stopIfManaDepleted = true;

		// Token: 0x040016BE RID: 5822
		[NonSerialized]
		public float currentCharge;

		// Token: 0x040016BF RID: 5823
		public float handEnterAngle = 30f;

		// Token: 0x040016C0 RID: 5824
		public float handEnterDistance = 0.2f;

		// Token: 0x040016C1 RID: 5825
		public float handExitAngle = 90f;

		// Token: 0x040016C2 RID: 5826
		public float handExitDistance = 0.6f;

		// Token: 0x040016C3 RID: 5827
		public float handCompletedDistance = 0.001f;

		// Token: 0x040016C4 RID: 5828
		public float minCharge = 0.9f;

		// Token: 0x040016C5 RID: 5829
		public AnimationCurve hapticIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x040016C6 RID: 5830
		public float hapticCurveModifier = 0.5f;

		// Token: 0x040016C7 RID: 5831
		public AnimationCurve handForceCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 120f),
			new Keyframe(0.05f, 180f),
			new Keyframe(0.1f, 120f)
		});

		// Token: 0x040016C8 RID: 5832
		public AnimationCurve handForceMultiplierDistanceCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 1f),
			new Keyframe(0.2f, 0f)
		});

		// Token: 0x040016C9 RID: 5833
		public AnimationCurve handForceChargeCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x040016CA RID: 5834
		public float handPositionSpringMultiplier = 0.4f;

		// Token: 0x040016CB RID: 5835
		public float handPositionDamperMultiplier = 1f;

		// Token: 0x040016CC RID: 5836
		public float handLocomotionVelocityCorrectionMultiplier = 0.3f;

		// Token: 0x040016CD RID: 5837
		private bool reachedMinCharge;

		// Token: 0x040016CE RID: 5838
		public string chargeEffectId;

		// Token: 0x040016CF RID: 5839
		protected EffectData chargeEffectData;

		// Token: 0x040016D0 RID: 5840
		protected EffectInstance chargeEffect;

		// Token: 0x040016D1 RID: 5841
		[Tooltip("Set to zero for no lerping")]
		public float effectLerpFactor;

		// Token: 0x040016D2 RID: 5842
		public string fingerEffectId;

		// Token: 0x040016D3 RID: 5843
		protected EffectData fingerEffectData;

		// Token: 0x040016D4 RID: 5844
		[NonSerialized]
		public EffectData overrideFingerEffect;

		// Token: 0x040016D5 RID: 5845
		[NonSerialized]
		public EffectData overrideChargeEffect;

		// Token: 0x040016D6 RID: 5846
		public bool allowThrow;

		// Token: 0x040016D7 RID: 5847
		public string throwEffectId;

		// Token: 0x040016D8 RID: 5848
		protected EffectData throwEffectData;

		// Token: 0x040016D9 RID: 5849
		public float throwMinHandVelocity = 2f;

		// Token: 0x040016DA RID: 5850
		public string closeHandPoseId;

		// Token: 0x040016DB RID: 5851
		[NonSerialized]
		public HandPoseData closeHandPoseData;

		// Token: 0x040016DC RID: 5852
		public string openHandPoseId;

		// Token: 0x040016DD RID: 5853
		[NonSerialized]
		public HandPoseData openHandPoseData;

		// Token: 0x040016DE RID: 5854
		[NonSerialized]
		public Mana mana;

		// Token: 0x0200083F RID: 2111
		// (Invoke) Token: 0x06003F70 RID: 16240
		public delegate void MergeEvent(SpellMergeData spell);
	}
}
