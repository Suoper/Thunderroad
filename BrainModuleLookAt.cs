using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000160 RID: 352
	public class BrainModuleLookAt : BrainData.Module
	{
		// Token: 0x17000108 RID: 264
		// (get) Token: 0x060010D3 RID: 4307 RVA: 0x00076B54 File Offset: 0x00074D54
		protected bool bodyUpright
		{
			get
			{
				return this.currentBodyBehaviour == BrainModuleLookAt.BodyBehaviour.BodyUpright && base.creature.state == Creature.State.Alive && !base.creature.isPlayingDynamicAnimation && !base.creature.brain.isDodging && !base.creature.brain.isAttacking;
			}
		}

		// Token: 0x17000109 RID: 265
		// (get) Token: 0x060010D4 RID: 4308 RVA: 0x00076BAC File Offset: 0x00074DAC
		protected Transform targetTransform
		{
			get
			{
				if (this._targetTransform == null)
				{
					this._targetTransform = base.creature.transform.FindOrAddTransform("LookTarget", base.creature.centerEyes.position + base.creature.transform.forward, null, null);
				}
				return this._targetTransform;
			}
		}

		// Token: 0x1700010A RID: 266
		// (get) Token: 0x060010D5 RID: 4309 RVA: 0x00076C20 File Offset: 0x00074E20
		protected Transform lookAtTransform
		{
			get
			{
				if (this._lookAtTransform == null)
				{
					this._lookAtTransform = base.creature.transform.FindOrAddTransform("LookAt", base.creature.centerEyes.position + base.creature.transform.forward, null, null);
				}
				return this._lookAtTransform;
			}
		}

		// Token: 0x1700010B RID: 267
		// (get) Token: 0x060010D6 RID: 4310 RVA: 0x00076C93 File Offset: 0x00074E93
		protected Transform referenceTransform
		{
			get
			{
				if (!this.headPart.gameObject.activeInHierarchy || base.creature.isPlayer)
				{
					return this.headBoneFixed;
				}
				return this.headPartFixed;
			}
		}

		// Token: 0x1700010C RID: 268
		// (get) Token: 0x060010D7 RID: 4311 RVA: 0x00076CC1 File Offset: 0x00074EC1
		protected Transform headPart
		{
			get
			{
				Creature creature = base.creature;
				if (creature == null)
				{
					return null;
				}
				return creature.ragdoll.headPart.transform;
			}
		}

		// Token: 0x060010D8 RID: 4312 RVA: 0x00076CE0 File Offset: 0x00074EE0
		public override void Load(Creature creature)
		{
			base.Load(creature);
			this.actionLock = false;
			IKControllerFIK ikController = (IKControllerFIK)creature.ragdoll.ik;
			this.bipedIK = ikController.bipedIk;
			this.torso.solverWeightSetter = delegate(float input)
			{
				this.bipedIK.solvers.lookAt.bodyWeight = input;
			};
			this.lookIKWeights.Add(this.torso);
			this.head.solverWeightSetter = delegate(float input)
			{
				this.bipedIK.solvers.lookAt.headWeight = input;
			};
			this.lookIKWeights.Add(this.head);
			this.eyes.solverWeightSetter = delegate(float input)
			{
				this.bipedIK.solvers.lookAt.eyesWeight = input;
			};
			this.lookIKWeights.Add(this.eyes);
			this.spine.solverWeightSetter = delegate(float input)
			{
				this.bipedIK.solvers.spine.SetIKPositionWeight(input);
			};
			this.spineTransform = creature.transform.FindOrAddTransform("SpineCorrector", creature.transform.TransformPoint(new Vector3(0f, creature.morphology.height * 1.5f, 0f)), null, null);
			this.headBoneFixed = creature.ragdoll.headPart.bone.mesh.FindOrAddTransform("ForwardUpOrient", creature.ragdoll.headPart.bone.mesh.position, new Quaternion?(Quaternion.LookRotation(creature.ragdoll.headPart.forwardDirection, creature.ragdoll.headPart.upDirection)), null);
			this.headPartFixed = this.headPart.FindOrAddTransform("ForwardUpOrient", this.headPart.position, new Quaternion?(Quaternion.LookRotation(creature.ragdoll.headPart.forwardDirection, creature.ragdoll.headPart.upDirection)), null);
			this.SuscribeEvents();
			ikController.OnPostIKUpdateEvent += this.PostIKUpdate;
			if (this.customEyeTargetOnly)
			{
				this.customLookEyes = new BrainModuleLookAt.CustomLookEye[creature.allEyes.Count];
				for (int i = 0; i < creature.allEyes.Count; i++)
				{
					this.customLookEyes[i] = BrainModuleLookAt.ConfigureEye(this.referenceTransform, creature.allEyes[i].transform.parent.FindChildRecursive("ForwardTransform").parent, i.ToString());
				}
				this.currentEyesMode = BrainModuleLookAt.EyesMode.CustomPostIK;
				this.StopLookAt(true);
				this.setupNeeded = false;
				return;
			}
			this.setupNeeded = true;
		}

		// Token: 0x060010D9 RID: 4313 RVA: 0x00076F70 File Offset: 0x00075170
		public override void OnBrainStop()
		{
			base.OnBrainStop();
			this.StopLookAt(false);
		}

		// Token: 0x060010DA RID: 4314 RVA: 0x00076F7F File Offset: 0x0007517F
		private void LevelUnload(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			this.Unload();
		}

		// Token: 0x060010DB RID: 4315 RVA: 0x00076F87 File Offset: 0x00075187
		public override void Unload()
		{
			base.Unload();
			this.StopLookAt(true);
			this.UnsubscribeEvents(true);
		}

		// Token: 0x060010DC RID: 4316 RVA: 0x00076FA0 File Offset: 0x000751A0
		public static BrainModuleLookAt.CustomLookEye ConfigureEye(Transform referenceTransform, Transform eyeTransform, string name)
		{
			BrainModuleLookAt.CustomLookEye result = new BrainModuleLookAt.CustomLookEye
			{
				eyeBone = eyeTransform,
				eyeForward = eyeTransform.Find("ForwardTransform")
			};
			result.aligner = result.eyeBone.parent.FindOrAddTransform("Eye" + name + "Aligner", result.eyeForward ? result.eyeForward.position : result.eyeBone.position, new Quaternion?(referenceTransform.rotation), null);
			if (!result.eyeForward)
			{
				result.eyeForward = new GameObject("ForwardTransform").transform;
				result.eyeForward.position = result.eyeBone.position;
				result.eyeForward.parent = result.eyeBone;
				result.eyeForward.rotation = result.aligner.rotation;
			}
			result.alignerStart = Matrix4x4.TRS(result.aligner.localPosition, result.aligner.localRotation, result.aligner.localScale);
			result.boneStart = Matrix4x4.TRS(result.eyeBone.localPosition, result.eyeBone.localRotation, result.eyeBone.localScale);
			result.forwardStart = Matrix4x4.TRS(result.eyeForward.localPosition, result.eyeForward.localRotation, result.eyeForward.localScale);
			result.forwardEyeParentStart = result.eyeBone.InverseTransformPoint(result.eyeForward.position);
			return result;
		}

		// Token: 0x060010DD RID: 4317 RVA: 0x00077134 File Offset: 0x00075334
		private IEnumerator SetupCustomTargeting(Action callback = null)
		{
			if (!this.setupNeeded)
			{
				yield break;
			}
			this.setupNeeded = false;
			this.customLookEyes = new BrainModuleLookAt.CustomLookEye[this.bipedIK.solvers.lookAt.eyes.Length];
			this.bipedIK.enabled = true;
			this.SetLookAt(this.referenceTransform, new Vector3(0f, 0f, 5f), BrainModuleLookAt.BodyBehaviour.None, 0f, true, null);
			yield return Yielders.EndOfFrame;
			for (int i = 0; i < this.bipedIK.solvers.lookAt.eyes.Length; i++)
			{
				IKSolverLookAt.LookAtBone eyeBone = this.bipedIK.solvers.lookAt.eyes[i];
				this.customLookEyes[i] = BrainModuleLookAt.ConfigureEye(this.referenceTransform, eyeBone.transform, i.ToString());
			}
			this.currentEyesMode = (this.forceAimPostIK ? BrainModuleLookAt.EyesMode.CustomPostIK : BrainModuleLookAt.EyesMode.CustomLateUpdate);
			this.StopLookAt(true);
			if (callback != null)
			{
				callback();
			}
			yield break;
		}

		// Token: 0x060010DE RID: 4318 RVA: 0x0007714A File Offset: 0x0007534A
		private void CreatureSpawned(Creature creature)
		{
			if (creature == base.creature)
			{
				this.StopLookAt(true);
				this.actionLock = false;
			}
		}

		// Token: 0x060010DF RID: 4319 RVA: 0x00077168 File Offset: 0x00075368
		private void CreatureKilled(CollisionInstance collisionInstance, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				if (!base.creature.gameObject.activeInHierarchy)
				{
					return;
				}
				base.creature.StartCoroutine(this.CreatureDeath());
				base.creature.OnResurrectEvent += this.CreatureRevived;
			}
		}

		// Token: 0x060010E0 RID: 4320 RVA: 0x000771B4 File Offset: 0x000753B4
		private void CreatureRevived(float newHealth, Creature resurrector, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				return;
			}
			base.creature.OnResurrectEvent -= this.CreatureRevived;
			this.actionLock = false;
			this.StopLookAt(true);
			this.SuscribeEvents();
		}

		// Token: 0x060010E1 RID: 4321 RVA: 0x000771E5 File Offset: 0x000753E5
		private IEnumerator CreatureDeath()
		{
			this.actionLock = false;
			this.StopLookAt(false);
			yield return Yielders.EndOfFrame;
			this.actionLock = false;
			this.SetLookAt(this.headPart, this.eyesDeathPosition, BrainModuleLookAt.BodyBehaviour.None, this.deathEyeRollDuration, true, delegate()
			{
				this.actionLock = true;
				base.creature.OnDespawnEvent += this.CreatureDespawn;
				EventManager.onLevelUnload += this.LevelUnload;
			});
			yield break;
		}

		// Token: 0x060010E2 RID: 4322 RVA: 0x000771F4 File Offset: 0x000753F4
		private void CreatureDespawn(EventTime eventTime)
		{
			if (this.customLookEyes == null)
			{
				return;
			}
			for (int i = 0; i < this.customLookEyes.Length; i++)
			{
				if (this.customLookEyes[i].aligner != null)
				{
					if (this.customLookEyes[i].eyeBone != null)
					{
						this.customLookEyes[i].aligner.parent = this.customLookEyes[i].eyeBone.parent;
					}
					this.customLookEyes[i].aligner.localPosition = this.customLookEyes[i].alignerStart.GetPosition();
					this.customLookEyes[i].aligner.localRotation = this.customLookEyes[i].alignerStart.GetRotation();
					this.customLookEyes[i].aligner.localScale = this.customLookEyes[i].alignerStart.GetScale();
				}
				if (this.customLookEyes[i].eyeBone != null)
				{
					this.customLookEyes[i].eyeBone.localPosition = this.customLookEyes[i].boneStart.GetPosition();
					this.customLookEyes[i].eyeBone.localRotation = this.customLookEyes[i].boneStart.GetRotation();
					this.customLookEyes[i].eyeBone.localScale = this.customLookEyes[i].boneStart.GetScale();
				}
				if (this.customLookEyes[i].eyeForward != null)
				{
					this.customLookEyes[i].eyeForward.parent = this.customLookEyes[i].eyeBone;
					this.customLookEyes[i].eyeForward.localPosition = this.customLookEyes[i].forwardStart.GetPosition();
					this.customLookEyes[i].eyeForward.localRotation = this.customLookEyes[i].forwardStart.GetRotation();
					this.customLookEyes[i].eyeForward.localScale = this.customLookEyes[i].forwardStart.GetScale();
				}
			}
			this.UnsubscribeEvents(true);
		}

		// Token: 0x060010E3 RID: 4323 RVA: 0x00077478 File Offset: 0x00075678
		protected void SuscribeEvents()
		{
			this.UnsubscribeEvents(false);
			EventManager.onCreatureSpawn += this.CreatureSpawned;
			base.creature.OnKillEvent += this.CreatureKilled;
			base.creature.OnDespawnEvent += this.CreatureDespawn;
			EventManager.onLevelUnload += this.LevelUnload;
		}

		// Token: 0x060010E4 RID: 4324 RVA: 0x000774DC File Offset: 0x000756DC
		protected void UnsubscribeEvents(bool despawnUnload = false)
		{
			EventManager.onCreatureSpawn -= this.CreatureSpawned;
			base.creature.OnKillEvent -= this.CreatureKilled;
			base.creature.OnResurrectEvent -= this.CreatureRevived;
			base.creature.OnDespawnEvent -= this.CreatureDespawn;
			EventManager.onLevelUnload -= this.LevelUnload;
			if (despawnUnload)
			{
				Creature creature = base.creature;
				UnityEngine.Object x;
				if (creature == null)
				{
					x = null;
				}
				else
				{
					Ragdoll ragdoll = creature.ragdoll;
					x = ((ragdoll != null) ? ragdoll.ik : null);
				}
				if (x != null)
				{
					((IKControllerFIK)base.creature.ragdoll.ik).OnPostIKUpdateEvent -= this.PostIKUpdate;
				}
			}
		}

		// Token: 0x060010E5 RID: 4325 RVA: 0x000775A0 File Offset: 0x000757A0
		protected void AlignAllEyesCustom()
		{
			if (this.customLookEyes.IsNullOrEmpty() || !base.creature.gameObject.activeInHierarchy)
			{
				return;
			}
			for (int i = 0; i < this.customLookEyes.Length; i++)
			{
				BrainModuleLookAt.CustomLookEye customEye = this.customLookEyes[i];
				customEye.aligner.localPosition = customEye.forwardEyeParentStart;
				customEye.aligner.LookAt(this.lookAtTransform, this.referenceTransform.up);
				Vector3 startLocalEulers = this.referenceTransform.InverseTransformRotation(customEye.aligner.rotation).eulerAngles;
				customEye.aligner.SetEulersLocalPseudoParent(this.referenceTransform, new Vector3(Mathf.Clamp(startLocalEulers.x + ((startLocalEulers.x > 180f) ? -360f : 0f), base.creature.data.eyeVerticalAngleClamps.x, base.creature.data.eyeVerticalAngleClamps.y), Mathf.Clamp(startLocalEulers.y + ((startLocalEulers.y > 180f) ? -360f : 0f), base.creature.data.eyeHorizontalAngleClamps.x, base.creature.data.eyeHorizontalAngleClamps.y), startLocalEulers.z));
				if (customEye.aligner.parent != customEye.eyeBone.parent)
				{
					customEye.aligner.parent = customEye.eyeBone.parent;
				}
				customEye.aligner.localPosition = customEye.forwardEyeParentStart;
				customEye.eyeBone.MoveAlign(customEye.eyeForward, customEye.aligner, null);
			}
		}

		// Token: 0x060010E6 RID: 4326 RVA: 0x00077758 File Offset: 0x00075958
		public override void FixedUpdate()
		{
			if (!this.customEyeTargetOnly)
			{
				this.bipedIK.enabled = this.updateIK;
			}
			if (base.creature.brain.isUnconscious)
			{
				this.StopLookAt(false);
			}
			this.CheckLookAtParentMatchesReference();
			this.SetIKTargetWeights();
			this.UpdateIK();
			this.CheckAngleLimits();
		}

		// Token: 0x060010E7 RID: 4327 RVA: 0x000777B0 File Offset: 0x000759B0
		protected virtual void CheckLookAtParentMatchesReference()
		{
			if ((this.lookAtTransform.parent == this.headPartFixed || this.lookAtTransform.parent == this.headBoneFixed) && this.lookAtTransform.parent != this.referenceTransform)
			{
				this.lookAtTransform.parent = this.referenceTransform;
			}
		}

		// Token: 0x060010E8 RID: 4328 RVA: 0x00077818 File Offset: 0x00075A18
		protected virtual void SetIKTargetWeights()
		{
			if (!this.isLooking)
			{
				return;
			}
			if (base.creature.state != Creature.State.Alive || base.creature.ragdoll.standingUp || base.creature.ragdoll.handlers.Count > 0)
			{
				this.torso.targetWeight = 0f;
				this.torso.SetCurrentWeight(0f);
				this.head.targetWeight = 0f;
				this.head.SetCurrentWeight(0f);
				this.spine.targetWeight = 0f;
				this.spine.SetCurrentWeight(0f);
			}
			else
			{
				bool atSpeed = this.uprightWhileMoving && base.creature.locomotion.physicBody.velocity.sqrMagnitude > this.forceUprightSpeed * this.forceUprightSpeed;
				this.spine.targetWeight = ((this.bodyUpright || atSpeed) ? 1f : 0f);
				this.head.targetWeight = 1f;
				this.torso.targetWeight = ((this.currentBodyBehaviour == BrainModuleLookAt.BodyBehaviour.UseLookIK && !atSpeed) ? 1f : 0f);
			}
			if (base.creature.state != Creature.State.Dead)
			{
				this.eyes.targetWeight = ((this.currentEyesMode == BrainModuleLookAt.EyesMode.UseLookIK) ? 1f : 0f);
			}
			this.updateIK = true;
		}

		// Token: 0x060010E9 RID: 4329 RVA: 0x0007798C File Offset: 0x00075B8C
		protected void UpdateIK()
		{
			if (!this.updateIK || this.customEyeTargetOnly)
			{
				return;
			}
			base.creature.ragdoll.ik.SetLookAtWeight(1f);
			float total = 0f;
			int count = this.lookIKWeights.Count;
			float fixedDeltaTime = Time.fixedDeltaTime;
			for (int i = 0; i < count; i++)
			{
				BrainModuleLookAt.IKWeight lookIKWeight = this.lookIKWeights[i];
				lookIKWeight.SetCurrentWeight(Mathf.MoveTowards(lookIKWeight.currentWeight, Mathf.Clamp(lookIKWeight.targetWeight, 0f, lookIKWeight.clampWeight), fixedDeltaTime * this.weightChangeSpeed));
				total += lookIKWeight.currentWeight;
			}
			this.spine.SetCurrentWeight(Mathf.SmoothDamp(this.spine.currentWeight, Mathf.Clamp(this.bodyUpright ? this.spine.targetWeight : 0f, 0f, this.spine.clampWeight), ref this.spineSpeed, this.spineChangeSmooth, this.spineChangeMaxSpeed));
			total += this.spine.currentWeight;
			if (Mathf.Approximately(total, 0f) && this.currentLookTarget == null && !this.isLooking)
			{
				this.updateIK = false;
			}
		}

		// Token: 0x060010EA RID: 4330 RVA: 0x00077AC8 File Offset: 0x00075CC8
		protected virtual void CheckAngleLimits()
		{
			if (!this.isLooking || base.creature.state == Creature.State.Dead)
			{
				return;
			}
			if (Vector3.Angle(this.referenceTransform.forward, this.targetTransform.position - base.creature.centerEyes.position) > this.lookMaxAngle)
			{
				this.lookAtTransform.parent = this.referenceTransform;
			}
			else if (this.lookAtTransform.parent == this.referenceTransform)
			{
				this.SetLookAt(this.targetTransform.parent, this.targetTransform.localPosition, this.currentBodyBehaviour, -1f, true, null);
			}
			if (this.lastLookingPosition != null)
			{
				Vector3 centerEyesPosition = base.creature.centerEyes.position;
				if (Vector3.Angle(this.lookAtTransform.position - centerEyesPosition, this.lastLookingPosition.Value - centerEyesPosition) > this.snapPreventionAngle)
				{
					this.SetLookAt(this.targetTransform.parent, this.targetTransform.localPosition, this.currentBodyBehaviour, -1f, true, null);
					return;
				}
				this.lastLookingPosition = new Vector3?(this.lookAtTransform.position);
			}
		}

		// Token: 0x060010EB RID: 4331 RVA: 0x00077C07 File Offset: 0x00075E07
		public override void LateUpdate()
		{
			base.LateUpdate();
			if (!base.creature.brain.instance.isActive)
			{
				this.UpdateIK();
			}
			if (this.currentEyesMode != BrainModuleLookAt.EyesMode.CustomLateUpdate)
			{
				return;
			}
			this.AlignAllEyesCustom();
		}

		// Token: 0x060010EC RID: 4332 RVA: 0x00077C3C File Offset: 0x00075E3C
		protected virtual void PostIKUpdate()
		{
			if (this.currentEyesMode != BrainModuleLookAt.EyesMode.CustomPostIK)
			{
				return;
			}
			this.AlignAllEyesCustom();
		}

		// Token: 0x060010ED RID: 4333 RVA: 0x00077C4E File Offset: 0x00075E4E
		protected IEnumerator PullLookToTarget(Transform endParent, float blendTime = -1f, AnimationCurve blendCurve = null)
		{
			this.lastLookingPosition = null;
			float blendDuration = (blendTime < 0f) ? this.defaultBlendTime : blendTime;
			AnimationCurve blendingCurve = (blendCurve != null) ? blendCurve : this.defaultBlendCurve;
			float blendStart = Time.time;
			this.lookAtTransform.parent = null;
			Vector3 startLocalVector = this.referenceTransform.InverseTransformPoint(this.lookAtTransform.position);
			if (blendDuration > 0f)
			{
				while (Time.time - blendStart <= blendDuration)
				{
					float curveTime = blendingCurve.Evaluate(Mathf.Clamp01((Time.time - blendStart) / blendDuration));
					this.lookAtTransform.position = Vector3.Lerp(this.referenceTransform.TransformPoint(startLocalVector), this.targetTransform.position, curveTime);
					yield return Yielders.EndOfFrame;
				}
			}
			this.lookAtTransform.position = this.targetTransform.position;
			this.lookAtTransform.parent = endParent;
			this.lastLookingPosition = new Vector3?(this.lookAtTransform.position);
			this.moveLookAtCoroutine = null;
			yield break;
		}

		// Token: 0x060010EE RID: 4334 RVA: 0x00077C74 File Offset: 0x00075E74
		public void SetLookAt(Vector3 targetPosition, Vector3 localOffset = default(Vector3), BrainModuleLookAt.BodyBehaviour bodyBehaviour = BrainModuleLookAt.BodyBehaviour.BodyUpright, float duration = -1f, bool force = false)
		{
			if (base.creature.state != Creature.State.Alive)
			{
				return;
			}
			if (this.actionLock)
			{
				return;
			}
			this.lookAtTransform.parent = null;
			this.targetTransform.position = targetPosition;
			this.currentLookTarget = null;
			this.SetLookAt(this.targetTransform, localOffset, bodyBehaviour, duration, force, null);
		}

		// Token: 0x060010EF RID: 4335 RVA: 0x00077CCC File Offset: 0x00075ECC
		public void SetLookAt(Transform target, Vector3 localOffset = default(Vector3), BrainModuleLookAt.BodyBehaviour bodyBehaviour = BrainModuleLookAt.BodyBehaviour.BodyUpright, float duration = -1f, bool force = false, Action callback = null)
		{
			if (this.setupNeeded)
			{
				base.creature.StartCoroutine(this.SetupCustomTargeting(delegate
				{
					this.SetLookAt(target, localOffset, bodyBehaviour, duration, force, null);
				}));
				return;
			}
			if (this.lookAtTransform == null || this.targetTransform == null)
			{
				return;
			}
			if (target == base.creature.centerEyes)
			{
				Debug.Log(base.creature.name + " tried to look at its own center eyes. This has been ignored. Check the stack trace to find the cause of this.");
				return;
			}
			if (this.actionLock)
			{
				return;
			}
			this.isLooking = true;
			base.creature.ragdoll.ik.SetLookAtTarget(this.lookAtTransform);
			this.bipedIK.solvers.spine.target = this.spineTransform;
			if (force || bodyBehaviour != this.currentBodyBehaviour)
			{
				this.currentBodyBehaviour = bodyBehaviour;
				bool atSpeed = base.creature.locomotion.physicBody.velocity.sqrMagnitude > this.forceUprightSpeed * this.forceUprightSpeed;
				this.spine.targetWeight = ((!this.customEyeTargetOnly && base.creature.state == Creature.State.Alive && (this.bodyUpright || atSpeed)) ? 1f : 0f);
				this.torso.targetWeight = ((!this.customEyeTargetOnly && base.creature.state == Creature.State.Alive && this.currentBodyBehaviour == BrainModuleLookAt.BodyBehaviour.UseLookIK && !atSpeed) ? 1f : 0f);
				this.head.targetWeight = ((!this.customEyeTargetOnly && base.creature.state == Creature.State.Alive && !base.creature.ragdoll.standingUp && base.creature.ragdoll.handlers.Count == 0) ? 1f : 0f);
				this.eyes.targetWeight = ((this.currentEyesMode == BrainModuleLookAt.EyesMode.UseLookIK) ? 1f : 0f);
			}
			if (force || this.currentLookTarget != target)
			{
				this.lastLookingPosition = null;
				this.lookAtTransform.parent = null;
				this.targetTransform.parent = null;
				if (target != this.targetTransform)
				{
					this.targetTransform.parent = target;
					this.targetTransform.localPosition = localOffset;
				}
				this.currentLookTarget = target;
				if (this.moveLookAtCoroutine != null)
				{
					base.creature.StopCoroutine(this.moveLookAtCoroutine);
					this.moveLookAtCoroutine = null;
				}
				if (!base.creature.gameObject.activeInHierarchy)
				{
					return;
				}
				this.moveLookAtCoroutine = base.creature.StartCoroutine(this.PullLookToTarget(this.targetTransform, duration, null));
			}
			if (callback != null)
			{
				callback();
			}
		}

		// Token: 0x060010F0 RID: 4336 RVA: 0x00077FE4 File Offset: 0x000761E4
		public void StopLookAt(bool instant = false)
		{
			if (this.actionLock)
			{
				return;
			}
			if (!this.isLooking)
			{
				return;
			}
			this.SetLookAt(this.referenceTransform, new Vector3(0f, 0f, 5f), BrainModuleLookAt.BodyBehaviour.None, instant ? 0f : -1f, true, null);
			this.isLooking = false;
		}

		// Token: 0x060010F1 RID: 4337 RVA: 0x0007803C File Offset: 0x0007623C
		public override void OnDrawGizmos()
		{
			base.OnDrawGizmos();
			Gizmos.color = Color.green;
			Gizmos.DrawLine(base.creature.centerEyes.position, this.lookAtTransform.position);
			Gizmos.color = Color.yellow;
			Gizmos.DrawLine(this.lookAtTransform.position, this.targetTransform.position);
			if (this.currentEyesMode == BrainModuleLookAt.EyesMode.CustomLateUpdate || this.currentEyesMode == BrainModuleLookAt.EyesMode.CustomPostIK)
			{
				for (int i = 0; i < this.customLookEyes.Length; i++)
				{
					BrainModuleLookAt.CustomLookEye customEye = this.customLookEyes[i];
					Gizmos.color = Color.grey;
					Gizmos.DrawLine(customEye.aligner.position, customEye.aligner.position + customEye.aligner.forward * Vector3.Distance(customEye.aligner.position, this.lookAtTransform.position));
					Gizmos.color = Color.white;
					Gizmos.DrawLine(customEye.eyeForward.position, customEye.eyeForward.position + customEye.eyeForward.forward * Vector3.Distance(customEye.eyeForward.position, this.lookAtTransform.position));
					Gizmos.color = Color.cyan;
					Gizmos.DrawLine(customEye.eyeForward.position, this.lookAtTransform.position);
				}
			}
		}

		// Token: 0x04000EDD RID: 3805
		[Header("Blending")]
		public float weightChangeSpeed = 2f;

		// Token: 0x04000EDE RID: 3806
		public float defaultBlendTime = 0.5f;

		// Token: 0x04000EDF RID: 3807
		public AnimationCurve defaultBlendCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x04000EE0 RID: 3808
		public float snapPreventionAngle = 30f;

		// Token: 0x04000EE1 RID: 3809
		public float forceUprightSpeed = 1f;

		// Token: 0x04000EE2 RID: 3810
		public float spineChangeMaxSpeed = 2f;

		// Token: 0x04000EE3 RID: 3811
		public float spineChangeSmooth = 0.1f;

		// Token: 0x04000EE4 RID: 3812
		[Header("Controls")]
		public bool customEyeTargetOnly;

		// Token: 0x04000EE5 RID: 3813
		public float lookMaxAngle = 80f;

		// Token: 0x04000EE6 RID: 3814
		[Header("IK")]
		public bool forceAimPostIK = true;

		// Token: 0x04000EE7 RID: 3815
		public bool uprightWhileMoving = true;

		// Token: 0x04000EE8 RID: 3816
		public BrainModuleLookAt.IKWeight torso = new BrainModuleLookAt.IKWeight
		{
			currentWeight = 0f,
			clampWeight = 0.7f,
			targetWeight = 0f
		};

		// Token: 0x04000EE9 RID: 3817
		public BrainModuleLookAt.IKWeight head = new BrainModuleLookAt.IKWeight
		{
			currentWeight = 0f,
			clampWeight = 0.75f,
			targetWeight = 0f
		};

		// Token: 0x04000EEA RID: 3818
		public BrainModuleLookAt.IKWeight eyes = new BrainModuleLookAt.IKWeight
		{
			currentWeight = 0f,
			clampWeight = 0.95f,
			targetWeight = 0f
		};

		// Token: 0x04000EEB RID: 3819
		public BrainModuleLookAt.IKWeight spine = new BrainModuleLookAt.IKWeight
		{
			currentWeight = 0f,
			clampWeight = 0.9f,
			targetWeight = 0f
		};

		// Token: 0x04000EEC RID: 3820
		[Header("Death")]
		public Vector3 eyesDeathPosition = new Vector3(-3f, 0f, 3f);

		// Token: 0x04000EED RID: 3821
		public float deathEyeRollDuration = 5f;

		// Token: 0x04000EEE RID: 3822
		[NonSerialized]
		public bool isLooking;

		// Token: 0x04000EEF RID: 3823
		[NonSerialized]
		public bool updateIK;

		// Token: 0x04000EF0 RID: 3824
		[NonSerialized]
		public bool actionLock;

		// Token: 0x04000EF1 RID: 3825
		[NonSerialized]
		public Transform currentLookTarget;

		// Token: 0x04000EF2 RID: 3826
		[NonSerialized]
		public BrainModuleLookAt.EyesMode currentEyesMode;

		// Token: 0x04000EF3 RID: 3827
		[NonSerialized]
		public BrainModuleLookAt.BodyBehaviour currentBodyBehaviour = BrainModuleLookAt.BodyBehaviour.BodyUpright;

		// Token: 0x04000EF4 RID: 3828
		protected float spineSpeed;

		// Token: 0x04000EF5 RID: 3829
		protected List<BrainModuleLookAt.IKWeight> lookIKWeights = new List<BrainModuleLookAt.IKWeight>();

		// Token: 0x04000EF6 RID: 3830
		private Transform _targetTransform;

		// Token: 0x04000EF7 RID: 3831
		private Transform _lookAtTransform;

		// Token: 0x04000EF8 RID: 3832
		protected Transform spineTransform;

		// Token: 0x04000EF9 RID: 3833
		protected BipedIKCustom bipedIK;

		// Token: 0x04000EFA RID: 3834
		protected Coroutine moveLookAtCoroutine;

		// Token: 0x04000EFB RID: 3835
		protected Vector3? lastLookingPosition;

		// Token: 0x04000EFC RID: 3836
		protected Transform headBoneFixed;

		// Token: 0x04000EFD RID: 3837
		protected Transform headPartFixed;

		// Token: 0x04000EFE RID: 3838
		protected bool setupNeeded = true;

		// Token: 0x04000EFF RID: 3839
		protected BrainModuleLookAt.CustomLookEye[] customLookEyes;

		// Token: 0x02000727 RID: 1831
		[Serializable]
		public class IKWeight
		{
			// Token: 0x06003BD9 RID: 15321 RVA: 0x0017A374 File Offset: 0x00178574
			public void SetCurrentWeight(float weight)
			{
				this.currentWeight = weight;
				this.solverWeightSetter(weight);
			}

			// Token: 0x04003C00 RID: 15360
			[Range(0f, 1f)]
			[NonSerialized]
			public float currentWeight;

			// Token: 0x04003C01 RID: 15361
			[Range(0f, 1f)]
			public float clampWeight;

			// Token: 0x04003C02 RID: 15362
			[Range(0f, 1f)]
			public float targetWeight;

			// Token: 0x04003C03 RID: 15363
			[NonSerialized]
			public Action<float> solverWeightSetter;
		}

		// Token: 0x02000728 RID: 1832
		public enum BodyBehaviour
		{
			// Token: 0x04003C05 RID: 15365
			UseLookIK,
			// Token: 0x04003C06 RID: 15366
			BodyUpright,
			// Token: 0x04003C07 RID: 15367
			None
		}

		// Token: 0x02000729 RID: 1833
		public struct CustomLookEye
		{
			// Token: 0x04003C08 RID: 15368
			public Transform aligner;

			// Token: 0x04003C09 RID: 15369
			public Transform eyeBone;

			// Token: 0x04003C0A RID: 15370
			public Transform eyeForward;

			// Token: 0x04003C0B RID: 15371
			public Matrix4x4 alignerStart;

			// Token: 0x04003C0C RID: 15372
			public Matrix4x4 boneStart;

			// Token: 0x04003C0D RID: 15373
			public Matrix4x4 forwardStart;

			// Token: 0x04003C0E RID: 15374
			public Vector3 forwardEyeParentStart;
		}

		// Token: 0x0200072A RID: 1834
		public enum EyesMode
		{
			// Token: 0x04003C10 RID: 15376
			UseLookIK,
			// Token: 0x04003C11 RID: 15377
			CustomLateUpdate,
			// Token: 0x04003C12 RID: 15378
			CustomPostIK,
			// Token: 0x04003C13 RID: 15379
			None
		}
	}
}
