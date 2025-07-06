using System;
using System.Collections.Generic;
using ThunderRoad.Splines;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

namespace ThunderRoad
{
	// Token: 0x0200035A RID: 858
	[RequireComponent(typeof(Rigidbody))]
	public class SplineFollower : MonoBehaviour
	{
		// Token: 0x1700026B RID: 619
		// (get) Token: 0x0600282C RID: 10284 RVA: 0x0011335B File Offset: 0x0011155B
		// (set) Token: 0x0600282D RID: 10285 RVA: 0x00113363 File Offset: 0x00111563
		public PhysicBody pb { get; protected set; }

		// Token: 0x1700026C RID: 620
		// (get) Token: 0x0600282E RID: 10286 RVA: 0x0011336C File Offset: 0x0011156C
		// (set) Token: 0x0600282F RID: 10287 RVA: 0x00113374 File Offset: 0x00111574
		public float splineNormalizedPosition { get; protected set; }

		// Token: 0x06002830 RID: 10288 RVA: 0x00113380 File Offset: 0x00111580
		protected void Start()
		{
			this.pb = base.gameObject.GetPhysicBody();
			this.myColliders = new List<Collider>();
			this.myColliders.AddRange(base.GetComponentsInChildren<Collider>());
			for (int i = this.myColliders.Count - 1; i >= 0; i--)
			{
				if (this.myColliders[i].gameObject.GetPhysicBodyInParent() != this.pb)
				{
					this.myColliders.RemoveAt(i);
				}
			}
			if (this.connectedSpline != null)
			{
				ThunderSpline spline = this.connectedSpline;
				this.connectedSpline = null;
				this.ConnectToSpline(spline);
			}
		}

		// Token: 0x06002831 RID: 10289 RVA: 0x00113424 File Offset: 0x00111624
		protected void OnCollisionEnter(Collision collision)
		{
			if (collision.collider.attachedRigidbody != null || collision.collider.attachedArticulationBody != null)
			{
				return;
			}
			ThunderSpline spline = collision.collider.GetComponentInParent<ThunderSpline>();
			if (spline != null || collision.collider.TryGetComponent<ThunderSpline>(out spline))
			{
				this.ConnectToSpline(spline);
			}
		}

		// Token: 0x06002832 RID: 10290 RVA: 0x00113484 File Offset: 0x00111684
		public void ConnectToSpline(ThunderSpline spline)
		{
			if (this.connectedSpline != null)
			{
				this.DisconnectFromSpline();
			}
			this.connectedSpline = spline;
			BezierKnot startKnot = this.connectedSpline.primarySpline[0];
			Vector4 startPos = new Vector4(startKnot.Position.x, startKnot.Position.y, startKnot.Position.z, 1f);
			startPos = this.connectedSpline.transform.localToWorldMatrix * startPos;
			Vector3 vector;
			Vector3 vector2;
			Vector3 vector3;
			Vector3 vector4;
			this.connectedSpline.GetClosestSplinePoint(startPos, out this.connectedSplineT0, out vector, out vector2, out vector3, out vector4);
			int knotCount = this.connectedSpline.primarySpline.Count;
			BezierKnot lastKnot = this.connectedSpline.primarySpline[knotCount - 1];
			Vector4 endPos = new Vector4(lastKnot.Position.x, lastKnot.Position.y, lastKnot.Position.z, 1f);
			endPos = this.connectedSpline.transform.localToWorldMatrix * endPos;
			this.connectedSpline.GetClosestSplinePoint(endPos, out this.connectedSplineTLast, out vector4, out vector3, out vector2, out vector);
			this.SetIgnoreCollision(spline.GetComponentsInChildren<Collider>(), true);
			this.configJoint = base.gameObject.AddComponent<ConfigurableJoint>();
			this.configJoint.anchor = ((this.anchorPoint != null) ? base.transform.InverseTransformPoint(this.anchorPoint.position) : Vector3.zero);
			this.configJoint.autoConfigureConnectedAnchor = false;
			this.ConstrainToSpline();
			this.configJoint.xMotion = ConfigurableJointMotion.Free;
			this.configJoint.yMotion = (this.configJoint.zMotion = ConfigurableJointMotion.Locked);
			if (this.connectedSpline.primarySpline != null)
			{
				BezierKnot knot = this.connectedSpline.primarySpline[0];
				Vector4 tempPos = new Vector4(knot.Position.x, knot.Position.y, knot.Position.z, 1f);
				tempPos = this.connectedSpline.transform.localToWorldMatrix * tempPos;
				this.isAtFirstKnot = (Vector3.Distance(this.pb.transform.position, tempPos) < 0.01f);
				knot = this.connectedSpline.primarySpline[this.connectedSpline.primarySpline.Count - 1];
				tempPos = new Vector4(knot.Position.x, knot.Position.y, knot.Position.z, 1f);
				tempPos = this.connectedSpline.transform.localToWorldMatrix * tempPos;
				this.isAtLastKnot = (Vector3.Distance(this.pb.transform.position, tempPos) < 0.01f);
			}
		}

