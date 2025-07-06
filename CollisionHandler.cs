using System;
using System.Collections.Generic;
using RainyReignGames.MeshUtilities;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000317 RID: 791
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/CollisionHandler")]
	[AddComponentMenu("ThunderRoad/Collision handler")]
	public class CollisionHandler : ThunderBehaviour
	{
		// Token: 0x14000121 RID: 289
		// (add) Token: 0x0600259B RID: 9627 RVA: 0x001026F8 File Offset: 0x001008F8
		// (remove) Token: 0x0600259C RID: 9628 RVA: 0x00102730 File Offset: 0x00100930
		public event CollisionHandler.RawCollisionEvent OnCollisionPreHitEvent;

		// Token: 0x14000122 RID: 290
		// (add) Token: 0x0600259D RID: 9629 RVA: 0x00102768 File Offset: 0x00100968
		// (remove) Token: 0x0600259E RID: 9630 RVA: 0x001027A0 File Offset: 0x001009A0
		public event CollisionHandler.CollisionEvent OnPressureHitStartEvent;

		// Token: 0x14000123 RID: 291
		// (add) Token: 0x0600259F RID: 9631 RVA: 0x001027D8 File Offset: 0x001009D8
		// (remove) Token: 0x060025A0 RID: 9632 RVA: 0x00102810 File Offset: 0x00100A10
		public event CollisionHandler.CollisionEvent OnCollisionStartEvent;

		// Token: 0x14000124 RID: 292
		// (add) Token: 0x060025A1 RID: 9633 RVA: 0x00102848 File Offset: 0x00100A48
		// (remove) Token: 0x060025A2 RID: 9634 RVA: 0x00102880 File Offset: 0x00100A80
		public event CollisionHandler.CollisionEvent OnCollisionStopEvent;

		// Token: 0x14000125 RID: 293
		// (add) Token: 0x060025A3 RID: 9635 RVA: 0x001028B8 File Offset: 0x00100AB8
		// (remove) Token: 0x060025A4 RID: 9636 RVA: 0x001028F0 File Offset: 0x00100AF0
		public event CollisionHandler.CollidingEvent OnCollidingEvent;

		// Token: 0x14000126 RID: 294
		// (add) Token: 0x060025A5 RID: 9637 RVA: 0x00102928 File Offset: 0x00100B28
		// (remove) Token: 0x060025A6 RID: 9638 RVA: 0x00102960 File Offset: 0x00100B60
		public event CollisionHandler.TriggerEvent OnTriggerEnterEvent;

		// Token: 0x14000127 RID: 295
		// (add) Token: 0x060025A7 RID: 9639 RVA: 0x00102998 File Offset: 0x00100B98
		// (remove) Token: 0x060025A8 RID: 9640 RVA: 0x001029D0 File Offset: 0x00100BD0
		public event CollisionHandler.TriggerEvent OnTriggerExitEvent;

		// Token: 0x060025A9 RID: 9641 RVA: 0x00102A08 File Offset: 0x00100C08
		protected void Awake()
		{
			base.GetComponentsInChildren<Damager>(this.damagers);
			base.GetComponentsInChildren<Holder>(this.holders);
			this.ragdollPart = base.GetComponentInParent<RagdollPart>();
			if (this.ragdollPart)
			{
				this.enterOnly = true;
				this.isRagdollPart = true;
			}
			this.physicBody = base.gameObject.GetPhysicBody();
			this.orgMass = this.physicBody.mass;
			this.orgDrag = this.physicBody.drag;
			this.orgAngularDrag = this.physicBody.angularDrag;
			this.orgSleepThreshold = this.physicBody.sleepThreshold;
			this.orgUseGravity = this.physicBody.useGravity;
			this.item = base.GetComponentInParent<Item>();
			if (this.item)
			{
				this.isItem = true;
			}
			this.breakable = base.GetComponentInParent<Breakable>();
			this.isBreakable = this.breakable;
			if (!Level.master)
			{
				this.SetMaxCollision(1);
				base.enabled = false;
				this.active = false;
				return;
			}
			this.SetMaxCollision(Catalog.gameData.maxObjectCollision);
			this.gravityMultiplierApproxZero = Mathf.Approximately(this.gravityMultiplier, 0f);
			this.gravityMultiplierApproxOne = Mathf.Approximately(this.gravityMultiplier, 1f);
		}

		// Token: 0x17000249 RID: 585
		// (get) Token: 0x060025AA RID: 9642 RVA: 0x00102B55 File Offset: 0x00100D55
		public ThunderEntity Entity
		{
			get
			{
				Item creature;
				if ((creature = this.item) == null)
				{
					RagdollPart ragdollPart = this.ragdollPart;
					if (ragdollPart == null)
					{
						return null;
					}
					Ragdoll ragdoll = ragdollPart.ragdoll;
					if (ragdoll == null)
					{
						return null;
					}
					creature = ragdoll.creature;
				}
				return creature;
			}
		}

		// Token: 0x060025AB RID: 9643 RVA: 0x00102B80 File Offset: 0x00100D80
		public void SetMaxCollision(int value)
		{
			this.collisions = new CollisionInstance[value];
			for (int i = 0; i < this.collisions.Length; i++)
			{
				this.collisions[i] = new CollisionInstance();
			}
		}

		// Token: 0x060025AC RID: 9644 RVA: 0x00102BBC File Offset: 0x00100DBC
		public void SetPhysicBody(float mass, float drag, float angularDrag)
		{
			this.physicBody.mass = mass;
			this.orgMass = mass;
			this.physicBody.drag = drag;
			this.orgDrag = drag;
			this.physicBody.angularDrag = angularDrag;
			this.orgAngularDrag = angularDrag;
		}

		// Token: 0x060025AD RID: 9645 RVA: 0x00102C08 File Offset: 0x00100E08
		public virtual bool TryGetFreeCollisionIndexAndNotSameGroup(Collider sourceCollider, ColliderGroup sourceColliderGroup, Collider targetCollider, ColliderGroup targetColliderGroup, out int index)
		{
			index = -1;
			for (int i = 0; i < this.collisions.Length; i++)
			{
				CollisionInstance collisionInstance = this.collisions[i];
				if (!collisionInstance.active)
				{
					if (index == -1)
					{
						index = i;
					}
				}
				else if (collisionInstance.IsSameSourceColliderGroup(sourceCollider, sourceColliderGroup) && collisionInstance.IsSameTargetColliderGroup(targetCollider, targetColliderGroup))
				{
					return false;
				}
			}
			return index != -1;
		}

		// Token: 0x060025AE RID: 9646 RVA: 0x00102C6C File Offset: 0x00100E6C
		public bool TryGetFreeCollisionIndex(out int index)
		{
			index = -1;
			for (int i = 0; i < this.collisions.Length; i++)
			{
				if (!this.collisions[i].active)
				{
					index = i;
					return true;
				}
			}
			return false;
		}

		// Token: 0x060025AF RID: 9647 RVA: 0x00102CA4 File Offset: 0x00100EA4
		public int GetFreeCollisionIndex()
		{
			for (int i = 0; i < this.collisions.Length; i++)
			{
				if (!this.collisions[i].active)
				{
					return i;
				}
			}
			return -1;
		}

		// Token: 0x060025B0 RID: 9648 RVA: 0x00102CD6 File Offset: 0x00100ED6
		protected override void ManagedOnDisable()
		{
			this.forceAllowHitLocomotionLayer = GameManager.GetLayer(LayerName.None);
			if (!GameManager.isQuitting)
			{
				this.StopCollisions();
			}
		}

		// Token: 0x060025B1 RID: 9649 RVA: 0x00102CF4 File Offset: 0x00100EF4
		public void SortDamagers()
		{
			List<Damager> filteredDamagers = new List<Damager>(this.damagers.Count);
			for (int i = this.damagers.Count - 1; i >= 0; i--)
			{
				Damager damager = this.damagers[i];
				bool flag;
				if (damager == null)
				{
					flag = (null != null);
				}
				else
				{
					DamagerData data = damager.data;
					flag = (((data != null) ? data.damageModifierData : null) != null);
				}
				if (flag)
				{
					filteredDamagers.Add(damager);
				}
			}
			this.damagers.Clear();
			this.AddDamagersByType(filteredDamagers, DamageType.Pierce);
			this.AddDamagersByType(filteredDamagers, DamageType.Slash);
			this.AddDamagersByType(filteredDamagers, DamageType.Blunt);
			this.AddDamagersByType(filteredDamagers, DamageType.Energy);
			this.AddDamagersByType(filteredDamagers, DamageType.Fire);
			this.AddDamagersByType(filteredDamagers, DamageType.Lightning);
		}

		// Token: 0x060025B2 RID: 9650 RVA: 0x00102D94 File Offset: 0x00100F94
		private void AddDamagersByType(List<Damager> damagersToAdd, DamageType damageType)
		{
			for (int i = damagersToAdd.Count - 1; i >= 0; i--)
			{
				Damager damager = damagersToAdd[i];
				if (damager.data.damageModifierData.damageType == damageType)
				{
					this.damagers.Add(damager);
					damagersToAdd.RemoveAt(i);
				}
			}
		}

		// Token: 0x060025B3 RID: 9651 RVA: 0x00102DE4 File Offset: 0x00100FE4
		public void SetPhysicModifier(object handler, float? gravityMultiplier = null, float massMultiplier = 1f, float drag = -1f, float angularDrag = -1f, float sleepThreshold = -1f, EffectData effectData = null)
		{
			CollisionHandler.PhysicModifier physicModifier = null;
			if (this.physicModifiers == null)
			{
				this.physicModifiers = new List<CollisionHandler.PhysicModifier>();
			}
			if (this.physicModifiers != null)
			{
				int physicModifiersCount = this.physicModifiers.Count;
				for (int i = 0; i < physicModifiersCount; i++)
				{
					if (this.physicModifiers[i].handler == handler)
					{
						physicModifier = this.physicModifiers[i];
						break;
					}
				}
			}
			if (physicModifier == null)
			{
				physicModifier = new CollisionHandler.PhysicModifier
				{
					handler = handler
				};
				this.physicModifiers.Add(physicModifier);
			}
			physicModifier.gravityMultiplier = gravityMultiplier;
			physicModifier.massMultiplier = massMultiplier;
			physicModifier.drag = drag;
			physicModifier.angularDrag = angularDrag;
			physicModifier.sleepThreshold = sleepThreshold;
			EffectInstance effectInstance = physicModifier.effectInstance;
			if (effectInstance != null)
			{
				effectInstance.End(false, -1f);
			}
			if (effectData != null)
			{
				if (this.item.renderers[0])
				{
					physicModifier.effectInstance = effectData.Spawn(this.physicBody.transform, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					if (this.item)
					{
						physicModifier.effectInstance.source = this.item;
					}
					if (this.ragdollPart)
					{
						physicModifier.effectInstance.source = this.ragdollPart.ragdoll.creature;
					}
					physicModifier.effectInstance.SetRenderer(this.item.renderers[0], false);
					physicModifier.effectInstance.Play(0, false, false);
				}
				else
				{
					Debug.LogError(this.item.name + " has no renderer, unable to spawn physic modifier effect");
				}
			}
			this.RefreshPhysicModifiers();
		}

		// Token: 0x060025B4 RID: 9652 RVA: 0x00102F84 File Offset: 0x00101184
		public void RemovePhysicModifier(object handler)
		{
			for (int i = this.physicModifiers.Count - 1; i >= 0; i--)
			{
				CollisionHandler.PhysicModifier physicModifier = this.physicModifiers[i];
				if (physicModifier.handler == handler)
				{
					if (physicModifier.effectInstance != null)
					{
						physicModifier.effectInstance.End(false, -1f);
					}
					this.physicModifiers.RemoveAt(i);
				}
			}
			this.RefreshPhysicModifiers();
		}

		// Token: 0x060025B5 RID: 9653 RVA: 0x00102FEC File Offset: 0x001011EC
		public void ClearPhysicModifiers()
		{
			int physicModifiersCount = this.physicModifiers.Count;
			for (int i = 0; i < physicModifiersCount; i++)
			{
				CollisionHandler.PhysicModifier physicModifier = this.physicModifiers[i];
				if (physicModifier.effectInstance != null)
				{
					physicModifier.effectInstance.End(false, -1f);
				}
			}
			this.physicModifiers.Clear();
			this.RefreshPhysicModifiers();
		}

		// Token: 0x060025B6 RID: 9654 RVA: 0x00103048 File Offset: 0x00101248
		public void RefreshPhysicModifiers()
		{
			if (!this.physicBody)
			{
				return;
			}
			if (this.physicModifiers.Count == 0)
			{
				this.physicBody.mass = this.orgMass;
				this.physicBody.drag = this.orgDrag;
				this.physicBody.angularDrag = this.orgAngularDrag;
				this.physicBody.sleepThreshold = this.orgSleepThreshold;
				this.physicBody.useGravity = this.orgUseGravity;
				this.gravityMultiplier = 1f;
				this.gravityMultiplierApproxZero = false;
				this.gravityMultiplierApproxOne = true;
				return;
			}
			float resultGravityMultiplier = 1f;
			float resultMassMultiplier = 1f;
			float maxDrag = 0f;
			float maxAngularDrag = 0f;
			float minSleepThreshold = float.PositiveInfinity;
			bool sleepThresholdOverride = false;
			int physicModifiersCount = this.physicModifiers.Count;
			for (int i = 0; i < physicModifiersCount; i++)
			{
				CollisionHandler.PhysicModifier physicModifier = this.physicModifiers[i];
				if (physicModifier.gravityMultiplier != null)
				{
					float? num = physicModifier.gravityMultiplier;
					float num2 = (float)1;
					if (!(num.GetValueOrDefault() == num2 & num != null))
					{
						resultGravityMultiplier *= physicModifier.gravityMultiplier.Value;
					}
				}
				if (physicModifier.massMultiplier > 0f && physicModifier.massMultiplier != 1f)
				{
					resultMassMultiplier *= physicModifier.massMultiplier;
				}
				if (physicModifier.drag >= 0f && physicModifier.drag > maxDrag)
				{
					maxDrag = physicModifier.drag;
				}
				if (physicModifier.angularDrag >= 0f && physicModifier.angularDrag > maxAngularDrag)
				{
					maxAngularDrag = physicModifier.angularDrag;
				}
				if (physicModifier.sleepThreshold > 0f && physicModifier.sleepThreshold < minSleepThreshold)
				{
					minSleepThreshold = physicModifier.sleepThreshold;
					sleepThresholdOverride = true;
				}
			}
			this.gravityMultiplier = resultGravityMultiplier;
			this.gravityMultiplierApproxZero = Mathf.Approximately(this.gravityMultiplier, 0f);
			this.gravityMultiplierApproxOne = Mathf.Approximately(this.gravityMultiplier, 1f);
			this.physicBody.useGravity = this.gravityMultiplierApproxOne;
			if (!this.gravityMultiplierApproxZero && !this.gravityMultiplierApproxOne)
			{
				if (!this.gravity && !base.TryGetComponent<Gravity>(out this.gravity))
				{
					this.gravity = base.gameObject.AddComponent<Gravity>();
				}
				this.gravity.collisionHandler = this;
				this.gravity.gravityMultiplier = this.gravityMultiplier;
				this.gravity.enabled = true;
			}
			else if (this.gravity)
			{
				this.gravity.enabled = false;
			}
			this.physicBody.mass = this.orgMass * resultMassMultiplier;
			this.physicBody.drag = ((maxDrag > 0f) ? maxDrag : this.orgDrag);
			this.physicBody.angularDrag = ((maxAngularDrag > 0f) ? maxAngularDrag : this.orgAngularDrag);
			this.physicBody.sleepThreshold = (sleepThresholdOverride ? minSleepThreshold : this.orgSleepThreshold);
		}

		// Token: 0x060025B7 RID: 9655 RVA: 0x00103328 File Offset: 0x00101528
		public void RemoveAllPenetratedObjects()
		{
			List<Damager> list;
			this.RemoveAllPenetratedObjects(false, out list);
		}

		// Token: 0x060025B8 RID: 9656 RVA: 0x0010333E File Offset: 0x0010153E
		public void RemoveAllPenetratedObjects(out List<Damager> removedDamagers)
		{
			this.RemoveAllPenetratedObjects(true, out removedDamagers);
		}

		// Token: 0x060025B9 RID: 9657 RVA: 0x00103348 File Offset: 0x00101548
		private void RemoveAllPenetratedObjects(bool output, out List<Damager> removedDamagers)
		{
			removedDamagers = this.lastUnpenetratedList;
			if (output)
			{
				removedDamagers.Clear();
			}
			for (int o = this.penetratedObjects.Count - 1; o >= 0; o--)
			{
				CollisionHandler collisionHandler = this.penetratedObjects[o];
				for (int c = 0; c < collisionHandler.collisions.Length; c++)
				{
					CollisionInstance collisionInstance = collisionHandler.collisions[c];
					RagdollPart hitRagdollPart = collisionInstance.damageStruct.hitRagdollPart;
					CollisionHandler x;
					if ((x = ((hitRagdollPart != null) ? hitRagdollPart.collisionHandler : null)) == null)
					{
						Item hitItem = collisionInstance.damageStruct.hitItem;
						x = ((hitItem != null) ? hitItem.mainCollisionHandler : null);
					}
					if (x == this)
					{
						Damager damager = collisionInstance.damageStruct.damager;
						if (damager != null)
						{
							damager.UnPenetrate(collisionInstance, false);
							if (output && !removedDamagers.Contains(damager))
							{
								removedDamagers.Add(damager);
							}
						}
					}
				}
			}
		}

		// Token: 0x1700024A RID: 586
		// (get) Token: 0x060025BA RID: 9658 RVA: 0x0010341C File Offset: 0x0010161C
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x060025BB RID: 9659 RVA: 0x0010341F File Offset: 0x0010161F
		protected internal override void ManagedFixedUpdate()
		{
			if (this.active && !this.gravityMultiplierApproxZero && !this.gravityMultiplierApproxOne)
			{
				this.physicBody.AddForce(this.gravityMultiplier * Physics.gravity, ForceMode.Acceleration);
			}
		}

		// Token: 0x060025BC RID: 9660 RVA: 0x00103458 File Offset: 0x00101658
		protected internal override void ManagedUpdate()
		{
			if (this.active && (!this.physicBody.IsSleeping() || this.physicBody.isKinematic))
			{
				if (!this.isPhysicBodyActive)
				{
					this.isPhysicBodyActive = true;
				}
				if (this.isColliding != this.wasColliding)
				{
					this.InvokeColliding(this.isColliding);
					this.wasColliding = this.isColliding;
				}
				this.isColliding = false;
				int frameCount = UpdateManager.frameCount;
				for (int i = this.collisions.Length - 1; i >= 0; i--)
				{
					if (this.collisions[i].active && this.collisions[i].damageStruct.penetration == DamageStruct.Penetration.None && frameCount - this.collisions[i].lastStayFrame > Catalog.gameData.maxFramesToKeepCollision)
					{
						this.StopCollision(this.collisions[i]);
					}
					if (this.collisions[i].active)
					{
						this.isColliding = true;
					}
				}
				this.lastLinearVelocity = this.physicBody.velocity;
				this.lastLinearVelocityMagnitude = this.lastLinearVelocity.magnitude;
				this.lastAngularVelocity = this.physicBody.angularVelocity;
			}
			else if (this.isPhysicBodyActive)
			{
				this.StopCollisions();
				this.lastLinearVelocity = Vector3.zero;
				this.lastAngularVelocity = Vector3.zero;
				if (this.isColliding)
				{
					this.wasColliding = (this.isColliding = false);
					this.InvokeColliding(false);
				}
				this.isPhysicBodyActive = false;
			}
			for (int j = this.collisions.Length - 1; j >= 0; j--)
			{
				if (this.collisions[j].active && this.collisions[j].damageStruct.active && this.collisions[j].damageStruct.penetration != DamageStruct.Penetration.None)
				{
					this.collisions[j].damageStruct.damager.UpdatePenetration(this.collisions[j]);
				}
			}
		}

		// Token: 0x060025BD RID: 9661 RVA: 0x00103630 File Offset: 0x00101830
		public Vector3 CalculateImpulseVelocity(Collision collision)
		{
			Vector3 impulseVelocity = -collision.relativeVelocity;
			impulseVelocity = -collision.relativeVelocity.normalized * (collision.impulse / Time.fixedDeltaTime / 100f).magnitude;
			if (impulseVelocity == Vector3.zero)
			{
				impulseVelocity = -collision.relativeVelocity;
			}
			return impulseVelocity;
		}

		// Token: 0x060025BE RID: 9662 RVA: 0x001036A0 File Offset: 0x001018A0
		public Vector3 CalculateLastPointVelocity(Vector3 hitPoint)
		{
			Vector3 lhs = this.lastAngularVelocity;
			Vector3 vector = base.transform.InverseTransformPoint(hitPoint);
			Vector3 centerOfMass = this.physicBody.centerOfMass;
			float rhsX = vector.x - centerOfMass.x;
			float rhsY = vector.y - centerOfMass.y;
			float rhsZ = vector.z - centerOfMass.z;
			float crossX = lhs.y * rhsZ - lhs.z * rhsY;
			float crossY = lhs.z * rhsX - lhs.x * rhsZ;
			float crossZ = lhs.x * rhsY - lhs.y * rhsX;
			float x = this.lastLinearVelocity.x + crossX;
			float y = this.lastLinearVelocity.y + crossY;
			float z = this.lastLinearVelocity.z + crossZ;
			return new Vector3(x, y, z);
		}

		// Token: 0x060025BF RID: 9663 RVA: 0x00103768 File Offset: 0x00101968
		public void StartNewCollision(CollisionInstance collisionInstance, Collider sourceCollider, Collider targetCollider, ColliderGroup sourceColliderGroup, ColliderGroup targetColliderGroup, Vector3 impactVelocity, Vector3 contactPoint, Vector3 contactNormal, float intensity, MaterialData sourceMaterial, MaterialData targetMaterial = null)
		{
			this.InvokeCollisionPreHit(sourceCollider, targetCollider, sourceColliderGroup, targetColliderGroup, impactVelocity, contactPoint, contactNormal);
			collisionInstance.NewHit(sourceCollider, targetCollider, sourceColliderGroup, targetColliderGroup, impactVelocity, contactPoint, contactNormal, intensity, sourceMaterial, targetMaterial);
			this.InvokeCollisionStart(collisionInstance);
		}

		// Token: 0x060025C0 RID: 9664 RVA: 0x001037A6 File Offset: 0x001019A6
		public void InvokeCollisionPreHit(Collider sourceCollider, Collider targetCollider, ColliderGroup sourceColliderGroup, ColliderGroup targetColliderGroup, Vector3 impactVelocity, Vector3 contactPoint, Vector3 contactNormal)
		{
			CollisionHandler.RawCollisionEvent onCollisionPreHitEvent = this.OnCollisionPreHitEvent;
			if (onCollisionPreHitEvent == null)
			{
				return;
			}
			onCollisionPreHitEvent(sourceCollider, targetCollider, sourceColliderGroup, targetColliderGroup, impactVelocity, contactPoint, contactNormal);
		}

		// Token: 0x060025C1 RID: 9665 RVA: 0x001037C3 File Offset: 0x001019C3
		public void InvokePressureHit(CollisionInstance collisionInstance)
		{
			CollisionHandler.CollisionEvent onPressureHitStartEvent = this.OnPressureHitStartEvent;
			if (onPressureHitStartEvent == null)
			{
				return;
			}
			onPressureHitStartEvent(collisionInstance);
		}

		// Token: 0x060025C2 RID: 9666 RVA: 0x001037D6 File Offset: 0x001019D6
		public void InvokeCollisionStart(CollisionInstance collisionInstance)
		{
			CollisionHandler.CollisionEvent onCollisionStartEvent = this.OnCollisionStartEvent;
			if (onCollisionStartEvent == null)
			{
				return;
			}
			onCollisionStartEvent(collisionInstance);
		}

		// Token: 0x060025C3 RID: 9667 RVA: 0x001037E9 File Offset: 0x001019E9
		public void InvokeColliding(bool colliding)
		{
			CollisionHandler.CollidingEvent onCollidingEvent = this.OnCollidingEvent;
			if (onCollidingEvent == null)
			{
				return;
			}
			onCollidingEvent(colliding);
		}

		// Token: 0x060025C4 RID: 9668 RVA: 0x001037FC File Offset: 0x001019FC
		public void InvokeTriggerEnter(Collider other)
		{
			CollisionHandler.TriggerEvent onTriggerEnterEvent = this.OnTriggerEnterEvent;
			if (onTriggerEnterEvent == null)
			{
				return;
			}
			onTriggerEnterEvent(other);
		}

		// Token: 0x060025C5 RID: 9669 RVA: 0x0010380F File Offset: 0x00101A0F
		public void InvokeTriggerExit(Collider other)
		{
			CollisionHandler.TriggerEvent onTriggerExitEvent = this.OnTriggerExitEvent;
			if (onTriggerExitEvent == null)
			{
				return;
			}
			onTriggerExitEvent(other);
		}

		// Token: 0x060025C6 RID: 9670 RVA: 0x00103824 File Offset: 0x00101A24
		private Vector3 CalculateImpactVelocity(Vector3 contactPoint, ColliderGroup targetColliderGroup)
		{
			Vector3 sourceImpactVelocity = this.CalculateLastPointVelocity(contactPoint);
			Vector3? vector;
			if (targetColliderGroup == null)
			{
				vector = null;
			}
			else
			{
				CollisionHandler collisionHandler = targetColliderGroup.collisionHandler;
				vector = ((collisionHandler != null) ? new Vector3?(collisionHandler.CalculateLastPointVelocity(contactPoint)) : null);
			}
			Vector3 targetImpactVelocity = vector ?? Vector3.zero;
			sourceImpactVelocity.x -= targetImpactVelocity.x;
			sourceImpactVelocity.y -= targetImpactVelocity.y;
			sourceImpactVelocity.z -= targetImpactVelocity.z;
			return sourceImpactVelocity;
		}

		// Token: 0x060025C7 RID: 9671 RVA: 0x001038B4 File Offset: 0x00101AB4
		protected void OnCollisionEnter(Collision collision)
		{
			if (!this.active)
			{
				return;
			}
			ContactPoint contact = collision.GetContact(0);
			Collider sourceCollider = contact.thisCollider;
			Collider targetCollider = contact.otherCollider;
			GameObject sourceColliderGameObject = sourceCollider.gameObject;
			GameObject targetColliderGameObject = targetCollider.gameObject;
			if (!sourceColliderGameObject.activeInHierarchy || !targetColliderGameObject.activeInHierarchy)
			{
				return;
			}
			int size;
			int[] locomotionLayerList = GameManager.GetLocomotionLayerList(out size);
			int sourceLayer = sourceColliderGameObject.layer;
			int targetLayer = targetColliderGameObject.layer;
			for (int i = 0; i < size; i++)
			{
				int layer = locomotionLayerList[i];
				if ((layer == sourceLayer && layer != this.forceAllowHitLocomotionLayer) || layer == targetLayer)
				{
					return;
				}
			}
			if (this.IsRagdollAndHitByExcludedLayers(targetCollider) || this.IsStandingUngrabbedRagdoll())
			{
				return;
			}
			ColliderGroup sourceColliderGroup = sourceCollider.GetComponentInParent<ColliderGroup>();
			ColliderGroup targetColliderGroup = targetCollider.GetComponentInParent<ColliderGroup>();
			bool targetColliderGroupIsNull = targetColliderGroup == null;
			if (!targetColliderGroupIsNull && this.IsRagdollGroundedAndHitSelf(targetColliderGroup.collisionHandler))
			{
				return;
			}
			if (!targetColliderGroupIsNull)
			{
				this.TransferLastHandler(targetColliderGroup);
			}
			Vector3 contactPoint = contact.point;
			Vector3 impactVelocity = this.checkMinVelocity ? this.CalculateImpactVelocity(contactPoint, targetColliderGroup) : Vector3.zero;
			float impactMagnitude = this.checkMinVelocity ? impactVelocity.magnitude : 0f;
			if (this.IsItemNotMinimumVelocityForHit(impactMagnitude))
			{
				return;
			}
			bool shouldTouchTargetCreature = false;
			if (this.IsRagdollNotMinimumVelocityForHit(impactMagnitude))
			{
				if (targetColliderGroupIsNull || targetColliderGroup.collisionHandler == null || !targetColliderGroup.collisionHandler.isRagdollPart || targetColliderGroup.collisionHandler.ragdollPart.ragdoll.creature.state == Creature.State.Dead || targetColliderGroup.collisionHandler.ragdollPart.ragdoll == this.ragdollPart.ragdoll)
				{
					return;
				}
				shouldTouchTargetCreature = true;
			}
			if (shouldTouchTargetCreature)
			{
				this.TouchTargetCreature(targetColliderGroup, sourceCollider, sourceColliderGroup, targetCollider);
				return;
			}
			int colIndex;
			if (!this.TryGetFreeCollisionIndexAndNotSameGroup(sourceCollider, sourceColliderGroup, targetCollider, targetColliderGroup, out colIndex))
			{
				return;
			}
			if (!this.checkMinVelocity)
			{
				impactVelocity = this.CalculateImpactVelocity(contactPoint, targetColliderGroup);
				impactMagnitude = impactVelocity.magnitude;
			}
			float intensity = this.forceFullIntensity ? 1f : Mathf.InverseLerp(Catalog.gameData.collisionEnterVelocityRange.x, Catalog.gameData.collisionEnterVelocityRange.y, impactMagnitude);
			Vector3 contactNormal = contact.normal;
			if (!targetColliderGroupIsNull && !(targetColliderGroup.collisionHandler == null) && !targetColliderGroup.collisionHandler.ragdollPart)
			{
				if (!(((sourceColliderGroup != null) ? sourceColliderGroup.collisionHandler : null) != null))
				{
					return;
				}
				CollisionHandler collisionHandler = sourceColliderGroup.collisionHandler;
				float? num = (collisionHandler != null) ? new float?(collisionHandler.lastLinearVelocityMagnitude) : null;
				CollisionHandler collisionHandler2 = targetColliderGroup.collisionHandler;
				float? num2 = (collisionHandler2 != null) ? new float?(collisionHandler2.lastLinearVelocityMagnitude) : null;
				if (!(num.GetValueOrDefault() > num2.GetValueOrDefault() & (num != null & num2 != null)))
				{
					return;
				}
			}
			bool sourceMeshRayCast = this.ShouldMeshRaycast();
			int physicsMaterialHash = this.GetPhysicsMaterialHash(sourceMeshRayCast, contactPoint, -contactNormal, -impactVelocity, sourceColliderGroup, sourceCollider);
			bool targetMeshRayCast = !targetColliderGroupIsNull && targetColliderGroup.collisionHandler != null && targetColliderGroup.collisionHandler.ShouldMeshRaycast();
			int targetPhysicMaterialHash = this.GetPhysicsMaterialHash(targetMeshRayCast, contactPoint, contactNormal, impactVelocity, targetColliderGroup, targetCollider);
			MaterialData sourceMaterial;
			MaterialData targetMaterial;
			if (MaterialData.TryGetMaterials(physicsMaterialHash, targetPhysicMaterialHash, out sourceMaterial, out targetMaterial))
			{
				this.InvokeCollisionPreHit(sourceCollider, targetCollider, sourceColliderGroup, targetColliderGroup, impactVelocity, contactPoint, contactNormal);
				this.collisions[colIndex].NewHit(sourceCollider, targetCollider, sourceColliderGroup, targetColliderGroup, impactVelocity, contactPoint, contactNormal, intensity, sourceMaterial, targetMaterial);
				this.collisions[colIndex].startingCollision = collision;
				this.InvokeCollisionStart(this.collisions[colIndex]);
				if ((targetColliderGroup != null) ? targetColliderGroup.collisionHandler : null)
				{
					targetColliderGroup.collisionHandler.InvokeCollisionStart(this.collisions[colIndex]);
					return;
				}
			}
			else
			{
				if (targetMaterial == null)
				{
					Debug.LogWarningFormat("Target physics material not found [" + targetColliderGameObject.GetPathFromRoot() + "]!", new object[]
					{
						targetCollider
					});
				}
				if (sourceMaterial == null)
				{
					Debug.LogWarningFormat("Source physics material not found [" + sourceColliderGameObject.GetPathFromRoot() + "]!", new object[]
					{
						sourceCollider
					});
				}
			}
		}

		// Token: 0x060025C8 RID: 9672 RVA: 0x00103CB8 File Offset: 0x00101EB8
		public virtual bool IsCollidingWithSameGroup(Collider sourceCollider, ColliderGroup sourceColliderGroup, Collider targetCollider, ColliderGroup targetColliderGroup)
		{
			for (int i = 0; i < this.collisions.Length; i++)
			{
				CollisionInstance collisionInstance = this.collisions[i];
				if (collisionInstance.active && collisionInstance.IsSameSourceColliderGroup(sourceCollider, sourceColliderGroup) && collisionInstance.IsSameTargetColliderGroup(targetCollider, targetColliderGroup))
				{
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// This triggers a collision on a target creature for the purposes of stealth detection
		/// </summary>
		// Token: 0x060025C9 RID: 9673 RVA: 0x00103D04 File Offset: 0x00101F04
		public virtual void TouchTargetCreature(ColliderGroup targetColliderGroup, Collider sourceCollider, ColliderGroup sourceColliderGroup, Collider targetCollider)
		{
			if (targetColliderGroup == null)
			{
				return;
			}
			CollisionHandler targetCollisionHandler = targetColliderGroup.collisionHandler;
			if (!targetCollisionHandler.isRagdollPart)
			{
				return;
			}
			if (targetCollisionHandler.ragdollPart.ragdoll == this.ragdollPart.ragdoll)
			{
				return;
			}
			if (targetCollisionHandler.ragdollPart.ragdoll.creature.state == Creature.State.Dead)
			{
				return;
			}
			targetCollisionHandler.ragdollPart.ragdoll.CollisionStartStop(new CollisionInstance
			{
				active = true,
				sourceCollider = sourceCollider,
				sourceColliderGroup = sourceColliderGroup,
				targetCollider = targetCollider,
				targetColliderGroup = targetColliderGroup
			}, targetCollisionHandler.ragdollPart, true);
		}

		// Token: 0x060025CA RID: 9674 RVA: 0x00103D9B File Offset: 0x00101F9B
		public virtual bool IsRagdollNotMinimumVelocityForHit(float impactMagnitude)
		{
			return this.checkMinVelocity && this.isRagdollPart && impactMagnitude < this.ragdollPart.ragdoll.collisionMinVelocity;
		}

		// Token: 0x060025CB RID: 9675 RVA: 0x00103DC4 File Offset: 0x00101FC4
		public virtual bool IsItemNotMinimumVelocityForHit(float impactMagnitude)
		{
			return this.checkMinVelocity && this.isItem && impactMagnitude < Catalog.gameData.collisionEnterVelocityRange.x && !this.item.leftPlayerHand && !this.item.rightPlayerHand;
		}

		// Token: 0x060025CC RID: 9676 RVA: 0x00103E1C File Offset: 0x0010201C
		private bool IsRagdollGroundedAndHitSelf(CollisionHandler targetCollisionHandler)
		{
			return this.isRagdollPart && this.ragdollPart != null && targetCollisionHandler != null && targetCollisionHandler.isRagdollPart && !this.ragdollPart.ragdoll.isGrabbed && !this.ragdollPart.ragdoll.isTkGrabbed && (this.ragdollPart.ragdoll.creature.state == Creature.State.Dead || this.ragdollPart.ragdoll.creature.fallState == Creature.FallState.StabilizedOnGround || this.ragdollPart.ragdoll.creature.fallState == Creature.FallState.Stabilizing) && targetCollisionHandler.ragdollPart.ragdoll == this.ragdollPart.ragdoll;
		}

		// Token: 0x060025CD RID: 9677 RVA: 0x00103EE4 File Offset: 0x001020E4
		public virtual void TransferLastHandler(ColliderGroup targetColliderGroup)
		{
			bool flag;
			if (targetColliderGroup == null)
			{
				flag = true;
			}
			else
			{
				CollisionHandler collisionHandler = targetColliderGroup.collisionHandler;
				bool? flag2 = (collisionHandler != null) ? new bool?(collisionHandler.isItem) : null;
				bool flag3 = true;
				flag = !(flag2.GetValueOrDefault() == flag3 & flag2 != null);
			}
			if (flag || targetColliderGroup.collisionHandler.item.lastHandler != null)
			{
				return;
			}
			RagdollHand lastHandler = null;
			if (this.isRagdollPart)
			{
				lastHandler = (this.isRagdollPart ? this.ragdollPart.ragdoll.creature.GetHand(Side.Right) : null);
			}
			if (this.isItem)
			{
				if (this.item.lastHandler)
				{
					lastHandler = this.item.lastHandler;
				}
				if (this.item.mainHandler)
				{
					lastHandler = this.item.mainHandler;
				}
			}
			targetColliderGroup.collisionHandler.item.lastHandler = (lastHandler ?? targetColliderGroup.collisionHandler.item.lastHandler);
		}

		// Token: 0x060025CE RID: 9678 RVA: 0x00103FD8 File Offset: 0x001021D8
		public virtual bool IsStandingUngrabbedRagdoll()
		{
			return this.isRagdollPart && this.ragdollPart.ragdoll.standingUp && !this.ragdollPart.ragdoll.isGrabbed && !this.ragdollPart.ragdoll.isTkGrabbed;
		}

		// Token: 0x060025CF RID: 9679 RVA: 0x00104028 File Offset: 0x00102228
		public virtual bool IsRagdollAndHitByExcludedLayers(Collider targetCollider)
		{
			if (!this.isRagdollPart)
			{
				return false;
			}
			int layer = targetCollider.gameObject.layer;
			return layer == GameManager.GetLayer(LayerName.MovingItem) || layer == GameManager.GetLayer(LayerName.DroppedItem) || layer == GameManager.GetLayer(LayerName.ItemAndRagdollOnly) || layer == GameManager.GetLayer(LayerName.PlayerHandAndFoot);
		}

		// Token: 0x060025D0 RID: 9680 RVA: 0x00104075 File Offset: 0x00102275
		public virtual bool IsPlayer()
		{
			return this.isRagdollPart && this.ragdollPart.ragdoll.creature.isPlayer;
		}

		// Token: 0x060025D1 RID: 9681 RVA: 0x00104098 File Offset: 0x00102298
		public bool ShouldMeshRaycast()
		{
			return this.isRagdollPart && this.ragdollPart.ragdoll.meshRaycast && this.ragdollPart.ragdoll.creature.data.ragdollData.meshRaycast && Creature.meshRaycast;
		}

		// Token: 0x060025D2 RID: 9682 RVA: 0x001040E8 File Offset: 0x001022E8
		protected int GetPhysicsMaterialHash(bool meshRaycast, Vector3 contactPoint, Vector3 contactNormal, Vector3 impactVelocity, ColliderGroup colliderGroup, Collider collider)
		{
			int physicMaterialHash = -1;
			if (meshRaycast && Creature.meshRaycast)
			{
				this.MeshRaycast(colliderGroup, contactPoint, contactNormal, impactVelocity, ref physicMaterialHash);
			}
			if (physicMaterialHash == -1)
			{
				return Animator.StringToHash(collider.material.name);
			}
			return physicMaterialHash;
		}

		// Token: 0x060025D3 RID: 9683 RVA: 0x00104128 File Offset: 0x00102328
		public virtual void MeshRaycast(ColliderGroup targetColliderGroup, Vector3 contactPoint, Vector3 contactNormal, Vector3 impactVelocity, ref int targetPhysicMaterialHash)
		{
			GameManager gameManager = GameManager.local;
			Vector3 direction = Vector3.Lerp(-contactNormal, impactVelocity.normalized, gameManager.meshRaycastNormalToVelocityRatio);
			Ray ray = new Ray(contactPoint + -direction * gameManager.meshRaycastBackDistance, direction);
			RagdollPart part = targetColliderGroup.collisionHandler.ragdollPart;
			CustomRaycast.RaycastHit hit;
			if (CustomRaycast.RaycastAll(ray, 10f, part.meshpartSkinnedMeshRenderers, out hit, 4))
			{
				Creature.RendererData renderData = part.meshpartRendererList[hit.MeshArrayIndex];
				if (renderData.meshPart.idMapArray != null && renderData.meshPart.idMapArray.nibbleArray.Length > 0)
				{
					int id = renderData.meshPart.idMapArray.GetIdAtUV(hit.TextureCoord.x, hit.TextureCoord.y);
					MaterialData idMapMaterial;
					if (MaterialData.TryGetMaterial(id, out idMapMaterial))
					{
						targetPhysicMaterialHash = idMapMaterial.physicMaterialHash;
						return;
					}
					Color idMapColor = Catalog.gameData.GetIDMapColor(id);
					Debug.LogWarning(string.Format("Cannot found ID map color <color=#{0:X2}{1:X2}{2:X2}>{3}</color> in materials", new object[]
					{
						(byte)(idMapColor.r * 255f),
						(byte)(idMapColor.g * 255f),
						(byte)(idMapColor.b * 255f),
						idMapColor
					}));
					return;
				}
				else if (renderData.meshPart.idMap)
				{
					Color idMapColor2 = renderData.meshPart.idMap.GetPixel((int)(hit.TextureCoord.x * (float)renderData.meshPart.idMap.width), (int)(hit.TextureCoord.y * (float)renderData.meshPart.idMap.height));
					if (idMapColor2.r == 0f && idMapColor2.g == 0f && idMapColor2.b == 0f)
					{
						return;
					}
					idMapColor2.r = (float)Mathf.RoundToInt(idMapColor2.r);
					idMapColor2.g = (float)Mathf.RoundToInt(idMapColor2.g);
					idMapColor2.b = (float)Mathf.RoundToInt(idMapColor2.b);
					MaterialData idMapMaterial2;
					if (MaterialData.TryGetMaterial(idMapColor2, out idMapMaterial2))
					{
						targetPhysicMaterialHash = idMapMaterial2.physicMaterialHash;
						return;
					}
					Debug.LogWarning(string.Format("Cannot found ID map color <color=#{0:X2}{1:X2}{2:X2}>{3}</color> in materials", new object[]
					{
						(byte)(idMapColor2.r * 255f),
						(byte)(idMapColor2.g * 255f),
						(byte)(idMapColor2.b * 255f),
						idMapColor2
					}));
					return;
				}
				else
				{
					targetPhysicMaterialHash = renderData.meshPart.defaultPhysicMaterialHash;
				}
			}
		}

		// Token: 0x060025D4 RID: 9684 RVA: 0x001043E0 File Offset: 0x001025E0
		protected virtual void OnCollisionStay(Collision collision)
		{
			if (!this.active)
			{
				return;
			}
			if (this.enterOnly)
			{
				return;
			}
			this.isColliding = false;
			int frameCount = UpdateManager.frameCount;
			ContactPoint? contactPoint = null;
			ColliderGroup thisColliderGroup = null;
			ColliderGroup otherColliderGroup = null;
			Collider thisCollider = null;
			Collider otherCollider = null;
			int collisionCount = this.collisions.Length;
			for (int i = 0; i < collisionCount; i++)
			{
				if (this.collisions[i].active)
				{
					this.isColliding = true;
					if (this.collisions[i].damageStruct.penetration == DamageStruct.Penetration.None && this.collisions[i].lastStayFrame != frameCount)
					{
						if (this.collisions[i].lastCheckFrame != frameCount)
						{
							this.collisions[i].lastCheckFrame = frameCount;
							this.collisions[i].lastCheckFrameCount = 0;
						}
						if (this.collisions[i].lastCheckFrameCount <= Catalog.gameData.maxCollisionStayCheckPerFrame)
						{
							this.collisions[i].lastCheckFrameCount++;
							if (contactPoint == null)
							{
								contactPoint = new ContactPoint?(collision.GetContact(0));
							}
							ContactPoint contact = contactPoint.Value;
							if (thisCollider == null)
							{
								thisCollider = contact.thisCollider;
							}
							if (this.collisions[i].IsSameSourceColliderGroupByLookup(thisCollider, thisColliderGroup))
							{
								thisColliderGroup = this.collisions[i].sourceColliderGroup;
								if (otherCollider == null)
								{
									otherCollider = contact.otherCollider;
								}
								if (this.collisions[i].IsSameTargetColliderGroupByLookup(otherCollider, otherColliderGroup))
								{
									otherColliderGroup = this.collisions[i].targetColliderGroup;
									this.collisions[i].Stay(collision);
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x060025D5 RID: 9685 RVA: 0x00104574 File Offset: 0x00102774
		public void StopCollisions()
		{
			for (int i = this.collisions.Length - 1; i >= 0; i--)
			{
				if (this.collisions[i].active && this.collisions[i].damageStruct.penetration == DamageStruct.Penetration.None)
				{
					this.StopCollision(this.collisions[i]);
				}
			}
			this.isColliding = false;
		}

		// Token: 0x060025D6 RID: 9686 RVA: 0x001045D0 File Offset: 0x001027D0
		public void StopCollision(CollisionInstance collisionInstance)
		{
			if (collisionInstance.hasEffect)
			{
				collisionInstance.effectInstance.End(true, Catalog.GetCollisionStayRatio(collisionInstance.pressureRelativeVelocity.magnitude));
			}
			collisionInstance.active = false;
			collisionInstance.damageStruct.Reset(false);
			CollisionHandler.CollisionEvent onCollisionStopEvent = this.OnCollisionStopEvent;
			if (onCollisionStopEvent == null)
			{
				return;
			}
			onCollisionStopEvent(collisionInstance);
		}

		// Token: 0x060025D7 RID: 9687 RVA: 0x00104625 File Offset: 0x00102825
		private void OnTriggerEnter(Collider other)
		{
			this.InvokeTriggerEnter(other);
		}

		// Token: 0x060025D8 RID: 9688 RVA: 0x0010462E File Offset: 0x0010282E
		private void OnTriggerExit(Collider other)
		{
			this.InvokeTriggerExit(other);
		}

		// Token: 0x04002516 RID: 9494
		public bool active = true;

		// Token: 0x04002517 RID: 9495
		public bool checkMinVelocity = true;

		// Token: 0x04002518 RID: 9496
		public bool forceFullIntensity;

		// Token: 0x04002519 RID: 9497
		public bool enterOnly;

		// Token: 0x0400251A RID: 9498
		public int forceAllowHitLocomotionLayer = -1;

		// Token: 0x0400251B RID: 9499
		[NonSerialized]
		public List<CollisionHandler> penetratedObjects = new List<CollisionHandler>();

		// Token: 0x0400251C RID: 9500
		[NonSerialized]
		public List<Holder> holders = new List<Holder>();

		// Token: 0x0400251D RID: 9501
		protected float orgMass;

		// Token: 0x0400251E RID: 9502
		protected float orgDrag;

		// Token: 0x0400251F RID: 9503
		protected float orgAngularDrag;

		// Token: 0x04002520 RID: 9504
		protected float orgSleepThreshold;

		// Token: 0x04002521 RID: 9505
		protected bool orgUseGravity;

		// Token: 0x04002522 RID: 9506
		public CollisionInstance[] collisions;

		// Token: 0x04002527 RID: 9511
		[NonSerialized]
		public List<Damager> damagers = new List<Damager>();

		// Token: 0x04002528 RID: 9512
		public bool isColliding;

		// Token: 0x04002529 RID: 9513
		protected bool wasColliding;

		// Token: 0x0400252D RID: 9517
		[NonSerialized]
		public PhysicBody physicBody;

		// Token: 0x0400252E RID: 9518
		[NonSerialized]
		public Item item;

		// Token: 0x0400252F RID: 9519
		[NonSerialized]
		public Breakable breakable;

		// Token: 0x04002530 RID: 9520
		[NonSerialized]
		public RagdollPart ragdollPart;

		// Token: 0x04002531 RID: 9521
		[NonSerialized]
		public Vector3 lastLinearVelocity;

		// Token: 0x04002532 RID: 9522
		[NonSerialized]
		public float lastLinearVelocityMagnitude;

		// Token: 0x04002533 RID: 9523
		[NonSerialized]
		public Vector3 lastAngularVelocity;

		// Token: 0x04002534 RID: 9524
		[NonSerialized]
		public bool isItem;

		// Token: 0x04002535 RID: 9525
		[NonSerialized]
		public bool isBreakable;

		// Token: 0x04002536 RID: 9526
		[NonSerialized]
		public bool isRagdollPart;

		// Token: 0x04002537 RID: 9527
		[NonSerialized]
		public List<CollisionHandler.PhysicModifier> physicModifiers = new List<CollisionHandler.PhysicModifier>();

		// Token: 0x04002538 RID: 9528
		protected float gravityMultiplier;

		// Token: 0x04002539 RID: 9529
		protected bool gravityMultiplierApproxZero;

		// Token: 0x0400253A RID: 9530
		protected bool gravityMultiplierApproxOne;

		// Token: 0x0400253B RID: 9531
		protected Gravity gravity;

		// Token: 0x0400253C RID: 9532
		[NonSerialized]
		public bool isPhysicBodyActive;

		// Token: 0x0400253D RID: 9533
		private List<Damager> lastUnpenetratedList = new List<Damager>();

		// Token: 0x02000A12 RID: 2578
		// (Invoke) Token: 0x0600452F RID: 17711
		public delegate void CollisionEvent(CollisionInstance collisionInstance);

		// Token: 0x02000A13 RID: 2579
		// (Invoke) Token: 0x06004533 RID: 17715
		public delegate void RawCollisionEvent(Collider sourceCollider, Collider targetCollider, ColliderGroup sourceColliderGroup, ColliderGroup targetColliderGroup, Vector3 impactVelocity, Vector3 contactPoint, Vector3 contactNormal);

		// Token: 0x02000A14 RID: 2580
		// (Invoke) Token: 0x06004537 RID: 17719
		public delegate void CollidingEvent(bool colliding);

		// Token: 0x02000A15 RID: 2581
		// (Invoke) Token: 0x0600453B RID: 17723
		public delegate void TriggerEvent(Collider other);

		// Token: 0x02000A16 RID: 2582
		[Serializable]
		public class PhysicModifier
		{
			// Token: 0x0600453E RID: 17726 RVA: 0x0019561F File Offset: 0x0019381F
			public PhysicModifier()
			{
			}

			// Token: 0x0600453F RID: 17727 RVA: 0x00195627 File Offset: 0x00193827
			public PhysicModifier(object handler, float? gravityMultiplier, float massMultiplier, float drag, float angularDrag, float sleepThreshold)
			{
				this.handler = handler;
				this.gravityMultiplier = gravityMultiplier;
				this.massMultiplier = massMultiplier;
				this.drag = drag;
				this.angularDrag = angularDrag;
				this.sleepThreshold = sleepThreshold;
			}

			// Token: 0x040046F4 RID: 18164
			[NonSerialized]
			public object handler;

			// Token: 0x040046F5 RID: 18165
			public float? gravityMultiplier;

			// Token: 0x040046F6 RID: 18166
			public float massMultiplier;

			// Token: 0x040046F7 RID: 18167
			public float drag;

			// Token: 0x040046F8 RID: 18168
			public float angularDrag;

			// Token: 0x040046F9 RID: 18169
			public float sleepThreshold;

			// Token: 0x040046FA RID: 18170
			[NonSerialized]
			public EffectInstance effectInstance;
		}
	}
}
