using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x0200027A RID: 634
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RagdollHand")]
	[AddComponentMenu("ThunderRoad/Creatures/Ragdoll hand")]
	public class RagdollHand : RagdollPart
	{
		// Token: 0x140000DA RID: 218
		// (add) Token: 0x06001D40 RID: 7488 RVA: 0x000C6B34 File Offset: 0x000C4D34
		// (remove) Token: 0x06001D41 RID: 7489 RVA: 0x000C6B6C File Offset: 0x000C4D6C
		public event RagdollHand.PunchEvent OnPunchStartEvent;

		// Token: 0x140000DB RID: 219
		// (add) Token: 0x06001D42 RID: 7490 RVA: 0x000C6BA4 File Offset: 0x000C4DA4
		// (remove) Token: 0x06001D43 RID: 7491 RVA: 0x000C6BDC File Offset: 0x000C4DDC
		public event RagdollHand.PunchEvent OnPunchEndEvent;

		// Token: 0x140000DC RID: 220
		// (add) Token: 0x06001D44 RID: 7492 RVA: 0x000C6C14 File Offset: 0x000C4E14
		// (remove) Token: 0x06001D45 RID: 7493 RVA: 0x000C6C4C File Offset: 0x000C4E4C
		public event RagdollHand.PunchHitEvent OnPunchHitEvent;

		/// <summary>
		/// The colliders of the linked forearm
		/// </summary>
		// Token: 0x170001D3 RID: 467
		// (get) Token: 0x06001D46 RID: 7494 RVA: 0x000C6C84 File Offset: 0x000C4E84
		// (set) Token: 0x06001D47 RID: 7495 RVA: 0x000C6CD7 File Offset: 0x000C4ED7
		public List<Collider> ForeArmColliders
		{
			get
			{
				if (this.foreArmColliders != null)
				{
					return this.foreArmColliders;
				}
				if (!this.lowerArmPart)
				{
					return null;
				}
				ColliderGroup armColliderGroup = this.lowerArmPart.colliderGroup;
				if (!armColliderGroup)
				{
					return null;
				}
				this.foreArmColliders = armColliderGroup.colliders;
				return this.foreArmColliders;
			}
			set
			{
				this.foreArmColliders = value;
			}
		}

		// Token: 0x170001D4 RID: 468
		// (get) Token: 0x06001D48 RID: 7496 RVA: 0x000C6CE0 File Offset: 0x000C4EE0
		public Vector3 PalmDir
		{
			get
			{
				return -base.transform.forward;
			}
		}

		// Token: 0x170001D5 RID: 469
		// (get) Token: 0x06001D49 RID: 7497 RVA: 0x000C6CF2 File Offset: 0x000C4EF2
		public Vector3 PointDir
		{
			get
			{
				return -base.transform.right;
			}
		}

		// Token: 0x170001D6 RID: 470
		// (get) Token: 0x06001D4A RID: 7498 RVA: 0x000C6D04 File Offset: 0x000C4F04
		public Vector3 ThumbDir
		{
			get
			{
				if (this.side != Side.Right)
				{
					return -base.transform.up;
				}
				return base.transform.up;
			}
		}

		// Token: 0x06001D4B RID: 7499 RVA: 0x000C6D2C File Offset: 0x000C4F2C
		public RagdollHand.Finger GetFinger(HandPoseData.FingerType type)
		{
			RagdollHand.Finger result;
			switch (type)
			{
			case HandPoseData.FingerType.Thumb:
				result = this.fingerThumb;
				break;
			case HandPoseData.FingerType.Index:
				result = this.fingerIndex;
				break;
			case HandPoseData.FingerType.Middle:
				result = this.fingerMiddle;
				break;
			case HandPoseData.FingerType.Ring:
				result = this.fingerRing;
				break;
			case HandPoseData.FingerType.Little:
				result = this.fingerLittle;
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
			return result;
		}

		// Token: 0x140000DD RID: 221
		// (add) Token: 0x06001D4C RID: 7500 RVA: 0x000C6D98 File Offset: 0x000C4F98
		// (remove) Token: 0x06001D4D RID: 7501 RVA: 0x000C6DD0 File Offset: 0x000C4FD0
		public event RagdollHand.ControlPoseChangeEvent OnControlPoseChangeEvent;

		// Token: 0x06001D4E RID: 7502 RVA: 0x000C6E08 File Offset: 0x000C5008
		protected override void OnValidate()
		{
			base.OnValidate();
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.grip = base.transform.Find("Grip");
			if (!this.grip)
			{
				this.grip = this.CreateDefaultGrip();
			}
			if (this.creature == null)
			{
				this.creature = base.GetComponentInParent<Creature>();
			}
			if (this.poser == null)
			{
				this.poser = base.GetComponent<RagdollHandPoser>();
			}
		}

		// Token: 0x06001D4F RID: 7503 RVA: 0x000C6E8C File Offset: 0x000C508C
		public void MirrorFingersToOtherHand()
		{
			if (!this.creature)
			{
				this.creature = base.GetComponentInParent<Creature>();
			}
			foreach (RagdollHand ragdollHand in this.creature.GetComponentsInChildren<RagdollHand>())
			{
				if (ragdollHand != this)
				{
					this.otherHand = ragdollHand;
					break;
				}
			}
			UnityEngine.Object.DestroyImmediate(this.otherHand.palmCollider.gameObject);
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.palmCollider.gameObject, this.otherHand.transform);
			gameObject.name = this.palmCollider.name;
			gameObject.transform.MirrorChilds(new Vector3(1f, -1f, 1f));
			Transform[] componentsInChildren2 = gameObject.GetComponentsInChildren<Transform>();
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].localScale = Vector3.one;
			}
			this.otherHand.SetupFingers();
		}

		// Token: 0x06001D50 RID: 7504 RVA: 0x000C6F70 File Offset: 0x000C5170
		public virtual void SetGripToDefaultPosition()
		{
			Transform grip = base.transform.Find("Grip");
			if (grip)
			{
				UnityEngine.Object.DestroyImmediate(grip.gameObject);
			}
			grip = this.CreateDefaultGrip();
		}

		// Token: 0x06001D51 RID: 7505 RVA: 0x000C6FA8 File Offset: 0x000C51A8
		public virtual Transform CreateDefaultGrip()
		{
			Transform newGrip = new GameObject("Grip").transform;
			newGrip.transform.SetParent(base.transform);
			newGrip.transform.localScale = Vector3.one;
			if (this.side == Side.Left)
			{
				newGrip.transform.localPosition = new Vector3(-0.042f, -0.01f, 0.003f);
				newGrip.transform.localRotation = Quaternion.Euler(0f, 220f, -90f);
			}
			if (this.side == Side.Right)
			{
				newGrip.transform.localPosition = new Vector3(0.042f, -0.01f, 0.003f);
				newGrip.transform.localRotation = Quaternion.Euler(0f, 140f, 90f);
			}
			return newGrip;
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x000C7074 File Offset: 0x000C5274
		public virtual void SetupFingers()
		{
			if (!this.creature)
			{
				this.creature = base.GetComponentInParent<Creature>();
			}
			this.fingers = new List<RagdollHand.Finger>();
			Transform transform = base.transform.Find("Palm");
			this.palmCollider = ((transform != null) ? transform.GetComponent<Collider>() : null);
			if (this.palmCollider == null)
			{
				this.palmCollider = new GameObject("Palm").AddComponent<BoxCollider>();
				this.palmCollider.transform.SetParentOrigin(base.transform);
				(this.palmCollider as BoxCollider).size = new Vector3(0.1f, 0.1f, 0.03f);
				this.palmCollider.gameObject.AddComponent<ColliderGroup>();
			}
			this.fingerThumb.proximal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightThumbProximal : HumanBodyBones.LeftThumbProximal);
			if (!this.fingerThumb.proximal.mesh)
			{
				Debug.LogError("Could not find ThumbProximal bone on animator");
			}
			this.fingerThumb.intermediate.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightThumbIntermediate : HumanBodyBones.LeftThumbIntermediate);
			if (!this.fingerThumb.intermediate.mesh)
			{
				Debug.LogError("Could not find ThumbIntermediate bone on animator");
			}
			this.fingerThumb.distal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightThumbDistal : HumanBodyBones.LeftThumbDistal);
			if (!this.fingerThumb.distal.mesh)
			{
				Debug.LogError("Could not find ThumbDistal bone on animator");
			}
			this.SetupFinger(this.fingerThumb, "Thumb");
			if (this.fingerThumb.proximal.mesh)
			{
				this.fingers.Add(this.fingerThumb);
			}
			this.fingerIndex.proximal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightIndexProximal : HumanBodyBones.LeftIndexProximal);
			if (!this.fingerIndex.proximal.mesh)
			{
				Debug.LogError("Could not find IndexProximal bone on animator");
			}
			this.fingerIndex.intermediate.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightIndexIntermediate : HumanBodyBones.LeftIndexIntermediate);
			if (!this.fingerIndex.intermediate.mesh)
			{
				Debug.LogError("Could not find IndexIntermediate bone on animator");
			}
			this.fingerIndex.distal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightIndexDistal : HumanBodyBones.LeftIndexDistal);
			if (!this.fingerIndex.distal.mesh)
			{
				Debug.LogError("Could not find IndexDistal bone on animator");
			}
			this.SetupFinger(this.fingerIndex, "Index");
			if (this.fingerIndex.proximal.mesh)
			{
				this.fingers.Add(this.fingerIndex);
			}
			this.fingerMiddle.proximal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightMiddleProximal : HumanBodyBones.LeftMiddleProximal);
			if (!this.fingerMiddle.proximal.mesh)
			{
				Debug.LogError("Could not find MiddleProximal bone on animator");
			}
			this.fingerMiddle.intermediate.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightMiddleIntermediate : HumanBodyBones.LeftMiddleIntermediate);
			if (!this.fingerMiddle.intermediate.mesh)
			{
				Debug.LogError("Could not find MiddleIntermediate bone on animator");
			}
			this.fingerMiddle.distal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightMiddleDistal : HumanBodyBones.LeftMiddleDistal);
			if (!this.fingerMiddle.distal.mesh)
			{
				Debug.LogError("Could not find MiddleDistal bone on animator");
			}
			this.SetupFinger(this.fingerMiddle, "Middle");
			if (this.fingerMiddle.proximal.mesh)
			{
				this.fingers.Add(this.fingerMiddle);
			}
			this.fingerRing.proximal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightRingProximal : HumanBodyBones.LeftRingProximal);
			if (!this.fingerRing.proximal.mesh)
			{
				Debug.LogError("Could not find RingProximal bone on animator");
			}
			this.fingerRing.intermediate.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightRingIntermediate : HumanBodyBones.LeftRingIntermediate);
			if (!this.fingerRing.intermediate.mesh)
			{
				Debug.LogError("Could not find RingIntermediate bone on animator");
			}
			this.fingerRing.distal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightRingDistal : HumanBodyBones.LeftRingDistal);
			if (!this.fingerRing.distal.mesh)
			{
				Debug.LogError("Could not find RingDistal bone on animator");
			}
			this.SetupFinger(this.fingerRing, "Ring");
			if (this.fingerRing.proximal.mesh)
			{
				this.fingers.Add(this.fingerRing);
			}
			this.fingerLittle.proximal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightLittleProximal : HumanBodyBones.LeftLittleProximal);
			if (!this.fingerLittle.proximal.mesh)
			{
				Debug.LogError("Could not find LittleProximal bone on animator");
			}
			this.fingerLittle.intermediate.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightLittleIntermediate : HumanBodyBones.LeftLittleIntermediate);
			if (!this.fingerLittle.intermediate.mesh)
			{
				Debug.LogError("Could not find LittleIntermediate bone on animator");
			}
			this.fingerLittle.distal.mesh = this.creature.animator.GetBoneTransform((this.side == Side.Right) ? HumanBodyBones.RightLittleDistal : HumanBodyBones.LeftLittleDistal);
			if (!this.fingerLittle.distal.mesh)
			{
				Debug.LogError("Could not find LittleDistal bone on animator");
			}
			this.SetupFinger(this.fingerLittle, "Little");
			if (this.fingerLittle.proximal.mesh)
			{
				this.fingers.Add(this.fingerLittle);
			}
		}

		// Token: 0x06001D53 RID: 7507 RVA: 0x000C76F4 File Offset: 0x000C58F4
		protected virtual void SetupFinger(RagdollHand.Finger finger, string name)
		{
			if (!finger.proximal.collider)
			{
				RagdollHand.Finger.Bone proximal = finger.proximal;
				Transform transform = this.palmCollider.transform.Find(name + "Proximal");
				proximal.collider = ((transform != null) ? transform.GetComponent<CapsuleCollider>() : null);
			}
			if (!finger.proximal.collider)
			{
				finger.proximal.collider = new GameObject(name + "Proximal").AddComponent<CapsuleCollider>();
				finger.proximal.collider.radius = 0.01f;
				finger.proximal.collider.height = 0.05f;
				finger.proximal.collider.direction = 0;
				finger.proximal.collider.transform.SetParent(this.palmCollider.transform);
			}
			Transform proximalColliderTransform = finger.proximal.collider.transform;
			proximalColliderTransform.SetPositionAndRotation(finger.proximal.mesh.position, finger.proximal.mesh.rotation);
			finger.proximal.colliderTransform = proximalColliderTransform;
			if (!finger.intermediate.collider)
			{
				RagdollHand.Finger.Bone intermediate = finger.intermediate;
				Transform transform2 = proximalColliderTransform.Find(name + "Intermediate");
				intermediate.collider = ((transform2 != null) ? transform2.GetComponent<CapsuleCollider>() : null);
			}
			if (!finger.intermediate.collider)
			{
				finger.intermediate.collider = new GameObject(name + "Intermediate").AddComponent<CapsuleCollider>();
				finger.intermediate.collider.radius = 0.01f;
				finger.intermediate.collider.height = 0.05f;
				finger.intermediate.collider.direction = 0;
				finger.intermediate.collider.transform.SetParent(proximalColliderTransform);
			}
			Transform intermediateColliderTransform = finger.intermediate.collider.transform;
			intermediateColliderTransform.SetPositionAndRotation(finger.intermediate.mesh.position, finger.intermediate.mesh.rotation);
			finger.intermediate.colliderTransform = intermediateColliderTransform;
			if (!finger.distal.collider)
			{
				RagdollHand.Finger.Bone distal = finger.distal;
				Transform transform3 = intermediateColliderTransform.Find(name + "Distal");
				distal.collider = ((transform3 != null) ? transform3.GetComponent<CapsuleCollider>() : null);
			}
			if (!finger.distal.collider)
			{
				finger.distal.collider = new GameObject(name + "Distal").AddComponent<CapsuleCollider>();
				finger.distal.collider.radius = 0.01f;
				finger.distal.collider.height = 0.05f;
				finger.distal.collider.direction = 0;
				finger.distal.collider.transform.SetParent(intermediateColliderTransform);
			}
			Transform distalColliderTransform = finger.distal.collider.transform;
			distalColliderTransform.SetPositionAndRotation(finger.distal.mesh.position, finger.distal.mesh.rotation);
			finger.distal.colliderTransform = distalColliderTransform;
			string tipName = name + "Tip";
			finger.tip = distalColliderTransform.Find(tipName);
			if (!finger.tip)
			{
				finger.tip = new GameObject(tipName).transform;
				finger.tip.SetParent(distalColliderTransform);
			}
			finger.tip.localRotation = Quaternion.identity;
			finger.tip.localPosition = Vector3.zero;
		}

		// Token: 0x06001D54 RID: 7508 RVA: 0x000C7A78 File Offset: 0x000C5C78
		public static List<RagdollHand.GrabState> UnGrabAllAndSaveState(List<Handle> handles)
		{
			List<RagdollHand.GrabState> grabStates = new List<RagdollHand.GrabState>();
			for (int i = handles.Count - 1; i >= 0; i--)
			{
				for (int i2 = handles[i].handlers.Count - 1; i2 >= 0; i2--)
				{
					grabStates.Add(new RagdollHand.GrabState(handles[i].handlers[i2]));
					handles[i].handlers[i2].UnGrab(false);
				}
			}
			return grabStates;
		}

		// Token: 0x06001D55 RID: 7509 RVA: 0x000C7AF4 File Offset: 0x000C5CF4
		public static void GrabFromSavedStates(List<RagdollHand.GrabState> grabStates)
		{
			foreach (RagdollHand.GrabState grabState in grabStates)
			{
				grabState.ragdollHand.Grab(grabState.handle, grabState.orientation, grabState.gripAxisPosition, false, false);
			}
		}

		// Token: 0x06001D56 RID: 7510 RVA: 0x000C7B5C File Offset: 0x000C5D5C
		protected override void Awake()
		{
			base.Awake();
			this.caster = base.GetComponentInChildren<SpellCaster>();
			this.wristStats = this.parentPart.GetComponentInChildren<WristStats>();
			this.grip = base.transform.Find("Grip");
			if (!this.grip)
			{
				this.grip = this.CreateDefaultGrip();
			}
			this.orgGripLocalPosition = this.grip.localPosition;
			this.orgGripLocalRotation = this.grip.localRotation;
			this.touchedInteractables = new List<Interactable>();
			this.touchCollider.isTrigger = true;
			this.touchCollider.gameObject.layer = GameManager.GetLayer(LayerName.Touch);
			this.touchCollider.enabled = false;
			this.waterHandler = new WaterHandler(true, false);
			this.waterHandler.OnWaterEnter += this.OnWaterEnter;
			this.waterHandler.OnWaterExit += this.OnWaterExit;
			this.collisionHandler.OnCollisionStartEvent += this.OnHandCollision;
		}

		// Token: 0x06001D57 RID: 7511 RVA: 0x000C7C68 File Offset: 0x000C5E68
		private void OnHandCollision(CollisionInstance hit)
		{
			RagdollHand.PunchHitEvent onPunchHitEvent = this.OnPunchHitEvent;
			if (onPunchHitEvent == null)
			{
				return;
			}
			onPunchHitEvent(this, hit, this.Fist);
		}

		// Token: 0x170001D7 RID: 471
		// (get) Token: 0x06001D58 RID: 7512 RVA: 0x000C7C82 File Offset: 0x000C5E82
		private bool Fist
		{
			get
			{
				return this.playerHand && this.playerHand.controlHand.gripPressed && this.playerHand.controlHand.usePressed;
			}
		}

		// Token: 0x06001D59 RID: 7513 RVA: 0x000C7CB5 File Offset: 0x000C5EB5
		protected override void ManagedOnDisable()
		{
			this.waterHandler.Reset();
		}

		// Token: 0x170001D8 RID: 472
		// (get) Token: 0x06001D5A RID: 7514 RVA: 0x000C7CC2 File Offset: 0x000C5EC2
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update | ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x06001D5B RID: 7515 RVA: 0x000C7CC8 File Offset: 0x000C5EC8
		protected internal override void ManagedFixedUpdate()
		{
			if (this.initialized && this.playerHand)
			{
				this.climb.FixedUpdate();
				this.swim.FixedUpdate();
				if (this.grabbedHandle)
				{
					this.grabbedHandle.FixedUpdateHandle(this);
				}
				Vector3 velocity = this.Velocity();
				if (!this.punching)
				{
					if (this.Fist && Vector3.Angle(velocity, this.PointDir) < RagdollHand.punchDetectionAngleThreshold && velocity.sqrMagnitude > RagdollHand.punchDetectionThreshold * RagdollHand.punchDetectionThreshold)
					{
						this.punching = true;
						RagdollHand.PunchEvent onPunchStartEvent = this.OnPunchStartEvent;
						if (onPunchStartEvent == null)
						{
							return;
						}
						onPunchStartEvent(this, velocity);
						return;
					}
				}
				else if (!this.Fist || velocity.sqrMagnitude < RagdollHand.punchStopThreshold * RagdollHand.punchStopThreshold)
				{
					this.punching = false;
					RagdollHand.PunchEvent onPunchEndEvent = this.OnPunchEndEvent;
					if (onPunchEndEvent == null)
					{
						return;
					}
					onPunchEndEvent(this, velocity);
				}
			}
		}

		// Token: 0x06001D5C RID: 7516 RVA: 0x000C7DAC File Offset: 0x000C5FAC
		protected internal override void ManagedUpdate()
		{
			if (this.initialized)
			{
				this.UpdateClimb();
				this.UpdateWater();
				if (this.grabbedHandle)
				{
					this.grabbedHandle.UpdateHandle(this);
				}
				this.UpdatePoseInformation();
			}
		}

		// Token: 0x06001D5D RID: 7517 RVA: 0x000C7DE4 File Offset: 0x000C5FE4
		private void UpdatePoseInformation()
		{
			RagdollHand.ControlPose oldPose = this.controlPose;
			if (this.grabbedHandle != null)
			{
				this.controlPose = RagdollHand.ControlPose.HandPose;
			}
			else if (this.playerHand != null && this.playerHand.controlHand != null)
			{
				int gripPressed = this.playerHand.controlHand.gripPressed ? 1 : 0;
				int usePressed = this.playerHand.controlHand.usePressed ? 2 : 0;
				this.controlPose = RagdollHand.ControlPose.Open + gripPressed + usePressed;
			}
			else
			{
				this.controlPose = RagdollHand.ControlPose.Open;
			}
			if (oldPose != this.controlPose && this.OnControlPoseChangeEvent != null)
			{
				this.OnControlPoseChangeEvent(this.side, oldPose, this.controlPose);
			}
		}

		// Token: 0x06001D5E RID: 7518 RVA: 0x000C7E95 File Offset: 0x000C6095
		protected internal override void ManagedLateUpdate()
		{
			if (this.initialized && !this.pauseCheck && this.touchedInteractables.Count > 0)
			{
				this.CheckInteractablesStillTouched();
			}
			this.pauseCheck = false;
		}

		// Token: 0x06001D5F RID: 7519 RVA: 0x000C7EC4 File Offset: 0x000C60C4
		public override void Init(Ragdoll ragdoll)
		{
			base.Init(ragdoll);
			this.creature = ragdoll.creature;
			this.creature.ragdoll.OnStateChange += this.OnRagdollStateChange;
			RenderPipelineManager.beginContextRendering += this.UpdateHandleSync;
			foreach (RagdollHand ragdollHand in ragdoll.GetComponentsInChildren<RagdollHand>())
			{
				if (ragdollHand != this)
				{
					this.otherHand = ragdollHand;
					break;
				}
			}
			this.climb = new RagdollHandClimb(this);
			this.swim.Init(this);
			this.SetFingerColliders(false);
		}

		// Token: 0x06001D60 RID: 7520 RVA: 0x000C7F5B File Offset: 0x000C615B
		private void UpdateHandleSync(ScriptableRenderContext arg1, List<Camera> arg2)
		{
			if (this.ragdoll.creature.isPlayer || !this.grabbedHandle || this.gripInfo.type != Handle.GripInfo.Type.HandSync)
			{
				return;
			}
			this.grabbedHandle.MoveAndAlignToHand(this);
		}

		// Token: 0x06001D61 RID: 7521 RVA: 0x000C7F98 File Offset: 0x000C6198
		public void InitAfterBoneInit()
		{
			foreach (RagdollHand.Finger finger in this.fingers)
			{
				if (finger.proximal.mesh)
				{
					finger.proximal.animation = this.ragdoll.GetBone(finger.proximal.mesh).animation;
				}
				else
				{
					Debug.LogError("A finger mesh reference is missing, did you setup fingers on RagdollHand?");
				}
				if (finger.intermediate.mesh)
				{
					finger.intermediate.animation = this.ragdoll.GetBone(finger.intermediate.mesh).animation;
				}
				else
				{
					Debug.LogError("A finger mesh reference is missing, did you setup fingers on RagdollHand?");
				}
				if (finger.distal.mesh)
				{
					finger.distal.animation = this.ragdoll.GetBone(finger.distal.mesh).animation;
				}
				else
				{
					Debug.LogError("A finger mesh reference is missing, did you setup fingers on RagdollHand?");
				}
			}
			this.RefreshFingerMeshParent(true);
		}

		// Token: 0x06001D62 RID: 7522 RVA: 0x000C80C0 File Offset: 0x000C62C0
		public override void Load()
		{
			base.Load();
			if (this.poser)
			{
				this.poser.ResetDefaultPose();
				this.poser.ResetTargetPose();
			}
		}

		// Token: 0x06001D63 RID: 7523 RVA: 0x000C80EB File Offset: 0x000C62EB
		private void OnWaterEnter()
		{
			this.caster.RefreshWater();
			this.swim.OnWaterEnter();
		}

		// Token: 0x06001D64 RID: 7524 RVA: 0x000C8104 File Offset: 0x000C6304
		private void UpdateWater()
		{
			if (this.playerHand)
			{
				this.waterHandler.Update(this.grip.position, this.grip.position.y - this.collisionUngrabRadius * 0.5f, this.grip.position.y + this.collisionUngrabRadius * 0.5f, this.collisionUngrabRadius, this.grabbedHandle ? this.grabbedHandle.physicBody.velocity : this.physicBody.velocity);
				if (this.waterHandler.inWater)
				{
					if (this.climb.isGripping)
					{
						this.playerHand.link.RemoveJointModifier(this);
						return;
					}
					float handPositionSpringMultiplier = this.creature.data.waterHandSpringMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
					this.playerHand.link.SetJointModifier(this, handPositionSpringMultiplier, 1f, 1f, 1f, this.creature.data.waterHandLocomotionVelocityCorrectionMultiplier);
				}
			}
		}

		// Token: 0x06001D65 RID: 7525 RVA: 0x000C821F File Offset: 0x000C641F
		private void OnWaterExit()
		{
			if (this.playerHand)
			{
				this.playerHand.link.RemoveJointModifier(this);
			}
			this.caster.RefreshWater();
			this.swim.OnWaterExit();
		}

		/// <summary>
		/// Play a single haptic tick on the hand. Only works if this is the player's hand
		/// </summary>
		// Token: 0x06001D66 RID: 7526 RVA: 0x000C8255 File Offset: 0x000C6455
		public void HapticTick(float intensity = 1f, bool oneFrameCooldown = false)
		{
			if (!this.playerHand)
			{
				return;
			}
			this.playerHand.controlHand.HapticShort(intensity, oneFrameCooldown);
		}

		/// <summary>
		/// Play a haptic clip based on an animation curve over a duration
		/// </summary>
		// Token: 0x06001D67 RID: 7527 RVA: 0x000C8277 File Offset: 0x000C6477
		public void PlayHapticClipOver(AnimationCurve curve, float duration)
		{
			if (this.playerHand)
			{
				base.StartCoroutine(this.HapticPlayer(curve, duration));
			}
		}

		// Token: 0x06001D68 RID: 7528 RVA: 0x000C8295 File Offset: 0x000C6495
		protected IEnumerator HapticPlayer(AnimationCurve curve, float duration)
		{
			float time = Time.time;
			while (Time.time - time < duration)
			{
				this.HapticTick(curve.Evaluate((Time.time - time) / duration), false);
				yield return 0;
			}
			yield break;
		}

		/// <summary>
		/// Return the velocity of the hand. Only works if this is the player's hand.
		/// </summary>
		// Token: 0x06001D69 RID: 7529 RVA: 0x000C82B4 File Offset: 0x000C64B4
		public Vector3 Velocity()
		{
			if (this.creature.isPlayer)
			{
				try
				{
					return Player.local.transform.rotation * this.playerHand.controlHand.GetHandVelocity();
				}
				catch (NullReferenceException)
				{
					return Vector3.zero;
				}
			}
			return Vector3.zero;
		}

		// Token: 0x06001D6A RID: 7530 RVA: 0x000C8318 File Offset: 0x000C6518
		public void SetFingerColliders(bool enabled)
		{
			this.palmCollider.enabled = enabled;
			foreach (RagdollHand.Finger finger in this.fingers)
			{
				finger.distal.collider.enabled = enabled;
				finger.intermediate.collider.enabled = enabled;
				finger.proximal.collider.enabled = enabled;
			}
			this.simplifiedCollider.enabled = !enabled;
		}

		// Token: 0x06001D6B RID: 7531 RVA: 0x000C83B0 File Offset: 0x000C65B0
		public override void OnRagdollEnable()
		{
			base.OnRagdollEnable();
			if (this.disabledGrabbedHandle)
			{
				this.disabledGrabbedHandle.item.gameObject.SetActive(true);
				this.Grab(this.disabledGrabbedHandle, true, false);
				this.disabledGrabbedHandle = null;
			}
		}

		// Token: 0x06001D6C RID: 7532 RVA: 0x000C83F0 File Offset: 0x000C65F0
		public override void OnRagdollDisable()
		{
			base.OnRagdollDisable();
			if (!this.creature.gameObject.activeInHierarchy && this.grabbedHandle && this.grabbedHandle.item)
			{
				this.disabledGrabbedHandle = this.grabbedHandle;
				this.UnGrab(false);
				this.disabledGrabbedHandle.item.gameObject.SetActive(false);
			}
		}

		// Token: 0x06001D6D RID: 7533 RVA: 0x000C8460 File Offset: 0x000C6660
		protected void OnRagdollStateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicStateChange, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart && this.grabbedHandle && !Ragdoll.IsPhysicalState(newState, false))
			{
				for (int i = this.grabbedHandle.handlers.Count - 1; i >= 0; i--)
				{
					if (this.grabbedHandle.handlers[i].gripInfo.type == Handle.GripInfo.Type.PlayerJoint)
					{
						this.grabbedHandle.handlers[i].UnGrab(false);
					}
				}
			}
			if (eventTime == EventTime.OnEnd)
			{
				if (this.grabbedHandle && (this.gripInfo.type == Handle.GripInfo.Type.HandJoint || this.gripInfo.type == Handle.GripInfo.Type.HandSync) && physicStateChange != Ragdoll.PhysicStateChange.None)
				{
					bool usePhysic = Ragdoll.IsPhysicalState(newState, false);
					this.grabbedHandle.Attach(this, usePhysic);
					Item item = this.grabbedHandle.item;
					if (item != null)
					{
						item.SetColliders(usePhysic, false);
					}
				}
				this.RefreshFingerMeshParent(true);
			}
		}

		// Token: 0x06001D6E RID: 7534 RVA: 0x000C8540 File Offset: 0x000C6740
		public void RefreshFingerMeshParent(bool force = false)
		{
			if (!this.poser)
			{
				this.AttachFingerMeshBoneToAnimation(force);
				return;
			}
			if (!this.ragdoll.creature.isPlayer && !this.grabbedHandle)
			{
				this.AttachFingerMeshBoneToAnimation(force);
				return;
			}
			this.AttachFingerMeshBoneToRagdoll(force);
		}

		// Token: 0x06001D6F RID: 7535 RVA: 0x000C8590 File Offset: 0x000C6790
		public void AttachFingerMeshBoneToRagdoll(bool force = false)
		{
			if (force || this.fingerMeshParentedToAnim)
			{
				foreach (RagdollHand.Finger finger in this.fingers)
				{
					finger.proximal.mesh.SetParentOrigin(finger.proximal.collider.transform);
					finger.intermediate.mesh.SetParentOrigin(finger.intermediate.collider.transform);
					finger.distal.mesh.SetParentOrigin(finger.distal.collider.transform);
				}
				this.fingerMeshParentedToAnim = false;
			}
		}

		// Token: 0x06001D70 RID: 7536 RVA: 0x000C8654 File Offset: 0x000C6854
		public void AttachFingerMeshBoneToAnimation(bool force = false)
		{
			if (force || !this.fingerMeshParentedToAnim)
			{
				foreach (RagdollHand.Finger finger in this.fingers)
				{
					finger.proximal.mesh.SetParentOrigin(finger.proximal.animation);
					finger.intermediate.mesh.SetParentOrigin(finger.intermediate.animation);
					finger.distal.mesh.SetParentOrigin(finger.distal.animation);
				}
				this.fingerMeshParentedToAnim = true;
			}
		}

		// Token: 0x06001D71 RID: 7537 RVA: 0x000C8708 File Offset: 0x000C6908
		public void ResetGripPositionAndRotation()
		{
			this.grip.localPosition = this.orgGripLocalPosition;
			this.grip.localRotation = this.orgGripLocalRotation;
			this.grip.localScale = Vector3.one;
		}

		// Token: 0x06001D72 RID: 7538 RVA: 0x000C873C File Offset: 0x000C693C
		public float GetArmLenghtRatio(bool XZOnly)
		{
			Vector3 bonePos = this.bone.animation.position;
			if (XZOnly)
			{
				return this.upperArmPart.bone.animation.InverseTransformPoint(new Vector3(bonePos.x, this.upperArmPart.bone.animation.position.y, bonePos.z)).magnitude;
			}
			return this.upperArmPart.bone.animation.InverseTransformPoint(bonePos).magnitude / this.creature.morphology.armsLength;
		}

		// Token: 0x06001D73 RID: 7539 RVA: 0x000C87D5 File Offset: 0x000C69D5
		private void OnTriggerEnter(Collider other)
		{
			this.OnInteractorTriggerEnter(other);
			this.climb.OnTriggerEnter(other);
		}

		// Token: 0x06001D74 RID: 7540 RVA: 0x000C87EA File Offset: 0x000C69EA
		private void OnTriggerStay(Collider other)
		{
			this.OnInteractorTriggerStay(other);
			this.climb.OnTriggerStay(other);
		}

		// Token: 0x06001D75 RID: 7541 RVA: 0x000C87FF File Offset: 0x000C69FF
		private void OnTriggerExit(Collider other)
		{
			this.OnInteractorTriggerExit(other);
		}

		// Token: 0x06001D76 RID: 7542 RVA: 0x000C8808 File Offset: 0x000C6A08
		protected void OnCollisionEnter(Collision collision)
		{
			if (!this.ragdoll.creature.isPlayer)
			{
				return;
			}
			this.climb.OnCollisionEnter(collision);
		}

		// Token: 0x06001D77 RID: 7543 RVA: 0x000C8829 File Offset: 0x000C6A29
		protected void OnCollisionStay(Collision collision)
		{
			if (!this.ragdoll.creature.isPlayer)
			{
				return;
			}
			this.climb.OnCollisionStay(collision);
		}

		// Token: 0x06001D78 RID: 7544 RVA: 0x000C884A File Offset: 0x000C6A4A
		protected void UpdateClimb()
		{
			this.climb.Update();
		}

		// Token: 0x140000DE RID: 222
		// (add) Token: 0x06001D79 RID: 7545 RVA: 0x000C8858 File Offset: 0x000C6A58
		// (remove) Token: 0x06001D7A RID: 7546 RVA: 0x000C8890 File Offset: 0x000C6A90
		public event RagdollHand.TouchHoverEvent OnTouchNewInteractable;

		// Token: 0x140000DF RID: 223
		// (add) Token: 0x06001D7B RID: 7547 RVA: 0x000C88C8 File Offset: 0x000C6AC8
		// (remove) Token: 0x06001D7C RID: 7548 RVA: 0x000C8900 File Offset: 0x000C6B00
		public event RagdollHand.HoverEndEvent OnStopTouchInteractable;

		// Token: 0x140000E0 RID: 224
		// (add) Token: 0x06001D7D RID: 7549 RVA: 0x000C8938 File Offset: 0x000C6B38
		// (remove) Token: 0x06001D7E RID: 7550 RVA: 0x000C8970 File Offset: 0x000C6B70
		public event RagdollHand.GrabEvent OnGrabEvent;

		// Token: 0x140000E1 RID: 225
		// (add) Token: 0x06001D7F RID: 7551 RVA: 0x000C89A8 File Offset: 0x000C6BA8
		// (remove) Token: 0x06001D80 RID: 7552 RVA: 0x000C89E0 File Offset: 0x000C6BE0
		public event RagdollHand.UnGrabEvent OnUnGrabEvent;

		// Token: 0x06001D81 RID: 7553 RVA: 0x000C8A18 File Offset: 0x000C6C18
		private void OnInteractorTriggerEnter(Collider other)
		{
			if (other.gameObject.layer == GameManager.GetLayer(LayerName.TouchObject) || other.gameObject.layer == GameManager.GetLayer(LayerName.Touch))
			{
				this.pauseCheck = true;
				Interactable interactable;
				if (other.TryGetComponent<Interactable>(out interactable) && interactable.CanTouch(this) && !this.TouchedInteractablesContain(interactable))
				{
					Holder holder = interactable as Holder;
					if (holder != null)
					{
						Item item = holder.parentItem;
						if (item != null)
						{
							UnityEngine.Object x = item;
							Handle handle = this.grabbedHandle;
							if (x == ((handle != null) ? handle.item : null))
							{
								goto IL_B8;
							}
						}
					}
					Interactable.InteractionResult interactionResult = interactable.CheckInteraction(this);
					if (interactionResult.isInteractable || interactionResult.showHint)
					{
						this.touchedInteractables.Add(interactable);
					}
					RagdollHand.TouchHoverEvent onTouchNewInteractable = this.OnTouchNewInteractable;
					if (onTouchNewInteractable != null)
					{
						onTouchNewInteractable(this.side, interactable, interactionResult);
					}
				}
				IL_B8:
				this.CheckTouch();
			}
		}

		// Token: 0x06001D82 RID: 7554 RVA: 0x000C8AE4 File Offset: 0x000C6CE4
		private void OnInteractorTriggerExit(Collider other)
		{
			if (other.gameObject.layer == GameManager.GetLayer(LayerName.TouchObject) || other.gameObject.layer == GameManager.GetLayer(LayerName.Touch))
			{
				Interactable interactable;
				if (other.TryGetComponent<Interactable>(out interactable) && this.TouchedInteractablesContain(interactable))
				{
					this.touchedInteractables.Remove(interactable);
					RagdollHand.HoverEndEvent onStopTouchInteractable = this.OnStopTouchInteractable;
					if (onStopTouchInteractable != null)
					{
						onStopTouchInteractable(this.side, interactable);
					}
				}
				this.CheckTouch();
			}
		}

		// Token: 0x06001D83 RID: 7555 RVA: 0x000C8B58 File Offset: 0x000C6D58
		private void OnInteractorTriggerStay(Collider other)
		{
			if (other.gameObject.layer == GameManager.GetLayer(LayerName.TouchObject) || other.gameObject.layer == GameManager.GetLayer(LayerName.Touch))
			{
				this.pauseCheck = true;
				if (this.forceTriggerCheck)
				{
					this.OnTriggerEnter(other);
					if (!this.ragdoll.creature.isPlayer)
					{
						this.forceTriggerCheck = false;
						return;
					}
					if (this.disableTriggerCheck == null)
					{
						this.disableTriggerCheck = base.StartCoroutine(this.WaitDisableTriggerCheck());
						return;
					}
				}
				else
				{
					this.CheckTouch();
				}
			}
		}

		// Token: 0x06001D84 RID: 7556 RVA: 0x000C8BDD File Offset: 0x000C6DDD
		protected IEnumerator WaitDisableTriggerCheck()
		{
			yield return Yielders.EndOfFrame;
			this.forceTriggerCheck = false;
			this.disableTriggerCheck = null;
			yield break;
		}

		// Token: 0x06001D85 RID: 7557 RVA: 0x000C8BEC File Offset: 0x000C6DEC
		protected bool TouchedInteractablesContain(Interactable interactable)
		{
			using (List<Interactable>.Enumerator enumerator = this.touchedInteractables.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current == interactable)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001D86 RID: 7558 RVA: 0x000C8C48 File Offset: 0x000C6E48
		private void CheckTouch()
		{
			if (this.touchedInteractables.Count > 0)
			{
				SpellCaster spellCaster = this.caster;
				UnityEngine.Object x;
				if (spellCaster == null)
				{
					x = null;
				}
				else
				{
					SpellTelekinesis telekinesis = spellCaster.telekinesis;
					x = ((telekinesis != null) ? telekinesis.targetHandle : null);
				}
				if (x != null)
				{
					this.caster.telekinesis.StopTargeting();
				}
				for (int i = this.touchedInteractables.Count - 1; i >= 0; i--)
				{
					if (this.touchedInteractables[i] == null)
					{
						this.touchedInteractables.RemoveAt(i);
					}
				}
			}
			if (this.touchedInteractables.Count == 0)
			{
				if (this.nearestInteractable)
				{
					this.nearestInteractable.OnTouchEnd(this);
					this.nearestInteractable = null;
					return;
				}
			}
			else if (this.touchedInteractables.Count == 1)
			{
				if (this.nearestInteractable != this.touchedInteractables[0])
				{
					if (this.nearestInteractable)
					{
						this.nearestInteractable.OnTouchEnd(this);
					}
					this.nearestInteractable = this.touchedInteractables[0];
					if (this.nearestInteractable)
					{
						this.nearestInteractable.OnTouchStart(this);
					}
				}
				if (this.nearestInteractable)
				{
					this.nearestInteractable.OnTouchStay(this);
					return;
				}
			}
			else
			{
				Interactable newNearestTouchZone = this.touchedInteractables[0];
				if (this.touchedInteractables.Count > 1)
				{
					float currentClosestDistance = (((newNearestTouchZone.axisLength > 0f) ? newNearestTouchZone.GetNearestPositionAlongAxis(this.GetReferencePoint(newNearestTouchZone).position) : newNearestTouchZone.transform.position) - this.GetReferencePoint(newNearestTouchZone).position).sqrMagnitude + newNearestTouchZone.artificialDistance;
					int count = this.touchedInteractables.Count;
					for (int j = 1; j < count; j++)
					{
						Interactable compare = this.touchedInteractables[j];
						if (compare.data == null || !compare.data.ignoreWhenTouchConflict)
						{
							float compareDistance = (((compare.axisLength > 0f) ? compare.GetNearestPositionAlongAxis(this.GetReferencePoint(compare).position) : compare.transform.position) - this.GetReferencePoint(compare).position).sqrMagnitude + compare.artificialDistance;
							if (compareDistance < currentClosestDistance)
							{
								newNearestTouchZone = compare;
								currentClosestDistance = compareDistance;
							}
						}
					}
				}
				if (newNearestTouchZone != this.nearestInteractable)
				{
					Interactable interactable = this.nearestInteractable;
					if (interactable != null)
					{
						interactable.OnTouchEnd(this);
					}
					this.nearestInteractable = newNearestTouchZone;
					this.nearestInteractable.OnTouchStart(this);
				}
				foreach (Interactable interactable2 in this.touchedInteractables)
				{
					interactable2.OnTouchStay(this);
				}
			}
		}

		// Token: 0x06001D87 RID: 7559 RVA: 0x000C8F18 File Offset: 0x000C7118
		public void RefreshTouch()
		{
			Interactable interactable = this.nearestInteractable;
			if (interactable != null)
			{
				interactable.OnTouchEnd(this);
			}
			Interactable interactable2 = this.nearestInteractable;
			if (interactable2 == null)
			{
				return;
			}
			interactable2.OnTouchStart(this);
		}

		// Token: 0x06001D88 RID: 7560 RVA: 0x000C8F3D File Offset: 0x000C713D
		protected Transform GetReferencePoint(Interactable interactable)
		{
			if (interactable.data != null)
			{
				if (interactable.data.referencePoint == InteractableData.ReferencePoint.Grip)
				{
					return this.grip;
				}
				if (interactable.data.referencePoint == InteractableData.ReferencePoint.IndexTip)
				{
					return this.fingerIndex.tip;
				}
			}
			return base.transform;
		}

		// Token: 0x06001D89 RID: 7561 RVA: 0x000C8F7C File Offset: 0x000C717C
		public void CheckInteractablesStillTouched()
		{
			bool removedSomething = false;
			int i;
			Action <>9__0;
			int j;
			for (i = this.touchedInteractables.Count - 1; i >= 0; i = j - 1)
			{
				Interactable interactable = this.touchedInteractables[i];
				if (interactable == null || this.InteractableStoppedTouching(interactable, false))
				{
					Interactable interactable2 = interactable;
					Action remove;
					if ((remove = <>9__0) == null)
					{
						remove = (<>9__0 = delegate()
						{
							this.touchedInteractables.RemoveAt(i);
						});
					}
					this.RemoveInteractable(interactable2, remove);
					removedSomething = true;
				}
				j = i;
			}
			if (removedSomething)
			{
				this.CheckTouch();
			}
		}

		// Token: 0x06001D8A RID: 7562 RVA: 0x000C9020 File Offset: 0x000C7220
		public bool InteractableStoppedTouching(Interactable interactable, bool remove = true)
		{
			if (!interactable)
			{
				return false;
			}
			if (!this.touchedInteractables.Contains(interactable))
			{
				return false;
			}
			bool shouldRemove = interactable.touchCollider == null || !interactable.touchCollider.enabled || !interactable.touchCollider.gameObject.activeInHierarchy;
			if (!shouldRemove)
			{
				shouldRemove = !this.touchCollider.bounds.Intersects(interactable.touchCollider.bounds);
			}
			if (shouldRemove)
			{
				if (remove)
				{
					this.RemoveInteractable(interactable, delegate
					{
						this.touchedInteractables.Remove(interactable);
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001D8B RID: 7563 RVA: 0x000C90F4 File Offset: 0x000C72F4
		public void RemoveInteractable(Interactable interactable, Action remove)
		{
			remove();
			if (interactable)
			{
				interactable.OnTouchEnd(this);
				RagdollHand.HoverEndEvent onStopTouchInteractable = this.OnStopTouchInteractable;
				if (onStopTouchInteractable != null)
				{
					onStopTouchInteractable(this.side, interactable);
				}
				if (interactable == this.nearestInteractable)
				{
					this.nearestInteractable = null;
				}
			}
		}

		// Token: 0x06001D8C RID: 7564 RVA: 0x000C9144 File Offset: 0x000C7344
		public virtual void ClearTouch()
		{
			foreach (Interactable interactable in this.touchedInteractables)
			{
				interactable.OnTouchEnd(this);
			}
			this.touchedInteractables.Clear();
			this.nearestInteractable = null;
			this.forceTriggerCheck = true;
		}

		// Token: 0x06001D8D RID: 7565 RVA: 0x000C91B0 File Offset: 0x000C73B0
		public virtual void SetBlockGrab(bool active, bool blockTK = true)
		{
			if (blockTK)
			{
				SpellCaster spellCaster = this.caster;
				if (((spellCaster != null) ? spellCaster.telekinesis : null) != null)
				{
					this.caster.telekinesis.tkBlocked = active;
				}
			}
			if (active)
			{
				this.ClearTouch();
				this.touchCollider.enabled = false;
				this.grabBlocked = true;
				return;
			}
			this.touchCollider.enabled = true;
			this.grabBlocked = false;
		}

		// Token: 0x06001D8E RID: 7566 RVA: 0x000C9218 File Offset: 0x000C7418
		public virtual bool IsGrabbingOrTK()
		{
			Handle handle;
			return this.IsGrabbingOrTK(out handle);
		}

		// Token: 0x06001D8F RID: 7567 RVA: 0x000C9230 File Offset: 0x000C7430
		public virtual bool IsGrabbingOrTK(out Handle handle)
		{
			handle = this.grabbedHandle;
			if (handle != null)
			{
				return true;
			}
			SpellCaster spellCaster = this.caster;
			Handle handle2;
			if (spellCaster == null)
			{
				handle2 = null;
			}
			else
			{
				SpellTelekinesis telekinesis = spellCaster.telekinesis;
				handle2 = ((telekinesis != null) ? telekinesis.catchedHandle : null);
			}
			handle = handle2;
			return handle != null;
		}

		// Token: 0x06001D90 RID: 7568 RVA: 0x000C9280 File Offset: 0x000C7480
		public virtual bool IsGrabbingOrTK(Handle handle)
		{
			Handle grabbedOrTKedHandle;
			return this.IsGrabbingOrTK(out grabbedOrTKedHandle) && grabbedOrTKedHandle == handle;
		}

		// Token: 0x06001D91 RID: 7569 RVA: 0x000C92A0 File Offset: 0x000C74A0
		public virtual bool IsGrabbingOrTK(ThunderEntity entity)
		{
			Handle handle;
			if (!this.IsGrabbingOrTK(out handle))
			{
				return false;
			}
			HandleRagdoll ragdollHandle = handle as HandleRagdoll;
			return (ragdollHandle != null && ragdollHandle.ragdollPart.ragdoll.creature == entity) || handle.item == entity;
		}

		// Token: 0x06001D92 RID: 7570 RVA: 0x000C92F0 File Offset: 0x000C74F0
		public virtual bool IsTK()
		{
			Handle handle;
			return this.IsTK(out handle);
		}

		// Token: 0x06001D93 RID: 7571 RVA: 0x000C9305 File Offset: 0x000C7505
		public virtual bool IsTK(out Handle handle)
		{
			SpellCaster spellCaster = this.caster;
			Handle handle2;
			if (spellCaster == null)
			{
				handle2 = null;
			}
			else
			{
				SpellTelekinesis telekinesis = spellCaster.telekinesis;
				handle2 = ((telekinesis != null) ? telekinesis.catchedHandle : null);
			}
			handle = handle2;
			return handle != null;
		}

		// Token: 0x06001D94 RID: 7572 RVA: 0x000C9330 File Offset: 0x000C7530
		public virtual ThunderEntity GetAttachedEntity()
		{
			Handle handle = this.grabbedHandle;
			if (((handle != null) ? handle.item : null) != null)
			{
				return this.grabbedHandle.item;
			}
			if (this.climb.isGripping)
			{
				Item result;
				if ((result = this.climb.gripItem) == null)
				{
					RagdollPart gripRagdollPart = this.climb.gripRagdollPart;
					Item item;
					if (gripRagdollPart == null)
					{
						item = null;
					}
					else
					{
						Ragdoll ragdoll = gripRagdollPart.ragdoll;
						item = ((ragdoll != null) ? ragdoll.creature : null);
					}
					if ((result = item) == null)
					{
						PhysicBody gripPhysicBody = this.climb.gripPhysicBody;
						if (gripPhysicBody == null)
						{
							return null;
						}
						result = gripPhysicBody.gameObject.GetComponentInParent<ThunderEntity>();
					}
				}
				return result;
			}
			HandleRagdoll handleRagdoll = this.grabbedHandle as HandleRagdoll;
			if (handleRagdoll != null)
			{
				return handleRagdoll.ragdollPart.ragdoll.creature;
			}
			RagdollPart affixedPart = null;
			Handle handle2 = this.grabbedHandle;
			bool flag;
			if (handle2 == null)
			{
				flag = false;
			}
			else
			{
				PhysicBody physicBody = handle2.physicBody;
				bool? flag2;
				if (physicBody == null)
				{
					flag2 = null;
				}
				else
				{
					GameObject gameObject = physicBody.gameObject;
					flag2 = ((gameObject != null) ? new bool?(gameObject.TryGetComponent<RagdollPart>(out affixedPart)) : null);
				}
				bool? flag3 = flag2;
				bool flag4 = true;
				flag = (flag3.GetValueOrDefault() == flag4 & flag3 != null);
			}
			if (flag)
			{
				return affixedPart.ragdoll.creature;
			}
			return null;
		}

		// Token: 0x06001D95 RID: 7573 RVA: 0x000C944B File Offset: 0x000C764B
		public virtual bool IsAttachedToEntity(out ThunderEntity heldEntity)
		{
			heldEntity = this.GetAttachedEntity();
			return heldEntity != null;
		}

		// Token: 0x06001D96 RID: 7574 RVA: 0x000C9460 File Offset: 0x000C7660
		public virtual bool IsAttachedToEntity(ThunderEntity entity)
		{
			ThunderEntity attachedEntity;
			return this.IsAttachedToEntity(out attachedEntity) && attachedEntity == entity;
		}

		// Token: 0x06001D97 RID: 7575 RVA: 0x000C9480 File Offset: 0x000C7680
		public virtual bool TryAction(Interactable.Action action)
		{
			if (this.nearestInteractable && this.nearestInteractable.TryTouchAction(this, action))
			{
				return true;
			}
			if (this.grabbedHandle && this.grabbedHandle.HeldActionAvailable(this, action))
			{
				this.grabbedHandle.HeldAction(this, action);
				return true;
			}
			return false;
		}

		// Token: 0x06001D98 RID: 7576 RVA: 0x000C94D7 File Offset: 0x000C76D7
		public virtual void EditorGrab()
		{
			this.Grab(this.editorGrabTarget);
		}

		// Token: 0x06001D99 RID: 7577 RVA: 0x000C94E5 File Offset: 0x000C76E5
		public virtual void EditorUngrab()
		{
			if (this.grabbedHandle)
			{
				this.UnGrab(true);
			}
		}

		/// <summary>
		/// Grab object relative to current hand grip position and rotation. Warning: Need to be used by local player only to be accurate!
		/// </summary>
		/// <param name="handle">Handle to grab</param>
		// Token: 0x06001D9A RID: 7578 RVA: 0x000C94FC File Offset: 0x000C76FC
		public virtual void GrabRelative(Handle handle, bool withTrigger = false)
		{
			this.Grab(handle, handle.GetNearestOrientation(this.grip, this.side), (handle.axisLength > 0f) ? handle.GetNearestAxisPosition(this.grip.position) : 0f, handle.data.alwaysTeleportToHand, withTrigger);
		}

		// Token: 0x06001D9B RID: 7579 RVA: 0x000C9553 File Offset: 0x000C7753
		public virtual void Grab(Handle handle)
		{
			this.Grab(handle, handle.GetDefaultOrientation(this.side), handle.GetDefaultAxisLocalPosition(), handle.data.alwaysTeleportToHand, false);
		}

		// Token: 0x06001D9C RID: 7580 RVA: 0x000C957A File Offset: 0x000C777A
		public virtual void Grab(Handle handle, bool teleportToHand, bool withTrigger = false)
		{
			this.Grab(handle, handle.GetDefaultOrientation(this.side), handle.GetDefaultAxisLocalPosition(), teleportToHand, withTrigger);
		}

		// Token: 0x06001D9D RID: 7581 RVA: 0x000C9598 File Offset: 0x000C7798
		public virtual void Grab(Handle handle, HandlePose orientation, float axisPosition, bool teleportToHand = false, bool withTrigger = false)
		{
			if (handle.handlers.Contains(this))
			{
				Debug.LogError("Trying to Grab two time with the same ragdollHand!");
				return;
			}
			if (orientation == null)
			{
				Debug.LogError("Trying to grab a handle with no orientation. Cannot grab " + handle.name + "!");
				return;
			}
			this.grabbedWithTrigger = withTrigger;
			if (this.OnGrabEvent != null)
			{
				this.OnGrabEvent(this.side, handle, axisPosition, orientation, EventTime.OnStart);
			}
			if (this.playerHand != null)
			{
				WheelMenu sideWheelMenu = (this.side == Side.Left) ? WheelMenuSpell.left : WheelMenuSpell.right;
				if (sideWheelMenu.isShown && !handle.data.allowSpellMenu)
				{
					sideWheelMenu.Hide();
				}
			}
			handle.OnGrab(this, axisPosition, orientation, teleportToHand);
			this.RefreshTwoHanded();
			this.ClearTouch();
			if (this.handOverlapCoroutine != null)
			{
				base.StopCoroutine(this.handOverlapCoroutine);
			}
			this.creature.UpdateHeldImbues();
			this.RefreshFingerMeshParent(false);
			RagdollHand.GrabEvent onGrabEvent = this.OnGrabEvent;
			if (onGrabEvent == null)
			{
				return;
			}
			onGrabEvent(this.side, handle, axisPosition, orientation, EventTime.OnEnd);
		}

		// Token: 0x06001D9E RID: 7582 RVA: 0x000C96A0 File Offset: 0x000C78A0
		public virtual void UnGrab(bool throwing)
		{
			if (this.grabbedHandle)
			{
				this.grabbedWithTrigger = false;
				Handle tmpHandle = this.grabbedHandle;
				if (tmpHandle.item && base.gameObject.activeInHierarchy)
				{
					base.StartCoroutine(this.PreventPierceSelf(tmpHandle.item));
				}
				if (this.OnUnGrabEvent != null)
				{
					this.OnUnGrabEvent(this.side, tmpHandle, throwing, EventTime.OnStart);
				}
				tmpHandle.OnUnGrab(this, throwing);
				this.RefreshTwoHanded();
				this.ClearTouch();
				if (this.playerHand && tmpHandle.physicBody && !this.tempIgnoreOverlap)
				{
					if (this.handOverlapCoroutine != null)
					{
						base.StopCoroutine(this.handOverlapCoroutine);
					}
					this.handOverlapCoroutine = base.StartCoroutine(this.HandOverlapCoroutine(tmpHandle));
				}
				this.lastSliding = null;
				this.creature.UpdateHeldImbues();
				this.RefreshFingerMeshParent(false);
				if (this.OnUnGrabEvent != null)
				{
					this.OnUnGrabEvent(this.side, tmpHandle, throwing, EventTime.OnEnd);
				}
				if (throwing)
				{
					this.creature.InvokeOnThrowEvent(this, tmpHandle);
					return;
				}
			}
			else
			{
				Debug.LogError("Trying to ungrab nothing!");
			}
		}

		// Token: 0x06001D9F RID: 7583 RVA: 0x000C97C1 File Offset: 0x000C79C1
		private IEnumerator PreventPierceSelf(Item item)
		{
			item.PreventPenetration(this);
			yield return Yielders.ForSeconds(1f);
			item.AllowPenetration(this);
			yield break;
		}

		// Token: 0x06001DA0 RID: 7584 RVA: 0x000C97D7 File Offset: 0x000C79D7
		private IEnumerator HandOverlapCoroutine(Handle handle)
		{
			if (handle.item)
			{
				RagdollPart.Type grabbedTypes = this.type | this.parentPart.type;
				for (int i = handle.item.handlers.Count - 1; i >= 0; i--)
				{
					RagdollHand handler = handle.item.handlers[i];
					if (!(handler.ragdoll != this.ragdoll))
					{
						grabbedTypes = (grabbedTypes | handler.type | handler.parentPart.type);
					}
				}
				handle.item.IgnoreRagdollCollision(this.ragdoll, ~grabbedTypes);
			}
			else if (handle is HandleRagdoll)
			{
				foreach (Collider collider in this.colliderGroup.colliders)
				{
					(handle as HandleRagdoll).ragdollPart.ragdoll.IgnoreCollision(collider, true, (RagdollPart.Type)0);
				}
			}
			foreach (Collider handleCollider in handle.handOverlapColliders)
			{
				foreach (Collider handCollider in this.colliderGroup.colliders)
				{
					Physics.IgnoreCollision(handleCollider, handCollider, true);
				}
			}
			LayerMask ungrabLayerMask = this.playerHand.collisionUngrabLayerMask;
			yield return Yielders.ForSeconds(this.collisionUngrabMinDelay);
			bool isOverlapped = true;
			while (isOverlapped)
			{
				isOverlapped = Physics.CheckSphere(this.grip.position, this.collisionUngrabRadius, ungrabLayerMask, QueryTriggerInteraction.Ignore);
				yield return Yielders.FixedUpdate;
			}
			if (handle.item)
			{
				handle.item.RefreshCollision(false);
			}
			else if (handle is HandleRagdoll)
			{
				foreach (Collider collider2 in this.colliderGroup.colliders)
				{
					(handle as HandleRagdoll).ragdollPart.ragdoll.IgnoreCollision(collider2, false, (RagdollPart.Type)0);
				}
			}
			using (List<Collider>.Enumerator enumerator = handle.handOverlapColliders.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Collider handleCollider2 = enumerator.Current;
					foreach (Collider handCollider2 in this.colliderGroup.colliders)
					{
						Physics.IgnoreCollision(handleCollider2, handCollider2, false);
					}
				}
				yield break;
			}
			yield break;
		}

		// Token: 0x06001DA1 RID: 7585 RVA: 0x000C97F0 File Offset: 0x000C79F0
		public virtual bool TryRelease()
		{
			if (this.grabbedHandle)
			{
				Handle tmpHandle = this.grabbedHandle;
				this.UnGrab(false);
				if (tmpHandle.item)
				{
					tmpHandle.item.isFlying = false;
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001DA2 RID: 7586 RVA: 0x000C9834 File Offset: 0x000C7A34
		protected virtual void RefreshTwoHanded()
		{
			if (this.grabbedHandle)
			{
				RagdollHand ragdollHand = this.otherHand;
				if ((ragdollHand != null) ? ragdollHand.grabbedHandle : null)
				{
					if (this.grabbedHandle.item && this.otherHand.grabbedHandle.item && this.grabbedHandle.item == this.otherHand.grabbedHandle.item)
					{
						this.isHandlingSameObject = (this.otherHand.isHandlingSameObject = true);
						return;
					}
					if (this.grabbedHandle == this.otherHand.grabbedHandle)
					{
						this.isHandlingSameObject = (this.otherHand.isHandlingSameObject = true);
						return;
					}
				}
			}
			this.isHandlingSameObject = false;
			if (this.otherHand)
			{
				this.otherHand.isHandlingSameObject = false;
			}
		}

		// Token: 0x04001BEC RID: 7148
		[Header("Hand")]
		public Side side;

		// Token: 0x04001BED RID: 7149
		public RagdollPart lowerArmPart;

		// Token: 0x04001BEE RID: 7150
		public RagdollPart upperArmPart;

		// Token: 0x04001BEF RID: 7151
		public bool meshFixedScale = true;

		// Token: 0x04001BF0 RID: 7152
		public Vector3 meshGlobalScale = Vector3.one;

		// Token: 0x04001BF1 RID: 7153
		public Vector3 axisThumb = Vector3.up;

		// Token: 0x04001BF2 RID: 7154
		public Vector3 axisPalm = Vector3.left;

		// Token: 0x04001BF3 RID: 7155
		public Collider touchCollider;

		// Token: 0x04001BF4 RID: 7156
		public WristStats wristStats;

		// Token: 0x04001BF5 RID: 7157
		public RagdollHandPoser poser;

		// Token: 0x04001BF6 RID: 7158
		[Header("Fingers")]
		public RagdollHand.Finger fingerThumb = new RagdollHand.Finger();

		// Token: 0x04001BF7 RID: 7159
		public RagdollHand.Finger fingerIndex = new RagdollHand.Finger();

		// Token: 0x04001BF8 RID: 7160
		public RagdollHand.Finger fingerMiddle = new RagdollHand.Finger();

		// Token: 0x04001BF9 RID: 7161
		public RagdollHand.Finger fingerRing = new RagdollHand.Finger();

		// Token: 0x04001BFA RID: 7162
		public RagdollHand.Finger fingerLittle = new RagdollHand.Finger();

		// Token: 0x04001BFB RID: 7163
		public List<RagdollHand.Finger> fingers = new List<RagdollHand.Finger>();

		// Token: 0x04001BFC RID: 7164
		public Collider palmCollider;

		// Token: 0x04001BFD RID: 7165
		public Collider simplifiedCollider;

		// Token: 0x04001C01 RID: 7169
		[NonSerialized]
		public bool fingerMeshParentedToAnim;

		// Token: 0x04001C02 RID: 7170
		[NonSerialized]
		public bool tempIgnoreOverlap;

		// Token: 0x04001C03 RID: 7171
		private List<Collider> foreArmColliders;

		// Token: 0x04001C04 RID: 7172
		[NonSerialized]
		public RagdollHand otherHand;

		// Token: 0x04001C05 RID: 7173
		[NonSerialized]
		public Creature creature;

		// Token: 0x04001C06 RID: 7174
		[NonSerialized]
		public Transform grip;

		// Token: 0x04001C07 RID: 7175
		[NonSerialized]
		public SpellCaster caster;

		// Token: 0x04001C08 RID: 7176
		[NonSerialized]
		public PlayerHand playerHand;

		// Token: 0x04001C09 RID: 7177
		public RagdollHandClimb climb;

		// Token: 0x04001C0A RID: 7178
		public RagdollHandSwim swim;

		// Token: 0x04001C0B RID: 7179
		protected Vector3 orgGripLocalPosition;

		// Token: 0x04001C0C RID: 7180
		protected Quaternion orgGripLocalRotation;

		// Token: 0x04001C0D RID: 7181
		[NonSerialized]
		public WaterHandler waterHandler;

		// Token: 0x04001C0E RID: 7182
		[NonSerialized]
		public RagdollHand.ControlPose controlPose;

		// Token: 0x04001C10 RID: 7184
		public static float punchDetectionAngleThreshold = 20f;

		// Token: 0x04001C11 RID: 7185
		public static float punchDetectionThreshold = 3f;

		// Token: 0x04001C12 RID: 7186
		public static float punchStopThreshold = 0.5f;

		// Token: 0x04001C13 RID: 7187
		[NonSerialized]
		public bool punching;

		// Token: 0x04001C14 RID: 7188
		[Header("Interactor")]
		public Interactable nearestInteractable;

		// Token: 0x04001C15 RID: 7189
		public List<Interactable> touchedInteractables;

		// Token: 0x04001C16 RID: 7190
		[NonSerialized]
		public bool grabBlocked;

		// Token: 0x04001C17 RID: 7191
		[NonSerialized]
		public bool grabbedWithTrigger;

		// Token: 0x04001C18 RID: 7192
		public Handle grabbedHandle;

		// Token: 0x04001C19 RID: 7193
		public Handle.GripInfo gripInfo;

		// Token: 0x04001C1A RID: 7194
		public Handle lastSliding;

		// Token: 0x04001C1B RID: 7195
		public Handle lastHandle;

		// Token: 0x04001C1C RID: 7196
		public bool isHandlingSameObject;

		// Token: 0x04001C1D RID: 7197
		public float collisionUngrabRadius = 0.13f;

		// Token: 0x04001C1E RID: 7198
		public float collisionUngrabMinDelay = 0.25f;

		// Token: 0x04001C1F RID: 7199
		protected Coroutine handOverlapCoroutine;

		// Token: 0x04001C20 RID: 7200
		protected bool forceTriggerCheck;

		// Token: 0x04001C21 RID: 7201
		protected Coroutine disableTriggerCheck;

		// Token: 0x04001C22 RID: 7202
		protected bool pauseCheck;

		// Token: 0x04001C23 RID: 7203
		protected Handle disabledGrabbedHandle;

		// Token: 0x04001C24 RID: 7204
		public Handle editorGrabTarget;

		// Token: 0x02000907 RID: 2311
		// (Invoke) Token: 0x0600423D RID: 16957
		public delegate void PunchEvent(RagdollHand hand, Vector3 velocity);

		// Token: 0x02000908 RID: 2312
		// (Invoke) Token: 0x06004241 RID: 16961
		public delegate void PunchHitEvent(RagdollHand hand, CollisionInstance hit, bool fist);

		// Token: 0x02000909 RID: 2313
		[Serializable]
		public class Finger
		{
			// Token: 0x04004364 RID: 17252
			public RagdollHand.Finger.Bone proximal = new RagdollHand.Finger.Bone();

			// Token: 0x04004365 RID: 17253
			public RagdollHand.Finger.Bone intermediate = new RagdollHand.Finger.Bone();

			// Token: 0x04004366 RID: 17254
			public RagdollHand.Finger.Bone distal = new RagdollHand.Finger.Bone();

			// Token: 0x04004367 RID: 17255
			public Transform tip;

			// Token: 0x02000BE4 RID: 3044
			[Serializable]
			public class Bone
			{
				// Token: 0x04004D3E RID: 19774
				public Transform mesh;

				// Token: 0x04004D3F RID: 19775
				public Transform animation;

				// Token: 0x04004D40 RID: 19776
				public CapsuleCollider collider;

				// Token: 0x04004D41 RID: 19777
				public Transform colliderTransform;
			}
		}

		// Token: 0x0200090A RID: 2314
		public enum ControlPose
		{
			// Token: 0x04004369 RID: 17257
			HandPose,
			// Token: 0x0400436A RID: 17258
			Open,
			// Token: 0x0400436B RID: 17259
			Point,
			// Token: 0x0400436C RID: 17260
			Trigger,
			// Token: 0x0400436D RID: 17261
			Fist
		}

		// Token: 0x0200090B RID: 2315
		// (Invoke) Token: 0x06004246 RID: 16966
		public delegate void ControlPoseChangeEvent(Side side, RagdollHand.ControlPose previousPose, RagdollHand.ControlPose newPose);

		// Token: 0x0200090C RID: 2316
		public class GrabState
		{
			// Token: 0x06004249 RID: 16969 RVA: 0x0018CDF0 File Offset: 0x0018AFF0
			public GrabState(RagdollHand ragdollHand)
			{
				this.ragdollHand = ragdollHand;
				this.handle = ragdollHand.grabbedHandle;
				this.gripAxisPosition = ((ragdollHand.gripInfo != null) ? ragdollHand.gripInfo.axisPosition : 0f);
				this.orientation = ((ragdollHand.gripInfo != null) ? ragdollHand.gripInfo.orientation : null);
			}

			// Token: 0x0400436E RID: 17262
			public RagdollHand ragdollHand;

			// Token: 0x0400436F RID: 17263
			public Handle handle;

			// Token: 0x04004370 RID: 17264
			public float gripAxisPosition;

			// Token: 0x04004371 RID: 17265
			public HandlePose orientation;
		}

		// Token: 0x0200090D RID: 2317
		// (Invoke) Token: 0x0600424B RID: 16971
		public delegate void TouchHoverEvent(Side side, Interactable interactable, Interactable.InteractionResult interactionResult);

		// Token: 0x0200090E RID: 2318
		// (Invoke) Token: 0x0600424F RID: 16975
		public delegate void HoverEndEvent(Side side, Interactable interactable);

		// Token: 0x0200090F RID: 2319
		// (Invoke) Token: 0x06004253 RID: 16979
		public delegate void GrabEvent(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime);

		// Token: 0x02000910 RID: 2320
		// (Invoke) Token: 0x06004257 RID: 16983
		public delegate void UnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime);
	}
}
