using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002AD RID: 685
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ClothingGenderSwitcher.html")]
	[AddComponentMenu("ThunderRoad/Items/Clothing Gender Switcher")]
	[RequireComponent(typeof(Item))]
	public class ClothingGenderSwitcher : MonoBehaviour
	{
		// Token: 0x0600200E RID: 8206 RVA: 0x000DA0AD File Offset: 0x000D82AD
		public void Refresh()
		{
			if (Player.currentCreature)
			{
				this.SetGender(Player.currentCreature.data.gender);
				return;
			}
			this.SetGender(CreatureData.Gender.Male);
		}

		// Token: 0x0600200F RID: 8207 RVA: 0x000DA0D8 File Offset: 0x000D82D8
		private void OnEnable()
		{
			EventManager.onPossess += this.OnPossessionEvent;
			if (this.maleModel == null)
			{
				Debug.LogWarning("Item " + base.GetComponentInParent<Item>().data.id + "'s ClothingGenderSwitcher is missing its male model.");
			}
			if (this.femaleModel == null)
			{
				Debug.LogWarning("Item " + base.GetComponentInParent<Item>().data.id + "'s ClothingGenderSwitcher is missing its female model.");
			}
		}

		// Token: 0x06002010 RID: 8208 RVA: 0x000DA15A File Offset: 0x000D835A
		private void OnDisable()
		{
			EventManager.onPossess -= this.OnPossessionEvent;
		}

		// Token: 0x06002011 RID: 8209 RVA: 0x000DA16D File Offset: 0x000D836D
		private void OnPossessionEvent(Creature creature, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.SetGender(Player.currentCreature.data.gender);
			}
		}

		// Token: 0x06002012 RID: 8210 RVA: 0x000DA188 File Offset: 0x000D8388
		public void SetModelActive(bool enabled)
		{
			this.SetMaleModelActive(enabled);
			this.SetFemaleModelActive(enabled);
		}

		// Token: 0x06002013 RID: 8211 RVA: 0x000DA198 File Offset: 0x000D8398
		public void SetMaleModelActive(bool enabled)
		{
			if (this.maleModel)
			{
				this.maleModel.SetActive(enabled);
				return;
			}
			Debug.LogErrorFormat(this, "ClothingGenderSwitcher - maleModel variable is not assigned!", Array.Empty<object>());
		}

		// Token: 0x06002014 RID: 8212 RVA: 0x000DA1C4 File Offset: 0x000D83C4
		public void SetFemaleModelActive(bool enabled)
		{
			if (this.femaleModel)
			{
				this.femaleModel.SetActive(enabled);
				return;
			}
			Debug.LogErrorFormat(this, "ClothingGenderSwitcher - femaleModel variable is not assigned!", Array.Empty<object>());
		}

		// Token: 0x06002015 RID: 8213 RVA: 0x000DA1F0 File Offset: 0x000D83F0
		public void SetGender(CreatureData.Gender gender)
		{
			Item item = base.GetComponent<Item>();
			if (gender == CreatureData.Gender.Female)
			{
				this.SetMaleModelActive(false);
				this.SetFemaleModelActive(true);
				if (item != null)
				{
					item.mainHandleLeft = ((this.mainFemaleHandleLeft != null) ? this.mainFemaleHandleLeft : item.mainHandleLeft);
					item.mainHandleRight = ((this.mainFemaleHandleRight != null) ? this.mainFemaleHandleRight : item.mainHandleRight);
					return;
				}
			}
			else
			{
				this.SetMaleModelActive(true);
				this.SetFemaleModelActive(false);
				if (item != null)
				{
					item.mainHandleLeft = ((this.mainMaleHandleLeft != null) ? this.mainMaleHandleLeft : item.mainHandleLeft);
					item.mainHandleRight = ((this.mainMaleHandleRight != null) ? this.mainMaleHandleRight : item.mainHandleRight);
				}
			}
		}

		// Token: 0x04001F19 RID: 7961
		[Tooltip("Male wearable item (Select item, not model)")]
		public GameObject maleModel;

		// Token: 0x04001F1A RID: 7962
		[Tooltip("Main left handle of the male item.")]
		public Handle mainMaleHandleRight;

		// Token: 0x04001F1B RID: 7963
		[Tooltip("Main right handle of item.")]
		public Handle mainMaleHandleLeft;

		// Token: 0x04001F1C RID: 7964
		[Space]
		[Tooltip("Female wearable item(Select item, not model")]
		public GameObject femaleModel;

		// Token: 0x04001F1D RID: 7965
		[Tooltip("Main left handle of the female item.")]
		public Handle mainFemaleHandleRight;

		// Token: 0x04001F1E RID: 7966
		[Tooltip("Main right handle of the female item.")]
		public Handle mainFemaleHandleLeft;
	}
}
