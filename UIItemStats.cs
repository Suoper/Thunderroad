using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000384 RID: 900
	public class UIItemStats : MonoBehaviour
	{
		// Token: 0x06002AED RID: 10989 RVA: 0x001222E4 File Offset: 0x001204E4
		private void Awake()
		{
			if (this.statParent == null)
			{
				Debug.LogError("No stat parent set for UIItemStats");
				return;
			}
			this.itemTierIcons = new List<Sprite>(this.itemIconAddresses.Count);
			for (int i = 0; i < this.itemIconAddresses.Count; i++)
			{
				this.itemTierIcons.Add(null);
			}
			EventManager.onPossess += this.OnPossess;
			EventManager.onUnpossess += this.OnUnpossess;
			InventorySlot.OnTouchHolder += this.OnTouchHolder;
			UIInventory.OnOpen += this.OnOpen;
			ItemData startingHeldItem = null;
			if (Player.currentCreature != null)
			{
				RagdollHand ragdollHand = Player.currentCreature.GetHand(Side.Left);
				ragdollHand.OnGrabEvent += this.OnGrabEvent;
				ragdollHand.OnUnGrabEvent += this.OnUnGrabEvent;
				RagdollHand otherHand = Player.currentCreature.GetHand(Side.Right);
				otherHand.OnGrabEvent += this.OnGrabEvent;
				otherHand.OnUnGrabEvent += this.OnUnGrabEvent;
				if (ragdollHand.grabbedHandle != null && ragdollHand.grabbedHandle.item != null && !ragdollHand.grabbedHandle.item.data.HasModule<ItemModuleOpenContainer>())
				{
					startingHeldItem = ragdollHand.grabbedHandle.item.data;
				}
				else if (otherHand.grabbedHandle != null && otherHand.grabbedHandle.item != null && !otherHand.grabbedHandle.item.data.HasModule<ItemModuleOpenContainer>())
				{
					startingHeldItem = otherHand.grabbedHandle.item.data;
				}
			}
			this.statItem = startingHeldItem;
			this.UpdateStatsPage(startingHeldItem);
		}

		// Token: 0x06002AEE RID: 10990 RVA: 0x0012249C File Offset: 0x0012069C
		private void OnDestroy()
		{
			foreach (Sprite obj in this.itemTierIcons)
			{
				Catalog.ReleaseAsset<Sprite>(obj);
			}
			foreach (UIStatIcons itemStat in this.itemStats)
			{
				if (!(itemStat == null))
				{
					Addressables.ReleaseInstance(itemStat.gameObject);
				}
			}
			if (Player.currentCreature != null)
			{
				RagdollHand hand = Player.currentCreature.GetHand(Side.Left);
				hand.OnGrabEvent -= this.OnGrabEvent;
				hand.OnUnGrabEvent -= this.OnUnGrabEvent;
				RagdollHand hand2 = Player.currentCreature.GetHand(Side.Right);
				hand2.OnGrabEvent -= this.OnGrabEvent;
				hand2.OnUnGrabEvent -= this.OnUnGrabEvent;
			}
			EventManager.onPossess -= this.OnPossess;
			EventManager.onPossess -= this.OnUnpossess;
			InventorySlot.OnTouchHolder -= this.OnTouchHolder;
			UIInventory.OnOpen -= this.OnOpen;
		}

		// Token: 0x06002AEF RID: 10991 RVA: 0x001225EC File Offset: 0x001207EC
		private void OnPossess(Creature creature, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			RagdollHand hand = creature.GetHand(Side.Left);
			hand.OnGrabEvent += this.OnGrabEvent;
			hand.OnUnGrabEvent += this.OnUnGrabEvent;
			RagdollHand hand2 = creature.GetHand(Side.Right);
			hand2.OnGrabEvent += this.OnGrabEvent;
			hand2.OnUnGrabEvent += this.OnUnGrabEvent;
		}

		// Token: 0x06002AF0 RID: 10992 RVA: 0x00122654 File Offset: 0x00120854
		private void OnUnpossess(Creature creature, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			RagdollHand hand = creature.GetHand(Side.Left);
			hand.OnGrabEvent -= this.OnGrabEvent;
			hand.OnUnGrabEvent -= this.OnUnGrabEvent;
			RagdollHand hand2 = creature.GetHand(Side.Right);
			hand2.OnGrabEvent -= this.OnGrabEvent;
			hand2.OnUnGrabEvent -= this.OnUnGrabEvent;
		}

		// Token: 0x06002AF1 RID: 10993 RVA: 0x001226BA File Offset: 0x001208BA
		private void OnUnGrabEvent(Side side, Handle handle, bool throwing, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			if (handle.item != null && handle.item.data.HasModule<ItemModuleOpenContainer>())
			{
				return;
			}
			this.statItem = null;
			this.UpdateStatsPage(null);
		}

		// Token: 0x06002AF2 RID: 10994 RVA: 0x001226F4 File Offset: 0x001208F4
		private void OnGrabEvent(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			if (handle.item == null)
			{
				return;
			}
			if (handle.item != null && handle.item.data.HasModule<ItemModuleOpenContainer>())
			{
				return;
			}
			this.statItem = handle.item.data;
			this.UpdateStatsPage(this.statItem);
		}

		// Token: 0x06002AF3 RID: 10995 RVA: 0x00122754 File Offset: 0x00120954
		private void OnOpen(UIInventory inventory, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				return;
			}
			this.UpdateStatsPage(this.statItem);
		}

		// Token: 0x06002AF4 RID: 10996 RVA: 0x00122768 File Offset: 0x00120968
		private void OnTouchHolder(InventorySlot holder, RagdollHand ragdollHand, EventTime eventTime)
		{
			if (ragdollHand.ragdoll.creature.isPlayer)
			{
				if (eventTime == EventTime.OnStart && !ragdollHand.IsGrabbingOrTK() && holder.itemContent != null)
				{
					this.UpdateStatsPage(holder.itemContent.data);
				}
				if (eventTime == EventTime.OnEnd)
				{
					this.UpdateStatsPage(null);
				}
			}
		}

		// Token: 0x06002AF5 RID: 10997 RVA: 0x001227B6 File Offset: 0x001209B6
		public void UpdateStatsPage(ItemData newStatItem)
		{
			if (this.updateStatsPageCoroutine != null)
			{
				base.StopCoroutine(this.updateStatsPageCoroutine);
			}
			this.updateStatsPageCoroutine = GameManager.local.StartCoroutine(this.UpdateStatsPageCoroutine(newStatItem));
		}

		// Token: 0x06002AF6 RID: 10998 RVA: 0x001227E3 File Offset: 0x001209E3
		public IEnumerator UpdateStatsPageCoroutine(ItemData newStatItem)
		{
			if (newStatItem == null && this.statItem != null && Player.currentCreature != null)
			{
				RagdollHand leftHand = Player.currentCreature.GetHand(Side.Left);
				RagdollHand rightHand = leftHand.otherHand;
				Item leftHandItem = (leftHand.grabbedHandle != null) ? leftHand.grabbedHandle.item : null;
				Item rightHandItem = (rightHand.grabbedHandle != null) ? rightHand.grabbedHandle.item : null;
				if (leftHandItem != null && leftHandItem.data.id == this.statItem.id)
				{
					newStatItem = this.statItem;
				}
				else if (rightHandItem != null && rightHandItem.data.id == this.statItem.id)
				{
					newStatItem = this.statItem;
				}
			}
			if (newStatItem != null)
			{
				List<IStats> copiedStats = new List<IStats>();
				int tier = newStatItem.tier;
				try
				{
					TextData.Item itemLocalization = LocalizationManager.Instance.GetLocalizedTextItem(newStatItem.localizationId);
					if (itemLocalization != null)
					{
						this.itemName.text = itemLocalization.name;
						this.itemDescription.text = itemLocalization.description;
					}
					else
					{
						this.itemName.text = newStatItem.displayName;
						this.itemDescription.text = newStatItem.description;
					}
					if (tier < 0 || tier >= this.itemTierIcons.Count)
					{
						Debug.LogError(string.Format("Invalid tier {0} for item {1}", tier, newStatItem.id));
						tier = 0;
					}
				}
				catch (Exception e)
				{
					Debug.LogError(string.Format("Error updating stats page for item {0} {1}", newStatItem.id, e));
				}
				if (this.itemTierIcons[tier] == null)
				{
					string itemIconAddress = this.itemIconAddresses[tier];
					yield return Catalog.LoadAssetCoroutine<Sprite>(itemIconAddress, delegate(Sprite value)
					{
						this.itemTierIcons[tier] = value;
					}, "inventoryTierIcons");
				}
				try
				{
					Sprite tierIcon = this.itemTierIcons[tier];
					this.itemTierIcon.sprite = tierIcon;
					this.itemTierIcon.color = Color.white;
					ItemModuleStats moduleStats;
					if (newStatItem.TryGetModule<ItemModuleStats>(out moduleStats))
					{
						List<IStats> stats = moduleStats.stats;
						copiedStats.AddRange(stats);
					}
					ItemStatFloat itemMassStat = new ItemStatFloat("Mass", newStatItem.mass);
					copiedStats.Add(itemMassStat);
					float value2 = newStatItem.value;
					ItemModuleValueModifier valueModifier;
					if (newStatItem.TryGetModule<ItemModuleValueModifier>(out valueModifier))
					{
						value2 = valueModifier.GetValue();
					}
					copiedStats.Add(new ItemStatInt("Value", Mathf.RoundToInt(value2))
					{
						useStarIcons = false
					});
				}
				catch (Exception e2)
				{
					Debug.LogError(string.Format("Error updating stats page for item {0} {1}", newStatItem.id, e2));
				}
				int num;
				if (this.itemStats.Count < copiedStats.Count)
				{
					int count = copiedStats.Count - this.itemStats.Count;
					for (int i = 0; i < count; i = num + 1)
					{
						yield return Catalog.InstantiateCoroutine<UIStatIcons>(this.itemStatPrefabAddress, delegate(UIStatIcons uiItemStat)
						{
							if (uiItemStat == null)
							{
								Debug.LogError("UIItemStat is null at address: " + this.itemStatPrefabAddress);
								return;
							}
							Transform transform = uiItemStat.transform;
							transform.SetParent(this.statParent, false);
							transform.localPosition = Vector3.zero;
							transform.localRotation = Quaternion.identity;
							uiItemStat.gameObject.SetActive(false);
							this.itemStats.Add(uiItemStat);
						}, "UIItemStats");
						num = i;
					}
				}
				for (int count = 0; count < this.itemStats.Count; count = num + 1)
				{
					UIStatIcons uiStatIcons = this.itemStats[count];
					if (count < copiedStats.Count)
					{
						IStats stat = copiedStats[count];
						uiStatIcons.gameObject.SetActive(true);
						uiStatIcons.stat = stat;
						yield return uiStatIcons.Refresh();
					}
					else
					{
						uiStatIcons.gameObject.SetActive(false);
					}
					num = count;
				}
				yield break;
			}
			this.itemName.text = "";
			this.itemDescription.text = "";
			this.itemTierIcon.sprite = null;
			this.itemTierIcon.color = Color.clear;
			for (int j = 0; j < this.itemStats.Count; j++)
			{
				if (!(this.itemStats[j] == null))
				{
					this.itemStats[j].gameObject.SetActive(false);
				}
			}
			yield break;
		}

		// Token: 0x040028A6 RID: 10406
		public TMP_Text itemName;

		// Token: 0x040028A7 RID: 10407
		public TMP_Text itemDescription;

		// Token: 0x040028A8 RID: 10408
		public Image itemTierIcon;

		// Token: 0x040028A9 RID: 10409
		public List<string> itemIconAddresses = new List<string>
		{
			"Bas.Icon.InventorySheet[UI_InventoryIcons_spreadsheet_T0]",
			"Bas.Icon.InventorySheet[UI_InventoryIcons_spreadsheet_T1]",
			"Bas.Icon.InventorySheet[UI_InventoryIcons_spreadsheet_T2]",
			"Bas.Icon.InventorySheet[UI_InventoryIcons_spreadsheet_T3]",
			"Bas.Icon.InventorySheet[UI_InventoryIcons_spreadsheet_T4]"
		};

		// Token: 0x040028AA RID: 10410
		public Transform statParent;

		// Token: 0x040028AB RID: 10411
		public string itemStatPrefabAddress = "Bas.UI.Inventory.ItemStat";

		// Token: 0x040028AC RID: 10412
		private List<UIStatIcons> itemStats = new List<UIStatIcons>();

		// Token: 0x040028AD RID: 10413
		private List<Sprite> itemTierIcons = new List<Sprite>();

		// Token: 0x040028AE RID: 10414
		private ItemData statItem;

		// Token: 0x040028AF RID: 10415
		private Coroutine updateStatsPageCoroutine;
	}
}
