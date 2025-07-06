using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200033F RID: 831
	public class FloatingHolder : ThunderBehaviour
	{
		// Token: 0x060026E4 RID: 9956 RVA: 0x0010C55C File Offset: 0x0010A75C
		protected void Awake()
		{
			this.collider = base.GetComponent<Collider>();
			this.item = base.GetComponentInParent<Item>();
		}

		// Token: 0x060026E5 RID: 9957 RVA: 0x0010C578 File Offset: 0x0010A778
		public void OnTriggerEnter(Collider other)
		{
			Item otherItem = other.GetComponentInParent<Item>();
			if (otherItem == this.item)
			{
				return;
			}
			if (otherItem == null)
			{
				return;
			}
			this.OnItemAdded(otherItem);
		}

		// Token: 0x060026E6 RID: 9958 RVA: 0x0010C5AC File Offset: 0x0010A7AC
		public void OnTriggerExit(Collider other)
		{
			Item otherItem = other.GetComponentInParent<Item>();
			if (otherItem == this.item)
			{
				return;
			}
			if (otherItem == null)
			{
				return;
			}
			this.OnItemRemoved(otherItem);
		}

		// Token: 0x060026E7 RID: 9959 RVA: 0x0010C5E0 File Offset: 0x0010A7E0
		public virtual void OnItemRemoved(Item otherItem)
		{
			this.items.Remove(otherItem);
			otherItem.OnGrabEvent -= this.OnItemGrabbed;
			otherItem.OnUngrabEvent -= this.OnItemUngrabbed;
		}

		// Token: 0x060026E8 RID: 9960 RVA: 0x0010C613 File Offset: 0x0010A813
		public virtual void OnItemAdded(Item otherItem)
		{
			if (this.items.Contains(otherItem))
			{
				return;
			}
			otherItem.OnGrabEvent += this.OnItemGrabbed;
			otherItem.OnUngrabEvent += this.OnItemUngrabbed;
		}

		// Token: 0x060026E9 RID: 9961 RVA: 0x0010C648 File Offset: 0x0010A848
		private void OnItemGrabbed(Handle handle, RagdollHand ragdollhand)
		{
			handle.item.transform.SetParent(null);
			handle.item.physicBody.isKinematic = false;
			handle.item.RemovePhysicModifier(this);
		}

		// Token: 0x060026EA RID: 9962 RVA: 0x0010C678 File Offset: 0x0010A878
		private void OnItemUngrabbed(Handle handle, RagdollHand ragdollhand, bool throwing)
		{
			if (handle.item.IsHeld())
			{
				return;
			}
			this.items.Add(handle.item);
			handle.item.transform.SetParent(base.transform);
			handle.item.physicBody.isKinematic = true;
			handle.item.SetPhysicModifier(this, new float?(0f), 0f, 0f, 0f, -1f, null);
		}

		// Token: 0x04002633 RID: 9779
		public Item item;

		// Token: 0x04002634 RID: 9780
		public Collider collider;

		// Token: 0x04002635 RID: 9781
		public bool isOpen;

		// Token: 0x04002636 RID: 9782
		public List<Item> items = new List<Item>();
	}
}
