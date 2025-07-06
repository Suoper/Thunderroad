using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AI;

namespace ThunderRoad
{
	// Token: 0x02000281 RID: 641
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/WayPoint.html")]
	public class WayPoint : MonoBehaviour
	{
		// Token: 0x06001E40 RID: 7744 RVA: 0x000CD8D9 File Offset: 0x000CBAD9
		public List<ValueDropdownItem<string>> GetAllAnimationID()
		{
			return Catalog.GetDropdownAllID(Category.Animation, "None");
		}

		// Token: 0x06001E41 RID: 7745 RVA: 0x000CD8E7 File Offset: 0x000CBAE7
		private void Awake()
		{
			if (this.animationId != null && this.animationId != "")
			{
				this.animationData = Catalog.GetData<AnimationData>(this.animationId, true);
			}
		}

		// Token: 0x06001E42 RID: 7746 RVA: 0x000CD915 File Offset: 0x000CBB15
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			WayPoint.navMeshPath = new NavMeshPath();
		}

		// Token: 0x06001E43 RID: 7747 RVA: 0x000CD930 File Offset: 0x000CBB30
		public void OnDrawGizmos()
		{
			if (base.transform.parent)
			{
				int index = base.transform.GetSiblingIndex();
				Transform nextWaypoint = base.transform.parent.GetChild((index < base.transform.parent.childCount - 1) ? (index + 1) : 0);
				if (nextWaypoint)
				{
					NavMesh.CalculatePath(base.transform.position, nextWaypoint.transform.position, -1, WayPoint.navMeshPath);
					if (WayPoint.navMeshPath.status == NavMeshPathStatus.PathComplete)
					{
						float arrowLenght = 0.5f;
						float arrowPosition = 0.8f;
						int arrowAngle = 40;
						Color groundLinkColor = Color.white;
						Color linkColor = Color.magenta;
						Gizmos.color = ((WayPoint.navMeshPath.status == NavMeshPathStatus.PathPartial) ? Color.yellow : groundLinkColor);
						Gizmos.DrawSphere(base.transform.position, 0.15f);
						Gizmos.DrawLine(base.transform.position, WayPoint.navMeshPath.corners[0]);
						if (this.turnToDirection)
						{
							Common.DrawGizmoArrow(base.transform.position, base.transform.forward * 0.5f, Gizmos.color, 0.25f, 20f, false);
						}
						for (int i = 0; i < WayPoint.navMeshPath.corners.Length; i++)
						{
							Vector3 dir = default(Vector3);
							if (i < WayPoint.navMeshPath.corners.Length - 1)
							{
								dir = WayPoint.navMeshPath.corners[i + 1] - WayPoint.navMeshPath.corners[i];
							}
							if (dir != Vector3.zero)
							{
								Gizmos.color = linkColor;
								Gizmos.DrawRay(WayPoint.navMeshPath.corners[i], dir);
								Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, (float)(180 + arrowAngle), 0f) * Vector3.forward;
								Vector3 left = Quaternion.LookRotation(dir) * Quaternion.Euler(0f, (float)(180 - arrowAngle), 0f) * Vector3.forward;
								Gizmos.DrawRay(WayPoint.navMeshPath.corners[i] + dir * arrowPosition, right * arrowLenght);
								Gizmos.DrawRay(WayPoint.navMeshPath.corners[i] + dir * arrowPosition, left * arrowLenght);
							}
						}
						return;
					}
					if (WayPoint.navMeshPath.status == NavMeshPathStatus.PathInvalid)
					{
						Gizmos.color = Color.red;
						Gizmos.DrawSphere(base.transform.position, 0.15f);
					}
				}
			}
		}

		// Token: 0x06001E44 RID: 7748 RVA: 0x000CDBEC File Offset: 0x000CBDEC
		public void OnDrawGizmosSelected()
		{
			if (base.transform.parent)
			{
				base.name = "Waypoint" + base.transform.GetSiblingIndex().ToString();
				return;
			}
			base.name = "Waypoint_PleaseINeedAParent";
		}

		// Token: 0x06001E45 RID: 7749 RVA: 0x000CDC3C File Offset: 0x000CBE3C
		public static void SpawnerDrawGizmos(Transform spawner, Transform waypoints)
		{
			if (waypoints && waypoints.childCount > 0)
			{
				Vector3 position = spawner.position;
				Vector3 position2 = waypoints.GetChild(0).transform.position;
				int areaMask = -1;
				NavMeshPath path;
				if ((path = WayPoint.navMeshPath) == null)
				{
					path = (WayPoint.navMeshPath = new NavMeshPath());
				}
				NavMesh.CalculatePath(position, position2, areaMask, path);
				if (WayPoint.navMeshPath.status == NavMeshPathStatus.PathComplete)
				{
					if (spawner.position.y > WayPoint.navMeshPath.corners[0].y)
					{
						Gizmos.color = ((WayPoint.navMeshPath.status == NavMeshPathStatus.PathPartial) ? Color.yellow : Color.gray);
						Gizmos.DrawSphere(spawner.position, 0.15f);
						Gizmos.color = ((WayPoint.navMeshPath.status == NavMeshPathStatus.PathPartial) ? Color.yellow : Color.green);
						Gizmos.DrawLine(spawner.position, WayPoint.navMeshPath.corners[0]);
						for (int i = 0; i < WayPoint.navMeshPath.corners.Length; i++)
						{
							Vector3 dir = default(Vector3);
							if (i < WayPoint.navMeshPath.corners.Length - 1)
							{
								dir = WayPoint.navMeshPath.corners[i + 1] - WayPoint.navMeshPath.corners[i];
							}
							if (dir != Vector3.zero)
							{
								Gizmos.color = Color.gray;
								Gizmos.DrawRay(WayPoint.navMeshPath.corners[i], dir);
							}
						}
						return;
					}
					Gizmos.color = Color.red;
					Gizmos.DrawSphere(spawner.position, 0.15f);
					return;
				}
				else if (WayPoint.navMeshPath.status == NavMeshPathStatus.PathInvalid)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawSphere(spawner.position, 0.15f);
					return;
				}
			}
			else
			{
				NavMeshHit navMeshHit;
				if (NavMesh.SamplePosition(spawner.position, out navMeshHit, 2f, -1) && spawner.position.y > navMeshHit.position.y)
				{
					Gizmos.color = Color.gray;
					Gizmos.DrawSphere(spawner.position, 0.15f);
					Gizmos.color = Color.green;
					Gizmos.DrawLine(spawner.position, navMeshHit.position);
					return;
				}
				Gizmos.color = Color.red;
				Gizmos.DrawSphere(spawner.position, 0.15f);
			}
		}

		// Token: 0x04001CAC RID: 7340
		[Header("Turn")]
		[Tooltip("When ticked, does the creature turn to the direction of the Z axis/Blue arrow.")]
		public bool turnToDirection;

		// Token: 0x04001CAD RID: 7341
		[Tooltip("The speed of which the creature turns to direction.")]
		public float turnSpeedRatio = 1f;

		// Token: 0x04001CAE RID: 7342
		[Header("Wait")]
		[Tooltip("How long the NPC waits for at the waypoint.")]
		public Vector2 waitMinMaxDuration = new Vector2(0f, 0f);

		// Token: 0x04001CAF RID: 7343
		[Header("Animation")]
		[Tooltip("When enabled, the creature will play the listed animation when at the waypoint")]
		public bool playAnimation;

		// Token: 0x04001CB0 RID: 7344
		[Tooltip("ID of the animation you want the creature to play")]
		public string animationId;

		// Token: 0x04001CB1 RID: 7345
		[Tooltip("The minimum angle of which the creature turns during the animation")]
		public float animationTurnMinAngle = 30f;

		// Token: 0x04001CB2 RID: 7346
		[Tooltip("Specify the delay before playing an animation at the waypoint.")]
		public Vector2 animationRandomMinMaxDelay = new Vector2(0f, 0f);

		// Token: 0x04001CB3 RID: 7347
		[Header("Action subtree")]
		[Tooltip("Sets a behaviour tree the creature tries to complete while standing at this waypoint")]
		public string actionBehaviorTreeID = "";

		// Token: 0x04001CB4 RID: 7348
		[Tooltip("Sets an object for the creature to target if the behaviour tree needs one")]
		public Transform target;

		// Token: 0x04001CB5 RID: 7349
		[Tooltip("How many times the creature needs to succeed the behaviour tree to proceed")]
		public int requiredTreeSuccessCount = 3;

		// Token: 0x04001CB6 RID: 7350
		[Tooltip("How many times the creature should be able to fail the behaviour tree before it stops trying")]
		public int failuresToSkip = 10;

		// Token: 0x04001CB7 RID: 7351
		protected static NavMeshPath navMeshPath;

		// Token: 0x04001CB8 RID: 7352
		[NonSerialized]
		public AnimationData animationData;
	}
}