		// Token: 0x06002833 RID: 10291 RVA: 0x00113765 File Offset: 0x00111965
		public void DisconnectFromSpline()
		{
			this.SetIgnoreCollision(this.connectedSpline.GetComponentsInChildren<Collider>(), false);
			this.connectedSpline = null;
			UnityEngine.Object.Destroy(this.configJoint);
			this.configJoint = null;
		}

		// Token: 0x06002834 RID: 10292 RVA: 0x00113794 File Offset: 0x00111994
		protected void SetIgnoreCollision(Collider[] otherColliders, bool ignore)
		{
			foreach (Collider collider in otherColliders)
			{
				foreach (Collider myCollider in this.myColliders)
				{
					Physics.IgnoreCollision(collider, myCollider, ignore);
				}
			}
		}

		// Token: 0x06002835 RID: 10293 RVA: 0x00113800 File Offset: 0x00111A00
		protected void FixedUpdate()
		{
			if (this.connectedSpline == null)
			{
				return;
			}
			this.ConstrainToSpline();
			if (this.connectedSpline.forceAlongSpline)
			{
				Vector3 force = this.connectedSpline.GetSplineForceAtNormalizedPointWithForward(this.splineNormalizedPosition, this.lastWorldForward);
				this.pb.AddForceAtPosition(force, this.lastWorldPosition, this.connectedSpline.forceCurves[this.connectedSpline.currentForceCurveIndex].forceMode);
			}
			if (this.connectedSpline.splineFriction)
			{
				Vector3 friction = this.connectedSpline.GetSplineFrictionForBodyAtT(this.splineNormalizedPosition, this.pb.velocity);
				this.pb.AddForceAtPosition(friction, this.lastWorldPosition, ForceMode.Force);
			}
			if (this.connectedSpline.primarySpline != null)
			{
				BezierKnot knot = this.connectedSpline.primarySpline[0];
				Vector4 tempPos = new Vector4(knot.Position.x, knot.Position.y, knot.Position.z, 1f);
				tempPos = this.connectedSpline.transform.localToWorldMatrix * tempPos;
				if (Vector3.Distance(this.pb.transform.position, tempPos) < 0.01f)
				{
					if (!this.isAtFirstKnot)
					{
						this.OnReachFirstKnotEvent.Invoke();
						this.connectedSpline.OnReachFirstKnotEvent.Invoke();
						this.isAtFirstKnot = true;
					}
				}
				else
				{
					this.isAtFirstKnot = false;
				}
				knot = this.connectedSpline.primarySpline[this.connectedSpline.primarySpline.Count - 1];
				tempPos = new Vector4(knot.Position.x, knot.Position.y, knot.Position.z, 1f);
				tempPos = this.connectedSpline.transform.localToWorldMatrix * tempPos;
				if (Vector3.Distance(this.pb.transform.position, tempPos) < 0.01f)
				{
					if (!this.isAtLastKnot)
					{
						this.OnReachLastKnotEvent.Invoke();
						this.connectedSpline.OnReachLastKnotEvent.Invoke();
						this.isAtFirstKnot = true;
						return;
					}
				}
				else
				{
					this.isAtLastKnot = false;
				}
			}
		}

