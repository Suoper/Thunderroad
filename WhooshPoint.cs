using System;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200036B RID: 875
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/WhooshPoint.html")]
	[AddComponentMenu("ThunderRoad/Whoosh")]
	public class WhooshPoint : ThunderBehaviour
	{
		// Token: 0x0600295C RID: 10588 RVA: 0x00119672 File Offset: 0x00117872
		private void Awake()
		{
			this.pb = base.gameObject.GetPhysicBodyInParent();
			this._transform = base.transform;
			this.myTimeSlice = WhooshPoint.timeSlice % WhooshPoint.timeSliceNumFrames;
			WhooshPoint.timeSlice++;
		}

		// Token: 0x0600295D RID: 10589 RVA: 0x001196B0 File Offset: 0x001178B0
		protected override void ManagedOnEnable()
		{
			if (this.effectData == null || !this.pb)
			{
				return;
			}
			if (this.trigger == WhooshPoint.Trigger.Always && (!this.item || !this.item.holder || !this.stopOnSnap))
			{
				this.effectInstance.SetIntensity(0f);
				this.effectActive = true;
				return;
			}
			if (this.trigger == WhooshPoint.Trigger.OnGrab && this.item && this.item.handlers.Count > 0)
			{
				this.effectInstance.SetIntensity(0f);
				this.effectActive = true;
				return;
			}
			if (this.effectInstance != null)
			{
				this.effectInstance.Stop(0);
			}
			this.effectActive = false;
			this.dampenedIntensity = 0f;
		}

		// Token: 0x0600295E RID: 10590 RVA: 0x00119781 File Offset: 0x00117981
		protected override void ManagedOnDisable()
		{
			this.effectActive = false;
			this.dampenedIntensity = 0f;
		}

		// Token: 0x0600295F RID: 10591 RVA: 0x00119798 File Offset: 0x00117998
		public virtual void Load(EffectData effectData, ItemData.Whoosh whooshData)
		{
			this.effectActive = false;
			this.dampenedIntensity = 0f;
			this.effectData = effectData;
			this.trigger = whooshData.trigger;
			this.minVelocity = whooshData.minVelocity;
			this.maxVelocity = whooshData.maxVelocity;
			this.stopOnSnap = whooshData.stopOnSnap;
			this.dampening = whooshData.dampening;
			this.item = base.GetComponentInParent<Item>();
			if (this.item)
			{
				this.item.OnGrabEvent += this.OnObjectGrabbed;
				this.item.OnTelekinesisGrabEvent += this.OnObjectGrabbed;
				this.item.OnTelekinesisReleaseEvent += this.OnObjectReleased;
				this.item.OnUngrabEvent += this.OnObjectReleased;
				this.item.OnFlyStartEvent += this.OnThrowingStarted;
				this.item.OnFlyEndEvent += this.OnThrowingEnded;
				this.item.OnSnapEvent += this.OnSnap;
			}
			if (this.effectInstance != null)
			{
				this.effectInstance.Despawn();
			}
			this.effectInstance = effectData.Spawn(base.transform.position, base.transform.rotation, base.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
			this.effectInstance.SetIntensity(0f);
			if (this.item)
			{
				this.effectInstance.source = this.item;
			}
			if (this.trigger == WhooshPoint.Trigger.Always && (!this.item || !this.item.holder || !this.stopOnSnap))
			{
				this.effectActive = true;
				return;
			}
			if (this.trigger == WhooshPoint.Trigger.OnGrab && this.item && this.item.handlers.Count > 0)
			{
				this.effectActive = true;
			}
		}

		// Token: 0x06002960 RID: 10592 RVA: 0x001199A4 File Offset: 0x00117BA4
		protected virtual void OnSnap(Holder holder)
		{
			if (this.item)
			{
				this.effectInstance.source = this.item;
			}
			if (this.stopOnSnap && this.effectInstance != null)
			{
				this.effectInstance.Stop(0);
			}
			this.effectActive = false;
			this.dampenedIntensity = 0f;
		}

		// Token: 0x06002961 RID: 10593 RVA: 0x001199FD File Offset: 0x00117BFD
		protected virtual void OnObjectGrabbed(Handle handle, SpellTelekinesis spellTelekinesis)
		{
			this.OnObjectGrabbed(handle, null);
		}

		// Token: 0x06002962 RID: 10594 RVA: 0x00119A08 File Offset: 0x00117C08
		protected virtual void OnObjectGrabbed(Handle handle, RagdollHand ragdollHand)
		{
			if (this.trigger == WhooshPoint.Trigger.OnGrab || this.trigger == WhooshPoint.Trigger.Always)
			{
				if (this.item)
				{
					this.effectInstance.source = this.item;
				}
				this.effectInstance.SetNoise(true);
				this.effectInstance.SetIntensity(0f);
				this.effectInstance.Play(0, false, false);
				this.effectActive = true;
			}
		}

		// Token: 0x06002963 RID: 10595 RVA: 0x00119A75 File Offset: 0x00117C75
		protected virtual void OnObjectReleased(Handle handle, SpellTelekinesis spellTelekinesis, bool tryThrow, bool isGrabbing)
		{
			this.OnObjectReleased(handle, null, false);
		}

		// Token: 0x06002964 RID: 10596 RVA: 0x00119A80 File Offset: 0x00117C80
		protected virtual void OnObjectReleased(Handle handle, RagdollHand ragdollHand, bool forSnap)
		{
			if (this.trigger == WhooshPoint.Trigger.OnGrab && !this.item.IsHanded())
			{
				if (this.item)
				{
					this.effectInstance.source = this.item;
				}
				this.effectInstance.Stop(0);
				this.effectActive = false;
				this.dampenedIntensity = 0f;
			}
		}

		// Token: 0x06002965 RID: 10597 RVA: 0x00119AE0 File Offset: 0x00117CE0
		protected virtual void OnThrowingStarted(Item _)
		{
			if (this.trigger == WhooshPoint.Trigger.OnFly && this.item.isFlying)
			{
				if (this.item)
				{
					this.effectInstance.source = this.item;
				}
				this.effectInstance.SetIntensity(0f);
				this.effectInstance.Play(0, false, false);
				this.effectActive = true;
			}
		}

		// Token: 0x06002966 RID: 10598 RVA: 0x00119B48 File Offset: 0x00117D48
		protected virtual void OnThrowingEnded(Item _)
		{
			if (this.trigger == WhooshPoint.Trigger.OnFly)
			{
				if (this.item)
				{
					this.effectInstance.source = this.item;
				}
				this.effectInstance.Stop(0);
				this.effectActive = false;
				this.dampenedIntensity = 0f;
			}
		}

		// Token: 0x06002967 RID: 10599 RVA: 0x00119B9C File Offset: 0x00117D9C
		public virtual void UpdateWhooshPoint()
		{
			if (UpdateManager.frameCount % WhooshPoint.timeSliceNumFrames != this.myTimeSlice)
			{
				return;
			}
			if (this.effectActive && !this.pb.IsSleeping())
			{
				Vector3 pointVelocity = this.pb.GetPointVelocity(this._transform.position);
				this.dampenedIntensity = Mathf.Lerp(this.dampenedIntensity, Mathf.InverseLerp(this.minVelocity, this.maxVelocity, pointVelocity.magnitude), this.dampening);
				this.effectInstance.SetIntensity(this.dampenedIntensity);
				if (this.item)
				{
					this.effectInstance.source = this.item;
				}
				if (this.dampenedIntensity > 0f && !this.effectInstance.isPlaying)
				{
					this.effectInstance.Play(0, false, false);
					return;
				}
			}
			else if (this.dampenedIntensity > 0f)
			{
				this.dampenedIntensity = Mathf.Lerp(this.dampenedIntensity, 0f, this.dampening);
			}
		}

		// Token: 0x06002968 RID: 10600 RVA: 0x00119CA0 File Offset: 0x00117EA0
		private void OnDestroy()
		{
			if (this.item)
			{
				this.item.OnGrabEvent -= this.OnObjectGrabbed;
				this.item.OnTelekinesisGrabEvent -= this.OnObjectGrabbed;
				this.item.OnTelekinesisReleaseEvent -= this.OnObjectReleased;
				this.item.OnUngrabEvent -= this.OnObjectReleased;
				this.item.OnFlyStartEvent -= this.OnThrowingStarted;
				this.item.OnFlyEndEvent -= this.OnThrowingEnded;
				this.item.OnSnapEvent -= this.OnSnap;
			}
		}

		// Token: 0x0400275A RID: 10074
		public static int timeSlice = 0;

		// Token: 0x0400275B RID: 10075
		public static int timeSliceNumFrames = 2;

		// Token: 0x0400275C RID: 10076
		private int myTimeSlice;

		// Token: 0x0400275D RID: 10077
		[Tooltip("Depicts how the whoosh is triggered.\n\"Always\" makes it so whoosh happens when velocity is met.\n\"On Grab\" makes Whoosh only play when grabbed.\n\"On Fly\" makes Whoosh only play when thrown/in air.")]
		public WhooshPoint.Trigger trigger;

		// Token: 0x0400275E RID: 10078
		[Tooltip("Minimum velocity of which the Whoosh plays")]
		public float minVelocity = 5f;

		// Token: 0x0400275F RID: 10079
		[Tooltip("Maximum velocity of item to play at max volume.")]
		public float maxVelocity = 12f;

		// Token: 0x04002760 RID: 10080
		[Tooltip("Depicts the volume of which the whoosh sound will play, depending on velocity. The slower the speed, the quieter the whoosh sound, while the faster the speed, the higher the volume. Lowest speed is depicted by minimunm velocity and highest speed is depicted by maximum velocity.")]
		public float dampening = 0.1f;

		// Token: 0x04002761 RID: 10081
		[Tooltip("Stops the whoosh sound once the item is connected to a holder.")]
		public bool stopOnSnap = true;

		// Token: 0x04002762 RID: 10082
		protected EffectData effectData;

		// Token: 0x04002763 RID: 10083
		protected EffectInstance effectInstance;

		// Token: 0x04002764 RID: 10084
		protected PhysicBody pb;

		// Token: 0x04002765 RID: 10085
		protected Item item;

		// Token: 0x04002766 RID: 10086
		protected bool effectActive;

		// Token: 0x04002767 RID: 10087
		protected float dampenedIntensity;

		// Token: 0x02000A63 RID: 2659
		public enum Trigger
		{
			// Token: 0x04004810 RID: 18448
			Always,
			// Token: 0x04004811 RID: 18449
			OnGrab,
			// Token: 0x04004812 RID: 18450
			OnFly
		}
	}
}
