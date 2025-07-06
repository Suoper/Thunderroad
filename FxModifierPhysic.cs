using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000296 RID: 662
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModifierPhysic.html")]
	public class FxModifierPhysic : ThunderBehaviour
	{
		// Token: 0x170001EC RID: 492
		// (get) Token: 0x06001F20 RID: 7968 RVA: 0x000D43AE File Offset: 0x000D25AE
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001F21 RID: 7969 RVA: 0x000D43B1 File Offset: 0x000D25B1
		private void OnValidate()
		{
			if (this.fxController == null)
			{
				this.fxController = base.GetComponent<FxController>();
			}
		}

		// Token: 0x06001F22 RID: 7970 RVA: 0x000D43CD File Offset: 0x000D25CD
		private void Awake()
		{
			if (this.fxController == null)
			{
				Debug.LogError("FxModifierPhysic have no fxController set!");
				base.enabled = false;
			}
			if (this.rb == null)
			{
				Debug.LogError("FxModifierPhysic have no rigidbody set!");
				base.enabled = false;
			}
		}

		// Token: 0x06001F23 RID: 7971 RVA: 0x000D4410 File Offset: 0x000D2610
		protected internal override void ManagedUpdate()
		{
			if (this.velocityLink != FxModifierPhysic.Link.None)
			{
				if (!this.rb.isKinematic && !this.rb.IsSleeping())
				{
					Vector3 pointVelocity = this.rb.GetPointVelocity(this.velocityPointTransform.position);
					this.dampenedVelocity = Mathf.Lerp(this.dampenedVelocity, Mathf.InverseLerp(this.velocityRange.x, this.velocityRange.y, pointVelocity.magnitude), this.velocityDampening);
				}
				else if (this.dampenedVelocity > 0f)
				{
					this.dampenedVelocity = Mathf.Lerp(this.dampenedVelocity, 0f, this.velocityDampening);
				}
				if (this.velocityLink == FxModifierPhysic.Link.Intensity && this.fxController.intensity != this.dampenedVelocity)
				{
					this.fxController.SetIntensity(this.dampenedVelocity);
				}
				if (this.velocityLink == FxModifierPhysic.Link.Speed && this.fxController.speed != this.dampenedVelocity)
				{
					this.fxController.SetSpeed(this.dampenedVelocity);
				}
			}
			if (this.torqueLink != FxModifierPhysic.Link.None)
			{
				if (!this.rb.isKinematic && !this.rb.IsSleeping())
				{
					this.dampenedTorque = Mathf.Lerp(this.dampenedTorque, Mathf.InverseLerp(this.torqueRange.x, this.torqueRange.y, this.rb.angularVelocity.magnitude), this.torqueDampening);
				}
				else if (this.dampenedTorque > 0f)
				{
					this.dampenedTorque = Mathf.Lerp(this.dampenedTorque, 0f, this.torqueDampening);
				}
				if (this.torqueLink == FxModifierPhysic.Link.Intensity && this.fxController.intensity != this.dampenedTorque)
				{
					this.fxController.SetIntensity(this.dampenedTorque);
				}
				if (this.torqueLink == FxModifierPhysic.Link.Speed && this.fxController.speed != this.dampenedTorque)
				{
					this.fxController.SetSpeed(this.dampenedTorque);
				}
			}
		}

		// Token: 0x04001E22 RID: 7714
		[Header("References")]
		public FxController fxController;

		// Token: 0x04001E23 RID: 7715
		public Rigidbody rb;

		// Token: 0x04001E24 RID: 7716
		[Header("Velocity")]
		public FxModifierPhysic.Link velocityLink;

		// Token: 0x04001E25 RID: 7717
		public Transform velocityPointTransform;

		// Token: 0x04001E26 RID: 7718
		public Vector2 velocityRange = new Vector2(0f, 5f);

		// Token: 0x04001E27 RID: 7719
		public float velocityDampening = 1f;

		// Token: 0x04001E28 RID: 7720
		[Header("Torque")]
		public FxModifierPhysic.Link torqueLink;

		// Token: 0x04001E29 RID: 7721
		public Vector2 torqueRange = new Vector2(2f, 8f);

		// Token: 0x04001E2A RID: 7722
		public float torqueDampening = 1f;

		// Token: 0x04001E2B RID: 7723
		protected float dampenedVelocity;

		// Token: 0x04001E2C RID: 7724
		protected float dampenedTorque;

		// Token: 0x02000935 RID: 2357
		public enum Link
		{
			// Token: 0x04004405 RID: 17413
			None,
			// Token: 0x04004406 RID: 17414
			Intensity,
			// Token: 0x04004407 RID: 17415
			Speed
		}
	}
}
