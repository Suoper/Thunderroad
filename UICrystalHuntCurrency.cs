using System;
using TMPro;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000379 RID: 889
	public class UICrystalHuntCurrency : MonoBehaviour
	{
		// Token: 0x06002A2D RID: 10797 RVA: 0x0011E128 File Offset: 0x0011C328
		private void Awake()
		{
			UIInventory.OnInventoryRefresh = (UIInventory.InventoryRefreshDelegate)Delegate.Combine(UIInventory.OnInventoryRefresh, new UIInventory.InventoryRefreshDelegate(this.Refresh));
			if (Player.characterData != null)
			{
				Player.characterData.inventory.OnCurrencyChangeEvent += this.OnCurrencyChange;
			}
			this.RefreshCounters();
		}

		// Token: 0x06002A2E RID: 10798 RVA: 0x0011E180 File Offset: 0x0011C380
		private void OnDestroy()
		{
			UIInventory.OnInventoryRefresh = (UIInventory.InventoryRefreshDelegate)Delegate.Remove(UIInventory.OnInventoryRefresh, new UIInventory.InventoryRefreshDelegate(this.Refresh));
			if (Player.characterData != null)
			{
				Player.characterData.inventory.OnCurrencyChangeEvent -= this.OnCurrencyChange;
			}
		}

		// Token: 0x06002A2F RID: 10799 RVA: 0x0011E1CF File Offset: 0x0011C3CF
		private void OnCurrencyChange(string currency, float oldValue, float newValue)
		{
			this.RefreshCounters();
		}

		// Token: 0x06002A30 RID: 10800 RVA: 0x0011E1D7 File Offset: 0x0011C3D7
		private void Refresh(UIInventory inventory)
		{
			this.RefreshCounters();
		}

		// Token: 0x06002A31 RID: 10801 RVA: 0x0011E1E0 File Offset: 0x0011C3E0
		private void RefreshCounters()
		{
			if (Player.characterData != null && Player.characterData.inventory != null)
			{
				string gold = Player.characterData.inventory.GetCurrencyString("Gold");
				string shards = Player.characterData.inventory.GetCurrencyString("CrystalShard");
				if (this.shardCounter)
				{
					this.shardCounter.text = (shards ?? "");
				}
				if (this.coinCounter)
				{
					this.coinCounter.text = (gold ?? "");
				}
			}
		}

		/// Only used for player inventory to show the shard and coin count
		// Token: 0x040027F2 RID: 10226
		public TMP_Text shardCounter;

		// Token: 0x040027F3 RID: 10227
		public TMP_Text coinCounter;
	}
}
