using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000343 RID: 835
	[RequireComponent(typeof(Rigidbody))]
	public class IKTargetZone : MonoBehaviour
	{
		// Token: 0x06002724 RID: 10020 RVA: 0x0010EA85 File Offset: 0x0010CC85
		private void Awake()
		{
			this.GenerateZone();
			if (this.points.Length == 0)
			{
				Debug.LogWarning("'" + base.name + "' has no points, it will be unusable!");
			}
		}

		// Token: 0x06002725 RID: 10021 RVA: 0x0010EAB0 File Offset: 0x0010CCB0
		private void OnDisable()
		{
			this.SetLimbToIKTarget(this.currentPlayer, false);
			this.currentPlayer = null;
		}

		/// <summary>
		/// Generate the zone.
		/// </summary>
		// Token: 0x06002726 RID: 10022 RVA: 0x0010EAC8 File Offset: 0x0010CCC8
		private void GenerateZone()
		{
			this.zone = base.gameObject.AddComponent<BoxCollider>();
			this.zone.isTrigger = true;
			this.zone.size = this.bounds;
			this.zone.center = this.centerOffset;
		}

		/// <summary>
		/// Move the target limb to the IK point if enabled,
		/// move back if otherwise.
		/// </summary>
		// Token: 0x06002727 RID: 10023 RVA: 0x0010EB14 File Offset: 0x0010CD14
		private void SetLimbToIKTarget(Player player, bool enabled)
		{
			if (enabled && this.currentFoot == null)
			{
				this.currentFoot = (this.randomizeFoot ? ((UnityEngine.Random.value < 0.5f) ? player.footLeft : player.footRight) : ((this.defaultFoot == Side.Left) ? player.footLeft : player.footRight));
				this.SetCollisionState(this.currentFoot.ragdollFoot.colliderGroup, false);
			}
			if (this.currentFoot == null || player == null)
			{
				return;
			}
			this.currentFoot.SetFootTracking(enabled);
			this.currentFoot.ragdollFoot.collisionHandler.active = enabled;
			if (!enabled)
			{
				this.SetCollisionState(this.currentFoot.ragdollFoot.colliderGroup, true);
				Vector3 targetPosition = player.creature.transform.TransformPoint(-(player.creature.morphology.legsSpacing / 2f), player.creature.morphology.legsLength, player.creature.morphology.legsLength);
				targetPosition = new Vector3(targetPosition.x, Mathf.Clamp(targetPosition.y, player.creature.transform.position.y, player.creature.transform.position.y + player.creature.morphology.legsLength), targetPosition.z);
				Quaternion lerpRotation = Quaternion.Euler(this.currentFoot.grip.position - player.creature.GetFoot(this.currentFoot.side).upperLegBone.position);
				this.currentFoot.transform.MoveAlign(this.currentFoot.grip.transform, targetPosition, lerpRotation, null);
				this.currentFoot = null;
				return;
			}
			if (this.interpolate && this.points.Length == 2)
			{
				Vector3 interpolatedPosition = this.GetInterpolatedPosition(this.points[0].position, this.points[1].position, this.GetPlayerPosition(player));
				this.currentFoot.transform.MoveAlign(this.currentFoot.grip.transform, interpolatedPosition, this.points[0].rotation, null);
				return;
			}
			Transform closest = this.GetClosestIKPoint(player);
			this.currentFoot.transform.MoveAlign(this.currentFoot.grip.transform, closest.position, closest.rotation, null);
		}

		/// <summary>
		/// Get the players current rotation.
		/// </summary>
		// Token: 0x06002728 RID: 10024 RVA: 0x0010ED88 File Offset: 0x0010CF88
		private Quaternion GetPlayerRotation(Player player)
		{
			return player.creature.ragdoll.GetPart(RagdollPart.Type.Torso).transform.rotation;
		}

		/// <summary>
		/// Get the players position.
		/// </summary>
		// Token: 0x06002729 RID: 10025 RVA: 0x0010EDA8 File Offset: 0x0010CFA8
		private Vector3 GetPlayerPosition(Player player)
		{
			return new Vector3(player.head.transform.position.x, player.transform.position.y, player.head.transform.position.z);
		}

		/// <summary>
		/// Get a positon interpolated between a-b by t.
		/// </summary>
		// Token: 0x0600272A RID: 10026 RVA: 0x0010EDF4 File Offset: 0x0010CFF4
		private Vector3 GetInterpolatedPosition(Vector3 a, Vector3 b, Vector3 t)
		{
			return Vector3.Lerp(a, b, -base.transform.InverseTransformPoint(t).x + 0.1f);
		}

		/// <summary>
		/// Get the closest point to the player.
		/// </summary>
		// Token: 0x0600272B RID: 10027 RVA: 0x0010EE18 File Offset: 0x0010D018
		private Transform GetClosestIKPoint(Player player)
		{
			if (this.points.Length == 0)
			{
				return null;
			}
			Vector3 playerPosition = this.GetPlayerPosition(player);
			Transform closest = this.points[0];
			float closestDistance = Vector3.Distance(closest.position, playerPosition);
			for (int i = 1; i < this.points.Length; i++)
			{
				if (Vector3.Distance(this.points[i].position, playerPosition) < closestDistance)
				{
					closest = this.points[i];
				}
			}
			return closest;
		}

		/// <summary>
		/// Is the player in the target zone?
		/// </summary>
		// Token: 0x0600272C RID: 10028 RVA: 0x0010EE84 File Offset: 0x0010D084
		private bool IsPlayerInZone(Player player)
		{
			Bounds bounds = new Bounds(base.transform.position + this.centerOffset, this.bounds);
			return player != null && (base.gameObject.activeInHierarchy && bounds.Contains(this.GetPlayerPosition(player)) && player.locomotion.isGrounded && !player.locomotion.isJumping) && !player.crouching;
		}

		/// <summary>
		/// Enable or disable a collider group.
		/// May be good for an extension?
		/// </summary>
		// Token: 0x0600272D RID: 10029 RVA: 0x0010EF04 File Offset: 0x0010D104
		private void SetCollisionState(ColliderGroup group, bool enabled)
		{
			foreach (Collider collider in group.colliders)
			{
				collider.enabled = enabled;
			}
		}

		// Token: 0x0600272E RID: 10030 RVA: 0x0010EF58 File Offset: 0x0010D158
		private void OnTriggerEnter(Collider other)
		{
			if ((PlayerControl.input.trackers[1] && PlayerControl.input.trackers[1].IsValid()) || (PlayerControl.input.trackers[2] && PlayerControl.input.trackers[2].IsValid()))
			{
				return;
			}
			if (this.currentPlayer == null && this.points.Length != 0)
			{
				this.currentPlayer = other.GetComponentInParent<Player>();
			}
		}

		// Token: 0x0600272F RID: 10031 RVA: 0x0010EFD8 File Offset: 0x0010D1D8
		private void OnTriggerStay(Collider other)
		{
			if (this.currentPlayer != null)
			{
				if (this.IsPlayerInZone(this.currentPlayer))
				{
					this.SetLimbToIKTarget(this.currentPlayer, true);
					return;
				}
				if (this.currentFoot != null)
				{
					this.SetLimbToIKTarget(this.currentPlayer, false);
				}
			}
		}

		// Token: 0x06002730 RID: 10032 RVA: 0x0010F02A File Offset: 0x0010D22A
		private void OnTriggerExit(Collider other)
		{
			if (!this.IsPlayerInZone(this.currentPlayer))
			{
				this.SetLimbToIKTarget(this.currentPlayer, false);
				this.currentPlayer = null;
			}
		}

		// Token: 0x04002652 RID: 9810
		public Transform[] points = new Transform[0];

		// Token: 0x04002653 RID: 9811
		public bool randomizeFoot;

		// Token: 0x04002654 RID: 9812
		public bool interpolate;

		// Token: 0x04002655 RID: 9813
		public Vector3 bounds;

		// Token: 0x04002656 RID: 9814
		[SerializeField]
		private Side defaultFoot = Side.Left;

		// Token: 0x04002657 RID: 9815
		public Vector3 centerOffset;

		// Token: 0x04002658 RID: 9816
		private BoxCollider zone;

		// Token: 0x04002659 RID: 9817
		private Player currentPlayer;

		// Token: 0x0400265A RID: 9818
		private PlayerFoot currentFoot;
	}
}
