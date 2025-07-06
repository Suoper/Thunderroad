using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RootMotion;
using Sirenix.OdinInspector;
using ThunderRoad.Modules;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Events;
using UnityEngine.VFX;

namespace ThunderRoad
{
	// Token: 0x0200025D RID: 605
	public class GolemController : ThunderEntity
	{
		// Token: 0x06001AC9 RID: 6857 RVA: 0x000B2AE4 File Offset: 0x000B0CE4
		public static GolemController.AttackSide GetAttackSide(GolemController.AttackMotion attack)
		{
			GolemController.AttackSide result;
			switch (attack)
			{
			case GolemController.AttackMotion.Rampage:
				result = GolemController.AttackSide.Both;
				break;
			case GolemController.AttackMotion.SwingRight:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.SwingLeft:
				result = GolemController.AttackSide.Right;
				break;
			case GolemController.AttackMotion.ComboSwing:
				result = GolemController.AttackSide.Both;
				break;
			case GolemController.AttackMotion.ComboSwingAndSlam:
				result = GolemController.AttackSide.Both;
				break;
			case GolemController.AttackMotion.SwingBehindRight:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.SwingBehindLeft:
				result = GolemController.AttackSide.Right;
				break;
			case GolemController.AttackMotion.SwingBehindRightTurnBack:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.SwingBehindLeftTurnBack:
				result = GolemController.AttackSide.Right;
				break;
			case GolemController.AttackMotion.SwingLeftStep:
				result = GolemController.AttackSide.Right;
				break;
			case GolemController.AttackMotion.SwingRightStep:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.Slam:
				result = GolemController.AttackSide.Both;
				break;
			case GolemController.AttackMotion.Stampede:
				result = GolemController.AttackSide.Both;
				break;
			case GolemController.AttackMotion.Breakdance:
				result = GolemController.AttackSide.Both;
				break;
			case GolemController.AttackMotion.SlamLeftTurn90:
				result = GolemController.AttackSide.Right;
				break;
			case GolemController.AttackMotion.SlamRightTurn90:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.SwingLeftTurn90:
				result = GolemController.AttackSide.Right;
				break;
			case GolemController.AttackMotion.SwingRightTurn90:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.Spray:
				result = GolemController.AttackSide.None;
				break;
			case GolemController.AttackMotion.SprayDance:
				result = GolemController.AttackSide.Left;
				break;
			case GolemController.AttackMotion.Throw:
				result = GolemController.AttackSide.None;
				break;
			case GolemController.AttackMotion.Beam:
				result = GolemController.AttackSide.None;
				break;
			case GolemController.AttackMotion.SelfImbue:
				result = GolemController.AttackSide.None;
				break;
			case GolemController.AttackMotion.RadialBurst:
				result = GolemController.AttackSide.None;
				break;
			case GolemController.AttackMotion.ShakeOff:
				result = GolemController.AttackSide.Both;
				break;
			default:
				result = GolemController.AttackSide.None;
				break;
			}
			return result;
		}

		// Token: 0x06001ACA RID: 6858 RVA: 0x000B2BC4 File Offset: 0x000B0DC4
		public static GolemController.AttackSide GetAttackSide(Side side)
		{
			GolemController.AttackSide result;
			if (side != Side.Right)
			{
				if (side == Side.Left)
				{
					result = GolemController.AttackSide.Left;
				}
				else
				{
					result = GolemController.AttackSide.None;
				}
			}
			else
			{
				result = GolemController.AttackSide.Right;
			}
			return result;
		}

		// Token: 0x170001A0 RID: 416
		// (get) Token: 0x06001ACB RID: 6859 RVA: 0x000B2BE3 File Offset: 0x000B0DE3
		// (set) Token: 0x06001ACC RID: 6860 RVA: 0x000B2BEB File Offset: 0x000B0DEB
		public bool animatorIsRoot { get; protected set; }

		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06001ACD RID: 6861 RVA: 0x000B2BF4 File Offset: 0x000B0DF4
		// (set) Token: 0x06001ACE RID: 6862 RVA: 0x000B2BFC File Offset: 0x000B0DFC
		public GolemController.State state { get; protected set; }

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06001ACF RID: 6863 RVA: 0x000B2C05 File Offset: 0x000B0E05
		// (set) Token: 0x06001AD0 RID: 6864 RVA: 0x000B2C0D File Offset: 0x000B0E0D
		public bool isDefeated { get; protected set; }

		// Token: 0x170001A3 RID: 419
		// (get) Token: 0x06001AD1 RID: 6865 RVA: 0x000B2C16 File Offset: 0x000B0E16
		// (set) Token: 0x06001AD2 RID: 6866 RVA: 0x000B2C1E File Offset: 0x000B0E1E
		public bool isKilled { get; protected set; }

		// Token: 0x170001A4 RID: 420
		// (get) Token: 0x06001AD3 RID: 6867 RVA: 0x000B2C27 File Offset: 0x000B0E27
		// (set) Token: 0x06001AD4 RID: 6868 RVA: 0x000B2C2F File Offset: 0x000B0E2F
		public bool isLooking { get; protected set; }

		// Token: 0x170001A5 RID: 421
		// (get) Token: 0x06001AD5 RID: 6869 RVA: 0x000B2C38 File Offset: 0x000B0E38
		// (set) Token: 0x06001AD6 RID: 6870 RVA: 0x000B2C40 File Offset: 0x000B0E40
		public Transform lookingTarget { get; protected set; }

		// Token: 0x170001A6 RID: 422
		// (get) Token: 0x06001AD7 RID: 6871 RVA: 0x000B2C49 File Offset: 0x000B0E49
		// (set) Token: 0x06001AD8 RID: 6872 RVA: 0x000B2C5B File Offset: 0x000B0E5B
		public bool isAwake
		{
			get
			{
				return this.animator.GetBool(GolemController.awakeHash);
			}
			set
			{
				this.animator.SetBool(GolemController.awakeHash, value);
			}
		}

		// Token: 0x170001A7 RID: 423
		// (get) Token: 0x06001AD9 RID: 6873 RVA: 0x000B2C6E File Offset: 0x000B0E6E
		// (set) Token: 0x06001ADA RID: 6874 RVA: 0x000B2C80 File Offset: 0x000B0E80
		public bool isBusy
		{
			get
			{
				return this.animator.GetBool(GolemController.isBusyHash);
			}
			set
			{
				this.animator.SetBool(GolemController.isBusyHash, value);
			}
		}

		// Token: 0x170001A8 RID: 424
		// (get) Token: 0x06001ADB RID: 6875 RVA: 0x000B2C93 File Offset: 0x000B0E93
		public bool inMovement
		{
			get
			{
				return this.animator.GetBool(GolemController.inMovementHash);
			}
		}

		// Token: 0x170001A9 RID: 425
		// (get) Token: 0x06001ADC RID: 6876 RVA: 0x000B2CA5 File Offset: 0x000B0EA5
		// (set) Token: 0x06001ADD RID: 6877 RVA: 0x000B2CB7 File Offset: 0x000B0EB7
		public bool inAttackMotion
		{
			get
			{
				return this.animator.GetBool(GolemController.inAttackMotionHash);
			}
			set
			{
				this.animator.SetBool(GolemController.inAttackMotionHash, value);
			}
		}

		// Token: 0x170001AA RID: 426
		// (get) Token: 0x06001ADE RID: 6878 RVA: 0x000B2CCA File Offset: 0x000B0ECA
		// (set) Token: 0x06001ADF RID: 6879 RVA: 0x000B2CDC File Offset: 0x000B0EDC
		public bool isDeployed
		{
			get
			{
				return this.animator.GetBool(GolemController.isDeployedHash);
			}
			protected set
			{
				this.animator.SetBool(GolemController.isDeployedHash, value);
			}
		}

		// Token: 0x170001AB RID: 427
		// (get) Token: 0x06001AE0 RID: 6880 RVA: 0x000B2CEF File Offset: 0x000B0EEF
		public bool deployInProgress
		{
			get
			{
				return this.animator.GetBool(GolemController.isDeployedHash) || this.animator.GetBool(GolemController.deployStartedHash);
			}
		}

		// Token: 0x170001AC RID: 428
		// (get) Token: 0x06001AE1 RID: 6881 RVA: 0x000B2D15 File Offset: 0x000B0F15
		public bool isStunned
		{
			get
			{
				return this.animator.GetBool(GolemController.isStunnedHash);
			}
		}

		// Token: 0x170001AD RID: 429
		// (get) Token: 0x06001AE2 RID: 6882 RVA: 0x000B2D27 File Offset: 0x000B0F27
		public bool stunInProgress
		{
			get
			{
				return this.animator.GetBool(GolemController.isStunnedHash) || this.animator.GetBool(GolemController.stunStartedHash);
			}
		}

		// Token: 0x170001AE RID: 430
		// (get) Token: 0x06001AE3 RID: 6883 RVA: 0x000B2D4D File Offset: 0x000B0F4D
		public bool isActiveState
		{
			get
			{
				return this.isBusy || this.inAttackMotion || this.deployInProgress || this.stunInProgress;
			}
		}

		// Token: 0x170001AF RID: 431
		// (get) Token: 0x06001AE4 RID: 6884 RVA: 0x000B2D70 File Offset: 0x000B0F70
		public bool isClimbed
		{
			get
			{
				if (this.grabbedParts.Count <= 0)
				{
					Creature currentCreature = Player.currentCreature;
					Rigidbody rigidbody;
					if (currentCreature == null)
					{
						rigidbody = null;
					}
					else
					{
						RagdollHand ragdollHand = currentCreature.handLeft;
						if (ragdollHand == null)
						{
							rigidbody = null;
						}
						else
						{
							RagdollHandClimb climb = ragdollHand.climb;
							if (climb == null)
							{
								rigidbody = null;
							}
							else
							{
								PhysicBody gripPhysicBody = climb.gripPhysicBody;
								rigidbody = ((gripPhysicBody != null) ? gripPhysicBody.rigidBody : null);
							}
						}
					}
					Rigidbody leftGrip = rigidbody;
					if (leftGrip == null || !this.bodyParts.Contains(leftGrip))
					{
						Creature currentCreature2 = Player.currentCreature;
						Rigidbody rigidbody2;
						if (currentCreature2 == null)
						{
							rigidbody2 = null;
						}
						else
						{
							RagdollHand ragdollHand2 = currentCreature2.handRight;
							if (ragdollHand2 == null)
							{
								rigidbody2 = null;
							}
							else
							{
								RagdollHandClimb climb2 = ragdollHand2.climb;
								if (climb2 == null)
								{
									rigidbody2 = null;
								}
								else
								{
									PhysicBody gripPhysicBody2 = climb2.gripPhysicBody;
									rigidbody2 = ((gripPhysicBody2 != null) ? gripPhysicBody2.rigidBody : null);
								}
							}
						}
						Rigidbody rightGrip = rigidbody2;
						return rightGrip != null && this.bodyParts.Contains(rightGrip);
					}
				}
				return true;
			}
		}

