using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using ThunderRoad.Pools;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ThunderRoad
{
	// Token: 0x020002B4 RID: 692
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Item.html")]
	[AddComponentMenu("ThunderRoad/Items/Item")]
	public class Item : ThunderEntity
	{
		// Token: 0x060020A9 RID: 8361 RVA: 0x000DFB11 File Offset: 0x000DDD11
		public List<ValueDropdownItem<string>> GetAllItemID()
		{
			return Catalog.GetDropdownAllID(Category.Item, "None");
		}

		// Token: 0x1700020D RID: 525
		// (get) Token: 0x060020AA RID: 8362 RVA: 0x000DFB1E File Offset: 0x000DDD1E
		[Tooltip("Determines the owner of the item. This is generally set at runtime to work with the shop and buying items.")]
		public Item.Owner owner
		{
			get
			{
				return this._owner;
			}
		}

		// Token: 0x1700020E RID: 526
		// (get) Token: 0x060020AB RID: 8363 RVA: 0x000DFB28 File Offset: 0x000DDD28
		public string OwnerString
		{
			get
			{
				Item.Owner owner = this.owner;
				string localizedString;
				switch (owner)
				{
				case Item.Owner.None:
					localizedString = LocalizationManager.Instance.GetLocalizedString("Default", "UnownedItem", false);
					break;
				case Item.Owner.Player:
					localizedString = LocalizationManager.Instance.GetLocalizedString("Default", "OwnedItem", false);
					break;
				case Item.Owner.Shopkeeper:
					localizedString = LocalizationManager.Instance.GetLocalizedString("Default", "ShopItem", false);
					break;
				default:
					throw new SwitchExpressionException(owner);
				}
				return localizedString;
			}
		}

		// Token: 0x060020AC RID: 8364 RVA: 0x000DFBA8 File Offset: 0x000DDDA8
		public void SetHolderPointToCenterOfMass()
		{
			this.holderPoint.transform.position = base.transform.TransformPoint(this.physicBody ? this.physicBody.centerOfMass : base.GetComponent<Rigidbody>().centerOfMass);
		}

		// Token: 0x1700020F RID: 527
		// (get) Token: 0x060020AD RID: 8365 RVA: 0x000DFBF5 File Offset: 0x000DDDF5
		public bool storeBlocked
		{
			get
			{
				return this.isNotStorableModifiers.Count > 0;
			}
		}

		// Token: 0x060020AE RID: 8366 RVA: 0x000DFC05 File Offset: 0x000DDE05
		public bool HasFlag(ItemFlags flag)
		{
			return this.data.HasFlag(flag);
		}

		// Token: 0x060020AF RID: 8367 RVA: 0x000DFC14 File Offset: 0x000DDE14
		public bool HasCustomData<T>() where T : ContentCustomData
		{
			if (this.contentCustomData == null)
			{
				return false;
			}
			int count = this.contentCustomData.Count;
			for (int i = 0; i < count; i++)
			{
				ContentCustomData customData = this.contentCustomData[i];
				if (customData != null && customData is T)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060020B0 RID: 8368 RVA: 0x000DFC60 File Offset: 0x000DDE60
		public bool TryGetCustomData<T>(out T customData) where T : ContentCustomData
		{
			if (this.contentCustomData == null)
			{
				customData = default(T);
				return false;
			}
			int count = this.contentCustomData.Count;
			for (int index = 0; index < count; index++)
			{
				ContentCustomData contentCustomData = this.contentCustomData[index];
				if (contentCustomData != null && contentCustomData is T)
				{
					customData = (contentCustomData as T);
					return true;
				}
			}
			customData = default(T);
			return false;
		}

		// Token: 0x060020B1 RID: 8369 RVA: 0x000DFCC9 File Offset: 0x000DDEC9
		public void AddCustomData<T>(T customData) where T : ContentCustomData
		{
			if (!this.HasCustomData<T>())
			{
				if (this.contentCustomData == null)
				{
					this.contentCustomData = new List<ContentCustomData>();
				}
				this.contentCustomData.Add(customData);
			}
		}

		/// <summary>
		/// Replace current content custom data with the given list.
		/// </summary>
		/// <param name="newContentCustomData">Content list to replace contentCustomData with.</param>
		// Token: 0x060020B2 RID: 8370 RVA: 0x000DFCF7 File Offset: 0x000DDEF7
		public void OverrideCustomData(List<ContentCustomData> newContentCustomData)
		{
			this.contentCustomData = newContentCustomData;
			Item.OverrideContentCustomDataEvent onOverrideContentCustomDataEvent = this.OnOverrideContentCustomDataEvent;
			if (onOverrideContentCustomDataEvent == null)
			{
				return;
			}
			onOverrideContentCustomDataEvent(this.contentCustomData);
		}

		/// <summary>
		/// Removes all occurrences of the ContentCustomData with the type T
		/// </summary>
		/// <typeparam name="T">Type of the ContentCustomData to remove</typeparam>
		// Token: 0x060020B3 RID: 8371 RVA: 0x000DFD18 File Offset: 0x000DDF18
		public void RemoveCustomData<T>() where T : ContentCustomData
		{
			if (this.contentCustomData == null)
			{
				return;
			}
			for (int i = this.contentCustomData.Count - 1; i >= 0; i--)
			{
				ContentCustomData customData = this.contentCustomData[i];
				if (customData is T)
				{
					this.contentCustomData.Remove(customData);
				}
			}
		}

		// Token: 0x060020B4 RID: 8372 RVA: 0x000DFD68 File Offset: 0x000DDF68
		public void AddNonStorableModifier(object handler)
		{
			if (!this.isNotStorableModifiers.Contains(handler))
			{
				this.isNotStorableModifiers.Add(handler);
			}
		}

		// Token: 0x060020B5 RID: 8373 RVA: 0x000DFD85 File Offset: 0x000DDF85
		public void RemoveNonStorableModifier(object handler)
		{
			if (this.isNotStorableModifiers.Contains(handler))
			{
				this.isNotStorableModifiers.Remove(handler);
			}
		}

		// Token: 0x060020B6 RID: 8374 RVA: 0x000DFDA2 File Offset: 0x000DDFA2
		public void AddNonStorableModifierInvokable(UnityEngine.Object handler)
		{
			this.AddNonStorableModifier(handler);
		}

		// Token: 0x060020B7 RID: 8375 RVA: 0x000DFDAB File Offset: 0x000DDFAB
		public void RemoveNonStorableModifierInvokable(UnityEngine.Object handler)
		{
			this.RemoveNonStorableModifier(handler);
		}

		// Token: 0x060020B8 RID: 8376 RVA: 0x000DFDB4 File Offset: 0x000DDFB4
		public void ClearNonStorableModifiers()
		{
			this.isNotStorableModifiers.Clear();
		}

		// Token: 0x060020B9 RID: 8377 RVA: 0x000DFDC1 File Offset: 0x000DDFC1
		protected virtual void OnValidate()
		{
			if (Application.isPlaying)
			{
				return;
			}
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			this.SetupDefaultComponents();
		}

		// Token: 0x060020BA RID: 8378 RVA: 0x000DFDE0 File Offset: 0x000DDFE0
		public void SetupDefaultComponents()
		{
			if (this.physicBody == null && base.GetComponent<Rigidbody>() == null && base.GetComponent<ArticulationBody>() == null)
			{
				Debug.LogWarning("[DefaultComponents] Adding Rigidbody to " + this.itemId);
				base.gameObject.AddComponent<Rigidbody>();
			}
			if (!this.holderPoint)
			{
				this.holderPoint = base.transform.Find("HolderPoint");
			}
			if (!this.holderPoint)
			{
				Debug.LogWarning("[DefaultComponents] Adding HolderPoint to " + this.itemId);
				this.holderPoint = new GameObject("HolderPoint").transform;
				this.holderPoint.SetParent(base.transform, false);
			}
			if (!this.parryPoint)
			{
				this.parryPoint = base.transform.Find("ParryPoint");
			}
			if (!this.parryPoint)
			{
				Debug.LogWarning("[DefaultComponents] Adding ParryPoint to " + this.itemId);
				this.parryPoint = new GameObject("ParryPoint").transform;
				this.parryPoint.SetParent(base.transform, false);
			}
			if (!this.spawnPoint)
			{
				this.spawnPoint = base.transform.Find("SpawnPoint");
			}
			if (!this.spawnPoint)
			{
				Debug.LogWarning("[DefaultComponents] Adding SpawnPoint to " + this.itemId);
				this.spawnPoint = new GameObject("SpawnPoint").transform;
				this.spawnPoint.SetParent(base.transform, false);
			}
			this.preview = base.GetComponentInChildren<Preview>();
			if (!this.preview && base.transform.Find("Preview"))
			{
				this.preview = base.transform.Find("Preview").gameObject.AddComponent<Preview>();
			}
			if (!this.preview)
			{
				Debug.LogWarning("[DefaultComponents] Adding Preview to " + this.itemId);
				this.preview = new GameObject("Preview").AddComponent<Preview>();
				this.preview.transform.SetParent(base.transform, false);
			}
			Transform whoosh = base.transform.Find("Whoosh");
			if (whoosh && !whoosh.GetComponent<WhooshPoint>())
			{
				Debug.LogWarning("[DefaultComponents] Adding WhooshPoint to " + this.itemId);
				whoosh.gameObject.AddComponent<WhooshPoint>();
			}
			if (!this.mainHandleRight)
			{
				Debug.LogWarning("[DefaultComponents] Adding Handle Right to " + this.itemId);
				foreach (Handle handle in base.GetComponentsInChildren<Handle>())
				{
					if (handle.IsAllowed(Side.Right))
					{
						this.mainHandleRight = handle;
						break;
					}
				}
			}
			if (!this.mainHandleLeft)
			{
				Debug.LogWarning("[DefaultComponents] Adding Handle Right to " + this.itemId);
				foreach (Handle handle2 in base.GetComponentsInChildren<Handle>())
				{
					if (handle2.IsAllowed(Side.Left))
					{
						this.mainHandleLeft = handle2;
						break;
					}
				}
			}
			if (!this.mainHandleRight)
			{
				this.mainHandleRight = base.GetComponentInChildren<Handle>();
			}
			this.physicBody = base.gameObject.GetPhysicBody();
			if (this.useCustomCenterOfMass)
			{
				this.physicBody.centerOfMass = this.customCenterOfMass;
			}
			else
			{
				this.physicBody.ResetCenterOfMass();
			}
			if (this.customInertiaTensor)
			{
				if (this.customInertiaTensorCollider == null)
				{
					this.customInertiaTensorCollider = new GameObject("InertiaTensorCollider").AddComponent<CapsuleCollider>();
					this.customInertiaTensorCollider.transform.SetParent(base.transform, false);
					this.customInertiaTensorCollider.radius = 0.05f;
					this.customInertiaTensorCollider.direction = 2;
				}
				this.customInertiaTensorCollider.enabled = false;
				this.customInertiaTensorCollider.isTrigger = true;
				this.customInertiaTensorCollider.gameObject.layer = 2;
			}
			if (!base.TryGetComponent<LightVolumeReceiver>(out this.lightVolumeReceiver))
			{
				this.lightVolumeReceiver = base.gameObject.AddComponent<LightVolumeReceiver>();
			}
			if (!this.audioSource)
			{
				this.audioSource = base.gameObject.AddComponent<AudioSource>();
			}
		}

		// Token: 0x060020BB RID: 8379 RVA: 0x000E021C File Offset: 0x000DE41C
		public Bounds GetWorldBounds()
		{
			Bounds bounds = new Bounds(base.transform.position, Vector3.zero);
			for (int i = 0; i < this.renderers.Count; i++)
			{
				bounds.Encapsulate(this.renderers[i].bounds);
			}
			return bounds;
		}

		// Token: 0x060020BC RID: 8380 RVA: 0x000E0270 File Offset: 0x000DE470
		public Bounds GetLocalBounds()
		{
			Bounds b = new Bounds(Vector3.zero, Vector3.zero);
			this.<GetLocalBounds>g__RecurseEncapsulate|89_0(base.transform, ref b);
			return b;
		}

		// Token: 0x060020BD RID: 8381 RVA: 0x000E02A0 File Offset: 0x000DE4A0
		public Vector3 GetLocalCenter()
		{
			ItemData itemData = this.data;
			bool flag = ((itemData != null) ? itemData.preferredItemCenter : CenterType.Mass) != CenterType.Mass;
			if (!flag)
			{
				PhysicBody physicBody = this.physicBody;
				if (physicBody != null && physicBody.IsSleeping())
				{
					this.physicBody.WakeUp();
				}
			}
			switch (flag ? 1 : 0)
			{
			case 0:
				if (this.physicBody != null)
				{
					return this.physicBody.centerOfMass;
				}
				break;
			case 1:
				return Vector3.zero;
			case 2:
				return this.GetLocalBounds().center;
			case 3:
				if ((this.mainHandleLeft ?? this.mainHandleRight) != null)
				{
					return base.transform.InverseTransformPoint((this.mainHandleLeft ?? this.mainHandleRight).transform.position);
				}
				break;
			case 4:
				if (this.holderPoint != null)
				{
					return base.transform.InverseTransformPoint(this.holderPoint.transform.position);
				}
				break;
			}
			return Vector3.zero;
		}

		// Token: 0x060020BE RID: 8382 RVA: 0x000E03B0 File Offset: 0x000DE5B0
		public void Haptic(float intensity, bool oneFrameCooldown = false)
		{
			int handlersCount = this.handlers.Count;
			for (int i = 0; i < handlersCount; i++)
			{
				PlayerHand playerHand = this.handlers[i].playerHand;
				if (playerHand != null)
				{
					playerHand.controlHand.HapticShort(intensity, oneFrameCooldown);
				}
			}
		}

		// Token: 0x060020BF RID: 8383 RVA: 0x000E03F8 File Offset: 0x000DE5F8
		public void HapticClip(PcmData pcmData, GameData.HapticClip fallbackClip)
		{
			int handlersCount = this.handlers.Count;
			for (int i = 0; i < handlersCount; i++)
			{
				PlayerHand playerHand = this.handlers[i].playerHand;
				if (playerHand != null)
				{
					playerHand.controlHand.Haptic(pcmData, fallbackClip, false);
				}
			}
		}

		// Token: 0x060020C0 RID: 8384 RVA: 0x000E0441 File Offset: 0x000DE641
		public bool TryGetCustomReference<T>(string name, out T custom) where T : Component
		{
			custom = this.GetCustomReference<T>(name, false);
			return custom != null;
		}

		// Token: 0x060020C1 RID: 8385 RVA: 0x000E0464 File Offset: 0x000DE664
		public T GetCustomReference<T>(string name, bool printError = true) where T : Component
		{
			CustomReference customReference = this.customReferences.Find((CustomReference cr) => cr.name == name);
			if (customReference == null)
			{
				if (printError)
				{
					Debug.LogError("[" + this.itemId + "] Cannot find item custom reference " + name);
				}
				return default(T);
			}
			if (customReference.transform is T)
			{
				return (T)((object)customReference.transform);
			}
			if (typeof(T) == typeof(Transform))
			{
				return customReference.transform.transform as T;
			}
			return customReference.transform.GetComponent<T>();
		}

		// Token: 0x060020C2 RID: 8386 RVA: 0x000E051B File Offset: 0x000DE71B
		public Transform GetCustomReference(string name, bool printError = true)
		{
			return this.GetCustomReference<Transform>(name, printError);
		}

		// Token: 0x060020C3 RID: 8387 RVA: 0x000E0528 File Offset: 0x000DE728
		protected virtual void Awake()
		{
			Item.all.Add(this);
			this.damageMultiplier = new FloatHandler();
			this.sliceAngleMultiplier = new FloatHandler();
			this.pushLevelMultiplier = new IntAddHandler();
			Transform root = base.transform;
			Breakable breakable;
			if (base.TryGetComponent<Breakable>(out breakable))
			{
				root = breakable.unbrokenObjectsHolder.transform;
			}
			this.clothingGenderSwitcher = root.GetComponentInChildren<ClothingGenderSwitcher>();
			ClothingGenderSwitcher clothingGenderSwitcher = this.clothingGenderSwitcher;
			if (clothingGenderSwitcher != null)
			{
				clothingGenderSwitcher.SetModelActive(true);
			}
			root.GetComponentsInChildren<Renderer>(this.renderers);
			int renderersCount = this.renderers.Count;
			for (int i = 0; i < renderersCount; i++)
			{
				Renderer renderer = this.renderers[i];
				if (!renderer.enabled || (!(renderer is SkinnedMeshRenderer) && !(renderer is MeshRenderer)))
				{
					this.renderers.RemoveAtIgnoreOrder(i);
					i--;
					renderersCount--;
				}
			}
			root.GetComponentsInChildren<FxController>(this.fxControllers);
			int fxControllersCount = this.fxControllers.Count;
			for (int j = 0; j < fxControllersCount; j++)
			{
				FxController fxController = this.fxControllers[j];
				if (!fxController.gameObject.activeInHierarchy || !fxController.enabled)
				{
					this.fxControllers.RemoveAtIgnoreOrder(j);
					j--;
					fxControllersCount--;
				}
			}
			root.GetComponentsInChildren<FxModule>(this.fxModules);
			int fxModulesCount = this.fxModules.Count;
			for (int k = 0; k < fxModulesCount; k++)
			{
				FxModule fxModule = this.fxModules[k];
				if (!fxModule.gameObject.activeInHierarchy || !fxModule.enabled || fxModule.GetComponentInParent<FxController>() != null)
				{
					this.fxModules.RemoveAtIgnoreOrder(k);
					k--;
					fxModulesCount--;
				}
			}
			this.waterHandler = new WaterHandler(true, false);
			this.waterHandler.OnWaterEnter += this.OnWaterEnter;
			this.waterHandler.OnWaterExit += this.OnWaterExit;
			if (!base.TryGetComponent<LightVolumeReceiver>(out this.lightVolumeReceiver))
			{
				this.lightVolumeReceiver = base.gameObject.AddComponent<LightVolumeReceiver>();
			}
			this.lightVolumeReceiver.initRenderersOnStart = false;
			this.lightVolumeReceiver.volumeDetection = LightVolumeReceiver.VolumeDetection.DynamicTrigger;
			this.lightVolumeReceiver.SetRenderers(this.renderers, true);
			root.GetComponentsInChildren<RevealDecal>(this.revealDecals);
			root.GetComponentsInChildren<WhooshPoint>(this.whooshPoints);
			root.GetComponentsInChildren<HingeEffect>(this.effectHinges);
			root.GetComponentsInChildren<ParryTarget>(this.parryTargets);
			root.GetComponentsInChildren<Handle>(this.handles);
			root.GetComponentsInChildren<Holder>(true, this.childHolders);
			this.SetPhysicBodyAndMainCollisionHandler();
			if (this.customInertiaTensorCollider)
			{
				this.CalculateCustomInertiaTensor();
			}
			this.imbues = new List<Imbue>();
			if (!this.audioSource)
			{
				this.audioSource = base.gameObject.AddComponent<AudioSource>();
			}
			this.audioSource.spatialBlend = 1f;
			this.audioSource.dopplerLevel = 0f;
			this.audioSource.playOnAwake = false;
			this.audioSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);
			if (AudioSettings.GetSpatializerPluginName() != null)
			{
				this.audioSource.spatialize = true;
			}
			this.orgSleepThreshold = this.physicBody.sleepThreshold;
			this.handlers = new List<RagdollHand>();
			if (this.worldAttached)
			{
				Item.allWorldAttached.Add(this);
			}
			ClothingGenderSwitcher clothingGenderSwitcher2 = this.clothingGenderSwitcher;
			if (clothingGenderSwitcher2 == null)
			{
				return;
			}
			clothingGenderSwitcher2.Refresh();
		}

		// Token: 0x060020C4 RID: 8388 RVA: 0x000E0880 File Offset: 0x000DEA80
		public void SetPhysicBodyAndMainCollisionHandler()
		{
			if (this.physicBody != null)
			{
				return;
			}
			this.physicBody = base.gameObject.GetPhysicBody();
			if (this.useCustomCenterOfMass)
			{
				this.physicBody.centerOfMass = this.customCenterOfMass;
			}
			Transform transform = base.transform;
			this.metalColliderGroups = new List<ColliderGroup>();
			transform.GetComponentsInChildren<ColliderGroup>(this.colliderGroups);
			transform.GetComponentsInChildren<CollisionHandler>(this.collisionHandlers);
			if (this.collisionHandlers.Count == 0)
			{
				CollisionHandler collisionHandler = base.gameObject.AddComponent<CollisionHandler>();
				this.collisionHandlers.Add(collisionHandler);
				int colliderGroupsCount = this.colliderGroups.Count;
				for (int i = 0; i < colliderGroupsCount; i++)
				{
					ColliderGroup colliderGroup = this.colliderGroups[i];
					if (colliderGroup.isMetal)
					{
						this.hasMetal = true;
					}
					this.metalColliderGroups.Add(colliderGroup);
					colliderGroup.collisionHandler = collisionHandler;
				}
			}
			if (!GameManager.local)
			{
				return;
			}
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int j = 0; j < collisionHandlersCount; j++)
			{
				CollisionHandler collisionHandler2 = this.collisionHandlers[j];
				collisionHandler2.OnCollisionStartEvent += this.OnCollisionStartEvent;
				if (!collisionHandler2.physicBody)
				{
					collisionHandler2.physicBody = collisionHandler2.gameObject.GetPhysicBody();
				}
				if (collisionHandler2.physicBody == this.physicBody)
				{
					this.mainCollisionHandler = collisionHandler2;
					return;
				}
			}
		}

		// Token: 0x060020C5 RID: 8389 RVA: 0x000E09E8 File Offset: 0x000DEBE8
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			Item.allActive.Add(this);
			if (this.holder == null && AreaManager.Instance != null && AreaManager.Instance.CurrentArea != null)
			{
				this.cullingDetectionEnabled = true;
				this.cullingDetectionCycleTime = Time.time;
				this.CheckCurrentArea();
			}
		}

		// Token: 0x060020C6 RID: 8390 RVA: 0x000E0A45 File Offset: 0x000DEC45
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			Item.allActive.Remove(this);
			this.cullingDetectionEnabled = false;
			this.waterHandler.Reset();
			this.ClearZones();
		}

		// Token: 0x060020C7 RID: 8391 RVA: 0x000E0A71 File Offset: 0x000DEC71
		private void OnWaterEnter()
		{
		}

		// Token: 0x060020C8 RID: 8392 RVA: 0x000E0A74 File Offset: 0x000DEC74
		private void UpdateWater()
		{
			if (this.loaded && Water.exist && !this.physicBody.isKinematic && (this.isMoving || this.isFlying || this.isThrowed || this.IsHanded() || this.physicBody.HasMeaningfulVelocity()))
			{
				float radius = this.data.waterSampleMinRadius;
				Vector3 lowerPosition = Vector3.zero;
				Vector3 higherPosition = Vector3.zero;
				int parryTargetsCount = this.parryTargets.Count;
				Vector3 velocity;
				if (parryTargetsCount > 0)
				{
					lowerPosition.y = float.PositiveInfinity;
					for (int i = 0; i < parryTargetsCount; i++)
					{
						ParryTarget parryTarget = this.parryTargets[i];
						Transform transform = parryTarget.transform;
						Vector3 parryTargetPos = transform.position;
						Vector3 parryUp = transform.up * parryTarget.length;
						Vector3 positiveMaxPosition = parryTargetPos + parryUp;
						Vector3 negativeMaxPosition = parryTargetPos - parryUp;
						if (negativeMaxPosition.y < lowerPosition.y)
						{
							lowerPosition = negativeMaxPosition;
							higherPosition = positiveMaxPosition;
						}
						if (positiveMaxPosition.y < lowerPosition.y)
						{
							lowerPosition = positiveMaxPosition;
							higherPosition = negativeMaxPosition;
						}
					}
					Vector3 velocityAtPoint = this.collisionHandlers[0].CalculateLastPointVelocity(lowerPosition);
					velocity = velocityAtPoint;
					Vector3 velocityNormalized = velocityAtPoint.normalized;
					radius = Vector3.Distance(Vector3.ProjectOnPlane(lowerPosition, velocityNormalized), Vector3.ProjectOnPlane(higherPosition, velocityNormalized));
					radius = Mathf.Max(radius * 0.25f, this.data.waterSampleMinRadius);
				}
				else
				{
					Vector3 holderPointPos = this.holderPoint.position;
					lowerPosition.x = holderPointPos.x;
					lowerPosition.y = holderPointPos.y + radius;
					lowerPosition.z = holderPointPos.z;
					higherPosition.x = holderPointPos.x;
					higherPosition.y = holderPointPos.y - radius;
					higherPosition.z = holderPointPos.z;
					velocity = this.physicBody.velocity;
				}
				this.waterHandler.Update(lowerPosition, lowerPosition.y, higherPosition.y, radius, velocity);
				if (this.waterHandler.inWater)
				{
					float dragMultiplier = this.data.waterDragMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
					this.SetPhysicModifier(this, new float?(Mathf.Lerp(1f, Catalog.gameData.water.minGravityItem, this.waterHandler.submergedRatio)), 1f, dragMultiplier, dragMultiplier * 0.1f, -1f, null);
					int handlersCount = this.handlers.Count;
					for (int j = 0; j < handlersCount; j++)
					{
						RagdollHand ragdollHand = this.handlers[j];
						if (ragdollHand.creature.isPlayer)
						{
							float handPositionSpringMultiplier = this.data.waterHandSpringMultiplierCurve.Evaluate(this.waterHandler.submergedRatio);
							ragdollHand.grabbedHandle.SetJointModifier(this, handPositionSpringMultiplier, 1f, handPositionSpringMultiplier, 1f);
						}
					}
				}
			}
			if (this.waterHandler.inWater)
			{
				int colliderGroupsCount = this.colliderGroups.Count;
				float waterImbueDepletionRate = Catalog.gameData.water.imbueDepletionRate * Time.deltaTime;
				for (int k = 0; k < colliderGroupsCount; k++)
				{
					ColliderGroup colliderGroup = this.colliderGroups[k];
					if (colliderGroup.transform.position.y < this.waterHandler.waterSurfacePosition.y)
					{
						Imbue imbue = colliderGroup.imbue;
						if (imbue && imbue.spellCastBase != null && imbue.energy > 0f)
						{
							imbue.ConsumeInstant(waterImbueDepletionRate * colliderGroup.data.GetModifier(colliderGroup).waterLossRateMultiplier, false);
						}
					}
				}
			}
			if (this.waterHandler.inWater && (!Water.exist || !this.loaded))
			{
				this.waterHandler.Reset();
			}
		}

		// Token: 0x060020C9 RID: 8393 RVA: 0x000E0E3C File Offset: 0x000DF03C
		private void OnWaterExit()
		{
			int handlersCount = this.handlers.Count;
			for (int i = 0; i < handlersCount; i++)
			{
				RagdollHand ragdollHand = this.handlers[i];
				if (ragdollHand.creature.isPlayer)
				{
					ragdollHand.grabbedHandle.RemoveJointModifier(this);
				}
			}
			this.RemovePhysicModifier(this);
		}

		// Token: 0x060020CA RID: 8394 RVA: 0x000E0E8E File Offset: 0x000DF08E
		public void SwapWith(string itemID)
		{
			this.SwapWith(itemID, true, null);
		}

		// Token: 0x060020CB RID: 8395 RVA: 0x000E0E9C File Offset: 0x000DF09C
		public void ForceUngrabAll()
		{
			for (int i = this.handlers.Count - 1; i >= 0; i--)
			{
				this.handlers[i].UnGrab(false);
			}
		}

		// Token: 0x060020CC RID: 8396 RVA: 0x000E0ED4 File Offset: 0x000DF0D4
		[return: TupleElementNames(new string[]
		{
			"valueType",
			"value"
		})]
		public ValueTuple<string, float> GetValue(bool skipOverride = false)
		{
			ContentCustomDataValueOverride valueOverride;
			if (!skipOverride && this.TryGetCustomData<ContentCustomDataValueOverride>(out valueOverride))
			{
				return new ValueTuple<string, float>(valueOverride.valueType.IsNullOrEmptyOrWhitespace() ? this.data.valueType : valueOverride.valueType, valueOverride.value);
			}
			return new ValueTuple<string, float>(this.data.valueType, this.data.value);
		}

		// Token: 0x060020CD RID: 8397 RVA: 0x000E0F38 File Offset: 0x000DF138
		public void SetValue(float value)
		{
			ContentCustomDataValueOverride priceOverride;
			if (this.TryGetCustomData<ContentCustomDataValueOverride>(out priceOverride))
			{
				priceOverride.value = value;
				return;
			}
			priceOverride = new ContentCustomDataValueOverride(value);
			this.AddCustomData<ContentCustomDataValueOverride>(priceOverride);
		}

		// Token: 0x060020CE RID: 8398 RVA: 0x000E0F68 File Offset: 0x000DF168
		public void SetOwner(Item.Owner owner)
		{
			if (this._owner != owner && this.data.allowedStorage > (ItemData.Storage)0)
			{
				Item.Owner previousOwner = this._owner;
				this._owner = owner;
				Action<Item.Owner, Item.Owner> action = this.onOwnerChange;
				if (action != null)
				{
					action(previousOwner, this._owner);
				}
				Action<Item, Item.Owner, Item.Owner> action2 = Item.onAnyOwnerChange;
				if (action2 == null)
				{
					return;
				}
				action2(this, previousOwner, this._owner);
			}
		}

		// Token: 0x060020CF RID: 8399 RVA: 0x000E0FC9 File Offset: 0x000DF1C9
		public void ClearValueOverride()
		{
			this.RemoveCustomData<ContentCustomDataValueOverride>();
		}

		// Token: 0x060020D0 RID: 8400 RVA: 0x000E0FD4 File Offset: 0x000DF1D4
		public void SwapWith(string itemID, bool transferCustomData, Action<Item, Item> callback)
		{
			if (this.isSwaping)
			{
				return;
			}
			ItemData itemData = Catalog.GetData<ItemData>(itemID, true);
			if (itemData != null)
			{
				this.isSwaping = true;
				itemData.SpawnAsync(delegate(Item newItem)
				{
					Item.<>c__DisplayClass109_1 CS$<>8__locals2 = new Item.<>c__DisplayClass109_1();
					CS$<>8__locals2.newItem = newItem;
					this.isSwaping = false;
					if (CS$<>8__locals2.newItem == null)
					{
						Debug.LogWarning("New item is null, can't swap.");
						return;
					}
					CS$<>8__locals2.newItem.transform.position = this.transform.position;
					CS$<>8__locals2.newItem.transform.rotation = this.transform.rotation;
					if (itemData.allowedStorage > (ItemData.Storage)0)
					{
						CS$<>8__locals2.newItem.SetOwner(this.owner);
					}
					if (this.holder)
					{
						Holder holder = this.holder;
						holder.UnSnap(this, false);
						holder.Snap(CS$<>8__locals2.newItem, false);
					}
					else if (this.handlers.Count > 0 && CS$<>8__locals2.newItem.handles.Count > 0)
					{
						for (int i = this.handlers.Count - 1; i >= 0; i--)
						{
							RagdollHand ragdollHand = this.handlers[i];
							float orgAxisPosition = ragdollHand.gripInfo.axisPosition;
							Handle newHandle = CS$<>8__locals2.newItem.handles.ElementAtOrDefault(this.handles.IndexOf(ragdollHand.grabbedHandle));
							if (newHandle == null || !newHandle.IsAllowed(ragdollHand.side))
							{
								Debug.LogWarning("Item swap could not find same handle index on new item");
								newHandle = CS$<>8__locals2.newItem.GetMainHandle(ragdollHand.side);
							}
							HandlePose newHandlePose = newHandle.orientations.ElementAtOrDefault(ragdollHand.grabbedHandle.orientations.IndexOf(ragdollHand.gripInfo.orientation));
							if (newHandlePose == null || newHandlePose.side != ragdollHand.side)
							{
								Debug.LogWarning("Item swap could not find same hand pose index on new item");
								newHandlePose = newHandle.GetDefaultOrientation(ragdollHand.side);
							}
							ragdollHand.TryRelease();
							ragdollHand.Grab(newHandle, newHandlePose, orgAxisPosition, true, false);
						}
					}
					CS$<>8__locals2.piercedDamagers = new List<Damager>();
					for (int j = 0; j < this.collisionHandlers.Count; j++)
					{
						if (this.collisionHandlers[j].penetratedObjects.Count > 0)
						{
							this.collisionHandlers[j].RemoveAllPenetratedObjects(out CS$<>8__locals2.piercedDamagers);
						}
					}
					if (CS$<>8__locals2.piercedDamagers.Count > 0)
					{
						for (int k = 0; k < CS$<>8__locals2.piercedDamagers.Count; k++)
						{
							Item item = CS$<>8__locals2.piercedDamagers[k].collisionHandler.item;
							PhysicBody pb = (item != null) ? item.physicBody : null;
							if (pb != null)
							{
								CS$<>8__locals2.newItem.gameObject.AddComponent<FixedJoint>().SetConnectedPhysicBody(pb);
							}
						}
						CS$<>8__locals2.newItem.StartCoroutine(CS$<>8__locals2.<SwapWith>g__TryPierceAgain|1());
					}
					Action<Item, Item> callback2 = callback;
					if (callback2 != null)
					{
						callback2(this, CS$<>8__locals2.newItem);
					}
					this.Despawn();
				}, null, null, null, this.isPooled, transferCustomData ? this.contentCustomData : null, Item.Owner.None);
			}
		}

		// Token: 0x17000210 RID: 528
		// (get) Token: 0x060020D1 RID: 8401 RVA: 0x000E1057 File Offset: 0x000DF257
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update | ManagedLoops.LateUpdate;
			}
		}

		// Token: 0x060020D2 RID: 8402 RVA: 0x000E105C File Offset: 0x000DF25C
		protected internal override void ManagedLateUpdate()
		{
			if (!this.cullingDetectionEnabled)
			{
				return;
			}
			if (this.holder != null)
			{
				return;
			}
			if (AreaManager.Instance == null || AreaManager.Instance.CurrentArea == null)
			{
				return;
			}
			if (this.currentArea == null)
			{
				this.CheckCurrentArea();
			}
			if (this.currentArea != null && !this.isRegistered && this.currentArea.IsSpawned)
			{
				this.currentArea.SpawnedArea.RegisterItem(this);
				this.isRegistered = true;
			}
			if (Time.time - this.cullingDetectionCycleTime < this.cullingDetectionCycleSpeed)
			{
				return;
			}
			this.cullingDetectionCycleTime = Time.time;
			this.CheckCurrentArea();
		}

		// Token: 0x060020D3 RID: 8403 RVA: 0x000E1104 File Offset: 0x000DF304
		public void CheckCurrentArea()
		{
			if (this.holder != null)
			{
				return;
			}
			if (AreaManager.Instance == null || AreaManager.Instance.CurrentArea == null)
			{
				return;
			}
			if (this.currentArea == null)
			{
				SpawnableArea areaFound = AreaManager.Instance.CurrentArea.FindRecursive(base.transform.position);
				if (areaFound != null)
				{
					this.currentArea = areaFound;
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.RegisterItem(this);
						this.isRegistered = true;
						return;
					}
					this.isRegistered = false;
					return;
				}
			}
			else
			{
				SpawnableArea areaFound2 = this.currentArea.FindRecursive(base.transform.position);
				if (areaFound2 == null)
				{
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.UnRegisterItem(this);
					}
					this.isRegistered = false;
					this.currentArea = areaFound2;
					return;
				}
				if (this.currentArea != areaFound2)
				{
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.UnRegisterItem(this);
					}
					this.isRegistered = false;
					this.currentArea = areaFound2;
					if (this.currentArea.IsSpawned)
					{
						this.currentArea.SpawnedArea.RegisterItem(this);
						this.isRegistered = true;
					}
				}
			}
		}

		// Token: 0x060020D4 RID: 8404 RVA: 0x000E1239 File Offset: 0x000DF439
		public void UnRegisterArea()
		{
			if (this.currentArea == null)
			{
				return;
			}
			if (!this.currentArea.IsSpawned)
			{
				return;
			}
			this.currentArea.SpawnedArea.UnRegisterItem(this);
			this.isRegistered = false;
		}

		// Token: 0x060020D5 RID: 8405 RVA: 0x000E126C File Offset: 0x000DF46C
		public void SetCull(bool cull, bool checkChange = true)
		{
			if (checkChange && this.isCulled == cull)
			{
				return;
			}
			this.isCulled = cull;
			if (!this.loaded)
			{
				return;
			}
			if (!Level.master || !this.IsHanded())
			{
				base.gameObject.SetActive(!cull);
			}
			if (this.OnCullEvent != null)
			{
				this.OnCullEvent(this.isCulled);
			}
		}

		// Token: 0x060020D6 RID: 8406 RVA: 0x000E12D4 File Offset: 0x000DF4D4
		[ContextMenu("Convert to spawner")]
		public void CreateItemSpawnerFromItem()
		{
			if (base.GetComponentInChildren<RopeSimple>() != null)
			{
				return;
			}
			GameObject itemGameObject = base.gameObject;
			GameObject newItemSpawnerRoot = UnityEngine.Object.Instantiate<GameObject>(new GameObject("itemSpawnerRoot"), this.holderPoint.position, this.holderPoint.rotation, itemGameObject.transform.parent);
			ItemSpawner itemSpawner = newItemSpawnerRoot.AddComponent<ItemSpawner>();
			itemSpawner.name = this.itemId + "_Spawner";
			itemSpawner.itemId = this.itemId;
			itemSpawner.spawnOnStart = false;
			try
			{
				UnityEngine.Object.DestroyImmediate(itemGameObject);
			}
			catch (Exception)
			{
				UnityEngine.Object.DestroyImmediate(newItemSpawnerRoot);
			}
		}

		// Token: 0x060020D7 RID: 8407 RVA: 0x000E1378 File Offset: 0x000DF578
		protected override void Start()
		{
			base.Start();
			this.CheckDestroyedMeshRenderers();
			if (GameManager.local && base.transform.root != base.transform && !this.holder && !this.keepParent)
			{
				base.transform.SetParent(null, true);
			}
			if (!GameManager.local)
			{
				this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem), false);
				return;
			}
			ItemData itemData = this.data;
			if (itemData == null && !string.IsNullOrEmpty(this.itemId) && this.itemId != "None")
			{
				if (Catalog.TryGetData<ItemData>(this.itemId, out itemData, true))
				{
					this.Load(itemData);
				}
				else
				{
					Debug.LogError("Unable to load itemData " + this.itemId + " for item " + base.name);
				}
			}
			if (this.data != null && !GameManager.CheckContentActive(this.data.sensitiveContent, this.data.sensitiveFilterBehaviour))
			{
				this.Despawn();
			}
			this.CacheSpawnTransformation();
		}

		/// <summary>
		/// Cache the position and rotation of the item (implicitly during spawn)
		/// Does the same for child skinned meshes
		/// </summary>
		// Token: 0x060020D8 RID: 8408 RVA: 0x000E1484 File Offset: 0x000DF684
		public void CacheSpawnTransformation()
		{
			Transform t = base.transform;
			this.spawnPosition = t.position;
			this.spawnRotation = t.rotation;
			foreach (SkinnedMeshRenderer sr in base.GetComponentsInChildren<SkinnedMeshRenderer>())
			{
				if (this.spawnSkinnedBonesTransforms == null)
				{
					this.spawnSkinnedBonesTransforms = new Dictionary<Transform, ValueTuple<Vector3, Quaternion>>();
				}
				for (int i = 0; i < sr.bones.Length; i++)
				{
					Transform b = sr.bones[i];
					if (b)
					{
						if (this.spawnSkinnedBonesTransforms.ContainsKey(b))
						{
							this.spawnSkinnedBonesTransforms[b] = new ValueTuple<Vector3, Quaternion>(b.position, b.rotation);
						}
						else
						{
							this.spawnSkinnedBonesTransforms.Add(b, new ValueTuple<Vector3, Quaternion>(b.position, b.rotation));
						}
					}
				}
			}
		}

		// Token: 0x060020D9 RID: 8409 RVA: 0x000E1560 File Offset: 0x000DF760
		public void CheckDestroyedMeshRenderers()
		{
			bool rendererRemoved = false;
			for (int i = this.renderers.Count - 1; i >= 0; i--)
			{
				if (!this.renderers[i])
				{
					this.renderers.RemoveAt(i);
					rendererRemoved = true;
				}
			}
			if (rendererRemoved)
			{
				this.lightVolumeReceiver.SetRenderers(this.renderers, true);
			}
		}

		// Token: 0x060020DA RID: 8410 RVA: 0x000E15C0 File Offset: 0x000DF7C0
		public void Hide(bool hide)
		{
			this.isHidden = hide;
			if (this.renderers != null)
			{
				int renderersCount = this.renderers.Count;
				for (int i = 0; i < renderersCount; i++)
				{
					Renderer renderer = this.renderers[i];
					if (renderer != null)
					{
						renderer.enabled = !hide;
					}
				}
			}
			if (this.fxControllers != null)
			{
				int fxControllersCount = this.fxControllers.Count;
				for (int j = 0; j < fxControllersCount; j++)
				{
					FxController fxController = this.fxControllers[j];
					if (fxController != null)
					{
						fxController.enabled = !hide;
					}
				}
			}
			if (this.fxModules != null)
			{
				int fxModulesCount = this.fxModules.Count;
				for (int k = 0; k < fxModulesCount; k++)
				{
					FxModule fxModule = this.fxModules[k];
					if (fxModule != null)
					{
						fxModule.enabled = !hide;
					}
				}
			}
			if (this.childHolders != null)
			{
				int childHoldersCount = this.childHolders.Count;
				for (int l = 0; l < childHoldersCount; l++)
				{
					Holder holder = this.childHolders[l];
					if (holder != null && holder.items != null)
					{
						int itemsCount = holder.items.Count;
						for (int m = 0; m < itemsCount; m++)
						{
							Item item = holder.items[m];
							if (item != null)
							{
								item.Hide(hide);
							}
						}
					}
				}
			}
		}

		// Token: 0x060020DB RID: 8411 RVA: 0x000E1734 File Offset: 0x000DF934
		public void SetCustomInertiaTensor()
		{
			if (this.customInertiaTensorPos == Vector3.zero)
			{
				this.CalculateCustomInertiaTensor();
			}
			if (!this.physicBody)
			{
				this.physicBody = base.gameObject.GetPhysicBody();
			}
			this.physicBody.inertiaTensor = this.customInertiaTensorPos;
			this.physicBody.inertiaTensorRotation = this.customInertiaTensorRot;
		}

		// Token: 0x060020DC RID: 8412 RVA: 0x000E1799 File Offset: 0x000DF999
		public virtual void ResetInertiaTensor()
		{
			if (!this.physicBody)
			{
				this.physicBody = base.gameObject.GetPhysicBody();
			}
			this.physicBody.ResetInertiaTensor();
		}

		// Token: 0x060020DD RID: 8413 RVA: 0x000E17C4 File Offset: 0x000DF9C4
		public void CalculateCustomInertiaTensor()
		{
			if (!this.physicBody)
			{
				this.physicBody = base.gameObject.GetPhysicBody();
			}
			if (!this.customInertiaTensorCollider)
			{
				Debug.LogWarning("Cannot calculate custom inertia tensor because no custom collider has been set on " + this.itemId);
				this.physicBody.ResetInertiaTensor();
				return;
			}
			List<Collider> orgColliders = new List<Collider>();
			foreach (Collider collider in this.physicBody.gameObject.GetComponentsInChildren<Collider>())
			{
				if (!collider.isTrigger && !(this.customInertiaTensorCollider == collider))
				{
					collider.enabled = false;
					orgColliders.Add(collider);
				}
			}
			this.customInertiaTensorCollider.enabled = true;
			this.customInertiaTensorCollider.isTrigger = false;
			this.physicBody.ResetInertiaTensor();
			this.customInertiaTensorPos = this.physicBody.inertiaTensor;
			this.customInertiaTensorRot = this.physicBody.inertiaTensorRotation;
			this.customInertiaTensorCollider.isTrigger = true;
			this.customInertiaTensorCollider.enabled = false;
			int orgCollidersCount = orgColliders.Count;
			for (int j = 0; j < orgCollidersCount; j++)
			{
				orgColliders[j].enabled = true;
			}
		}

		// Token: 0x140000F9 RID: 249
		// (add) Token: 0x060020DE RID: 8414 RVA: 0x000E18F4 File Offset: 0x000DFAF4
		// (remove) Token: 0x060020DF RID: 8415 RVA: 0x000E192C File Offset: 0x000DFB2C
		public event Item.CullEvent OnCullEvent;

		// Token: 0x140000FA RID: 250
		// (add) Token: 0x060020E0 RID: 8416 RVA: 0x000E1964 File Offset: 0x000DFB64
		// (remove) Token: 0x060020E1 RID: 8417 RVA: 0x000E199C File Offset: 0x000DFB9C
		public event Item.SpawnEvent OnSpawnEvent;

		// Token: 0x140000FB RID: 251
		// (add) Token: 0x060020E2 RID: 8418 RVA: 0x000E19D4 File Offset: 0x000DFBD4
		// (remove) Token: 0x060020E3 RID: 8419 RVA: 0x000E1A0C File Offset: 0x000DFC0C
		public event Item.SpawnEvent OnDespawnEvent;

		// Token: 0x060020E4 RID: 8420 RVA: 0x000E1A44 File Offset: 0x000DFC44
		private static void InvokeOnItemSpawn(Item item)
		{
			if (Item.OnItemSpawn == null)
			{
				return;
			}
			Delegate[] invocationList = Item.OnItemSpawn.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Action<Item> eventDelegate = invocationList[i] as Action<Item>;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(item);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnItemSpawn: {0}", e));
					}
				}
			}
		}

		// Token: 0x060020E5 RID: 8421 RVA: 0x000E1AAC File Offset: 0x000DFCAC
		private static void InvokeOnItemDespawn(Item item)
		{
			if (Item.OnItemDespawn == null)
			{
				return;
			}
			Delegate[] invocationList = Item.OnItemDespawn.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Action<Item> eventDelegate = invocationList[i] as Action<Item>;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(item);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnItemDespawn: {0}", e));
					}
				}
			}
		}

		// Token: 0x060020E6 RID: 8422 RVA: 0x000E1B14 File Offset: 0x000DFD14
		private static void InvokeOnItemSnap(Item item, Holder holder)
		{
			if (Item.OnItemSnap == null)
			{
				return;
			}
			Delegate[] invocationList = Item.OnItemSnap.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Action<Item, Holder> eventDelegate = invocationList[i] as Action<Item, Holder>;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(item, holder);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnItemSnap: {0}", e));
					}
				}
			}
		}

		// Token: 0x060020E7 RID: 8423 RVA: 0x000E1B7C File Offset: 0x000DFD7C
		private static void InvokeOnItemUnSnap(Item item, Holder holder)
		{
			if (Item.OnItemUnSnap == null)
			{
				return;
			}
			Delegate[] invocationList = Item.OnItemUnSnap.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Action<Item, Holder> eventDelegate = invocationList[i] as Action<Item, Holder>;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(item, holder);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnItemUnSnap: {0}", e));
					}
				}
			}
		}

		// Token: 0x060020E8 RID: 8424 RVA: 0x000E1BE4 File Offset: 0x000DFDE4
		private static void InvokeOnItemGrab(Item item, Handle handle, RagdollHand ragdollHand)
		{
			if (Item.OnItemGrab == null)
			{
				return;
			}
			Delegate[] invocationList = Item.OnItemGrab.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Action<Item, Handle, RagdollHand> eventDelegate = invocationList[i] as Action<Item, Handle, RagdollHand>;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(item, handle, ragdollHand);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnItemGrab: {0}", e));
					}
				}
			}
		}

		// Token: 0x060020E9 RID: 8425 RVA: 0x000E1C50 File Offset: 0x000DFE50
		private static void InvokeOnItemUngrab(Item item, Handle handle, RagdollHand ragdollHand)
		{
			if (Item.OnItemUngrab == null)
			{
				return;
			}
			Delegate[] invocationList = Item.OnItemUngrab.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Action<Item, Handle, RagdollHand> eventDelegate = invocationList[i] as Action<Item, Handle, RagdollHand>;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(item, handle, ragdollHand);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during OnItemUngrab: {0}", e));
					}
				}
			}
		}

		// Token: 0x140000FC RID: 252
		// (add) Token: 0x060020EA RID: 8426 RVA: 0x000E1CBC File Offset: 0x000DFEBC
		// (remove) Token: 0x060020EB RID: 8427 RVA: 0x000E1CF4 File Offset: 0x000DFEF4
		public event Item.ImbuesChangeEvent OnImbuesChangeEvent;

		// Token: 0x140000FD RID: 253
		// (add) Token: 0x060020EC RID: 8428 RVA: 0x000E1D2C File Offset: 0x000DFF2C
		// (remove) Token: 0x060020ED RID: 8429 RVA: 0x000E1D64 File Offset: 0x000DFF64
		public event Item.ContainerEvent OnContainerAddEvent;

		// Token: 0x140000FE RID: 254
		// (add) Token: 0x060020EE RID: 8430 RVA: 0x000E1D9C File Offset: 0x000DFF9C
		// (remove) Token: 0x060020EF RID: 8431 RVA: 0x000E1DD4 File Offset: 0x000DFFD4
		public event Item.ZoneEvent OnZoneEvent;

		// Token: 0x060020F0 RID: 8432 RVA: 0x000E1E09 File Offset: 0x000E0009
		public void InvokeOnItemStored(UIInventory inventory, ItemContent itemContent)
		{
			UIInventory.ItemDelegate onItemStored = this.OnItemStored;
			if (onItemStored == null)
			{
				return;
			}
			onItemStored(inventory, itemContent, this);
		}

		// Token: 0x060020F1 RID: 8433 RVA: 0x000E1E1E File Offset: 0x000E001E
		public void InvokeOnItemRetrieved(UIInventory inventory, ItemContent itemContent)
		{
			UIInventory.ItemDelegate onItemRetrieved = this.OnItemRetrieved;
			if (onItemRetrieved == null)
			{
				return;
			}
			onItemRetrieved(inventory, itemContent, this);
		}

		// Token: 0x140000FF RID: 255
		// (add) Token: 0x060020F2 RID: 8434 RVA: 0x000E1E34 File Offset: 0x000E0034
		// (remove) Token: 0x060020F3 RID: 8435 RVA: 0x000E1E6C File Offset: 0x000E006C
		public event Item.DamageReceivedDelegate OnDamageReceivedEvent;

		// Token: 0x14000100 RID: 256
		// (add) Token: 0x060020F4 RID: 8436 RVA: 0x000E1EA4 File Offset: 0x000E00A4
		// (remove) Token: 0x060020F5 RID: 8437 RVA: 0x000E1EDC File Offset: 0x000E00DC
		public event Item.GrabDelegate OnGrabEvent;

		// Token: 0x14000101 RID: 257
		// (add) Token: 0x060020F6 RID: 8438 RVA: 0x000E1F14 File Offset: 0x000E0114
		// (remove) Token: 0x060020F7 RID: 8439 RVA: 0x000E1F4C File Offset: 0x000E014C
		public event Item.ReleaseDelegate OnHandleReleaseEvent;

		// Token: 0x14000102 RID: 258
		// (add) Token: 0x060020F8 RID: 8440 RVA: 0x000E1F84 File Offset: 0x000E0184
		// (remove) Token: 0x060020F9 RID: 8441 RVA: 0x000E1FBC File Offset: 0x000E01BC
		public event Item.ReleaseDelegate OnUngrabEvent;

		// Token: 0x14000103 RID: 259
		// (add) Token: 0x060020FA RID: 8442 RVA: 0x000E1FF4 File Offset: 0x000E01F4
		// (remove) Token: 0x060020FB RID: 8443 RVA: 0x000E202C File Offset: 0x000E022C
		public event Item.HolderDelegate OnSnapEvent;

		// Token: 0x14000104 RID: 260
		// (add) Token: 0x060020FC RID: 8444 RVA: 0x000E2064 File Offset: 0x000E0264
		// (remove) Token: 0x060020FD RID: 8445 RVA: 0x000E209C File Offset: 0x000E029C
		public event Item.HolderDelegate OnUnSnapEvent;

		// Token: 0x14000105 RID: 261
		// (add) Token: 0x060020FE RID: 8446 RVA: 0x000E20D4 File Offset: 0x000E02D4
		// (remove) Token: 0x060020FF RID: 8447 RVA: 0x000E210C File Offset: 0x000E030C
		public event Item.ThrowingDelegate OnThrowEvent;

		// Token: 0x14000106 RID: 262
		// (add) Token: 0x06002100 RID: 8448 RVA: 0x000E2144 File Offset: 0x000E0344
		// (remove) Token: 0x06002101 RID: 8449 RVA: 0x000E217C File Offset: 0x000E037C
		public event Item.ThrowingDelegate OnFlyStartEvent;

		// Token: 0x14000107 RID: 263
		// (add) Token: 0x06002102 RID: 8450 RVA: 0x000E21B4 File Offset: 0x000E03B4
		// (remove) Token: 0x06002103 RID: 8451 RVA: 0x000E21EC File Offset: 0x000E03EC
		public event Item.ThrowingDelegate OnFlyEndEvent;

		// Token: 0x14000108 RID: 264
		// (add) Token: 0x06002104 RID: 8452 RVA: 0x000E2224 File Offset: 0x000E0424
		// (remove) Token: 0x06002105 RID: 8453 RVA: 0x000E225C File Offset: 0x000E045C
		public event Item.TelekinesisDelegate OnTelekinesisGrabEvent;

		// Token: 0x14000109 RID: 265
		// (add) Token: 0x06002106 RID: 8454 RVA: 0x000E2294 File Offset: 0x000E0494
		// (remove) Token: 0x06002107 RID: 8455 RVA: 0x000E22CC File Offset: 0x000E04CC
		public event Item.TelekinesisReleaseDelegate OnTelekinesisReleaseEvent;

		// Token: 0x1400010A RID: 266
		// (add) Token: 0x06002108 RID: 8456 RVA: 0x000E2304 File Offset: 0x000E0504
		// (remove) Token: 0x06002109 RID: 8457 RVA: 0x000E233C File Offset: 0x000E053C
		public event Item.TelekinesisTemporalDelegate OnTelekinesisRepelEvent;

		// Token: 0x1400010B RID: 267
		// (add) Token: 0x0600210A RID: 8458 RVA: 0x000E2374 File Offset: 0x000E0574
		// (remove) Token: 0x0600210B RID: 8459 RVA: 0x000E23AC File Offset: 0x000E05AC
		public event Item.TelekinesisTemporalDelegate OnTelekinesisPullEvent;

		// Token: 0x1400010C RID: 268
		// (add) Token: 0x0600210C RID: 8460 RVA: 0x000E23E4 File Offset: 0x000E05E4
		// (remove) Token: 0x0600210D RID: 8461 RVA: 0x000E241C File Offset: 0x000E061C
		public event Item.TelekinesisSpinEvent OnTKSpinStart;

		// Token: 0x1400010D RID: 269
		// (add) Token: 0x0600210E RID: 8462 RVA: 0x000E2454 File Offset: 0x000E0654
		// (remove) Token: 0x0600210F RID: 8463 RVA: 0x000E248C File Offset: 0x000E068C
		public event Item.TelekinesisSpinEvent OnTKSpinEnd;

		// Token: 0x1400010E RID: 270
		// (add) Token: 0x06002110 RID: 8464 RVA: 0x000E24C4 File Offset: 0x000E06C4
		// (remove) Token: 0x06002111 RID: 8465 RVA: 0x000E24FC File Offset: 0x000E06FC
		public event Item.TouchActionDelegate OnTouchActionEvent;

		// Token: 0x1400010F RID: 271
		// (add) Token: 0x06002112 RID: 8466 RVA: 0x000E2534 File Offset: 0x000E0734
		// (remove) Token: 0x06002113 RID: 8467 RVA: 0x000E256C File Offset: 0x000E076C
		public event Item.HeldActionDelegate OnHeldActionEvent;

		// Token: 0x14000110 RID: 272
		// (add) Token: 0x06002114 RID: 8468 RVA: 0x000E25A4 File Offset: 0x000E07A4
		// (remove) Token: 0x06002115 RID: 8469 RVA: 0x000E25DC File Offset: 0x000E07DC
		public event Item.MagnetDelegate OnMagnetCatchEvent;

		// Token: 0x14000111 RID: 273
		// (add) Token: 0x06002116 RID: 8470 RVA: 0x000E2614 File Offset: 0x000E0814
		// (remove) Token: 0x06002117 RID: 8471 RVA: 0x000E264C File Offset: 0x000E084C
		public event Item.MagnetDelegate OnMagnetReleaseEvent;

		// Token: 0x14000112 RID: 274
		// (add) Token: 0x06002118 RID: 8472 RVA: 0x000E2684 File Offset: 0x000E0884
		// (remove) Token: 0x06002119 RID: 8473 RVA: 0x000E26BC File Offset: 0x000E08BC
		public event Item.BreakStartDelegate OnBreakStart;

		// Token: 0x14000113 RID: 275
		// (add) Token: 0x0600211A RID: 8474 RVA: 0x000E26F4 File Offset: 0x000E08F4
		// (remove) Token: 0x0600211B RID: 8475 RVA: 0x000E272C File Offset: 0x000E092C
		public event Item.LoadDelegate OnDataLoaded;

		// Token: 0x0600211C RID: 8476 RVA: 0x000E2764 File Offset: 0x000E0964
		public void InvokeOnDataLoaded()
		{
			if (this.OnDataLoaded == null)
			{
				return;
			}
			Delegate[] invocationList = this.OnDataLoaded.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Item.LoadDelegate eventDelegate = invocationList[i] as Item.LoadDelegate;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate();
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during onGameLoad event: {0}", e));
					}
				}
			}
		}

		// Token: 0x14000114 RID: 276
		// (add) Token: 0x0600211D RID: 8477 RVA: 0x000E27CC File Offset: 0x000E09CC
		// (remove) Token: 0x0600211E RID: 8478 RVA: 0x000E2804 File Offset: 0x000E0A04
		public event Item.OverrideContentCustomDataEvent OnOverrideContentCustomDataEvent;

		// Token: 0x14000115 RID: 277
		// (add) Token: 0x0600211F RID: 8479 RVA: 0x000E283C File Offset: 0x000E0A3C
		// (remove) Token: 0x06002120 RID: 8480 RVA: 0x000E2874 File Offset: 0x000E0A74
		public event Item.IgnoreRagdollCollisionEvent OnIgnoreRagdollCollision;

		// Token: 0x14000116 RID: 278
		// (add) Token: 0x06002121 RID: 8481 RVA: 0x000E28AC File Offset: 0x000E0AAC
		// (remove) Token: 0x06002122 RID: 8482 RVA: 0x000E28E4 File Offset: 0x000E0AE4
		public event Item.IgnoreItemCollisionEvent OnIgnoreItemCollision;

		// Token: 0x14000117 RID: 279
		// (add) Token: 0x06002123 RID: 8483 RVA: 0x000E291C File Offset: 0x000E0B1C
		// (remove) Token: 0x06002124 RID: 8484 RVA: 0x000E2954 File Offset: 0x000E0B54
		public event Item.IgnoreColliderCollisionEvent OnIgnoreColliderCollision;

		// Token: 0x14000118 RID: 280
		// (add) Token: 0x06002125 RID: 8485 RVA: 0x000E298C File Offset: 0x000E0B8C
		// (remove) Token: 0x06002126 RID: 8486 RVA: 0x000E29C4 File Offset: 0x000E0BC4
		public event Item.SetCollidersEvent OnSetCollidersEvent;

		// Token: 0x14000119 RID: 281
		// (add) Token: 0x06002127 RID: 8487 RVA: 0x000E29FC File Offset: 0x000E0BFC
		// (remove) Token: 0x06002128 RID: 8488 RVA: 0x000E2A34 File Offset: 0x000E0C34
		public event Item.SetColliderLayerEvent OnSetColliderLayerEvent;

		// Token: 0x17000211 RID: 529
		// (get) Token: 0x06002129 RID: 8489 RVA: 0x000E2A6C File Offset: 0x000E0C6C
		public LayerName forcedLayer
		{
			get
			{
				if (this.forcedItemLayer != LayerName.None)
				{
					return this.forcedItemLayer;
				}
				ItemData itemData = this.data;
				LayerName? layerName;
				if (itemData != null && itemData.diffForceLayerWhenHeld)
				{
					List<RagdollHand> list = this.handlers;
					if (((list != null) ? list.Count : 0) > 0)
					{
						ItemData itemData2 = this.data;
						layerName = ((itemData2 != null) ? new LayerName?(itemData2.forceLayerHeld) : null);
						goto IL_78;
					}
				}
				ItemData itemData3 = this.data;
				layerName = ((itemData3 != null) ? new LayerName?(itemData3.forceLayer) : null);
				IL_78:
				LayerName? layerName2 = layerName;
				return layerName2.GetValueOrDefault();
			}
		}

		// Token: 0x0600212A RID: 8490 RVA: 0x000E2AF9 File Offset: 0x000E0CF9
		public Handle GetMainHandle(Side side)
		{
			if (side != Side.Right)
			{
				return this.mainHandleLeft;
			}
			return this.mainHandleRight;
		}

		// Token: 0x0600212B RID: 8491 RVA: 0x000E2B0B File Offset: 0x000E0D0B
		public void ForceLayer(LayerName layer)
		{
			this.forcedItemLayer = layer;
			this.SetColliderAndMeshLayer(GameManager.GetLayer(layer), false);
		}

		// Token: 0x0600212C RID: 8492 RVA: 0x000E2B21 File Offset: 0x000E0D21
		public virtual bool TryGetData(out ItemData data, bool printError = true)
		{
			data = this.data;
			bool flag = this.data != null;
			if (!flag && printError)
			{
				Debug.LogError("Something tried to access the item data of item [ " + base.name + " ], but the item data is null! An error occurred and prevented the item data from properly loading.");
			}
			return flag;
		}

		// Token: 0x0600212D RID: 8493 RVA: 0x000E2B58 File Offset: 0x000E0D58
		public virtual void Load(ItemData itemData)
		{
			if (this.loaded)
			{
				return;
			}
			if (this.data == null)
			{
				this.LoadData(itemData);
			}
			this.LoadInteractable(itemData);
			this.LoadModules();
			this.RefreshTotalItemMass();
			this.RefreshItemHasSlash();
			this.loaded = true;
			this.SetCull(this.isCulled, false);
			this.InvokeOnDataLoaded();
		}

		// Token: 0x0600212E RID: 8494 RVA: 0x000E2BB0 File Offset: 0x000E0DB0
		public void LoadData(ItemData itemData)
		{
			this.itemId = itemData.id;
			this.data = (itemData.Clone() as ItemData);
			base.Load(this.data);
			if (this.customInertiaTensorCollider)
			{
				this.SetCustomInertiaTensor();
			}
			if (itemData.overrideMassAndDrag)
			{
				if (this.mainCollisionHandler)
				{
					this.mainCollisionHandler.SetPhysicBody(itemData.mass, itemData.drag, itemData.angularDrag);
				}
				else
				{
					Debug.LogErrorFormat(this, "Item " + base.name + " have no mainCollisionHandler!", Array.Empty<object>());
				}
			}
			this.flyRotationSpeed = itemData.flyRotationSpeed;
			this.flyThrowAngle = itemData.flyThrowAngle;
			this.distantGrabSafeDistance = itemData.telekinesisSafeDistance;
			this.distantGrabSpinEnabled = itemData.HasFlag(ItemFlags.Spinnable);
			this.distantGrabThrowRatio = itemData.telekinesisThrowRatio;
			this.customSnaps = itemData.customSnaps;
			this.forcedItemLayer = LayerName.None;
			if (this.audioContainerSnap)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioContainerSnap);
			}
			if (itemData.snapAudioContainerLocation != null)
			{
				Catalog.LoadAssetAsync<AudioContainer>(itemData.snapAudioContainerLocation, delegate(AudioContainer value)
				{
					this.audioContainerSnap = value;
					Action<AudioContainer> onSnapAudioLoaded = this.OnSnapAudioLoaded;
					if (onSnapAudioLoaded == null)
					{
						return;
					}
					onSnapAudioLoaded(this.audioContainerSnap);
				}, itemData.id);
			}
			if (this.audioContainerInventory)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioContainerInventory);
			}
			if (itemData.inventoryAudioContainerLocation != null)
			{
				Catalog.LoadAssetAsync<AudioContainer>(itemData.inventoryAudioContainerLocation, delegate(AudioContainer value)
				{
					this.audioContainerInventory = value;
					if (!(this.audioContainerInventory != null))
					{
						Debug.LogWarning("Inventory audio container is null for item " + itemData.id);
						return;
					}
					Action<AudioContainer> onInventoryAudioLoaded = this.OnInventoryAudioLoaded;
					if (onInventoryAudioLoaded == null)
					{
						return;
					}
					onInventoryAudioLoaded(this.audioContainerInventory);
				}, itemData.id);
			}
			int effectHingesCount = itemData.effectHinges.Count;
			for (int i = 0; i < effectHingesCount; i++)
			{
				ItemData.EffectHinge effectHingeData = itemData.effectHinges[i];
				int hingesCount = this.effectHinges.Count;
				for (int j = 0; j < hingesCount; j++)
				{
					HingeEffect effectHinge = this.effectHinges[j];
					if (effectHinge.name == effectHingeData.transformName)
					{
						effectHinge.Load(effectHingeData.effectId, effectHingeData.minTorque, effectHingeData.maxTorque);
					}
				}
			}
			int whooshsCount = itemData.whooshs.Count;
			for (int k = 0; k < whooshsCount; k++)
			{
				ItemData.Whoosh itemWhoosh = itemData.whooshs[k];
				int whooshPointsCount = this.whooshPoints.Count;
				for (int l = 0; l < whooshPointsCount; l++)
				{
					WhooshPoint whooshPoint = this.whooshPoints[l];
					if (whooshPoint.name == itemWhoosh.transformName)
					{
						EffectData effectData = Catalog.GetData<EffectData>(itemWhoosh.effectId, true);
						if (effectData != null)
						{
							whooshPoint.Load(effectData, itemWhoosh);
						}
					}
				}
			}
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int m = 0; m < colliderGroupsCount; m++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[m];
				int groupsCount = itemData.colliderGroups.Count;
				for (int n = 0; n < groupsCount; n++)
				{
					ItemData.ColliderGroup itemColliderGroup = itemData.colliderGroups[n];
					if (colliderGroup.name == itemColliderGroup.transformName)
					{
						if (itemColliderGroup.colliderGroupData != null)
						{
							colliderGroup.Load(itemColliderGroup.colliderGroupData);
						}
						else
						{
							Debug.LogWarning("ColliderGroupData is null for " + itemColliderGroup.transformName);
						}
					}
				}
			}
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int i2 = 0; i2 < collisionHandlersCount; i2++)
			{
				CollisionHandler collisionHandler = this.collisionHandlers[i2];
				int damagersCount = collisionHandler.damagers.Count;
				for (int j2 = 0; j2 < damagersCount; j2++)
				{
					Damager damager = collisionHandler.damagers[j2];
					bool found = false;
					int count = itemData.damagers.Count;
					for (int k2 = 0; k2 < count; k2++)
					{
						ItemData.Damager itemDamager = itemData.damagers[k2];
						if (damager.name == itemDamager.transformName)
						{
							found = true;
							damager.Load(itemDamager.damagerData, collisionHandler);
						}
					}
					if (!found)
					{
						Debug.LogWarning(string.Concat(new string[]
						{
							"Damager '",
							damager.name,
							"' on item ",
							this.data.id,
							" did not load any DamagerData!"
						}), this);
					}
				}
				collisionHandler.SortDamagers();
			}
			int handlersCount = this.collisionHandlers.Count;
			for (int i3 = 0; i3 < handlersCount; i3++)
			{
				CollisionHandler collisionHandler2 = this.collisionHandlers[i3];
				bool collisionsNull = collisionHandler2.collisions == null;
				if (!collisionsNull)
				{
					collisionHandler2.StopCollisions();
				}
				if (this.data.collisionMaxOverride > 0)
				{
					if (collisionsNull || collisionHandler2.collisions.Length != this.data.collisionMaxOverride)
					{
						collisionHandler2.SetMaxCollision(this.data.collisionMaxOverride);
					}
				}
				else if (collisionsNull || collisionHandler2.collisions.Length != Catalog.gameData.maxObjectCollision)
				{
					collisionHandler2.SetMaxCollision(Catalog.gameData.maxObjectCollision);
				}
				collisionHandler2.checkMinVelocity = !this.data.collisionNoMinVelocityCheck;
				collisionHandler2.enterOnly = this.data.collisionEnterOnly;
			}
			this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem), false);
		}

		// Token: 0x0600212F RID: 8495 RVA: 0x000E3154 File Offset: 0x000E1354
		public void LoadInteractable(ItemData itemData)
		{
			Interactable[] children;
			if (this.breakable)
			{
				children = this.breakable.unbrokenObjectsHolder.GetComponentsInChildren<Interactable>();
			}
			else
			{
				children = base.GetComponentsInChildren<Interactable>();
			}
			foreach (Interactable interactable in children)
			{
				int interactablesCount = itemData.Interactables.Count;
				for (int j = 0; j < interactablesCount; j++)
				{
					ItemData.Interactable itemInteractable = itemData.Interactables[j];
					if (!(interactable.name != itemInteractable.transformName))
					{
						interactable.interactableId = itemInteractable.interactableId;
						InteractableData interactableData;
						if (Catalog.TryGetData<InteractableData>(itemInteractable.interactableId, out interactableData, true))
						{
							InteractableData clonedData = interactableData.Clone() as InteractableData;
							interactable.Load(clonedData);
							break;
						}
						Debug.LogWarning(string.Concat(new string[]
						{
							"Interactable '",
							interactable.name,
							"' on item ",
							this.data.id,
							" did not load any InteractableData with interactableId: ",
							itemInteractable.interactableId,
							"!"
						}));
					}
				}
				if (interactable.data == null)
				{
					interactable.TryLoadFromID();
				}
				if (interactable.data == null)
				{
					Debug.LogWarning(string.Concat(new string[]
					{
						"Interactable '",
						interactable.name,
						"' on item ",
						this.data.id,
						" did not load any InteractableData. It may not have a matching interactable in the itemData or no ID in the prefab"
					}), this);
				}
			}
		}

		// Token: 0x06002130 RID: 8496 RVA: 0x000E32C8 File Offset: 0x000E14C8
		public void LoadModules()
		{
			if (this.loadedItemModules)
			{
				return;
			}
			if (this.data.modules != null)
			{
				int modulesCount = this.data.modules.Count;
				for (int i = 0; i < modulesCount; i++)
				{
					this.data.modules[i].OnItemLoaded(this);
				}
				this.loadedItemModules = true;
			}
		}

		// Token: 0x06002131 RID: 8497 RVA: 0x000E3328 File Offset: 0x000E1528
		protected internal override void ManagedUpdate()
		{
			this.CheckIfItemIsMoving();
			if (this.isMoving || this.isFlying)
			{
				bool hadNPCHandler = this.lastHandler && !this.lastHandler.creature.isPlayer;
				bool handlerTargetIsNoPhysic = false;
				if (hadNPCHandler)
				{
					handlerTargetIsNoPhysic = (this.lastHandler.creature.brain.currentTarget && !this.lastHandler.creature.brain.currentTarget.isPlayer && !this.lastHandler.creature.brain.currentTarget.ragdoll.IsPhysicsEnabled(false));
				}
				bool closeToPlayer = false;
				if (Player.currentCreature && hadNPCHandler && handlerTargetIsNoPhysic)
				{
					float num = Vector3.Distance(Player.currentCreature.transform.position, base.transform.position);
					Creature creature = Creature.allActive[0];
					float? num2;
					if (creature == null)
					{
						num2 = null;
					}
					else
					{
						Ragdoll ragdoll = creature.ragdoll;
						num2 = ((ragdoll != null) ? new float?(ragdoll.physicTogglePlayerRadius) : null);
					}
					closeToPlayer = (num < (num2 ?? 5f));
				}
				if (hadNPCHandler && handlerTargetIsNoPhysic && !closeToPlayer)
				{
					this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingObjectOnly), false);
				}
				else
				{
					this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
				}
				if (this.isFlying && this.flyDirRef)
				{
					Vector3 forwardDir = this.isFlyingBackwards ? (-this.flyDirRef.forward) : this.flyDirRef.forward;
					float num3 = Vector3.SignedAngle(Vector3.ProjectOnPlane(this.physicBody.velocity.normalized, this.flyDirRef.right), forwardDir, this.flyDirRef.right);
					float horizontalAngle = Vector3.SignedAngle(Vector3.ProjectOnPlane(this.physicBody.velocity.normalized, this.flyDirRef.up), forwardDir, this.flyDirRef.up);
					Vector3 localFlyUp = base.transform.InverseTransformDirection(this.flyDirRef.up);
					Vector3 localFlyRight = base.transform.InverseTransformDirection(this.flyDirRef.right);
					Vector3 targetRotationEuler = -num3 * localFlyRight + -horizontalAngle * localFlyUp;
					base.transform.Rotate(Vector3.Slerp(Vector3.zero, targetRotationEuler, this.flyRotationSpeed * this.physicBody.velocity.magnitude * Time.deltaTime), Space.Self);
				}
			}
			this.UpdateWater();
			this.UpdateLastSpeeds();
			if ((this.lastHandler != null && this.physicBody.IsSleeping()) || (this.waterHandler.inWater && !this.physicBody.HasMeaningfulVelocity()))
			{
				this.StopThrowing();
				this.StopFlying();
			}
			int whooshPointsCount = this.whooshPoints.Count;
			for (int i = 0; i < whooshPointsCount; i++)
			{
				this.whooshPoints[i].UpdateWhooshPoint();
			}
			this.wasMoving = this.isMoving;
		}

		/// <summary>
		/// Check if the item is currently moving by looking at its rigidbody velocity, and if moving is not ignored.
		/// </summary>
		// Token: 0x06002132 RID: 8498 RVA: 0x000E363C File Offset: 0x000E183C
		private void CheckIfItemIsMoving()
		{
			this.isMoving = (!this.ignoreIsMoving && !this.isThrowed && (this.isFlying || (!this.physicBody.IsSleeping() && this.physicBody.HasMeaningfulVelocity())));
			if (this.isMoving)
			{
				if (!this.wasMoving)
				{
					Item.allMoving.Add(this);
					return;
				}
			}
			else if (this.wasMoving)
			{
				Item.allMoving.Remove(this);
			}
		}

		// Token: 0x06002133 RID: 8499 RVA: 0x000E36BC File Offset: 0x000E18BC
		public void RefreshTotalItemMass()
		{
			this.totalCombinedMass = this.physicBody.mass;
			Breakable breakable;
			base.TryGetComponent<Breakable>(out breakable);
			foreach (PhysicBody pb in base.gameObject.GetPhysicBodiesInChildren(true))
			{
				if (pb.gameObject.activeInHierarchy && !pb.isKinematic && !(pb == this.physicBody) && (!breakable || !breakable.allSubBodies.Contains(pb)))
				{
					this.totalCombinedMass += pb.mass;
				}
			}
		}

		// Token: 0x06002134 RID: 8500 RVA: 0x000E3750 File Offset: 0x000E1950
		public void RefreshItemHasSlash()
		{
			this.hasSlash = false;
			foreach (CollisionHandler collisionHandler in this.collisionHandlers)
			{
				using (List<Damager>.Enumerator enumerator2 = collisionHandler.damagers.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.type == Damager.Type.Slash)
						{
							this.hasSlash = true;
							break;
						}
					}
				}
				if (this.hasSlash)
				{
					break;
				}
			}
		}

		// Token: 0x06002135 RID: 8501 RVA: 0x000E37F8 File Offset: 0x000E19F8
		public virtual void UpdateReveal()
		{
			if (this.updateReveal)
			{
				this.updateReveal = false;
				int revealDecalsCount = this.revealDecals.Count;
				for (int i = 0; i < revealDecalsCount; i++)
				{
					if (this.revealDecals[i] != null)
					{
						this.updateReveal = (this.revealDecals[i].UpdateOvertime() || this.updateReveal);
					}
				}
			}
		}

		// Token: 0x06002136 RID: 8502 RVA: 0x000E3862 File Offset: 0x000E1A62
		protected void OnCollisionStartEvent(CollisionInstance collisionInstance)
		{
			this.StopFlying();
		}

		// Token: 0x06002137 RID: 8503 RVA: 0x000E386A File Offset: 0x000E1A6A
		public void OnDamageReceived(CollisionInstance collisionInstance)
		{
			if (this.OnDamageReceivedEvent != null)
			{
				this.OnDamageReceivedEvent(collisionInstance);
			}
		}

		// Token: 0x06002138 RID: 8504 RVA: 0x000E3880 File Offset: 0x000E1A80
		public bool IsVisible()
		{
			int renderersCount = this.renderers.Count;
			for (int i = 0; i < renderersCount; i++)
			{
				if (this.renderers[i].isVisible)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002139 RID: 8505 RVA: 0x000E38BC File Offset: 0x000E1ABC
		public void SetParryMagic(bool active)
		{
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int i = 0; i < colliderGroupsCount; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				if (colliderGroup.modifier.imbueType != ColliderGroupData.ImbueType.None)
				{
					int collidersCount = colliderGroup.colliders.Count;
					for (int j = 0; j < collidersCount; j++)
					{
						Collider collider = colliderGroup.colliders[j];
						if (active)
						{
							collider.tag = Item.parryMagicTag;
						}
						else
						{
							collider.tag = "Untagged";
						}
					}
				}
			}
			this.parryActive = active;
		}

		// Token: 0x0600213A RID: 8506 RVA: 0x000E394C File Offset: 0x000E1B4C
		public void SetColliders(bool active, bool force = false)
		{
			if (!force && active == this.isCollidersOn)
			{
				return;
			}
			if (active)
			{
				foreach (Collider collider2 in this.disabledColliders)
				{
					collider2.enabled = true;
				}
				this.disabledColliders.Clear();
			}
			else
			{
				foreach (ColliderGroup colliderGroup in this.colliderGroups)
				{
					foreach (Collider collider in colliderGroup.colliders)
					{
						if (!(collider == this.customInertiaTensorCollider) && collider.enabled)
						{
							collider.enabled = false;
							this.disabledColliders.Add(collider);
						}
					}
				}
			}
			this.isCollidersOn = active;
			Item.SetCollidersEvent onSetCollidersEvent = this.OnSetCollidersEvent;
			if (onSetCollidersEvent == null)
			{
				return;
			}
			onSetCollidersEvent(this, active, force);
		}

		// Token: 0x0600213B RID: 8507 RVA: 0x000E3A74 File Offset: 0x000E1C74
		public void ForceMainHandler(RagdollHand ragdollHand)
		{
			this.mainHandler = ragdollHand;
		}

		// Token: 0x17000212 RID: 530
		// (get) Token: 0x0600213C RID: 8508 RVA: 0x000E3A7D File Offset: 0x000E1C7D
		public bool IsFree
		{
			get
			{
				return !this.IsHeld() && this.holder == null && !this.isGripped && !this.isTelekinesisGrabbed;
			}
		}

		// Token: 0x0600213D RID: 8509 RVA: 0x000E3AA8 File Offset: 0x000E1CA8
		public bool IsHeld()
		{
			return this.IsHanded();
		}

		// Token: 0x17000213 RID: 531
		// (get) Token: 0x0600213E RID: 8510 RVA: 0x000E3AB0 File Offset: 0x000E1CB0
		public bool IsHeldByPlayer
		{
			get
			{
				if (!this.isGripped)
				{
					RagdollHand ragdollHand = this.mainHandler;
					bool flag;
					if (ragdollHand == null)
					{
						flag = false;
					}
					else
					{
						Creature creature = ragdollHand.creature;
						bool? flag2 = (creature != null) ? new bool?(creature.isPlayer) : null;
						bool flag3 = true;
						flag = (flag2.GetValueOrDefault() == flag3 & flag2 != null);
					}
					if (!flag)
					{
						return this.isTelekinesisGrabbed;
					}
				}
				return true;
			}
		}

		// Token: 0x0600213F RID: 8511 RVA: 0x000E3B0F File Offset: 0x000E1D0F
		public virtual bool IsHanded()
		{
			return this.handlers.Count > 0;
		}

		// Token: 0x06002140 RID: 8512 RVA: 0x000E3B20 File Offset: 0x000E1D20
		public virtual bool IsHanded(Handle ignoreHandle)
		{
			int handlesCount = this.handles.Count;
			for (int i = 0; i < handlesCount; i++)
			{
				Handle handle = this.handles[i];
				if (handle.IsHanded() && handle != ignoreHandle)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002141 RID: 8513 RVA: 0x000E3B68 File Offset: 0x000E1D68
		public virtual bool IsHanded(Side side)
		{
			int handlersCount = this.handlers.Count;
			for (int i = 0; i < handlersCount; i++)
			{
				if (this.handlers[i].side == side)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002142 RID: 8514 RVA: 0x000E3BA4 File Offset: 0x000E1DA4
		public virtual bool IsTwoHanded(List<Handle> validHandles = null)
		{
			if (validHandles == null)
			{
				validHandles = this.handles;
			}
			int validHandlesCount = validHandles.Count;
			for (int i = 0; i < validHandlesCount; i++)
			{
				Handle handle = validHandles[i];
				int handlersCount = handle.handlers.Count;
				for (int j = 0; j < handlersCount; j++)
				{
					RagdollHand otherHand = handle.handlers[j].otherHand;
					Handle otherHandle = (otherHand != null) ? otherHand.grabbedHandle : null;
					if (otherHandle && otherHandle.item == this)
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002143 RID: 8515 RVA: 0x000E3C30 File Offset: 0x000E1E30
		public virtual void GetPositionRotationRelativeToHand(out Vector3 handLocalPos, out Quaternion handLocalRot, RagdollHand hand, Handle handle = null, HandlePose handlePose = null, float? axisRatio = null)
		{
			if (this.handlers.Contains(hand))
			{
				handLocalPos = hand.transform.InverseTransformPoint(base.transform.position);
				handLocalRot = hand.transform.InverseTransformRotation(base.transform.rotation);
				return;
			}
			if (handle == null)
			{
				handle = this.GetMainHandle(hand.side);
			}
			if (handlePose == null)
			{
				handlePose = handle.GetNearestOrientation(hand.grip, hand.side);
			}
			if (axisRatio == null)
			{
				axisRatio = new float?(handle.GetNearestAxisPosition(hand.transform.position));
			}
			Handle.GripInfo gripInfo = handle.CreateGripPoint(hand, axisRatio.Value, handlePose);
			handLocalPos = gripInfo.transform.InverseTransformPoint(base.transform.position);
			handLocalRot = gripInfo.transform.InverseTransformRotation(base.transform.rotation);
		}

		// Token: 0x06002144 RID: 8516 RVA: 0x000E3D27 File Offset: 0x000E1F27
		public virtual void StopFlying()
		{
			if (this.isFlying)
			{
				this.isFlying = false;
				this.isFlyingBackwards = false;
				this.RefreshCollision(false);
				Item.ThrowingDelegate onFlyEndEvent = this.OnFlyEndEvent;
				if (onFlyEndEvent == null)
				{
					return;
				}
				onFlyEndEvent(this);
			}
		}

		// Token: 0x06002145 RID: 8517 RVA: 0x000E3D58 File Offset: 0x000E1F58
		public virtual void StopThrowing()
		{
			if (this.isThrowed && !this.forceThrown)
			{
				this.isThrowed = false;
				this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.DroppedItem), false);
				this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.dropped;
				if (Item.allThrowed.Contains(this))
				{
					Item.allThrowed.Remove(this);
				}
				this.lastInteractionTime = Time.time;
				this.lastHandler = null;
			}
		}

		// Token: 0x06002146 RID: 8518 RVA: 0x000E3DCE File Offset: 0x000E1FCE
		public void SetCenterOfMass(Vector3 localPosition)
		{
			this.physicBody.centerOfMass = localPosition;
		}

		// Token: 0x06002147 RID: 8519 RVA: 0x000E3DDC File Offset: 0x000E1FDC
		private void UpdateLastSpeeds()
		{
			if (!this.trackVelocity)
			{
				return;
			}
			if (!this.physicBody.isKinematic && !this.physicBody.HasMeaningfulVelocity())
			{
				return;
			}
			Transform transform = base.transform;
			Vector3 tEulerAngles = transform.eulerAngles;
			Vector3 tPosition = transform.position;
			if (this.lastPosition != tPosition || this.lastEulers != tEulerAngles)
			{
				float time = Time.time;
				float lastTimeRatio = 1f / (time - this.lastUpdateTime);
				if (this.physicBody.isKinematic || (this.mainHandler != null && this.mainHandler.gripInfo.type == Handle.GripInfo.Type.HandSync))
				{
					this.lastLinearVelocity = (tPosition - this.lastPosition) * lastTimeRatio;
					this.lastAngularVelocity = (tEulerAngles - this.lastEulers) * (0.017453292f * lastTimeRatio);
				}
				else
				{
					this.lastLinearVelocity = this.physicBody.velocity;
					this.lastAngularVelocity = this.physicBody.angularVelocity;
				}
				this.lastUpdateTime = Time.time;
			}
			this.lastPosition = tPosition;
			this.lastEulers = tEulerAngles;
		}

		/// <summary>
		/// Gets the item's velocity at the given world space position. Used to determine impact velocity for both real and manufactured collisions.
		/// </summary>
		// Token: 0x06002148 RID: 8520 RVA: 0x000E3EF8 File Offset: 0x000E20F8
		public Vector3 GetItemPointVelocity(Vector3 worldSpacePosition, bool useCalculated = false)
		{
			if (!useCalculated)
			{
				return this.physicBody.GetPointVelocity(worldSpacePosition);
			}
			Vector3 centerToPoint = base.transform.InverseTransformPoint(worldSpacePosition) - this.physicBody.centerOfMass;
			Vector3 pointVelocity = Vector3.Cross(this.lastAngularVelocity, centerToPoint);
			pointVelocity = base.transform.TransformDirection(pointVelocity);
			return pointVelocity + this.lastLinearVelocity;
		}

		// Token: 0x06002149 RID: 8521 RVA: 0x000E3F5A File Offset: 0x000E215A
		public void InvokeMagnetCatchEvent(ItemMagnet itemMagnet, EventTime eventTime)
		{
			this.magnets.Add(itemMagnet);
			if (this.OnMagnetCatchEvent != null)
			{
				this.OnMagnetCatchEvent(itemMagnet, eventTime);
			}
		}

		// Token: 0x0600214A RID: 8522 RVA: 0x000E3F7D File Offset: 0x000E217D
		public void InvokeMagnetReleaseEvent(ItemMagnet itemMagnet, EventTime eventTime)
		{
			this.magnets.Remove(itemMagnet);
			if (this.OnMagnetReleaseEvent != null)
			{
				this.OnMagnetReleaseEvent(itemMagnet, eventTime);
			}
		}

		// Token: 0x0600214B RID: 8523 RVA: 0x000E3FA1 File Offset: 0x000E21A1
		public void ResetCenterOfMass()
		{
			if (this.useCustomCenterOfMass)
			{
				this.physicBody.centerOfMass = this.customCenterOfMass;
				return;
			}
			this.physicBody.ResetCenterOfMass();
		}

		// Token: 0x0600214C RID: 8524 RVA: 0x000E3FC8 File Offset: 0x000E21C8
		public virtual void OnTouchAction(RagdollHand ragdollHand, Interactable interactable, Interactable.Action action)
		{
			if (this.OnTouchActionEvent != null)
			{
				this.OnTouchActionEvent(ragdollHand, interactable, action);
			}
		}

		// Token: 0x0600214D RID: 8525 RVA: 0x000E3FE0 File Offset: 0x000E21E0
		public virtual void OnHeldAction(RagdollHand ragdollHand, Handle handle, Interactable.Action action)
		{
			Interactable.Action configuredStartAction = GameManager.options.invertUseAndSlide ? Interactable.Action.AlternateUseStart : Interactable.Action.UseStart;
			Interactable.Action configuredStopAction = GameManager.options.invertUseAndSlide ? Interactable.Action.AlternateUseStop : Interactable.Action.UseStop;
			bool isStartAction = action == configuredStartAction;
			bool isStopAction = action == configuredStopAction;
			if (isStartAction || isStopAction)
			{
				bool active = false;
				if (isStartAction)
				{
					active = true;
				}
				if (isStopAction)
				{
					active = false;
				}
				int imbuesCount = this.imbues.Count;
				for (int i = 0; i < imbuesCount; i++)
				{
					Imbue imbue = this.imbues[i];
					if (imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal)
					{
						imbue.OnCrystalUse(ragdollHand, active);
					}
				}
			}
			if (this.OnHeldActionEvent != null)
			{
				this.OnHeldActionEvent(ragdollHand, handle, action);
			}
		}

		// Token: 0x0600214E RID: 8526 RVA: 0x000E4090 File Offset: 0x000E2290
		public void RefreshAllowTelekinesis()
		{
			if (this.IsHanded() || this.isGripped)
			{
				int handlesCount = this.handles.Count;
				for (int i = 0; i < handlesCount; i++)
				{
					this.handles[i].SetTelekinesis(false);
				}
				return;
			}
			int handlesCount2 = this.handles.Count;
			for (int j = 0; j < handlesCount2; j++)
			{
				Handle h = this.handles[j];
				if (h.data == null)
				{
					Debug.LogWarning(string.Concat(new string[]
					{
						"Handle(",
						h.name,
						") on Item '",
						base.transform.name,
						"' contains no 'data', defaulting telekenesis to false. [EXPAND FOR MORE INFO]\n\nThis can happen when one of your handles isn't defined in the JSON file, in this case ",
						h.name,
						" is not defined in your 'Interactables' array, please add it to fix this warning."
					}));
					h.SetTelekinesis(false);
				}
				else
				{
					h.SetTelekinesis(h.data.allowTelekinesis);
				}
			}
		}

		// Token: 0x0600214F RID: 8527 RVA: 0x000E417C File Offset: 0x000E237C
		public virtual void OnTelekinesisGrab(Handle handle, SpellTelekinesis teleGrabber)
		{
			this.RefreshCollision(false);
			if (!Item.allTk.Contains(this))
			{
				Item.allTk.Add(this);
			}
			Item.TelekinesisDelegate onTelekinesisGrabEvent = this.OnTelekinesisGrabEvent;
			if (onTelekinesisGrabEvent != null)
			{
				onTelekinesisGrabEvent(handle, teleGrabber);
			}
			EventManager.InvokeSpellUsed("Telekinesis", Player.local.creature, teleGrabber.spellCaster.ragdollHand.side);
		}

		// Token: 0x06002150 RID: 8528 RVA: 0x000E41DF File Offset: 0x000E23DF
		public virtual void OnTelekinesisRelease(Handle handle, SpellTelekinesis teleGrabber, bool tryThrow, bool isGrabbing)
		{
			this.RefreshCollision(false);
			if (Item.allTk.Contains(this))
			{
				Item.allTk.Remove(this);
			}
			Item.TelekinesisReleaseDelegate onTelekinesisReleaseEvent = this.OnTelekinesisReleaseEvent;
			if (onTelekinesisReleaseEvent == null)
			{
				return;
			}
			onTelekinesisReleaseEvent(handle, teleGrabber, tryThrow, isGrabbing);
		}

		// Token: 0x06002151 RID: 8529 RVA: 0x000E4218 File Offset: 0x000E2418
		public virtual void OnGrab(Handle handle, RagdollHand ragdollHand)
		{
			if (this.handlers.Count == 0)
			{
				this.mainHandler = ragdollHand;
			}
			this.handlers.Add(ragdollHand);
			this.StopThrowing();
			this.StopFlying();
			this.IgnoreIsMoving();
			this.lastHandler = ragdollHand;
			this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.grabbed;
			this.physicBody.sleepThreshold = 0f;
			this.RefreshCollision(false);
			this.RefreshAllowTelekinesis();
			if (this.parryActive)
			{
				this.SetParryMagic(false);
			}
			this.SetColliders(ragdollHand.ragdoll.IsPhysicsEnabled(true), false);
			this.lastInteractionTime = Time.time;
			Item.GrabDelegate onGrabEvent = this.OnGrabEvent;
			if (onGrabEvent != null)
			{
				onGrabEvent(handle, ragdollHand);
			}
			EventManager.InvokeItemGrab(handle, ragdollHand);
			Item.InvokeOnItemGrab(this, handle, ragdollHand);
		}

		// Token: 0x06002152 RID: 8530 RVA: 0x000E42E4 File Offset: 0x000E24E4
		public virtual void OnUnGrab(Handle handle, RagdollHand ragdollHand, bool throwing)
		{
			this.handlers.Remove(ragdollHand);
			if (this.handlers.Count == 0)
			{
				this.mainHandler = null;
				this.SetColliders(true, false);
			}
			else
			{
				this.mainHandler = this.handlers[0];
			}
			this.RefreshCollision(throwing);
			if (this.handlers.Count == 0)
			{
				this.physicBody.sleepThreshold = this.orgSleepThreshold;
				this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.dropped;
				if (throwing)
				{
					bool throwVelocityReached = ragdollHand.playerHand && PlayerControl.GetHand(ragdollHand.side).GetHandVelocity().magnitude * (1f / Time.timeScale) > Catalog.gameData.throwVelocity;
					this.Throw(throwVelocityReached ? (Catalog.gameData.throwMultiplier * this.data.throwMultiplier) : 1f, (this.data.HasFlag(ItemFlags.Throwable) && throwVelocityReached) ? Item.FlyDetection.CheckAngle : Item.FlyDetection.Disabled);
				}
				Item.ReleaseDelegate onUngrabEvent = this.OnUngrabEvent;
				if (onUngrabEvent != null)
				{
					onUngrabEvent(handle, ragdollHand, throwing);
				}
			}
			this.RefreshAllowTelekinesis();
			if (!this.IsHanded() && this.parryActive)
			{
				this.SetParryMagic(false);
			}
			this.lastInteractionTime = Time.time;
			Item.ReleaseDelegate onHandleReleaseEvent = this.OnHandleReleaseEvent;
			if (onHandleReleaseEvent != null)
			{
				onHandleReleaseEvent(handle, ragdollHand, throwing);
			}
			EventManager.InvokeItemRelease(handle, ragdollHand, throwing);
			Item.InvokeOnItemUngrab(this, handle, ragdollHand);
		}

		// Token: 0x06002153 RID: 8531 RVA: 0x000E4450 File Offset: 0x000E2650
		public virtual void Throw(float throwMultiplier = 1f, Item.FlyDetection flyDetection = Item.FlyDetection.CheckAngle)
		{
			Item.ThrowingDelegate onThrowEvent = this.OnThrowEvent;
			if (onThrowEvent != null)
			{
				onThrowEvent(this);
			}
			this.isFlying = false;
			this.isThrowed = true;
			this.ignoreIsMoving = false;
			if (flyDetection == Item.FlyDetection.CheckAngle && this.flyDirRef && this.flyRotationSpeed > 0f)
			{
				if (this.flyThrowAngle > 0f)
				{
					if (Vector3.Angle(this.physicBody.velocity.normalized, this.flyDirRef.forward) < this.flyThrowAngle)
					{
						this.isFlying = true;
					}
					else if (this.data.allowFlyBackwards && Vector3.Angle(this.physicBody.velocity.normalized, -this.flyDirRef.forward) < this.flyThrowAngle)
					{
						this.isFlying = true;
					}
				}
				else
				{
					this.isFlying = true;
				}
			}
			if (flyDetection == Item.FlyDetection.Forced)
			{
				this.isFlying = true;
			}
			this.isFlyingBackwards = (this.isFlying && this.flyDirRef && this.data.allowFlyBackwards && Vector3.Dot(this.physicBody.velocity.normalized, -this.flyDirRef.forward) > Vector3.Dot(this.physicBody.velocity.normalized, this.flyDirRef.forward));
			if (throwMultiplier > 1f)
			{
				this.physicBody.velocity = this.physicBody.velocity * Mathf.Clamp(throwMultiplier, 1f, float.PositiveInfinity);
			}
			this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
			this.physicBody.collisionDetectionMode = Catalog.gameData.collisionDetection.throwed;
			if (!Item.allThrowed.Contains(this))
			{
				Item.allThrowed.Add(this);
			}
			if (this.isFlying && this.OnFlyStartEvent != null)
			{
				this.OnFlyStartEvent(this);
			}
		}

		/// <summary>
		/// Adds a recoil force to the item, and a lingering force if the item is held after the initial force is applied.
		/// </summary>
		/// <param name="forcePosAndDir">The position and rotation for the force. This can be any transform and can even be parented to this item.</param>
		/// <param name="initialForce">The amount of force to apply, relative to the force position and direction transform.</param>
		/// <param name="initialForceMode">How to apply initial force, usually impulse or velocity change works best here.</param>
		/// <param name="lingeringForce">How much force should be applied to prevent "weapon snapping". Also relative to the force position and direction transform.</param>
		/// <param name="lingerCurve">How much of that initial force to apply over time. Should usually be an ease-in-east-out curve or straight 1-to-0 line.</param>
		// Token: 0x06002154 RID: 8532 RVA: 0x000E464B File Offset: 0x000E284B
		public virtual void AddRecoil(Transform forcePosAndDir, Vector3 initialForce, ForceMode initialForceMode, Vector3 lingeringForce, AnimationCurve lingerCurve = null, bool temporaryTransform = false)
		{
			if (lingerCurve == null)
			{
				lingerCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			}
			base.StartCoroutine(this.LingeringForce(forcePosAndDir, initialForce, initialForceMode, lingeringForce, lingerCurve, temporaryTransform));
		}

		// Token: 0x06002155 RID: 8533 RVA: 0x000E4682 File Offset: 0x000E2882
		private IEnumerator LingeringForce(Transform forcePosAndDir, Vector3 initialForce, ForceMode initialForceMode, Vector3 lingeringForce, AnimationCurve lingerCurve, bool temporaryTransform)
		{
			float startTime = Time.time;
			float curveDuration = lingerCurve.GetLastTime();
			float endTime = startTime + curveDuration;
			if (lingerCurve.GetLastValue() != 0f)
			{
				Debug.LogWarning("Recoil linger curve doesn't end at value 0! The item may snap suddenly when recoil ends.");
			}
			Vector3 instantaneousForce = forcePosAndDir.right * initialForce.x + forcePosAndDir.up * initialForce.y + forcePosAndDir.forward * initialForce.z;
			this.physicBody.AddForceAtPosition(instantaneousForce, forcePosAndDir.position, initialForceMode);
			while (Time.time < endTime)
			{
				instantaneousForce = forcePosAndDir.right * lingeringForce.x + forcePosAndDir.up * lingeringForce.y + forcePosAndDir.forward * lingeringForce.z;
				instantaneousForce *= lingerCurve.Evaluate(Mathf.Clamp(Time.time - startTime, 0f, curveDuration));
				this.physicBody.AddForceAtPosition(instantaneousForce, forcePosAndDir.position, ForceMode.Force);
				yield return Yielders.FixedUpdate;
				if (this.mainHandler == null || (this.mainHandler.gripInfo.type != Handle.GripInfo.Type.PlayerJoint && this.mainHandler.gripInfo.type != Handle.GripInfo.Type.HandJoint))
				{
					break;
				}
			}
			if (temporaryTransform)
			{
				UnityEngine.Object.Destroy(forcePosAndDir.gameObject);
			}
			yield break;
		}

		// Token: 0x06002156 RID: 8534 RVA: 0x000E46C0 File Offset: 0x000E28C0
		public virtual void OnSnap(Holder holder, bool silent = false)
		{
			silent |= (Level.current != null && !Level.current.loaded);
			this.holder = holder;
			this.StopThrowing();
			this.StopFlying();
			this.IgnoreIsMoving();
			this.RefreshCollision(false);
			this.ToggleImbueDrainOnSnap(true);
			if (!silent && this.audioSource)
			{
				if (this.snapPitchRange > 0f)
				{
					this.audioSource.pitch = UnityEngine.Random.Range(1f - this.snapPitchRange, 1f + this.snapPitchRange);
				}
				if (this.audioContainerSnap)
				{
					this.audioSource.PlayOneShot(this.audioContainerSnap.PickAudioClip(0));
				}
				else if (holder.audioContainer)
				{
					this.audioSource.PlayOneShot(holder.audioContainer.PickAudioClip(0));
				}
			}
			if (AreaManager.Instance != null && AreaManager.Instance.CurrentArea != null)
			{
				Creature creature = holder.GetRootHolder().creature;
				if (creature)
				{
					this.UnRegisterArea();
					this.Hide(creature.hidden);
				}
				else
				{
					Item itemParent = holder.GetRootHolder().parentItem;
					if (itemParent)
					{
						this.UnRegisterArea();
						this.Hide(itemParent.isHidden);
					}
					else
					{
						this.CheckCurrentArea();
					}
				}
			}
			Item.HolderDelegate onSnapEvent = this.OnSnapEvent;
			if (onSnapEvent != null)
			{
				onSnapEvent(holder);
			}
			EventManager.InvokeHolderSnap(holder);
			Item.InvokeOnItemSnap(this, holder);
		}

		// Token: 0x06002157 RID: 8535 RVA: 0x000E4838 File Offset: 0x000E2A38
		public virtual void OnUnSnap(Holder holder, bool silent = false)
		{
			silent |= (Level.current != null && !Level.current.loaded);
			this.ToggleImbueDrainOnSnap(false);
			this.holder = null;
			if (!silent && this.audioSource)
			{
				if (this.snapPitchRange > 0f)
				{
					this.audioSource.pitch = UnityEngine.Random.Range(1f - this.snapPitchRange, 1f + this.snapPitchRange);
				}
				if (this.audioContainerSnap)
				{
					this.audioSource.PlayOneShot(this.audioContainerSnap.PickAudioClip(1));
				}
				else if (holder.audioContainer)
				{
					this.audioSource.PlayOneShot(holder.audioContainer.PickAudioClip(1));
				}
			}
			Item.HolderDelegate onUnSnapEvent = this.OnUnSnapEvent;
			if (onUnSnapEvent != null)
			{
				onUnSnapEvent(holder);
			}
			EventManager.InvokeHolderUnsnap(holder);
			Item.InvokeOnItemUnSnap(this, holder);
		}

		/// <summary>
		/// Start imbue decreasing on snap
		/// Stop imbue decreasing on un-snap
		/// </summary>
		/// <param name="snapped">Did the item snap or un-snap ?</param>
		// Token: 0x06002158 RID: 8536 RVA: 0x000E4928 File Offset: 0x000E2B28
		private void ToggleImbueDrainOnSnap(bool snapped)
		{
			if (this.data == null)
			{
				return;
			}
			if (!this.data.drainImbueOnSnap)
			{
				return;
			}
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (snapped)
			{
				this.imbueDecreaseRoutine = base.StartCoroutine(this.DecreaseImbueRoutine(this.data.imbueEnergyOverTimeOnSnap));
				return;
			}
			if (this.imbueDecreaseRoutine != null)
			{
				base.StopCoroutine(this.imbueDecreaseRoutine);
			}
		}

		/// <summary>
		/// Decrease imbues energy gradually according to the given curve
		/// </summary>
		/// <param name="imbueEnergyOverTimeOnSnap">Curve to follow for energy decrease</param>
		/// <returns></returns>
		// Token: 0x06002159 RID: 8537 RVA: 0x000E498F File Offset: 0x000E2B8F
		private IEnumerator DecreaseImbueRoutine(AnimationCurve imbueEnergyOverTimeOnSnap)
		{
			if (Imbue.infiniteImbue)
			{
				yield break;
			}
			if (this.imbues == null)
			{
				yield break;
			}
			if (this.imbues.Count <= 0)
			{
				yield break;
			}
			float t = 0f;
			float totalTime = imbueEnergyOverTimeOnSnap.GetLastTime();
			float[] onSnapEnergyValues = new float[this.imbues.Count];
			int imbuesCount = this.imbues.Count;
			for (int i = 0; i < imbuesCount; i++)
			{
				onSnapEnergyValues[i] = this.imbues[i].energy;
			}
			while (t < totalTime)
			{
				imbuesCount = this.imbues.Count;
				for (int j = 0; j < imbuesCount; j++)
				{
					this.imbues[j].SetEnergyInstant(onSnapEnergyValues[j] * Mathf.Clamp01(imbueEnergyOverTimeOnSnap.Evaluate(t)));
				}
				t += Time.deltaTime;
				yield return Yielders.EndOfFrame;
			}
			imbuesCount = this.imbues.Count;
			for (int k = 0; k < imbuesCount; k++)
			{
				float lastCurveValue = 1f - Mathf.Clamp01(imbueEnergyOverTimeOnSnap.Evaluate(totalTime));
				this.imbues[k].SetEnergyInstant(Mathf.Lerp(onSnapEnergyValues[k], 0f, lastCurveValue));
			}
			yield break;
		}

		// Token: 0x0600215A RID: 8538 RVA: 0x000E49A8 File Offset: 0x000E2BA8
		public virtual void SetMeshLayer(int layer)
		{
			base.gameObject.layer = ((this.forceMeshLayer >= 0) ? this.forceMeshLayer : layer);
			int uiLayer = Common.GetLayer(LayerName.UI);
			int renderersCount = this.renderers.Count;
			for (int i = 0; i < renderersCount; i++)
			{
				Renderer renderer = this.renderers[i];
				if (!(renderer == null) && renderer.gameObject.layer != uiLayer)
				{
					renderer.gameObject.layer = ((this.forceMeshLayer >= 0) ? this.forceMeshLayer : layer);
				}
			}
		}

		// Token: 0x17000214 RID: 532
		// (get) Token: 0x0600215B RID: 8539 RVA: 0x000E4A32 File Offset: 0x000E2C32
		// (set) Token: 0x0600215C RID: 8540 RVA: 0x000E4A3A File Offset: 0x000E2C3A
		public int currentPhysicsLayer { get; protected set; }

		// Token: 0x0600215D RID: 8541 RVA: 0x000E4A43 File Offset: 0x000E2C43
		public virtual void SetColliderAndMeshLayer(int layer, bool force = false)
		{
			if (this.currentPhysicsLayer == layer && !force)
			{
				return;
			}
			this.currentPhysicsLayer = layer;
			this.SetColliderLayer(layer);
			this.SetMeshLayer(layer);
		}

		// Token: 0x0600215E RID: 8542 RVA: 0x000E4A68 File Offset: 0x000E2C68
		public virtual void SetColliderLayer(int layer)
		{
			if (base.gameObject == null)
			{
				return;
			}
			if (this.colliderGroups.CountCheck((int count) => count > 0))
			{
				int colliderGroupsCount = this.colliderGroups.Count;
				for (int i = 0; i < colliderGroupsCount; i++)
				{
					ColliderGroup colliderGroup = this.colliderGroups[i];
					if (colliderGroup.colliders == null)
					{
						colliderGroup.GroupSetup();
					}
					if (colliderGroup.colliders != null)
					{
						int collidersCount = colliderGroup.colliders.Count;
						for (int j = 0; j < collidersCount; j++)
						{
							Collider collider = colliderGroup.colliders[j];
							if (!(collider.gameObject == null) && collider.gameObject.layer != Common.GetLayer(LayerName.UI))
							{
								if (this.data != null)
								{
									collider.gameObject.layer = ((this.forcedLayer != LayerName.None) ? Common.GetLayer(this.forcedLayer) : layer);
								}
								else
								{
									collider.gameObject.layer = layer;
								}
							}
						}
					}
				}
			}
			else
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Item ",
					base.name,
					" (",
					this.data.id,
					") has no collider groups on it! This is a potential problem."
				}));
				foreach (Collider collider2 in base.GetComponentsInChildren<Collider>())
				{
					if (collider2.gameObject.layer != Common.GetLayer(LayerName.Touch) && collider2.gameObject.layer != Common.GetLayer(LayerName.UI))
					{
						collider2.gameObject.layer = layer;
					}
				}
			}
			Item.SetColliderLayerEvent onSetColliderLayerEvent = this.OnSetColliderLayerEvent;
			if (onSetColliderLayerEvent == null)
			{
				return;
			}
			onSetColliderLayerEvent(this, layer);
		}

		// Token: 0x0600215F RID: 8543 RVA: 0x000E4C2C File Offset: 0x000E2E2C
		public virtual void GetFurthestDamagerCollider(out Damager furthestDamager, out Collider furthestCollider, Vector3? origin = null, bool ignoreHandleDamagers = true)
		{
			if (origin == null)
			{
				Handle handle = this.mainHandleRight;
				origin = new Vector3?((handle != null) ? handle.transform.position : base.transform.position);
			}
			furthestDamager = null;
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int i = 0; i < collisionHandlersCount; i++)
			{
				int damagersCount = this.mainCollisionHandler.damagers.Count;
				for (int j = 0; j < damagersCount; j++)
				{
					Damager damager = this.collisionHandlers[i].damagers[j];
					if ((!(furthestDamager != null) || !damager.data.id.Contains("Handle") || !ignoreHandleDamagers) && (furthestDamager == null || (furthestDamager.data.id.Contains("Handle") && ignoreHandleDamagers) || (damager.transform.position - origin.Value).sqrMagnitude > (furthestDamager.transform.position - origin.Value).sqrMagnitude))
					{
						furthestDamager = damager;
					}
				}
			}
			if (furthestDamager == null)
			{
				furthestCollider = null;
				return;
			}
			furthestCollider = furthestDamager.colliderOnly;
			if (furthestCollider == null)
			{
				furthestCollider = furthestDamager.colliderGroup.colliders[0];
				int collidersCount = furthestDamager.colliderGroup.colliders.Count;
				for (int k = 1; k < collidersCount; k++)
				{
					Collider collider = furthestDamager.colliderGroup.colliders[k];
					if ((collider.transform.position - origin.Value).sqrMagnitude > (furthestCollider.transform.position - origin.Value).sqrMagnitude)
					{
						furthestCollider = collider;
					}
				}
			}
		}

		// Token: 0x06002160 RID: 8544 RVA: 0x000E4E15 File Offset: 0x000E3015
		[ContextMenu("RefreshCollision")]
		protected virtual void RefreshCollisionTest()
		{
			this.RefreshCollision(false);
		}

		// Token: 0x06002161 RID: 8545 RVA: 0x000E4E20 File Offset: 0x000E3020
		public virtual void RefreshCollision(bool throwing = false)
		{
			this.handlerArmGrabbed = false;
			this.leftPlayerHand = null;
			this.rightPlayerHand = null;
			this.leftNpcHand = null;
			this.rightNpcHand = null;
			this.ResetRagdollCollision();
			this.ResetObjectCollision();
			int handlersCount = this.handlers.Count;
			for (int i = 0; i < handlersCount; i++)
			{
				RagdollHand handler = this.handlers[i];
				if (handler.playerHand)
				{
					if (handler.playerHand.side == Side.Left)
					{
						this.leftPlayerHand = handler.playerHand;
					}
					else
					{
						this.rightPlayerHand = handler.playerHand;
					}
				}
				else if (handler.side == Side.Left)
				{
					this.leftNpcHand = handler;
				}
				else
				{
					this.rightNpcHand = handler;
				}
			}
			int tkHandlersCount = this.tkHandlers.Count;
			for (int j = 0; j < tkHandlersCount; j++)
			{
				SpellCaster caster = this.tkHandlers[j];
				if (caster.ragdollHand.playerHand)
				{
					if (caster.ragdollHand.playerHand.side == Side.Left)
					{
						this.leftPlayerHand = caster.ragdollHand.playerHand;
					}
					else
					{
						this.rightPlayerHand = caster.ragdollHand.playerHand;
					}
				}
				else if (caster.ragdollHand.side == Side.Left)
				{
					this.leftNpcHand = caster.ragdollHand;
				}
				else
				{
					this.rightNpcHand = caster.ragdollHand;
				}
			}
			Player local = Player.local;
			if (((local != null) ? local.handLeft.ragdollHand : null) && Player.local.handLeft.ragdollHand.climb.gripItem == this)
			{
				this.leftPlayerHand = Player.local.handLeft;
			}
			Player local2 = Player.local;
			if (((local2 != null) ? local2.handRight.ragdollHand : null) && Player.local.handRight.ragdollHand.climb.gripItem == this)
			{
				this.rightPlayerHand = Player.local.handRight;
			}
			if (this.rightPlayerHand || this.leftPlayerHand)
			{
				RagdollPart.Type feetFilter = RagdollPart.Type.LeftFoot | RagdollPart.Type.RightFoot;
				if (this.data.playerGrabAndGripChangeLayer)
				{
					this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
				}
				if (this.isTelekinesisGrabbed)
				{
					if (this.rightPlayerHand || this.leftPlayerHand)
					{
						if (this.rightPlayerHand && this.leftPlayerHand)
						{
							this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand | feetFilter);
							return;
						}
						if (this.rightPlayerHand)
						{
							this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.LeftHand | feetFilter);
							return;
						}
						if (this.leftPlayerHand)
						{
							this.IgnoreRagdollCollision(this.leftPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.RightHand | feetFilter);
							return;
						}
					}
					else if (this.rightNpcHand || this.leftNpcHand)
					{
						if (this.rightNpcHand && this.leftNpcHand)
						{
							this.IgnoreRagdollCollision(this.rightNpcHand.creature.ragdoll, RagdollPart.Type.LeftHand | RagdollPart.Type.RightHand);
							return;
						}
						if (this.rightNpcHand)
						{
							this.IgnoreRagdollCollision(this.rightNpcHand.creature.ragdoll, RagdollPart.Type.LeftHand);
							return;
						}
						if (this.leftNpcHand)
						{
							this.IgnoreRagdollCollision(this.leftNpcHand.creature.ragdoll, RagdollPart.Type.RightHand);
							return;
						}
					}
				}
				else if (Player.selfCollision)
				{
					RagdollPart.Type leftArmFilter = RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand;
					RagdollPart.Type rightArmFilter = RagdollPart.Type.RightArm | RagdollPart.Type.RightHand;
					RagdollPart.Type nonArmFilter = ~(leftArmFilter | rightArmFilter);
					if (this.rightPlayerHand && this.leftPlayerHand)
					{
						this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, nonArmFilter);
					}
					else if (this.rightPlayerHand)
					{
						this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, leftArmFilter | nonArmFilter);
					}
					else if (this.leftPlayerHand)
					{
						this.IgnoreRagdollCollision(this.leftPlayerHand.ragdollHand.creature.ragdoll, rightArmFilter | nonArmFilter);
					}
					if (this.isPenetrating)
					{
						int collisionHandlersCount = this.collisionHandlers.Count;
						for (int index = 0; index < collisionHandlersCount; index++)
						{
							CollisionHandler collisionHandler = this.collisionHandlers[index];
							for (int k = collisionHandler.collisions.Length - 1; k >= 0; k--)
							{
								CollisionInstance collisionHandlerCollision = collisionHandler.collisions[k];
								if (collisionHandlerCollision.active && collisionHandlerCollision.damageStruct.active && collisionHandlerCollision.damageStruct.penetration != DamageStruct.Penetration.None && collisionHandlerCollision.damageStruct.hitRagdollPart && collisionHandlerCollision.damageStruct.hitRagdollPart.ragdoll.creature.isPlayer)
								{
									int collidersCount = collisionHandlerCollision.sourceColliderGroup.colliders.Count;
									for (int l = 0; l < collidersCount; l++)
									{
										Collider collider = collisionHandlerCollision.sourceColliderGroup.colliders[l];
										if (collisionHandlerCollision.damageStruct.hitRagdollPart)
										{
											collisionHandlerCollision.damageStruct.hitRagdollPart.ragdoll.IgnoreCollision(collider, true, (RagdollPart.Type)0);
										}
										else
										{
											Physics.IgnoreCollision(collisionHandlerCollision.targetCollider, collider, true);
										}
									}
								}
							}
						}
						return;
					}
				}
				else
				{
					if (this.rightPlayerHand && this.leftPlayerHand)
					{
						this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, feetFilter);
						return;
					}
					if (this.rightPlayerHand)
					{
						this.IgnoreRagdollCollision(this.rightPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.LeftHand | feetFilter);
						return;
					}
					if (this.leftPlayerHand)
					{
						this.IgnoreRagdollCollision(this.leftPlayerHand.ragdollHand.creature.ragdoll, RagdollPart.Type.RightHand | feetFilter);
						return;
					}
				}
			}
			else if (this.rightNpcHand || this.leftNpcHand)
			{
				if ((this.rightNpcHand && (this.rightNpcHand.creature.state != Creature.State.Alive || this.rightNpcHand.creature.ragdoll.standingUp)) || (this.leftNpcHand && (this.leftNpcHand.creature.state != Creature.State.Alive || this.leftNpcHand.creature.ragdoll.standingUp)))
				{
					this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
					return;
				}
				if (this.rightNpcHand && (this.rightNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.RightHand).isGrabbed || this.rightNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.RightArm).isGrabbed))
				{
					this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
					this.handlerArmGrabbed = true;
					return;
				}
				if (this.leftNpcHand && (this.leftNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.LeftHand).isGrabbed || this.leftNpcHand.creature.ragdoll.GetPart(RagdollPart.Type.LeftArm).isGrabbed))
				{
					this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
					this.handlerArmGrabbed = true;
					return;
				}
				RagdollHand npcHand = this.rightNpcHand ? this.rightNpcHand : this.leftNpcHand;
				this.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.ItemAndRagdollOnly), false);
				if (npcHand.ragdoll.allowSelfDamage)
				{
					this.IgnoreRagdollCollision(npcHand.creature.ragdoll, this.rightNpcHand ? RagdollPart.Type.RightHand : RagdollPart.Type.LeftHand);
				}
				else
				{
					this.IgnoreRagdollCollision(npcHand.creature.ragdoll);
				}
				RagdollHand otherHand = npcHand.otherHand;
				if ((otherHand != null) ? otherHand.grabbedHandle : null)
				{
					this.IgnoreObjectCollision(npcHand.otherHand.grabbedHandle.item);
					return;
				}
			}
			else
			{
				if (this.holder)
				{
					this.ResetRagdollCollision();
					this.ResetObjectCollision();
					this.SetColliderAndMeshLayer(GameManager.GetLayer(throwing ? LayerName.MovingItem : LayerName.DroppedItem), false);
					return;
				}
				this.SetColliderAndMeshLayer(GameManager.GetLayer(throwing ? LayerName.MovingItem : LayerName.DroppedItem), false);
			}
		}

		// Token: 0x06002162 RID: 8546 RVA: 0x000E5680 File Offset: 0x000E3880
		public virtual void IgnoreRagdollCollision(Ragdoll ragdoll)
		{
			this.ResetRagdollCollision();
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int i = 0; i < colliderGroupsCount; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				int collidersCount = colliderGroup.colliders.Count;
				for (int j = 0; j < collidersCount; j++)
				{
					Collider collider = colliderGroup.colliders[j];
					ragdoll.IgnoreCollision(collider, true, (RagdollPart.Type)0);
				}
			}
			this.ignoredRagdoll = ragdoll;
		}

		// Token: 0x06002163 RID: 8547 RVA: 0x000E56F4 File Offset: 0x000E38F4
		public virtual void IgnoreRagdollCollision(Ragdoll ragdoll, RagdollPart.Type ignoredParts)
		{
			if (this.ignoreRagdollCollisionRoutine != null)
			{
				base.StopCoroutine(this.ignoreRagdollCollisionRoutine);
				this.ignoreRagdollCollisionRoutine = null;
			}
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int i = 0; i < colliderGroupsCount; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				int collidersCount = colliderGroup.colliders.Count;
				for (int j = 0; j < collidersCount; j++)
				{
					Collider collider = colliderGroup.colliders[j];
					ragdoll.IgnoreCollision(collider, true, ignoredParts);
				}
			}
			Item.IgnoreRagdollCollisionEvent onIgnoreRagdollCollision = this.OnIgnoreRagdollCollision;
			if (onIgnoreRagdollCollision != null)
			{
				onIgnoreRagdollCollision(this, ragdoll, true, ignoredParts);
			}
			this.ignoredRagdoll = ragdoll;
		}

		// Token: 0x06002164 RID: 8548 RVA: 0x000E5792 File Offset: 0x000E3992
		public virtual void IgnoreRagdollCollision(Ragdoll ragdoll, float duration)
		{
			if (this.ignoreRagdollCollisionRoutine != null)
			{
				base.StopCoroutine(this.ignoreRagdollCollisionRoutine);
			}
			this.ignoreRagdollCollisionRoutine = base.StartCoroutine(this.IgnoreRagdollCollisionRoutine(ragdoll, duration));
		}

		// Token: 0x06002165 RID: 8549 RVA: 0x000E57BC File Offset: 0x000E39BC
		public IEnumerator IgnoreRagdollCollisionRoutine(Ragdoll ragdoll, float duration)
		{
			this.IgnoreRagdollCollision(ragdoll);
			yield return new WaitForSeconds(duration);
			this.ResetRagdollCollision();
			yield break;
		}

		// Token: 0x06002166 RID: 8550 RVA: 0x000E57DC File Offset: 0x000E39DC
		public virtual void ResetRagdollCollision()
		{
			if (!this.ignoredRagdoll)
			{
				return;
			}
			if (this.ignoreRagdollCollisionRoutine != null)
			{
				base.StopCoroutine(this.ignoreRagdollCollisionRoutine);
				this.ignoreRagdollCollisionRoutine = null;
			}
			int count = this.colliderGroups.Count;
			for (int i = 0; i < count; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				int collidersCount = colliderGroup.colliders.Count;
				for (int j = 0; j < collidersCount; j++)
				{
					Collider collider = colliderGroup.colliders[j];
					this.ignoredRagdoll.IgnoreCollision(collider, false, (RagdollPart.Type)0);
				}
			}
			Item.IgnoreRagdollCollisionEvent onIgnoreRagdollCollision = this.OnIgnoreRagdollCollision;
			if (onIgnoreRagdollCollision != null)
			{
				onIgnoreRagdollCollision(this, this.ignoredRagdoll, false, (RagdollPart.Type)0);
			}
			this.ignoredRagdoll = null;
		}

		// Token: 0x06002167 RID: 8551 RVA: 0x000E5892 File Offset: 0x000E3A92
		public virtual void IgnoreObjectCollision(Item item)
		{
			this.ResetObjectCollision();
			this.IgnoreItemCollision(item, true);
			this.ignoredItem = item;
		}

		// Token: 0x06002168 RID: 8552 RVA: 0x000E58AC File Offset: 0x000E3AAC
		public virtual void IgnoreItemCollision(Item item, bool ignore = true)
		{
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int i = 0; i < colliderGroupsCount; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				int collidersCount = colliderGroup.colliders.Count;
				for (int j = 0; j < collidersCount; j++)
				{
					Collider collider = colliderGroup.colliders[j];
					int itemColliderGroupsCount = item.colliderGroups.Count;
					for (int k = 0; k < itemColliderGroupsCount; k++)
					{
						ColliderGroup colliderGroup2 = item.colliderGroups[k];
						int itemColliders = colliderGroup2.colliders.Count;
						for (int l = 0; l < itemColliders; l++)
						{
							Collider collider2 = colliderGroup2.colliders[l];
							Physics.IgnoreCollision(collider, collider2, ignore);
						}
					}
				}
			}
			Item.IgnoreItemCollisionEvent onIgnoreItemCollision = this.OnIgnoreItemCollision;
			if (onIgnoreItemCollision == null)
			{
				return;
			}
			onIgnoreItemCollision(this, item, ignore);
		}

		// Token: 0x06002169 RID: 8553 RVA: 0x000E5988 File Offset: 0x000E3B88
		public virtual void ResetObjectCollision()
		{
			if (!this.ignoredItem)
			{
				return;
			}
			this.IgnoreItemCollision(this.ignoredItem, false);
			this.ignoredItem = null;
		}

		// Token: 0x0600216A RID: 8554 RVA: 0x000E59AC File Offset: 0x000E3BAC
		public virtual void IgnoreColliderCollision(Collider targetCollider)
		{
			this.ResetColliderCollision();
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int i = 0; i < colliderGroupsCount; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				int collidersCount = colliderGroup.colliders.Count;
				for (int j = 0; j < collidersCount; j++)
				{
					Physics.IgnoreCollision(colliderGroup.colliders[j], targetCollider, true);
				}
			}
			Item.IgnoreColliderCollisionEvent onIgnoreColliderCollision = this.OnIgnoreColliderCollision;
			if (onIgnoreColliderCollision != null)
			{
				onIgnoreColliderCollision(this, targetCollider, true);
			}
			this.ignoredCollider = targetCollider;
		}

		// Token: 0x0600216B RID: 8555 RVA: 0x000E5A30 File Offset: 0x000E3C30
		public virtual void ResetColliderCollision()
		{
			if (!this.ignoredCollider)
			{
				return;
			}
			int colliderGroupsCount = this.colliderGroups.Count;
			for (int i = 0; i < colliderGroupsCount; i++)
			{
				ColliderGroup colliderGroup = this.colliderGroups[i];
				int collidersCount = colliderGroup.colliders.Count;
				for (int j = 0; j < collidersCount; j++)
				{
					Physics.IgnoreCollision(colliderGroup.colliders[j], this.ignoredCollider, false);
				}
			}
			Item.IgnoreColliderCollisionEvent onIgnoreColliderCollision = this.OnIgnoreColliderCollision;
			if (onIgnoreColliderCollision != null)
			{
				onIgnoreColliderCollision(this, this.ignoredCollider, true);
			}
			this.ignoredCollider = null;
		}

		// Token: 0x0600216C RID: 8556 RVA: 0x000E5AC5 File Offset: 0x000E3CC5
		public virtual bool CanPenetratePart(RagdollPart part)
		{
			return this.penetrateNotAllowedParts.Contains(part);
		}

		// Token: 0x0600216D RID: 8557 RVA: 0x000E5AD3 File Offset: 0x000E3CD3
		public virtual void PreventPenetration(RagdollPart part)
		{
			this.penetrateNotAllowedParts.Add(part);
		}

		// Token: 0x0600216E RID: 8558 RVA: 0x000E5AE2 File Offset: 0x000E3CE2
		public virtual void AllowPenetration(RagdollPart part)
		{
			this.penetrateNotAllowedParts.Remove(part);
		}

		// Token: 0x0600216F RID: 8559 RVA: 0x000E5AF4 File Offset: 0x000E3CF4
		public virtual bool FullyUnpenetrate()
		{
			if (!this.isPenetrating)
			{
				return false;
			}
			for (int c = 0; c < this.collisionHandlers.Count; c++)
			{
				CollisionHandler handler = this.collisionHandlers[c];
				for (int d = 0; d < handler.damagers.Count; d++)
				{
					handler.damagers[d].UnPenetrateAll();
				}
			}
			return true;
		}

		// Token: 0x06002170 RID: 8560 RVA: 0x000E5B56 File Offset: 0x000E3D56
		public void InvokeZoneEvent(Zone zone, bool enter)
		{
			Item.ZoneEvent onZoneEvent = this.OnZoneEvent;
			if (onZoneEvent != null)
			{
				onZoneEvent(zone, enter);
			}
			if (enter)
			{
				this.zones.Add(zone);
				return;
			}
			this.zones.Remove(zone);
		}

		// Token: 0x06002171 RID: 8561 RVA: 0x000E5B8C File Offset: 0x000E3D8C
		private void OnTriggerEnter(Collider other)
		{
			Creature potentialHit;
			if (other.gameObject.layer == GameManager.GetLayer(LayerName.BodyLocomotion) && other.gameObject.CompareTag("DefenseCollider") && this.lastHandler && !this.lastHandler.creature.isPlayer && this.lastHandler.creature.brain.currentTarget != null && !this.lastHandler.creature.brain.currentTarget.isPlayer && !this.lastHandler.creature.brain.currentTarget.ragdoll.IsPhysicsEnabled(false) && other.transform.parent.TryGetComponent<Creature>(out potentialHit) && potentialHit != this.lastHandler.creature && potentialHit != null && !potentialHit.ragdoll.IsPhysicsEnabled(false) && (potentialHit == null || potentialHit.state > Creature.State.Dead))
			{
				Damager furthestDamager;
				Collider furthestCollider;
				this.GetFurthestDamagerCollider(out furthestDamager, out furthestCollider, null, true);
				RagdollPart closestPart;
				Collider closestCollider;
				int hitMaterialHash;
				potentialHit.ragdoll.GetClosestPartColliderAndMatHash((furthestCollider != null) ? furthestCollider.transform.position : base.transform.position, out closestPart, out closestCollider, out hitMaterialHash, true, null);
				Vector3 itemVelocity = (this.physicBody.velocity.sqrMagnitude > 0f) ? this.physicBody.velocity : this.mainCollisionHandler.lastLinearVelocity;
				CollisionInstance.FakeCollision((furthestDamager != null) ? furthestDamager.colliderGroup : null, closestPart.colliderGroup, furthestCollider, closestCollider, itemVelocity, (furthestCollider != null) ? furthestCollider.transform.position : base.transform.position, closestCollider.transform.position, -itemVelocity.normalized, null, null, new int?(hitMaterialHash), 1f, 0.1f);
			}
		}

		// Token: 0x06002172 RID: 8562 RVA: 0x000E5D9C File Offset: 0x000E3F9C
		private void OnDestroy()
		{
			if ((!this.despawned || this.fellOutOfBounds) && this._owner == Item.Owner.Player)
			{
				Debug.LogError(string.Concat(new string[]
				{
					"Item ",
					this.itemId,
					" on ",
					base.gameObject.name,
					" was lost, sending the item to player stash"
				}), base.gameObject);
				Item.potentialLostItems.Add(new ItemContent(this, null, this.contentCustomData, 1));
			}
			if (GameManager.isQuitting)
			{
				return;
			}
			Item.all.Remove(this);
			if (Item.allThrowed.Contains(this))
			{
				Item.allThrowed.Remove(this);
			}
			if (Item.allMoving.Contains(this))
			{
				Item.allMoving.Remove(this);
			}
			if (Item.allTk.Contains(this))
			{
				Item.allTk.Remove(this);
			}
			if (Item.allWorldAttached.Contains(this))
			{
				Item.allWorldAttached.Remove(this);
			}
		}

		// Token: 0x06002173 RID: 8563 RVA: 0x000E5E97 File Offset: 0x000E4097
		public virtual void InvokeTKSpinEvent(Handle held, bool spinning, EventTime eventTime, bool start)
		{
			if (start && this.OnTKSpinStart != null)
			{
				this.OnTKSpinStart(held, spinning, eventTime);
				return;
			}
			if (this.OnTKSpinEnd != null)
			{
				this.OnTKSpinEnd(held, spinning, eventTime);
			}
		}

		/// <summary>
		/// Invoked when the TK held item is repeled away from the player
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="spellTelekinesis"></param>
		/// /// <param name="eventTime"></param>
		// Token: 0x06002174 RID: 8564 RVA: 0x000E5ECA File Offset: 0x000E40CA
		public virtual void InvokeOnTKRepel(Handle handle, SpellTelekinesis spellTelekinesis, EventTime eventTime)
		{
			Item.TelekinesisTemporalDelegate onTelekinesisRepelEvent = this.OnTelekinesisRepelEvent;
			if (onTelekinesisRepelEvent == null)
			{
				return;
			}
			onTelekinesisRepelEvent(handle, spellTelekinesis, eventTime);
		}

		/// <summary>
		/// Invoked when the TK held item is pulled towards the player
		/// </summary>
		/// <param name="handle"></param>
		/// <param name="spellTelekinesis"></param>
		/// <param name="eventTime"></param>
		// Token: 0x06002175 RID: 8565 RVA: 0x000E5EDF File Offset: 0x000E40DF
		public virtual void InvokeOnTKPull(Handle handle, SpellTelekinesis spellTelekinesis, EventTime eventTime)
		{
			Item.TelekinesisTemporalDelegate onTelekinesisPullEvent = this.OnTelekinesisPullEvent;
			if (onTelekinesisPullEvent == null)
			{
				return;
			}
			onTelekinesisPullEvent(handle, spellTelekinesis, eventTime);
		}

		/// <summary>
		/// Invoke the OnContainerAddEvent event from another class
		/// </summary>
		/// <param name="container">Container in which the item was added</param>
		// Token: 0x06002176 RID: 8566 RVA: 0x000E5EF4 File Offset: 0x000E40F4
		public virtual void InvokeOnContainerAddEvent(Container container)
		{
			Item.ContainerEvent onContainerAddEvent = this.OnContainerAddEvent;
			if (onContainerAddEvent == null)
			{
				return;
			}
			onContainerAddEvent(container);
		}

		// Token: 0x06002177 RID: 8567 RVA: 0x000E5F07 File Offset: 0x000E4107
		public virtual void InvokeBreakStartEvent(Breakable breakable)
		{
			Item.BreakStartDelegate onBreakStart = this.OnBreakStart;
			if (onBreakStart == null)
			{
				return;
			}
			onBreakStart(breakable);
		}

		// Token: 0x06002178 RID: 8568 RVA: 0x000E5F1C File Offset: 0x000E411C
		public void OnSpawn(List<ContentCustomData> contentCustomDataList, Item.Owner owner)
		{
			if (contentCustomDataList != null)
			{
				this.OverrideCustomData(contentCustomDataList);
			}
			this._owner = owner;
			this.spawnTime = (this.lastInteractionTime = Time.time);
			FloatHandler floatHandler = this.damageMultiplier;
			if (floatHandler != null)
			{
				floatHandler.Clear();
			}
			FloatHandler floatHandler2 = this.sliceAngleMultiplier;
			if (floatHandler2 != null)
			{
				floatHandler2.Clear();
			}
			IntAddHandler intAddHandler = this.pushLevelMultiplier;
			if (intAddHandler != null)
			{
				intAddHandler.Clear();
			}
			this.CheckCurrentArea();
			Item.InvokeOnItemSpawn(this);
			this.InvokeOnSpawnEvent(EventTime.OnEnd);
		}

		// Token: 0x06002179 RID: 8569 RVA: 0x000E5F98 File Offset: 0x000E4198
		private void InvokeOnSpawnEvent(EventTime eventTime)
		{
			if (this.OnSpawnEvent == null)
			{
				return;
			}
			Delegate[] invocationList = this.OnSpawnEvent.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Item.SpawnEvent eventDelegate = invocationList[i] as Item.SpawnEvent;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(eventTime);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during Item OnSpawnEvent: {0}", e));
					}
				}
			}
		}

		// Token: 0x0600217A RID: 8570 RVA: 0x000E6004 File Offset: 0x000E4204
		private void InvokeOnDespawnEvent(EventTime eventTime)
		{
			if (this.OnDespawnEvent == null)
			{
				return;
			}
			Delegate[] invocationList = this.OnDespawnEvent.GetInvocationList();
			for (int i = 0; i < invocationList.Length; i++)
			{
				Item.SpawnEvent eventDelegate = invocationList[i] as Item.SpawnEvent;
				if (eventDelegate != null)
				{
					try
					{
						eventDelegate(eventTime);
					}
					catch (Exception e)
					{
						Debug.LogError(string.Format("Error during Item OnDespawnEvent: {0}", e));
					}
				}
			}
		}

		// Token: 0x0600217B RID: 8571 RVA: 0x000E6070 File Offset: 0x000E4270
		public void Despawn(float delay)
		{
			if (delay > 0f && !base.IsInvoking("Despawn"))
			{
				base.Invoke("Despawn", delay);
				return;
			}
			this.Despawn();
		}

		// Token: 0x0600217C RID: 8572 RVA: 0x000E609C File Offset: 0x000E429C
		[ContextMenu("Despawn")]
		public override void Despawn()
		{
			base.Despawn();
			if (this == null)
			{
				return;
			}
			this.despawning = true;
			Item.InvokeOnItemDespawn(this);
			this.InvokeOnDespawnEvent(EventTime.OnStart);
			if (this.currentArea != null)
			{
				if (this.currentArea.IsSpawned)
				{
					this.currentArea.SpawnedArea.UnRegisterItem(this);
				}
				this.isRegistered = false;
				this.currentArea = null;
			}
			this.SetCull(false, true);
			if (this.audioContainerSnap)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioContainerSnap);
			}
			if (this.audioContainerInventory)
			{
				Catalog.ReleaseAsset<AudioContainer>(this.audioContainerInventory);
			}
			if (this.holder)
			{
				this.holder.UnSnap(this, false);
			}
			for (int i = this.handlers.Count - 1; i >= 0; i--)
			{
				this.handlers[i].UnGrab(false);
			}
			if (this.isGripped)
			{
				for (int j = Creature.allActive.Count - 1; j >= 0; j--)
				{
					Creature check = Creature.allActive[j];
					if (check.handRight.climb.gripItem == this)
					{
						check.handRight.climb.UnGrip();
					}
					if (check.handLeft.climb.gripItem == this)
					{
						check.handLeft.climb.UnGrip();
					}
				}
			}
			if (this.isTelekinesisGrabbed)
			{
				int handlesCount = this.handles.Count;
				for (int k = 0; k < handlesCount; k++)
				{
					this.handles[k].ReleaseAllTkHandlers();
				}
			}
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int l = 0; l < collisionHandlersCount; l++)
			{
				this.collisionHandlers[l].ClearPhysicModifiers();
			}
			int handlersCount = this.collisionHandlers.Count;
			for (int index = 0; index < handlersCount; index++)
			{
				CollisionHandler collisionHandler = this.collisionHandlers[index];
				int damagersCount = collisionHandler.damagers.Count;
				for (int m = 0; m < damagersCount; m++)
				{
					collisionHandler.damagers[m].UnPenetrateAll();
				}
				for (int n = collisionHandler.penetratedObjects.Count - 1; n >= 0; n--)
				{
					CollisionHandler penetratedObject = collisionHandler.penetratedObjects[n];
					int count = penetratedObject.damagers.Count;
					for (int k2 = 0; k2 < count; k2++)
					{
						penetratedObject.damagers[k2].UnPenetrateAll();
					}
				}
			}
			int imbuesCount = this.imbues.Count;
			for (int i2 = 0; i2 < imbuesCount; i2++)
			{
				Imbue imbue = this.imbues[i2];
				if (imbue != null)
				{
					imbue.UnloadCurrentSpell();
				}
			}
			foreach (Holder holder in base.GetComponentsInChildren<Holder>())
			{
				for (int i3 = holder.items.Count - 1; i3 >= 0; i3--)
				{
					holder.items[i3].Despawn();
				}
			}
			foreach (Effect effect in base.GetComponentsInChildren<Effect>(true))
			{
				try
				{
					effect.Despawn();
				}
				catch (NullReferenceException e)
				{
					string format = "Could not despawn item {0} (instance of {1}) because effect {2} ({3}) despawn threw NRE.";
					object[] array = new object[4];
					array[0] = base.name;
					array[1] = this.data.id;
					array[2] = effect;
					int num = 3;
					object obj;
					if (effect == null)
					{
						obj = null;
					}
					else
					{
						EffectModule module = effect.module;
						if (module == null)
						{
							obj = null;
						}
						else
						{
							EffectData rootData = module.rootData;
							obj = ((rootData != null) ? rootData.id : null);
						}
					}
					array[num] = obj;
					Debug.LogError(string.Format(format, array));
					Debug.LogException(e);
				}
			}
			int revealDecalsCount = this.revealDecals.Count;
			for (int index4 = 0; index4 < revealDecalsCount; index4++)
			{
				RevealDecal revealDecal = this.revealDecals[index4];
				if (revealDecal.revealMaterialController)
				{
					revealDecal.revealMaterialController.Reset();
				}
			}
			if (Item.allWorldAttached.Contains(this))
			{
				Item.allWorldAttached.Remove(this);
			}
			this.StopThrowing();
			this.StopFlying();
			if (this.physicBody != null)
			{
				this.physicBody.velocity = Vector3.zero;
			}
			this.loaded = false;
			if (this.isPooled)
			{
				this.Hide(false);
				this.ReturnToPool();
			}
			else
			{
				base.gameObject.SetActive(false);
				if (this.addressableHandle.IsValid())
				{
					Addressables.ReleaseInstance(this.addressableHandle);
				}
				else
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			if (this.imbueDecreaseRoutine != null)
			{
				base.StopCoroutine(this.imbueDecreaseRoutine);
			}
			this.despawning = false;
			this.despawned = true;
			this.InvokeOnDespawnEvent(EventTime.OnEnd);
		}

		// Token: 0x0600217D RID: 8573 RVA: 0x000E655C File Offset: 0x000E475C
		public void ReturnToPool()
		{
			base.transform.SetParent(ItemData.poolRoot);
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.transform.localScale = Vector3.one;
			this._owner = Item.Owner.None;
			base.gameObject.SetActive(false);
		}

		// Token: 0x0600217E RID: 8574 RVA: 0x000E65BC File Offset: 0x000E47BC
		public Item.HolderPoint GetHolderPoint(string holderPoint)
		{
			Item.HolderPoint hp = this.additionalHolderPoints.Find((Item.HolderPoint x) => x.anchorName == holderPoint);
			if (hp != null)
			{
				return hp;
			}
			if (!string.IsNullOrEmpty(holderPoint))
			{
				Debug.LogWarning(string.Concat(new string[]
				{
					"HolderPoint ",
					holderPoint,
					" not found on item ",
					base.name,
					" : returning default HolderPoint."
				}));
			}
			return this.GetDefaultHolderPoint();
		}

		// Token: 0x0600217F RID: 8575 RVA: 0x000E6640 File Offset: 0x000E4840
		public ItemData.CustomSnap GetCustomSnap(string holderName)
		{
			foreach (ItemData.CustomSnap customSnap in this.customSnaps)
			{
				if (customSnap.holderName == holderName)
				{
					return customSnap;
				}
			}
			return null;
		}

		// Token: 0x06002180 RID: 8576 RVA: 0x000E66A4 File Offset: 0x000E48A4
		public Item.HolderPoint GetDefaultHolderPoint()
		{
			return new Item.HolderPoint(this.holderPoint, "Default");
		}

		/// <summary>
		/// Assign the position and rotation of the item to the spawning cached values.
		/// Stops the physic body from moving.
		/// </summary>
		// Token: 0x06002181 RID: 8577 RVA: 0x000E66B8 File Offset: 0x000E48B8
		public void ResetToSpawningTransformation()
		{
			Transform transform = base.transform;
			transform.position = this.spawnPosition;
			transform.rotation = this.spawnRotation;
			if (this.spawnSkinnedBonesTransforms != null)
			{
				foreach (KeyValuePair<Transform, ValueTuple<Vector3, Quaternion>> pair in this.spawnSkinnedBonesTransforms)
				{
					Transform bone = pair.Key;
					if (pair.Key)
					{
						ValueTuple<Vector3, Quaternion> value = pair.Value;
						Vector3 boneSpawnPosition = value.Item1;
						Quaternion boneSpawnRotation = value.Item2;
						bone.position = boneSpawnPosition;
						bone.rotation = boneSpawnRotation;
					}
				}
			}
			if (this.physicBody)
			{
				this.physicBody.velocity = Vector3.zero;
				this.physicBody.angularVelocity = Vector3.zero;
			}
		}

		// Token: 0x06002182 RID: 8578 RVA: 0x000E6790 File Offset: 0x000E4990
		public void SetPhysicModifier(object handler, float? gravityRatio = null, float massRatio = 1f, float drag = -1f, float angularDrag = -1f, float sleepThreshold = -1f, EffectData effectData = null)
		{
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int i = 0; i < collisionHandlersCount; i++)
			{
				this.collisionHandlers[i].SetPhysicModifier(handler, gravityRatio, massRatio, drag, angularDrag, sleepThreshold, effectData);
			}
		}

		// Token: 0x06002183 RID: 8579 RVA: 0x000E67D4 File Offset: 0x000E49D4
		public void ClearPhysicModifiers()
		{
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int i = 0; i < collisionHandlersCount; i++)
			{
				this.collisionHandlers[i].ClearPhysicModifiers();
			}
		}

		// Token: 0x06002184 RID: 8580 RVA: 0x000E680C File Offset: 0x000E4A0C
		public new void RemovePhysicModifier(object handler)
		{
			int collisionHandlersCount = this.collisionHandlers.Count;
			for (int i = 0; i < collisionHandlersCount; i++)
			{
				this.collisionHandlers[i].RemovePhysicModifier(handler);
			}
		}

		// Token: 0x06002185 RID: 8581 RVA: 0x000E6843 File Offset: 0x000E4A43
		public void IgnoreIsMoving()
		{
			this.ignoreIsMoving = true;
		}

		// Token: 0x06002186 RID: 8582 RVA: 0x000E684C File Offset: 0x000E4A4C
		public void ClearZones()
		{
			List<Zone> tmpList = LazyListPool<Zone>.Instance.Get(this.zones.Count);
			foreach (Zone z in this.zones)
			{
				tmpList.Add(z);
			}
			foreach (Zone zone in tmpList)
			{
				zone.RemoveItem(this);
			}
			LazyListPool<Zone>.Instance.Return(tmpList);
		}

		// Token: 0x06002187 RID: 8583 RVA: 0x000E68FC File Offset: 0x000E4AFC
		public override void AddForce(Vector3 force, ForceMode forceMode, CollisionHandler handler = null)
		{
			base.AddForce(force, forceMode, null);
			this.physicBody.AddForce(force, forceMode);
		}

		// Token: 0x06002188 RID: 8584 RVA: 0x000E6914 File Offset: 0x000E4B14
		public override void AddRadialForce(float force, Vector3 origin, float upwardsModifier, ForceMode forceMode, CollisionHandler handler = null)
		{
			base.AddRadialForce(force, origin, upwardsModifier, forceMode, null);
			this.physicBody.AddRadialForce(force, origin, upwardsModifier, forceMode);
		}

		// Token: 0x06002189 RID: 8585 RVA: 0x000E6932 File Offset: 0x000E4B32
		public override void AddExplosionForce(float force, Vector3 origin, float radius, float upwardsModifier, ForceMode forceMode, CollisionHandler handler = null)
		{
			base.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode, null);
			this.physicBody.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode);
		}

		// Token: 0x0600218A RID: 8586 RVA: 0x000E6954 File Offset: 0x000E4B54
		public void InvokeOnImbuesChange(SpellData spellData, float amount, float change, EventTime time)
		{
			Item.ImbuesChangeEvent onImbuesChangeEvent = this.OnImbuesChangeEvent;
			if (onImbuesChangeEvent != null)
			{
				onImbuesChangeEvent();
			}
			RagdollHand ragdollHand = this.mainHandler;
			if (ragdollHand == null)
			{
				return;
			}
			Creature creature = ragdollHand.creature;
			if (creature == null)
			{
				return;
			}
			creature.UpdateHeldImbues();
		}

		// Token: 0x0600218D RID: 8589 RVA: 0x000E6B5C File Offset: 0x000E4D5C
		[CompilerGenerated]
		private void <GetLocalBounds>g__RecurseEncapsulate|89_0(Transform child, ref Bounds bounds)
		{
			Transform childTransform = child.transform;
			MeshFilter mesh;
			SkinnedMeshRenderer smr;
			if (child.TryGetComponent<MeshFilter>(out mesh) && ((mesh != null) ? mesh.sharedMesh : null) != null)
			{
				Bounds lsBounds = mesh.sharedMesh.bounds;
				Vector3 wsMin = child.TransformPoint(lsBounds.center - lsBounds.extents);
				Vector3 wsMax = child.TransformPoint(lsBounds.center + lsBounds.extents);
				bounds.Encapsulate(base.transform.InverseTransformPoint(wsMin));
				bounds.Encapsulate(base.transform.InverseTransformPoint(wsMax));
			}
			else if (child.TryGetComponent<SkinnedMeshRenderer>(out smr))
			{
				Bounds lsBounds2 = smr.localBounds;
				Vector3 wsMin2 = child.TransformPoint(lsBounds2.center - lsBounds2.extents);
				Vector3 wsMax2 = child.TransformPoint(lsBounds2.center + lsBounds2.extents);
				bounds.Encapsulate(base.transform.InverseTransformPoint(wsMin2));
				bounds.Encapsulate(base.transform.InverseTransformPoint(wsMax2));
			}
			else
			{
				bounds.Encapsulate(base.transform.InverseTransformPoint(childTransform.position));
			}
			for (int i = 0; i < childTransform.childCount; i++)
			{
				Transform grandChild = childTransform.GetChild(i);
				this.<GetLocalBounds>g__RecurseEncapsulate|89_0(grandChild, ref bounds);
			}
		}

		// Token: 0x04001F95 RID: 8085
		public static List<Item> all = new List<Item>();

		// Token: 0x04001F96 RID: 8086
		public static List<Item> allActive = new List<Item>();

		// Token: 0x04001F97 RID: 8087
		public static List<Item> allThrowed = new List<Item>();

		// Token: 0x04001F98 RID: 8088
		public static HashSet<Item> allMoving = new HashSet<Item>();

		// Token: 0x04001F99 RID: 8089
		public static List<Item> allTk = new List<Item>();

		// Token: 0x04001F9A RID: 8090
		public static List<Item> allWorldAttached = new List<Item>();

		// Token: 0x04001F9B RID: 8091
		public static List<ItemContent> potentialLostItems = new List<ItemContent>();

		// Token: 0x04001F9C RID: 8092
		[Tooltip("The Item ID of the item specified in the Catalog")]
		public string itemId;

		// Token: 0x04001F9D RID: 8093
		[Tooltip("Specifies the Holder Point of the item. This specifies the position and rotation of the item when held in a holder, such as on player hips and back. The Z axis/blue arrow specifies towards the floor.")]
		public Transform holderPoint;

		// Token: 0x04001F9E RID: 8094
		[Tooltip("Specifies the spawn point of the item. This specifies the position and rotation of the item when spawned, it's mostly used for itemSpawner and spawning the item via an item book")]
		public Transform spawnPoint;

		// Token: 0x04001F9F RID: 8095
		[Tooltip("Can add additional holderpoints for different interactables.\n \nFor Items on the Item Rack, the anchor must be named HolderRackTopAnchor, or alternatively for the bow rack, HolderRackTopAnchorBow, and HolderRackSideAnchor for Shield rack")]
		public List<Item.HolderPoint> additionalHolderPoints = new List<Item.HolderPoint>();

		// Token: 0x04001FA0 RID: 8096
		[Tooltip("Shows the point that AI will try to block with when they are holding the item.")]
		public Transform parryPoint;

		// Token: 0x04001FA1 RID: 8097
		[Tooltip("Specifies what handle is grabbed by default for the Right Hand")]
		public Handle mainHandleRight;

		// Token: 0x04001FA2 RID: 8098
		[Tooltip("Specifies what handle is grabbed by default for the Left Hand")]
		public Handle mainHandleLeft;

		// Token: 0x04001FA3 RID: 8099
		[Tooltip("Used to point in direction when thrown.\nZ-Axis/Blue Arrow points forwards.")]
		public Transform flyDirRef;

		// Token: 0x04001FA4 RID: 8100
		[Tooltip("States the Preview for the item.")]
		public Preview preview;

		// Token: 0x04001FA5 RID: 8101
		[Tooltip("Tick if the item is attached to the world and not spawned via item spawner or item book.")]
		public bool worldAttached;

		// Token: 0x04001FA6 RID: 8102
		[Tooltip("Tick if the item should keep its parent when it loads into the scene.")]
		public bool keepParent;

		// Token: 0x04001FA7 RID: 8103
		[Tooltip("Radius to depict how close this item needs to be to a creature before the creatures' collision is enabled.")]
		public float creaturePhysicToggleRadius = 2f;

		// Token: 0x04001FA8 RID: 8104
		[Tooltip("Allows user to adjust the center of mass on an object.\nIf unticked, this is automatically adjusted. When ticked, adds a custom gizmo to adjust.\n \nUse this if weight on the item is acting strange.")]
		public bool useCustomCenterOfMass;

		// Token: 0x04001FA9 RID: 8105
		[Tooltip("Position of Center of Mass (if ticked)")]
		public Vector3 customCenterOfMass;

		// Token: 0x04001FAA RID: 8106
		[Tooltip("Used for balance adjustment on a weapon.\n \nUse this if swinging weapons are strange. Adjust the Capsule collider to the width of the weapon.")]
		public bool customInertiaTensor;

		// Token: 0x04001FAB RID: 8107
		[Tooltip("Collider of the Custom Inertia Tensor")]
		public CapsuleCollider customInertiaTensorCollider;

		// Token: 0x04001FAC RID: 8108
		[Tooltip("Allows a custom reference to be able to reference specific gameobjects and scripts in External code.")]
		public List<CustomReference> customReferences = new List<CustomReference>();

		// Token: 0x04001FAD RID: 8109
		[Tooltip("When ticked, item is automatically set as \"Thrown\" when spawned.")]
		public bool forceThrown;

		// Token: 0x04001FAE RID: 8110
		[Tooltip("Forces layer of mesh when an item is spawned.\n\n(Items will have their layer automatically applied when spawned, unless this is set)")]
		public int forceMeshLayer = -1;

		// Token: 0x04001FAF RID: 8111
		private Item.Owner _owner;

		// Token: 0x04001FB0 RID: 8112
		[NonSerialized]
		public bool isUsed = true;

		// Token: 0x04001FB1 RID: 8113
		[NonSerialized]
		public List<Renderer> renderers = new List<Renderer>();

		// Token: 0x04001FB2 RID: 8114
		[NonSerialized]
		public List<FxController> fxControllers = new List<FxController>();

		// Token: 0x04001FB3 RID: 8115
		[NonSerialized]
		public List<FxModule> fxModules = new List<FxModule>();

		// Token: 0x04001FB4 RID: 8116
		[NonSerialized]
		public List<RevealDecal> revealDecals = new List<RevealDecal>();

		// Token: 0x04001FB5 RID: 8117
		[NonSerialized]
		public List<ColliderGroup> colliderGroups = new List<ColliderGroup>();

		// Token: 0x04001FB6 RID: 8118
		[NonSerialized]
		public List<Collider> disabledColliders = new List<Collider>();

		// Token: 0x04001FB7 RID: 8119
		[NonSerialized]
		public List<HingeEffect> effectHinges = new List<HingeEffect>();

		// Token: 0x04001FB8 RID: 8120
		[NonSerialized]
		public List<WhooshPoint> whooshPoints = new List<WhooshPoint>();

		// Token: 0x04001FB9 RID: 8121
		[HideInInspector]
		public LightVolumeReceiver lightVolumeReceiver;

		// Token: 0x04001FBA RID: 8122
		public AudioSource audioSource;

		// Token: 0x04001FBB RID: 8123
		[NonSerialized]
		public List<CollisionHandler> collisionHandlers = new List<CollisionHandler>();

		// Token: 0x04001FBC RID: 8124
		[NonSerialized]
		public List<Handle> handles = new List<Handle>();

		// Token: 0x04001FBD RID: 8125
		[NonSerialized]
		public PhysicBody physicBody;

		// Token: 0x04001FBE RID: 8126
		[NonSerialized]
		public Breakable breakable;

		// Token: 0x04001FBF RID: 8127
		[NonSerialized]
		public List<ParryTarget> parryTargets = new List<ParryTarget>();

		// Token: 0x04001FC0 RID: 8128
		[NonSerialized]
		public Holder holder;

		// Token: 0x04001FC1 RID: 8129
		[NonSerialized]
		private ClothingGenderSwitcher clothingGenderSwitcher;

		// Token: 0x04001FC2 RID: 8130
		[NonSerialized]
		public bool allowGrip = true;

		// Token: 0x04001FC3 RID: 8131
		[NonSerialized]
		public bool hasSlash;

		// Token: 0x04001FC4 RID: 8132
		[NonSerialized]
		public bool hasMetal;

		// Token: 0x04001FC5 RID: 8133
		[NonSerialized]
		public List<ColliderGroup> metalColliderGroups;

		// Token: 0x04001FC6 RID: 8134
		[NonSerialized]
		public FloatHandler sliceAngleMultiplier;

		// Token: 0x04001FC7 RID: 8135
		public FloatHandler damageMultiplier;

		// Token: 0x04001FC8 RID: 8136
		public IntAddHandler pushLevelMultiplier;

		// Token: 0x04001FC9 RID: 8137
		[NonSerialized]
		public Container linkedContainer;

		// Token: 0x04001FCA RID: 8138
		[NonSerialized]
		public List<ContentCustomData> contentCustomData;

		// Token: 0x04001FCB RID: 8139
		[NonSerialized]
		public CollisionHandler mainCollisionHandler;

		// Token: 0x04001FCC RID: 8140
		[NonSerialized]
		public Vector3 customInertiaTensorPos;

		// Token: 0x04001FCD RID: 8141
		[NonSerialized]
		public Quaternion customInertiaTensorRot;

		// Token: 0x04001FCE RID: 8142
		[NonSerialized]
		public bool updateReveal;

		// Token: 0x04001FCF RID: 8143
		[NonSerialized]
		public bool loaded;

		// Token: 0x04001FD0 RID: 8144
		[NonSerialized]
		public bool loadedItemModules;

		// Token: 0x04001FD1 RID: 8145
		[NonSerialized]
		public bool isSwaping;

		// Token: 0x04001FD2 RID: 8146
		[NonSerialized]
		public bool ignoreGravityPush;

		// Token: 0x04001FD3 RID: 8147
		[NonSerialized]
		public HashSet<Zone> zones = new HashSet<Zone>();

		// Token: 0x04001FD4 RID: 8148
		[NonSerialized]
		public AsyncOperationHandle<GameObject> addressableHandle;

		// Token: 0x04001FD5 RID: 8149
		protected HashSet<object> isNotStorableModifiers = new HashSet<object>();

		// Token: 0x04001FD6 RID: 8150
		[NonSerialized]
		public SpawnableArea currentArea;

		// Token: 0x04001FD7 RID: 8151
		[NonSerialized]
		public bool isCulled;

		// Token: 0x04001FD8 RID: 8152
		[NonSerialized]
		public bool isHidden;

		// Token: 0x04001FD9 RID: 8153
		protected bool isRegistered;

		// Token: 0x04001FDA RID: 8154
		protected bool cullingDetectionEnabled;

		// Token: 0x04001FDB RID: 8155
		protected float cullingDetectionCycleSpeed = 1f;

		// Token: 0x04001FDC RID: 8156
		protected float cullingDetectionCycleTime;

		// Token: 0x04001FDD RID: 8157
		[NonSerialized]
		public ItemData data;

		// Token: 0x04001FDE RID: 8158
		[NonSerialized]
		public ItemSpawner spawner;

		// Token: 0x04001FDF RID: 8159
		[NonSerialized]
		public List<RagdollHand> handlers = new List<RagdollHand>();

		// Token: 0x04001FE0 RID: 8160
		[NonSerialized]
		public List<SpellCaster> tkHandlers = new List<SpellCaster>();

		// Token: 0x04001FE1 RID: 8161
		[NonSerialized]
		public RagdollHand mainHandler;

		// Token: 0x04001FE2 RID: 8162
		[NonSerialized]
		public List<Imbue> imbues = new List<Imbue>();

		// Token: 0x04001FE3 RID: 8163
		[NonSerialized]
		public PlayerHand leftPlayerHand;

		// Token: 0x04001FE4 RID: 8164
		[NonSerialized]
		public PlayerHand rightPlayerHand;

		// Token: 0x04001FE5 RID: 8165
		[NonSerialized]
		public RagdollHand leftNpcHand;

		// Token: 0x04001FE6 RID: 8166
		[NonSerialized]
		public RagdollHand rightNpcHand;

		// Token: 0x04001FE7 RID: 8167
		[NonSerialized]
		public bool handlerArmGrabbed;

		// Token: 0x04001FE8 RID: 8168
		[NonSerialized]
		public RagdollHand lastHandler;

		// Token: 0x04001FE9 RID: 8169
		public float snapPitchRange = 0.05f;

		// Token: 0x04001FEA RID: 8170
		public List<Holder> childHolders = new List<Holder>();

		// Token: 0x04001FEB RID: 8171
		[Header("Holder")]
		public List<ItemData.CustomSnap> customSnaps = new List<ItemData.CustomSnap>();

		// Token: 0x04001FEC RID: 8172
		[Header("Fly")]
		[Tooltip("When ticked, item is automatically set as \"Thrown\" when spawned.")]
		public bool flyFromThrow;

		// Token: 0x04001FED RID: 8173
		[Tooltip("Speed of which the item rotates when thrown")]
		public float flyRotationSpeed = 2f;

		// Token: 0x04001FEE RID: 8174
		[Tooltip("Angle offset of the z-axis arrow when thrown.")]
		public float flyThrowAngle;

		// Token: 0x04001FEF RID: 8175
		public bool isTelekinesisGrabbed;

		// Token: 0x04001FF0 RID: 8176
		public bool isThrowed;

		// Token: 0x04001FF1 RID: 8177
		public bool isFlying;

		// Token: 0x04001FF2 RID: 8178
		public bool isFlyingBackwards;

		// Token: 0x04001FF3 RID: 8179
		public bool isMoving;

		// Token: 0x04001FF4 RID: 8180
		public bool wasMoving;

		// Token: 0x04001FF5 RID: 8181
		public bool isGripped;

		// Token: 0x04001FF6 RID: 8182
		public bool isBrokenPiece;

		// Token: 0x04001FF7 RID: 8183
		[NonSerialized]
		public bool isCollidersOn = true;

		// Token: 0x04001FF8 RID: 8184
		public Ragdoll ignoredRagdoll;

		// Token: 0x04001FF9 RID: 8185
		public Item ignoredItem;

		// Token: 0x04001FFA RID: 8186
		public Collider ignoredCollider;

		// Token: 0x04001FFB RID: 8187
		public bool disableSnap;

		// Token: 0x04001FFC RID: 8188
		public AudioContainer audioContainerSnap;

		// Token: 0x04001FFD RID: 8189
		public AudioContainer audioContainerInventory;

		// Token: 0x04001FFE RID: 8190
		[Header("Telekinesis")]
		public float distantGrabSafeDistance = 1f;

		// Token: 0x04001FFF RID: 8191
		public bool distantGrabSpinEnabled = true;

		// Token: 0x04002000 RID: 8192
		public float distantGrabThrowRatio = 1f;

		// Token: 0x04002004 RID: 8196
		public static Action<Item> OnItemSpawn;

		// Token: 0x04002005 RID: 8197
		public static Action<Item> OnItemDespawn;

		// Token: 0x04002006 RID: 8198
		public static Action<Item, Holder> OnItemSnap;

		// Token: 0x04002007 RID: 8199
		public static Action<Item, Holder> OnItemUnSnap;

		// Token: 0x04002008 RID: 8200
		public static Action<Item, Handle, RagdollHand> OnItemGrab;

		// Token: 0x04002009 RID: 8201
		public static Action<Item, Handle, RagdollHand> OnItemUngrab;

		// Token: 0x0400200A RID: 8202
		public Action<Item.Owner, Item.Owner> onOwnerChange;

		// Token: 0x0400200B RID: 8203
		public static Action<Item, Item.Owner, Item.Owner> onAnyOwnerChange;

		// Token: 0x0400200F RID: 8207
		public UIInventory.ItemDelegate OnItemStored;

		// Token: 0x04002010 RID: 8208
		public UIInventory.ItemDelegate OnItemRetrieved;

		// Token: 0x04002011 RID: 8209
		protected Zone zone;

		// Token: 0x04002012 RID: 8210
		public bool isPooled;

		// Token: 0x04002013 RID: 8211
		public bool DisallowDespawn;

		// Token: 0x04002014 RID: 8212
		public bool parryActive;

		// Token: 0x04002015 RID: 8213
		public bool isPenetrating;

		// Token: 0x04002016 RID: 8214
		public static string parryMagicTag = "ParryMagic";

		// Token: 0x04002017 RID: 8215
		public float spawnTime;

		// Token: 0x04002018 RID: 8216
		public float lastInteractionTime;

		// Token: 0x0400202C RID: 8236
		[NonSerialized]
		public List<ItemMagnet> magnets = new List<ItemMagnet>();

		// Token: 0x0400202F RID: 8239
		public Action<AudioContainer> OnSnapAudioLoaded;

		// Token: 0x04002030 RID: 8240
		public Action<AudioContainer> OnInventoryAudioLoaded;

		// Token: 0x04002037 RID: 8247
		[NonSerialized]
		public float orgSleepThreshold;

		// Token: 0x04002038 RID: 8248
		[NonSerialized]
		public float orgMass;

		// Token: 0x04002039 RID: 8249
		[NonSerialized]
		public float totalCombinedMass = -1f;

		// Token: 0x0400203A RID: 8250
		private bool ignoreIsMoving;

		// Token: 0x0400203B RID: 8251
		private Coroutine imbueDecreaseRoutine;

		// Token: 0x0400203C RID: 8252
		[NonSerialized]
		public bool trackVelocity;

		// Token: 0x0400203D RID: 8253
		[NonSerialized]
		public float lastUpdateTime;

		// Token: 0x0400203E RID: 8254
		[NonSerialized]
		public Vector3 lastLinearVelocity;

		// Token: 0x0400203F RID: 8255
		[NonSerialized]
		public Vector3 lastAngularVelocity;

		// Token: 0x04002040 RID: 8256
		[NonSerialized]
		public Vector3 lastPosition;

		// Token: 0x04002041 RID: 8257
		[NonSerialized]
		public Vector3 lastEulers;

		// Token: 0x04002042 RID: 8258
		[NonSerialized]
		public Vector3 spawnPosition;

		// Token: 0x04002043 RID: 8259
		[NonSerialized]
		public Quaternion spawnRotation;

		// Token: 0x04002044 RID: 8260
		[NonSerialized]
		private Dictionary<Transform, ValueTuple<Vector3, Quaternion>> spawnSkinnedBonesTransforms;

		// Token: 0x04002045 RID: 8261
		[NonSerialized]
		public LayerName forcedItemLayer;

		// Token: 0x04002046 RID: 8262
		private HashSet<RagdollPart> penetrateNotAllowedParts = new HashSet<RagdollPart>();

		// Token: 0x04002047 RID: 8263
		private Coroutine ignoreRagdollCollisionRoutine;

		// Token: 0x04002048 RID: 8264
		[NonSerialized]
		public bool despawning;

		// Token: 0x04002049 RID: 8265
		private bool despawned;

		// Token: 0x0400204A RID: 8266
		[NonSerialized]
		public bool fellOutOfBounds;

		// Token: 0x02000954 RID: 2388
		public enum Owner
		{
			// Token: 0x0400446E RID: 17518
			None,
			// Token: 0x0400446F RID: 17519
			Player,
			// Token: 0x04004470 RID: 17520
			Shopkeeper
		}

		// Token: 0x02000955 RID: 2389
		[Serializable]
		public class IconMarker
		{
			// Token: 0x06004329 RID: 17193 RVA: 0x0018E9A7 File Offset: 0x0018CBA7
			public IconMarker(string damagerId, Vector2 position, Damager.Direction direction, float directionAngle)
			{
				this.damagerId = damagerId;
				this.position = position;
				this.direction = direction;
				this.directionAngle = directionAngle;
			}

			// Token: 0x04004471 RID: 17521
			public string damagerId;

			// Token: 0x04004472 RID: 17522
			public Vector2 position;

			// Token: 0x04004473 RID: 17523
			public float directionAngle;

			// Token: 0x04004474 RID: 17524
			public Damager.Direction direction;
		}

		// Token: 0x02000956 RID: 2390
		// (Invoke) Token: 0x0600432B RID: 17195
		public delegate void CullEvent(bool culled);

		// Token: 0x02000957 RID: 2391
		// (Invoke) Token: 0x0600432F RID: 17199
		public delegate void SpawnEvent(EventTime eventTime);

		// Token: 0x02000958 RID: 2392
		// (Invoke) Token: 0x06004333 RID: 17203
		public delegate void ImbuesChangeEvent();

		// Token: 0x02000959 RID: 2393
		// (Invoke) Token: 0x06004337 RID: 17207
		public delegate void ContainerEvent(Container container);

		// Token: 0x0200095A RID: 2394
		// (Invoke) Token: 0x0600433B RID: 17211
		public delegate void ZoneEvent(Zone zone, bool enter);

		// Token: 0x0200095B RID: 2395
		// (Invoke) Token: 0x0600433F RID: 17215
		public delegate void DamageReceivedDelegate(CollisionInstance collisionInstance);

		// Token: 0x0200095C RID: 2396
		// (Invoke) Token: 0x06004343 RID: 17219
		public delegate void GrabDelegate(Handle handle, RagdollHand ragdollHand);

		// Token: 0x0200095D RID: 2397
		// (Invoke) Token: 0x06004347 RID: 17223
		public delegate void ReleaseDelegate(Handle handle, RagdollHand ragdollHand, bool throwing);

		// Token: 0x0200095E RID: 2398
		// (Invoke) Token: 0x0600434B RID: 17227
		public delegate void HolderDelegate(Holder holder);

		// Token: 0x0200095F RID: 2399
		// (Invoke) Token: 0x0600434F RID: 17231
		public delegate void ThrowingDelegate(Item item);

		// Token: 0x02000960 RID: 2400
		// (Invoke) Token: 0x06004353 RID: 17235
		public delegate void TelekinesisDelegate(Handle handle, SpellTelekinesis teleGrabber);

		// Token: 0x02000961 RID: 2401
		// (Invoke) Token: 0x06004357 RID: 17239
		public delegate void TelekinesisReleaseDelegate(Handle handle, SpellTelekinesis teleGrabber, bool tryThrow, bool isGrabbing);

		// Token: 0x02000962 RID: 2402
		// (Invoke) Token: 0x0600435B RID: 17243
		public delegate void TelekinesisTemporalDelegate(Handle handle, SpellTelekinesis teleGrabber, EventTime eventTime);

		// Token: 0x02000963 RID: 2403
		// (Invoke) Token: 0x0600435F RID: 17247
		public delegate void TelekinesisSpinEvent(Handle held, bool spinning, EventTime eventTime);

		// Token: 0x02000964 RID: 2404
		// (Invoke) Token: 0x06004363 RID: 17251
		public delegate void TouchActionDelegate(RagdollHand ragdollHand, Interactable interactable, Interactable.Action action);

		// Token: 0x02000965 RID: 2405
		// (Invoke) Token: 0x06004367 RID: 17255
		public delegate void HeldActionDelegate(RagdollHand ragdollHand, Handle handle, Interactable.Action action);

		// Token: 0x02000966 RID: 2406
		// (Invoke) Token: 0x0600436B RID: 17259
		public delegate void MagnetDelegate(ItemMagnet itemMagnet, EventTime eventTime);

		// Token: 0x02000967 RID: 2407
		// (Invoke) Token: 0x0600436F RID: 17263
		public delegate void BreakStartDelegate(Breakable breakable);

		// Token: 0x02000968 RID: 2408
		// (Invoke) Token: 0x06004373 RID: 17267
		public delegate void LoadDelegate();

		// Token: 0x02000969 RID: 2409
		// (Invoke) Token: 0x06004377 RID: 17271
		public delegate void OverrideContentCustomDataEvent(List<ContentCustomData> contentCustomData);

		// Token: 0x0200096A RID: 2410
		// (Invoke) Token: 0x0600437B RID: 17275
		public delegate void IgnoreRagdollCollisionEvent(Item item, Ragdoll ragdoll, bool ignore, RagdollPart.Type ignoredParts);

		// Token: 0x0200096B RID: 2411
		// (Invoke) Token: 0x0600437F RID: 17279
		public delegate void IgnoreItemCollisionEvent(Item item, Item other, bool ignore);

		// Token: 0x0200096C RID: 2412
		// (Invoke) Token: 0x06004383 RID: 17283
		public delegate void IgnoreColliderCollisionEvent(Item item, Collider other, bool ignore);

		// Token: 0x0200096D RID: 2413
		// (Invoke) Token: 0x06004387 RID: 17287
		public delegate void SetCollidersEvent(Item item, bool active, bool force);

		// Token: 0x0200096E RID: 2414
		// (Invoke) Token: 0x0600438B RID: 17291
		public delegate void SetColliderLayerEvent(Item item, int layer);

		// Token: 0x0200096F RID: 2415
		public enum FlyDetection
		{
			// Token: 0x04004476 RID: 17526
			Disabled,
			// Token: 0x04004477 RID: 17527
			CheckAngle,
			// Token: 0x04004478 RID: 17528
			Forced
		}

		// Token: 0x02000970 RID: 2416
		[Serializable]
		public class HolderPoint
		{
			// Token: 0x0600438E RID: 17294 RVA: 0x0018E9CC File Offset: 0x0018CBCC
			public HolderPoint(Transform t, string s)
			{
				this.anchor = t;
				this.anchorName = s;
			}

			// Token: 0x04004479 RID: 17529
			public Transform anchor;

			// Token: 0x0400447A RID: 17530
			public string anchorName;
		}
	}
}
