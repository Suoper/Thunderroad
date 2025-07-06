using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000284 RID: 644
	[AddComponentMenu("ThunderRoad/Creatures/Wrist stats")]
	public class WristStats : ThunderBehaviour
	{
		// Token: 0x06001E56 RID: 7766 RVA: 0x000CE81A File Offset: 0x000CCA1A
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06001E57 RID: 7767 RVA: 0x000CE827 File Offset: 0x000CCA27
		public void Awake()
		{
			this.creature = base.GetComponentInParent<Creature>();
			EventManager.onPossess += this.OnPossessionEvent;
		}

		// Token: 0x06001E58 RID: 7768 RVA: 0x000CE846 File Offset: 0x000CCA46
		public void OnPossessionEvent(Creature creature, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd && this.creature == creature)
			{
				this.InitEffects();
			}
		}

		// Token: 0x06001E59 RID: 7769 RVA: 0x000CE860 File Offset: 0x000CCA60
		public void InitEffects()
		{
			RagdollPart ragdollPart = base.GetComponentInParent<RagdollPart>();
			if (ragdollPart)
			{
				base.transform.SetParent(ragdollPart.meshBone, false);
			}
			if (this.healthEffectData == null && !string.IsNullOrEmpty(this.healthEffectId))
			{
				this.healthEffectData = Catalog.GetData<EffectData>(this.healthEffectId, true);
			}
			if (this.manaEffectData == null && !string.IsNullOrEmpty(this.manaEffectId))
			{
				this.manaEffectData = Catalog.GetData<EffectData>(this.manaEffectId, true);
			}
			if (this.focusEffectData == null && !string.IsNullOrEmpty(this.focusEffectId))
			{
				this.focusEffectData = Catalog.GetData<EffectData>(this.focusEffectId, true);
			}
			if (this.healthEffectInstance == null && this.healthEffectData != null)
			{
				this.healthEffectInstance = this.healthEffectData.Spawn(base.transform, true, null, false);
				foreach (Effect effect in this.healthEffectInstance.effects)
				{
					effect.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.Highlighter));
				}
			}
			if (this.manaEffectInstance == null && this.manaEffectData != null)
			{
				this.manaEffectInstance = this.manaEffectData.Spawn(base.transform, true, null, false);
				foreach (Effect effect2 in this.manaEffectInstance.effects)
				{
					effect2.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.Highlighter));
				}
			}
			if (this.focusEffectInstance == null && this.focusEffectData != null)
			{
				this.focusEffectInstance = this.focusEffectData.Spawn(base.transform, true, null, false);
				foreach (Effect effect3 in this.focusEffectInstance.effects)
				{
					effect3.gameObject.SetLayerRecursively(GameManager.GetLayer(LayerName.Highlighter));
				}
			}
		}

		// Token: 0x170001E5 RID: 485
		// (get) Token: 0x06001E5A RID: 7770 RVA: 0x000CEA78 File Offset: 0x000CCC78
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x06001E5B RID: 7771 RVA: 0x000CEA7C File Offset: 0x000CCC7C
		protected internal override void ManagedUpdate()
		{
			if (!this.creature.player)
			{
				return;
			}
			float eyesDistance = Vector3.Distance(this.creature.centerEyes.position, base.transform.position);
			float eyesAngle = Vector3.Angle(-this.creature.centerEyes.forward, base.transform.forward);
			if (this.isShown && !GameManager.options.showWristStats)
			{
				this.Show(false);
				return;
			}
			if (eyesDistance < this.showDistance && eyesAngle < this.showAngle)
			{
				if (!this.isShown)
				{
					this.Show(true);
					this.isShown = true;
				}
			}
			else if (this.isShown)
			{
				this.Show(false);
				this.isShown = false;
			}
			if (!this.isShown)
			{
				return;
			}
			EffectInstance effectInstance = this.healthEffectInstance;
			if (effectInstance != null)
			{
				effectInstance.SetIntensity((this.creature.maxHealth == float.PositiveInfinity) ? 1f : (this.creature.currentHealth / this.creature.maxHealth));
			}
			if (!this.creature.mana)
			{
				return;
			}
			EffectInstance effectInstance2 = this.manaEffectInstance;
			if (effectInstance2 != null)
			{
				effectInstance2.SetIntensity((this.creature.mana.MaxFocus == float.PositiveInfinity) ? 1f : (this.creature.mana.currentFocus / this.creature.mana.MaxFocus));
			}
			if (!this.creature.player)
			{
				return;
			}
			EffectInstance effectInstance3 = this.focusEffectInstance;
			if (effectInstance3 == null)
			{
				return;
			}
			effectInstance3.SetIntensity(Mathf.Lerp(0f, 0.75f, (this.creature.mana.MaxFocus == float.PositiveInfinity) ? 1f : (this.creature.mana.currentFocus / this.creature.mana.MaxFocus)));
		}

		// Token: 0x06001E5C RID: 7772 RVA: 0x000CEC60 File Offset: 0x000CCE60
		public void Show(bool active)
		{
			if (active)
			{
				this.InitEffects();
				if (this.healthEffectData != null)
				{
					EffectInstance effectInstance = this.healthEffectInstance;
					if (effectInstance != null)
					{
						effectInstance.Play(0, false, false);
					}
				}
				else
				{
					Debug.LogError("Wrist stats health effect is missing!");
				}
				if (this.manaEffectData != null)
				{
					EffectInstance effectInstance2 = this.manaEffectInstance;
					if (effectInstance2 != null)
					{
						effectInstance2.Play(0, false, false);
					}
				}
				else
				{
					Debug.LogError("Wrist stats mana effect is missing!");
				}
				if (this.focusEffectData == null)
				{
					Debug.LogError("Wrist stats focus effect is missing!");
					return;
				}
				EffectInstance effectInstance3 = this.focusEffectInstance;
				if (effectInstance3 == null)
				{
					return;
				}
				effectInstance3.Play(0, false, false);
				return;
			}
			else
			{
				EffectInstance effectInstance4 = this.healthEffectInstance;
				if (effectInstance4 != null)
				{
					effectInstance4.Stop(0);
				}
				EffectInstance effectInstance5 = this.manaEffectInstance;
				if (effectInstance5 != null)
				{
					effectInstance5.Stop(0);
				}
				EffectInstance effectInstance6 = this.focusEffectInstance;
				if (effectInstance6 == null)
				{
					return;
				}
				effectInstance6.Stop(0);
				return;
			}
		}

		// Token: 0x04001CCE RID: 7374
		public float showDistance = 0.31f;

		// Token: 0x04001CCF RID: 7375
		public float showAngle = 40f;

		// Token: 0x04001CD0 RID: 7376
		public string healthEffectId;

		// Token: 0x04001CD1 RID: 7377
		public string manaEffectId;

		// Token: 0x04001CD2 RID: 7378
		public string focusEffectId;

		// Token: 0x04001CD3 RID: 7379
		[NonSerialized]
		public Creature creature;

		// Token: 0x04001CD4 RID: 7380
		[NonSerialized]
		public bool isShown = true;

		// Token: 0x04001CD5 RID: 7381
		[NonSerialized]
		public EffectData healthEffectData;

		// Token: 0x04001CD6 RID: 7382
		[NonSerialized]
		public EffectData manaEffectData;

		// Token: 0x04001CD7 RID: 7383
		[NonSerialized]
		public EffectData focusEffectData;

		// Token: 0x04001CD8 RID: 7384
		protected EffectInstance healthEffectInstance;

		// Token: 0x04001CD9 RID: 7385
		protected EffectInstance manaEffectInstance;

		// Token: 0x04001CDA RID: 7386
		protected EffectInstance focusEffectInstance;
	}
}