		// Token: 0x170001B0 RID: 432
		// (get) Token: 0x06001AE5 RID: 6885 RVA: 0x000B2E1E File Offset: 0x000B101E
		// (set) Token: 0x06001AE6 RID: 6886 RVA: 0x000B2E35 File Offset: 0x000B1035
		public Color HeadEmissionColor
		{
			get
			{
				return this.headRenderer.material.GetColor(GolemController.EmissionColor);
			}
			set
			{
				this.headRenderer.material.SetColor(GolemController.EmissionColor, value);
			}
		}

		// Token: 0x140000BA RID: 186
		// (add) Token: 0x06001AE7 RID: 6887 RVA: 0x000B2E50 File Offset: 0x000B1050
		// (remove) Token: 0x06001AE8 RID: 6888 RVA: 0x000B2E88 File Offset: 0x000B1088
		public event GolemController.GolemStateChange OnGolemStateChange;

		// Token: 0x140000BB RID: 187
		// (add) Token: 0x06001AE9 RID: 6889 RVA: 0x000B2EC0 File Offset: 0x000B10C0
		// (remove) Token: 0x06001AEA RID: 6890 RVA: 0x000B2EF8 File Offset: 0x000B10F8
		public event GolemController.GolemAttackEvent OnGolemAttackEvent;

		// Token: 0x140000BC RID: 188
		// (add) Token: 0x06001AEB RID: 6891 RVA: 0x000B2F30 File Offset: 0x000B1130
		// (remove) Token: 0x06001AEC RID: 6892 RVA: 0x000B2F68 File Offset: 0x000B1168
		public event GolemController.GolemRampageEvent OnGolemRampage;

		// Token: 0x140000BD RID: 189
		// (add) Token: 0x06001AED RID: 6893 RVA: 0x000B2FA0 File Offset: 0x000B11A0
		// (remove) Token: 0x06001AEE RID: 6894 RVA: 0x000B2FD8 File Offset: 0x000B11D8
		public event GolemController.GolemStaggerEvent OnGolemStagger;

		// Token: 0x140000BE RID: 190
		// (add) Token: 0x06001AEF RID: 6895 RVA: 0x000B3010 File Offset: 0x000B1210
		// (remove) Token: 0x06001AF0 RID: 6896 RVA: 0x000B3048 File Offset: 0x000B1248
		public event GolemController.GolemStunEvent OnGolemStun;

		// Token: 0x140000BF RID: 191
		// (add) Token: 0x06001AF1 RID: 6897 RVA: 0x000B3080 File Offset: 0x000B1280
		// (remove) Token: 0x06001AF2 RID: 6898 RVA: 0x000B30B8 File Offset: 0x000B12B8
		public event GolemController.GolemInterrupt OnGolemInterrupted;

		// Token: 0x140000C0 RID: 192
		// (add) Token: 0x06001AF3 RID: 6899 RVA: 0x000B30F0 File Offset: 0x000B12F0
		// (remove) Token: 0x06001AF4 RID: 6900 RVA: 0x000B3128 File Offset: 0x000B1328
		public event GolemController.GolemInterrupt OnGolemHeadshotInterrupt;

		// Token: 0x140000C1 RID: 193
		// (add) Token: 0x06001AF5 RID: 6901 RVA: 0x000B3160 File Offset: 0x000B1360
		// (remove) Token: 0x06001AF6 RID: 6902 RVA: 0x000B3198 File Offset: 0x000B1398
		public event GolemController.GolemDealDamage OnDamageDealt;

		// Token: 0x06001AF7 RID: 6903 RVA: 0x000B31CD File Offset: 0x000B13CD
		protected List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06001AF8 RID: 6904 RVA: 0x000B31DC File Offset: 0x000B13DC
		private void GetBodyParts()
		{
			this.bodyParts = new List<Rigidbody>();
			foreach (object obj in Enum.GetValues(typeof(HumanBodyBones)))
			{
				HumanBodyBones humanBodyBone = (HumanBodyBones)obj;
				foreach (Rigidbody bodyPart in this.animator.GetComponentsInChildren<Rigidbody>())
				{
					if (this.animator.GetBoneTransform(humanBodyBone) == bodyPart.transform)
					{
						bodyPart.isKinematic = true;
						this.bodyParts.Add(bodyPart);
					}
				}
			}
		}

		// Token: 0x06001AF9 RID: 6905 RVA: 0x000B3294 File Offset: 0x000B1494
		private void GetBodyPartColliders()
		{
			this.bodyPartColliders = new List<Collider>();
			foreach (Rigidbody bodyPart in this.bodyParts)
			{
				foreach (Collider collider in bodyPart.GetComponentsInChildren<Collider>())
				{
					if (collider.attachedRigidbody == bodyPart && collider.name.EndsWith("_Collider"))
					{
						this.bodyPartColliders.Add(collider);
					}
				}
			}
		}

		// Token: 0x06001AFA RID: 6906 RVA: 0x000B3334 File Offset: 0x000B1534
		public virtual void LookAt(Transform target)
		{
			if (target == null)
			{
				if (this.isLooking)
				{
					this.headIktarget.SetParent(base.transform);
					this.headIktarget.position = this.headAimConstraint.data.constrainedObject.position + this.headAimConstraint.data.constrainedObject.forward * 1f;
					this.lookingTarget = null;
					this.isLooking = false;
					return;
				}
			}
			else if (target != this.lookingTarget)
			{
				float targetDistance = Vector3.Distance(this.headAimConstraint.data.constrainedObject.position, target.position);
				this.headIktarget.position = this.headAimConstraint.data.constrainedObject.position + this.headAimConstraint.data.constrainedObject.forward * targetDistance;
				this.lookingTarget = target;
				if (!this.isLooking)
				{
					this.headIktarget.SetParent(null);
					this.isLooking = true;
					this.headAimConstraint.weight = 1f;
					if (this.headLookCoroutine != null)
					{
						base.StopCoroutine(this.headLookCoroutine);
					}
					this.headLookCoroutine = base.StartCoroutine(this.LookAtCoroutine());
				}
			}
		}

		// Token: 0x06001AFB RID: 6907 RVA: 0x000B3484 File Offset: 0x000B1684
		public virtual void UnlockFacePlate(bool open)
		{
			this.facePlateJoint.angularXMotion = (open ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked);
			this.facePlateJoint.angularYMotion = (open ? ConfigurableJointMotion.Limited : ConfigurableJointMotion.Locked);
			this.headCrystalBody.isKinematic = !open;
			this.facePlateBody.isKinematic = !open;
			this.headCrystalLinkVfx.enabled = open;
			if (open)
			{
				this.facePlateBreakable.enabled = true;
				this.facePlateBreakable.allowedDamageTypes = (SimpleBreakable.DamageType)(-1);
				this.facePlateBreakable.Restore();
				SoftJointLimit softJointLimit = this.facePlateJoint.lowAngularXLimit;
				softJointLimit.limit = -this.facePlateUnlockAngle;
				this.facePlateJoint.lowAngularXLimit = softJointLimit;
				this.headCrystalBody.mass = this.headCrystalGrabMass;
				this.headCrystalBody.drag = this.headCrystalGrabDrag;
				this.headCrystalBody.transform.SetParent(null);
				this.headCrystalParticle.Play();
				this.headCrystalLinkVfx.Play();
				this.headCrystalAudioSourceLoop.Play();
				this.SetHeadCrystalEffect(0f);
				return;
			}
			this.facePlateBreakable.enabled = false;
			this.facePlateBreakable.allowedDamageTypes = SimpleBreakable.DamageType.None;
			this.headCrystalBody.mass = this.orgHeadCrystalMass;
			this.headCrystalBody.drag = this.orgHeadCrystalDrag;
			this.headCrystalBody.transform.SetParent(this.animator.GetBoneTransform(HumanBodyBones.Head));
			this.headCrystalParticle.Stop();
			this.headCrystalLinkVfx.Stop();
			this.headCrystalAudioSourceLoop.Stop();
			this.headCrystalTearingAudioSource.Stop();
		}

		// Token: 0x06001AFC RID: 6908 RVA: 0x000B3611 File Offset: 0x000B1811
		public virtual void Kill()
		{
			base.StartCoroutine(this.KillCoroutine());
		}

