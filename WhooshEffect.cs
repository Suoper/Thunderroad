using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200036A RID: 874
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/WhooshEffect")]
	[AddComponentMenu("ThunderRoad/Whoosh Effect")]
	public class WhooshEffect : ThunderBehaviour
	{
		// Token: 0x0600294F RID: 10575 RVA: 0x00118F64 File Offset: 0x00117164
		private void Awake()
		{
			this.rb = base.GetComponentInParent<Rigidbody>();
			this.effectMixer = base.GetComponentInParent<FxController>();
			this.item = base.GetComponentInParent<Item>();
			if (this.item)
			{
				this.effectMixer.source = this.item;
				this.item.OnGrabEvent += this.OnObjectGrabbed;
				this.item.OnUngrabEvent += this.OnObjectReleased;
				this.item.OnFlyStartEvent += this.OnThrowingStarted;
				this.item.OnFlyEndEvent += this.OnThrowingEnded;
				this.item.OnSnapEvent += this.OnSnap;
			}
		}

		// Token: 0x06002950 RID: 10576 RVA: 0x0011902E File Offset: 0x0011722E
		private void Start()
		{
			this.started = true;
			this.Init();
		}

		// Token: 0x06002951 RID: 10577 RVA: 0x0011903D File Offset: 0x0011723D
		protected override void ManagedOnEnable()
		{
			if (this.started)
			{
				this.Init();
			}
		}

		// Token: 0x06002952 RID: 10578 RVA: 0x0011904D File Offset: 0x0011724D
		protected override void ManagedOnDisable()
		{
			this.effectActive = false;
			this.dampenedIntensity = 0f;
		}

		// Token: 0x06002953 RID: 10579 RVA: 0x00119064 File Offset: 0x00117264
		protected virtual void Init()
		{
			if (this.effectMixer == null || !this.rb)
			{
				return;
			}
			if (this.trigger == WhooshEffect.Trigger.Always && (!this.item || !this.item.holder || !this.stopOnSnap))
			{
				this.effectMixer.SetSpeed(0f);
				if (this.item)
				{
					this.effectMixer.source = this.item;
				}
				this.effectMixer.Play();
				foreach (GameObject gameObject in this.toggleGameobjects)
				{
					gameObject.SetActive(true);
				}
				this.effectActive = true;
				return;
			}
			if (this.trigger == WhooshEffect.Trigger.OnGrab && this.item && this.item.handlers.Count > 0)
			{
				this.effectMixer.SetSpeed(0f);
				if (this.item)
				{
					this.effectMixer.source = this.item;
				}
				this.effectMixer.Play();
				foreach (GameObject gameObject2 in this.toggleGameobjects)
				{
					gameObject2.SetActive(true);
				}
				this.effectActive = true;
				return;
			}
			this.effectMixer.SetSpeed(0f);
			if (this.item)
			{
				this.effectMixer.source = this.item;
			}
			this.effectMixer.Stop();
			this.effectActive = false;
			foreach (GameObject gameObject3 in this.toggleGameobjects)
			{
				gameObject3.SetActive(false);
			}
			this.dampenedIntensity = 0f;
		}

		// Token: 0x06002954 RID: 10580 RVA: 0x00119280 File Offset: 0x00117480
		protected virtual void OnSnap(Holder holder)
		{
			if (this.stopOnSnap)
			{
				this.effectMixer.SetSpeed(0f);
				if (this.item)
				{
					this.effectMixer.source = this.item;
				}
				this.effectMixer.Stop();
				foreach (GameObject gameObject in this.toggleGameobjects)
				{
					gameObject.SetActive(false);
				}
			}
			this.effectActive = false;
			this.dampenedIntensity = 0f;
		}

		// Token: 0x06002955 RID: 10581 RVA: 0x00119324 File Offset: 0x00117524
		protected virtual void OnObjectGrabbed(Handle handle, RagdollHand ragdollHand)
		{
			if (this.trigger == WhooshEffect.Trigger.OnGrab || this.trigger == WhooshEffect.Trigger.Always)
			{
				this.effectMixer.SetSpeed(0f);
				if (this.item)
				{
					this.effectMixer.source = this.item;
				}
				this.effectMixer.Play();
				foreach (GameObject gameObject in this.toggleGameobjects)
				{
					gameObject.SetActive(true);
				}
				this.effectActive = true;
			}
		}

		// Token: 0x06002956 RID: 10582 RVA: 0x001193C8 File Offset: 0x001175C8
		protected virtual void OnObjectReleased(Handle handle, RagdollHand ragdollHand, bool forSnap)
		{
			if (this.trigger == WhooshEffect.Trigger.OnGrab && !this.item.IsHanded())
			{
				this.effectMixer.SetSpeed(0f);
				this.effectMixer.Stop();
				foreach (GameObject gameObject in this.toggleGameobjects)
				{
					gameObject.SetActive(false);
				}
				this.effectActive = false;
				this.dampenedIntensity = 0f;
			}
		}

		// Token: 0x06002957 RID: 10583 RVA: 0x0011945C File Offset: 0x0011765C
		protected virtual void OnThrowingStarted(Item _)
		{
			if (this.trigger == WhooshEffect.Trigger.OnFly && this.item.isFlying)
			{
				this.effectMixer.SetSpeed(0f);
				if (this.item)
				{
					this.effectMixer.source = this.item;
				}
				this.effectMixer.Play();
				foreach (GameObject gameObject in this.toggleGameobjects)
				{
					gameObject.SetActive(true);
				}
				this.effectActive = true;
			}
		}

		// Token: 0x06002958 RID: 10584 RVA: 0x00119508 File Offset: 0x00117708
		protected virtual void OnThrowingEnded(Item _)
		{
			if (this.trigger == WhooshEffect.Trigger.OnFly)
			{
				this.effectMixer.SetSpeed(0f);
				this.effectMixer.Stop();
				this.effectActive = false;
				foreach (GameObject gameObject in this.toggleGameobjects)
				{
					gameObject.SetActive(false);
				}
				this.dampenedIntensity = 0f;
			}
		}

		// Token: 0x17000279 RID: 633
		// (get) Token: 0x06002959 RID: 10585 RVA: 0x00119590 File Offset: 0x00117790
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x0600295A RID: 10586 RVA: 0x00119594 File Offset: 0x00117794
		protected internal override void ManagedUpdate()
		{
			if (this.effectActive && !this.rb.IsSleeping())
			{
				Vector3 pointVelocity = this.rb.GetPointVelocity(base.transform.position);
				this.dampenedIntensity = Mathf.Lerp(this.dampenedIntensity, Mathf.InverseLerp(this.minVelocity, this.maxVelocity, pointVelocity.magnitude), this.dampening);
				this.effectMixer.SetSpeed(this.dampenedIntensity);
				return;
			}
			if (this.dampenedIntensity > 0f)
			{
				this.dampenedIntensity = Mathf.Lerp(this.dampenedIntensity, 0f, this.dampening);
			}
		}

		// Token: 0x0400274E RID: 10062
		public WhooshEffect.Trigger trigger;

		// Token: 0x0400274F RID: 10063
		public float minVelocity = 5f;

		// Token: 0x04002750 RID: 10064
		public float maxVelocity = 12f;

		// Token: 0x04002751 RID: 10065
		public float dampening = 1f;

		// Token: 0x04002752 RID: 10066
		public bool stopOnSnap = true;

		// Token: 0x04002753 RID: 10067
		public List<GameObject> toggleGameobjects = new List<GameObject>();

		// Token: 0x04002754 RID: 10068
		protected Rigidbody rb;

		// Token: 0x04002755 RID: 10069
		protected Item item;

		// Token: 0x04002756 RID: 10070
		protected bool effectActive;

		// Token: 0x04002757 RID: 10071
		protected float dampenedIntensity;

		// Token: 0x04002758 RID: 10072
		protected FxController effectMixer;

		// Token: 0x04002759 RID: 10073
		protected bool started;

		// Token: 0x02000A62 RID: 2658
		public enum Trigger
		{
			// Token: 0x0400480C RID: 18444
			Always,
			// Token: 0x0400480D RID: 18445
			OnGrab,
			// Token: 0x0400480E RID: 18446
			OnFly
		}
	}
}
