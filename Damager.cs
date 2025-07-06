using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000335 RID: 821
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Damager.html")]
	public class Damager : MonoBehaviour
	{
		// Token: 0x14000128 RID: 296
		// (add) Token: 0x0600262A RID: 9770 RVA: 0x00106088 File Offset: 0x00104288
		// (remove) Token: 0x0600262B RID: 9771 RVA: 0x001060C0 File Offset: 0x001042C0
		public event Damager.PenetrationEvent OnPenetrateEvent;

		// Token: 0x14000129 RID: 297
		// (add) Token: 0x0600262C RID: 9772 RVA: 0x001060F8 File Offset: 0x001042F8
		// (remove) Token: 0x0600262D RID: 9773 RVA: 0x00106130 File Offset: 0x00104330
		public event Damager.DepenetrationEvent OnUnpenetrateEvent;

		// Token: 0x1400012A RID: 298
		// (add) Token: 0x0600262E RID: 9774 RVA: 0x00106168 File Offset: 0x00104368
		// (remove) Token: 0x0600262F RID: 9775 RVA: 0x001061A0 File Offset: 0x001043A0
		public event Damager.SkewerEvent OnSkewerEvent;

		// Token: 0x06002630 RID: 9776 RVA: 0x001061D5 File Offset: 0x001043D5
		public Vector3 GetMaxDepthPosition(bool reverted = false)
		{
			return base.transform.position + (reverted ? base.transform.forward : (-base.transform.forward)) * this.penetrationDepth;
		}

		// Token: 0x06002631 RID: 9777 RVA: 0x00106212 File Offset: 0x00104412
		public Vector3 GetMaxDepthPosition(float structDepth, bool reverted = false)
		{
			return base.transform.position + (reverted ? base.transform.forward : (-base.transform.forward)) * structDepth;
		}

		// Token: 0x06002632 RID: 9778 RVA: 0x0010624A File Offset: 0x0010444A
		[ContextMenu("Set colliderOnly from this")]
		public void GetColliderOnlyFromThis()
		{
			this.colliderOnly = base.GetComponent<Collider>();
			this.isColliderOnly = this.colliderOnly;
		}

		// Token: 0x17000250 RID: 592
		// (get) Token: 0x06002633 RID: 9779 RVA: 0x00106269 File Offset: 0x00104469
		public Damager.Type type
		{
			get
			{
				if (this.penetrationLength > 0f)
				{
					return Damager.Type.Slash;
				}
				if (this.penetrationDepth > 0f)
				{
					return Damager.Type.Pierce;
				}
				return Damager.Type.Blunt;
			}
		}

		// Token: 0x06002634 RID: 9780 RVA: 0x0010628A File Offset: 0x0010448A
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x06002635 RID: 9781 RVA: 0x00106297 File Offset: 0x00104497
		protected void Awake()
		{
			this.dmgCollider = base.GetComponent<Collider>();
			this.isColliderOnly = this.colliderOnly;
		}

		// Token: 0x06002636 RID: 9782 RVA: 0x001062B8 File Offset: 0x001044B8
		public void Load(DamagerData damagerData, CollisionHandler collisionHandler)
		{
			this.collisionHandler = collisionHandler;
			if (damagerData == null)
			{
				string str = "Trying to load a null damager onto ";
				string name = base.name;
				string str2 = "! Please check the JSON for item ";
				Item item = collisionHandler.item;
				Debug.LogError(str + name + str2 + ((item != null) ? item.data.id : null));
			}
			else
			{
				this.data = (damagerData.Clone() as DamagerData);
			}
			if (collisionHandler.item)
			{
				collisionHandler.item.OnGrabEvent += this.OnGrab;
				collisionHandler.item.OnUngrabEvent += this.OnRelease;
				collisionHandler.item.OnSnapEvent += this.OnSnap;
			}
			this.loaded = true;
		}

		// Token: 0x06002637 RID: 9783 RVA: 0x0010636C File Offset: 0x0010456C
		public void TryPierceItems([TupleElementNames(new string[]
		{
			"transform",
			"body"
		})] List<ValueTuple<Transform, PhysicBody>> bodyTransforms)
		{
			if (this.type != Damager.Type.Pierce)
			{
				return;
			}
			if (this.colliderGroup != null)
			{
				this.colliderGroup.gameObject.SetActive(false);
			}
			if (this.colliderOnly != null)
			{
				this.colliderOnly.gameObject.SetActive(false);
			}
			Dictionary<Transform, ValueTuple<float, Collider, Vector3>> bodyEnterPoints = new Dictionary<Transform, ValueTuple<float, Collider, Vector3>>();
			int mask = Common.MakeLayerMask(new LayerName[]
			{
				LayerName.Default,
				LayerName.DroppedItem,
				LayerName.MovingItem,
				LayerName.MovingObjectOnly,
				LayerName.ItemAndRagdollOnly,
				LayerName.PlayerLocomotionObject
			});
			RaycastHit[] hits = Physics.RaycastAll(new Ray(this.GetMaxDepthPosition(false), base.transform.forward), this.penetrationDepth, mask, QueryTriggerInteraction.Ignore);
			if (this.colliderGroup != null)
			{
				this.colliderGroup.gameObject.SetActive(true);
			}
			if (this.colliderOnly != null)
			{
				this.colliderOnly.gameObject.SetActive(true);
			}
			foreach (RaycastHit hit in hits)
			{
				PhysicBody pb = hit.GetPhysicBody();
				ValueTuple<float, Collider, Vector3> entryPoint;
				if (pb != null && (!bodyEnterPoints.TryGetValue(pb.transform, out entryPoint) || hit.distance < entryPoint.Item1))
				{
					bodyEnterPoints[pb.transform] = new ValueTuple<float, Collider, Vector3>(hit.distance, hit.collider, hit.point);
				}
			}
			for (int j = 0; j < bodyTransforms.Count; j++)
			{
				ValueTuple<float, Collider, Vector3> entryPoint2;
				if (bodyEnterPoints.TryGetValue(bodyTransforms[j].Item1, out entryPoint2))
				{
					CollisionInstance collisionInstance;
					this.ForcePierce(out collisionInstance, entryPoint2.Item3, entryPoint2.Item2, null, null, null, -1f, null, Damager.PenetrationConditions.DataAllowsPierce | Damager.PenetrationConditions.HasDepth);
				}
				bodyTransforms[j].Item2.isKinematic = false;
			}
		}

		// Token: 0x06002638 RID: 9784 RVA: 0x00106520 File Offset: 0x00104720
		public virtual void UpdatePenetration(CollisionInstance collisionInstance)
		{
			if (!this.loaded)
			{
				return;
			}
			DamagerData data = collisionInstance.damageStruct.damagerData ?? this.data;
			if (!collisionInstance.damageStruct.penetrationJoint || !collisionInstance.damageStruct.penetrationJoint.gameObject.activeInHierarchy || !collisionInstance.damageStruct.penetrationJoint.connectedBody || !collisionInstance.damageStruct.penetrationJoint.connectedBody.gameObject.activeInHierarchy)
			{
				if (!GameManager.isQuitting)
				{
					collisionInstance.damageStruct.damager.UnPenetrate(collisionInstance, false);
				}
				return;
			}
			Vector3 anchorPoint = collisionInstance.damageStruct.penetrationRb.transform.TransformPoint(collisionInstance.damageStruct.penetrationJoint.connectedAnchor);
			Vector3 penetrationPointLocalPosition = collisionInstance.damageStruct.penetrationPoint.InverseTransformPointUnscaled(anchorPoint);
			collisionInstance.damageStruct.penetrationDepth = -penetrationPointLocalPosition.z;
			if (collisionInstance.damageStruct.hitRagdollPart)
			{
				if (!collisionInstance.damageStruct.penetrationDeepReached)
				{
					float deepDepth = 0f;
					if (collisionInstance.damageStruct.damageType == DamageType.Pierce)
					{
						deepDepth = collisionInstance.damageStruct.hitRagdollPart.data.penetrationPierceDeepDepth * data.penetrationDeepDepthMultiplier;
					}
					if (collisionInstance.damageStruct.penetrationDepth > deepDepth)
					{
						collisionInstance.damageStruct.penetrationDeepReached = true;
						float damage;
						if (collisionInstance.damageStruct.damageType == DamageType.Pierce)
						{
							damage = data.penetrationDamage * collisionInstance.damageStruct.hitRagdollPart.data.penetrationPierceDeepDamageMultiplier;
						}
						else
						{
							damage = data.penetrationDamage * collisionInstance.damageStruct.hitRagdollPart.data.penetrationSlashDamageMultiplier;
						}
						if (damage > 0f)
						{
							float num = damage;
							CollisionHandler collisionHandler = this.collisionHandler;
							float? num2;
							if (collisionHandler == null)
							{
								num2 = null;
							}
							else
							{
								Item item = collisionHandler.item;
								if (item == null)
								{
									num2 = null;
								}
								else
								{
									FloatHandler damageMultiplier = item.damageMultiplier;
									num2 = ((damageMultiplier != null) ? new float?(damageMultiplier.Value) : null);
								}
							}
							damage = num * (num2 ?? 1f);
							collisionInstance.damageStruct.damage = damage;
							collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.Damage(collisionInstance);
						}
						if (data.penetrationEffect && collisionInstance.damageStruct.hitRagdollPart.data.penetrationDeepEffectData != null)
						{
							Vector3 pos = collisionInstance.damageStruct.penetrationPoint.position + collisionInstance.damageStruct.penetrationPoint.forward * collisionInstance.damageStruct.penetrationDepth;
							Quaternion rot = Quaternion.LookRotation(-collisionInstance.damageStruct.penetrationPoint.forward, collisionInstance.damageStruct.penetrationPoint.up);
							collisionInstance.damageStruct.penetrationDeepEffectInstance = collisionInstance.damageStruct.hitRagdollPart.data.penetrationDeepEffectData.Spawn(pos, rot, collisionInstance.targetColliderGroup.transform, collisionInstance, true, null, false, 1f, 1f, Array.Empty<System.Type>());
							float penetrationSpeed = Math.Abs(collisionInstance.damageStruct.penetrationDepth - collisionInstance.damageStruct.lastDepth);
							float intensity = Mathf.InverseLerp(0f, 0.02f, penetrationSpeed);
							collisionInstance.damageStruct.penetrationDeepEffectInstance.SetIntensity(intensity);
							collisionInstance.damageStruct.penetrationDeepEffectInstance.Play(0, false, false);
						}
					}
				}
				if (collisionInstance.damageStruct.penetrationDepth > collisionInstance.damageStruct.penetrationDepthReached)
				{
					collisionInstance.damageStruct.penetrationDepthReached = collisionInstance.damageStruct.penetrationDepth;
					if (data.penetrationSkewerDetection && collisionInstance.damageStruct.penetration != DamageStruct.Penetration.Skewer && data.damageModifierData.damageType == DamageType.Pierce && collisionInstance.damageStruct.penetrationDepth > 0.05f && !this.InsideColliderGroup(collisionInstance.targetColliderGroup, collisionInstance.damageStruct.penetrationPoint.position))
					{
						Damager.SkewerEvent onSkewerEvent = this.OnSkewerEvent;
						if (onSkewerEvent != null)
						{
							onSkewerEvent(this, collisionInstance, EventTime.OnStart);
						}
						collisionInstance.damageStruct.penetration = DamageStruct.Penetration.Skewer;
						int targetPhysicMaterial = collisionInstance.targetMaterial.physicMaterialHash;
						Vector3 raycastPos = collisionInstance.damageStruct.penetrationPoint.position + collisionInstance.damageStruct.penetrationPoint.forward * 2f;
						this.colliderGroup.collisionHandler.MeshRaycast(collisionInstance.targetColliderGroup, raycastPos, collisionInstance.damageStruct.penetrationPoint.forward, -collisionInstance.damageStruct.penetrationPoint.forward, ref targetPhysicMaterial);
						MaterialData.Collision materialCollision = collisionInstance.sourceMaterial.GetCollision(targetPhysicMaterial);
						if (materialCollision != null && materialCollision.targetMaterials.Count > 0)
						{
							float penetrationSpeed2 = Math.Abs(collisionInstance.damageStruct.penetrationDepth - collisionInstance.damageStruct.lastDepth);
							float intensity2 = Mathf.Lerp(0.25f, 1f, Mathf.InverseLerp(0f, 0.02f, penetrationSpeed2));
							EffectInstance.Spawn(materialCollision.effects, collisionInstance.damageStruct.penetrationPoint.position, collisionInstance.damageStruct.penetrationPoint.rotation, intensity2, 0f, collisionInstance.targetColliderGroup.transform, collisionInstance, true, null, true).End(false, -1f);
						}
						if (data.penetrationSkewerDamage > 0f)
						{
							collisionInstance.damageStruct.damage = data.penetrationSkewerDamage * collisionInstance.damageStruct.hitRagdollPart.data.penetrationSkewerDamageMultiplier;
							collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.Damage(collisionInstance);
						}
						Damager.SkewerEvent onSkewerEvent2 = this.OnSkewerEvent;
						if (onSkewerEvent2 != null)
						{
							onSkewerEvent2(this, collisionInstance, EventTime.OnEnd);
						}
					}
					if (collisionInstance.damageStruct.hasPenetrationEffect && collisionInstance.damageStruct.penetrationDepth > collisionInstance.damageStruct.penetrationEffectLastDistance)
					{
						Vector3 penetrationPosition = collisionInstance.damageStruct.penetrationPoint.position + -collisionInstance.damageStruct.penetrationPoint.forward * collisionInstance.damageStruct.penetrationEffectLastDistance;
						collisionInstance.damageStruct.penetrationEffectInstance.CollisionStay(penetrationPosition, Quaternion.LookRotation(base.transform.right) * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)), 1f);
						collisionInstance.damageStruct.penetrationEffectInstance.Play(0, false, false);
						collisionInstance.damageStruct.penetrationEffectInstance.CollisionStay(penetrationPosition, Quaternion.LookRotation(-base.transform.right) * Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f)), 1f);
						collisionInstance.damageStruct.penetrationEffectInstance.Play(0, false, false);
						collisionInstance.damageStruct.penetrationEffectLastDistance = collisionInstance.damageStruct.penetrationEffectLastDistance + collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.data.ragdollData.penetrationEffectRepeatDistance;
					}
				}
			}
			this.RefreshJointDrive(collisionInstance);
			if (collisionInstance.hasEffect)
			{
				Vector3 relativeVeloctity = collisionInstance.damageStruct.penetrationRb.velocity - this.collisionHandler.physicBody.velocity;
				float speed = Mathf.InverseLerp(Catalog.gameData.penetrationVelocityEffectRange.x, Catalog.gameData.penetrationVelocityEffectRange.y, relativeVeloctity.magnitude);
				if (collisionInstance.hasEffect)
				{
					if (this.colliderGroup.imbue)
					{
						collisionInstance.effectInstance.CollisionStay(speed, Mathf.InverseLerp(0f, this.colliderGroup.imbue.maxEnergy, this.colliderGroup.imbue.energy));
					}
					else
					{
						collisionInstance.effectInstance.CollisionStay(speed);
					}
				}
			}
			Item item2 = this.collisionHandler.item;
			if (!((item2 != null) ? item2.leftPlayerHand : null))
			{
				Item item3 = this.collisionHandler.item;
				if (!((item3 != null) ? item3.rightPlayerHand : null))
				{
					goto IL_8B5;
				}
			}
			float penetrationPeriod = Catalog.gameData.haptics.penetrationPeriod;
			float penetrationIntensity = Catalog.gameData.haptics.penetrationIntensity;
			if (Mathf.Abs(collisionInstance.damageStruct.penetrationDepth - collisionInstance.damageStruct.lastRumbleDepth) > penetrationPeriod)
			{
				Item item4 = this.collisionHandler.item;
				if ((item4 != null) ? item4.leftPlayerHand : null)
				{
					PlayerControl.handLeft.HapticShort(penetrationIntensity, false);
				}
				else
				{
					Item item5 = this.collisionHandler.item;
					if ((item5 != null) ? item5.rightPlayerHand : null)
					{
						PlayerControl.handRight.HapticShort(penetrationIntensity, false);
					}
				}
				collisionInstance.damageStruct.lastRumbleDepth = collisionInstance.damageStruct.penetrationDepth;
			}
			IL_8B5:
			if (this.penetrationLength > 0f)
			{
				collisionInstance.damageStruct.penetrationCutAxisPos = base.transform.InverseTransformPoint(anchorPoint).y;
				if (collisionInstance.damageStruct.penetrationCutAxisPos > this.penetrationLength * 0.5f || collisionInstance.damageStruct.penetrationCutAxisPos < -(this.penetrationLength * 0.5f))
				{
					this.UnPenetrate(collisionInstance, false);
				}
			}
			if (collisionInstance.damageStruct.penetrationDepth <= 0f && collisionInstance.damageStruct.penetrationDepth < collisionInstance.damageStruct.lastDepth)
			{
				this.UnPenetrate(collisionInstance, false);
			}
			else if (collisionInstance.damageStruct.exitOnMaxDepth && collisionInstance.damageStruct.penetrationDepth >= collisionInstance.damageStruct.damagerDepth && !this.InsideCollision(collisionInstance, this.GetMaxDepthPosition(collisionInstance.damageStruct.damagerDepth, false)))
			{
				this.UnPenetrate(collisionInstance, true);
			}
			collisionInstance.damageStruct.lastDepth = collisionInstance.damageStruct.penetrationDepth;
		}

		// Token: 0x06002639 RID: 9785 RVA: 0x00106ED8 File Offset: 0x001050D8
		public bool InsideCollision(CollisionInstance collisionInstance, Vector3 point)
		{
			if (collisionInstance.targetColliderGroup != null)
			{
				return this.InsideColliderGroup(collisionInstance.targetColliderGroup, point);
			}
			return Utils.InsideCollider(collisionInstance.targetCollider, point);
		}

		// Token: 0x0600263A RID: 9786 RVA: 0x00106F04 File Offset: 0x00105104
		public bool InsideColliderGroup(ColliderGroup targetColliderGroup, Vector3 point)
		{
			using (List<Collider>.Enumerator enumerator = targetColliderGroup.colliders.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (Utils.InsideCollider(enumerator.Current, point))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x0600263B RID: 9787 RVA: 0x00106F60 File Offset: 0x00105160
		public void RefreshJointDrive(CollisionInstance collisionInstance)
		{
			JointDrive jointDrive = default(JointDrive);
			jointDrive.positionSpring = 0f;
			float damperModifier = collisionInstance.damageStruct.penetrationDamperModifier;
			float penetrationDamperIn;
			float penetrationDamperOut;
			if (collisionInstance.sourceColliderGroup.collisionHandler.isItem && collisionInstance.sourceColliderGroup.collisionHandler.item.handlers.Count > 0)
			{
				penetrationDamperIn = this.data.penetrationHeldDamperIn * collisionInstance.damageStruct.materialModifier.penetrationDamperMultiplier * damperModifier;
				penetrationDamperOut = this.data.penetrationHeldDamperOut * collisionInstance.damageStruct.materialModifier.penetrationDamperMultiplier * damperModifier;
			}
			else
			{
				penetrationDamperIn = this.data.penetrationDamper * collisionInstance.damageStruct.materialModifier.penetrationDamperMultiplier * damperModifier;
				penetrationDamperOut = this.data.penetrationDamper * collisionInstance.damageStruct.materialModifier.penetrationDamperMultiplier * damperModifier;
			}
			if (this.data.penetrationTempModifier.HasFlagNoGC(DamagerData.PenetrationTempModifier.OnHit) || (this.data.penetrationTempModifier.HasFlagNoGC(DamagerData.PenetrationTempModifier.OnThrow) && collisionInstance.damageStruct.penetrationFromThrow))
			{
				float startToNormalRatio = Mathf.InverseLerp(0f, this.data.penetrationTempModifierDuration, Time.time - collisionInstance.damageStruct.time);
				if (collisionInstance.damageStruct.penetrationDepth >= collisionInstance.damageStruct.lastDepth)
				{
					jointDrive.positionDamper = Mathf.Lerp(this.data.penetrationTempModifierDamperIn * collisionInstance.damageStruct.materialModifier.penetrationDamperMultiplier, penetrationDamperIn, startToNormalRatio) * damperModifier;
				}
				else
				{
					jointDrive.positionDamper = Mathf.Lerp(this.data.penetrationTempModifierDamperOut * collisionInstance.damageStruct.materialModifier.penetrationDamperMultiplier, penetrationDamperOut, startToNormalRatio) * damperModifier;
				}
			}
			else if (collisionInstance.damageStruct.penetrationDepth >= collisionInstance.damageStruct.lastDepth)
			{
				jointDrive.positionDamper = penetrationDamperIn * damperModifier;
			}
			else
			{
				jointDrive.positionDamper = penetrationDamperOut * damperModifier;
			}
			jointDrive.maximumForce = float.PositiveInfinity;
			collisionInstance.damageStruct.penetrationJoint.zDrive = jointDrive;
			SoftJointLimit softJointLimit = default(SoftJointLimit);
			if (this.data.penetrationShortDepth > 0f)
			{
				softJointLimit.limit = Mathf.Lerp(this.data.penetrationShortDepthAngle, 0f, Mathf.InverseLerp(0f, this.data.penetrationShortDepth, Mathf.Clamp(collisionInstance.damageStruct.penetrationDepth, 0f, float.PositiveInfinity)));
			}
			else
			{
				softJointLimit.limit = 0f;
			}
			collisionInstance.damageStruct.penetrationJoint.highAngularXLimit = softJointLimit;
			softJointLimit.limit = -softJointLimit.limit;
			collisionInstance.damageStruct.penetrationJoint.lowAngularXLimit = softJointLimit;
			if (this.data.penetrationAllowSlide && this.penetrationLength > 0f)
			{
				jointDrive.positionDamper = this.data.penetrationSlideDamper;
				collisionInstance.damageStruct.penetrationJoint.yDrive = jointDrive;
				return;
			}
			collisionInstance.damageStruct.penetrationJoint.angularYLimit = softJointLimit;
		}

		// Token: 0x0600263C RID: 9788 RVA: 0x00107254 File Offset: 0x00105454
		public bool CheckAngles(Vector3 vector, Vector3 normal, Vector3? forward = null, Vector3? up = null, DamagerData.Tier damagerTier = null)
		{
			if (forward == null)
			{
				forward = new Vector3?(base.transform.forward);
			}
			if (damagerTier == null)
			{
				damagerTier = this.data.GetTier(this.collisionHandler);
			}
			if (damagerTier == null)
			{
				string[] array = new string[9];
				array[0] = "Getting damager tier failed for damager ";
				array[1] = base.name;
				array[2] = " [";
				array[3] = this.data.id;
				array[4] = "] on ";
				int num = 5;
				CollisionHandler collisionHandler = this.collisionHandler;
				object obj;
				if (collisionHandler == null)
				{
					obj = null;
				}
				else
				{
					Item item = collisionHandler.item;
					obj = ((item != null) ? item.gameObject : null);
				}
				object obj2;
				if ((obj2 = obj) == null)
				{
					CollisionHandler collisionHandler2 = this.collisionHandler;
					if (collisionHandler2 == null)
					{
						obj2 = null;
					}
					else
					{
						RagdollPart ragdollPart = collisionHandler2.ragdollPart;
						if (ragdollPart == null)
						{
							obj2 = null;
						}
						else
						{
							Ragdoll ragdoll = ragdollPart.ragdoll;
							if (ragdoll == null)
							{
								obj2 = null;
							}
							else
							{
								Creature creature = ragdoll.creature;
								obj2 = ((creature != null) ? creature.gameObject : null);
							}
						}
					}
				}
				array[num] = obj2.name;
				array[6] = " [";
				int num2 = 7;
				CollisionHandler collisionHandler3 = this.collisionHandler;
				object obj3;
				if (collisionHandler3 == null)
				{
					obj3 = null;
				}
				else
				{
					Item item2 = collisionHandler3.item;
					obj3 = ((item2 != null) ? item2.data : null);
				}
				object obj4;
				if ((obj4 = obj3) == null)
				{
					CollisionHandler collisionHandler4 = this.collisionHandler;
					if (collisionHandler4 == null)
					{
						obj4 = null;
					}
					else
					{
						RagdollPart ragdollPart2 = collisionHandler4.ragdollPart;
						if (ragdollPart2 == null)
						{
							obj4 = null;
						}
						else
						{
							Ragdoll ragdoll2 = ragdollPart2.ragdoll;
							if (ragdoll2 == null)
							{
								obj4 = null;
							}
							else
							{
								Creature creature2 = ragdoll2.creature;
								obj4 = ((creature2 != null) ? creature2.data : null);
							}
						}
					}
				}
				array[num2] = obj4.id;
				array[8] = "]";
				Debug.LogError(string.Concat(array));
			}
			return (Vector3.Angle(-normal, forward.Value) <= damagerTier.maxNormalAngle || Vector3.Angle(-normal, -forward.Value) <= damagerTier.maxNormalAngle) && this.CheckAngles(vector, forward, up, damagerTier);
		}

		// Token: 0x0600263D RID: 9789 RVA: 0x001073F4 File Offset: 0x001055F4
		public bool CheckAngles(Vector3 vector, Vector3? forward = null, Vector3? up = null, DamagerData.Tier damagerTier = null)
		{
			if (forward == null)
			{
				forward = new Vector3?(base.transform.forward);
			}
			if (up == null)
			{
				up = new Vector3?(base.transform.up);
			}
			Vector3 right = Vector3.Cross(forward.Value, up.Value);
			Vector3 verticalDir = Vector3.ProjectOnPlane(vector.normalized, right);
			Vector3 horizontalDir = Vector3.ProjectOnPlane(vector.normalized, up.Value);
			float horizontalAngle = Vector3.Angle(horizontalDir.normalized, forward.Value);
			float verticalAngle = Vector3.Angle(verticalDir.normalized, forward.Value);
			if (damagerTier == null)
			{
				damagerTier = this.data.GetTier(this.collisionHandler);
			}
			if (damagerTier == null)
			{
				string[] array = new string[9];
				array[0] = "Getting damager tier failed for damager ";
				array[1] = base.name;
				array[2] = " [";
				array[3] = this.data.id;
				array[4] = "] on ";
				int num = 5;
				CollisionHandler collisionHandler = this.collisionHandler;
				object obj;
				if (collisionHandler == null)
				{
					obj = null;
				}
				else
				{
					Item item = collisionHandler.item;
					obj = ((item != null) ? item.gameObject : null);
				}
				object obj2;
				if ((obj2 = obj) == null)
				{
					CollisionHandler collisionHandler2 = this.collisionHandler;
					if (collisionHandler2 == null)
					{
						obj2 = null;
					}
					else
					{
						RagdollPart ragdollPart = collisionHandler2.ragdollPart;
						if (ragdollPart == null)
						{
							obj2 = null;
						}
						else
						{
							Ragdoll ragdoll = ragdollPart.ragdoll;
							if (ragdoll == null)
							{
								obj2 = null;
							}
							else
							{
								Creature creature = ragdoll.creature;
								obj2 = ((creature != null) ? creature.gameObject : null);
							}
						}
					}
				}
				array[num] = obj2.name;
				array[6] = " [";
				int num2 = 7;
				CollisionHandler collisionHandler3 = this.collisionHandler;
				object obj3;
				if (collisionHandler3 == null)
				{
					obj3 = null;
				}
				else
				{
					Item item2 = collisionHandler3.item;
					obj3 = ((item2 != null) ? item2.data : null);
				}
				object obj4;
				if ((obj4 = obj3) == null)
				{
					CollisionHandler collisionHandler4 = this.collisionHandler;
					if (collisionHandler4 == null)
					{
						obj4 = null;
					}
					else
					{
						RagdollPart ragdollPart2 = collisionHandler4.ragdollPart;
						if (ragdollPart2 == null)
						{
							obj4 = null;
						}
						else
						{
							Ragdoll ragdoll2 = ragdollPart2.ragdoll;
							if (ragdoll2 == null)
							{
								obj4 = null;
							}
							else
							{
								Creature creature2 = ragdoll2.creature;
								obj4 = ((creature2 != null) ? creature2.data : null);
							}
						}
					}
				}
				array[num2] = obj4.id;
				array[8] = "]";
				Debug.LogError(string.Concat(array));
			}
			if (horizontalAngle < damagerTier.maxHorizontalAngle && verticalAngle < damagerTier.maxVerticalAngle)
			{
				return true;
			}
			if (this.direction == Damager.Direction.ForwardAndBackward)
			{
				horizontalAngle = Vector3.Angle(horizontalDir.normalized, -forward.Value);
				verticalAngle = Vector3.Angle(verticalDir.normalized, -forward.Value);
				if (horizontalAngle < damagerTier.maxHorizontalAngle && verticalAngle < damagerTier.maxVerticalAngle)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600263E RID: 9790 RVA: 0x00107630 File Offset: 0x00105830
		public bool CheckPenetration(CollisionInstance collisionInstance, DamagerData data = null, Damager.PenetrationConditions conditions = (Damager.PenetrationConditions)(-1))
		{
			if (data == null)
			{
				data = this.data;
			}
			if (conditions.HasFlagNoGC(Damager.PenetrationConditions.DataAllowsPierce) && !data.penetrationAllowed)
			{
				return false;
			}
			if (collisionInstance.damageStruct.materialModifier == null)
			{
				return false;
			}
			if (conditions.HasFlagNoGC(Damager.PenetrationConditions.MaterialAllowsPierce) && !collisionInstance.damageStruct.materialModifier.allowPenetration)
			{
				return false;
			}
			if (conditions.HasFlagNoGC(Damager.PenetrationConditions.HasDepth) && this.penetrationDepth <= 0f)
			{
				return false;
			}
			if (collisionInstance.damageStruct.penetrationJoint)
			{
				return false;
			}
			if (conditions.HasFlagNoGC(Damager.PenetrationConditions.NotNPCHandler) && this.collisionHandler.item && this.collisionHandler.item.mainHandler && !this.collisionHandler.item.mainHandler.creature.player)
			{
				return false;
			}
			if (conditions.HasFlagNoGC(Damager.PenetrationConditions.VelocityThreshold) && collisionInstance.impactVelocity.magnitude < collisionInstance.damageStruct.materialModifier.penetrationMinVelocity)
			{
				return false;
			}
			if (collisionInstance.targetColliderGroup)
			{
				if (conditions.HasFlagNoGC(Damager.PenetrationConditions.TargetAllowsPierce) && !collisionInstance.targetColliderGroup.data.allowPenetration)
				{
					return false;
				}
				CollisionHandler targetHandler = collisionInstance.targetColliderGroup.collisionHandler;
				Breakable breakable;
				if (targetHandler != null && targetHandler.isItem && targetHandler.item.TryGetComponent<Breakable>(out breakable) && breakable.IsBroken)
				{
					return false;
				}
				if (conditions.HasFlagNoGC(Damager.PenetrationConditions.PartNotBlockingPierce) && this.collisionHandler.isItem && targetHandler != null && targetHandler.isRagdollPart && this.collisionHandler.item.CanPenetratePart(targetHandler.ragdollPart))
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600263F RID: 9791 RVA: 0x001077CC File Offset: 0x001059CC
		public bool TryHit(CollisionInstance collisionInstance)
		{
			if (!this.loaded)
			{
				return false;
			}
			if (this.collisionHandler.physicBody.velocity.magnitude < this.data.minSelfVelocity)
			{
				return false;
			}
			if (this.data.hitDelayByCollider > 0f && collisionInstance.targetCollider == this.lastHitCollider && Time.time - this.lastHitTime < this.data.hitDelayByCollider)
			{
				return false;
			}
			if (this.data.damageModifierData == null)
			{
				return false;
			}
			if (this.direction == Damager.Direction.All || (this.collisionHandler.item && (this.collisionHandler.item.leftNpcHand || this.collisionHandler.item.rightNpcHand)) || this.CheckAngles(collisionInstance.impactVelocity, collisionInstance.contactNormal, null, null, null))
			{
				collisionInstance.damageStruct.materialModifier = this.data.damageModifierData.GetModifier(collisionInstance);
				if (collisionInstance.damageStruct.damageType == DamageType.Unknown)
				{
					collisionInstance.damageStruct.damageType = this.data.damageModifierData.damageType;
					collisionInstance.damageStruct.damager = this;
				}
			}
			else
			{
				if (!this.data.badAngleBluntFallback || this.data.badAngleModifierData == null)
				{
					collisionInstance.damageStruct.Reset(true);
					return false;
				}
				collisionInstance.damageStruct.materialModifier = this.data.badAngleModifierData.GetModifier(collisionInstance);
				if (collisionInstance.damageStruct.materialModifier == null)
				{
					Debug.LogError(string.Concat(new string[]
					{
						"Bad angle fallback set but no modifier found for damager ",
						base.name,
						" with ",
						collisionInstance.sourceMaterial.id,
						" on ",
						collisionInstance.targetMaterial.id
					}));
					collisionInstance.damageStruct.Reset(true);
					return false;
				}
				if (collisionInstance.damageStruct.damageType == DamageType.Unknown)
				{
					collisionInstance.damageStruct.damageType = this.data.damageModifierData.damageType;
					collisionInstance.damageStruct.damager = this;
				}
				collisionInstance.damageStruct.badAngle = true;
			}
			if (((collisionInstance != null) ? collisionInstance.damageStruct.materialModifier : null) != null && this.collisionHandler.checkMinVelocity)
			{
				if (this.data.penetrationAllowed && collisionInstance.damageStruct.materialModifier.allowPenetration)
				{
					if (collisionInstance.impactVelocity.magnitude < Mathf.Min(collisionInstance.damageStruct.materialModifier.minVelocity, collisionInstance.damageStruct.materialModifier.penetrationMinVelocity))
					{
						collisionInstance.damageStruct.Reset(true);
						return false;
					}
				}
				else if (collisionInstance.impactVelocity.magnitude < collisionInstance.damageStruct.materialModifier.minVelocity)
				{
					collisionInstance.damageStruct.Reset(true);
					return false;
				}
			}
			ColliderGroup targetColliderGroup = collisionInstance.targetColliderGroup;
			RagdollPart hitRagdollPart;
			if (targetColliderGroup == null)
			{
				hitRagdollPart = null;
			}
			else
			{
				CollisionHandler collisionHandler = targetColliderGroup.collisionHandler;
				hitRagdollPart = ((collisionHandler != null) ? collisionHandler.ragdollPart : null);
			}
			collisionInstance.damageStruct.hitRagdollPart = hitRagdollPart;
			if (this.collisionHandler.item && !this.collisionHandler.item.handlerArmGrabbed && this.collisionHandler.item.mainHandler && !this.collisionHandler.item.leftPlayerHand && !this.collisionHandler.item.rightPlayerHand && !this.collisionHandler.item.mainHandler.creature.player && collisionInstance.damageStruct.hitRagdollPart && collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.state == Creature.State.Alive && !this.collisionHandler.item.mainHandler.creature.brain.canDamage)
			{
				collisionInstance.damageStruct.Reset(true);
				return false;
			}
			if (collisionInstance.damageStruct.hitRagdollPart)
			{
				collisionInstance.damageStruct.hitBack = (Vector3.Dot((collisionInstance.contactPoint - collisionInstance.damageStruct.hitRagdollPart.transform.position).normalized, collisionInstance.damageStruct.hitRagdollPart.forwardDirection) < 0f);
				if (this.collisionHandler.item && !this.collisionHandler.item.handlerArmGrabbed && this.collisionHandler.item.mainHandler && !this.collisionHandler.item.mainHandler.creature.ragdoll.allowSelfDamage && this.collisionHandler.item.mainHandler.creature.ragdoll == collisionInstance.damageStruct.hitRagdollPart.ragdoll && !this.collisionHandler.item.leftPlayerHand && !this.collisionHandler.item.rightPlayerHand && !this.collisionHandler.item.mainHandler.creature.player)
				{
					collisionInstance.damageStruct.Reset(true);
					return false;
				}
				if (Damager.ShieldSweep(collisionInstance))
				{
					collisionInstance.damageStruct.Reset(true);
					return false;
				}
			}
			else
			{
				ColliderGroup targetColliderGroup2 = collisionInstance.targetColliderGroup;
				Item hitItem;
				if (targetColliderGroup2 == null)
				{
					hitItem = null;
				}
				else
				{
					CollisionHandler collisionHandler2 = targetColliderGroup2.collisionHandler;
					hitItem = ((collisionHandler2 != null) ? collisionHandler2.item : null);
				}
				collisionInstance.damageStruct.hitItem = hitItem;
			}
			if (!collisionInstance.damageStruct.badAngle)
			{
				collisionInstance.damageStruct.damageType = this.data.damageModifierData.damageType;
				collisionInstance.damageStruct.damage = (collisionInstance.damageStruct.baseDamage = this.data.velocityDamageCurve.Evaluate(collisionInstance.impactVelocity.magnitude));
			}
			else
			{
				collisionInstance.damageStruct.damageType = DamageType.Blunt;
				collisionInstance.damageStruct.damage = (collisionInstance.damageStruct.baseDamage = this.data.badAngleDamage);
			}
			collisionInstance.damageStruct.damager = this;
			collisionInstance.damageStruct.time = Time.time;
			collisionInstance.damageStruct.active = true;
			if (Time.time - this.lastSliceTime > this.data.dismembermentNoPenetrationDuration && this.CheckPenetration(collisionInstance, null, (Damager.PenetrationConditions)(-1)))
			{
				collisionInstance.damageStruct.penetration = DamageStruct.Penetration.Hit;
			}
			this.RefreshPushLevel(collisionInstance);
			DamagerData.Tier damagerTier = this.data.GetTier(this.collisionHandler);
			if (damagerTier == null)
			{
				string[] array = new string[9];
				array[0] = "Getting damager tier failed for damager ";
				array[1] = base.name;
				array[2] = " [";
				array[3] = this.data.id;
				array[4] = "] on ";
				int num = 5;
				CollisionHandler collisionHandler3 = this.collisionHandler;
				object obj;
				if (collisionHandler3 == null)
				{
					obj = null;
				}
				else
				{
					Item item = collisionHandler3.item;
					obj = ((item != null) ? item.gameObject : null);
				}
				object obj2;
				if ((obj2 = obj) == null)
				{
					CollisionHandler collisionHandler4 = this.collisionHandler;
					if (collisionHandler4 == null)
					{
						obj2 = null;
					}
					else
					{
						RagdollPart ragdollPart = collisionHandler4.ragdollPart;
						if (ragdollPart == null)
						{
							obj2 = null;
						}
						else
						{
							Ragdoll ragdoll = ragdollPart.ragdoll;
							if (ragdoll == null)
							{
								obj2 = null;
							}
							else
							{
								Creature creature = ragdoll.creature;
								obj2 = ((creature != null) ? creature.gameObject : null);
							}
						}
					}
				}
				array[num] = obj2.name;
				array[6] = " [";
				int num2 = 7;
				CollisionHandler collisionHandler5 = this.collisionHandler;
				object obj3;
				if (collisionHandler5 == null)
				{
					obj3 = null;
				}
				else
				{
					Item item2 = collisionHandler5.item;
					obj3 = ((item2 != null) ? item2.data : null);
				}
				object obj4;
				if ((obj4 = obj3) == null)
				{
					CollisionHandler collisionHandler6 = this.collisionHandler;
					if (collisionHandler6 == null)
					{
						obj4 = null;
					}
					else
					{
						RagdollPart ragdollPart2 = collisionHandler6.ragdollPart;
						if (ragdollPart2 == null)
						{
							obj4 = null;
						}
						else
						{
							Ragdoll ragdoll2 = ragdollPart2.ragdoll;
							if (ragdoll2 == null)
							{
								obj4 = null;
							}
							else
							{
								Creature creature2 = ragdoll2.creature;
								obj4 = ((creature2 != null) ? creature2.data : null);
							}
						}
					}
				}
				array[num2] = obj4.id;
				array[8] = "]";
				Debug.LogError(string.Concat(array));
			}
			if (Damager.dismembermentEnabled && (this.data.dismembermentAllowed || Damager.easyDismemberment) && collisionInstance.damageStruct.hitRagdollPart && collisionInstance.damageStruct.hitRagdollPart.ragdoll.state != Ragdoll.State.NoPhysic && collisionInstance.damageStruct.hitRagdollPart.ragdoll.state != Ragdoll.State.Disabled && collisionInstance.damageStruct.hitRagdollPart.ragdoll.state != Ragdoll.State.Kinematic && (Damager.easyDismemberment || collisionInstance.impactVelocity.magnitude > this.data.dismembermentMinVelocity * damagerTier.dismembermentMinVelocityMultiplier) && collisionInstance.damageStruct.penetration == DamageStruct.Penetration.Hit)
			{
				bool sliceDone = this.TryPerformSlice(collisionInstance, null, null, null);
				if (sliceDone)
				{
					if (collisionInstance.damageStruct.hitRagdollPart.data.sliceForceKill)
					{
						collisionInstance.damageStruct.damage = float.PositiveInfinity;
					}
					collisionInstance.damageStruct.sliceDone = sliceDone;
					collisionInstance.damageStruct.penetration = DamageStruct.Penetration.None;
					collisionInstance.sourceColliderGroup.collisionHandler.physicBody.velocity = collisionInstance.sourceColliderGroup.collisionHandler.lastLinearVelocity;
					collisionInstance.sourceColliderGroup.collisionHandler.physicBody.angularVelocity = collisionInstance.sourceColliderGroup.collisionHandler.lastAngularVelocity;
					this.lastSliceTime = Time.time;
					this.lastSliceRagdollPart = collisionInstance.damageStruct.hitRagdollPart;
				}
			}
			if (collisionInstance.damageStruct.damager.data.selfDamage && this.collisionHandler.isRagdollPart)
			{
				if (collisionInstance.damageStruct.hitRagdollPart)
				{
					collisionInstance.damageStruct.damage = (collisionInstance.damageStruct.baseDamage = this.data.velocityDamageCurve.Evaluate(collisionInstance.impactVelocity.magnitude));
				}
				else
				{
					collisionInstance.damageStruct.damage = (collisionInstance.damageStruct.baseDamage = this.data.staticVelocityDamageCurve.Evaluate(collisionInstance.impactVelocity.magnitude) * this.collisionHandler.ragdollPart.ragdoll.creature.hitEnvironmentDamageModifier);
				}
				ColliderGroup targetColliderGroup3 = collisionInstance.targetColliderGroup;
				Creature creature3;
				if (targetColliderGroup3 == null)
				{
					creature3 = null;
				}
				else
				{
					CollisionHandler collisionHandler7 = targetColliderGroup3.collisionHandler;
					if (collisionHandler7 == null)
					{
						creature3 = null;
					}
					else
					{
						RagdollPart ragdollPart3 = collisionHandler7.ragdollPart;
						if (ragdollPart3 == null)
						{
							creature3 = null;
						}
						else
						{
							Ragdoll ragdoll3 = ragdollPart3.ragdoll;
							creature3 = ((ragdoll3 != null) ? ragdoll3.creature : null);
						}
					}
				}
				Creature hitCreature = creature3;
				if (hitCreature != null && !collisionInstance.targetColliderGroup.collisionHandler.active)
				{
					hitCreature.Damage(collisionInstance);
				}
				collisionInstance.damageStruct.hitRagdollPart = this.collisionHandler.ragdollPart;
				if (collisionInstance.damageStruct.hitRagdollPart)
				{
					collisionInstance.damageStruct.hitBack = (Vector3.Dot((collisionInstance.contactPoint - collisionInstance.damageStruct.hitRagdollPart.transform.position).normalized, collisionInstance.damageStruct.hitRagdollPart.forwardDirection) < 0f);
				}
				collisionInstance.targetColliderGroup = collisionInstance.sourceColliderGroup;
				collisionInstance.contactNormal = -collisionInstance.contactNormal;
				collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * damagerTier.damageMultiplier;
				collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * collisionInstance.damageStruct.materialModifier.damageMultiplier;
				collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * this.collisionHandler.ragdollPart.data.damageMultiplier;
				float damage = collisionInstance.damageStruct.damage;
				FloatHandler floatHandler = this.skillDamageMultiplierHandler;
				collisionInstance.damageStruct.damage = damage * (((floatHandler != null) ? floatHandler.Value : 1f) * this.skillDamageMultiplier);
				this.RefreshPushLevel(collisionInstance);
				this.collisionHandler.ragdollPart.ragdoll.creature.Damage(collisionInstance);
			}
			else
			{
				if (this.collisionHandler.item && this.collisionHandler.item.isThrowed)
				{
					collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * this.data.throwedMultiplier;
				}
				collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * damagerTier.damageMultiplier;
				collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * collisionInstance.damageStruct.materialModifier.damageMultiplier;
				float damage2 = collisionInstance.damageStruct.damage;
				FloatHandler floatHandler2 = this.skillDamageMultiplierHandler;
				collisionInstance.damageStruct.damage = damage2 * (((floatHandler2 != null) ? floatHandler2.Value : 1f) * this.skillDamageMultiplier);
				if (collisionInstance.damageStruct.hitRagdollPart)
				{
					collisionInstance.damageStruct.damage = collisionInstance.damageStruct.damage * collisionInstance.damageStruct.hitRagdollPart.data.damageMultiplier;
				}
				if (collisionInstance.damageStruct.hitItem)
				{
					collisionInstance.damageStruct.hitItem.OnDamageReceived(collisionInstance);
				}
				else if (collisionInstance.damageStruct.hitRagdollPart)
				{
					collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.Damage(collisionInstance);
				}
			}
			if (collisionInstance.damageStruct.hitRagdollPart && collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.state != Creature.State.Alive && this.data.addForce > 0f && collisionInstance.targetCollider.attachedRigidbody && !collisionInstance.targetCollider.attachedRigidbody.isKinematic && collisionInstance.targetCollider.attachedRigidbody.gameObject.activeInHierarchy && (this.data.addForceTargetType == DamagerData.TargetType.All || (this.data.addForceTargetType == DamagerData.TargetType.Creature && collisionInstance.damageStruct.hitRagdollPart) || (this.data.addForceTargetType == DamagerData.TargetType.Object && collisionInstance.damageStruct.hitItem)) && (!this.collisionHandler.item || this.data.addForceState == DamagerData.ObjectState.All || (this.data.addForceState == DamagerData.ObjectState.Flying && this.collisionHandler.item.isFlying) || (this.data.addForceState == DamagerData.ObjectState.Handled && this.collisionHandler.item.IsHanded())))
			{
				base.StartCoroutine(this.AddForceCoroutine(collisionInstance.targetCollider, collisionInstance.impactVelocity, collisionInstance.contactPoint, collisionInstance.damageStruct.hitRagdollPart));
			}
			if (collisionInstance.damageStruct.penetration != DamageStruct.Penetration.None)
			{
				this.Penetrate(collisionInstance, false, true, null, null, -1f, -1f);
			}
			this.lastHitTime = Time.time;
			this.lastHitCollider = collisionInstance.targetCollider;
			return true;
		}

		// Token: 0x06002640 RID: 9792 RVA: 0x00108658 File Offset: 0x00106858
		public bool TryPerformSlice(CollisionInstance collisionInstance, Vector3? forward = null, Vector3? up = null, DamagerData.Tier damagerTier = null)
		{
			bool sliceDone = false;
			if (collisionInstance.damageStruct.hitRagdollPart.sliceAllowed && (Damager.easyDismemberment || this.CheckSlice(collisionInstance.contactPoint, collisionInstance.damageStruct.hitRagdollPart, forward, up, damagerTier)))
			{
				sliceDone = collisionInstance.damageStruct.hitRagdollPart.TrySlice();
			}
			else if (collisionInstance.damageStruct.hitRagdollPart.parentPart && collisionInstance.damageStruct.hitRagdollPart.parentPart.sliceAllowed && this.CheckSlice(collisionInstance.contactPoint, collisionInstance.damageStruct.hitRagdollPart.parentPart, forward, up, damagerTier))
			{
				sliceDone = collisionInstance.damageStruct.hitRagdollPart.parentPart.TrySlice();
			}
			else
			{
				foreach (Ragdoll.Bone ragdollBone in collisionInstance.damageStruct.hitRagdollPart.bone.childs)
				{
					if (ragdollBone.part && ragdollBone.part.sliceAllowed && this.CheckSlice(collisionInstance.contactPoint, ragdollBone.part, forward, up, damagerTier))
					{
						sliceDone = ragdollBone.part.TrySlice();
					}
				}
			}
			return sliceDone;
		}

		// Token: 0x06002641 RID: 9793 RVA: 0x001087A8 File Offset: 0x001069A8
		public bool CheckSlice(Vector3 contactPoint, RagdollPart ragdollPart, Vector3? forward = null, Vector3? up = null, DamagerData.Tier damagerTier = null)
		{
			Vector3 slicePosition;
			Vector3 sliceDirection;
			ragdollPart.GetSlicePositionAndDirection(out slicePosition, out sliceDirection);
			if (Vector3.Project(contactPoint - slicePosition, sliceDirection).magnitude < ragdollPart.sliceWidth)
			{
				if (up == null)
				{
					up = new Vector3?(base.transform.up);
				}
				float upDot = Vector3.Dot(sliceDirection, up.Value);
				if (damagerTier == null)
				{
					damagerTier = this.data.GetTier(this.collisionHandler);
				}
				float num = damagerTier.dismembermentMaxVerticalAngle / 90f;
				CollisionHandler collisionHandler = this.collisionHandler;
				float? num2;
				if (collisionHandler == null)
				{
					num2 = null;
				}
				else
				{
					Item item = collisionHandler.item;
					if (item == null)
					{
						num2 = null;
					}
					else
					{
						FloatHandler sliceAngleMultiplier = item.sliceAngleMultiplier;
						num2 = ((sliceAngleMultiplier != null) ? new float?(sliceAngleMultiplier.Value) : null);
					}
				}
				float maxUpDot = num * (num2 ?? 1f);
				if (upDot < maxUpDot && upDot > -maxUpDot)
				{
					if (forward == null)
					{
						forward = new Vector3?(base.transform.forward);
					}
					float forwardDot = Vector3.Dot(sliceDirection, forward.Value);
					float num3 = damagerTier.dismembermentMaxHorizontalAngle / 90f;
					CollisionHandler collisionHandler2 = this.collisionHandler;
					float? num4;
					if (collisionHandler2 == null)
					{
						num4 = null;
					}
					else
					{
						Item item2 = collisionHandler2.item;
						if (item2 == null)
						{
							num4 = null;
						}
						else
						{
							FloatHandler sliceAngleMultiplier2 = item2.sliceAngleMultiplier;
							num4 = ((sliceAngleMultiplier2 != null) ? new float?(sliceAngleMultiplier2.Value) : null);
						}
					}
					float maxForwardDot = num3 * (num4 ?? 1f);
					if (forwardDot < maxForwardDot && forwardDot > -maxForwardDot)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Make a sweep test to check if the hit arm is holding a shield, and should have parried the hit.
		/// </summary>
		/// <param name="collisionInstance">Collision instance to check for.</param>
		/// <returns>True if the sweep test should have hit a shield, false otherwise.</returns>
		// Token: 0x06002642 RID: 9794 RVA: 0x00108950 File Offset: 0x00106B50
		private static bool ShieldSweep(CollisionInstance collisionInstance)
		{
			if (!collisionInstance.damageStruct.hitRagdollPart)
			{
				return false;
			}
			RagdollPart part = collisionInstance.damageStruct.hitRagdollPart;
			RagdollHand hitHand = null;
			if (part.type == RagdollPart.Type.LeftHand || part.type == RagdollPart.Type.LeftArm)
			{
				hitHand = part.ragdoll.creature.GetHand(Side.Left);
			}
			if (part.type == RagdollPart.Type.RightHand || part.type == RagdollPart.Type.RightArm)
			{
				hitHand = part.ragdoll.creature.GetHand(Side.Right);
			}
			if (!hitHand)
			{
				return false;
			}
			if (!hitHand.grabbedHandle)
			{
				return false;
			}
			if (!hitHand.grabbedHandle.item)
			{
				return false;
			}
			if (hitHand.grabbedHandle.item.data.type != ItemData.Type.Shield)
			{
				return false;
			}
			Item hittingItem = collisionInstance.sourceColliderGroup.collisionHandler.item;
			if (!hittingItem)
			{
				return false;
			}
			Vector3 offset = hittingItem.transform.InverseTransformPoint(collisionInstance.contactPoint);
			Vector3 line = Matrix4x4.TRS(hittingItem.lastPosition, Quaternion.Euler(hittingItem.lastEulers), Vector3.one).MultiplyPoint(offset) - collisionInstance.contactPoint;
			Vector3 dir = line.normalized;
			return Damager.SweepTestCheckForHits(Physics.RaycastNonAlloc(collisionInstance.contactPoint - dir * 0.1f, dir, Damager.shieldSweepRayHits, line.magnitude + 0.2f), hitHand);
		}

		/// <summary>
		/// Checks in the shieldSweepRayHits buffer if any hit is containing the held shield.
		/// </summary>
		/// <param name="hitCount">Number of hits returned by the raycast</param>
		/// <param name="hitHand">Hand of the creature that received the hit</param>
		/// <returns>True if we hit the shield, false otherwise</returns>
		// Token: 0x06002643 RID: 9795 RVA: 0x00108AB4 File Offset: 0x00106CB4
		private static bool SweepTestCheckForHits(int hitCount, RagdollHand hitHand)
		{
			for (int i = 0; i < hitCount; i++)
			{
				Item hitItem;
				if (Damager.shieldSweepRayHits[i].rigidbody && Damager.shieldSweepRayHits[i].rigidbody.TryGetComponent<Item>(out hitItem) && hitItem == hitHand.grabbedHandle.item)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002644 RID: 9796 RVA: 0x00108B14 File Offset: 0x00106D14
		public void RefreshPushLevel(CollisionInstance collisionInstance)
		{
			if (!collisionInstance.damageStruct.hitRagdollPart)
			{
				return;
			}
			if (collisionInstance.damageStruct.materialModifier.pushLevels == null)
			{
				return;
			}
			for (int i = collisionInstance.damageStruct.materialModifier.pushLevels.Length - 1; i >= 0; i--)
			{
				if ((this.collisionHandler.item && this.collisionHandler.item.isThrowed) || (this.collisionHandler.ragdollPart && this.collisionHandler.ragdollPart.isSliced))
				{
					if (collisionInstance.impactVelocity.magnitude > collisionInstance.damageStruct.materialModifier.pushLevels[i].throwVelocity)
					{
						collisionInstance.damageStruct.pushLevel = i;
						return;
					}
				}
				else if (collisionInstance.impactVelocity.magnitude > collisionInstance.damageStruct.materialModifier.pushLevels[i].hitVelocity)
				{
					collisionInstance.damageStruct.pushLevel = i;
					return;
				}
			}
			int pushLevel = collisionInstance.damageStruct.pushLevel;
			CollisionHandler collisionHandler = this.collisionHandler;
			int? num;
			if (collisionHandler == null)
			{
				num = null;
			}
			else
			{
				Item item = collisionHandler.item;
				if (item == null)
				{
					num = null;
				}
				else
				{
					IntAddHandler pushLevelMultiplier = item.pushLevelMultiplier;
					num = ((pushLevelMultiplier != null) ? new int?(pushLevelMultiplier.Value) : null);
				}
			}
			int? num2 = num;
			collisionInstance.damageStruct.pushLevel = pushLevel + num2.GetValueOrDefault();
		}

		// Token: 0x06002645 RID: 9797 RVA: 0x00108C74 File Offset: 0x00106E74
		public bool TryHitByPressure(CollisionInstance collisionInstance)
		{
			if (!this.loaded)
			{
				return false;
			}
			if (this.data.hitDelayByCollider > 0f && collisionInstance.targetCollider == this.lastHitCollider && Time.time - this.lastHitTime < this.data.hitDelayByCollider)
			{
				return false;
			}
			if (collisionInstance.damageStruct.penetration != DamageStruct.Penetration.None)
			{
				return false;
			}
			if (this.penetrationLength <= 0f && this.penetrationDepth <= 0f)
			{
				return false;
			}
			if (collisionInstance.targetColliderGroup && !collisionInstance.targetColliderGroup.data.allowPenetration)
			{
				return false;
			}
			if (this.data.damageModifierData == null)
			{
				return false;
			}
			collisionInstance.damageStruct.materialModifier = this.data.damageModifierData.GetModifier(collisionInstance);
			if (collisionInstance.damageStruct.materialModifier == null)
			{
				return false;
			}
			if (!this.data.penetrationAllowed || !collisionInstance.damageStruct.materialModifier.allowPenetration)
			{
				return false;
			}
			if (!this.data.penetrationPressureAllowed || !collisionInstance.damageStruct.materialModifier.pressureAllowed || this.penetrationDepth <= 0f)
			{
				return false;
			}
			if (!this.CheckAngles(collisionInstance.pressureForce, null, null, null))
			{
				return false;
			}
			if (this.penetrationLength > 0f)
			{
				float dot = Vector3.Dot(collisionInstance.pressureRelativeVelocity.normalized, base.transform.up);
				if (dot < 1f - this.data.penetrationPressureMaxDot && dot > -1f + this.data.penetrationPressureMaxDot)
				{
					return false;
				}
			}
			if (collisionInstance.pressureForce.magnitude >= this.data.penetrationPressureForceCurve.Evaluate(collisionInstance.pressureRelativeVelocity.magnitude) * collisionInstance.damageStruct.materialModifier.pressureForceMultiplier && (!this.collisionHandler.item || !this.collisionHandler.item.lastHandler || this.collisionHandler.item.lastHandler.creature.player))
			{
				collisionInstance.damageStruct.penetration = DamageStruct.Penetration.Pressure;
			}
			if (collisionInstance.damageStruct.penetration == DamageStruct.Penetration.None)
			{
				return false;
			}
			collisionInstance.damageStruct.damage = (collisionInstance.damageStruct.baseDamage = 0f);
			collisionInstance.damageStruct.damager = this;
			collisionInstance.damageStruct.damageType = this.data.damageModifierData.damageType;
			collisionInstance.damageStruct.time = Time.time;
			collisionInstance.damageStruct.active = true;
			ColliderGroup targetColliderGroup = collisionInstance.targetColliderGroup;
			collisionInstance.damageStruct.hitRagdollPart = ((targetColliderGroup != null) ? targetColliderGroup.collisionHandler.ragdollPart : null);
			ColliderGroup targetColliderGroup2 = collisionInstance.targetColliderGroup;
			collisionInstance.damageStruct.hitItem = ((targetColliderGroup2 != null) ? targetColliderGroup2.collisionHandler.item : null);
			if (collisionInstance.damageStruct.hitItem)
			{
				collisionInstance.damageStruct.hitItem.OnDamageReceived(collisionInstance);
			}
			if (collisionInstance.damageStruct.hitRagdollPart)
			{
				collisionInstance.damageStruct.hitBack = (Vector3.Dot((collisionInstance.contactPoint - collisionInstance.damageStruct.hitRagdollPart.transform.position).normalized, collisionInstance.damageStruct.hitRagdollPart.forwardDirection) < 0f);
			}
			this.Penetrate(collisionInstance, true, true, null, null, -1f, -1f);
			this.lastHitTime = Time.time;
			this.lastHitCollider = collisionInstance.targetCollider;
			return true;
		}

		// Token: 0x06002646 RID: 9798 RVA: 0x0010901A File Offset: 0x0010721A
		private IEnumerator AddForceCoroutine(Collider targetCollider, Vector3 impactVelocity, Vector3 contactPoint, RagdollPart hitPart)
		{
			Vector3 localContactPoint = targetCollider.attachedRigidbody.transform.InverseTransformPoint(contactPoint);
			float timeElapsed = 0f;
			Vector3 velocity = this.data.addForceNormalize ? impactVelocity.normalized : impactVelocity;
			while (timeElapsed <= this.data.addForceDuration)
			{
				if (hitPart)
				{
					using (List<RagdollPart>.Enumerator enumerator = hitPart.ragdollRegion.parts.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							RagdollPart ragdollPart = enumerator.Current;
							if (ragdollPart.HasCollider(targetCollider) && this.data.addForceRagdollPartMultiplier > 0f && !ragdollPart.physicBody.isKinematic)
							{
								Vector3 force = velocity * (this.data.addForce * this.data.addForceRagdollPartMultiplier * ((TimeManager.slowMotionState == TimeManager.SlowMotionState.Disabled) ? 1f : this.data.addForceSlowMoMultiplier) * Time.unscaledDeltaTime);
								ragdollPart.physicBody.AddForceAtPosition(force, targetCollider.attachedRigidbody.transform.TransformPoint(localContactPoint), this.data.addForceMode);
							}
							else if (this.data.addForceRagdollOtherMultiplier > 0f && !ragdollPart.physicBody.isKinematic)
							{
								Vector3 force2 = velocity * (this.data.addForce * this.data.addForceRagdollOtherMultiplier * ((TimeManager.slowMotionState == TimeManager.SlowMotionState.Disabled) ? 1f : this.data.addForceSlowMoMultiplier) * Time.unscaledDeltaTime);
								ragdollPart.physicBody.AddForceAtPosition(force2, targetCollider.attachedRigidbody.transform.TransformPoint(localContactPoint), this.data.addForceMode);
							}
						}
						goto IL_2A5;
					}
					goto IL_212;
				}
				goto IL_212;
				IL_2A5:
				timeElapsed += Time.deltaTime;
				yield return null;
				continue;
				IL_212:
				if (targetCollider != null && targetCollider.attachedRigidbody)
				{
					Vector3 force3 = velocity * (this.data.addForce * ((TimeManager.slowMotionState == TimeManager.SlowMotionState.Disabled) ? 1f : this.data.addForceSlowMoMultiplier) * Time.unscaledDeltaTime);
					targetCollider.attachedRigidbody.AddForceAtPosition(force3, targetCollider.attachedRigidbody.transform.TransformPoint(localContactPoint), this.data.addForceMode);
					goto IL_2A5;
				}
				goto IL_2A5;
			}
			yield break;
		}

		// Token: 0x06002647 RID: 9799 RVA: 0x00109048 File Offset: 0x00107248
		protected void OnGrab(Handle handle, RagdollHand ragdollHand)
		{
			for (int i = this.collisionHandler.collisions.Length - 1; i >= 0; i--)
			{
				if (this.collisionHandler.collisions[i].damageStruct.active && !(this.collisionHandler.collisions[i].damageStruct.damager != this) && this.collisionHandler.collisions[i].damageStruct.penetration != DamageStruct.Penetration.None && this.collisionHandler.collisions[i].damageStruct.hitRagdollPart && this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.ragdoll.creature.player)
				{
					this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.ragdoll.creature.turnRelativeToHand = false;
					if (ragdollHand.side == Side.Left && (this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.type == RagdollPart.Type.LeftArm || this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.type == RagdollPart.Type.LeftHand))
					{
						this.UnPenetrate(this.collisionHandler.collisions[i], false);
					}
					if (ragdollHand.side == Side.Right && (this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.type == RagdollPart.Type.RightArm || this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.type == RagdollPart.Type.RightHand))
					{
						this.UnPenetrate(this.collisionHandler.collisions[i], false);
					}
				}
			}
		}

		// Token: 0x06002648 RID: 9800 RVA: 0x00109200 File Offset: 0x00107400
		protected void OnRelease(Handle handle, RagdollHand ragdollHand, bool throwing)
		{
			for (int i = this.collisionHandler.collisions.Length - 1; i >= 0; i--)
			{
				if (this.collisionHandler.collisions[i].damageStruct.active && !(this.collisionHandler.collisions[i].damageStruct.damager != this))
				{
					if (this.collisionHandler.collisions[i].damageStruct.penetrationJoint)
					{
						handle.item.StopThrowing();
						handle.item.StopFlying();
						handle.item.IgnoreIsMoving();
					}
					if (this.collisionHandler.collisions[i].damageStruct.penetration != DamageStruct.Penetration.None && this.collisionHandler.collisions[i].damageStruct.hitRagdollPart)
					{
						if (this.collisionHandler.isItem)
						{
							this.collisionHandler.item.transform.SetParent(this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.ragdoll.creature.transform, true);
						}
						if (this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.ragdoll.creature.player)
						{
							this.collisionHandler.collisions[i].damageStruct.hitRagdollPart.ragdoll.creature.turnRelativeToHand = true;
						}
					}
				}
			}
		}

		// Token: 0x06002649 RID: 9801 RVA: 0x00109384 File Offset: 0x00107584
		protected void OnSnap(Holder holder)
		{
			for (int i = this.collisionHandler.collisions.Length - 1; i >= 0; i--)
			{
				if (this.collisionHandler.collisions[i].damageStruct.active && !(this.collisionHandler.collisions[i].damageStruct.damager != this) && this.collisionHandler.collisions[i].damageStruct.penetrationJoint)
				{
					this.UnPenetrate(this.collisionHandler.collisions[i], false);
				}
			}
		}

		// Token: 0x0600264A RID: 9802 RVA: 0x00109418 File Offset: 0x00107618
		public bool ForcePierce(out CollisionInstance pierceCollision, Vector3 point, Collider targetCollider, Collider sourceCollider = null, Vector3? forward = null, Vector3? up = null, float depthOverride = -1f, DamagerData data = null, Damager.PenetrationConditions conditions = Damager.PenetrationConditions.DataAllowsPierce | Damager.PenetrationConditions.HasDepth)
		{
			if (sourceCollider == null)
			{
				sourceCollider = Common.GetClosest<Collider>(this.colliderGroup.colliders, base.transform.position, false);
			}
			MaterialData sourceMaterial;
			MaterialData targetMaterial;
			MaterialData.TryGetMaterials(Animator.StringToHash(sourceCollider.material.name), Animator.StringToHash(targetCollider.material.name), out sourceMaterial, out targetMaterial);
			if (forward == null)
			{
				forward = new Vector3?(base.transform.forward);
			}
			if (up == null)
			{
				up = new Vector3?(base.transform.up);
			}
			if (data == null)
			{
				data = this.data;
			}
			pierceCollision = new CollisionInstance
			{
				active = true,
				sourceMaterial = sourceMaterial,
				sourceCollider = sourceCollider,
				sourceColliderGroup = this.colliderGroup,
				targetMaterial = targetMaterial,
				targetCollider = targetCollider,
				targetColliderGroup = targetCollider.GetComponentInParent<ColliderGroup>(),
				contactPoint = point,
				contactNormal = -forward.Value,
				pressureForce = forward.Value,
				damageStruct = new DamageStruct
				{
					active = true,
					damager = this,
					damagerData = data,
					damageType = DamageType.Pierce,
					penetration = DamageStruct.Penetration.Hit
				}
			};
			pierceCollision.damageStruct.materialModifier = data.damageModifierData.GetModifier(pierceCollision);
			pierceCollision.impactVelocity = pierceCollision.damageStruct.materialModifier.penetrationMinVelocity * 1.1f * base.transform.forward;
			if (pierceCollision.targetColliderGroup != null && pierceCollision.targetColliderGroup.collisionHandler != null)
			{
				pierceCollision.damageStruct.hitItem = pierceCollision.targetColliderGroup.collisionHandler.item;
				pierceCollision.damageStruct.hitRagdollPart = pierceCollision.targetColliderGroup.collisionHandler.ragdollPart;
			}
			int index;
			if (this.CheckPenetration(pierceCollision, data, conditions) && this.collisionHandler.TryGetFreeCollisionIndexAndNotSameGroup(pierceCollision.sourceCollider, pierceCollision.sourceColliderGroup, pierceCollision.targetCollider, pierceCollision.targetColliderGroup, out index))
			{
				this.collisionHandler.collisions[index] = pierceCollision;
				this.Penetrate(pierceCollision, true, false, forward, up, depthOverride, -1f);
				return true;
			}
			return false;
		}

		// Token: 0x0600264B RID: 9803 RVA: 0x0010965C File Offset: 0x0010785C
		public void Penetrate(CollisionInstance collisionInstance, bool usePressure, bool realCollision = true, Vector3? forward = null, Vector3? up = null, float depth = -1f, float velocityMultiplier = -1f)
		{
			if (collisionInstance.damageStruct.penetrationJoint)
			{
				return;
			}
			Damager.PenetrationEvent onPenetrateEvent = this.OnPenetrateEvent;
			if (onPenetrateEvent != null)
			{
				onPenetrateEvent(this, collisionInstance, EventTime.OnStart);
			}
			if (velocityMultiplier <= 0f)
			{
				velocityMultiplier = this.data.penetrationInitialVelocityMultiplier;
			}
			collisionInstance.sourceColliderGroup.collisionHandler.physicBody.velocity = collisionInstance.sourceColliderGroup.collisionHandler.lastLinearVelocity * velocityMultiplier;
			collisionInstance.sourceColliderGroup.collisionHandler.physicBody.angularVelocity = collisionInstance.sourceColliderGroup.collisionHandler.lastAngularVelocity;
			collisionInstance.damageStruct.penetrationPoint = new GameObject("PenPoint").transform;
			collisionInstance.damageStruct.penetrationPoint.SetParent(base.transform);
			Vector3 force = usePressure ? collisionInstance.pressureForce : collisionInstance.impactVelocity;
			collisionInstance.damageStruct.penetrationPoint.position = (realCollision ? collisionInstance.contactPoint : base.transform.position);
			if (Vector3.Angle(base.transform.forward, force.normalized) < 90f)
			{
				collisionInstance.damageStruct.penetrationPoint.rotation = Quaternion.LookRotation(base.transform.forward, base.transform.up);
			}
			else
			{
				collisionInstance.damageStruct.penetrationPoint.rotation = Quaternion.LookRotation(-base.transform.forward, base.transform.up);
			}
			collisionInstance.damageStruct.penetrationPoint.localPosition = new Vector3(0f, (this.penetrationLength > 0f) ? collisionInstance.damageStruct.penetrationPoint.localPosition.y : 0f, collisionInstance.damageStruct.penetrationPoint.localPosition.z);
			if (this.collisionHandler.isItem && !this.collisionHandler.item.worldAttached && collisionInstance.damageStruct.hitRagdollPart)
			{
				this.collisionHandler.item.transform.SetParent(collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.transform, true);
				if (collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.data.ragdollData.hasPenetrationEffect)
				{
					collisionInstance.damageStruct.penetrationEffectInstance = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.data.ragdollData.penetrationEffectData.Spawn(collisionInstance.contactPoint, Quaternion.identity, base.transform, collisionInstance, true, null, false, 1f, 1f, Array.Empty<System.Type>());
					collisionInstance.damageStruct.penetrationEffectInstance.SetIntensity(1f);
					collisionInstance.damageStruct.hasPenetrationEffect = true;
				}
			}
			Ray ray = new Ray(collisionInstance.damageStruct.penetrationPoint.position + collisionInstance.damageStruct.penetrationPoint.forward * 1f, -collisionInstance.damageStruct.penetrationPoint.forward);
			RaycastHit raycastHit;
			if (collisionInstance.sourceCollider.Raycast(ray, out raycastHit, 2f))
			{
				collisionInstance.damageStruct.penetrationPoint.position = raycastHit.point;
			}
			if (this.collisionHandler.item)
			{
				collisionInstance.damageStruct.penetrationFromThrow = this.collisionHandler.item.isThrowed;
				this.collisionHandler.item.isPenetrating = true;
				this.collisionHandler.item.disableSnap = true;
				this.collisionHandler.item.StopThrowing();
				this.collisionHandler.item.StopFlying();
			}
			if (collisionInstance.targetCollider.attachedRigidbody)
			{
				collisionInstance.damageStruct.penetrationRb = collisionInstance.targetCollider.attachedRigidbody;
			}
			else
			{
				collisionInstance.damageStruct.penetrationRb = new GameObject("PenRb").AddComponent<Rigidbody>();
				collisionInstance.damageStruct.penetrationRb.isKinematic = true;
				collisionInstance.damageStruct.penetrationRb.detectCollisions = false;
				collisionInstance.damageStruct.penetrationRb.transform.position = collisionInstance.contactPoint;
				collisionInstance.damageStruct.penetrationRb.transform.rotation = base.transform.rotation;
				collisionInstance.damageStruct.penetrationRb.transform.SetParent(collisionInstance.targetCollider.transform, true);
				collisionInstance.damageStruct.penetrationTempRb = true;
			}
			collisionInstance.damageStruct.penetrationJoint = this.collisionHandler.physicBody.gameObject.AddComponent<ConfigurableJoint>();
			collisionInstance.damageStruct.penetrationJoint.enableCollision = false;
			collisionInstance.damageStruct.penetrationJoint.connectedBody = collisionInstance.damageStruct.penetrationRb;
			collisionInstance.damageStruct.penetrationJoint.anchor = this.collisionHandler.physicBody.transform.InverseTransformPoint(collisionInstance.damageStruct.penetrationPoint.position);
			collisionInstance.damageStruct.penetrationJoint.axis = this.collisionHandler.physicBody.transform.InverseTransformDirection(collisionInstance.damageStruct.penetrationPoint.right);
			collisionInstance.damageStruct.penetrationJoint.secondaryAxis = this.collisionHandler.physicBody.transform.InverseTransformDirection(collisionInstance.damageStruct.penetrationPoint.up);
			collisionInstance.damageStruct.penetrationJoint.autoConfigureConnectedAnchor = false;
			collisionInstance.damageStruct.penetrationJoint.connectedAnchor = collisionInstance.damageStruct.penetrationRb.transform.InverseTransformPoint(collisionInstance.contactPoint);
			if (this.penetrationExitOnMaxDepth)
			{
				collisionInstance.damageStruct.penetrationJoint.zMotion = ConfigurableJointMotion.Free;
			}
			else
			{
				collisionInstance.damageStruct.penetrationJoint.zMotion = ConfigurableJointMotion.Limited;
			}
			SoftJointLimit softJointLimit = collisionInstance.damageStruct.penetrationJoint.linearLimit;
			float depthLimit = (depth > 0f) ? depth : this.penetrationDepth;
			if (collisionInstance.damageStruct.materialModifier.penetrationVelocityMaxDepth)
			{
				float extraVelocity = Mathf.Clamp(force.magnitude - collisionInstance.damageStruct.materialModifier.penetrationMinVelocity, 0f, float.PositiveInfinity);
				softJointLimit.limit = Mathf.Clamp(collisionInstance.damageStruct.materialModifier.penetrationVelocityMaxDepthCurve.Evaluate(extraVelocity), 0f, depthLimit);
			}
			else
			{
				softJointLimit.limit = depthLimit;
			}
			collisionInstance.damageStruct.penetrationJoint.linearLimit = softJointLimit;
			if (this.data.penetrationAllowSlide && this.penetrationLength > 0f)
			{
				collisionInstance.damageStruct.penetrationJoint.yMotion = ConfigurableJointMotion.Free;
			}
			else
			{
				collisionInstance.damageStruct.penetrationJoint.yMotion = ConfigurableJointMotion.Locked;
			}
			this.RefreshJointDrive(collisionInstance);
			collisionInstance.damageStruct.penetrationJoint.xMotion = ConfigurableJointMotion.Locked;
			collisionInstance.damageStruct.penetrationJoint.angularXMotion = ConfigurableJointMotion.Limited;
			collisionInstance.damageStruct.penetrationJoint.angularYMotion = ConfigurableJointMotion.Limited;
			collisionInstance.damageStruct.penetrationJoint.angularZMotion = ConfigurableJointMotion.Locked;
			if (this.isColliderOnly)
			{
				if (collisionInstance.damageStruct.hitRagdollPart)
				{
					collisionInstance.damageStruct.hitRagdollPart.ragdoll.IgnoreCollision(this.colliderOnly, true, (RagdollPart.Type)0);
				}
				else
				{
					Physics.IgnoreCollision(collisionInstance.targetCollider, this.colliderOnly, true);
				}
			}
			else
			{
				foreach (Collider collider in this.colliderGroup.colliders)
				{
					if (collisionInstance.damageStruct.hitRagdollPart)
					{
						collisionInstance.damageStruct.hitRagdollPart.ragdoll.IgnoreCollision(collider, true, (RagdollPart.Type)0);
					}
					else
					{
						Physics.IgnoreCollision(collisionInstance.targetCollider, collider, true);
					}
				}
			}
			if (collisionInstance.targetColliderGroup && collisionInstance.targetColliderGroup.collisionHandler && !collisionInstance.targetColliderGroup.collisionHandler.penetratedObjects.Contains(this.collisionHandler))
			{
				collisionInstance.targetColliderGroup.collisionHandler.penetratedObjects.Add(this.collisionHandler);
			}
			Vector3 anchorPoint = collisionInstance.damageStruct.penetrationRb.transform.TransformPoint(collisionInstance.damageStruct.penetrationJoint.connectedAnchor);
			Vector3 penetrationPointLocalPosition = collisionInstance.damageStruct.penetrationPoint.InverseTransformPoint(anchorPoint);
			collisionInstance.damageStruct.penetrationDepth = -penetrationPointLocalPosition.z;
			collisionInstance.damageStruct.lastDepth = -penetrationPointLocalPosition.z;
			collisionInstance.damageStruct.exitOnMaxDepth = this.penetrationExitOnMaxDepth;
			collisionInstance.damageStruct.damagerDepth = this.penetrationDepth;
			Damager.PenetrationEvent onPenetrateEvent2 = this.OnPenetrateEvent;
			if (onPenetrateEvent2 == null)
			{
				return;
			}
			onPenetrateEvent2(this, collisionInstance, EventTime.OnEnd);
		}

		// Token: 0x0600264C RID: 9804 RVA: 0x00109F34 File Offset: 0x00108134
		public void OverridePenetrationSettings(CollisionInstance collisionInstance, bool exitOnMaxDepth = false, float overrideDepth = -1f, float damperModifier = 1f)
		{
			if (collisionInstance.damageStruct.exitOnMaxDepth != exitOnMaxDepth)
			{
				collisionInstance.damageStruct.penetrationJoint.zMotion = (exitOnMaxDepth ? ConfigurableJointMotion.Free : ConfigurableJointMotion.Limited);
			}
			collisionInstance.damageStruct.exitOnMaxDepth = exitOnMaxDepth;
			if (overrideDepth > 0f)
			{
				collisionInstance.damageStruct.damagerDepth = overrideDepth;
			}
			collisionInstance.damageStruct.penetrationDamperModifier = damperModifier;
		}

		// Token: 0x0600264D RID: 9805 RVA: 0x00109F94 File Offset: 0x00108194
		public void UnPenetrate(CollisionInstance collisionInstance, bool wentThrough = false)
		{
			Damager.DepenetrationEvent onUnpenetrateEvent = this.OnUnpenetrateEvent;
			if (onUnpenetrateEvent != null)
			{
				onUnpenetrateEvent(this, collisionInstance, wentThrough, EventTime.OnStart);
			}
			if (collisionInstance.damageStruct.hasPenetrationEffect)
			{
				collisionInstance.damageStruct.penetrationEffectInstance.Despawn();
				collisionInstance.damageStruct.hasPenetrationEffect = false;
			}
			if (collisionInstance.damageStruct.penetrationDeepEffectInstance != null)
			{
				collisionInstance.damageStruct.penetrationDeepEffectInstance.End(false, -1f);
			}
			UnityEngine.Object.Destroy(collisionInstance.damageStruct.penetrationJoint);
			if (collisionInstance.damageStruct.penetrationPoint)
			{
				UnityEngine.Object.Destroy(collisionInstance.damageStruct.penetrationPoint.gameObject);
			}
			if (collisionInstance.damageStruct.penetrationTempRb)
			{
				UnityEngine.Object.Destroy(collisionInstance.damageStruct.penetrationRb.gameObject);
				collisionInstance.damageStruct.penetrationTempRb = false;
			}
			collisionInstance.damageStruct.penetrationDepth = 0f;
			collisionInstance.damageStruct.penetration = DamageStruct.Penetration.None;
			collisionInstance.damageStruct.penetrationRb = null;
			if (this.collisionHandler.isItem)
			{
				this.collisionHandler.item.isPenetrating = false;
				this.collisionHandler.item.disableSnap = false;
				for (int i = this.collisionHandler.collisions.Length - 1; i >= 0; i--)
				{
					if (this.collisionHandler.collisions[i].damageStruct.penetration != DamageStruct.Penetration.None)
					{
						this.collisionHandler.item.isPenetrating = true;
						this.collisionHandler.item.disableSnap = true;
						break;
					}
				}
				if (!this.collisionHandler.item.isPenetrating && collisionInstance.damageStruct.hitRagdollPart)
				{
					this.collisionHandler.item.transform.SetParent(null, true);
				}
			}
			if (collisionInstance.targetCollider)
			{
				if (this.isColliderOnly)
				{
					if (collisionInstance.damageStruct.hitRagdollPart)
					{
						collisionInstance.damageStruct.hitRagdollPart.ragdoll.IgnoreCollision(this.colliderOnly, false, (RagdollPart.Type)0);
					}
					else
					{
						Physics.IgnoreCollision(collisionInstance.targetCollider, this.colliderOnly, false);
					}
				}
				else
				{
					foreach (Collider collider in this.colliderGroup.colliders)
					{
						if (collisionInstance.damageStruct.hitRagdollPart)
						{
							collisionInstance.damageStruct.hitRagdollPart.ragdoll.IgnoreCollision(collider, false, (RagdollPart.Type)0);
						}
						else
						{
							Physics.IgnoreCollision(collisionInstance.targetCollider, collider, false);
						}
					}
				}
			}
			if (collisionInstance.damageStruct.hitRagdollPart && this.collisionHandler.item)
			{
				this.collisionHandler.item.RefreshCollision(false);
				Creature creature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;
				if ((creature != null) ? creature.player : null)
				{
					collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature.turnRelativeToHand = true;
				}
			}
			if (collisionInstance.targetColliderGroup && collisionInstance.targetColliderGroup.collisionHandler.penetratedObjects.Contains(this.collisionHandler))
			{
				collisionInstance.targetColliderGroup.collisionHandler.penetratedObjects.Remove(this.collisionHandler);
			}
			this.collisionHandler.StopCollision(collisionInstance);
			Damager.DepenetrationEvent onUnpenetrateEvent2 = this.OnUnpenetrateEvent;
			if (onUnpenetrateEvent2 == null)
			{
				return;
			}
			onUnpenetrateEvent2(this, collisionInstance, wentThrough, EventTime.OnEnd);
		}

		// Token: 0x0600264E RID: 9806 RVA: 0x0010A310 File Offset: 0x00108510
		public void UnPenetrateAll()
		{
			if (!this.collisionHandler)
			{
				return;
			}
			for (int i = this.collisionHandler.collisions.Length - 1; i >= 0; i--)
			{
				if (this.collisionHandler.collisions[i].damageStruct.active && !(this.collisionHandler.collisions[i].damageStruct.damager != this))
				{
					this.UnPenetrate(this.collisionHandler.collisions[i], false);
				}
			}
		}

		// Token: 0x040025FC RID: 9724
		public static bool easyDismemberment = false;

		// Token: 0x040025FD RID: 9725
		[Tooltip("Specify Collider Group of which this damager will apply to.")]
		public ColliderGroup colliderGroup;

		// Token: 0x040025FE RID: 9726
		[Tooltip("(Optional) Can reference collider inside group if it is only using one collider")]
		public Collider colliderOnly;

		// Token: 0x040025FF RID: 9727
		[NonSerialized]
		public bool isColliderOnly;

		// Token: 0x04002600 RID: 9728
		[Tooltip("Specify which direction the damager will deal damage in.\nAll is best for Blunt Damage.\nForward and Back is best for Slash Damage.\nForward is best for Piercing Damage.")]
		public Damager.Direction direction;

		// Token: 0x04002601 RID: 9729
		[Tooltip("Length of which the item can pierce/slash with.\nSet to 0 for Blunt damage and single-point Pierce damage.")]
		public float penetrationLength;

		// Token: 0x04002602 RID: 9730
		[Tooltip("Depth of which a damager can deal slash damage.\nSet to 0 for Blunt damage")]
		public float penetrationDepth;

		// Token: 0x04002603 RID: 9731
		[Tooltip("Once the Penetration Depth has reached its' max, unpierce from the object")]
		public bool penetrationExitOnMaxDepth;

		// Token: 0x04002604 RID: 9732
		[NonSerialized]
		public FloatHandler skillDamageMultiplierHandler;

		// Token: 0x04002605 RID: 9733
		[NonSerialized]
		public float skillDamageMultiplier = 1f;

		/// <summary>
		/// Buffer to hold raycast hits from the shield sweeptest.
		/// </summary>
		// Token: 0x04002609 RID: 9737
		private static RaycastHit[] shieldSweepRayHits = new RaycastHit[10];

		// Token: 0x0400260A RID: 9738
		public static bool dismembermentEnabled = true;

		// Token: 0x0400260B RID: 9739
		[NonSerialized]
		public DamagerData data;

		// Token: 0x0400260C RID: 9740
		[NonSerialized]
		public CollisionHandler collisionHandler;

		// Token: 0x0400260D RID: 9741
		[NonSerialized]
		public Damager damagerImbue;

		// Token: 0x0400260E RID: 9742
		protected Collider dmgCollider;

		// Token: 0x0400260F RID: 9743
		protected RagdollPart lastSliceRagdollPart;

		// Token: 0x04002610 RID: 9744
		protected float lastSliceTime;

		// Token: 0x04002611 RID: 9745
		protected Collider lastHitCollider;

		// Token: 0x04002612 RID: 9746
		protected float lastHitTime;

		// Token: 0x04002613 RID: 9747
		protected bool loaded;

		// Token: 0x02000A19 RID: 2585
		// (Invoke) Token: 0x06004547 RID: 17735
		public delegate void PenetrationEvent(Damager damager, CollisionInstance collision, EventTime time);

		// Token: 0x02000A1A RID: 2586
		// (Invoke) Token: 0x0600454B RID: 17739
		public delegate void DepenetrationEvent(Damager damager, CollisionInstance collision, bool wentThrough, EventTime time);

		// Token: 0x02000A1B RID: 2587
		// (Invoke) Token: 0x0600454F RID: 17743
		public delegate void SkewerEvent(Damager damager, CollisionInstance collisionInstance, EventTime eventTime);

		// Token: 0x02000A1C RID: 2588
		public enum Direction
		{
			// Token: 0x04004705 RID: 18181
			All,
			// Token: 0x04004706 RID: 18182
			Forward,
			// Token: 0x04004707 RID: 18183
			ForwardAndBackward
		}

		// Token: 0x02000A1D RID: 2589
		[Flags]
		public enum PenetrationConditions
		{
			// Token: 0x04004709 RID: 18185
			None = 0,
			// Token: 0x0400470A RID: 18186
			DataAllowsPierce = 1,
			// Token: 0x0400470B RID: 18187
			MaterialAllowsPierce = 2,
			// Token: 0x0400470C RID: 18188
			HasDepth = 4,
			// Token: 0x0400470D RID: 18189
			NotNPCHandler = 8,
			// Token: 0x0400470E RID: 18190
			VelocityThreshold = 16,
			// Token: 0x0400470F RID: 18191
			TargetAllowsPierce = 32,
			// Token: 0x04004710 RID: 18192
			PartNotBlockingPierce = 64
		}

		// Token: 0x02000A1E RID: 2590
		public enum Type
		{
			// Token: 0x04004712 RID: 18194
			Blunt,
			// Token: 0x04004713 RID: 18195
			Pierce,
			// Token: 0x04004714 RID: 18196
			Slash
		}
	}
}