		// Token: 0x06001AFD RID: 6909 RVA: 0x000B3620 File Offset: 0x000B1820
		public virtual IEnumerator KillCoroutine()
		{
			this.characterController.enabled = false;
			this.ChangeState(GolemController.State.Dead);
			this.RefreshGrabbed(true);
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.skinnedMeshRenderers)
			{
				if (skinnedMeshRenderer)
				{
					skinnedMeshRenderer.updateWhenOffscreen = true;
				}
			}
			this.ProgressiveAction(0.3f, delegate(float t)
			{
				foreach (Transform transform in this.colliderResizeOnDeath)
				{
					transform.localScale = Vector3.one * t;
				}
			});
			foreach (Collider collider in this.bodyPartColliders)
			{
				MeshCollider mesh = collider as MeshCollider;
				if (mesh != null && !mesh.convex)
				{
					mesh.cookingOptions = (MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.UseFastMidphase);
					mesh.convex = true;
					yield return Yielders.EndOfFrame;
				}
			}
			List<Collider>.Enumerator enumerator2 = default(List<Collider>.Enumerator);
			foreach (Rigidbody rigidbody in this.bodyParts)
			{
				rigidbody.isKinematic = false;
			}
			foreach (Rigidbody bodyPart in this.bodyParts)
			{
				if (this.killExplosionForce > 0f)
				{
					bodyPart.AddExplosionForce(this.killExplosionForce, this.killExplosionSourceTransform.position, this.killExplosionRadius, this.killExplosionUpward, this.killExplosionForceMode);
				}
			}
			this.headCrystalBody.mass = this.orgHeadCrystalMass;
			this.headCrystalBody.drag = this.orgHeadCrystalDrag;
			this.headCrystalBody.ResetInertiaTensor();
			UnityEngine.Object.Destroy(this.headCrystalJoint);
			this.DelayedAction(2f, delegate
			{
				this.targetHeadEmissionColor = Color.black;
				this.headCrystalParticle.Stop();
			});
			this.headCrystalLinkVfx.Stop();
			this.headCrystalLinkVfx.gameObject.SetActive(false);
			this.ProgressiveAction(this.headCrystalShutdownDuration, delegate(float t)
			{
				this.headCrystalEffectController.SetIntensity(Mathf.Lerp(1f, 0f, t));
			});
			this.animator.GetBoneTransform(HumanBodyBones.Head).GetComponent<Rigidbody>().AddExplosionForce(this.killExplosionForce, this.killExplosionSourceTransform.position, this.killExplosionRadius, this.killExplosionUpward, this.killExplosionForceMode);
			this.animator.GetBoneTransform(HumanBodyBones.Spine).GetComponent<Rigidbody>().AddExplosionForce(this.killExplosionForce, this.killExplosionSourceTransform.position, this.killExplosionRadius, this.killExplosionUpward, this.killExplosionForceMode);
			this.animator.GetBoneTransform(HumanBodyBones.UpperChest).GetComponent<Rigidbody>().AddExplosionForce(this.killExplosionForce, this.killExplosionSourceTransform.position, this.killExplosionRadius, this.killExplosionUpward, this.killExplosionForceMode);
			this.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).GetComponent<Rigidbody>().AddExplosionForce(this.killExplosionForce, this.killExplosionSourceTransform.position, this.killExplosionRadius, this.killExplosionUpward, this.killExplosionForceMode);
			this.animator.GetBoneTransform(HumanBodyBones.RightUpperArm).GetComponent<Rigidbody>().AddExplosionForce(this.killExplosionForce, this.killExplosionSourceTransform.position, this.killExplosionRadius, this.killExplosionUpward, this.killExplosionForceMode);
			this.DelayedAction(0.3f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.Head).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(0.4f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(0.5f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.RightLowerArm).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(0.6f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(0.7f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.RightUpperArm).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(0.8f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(0.9f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(1f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(1.1f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(1.2f, delegate
			{
				UnityEngine.Object.Destroy(this.facePlateJoint);
			});
			this.DelayedAction(1.3f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.UpperChest).GetComponent<CharacterJoint>());
			});
			this.DelayedAction(1.5f, delegate
			{
				UnityEngine.Object.Destroy(this.animator.GetBoneTransform(HumanBodyBones.Spine).GetComponent<CharacterJoint>());
			});
			if (this.headCrystalBody.transform.parent)
			{
				this.headCrystalBody.gameObject.SetActive(false);
			}
			this.animator.enabled = false;
			this.SetMove(false);
			this.headCrystalAudioSourceLoop.Stop();
			this.headCrystalTearingAudioSource.Stop();
			this.killparticle.transform.SetParent(base.transform);
			this.killparticle.Play();
			this.killAudioSource.Play();
			this.isKilled = true;
			yield break;
			yield break;
		}

		// Token: 0x06001AFE RID: 6910 RVA: 0x000B3630 File Offset: 0x000B1830
		public virtual void Resurrect()
		{
			this.characterController.enabled = true;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in this.skinnedMeshRenderers)
			{
				skinnedMeshRenderer.updateWhenOffscreen = false;
			}
			foreach (Rigidbody rigidbody in this.bodyParts)
			{
				rigidbody.isKinematic = true;
			}
			if (this.headCrystalBody.transform.parent)
			{
				this.headCrystalBody.gameObject.SetActive(true);
			}
			this.animator.enabled = true;
			this.isKilled = false;
		}

		// Token: 0x06001AFF RID: 6911 RVA: 0x000B3708 File Offset: 0x000B1908
		public virtual void SetAwake(bool awake)
		{
			this.isAwake = awake;
			this.animator.SetInteger(GolemController.wakeMotionHash, this.wakeMotion);
			if (this.spawner != null)
			{
				this.spawner.StartWakeSequence(this.wakeMotion);
			}
			if (awake)
			{
				base.StartCoroutine(this.WakeCoroutine());
			}
		}

		// Token: 0x06001B00 RID: 6912 RVA: 0x000B3761 File Offset: 0x000B1961
		public void Stun(float duration)
		{
			this.Stun(duration, null, null, null);
		}

		// Token: 0x06001B01 RID: 6913 RVA: 0x000B376D File Offset: 0x000B196D
		public virtual void StopStun()
		{
			if (this.isDefeated && this.stunInProgress)
			{
				return;
			}
			this.animator.SetBool(GolemController.stunHash, false);
		}

		// Token: 0x06001B02 RID: 6914 RVA: 0x000B3794 File Offset: 0x000B1994
		public virtual void StaggerImpact(Vector3 point)
		{
			Vector3 direction = (base.transform.position.ToXZ() - point.ToXZ()).normalized;
			direction = base.transform.InverseTransformDirection(direction);
			this.Stagger(new Vector2(direction.x, direction.z));
		}

		// Token: 0x06001B03 RID: 6915 RVA: 0x000B37E9 File Offset: 0x000B19E9
		public virtual void Stagger(float lateral, float axial)
		{
			this.Stagger(new Vector2(lateral, axial));
		}

		// Token: 0x06001B04 RID: 6916 RVA: 0x000B37F8 File Offset: 0x000B19F8
		public virtual void Stagger(Vector2 direction)
		{
			if (this.stunInProgress)
			{
				return;
			}
			if (this.isDefeated)
			{
				return;
			}
			if (!this.isAwake)
			{
				return;
			}
			this.isBusy = true;
			this.EndAbility();
			this.StopDeploy();
			this.animator.SetTrigger(GolemController.staggerHash);
			direction = direction.normalized;
			this.animator.SetFloat(GolemController.staggerAxialHash, direction.y);
			this.animator.SetFloat(GolemController.staggerLateralHash, direction.x);
			GolemController.GolemStaggerEvent onGolemStagger = this.OnGolemStagger;
			if (onGolemStagger == null)
			{
				return;
			}
			onGolemStagger(direction);
		}

		// Token: 0x06001B05 RID: 6917 RVA: 0x000B3889 File Offset: 0x000B1A89
		public virtual void ResistPush(bool active)
		{
			this.isBusy = true;
			this.animator.SetBool(GolemController.resistPushHash, active);
		}

		// Token: 0x06001B06 RID: 6918 RVA: 0x000B38A4 File Offset: 0x000B1AA4
		public virtual void PerformAttackMotion(GolemController.AttackMotion meleeAttack, Action onMeleeEnd = null)
		{
			if (meleeAttack != GolemController.AttackMotion.Rampage && this.currentAbility != null)
			{
				this.currentAbility.stepMotion = meleeAttack;
			}
			this.animatorEvent.RightPlant(null);
			this.animatorEvent.LeftPlant(null);
			this.meleeAttackStartTime = Time.time;
			this.inAttackMotion = (this.isBusy = true);
			this.lastAttackMotion = meleeAttack;
			this.animator.SetInteger(GolemController.attackMotionHash, (int)meleeAttack);
			this.animator.SetTrigger(GolemController.attackHash);
			if (this.attackEndCoroutine != null)
			{
				this.replaceAttackCoroutineAction = delegate()
				{
					this.attackEndCoroutine = this.StartCoroutine(this.AttackMotionCoroutine(onMeleeEnd));
				};
			}
			else
			{
				this.attackEndCoroutine = base.StartCoroutine(this.AttackMotionCoroutine(onMeleeEnd));
			}
			if (this.currentAbility == null)
			{
				GolemController.GolemAttackEvent onGolemAttackEvent = this.OnGolemAttackEvent;
				if (onGolemAttackEvent == null)
				{
					return;
				}
				onGolemAttackEvent(meleeAttack, null);
			}
		}

		// Token: 0x06001B07 RID: 6919 RVA: 0x000B3991 File Offset: 0x000B1B91
		public virtual void UseAbility(int index)
		{
			this.StartAbility(this.abilities[index], null);
		}

		// Token: 0x06001B08 RID: 6920 RVA: 0x000B39A8 File Offset: 0x000B1BA8
		public void StartAbility(GolemAbility ability, Action endCallback = null)
		{
			if (this.stunInProgress)
			{
				return;
			}
			this.EndAbility();
			if (ability == null)
			{
				return;
			}
			this.isBusy = true;
			this.lastAbilityTime = Time.time;
			this.currentAbility = ability;
			this.currentAbility.Begin(this, endCallback);
			this.OnGolemInterrupted += this.currentAbility.Interrupt;
			GolemController.GolemAttackEvent onGolemAttackEvent = this.OnGolemAttackEvent;
			if (onGolemAttackEvent == null)
			{
				return;
			}
			onGolemAttackEvent((ability is GolemBeam) ? GolemController.AttackMotion.Beam : this.lastAttackMotion, ability);
		}

		// Token: 0x06001B09 RID: 6921 RVA: 0x000B3A30 File Offset: 0x000B1C30
		public void EndAbility()
		{
			if (this.currentAbility == null)
			{
				return;
			}
			this.OnGolemInterrupted -= this.currentAbility.Interrupt;
			this.currentAbility.OnEnd();
			this.currentAbility = null;
			this.lookMode = LookMode.Follow;
		}

		// Token: 0x06001B0A RID: 6922 RVA: 0x000B3A7D File Offset: 0x000B1C7D
		public void SetMoveSpeedMultiplier(float value)
		{
			this.animator.SetFloat(GolemController.moveSpeedMultiplierHash, value);
		}

		// Token: 0x06001B0B RID: 6923 RVA: 0x000B3A90 File Offset: 0x000B1C90
		protected virtual void OnValidate()
		{
			if (!this.animator)
			{
				this.animator = base.GetComponentInChildren<Animator>();
			}
			if (!this.animatorEvent)
			{
				this.animatorEvent = base.GetComponentInChildren<GolemAnimatorEvent>();
			}
			if (!this.headAimConstraint)
			{
				this.headAimConstraint = base.GetComponentInChildren<MultiAimConstraint>();
			}
			if (!this.characterController)
			{
				this.characterController = base.GetComponentInParent<CharacterController>();
			}
			if (!this.skinnedMeshRenderers.IsNullOrEmpty())
			{
				this.skinnedMeshRenderers.RemoveAll((SkinnedMeshRenderer x) => x == null);
			}
			if (this.skinnedMeshRenderers.IsNullOrEmpty())
			{
				this.skinnedMeshRenderers = new List<SkinnedMeshRenderer>(base.GetComponentsInChildren<SkinnedMeshRenderer>());
			}
			if (this.bodyParts.Count == 0)
			{
				this.GetBodyParts();
			}
			if (Application.isPlaying)
			{
				this.SetMoveSpeedMultiplier(this.moveSpeedMultiplier);
			}
		}

		// Token: 0x170001B1 RID: 433
		// (get) Token: 0x06001B0C RID: 6924 RVA: 0x000B3B7D File Offset: 0x000B1D7D
		// (set) Token: 0x06001B0D RID: 6925 RVA: 0x000B3B85 File Offset: 0x000B1D85
		public GolemAbility lastAbility { get; protected set; }

		// Token: 0x170001B2 RID: 434
		// (get) Token: 0x06001B0E RID: 6926 RVA: 0x000B3B8E File Offset: 0x000B1D8E
		// (set) Token: 0x06001B0F RID: 6927 RVA: 0x000B3B96 File Offset: 0x000B1D96
		public GolemController.AttackMotion lastAttackMotion { get; protected set; }

		// Token: 0x170001B3 RID: 435
		// (get) Token: 0x06001B10 RID: 6928 RVA: 0x000B3B9F File Offset: 0x000B1D9F
		// (set) Token: 0x06001B11 RID: 6929 RVA: 0x000B3BA7 File Offset: 0x000B1DA7
		public float lastAbilityTime { get; protected set; }

		// Token: 0x170001B4 RID: 436
		// (get) Token: 0x06001B12 RID: 6930 RVA: 0x000B3BB0 File Offset: 0x000B1DB0
		// (set) Token: 0x06001B13 RID: 6931 RVA: 0x000B3BB8 File Offset: 0x000B1DB8
		public Dictionary<RagdollHand, Rigidbody> grabbedParts { get; protected set; } = new Dictionary<RagdollHand, Rigidbody>();

		// Token: 0x170001B5 RID: 437
		// (get) Token: 0x06001B14 RID: 6932 RVA: 0x000B3BC1 File Offset: 0x000B1DC1
		// (set) Token: 0x06001B15 RID: 6933 RVA: 0x000B3BC9 File Offset: 0x000B1DC9
		public List<Creature> climbers { get; protected set; } = new List<Creature>();

		// Token: 0x170001B6 RID: 438
		// (get) Token: 0x06001B16 RID: 6934 RVA: 0x000B3BD2 File Offset: 0x000B1DD2
		// (set) Token: 0x06001B17 RID: 6935 RVA: 0x000B3BDA File Offset: 0x000B1DDA
		public RagdollHand mainGrabbedHand { get; protected set; }

		// Token: 0x06001B18 RID: 6936 RVA: 0x000B3BE3 File Offset: 0x000B1DE3
		public Transform GetHand(Side side)
		{
			if (side != Side.Left)
			{
				return this.handRight;
			}
			return this.handLeft;
		}

		// Token: 0x06001B19 RID: 6937 RVA: 0x000B3BF8 File Offset: 0x000B1DF8
		protected virtual void Awake()
		{
			base.Load(null);
			this.statusImmune = true;
			this.facePlateBreakable.enabled = false;
			this.facePlateBreakable.allowedDamageTypes = SimpleBreakable.DamageType.None;
			this.facePlateBreakable.onBreak.AddListener(new UnityAction(this.OpenFacePlate));
			this.orgHeadCrystalMass = this.headCrystalBody.mass;
			this.orgHeadCrystalDrag = this.headCrystalBody.drag;
			this.orgCharacterRadius = this.characterController.radius;
			this.headCrystalJoint.autoConfigureConnectedAnchor = false;
			this.HeadCrystalHandlers(true);
			this.animatorIsRoot = (this.animator.gameObject == base.gameObject);
			this.InitAnimationParametersHashes();
			this.UnlockFacePlate(false);
			this.SetMoveSpeedMultiplier(this.moveSpeedMultiplier);
			CrystalHuntProgressionModule progressionModule;
			if (GameModeManager.instance.currentGameMode.TryGetModule<CrystalHuntProgressionModule>(out progressionModule))
			{
				this.wakeMotion = (progressionModule.progressionLevel - 1) % 3;
				if (this.wakeMotion < 0)
				{
					this.wakeMotion = 0;
				}
			}
			else
			{
				this.wakeMotion = UnityEngine.Random.Range(0, 3);
			}
			this.SetAwake(this.awakeOnStart);
			this.speed.OnChangeEvent += delegate(float _, float newValue)
			{
				this.animator.speed = newValue;
			};
			this.OnGolemStagger += delegate(Vector2 direction)
			{
				GolemController.GolemInterrupt onGolemInterrupted = this.OnGolemInterrupted;
				if (onGolemInterrupted == null)
				{
					return;
				}
				onGolemInterrupted();
			};
			this.OnGolemStun += delegate(float duration)
			{
				GolemController.GolemInterrupt onGolemInterrupted = this.OnGolemInterrupted;
				if (onGolemInterrupted == null)
				{
					return;
				}
				onGolemInterrupted();
			};
			this.OnGolemRampage += delegate()
			{
				GolemController.GolemInterrupt onGolemInterrupted = this.OnGolemInterrupted;
				if (onGolemInterrupted == null)
				{
					return;
				}
				onGolemInterrupted();
			};
			foreach (CollisionListener collisionListener in this.headListeners)
			{
				collisionListener.OnCollisionEnterEvent += this.OnHeadCollision;
			}
			this.swingEffectData = Catalog.GetData<EffectData>(this.swingEffectId, true);
			Handle handle = this.headCrystalHandle;
			handle.onDataLoaded = (Action<HandleData>)Delegate.Combine(handle.onDataLoaded, new Action<HandleData>(this.OnHandleDataLoaded));
		}

		// Token: 0x06001B1A RID: 6938 RVA: 0x000B3DE8 File Offset: 0x000B1FE8
		private void OnHandleDataLoaded(HandleData handleData)
		{
			this.headCrystalHandle.SetTelekinesis(false);
			this.headCrystalHandle.SetTouch(false);
		}

		// Token: 0x06001B1B RID: 6939 RVA: 0x000B3E04 File Offset: 0x000B2004
		private void OnHeadCollision(Collision other)
		{
			if (!(this.currentAbility == null) && this.currentAbility.HeadshotInterruptable && !this.isDefeated && !this.isKilled)
			{
				Rigidbody rigidbody = other.rigidbody;
				if (!(((rigidbody != null) ? rigidbody.GetComponentInParent<CollisionHandler>() : null) == null) && other.relativeVelocity.magnitude >= 3f)
				{
					this.StaggerImpact(other.GetContact(0).point);
					GolemController.GolemInterrupt onGolemHeadshotInterrupt = this.OnGolemHeadshotInterrupt;
					if (onGolemHeadshotInterrupt == null)
					{
						return;
					}
					onGolemHeadshotInterrupt();
					return;
				}
			}
		}

		/// <summary>
		/// Register listeners to all the ladder handle events to enable player climbing
		/// </summary>
		// Token: 0x06001B1C RID: 6940 RVA: 0x000B3E94 File Offset: 0x000B2094
		public void RegisterGrabEvents()
		{
			RagdollHandClimb.OnClimberGrip += this.ClimberGrip;
			RagdollHandClimb.OnClimberRelease += this.ClimberRelease;
			Handle[] handles = base.GetComponentsInChildren<Handle>();
			for (int i = 0; i < handles.Length; i++)
			{
				if (!(handles[i].item != null))
				{
					handles[i].Grabbed += this.OnHandleGrab;
					handles[i].UnGrabbed += this.OnHandleUnGrab;
				}
			}
		}

		// Token: 0x06001B1D RID: 6941 RVA: 0x000B3F10 File Offset: 0x000B2110
		private void ClimberGrip(RagdollHandClimb climber)
		{
			if (!this.bodyParts.Contains(climber.gripPhysicBody.rigidBody))
			{
				return;
			}
			this.OnClimbHandAdd(climber.gripPhysicBody.rigidBody, climber.ragdollHand);
		}

		// Token: 0x06001B1E RID: 6942 RVA: 0x000B3F42 File Offset: 0x000B2142
		private void ClimberRelease(RagdollHandClimb climber)
		{
			if (!this.grabbedParts.ContainsKey(climber.ragdollHand))
			{
				return;
			}
			this.OnClimbHandRemove(climber.ragdollHand);
		}

		// Token: 0x06001B1F RID: 6943 RVA: 0x000B3F64 File Offset: 0x000B2164
		private void OnHandleGrab(RagdollHand hand, Handle handle, EventTime time)
		{
			if (time == EventTime.OnStart)
			{
				return;
			}
			this.OnClimbHandAdd(handle.physicBody.rigidBody, hand);
		}

		// Token: 0x06001B20 RID: 6944 RVA: 0x000B3F7C File Offset: 0x000B217C
		private void OnHandleUnGrab(RagdollHand hand, Handle handle, EventTime time)
		{
			if (time == EventTime.OnStart)
			{
				return;
			}
			this.OnClimbHandRemove(hand);
		}

		// Token: 0x06001B21 RID: 6945 RVA: 0x000B3F8C File Offset: 0x000B218C
		public void OnClimbHandAdd(Rigidbody part, RagdollHand hand)
		{
			if (this.mainGrabbedHand == null && hand.playerHand)
			{
				this.mainGrabbedHand = hand;
			}
			this.grabbedParts[hand] = part;
			if (!this.climbers.Contains(hand.creature))
			{
				this.climbers.Add(hand.creature);
			}
			if (hand.playerHand)
			{
				hand.playerHand.link.SetAllJointModifiers(this, 5f);
			}
			this.RefreshGrabbed(false);
		}

		// Token: 0x06001B22 RID: 6946 RVA: 0x000B4018 File Offset: 0x000B2218
		public void OnClimbHandRemove(RagdollHand hand)
		{
			if (hand.playerHand && this.mainGrabbedHand == hand)
			{
				this.mainGrabbedHand = (this.grabbedParts.ContainsKey(this.mainGrabbedHand.otherHand) ? this.mainGrabbedHand.otherHand : null);
			}
			if (hand.playerHand)
			{
				hand.playerHand.link.RemoveJointModifier(this);
			}
			this.grabbedParts.Remove(hand);
			if (!this.grabbedParts.ContainsKey(hand.otherHand))
			{
				this.climbers.Remove(hand.creature);
			}
			this.RefreshGrabbed(false);
		}

		// Token: 0x06001B23 RID: 6947 RVA: 0x000B40C4 File Offset: 0x000B22C4
		public void RefreshGrabbed(bool killStart = false)
		{
			bool disableReferenceFrame = this.isDefeated || this.isKilled || killStart;
			bool grabbed = this.grabbedParts.Count > 0 && !disableReferenceFrame;
			if (grabbed)
			{
				Player.local.SetFrameOfReference(this.grabbedParts[this.mainGrabbedHand].transform);
				Player.local.crouching = true;
				if (!Player.local.locomotion.colliderIsShrinking)
				{
					Player.local.locomotion.StartShrinkCollider();
				}
			}
			else
			{
				Player.local.SetFrameOfReference((this.mainGrabbedHand && !disableReferenceFrame) ? this.grabbedParts[this.mainGrabbedHand].transform : null);
				Player.local.crouching = false;
				if (Player.local.locomotion.colliderIsShrinking)
				{
					Player.local.locomotion.StopShrinkCollider();
				}
			}
			Player.local.autoAlign = !grabbed;
			Player.currentCreature.climber.enabled = !grabbed;
			if (grabbed)
			{
				Player.local.locomotion.groundMask = Player.local.locomotion.groundMask.RemoveFromMask(new string[]
				{
					LayerName.PlayerLocomotionObject.ToString()
				});
				return;
			}
			Player.local.locomotion.groundMask = Player.local.locomotion.groundMask.AddToMask(new string[]
			{
				LayerName.PlayerLocomotionObject.ToString()
			});
		}

		// Token: 0x06001B24 RID: 6948 RVA: 0x000B4248 File Offset: 0x000B2448
		public void ForceUngripClimbers(bool climbedOnly, bool bothHands = false)
		{
			foreach (Creature creature in this.ForceUngripClimbersEnumerable(climbedOnly, bothHands))
			{
			}
			this.RefreshGrabbed(false);
		}

		// Token: 0x06001B25 RID: 6949 RVA: 0x000B4298 File Offset: 0x000B2498
		public IEnumerable<Creature> ForceUngripClimbersEnumerable(bool climbedOnly, bool bothHands = false)
		{
			GolemController.<>c__DisplayClass251_0 CS$<>8__locals1;
			CS$<>8__locals1.climbedOnly = climbedOnly;
			CS$<>8__locals1.<>4__this = this;
			int num;
			for (int i = this.climbers.Count - 1; i >= 0; i = num - 1)
			{
				Creature creature = this.climbers[i];
				if (bothHands)
				{
					this.<ForceUngripClimbersEnumerable>g__UngrabHand|251_0(creature.handRight, ref CS$<>8__locals1);
					this.<ForceUngripClimbersEnumerable>g__UngrabHand|251_0(creature.handLeft, ref CS$<>8__locals1);
				}
				else
				{
					RagdollHand hand = creature.GetHand((Side)UnityEngine.Random.Range(0, 2));
					if (!this.<ForceUngripClimbersEnumerable>g__UngrabHand|251_0(hand, ref CS$<>8__locals1))
					{
						this.<ForceUngripClimbersEnumerable>g__UngrabHand|251_0(hand.otherHand, ref CS$<>8__locals1);
					}
				}
				if (!this.climbers.Contains(creature))
				{
					yield return creature;
				}
				num = i;
			}
			yield break;
		}

		// Token: 0x06001B26 RID: 6950 RVA: 0x000B42B6 File Offset: 0x000B24B6
		public void InvokeDamageDealt(Creature target, float damage)
		{
			GolemController.GolemDealDamage onDamageDealt = this.OnDamageDealt;
			if (onDamageDealt == null)
			{
				return;
			}
			onDamageDealt(target, damage);
		}

		// Token: 0x06001B27 RID: 6951 RVA: 0x000B42CC File Offset: 0x000B24CC
		private void InitAnimationParametersHashes()
		{
			GolemController.awakeHash = Animator.StringToHash("Awake");
			GolemController.wakeMotionHash = Animator.StringToHash("WakeMotion");
			GolemController.locomotionMultHash = Animator.StringToHash("LocomotionMult");
			GolemController.isBusyHash = Animator.StringToHash("IsBusy");
			GolemController.moveHash = Animator.StringToHash("Move");
			GolemController.inMovementHash = Animator.StringToHash("InMovement");
			GolemController.staggerHash = Animator.StringToHash("Stagger");
			GolemController.staggerAxialHash = Animator.StringToHash("StaggerAxial");
			GolemController.staggerLateralHash = Animator.StringToHash("StaggerLateral");
			GolemController.resistPushHash = Animator.StringToHash("Resist");
			GolemController.attackHash = Animator.StringToHash("Attack");
			GolemController.attackMotionHash = Animator.StringToHash("AttackMotion");
			GolemController.inAttackMotionHash = Animator.StringToHash("InAttackMotion");
			GolemController.deployHash = Animator.StringToHash("Deploy");
			GolemController.deployStartedHash = Animator.StringToHash("DeployStarted");
			GolemController.isDeployedHash = Animator.StringToHash("IsDeployed");
			GolemController.stunHash = Animator.StringToHash("Stun");
			GolemController.stunDirectionHash = Animator.StringToHash("StunDirection");
			GolemController.stunStartedHash = Animator.StringToHash("StunStarted");
			GolemController.isStunnedHash = Animator.StringToHash("IsStunned");
			GolemController.moveSpeedMultiplierHash = Animator.StringToHash("MoveSpeedMultiplier");
		}

		// Token: 0x06001B28 RID: 6952 RVA: 0x000B4414 File Offset: 0x000B2614
		protected override void Start()
		{
			base.Start();
			this.RegisterGrabEvents();
			foreach (Collider collider in this.ignoreCollisionColliders)
			{
				foreach (Collider collider2 in this.ignoreCollisionColliders)
				{
					if (!(collider == collider2))
					{
						Physics.IgnoreCollision(collider, collider2, true);
					}
				}
			}
		}

		// Token: 0x170001B7 RID: 439
		// (get) Token: 0x06001B29 RID: 6953 RVA: 0x000B44B8 File Offset: 0x000B26B8
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001B2A RID: 6954 RVA: 0x000B44BC File Offset: 0x000B26BC
		public virtual void OnAnimatorMove()
		{
			if (this.animatorIsRoot)
			{
				this.animator.ApplyBuiltinRootMotion();
				return;
			}
			Vector3 velocity = this.animator.deltaPosition;
			if (this.characterController.enabled)
			{
				if (this.characterController.isGrounded)
				{
					this.gravityVelocity = Vector3.zero;
					velocity += Physics.gravity * Time.deltaTime;
				}
				else
				{
					this.gravityVelocity += Physics.gravity * Time.deltaTime * Time.deltaTime;
					velocity += this.gravityVelocity;
				}
				this.characterController.Move(velocity);
			}
			else
			{
				base.transform.position += velocity;
			}
			base.transform.rotation *= this.animator.deltaRotation;
		}

		// Token: 0x06001B2B RID: 6955 RVA: 0x000B45A4 File Offset: 0x000B27A4
		protected IEnumerator LookAtCoroutine()
		{
			while (this.isLooking)
			{
				if (!this.lookingTarget)
				{
					this.LookAt(null);
					IL_10F:
					while (this.headAimConstraint.weight > 0f)
					{
						this.headAimConstraint.weight = Mathf.Clamp01(this.headAimConstraint.weight - Time.deltaTime);
						yield return null;
					}
					this.headIktarget.position = Vector3.zero;
					this.headLookCoroutine = null;
					yield break;
				}
				GolemAbility golemAbility = this.currentAbility;
				if (golemAbility != null && golemAbility.OverrideLook)
				{
					this.currentAbility.LookAt();
				}
				else if (this.lookMode == LookMode.Follow)
				{
					this.headIktarget.transform.position = Vector3.Lerp(this.headIktarget.transform.position, this.lookingTarget.position, this.headLookSpeed * this.headLookSpeedMultiplier * Time.deltaTime);
				}
				yield return null;
			}
			goto IL_10F;
		}

		// Token: 0x06001B2C RID: 6956 RVA: 0x000B45B4 File Offset: 0x000B27B4
		public virtual void OpenFacePlate()
		{
			this.headCrystalHandle.SetTelekinesis(true);
			this.headCrystalHandle.SetTouch(true);
			SoftJointLimit softJointLimit = this.facePlateJoint.lowAngularXLimit;
			softJointLimit.limit = -this.facePlateOpenAngle;
			this.facePlateJoint.lowAngularXLimit = softJointLimit;
		}

		// Token: 0x06001B2D RID: 6957 RVA: 0x000B4600 File Offset: 0x000B2800
		protected virtual void OnHeadCrystalGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.LookAt(ragdollHand.playerHand.grip.transform);
				this.headCrystalJoint.angularXDrive = default(JointDrive);
				this.headCrystalJoint.angularYZDrive = default(JointDrive);
				this.headCrystalTearingAudioSource.Play();
				GolemSpawner golemSpawner = this.spawner;
				if (golemSpawner != null)
				{
					golemSpawner.onCrystalGrabbed.Invoke();
				}
				base.InvokeRepeating("UpdateHeadCrystalTearing", 0.1f, 0.1f);
			}
		}

		// Token: 0x06001B2E RID: 6958 RVA: 0x000B4688 File Offset: 0x000B2888
		protected virtual void OnHeadCrystalTkGrabbed(Handle handle, SpellTelekinesis telekinesis)
		{
			this.LookAt(telekinesis.spellCaster.ragdollHand.grip.transform);
			this.headCrystalJoint.angularXDrive = default(JointDrive);
			this.headCrystalJoint.angularYZDrive = default(JointDrive);
			this.headCrystalTearingAudioSource.Play();
			GolemSpawner golemSpawner = this.spawner;
			if (golemSpawner != null)
			{
				golemSpawner.onCrystalGrabbed.Invoke();
			}
			base.InvokeRepeating("UpdateHeadCrystalTearing", 0.1f, 0.1f);
		}

		// Token: 0x06001B2F RID: 6959 RVA: 0x000B4710 File Offset: 0x000B2910
		protected void UpdateHeadCrystalTearing()
		{
			try
			{
				float radiusDistanceRatio;
				if (this.headCrystalLinkVfx.transform.position.PointInRadius(this.headCrystalHandle.transform.position, this.headCrystalTearingDistance, out radiusDistanceRatio))
				{
					this.SetHeadCrystalEffect(1f - radiusDistanceRatio);
				}
				else
				{
					this.HeadCrystalHandlers(false);
					this.Kill();
					base.CancelInvoke("UpdateHeadCrystalTearing");
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Exception when updating head crystal tearing: {0}", e));
			}
		}

		// Token: 0x06001B30 RID: 6960 RVA: 0x000B4798 File Offset: 0x000B2998
		private void HeadCrystalHandlers(bool add)
		{
			this.headCrystalHandle.Grabbed -= this.OnHeadCrystalGrabbed;
			this.headCrystalHandle.UnGrabbed -= this.OnHeadCrystalUnGrabbed;
			this.headCrystalHandle.TkGrabbed -= this.OnHeadCrystalTkGrabbed;
			this.headCrystalHandle.TkUnGrabbed -= this.OnHeadCrystalTkUnGrabbed;
			if (!add)
			{
				return;
			}
			this.headCrystalHandle.Grabbed += this.OnHeadCrystalGrabbed;
			this.headCrystalHandle.UnGrabbed += this.OnHeadCrystalUnGrabbed;
			this.headCrystalHandle.TkGrabbed += this.OnHeadCrystalTkGrabbed;
			this.headCrystalHandle.TkUnGrabbed += this.OnHeadCrystalTkUnGrabbed;
		}

		// Token: 0x06001B31 RID: 6961 RVA: 0x000B486C File Offset: 0x000B2A6C
		protected void SetHeadCrystalEffect(float value)
		{
			this.headCrystalAudioSourceLoop.pitch = this.headCrystalLoopAudioPitchCurve.Evaluate(value);
			this.headCrystalTearingAudioSource.volume = this.headCrystalTearingAudioVolumeCurve.Evaluate(value);
			this.headCrystalTearingAudioSource.pitch = this.headCrystalTearingAudioPitchCurve.Evaluate(value);
			this.headCrystalEffectController.SetIntensity(Mathf.Lerp(0.5f, 1f, value));
		}

		// Token: 0x06001B32 RID: 6962 RVA: 0x000B48DC File Offset: 0x000B2ADC
		protected virtual void OnHeadCrystalUnGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.headCrystalTearingAudioSource.Stop();
				base.CancelInvoke("UpdateHeadCrystalTearing");
				this.LookAt(null);
				GolemSpawner golemSpawner = this.spawner;
				if (golemSpawner != null)
				{
					golemSpawner.onCrystalUnGrabbed.Invoke();
				}
				this.SetHeadCrystalEffect(0f);
			}
		}

		// Token: 0x06001B33 RID: 6963 RVA: 0x000B492B File Offset: 0x000B2B2B
		protected virtual void OnHeadCrystalTkUnGrabbed(Handle handle, SpellTelekinesis telekinesis)
		{
			this.headCrystalTearingAudioSource.Stop();
			base.CancelInvoke("UpdateHeadCrystalTearing");
			this.LookAt(null);
			GolemSpawner golemSpawner = this.spawner;
			if (golemSpawner != null)
			{
				golemSpawner.onCrystalUnGrabbed.Invoke();
			}
			this.SetHeadCrystalEffect(0f);
		}

		// Token: 0x06001B34 RID: 6964 RVA: 0x000B496C File Offset: 0x000B2B6C
		protected void ChangeState(GolemController.State newState)
		{
			if (newState == this.state)
			{
				return;
			}
			GolemController.GolemStateChange onGolemStateChange = this.OnGolemStateChange;
			if (onGolemStateChange != null)
			{
				onGolemStateChange(newState);
			}
			if (newState == GolemController.State.WakingUp)
			{
				UnityEvent unityEvent = this.wakeEvent;
				if (unityEvent != null)
				{
					unityEvent.Invoke();
				}
			}
			if (newState == GolemController.State.Stunned && newState != this.state)
			{
				UnityEvent unityEvent2 = this.startStunEvent;
				if (unityEvent2 != null)
				{
					unityEvent2.Invoke();
				}
			}
			if (this.state == GolemController.State.Stunned && newState != this.state)
			{
				UnityEvent unityEvent3 = this.endStunEvent;
				if (unityEvent3 != null)
				{
					unityEvent3.Invoke();
				}
			}
			if (newState == GolemController.State.Defeated)
			{
				UnityEvent unityEvent4 = this.defeatEvent;
				if (unityEvent4 != null)
				{
					unityEvent4.Invoke();
				}
			}
			if (newState == GolemController.State.Dead)
			{
				UnityEvent unityEvent5 = this.killEvent;
				if (unityEvent5 != null)
				{
					unityEvent5.Invoke();
				}
			}
			this.state = newState;
		}

		// Token: 0x06001B35 RID: 6965 RVA: 0x000B4A1C File Offset: 0x000B2C1C
		protected internal override void ManagedUpdate()
		{
			base.ManagedUpdate();
			this.HeadEmissionColor = Color.Lerp(this.HeadEmissionColor, this.targetHeadEmissionColor, Time.deltaTime * 10f);
		}

		// Token: 0x06001B36 RID: 6966 RVA: 0x000B4A46 File Offset: 0x000B2C46
		protected virtual IEnumerator WakeCoroutine()
		{
			this.ChangeState(GolemController.State.WakingUp);
			while (this.isBusy)
			{
				yield return null;
			}
			this.characterController.radius = this.orgCharacterRadius;
			this.characterController.enableOverlapRecovery = true;
			this.ChangeState(GolemController.State.Active);
			GolemSpawner golemSpawner = this.spawner;
			if (golemSpawner != null)
			{
				UnityEvent onGolemAwaken = golemSpawner.onGolemAwaken;
				if (onGolemAwaken != null)
				{
					onGolemAwaken.Invoke();
				}
			}
			yield break;
		}

		// Token: 0x06001B37 RID: 6967 RVA: 0x000B4A55 File Offset: 0x000B2C55
		public virtual void SetMove(bool active)
		{
			this.animator.SetBool(GolemController.moveHash, active);
		}

		// Token: 0x06001B38 RID: 6968 RVA: 0x000B4A68 File Offset: 0x000B2C68
		public virtual void Stun(float duration = 0f, Action onStunStart = null, Action onStunnedBegin = null, Action onStunnedEnd = null)
		{
			this.isBusy = true;
			this.EndAbility();
			this.animator.SetBool(GolemController.stunHash, true);
			float weightTotal = 0f;
			int pickedDir = -2;
			for (int i = -1; i < 2; i++)
			{
				Vector3 point = base.transform.position + new Vector3(0f, this.stunCheckCapsuleHeights.x, 0f);
				Vector3 high = base.transform.position + new Vector3(0f, this.stunCheckCapsuleHeights.y, 0f);
				RaycastHit capsuleHit;
				bool hit = Physics.CapsuleCast(point, high, this.radiusMinMaxCapsuleCast.x, Quaternion.Euler(0f, (float)i * 120f, 0f) * base.transform.forward, out capsuleHit, this.radiusMinMaxCapsuleCast.z, Common.MakeLayerMask(new LayerName[]
				{
					LayerName.Default,
					LayerName.LocomotionOnly
				}));
				float weight = Mathf.InverseLerp(this.radiusMinMaxCapsuleCast.y, this.radiusMinMaxCapsuleCast.z, hit ? capsuleHit.distance : this.radiusMinMaxCapsuleCast.z);
				if (UnityEngine.Random.Range(0f, weightTotal + weight) > weightTotal)
				{
					pickedDir = i;
				}
				weightTotal += weight;
			}
			if (pickedDir == -2)
			{
				pickedDir = UnityEngine.Random.Range(-1, 2);
			}
			this.animator.SetInteger(GolemController.stunDirectionHash, pickedDir);
			base.StartCoroutine(this.StunCoroutine(duration, onStunStart, onStunnedBegin, onStunnedEnd));
		}

		// Token: 0x06001B39 RID: 6969 RVA: 0x000B4BDB File Offset: 0x000B2DDB
		protected IEnumerator StunCoroutine(float duration, Action onStunStart, Action onStunnedBegin, Action onStunnedEnd)
		{
			while (!this.animator.GetBool(GolemController.stunStartedHash))
			{
				yield return null;
			}
			if (this.state != GolemController.State.Defeated && this.state != GolemController.State.Dead)
			{
				this.ChangeState(GolemController.State.Stunned);
			}
			if (onStunStart != null)
			{
				onStunStart();
			}
			GolemController.GolemStunEvent onGolemStun = this.OnGolemStun;
			if (onGolemStun != null)
			{
				onGolemStun(duration);
			}
			while (!this.isStunned)
			{
				yield return null;
			}
			this.waitStunApproach = true;
			this.stunEndTime = Time.time + duration;
			if (onStunnedBegin != null)
			{
				onStunnedBegin();
			}
			if (this.isDefeated)
			{
				this.RefreshGrabbed(false);
			}
			if (duration > 0f)
			{
				while (this.animator.GetBool(GolemController.stunHash) && Time.time < this.stunEndTime)
				{
					if (this.isDefeated)
					{
						yield break;
					}
					yield return null;
				}
				this.StopStun();
			}
			while (this.isStunned)
			{
				yield return null;
			}
			if (onStunnedEnd != null)
			{
				onStunnedEnd();
			}
			if (this.state == GolemController.State.Stunned)
			{
				this.ChangeState(GolemController.State.Active);
			}
			yield break;
		}

		// Token: 0x06001B3A RID: 6970 RVA: 0x000B4C07 File Offset: 0x000B2E07
		protected void InvokeRampageState()
		{
			GolemController.GolemRampageEvent onGolemRampage = this.OnGolemRampage;
			if (onGolemRampage != null)
			{
				onGolemRampage();
			}
			this.ChangeState(GolemController.State.Rampage);
		}

		// Token: 0x06001B3B RID: 6971 RVA: 0x000B4C21 File Offset: 0x000B2E21
		protected IEnumerator AttackMotionCoroutine(Action onAttackEnd)
		{
			this.replaceAttackCoroutineAction = null;
			yield return Yielders.EndOfFrame;
			while (this.replaceAttackCoroutineAction == null && this.inAttackMotion)
			{
				yield return null;
			}
			if (onAttackEnd != null)
			{
				onAttackEnd();
			}
			if (this.state == GolemController.State.Rampage && !this.inAttackMotion)
			{
				this.ChangeState(GolemController.State.Active);
			}
			this.attackEndCoroutine = null;
			Action action = this.replaceAttackCoroutineAction;
			if (action != null)
			{
				action();
			}
			yield break;
		}

		// Token: 0x06001B3C RID: 6972 RVA: 0x000B4C37 File Offset: 0x000B2E37
		public bool TryGetCurrentAttackMotion(out GolemController.AttackMotion meleeAttack)
		{
			meleeAttack = this.lastAttackMotion;
			return this.inAttackMotion;
		}

		// Token: 0x06001B3D RID: 6973 RVA: 0x000B4C4C File Offset: 0x000B2E4C
		public bool WithinForwardCone(Transform target, float maxDistance, float maxAngle)
		{
			float num;
			float num2;
			return this.WithinForwardCone(target, maxDistance, out num, maxAngle, out num2);
		}

		// Token: 0x06001B3E RID: 6974 RVA: 0x000B4C68 File Offset: 0x000B2E68
		public bool WithinForwardCone(Transform target, float maxDistance, out float dist, float maxAngle)
		{
			float num;
			return this.WithinForwardCone(target, maxDistance, out dist, maxAngle, out num);
		}

		// Token: 0x06001B3F RID: 6975 RVA: 0x000B4C84 File Offset: 0x000B2E84
		public bool WithinForwardCone(Transform target, float maxDistance, float maxAngle, out float angle)
		{
			float num;
			return this.WithinForwardCone(target, maxDistance, out num, maxAngle, out angle);
		}

		// Token: 0x06001B40 RID: 6976 RVA: 0x000B4CA0 File Offset: 0x000B2EA0
		public bool WithinForwardCone(Transform target, float maxDistance, out float dist, float maxAngle, out float angle)
		{
			dist = 0f;
			angle = 0f;
			if (target == null)
			{
				return false;
			}
			angle = Vector3.Angle(base.transform.forward.ToXZ().normalized, (target.position - base.transform.position).ToXZ().normalized);
			if (angle > maxAngle)
			{
				return false;
			}
			dist = Vector3.Distance(this.eyeTransform.transform.position, target.position);
			return dist <= maxDistance;
		}

		// Token: 0x06001B41 RID: 6977 RVA: 0x000B4D38 File Offset: 0x000B2F38
		public bool IsSightable(Transform target, float maxDistance, float maxAngle)
		{
			float sightDistance;
			return this.WithinForwardCone(target, maxDistance, out sightDistance, maxAngle) && !Physics.Raycast(this.eyeTransform.position, target.position - this.eyeTransform.position, sightDistance, this.sightLayer);
		}

		// Token: 0x06001B42 RID: 6978 RVA: 0x000B4D8C File Offset: 0x000B2F8C
		public void UpdateSwingEffects()
		{
			GolemController.AttackMotion attack;
			if (this.TryGetCurrentAttackMotion(out attack))
			{
				if (GolemController.GetAttackSide(attack).HasFlag(GolemController.AttackSide.Left))
				{
					this.UpdateSwingEffect(Side.Left);
				}
				if (GolemController.GetAttackSide(attack).HasFlag(GolemController.AttackSide.Right))
				{
					this.UpdateSwingEffect(Side.Right);
					return;
				}
			}
			else
			{
				this.StopSwingEffect(Side.Left);
				this.StopSwingEffect(Side.Right);
			}
		}

		// Token: 0x06001B43 RID: 6979 RVA: 0x000B4DF0 File Offset: 0x000B2FF0
		private void UpdateSwingEffect(Side side)
		{
			EffectInstance swingEffect = this.swingEffects[(int)side];
			VelocityTracker swingTracker = this.swingTrackers[(int)side];
			Rigidbody arm = this.armRigidbodies[(int)side];
			Vector3 armVelocity = swingTracker.velocity;
			if ((swingEffect == null && armVelocity.magnitude > this.swingVelocity.y) || (swingEffect != null && armVelocity.magnitude > this.swingVelocity.x))
			{
				if (swingEffect == null)
				{
					swingEffect = (this.swingEffects[(int)side] = this.swingEffectData.Spawn(swingTracker, null, true, null, false, 0f, 1f, Array.Empty<Type>()));
					swingEffect.Play(0, false, false);
				}
				Vector3 armUpDir = arm.transform.right * (float)((side == Side.Left) ? -1 : 1);
				swingTracker.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(armVelocity, armUpDir), armUpDir);
				swingEffect.SetIntensity(Mathf.InverseLerp(0f, 15f, armVelocity.magnitude));
				return;
			}
			if (swingEffect != null)
			{
				swingEffect.End(false, -1f);
				this.swingEffects[(int)side] = null;
			}
		}

		// Token: 0x06001B44 RID: 6980 RVA: 0x000B4EFB File Offset: 0x000B30FB
		protected void StopSwingEffect(Side side)
		{
			if (this.swingEffects[(int)side] == null)
			{
				return;
			}
			EffectInstance effectInstance = this.swingEffects[(int)side];
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			this.swingEffects[(int)side] = null;
		}

		// Token: 0x06001B45 RID: 6981 RVA: 0x000B4F2C File Offset: 0x000B312C
		public virtual void Deploy(float duration = 0f, Action onDeployStart = null, Action onDeployedBegin = null, Action onDeployedEnd = null)
		{
			this.isBusy = true;
			this.animator.SetBool(GolemController.deployHash, true);
			if (this.deployRoutine != null)
			{
				base.StopCoroutine(this.deployRoutine);
			}
			this.deployRoutine = base.StartCoroutine(this.DeployCoroutine(duration, onDeployStart, onDeployedBegin, onDeployedEnd));
		}

		// Token: 0x06001B46 RID: 6982 RVA: 0x000B4F7C File Offset: 0x000B317C
		protected IEnumerator DeployCoroutine(float duration, Action onDeployStart, Action onDeployedBegin, Action onDeployedEnd)
		{
			while (!this.animator.GetBool(GolemController.deployStartedHash))
			{
				yield return null;
			}
			if (!this.animator.GetBool(GolemController.deployHash))
			{
				yield break;
			}
			if (onDeployStart != null)
			{
				onDeployStart();
			}
			while (!this.isDeployed)
			{
				if (!this.animator.GetBool(GolemController.deployHash))
				{
					yield break;
				}
				yield return null;
			}
			if (onDeployedBegin != null)
			{
				onDeployedBegin();
			}
			if (duration > 0f)
			{
				float time = 0f;
				while (this.animator.GetBool(GolemController.deployHash) && time < duration)
				{
					time += Time.deltaTime;
					yield return null;
				}
				this.animator.SetBool(GolemController.deployHash, false);
			}
			while (this.isDeployed)
			{
				yield return null;
			}
			if (onDeployedEnd != null)
			{
				onDeployedEnd();
			}
			this.deployRoutine = null;
			yield break;
		}

		// Token: 0x06001B47 RID: 6983 RVA: 0x000B4FA8 File Offset: 0x000B31A8
		public virtual void StopDeploy()
		{
			if (this.deployRoutine != null)
			{
				base.StopCoroutine(this.deployRoutine);
			}
			this.isDeployed = false;
			this.animator.SetBool(GolemController.deployStartedHash, false);
			this.animator.SetBool(GolemController.deployHash, false);
			this.RunAfter(delegate()
			{
				this.isDeployed = false;
			}, 2.5f, false);
		}

		// Token: 0x06001B5D RID: 7005 RVA: 0x000B53BC File Offset: 0x000B35BC
		[CompilerGenerated]
		private bool <ForceUngripClimbersEnumerable>g__UngrabHand|251_0(RagdollHand hand, ref GolemController.<>c__DisplayClass251_0 A_2)
		{
			if (A_2.climbedOnly && !this.grabbedParts.ContainsKey(hand))
			{
				return false;
			}
			if (hand.grabbedHandle != null)
			{
				hand.UnGrab(false);
			}
			RagdollHandClimb climb = hand.climb;
			if (((climb != null) ? climb.gripPhysicBody : null) != null)
			{
				hand.climb.UnGrip();
			}
			RagdollHandClimb climb2 = hand.climb;
			if (climb2 != null)
			{
				climb2.DisableGripTemp(0.5f);
			}
			return true;
		}

		// Token: 0x04001979 RID: 6521
		[Header("References")]
		public Animator animator;

		// Token: 0x0400197A RID: 6522
		public CharacterController characterController;

		// Token: 0x0400197B RID: 6523
		public GolemAnimatorEvent animatorEvent;

		// Token: 0x0400197C RID: 6524
		public List<CollisionListener> headListeners = new List<CollisionListener>();

		// Token: 0x0400197D RID: 6525
		public List<Rigidbody> bodyParts = new List<Rigidbody>();

		// Token: 0x0400197E RID: 6526
		public List<Collider> bodyPartColliders = new List<Collider>();

		// Token: 0x0400197F RID: 6527
		public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

		// Token: 0x04001980 RID: 6528
		public List<Collider> ignoreCollisionColliders = new List<Collider>();

		// Token: 0x04001981 RID: 6529
		public List<Transform> magicSprayPoints = new List<Transform>();

		// Token: 0x04001982 RID: 6530
		[Header("Global")]
		public bool awakeOnStart;

		// Token: 0x04001983 RID: 6531
		public float moveSpeedMultiplier = 1.1f;

		// Token: 0x04001984 RID: 6532
		public float hitDamageMultiplier = 1f;

		// Token: 0x04001985 RID: 6533
		public float hitForceMultiplier = 1.1f;

		// Token: 0x04001986 RID: 6534
		public Renderer headRenderer;

		// Token: 0x04001987 RID: 6535
		[Header("Head look")]
		public MultiAimConstraint headAimConstraint;

		// Token: 0x04001988 RID: 6536
		public Transform headIktarget;

		// Token: 0x04001989 RID: 6537
		public float headLookSpeed = 3f;

		// Token: 0x0400198A RID: 6538
		public LookMode lookMode;

		// Token: 0x0400198B RID: 6539
		public LayerMask sightLayer = 1;

		// Token: 0x0400198C RID: 6540
		public float headLookSpeedMultiplier = 1f;

		// Token: 0x0400198D RID: 6541
		public Transform eyeTransform;

		// Token: 0x0400198E RID: 6542
		[Header("Face plate")]
		public Rigidbody facePlateBody;

		// Token: 0x0400198F RID: 6543
		public SimpleBreakable facePlateBreakable;

		// Token: 0x04001990 RID: 6544
		public ConfigurableJoint facePlateJoint;

		// Token: 0x04001991 RID: 6545
		public float facePlateUnlockAngle = 5f;

		// Token: 0x04001992 RID: 6546
		public float facePlateOpenAngle = 110f;

		// Token: 0x04001993 RID: 6547
		[Header("Head crystal")]
		public ConfigurableJoint headCrystalJoint;

		// Token: 0x04001994 RID: 6548
		public VisualEffect headCrystalLinkVfx;

		// Token: 0x04001995 RID: 6549
		public ParticleSystem headCrystalParticle;

		// Token: 0x04001996 RID: 6550
		public FxController headCrystalEffectController;

		// Token: 0x04001997 RID: 6551
		public AudioSource headCrystalAudioSourceLoop;

		// Token: 0x04001998 RID: 6552
		public AudioSource headCrystalTearingAudioSource;

		// Token: 0x04001999 RID: 6553
		public AnimationCurve headCrystalLoopAudioPitchCurve;

		// Token: 0x0400199A RID: 6554
		public AnimationCurve headCrystalTearingAudioPitchCurve;

		// Token: 0x0400199B RID: 6555
		public AnimationCurve headCrystalTearingAudioVolumeCurve;

		// Token: 0x0400199C RID: 6556
		public Rigidbody headCrystalBody;

		// Token: 0x0400199D RID: 6557
		public Handle headCrystalHandle;

		// Token: 0x0400199E RID: 6558
		public float headCrystalGrabMass = 5f;

		// Token: 0x0400199F RID: 6559
		public float headCrystalGrabDrag = 100f;

		// Token: 0x040019A0 RID: 6560
		public float headCrystalShutdownDuration = 8f;

		// Token: 0x040019A1 RID: 6561
		public float headCrystalTearingDistance = 1f;

		// Token: 0x040019A2 RID: 6562
		[Header("Death")]
		public Vector2 stunCheckCapsuleHeights = new Vector2(1f, 2f);

		// Token: 0x040019A3 RID: 6563
		public Vector3 radiusMinMaxCapsuleCast = new Vector3(1.5f, 3.5f, 6f);

		// Token: 0x040019A4 RID: 6564
		public AudioSource killAudioSource;

		// Token: 0x040019A5 RID: 6565
		public ParticleSystem killparticle;

		// Token: 0x040019A6 RID: 6566
		public float killExplosionForce = 5f;

		// Token: 0x040019A7 RID: 6567
		public float killExplosionRadius = 5f;

		// Token: 0x040019A8 RID: 6568
		public float killExplosionUpward = 0.5f;

		// Token: 0x040019A9 RID: 6569
		public ForceMode killExplosionForceMode = ForceMode.VelocityChange;

		// Token: 0x040019AA RID: 6570
		public Transform killExplosionSourceTransform;

		// Token: 0x040019AB RID: 6571
		public List<Transform> colliderResizeOnDeath;

		// Token: 0x040019AC RID: 6572
		public UnityEvent wakeEvent;

		// Token: 0x040019AD RID: 6573
		public UnityEvent startStunEvent;

		// Token: 0x040019AE RID: 6574
		public UnityEvent endStunEvent;

		// Token: 0x040019AF RID: 6575
		public UnityEvent crystalBreakEvent;

		// Token: 0x040019B0 RID: 6576
		public UnityEvent defeatEvent;

		// Token: 0x040019B1 RID: 6577
		public UnityEvent killEvent;

		// Token: 0x040019B2 RID: 6578
		[Header("Swing")]
		public Vector2 swingVelocity = new Vector2(2f, 5f);

		// Token: 0x040019B3 RID: 6579
		public string swingEffectId = "GolemSwingArm";

		// Token: 0x040019B4 RID: 6580
		protected EffectData swingEffectData;

		// Token: 0x040019B5 RID: 6581
		protected EffectInstance[] swingEffects = new EffectInstance[2];

		// Token: 0x040019B6 RID: 6582
		public VelocityTracker[] swingTrackers = new VelocityTracker[2];

		// Token: 0x040019B7 RID: 6583
		public Transform handLeft;

		// Token: 0x040019B8 RID: 6584
		public Transform handRight;

		// Token: 0x040019B9 RID: 6585
		public Rigidbody[] armRigidbodies = new Rigidbody[2];

		// Token: 0x040019BA RID: 6586
		[Header("Powers")]
		public Golem.Tier tier;

		// Token: 0x040019BB RID: 6587
		public List<GolemAbility> abilities = new List<GolemAbility>();

		// Token: 0x040019BC RID: 6588
		private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

		// Token: 0x040019C3 RID: 6595
		[Range(0f, 2f)]
		protected int wakeMotion;

		// Token: 0x040019C4 RID: 6596
		public static int awakeHash;

		// Token: 0x040019C5 RID: 6597
		public static int wakeMotionHash;

		// Token: 0x040019C6 RID: 6598
		public static int moveSpeedMultiplierHash;

		// Token: 0x040019C7 RID: 6599
		public static int isBusyHash;

		// Token: 0x040019C8 RID: 6600
		public static int moveHash;

		// Token: 0x040019C9 RID: 6601
		public static int inMovementHash;

		// Token: 0x040019CA RID: 6602
		public static int locomotionMultHash;

		// Token: 0x040019CB RID: 6603
		public static int staggerHash;

		// Token: 0x040019CC RID: 6604
		public static int staggerLateralHash;

		// Token: 0x040019CD RID: 6605
		public static int staggerAxialHash;

		// Token: 0x040019CE RID: 6606
		public static int resistPushHash;

		// Token: 0x040019CF RID: 6607
		public static int attackHash;

		// Token: 0x040019D0 RID: 6608
		public static int attackMotionHash;

		// Token: 0x040019D1 RID: 6609
		public static int deployHash;

		// Token: 0x040019D2 RID: 6610
		public static int deployStartedHash;

		// Token: 0x040019D3 RID: 6611
		public static int isDeployedHash;

		// Token: 0x040019D4 RID: 6612
		public static int stunHash;

		// Token: 0x040019D5 RID: 6613
		public static int stunDirectionHash;

		// Token: 0x040019D6 RID: 6614
		public static int stunStartedHash;

		// Token: 0x040019D7 RID: 6615
		public static int isStunnedHash;

		// Token: 0x040019D8 RID: 6616
		public static int inAttackMotionHash;

		// Token: 0x040019E1 RID: 6625
		[NonSerialized]
		public Color targetHeadEmissionColor = Color.black;

		// Token: 0x040019E2 RID: 6626
		[NonSerialized]
		public FloatHandler speed = new FloatHandler();

		// Token: 0x040019E3 RID: 6627
		[NonSerialized]
		public GolemSpawner spawner;

		// Token: 0x040019E4 RID: 6628
		[NonSerialized]
		public GolemAbility currentAbility;

		// Token: 0x040019E5 RID: 6629
		[NonSerialized]
		public Transform attackTarget;

		// Token: 0x040019EC RID: 6636
		protected float orgCharacterRadius;

		// Token: 0x040019ED RID: 6637
		protected Vector3 gravityVelocity;

		// Token: 0x040019EE RID: 6638
		protected Coroutine headLookCoroutine;

		// Token: 0x040019EF RID: 6639
		protected float orgHeadCrystalMass;

		// Token: 0x040019F0 RID: 6640
		protected float orgHeadCrystalDrag;

		// Token: 0x040019F1 RID: 6641
		protected float meleeAttackStartTime;

		// Token: 0x040019F2 RID: 6642
		protected float stunEndTime;

		// Token: 0x040019F3 RID: 6643
		protected bool waitStunApproach;

		// Token: 0x040019F4 RID: 6644
		protected Coroutine deployRoutine;

		// Token: 0x040019F5 RID: 6645
		protected Coroutine attackEndCoroutine;

		// Token: 0x040019F6 RID: 6646
		protected Action replaceAttackCoroutineAction;

		// Token: 0x020008BB RID: 2235
		public enum AttackMotion
		{
			// Token: 0x04004268 RID: 17000
			Rampage,
			// Token: 0x04004269 RID: 17001
			SwingRight,
			// Token: 0x0400426A RID: 17002
			SwingLeft,
			// Token: 0x0400426B RID: 17003
			ComboSwing,
			// Token: 0x0400426C RID: 17004
			ComboSwingAndSlam,
			// Token: 0x0400426D RID: 17005
			SwingBehindRight,
			// Token: 0x0400426E RID: 17006
			SwingBehindLeft,
			// Token: 0x0400426F RID: 17007
			SwingBehindRightTurnBack,
			// Token: 0x04004270 RID: 17008
			SwingBehindLeftTurnBack,
			// Token: 0x04004271 RID: 17009
			SwingLeftStep,
			// Token: 0x04004272 RID: 17010
			SwingRightStep,
			// Token: 0x04004273 RID: 17011
			Slam,
			// Token: 0x04004274 RID: 17012
			Stampede,
			// Token: 0x04004275 RID: 17013
			Breakdance,
			// Token: 0x04004276 RID: 17014
			SlamLeftTurn90,
			// Token: 0x04004277 RID: 17015
			SlamRightTurn90,
			// Token: 0x04004278 RID: 17016
			SwingLeftTurn90,
			// Token: 0x04004279 RID: 17017
			SwingRightTurn90,
			// Token: 0x0400427A RID: 17018
			Spray,
			// Token: 0x0400427B RID: 17019
			SprayDance,
			// Token: 0x0400427C RID: 17020
			Throw,
			// Token: 0x0400427D RID: 17021
			Beam,
			// Token: 0x0400427E RID: 17022
			SelfImbue,
			// Token: 0x0400427F RID: 17023
			RadialBurst,
			// Token: 0x04004280 RID: 17024
			ShakeOff,
			// Token: 0x04004281 RID: 17025
			LightShake
		}

		// Token: 0x020008BC RID: 2236
		[Flags]
		public enum AttackSide
		{
			// Token: 0x04004283 RID: 17027
			None = 0,
			// Token: 0x04004284 RID: 17028
			Left = 1,
			// Token: 0x04004285 RID: 17029
			Right = 2,
			// Token: 0x04004286 RID: 17030
			Both = 3
		}

		// Token: 0x020008BD RID: 2237
		// (Invoke) Token: 0x06004141 RID: 16705
		public delegate void GolemStateChange(GolemController.State newState);

		// Token: 0x020008BE RID: 2238
		// (Invoke) Token: 0x06004145 RID: 16709
		public delegate void GolemAttackEvent(GolemController.AttackMotion motion, GolemAbility ability);

		// Token: 0x020008BF RID: 2239
		// (Invoke) Token: 0x06004149 RID: 16713
		public delegate void GolemRampageEvent();

		// Token: 0x020008C0 RID: 2240
		// (Invoke) Token: 0x0600414D RID: 16717
		public delegate void GolemStaggerEvent(Vector2 direction);

		// Token: 0x020008C1 RID: 2241
		// (Invoke) Token: 0x06004151 RID: 16721
		public delegate void GolemStunEvent(float duration);

		// Token: 0x020008C2 RID: 2242
		// (Invoke) Token: 0x06004155 RID: 16725
		public delegate void GolemInterrupt();

		// Token: 0x020008C3 RID: 2243
		// (Invoke) Token: 0x06004159 RID: 16729
		public delegate void GolemDealDamage(Creature target, float damage);

		// Token: 0x020008C4 RID: 2244
		public enum State
		{
			// Token: 0x04004288 RID: 17032
			Inactive,
			// Token: 0x04004289 RID: 17033
			WakingUp = 6,
			// Token: 0x0400428A RID: 17034
			Active = 1,
			// Token: 0x0400428B RID: 17035
			Stunned,
			// Token: 0x0400428C RID: 17036
			Rampage,
			// Token: 0x0400428D RID: 17037
			Defeated,
			// Token: 0x0400428E RID: 17038
			Dead
		}
	}
}
