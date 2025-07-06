using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000268 RID: 616
	[CreateAssetMenu(menuName = "ThunderRoad/Creatures/Golem/Throw config")]
	public class GolemThrow : GolemAbility
	{
		// Token: 0x06001BAC RID: 7084 RVA: 0x000B73D3 File Offset: 0x000B55D3
		private List<ValueDropdownItem<string>> GetAllItemID()
		{
			return Catalog.GetDropdownAllID(Category.Item, "None");
		}

		// Token: 0x06001BAD RID: 7085 RVA: 0x000B73E0 File Offset: 0x000B55E0
		private List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06001BAE RID: 7086 RVA: 0x000B73ED File Offset: 0x000B55ED
		public override bool Allow(GolemController golem)
		{
			return base.Allow(golem) && Time.time - GolemThrow.lastThrowTime > this.throwCooldownDuration && golem.IsSightable(golem.attackTarget, this.throwMaxDistance, this.throwMaxAngle);
		}

		// Token: 0x06001BAF RID: 7087 RVA: 0x000B7428 File Offset: 0x000B5628
		public override void Begin(GolemController golem)
		{
			base.Begin(golem);
			GolemThrow.lastThrowTime = Time.time;
			golem.PerformAttackMotion(GolemController.AttackMotion.Throw, new Action(base.End));
			this.part = golem.GetHand(this.grabArmSide);
			if (this.summonEffectData == null)
			{
				this.summonEffectData = Catalog.GetData<EffectData>(this.summonEffectID, true);
			}
			if (this.throwObjectData == null)
			{
				this.throwObjectData = Catalog.GetData<ItemData>(this.throwObjectID, true);
			}
			if (this.objectEffectData == null)
			{
				this.objectEffectData = Catalog.GetData<EffectData>(this.objectEffectID, true);
			}
			if (this.explosionEffectData == null)
			{
				this.explosionEffectData = Catalog.GetData<EffectData>(this.explosionEffectID, true);
			}
		}

		// Token: 0x06001BB0 RID: 7088 RVA: 0x000B74D4 File Offset: 0x000B56D4
		public override void AbilityStep(int step)
		{
			base.AbilityStep(step);
			switch (step)
			{
			case 0:
			case 3:
				break;
			case 1:
				this.ConjureObject();
				return;
			case 2:
				this.ReleaseObject(true);
				break;
			default:
				return;
			}
		}

		// Token: 0x06001BB1 RID: 7089 RVA: 0x000B7502 File Offset: 0x000B5702
		public override void OnEnd()
		{
			base.OnEnd();
			GolemThrow.lastThrowTime = Time.time;
			this.ReleaseObject(false);
		}

		// Token: 0x06001BB2 RID: 7090 RVA: 0x000B751C File Offset: 0x000B571C
		public virtual void ConjureObject()
		{
			Vector3 anchorPoint = this.part.transform.TransformPoint(this.holdPosition);
			Vector3 spawnPoint = anchorPoint;
			Vector3 castDirection = (this.part.transform.TransformPoint(this.holdPosition) - this.part.transform.position).normalized;
			RaycastHit hit;
			bool hasHitpoint = Physics.Raycast(this.part.transform.position, castDirection, out hit, 10f, this.objectSpawnRaycastMask, QueryTriggerInteraction.Ignore);
			ItemData itemData = this.throwObjectData;
			if (itemData == null)
			{
				return;
			}
			itemData.SpawnAsync(delegate(Item item)
			{
				if (hasHitpoint)
				{
					EffectData effectData = this.summonEffectData;
					if (effectData != null)
					{
						effectData.Spawn(hit.point, Quaternion.identity, null, null, true, null, false, 1f, 1f, Array.Empty<Type>()).Play(0, false, false);
					}
					spawnPoint = hit.point;
				}
				item.transform.position = spawnPoint;
				item.DisallowDespawn = true;
				this.throwingObject = item;
				this.throwingObject.SetPhysicModifier(this, new float?(this.gravityMultiplier), 1f, -1f, -1f, -1f, null);
				ItemMagicAreaProjectile projectile;
				if (item.TryGetComponent<ItemMagicAreaProjectile>(out projectile))
				{
					projectile.explosionEffectData = this.explosionEffectData;
					projectile.OnHit -= this.ObjectCollide;
					projectile.OnHit += this.ObjectCollide;
					projectile.areaRadius = this.explosionRadius;
					projectile.guidance = GuidanceMode.NonGuided;
					projectile.guidanceFunc = null;
					projectile.speed = this.throwVelocity;
					projectile.effectIntensityCurve = this.objectEffectIntensityCurve;
					projectile.Fire(Vector3.zero, this.objectEffectData, null, null, HapticDevice.None, false);
					projectile.doExplosion = false;
				}
				else
				{
					if (this.objectEffectData != null)
					{
						item.StartCoroutine(this.ForceObjectEffect(item, this.objectEffectData));
					}
					if (item.breakable != null)
					{
						item.OnBreakStart -= this.ItemBreak;
						item.OnBreakStart += this.ItemBreak;
					}
					else
					{
						item.mainCollisionHandler.OnCollisionStartEvent -= this.ObjectCollide;
						item.mainCollisionHandler.OnCollisionStartEvent += this.ObjectCollide;
					}
				}
				Rigidbody rb = this.part.GetComponentInParent<Rigidbody>();
				this.holdJoint = rb.gameObject.AddComponent<ConfigurableJoint>();
				this.holdJoint.autoConfigureConnectedAnchor = false;
				this.holdJoint.SetConnectedPhysicBody(item.physicBody);
				this.holdJoint.anchor = rb.transform.InverseTransformPoint(this.part.transform.TransformPoint(this.holdPosition));
				this.holdJoint.connectedAnchor = Vector3.zero;
				this.holdJoint.targetPosition = Vector3.zero;
				JointDrive drive = new JointDrive
				{
					positionSpring = this.holdForce,
					positionDamper = this.holdDamper,
					maximumForce = float.PositiveInfinity
				};
				this.holdJoint.xDrive = drive;
				this.holdJoint.yDrive = drive;
				this.holdJoint.zDrive = drive;
				Collider[] armColliders = rb.GetComponentsInChildren<Collider>();
				Collider[] componentsInChildren = item.GetComponentsInChildren<Collider>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					Collider collider = componentsInChildren[i];
					collider.enabled = false;
					if (hit.collider != null)
					{
						Physics.IgnoreCollision(collider, hit.collider);
						item.DelayedAction(0.5f, delegate
						{
							Physics.IgnoreCollision(collider, hit.collider, false);
						});
					}
					Collider[] array = armColliders;
					for (int j = 0; j < array.Length; j++)
					{
						Physics.IgnoreCollision(array[j], collider, true);
					}
				}
				LightProbeVolume closestVolume;
				LightVolumeReceiver.GetVolumeFromPosition(item.lightVolumeReceiver.transform.position, out closestVolume, this.golem.GetComponentInParent<Area>());
				item.lightVolumeReceiver.TriggerEnter(closestVolume.BoxCollider);
			}, new Vector3?(anchorPoint), new Quaternion?(this.golem.transform.rotation), null, true, null, Item.Owner.None);
		}

		// Token: 0x06001BB3 RID: 7091 RVA: 0x000B75F8 File Offset: 0x000B57F8
		public virtual void ReleaseObject(bool launch = true)
		{
			if (this.throwingObject == null)
			{
				return;
			}
			if (this.holdJoint != null)
			{
				UnityEngine.Object.Destroy(this.holdJoint);
			}
			if (launch)
			{
				Vector3 targetPosition = this.golem.transform.position + this.golem.transform.forward * 15f;
				if (Mathf.Abs(Vector3.SignedAngle(this.golem.transform.forward, (this.golem.attackTarget.position.ToXZ() - this.golem.transform.position.ToXZ()).normalized, this.golem.transform.up)) <= 45f)
				{
					Transform attackTarget = this.golem.attackTarget;
					Ragdoll ragdoll = (attackTarget != null) ? attackTarget.GetComponentInParent<Ragdoll>() : null;
					if (ragdoll != null)
					{
						targetPosition = ragdoll.targetPart.transform.position;
					}
				}
				Vector3 launchVector;
				this.throwingObject.physicBody.CalculateBodyLaunchVector(targetPosition, out launchVector, this.throwVelocity, this.gravityMultiplier);
				this.throwingObject.physicBody.velocity = launchVector;
			}
			Collider[] array = this.throwingObject.GetComponentsInChildren<Collider>();
			int i = 0;
			for (;;)
			{
				int num = i;
				Collider[] array4 = array;
				if (num >= ((array4 != null) ? array4.Length : 0))
				{
					break;
				}
				array[i].enabled = true;
				i++;
			}
			this.throwingObject.RunAfter(delegate()
			{
				Collider[] armColliders = this.part.GetComponentInParent<Rigidbody>().GetComponentsInChildren<Collider>();
				int j = 0;
				for (;;)
				{
					int num2 = j;
					Collider[] array2 = array;
					if (num2 >= ((array2 != null) ? array2.Length : 0))
					{
						break;
					}
					Collider[] array3 = armColliders;
					for (int k = 0; k < array3.Length; k++)
					{
						Physics.IgnoreCollision(array3[k], array[j], false);
					}
					j++;
				}
			}, 0.5f, false);
			ItemMagicAreaProjectile projectile;
			if (this.throwingObject.TryGetComponent<ItemMagicAreaProjectile>(out projectile))
			{
				projectile.doExplosion = true;
				return;
			}
			this.throwingObject.Throw(1f, Item.FlyDetection.Forced);
		}

		// Token: 0x06001BB4 RID: 7092 RVA: 0x000B77C0 File Offset: 0x000B59C0
		private void ObjectCollide(CollisionInstance collision)
		{
			ColliderGroup sourceColliderGroup = collision.sourceColliderGroup;
			CollisionHandler handler = (sourceColliderGroup != null) ? sourceColliderGroup.collisionHandler : null;
			if (handler != null)
			{
				ItemMagicAreaProjectile projectile;
				if (handler.item && handler.item.TryGetComponent<ItemMagicAreaProjectile>(out projectile))
				{
					projectile.OnHit -= this.ObjectCollide;
				}
				handler.OnCollisionStartEvent -= this.ObjectCollide;
			}
			Rigidbody attachedRigidbody = collision.targetCollider.attachedRigidbody;
			Golem hitGolem = (attachedRigidbody != null) ? attachedRigidbody.GetComponentInParent<Golem>() : null;
			if (hitGolem != null)
			{
				hitGolem.StaggerImpact(collision.contactPoint);
			}
			GolemBlast.Explosion(collision.contactPoint, this.explosionRadius, this.explosionLayerMask, this.explosionDamage, this.appliedStatuses, this.explosionForce, this.upwardForceMult, this.forceMode, true, delegate
			{
				this.throwingObject = null;
			});
		}

		// Token: 0x06001BB5 RID: 7093 RVA: 0x000B788C File Offset: 0x000B5A8C
		private void ItemBreak(Breakable breakable)
		{
			breakable.LinkedItem.OnBreakStart -= this.ItemBreak;
			Collision collision = breakable.breakingCollision;
			if (collision != null)
			{
				ContactPoint contact = breakable.breakingCollision.GetContact(0);
				Collider collider = collision.collider;
				Golem hitGolem = (collider != null) ? collider.GetComponentInParent<Golem>() : null;
				if (hitGolem != null)
				{
					hitGolem.StaggerImpact(contact.point);
				}
				EffectData effectData = this.explosionEffectData;
				if (effectData != null)
				{
					EffectInstance effectInstance = effectData.Spawn(contact.point, Quaternion.LookRotation(Vector3.Cross(contact.normal, Vector3.forward), contact.normal), null, null, true, null, false, 1f, 1f, Array.Empty<Type>());
					if (effectInstance != null)
					{
						effectInstance.Play(0, false, false);
					}
				}
			}
			GolemBlast.Explosion(breakable.transform.position, this.explosionRadius, this.explosionLayerMask, this.explosionDamage, this.appliedStatuses, this.explosionForce, this.upwardForceMult, this.forceMode, true, delegate
			{
				this.throwingObject = null;
			});
		}

		// Token: 0x06001BB6 RID: 7094 RVA: 0x000B798B File Offset: 0x000B5B8B
		protected IEnumerator ForceObjectEffect(Item item, EffectData effectData)
		{
			EffectInstance effect = (effectData != null) ? effectData.Spawn(item.transform, null, true, null, false, 0f, 1f, Array.Empty<Type>()) : null;
			if (effect == null)
			{
				yield break;
			}
			effect.Play(0, false, false);
			item.OnDespawnEvent += delegate(EventTime time)
			{
				if (time == EventTime.OnStart)
				{
					EffectInstance effect = effect;
					if (effect == null)
					{
						return;
					}
					effect.End(false, -1f);
				}
			};
			item.OnBreakStart += delegate(Breakable _)
			{
				EffectInstance effect = effect;
				if (effect == null)
				{
					return;
				}
				effect.End(false, -1f);
			};
			while (item && !item.despawning && effect.isPlaying)
			{
				effect.SetIntensity(item.Velocity.magnitude.RemapClamp01(3f, 15f));
				yield return 0;
			}
			yield break;
		}

		// Token: 0x04001A72 RID: 6770
		public static float lastThrowTime;

		// Token: 0x04001A73 RID: 6771
		public string summonEffectID;

		// Token: 0x04001A74 RID: 6772
		public string throwObjectID;

		// Token: 0x04001A75 RID: 6773
		public string objectEffectID;

		// Token: 0x04001A76 RID: 6774
		public AnimationCurve objectEffectIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		// Token: 0x04001A77 RID: 6775
		public LayerMask objectSpawnRaycastMask;

		// Token: 0x04001A78 RID: 6776
		public float throwVelocity = 5f;

		// Token: 0x04001A79 RID: 6777
		public float throwCooldownDuration = 20f;

		// Token: 0x04001A7A RID: 6778
		public float throwMaxDistance = 50f;

		// Token: 0x04001A7B RID: 6779
		public float throwMaxAngle = 30f;

		// Token: 0x04001A7C RID: 6780
		public float gravityMultiplier = 1f;

		// Token: 0x04001A7D RID: 6781
		public Side grabArmSide;

		// Token: 0x04001A7E RID: 6782
		public Vector3 holdPosition = Vector3.zero;

		// Token: 0x04001A7F RID: 6783
		public float holdForce;

		// Token: 0x04001A80 RID: 6784
		public float holdDamper;

		// Token: 0x04001A81 RID: 6785
		public float explosionRadius = 10f;

		// Token: 0x04001A82 RID: 6786
		public float explosionDamage = 20f;

		// Token: 0x04001A83 RID: 6787
		public List<Golem.InflictedStatus> appliedStatuses = new List<Golem.InflictedStatus>();

		// Token: 0x04001A84 RID: 6788
		public float explosionForce = 20f;

		// Token: 0x04001A85 RID: 6789
		public ForceMode forceMode = ForceMode.Impulse;

		// Token: 0x04001A86 RID: 6790
		public float upwardForceMult = 1f;

		// Token: 0x04001A87 RID: 6791
		public LayerMask explosionLayerMask;

		// Token: 0x04001A88 RID: 6792
		public string explosionEffectID;

		// Token: 0x04001A89 RID: 6793
		[NonSerialized]
		public EffectData summonEffectData;

		// Token: 0x04001A8A RID: 6794
		[NonSerialized]
		public ItemData throwObjectData;

		// Token: 0x04001A8B RID: 6795
		[NonSerialized]
		public EffectData objectEffectData;

		// Token: 0x04001A8C RID: 6796
		[NonSerialized]
		public EffectData explosionEffectData;

		// Token: 0x04001A8D RID: 6797
		protected ConfigurableJoint holdJoint;

		// Token: 0x04001A8E RID: 6798
		protected Item throwingObject;

		// Token: 0x04001A8F RID: 6799
		protected Transform part;
	}
}
