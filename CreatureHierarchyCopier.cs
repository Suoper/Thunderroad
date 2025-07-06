using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000254 RID: 596
	public class CreatureHierarchyCopier : MonoBehaviour
	{
		// Token: 0x060019EA RID: 6634 RVA: 0x000ACAC2 File Offset: 0x000AACC2
		private void Awake()
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(this);
			}
		}

		// Token: 0x060019EB RID: 6635 RVA: 0x000ACAD4 File Offset: 0x000AACD4
		private void Copy()
		{
			foreach (object obj in this.copyFrom)
			{
				Transform directChild = (Transform)obj;
				if (!(directChild == this.fromRig))
				{
					MeshPart childMeshPart = directChild.GetComponent<MeshPart>();
					Transform target = this.copyTo.Find(directChild.name);
					if (target == null)
					{
						target = UnityEngine.Object.Instantiate<GameObject>(directChild.gameObject).transform;
						target.name = directChild.name;
						for (int i = target.childCount - 1; i >= 0; i--)
						{
							UnityEngine.Object.DestroyImmediate(target.GetChild(i).gameObject);
						}
					}
					target.parent = this.copyTo;
					if (this.copyMeshParts && childMeshPart)
					{
						MeshPart meshPart;
						if (!target.TryGetComponent<MeshPart>(out meshPart))
						{
							meshPart = target.gameObject.AddComponent<MeshPart>();
						}
						meshPart.skinnedMeshRenderer = this.copyTo.FindChildRecursive(childMeshPart.skinnedMeshRenderer.name).GetComponent<SkinnedMeshRenderer>();
						meshPart.defaultPhysicMaterial = childMeshPart.defaultPhysicMaterial;
						meshPart.idMap = childMeshPart.idMap;
					}
					foreach (object obj2 in directChild)
					{
						Transform subChild = (Transform)obj2;
						RevealDecal childReveal = subChild.GetComponent<RevealDecal>();
						Transform targetSub = this.copyTo.Find(subChild.name);
						targetSub.parent = target;
						if (this.copyReveal && childReveal)
						{
							RevealDecal subReveal;
							if (!targetSub.TryGetComponent<RevealDecal>(out subReveal))
							{
								subReveal = targetSub.gameObject.AddComponent<RevealDecal>();
							}
							subReveal.type = childReveal.type;
							subReveal.maskHeight = childReveal.maskHeight;
							subReveal.maskWidth = childReveal.maskWidth;
						}
					}
				}
			}
			if (this.reassignPartBones)
			{
				foreach (object obj3 in this.ragdollParts)
				{
					RagdollPart part = ((Transform)obj3).GetComponent<RagdollPart>();
					part.meshBone = this.toRig.FindChildRecursive(part.meshBone.name);
					if (this.alignParts)
					{
						part.transform.position = part.meshBone.position;
						part.transform.rotation = part.meshBone.rotation;
					}
					for (int j = 0; j < part.linkedMeshBones.Length; j++)
					{
						part.linkedMeshBones[j] = this.toRig.FindChildRecursive(part.linkedMeshBones[j].name);
					}
				}
			}
		}

		// Token: 0x040018B9 RID: 6329
		public Transform copyFrom;

		// Token: 0x040018BA RID: 6330
		public Transform fromRig;

		// Token: 0x040018BB RID: 6331
		public Transform copyTo;

		// Token: 0x040018BC RID: 6332
		public Transform toRig;

		// Token: 0x040018BD RID: 6333
		public Transform ragdollParts;

		// Token: 0x040018BE RID: 6334
		public bool copyMeshParts = true;

		// Token: 0x040018BF RID: 6335
		public bool copyReveal = true;

		// Token: 0x040018C0 RID: 6336
		public bool reassignPartBones = true;

		// Token: 0x040018C1 RID: 6337
		public bool alignParts = true;
	}
}
