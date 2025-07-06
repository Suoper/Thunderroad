using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200027E RID: 638
	public class RagdollPartJointFixer : ThunderBehaviour
	{
		// Token: 0x170001DD RID: 477
		// (get) Token: 0x06001E07 RID: 7687 RVA: 0x000CC2EF File Offset: 0x000CA4EF
		public bool initialized
		{
			get
			{
				return this._initialized;
			}
		}

		// Token: 0x170001DE RID: 478
		// (get) Token: 0x06001E08 RID: 7688 RVA: 0x000CC2F7 File Offset: 0x000CA4F7
		protected override int SliceOverNumFrames
		{
			get
			{
				return 2;
			}
		}

		// Token: 0x170001DF RID: 479
		// (get) Token: 0x06001E09 RID: 7689 RVA: 0x000CC2FA File Offset: 0x000CA4FA
		protected override int GetNextTimeSliceId
		{
			get
			{
				return RagdollPartJointFixer.nextTimeSliceId++;
			}
		}

		// Token: 0x170001E0 RID: 480
		// (get) Token: 0x06001E0A RID: 7690 RVA: 0x000CC309 File Offset: 0x000CA509
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001E0B RID: 7691 RVA: 0x000CC30C File Offset: 0x000CA50C
		public void SetPart(RagdollPart part)
		{
			this.part = part;
			this._initialized = true;
		}

		// Token: 0x06001E0C RID: 7692 RVA: 0x000CC31C File Offset: 0x000CA51C
		private void OnCollisionEnter(Collision collision)
		{
			this.collidingCount++;
		}

		// Token: 0x06001E0D RID: 7693 RVA: 0x000CC32C File Offset: 0x000CA52C
		private void OnCollisionExit(Collision collision)
		{
			this.collidingCount--;
		}

		// Token: 0x06001E0E RID: 7694 RVA: 0x000CC33C File Offset: 0x000CA53C
		protected internal override void ManagedUpdate()
		{
			if (!this.initialized)
			{
				return;
			}
			if (!this.part.initialized)
			{
				return;
			}
			if (this.part.hasParent && !this.part.isSliced && this.collidingCount > 0 && Ragdoll.IsPhysicalState(this.part.ragdoll.state, false) && this.part.ragdoll.handlers.Count == 0)
			{
				Creature currentCreature = Player.currentCreature;
				if (!(((currentCreature != null) ? currentCreature.ragdoll : null) == this.part.ragdoll) && !(this.part.characterJoint == null))
				{
					Vector3 transformPosition = base.transform.position;
					Vector3 parentAnchor = this.part.parentPart.transform.TransformPoint(this.part.characterJoint.connectedAnchor);
					if (transformPosition.PointInRadius(parentAnchor, 0.01f))
					{
						return;
					}
					Vector3 toAnchor = transformPosition - parentAnchor;
					RaycastHit hitInfo;
					if (!Physics.Raycast(parentAnchor, toAnchor.normalized, out hitInfo, toAnchor.magnitude, base.gameObject.layer | ThunderRoadSettings.current.groundLayer, QueryTriggerInteraction.Ignore))
					{
						return;
					}
					if (hitInfo.collider.GetPhysicBody() != this.part.physicBody)
					{
						base.transform.position = hitInfo.point + hitInfo.normal.normalized * 0.1f;
					}
					return;
				}
			}
		}

		// Token: 0x04001C83 RID: 7299
		private bool _initialized;

		// Token: 0x04001C84 RID: 7300
		private RagdollPart part;

		// Token: 0x04001C85 RID: 7301
		private int collidingCount;

		// Token: 0x04001C86 RID: 7302
		private static int nextTimeSliceId;
	}
}
