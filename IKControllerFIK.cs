using System;
using RootMotion;
using RootMotion.FinalIK;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000272 RID: 626
	[AddComponentMenu("ThunderRoad/Creatures/IK Controller")]
	public class IKControllerFIK : IkController
	{
		// Token: 0x170001C3 RID: 451
		// (get) Token: 0x06001C26 RID: 7206 RVA: 0x000BB2A2 File Offset: 0x000B94A2
		// (set) Token: 0x06001C27 RID: 7207 RVA: 0x000BB2AA File Offset: 0x000B94AA
		public VRIK vrik { get; protected set; }

		// Token: 0x170001C4 RID: 452
		// (get) Token: 0x06001C28 RID: 7208 RVA: 0x000BB2B3 File Offset: 0x000B94B3
		// (set) Token: 0x06001C29 RID: 7209 RVA: 0x000BB2BB File Offset: 0x000B94BB
		public BipedIKCustom bipedIk { get; protected set; }

		// Token: 0x170001C5 RID: 453
		// (get) Token: 0x06001C2A RID: 7210 RVA: 0x000BB2C4 File Offset: 0x000B94C4
		// (set) Token: 0x06001C2B RID: 7211 RVA: 0x000BB2CC File Offset: 0x000B94CC
		public FullBodyBipedIK fullBodyBipedIk { get; protected set; }

		// Token: 0x170001C6 RID: 454
		// (get) Token: 0x06001C2C RID: 7212 RVA: 0x000BB2D5 File Offset: 0x000B94D5
		// (set) Token: 0x06001C2D RID: 7213 RVA: 0x000BB2DD File Offset: 0x000B94DD
		public FBBIKHeadEffector headEffector { get; protected set; }

		// Token: 0x06001C2E RID: 7214 RVA: 0x000BB2E8 File Offset: 0x000B94E8
		public override void Setup()
		{
			this.vrik = base.gameObject.AddComponent<VRIK>();
			this.vrik.solver.locomotion.weight = 0f;
			this.vrik.solver.locomotion.footDistance = 0.25f;
			this.vrik.solver.locomotion.stepThreshold = 0.25f;
			this.vrik.solver.locomotion.angleThreshold = 60f;
			this.vrik.solver.plantFeet = false;
			this.vrik.solver.spine.positionWeight = 0f;
			this.vrik.solver.spine.rotationWeight = 0f;
			this.vrik.solver.spine.pelvisPositionWeight = 0f;
			this.vrik.solver.spine.pelvisRotationWeight = 0f;
			this.vrik.solver.spine.rotateChestByHands = 0f;
			this.vrik.solver.spine.maxRootAngle = 180f;
			this.vrik.solver.spine.maintainPelvisPosition = 0f;
			this.vrik.solver.spine.bodyPosStiffness = 0.5f;
			this.vrik.solver.spine.bodyRotStiffness = 0.2f;
			this.vrik.solver.spine.neckStiffness = 0f;
			this.vrik.solver.leftArm.positionWeight = 0f;
			this.vrik.solver.leftArm.rotationWeight = 0f;
			this.vrik.solver.leftArm.stretchCurve = this.stretchArmsCurve;
			this.vrik.solver.rightArm.positionWeight = 0f;
			this.vrik.solver.rightArm.rotationWeight = 0f;
			this.vrik.solver.rightArm.stretchCurve = this.stretchArmsCurve;
			IKSolverVR.Arm rightArm = this.vrik.solver.rightArm;
			RagdollHand handRight = this.creature.handRight;
			rightArm.wristToPalmAxis = ((handRight != null) ? handRight.axisPalm : Vector3.left);
			IKSolverVR.Arm rightArm2 = this.vrik.solver.rightArm;
			RagdollHand handRight2 = this.creature.handRight;
			rightArm2.palmToThumbAxis = ((handRight2 != null) ? handRight2.axisThumb : Vector3.up);
			IKSolverVR.Arm leftArm = this.vrik.solver.leftArm;
			RagdollHand handLeft = this.creature.handLeft;
			leftArm.wristToPalmAxis = ((handLeft != null) ? handLeft.axisPalm : Vector3.left);
			IKSolverVR.Arm leftArm2 = this.vrik.solver.leftArm;
			RagdollHand handLeft2 = this.creature.handLeft;
			leftArm2.palmToThumbAxis = ((handLeft2 != null) ? handLeft2.axisThumb : Vector3.down);
			this.vrik.solver.leftLeg.positionWeight = 0f;
			this.vrik.solver.leftLeg.rotationWeight = 0f;
			this.vrik.solver.leftLeg.swivelOffset = -20f;
			this.vrik.solver.leftLeg.bendToTargetWeight = 0f;
			this.vrik.solver.leftLeg.stretchCurve = this.stretchLegsCurve;
			this.vrik.solver.rightLeg.positionWeight = 0f;
			this.vrik.solver.rightLeg.rotationWeight = 0f;
			this.vrik.solver.rightLeg.swivelOffset = 0f;
			this.vrik.solver.rightLeg.bendToTargetWeight = 0f;
			this.vrik.solver.rightLeg.stretchCurve = this.stretchLegsCurve;
			IKSolver iksolver = this.vrik.GetIKSolver();
			iksolver.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver.OnPreUpdate, new IKSolver.UpdateDelegate(this.PreIKUpdate));
			IKSolver iksolver2 = this.vrik.GetIKSolver();
			iksolver2.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver2.OnPostUpdate, new IKSolver.UpdateDelegate(this.PostIKUpdate));
			this.vrik.enabled = false;
			this.bipedIk = base.gameObject.AddComponent<BipedIKCustom>();
			this.bipedIk.references.root = base.transform;
			this.bipedIk.references.pelvis = this.creature.animator.GetBoneTransform(HumanBodyBones.Hips);
			Transform chestBone = this.creature.animator.GetBoneTransform(HumanBodyBones.Chest);
			int bones = 2 + (chestBone ? 1 : 0);
			this.bipedIk.references.spine = new Transform[bones];
			this.bipedIk.references.spine[0] = this.creature.animator.GetBoneTransform(HumanBodyBones.Spine);
			if (chestBone)
			{
				this.bipedIk.references.spine[1] = chestBone;
			}
			this.bipedIk.references.spine[bones - 1] = this.creature.animator.GetBoneTransform(HumanBodyBones.Neck);
			Transform leftEye = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftEye);
			Transform rightEye = this.creature.animator.GetBoneTransform(HumanBodyBones.RightEye);
			if (leftEye && rightEye)
			{
				this.bipedIk.references.eyes = new Transform[2];
				this.bipedIk.references.eyes[0] = leftEye;
				this.bipedIk.references.eyes[1] = rightEye;
			}
			else if (leftEye || rightEye)
			{
				this.bipedIk.references.eyes = new Transform[1];
				this.bipedIk.references.eyes[0] = (leftEye ?? rightEye);
			}
			this.bipedIk.solvers.lookAt.IKPositionWeight = 0f;
			this.bipedIk.solvers.lookAt.bodyWeight = 0.6f;
			this.bipedIk.solvers.lookAt.headWeight = 1f;
			this.bipedIk.solvers.lookAt.eyesWeight = 0.25f;
			this.bipedIk.solvers.lookAt.clampWeight = 0.7f;
			this.bipedIk.solvers.lookAt.clampWeightHead = 0.8f;
			this.bipedIk.solvers.lookAt.clampWeightEyes = 0.95f;
			this.bipedIk.references.head = this.creature.animator.GetBoneTransform(HumanBodyBones.Head);
			this.bipedIk.references.leftUpperArm = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
			this.bipedIk.references.leftForearm = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
			this.bipedIk.references.leftHand = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftHand);
			this.bipedIk.references.rightUpperArm = this.creature.animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
			this.bipedIk.references.rightForearm = this.creature.animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
			this.bipedIk.references.rightHand = this.creature.animator.GetBoneTransform(HumanBodyBones.RightHand);
			this.bipedIk.references.leftThigh = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
			this.bipedIk.references.leftCalf = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
			this.bipedIk.references.leftFoot = this.creature.animator.GetBoneTransform(HumanBodyBones.LeftFoot);
			this.bipedIk.references.rightThigh = this.creature.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
			this.bipedIk.references.rightCalf = this.creature.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
			this.bipedIk.references.rightFoot = this.creature.animator.GetBoneTransform(HumanBodyBones.RightFoot);
			this.bipedIk.solvers.aim.IKPositionWeight = 0f;
			this.bipedIk.solvers.leftFoot.IKPositionWeight = 0f;
			this.bipedIk.solvers.leftFoot.IKRotationWeight = 0f;
			this.bipedIk.solvers.rightFoot.IKPositionWeight = 0f;
			this.bipedIk.solvers.rightFoot.IKRotationWeight = 0f;
			this.bipedIk.solvers.leftHand.IKPositionWeight = 0f;
			this.bipedIk.solvers.leftHand.IKRotationWeight = 0f;
			this.bipedIk.solvers.rightHand.IKPositionWeight = 0f;
			this.bipedIk.solvers.rightHand.IKRotationWeight = 0f;
			this.bipedIk.solvers.spine.IKPositionWeight = 0f;
			this.bipedIk.solvers.aim.IKPositionWeight = 0f;
			this.bipedIk.solvers.pelvis.positionWeight = 0f;
			this.bipedIk.solvers.pelvis.rotationWeight = 0f;
			this.bipedIk.OnPreUpdateEvent += this.PreIKUpdate;
			this.bipedIk.OnPostUpdateEvent += this.PostIKUpdate;
			this.bipedIk.enabled = false;
			this.fullBodyBipedIk = base.gameObject.AddComponent<FullBodyBipedIK>();
			this.fullBodyBipedIk.references = new BipedReferences();
			BipedReferences.AutoDetectReferences(ref this.fullBodyBipedIk.references, base.transform, new BipedReferences.AutoDetectParams(true, false));
			this.fullBodyBipedIk.solver.rootNode = IKSolverFullBodyBiped.DetectRootNodeBone(this.fullBodyBipedIk.references);
			this.fullBodyBipedIk.solver.SetToReferences(this.fullBodyBipedIk.references, this.fullBodyBipedIk.solver.rootNode);
			IKSolver iksolver3 = this.fullBodyBipedIk.GetIKSolver();
			iksolver3.OnPreUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver3.OnPreUpdate, new IKSolver.UpdateDelegate(this.PreIKUpdate));
			IKSolver iksolver4 = this.fullBodyBipedIk.GetIKSolver();
			iksolver4.OnPostUpdate = (IKSolver.UpdateDelegate)Delegate.Combine(iksolver4.OnPostUpdate, new IKSolver.UpdateDelegate(this.PostIKUpdate));
			this.fullBodyBipedIk.solver.leftArmMapping.weight = 0f;
			this.fullBodyBipedIk.solver.rightArmMapping.weight = 0f;
			this.fullBodyBipedIk.enabled = false;
			this.headEffector = new GameObject("HeadEffector").AddComponent<FBBIKHeadEffector>();
			this.headEffector.transform.SetParent(base.transform);
			this.headEffector.transform.localPosition = Vector3.zero;
			this.headEffector.transform.localRotation = Quaternion.identity;
			this.headEffector.ik = this.fullBodyBipedIk;
			this.headEffector.bendBones = new FBBIKHeadEffector.BendBone[3];
			this.headEffector.bendBones[0] = new FBBIKHeadEffector.BendBone(this.creature.animator.GetBoneTransform(HumanBodyBones.Spine), 1f);
			this.headEffector.bendBones[1] = new FBBIKHeadEffector.BendBone(this.creature.animator.GetBoneTransform(HumanBodyBones.Chest), 1f);
			this.headEffector.bendBones[2] = new FBBIKHeadEffector.BendBone(this.creature.animator.GetBoneTransform(HumanBodyBones.Neck), 1f);
			this.headEffector.CCDBones = new Transform[3];
			this.headEffector.CCDBones[0] = this.creature.animator.GetBoneTransform(HumanBodyBones.Spine);
			this.headEffector.CCDBones[1] = this.creature.animator.GetBoneTransform(HumanBodyBones.Chest);
			this.headEffector.CCDBones[2] = this.creature.animator.GetBoneTransform(HumanBodyBones.Neck);
			this.headEffector.CCDWeight = 0f;
			this.headEffector.positionWeight = 0f;
			this.headEffector.rotationWeight = 0f;
			this.headEffector.bendWeight = 0f;
			FBBIKHeadEffector headEffector = this.headEffector;
			headEffector.OnPostHeadEffectorFK = (IKSolver.UpdateDelegate)Delegate.Combine(headEffector.OnPostHeadEffectorFK, new IKSolver.UpdateDelegate(this.PostIKUpdate));
			this.headEffector.enabled = false;
			base.initialized = true;
		}

		// Token: 0x06001C2F RID: 7215 RVA: 0x000BBFD5 File Offset: 0x000BA1D5
		protected Transform AddFingerTip(Transform bone, float tipDistance)
		{
			Transform transform = new GameObject("tip").transform;
			transform.SetParent(bone);
			transform.localPosition = new Vector3(tipDistance, 0f, 0f);
			transform.localRotation = Quaternion.identity;
			return transform;
		}

		// Token: 0x06001C30 RID: 7216 RVA: 0x000BC00E File Offset: 0x000BA20E
		public override void SetFullbody(bool active)
		{
			base.SetFullbody(active);
			this.RefreshState();
		}

		// Token: 0x06001C31 RID: 7217 RVA: 0x000BC020 File Offset: 0x000BA220
		protected void RefreshState()
		{
			if (!this.eyesTarget && !this.headEnabled && !this.hipsEnabled && !this.handLeftEnabled && !this.handRightEnabled && !this.footLeftEnabled && !this.footRightEnabled && !this.shoulderLeftEnabled && !this.shoulderRightEnabled)
			{
				this.fullbody = false;
				this.SetState(IKControllerFIK.State.Disabled);
				return;
			}
			if (this.creature.player)
			{
				this.SetState(IKControllerFIK.State.Player);
				return;
			}
			if (this.fullbody || this.headEnabled || this.shoulderLeftEnabled || this.shoulderRightEnabled)
			{
				this.SetState(IKControllerFIK.State.FullBody);
				return;
			}
			this.SetState(IKControllerFIK.State.Default);
		}

		// Token: 0x06001C32 RID: 7218 RVA: 0x000BC0D4 File Offset: 0x000BA2D4
		protected void SetState(IKControllerFIK.State state)
		{
			this.bipedIk.enabled = (state == IKControllerFIK.State.Default);
			this.fullBodyBipedIk.enabled = (state == IKControllerFIK.State.FullBody);
			if (this.vrik)
			{
				this.vrik.enabled = (state == IKControllerFIK.State.Player);
			}
			this.turnBodyByHeadAndHands = (state == IKControllerFIK.State.Player);
		}

		/// LOCOMOTION
		// Token: 0x06001C33 RID: 7219 RVA: 0x000BC135 File Offset: 0x000BA335
		public override float GetLocomotionWeight()
		{
			return this.vrik.solver.locomotion.weight;
		}

		// Token: 0x06001C34 RID: 7220 RVA: 0x000BC14C File Offset: 0x000BA34C
		public override void SetLocomotionWeight(float weight)
		{
			this.vrik.solver.locomotion.weight = weight;
		}

		// Token: 0x06001C35 RID: 7221 RVA: 0x000BC164 File Offset: 0x000BA364
		public override void AddLocomotionDeltaPosition(Vector3 delta)
		{
			this.vrik.solver.locomotion.AddDeltaPosition(delta);
		}

		// Token: 0x06001C36 RID: 7222 RVA: 0x000BC17C File Offset: 0x000BA37C
		public override void AddLocomotionDeltaRotation(Quaternion delta, Vector3 pivot)
		{
			this.vrik.solver.locomotion.AddDeltaRotation(delta, pivot);
		}

		/// EYES
		// Token: 0x06001C37 RID: 7223 RVA: 0x000BC195 File Offset: 0x000BA395
		public override float GetLookAtWeight()
		{
			return this.bipedIk.solvers.lookAt.IKPositionWeight;
		}

		// Token: 0x06001C38 RID: 7224 RVA: 0x000BC1AC File Offset: 0x000BA3AC
		public override void SetLookAtTarget(Transform target)
		{
			if (target)
			{
				this.bipedIk.solvers.lookAt.target = target;
				this.bipedIk.solvers.lookAt.IKPositionWeight = 1f;
				this.eyesTarget = target;
			}
			else
			{
				this.bipedIk.solvers.lookAt.target = null;
				this.bipedIk.solvers.lookAt.IKPositionWeight = 0f;
				this.eyesTarget = null;
			}
			this.RefreshState();
		}

		// Token: 0x06001C39 RID: 7225 RVA: 0x000BC237 File Offset: 0x000BA437
		public override void SetLookAtWeight(float weight)
		{
			this.bipedIk.solvers.lookAt.IKPositionWeight = weight;
		}

		// Token: 0x06001C3A RID: 7226 RVA: 0x000BC24F File Offset: 0x000BA44F
		public override void SetLookAtBodyWeight(float weight, float clamp)
		{
			this.bipedIk.solvers.lookAt.bodyWeight = weight;
			this.bipedIk.solvers.lookAt.clampWeight = clamp;
		}

		// Token: 0x06001C3B RID: 7227 RVA: 0x000BC27D File Offset: 0x000BA47D
		public override void SetLookAtHeadWeight(float weight, float clamp)
		{
			this.bipedIk.solvers.lookAt.headWeight = weight;
			this.bipedIk.solvers.lookAt.clampWeightHead = clamp;
		}

		// Token: 0x06001C3C RID: 7228 RVA: 0x000BC2AB File Offset: 0x000BA4AB
		public override void SetLookAtEyesWeight(float weight, float clamp)
		{
			this.bipedIk.solvers.lookAt.eyesWeight = weight;
			this.bipedIk.solvers.lookAt.clampWeightEyes = clamp;
		}

		/// HEAD
		// Token: 0x06001C3D RID: 7229 RVA: 0x000BC2DC File Offset: 0x000BA4DC
		public override void SetHeadAnchor(Transform anchor)
		{
			if (anchor)
			{
				this.headEffector.transform.SetParent(anchor);
				this.headEffector.transform.localPosition = Vector3.zero;
				this.headEffector.transform.localRotation = Quaternion.identity;
				this.headEffector.enabled = true;
				this.vrik.solver.spine.headTarget = this.headEffector.transform;
				this.SetHeadState(true, true);
				this.SetHeadWeight(1f, 1f);
				this.headTarget = this.headEffector.transform;
			}
			else
			{
				this.headEffector.transform.SetParent(base.transform);
				this.headEffector.transform.localPosition = Vector3.zero;
				this.headEffector.transform.localRotation = Quaternion.identity;
				this.headEffector.enabled = false;
				this.vrik.solver.spine.headTarget = null;
				this.SetHeadState(false, false);
				this.SetHeadWeight(0f, 0f);
				this.headTarget = null;
			}
			this.RefreshState();
		}

		// Token: 0x06001C3E RID: 7230 RVA: 0x000BC410 File Offset: 0x000BA610
		public override void SetHeadState(bool positionEnabled, bool rotationEnabled)
		{
			this.vrik.solver.spine.positionWeight = (float)(positionEnabled ? 1 : 0);
			this.vrik.solver.spine.rotationWeight = (float)(rotationEnabled ? 1 : 0);
			this.headEffector.positionWeight = (float)(positionEnabled ? 1 : 0);
			this.headEffector.rotationWeight = (float)(rotationEnabled ? 1 : 0);
			this.headEffector.enabled = (positionEnabled || rotationEnabled);
			this.headEnabled = (positionEnabled || rotationEnabled);
			this.RefreshState();
		}

		// Token: 0x06001C3F RID: 7231 RVA: 0x000BC4A4 File Offset: 0x000BA6A4
		public override void SetHeadWeight(float positionWeight, float rotationWeight)
		{
			this.vrik.solver.spine.positionWeight = positionWeight;
			this.vrik.solver.spine.rotationWeight = rotationWeight;
			this.headEffector.positionWeight = positionWeight;
			this.headEffector.rotationWeight = rotationWeight;
			this.headEffector.bendWeight = rotationWeight * this.headTorsoBendWeightMultiplier;
		}

		// Token: 0x06001C40 RID: 7232 RVA: 0x000BC508 File Offset: 0x000BA708
		public override float GetHeadWeight()
		{
			return this.vrik.solver.spine.positionWeight;
		}

		/// HIPS
		// Token: 0x06001C41 RID: 7233 RVA: 0x000BC520 File Offset: 0x000BA720
		public override void SetHipsAnchor(Transform anchor)
		{
			if (anchor)
			{
				this.vrik.solver.spine.pelvisTarget = anchor;
				this.SetHipsState(true);
				this.SetHipsWeight(1f);
				this.hipsTarget = anchor;
			}
			else
			{
				this.vrik.solver.spine.pelvisTarget = null;
				this.SetHipsState(false);
				this.SetHipsWeight(0f);
				this.hipsTarget = null;
			}
			this.RefreshState();
		}

		// Token: 0x06001C42 RID: 7234 RVA: 0x000BC59C File Offset: 0x000BA79C
		public override void SetHipsState(bool enabled)
		{
			this.vrik.solver.spine.pelvisPositionWeight = (float)(enabled ? 1 : 0);
			this.vrik.solver.spine.pelvisRotationWeight = (float)(enabled ? 1 : 0);
			this.hipsEnabled = enabled;
			this.RefreshState();
		}

		// Token: 0x06001C43 RID: 7235 RVA: 0x000BC5F0 File Offset: 0x000BA7F0
		public override void SetHipsWeight(float value)
		{
			this.vrik.solver.spine.pelvisPositionWeight = value;
			this.vrik.solver.spine.pelvisRotationWeight = value;
		}

		// Token: 0x06001C44 RID: 7236 RVA: 0x000BC61E File Offset: 0x000BA81E
		public override float GetHipsWeight()
		{
			return this.vrik.solver.spine.pelvisPositionWeight;
		}

		/// UPPER ARM / SHOULDER
		// Token: 0x06001C45 RID: 7237 RVA: 0x000BC638 File Offset: 0x000BA838
		public override void SetShoulderAnchor(Side side, Transform anchor)
		{
			if (anchor)
			{
				if (side == Side.Left)
				{
					this.fullBodyBipedIk.solver.leftShoulderEffector.target = anchor;
					this.shoulderLeftTarget = anchor;
				}
				if (side == Side.Right)
				{
					this.fullBodyBipedIk.solver.rightShoulderEffector.target = anchor;
					this.shoulderLeftTarget = anchor;
				}
				this.SetShoulderState(side, true, true);
				this.SetShoulderWeight(side, 1f, 1f);
			}
			else
			{
				if (side == Side.Left)
				{
					this.fullBodyBipedIk.solver.leftShoulderEffector.target = null;
					this.shoulderLeftTarget = null;
				}
				if (side == Side.Right)
				{
					this.fullBodyBipedIk.solver.rightShoulderEffector.target = null;
					this.shoulderRightTarget = null;
				}
				this.SetShoulderState(side, false, false);
				this.SetShoulderWeight(side, 0f, 0f);
			}
			this.RefreshState();
		}

		// Token: 0x06001C46 RID: 7238 RVA: 0x000BC70C File Offset: 0x000BA90C
		public override void SetShoulderState(Side side, bool positionEnabled, bool rotationEnabled)
		{
			if (side == Side.Left)
			{
				this.fullBodyBipedIk.solver.leftShoulderEffector.positionWeight = (float)(positionEnabled ? 1 : 0);
				this.fullBodyBipedIk.solver.leftShoulderEffector.rotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.shoulderLeftEnabled = (positionEnabled || rotationEnabled);
			}
			if (side == Side.Right)
			{
				this.fullBodyBipedIk.solver.rightShoulderEffector.positionWeight = (float)(positionEnabled ? 1 : 0);
				this.fullBodyBipedIk.solver.rightShoulderEffector.rotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.shoulderLeftEnabled = (positionEnabled || rotationEnabled);
			}
			this.RefreshState();
		}

		// Token: 0x06001C47 RID: 7239 RVA: 0x000BC7B4 File Offset: 0x000BA9B4
		public override void SetShoulderWeight(Side side, float positionWeight, float rotationWeight)
		{
			if (side == Side.Left)
			{
				this.fullBodyBipedIk.solver.leftShoulderEffector.positionWeight = positionWeight;
				this.fullBodyBipedIk.solver.leftShoulderEffector.rotationWeight = rotationWeight;
			}
			if (side == Side.Right)
			{
				this.fullBodyBipedIk.solver.rightShoulderEffector.positionWeight = positionWeight;
				this.fullBodyBipedIk.solver.rightShoulderEffector.rotationWeight = rotationWeight;
			}
		}

		/// HANDS
		// Token: 0x06001C48 RID: 7240 RVA: 0x000BC820 File Offset: 0x000BAA20
		public override void SetHandAnchor(Side side, Transform anchor, Quaternion palmRotation)
		{
			if (anchor)
			{
				if (side == Side.Left)
				{
					this.vrik.solver.leftArm.target = anchor;
					this.bipedIk.solvers.leftHand.target = anchor;
					this.fullBodyBipedIk.solver.leftHandEffector.target = anchor;
					this.handLeftTarget = anchor;
				}
				if (side == Side.Right)
				{
					this.vrik.solver.rightArm.target = anchor;
					this.bipedIk.solvers.rightHand.target = anchor;
					this.fullBodyBipedIk.solver.rightHandEffector.target = anchor;
					this.handRightTarget = anchor;
				}
				this.SetHandState(side, true, true);
				this.SetHandWeight(side, 1f, 1f);
			}
			else
			{
				if (side == Side.Left)
				{
					this.vrik.solver.leftArm.target = null;
					this.bipedIk.solvers.leftHand.target = null;
					this.handLeftTarget = null;
				}
				if (side == Side.Right)
				{
					this.vrik.solver.rightArm.target = null;
					this.bipedIk.solvers.rightHand.target = null;
					this.handRightTarget = null;
				}
				this.SetHandState(side, false, false);
				this.SetHandWeight(side, 0f, 0f);
			}
			this.RefreshState();
		}

		// Token: 0x06001C49 RID: 7241 RVA: 0x000BC980 File Offset: 0x000BAB80
		public override void SetHandState(Side side, bool positionEnabled, bool rotationEnabled)
		{
			if (side == Side.Left)
			{
				this.vrik.solver.leftArm.positionWeight = (float)(positionEnabled ? 1 : 0);
				this.vrik.solver.leftArm.rotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.bipedIk.solvers.leftHand.IKPositionWeight = (float)(positionEnabled ? 1 : 0);
				this.bipedIk.solvers.leftHand.IKRotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.fullBodyBipedIk.solver.leftHandEffector.positionWeight = (float)(positionEnabled ? 1 : 0);
				this.fullBodyBipedIk.solver.leftHandEffector.rotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.handLeftEnabled = (positionEnabled || rotationEnabled);
			}
			if (side == Side.Right)
			{
				this.vrik.solver.rightArm.positionWeight = (float)(positionEnabled ? 1 : 0);
				this.vrik.solver.rightArm.rotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.bipedIk.solvers.rightHand.IKPositionWeight = (float)(positionEnabled ? 1 : 0);
				this.bipedIk.solvers.rightHand.IKRotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.fullBodyBipedIk.solver.rightHandEffector.positionWeight = (float)(positionEnabled ? 1 : 0);
				this.fullBodyBipedIk.solver.rightHandEffector.rotationWeight = (float)(rotationEnabled ? 1 : 0);
				this.handRightEnabled = (positionEnabled || rotationEnabled);
			}
			this.RefreshState();
		}

		// Token: 0x06001C4A RID: 7242 RVA: 0x000BCB18 File Offset: 0x000BAD18
		public override void SetHandWeight(Side side, float positionWeight, float rotationWeight)
		{
			if (side == Side.Left)
			{
				this.vrik.solver.leftArm.positionWeight = positionWeight;
				this.vrik.solver.leftArm.rotationWeight = rotationWeight;
				this.bipedIk.solvers.leftHand.IKPositionWeight = positionWeight;
				this.bipedIk.solvers.leftHand.IKRotationWeight = rotationWeight;
				this.fullBodyBipedIk.solver.leftHandEffector.positionWeight = positionWeight;
				this.fullBodyBipedIk.solver.leftHandEffector.rotationWeight = rotationWeight;
			}
			if (side == Side.Right)
			{
				this.vrik.solver.rightArm.positionWeight = positionWeight;
				this.vrik.solver.rightArm.rotationWeight = rotationWeight;
				this.bipedIk.solvers.rightHand.IKPositionWeight = positionWeight;
				this.bipedIk.solvers.rightHand.IKRotationWeight = rotationWeight;
				this.fullBodyBipedIk.solver.rightHandEffector.positionWeight = positionWeight;
				this.fullBodyBipedIk.solver.rightHandEffector.rotationWeight = rotationWeight;
			}
		}

		// Token: 0x06001C4B RID: 7243 RVA: 0x000BCC3C File Offset: 0x000BAE3C
		public override float GetHandPositionWeight(Side side)
		{
			if (this.creature.player)
			{
				if (side == Side.Left)
				{
					return this.vrik.solver.leftArm.positionWeight;
				}
				return this.vrik.solver.rightArm.positionWeight;
			}
			else
			{
				if (side == Side.Left)
				{
					return this.bipedIk.solvers.leftHand.IKPositionWeight;
				}
				return this.bipedIk.solvers.rightHand.IKPositionWeight;
			}
		}

		// Token: 0x06001C4C RID: 7244 RVA: 0x000BCCBC File Offset: 0x000BAEBC
		public override float GetHandRotationWeight(Side side)
		{
			if (this.creature.player)
			{
				if (side == Side.Left)
				{
					return this.vrik.solver.leftArm.rotationWeight;
				}
				return this.vrik.solver.rightArm.rotationWeight;
			}
			else
			{
				if (side == Side.Left)
				{
					return this.bipedIk.solvers.leftHand.IKRotationWeight;
				}
				return this.bipedIk.solvers.rightHand.IKRotationWeight;
			}
		}

		/// FOOTS
		// Token: 0x06001C4D RID: 7245 RVA: 0x000BCD3C File Offset: 0x000BAF3C
		public override void SetFootAnchor(Side side, Transform anchor, Quaternion toesRotation)
		{
			if (anchor)
			{
				if (side == Side.Left)
				{
					this.vrik.solver.leftLeg.target = anchor;
					this.bipedIk.solvers.leftFoot.target = anchor;
					this.footLeftTarget = anchor;
				}
				if (side == Side.Right)
				{
					this.vrik.solver.rightLeg.target = anchor;
					this.bipedIk.solvers.rightFoot.target = anchor;
					this.footRightTarget = anchor;
				}
				this.SetFootState(side, true);
				this.SetFootWeight(side, 1f, 1f);
			}
			else
			{
				if (side == Side.Left)
				{
					this.vrik.solver.leftLeg.target = null;
					this.bipedIk.solvers.leftFoot.target = null;
					this.footLeftTarget = null;
				}
				if (side == Side.Right)
				{
					this.vrik.solver.rightLeg.target = null;
					this.bipedIk.solvers.rightFoot.target = null;
					this.footRightTarget = null;
				}
				this.SetFootState(side, false);
				this.SetFootWeight(side, 0f, 0f);
			}
			this.RefreshState();
		}

		// Token: 0x06001C4E RID: 7246 RVA: 0x000BCE6B File Offset: 0x000BB06B
		public override void SetFootPull(Side side, float value)
		{
		}

		// Token: 0x06001C4F RID: 7247 RVA: 0x000BCE70 File Offset: 0x000BB070
		public override void SetFootState(Side side, bool active)
		{
			if (side == Side.Left)
			{
				this.vrik.solver.leftLeg.positionWeight = (float)(active ? 1 : 0);
				this.vrik.solver.leftLeg.rotationWeight = (float)(active ? 1 : 0);
				this.bipedIk.solvers.leftFoot.IKPositionWeight = (float)(active ? 1 : 0);
				this.bipedIk.solvers.leftFoot.IKRotationWeight = (float)(active ? 1 : 0);
				this.footLeftEnabled = active;
			}
			if (side == Side.Right)
			{
				this.vrik.solver.rightLeg.positionWeight = (float)(active ? 1 : 0);
				this.vrik.solver.rightLeg.rotationWeight = (float)(active ? 1 : 0);
				this.bipedIk.solvers.rightFoot.IKPositionWeight = (float)(active ? 1 : 0);
				this.bipedIk.solvers.rightFoot.IKRotationWeight = (float)(active ? 1 : 0);
				this.footRightEnabled = active;
			}
			this.RefreshState();
		}

		// Token: 0x06001C50 RID: 7248 RVA: 0x000BCF80 File Offset: 0x000BB180
		public override void SetFootWeight(Side side, float positionWeight, float rotationWeight)
		{
			if (side == Side.Left)
			{
				this.vrik.solver.leftLeg.positionWeight = positionWeight;
				this.vrik.solver.leftLeg.rotationWeight = rotationWeight;
				this.bipedIk.solvers.leftFoot.IKPositionWeight = positionWeight;
				this.bipedIk.solvers.leftFoot.IKRotationWeight = rotationWeight;
			}
			if (side == Side.Right)
			{
				this.vrik.solver.rightLeg.positionWeight = positionWeight;
				this.vrik.solver.rightLeg.rotationWeight = rotationWeight;
				this.bipedIk.solvers.rightFoot.IKPositionWeight = positionWeight;
				this.bipedIk.solvers.rightFoot.IKRotationWeight = rotationWeight;
			}
		}

		// Token: 0x06001C51 RID: 7249 RVA: 0x000BD044 File Offset: 0x000BB244
		public override void SetKneeAnchor(Side side, Transform anchor)
		{
			if (anchor)
			{
				if (side == Side.Left)
				{
					this.vrik.solver.leftLeg.bendGoal = anchor;
					this.bipedIk.solvers.leftFoot.bendGoal = anchor;
					this.kneeLeftHint = anchor;
				}
				if (side == Side.Right)
				{
					this.vrik.solver.rightLeg.bendGoal = anchor;
					this.bipedIk.solvers.rightFoot.bendGoal = anchor;
					this.kneeRightHint = anchor;
				}
			}
			else
			{
				if (side == Side.Left)
				{
					this.vrik.solver.leftLeg.bendGoal = null;
					this.bipedIk.solvers.leftFoot.bendGoal = null;
					this.kneeLeftHint = null;
				}
				if (side == Side.Right)
				{
					this.vrik.solver.rightLeg.bendGoal = null;
					this.bipedIk.solvers.rightFoot.bendGoal = null;
					this.kneeRightHint = null;
				}
			}
			this.RefreshState();
		}

		// Token: 0x06001C52 RID: 7250 RVA: 0x000BD140 File Offset: 0x000BB340
		public override void SetKneeWeight(Side side, float weight)
		{
			if (side == Side.Left)
			{
				this.vrik.solver.leftLeg.bendToTargetWeight = weight;
				this.bipedIk.solvers.leftFoot.bendModifierWeight = weight;
			}
			if (side == Side.Right)
			{
				this.vrik.solver.rightLeg.bendToTargetWeight = weight;
				this.bipedIk.solvers.rightFoot.bendModifierWeight = weight;
			}
		}

		// Token: 0x06001C53 RID: 7251 RVA: 0x000BD1AC File Offset: 0x000BB3AC
		public override IkController.FootBoneTarget GetFootBoneTarget()
		{
			return IkController.FootBoneTarget.Toes;
		}

		// Token: 0x04001AF3 RID: 6899
		[Header("Final IK")]
		public AnimationCurve stretchArmsCurve;

		// Token: 0x04001AF4 RID: 6900
		public AnimationCurve stretchLegsCurve;

		// Token: 0x04001AF5 RID: 6901
		public float headTorsoBendWeightMultiplier = 0.5f;

		// Token: 0x04001AFA RID: 6906
		protected HandPoser handPoserLeft;

		// Token: 0x04001AFB RID: 6907
		protected HandPoser handPoserRight;

		// Token: 0x04001AFC RID: 6908
		protected FingerRig leftFingerRig;

		// Token: 0x04001AFD RID: 6909
		protected FingerRig rightFingerRig;

		// Token: 0x020008DF RID: 2271
		public enum State
		{
			// Token: 0x040042FA RID: 17146
			Disabled,
			// Token: 0x040042FB RID: 17147
			Default,
			// Token: 0x040042FC RID: 17148
			FullBody,
			// Token: 0x040042FD RID: 17149
			Player
		}
	}
}
