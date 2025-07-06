using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002C1 RID: 705
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/HingeDrive.html")]
	public class HingeDrive : ThunderBehaviour
	{
		// Token: 0x17000218 RID: 536
		// (get) Token: 0x06002219 RID: 8729 RVA: 0x000EB213 File Offset: 0x000E9413
		public float HingeAngle
		{
			get
			{
				return this.GetCurrentAngle();
			}
		}

		// Token: 0x0600221A RID: 8730 RVA: 0x000EB21C File Offset: 0x000E941C
		private float GetCurrentAngle()
		{
			if (!this.init)
			{
				return this.defaultAngle;
			}
			if (this.hingesFlatArray.Length == 0)
			{
				return this.defaultAngle;
			}
			if (this.hingesFlatArray.Length != 1)
			{
				float sum = 0f;
				foreach (HingeDrive.HingeMetaData item in this.hingesFlatArray)
				{
					if (!(item.joint == null))
					{
						sum += item.joint.angle;
					}
				}
				return sum / (float)this.hingesFlatArray.Length;
			}
			if (!(this.hingesFlatArray[0].joint != null))
			{
				return 0f;
			}
			return this.hingesFlatArray[0].joint.angle;
		}

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x0600221B RID: 8731 RVA: 0x000EB2D3 File Offset: 0x000E94D3
		public float HingeVelocity
		{
			get
			{
				return this.GetCurrentVelocity();
			}
		}

		// Token: 0x0600221C RID: 8732 RVA: 0x000EB2DC File Offset: 0x000E94DC
		private float GetCurrentVelocity()
		{
			if (!this.init)
			{
				return 0f;
			}
			if (this.hingesFlatArray.Length == 0)
			{
				return 0f;
			}
			if (this.hingesFlatArray.Length == 1)
			{
				return this.hingesFlatArray[0].joint.velocity;
			}
			float sum = 0f;
			foreach (HingeDrive.HingeMetaData item in this.hingesFlatArray)
			{
				sum += item.joint.velocity;
			}
			return sum / (float)this.hingesFlatArray.Length;
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x0600221D RID: 8733 RVA: 0x000EB364 File Offset: 0x000E9564
		public HingeDrive.HingeDriveSpeedState SpeedState
		{
			get
			{
				return this.GetSpeedState();
			}
		}

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x0600221E RID: 8734 RVA: 0x000EB36C File Offset: 0x000E956C
		// (set) Token: 0x0600221F RID: 8735 RVA: 0x000EB374 File Offset: 0x000E9574
		public HingeDrive.HingeDriveState currentState { get; protected set; } = HingeDrive.HingeDriveState.Unlocked;

		// Token: 0x06002220 RID: 8736 RVA: 0x000EB380 File Offset: 0x000E9580
		public void CollisionOnHingeHolderEnter(Collision collision)
		{
			float impulse = collision.impulse.magnitude;
			if (this.currentState != HingeDrive.HingeDriveState.LatchLocked)
			{
				return;
			}
			if (collision.impulse.magnitude > this.forceLatchOpeningImpulseThreshold)
			{
				this.TryBruteForceLatch();
			}
			if (this.isLatchBreakable)
			{
				this.latchHealth -= impulse;
			}
			if (this.latchHealth <= 0f)
			{
				this.BreakLatch();
			}
		}

		// Token: 0x06002221 RID: 8737 RVA: 0x000EB3EA File Offset: 0x000E95EA
		public void TryBruteForceLatch()
		{
			if (this.isLatchBruteForceable)
			{
				this.ReleaseLatch();
			}
		}

		// Token: 0x06002222 RID: 8738 RVA: 0x000EB3FA File Offset: 0x000E95FA
		public void ReleaseLatch()
		{
			if (this.currentState != HingeDrive.HingeDriveState.LatchLocked || !this.useLatch)
			{
				return;
			}
			this.lastUnlockTime = Time.time;
			this.onLatchUnlock.Invoke(this.HingeVelocity, this.SpeedState);
			this.currentState = HingeDrive.HingeDriveState.Unlocked;
		}

		/// <summary>
		/// Activate a motor on the joint to close it with default parameters specified in editor.
		/// </summary>
		// Token: 0x06002223 RID: 8739 RVA: 0x000EB436 File Offset: 0x000E9636
		public void AutoClose()
		{
			this.AutoClose(this.autoOpenFallBackVelocity, this.autoOpenFallBackForce);
		}

		/// <summary>
		/// Activate a motor on the joint to open it to minimal angle with default parameters specified in editor.
		/// </summary>
		// Token: 0x06002224 RID: 8740 RVA: 0x000EB44A File Offset: 0x000E964A
		public void AutoOpenMin()
		{
			this.AutoOpenMin(this.autoOpenFallBackVelocity, this.autoOpenFallBackForce, this.autoOpenBypassLatch);
		}

		/// <summary>
		/// Activate a motor on the joint to open it to maximal angle with default parameters specified in editor.
		/// </summary>
		// Token: 0x06002225 RID: 8741 RVA: 0x000EB464 File Offset: 0x000E9664
		public void AutoOpenMax()
		{
			this.AutoOpenMax(this.autoOpenFallBackVelocity, this.autoOpenFallBackForce, this.autoOpenBypassLatch);
		}

		/// <summary>
		/// Activate a motor on the joint to open it to some desired angle in degrees
		/// with default parameters specified in editor.
		/// </summary>
		/// <param name="targetAngle">Desired angle for the door to open to in degrees.</param>
		// Token: 0x06002226 RID: 8742 RVA: 0x000EB47E File Offset: 0x000E967E
		public void AutoRotateTo(float targetAngle)
		{
			this.AutoRotateTo(targetAngle, this.autoOpenFallBackVelocity, this.autoOpenFallBackForce, this.autoOpenBypassLatch);
		}

		// Token: 0x06002227 RID: 8743 RVA: 0x000EB49C File Offset: 0x000E969C
		private void StopAutoRotate()
		{
			if (this.autoRotateRoutine == null)
			{
				return;
			}
			base.StopCoroutine(this.autoRotateRoutine);
			foreach (KeyValuePair<HingeJoint, HingeDrive.HingeMetaData> hingePair in this.hinges)
			{
				this.StopMotor(hingePair.Value.joint);
			}
			this.autoRotateRoutine = null;
		}

		/// <summary>
		/// Prevent opening this hinge drive
		/// </summary>
		// Token: 0x06002228 RID: 8744 RVA: 0x000EB518 File Offset: 0x000E9718
		public void PreventOpening()
		{
			this.cachedAllowedInputs = this.allowedInputsToOpenLatch;
			this.isLatchBruteForceable = false;
			this.allowedInputsToOpenLatch = HingeDrive.InputType.None;
		}

		/// <summary>
		/// Allow opening this hinge drive with restored previous allowed inputs
		/// </summary>
		// Token: 0x06002229 RID: 8745 RVA: 0x000EB534 File Offset: 0x000E9734
		public void AllowOpening()
		{
			this.isLatchBruteForceable = true;
			this.allowedInputsToOpenLatch = this.cachedAllowedInputs;
		}

		// Token: 0x0600222A RID: 8746 RVA: 0x000EB549 File Offset: 0x000E9749
		private void Awake()
		{
			this.Init();
		}

		// Token: 0x0600222B RID: 8747 RVA: 0x000EB551 File Offset: 0x000E9751
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			if (this.hingesHolder != null && this.hingesHolder.gameObject != base.gameObject)
			{
				this.hingesHolder.gameObject.SetActive(true);
			}
		}

		// Token: 0x0600222C RID: 8748 RVA: 0x000EB590 File Offset: 0x000E9790
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			if (this.hingesHolder != null && this.hingesHolder.gameObject != base.gameObject)
			{
				this.hingesHolder.gameObject.SetActive(false);
			}
		}

		// Token: 0x0600222D RID: 8749 RVA: 0x000EB5D0 File Offset: 0x000E97D0
		public void Init()
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			int k = (int)this.rotationAxis;
			vector[k] = 1f;
			this.rotationAxisVector3 = vector;
			if (this.frame == null)
			{
				this.frame = base.gameObject.GetComponent<Rigidbody>();
				if (this.frame == null)
				{
					this.frame = base.gameObject.AddComponent<Rigidbody>();
				}
			}
			this.frameRigidbody = this.frame;
			this.hingeRigidbody = this.hingesHolder.GetComponent<Rigidbody>();
			this.closedHingeDefaultRotation = this.hingeRigidbody.rotation;
			bool useGravity = this.hingeRigidbody.useGravity;
			this.hingeRigidbody.useGravity = false;
			this.hinges = new Dictionary<HingeJoint, HingeDrive.HingeMetaData>();
			foreach (Transform hingeHolder in this.hingesTargets)
			{
				HingeJoint hinge;
				this.ConfigureHinge(hingeHolder, out hinge);
				HingeDrive.HingeMetaData hingeMetaData = new HingeDrive.HingeMetaData(hinge);
				this.hinges.Add(hinge, hingeMetaData);
			}
			this.hingesFlatArray = this.hinges.Values.ToArray<HingeDrive.HingeMetaData>();
			this.hingesHolder.rotation *= Quaternion.AngleAxis(this.defaultAngle, this.rotationAxisVector3);
			if (this.handles != null)
			{
				this.playerHands = new Dictionary<Handle, PlayerHand>();
				this.latchButtonStatePerHandle = new Dictionary<Handle, bool>();
				for (int i = 0; i < this.handles.Length; i++)
				{
					Handle handle = this.handles[i];
					if (!this.IsHandleLatchLock(handle))
					{
						handle.Grabbed += this.OnHandleGrab;
						handle.UnGrabbed += this.OnHandleUnGrab;
						this.playerHands.Add(handle, null);
						this.latchButtonStatePerHandle.Add(handle, false);
					}
				}
			}
			if (this.latchAngles == null)
			{
				this.latchAngles = Array.Empty<HingeDrive.AngleThreshold>();
			}
			this.latchAngles = (from a in this.latchAngles
			orderby a.center
			select a).ToArray<HingeDrive.AngleThreshold>();
			this.smoothingVelocityBuffer = new float[this.smoothingSamples];
			if (this.effectAudioHingeMovingPositive)
			{
				this.onHingeMove.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState, float, float>(this.HandleCreakingSound));
			}
			if (this.effectAudioLatchLock)
			{
				this.onLatchLock.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState>(this.HandleLatchLockSound));
			}
			if (this.effectAudioLatchUnlock)
			{
				this.onLatchUnlock.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState>(this.HandleLatchUnlockSound));
			}
			if (this.effectAudioLatchBreak)
			{
				this.onLatchBreak.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState>(this.HandleLatchBreakSound));
			}
			if (this.effectAudioWiggle)
			{
				this.onHingeHitThreshold.AddListener(new UnityAction<bool, float, HingeDrive.HingeDriveSpeedState>(this.HandleHingeHitThresholdSound));
			}
			if (this.effectAudioLatchButtonPress)
			{
				this.onPlayerPressingLatchButton.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState, Handle>(this.HandleLatchButtonPressingSound));
			}
			if (this.effectAudioLatchButtonRelease)
			{
				this.onPlayerReleasingLatchButton.AddListener(new UnityAction<float, HingeDrive.HingeDriveSpeedState, Handle>(this.HandleLatchButtonReleasingSound));
			}
			this.hingeRigidbody.useGravity = useGravity;
			this.init = true;
			if (this.collidersToIgnore != null)
			{
				for (int j = 0; j < this.collidersToIgnore.Length; j++)
				{
					this.IgnoreCollision(this.collidersToIgnore[j], true, true);
				}
			}
			if (AreaManager.Instance && AreaManager.Instance.CurrentArea != null)
			{
				this.currentArea = AreaManager.Instance.CurrentArea.FindRecursive(base.transform.position);
			}
		}

		/// <summary>
		/// Ignores collision with the given collider.
		/// </summary>
		/// <param name="colliderToIgnore">Collider to ignore</param>
		/// <param name="withFrame">Should we ignore the collision with the frame?</param>
		/// <param name="withHingeHolder">Should we ignore the collision with the hinge holder?</param>
		// Token: 0x0600222E RID: 8750 RVA: 0x000EB978 File Offset: 0x000E9B78
		public void IgnoreCollision(Collider colliderToIgnore, bool withFrame = true, bool withHingeHolder = true)
		{
			if (withFrame)
			{
				Collider[] frameColliders = this.frame.GetComponentsInChildren<Collider>();
				for (int i = 0; i < frameColliders.Length; i++)
				{
					Physics.IgnoreCollision(colliderToIgnore, frameColliders[i]);
				}
			}
			if (withHingeHolder)
			{
				Collider[] hingeColliders = this.hingesHolder.GetComponentsInChildren<Collider>();
				for (int j = 0; j < hingeColliders.Length; j++)
				{
					Physics.IgnoreCollision(colliderToIgnore, hingeColliders[j]);
				}
			}
		}

		// Token: 0x0600222F RID: 8751 RVA: 0x000EB9D1 File Offset: 0x000E9BD1
		private void Start()
		{
			if (this.useLatch)
			{
				this.CheckForLatches();
			}
		}

		// Token: 0x06002230 RID: 8752 RVA: 0x000EB9E4 File Offset: 0x000E9BE4
		private void OnDestroy()
		{
			if (this.handles == null)
			{
				return;
			}
			foreach (Handle handle in this.handles)
			{
				handle.Grabbed -= this.OnHandleGrab;
				handle.UnGrabbed -= this.OnHandleUnGrab;
			}
		}

		/// <summary>
		/// Check if the handle latch is locked
		/// </summary>
		/// <param name="handle">The handle you want to know if its locked</param>
		/// <returns>True if handle latch is Lock</returns>
		// Token: 0x06002231 RID: 8753 RVA: 0x000EBA38 File Offset: 0x000E9C38
		public bool IsHandleLatchLock(Handle handle)
		{
			if (this.handleLatchLock != null)
			{
				for (int i = this.handleLatchLock.Count - 1; i >= 0; i--)
				{
					if (handle == this.handleLatchLock[i])
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Lock handle latch so it wont open the door using the handle
		/// </summary>
		/// <param name="handle">The handle you want to be lock</param>
		// Token: 0x06002232 RID: 8754 RVA: 0x000EBA7C File Offset: 0x000E9C7C
		public void LockHandleLatch(Handle handle)
		{
			if (this.handleLatchLock == null)
			{
				this.handleLatchLock = new List<Handle>();
			}
			bool isHandleValid = false;
			for (int i = 0; i < this.handles.Length; i++)
			{
				if (handle == this.handles[i])
				{
					isHandleValid = true;
					break;
				}
			}
			if (isHandleValid && !this.IsHandleLatchLock(handle))
			{
				handle.Grabbed -= this.OnHandleGrab;
				handle.UnGrabbed -= this.OnHandleUnGrab;
				this.handleLatchLock.Add(handle);
				this.playerHands.Remove(handle);
				this.latchButtonStatePerHandle.Remove(handle);
			}
		}

		/// <summary>
		/// unlock handle latch so it wont open the door using the handle
		/// </summary>
		/// <param name="handle">The handle you want to be unlock</param>
		// Token: 0x06002233 RID: 8755 RVA: 0x000EBB1C File Offset: 0x000E9D1C
		public void UnLockhandleLatch(Handle handle)
		{
			if (this.IsHandleLatchLock(handle))
			{
				handle.Grabbed += this.OnHandleGrab;
				handle.UnGrabbed += this.OnHandleUnGrab;
				this.handleLatchLock.Remove(handle);
				this.playerHands.Add(handle, null);
				this.latchButtonStatePerHandle.Add(handle, false);
			}
		}

		// Token: 0x06002234 RID: 8756 RVA: 0x000EBB80 File Offset: 0x000E9D80
		private HingeDrive.HingeDriveSpeedState GetSpeedState()
		{
			float velocity = Mathf.Abs(this.HingeVelocity);
			if (velocity <= 0.01f)
			{
				return HingeDrive.HingeDriveSpeedState.NotMoving;
			}
			if (velocity <= 30f)
			{
				return HingeDrive.HingeDriveSpeedState.Slow;
			}
			if (velocity <= 50f)
			{
				return HingeDrive.HingeDriveSpeedState.Fast;
			}
			return HingeDrive.HingeDriveSpeedState.ReallyFast;
		}

		// Token: 0x06002235 RID: 8757 RVA: 0x000EBBB8 File Offset: 0x000E9DB8
		private void OnHandleGrab(RagdollHand ragdollHand, Handle handle1, EventTime eventTime)
		{
			if (!ragdollHand.playerHand)
			{
				return;
			}
			this.lastGrabTime = Time.time;
			this.playerHands[handle1] = ragdollHand.playerHand;
			this.isGrabbed = true;
		}

		// Token: 0x06002236 RID: 8758 RVA: 0x000EBBEC File Offset: 0x000E9DEC
		private void OnHandleUnGrab(RagdollHand ragdollHand, Handle handle1, EventTime eventTime)
		{
			this.previousAngle = this.HingeAngle;
			this.playerHands[handle1] = null;
			this.isGrabbed = false;
		}

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x06002237 RID: 8759 RVA: 0x000EBC0E File Offset: 0x000E9E0E
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update;
			}
		}

		// Token: 0x06002238 RID: 8760 RVA: 0x000EBC14 File Offset: 0x000E9E14
		protected internal override void ManagedFixedUpdate()
		{
			if (!this.init)
			{
				return;
			}
			if (this.hingesHolder == null || !this.hingesHolder.gameObject.activeInHierarchy)
			{
				return;
			}
			float hingeAngle = this.HingeAngle;
			if (this.playOnceOnOpenLatchEffect != null && !this.hasPlayedOpenFX && Mathf.Sqrt(Mathf.Pow(this.defaultAngle - hingeAngle, 2f)) >= this.angleThresholdForOpenFX)
			{
				this.OpenFXTimer += Time.deltaTime;
				if (this.OpenFXTimer > 0.25f)
				{
					this.playOnceOnOpenLatchEffect.Play();
					this.hasPlayedOpenFX = true;
				}
			}
			if (hingeAngle <= this.currentMinAngle * 0.99f || hingeAngle >= this.currentMaxAngle * 0.99f)
			{
				if (!this.hasHingeHitThreshold && Time.time - this.lastHitThresholdTime > 0.25f)
				{
					if (Time.time - this.lastHitThresholdTime < 0.25f)
					{
						this.previousAngle = hingeAngle;
						return;
					}
					this.lastHitThresholdTime = Time.time;
					this.hasHingeHitThreshold = true;
					this.onHingeHitThreshold.Invoke(hingeAngle <= this.currentMinAngle * 0.99f, this.HingeVelocity, this.SpeedState);
				}
			}
			else
			{
				this.hasHingeHitThreshold = false;
			}
			if (this.currentState == HingeDrive.HingeDriveState.LatchLocked)
			{
				if (Math.Abs(this.currentMinAngle - this.currentLockedLatchAngle.center) <= 0.001f)
				{
					goto IL_21C;
				}
				using (Dictionary<HingeJoint, HingeDrive.HingeMetaData>.Enumerator enumerator = this.hinges.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<HingeJoint, HingeDrive.HingeMetaData> pair = enumerator.Current;
						this.UpdateLimits(pair.Value.joint, this.currentLockedLatchAngle.Min, this.currentLockedLatchAngle.Max, true);
					}
					goto IL_21C;
				}
			}
			if (Math.Abs(this.currentMinAngle - this.minAngle) > 0.001f)
			{
				foreach (KeyValuePair<HingeJoint, HingeDrive.HingeMetaData> pair2 in this.hinges)
				{
					this.UpdateLimits(pair2.Value.joint, this.minAngle, this.maxAngle, this.isLimited);
				}
			}
			IL_21C:
			if ((!this.autoCloses && Math.Abs(this.HingeVelocity) <= 0.001f) || !this.useLatch || this.currentState == HingeDrive.HingeDriveState.LatchLocked)
			{
				this.previousAngle = hingeAngle;
				return;
			}
			if (Time.time - this.lastGrabTime > 0.07f && !this.anyAllowedLatchUnlockButtonPressed)
			{
				this.CheckForLatches();
			}
			this.previousAngle = hingeAngle;
		}

		// Token: 0x06002239 RID: 8761 RVA: 0x000EBEB8 File Offset: 0x000EA0B8
		private void CheckForLatches()
		{
			if (Mathf.Abs(this.previousAngle - this.HingeAngle) > 20f)
			{
				return;
			}
			if (this.allowedInputsToOpenLatch != HingeDrive.InputType.None || (this.allowedInputsToOpenLatch == HingeDrive.InputType.None && !this.isGrabbed))
			{
				for (int i = 0; i < this.latchAngles.Length; i++)
				{
					ref HingeDrive.AngleThreshold ptr = this.latchAngles[i];
					float a = this.HingeAngle;
					float latchAngle = ptr.center;
					bool hasCrossedThreshold = (a < latchAngle && this.previousAngle > latchAngle) || (a > latchAngle && this.previousAngle < latchAngle);
					if (Math.Abs(a - latchAngle) < 0.025f || hasCrossedThreshold)
					{
						this.LatchLock(i);
					}
				}
			}
			this.previousAngle = this.HingeAngle;
		}

		// Token: 0x0600223A RID: 8762 RVA: 0x000EBF6C File Offset: 0x000EA16C
		private void LatchLock(int latchAngleIndex)
		{
			if (this.currentState == HingeDrive.HingeDriveState.LatchLocked || !this.useLatch)
			{
				return;
			}
			if (Time.time - this.lastUnlockTime < 0.25f)
			{
				return;
			}
			this.currentLockedLatchAngle = this.latchAngles[latchAngleIndex];
			this.onLatchLock.Invoke(this.HingeVelocity, this.SpeedState);
			this.hingeRigidbody.angularVelocity = Vector3.zero;
			this.hingeRigidbody.velocity = Vector3.zero;
			this.hingeRigidbody.rotation = this.closedHingeDefaultRotation * Quaternion.AngleAxis(this.currentLockedLatchAngle.center, this.rotationAxisVector3);
			this.currentState = HingeDrive.HingeDriveState.LatchLocked;
		}

		// Token: 0x0600223B RID: 8763 RVA: 0x000EC01C File Offset: 0x000EA21C
		protected internal override void ManagedUpdate()
		{
			if (!this.init)
			{
				return;
			}
			if (this.hingesHolder == null || !this.hingesHolder.gameObject.activeInHierarchy)
			{
				return;
			}
			this.ListenForInputs();
			float angle = this.HingeAngle;
			this.onHingeMove.Invoke(this.HingeVelocity, this.SpeedState, angle, Mathf.InverseLerp(this.currentMinAngle, this.currentMaxAngle, angle));
		}

		/// <summary>
		/// Activate a motor on the joint to open it.
		/// </summary>
		/// <param name="angle01">Desired angle between 0 (min angle), and 1 (max angle).</param>
		/// <param name="targetVelocity">Desired velocity for the motor.</param>
		/// <param name="force">Desired force for the motor.</param>
		/// <param name="bypassLatch">Force open the latch (or not) if encountered.</param>
		// Token: 0x0600223C RID: 8764 RVA: 0x000EC08C File Offset: 0x000EA28C
		public void AutoOpenTo01(float angle01, float targetVelocity, float force, bool bypassLatch = false)
		{
			this.StopAutoRotate();
			float t = Mathf.Clamp01(angle01);
			float min = bypassLatch ? this.minAngle : this.currentMinAngle;
			float max = bypassLatch ? this.maxAngle : this.currentMaxAngle;
			this.autoRotateRoutine = base.StartCoroutine(this.AutoRotateToRoutine(Mathf.Lerp(min, max, t), targetVelocity, force, bypassLatch));
		}

		/// <summary>
		/// Activate a motor on the joint to open it with default parameters specified in editor.
		/// </summary>
		/// <param name="angle01">Desired angle between 0 (min angle), and 1 (max angle).</param>
		// Token: 0x0600223D RID: 8765 RVA: 0x000EC0EA File Offset: 0x000EA2EA
		public void AutoOpenTo01(float angle01)
		{
			this.AutoOpenTo01(angle01, this.autoOpenFallBackVelocity, this.autoOpenFallBackForce, this.autoOpenBypassLatch);
		}

		/// <summary>
		/// Activate a motor on the joint to close it, finding nearest latch.
		/// </summary>
		/// <param name="targetVelocity">Desired velocity for the motor.</param>
		/// <param name="force">Desired force for the motor.</param>
		// Token: 0x0600223E RID: 8766 RVA: 0x000EC108 File Offset: 0x000EA308
		public void AutoClose(float targetVelocity, float force)
		{
			float closingAngle = this.defaultAngle;
			if (this.autoCloses)
			{
				closingAngle = this.restingAngle;
			}
			if (this.useLatch)
			{
				closingAngle = this.GetClosestLatchAngle();
			}
			this.StopAutoRotate();
			this.autoRotateRoutine = base.StartCoroutine(this.AutoRotateToRoutine(closingAngle, targetVelocity, force, false));
		}

		/// <summary>
		/// Finds the nearest latch angle from the current hinge angle.
		/// </summary>
		// Token: 0x0600223F RID: 8767 RVA: 0x000EC158 File Offset: 0x000EA358
		public float GetClosestLatchAngle()
		{
			float currentAngle = this.HingeAngle;
			int i = this.latchAngles.Length;
			if (currentAngle <= this.latchAngles[0].center)
			{
				return this.latchAngles[0].center;
			}
			if (currentAngle >= this.latchAngles[i - 1].center)
			{
				return this.latchAngles[i - 1].center;
			}
			int j = 0;
			int k = i;
			int mid = 0;
			while (j < k)
			{
				mid = (j + k) / 2;
				if (Math.Abs(this.latchAngles[mid].center - currentAngle) < 0.001f)
				{
					return this.latchAngles[mid].center;
				}
				if (currentAngle < this.latchAngles[mid].center)
				{
					if (mid > 0 && currentAngle > this.latchAngles[mid - 1].center)
					{
						return this.GetClosestValue(this.latchAngles[mid - 1].center, this.latchAngles[mid].center, currentAngle);
					}
					k = mid;
				}
				else
				{
					if (mid < i - 1 && currentAngle < this.latchAngles[mid + 1].center)
					{
						return this.GetClosestValue(this.latchAngles[mid].center, this.latchAngles[mid + 1].center, currentAngle);
					}
					j = mid + 1;
				}
			}
			return this.latchAngles[mid].center;
		}

		// Token: 0x06002240 RID: 8768 RVA: 0x000EC2D9 File Offset: 0x000EA4D9
		private float GetClosestValue(float v1, float v2, float target)
		{
			if (target - v1 < v2 - target)
			{
				return v1;
			}
			return v2;
		}

		/// <summary>
		/// Activate a motor on the joint to open it to minimal angle.
		/// </summary>
		/// <param name="targetVelocity">Desired velocity for the motor.</param>
		/// <param name="force">Desired force for the motor.</param>
		/// <param name="bypassLatch">Force open the latch (or not) if encountered.</param>
		// Token: 0x06002241 RID: 8769 RVA: 0x000EC2E6 File Offset: 0x000EA4E6
		public void AutoOpenMin(float targetVelocity, float force, bool bypassLatch = false)
		{
			this.StopAutoRotate();
			this.autoRotateRoutine = base.StartCoroutine(this.AutoRotateToRoutine(this.minAngle, targetVelocity, force, bypassLatch));
		}

		/// <summary>
		/// Activate a motor on the joint to open it to maximal angle.
		/// </summary>
		/// <param name="targetVelocity">Desired velocity for the motor.</param>
		/// <param name="force">Desired force for the motor.</param>
		/// <param name="bypassLatch">Force open the latch (or not) if encountered.</param>
		// Token: 0x06002242 RID: 8770 RVA: 0x000EC309 File Offset: 0x000EA509
		public void AutoOpenMax(float targetVelocity, float force, bool bypassLatch = false)
		{
			this.StopAutoRotate();
			this.autoRotateRoutine = base.StartCoroutine(this.AutoRotateToRoutine(this.maxAngle, targetVelocity, force, bypassLatch));
		}

		/// <summary>
		/// Activate a motor on the joint to open it to some desired angle in degrees.
		/// </summary>
		/// <param name="targetAngle">Desired angle for the door to open to in degrees.</param>
		/// <param name="targetVelocity">Desired velocity for the motor.</param>
		/// <param name="force">Desired force for the motor.</param>
		/// <param name="bypassLatch">Force open the latch (or not) if encountered.</param>
		// Token: 0x06002243 RID: 8771 RVA: 0x000EC32C File Offset: 0x000EA52C
		public void AutoRotateTo(float targetAngle, float targetVelocity, float force, bool bypassLatch = false)
		{
			this.StopAutoRotate();
			this.autoRotateRoutine = base.StartCoroutine(this.AutoRotateToRoutine(targetAngle, targetVelocity, force, bypassLatch));
		}

		// Token: 0x06002244 RID: 8772 RVA: 0x000EC34B File Offset: 0x000EA54B
		private IEnumerator AutoRotateToRoutine(float targetAngle, float targetVelocity, float force, bool bypassLatch = false)
		{
			float startAngle = this.HingeAngle;
			float direction = Mathf.Sign(targetAngle - startAngle);
			targetVelocity = Mathf.Sign(targetAngle - startAngle) * Mathf.Abs(targetVelocity);
			using (Dictionary<HingeJoint, HingeDrive.HingeMetaData>.Enumerator enumerator = this.hinges.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					KeyValuePair<HingeJoint, HingeDrive.HingeMetaData> hingePair = enumerator.Current;
					this.UpdateMotor(targetVelocity, force, hingePair.Value.joint, true, false);
				}
				goto IL_DF;
			}
			IL_AA:
			if (bypassLatch && this.useLatch && this.currentState == HingeDrive.HingeDriveState.LatchLocked)
			{
				this.ReleaseLatch();
			}
			yield return null;
			IL_DF:
			if ((direction <= 0f || this.HingeAngle >= targetAngle) && (direction >= 0f || this.HingeAngle <= targetAngle))
			{
				foreach (KeyValuePair<HingeJoint, HingeDrive.HingeMetaData> hingePair2 in this.hinges)
				{
					this.StopMotor(hingePair2.Value.joint);
				}
				this.autoRotateRoutine = null;
				yield break;
			}
			goto IL_AA;
		}

		// Token: 0x06002245 RID: 8773 RVA: 0x000EC377 File Offset: 0x000EA577
		private void StopMotor(HingeJoint hinge)
		{
			this.UpdateMotor(0f, 0f, hinge, false, false);
		}

		// Token: 0x06002246 RID: 8774 RVA: 0x000EC38C File Offset: 0x000EA58C
		private void UpdateMotor(float targetVelocity, float force, HingeJoint hinge, bool useMotor = true, bool fromInit = false)
		{
			JointMotor motor = hinge.motor;
			motor.force = force;
			motor.targetVelocity = targetVelocity;
			motor.freeSpin = true;
			hinge.motor = motor;
			hinge.useMotor = useMotor;
			if (!fromInit)
			{
				this.UpdateLimits(hinge, this.currentMinAngle, this.currentMaxAngle, this.isLimited);
				this.UpdateSpring(hinge);
			}
		}

		// Token: 0x06002247 RID: 8775 RVA: 0x000EC3EC File Offset: 0x000EA5EC
		private bool MakeSound()
		{
			if (Level.current && !Level.current.loaded)
			{
				return false;
			}
			if (this.currentArea == null)
			{
				if (!AreaManager.Instance || AreaManager.Instance.CurrentArea == null)
				{
					return true;
				}
				this.currentArea = AreaManager.Instance.CurrentArea.FindRecursive(base.transform.position);
				if (this.currentArea == null)
				{
					return true;
				}
			}
			if (!this.currentArea.IsSpawned)
			{
				return false;
			}
			Area area = this.currentArea.SpawnedArea;
			return area.initialized && !area.isCulled && !area.isHidden;
		}

		// Token: 0x06002248 RID: 8776 RVA: 0x000EC49C File Offset: 0x000EA69C
		private void HandleLatchLockSound(float arg0, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState)
		{
			if (!this.MakeSound())
			{
				return;
			}
			if (hingeDriveSpeedState == HingeDrive.HingeDriveSpeedState.ReallyFast && !this.effectAudioSlam.IsPlaying())
			{
				this.effectAudioSlam.SetIntensity(Mathf.Abs(this.HingeVelocity) / 20f);
				this.effectAudioSlam.Play();
			}
			if (!this.effectAudioLatchLock.IsPlaying())
			{
				this.effectAudioLatchLock.SetIntensity(Mathf.Abs(this.HingeVelocity) / 20f);
				this.effectAudioLatchLock.Play();
			}
		}

		// Token: 0x06002249 RID: 8777 RVA: 0x000EC51E File Offset: 0x000EA71E
		private void HandleLatchUnlockSound(float arg0, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState)
		{
			if (!this.MakeSound())
			{
				return;
			}
			if (!this.effectAudioLatchUnlock.IsPlaying())
			{
				this.effectAudioLatchUnlock.Play();
			}
		}

		// Token: 0x0600224A RID: 8778 RVA: 0x000EC541 File Offset: 0x000EA741
		private void HandleLatchBreakSound(float arg0, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState)
		{
			if (!this.MakeSound())
			{
				return;
			}
			if (!this.effectAudioLatchBreak.IsPlaying())
			{
				this.effectAudioLatchBreak.Play();
			}
		}

		// Token: 0x0600224B RID: 8779 RVA: 0x000EC564 File Offset: 0x000EA764
		private void HandleLatchButtonPressingSound(float arg0, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState, Handle handle)
		{
			if (!this.MakeSound())
			{
				return;
			}
			if (!this.effectAudioLatchButtonPress.IsPlaying())
			{
				this.effectAudioLatchButtonPress.Play();
			}
		}

		// Token: 0x0600224C RID: 8780 RVA: 0x000EC587 File Offset: 0x000EA787
		private void HandleLatchButtonReleasingSound(float arg0, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState, Handle handle)
		{
			if (!this.MakeSound())
			{
				return;
			}
			if (!this.effectAudioLatchButtonRelease.IsPlaying())
			{
				this.effectAudioLatchButtonRelease.Play();
			}
		}

		// Token: 0x0600224D RID: 8781 RVA: 0x000EC5AA File Offset: 0x000EA7AA
		private void HandleHingeHitThresholdSound(bool isMinAngle, float velocity, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState)
		{
			if (!this.MakeSound())
			{
				return;
			}
			velocity = Mathf.Abs(velocity);
			this.effectAudioWiggle.SetIntensity(velocity / 20f);
			this.effectAudioWiggle.Play();
		}

		// Token: 0x0600224E RID: 8782 RVA: 0x000EC5DC File Offset: 0x000EA7DC
		private void HandleCreakingSound(float velocity, HingeDrive.HingeDriveSpeedState hingeDriveSpeedState, float currentAngle, float currentAngle01)
		{
			if (!this.MakeSound())
			{
				return;
			}
			if (this.smoothingVelocityBuffer.Length != this.smoothingSamples)
			{
				Array.Resize<float>(ref this.smoothingVelocityBuffer, this.smoothingSamples);
				this.sampleIndex = 0;
			}
			float avg = velocity;
			if (this.smoothingVelocityBuffer.Length != 0)
			{
				this.smoothingVelocityBuffer[this.sampleIndex] = velocity / 20f;
				this.sampleIndex = (this.sampleIndex + 1) % this.smoothingVelocityBuffer.Length;
				float sum = 0f;
				for (int i = 0; i < this.smoothingVelocityBuffer.Length; i++)
				{
					sum += this.smoothingVelocityBuffer[i];
				}
				avg = sum / (float)this.smoothingVelocityBuffer.Length;
			}
			if (avg < 0.01f)
			{
				return;
			}
			if (Mathf.Sign(avg) > 0f)
			{
				if (this.effectAudioHingeMovingNegative.IsPlaying())
				{
					this.effectAudioHingeMovingNegative.Stop(true);
				}
				if (!this.effectAudioHingeMovingPositive.IsPlaying())
				{
					this.effectAudioHingeMovingPositive.Play();
				}
			}
			else
			{
				if (this.effectAudioHingeMovingPositive.IsPlaying())
				{
					this.effectAudioHingeMovingPositive.Stop(true);
				}
				if (!this.effectAudioHingeMovingNegative.IsPlaying())
				{
					this.effectAudioHingeMovingNegative.Play();
				}
			}
			this.effectAudioHingeMovingPositive.SetSpeed(Mathf.Abs(avg));
			this.effectAudioHingeMovingNegative.SetSpeed(Mathf.Abs(avg));
		}

		// Token: 0x0600224F RID: 8783 RVA: 0x000EC71C File Offset: 0x000EA91C
		private void ListenForInputs()
		{
			if (this.playerHands == null)
			{
				return;
			}
			this.anyAllowedLatchUnlockButtonPressed = false;
			foreach (KeyValuePair<Handle, PlayerHand> handleHandPair in this.playerHands)
			{
				if (handleHandPair.Value == null)
				{
					Handle key = handleHandPair.Key;
					if (key == null || !key.IsTkGrabbed)
					{
						continue;
					}
				}
				PlayerHand value = handleHandPair.Value;
				PlayerControl.Hand hand;
				if ((hand = ((value != null) ? value.controlHand : null)) == null)
				{
					SpellCaster mainTkHandler = handleHandPair.Key.MainTkHandler;
					if (mainTkHandler == null)
					{
						hand = null;
					}
					else
					{
						RagdollHand ragdollHand = mainTkHandler.ragdollHand;
						if (ragdollHand == null)
						{
							hand = null;
						}
						else
						{
							PlayerHand playerHand = ragdollHand.playerHand;
							hand = ((playerHand != null) ? playerHand.controlHand : null);
						}
					}
				}
				PlayerControl.Hand controlHand = hand;
				if (controlHand != null)
				{
					if (handleHandPair.Value != null)
					{
						this.HandleHaptics(handleHandPair.Value.controlHand);
					}
					bool anyAllowedLatchUnlockButtonPressed = (this.allowedInputsToOpenLatch.HasFlagNoGC(HingeDrive.InputType.Pinch) && controlHand.pinchPressed) || (this.allowedInputsToOpenLatch.HasFlagNoGC(HingeDrive.InputType.Use) && controlHand.usePressed) || (this.allowedInputsToOpenLatch.HasFlagNoGC(HingeDrive.InputType.AlternateUse) && controlHand.alternateUsePressed) || (this.allowedInputsToOpenLatch.HasFlagNoGC(HingeDrive.InputType.Cast) && controlHand.castPressed);
					if (anyAllowedLatchUnlockButtonPressed)
					{
						this.ReleaseLatch();
					}
					bool wasPressed = this.latchButtonStatePerHandle[handleHandPair.Key];
					if (anyAllowedLatchUnlockButtonPressed && !wasPressed)
					{
						this.onPlayerPressingLatchButton.Invoke(this.HingeAngle, this.SpeedState, handleHandPair.Key);
						this.latchButtonStatePerHandle[handleHandPair.Key] = true;
						Side side = controlHand.side;
						if (side != Side.Right)
						{
							if (side == Side.Left)
							{
								PlayerControl.handLeft.HapticPlayClip(Catalog.gameData.haptics.hit, 3f);
							}
						}
						else
						{
							PlayerControl.handRight.HapticPlayClip(Catalog.gameData.haptics.hit, 3f);
						}
					}
					else if (!anyAllowedLatchUnlockButtonPressed && wasPressed)
					{
						this.onPlayerReleasingLatchButton.Invoke(this.HingeAngle, this.SpeedState, handleHandPair.Key);
						this.latchButtonStatePerHandle[handleHandPair.Key] = false;
					}
					if (anyAllowedLatchUnlockButtonPressed)
					{
						this.anyAllowedLatchUnlockButtonPressed = true;
					}
				}
			}
		}

		// Token: 0x06002250 RID: 8784 RVA: 0x000EC974 File Offset: 0x000EAB74
		private void HandleHaptics(PlayerControl.Hand controlHand)
		{
			float velocity = this.useSpeedFactor ? this.speedFactor.Evaluate(Mathf.Abs(this.HingeVelocity)) : 1f;
			if (this.enableContinuousHapticOnMove && this.SpeedState != HingeDrive.HingeDriveSpeedState.NotMoving)
			{
				Side side = controlHand.side;
				if (side != Side.Right)
				{
					if (side == Side.Left)
					{
						PlayerControl.handLeft.HapticShort(this.continuousHapticAmplitude * velocity, false);
					}
				}
				else
				{
					PlayerControl.handRight.HapticShort(this.continuousHapticAmplitude * velocity, false);
				}
			}
			int bumpIndex = Mathf.FloorToInt(this.HingeAngle / this.angleStepForBumps);
			if (this.enableHapticAngleBump && bumpIndex != this.previousBumpIndex)
			{
				Side side = controlHand.side;
				if (side != Side.Right)
				{
					if (side == Side.Left)
					{
						PlayerControl.handLeft.HapticShort(this.angleStepHapticAmplitude, false);
					}
				}
				else
				{
					PlayerControl.handRight.HapticShort(this.angleStepHapticAmplitude, false);
				}
			}
			this.previousBumpIndex = bumpIndex;
		}

		// Token: 0x06002251 RID: 8785 RVA: 0x000ECA4A File Offset: 0x000EAC4A
		private void BreakLatch()
		{
			if (this.brokenLatch || !this.useLatch)
			{
				return;
			}
			this.useLatch = false;
			this.onLatchBreak.Invoke(this.HingeVelocity, this.SpeedState);
			this.brokenLatch = true;
			this.currentState = HingeDrive.HingeDriveState.Unlocked;
		}

		// Token: 0x06002252 RID: 8786 RVA: 0x000ECA8C File Offset: 0x000EAC8C
		private void ConfigureHinge(Transform hingeHolder, out HingeJoint hinge)
		{
			hinge = this.hingesHolder.gameObject.AddComponent<HingeJoint>();
			hinge.connectedBody = this.frameRigidbody;
			hinge.axis = this.rotationAxisVector3;
			hinge.anchor = this.hingesHolder.InverseTransformPoint(hingeHolder.position);
			this.UpdateLimits(hinge, this.minAngle, this.maxAngle, this.isLimited);
			this.UpdateSpring(hinge);
			this.UpdateMotor(this.motorTargetVelocity, this.motorForce, hinge, this.useMotor, true);
			hinge.enableCollision = this.enableCollisionWithFrame;
		}

		// Token: 0x06002253 RID: 8787 RVA: 0x000ECB28 File Offset: 0x000EAD28
		private void UpdateSpring(HingeJoint hinge)
		{
			JointSpring spring = hinge.spring;
			hinge.useSpring = true;
			spring.damper = this.damper;
			spring.spring = 0f;
			spring.targetPosition = 0f;
			if (this.autoCloses)
			{
				spring.spring = this.autoCloseSpring;
				spring.damper = this.damper;
				spring.targetPosition = this.restingAngle;
			}
			hinge.spring = spring;
		}

		// Token: 0x06002254 RID: 8788 RVA: 0x000ECBA0 File Offset: 0x000EADA0
		private void UpdateLimits(HingeJoint hinge, float min, float max, bool useLimits)
		{
			JointLimits limits = hinge.limits;
			limits.min = min;
			limits.max = max;
			limits.bounciness = this.angleLimitsBounciness;
			limits.bounceMinVelocity = 0f;
			hinge.limits = limits;
			hinge.useLimits = useLimits;
			this.currentMinAngle = min;
			this.currentMaxAngle = max;
		}

		// Token: 0x06002255 RID: 8789 RVA: 0x000ECBFC File Offset: 0x000EADFC
		private void OnDrawGizmos()
		{
			if (this.hinges == null || (this.hinges.Count <= 0 && this.hingesTargets != null))
			{
				foreach (Transform hingeTarget in this.hingesTargets)
				{
					this.DrawWireArc(this.hingesHolder, hingeTarget.position - this.hingesHolder.position, (float)this.angleSteps, this.gizmosSize);
				}
				return;
			}
			foreach (Vector3 position in from hingePair in this.hinges
			select hingePair.Value into hingeMetaData
			select hingeMetaData.joint.anchor)
			{
				this.DrawWireArc(this.hingesHolder, position, (float)this.angleSteps, this.gizmosSize);
			}
		}

		// Token: 0x06002256 RID: 8790 RVA: 0x000ECD10 File Offset: 0x000EAF10
		private void DrawWireArc(Transform t, Vector3 offset, float maxSteps = 5f, float size = 1f)
		{
			Vector3 vector = new Vector3(0f, 0f, 0f);
			int j = (int)this.rotationAxis;
			vector[j] = 1f;
			Vector3 axis = vector;
			Vector3 f = Vector3.forward;
			if (this.rotationAxis == HingeDrive.RotationAxis.ZAxis)
			{
				f = Vector3.right;
			}
			Matrix4x4 matrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(t.position + offset, t.rotation, Vector3.one);
			Vector3 p = Vector3.zero;
			Vector3 previousPos = p;
			int i = 0;
			Vector3 dir;
			while ((float)i <= maxSteps)
			{
				Gizmos.color = new Color((float)i / maxSteps, 0.3f, 1f - (float)i / maxSteps);
				dir = Quaternion.AngleAxis(this.minAngle + (this.maxAngle - this.minAngle) * ((float)i / maxSteps), axis) * (f / 2f) * size;
				Gizmos.DrawLine(p, p + dir);
				Gizmos.DrawLine(previousPos, p + dir);
				previousPos = p + dir;
				i++;
			}
			Gizmos.color = Color.red;
			dir = Quaternion.AngleAxis(this.defaultAngle, axis) * f * size;
			Gizmos.DrawLine(p, p + dir);
			Gizmos.DrawWireSphere(p + dir, 0.05f * size);
			Gizmos.color = Color.grey;
			dir = f / 1.5f * size;
			Gizmos.DrawLine(p, p + dir);
			Gizmos.DrawWireSphere(p + dir, 0.025f * size);
			Gizmos.color = new Color(0.1f, 1f, 0.7f);
			foreach (HingeDrive.AngleThreshold latchAngle in this.latchAngles)
			{
				Vector3 min = Quaternion.AngleAxis(latchAngle.Min, axis) * f * 0.52f * size;
				Gizmos.DrawLine(p, p + min);
				Vector3 center = Quaternion.AngleAxis(latchAngle.center, axis) * f * 0.52f * size;
				Gizmos.DrawLine(p, p + center);
				Vector3 max = Quaternion.AngleAxis(latchAngle.Max, axis) * f * 0.52f * size;
				Gizmos.DrawLine(p, p + max);
				Gizmos.DrawLine(p + min, p + center);
				Gizmos.DrawLine(p + center, p + max);
				Gizmos.DrawWireSphere(p + center, 0.01f);
			}
			Gizmos.matrix = matrix;
		}

		// Token: 0x0400210C RID: 8460
		private const float UnlockCoolDown = 0.25f;

		// Token: 0x0400210D RID: 8461
		private const float HingeHitThresholdCooldown = 0.25f;

		// Token: 0x0400210E RID: 8462
		private const float GrabSweepTestCooldown = 0.07f;

		// Token: 0x0400210F RID: 8463
		[Header("World references")]
		[Tooltip("This transform is the holder for the Hinge Joint when generated.")]
		public Transform hingesHolder;

		// Token: 0x04002110 RID: 8464
		[Tooltip("Transform that the hinge joint links to.")]
		public Rigidbody frame;

		// Token: 0x04002111 RID: 8465
		[Tooltip("References transforms are used as targets for defining the hinge anchors.")]
		public Transform[] hingesTargets;

		// Token: 0x04002112 RID: 8466
		[Tooltip("This object will ignore collision with the referenced colliders")]
		public Collider[] collidersToIgnore;

		// Token: 0x04002113 RID: 8467
		[Tooltip("Referenced Handles are used to retrieve inputs from, such as Latches.")]
		public Handle[] handles;

		// Token: 0x04002114 RID: 8468
		public List<Handle> handleLatchLock;

		// Token: 0x04002115 RID: 8469
		[Header("Angles config values")]
		[Tooltip("Axis of which the hinge joint rotates in. Gives an accurate gizmo of what axis the hinge rotates.")]
		public HingeDrive.RotationAxis rotationAxis = HingeDrive.RotationAxis.Yaxis;

		// Token: 0x04002116 RID: 8470
		[Tooltip("When disabled, object ignores the min/max axis, and be able to move the full 360 degree angles.")]
		public bool isLimited = true;

		// Token: 0x04002117 RID: 8471
		[Tooltip("The angle of which the hinge will start with when spawned/activated.")]
		public float defaultAngle;

		// Token: 0x04002118 RID: 8472
		[Tooltip("The minimum angle of which the hinge will open to")]
		public float minAngle = -90f;

		// Token: 0x04002119 RID: 8473
		[Tooltip("The maximum angle of which the hinge will open to")]
		public float maxAngle = 90f;

		// Token: 0x0400211A RID: 8474
		[Tooltip("Angle change from default angle to play open FX")]
		public float angleThresholdForOpenFX = 30f;

		// Token: 0x0400211B RID: 8475
		[Header("Misc config values")]
		[Tooltip("When enabled, the Hinge object will collide with the Frame")]
		public bool enableCollisionWithFrame;

		// Token: 0x0400211C RID: 8476
		[Header("Latch config values")]
		[Tooltip("When enabled, the \"Latch\" will be enabled, allowing the hinge to be locked when reaching its resting angle.")]
		public bool useLatch = true;

		// Token: 0x0400211D RID: 8477
		[Tooltip("The inputs of which can open the latch. This uses the players input, such as \"Cast\", which can open the latch via a spell cast (like Gravity Push)")]
		public HingeDrive.InputType allowedInputsToOpenLatch = HingeDrive.InputType.Use;

		// Token: 0x0400211E RID: 8478
		[Tooltip("The specific angles of which the latch will lock at.")]
		public HingeDrive.AngleThreshold[] latchAngles;

		// Token: 0x0400211F RID: 8479
		[Tooltip("When enabled, Latch can be brute forced, forcing it to open (e.g. when \"slammed\" or pushed)")]
		public bool isLatchBruteForceable = true;

		// Token: 0x04002120 RID: 8480
		[Tooltip("This threshold determines that the hinge WILL Open if hit at this impulse, or greater than.")]
		public float forceLatchOpeningImpulseThreshold = 1f;

		// Token: 0x04002121 RID: 8481
		[Tooltip("When enabled, allows the hinge to be broken")]
		public bool isLatchBreakable = true;

		// Token: 0x04002122 RID: 8482
		[Tooltip("The health of the hatch of which will be broken if enabled to.")]
		public float latchHealth = 100f;

		// Token: 0x04002123 RID: 8483
		[Header("Motor config values")]
		[Tooltip("When enabled, Motor becomes active. The Motor rotates the Hinge in perpetual motion. If the hinge is limited, will automatically rotate from Min to Max. When not limited, it will rotate in perpetual motion, like a wheel.")]
		public bool useMotor;

		// Token: 0x04002124 RID: 8484
		[Tooltip("Determines the target velocity of which the motor rotates the hinge.")]
		public float motorTargetVelocity;

		// Token: 0x04002125 RID: 8485
		[Tooltip("Determines the force of which the motor rotates the hinge.")]
		public float motorForce;

		// Token: 0x04002126 RID: 8486
		[Header("Auto close config values")]
		[Range(0f, 1f)]
		[Tooltip("Makes the hinge bounce (or not) when it is opened fully.")]
		public float angleLimitsBounciness = 0.2f;

		// Token: 0x04002127 RID: 8487
		[Tooltip("This damper prevents the hinge from moving very slow for a long time.")]
		public float damper = 5f;

		// Token: 0x04002128 RID: 8488
		[Tooltip("Allows the hinge to return to its resting angle.")]
		public bool autoCloses;

		// Token: 0x04002129 RID: 8489
		[Tooltip("Angle of which the hinge will be pulled towards, by a spring")]
		public float restingAngle;

		// Token: 0x0400212A RID: 8490
		[Tooltip("Force of the spring that pulls the hinge to the resting position")]
		public float autoCloseSpring = 10f;

		// Token: 0x0400212B RID: 8491
		[Header("Haptic config values")]
		[Tooltip("When enabled, when grabbed, the hinge handle will vibrate player's controllers when the hinge moves/rotates.")]
		public bool enableContinuousHapticOnMove;

		// Token: 0x0400212C RID: 8492
		[Tooltip("When enabled, the speed affects the vibration when hinge moves.")]
		public bool useSpeedFactor = true;

		// Token: 0x0400212D RID: 8493
		[Tooltip("Curve of which determines the intensity of the vibration on speed factor.")]
		public AnimationCurve speedFactor;

		// Token: 0x0400212E RID: 8494
		[Tooltip("Determines the intensity of the continuous vibration.")]
		public float continuousHapticAmplitude = 0.1f;

		// Token: 0x0400212F RID: 8495
		[Tooltip("When enabled, the grabbed handle will vibrate each time it reaches a threshold/desired angle.")]
		public bool enableHapticAngleBump;

		// Token: 0x04002130 RID: 8496
		[Tooltip("Angle step at which the handle will vibrate (in degrees)")]
		public float angleStepForBumps = 10f;

		// Token: 0x04002131 RID: 8497
		[Tooltip("Intensity of the bump vibration.")]
		public float angleStepHapticAmplitude = 5f;

		// Token: 0x04002132 RID: 8498
		[Header("Audio config values")]
		[Tooltip("Determines how smooth the looping sound effects are. The higher the number, the smoother the sound effect.")]
		public int smoothingSamples = 60;

		// Token: 0x04002133 RID: 8499
		[Tooltip("FX Plays when Hinge is Moving from the Minimum to Maximum angle")]
		public FxModule effectAudioHingeMovingPositive;

		// Token: 0x04002134 RID: 8500
		[Tooltip("FX Plays when Hinge is Moving from the Maximum to Minimum angle")]
		public FxModule effectAudioHingeMovingNegative;

		// Token: 0x04002135 RID: 8501
		[Tooltip("FX Plays when hinge is closed quickly (slam).")]
		public FxModule effectAudioSlam;

		// Token: 0x04002136 RID: 8502
		[Tooltip("FX plays when Latch is Closed.")]
		public FxModule effectAudioLatchLock;

		// Token: 0x04002137 RID: 8503
		[Tooltip("FX plays when Latch is Opened.")]
		public FxModule effectAudioLatchUnlock;

		// Token: 0x04002138 RID: 8504
		[Tooltip("FX plays when Latch is Broken.")]
		public FxModule effectAudioLatchBreak;

		// Token: 0x04002139 RID: 8505
		[Tooltip("FX plays when hinge hits its minimum/maximum angle.")]
		public FxModule effectAudioWiggle;

		// Token: 0x0400213A RID: 8506
		[Tooltip("FX plays when player presses the latch button.")]
		public FxModule effectAudioLatchButtonPress;

		// Token: 0x0400213B RID: 8507
		[Tooltip("FX plays when player releases the latch button.")]
		public FxModule effectAudioLatchButtonRelease;

		// Token: 0x0400213C RID: 8508
		[Tooltip("FX plays once when opening latch")]
		public FxModule playOnceOnOpenLatchEffect;

		// Token: 0x0400213D RID: 8509
		[Header("Auto open config values")]
		[Tooltip("Calling any automatic method without a given velocity will fall back to this.")]
		public float autoOpenFallBackVelocity = 45f;

		// Token: 0x0400213E RID: 8510
		[Tooltip("Calling any automatic method without a given force will fall back to this.")]
		public float autoOpenFallBackForce = 9999f;

		// Token: 0x0400213F RID: 8511
		[Tooltip("Calling any automatic method without a given force will bypass the latch to open.")]
		public bool autoOpenBypassLatch = true;

		// Token: 0x04002140 RID: 8512
		[Header("Events")]
		public UnityEvent<float, HingeDrive.HingeDriveSpeedState, float, float> onHingeMove = new UnityEvent<float, HingeDrive.HingeDriveSpeedState, float, float>();

		// Token: 0x04002141 RID: 8513
		public UnityEvent<float, HingeDrive.HingeDriveSpeedState> onLatchLock = new UnityEvent<float, HingeDrive.HingeDriveSpeedState>();

		// Token: 0x04002142 RID: 8514
		public UnityEvent<float, HingeDrive.HingeDriveSpeedState> onLatchUnlock = new UnityEvent<float, HingeDrive.HingeDriveSpeedState>();

		// Token: 0x04002143 RID: 8515
		public UnityEvent<float, HingeDrive.HingeDriveSpeedState, Handle> onPlayerPressingLatchButton = new UnityEvent<float, HingeDrive.HingeDriveSpeedState, Handle>();

		// Token: 0x04002144 RID: 8516
		public UnityEvent<float, HingeDrive.HingeDriveSpeedState, Handle> onPlayerReleasingLatchButton = new UnityEvent<float, HingeDrive.HingeDriveSpeedState, Handle>();

		// Token: 0x04002145 RID: 8517
		public UnityEvent<float, HingeDrive.HingeDriveSpeedState> onLatchBreak = new UnityEvent<float, HingeDrive.HingeDriveSpeedState>();

		// Token: 0x04002146 RID: 8518
		public UnityEvent<bool, float, HingeDrive.HingeDriveSpeedState> onHingeHitThreshold = new UnityEvent<bool, float, HingeDrive.HingeDriveSpeedState>();

		// Token: 0x04002147 RID: 8519
		[Header("Gizmos")]
		[Range(0f, 3f)]
		[Tooltip("Determines the size of the gizmo")]
		public float gizmosSize = 1f;

		// Token: 0x04002148 RID: 8520
		[Range(0f, 20f)]
		[Tooltip("Determines how many steps are in the gizmo.")]
		public int angleSteps = 5;

		// Token: 0x04002149 RID: 8521
		private Rigidbody frameRigidbody;

		// Token: 0x0400214A RID: 8522
		private Rigidbody hingeRigidbody;

		// Token: 0x0400214B RID: 8523
		private Vector3 rotationAxisVector3;

		// Token: 0x0400214C RID: 8524
		private Dictionary<HingeJoint, HingeDrive.HingeMetaData> hinges;

		// Token: 0x0400214D RID: 8525
		private HingeDrive.HingeMetaData[] hingesFlatArray;

		// Token: 0x0400214F RID: 8527
		private float previousAngle;

		// Token: 0x04002150 RID: 8528
		private float lastUnlockTime;

		// Token: 0x04002151 RID: 8529
		private Quaternion closedHingeDefaultRotation;

		// Token: 0x04002152 RID: 8530
		private Dictionary<Handle, PlayerHand> playerHands;

		// Token: 0x04002153 RID: 8531
		private Dictionary<Handle, bool> latchButtonStatePerHandle;

		// Token: 0x04002154 RID: 8532
		private float currentMinAngle;

		// Token: 0x04002155 RID: 8533
		private float currentMaxAngle;

		// Token: 0x04002156 RID: 8534
		private bool brokenLatch;

		// Token: 0x04002157 RID: 8535
		private bool hasHingeHitThreshold;

		// Token: 0x04002158 RID: 8536
		private float lastHitThresholdTime;

		// Token: 0x04002159 RID: 8537
		private Coroutine autoRotateRoutine;

		// Token: 0x0400215A RID: 8538
		private bool init;

		// Token: 0x0400215B RID: 8539
		private bool anyAllowedLatchUnlockButtonPressed;

		// Token: 0x0400215C RID: 8540
		private HingeDrive.AngleThreshold currentLockedLatchAngle;

		// Token: 0x0400215D RID: 8541
		private float lastGrabTime;

		// Token: 0x0400215E RID: 8542
		private int previousBumpIndex = -1;

		// Token: 0x0400215F RID: 8543
		private float[] smoothingVelocityBuffer;

		// Token: 0x04002160 RID: 8544
		private int sampleIndex;

		// Token: 0x04002161 RID: 8545
		private HingeDrive.InputType cachedAllowedInputs;

		// Token: 0x04002162 RID: 8546
		private SpawnableArea currentArea;

		// Token: 0x04002163 RID: 8547
		private bool hasPlayedOpenFX;

		// Token: 0x04002164 RID: 8548
		private float OpenFXTimer;

		// Token: 0x04002165 RID: 8549
		private bool isGrabbed;

		// Token: 0x0200099A RID: 2458
		public enum RotationAxis
		{
			// Token: 0x04004526 RID: 17702
			XAxis,
			// Token: 0x04004527 RID: 17703
			Yaxis,
			// Token: 0x04004528 RID: 17704
			ZAxis
		}

		// Token: 0x0200099B RID: 2459
		public enum HingeDriveState
		{
			// Token: 0x0400452A RID: 17706
			LatchLocked,
			// Token: 0x0400452B RID: 17707
			Unlocked
		}

		// Token: 0x0200099C RID: 2460
		public enum HingeDriveSpeedState
		{
			// Token: 0x0400452D RID: 17709
			NotMoving,
			// Token: 0x0400452E RID: 17710
			Slow,
			// Token: 0x0400452F RID: 17711
			Fast,
			// Token: 0x04004530 RID: 17712
			ReallyFast
		}

		// Token: 0x0200099D RID: 2461
		private struct HingeMetaData
		{
			// Token: 0x060043FF RID: 17407 RVA: 0x00190767 File Offset: 0x0018E967
			public HingeMetaData(HingeJoint joint)
			{
				this.joint = joint;
			}

			// Token: 0x04004531 RID: 17713
			public HingeJoint joint;
		}

		// Token: 0x0200099E RID: 2462
		[Flags]
		public enum InputType
		{
			// Token: 0x04004533 RID: 17715
			None = 0,
			// Token: 0x04004534 RID: 17716
			Pinch = 2,
			// Token: 0x04004535 RID: 17717
			Use = 4,
			// Token: 0x04004536 RID: 17718
			AlternateUse = 8,
			// Token: 0x04004537 RID: 17719
			Cast = 16
		}

		// Token: 0x0200099F RID: 2463
		[Serializable]
		public struct AngleThreshold
		{
			// Token: 0x17000584 RID: 1412
			// (get) Token: 0x06004400 RID: 17408 RVA: 0x00190770 File Offset: 0x0018E970
			public float Min
			{
				get
				{
					return this.center + this.minOffset;
				}
			}

			// Token: 0x17000585 RID: 1413
			// (get) Token: 0x06004401 RID: 17409 RVA: 0x0019077F File Offset: 0x0018E97F
			public float Max
			{
				get
				{
					return this.center + this.maxOffset;
				}
			}

			// Token: 0x06004402 RID: 17410 RVA: 0x0019078E File Offset: 0x0018E98E
			public AngleThreshold(float center, float minOffset, float maxOffset)
			{
				this.minOffset = minOffset;
				this.maxOffset = maxOffset;
				this.center = center;
			}

			// Token: 0x04004538 RID: 17720
			public float minOffset;

			// Token: 0x04004539 RID: 17721
			public float center;

			// Token: 0x0400453A RID: 17722
			public float maxOffset;
		}
	}
}
