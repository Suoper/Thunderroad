using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000280 RID: 640
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/SpellCaster")]
	[AddComponentMenu("ThunderRoad/Creatures/Spell caster")]
	public class SpellCaster : MonoBehaviour
	{
		// Token: 0x06001E17 RID: 7703 RVA: 0x000CC6C4 File Offset: 0x000CA8C4
		public void DisallowCasting(object handler)
		{
			this.disallowCasting.Add(handler);
			this.allowCasting = !this.disallowCasting;
			if (this.isFiring)
			{
				this.Fire(false);
			}
		}

		// Token: 0x06001E18 RID: 7704 RVA: 0x000CC6F5 File Offset: 0x000CA8F5
		public void AllowCasting(object handler)
		{
			this.disallowCasting.Remove(handler);
			this.allowCasting = !this.disallowCasting;
		}

		// Token: 0x06001E19 RID: 7705 RVA: 0x000CC718 File Offset: 0x000CA918
		public void ClearDisallowCasting()
		{
			this.disallowCasting.Clear();
			this.allowCasting = true;
		}

		// Token: 0x170001E2 RID: 482
		// (get) Token: 0x06001E1A RID: 7706 RVA: 0x000CC72D File Offset: 0x000CA92D
		// (set) Token: 0x06001E1B RID: 7707 RVA: 0x000CC735 File Offset: 0x000CA935
		public bool allowSpellWheel { get; protected set; } = true;

		// Token: 0x170001E3 RID: 483
		// (get) Token: 0x06001E1C RID: 7708 RVA: 0x000CC740 File Offset: 0x000CA940
		public Transform Orb
		{
			get
			{
				if (this.orb == null)
				{
					this.orb = new GameObject("Orb" + this.ragdollHand.side.ToString()).transform;
				}
				return this.orb;
			}
		}

		// Token: 0x140000E8 RID: 232
		// (add) Token: 0x06001E1D RID: 7709 RVA: 0x000CC794 File Offset: 0x000CA994
		// (remove) Token: 0x06001E1E RID: 7710 RVA: 0x000CC7CC File Offset: 0x000CA9CC
		public event SpellCaster.TriggerImbueEvent OnTriggerImbueEvent;

		// Token: 0x140000E9 RID: 233
		// (add) Token: 0x06001E1F RID: 7711 RVA: 0x000CC804 File Offset: 0x000CAA04
		// (remove) Token: 0x06001E20 RID: 7712 RVA: 0x000CC83C File Offset: 0x000CAA3C
		public event SpellCaster.CastEvent OnSpellCastStep;

		// Token: 0x06001E21 RID: 7713 RVA: 0x000CC871 File Offset: 0x000CAA71
		private void OnEnable()
		{
			PlayerControl.GetHand(this.side).OnButtonPressEvent += this.OnControllerButtonPress;
		}

		// Token: 0x06001E22 RID: 7714 RVA: 0x000CC88F File Offset: 0x000CAA8F
		private void OnDisable()
		{
			PlayerControl.GetHand(this.side).OnButtonPressEvent -= this.OnControllerButtonPress;
		}

		// Token: 0x06001E23 RID: 7715 RVA: 0x000CC8B0 File Offset: 0x000CAAB0
		private void Awake()
		{
			this.disallowCasting = new BoolHandler(false);
			this.disallowSpellWheel = new BoolHandler(false);
			this.chargeSpeedMult = new FloatHandler();
			this.mana = base.GetComponentInParent<Mana>();
			if (!this.mana)
			{
				Debug.LogError("Spellcaster can't work without a mana component on the creature");
				base.gameObject.SetActive(false);
				return;
			}
			this.ragdollHand = base.GetComponentInParent<RagdollHand>();
			this.ragdollHand.OnGrabEvent += this.OnHandGrab;
			this.ragdollHand.OnUnGrabEvent += this.OnHandUnGrab;
			this.side = this.ragdollHand.side;
			foreach (SpellCaster spellCaster in this.mana.GetComponentsInChildren<SpellCaster>())
			{
				if (spellCaster != this)
				{
					this.other = spellCaster;
					break;
				}
			}
			this.Orb.SetParent(this.magicSource);
			this.imbueTrigger = new GameObject("ImbueTrigger").AddComponent<Trigger>();
			Transform transform = this.imbueTrigger.transform;
			transform.SetParent(this.magicSource);
			transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
			this.imbueTrigger.SetCallback(new Trigger.CallBack(this.OnTriggerImbue));
			this.imbueTrigger.SetLayer(GameManager.GetLayer(LayerName.DroppedItem));
		}

		// Token: 0x06001E24 RID: 7716 RVA: 0x000CCA00 File Offset: 0x000CAC00
		protected void OnControllerButtonPress(PlayerControl.Hand.Button button, bool pressed)
		{
			if (this.isFiring && button == PlayerControl.Hand.Button.Grip && pressed)
			{
				SpellCastCharge spellCastCharge = this.spellInstance as SpellCastCharge;
				if (spellCastCharge != null && spellCastCharge.endOnGrip)
				{
					spellCastCharge.InvokeOnGripEndEvent();
					this.Fire(false);
				}
			}
		}

		// Token: 0x06001E25 RID: 7717 RVA: 0x000CCA44 File Offset: 0x000CAC44
		public void OnHandGrab(Side side, Handle handle, float axisPosition, HandlePose orientation, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart && !handle.data.allowSpellFire)
			{
				this.DisallowCasting(handle);
			}
			if (eventTime == EventTime.OnEnd)
			{
				this.mana.OnHandGrabChangeEvent();
			}
		}

		// Token: 0x06001E26 RID: 7718 RVA: 0x000CCA6E File Offset: 0x000CAC6E
		public void OnHandUnGrab(Side side, Handle handle, bool throwing, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				this.AllowCasting(handle);
			}
			if (eventTime == EventTime.OnEnd)
			{
				this.mana.OnHandGrabChangeEvent();
			}
		}

		// Token: 0x06001E27 RID: 7719 RVA: 0x000CCA8C File Offset: 0x000CAC8C
		public void InvokeCastStep(SpellCastData spell, SpellCaster.CastStep step)
		{
			if (spell == null)
			{
				Debug.LogError("Cast step shouldn't be invoked with a null spell!");
				return;
			}
			if (this.OnSpellCastStep == null)
			{
				return;
			}
			Delegate[] invocationList = this.OnSpellCastStep.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				SpellCaster.CastEvent eventDelegate = invocationList[i] as SpellCaster.CastEvent;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(spell, step);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnSpellCastStep event: {0}", e));
					}
				}
			}
		}

		// Token: 0x06001E28 RID: 7720 RVA: 0x000CCB04 File Offset: 0x000CAD04
		public void SpawnFingersEffect(EffectData effectData, bool play = false, float intensity = 1f, Transform target = null)
		{
			SpellCaster.<>c__DisplayClass56_0 CS$<>8__locals1;
			CS$<>8__locals1.effectData = effectData;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.target = target;
			CS$<>8__locals1.intensity = intensity;
			CS$<>8__locals1.play = play;
			if (CS$<>8__locals1.effectData == null)
			{
				return;
			}
			this.StopFingersEffect();
			this.<SpawnFingersEffect>g__SpawnFingerEffect|56_0(this.ragdollHand.fingerThumb.tip, ref CS$<>8__locals1);
			this.<SpawnFingersEffect>g__SpawnFingerEffect|56_0(this.ragdollHand.fingerIndex.tip, ref CS$<>8__locals1);
			this.<SpawnFingersEffect>g__SpawnFingerEffect|56_0(this.ragdollHand.fingerMiddle.tip, ref CS$<>8__locals1);
			this.<SpawnFingersEffect>g__SpawnFingerEffect|56_0(this.ragdollHand.fingerRing.tip, ref CS$<>8__locals1);
			this.<SpawnFingersEffect>g__SpawnFingerEffect|56_0(this.ragdollHand.fingerLittle.tip, ref CS$<>8__locals1);
		}

		// Token: 0x06001E29 RID: 7721 RVA: 0x000CCBC4 File Offset: 0x000CADC4
		public void SetFingersEffect(float intensity)
		{
			int count = this.fingerEffectInstances.Count;
			for (int i = 0; i < count; i++)
			{
				this.fingerEffectInstances[i].SetIntensity(intensity);
			}
		}

		// Token: 0x06001E2A RID: 7722 RVA: 0x000CCBFC File Offset: 0x000CADFC
		public void StopFingersEffect()
		{
			int count = this.fingerEffectInstances.Count;
			for (int i = 0; i < count; i++)
			{
				this.fingerEffectInstances[i].End(false, -1f);
			}
			this.fingerEffectInstances.Clear();
		}

		// Token: 0x06001E2B RID: 7723 RVA: 0x000CCC43 File Offset: 0x000CAE43
		public void DisableSpellWheel(object handler)
		{
			this.disallowSpellWheel.Add(handler);
			this.allowSpellWheel = !this.disallowSpellWheel;
		}

		// Token: 0x06001E2C RID: 7724 RVA: 0x000CCC65 File Offset: 0x000CAE65
		public void AllowSpellWheel(object handler)
		{
			this.disallowSpellWheel.Remove(handler);
			this.allowSpellWheel = !this.disallowSpellWheel;
		}

		// Token: 0x06001E2D RID: 7725 RVA: 0x000CCC88 File Offset: 0x000CAE88
		public void SetMagicOffset(Vector3 offset, bool switchForHands = false)
		{
			if (switchForHands && this.ragdollHand.side == Side.Left)
			{
				this.magicOffset = new Vector3(-offset.x, offset.y, offset.z);
				return;
			}
			this.magicOffset = offset;
		}

		// Token: 0x06001E2E RID: 7726 RVA: 0x000CCCC4 File Offset: 0x000CAEC4
		public void LoadSpell(SpellCastData spellCastData)
		{
			SpellCastData spellInstanceToLoad = spellCastData.Clone();
			this.UnloadSpell();
			if (spellInstanceToLoad != null)
			{
				this.spellInstance = spellInstanceToLoad;
				try
				{
					this.spellInstance.Load(this);
				}
				catch (NullReferenceException exception)
				{
					Debug.LogError("Caught NullReferenceException while loading spell " + this.spellInstance.id + ", skipping. Exception below.");
					Debug.LogException(exception);
				}
				float radius = 0f;
				SpellCastCharge spellCastCharge = this.spellInstance as SpellCastCharge;
				if (spellCastCharge != null && spellCastCharge.imbueEnabled)
				{
					radius = spellCastCharge.imbueRadius;
				}
				this.imbueTrigger.SetRadius(radius);
			}
			this.RefreshWater();
			this.mana.InvokeOnSpellLoad(this.spellInstance, this);
			this.mana.OnSpellChange();
		}

		// Token: 0x06001E2F RID: 7727 RVA: 0x000CCD80 File Offset: 0x000CAF80
		public void UnloadSpell()
		{
			if (this.spellInstance != null)
			{
				try
				{
					this.spellInstance.Unload();
				}
				catch (NullReferenceException exception)
				{
					Debug.LogError("Caught NullReferenceException while unloading spell " + this.spellInstance.id + ", skipping. Exception below.");
					Debug.LogException(exception);
				}
			}
			this.Fire(false);
			this.mana.InvokeOnSpellUnload(this.spellInstance, this);
			this.mana.UnloadMerge();
			this.spellInstance = null;
		}

		// Token: 0x06001E30 RID: 7728 RVA: 0x000CCE04 File Offset: 0x000CB004
		public void RefreshWater()
		{
			SpellCastCharge spell = this.spellInstance as SpellCastCharge;
			if (spell != null)
			{
				if (this.ragdollHand.waterHandler.inWater)
				{
					if (!spell.allowUnderwater)
					{
						this.DisallowCasting(this.ragdollHand.waterHandler);
						return;
					}
				}
				else
				{
					this.AllowCasting(this.ragdollHand.waterHandler);
				}
			}
		}

		// Token: 0x06001E31 RID: 7729 RVA: 0x000CCE60 File Offset: 0x000CB060
		protected void OnTriggerImbue(Collider other, bool enter)
		{
			SpellCaster.TriggerImbueEvent onTriggerImbueEvent = this.OnTriggerImbueEvent;
			if (onTriggerImbueEvent != null)
			{
				onTriggerImbueEvent(other, enter);
			}
			ColliderGroup componentInParent = other.GetComponentInParent<ColliderGroup>();
			ColliderGroup colliderGroup = (componentInParent != null) ? componentInParent.RootGroup : null;
			if (!colliderGroup)
			{
				return;
			}
			if (!enter)
			{
				for (int i = this.imbueObjects.Count - 1; i >= 0; i--)
				{
					SpellCaster.ImbueObject imbueObject = this.imbueObjects[i];
					if (!(imbueObject.colliderGroup != colliderGroup))
					{
						EffectInstance effectInstance = imbueObject.effectInstance;
						if (effectInstance != null)
						{
							effectInstance.End(false, -1f);
						}
						this.imbueObjects.RemoveAt(i);
					}
				}
				return;
			}
			if (colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.None)
			{
				return;
			}
			SpellCastCharge spellCastCharge = this.spellInstance as SpellCastCharge;
			if (spellCastCharge != null && !spellCastCharge.imbueAllowMetal && colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Metal)
			{
				return;
			}
			if (!string.IsNullOrEmpty(colliderGroup.imbueCustomSpellID) && !string.Equals(this.spellInstance.id, colliderGroup.imbueCustomSpellID, StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			if (colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Custom && (colliderGroup.imbueCustomSpellID == null || !string.Equals(this.spellInstance.id, colliderGroup.imbueCustomSpellID, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}
			for (int j = this.imbueObjects.Count - 1; j >= 0; j--)
			{
				if (this.imbueObjects[j].colliderGroup == colliderGroup)
				{
					return;
				}
			}
			colliderGroup.gameObject.GetComponentInParent<Item>();
			this.imbueObjects.Add(new SpellCaster.ImbueObject(colliderGroup));
		}

		// Token: 0x06001E32 RID: 7730 RVA: 0x000CCFD8 File Offset: 0x000CB1D8
		public void Fire(bool active)
		{
			if (active && !this.allowCasting)
			{
				return;
			}
			bool hasGrabbedHandle = this.ragdollHand.grabbedHandle;
			if (hasGrabbedHandle)
			{
				this.grabbedFire = false;
				if (!this.ragdollHand.grabbedHandle.data.allowSpellFire)
				{
					return;
				}
			}
			else if (this.mana.creature.player && this.mana.creature.player.isLocal && active && PlayerControl.GetHand(this.ragdollHand.side).gripPressed)
			{
				return;
			}
			if (this.isFiring == active || this.spellInstance == null)
			{
				return;
			}
			this.InvokeCastStep(this.spellInstance, active ? SpellCaster.CastStep.CastStart : SpellCaster.CastStep.CastStop);
			this.isFiring = active;
			if (this.isFiring)
			{
				if (hasGrabbedHandle)
				{
					this.ragdollHand.grabbedHandle.UnGrabbed += this.OnUngrabFire;
				}
				this.grabbedFire = hasGrabbedHandle;
				this.Orb.SetParent(this.mana.transform);
				this.Orb.SetPositionAndRotation(this.magicSource.position, this.magicSource.rotation);
				this.Orb.transform.localScale = Vector3.one;
				this.fireTime = Time.time;
				this.magicOffset = Vector3.zero;
			}
			else
			{
				if (hasGrabbedHandle)
				{
					this.ragdollHand.grabbedHandle.UnGrabbed -= this.OnUngrabFire;
				}
				this.grabbedFire = false;
				Handle.GripInfo gripInfo = this.ragdollHand.gripInfo;
				Transform target = (gripInfo != null) ? gripInfo.SpellOrbTarget : null;
				if (target != null)
				{
					this.Orb.SetParent(target);
					this.Orb.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				}
				else
				{
					Transform transform = this.Orb;
					Handle handle = this.ragdollHand.grabbedHandle;
					transform.SetParent((handle != null && handle.data.allowSpellFire && handle.data.offsetInHandleSpace) ? this.ragdollHand.grabbedHandle.transform : (this.ragdollHand.playerHand ? this.magicSource : this.mana.transform));
					Transform transform2 = this.Orb;
					Handle grabbedHandle = this.ragdollHand.grabbedHandle;
					bool flag;
					if (grabbedHandle == null)
					{
						flag = false;
					}
					else
					{
						HandleData data = grabbedHandle.data;
						bool? flag2 = (data != null) ? new bool?(data.offsetInHandleSpace) : null;
						bool flag3 = true;
						flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
					}
					transform2.SetPositionAndRotation(flag ? this.ragdollHand.grabbedHandle.transform.TransformPoint(this.magicOffset) : this.magicSource.TransformPoint(this.magicOffset), this.magicSource.rotation);
				}
				this.Orb.transform.localScale = Vector3.one;
				for (int i = this.imbueObjects.Count - 1; i >= 0; i--)
				{
					EffectInstance effectInstance = this.imbueObjects[i].effectInstance;
					if (effectInstance != null)
					{
						effectInstance.End(false, -1f);
					}
				}
				this.imbueObjects.Clear();
				this.intensity = 0f;
			}
			this.imbueTrigger.SetActive(this.isFiring);
			EventManager.InvokeSpellUsed(this.spellInstance.id, this.ragdollHand.creature, this.side);
			this.spellInstance.Fire(active);
		}

		// Token: 0x06001E33 RID: 7731 RVA: 0x000CD33C File Offset: 0x000CB53C
		public void FireAxis(float value)
		{
			this.fireAxis = value;
			if (this.isFiring)
			{
				this.spellInstance.FireAxis(value);
			}
			if (this.mana.mergeActive)
			{
				this.mana.mergeInstance.FireAxis(value, this.ragdollHand.side);
			}
		}

		// Token: 0x06001E34 RID: 7732 RVA: 0x000CD38D File Offset: 0x000CB58D
		private void OnUngrabFire(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			this.Orb.SetParent(this.mana.transform);
			if (eventTime == EventTime.OnEnd)
			{
				this.Fire(false);
				handle.UnGrabbed -= this.OnUngrabFire;
			}
		}

		// Token: 0x06001E35 RID: 7733 RVA: 0x000CD3C2 File Offset: 0x000CB5C2
		public void ManaFixedUpdate()
		{
			if (this.isFiring)
			{
				this.spellInstance.FixedUpdateCaster();
			}
		}

		// Token: 0x06001E36 RID: 7734 RVA: 0x000CD3D8 File Offset: 0x000CB5D8
		public void ManaUpdate()
		{
			SpellTelekinesis spellTelekinesis = this.telekinesis;
			if (spellTelekinesis != null)
			{
				spellTelekinesis.Update();
			}
			if (this.isFiring)
			{
				this.UpdateManaPosition();
				this.UpdateImbueEffects();
			}
			else if (this.mana.mergeActive)
			{
				this.Orb.position = Vector3.MoveTowards(this.Orb.position, this.mana.mergePoint.position, this.magicFollowSpeed * Time.deltaTime);
			}
			SpellCastData spellCastData = this.spellInstance;
			if (spellCastData == null)
			{
				return;
			}
			spellCastData.UpdateCaster();
		}

		// Token: 0x06001E37 RID: 7735 RVA: 0x000CD460 File Offset: 0x000CB660
		private void UpdateManaPosition()
		{
			Vector3 position;
			Quaternion rotation;
			if (this.mana.mergeActive)
			{
				position = Vector3.Lerp(this.Orb.position, this.mana.mergePoint.position, this.mergeAttractSpeed * Time.deltaTime);
				rotation = Quaternion.LookRotation(this.mana.creature.transform.forward);
			}
			else
			{
				Handle.GripInfo gripInfo = this.ragdollHand.gripInfo;
				Transform target = (gripInfo != null) ? gripInfo.SpellOrbTarget : null;
				if (target != null)
				{
					position = target.position;
					rotation = target.rotation;
				}
				else
				{
					Vector3 position2 = this.Orb.position;
					Handle grabbedHandle = this.ragdollHand.grabbedHandle;
					bool flag;
					if (grabbedHandle == null)
					{
						flag = false;
					}
					else
					{
						HandleData data = grabbedHandle.data;
						bool? flag2 = (data != null) ? new bool?(data.offsetInHandleSpace) : null;
						bool flag3 = true;
						flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
					}
					position = Vector3.MoveTowards(position2, flag ? this.ragdollHand.grabbedHandle.transform.TransformPoint(this.magicOffset) : this.magicSource.TransformPoint(this.magicOffset), this.magicFollowSpeed * Time.deltaTime);
					rotation = this.magicSource.rotation;
				}
			}
			this.Orb.SetPositionAndRotation(position, rotation);
		}

		// Token: 0x170001E4 RID: 484
		// (get) Token: 0x06001E38 RID: 7736 RVA: 0x000CD5A4 File Offset: 0x000CB7A4
		public float ChargeRatio
		{
			get
			{
				SpellCastCharge spell = this.spellInstance as SpellCastCharge;
				if (spell == null)
				{
					return 0f;
				}
				if (!this.grabbedFire)
				{
					return spell.currentCharge;
				}
				return spell.currentCharge / ((spell.grabbedFireMaxCharge == 0f) ? 1f : spell.grabbedFireMaxCharge);
			}
		}

		// Token: 0x06001E39 RID: 7737 RVA: 0x000CD5F8 File Offset: 0x000CB7F8
		private void UpdateImbueEffects()
		{
			SpellCastCharge spellCastCharge = this.spellInstance as SpellCastCharge;
			if (spellCastCharge == null || !spellCastCharge.imbueEnabled)
			{
				return;
			}
			int imbueObjectsCount = this.imbueObjects.Count;
			Transform creatureTransform = this.mana.creature.transform;
			for (int i = 0; i < imbueObjectsCount; i++)
			{
				SpellCaster.ImbueObject imbueObject = this.imbueObjects[i];
				imbueObject.colliderGroup.imbue.Transfer(spellCastCharge, spellCastCharge.imbueRate * this.ChargeRatio / (float)imbueObjectsCount * Time.unscaledDeltaTime, null);
				if (imbueObject.effectInstance == null && imbueObject.colliderGroup.parentGroup == null)
				{
					EffectInstance effectInstance = spellCastCharge.imbueTransferEffectData.Spawn(creatureTransform.position, creatureTransform.rotation, creatureTransform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					Renderer effectRenderer = imbueObject.colliderGroup.imbueEffectRenderer ? imbueObject.colliderGroup.imbueEffectRenderer : imbueObject.colliderGroup.imbueEmissionRenderer;
					if (effectRenderer)
					{
						effectInstance.SetSource(this.Orb.transform);
						effectInstance.SetRenderer(effectRenderer, false);
						effectInstance.SetTarget(effectRenderer.transform);
						effectInstance.Play(0, false, false);
					}
					else
					{
						Debug.LogError("Can't play imbue effect, there is no imbueEffectRenderer and imbueEmissionRenderer on the prefab");
					}
					imbueObject.effectInstance = effectInstance;
				}
				float newIntensity = this.imbueTransferMinIntensity;
				if (imbueObject.colliderGroup.imbue.energy != imbueObject.colliderGroup.imbue.maxEnergy)
				{
					newIntensity = this.ChargeRatio / (float)imbueObjectsCount;
				}
				EffectInstance effectInstance2 = imbueObject.effectInstance;
				if (effectInstance2 != null)
				{
					effectInstance2.SetIntensity(newIntensity);
				}
			}
		}

		// Token: 0x06001E3A RID: 7738 RVA: 0x000CD7A0 File Offset: 0x000CB9A0
		public Vector3 GetShootDirection()
		{
			return this.magicSource.transform.TransformDirection(Vector3.forward);
		}

		// Token: 0x06001E3B RID: 7739 RVA: 0x000CD7B7 File Offset: 0x000CB9B7
		private void OnDrawGizmos()
		{
			SpellCastData spellCastData = this.spellInstance;
			if (spellCastData != null)
			{
				spellCastData.DrawGizmos();
			}
			SpellTelekinesis spellTelekinesis = this.telekinesis;
			if (spellTelekinesis == null)
			{
				return;
			}
			spellTelekinesis.DrawGizmos();
		}

		// Token: 0x06001E3C RID: 7740 RVA: 0x000CD7DA File Offset: 0x000CB9DA
		private void OnDrawGizmosSelected()
		{
			SpellCastData spellCastData = this.spellInstance;
			if (spellCastData != null)
			{
				spellCastData.DrawGizmosSelected();
			}
			SpellTelekinesis spellTelekinesis = this.telekinesis;
			if (spellTelekinesis == null)
			{
				return;
			}
			spellTelekinesis.DrawGizmosSelected();
		}

		// Token: 0x06001E3F RID: 7743 RVA: 0x000CD864 File Offset: 0x000CBA64
		[CompilerGenerated]
		private void <SpawnFingersEffect>g__SpawnFingerEffect|56_0(Transform tip, ref SpellCaster.<>c__DisplayClass56_0 A_2)
		{
			EffectInstance tipEffectInstance = A_2.effectData.Spawn(tip.position, tip.rotation, tip, null, true, null, false, 1f, 1f, Array.Empty<Type>());
			this.fingerEffectInstances.Add(tipEffectInstance);
			tipEffectInstance.SetSource(tip);
			tipEffectInstance.SetTarget(A_2.target);
			tipEffectInstance.SetIntensity(A_2.intensity);
			if (A_2.play)
			{
				tipEffectInstance.Play(0, false, false);
			}
		}

		// Token: 0x04001C8B RID: 7307
		public Transform fire;

		// Token: 0x04001C8C RID: 7308
		public Transform magicSource;

		// Token: 0x04001C8D RID: 7309
		public Transform rayDir;

		// Token: 0x04001C8E RID: 7310
		public FloatHandler chargeSpeedMult;

		// Token: 0x04001C8F RID: 7311
		public static float throwMinHandVelocity = 1f;

		// Token: 0x04001C90 RID: 7312
		[NonSerialized]
		public bool allowCasting = true;

		// Token: 0x04001C91 RID: 7313
		public BoolHandler disallowCasting;

		// Token: 0x04001C92 RID: 7314
		public BoolHandler disallowSpellWheel;

		// Token: 0x04001C94 RID: 7316
		[NonSerialized]
		private Transform orb;

		// Token: 0x04001C95 RID: 7317
		[NonSerialized]
		public float intensity;

		// Token: 0x04001C96 RID: 7318
		[NonSerialized]
		public Vector3 magicOffset;

		// Token: 0x04001C97 RID: 7319
		[NonSerialized]
		public bool parryable;

		// Token: 0x04001C98 RID: 7320
		[NonSerialized]
		public bool isFiring;

		// Token: 0x04001C99 RID: 7321
		[NonSerialized]
		public bool isMerging;

		// Token: 0x04001C9A RID: 7322
		[NonSerialized]
		public bool grabbedFire;

		// Token: 0x04001C9B RID: 7323
		[NonSerialized]
		public bool isSpraying;

		// Token: 0x04001C9C RID: 7324
		[NonSerialized]
		public float fireAxis;

		// Token: 0x04001C9D RID: 7325
		[NonSerialized]
		public List<EffectInstance> fingerEffectInstances = new List<EffectInstance>();

		// Token: 0x04001C9E RID: 7326
		[NonSerialized]
		public float fireTime;

		// Token: 0x04001C9F RID: 7327
		[NonSerialized]
		public SpellTelekinesis telekinesis;

		// Token: 0x04001CA0 RID: 7328
		[NonSerialized]
		public Mana mana;

		// Token: 0x04001CA1 RID: 7329
		[NonSerialized]
		public RagdollHand ragdollHand;

		// Token: 0x04001CA2 RID: 7330
		[NonSerialized]
		public Side side;

		// Token: 0x04001CA3 RID: 7331
		[NonSerialized]
		public SpellCaster other;

		// Token: 0x04001CA4 RID: 7332
		[NonSerialized]
		public SpellCastData spellInstance;

		// Token: 0x04001CA5 RID: 7333
		protected float magicFollowSpeed = 50f;

		// Token: 0x04001CA6 RID: 7334
		protected float mergeAttractSpeed = 10f;

		// Token: 0x04001CA7 RID: 7335
		protected float imbueTransferMinIntensity = 0.1f;

		// Token: 0x04001CA8 RID: 7336
		public Trigger imbueTrigger;

		// Token: 0x04001CAB RID: 7339
		[NonSerialized]
		public List<SpellCaster.ImbueObject> imbueObjects = new List<SpellCaster.ImbueObject>();

		// Token: 0x02000925 RID: 2341
		// (Invoke) Token: 0x060042A1 RID: 17057
		public delegate void TriggerImbueEvent(Collider other, bool enter);

		// Token: 0x02000926 RID: 2342
		// (Invoke) Token: 0x060042A5 RID: 17061
		public delegate void CastEvent(SpellCastData spell, SpellCaster.CastStep step);

		// Token: 0x02000927 RID: 2343
		public enum CastStep
		{
			// Token: 0x040043C4 RID: 17348
			CastStart,
			// Token: 0x040043C5 RID: 17349
			ChargeStart,
			// Token: 0x040043C6 RID: 17350
			ChargeSprayable,
			// Token: 0x040043C7 RID: 17351
			ChargeThrowable,
			// Token: 0x040043C8 RID: 17352
			ChargeThrow,
			// Token: 0x040043C9 RID: 17353
			SprayStart,
			// Token: 0x040043CA RID: 17354
			SprayEnd,
			// Token: 0x040043CB RID: 17355
			ChargeStop,
			// Token: 0x040043CC RID: 17356
			CastStop,
			// Token: 0x040043CD RID: 17357
			MergeStart,
			// Token: 0x040043CE RID: 17358
			MergeCharged,
			// Token: 0x040043CF RID: 17359
			MergeFireStart,
			// Token: 0x040043D0 RID: 17360
			MergeFireEnd,
			// Token: 0x040043D1 RID: 17361
			MergeStop
		}

		// Token: 0x02000928 RID: 2344
		public class ImbueObject
		{
			// Token: 0x060042A8 RID: 17064 RVA: 0x0018D949 File Offset: 0x0018BB49
			public ImbueObject(ColliderGroup colliderGroup)
			{
				this.colliderGroup = colliderGroup;
			}

			// Token: 0x040043D2 RID: 17362
			public ColliderGroup colliderGroup;

			// Token: 0x040043D3 RID: 17363
			public EffectInstance effectInstance;
		}
	}
}
