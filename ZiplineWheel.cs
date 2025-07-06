using System;
using System.Collections.Generic;
using System.Media;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B7 RID: 695
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/ZiplineWheel")]
	public class ZiplineWheel : MonoBehaviour
	{
		// Token: 0x060021BB RID: 8635 RVA: 0x000E8894 File Offset: 0x000E6A94
		protected void Awake()
		{
			this.orgConnectedMassScale = this.wheelHingeJoint.connectedMassScale;
			this.orgWheelMass = this.wheelRigidbody.mass;
			this.wheelCenterPhysicMaterial = new ZiplineWheel.PhysicMaterialLink(this.wheelCenterCollider.material);
			this.wheelRailLeftPhysicMaterial = new ZiplineWheel.PhysicMaterialLink(this.wheelRailLeftCollider.material);
			this.wheelRailRightPhysicMaterial = new ZiplineWheel.PhysicMaterialLink(this.wheelRailRightCollider.material);
			this.jointMotor = this.wheelHingeJoint.motor;
			this.item = base.GetComponentInParent<Item>();
			this.item.OnUngrabEvent += this.OnRelease;
			this.item.OnGrabEvent += this.OnGrab;
		}

		// Token: 0x060021BC RID: 8636 RVA: 0x000E8950 File Offset: 0x000E6B50
		protected void OnGrab(Handle handle, RagdollHand ragdollHand)
		{
			if (this.motorTriggerHandle == handle)
			{
				this.motorRagdollHand = ragdollHand;
			}
		}

		// Token: 0x060021BD RID: 8637 RVA: 0x000E8967 File Offset: 0x000E6B67
		protected void OnRelease(Handle handle, RagdollHand ragdollHand, bool throwing)
		{
			if (this.motorTriggerHandle == handle)
			{
				this.motorRagdollHand = null;
			}
		}

		// Token: 0x060021BE RID: 8638 RVA: 0x000E8980 File Offset: 0x000E6B80
		private void Update()
		{
			if (!this.allowMotor || !this.motorRagdollHand)
			{
				if (this.motorActive)
				{
					this.OnMotorStop();
				}
				return;
			}
			PlayerControl.Hand playerControlHand = PlayerControl.GetHand(this.motorRagdollHand.side);
			if (playerControlHand.useAxis > 0f)
			{
				if (!this.motorActive)
				{
					this.OnMotorStart();
				}
				this.OnMotorUpdate(playerControlHand.useAxis);
				return;
			}
			this.OnMotorStop();
		}

		// Token: 0x060021BF RID: 8639 RVA: 0x000E89F0 File Offset: 0x000E6BF0
		private void OnMotorStart()
		{
			this.wheelHingeJoint.useMotor = true;
			this.wheelCenterPhysicMaterial.Set(this.wheelCenterMotorFriction, PhysicMaterialCombine.Maximum);
			if (this.motorFxController)
			{
				this.motorFxController.Play();
			}
			this.motorActive = true;
		}

		// Token: 0x060021C0 RID: 8640 RVA: 0x000E8A2F File Offset: 0x000E6C2F
		private void OnMotorUpdate(float intensity)
		{
			this.jointMotor.targetVelocity = Mathf.Lerp(0f, this.motorMaxVelocity, intensity);
			this.wheelHingeJoint.motor = this.jointMotor;
			this.motorFxController.SetIntensity(intensity);
		}

		// Token: 0x060021C1 RID: 8641 RVA: 0x000E8A6C File Offset: 0x000E6C6C
		private void OnMotorStop()
		{
			this.wheelHingeJoint.useMotor = false;
			if (this.onZipline)
			{
				this.wheelCenterPhysicMaterial.Set(this.wheelCenterOnZiplineFriction, PhysicMaterialCombine.Minimum);
			}
			else
			{
				this.wheelCenterPhysicMaterial.Reset();
			}
			if (this.motorFxController)
			{
				this.motorFxController.Stop();
			}
			this.motorActive = false;
		}

		// Token: 0x060021C2 RID: 8642 RVA: 0x000E8ACB File Offset: 0x000E6CCB
		private void OnTriggerEnter(Collider other)
		{
			if (!this.onZipline && other.gameObject.CompareTag(this.ropeTag))
			{
				this.OnZiplineEnter(other);
			}
		}

		// Token: 0x060021C3 RID: 8643 RVA: 0x000E8AEF File Offset: 0x000E6CEF
		private void OnTriggerExit(Collider other)
		{
			if (this.onZipline && other == this.ziplineCollider)
			{
				this.OnZiplineExit();
			}
		}

		// Token: 0x060021C4 RID: 8644 RVA: 0x000E8B10 File Offset: 0x000E6D10
		protected void OnZiplineEnter(Collider ropeCollider)
		{
			if (this.debugEnterExitZiplineSound)
			{
				SystemSounds.Beep.Play();
			}
			this.ziplineCollider = ropeCollider;
			if (!this.motorActive)
			{
				this.wheelCenterPhysicMaterial.Set(this.wheelCenterOnZiplineFriction, PhysicMaterialCombine.Minimum);
			}
			this.wheelRailLeftPhysicMaterial.Set(this.wheelRailOnZiplineFriction, PhysicMaterialCombine.Minimum);
			this.wheelRailRightPhysicMaterial.Set(this.wheelRailOnZiplineFriction, PhysicMaterialCombine.Minimum);
			if (this.hingeConnectedMassScaleMultiplier != 1f)
			{
				this.wheelHingeJoint.connectedMassScale = this.orgConnectedMassScale * this.hingeConnectedMassScaleMultiplier;
			}
			if (this.wheelMassMultiplier != 1f)
			{
				this.wheelRigidbody.mass = this.orgWheelMass * this.wheelMassMultiplier;
			}
			if (this.forcePhysicJoint)
			{
				foreach (Handle handle in this.handles)
				{
					handle.SetForcePlayerJointModifier(this, true);
				}
			}
			this.onZipline = true;
		}

		// Token: 0x060021C5 RID: 8645 RVA: 0x000E8C14 File Offset: 0x000E6E14
		protected void OnZiplineExit()
		{
			if (this.debugEnterExitZiplineSound)
			{
				SystemSounds.Asterisk.Play();
			}
			this.ziplineCollider = null;
			this.wheelCenterPhysicMaterial.Reset();
			this.wheelRailLeftPhysicMaterial.Reset();
			this.wheelRailRightPhysicMaterial.Reset();
			if (this.hingeConnectedMassScaleMultiplier != 1f)
			{
				this.wheelHingeJoint.connectedMassScale = this.orgConnectedMassScale;
			}
			if (this.wheelMassMultiplier != 1f)
			{
				this.wheelRigidbody.mass = this.orgWheelMass;
			}
			if (this.forcePhysicJoint)
			{
				foreach (Handle handle in this.handles)
				{
					handle.SetForcePlayerJointModifier(this, false);
				}
			}
			this.onZipline = false;
		}

		// Token: 0x04002092 RID: 8338
		[Header("References")]
		public List<Handle> handles;

		// Token: 0x04002093 RID: 8339
		public HingeJoint wheelHingeJoint;

		// Token: 0x04002094 RID: 8340
		public Rigidbody wheelRigidbody;

		// Token: 0x04002095 RID: 8341
		[Header("Motor")]
		public bool allowMotor;

		// Token: 0x04002096 RID: 8342
		public Handle motorTriggerHandle;

		// Token: 0x04002097 RID: 8343
		public float motorMaxVelocity = 5000f;

		// Token: 0x04002098 RID: 8344
		public FxController motorFxController;

		// Token: 0x04002099 RID: 8345
		[Header("Friction")]
		public Collider wheelCenterCollider;

		// Token: 0x0400209A RID: 8346
		public Collider wheelRailLeftCollider;

		// Token: 0x0400209B RID: 8347
		public Collider wheelRailRightCollider;

		// Token: 0x0400209C RID: 8348
		public float wheelRailOnZiplineFriction;

		// Token: 0x0400209D RID: 8349
		public float wheelCenterOnZiplineFriction = 0.2f;

		// Token: 0x0400209E RID: 8350
		public float wheelCenterMotorFriction = 1f;

		// Token: 0x0400209F RID: 8351
		[Header("Rope detection")]
		public string ropeTag = "Zipline";

		// Token: 0x040020A0 RID: 8352
		[Header("On zipline")]
		public float hingeConnectedMassScaleMultiplier = 1f;

		// Token: 0x040020A1 RID: 8353
		public float wheelMassMultiplier = 1f;

		// Token: 0x040020A2 RID: 8354
		public bool forcePhysicJoint = true;

		// Token: 0x040020A3 RID: 8355
		public bool debugEnterExitZiplineSound;

		// Token: 0x040020A4 RID: 8356
		protected Collider ziplineCollider;

		// Token: 0x040020A5 RID: 8357
		protected Item item;

		// Token: 0x040020A6 RID: 8358
		[NonSerialized]
		public bool onZipline;

		// Token: 0x040020A7 RID: 8359
		protected float orgConnectedMassScale;

		// Token: 0x040020A8 RID: 8360
		protected float orgWheelMass;

		// Token: 0x040020A9 RID: 8361
		[NonSerialized]
		public bool motorActive;

		// Token: 0x040020AA RID: 8362
		protected JointMotor jointMotor;

		// Token: 0x040020AB RID: 8363
		protected RagdollHand motorRagdollHand;

		// Token: 0x040020AC RID: 8364
		protected ZiplineWheel.PhysicMaterialLink wheelCenterPhysicMaterial;

		// Token: 0x040020AD RID: 8365
		protected ZiplineWheel.PhysicMaterialLink wheelRailLeftPhysicMaterial;

		// Token: 0x040020AE RID: 8366
		protected ZiplineWheel.PhysicMaterialLink wheelRailRightPhysicMaterial;

		// Token: 0x02000983 RID: 2435
		public class PhysicMaterialLink
		{
			// Token: 0x060043D3 RID: 17363 RVA: 0x0018FD98 File Offset: 0x0018DF98
			public PhysicMaterialLink(PhysicMaterial physicMaterial)
			{
				this.physicMaterial = physicMaterial;
				this.orgDynamicFriction = physicMaterial.dynamicFriction;
				this.orgStaticFriction = physicMaterial.staticFriction;
				this.orgFrictionCombine = physicMaterial.frictionCombine;
			}

			// Token: 0x060043D4 RID: 17364 RVA: 0x0018FDCB File Offset: 0x0018DFCB
			public void Set(float friction, PhysicMaterialCombine frictionCombine)
			{
				this.physicMaterial.dynamicFriction = friction;
				this.physicMaterial.staticFriction = friction;
				this.physicMaterial.frictionCombine = frictionCombine;
			}

			// Token: 0x060043D5 RID: 17365 RVA: 0x0018FDF1 File Offset: 0x0018DFF1
			public void Reset()
			{
				this.physicMaterial.dynamicFriction = this.orgDynamicFriction;
				this.physicMaterial.staticFriction = this.orgStaticFriction;
				this.physicMaterial.frictionCombine = this.orgFrictionCombine;
			}

			// Token: 0x040044CE RID: 17614
			public PhysicMaterial physicMaterial;

			// Token: 0x040044CF RID: 17615
			public float orgDynamicFriction;

			// Token: 0x040044D0 RID: 17616
			public float orgStaticFriction;

			// Token: 0x040044D1 RID: 17617
			public PhysicMaterialCombine orgFrictionCombine;
		}
	}
}
