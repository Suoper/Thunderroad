using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B2 RID: 690
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/HandPoseSwapper")]
	[AddComponentMenu("ThunderRoad/Hand Pose Swapper")]
	[RequireComponent(typeof(HandlePose))]
	public class HandPoseSwapper : MonoBehaviour
	{
		// Token: 0x06002090 RID: 8336 RVA: 0x000DF5AB File Offset: 0x000DD7AB
		public void SetDefaultPoseByIndex(int newIDIndex)
		{
			this.SetDefaultPose(this.alternateHandPoseIDs[newIDIndex]);
		}

		// Token: 0x06002091 RID: 8337 RVA: 0x000DF5BF File Offset: 0x000DD7BF
		public void SetDefaultPose(string newPoseID)
		{
			this.SetPoses(newPoseID, this.handlePose.targetHandPoseId);
		}

		// Token: 0x06002092 RID: 8338 RVA: 0x000DF5D3 File Offset: 0x000DD7D3
		public void SetTargetPoseByIndex(int newIDIndex)
		{
			this.SetTargetPose(this.alternateHandPoseIDs[newIDIndex]);
		}

		// Token: 0x06002093 RID: 8339 RVA: 0x000DF5E7 File Offset: 0x000DD7E7
		public void SetTargetPose(string newPoseID)
		{
			this.SetPoses(this.handlePose.defaultHandPoseId, newPoseID);
		}

		// Token: 0x06002094 RID: 8340 RVA: 0x000DF5FB File Offset: 0x000DD7FB
		protected void Start()
		{
			this.handlePose = base.GetComponent<HandlePose>();
		}

		// Token: 0x06002095 RID: 8341 RVA: 0x000DF60C File Offset: 0x000DD80C
		protected void SetPoses(string defaultID, string targetID)
		{
			this.handlePose.defaultHandPoseId = defaultID;
			this.handlePose.targetHandPoseId = targetID;
			this.handlePose.LoadHandPosesData();
			Handle handle = this.handlePose.handle;
			int? num;
			if (handle == null)
			{
				num = null;
			}
			else
			{
				List<RagdollHand> handlers = handle.handlers;
				num = ((handlers != null) ? new int?(handlers.Count) : null);
			}
			int? num2 = num;
			if (num2.GetValueOrDefault() > 0)
			{
				foreach (RagdollHand ragdollHand in this.handlePose.handle.handlers)
				{
					ragdollHand.poser.SetDefaultPose(this.handlePose.defaultHandPoseData);
					ragdollHand.poser.SetTargetPose(this.handlePose.targetHandPoseData, false, false, false, false, false);
					ragdollHand.poser.SetTargetWeight(this.handlePose.targetWeight, false);
				}
			}
		}

		// Token: 0x04001F8D RID: 8077
		public List<string> alternateHandPoseIDs = new List<string>();

		// Token: 0x04001F8E RID: 8078
		protected HandlePose handlePose;
	}
}
