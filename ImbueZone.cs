using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ThunderRoad.Skill;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderRoad
{
	// Token: 0x020002D2 RID: 722
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/ImbueZone.html")]
	[RequireComponent(typeof(Collider))]
	public class ImbueZone : MonoBehaviour
	{
		// Token: 0x060022E5 RID: 8933 RVA: 0x000EFBC6 File Offset: 0x000EDDC6
		public List<ValueDropdownItem<string>> GetAllSpellCastChargeID()
		{
			return Catalog.GetDropdownAllID<SpellCastCharge>("None");
		}

		// Token: 0x060022E6 RID: 8934 RVA: 0x000EFBD2 File Offset: 0x000EDDD2
		public List<ValueDropdownItem<string>> GetAllSkillID()
		{
			return Catalog.GetDropdownAllID<SpellSkillData>("None");
		}

		// Token: 0x060022E7 RID: 8935 RVA: 0x000EFBDE File Offset: 0x000EDDDE
		public void Start()
		{
			this.SetImbueSpell(this.imbueSpellId);
			if (this.imbueSkillIDs != null)
			{
				this.SetImbueSkills(this.imbueSkillIDs);
			}
			this.imbueObjects = new List<SpellCaster.ImbueObject>();
		}

		// Token: 0x060022E8 RID: 8936 RVA: 0x000EFC0B File Offset: 0x000EDE0B
		public void SetImbueSpell(string newSpellID)
		{
			this.imbueSpellId = newSpellID;
			this.imbueSpell = Catalog.GetData<SpellCastCharge>(this.imbueSpellId, true);
		}

		// Token: 0x060022E9 RID: 8937 RVA: 0x000EFC28 File Offset: 0x000EDE28
		public void SetImbueSkills(List<string> imbueSkillIDs)
		{
			this.imbueSkillIDs = imbueSkillIDs;
			this.imbueSkillDatas = new List<SpellSkillData>();
			for (int i = 0; i < this.imbueSkillIDs.Count; i++)
			{
				SpellSkillData skill = Catalog.GetData<SpellSkillData>(imbueSkillIDs[0], true);
				if (skill != null)
				{
					this.imbueSkillDatas.Add(skill);
				}
			}
		}

		// Token: 0x060022EA RID: 8938 RVA: 0x000EFC7C File Offset: 0x000EDE7C
		public void Update()
		{
			for (int i = 0; i < this.imbueObjects.Count; i++)
			{
				Imbue imbue = this.imbueObjects[i].colliderGroup.imbue;
				float maxImbueEnergy = imbue.maxEnergy * (this.transferMaxPercent / 100f);
				SpellCastCharge spellCastBase = imbue.spellCastBase;
				int? num = (spellCastBase != null) ? new int?(spellCastBase.hashId) : null;
				int hashId = this.imbueSpell.hashId;
				if (!(num.GetValueOrDefault() == hashId & num != null) || imbue.energy < maxImbueEnergy)
				{
					float transferAmount = Mathf.Min(this.imbueSpell.imbueRate * this.transferRate * (1f / this.imbueObjects[i].colliderGroup.modifier.imbueRate) * Time.unscaledDeltaTime, maxImbueEnergy - imbue.energy);
					ColliderGroup colliderGroup = this.imbueObjects[i].colliderGroup;
					Creature creature;
					if (colliderGroup == null)
					{
						creature = null;
					}
					else
					{
						CollisionHandler collisionHandler = colliderGroup.collisionHandler;
						if (collisionHandler == null)
						{
							creature = null;
						}
						else
						{
							Item item = collisionHandler.item;
							if (item == null)
							{
								creature = null;
							}
							else
							{
								RagdollHand mainHandler = item.mainHandler;
								if (mainHandler == null)
								{
									creature = null;
								}
								else
								{
									Ragdoll ragdoll = mainHandler.ragdoll;
									creature = ((ragdoll != null) ? ragdoll.creature : null);
								}
							}
						}
					}
					Creature imbueCreature = creature;
					SpellCastCharge spell = imbue.Transfer(this.imbueSpell, transferAmount, imbueCreature);
					if (spell != null)
					{
						this.OnImbueLoaded(spell, imbue);
					}
				}
			}
		}

		// Token: 0x060022EB RID: 8939 RVA: 0x000EFDD8 File Offset: 0x000EDFD8
		private void OnImbueLoaded(SpellData spell, Imbue imbue)
		{
			foreach (SpellSkillData spellSkillData in this.imbueSkillDatas)
			{
				spellSkillData.OnImbueLoad(spell, imbue);
			}
		}

		// Token: 0x060022EC RID: 8940 RVA: 0x000EFE2C File Offset: 0x000EE02C
		public void OnTriggerEnter(Collider other)
		{
			ColliderGroup colliderGroup = other.GetComponentInParent<ColliderGroup>();
			if (colliderGroup)
			{
				if (colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.None)
				{
					return;
				}
				if (!this.imbueSpell.imbueAllowMetal && colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Metal)
				{
					return;
				}
				if (colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Custom && (colliderGroup.imbueCustomSpellID == null || this.imbueSpell.id.ToLower() != colliderGroup.imbueCustomSpellID.ToLower()))
				{
					return;
				}
				for (int i = this.imbueObjects.Count - 1; i >= 0; i--)
				{
					if (this.imbueObjects[i].colliderGroup == other.attachedRigidbody)
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

		// Token: 0x060022ED RID: 8941 RVA: 0x000EFF08 File Offset: 0x000EE108
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

		// Token: 0x040021F2 RID: 8690
		public float transferRate = 0.1f;

		// Token: 0x040021F3 RID: 8691
		public float transferMaxPercent = 50f;

		// Token: 0x040021F4 RID: 8692
		public string imbueSpellId;

		// Token: 0x040021F5 RID: 8693
		[FormerlySerializedAs("imbueSkillIds")]
		public List<string> imbueSkillIDs;

		// Token: 0x040021F6 RID: 8694
		[NonSerialized]
		public List<SpellSkillData> imbueSkillDatas;

		// Token: 0x040021F7 RID: 8695
		private List<SpellCaster.ImbueObject> imbueObjects;

		// Token: 0x040021F8 RID: 8696
		[NonSerialized]
		public SpellCastCharge imbueSpell;
	}
}
