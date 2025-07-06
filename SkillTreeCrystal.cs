using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using ThunderRoad.Pools;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.VFX;

namespace ThunderRoad
{
	// Token: 0x020002B6 RID: 694
	public class SkillTreeCrystal : ThunderBehaviour
	{
		// Token: 0x17000215 RID: 533
		// (get) Token: 0x0600219B RID: 8603 RVA: 0x000E70F6 File Offset: 0x000E52F6
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update;
			}
		}

		// Token: 0x0600219C RID: 8604 RVA: 0x000E70FC File Offset: 0x000E52FC
		private void Awake()
		{
			if (base.TryGetComponent<Item>(out this.item))
			{
				this.item.OnHeldActionEvent += this.HeldAction;
				this.item.OnDespawnEvent += this.CrystalDespawned;
				this.item.OnUngrabEvent += this.OnUnGrabEvent;
			}
			else
			{
				Debug.LogError("SkillTreeCrystal: [" + base.name + "] - No Item component found");
			}
			if (Common.IsWindows)
			{
				this.mergeVfx = this.mergeVfxWindows;
				this.mergeVfxTarget = this.mergeVfxTargetWindows;
				this.linkVfx = this.linkVfxWindows;
				this.linkVfxTarget = this.linkVfxTargetWindows;
			}
			else
			{
				this.mergeVfx = this.mergeVfxAndroid;
				this.mergeVfxTarget = this.mergeVfxTargetAndroid;
				this.linkVfx = this.linkVfxAndroid;
				this.linkVfxTarget = this.linkVfxTargetAndroid;
			}
			this.mergePoint = new GameObject("MergePoint").transform;
			this.mergePoint.SetParent(base.transform);
			MeshRenderer[] renderers = base.GetComponentsInChildren<MeshRenderer>(true);
			this.materialInstances = new MaterialInstance[renderers.Length];
			for (int i = 0; i < renderers.Length; i++)
			{
				this.materialInstances[i] = renderers[i].GetOrAddComponent<MaterialInstance>();
				this.materialInstances[i].AcquireMaterials();
			}
			this.UpdateVertexLerp(0f, default(Vector3), false);
		}

		/// Stop effects (and prevent merge in the early stages) on ungrab
		// Token: 0x0600219D RID: 8605 RVA: 0x000E7260 File Offset: 0x000E5460
		private void OnUnGrabEvent(Handle handle, RagdollHand ragdollHand, bool throwing)
		{
			this.SetGlow(false);
			if (this.hand != null || !this.mergingCrystal)
			{
				return;
			}
			this.DisconnectMergeVFX();
			this.mergingCrystal.DisconnectMergeVFX();
			this.mergingCrystal.mergingCrystal = null;
			this.mergingCrystal = null;
			EffectInstance effectInstance = this.mergeBeginEffect;
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			this.mergeBeginEffect = null;
		}

		/// Tidy up after ourselves
		// Token: 0x0600219E RID: 8606 RVA: 0x000E72D4 File Offset: 0x000E54D4
		private void CrystalDespawned(EventTime eventTime)
		{
			VisualEffect visualEffect = this.mergeVfx;
			if (visualEffect != null)
			{
				visualEffect.transform.parent.SetParent(base.transform);
			}
			this.item.OnUngrabEvent -= this.OnUnGrabEvent;
			this.item.OnHeldActionEvent -= this.HeldAction;
			this.item.OnDespawnEvent -= this.CrystalDespawned;
			EffectInstance effectInstance = this.activateEffect;
			if (effectInstance != null)
			{
				effectInstance.Despawn();
			}
			this.activateEffect = null;
			SkillTreeCrystal.crystalsOfType.RemoveFromKeyedList(this.treeName, this);
			SkillTreeCrystal.allCrystals.Remove(this);
		}

		/// Test if we can merge, and set up the conditions for the merge to start
		// Token: 0x0600219F RID: 8607 RVA: 0x000E737C File Offset: 0x000E557C
		public void TestMerge()
		{
			RagdollHand mainHandler = this.item.mainHandler;
			Item item;
			if (mainHandler == null)
			{
				item = null;
			}
			else
			{
				Handle grabbedHandle = mainHandler.otherHand.grabbedHandle;
				item = ((grabbedHandle != null) ? grabbedHandle.item : null);
			}
			Item otherItem = item;
			SkillTreeCrystal crystal;
			if (otherItem == null || otherItem == this.item || this.mergingCrystal != null || !otherItem.TryGetComponent<SkillTreeCrystal>(out crystal) || !crystal.treeName.Equals(this.treeName) || !this.glowing || !crystal.glowing)
			{
				return;
			}
			this.mergingCrystal = crystal;
			this.mergingCrystal.mergingCrystal = this;
			this.mergingCrystal.mergeStartTime = (this.mergeStartTime = Time.time);
		}

		// Token: 0x060021A0 RID: 8608 RVA: 0x000E7434 File Offset: 0x000E5634
		private void HeldAction(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
		{
			if (action != Interactable.Action.UseStart && action != Interactable.Action.UseStop)
			{
				return;
			}
			if (this.item.handlers.CountCheck((int count) => count > 1))
			{
				return;
			}
			this.SetGlow(action == Interactable.Action.UseStart);
		}

		// Token: 0x17000216 RID: 534
		// (get) Token: 0x060021A1 RID: 8609 RVA: 0x000E7488 File Offset: 0x000E5688
		public bool HasOtherGlowingCrystal
		{
			get
			{
				RagdollHand mainHandler = this.item.mainHandler;
				Item item;
				if (mainHandler == null)
				{
					item = null;
				}
				else
				{
					Handle grabbedHandle = mainHandler.otherHand.grabbedHandle;
					item = ((grabbedHandle != null) ? grabbedHandle.item : null);
				}
				Item otherItem = item;
				SkillTreeCrystal crystal;
				return otherItem != null && otherItem != this.item && otherItem.TryGetComponent<SkillTreeCrystal>(out crystal) && crystal.treeName.Equals(this.treeName) && crystal.glowing;
			}
		}

		// Token: 0x060021A2 RID: 8610 RVA: 0x000E74F4 File Offset: 0x000E56F4
		public void SetGlow(bool active)
		{
			if (this.glowing == active)
			{
				return;
			}
			this.glowing = active;
			this.ToggleSparkles(this.glowing);
			if (active && !this.HasOtherGlowingCrystal)
			{
				EffectInstance effectInstance = this.activateEffect;
				if (effectInstance != null)
				{
					effectInstance.End(false, -1f);
				}
				EffectData activateEffectData = this.module.activateEffectData;
				this.activateEffect = ((activateEffectData != null) ? activateEffectData.Spawn(base.transform, true, null, false) : null);
				EffectInstance effectInstance2 = this.activateEffect;
				if (effectInstance2 == null)
				{
					return;
				}
				effectInstance2.Play(0, false, false);
				return;
			}
			else
			{
				EffectInstance effectInstance3 = this.activateEffect;
				if (effectInstance3 == null)
				{
					return;
				}
				effectInstance3.End(false, -1f);
				return;
			}
		}

		/// Connect this crystal to its merge point
		// Token: 0x060021A3 RID: 8611 RVA: 0x000E7590 File Offset: 0x000E5790
		private void ConnectMergeVFX(Transform target)
		{
			this.mergeVfxTarget.parent = target;
			this.mergeVfxTarget.localPosition = Vector3.zero;
			this.mergeVfx.gameObject.SetActive(true);
			this.mergeVfxActive = true;
		}

		/// Stop all merge VFX
		// Token: 0x060021A4 RID: 8612 RVA: 0x000E75C8 File Offset: 0x000E57C8
		private void DisconnectMergeVFX()
		{
			if (!this.mergeVfxActive && this.mergeBeginEffect == null)
			{
				return;
			}
			this.mergeVfx.gameObject.SetActive(false);
			this.mergeVfxActive = false;
			if (!this.mergeVfxTarget)
			{
				return;
			}
			this.mergeVfxTarget.parent = this.mergeVfx.transform;
			EffectInstance effectInstance = this.mergeBeginEffect;
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			this.mergeBeginEffect = null;
			EffectInstance effectInstance2 = this.mergeCrystalEffect;
			if (effectInstance2 != null)
			{
				effectInstance2.End(false, -1f);
			}
			this.mergeCrystalEffect = null;
			EffectInstance effectInstance3 = this.mergeEffect;
			if (effectInstance3 != null)
			{
				effectInstance3.End(false, -1f);
			}
			this.mergeEffect = null;
			if (this.mergingCrystal == null)
			{
				return;
			}
			EffectInstance effectInstance4 = this.mergingCrystal.mergeBeginEffect;
			if (effectInstance4 != null)
			{
				effectInstance4.End(false, -1f);
			}
			this.mergingCrystal.mergeBeginEffect = null;
			EffectInstance effectInstance5 = this.mergingCrystal.mergeEffect;
			if (effectInstance5 != null)
			{
				effectInstance5.End(false, -1f);
			}
			this.mergingCrystal.mergeEffect = null;
			EffectInstance effectInstance6 = this.mergingCrystal.mergeCrystalEffect;
			if (effectInstance6 != null)
			{
				effectInstance6.End(false, -1f);
			}
			this.mergingCrystal.mergeCrystalEffect = null;
			this.mergingCrystal.DisconnectMergeVFX();
		}

		// Token: 0x060021A5 RID: 8613 RVA: 0x000E7710 File Offset: 0x000E5910
		public void Init()
		{
			SkillTreeCrystal.crystalsOfType.AddToKeyedList(this.treeName, this);
			SkillTreeCrystal.allCrystals.Add(this);
			this.UpdateVertexLerp(0f, default(Vector3), false);
			if (!this.overrideCrystalColors)
			{
				return;
			}
			for (int i = 0; i < this.materialInstances.Length; i++)
			{
				this.materialInstances[i].material.SetColor(SkillTreeCrystal.CrystalColor, this.baseColor);
				this.materialInstances[i].material.SetColor(SkillTreeCrystal.InternalColor, this.internalColor);
				this.materialInstances[i].material.SetColor(SkillTreeCrystal.AnimatedColor, this.animatedColor);
				this.materialInstances[i].material.SetColor(SkillTreeCrystal.EmissionColor, this.emissionColor);
			}
			this.linkVfx.SetVector4("Source Color", this.linkVfxColor);
			this.mergeVfx.SetVector4("Source Color", this.mergeVfxColor);
		}

		/// <summary>
		/// Returns true if the skill is on the same tree or on a combining tree
		/// </summary>
		/// <param name="skill"></param>
		/// <returns></returns>
		// Token: 0x060021A6 RID: 8614 RVA: 0x000E7817 File Offset: 0x000E5A17
		public bool IsSkillOnTree(SkillData skill)
		{
			return skill.primarySkillTreeId == this.treeName || skill.secondarySkillTreeId == this.treeName;
		}

		// Token: 0x060021A7 RID: 8615 RVA: 0x000E783F File Offset: 0x000E5A3F
		protected internal override void ManagedUpdate()
		{
			if (!this.module.allowMerge)
			{
				return;
			}
			this.UpdateCrystalsLink();
			this.UpdateCrystalGlow();
			this.TestMerge();
		}

		/// <summary>
		/// Update link and merge VFX, this is also where the actual merge is triggered
		/// (because we want to sync up the VFX and the start of the merge)
		/// </summary>
		// Token: 0x060021A8 RID: 8616 RVA: 0x000E7864 File Offset: 0x000E5A64
		private void UpdateCrystalsLink()
		{
			this.linkVfxActive = false;
			bool playerHoldingTwoCrystals = false;
			if (this.hand == null)
			{
				Item crystalLinked = null;
				if (Player.currentCreature)
				{
					Item crystalLeft = null;
					Item crystalRight = null;
					RagdollHand handRight = Player.currentCreature.handRight;
					Item item;
					if (handRight == null)
					{
						item = null;
					}
					else
					{
						Handle grabbedHandle = handRight.grabbedHandle;
						item = ((grabbedHandle != null) ? grabbedHandle.item : null);
					}
					Item itemRight = item;
					ItemModuleCrystal crystalModuleRight;
					if (itemRight != null && itemRight.data.TryGetModule<ItemModuleCrystal>(out crystalModuleRight) && string.Equals(crystalModuleRight.treeName, this.treeName))
					{
						crystalRight = itemRight;
					}
					RagdollHand handLeft = Player.currentCreature.handLeft;
					Item item2;
					if (handLeft == null)
					{
						item2 = null;
					}
					else
					{
						Handle grabbedHandle2 = handLeft.grabbedHandle;
						item2 = ((grabbedHandle2 != null) ? grabbedHandle2.item : null);
					}
					Item itemLeft = item2;
					ItemModuleCrystal crystalModuleLeft;
					if (itemLeft != null && itemLeft.data.TryGetModule<ItemModuleCrystal>(out crystalModuleLeft) && string.Equals(crystalModuleLeft.treeName, this.treeName))
					{
						crystalLeft = itemLeft;
					}
					if (crystalLeft && crystalRight)
					{
						playerHoldingTwoCrystals = true;
						if (crystalLeft == this.item)
						{
							crystalLinked = crystalRight;
						}
						else if (crystalRight == this.item)
						{
							crystalLinked = crystalLeft;
						}
					}
					else
					{
						crystalLinked = (crystalLeft ?? crystalRight);
						if (crystalLinked == this.item)
						{
							crystalLinked = null;
						}
					}
				}
				Vector3? targetPos = null;
				if (crystalLinked != null)
				{
					targetPos = new Vector3?(crystalLinked.transform.position);
				}
				else if (!playerHoldingTwoCrystals && this.IsCrystalSameTreeInInventory())
				{
					targetPos = new Vector3?(Player.currentCreature.Center);
				}
				if (targetPos != null && Vector3.Distance(base.transform.position, targetPos.Value) < this.linkMaxDistance)
				{
					this.linkVfxActive = true;
					if (this.timeLerpLinkVfx < this.timeVfxTransitionMin)
					{
						this.timeLerpLinkVfx = this.timeVfxTransitionMin;
					}
					this.linkVfxTarget.position = targetPos.Value;
				}
			}
			if (this.mergeEffect != null)
			{
				this.linkVfx.gameObject.SetActive(false);
			}
			if (this.mergeVfxTarget)
			{
				this.mergePoint.transform.position = Vector3.Lerp(this.mergeVfxTarget.position, base.transform.position, 0.5f);
			}
			if (this.mergingCrystal != null && this.mergeBeginEffect == null)
			{
				EffectInstance effectInstance = this.mergeBeginEffect;
				if (effectInstance != null)
				{
					effectInstance.End(false, -1f);
				}
				EffectData effectData = this.mergeBeginEffectData;
				this.mergeBeginEffect = ((effectData != null) ? effectData.Spawn(this.mergePoint, true, null, false) : null);
				EffectInstance effectInstance2 = this.mergeBeginEffect;
				if (effectInstance2 != null)
				{
					effectInstance2.Play(0, false, false);
				}
			}
			if (this.mergingCrystal != null && this.mergeEffect == null && Time.time - this.mergeStartTime > ItemModuleCrystal.mergeEffectDelay)
			{
				EffectInstance effectInstance3 = this.mergeEffect;
				if (effectInstance3 != null)
				{
					effectInstance3.End(false, -1f);
				}
				EffectData effectData2 = this.mergeEffectData;
				this.mergeEffect = ((effectData2 != null) ? effectData2.Spawn(this.mergePoint, true, null, false) : null);
				EffectInstance effectInstance4 = this.mergeCrystalEffect;
				if (effectInstance4 != null)
				{
					effectInstance4.End(false, -1f);
				}
				EffectData effectData3 = this.mergeCrystalEffectAndroidData;
				this.mergeCrystalEffect = ((effectData3 != null) ? effectData3.Spawn(base.transform, true, null, false) : null);
				if (Common.IsAndroid)
				{
					EffectInstance effectInstance5 = this.mergeEffect;
					if (effectInstance5 != null)
					{
						effectInstance5.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor.UnHDR(), this.skillTreeEmissionColor.UnHDR()));
					}
					EffectInstance effectInstance6 = this.mergeCrystalEffect;
					if (effectInstance6 != null)
					{
						effectInstance6.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor.UnHDR(), this.skillTreeEmissionColor.UnHDR()));
					}
				}
				else
				{
					EffectInstance effectInstance7 = this.mergeEffect;
					if (effectInstance7 != null)
					{
						effectInstance7.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor, this.skillTreeEmissionColor));
					}
					EffectInstance effectInstance8 = this.mergeCrystalEffect;
					if (effectInstance8 != null)
					{
						effectInstance8.SetMainGradient(Utils.CreateGradient(this.skillTreeEmissionColor, this.skillTreeEmissionColor));
					}
				}
				EffectInstance effectInstance9 = this.mergeEffect;
				if (effectInstance9 != null)
				{
					effectInstance9.Play(0, false, false);
				}
				EffectInstance effectInstance10 = this.mergeCrystalEffect;
				if (effectInstance10 != null)
				{
					effectInstance10.Play(0, false, false);
				}
				this.ConnectMergeVFX(this.mergingCrystal.transform);
				this.mergingCrystal.ConnectMergeVFX(base.transform);
				this.Merge(this.mergingCrystal);
			}
			if (this.linkVfx)
			{
				if (this.linkVfxActive)
				{
					if (!this.linkVfx.gameObject.activeSelf)
					{
						this.linkVfx.gameObject.SetActive(true);
					}
					if (this.timeLerpLinkVfx < 1f)
					{
						this.timeLerpLinkVfx += Time.deltaTime / this.timeVfxTransition;
						if (this.timeLerpLinkVfx > 1f)
						{
							this.timeLerpLinkVfx = 1f;
						}
					}
				}
				else if (this.timeLerpLinkVfx > 0f)
				{
					this.timeLerpLinkVfx -= Time.deltaTime / this.timeVfxTransition;
					if (this.timeLerpLinkVfx <= 0f)
					{
						this.timeLerpLinkVfx = 0f;
						float intensity = Mathf.InverseLerp(this.timeVfxTransitionMin, 1f, this.timeLerpLinkVfx);
						this.linkVfx.SetFloat(SkillTreeCrystal.Intensity, intensity);
						this.linkVfx.gameObject.SetActive(false);
					}
				}
				if (this.linkVfx.gameObject.activeSelf)
				{
					float intensity2 = Mathf.InverseLerp(this.timeVfxTransitionMin, 1f, this.timeLerpLinkVfx);
					this.linkVfx.SetFloat(SkillTreeCrystal.Intensity, intensity2);
				}
			}
		}

		// Token: 0x060021A9 RID: 8617 RVA: 0x000E7DB8 File Offset: 0x000E5FB8
		private void UpdateCrystalGlow()
		{
			if (this.glowing)
			{
				if (this.glowIntensity < 1f)
				{
					this.glowIntensity += Time.deltaTime / this.timeGlowTransition;
					if (this.glowIntensity > 1f)
					{
						this.glowIntensity = 1f;
					}
				}
				this.item.Haptic((0.2f + (Mathf.Sin(Time.unscaledTime * 3f) + 1f) / 2f * 0.8f) * this.glowIntensity * 0.5f, false);
			}
			else if (this.glowIntensity > 0f)
			{
				this.glowIntensity -= Time.deltaTime / this.timeGlowTransition;
				if (this.glowIntensity < 0f)
				{
					this.glowIntensity = 0f;
					for (int i = 0; i < this.materialInstances.Length; i++)
					{
						this.materialInstances[i].material.SetFloat(SkillTreeCrystal.GlowProperty, 0f);
					}
				}
			}
			if (this.timeGlowTransition > 0f && this.glowCurve != null)
			{
				float glow = this.glowCurve.Evaluate(this.glowTime) * this.glowIntensity * ItemModuleCrystal.glowMultiplier;
				if (this.mergeVfxTarget)
				{
					float distance = Vector3.Distance(this.mergeVfxTarget.position, base.transform.position);
					glow *= Mathf.Pow(2f, 1f + Mathf.InverseLerp(0.5f, 0f, distance) * 2f);
				}
				this.glowTime += Time.deltaTime;
				for (int j = 0; j < this.materialInstances.Length; j++)
				{
					this.materialInstances[j].material.SetFloat(SkillTreeCrystal.GlowProperty, glow);
				}
			}
		}

		// Token: 0x060021AA RID: 8618 RVA: 0x000E7F88 File Offset: 0x000E6188
		public void UpdateVertexLerp(float amount, Vector3 target = default(Vector3), bool isLocal = false)
		{
			for (int i = 0; i < this.materialInstances.Length; i++)
			{
				this.materialInstances[i].material.SetFloat(SkillTreeCrystal.VertexLerpAmount, amount);
				this.materialInstances[i].material.SetVector(SkillTreeCrystal.VertexLerpTarget, isLocal ? target : this.materialInstances[i].transform.InverseTransformPoint(target));
			}
		}

		// Token: 0x060021AB RID: 8619 RVA: 0x000E7FF8 File Offset: 0x000E61F8
		private bool IsCrystalSameTreeInInventory()
		{
			if (this.item == null || this.item.data == null || Player.currentCreature == null)
			{
				return false;
			}
			for (int i = 0; i < Player.currentCreature.container.contents.Count; i++)
			{
				ItemContent itemContent = Player.currentCreature.container.contents[i] as ItemContent;
				if (itemContent != null)
				{
					ItemData data = itemContent.catalogData as ItemData;
					ItemModuleCrystal crystalModule;
					if (data != null && data.type == ItemData.Type.Crystal && itemContent.state == null && !(data.category != this.item.data.category) && data.TryGetModule<ItemModuleCrystal>(out crystalModule) && string.Equals(this.treeName, crystalModule.treeName))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x060021AC RID: 8620 RVA: 0x000E80CD File Offset: 0x000E62CD
		protected internal override void ManagedFixedUpdate()
		{
			base.ManagedUpdate();
		}

		// Token: 0x060021AD RID: 8621 RVA: 0x000E80D8 File Offset: 0x000E62D8
		public void ToggleSparkles(bool enable)
		{
			if (!enable)
			{
				EffectInstance effectInstance = this.sparkleEffect;
				if (effectInstance != null)
				{
					effectInstance.End(false, -1f);
				}
				this.sparkleEffect = null;
				return;
			}
			EffectInstance effectInstance2 = this.sparkleEffect;
			if (effectInstance2 != null)
			{
				effectInstance2.End(false, -1f);
			}
			EffectData effectData = this.sparkleEffectData;
			this.sparkleEffect = ((effectData != null) ? effectData.Spawn(base.transform, true, null, false) : null);
			if (Common.IsAndroid)
			{
				EffectInstance effectInstance3 = this.sparkleEffect;
				if (effectInstance3 != null)
				{
					effectInstance3.SetMainGradient(Utils.CreateGradient(this.module.skillTreeData.color.UnHDR(), this.module.skillTreeData.emissionColor.UnHDR()));
				}
			}
			else
			{
				EffectInstance effectInstance4 = this.sparkleEffect;
				if (effectInstance4 != null)
				{
					effectInstance4.SetMainGradient(Utils.CreateGradient(this.module.skillTreeData.color, this.module.skillTreeData.emissionColor));
				}
			}
			EffectInstance effectInstance5 = this.sparkleEffect;
			if (effectInstance5 == null)
			{
				return;
			}
			effectInstance5.Play(0, false, false);
		}

		// Token: 0x060021AE RID: 8622 RVA: 0x000E81D4 File Offset: 0x000E63D4
		public void Merge(SkillTreeCrystal other)
		{
			if (other.treeName != this.treeName)
			{
				return;
			}
			SkillTreeCrystal.MergeCrystal(this, other);
		}

		/// Static method to merge two crystals
		// Token: 0x060021AF RID: 8623 RVA: 0x000E81F4 File Offset: 0x000E63F4
		public static void MergeCrystal(SkillTreeCrystal crystal1, SkillTreeCrystal crystal2)
		{
			if (crystal1.merging || crystal2.merging)
			{
				return;
			}
			int tier = crystal1.item.data.tier;
			int tier2 = crystal2.item.data.tier;
			bool flag = tier > tier2;
			SkillTreeCrystal mainCrystal = flag ? crystal1 : crystal2;
			SkillTreeCrystal otherCrystal = flag ? crystal2 : crystal1;
			int numberUpgrade = flag ? tier2 : tier;
			ItemModuleCrystal previousCrystal = mainCrystal.item.data.GetModule<ItemModuleCrystal>();
			ItemData crystalMerged = mainCrystal.item.data;
			string itemLeftover = null;
			ItemData higherCrystal;
			int leftovers;
			if (string.IsNullOrEmpty(mainCrystal.module.higherTierCrystalId) || !Catalog.TryGetData<ItemData>(mainCrystal.module.higherTierCrystalId, out higherCrystal, true))
			{
				leftovers = otherCrystal.item.data.tier;
			}
			else
			{
				leftovers = tier + tier2 - higherCrystal.tier;
			}
			leftovers *= Catalog.gameData.mergeLeftoverShardMultiplier;
			for (int i = 0; i < numberUpgrade; i++)
			{
				ItemData tempCrystal;
				if (string.IsNullOrEmpty(previousCrystal.higherTierCrystalId) || !Catalog.TryGetData<ItemData>(previousCrystal.higherTierCrystalId, out tempCrystal, true))
				{
					itemLeftover = previousCrystal.shardId;
					break;
				}
				crystalMerged = tempCrystal;
				previousCrystal = crystalMerged.GetModule<ItemModuleCrystal>();
			}
			if (crystalMerged == null)
			{
				Debug.LogError("Encountered an unexpected error: no merge could be complete");
				return;
			}
			crystal1.merging = true;
			crystal2.merging = true;
			crystal1.item.AddNonStorableModifier(crystal2);
			crystal2.item.AddNonStorableModifier(crystal1);
			SkillTreeCrystal.isMerging = true;
			mainCrystal.StartCoroutine(SkillTreeCrystal.MergeCoroutine(mainCrystal, otherCrystal, crystalMerged, itemLeftover, leftovers));
		}

		/// <summary>
		/// Where the merge-y magic happens. Brings crystals close together, plays an effect, then calls FinishMerge once done.
		/// </summary>
		/// <returns></returns>
		// Token: 0x060021B0 RID: 8624 RVA: 0x000E8360 File Offset: 0x000E6560
		private static IEnumerator MergeCoroutine(SkillTreeCrystal mainCrystal, SkillTreeCrystal otherCrystal, ItemData mergedData, string leftoverId, int leftoverCount)
		{
			SkillTreeCrystal.<>c__DisplayClass87_0 CS$<>8__locals1 = new SkillTreeCrystal.<>c__DisplayClass87_0();
			CS$<>8__locals1.mainCrystal = mainCrystal;
			CS$<>8__locals1.otherCrystal = otherCrystal;
			CS$<>8__locals1.mergedData = mergedData;
			CS$<>8__locals1.leftoverId = leftoverId;
			CS$<>8__locals1.leftoverCount = leftoverCount;
			SkillTreeCrystal.<MergeCoroutine>g__StartCrystal|87_1(CS$<>8__locals1.mainCrystal);
			SkillTreeCrystal.<MergeCoroutine>g__StartCrystal|87_1(CS$<>8__locals1.otherCrystal);
			CS$<>8__locals1.mergePoint = PoolUtils.GetTransformPoolManager().Get();
			CS$<>8__locals1.mergePoint.transform.position = Vector3.Lerp(Player.currentCreature.handLeft.grip.position, Player.currentCreature.handRight.grip.position, 0.5f);
			CS$<>8__locals1.mergeCompleteEffect = null;
			yield return Utils.LoopOver(new Action<float>(CS$<>8__locals1.<MergeCoroutine>g__Loop|0), 3f, new Action(CS$<>8__locals1.<MergeCoroutine>g__End|4), 0f, false);
			yield break;
		}

		/// <summary>
		/// Finish the merge, despawn old crystals, spawn new crystals and leftover shards.
		/// </summary>
		// Token: 0x060021B1 RID: 8625 RVA: 0x000E838C File Offset: 0x000E658C
		private static void FinishMerge(SkillTreeCrystal mainCrystal, SkillTreeCrystal otherCrystal, ItemData merged, string leftoversID, int leftovers)
		{
			SkillTreeCrystal.<>c__DisplayClass88_0 CS$<>8__locals1 = new SkillTreeCrystal.<>c__DisplayClass88_0();
			CS$<>8__locals1.mainCrystal = mainCrystal;
			CS$<>8__locals1.otherCrystal = otherCrystal;
			CS$<>8__locals1.joint = null;
			SkillTreeCrystal.isMerging = false;
			if (!CS$<>8__locals1.mainCrystal.item.data.id.Equals(merged.id))
			{
				merged.SpawnAsync(delegate(Item crystalSpawn)
				{
					CS$<>8__locals1.mainCrystal.item.RemoveNonStorableModifier(CS$<>8__locals1.otherCrystal);
					CS$<>8__locals1.otherCrystal.item.RemoveNonStorableModifier(CS$<>8__locals1.mainCrystal);
					CS$<>8__locals1.otherCrystal.item.Despawn();
					CS$<>8__locals1.mainCrystal.item.Despawn();
					CS$<>8__locals1.mainCrystal = null;
					SkillTreeCrystal spawnedCrystal = crystalSpawn.GetComponent<SkillTreeCrystal>();
					base.<FinishMerge>g__FloatInPlace|2(spawnedCrystal);
					SkillTreeCrystal.LerpIn(spawnedCrystal);
				}, null, null, null, true, null, Item.Owner.None);
			}
			else
			{
				CS$<>8__locals1.mainCrystal.item.RemoveNonStorableModifier(CS$<>8__locals1.otherCrystal);
				CS$<>8__locals1.otherCrystal.item.Despawn();
				CS$<>8__locals1.mainCrystal.mergingCrystal = null;
				CS$<>8__locals1.mainCrystal.UpdateVertexLerp(0f, default(Vector3), false);
				CS$<>8__locals1.mainCrystal.DisconnectMergeVFX();
				CS$<>8__locals1.mainCrystal.mergingCrystal = null;
				CS$<>8__locals1.mainCrystal.merging = false;
				CS$<>8__locals1.<FinishMerge>g__FloatInPlace|2(CS$<>8__locals1.mainCrystal);
				SkillTreeCrystal.LerpIn(CS$<>8__locals1.mainCrystal);
			}
			Player.currentCreature.handLeft.HapticTick(1f, true);
			Player.currentCreature.handRight.HapticTick(1f, true);
			if (leftovers <= 0)
			{
				return;
			}
			RagdollHand hand = Player.currentCreature.handRight;
			ItemData leftoverItem = Catalog.GetData<ItemData>(leftoversID, true);
			if (leftoverItem == null)
			{
				return;
			}
			CS$<>8__locals1.pos = hand.transform.position + Vector3.up * 0.05f;
			for (int i = 0; i < leftovers; i++)
			{
				ItemData itemData = leftoverItem;
				Action<Item> callback;
				if ((callback = CS$<>8__locals1.<>9__3) == null)
				{
					callback = (CS$<>8__locals1.<>9__3 = delegate(Item leftoverItemSpawn)
					{
						leftoverItemSpawn.GetPhysicBody().useGravity = false;
						leftoverItemSpawn.OnGrabEvent += SkillTreeCrystal.ResetGravity;
						SkillTreeShard component = leftoverItemSpawn.GetComponent<SkillTreeShard>();
						component.StartCoroutine(SkillTreeCrystal.LeftoverShardRoutine(component, CS$<>8__locals1.pos));
					});
				}
				itemData.SpawnAsync(callback, new Vector3?(CS$<>8__locals1.pos + UnityEngine.Random.onUnitSphere * 0.2f), new Quaternion?(Quaternion.identity), null, true, null, Item.Owner.None);
			}
		}

		// Token: 0x060021B2 RID: 8626 RVA: 0x000E8564 File Offset: 0x000E6764
		public static void LerpIn(SkillTreeCrystal crystal)
		{
			crystal.item.SetOwner(Item.Owner.Player);
			Vector3 position = crystal.transform.position;
			crystal.UpdateVertexLerp(1f, position, false);
			crystal.RunAfter(delegate()
			{
				EffectData mergeCompleteEffectData = crystal.module.mergeCompleteEffectData;
				if (mergeCompleteEffectData == null)
				{
					return;
				}
				EffectInstance effectInstance = mergeCompleteEffectData.Spawn(crystal.transform, true, null, false);
				if (effectInstance == null)
				{
					return;
				}
				effectInstance.Play(0, false, false);
			}, 0.01f, false);
			crystal.LoopOver(delegate(float time)
			{
				crystal.UpdateVertexLerp(1f - time, position, false);
			}, 0.5f, null, 0f, false);
		}

		// Token: 0x060021B3 RID: 8627 RVA: 0x000E85FE File Offset: 0x000E67FE
		public static IEnumerator AttachmentBobRoutine(Rigidbody attachment)
		{
			Vector3 startPos = attachment.transform.position;
			float startTime = Time.time;
			while (!(attachment == null))
			{
				attachment.transform.SetPositionAndRotation(startPos + Vector3.up * (Mathf.Sin((Time.time - startTime) * 1.5f) * 0.1f), Quaternion.AngleAxis(Time.time * 30f, Vector3.up));
				yield return 0;
			}
			yield break;
			yield break;
		}

		// Token: 0x060021B4 RID: 8628 RVA: 0x000E860D File Offset: 0x000E680D
		protected static IEnumerator LeftoverShardRoutine(SkillTreeShard shard, Vector3 pos)
		{
			shard.item.SetPhysicModifier(shard, new float?(0f), 1f, 3f);
			shard.item.AddExplosionForce(0.7f, pos, 1f, 0f, ForceMode.VelocityChange, null);
			shard.item.AddForce(Player.local.head.transform.forward * 2f, ForceMode.VelocityChange, null);
			shard.item.mainHandleLeft.SetTouch(false);
			shard.item.mainHandleLeft.SetTelekinesis(false);
			yield return new WaitForSeconds(UnityEngine.Random.Range(1f, 2f));
			shard.item.RemovePhysicModifier(shard);
			Player.characterData.inventory.AddCurrencyValue(Currency.CrystalShard, 1f);
			shard.genericAttractionTarget = Player.currentCreature.ragdoll.targetPart.transform;
			yield break;
		}

		// Token: 0x060021B5 RID: 8629 RVA: 0x000E8623 File Offset: 0x000E6823
		private static void ResetGravity(Handle handle, RagdollHand ragdollHand)
		{
			handle.item.GetPhysicBody().useGravity = true;
			handle.item.OnGrabEvent -= SkillTreeCrystal.ResetGravity;
		}

		// Token: 0x060021B6 RID: 8630 RVA: 0x000E864D File Offset: 0x000E684D
		public List<ValueDropdownItem<string>> GetAllSkillTreeID()
		{
			if (Catalog.gameData != null)
			{
				return Catalog.GetDropdownAllID(Category.SkillTree, "None");
			}
			return new List<ValueDropdownItem<string>>();
		}

		// Token: 0x060021B9 RID: 8633 RVA: 0x000E8744 File Offset: 0x000E6944
		[CompilerGenerated]
		internal static void <MergeCoroutine>g__StartCrystal|87_1(SkillTreeCrystal crystal)
		{
			crystal.hand = crystal.item.mainHandler;
			crystal.item.ForceUngrabAll();
			crystal.hand.caster.DisableSpellWheel(crystal);
			crystal.hand.caster.DisallowCasting(crystal);
			crystal.hand.SetBlockGrab(true, true);
			crystal.item.physicBody.isKinematic = true;
			crystal.item.AddNonStorableModifier(crystal);
			crystal.hand.poser.SetDefaultPose(crystal.floatHandPose.defaultHandPoseData);
			crystal.hand.poser.SetTargetPose(crystal.floatHandPose.targetHandPoseData, false, false, false, false, false);
			crystal.hand.poser.SetTargetWeight(0f, false);
		}

		// Token: 0x060021BA RID: 8634 RVA: 0x000E880C File Offset: 0x000E6A0C
		[CompilerGenerated]
		internal static void <MergeCoroutine>g__FinishCrystal|87_3(SkillTreeCrystal crystal)
		{
			crystal.hand.SetBlockGrab(false, true);
			crystal.item.physicBody.isKinematic = false;
			crystal.item.RemoveNonStorableModifier(crystal);
			crystal.hand.caster.AllowSpellWheel(crystal);
			crystal.hand.caster.AllowCasting(crystal);
			crystal.hand.poser.ResetDefaultPose();
			crystal.hand.poser.ResetTargetPose();
			crystal.hand = null;
			crystal.SetGlow(false);
		}

		// Token: 0x04002052 RID: 8274
		public RagdollHand hand;

		// Token: 0x04002053 RID: 8275
		public HandlePose floatHandPose;

		// Token: 0x04002054 RID: 8276
		[Header("Merge VFx")]
		public AnimationCurve glowCurve;

		// Token: 0x04002055 RID: 8277
		public float timeGlowTransition = 1f;

		// Token: 0x04002056 RID: 8278
		public float timeVfxTransition = 2f;

		// Token: 0x04002057 RID: 8279
		public float timeVfxTransitionMin = 0.5f;

		// Token: 0x04002058 RID: 8280
		[FormerlySerializedAs("mergeVfx")]
		public VisualEffect mergeVfxWindows;

		// Token: 0x04002059 RID: 8281
		[FormerlySerializedAs("mergeVfxTarget")]
		public Transform mergeVfxTargetWindows;

		// Token: 0x0400205A RID: 8282
		[FormerlySerializedAs("linkVfx")]
		public VisualEffect linkVfxWindows;

		// Token: 0x0400205B RID: 8283
		[FormerlySerializedAs("linkVfxTarget")]
		public Transform linkVfxTargetWindows;

		// Token: 0x0400205C RID: 8284
		public VisualEffect mergeVfxAndroid;

		// Token: 0x0400205D RID: 8285
		public Transform mergeVfxTargetAndroid;

		// Token: 0x0400205E RID: 8286
		public VisualEffect linkVfxAndroid;

		// Token: 0x0400205F RID: 8287
		public Transform linkVfxTargetAndroid;

		// Token: 0x04002060 RID: 8288
		protected VisualEffect mergeVfx;

		// Token: 0x04002061 RID: 8289
		protected Transform mergeVfxTarget;

		// Token: 0x04002062 RID: 8290
		protected VisualEffect linkVfx;

		// Token: 0x04002063 RID: 8291
		protected Transform linkVfxTarget;

		// Token: 0x04002064 RID: 8292
		public float linkMaxDistance = 2f;

		// Token: 0x04002065 RID: 8293
		[Header("Skill tree")]
		public string treeName;

		// Token: 0x04002066 RID: 8294
		[ColorUsage(true, true)]
		public Color skillTreeEmissionColor = Color.white;

		// Token: 0x04002067 RID: 8295
		[Header("Custom")]
		public bool overrideCrystalColors;

		// Token: 0x04002068 RID: 8296
		[ColorUsage(true, true)]
		public Color baseColor;

		// Token: 0x04002069 RID: 8297
		[ColorUsage(true, true)]
		public Color internalColor;

		// Token: 0x0400206A RID: 8298
		[ColorUsage(true, true)]
		public Color animatedColor;

		// Token: 0x0400206B RID: 8299
		[ColorUsage(true, true)]
		public Color emissionColor;

		// Token: 0x0400206C RID: 8300
		[ColorUsage(true, true)]
		public Color linkVfxColor;

		// Token: 0x0400206D RID: 8301
		[ColorUsage(true, true)]
		public Color mergeVfxColor;

		// Token: 0x0400206E RID: 8302
		public EffectData sparkleEffectData;

		// Token: 0x0400206F RID: 8303
		public EffectInstance sparkleEffect;

		// Token: 0x04002070 RID: 8304
		[NonSerialized]
		public EffectData hoverEffectData;

		// Token: 0x04002071 RID: 8305
		public static Dictionary<string, HashSet<SkillTreeCrystal>> crystalsOfType = new Dictionary<string, HashSet<SkillTreeCrystal>>();

		// Token: 0x04002072 RID: 8306
		public static List<SkillTreeCrystal> allCrystals = new List<SkillTreeCrystal>();

		// Token: 0x04002073 RID: 8307
		[NonSerialized]
		public Item item;

		// Token: 0x04002074 RID: 8308
		[NonSerialized]
		public SkillTreeReceptacle receptacle;

		// Token: 0x04002075 RID: 8309
		[NonSerialized]
		public bool merging;

		// Token: 0x04002076 RID: 8310
		[NonSerialized]
		protected Transform mergePoint;

		// Token: 0x04002077 RID: 8311
		public EffectData mergeBeginEffectData;

		// Token: 0x04002078 RID: 8312
		protected EffectInstance mergeBeginEffect;

		// Token: 0x04002079 RID: 8313
		public EffectData mergeEffectData;

		// Token: 0x0400207A RID: 8314
		public EffectData mergeCrystalEffectAndroidData;

		// Token: 0x0400207B RID: 8315
		protected EffectInstance mergeEffect;

		// Token: 0x0400207C RID: 8316
		protected EffectInstance mergeCrystalEffect;

		// Token: 0x0400207D RID: 8317
		protected EffectInstance activateEffect;

		// Token: 0x0400207E RID: 8318
		protected float mergeStartTime;

		// Token: 0x0400207F RID: 8319
		protected SkillTreeCrystal mergingCrystal;

		// Token: 0x04002080 RID: 8320
		public ItemModuleCrystal module;

		// Token: 0x04002081 RID: 8321
		private MaterialInstance[] materialInstances;

		// Token: 0x04002082 RID: 8322
		[NonSerialized]
		public bool mergeVfxActive;

		// Token: 0x04002083 RID: 8323
		[NonSerialized]
		public bool linkVfxActive;

		// Token: 0x04002084 RID: 8324
		private bool glowing;

		// Token: 0x04002085 RID: 8325
		private float glowTime;

		// Token: 0x04002086 RID: 8326
		private float glowIntensity;

		// Token: 0x04002087 RID: 8327
		private float timeLerpLinkVfx;

		// Token: 0x04002088 RID: 8328
		private static readonly int GlowProperty = Shader.PropertyToID("_Glow");

		// Token: 0x04002089 RID: 8329
		private static readonly int CrystalColor = Shader.PropertyToID("_Color");

		// Token: 0x0400208A RID: 8330
		private static readonly int InternalColor = Shader.PropertyToID("_InternalColor");

		// Token: 0x0400208B RID: 8331
		private static readonly int AnimatedColor = Shader.PropertyToID("_InternalAnimatedColor");

		// Token: 0x0400208C RID: 8332
		private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");

		// Token: 0x0400208D RID: 8333
		private static readonly int VertexLerpAmount = Shader.PropertyToID("_VertexLerpAmount");

		// Token: 0x0400208E RID: 8334
		private static readonly int VertexLerpTarget = Shader.PropertyToID("_VertexLerpTarget");

		// Token: 0x0400208F RID: 8335
		private static readonly int Intensity = Shader.PropertyToID("Intensity");

		// Token: 0x04002090 RID: 8336
		public static bool isMerging;

		// Token: 0x04002091 RID: 8337
		public bool isSocketed;
	}
}
