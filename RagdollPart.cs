using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Manikin;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200027D RID: 637
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/RagdollPart.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Ragdoll part")]
	[RequireComponent(typeof(CollisionHandler))]
	public class RagdollPart : ThunderBehaviour
	{
		// Token: 0x170001DA RID: 474
		// (get) Token: 0x06001DC0 RID: 7616 RVA: 0x000CAAB9 File Offset: 0x000C8CB9
		// (set) Token: 0x06001DC1 RID: 7617 RVA: 0x000CAAC1 File Offset: 0x000C8CC1
		public bool hasParent { get; protected set; }

		// Token: 0x06001DC2 RID: 7618 RVA: 0x000CAACC File Offset: 0x000C8CCC
		protected virtual void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.parentPart == null)
			{
				CharacterJoint characterJoint = base.GetComponent<CharacterJoint>();
				if (characterJoint)
				{
					this.parentPart = characterJoint.connectedBody.GetComponent<RagdollPart>();
				}
			}
			if (this.ragdolledMass < 0f)
			{
				PhysicBody pb = this.GetPhysicBody();
				if (pb != null)
				{
					this.ragdolledMass = pb.mass;
				}
			}
		}

		// Token: 0x06001DC3 RID: 7619 RVA: 0x000CAB38 File Offset: 0x000C8D38
		public void SetAllowSlice(bool allow)
		{
			this.sliceAllowed = allow;
		}

		// Token: 0x06001DC4 RID: 7620 RVA: 0x000CAB44 File Offset: 0x000C8D44
		public void SetPositionToBone()
		{
			base.transform.position = this.meshBone.position;
			base.transform.rotation = this.meshBone.rotation;
			base.transform.localScale = this.meshBone.localScale;
		}

		// Token: 0x06001DC5 RID: 7621 RVA: 0x000CAB94 File Offset: 0x000C8D94
		public void SetPositionToBoneLeaveChildren()
		{
			List<Transform> childs = new List<Transform>();
			foreach (Transform child in base.GetComponentsInChildren<Transform>())
			{
				childs.Add(child);
				child.SetParent(base.transform.parent, true);
			}
			base.transform.position = this.meshBone.position;
			base.transform.rotation = this.meshBone.rotation;
			base.transform.localScale = this.meshBone.localScale;
			foreach (Transform transform in childs)
			{
				transform.SetParent(base.transform, true);
			}
		}

		// Token: 0x06001DC6 RID: 7622 RVA: 0x000CAC64 File Offset: 0x000C8E64
		public void IgnoreParentPart()
		{
			if (this.parentPart == null)
			{
				return;
			}
			if (this.ignoredParts == null)
			{
				this.ignoredParts = new List<RagdollPart>();
			}
			if (this.ignoredParts.Contains(this.parentPart))
			{
				return;
			}
			this.ignoredParts.Add(this.parentPart);
		}

		// Token: 0x06001DC7 RID: 7623 RVA: 0x000CACB8 File Offset: 0x000C8EB8
		public void IgnoreAllParts()
		{
			if (this.ignoredParts == null)
			{
				this.ignoredParts = new List<RagdollPart>();
			}
			this.ignoredParts.Clear();
			foreach (RagdollPart part in base.GetComponentInParent<Ragdoll>().GetComponentsInChildren<RagdollPart>())
			{
				if (!(part == this))
				{
					this.ignoredParts.Add(part);
				}
			}
		}

		// Token: 0x06001DC8 RID: 7624 RVA: 0x000CAD18 File Offset: 0x000C8F18
		public void FindBoneFromName()
		{
			this.ragdoll = base.GetComponentInParent<Ragdoll>();
			foreach (Transform child in this.ragdoll.meshRig.GetComponentsInChildren<Transform>())
			{
				if (child.name == base.name)
				{
					this.meshBone = child;
					return;
				}
			}
		}

		// Token: 0x06001DC9 RID: 7625 RVA: 0x000CAD6F File Offset: 0x000C8F6F
		public void AssignJointBodyFromParent()
		{
			if (this.parentPart != null)
			{
				CharacterJoint component = base.GetComponent<CharacterJoint>();
				if (component == null)
				{
					return;
				}
				component.SetConnectedPhysicBody(this.parentPart.physicBody ?? this.parentPart.GetPhysicBody());
			}
		}

		// Token: 0x06001DCA RID: 7626 RVA: 0x000CADA9 File Offset: 0x000C8FA9
		public virtual void GetSlicePositionAndDirection(out Vector3 position, out Vector3 direction)
		{
			direction = this.GetSliceDirection();
			position = this.meshBone.transform.position + direction * this.sliceHeight;
		}

		// Token: 0x06001DCB RID: 7627 RVA: 0x000CADE4 File Offset: 0x000C8FE4
		public virtual Vector3 GetSliceDirection()
		{
			if (!this.parentPart)
			{
				return this.meshBone.transform.TransformDirection(this.boneToChildDirection);
			}
			return Vector3.Lerp(this.meshBone.transform.TransformDirection(this.boneToChildDirection), this.parentPart.meshBone.transform.TransformDirection(this.parentPart.boneToChildDirection), this.sliceParentAdjust);
		}

		// Token: 0x06001DCC RID: 7628 RVA: 0x000CAE56 File Offset: 0x000C9056
		protected virtual void OnDrawGizmosSelected()
		{
		}

		// Token: 0x170001DB RID: 475
		// (get) Token: 0x06001DCD RID: 7629 RVA: 0x000CAE58 File Offset: 0x000C9058
		public Vector3 forwardDirection
		{
			get
			{
				return this.AxisToDirection(this.frontAxis);
			}
		}

		// Token: 0x170001DC RID: 476
		// (get) Token: 0x06001DCE RID: 7630 RVA: 0x000CAE66 File Offset: 0x000C9066
		public Vector3 upDirection
		{
			get
			{
				return this.AxisToDirection(this.upAxis);
			}
		}

		// Token: 0x140000E2 RID: 226
		// (add) Token: 0x06001DCF RID: 7631 RVA: 0x000CAE74 File Offset: 0x000C9074
		// (remove) Token: 0x06001DD0 RID: 7632 RVA: 0x000CAEAC File Offset: 0x000C90AC
		public event RagdollPart.TouchActionDelegate OnTouchActionEvent;

		// Token: 0x140000E3 RID: 227
		// (add) Token: 0x06001DD1 RID: 7633 RVA: 0x000CAEE4 File Offset: 0x000C90E4
		// (remove) Token: 0x06001DD2 RID: 7634 RVA: 0x000CAF1C File Offset: 0x000C911C
		public event RagdollPart.HandlerEvent OnGrabbed;

		// Token: 0x140000E4 RID: 228
		// (add) Token: 0x06001DD3 RID: 7635 RVA: 0x000CAF54 File Offset: 0x000C9154
		// (remove) Token: 0x06001DD4 RID: 7636 RVA: 0x000CAF8C File Offset: 0x000C918C
		public event RagdollPart.HandlerEvent OnUngrabbed;

		// Token: 0x140000E5 RID: 229
		// (add) Token: 0x06001DD5 RID: 7637 RVA: 0x000CAFC4 File Offset: 0x000C91C4
		// (remove) Token: 0x06001DD6 RID: 7638 RVA: 0x000CAFFC File Offset: 0x000C91FC
		public event RagdollPart.TKHandlerEvent OnTKGrab;

		// Token: 0x140000E6 RID: 230
		// (add) Token: 0x06001DD7 RID: 7639 RVA: 0x000CB034 File Offset: 0x000C9234
		// (remove) Token: 0x06001DD8 RID: 7640 RVA: 0x000CB06C File Offset: 0x000C926C
		public event RagdollPart.TKHandlerEvent OnTKRelease;

		// Token: 0x140000E7 RID: 231
		// (add) Token: 0x06001DD9 RID: 7641 RVA: 0x000CB0A4 File Offset: 0x000C92A4
		// (remove) Token: 0x06001DDA RID: 7642 RVA: 0x000CB0DC File Offset: 0x000C92DC
		public event RagdollPart.HeldActionDelegate OnHeldActionEvent;

		// Token: 0x06001DDB RID: 7643 RVA: 0x000CB111 File Offset: 0x000C9311
		public virtual void OnRagdollEnable()
		{
		}

		// Token: 0x06001DDC RID: 7644 RVA: 0x000CB113 File Offset: 0x000C9313
		public virtual void OnRagdollDisable()
		{
		}

		// Token: 0x06001DDD RID: 7645 RVA: 0x000CB118 File Offset: 0x000C9318
		private Vector3 AxisToDirection(RagdollPart.Axis axis)
		{
			switch (axis)
			{
			case RagdollPart.Axis.Right:
				return base.transform.right;
			case RagdollPart.Axis.Left:
				return -base.transform.right;
			case RagdollPart.Axis.Up:
				return base.transform.up;
			case RagdollPart.Axis.Down:
				return -base.transform.up;
			case RagdollPart.Axis.Forwards:
				return base.transform.forward;
			case RagdollPart.Axis.Backwards:
				return -base.transform.forward;
			default:
				return base.transform.forward;
			}
		}

		// Token: 0x06001DDE RID: 7646 RVA: 0x000CB1A8 File Offset: 0x000C93A8
		public bool SafeSlice()
		{
			RagdollPart.Type x = this.type;
			if (x <= RagdollPart.Type.LeftArm)
			{
				if (x == RagdollPart.Type.Head)
				{
					return this.parentPart.TrySlice();
				}
				if (x == RagdollPart.Type.Neck)
				{
					return this.TrySlice();
				}
				if (x != RagdollPart.Type.LeftArm)
				{
					goto IL_5B;
				}
			}
			else if (x != RagdollPart.Type.RightArm && x != RagdollPart.Type.LeftLeg && x != RagdollPart.Type.RightLeg)
			{
				goto IL_5B;
			}
			return this.ragdoll.GetPart(x).TrySlice();
			IL_5B:
			return false;
		}

		// Token: 0x06001DDF RID: 7647 RVA: 0x000CB214 File Offset: 0x000C9414
		public bool TrySlice()
		{
			if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Dismemberment, BuildSettings.ContentFlagBehaviour.Discard))
			{
				return false;
			}
			if (this == this.ragdoll.rootPart)
			{
				return false;
			}
			if (this.sliceChildAndDisableSelf)
			{
				if (!this.ragdoll.TrySlice(this.sliceChildAndDisableSelf))
				{
					return false;
				}
				this.isSliced = true;
				this.ragdoll.isSliced = true;
				foreach (Collider collider in this.colliderGroup.colliders)
				{
					collider.enabled = false;
				}
				foreach (HandleRagdoll handleRagdoll in this.handles)
				{
					handleRagdoll.SetTouch(false);
				}
				this.FixedCharJointLimit();
				this.characterJointLocked = true;
			}
			else if (!this.ragdoll.TrySlice(this))
			{
				return false;
			}
			return true;
		}

		// Token: 0x06001DE0 RID: 7648 RVA: 0x000CB324 File Offset: 0x000C9524
		protected bool HasMetalArmor()
		{
			Ragdoll ragdoll = this.ragdoll;
			ItemContent[] array;
			if (ragdoll == null)
			{
				array = null;
			}
			else
			{
				Creature creature = ragdoll.creature;
				if (creature == null)
				{
					array = null;
				}
				else
				{
					Equipment equipment = creature.equipment;
					array = ((equipment != null) ? equipment.GetEquipmentOnPart(this.type) : null);
				}
			}
			ItemContent[] contents = array;
			if (contents != null)
			{
				ItemContent[] array2 = contents;
				for (int i = 0; i < array2.Length; i++)
				{
					ItemData itemData = array2[i].data;
					bool flag;
					if (itemData == null)
					{
						flag = false;
					}
					else
					{
						ItemModuleWardrobe module = itemData.GetModule<ItemModuleWardrobe>();
						bool? flag2 = (module != null) ? new bool?(module.isMetal) : null;
						bool flag3 = true;
						flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
					}
					if (flag)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001DE1 RID: 7649 RVA: 0x000CB3C0 File Offset: 0x000C95C0
		protected virtual void Awake()
		{
			this.physicBody = base.gameObject.GetPhysicBody();
			this.physicBody.isKinematic = true;
			this.standingMass = this.physicBody.mass;
			if (this.ragdolledMass < 0f)
			{
				this.ragdolledMass = this.physicBody.mass;
			}
			this.colliderGroup = base.GetComponentInChildren<ColliderGroup>();
			this.collisionHandler = base.GetComponent<CollisionHandler>();
			this.handles = new List<HandleRagdoll>(base.GetComponentsInChildren<HandleRagdoll>());
			this.bodyDamager = this.collisionHandler.gameObject.GetComponent<Damager>();
			if (!this.bodyDamager)
			{
				this.bodyDamager = this.collisionHandler.gameObject.AddComponent<Damager>();
			}
			this.bodyDamager.colliderGroup = this.colliderGroup;
			this.bodyDamager.direction = Damager.Direction.All;
			this.collisionHandler.damagers.Add(this.bodyDamager);
			this.hasParent = (this.parentPart != null);
			if (this.hasParent)
			{
				RagdollPartJointFixer fixer;
				if (!base.TryGetComponent<RagdollPartJointFixer>(out fixer))
				{
					fixer = base.gameObject.AddComponent<RagdollPartJointFixer>();
				}
				if (!fixer.initialized)
				{
					fixer.SetPart(this);
				}
			}
		}

		// Token: 0x06001DE2 RID: 7650 RVA: 0x000CB4ED File Offset: 0x000C96ED
		public void LinkParent()
		{
			if (this.parentPart != null && !this.parentPart.childParts.Contains(this))
			{
				this.parentPart.childParts.Add(this);
			}
		}

		// Token: 0x06001DE3 RID: 7651 RVA: 0x000CB524 File Offset: 0x000C9724
		public virtual void Init(Ragdoll ragdoll)
		{
			if (this.meshBone == null)
			{
				Debug.LogError("Mesh bone is not set on part " + base.name);
				return;
			}
			this.ragdoll = ragdoll;
			this.root = base.transform.parent;
			this.rootOrgLocalPosition = base.transform.localPosition;
			this.rootOrgLocalRotation = base.transform.localRotation;
			this.characterJoint = base.GetComponent<CharacterJoint>();
			if (this.characterJoint)
			{
				this.orgCharacterJointData = new RagdollPart.CharacterJointData(this.characterJoint);
			}
			foreach (RagdollPart part in this.ignoredParts)
			{
				foreach (Collider thisCollider in base.GetComponentsInChildren<Collider>(true))
				{
					foreach (Collider ignoredCollider in part.GetComponentsInChildren<Collider>(true))
					{
						Physics.IgnoreCollision(thisCollider, ignoredCollider, true);
					}
				}
			}
			this.collisionHandler.OnCollisionStartEvent += this.CollisionStart;
			this.collisionHandler.OnCollisionStopEvent += this.CollisionEnd;
			ragdoll.OnGrabEvent += this.Grabbed;
			ragdoll.OnUngrabEvent += this.Ungrabbed;
			ragdoll.OnTelekinesisGrabEvent += this.TKGrabbed;
			ragdoll.OnTelekinesisReleaseEvent += this.TKReleased;
			this.RefreshLayer();
			this.initialized = true;
		}

		// Token: 0x06001DE4 RID: 7652 RVA: 0x000CB6C4 File Offset: 0x000C98C4
		private void Grabbed(RagdollHand ragdollHand, HandleRagdoll handleRagdoll)
		{
			if (handleRagdoll.ragdollPart == this)
			{
				RagdollPart.HandlerEvent onGrabbed = this.OnGrabbed;
				if (onGrabbed == null)
				{
					return;
				}
				onGrabbed(ragdollHand, handleRagdoll);
			}
		}

		// Token: 0x06001DE5 RID: 7653 RVA: 0x000CB6E6 File Offset: 0x000C98E6
		private void Ungrabbed(RagdollHand ragdollHand, HandleRagdoll handleRagdoll, bool lastHandler)
		{
			if (handleRagdoll.ragdollPart == this)
			{
				RagdollPart.HandlerEvent onUngrabbed = this.OnUngrabbed;
				if (onUngrabbed == null)
				{
					return;
				}
				onUngrabbed(ragdollHand, handleRagdoll);
			}
		}

		// Token: 0x06001DE6 RID: 7654 RVA: 0x000CB708 File Offset: 0x000C9908
		private void TKGrabbed(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll)
		{
			if (handleRagdoll.ragdollPart == this)
			{
				RagdollPart.TKHandlerEvent onTKGrab = this.OnTKGrab;
				if (onTKGrab == null)
				{
					return;
				}
				onTKGrab(spellTelekinesis, handleRagdoll);
			}
		}

		// Token: 0x06001DE7 RID: 7655 RVA: 0x000CB72A File Offset: 0x000C992A
		private void TKReleased(SpellTelekinesis spellTelekinesis, HandleRagdoll handleRagdoll, bool lastHandler)
		{
			if (handleRagdoll.ragdollPart == this)
			{
				RagdollPart.TKHandlerEvent onTKRelease = this.OnTKRelease;
				if (onTKRelease == null)
				{
					return;
				}
				onTKRelease(spellTelekinesis, handleRagdoll);
			}
		}

		// Token: 0x06001DE8 RID: 7656 RVA: 0x000CB74C File Offset: 0x000C994C
		private void CollisionStart(CollisionInstance collisionInstance)
		{
			this.ragdoll.CollisionStartStop(collisionInstance, this, true);
		}

		// Token: 0x06001DE9 RID: 7657 RVA: 0x000CB75C File Offset: 0x000C995C
		private void CollisionEnd(CollisionInstance collisionInstance)
		{
			this.ragdoll.CollisionStartStop(collisionInstance, this, false);
		}

		// Token: 0x06001DEA RID: 7658 RVA: 0x000CB76C File Offset: 0x000C996C
		public virtual void Load()
		{
			this.SetBodyDamagerToDefault();
		}

		// Token: 0x06001DEB RID: 7659 RVA: 0x000CB774 File Offset: 0x000C9974
		public bool UpdateMetalArmor()
		{
			this.hasMetalArmor = this.HasMetalArmor();
			return this.hasMetalArmor;
		}

		// Token: 0x06001DEC RID: 7660 RVA: 0x000CB788 File Offset: 0x000C9988
		public void UpdateRenderers()
		{
			this.renderers.Clear();
			this.skinnedMeshRenderers.Clear();
			this.skinnedMeshRendererIndexes.Clear();
			this.meshpartSkinnedMeshRenderers.Clear();
			this.meshpartRendererList.Clear();
			ManikinPart[] manikinParts = null;
			if (this.ragdoll.creature.manikinLocations)
			{
				manikinParts = this.ragdoll.creature.manikinLocations.GetPartsOnBoneInLayerOrder(this.bone.boneHashes);
			}
			for (int i = 0; i < this.ragdoll.creature.renderers.Count; i++)
			{
				Creature.RendererData creatureRenderer = this.ragdoll.creature.renderers[i];
				bool hasMeshPart = creatureRenderer.meshPart != null;
				bool boneMatch = manikinParts == null || manikinParts.Contains(creatureRenderer.manikinPart);
				if (!boneMatch)
				{
					ManikinGroupPart groupPart = creatureRenderer.manikinPart as ManikinGroupPart;
					if (groupPart != null)
					{
						boneMatch = groupPart.PartOfBones(this.bone.boneHashes);
					}
				}
				if (boneMatch && creatureRenderer.revealDecal != null)
				{
					if (creatureRenderer.renderer)
					{
						this.skinnedMeshRenderers.Add(hasMeshPart ? creatureRenderer.renderer : null);
						this.skinnedMeshRendererIndexes.Add(this.renderers.Count);
						if (hasMeshPart && creatureRenderer.meshPart.skinnedMeshRenderer == creatureRenderer.renderer)
						{
							this.meshpartSkinnedMeshRenderers.Add(creatureRenderer.renderer);
							this.meshpartRendererList.Add(creatureRenderer);
						}
					}
					if (creatureRenderer.splitRenderer)
					{
						this.skinnedMeshRenderers.Add(hasMeshPart ? creatureRenderer.splitRenderer : null);
						this.skinnedMeshRendererIndexes.Add(this.renderers.Count);
					}
					this.renderers.Add(creatureRenderer);
				}
			}
		}

		// Token: 0x06001DED RID: 7661 RVA: 0x000CB960 File Offset: 0x000C9B60
		public void AnimatorMoveUpdate()
		{
			if (this.isSliced)
			{
				Animator creatureAnimator = this.ragdoll.creature.animator;
				base.transform.position -= creatureAnimator.deltaPosition;
				Quaternion back = Quaternion.Inverse(creatureAnimator.deltaRotation);
				base.transform.position = creatureAnimator.transform.TransformPoint(back * creatureAnimator.transform.InverseTransformPoint(base.transform.position));
				Vector3 newForward = creatureAnimator.transform.TransformPoint(back * creatureAnimator.transform.InverseTransformPoint(base.transform.position + base.transform.forward)) - base.transform.position;
				Vector3 newUp = creatureAnimator.transform.TransformPoint(back * creatureAnimator.transform.InverseTransformPoint(base.transform.position + base.transform.up)) - base.transform.position;
				base.transform.rotation = Quaternion.LookRotation(newForward, newUp);
			}
		}

		// Token: 0x06001DEE RID: 7662 RVA: 0x000CBA85 File Offset: 0x000C9C85
		public virtual void OnHeldAction(RagdollHand ragdollHand, HandleRagdoll handle, Interactable.Action action)
		{
			this.ragdoll.OnHeldAction(ragdollHand, this, handle, action);
			RagdollPart.HeldActionDelegate onHeldActionEvent = this.OnHeldActionEvent;
			if (onHeldActionEvent == null)
			{
				return;
			}
			onHeldActionEvent(ragdollHand, handle, action);
		}

		// Token: 0x06001DEF RID: 7663 RVA: 0x000CBAA9 File Offset: 0x000C9CA9
		public virtual void OnTouchAction(RagdollHand ragdollHand, Interactable interactable, Interactable.Action action)
		{
			this.ragdoll.OnTouchAction(ragdollHand, this, interactable, action);
			RagdollPart.TouchActionDelegate onTouchActionEvent = this.OnTouchActionEvent;
			if (onTouchActionEvent == null)
			{
				return;
			}
			onTouchActionEvent(ragdollHand, interactable, action);
		}

		// Token: 0x06001DF0 RID: 7664 RVA: 0x000CBACD File Offset: 0x000C9CCD
		public virtual void SetBodyDamagerToDefault()
		{
			this.SetBodyDamager(false);
		}

		// Token: 0x06001DF1 RID: 7665 RVA: 0x000CBAD6 File Offset: 0x000C9CD6
		public virtual void SetBodyDamagerToAttack()
		{
			this.SetBodyDamager(true);
		}

		// Token: 0x06001DF2 RID: 7666 RVA: 0x000CBAE0 File Offset: 0x000C9CE0
		private void SetBodyDamager(bool isAttack)
		{
			DamagerData damagerData2;
			if (!isAttack)
			{
				CreatureData.PartData partData = this.data;
				damagerData2 = ((partData != null) ? partData.bodyDamagerData : null);
			}
			else
			{
				CreatureData.PartData partData2 = this.data;
				damagerData2 = ((partData2 != null) ? partData2.bodyAttackDamagerData : null);
			}
			DamagerData damagerData = damagerData2;
			if (damagerData == null)
			{
				damagerData = this.ragdoll.creature.data.ragdollData.bodyDefaultDamagerData;
				isAttack = false;
			}
			this.bodyDamager.Load(damagerData, this.collisionHandler);
			this.collisionHandler.SortDamagers();
			this.bodyDamagerIsAttack = isAttack;
			this.RefreshLayer();
		}

		// Token: 0x06001DF3 RID: 7667 RVA: 0x000CBB64 File Offset: 0x000C9D64
		public virtual void RefreshLayer()
		{
			if (this.bodyDamagerIsAttack)
			{
				this.SetLayer(LayerName.PlayerHandAndFoot);
				return;
			}
			if (!this.isSliced && !this.isGrabbed && (this.ragdoll.state == Ragdoll.State.Standing || this.ragdoll.state == Ragdoll.State.NoPhysic || this.ragdoll.state == Ragdoll.State.Kinematic || this.ragdoll.state == Ragdoll.State.Disabled))
			{
				if (this.ragdoll.creature.player)
				{
					this.SetLayer(LayerName.Avatar);
					return;
				}
				this.SetLayer(this.ignoreStaticCollision ? LayerName.ItemAndRagdollOnly : LayerName.NPC);
				return;
			}
			else
			{
				if (!GameManager.CheckContentActive(BuildSettings.ContentFlag.Desecration, BuildSettings.ContentFlagBehaviour.Discard) && this.ragdoll.creature.isKilled)
				{
					this.SetLayer(LayerName.LocomotionOnly);
					return;
				}
				this.SetLayer(LayerName.Ragdoll);
				return;
			}
		}

		// Token: 0x06001DF4 RID: 7668 RVA: 0x000CBC2C File Offset: 0x000C9E2C
		public void ResetCharJointLimit()
		{
			if (!this.characterJointLocked && this.characterJoint)
			{
				SoftJointLimit softJointLimit = this.characterJoint.lowTwistLimit;
				softJointLimit.limit = this.orgCharacterJointData.lowTwistLimit.limit;
				this.characterJoint.lowTwistLimit = softJointLimit;
				softJointLimit = this.characterJoint.highTwistLimit;
				softJointLimit.limit = this.orgCharacterJointData.highTwistLimit.limit;
				this.characterJoint.highTwistLimit = softJointLimit;
				softJointLimit = this.characterJoint.swing1Limit;
				softJointLimit.limit = this.orgCharacterJointData.swing1Limit.limit;
				this.characterJoint.swing1Limit = softJointLimit;
				softJointLimit = this.characterJoint.swing2Limit;
				softJointLimit.limit = this.orgCharacterJointData.swing2Limit.limit;
				this.characterJoint.swing2Limit = softJointLimit;
			}
		}

		// Token: 0x06001DF5 RID: 7669 RVA: 0x000CBD10 File Offset: 0x000C9F10
		public void DisableCharJointLimit()
		{
			if (!this.characterJointLocked && this.characterJoint)
			{
				SoftJointLimit softJointLimit = this.characterJoint.lowTwistLimit;
				softJointLimit.limit = -180f;
				this.characterJoint.lowTwistLimit = softJointLimit;
				softJointLimit = this.characterJoint.highTwistLimit;
				softJointLimit.limit = 180f;
				this.characterJoint.highTwistLimit = softJointLimit;
				softJointLimit = this.characterJoint.swing1Limit;
				softJointLimit.limit = 180f;
				this.characterJoint.swing1Limit = softJointLimit;
				softJointLimit = this.characterJoint.swing2Limit;
				softJointLimit.limit = 180f;
				this.characterJoint.swing2Limit = softJointLimit;
			}
		}

		// Token: 0x06001DF6 RID: 7670 RVA: 0x000CBDC8 File Offset: 0x000C9FC8
		public void FixedCharJointLimit()
		{
			if (!this.characterJointLocked && this.characterJoint)
			{
				SoftJointLimit softJointLimit = this.characterJoint.lowTwistLimit;
				softJointLimit.limit = 0f;
				this.characterJoint.lowTwistLimit = softJointLimit;
				softJointLimit = this.characterJoint.highTwistLimit;
				softJointLimit.limit = 0f;
				this.characterJoint.highTwistLimit = softJointLimit;
				softJointLimit = this.characterJoint.swing1Limit;
				softJointLimit.limit = 0f;
				this.characterJoint.swing1Limit = softJointLimit;
				softJointLimit = this.characterJoint.swing2Limit;
				softJointLimit.limit = 0f;
				this.characterJoint.swing2Limit = softJointLimit;
			}
		}

		// Token: 0x06001DF7 RID: 7671 RVA: 0x000CBE80 File Offset: 0x000CA080
		public void EnableCharJointBreakForce(float multiplier = 1f)
		{
			if (this.characterJointLocked || !Damager.dismembermentEnabled || !this.ripBreak || this.isSliced || !this.characterJoint)
			{
				return;
			}
			this.breakForceMultiplier = multiplier;
			this.characterJoint.breakForce = this.ripBreakForce * multiplier * 1f / Time.timeScale;
			SpellPowerSlowTime.OnTimeScaleChangeEvent -= this.UpdateJointBreakForceToTimeScale;
			SpellPowerSlowTime.OnTimeScaleChangeEvent += this.UpdateJointBreakForceToTimeScale;
		}

		// Token: 0x06001DF8 RID: 7672 RVA: 0x000CBF04 File Offset: 0x000CA104
		public void ResetCharJointBreakForce()
		{
			if (this.characterJointLocked || !Damager.dismembermentEnabled || !this.ripBreak || !this.characterJoint)
			{
				return;
			}
			SpellPowerSlowTime.OnTimeScaleChangeEvent -= this.UpdateJointBreakForceToTimeScale;
			this.breakForceMultiplier = 1f;
			this.characterJoint.breakForce = (this.breakForceMultiplier = this.orgCharacterJointData.breakForce);
		}

		// Token: 0x06001DF9 RID: 7673 RVA: 0x000CBF74 File Offset: 0x000CA174
		public void UpdateJointBreakForceToTimeScale(SpellPowerSlowTime slowTime, float scale)
		{
			if (this.characterJointLocked || !Damager.dismembermentEnabled || !this.ripBreak || !this.characterJoint)
			{
				return;
			}
			this.characterJoint.breakForce = this.ripBreakForce * this.breakForceMultiplier * 1f / scale;
		}

		// Token: 0x06001DFA RID: 7674 RVA: 0x000CBFC6 File Offset: 0x000CA1C6
		public void CreateCharJoint(bool resetPosition)
		{
			if (!this.characterJointLocked)
			{
				this.DestroyCharJoint();
				this.characterJoint = this.orgCharacterJointData.CreateJoint(base.gameObject, resetPosition);
			}
		}

		// Token: 0x06001DFB RID: 7675 RVA: 0x000CBFEE File Offset: 0x000CA1EE
		public void DestroyCharJoint()
		{
			if (!this.characterJointLocked && this.characterJoint)
			{
				UnityEngine.Object.Destroy(this.characterJoint);
			}
		}

		// Token: 0x06001DFC RID: 7676 RVA: 0x000CC010 File Offset: 0x000CA210
		protected void OnJointBreak()
		{
			if (this.TrySlice() && this.data.sliceForceKill)
			{
				CollisionInstance collisionInstance = new CollisionInstance(new DamageStruct(DamageType.Energy, float.PositiveInfinity), null, null);
				collisionInstance.damageStruct.hitRagdollPart = this;
				this.ragdoll.creature.Damage(collisionInstance);
			}
		}

		// Token: 0x06001DFD RID: 7677 RVA: 0x000CC062 File Offset: 0x000CA262
		public float GetPinSpringPosition()
		{
			return this.ragdoll.springPositionForce * this.springPositionMultiplier;
		}

		// Token: 0x06001DFE RID: 7678 RVA: 0x000CC076 File Offset: 0x000CA276
		public float GetPinDamperPosition()
		{
			return this.ragdoll.springPositionForce * this.damperPositionMultiplier;
		}

		// Token: 0x06001DFF RID: 7679 RVA: 0x000CC08A File Offset: 0x000CA28A
		public float GetPinSpringRotation()
		{
			return this.ragdoll.springRotationForce * this.springRotationMultiplier;
		}

		// Token: 0x06001E00 RID: 7680 RVA: 0x000CC09E File Offset: 0x000CA29E
		public float GetPinDamperRotation()
		{
			return this.ragdoll.springRotationForce * this.damperRotationMultiplier;
		}

		// Token: 0x06001E01 RID: 7681 RVA: 0x000CC0B4 File Offset: 0x000CA2B4
		public bool HasCollider(Collider collider)
		{
			for (int i = 0; i < this.colliderGroup.colliders.Count; i++)
			{
				if (collider == this.colliderGroup.colliders[i])
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001E02 RID: 7682 RVA: 0x000CC0F8 File Offset: 0x000CA2F8
		public void SetLayer(int layer)
		{
			base.gameObject.layer = layer;
			for (int i = 0; i < this.colliderGroup.colliders.Count; i++)
			{
				if (!(this.colliderGroup.colliders[i] == null) && !(this.colliderGroup.colliders[i].gameObject == null))
				{
					this.colliderGroup.colliders[i].gameObject.layer = layer;
				}
			}
		}

		// Token: 0x06001E03 RID: 7683 RVA: 0x000CC17F File Offset: 0x000CA37F
		public void SetLayer(LayerName layerName)
		{
			this.SetLayer(GameManager.GetLayer(layerName));
		}

		// Token: 0x06001E04 RID: 7684 RVA: 0x000CC190 File Offset: 0x000CA390
		public Bounds GetWorldBounds()
		{
			Bounds bounds = new Bounds(base.transform.position, Vector3.zero);
			for (int i = 0; i < this.renderers.Count; i++)
			{
				bounds.Encapsulate(this.renderers[i].renderer.bounds);
			}
			return bounds;
		}

		// Token: 0x06001E05 RID: 7685 RVA: 0x000CC1E8 File Offset: 0x000CA3E8
		public bool IsPenetrated()
		{
			return this.collisionHandler != null && this.collisionHandler.penetratedObjects.Count > 0;
		}

		// Token: 0x04001C42 RID: 7234
		[Header("Part")]
		public Transform meshBone;

		// Token: 0x04001C43 RID: 7235
		public Transform[] linkedMeshBones;

		// Token: 0x04001C44 RID: 7236
		public RagdollPart.Type type;

		// Token: 0x04001C45 RID: 7237
		public RagdollPart.Section section;

		// Token: 0x04001C46 RID: 7238
		[SerializeField]
		private RagdollPart.Axis frontAxis = RagdollPart.Axis.Forwards;

		// Token: 0x04001C47 RID: 7239
		[SerializeField]
		private RagdollPart.Axis upAxis = RagdollPart.Axis.Left;

		// Token: 0x04001C48 RID: 7240
		public Vector3 boneToChildDirection = Vector3.left;

		// Token: 0x04001C49 RID: 7241
		public RagdollPart parentPart;

		// Token: 0x04001C4B RID: 7243
		public bool ignoreStaticCollision;

		// Token: 0x04001C4C RID: 7244
		[Header("Dismemberment")]
		public bool sliceAllowed;

		// Token: 0x04001C4D RID: 7245
		[Range(0f, 1f)]
		public float sliceParentAdjust = 0.5f;

		// Token: 0x04001C4E RID: 7246
		public float sliceWidth = 0.04f;

		// Token: 0x04001C4F RID: 7247
		public float sliceHeight;

		// Token: 0x04001C50 RID: 7248
		public float sliceThreshold = 0.5f;

		// Token: 0x04001C51 RID: 7249
		public Material sliceFillMaterial;

		// Token: 0x04001C52 RID: 7250
		[Tooltip("Disable this part collider and slice the referenced child part on slice (usefull for necks)")]
		public RagdollPart sliceChildAndDisableSelf;

		// Token: 0x04001C53 RID: 7251
		public bool ripBreak;

		// Token: 0x04001C54 RID: 7252
		public float ripBreakForce = 3000f;

		// Token: 0x04001C55 RID: 7253
		[Header("Forces")]
		[Min(1E-05f)]
		public float handledMass = -1f;

		// Token: 0x04001C56 RID: 7254
		[Min(1E-05f)]
		public float ragdolledMass = -1f;

		// Token: 0x04001C57 RID: 7255
		public float springPositionMultiplier = 1f;

		// Token: 0x04001C58 RID: 7256
		public float damperPositionMultiplier = 1f;

		// Token: 0x04001C59 RID: 7257
		public float springRotationMultiplier = 1f;

		// Token: 0x04001C5A RID: 7258
		public float damperRotationMultiplier = 1f;

		// Token: 0x04001C5B RID: 7259
		public List<RagdollPart> ignoredParts;

		// Token: 0x04001C5C RID: 7260
		[NonSerialized]
		public CreatureData.PartData data;

		// Token: 0x04001C5D RID: 7261
		[NonSerialized]
		public bool initialized;

		// Token: 0x04001C5E RID: 7262
		[NonSerialized]
		public bool bodyDamagerIsAttack;

		// Token: 0x04001C5F RID: 7263
		[NonSerialized]
		public PhysicBody physicBody;

		// Token: 0x04001C60 RID: 7264
		[NonSerialized]
		public Ragdoll ragdoll;

		// Token: 0x04001C61 RID: 7265
		[NonSerialized]
		public Ragdoll.Region ragdollRegion;

		// Token: 0x04001C62 RID: 7266
		[NonSerialized]
		public ColliderGroup colliderGroup;

		// Token: 0x04001C63 RID: 7267
		[NonSerialized]
		public CollisionHandler collisionHandler;

		// Token: 0x04001C64 RID: 7268
		public Wearable wearable;

		// Token: 0x04001C65 RID: 7269
		[NonSerialized]
		public bool hasMetalArmor;

		// Token: 0x04001C66 RID: 7270
		[NonSerialized]
		public List<RagdollPart> childParts = new List<RagdollPart>();

		// Token: 0x04001C67 RID: 7271
		[NonSerialized]
		public List<Creature.RendererData> renderers = new List<Creature.RendererData>();

		// Token: 0x04001C68 RID: 7272
		[NonSerialized]
		public List<SkinnedMeshRenderer> skinnedMeshRenderers = new List<SkinnedMeshRenderer>();

		// Token: 0x04001C69 RID: 7273
		[NonSerialized]
		public List<int> skinnedMeshRendererIndexes = new List<int>();

		// Token: 0x04001C6A RID: 7274
		[NonSerialized]
		public List<SkinnedMeshRenderer> meshpartSkinnedMeshRenderers = new List<SkinnedMeshRenderer>();

		// Token: 0x04001C6B RID: 7275
		[NonSerialized]
		public List<Creature.RendererData> meshpartRendererList = new List<Creature.RendererData>();

		// Token: 0x04001C6C RID: 7276
		[NonSerialized]
		public float standingMass = -1f;

		// Token: 0x04001C6D RID: 7277
		[NonSerialized]
		public Ragdoll.Bone bone;

		// Token: 0x04001C6E RID: 7278
		[NonSerialized]
		public bool isSliced;

		// Token: 0x04001C6F RID: 7279
		[NonSerialized]
		public Transform slicedMeshRoot;

		// Token: 0x04001C70 RID: 7280
		[NonSerialized]
		public Transform root;

		// Token: 0x04001C71 RID: 7281
		[NonSerialized]
		public Vector3 rootOrgLocalPosition;

		// Token: 0x04001C72 RID: 7282
		[NonSerialized]
		public Quaternion rootOrgLocalRotation;

		// Token: 0x04001C73 RID: 7283
		[NonSerialized]
		public Vector3 savedPosition;

		// Token: 0x04001C74 RID: 7284
		[NonSerialized]
		public Quaternion savedRotation;

		// Token: 0x04001C75 RID: 7285
		[NonSerialized]
		public List<HandleRagdoll> handles;

		// Token: 0x04001C76 RID: 7286
		[NonSerialized]
		public bool isGrabbed;

		// Token: 0x04001C77 RID: 7287
		[NonSerialized]
		public Damager bodyDamager;

		// Token: 0x04001C78 RID: 7288
		[NonSerialized]
		public CharacterJoint characterJoint;

		// Token: 0x04001C79 RID: 7289
		public RagdollPart.CharacterJointData orgCharacterJointData;

		// Token: 0x04001C7A RID: 7290
		[NonSerialized]
		public bool characterJointLocked;

		// Token: 0x04001C7B RID: 7291
		public int damagerTier;

		// Token: 0x04001C82 RID: 7298
		protected float breakForceMultiplier;

		// Token: 0x0200091D RID: 2333
		[Flags]
		public enum Type
		{
			// Token: 0x04004392 RID: 17298
			Head = 1,
			// Token: 0x04004393 RID: 17299
			Neck = 2,
			// Token: 0x04004394 RID: 17300
			Torso = 4,
			// Token: 0x04004395 RID: 17301
			LeftArm = 8,
			// Token: 0x04004396 RID: 17302
			RightArm = 16,
			// Token: 0x04004397 RID: 17303
			LeftHand = 32,
			// Token: 0x04004398 RID: 17304
			RightHand = 64,
			// Token: 0x04004399 RID: 17305
			LeftLeg = 128,
			// Token: 0x0400439A RID: 17306
			RightLeg = 256,
			// Token: 0x0400439B RID: 17307
			LeftFoot = 512,
			// Token: 0x0400439C RID: 17308
			RightFoot = 1024,
			// Token: 0x0400439D RID: 17309
			LeftWing = 2048,
			// Token: 0x0400439E RID: 17310
			RightWing = 4096,
			// Token: 0x0400439F RID: 17311
			Tail = 8192
		}

		// Token: 0x0200091E RID: 2334
		public enum Section
		{
			// Token: 0x040043A1 RID: 17313
			Full,
			// Token: 0x040043A2 RID: 17314
			Lower,
			// Token: 0x040043A3 RID: 17315
			Mid,
			// Token: 0x040043A4 RID: 17316
			Upper
		}

		// Token: 0x0200091F RID: 2335
		public enum Axis
		{
			// Token: 0x040043A6 RID: 17318
			Right,
			// Token: 0x040043A7 RID: 17319
			Left,
			// Token: 0x040043A8 RID: 17320
			Up,
			// Token: 0x040043A9 RID: 17321
			Down,
			// Token: 0x040043AA RID: 17322
			Forwards,
			// Token: 0x040043AB RID: 17323
			Backwards
		}

		// Token: 0x02000920 RID: 2336
		// (Invoke) Token: 0x0600428F RID: 17039
		public delegate void TouchActionDelegate(RagdollHand ragdollHand, Interactable interactable, Interactable.Action action);

		// Token: 0x02000921 RID: 2337
		// (Invoke) Token: 0x06004293 RID: 17043
		public delegate void HandlerEvent(RagdollHand ragdollHand, HandleRagdoll handle);

		// Token: 0x02000922 RID: 2338
		// (Invoke) Token: 0x06004297 RID: 17047
		public delegate void TKHandlerEvent(SpellTelekinesis spellTelekinesis, HandleRagdoll handle);

		// Token: 0x02000923 RID: 2339
		// (Invoke) Token: 0x0600429B RID: 17051
		public delegate void HeldActionDelegate(RagdollHand ragdollHand, HandleRagdoll handle, Interactable.Action action);

		// Token: 0x02000924 RID: 2340
		public class CharacterJointData
		{
			// Token: 0x0600429E RID: 17054 RVA: 0x0018D694 File Offset: 0x0018B894
			public CharacterJointData(CharacterJoint characterJoint)
			{
				this.localPosition = characterJoint.connectedBody.transform.InverseTransformPoint(characterJoint.transform.position);
				this.localRotation = Quaternion.Inverse(characterJoint.connectedBody.transform.rotation) * characterJoint.transform.rotation;
				this.connectedBody = characterJoint.connectedBody;
				this.anchor = characterJoint.anchor;
				this.axis = characterJoint.axis;
				this.autoConfigureConnectedAnchor = characterJoint.autoConfigureConnectedAnchor;
				this.connectedAnchor = characterJoint.connectedAnchor;
				this.swingAxis = characterJoint.swingAxis;
				this.twistLimitSpring = characterJoint.twistLimitSpring;
				this.lowTwistLimit = characterJoint.lowTwistLimit;
				this.highTwistLimit = characterJoint.highTwistLimit;
				this.swingLimitSpring = characterJoint.swingLimitSpring;
				this.swing1Limit = characterJoint.swing1Limit;
				this.swing2Limit = characterJoint.swing2Limit;
				this.enableProjection = characterJoint.enableProjection;
				this.projectionDistance = characterJoint.projectionDistance;
				this.projectionAngle = characterJoint.projectionAngle;
				this.breakForce = characterJoint.breakForce;
				this.breakTorque = characterJoint.breakTorque;
				this.enableCollision = characterJoint.enableCollision;
				this.enablePreprocessing = characterJoint.enablePreprocessing;
				this.massScale = characterJoint.massScale;
				this.connectedMassScale = characterJoint.connectedMassScale;
			}

			// Token: 0x0600429F RID: 17055 RVA: 0x0018D7F0 File Offset: 0x0018B9F0
			public CharacterJoint CreateJoint(GameObject gameobject, bool resetPosition = true)
			{
				if (resetPosition)
				{
					gameobject.transform.position = this.connectedBody.transform.TransformPoint(this.localPosition);
					gameobject.transform.rotation = this.connectedBody.transform.rotation * this.localRotation;
				}
				CharacterJoint characterJoint = gameobject.AddComponent<CharacterJoint>();
				characterJoint.anchor = this.anchor;
				characterJoint.axis = this.axis;
				characterJoint.autoConfigureConnectedAnchor = this.autoConfigureConnectedAnchor;
				characterJoint.connectedAnchor = this.connectedAnchor;
				characterJoint.swingAxis = this.swingAxis;
				characterJoint.twistLimitSpring = this.twistLimitSpring;
				characterJoint.lowTwistLimit = this.lowTwistLimit;
				characterJoint.highTwistLimit = this.highTwistLimit;
				characterJoint.swingLimitSpring = this.swingLimitSpring;
				characterJoint.swing1Limit = this.swing1Limit;
				characterJoint.swing2Limit = this.swing2Limit;
				characterJoint.enableProjection = this.enableProjection;
				characterJoint.projectionDistance = this.projectionDistance;
				characterJoint.projectionAngle = this.projectionAngle;
				characterJoint.breakForce = this.breakForce;
				characterJoint.breakTorque = this.breakTorque;
				characterJoint.enableCollision = this.enableCollision;
				characterJoint.enablePreprocessing = this.enablePreprocessing;
				characterJoint.massScale = this.massScale;
				characterJoint.connectedMassScale = this.connectedMassScale;
				characterJoint.connectedBody = this.connectedBody;
				return characterJoint;
			}

			// Token: 0x040043AC RID: 17324
			public Vector3 localPosition;

			// Token: 0x040043AD RID: 17325
			public Quaternion localRotation;

			// Token: 0x040043AE RID: 17326
			public Rigidbody connectedBody;

			// Token: 0x040043AF RID: 17327
			public Vector3 anchor;

			// Token: 0x040043B0 RID: 17328
			public Vector3 axis;

			// Token: 0x040043B1 RID: 17329
			public bool autoConfigureConnectedAnchor;

			// Token: 0x040043B2 RID: 17330
			public Vector3 connectedAnchor;

			// Token: 0x040043B3 RID: 17331
			public Vector3 swingAxis;

			// Token: 0x040043B4 RID: 17332
			public SoftJointLimitSpring twistLimitSpring;

			// Token: 0x040043B5 RID: 17333
			public SoftJointLimit lowTwistLimit;

			// Token: 0x040043B6 RID: 17334
			public SoftJointLimit highTwistLimit;

			// Token: 0x040043B7 RID: 17335
			public SoftJointLimitSpring swingLimitSpring;

			// Token: 0x040043B8 RID: 17336
			public SoftJointLimit swing1Limit;

			// Token: 0x040043B9 RID: 17337
			public SoftJointLimit swing2Limit;

			// Token: 0x040043BA RID: 17338
			public bool enableProjection;

			// Token: 0x040043BB RID: 17339
			public float projectionDistance;

			// Token: 0x040043BC RID: 17340
			public float projectionAngle;

			// Token: 0x040043BD RID: 17341
			public float breakForce;

			// Token: 0x040043BE RID: 17342
			public float breakTorque;

			// Token: 0x040043BF RID: 17343
			public bool enableCollision;

			// Token: 0x040043C0 RID: 17344
			public bool enablePreprocessing;

			// Token: 0x040043C1 RID: 17345
			public float massScale;

			// Token: 0x040043C2 RID: 17346
			public float connectedMassScale;
		}
	}
}