		// Token: 0x06002836 RID: 10294 RVA: 0x00113A2C File Offset: 0x00111C2C
		protected void ConstrainToSpline()
		{
			float t;
			Vector3 right;
			this.connectedSpline.GetClosestSplinePoint(base.transform.position, out t, out this.lastWorldPosition, out this.lastWorldForward, out this.lastWorldUp, out right);
			this.splineNormalizedPosition = t;
			this.configJoint.connectedAnchor = this.lastWorldPosition;
			this.configJoint.axis = base.transform.InverseTransformDirection(this.lastWorldForward);
			this.configJoint.secondaryAxis = base.transform.InverseTransformDirection(this.lastWorldUp);
			if (!this.connectedSpline.primarySpline.Closed && ((t <= this.connectedSplineT0 && this.connectedSpline.capStart) || (t >= this.connectedSplineTLast && this.connectedSpline.capEnd)))
			{
				if (this.anchorPoint != null)
				{
					base.transform.MoveAlign(this.anchorPoint, this.configJoint.connectedAnchor, base.transform.rotation, null);
				}
				else
				{
					base.transform.position = this.configJoint.connectedAnchor;
				}
				Vector3 stopVelocity = Vector3.Project(this.pb.velocity, this.lastWorldForward);
				int num = Mathf.RoundToInt(t);
				this.pb.velocity -= stopVelocity;
				if (Vector3.Dot((num == 1) ? Vector3.down : Vector3.up, this.lastWorldForward) > 0f)
				{
					this.pb.AddForceAtPosition(Vector3.Project(-Physics.gravity, this.lastWorldForward), this.lastWorldPosition, ForceMode.Acceleration);
				}
			}
			switch (this.alignmentMode)
			{
			case SplineFollower.SplineAlignmentMode.Soft:
				base.transform.rotation = Quaternion.LookRotation(base.transform.up, this.lastWorldForward);
				base.transform.RotateAround(base.transform.position, base.transform.right, 90f);
				base.transform.RotateAround(base.transform.position, base.transform.up, 180f);
				return;
			case SplineFollower.SplineAlignmentMode.Medium:
				base.transform.rotation = Quaternion.LookRotation(this.lastWorldForward, base.transform.up);
				return;
			case SplineFollower.SplineAlignmentMode.Hard:
				base.transform.rotation = Quaternion.LookRotation(this.lastWorldForward, this.lastWorldUp);
				return;
			default:
				return;
			}
		}

		// Token: 0x04002707 RID: 9991
		public Transform anchorPoint;

		// Token: 0x04002708 RID: 9992
		public ThunderSpline connectedSpline;

		// Token: 0x04002709 RID: 9993
		public SplineFollower.SplineAlignmentMode alignmentMode = SplineFollower.SplineAlignmentMode.Soft;

		// Token: 0x0400270A RID: 9994
		public UnityEvent OnReachFirstKnotEvent;

		// Token: 0x0400270B RID: 9995
		public UnityEvent OnReachLastKnotEvent;

		// Token: 0x0400270C RID: 9996
		private bool isAtFirstKnot;

		// Token: 0x0400270D RID: 9997
		private bool isAtLastKnot;

		// Token: 0x04002710 RID: 10000
		protected ConfigurableJoint configJoint;

		// Token: 0x04002711 RID: 10001
		private Vector3 lastWorldPosition;

		// Token: 0x04002712 RID: 10002
		private Vector3 lastWorldForward;

		// Token: 0x04002713 RID: 10003
		private Vector3 lastWorldUp;

		// Token: 0x04002714 RID: 10004
		private List<Collider> myColliders;

		// Token: 0x04002715 RID: 10005
		private float connectedSplineT0;

		// Token: 0x04002716 RID: 10006
		private float connectedSplineTLast;

		// Token: 0x02000A49 RID: 2633
		public enum SplineAlignmentMode
		{
			// Token: 0x040047B6 RID: 18358
			None,
			// Token: 0x040047B7 RID: 18359
			Soft,
			// Token: 0x040047B8 RID: 18360
			Medium,
			// Token: 0x040047B9 RID: 18361
			Hard
		}
	}
}
