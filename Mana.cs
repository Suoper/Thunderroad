using System;
using System.Collections.Generic;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000275 RID: 629
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Mana.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Mana")]
	[RequireComponent(typeof(Creature))]
	public class Mana : ThunderBehaviour
	{
		// Token: 0x140000C9 RID: 201
		// (add) Token: 0x06001C86 RID: 7302 RVA: 0x000BF7BC File Offset: 0x000BD9BC
		// (remove) Token: 0x06001C87 RID: 7303 RVA: 0x000BF7F4 File Offset: 0x000BD9F4
		public event Mana.FocusChangeEvent OnFocusChange;

		// Token: 0x170001CA RID: 458
		// (get) Token: 0x06001C88 RID: 7304 RVA: 0x000BF829 File Offset: 0x000BDA29
		// (set) Token: 0x06001C89 RID: 7305 RVA: 0x000BF831 File Offset: 0x000BDA31
		public float currentFocus
		{
			get
			{
				return this._currentFocus;
			}
			set
			{
				this._currentFocus = value;
				Mana.FocusChangeEvent onFocusChange = this.OnFocusChange;
				if (onFocusChange == null)
				{
					return;
				}
				onFocusChange(this._currentFocus, this.MaxFocus);
			}
		}

		// Token: 0x170001CB RID: 459
		// (get) Token: 0x06001C8A RID: 7306 RVA: 0x000BF856 File Offset: 0x000BDA56
		// (set) Token: 0x06001C8B RID: 7307 RVA: 0x000BF85E File Offset: 0x000BDA5E
		public float baseMaxFocus
		{
			get
			{
				return this._baseMaxFocus;
			}
			set
			{
				this._baseMaxFocus = value;
				Mana.FocusChangeEvent onFocusChange = this.OnFocusChange;
				if (onFocusChange == null)
				{
					return;
				}
				onFocusChange(this._currentFocus, this.MaxFocus);
			}
		}

		// Token: 0x170001CC RID: 460
		// (get) Token: 0x06001C8C RID: 7308 RVA: 0x000BF883 File Offset: 0x000BDA83
		public float MaxFocus
		{
			get
			{
				return this.baseMaxFocus * (1f + this.maxFocusMult);
			}
		}

		// Token: 0x170001CD RID: 461
		// (get) Token: 0x06001C8D RID: 7309 RVA: 0x000BF898 File Offset: 0x000BDA98
		// (set) Token: 0x06001C8E RID: 7310 RVA: 0x000BF8A0 File Offset: 0x000BDAA0
		public float maxFocusMult
		{
			get
			{
				return this._maxFocusMult;
			}
			set
			{
				this._maxFocusMult = value;
				Mana.FocusChangeEvent onFocusChange = this.OnFocusChange;
				if (onFocusChange == null)
				{
					return;
				}
				onFocusChange(this._currentFocus, this.MaxFocus);
			}
		}

		// Token: 0x140000CA RID: 202
		// (add) Token: 0x06001C8F RID: 7311 RVA: 0x000BF8C8 File Offset: 0x000BDAC8
		// (remove) Token: 0x06001C90 RID: 7312 RVA: 0x000BF900 File Offset: 0x000BDB00
		public event Mana.SpellLoadEvent OnSpellLoadEvent;

		// Token: 0x140000CB RID: 203
		// (add) Token: 0x06001C91 RID: 7313 RVA: 0x000BF938 File Offset: 0x000BDB38
		// (remove) Token: 0x06001C92 RID: 7314 RVA: 0x000BF970 File Offset: 0x000BDB70
		public event Mana.SpellLoadEvent OnSpellUnloadEvent;

		// Token: 0x140000CC RID: 204
		// (add) Token: 0x06001C93 RID: 7315 RVA: 0x000BF9A8 File Offset: 0x000BDBA8
		// (remove) Token: 0x06001C94 RID: 7316 RVA: 0x000BF9E0 File Offset: 0x000BDBE0
		public event Mana.ImbueLoadEvent OnImbueLoadEvent;

		// Token: 0x140000CD RID: 205
		// (add) Token: 0x06001C95 RID: 7317 RVA: 0x000BFA18 File Offset: 0x000BDC18
		// (remove) Token: 0x06001C96 RID: 7318 RVA: 0x000BFA50 File Offset: 0x000BDC50
		public event Mana.ImbueLoadEvent OnImbueUnloadEvent;

		// Token: 0x140000CE RID: 206
		// (add) Token: 0x06001C97 RID: 7319 RVA: 0x000BFA88 File Offset: 0x000BDC88
		// (remove) Token: 0x06001C98 RID: 7320 RVA: 0x000BFAC0 File Offset: 0x000BDCC0
		public event Mana.PowerUseEvent OnPowerUseEvent;

		// Token: 0x140000CF RID: 207
		// (add) Token: 0x06001C99 RID: 7321 RVA: 0x000BFAF8 File Offset: 0x000BDCF8
		// (remove) Token: 0x06001C9A RID: 7322 RVA: 0x000BFB30 File Offset: 0x000BDD30
		public event Mana.MergeCastEvent OnMergeCastStep;

		// Token: 0x06001C9B RID: 7323 RVA: 0x000BFB65 File Offset: 0x000BDD65
		public SpellCaster GetCaster(Side side)
		{
			if (side == Side.Left)
			{
				return this.casterLeft;
			}
			return this.casterRight;
		}

		// Token: 0x06001C9C RID: 7324 RVA: 0x000BFB78 File Offset: 0x000BDD78
		private void Awake()
		{
			this.creature = base.GetComponent<Creature>();
			this.mergePoint = new GameObject("SpellMerge").transform;
			this.mergePoint.SetParent(base.transform, false);
			this.overlapColliders = new Collider[200];
			this.focusConsumptionMult = new FloatHandler();
		}

		// Token: 0x06001C9D RID: 7325 RVA: 0x000BFBD3 File Offset: 0x000BDDD3
		public void Init(Creature creature)
		{
			this.casterRight = creature.handRight.caster;
			this.casterLeft = creature.handLeft.caster;
			this.initialized = true;
		}

		// Token: 0x06001C9E RID: 7326 RVA: 0x000BFC00 File Offset: 0x000BDE00
		public void Load()
		{
			this.chargeSpeedMult = new FloatHandler();
			this.baseMaxFocus = (this.currentFocus = this.creature.data.focus);
			this.focusRegen = this.creature.data.focusRegen;
			this.focusRegenPerSkill = this.creature.data.focusRegenPerSkill;
			this.maxFocusPerSkill = this.creature.data.maxFocusPerSkill;
			this.overlapRadius = this.creature.data.overlapRadius;
			this.overlapMinDelay = this.creature.data.overlapMinDelay;
			this.overlapMask = this.creature.data.overlapMask;
			this.focusReady = (this.focusFull = true);
			this.RemoveSpell(null);
			this.creature.container.OnContentAddEvent += this.OnContainerContentAdd;
			this.creature.container.OnContentRemoveEvent += this.OnContainerContentRemove;
			foreach (SpellContent spellContent in this.creature.container.contents.GetEnumerableContentsOfType(true, Array.Empty<Func<SpellContent, bool>>()))
			{
				this.AddSpell(spellContent.data);
			}
			this.RefreshMultipliers();
		}

		// Token: 0x06001C9F RID: 7327 RVA: 0x000BFD6C File Offset: 0x000BDF6C
		public void OnContainerContentAdd(ContainerContent content, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			SpellContent spellContent = content as SpellContent;
			if (spellContent != null)
			{
				if (spellContent.data != null)
				{
					this.TempUnloadSpells();
					this.creature.ForceLoadSkill(spellContent.data.id);
					this.AddSpell(spellContent.data);
					this.TempReloadSpells();
				}
			}
			else
			{
				SkillContent skillContent = content as SkillContent;
				if (skillContent != null && skillContent.data != null)
				{
					Imbue.TempUnloadAll();
					this.TempUnloadSpells();
					this.creature.ForceLoadSkill(skillContent.data.id);
					this.TempReloadSpells();
					Imbue.TempReloadAll();
				}
			}
			this.RefreshMultipliers();
		}

		// Token: 0x06001CA0 RID: 7328 RVA: 0x000BFE0C File Offset: 0x000BE00C
		public void OnContainerContentRemove(ContainerContent content, EventTime eventTime)
		{
			if (eventTime != EventTime.OnEnd)
			{
				return;
			}
			SpellContent spellContent = content as SpellContent;
			if (spellContent != null)
			{
				if (spellContent.data != null)
				{
					this.RemoveSpell(spellContent.data.id);
					this.creature.ForceUnloadSkill(spellContent.data.id);
					this.TempUnloadSpells();
					this.TempReloadSpells();
				}
			}
			else
			{
				SkillContent skillContent = content as SkillContent;
				if (skillContent != null && skillContent.data != null)
				{
					Imbue.TempUnloadAll();
					this.TempUnloadSpells();
					this.creature.ForceUnloadSkill(skillContent.data.id);
					this.TempReloadSpells();
					Imbue.TempReloadAll();
				}
			}
			this.RefreshMultipliers();
		}

		// Token: 0x06001CA1 RID: 7329 RVA: 0x000BFEAF File Offset: 0x000BE0AF
		public float GetHandsIntensity()
		{
			return (this.casterLeft.intensity + this.casterRight.intensity) * 0.5f;
		}

		// Token: 0x06001CA2 RID: 7330 RVA: 0x000BFED0 File Offset: 0x000BE0D0
		public SpellPowerSlowTime GetPowerSlowTime()
		{
			foreach (SpellPowerData spellPowerData in this.spellPowerInstances)
			{
				if (spellPowerData is SpellPowerSlowTime)
				{
					return spellPowerData as SpellPowerSlowTime;
				}
			}
			return null;
		}

		// Token: 0x06001CA3 RID: 7331 RVA: 0x000BFF30 File Offset: 0x000BE130
		public void OnHandGrabChangeEvent()
		{
			this.RefreshMultipliers();
		}

		// Token: 0x06001CA4 RID: 7332 RVA: 0x000BFF38 File Offset: 0x000BE138
		public void OnApparelChangeEvent()
		{
			this.RefreshMultipliers();
		}

		// Token: 0x06001CA5 RID: 7333 RVA: 0x000BFF40 File Offset: 0x000BE140
		public void RefreshMultipliers()
		{
			this.itemChargeSpeedMult = 1f;
			this.focusRegenMult = 1f;
			this.maxFocusMult = 1f;
			if (!this.creature.isPlayer)
			{
				return;
			}
			this.casterLeft.chargeSpeedMult.ClearByType<ItemModuleGlove>();
			this.casterRight.chargeSpeedMult.ClearByType<ItemModuleGlove>();
			PlayerHand playerHand = this.creature.handLeft.playerHand;
			if (playerHand != null)
			{
				playerHand.link.ClearJointModifiersByType<ItemModuleGlove>();
			}
			PlayerHand playerHand2 = this.creature.handRight.playerHand;
			if (playerHand2 != null)
			{
				playerHand2.link.ClearJointModifiersByType<ItemModuleGlove>();
			}
			foreach (ItemData wardrobe in this.creature.container.GetAllWardrobe())
			{
				this.itemChargeSpeedMult += wardrobe.data.spellChargeSpeedPlayerMultiplier - 1f;
				this.focusRegenMult += wardrobe.data.focusRegenMultiplier - 1f;
				ItemModuleGlove glove;
				if (wardrobe.data.TryGetModule<ItemModuleGlove>(out glove))
				{
					this.GetCaster(glove.side).chargeSpeedMult.Add(glove, glove.chargeSpeed);
					PlayerHand playerHand3 = this.creature.GetHand(glove.side).playerHand;
					if (playerHand3 != null)
					{
						playerHand3.link.SetAllJointModifiers(glove, glove.strength);
					}
				}
			}
			Handle grabbedHandle = this.creature.handLeft.grabbedHandle;
			Item itemLeft = (grabbedHandle != null) ? grabbedHandle.item : null;
			if (itemLeft != null)
			{
				this.itemChargeSpeedMult += (this.creature.isPlayer ? itemLeft.data.spellChargeSpeedPlayerMultiplier : itemLeft.data.spellChargeSpeedNPCMultiplier) - 1f;
				this.focusRegenMult += itemLeft.data.focusRegenMultiplier - 1f;
			}
			Handle grabbedHandle2 = this.creature.handRight.grabbedHandle;
			Item itemRight = (grabbedHandle2 != null) ? grabbedHandle2.item : null;
			if (itemRight != null)
			{
				this.itemChargeSpeedMult += (this.creature.isPlayer ? itemRight.data.spellChargeSpeedPlayerMultiplier : itemRight.data.spellChargeSpeedNPCMultiplier) - 1f;
				this.focusRegenMult += itemRight.data.focusRegenMultiplier - 1f;
			}
			this.focusRegenMult += (float)this.creature.CountSkillsOfTree("Mind", false, false) * this.focusRegenPerSkill;
			this.maxFocusMult += (float)this.creature.CountSkillsOfTree("Mind", false, false) * this.maxFocusPerSkill;
			this.creature.healthModifier.Add(this, 1f + (float)this.creature.CountSkillsOfTree("Body", false, false) * this.creature.data.maxHealthPerSkill);
		}

		// Token: 0x170001CE RID: 462
		// (get) Token: 0x06001CA6 RID: 7334 RVA: 0x000C0244 File Offset: 0x000BE444
		public float ChargeSpeedMultiplier
		{
			get
			{
				return this.itemChargeSpeedMult * this.chargeSpeedMult;
			}
		}

		// Token: 0x06001CA7 RID: 7335 RVA: 0x000C0258 File Offset: 0x000BE458
		public SavedSpells UnloadSpells()
		{
			SpellCastData spellInstance = this.casterLeft.spellInstance;
			if (spellInstance != null)
			{
				this.casterLeft.UnloadSpell();
			}
			SpellCastData right = this.casterRight.spellInstance;
			if (right != null)
			{
				this.casterRight.UnloadSpell();
			}
			SpellTelekinesis tkLeft = this.casterLeft.telekinesis;
			if (tkLeft != null)
			{
				tkLeft.Unload();
				this.InvokeOnSpellUnload(tkLeft, this.casterLeft);
			}
			SpellTelekinesis tkRight = this.casterRight.telekinesis;
			if (tkRight != null)
			{
				tkRight.Unload();
				this.InvokeOnSpellUnload(tkRight, this.casterRight);
			}
			if (this.mergeData != null)
			{
				if (this.mergeInstance != null)
				{
					this.mergeInstance.Unload();
					this.InvokeOnSpellUnload(this.mergeInstance, null);
				}
				this.mergeInstance = null;
				this.mergeData = null;
				this.mergeCastLoaded = false;
			}
			return new SavedSpells(spellInstance, right, tkLeft, tkRight);
		}

		// Token: 0x06001CA8 RID: 7336 RVA: 0x000C0324 File Offset: 0x000BE524
		public void LoadSpells(SavedSpells saved)
		{
			if (this.mergeData != null)
			{
				if (this.mergeInstance != null)
				{
					this.mergeInstance.Unload();
					this.InvokeOnSpellUnload(this.mergeInstance, null);
				}
				this.mergeInstance = null;
				this.mergeData = null;
				this.mergeCastLoaded = false;
			}
			if (saved.left != null)
			{
				this.casterLeft.LoadSpell(saved.left);
			}
			if (saved.right != null)
			{
				this.casterRight.LoadSpell(saved.right);
			}
			if (saved.tkLeft != null)
			{
				saved.tkLeft.Load(this.casterLeft);
				this.InvokeOnSpellLoad(saved.tkLeft, this.casterLeft);
			}
			if (saved.tkRight != null)
			{
				saved.tkRight.Load(this.casterRight);
				this.InvokeOnSpellLoad(saved.tkRight, this.casterRight);
			}
		}

		// Token: 0x06001CA9 RID: 7337 RVA: 0x000C03F6 File Offset: 0x000BE5F6
		public void TempUnloadSpells()
		{
			this.tempSavedSpells = this.UnloadSpells();
		}

		// Token: 0x06001CAA RID: 7338 RVA: 0x000C0404 File Offset: 0x000BE604
		public void TempReloadSpells()
		{
			this.LoadSpells(this.tempSavedSpells);
		}

		// Token: 0x06001CAB RID: 7339 RVA: 0x000C0412 File Offset: 0x000BE612
		protected override void ManagedOnDisable()
		{
		}

		// Token: 0x06001CAC RID: 7340 RVA: 0x000C0414 File Offset: 0x000BE614
		public void AddSpell(SpellData spellData)
		{
			SpellPowerData data = spellData as SpellPowerData;
			if (data != null)
			{
				SpellPowerData spellPowerInstance = data.Clone();
				try
				{
					spellPowerInstance.Load(this);
				}
				catch (NullReferenceException exception)
				{
					Debug.LogError("Caught NullReferenceException while loading power " + spellPowerInstance.id + ", skipping. Exception below.");
					Debug.LogException(exception);
				}
				this.spellPowerInstances.Add(spellPowerInstance);
				return;
			}
			SpellTelekinesis telekinesis = spellData as SpellTelekinesis;
			if (telekinesis == null)
			{
				SpellData spellDataCloned = spellData.Clone() as SpellData;
				this.spells.Add(spellDataCloned);
				return;
			}
			this.casterLeft.telekinesis = telekinesis.Clone();
			this.casterRight.telekinesis = telekinesis.Clone();
			try
			{
				this.casterLeft.telekinesis.Load(this.casterLeft);
				this.casterRight.telekinesis.Load(this.casterRight);
			}
			catch (NullReferenceException exception2)
			{
				Debug.LogError("Caught NullReferenceException while loading Telekinesis, skipping. Exception below.");
				Debug.LogException(exception2);
			}
			this.InvokeOnSpellLoad(this.casterLeft.telekinesis, this.casterLeft);
			this.InvokeOnSpellLoad(this.casterRight.telekinesis, this.casterRight);
		}

		// Token: 0x06001CAD RID: 7341 RVA: 0x000C053C File Offset: 0x000BE73C
		public void RemoveSpell(string spellId = null)
		{
			if (this.casterLeft.telekinesis != null && this.casterLeft.telekinesis.id == spellId)
			{
				try
				{
					this.casterLeft.telekinesis.Unload();
				}
				catch (NullReferenceException exception)
				{
					Debug.LogError("Caught NullReferenceException while unloading left Telekinesis spell, skipping. Exception below.");
					Debug.LogException(exception);
				}
				this.casterLeft.telekinesis = null;
			}
			if (this.casterRight.telekinesis != null && this.casterRight.telekinesis.id == spellId)
			{
				try
				{
					this.casterRight.telekinesis.Unload();
				}
				catch (NullReferenceException exception2)
				{
					Debug.LogError("Caught NullReferenceException while unloading left Telekinesis spell, skipping. Exception below.");
					Debug.LogException(exception2);
				}
				this.casterRight.telekinesis = null;
			}
			for (int i = this.spells.Count - 1; i >= 0; i--)
			{
				if (spellId == null || this.spells[i].id == spellId)
				{
					if (this.mergeData != null && this.mergeData.id == spellId)
					{
						try
						{
							SpellMergeData spellMergeData = this.mergeInstance;
							if (spellMergeData != null)
							{
								spellMergeData.Unload();
							}
						}
						catch (NullReferenceException exception3)
						{
							Debug.LogError("Caught NullReferenceException while unloading merge spell " + this.mergeData.id + ", skipping. Exception below.");
							Debug.LogException(exception3);
						}
						this.mergeInstance = null;
						this.mergeData = null;
					}
					if (this.casterLeft.spellInstance != null && this.casterLeft.spellInstance.id == spellId)
					{
						this.casterLeft.UnloadSpell();
					}
					if (this.casterRight.spellInstance != null && this.casterRight.spellInstance.id == spellId)
					{
						this.casterRight.UnloadSpell();
					}
					this.spells.RemoveAt(i);
				}
			}
			for (int j = this.spellPowerInstances.Count - 1; j >= 0; j--)
			{
				if (spellId == null || !(this.spellPowerInstances[j].id != spellId))
				{
					try
					{
						this.spellPowerInstances[j].Unload();
					}
					catch (NullReferenceException exception4)
					{
						Debug.LogError("Caught NullReferenceException while unloading spell " + this.spellPowerInstances[j].id + ", skipping. Exception below.");
						Debug.LogException(exception4);
					}
					this.spellPowerInstances.RemoveAt(j);
				}
			}
		}

		// Token: 0x06001CAE RID: 7342 RVA: 0x000C07B0 File Offset: 0x000BE9B0
		public void ClearSpells()
		{
			if (this.casterLeft)
			{
				this.casterLeft.UnloadSpell();
				this.casterLeft.telekinesis = null;
			}
			if (this.casterRight)
			{
				this.casterRight.UnloadSpell();
				this.casterRight.telekinesis = null;
			}
			if (this.mergeInstance != null)
			{
				this.mergeInstance.Unload();
			}
			foreach (SpellPowerData spellPowerData in this.spellPowerInstances)
			{
				spellPowerData.Unload();
			}
			this.spellPowerInstances.Clear();
			this.spells.Clear();
		}

		// Token: 0x06001CAF RID: 7343 RVA: 0x000C0874 File Offset: 0x000BEA74
		public void UsePower(bool active)
		{
			Mana.PowerUseEvent onPowerUseEvent = this.OnPowerUseEvent;
			if (onPowerUseEvent != null)
			{
				onPowerUseEvent(active, EventTime.OnStart);
			}
			foreach (SpellPowerData spellPowerData in this.spellPowerInstances)
			{
				spellPowerData.Use(active);
			}
			Mana.PowerUseEvent onPowerUseEvent2 = this.OnPowerUseEvent;
			if (onPowerUseEvent2 == null)
			{
				return;
			}
			onPowerUseEvent2(active, EventTime.OnEnd);
		}

		// Token: 0x06001CB0 RID: 7344 RVA: 0x000C08EC File Offset: 0x000BEAEC
		public void InvokeMergeStep(SpellMergeData merge, Mana.CastStep step)
		{
			if (merge == null)
			{
				Debug.LogError("Merge step shouldn't be invoked with a null merge!");
				return;
			}
			if (this.OnMergeCastStep == null)
			{
				return;
			}
			Delegate[] invocationList = this.OnMergeCastStep.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Mana.MergeCastEvent eventDelegate = invocationList[i] as Mana.MergeCastEvent;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(merge, step);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnMergeCastStep event: {0}", e));
					}
				}
			}
		}

		// Token: 0x170001CF RID: 463
		// (get) Token: 0x06001CB1 RID: 7345 RVA: 0x000C0964 File Offset: 0x000BEB64
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update;
			}
		}

		// Token: 0x06001CB2 RID: 7346 RVA: 0x000C0967 File Offset: 0x000BEB67
		protected internal override void ManagedFixedUpdate()
		{
			if (this.initialized)
			{
				if (this.mergeActive)
				{
					this.mergeInstance.FixedUpdate();
				}
				this.casterLeft.ManaFixedUpdate();
				this.casterRight.ManaFixedUpdate();
			}
		}

		// Token: 0x06001CB3 RID: 7347 RVA: 0x000C099C File Offset: 0x000BEB9C
		protected internal override void ManagedUpdate()
		{
			if (this.initialized)
			{
				this.UpdateRegen();
				this.UpdateMerge();
				this.UpdatePowers();
				if (this.mergeActive)
				{
					this.mergeInstance.Update();
				}
				this.casterLeft.ManaUpdate();
				this.casterRight.ManaUpdate();
			}
		}

		// Token: 0x06001CB4 RID: 7348 RVA: 0x000C09EC File Offset: 0x000BEBEC
		private void UpdatePowers()
		{
			if (this.spellPowerInstances.IsNullOrEmpty())
			{
				return;
			}
			foreach (SpellPowerData spellPowerData in this.spellPowerInstances)
			{
				spellPowerData.Update();
			}
		}

		// Token: 0x06001CB5 RID: 7349 RVA: 0x000C0A4C File Offset: 0x000BEC4C
		private void UpdateRegen()
		{
			if (!this.creature.player)
			{
				return;
			}
			if (this.creature.state == Creature.State.Dead)
			{
				return;
			}
			if (Mana.infiniteFocus)
			{
				return;
			}
			if (this.focusRegen <= 0f || this.currentFocus >= this.MaxFocus)
			{
				return;
			}
			float moveMultiplier = (this.creature.player.locomotion.moveDirection.magnitude > 0f) ? 0.5f : 1f;
			this.currentFocus = Mathf.Clamp(this.currentFocus + this.focusRegen * moveMultiplier * this.focusRegenMult * Time.deltaTime, 0f, this.MaxFocus);
			if (this.focusReady)
			{
				if (this.currentFocus < this.minFocus)
				{
					this.focusReady = false;
				}
			}
			else if (this.currentFocus >= this.minFocus)
			{
				EffectData focusReadyEffect = this.creature.data.focusReadyEffect;
				if (focusReadyEffect != null)
				{
					focusReadyEffect.Spawn(base.transform, true, null, false).Play(0, false, false);
				}
				this.focusReady = true;
			}
			if (this.focusFull)
			{
				if (this.currentFocus < this.MaxFocus)
				{
					this.focusFull = false;
					return;
				}
			}
			else if (this.currentFocus >= this.MaxFocus)
			{
				EffectData focusFullEffect = this.creature.data.focusFullEffect;
				if (focusFullEffect != null)
				{
					focusFullEffect.Spawn(base.transform, true, null, false).Play(0, false, false);
				}
				this.focusFull = true;
			}
		}

		// Token: 0x06001CB6 RID: 7350 RVA: 0x000C0BC0 File Offset: 0x000BEDC0
		private void UpdateMerge()
		{
			if (this.mergeCastLoaded)
			{
				SpellCaster spellCaster = this.casterLeft;
				if (spellCaster != null && spellCaster.allowCasting && spellCaster.intensity > 0f)
				{
					spellCaster = this.casterRight;
					if (spellCaster != null && spellCaster.allowCasting && spellCaster.intensity > 0f && this.mergeInstance.CanMerge() && !this.casterLeft.grabbedFire && !this.casterRight.grabbedFire)
					{
						this.mergeHandsDistance = Vector3.Distance(this.casterLeft.magicSource.position, this.casterRight.magicSource.position);
						if (!this.mergeActive && Vector3.Angle(this.casterLeft.magicSource.forward, this.casterRight.magicSource.position - this.casterLeft.magicSource.position) < this.mergeData.handEnterAngle && Vector3.Angle(this.casterRight.magicSource.forward, this.casterLeft.magicSource.position - this.casterRight.magicSource.position) < this.mergeData.handEnterAngle && this.mergeHandsDistance < this.mergeData.handEnterDistance && this.casterLeft.isFiring && this.casterRight.isFiring)
						{
							this.mergeActive = true;
						}
						if (!this.mergeActive)
						{
							return;
						}
						this.mergePoint.position = Vector3.Lerp(this.mergePoint.position, Vector3.Lerp(this.casterLeft.magicSource.position, this.casterRight.magicSource.position, 0.5f), (this.mergeData.effectLerpFactor == 0f) ? 1f : (Time.deltaTime * this.mergeData.effectLerpFactor));
						this.mergePoint.rotation.SetLookRotation(this.mergePoint.position - this.creature.ragdoll.headPart.bone.animation.position, this.creature.transform.up);
						if (Vector3.Angle(this.casterLeft.magicSource.forward, this.casterRight.magicSource.position - this.casterLeft.magicSource.position) > this.mergeData.handExitAngle || Vector3.Angle(this.casterRight.magicSource.forward, this.casterLeft.magicSource.position - this.casterRight.magicSource.position) > this.mergeData.handExitAngle || this.mergeHandsDistance > this.mergeData.handExitDistance)
						{
							this.mergeActive = false;
							this.mergeCompleted = false;
							this.mergeInstance.Merge(false);
							return;
						}
						if (!this.mergeCompleted && Vector3.Distance(this.casterLeft.Orb.position, this.casterRight.Orb.position) < this.mergeData.handCompletedDistance)
						{
							this.mergeCompleted = true;
							this.casterLeft.Fire(false);
							this.casterRight.Fire(false);
							this.mergeInstance.Merge(true);
							this.mergeInstance.FireAxis(this.casterLeft.fireAxis, Side.Left);
							this.mergeInstance.FireAxis(this.casterLeft.fireAxis, Side.Right);
							return;
						}
						return;
					}
				}
			}
			if (this.mergeActive)
			{
				this.mergeActive = false;
				this.mergeCompleted = false;
				SpellMergeData spellMergeData = this.mergeInstance;
				if (spellMergeData == null)
				{
					return;
				}
				spellMergeData.Merge(false);
			}
		}

		// Token: 0x06001CB7 RID: 7351 RVA: 0x000C0F9C File Offset: 0x000BF19C
		public bool TryLoadMerge()
		{
			if (this.casterLeft.spellInstance != null && this.casterRight.spellInstance != null)
			{
				List<ContainerContent> contents = this.creature.container.contents;
				Func<SpellContent, SpellData, bool>[] array = new Func<SpellContent, SpellData, bool>[1];
				array[0] = ((SpellContent _, SpellData data) => data is SpellMergeData);
				foreach (SpellData spellData in contents.GetEnumerableContentCatalogDatasOfType(array))
				{
					SpellMergeData spellMergeData = (SpellMergeData)spellData;
					if ((!(spellMergeData.leftSpellId != this.casterLeft.spellInstance.id) && !(spellMergeData.rightSpellId != this.casterRight.spellInstance.id)) || (!(spellMergeData.rightSpellId != this.casterLeft.spellInstance.id) && !(spellMergeData.leftSpellId != this.casterRight.spellInstance.id)))
					{
						this.mergeData = spellMergeData;
						this.mergeInstance = (this.mergeData.Clone() as SpellMergeData);
						SpellMergeData spellMergeData2 = this.mergeInstance;
						if (spellMergeData2 != null)
						{
							spellMergeData2.Load(this);
						}
						this.InvokeOnSpellLoad(this.mergeInstance, null);
						this.mergeCastLoaded = true;
						return true;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x06001CB8 RID: 7352 RVA: 0x000C1104 File Offset: 0x000BF304
		public void UnloadMerge()
		{
			if (this.mergeInstance != null)
			{
				if (this.mergeActive)
				{
					this.mergeActive = false;
					this.mergeCompleted = false;
					this.mergeInstance.Merge(false);
				}
				this.mergeInstance.Unload();
				this.InvokeOnSpellUnload(this.mergeInstance, null);
			}
			this.mergeInstance = null;
			this.mergeCastLoaded = false;
			this.mergeData = null;
		}

		// Token: 0x06001CB9 RID: 7353 RVA: 0x000C1168 File Offset: 0x000BF368
		public void OnSpellChange()
		{
			this.UnloadMerge();
			if (this.TryLoadMerge())
			{
				return;
			}
			this.mergeCastLoaded = false;
			this.mergeData = null;
			this.mergeInstance = null;
		}

		// Token: 0x06001CBA RID: 7354 RVA: 0x000C1190 File Offset: 0x000BF390
		public bool RegenFocus(float focusToRegen)
		{
			if (Mana.infiniteFocus)
			{
				return true;
			}
			float newFocus = this.currentFocus + focusToRegen;
			this.currentFocus = Mathf.Clamp(newFocus, 0f, this.MaxFocus);
			return true;
		}

		// Token: 0x06001CBB RID: 7355 RVA: 0x000C11C8 File Offset: 0x000BF3C8
		public bool ConsumeFocus(float focusToConsume)
		{
			if (focusToConsume < 0f)
			{
				Debug.LogError("Focus to consume is negative, redirecting it to regen focus instead. Update the method call where you are trying to consume negative");
				return this.RegenFocus(-focusToConsume);
			}
			if (Mana.infiniteFocus)
			{
				return true;
			}
			float newFocus = this.currentFocus - focusToConsume * this.focusConsumptionMult;
			if (newFocus > 0f)
			{
				this.currentFocus = Mathf.Clamp(newFocus, 0f, this.MaxFocus);
				return true;
			}
			return false;
		}

		// Token: 0x06001CBC RID: 7356 RVA: 0x000C1230 File Offset: 0x000BF430
		public bool CanConsumeFocus(float focusToConsume)
		{
			return Mana.infiniteFocus || this.currentFocus - focusToConsume * this.focusConsumptionMult > 0f;
		}

		// Token: 0x06001CBD RID: 7357 RVA: 0x000C125C File Offset: 0x000BF45C
		public void CastOverlapSphere()
		{
			if (Time.time - this.overlapLastTime > this.overlapMinDelay)
			{
				this.overlapCount = Physics.OverlapSphereNonAlloc(this.creature.ragdoll.headPart.bone.animation.position, this.overlapRadius, this.overlapColliders, this.overlapMask);
				this.overlapLastTime = Time.time;
			}
		}

		// Token: 0x06001CBE RID: 7358 RVA: 0x000C12CC File Offset: 0x000BF4CC
		public bool TryGetSpell<T>(string id, out T spell) where T : SpellData
		{
			for (int i = 0; i < this.spells.Count; i++)
			{
				if (this.spells[i].id == id)
				{
					T outSpell = this.spells[i] as T;
					if (outSpell != null)
					{
						spell = outSpell;
						return true;
					}
				}
			}
			spell = default(T);
			return false;
		}

		// Token: 0x06001CBF RID: 7359 RVA: 0x000C1338 File Offset: 0x000BF538
		public void InvokeOnSpellLoad(SpellData spellInstance, SpellCaster spellCaster = null)
		{
			Mana.SpellLoadEvent onSpellLoadEvent = this.OnSpellLoadEvent;
			if (onSpellLoadEvent == null)
			{
				return;
			}
			onSpellLoadEvent(spellInstance, spellCaster);
		}

		// Token: 0x06001CC0 RID: 7360 RVA: 0x000C134C File Offset: 0x000BF54C
		public void InvokeOnSpellUnload(SpellData spellInstance, SpellCaster spellCaster = null)
		{
			Mana.SpellLoadEvent onSpellUnloadEvent = this.OnSpellUnloadEvent;
			if (onSpellUnloadEvent == null)
			{
				return;
			}
			onSpellUnloadEvent(spellInstance, spellCaster);
		}

		// Token: 0x06001CC1 RID: 7361 RVA: 0x000C1360 File Offset: 0x000BF560
		public void InvokeOnImbueLoad(SpellCastCharge spell, Imbue imbue)
		{
			Mana.ImbueLoadEvent onImbueLoadEvent = this.OnImbueLoadEvent;
			if (onImbueLoadEvent == null)
			{
				return;
			}
			onImbueLoadEvent(spell, imbue);
		}

		// Token: 0x06001CC2 RID: 7362 RVA: 0x000C1374 File Offset: 0x000BF574
		public void InvokeOnImbueUnload(SpellCastCharge spell, Imbue imbue)
		{
			Mana.ImbueLoadEvent onImbueUnloadEvent = this.OnImbueUnloadEvent;
			if (onImbueUnloadEvent == null)
			{
				return;
			}
			onImbueUnloadEvent(spell, imbue);
		}

		// Token: 0x04001B61 RID: 7009
		public float _currentFocus = 30f;

		// Token: 0x04001B62 RID: 7010
		public float _baseMaxFocus = 30f;

		// Token: 0x04001B63 RID: 7011
		public float focusRegen = 2f;

		// Token: 0x04001B64 RID: 7012
		public float focusRegenPerSkill;

		// Token: 0x04001B65 RID: 7013
		public FloatHandler focusConsumptionMult;

		// Token: 0x04001B66 RID: 7014
		public float maxFocusPerSkill = 0.05f;

		// Token: 0x04001B67 RID: 7015
		public float minFocus = 10f;

		// Token: 0x04001B68 RID: 7016
		protected bool focusReady = true;

		// Token: 0x04001B69 RID: 7017
		protected bool focusFull = true;

		// Token: 0x04001B6A RID: 7018
		[NonSerialized]
		public Creature creature;

		// Token: 0x04001B6B RID: 7019
		[NonSerialized]
		public SpellCaster casterLeft;

		// Token: 0x04001B6C RID: 7020
		[NonSerialized]
		public SpellCaster casterRight;

		// Token: 0x04001B6D RID: 7021
		public static bool infiniteFocus;

		// Token: 0x04001B6E RID: 7022
		public static bool fastCast;

		// Token: 0x04001B6F RID: 7023
		[NonSerialized]
		public List<SpellData> spells = new List<SpellData>();

		// Token: 0x04001B70 RID: 7024
		[NonSerialized]
		public List<SpellPowerData> spellPowerInstances = new List<SpellPowerData>();

		// Token: 0x04001B71 RID: 7025
		public AudioClip noManaSound;

		// Token: 0x04001B72 RID: 7026
		[NonSerialized]
		protected float itemChargeSpeedMult = 1f;

		// Token: 0x04001B73 RID: 7027
		[NonSerialized]
		public float focusRegenMult = 1f;

		// Token: 0x04001B74 RID: 7028
		[NonSerialized]
		public float _maxFocusMult = 1f;

		// Token: 0x04001B75 RID: 7029
		[NonSerialized]
		public Transform mergePoint;

		// Token: 0x04001B76 RID: 7030
		public bool mergeActive;

		// Token: 0x04001B77 RID: 7031
		public bool mergeCompleted;

		// Token: 0x04001B78 RID: 7032
		public float mergeHandsDistance;

		// Token: 0x04001B79 RID: 7033
		[NonSerialized]
		public bool mergeCastLoaded;

		// Token: 0x04001B7A RID: 7034
		[NonSerialized]
		public SpellMergeData mergeInstance;

		// Token: 0x04001B7B RID: 7035
		[NonSerialized]
		public SpellMergeData mergeData;

		// Token: 0x04001B7C RID: 7036
		public float overlapRadius = 5f;

		// Token: 0x04001B7D RID: 7037
		public float overlapMinDelay = 0.5f;

		// Token: 0x04001B7E RID: 7038
		public LayerMask overlapMask;

		// Token: 0x04001B7F RID: 7039
		public int overlapCount;

		// Token: 0x04001B80 RID: 7040
		public Collider[] overlapColliders;

		// Token: 0x04001B81 RID: 7041
		protected float overlapLastTime;

		// Token: 0x04001B82 RID: 7042
		protected bool initialized;

		// Token: 0x04001B83 RID: 7043
		public FloatHandler chargeSpeedMult;

		// Token: 0x04001B8A RID: 7050
		public SavedSpells tempSavedSpells;

		// Token: 0x020008EB RID: 2283
		// (Invoke) Token: 0x060041DC RID: 16860
		public delegate void FocusChangeEvent(float focus, float maxFocus);

		// Token: 0x020008EC RID: 2284
		// (Invoke) Token: 0x060041E0 RID: 16864
		public delegate void SpellLoadEvent(SpellData spellInstance, SpellCaster caster);

		// Token: 0x020008ED RID: 2285
		// (Invoke) Token: 0x060041E4 RID: 16868
		public delegate void ImbueLoadEvent(SpellCastCharge spellData, Imbue imbue);

		// Token: 0x020008EE RID: 2286
		// (Invoke) Token: 0x060041E8 RID: 16872
		public delegate void PowerUseEvent(bool active, EventTime eventTime);

		// Token: 0x020008EF RID: 2287
		// (Invoke) Token: 0x060041EC RID: 16876
		public delegate void MergeCastEvent(SpellMergeData merge, Mana.CastStep step);

		// Token: 0x020008F0 RID: 2288
		public enum CastStep
		{
			// Token: 0x0400431E RID: 17182
			MergeStart,
			// Token: 0x0400431F RID: 17183
			MergeCharged,
			// Token: 0x04004320 RID: 17184
			MergeFireStart,
			// Token: 0x04004321 RID: 17185
			MergeFireEnd,
			// Token: 0x04004322 RID: 17186
			MergeStop
		}
	}
}
