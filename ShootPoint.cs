using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ThunderRoad
{
	// Token: 0x0200027F RID: 639
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ShootPoint.html")]
	public class ShootPoint : ThunderBehaviour
	{
		// Token: 0x06001E10 RID: 7696 RVA: 0x000CC4C0 File Offset: 0x000CA6C0
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			ShootPoint.list.Add(this);
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(base.transform.position, out navMeshHit, 100f, -1) && base.transform.position.y > navMeshHit.position.y)
			{
				this.navPosition = navMeshHit.position;
				return;
			}
			this.navPosition = base.transform.position;
		}

		// Token: 0x170001E1 RID: 481
		// (get) Token: 0x06001E11 RID: 7697 RVA: 0x000CC535 File Offset: 0x000CA735
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x06001E12 RID: 7698 RVA: 0x000CC538 File Offset: 0x000CA738
		protected internal override void ManagedLateUpdate()
		{
			Creature creature = this.currentCreature;
			if (creature != null && creature.state == Creature.State.Dead)
			{
				this.currentCreature = null;
			}
		}

		// Token: 0x06001E13 RID: 7699 RVA: 0x000CC558 File Offset: 0x000CA758
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			ShootPoint.list.Remove(this);
		}

		// Token: 0x06001E14 RID: 7700 RVA: 0x000CC56C File Offset: 0x000CA76C
		public void OnDrawGizmos()
		{
			if (this.allowedAngle > 0f && this.allowedAngle < 360f)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawRay(base.transform.position, Quaternion.AngleAxis(this.allowedAngle * 0.5f, Vector3.up) * base.transform.forward);
				Gizmos.DrawRay(base.transform.position, Quaternion.AngleAxis(-this.allowedAngle * 0.5f, Vector3.up) * base.transform.forward);
			}
			NavMeshHit navMeshHit;
			if (NavMesh.SamplePosition(base.transform.position, out navMeshHit, 100f, -1) && base.transform.position.y > navMeshHit.position.y)
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawSphere(base.transform.position, 0.15f);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(base.transform.position, navMeshHit.position);
				return;
			}
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(base.transform.position, 0.15f);
		}

		// Token: 0x04001C87 RID: 7303
		public static List<ShootPoint> list = new List<ShootPoint>();

		// Token: 0x04001C88 RID: 7304
		[Range(0f, 360f)]
		[Tooltip("Depicts the angle of which the NPC will go to the shootpoint. If the player is outside of the radius, the NPC will choose another path.")]
		public float allowedAngle = 60f;

		// Token: 0x04001C89 RID: 7305
		[NonSerialized]
		public Vector3 navPosition;

		// Token: 0x04001C8A RID: 7306
		[NonSerialized]
		public Creature currentCreature;
	}
}
