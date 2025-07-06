using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200026B RID: 619
	public class SMRBoneTransferTool : MonoBehaviour
	{
		// Token: 0x06001BCC RID: 7116 RVA: 0x000B80C0 File Offset: 0x000B62C0
		public void ReBone()
		{
			Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
			SkinnedMeshRenderer[] componentsInChildren = this.sourceHierarchy.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				foreach (Transform bone in componentsInChildren[i].bones)
				{
					boneMap[bone.name] = bone;
				}
			}
			foreach (SkinnedMeshRenderer smr in this.targetHierarchy.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				Transform sourceRootBone;
				if (boneMap.TryGetValue(smr.rootBone.name, out sourceRootBone))
				{
					smr.rootBone = sourceRootBone;
				}
				else
				{
					Debug.LogError("failed to get bone: " + smr.rootBone.name);
				}
				Transform[] boneArray = smr.bones;
				for (int idx = 0; idx < boneArray.Length; idx++)
				{
					string boneName = boneArray[idx].name;
					if (!boneMap.TryGetValue(boneName, out boneArray[idx]))
					{
						Debug.LogError("failed to get bone: " + boneName);
					}
				}
				smr.bones = boneArray;
			}
		}

		// Token: 0x04001AAE RID: 6830
		public GameObject sourceHierarchy;

		// Token: 0x04001AAF RID: 6831
		public GameObject targetHierarchy;
	}
}
