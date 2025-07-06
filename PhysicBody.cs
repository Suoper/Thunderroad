using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000350 RID: 848
	[Serializable]
	public class PhysicBody
	{
		// Token: 0x06002795 RID: 10133 RVA: 0x00110FF2 File Offset: 0x0010F1F2
		public PhysicBody(Rigidbody rigidbody)
		{
			this.rigidBody = rigidbody;
			this.transform = rigidbody.transform;
			this.gameObject = rigidbody.gameObject;
		}

		// Token: 0x06002796 RID: 10134 RVA: 0x00111019 File Offset: 0x0010F219
		public PhysicBody(Transform transform)
		{
			this.transform = transform;
			this.gameObject = transform.gameObject;
			this.rigidBody = transform.GetComponent<Rigidbody>();
		}

		// Token: 0x17000258 RID: 600
		// (get) Token: 0x06002797 RID: 10135 RVA: 0x00111040 File Offset: 0x0010F240
		// (set) Token: 0x06002798 RID: 10136 RVA: 0x00111068 File Offset: 0x0010F268
		public Vector3 centerOfMass
		{
			get
			{
				if (!this.comInitialSet)
				{
					this._centerOfMass = this.rigidBody.centerOfMass;
					this.comInitialSet = true;
				}
				return this._centerOfMass;
			}
			set
			{
				this.rigidBody.centerOfMass = value;
				this._centerOfMass = value;
				Action action = this.onCenterOfMassChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x17000259 RID: 601
		// (get) Token: 0x06002799 RID: 10137 RVA: 0x0011108D File Offset: 0x0010F28D
		public Vector3 worldCenterOfMass
		{
			get
			{
				return this.rigidBody.worldCenterOfMass;
			}
		}

		// Token: 0x1700025A RID: 602
		// (get) Token: 0x0600279A RID: 10138 RVA: 0x0011109A File Offset: 0x0010F29A
		// (set) Token: 0x0600279B RID: 10139 RVA: 0x001110CC File Offset: 0x0010F2CC
		public Vector3 velocity
		{
			get
			{
				if (this.velocityLastUpdate < UpdateManager.fixedFrameCount)
				{
					this.velocityLastUpdate = UpdateManager.fixedFrameCount;
					this._velocity = this.rigidBody.velocity;
				}
				return this._velocity;
			}
			set
			{
				Rigidbody rigidbody = this.rigidBody;
				this._velocity = value;
				rigidbody.velocity = value;
			}
		}

		// Token: 0x1700025B RID: 603
		// (get) Token: 0x0600279C RID: 10140 RVA: 0x001110EE File Offset: 0x0010F2EE
		// (set) Token: 0x0600279D RID: 10141 RVA: 0x00111120 File Offset: 0x0010F320
		public Vector3 angularVelocity
		{
			get
			{
				if (this.angularVelocityLastUpdate < UpdateManager.fixedFrameCount)
				{
					this.angularVelocityLastUpdate = UpdateManager.fixedFrameCount;
					this._angularVelocity = this.rigidBody.angularVelocity;
				}
				return this._angularVelocity;
			}
			set
			{
				Rigidbody rigidbody = this.rigidBody;
				this._angularVelocity = value;
				rigidbody.angularVelocity = value;
			}
		}

		// Token: 0x1700025C RID: 604
		// (get) Token: 0x0600279E RID: 10142 RVA: 0x00111142 File Offset: 0x0010F342
		// (set) Token: 0x0600279F RID: 10143 RVA: 0x0011114F File Offset: 0x0010F34F
		public Vector3 inertiaTensor
		{
			get
			{
				return this.rigidBody.inertiaTensor;
			}
			set
			{
				if (this._isForcedFreeze)
				{
					this._unfreezInertiaTensor = value;
				}
				else
				{
					this.rigidBody.inertiaTensor = value;
				}
				Action action = this.onInertiaTensorChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x1700025D RID: 605
		// (get) Token: 0x060027A0 RID: 10144 RVA: 0x0011117E File Offset: 0x0010F37E
		// (set) Token: 0x060027A1 RID: 10145 RVA: 0x0011118B File Offset: 0x0010F38B
		public Quaternion inertiaTensorRotation
		{
			get
			{
				return this.rigidBody.inertiaTensorRotation;
			}
			set
			{
				if (this._isForcedFreeze)
				{
					this._unfreezInertiaTensorRotation = value;
				}
				else
				{
					this.rigidBody.inertiaTensorRotation = value;
				}
				Action action = this.onInertiaTensorChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x1700025E RID: 606
		// (get) Token: 0x060027A2 RID: 10146 RVA: 0x001111BA File Offset: 0x0010F3BA
		// (set) Token: 0x060027A3 RID: 10147 RVA: 0x001111C7 File Offset: 0x0010F3C7
		public float drag
		{
			get
			{
				return this.rigidBody.drag;
			}
			set
			{
				this.rigidBody.drag = value;
			}
		}

		// Token: 0x1700025F RID: 607
		// (get) Token: 0x060027A4 RID: 10148 RVA: 0x001111D5 File Offset: 0x0010F3D5
		// (set) Token: 0x060027A5 RID: 10149 RVA: 0x001111E2 File Offset: 0x0010F3E2
		public float angularDrag
		{
			get
			{
				return this.rigidBody.angularDrag;
			}
			set
			{
				this.rigidBody.angularDrag = value;
			}
		}

		// Token: 0x17000260 RID: 608
		// (get) Token: 0x060027A6 RID: 10150 RVA: 0x001111F0 File Offset: 0x0010F3F0
		// (set) Token: 0x060027A7 RID: 10151 RVA: 0x001111FD File Offset: 0x0010F3FD
		public float sleepThreshold
		{
			get
			{
				return this.rigidBody.sleepThreshold;
			}
			set
			{
				this.rigidBody.sleepThreshold = value;
			}
		}

		// Token: 0x17000261 RID: 609
		// (get) Token: 0x060027A8 RID: 10152 RVA: 0x0011120B File Offset: 0x0010F40B
		// (set) Token: 0x060027A9 RID: 10153 RVA: 0x00111218 File Offset: 0x0010F418
		public float mass
		{
			get
			{
				return this.rigidBody.mass;
			}
			set
			{
				this.rigidBody.mass = value;
				Action action = this.onMassChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x17000262 RID: 610
		// (get) Token: 0x060027AA RID: 10154 RVA: 0x00111236 File Offset: 0x0010F436
		// (set) Token: 0x060027AB RID: 10155 RVA: 0x00111243 File Offset: 0x0010F443
		public bool useGravity
		{
			get
			{
				return this.rigidBody.useGravity;
			}
			set
			{
				this.rigidBody.useGravity = value;
				Action action = this.onGravityChanged;
				if (action == null)
				{
					return;
				}
				action();
			}
		}

		// Token: 0x17000263 RID: 611
		// (get) Token: 0x060027AC RID: 10156 RVA: 0x00111261 File Offset: 0x0010F461
		// (set) Token: 0x060027AD RID: 10157 RVA: 0x0011126E File Offset: 0x0010F46E
		public RigidbodyInterpolation interpolation
		{
			get
			{
				return this.rigidBody.interpolation;
			}
			set
			{
				this.rigidBody.interpolation = value;
			}
		}

		// Token: 0x17000264 RID: 612
		// (get) Token: 0x060027AE RID: 10158 RVA: 0x0011127C File Offset: 0x0010F47C
		// (set) Token: 0x060027AF RID: 10159 RVA: 0x00111289 File Offset: 0x0010F489
		public bool freezeRotation
		{
			get
			{
				return this.rigidBody.freezeRotation;
			}
			set
			{
				this.rigidBody.freezeRotation = value;
			}
		}

		// Token: 0x17000265 RID: 613
		// (get) Token: 0x060027B0 RID: 10160 RVA: 0x00111297 File Offset: 0x0010F497
		// (set) Token: 0x060027B1 RID: 10161 RVA: 0x001112A2 File Offset: 0x0010F4A2
		public bool isEnabled
		{
			get
			{
				return !this.isKinematic;
			}
			set
			{
				this.isKinematic = !value;
			}
		}

		// Token: 0x17000266 RID: 614
		// (get) Token: 0x060027B2 RID: 10162 RVA: 0x001112AE File Offset: 0x0010F4AE
		// (set) Token: 0x060027B3 RID: 10163 RVA: 0x001112E0 File Offset: 0x0010F4E0
		public bool isKinematic
		{
			get
			{
				if (this.isKinematicLastUpdate < UpdateManager.fixedFrameCount)
				{
					this.isKinematicLastUpdate = UpdateManager.fixedFrameCount;
					this._isKinematic = this.rigidBody.isKinematic;
				}
				return this._isKinematic;
			}
			set
			{
				Rigidbody rigidbody = this.rigidBody;
				this._isKinematic = value;
				rigidbody.isKinematic = value;
			}
		}

		// Token: 0x17000267 RID: 615
		// (get) Token: 0x060027B4 RID: 10164 RVA: 0x00111302 File Offset: 0x0010F502
		// (set) Token: 0x060027B5 RID: 10165 RVA: 0x0011130F File Offset: 0x0010F50F
		public CollisionDetectionMode collisionDetectionMode
		{
			get
			{
				return this.rigidBody.collisionDetectionMode;
			}
			set
			{
				this.rigidBody.collisionDetectionMode = value;
			}
		}

		// Token: 0x17000268 RID: 616
		// (get) Token: 0x060027B6 RID: 10166 RVA: 0x0011131D File Offset: 0x0010F51D
		// (set) Token: 0x060027B7 RID: 10167 RVA: 0x00111339 File Offset: 0x0010F539
		public RigidbodyConstraints constraints
		{
			get
			{
				if (this._isForcedFreeze)
				{
					return this._unfreezeConstraint;
				}
				return this.rigidBody.constraints;
			}
			set
			{
				if (this._isForcedFreeze)
				{
					this._unfreezeConstraint = value;
					return;
				}
				this.rigidBody.constraints = value;
			}
		}

		// Token: 0x060027B8 RID: 10168 RVA: 0x00111358 File Offset: 0x0010F558
		public void ForceFreeze()
		{
			if (this._isForcedFreeze)
			{
				return;
			}
			this._unfreezeConstraint = this.constraints;
			this._unfreezInertiaTensorRotation = this.rigidBody.inertiaTensorRotation;
			this._unfreezInertiaTensor = this.rigidBody.inertiaTensor;
			this.rigidBody.constraints = RigidbodyConstraints.FreezeAll;
			this._isForcedFreeze = true;
		}

		// Token: 0x060027B9 RID: 10169 RVA: 0x001113B0 File Offset: 0x0010F5B0
		public void UnFreeze()
		{
			if (!this._isForcedFreeze)
			{
				return;
			}
			this.rigidBody.constraints = this.constraints;
			this.rigidBody.inertiaTensorRotation = this._unfreezInertiaTensorRotation;
			this.rigidBody.inertiaTensor = this._unfreezInertiaTensor;
			this._isForcedFreeze = false;
		}

		// Token: 0x060027BA RID: 10170 RVA: 0x00111400 File Offset: 0x0010F600
		public void Teleport(Vector3 position, Quaternion rotation)
		{
			this.rigidBody.transform.SetPositionAndRotation(position, rotation);
		}

		// Token: 0x060027BB RID: 10171 RVA: 0x00111414 File Offset: 0x0010F614
		public void MovePosition(Vector3 position)
		{
			this.rigidBody.MovePosition(position);
		}

		// Token: 0x060027BC RID: 10172 RVA: 0x00111422 File Offset: 0x0010F622
		public void MoveRotation(Quaternion rotation)
		{
			this.rigidBody.MoveRotation(rotation);
		}

		// Token: 0x060027BD RID: 10173 RVA: 0x00111430 File Offset: 0x0010F630
		public Vector3 GetPointVelocity(Vector3 worldPoint)
		{
			return this.rigidBody.GetPointVelocity(worldPoint);
		}

		// Token: 0x060027BE RID: 10174 RVA: 0x0011143E File Offset: 0x0010F63E
		public bool IsSleeping()
		{
			if (this.sleepingLastUpdate < UpdateManager.fixedFrameCount)
			{
				this.sleepingLastUpdate = UpdateManager.fixedFrameCount;
				this._isSleeping = this.rigidBody.IsSleeping();
			}
			return this._isSleeping;
		}

		// Token: 0x060027BF RID: 10175 RVA: 0x00111470 File Offset: 0x0010F670
		public bool HasMeaningfulVelocity()
		{
			if (this.meaningfulVelocityLastUpdate < UpdateManager.fixedFrameCount)
			{
				this.meaningfulVelocityLastUpdate = UpdateManager.fixedFrameCount;
				this._hasMeaningfulVelocity = (Math.Abs(this.velocity.sqrMagnitude) > 0.001f || Math.Abs(this.angularVelocity.sqrMagnitude) > 0.001f);
			}
			return this._hasMeaningfulVelocity;
		}

		// Token: 0x060027C0 RID: 10176 RVA: 0x001114D8 File Offset: 0x0010F6D8
		public Vector3 NaiveFuturePosition(float time)
		{
			return this.NaiveFuturePosition(this.transform.position, time);
		}

		// Token: 0x060027C1 RID: 10177 RVA: 0x001114EC File Offset: 0x0010F6EC
		public Vector3 NaiveFuturePosition(Vector3 position, float time)
		{
			Vector3 timeRotation = this.angularVelocity * time;
			return position.RotateAroundPivot(this.transform.TransformPoint(this.centerOfMass), Quaternion.Euler(timeRotation)) + this.velocity * time;
		}

		// Token: 0x060027C2 RID: 10178 RVA: 0x00111534 File Offset: 0x0010F734
		public void WakeUp()
		{
			this.rigidBody.WakeUp();
			this._isSleeping = false;
		}

		// Token: 0x060027C3 RID: 10179 RVA: 0x00111548 File Offset: 0x0010F748
		public void ResetInertiaTensor()
		{
			this.rigidBody.ResetInertiaTensor();
			Action action = this.onInertiaTensorChanged;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x060027C4 RID: 10180 RVA: 0x00111565 File Offset: 0x0010F765
		public void ResetCenterOfMass()
		{
			this.rigidBody.ResetCenterOfMass();
			Action action = this.onCenterOfMassChanged;
			if (action == null)
			{
				return;
			}
			action();
		}

		// Token: 0x060027C5 RID: 10181 RVA: 0x00111584 File Offset: 0x0010F784
		public void AddRadialForce(float force, Vector3 position, float upwardsModifier, ForceMode mode)
		{
			Vector3 modifiedTarget = this.rigidBody.ClosestPointOnBounds(position) + Vector3.up * upwardsModifier;
			this.AddForceAtPosition((modifiedTarget - position).normalized * force, modifiedTarget, mode);
		}

		// Token: 0x060027C6 RID: 10182 RVA: 0x001115CC File Offset: 0x0010F7CC
		public void AddExplosionForce(float force, Vector3 position, float radius, float upwardsModifier, ForceMode mode)
		{
			this.rigidBody.AddExplosionForce(force, position, radius, upwardsModifier, mode);
		}

		// Token: 0x060027C7 RID: 10183 RVA: 0x001115E0 File Offset: 0x0010F7E0
		public void AddForce(Vector3 force, ForceMode mode)
		{
			this.rigidBody.AddForce(force, mode);
		}

		// Token: 0x060027C8 RID: 10184 RVA: 0x001115EF File Offset: 0x0010F7EF
		public void AddRelativeForce(Vector3 force, ForceMode mode)
		{
			this.rigidBody.AddRelativeForce(force, mode);
		}

		// Token: 0x060027C9 RID: 10185 RVA: 0x001115FE File Offset: 0x0010F7FE
		public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode)
		{
			this.rigidBody.AddForceAtPosition(force, position, mode);
		}

		// Token: 0x060027CA RID: 10186 RVA: 0x0011160E File Offset: 0x0010F80E
		public void AddTorque(Vector3 force, ForceMode mode)
		{
			this.rigidBody.AddTorque(force, mode);
		}

		// Token: 0x060027CB RID: 10187 RVA: 0x0011161D File Offset: 0x0010F81D
		public void AddRelativeTorque(Vector3 force, ForceMode mode)
		{
			this.rigidBody.AddRelativeTorque(force, mode);
		}

		// Token: 0x060027CC RID: 10188 RVA: 0x0011162C File Offset: 0x0010F82C
		public static bool operator ==(PhysicBody lhs, PhysicBody rhs)
		{
			if (lhs == null)
			{
				return rhs == null;
			}
			return lhs.Equals(rhs);
		}

		// Token: 0x060027CD RID: 10189 RVA: 0x0011163D File Offset: 0x0010F83D
		public static bool operator !=(PhysicBody lhs, PhysicBody rhs)
		{
			return !(lhs == rhs);
		}

		// Token: 0x060027CE RID: 10190 RVA: 0x0011164C File Offset: 0x0010F84C
		public override bool Equals(object other)
		{
			PhysicBody pb = other as PhysicBody;
			return pb != null && this.Equals(pb);
		}

		// Token: 0x060027CF RID: 10191 RVA: 0x0011166C File Offset: 0x0010F86C
		protected bool Equals(PhysicBody other)
		{
			return other != null && (object.Equals(this.rigidBody, other.rigidBody) && object.Equals(this.transform, other.transform)) && object.Equals(this.gameObject, other.gameObject);
		}

		// Token: 0x060027D0 RID: 10192 RVA: 0x001116AC File Offset: 0x0010F8AC
		public override int GetHashCode()
		{
			return (((this.rigidBody != null) ? this.rigidBody.GetHashCode() : 0) * 397 ^ ((this.transform != null) ? this.transform.GetHashCode() : 0)) * 397 ^ ((this.gameObject != null) ? this.gameObject.GetHashCode() : 0);
		}

		// Token: 0x060027D1 RID: 10193 RVA: 0x0011171B File Offset: 0x0010F91B
		public static implicit operator bool(PhysicBody pb)
		{
			return pb != null && pb.rigidBody;
		}

		// Token: 0x040026B1 RID: 9905
		public Rigidbody rigidBody;

		// Token: 0x040026B2 RID: 9906
		public Transform transform;

		// Token: 0x040026B3 RID: 9907
		public GameObject gameObject;

		// Token: 0x040026B4 RID: 9908
		private bool _isForcedFreeze;

		// Token: 0x040026B5 RID: 9909
		public RigidbodyConstraints _unfreezeConstraint;

		// Token: 0x040026B6 RID: 9910
		public Vector3 _unfreezInertiaTensor;

		// Token: 0x040026B7 RID: 9911
		public Quaternion _unfreezInertiaTensorRotation;

		// Token: 0x040026B8 RID: 9912
		public Action onMassChanged;

		// Token: 0x040026B9 RID: 9913
		public Action onCenterOfMassChanged;

		// Token: 0x040026BA RID: 9914
		public Action onGravityChanged;

		// Token: 0x040026BB RID: 9915
		public Action onInertiaTensorChanged;

		// Token: 0x040026BC RID: 9916
		private Vector3 _centerOfMass;

		// Token: 0x040026BD RID: 9917
		private bool comInitialSet;

		// Token: 0x040026BE RID: 9918
		private int velocityLastUpdate;

		// Token: 0x040026BF RID: 9919
		private Vector3 _velocity;

		// Token: 0x040026C0 RID: 9920
		private int angularVelocityLastUpdate;

		// Token: 0x040026C1 RID: 9921
		private Vector3 _angularVelocity;

		// Token: 0x040026C2 RID: 9922
		private int isKinematicLastUpdate;

		// Token: 0x040026C3 RID: 9923
		private bool _isKinematic;

		// Token: 0x040026C4 RID: 9924
		private int sleepingLastUpdate;

		// Token: 0x040026C5 RID: 9925
		private bool _isSleeping;

		// Token: 0x040026C6 RID: 9926
		private int meaningfulVelocityLastUpdate;

		// Token: 0x040026C7 RID: 9927
		private bool _hasMeaningfulVelocity;
	}
}
