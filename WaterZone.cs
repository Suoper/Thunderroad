using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002F8 RID: 760
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/WaterZone")]
	[RequireComponent(typeof(Collider))]
	public class WaterZone : MonoBehaviour
	{
		// Token: 0x06002442 RID: 9282 RVA: 0x000F7909 File Offset: 0x000F5B09
		public List<ValueDropdownItem<string>> GetAllSpellCastChargeID()
		{
			return Catalog.GetDropdownAllID<SpellCastCharge>("None");
		}

		// Token: 0x06002443 RID: 9283 RVA: 0x000F7915 File Offset: 0x000F5B15
		public void Start()
		{
			this.steamEffectData = Catalog.GetData<EffectData>(this.steamEffectId, true);
			this.imbueObjects = new List<SpellCaster.ImbueObject>();
		}

		// Token: 0x06002444 RID: 9284 RVA: 0x000F7934 File Offset: 0x000F5B34
		public void Update()
		{
			for (int i = 0; i < this.imbueObjects.Count; i++)
			{
				Imbue imbue = this.imbueObjects[i].colliderGroup.imbue;
				if (imbue.spellCastBase != null && imbue.spellCastBase.id == this.depletionSpellId && imbue.CanConsume(this.depletionRate * Time.deltaTime))
				{
					imbue.ConsumeInstant(this.depletionRate * Time.deltaTime * imbue.colliderGroup.data.GetModifier(imbue.colliderGroup).waterLossRateMultiplier, false);
				}
			}
		}

		// Token: 0x06002445 RID: 9285 RVA: 0x000F79D4 File Offset: 0x000F5BD4
		public void OnTriggerEnter(Collider other)
		{
			ColliderGroup colliderGroup = other.GetComponentInParent<ColliderGroup>();
			if (colliderGroup)
			{
				for (int i = this.imbueObjects.Count - 1; i >= 0; i--)
				{
					if (this.imbueObjects[i].colliderGroup == other.attachedRigidbody)
					{
						return;
					}
				}
				if (colliderGroup.imbue.energy > 0f && colliderGroup.imbue.spellCastBase.id == this.depletionSpellId)
				{
					EffectData effectData = this.steamEffectData;
					if (effectData != null)
					{
						EffectInstance effectInstance = effectData.Spawn(this.effectSpawnPoint, true, null, false);
						if (effectInstance != null)
						{
							effectInstance.Play(0, false, false);
						}
					}
				}
				if (colliderGroup.gameObject.GetComponentInParent<Item>())
				{
					this.imbueObjects.Add(new SpellCaster.ImbueObject(colliderGroup));
				}
			}
		}

		// Token: 0x06002446 RID: 9286 RVA: 0x000F7AA4 File Offset: 0x000F5CA4
		public void OnTriggerExit(Collider other)
		{
			ColliderGroup colliderGroup = other.GetComponentInParent<ColliderGroup>();
			if (colliderGroup)
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

		// Token: 0x0400238E RID: 9102
		public float depletionRate = 1f;

		// Token: 0x0400238F RID: 9103
		public string depletionSpellId = "Fire";

		// Token: 0x04002390 RID: 9104
		public string steamEffectId;

		// Token: 0x04002391 RID: 9105
		public Transform effectSpawnPoint;

		// Token: 0x04002392 RID: 9106
		protected EffectData steamEffectData;

		// Token: 0x04002393 RID: 9107
		protected List<SpellCaster.ImbueObject> imbueObjects;

		// Token: 0x04002394 RID: 9108
		[NonSerialized]
		public SpellCastCharge imbueSpell;
	}
}
