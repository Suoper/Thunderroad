using System;
using UnityEngine;

namespace ThunderRoad
{
	/// <summary>
	/// This script allows events based around a creatures mouth(s) to be triggered and hooked into.
	/// </summary>
	// Token: 0x02000255 RID: 597
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Creatures/CreatureMouthRelay.html")]
	[RequireComponent(typeof(Rigidbody))]
	public class CreatureMouthRelay : ThunderBehaviour
	{
		/// <summary>
		/// Invoked when an object touches the mouth.
		/// </summary>
		// Token: 0x140000AA RID: 170
		// (add) Token: 0x060019ED RID: 6637 RVA: 0x000ACE10 File Offset: 0x000AB010
		// (remove) Token: 0x060019EE RID: 6638 RVA: 0x000ACE48 File Offset: 0x000AB048
		public event CreatureMouthRelay.OnObjectTouchMouth OnObjectTouchMouthEvent;

		/// <summary>
		/// Invoked when an object touches the mouth.
		/// </summary>
		// Token: 0x140000AB RID: 171
		// (add) Token: 0x060019EF RID: 6639 RVA: 0x000ACE80 File Offset: 0x000AB080
		// (remove) Token: 0x060019F0 RID: 6640 RVA: 0x000ACEB8 File Offset: 0x000AB0B8
		public event CreatureMouthRelay.OnObjectLeaveMouth OnObjectLeaveMouthEvent;

		/// <summary>
		/// Invoked when an item touches the mouth.
		/// </summary>
		// Token: 0x140000AC RID: 172
		// (add) Token: 0x060019F1 RID: 6641 RVA: 0x000ACEF0 File Offset: 0x000AB0F0
		// (remove) Token: 0x060019F2 RID: 6642 RVA: 0x000ACF28 File Offset: 0x000AB128
		public event CreatureMouthRelay.OnItemTouchMouth OnItemTouchMouthEvent;

		/// <summary>
		/// Invoked when an item leaves the mouth.
		/// </summary>
		// Token: 0x140000AD RID: 173
		// (add) Token: 0x060019F3 RID: 6643 RVA: 0x000ACF60 File Offset: 0x000AB160
		// (remove) Token: 0x060019F4 RID: 6644 RVA: 0x000ACF98 File Offset: 0x000AB198
		public event CreatureMouthRelay.OnItemLeaveMouth OnItemLeaveMouthEvent;

		/// <summary>
		/// Invoked each frame when the relay updates, its useful for relay hooks like the LiquidReciever to reduce the update call count.
		/// </summary>
		// Token: 0x140000AE RID: 174
		// (add) Token: 0x060019F5 RID: 6645 RVA: 0x000ACFD0 File Offset: 0x000AB1D0
		// (remove) Token: 0x060019F6 RID: 6646 RVA: 0x000AD008 File Offset: 0x000AB208
		public event CreatureMouthRelay.OnRelayUpdate OnRelayUpdateEvent;

		// Token: 0x060019F7 RID: 6647 RVA: 0x000AD040 File Offset: 0x000AB240
		private void Awake()
		{
			this.collisionHandler = base.GetComponentInParent<CollisionHandler>();
			this.creature = base.GetComponentInParent<Creature>();
			this.zone = base.gameObject.AddComponent<SphereCollider>();
			this.zone.isTrigger = true;
			this.zone.radius = this.mouthRadius;
			Collider[] componentsInChildren = this.creature.GetComponentsInChildren<Collider>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				Physics.IgnoreCollision(componentsInChildren[i], this.zone, true);
			}
			if (this.liquidReciever != null)
			{
				this.liquidReciever.AddComponent<SphereCollider>().radius = this.mouthRadius;
				this.liquidReciever.SetLayerRecursively(GameManager.GetLayer(LayerName.LiquidFlow));
			}
			this.orgTransform = base.transform.parent;
			this.orgLocalPosition = base.transform.localPosition;
			this.orgLocalRotation = base.transform.localRotation;
			if (this.playerOnly && !this.creature.isPlayer)
			{
				this.isMouthActive = false;
			}
		}

		// Token: 0x060019F8 RID: 6648 RVA: 0x000AD141 File Offset: 0x000AB341
		protected override void ManagedOnEnable()
		{
			this.zone.enabled = true;
			this.collisionHandler.enabled = true;
		}

