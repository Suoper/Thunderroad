using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B3 RID: 691
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/ImbueController.html")]
	[AddComponentMenu("ThunderRoad/Items/Imbue Controller")]
	[DisallowMultipleComponent]
	public class ImbueController : ThunderBehaviour
	{
		// Token: 0x1700020A RID: 522
		// (get) Token: 0x06002097 RID: 8343 RVA: 0x000DF723 File Offset: 0x000DD923
		// (set) Token: 0x06002098 RID: 8344 RVA: 0x000DF72B File Offset: 0x000DD92B
		public SpellCastCharge imbueSpell { get; protected set; }

		// Token: 0x1700020B RID: 523
		// (get) Token: 0x06002099 RID: 8345 RVA: 0x000DF734 File Offset: 0x000DD934
		// (set) Token: 0x0600209A RID: 8346 RVA: 0x000DF73C File Offset: 0x000DD93C
		public SpellCaster.ImbueObject imbueObject { get; protected set; }

		// Token: 0x0600209B RID: 8347 RVA: 0x000DF745 File Offset: 0x000DD945
		private void OnValidate()
		{
			this.TryAssignGroup();
		}

		// Token: 0x0600209C RID: 8348 RVA: 0x000DF74D File Offset: 0x000DD94D
		private void TryAssignGroup()
		{
			if (this.imbueGroup == null)
			{
				this.imbueGroup = base.GetComponent<ColliderGroup>();
			}
		}

		// Token: 0x0600209D RID: 8349 RVA: 0x000DF763 File Offset: 0x000DD963
		public void SetImbueID(string newID)
		{
			this.imbueSpellId = newID;
			if (this.imbueSpell != null && newID != this.imbueSpell.id)
			{
				this.imbueSpell = null;
			}
		}

		// Token: 0x0600209E RID: 8350 RVA: 0x000DF78E File Offset: 0x000DD98E
		public void SetImbueRate(float newRate)
		{
			this.imbueRate = newRate;
		}

		// Token: 0x0600209F RID: 8351 RVA: 0x000DF797 File Offset: 0x000DD997
		public void SetImbueMaxPercent(float newMax)
		{
			this.imbueMaxPercent = newMax;
		}

		// Token: 0x060020A0 RID: 8352 RVA: 0x000DF7A0 File Offset: 0x000DD9A0
		public void ClearImbueID()
		{
			this.SetImbueID(string.Empty);
		}

		// Token: 0x060020A1 RID: 8353 RVA: 0x000DF7AD File Offset: 0x000DD9AD
		public void ImbueUseStart()
		{
			this.ImbueUse(true);
		}

		// Token: 0x060020A2 RID: 8354 RVA: 0x000DF7B6 File Offset: 0x000DD9B6
		public void ImbueUseEnd()
		{
			this.ImbueUse(false);
		}

		// Token: 0x060020A3 RID: 8355 RVA: 0x000DF7BF File Offset: 0x000DD9BF
		private void Awake()
		{
			this.TryAssignGroup();
		}

		// Token: 0x060020A4 RID: 8356 RVA: 0x000DF7C7 File Offset: 0x000DD9C7
		private void Start()
		{
			this.SetImbueID(this.imbueSpellId);
		}

		// Token: 0x060020A5 RID: 8357 RVA: 0x000DF7D8 File Offset: 0x000DD9D8
		protected void ImbueUse(bool start)
		{
			Creature currentCreature = Player.currentCreature;
			RagdollHand hand = ((currentCreature != null) ? currentCreature.GetHand(Side.Right) : null) ?? Creature.all[0].GetHand(Side.Right);
			CollisionHandler collisionHandler = this.imbueGroup.collisionHandler;
			if (collisionHandler != null && collisionHandler.isItem)
			{
				hand = (this.imbueGroup.collisionHandler.item.mainHandler ?? this.imbueGroup.collisionHandler.item.lastHandler);
			}
			CollisionHandler collisionHandler2 = this.imbueGroup.collisionHandler;
			if (collisionHandler2 != null && collisionHandler2.isRagdollPart)
			{
				RagdollPart part = this.imbueGroup.collisionHandler.ragdollPart;
				foreach (RagdollHand handler in part.ragdoll.handlers)
				{
					HandleRagdoll handleRagdoll = handler.grabbedHandle as HandleRagdoll;
					if (handleRagdoll != null && handleRagdoll.ragdollPart == part)
					{
						hand = handler;
						break;
					}
				}
				if (hand == null)
				{
					hand = (part.ragdoll.creature.GetHand(Side.Right) ?? part.ragdoll.creature.GetHand(Side.Left));
				}
			}
			if (hand == null)
			{
				Debug.LogError("You are activating an ImbueController's ImbueUseStart or ImbueUseEnd method before any creatures exist! This shouldn't happen ever.\nThe method is aborting to prevent further errors.");
				return;
			}
			this.imbueObject.colliderGroup.imbue.OnCrystalUse(hand, start);
		}

		// Token: 0x1700020C RID: 524
		// (get) Token: 0x060020A6 RID: 8358 RVA: 0x000DF940 File Offset: 0x000DDB40
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x060020A7 RID: 8359 RVA: 0x000DF944 File Offset: 0x000DDB44
		protected internal override void ManagedUpdate()
		{
			if (this.imbueGroup == null)
			{
				return;
			}
			if (this.imbueObject == null || this.imbueObject.colliderGroup != this.imbueGroup)
			{
				this.imbueObject = new SpellCaster.ImbueObject(this.imbueGroup);
			}
			if (!string.IsNullOrEmpty(this.imbueSpellId) && (this.imbueSpell == null || this.imbueSpell.id != this.imbueSpellId))
			{
				this.imbueSpell = Catalog.GetData<SpellCastCharge>(this.imbueSpellId, true);
			}
			if (this.imbueSpell == null && this.imbueObject.colliderGroup.imbue.spellCastBase != null)
			{
				this.imbueSpell = this.imbueObject.colliderGroup.imbue.spellCastBase;
			}
			if (this.imbueSpell != null)
			{
				if (this.imbueRate < 0f && this.imbueObject.colliderGroup.imbue.energy <= 0f)
				{
					return;
				}
				if (this.imbueRate > 0f && this.imbueObject.colliderGroup.imbue.energy >= this.imbueObject.colliderGroup.imbue.maxEnergy * (this.imbueMaxPercent / 100f) && this.imbueSpellId == this.imbueObject.colliderGroup.imbue.spellCastBase.id)
				{
					return;
				}
				this.imbueObject.colliderGroup.imbue.Transfer(this.imbueSpell, this.imbueSpell.imbueRate * this.imbueRate * (1f / this.imbueObject.colliderGroup.modifier.imbueRate) * Time.deltaTime, null);
			}
		}

		// Token: 0x04001F8F RID: 8079
		public ColliderGroup imbueGroup;

		// Token: 0x04001F90 RID: 8080
		[Range(-100f, 100f)]
		public float imbueRate;

		// Token: 0x04001F91 RID: 8081
		[Range(0f, 100f)]
		public float imbueMaxPercent = 50f;

		// Token: 0x04001F92 RID: 8082
		public string imbueSpellId;
	}
}
