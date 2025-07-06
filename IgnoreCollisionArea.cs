using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000342 RID: 834
	public class IgnoreCollisionArea : ThunderBehaviour
	{
		// Token: 0x0600271E RID: 10014 RVA: 0x0010E9A9 File Offset: 0x0010CBA9
		protected void Awake()
		{
			this.collider = base.GetComponent<Collider>();
			this.collider.isTrigger = true;
			this.item = base.GetComponentInParent<Item>();
		}

		// Token: 0x0600271F RID: 10015 RVA: 0x0010E9D0 File Offset: 0x0010CBD0
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

		// Token: 0x06002720 RID: 10016 RVA: 0x0010EA04 File Offset: 0x0010CC04
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

		// Token: 0x06002721 RID: 10017 RVA: 0x0010EA38 File Offset: 0x0010CC38
		public virtual void OnItemRemoved(Item otherItem)
		{
			this.items.Remove(otherItem);
			otherItem.IgnoreItemCollision(this.item, false);
		}

		// Token: 0x06002722 RID: 10018 RVA: 0x0010EA54 File Offset: 0x0010CC54
		public virtual void OnItemAdded(Item otherItem)
		{
			if (this.items.Contains(otherItem))
			{
				return;
			}
			otherItem.IgnoreItemCollision(this.item, true);
		}

		// Token: 0x0400264F RID: 9807
		public Item item;

		// Token: 0x04002650 RID: 9808
		public Collider collider;

		// Token: 0x04002651 RID: 9809
		public List<Item> items = new List<Item>();
	}
}
