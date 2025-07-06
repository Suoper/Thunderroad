using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000224 RID: 548
	[Serializable]
	public class SpellCastCharge : SpellCastData
	{
		// Token: 0x06001705 RID: 5893 RVA: 0x0009A6E9 File Offset: 0x000988E9
		public bool ShouldSerializeallowUnderwater()
		{
			return false;
		}

		// Token: 0x14000080 RID: 128
		// (add) Token: 0x06001706 RID: 5894 RVA: 0x0009A6EC File Offset: 0x000988EC
		// (remove) Token: 0x06001707 RID: 5895 RVA: 0x0009A724 File Offset: 0x00098924
		public event SpellCastCharge.SpellEvent OnSpellUpdateEvent;

		// Token: 0x14000081 RID: 129
		// (add) Token: 0x06001708 RID: 5896 RVA: 0x0009A75C File Offset: 0x0009895C
		// (remove) Token: 0x06001709 RID: 5897 RVA: 0x0009A794 File Offset: 0x00098994
		public event SpellCastCharge.SpellEvent OnSpellFixedUpdateEvent;

		// Token: 0x14000082 RID: 130
		// (add) Token: 0x0600170A RID: 5898 RVA: 0x0009A7CC File Offset: 0x000989CC
		// (remove) Token: 0x0600170B RID: 5899 RVA: 0x0009A804 File Offset: 0x00098A04
		public event SpellCastCharge.SpellEvent OnSpellCastEvent;

		// Token: 0x14000083 RID: 131
		// (add) Token: 0x0600170C RID: 5900 RVA: 0x0009A83C File Offset: 0x00098A3C
		// (remove) Token: 0x0600170D RID: 5901 RVA: 0x0009A874 File Offset: 0x00098A74
		public event SpellCastCharge.SpellEvent OnSpellStopEvent;

		// Token: 0x14000084 RID: 132
		// (add) Token: 0x0600170E RID: 5902 RVA: 0x0009A8AC File Offset: 0x00098AAC
		// (remove) Token: 0x0600170F RID: 5903 RVA: 0x0009A8E4 File Offset: 0x00098AE4
		public event SpellCastCharge.SpellEvent OnImbueUnloadEvent;

		// Token: 0x14000085 RID: 133
		// (add) Token: 0x06001710 RID: 5904 RVA: 0x0009A91C File Offset: 0x00098B1C
		// (remove) Token: 0x06001711 RID: 5905 RVA: 0x0009A954 File Offset: 0x00098B54
		public event SpellCastCharge.SpellEvent OnGripEndEvent;

		// Token: 0x14000086 RID: 134
		// (add) Token: 0x06001712 RID: 5906 RVA: 0x0009A98C File Offset: 0x00098B8C
		// (remove) Token: 0x06001713 RID: 5907 RVA: 0x0009A9C4 File Offset: 0x00098BC4
		public event SpellCastCharge.SpellThrowEvent OnSpellThrowEvent;

		// Token: 0x14000087 RID: 135
		// (add) Token: 0x06001714 RID: 5908 RVA: 0x0009A9FC File Offset: 0x00098BFC
		// (remove) Token: 0x06001715 RID: 5909 RVA: 0x0009AA34 File Offset: 0x00098C34
		public event SpellCastCharge.CrystalUseEvent OnCrystalUseEvent;

		// Token: 0x06001716 RID: 5910 RVA: 0x0009AA69 File Offset: 0x00098C69
		public string GetCastDescription()
		{
			return LocalizationManager.Instance.TryGetLocalization("Skills", this.castDescription, null, false);
		}

		// Token: 0x06001717 RID: 5911 RVA: 0x0009AA82 File Offset: 0x00098C82
		public string GetImbueDescription()
		{
			return LocalizationManager.Instance.TryGetLocalization("Skills", this.imbueDescription, null, false);
		}

		// Token: 0x06001718 RID: 5912 RVA: 0x0009AA9B File Offset: 0x00098C9B
		public string GetSlamDescription()
		{
			return LocalizationManager.Instance.TryGetLocalization("Skills", this.slamDescription, null, false);
		}

		// Token: 0x06001719 RID: 5913 RVA: 0x0009AAB4 File Offset: 0x00098CB4
		public new SpellCastCharge Clone()
		{
			return base.MemberwiseClone() as SpellCastCharge;
		}

		// Token: 0x0600171A RID: 5914 RVA: 0x0009AAC1 File Offset: 0x00098CC1
		public override int GetCurrentVersion()
		{
			return 0;
		}

		// Token: 0x0600171B RID: 5915 RVA: 0x0009AAC4 File Offset: 0x00098CC4
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			if (this.heldStaffModifiers == null)
			{
				this.heldStaffModifiers = new Dictionary<Modifier, float>
				{
					{
						Modifier.ChargeSpeed,
						1.5f
					}
				};
			}
			if (SpellCastCharge.forceOverrideToLocalizedText)
			{
				this.castDescription = "{" + this.id + "DescriptionCast}";
				this.imbueDescription = "{" + this.id + "DescriptionImbue}";
				this.slamDescription = "{" + this.id + "DescriptionSlam}";
			}
			this.chargeEffectData = Catalog.GetData<EffectData>(this.chargeEffectId, true);
			this.readyEffectData = Catalog.GetData<EffectData>(this.readyEffectId, true);
			this.readyMinorEffectData = Catalog.GetData<EffectData>(this.readyMinorEffectId, true);
			this.fingerEffectData = Catalog.GetData<EffectData>(this.fingerEffectId, true);
			this.gripCastEffectData = Catalog.GetData<EffectData>(this.gripCastEffectId, true);
			this.closeHandPoseData = Catalog.GetData<HandPoseData>(this.closeHandPoseId, true);
			this.openHandPoseData = Catalog.GetData<HandPoseData>(this.openHandPoseId, true);
			this.throwEffectData = Catalog.GetData<EffectData>(this.throwEffectId, true);
			this.sprayHandPoseData = Catalog.GetData<HandPoseData>(this.sprayHandPoseId, true);
			this.imbueMetalEffectData = Catalog.GetData<EffectData>(this.imbueMetalEffectId, true);
			this.imbueBladeEffectData = Catalog.GetData<EffectData>(this.imbueBladeEffectId, true);
			this.imbueTransferEffectData = Catalog.GetData<EffectData>(this.imbueTransferEffectId, true);
			this.imbueCrystalEffectData = Catalog.GetData<EffectData>(this.imbueCrystalEffectId, true);
			this.imbueNaaEffectData = Catalog.GetData<EffectData>(this.imbueNaaEffectId, true);
			this.staffSlamCollisionEffectData = Catalog.GetData<EffectData>(this.staffSlamCollisionEffectId, true);
			this.staffSlamTipEffectData = Catalog.GetData<EffectData>(this.staffSlamTipEffectId, true);
			this.gripCastStatusEffect = Catalog.GetData<StatusData>(this.gripCastStatusEffectId, true);
		}

		// Token: 0x0600171C RID: 5916 RVA: 0x0009AC80 File Offset: 0x00098E80
		public override void LoadSkillPassives(int skillCount)
		{
			base.LoadSkillPassives(skillCount);
			base.AddModifier(this, Modifier.ChargeSpeed, 1f + (float)skillCount * this.chargeSpeedPerSkill);
		}

		// Token: 0x0600171D RID: 5917 RVA: 0x0009ACA0 File Offset: 0x00098EA0
		public override void Load(SpellCaster spellCaster)
		{
			base.Load(spellCaster);
			this.spellCaster = spellCaster;
			base.SetupModifiers();
			base.CountAndLoadSkillPassives(spellCaster.ragdollHand.creature);
			if (this.allowStaffBuff)
			{
				spellCaster.ragdollHand.creature.OnHeldImbueChange -= this.OnHeldImbueChange;
				spellCaster.ragdollHand.creature.OnHeldImbueChange += this.OnHeldImbueChange;
			}
		}

		// Token: 0x0600171E RID: 5918 RVA: 0x0009AD12 File Offset: 0x00098F12
		private void OnHeldImbueChange(Creature creature, HashSet<string> before, HashSet<string> after)
		{
			if (!this.allowStaffBuff)
			{
				return;
			}
			this.OnHoldingMatchingStaff(creature.heldCrystalImbues.Contains(this.id));
		}

		// Token: 0x0600171F RID: 5919 RVA: 0x0009AD34 File Offset: 0x00098F34
		public virtual void OnHoldingMatchingStaff(bool holding)
		{
			if (holding)
			{
				using (Dictionary<Modifier, float>.KeyCollection.Enumerator enumerator = this.heldStaffModifiers.Keys.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Modifier key = enumerator.Current;
						base.AddModifier(this, key, this.heldStaffModifiers[key]);
					}
					return;
				}
			}
			foreach (Modifier key2 in this.heldStaffModifiers.Keys)
			{
				base.RemoveModifier(this, key2);
			}
		}

		// Token: 0x06001720 RID: 5920 RVA: 0x0009ADE4 File Offset: 0x00098FE4
		public override void Fire(bool active)
		{
			base.Fire(active);
			this.spellCaster.InvokeCastStep(this, active ? SpellCaster.CastStep.ChargeStart : SpellCaster.CastStep.ChargeStop);
			this.doneReadyHaptic = false;
			if (active)
			{
				this.spellCaster.SpawnFingersEffect(this.fingerEffectData, true, 0f, this.spellCaster.Orb);
				Handle handle = this.spellCaster.ragdollHand.grabbedHandle;
				if (handle != null)
				{
					this.spellCaster.SetMagicOffset(handle.data.spellFireMagicOffset, handle.data.offsetInHandleSpace);
				}
				else
				{
					this.spellCaster.ragdollHand.poser.SetDefaultPose(this.openHandPoseData);
					this.spellCaster.ragdollHand.poser.SetTargetPose(this.closeHandPoseData, false, false, false, false, false);
				}
				if (this.spellCaster.mana.creature == Player.currentCreature)
				{
					this.spellCaster.ragdollHand.playerHand.link.SetJointModifier(this, this.handSpringMultiplier, 1f, 1f, 1f, this.handLocomotionVelocityCorrectionMultiplier);
				}
				EffectData effectData = this.chargeEffectData;
				this.chargeEffectInstance = ((effectData != null) ? effectData.Spawn(this.spellCaster.Orb.position, this.spellCaster.Orb.rotation, this.spellCaster.Orb, null, true, null, false, 1f, 1f, Array.Empty<Type>()) : null);
				EffectInstance effectInstance = this.chargeEffectInstance;
				if (effectInstance != null)
				{
					effectInstance.SetIntensity(0f);
				}
				EffectInstance effectInstance2 = this.chargeEffectInstance;
				if (effectInstance2 != null)
				{
					effectInstance2.Play(0, false, false);
				}
				this.currentCharge = 0f;
				SpellCastCharge.SpellEvent onSpellCastEvent = this.OnSpellCastEvent;
				if (onSpellCastEvent == null)
				{
					return;
				}
				onSpellCastEvent(this);
				return;
			}
			else
			{
				if (this.spellCaster.ragdollHand.grabbedHandle)
				{
					this.spellCaster.ragdollHand.poser.SetTargetWeight(0f, false);
				}
				else
				{
					this.spellCaster.ragdollHand.poser.ResetDefaultPose();
					this.spellCaster.ragdollHand.poser.ResetTargetPose();
					this.spellCaster.ragdollHand.poser.SetTargetWeight(0f, false);
				}
				EffectInstance effectInstance3 = this.chargeEffectInstance;
				if (effectInstance3 != null)
				{
					effectInstance3.End(false, -1f);
				}
				this.spellCaster.StopFingersEffect();
				if (this.spellCaster.mana.creature == Player.currentCreature)
				{
					Creature currentCreature = Player.currentCreature;
					if (currentCreature != null && !currentCreature.isKilled)
					{
						this.spellCaster.ragdollHand.playerHand.link.RemoveJointModifier(this);
					}
				}
				if (this.allowThrow && this.spellCaster.allowCasting && !this.spellCaster.grabbedFire && this.spellCaster.mana.creature.player && !this.spellCaster.ragdollHand.playerHand.controlHand.gripPressed && !this.spellCaster.mana.mergeActive && this.currentCharge > this.throwMinCharge)
				{
					Vector3 velocity = Player.local.transform.rotation * PlayerControl.GetHand(this.spellCaster.ragdollHand.side).GetHandVelocity();
					if (velocity.magnitude > SpellCaster.throwMinHandVelocity)
					{
						this.Throw(velocity);
					}
				}
				SpellCastCharge.SpellEvent onSpellStopEvent = this.OnSpellStopEvent;
				if (onSpellStopEvent == null)
				{
					return;
				}
				onSpellStopEvent(this);
				return;
			}
		}

		// Token: 0x06001721 RID: 5921 RVA: 0x0009B15E File Offset: 0x0009935E
		public override void FireAxis(float value)
		{
			this.spellCaster.intensity = value;
		}

		// Token: 0x06001722 RID: 5922 RVA: 0x0009B16C File Offset: 0x0009936C
		public virtual void Throw(Vector3 velocity)
		{
			this.spellCaster.InvokeCastStep(this, SpellCaster.CastStep.ChargeThrow);
			SpellCastCharge.SpellThrowEvent onSpellThrowEvent = this.OnSpellThrowEvent;
			if (onSpellThrowEvent == null)
			{
				return;
			}
			onSpellThrowEvent(this, velocity);
		}

		// Token: 0x06001723 RID: 5923 RVA: 0x0009B18D File Offset: 0x0009938D
		public virtual void OnSprayStart()
		{
			this.spellCaster.InvokeCastStep(this, SpellCaster.CastStep.SprayStart);
		}

		// Token: 0x06001724 RID: 5924 RVA: 0x0009B19C File Offset: 0x0009939C
		public virtual void OnSprayLoop()
		{
		}

		// Token: 0x06001725 RID: 5925 RVA: 0x0009B19E File Offset: 0x0009939E
		public virtual void OnSprayStop()
		{
			this.spellCaster.InvokeCastStep(this, SpellCaster.CastStep.SprayEnd);
		}

		// Token: 0x06001726 RID: 5926 RVA: 0x0009B1AD File Offset: 0x000993AD
		public override void FixedUpdateCaster()
		{
			base.FixedUpdateCaster();
			if (this.spellCaster.isFiring)
			{
				SpellCastCharge.SpellEvent onSpellFixedUpdateEvent = this.OnSpellFixedUpdateEvent;
				if (onSpellFixedUpdateEvent == null)
				{
					return;
				}
				onSpellFixedUpdateEvent(this);
			}
		}

		// Token: 0x1700015D RID: 349
		// (get) Token: 0x06001727 RID: 5927 RVA: 0x0009B1D3 File Offset: 0x000993D3
		public float StaffSlamConsumption
		{
			get
			{
				if (!this.imbue)
				{
					return 0f;
				}
				return this.imbue.colliderGroup.modifier.staffSlamLoss * this.staffSlamConsumptionMult;
			}
		}

		// Token: 0x1700015E RID: 350
		// (get) Token: 0x06001728 RID: 5928 RVA: 0x0009B204 File Offset: 0x00099404
		public float ReadyThreshold
		{
			get
			{
				if (!(this.imbue == null))
				{
					return this.StaffSlamConsumption;
				}
				return Mathf.Min(this.throwMinCharge, this.sprayStartMinCharge);
			}
		}

		// Token: 0x1700015F RID: 351
		// (get) Token: 0x06001729 RID: 5929 RVA: 0x0009B22C File Offset: 0x0009942C
		public bool Ready
		{
			get
			{
				return (this.spellCaster != null && this.allowThrow && this.currentCharge > this.throwMinCharge) || (this.allowSpray && this.currentCharge > this.sprayStartMinCharge) || (this.imbue != null && this.imbue.CanConsume(this.StaffSlamConsumption) && Time.time - this.imbueHitGroundLastTime >= this.staffSlamRechargeDelay);
			}
		}

		// Token: 0x0600172A RID: 5930 RVA: 0x0009B2B0 File Offset: 0x000994B0
		public float GetChargeCurve(float charge)
		{
			float intensity = (charge < this.ReadyThreshold) ? (Mathf.InverseLerp(0f, this.ReadyThreshold, charge) * 0.5f * Mathf.Lerp(1f - this.orbVariationAmount, 1f + this.orbVariationAmount, Mathf.PerlinNoise(Time.time * this.orbVariationSpeed, (float)this.spellCaster.side))) : (Mathf.InverseLerp(this.ReadyThreshold, 1f, charge) * 0.5f + 0.5f);
			return this.chargeEffectCurve.Evaluate(intensity);
		}

		// Token: 0x0600172B RID: 5931 RVA: 0x0009B344 File Offset: 0x00099544
		public void ForceRecharge()
		{
			this.doneReadyHaptic = false;
			this.currentCharge = 0f;
		}

		// Token: 0x0600172C RID: 5932 RVA: 0x0009B358 File Offset: 0x00099558
		public override void UpdateCaster()
		{
			if (this.spellCaster.isFiring)
			{
				bool flag = this.currentCharge >= this.throwMinCharge;
				bool startedAboveMinSpray = this.currentCharge >= this.sprayStartMinCharge;
				float chargeIncrease = Mathf.Clamp(this.chargeSpeed * this.spellCaster.intensity * this.spellCaster.mana.ChargeSpeedMultiplier * this.spellCaster.chargeSpeedMult * base.GetModifier(Modifier.ChargeSpeed) * Time.deltaTime, 0f, (this.spellCaster.grabbedFire ? this.grabbedFireMaxCharge : 1f) - this.currentCharge);
				if (!this.allowCharge)
				{
					chargeIncrease = 0f;
				}
				if (Mana.fastCast)
				{
					this.currentCharge = 1f;
				}
				else
				{
					this.currentCharge = Mathf.Clamp01(this.currentCharge + chargeIncrease);
				}
				if (this.spellCaster.grabbedFire)
				{
					this.currentCharge = Mathf.Clamp(this.currentCharge, 0f, this.grabbedFireMaxCharge);
					this.spellCaster.ragdollHand.poser.SetTargetWeight(Mathf.InverseLerp(0f, this.grabbedFireMaxCharge, this.currentCharge), false);
				}
				else
				{
					this.spellCaster.ragdollHand.poser.SetTargetWeight(Mathf.Lerp(this.spellCaster.ragdollHand.poser.targetWeight, 1f - this.currentCharge, Time.deltaTime * 20f), false);
				}
				if (!flag && this.currentCharge >= this.throwMinCharge)
				{
					this.spellCaster.InvokeCastStep(this, SpellCaster.CastStep.ChargeThrowable);
				}
				if (!startedAboveMinSpray && this.currentCharge >= this.sprayStartMinCharge)
				{
					this.spellCaster.InvokeCastStep(this, SpellCaster.CastStep.ChargeSprayable);
				}
				EffectInstance effectInstance = this.chargeEffectInstance;
				if (effectInstance != null)
				{
					effectInstance.SetIntensity(this.GetChargeCurve(this.currentCharge));
				}
				this.spellCaster.SetFingersEffect(this.currentCharge);
				if (!this.doneReadyHaptic && !this.isSpraying && this.doReadyHaptic && this.Ready)
				{
					this.doneReadyHaptic = true;
					this.spellCaster.ragdollHand.HapticTick(1f, true);
					EffectData effectData = this.readyEffectData;
					if (effectData != null)
					{
						EffectInstance effectInstance2 = effectData.Spawn(this.spellCaster.magicSource.TransformPoint(this.spellCaster.magicOffset), Quaternion.identity, this.spellCaster.magicSource, null, true, null, false, 1f, 1f, Array.Empty<Type>());
						if (effectInstance2 != null)
						{
							effectInstance2.Play(0, false, true);
						}
					}
				}
				else
				{
					float haptic = Mathf.Lerp(this.chargeMinHaptic, this.chargeMaxHaptic, this.currentCharge);
					if (!this.isSpraying && haptic > 0f && this.spellCaster.mana.creature.player)
					{
						PlayerControl.GetHand(this.spellCaster.ragdollHand.side).HapticShort(haptic, false);
					}
				}
				this.UpdateSpray();
				SpellCastCharge.SpellEvent onSpellUpdateEvent = this.OnSpellUpdateEvent;
				if (onSpellUpdateEvent != null)
				{
					onSpellUpdateEvent(this);
				}
			}
			else if (this.doneReadyHaptic)
			{
				this.doneReadyHaptic = false;
			}
			if (this.allowGripCast)
			{
				SpellCaster spellCaster = this.spellCaster;
				RagdollHand ragdollHand = (spellCaster != null) ? spellCaster.ragdollHand : null;
				if (ragdollHand != null)
				{
					HandleRagdoll handleRagdoll = ragdollHand.grabbedHandle as HandleRagdoll;
					if (handleRagdoll != null && handleRagdoll.wasTkGrabbed)
					{
						PlayerHand playerHand = ragdollHand.playerHand;
						if (playerHand != null)
						{
							PlayerControl.Hand controlHand = playerHand.controlHand;
							if (controlHand != null && !controlHand.castPressed)
							{
								handleRagdoll.wasTkGrabbed = false;
							}
						}
					}
				}
			}
			if (this.allowGripCast)
			{
				SpellCaster spellCaster2 = this.spellCaster;
				RagdollHand ragdollHand = (spellCaster2 != null) ? spellCaster2.ragdollHand : null;
				if (ragdollHand != null)
				{
					HandleRagdoll handle = ragdollHand.grabbedHandle as HandleRagdoll;
					if (handle != null && !handle.wasTkGrabbed)
					{
						PlayerHand playerHand = ragdollHand.playerHand;
						if (playerHand != null)
						{
							PlayerControl.Hand controlHand = playerHand.controlHand;
							if (controlHand != null && controlHand.castPressed)
							{
								if (!this.isGripCasting)
								{
									this.isGripCasting = true;
									EffectData effectData2 = this.gripCastEffectData;
									this.gripCastEffectInstance = ((effectData2 != null) ? effectData2.Spawn(this.spellCaster.magicSource, true, null, false) : null);
									EffectInstance effectInstance3 = this.gripCastEffectInstance;
									if (effectInstance3 != null)
									{
										effectInstance3.Play(0, false, false);
									}
								}
								this.UpdateGripCast(handle);
								return;
							}
						}
					}
				}
			}
			if (!this.isGripCasting)
			{
				return;
			}
			this.isGripCasting = false;
			EffectInstance effectInstance4 = this.gripCastEffectInstance;
			if (effectInstance4 == null)
			{
				return;
			}
			effectInstance4.End(false, -1f);
		}

		// Token: 0x0600172D RID: 5933 RVA: 0x0009B7B0 File Offset: 0x000999B0
		public virtual void UpdateGripCast(HandleRagdoll handle)
		{
			if (this.gripCastDamageAmount != 0f && Time.unscaledTime - this.lastGripCastTime > this.gripCastDamageInterval)
			{
				this.lastGripCastTime = Time.unscaledTime;
				handle.ragdollPart.ragdoll.creature.Damage(this.gripCastDamageAmount);
			}
			if (!handle.ragdollPart.isSliced && this.gripCastStatusEffect != null)
			{
				handle.ragdollPart.ragdoll.creature.Inflict(this.gripCastStatusEffect, this, this.gripCastStatusDuration, null, true);
			}
		}

		// Token: 0x0600172E RID: 5934 RVA: 0x0009B840 File Offset: 0x00099A40
		private bool CanSpray()
		{
			if (!this.allowSpray)
			{
				return false;
			}
			if (this.isSpraying && this.currentCharge <= this.sprayStopMinCharge)
			{
				return false;
			}
			if (!this.isSpraying && this.currentCharge <= this.sprayStartMinCharge)
			{
				return false;
			}
			if (this.spellCaster.grabbedFire)
			{
				return false;
			}
			Vector3 headPosition = this.spellCaster.mana.creature.ragdoll.headPart.transform.position;
			Vector3 pointingDirection = this.spellCaster.magicSource.position;
			pointingDirection.x -= headPosition.x;
			pointingDirection.y -= headPosition.y;
			pointingDirection.z -= headPosition.z;
			return Vector3.Angle(this.spellCaster.fire.forward, pointingDirection) < this.sprayHeadToFireMaxAngle;
		}

		// Token: 0x0600172F RID: 5935 RVA: 0x0009B920 File Offset: 0x00099B20
		public virtual void UpdateSpray()
		{
			if (this.CanSpray())
			{
				if (!this.isSpraying)
				{
					this.spellCaster.ragdollHand.poser.SetDefaultPose(this.sprayHandPoseData);
					this.spellCaster.SetMagicOffset(this.sprayMagicOffset, false);
					this.isSpraying = true;
					this.OnSprayStart();
				}
				this.spellCaster.isSpraying = this.isSpraying;
				this.OnSprayLoop();
				return;
			}
			if (this.isSpraying)
			{
				this.spellCaster.ragdollHand.poser.SetDefaultPose(this.openHandPoseData);
				this.spellCaster.SetMagicOffset(Vector3.zero, false);
				this.isSpraying = false;
				this.OnSprayStop();
			}
		}

		// Token: 0x06001730 RID: 5936 RVA: 0x0009B9D0 File Offset: 0x00099BD0
		public override void Unload()
		{
			base.Unload();
			SpellCaster spellCaster = this.spellCaster;
			Creature creature2;
			if (spellCaster == null)
			{
				creature2 = null;
			}
			else
			{
				RagdollHand ragdollHand = spellCaster.ragdollHand;
				creature2 = ((ragdollHand != null) ? ragdollHand.creature : null);
			}
			Creature creature = creature2;
			if (creature != null)
			{
				creature.OnHeldImbueChange -= this.OnHeldImbueChange;
			}
			if (this.imbue)
			{
				if (this.isCustomImbue)
				{
					this.imbue.colliderGroup.imbueCustomFxController.SetIntensity(0f);
					this.imbue.colliderGroup.imbueCustomFxController.Stop();
				}
				else if (this.imbueEffect != null)
				{
					this.imbueEffect.blockPoolSteal = false;
					this.imbueEffect.SetIntensity(0f);
					this.imbueEffect.End(false, -1f);
				}
				SpellCastCharge.SpellEvent onImbueUnloadEvent = this.OnImbueUnloadEvent;
				if (onImbueUnloadEvent != null)
				{
					onImbueUnloadEvent(this);
				}
				this.imbue = null;
			}
			else
			{
				EffectInstance effectInstance = this.chargeEffectInstance;
				if (effectInstance != null)
				{
					effectInstance.End(false, -1f);
				}
				if (this.spellCaster)
				{
					this.spellCaster.StopFingersEffect();
					RagdollHand ragdollHand2 = this.spellCaster.ragdollHand;
					if (ragdollHand2 != null)
					{
						ragdollHand2.poser.SetTargetWeight(0f, false);
					}
				}
				SpellCastCharge.SpellEvent onSpellStopEvent = this.OnSpellStopEvent;
				if (onSpellStopEvent != null)
				{
					onSpellStopEvent(this);
				}
			}
			EffectInstance effectInstance2 = this.gripCastEffectInstance;
			if (effectInstance2 == null)
			{
				return;
			}
			effectInstance2.Despawn();
		}

		// Token: 0x06001731 RID: 5937 RVA: 0x0009BB24 File Offset: 0x00099D24
		public virtual void Load(Imbue imbue)
		{
			this.imbue = imbue;
			base.SetupModifiers();
			base.CountAndLoadSkillPassives(imbue.imbueCreature);
			this.isCustomImbue = false;
			ColliderGroupData colliderGroupData = imbue.colliderGroup.data;
			if (!imbue.colliderGroup.allowImbueEffect)
			{
				return;
			}
			EffectData effect;
			if (colliderGroupData != null && colliderGroupData.customSpellEffects && colliderGroupData.customSpellEffectData != null && colliderGroupData.customSpellEffectData.TryGetValue(this.hashId, out effect))
			{
				this.imbueEffect = effect.Spawn(imbue.transform.position, imbue.transform.rotation, imbue.transform, null, true, imbue.colliderGroup, false, imbue.EnergyRatio, 1f, (colliderGroupData != null) ? colliderGroupData.ignoredImbueEffectModules : null);
				this.imbueEffect.blockPoolSteal = colliderGroupData.blockPoolSteal;
			}
			else
			{
				switch (imbue.colliderGroup.modifier.imbueType)
				{
				case ColliderGroupData.ImbueType.Metal:
				{
					EffectData effectData = this.imbueMetalEffectData;
					this.imbueEffect = ((effectData != null) ? effectData.Spawn(imbue.transform.position, imbue.transform.rotation, imbue.transform, null, true, imbue.colliderGroup, false, imbue.EnergyRatio, 1f, (colliderGroupData != null) ? colliderGroupData.ignoredImbueEffectModules : null) : null);
					break;
				}
				case ColliderGroupData.ImbueType.Blade:
				{
					EffectData effectData2 = this.imbueBladeEffectData;
					this.imbueEffect = ((effectData2 != null) ? effectData2.Spawn(imbue.transform.position, imbue.transform.rotation, imbue.transform, null, true, imbue.colliderGroup, false, imbue.EnergyRatio, 1f, (colliderGroupData != null) ? colliderGroupData.ignoredImbueEffectModules : null) : null);
					break;
				}
				case ColliderGroupData.ImbueType.Crystal:
				{
					EffectData effectData3 = this.imbueCrystalEffectData;
					this.imbueEffect = ((effectData3 != null) ? effectData3.Spawn(imbue.transform.position, imbue.transform.rotation, imbue.transform, null, true, imbue.colliderGroup, false, imbue.EnergyRatio, 1f, (colliderGroupData != null) ? colliderGroupData.ignoredImbueEffectModules : null) : null);
					break;
				}
				case ColliderGroupData.ImbueType.Custom:
					this.isCustomImbue = true;
					break;
				}
			}
			if (this.isCustomImbue)
			{
				imbue.colliderGroup.imbueCustomFxController.Play();
				return;
			}
			if (this.imbueEffect != null)
			{
				if (imbue.colliderGroup.imbueEffectRenderer)
				{
					this.imbueEffect.SetRenderer(imbue.colliderGroup.imbueEffectRenderer, false);
				}
				else if (imbue.colliderGroup.colliders.Count == 1)
				{
					this.imbueEffect.SetCollider(imbue.colliderGroup.colliders[0]);
				}
				this.imbueEffect.SetRenderer(imbue.colliderGroup.imbueEmissionRenderer, true);
				this.imbueEffect.Play(0, false, false);
			}
		}

		// Token: 0x06001732 RID: 5938 RVA: 0x0009BDCE File Offset: 0x00099FCE
		public virtual bool OnCrystalUse(RagdollHand hand, bool active)
		{
			SpellCastCharge.CrystalUseEvent onCrystalUseEvent = this.OnCrystalUseEvent;
			if (onCrystalUseEvent != null)
			{
				onCrystalUseEvent(this, this.imbue, hand, active);
			}
			return false;
		}

		// Token: 0x06001733 RID: 5939 RVA: 0x0009BDEC File Offset: 0x00099FEC
		public virtual bool OnImbueCollisionStart(CollisionInstance collisionInstance)
		{
			if (this.imbue.colliderGroup.collisionHandler == null)
			{
				return false;
			}
			if (this.imbue.colliderGroup.modifier.imbueType != ColliderGroupData.ImbueType.Crystal)
			{
				return false;
			}
			if (collisionInstance.targetColliderGroup)
			{
				CollisionHandler collisionHandler = collisionInstance.targetColliderGroup.collisionHandler;
				bool flag;
				if (collisionHandler == null)
				{
					flag = false;
				}
				else
				{
					RagdollPart ragdollPart = collisionHandler.ragdollPart;
					bool? flag2 = (ragdollPart != null) ? new bool?(ragdollPart.ragdoll.creature.isKilled) : null;
					bool flag3 = false;
					flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
				}
				if (flag)
				{
					this.imbue.ConsumeInstant(-this.imbueHitEnergySteal * collisionInstance.impactVelocity.magnitude, false);
				}
				return false;
			}
			if (collisionInstance.impactVelocity.magnitude >= this.staffSlamMinVelocity && this.imbue.CanConsume(this.StaffSlamConsumption) && Time.time - this.imbueHitGroundLastTime >= this.staffSlamRechargeDelay)
			{
				RagdollHand lastHandler = this.imbue.colliderGroup.collisionHandler.item.lastHandler;
				if ((lastHandler == null || lastHandler.creature.isPlayer) && (this.allowEnemySelfSlam || this.imbue.colliderGroup.collisionHandler.item.handlers.Count != 1 || this.imbue.colliderGroup.collisionHandler.item.handlers[0].creature.isPlayer))
				{
					this.imbue.ConsumeInstant(this.StaffSlamConsumption, false);
					this.imbueHitGroundLastTime = Time.time;
					EffectData effectData = this.staffSlamTipEffectData;
					if (effectData != null)
					{
						effectData.Spawn(this.imbue.colliderGroup.imbueShoot ?? this.imbue.colliderGroup.transform, true, null, false).Play(0, false, false);
					}
					EffectData effectData2 = this.staffSlamCollisionEffectData;
					if (effectData2 != null)
					{
						effectData2.Spawn(collisionInstance.contactPoint, Quaternion.LookRotation(collisionInstance.contactNormal), null, collisionInstance, true, null, false, collisionInstance.intensity, 0f, Array.Empty<Type>()).Play(0, false, false);
					}
					return this.OnCrystalSlam(collisionInstance);
				}
			}
			return false;
		}

		// Token: 0x06001734 RID: 5940 RVA: 0x0009C022 File Offset: 0x0009A222
		public virtual void OnImbueCollisionStop(CollisionInstance collisionInstance)
		{
		}

		// Token: 0x06001735 RID: 5941 RVA: 0x0009C024 File Offset: 0x0009A224
		public virtual bool OnCrystalSlam(CollisionInstance collisionInstance)
		{
			return false;
		}

		// Token: 0x06001736 RID: 5942 RVA: 0x0009C028 File Offset: 0x0009A228
		public virtual void UpdateImbue(float speedRatio)
		{
			if (!this.isCustomImbue)
			{
				this.imbueEffect.SetSpeed(speedRatio);
			}
			speedRatio *= this.imbueWhooshHapticMultiplier;
			if (speedRatio > 0f && this.playImbueHaptic)
			{
				CollisionHandler collisionHandler = this.imbue.colliderGroup.collisionHandler;
				if ((collisionHandler != null) ? collisionHandler.item.leftPlayerHand : null)
				{
					PlayerControl.handLeft.HapticShort(speedRatio, false);
				}
				CollisionHandler collisionHandler2 = this.imbue.colliderGroup.collisionHandler;
				if ((collisionHandler2 != null) ? collisionHandler2.item.rightPlayerHand : null)
				{
					PlayerControl.handRight.HapticShort(speedRatio, false);
				}
			}
			if (this.imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal && this.doReadyHaptic && this.imbue.colliderGroup.collisionHandler)
			{
				if (this.Ready)
				{
					if (!this.doneReadyHaptic)
					{
						this.imbue.colliderGroup.collisionHandler.item.Haptic(1f, true);
						EffectData effectData = this.readyEffectData;
						if (effectData != null)
						{
							EffectInstance effectInstance = effectData.Spawn(this.imbue.colliderGroup.imbueShoot, true, null, false);
							if (effectInstance != null)
							{
								effectInstance.Play(0, false, true);
							}
						}
						this.doneReadyHaptic = true;
					}
				}
				else if (this.doneReadyHaptic)
				{
					this.doneReadyHaptic = false;
				}
				int numCharges = Mathf.FloorToInt(this.imbue.maxEnergy / this.StaffSlamConsumption);
				if (numCharges > 1)
				{
					int i = 1;
					while (i < numCharges)
					{
						if (this.lastEnergy < this.StaffSlamConsumption * (float)(i + 1))
						{
							if (this.imbue.energy >= this.StaffSlamConsumption * (float)(i + 1))
							{
								EffectData effectData2 = this.readyMinorEffectData;
								if (effectData2 != null)
								{
									EffectInstance effectInstance2 = effectData2.Spawn(this.imbue.colliderGroup.imbueShoot, true, null, false);
									if (effectInstance2 != null)
									{
										effectInstance2.Play(0, false, false);
									}
								}
								this.imbue.colliderGroup.collisionHandler.item.Haptic(0.5f, true);
								break;
							}
							break;
						}
						else
						{
							i++;
						}
					}
				}
				this.lastEnergy = this.imbue.energy;
			}
		}

		// Token: 0x06001737 RID: 5943 RVA: 0x0009C248 File Offset: 0x0009A448
		public virtual void SlowUpdateImbue()
		{
			if (UpdateManager.frameCount % 2 == 0)
			{
				float inverseLerp = Mathf.InverseLerp(0f, Mathf.Max(0f, this.imbue.maxEnergy), Mathf.Max(0f, this.imbue.energy));
				if (this.isCustomImbue)
				{
					this.imbue.colliderGroup.imbueCustomFxController.SetIntensity(inverseLerp);
					return;
				}
				this.imbueEffect.SetIntensity(inverseLerp);
			}
		}

		// Token: 0x06001738 RID: 5944 RVA: 0x0009C2BE File Offset: 0x0009A4BE
		public void InvokeOnGripEndEvent()
		{
			SpellCastCharge.SpellEvent onGripEndEvent = this.OnGripEndEvent;
			if (onGripEndEvent == null)
			{
				return;
			}
			onGripEndEvent(this);
		}

		// Token: 0x0400163C RID: 5692
		public static bool forceOverrideToLocalizedText;

		// Token: 0x0400163D RID: 5693
		public string castDescription;

		// Token: 0x0400163E RID: 5694
		public string imbueDescription;

		// Token: 0x0400163F RID: 5695
		public string slamDescription;

		// Token: 0x04001640 RID: 5696
		public string chargeEffectId;

		// Token: 0x04001641 RID: 5697
		[NonSerialized]
		public EffectData chargeEffectData;

		// Token: 0x04001642 RID: 5698
		public string readyEffectId;

		// Token: 0x04001643 RID: 5699
		[NonSerialized]
		public EffectData readyEffectData;

		// Token: 0x04001644 RID: 5700
		public string readyMinorEffectId = "SpellFlourishMinor";

		// Token: 0x04001645 RID: 5701
		protected EffectData readyMinorEffectData;

		// Token: 0x04001646 RID: 5702
		protected EffectInstance chargeEffectInstance;

		// Token: 0x04001647 RID: 5703
		public string fingerEffectId;

		// Token: 0x04001648 RID: 5704
		protected EffectData fingerEffectData;

		// Token: 0x04001649 RID: 5705
		public string closeHandPoseId;

		// Token: 0x0400164A RID: 5706
		[NonSerialized]
		public HandPoseData closeHandPoseData;

		// Token: 0x0400164B RID: 5707
		public string openHandPoseId;

		// Token: 0x0400164C RID: 5708
		[NonSerialized]
		public HandPoseData openHandPoseData;

		// Token: 0x0400164D RID: 5709
		public bool doReadyHaptic = true;

		// Token: 0x0400164E RID: 5710
		public AnimationCurve chargeEffectCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		// Token: 0x0400164F RID: 5711
		public float orbVariationSpeed = 10f;

		// Token: 0x04001650 RID: 5712
		public float orbVariationAmount = 0.2f;

		// Token: 0x04001651 RID: 5713
		public float chargeSpeed = 0.5f;

		// Token: 0x04001652 RID: 5714
		public bool allowStaffBuff;

		// Token: 0x04001653 RID: 5715
		public Dictionary<Modifier, float> heldStaffModifiers;

		// Token: 0x04001654 RID: 5716
		public float chargeSpeedPerSkill = 0.05f;

		// Token: 0x04001655 RID: 5717
		public float grabbedFireMaxCharge = 0.5f;

		// Token: 0x04001656 RID: 5718
		public bool allowUnderwater;

		// Token: 0x04001657 RID: 5719
		public bool endOnGrip = true;

		// Token: 0x04001658 RID: 5720
		public bool allowCharge = true;

		// Token: 0x04001659 RID: 5721
		public float chargeMinHaptic = 0.05f;

		// Token: 0x0400165A RID: 5722
		public float chargeMaxHaptic = 0.3f;

		// Token: 0x0400165B RID: 5723
		public float handSpringMultiplier = 0.8f;

		// Token: 0x0400165C RID: 5724
		public float handLocomotionVelocityCorrectionMultiplier = 1f;

		// Token: 0x0400165D RID: 5725
		public bool allowThrow = true;

		// Token: 0x0400165E RID: 5726
		public string throwEffectId;

		// Token: 0x0400165F RID: 5727
		[NonSerialized]
		public EffectData throwEffectData;

		// Token: 0x04001660 RID: 5728
		public float throwMinCharge = 0.9f;

		// Token: 0x04001661 RID: 5729
		public bool allowSpray;

		// Token: 0x04001662 RID: 5730
		public string sprayHandPoseId;

		// Token: 0x04001663 RID: 5731
		[NonSerialized]
		public HandPoseData sprayHandPoseData;

		// Token: 0x04001664 RID: 5732
		public Vector3 sprayMagicOffset;

		// Token: 0x04001665 RID: 5733
		public float sprayHeadToFireMaxAngle = 45f;

		// Token: 0x04001666 RID: 5734
		public float sprayStopMinCharge = 0.2f;

		// Token: 0x04001667 RID: 5735
		public float sprayStartMinCharge = 0.5f;

		// Token: 0x04001668 RID: 5736
		public string gripCastEffectId;

		// Token: 0x04001669 RID: 5737
		[NonSerialized]
		public EffectData gripCastEffectData;

		// Token: 0x0400166A RID: 5738
		public string gripCastStatusEffectId;

		// Token: 0x0400166B RID: 5739
		public float gripCastStatusDuration;

		// Token: 0x0400166C RID: 5740
		[NonSerialized]
		public bool allowGripCast;

		// Token: 0x0400166D RID: 5741
		[NonSerialized]
		protected float lastGripCastTime;

		// Token: 0x0400166E RID: 5742
		[NonSerialized]
		public float gripCastDamageInterval;

		// Token: 0x0400166F RID: 5743
		[NonSerialized]
		public float gripCastDamageAmount;

		// Token: 0x04001670 RID: 5744
		[NonSerialized]
		public StatusData gripCastStatusEffect;

		// Token: 0x04001671 RID: 5745
		[NonSerialized]
		public bool isGripCasting;

		// Token: 0x04001672 RID: 5746
		[NonSerialized]
		public EffectInstance gripCastEffectInstance;

		// Token: 0x04001673 RID: 5747
		[NonSerialized]
		public bool isSpraying;

		// Token: 0x04001674 RID: 5748
		[NonSerialized]
		public float currentCharge;

		// Token: 0x04001675 RID: 5749
		public bool imbueEnabled = true;

		// Token: 0x04001676 RID: 5750
		public bool imbueAllowMetal;

		// Token: 0x04001677 RID: 5751
		public float imbueRate = 1f;

		// Token: 0x04001678 RID: 5752
		public float imbueLossMultiplier = 1f;

		// Token: 0x04001679 RID: 5753
		public float imbueRadius = 0.2f;

		// Token: 0x0400167A RID: 5754
		public bool imbueHitUseDamager;

		// Token: 0x0400167B RID: 5755
		public float imbueHitMinVelocity = 4f;

		// Token: 0x0400167C RID: 5756
		public float imbueWhooshMinSpeed = 4f;

		// Token: 0x0400167D RID: 5757
		public float imbueWhooshMaxSpeed = 12f;

		// Token: 0x0400167E RID: 5758
		public float imbueWhooshHapticMultiplier = 1f;

		// Token: 0x0400167F RID: 5759
		public string imbueMetalEffectId;

		// Token: 0x04001680 RID: 5760
		[NonSerialized]
		public EffectData imbueMetalEffectData;

		// Token: 0x04001681 RID: 5761
		public string imbueBladeEffectId;

		// Token: 0x04001682 RID: 5762
		[NonSerialized]
		public EffectData imbueBladeEffectData;

		// Token: 0x04001683 RID: 5763
		public string imbueCrystalEffectId;

		// Token: 0x04001684 RID: 5764
		[NonSerialized]
		public EffectData imbueCrystalEffectData;

		// Token: 0x04001685 RID: 5765
		public string imbueNaaEffectId;

		// Token: 0x04001686 RID: 5766
		[NonSerialized]
		public EffectData imbueNaaEffectData;

		// Token: 0x04001687 RID: 5767
		public string imbueTransferEffectId;

		// Token: 0x04001688 RID: 5768
		[NonSerialized]
		public EffectData imbueTransferEffectData;

		// Token: 0x04001689 RID: 5769
		public float imbueHitEnergySteal = 1f;

		// Token: 0x0400168A RID: 5770
		public float staffSlamRechargeDelay = 2f;

		// Token: 0x0400168B RID: 5771
		public float staffSlamMinVelocity = 4f;

		// Token: 0x0400168C RID: 5772
		public float staffSlamConsumptionMult = 1f;

		// Token: 0x0400168D RID: 5773
		public float staffSlamMaxVelocity = 15f;

		// Token: 0x0400168E RID: 5774
		public string staffSlamTipEffectId;

		// Token: 0x0400168F RID: 5775
		[NonSerialized]
		public EffectData staffSlamTipEffectData;

		// Token: 0x04001690 RID: 5776
		public string staffSlamCollisionEffectId;

		// Token: 0x04001691 RID: 5777
		[NonSerialized]
		public EffectData staffSlamCollisionEffectData;

		// Token: 0x04001692 RID: 5778
		public bool allowEnemySelfSlam;

		// Token: 0x0400169B RID: 5787
		[NonSerialized]
		public bool playImbueHaptic = true;

		// Token: 0x0400169C RID: 5788
		[NonSerialized]
		public EffectInstance imbueEffect;

		// Token: 0x0400169D RID: 5789
		protected bool isCustomImbue;

		// Token: 0x0400169E RID: 5790
		protected bool doneReadyHaptic;

		// Token: 0x0400169F RID: 5791
		protected float imbueHitGroundLastTime;

		// Token: 0x040016A0 RID: 5792
		protected float lastEnergy;

		// Token: 0x040016A1 RID: 5793
		[NonSerialized]
		public SpellCaster spellCaster;

		// Token: 0x040016A2 RID: 5794
		[NonSerialized]
		public Imbue imbue;

		// Token: 0x0200083B RID: 2107
		// (Invoke) Token: 0x06003F64 RID: 16228
		public delegate void SpellEvent(SpellCastCharge spell);

		// Token: 0x0200083C RID: 2108
		// (Invoke) Token: 0x06003F68 RID: 16232
		public delegate void SpellThrowEvent(SpellCastCharge spell, Vector3 velocity);

		// Token: 0x0200083D RID: 2109
		// (Invoke) Token: 0x06003F6C RID: 16236
		public delegate void CrystalUseEvent(SpellCastCharge spell, Imbue imbue, RagdollHand hand, bool active);
	}
}
