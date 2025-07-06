using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000271 RID: 625
	public class IkController : ThunderBehaviour
	{
		// Token: 0x140000C2 RID: 194
		// (add) Token: 0x06001BF2 RID: 7154 RVA: 0x000BACC4 File Offset: 0x000B8EC4
		// (remove) Token: 0x06001BF3 RID: 7155 RVA: 0x000BACFC File Offset: 0x000B8EFC
		public event IkController.LateUpdateDelegate OnPreIKUpdateEvent;

		// Token: 0x140000C3 RID: 195
		// (add) Token: 0x06001BF4 RID: 7156 RVA: 0x000BAD34 File Offset: 0x000B8F34
		// (remove) Token: 0x06001BF5 RID: 7157 RVA: 0x000BAD6C File Offset: 0x000B8F6C
		public event IkController.LateUpdateDelegate OnPostIKUpdateEvent;

		// Token: 0x170001C1 RID: 449
		// (get) Token: 0x06001BF6 RID: 7158 RVA: 0x000BADA1 File Offset: 0x000B8FA1
		// (set) Token: 0x06001BF7 RID: 7159 RVA: 0x000BADA9 File Offset: 0x000B8FA9
		public bool initialized { get; protected set; }

		// Token: 0x06001BF8 RID: 7160 RVA: 0x000BADB2 File Offset: 0x000B8FB2
		protected virtual void Awake()
		{
			this.creature = base.GetComponentInParent<Creature>();
		}

		// Token: 0x06001BF9 RID: 7161 RVA: 0x000BADC0 File Offset: 0x000B8FC0
		public virtual void PreIKUpdate()
		{
			if (this.OnPreIKUpdateEvent != null)
			{
				this.OnPreIKUpdateEvent();
			}
		}

		// Token: 0x06001BFA RID: 7162 RVA: 0x000BADD5 File Offset: 0x000B8FD5
		public virtual void PostIKUpdate()
		{
			if (this.OnPostIKUpdateEvent != null)
			{
				this.OnPostIKUpdateEvent();
			}
		}

		// Token: 0x06001BFB RID: 7163 RVA: 0x000BADEA File Offset: 0x000B8FEA
		private void OnAnimatorMove()
		{
			this.creature.AnimatorMoveUpdate();
		}

		// Token: 0x170001C2 RID: 450
		// (get) Token: 0x06001BFC RID: 7164 RVA: 0x000BADF7 File Offset: 0x000B8FF7
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001BFD RID: 7165 RVA: 0x000BADFC File Offset: 0x000B8FFC
		protected internal override void ManagedUpdate()
		{
			if (!this.initialized || !this.creature.initialized)
			{
				return;
			}
			if (this.creature.player)
			{
				this.creature.SetAnimatorHeightRatio(this.creature.transform.InverseTransformPoint(this.creature.player.head.anchor.position).y / this.creature.morphology.headHeight);
			}
			if (this.turnBodyByHeadAndHands)
			{
				try
				{
					this.UpdateBodyRotation();
				}
				catch (Exception message)
				{
					Debug.LogError(message);
				}
			}
		}

		// Token: 0x06001BFE RID: 7166 RVA: 0x000BAEA4 File Offset: 0x000B90A4
		public virtual void UpdateBodyRotation()
		{
			Vector3 headHorizontalDirection = this.creature.centerEyes.forward.ToXZ();
			bool isRunning = this.creature.currentLocomotion && this.creature.currentLocomotion.velocity.magnitude > this.creature.handToBodyRotationMaxVelocity && Vector3.Angle(this.creature.currentLocomotion.velocity.normalized, headHorizontalDirection) < this.creature.handToBodyRotationMaxAngle;
			Vector3 handRightDirXZ = new Vector3(this.creature.ragdoll.ik.handRightTarget.position.x, this.creature.transform.position.y, this.creature.ragdoll.ik.handRightTarget.position.z) - this.creature.transform.position;
			Vector3 handLeftDirXZ = new Vector3(this.creature.ragdoll.ik.handLeftTarget.position.x, this.creature.transform.position.y, this.creature.ragdoll.ik.handLeftTarget.position.z) - this.creature.transform.position;
			float headAngle = Vector3.Angle(this.creature.transform.forward, headHorizontalDirection);
			float headAngleRatio = isRunning ? 1f : Utils.CalculateRatio(headAngle, this.creature.headMinAngle, this.creature.headMaxAngle, 0f, 1f);
			float handRightAngleNeeded = Vector3.SignedAngle(this.creature.transform.right, handRightDirXZ, this.creature.transform.up) * (isRunning ? 0f : this.creature.handRight.GetArmLenghtRatio(true)) * (1f - headAngleRatio);
			float handLeftAngleNeeded = Vector3.SignedAngle(-this.creature.transform.right, handLeftDirXZ, this.creature.transform.up) * (isRunning ? 0f : this.creature.handLeft.GetArmLenghtRatio(true)) * (1f - headAngleRatio);
			float headAngleNeeded = Vector3.SignedAngle(this.creature.transform.forward, headHorizontalDirection, this.creature.transform.up) * headAngleRatio;
			float turnAngle = this.creature.turnRelativeToHand ? (handRightAngleNeeded + handLeftAngleNeeded + headAngleNeeded) : headAngleNeeded;
			this.creature.transform.localEulerAngles = new Vector3(0f, this.creature.transform.localEulerAngles.y + turnAngle * this.creature.turnSpeed * Time.deltaTime, 0f);
		}

		// Token: 0x06001BFF RID: 7167 RVA: 0x000BB17E File Offset: 0x000B937E
		public virtual void Setup()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C00 RID: 7168 RVA: 0x000BB185 File Offset: 0x000B9385
		public virtual float GetLocomotionWeight()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C01 RID: 7169 RVA: 0x000BB18C File Offset: 0x000B938C
		public virtual void SetLocomotionWeight(float weight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C02 RID: 7170 RVA: 0x000BB193 File Offset: 0x000B9393
		public virtual void AddLocomotionDeltaPosition(Vector3 delta)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C03 RID: 7171 RVA: 0x000BB19A File Offset: 0x000B939A
		public virtual void AddLocomotionDeltaRotation(Quaternion delta, Vector3 pivot)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C04 RID: 7172 RVA: 0x000BB1A1 File Offset: 0x000B93A1
		public virtual void SetFullbody(bool active)
		{
			this.fullbody = active;
		}

		// Token: 0x06001C05 RID: 7173 RVA: 0x000BB1AA File Offset: 0x000B93AA
		public virtual float GetLookAtWeight()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C06 RID: 7174 RVA: 0x000BB1B1 File Offset: 0x000B93B1
		public virtual void SetLookAtTarget(Transform anchor)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C07 RID: 7175 RVA: 0x000BB1B8 File Offset: 0x000B93B8
		public virtual void SetLookAtWeight(float weight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C08 RID: 7176 RVA: 0x000BB1BF File Offset: 0x000B93BF
		public virtual void SetLookAtBodyWeight(float weight, float clamp)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C09 RID: 7177 RVA: 0x000BB1C6 File Offset: 0x000B93C6
		public virtual void SetLookAtHeadWeight(float weight, float clamp)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C0A RID: 7178 RVA: 0x000BB1CD File Offset: 0x000B93CD
		public virtual void SetLookAtEyesWeight(float weight, float clamp)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C0B RID: 7179 RVA: 0x000BB1D4 File Offset: 0x000B93D4
		public virtual void SetHeadAnchor(Transform anchor)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C0C RID: 7180 RVA: 0x000BB1DB File Offset: 0x000B93DB
		public virtual void SetHeadState(bool positionEnabled, bool rotationEnabled)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C0D RID: 7181 RVA: 0x000BB1E2 File Offset: 0x000B93E2
		public virtual void SetHeadWeight(float positionWeight, float rotationWeight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C0E RID: 7182 RVA: 0x000BB1E9 File Offset: 0x000B93E9
		public virtual float GetHeadWeight()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C0F RID: 7183 RVA: 0x000BB1F0 File Offset: 0x000B93F0
		public virtual void SetHipsAnchor(Transform anchor)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C10 RID: 7184 RVA: 0x000BB1F7 File Offset: 0x000B93F7
		public virtual void SetHipsState(bool active)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C11 RID: 7185 RVA: 0x000BB1FE File Offset: 0x000B93FE
		public virtual void SetHipsWeight(float active)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C12 RID: 7186 RVA: 0x000BB205 File Offset: 0x000B9405
		public virtual float GetHipsWeight()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C13 RID: 7187 RVA: 0x000BB20C File Offset: 0x000B940C
		public virtual void SetShoulderAnchor(Side side, Transform anchor)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C14 RID: 7188 RVA: 0x000BB213 File Offset: 0x000B9413
		public virtual void SetShoulderState(Side side, bool positionEnabled, bool rotationEnabled)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C15 RID: 7189 RVA: 0x000BB21A File Offset: 0x000B941A
		public virtual void SetShoulderWeight(Side side, float positionWeight, float rotationWeight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C16 RID: 7190 RVA: 0x000BB221 File Offset: 0x000B9421
		public virtual void SetHandAnchor(Side side, Transform anchor)
		{
			this.SetHandAnchor(side, anchor, Quaternion.identity);
		}

		// Token: 0x06001C17 RID: 7191 RVA: 0x000BB230 File Offset: 0x000B9430
		public virtual void SetHandAnchor(Side side, Transform anchor, Quaternion palmRotation)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C18 RID: 7192 RVA: 0x000BB237 File Offset: 0x000B9437
		public virtual void SetHandState(Side side, bool positionEnabled, bool rotationEnabled)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C19 RID: 7193 RVA: 0x000BB23E File Offset: 0x000B943E
		public virtual void SetHandWeight(Side side, float positionWeight, float rotationWeight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C1A RID: 7194 RVA: 0x000BB245 File Offset: 0x000B9445
		public virtual float GetHandPositionWeight(Side side)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C1B RID: 7195 RVA: 0x000BB24C File Offset: 0x000B944C
		public virtual float GetHandRotationWeight(Side side)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C1C RID: 7196 RVA: 0x000BB253 File Offset: 0x000B9453
		public virtual void SetFootAnchor(Side side, Transform anchor)
		{
			this.SetFootAnchor(side, anchor, Quaternion.identity);
		}

		// Token: 0x06001C1D RID: 7197 RVA: 0x000BB262 File Offset: 0x000B9462
		public virtual void SetFootAnchor(Side side, Transform anchor, Quaternion toesRotation)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C1E RID: 7198 RVA: 0x000BB269 File Offset: 0x000B9469
		public virtual void SetFootState(Side side, bool active)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C1F RID: 7199 RVA: 0x000BB270 File Offset: 0x000B9470
		public virtual void SetFootWeight(Side side, float positionWeight, float rotationWeight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C20 RID: 7200 RVA: 0x000BB277 File Offset: 0x000B9477
		public virtual void SetFootPull(Side side, float value)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C21 RID: 7201 RVA: 0x000BB27E File Offset: 0x000B947E
		public virtual IkController.FootBoneTarget GetFootBoneTarget()
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C22 RID: 7202 RVA: 0x000BB285 File Offset: 0x000B9485
		public virtual void SetKneeAnchor(Side side, Transform anchor)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C23 RID: 7203 RVA: 0x000BB28C File Offset: 0x000B948C
		public virtual void SetKneeState(Side side, bool active)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06001C24 RID: 7204 RVA: 0x000BB293 File Offset: 0x000B9493
		public virtual void SetKneeWeight(Side side, float weight)
		{
			throw new NotImplementedException();
		}

		// Token: 0x04001ACE RID: 6862
		protected Creature creature;

		// Token: 0x04001ACF RID: 6863
		public bool fullbody;

		// Token: 0x04001AD0 RID: 6864
		public bool turnBodyByHeadAndHands;

		// Token: 0x04001AD1 RID: 6865
		[Header("Head")]
		public bool headEnabled;

		// Token: 0x04001AD2 RID: 6866
		public Transform headTarget;

		// Token: 0x04001AD3 RID: 6867
		public Transform eyesTarget;

		// Token: 0x04001AD4 RID: 6868
		[Header("Hips")]
		public bool hipsEnabled;

		// Token: 0x04001AD5 RID: 6869
		public Transform hipsTarget;

		// Token: 0x04001AD6 RID: 6870
		[Header("Hand Left")]
		public bool handLeftEnabled;

		// Token: 0x04001AD7 RID: 6871
		public Transform handLeftTarget;

		// Token: 0x04001AD8 RID: 6872
		public Transform fingerLeftThumbTarget;

		// Token: 0x04001AD9 RID: 6873
		public Transform fingerLeftIndexTarget;

		// Token: 0x04001ADA RID: 6874
		public Transform fingerLeftMiddleTarget;

		// Token: 0x04001ADB RID: 6875
		public Transform fingerLeftRingTarget;

		// Token: 0x04001ADC RID: 6876
		public Transform fingerLeftLittleTarget;

		// Token: 0x04001ADD RID: 6877
		[Header("Hand Right")]
		public bool handRightEnabled;

		// Token: 0x04001ADE RID: 6878
		public Transform handRightTarget;

		// Token: 0x04001ADF RID: 6879
		public Transform fingerRightThumbTarget;

		// Token: 0x04001AE0 RID: 6880
		public Transform fingerRightIndexTarget;

		// Token: 0x04001AE1 RID: 6881
		public Transform fingerRightMiddleTarget;

		// Token: 0x04001AE2 RID: 6882
		public Transform fingerRightRingTarget;

		// Token: 0x04001AE3 RID: 6883
		public Transform fingerRightLittleTarget;

		// Token: 0x04001AE4 RID: 6884
		[Header("Foot Left")]
		public bool footLeftEnabled;

		// Token: 0x04001AE5 RID: 6885
		public Transform footLeftTarget;

		// Token: 0x04001AE6 RID: 6886
		public Transform kneeLeftHint;

		// Token: 0x04001AE7 RID: 6887
		public bool kneeLeftEnabled;

		// Token: 0x04001AE8 RID: 6888
		[Header("Foot Right")]
		public bool footRightEnabled;

		// Token: 0x04001AE9 RID: 6889
		public Transform footRightTarget;

		// Token: 0x04001AEA RID: 6890
		public Transform kneeRightHint;

		// Token: 0x04001AEB RID: 6891
		public bool kneeRightEnabled;

		// Token: 0x04001AEC RID: 6892
		[Header("Shoulder Left")]
		public bool shoulderLeftEnabled;

		// Token: 0x04001AED RID: 6893
		public Transform shoulderLeftTarget;

		// Token: 0x04001AEE RID: 6894
		[Header("Shoulder Right")]
		public bool shoulderRightEnabled;

		// Token: 0x04001AEF RID: 6895
		public Transform shoulderRightTarget;

		// Token: 0x020008DD RID: 2269
		// (Invoke) Token: 0x060041BE RID: 16830
		public delegate void LateUpdateDelegate();

		// Token: 0x020008DE RID: 2270
		public enum FootBoneTarget
		{
			// Token: 0x040042F7 RID: 17143
			Ankle,
			// Token: 0x040042F8 RID: 17144
			Toes
		}
	}
}
