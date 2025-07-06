using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000359 RID: 857
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/SimpleBreakable.html")]
	public class SimpleBreakable : MonoBehaviour
	{
		// Token: 0x0600281C RID: 10268 RVA: 0x00112E69 File Offset: 0x00111069
		public bool HasFlagNoGC(SimpleBreakable.DamageType flags, SimpleBreakable.DamageType value)
		{
			return (flags & value) > SimpleBreakable.DamageType.None;
		}

		// Token: 0x0600281D RID: 10269 RVA: 0x00112E71 File Offset: 0x00111071
		protected virtual void Awake()
		{
			this.health = this.maxHealth;
		}

		// Token: 0x0600281E RID: 10270 RVA: 0x00112E7F File Offset: 0x0011107F
		public void Restore()
		{
			this.isBroken = false;
			this.health = this.maxHealth;
			UnityEvent unityEvent = this.onRestore;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x0600281F RID: 10271 RVA: 0x00112EA4 File Offset: 0x001110A4
		private void OnCollisionEnter(Collision collision)
		{
			this.EnterCollision(collision);
		}

		// Token: 0x06002820 RID: 10272 RVA: 0x00112EB0 File Offset: 0x001110B0
		protected virtual bool EnterCollision(Collision collision)
		{
			if (!base.isActiveAndEnabled)
			{
				return false;
			}
			if (this.isBroken)
			{
				return false;
			}
			if (Time.unscaledTime - this.lastCollision <= this.collisionDelay)
			{
				return false;
			}
			this.lastCollision = Time.unscaledTime;
			float velocity = collision.relativeVelocity.magnitude;
			if (velocity < this.minHitVelocity)
			{
				return false;
			}
			Rigidbody rigidbody = collision.rigidbody;
			float num;
			if (rigidbody == null)
			{
				ArticulationBody articulationBody = collision.articulationBody;
				num = ((articulationBody != null) ? articulationBody.mass : 0f);
			}
			else
			{
				num = rigidbody.mass;
			}
			float mass = num;
			if (mass < this.minHitMass)
			{
				return false;
			}
			float damage = 0f;
			bool overridden = false;
			SimpleBreakable.DamageSource source = this.GetDamageSource(collision);
			SimpleBreakable.DamageOverride? damageSourceOverride = this.GetDamageSourceOverride(source);
			if (damageSourceOverride != null)
			{
				SimpleBreakable.DamageOverride damageOverride = damageSourceOverride.GetValueOrDefault();
				damage = damageOverride.damage;
				float num2 = damage;
				AnimationCurve animationCurve = damageOverride.velocityCurve;
				damage = num2 * ((animationCurve != null) ? animationCurve.Evaluate(velocity) : 1f);
				float num3 = damage;
				AnimationCurve animationCurve2 = damageOverride.massCurve;
				damage = num3 * ((animationCurve2 != null) ? animationCurve2.Evaluate(mass) : 1f);
				overridden = true;
			}
			if (!overridden)
			{
				damage = this.GetVelocityMassDamage(velocity, mass);
			}
			this.Hit(damage, (collision.rigidbody != null || collision.articulationBody != null) ? SimpleBreakable.DamageType.ArticulationOrRigidbody : SimpleBreakable.DamageType.Static);
			return true;
		}

		// Token: 0x06002821 RID: 10273 RVA: 0x00112FE4 File Offset: 0x001111E4
		private SimpleBreakable.DamageSource GetDamageSource(Collision collision)
		{
			Rigidbody rigidbody = collision.rigidbody;
			CollisionHandler hitCollisionHandler = (rigidbody != null) ? rigidbody.GetComponentInParent<CollisionHandler>() : null;
			if (hitCollisionHandler)
			{
				SimpleBreakable.DamageSource result;
				if (hitCollisionHandler.isItem)
				{
					if (hitCollisionHandler.GetComponentInParent<ItemMagicProjectile>())
					{
						result = SimpleBreakable.DamageSource.Fireball;
					}
					else if (hitCollisionHandler.item.isThrowed)
					{
						result = SimpleBreakable.DamageSource.ThrownWeapon;
					}
					else if (!hitCollisionHandler.item.IsFree)
					{
						result = SimpleBreakable.DamageSource.HeldWeapon;
					}
					else
					{
						result = SimpleBreakable.DamageSource.Item;
					}
				}
				else
				{
					result = SimpleBreakable.DamageSource.Ragdoll;
				}
				return result;
			}
			return SimpleBreakable.DamageSource.Unknown;
		}

		// Token: 0x06002822 RID: 10274 RVA: 0x00113050 File Offset: 0x00111250
		private SimpleBreakable.DamageOverride? GetDamageSourceOverride(SimpleBreakable.DamageSource source)
		{
			for (int i = 0; i < this.damageOverrides.Count; i++)
			{
				if (this.damageOverrides[i].source == source)
				{
					return new SimpleBreakable.DamageOverride?(this.damageOverrides[i]);
				}
			}
			return null;
		}

		// Token: 0x06002823 RID: 10275 RVA: 0x001130A2 File Offset: 0x001112A2
		private float GetVelocityMassDamage(float velocity, float mass)
		{
			return this.damageCurve.Evaluate(this.velocityCurve.Evaluate(velocity) * this.massCurve.Evaluate(mass));
		}

		// Token: 0x06002824 RID: 10276 RVA: 0x001130C8 File Offset: 0x001112C8
		public void OnParticleEffectHit(float hitValue)
		{
			this.Hit(hitValue, SimpleBreakable.DamageType.ParticleHit);
		}

		// Token: 0x06002825 RID: 10277 RVA: 0x001130D2 File Offset: 0x001112D2
		public void Hit(float damage)
		{
			this.Hit(damage, SimpleBreakable.DamageType.Scripts);
		}

		// Token: 0x06002826 RID: 10278 RVA: 0x001130DC File Offset: 0x001112DC
		public void Hit(float damage, SimpleBreakable.DamageSource source, SimpleBreakable.DamageType type = SimpleBreakable.DamageType.Scripts, float velocity = -1f, float mass = -1f)
		{
			SimpleBreakable.DamageOverride? damageSourceOverride = this.GetDamageSourceOverride(source);
			if (damageSourceOverride != null)
			{
				SimpleBreakable.DamageOverride damageOverride = damageSourceOverride.GetValueOrDefault();
				damage = damageOverride.damage;
				if (velocity >= 0f)
				{
					float num = damage;
					AnimationCurve animationCurve = damageOverride.velocityCurve;
					damage = num * ((animationCurve != null) ? animationCurve.Evaluate(velocity) : 1f);
				}
				if (mass >= 0f)
				{
					float num2 = damage;
					AnimationCurve animationCurve2 = damageOverride.massCurve;
					damage = num2 * ((animationCurve2 != null) ? animationCurve2.Evaluate(mass) : 1f);
				}
			}
			this.Hit(damage, type);
		}

		// Token: 0x06002827 RID: 10279 RVA: 0x0011315C File Offset: 0x0011135C
		protected virtual void Hit(float damage, SimpleBreakable.DamageType damageType)
		{
			if (!base.isActiveAndEnabled)
			{
				return;
			}
			if (!Application.isPlaying || this.isBroken)
			{
				return;
			}
			if (!this.HasFlagNoGC(this.allowedDamageTypes, damageType))
			{
				return;
			}
			if (this.health - damage <= 0f)
			{
				this.Break();
				return;
			}
			this.health -= damage;
			UnityEvent<float> unityEvent = this.onDamage;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke(damage);
		}

		// Token: 0x06002828 RID: 10280 RVA: 0x001131C8 File Offset: 0x001113C8
		public void Break()
		{
			if (!Application.isPlaying || this.isBroken)
			{
				return;
			}
			this.isBroken = true;
			this.health = 0f;
			if (this.forceReleaseOnBreak)
			{
				Handle[] componentsInChildren = base.GetComponentsInChildren<Handle>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].Release();
				}
				this.TryUngripClimber(Player.currentCreature.handRight.climb);
				this.TryUngripClimber(Player.currentCreature.handLeft.climb);
			}
			UnityEvent unityEvent = this.onBreak;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x06002829 RID: 10281 RVA: 0x00113256 File Offset: 0x00111456
		private void TryUngripClimber(RagdollHandClimb climber)
		{
			if (!climber.isGripping)
			{
				return;
			}
			if (!this.RecursiveSelfSeek(climber.gripCollider.transform))
			{
				return;
			}
			climber.UnGrip();
		}

		// Token: 0x0600282A RID: 10282 RVA: 0x0011327B File Offset: 0x0011147B
		private bool RecursiveSelfSeek(Transform transform)
		{
			return transform.parent == base.transform || (!(transform.parent == null) && this.RecursiveSelfSeek(transform.parent));
		}

		// Token: 0x040026F7 RID: 9975
		[Header("Damage")]
		public SimpleBreakable.DamageType allowedDamageTypes = SimpleBreakable.DamageType.ArticulationOrRigidbody | SimpleBreakable.DamageType.Static | SimpleBreakable.DamageType.Scripts;

		// Token: 0x040026F8 RID: 9976
		public float maxHealth = 30f;

		// Token: 0x040026F9 RID: 9977
		[Tooltip("Minimum delay between collisions being registered.")]
		public float collisionDelay = 0.1f;

		// Token: 0x040026FA RID: 9978
		[Tooltip("Minimum velocity to deal damage to a crystal.")]
		public float minHitVelocity = 5f;

		// Token: 0x040026FB RID: 9979
		[Tooltip("Minimum mass for a rigidbody to deal damage on collision.")]
		public float minHitMass;

		// Token: 0x040026FC RID: 9980
		public List<SimpleBreakable.DamageOverride> damageOverrides = new List<SimpleBreakable.DamageOverride>();

		// Token: 0x040026FD RID: 9981
		[Header("Damage Fallback")]
		public AnimationCurve velocityCurve = AnimationCurve.Linear(5f, 1f, 15f, 2f);

		// Token: 0x040026FE RID: 9982
		public AnimationCurve massCurve = AnimationCurve.Linear(0f, 1f, 5f, 2f);

		// Token: 0x040026FF RID: 9983
		public AnimationCurve damageCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

		// Token: 0x04002700 RID: 9984
		[Header("Events")]
		public UnityEvent onRestore;

		// Token: 0x04002701 RID: 9985
		public UnityEvent<float> onDamage;

		// Token: 0x04002702 RID: 9986
		public UnityEvent onBreak;

		// Token: 0x04002703 RID: 9987
		public bool forceReleaseOnBreak = true;

		// Token: 0x04002704 RID: 9988
		[NonSerialized]
		public float health;

		// Token: 0x04002705 RID: 9989
		private bool isBroken;

		// Token: 0x04002706 RID: 9990
		private float lastCollision;

		// Token: 0x02000A46 RID: 2630
		[Flags]
		public enum DamageType
		{
			// Token: 0x040047A4 RID: 18340
			None = 0,
			// Token: 0x040047A5 RID: 18341
			ArticulationOrRigidbody = 1,
			// Token: 0x040047A6 RID: 18342
			Static = 2,
			// Token: 0x040047A7 RID: 18343
			ParticleHit = 4,
			// Token: 0x040047A8 RID: 18344
			Scripts = 8
		}

		// Token: 0x02000A47 RID: 2631
		public enum DamageSource
		{
			// Token: 0x040047AA RID: 18346
			Unknown,
			// Token: 0x040047AB RID: 18347
			Ragdoll,
			// Token: 0x040047AC RID: 18348
			Fireball,
			// Token: 0x040047AD RID: 18349
			ThrownWeapon,
			// Token: 0x040047AE RID: 18350
			HeldWeapon,
			// Token: 0x040047AF RID: 18351
			Item,
			// Token: 0x040047B0 RID: 18352
			Thunderbolt
		}

		// Token: 0x02000A48 RID: 2632
		[Serializable]
		public struct DamageOverride
		{
			// Token: 0x060045B1 RID: 17841 RVA: 0x0019681A File Offset: 0x00194A1A
			public DamageOverride(SimpleBreakable.DamageSource source, float damage, AnimationCurve velocityCurve, AnimationCurve massCurve)
			{
				this.source = source;
				this.damage = damage;
				this.velocityCurve = velocityCurve;
				this.massCurve = massCurve;
			}

			// Token: 0x040047B1 RID: 18353
			[Tooltip("Type of damage this override will apply to")]
			public SimpleBreakable.DamageSource source;

			// Token: 0x040047B2 RID: 18354
			[Tooltip("Base amount of damage done by this damage type")]
			public float damage;

			// Token: 0x040047B3 RID: 18355
			[Tooltip("Multiply damage by this curve, evaluated by the impact velocity of the collision")]
			public AnimationCurve velocityCurve;

			// Token: 0x040047B4 RID: 18356
			[Tooltip("Multiply damage by this curve, evaluated by the mass of the colliding object")]
			public AnimationCurve massCurve;
		}
	}
}
