using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000253 RID: 595
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/CreatureEye.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Creature Eye")]
	public class CreatureEye : MonoBehaviour
	{
		// Token: 0x060019E7 RID: 6631 RVA: 0x000AC9BC File Offset: 0x000AABBC
		public void SetKeyframes()
		{
			foreach (CreatureEye.EyeMoveable moveable in this.eyeParts)
			{
				moveable.curves.SetKeyframeByRotation(moveable.transform, this.closeAmount);
			}
		}

		// Token: 0x060019E8 RID: 6632 RVA: 0x000ACA20 File Offset: 0x000AAC20
		public void SetClose()
		{
			foreach (CreatureEye.EyeMoveable moveable in this.eyeParts)
			{
				if ((!Application.isPlaying || moveable.isFixed) && moveable.transform)
				{
					moveable.transform.localRotation = moveable.curves.TimeToQuaternion(this.closeAmount);
				}
			}
		}

		// Token: 0x040018B5 RID: 6325
		[Range(0f, 1f)]
		public float closeAmount;

		// Token: 0x040018B6 RID: 6326
		[NonSerialized]
		public float lastUpdateTime;

		// Token: 0x040018B7 RID: 6327
		public string eyeTag = "";

		// Token: 0x040018B8 RID: 6328
		public List<CreatureEye.EyeMoveable> eyeParts = new List<CreatureEye.EyeMoveable>();

		// Token: 0x02000894 RID: 2196
		[Serializable]
		public class EyeMoveable
		{
			// Token: 0x1700052A RID: 1322
			// (get) Token: 0x060040B0 RID: 16560 RVA: 0x00189391 File Offset: 0x00187591
			// (set) Token: 0x060040B1 RID: 16561 RVA: 0x00189399 File Offset: 0x00187599
			public bool isFixed { get; private set; }

			// Token: 0x060040B2 RID: 16562 RVA: 0x001893A2 File Offset: 0x001875A2
			public void ParentingFix()
			{
				if (this.isFixed)
				{
					return;
				}
				this.isFixed = true;
				this.transform = this.transform.parent;
			}

			// Token: 0x04004208 RID: 16904
			public string name;

			// Token: 0x04004209 RID: 16905
			public Transform transform;

			// Token: 0x0400420A RID: 16906
			public CreatureEye.EyeMoveable.RotationCurves curves;

			// Token: 0x02000BDF RID: 3039
			[Serializable]
			public class RotationCurves
			{
				// Token: 0x06004A6B RID: 19051 RVA: 0x001A65F9 File Offset: 0x001A47F9
				public Quaternion TimeToQuaternion(float t)
				{
					return new Quaternion(this.closeCurveX.Evaluate(t), this.closeCurveY.Evaluate(t), this.closeCurveZ.Evaluate(t), this.closeCurveW.Evaluate(t));
				}

				// Token: 0x06004A6C RID: 19052 RVA: 0x001A6630 File Offset: 0x001A4830
				public void SetKeyframeByRotation(Transform pull, float t)
				{
					this.AddOrMoveKeyframe(this.closeCurveX, t, pull.localRotation.x);
					this.AddOrMoveKeyframe(this.closeCurveY, t, pull.localRotation.y);
					this.AddOrMoveKeyframe(this.closeCurveZ, t, pull.localRotation.z);
					this.AddOrMoveKeyframe(this.closeCurveW, t, pull.localRotation.w);
				}

				// Token: 0x06004A6D RID: 19053 RVA: 0x001A66A0 File Offset: 0x001A48A0
				public void AddOrMoveKeyframe(AnimationCurve curve, float t, float v)
				{
					if (curve.AddKey(t, v) != -1)
					{
						return;
					}
					Keyframe toChange = default(Keyframe);
					foreach (Keyframe frame in curve.keys)
					{
						if (frame.time == t)
						{
							toChange = frame;
							break;
						}
					}
					toChange.value = v;
				}

				// Token: 0x06004A6E RID: 19054 RVA: 0x001A66F4 File Offset: 0x001A48F4
				public void ClearCurves()
				{
					this.closeCurveX = new AnimationCurve();
					this.closeCurveY = new AnimationCurve();
					this.closeCurveZ = new AnimationCurve();
					this.closeCurveW = new AnimationCurve();
				}

				// Token: 0x04004D30 RID: 19760
				public AnimationCurve closeCurveX;

				// Token: 0x04004D31 RID: 19761
				public AnimationCurve closeCurveY;

				// Token: 0x04004D32 RID: 19762
				public AnimationCurve closeCurveZ;

				// Token: 0x04004D33 RID: 19763
				public AnimationCurve closeCurveW;
			}
		}
	}
}
