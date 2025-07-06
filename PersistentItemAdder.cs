using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002E6 RID: 742
	public class PersistentItemAdder : MonoBehaviour
	{
		// Token: 0x060023BF RID: 9151 RVA: 0x000F4DB5 File Offset: 0x000F2FB5
		private void PlacementToTransform()
		{
			this.placement = new ContentStatePlaced
			{
				position = base.transform.position,
				rotation = base.transform.rotation
			};
		}

		// Token: 0x060023C0 RID: 9152 RVA: 0x000F4DE4 File Offset: 0x000F2FE4
		private void Start()
		{
			if (this.addOnStart)
			{
				this.AddItem();
			}
		}

		// Token: 0x060023C1 RID: 9153 RVA: 0x000F4DF4 File Offset: 0x000F2FF4
		public void AddItem()
		{
			PlayerSaveData characterData = Player.characterData;
			SavedInventoryData inventory = (characterData != null) ? characterData.inventory : null;
			if (inventory == null)
			{
				return;
			}
			if (inventory.ItemExistInSave(this.item.data))
			{
				return;
			}
			if (this.persistenceContainer.loadPlayerContainerID.IsNullOrEmptyOrWhitespace())
			{
				return;
			}
			this.item.state = this.placement;
			Player.characterData.AddToContainer(this.persistenceContainer.loadPlayerContainerID, this.item);
		}

		// Token: 0x040022D4 RID: 8916
		[Header("Item")]
		public ItemContent item = new ItemContent();

		// Token: 0x040022D5 RID: 8917
		[Header("Placement")]
		public ContentStatePlaced placement = new ContentStatePlaced();

		// Token: 0x040022D6 RID: 8918
		[Header("Container")]
		public Container persistenceContainer;

		// Token: 0x040022D7 RID: 8919
		public bool addOnStart = true;
	}
}