		// Token: 0x060019F9 RID: 6649 RVA: 0x000AD15C File Offset: 0x000AB35C
		private void Start()
		{
			if (this.collisionHandler != null && this.collisionHandler.ragdollPart != null)
			{
				base.transform.SetParent(this.creature.transform, true);
				this.creature.ragdoll.OnStateChange += this.OnRagdollStateChange;
				this.creature.OnDespawnEvent += this.OnCreatureDespawn;
				this.creature.OnKillEvent += this.OnCreatureDied;
			}
		}

		// Token: 0x1700018E RID: 398
		// (get) Token: 0x060019FA RID: 6650 RVA: 0x000AD1EB File Offset: 0x000AB3EB
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x060019FB RID: 6651 RVA: 0x000AD1F0 File Offset: 0x000AB3F0
		protected internal override void ManagedUpdate()
		{
			if (this.collisionHandler && this.collisionHandler.isRagdollPart && (this.creature.ragdoll.state == Ragdoll.State.NoPhysic || this.creature.ragdoll.state == Ragdoll.State.Kinematic))
			{
				base.transform.SetPositionAndRotation(this.orgTransform.TransformPoint(this.orgLocalPosition), this.orgTransform.rotation * this.orgLocalRotation);
			}
		}

		/// <summary>
		/// Invoked when this creature dies.
		/// </summary>
		// Token: 0x060019FC RID: 6652 RVA: 0x000AD26F File Offset: 0x000AB46F
		protected void OnCreatureDied(CollisionInstance collisionInstance, EventTime eventTime)
		{
			this.zone.enabled = false;
			GameObject gameObject = this.liquidReciever;
			if (gameObject == null)
			{
				return;
			}
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Invoked when this creature is despawned.
		/// </summary>
		// Token: 0x060019FD RID: 6653 RVA: 0x000AD290 File Offset: 0x000AB490
		protected void OnCreatureDespawn(EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				base.transform.SetParent(this.orgTransform, true);
				base.transform.localPosition = this.orgLocalPosition;
				base.transform.localRotation = this.orgLocalRotation;
				base.gameObject.SetActive(this.collisionHandler.isRagdollPart && this.collisionHandler.ragdollPart.ragdoll.creature.isPlayer);
			}
		}

		/// <summary>
		/// Invoked when the state of this ragdoll changes.
		/// </summary>
		// Token: 0x060019FE RID: 6654 RVA: 0x000AD30C File Offset: 0x000AB50C
		protected void OnRagdollStateChange(Ragdoll.State previousState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicStateChange, EventTime eventTime)
		{
			if (eventTime == EventTime.OnEnd)
			{
				if (newState == Ragdoll.State.NoPhysic || newState == Ragdoll.State.Kinematic)
				{
					if (base.transform.parent != this.collisionHandler.ragdollPart.ragdoll.creature.transform)
					{
						base.transform.SetParent(this.collisionHandler.ragdollPart.ragdoll.creature.transform, true);
						base.transform.position = this.orgTransform.transform.TransformPoint(this.orgLocalPosition);
						base.transform.rotation = this.orgTransform.transform.rotation * this.orgLocalRotation;
						base.gameObject.SetActive(this.collisionHandler.ragdollPart.ragdoll.creature.isPlayer);
						return;
					}
				}
				else if (base.transform.parent != this.orgTransform)
				{
					base.transform.SetParent(this.orgTransform, true);
					base.transform.localPosition = this.orgLocalPosition;
					base.transform.localRotation = this.orgLocalRotation;
					base.gameObject.SetActive(true);
				}
			}
		}

		// Token: 0x060019FF RID: 6655 RVA: 0x000AD446 File Offset: 0x000AB646
		public void DisableTemporarily(float disableTime)
		{
			this.mouthEnterableTime = Time.time + disableTime;
		}

