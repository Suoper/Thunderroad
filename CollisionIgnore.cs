using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000318 RID: 792
	public class CollisionIgnore : MonoBehaviour
	{
		// Token: 0x060025DA RID: 9690 RVA: 0x00104697 File Offset: 0x00102897
		private void OnValidate()
		{
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (this.colliders.Count == 0)
			{
				this.GetChildColliders();
			}
		}

		// Token: 0x060025DB RID: 9691 RVA: 0x001046BA File Offset: 0x001028BA
		protected void Awake()
		{
			if (this.ignoreCollisionOnAwake)
			{
				this.Set(true);
			}
		}

		// Token: 0x060025DC RID: 9692 RVA: 0x001046CC File Offset: 0x001028CC
		public void Set(bool ignoreCollision)
		{
			foreach (Collider collider in this.colliders)
			{
				foreach (Collider collider2 in this.colliders)
				{
					if (collider != collider2)
					{
						Physics.IgnoreCollision(collider, collider2, ignoreCollision);
					}
				}
			}
		}

		// Token: 0x060025DD RID: 9693 RVA: 0x00104764 File Offset: 0x00102964
		public void GetChildColliders()
		{
			this.colliders = new List<Collider>(base.gameObject.GetComponentsInChildren<Collider>(true));
		}

		// Token: 0x0400253E RID: 9534
		public bool ignoreCollisionOnAwake = true;

		// Token: 0x0400253F RID: 9535
		public List<Collider> colliders = new List<Collider>();
	}
}
