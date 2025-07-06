using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002CF RID: 719
	public class GravityZone : Zone
	{
		// Token: 0x17000225 RID: 549
		// (get) Token: 0x060022C2 RID: 8898 RVA: 0x000EE688 File Offset: 0x000EC888
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate;
			}
		}

		// Token: 0x060022C3 RID: 8899 RVA: 0x000EE68C File Offset: 0x000EC88C
		protected override void Awake()
		{
			base.Awake();
			this.playerEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.AddGravitySwim));
			this.playerExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.RemoveGravitySwim));
			this.playerEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.StartHeadLocomotion));
			this.playerExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.StopHeadLocomotion));
			this.creatureEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ApplyLocomotionPhysicModifier));
			this.creatureExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.RemoveLocomotionPhysicModifier));
			this.creatureEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ApplyRagdollPhysicModifier));
			this.creatureExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.RemoveRagdollPhysicModifier));
			this.itemEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.ApplyItemPhysicModifier));
			this.itemExitEvent.AddListener(new UnityAction<UnityEngine.Object>(this.RemoveItemPhysicModifier));
			this.creatureEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.PushUpwards));
			this.itemEnterEvent.AddListener(new UnityAction<UnityEngine.Object>(this.PushUpwards));
			this.loopEffectData = Catalog.GetData<EffectData>(this.loopEffectId, true);
		}

		// Token: 0x060022C4 RID: 8900 RVA: 0x000EE7C8 File Offset: 0x000EC9C8
		private static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			Vector3 ab = b - a;
			return Vector3.Dot(value - a, ab) / Vector3.Dot(ab, ab);
		}

		// Token: 0x060022C5 RID: 8901 RVA: 0x000EE7F4 File Offset: 0x000EC9F4
		private void PushUpwards(UnityEngine.Object entityObj)
		{
			if (!this.pushOnEnter)
			{
				return;
			}
			Vector3 root = base.transform.TransformPoint(this.liftRoot);
			Vector3 top = base.transform.TransformPoint(this.liftTop);
			Creature creature = entityObj as Creature;
			if (creature)
			{
				Locomotion locomotion = creature.player ? creature.player.locomotion : creature.locomotion;
				float height = GravityZone.InverseLerp(root, top, locomotion.transform.position);
				locomotion.physicBody.AddForce(base.transform.up * this.enterPushForce * height, ForceMode.Impulse);
				return;
			}
			Item item = entityObj as Item;
			if (item)
			{
				float height2 = GravityZone.InverseLerp(root, top, item.transform.position);
				item.physicBody.AddForce(base.transform.up * this.enterPushForce * height2, ForceMode.Impulse);
			}
		}

		// Token: 0x060022C6 RID: 8902 RVA: 0x000EE8EC File Offset: 0x000ECAEC
		public void AddGravitySwim(UnityEngine.Object playerObj)
		{
			if (!this.allowSwimming)
			{
				return;
			}
			Player player = playerObj as Player;
			if (!player)
			{
				return;
			}
			EffectInstance effectInstance = this.loopEffect;
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			EffectData effectData = this.loopEffectData;
			this.loopEffect = ((effectData != null) ? effectData.Spawn(player.head.transform, true, null, false) : null);
			EffectInstance effectInstance2 = this.loopEffect;
			if (effectInstance2 != null)
			{
				effectInstance2.Play(0, false, false);
			}
			player.handLeft.ragdollHand.swim.AddGravitySwimmingHandler(this, this.swimmingForce);
			player.handRight.ragdollHand.swim.AddGravitySwimmingHandler(this, this.swimmingForce);
			TimeManager.SlowMotionState slowMotionState = TimeManager.slowMotionState;
			if ((slowMotionState == TimeManager.SlowMotionState.Disabled || slowMotionState == TimeManager.SlowMotionState.Stopping) && SnapshotTool.currentSnapshot != ThunderRoadSettings.audioMixerSnapshotMute)
			{
				SnapshotTool.DoSnapshotTransition(ThunderRoadSettings.audioMixerSnapshotUnderwater, 0f);
			}
		}

		// Token: 0x060022C7 RID: 8903 RVA: 0x000EE9C8 File Offset: 0x000ECBC8
		public void RemoveGravitySwim(UnityEngine.Object playerObj)
		{
			if (!this.allowSwimming)
			{
				return;
			}
			Player player = playerObj as Player;
			if (!player)
			{
				return;
			}
			EffectInstance effectInstance = this.loopEffect;
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			player.handLeft.ragdollHand.swim.RemoveGravitySwimmingHandler(this);
			player.handRight.ragdollHand.swim.RemoveGravitySwimmingHandler(this);
			TimeManager.SlowMotionState slowMotionState = TimeManager.slowMotionState;
			if ((slowMotionState == TimeManager.SlowMotionState.Disabled || slowMotionState == TimeManager.SlowMotionState.Stopping) && SnapshotTool.currentSnapshot != ThunderRoadSettings.audioMixerSnapshotMute)
			{
				SnapshotTool.DoSnapshotTransition(ThunderRoadSettings.audioMixerSnapshotDefault, 0f);
			}
		}

		// Token: 0x060022C8 RID: 8904 RVA: 0x000EEA60 File Offset: 0x000ECC60
		private void ApplyLocomotionPhysicModifier(UnityEngine.Object creatureObj)
		{
			Creature creature = creatureObj as Creature;
			if (creature == null)
			{
				return;
			}
			if (!creature.player && !creature.isKilled)
			{
				if (this.destabilize)
				{
					creature.MaxPush(Creature.PushType.Magic, Vector3.zero, (RagdollPart.Type)0);
				}
				if (creature.ragdoll.state == Ragdoll.State.Destabilized)
				{
					creature.ragdoll.forcePhysic.Add(this);
					creature.brain.AddNoStandUpModifier(this);
				}
			}
			Locomotion locomotion = creature.player ? creature.player.locomotion : creature.locomotion;
			float? gravityMultiplier = this.useGravityMultiplierForLocomotion ? new float?(this.gravityMultiplierToApplyToLocomotion) : null;
			Locomotion.PhysicModifier physicModifier = this.physicModifierToApplyToLocomotion;
			float massMultiplier = (physicModifier != null) ? physicModifier.massMultiplier : -1f;
			Locomotion.PhysicModifier physicModifier2 = this.physicModifierToApplyToLocomotion;
			locomotion.SetPhysicModifier(this, gravityMultiplier, massMultiplier, (physicModifier2 != null) ? physicModifier2.dragMultiplier : -1f, GravityZone.duplicateId);
		}

		// Token: 0x060022C9 RID: 8905 RVA: 0x000EEB48 File Offset: 0x000ECD48
		private void RemoveLocomotionPhysicModifier(UnityEngine.Object creatureObj)
		{
			Creature creature = creatureObj as Creature;
			if (creature == null)
			{
				return;
			}
			if (!creature.player && !creature.isKilled)
			{
				creature.ragdoll.forcePhysic.Remove(this);
				creature.brain.RemoveNoStandUpModifier(this);
			}
			if (creature.player)
			{
				creature.player.locomotion.RemovePhysicModifier(this);
			}
			creature.locomotion.RemovePhysicModifier(this);
		}

		// Token: 0x060022CA RID: 8906 RVA: 0x000EEBC4 File Offset: 0x000ECDC4
		private void ApplyRagdollPhysicModifier(UnityEngine.Object creatureObj)
		{
			Creature creature = creatureObj as Creature;
			if (creature == null)
			{
				return;
			}
			if (!creature.player && creature.ragdoll.IsPhysicsEnabled(false))
			{
				for (int i = 0; i < creature.ragdoll.parts.Count; i++)
				{
					CollisionHandler collisionHandler = creature.ragdoll.parts[i].collisionHandler;
					if (collisionHandler != null)
					{
						collisionHandler.SetPhysicModifier(this, this.useGravityMultiplierForItem ? new float?(this.gravityMultiplierToApplyToItem) : null, this.physicModifierToApplyToItem.massMultiplier, this.physicModifierToApplyToItem.drag, this.physicModifierToApplyToItem.angularDrag, this.physicModifierToApplyToItem.sleepThreshold, null);
					}
				}
			}
		}

		// Token: 0x060022CB RID: 8907 RVA: 0x000EEC90 File Offset: 0x000ECE90
		private void RemoveRagdollPhysicModifier(UnityEngine.Object creatureObj)
		{
			Creature creature = creatureObj as Creature;
			if (creature == null)
			{
				return;
			}
			if (!creature.player && creature.ragdoll.IsPhysicsEnabled(false))
			{
				for (int i = 0; i < creature.ragdoll.parts.Count; i++)
				{
					CollisionHandler collisionHandler = creature.ragdoll.parts[i].collisionHandler;
					if (collisionHandler != null)
					{
						collisionHandler.RemovePhysicModifier(this);
					}
				}
			}
		}

		// Token: 0x060022CC RID: 8908 RVA: 0x000EED08 File Offset: 0x000ECF08
		private void ApplyItemPhysicModifier(UnityEngine.Object itemObj)
		{
			Item item = itemObj as Item;
			if (item == null)
			{
				return;
			}
			Item item2 = item;
			float? gravityRatio = this.useGravityMultiplierForItem ? new float?(this.gravityMultiplierToApplyToItem) : null;
			CollisionHandler.PhysicModifier physicModifier = this.physicModifierToApplyToItem;
			float massRatio = (physicModifier != null) ? physicModifier.massMultiplier : -1f;
			CollisionHandler.PhysicModifier physicModifier2 = this.physicModifierToApplyToItem;
			float drag = (physicModifier2 != null) ? physicModifier2.drag : -1f;
			CollisionHandler.PhysicModifier physicModifier3 = this.physicModifierToApplyToItem;
			float angularDrag = (physicModifier3 != null) ? physicModifier3.angularDrag : -1f;
			CollisionHandler.PhysicModifier physicModifier4 = this.physicModifierToApplyToItem;
			item2.SetPhysicModifier(this, gravityRatio, massRatio, drag, angularDrag, (physicModifier4 != null) ? physicModifier4.sleepThreshold : -1f, null);
		}

		// Token: 0x060022CD RID: 8909 RVA: 0x000EEDA4 File Offset: 0x000ECFA4
		private void RemoveItemPhysicModifier(UnityEngine.Object itemObj)
		{
			Item item = itemObj as Item;
			if (item == null)
			{
				return;
			}
			item.RemovePhysicModifier(this);
		}

		// Token: 0x060022CE RID: 8910 RVA: 0x000EEDC9 File Offset: 0x000ECFC9
		private void StartHeadLocomotion(UnityEngine.Object arg0)
		{
			this.isPlayerHeadLocomotionActive = true;
		}

		// Token: 0x060022CF RID: 8911 RVA: 0x000EEDD2 File Offset: 0x000ECFD2
		private void StopHeadLocomotion(UnityEngine.Object arg0)
		{
			this.isPlayerHeadLocomotionActive = false;
		}

		// Token: 0x060022D0 RID: 8912 RVA: 0x000EEDDC File Offset: 0x000ECFDC
		protected internal override void ManagedFixedUpdate()
		{
			base.ManagedFixedUpdate();
			if (this.useHeadLocomotion && this.isPlayerHeadLocomotionActive)
			{
				Vector3 globalDirection = base.transform.localToWorldMatrix * this.headLocomotionLocalDirection;
				globalDirection.Normalize();
				Vector3 headDirection = Player.local.head.transform.forward;
				headDirection.Normalize();
				float dot = Vector3.Dot(headDirection, globalDirection);
				Vector3 velocity;
				if (dot >= 0f)
				{
					velocity = globalDirection * (this.headLocomotionDotCurve.Evaluate(dot) * this.headLocomotionVelocity);
				}
				else
				{
					velocity = -globalDirection * (this.headLocomotionDotCurve.Evaluate(-dot) * this.headLocomotionVelocity);
				}
				Player.local.locomotion.physicBody.AddForce(velocity, ForceMode.VelocityChange);
			}
		}

		/// <summary>
		/// Checks if the given point is inside the given collider.
		/// </summary>
		/// <param name="collider">Collider to check for.</param>
		/// <param name="point">Point to check for.</param>
		/// <returns>True if the point is inside the given collider, false otherwise.</returns>
		// Token: 0x060022D1 RID: 8913 RVA: 0x000EEEAC File Offset: 0x000ED0AC
		public static bool IsPointWithinCollider(Collider collider, Vector3 point)
		{
			return collider && (collider.ClosestPoint(point) - point).sqrMagnitude < Mathf.Epsilon * Mathf.Epsilon;
		}

		// Token: 0x060022D2 RID: 8914 RVA: 0x000EEEE8 File Offset: 0x000ED0E8
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.yellow;
			Vector3 vector = base.transform.TransformPoint(this.liftRoot);
			Vector3 top = base.transform.TransformPoint(this.liftTop);
			Gizmos.DrawLine(vector, top);
			Matrix4x4 oldMatrix = Gizmos.matrix;
			Gizmos.matrix = Matrix4x4.TRS(vector, Quaternion.identity, new Vector3(1f, 0f, 1f));
			Gizmos.DrawWireSphere(Vector3.zero, this.mainCollider.bounds.extents.x);
			Gizmos.matrix = Matrix4x4.TRS(top, Quaternion.identity, new Vector3(1f, 0f, 1f));
			Gizmos.DrawWireSphere(Vector3.zero, this.mainCollider.bounds.extents.x);
			Gizmos.matrix = oldMatrix;
			if (this.allowSwimming && this.mainCollider)
			{
				Gizmos.matrix = base.transform.localToWorldMatrix;
				Gizmos.color = new Color(0.5f, 0.1f, 0.8f, 0.3f);
				BoxCollider box = this.mainCollider as BoxCollider;
				if (box != null)
				{
					Gizmos.DrawCube(box.center, this.mainCollider.bounds.size);
					return;
				}
				SphereCollider sphere = this.mainCollider as SphereCollider;
				if (sphere != null)
				{
					Gizmos.DrawSphere(sphere.center, sphere.radius);
					return;
				}
				CapsuleCollider capsule = this.mainCollider as CapsuleCollider;
				if (capsule != null)
				{
					this.DrawCapsule(capsule.center, Vector3.up, new Color(0.5f, 0.1f, 0.8f, 0.3f), capsule.height / 2f, capsule.radius);
					return;
				}
				MeshCollider meshCollider = this.mainCollider as MeshCollider;
				if (meshCollider != null)
				{
					Gizmos.DrawMesh(meshCollider.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one);
				}
			}
		}

		// Token: 0x060022D3 RID: 8915 RVA: 0x000EF0DC File Offset: 0x000ED2DC
		private void DrawCapsule(Vector3 pos, Vector3 direction, Color color, float capsuleLength, float capsuleRadius)
		{
			Gizmos.color = color;
			Gizmos.DrawWireSphere(pos + direction.normalized * capsuleLength - direction.normalized * capsuleRadius, capsuleRadius);
			Gizmos.DrawWireSphere(pos - direction.normalized * capsuleLength + direction.normalized * capsuleRadius, capsuleRadius);
			Gizmos.DrawRay(Vector3.right * capsuleRadius, Vector3.up * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.right * capsuleRadius, Vector3.down * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.left * capsuleRadius, Vector3.up * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.left * capsuleRadius, Vector3.down * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.forward * capsuleRadius, Vector3.up * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.forward * capsuleRadius, Vector3.down * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.back * capsuleRadius, Vector3.up * capsuleLength / 2f);
			Gizmos.DrawRay(Vector3.back * capsuleRadius, Vector3.down * capsuleLength / 2f);
		}

		// Token: 0x060022D4 RID: 8916 RVA: 0x000EF284 File Offset: 0x000ED484
		private void OnDrawGizmosSelected()
		{
			if (!this.mainCollider)
			{
				return;
			}
			float size = Mathf.Round(Mathf.Log(this.mainCollider.bounds.extents.magnitude * 4f) + 1f);
			float arrowSize = Mathf.Clamp(size, 0f, 1f);
			List<Vector3> list = this.ScatterPoints(this.mainCollider, (int)size, false);
			List<Vector3> offsetPoints = this.ScatterPoints(this.mainCollider, (int)size, true);
			foreach (Vector3 p in list)
			{
				float f = this.useGravityMultiplierForLocomotion ? this.gravityMultiplierToApplyToLocomotion : 0f;
				if (f == 0f)
				{
					Color[,] array = new Color[2, 2];
					array[0, 0] = Color.blue;
					array[0, 1] = Color.blue;
					array[1, 0] = Color.red;
					array[1, 1] = Color.red;
					Gizmos.color = GravityZone.Bilinear(array, Vector2.zero);
					Gizmos.DrawSphere(p, 0.065f);
				}
				else
				{
					Color[,] array2 = new Color[2, 2];
					array2[0, 0] = Color.black;
					array2[0, 1] = Color.blue;
					array2[1, 0] = Color.blue;
					array2[1, 1] = Color.red;
					Gizmos.color = GravityZone.Bilinear(array2, new Vector2(Mathf.Abs(f / 2f), Mathf.Abs(f / 2f)));
					GravityZone.ArrowGizmo(p, -f * 0.35f * arrowSize, 0.1f * arrowSize, 20f);
				}
			}
			foreach (Vector3 p2 in offsetPoints)
			{
				float f2 = this.useGravityMultiplierForItem ? this.gravityMultiplierToApplyToItem : 0f;
				if (f2 == 0f)
				{
					Color[,] array3 = new Color[2, 2];
					array3[0, 0] = Color.green;
					array3[0, 1] = Color.green;
					array3[1, 0] = Color.yellow;
					array3[1, 1] = Color.yellow;
					Gizmos.color = GravityZone.Bilinear(array3, Vector2.zero);
					Gizmos.DrawSphere(p2, 0.065f);
				}
				else
				{
					Color[,] array4 = new Color[2, 2];
					array4[0, 0] = Color.black;
					array4[0, 1] = Color.green;
					array4[1, 0] = Color.green;
					array4[1, 1] = Color.yellow;
					Gizmos.color = GravityZone.Bilinear(array4, new Vector2(Mathf.Abs(f2 / 2f), Mathf.Abs(f2 / 2f)));
					GravityZone.ArrowGizmo(p2, -f2 * 0.35f * arrowSize, 0.1f * arrowSize, 20f);
				}
			}
		}

		// Token: 0x060022D5 RID: 8917 RVA: 0x000EF594 File Offset: 0x000ED794
		private static Color Bilinear(Color[,] corners, Vector2 uv)
		{
			Color cTop = Color.Lerp(corners[0, 1], corners[1, 1], uv.x);
			return Color.Lerp(Color.Lerp(corners[0, 0], corners[1, 0], uv.x), cTop, uv.y);
		}

		// Token: 0x060022D6 RID: 8918 RVA: 0x000EF5E4 File Offset: 0x000ED7E4
		private List<Vector3> ScatterPoints(Collider c, int count, bool offset = false)
		{
			Bounds bounds = this.mainCollider.bounds;
			float sx = bounds.size.x / (float)count * (offset ? 1.5f : 1f);
			float sy = bounds.size.y / (float)count * (offset ? 1.5f : 1f);
			float sz = bounds.size.z / (float)count * (offset ? 1.5f : 1f);
			List<Vector3> points = new List<Vector3>();
			for (int x = 0; x < count + 1; x++)
			{
				for (int y = 0; y < count + 1; y++)
				{
					for (int z = 0; z < count + 1; z++)
					{
						Vector3 p = bounds.center - (offset ? bounds.extents : Vector3.zero) + new Vector3((float)x * sx - bounds.extents.x, (float)y * sy - bounds.extents.y, (float)z * sz - bounds.extents.z);
						if (GravityZone.IsPointWithinCollider(c, p))
						{
							points.Add(p);
						}
					}
				}
			}
			return points;
		}

		// Token: 0x060022D7 RID: 8919 RVA: 0x000EF71C File Offset: 0x000ED91C
		private static void ArrowGizmo(Vector3 pos, float magnitude, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20f)
		{
			if (magnitude == 0f)
			{
				return;
			}
			Vector3 direction = Vector3.up * magnitude;
			Gizmos.DrawRay(pos, direction);
			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f + arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 180f - arrowHeadAngle, 0f) * new Vector3(0f, 0f, 1f);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		// Token: 0x040021D3 RID: 8659
		public static int duplicateId = Animator.StringToHash("GravityZone");

		// Token: 0x040021D4 RID: 8660
		[Header("Effect")]
		public string loopEffectId = "GravityElevator";

		// Token: 0x040021D5 RID: 8661
		protected EffectData loopEffectData;

		// Token: 0x040021D6 RID: 8662
		[Header("Set up")]
		public Vector3 liftRoot;

		// Token: 0x040021D7 RID: 8663
		public Vector3 liftTop;

		// Token: 0x040021D8 RID: 8664
		[Header("Enter")]
		public bool pushOnEnter;

		// Token: 0x040021D9 RID: 8665
		public float enterPushForce = 50f;

		// Token: 0x040021DA RID: 8666
		[Header("Creature")]
		public bool destabilize = true;

		// Token: 0x040021DB RID: 8667
		[Header("Swimming")]
		public bool allowSwimming;

		// Token: 0x040021DC RID: 8668
		public float swimmingForce = 5f;

		// Token: 0x040021DD RID: 8669
		[Header("Locomotion")]
		public bool useGravityMultiplierForLocomotion;

		// Token: 0x040021DE RID: 8670
		public float gravityMultiplierToApplyToLocomotion;

		// Token: 0x040021DF RID: 8671
		[Header("HeadLocomotion")]
		public bool useHeadLocomotion;

		// Token: 0x040021E0 RID: 8672
		public Vector3 headLocomotionLocalDirection;

		// Token: 0x040021E1 RID: 8673
		public AnimationCurve headLocomotionDotCurve;

		// Token: 0x040021E2 RID: 8674
		public float headLocomotionVelocity;

		// Token: 0x040021E3 RID: 8675
		[Header("Item")]
		public bool useGravityMultiplierForItem;

		// Token: 0x040021E4 RID: 8676
		public float gravityMultiplierToApplyToItem = 1f;

		// Token: 0x040021E5 RID: 8677
		public Locomotion.PhysicModifier physicModifierToApplyToLocomotion;

		// Token: 0x040021E6 RID: 8678
		public CollisionHandler.PhysicModifier physicModifierToApplyToItem;

		// Token: 0x040021E7 RID: 8679
		private bool isPlayerHeadLocomotionActive;

		// Token: 0x040021E8 RID: 8680
		protected EffectInstance loopEffect;
	}
}
