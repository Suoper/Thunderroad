using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200034D RID: 845
	public class KeyHolder : Holder
	{
		// Token: 0x06002782 RID: 10114 RVA: 0x00110A74 File Offset: 0x0010EC74
		public override void Snap(Item item, bool silent = false)
		{
			base.Snap(item, silent);
			if (this.rigidBody != null)
			{
				this.fixedJoint = this.rigidBody.gameObject.AddComponent<FixedJoint>();
				this.fixedJoint.connectedBody = item.physicBody.rigidBody;
				item.physicBody.isEnabled = true;
				foreach (ColliderGroup colliderGroup in item.colliderGroups)
				{
					foreach (Collider collider in colliderGroup.colliders)
					{
						if (collider)
						{
							foreach (Collider collider2 in this.ignoredColliders)
							{
								if (collider2)
								{
									Physics.IgnoreCollision(collider, collider2, false);
								}
							}
							if (this.ignoredColliders.Count == 0 && !collider.isTrigger)
							{
								collider.enabled = true;
							}
						}
					}
				}
			}
			this.keySnapEvent.Invoke();
		}

		// Token: 0x06002783 RID: 10115 RVA: 0x00110BD0 File Offset: 0x0010EDD0
		public override void UnSnap(Item item, bool silent = false)
		{
			if (this.isKeyLock)
			{
				return;
			}
			if (this.fixedJoint != null)
			{
				UnityEngine.Object.Destroy(this.fixedJoint);
			}
			base.UnSnap(item, silent);
			this.keyUnSnapEvent.Invoke();
		}

		// Token: 0x06002784 RID: 10116 RVA: 0x00110C08 File Offset: 0x0010EE08
		public override Interactable.InteractionResult CheckInteraction(RagdollHand ragdollHand)
		{
			if (this.isKeyLock)
			{
				return new Interactable.InteractionResult(ragdollHand, true, false, null, null, null, null, null);
			}
			return base.CheckInteraction(ragdollHand);
		}

		// Token: 0x06002785 RID: 10117 RVA: 0x00110C3A File Offset: 0x0010EE3A
		public void SetLock(bool isLock)
		{
			this.isKeyLock = isLock;
		}

		// Token: 0x06002786 RID: 10118 RVA: 0x00110C43 File Offset: 0x0010EE43
		private void OnJointBreak(float breakForce)
		{
			this.isKeyLock = false;
		}

		// Token: 0x040026A0 RID: 9888
		public bool isKeyLock;

		// Token: 0x040026A1 RID: 9889
		public UnityEvent keySnapEvent;

		// Token: 0x040026A2 RID: 9890
		public UnityEvent keyUnSnapEvent;

		// Token: 0x040026A3 RID: 9891
		public Rigidbody rigidBody;

		// Token: 0x040026A4 RID: 9892
		private FixedJoint fixedJoint;
	}
}
