using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200027B RID: 635
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RagdollHandPoser")]
	[AddComponentMenu("ThunderRoad/Creatures/Ragdoll hand poser")]
	[RequireComponent(typeof(RagdollHand))]
	public class RagdollHandPoser : ThunderBehaviour
	{
		// Token: 0x06001DA5 RID: 7589 RVA: 0x000C99D0 File Offset: 0x000C7BD0
		public void UpdatePoseThumb(float targetWeight)
		{
			this.thumbCloseWeight = targetWeight;
			if (this.hasTargetHandPose)
			{
				this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb, this.targetHandPoseFingers.thumb, targetWeight);
				return;
			}
			this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb);
		}

		// Token: 0x06001DA6 RID: 7590 RVA: 0x000C9A34 File Offset: 0x000C7C34
		public void UpdatePoseIndex(float targetWeight)
		{
			this.indexCloseWeight = targetWeight;
			if (this.hasTargetHandPose)
			{
				this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index, this.targetHandPoseFingers.index, targetWeight);
				return;
			}
			this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index);
		}

		// Token: 0x06001DA7 RID: 7591 RVA: 0x000C9A98 File Offset: 0x000C7C98
		public void UpdatePoseMiddle(float targetWeight)
		{
			this.middleCloseWeight = targetWeight;
			if (this.hasTargetHandPose)
			{
				this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle, this.targetHandPoseFingers.middle, targetWeight);
				return;
			}
			this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle);
		}

		// Token: 0x06001DA8 RID: 7592 RVA: 0x000C9AFC File Offset: 0x000C7CFC
		public void UpdatePoseRing(float targetWeight)
		{
			this.ringCloseWeight = targetWeight;
			if (this.hasTargetHandPose)
			{
				this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring, this.targetHandPoseFingers.ring, targetWeight);
				return;
			}
			this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring);
		}

		// Token: 0x06001DA9 RID: 7593 RVA: 0x000C9B60 File Offset: 0x000C7D60
		public void UpdatePoseLittle(float targetWeight)
		{
			this.littleCloseWeight = targetWeight;
			if (this.hasTargetHandPose)
			{
				this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little, this.targetHandPoseFingers.little, targetWeight);
				return;
			}
			this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little);
		}

		// Token: 0x06001DAA RID: 7594 RVA: 0x000C9BC1 File Offset: 0x000C7DC1
		public void UpdatePose(HandPoseData.FingerType finger, float weight)
		{
			this.UpdateFinger(this.ragdollHand.GetFinger(finger), this.defaultHandPoseFingers.GetFinger(finger));
		}

		// Token: 0x06001DAB RID: 7595 RVA: 0x000C9BE4 File Offset: 0x000C7DE4
		public virtual void UpdateFinger(RagdollHand.Finger finger, HandPoseData.Pose.Finger defaultHandPoseFingers, HandPoseData.Pose.Finger targetHandPoseFingers, float targetWeight)
		{
			HandPoseData.Pose.Finger.Bone proximal = defaultHandPoseFingers.proximal;
			Vector3 proximalPos = Vector3.Lerp(proximal.localPosition, targetHandPoseFingers.proximal.localPosition, targetWeight);
			Quaternion proximalRot = Quaternion.Lerp(proximal.localRotation, targetHandPoseFingers.proximal.localRotation, targetWeight);
			finger.proximal.collider.transform.SetPositionAndRotation(this.ragdollHand.transform.TransformPoint(proximalPos), this.ragdollHand.transform.TransformRotation(proximalRot));
			Vector3 intermediatePos = Vector3.Lerp(defaultHandPoseFingers.intermediate.localPosition, targetHandPoseFingers.intermediate.localPosition, targetWeight);
			Quaternion intermediateRot = Quaternion.Lerp(defaultHandPoseFingers.intermediate.localRotation, targetHandPoseFingers.intermediate.localRotation, targetWeight);
			finger.intermediate.collider.transform.SetLocalPositionAndRotation(intermediatePos, intermediateRot);
			Vector3 distalPos = Vector3.Lerp(defaultHandPoseFingers.distal.localPosition, targetHandPoseFingers.distal.localPosition, targetWeight);
			Quaternion distalRot = Quaternion.Lerp(defaultHandPoseFingers.distal.localRotation, targetHandPoseFingers.distal.localRotation, targetWeight);
			finger.distal.collider.transform.SetLocalPositionAndRotation(distalPos, distalRot);
		}

		// Token: 0x06001DAC RID: 7596 RVA: 0x000C9D0C File Offset: 0x000C7F0C
		public virtual void UpdateFinger(RagdollHand.Finger finger, HandPoseData.Pose.Finger defaultHandPoseFingers)
		{
			finger.proximal.collider.transform.SetLocalPositionAndRotation(defaultHandPoseFingers.proximal.localPosition, defaultHandPoseFingers.proximal.localRotation);
			finger.intermediate.collider.transform.SetLocalPositionAndRotation(defaultHandPoseFingers.intermediate.localPosition, defaultHandPoseFingers.intermediate.localRotation);
			finger.distal.collider.transform.SetLocalPositionAndRotation(defaultHandPoseFingers.distal.localPosition, defaultHandPoseFingers.distal.localRotation);
		}

		// Token: 0x06001DAD RID: 7597 RVA: 0x000C9D9C File Offset: 0x000C7F9C
		public void SetGripFromPose(HandPoseData handPoseData)
		{
			if (handPoseData == null)
			{
				if (this.defaultHandPoseData == null)
				{
					this.ResetDefaultPose();
				}
				handPoseData = this.defaultHandPoseData;
				if (handPoseData == null)
				{
					return;
				}
			}
			HandPoseData.Pose pose = handPoseData.GetCreaturePose(this.ragdollHand.creature);
			if (pose != null)
			{
				Transform grip = this.ragdollHand.grip;
				HandPoseData.Pose.Fingers fingers = pose.GetFingers(this.ragdollHand.side);
				grip.SetLocalPositionAndRotation(fingers.gripLocalPosition, fingers.gripLocalRotation);
				grip.localScale = Vector3.one;
				return;
			}
			Debug.LogError("Could not find creature pose " + handPoseData.id + " for " + this.ragdollHand.creature.data.name);
		}

		// Token: 0x06001DAE RID: 7598 RVA: 0x000C9E44 File Offset: 0x000C8044
		public void SetDefaultPose(HandPoseData handPoseData)
		{
			if (handPoseData == null)
			{
				this.ResetDefaultPose();
				return;
			}
			this.defaultHandPoseData = handPoseData;
			this.defaultHandPoseFingers = this.defaultHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
		}

		// Token: 0x06001DAF RID: 7599 RVA: 0x000C9E83 File Offset: 0x000C8083
		public void ResetDefaultPose()
		{
			this.defaultHandPoseData = Catalog.GetData<HandPoseData>(this.defaultHandPoseId, true);
			this.defaultHandPoseFingers = this.defaultHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
		}

		// Token: 0x06001DB0 RID: 7600 RVA: 0x000C9EC4 File Offset: 0x000C80C4
		public void SetTargetWeight(float weight, bool lerpFingers = false)
		{
			RagdollHandPoser.<>c__DisplayClass33_0 CS$<>8__locals1;
			CS$<>8__locals1.weight = weight;
			this.targetWeight = CS$<>8__locals1.weight;
			if (lerpFingers)
			{
				RagdollHandPoser.<>c__DisplayClass33_1 CS$<>8__locals2;
				CS$<>8__locals2.ragdollDataFingerSpeed = this.ragdollHand.ragdoll.creature.data.ragdollData.fingerSpeed;
				CS$<>8__locals2.deltaTime = (this.ragdollHand.ragdoll.creature.isPlayer ? Time.unscaledDeltaTime : Time.deltaTime);
				RagdollHandPoser.<SetTargetWeight>g__UpdateFingerLerp|33_0(ref this.thumbCloseWeight, new Action<float>(this.UpdatePoseThumb), ref CS$<>8__locals1, ref CS$<>8__locals2);
				RagdollHandPoser.<SetTargetWeight>g__UpdateFingerLerp|33_0(ref this.indexCloseWeight, new Action<float>(this.UpdatePoseIndex), ref CS$<>8__locals1, ref CS$<>8__locals2);
				RagdollHandPoser.<SetTargetWeight>g__UpdateFingerLerp|33_0(ref this.middleCloseWeight, new Action<float>(this.UpdatePoseMiddle), ref CS$<>8__locals1, ref CS$<>8__locals2);
				RagdollHandPoser.<SetTargetWeight>g__UpdateFingerLerp|33_0(ref this.ringCloseWeight, new Action<float>(this.UpdatePoseRing), ref CS$<>8__locals1, ref CS$<>8__locals2);
				RagdollHandPoser.<SetTargetWeight>g__UpdateFingerLerp|33_0(ref this.littleCloseWeight, new Action<float>(this.UpdatePoseLittle), ref CS$<>8__locals1, ref CS$<>8__locals2);
				return;
			}
			this.UpdatePoseThumb(this.targetWeight);
			this.UpdatePoseIndex(this.targetWeight);
			this.UpdatePoseMiddle(this.targetWeight);
			this.UpdatePoseRing(this.targetWeight);
			this.UpdatePoseLittle(this.targetWeight);
		}

		// Token: 0x06001DB1 RID: 7601 RVA: 0x000CA000 File Offset: 0x000C8200
		public void SetTargetPose(HandPoseData handPoseData, bool allowThumbTracking = false, bool allowIndexTracking = false, bool allowMiddleTracking = false, bool allowRingTracking = false, bool allowLittleTracking = false)
		{
			if (handPoseData == null)
			{
				this.ResetTargetPose();
				return;
			}
			this.targetHandPoseData = handPoseData;
			if (this.targetHandPoseData == null)
			{
				this.targetHandPoseFingers = null;
				this.hasTargetHandPose = false;
				this.allowThumbTracking = (this.allowIndexTracking = (this.allowMiddleTracking = (this.allowRingTracking = (this.allowLittleTracking = false))));
				return;
			}
			this.targetHandPoseFingers = this.targetHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
			this.hasTargetHandPose = true;
			this.allowThumbTracking = allowThumbTracking;
			this.allowIndexTracking = allowIndexTracking;
			this.allowMiddleTracking = allowMiddleTracking;
			this.allowRingTracking = allowRingTracking;
			this.allowLittleTracking = allowLittleTracking;
		}

		// Token: 0x06001DB2 RID: 7602 RVA: 0x000CA0BC File Offset: 0x000C82BC
		public void ResetTargetPose()
		{
			this.targetHandPoseData = Catalog.GetData<HandPoseData>(this.targetHandPoseId, true);
			if (this.targetHandPoseData == null)
			{
				this.targetHandPoseFingers = null;
				this.hasTargetHandPose = false;
				this.allowThumbTracking = (this.allowIndexTracking = (this.allowMiddleTracking = (this.allowRingTracking = (this.allowLittleTracking = false))));
				return;
			}
			this.targetHandPoseFingers = this.targetHandPoseData.GetCreaturePose(this.ragdollHand.creature).GetFingers(this.ragdollHand.side);
			this.hasTargetHandPose = true;
			this.allowThumbTracking = (this.allowIndexTracking = (this.allowMiddleTracking = (this.allowRingTracking = (this.allowLittleTracking = true))));
		}

		// Token: 0x170001D9 RID: 473
		// (get) Token: 0x06001DB3 RID: 7603 RVA: 0x000CA17B File Offset: 0x000C837B
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001DB4 RID: 7604 RVA: 0x000CA180 File Offset: 0x000C8380
		protected internal override void ManagedUpdate()
		{
			if (this.ragdollHand.initialized && this.ragdollHand.ragdoll.creature.isPlayer)
			{
				this.poseComplete = true;
				if (this.hasTargetHandPose)
				{
					float deltaTime = Time.deltaTime;
					PlayerControl.Hand hand = PlayerControl.GetHand(this.ragdollHand.side);
					float ragdollDataFingerSpeed = this.ragdollHand.ragdoll.creature.data.ragdollData.fingerSpeed;
					bool grippingOrGrippable = this.ragdollHand.climb.isGripping;
					if (this.allowThumbTracking && !this.ragdollHand.climb.thumbContact)
					{
						float thumbTarget = grippingOrGrippable ? 1f : hand.thumbCurl;
						this.thumbCloseWeight = Mathf.MoveTowards(this.thumbCloseWeight, thumbTarget, ragdollDataFingerSpeed * deltaTime);
						if (!Mathf.Approximately(this.thumbCloseWeight, thumbTarget))
						{
							this.poseComplete = false;
							this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb, this.targetHandPoseFingers.thumb, this.thumbCloseWeight);
						}
					}
					if (this.allowIndexTracking && !this.ragdollHand.climb.indexContact)
					{
						float indexTarget = grippingOrGrippable ? 1f : hand.indexCurl;
						this.indexCloseWeight = Mathf.MoveTowards(this.indexCloseWeight, indexTarget, ragdollDataFingerSpeed * deltaTime);
						if (!Mathf.Approximately(this.indexCloseWeight, indexTarget))
						{
							this.poseComplete = false;
							this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index, this.targetHandPoseFingers.index, this.indexCloseWeight);
						}
					}
					if (this.allowMiddleTracking && !this.ragdollHand.climb.middleContact)
					{
						float middleTarget = grippingOrGrippable ? 1f : hand.middleCurl;
						this.middleCloseWeight = Mathf.MoveTowards(this.middleCloseWeight, middleTarget, ragdollDataFingerSpeed * deltaTime);
						if (!Mathf.Approximately(this.middleCloseWeight, middleTarget))
						{
							this.poseComplete = false;
							this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle, this.targetHandPoseFingers.middle, this.middleCloseWeight);
						}
					}
					if (this.allowRingTracking && !this.ragdollHand.climb.ringContact)
					{
						float ringTarget = grippingOrGrippable ? 1f : hand.ringCurl;
						this.ringCloseWeight = Mathf.MoveTowards(this.ringCloseWeight, ringTarget, ragdollDataFingerSpeed * deltaTime);
						if (!Mathf.Approximately(this.ringCloseWeight, ringTarget))
						{
							this.poseComplete = false;
							this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring, this.targetHandPoseFingers.ring, this.ringCloseWeight);
						}
					}
					if (this.allowLittleTracking && !this.ragdollHand.climb.littleContact)
					{
						float littleTarget = grippingOrGrippable ? 1f : hand.littleCurl;
						this.littleCloseWeight = Mathf.MoveTowards(this.littleCloseWeight, littleTarget, ragdollDataFingerSpeed * deltaTime);
						if (!Mathf.Approximately(this.littleCloseWeight, littleTarget))
						{
							this.poseComplete = false;
							this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little, this.targetHandPoseFingers.little, this.littleCloseWeight);
							return;
						}
					}
				}
				else
				{
					if (this.allowThumbTracking && !this.ragdollHand.climb.thumbContact)
					{
						this.UpdateFinger(this.ragdollHand.fingerThumb, this.defaultHandPoseFingers.thumb);
					}
					if (this.allowIndexTracking && !this.ragdollHand.climb.indexContact)
					{
						this.UpdateFinger(this.ragdollHand.fingerIndex, this.defaultHandPoseFingers.index);
					}
					if (this.allowMiddleTracking && !this.ragdollHand.climb.middleContact)
					{
						this.UpdateFinger(this.ragdollHand.fingerMiddle, this.defaultHandPoseFingers.middle);
					}
					if (this.allowRingTracking && !this.ragdollHand.climb.ringContact)
					{
						this.UpdateFinger(this.ragdollHand.fingerRing, this.defaultHandPoseFingers.ring);
					}
					if (this.allowLittleTracking && !this.ragdollHand.climb.littleContact)
					{
						this.UpdateFinger(this.ragdollHand.fingerLittle, this.defaultHandPoseFingers.little);
					}
				}
			}
		}

		// Token: 0x06001DB5 RID: 7605 RVA: 0x000CA5C4 File Offset: 0x000C87C4
		protected void OnDrawGizmosSelected()
		{
			Gizmos.DrawWireSphere(this.ragdollHand.grip.position, this.ragdollHand.collisionUngrabRadius);
			foreach (RagdollHand.Finger finger in this.ragdollHand.fingers)
			{
				Gizmos.color = Color.gray;
				Gizmos.DrawWireSphere(finger.distal.collider.transform.position, 0.001f);
				Gizmos.DrawWireSphere(finger.intermediate.collider.transform.position, 0.001f);
				Gizmos.DrawWireSphere(finger.proximal.collider.transform.position, 0.001f);
				Gizmos.DrawWireSphere(finger.tip.position, 0.001f);
				Gizmos.DrawLine(base.transform.position, finger.proximal.collider.transform.position);
				Gizmos.DrawLine(finger.proximal.collider.transform.position, finger.intermediate.collider.transform.position);
				Gizmos.DrawLine(finger.intermediate.collider.transform.position, finger.distal.collider.transform.position);
				Gizmos.DrawLine(finger.distal.collider.transform.position, finger.tip.position);
				Gizmos.color = Color.blue;
				Gizmos.DrawRay(finger.tip.position, finger.tip.forward * 0.01f);
				Gizmos.color = Color.green;
				Gizmos.DrawRay(finger.tip.position, finger.tip.up * 0.01f);
			}
			if (this.ragdollHand.grip)
			{
				Gizmos.matrix = this.ragdollHand.grip.localToWorldMatrix;
				Gizmos.color = Common.HueColourValue(HueColorName.Purple);
				Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(0.01f, 0.05f, 0.01f));
				Gizmos.DrawWireCube(new Vector3(0f, 0.03f, 0.01f), new Vector3(0.01f, 0.01f, 0.03f));
			}
		}

		// Token: 0x06001DB6 RID: 7606 RVA: 0x000CA85C File Offset: 0x000C8A5C
		public float GetCloseWeight(HandPoseData.FingerType type)
		{
			float result;
			switch (type)
			{
			case HandPoseData.FingerType.Thumb:
				result = this.thumbCloseWeight;
				break;
			case HandPoseData.FingerType.Index:
				result = this.indexCloseWeight;
				break;
			case HandPoseData.FingerType.Middle:
				result = this.middleCloseWeight;
				break;
			case HandPoseData.FingerType.Ring:
				result = this.ringCloseWeight;
				break;
			case HandPoseData.FingerType.Little:
				result = this.littleCloseWeight;
				break;
			default:
				throw new ArgumentOutOfRangeException("type", type, null);
			}
			return result;
		}

		// Token: 0x06001DB8 RID: 7608 RVA: 0x000CA91B File Offset: 0x000C8B1B
		[CompilerGenerated]
		internal static void <SetTargetWeight>g__UpdateFingerLerp|33_0(ref float closeWeight, Action<float> updatePoseFunc, ref RagdollHandPoser.<>c__DisplayClass33_0 A_2, ref RagdollHandPoser.<>c__DisplayClass33_1 A_3)
		{
			closeWeight = Mathf.MoveTowards(closeWeight, A_2.weight, A_3.ragdollDataFingerSpeed * A_3.deltaTime);
			updatePoseFunc(closeWeight);
		}

		// Token: 0x04001C29 RID: 7209
		public RagdollHand ragdollHand;

		// Token: 0x04001C2A RID: 7210
		[CatalogPicker(new Category[]
		{
			Category.HandPose
		})]
		public string defaultHandPoseId = "DefaultOpen";

		// Token: 0x04001C2B RID: 7211
		[Range(0f, 1f)]
		public float targetWeight;

		// Token: 0x04001C2C RID: 7212
		[CatalogPicker(new Category[]
		{
			Category.HandPose
		})]
		public string targetHandPoseId = "DefaultClose";

		// Token: 0x04001C2D RID: 7213
		public bool globalRatio = true;

		// Token: 0x04001C2E RID: 7214
		[Range(0f, 1f)]
		public float thumbCloseWeight;

		// Token: 0x04001C2F RID: 7215
		[Range(0f, 1f)]
		public float indexCloseWeight;

		// Token: 0x04001C30 RID: 7216
		[Range(0f, 1f)]
		public float middleCloseWeight;

		// Token: 0x04001C31 RID: 7217
		[Range(0f, 1f)]
		public float ringCloseWeight;

		// Token: 0x04001C32 RID: 7218
		[Range(0f, 1f)]
		public float littleCloseWeight;

		// Token: 0x04001C33 RID: 7219
		public bool allowThumbTracking = true;

		// Token: 0x04001C34 RID: 7220
		public bool allowIndexTracking = true;

		// Token: 0x04001C35 RID: 7221
		public bool allowMiddleTracking = true;

		// Token: 0x04001C36 RID: 7222
		public bool allowRingTracking = true;

		// Token: 0x04001C37 RID: 7223
		public bool allowLittleTracking = true;

		// Token: 0x04001C38 RID: 7224
		public HandPoseData.Pose.MirrorParams mirrorParams;

		// Token: 0x04001C39 RID: 7225
		[NonSerialized]
		public HandPoseData defaultHandPoseData;

		// Token: 0x04001C3A RID: 7226
		[NonSerialized]
		public HandPoseData.Pose.Fingers defaultHandPoseFingers;

		// Token: 0x04001C3B RID: 7227
		[NonSerialized]
		public HandPoseData targetHandPoseData;

		// Token: 0x04001C3C RID: 7228
		[NonSerialized]
		public HandPoseData.Pose.Fingers targetHandPoseFingers;

		// Token: 0x04001C3D RID: 7229
		[NonSerialized]
		public bool hasTargetHandPose;

		// Token: 0x04001C3E RID: 7230
		public bool poseComplete;
	}
}
