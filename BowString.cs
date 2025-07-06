using System;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002AC RID: 684
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/BowString.html")]
	public class BowString : ThunderBehaviour
	{
		// Token: 0x170001F1 RID: 497
		// (get) Token: 0x06001FB6 RID: 8118 RVA: 0x000D7C20 File Offset: 0x000D5E20
		// (set) Token: 0x06001FB7 RID: 8119 RVA: 0x000D7C28 File Offset: 0x000D5E28
		public Item item { get; protected set; }

		// Token: 0x170001F2 RID: 498
		// (get) Token: 0x06001FB8 RID: 8120 RVA: 0x000D7C31 File Offset: 0x000D5E31
		// (set) Token: 0x06001FB9 RID: 8121 RVA: 0x000D7C39 File Offset: 0x000D5E39
		public PhysicBody pb { get; protected set; }

		// Token: 0x170001F3 RID: 499
		// (get) Token: 0x06001FBA RID: 8122 RVA: 0x000D7C42 File Offset: 0x000D5E42
		// (set) Token: 0x06001FBB RID: 8123 RVA: 0x000D7C4A File Offset: 0x000D5E4A
		public ConfigurableJoint stringJoint { get; protected set; }

		// Token: 0x170001F4 RID: 500
		// (get) Token: 0x06001FBC RID: 8124 RVA: 0x000D7C53 File Offset: 0x000D5E53
		// (set) Token: 0x06001FBD RID: 8125 RVA: 0x000D7C5B File Offset: 0x000D5E5B
		public Handle stringHandle { get; protected set; }

		// Token: 0x170001F5 RID: 501
		// (get) Token: 0x06001FBE RID: 8126 RVA: 0x000D7C64 File Offset: 0x000D5E64
		// (set) Token: 0x06001FBF RID: 8127 RVA: 0x000D7C6C File Offset: 0x000D5E6C
		public Vector3 orgBowStringPos { get; protected set; }

		// Token: 0x170001F6 RID: 502
		// (get) Token: 0x06001FC0 RID: 8128 RVA: 0x000D7C75 File Offset: 0x000D5E75
		// (set) Token: 0x06001FC1 RID: 8129 RVA: 0x000D7C7D File Offset: 0x000D5E7D
		public ItemModuleBow module { get; protected set; }

		// Token: 0x06001FC2 RID: 8130 RVA: 0x000D7C88 File Offset: 0x000D5E88
		private void TryAssignReferences()
		{
			if (this.item == null)
			{
				this.item = base.GetComponentInParent<Item>();
			}
			if (this.pb == null)
			{
				this.pb = base.gameObject.GetPhysicBody();
			}
			if (this.pb == null)
			{
				this.pb = base.gameObject.AddComponent<Rigidbody>().AsPhysicBody();
			}
			if (this.stringJoint == null)
			{
				this.stringJoint = base.GetComponent<ConfigurableJoint>();
			}
			if (this.stringJoint == null)
			{
				this.stringJoint = base.gameObject.AddComponent<ConfigurableJoint>();
			}
			if (this.stringHandle == null)
			{
				this.stringHandle = (base.GetComponent<Handle>() ?? base.GetComponentInChildren<Handle>());
			}
			if (this.stringHandle == null)
			{
				Debug.LogError("Could not assign Handle reference! Make sure that this BowString component (" + base.gameObject.name + ") has a Handle on it, or has a Handle as a child object!");
			}
		}

		// Token: 0x06001FC3 RID: 8131 RVA: 0x000D7D7C File Offset: 0x000D5F7C
		private void JointSetup(bool init, float allowance = 0f)
		{
			if (init && (!this.setupFinished || !Application.isPlaying))
			{
				this.SetStringTargetRatio(0f);
				this.orgBowStringPos = this.pb.transform.localPosition;
				ItemModuleBow module = this.module;
				this.SetStringSpring((module != null) ? module.stringSpring : 500f);
			}
			this.stringJoint.SetConnectedPhysicBody(this.item.gameObject.GetPhysicBody());
			this.stringJoint.autoConfigureConnectedAnchor = false;
			this.stringJoint.configuredInWorldSpace = false;
			this.stringJoint.anchor = Vector3.zero;
			this.stringJoint.linearLimit = new SoftJointLimit
			{
				limit = 0.5f * (this.stringDrawLength + allowance),
				contactDistance = 0.01f
			};
			this.stringJoint.xMotion = ConfigurableJointMotion.Locked;
			this.stringJoint.yMotion = ConfigurableJointMotion.Locked;
			this.stringJoint.zMotion = ConfigurableJointMotion.Limited;
			this.stringJoint.angularXMotion = ConfigurableJointMotion.Locked;
			this.stringJoint.angularYMotion = ConfigurableJointMotion.Locked;
			this.stringJoint.angularZMotion = ConfigurableJointMotion.Locked;
			this.stringJoint.connectedAnchor = this.orgBowStringPos - new Vector3(0f, 0f, 0.5f * this.stringDrawLength + 0.5f * allowance);
			this.setupFinished = true;
		}

		// Token: 0x06001FC4 RID: 8132 RVA: 0x000D7ED8 File Offset: 0x000D60D8
		public void SetStringTargetRatio(float targetRatio)
		{
			this.currentTargetRatio = targetRatio;
			this.stringJoint.targetPosition = new Vector3(0f, 0f, -0.5f * this.stringDrawLength) + new Vector3(0f, 0f, targetRatio * this.stringDrawLength);
		}

		// Token: 0x06001FC5 RID: 8133 RVA: 0x000D7F30 File Offset: 0x000D6130
		public void SetStringSpring(float spring)
		{
			JointDrive jointDrive = this.stringJoint.zDrive;
			jointDrive.positionSpring = spring;
			this.stringJoint.zDrive = jointDrive;
		}

		// Token: 0x170001F7 RID: 503
		// (get) Token: 0x06001FC6 RID: 8134 RVA: 0x000D7F5D File Offset: 0x000D615D
		// (set) Token: 0x06001FC7 RID: 8135 RVA: 0x000D7F6C File Offset: 0x000D616C
		public bool blockStringRelease
		{
			get
			{
				bool blockStringRelease = this._blockStringRelease;
				this._blockStringRelease = false;
				return blockStringRelease;
			}
			set
			{
				this._blockStringRelease = value;
			}
		}

		// Token: 0x06001FC8 RID: 8136 RVA: 0x000D7F75 File Offset: 0x000D6175
		public void SetMinFireVelocity(float fireVelocity)
		{
			this.minFireVelocity = fireVelocity;
		}

		// Token: 0x06001FC9 RID: 8137 RVA: 0x000D7F7E File Offset: 0x000D617E
		public void ReleaseString()
		{
			this.TryReleaseString();
		}

		// Token: 0x06001FCA RID: 8138 RVA: 0x000D7F88 File Offset: 0x000D6188
		public bool TryReleaseString()
		{
			if (this.stringHandle.handlers.Count > 0)
			{
				return false;
			}
			if (this.currentTargetRatio > 0f)
			{
				this.SetStringTargetRatio(0f);
			}
			this.SetLimitBounciness(0.33f);
			this.releasePullRatio = this.currentPullRatio;
			if (this.loadedArrow)
			{
				this.SetBowHandleDrives((this.currentPullRatio > 0f) ? Vector2.zero : Vector2.one, null);
				this.ResetStringHandleDrive();
				if (this.audioContainerShoot != null)
				{
					this.audioSourceShoot.PlayOneShot(this.audioContainerShoot.PickAudioClip(), Mathf.InverseLerp(this.module.audioShootMinPull, 1f, this.currentPullRatio));
				}
				this.maxArrowVelocity = Vector3.zero;
				this.lastStringLocal = new Vector3?(this.item.transform.InverseTransformPoint(base.transform.position));
				this.stringReleaseLocal = new Vector3?(this.item.transform.InverseTransformPoint(base.transform.position));
				this.arrowShootDirection = this.loadedArrow.transform.forward;
				this.loadedArrow.mainCollisionHandler.OnCollisionStartEvent += this.ArrowHitObject;
			}
			BowString.StringReleaseDelegate stringReleaseDelegate = this.onStringReleased;
			if (stringReleaseDelegate != null)
			{
				stringReleaseDelegate(this.currentPullRatio, this.loadedArrow);
			}
			return true;
		}

		// Token: 0x06001FCB RID: 8139 RVA: 0x000D8100 File Offset: 0x000D6300
		public void SpawnAndAttachArrow(string arrowID)
		{
			if (this.loadedArrow != null)
			{
				return;
			}
			ItemData projectileData = Catalog.GetData<ItemData>(arrowID, true);
			if (projectileData != null)
			{
				projectileData.SpawnAsync(delegate(Item projectile)
				{
					this.NockArrow(projectile.GetMainHandle(Side.Right), null, true, null);
				}, null, null, null, true, null, Item.Owner.None);
			}
		}

		// Token: 0x06001FCC RID: 8140 RVA: 0x000D8150 File Offset: 0x000D6350
		public void RemoveArrow(bool despawn)
		{
			if (this.loadedArrow == null)
			{
				return;
			}
			Item prev = this.loadedArrow;
			this.DetachArrow(BowString.DetachType.Both);
			if (despawn)
			{
				prev.Despawn();
			}
		}

		// Token: 0x170001F8 RID: 504
		// (get) Token: 0x06001FCD RID: 8141 RVA: 0x000D8183 File Offset: 0x000D6383
		// (set) Token: 0x06001FCE RID: 8142 RVA: 0x000D818B File Offset: 0x000D638B
		public bool isPulling { get; protected set; }

		// Token: 0x170001F9 RID: 505
		// (get) Token: 0x06001FCF RID: 8143 RVA: 0x000D8194 File Offset: 0x000D6394
		// (set) Token: 0x06001FD0 RID: 8144 RVA: 0x000D819C File Offset: 0x000D639C
		public float pullDistance { get; protected set; }

		// Token: 0x170001FA RID: 506
		// (get) Token: 0x06001FD1 RID: 8145 RVA: 0x000D81A5 File Offset: 0x000D63A5
		// (set) Token: 0x06001FD2 RID: 8146 RVA: 0x000D81AD File Offset: 0x000D63AD
		public float currentPullRatio { get; protected set; }

		// Token: 0x170001FB RID: 507
		// (get) Token: 0x06001FD3 RID: 8147 RVA: 0x000D81B6 File Offset: 0x000D63B6
		// (set) Token: 0x06001FD4 RID: 8148 RVA: 0x000D81BE File Offset: 0x000D63BE
		public float releasePullRatio { get; protected set; }

		// Token: 0x170001FC RID: 508
		// (get) Token: 0x06001FD5 RID: 8149 RVA: 0x000D81C7 File Offset: 0x000D63C7
		// (set) Token: 0x06001FD6 RID: 8150 RVA: 0x000D81CF File Offset: 0x000D63CF
		public Item loadedArrow { get; protected set; }

		// Token: 0x170001FD RID: 509
		// (get) Token: 0x06001FD7 RID: 8151 RVA: 0x000D81D8 File Offset: 0x000D63D8
		// (set) Token: 0x06001FD8 RID: 8152 RVA: 0x000D81E0 File Offset: 0x000D63E0
		public BowString.ArrowState arrowState { get; protected set; }

		// Token: 0x170001FE RID: 510
		// (get) Token: 0x06001FD9 RID: 8153 RVA: 0x000D81E9 File Offset: 0x000D63E9
		// (set) Token: 0x06001FDA RID: 8154 RVA: 0x000D81F1 File Offset: 0x000D63F1
		public ConfigurableJoint nockJoint { get; protected set; }

		// Token: 0x170001FF RID: 511
		// (get) Token: 0x06001FDB RID: 8155 RVA: 0x000D81FA File Offset: 0x000D63FA
		// (set) Token: 0x06001FDC RID: 8156 RVA: 0x000D8202 File Offset: 0x000D6402
		public ConfigurableJoint restJoint { get; protected set; }

		// Token: 0x140000EC RID: 236
		// (add) Token: 0x06001FDD RID: 8157 RVA: 0x000D820C File Offset: 0x000D640C
		// (remove) Token: 0x06001FDE RID: 8158 RVA: 0x000D8244 File Offset: 0x000D6444
		public event BowString.GrabNockEvent onArrowAdded;

		// Token: 0x140000ED RID: 237
		// (add) Token: 0x06001FDF RID: 8159 RVA: 0x000D827C File Offset: 0x000D647C
		// (remove) Token: 0x06001FE0 RID: 8160 RVA: 0x000D82B4 File Offset: 0x000D64B4
		public event BowString.GrabNockEvent onStringGrabbed;

		// Token: 0x140000EE RID: 238
		// (add) Token: 0x06001FE1 RID: 8161 RVA: 0x000D82EC File Offset: 0x000D64EC
		// (remove) Token: 0x06001FE2 RID: 8162 RVA: 0x000D8324 File Offset: 0x000D6524
		public event BowString.GrabNockEvent onStringUngrabbed;

		// Token: 0x140000EF RID: 239
		// (add) Token: 0x06001FE3 RID: 8163 RVA: 0x000D835C File Offset: 0x000D655C
		// (remove) Token: 0x06001FE4 RID: 8164 RVA: 0x000D8394 File Offset: 0x000D6594
		public event BowString.StringReleaseDelegate onStringReleased;

		// Token: 0x140000F0 RID: 240
		// (add) Token: 0x06001FE5 RID: 8165 RVA: 0x000D83CC File Offset: 0x000D65CC
		// (remove) Token: 0x06001FE6 RID: 8166 RVA: 0x000D8404 File Offset: 0x000D6604
		public event BowString.StringReleaseDelegate onStringSnap;

		// Token: 0x140000F1 RID: 241
		// (add) Token: 0x06001FE7 RID: 8167 RVA: 0x000D843C File Offset: 0x000D663C
		// (remove) Token: 0x06001FE8 RID: 8168 RVA: 0x000D8474 File Offset: 0x000D6674
		public event BowString.UnnockingEvent onArrowRemoved;

		// Token: 0x06001FE9 RID: 8169 RVA: 0x000D84AC File Offset: 0x000D66AC
		protected void Awake()
		{
			if (this.animation && this.animation.clip)
			{
				this.clipName = this.animation.clip.name;
			}
			this.SetNormalizedTime(0f);
			this.TryAssignReferences();
			this.item.OnDataLoaded += this.OnItemDataLoaded;
			this.item.mainHandleLeft.Grabbed += this.OnBowGrabbed;
			this.item.mainHandleRight.Grabbed += this.OnBowGrabbed;
			this.item.mainHandleLeft.UnGrabbed += this.OnBowUngrabbed;
			this.item.mainHandleRight.UnGrabbed += this.OnBowUngrabbed;
			this.item.OnSnapEvent += this.OnBowSnapped;
			this.stringHandle.Grabbed += this.OnStringGrab;
			this.stringHandle.UnGrabbed += this.OnStringUnGrab;
			this.pb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			this.orgBowStringPos = this.pb.transform.localPosition;
			this.audioSourceString = this.AddAndConfigureAudioSource();
			if (this.audioClipString != null)
			{
				this.audioSourceString.clip = this.audioClipString;
			}
			this.audioSourceShoot = this.AddAndConfigureAudioSource();
		}

		// Token: 0x06001FEA RID: 8170 RVA: 0x000D8624 File Offset: 0x000D6824
		private void Start()
		{
			this.JointSetup(true, 0f);
		}

		// Token: 0x06001FEB RID: 8171 RVA: 0x000D8634 File Offset: 0x000D6834
		private AudioSource AddAndConfigureAudioSource()
		{
			AudioSource newSource = base.gameObject.AddComponent<AudioSource>();
			newSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(AudioMixerName.Effect);
			newSource.playOnAwake = false;
			if (AudioSettings.GetSpatializerPluginName() != null)
			{
				newSource.spatialize = true;
			}
			newSource.spatialBlend = 1f;
			newSource.dopplerLevel = 0f;
			return newSource;
		}

		// Token: 0x06001FEC RID: 8172 RVA: 0x000D8685 File Offset: 0x000D6885
		private void OnItemDataLoaded()
		{
			this.module = this.item.data.GetModule<ItemModuleBow>();
			this.ResetStringSpring();
			if (!this.stringAlwaysGrabbable)
			{
				this.stringHandle.SetTouchPersistent(false);
			}
		}

		// Token: 0x06001FED RID: 8173 RVA: 0x000D86B8 File Offset: 0x000D68B8
		private void OnStringGrab(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				if (!this.loadedArrow && this.module.spawnArrow)
				{
					this.SpawnAndAttachArrow(this.module.arrowProjectileID);
				}
				this.SetLimitBounciness(0f);
				this.SetStringSpring(this.module.stringSpring);
				Item loadedArrow = this.loadedArrow;
				if (loadedArrow != null)
				{
					loadedArrow.IgnoreRagdollCollision(ragdollHand.ragdoll);
				}
				BowString.GrabNockEvent grabNockEvent = this.onStringGrabbed;
				if (grabNockEvent == null)
				{
					return;
				}
				grabNockEvent(this.loadedArrow);
			}
		}

		// Token: 0x06001FEE RID: 8174 RVA: 0x000D8740 File Offset: 0x000D6940
		private void OnStringUnGrab(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				BowString.GrabNockEvent grabNockEvent = this.onStringUngrabbed;
				if (grabNockEvent != null)
				{
					grabNockEvent(this.loadedArrow);
				}
				object obj = this.currentPullRatio > this.minPull && !this.blockStringRelease;
				if (this.loadedArrow)
				{
					this.loadedArrow.lastHandler = ragdollHand;
					if (this.arrowState == BowString.ArrowState.Nocked)
					{
						this.DetachArrow(BowString.DetachType.Unnock);
					}
				}
				object obj2 = obj;
				if (obj2 != null && this.TryReleaseString())
				{
					EventManager.InvokeBowRelease(ragdollHand.creature, this);
				}
				if (obj2 == null || this.loadedArrow == null)
				{
					this.ResetBowHandleDrives();
				}
			}
		}

		// Token: 0x06001FEF RID: 8175 RVA: 0x000D87DC File Offset: 0x000D69DC
		private void OnTriggerEnter(Collider collider)
		{
			if (!this.loadedArrow)
			{
				Handle grabbedHandle = Player.currentCreature.handRight.grabbedHandle;
				if (!(((grabbedHandle != null) ? grabbedHandle.item : null) == this.item))
				{
					Handle grabbedHandle2 = Player.currentCreature.handLeft.grabbedHandle;
					if (!(((grabbedHandle2 != null) ? grabbedHandle2.item : null) == this.item))
					{
						return;
					}
				}
				Handle handle;
				if (collider.TryGetComponent<Handle>(out handle) && handle.handlers.Count > 0 && handle.item && handle.item.data.slot == this.ammoCategory)
				{
					Handle grabbedHandle3 = Player.currentCreature.handRight.grabbedHandle;
					if (!(((grabbedHandle3 != null) ? grabbedHandle3.item : null) == handle.item))
					{
						Handle grabbedHandle4 = Player.currentCreature.handLeft.grabbedHandle;
						if (!(((grabbedHandle4 != null) ? grabbedHandle4.item : null) == handle.item))
						{
							return;
						}
					}
					Handle mainHandle = handle;
					if (handle != handle.item.mainHandleLeft && handle != handle.item.mainHandleRight)
					{
						mainHandle = handle.item.GetMainHandle(handle.handlers[0].side);
					}
					this.NockArrow(mainHandle, null, false, null);
				}
			}
		}

		// Token: 0x06001FF0 RID: 8176 RVA: 0x000D8941 File Offset: 0x000D6B41
		public void ResetStringSpring()
		{
			this.SetStringSpring(this.module.stringSpring);
		}

		// Token: 0x06001FF1 RID: 8177 RVA: 0x000D8954 File Offset: 0x000D6B54
		public void SetLimitBounciness(float bounciness)
		{
			SoftJointLimit limit = this.stringJoint.linearLimit;
			limit.bounciness = bounciness;
			this.stringJoint.linearLimit = limit;
		}

		// Token: 0x06001FF2 RID: 8178 RVA: 0x000D8981 File Offset: 0x000D6B81
		public void SetJointMotion(ConfigurableJointMotion configurableJointMotion)
		{
			this.stringJoint.xMotion = configurableJointMotion;
			this.stringJoint.yMotion = configurableJointMotion;
			this.stringJoint.angularXMotion = configurableJointMotion;
			this.stringJoint.angularYMotion = configurableJointMotion;
			this.stringJoint.angularZMotion = configurableJointMotion;
		}

		// Token: 0x06001FF3 RID: 8179 RVA: 0x000D89C0 File Offset: 0x000D6BC0
		protected void SetBowHandleDrives(Vector2 rotationStrength, Vector2? positionStrength = null)
		{
			if (positionStrength == null)
			{
				positionStrength = new Vector2?(Vector2.one);
			}
			foreach (Handle handle in this.item.handles)
			{
				handle.SetJointDrive(positionStrength.Value, rotationStrength);
			}
		}

		// Token: 0x06001FF4 RID: 8180 RVA: 0x000D8A34 File Offset: 0x000D6C34
		protected void ResetBowHandleDrives()
		{
			foreach (Handle handle in this.item.handles)
			{
				handle.RefreshJointDrive();
			}
		}

		// Token: 0x06001FF5 RID: 8181 RVA: 0x000D8A8C File Offset: 0x000D6C8C
		protected void SetStringHandleDrive(Vector2 positionStrength)
		{
			this.stringHandle.SetJointDrive(positionStrength, Vector2.zero);
		}

		// Token: 0x06001FF6 RID: 8182 RVA: 0x000D8A9F File Offset: 0x000D6C9F
		protected void ResetStringHandleDrive()
		{
			this.stringHandle.RefreshJointDrive();
		}

		// Token: 0x17000200 RID: 512
		// (get) Token: 0x06001FF7 RID: 8183 RVA: 0x000D8AAC File Offset: 0x000D6CAC
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate | ManagedLoops.Update;
			}
		}

		// Token: 0x06001FF8 RID: 8184 RVA: 0x000D8AB0 File Offset: 0x000D6CB0
		protected internal override void ManagedUpdate()
		{
			if (!this.stringHandle || this.item.holder)
			{
				return;
			}
			float dist = this.orgBowStringPos.z - base.transform.localPosition.z;
			if (dist > this.minPull)
			{
				this.currentPullRatio = dist / this.stringDrawLength;
				this.SetNormalizedTime(this.currentPullRatio);
				if (this.stringHandle.handlers.Count > 0 || this.releasePullRatio > this.minPull)
				{
					if (Mathf.Abs(this.currentPullRatio - this.stepPull) > Catalog.gameData.haptics.bowDrawPeriod)
					{
						float forceMult = this.currentPullRatio;
						this.audioSourceString.volume = 0.1f + forceMult;
						this.audioSourceString.pitch = 1f + forceMult / 5f;
						if (!this.audioSourceString.isPlaying && this.audioClipString != null)
						{
							this.audioSourceString.Play();
						}
						if (this.stringHandle.handlers.Count > 0 && this.stringHandle.handlers[0].playerHand)
						{
							PlayerControl.GetHand(this.stringHandle.handlers[0].playerHand.side).HapticShort(Catalog.gameData.haptics.bowDrawIntensity, false);
						}
						if (this.item.mainHandleRight.handlers.Count > 0 && this.item.mainHandleRight.handlers[0].playerHand)
						{
							PlayerControl.handRight.HapticShort(Catalog.gameData.haptics.bowDrawIntensity * 0.5f, false);
						}
						else if (this.item.mainHandleLeft.handlers.Count > 0 && this.item.mainHandleLeft.handlers[0].playerHand)
						{
							PlayerControl.handLeft.HapticShort(Catalog.gameData.haptics.bowDrawIntensity * 0.5f, false);
						}
						if (this.audioClipString != null)
						{
							this.audioSourceString.PlayOneShot(this.audioSourceString.clip);
						}
						this.stepPull = this.currentPullRatio;
					}
					if (this.loadedArrow && this.arrowState == (BowString.ArrowState.Nocked | BowString.ArrowState.Rested) && !this.drawAudioPlayed && this.currentPullRatio - this.previousPull > this.module.audioDrawPullSpeed)
					{
						if (this.audioContainerDraw != null)
						{
							this.audioSourceShoot.PlayOneShot(this.audioContainerDraw.PickAudioClip(), 1f);
						}
						this.drawAudioPlayed = true;
					}
					this.previousPull = this.currentPullRatio;
					if (!this.isPulling)
					{
						this.OnPullStart();
						this.isPulling = true;
						return;
					}
				}
			}
			else
			{
				this.drawAudioPlayed = false;
				this.currentPullRatio = 0f;
				this.stepPull = 0f;
				this.SetNormalizedTime(0f);
				if (this.isPulling)
				{
					this.OnPullEnd();
					this.isPulling = false;
				}
				this.releasePullRatio = 0f;
			}
		}

		// Token: 0x06001FF9 RID: 8185 RVA: 0x000D8DE0 File Offset: 0x000D6FE0
		protected void FixStringBouncy(float deltaTime)
		{
			if (Player.currentCreature != null)
			{
				this.stringCorrectionVelocity = Vector3.zero;
				RagdollHand hand = Player.currentCreature.GetHand(Side.Right);
				if (!(((hand != null) ? hand.grabbedHandle : null) == this.stringHandle))
				{
					RagdollHand hand2 = Player.currentCreature.GetHand(Side.Left);
					if (!(((hand2 != null) ? hand2.grabbedHandle : null) == this.stringHandle))
					{
						goto IL_AE;
					}
				}
				if (Player.currentCreature.currentLocomotion.isGrounded)
				{
					this.stringCorrectionVelocity = base.transform.forward * Vector3.Dot(base.transform.forward, Player.currentCreature.currentLocomotion.physicBody.velocity);
				}
				IL_AE:
				base.transform.position += this.stringCorrectionVelocity * deltaTime;
			}
		}

		// Token: 0x06001FFA RID: 8186 RVA: 0x000D8EC0 File Offset: 0x000D70C0
		protected internal override void ManagedFixedUpdate()
		{
			if ((this.loadedArrow == null || (this.loadedArrow != null && this.restJoint == null && this.nockJoint == null)) && this.checkJointsOnNull)
			{
				bool destroyedMissedJoint = false;
				ConfigurableJoint[] joints = base.GetComponents<ConfigurableJoint>();
				if (joints.Length > 1)
				{
					for (int i = joints.Length - 1; i >= 0; i--)
					{
						if (joints[i] != this.stringJoint)
						{
							for (int j = joints[i].GetConnectedPhysicBody().gameObject.GetComponents<ConfigurableJoint>().Length - 1; j >= 0; j--)
							{
								PhysicBody otherPB = joints[i].GetConnectedPhysicBody();
								if (otherPB == this.pb || otherPB == this.item.physicBody)
								{
									UnityEngine.Object.Destroy(joints[j]);
								}
							}
							UnityEngine.Object.Destroy(joints[i]);
							destroyedMissedJoint = true;
						}
					}
				}
				this.arrowState = BowString.ArrowState.None;
				if (this.loadedArrow)
				{
					this.loadedArrow.OnGrabEvent -= this.OnNockedArrowGrabbed;
					this.loadedArrow.OnTelekinesisGrabEvent -= this.OnNockedArrowTeleGrabbed;
					this.loadedArrow.OnDespawnEvent -= this.OnNockedArrowDespawn;
				}
				this.SetBowHandleDrives((this.currentPullRatio > 0f) ? Vector2.zero : Vector2.one, null);
				this.loadedArrow = null;
				this.checkJointsOnNull = false;
				if (destroyedMissedJoint)
				{
					Debug.LogWarning("Bowstring on bow [ " + this.item.name + " ] had an error with detaching arrow; The joints have been removed, loaded arrow cleared, and string reset");
				}
			}
			if (Player.currentCreature != null)
			{
				Player local = Player.local;
				UnityEngine.Object x;
				if (local == null)
				{
					x = null;
				}
				else
				{
					PlayerHand hand = local.GetHand(Side.Right);
					if (hand == null)
					{
						x = null;
					}
					else
					{
						RagdollHand ragdollHand = hand.ragdollHand;
						x = ((ragdollHand != null) ? ragdollHand.grabbedHandle : null);
					}
				}
				if (!(x == this.stringHandle))
				{
					Player local2 = Player.local;
					UnityEngine.Object x2;
					if (local2 == null)
					{
						x2 = null;
					}
					else
					{
						PlayerHand hand2 = local2.GetHand(Side.Left);
						if (hand2 == null)
						{
							x2 = null;
						}
						else
						{
							RagdollHand ragdollHand2 = hand2.ragdollHand;
							x2 = ((ragdollHand2 != null) ? ragdollHand2.grabbedHandle : null);
						}
					}
					if (!(x2 == this.stringHandle))
					{
						goto IL_249;
					}
				}
				this.SetStringHandleDrive(new Vector2(Mathf.Clamp(1f - this.pullDifficultyByDraw.Evaluate(this.currentPullRatio), 1E-05f, 1f), 1f));
			}
			IL_249:
			bool isShooting = this.lastStringLocal != null;
			if (this.stringHandle.handlers.Count == 0)
			{
				if (!isShooting)
				{
					this.ResetBowHandleDrives();
				}
				else
				{
					this.SetBowHandleDrives(Vector2.zero, new Vector2?((1f - this.currentPullRatio) * Vector2.one));
				}
			}
			this.FixStringBouncy(Time.fixedDeltaTime);
			if (this.loadedArrow)
			{
				if (this.loadedArrow.isPenetrating)
				{
					this.DetachArrow(BowString.DetachType.Both);
					return;
				}
				if (this.stringHandle.handlers.Count == 0 && isShooting)
				{
					this.maxArrowVelocity = ((this.loadedArrow.physicBody.velocity.sqrMagnitude >= this.maxArrowVelocity.sqrMagnitude) ? this.loadedArrow.physicBody.velocity : this.maxArrowVelocity);
					if (this.currentPullRatio > this.minPull)
					{
						this.item.transform.RotateAroundPivot(this.currentRest.position, Quaternion.FromToRotation(this.GetCurrentStringRestDirection(), this.GetPreviousStringRestDirection()));
						this.lastStringLocal = new Vector3?(this.item.transform.InverseTransformPoint(base.transform.position));
					}
				}
				if (this.arrowState == BowString.ArrowState.Rested)
				{
					float endRestZ = this.GetZFromRestToArrowTransform(this.loadedArrow.mainHandleRight.transform);
					if (this.arrowUnnockForward != null)
					{
						this.loadedArrow.transform.RotateAroundPivot(this.currentRest.position, Quaternion.FromToRotation(this.loadedArrow.transform.forward, Vector3.Slerp(this.arrowShootDirection, this.arrowUnnockForward.Value, endRestZ / this.arrowUnnockZ)));
					}
					if (endRestZ >= 0f)
					{
						this.DetachArrow(BowString.DetachType.Fire);
						return;
					}
				}
				else if (this.GetZFromRestToArrowTransform(this.loadedArrow.GetDefaultHolderPoint().anchor) <= 0f && this.restJoint != null && this.allowOverdraw)
				{
					this.DetachArrow(BowString.DetachType.Unrest);
				}
			}
		}

		// Token: 0x06001FFB RID: 8187 RVA: 0x000D9320 File Offset: 0x000D7520
		protected Vector3 GetCurrentStringRestDirection()
		{
			return (this.currentRest.position - base.transform.position).normalized;
		}

		// Token: 0x06001FFC RID: 8188 RVA: 0x000D9350 File Offset: 0x000D7550
		protected Vector3 GetPreviousStringRestDirection()
		{
			if (this.lastStringLocal != null)
			{
				return (this.currentRest.position - this.item.transform.TransformPoint(this.lastStringLocal.Value)).normalized;
			}
			return this.loadedArrow.flyDirRef.forward;
		}

		// Token: 0x06001FFD RID: 8189 RVA: 0x000D93AE File Offset: 0x000D75AE
		public float GetZFromRestToArrowTransform(Transform transform)
		{
			return this.loadedArrow.transform.InverseTransformPoint(transform.position).z - this.loadedArrow.transform.InverseTransformPoint(this.currentRest.position).z;
		}

		// Token: 0x06001FFE RID: 8190 RVA: 0x000D93EC File Offset: 0x000D75EC
		public void SpawnAndAttachArrow(string arrowID, Side? forceSide = null)
		{
			if (this.loadedArrow != null)
			{
				return;
			}
			ItemData projectileData = Catalog.GetData<ItemData>(arrowID, true);
			if (projectileData != null)
			{
				projectileData.SpawnAsync(delegate(Item projectile)
				{
					this.NockArrow(projectile.GetMainHandle(Side.Right), null, true, forceSide);
				}, null, null, null, true, null, Item.Owner.None);
			}
		}

		// Token: 0x06001FFF RID: 8191 RVA: 0x000D9450 File Offset: 0x000D7650
		public void NockArrow(Handle arrowHandle, HandlePose handleOrientation = null, bool ignoreHandling = false, Side? forceSide = null)
		{
			if (arrowHandle != arrowHandle.item.mainHandleRight && arrowHandle != arrowHandle.item.mainHandleLeft)
			{
				return;
			}
			if (!ignoreHandling && (!arrowHandle.item.IsHanded() || (this.nockOnlyMainHandle && !arrowHandle.IsHanded())))
			{
				return;
			}
			arrowHandle.SetTouch(false);
			RagdollHand bowragdollHand = this.item.IsHanded() ? this.item.mainHandler : null;
			if (this.item.mainHandleRight.IsHanded())
			{
				bowragdollHand = this.item.mainHandleRight.handlers[0];
			}
			else if (this.item.mainHandleLeft.IsHanded())
			{
				bowragdollHand = this.item.mainHandleLeft.handlers[0];
			}
			this.currentRest = ((this.restRight.position.y > this.restLeft.position.y) ? this.restRight : this.restLeft);
			if (bowragdollHand != null)
			{
				this.currentRest = ((bowragdollHand.side == Side.Right) ? this.restRight : this.restLeft);
				if (Vector3.Dot(bowragdollHand.gripInfo.orientation.transform.up, bowragdollHand.grabbedHandle.transform.up) < 0f)
				{
					this.currentRest = ((bowragdollHand.side == Side.Right) ? this.restLeft : this.restRight);
				}
			}
			if (forceSide != null)
			{
				Side? side = forceSide;
				Side side2 = Side.Right;
				this.currentRest = ((side.GetValueOrDefault() == side2 & side != null) ? this.restRight : this.restLeft);
			}
			this.loadedArrow = arrowHandle.item;
			this.loadedArrow.transform.MoveAlign(arrowHandle.transform, base.transform, null);
			this.loadedArrow.transform.rotation = Quaternion.LookRotation((this.currentRest.position - base.transform.position).normalized, base.transform.up) * Quaternion.FromToRotation(this.loadedArrow.transform.forward, arrowHandle.transform.forward);
			this.loadedArrow.DisallowDespawn = true;
			this.loadedArrow.OnGrabEvent += this.OnNockedArrowGrabbed;
			this.loadedArrow.OnTelekinesisGrabEvent += this.OnNockedArrowTeleGrabbed;
			this.nockJoint = this.pb.gameObject.AddComponent<ConfigurableJoint>();
			this.nockJoint.configuredInWorldSpace = false;
			this.nockJoint.autoConfigureConnectedAnchor = false;
			this.nockJoint.SetConnectedPhysicBody(this.loadedArrow.physicBody);
			this.nockJoint.connectedAnchor = arrowHandle.transform.localPosition;
			this.nockJoint.xMotion = ConfigurableJointMotion.Locked;
			this.nockJoint.yMotion = ConfigurableJointMotion.Locked;
			this.nockJoint.zMotion = ConfigurableJointMotion.Locked;
			this.nockJoint.angularZMotion = ConfigurableJointMotion.Locked;
			this.restJoint = this.loadedArrow.physicBody.gameObject.AddComponent<ConfigurableJoint>();
			this.restJoint.SetConnectedPhysicBody(this.item.physicBody);
			this.restJoint.anchor = Vector3.zero;
			this.restJoint.autoConfigureConnectedAnchor = false;
			this.restJoint.connectedAnchor = this.currentRest.localPosition;
			this.restJoint.xMotion = ConfigurableJointMotion.Locked;
			this.restJoint.yMotion = ConfigurableJointMotion.Locked;
			RagdollHand stringRagdollHand = null;
			if (arrowHandle.handlers.Count > 0)
			{
				stringRagdollHand = arrowHandle.handlers[0];
			}
			bool withTrigger = stringRagdollHand != null && stringRagdollHand.grabbedWithTrigger;
			for (int i = this.loadedArrow.handlers.Count - 1; i >= 0; i--)
			{
				this.loadedArrow.handlers[i].UnGrab(false);
			}
			this.loadedArrow.IgnoreObjectCollision(this.item);
			this.loadedArrow.SetColliderAndMeshLayer(GameManager.GetLayer(LayerName.MovingItem), false);
			this.loadedArrow.physicBody.sleepThreshold = 0f;
			if (handleOrientation == null)
			{
				if (stringRagdollHand != null)
				{
					stringRagdollHand.GrabRelative(this.stringHandle, withTrigger);
				}
			}
			else if (stringRagdollHand != null)
			{
				stringRagdollHand.Grab(this.stringHandle, handleOrientation, withTrigger);
			}
			this.arrowState = (BowString.ArrowState.Nocked | BowString.ArrowState.Rested);
			this.loadedArrow.OnDespawnEvent += this.OnNockedArrowDespawn;
			BowString.GrabNockEvent grabNockEvent = this.onArrowAdded;
			if (grabNockEvent != null)
			{
				grabNockEvent(this.loadedArrow);
			}
			this.checkJointsOnNull = true;
		}

		// Token: 0x06002000 RID: 8192 RVA: 0x000D98D8 File Offset: 0x000D7AD8
		public void SetNormalizedTime(float time)
		{
			this.animation[this.clipName].speed = 0f;
			this.animation[this.clipName].normalizedTime = this.pullCurve.Evaluate(time);
			this.animation.Play(this.clipName);
		}

		// Token: 0x06002001 RID: 8193 RVA: 0x000D9934 File Offset: 0x000D7B34
		private void OnPullStart()
		{
			if (this.stringHandle.handlers.Count > 0)
			{
				EventManager.InvokeBowDraw(this.stringHandle.handlers[0].creature, this);
			}
		}

		// Token: 0x06002002 RID: 8194 RVA: 0x000D9968 File Offset: 0x000D7B68
		private void OnPullEnd()
		{
			if (!this.stringHandle.IsHanded() && this.releasePullRatio > this.minPull)
			{
				if (this.loadedArrow && (this.maxArrowVelocity - this.item.physicBody.velocity).sqrMagnitude > this.minFireVelocity * this.minFireVelocity)
				{
					float amplitude = Utils.CalculateRatio(this.loadedArrow.physicBody.velocity.magnitude, this.minFireVelocity, 5f * this.module.velocityMultiplier, 0.1f, 1f);
					if (this.item.leftPlayerHand)
					{
						PlayerControl.handLeft.HapticPlayClip(Catalog.gameData.haptics.bowShoot, amplitude);
					}
					else if (this.item.rightPlayerHand)
					{
						PlayerControl.handRight.HapticPlayClip(Catalog.gameData.haptics.bowShoot, amplitude);
					}
					Item loadedArrow = this.loadedArrow;
					this.DetachArrow(BowString.DetachType.Unnock);
					loadedArrow.physicBody.velocity = this.maxArrowVelocity;
					if (this.arrowState == BowString.ArrowState.Rested && this.loadedArrow != null)
					{
						this.arrowUnnockForward = new Vector3?(this.loadedArrow.transform.forward);
						this.arrowUnnockZ = this.GetZFromRestToArrowTransform(this.loadedArrow.mainHandleRight.transform);
					}
				}
				else
				{
					this.lastStringLocal = null;
				}
				this.ResetBowHandleDrives();
				BowString.StringReleaseDelegate stringReleaseDelegate = this.onStringSnap;
				if (stringReleaseDelegate != null)
				{
					stringReleaseDelegate(this.releasePullRatio, this.loadedArrow);
				}
			}
			if (this.stringHandle.handlers.Count > 0)
			{
				EventManager.InvokeBowRelease(this.stringHandle.handlers[0].creature, this);
			}
		}

		// Token: 0x06002003 RID: 8195 RVA: 0x000D9B3F File Offset: 0x000D7D3F
		private void OnBowGrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (this.stringAlwaysGrabbable)
			{
				return;
			}
			if (eventTime == EventTime.OnEnd)
			{
				Item loadedArrow = this.loadedArrow;
				if (loadedArrow != null)
				{
					loadedArrow.IgnoreRagdollCollision(ragdollHand.ragdoll);
				}
				this.stringHandle.SetTouchPersistent(true);
			}
		}

		// Token: 0x06002004 RID: 8196 RVA: 0x000D9B74 File Offset: 0x000D7D74
		private void OnBowUngrabbed(RagdollHand ragdollHand, Handle handle, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				if (this.loseNockOnUngrab)
				{
					this.DetachArrow(BowString.DetachType.Both);
				}
				if (this.stringAlwaysGrabbable)
				{
					return;
				}
				if (ragdollHand.otherHand.grabbedHandle && ragdollHand.otherHand.grabbedHandle == this.stringHandle)
				{
					ragdollHand.otherHand.UnGrab(false);
				}
				this.stringHandle.SetTouchPersistent(false);
			}
		}

		// Token: 0x06002005 RID: 8197 RVA: 0x000D9BDF File Offset: 0x000D7DDF
		private void OnBowSnapped(Holder holder)
		{
			if (this.loseNockOnUngrab)
			{
				this.DetachArrow(BowString.DetachType.Both);
			}
			this.pb.velocity = Vector3.zero;
			this.pb.angularVelocity = Vector3.zero;
		}

		// Token: 0x06002006 RID: 8198 RVA: 0x000D9C10 File Offset: 0x000D7E10
		private void OnNockedArrowGrabbed(Handle handle, RagdollHand ragdollHand)
		{
			this.DetachArrow(BowString.DetachType.Both);
		}

		// Token: 0x06002007 RID: 8199 RVA: 0x000D9C19 File Offset: 0x000D7E19
		private void OnNockedArrowTeleGrabbed(Handle handle, SpellTelekinesis teleGrabber)
		{
			this.DetachArrow(BowString.DetachType.Both);
		}

		// Token: 0x06002008 RID: 8200 RVA: 0x000D9C24 File Offset: 0x000D7E24
		private void ArrowHitObject(CollisionInstance collisionInstance)
		{
			PhysicBody pb = collisionInstance.targetCollider.GetPhysicBody();
			if (pb != null)
			{
				if (pb == this.pb || pb == this.item.physicBody)
				{
					return;
				}
				RagdollPart ragdollPart;
				if (pb.gameObject.TryGetComponent<RagdollPart>(out ragdollPart))
				{
					UnityEngine.Object creature = ragdollPart.ragdoll.creature;
					RagdollHand mainHandler = this.item.mainHandler;
					if (creature == ((mainHandler != null) ? mainHandler.creature : null))
					{
						return;
					}
				}
			}
			this.DetachArrow(BowString.DetachType.Both);
		}

		// Token: 0x06002009 RID: 8201 RVA: 0x000D9CA2 File Offset: 0x000D7EA2
		private void OnNockedArrowDespawn(EventTime eventTime)
		{
			this.DetachArrow(BowString.DetachType.Both);
		}

		// Token: 0x0600200A RID: 8202 RVA: 0x000D9CAC File Offset: 0x000D7EAC
		public void DetachArrow(BowString.DetachType detachType)
		{
			if (this.loadedArrow == null)
			{
				return;
			}
			bool fire = detachType == BowString.DetachType.Fire;
			if (fire)
			{
				detachType = BowString.DetachType.Both;
			}
			if ((detachType == BowString.DetachType.Unnock || detachType == BowString.DetachType.Both) && this.nockJoint != null)
			{
				UnityEngine.Object.Destroy(this.nockJoint);
				this.nockJoint = null;
				this.loadedArrow.mainHandleRight.SetTouch(true);
				this.loadedArrow.mainHandleLeft.SetTouch(true);
			}
			if ((detachType == BowString.DetachType.Unrest || detachType == BowString.DetachType.Both) && this.restJoint != null)
			{
				UnityEngine.Object.Destroy(this.restJoint);
				this.restJoint = null;
			}
			this.arrowState = (BowString.ArrowState.None | ((this.restJoint != null) ? BowString.ArrowState.Rested : BowString.ArrowState.None) | ((this.nockJoint != null) ? BowString.ArrowState.Nocked : BowString.ArrowState.None));
			if (fire)
			{
				if (this.arrowState != BowString.ArrowState.None)
				{
					Debug.LogError("Error on bow " + this.item.name + ": Attempted to fire arrow but the arrow is still partially connected.");
					return;
				}
				Vector3 velocityDirection = this.loadedArrow.flyDirRef.forward;
				float fireVelocity = this.maxArrowVelocity.magnitude;
				this.loadedArrow.physicBody.velocity = velocityDirection * fireVelocity;
				this.loadedArrow.physicBody.angularVelocity = Vector3.zero;
				this.loadedArrow.Throw(this.module.velocityMultiplier, Item.FlyDetection.Forced);
				this.loadedArrow.lastHandler = this.item.lastHandler;
				float amplitude = Utils.CalculateRatio(fireVelocity, this.minFireVelocity, 5f * this.module.velocityMultiplier, 0.1f, 1f);
				if (this.item.leftPlayerHand)
				{
					PlayerControl.handLeft.HapticPlayClip(Catalog.gameData.haptics.bowShoot, amplitude);
				}
				else if (this.item.rightPlayerHand)
				{
					PlayerControl.handRight.HapticPlayClip(Catalog.gameData.haptics.bowShoot, amplitude);
				}
				EventManager.InvokeBowFire(this.item.mainHandler, this, this.loadedArrow);
			}
			if (this.arrowState == BowString.ArrowState.None)
			{
				this.lastStringLocal = null;
				this.stringReleaseLocal = null;
				this.arrowUnnockForward = null;
				this.loadedArrow.DisallowDespawn = false;
				this.loadedArrow.physicBody.sleepThreshold = this.loadedArrow.orgSleepThreshold;
				this.loadedArrow.mainCollisionHandler.OnCollisionStartEvent -= this.ArrowHitObject;
				this.loadedArrow.OnGrabEvent -= this.OnNockedArrowGrabbed;
				this.loadedArrow.OnTelekinesisGrabEvent -= this.OnNockedArrowTeleGrabbed;
				this.ResetBowHandleDrives();
				this.loadedArrow.OnDespawnEvent -= this.OnNockedArrowDespawn;
				BowString.UnnockingEvent unnockingEvent = this.onArrowRemoved;
				if (unnockingEvent != null)
				{
					unnockingEvent(this.loadedArrow, fire);
				}
				this.loadedArrow = null;
			}
		}

		// Token: 0x0600200B RID: 8203 RVA: 0x000D9F8C File Offset: 0x000D818C
		protected override void ManagedOnDisable()
		{
			if (!GameManager.isQuitting)
			{
				this.DetachArrow(BowString.DetachType.Both);
				base.transform.localPosition = this.orgBowStringPos;
				this.pb.velocity = Vector3.zero;
				this.pb.angularVelocity = Vector3.zero;
			}
		}

		// Token: 0x04001EE3 RID: 7907
		[Header("Draw and animation")]
		public Animation animation;

		// Token: 0x04001EE4 RID: 7908
		[Tooltip("This allows you to adjust the animation time so that the pink line matches where your bow is drawn to better.")]
		public AnimationCurve pullCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		// Token: 0x04001EE5 RID: 7909
		[Tooltip("Defines how far your bow string can be pulled (in meters). This gets set automatically by the auto-configure, but can be manually adjusted if you feel it's wrong.")]
		public float stringDrawLength = 0.5f;

		// Token: 0x04001EE6 RID: 7910
		[Tooltip("Set the minimum speed for the bow to fire an arrow.")]
		public float minFireVelocity = 4f;

		// Token: 0x04001EE7 RID: 7911
		[Range(0f, 0.1f)]
		[Tooltip("Defines the minimum distance the handle has to move to register a pull happening.")]
		public float minPull = 0.01f;

		// Token: 0x04001EE8 RID: 7912
		[Tooltip("As the pull difficulty increases, the player's hand will become weaker. Allows you to make it \"tougher\" to achieve full draw.")]
		public AnimationCurve pullDifficultyByDraw = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001EE9 RID: 7913
		[Header("Resting and nocking")]
		public Transform restLeft;

		// Token: 0x04001EEA RID: 7914
		public Transform restRight;

		// Token: 0x04001EEB RID: 7915
		public string ammoCategory = "Arrow";

		// Token: 0x04001EEC RID: 7916
		[Tooltip("Allow the player to always grab the string, even if the bow itself isn't grabbed. Defaults to false.")]
		public bool stringAlwaysGrabbable;

		// Token: 0x04001EED RID: 7917
		[Tooltip("Sets whether or not arrows can be nocked when holding the non-main handle. Defaults to true.")]
		public bool nockOnlyMainHandle = true;

		// Token: 0x04001EEE RID: 7918
		[Tooltip("Defines whether or not to drop the arrow when the bow is ungrabbed. If set to false, the bow can hold an arrow even when not held.")]
		public bool loseNockOnUngrab = true;

		// Token: 0x04001EEF RID: 7919
		[Tooltip("If true, allows arrows to drop out of the bow. If false, prevents arrows from falling out of the bow.")]
		public bool allowOverdraw = true;

		// Token: 0x04001EF0 RID: 7920
		[Header("Audio")]
		[Tooltip("Plays when the bow is released and the arrow gets fired.")]
		public AudioContainer audioContainerShoot;

		// Token: 0x04001EF1 RID: 7921
		[Tooltip("Plays on a loop while the player is pulling the string back.")]
		public AudioContainer audioContainerDraw;

		// Token: 0x04001EF2 RID: 7922
		[Tooltip("Plays on loop while the string is moving (Either being pulled back, or snapping forward when released)")]
		public AudioClip audioClipString;

		// Token: 0x04001EF9 RID: 7929
		protected bool setupFinished;

		// Token: 0x04001EFA RID: 7930
		protected float currentTargetRatio;

		// Token: 0x04001EFB RID: 7931
		private string clipName;

		// Token: 0x04001EFC RID: 7932
		private bool _blockStringRelease;

		// Token: 0x04001EFD RID: 7933
		[NonSerialized]
		public Transform currentRest;

		// Token: 0x04001F06 RID: 7942
		protected AudioSource audioSourceString;

		// Token: 0x04001F07 RID: 7943
		protected AudioSource audioSourceShoot;

		// Token: 0x04001F08 RID: 7944
		protected Vector3 stringCorrectionVelocity;

		// Token: 0x04001F09 RID: 7945
		protected Vector3 maxArrowVelocity;

		// Token: 0x04001F0A RID: 7946
		protected Vector3? lastStringLocal;

		// Token: 0x04001F0B RID: 7947
		protected Vector3? stringReleaseLocal;

		// Token: 0x04001F0C RID: 7948
		protected Vector3? arrowUnnockForward;

		// Token: 0x04001F0D RID: 7949
		protected float arrowUnnockZ;

		// Token: 0x04001F0E RID: 7950
		protected Vector3 arrowShootDirection;

		// Token: 0x04001F0F RID: 7951
		protected float previousPull;

		// Token: 0x04001F10 RID: 7952
		protected float stepPull;

		// Token: 0x04001F11 RID: 7953
		protected bool drawAudioPlayed;

		// Token: 0x04001F12 RID: 7954
		protected bool checkJointsOnNull;

		// Token: 0x02000941 RID: 2369
		[Flags]
		public enum ArrowState
		{
			// Token: 0x04004434 RID: 17460
			None = 0,
			// Token: 0x04004435 RID: 17461
			Nocked = 1,
			// Token: 0x04004436 RID: 17462
			Rested = 2
		}

		// Token: 0x02000942 RID: 2370
		// (Invoke) Token: 0x060042FC RID: 17148
		public delegate void GrabNockEvent(Item arrow);

		// Token: 0x02000943 RID: 2371
		// (Invoke) Token: 0x06004300 RID: 17152
		public delegate void StringReleaseDelegate(float pullRatio, Item arrow);

		// Token: 0x02000944 RID: 2372
		// (Invoke) Token: 0x06004304 RID: 17156
		public delegate void UnnockingEvent(Item arrow, bool fired);

		// Token: 0x02000945 RID: 2373
		public enum DetachType
		{
			// Token: 0x04004438 RID: 17464
			Unnock,
			// Token: 0x04004439 RID: 17465
			Unrest,
			// Token: 0x0400443A RID: 17466
			Fire,
			// Token: 0x0400443B RID: 17467
			Both
		}
	}
}
