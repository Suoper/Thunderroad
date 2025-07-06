using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000279 RID: 633
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RagdollFoot")]
	[AddComponentMenu("ThunderRoad/Creatures/Ragdoll foot")]
	public class RagdollFoot : RagdollPart
	{
		// Token: 0x06001D39 RID: 7481 RVA: 0x000C66F8 File Offset: 0x000C48F8
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
				this.grip = new GameObject("Grip").transform;
				this.grip.SetParent(base.transform);
			}
		}

		// Token: 0x06001D3A RID: 7482 RVA: 0x000C6764 File Offset: 0x000C4964
		protected override void OnDrawGizmosSelected()
		{
			base.OnDrawGizmosSelected();
			if (this.grip)
			{
				Gizmos.matrix = this.grip.transform.localToWorldMatrix;
				Common.DrawGizmoArrow(Vector3.zero, Vector3.forward * 0.05f, Common.HueColourValue(HueColorName.Purple), 0.05f, 10f, false);
				Common.DrawGizmoArrow(Vector3.zero, Vector3.up * 0.025f, Common.HueColourValue(HueColorName.Green), 0.025f, 20f, false);
			}
		}

		// Token: 0x06001D3B RID: 7483 RVA: 0x000C67F8 File Offset: 0x000C49F8
		public virtual void Init()
		{
			if (this.toesBone)
			{
				this.toesBone = this.ragdoll.GetBone(this.toesBone).animation;
			}
			if (this.upperLegBone)
			{
				this.upperLegBone = this.ragdoll.GetBone(this.upperLegBone).animation;
			}
			if (this.lowerLegBone)
			{
				this.lowerLegBone = this.ragdoll.GetBone(this.lowerLegBone).animation;
			}
			if (this.side == Side.Left)
			{
				if (!this.toesBone)
				{
					this.toesBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftToes);
				}
				if (!this.upperLegBone)
				{
					this.upperLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
				}
				if (!this.lowerLegBone)
				{
					this.lowerLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
				}
			}
			else if (this.side == Side.Right)
			{
				if (!this.toesBone)
				{
					this.toesBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightToes);
				}
				if (!this.upperLegBone)
				{
					this.upperLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
				}
				if (!this.lowerLegBone)
				{
					this.lowerLegBone = this.ragdoll.creature.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
				}
			}
			if (this.toesBone)
			{
				this.toesAnchor = new GameObject("ToesAnchor").transform;
				this.toesAnchor.SetParent(base.transform);
				this.toesAnchor.position = this.toesBone.position;
				this.toesAnchor.rotation = this.toesBone.rotation;
			}
		}

		// Token: 0x06001D3C RID: 7484 RVA: 0x000C69F4 File Offset: 0x000C4BF4
		public float GetCurrentLegDistance(Space space)
		{
			if (space == Space.Self)
			{
				return Vector3.Distance(this.ragdoll.creature.transform.InverseTransformPoint(this.upperLegBone ? this.upperLegBone.position : this.lowerLegBone.position), this.ragdoll.creature.transform.InverseTransformPoint(this.bone.animation.position));
			}
			return Vector3.Distance(this.upperLegBone ? this.upperLegBone.position : this.lowerLegBone.position, this.bone.animation.position);
		}

		// Token: 0x06001D3D RID: 7485 RVA: 0x000C6AA4 File Offset: 0x000C4CA4
		public float GetLegLenght(Space space)
		{
			if (space != Space.Self)
			{
				return this.ragdoll.creature.morphology.legsLength * this.ragdoll.creature.transform.localScale.y;
			}
			return this.ragdoll.creature.morphology.legsLength;
		}

		// Token: 0x06001D3E RID: 7486 RVA: 0x000C6AFB File Offset: 0x000C4CFB
		public override void RefreshLayer()
		{
			if (this.playerFoot && this.playerFoot.footTracker)
			{
				base.SetLayer(LayerName.ItemAndRagdollOnly);
				return;
			}
			base.RefreshLayer();
		}

		// Token: 0x04001BE5 RID: 7141
		public Side side;

		// Token: 0x04001BE6 RID: 7142
		public Transform grip;

		// Token: 0x04001BE7 RID: 7143
		[Header("Bones (non-humanoid creature only)")]
		[Tooltip("detected automatically for humanoid")]
		public Transform upperLegBone;

		// Token: 0x04001BE8 RID: 7144
		[Tooltip("detected automatically for humanoid")]
		public Transform lowerLegBone;

		// Token: 0x04001BE9 RID: 7145
		[Tooltip("detected automatically for humanoid")]
		public Transform toesBone;

		// Token: 0x04001BEA RID: 7146
		[NonSerialized]
		public Transform toesAnchor;

		// Token: 0x04001BEB RID: 7147
		[NonSerialized]
		public PlayerFoot playerFoot;
	}
}
