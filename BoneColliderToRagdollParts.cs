using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200024D RID: 589
	public class BoneColliderToRagdollParts : MonoBehaviour
	{
		// Token: 0x060018AB RID: 6315 RVA: 0x000A2E44 File Offset: 0x000A1044
		public void Transfer()
		{
			Dictionary<Transform, List<Collider>> colliderParents = new Dictionary<Transform, List<Collider>>();
			List<Collider> allColliders = new List<Collider>();
			allColliders.AddRange(this.rig.GetComponentsInChildren<Collider>());
			foreach (Transform bone in this.partsWithSubBones)
			{
				Collider[] componentsInChildren = bone.GetComponentsInChildren<Collider>();
				colliderParents.Add(bone, new List<Collider>());
				foreach (Collider collider in componentsInChildren)
				{
					colliderParents[bone].Add(collider);
					allColliders.Remove(collider);
				}
			}
			foreach (Collider collider2 in allColliders)
			{
				Transform parent = collider2.transform.parent;
				List<Collider> childs;
				if (!colliderParents.TryGetValue(parent, out childs))
				{
					childs = new List<Collider>();
					colliderParents.Add(parent, childs);
				}
				childs.Add(collider2);
			}
			Dictionary<Transform, RagdollPart> bonedParts = new Dictionary<Transform, RagdollPart>();
			foreach (KeyValuePair<Transform, List<Collider>> parentChilds in colliderParents)
			{
				Transform newPart = base.transform.FindOrAddTransform(parentChilds.Key.name, parentChilds.Key.position, new Quaternion?(parentChilds.Key.rotation), new Vector3?(parentChilds.Key.lossyScale));
				foreach (Collider collider3 in parentChilds.Value)
				{
					collider3.transform.parent = newPart;
				}
				if (this.addParts)
				{
					RagdollPart newRagdollPart = newPart.gameObject.AddComponent<RagdollPart>();
					newRagdollPart.meshBone = parentChilds.Key;
					bonedParts[parentChilds.Key] = newRagdollPart;
				}
			}
			foreach (KeyValuePair<Transform, RagdollPart> bonedPart in bonedParts)
			{
				RagdollPart part = bonedPart.Value;
				Transform boneParent = bonedPart.Key.parent;
				while (boneParent != null)
				{
					RagdollPart parentPart;
					if (bonedParts.TryGetValue(boneParent, out parentPart))
					{
						part.parentPart = parentPart;
						break;
					}
					boneParent = boneParent.parent;
				}
			}
		}

		// Token: 0x040017B8 RID: 6072
		public Transform rig;

		// Token: 0x040017B9 RID: 6073
		public bool addParts = true;

		// Token: 0x040017BA RID: 6074
		public List<Transform> partsWithSubBones = new List<Transform>();
	}
}