		// Token: 0x06001A00 RID: 6656 RVA: 0x000AD458 File Offset: 0x000AB658
		private void OnTriggerEnter(Collider collider)
		{
			if (!this.isMouthActive && this.mouthEnterableTime <= Time.time)
			{
				return;
			}
			CreatureMouthRelay.OnObjectTouchMouth onObjectTouchMouthEvent = this.OnObjectTouchMouthEvent;
			if (onObjectTouchMouthEvent != null)
			{
				onObjectTouchMouthEvent(collider.gameObject);
			}
			Item item = collider.GetComponentInParent<Item>();
			if (item != null)
			{
				if (item.data != null)
				{
					ItemModuleMouthTouch itemModuleMouthTouch = item.data.GetModule<ItemModuleMouthTouch>();
					if (itemModuleMouthTouch != null)
					{
						itemModuleMouthTouch.OnMouthTouch(item, this);
					}
				}
				CreatureMouthRelay.OnItemTouchMouth onItemTouchMouthEvent = this.OnItemTouchMouthEvent;
				if (onItemTouchMouthEvent == null)
				{
					return;
				}
				onItemTouchMouthEvent(item);
			}
		}

		// Token: 0x06001A01 RID: 6657 RVA: 0x000AD4D4 File Offset: 0x000AB6D4
		private void OnTriggerExit(Collider collider)
		{
			if (!this.isMouthActive)
			{
				return;
			}
			CreatureMouthRelay.OnObjectLeaveMouth onObjectLeaveMouthEvent = this.OnObjectLeaveMouthEvent;
			if (onObjectLeaveMouthEvent != null)
			{
				onObjectLeaveMouthEvent(collider.gameObject);
			}
			Item item = collider.GetComponentInParent<Item>();
			if (item != null)
			{
				CreatureMouthRelay.OnItemLeaveMouth onItemLeaveMouthEvent = this.OnItemLeaveMouthEvent;
				if (onItemLeaveMouthEvent == null)
				{
					return;
				}
				onItemLeaveMouthEvent(item);
			}
		}

		// Token: 0x06001A02 RID: 6658 RVA: 0x000AD522 File Offset: 0x000AB722
		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.blue;
			Gizmos.DrawWireSphere(base.transform.position, this.mouthRadius);
		}

		// Token: 0x040018C2 RID: 6338
		[Tooltip("How big is the detection radius?")]
		public float mouthRadius = 0.05f;

		// Token: 0x040018C3 RID: 6339
		[Tooltip("Can this mouth receive food/liquid?")]
		public bool isMouthActive = true;

		// Token: 0x040018C4 RID: 6340
		[Tooltip("If enabled this relay will only be active if the current creature is the player.")]
		public bool playerOnly;

		// Token: 0x040018C5 RID: 6341
		[NonSerialized]
		public CollisionHandler collisionHandler;

		// Token: 0x040018C6 RID: 6342
		protected float mouthEnterableTime = -1f;

		// Token: 0x040018C7 RID: 6343
		protected Transform orgTransform;

		// Token: 0x040018C8 RID: 6344
		protected Vector3 orgLocalPosition;

		// Token: 0x040018C9 RID: 6345
		protected Quaternion orgLocalRotation;

		// Token: 0x040018CA RID: 6346
		[SerializeField]
		private GameObject liquidReciever;

		// Token: 0x040018CB RID: 6347
		private SphereCollider zone;

		/// <summary>
		/// The creature this relay is for.
		/// </summary>
		// Token: 0x040018CC RID: 6348
		public Creature creature;

		/// <summary>
		/// Invoked when a particle collides.
		///
		/// This gets invoked from LiquidReciever, here just for the sake of consistency.
		/// </summary>
		// Token: 0x040018CD RID: 6349
		public CreatureMouthRelay.OnParticleCollide OnParticleCollideEvent;

		// Token: 0x02000895 RID: 2197
		// (Invoke) Token: 0x060040B5 RID: 16565
		public delegate void OnParticleCollide(GameObject other);

		// Token: 0x02000896 RID: 2198
		// (Invoke) Token: 0x060040B9 RID: 16569
		public delegate void OnObjectTouchMouth(GameObject gameObject);

		// Token: 0x02000897 RID: 2199
		// (Invoke) Token: 0x060040BD RID: 16573
		public delegate void OnObjectLeaveMouth(GameObject gameObject);

		// Token: 0x02000898 RID: 2200
		// (Invoke) Token: 0x060040C1 RID: 16577
		public delegate void OnItemTouchMouth(Item item);

		// Token: 0x02000899 RID: 2201
		// (Invoke) Token: 0x060040C5 RID: 16581
		public delegate void OnItemLeaveMouth(Item item);

		// Token: 0x0200089A RID: 2202
		// (Invoke) Token: 0x060040C9 RID: 16585
		public delegate void OnRelayUpdate();
	}
}
