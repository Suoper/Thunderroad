using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	/// <summary>
	/// This component allows objects and items to be broken in various different ways.
	/// </summary>
	// Token: 0x02000313 RID: 787
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Items/Breakable.html")]
	public class Breakable : ThunderBehaviour
	{
		/// <summary>
		/// Default handle ID.
		/// </summary>
		// Token: 0x17000240 RID: 576
		// (get) Token: 0x0600254A RID: 9546 RVA: 0x000FF92D File Offset: 0x000FDB2D
		private string DefaultHandleID { get; } = "ObjectHandleProp";

		/// <summary>
		/// Allow breakables to be broken?
		/// </summary>
		// Token: 0x17000241 RID: 577
		// (get) Token: 0x0600254B RID: 9547 RVA: 0x000FF935 File Offset: 0x000FDB35
		// (set) Token: 0x0600254C RID: 9548 RVA: 0x000FF93C File Offset: 0x000FDB3C
		public static bool AllowBreaking { get; set; } = true;

		/// <summary>
		/// Is this broken?
		/// </summary>
		// Token: 0x17000242 RID: 578
		// (get) Token: 0x0600254D RID: 9549 RVA: 0x000FF944 File Offset: 0x000FDB44
		// (set) Token: 0x0600254E RID: 9550 RVA: 0x000FF94C File Offset: 0x000FDB4C
		public bool IsBroken { get; private set; }

		/// <summary>
		/// Unbroken item if any.
		/// </summary>
		// Token: 0x17000243 RID: 579
		// (get) Token: 0x0600254F RID: 9551 RVA: 0x000FF955 File Offset: 0x000FDB55
		// (set) Token: 0x06002550 RID: 9552 RVA: 0x000FF95D File Offset: 0x000FDB5D
		public Item LinkedItem { get; private set; }

		// Token: 0x17000244 RID: 580
		// (get) Token: 0x06002551 RID: 9553 RVA: 0x000FF966 File Offset: 0x000FDB66
		// (set) Token: 0x06002552 RID: 9554 RVA: 0x000FF96E File Offset: 0x000FDB6E
		public Vector3 ItemLocalBarycenter { get; private set; }

		/// <summary>
		/// Cache the rigidbodies and items in lists
		/// </summary>
		// Token: 0x06002553 RID: 9555 RVA: 0x000FF978 File Offset: 0x000FDB78
		public void RetrieveSubItems()
		{
			if (!this.unbrokenObjectsHolder || !this.brokenObjectsHolder)
			{
				return;
			}
			List<Breakable> subBreakables = new List<Breakable>();
			Rigidbody[] rigidbodies = this.unbrokenObjectsHolder.GetComponentsInChildren<Rigidbody>(true);
			for (int i = 0; i < rigidbodies.Length; i++)
			{
				Item isItem = rigidbodies[i].GetComponent<Item>();
				if (isItem != null)
				{
					this.subUnbrokenItems.Add(isItem);
				}
				else
				{
					this.subUnbrokenBodies.Add(rigidbodies[i].AsPhysicBody());
				}
				this.allSubBodies.Add(rigidbodies[i].AsPhysicBody());
			}
			Rigidbody currentBody;
			if (base.TryGetComponent<Rigidbody>(out currentBody))
			{
				Item isItem2 = currentBody.GetComponent<Item>();
				if (isItem2 != null && !this.subUnbrokenItems.Contains(isItem2))
				{
					this.subUnbrokenItems.Add(isItem2);
				}
				else if (!this.subUnbrokenBodies.Contains(currentBody.AsPhysicBody()))
				{
					this.subUnbrokenBodies.Add(currentBody.AsPhysicBody());
				}
			}
			rigidbodies = this.brokenObjectsHolder.GetComponentsInChildren<Rigidbody>(true);
			for (int j = 0; j < rigidbodies.Length; j++)
			{
				Breakable isSubBreakable = rigidbodies[j].gameObject.GetComponent<Breakable>();
				if (isSubBreakable)
				{
					isSubBreakable.RetrieveSubItems();
					subBreakables.Add(isSubBreakable);
				}
				Item isItem3 = rigidbodies[j].GetComponent<Item>();
				if (isItem3)
				{
					this.subBrokenItems.Add(isItem3);
				}
				else
				{
					this.subBrokenBodies.Add(rigidbodies[j].AsPhysicBody());
				}
				this.allSubBodies.Add(rigidbodies[j].AsPhysicBody());
				MeshFilter mf = rigidbodies[j].GetComponent<MeshFilter>();
				if (mf)
				{
					this.brokenItemMeshes.Add(mf.mesh);
				}
			}
			for (int k = 0; k < subBreakables.Count; k++)
			{
				Breakable subBreakable = subBreakables[k];
				for (int l = 0; l < subBreakable.subBrokenItems.Count; l++)
				{
					this.subBrokenItems.Remove(subBreakable.subBrokenItems[l]);
				}
				for (int m = 0; m < subBreakable.subBrokenBodies.Count; m++)
				{
					this.subBrokenBodies.Remove(subBreakable.subBrokenBodies[m]);
				}
			}
		}

		/// <summary>
		/// Set whether or not the breakable can be broken
		/// </summary>
		// Token: 0x06002554 RID: 9556 RVA: 0x000FFBB2 File Offset: 0x000FDDB2
		public void SetBreakable(bool active)
		{
			this.canBreak = active;
		}

		// Token: 0x06002555 RID: 9557 RVA: 0x000FFBBB File Offset: 0x000FDDBB
		public void Explode(float force, Vector3 origin, float radius, float upwardsModifier, ForceMode forceMode)
		{
			if (!this.CanBeBroken())
			{
				return;
			}
			this.Break();
			this.InheritParentVelocity();
			this.ExplodePieces(force, origin, radius, upwardsModifier, forceMode);
		}

		/// <summary>
		/// Break the breakable.
		/// This is used as a fake method to bypass the real behaviour, to break via events for example.
		/// </summary>
		// Token: 0x06002556 RID: 9558 RVA: 0x000FFBE0 File Offset: 0x000FDDE0
		public void Break()
		{
			if (!this.CanBeBroken())
			{
				return;
			}
			this.IsBroken = true;
			EventManager.InvokeBreakStart(this);
			if (this.LinkedItem)
			{
				this.LinkedItem.InvokeBreakStartEvent(this);
			}
			UnityEvent<float> unityEvent = this.onBreak;
			if (unityEvent != null)
			{
				unityEvent.Invoke(this.instantaneousBreakDamage);
			}
			PhysicBody[] pieces = this.EnableBrokenPieces();
			this.UpdateHandleLinks();
			this.ReleaseUnbrokenObjects();
			EventManager.InvokeBreakEnd(this, pieces);
		}

		// Token: 0x06002557 RID: 9559 RVA: 0x000FFC50 File Offset: 0x000FDE50
		private void Awake()
		{
			this.currentRigidbody = base.GetComponent<Rigidbody>();
			this.LinkedItem = base.GetComponent<Item>();
			if (this.LinkedItem != null)
			{
				this.LinkedItem.breakable = this;
			}
			if (this.subBrokenItems.Count == 0 || this.subBrokenBodies.Count == 0)
			{
				this.RetrieveSubItems();
			}
		}

		// Token: 0x06002558 RID: 9560 RVA: 0x000FFCAF File Offset: 0x000FDEAF
		private void Start()
		{
			this.Initialize();
		}

		// Token: 0x06002559 RID: 9561 RVA: 0x000FFCB7 File Offset: 0x000FDEB7
		private void OnDestroy()
		{
			this.AssignCachedMeshToBrokenItems();
		}

		// Token: 0x0600255A RID: 9562 RVA: 0x000FFCBF File Offset: 0x000FDEBF
		public void Initialize()
		{
			if (this.isInitialized)
			{
				return;
			}
			this.InitUnbrokenObjects();
			this.InitBrokenObjects();
			this.isInitialized = true;
		}

		/// <summary>
		/// Enable the default object
		/// </summary>
		// Token: 0x0600255B RID: 9563 RVA: 0x000FFCDD File Offset: 0x000FDEDD
		private void InitUnbrokenObjects()
		{
			this.unbrokenObjectsHolder.SetActive(true);
		}

		/// <summary>
		/// Enable the broken sub pieces, init them, then disable it
		/// </summary>
		// Token: 0x0600255C RID: 9564 RVA: 0x000FFCEC File Offset: 0x000FDEEC
		private void InitBrokenObjects()
		{
			this.brokenObjectsHolder.SetActive(true);
			int count = this.subBrokenItems.Count;
			for (int i = 0; i < count; i++)
			{
				Item subBrokenItem = this.subBrokenItems[i];
				ItemData itemData = Catalog.GetData<ItemData>(this.subBrokenItems[i].itemId, true);
				if (itemData != null)
				{
					if (subBrokenItem.physicBody == null)
					{
						subBrokenItem.SetPhysicBodyAndMainCollisionHandler();
					}
					subBrokenItem.Load(itemData);
					subBrokenItem.UnRegisterArea();
				}
				else
				{
					Debug.LogError("Invalid item ID set on sub-breakable item [ " + this.subBrokenItems[i].name + " ]. This item does not have item data and will cause game-breaking errors if not fixed!");
				}
				if (subBrokenItem.gameObject != base.gameObject)
				{
					Breakable subBreakable;
					if (subBrokenItem.TryGetComponent<Breakable>(out subBreakable))
					{
						subBreakable.Initialize();
					}
					else
					{
						subBrokenItem.isBrokenPiece = true;
					}
				}
			}
			int subBrokenBodiesCount = this.subBrokenBodies.Count;
			for (int j = 0; j < subBrokenBodiesCount; j++)
			{
				foreach (Handle handle in this.subBrokenBodies[j].gameObject.GetComponentsInChildren<Handle>())
				{
					string handleId = handle.interactableId;
					if (string.IsNullOrEmpty(handleId))
					{
						handleId = this.DefaultHandleID;
					}
					InteractableData interactableData = Catalog.GetData<InteractableData>(handleId, true).Clone() as InteractableData;
					if (interactableData != null)
					{
						handle.Load(interactableData);
					}
				}
			}
			this.GetBrokenItemBarycenter();
			this.brokenObjectsHolder.SetActive(false);
		}

		/// <summary>
		/// Break this object.
		/// </summary>
		/// <param name="collision">Collision that caused the break</param>
		/// <param name="creature">Colliding creature, passed to avoid re-caching it</param>
		// Token: 0x0600255D RID: 9565 RVA: 0x000FFE60 File Offset: 0x000FE060
		public void Break(Collision collision, Creature creature)
		{
			if (!this.CanBeBroken())
			{
				return;
			}
			this.breakingCollision = collision;
			this.IsBroken = true;
			EventManager.InvokeBreakStart(this);
			if (this.LinkedItem)
			{
				this.LinkedItem.InvokeBreakStartEvent(this);
			}
			UnityEvent<float> unityEvent = this.onBreak;
			if (unityEvent != null)
			{
				unityEvent.Invoke(collision.relativeVelocity.sqrMagnitude);
			}
			PhysicBody[] pieces = this.EnableBrokenPieces();
			this.UpdateHandleLinks();
			this.UpdateSubPiecesVelocities(collision, creature);
			this.ReleaseUnbrokenObjects();
			this.FixCollidingBodyVelocity(collision);
			EventManager.InvokeBreakEnd(this, pieces);
		}

		/// <summary>
		/// Hit the breakable with the given collision.
		/// If the hit is not strong enough, nothing happens.
		/// one parameter only to be used from Unity events.
		/// </summary>
		/// <param name="collision">Collision of the hit</param>
		// Token: 0x0600255E RID: 9566 RVA: 0x000FFEEB File Offset: 0x000FE0EB
		public void Hit(Collision collision)
		{
			this.Hit(collision, false);
		}

		/// <summary>
		/// Hit the breakable with the given collision.
		/// If the hit is not strong enough, nothing happens.
		/// </summary>
		/// <param name="collision">Collision of the hit</param>
		/// <param name="bypassHitPoints">If true, instantaneously breaks the item without caring about remaining hit points.</param>
		// Token: 0x0600255F RID: 9567 RVA: 0x000FFEF8 File Offset: 0x000FE0F8
		public void Hit(Collision collision, bool bypassHitPoints)
		{
			if (this.IsUnbrokenItemInInitializingArea())
			{
				return;
			}
			if (!base.gameObject.activeInHierarchy)
			{
				return;
			}
			if (Level.current && !Level.current.loaded)
			{
				return;
			}
			if (collision.collider.GetPhysicBody() == null)
			{
				if (this.breakOnEnviro == Breakable.EnviroBreakMode.Never)
				{
					return;
				}
				if (this.breakOnEnviro == Breakable.EnviroBreakMode.Handled)
				{
					Item linkedItem = this.LinkedItem;
					int? num;
					if (linkedItem == null)
					{
						num = null;
					}
					else
					{
						List<RagdollHand> handlers = linkedItem.handlers;
						num = ((handlers != null) ? new int?(handlers.Count) : null);
					}
					int? num2 = num;
					if (num2.GetValueOrDefault() <= 0)
					{
						return;
					}
				}
			}
			Creature creature;
			bool isCreature = this.CheckIfHittingObjectIsCreature(collision, out creature);
			float mass;
			if (this.ignoreObjectUnderCertainMass && !isCreature && this.GetCollidingObjectMass(collision, out mass) && mass <= this.minimalMassThreshold)
			{
				return;
			}
			float momentumMagnitude = this.GetMomentum(collision, creature).sqrMagnitude;
			if (momentumMagnitude < this.minimumDamageMomentum)
			{
				return;
			}
			if (this.lastHitTime + this.hitCooldownTime > Time.time)
			{
				return;
			}
			if (this.useBreakPoints && !this.CheckIfHitBreakPoint(collision))
			{
				return;
			}
			if (!this.CanBeBroken())
			{
				return;
			}
			this.momentumHealth -= Mathf.Clamp(Mathf.Sqrt(momentumMagnitude), 0f, this.clampDamage ? this.maxCollisionMomentum : float.PositiveInfinity);
			this.lastHitTime = Time.time;
			if (this.canInstantaneouslyBreak && momentumMagnitude >= this.instantaneousBreakDamage)
			{
				this.momentumHealth = 0f;
			}
			if (bypassHitPoints)
			{
				this.momentumHealth = 0f;
			}
			UnityEvent<float> unityEvent = this.onTakeDamage;
			if (unityEvent != null)
			{
				unityEvent.Invoke(momentumMagnitude);
			}
			if (this.momentumHealth <= 0f)
			{
				this.Break(collision, creature);
				return;
			}
			UnityEvent<float> unityEvent2 = this.onNonBreakHit;
			if (unityEvent2 == null)
			{
				return;
			}
			unityEvent2.Invoke(momentumMagnitude);
		}

		// Token: 0x06002560 RID: 9568 RVA: 0x001000B8 File Offset: 0x000FE2B8
		public void ExplodePieces(float force, Vector3 origin, float radius, float upwardsModifier, ForceMode forceMode)
		{
			if (this.subBrokenBodies != null)
			{
				for (int i = 0; i < this.subBrokenItems.Count; i++)
				{
					Item item = this.subBrokenItems[i];
					if (item != null)
					{
						item.AddExplosionForce(force, origin, radius, upwardsModifier, forceMode, null);
					}
				}
			}
		}

		/// <summary>
		/// Apply velocity to broken pieces.
		/// </summary>
		// Token: 0x06002561 RID: 9569 RVA: 0x00100104 File Offset: 0x000FE304
		public void ApplyVelocityToBrokenPieces(Vector3 hitForce, float hitForceMagnitude, Vector3 hitBarycenter)
		{
			if (this.subBrokenItems != null)
			{
				Vector3 itemWorldSpaceBarycenter = base.transform.TransformPoint(this.ItemLocalBarycenter);
				for (int i = 0; i < this.subBrokenItems.Count; i++)
				{
					if (this.useExplosionForce)
					{
						if (this.subBrokenItems[i] == null)
						{
							Debug.LogWarning("There is a null subBrokenItem on " + base.transform.name);
						}
						else
						{
							Vector3 offsetFromCenter = (this.subBrokenItems[i].transform.TransformPoint(this.subBrokenItemsBarycenters[i]) - itemWorldSpaceBarycenter).normalized;
							bool flag = (double)Mathf.Abs(-Vector3.Dot(hitForce.normalized, offsetFromCenter)) > 0.8;
							Vector3 p = Vector3.ProjectOnPlane(offsetFromCenter, hitForce.normalized);
							Vector3 force = flag ? hitForce : (p * (this.explosionForceFactor * hitForceMagnitude));
							this.subBrokenItems[i].physicBody.AddForceAtPosition(force, itemWorldSpaceBarycenter, ForceMode.Acceleration);
						}
					}
					else
					{
						this.subBrokenItems[i].physicBody.AddForceAtPosition(hitForce, hitBarycenter, ForceMode.Acceleration);
					}
				}
			}
			if (this.subBrokenBodies != null)
			{
				for (int j = 0; j < this.subBrokenBodies.Count; j++)
				{
					this.subBrokenBodies[j].AddForceAtPosition(hitForce, hitBarycenter, ForceMode.Acceleration);
				}
			}
		}

		/// <summary>
		/// Compute and caches the item and broken pieces barycenters.
		/// This uses the center of their local bounds.
		/// We need to do that since most props have shared origin across their broken pieces.
		/// </summary>
		// Token: 0x06002562 RID: 9570 RVA: 0x00100268 File Offset: 0x000FE468
		private void GetBrokenItemBarycenter()
		{
			this.subBrokenItemsBarycenters = new Vector3[this.subBrokenItems.Count];
			for (int i = 0; i < this.subBrokenItems.Count; i++)
			{
				this.subBrokenItemsBarycenters[i] = this.subBrokenItems[i].GetLocalBounds().center;
				this.ItemLocalBarycenter += this.subBrokenItemsBarycenters[i];
			}
			this.ItemLocalBarycenter /= (float)this.subBrokenItems.Count;
		}

		/// <summary>
		/// Colliding body will be stopped/slowed down by the collision, so this method reapply its speed over 2 frames.
		/// </summary>
		/// <param name="collision">Current collision.</param>
		// Token: 0x06002563 RID: 9571 RVA: 0x00100300 File Offset: 0x000FE500
		private void FixCollidingBodyVelocity(Collision collision)
		{
			if (!collision.body)
			{
				return;
			}
			Item collidingItem = collision.body.GetComponent<Item>();
			PhysicBody pb = collidingItem ? collidingItem.physicBody : collision.body.GetPhysicBody();
			if (pb != null)
			{
				base.StartCoroutine(this.UpdateCollidingBodyVelocityCoroutine(pb));
			}
		}

		/// <summary>
		/// Colliding body will be stopped/slowed down by the collision, so this method reapply its speed over 2 frames.
		/// </summary>
		/// <param name="collision">Current collision.</param>
		// Token: 0x06002564 RID: 9572 RVA: 0x0010035A File Offset: 0x000FE55A
		private IEnumerator UpdateCollidingBodyVelocityCoroutine(PhysicBody pb)
		{
			Vector3 linearVelocityToReapply = pb.velocity;
			Vector3 angularVelocityToReapply = pb.angularVelocity;
			yield return Yielders.FixedUpdate;
			pb.velocity = linearVelocityToReapply;
			pb.angularVelocity = angularVelocityToReapply;
			yield return Yielders.FixedUpdate;
			pb.velocity = linearVelocityToReapply;
			pb.angularVelocity = angularVelocityToReapply;
			yield break;
		}

		/// <summary>
		/// Unbroken items are despawned.
		/// the breakable holder is released with a  bit of delay to allow event calls and sounds
		/// </summary>
		// Token: 0x06002565 RID: 9573 RVA: 0x0010036C File Offset: 0x000FE56C
		private void ReleaseUnbrokenObjects()
		{
			if (this.LinkedItem != null)
			{
				for (int i = 0; i < this.LinkedItem.collisionHandlers.Count; i++)
				{
					if (this.LinkedItem.collisionHandlers[i].penetratedObjects.Count > 0)
					{
						this.LinkedItem.collisionHandlers[i].RemoveAllPenetratedObjects(out this.piercedDamagers);
					}
				}
			}
			this.MoveEffectsToBrokenItems();
			this.TransferStatusEffects();
			for (int j = 0; j < this.piercedDamagers.Count; j++)
			{
				if (this.piercedDamagers[j].type == Damager.Type.Pierce)
				{
					this.piercedDamagers[j].TryPierceItems(this.bodyTransforms);
				}
			}
			if (this.unbrokenObjectsHolder)
			{
				this.unbrokenObjectsHolder.SetActive(false);
			}
			if (this.subUnbrokenItems != null)
			{
				for (int k = 0; k < this.subUnbrokenItems.Count; k++)
				{
					Item unbrokenItem = this.subUnbrokenItems[k];
					for (int l = 0; l < unbrokenItem.handlers.Count; l++)
					{
						unbrokenItem.handlers[l].TryRelease();
					}
					this.CleanUp(unbrokenItem);
					if (!this.LinkedItem)
					{
						unbrokenItem.Despawn();
					}
					if (unbrokenItem == this.LinkedItem)
					{
						if (this.despawnLinkedItem)
						{
							this.subUnbrokenItems[k].Despawn(this.despawnLinkedItemDelay);
							if (unbrokenItem.spawner)
							{
								unbrokenItem.spawner.UnloadFromSpawner(unbrokenItem);
							}
							this.subUnbrokenItems[k].physicBody.isEnabled = false;
						}
					}
					else
					{
						this.subUnbrokenItems[k].Despawn();
					}
				}
			}
			this.AssignCachedMeshToBrokenItems();
			UnityEngine.Object.Destroy(this, 10f);
		}

		// Token: 0x06002566 RID: 9574 RVA: 0x00100540 File Offset: 0x000FE740
		private void AssignCachedMeshToBrokenItems()
		{
			for (int i = 0; i < this.subBrokenItems.Count; i++)
			{
				Item subBrokenItem = this.subBrokenItems[i];
				if (subBrokenItem != null)
				{
					MeshFilter mf = subBrokenItem.GetComponent<MeshFilter>();
					if (mf)
					{
						mf.mesh = this.brokenItemMeshes[i];
					}
				}
			}
		}

		/// <summary>
		/// Move all effects to any broken pieces.
		/// </summary>
		// Token: 0x06002567 RID: 9575 RVA: 0x0010059C File Offset: 0x000FE79C
		private void MoveEffectsToBrokenItems()
		{
			if (this.LinkedItem == null)
			{
				return;
			}
			foreach (Effect effect in this.LinkedItem.GetComponentsInChildren<Effect>(true))
			{
				if (!(effect == null) && effect.module != null && effect.module.reparentWithBreakable)
				{
					Transform effectTransform = effect.transform;
					Collider closestCollider;
					if (this.GetClosestCollider(effectTransform, out closestCollider))
					{
						effectTransform.SetParent(closestCollider.transform);
					}
				}
			}
		}

		// Token: 0x06002568 RID: 9576 RVA: 0x00100614 File Offset: 0x000FE814
		private void TransferStatusEffects()
		{
			for (int i = 0; i < this.subBrokenItems.Count; i++)
			{
				this.LinkedItem.TransferStatuses(this.subBrokenItems[i]);
			}
		}

		/// <summary>
		/// Obtain the closest collider to the target transform.
		/// </summary>
		// Token: 0x06002569 RID: 9577 RVA: 0x00100650 File Offset: 0x000FE850
		private bool GetClosestCollider(Transform effectTransform, out Collider closestCollider)
		{
			closestCollider = null;
			bool found = false;
			Vector3 effectTransformPosition = effectTransform.position;
			float closestDistance = float.PositiveInfinity;
			int count = this.subBrokenItems.Count;
			for (int i = 0; i < count; i++)
			{
				Item subBrokenItem = this.subBrokenItems[i];
				int colliderGroupsCount = subBrokenItem.colliderGroups.Count;
				for (int j = 0; j < colliderGroupsCount; j++)
				{
					ColliderGroup subColliderGroup = subBrokenItem.colliderGroups[j];
					int collidersCount = subColliderGroup.colliders.Count;
					for (int k = 0; k < collidersCount; k++)
					{
						Collider subCollider = subColliderGroup.colliders[k];
						float distance = (subCollider.ClosestPoint(effectTransformPosition) - effectTransformPosition).sqrMagnitude;
						if (distance < closestDistance)
						{
							closestDistance = distance;
							closestCollider = subCollider;
							found = true;
						}
					}
				}
			}
			return found;
		}

		/// <summary>
		/// Cleans up the given item, before getting removed
		/// ie. un-penetrates everything, and un grip if needed
		/// </summary>
		/// <param name="subUnbrokenItem">Item to clean up</param>
		// Token: 0x0600256A RID: 9578 RVA: 0x00100728 File Offset: 0x000FE928
		private void CleanUp(Item subUnbrokenItem)
		{
			int collisionHandlersCount = subUnbrokenItem.collisionHandlers.Count;
			for (int index = 0; index < collisionHandlersCount; index++)
			{
				CollisionHandler collisionHandler = subUnbrokenItem.collisionHandlers[index];
				int damagersCount = collisionHandler.damagers.Count;
				for (int i = 0; i < damagersCount; i++)
				{
					collisionHandler.damagers[i].UnPenetrateAll();
				}
				for (int j = collisionHandler.penetratedObjects.Count - 1; j >= 0; j--)
				{
					CollisionHandler penetratedObject = collisionHandler.penetratedObjects[j];
					int count = penetratedObject.damagers.Count;
					for (int k = 0; k < count; k++)
					{
						penetratedObject.damagers[k].UnPenetrateAll();
					}
				}
			}
			if (subUnbrokenItem.isGripped)
			{
				for (int l = 0; l < Creature.allActive.Count; l++)
				{
					Creature creature = Creature.allActive[l];
					if (creature.handRight.climb.gripItem == subUnbrokenItem)
					{
						creature.handRight.climb.UnGrip();
					}
					if (creature.handLeft.climb.gripItem == subUnbrokenItem)
					{
						creature.handLeft.climb.UnGrip();
					}
				}
			}
		}

		/// <summary>
		/// Release manin (unbroken) handles and grab their broken version.
		/// </summary>
		// Token: 0x0600256B RID: 9579 RVA: 0x00100870 File Offset: 0x000FEA70
		private void UpdateHandleLinks()
		{
			if (this.handleLinks != null)
			{
				for (int i = 0; i < this.handleLinks.Length; i++)
				{
					Handle handleMain = this.handleLinks[i].handleMain;
					Handle handleSecondary = this.handleLinks[i].handleSecondary;
					List<RagdollHand> handlers = handleMain.handlers;
					if (handlers != null)
					{
						for (int j = handlers.Count - 1; j >= 0; j--)
						{
							RagdollHand ragdollHand = handlers[j];
							ragdollHand.TryRelease();
							ragdollHand.Grab(handleSecondary, true, false);
						}
					}
				}
			}
		}

		/// <summary>
		/// Enables and inits sub items and sub bodies.
		/// </summary>
		// Token: 0x0600256C RID: 9580 RVA: 0x001008E8 File Offset: 0x000FEAE8
		private PhysicBody[] EnableBrokenPieces()
		{
			if (this.brokenObjectsHolder != null)
			{
				this.brokenObjectsHolder.transform.SetParent(this.unbrokenObjectsHolder.transform.parent);
				this.brokenObjectsHolder.SetActive(true);
			}
			bool hasSubBrokenItems = !this.subBrokenItems.IsNullOrEmpty();
			bool hasSubBrokenBodies = !this.subBrokenBodies.IsNullOrEmpty();
			if (!hasSubBrokenItems && !hasSubBrokenBodies)
			{
				return null;
			}
			this.bodyTransforms.Clear();
			List<Item> list = this.subBrokenItems;
			int num = (list != null) ? list.Count : 0;
			List<PhysicBody> list2 = this.subBrokenBodies;
			PhysicBody[] pieces = new PhysicBody[num + ((list2 != null) ? list2.Count : 0)];
			int runningIndex = 0;
			Item item;
			if (hasSubBrokenItems && base.TryGetComponent<Item>(out item))
			{
				ItemSpawner spawner = item.spawner;
				int count = this.subBrokenItems.Count;
				for (int i = 0; i < count; i++)
				{
					Item child = this.subBrokenItems[i];
					child.gameObject.SetActive(true);
					child.transform.parent = null;
					child.spawnTime = (child.lastInteractionTime = Time.time);
					child.spawner = spawner;
					pieces[runningIndex] = child.physicBody;
					this.bodyTransforms.Add(new ValueTuple<Transform, PhysicBody>(child.physicBody.transform, child.physicBody));
					runningIndex++;
				}
			}
			if (hasSubBrokenBodies)
			{
				int count2 = this.subBrokenBodies.Count;
				for (int j = 0; j < count2; j++)
				{
					PhysicBody child2 = this.subBrokenBodies[j];
					child2.gameObject.SetActive(true);
					child2.transform.parent = null;
					pieces[runningIndex] = child2;
					runningIndex++;
				}
			}
			if (this.ignoreCollisionOnBreak)
			{
				this.IgnoreCollision();
			}
			return pieces;
		}

		// Token: 0x0600256D RID: 9581 RVA: 0x00100AAC File Offset: 0x000FECAC
		private void InheritParentVelocity()
		{
			if (!this.currentRigidbody)
			{
				return;
			}
			if (this.subBrokenItems != null)
			{
				for (int i = 0; i < this.subBrokenItems.Count; i++)
				{
					this.subBrokenItems[i].physicBody.velocity = this.currentRigidbody.velocity;
				}
			}
			if (this.subBrokenBodies != null)
			{
				for (int j = 0; j < this.subBrokenBodies.Count; j++)
				{
					this.subBrokenBodies[j].velocity = this.currentRigidbody.velocity;
				}
			}
		}

		/// <summary>
		/// Set the velocity of all bodies to match the velocity of the unbroken object
		/// </summary>
		/// <param name="collision">Current collsion context</param>
		/// <param name="creature">Colliding creature, passed to avoid re-caching it</param>
		// Token: 0x0600256E RID: 9582 RVA: 0x00100B40 File Offset: 0x000FED40
		private void UpdateSubPiecesVelocities(Collision collision, Creature creature)
		{
			this.InheritParentVelocity();
			int contactsCount = collision.GetContacts(this.contactPoints);
			Vector3 hitBarycenter = Vector3.zero;
			for (int i = 0; i < contactsCount; i++)
			{
				hitBarycenter += this.contactPoints[i].point;
			}
			hitBarycenter /= (float)contactsCount;
			Vector3 hitForce = this.GetMomentum(collision, creature);
			float hitForceMagnitude = hitForce.magnitude;
			this.ApplyVelocityToBrokenPieces(hitForce, hitForceMagnitude, hitBarycenter);
		}

		/// <summary>
		/// Ignore collision between all broken item pieces.
		/// </summary>
		/// <returns></returns>
		// Token: 0x0600256F RID: 9583 RVA: 0x00100BB4 File Offset: 0x000FEDB4
		private void IgnoreCollision()
		{
			if (this.subBrokenItems == null)
			{
				return;
			}
			int count = this.subBrokenItems.Count;
			for (int i = 0; i < count; i++)
			{
				this.IgnoreAllSubBrokenItems(this.subBrokenItems[i], true);
			}
		}

		/// <summary>
		/// Ignore collision between the input item and all broken pieces.
		/// </summary>
		// Token: 0x06002570 RID: 9584 RVA: 0x00100BF8 File Offset: 0x000FEDF8
		private void IgnoreAllSubBrokenItems(Item item, bool ignore = true)
		{
			int count = this.subBrokenItems.Count;
			for (int i = 0; i < count; i++)
			{
				if (!(this.subBrokenItems[i] == item))
				{
					item.IgnoreItemCollision(this.subBrokenItems[i], ignore);
				}
			}
		}

		/// <summary>
		/// Is this item able to be broken?
		/// </summary>
		// Token: 0x06002571 RID: 9585 RVA: 0x00100C44 File Offset: 0x000FEE44
		public bool CanBeBroken()
		{
			return !this.IsBroken && Breakable.AllowBreaking && this.canBreak;
		}

		/// <summary>
		/// Is this item in an area not yet initialized?
		/// </summary>
		// Token: 0x06002572 RID: 9586 RVA: 0x00100C60 File Offset: 0x000FEE60
		private bool IsUnbrokenItemInInitializingArea()
		{
			if (AreaManager.Instance == null)
			{
				return false;
			}
			if (this.subUnbrokenItems != null)
			{
				int count = this.subUnbrokenItems.Count;
				for (int i = 0; i < count; i++)
				{
					SpawnableArea itemArea = this.subUnbrokenItems[i].currentArea;
					if (itemArea == null)
					{
						this.subUnbrokenItems[i].CheckCurrentArea();
						itemArea = this.subUnbrokenItems[i].currentArea;
					}
					if (itemArea != null && itemArea.IsSpawned && !itemArea.SpawnedArea.initialized)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if the linked collider is a creature.
		/// </summary>
		/// <param name="collision">Current collision struct</param>
		/// <returns>True if the linked body is a ragdoll part belonging to a creature, false otherwise. And the corresponding creature</returns>
		// Token: 0x06002573 RID: 9587 RVA: 0x00100CF0 File Offset: 0x000FEEF0
		private bool CheckIfHittingObjectIsCreature(Collision collision, out Creature creature)
		{
			creature = null;
			if (collision.collider)
			{
				if (!collision.collider.attachedRigidbody)
				{
					return false;
				}
				RagdollPart part;
				if (collision.collider.attachedRigidbody.TryGetComponent<RagdollPart>(out part))
				{
					creature = part.ragdoll.creature;
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Compute the hit momentum.
		/// The idea is to take mass into account, and not only velocity.
		/// This is because big items (ie. 2 handed hammers) are going slower and are harder to break things with.
		/// Note: This is capped to 5, to avoid aberrant values.
		/// </summary>
		/// <param name="collision">Current collision.</param>
		/// <param name="creature">Colliding creature, passed to avoid re-caching it</param>
		/// <returns>The momentum as a force vector.</returns>
		// Token: 0x06002574 RID: 9588 RVA: 0x00100D48 File Offset: 0x000FEF48
		private Vector3 GetMomentum(Collision collision, Creature creature)
		{
			float mass;
			this.GetCollidingObjectMass(collision, out mass);
			if (creature && !creature.isPlayer)
			{
				mass = creature.ragdoll.totalMass;
			}
			return Mathf.Clamp(Mathf.Log(mass) * 0.5f, this.momentumMassFactorClamp.x, this.momentumMassFactorClamp.y) * collision.relativeVelocity;
		}

		/// <summary>
		/// Returns the mass of the body attached to the collision struct.
		/// </summary>
		/// <param name="collision">CurrentCollision</param>
		/// <param name="mass">returned mass</param>
		/// <returns>If the hit object has a body and isn't static, and the mass of the body  (1 when not found)</returns>
		// Token: 0x06002575 RID: 9589 RVA: 0x00100DB0 File Offset: 0x000FEFB0
		private bool GetCollidingObjectMass(Collision collision, out float mass)
		{
			mass = 1f;
			if (!collision.body)
			{
				return false;
			}
			Item item;
			if (collision.body.TryGetComponent<Item>(out item) && item.worldAttached)
			{
				return false;
			}
			Component body = collision.body;
			Rigidbody rb = body as Rigidbody;
			float num;
			if (rb == null)
			{
				ArticulationBody ab = body as ArticulationBody;
				if (ab == null)
				{
					num = mass;
				}
				else
				{
					num = ab.mass;
				}
			}
			else
			{
				num = rb.mass;
			}
			mass = num;
			return true;
		}

		/// <summary>
		/// Does the target collision hit any break points?
		/// </summary>
		// Token: 0x06002576 RID: 9590 RVA: 0x00100E28 File Offset: 0x000FF028
		private bool CheckIfHitBreakPoint(Collision collision)
		{
			if (this.breakPoints == null || this.breakPoints.Count <= 0)
			{
				return false;
			}
			int contactsCount = collision.GetContacts(this.contactPoints);
			for (int i = 0; i < contactsCount; i++)
			{
				ContactPoint contactPoint = this.contactPoints[i];
				for (int j = 0; j < this.breakPoints.Count; j++)
				{
					if (this.breakPoints[j].CheckIfHit(base.transform, contactPoint))
					{
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06002577 RID: 9591 RVA: 0x00100EA6 File Offset: 0x000FF0A6
		private void OnCollisionEnter(Collision collision)
		{
			this.Hit(collision);
		}

		/// <summary>
		/// Checks if two transforms are close enough.
		/// </summary>
		/// <param name="source">Source transform</param>
		/// <param name="other">Transform to compare the source against</param>
		/// <param name="positionThreshold">Allowed distance threshold for transforms to be considered close</param>
		/// <param name="rotationThreshold">Allowed rotation threshold (in degrees) for transforms to be considered close</param>
		/// <param name="checkRotation">If true, compares rotations.</param>
		/// <returns>True if threshold are low enough</returns>
		// Token: 0x06002578 RID: 9592 RVA: 0x00100EB0 File Offset: 0x000FF0B0
		private bool IsTransformsRoughlyMatching(Transform source, Transform other, float positionThreshold = 0.2f, float rotationThreshold = 10f, bool checkRotation = false)
		{
			return (!checkRotation || Quaternion.Angle(source.rotation, other.rotation) <= rotationThreshold) && (source.position - other.position).magnitude <= positionThreshold;
		}

		/// <summary>
		/// Fill the handle links list with handles that have close enough transformation.
		/// </summary>
		// Token: 0x06002579 RID: 9593 RVA: 0x00100EF8 File Offset: 0x000FF0F8
		public void AutoMatchHandles()
		{
			if (!this.unbrokenObjectsHolder)
			{
				return;
			}
			if (!this.brokenObjectsHolder)
			{
				return;
			}
			Handle[] unbrokenHandles = this.unbrokenObjectsHolder.GetComponentsInChildren<Handle>();
			Handle[] brokenHandles = this.brokenObjectsHolder.GetComponentsInChildren<Handle>();
			List<Handle> brokenHandlesList = new List<Handle>();
			brokenHandlesList.AddRange(brokenHandles);
			List<Breakable.HandleLink> handleLinksBuffer = new List<Breakable.HandleLink>(Mathf.Max(unbrokenHandles.Length, brokenHandles.Length));
			foreach (Handle unbrokenHandle in unbrokenHandles)
			{
				Handle closest = Breakable.<AutoMatchHandles>g__GetClosestHandle|106_0(brokenHandlesList, unbrokenHandle);
				if (closest && this.IsTransformsRoughlyMatching(unbrokenHandle.transform, closest.transform, 0.2f, 10f, false))
				{
					handleLinksBuffer.Add(new Breakable.HandleLink(unbrokenHandle, closest));
					brokenHandlesList.Remove(closest);
				}
			}
			this.handleLinks = handleLinksBuffer.ToArray();
		}

		// Token: 0x0600257C RID: 9596 RVA: 0x00101104 File Offset: 0x000FF304
		[CompilerGenerated]
		internal static Handle <AutoMatchHandles>g__GetClosestHandle|106_0(List<Handle> handles, Handle handle)
		{
			float closestDistanceSqr = float.PositiveInfinity;
			Handle closestHandle = null;
			foreach (Handle behaviour in handles)
			{
				float dSqrToTarget = (behaviour.transform.position - handle.transform.position).sqrMagnitude;
				if (dSqrToTarget < closestDistanceSqr)
				{
					closestDistanceSqr = dSqrToTarget;
					closestHandle = behaviour;
				}
			}
			return closestHandle;
		}

		// Token: 0x040024CE RID: 9422
		[Tooltip("The parent Breakable of this item, if any.")]
		[Header("Parent Breakable")]
		public Breakable parentBreakable;

		// Token: 0x040024CF RID: 9423
		[Tooltip("This is the mesh which is whole, containing JUST the unbroken mesh.")]
		[Header("Object holders")]
		public GameObject unbrokenObjectsHolder;

		// Token: 0x040024D0 RID: 9424
		[Tooltip("Put the parent object containing all the broken meshes here.")]
		public GameObject brokenObjectsHolder;

		// Token: 0x040024D1 RID: 9425
		[Tooltip("When enabled, break points allow the Breakables Item to only break if damaged inside these points. They will not break outside these points.")]
		[Header("Break points")]
		public bool useBreakPoints;

		// Token: 0x040024D2 RID: 9426
		[Tooltip("This is the list of the Break Points if you have any. Currently, only spheres are accepted.")]
		public List<Breakable.BreakPoint> breakPoints = new List<Breakable.BreakPoint>();

		// Token: 0x040024D3 RID: 9427
		[Tooltip("Whether or not the item can be broken, can be changed.")]
		[Header("Damage")]
		public bool canBreak = true;

		// Token: 0x040024D4 RID: 9428
		[Tooltip("Whether or not the item can break on static environment. Defaults to always.")]
		[Header("Damage")]
		public Breakable.EnviroBreakMode breakOnEnviro = Breakable.EnviroBreakMode.Always;

		// Token: 0x040024D5 RID: 9429
		[Tooltip("Whether or not this item can be broken by something that isn't a collision, can be changed.")]
		public bool contactBreakOnly;

		// Token: 0x040024D6 RID: 9430
		[Tooltip("Set if this item should or shouldn't use \"momentum health\". If set to false, the breakable will break in one sufficient hit.")]
		public bool useHealth;

		// Token: 0x040024D7 RID: 9431
		[Tooltip("How much \"health\" the breakable has. This health amount is decreased proportional to the strength of the hit.")]
		public float momentumHealth;

		// Token: 0x040024D8 RID: 9432
		[Tooltip("How much momentum is needed for a hit to count for damage.")]
		public float minimumDamageMomentum = 20f;

		// Token: 0x040024D9 RID: 9433
		[Tooltip("Applies a maximum amount of \"momentum health\" damage that can be dealt in a single hit Can be used to ensure that something can't be broken in a single hit, no matter what.")]
		public bool clampDamage;

		// Token: 0x040024DA RID: 9434
		[Tooltip("The max collision momentum damage that can be taken in a single hit. Only applies if \"clampDamage\" is set to true.")]
		public float maxCollisionMomentum = float.MaxValue;

		// Token: 0x040024DB RID: 9435
		[Tooltip("The min (x) and max (y) values for the mass factor when calculating momentum. Only tweak this if you find that heavy/lighter objects aren't properly affecting the breakable.")]
		public Vector2 momentumMassFactorClamp = new Vector2(1f, 5f);

		// Token: 0x040024DC RID: 9436
		[Tooltip("When enabled, objects under a given mass won't be able to break this item. Minimal mass is defined by \"minimalMassThreshold\"")]
		public bool ignoreObjectUnderCertainMass;

		// Token: 0x040024DD RID: 9437
		[Tooltip("Objects under this mass won't be able to break this item. Used when \"ignoreObjectUnderCertainMass\" is true")]
		public float minimalMassThreshold = 1.01f;

		// Token: 0x040024DE RID: 9438
		[Tooltip("When enabled, the Item can instantly break if the Break Velocity is met.")]
		public bool canInstantaneouslyBreak = true;

		// Token: 0x040024DF RID: 9439
		[Tooltip("Momentum damage required to instantly break the Item.")]
		public float instantaneousBreakDamage = 50f;

		// Token: 0x040024E0 RID: 9440
		[Tooltip("Cooldown between hits for the item to break.")]
		[Header("Time")]
		public float hitCooldownTime = 0.1f;

		// Token: 0x040024E1 RID: 9441
		[Tooltip("When ticked, linked items inside breakable will despawn.")]
		public bool despawnLinkedItem = true;

		// Token: 0x040024E2 RID: 9442
		[Tooltip("Delay before the linked items inside breakable will despawn")]
		public float despawnLinkedItemDelay = 5f;

		// Token: 0x040024E3 RID: 9443
		[Tooltip("These link between handles, which means that if the unbroken item has a handle, and one of the breakables has the same handle, you will keep grabbing the handle despite the held item switching to a broken version.")]
		[Header("Handles")]
		public Breakable.HandleLink[] handleLinks = Array.Empty<Breakable.HandleLink>();

		// Token: 0x040024E4 RID: 9444
		[Header("Explosion forces")]
		[Tooltip("When enabled, broken pieces of this item won't collide. Useful for big items, to avoid jitter.")]
		public bool ignoreCollisionOnBreak;

		// Token: 0x040024E5 RID: 9445
		[Tooltip("When enabled, broken pieces Will be pushed when parallel to the hit direction.")]
		public bool useExplosionForce;

		// Token: 0x040024E6 RID: 9446
		[Tooltip("Multiplication factor used when applying the explosion force. It multiplies the hit force.")]
		public float explosionForceFactor = 50f;

		// Token: 0x040024E7 RID: 9447
		[NonSerialized]
		public List<Item> subUnbrokenItems = new List<Item>();

		// Token: 0x040024E8 RID: 9448
		[NonSerialized]
		public List<PhysicBody> subUnbrokenBodies = new List<PhysicBody>();

		// Token: 0x040024E9 RID: 9449
		[NonSerialized]
		public List<Item> subBrokenItems = new List<Item>();

		// Token: 0x040024EA RID: 9450
		[NonSerialized]
		public List<PhysicBody> subBrokenBodies = new List<PhysicBody>();

		// Token: 0x040024EB RID: 9451
		[NonSerialized]
		public List<PhysicBody> allSubBodies = new List<PhysicBody>();

		// Token: 0x040024EC RID: 9452
		private bool isInitialized;

		// Token: 0x040024ED RID: 9453
		private Rigidbody currentRigidbody;

		// Token: 0x040024EE RID: 9454
		private float lastHitTime;

		// Token: 0x040024EF RID: 9455
		private Vector3[] subBrokenItemsBarycenters;

		// Token: 0x040024F0 RID: 9456
		private List<Damager> piercedDamagers = new List<Damager>();

		// Token: 0x040024F1 RID: 9457
		[TupleElementNames(new string[]
		{
			"transform",
			"body"
		})]
		private List<ValueTuple<Transform, PhysicBody>> bodyTransforms = new List<ValueTuple<Transform, PhysicBody>>();

		// Token: 0x040024F2 RID: 9458
		[Header("Events")]
		public UnityEvent<float> onTakeDamage;

		// Token: 0x040024F3 RID: 9459
		public UnityEvent<float> onNonBreakHit;

		// Token: 0x040024F4 RID: 9460
		public UnityEvent<float> onBreak;

		// Token: 0x040024F5 RID: 9461
		[Header("Deprecated")]
		[Tooltip("Don't change this!")]
		public bool updated;

		// Token: 0x040024F6 RID: 9462
		[Tooltip("Number of hits required for the item to break.")]
		public int hitsUntilBreak = 1;

		// Token: 0x040024F7 RID: 9463
		[Tooltip("How much force is needed for the \"Hits Until Break\" to count as a hit.")]
		public float neededImpactForceToDamage = 20f;

		// Token: 0x040024F8 RID: 9464
		[Tooltip("Force required to instantly break the Item.")]
		public float instantaneousBreakVelocityThreshold = 50f;

		/// <summary>
		/// Used as a buffer to cache collision contact points
		/// </summary>
		// Token: 0x040024F9 RID: 9465
		private ContactPoint[] contactPoints = new ContactPoint[16];

		// Token: 0x040024FA RID: 9466
		private List<Mesh> brokenItemMeshes = new List<Mesh>();

		// Token: 0x040024FE RID: 9470
		[NonSerialized]
		public Collision breakingCollision;

		/// <summary>
		/// Allows two handles to be linked between meshes.
		///
		/// TODO: Possibly overhaul this to instead allow Handle[] and GetClosest to handler?
		///       Also might be worth making this a struct, it should be immutable anyway.
		/// </summary>
		// Token: 0x02000A0D RID: 2573
		[Serializable]
		public class HandleLink
		{
			// Token: 0x06004525 RID: 17701 RVA: 0x001954AB File Offset: 0x001936AB
			public HandleLink(Handle handleMain, Handle handleSecondary)
			{
				this.handleMain = handleMain;
				this.handleSecondary = handleSecondary;
			}

			// Token: 0x040046E3 RID: 18147
			public Handle handleMain;

			// Token: 0x040046E4 RID: 18148
			public Handle handleSecondary;
		}

		/// <summary>
		/// Used to create and edit breakable points targets
		/// </summary>
		// Token: 0x02000A0E RID: 2574
		[Serializable]
		public class BreakPoint
		{
			// Token: 0x06004526 RID: 17702 RVA: 0x001954C1 File Offset: 0x001936C1
			public BreakPoint(Breakable.BreakPoint.BreakPointType type, Vector3 center, float radius, float length)
			{
				this.type = type;
				this.center = center;
				this.radius = radius;
			}

			/// <summary>
			/// Check if the target transform is within the contact point.
			/// </summary>
			// Token: 0x06004527 RID: 17703 RVA: 0x001954E0 File Offset: 0x001936E0
			public bool CheckIfHit(Transform transform, ContactPoint contactPoint)
			{
				Vector3 p = contactPoint.point;
				Vector3 c = transform.TransformPoint(this.center);
				return this.type == Breakable.BreakPoint.BreakPointType.Sphere && (p - c).sqrMagnitude < this.radius * this.radius;
			}

			// Token: 0x040046E5 RID: 18149
			public Breakable.BreakPoint.BreakPointType type;

			// Token: 0x040046E6 RID: 18150
			public Vector3 center;

			// Token: 0x040046E7 RID: 18151
			public float radius;

			// Token: 0x02000BF1 RID: 3057
			public enum BreakPointType
			{
				// Token: 0x04004D6A RID: 19818
				Sphere
			}
		}

		// Token: 0x02000A0F RID: 2575
		public enum EnviroBreakMode
		{
			// Token: 0x040046E9 RID: 18153
			Never,
			// Token: 0x040046EA RID: 18154
			Handled,
			// Token: 0x040046EB RID: 18155
			Always
		}
	}
}
