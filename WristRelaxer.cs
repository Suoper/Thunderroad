using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000283 RID: 643
	[AddComponentMenu("ThunderRoad/Creatures/Wrist relaxer")]
	public class WristRelaxer : MonoBehaviour
	{
		// Token: 0x06001E51 RID: 7761 RVA: 0x000CE3AC File Offset: 0x000CC5AC
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (!this.upperArmBone || !this.lowerArmBone || !this.handBone || !this.armTwistBone)
			{
				RagdollHand ragdollHand = base.GetComponentInParent<RagdollHand>();
				Creature creature = base.GetComponentInParent<Creature>();
				if (creature && creature.animator && ragdollHand)
				{
					if (!this.upperArmBone)
					{
						this.upperArmBone = creature.animator.GetBoneTransform((ragdollHand.side == Side.Right) ? HumanBodyBones.RightUpperArm : HumanBodyBones.LeftUpperArm);
					}
					if (!this.lowerArmBone)
					{
						this.lowerArmBone = creature.animator.GetBoneTransform((ragdollHand.side == Side.Right) ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm);
					}
					if (!this.armTwistBone)
					{
						this.armTwistBone = creature.animator.GetBoneTransform((ragdollHand.side == Side.Right) ? HumanBodyBones.RightLowerArm : HumanBodyBones.LeftLowerArm).GetChild(0);
					}
					if (!this.handBone)
					{
						this.handBone = creature.animator.GetBoneTransform((ragdollHand.side == Side.Right) ? HumanBodyBones.RightHand : HumanBodyBones.LeftHand);
					}
				}
			}
		}

		// Token: 0x06001E52 RID: 7762 RVA: 0x000CE4E8 File Offset: 0x000CC6E8
		protected void Awake()
		{
			this.ragdollHand = base.GetComponentInParent<RagdollHand>();
		}

		// Token: 0x06001E53 RID: 7763 RVA: 0x000CE4F8 File Offset: 0x000CC6F8
		public void Init()
		{
			this.ragdollHand.creature.ragdoll.ik.OnPostIKUpdateEvent += this.OnPostIKUpdate;
			this.armTwistBone = this.ragdollHand.ragdoll.GetBone(this.armTwistBone).animation;
			this.upperArmBone = this.ragdollHand.ragdoll.GetBone(this.upperArmBone).animation;
			this.lowerArmBoneMesh = this.lowerArmBone;
			this.lowerArmBone = this.ragdollHand.ragdoll.GetBone(this.lowerArmBone).animation;
			this.handBone = this.ragdollHand.ragdoll.GetBone(this.handBone).animation;
			this.twistAxis = this.lowerArmBone.InverseTransformDirection(this.handBone.position - this.lowerArmBone.position);
			this.axis = new Vector3(this.twistAxis.y, this.twistAxis.z, this.twistAxis.x);
			Vector3 axisWorld = this.lowerArmBone.rotation * this.axis;
			this.axisRelativeToParentDefault = Quaternion.Inverse(this.upperArmBone.rotation) * axisWorld;
			this.axisRelativeToChildDefault = Quaternion.Inverse(this.handBone.rotation) * axisWorld;
			if (this.armTwistBone == this.handBone)
			{
				Debug.LogWarningFormat(this, "No twist bone found on skeleton, wristRelaxer cannot work correctly when grabbing objects", Array.Empty<object>());
				this.noTwistBone = true;
			}
		}

		// Token: 0x06001E54 RID: 7764 RVA: 0x000CE68C File Offset: 0x000CC88C
		private void OnPostIKUpdate()
		{
			if (!this.ragdollHand || this.weight <= 0f || this.ragdollHand.isSliced || !base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.noTwistBone && this.ragdollHand.grabbedHandle)
			{
				return;
			}
			Quaternion rotation = this.lowerArmBone.rotation;
			Quaternion lhs = Quaternion.AngleAxis(this.twistAngleOffset, rotation * this.twistAxis);
			rotation = lhs * rotation;
			Vector3 relaxedAxisParent = lhs * this.upperArmBone.rotation * this.axisRelativeToParentDefault;
			Vector3 relaxedAxisChild = lhs * this.handBone.rotation * this.axisRelativeToChildDefault;
			Vector3 relaxedAxis = Vector3.Slerp(relaxedAxisParent, relaxedAxisChild, this.parentChildCrossfade);
			relaxedAxis = Quaternion.Inverse(Quaternion.LookRotation(rotation * this.axis, rotation * this.twistAxis)) * relaxedAxis;
			float angle = Mathf.Atan2(relaxedAxis.x, relaxedAxis.z) * 57.29578f;
			this.armTwistBone.localRotation = Quaternion.AngleAxis(angle * this.weight, this.twistAxis);
			this.lowerArmBoneMesh.localRotation = Quaternion.AngleAxis(angle * this.weightArm, this.twistAxis);
		}

		// Token: 0x04001CBF RID: 7359
		public Transform armTwistBone;

		// Token: 0x04001CC0 RID: 7360
		public Transform upperArmBone;

		// Token: 0x04001CC1 RID: 7361
		public Transform lowerArmBone;

		// Token: 0x04001CC2 RID: 7362
		public Transform handBone;

		// Token: 0x04001CC3 RID: 7363
		[Tooltip("The weight of relaxing the twist")]
		[Range(0f, 1f)]
		public float weight = 1f;

		// Token: 0x04001CC4 RID: 7364
		[Tooltip("The weight of relaxing the arm of this Transform (UMA)")]
		[Range(0f, 1f)]
		public float weightArm = 0.5f;

		// Token: 0x04001CC5 RID: 7365
		[Tooltip("If 0.5, will be twisted half way from parent to child. If 1, the twist angle will be locked to the child and will rotate with along with it.")]
		[Range(0f, 1f)]
		public float parentChildCrossfade = 0.5f;

		// Token: 0x04001CC6 RID: 7366
		[Tooltip("Rotation offset around the twist axis.")]
		[Range(-180f, 180f)]
		public float twistAngleOffset;

		// Token: 0x04001CC7 RID: 7367
		protected bool noTwistBone;

		// Token: 0x04001CC8 RID: 7368
		protected RagdollHand ragdollHand;

		// Token: 0x04001CC9 RID: 7369
		private Transform lowerArmBoneMesh;

		// Token: 0x04001CCA RID: 7370
		private Vector3 twistAxis = Vector3.right;

		// Token: 0x04001CCB RID: 7371
		private Vector3 axis = Vector3.forward;

		// Token: 0x04001CCC RID: 7372
		private Vector3 axisRelativeToParentDefault;

		// Token: 0x04001CCD RID: 7373
		private Vector3 axisRelativeToChildDefault;
	}
}
