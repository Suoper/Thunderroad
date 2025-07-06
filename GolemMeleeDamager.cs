using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000269 RID: 617
	public class GolemMeleeDamager : MonoBehaviour
	{
		// Token: 0x06001BBA RID: 7098 RVA: 0x000B7A68 File Offset: 0x000B5C68
		private void OnEnable()
		{
			GolemAnimatorEvent golemAnimatorEvent = this.golemAnimatorEvent;
			golemAnimatorEvent.onEnableHitbox = (Action<bool>)Delegate.Combine(golemAnimatorEvent.onEnableHitbox, new Action<bool>(this.OnEnableHitbox));
			this.golemController.OnGolemStateChange += this.GolemStateChange;
		}

		// Token: 0x06001BBB RID: 7099 RVA: 0x000B7AA8 File Offset: 0x000B5CA8
		private void GolemStateChange(GolemController.State newState)
		{
			if (newState != GolemController.State.Active && newState != GolemController.State.Rampage)
			{
				this.OnEnableHitbox(false);
			}
		}

		// Token: 0x06001BBC RID: 7100 RVA: 0x000B7AB9 File Offset: 0x000B5CB9
		private void OnDisable()
		{
			GolemAnimatorEvent golemAnimatorEvent = this.golemAnimatorEvent;
			golemAnimatorEvent.onEnableHitbox = (Action<bool>)Delegate.Remove(golemAnimatorEvent.onEnableHitbox, new Action<bool>(this.OnEnableHitbox));
			this.golemController.OnGolemStateChange -= this.GolemStateChange;
		}

		// Token: 0x06001BBD RID: 7101 RVA: 0x000B7AFC File Offset: 0x000B5CFC
		private void OnEnableHitbox(bool hitBoxEnabled)
		{
			if (this.disableColliderDuringAttack)
			{
				foreach (Collider collider in this.colliders)
				{
					collider.enabled = !hitBoxEnabled;
				}
			}
			this.hitBoxEnabled = hitBoxEnabled;
			this.hitCreatures.Clear();
		}

		// Token: 0x06001BBE RID: 7102 RVA: 0x000B7B6C File Offset: 0x000B5D6C
		private void OnTriggerEnter(Collider other)
		{
			if (this.hitBoxEnabled && other.attachedRigidbody && !this.golemController.isClimbed)
			{
				Creature creature = other.attachedRigidbody.GetComponentInParent<Creature>();
				if (!this.hitCreatures.Contains(creature) && creature)
				{
					Vector3 velocity = (base.transform.position - this.lastPosition) / Time.deltaTime;
					bool blocked = false;
					if (this.shieldBlocksDamage || this.shieldBlocksForce)
					{
						blocked = (this.ShieldIsBlocking(other.attachedRigidbody.transform.position, velocity, creature, Side.Right) || this.ShieldIsBlocking(other.attachedRigidbody.transform.position, velocity, creature, Side.Left));
						if (blocked && !this.blockAudio.isPlaying)
						{
							this.blockAudio.Play();
						}
					}
					if (!blocked || !this.shieldBlocksForce)
					{
						creature.currentLocomotion.physicBody.velocity = Vector3.zero;
						creature.currentLocomotion.physicBody.AddForce((velocity.normalized.ToXZ() * this.hitForce + new Vector3(0f, this.hitForceUpward, 0f)) * this.golemController.hitForceMultiplier, ForceMode.VelocityChange);
					}
					if ((!blocked || !this.shieldBlocksDamage) && this.hitDamage > 0f)
					{
						creature.Damage(this.hitDamage * this.golemController.hitDamageMultiplier, DamageType.Blunt);
					}
					this.onHit.Invoke();
					this.hitCreatures.Add(creature);
				}
			}
		}

		// Token: 0x06001BBF RID: 7103 RVA: 0x000B7D0C File Offset: 0x000B5F0C
		private bool ShieldIsBlocking(Vector3 bodyPosition, Vector3 velocity, Creature creature, Side side)
		{
			bool hitHeld = false;
			RagdollHand hand = creature.GetHand(side);
			Item item2;
			if (hand == null)
			{
				item2 = null;
			}
			else
			{
				Handle grabbedHandle = hand.grabbedHandle;
				item2 = ((grabbedHandle != null) ? grabbedHandle.item : null);
			}
			Item item = item2;
			if (item == null)
			{
				return hitHeld;
			}
			if (item.data.type != ItemData.Type.Shield)
			{
				return hitHeld;
			}
			if (Vector3.Dot(item.parryPoint.forward, velocity) > 0f)
			{
				return hitHeld;
			}
			Vector3 rayStart = bodyPosition - velocity;
			if (rayStart.DistanceSqr(bodyPosition) < rayStart.DistanceSqr(item.parryPoint.position))
			{
				return hitHeld;
			}
			Plane plane = new Plane(item.parryPoint.forward, item.parryPoint.position);
			Ray ray = new Ray(rayStart, velocity.normalized);
			float enterDist;
			if (plane.Raycast(ray, out enterDist))
			{
				Vector3 planePosition = rayStart + velocity.normalized * enterDist;
				hitHeld = item.parryPoint.position.PointInRadius(planePosition, this.shieldBlockRadius);
			}
			return hitHeld;
		}

		// Token: 0x06001BC0 RID: 7104 RVA: 0x000B7DFE File Offset: 0x000B5FFE
		private void Update()
		{
			this.lastPosition = base.transform.position;
		}

		// Token: 0x04001A90 RID: 6800
		public Rigidbody rigidbody;

		// Token: 0x04001A91 RID: 6801
		public GolemController golemController;

		// Token: 0x04001A92 RID: 6802
		public GolemAnimatorEvent golemAnimatorEvent;

		// Token: 0x04001A93 RID: 6803
		public List<Collider> colliders;

		// Token: 0x04001A94 RID: 6804
		public bool disableColliderDuringAttack = true;

		// Token: 0x04001A95 RID: 6805
		public float hitDamage = 20f;

		// Token: 0x04001A96 RID: 6806
		public float hitForce = 10f;

		// Token: 0x04001A97 RID: 6807
		public float hitForceUpward = 3f;

		// Token: 0x04001A98 RID: 6808
		public bool shieldBlocksDamage = true;

		// Token: 0x04001A99 RID: 6809
		public bool shieldBlocksForce;

		// Token: 0x04001A9A RID: 6810
		public float shieldBlockRadius = 0.75f;

		// Token: 0x04001A9B RID: 6811
		public AudioSource blockAudio;

		// Token: 0x04001A9C RID: 6812
		public UnityEvent onHit;

		// Token: 0x04001A9D RID: 6813
		protected bool hitBoxEnabled;

		// Token: 0x04001A9E RID: 6814
		protected Vector3 lastPosition;

		// Token: 0x04001A9F RID: 6815
		protected List<Creature> hitCreatures = new List<Creature>();
	}
}
