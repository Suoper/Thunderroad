using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using ThunderRoad.Manikin;
using ThunderRoad.Skill;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000250 RID: 592
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/Creature.html")]
	[AddComponentMenu("ThunderRoad/Creatures/Creature")]
	public class Creature : ThunderEntity
	{
		// Token: 0x17000185 RID: 389
		// (get) Token: 0x0600190A RID: 6410 RVA: 0x000A4A8D File Offset: 0x000A2C8D
		public Locomotion currentLocomotion
		{
			get
			{
				if (!this.isPlayer)
				{
					return this.locomotion;
				}
				return this.player.locomotion;
			}
		}

		// Token: 0x14000098 RID: 152
		// (add) Token: 0x0600190B RID: 6411 RVA: 0x000A4AAC File Offset: 0x000A2CAC
		// (remove) Token: 0x0600190C RID: 6412 RVA: 0x000A4AE4 File Offset: 0x000A2CE4
		public event Action onEyesEnterUnderwater;

		// Token: 0x14000099 RID: 153
		// (add) Token: 0x0600190D RID: 6413 RVA: 0x000A4B1C File Offset: 0x000A2D1C
		// (remove) Token: 0x0600190E RID: 6414 RVA: 0x000A4B54 File Offset: 0x000A2D54
		public event Action onEyesExitUnderwater;

		// Token: 0x1400009A RID: 154
		// (add) Token: 0x0600190F RID: 6415 RVA: 0x000A4B8C File Offset: 0x000A2D8C
		// (remove) Token: 0x06001910 RID: 6416 RVA: 0x000A4BC4 File Offset: 0x000A2DC4
		public event Creature.FallEvent OnFallEvent;

		// Token: 0x1400009B RID: 155
		// (add) Token: 0x06001911 RID: 6417 RVA: 0x000A4BFC File Offset: 0x000A2DFC
		// (remove) Token: 0x06001912 RID: 6418 RVA: 0x000A4C34 File Offset: 0x000A2E34
		public event Creature.ForceSkillLoadEvent OnForceSkillLoadEvent;

		// Token: 0x1400009C RID: 156
		// (add) Token: 0x06001913 RID: 6419 RVA: 0x000A4C6C File Offset: 0x000A2E6C
		// (remove) Token: 0x06001914 RID: 6420 RVA: 0x000A4CA4 File Offset: 0x000A2EA4
		public event Creature.ForceSkillLoadEvent OnForceSkillUnloadEvent;

		// Token: 0x1400009D RID: 157
		// (add) Token: 0x06001915 RID: 6421 RVA: 0x000A4CDC File Offset: 0x000A2EDC
		// (remove) Token: 0x06001916 RID: 6422 RVA: 0x000A4D14 File Offset: 0x000A2F14
		public event Creature.ImbueChangeEvent OnHeldImbueChange;

		// Token: 0x1400009E RID: 158
		// (add) Token: 0x06001917 RID: 6423 RVA: 0x000A4D4C File Offset: 0x000A2F4C
		// (remove) Token: 0x06001918 RID: 6424 RVA: 0x000A4D84 File Offset: 0x000A2F84
		public event Creature.DespawnEvent OnDespawnEvent;

		// Token: 0x1400009F RID: 159
		// (add) Token: 0x06001919 RID: 6425 RVA: 0x000A4DBC File Offset: 0x000A2FBC
		// (remove) Token: 0x0600191A RID: 6426 RVA: 0x000A4DF4 File Offset: 0x000A2FF4
		public event Creature.ThrowEvent OnThrowEvent;

		// Token: 0x140000A0 RID: 160
		// (add) Token: 0x0600191B RID: 6427 RVA: 0x000A4E2C File Offset: 0x000A302C
		// (remove) Token: 0x0600191C RID: 6428 RVA: 0x000A4E64 File Offset: 0x000A3064
		public event Creature.ThisCreatureAttackEvent OnThisCreatureAttackEvent;

		// Token: 0x17000186 RID: 390
		// (get) Token: 0x0600191D RID: 6429 RVA: 0x000A4E99 File Offset: 0x000A3099
		public bool canPlayDynamicAnimation
		{
			get
			{
				return this.animatorOverrideController != null;
			}
		}

		// Token: 0x140000A1 RID: 161
		// (add) Token: 0x0600191E RID: 6430 RVA: 0x000A4EA8 File Offset: 0x000A30A8
		// (remove) Token: 0x0600191F RID: 6431 RVA: 0x000A4EE0 File Offset: 0x000A30E0
		public event Creature.ZoneEvent OnZoneEvent;

		// Token: 0x140000A2 RID: 162
		// (add) Token: 0x06001920 RID: 6432 RVA: 0x000A4F18 File Offset: 0x000A3118
		// (remove) Token: 0x06001921 RID: 6433 RVA: 0x000A4F50 File Offset: 0x000A3150
		public event Creature.SimpleDelegate OnDataLoaded;

		// Token: 0x140000A3 RID: 163
		// (add) Token: 0x06001922 RID: 6434 RVA: 0x000A4F88 File Offset: 0x000A3188
		// (remove) Token: 0x06001923 RID: 6435 RVA: 0x000A4FC0 File Offset: 0x000A31C0
		public event Creature.SimpleDelegate OnHeightChanged;

		// Token: 0x17000187 RID: 391
		// (get) Token: 0x06001924 RID: 6436 RVA: 0x000A4FF5 File Offset: 0x000A31F5
		public Creature.State state
		{
			get
			{
				if (this.isKilled)
				{
					return Creature.State.Dead;
				}
				if (this.ragdoll.state == Ragdoll.State.Destabilized || this.ragdoll.state == Ragdoll.State.Inert)
				{
					return Creature.State.Destabilized;
				}
				return Creature.State.Alive;
			}
		}

		// Token: 0x17000188 RID: 392
		// (get) Token: 0x06001925 RID: 6437 RVA: 0x000A501F File Offset: 0x000A321F
		public bool HasMetal
		{
			get
			{
				CreatureData creatureData = this.data;
				if (creatureData == null || !creatureData.hasMetal)
				{
					Ragdoll ragdoll = this.ragdoll;
					return ragdoll != null && ragdoll.hasMetalArmor;
				}
				return true;
			}
		}

		// Token: 0x06001926 RID: 6438 RVA: 0x000A5048 File Offset: 0x000A3248
		public List<ValueDropdownItem<string>> GetAllCreatureID()
		{
			return Catalog.GetDropdownAllID(Category.Creature, "None");
		}

		// Token: 0x06001927 RID: 6439 RVA: 0x000A5055 File Offset: 0x000A3255
		public List<ValueDropdownItem<int>> GetAllFactionID()
		{
			if (Catalog.gameData == null)
			{
				return null;
			}
			return Catalog.gameData.GetFactions();
		}

		// Token: 0x06001928 RID: 6440 RVA: 0x000A506C File Offset: 0x000A326C
		protected void Awake()
		{
			this.detectionFOVModifier = new FloatHandler();
			this.hitEnvironmentDamageModifier = new FloatHandler();
			this.healthModifier = new FloatHandler();
			this.healthModifier.OnChangeEvent += this.OnMaxHealthModifierChangeEvent;
			SkinnedMeshRenderer[] componentsInChildren = base.GetComponentsInChildren<SkinnedMeshRenderer>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].updateWhenOffscreen = true;
			}
			if (!this.lodGroup)
			{
				this.lodGroup = base.GetComponentInChildren<LODGroup>();
			}
			if (!string.IsNullOrEmpty(this.creatureId) && this.creatureId != "None")
			{
				this.data = Catalog.GetData<CreatureData>(this.creatureId, true);
			}
			Creature.all.Add(this);
			this.ragdoll = base.GetComponentInChildren<Ragdoll>();
			this.brain = base.GetComponentInChildren<Brain>();
			this.equipment = base.GetComponentInChildren<Equipment>();
			if (!this.container)
			{
				this.container = base.GetComponentInChildren<Container>();
			}
			this.locomotion = base.GetComponent<Locomotion>();
			this.mana = base.GetComponent<Mana>();
			this.climber = base.GetComponentInChildren<FeetClimber>();
			this.airHelper = this.GetOrAddComponent<AirHelper>();
			this.holders = new List<Holder>(base.GetComponentsInChildren<Holder>());
			this.heldCrystalImbues = new HashSet<string>();
			this.heldImbueIDs = new HashSet<string>();
			this.jointForceMultipliers = new Dictionary<object, ValueTuple<float, float>>();
			this.damageMultipliers = new Dictionary<object, float>();
			foreach (RagdollHand hand in base.GetComponentsInChildren<RagdollHand>())
			{
				if (hand.side == Side.Right)
				{
					this.handRight = hand;
				}
				if (hand.side == Side.Left)
				{
					this.handLeft = hand;
				}
			}
			foreach (RagdollFoot foot in base.GetComponentsInChildren<RagdollFoot>())
			{
				if (foot.side == Side.Right)
				{
					this.footRight = foot;
				}
				if (foot.side == Side.Left)
				{
					this.footLeft = foot;
				}
			}
			this.lightVolumeReceiver = base.GetComponent<LightVolumeReceiver>();
			if (!this.lightVolumeReceiver)
			{
				this.lightVolumeReceiver = base.gameObject.AddComponent<LightVolumeReceiver>();
			}
			this.lightVolumeReceiver.initRenderersOnStart = false;
			this.lightVolumeReceiver.addMaterialInstances = false;
			if (!Creature.hashInitialized)
			{
				this.InitAnimatorHashs();
			}
			this.manikinLocations = base.GetComponentInChildren<ManikinLocations>();
			if (this.manikinLocations)
			{
				this.orgWardrobeLocations = this.manikinLocations.ToJson();
			}
			this.manikinParts = base.GetComponentInChildren<ManikinPartList>();
			this.manikinProperties = base.GetComponentInChildren<ManikinProperties>();
			this.mouthRelay = base.GetComponentInChildren<CreatureMouthRelay>();
			this.turnTargetAngle = base.transform.rotation.eulerAngles.y;
			this.stepTargetPos = base.transform.position;
			if (this.locomotion)
			{
				base.gameObject.layer = GameManager.GetLayer(LayerName.BodyLocomotion);
			}
			this.InitLocomotionAnimation();
			this.stepEnabled = true;
			this.animator.keepAnimatorStateOnDisable = true;
			this.waterHandler = new WaterHandler(true, true);
			this.waterHandler.OnWaterExit += this.OnWaterExit;
			if (this.equipment)
			{
				Equipment equipment = this.equipment;
				equipment.onCreaturePartChanged = (Equipment.OnCreaturePartChanged)Delegate.Combine(equipment.onCreaturePartChanged, new Equipment.OnCreaturePartChanged(this.UpdateManikinAfterHeadChange));
			}
		}

		// Token: 0x06001929 RID: 6441 RVA: 0x000A53A4 File Offset: 0x000A35A4
		protected void InitAnimatorHashs()
		{
			Creature.hashFeminity = Animator.StringToHash("Feminity");
			Creature.hashHeight = Animator.StringToHash("Height");
			Creature.hashFalling = Animator.StringToHash("Falling");
			Creature.hashUnderwater = Animator.StringToHash("Underwater");
			Creature.hashGetUp = Animator.StringToHash("GetUp");
			Creature.hashIsBusy = Animator.StringToHash("IsBusy");
			Creature.hashTstance = Animator.StringToHash("TStance");
			Creature.hashStaticIdle = Animator.StringToHash("StaticIdle");
			Creature.hashFreeHands = Animator.StringToHash("FreeHands");
			Creature.hashDynamicOneShot = Animator.StringToHash("DynamicOneShot");
			Creature.hashDynamicLoop = Animator.StringToHash("DynamicLoop");
			Creature.hashDynamicLoopAdd = Animator.StringToHash("DynamicLoopAdd");
			Creature.hashDynamicLoop3 = Animator.StringToHash("DynamicLoop3");
			Creature.hashDynamicInterrupt = Animator.StringToHash("DynamicInterrupt");
			Creature.hashDynamicSpeedMultiplier = Animator.StringToHash("DynamicSpeedMultiplier");
			Creature.hashDynamicMirror = Animator.StringToHash("DynamicMirror");
			Creature.hashDynamicUpperOneShot = Animator.StringToHash("UpperBodyDynamicOneShot");
			Creature.hashDynamicUpperLoop = Animator.StringToHash("UpperBodyDynamicLoop");
			Creature.hashDynamicUpperMultiplier = Animator.StringToHash("UpperBodyDynamicSpeed");
			Creature.hashDynamicUpperMirror = Animator.StringToHash("UpperBodyDynamicMirror");
			Creature.hashExitDynamic = Animator.StringToHash("ExitDynamic");
			Creature.hashInvokeCallback = Animator.StringToHash("InvokeCallback");
			Creature.hashInitialized = true;
		}

		// Token: 0x0600192A RID: 6442 RVA: 0x000A5501 File Offset: 0x000A3701
		public RagdollHand GetHand(Side side)
		{
			if (side == Side.Left)
			{
				return this.handLeft;
			}
			return this.handRight;
		}

		// Token: 0x0600192B RID: 6443 RVA: 0x000A5514 File Offset: 0x000A3714
		public RagdollFoot GetFoot(Side side)
		{
			if (side == Side.Left)
			{
				return this.footLeft;
			}
			return this.footRight;
		}

		// Token: 0x17000189 RID: 393
		// (get) Token: 0x0600192C RID: 6444 RVA: 0x000A5527 File Offset: 0x000A3727
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update | ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x0600192D RID: 6445 RVA: 0x000A552A File Offset: 0x000A372A
		protected internal override void ManagedUpdate()
		{
			if (!this.initialized)
			{
				return;
			}
			this.UpdateFall();
			this.UpdateWater();
			this.UpdateReveal();
			this.CheckInvokeDynamicCallback();
			this.UpdateLocomotionAnimation();
			this.UpdateDynamicAnimation();
			this.UpdateFacialAnimation();
		}

		// Token: 0x0600192E RID: 6446 RVA: 0x000A5560 File Offset: 0x000A3760
		public void ApplyAnimatorController(AnimatorBundle animatorBundle)
		{
			this.animator.applyRootMotion = false;
			this.animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
			this.animatorOverrideController = new AnimatorOverrideController(animatorBundle.controller);
			this.animatorOverrideController.name = "OverrideForDynamicAnimation";
			this.animator.runtimeAnimatorController = this.animatorOverrideController;
			if (Creature.clipIndex == null)
			{
				Creature.clipIndex = new Creature.ReplaceClipIndexHolder();
			}
			this.animationClipOverrides = new KeyValuePair<AnimationClip, AnimationClip>[Creature.clipIndex.count];
			int index = 0;
			foreach (AnimationClip clip in animatorBundle.GetAllClips())
			{
				if (clip == null)
				{
					Debug.LogError("Clip is null in " + animatorBundle.name);
				}
				this.animationClipOverrides[index] = new KeyValuePair<AnimationClip, AnimationClip>(clip, clip);
				index++;
			}
		}

		// Token: 0x0600192F RID: 6447 RVA: 0x000A5650 File Offset: 0x000A3850
		protected void ResetTPose()
		{
			Avatar avatar = this.animator.avatar;
			foreach (SkeletonBone sb in (avatar != null) ? avatar.humanDescription.skeleton : null)
			{
				foreach (object obj in Enum.GetValues(typeof(HumanBodyBones)))
				{
					HumanBodyBones hbb = (HumanBodyBones)obj;
					if (hbb != HumanBodyBones.LastBone)
					{
						Transform bone = this.animator.GetBoneTransform(hbb);
						if (bone != null && sb.name == bone.name)
						{
							bone.localPosition = sb.position;
							bone.localRotation = sb.rotation;
						}
					}
				}
			}
		}

		// Token: 0x06001930 RID: 6448 RVA: 0x000A5738 File Offset: 0x000A3938
		protected void UpdateReveal()
		{
			if (this.updateReveal)
			{
				this.updateReveal = false;
				for (int i = 0; i < this.renderers.Count; i++)
				{
					Creature.RendererData rendererData = this.renderers[i];
					bool flag;
					if (rendererData == null)
					{
						flag = false;
					}
					else
					{
						RevealDecal revealDecal = rendererData.revealDecal;
						bool? flag2 = (revealDecal != null) ? new bool?(revealDecal.UpdateOvertime()) : null;
						bool flag3 = true;
						flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
					}
					bool normalReveal = flag || this.updateReveal;
					Creature.RendererData rendererData2 = this.renderers[i];
					bool flag4;
					if (rendererData2 == null)
					{
						flag4 = false;
					}
					else
					{
						RevealDecal splitReveal2 = rendererData2.splitReveal;
						bool? flag2 = (splitReveal2 != null) ? new bool?(splitReveal2.UpdateOvertime()) : null;
						bool flag3 = true;
						flag4 = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
					}
					bool splitReveal = flag4 || this.updateReveal;
					this.updateReveal = (normalReveal || splitReveal);
				}
			}
		}

		// Token: 0x06001931 RID: 6449 RVA: 0x000A5828 File Offset: 0x000A3A28
		protected void CheckInvokeDynamicCallback()
		{
			if (this.animator.GetBool(Creature.hashInvokeCallback))
			{
				this.animator.SetBool(Creature.hashInvokeCallback, false);
				if (this.dynamicAnimationendEndCallback != null && this.animator.GetInteger(Creature.hashDynamicOneShot) == 0)
				{
					this.dynamicAnimationendEndCallback();
					this.dynamicAnimationendEndCallback = null;
				}
			}
		}

		// Token: 0x06001932 RID: 6450 RVA: 0x000A5884 File Offset: 0x000A3A84
		public virtual float GetAnimatorHeightRatio()
		{
			return this.animator.GetFloat(Creature.hashHeight);
		}

		// Token: 0x06001933 RID: 6451 RVA: 0x000A5896 File Offset: 0x000A3A96
		public void Heal(float healing)
		{
			this.Heal(healing, this);
		}

		// Token: 0x06001934 RID: 6452 RVA: 0x000A58A0 File Offset: 0x000A3AA0
		public bool TryAddSkill(string id)
		{
			return this.TryAddSkill(Catalog.GetData<SkillData>(id, true));
		}

		// Token: 0x06001935 RID: 6453 RVA: 0x000A58AF File Offset: 0x000A3AAF
		public bool TryRemoveSkill(string id)
		{
			return this.TryRemoveSkill(Catalog.GetData<SkillData>(id, true));
		}

		// Token: 0x06001936 RID: 6454 RVA: 0x000A58BE File Offset: 0x000A3ABE
		public void ResurrectMaxHealth()
		{
			this.Resurrect(this.maxHealth, this);
		}

		// Token: 0x06001937 RID: 6455 RVA: 0x000A58CD File Offset: 0x000A3ACD
		public void Resurrect(float healing)
		{
			this.Resurrect(healing, this);
		}

		// Token: 0x06001938 RID: 6456 RVA: 0x000A58D8 File Offset: 0x000A3AD8
		public void SetFaction(int factionId)
		{
			this.factionId = factionId;
			this.faction = Catalog.gameData.GetFaction(factionId);
			if (this.faction == null)
			{
				Debug.LogError("Faction ID " + factionId.ToString() + " could not be found for creature " + this.data.id);
				this.faction = Catalog.gameData.factions[0];
				this.factionId = Catalog.gameData.factions[0].id;
			}
		}

		// Token: 0x06001939 RID: 6457 RVA: 0x000A595C File Offset: 0x000A3B5C
		public void Damage(float amount)
		{
			this.Damage(amount, DamageType.Energy);
		}

		// Token: 0x0600193A RID: 6458 RVA: 0x000A5968 File Offset: 0x000A3B68
		public void Kill()
		{
			CollisionInstance collisionInstance = new CollisionInstance(new DamageStruct(DamageType.Energy, 99999f), null, null);
			this.Kill(collisionInstance);
		}

		// Token: 0x0600193B RID: 6459 RVA: 0x000A598F File Offset: 0x000A3B8F
		public void Despawn(float delay)
		{
			if (delay > 0f && !base.IsInvoking("Despawn"))
			{
				base.Invoke("Despawn", delay);
				return;
			}
			this.Despawn();
		}

		// Token: 0x0600193C RID: 6460 RVA: 0x000A59BC File Offset: 0x000A3BBC
		public override void Despawn()
		{
			base.Despawn();
			EventManager.InvokeCreatureDespawn(this, EventTime.OnStart);
			if (this.OnDespawnEvent != null)
			{
				this.OnDespawnEvent(EventTime.OnStart);
			}
			if (this.currentArea != null && this.currentArea.IsSpawned)
			{
				this.currentArea.SpawnedArea.UnRegisterCreature(this);
			}
			this.ClearMultipliers();
			this.currentArea = null;
			this.isCulled = false;
			if (this.initialArea != null && this.initialArea.isCreatureSpawnedExist != null && this.areaSpawnerIndex >= 0 && this.areaSpawnerIndex < this.initialArea.isCreatureSpawnedExist.Length)
			{
				this.initialArea.isCreatureSpawnedExist[this.areaSpawnerIndex] = false;
			}
			if (this.brain)
			{
				this.brain.Stop();
			}
			if (!this.wasLoadedForCharacterSelect)
			{
				this.UnloadSkills();
			}
			if (this.mana)
			{
				this.mana.casterLeft.UnloadSpell();
				this.mana.casterRight.UnloadSpell();
			}
			this.creatureSpawner = null;
			if (this.player)
			{
				this.player.ReleaseCreature();
			}
			for (int i = 0; i < this.revealDecals.Count; i++)
			{
				RevealDecal revealDecal = this.revealDecals[i];
				if (revealDecal != null)
				{
					revealDecal.Reset();
				}
			}
			if (this.ragdoll && this.ragdoll.parts != null)
			{
				int partsCount = this.ragdoll.parts.Count;
				for (int index = 0; index < partsCount; index++)
				{
					RagdollPart ragdollPart = this.ragdoll.parts[index];
					int handlesCount = ragdollPart.handles.Count;
					for (int j = 0; j < handlesCount; j++)
					{
						HandleRagdoll ragdollHandle = ragdollPart.handles[j];
						for (int k = ragdollHandle.handlers.Count - 1; k >= 0; k--)
						{
							ragdollHandle.handlers[k].UnGrab(false);
						}
					}
					for (int l = ragdollPart.collisionHandler.penetratedObjects.Count - 1; l >= 0; l--)
					{
						foreach (Damager damager in ragdollPart.collisionHandler.penetratedObjects[l].damagers)
						{
							damager.UnPenetrateAll();
						}
					}
				}
			}
			if (this.equipment)
			{
				this.equipment.OnDespawn();
			}
			if (this.ragdoll)
			{
				this.ragdoll.OnDespawn();
			}
			foreach (Creature.RendererData smrInfo in this.renderers)
			{
				smrInfo.splitRenderer = null;
				if (smrInfo.revealDecal)
				{
					smrInfo.revealDecal.Reset();
				}
			}
			Effect[] componentsInChildren = base.GetComponentsInChildren<Effect>(true);
			for (int m = 0; m < componentsInChildren.Length; m++)
			{
				componentsInChildren[m].Despawn();
			}
			Creature.allActive.Remove(this);
			if (Creature.onAllActiveRemoved != null)
			{
				Creature.onAllActiveRemoved(this);
			}
			this.turnRelativeToHand = true;
			this.isKilled = false;
			this.spawnGroup = null;
			this.loaded = false;
			this.animator.keepAnimatorStateOnDisable = false;
			if (this.pooled)
			{
				if (this.data.removeMeshWhenPooled && this.manikinLocations)
				{
					this.manikinLocations.RemoveAll();
					this.manikinLocations.UpdateParts();
				}
				this.Hide(false);
				CreatureData.ReturnToPool(this);
			}
			else
			{
				if (this.manikinLocations)
				{
					this.manikinLocations.RemoveAll();
					this.manikinLocations.UpdateParts();
				}
				base.gameObject.SetActive(false);
				Catalog.ReleaseAsset<GameObject>(base.gameObject);
			}
			Creature.DespawnEvent onDespawnEvent = this.OnDespawnEvent;
			if (onDespawnEvent != null)
			{
				onDespawnEvent(EventTime.OnEnd);
			}
			EventManager.InvokeCreatureDespawn(this, EventTime.OnEnd);
		}

		// Token: 0x0600193D RID: 6461 RVA: 0x000A5DC8 File Offset: 0x000A3FC8
		public static int CompareByLastInteractionTime(Creature c1, Creature c2)
		{
			return c1.lastInteractionTime.CompareTo(c2.lastInteractionTime);
		}

		// Token: 0x0600193E RID: 6462 RVA: 0x000A5DDC File Offset: 0x000A3FDC
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			Creature.allActive.Add(this);
			if (AreaManager.Instance != null)
			{
				this.cullingDetectionEnabled = true;
			}
			if (this.ragdoll)
			{
				this.ragdoll.OnCreatureEnable();
			}
			if (this.brain)
			{
				this.brain.OnCreatureEnable();
			}
		}

		// Token: 0x0600193F RID: 6463 RVA: 0x000A5E40 File Offset: 0x000A4040
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			if (GameManager.isQuitting)
			{
				return;
			}
			Creature.allActive.Remove(this);
			if (Creature.onAllActiveRemoved != null)
			{
				Creature.onAllActiveRemoved(this);
			}
			this.cullingDetectionEnabled = false;
			if (this.brain)
			{
				this.brain.OnCreatureDisable();
			}
			if (this.ragdoll)
			{
				this.ragdoll.OnCreatureDisable();
			}
			this.waterHandler.Reset();
		}

		// Token: 0x06001940 RID: 6464 RVA: 0x000A5EBB File Offset: 0x000A40BB
		private void OnDestroy()
		{
			Creature.all.Remove(this);
		}

		// Token: 0x06001941 RID: 6465 RVA: 0x000A5EC9 File Offset: 0x000A40C9
		protected override void Start()
		{
			base.Start();
			if (!this.initialized)
			{
				this.Init();
			}
		}

		// Token: 0x06001942 RID: 6466 RVA: 0x000A5EE0 File Offset: 0x000A40E0
		protected virtual void Init()
		{
			base.Load(this.data);
			this.ragdoll.Init(this);
			if (this.manikinParts)
			{
				this.manikinParts.disableRenderersDuringUpdate = true;
				this.manikinParts.UpdateParts_Completed += this.OnManikinChangedEvent;
			}
			else
			{
				this.UpdateRenderers();
			}
			this.InitCenterEyes();
			this.morphology = new Morphology(base.transform.InverseTransformPoint(this.centerEyes.position).y);
			if (this.footRight)
			{
				this.footRight.Init();
			}
			if (this.footLeft)
			{
				this.footLeft.Init();
			}
			this.RefreshMorphology();
			this.locomotion.SetCapsuleCollider(this.morphology.hipsHeight);
			if (this.climber)
			{
				this.climber.Init();
			}
			if (this.handRight)
			{
				this.handRight.ResetGripPositionAndRotation();
			}
			if (this.handLeft)
			{
				this.handLeft.ResetGripPositionAndRotation();
			}
			if (this.equipment)
			{
				this.equipment.Init(this);
			}
			if (this.mana)
			{
				this.mana.Init(this);
			}
			if (this.brain)
			{
				this.brain.Init(this);
			}
			this.autoEyeClipsActive = true;
			this.initialized = true;
			this.weakpoints = new List<Transform>();
		}

		// Token: 0x06001943 RID: 6467 RVA: 0x000A6064 File Offset: 0x000A4264
		public void InitCenterEyes()
		{
			if (!this.centerEyes)
			{
				Transform leftEye = this.animator.GetBoneTransform(HumanBodyBones.LeftEye);
				Transform rightEye = this.animator.GetBoneTransform(HumanBodyBones.RightEye);
				if (leftEye && rightEye)
				{
					this.centerEyes = new GameObject("CenterEyes").transform;
					this.centerEyes.SetParent(this.ragdoll.headPart.transform);
					this.centerEyes.position = (leftEye.position + rightEye.position) / 2f;
					this.centerEyes.rotation = base.transform.rotation;
					return;
				}
				Debug.LogErrorFormat(this, "Cannot create centerEyes because HumanBodyBones LeftEye and RightEye are missing!", Array.Empty<object>());
			}
		}

		// Token: 0x06001944 RID: 6468 RVA: 0x000A612A File Offset: 0x000A432A
		public void SetSwimVertical(float ratio)
		{
			this.swimVerticalRatio = ratio;
		}

		// Token: 0x06001945 RID: 6469 RVA: 0x000A6134 File Offset: 0x000A4334
		private void UpdateWater()
		{
			this.isSwimming = false;
			if (this.waterHandler != null)
			{
				if (Water.exist)
				{
					RagdollFoot ragdollFoot = this.ragdoll.creature.footLeft;
					object obj = (ragdollFoot != null) ? ragdollFoot.transform.position : base.transform.position;
					RagdollFoot ragdollFoot2 = this.ragdoll.creature.footRight;
					Vector3 rightFootPos = (ragdollFoot2 != null) ? ragdollFoot2.transform.position : base.transform.position;
					object obj2 = obj;
					Vector3 feetPosition = Vector3.Lerp(obj2, rightFootPos, 0.5f);
					float feetPositionY = Mathf.Min(obj2.y, rightFootPos.y);
					this.waterHandler.Update(feetPosition, feetPositionY, this.centerEyes.position.y, this.locomotion.colliderRadius, this.ragdoll.IsPhysicsEnabled(false) ? this.ragdoll.rootPart.physicBody.velocity : this.currentLocomotion.physicBody.velocity);
					if (this.waterHandler.inWater)
					{
						this.isSwimming = (Catalog.gameData.water.swimmingEnabled && this.waterHandler.submergedRatio >= this.swimFallAnimationRatio);
						float dragMultiplier = this.data.waterLocomotionDragMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
						bool isPlayerFloating = !this.eyesUnderwater && this.waterHandler.submergedRatio > 0.8f;
						this.currentLocomotion.SetPhysicModifier(this, new float?(isPlayerFloating ? 0.0001f : Mathf.Lerp(1f, Catalog.gameData.water.minGravityLocomotion, this.waterHandler.submergedRatio)), -1f, dragMultiplier, -1);
						this.currentLocomotion.SetSpeedModifier(this, 1f, 1f, 1f, 1f, this.data.waterJumpForceMutiplierCurve.Evaluate(this.waterHandler.submergedRatio), 1f);
						if (this.isSwimming)
						{
							float fullSubmersionRatio = Mathf.Clamp01(Mathf.Clamp01(this.waterHandler.submergedRatio - this.swimFallAnimationRatio) / Mathf.Clamp01(1f - this.swimFallAnimationRatio));
							if (Mathf.Abs(this.swimVerticalRatio) > 0f && (!isPlayerFloating || this.swimVerticalRatio < 0f))
							{
								this.currentLocomotion.physicBody.AddForce(Vector3.up * (this.data.waterSwimUpForce * this.swimVerticalRatio * fullSubmersionRatio * Time.deltaTime), ForceMode.VelocityChange);
							}
						}
						if (this.waterHandler.submergedRatio >= 1f)
						{
							if (!this.eyesUnderwater)
							{
								this.waterEyesEnterUnderwaterTime = Time.time;
								this.eyesUnderwater = true;
								this.brain.isMuffled = true;
								Action action = this.onEyesEnterUnderwater;
								if (action != null)
								{
									action();
								}
							}
						}
						else if (this.eyesUnderwater)
						{
							this.eyesUnderwater = false;
							this.brain.isMuffled = this.CheckMuffled();
							Action action2 = this.onEyesExitUnderwater;
							if (action2 != null)
							{
								action2();
							}
						}
						if (this.state != Creature.State.Dead && this.eyesUnderwater && (!this.player || !Player.invincibility) && Time.time - this.waterEyesEnterUnderwaterTime > this.data.waterDrowningStartTime * this.data.waterDrowningWarningRatio && Time.time - this.waterLastDrowningTime > this.data.waterDrowningDamageInterval)
						{
							EffectData drownEffectData = this.data.drownEffectData;
							EffectInstance effectInstance = (drownEffectData != null) ? drownEffectData.Spawn(this.jaw.position, this.jaw.rotation, this.jaw, null, true, null, false, 1f, 1f, Array.Empty<Type>()) : null;
							if (effectInstance != null)
							{
								effectInstance.Play(0, false, false);
							}
							if (Time.time - this.waterEyesEnterUnderwaterTime > this.data.waterDrowningStartTime)
							{
								CollisionInstance collisionInstance = new CollisionInstance(new DamageStruct(DamageType.Energy, this.data.waterDrowningDamage), null, null);
								this.Damage(collisionInstance);
							}
							this.waterLastDrowningTime = Time.time;
						}
					}
				}
				else
				{
					this.waterHandler.Reset();
					if (this.eyesUnderwater)
					{
						this.eyesUnderwater = false;
						this.brain.isMuffled = this.CheckMuffled();
						Action action3 = this.onEyesExitUnderwater;
						if (action3 != null)
						{
							action3();
						}
					}
				}
			}
			this.animator.SetBool(Creature.hashUnderwater, this.isSwimming);
		}

		// Token: 0x06001946 RID: 6470 RVA: 0x000A65A0 File Offset: 0x000A47A0
		private void OnWaterExit()
		{
			this.currentLocomotion.RemovePhysicModifier(this);
			this.currentLocomotion.RemoveSpeedModifier(this);
		}

		// Token: 0x06001947 RID: 6471 RVA: 0x000A65BC File Offset: 0x000A47BC
		private bool CheckMuffled()
		{
			for (int i = 0; i < this.ragdoll.handlers.Count; i++)
			{
				HandleRagdoll handleRagdoll = this.ragdoll.handlers[i].grabbedHandle as HandleRagdoll;
				if (handleRagdoll != null && (handleRagdoll.handleRagdollData.behaviour == HandleRagdollData.Behaviour.Muffle || handleRagdoll.handleRagdollData.behaviour == HandleRagdollData.Behaviour.ChokeAndMuffle))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001948 RID: 6472 RVA: 0x000A6624 File Offset: 0x000A4824
		private void ParentEyes()
		{
			foreach (CreatureEye creatureEye in this.allEyes)
			{
				foreach (CreatureEye.EyeMoveable eyeMoveable in creatureEye.eyeParts)
				{
					eyeMoveable.ParentingFix();
				}
			}
		}

		// Token: 0x06001949 RID: 6473 RVA: 0x000A66B0 File Offset: 0x000A48B0
		private void SetupManikin(PlayerSaveData playerCharacterData = null)
		{
			if (!this.manikinProperties)
			{
				Debug.LogWarning("ManikinProperties is missing on " + base.name);
				return;
			}
			List<Color> list;
			if (playerCharacterData == null)
			{
				if (this.data.ethnicGroups.Count > 0)
				{
					CreatureData.EthnicGroup ethnicGroup = this.GetEthnicGroupFromId(this.data.ethnicityId);
					this.SetEthnicGroup(ethnicGroup);
					list = ethnicGroup.hairColorsPrimary;
					Color hairColorsPrimary = (list != null && list.Count > 0) ? ethnicGroup.hairColorsPrimary[0] : this.data.PickHairColorPrimary(ethnicGroup, -1, true);
					list = ethnicGroup.hairColorsSecondary;
					Color hairColorsSecondary = (list != null && list.Count > 0) ? ethnicGroup.hairColorsSecondary[0] : this.data.PickHairColorSecondary(ethnicGroup, -1, true);
					list = ethnicGroup.hairColorsSpecular;
					Color hairColorsSpecular = (list != null && list.Count > 0) ? ethnicGroup.hairColorsSpecular[0] : this.data.PickHairColorSecondary(ethnicGroup, -1, true);
					this.SetColor(hairColorsPrimary, Creature.ColorModifier.Hair, false);
					this.SetColor(hairColorsSecondary, Creature.ColorModifier.HairSecondary, false);
					this.SetColor(hairColorsSpecular, Creature.ColorModifier.HairSpecular, false);
					list = ethnicGroup.eyesColorsIris;
					Color eyesColorIris = (list != null && list.Count > 0) ? ethnicGroup.eyesColorsIris[0] : this.data.PickEyesColorIris(ethnicGroup, -1, true);
					list = ethnicGroup.eyesColorsSclera;
					Color eyesColorSclera = (list != null && list.Count > 0) ? ethnicGroup.eyesColorsSclera[0] : this.data.PickEyesColorSclera(ethnicGroup, -1, true);
					this.SetColor(eyesColorIris, Creature.ColorModifier.EyesIris, false);
					this.SetColor(eyesColorSclera, Creature.ColorModifier.EyesSclera, false);
					this.SetColor(this.data.PickSkinColor(ethnicGroup, -1, true, false), Creature.ColorModifier.Skin, false);
				}
				return;
			}
			CreatureData.EthnicGroup ethnicGroup2 = this.GetEthnicGroupFromId(playerCharacterData.customization.ethnicGroupId);
			this.SetEthnicGroup(ethnicGroup2);
			if (playerCharacterData.customization.hairColor.a > 0f)
			{
				this.SetColor(playerCharacterData.customization.hairColor, Creature.ColorModifier.Hair, false);
				this.SetColor(playerCharacterData.customization.hairSecondaryColor, Creature.ColorModifier.HairSecondary, false);
				this.SetColor(playerCharacterData.customization.hairSpecularColor, Creature.ColorModifier.HairSpecular, false);
			}
			else
			{
				list = ethnicGroup2.hairColorsPrimary;
				Color hairColorsPrimary2 = (list != null && list.Count > 0) ? ethnicGroup2.hairColorsPrimary[0] : this.data.PickHairColorPrimary(ethnicGroup2, -1, true);
				list = ethnicGroup2.hairColorsSecondary;
				Color hairColorsSecondary2 = (list != null && list.Count > 0) ? ethnicGroup2.hairColorsSecondary[0] : this.data.PickHairColorSecondary(ethnicGroup2, -1, true);
				list = ethnicGroup2.hairColorsSpecular;
				Color hairColorsSpecular2 = (list != null && list.Count > 0) ? ethnicGroup2.hairColorsSpecular[0] : this.data.PickHairColorSecondary(ethnicGroup2, -1, true);
				this.SetColor(hairColorsPrimary2, Creature.ColorModifier.Hair, false);
				this.SetColor(hairColorsSpecular2, Creature.ColorModifier.HairSecondary, false);
				this.SetColor(hairColorsSecondary2, Creature.ColorModifier.HairSpecular, false);
			}
			if (playerCharacterData.customization.eyesIrisColor.a > 0f)
			{
				this.SetColor(playerCharacterData.customization.eyesIrisColor, Creature.ColorModifier.EyesIris, false);
				this.SetColor(playerCharacterData.customization.eyesScleraColor, Creature.ColorModifier.EyesSclera, false);
			}
			else
			{
				list = ethnicGroup2.eyesColorsIris;
				Color eyesColorIris2 = (list != null && list.Count > 0) ? ethnicGroup2.eyesColorsIris[0] : this.data.PickEyesColorIris(ethnicGroup2, -1, true);
				list = ethnicGroup2.eyesColorsSclera;
				Color eyesColorSclera2 = (list != null && list.Count > 0) ? ethnicGroup2.eyesColorsSclera[0] : this.data.PickEyesColorSclera(ethnicGroup2, -1, true);
				this.SetColor(eyesColorIris2, Creature.ColorModifier.EyesIris, false);
				this.SetColor(eyesColorSclera2, Creature.ColorModifier.EyesSclera, false);
			}
			if (playerCharacterData.customization.skinColor.a > 0f)
			{
				this.SetColor(playerCharacterData.customization.skinColor, Creature.ColorModifier.Skin, false);
				return;
			}
			list = ethnicGroup2.skinColors;
			this.SetColor((list != null && list.Count > 0) ? ethnicGroup2.skinColors[0] : this.data.PickSkinColor(ethnicGroup2, -1, true, false), Creature.ColorModifier.Skin, false);
		}

		// Token: 0x0600194A RID: 6474 RVA: 0x000A6AB8 File Offset: 0x000A4CB8
		private void LoadContainer(PlayerSaveData playerCharacterData = null, bool clearLinkedHolders = false)
		{
			if (!this.container)
			{
				return;
			}
			if (clearLinkedHolders)
			{
				this.container.ClearLinkedHolders();
			}
			if (playerCharacterData != null)
			{
				List<ContainerContent> inventory = playerCharacterData.CloneInventory();
				if (!inventory.IsNullOrEmpty())
				{
					this.container.spawnOwner = Item.Owner.Player;
					this.container.Load(inventory);
					this.container.containerID = this.data.containerID;
					return;
				}
			}
			this.container.spawnOwner = Item.Owner.None;
			this.container.loadContent = Container.LoadContent.ContainerID;
			this.container.containerID = this.data.containerID;
			this.container.Load();
		}

		// Token: 0x0600194B RID: 6475 RVA: 0x000A6B5C File Offset: 0x000A4D5C
		public void LoadDefaultSkills()
		{
			foreach (SkillData skill in Catalog.GetDataList<SkillData>())
			{
				if (skill.allowSkill && skill.isDefaultSkill && !this.HasSkill(skill))
				{
					SpellData spell = skill as SpellData;
					if (spell != null)
					{
						this.container.AddSpellContent(spell);
					}
					else
					{
						this.container.AddSkillContent(skill);
					}
				}
			}
		}

		// Token: 0x0600194C RID: 6476 RVA: 0x000A6BE8 File Offset: 0x000A4DE8
		public void LoadSkills()
		{
			if (this.container == null)
			{
				return;
			}
			foreach (SpellContent skill in this.container.contents.GetEnumerableContentsOfType(true, Array.Empty<Func<SpellContent, bool>>()))
			{
				if (skill.data.allowSkill)
				{
					try
					{
						skill.data.OnSkillLoaded(skill.data, this);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"Error loading skill ",
							skill.data.id,
							" for ",
							this.data.id,
							", skipping. Exception below."
						}));
						Debug.LogException(e);
					}
				}
			}
			foreach (SkillContent skill2 in this.container.contents.GetEnumerableContentsOfType(true, Array.Empty<Func<SkillContent, bool>>()))
			{
				if (skill2.data.allowSkill)
				{
					try
					{
						skill2.data.OnSkillLoaded(skill2.data, this);
					}
					catch (Exception e2)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"Error loading skill ",
							skill2.data.id,
							" for ",
							this.data.id,
							", skipping. Exception below."
						}));
						Debug.LogException(e2);
					}
				}
			}
			foreach (SpellContent skill3 in this.container.contents.GetEnumerableContentsOfType(true, Array.Empty<Func<SpellContent, bool>>()))
			{
				if (skill3.data.allowSkill)
				{
					try
					{
						skill3.data.OnLateSkillsLoaded(skill3.data, this);
					}
					catch (Exception e3)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"Error late loading skill ",
							skill3.data.id,
							" for ",
							this.data.id,
							", skipping. Exception below."
						}));
						Debug.LogException(e3);
					}
				}
			}
			foreach (SkillContent skill4 in this.container.contents.GetEnumerableContentsOfType(true, Array.Empty<Func<SkillContent, bool>>()))
			{
				if (skill4.data.allowSkill)
				{
					try
					{
						skill4.data.OnLateSkillsLoaded(skill4.data, this);
					}
					catch (Exception e4)
					{
						Debug.LogError(string.Concat(new string[]
						{
							"Error late loading skill ",
							skill4.data.id,
							" for ",
							this.data.id,
							", skipping. Exception below."
						}));
						Debug.LogException(e4);
					}
				}
			}
			try
			{
				if (this.mana)
				{
					this.mana.InvokeOnSpellLoad(this.handLeft.caster.telekinesis, this.handLeft.caster);
					this.mana.InvokeOnSpellLoad(this.handRight.caster.telekinesis, this.handRight.caster);
				}
			}
			catch (Exception e5)
			{
				Debug.LogError(string.Format("Error loading telekinesis for {0}: {1}", this.data.id, e5));
			}
		}

		// Token: 0x0600194D RID: 6477 RVA: 0x000A6FB0 File Offset: 0x000A51B0
		public void LogSkills()
		{
			List<string> skills = new List<string>();
			foreach (SkillContent skill in this.container.contents.GetEnumerableContentsOfType(false, Array.Empty<Func<SkillContent, bool>>()))
			{
				skills.Add(skill.data.id);
			}
			if (skills.Count == 0)
			{
				Debug.Log("Player has no skills");
				return;
			}
			Debug.Log("Loaded player skills:\n - " + string.Join("\n - ", skills));
		}

		// Token: 0x0600194E RID: 6478 RVA: 0x000A704C File Offset: 0x000A524C
		public void UnloadSkills()
		{
			if (this.container == null)
			{
				return;
			}
			foreach (SkillContent skill in this.container.contents.GetEnumerableContentsOfType(true, Array.Empty<Func<SkillContent, bool>>()))
			{
				try
				{
					skill.data.OnSkillUnloaded(skill.data, this);
				}
				catch (NullReferenceException exception)
				{
					Debug.LogError("Caught NullReferenceException while unloading " + skill.data.id + ", skipping. Exception below.");
					Debug.LogException(exception);
				}
			}
		}

		// Token: 0x0600194F RID: 6479 RVA: 0x000A70F8 File Offset: 0x000A52F8
		public bool ForceLoadSkill(string id)
		{
			SkillData skillData;
			if (!Catalog.TryGetData<SkillData>(id, out skillData, true))
			{
				return false;
			}
			skillData.OnSkillLoaded(skillData, this);
			skillData.OnLateSkillsLoaded(skillData, this);
			Creature.ForceSkillLoadEvent onForceSkillLoadEvent = this.OnForceSkillLoadEvent;
			if (onForceSkillLoadEvent != null)
			{
				onForceSkillLoadEvent(skillData);
			}
			return true;
		}

		// Token: 0x06001950 RID: 6480 RVA: 0x000A7138 File Offset: 0x000A5338
		public bool ForceUnloadSkill(string id)
		{
			SkillData skillData;
			if (!Catalog.TryGetData<SkillData>(id, out skillData, true))
			{
				return false;
			}
			skillData.OnSkillUnloaded(skillData, this);
			Creature.ForceSkillLoadEvent onForceSkillUnloadEvent = this.OnForceSkillUnloadEvent;
			if (onForceSkillUnloadEvent != null)
			{
				onForceSkillUnloadEvent(skillData);
			}
			return true;
		}

		// Token: 0x06001951 RID: 6481 RVA: 0x000A716D File Offset: 0x000A536D
		public bool TryAddSkill(SkillData skill)
		{
			if (skill == null)
			{
				return false;
			}
			if (this.HasSkill(skill))
			{
				return true;
			}
			Container container = this.container;
			if (container != null)
			{
				container.AddSkillContent(skill);
			}
			return true;
		}

		// Token: 0x06001952 RID: 6482 RVA: 0x000A7193 File Offset: 0x000A5393
		public bool TryRemoveSkill(SkillData skill)
		{
			if (skill == null)
			{
				return false;
			}
			if (!this.HasSkill(skill))
			{
				return true;
			}
			Container container = this.container;
			if (container != null)
			{
				container.RemoveContent(skill.id, 0, true);
			}
			return true;
		}

		// Token: 0x06001953 RID: 6483 RVA: 0x000A71BF File Offset: 0x000A53BF
		public bool HasSkill(SkillData skill)
		{
			if (skill == null)
			{
				return false;
			}
			Container container = this.container;
			return container != null && container.HasSkillContent(skill);
		}

		// Token: 0x06001954 RID: 6484 RVA: 0x000A71D8 File Offset: 0x000A53D8
		public bool HasSkill(string id)
		{
			Container container = this.container;
			return container != null && container.HasSkillContent(id);
		}

		// Token: 0x06001955 RID: 6485 RVA: 0x000A71EC File Offset: 0x000A53EC
		public bool TryGetSkill<T>(string id, out T skillData) where T : SkillData
		{
			skillData = default(T);
			Container container = this.container;
			return container != null && container.TryGetSkillContent<T>(id, out skillData);
		}

		// Token: 0x06001956 RID: 6486 RVA: 0x000A7208 File Offset: 0x000A5408
		public bool TryGetSkill<T>(SkillData data, out T skillData) where T : SkillData
		{
			skillData = default(T);
			Container container = this.container;
			return container != null && container.TryGetSkillContent<T>(data, out skillData);
		}

		// Token: 0x06001957 RID: 6487 RVA: 0x000A7224 File Offset: 0x000A5424
		public int CountSkillsOfTree(string tree, bool includeNonCore = false, bool includeDual = false)
		{
			int count = 0;
			for (int i = 0; i < this.container.contents.Count; i++)
			{
				SkillContent skillContent = this.container.contents[i] as SkillContent;
				SkillData skill = (skillContent != null) ? skillContent.data : null;
				if (skill != null && !skill.isDefaultSkill && skill.showInTree && (skill.primarySkillTreeId == tree || skill.secondarySkillTreeId == tree) && (includeDual || !skill.IsCombinedSkill) && (includeNonCore || skill.isTierBlocker))
				{
					count++;
				}
			}
			return count;
		}

		// Token: 0x06001958 RID: 6488 RVA: 0x000A72C0 File Offset: 0x000A54C0
		public void AddForce(Vector3 force, ForceMode forceMode, float nonRootMultiplier = 1f, CollisionHandler hitHandler = null)
		{
			if (this.isPlayer)
			{
				this.player.locomotion.physicBody.AddForce(force, forceMode);
				return;
			}
			List<RagdollPart> parts2;
			if (hitHandler != null)
			{
				RagdollPart ragdollPart = hitHandler.ragdollPart;
				if (ragdollPart != null)
				{
					Ragdoll.Region region = ragdollPart.ragdollRegion;
					parts2 = region.parts;
					goto IL_4B;
				}
			}
			parts2 = this.ragdoll.parts;
			IL_4B:
			List<RagdollPart> parts = parts2;
			for (int i = 0; i < parts.Count; i++)
			{
				RagdollPart part = parts[i];
				if (!part.isSliced)
				{
					PhysicBody physicBody = part.physicBody;
					if (physicBody != null)
					{
						physicBody.AddForce(force * ((part == this.ragdoll.rootPart) ? 1f : nonRootMultiplier), forceMode);
					}
				}
			}
		}

		// Token: 0x06001959 RID: 6489 RVA: 0x000A7374 File Offset: 0x000A5574
		public override void AddForce(Vector3 force, ForceMode forceMode, CollisionHandler hitHandler = null)
		{
			base.AddForce(force, forceMode, hitHandler);
			if (this.isPlayer)
			{
				this.player.locomotion.physicBody.AddForce(force, forceMode);
				return;
			}
			List<RagdollPart> parts2;
			if (hitHandler != null)
			{
				RagdollPart ragdollPart = hitHandler.ragdollPart;
				if (ragdollPart != null)
				{
					Ragdoll.Region region = ragdollPart.ragdollRegion;
					parts2 = region.parts;
					goto IL_52;
				}
			}
			parts2 = this.ragdoll.parts;
			IL_52:
			List<RagdollPart> parts = parts2;
			for (int i = 0; i < parts.Count; i++)
			{
				RagdollPart part = parts[i];
				if (!part.isSliced)
				{
					PhysicBody physicBody = part.physicBody;
					if (physicBody != null)
					{
						physicBody.AddForce(force, forceMode);
					}
				}
			}
		}

		// Token: 0x0600195A RID: 6490 RVA: 0x000A7410 File Offset: 0x000A5610
		public override void AddRadialForce(float force, Vector3 origin, float upwardsModifier, ForceMode forceMode, CollisionHandler hitHandler = null)
		{
			if (this.isPlayer)
			{
				this.player.locomotion.physicBody.AddRadialForce(force, origin, upwardsModifier, forceMode);
				return;
			}
			List<RagdollPart> parts2;
			if (hitHandler != null)
			{
				RagdollPart ragdollPart = hitHandler.ragdollPart;
				if (ragdollPart != null)
				{
					Ragdoll.Region region = ragdollPart.ragdollRegion;
					parts2 = region.parts;
					goto IL_4E;
				}
			}
			parts2 = this.ragdoll.parts;
			IL_4E:
			List<RagdollPart> parts = parts2;
			for (int i = 0; i < parts.Count; i++)
			{
				PhysicBody physicBody = parts[i].physicBody;
				if (physicBody != null)
				{
					physicBody.AddRadialForce(force, origin, upwardsModifier, forceMode);
				}
			}
		}

		// Token: 0x0600195B RID: 6491 RVA: 0x000A749C File Offset: 0x000A569C
		public override void AddExplosionForce(float force, Vector3 origin, float radius, float upwardsModifier, ForceMode forceMode, CollisionHandler hitHandler = null)
		{
			base.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode, null);
			if (this.isPlayer)
			{
				this.player.locomotion.physicBody.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode);
				return;
			}
			List<RagdollPart> parts2;
			if (hitHandler != null)
			{
				RagdollPart ragdollPart = hitHandler.ragdollPart;
				if (ragdollPart != null)
				{
					Ragdoll.Region region = ragdollPart.ragdollRegion;
					parts2 = region.parts;
					goto IL_5E;
				}
			}
			parts2 = this.ragdoll.parts;
			IL_5E:
			List<RagdollPart> parts = parts2;
			for (int i = 0; i < parts.Count; i++)
			{
				PhysicBody physicBody = parts[i].physicBody;
				if (physicBody != null)
				{
					physicBody.AddRadialForce(force, origin, upwardsModifier, forceMode);
				}
			}
		}

		// Token: 0x0600195C RID: 6492 RVA: 0x000A7538 File Offset: 0x000A5738
		public void InvokeOnThrowEvent(RagdollHand hand, Handle handle)
		{
			Creature.ThrowEvent onThrowEvent = this.OnThrowEvent;
			if (onThrowEvent == null)
			{
				return;
			}
			onThrowEvent(hand, handle);
		}

		// Token: 0x0600195D RID: 6493 RVA: 0x000A754C File Offset: 0x000A574C
		public void InvokeThisCreatureAttackEvent(Creature targetCreature, Transform targetTransform, BrainModuleAttack.AttackType type, BrainModuleAttack.AttackStage stage)
		{
			Creature.ThisCreatureAttackEvent onThisCreatureAttackEvent = this.OnThisCreatureAttackEvent;
			if (onThisCreatureAttackEvent == null)
			{
				return;
			}
			onThisCreatureAttackEvent(targetCreature, targetTransform, type, stage);
		}

		// Token: 0x0600195E RID: 6494 RVA: 0x000A7563 File Offset: 0x000A5763
		public void OnMaxHealthModifierChangeEvent(float oldValue, float newValue)
		{
			this.currentHealth = this.currentHealth / oldValue * newValue;
		}

		// Token: 0x0600195F RID: 6495 RVA: 0x000A7575 File Offset: 0x000A5775
		public void InvokeHealthChangeEvent(float health, float maxHealth)
		{
			if (this.currentHealth > maxHealth)
			{
				this.currentHealth = maxHealth;
			}
			Creature.HealthChangeEvent onHealthChange = this.OnHealthChange;
			if (onHealthChange == null)
			{
				return;
			}
			onHealthChange(health, maxHealth);
		}

		// Token: 0x06001960 RID: 6496 RVA: 0x000A759C File Offset: 0x000A579C
		private void SetupData(CreatureData data, PlayerSaveData playerCharacterData = null, bool characterSelect = false)
		{
			Creature.SetupDataEvent onSetupDataEvent = this.OnSetupDataEvent;
			if (onSetupDataEvent != null)
			{
				onSetupDataEvent(EventTime.OnStart);
			}
			this.SetFaction(data.factionId);
			this.maxHealth = (float)data.health;
			this.currentHealth = this.maxHealth;
			this.countTowardsMaxAlive = data.countTowardsMaxAlive;
			this.eyeClips.Clear();
			if (!string.IsNullOrEmpty(data.animatorBundleAddress))
			{
				AnimatorBundle controllerBundle;
				if (Creature.creatureAnimatorControllers.TryGetValue(data.animatorBundleAddress, out controllerBundle))
				{
					this.ApplyAnimatorController(controllerBundle);
				}
				else
				{
					Debug.LogError("Controller with address \"" + data.animatorBundleAddress + "\" did not load!");
				}
			}
			else
			{
				Debug.LogWarning("Field \"animatorControllerAddress\" is not set in CreatureData! If this creature should have dynamic animations, fix this or it will not work!");
			}
			this.fallAliveDestabilizeHeight = data.ragdollData.fallAliveDestabilizeHeight;
			this.fallAliveAnimationHeight = data.ragdollData.fallAliveAnimationHeight;
			this.groundStabilizationMinDuration = data.ragdollData.groundStabilizationMinDuration;
			this.groundStabilizationMaxVelocity = data.ragdollData.groundStabilizationMaxVelocity;
			this.ragdoll.Load(data);
			this.LoadContainer(playerCharacterData, characterSelect);
			if (this.equipment)
			{
				this.equipment.Load(characterSelect);
			}
			this.wasLoadedForCharacterSelect = characterSelect;
			if (!characterSelect && this.mana)
			{
				this.mana.Load();
			}
			this.SetupManikin(playerCharacterData);
			this.locomotion.Init();
			if (characterSelect)
			{
				return;
			}
			base.SetVariable<bool>("IsPlayer", true);
			if (playerCharacterData != null)
			{
				this.LoadDefaultSkills();
			}
			this.LoadSkills();
			if (playerCharacterData != null)
			{
				this.LogSkills();
			}
			this.currentHealth = this.maxHealth;
			Creature.SetupDataEvent onSetupDataEvent2 = this.OnSetupDataEvent;
			if (onSetupDataEvent2 == null)
			{
				return;
			}
			onSetupDataEvent2(EventTime.OnEnd);
		}

		// Token: 0x06001961 RID: 6497 RVA: 0x000A7732 File Offset: 0x000A5932
		public virtual void Load(PlayerSaveData playerCharacterData = null, bool characterSelect = false)
		{
			this.Load(this.data, playerCharacterData, characterSelect);
		}

		// Token: 0x06001962 RID: 6498 RVA: 0x000A7742 File Offset: 0x000A5942
		public virtual IEnumerator LoadCoroutine(CreatureData data, PlayerSaveData playerCharacterData = null, bool characterSelect = false)
		{
			if (!this.initialized)
			{
				this.Init();
			}
			this.ClearMultipliers();
			this.data = data;
			Coroutine brainLoad = base.StartCoroutine(this.brain.LoadCoroutine(data.brainId));
			this.SetupData(data, playerCharacterData, characterSelect);
			yield return brainLoad;
			this.ParentEyes();
			this.ragdoll.UpdateMetalArmor();
			this.loaded = true;
			Creature.SimpleDelegate onDataLoaded = this.OnDataLoaded;
			if (onDataLoaded != null)
			{
				onDataLoaded();
			}
			yield return null;
			yield break;
		}

		// Token: 0x06001963 RID: 6499 RVA: 0x000A7766 File Offset: 0x000A5966
		public void SetRandomEthnicGroup()
		{
			this.SetEthnicGroup(this.data.ethnicGroups[UnityEngine.Random.Range(0, this.data.ethnicGroups.Count)]);
		}

		// Token: 0x06001964 RID: 6500 RVA: 0x000A7794 File Offset: 0x000A5994
		public void SetEthnicGroupFromId(string ethnicGroupId = "Eradian")
		{
			this.SetEthnicGroup(this.GetEthnicGroupFromId(ethnicGroupId));
		}

		// Token: 0x06001965 RID: 6501 RVA: 0x000A77A4 File Offset: 0x000A59A4
		public virtual void Load(CreatureData data, PlayerSaveData playerCharacterData = null, bool characterSelect = false)
		{
			if (!this.initialized)
			{
				this.Init();
			}
			this.data = data;
			this.ClearMultipliers();
			this.SetupData(data, playerCharacterData, characterSelect);
			this.brain.Load(data.brainId);
			this.loaded = true;
			this.ParentEyes();
			this.ragdoll.UpdateMetalArmor();
			Creature.SimpleDelegate onDataLoaded = this.OnDataLoaded;
			if (onDataLoaded == null)
			{
				return;
			}
			onDataLoaded();
		}

		// Token: 0x06001966 RID: 6502 RVA: 0x000A780E File Offset: 0x000A5A0E
		private void InitLocomotionAnimation()
		{
			Creature.hashStrafe = Animator.StringToHash("Strafe");
			Creature.hashTurn = Animator.StringToHash("Turn");
			Creature.hashSpeed = Animator.StringToHash("Speed");
			Creature.hashVerticalSpeed = Animator.StringToHash("VerticalSpeed");
		}

		// Token: 0x06001967 RID: 6503 RVA: 0x000A784C File Offset: 0x000A5A4C
		private void UpdateLocomotionAnimation()
		{
			if (this.locomotion.capsuleCollider.enabled == this.isKilled)
			{
				this.locomotion.capsuleCollider.enabled = !this.isKilled;
			}
			Brain brain = this.brain;
			if (((brain != null) ? brain.navMeshAgent : null) != null && !this.brain.navMeshAgent.enabled)
			{
				this.brain.navMeshAgent.enabled = this.loaded;
			}
			Vector3 moveLocalVelocity = base.transform.InverseTransformDirection(this.currentLocomotion.velocity);
			if ((this.currentLocomotion.isGrounded && this.currentLocomotion.horizontalSpeed + Mathf.Abs(this.currentLocomotion.angularSpeed) > this.stationaryVelocityThreshold) || this.animator.GetBool(Creature.hashUnderwater))
			{
				float clampedVertical = this.currentLocomotion.isGrounded ? 0f : Mathf.Clamp(0.2f + moveLocalVelocity.y * (1f / base.transform.lossyScale.y), -0.6f, 0.7f);
				this.animator.SetFloat(Creature.hashVerticalSpeed, clampedVertical, this.animationDampTime, Time.deltaTime);
				float mult = Mathf.Lerp(1f, 0.3f, Mathf.Pow(Mathf.Clamp01(clampedVertical / -0.6f), 3f));
				this.animator.SetFloat(Creature.hashSpeed, moveLocalVelocity.z * (1f / base.transform.lossyScale.z) * mult, this.animationDampTime, Time.deltaTime);
				this.animator.SetFloat(Creature.hashStrafe, moveLocalVelocity.x * (1f / base.transform.lossyScale.x) * mult, this.animationDampTime, Time.deltaTime);
				this.animator.SetFloat(Creature.hashTurn, this.currentLocomotion.angularSpeed * (1f / base.transform.lossyScale.y) * this.turnAnimSpeed, this.animationDampTime, Time.deltaTime);
				return;
			}
			this.animator.SetFloat(Creature.hashStrafe, 0f, this.animationDampTime, Time.deltaTime);
			this.animator.SetFloat(Creature.hashTurn, 0f, this.animationDampTime, Time.deltaTime);
			this.animator.SetFloat(Creature.hashSpeed, 0f, this.animationDampTime, Time.deltaTime);
			float clampedVertical2 = Mathf.Clamp(0.2f + moveLocalVelocity.y * (1f / base.transform.lossyScale.y), -0.6f, 0.7f);
			this.animator.SetFloat(Creature.hashVerticalSpeed, clampedVertical2, this.verticalDampTime, Time.deltaTime);
		}

		// Token: 0x06001968 RID: 6504 RVA: 0x000A7B1C File Offset: 0x000A5D1C
		public void AnimatorMoveUpdate()
		{
			if (this.animator.applyRootMotion)
			{
				base.transform.rotation = this.animator.rootRotation;
				base.transform.position += this.animator.deltaPosition;
				this.animator.transform.localPosition = Vector3.zero;
				this.animator.transform.localRotation = Quaternion.identity;
				this.ragdoll.AnimatorMoveUpdate();
			}
		}

		// Token: 0x06001969 RID: 6505 RVA: 0x000A7BA4 File Offset: 0x000A5DA4
		protected internal override void ManagedLateUpdate()
		{
			if (!this.cullingDetectionEnabled)
			{
				return;
			}
			if (AreaManager.Instance == null)
			{
				return;
			}
			if (!this.initialized || !this.loaded)
			{
				return;
			}
			if (this.equipment && this.equipment.GetPendingApparelLoading() > 0)
			{
				return;
			}
			using (List<Holder>.Enumerator enumerator = this.holders.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.spawningItem)
					{
						return;
					}
				}
			}
			if (Time.time - this.cullingDetectionCycleTime < this.cullingDetectionCycleSpeed)
			{
				return;
			}
			this.cullingDetectionCycleTime = Time.time;
			if (this.currentArea == null)
			{
				SpawnableArea areaFound = AreaManager.Instance.CurrentArea.FindRecursive(this.ragdoll.rootPart.transform.position);
				if (areaFound != null)
				{
					this.currentArea = areaFound;
					this.SetCull(!this.currentArea.IsSpawned || this.currentArea.SpawnedArea.isCulled);
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.RegisterCreature(this);
					}
				}
			}
			else
			{
				SpawnableArea areaFound2 = this.currentArea.FindRecursive(this.ragdoll.rootPart.transform.position);
				if (areaFound2 == null)
				{
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.UnRegisterCreature(this);
					}
					this.currentArea = areaFound2;
				}
				else if (this.currentArea != areaFound2)
				{
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.UnRegisterCreature(this);
					}
					this.currentArea = areaFound2;
					this.SetCull(!this.currentArea.IsSpawned || this.currentArea.SpawnedArea.isCulled);
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.RegisterCreature(this);
					}
				}
			}
			if (this.currentArea == AreaManager.Instance.CurrentArea)
			{
				this.Hide(false);
			}
		}

		// Token: 0x0600196A RID: 6506 RVA: 0x000A7DBC File Offset: 0x000A5FBC
		public void SetCull(bool cull)
		{
			if (this.isPlayer)
			{
				return;
			}
			if (this.isCulled == cull)
			{
				return;
			}
			base.gameObject.SetActive(!cull);
			this.isCulled = cull;
			if (!this.isCulled && this.manikinProperties)
			{
				this.manikinProperties.UpdateProperties();
			}
		}

		// Token: 0x0600196B RID: 6507 RVA: 0x000A7E14 File Offset: 0x000A6014
		public void SetColor(Color color, Creature.ColorModifier colorModifier, bool updateProperties = false)
		{
			if (this.manikinProperties)
			{
				ManikinProperty manikinProperty6;
				if (colorModifier == Creature.ColorModifier.Hair)
				{
					ManikinProperty manikinProperty;
					if (this.TryGetManikinProperty("HairColor", out manikinProperty))
					{
						this.manikinProperties.TryUpdateProperty(color, false, manikinProperty.set, false, 0);
					}
				}
				else if (colorModifier == Creature.ColorModifier.HairSecondary)
				{
					ManikinProperty manikinProperty2;
					if (this.TryGetManikinProperty("HairSecondaryColor", out manikinProperty2))
					{
						this.manikinProperties.TryUpdateProperty(color, false, manikinProperty2.set, false, 0);
					}
				}
				else if (colorModifier == Creature.ColorModifier.HairSpecular)
				{
					ManikinProperty manikinProperty3;
					if (this.TryGetManikinProperty("HairSpecularColor", out manikinProperty3))
					{
						this.manikinProperties.TryUpdateProperty(color, false, manikinProperty3.set, false, 0);
					}
				}
				else if (colorModifier == Creature.ColorModifier.EyesIris)
				{
					ManikinProperty manikinProperty4;
					if (this.TryGetManikinProperty("EyeIrisColor", out manikinProperty4))
					{
						this.manikinProperties.TryUpdateProperty(color, false, manikinProperty4.set, false, 0);
					}
				}
				else if (colorModifier == Creature.ColorModifier.EyesSclera)
				{
					ManikinProperty manikinProperty5;
					if (this.TryGetManikinProperty("EyeScleraColor", out manikinProperty5))
					{
						this.manikinProperties.TryUpdateProperty(color, false, manikinProperty5.set, false, 0);
					}
				}
				else if (colorModifier == Creature.ColorModifier.Skin && this.TryGetManikinProperty("SkinColor", out manikinProperty6))
				{
					this.manikinProperties.TryUpdateProperty(color, false, manikinProperty6.set, false, 0);
				}
				if (updateProperties)
				{
					this.manikinProperties.UpdateProperties();
				}
			}
		}

		// Token: 0x0600196C RID: 6508 RVA: 0x000A7F54 File Offset: 0x000A6154
		public Color GetColor(Creature.ColorModifier colorModifier)
		{
			if (this.manikinProperties)
			{
				ManikinProperty manikinProperty6;
				if (colorModifier == Creature.ColorModifier.Hair)
				{
					ManikinProperty manikinProperty;
					if (this.TryGetManikinProperty("HairColor", out manikinProperty))
					{
						return new Color(manikinProperty.values[0], manikinProperty.values[1], manikinProperty.values[2], manikinProperty.values[3]);
					}
				}
				else if (colorModifier == Creature.ColorModifier.HairSecondary)
				{
					ManikinProperty manikinProperty2;
					if (this.TryGetManikinProperty("HairSecondaryColor", out manikinProperty2))
					{
						return new Color(manikinProperty2.values[0], manikinProperty2.values[1], manikinProperty2.values[2], manikinProperty2.values[3]);
					}
				}
				else if (colorModifier == Creature.ColorModifier.HairSpecular)
				{
					ManikinProperty manikinProperty3;
					if (this.TryGetManikinProperty("HairSpecularColor", out manikinProperty3))
					{
						return new Color(manikinProperty3.values[0], manikinProperty3.values[1], manikinProperty3.values[2], manikinProperty3.values[3]);
					}
				}
				else if (colorModifier == Creature.ColorModifier.EyesIris)
				{
					ManikinProperty manikinProperty4;
					if (this.TryGetManikinProperty("EyeIrisColor", out manikinProperty4))
					{
						return new Color(manikinProperty4.values[0], manikinProperty4.values[1], manikinProperty4.values[2], manikinProperty4.values[3]);
					}
				}
				else if (colorModifier == Creature.ColorModifier.EyesSclera)
				{
					ManikinProperty manikinProperty5;
					if (this.TryGetManikinProperty("EyeScleraColor", out manikinProperty5))
					{
						return new Color(manikinProperty5.values[0], manikinProperty5.values[1], manikinProperty5.values[2], manikinProperty5.values[3]);
					}
				}
				else if (colorModifier == Creature.ColorModifier.Skin && this.TryGetManikinProperty("SkinColor", out manikinProperty6))
				{
					return new Color(manikinProperty6.values[0], manikinProperty6.values[1], manikinProperty6.values[2], manikinProperty6.values[3]);
				}
			}
			Debug.LogWarning("Can't retrieve creature " + colorModifier.ToString() + ", manikin property maybe missing");
			return Color.white;
		}

		// Token: 0x0600196D RID: 6509 RVA: 0x000A8100 File Offset: 0x000A6300
		public bool TryGetManikinProperty(string name, out ManikinProperty manikinProperty)
		{
			return this.TryGetManikinProperty(name, this.manikinProperties, out manikinProperty);
		}

		// Token: 0x0600196E RID: 6510 RVA: 0x000A8110 File Offset: 0x000A6310
		public bool TryGetManikinProperty(string name, ManikinProperties properties, out ManikinProperty manikinProperty)
		{
			foreach (ManikinProperty mp in properties.properties)
			{
				if (mp.set.name == name)
				{
					manikinProperty = mp;
					return true;
				}
			}
			manikinProperty = default(ManikinProperty);
			return false;
		}

		// Token: 0x0600196F RID: 6511 RVA: 0x000A815E File Offset: 0x000A635E
		public bool IsFromWave()
		{
			return this.spawnGroup != null;
		}

		// Token: 0x06001970 RID: 6512 RVA: 0x000A816C File Offset: 0x000A636C
		public bool IsEnemy(Creature creatureTarget, bool aliveOnly = false)
		{
			return !(creatureTarget == this) && (!aliveOnly || creatureTarget.state != Creature.State.Dead) && this.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Passive && this.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && creatureTarget.faction.attackBehaviour != GameData.Faction.AttackBehaviour.Ignored && (this.faction.attackBehaviour == GameData.Faction.AttackBehaviour.Agressive || this.factionId != creatureTarget.factionId);
		}

		// Token: 0x06001971 RID: 6513 RVA: 0x000A81E0 File Offset: 0x000A63E0
		public bool IsVisible()
		{
			using (List<Creature.RendererData>.Enumerator enumerator = this.renderers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.renderer.isVisible)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001972 RID: 6514 RVA: 0x000A8240 File Offset: 0x000A6440
		public void Hide(bool hide)
		{
			if (this.isPlayer)
			{
				return;
			}
			if (!hide && this.manikinParts && this.manikinParts.disableRenderersDuringUpdate && this.manikinParts.PendingHandles() > 0)
			{
				this.hidden = false;
				return;
			}
			foreach (Creature.RendererData smrInfo in this.renderers)
			{
				if (smrInfo.splitRenderer)
				{
					smrInfo.splitRenderer.enabled = !hide;
				}
				else if (smrInfo.renderer)
				{
					smrInfo.renderer.enabled = !hide;
				}
			}
			if (hide)
			{
				using (List<Holder>.Enumerator enumerator2 = this.holders.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Holder holder = enumerator2.Current;
						foreach (Item item in holder.items)
						{
							item.Hide(true);
						}
					}
					goto IL_118;
				}
			}
			if (!this.holsterItemsHidden)
			{
				this.HideItemsInHolders(false);
			}
			IL_118:
			this.hidden = hide;
		}

		// Token: 0x06001973 RID: 6515 RVA: 0x000A8394 File Offset: 0x000A6594
		public void HideItemsInHolders(bool hide)
		{
			foreach (Holder holder in this.holders)
			{
				foreach (Item item in holder.items)
				{
					item.Hide(hide);
				}
			}
			this.holsterItemsHidden = hide;
		}

		// Token: 0x06001974 RID: 6516 RVA: 0x000A8428 File Offset: 0x000A6628
		public virtual void OnManikinChangedEvent(ManikinPart[] partsAdded)
		{
			this.UpdateRenderers();
			this.Hide(this.hidden);
			int wearableSlotsCount = this.equipment.wearableSlots.Count;
			for (int i = 0; i < wearableSlotsCount; i++)
			{
				Wearable equipmentWearableSlot = this.equipment.wearableSlots[i];
				if (!equipmentWearableSlot.IsEmpty())
				{
					foreach (Wearable.WearableEntry wearableEntry in equipmentWearableSlot.wardrobeLayers)
					{
						ItemContent worn = equipmentWearableSlot.GetEquipmentOnLayer(wearableEntry.layer);
						ItemModuleWardrobe wardrobe;
						int layerIndex;
						if (worn != null && worn.data.TryGetModule<ItemModuleWardrobe>(out wardrobe) && equipmentWearableSlot.TryGetLayerIndex(wearableEntry.layer, out layerIndex))
						{
							List<Renderer> wornPartRenderers = equipmentWearableSlot.GetWornPartRenderers(layerIndex);
							if (wornPartRenderers != null)
							{
								int count = wornPartRenderers.Count;
								for (int k = 0; k < count; k++)
								{
									Renderer renderer = wornPartRenderers[k];
									if (!(renderer == null))
									{
										switch (wardrobe.castShadows)
										{
										case ItemModuleWardrobe.CastShadows.None:
											renderer.shadowCastingMode = ShadowCastingMode.Off;
											break;
										case ItemModuleWardrobe.CastShadows.PlayerOnly:
											renderer.shadowCastingMode = (this.isPlayer ? ShadowCastingMode.On : ShadowCastingMode.Off);
											break;
										case ItemModuleWardrobe.CastShadows.PlayerAndNPC:
											renderer.shadowCastingMode = ShadowCastingMode.On;
											break;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06001975 RID: 6517 RVA: 0x000A856C File Offset: 0x000A676C
		public Renderer GetRendererForVFX()
		{
			if (this.vfxRenderer)
			{
				return this.vfxRenderer;
			}
			foreach (Creature.RendererData rd in this.renderers)
			{
				if (rd.renderer.name.IndexOf("vfx", StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return rd.renderer;
				}
			}
			return null;
		}

		// Token: 0x06001976 RID: 6518 RVA: 0x000A85F4 File Offset: 0x000A67F4
		public bool HaveRendererData(SkinnedMeshRenderer skinnedMeshRenderer)
		{
			using (List<Creature.RendererData>.Enumerator enumerator = this.renderers.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (enumerator.Current.renderer == skinnedMeshRenderer)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001977 RID: 6519 RVA: 0x000A8654 File Offset: 0x000A6854
		public void UpdateRenderers()
		{
			this.renderers = new List<Creature.RendererData>();
			this.revealDecals = new List<RevealDecal>();
			if (this.manikinParts)
			{
				ManikinPart[] allParts = this.manikinParts.GetAllParts(false);
				foreach (ManikinPart manikinPart in allParts)
				{
					MeshPart[] meshParts = manikinPart.GetComponents<MeshPart>();
					MeshPart meshPart = (meshParts.Length != 0) ? meshParts[0] : null;
					if (meshParts.Length > 1)
					{
						Debug.LogError("Creature " + base.name + " has more than one mesh part on manikin part " + manikinPart.name);
					}
					foreach (SkinnedMeshRenderer smr in manikinPart.GetComponentsInChildren<SkinnedMeshRenderer>())
					{
						if (!(smr.sharedMesh == null) && !this.HaveRendererData(smr))
						{
							int lod = -1;
							if (manikinPart is ManikinGroupPart)
							{
								lod = this.GetLODFromManikinGroupPart(manikinPart as ManikinGroupPart, smr);
							}
							bool isSamePart = meshPart && meshPart.skinnedMeshRenderer != null && (meshPart.skinnedMeshRenderer == smr || smr.transform.parent == meshPart.skinnedMeshRenderer.transform.parent);
							RevealDecal revealDecal = smr.GetComponent<RevealDecal>();
							this.renderers.Add(new Creature.RendererData(smr, lod, isSamePart ? meshPart : null, revealDecal, manikinPart));
							if (revealDecal)
							{
								this.revealDecals.Add(revealDecal);
							}
						}
					}
				}
				this.UpdatePartsBlendShapes(allParts);
			}
			else
			{
				if (this.lodGroup)
				{
					LOD[] lods = this.lodGroup.GetLODs();
					for (int i = 0; i < lods.Length; i++)
					{
						foreach (Renderer renderer in lods[i].renderers)
						{
							if (renderer is SkinnedMeshRenderer)
							{
								SkinnedMeshRenderer smr2 = renderer as SkinnedMeshRenderer;
								if (!(smr2.sharedMesh == null) && !this.HaveRendererData(smr2))
								{
									MeshPart meshPart2 = smr2.GetComponentInParent<MeshPart>();
									meshPart2 = ((meshPart2 && meshPart2.skinnedMeshRenderer == smr2) ? meshPart2 : null);
									RevealDecal revealDecal2 = smr2.GetComponent<RevealDecal>();
									this.renderers.Add(new Creature.RendererData(smr2, i, meshPart2, revealDecal2, null));
									if (revealDecal2)
									{
										this.revealDecals.Add(revealDecal2);
									}
								}
							}
						}
					}
				}
				foreach (SkinnedMeshRenderer smr3 in base.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					if (!(smr3.sharedMesh == null) && !this.HaveRendererData(smr3))
					{
						MeshPart meshPart3 = smr3.GetComponentInParent<MeshPart>();
						meshPart3 = ((meshPart3 && meshPart3.skinnedMeshRenderer == smr3) ? meshPart3 : null);
						RevealDecal revealDecal3 = smr3.GetComponent<RevealDecal>();
						this.renderers.Add(new Creature.RendererData(smr3, -1, meshPart3, revealDecal3, null));
						if (revealDecal3)
						{
							this.revealDecals.Add(revealDecal3);
						}
					}
				}
			}
			foreach (RagdollPart ragdollPart in this.ragdoll.parts)
			{
				ragdollPart.UpdateRenderers();
			}
			if (LightProbeVolume.Exists)
			{
				List<Renderer> tmpRenderers = new List<Renderer>();
				foreach (Creature.RendererData rendererData in this.renderers)
				{
					tmpRenderers.Add(rendererData.renderer);
				}
				this.lightVolumeReceiver.SetRenderers(tmpRenderers, false);
				this.lightVolumeReceiver.UpdateRenderers();
			}
			this.RefreshRenderers();
		}

		/// <summary>
		/// Update the current parts blends shapes from the current wardrobes
		/// </summary>
		/// <param name="allParts"></param>
		// Token: 0x06001978 RID: 6520 RVA: 0x000A8A38 File Offset: 0x000A6C38
		private void UpdatePartsBlendShapes(ManikinPart[] allParts)
		{
			ManikinWardrobeData[] wardrobesData = this.GetCurrentWardrobesData();
			if (wardrobesData == null)
			{
				return;
			}
			if (allParts == null)
			{
				return;
			}
			for (int i = 0; i < allParts.Length; i++)
			{
				allParts[i].UpdateBlendShapes(wardrobesData);
			}
		}

		/// <summary>
		/// Return current wardrobes data by fetching into layers.
		/// </summary>
		/// <returns>wardrobes data</returns>
		// Token: 0x06001979 RID: 6521 RVA: 0x000A8A6C File Offset: 0x000A6C6C
		private ManikinWardrobeData[] GetCurrentWardrobesData()
		{
			if (!this.manikinLocations)
			{
				return null;
			}
			HashSet<ManikinWardrobeData> wardrobesData = new HashSet<ManikinWardrobeData>();
			foreach (KeyValuePair<ManikinLocations.LocationKey, ManikinWardrobeData> partLocation in this.manikinLocations.partLocations)
			{
				wardrobesData.Add(partLocation.Value);
			}
			return wardrobesData.ToArray<ManikinWardrobeData>();
		}

		// Token: 0x0600197A RID: 6522 RVA: 0x000A8AE8 File Offset: 0x000A6CE8
		public void RefreshRenderers()
		{
			if (this.equipment && this.equipment.GetPendingApparelLoading() > 0)
			{
				return;
			}
			this.headManikinPart.Clear();
			if (this.manikinLocations)
			{
				this.manikinLocations.GetPartsAtChannel("Head", this.headManikinPart);
			}
			int renderersCount = this.renderers.Count;
			for (int i = 0; i < renderersCount; i++)
			{
				Creature.RendererData rendererData = this.renderers[i];
				if ((this.headManikinPart.Count > 0 && this.headManikinPart.Contains(rendererData.manikinPart)) || this.meshesToHideForFPV.Contains(rendererData.renderer))
				{
					rendererData.renderer.gameObject.layer = GameManager.GetLayer(this.player ? LayerName.FPVHide : LayerName.NPC);
				}
				else
				{
					rendererData.renderer.gameObject.layer = GameManager.GetLayer(this.player ? LayerName.Avatar : LayerName.NPC);
				}
				rendererData.renderer.updateWhenOffscreen = this.ShouldUpdateWhenOffscreen(rendererData);
				RagdollPart torsoPart = this.ragdoll.GetPart(RagdollPart.Type.Torso);
				rendererData.renderer.probeAnchor = (torsoPart ? torsoPart.transform : this.ragdoll.rootPart.transform);
				if (rendererData.splitRenderer)
				{
					rendererData.splitRenderer.gameObject.layer = GameManager.GetLayer(this.player ? LayerName.Avatar : LayerName.NPC);
					rendererData.splitRenderer.updateWhenOffscreen = this.ShouldUpdateWhenOffscreen(rendererData);
				}
			}
		}

		// Token: 0x0600197B RID: 6523 RVA: 0x000A8C84 File Offset: 0x000A6E84
		public int GetLODFromManikinGroupPart(ManikinGroupPart manikinGroupPart, Renderer renderer)
		{
			for (int i = 0; i < manikinGroupPart.partLODs.Count; i++)
			{
				using (List<Renderer>.Enumerator enumerator = manikinGroupPart.partLODs[i].renderers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current == renderer)
						{
							return i;
						}
					}
				}
			}
			return -1;
		}

		// Token: 0x0600197C RID: 6524 RVA: 0x000A8D00 File Offset: 0x000A6F00
		public bool ShouldUpdateWhenOffscreen(Creature.RendererData rendererData)
		{
			return rendererData.lod <= QualitySettings.maximumLODLevel && rendererData.renderer.gameObject.layer != GameManager.GetLayer(LayerName.FPVHide) && (this.player || this.state != Creature.State.Alive || this.ragdoll.isGrabbed);
		}

		// Token: 0x0600197D RID: 6525 RVA: 0x000A8D60 File Offset: 0x000A6F60
		public void UpdateHeldImbues()
		{
			HashSet<string> before = this.heldImbueIDs;
			HashSet<string> after = new HashSet<string>();
			this.heldCrystalImbues.Clear();
			Handle grabbedHandle = this.handLeft.grabbedHandle;
			Item left = (grabbedHandle != null) ? grabbedHandle.item : null;
			if (left != null)
			{
				for (int i = 0; i < left.imbues.Count; i++)
				{
					if (left.imbues[i].spellCastBase != null)
					{
						if (left.imbues[i].colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal)
						{
							this.heldCrystalImbues.Add(left.imbues[i].spellCastBase.id);
						}
						after.Add(left.imbues[i].spellCastBase.id);
					}
				}
			}
			Handle grabbedHandle2 = this.handRight.grabbedHandle;
			Item right = (grabbedHandle2 != null) ? grabbedHandle2.item : null;
			if (right != null)
			{
				for (int j = 0; j < right.imbues.Count; j++)
				{
					if (right.imbues[j].spellCastBase != null)
					{
						if (right.imbues[j].colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal)
						{
							this.heldCrystalImbues.Add(right.imbues[j].spellCastBase.id);
						}
						after.Add(right.imbues[j].spellCastBase.id);
					}
				}
			}
			this.heldImbueIDs = after;
			Creature.ImbueChangeEvent onHeldImbueChange = this.OnHeldImbueChange;
			if (onHeldImbueChange == null)
			{
				return;
			}
			onHeldImbueChange(this, before, after);
		}

		/// <summary>
		/// Make a creature drop whatever they are holding
		/// </summary>
		/// <param name="creature"></param>
		// Token: 0x0600197E RID: 6526 RVA: 0x000A8EFD File Offset: 0x000A70FD
		public static void DisarmCreature(Creature creature)
		{
			if (creature == null)
			{
				return;
			}
			Creature.DisarmCreature(creature, Side.Left);
			Creature.DisarmCreature(creature, Side.Right);
		}

		/// <summary>
		/// Make a creature drop whatever they are holding in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		// Token: 0x0600197F RID: 6527 RVA: 0x000A8F17 File Offset: 0x000A7117
		public static void DisarmCreature(Creature creature, Side side)
		{
			if (creature == null)
			{
				return;
			}
			if (side == Side.Left)
			{
				if (Creature.IsCreatureGrabbingHandle(creature, side))
				{
					creature.handLeft.UnGrab(false);
					return;
				}
			}
			else if (Creature.IsCreatureGrabbingHandle(creature, side))
			{
				creature.handRight.UnGrab(false);
			}
		}

		/// <summary>
		/// Returns true if the creature is holding something
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		// Token: 0x06001980 RID: 6528 RVA: 0x000A8F52 File Offset: 0x000A7152
		public static bool IsCreatureGrabbingHandle(Creature creature)
		{
			return Creature.IsCreatureGrabbingHandle(creature, Side.Left) || Creature.IsCreatureGrabbingHandle(creature, Side.Right);
		}

		/// <summary>
		/// Returns true if the creature is holding something in a particular side
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="side"></param>
		/// <returns></returns>
		// Token: 0x06001981 RID: 6529 RVA: 0x000A8F66 File Offset: 0x000A7166
		public static bool IsCreatureGrabbingHandle(Creature creature, Side side)
		{
			if (side == Side.Left)
			{
				return ((creature != null) ? creature.handLeft.grabbedHandle : null) != null;
			}
			return ((creature != null) ? creature.handRight.grabbedHandle : null) != null;
		}

		// Token: 0x06001982 RID: 6530 RVA: 0x000A8F9C File Offset: 0x000A719C
		public static List<Creature> InRadius(Vector3 position, float radius, Func<Creature, bool> filter = null, List<Creature> allocList = null)
		{
			if (allocList == null)
			{
				allocList = new List<Creature>();
			}
			float sqrRadius = radius * radius;
			for (int i = 0; i < Creature.allActive.Count; i++)
			{
				Creature entity = Creature.allActive[i];
				if ((filter == null || filter(entity)) && (position - entity.ClosestPoint(position)).sqrMagnitude <= sqrRadius)
				{
					allocList.Add(entity);
				}
			}
			return allocList;
		}

		// Token: 0x06001983 RID: 6531 RVA: 0x000A9008 File Offset: 0x000A7208
		public static List<Creature> InRadiusNaive(Vector3 position, float radius, Func<Creature, bool> filter = null, List<Creature> allocList = null)
		{
			if (allocList == null)
			{
				allocList = new List<Creature>();
			}
			float sqrRadius = radius * radius;
			for (int i = 0; i < Creature.allActive.Count; i++)
			{
				Creature entity = Creature.allActive[i];
				if ((filter == null || filter(entity)) && (position - entity.Center).sqrMagnitude <= sqrRadius)
				{
					allocList.Add(entity);
				}
			}
			return allocList;
		}

		// Token: 0x06001984 RID: 6532 RVA: 0x000A9072 File Offset: 0x000A7272
		public static ThunderEntity AimAssist(Vector3 position, Vector3 direction, float maxDistance, float maxAngle, out Transform targetPoint, Func<Creature, bool> filter = null, CreatureType weakpointFilter = (CreatureType)0, Creature ignoredCreature = null, float minDistance = 0.1f)
		{
			return Creature.AimAssist(new Ray(position, direction), maxDistance, maxAngle, out targetPoint, filter, weakpointFilter, ignoredCreature, minDistance);
		}

		// Token: 0x06001985 RID: 6533 RVA: 0x000A908C File Offset: 0x000A728C
		public static ThunderEntity AimAssist(Ray ray, float maxDistance, float maxAngle, out Transform targetPoint, Func<Creature, bool> filter = null, CreatureType weakpointFilter = (CreatureType)0, Creature ignoredCreature = null, float minDistance = 0.1f)
		{
			float sqrMinDistance = minDistance * minDistance;
			float sqrMaxDistance = maxDistance * maxDistance;
			float largestRightSqrDistance = float.PositiveInfinity;
			ThunderEntity outputCreature = null;
			targetPoint = null;
			for (int i = 0; i < Creature.allActive.Count; i++)
			{
				Creature creature = Creature.allActive[i];
				if ((filter == null || filter(creature)) && !(creature == ignoredCreature))
				{
					Vector3 toCreature = creature.Center - ray.origin;
					if (weakpointFilter.HasFlag(creature.data.type) && creature != null)
					{
						List<Transform> weakpoints = creature.weakpoints;
						if (weakpoints != null && weakpoints.Count > 0)
						{
							for (int j = 0; j < creature.weakpoints.Count; j++)
							{
								Transform weakpoint = creature.weakpoints[j];
								Vector3 toWeakpoint = weakpoint.position - ray.origin;
								if (toWeakpoint.sqrMagnitude <= sqrMaxDistance && toWeakpoint.sqrMagnitude >= sqrMinDistance && Vector3.Angle(toWeakpoint, ray.direction) <= maxAngle)
								{
									float distance = toWeakpoint.magnitude;
									float rightSqrDistance = (ray.GetPoint(distance) - weakpoint.position).sqrMagnitude;
									if (rightSqrDistance < largestRightSqrDistance)
									{
										largestRightSqrDistance = rightSqrDistance;
										outputCreature = creature;
										targetPoint = weakpoint;
									}
								}
							}
							goto IL_1B0;
						}
					}
					if (toCreature.sqrMagnitude <= sqrMaxDistance && toCreature.sqrMagnitude >= sqrMinDistance && Vector3.Angle(toCreature, ray.direction) <= maxAngle)
					{
						float distance2 = toCreature.magnitude;
						float rightSqrDistance2 = (ray.GetPoint(distance2) - creature.ClosestPoint(ray.GetPoint(distance2))).sqrMagnitude;
						if (rightSqrDistance2 < largestRightSqrDistance)
						{
							largestRightSqrDistance = rightSqrDistance2;
							outputCreature = creature;
						}
					}
				}
				IL_1B0:;
			}
			if (targetPoint == null)
			{
				Creature creature2 = (Creature)outputCreature;
				targetPoint = ((creature2 != null) ? creature2.ragdoll.targetPart.transform : null);
			}
			if (!(Golem.local == null))
			{
				Golem golem = Golem.local;
				if (golem != null && !golem.isKilled)
				{
					Vector3 toGolem = golem.Center - ray.origin;
					if (weakpointFilter.HasFlag(CreatureType.Golem))
					{
						List<Transform> weakpoints = golem.weakpoints;
						if (weakpoints != null && weakpoints.Count > 0)
						{
							for (int k = 0; k < golem.weakpoints.Count; k++)
							{
								Transform weakpoint2 = golem.weakpoints[k];
								Vector3 toWeakpoint2 = weakpoint2.position - ray.origin;
								if (toWeakpoint2.sqrMagnitude <= sqrMaxDistance && toWeakpoint2.sqrMagnitude >= sqrMinDistance && Vector3.Angle(toWeakpoint2, ray.direction) <= maxAngle)
								{
									float distance3 = toWeakpoint2.magnitude;
									float rightSqrDistance3 = (ray.GetPoint(distance3) - weakpoint2.position).sqrMagnitude;
									if (rightSqrDistance3 < largestRightSqrDistance)
									{
										largestRightSqrDistance = rightSqrDistance3;
										outputCreature = golem;
										targetPoint = weakpoint2;
									}
								}
							}
							return outputCreature;
						}
					}
					if (toGolem.sqrMagnitude <= sqrMaxDistance && toGolem.sqrMagnitude >= sqrMinDistance && Vector3.Angle(toGolem, ray.direction) <= maxAngle)
					{
						float distanceGolem = toGolem.magnitude;
						if ((ray.GetPoint(distanceGolem) - golem.ClosestPoint(ray.GetPoint(distanceGolem))).sqrMagnitude < largestRightSqrDistance)
						{
							outputCreature = golem;
						}
					}
					return outputCreature;
				}
			}
			return outputCreature;
		}

		// Token: 0x06001986 RID: 6534 RVA: 0x000A93EF File Offset: 0x000A75EF
		public override void RefreshWeakPoints()
		{
			if (this.weakpoints.Count == 0)
			{
				this.weakpoints.Add(this.ragdoll.headPart.transform);
			}
		}

		// Token: 0x06001987 RID: 6535 RVA: 0x000A941C File Offset: 0x000A761C
		public void Teleport(Vector3 position, Quaternion rotation)
		{
			if (this.isPlayer)
			{
				Debug.Log("Teleporting the player via Creature.Teleport will not work as intended! You want Player.local.Teleport instead!");
			}
			Vector3 leftItemLocalPosition = Vector3.zero;
			Quaternion leftItemLocalRotation = Quaternion.identity;
			if (this.handLeft.grabbedHandle && this.handLeft.grabbedHandle.item)
			{
				leftItemLocalPosition = base.transform.InverseTransformPoint(this.handLeft.grabbedHandle.item.transform.position);
				leftItemLocalRotation = base.transform.InverseTransformRotation(this.handLeft.grabbedHandle.item.transform.rotation);
			}
			Vector3 rightItemLocalPosition = Vector3.zero;
			Quaternion rightItemLocalRotation = Quaternion.identity;
			if (this.handRight.grabbedHandle && this.handRight.grabbedHandle.item)
			{
				rightItemLocalPosition = base.transform.InverseTransformPoint(this.handRight.grabbedHandle.item.transform.position);
				rightItemLocalRotation = base.transform.InverseTransformRotation(this.handRight.grabbedHandle.item.transform.rotation);
			}
			base.transform.position = position;
			base.transform.rotation = rotation;
			this.locomotion.prevPosition = position;
			this.locomotion.prevRotation = rotation;
			this.locomotion.velocity = Vector3.zero;
			if (this.handLeft.grabbedHandle && this.handLeft.grabbedHandle.item)
			{
				this.handLeft.grabbedHandle.item.transform.position = base.transform.TransformPoint(leftItemLocalPosition);
				this.handLeft.grabbedHandle.item.transform.rotation = base.transform.TransformRotation(leftItemLocalRotation);
			}
			if (this.handRight.grabbedHandle && this.handRight.grabbedHandle.item)
			{
				this.handRight.grabbedHandle.item.transform.position = base.transform.TransformPoint(rightItemLocalPosition);
				this.handRight.grabbedHandle.item.transform.rotation = base.transform.TransformRotation(rightItemLocalRotation);
			}
		}

		// Token: 0x06001988 RID: 6536 RVA: 0x000A9668 File Offset: 0x000A7868
		public void SetHeight(CreatureData creatureData)
		{
			if (creatureData.adjustHeightToPlayer && Player.characterData != null)
			{
				float randomHeight = UnityEngine.Random.Range(Player.characterData.calibration.height - creatureData.adjustHeightToPlayerDelta, Player.characterData.calibration.height + creatureData.adjustHeightToPlayerDelta);
				this.SetHeight(Mathf.Clamp(randomHeight, creatureData.randomMinHeight, creatureData.randomMaxHeight));
				return;
			}
			if (creatureData.randomMinHeight > 0f && creatureData.randomMaxHeight > 0f)
			{
				this.SetHeight(UnityEngine.Random.Range(creatureData.randomMinHeight, creatureData.randomMaxHeight));
			}
		}

		// Token: 0x06001989 RID: 6537 RVA: 0x000A9700 File Offset: 0x000A7900
		public void SetHeight(float height)
		{
			if (height == 0f)
			{
				return;
			}
			if (base.isActiveAndEnabled && this.initialized)
			{
				Ragdoll.State orgState = this.ragdoll.state;
				this.ragdoll.SetState(Ragdoll.State.Disabled);
				this.ragdoll.gameObject.SetActive(false);
				base.transform.localScale = Vector3.one * (height / this.morphology.height);
				this.ragdoll.gameObject.SetActive(true);
				this.ragdoll.SetState(orgState);
				if (this.OnHeightChanged != null)
				{
					this.OnHeightChanged();
					return;
				}
			}
			else
			{
				if (this.morphology.height == 0f)
				{
					this.InitCenterEyes();
					this.morphology.eyesHeight = base.transform.InverseTransformPoint(this.centerEyes.position).y;
					this.morphology.height = Morphology.GetHeight(this.morphology.eyesHeight);
				}
				base.transform.localScale = Vector3.one * (height / this.morphology.height);
			}
		}

		// Token: 0x0600198A RID: 6538 RVA: 0x000A9823 File Offset: 0x000A7A23
		public float GetHeight()
		{
			return this.morphology.height * base.transform.lossyScale.y;
		}

		// Token: 0x0600198B RID: 6539 RVA: 0x000A9841 File Offset: 0x000A7A41
		public float GetEyesHeight()
		{
			return this.morphology.eyesHeight * base.transform.lossyScale.y;
		}

		// Token: 0x0600198C RID: 6540 RVA: 0x000A985F File Offset: 0x000A7A5F
		public bool IsAnimatorBusy()
		{
			return !this.animator || this.animator.GetBool(Creature.hashIsBusy);
		}

		// Token: 0x0600198D RID: 6541 RVA: 0x000A9880 File Offset: 0x000A7A80
		public void SetAnimatorBusy(bool active)
		{
			this.animator.SetBool(Creature.hashIsBusy, active);
		}

		// Token: 0x0600198E RID: 6542 RVA: 0x000A9894 File Offset: 0x000A7A94
		public void TryElectrocute(float power, float duration, bool forced, bool imbueHit, EffectData effectData = null)
		{
			if (this.brain && this.brain.instance != null)
			{
				BrainModuleElectrocute moduleElectrocute = this.brain.instance.GetModule<BrainModuleElectrocute>(true);
				if (moduleElectrocute != null)
				{
					moduleElectrocute.TryElectrocute(power, duration, forced, imbueHit, effectData);
				}
			}
		}

		// Token: 0x0600198F RID: 6543 RVA: 0x000A98E0 File Offset: 0x000A7AE0
		public void StopShock()
		{
			if (this.brain && this.brain.instance != null)
			{
				BrainModuleElectrocute moduleElectrocute = this.brain.instance.GetModule<BrainModuleElectrocute>(true);
				if (moduleElectrocute != null)
				{
					moduleElectrocute.StopElectrocute(true);
				}
			}
		}

		// Token: 0x06001990 RID: 6544 RVA: 0x000A9923 File Offset: 0x000A7B23
		public void MaxPush(Creature.PushType type, Vector3 direction, RagdollPart.Type bodyPart = (RagdollPart.Type)0)
		{
			if (this.TryBrainModulePush(type, direction, 0, true, bodyPart))
			{
				return;
			}
			this.PushFallback(type);
		}

		// Token: 0x06001991 RID: 6545 RVA: 0x000A993A File Offset: 0x000A7B3A
		public void TryPush(Creature.PushType type, Vector3 direction, int pushLevel, RagdollPart.Type bodyPart = (RagdollPart.Type)0)
		{
			if (this.TryBrainModulePush(type, direction, pushLevel, false, bodyPart))
			{
				return;
			}
			this.PushFallback(type);
		}

		// Token: 0x06001992 RID: 6546 RVA: 0x000A9954 File Offset: 0x000A7B54
		private bool TryBrainModulePush(Creature.PushType type, Vector3 direction, int pushLevel, bool max = false, RagdollPart.Type bodyPart = (RagdollPart.Type)0)
		{
			if (this.brain && this.brain.instance != null)
			{
				BrainModuleHitReaction moduleHitReaction = this.brain.instance.GetModule<BrainModuleHitReaction>(true);
				if (moduleHitReaction != null)
				{
					moduleHitReaction.TryPush(type, direction, max ? moduleHitReaction.GetMaxPushLevel(type) : pushLevel, bodyPart);
					return true;
				}
			}
			return false;
		}

		// Token: 0x06001993 RID: 6547 RVA: 0x000A99AB File Offset: 0x000A7BAB
		public void ForceStagger(Vector3 direction, BrainModuleHitReaction.PushBehaviour.Effect pushType, RagdollPart.Type bodyPart = RagdollPart.Type.Torso)
		{
			Brain brain = this.brain;
			if (brain == null)
			{
				return;
			}
			BrainData instance = brain.instance;
			if (instance == null)
			{
				return;
			}
			BrainModuleHitReaction module = instance.GetModule<BrainModuleHitReaction>(true);
			if (module == null)
			{
				return;
			}
			module.ForceStaggerBehaviour(direction, pushType, bodyPart);
		}

		// Token: 0x06001994 RID: 6548 RVA: 0x000A99D5 File Offset: 0x000A7BD5
		private void PushFallback(Creature.PushType type)
		{
			if (type != Creature.PushType.Parry)
			{
				if (this.ragdoll.standingUp)
				{
					this.ragdoll.CancelGetUp(true);
				}
				this.ragdoll.SetState(Ragdoll.State.Destabilized);
			}
		}

		// Token: 0x06001995 RID: 6549 RVA: 0x000A9A00 File Offset: 0x000A7C00
		public void PlayAnimation(string id, Action endCallback = null)
		{
			AnimationData animationData;
			if (Catalog.TryGetData<AnimationData>(id, out animationData, true))
			{
				AnimationData.Clip[] array;
				this.PlayAnimation(animationData, out array, endCallback);
			}
		}

		// Token: 0x06001996 RID: 6550 RVA: 0x000A9A24 File Offset: 0x000A7C24
		public void PlayAnimation(AnimationData animationData, Action endCallback = null)
		{
			AnimationData.Clip[] array;
			this.PlayAnimation(animationData, out array, endCallback);
		}

		// Token: 0x06001997 RID: 6551 RVA: 0x000A9A3C File Offset: 0x000A7C3C
		public void PlayAnimation(AnimationData animationData, out AnimationData.Clip[] clips, Action endCallback = null)
		{
			AnimationData.Clip startAnimClip = animationData.Pick(AnimationData.Clip.Step.Start, false);
			AnimationData.Clip loopAnimClip = animationData.Pick(AnimationData.Clip.Step.Loop, false);
			AnimationData.Clip endAnimClip = animationData.Pick(AnimationData.Clip.Step.End, false);
			clips = new AnimationData.Clip[]
			{
				startAnimClip,
				loopAnimClip,
				endAnimClip
			};
			if (startAnimClip != null && loopAnimClip != null && endAnimClip != null)
			{
				this.PlayAnimation(startAnimClip.animationClip, loopAnimClip.animationClip, endAnimClip.animationClip, endAnimClip.animationSpeed, endCallback, true);
				return;
			}
			if (startAnimClip != null)
			{
				this.PlayAnimation(startAnimClip.animationClip, false, startAnimClip.animationSpeed, endCallback, false, true, false);
				return;
			}
			if (loopAnimClip != null)
			{
				this.PlayAnimation(loopAnimClip.animationClip, true, loopAnimClip.animationSpeed, endCallback, false, true, false);
			}
		}

		// Token: 0x06001998 RID: 6552 RVA: 0x000A9AD8 File Offset: 0x000A7CD8
		public void PlayAnimation(AnimationClip animationClip, bool loop, float speedMultiplier = 1f, Action endCallback = null, bool mirror = false, bool exitAutomatically = true, bool overrideAllOthers = false)
		{
			if (this.isPlayingDynamicAnimation)
			{
				this.StopAnimation(true);
			}
			this.animator.ResetTrigger(Creature.hashDynamicInterrupt);
			this.animator.SetBool(Creature.hashDynamicLoop, false);
			this.animator.SetBool(Creature.hashDynamicLoop3, false);
			this.animator.SetFloat(Creature.hashDynamicSpeedMultiplier, speedMultiplier);
			this.animator.SetBool(Creature.hashDynamicMirror, mirror);
			this.animator.SetBool(Creature.hashExitDynamic, exitAutomatically);
			if (this.animatorOverrideController)
			{
				if (loop)
				{
					this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>[]
					{
						new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicLoopClip, animationClip)
					});
					this.animator.SetBool(Creature.hashDynamicLoop, true);
				}
				else
				{
					int currentState = this.animator.GetInteger(Creature.hashDynamicOneShot);
					this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>[]
					{
						new KeyValuePair<int, AnimationClip>((currentState == 1) ? Creature.clipIndex.dynamicStartClipB : Creature.clipIndex.dynamicStartClipA, animationClip)
					});
					this.animator.SetInteger(Creature.hashDynamicOneShot, (currentState == 1) ? 2 : 1);
				}
				this.dynamicAnimationendEndCallback = endCallback;
				this.SetAnimatorBusy(true);
				this.isPlayingDynamicAnimation = true;
				return;
			}
			Debug.LogError("No AnimatorOverrideController found on animator!");
		}

		// Token: 0x06001999 RID: 6553 RVA: 0x000A9C24 File Offset: 0x000A7E24
		public void PlayUpperAnimation(AnimationClip animationClip, bool loop, float speedMultiplier = 1f, Action endCallback = null, bool mirror = false, bool exitAutomatically = true)
		{
			this.animator.SetBool(Creature.hashDynamicUpperLoop, false);
			this.animator.SetFloat(Creature.hashDynamicUpperMultiplier, speedMultiplier);
			this.animator.SetBool(Creature.hashDynamicUpperMirror, mirror);
			this.animator.SetBool(Creature.hashExitDynamic, exitAutomatically);
			if (this.animatorOverrideController)
			{
				if (loop)
				{
					this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>[]
					{
						new KeyValuePair<int, AnimationClip>(Creature.clipIndex.upperBodyDynamicLoopClip, animationClip)
					});
					this.animator.SetBool(Creature.hashDynamicUpperLoop, true);
				}
				else
				{
					int currentState = this.animator.GetInteger(Creature.hashDynamicUpperOneShot);
					this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>[]
					{
						new KeyValuePair<int, AnimationClip>((currentState == 1) ? Creature.clipIndex.upperBodyDynamicClipB : Creature.clipIndex.upperBodyDynamicClipA, animationClip)
					});
					this.animator.SetInteger(Creature.hashDynamicUpperOneShot, (currentState == 1) ? 2 : 1);
				}
				this.dynamicAnimationendEndCallback = endCallback;
				return;
			}
			Debug.LogError("No AnimatorOverrideController found on animator!");
		}

		// Token: 0x0600199A RID: 6554 RVA: 0x000A9D30 File Offset: 0x000A7F30
		public void PlayAnimation(AnimationClip startAnimationClip, AnimationClip loopAnimationClip, AnimationClip endAnimationClip, float speedMultiplier = 1f, Action endCallback = null, bool exitAutomatically = true)
		{
			if (this.isPlayingDynamicAnimation)
			{
				this.StopAnimation(true);
			}
			this.animator.ResetTrigger(Creature.hashDynamicInterrupt);
			this.animator.SetBool(Creature.hashDynamicLoop, false);
			this.animator.SetBool(Creature.hashDynamicLoop3, false);
			this.animator.SetFloat(Creature.hashDynamicSpeedMultiplier, speedMultiplier);
			this.animator.SetBool(Creature.hashExitDynamic, exitAutomatically);
			if (this.animatorOverrideController)
			{
				this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>[]
				{
					new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicStartClipA, startAnimationClip),
					new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicLoopClip, loopAnimationClip),
					new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicEndClip, endAnimationClip)
				});
				this.animator.SetBool(Creature.hashDynamicLoop3, true);
				this.dynamicAnimationendEndCallback = endCallback;
				this.SetAnimatorBusy(true);
				this.isPlayingDynamicAnimation = true;
				return;
			}
			Debug.LogError("No AnimatorOverrideController found on animator!");
		}

		// Token: 0x0600199B RID: 6555 RVA: 0x000A9E34 File Offset: 0x000A8034
		public void PlayAnimationLoopAdd(AnimationClip animationLoopAddClip)
		{
			if (this.isPlayingDynamicAnimation && this.animatorOverrideController)
			{
				this.UpdateOverrideClip(new KeyValuePair<int, AnimationClip>[]
				{
					new KeyValuePair<int, AnimationClip>(Creature.clipIndex.dynamicLoopAddClip, animationLoopAddClip)
				});
				this.animator.SetTrigger(Creature.hashDynamicLoopAdd);
			}
		}

		// Token: 0x0600199C RID: 6556 RVA: 0x000A9E8C File Offset: 0x000A808C
		public void PlayAnimationLoopAdd(AnimationData animationData)
		{
			AnimationData.Clip animClip = animationData.Pick(AnimationData.Clip.Step.Start, false);
			if (animClip != null)
			{
				this.PlayAnimationLoopAdd(animClip.animationClip);
			}
		}

		// Token: 0x0600199D RID: 6557 RVA: 0x000A9EB1 File Offset: 0x000A80B1
		public AnimationClip GetOverrideClip(int key)
		{
			return this.animationClipOverrides[key].Value;
		}

		// Token: 0x0600199E RID: 6558 RVA: 0x000A9EC4 File Offset: 0x000A80C4
		public void UpdateOverrideClip(params KeyValuePair<int, AnimationClip>[] overrides)
		{
			foreach (KeyValuePair<int, AnimationClip> over in overrides)
			{
				this.animationClipOverrides[over.Key] = new KeyValuePair<AnimationClip, AnimationClip>(this.animationClipOverrides[over.Key].Key, over.Value);
			}
			this.animatorOverrideController.ApplyOverrides(this.animationClipOverrides);
		}

		// Token: 0x0600199F RID: 6559 RVA: 0x000A9F2F File Offset: 0x000A812F
		protected void UpdateDynamicAnimation()
		{
			if (this.isPlayingDynamicAnimation && !this.IsAnimatorBusy())
			{
				this.isPlayingDynamicAnimation = false;
				if (this.dynamicAnimationendEndCallback != null)
				{
					this.dynamicAnimationendEndCallback();
					this.dynamicAnimationendEndCallback = null;
				}
			}
		}

		// Token: 0x060019A0 RID: 6560 RVA: 0x000A9F64 File Offset: 0x000A8164
		public void StopAnimation(bool interrupt = false)
		{
			if (this.isPlayingDynamicAnimation)
			{
				this.isPlayingDynamicAnimation = false;
				if (interrupt)
				{
					this.animator.SetTrigger(Creature.hashDynamicInterrupt);
				}
				this.animator.SetBool(Creature.hashDynamicLoop, false);
				this.animator.SetBool(Creature.hashDynamicLoop3, false);
				this.animator.SetFloat(Creature.hashDynamicSpeedMultiplier, 1f);
				if (interrupt && this.dynamicAnimationendEndCallback != null)
				{
					this.dynamicAnimationendEndCallback();
					this.dynamicAnimationendEndCallback = null;
				}
			}
			if (this.animator.GetBool(Creature.hashDynamicUpperLoop) || this.animator.GetInteger(Creature.hashDynamicUpperOneShot) != 0)
			{
				if (interrupt)
				{
					this.animator.SetTrigger(Creature.hashDynamicInterrupt);
				}
				this.animator.SetBool(Creature.hashDynamicUpperLoop, false);
				this.animator.SetBool(Creature.hashDynamicUpperMirror, false);
				this.animator.SetFloat(Creature.hashDynamicUpperMultiplier, 1f);
			}
		}

		// Token: 0x060019A1 RID: 6561 RVA: 0x000AA058 File Offset: 0x000A8258
		public CreatureData.EyeClip GetEyeClip(string clipName)
		{
			int count = this.eyeClips.Count;
			for (int i = 0; i < count; i++)
			{
				CreatureData.EyeClip eyeClip = this.eyeClips[i];
				if (!(eyeClip.clipName != clipName))
				{
					return eyeClip;
				}
			}
			return null;
		}

		// Token: 0x060019A2 RID: 6562 RVA: 0x000AA09B File Offset: 0x000A829B
		public void PlayEyeClip(string clipName)
		{
			this.PlayEyeClip(this.GetEyeClip(clipName));
		}

		// Token: 0x060019A3 RID: 6563 RVA: 0x000AA0AC File Offset: 0x000A82AC
		public void PlayEyeClip(CreatureData.EyeClip eyeClip)
		{
			if (eyeClip == null)
			{
				return;
			}
			if (eyeClip.active)
			{
				return;
			}
			if (eyeClip.playAutomaticallyWhileAlive && eyeClip.nextPlayDelay < 0f)
			{
				eyeClip.lastEndTime = Time.time;
				eyeClip.nextPlayDelay = UnityEngine.Random.Range(eyeClip.minMaxBetweenPlays.x, eyeClip.minMaxBetweenPlays.y);
				return;
			}
			if (Time.time < eyeClip.lastEndTime + eyeClip.nextPlayDelay + eyeClip.maxIndividualDelay)
			{
				return;
			}
			eyeClip.active = true;
			eyeClip.lastStartTime = Time.time;
			eyeClip.lastEndTime = Time.time + eyeClip.duration;
			eyeClip.nextPlayDelay = UnityEngine.Random.Range(eyeClip.minMaxBetweenPlays.x, eyeClip.minMaxBetweenPlays.y);
			eyeClip.maxIndividualDelay = 0f;
			if (eyeClip.affectedEyes.Count == 0)
			{
				foreach (CreatureEye eye in this.allEyes)
				{
					if (string.IsNullOrEmpty(eyeClip.eyeTagFilter) || eye.eyeTag.Contains(eyeClip.eyeTagFilter))
					{
						eyeClip.affectedEyes.Add(eye, 0f);
					}
				}
			}
			if (eyeClip.minMaxDelayPerEye.y > 0f)
			{
				foreach (CreatureEye eye2 in eyeClip.affectedEyes.Keys)
				{
					float individualDelay = UnityEngine.Random.Range(eyeClip.minMaxDelayPerEye.x, eyeClip.minMaxDelayPerEye.y);
					eyeClip.affectedEyes[eye2] = individualDelay;
					eyeClip.maxIndividualDelay = Mathf.Max(eyeClip.maxIndividualDelay, individualDelay);
				}
			}
		}

		// Token: 0x060019A4 RID: 6564 RVA: 0x000AA284 File Offset: 0x000A8484
		protected void UpdateFacialAnimation()
		{
			this.UpdateEyesAnimation();
		}

		// Token: 0x060019A5 RID: 6565 RVA: 0x000AA28C File Offset: 0x000A848C
		protected void UpdateEyesAnimation()
		{
			if (this.isPlayer && ((Mirror.local != null && Mirror.local.isRendering && Mirror.local.playerHeadVisible) || Spectator.local == null || Spectator.local.state == Spectator.State.Auto || Spectator.local.state == Spectator.State.FPV))
			{
				foreach (CreatureEye creatureEye in this.allEyes)
				{
					creatureEye.closeAmount = 0f;
					creatureEye.SetClose();
				}
				return;
			}
			if (this.eyeClips.Count == 0)
			{
				foreach (CreatureData.EyeClip eyeClip in this.data.eyeClips)
				{
					this.eyeClips.Add(eyeClip.Clone());
				}
			}
			int i = 0;
			while (i < this.eyeClips.Count)
			{
				CreatureData.EyeClip eyeClip2 = this.eyeClips[i];
				if (eyeClip2.active)
				{
					goto IL_149;
				}
				if (Time.time > eyeClip2.lastEndTime + eyeClip2.nextPlayDelay + eyeClip2.maxIndividualDelay && eyeClip2.playAutomaticallyWhileAlive && this.autoEyeClipsActive)
				{
					this.PlayEyeClip(eyeClip2);
					goto IL_149;
				}
				IL_273:
				i++;
				continue;
				IL_149:
				bool noneActive = true;
				foreach (CreatureEye eye in eyeClip2.affectedEyes.Keys)
				{
					if (eye.lastUpdateTime != Time.time)
					{
						eye.closeAmount = 0f;
						eye.lastUpdateTime = Time.time;
					}
					float offset = eyeClip2.affectedEyes[eye];
					if (Time.time <= eyeClip2.lastEndTime + offset)
					{
						noneActive = false;
						float clipTime = Mathf.InverseLerp(eyeClip2.lastStartTime + offset, eyeClip2.lastEndTime + offset, Time.time);
						float closedness = 1f - eyeClip2.openCurve.Evaluate(clipTime * (float)eyeClip2.openCurve.length);
						eye.closeAmount = Mathf.Max(eye.closeAmount, closedness);
					}
					else
					{
						eye.closeAmount = Mathf.Max(eye.closeAmount, 1f - eyeClip2.openCurve.Evaluate((float)eyeClip2.openCurve.length));
					}
					eye.SetClose();
				}
				if (noneActive)
				{
					eyeClip2.active = false;
					goto IL_273;
				}
				goto IL_273;
			}
		}

		// Token: 0x060019A6 RID: 6566 RVA: 0x000AA54C File Offset: 0x000A874C
		public virtual void InvokeZoneEvent(Zone zone, bool enter)
		{
			this.OnZoneEvent(zone, enter);
		}

		// Token: 0x1700018A RID: 394
		// (get) Token: 0x060019A7 RID: 6567 RVA: 0x000AA55B File Offset: 0x000A875B
		// (set) Token: 0x060019A8 RID: 6568 RVA: 0x000AA563 File Offset: 0x000A8763
		public float currentHealth
		{
			get
			{
				return this._currentHealth;
			}
			set
			{
				this._currentHealth = value;
				this.PreventHealthNaN();
				this.InvokeHealthChangeEvent(this._currentHealth, this.maxHealth);
			}
		}

		// Token: 0x1700018B RID: 395
		// (get) Token: 0x060019A9 RID: 6569 RVA: 0x000AA584 File Offset: 0x000A8784
		// (set) Token: 0x060019AA RID: 6570 RVA: 0x000AA598 File Offset: 0x000A8798
		public float maxHealth
		{
			get
			{
				return this._currentMaxHealth * this.healthModifier;
			}
			set
			{
				float ratio = this.currentHealth / this.maxHealth;
				this._currentMaxHealth = value;
				this.currentHealth = this.maxHealth * ratio;
				this.PreventHealthNaN();
				this.InvokeHealthChangeEvent(this.currentHealth, this.maxHealth);
			}
		}

		// Token: 0x140000A4 RID: 164
		// (add) Token: 0x060019AB RID: 6571 RVA: 0x000AA5E0 File Offset: 0x000A87E0
		// (remove) Token: 0x060019AC RID: 6572 RVA: 0x000AA618 File Offset: 0x000A8818
		public event Creature.SetupDataEvent OnSetupDataEvent;

		// Token: 0x140000A5 RID: 165
		// (add) Token: 0x060019AD RID: 6573 RVA: 0x000AA650 File Offset: 0x000A8850
		// (remove) Token: 0x060019AE RID: 6574 RVA: 0x000AA688 File Offset: 0x000A8888
		public event Creature.HealthChangeEvent OnHealthChange;

		// Token: 0x140000A6 RID: 166
		// (add) Token: 0x060019AF RID: 6575 RVA: 0x000AA6C0 File Offset: 0x000A88C0
		// (remove) Token: 0x060019B0 RID: 6576 RVA: 0x000AA6F8 File Offset: 0x000A88F8
		public event Creature.HealEvent OnHealEvent;

		// Token: 0x140000A7 RID: 167
		// (add) Token: 0x060019B1 RID: 6577 RVA: 0x000AA730 File Offset: 0x000A8930
		// (remove) Token: 0x060019B2 RID: 6578 RVA: 0x000AA768 File Offset: 0x000A8968
		public event Creature.ResurrectEvent OnResurrectEvent;

		// Token: 0x140000A8 RID: 168
		// (add) Token: 0x060019B3 RID: 6579 RVA: 0x000AA7A0 File Offset: 0x000A89A0
		// (remove) Token: 0x060019B4 RID: 6580 RVA: 0x000AA7D8 File Offset: 0x000A89D8
		public event Creature.DamageEvent OnDamageEvent;

		// Token: 0x140000A9 RID: 169
		// (add) Token: 0x060019B5 RID: 6581 RVA: 0x000AA810 File Offset: 0x000A8A10
		// (remove) Token: 0x060019B6 RID: 6582 RVA: 0x000AA848 File Offset: 0x000A8A48
		public event Creature.KillEvent OnKillEvent;

		// Token: 0x060019B7 RID: 6583 RVA: 0x000AA87D File Offset: 0x000A8A7D
		public void TestKill()
		{
			this.Kill();
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x000AA885 File Offset: 0x000A8A85
		public void TestDamage()
		{
			this.Damage(5f);
		}

		// Token: 0x060019B9 RID: 6585 RVA: 0x000AA892 File Offset: 0x000A8A92
		private void PreventHealthNaN()
		{
			if (float.IsNaN(this.currentHealth))
			{
				this.currentHealth = 0f;
			}
			if (float.IsNaN(this.maxHealth))
			{
				this.maxHealth = (float)this.data.health;
			}
		}

		// Token: 0x060019BA RID: 6586 RVA: 0x000AA8CC File Offset: 0x000A8ACC
		public float GetDamageMultiplier()
		{
			float output = 1f;
			foreach (float value in this.damageMultipliers.Values)
			{
				output *= value;
			}
			return output;
		}

		// Token: 0x060019BB RID: 6587 RVA: 0x000AA928 File Offset: 0x000A8B28
		public void SetDamageMultiplier(object handler, float value)
		{
			this.damageMultipliers[handler] = value;
		}

		// Token: 0x060019BC RID: 6588 RVA: 0x000AA937 File Offset: 0x000A8B37
		public bool RemoveDamageMultiplier(object handler)
		{
			return this.damageMultipliers.Remove(handler);
		}

		// Token: 0x060019BD RID: 6589 RVA: 0x000AA945 File Offset: 0x000A8B45
		public void ClearMultipliers()
		{
			this.damageMultipliers.Clear();
			this.detectionFOVModifier.Clear();
			this.hitEnvironmentDamageModifier.Clear();
			this.healthModifier.Clear();
		}

		// Token: 0x060019BE RID: 6590 RVA: 0x000AA978 File Offset: 0x000A8B78
		public void Damage(float amount, DamageType type = DamageType.Energy)
		{
			CollisionInstance collisionInstance = new CollisionInstance(new DamageStruct(type, amount), null, null);
			this.Damage(collisionInstance);
		}

		// Token: 0x060019BF RID: 6591 RVA: 0x000AA99C File Offset: 0x000A8B9C
		public void Damage(CollisionInstance collisionInstance)
		{
			Creature.DamageEvent onDamageEvent = this.OnDamageEvent;
			if (onDamageEvent != null)
			{
				onDamageEvent(collisionInstance, EventTime.OnStart);
			}
			EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnStart);
			if (!collisionInstance.damageStruct.active)
			{
				return;
			}
			if (collisionInstance.damageStruct.damage == 0f)
			{
				return;
			}
			Brain brain = this.brain;
			if (brain != null && brain.isAttacking && !collisionInstance.damageStruct.sliceDone && collisionInstance.damageStruct.hitRagdollPart != null && (collisionInstance.damageStruct.hitRagdollPart.type == RagdollPart.Type.RightHand || collisionInstance.damageStruct.hitRagdollPart.type == RagdollPart.Type.LeftHand))
			{
				return;
			}
			if (this.isKilled)
			{
				Creature.DamageEvent onDamageEvent2 = this.OnDamageEvent;
				if (onDamageEvent2 != null)
				{
					onDamageEvent2(collisionInstance, EventTime.OnEnd);
				}
				EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnEnd);
				return;
			}
			this.lastInteractionTime = Time.time;
			ColliderGroup sourceColliderGroup = collisionInstance.sourceColliderGroup;
			Creature creature;
			if (sourceColliderGroup == null)
			{
				creature = null;
			}
			else
			{
				Item item = sourceColliderGroup.collisionHandler.item;
				if (item == null)
				{
					creature = null;
				}
				else
				{
					RagdollHand lastHandler = item.lastHandler;
					creature = ((lastHandler != null) ? lastHandler.creature : null);
				}
			}
			Creature attacker = creature;
			if (!attacker)
			{
				SpellCaster casterHand = collisionInstance.casterHand;
				attacker = ((casterHand != null) ? casterHand.mana.creature : null);
			}
			if (attacker)
			{
				this.lastInteractionCreature = attacker;
			}
			this.lastDamage = collisionInstance;
			this.lastDamageTime = Time.time;
			float damage = collisionInstance.damageStruct.damage;
			if (this.player)
			{
				if (Player.invincibility)
				{
					Creature.DamageEvent onDamageEvent3 = this.OnDamageEvent;
					if (onDamageEvent3 != null)
					{
						onDamageEvent3(collisionInstance, EventTime.OnEnd);
					}
					EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnEnd);
					return;
				}
				if (collisionInstance.damageStruct.damager)
				{
					damage *= collisionInstance.damageStruct.damager.data.GetTier(collisionInstance.damageStruct.damager.collisionHandler).playerDamageMultiplier;
					damage = Mathf.Clamp(damage, collisionInstance.damageStruct.damager.data.playerMinDamage, collisionInstance.damageStruct.damager.data.playerMaxDamage);
					if (UnityEngine.Random.value < collisionInstance.damageStruct.damager.data.GetTier(collisionInstance.damageStruct.damager.collisionHandler).gripDisableChance && this.data.gripRecoverTime > Time.fixedDeltaTime && Time.time - this.handRight.climb.lastGripDisableTime > Time.fixedDeltaTime * 2f && Time.time - this.handLeft.climb.lastGripDisableTime > Time.fixedDeltaTime * 2f)
					{
						if (UnityEngine.Random.value < 0.5f)
						{
							if (this.handLeft.climb.lastGripDisableTime < 0f)
							{
								this.handLeft.climb.DisableGripTemp(this.data.gripRecoverTime);
							}
							else if (UnityEngine.Random.value < 0.5f)
							{
								this.handRight.climb.DisableGripTemp(this.data.gripRecoverTime);
							}
						}
						else if (this.handRight.climb.lastGripDisableTime < 0f)
						{
							this.handRight.climb.DisableGripTemp(this.data.gripRecoverTime);
						}
						else if (UnityEngine.Random.value < 0.5f)
						{
							this.handLeft.climb.DisableGripTemp(this.data.gripRecoverTime);
						}
					}
				}
			}
			if (!this.isPlayer && !this.ragdoll.IsPhysicsEnabled(false))
			{
				damage *= this.data.physicsOffDamageMult;
			}
			damage *= this.GetDamageMultiplier();
			if (GameManager.local.collisionDebug != GameManager.CollisionDebug.None)
			{
				Debug.Log(string.Format("{0} took damage. Current health: {1}. Damage: {2}. Resulting health: {3}", new object[]
				{
					base.name,
					this.currentHealth,
					damage,
					this.currentHealth - damage
				}));
			}
			if (this.currentHealth - damage <= 0f)
			{
				this.Kill(collisionInstance);
				return;
			}
			this.currentHealth -= damage;
			if (this.currentHealth <= 0f)
			{
				this.currentHealth = 0f;
				this.Kill(collisionInstance);
				return;
			}
			Creature.DamageEvent onDamageEvent4 = this.OnDamageEvent;
			if (onDamageEvent4 != null)
			{
				onDamageEvent4(collisionInstance, EventTime.OnEnd);
			}
			EventManager.InvokeCreatureHit(this, collisionInstance, EventTime.OnEnd);
		}

		// Token: 0x060019C0 RID: 6592 RVA: 0x000AADD0 File Offset: 0x000A8FD0
		public void Kill(CollisionInstance collisionInstance)
		{
			if (this.isKilled)
			{
				return;
			}
			SkillSecondWind secondWind;
			if (this.TryGetSkill<SkillSecondWind>(Creature.secondWindId, out secondWind) && secondWind.ConsumeCharge(this, collisionInstance))
			{
				return;
			}
			EventManager.InvokeCreatureKill(this, this.player, collisionInstance, EventTime.OnStart);
			if (this.OnKillEvent != null)
			{
				this.OnKillEvent(collisionInstance, EventTime.OnStart);
			}
			Player orgPlayer = this.player;
			Creature.State state = this.state;
			if (this.player || this.brain.instance == null || !this.brain.instance.isActive)
			{
				if (this.handRight)
				{
					this.handRight.TryRelease();
				}
				if (this.handLeft)
				{
					this.handLeft.TryRelease();
				}
			}
			RagdollHand ragdollHand = this.handLeft;
			if (((ragdollHand != null) ? ragdollHand.caster : null) && this.handLeft.caster.telekinesis != null)
			{
				this.handLeft.caster.telekinesis.TryRelease(false, false);
			}
			RagdollHand ragdollHand2 = this.handRight;
			if (((ragdollHand2 != null) ? ragdollHand2.caster : null) && this.handRight.caster.telekinesis != null)
			{
				this.handRight.caster.telekinesis.TryRelease(false, false);
			}
			if (this.player)
			{
				this.player.ReleaseCreature();
			}
			this.ragdoll.SetState(Ragdoll.State.Inert);
			this.RefreshFallState(Creature.FallState.None, true);
			this.lastInteractionTime = Time.time;
			ColliderGroup sourceColliderGroup = collisionInstance.sourceColliderGroup;
			UnityEngine.Object exists;
			if (sourceColliderGroup == null)
			{
				exists = null;
			}
			else
			{
				Item item = sourceColliderGroup.collisionHandler.item;
				exists = ((item != null) ? item.lastHandler : null);
			}
			if (exists)
			{
				this.lastInteractionCreature = collisionInstance.sourceColliderGroup.collisionHandler.item.lastHandler.creature;
			}
			this.isKilled = true;
			this.currentHealth = 0f;
			this.brain.Stop();
			this.autoEyeClipsActive = false;
			this.locomotion.ClearPhysicModifiers();
			this.locomotion.ClearSpeedModifiers();
			this.spawnGroup = null;
			if (orgPlayer)
			{
				Player.characterData.inventory.LoadCurrencies();
			}
			if (this.OnKillEvent != null)
			{
				this.OnKillEvent(collisionInstance, EventTime.OnEnd);
			}
			EventManager.InvokeCreatureKill(this, orgPlayer, collisionInstance, EventTime.OnEnd);
			if (this.initialArea != null)
			{
				this.initialArea.OnCreatureKill(this.areaSpawnerIndex);
			}
		}

		// Token: 0x060019C1 RID: 6593 RVA: 0x000AB028 File Offset: 0x000A9228
		public void Heal(float heal, Creature healer)
		{
			if (this.currentHealth != this.maxHealth)
			{
				if (this.OnHealEvent != null)
				{
					this.OnHealEvent(heal, healer, EventTime.OnStart);
				}
				EventManager.InvokeCreatureHeal(this, heal, healer, EventTime.OnStart);
				this.currentHealth += heal;
				if (this.currentHealth >= this.maxHealth)
				{
					this.currentHealth = this.maxHealth;
				}
				this.lastInteractionTime = Time.time;
				this.lastInteractionCreature = healer;
				if (this.OnHealEvent != null)
				{
					this.OnHealEvent(heal, healer, EventTime.OnEnd);
				}
				EventManager.InvokeCreatureHeal(this, heal, healer, EventTime.OnEnd);
			}
		}

		// Token: 0x060019C2 RID: 6594 RVA: 0x000AB0BC File Offset: 0x000A92BC
		public void Resurrect(float newHealth, Creature resurrector)
		{
			if (this.isKilled)
			{
				if (this.OnResurrectEvent != null)
				{
					this.OnResurrectEvent(newHealth, resurrector, EventTime.OnStart);
				}
				this.currentHealth = newHealth;
				this.isKilled = false;
				this.lastInteractionTime = Time.time;
				this.lastInteractionCreature = resurrector;
				this.ragdoll.SetState(Ragdoll.State.Destabilized, true);
				if (this.brain.instance != null)
				{
					this.brain.instance.Start();
				}
				this.autoEyeClipsActive = true;
				if (this.OnResurrectEvent != null)
				{
					this.OnResurrectEvent(newHealth, resurrector, EventTime.OnEnd);
				}
			}
		}

		// Token: 0x060019C3 RID: 6595 RVA: 0x000AB14E File Offset: 0x000A934E
		public void ToogleTPose()
		{
			this.toogleTPose = !this.toogleTPose;
			this.animator.SetBool(Creature.hashTstance, this.toogleTPose);
		}

		// Token: 0x060019C4 RID: 6596 RVA: 0x000AB178 File Offset: 0x000A9378
		public void RefreshMorphology()
		{
			this.animator.SetBool(Creature.hashTstance, true);
			this.animator.Update(0f);
			Vector3 upperArmCenterLocalPos = Vector3.zero;
			if (this.handLeft && this.handRight)
			{
				upperArmCenterLocalPos = base.transform.InverseTransformPoint((this.handLeft.upperArmPart.bone.animation.position + this.handRight.upperArmPart.bone.animation.position) / 2f);
				float leftArmLenght = Vector3.Distance(base.transform.InverseTransformPoint(this.handLeft.upperArmPart.bone.animation.position), base.transform.InverseTransformPoint(this.handLeft.grip.position));
				float rightArmLenght = Vector3.Distance(base.transform.InverseTransformPoint(this.handRight.upperArmPart.bone.animation.position), base.transform.InverseTransformPoint(this.handRight.grip.position));
				this.morphology.armsLength = (leftArmLenght + rightArmLenght) / 2f;
				this.morphology.armsHeight = upperArmCenterLocalPos.y;
				this.morphology.armsSpacing = Vector3.Distance(base.transform.InverseTransformPoint(this.handLeft.upperArmPart.bone.animation.position), base.transform.InverseTransformPoint(this.handRight.upperArmPart.bone.animation.position));
			}
			if (this.footLeft && this.footRight)
			{
				float leftlegLenght = this.footLeft.GetCurrentLegDistance(Space.Self);
				float rightlegLenght = this.footRight.GetCurrentLegDistance(Space.Self);
				this.morphology.legsLength = (leftlegLenght + rightlegLenght) / 2f;
				this.morphology.legsSpacing = Vector3.Distance(base.transform.InverseTransformPoint(this.footLeft.upperLegBone ? this.footLeft.upperLegBone.position : this.footLeft.lowerLegBone.position), base.transform.InverseTransformPoint(this.footRight.upperLegBone ? this.footRight.upperLegBone.position : this.footRight.lowerLegBone.position));
			}
			this.morphology.eyesHeight = base.transform.InverseTransformPoint(this.centerEyes.position).y;
			this.morphology.headHeight = base.transform.InverseTransformPoint(this.ragdoll.headPart.bone.animation.position).y;
			this.morphology.headForward = Mathf.Abs(base.transform.InverseTransformPoint(this.ragdoll.headPart.bone.animation.position).z - upperArmCenterLocalPos.z);
			this.morphology.eyesForward = Mathf.Abs(base.transform.InverseTransformPoint(this.centerEyes.position).z - upperArmCenterLocalPos.z);
			this.morphology.hipsHeight = base.transform.InverseTransformPoint(this.ragdoll.rootPart.bone.animation.position).y;
			if (this.animator.isHuman)
			{
				this.morphology.chestHeight = base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.Chest).position).y;
				this.morphology.spineHeight = base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.Spine).position).y;
				this.morphology.upperLegsHeight = (base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg).position).y + base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.RightUpperLeg).position).y) / 2f;
				this.morphology.lowerLegsHeight = (base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position).y + base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).position).y) / 2f;
				this.morphology.footHeight = (base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.LeftFoot).position).y + base.transform.InverseTransformPoint(this.animator.GetBoneTransform(HumanBodyBones.RightFoot).position).y) / 2f;
			}
			this.morphology.height = Morphology.GetHeight(this.morphology.eyesHeight);
			this.animator.SetBool(Creature.hashTstance, false);
		}

		// Token: 0x060019C5 RID: 6597 RVA: 0x000AB698 File Offset: 0x000A9898
		public virtual void UpdateStep(Vector3 position, float stepSpeedMultiplier = 1f, float stepThresholdMultiplier = 1f)
		{
			Vector3 headPositionXZ = new Vector3(position.x, base.transform.position.y, position.z);
			if (Vector3.Distance(headPositionXZ, base.transform.position) > this.stepThreshold * stepThresholdMultiplier)
			{
				this.stepTargetPos = headPositionXZ;
			}
			Vector3 stepHorizontalVector = (this.stepTargetPos - base.transform.position) * (this.locomotion.forwardSpeed * stepSpeedMultiplier);
			this.locomotion.moveDirection = new Vector3(stepHorizontalVector.x, 0f, stepHorizontalVector.z);
		}

		// Token: 0x060019C6 RID: 6598 RVA: 0x000AB734 File Offset: 0x000A9934
		public virtual void UpdateFall()
		{
			if (!this.loaded || this.state == Creature.State.Dead)
			{
				return;
			}
			if (AreaManager.Instance != null && AreaManager.Instance.CurrentArea != null)
			{
				if (this.currentArea == null)
				{
					return;
				}
				if (!this.currentArea.IsSpawned)
				{
					return;
				}
				if (!this.currentArea.SpawnedArea.initialized || this.currentArea.SpawnedArea.isCulled)
				{
					return;
				}
			}
			if (this.currentLocomotion.isGrounded && this.state == Creature.State.Alive)
			{
				this.RefreshFallState(Creature.FallState.None, false);
				return;
			}
			if (this.state == Creature.State.Destabilized)
			{
				RaycastHit raycastHit;
				float groundDistance;
				if (!this.ragdoll.SphereCastGround(this.locomotion.capsuleCollider.radius, this.morphology.hipsHeight, out raycastHit, out groundDistance))
				{
					this.RefreshFallState(Creature.FallState.Falling, false);
					return;
				}
				if (this.ragdoll.rootPart.physicBody.velocity.magnitude >= this.groundStabilizationMaxVelocity)
				{
					this.RefreshFallState(Creature.FallState.NearGround, false);
					return;
				}
				this.groundStabilizeDuration += Time.deltaTime;
				if (this.groundStabilizeDuration > this.groundStabilizationMinDuration)
				{
					this.RefreshFallState(Creature.FallState.StabilizedOnGround, false);
					return;
				}
				this.RefreshFallState(Creature.FallState.Stabilizing, false);
				return;
			}
			else
			{
				RaycastHit raycastHit2;
				float groundDistance2;
				if (!this.currentLocomotion.SphereCastGround(this.fallAliveDestabilizeHeight, out raycastHit2, out groundDistance2))
				{
					bool fallingIntoWater = false;
					float waterHeight;
					if (Water.exist && Water.current.TryGetWaterHeight(this.currentLocomotion.groundHit.point, out waterHeight))
					{
						fallingIntoWater = (this.currentLocomotion.groundHit.point.y < waterHeight - this.morphology.height * this.swimFallAnimationRatio);
					}
					if (!this.player && !fallingIntoWater && this.data.destabilizeOnFall)
					{
						this.ragdoll.SetState(Ragdoll.State.Destabilized);
					}
					this.RefreshFallState(Creature.FallState.Falling, false);
					return;
				}
				if (groundDistance2 < this.fallAliveAnimationHeight)
				{
					this.RefreshFallState(Creature.FallState.NearGround, false);
					return;
				}
				this.RefreshFallState(Creature.FallState.Falling, false);
				return;
			}
		}

		// Token: 0x060019C7 RID: 6599 RVA: 0x000AB924 File Offset: 0x000A9B24
		protected void RefreshFallState(Creature.FallState newState, bool force = false)
		{
			if (force || this.fallState != newState)
			{
				this.fallState = newState;
				if (newState == Creature.FallState.None)
				{
					this.animator.SetBool(Creature.hashFalling, false);
					this.groundStabilizeDuration = 0f;
				}
				else if (newState == Creature.FallState.NearGround)
				{
					this.animator.SetBool(Creature.hashFalling, this.state != Creature.State.Alive);
				}
				else if (newState == Creature.FallState.Stabilizing)
				{
					this.animator.SetBool(Creature.hashFalling, true);
				}
				else if (newState == Creature.FallState.StabilizedOnGround)
				{
					this.groundStabilizationLastTime = Time.time;
					this.animator.SetBool(Creature.hashFalling, true);
				}
				else
				{
					this.animator.SetBool(Creature.hashFalling, true);
					this.groundStabilizeDuration = 0f;
				}
				if (this.OnFallEvent != null)
				{
					this.OnFallEvent(this.fallState);
				}
			}
		}

		// Token: 0x060019C8 RID: 6600 RVA: 0x000AB9F9 File Offset: 0x000A9BF9
		public virtual void SetAnimatorHeightRatio(float height)
		{
			this.animator.SetFloat(Creature.hashHeight, height);
		}

		// Token: 0x060019C9 RID: 6601 RVA: 0x000ABA0C File Offset: 0x000A9C0C
		public virtual void SetGrabbedObjectLayer(LayerName layerName)
		{
			RagdollHand ragdollHand = this.handLeft;
			UnityEngine.Object exists;
			if (ragdollHand == null)
			{
				exists = null;
			}
			else
			{
				Handle grabbedHandle = ragdollHand.grabbedHandle;
				exists = ((grabbedHandle != null) ? grabbedHandle.item : null);
			}
			if (exists)
			{
				this.handLeft.grabbedHandle.item.SetColliderAndMeshLayer(GameManager.GetLayer(layerName), false);
			}
			RagdollHand ragdollHand2 = this.handRight;
			UnityEngine.Object exists2;
			if (ragdollHand2 == null)
			{
				exists2 = null;
			}
			else
			{
				Handle grabbedHandle2 = ragdollHand2.grabbedHandle;
				exists2 = ((grabbedHandle2 != null) ? grabbedHandle2.item : null);
			}
			if (exists2)
			{
				this.handRight.grabbedHandle.item.SetColliderAndMeshLayer(GameManager.GetLayer(layerName), false);
			}
		}

		// Token: 0x060019CA RID: 6602 RVA: 0x000ABA9C File Offset: 0x000A9C9C
		public virtual void RefreshCollisionOfGrabbedItems()
		{
			RagdollHand ragdollHand = this.handLeft;
			UnityEngine.Object exists;
			if (ragdollHand == null)
			{
				exists = null;
			}
			else
			{
				Handle grabbedHandle = ragdollHand.grabbedHandle;
				exists = ((grabbedHandle != null) ? grabbedHandle.item : null);
			}
			if (exists)
			{
				this.handLeft.grabbedHandle.item.RefreshCollision(false);
			}
			RagdollHand ragdollHand2 = this.handRight;
			UnityEngine.Object exists2;
			if (ragdollHand2 == null)
			{
				exists2 = null;
			}
			else
			{
				Handle grabbedHandle2 = ragdollHand2.grabbedHandle;
				exists2 = ((grabbedHandle2 != null) ? grabbedHandle2.item : null);
			}
			if (exists2)
			{
				this.handRight.grabbedHandle.item.RefreshCollision(false);
			}
		}

		// Token: 0x060019CB RID: 6603 RVA: 0x000ABB20 File Offset: 0x000A9D20
		public bool TryGetVfxManikinPart(string channel, out ManikinSmrPart part)
		{
			part = null;
			List<ManikinPart> foundParts = this.manikinLocations.GetPartsAtChannel(channel, null);
			if (foundParts == null || foundParts.Count == 0)
			{
				return false;
			}
			for (int i = 0; i < foundParts.Count; i++)
			{
				ManikinPart foundPart = foundParts[i];
				if (foundPart.isActiveAndEnabled)
				{
					ManikinGroupPart groupPart = foundPart as ManikinGroupPart;
					if (groupPart != null)
					{
						foreach (ManikinPart child in groupPart.ChildParts)
						{
							if (child.isActiveAndEnabled)
							{
								ManikinSmrPart smrPart = child as ManikinSmrPart;
								if (smrPart != null && smrPart.GetSkinnedMeshRenderer().enabled)
								{
									part = smrPart;
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		// Token: 0x060019CC RID: 6604 RVA: 0x000ABBC4 File Offset: 0x000A9DC4
		public void SetBodyMaterials(Material[] bodyMaterials)
		{
			if (bodyMaterials == null)
			{
				return;
			}
			if (bodyMaterials.Length == 0)
			{
				return;
			}
			for (int i = 0; i < bodyMaterials.Length; i++)
			{
				ManikinProperty manikinProperty;
				if (this.TryGetManikinProperty(string.Format("EthnicGroupMaterialBodyLOD{0}", i), out manikinProperty))
				{
					this.manikinProperties.TryUpdateProperty(bodyMaterials[i], manikinProperty.set, true, 0);
				}
			}
		}

		// Token: 0x060019CD RID: 6605 RVA: 0x000ABC1C File Offset: 0x000A9E1C
		public void SetHandsMaterials(Material[] handsMaterials)
		{
			if (handsMaterials == null)
			{
				return;
			}
			if (handsMaterials.Length == 0)
			{
				return;
			}
			for (int i = 0; i < handsMaterials.Length; i++)
			{
				ManikinProperty manikinProperty;
				if (this.TryGetManikinProperty(string.Format("EthnicGroupMaterialHandsLOD{0}", i), out manikinProperty))
				{
					this.manikinProperties.TryUpdateProperty(handsMaterials[i], manikinProperty.set, true, 0);
				}
			}
		}

		/// <summary>
		/// Return the current ethnic group from it's id
		/// </summary>
		/// <param name="id">Id of the ethnic group to seartch</param>
		/// <returns>The matching ethnic group if found, the first one if not</returns>
		// Token: 0x060019CE RID: 6606 RVA: 0x000ABC74 File Offset: 0x000A9E74
		public CreatureData.EthnicGroup GetEthnicGroupFromId(string id)
		{
			for (int i = 0; i < this.data.ethnicGroups.Count; i++)
			{
				if (this.data.ethnicGroups[i].id == id)
				{
					return this.data.ethnicGroups[i];
				}
			}
			return this.data.ethnicGroups[0];
		}

		/// <summary>
		/// Sets the ethnic group of the creature.
		/// It will change its head wardrobe part if the current one doesn't match
		/// </summary>
		/// <param name="creatureDataEthnicGroup">Ethnic group to change to.</param>
		// Token: 0x060019CF RID: 6607 RVA: 0x000ABCE0 File Offset: 0x000A9EE0
		public void SetEthnicGroup(CreatureData.EthnicGroup creatureDataEthnicGroup)
		{
			if (creatureDataEthnicGroup == null)
			{
				return;
			}
			this.currentEthnicGroup = creatureDataEthnicGroup;
			ManikinWardrobeData currentHeadWardrobe = this.manikinLocations.GetWardrobeData("Head", ItemModuleWardrobe.GetLayer("Head", "Body"));
			ItemData currentHeadItemData = null;
			foreach (ItemData itemData in Catalog.GetDataList<ItemData>())
			{
				ItemModuleWardrobe itemModuleWardrobe = itemData.GetModule<ItemModuleWardrobe>();
				if (itemModuleWardrobe != null && itemModuleWardrobe.category == Equipment.WardRobeCategory.Body)
				{
					for (int i = 0; i < itemModuleWardrobe.wardrobes.Count; i++)
					{
						if (itemModuleWardrobe.wardrobes[i].manikinWardrobeData == currentHeadWardrobe)
						{
							currentHeadItemData = itemData;
							break;
						}
					}
				}
			}
			if (currentHeadItemData == null)
			{
				return;
			}
			bool found = false;
			if (creatureDataEthnicGroup.allowedHeadsIDs != null)
			{
				for (int j = 0; j < creatureDataEthnicGroup.allowedHeadsIDs.Count; j++)
				{
					if (creatureDataEthnicGroup.allowedHeadsIDs[j] == currentHeadItemData.id)
					{
						found = true;
						break;
					}
				}
			}
			if (!found && creatureDataEthnicGroup.allowedHeadsIDs != null)
			{
				ItemModuleWardrobe headItemModuleWardrobe = null;
				foreach (ItemData itemData2 in Catalog.GetDataList<ItemData>())
				{
					ItemModuleWardrobe itemModuleWardrobe2 = itemData2.GetModule<ItemModuleWardrobe>();
					if (itemModuleWardrobe2 != null && itemModuleWardrobe2.category == Equipment.WardRobeCategory.Body)
					{
						ItemModuleWardrobe.CreatureWardrobe creatureWardrobe = itemModuleWardrobe2.GetWardrobe(this);
						if (creatureWardrobe != null && creatureWardrobe.manikinWardrobeData != null && itemModuleWardrobe2.IsCompatible(this))
						{
							for (int k = 0; k < creatureDataEthnicGroup.allowedHeadsIDs.Count; k++)
							{
								if (creatureDataEthnicGroup.allowedHeadsIDs[k] == itemModuleWardrobe2.itemData.id)
								{
									headItemModuleWardrobe = itemModuleWardrobe2;
									break;
								}
							}
						}
					}
				}
				if (headItemModuleWardrobe != null)
				{
					this.equipment.EquipWardrobe(new ItemContent(headItemModuleWardrobe.itemData, null, null, 1), true);
				}
			}
			this.SetBodyMaterials(new Material[]
			{
				creatureDataEthnicGroup.bodyMaterialLod0,
				creatureDataEthnicGroup.bodyMaterialLod1
			});
			this.SetHandsMaterials(new Material[]
			{
				creatureDataEthnicGroup.handsMaterialLod0,
				creatureDataEthnicGroup.handsMaterialLod1
			});
			this.manikinProperties.UpdateProperties();
		}

		/// <summary>
		/// Update the property of the creature when the head changed
		/// </summary>
		/// <param name="eventTime"></param>
		// Token: 0x060019D0 RID: 6608 RVA: 0x000ABF28 File Offset: 0x000AA128
		private void UpdateManikinAfterHeadChange(EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				this.manikinProperties.UpdateProperties();
				if (this.isPlayer)
				{
					foreach (ManikinPart manikinPart in this.manikinLocations.GetPartsAtChannel("Head", null))
					{
						if (manikinPart.name.Contains("Eyes"))
						{
							Renderer[] array = manikinPart.GetRenderers();
							for (int i = 0; i < array.Length; i++)
							{
								array[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
							}
						}
					}
				}
			}
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x000ABFC8 File Offset: 0x000AA1C8
		public bool AddJointForceMultiplier(object handler, float position, float rotation)
		{
			ValueTuple<float, float> current;
			if (this.jointForceMultipliers.TryGetValue(handler, out current))
			{
				ValueTuple<float, float> valueTuple = current;
				if (valueTuple.Item1 == position && valueTuple.Item2 == rotation)
				{
					return false;
				}
			}
			this.jointForceMultipliers[handler] = new ValueTuple<float, float>(position, rotation);
			this.RefreshJointForceMultipliers();
			return true;
		}

		// Token: 0x060019D2 RID: 6610 RVA: 0x000AC019 File Offset: 0x000AA219
		public void RemoveJointForceMultiplier(object handler)
		{
			if (!this.jointForceMultipliers.ContainsKey(handler))
			{
				return;
			}
			this.jointForceMultipliers.Remove(handler);
			this.RefreshJointForceMultipliers();
		}

		// Token: 0x060019D3 RID: 6611 RVA: 0x000AC03D File Offset: 0x000AA23D
		public void ClearJointForceMultipliers()
		{
			bool flag = this.jointForceMultipliers.Count > 0;
			this.jointForceMultipliers.Clear();
			if (flag)
			{
				this.RefreshJointForceMultipliers();
			}
		}

		// Token: 0x060019D4 RID: 6612 RVA: 0x000AC060 File Offset: 0x000AA260
		public void RefreshJointForceMultipliers()
		{
			this.jointPosForceMult = 1f;
			this.jointRotForceMult = 1f;
			foreach (ValueTuple<float, float> valueTuple in this.jointForceMultipliers.Values)
			{
				float position = valueTuple.Item1;
				float rotation = valueTuple.Item2;
				this.jointPosForceMult *= position;
				this.jointRotForceMult *= rotation;
			}
			Handle grabbedHandle = this.handLeft.grabbedHandle;
			if (grabbedHandle != null)
			{
				grabbedHandle.RefreshJointModifiers();
			}
			PlayerHand playerHand = this.handLeft.playerHand;
			if (playerHand != null)
			{
				playerHand.link.RefreshJointModifiers();
			}
			Handle grabbedHandle2 = this.handRight.grabbedHandle;
			if (grabbedHandle2 != null)
			{
				grabbedHandle2.RefreshJointModifiers();
			}
			PlayerHand playerHand2 = this.handRight.playerHand;
			if (playerHand2 == null)
			{
				return;
			}
			playerHand2.link.RefreshJointModifiers();
		}

		// Token: 0x060019D5 RID: 6613 RVA: 0x000AC150 File Offset: 0x000AA350
		public Vector3 GetPositionJointConfig()
		{
			return new Vector3(this.data.forcePositionSpringDamper.x * this.jointPosForceMult, this.data.forcePositionSpringDamper.y * Mathf.Max(1f, this.jointPosForceMult / 2f), this.jointPosForceMult);
		}

		// Token: 0x060019D6 RID: 6614 RVA: 0x000AC1A8 File Offset: 0x000AA3A8
		public Vector3 GetRotationJointConfig()
		{
			return new Vector3(this.data.forceRotationSpringDamper.x * this.jointRotForceMult, this.data.forceRotationSpringDamper.y * Mathf.Max(1f, this.jointRotForceMult / 2f), this.jointRotForceMult);
		}

		// Token: 0x04001803 RID: 6147
		public static string secondWindId = "SecondWind";

		// Token: 0x04001804 RID: 6148
		[Tooltip("The Creature ID of the creature.")]
		public string creatureId;

		// Token: 0x04001805 RID: 6149
		[Tooltip("The animator the creature will use for animations")]
		public Animator animator;

		// Token: 0x04001806 RID: 6150
		[Tooltip("The LOD Group for the creature meshes, if it has one.")]
		public LODGroup lodGroup;

		// Token: 0x04001807 RID: 6151
		[Tooltip("The container the creature uses for its parts.")]
		public Container container;

		// Token: 0x04001808 RID: 6152
		[Tooltip("The transform used for eye rotation")]
		public Transform centerEyes;

		// Token: 0x04001809 RID: 6153
		[Tooltip("The offset used for the eye camera")]
		public Vector3 eyeCameraOffset;

		// Token: 0x0400180A RID: 6154
		[Tooltip("If the creature use a VFX renderer, put it here.")]
		public Renderer vfxRenderer;

		// Token: 0x0400180B RID: 6155
		[NonSerialized]
		public Ragdoll ragdoll;

		// Token: 0x0400180C RID: 6156
		[NonSerialized]
		public Brain brain;

		// Token: 0x0400180D RID: 6157
		[NonSerialized]
		public Locomotion locomotion;

		// Token: 0x0400180E RID: 6158
		[NonSerialized]
		public RagdollHand handLeft;

		// Token: 0x0400180F RID: 6159
		[NonSerialized]
		public RagdollHand handRight;

		// Token: 0x04001810 RID: 6160
		[NonSerialized]
		public Equipment equipment;

		// Token: 0x04001811 RID: 6161
		[NonSerialized]
		public Mana mana;

		// Token: 0x04001812 RID: 6162
		[NonSerialized]
		public FeetClimber climber;

		// Token: 0x04001813 RID: 6163
		[NonSerialized]
		public RagdollFoot footLeft;

		// Token: 0x04001814 RID: 6164
		[NonSerialized]
		public RagdollFoot footRight;

		// Token: 0x04001815 RID: 6165
		[NonSerialized]
		public LightVolumeReceiver lightVolumeReceiver;

		// Token: 0x04001816 RID: 6166
		[NonSerialized]
		public bool wasLoadedForCharacterSelect;

		// Token: 0x04001817 RID: 6167
		[NonSerialized]
		public HashSet<string> heldCrystalImbues;

		// Token: 0x04001818 RID: 6168
		[Tooltip("References the class to tell in-game skills if the player is airborne.")]
		public AirHelper airHelper;

		// Token: 0x04001819 RID: 6169
		[Tooltip("References the class for armor SFX")]
		public ArmorSFX armorSFX;

		// Token: 0x0400181A RID: 6170
		protected float waterEyesEnterUnderwaterTime;

		// Token: 0x0400181B RID: 6171
		protected float waterLastDrowningTime;

		// Token: 0x0400181C RID: 6172
		[NonSerialized]
		public bool eyesUnderwater;

		// Token: 0x0400181F RID: 6175
		[Header("Speak")]
		[Tooltip("Reference the jaw bone for creature speaking")]
		public Transform jaw;

		// Token: 0x04001820 RID: 6176
		[Tooltip("Max rotation of the jaw when it speaks.")]
		public Vector3 jawMaxRotation = new Vector3(0f, -30f, 0f);

		// Token: 0x04001821 RID: 6177
		[Header("Head")]
		[Tooltip("When enabled, the creature blinks")]
		public bool autoEyeClipsActive = true;

		// Token: 0x04001822 RID: 6178
		[Tooltip("Reference the eyes for blinking")]
		public List<CreatureEye> allEyes = new List<CreatureEye>();

		// Token: 0x04001823 RID: 6179
		[Tooltip("Reference the eye animation clips")]
		public List<CreatureData.EyeClip> eyeClips = new List<CreatureData.EyeClip>();

		// Token: 0x04001824 RID: 6180
		[Tooltip("Reference the meshes to hide when in first person.")]
		public List<SkinnedMeshRenderer> meshesToHideForFPV;

		// Token: 0x04001825 RID: 6181
		[Header("Fall")]
		[Tooltip("The height off the ground the creature needs to be before they play the fall animation")]
		public float fallAliveAnimationHeight = 0.5f;

		// Token: 0x04001826 RID: 6182
		[Tooltip("The height the creautre needs to fall before their ragdoll distabilizes")]
		public float fallAliveDestabilizeHeight = 3f;

		// Token: 0x04001827 RID: 6183
		[Tooltip("The maximum velocity for the creatures body before it can stand up")]
		public float groundStabilizationMaxVelocity = 1f;

		// Token: 0x04001828 RID: 6184
		[Tooltip("The minimum duration a creature is on the ground before getting up.")]
		public float groundStabilizationMinDuration = 3f;

		// Token: 0x04001829 RID: 6185
		[Tooltip("How submerged the creature needs to be before they can start the swimming animations.")]
		[Range(0f, 1f)]
		public float swimFallAnimationRatio = 0.6f;

		// Token: 0x0400182A RID: 6186
		[Tooltip("Toggle T Pose for the creature.")]
		public bool toogleTPose;

		// Token: 0x0400182B RID: 6187
		[Header("Movement")]
		public bool stepEnabled;

		// Token: 0x0400182C RID: 6188
		public float stepThreshold = 0.2f;

		// Token: 0x0400182D RID: 6189
		public bool turnRelativeToHand = true;

		// Token: 0x0400182E RID: 6190
		public float headMinAngle = 30f;

		// Token: 0x0400182F RID: 6191
		public float headMaxAngle = 80f;

		// Token: 0x04001830 RID: 6192
		public float handToBodyRotationMaxVelocity = 2f;

		// Token: 0x04001831 RID: 6193
		public float handToBodyRotationMaxAngle = 30f;

		// Token: 0x04001832 RID: 6194
		public float turnSpeed = 6f;

		// Token: 0x04001833 RID: 6195
		public float ikLocomotionSpeedThreshold = 1f;

		// Token: 0x04001834 RID: 6196
		public float ikLocomotionAngularSpeedThreshold = 30f;

		// Token: 0x04001835 RID: 6197
		public FloatHandler detectionFOVModifier;

		// Token: 0x04001836 RID: 6198
		public FloatHandler hitEnvironmentDamageModifier;

		// Token: 0x04001837 RID: 6199
		public FloatHandler healthModifier;

		// Token: 0x04001838 RID: 6200
		public static int hashDynamicOneShot;

		// Token: 0x04001839 RID: 6201
		public static int hashDynamicLoop;

		// Token: 0x0400183A RID: 6202
		public static int hashDynamicLoopAdd;

		// Token: 0x0400183B RID: 6203
		public static int hashDynamicLoop3;

		// Token: 0x0400183C RID: 6204
		public static int hashDynamicInterrupt;

		// Token: 0x0400183D RID: 6205
		public static int hashDynamicSpeedMultiplier;

		// Token: 0x0400183E RID: 6206
		public static int hashDynamicMirror;

		// Token: 0x0400183F RID: 6207
		public static int hashDynamicUpperOneShot;

		// Token: 0x04001840 RID: 6208
		public static int hashDynamicUpperLoop;

		// Token: 0x04001841 RID: 6209
		public static int hashDynamicUpperMultiplier;

		// Token: 0x04001842 RID: 6210
		public static int hashDynamicUpperMirror;

		// Token: 0x04001843 RID: 6211
		public static int hashExitDynamic;

		// Token: 0x04001844 RID: 6212
		public static int hashInvokeCallback;

		// Token: 0x04001845 RID: 6213
		public static int hashIsBusy;

		// Token: 0x04001846 RID: 6214
		public static int hashFeminity;

		// Token: 0x04001847 RID: 6215
		public static int hashHeight;

		// Token: 0x04001848 RID: 6216
		public static int hashFalling;

		// Token: 0x04001849 RID: 6217
		public static int hashUnderwater;

		// Token: 0x0400184A RID: 6218
		public static int hashGetUp;

		// Token: 0x0400184B RID: 6219
		public static int hashTstance;

		// Token: 0x0400184C RID: 6220
		public static int hashStaticIdle;

		// Token: 0x0400184D RID: 6221
		public static int hashFreeHands;

		// Token: 0x0400184E RID: 6222
		public static bool hashInitialized;

		// Token: 0x0400184F RID: 6223
		public Creature.FallState fallState;

		// Token: 0x04001850 RID: 6224
		[NonSerialized]
		public Morphology morphology;

		// Token: 0x04001851 RID: 6225
		protected float groundStabilizeDuration;

		// Token: 0x04001852 RID: 6226
		[NonSerialized]
		public float groundStabilizationLastTime;

		// Token: 0x0400185A RID: 6234
		[NonSerialized]
		public float turnTargetAngle;

		// Token: 0x0400185B RID: 6235
		[NonSerialized]
		public Vector3 stepTargetPos;

		// Token: 0x0400185C RID: 6236
		[NonSerialized]
		public CollisionInstance lastDamage;

		// Token: 0x0400185D RID: 6237
		[NonSerialized]
		public ManikinPartList manikinParts;

		// Token: 0x0400185E RID: 6238
		[NonSerialized]
		public ManikinLocations manikinLocations;

		// Token: 0x0400185F RID: 6239
		[NonSerialized]
		public ManikinProperties manikinProperties;

		// Token: 0x04001860 RID: 6240
		[NonSerialized]
		public ManikinLocations.JsonWardrobeLocations orgWardrobeLocations;

		// Token: 0x04001861 RID: 6241
		[NonSerialized]
		private List<ManikinPart> headManikinPart = new List<ManikinPart>(10);

		// Token: 0x04001862 RID: 6242
		[NonSerialized]
		public Player player;

		// Token: 0x04001863 RID: 6243
		[NonSerialized]
		public List<Holder> holders;

		// Token: 0x04001864 RID: 6244
		[NonSerialized]
		public List<Creature.RendererData> renderers = new List<Creature.RendererData>();

		// Token: 0x04001865 RID: 6245
		[NonSerialized]
		public List<RevealDecal> revealDecals = new List<RevealDecal>();

		// Token: 0x04001866 RID: 6246
		[NonSerialized]
		public CreatureMouthRelay mouthRelay;

		// Token: 0x04001867 RID: 6247
		public static List<Creature> all = new List<Creature>();

		// Token: 0x04001868 RID: 6248
		public static List<Creature> allActive = new List<Creature>();

		// Token: 0x04001869 RID: 6249
		public static Action<Creature> onAllActiveRemoved;

		// Token: 0x0400186A RID: 6250
		[NonSerialized]
		public static Dictionary<string, AnimatorBundle> creatureAnimatorControllers = new Dictionary<string, AnimatorBundle>();

		// Token: 0x0400186B RID: 6251
		[NonSerialized]
		public bool isPlayer;

		// Token: 0x0400186C RID: 6252
		[NonSerialized]
		public bool hidden;

		// Token: 0x0400186D RID: 6253
		[NonSerialized]
		public bool holsterItemsHidden;

		// Token: 0x0400186E RID: 6254
		[NonSerialized]
		public HashSet<string> heldImbueIDs;

		// Token: 0x0400186F RID: 6255
		public int factionId;

		// Token: 0x04001870 RID: 6256
		public GameData.Faction faction;

		// Token: 0x04001871 RID: 6257
		[NonSerialized]
		public CreatureData data;

		// Token: 0x04001872 RID: 6258
		[NonSerialized]
		public bool pooled;

		// Token: 0x04001873 RID: 6259
		[NonSerialized]
		public WaveData.Group spawnGroup;

		// Token: 0x04001874 RID: 6260
		[NonSerialized]
		public CreatureSpawner creatureSpawner;

		// Token: 0x04001875 RID: 6261
		[NonSerialized]
		public bool countTowardsMaxAlive;

		// Token: 0x04001876 RID: 6262
		[NonSerialized]
		public float spawnTime;

		// Token: 0x04001877 RID: 6263
		[NonSerialized]
		public float lastInteractionTime;

		// Token: 0x04001878 RID: 6264
		[NonSerialized]
		public Creature lastInteractionCreature;

		// Token: 0x04001879 RID: 6265
		[NonSerialized]
		public float swimVerticalRatio;

		// Token: 0x0400187A RID: 6266
		[NonSerialized]
		public CreatureData.EthnicGroup currentEthnicGroup;

		// Token: 0x0400187B RID: 6267
		public bool initialized;

		// Token: 0x0400187C RID: 6268
		public bool loaded;

		// Token: 0x0400187D RID: 6269
		public bool isPlayingDynamicAnimation;

		// Token: 0x0400187E RID: 6270
		protected Action dynamicAnimationendEndCallback;

		// Token: 0x0400187F RID: 6271
		protected Action upperDynamicAnimationendEndCallback;

		// Token: 0x04001880 RID: 6272
		protected AnimatorOverrideController animatorOverrideController;

		// Token: 0x04001881 RID: 6273
		protected KeyValuePair<AnimationClip, AnimationClip>[] animationClipOverrides;

		// Token: 0x04001885 RID: 6277
		[NonSerialized]
		public bool updateReveal;

		// Token: 0x04001886 RID: 6278
		public bool isKilled;

		// Token: 0x04001887 RID: 6279
		public static Creature.ReplaceClipIndexHolder clipIndex = new Creature.ReplaceClipIndexHolder();

		// Token: 0x04001888 RID: 6280
		[NonSerialized]
		public bool isSwimming;

		// Token: 0x04001889 RID: 6281
		public float animationDampTime = 0.1f;

		// Token: 0x0400188A RID: 6282
		public float verticalDampTime = 0.5f;

		// Token: 0x0400188B RID: 6283
		public float stationaryVelocityThreshold = 0.01f;

		// Token: 0x0400188C RID: 6284
		public float turnAnimSpeed = 0.007f;

		// Token: 0x0400188D RID: 6285
		public static int hashStrafe;

		// Token: 0x0400188E RID: 6286
		public static int hashTurn;

		// Token: 0x0400188F RID: 6287
		public static int hashSpeed;

		// Token: 0x04001890 RID: 6288
		public static int hashVerticalSpeed;

		// Token: 0x04001891 RID: 6289
		[NonSerialized]
		public SpawnableArea initialArea;

		// Token: 0x04001892 RID: 6290
		[NonSerialized]
		public int areaSpawnerIndex = -1;

		// Token: 0x04001893 RID: 6291
		[NonSerialized]
		public SpawnableArea currentArea;

		// Token: 0x04001894 RID: 6292
		[NonSerialized]
		public bool isCulled;

		// Token: 0x04001895 RID: 6293
		protected bool cullingDetectionEnabled;

		// Token: 0x04001896 RID: 6294
		protected float cullingDetectionCycleSpeed = 1f;

		// Token: 0x04001897 RID: 6295
		protected float cullingDetectionCycleTime;

		// Token: 0x04001898 RID: 6296
		[Header("Health")]
		public float _currentHealth = 50f;

		// Token: 0x04001899 RID: 6297
		[Header("Health")]
		public float _currentMaxHealth = 50f;

		// Token: 0x0400189A RID: 6298
		public float resurrectMinHeal = 5f;

		// Token: 0x0400189B RID: 6299
		public static bool meshRaycast = true;

		// Token: 0x040018A2 RID: 6306
		[NonSerialized]
		public float lastDamageTime;

		// Token: 0x040018A3 RID: 6307
		public Dictionary<object, float> damageMultipliers;

		// Token: 0x040018A4 RID: 6308
		[TupleElementNames(new string[]
		{
			"position",
			"rotation"
		})]
		private Dictionary<object, ValueTuple<float, float>> jointForceMultipliers;

		// Token: 0x040018A5 RID: 6309
		private float jointPosForceMult = 1f;

		// Token: 0x040018A6 RID: 6310
		private float jointRotForceMult = 1f;

		// Token: 0x0200087A RID: 2170
		public enum StaggerAnimation
		{
			// Token: 0x040041C1 RID: 16833
			Default,
			// Token: 0x040041C2 RID: 16834
			Parry,
			// Token: 0x040041C3 RID: 16835
			Head,
			// Token: 0x040041C4 RID: 16836
			Torso,
			// Token: 0x040041C5 RID: 16837
			Legs,
			// Token: 0x040041C6 RID: 16838
			FallGround,
			// Token: 0x040041C7 RID: 16839
			Riposte
		}

		// Token: 0x0200087B RID: 2171
		public enum PushType
		{
			// Token: 0x040041C9 RID: 16841
			Magic,
			// Token: 0x040041CA RID: 16842
			Grab,
			// Token: 0x040041CB RID: 16843
			Hit,
			// Token: 0x040041CC RID: 16844
			Parry
		}

		// Token: 0x0200087C RID: 2172
		public enum FallState
		{
			// Token: 0x040041CE RID: 16846
			None,
			// Token: 0x040041CF RID: 16847
			Falling,
			// Token: 0x040041D0 RID: 16848
			NearGround,
			// Token: 0x040041D1 RID: 16849
			Stabilizing,
			// Token: 0x040041D2 RID: 16850
			StabilizedOnGround
		}

		// Token: 0x0200087D RID: 2173
		// (Invoke) Token: 0x0600404B RID: 16459
		public delegate void FallEvent(Creature.FallState state);

		// Token: 0x0200087E RID: 2174
		// (Invoke) Token: 0x0600404F RID: 16463
		public delegate void ForceSkillLoadEvent(SkillData skill);

		// Token: 0x0200087F RID: 2175
		// (Invoke) Token: 0x06004053 RID: 16467
		public delegate void ImbueChangeEvent(Creature creature, HashSet<string> before, HashSet<string> after);

		// Token: 0x02000880 RID: 2176
		// (Invoke) Token: 0x06004057 RID: 16471
		public delegate void DespawnEvent(EventTime eventTime);

		// Token: 0x02000881 RID: 2177
		// (Invoke) Token: 0x0600405B RID: 16475
		public delegate void ThrowEvent(RagdollHand hand, Handle handle);

		// Token: 0x02000882 RID: 2178
		// (Invoke) Token: 0x0600405F RID: 16479
		public delegate void ThisCreatureAttackEvent(Creature targetCreature, Transform targetTransform, BrainModuleAttack.AttackType type, BrainModuleAttack.AttackStage stage);

		// Token: 0x02000883 RID: 2179
		public class RendererData
		{
			// Token: 0x06004062 RID: 16482 RVA: 0x00188F64 File Offset: 0x00187164
			public RendererData(SkinnedMeshRenderer renderer, int lod, MeshPart meshPart = null, RevealDecal revealDecal = null, ManikinPart manikinPart = null)
			{
				this.renderer = renderer;
				this.lod = lod;
				this.meshPart = meshPart;
				this.revealDecal = revealDecal;
				this.manikinPart = manikinPart;
			}

			// Token: 0x040041D3 RID: 16851
			public SkinnedMeshRenderer renderer;

			// Token: 0x040041D4 RID: 16852
			public SkinnedMeshRenderer splitRenderer;

			// Token: 0x040041D5 RID: 16853
			public MeshPart meshPart;

			// Token: 0x040041D6 RID: 16854
			public ManikinPart manikinPart;

			// Token: 0x040041D7 RID: 16855
			public RevealDecal revealDecal;

			// Token: 0x040041D8 RID: 16856
			public RevealDecal splitReveal;

			// Token: 0x040041D9 RID: 16857
			public int lod;
		}

		// Token: 0x02000884 RID: 2180
		// (Invoke) Token: 0x06004064 RID: 16484
		public delegate void ZoneEvent(Zone zone, bool enter);

		// Token: 0x02000885 RID: 2181
		// (Invoke) Token: 0x06004068 RID: 16488
		public delegate void SimpleDelegate();

		// Token: 0x02000886 RID: 2182
		public enum State
		{
			// Token: 0x040041DB RID: 16859
			Dead,
			// Token: 0x040041DC RID: 16860
			Destabilized,
			// Token: 0x040041DD RID: 16861
			Alive
		}

		// Token: 0x02000887 RID: 2183
		public enum ProtectToAim
		{
			// Token: 0x040041DF RID: 16863
			Protect,
			// Token: 0x040041E0 RID: 16864
			Idle,
			// Token: 0x040041E1 RID: 16865
			Aim
		}

		// Token: 0x02000888 RID: 2184
		public enum AnimFootStep
		{
			// Token: 0x040041E3 RID: 16867
			Slow,
			// Token: 0x040041E4 RID: 16868
			Walk,
			// Token: 0x040041E5 RID: 16869
			Run
		}

		// Token: 0x02000889 RID: 2185
		public enum ColorModifier
		{
			// Token: 0x040041E7 RID: 16871
			Hair,
			// Token: 0x040041E8 RID: 16872
			HairSecondary,
			// Token: 0x040041E9 RID: 16873
			HairSpecular,
			// Token: 0x040041EA RID: 16874
			EyesIris,
			// Token: 0x040041EB RID: 16875
			EyesSclera,
			// Token: 0x040041EC RID: 16876
			Skin
		}

		// Token: 0x0200088A RID: 2186
		public class ReplaceClipIndexHolder
		{
			// Token: 0x17000515 RID: 1301
			// (get) Token: 0x0600406B RID: 16491 RVA: 0x00188F91 File Offset: 0x00187191
			// (set) Token: 0x0600406C RID: 16492 RVA: 0x00188F99 File Offset: 0x00187199
			public int count { get; protected set; }

			// Token: 0x17000516 RID: 1302
			// (get) Token: 0x0600406D RID: 16493 RVA: 0x00188FA2 File Offset: 0x001871A2
			// (set) Token: 0x0600406E RID: 16494 RVA: 0x00188FAA File Offset: 0x001871AA
			public int dynamicStartClipA { get; protected set; }

			// Token: 0x17000517 RID: 1303
			// (get) Token: 0x0600406F RID: 16495 RVA: 0x00188FB3 File Offset: 0x001871B3
			// (set) Token: 0x06004070 RID: 16496 RVA: 0x00188FBB File Offset: 0x001871BB
			public int dynamicStartClipB { get; protected set; }

			// Token: 0x17000518 RID: 1304
			// (get) Token: 0x06004071 RID: 16497 RVA: 0x00188FC4 File Offset: 0x001871C4
			// (set) Token: 0x06004072 RID: 16498 RVA: 0x00188FCC File Offset: 0x001871CC
			public int dynamicLoopClip { get; protected set; }

			// Token: 0x17000519 RID: 1305
			// (get) Token: 0x06004073 RID: 16499 RVA: 0x00188FD5 File Offset: 0x001871D5
			// (set) Token: 0x06004074 RID: 16500 RVA: 0x00188FDD File Offset: 0x001871DD
			public int dynamicLoopAddClip { get; protected set; }

			// Token: 0x1700051A RID: 1306
			// (get) Token: 0x06004075 RID: 16501 RVA: 0x00188FE6 File Offset: 0x001871E6
			// (set) Token: 0x06004076 RID: 16502 RVA: 0x00188FEE File Offset: 0x001871EE
			public int dynamicEndClip { get; protected set; }

			// Token: 0x1700051B RID: 1307
			// (get) Token: 0x06004077 RID: 16503 RVA: 0x00188FF7 File Offset: 0x001871F7
			// (set) Token: 0x06004078 RID: 16504 RVA: 0x00188FFF File Offset: 0x001871FF
			public int upperBodyDynamicClipA { get; protected set; }

			// Token: 0x1700051C RID: 1308
			// (get) Token: 0x06004079 RID: 16505 RVA: 0x00189008 File Offset: 0x00187208
			// (set) Token: 0x0600407A RID: 16506 RVA: 0x00189010 File Offset: 0x00187210
			public int upperBodyDynamicClipB { get; protected set; }

			// Token: 0x1700051D RID: 1309
			// (get) Token: 0x0600407B RID: 16507 RVA: 0x00189019 File Offset: 0x00187219
			// (set) Token: 0x0600407C RID: 16508 RVA: 0x00189021 File Offset: 0x00187221
			public int upperBodyDynamicLoopClip { get; protected set; }

			// Token: 0x1700051E RID: 1310
			// (get) Token: 0x0600407D RID: 16509 RVA: 0x0018902A File Offset: 0x0018722A
			// (set) Token: 0x0600407E RID: 16510 RVA: 0x00189032 File Offset: 0x00187232
			public int subStanceClipA { get; protected set; }

			// Token: 0x1700051F RID: 1311
			// (get) Token: 0x0600407F RID: 16511 RVA: 0x0018903B File Offset: 0x0018723B
			// (set) Token: 0x06004080 RID: 16512 RVA: 0x00189043 File Offset: 0x00187243
			public int subStanceClipB { get; protected set; }

			// Token: 0x17000520 RID: 1312
			// (get) Token: 0x06004081 RID: 16513 RVA: 0x0018904C File Offset: 0x0018724C
			// (set) Token: 0x06004082 RID: 16514 RVA: 0x00189054 File Offset: 0x00187254
			public int upperLeftGuard { get; protected set; }

			// Token: 0x17000521 RID: 1313
			// (get) Token: 0x06004083 RID: 16515 RVA: 0x0018905D File Offset: 0x0018725D
			// (set) Token: 0x06004084 RID: 16516 RVA: 0x00189065 File Offset: 0x00187265
			public int upperRightGuard { get; protected set; }

			// Token: 0x17000522 RID: 1314
			// (get) Token: 0x06004085 RID: 16517 RVA: 0x0018906E File Offset: 0x0018726E
			// (set) Token: 0x06004086 RID: 16518 RVA: 0x00189076 File Offset: 0x00187276
			public int leftGuard { get; protected set; }

			// Token: 0x17000523 RID: 1315
			// (get) Token: 0x06004087 RID: 16519 RVA: 0x0018907F File Offset: 0x0018727F
			// (set) Token: 0x06004088 RID: 16520 RVA: 0x00189087 File Offset: 0x00187287
			public int midGuard { get; protected set; }

			// Token: 0x17000524 RID: 1316
			// (get) Token: 0x06004089 RID: 16521 RVA: 0x00189090 File Offset: 0x00187290
			// (set) Token: 0x0600408A RID: 16522 RVA: 0x00189098 File Offset: 0x00187298
			public int rightGuard { get; protected set; }

			// Token: 0x17000525 RID: 1317
			// (get) Token: 0x0600408B RID: 16523 RVA: 0x001890A1 File Offset: 0x001872A1
			// (set) Token: 0x0600408C RID: 16524 RVA: 0x001890A9 File Offset: 0x001872A9
			public int lowerLeftGuard { get; protected set; }

			// Token: 0x17000526 RID: 1318
			// (get) Token: 0x0600408D RID: 16525 RVA: 0x001890B2 File Offset: 0x001872B2
			// (set) Token: 0x0600408E RID: 16526 RVA: 0x001890BA File Offset: 0x001872BA
			public int lowerRightGuard { get; protected set; }

			// Token: 0x0600408F RID: 16527 RVA: 0x001890C4 File Offset: 0x001872C4
			public ReplaceClipIndexHolder()
			{
				this.count = 0;
				int count = this.count;
				this.count = count + 1;
				this.dynamicStartClipA = count;
				count = this.count;
				this.count = count + 1;
				this.dynamicStartClipB = count;
				count = this.count;
				this.count = count + 1;
				this.dynamicLoopClip = count;
				count = this.count;
				this.count = count + 1;
				this.dynamicLoopAddClip = count;
				count = this.count;
				this.count = count + 1;
				this.dynamicEndClip = count;
				count = this.count;
				this.count = count + 1;
				this.upperBodyDynamicClipA = count;
				count = this.count;
				this.count = count + 1;
				this.upperBodyDynamicClipB = count;
				count = this.count;
				this.count = count + 1;
				this.upperBodyDynamicLoopClip = count;
				count = this.count;
				this.count = count + 1;
				this.subStanceClipA = count;
				count = this.count;
				this.count = count + 1;
				this.subStanceClipB = count;
				count = this.count;
				this.count = count + 1;
				this.upperLeftGuard = count;
				count = this.count;
				this.count = count + 1;
				this.upperRightGuard = count;
				count = this.count;
				this.count = count + 1;
				this.leftGuard = count;
				count = this.count;
				this.count = count + 1;
				this.midGuard = count;
				count = this.count;
				this.count = count + 1;
				this.rightGuard = count;
				count = this.count;
				this.count = count + 1;
				this.lowerLeftGuard = count;
				count = this.count;
				this.count = count + 1;
				this.lowerRightGuard = count;
			}
		}

		// Token: 0x0200088B RID: 2187
		// (Invoke) Token: 0x06004091 RID: 16529
		public delegate void SetupDataEvent(EventTime eventTime);

		// Token: 0x0200088C RID: 2188
		// (Invoke) Token: 0x06004095 RID: 16533
		public delegate void HealthChangeEvent(float health, float maxHealth);

		// Token: 0x0200088D RID: 2189
		// (Invoke) Token: 0x06004099 RID: 16537
		public delegate void HealEvent(float heal, Creature healer, EventTime eventTime);

		// Token: 0x0200088E RID: 2190
		// (Invoke) Token: 0x0600409D RID: 16541
		public delegate void ResurrectEvent(float newHealth, Creature resurrector, EventTime eventTime);

		// Token: 0x0200088F RID: 2191
		// (Invoke) Token: 0x060040A1 RID: 16545
		public delegate void DamageEvent(CollisionInstance collisionInstance, EventTime eventTime);

		// Token: 0x02000890 RID: 2192
		// (Invoke) Token: 0x060040A5 RID: 16549
		public delegate void KillEvent(CollisionInstance collisionInstance, EventTime eventTime);
	}
}
