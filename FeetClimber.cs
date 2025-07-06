using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000258 RID: 600
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/FeetClimber.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Feet climber")]
	public class FeetClimber : ThunderBehaviour
	{
		// Token: 0x06001A52 RID: 6738 RVA: 0x000AF751 File Offset: 0x000AD951
		protected void Awake()
		{
			this.creature = base.GetComponentInParent<Creature>();
			base.transform.rotation = base.transform.rotation * Quaternion.AngleAxis(-this.sweepAngle, Vector3.right);
		}

		// Token: 0x06001A53 RID: 6739 RVA: 0x000AF78C File Offset: 0x000AD98C
		public void Init()
		{
			this.stepAngle = Vector3.Angle(Vector3.forward, new Vector3(0f, this.sphereCastRadius * 2f, this.GetSweepDistance()));
			this.sphereCastHits = new RaycastHit[Mathf.CeilToInt(this.sweepMaxVerticalAngle / this.stepAngle + 1f) * 2 * (Mathf.CeilToInt(this.sweepMaxHorizontalAngle / this.stepAngle) * 2)];
			this.footLeft = new FeetClimber.Foot(this, Side.Left);
			this.footRight = new FeetClimber.Foot(this, Side.Right);
			this.initialized = true;
		}

		// Token: 0x06001A54 RID: 6740 RVA: 0x000AF820 File Offset: 0x000ADA20
		public float GetSweepDistance()
		{
			return this.creature.morphology.legsLength * this.legLenghtMultiplier;
		}

		// Token: 0x06001A55 RID: 6741 RVA: 0x000AF839 File Offset: 0x000ADA39
		public FeetClimber.Foot GetFoot(Side side)
		{
			if (side != Side.Left)
			{
				return this.footRight;
			}
			return this.footLeft;
		}

		// Token: 0x1700019A RID: 410
		// (get) Token: 0x06001A56 RID: 6742 RVA: 0x000AF84C File Offset: 0x000ADA4C
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001A57 RID: 6743 RVA: 0x000AF850 File Offset: 0x000ADA50
		protected internal override void ManagedUpdate()
		{
			if (!this.initialized || !this.creature.isPlayer)
			{
				return;
			}
			if (this.creature.currentLocomotion.isGrounded)
			{
				if (this.footLeft.enabled && !this.creature.player.footLeft.footTracked)
				{
					this.footLeft.StopPose();
				}
				if (this.footRight.enabled && !this.creature.player.footRight.footTracked)
				{
					this.footRight.StopPose();
				}
				if (!this.footLeft.enabled && !this.footRight.enabled)
				{
					this.creature.animator.SetBool(Creature.hashFalling, false);
					return;
				}
			}
			else
			{
				base.transform.position = this.creature.ragdoll.rootPart.transform.position;
				if (this.creature.handLeft.climb.isGripping || this.creature.handRight.climb.isGripping || this.creature.handLeft.collisionHandler.isColliding || this.creature.handRight.collisionHandler.isColliding)
				{
					if (!this.footLeft.enabled && !this.creature.player.footLeft.footTracked)
					{
						this.footLeft.StartPose();
					}
					if (!this.footRight.enabled && !this.creature.player.footRight.footTracked)
					{
						this.footRight.StartPose();
					}
				}
				this.footLeft.Update();
				this.footRight.Update();
			}
		}

		// Token: 0x06001A58 RID: 6744 RVA: 0x000AFA14 File Offset: 0x000ADC14
		protected void CastSweep()
		{
			if (Time.time - this.lastSweep < this.sweepMinDelay)
			{
				return;
			}
			this.sphereCastHitCount = 0;
			for (float currentStepAngleX = 0f; currentStepAngleX < this.sweepMaxHorizontalAngle; currentStepAngleX += this.stepAngle)
			{
				this.SphereCastVertical(currentStepAngleX);
				this.SphereCastVertical(-currentStepAngleX);
			}
			this.footLeft.hitValid = false;
			this.footRight.hitValid = false;
			float smallestAngle = float.PositiveInfinity;
			for (int i = 0; i < this.sphereCastHitCount; i++)
			{
				if (base.transform.InverseTransformPoint(this.sphereCastHits[i].point).x < 0f)
				{
					float angle = Vector3.Angle(this.sphereCastHits[i].normal, base.transform.position - this.sphereCastHits[i].point);
					if (angle < this.footMaxAngle && angle < smallestAngle)
					{
						smallestAngle = angle;
						this.footLeft.raycastHit = this.sphereCastHits[i];
						this.footLeft.hitValid = true;
					}
				}
			}
			smallestAngle = float.PositiveInfinity;
			for (int j = 0; j < this.sphereCastHitCount; j++)
			{
				if (base.transform.InverseTransformPoint(this.sphereCastHits[j].point).x > 0f && (!this.footLeft.hitValid || Vector3.Distance(this.sphereCastHits[j].point, this.footLeft.raycastHit.point) > this.minFootSpacing))
				{
					float angle2 = Vector3.Angle(this.sphereCastHits[j].normal, base.transform.position - this.sphereCastHits[j].point);
					if (angle2 < this.footMaxAngle && angle2 < smallestAngle)
					{
						smallestAngle = angle2;
						this.footRight.raycastHit = this.sphereCastHits[j];
						this.footRight.hitValid = true;
					}
				}
			}
			this.lastSweep = Time.time;
		}

		// Token: 0x06001A59 RID: 6745 RVA: 0x000AFC34 File Offset: 0x000ADE34
		protected void SphereCastVertical(float horizontalAngle)
		{
			RaycastHit hitInfo;
			if (this.SphereCastAngle(0f, horizontalAngle, out hitInfo))
			{
				this.sphereCastHits[this.sphereCastHitCount] = hitInfo;
				this.sphereCastHitCount++;
			}
			float currentStepAngleY = 0f;
			while (currentStepAngleY < this.sweepMaxVerticalAngle)
			{
				currentStepAngleY += this.stepAngle;
				if (this.SphereCastAngle(-currentStepAngleY, horizontalAngle, out hitInfo))
				{
					this.sphereCastHits[this.sphereCastHitCount] = hitInfo;
					this.sphereCastHitCount++;
				}
				if (this.SphereCastAngle(currentStepAngleY, horizontalAngle, out hitInfo))
				{
					this.sphereCastHits[this.sphereCastHitCount] = hitInfo;
					this.sphereCastHitCount++;
				}
			}
		}

		// Token: 0x06001A5A RID: 6746 RVA: 0x000AFCE4 File Offset: 0x000ADEE4
		protected bool SphereCastAngle(float verticalAngle, float horizontalAngle, out RaycastHit hitInfo)
		{
			return Physics.SphereCast(new Ray(base.transform.position, base.transform.rotation * (Quaternion.AngleAxis(verticalAngle, Vector3.right) * Quaternion.AngleAxis(horizontalAngle, Vector3.up) * Vector3.forward)), this.sphereCastRadius, out hitInfo, this.GetSweepDistance() - this.sphereCastRadius, ThunderRoadSettings.current.groundLayer);
		}

		// Token: 0x06001A5B RID: 6747 RVA: 0x000AFD64 File Offset: 0x000ADF64
		protected void OnDrawGizmos()
		{
			for (int i = 0; i < this.sphereCastHitCount; i++)
			{
				if (this.footRight.hitValid && this.sphereCastHits[i].point == this.footRight.raycastHit.point)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawWireSphere(this.sphereCastHits[i].point, this.minFootSpacing);
					Gizmos.DrawRay(this.sphereCastHits[i].point, this.sphereCastHits[i].normal * 0.02f);
					Gizmos.DrawLine(base.transform.position, this.sphereCastHits[i].point);
				}
				if (this.footLeft.hitValid && this.sphereCastHits[i].point == this.footLeft.raycastHit.point)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawWireSphere(this.sphereCastHits[i].point, this.minFootSpacing);
					Gizmos.DrawRay(this.sphereCastHits[i].point, this.sphereCastHits[i].normal * 0.02f);
					Gizmos.DrawLine(base.transform.position, this.sphereCastHits[i].point);
				}
				if (this.showDebug)
				{
					Gizmos.color = Color.gray;
					Gizmos.DrawRay(this.sphereCastHits[i].point, this.sphereCastHits[i].normal * 0.02f);
				}
			}
		}

		// Token: 0x040018FF RID: 6399
		public float footSpeed = 4f;

		// Token: 0x04001900 RID: 6400
		public float sweepAngle = -70f;

		// Token: 0x04001901 RID: 6401
		public float sweepMinDelay = 0.5f;

		// Token: 0x04001902 RID: 6402
		public float sweepMaxVerticalAngle = 30f;

		// Token: 0x04001903 RID: 6403
		public float sweepMaxHorizontalAngle = 30f;

		// Token: 0x04001904 RID: 6404
		public float sphereCastRadius = 0.05f;

		// Token: 0x04001905 RID: 6405
		public float moveOutWeight = 0.2f;

		// Token: 0x04001906 RID: 6406
		public float legLenghtMultiplier = 1.3f;

		// Token: 0x04001907 RID: 6407
		public float minFootSpacing = 0.2f;

		// Token: 0x04001908 RID: 6408
		public float legToHeadMaxAngle = 30f;

		// Token: 0x04001909 RID: 6409
		public float footMaxAngle = 45f;

		// Token: 0x0400190A RID: 6410
		public bool showDebug;

		// Token: 0x0400190B RID: 6411
		public FeetClimber.Foot footLeft;

		// Token: 0x0400190C RID: 6412
		public FeetClimber.Foot footRight;

		// Token: 0x0400190D RID: 6413
		public float stepAngle;

		// Token: 0x0400190E RID: 6414
		public RaycastHit[] sphereCastHits;

		// Token: 0x0400190F RID: 6415
		public int sphereCastHitCount;

		// Token: 0x04001910 RID: 6416
		protected Creature creature;

		// Token: 0x04001911 RID: 6417
		protected float lastSweep;

		// Token: 0x04001912 RID: 6418
		protected bool initialized;

		// Token: 0x020008AB RID: 2219
		[Serializable]
		public class Foot
		{
			// Token: 0x06004107 RID: 16647 RVA: 0x001896EC File Offset: 0x001878EC
			public Foot(FeetClimber feetClimber, Side side)
			{
				this.feetClimber = feetClimber;
				this.side = side;
				this.grip = new GameObject("FootGrip" + side.ToString()).transform;
				this.grip.SetParent(feetClimber.transform);
				this.gripIkAnchor = new GameObject("IkAnchor").transform;
				this.gripIkAnchor.SetParent(this.grip);
			}

			// Token: 0x06004108 RID: 16648 RVA: 0x0018976C File Offset: 0x0018796C
			public void StartPose()
			{
				this.grip.SetParent(null);
				this.state = FeetClimber.Foot.State.Idle;
				this.weight = 0f;
				if (this.feetClimber.creature.isPlayer)
				{
					this.gripIkAnchor.localPosition = this.feetClimber.creature.GetFoot(this.side).grip.transform.InverseTransformPointUnscaled(this.feetClimber.creature.GetFoot(this.side).toesAnchor.position);
					this.gripIkAnchor.localRotation = Quaternion.Inverse(this.feetClimber.creature.GetFoot(this.side).grip.transform.rotation) * this.feetClimber.creature.GetFoot(this.side).toesAnchor.rotation;
				}
				else
				{
					this.gripIkAnchor.localPosition = this.feetClimber.creature.GetFoot(this.side).grip.transform.InverseTransformPointUnscaled(this.feetClimber.creature.GetFoot(this.side).transform.position);
					this.gripIkAnchor.localRotation = Quaternion.Inverse(this.feetClimber.creature.GetFoot(this.side).grip.transform.rotation) * this.feetClimber.creature.GetFoot(this.side).transform.rotation;
				}
				this.feetClimber.creature.animator.SetBool(Creature.hashFalling, true);
				this.feetClimber.creature.ragdoll.ik.SetFootAnchor(this.side, this.gripIkAnchor);
				this.feetClimber.creature.ragdoll.ik.SetFootWeight(this.side, this.weight, this.weight);
				this.enabled = true;
			}

			// Token: 0x06004109 RID: 16649 RVA: 0x00189979 File Offset: 0x00187B79
			public FeetClimber.Foot GetOtherFoot()
			{
				if (this.side != Side.Left)
				{
					return this.feetClimber.footLeft;
				}
				return this.feetClimber.footRight;
			}

			// Token: 0x0600410A RID: 16650 RVA: 0x0018999C File Offset: 0x00187B9C
			public void Update()
			{
				if (!this.enabled)
				{
					return;
				}
				if (this.state == FeetClimber.Foot.State.Idle)
				{
					this.feetClimber.CastSweep();
					if (this.hitValid && ((this.GetOtherFoot().state != FeetClimber.Foot.State.Posed && this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn) || (this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn && Vector3.Distance(this.raycastHit.point, this.GetOtherFoot().grip.position) > this.feetClimber.minFootSpacing)))
					{
						this.state = FeetClimber.Foot.State.MovingIn;
					}
					this.weight = Mathf.MoveTowards(this.weight, 0f, Time.deltaTime * this.feetClimber.footSpeed);
				}
				else if (this.state == FeetClimber.Foot.State.MovingOut)
				{
					this.feetClimber.CastSweep();
					if (this.hitValid)
					{
						this.weight = Mathf.MoveTowards(this.weight, this.feetClimber.moveOutWeight, Time.deltaTime * this.feetClimber.footSpeed);
						if (this.weight == this.feetClimber.moveOutWeight && ((this.GetOtherFoot().state != FeetClimber.Foot.State.Posed && this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn) || (this.GetOtherFoot().state != FeetClimber.Foot.State.MovingIn && Vector3.Distance(this.raycastHit.point, this.GetOtherFoot().grip.position) > this.feetClimber.minFootSpacing)))
						{
							this.state = FeetClimber.Foot.State.MovingIn;
						}
					}
					else
					{
						this.state = FeetClimber.Foot.State.Idle;
					}
				}
				else if (this.state == FeetClimber.Foot.State.MovingIn)
				{
					if (this.grip.position != this.raycastHit.point)
					{
						this.grip.position = this.raycastHit.point;
						this.grip.rotation = Quaternion.LookRotation(this.raycastHit.normal, this.feetClimber.transform.up);
						this.grip.rotation = Quaternion.LookRotation(this.grip.transform.up, this.grip.transform.forward);
						if (Math.Abs(Vector3.SignedAngle(Vector3.up, this.grip.up, this.grip.forward)) > 45f)
						{
							this.grip.rotation = this.grip.rotation * Quaternion.AngleAxis(45f, Vector3.right);
						}
					}
					if (this.PoseValid(this.grip.position))
					{
						this.weight = Mathf.MoveTowards(this.weight, 1f, Time.deltaTime * this.feetClimber.footSpeed);
						if (Math.Abs((double)this.weight - 1.0) < 0.001)
						{
							this.state = FeetClimber.Foot.State.Posed;
						}
					}
					else
					{
						this.state = FeetClimber.Foot.State.MovingOut;
					}
				}
				else if (this.state == FeetClimber.Foot.State.Posed && !this.PoseValid(this.grip.position))
				{
					this.state = FeetClimber.Foot.State.MovingOut;
				}
				this.feetClimber.creature.ragdoll.ik.SetFootWeight(this.side, this.weight, this.weight);
			}

			// Token: 0x0600410B RID: 16651 RVA: 0x00189CE4 File Offset: 0x00187EE4
			protected bool PoseValid(Vector3 position)
			{
				if (Vector3.Distance(position, this.feetClimber.transform.position) > this.feetClimber.creature.morphology.legsLength)
				{
					return false;
				}
				if (this.side == Side.Right)
				{
					if (this.feetClimber.transform.InverseTransformPoint(position).x < 0f)
					{
						return false;
					}
					Vector3 from = this.feetClimber.creature.ragdoll.headPart.bone.animation.transform.position - this.feetClimber.transform.position;
					Vector3 legDirection = this.feetClimber.creature.footRight.lowerLegBone.position - this.feetClimber.creature.footRight.upperLegBone.position;
					if (Vector3.Angle(from, legDirection) < this.feetClimber.legToHeadMaxAngle)
					{
						return false;
					}
				}
				if (this.side == Side.Left)
				{
					if (this.feetClimber.transform.InverseTransformPoint(position).x > 0f)
					{
						return false;
					}
					Vector3 from2 = this.feetClimber.creature.ragdoll.headPart.bone.animation.transform.position - this.feetClimber.transform.position;
					Vector3 legDirection2 = this.feetClimber.creature.footLeft.lowerLegBone.position - this.feetClimber.creature.footLeft.upperLegBone.position;
					if (Vector3.Angle(from2, legDirection2) < this.feetClimber.legToHeadMaxAngle)
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x0600410C RID: 16652 RVA: 0x00189E90 File Offset: 0x00188090
			public void StopPose()
			{
				this.grip.SetParent(this.feetClimber.transform);
				if (this.enabled)
				{
					this.feetClimber.creature.ragdoll.ik.SetFootAnchor(this.side, null);
					this.weight = 0f;
				}
				this.enabled = false;
			}

			// Token: 0x0400423C RID: 16956
			protected FeetClimber feetClimber;

			// Token: 0x0400423D RID: 16957
			public bool enabled;

			// Token: 0x0400423E RID: 16958
			public Side side;

			// Token: 0x0400423F RID: 16959
			public FeetClimber.Foot.State state;

			// Token: 0x04004240 RID: 16960
			public bool hitValid;

			// Token: 0x04004241 RID: 16961
			public RaycastHit raycastHit;

			// Token: 0x04004242 RID: 16962
			public Transform grip;

			// Token: 0x04004243 RID: 16963
			public Transform gripIkAnchor;

			// Token: 0x04004244 RID: 16964
			protected float weight;

			// Token: 0x02000BE0 RID: 3040
			public enum State
			{
				// Token: 0x04004D35 RID: 19765
				Idle,
				// Token: 0x04004D36 RID: 19766
				MovingOut,
				// Token: 0x04004D37 RID: 19767
				MovingIn,
				// Token: 0x04004D38 RID: 19768
				Posed
			}
		}
	}
}
