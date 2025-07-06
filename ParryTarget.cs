using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B5 RID: 693
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ParryTarget.html")]
	public class ParryTarget : ThunderBehaviour
	{
		// Token: 0x0600218E RID: 8590 RVA: 0x000E6CA9 File Offset: 0x000E4EA9
		public Vector3 GetLineStart()
		{
			return base.transform.position + base.transform.up * this.length;
		}

		// Token: 0x0600218F RID: 8591 RVA: 0x000E6CD1 File Offset: 0x000E4ED1
		public Vector3 GetLineEnd()
		{
			return base.transform.position + -base.transform.up * this.length;
		}

		// Token: 0x06002190 RID: 8592 RVA: 0x000E6CFE File Offset: 0x000E4EFE
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.white;
			Gizmos.DrawLine(this.GetLineStart(), this.GetLineEnd());
		}

		// Token: 0x06002191 RID: 8593 RVA: 0x000E6D1B File Offset: 0x000E4F1B
		protected new virtual void OnDisable()
		{
			if (ParryTarget.list.Contains(this))
			{
				ParryTarget.list.Remove(this);
			}
		}

		// Token: 0x06002192 RID: 8594 RVA: 0x000E6D38 File Offset: 0x000E4F38
		protected void Awake()
		{
			this.item = base.GetComponentInParent<Item>();
			this.item.OnGrabEvent += this.OnGrab;
			this.item.OnUngrabEvent += this.OnRelease;
			this.item.OnFlyEndEvent += this.OnThrowingEnd;
			this.item.OnSnapEvent += this.OnSnap;
		}

		// Token: 0x06002193 RID: 8595 RVA: 0x000E6DAD File Offset: 0x000E4FAD
		protected void OnGrab(Handle handle, RagdollHand ragdollHand)
		{
			this.owner = handle.item.mainHandler.ragdoll.creature;
			if (!ParryTarget.list.Contains(this))
			{
				ParryTarget.list.Add(this);
			}
		}

		// Token: 0x06002194 RID: 8596 RVA: 0x000E6DE2 File Offset: 0x000E4FE2
		protected void OnRelease(Handle handle, RagdollHand ragdollHand, bool throwing)
		{
			if (!throwing)
			{
				this.owner = null;
				if (ParryTarget.list.Contains(this))
				{
					ParryTarget.list.Remove(this);
				}
			}
		}

		// Token: 0x06002195 RID: 8597 RVA: 0x000E6E07 File Offset: 0x000E5007
		protected void OnThrowingEnd(Item _)
		{
			this.owner = null;
			if (ParryTarget.list.Contains(this))
			{
				ParryTarget.list.Remove(this);
			}
		}

		// Token: 0x06002196 RID: 8598 RVA: 0x000E6E29 File Offset: 0x000E5029
		protected void OnSnap(Holder handle)
		{
			if (ParryTarget.list.Contains(this))
			{
				ParryTarget.list.Remove(this);
			}
		}

		// Token: 0x06002197 RID: 8599 RVA: 0x000E6E44 File Offset: 0x000E5044
		public ParryTarget.ParryInfo GetParryPositionAndRotation(Collider defenseCollider)
		{
			ParryTarget.ParryInfo parryInfo;
			parryInfo.parryTarget = this;
			parryInfo.onHitCourse = false;
			parryInfo.windingUp = false;
			Utils.ClosestPointOnSurface(defenseCollider, this.GetLineStart(), this.GetLineEnd(), out parryInfo.colliderPoint, out parryInfo.targetPoint, out parryInfo.insideCollider);
			parryInfo.eventualColliderPoint = parryInfo.colliderPoint;
			parryInfo.targetPointVelocity = this.item.GetItemPointVelocity(parryInfo.targetPoint, this.item.physicBody.isKinematic);
			parryInfo.noHitCourseColliderPoint = parryInfo.colliderPoint;
			RaycastHit forwardHit;
			if (!parryInfo.insideCollider && parryInfo.targetPointVelocity != Vector3.zero && defenseCollider.Raycast(new Ray(parryInfo.targetPoint, parryInfo.targetPointVelocity.normalized), out forwardHit, 10f))
			{
				parryInfo.onHitCourse = true;
				parryInfo.colliderPoint = forwardHit.point;
			}
			RaycastHit backwardHit;
			if (!parryInfo.onHitCourse && parryInfo.targetPointVelocity != Vector3.zero && Vector3.Dot(parryInfo.targetPointVelocity.normalized, parryInfo.parryTarget.item.physicBody.velocity.normalized) > 0.8f && Vector3.Dot(parryInfo.targetPointVelocity.normalized, (parryInfo.colliderPoint - parryInfo.targetPoint).normalized) < 0f && defenseCollider.Raycast(new Ray(parryInfo.targetPoint, -parryInfo.targetPointVelocity.normalized), out backwardHit, 10f))
			{
				parryInfo.windingUp = true;
				parryInfo.eventualColliderPoint = backwardHit.point;
			}
			parryInfo.dir = (parryInfo.insideCollider ? (parryInfo.colliderPoint - parryInfo.targetPoint) : (parryInfo.targetPoint - parryInfo.colliderPoint)).normalized;
			parryInfo.distance = Vector3.Distance(parryInfo.colliderPoint, parryInfo.targetPoint);
			parryInfo.noHitCourseDistance = Vector3.Distance(parryInfo.noHitCourseColliderPoint, parryInfo.targetPoint);
			return parryInfo;
		}

		// Token: 0x06002198 RID: 8600 RVA: 0x000E705C File Offset: 0x000E525C
		private void OnDrawGizmos()
		{
			if (this.testCollider)
			{
				ParryTarget.ParryInfo parryInfo = this.GetParryPositionAndRotation(this.testCollider);
				Gizmos.color = Color.yellow;
				Gizmos.DrawLine(parryInfo.colliderPoint, parryInfo.targetPoint);
				Gizmos.color = (parryInfo.insideCollider ? Color.red : Color.green);
				Gizmos.DrawWireSphere(parryInfo.colliderPoint, 0.01f);
				Gizmos.DrawWireSphere(parryInfo.targetPoint, 0.01f);
			}
		}

		// Token: 0x0400204C RID: 8268
		[Tooltip("Depicts the length of the ParryTarget. With this, AI will know how long your weapon is, and be able to parry it.\n\nCan be adjusted via button to adjust the edge-to-edge gizmo.")]
		public float length = 0.25f;

		// Token: 0x0400204D RID: 8269
		public static List<ParryTarget> list = new List<ParryTarget>();

		// Token: 0x0400204E RID: 8270
		public Collider testCollider;

		// Token: 0x0400204F RID: 8271
		public Side testSide;

		// Token: 0x04002050 RID: 8272
		public Item item;

		// Token: 0x04002051 RID: 8273
		public Creature owner;

		// Token: 0x0200097A RID: 2426
		public struct ParryInfo
		{
			// Token: 0x0400449E RID: 17566
			public ParryTarget parryTarget;

			// Token: 0x0400449F RID: 17567
			public Vector3 colliderPoint;

			// Token: 0x040044A0 RID: 17568
			public Vector3 eventualColliderPoint;

			// Token: 0x040044A1 RID: 17569
			public Vector3 targetPoint;

			// Token: 0x040044A2 RID: 17570
			public Vector3 targetPointVelocity;

			// Token: 0x040044A3 RID: 17571
			public bool onHitCourse;

			// Token: 0x040044A4 RID: 17572
			public bool windingUp;

			// Token: 0x040044A5 RID: 17573
			public Vector3 noHitCourseColliderPoint;

			// Token: 0x040044A6 RID: 17574
			public float noHitCourseDistance;

			// Token: 0x040044A7 RID: 17575
			public bool insideCollider;

			// Token: 0x040044A8 RID: 17576
			public Vector3 dir;

			// Token: 0x040044A9 RID: 17577
			public float distance;
		}
	}
}
