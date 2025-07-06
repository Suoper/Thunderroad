using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B1 RID: 689
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/HandlePose.html")]
	public class HandlePose : MonoBehaviour
	{
		// Token: 0x06002089 RID: 8329 RVA: 0x000DF255 File Offset: 0x000DD455
		private void Awake()
		{
			if (!this.handle)
			{
				this.handle = base.GetComponentInParent<Handle>();
			}
		}

		// Token: 0x0600208A RID: 8330 RVA: 0x000DF270 File Offset: 0x000DD470
		private void Start()
		{
			this.LoadHandPosesData();
		}

		// Token: 0x0600208B RID: 8331 RVA: 0x000DF278 File Offset: 0x000DD478
		public void LoadHandPosesData()
		{
			this.defaultHandPoseData = (Catalog.GetData<HandPoseData>(this.defaultHandPoseId, true) ?? Catalog.GetData<HandPoseData>("HandleDefault", true));
			this.targetHandPoseData = (Catalog.GetData<HandPoseData>(this.targetHandPoseId, true) ?? Catalog.GetData<HandPoseData>("HandleDefault", true));
		}

		// Token: 0x0600208C RID: 8332 RVA: 0x000DF2C8 File Offset: 0x000DD4C8
		public void UpdateName()
		{
			if (this.handle)
			{
				if (this.side == Side.Right)
				{
					base.name = "OrientRight" + ((this.handle.orientationDefaultRight == this) ? "_Default" : "");
					return;
				}
				if (this.side == Side.Left)
				{
					base.name = "OrientLeft" + ((this.handle.orientationDefaultLeft == this) ? "_Default" : "");
				}
			}
		}

		// Token: 0x0600208D RID: 8333 RVA: 0x000DF354 File Offset: 0x000DD554
		protected void GizmoDrawFinger(HandPoseData.Pose.Finger finger, HandPoseData.Pose.Finger finger2, Matrix4x4 matrix)
		{
			Vector3 proximalLocalPosition = finger.proximal.localPosition;
			Quaternion proximalLocalRotation = finger.proximal.localRotation;
			Vector3 intermediateLocalPosition = finger.intermediate.localPosition;
			Quaternion intermediateLocalRotation = finger.intermediate.localRotation;
			Vector3 distalLocalPosition = finger.distal.localPosition;
			Quaternion distalLocalRotation = finger.distal.localRotation;
			Vector3 tipLocalPosition = finger.tipLocalPosition;
			if (finger2 != null)
			{
				proximalLocalPosition = Vector3.Lerp(finger.proximal.localPosition, finger2.proximal.localPosition, this.targetWeight);
				proximalLocalRotation = Quaternion.Lerp(finger.proximal.localRotation, finger2.proximal.localRotation, this.targetWeight);
				intermediateLocalPosition = Vector3.Lerp(finger.intermediate.localPosition, finger2.intermediate.localPosition, this.targetWeight);
				intermediateLocalRotation = Quaternion.Lerp(finger.intermediate.localRotation, finger2.intermediate.localRotation, this.targetWeight);
				distalLocalPosition = Vector3.Lerp(finger.distal.localPosition, finger2.distal.localPosition, this.targetWeight);
				distalLocalRotation = Quaternion.Lerp(finger.distal.localRotation, finger2.distal.localRotation, this.targetWeight);
				tipLocalPosition = Vector3.Lerp(finger.tipLocalPosition, finger2.tipLocalPosition, this.targetWeight);
			}
			Gizmos.matrix = matrix;
			Gizmos.DrawWireSphere(proximalLocalPosition, 0.006f);
			if (this.CheckQuaternion(proximalLocalRotation))
			{
				Gizmos.matrix *= Matrix4x4.TRS(proximalLocalPosition, proximalLocalRotation, Vector3.one);
				Gizmos.DrawWireSphere(intermediateLocalPosition, 0.005f);
				Gizmos.DrawLine(Vector3.zero, intermediateLocalPosition);
			}
			if (this.CheckQuaternion(intermediateLocalRotation))
			{
				Gizmos.matrix *= Matrix4x4.TRS(intermediateLocalPosition, intermediateLocalRotation, Vector3.one);
				Gizmos.DrawWireSphere(distalLocalPosition, 0.003f);
				Gizmos.DrawLine(Vector3.zero, distalLocalPosition);
			}
			if (this.CheckQuaternion(distalLocalRotation))
			{
				Gizmos.matrix *= Matrix4x4.TRS(distalLocalPosition, distalLocalRotation, Vector3.one);
				Gizmos.DrawWireSphere(tipLocalPosition, 0.002f);
				Gizmos.DrawLine(Vector3.zero, tipLocalPosition);
			}
		}

		// Token: 0x0600208E RID: 8334 RVA: 0x000DF566 File Offset: 0x000DD766
		protected bool CheckQuaternion(Quaternion quaternion)
		{
			return quaternion.x + quaternion.y + quaternion.z + quaternion.w != 0f;
		}

		// Token: 0x04001F82 RID: 8066
		[Tooltip("References the handle this handpose is attached to.")]
		public Handle handle;

		// Token: 0x04001F83 RID: 8067
		[Tooltip("Depicts which hand this handpose directs to.")]
		public Side side;

		// Token: 0x04001F84 RID: 8068
		[Tooltip("ID of the handpose that is set to default if the target weight is zero.")]
		[CatalogPicker(new Category[]
		{
			Category.HandPose
		})]
		public string defaultHandPoseId = "HandleDefault";

		// Token: 0x04001F85 RID: 8069
		[Tooltip("A per-HandlePose override for the Handle's SpellOrbTarget")]
		public Transform spellOrbTarget;

		// Token: 0x04001F86 RID: 8070
		[NonSerialized]
		public HandPoseData defaultHandPoseData;

		// Token: 0x04001F87 RID: 8071
		protected HandPoseData.Pose defaultHandPose;

		// Token: 0x04001F88 RID: 8072
		[Range(0f, 1f)]
		[Tooltip("Blends the \"Default\" handpose and the \"Target\" handpose, allowing you to create more unique and fitting handposes without needing to create new ones.")]
		public float targetWeight;

		// Token: 0x04001F89 RID: 8073
		[NonSerialized]
		public float lastTargetWeight = -1f;

		// Token: 0x04001F8A RID: 8074
		[Tooltip("ID of the handpose that is used to blend against the default handpose. Handpose that is used if the target weight is one.")]
		[CatalogPicker(new Category[]
		{
			Category.HandPose
		})]
		public string targetHandPoseId;

		// Token: 0x04001F8B RID: 8075
		[NonSerialized]
		public HandPoseData targetHandPoseData;

		// Token: 0x04001F8C RID: 8076
		protected HandPoseData.Pose targetHandPose;
	}
}
