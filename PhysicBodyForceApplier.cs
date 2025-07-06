using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000351 RID: 849
	internal class PhysicBodyForceApplier : ThunderBehaviour, IToolControllable
	{
		// Token: 0x060027D2 RID: 10194 RVA: 0x0011172D File Offset: 0x0010F92D
		public void ToggleLinearForce()
		{
			this.linearForceActive = !this.linearForceActive;
		}

		// Token: 0x060027D3 RID: 10195 RVA: 0x0011173E File Offset: 0x0010F93E
		public void ToggleAngularForce()
		{
			this.angularForceActive = !this.angularForceActive;
		}

		// Token: 0x060027D4 RID: 10196 RVA: 0x0011174F File Offset: 0x0010F94F
		public void SetLinearForceActive(bool active)
		{
			this.linearForceActive = active;
		}

		// Token: 0x060027D5 RID: 10197 RVA: 0x00111758 File Offset: 0x0010F958
		public void SetAngularForceActive(bool active)
		{
			this.angularForceActive = active;
		}

		// Token: 0x060027D6 RID: 10198 RVA: 0x00111761 File Offset: 0x0010F961
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.TryAssignBody();
		}

		// Token: 0x060027D7 RID: 10199 RVA: 0x00111777 File Offset: 0x0010F977
		private void TryAssignBody()
		{
			if (this.rb == null)
			{
				this.rb = base.GetComponent<Rigidbody>();
			}
			if (this.ab == null)
			{
				this.ab = base.GetComponent<ArticulationBody>();
			}
		}

		// Token: 0x060027D8 RID: 10200 RVA: 0x001117A1 File Offset: 0x0010F9A1
		public void StartAngularForce()
		{
			this.angularForceRemainingDuration = this.angularForceDuration;
		}

		// Token: 0x060027D9 RID: 10201 RVA: 0x001117AF File Offset: 0x0010F9AF
		public void StartLinearForce()
		{
			this.linearForceRemainingDuration = this.linearForceDuration;
		}

		// Token: 0x060027DA RID: 10202 RVA: 0x001117BD File Offset: 0x0010F9BD
		public bool IsCopyable()
		{
			return true;
		}

		// Token: 0x060027DB RID: 10203 RVA: 0x001117C0 File Offset: 0x0010F9C0
		public void CopyTo(UnityEngine.Object other)
		{
			((IToolControllable)this).CopyControllableTo(other);
		}

		// Token: 0x060027DC RID: 10204 RVA: 0x001117CC File Offset: 0x0010F9CC
		public void CopyFrom(IToolControllable original)
		{
			PhysicBodyForceApplier originalApplier = original as PhysicBodyForceApplier;
			this.linearForceActive = originalApplier.linearForceActive;
			this.linearForceDuration = originalApplier.linearForceDuration;
			this.linearForceApplyMode = originalApplier.linearForceApplyMode;
			this.linearForceType = originalApplier.linearForceType;
			this.linearForce = originalApplier.linearForce;
			this.angularForceActive = originalApplier.angularForceActive;
			this.angularForceDuration = originalApplier.angularForceDuration;
			this.localAngularForce = originalApplier.localAngularForce;
			this.angularForceType = originalApplier.angularForceType;
			this.angularForce = originalApplier.angularForce;
		}

		// Token: 0x060027DD RID: 10205 RVA: 0x00111858 File Offset: 0x0010FA58
		public void ReparentAlign(Component other)
		{
			((IToolControllable)this).ReparentAlignTransform(other);
		}

		// Token: 0x060027DE RID: 10206 RVA: 0x00111861 File Offset: 0x0010FA61
		public void Remove()
		{
			UnityEngine.Object.Destroy(this);
		}

		// Token: 0x060027DF RID: 10207 RVA: 0x00111869 File Offset: 0x0010FA69
		public Transform GetTransform()
		{
			return base.transform;
		}

		// Token: 0x060027E0 RID: 10208 RVA: 0x00111871 File Offset: 0x0010FA71
		private void Awake()
		{
			this.TryAssignBody();
			if (this.rb)
			{
				this.physicBody = new PhysicBody(this.rb);
			}
		}

		// Token: 0x17000269 RID: 617
		// (get) Token: 0x060027E1 RID: 10209 RVA: 0x00111897 File Offset: 0x0010FA97
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate;
			}
		}

		// Token: 0x060027E2 RID: 10210 RVA: 0x0011189C File Offset: 0x0010FA9C
		protected internal override void ManagedFixedUpdate()
		{
			if (this.physicBody == null)
			{
				return;
			}
			if (this.linearForceActive)
			{
				this.ApplyLinearForce();
			}
			else if (this.linearForceRemainingDuration >= 0f)
			{
				this.ApplyLinearForce();
				this.linearForceRemainingDuration -= Time.deltaTime;
			}
			if (this.angularForceActive)
			{
				this.ApplyAngularForce();
				return;
			}
			if (this.angularForceRemainingDuration >= 0f)
			{
				this.ApplyAngularForce();
				this.angularForceRemainingDuration -= Time.deltaTime;
			}
		}

		// Token: 0x060027E3 RID: 10211 RVA: 0x00111924 File Offset: 0x0010FB24
		protected void ApplyLinearForce()
		{
			switch (this.linearForceApplyMode)
			{
			case PhysicBodyForceApplier.ForceApplyMode.WorldSpace:
				this.physicBody.AddForce(this.linearForce, this.linearForceType);
				return;
			case PhysicBodyForceApplier.ForceApplyMode.LocalSpace:
			{
				Vector3 localForce = this.MakeLocalForce(this.linearForce, this.physicBody.transform);
				this.physicBody.AddForce(localForce, this.linearForceType);
				return;
			}
			case PhysicBodyForceApplier.ForceApplyMode.OffsetSpace:
			{
				Vector3 applierForce = this.MakeLocalForce(this.linearForce, base.transform);
				this.physicBody.AddForceAtPosition(applierForce, base.transform.position, this.linearForceType);
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x060027E4 RID: 10212 RVA: 0x001119C0 File Offset: 0x0010FBC0
		protected void ApplyAngularForce()
		{
			Vector3 torqueForce = this.localAngularForce ? this.MakeLocalForce(this.angularForce, this.physicBody.transform) : this.angularForce;
			this.physicBody.AddTorque(torqueForce, this.angularForceType);
		}

		// Token: 0x060027E5 RID: 10213 RVA: 0x00111A07 File Offset: 0x0010FC07
		private Vector3 MakeLocalForce(Vector3 inputForce, Transform reference)
		{
			return reference.right * inputForce.x + reference.up * inputForce.y + reference.forward * inputForce.z;
		}

		// Token: 0x040026C8 RID: 9928
		public Rigidbody rb;

		// Token: 0x040026C9 RID: 9929
		public ArticulationBody ab;

		// Token: 0x040026CA RID: 9930
		protected PhysicBody physicBody;

		// Token: 0x040026CB RID: 9931
		[Header("Linear force")]
		public bool linearForceActive;

		// Token: 0x040026CC RID: 9932
		public float linearForceDuration;

		// Token: 0x040026CD RID: 9933
		protected float linearForceRemainingDuration = -1f;

		// Token: 0x040026CE RID: 9934
		public PhysicBodyForceApplier.ForceApplyMode linearForceApplyMode;

		// Token: 0x040026CF RID: 9935
		public ForceMode linearForceType;

		// Token: 0x040026D0 RID: 9936
		public Vector3 linearForce;

		// Token: 0x040026D1 RID: 9937
		[Header("Angular force")]
		public bool angularForceActive;

		// Token: 0x040026D2 RID: 9938
		public float angularForceDuration;

		// Token: 0x040026D3 RID: 9939
		protected float angularForceRemainingDuration = -1f;

		// Token: 0x040026D4 RID: 9940
		public bool localAngularForce;

		// Token: 0x040026D5 RID: 9941
		public ForceMode angularForceType;

		// Token: 0x040026D6 RID: 9942
		public Vector3 angularForce;

		// Token: 0x02000A41 RID: 2625
		public enum ForceApplyMode
		{
			// Token: 0x04004788 RID: 18312
			WorldSpace,
			// Token: 0x04004789 RID: 18313
			LocalSpace,
			// Token: 0x0400478A RID: 18314
			OffsetSpace
		}
	}
}
