using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002D0 RID: 720
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/HeatZone.html")]
	[RequireComponent(typeof(Collider))]
	public class HeatZone : MonoBehaviour
	{
		// Token: 0x060022DA RID: 8922 RVA: 0x000EF82D File Offset: 0x000EDA2D
		public void Start()
		{
			this.imbueObjects = new List<SpellCaster.ImbueObject>();
			this.imbueSpell = Catalog.GetData<SpellCastCharge>("Fire", true);
		}

		// Token: 0x060022DB RID: 8923 RVA: 0x000EF84C File Offset: 0x000EDA4C
		public void Update()
		{
			if (!this.active)
			{
				return;
			}
			for (int i = 0; i < this.imbueObjects.Count; i++)
			{
				Imbue imbue = this.imbueObjects[i].colliderGroup.imbue;
				float maxImbueEnergy = imbue.maxEnergy;
				SpellCastCharge spellCastBase = imbue.spellCastBase;
				int? num = (spellCastBase != null) ? new int?(spellCastBase.hashId) : null;
				int hashId = this.imbueSpell.hashId;
				if (!(num.GetValueOrDefault() == hashId & num != null) || imbue.energy < maxImbueEnergy)
				{
					imbue.Transfer(this.imbueSpell, Mathf.Min(this.imbueSpell.imbueRate * 5f * (1f / this.imbueObjects[i].colliderGroup.modifier.imbueRate) * Time.deltaTime, maxImbueEnergy - imbue.energy), null);
				}
			}
		}

		// Token: 0x060022DC RID: 8924 RVA: 0x000EF93C File Offset: 0x000EDB3C
		public void OnTriggerEnter(Collider other)
		{
			ColliderGroup colliderGroup = other.GetComponentInParent<ColliderGroup>();
			if (colliderGroup != null && colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Custom && colliderGroup.imbueCustomSpellID == "Fire")
			{
				for (int i = this.imbueObjects.Count - 1; i >= 0; i--)
				{
					if (this.imbueObjects[i].colliderGroup == colliderGroup)
					{
						return;
					}
				}
				if (colliderGroup.gameObject.GetComponentInParent<Item>())
				{
					this.imbueObjects.Add(new SpellCaster.ImbueObject(colliderGroup));
				}
			}
		}

		// Token: 0x060022DD RID: 8925 RVA: 0x000EF9C8 File Offset: 0x000EDBC8
		public void OnTriggerExit(Collider other)
		{
			ColliderGroup colliderGroup = other.GetComponentInParent<ColliderGroup>();
			if (colliderGroup != null)
			{
				for (int i = this.imbueObjects.Count - 1; i >= 0; i--)
				{
					if (this.imbueObjects[i].colliderGroup == colliderGroup)
					{
						if (this.imbueObjects[i].effectInstance != null)
						{
							this.imbueObjects[i].effectInstance.End(false, -1f);
						}
						this.imbueObjects.RemoveAt(i);
					}
				}
			}
		}

		// Token: 0x060022DE RID: 8926 RVA: 0x000EFA4B File Offset: 0x000EDC4B
		private void Clear()
		{
			this.imbueObjects.Clear();
		}

		// Token: 0x060022DF RID: 8927 RVA: 0x000EFA58 File Offset: 0x000EDC58
		public void Enable()
		{
			this.active = true;
		}

		// Token: 0x060022E0 RID: 8928 RVA: 0x000EFA61 File Offset: 0x000EDC61
		public void Disable()
		{
			this.Clear();
			this.active = false;
		}

		// Token: 0x040021E9 RID: 8681
		private List<SpellCaster.ImbueObject> imbueObjects;

		// Token: 0x040021EA RID: 8682
		[NonSerialized]
		public SpellCastCharge imbueSpell;

		// Token: 0x040021EB RID: 8683
		[Tooltip("When enabled, items placed in the zone will be imbued with fire.\n\nNote: The zone must be trigger, and the layer must be in the Zone Layer.")]
		public bool active;
	}
}
