using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200026F RID: 623
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/HandleRagdoll")]
	[AddComponentMenu("ThunderRoad/Creatures/Handle ragdoll")]
	public class HandleRagdoll : Handle
	{
		// Token: 0x06001BDA RID: 7130 RVA: 0x000B848A File Offset: 0x000B668A
		protected override void Awake()
		{
			this.initialTouchRadius = this.touchRadius;
			this.ragdollPart = base.GetComponentInParent<RagdollPart>();
			base.Awake();
		}

		// Token: 0x06001BDB RID: 7131 RVA: 0x000B84AC File Offset: 0x000B66AC
		protected override void Start()
		{
			base.Start();
			this.ragdollPart.ragdoll.creature.brain.OnStateChangeEvent += this.BrainStateChange;
			if (this.handleRagdollData.allowTelekinesis && this.handleRagdollData.tkActiveCondition != HandleRagdollData.TkCondition.Always)
			{
				this.ragdollPart.ragdoll.creature.OnDespawnEvent -= this.CreatureDespawned;
				this.ragdollPart.ragdoll.creature.OnDespawnEvent += this.CreatureDespawned;
				if (this.handleRagdollData.tkActiveCondition == HandleRagdollData.TkCondition.CreatureDead)
				{
					this.ragdollPart.ragdoll.creature.OnKillEvent -= this.CreatureKilled;
					this.ragdollPart.ragdoll.creature.OnKillEvent += this.CreatureKilled;
				}
				if (this.handleRagdollData.tkActiveCondition == HandleRagdollData.TkCondition.PartSliced)
				{
					this.ragdollPart.ragdoll.OnSliceEvent -= this.RagdollSliced;
					this.ragdollPart.ragdoll.OnSliceEvent += this.RagdollSliced;
				}
			}
		}

		// Token: 0x06001BDC RID: 7132 RVA: 0x000B85E0 File Offset: 0x000B67E0
		public override void Load(InteractableData interactableData)
		{
			if (!(interactableData is HandleRagdollData))
			{
				Debug.LogError("Trying to load wrong data type");
				return;
			}
			base.Load(interactableData as HandleRagdollData);
			this.handleRagdollData = (this.data as HandleRagdollData);
			if (this.handleRagdollData.allowTelekinesis)
			{
				this.SetTelekinesis(this.handleRagdollData.tkActiveCondition == HandleRagdollData.TkCondition.Always);
			}
		}

		// Token: 0x06001BDD RID: 7133 RVA: 0x000B863E File Offset: 0x000B683E
		private void CreatureDespawned(EventTime eventTime)
		{
			this.SetTelekinesis(false);
		}

		// Token: 0x06001BDE RID: 7134 RVA: 0x000B8647 File Offset: 0x000B6847
		private void CreatureKilled(CollisionInstance collisionInstance, EventTime eventTime)
		{
			this.SetTelekinesis(true);
		}

		// Token: 0x06001BDF RID: 7135 RVA: 0x000B8650 File Offset: 0x000B6850
		private void RagdollSliced(RagdollPart ragdollPart, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd && this.ragdollPart.isSliced)
			{
				this.SetTelekinesis(true);
			}
		}

		// Token: 0x06001BE0 RID: 7136 RVA: 0x000B866A File Offset: 0x000B686A
		protected override bool HoldGripToGrab()
		{
			return this.handleRagdollData.forceHoldGripToGrab || HandleRagdoll.holdGripRagdoll;
		}

		// Token: 0x170001C0 RID: 448
		// (get) Token: 0x06001BE1 RID: 7137 RVA: 0x000B8680 File Offset: 0x000B6880
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return base.EnabledManagedLoops | ManagedLoops.Update;
			}
		}

		// Token: 0x06001BE2 RID: 7138 RVA: 0x000B868C File Offset: 0x000B688C
		protected internal override void ManagedUpdate()
		{
			if (!this.ragdollPart.initialized)
			{
				return;
			}
			if ((this.ragdollPart.ragdoll.isGrabbed || this.ragdollPart.ragdoll.isTkGrabbed) && (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive || this.ragdollPart.ragdoll.standingUp) && (this.handlers.Count > 0 || base.IsTkGrabbed))
			{
				if (this.handleRagdollData.CheckForceLiftCondition(this.ragdollPart.ragdoll.creature))
				{
					if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
					{
						this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
					}
				}
				else
				{
					if (this.handleRagdollData.bodyTurnDirection != HandleRagdollData.BodyTurnDirection.None)
					{
						foreach (RagdollHand handler in this.handlers)
						{
							Vector3 handDirectionXZ = handler.bone.animation.position.ToXZ() - handler.gripInfo.transform.position.ToXZ();
							Vector3 upperArmDirectionXZ = handler.upperArmPart.transform.position.ToXZ() - this.ragdollPart.ragdoll.creature.transform.position.ToXZ();
							float targetAngle = 0f;
							if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.HandDirection)
							{
								targetAngle = Vector3.SignedAngle(handDirectionXZ, upperArmDirectionXZ, this.ragdollPart.ragdoll.creature.transform.up);
							}
							else if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.PartDirection)
							{
								targetAngle = Vector3.SignedAngle(this.isBackGrab ? (-this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ()) : this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ(), upperArmDirectionXZ, this.ragdollPart.ragdoll.creature.transform.up);
							}
							else if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.GrabberPosition)
							{
								if (this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(handler.creature.transform.position).z > 0f)
								{
									targetAngle = Vector3.SignedAngle(this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ(), upperArmDirectionXZ, this.ragdollPart.ragdoll.creature.transform.up);
								}
								else
								{
									targetAngle = Vector3.SignedAngle(-this.ragdollPart.ragdoll.rootPart.transform.forward.ToXZ(), upperArmDirectionXZ, this.ragdollPart.ragdoll.creature.transform.up);
								}
							}
							else if (this.handleRagdollData.bodyTurnDirection == HandleRagdollData.BodyTurnDirection.ClosestCardinal)
							{
								Vector3 cardinalDirection = Utils.ClosestDirection(this.ragdollPart.ragdoll.creature.transform.InverseTransformDirection(handDirectionXZ), this.handleRagdollData.cardinal);
								cardinalDirection = this.ragdollPart.ragdoll.creature.transform.TransformDirection(cardinalDirection);
								targetAngle = Vector3.SignedAngle(cardinalDirection, upperArmDirectionXZ, this.ragdollPart.ragdoll.creature.transform.up);
							}
							this.ragdollPart.ragdoll.creature.transform.Rotate(this.ragdollPart.ragdoll.creature.transform.up, targetAngle * this.ragdollPart.ragdoll.creature.turnSpeed * Time.deltaTime);
						}
					}
					if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
					{
						if (this.handleRagdollData.moveStep)
						{
							if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Head)
							{
								this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.ragdoll.headPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
							}
							else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Root)
							{
								this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.ragdoll.rootPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
							}
							else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Target)
							{
								this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.ragdoll.targetPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
							}
							else if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Self)
							{
								this.ragdollPart.ragdoll.creature.UpdateStep(this.ragdollPart.transform.position, this.handleRagdollData.stepSpeedMultiplier, this.handleRagdollData.stepThresholdMultiplier);
							}
						}
						if (this.handleRagdollData.changeHeight)
						{
							float height = this.ragdollPart.bone.GetAnimationBoneHeight();
							this.ragdollPart.ragdoll.creature.SetAnimatorHeightRatio(this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(this.bodyAnchor.position).y / height);
						}
						if (this.handleRagdollData.liftBehaviour != HandleRagdollData.LiftBehaviour.None)
						{
							RagdollPart liftPartReference = this.ragdollPart.ragdoll.rootPart;
							if (this.handleRagdollData.liftPartReference == HandleRagdollData.Part.Head)
							{
								liftPartReference = this.ragdollPart.ragdoll.headPart;
							}
							else if (this.handleRagdollData.liftPartReference == HandleRagdollData.Part.Target)
							{
								liftPartReference = this.ragdollPart.ragdoll.targetPart;
							}
							else if (this.handleRagdollData.liftPartReference == HandleRagdollData.Part.Self)
							{
								liftPartReference = this.ragdollPart;
							}
							if (this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(liftPartReference.transform.position).y > liftPartReference.bone.orgCreatureLocalPosition.y + this.handleRagdollData.liftOffset)
							{
								if (this.handleRagdollData.liftBehaviour == HandleRagdollData.LiftBehaviour.Fall)
								{
									this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
								}
								else if (this.handleRagdollData.liftBehaviour == HandleRagdollData.LiftBehaviour.Ungrab && this.handlers.Count > 0)
								{
									this.handlers[0].UnGrab(false);
								}
							}
						}
						if (this.ragdollPart.ragdoll.creature.transform.InverseTransformPoint(this.ragdollPart.ragdoll.headPart.transform.position).y < this.ragdollPart.ragdoll.rootPart.bone.orgCreatureLocalPosition.y)
						{
							this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
						}
					}
				}
			}
			if (this.handleRagdollData.liftSpinningFix && this.ragdollPart.ragdoll.isGrabbed && !this.ragdollPart.ragdoll.isTkGrabbed && this.ragdollPart.ragdoll.handlers.Count == 1)
			{
				PhysicBody thisRb = this.ragdollPart.ragdoll.rootPart.physicBody;
				if (this.ragdollPart.ragdoll.state != Ragdoll.State.Destabilized || this.ragdollPart.ragdoll.creature.fallState != Creature.FallState.Falling)
				{
					thisRb.constraints = RigidbodyConstraints.None;
					return;
				}
				Vector3 gripForward = Vector3.ProjectOnPlane(this.ragdollPart.ragdoll.handlers[0].playerHand.grip.gameObject.transform.rotation * this.grabDir, Vector3.up);
				Debug.DrawLine(thisRb.transform.position, thisRb.transform.position + this.grabDir * 4f, Color.black);
				Debug.DrawLine(thisRb.transform.position, thisRb.transform.position + gripForward * 4f, Color.white);
				float num = Mathf.Max(0f, thisRb.velocity.magnitude);
				float minSpeed = 2f;
				if (num < minSpeed)
				{
					thisRb.constraints = RigidbodyConstraints.FreezeRotationX;
					float angle = Vector3.Angle(Vector3.ProjectOnPlane(thisRb.transform.forward, Vector3.up), Vector3.ProjectOnPlane(gripForward, Vector3.up));
					thisRb.transform.rotation = Quaternion.Slerp(thisRb.transform.rotation, Quaternion.LookRotation(gripForward), Time.deltaTime * Mathf.Min(angle / 180f * this.handleRagdollData.liftSpinningFacingPower, this.handleRagdollData.liftSpinningFacingMaxSpeed));
				}
			}
		}

		// Token: 0x06001BE3 RID: 7139 RVA: 0x000B8FE0 File Offset: 0x000B71E0
		private void BrainStateChange(Brain.State state)
		{
			if (this.lastBrainState == Brain.State.Combat || state == Brain.State.Combat)
			{
				if (this.combatScaleCoroutine != null)
				{
					this.ragdollPart.ragdoll.creature.StopCoroutine(this.combatScaleCoroutine);
				}
				this.combatScaleCoroutine = this.ragdollPart.ragdoll.creature.StartCoroutine(this.CombatScale(state == Brain.State.Combat));
			}
			this.lastBrainState = state;
		}

		// Token: 0x06001BE4 RID: 7140 RVA: 0x000B9049 File Offset: 0x000B7249
		protected IEnumerator CombatScale(bool combat)
		{
			float touchRadiusMultiplier = this.touchRadius / this.initialTouchRadius;
			float lerpStart = combat ? 1f : this.handleRagdollData.scaleDuringCombat;
			float lerpEnd = combat ? this.handleRagdollData.scaleDuringCombat : 1f;
			float changeEnd = Time.time + HandleRagdoll.combatScaleChangeTime * Mathf.InverseLerp(lerpStart, lerpEnd, touchRadiusMultiplier);
			while (Time.time < changeEnd)
			{
				touchRadiusMultiplier = Mathf.MoveTowards(touchRadiusMultiplier, lerpEnd, Mathf.Abs(lerpEnd - lerpStart) * (Time.deltaTime / HandleRagdoll.combatScaleChangeTime));
				base.SetTouchRadius(this.initialTouchRadius * touchRadiusMultiplier, false);
				yield return Yielders.FixedUpdate;
			}
			base.SetTouchRadius(this.initialTouchRadius * lerpEnd, false);
			this.combatScaleCoroutine = null;
			yield break;
		}

		// Token: 0x06001BE5 RID: 7141 RVA: 0x000B9060 File Offset: 0x000B7260
		protected void ResetStep()
		{
			if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive && this.handleRagdollData.moveStep)
			{
				if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Head)
				{
					this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.ragdoll.headPart.transform.position;
					return;
				}
				if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Root)
				{
					this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.ragdoll.rootPart.transform.position;
					return;
				}
				if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Target)
				{
					this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.ragdoll.targetPart.transform.position;
					return;
				}
				if (this.handleRagdollData.stepPartReference == HandleRagdollData.Part.Self)
				{
					this.ragdollPart.ragdoll.creature.stepTargetPos = this.ragdollPart.transform.position;
				}
			}
		}

		// Token: 0x06001BE6 RID: 7142 RVA: 0x000B9184 File Offset: 0x000B7384
		public override void OnTelekinesisGrab(SpellTelekinesis spellTelekinesis)
		{
			if (this.ragdollPart.ragdoll.state == Ragdoll.State.NoPhysic || this.ragdollPart.ragdoll.state == Ragdoll.State.Kinematic)
			{
				this.ragdollPart.ragdoll.SetState(Ragdoll.State.Standing);
			}
			this.telekinesisHandlers.Add(spellTelekinesis.spellCaster);
			this.ragdollPart.ragdoll.tkHandlers.Add(spellTelekinesis.spellCaster);
			if (this.handleRagdollData.useIK)
			{
				this.bodyAnchor = spellTelekinesis.grip.transform;
				if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
				{
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadAnchor(spellTelekinesis.grip.transform);
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadState(true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadWeight(this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
				else if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
				{
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Left, spellTelekinesis.grip.transform);
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandState(Side.Left, true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandWeight(Side.Left, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
				else if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
				{
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Right, spellTelekinesis.grip.transform);
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandState(Side.Right, true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandWeight(Side.Right, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
			}
			else if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
			{
				this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, true, false, RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand, null);
			}
			else if (this.ragdollPart.type == RagdollPart.Type.RightArm)
			{
				this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, true, false, RagdollPart.Type.RightArm | RagdollPart.Type.RightHand, null);
			}
			if (this.ragdollPart.ragdoll.tkHandlers.Count > 1 && spellTelekinesis.allowDismemberment && GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment, BuildSettings.ContentFlagBehaviour.Discard))
			{
				this.ragdollPart.ragdoll.EnableCharJointBreakForce(spellTelekinesis.dismembermentBreakForceMultiplier);
			}
			this.ResetStep();
			this.RefreshJointAndCollision();
			this.ragdollPart.ragdoll.isTkGrabbed = true;
			this.ragdollPart.ragdoll.InvokeTelekinesisGrabEvent(spellTelekinesis, this);
			this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
			this.ragdollPart.ragdoll.creature.lastInteractionCreature = spellTelekinesis.spellCaster.ragdollHand.creature;
		}

		// Token: 0x06001BE7 RID: 7143 RVA: 0x000B95B4 File Offset: 0x000B77B4
		public override void OnTelekinesisRelease(SpellTelekinesis spellTelekinesis, bool tryThrow, out bool throwing, bool grabbing)
		{
			throwing = false;
			this.telekinesisHandlers.Remove(spellTelekinesis.spellCaster);
			this.ragdollPart.ragdoll.tkHandlers.Remove(spellTelekinesis.spellCaster);
			if (this.handleRagdollData.useIK)
			{
				if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
				{
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadAnchor(null);
				}
				if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
				{
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Left, null);
				}
				if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
				{
					this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Right, null);
				}
				this.bodyAnchor = null;
			}
			this.ragdollPart.ragdoll.RefreshPartJointAndCollision();
			if (this.ragdollPart.ragdoll.charJointBreakEnabled && this.ragdollPart.ragdoll.tkHandlers.Count < 2)
			{
				this.ragdollPart.ragdoll.DisableCharJointBreakForce();
			}
			if (this.ragdollPart.ragdoll.tkHandlers.Count == 0)
			{
				this.ragdollPart.ragdoll.isTkGrabbed = false;
				this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
				this.ragdollPart.ragdoll.creature.lastInteractionCreature = spellTelekinesis.spellCaster.ragdollHand.creature;
				if (tryThrow)
				{
					Vector3 controllerVelocity = Player.local.transform.rotation * PlayerControl.GetHand(spellTelekinesis.spellCaster.ragdollHand.side).GetHandVelocity();
					if (controllerVelocity.magnitude > SpellCaster.throwMinHandVelocity)
					{
						if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
						{
							if (spellTelekinesis.forceDestabilizeOnThrow && !this.ragdollPart.ragdoll.creature.isKilled)
							{
								this.ragdollPart.ragdoll.SetState(Ragdoll.State.Destabilized);
							}
							this.ragdollPart.ragdoll.creature.TryPush(Creature.PushType.Grab, controllerVelocity.normalized, spellTelekinesis.grabThrowLevel, this.ragdollPart.type);
						}
						if (this.ragdollPart.isSliced)
						{
							this.ragdollPart.physicBody.AddForce(controllerVelocity.normalized * spellTelekinesis.pushRagdollForce, ForceMode.VelocityChange);
						}
						else
						{
							foreach (RagdollPart ragdollPart in this.ragdollPart.ragdoll.parts)
							{
								if (ragdollPart == this.ragdollPart)
								{
									ragdollPart.physicBody.AddForce(controllerVelocity.normalized * spellTelekinesis.pushRagdollForce, ForceMode.VelocityChange);
								}
								else if (!ragdollPart.isSliced)
								{
									ragdollPart.physicBody.AddForce(controllerVelocity.normalized * spellTelekinesis.pushRagdollOtherPartsForce, ForceMode.VelocityChange);
								}
							}
						}
						if (spellTelekinesis.clearFloatingOnThrow)
						{
							this.ragdollPart.ragdoll.creature.Clear("Floating");
						}
						throwing = true;
					}
				}
				this.ragdollPart.ragdoll.InvokeTelekinesisReleaseEvent(spellTelekinesis, this, true);
				return;
			}
			this.ragdollPart.ragdoll.InvokeTelekinesisReleaseEvent(spellTelekinesis, this, false);
		}

		// Token: 0x06001BE8 RID: 7144 RVA: 0x000B9950 File Offset: 0x000B7B50
		public override void OnGrab(RagdollHand ragdollHand, float axisPosition, HandlePose orientation, bool teleportToHand = false)
		{
			this.ragdollPart.ragdoll.CancelGetUp(true);
			this.wasTkGrabbed = false;
			base.OnGrab(ragdollHand, axisPosition, orientation, teleportToHand);
			if (!this.bodyAnchor)
			{
				this.bodyAnchor = new GameObject("BodyAnchor" + this.name).transform;
			}
			this.bodyAnchor.SetParent(ragdollHand.playerHand.grip.transform);
			this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.transform.position);
			this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.transform.rotation) * orientation.transform.rotation;
			if (this.handleRagdollData.useIK)
			{
				if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
				{
					if (this.ragdollPart.type == RagdollPart.Type.Neck)
					{
						this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.ragdoll.headPart.transform.position);
						this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.ragdoll.headPart.transform.rotation) * orientation.transform.rotation;
					}
					this.ragdollPart.ragdoll.ik.SetHeadAnchor(this.bodyAnchor);
					this.ragdollPart.ragdoll.ik.SetHeadState(true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.ik.SetHeadWeight(this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
				else if (this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftShoulder) == this.ragdollPart.bone.animation)
				{
					this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.ragdoll.leftUpperArmPart.transform.position);
					this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.ragdoll.leftUpperArmPart.transform.rotation) * orientation.transform.rotation;
					this.ragdollPart.ragdoll.ik.SetShoulderAnchor(Side.Left, this.bodyAnchor);
					this.ragdollPart.ragdoll.ik.SetShoulderState(Side.Left, true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.ik.SetShoulderWeight(Side.Left, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
				else if (this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightShoulder) == this.ragdollPart.bone.animation)
				{
					this.bodyAnchor.localPosition = orientation.transform.InverseTransformPointUnscaled(this.ragdollPart.ragdoll.rightUpperArmPart.transform.position);
					this.bodyAnchor.localRotation = Quaternion.Inverse(this.ragdollPart.ragdoll.rightUpperArmPart.transform.rotation) * orientation.transform.rotation;
					this.ragdollPart.ragdoll.ik.SetShoulderAnchor(Side.Right, this.bodyAnchor);
					this.ragdollPart.ragdoll.ik.SetShoulderState(Side.Right, true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.ik.SetShoulderWeight(Side.Right, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
				else if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
				{
					if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
					{
						Transform transform = this.bodyAnchor;
						Transform transform2 = orientation.transform;
						RagdollHand handLeft = this.ragdollPart.ragdoll.creature.handLeft;
						transform.localPosition = transform2.InverseTransformPointUnscaled((handLeft != null) ? handLeft.transform.position : base.transform.position);
						Transform transform3 = this.bodyAnchor;
						RagdollHand handLeft2 = this.ragdollPart.ragdoll.creature.handLeft;
						transform3.localRotation = Quaternion.Inverse((handLeft2 != null) ? handLeft2.transform.rotation : base.transform.rotation) * orientation.transform.rotation;
					}
					this.ragdollPart.ragdoll.ik.SetHandAnchor(Side.Left, this.bodyAnchor);
					this.ragdollPart.ragdoll.ik.SetHandState(Side.Left, true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.ik.SetHandWeight(Side.Left, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
				else if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
				{
					if (this.ragdollPart.type == RagdollPart.Type.RightArm)
					{
						Transform transform4 = this.bodyAnchor;
						Transform transform5 = orientation.transform;
						RagdollHand handRight = this.ragdollPart.ragdoll.creature.handRight;
						transform4.localPosition = transform5.InverseTransformPointUnscaled((handRight != null) ? handRight.transform.position : base.transform.position);
						Transform transform6 = this.bodyAnchor;
						RagdollHand handRight2 = this.ragdollPart.ragdoll.creature.handRight;
						transform6.localRotation = Quaternion.Inverse((handRight2 != null) ? handRight2.transform.rotation : base.transform.rotation) * orientation.transform.rotation;
					}
					this.ragdollPart.ragdoll.ik.SetHandAnchor(Side.Right, this.bodyAnchor);
					this.ragdollPart.ragdoll.ik.SetHandState(Side.Right, true, this.handleRagdollData.allowRotationIK);
					this.ragdollPart.ragdoll.ik.SetHandWeight(Side.Right, this.handleRagdollData.IkPositionWeight, this.handleRagdollData.allowRotationIK ? this.handleRagdollData.IkRotationWeight : 0f);
				}
			}
			else if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
			{
				this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, true, false, RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand, null);
			}
			else if (this.ragdollPart.type == RagdollPart.Type.RightArm)
			{
				this.ragdollPart.ragdoll.SetPinForceMultiplier(this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, this.handleRagdollData.releaseArmSpring, this.handleRagdollData.releaseArmDamper, true, false, RagdollPart.Type.RightArm | RagdollPart.Type.RightHand, null);
			}
			if (this.handleRagdollData.liftSpinningFix)
			{
				PhysicBody thisRb = this.ragdollPart.ragdoll.rootPart.physicBody;
				if (thisRb != null)
				{
					this.grabDir = thisRb.transform.forward;
				}
			}
			foreach (Collider handCollider in ragdollHand.colliderGroup.colliders)
			{
				this.ragdollPart.ragdoll.IgnoreCollision(handCollider, true, (RagdollPart.Type)0);
			}
			if (Vector3.Angle(base.transform.forward.ToXZ(), orientation.transform.forward.ToXZ()) > 90f)
			{
				this.isBackGrab = false;
			}
			else
			{
				this.isBackGrab = true;
			}
			this.ragdollPart.isGrabbed = true;
			this.ragdollPart.ragdoll.creature.locomotion.StartShrinkCollider();
			if (this.ragdollPart.type == RagdollPart.Type.LeftHand || this.ragdollPart.type == RagdollPart.Type.LeftArm)
			{
				if (this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle && this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item)
				{
					this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item.RefreshCollision(false);
				}
			}
			else if ((this.ragdollPart.type == RagdollPart.Type.RightHand || this.ragdollPart.type == RagdollPart.Type.RightArm) && this.ragdollPart.ragdoll.creature.handRight.grabbedHandle && this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item)
			{
				this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item.RefreshCollision(false);
			}
			this.ResetStep();
			this.RefreshJointAndCollision();
			this.ragdollPart.ragdoll.isGrabbed = true;
			this.ragdollPart.ragdoll.handlers.Add(ragdollHand);
			this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
			this.ragdollPart.ragdoll.creature.lastInteractionCreature = ragdollHand.creature;
			this.ragdollPart.ragdoll.InvokeGrabEvent(ragdollHand, this);
		}

		// Token: 0x06001BE9 RID: 7145 RVA: 0x000BA350 File Offset: 0x000B8550
		private void CreateStabilizationJoint(RagdollHand ragdollHand)
		{
			Ragdoll.StabilizationJointSettings stabilizationJointSettings = new Ragdoll.StabilizationJointSettings();
			stabilizationJointSettings.angularXDrive = new JointDrive
			{
				positionSpring = this.handleRagdollData.stabilizationPositionSpring,
				positionDamper = this.handleRagdollData.stabilizationPositionDamper,
				maximumForce = this.handleRagdollData.stabilizationPositionMaxForce
			};
			stabilizationJointSettings.axis = new Vector3(0f, 1f, 0f);
			stabilizationJointSettings.isKinematic = true;
			this.ragdollPart.ragdoll.AddStabilizationJoint(base.gameObject, stabilizationJointSettings);
		}

		// Token: 0x06001BEA RID: 7146 RVA: 0x000BA3E0 File Offset: 0x000B85E0
		public override void OnUnGrab(RagdollHand ragdollHand, bool throwing)
		{
			base.OnUnGrab(ragdollHand, throwing);
			this.ragdollPart.ragdoll.handlers.Remove(ragdollHand);
			if (this.ragdollPart.type == RagdollPart.Type.LeftHand || this.ragdollPart.type == RagdollPart.Type.LeftArm)
			{
				RagdollHand handLeft = this.ragdollPart.ragdoll.creature.handLeft;
				if (((handLeft != null) ? handLeft.grabbedHandle : null) && this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item)
				{
					this.ragdollPart.ragdoll.creature.handLeft.grabbedHandle.item.RefreshCollision(false);
				}
			}
			else if (this.ragdollPart.type == RagdollPart.Type.RightHand || this.ragdollPart.type == RagdollPart.Type.RightArm)
			{
				RagdollHand handRight = this.ragdollPart.ragdoll.creature.handRight;
				if (((handRight != null) ? handRight.grabbedHandle : null) && this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item)
				{
					this.ragdollPart.ragdoll.creature.handRight.grabbedHandle.item.RefreshCollision(false);
				}
			}
			if (this.handleRagdollData.useIK && this.bodyAnchor)
			{
				if (this.handlers.Count == 0)
				{
					if (this.ragdollPart.type == RagdollPart.Type.Head || this.ragdollPart.type == RagdollPart.Type.Neck)
					{
						this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHeadAnchor(null);
					}
					else if (this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftShoulder) == this.ragdollPart.bone.animation)
					{
						this.ragdollPart.ragdoll.creature.ragdoll.ik.SetShoulderAnchor(Side.Left, null);
					}
					else if (this.ragdollPart.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightShoulder) == this.ragdollPart.bone.animation)
					{
						this.ragdollPart.ragdoll.creature.ragdoll.ik.SetShoulderAnchor(Side.Right, null);
					}
					else if (this.ragdollPart.type == RagdollPart.Type.LeftArm || this.ragdollPart.type == RagdollPart.Type.LeftHand)
					{
						this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Left, null);
					}
					else if (this.ragdollPart.type == RagdollPart.Type.RightArm || this.ragdollPart.type == RagdollPart.Type.RightHand)
					{
						this.ragdollPart.ragdoll.creature.ragdoll.ik.SetHandAnchor(Side.Right, null);
					}
					this.bodyAnchor.SetParent(base.transform);
					this.bodyAnchor.position = this.ragdollPart.transform.position;
					this.bodyAnchor.rotation = this.ragdollPart.transform.rotation;
					goto IL_3D8;
				}
				using (List<RagdollHand>.Enumerator enumerator = this.handlers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						RagdollHand otherRagdollHand = enumerator.Current;
						if (otherRagdollHand.otherHand == ragdollHand)
						{
							this.bodyAnchor.SetParent(otherRagdollHand.playerHand.grip.transform, true);
							break;
						}
					}
					goto IL_3D8;
				}
			}
			if (this.ragdollPart.type == RagdollPart.Type.LeftArm)
			{
				this.ragdollPart.ragdoll.ResetPinForce(true, false, RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand);
			}
			else if (this.ragdollPart.type == RagdollPart.Type.RightArm)
			{
				this.ragdollPart.ragdoll.ResetPinForce(true, false, RagdollPart.Type.RightArm | RagdollPart.Type.RightHand);
			}
			IL_3D8:
			this.ragdollPart.ragdoll.RefreshPartJointAndCollision();
			foreach (Collider handCollider in ragdollHand.colliderGroup.colliders)
			{
				this.ragdollPart.ragdoll.IgnoreCollision(handCollider, false, (RagdollPart.Type)0);
			}
			if (this.handlers.Count == 0)
			{
				this.ragdollPart.isGrabbed = false;
				this.isBackGrab = false;
			}
			this.ragdollPart.ragdoll.RemoveStabilizationJoint(base.gameObject);
			this.ragdollPart.physicBody.constraints = RigidbodyConstraints.None;
			if (this.ragdollPart.ragdoll.handlers.Count == 0)
			{
				this.ragdollPart.ragdoll.isGrabbed = false;
				if (this.ragdollPart.ragdoll.creature.state == Creature.State.Alive)
				{
					if (!this.ragdollPart.ragdoll.standingUp)
					{
						this.ragdollPart.ragdoll.SetState(Ragdoll.State.Standing);
					}
					this.ragdollPart.ragdoll.creature.locomotion.StopShrinkCollider();
				}
				this.ragdollPart.ragdoll.RefreshPartsLayer();
				this.ragdollPart.ragdoll.creature.lastInteractionTime = Time.time;
				this.ragdollPart.ragdoll.creature.lastInteractionCreature = ragdollHand.creature;
				this.ragdollPart.ragdoll.InvokeUngrabEvent(ragdollHand, this, true);
				return;
			}
			this.ragdollPart.ragdoll.InvokeUngrabEvent(ragdollHand, this, false);
		}

		// Token: 0x06001BEB RID: 7147 RVA: 0x000BA96C File Offset: 0x000B8B6C
		public void RefreshJointAndCollision()
		{
			if (this.handlers.Count > 0 || base.IsTkGrabbed)
			{
				if (this.handleRagdollData.overrideCharJointLimitsOnParts.Count > 0)
				{
					foreach (RagdollPart ragdollPart in this.ragdollPart.ragdoll.parts)
					{
						if (this.handleRagdollData.overrideCharJointLimitsOnParts.Contains(ragdollPart.type))
						{
							if (!ragdollPart.characterJoint)
							{
								return;
							}
							SoftJointLimit jointLimit = ragdollPart.characterJoint.swing1Limit;
							jointLimit.limit = this.handleRagdollData.swing1Limit;
							ragdollPart.characterJoint.swing1Limit = jointLimit;
							jointLimit = ragdollPart.characterJoint.lowTwistLimit;
							jointLimit.limit = this.handleRagdollData.lowTwistLimit;
							ragdollPart.characterJoint.lowTwistLimit = jointLimit;
							jointLimit = ragdollPart.characterJoint.highTwistLimit;
							jointLimit.limit = this.handleRagdollData.highTwistLimit;
							ragdollPart.characterJoint.highTwistLimit = jointLimit;
						}
					}
				}
				if (this.handleRagdollData.activateCollisionOnParts.Count > 0)
				{
					foreach (RagdollPart ragdollPart2 in this.ragdollPart.ragdoll.parts)
					{
						if (this.handleRagdollData.activateCollisionOnParts.Contains(ragdollPart2.type))
						{
							ragdollPart2.collisionHandler.active = true;
						}
					}
				}
			}
		}

		// Token: 0x04001AB5 RID: 6837
		public static float combatScaleChangeTime = 0.75f;

		// Token: 0x04001AB6 RID: 6838
		public bool canBeEscaped;

		// Token: 0x04001AB7 RID: 6839
		public bool wasTkGrabbed;

		// Token: 0x04001AB8 RID: 6840
		public int grappleEscapeParameterValue;

		// Token: 0x04001AB9 RID: 6841
		public float escapeDelay;

		// Token: 0x04001ABA RID: 6842
		[NonSerialized]
		public HandleRagdollData handleRagdollData;

		// Token: 0x04001ABB RID: 6843
		public static bool holdGripRagdoll = false;

		// Token: 0x04001ABC RID: 6844
		[NonSerialized]
		public float initialTouchRadius;

		// Token: 0x04001ABD RID: 6845
		[NonSerialized]
		public RagdollPart ragdollPart;

		// Token: 0x04001ABE RID: 6846
		[NonSerialized]
		public Transform bodyAnchor;

		// Token: 0x04001ABF RID: 6847
		public bool isBackGrab;

		// Token: 0x04001AC0 RID: 6848
		protected Brain.State lastBrainState;

		// Token: 0x04001AC1 RID: 6849
		protected Coroutine combatScaleCoroutine;

		// Token: 0x04001AC2 RID: 6850
		private bool stabilityReach;

		// Token: 0x04001AC3 RID: 6851
		private Vector3 grabDir = Vector3.zero;
	}
}
