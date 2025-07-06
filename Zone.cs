using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace ThunderRoad
{
	// Token: 0x020002FA RID: 762
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/Zone.html")]
	public class Zone : ThunderBehaviour
	{
		// Token: 0x0600246A RID: 9322 RVA: 0x000F8CC2 File Offset: 0x000F6EC2
		public void ForceCreatureEnterZone(Creature creature)
		{
			this.CreatureCrossThreshold(creature, true, null);
		}

		// Token: 0x0600246B RID: 9323 RVA: 0x000F8CCD File Offset: 0x000F6ECD
		public void ForceCreatureExitZone(Creature creature)
		{
			this.CreatureCrossThreshold(creature, false, null);
		}

		// Token: 0x0600246C RID: 9324 RVA: 0x000F8CD8 File Offset: 0x000F6ED8
		public void ForceItemEnterZone(Item item)
		{
			this.ItemCrossThreshold(item, true);
		}

		// Token: 0x0600246D RID: 9325 RVA: 0x000F8CE2 File Offset: 0x000F6EE2
		public void ForceItemExitZone(Item item)
		{
			this.ItemCrossThreshold(item, false);
		}

		// Token: 0x1400011D RID: 285
		// (add) Token: 0x0600246E RID: 9326 RVA: 0x000F8CEC File Offset: 0x000F6EEC
		// (remove) Token: 0x0600246F RID: 9327 RVA: 0x000F8D20 File Offset: 0x000F6F20
		public static event Zone.GlobalZoneEvent OnZoneEvent;

		// Token: 0x06002470 RID: 9328 RVA: 0x000F8D54 File Offset: 0x000F6F54
		protected virtual void Awake()
		{
			base.gameObject.layer = Common.zoneLayer;
			this.mainCollider = base.GetComponent<Collider>();
			foreach (Collider collider in base.GetComponents<Collider>())
			{
				collider.isTrigger = true;
				if (this.mainCollider == null)
				{
					this.mainCollider = collider;
				}
				collider.gameObject.layer = Common.zoneLayer;
			}
			this.playerMask = 1 << GameManager.GetLayer(LayerName.PlayerLocomotion);
			this.golemMask = 1 << GameManager.GetLayer(LayerName.PlayerLocomotionObject);
			this.creatureMask = (1 << GameManager.GetLayer(LayerName.NPC) | 1 << GameManager.GetLayer(LayerName.Ragdoll) | 1 << GameManager.GetLayer(LayerName.BodyLocomotion) | 1 << GameManager.GetLayer(LayerName.Avatar));
			this.itemMask = (1 << GameManager.GetLayer(LayerName.MovingItem) | 1 << GameManager.GetLayer(LayerName.DroppedItem));
			if (this.statusIDs == null)
			{
				this.statusIDs = new List<StatusEntry>();
			}
			this.statuses = new Dictionary<StatusData, ValueTuple<float, float?>>();
			if (this.invokePlayerExitOnAwake)
			{
				this.PlayerZoneChange(false);
			}
		}

		// Token: 0x06002471 RID: 9329 RVA: 0x000F8E6C File Offset: 0x000F706C
		public void Start()
		{
			foreach (StatusEntry statusEntry in this.statusIDs)
			{
				string text;
				float num;
				float? num2;
				statusEntry.Deconstruct(out text, out num, out num2);
				string id = text;
				float value = num;
				float? parameter = num2;
				StatusData data = Catalog.GetData<StatusData>(id, true);
				if (data != null)
				{
					this.statuses[data] = new ValueTuple<float, float?>((value == 0f) ? float.PositiveInfinity : value, parameter);
				}
			}
		}

		// Token: 0x06002472 RID: 9330 RVA: 0x000F8EF8 File Offset: 0x000F70F8
		public void TestEvent(UnityEngine.Object obj)
		{
			Debug.Log("Hello " + obj.name);
		}

		// Token: 0x17000233 RID: 563
		// (get) Token: 0x06002473 RID: 9331 RVA: 0x000F8F0F File Offset: 0x000F710F
		// (set) Token: 0x06002474 RID: 9332 RVA: 0x000F8F17 File Offset: 0x000F7117
		public bool playerInZone { get; protected set; }

		// Token: 0x17000234 RID: 564
		// (get) Token: 0x06002475 RID: 9333 RVA: 0x000F8F20 File Offset: 0x000F7120
		// (set) Token: 0x06002476 RID: 9334 RVA: 0x000F8F28 File Offset: 0x000F7128
		public Dictionary<Creature, int> creaturesInZone { get; protected set; } = new Dictionary<Creature, int>();

		// Token: 0x17000235 RID: 565
		// (get) Token: 0x06002477 RID: 9335 RVA: 0x000F8F31 File Offset: 0x000F7131
		// (set) Token: 0x06002478 RID: 9336 RVA: 0x000F8F39 File Offset: 0x000F7139
		public Dictionary<Item, int> itemsInZone { get; protected set; } = new Dictionary<Item, int>();

		// Token: 0x17000236 RID: 566
		// (get) Token: 0x06002479 RID: 9337 RVA: 0x000F8F42 File Offset: 0x000F7142
		private bool activeForce
		{
			get
			{
				return this.linearForceActive || this.radialForceActive || this.swirlForceActive || this.resistiveForceActive;
			}
		}

		// Token: 0x0600247A RID: 9338 RVA: 0x000F8F64 File Offset: 0x000F7164
		protected override void ManagedOnDisable()
		{
			Collider collider = this.mainCollider;
			BoxCollider box = collider as BoxCollider;
			if (box != null)
			{
				this.orgColliderSize = box.size;
				box.size = Vector3.zero;
				return;
			}
			SphereCollider sphere = collider as SphereCollider;
			if (sphere != null)
			{
				this.orgColliderRadius = sphere.radius;
				sphere.radius = 0f;
				return;
			}
			CapsuleCollider capsule = collider as CapsuleCollider;
			if (capsule == null)
			{
				return;
			}
			this.orgColliderRadius = capsule.radius;
			capsule.radius = 0f;
		}

		// Token: 0x0600247B RID: 9339 RVA: 0x000F8FE0 File Offset: 0x000F71E0
		protected override void ManagedOnEnable()
		{
			Collider collider = this.mainCollider;
			BoxCollider box = collider as BoxCollider;
			if (box == null)
			{
				SphereCollider sphere = collider as SphereCollider;
				if (sphere == null)
				{
					CapsuleCollider capsule = collider as CapsuleCollider;
					if (capsule == null)
					{
						return;
					}
					if (this.orgColliderRadius != 0f)
					{
						capsule.radius = this.orgColliderRadius;
					}
				}
				else if (this.orgColliderRadius != 0f)
				{
					sphere.radius = this.orgColliderRadius;
					return;
				}
			}
			else if (this.orgColliderSize != Vector3.zero)
			{
				box.size = this.orgColliderSize;
				return;
			}
		}

		// Token: 0x0600247C RID: 9340 RVA: 0x000F9068 File Offset: 0x000F7268
		private void OnDestroy()
		{
			foreach (KeyValuePair<Creature, int> keyValuePair in this.creaturesInZone)
			{
				Creature creature;
				int num;
				keyValuePair.Deconstruct(out creature, out num);
				creature.ClearByHandler(this);
			}
			foreach (KeyValuePair<Item, int> keyValuePair2 in this.itemsInZone)
			{
				int num;
				Item item;
				keyValuePair2.Deconstruct(out item, out num);
				item.ClearByHandler(this);
			}
		}

		// Token: 0x0600247D RID: 9341 RVA: 0x000F9118 File Offset: 0x000F7318
		public void SetRadius(float radius)
		{
			Collider collider = this.mainCollider;
			BoxCollider box = collider as BoxCollider;
			if (box != null)
			{
				box.size = Vector3.one * radius;
				return;
			}
			SphereCollider sphere = collider as SphereCollider;
			if (sphere != null)
			{
				sphere.radius = radius;
				return;
			}
			CapsuleCollider capsule = collider as CapsuleCollider;
			if (capsule == null)
			{
				return;
			}
			capsule.radius = radius;
		}

		// Token: 0x17000237 RID: 567
		// (get) Token: 0x0600247E RID: 9342 RVA: 0x000F916C File Offset: 0x000F736C
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.FixedUpdate;
			}
		}

		// Token: 0x0600247F RID: 9343 RVA: 0x000F9170 File Offset: 0x000F7370
		protected internal override void ManagedFixedUpdate()
		{
			base.ManagedFixedUpdate();
			if (this.physicBodiesInZone.Count == 0)
			{
				return;
			}
			if (this.constantStatus)
			{
				this.UpdateStatus();
			}
			if (!this.activeForce)
			{
				return;
			}
			if (this.missingBodies == null)
			{
				this.missingBodies = new List<PhysicBody>();
			}
			foreach (PhysicBody pb in this.physicBodiesInZone.Keys)
			{
				Zone.PhysicBodyTypes type;
				if (this.physicBodyTypes.TryGetValue(pb, out type))
				{
					float overallMultiplier = 1f;
					switch (type)
					{
					case Zone.PhysicBodyTypes.PlayerRoot:
						if (!this.forcePlayer)
						{
							continue;
						}
						overallMultiplier = this.playerForceMult;
						break;
					case Zone.PhysicBodyTypes.PlayerPart:
						continue;
					case Zone.PhysicBodyTypes.CreatureRoot:
						if (this.creatureForceMode != Zone.CreatureForceMode.ForceRoot)
						{
							continue;
						}
						overallMultiplier = this.creatureForceMult;
						break;
					case Zone.PhysicBodyTypes.CreaturePart:
						if (this.creatureForceMode != Zone.CreatureForceMode.ForceParts)
						{
							continue;
						}
						overallMultiplier = this.creatureForceMult;
						break;
					case Zone.PhysicBodyTypes.Other:
						if (!this.forceNonCreatures)
						{
							continue;
						}
						overallMultiplier = this.itemForceMult;
						break;
					}
					if (pb.transform == null)
					{
						this.missingBodies.Add(pb);
					}
					else
					{
						Zone zone = this.forceExclusionZone;
						if (zone == null || !zone.physicBodiesInZone.ContainsKey(pb))
						{
							if (this.linearForceActive)
							{
								pb.AddForce((this.linearForce.x * this.forceTransform.right + this.linearForce.y * this.forceTransform.up + this.linearForce.z * this.forceTransform.forward) * overallMultiplier, this.linearForceMode);
							}
							if (this.radialForceActive)
							{
								Vector3 toPB = pb.transform.position - this.forceTransform.position;
								Vector3 force = this.radialForceMultiplier * this.radialForce.Evaluate(Mathf.Clamp(toPB.magnitude, this.radialForce.GetFirstTime(), this.radialForce.GetLastTime())) * toPB.normalized;
								if (this.noDownwardsForce && force.y < 0f)
								{
									force.y = 0f;
								}
								pb.AddForce(force * overallMultiplier, this.radialForceMode);
							}
							if (this.swirlForceActive)
							{
								Vector3 toPB2 = pb.transform.position - this.forceTransform.position;
								float distanceMult = this.swirlForce.Evaluate(Mathf.Clamp(toPB2.magnitude, this.swirlForce.GetFirstTime(), this.swirlForce.GetLastTime()));
								Vector3 rotatedPosition = Quaternion.AngleAxis(this.swirlForceDegrees * (float)(this.swirlRandomDirection ? (pb.GetHashCode() % 2 * 2 - 1) : 1), this.forceTransform.TransformDirection(this.swirlLocalAxis)) * toPB2 * 0.6f;
								Vector3 forceDirection = (this.forceTransform.position + rotatedPosition - pb.transform.position).normalized;
								pb.AddForce(forceDirection * (this.swirlForceMultiplier * distanceMult * overallMultiplier), this.swirlForceMode);
							}
							if (this.resistiveForceActive)
							{
								Vector3 pbVelocity = pb.velocity;
								if (this.resistInForceTransformForward)
								{
									this.resistiveForceDirection = this.forceTransform.forward;
								}
								if (!this.resistiveForceDirection.sqrMagnitude.IsApproximately(0f))
								{
									if (Vector3.Dot(pbVelocity.normalized, -this.resistiveForceDirection) > 0f)
									{
										pbVelocity = Vector3.Project(pbVelocity, -this.resistiveForceDirection);
									}
									else
									{
										pbVelocity = Vector3.zero;
									}
								}
								if (pbVelocity.sqrMagnitude > this.minimumResistedVelocity * this.minimumResistedVelocity)
								{
									pb.AddForce(pbVelocity.normalized * -this.resistiveForce, this.resistiveForceMode);
								}
							}
						}
					}
				}
			}
			for (int i = 0; i < this.missingBodies.Count; i++)
			{
				this.physicBodiesInZone.Remove(this.missingBodies[i]);
			}
			this.missingBodies.Clear();
		}

		// Token: 0x06002480 RID: 9344 RVA: 0x000F95C8 File Offset: 0x000F77C8
		public void UpdateStatus()
		{
			foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
			{
				StatusData statusData;
				ValueTuple<float, float?> valueTuple;
				keyValuePair.Deconstruct(out statusData, out valueTuple);
				ref ValueTuple<float, float?> ptr = valueTuple;
				StatusData status = statusData;
				float? parameter = ptr.Item2;
				float? num;
				if (parameter != null)
				{
					float param = parameter.GetValueOrDefault();
					num = new float?(param * Time.fixedDeltaTime);
				}
				else
				{
					num = null;
				}
				float? floatParam = num;
				if (this.statusOnCreature)
				{
					foreach (KeyValuePair<Creature, int> keyValuePair2 in this.creaturesInZone)
					{
						Creature creature2;
						int num2;
						keyValuePair2.Deconstruct(out creature2, out num2);
						Creature creature = creature2;
						if ((this.statusOnPlayer && creature.isPlayer) || (this.statusOnCreature && !creature.isPlayer))
						{
							creature.Inflict(status, this, float.PositiveInfinity, floatParam, true);
						}
					}
				}
				if (this.statusOnItem)
				{
					foreach (KeyValuePair<Item, int> keyValuePair3 in this.itemsInZone)
					{
						int num2;
						Item item;
						keyValuePair3.Deconstruct(out item, out num2);
						item.Inflict(status, this, float.PositiveInfinity, floatParam, true);
					}
				}
				if (this.golemPartsInZone.Count > 0)
				{
					Golem.local.Inflict(status, this, float.PositiveInfinity, floatParam, true);
				}
			}
		}

		// Token: 0x06002481 RID: 9345 RVA: 0x000F979C File Offset: 0x000F799C
		private void OnTriggerEnter(Collider other)
		{
			this.HandleTriggerChange(other, true);
		}

		// Token: 0x06002482 RID: 9346 RVA: 0x000F97A6 File Offset: 0x000F79A6
		private void OnTriggerExit(Collider other)
		{
			this.HandleTriggerChange(other, false);
		}

		// Token: 0x06002483 RID: 9347 RVA: 0x000F97B0 File Offset: 0x000F79B0
		protected bool IsInLayerMask(int layerMask, int layer)
		{
			return layerMask == (layerMask | 1 << layer);
		}

		// Token: 0x06002484 RID: 9348 RVA: 0x000F97C0 File Offset: 0x000F79C0
		protected void HandleTriggerChange(Collider collider, bool enter)
		{
			try
			{
				PhysicBody pb = collider.GetPhysicBody();
				int objectLayer = (pb != null) ? pb.gameObject.layer : collider.gameObject.layer;
				this.collidersInside += (enter ? 1 : -1);
				if (this.IsInLayerMask(this.playerMask, objectLayer))
				{
					this.PhysicBodyZoneChange(pb, collider, enter, Zone.PhysicBodyTypes.PlayerRoot);
					this.PlayerZoneChange(enter);
					Zone.GlobalZoneEvent onZoneEvent = Zone.OnZoneEvent;
					if (onZoneEvent != null)
					{
						onZoneEvent(this, collider, Zone.ZoneEventType.Player, enter);
					}
				}
				else
				{
					if (this.IsInLayerMask(this.golemMask, objectLayer))
					{
						Golem golem = (pb != null) ? pb.gameObject.GetComponentInParent<Golem>() : null;
						if (golem != null)
						{
							this.PhysicBodyZoneChange(pb, collider, enter, Zone.PhysicBodyTypes.Golem);
							this.GolemZoneChange(golem, pb.rigidBody, enter);
							Zone.GlobalZoneEvent onZoneEvent2 = Zone.OnZoneEvent;
							if (onZoneEvent2 == null)
							{
								goto IL_191;
							}
							onZoneEvent2(this, collider, Zone.ZoneEventType.Golem, enter);
							goto IL_191;
						}
					}
					if (this.IsInLayerMask(this.creatureMask, objectLayer))
					{
						this.PhysicBodyZoneChange(pb, collider, enter, Zone.PhysicBodyTypes.CreatureRoot);
						if (!this.ignorePlayerCreature || objectLayer != GameManager.GetLayer(LayerName.Avatar))
						{
							this.CreatureZoneChange(collider, enter, objectLayer == GameManager.GetLayer(LayerName.BodyLocomotion));
							Zone.GlobalZoneEvent onZoneEvent3 = Zone.OnZoneEvent;
							if (onZoneEvent3 != null)
							{
								onZoneEvent3(this, collider, Zone.ZoneEventType.Creature, enter);
							}
						}
					}
					else if (this.IsInLayerMask(this.itemMask, objectLayer))
					{
						this.PhysicBodyZoneChange(pb, collider, enter, Zone.PhysicBodyTypes.Other);
						this.ItemZoneChange(collider, enter);
						Zone.GlobalZoneEvent onZoneEvent4 = Zone.OnZoneEvent;
						if (onZoneEvent4 != null)
						{
							onZoneEvent4(this, collider, Zone.ZoneEventType.Item, enter);
						}
					}
					else
					{
						if (pb == null)
						{
							Debug.LogWarning(string.Format("Zone is handling a trigger change for a null PhysicBody on collider {0}.", collider));
						}
						this.PhysicBodyZoneChange(pb, collider, enter, Zone.PhysicBodyTypes.Other);
						Zone.GlobalZoneEvent onZoneEvent5 = Zone.OnZoneEvent;
						if (onZoneEvent5 != null)
						{
							onZoneEvent5(this, collider, Zone.ZoneEventType.Other, enter);
						}
					}
				}
				IL_191:;
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in zone {0} when handling trigger change: {1}", base.name, e));
			}
		}

		// Token: 0x06002485 RID: 9349 RVA: 0x000F9998 File Offset: 0x000F7B98
		protected void PlayerZoneChange(bool enter)
		{
			try
			{
				this.playerCollidersEntered += (enter ? 1 : -1);
				if (enter && !this.playerInZone)
				{
					if (this.killPlayer)
					{
						Creature currentCreature = Player.currentCreature;
						if (currentCreature != null)
						{
							currentCreature.Kill();
						}
					}
					if (this.teleportPlayer)
					{
						Player.local.Teleport(this.customTeleportTarget ? this.customTeleportTarget : PlayerSpawner.current.transform, this.keepPlayerVelocity, true);
					}
					if (this.cancelPlayerVelocity)
					{
						Player.local.locomotion.velocity = Vector3.zero;
					}
					if (this.playerVelocityMultOnEnter != 1f)
					{
						Player.local.locomotion.velocity *= this.playerVelocityMultOnEnter;
					}
					if (this.portals.Count > 0)
					{
						bool doEnterEvent = true;
						foreach (ZonePortal zonePortal in this.portals)
						{
							if (zonePortal.IsInside(Player.local.head.transform.position))
							{
								zonePortal.enterEvent.Invoke(Player.local);
								if (zonePortal.preventZoneEnterEvent)
								{
									doEnterEvent = false;
								}
							}
						}
						if (doEnterEvent)
						{
							this.playerEnterEvent.Invoke(Player.local);
						}
					}
					else
					{
						this.playerEnterEvent.Invoke(Player.local);
					}
				}
				this.playerInZone = (this.playerCollidersEntered > 0);
				if (!enter && !this.playerInZone)
				{
					if (this.portals.Count > 0)
					{
						bool doExitEvent = true;
						foreach (ZonePortal zonePortal2 in this.portals)
						{
							if (zonePortal2.IsInside(Player.local.head.transform.position))
							{
								zonePortal2.exitEvent.Invoke(Player.local);
								if (zonePortal2.preventZoneExitEvent)
								{
									doExitEvent = false;
								}
							}
						}
						if (doExitEvent)
						{
							this.playerExitEvent.Invoke(Player.local);
						}
					}
					else
					{
						this.playerExitEvent.Invoke(Player.local);
					}
					this.playerCollidersEntered = 0;
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in zone {0} when handling player zone change: {1}", base.name, e));
			}
		}

		// Token: 0x06002486 RID: 9350 RVA: 0x000F9C30 File Offset: 0x000F7E30
		protected void CreatureZoneChange(Collider collider, bool enter, bool bodyLoc)
		{
			try
			{
				if (!(collider.attachedRigidbody == null))
				{
					Creature creature = null;
					RagdollPart part = null;
					if (bodyLoc)
					{
						if (collider.attachedRigidbody.TryGetComponent<Creature>(out creature))
						{
							if (!creature.initialized || !creature.loaded)
							{
								base.StartCoroutine(this.WaitCreatureToLoad(creature, collider, enter, bodyLoc));
							}
							else if (!creature.ragdoll.IsPhysicsEnabled(false))
							{
								goto IL_EC;
							}
						}
						return;
					}
					if (collider.attachedRigidbody.TryGetComponent<RagdollPart>(out part))
					{
						if (!part.initialized)
						{
							Creature creatureFromPart = part.GetComponentInParent<Creature>();
							if (creatureFromPart)
							{
								base.StartCoroutine(this.WaitCreatureToLoad(creatureFromPart, collider, enter, bodyLoc));
							}
							else
							{
								Debug.LogError("RagdollPart " + part.name + " have no creature at root!");
							}
							return;
						}
						if (this.ignoreNonRootParts && part != part.ragdoll.rootPart)
						{
							return;
						}
						creature = part.ragdoll.creature;
					}
					IL_EC:
					if (!(creature == null))
					{
						if (this.ignoreIdleCreatures && creature.brain)
						{
							Brain.State state = creature.brain.state;
							if (state == Brain.State.Idle || state == Brain.State.Patrol)
							{
								return;
							}
						}
						int current;
						int start = this.creaturesInZone.TryGetValue(creature, out current) ? current : 0;
						if (start != 0 || enter)
						{
							current = (this.creaturesInZone[creature] = start + (enter ? 1 : -1));
							if ((enter && start == 0) || (!enter && current == 0))
							{
								this.CreatureCrossThreshold(creature, enter, part);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in zone {0} when handling creature zone change: {1}", base.name, e));
			}
		}

		// Token: 0x06002487 RID: 9351 RVA: 0x000F9DEC File Offset: 0x000F7FEC
		protected void CreatureCrossThreshold(Creature creature, bool enter, RagdollPart part = null)
		{
			Zone.<>c__DisplayClass139_0 CS$<>8__locals1 = new Zone.<>c__DisplayClass139_0();
			CS$<>8__locals1.creature = creature;
			CS$<>8__locals1.<>4__this = this;
			if (enter)
			{
				CS$<>8__locals1.creature.OnDespawnEvent += CS$<>8__locals1.<CreatureCrossThreshold>g__CreatureDespawn|0;
				this.creatureEnterEvent.Invoke(CS$<>8__locals1.creature);
				if (!this.constantStatus && (this.statusOnCreature || this.statusOnPlayer))
				{
					foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
					{
						StatusData statusData;
						ValueTuple<float, float?> valueTuple;
						keyValuePair.Deconstruct(out statusData, out valueTuple);
						ref ValueTuple<float, float?> ptr = valueTuple;
						StatusData status = statusData;
						float? parameter = ptr.Item2;
						if ((CS$<>8__locals1.creature.isPlayer && this.statusOnPlayer) || (!CS$<>8__locals1.creature.isPlayer && this.statusOnCreature))
						{
							CS$<>8__locals1.creature.Inflict(status, this, float.PositiveInfinity, parameter, this.playStatusEffects);
						}
					}
				}
				if (this.disableArmorDetection)
				{
					CS$<>8__locals1.creature.ragdoll.meshRaycast = false;
				}
				if (this.creaturesInZone.Count <= 1)
				{
					this.firstCreatureEnterEvent.Invoke(CS$<>8__locals1.creature);
				}
				if (this.creaturesInZone.Count <= 1 && this.itemsInZone.Count <= 0)
				{
					this.firstEntityEnterEvent.Invoke(CS$<>8__locals1.creature);
				}
				CS$<>8__locals1.creature.InvokeZoneEvent(this, true);
				if (this.spawnEffect)
				{
					if (CS$<>8__locals1.creature.ragdoll.IsPhysicsEnabled(CS$<>8__locals1.creature.isPlayer))
					{
						this.SpawnZoneEffect((part != null) ? part.transform.position : CS$<>8__locals1.creature.ragdoll.rootPart.transform.position, CS$<>8__locals1.creature.ragdoll.totalMass, CS$<>8__locals1.creature.ragdoll.rootPart.physicBody.velocity.magnitude);
					}
					else
					{
						this.SpawnZoneEffect(CS$<>8__locals1.creature.transform.position, CS$<>8__locals1.creature.ragdoll.totalMass, CS$<>8__locals1.creature.locomotion.physicBody.velocity.magnitude);
					}
				}
				if (!CS$<>8__locals1.creature.loaded)
				{
					return;
				}
				if (!CS$<>8__locals1.creature.isPlayer && this.blockPhysicsCulling)
				{
					CS$<>8__locals1.creature.ragdoll.forcePhysic.Add(this);
				}
				if ((!CS$<>8__locals1.creature.isPlayer && this.killNPC) || (CS$<>8__locals1.creature.isPlayer && this.killPlayer))
				{
					CS$<>8__locals1.creature.Kill();
				}
				if (!CS$<>8__locals1.creature.isPlayer && this.despawnNPC)
				{
					CS$<>8__locals1.creature.Despawn(this.despawnDelay);
				}
				if (!CS$<>8__locals1.creature.isPlayer && this.cancelCreatureVelocity)
				{
					CS$<>8__locals1.creature.ragdoll.CancelVelocity();
				}
				if (!CS$<>8__locals1.creature.isPlayer && this.creatureVelocityMultOnEnter != 1f)
				{
					CS$<>8__locals1.creature.ragdoll.MultiplyVelocity(this.creatureVelocityMultOnEnter);
					return;
				}
			}
			else
			{
				this.creatureExitEvent.Invoke(CS$<>8__locals1.creature);
				CS$<>8__locals1.creature.InvokeZoneEvent(this, false);
				if (!CS$<>8__locals1.creature.isPlayer && this.blockPhysicsCulling)
				{
					CS$<>8__locals1.creature.ragdoll.forcePhysic.Remove(this);
				}
				if (!this.constantStatus)
				{
					foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
					{
						StatusData statusData;
						ValueTuple<float, float?> valueTuple;
						keyValuePair.Deconstruct(out statusData, out valueTuple);
						ValueTuple<float, float?> valueTuple2 = valueTuple;
						StatusData status2 = statusData;
						float duration = valueTuple2.Item1;
						float? parameter2 = valueTuple2.Item2;
						if (duration != 0f && float.IsFinite(duration) && ((CS$<>8__locals1.creature.isPlayer && this.statusOnPlayer) || (!CS$<>8__locals1.creature.isPlayer && this.statusOnCreature)))
						{
							CS$<>8__locals1.creature.Inflict(status2, this, duration, parameter2, this.playStatusEffects);
						}
						else
						{
							CS$<>8__locals1.creature.Remove(status2, this);
						}
					}
				}
				if (this.disableArmorDetection)
				{
					CS$<>8__locals1.creature.ragdoll.meshRaycast = true;
				}
				this.creaturesInZone.Remove(CS$<>8__locals1.creature);
				if (this.creaturesInZone.Count <= 0)
				{
					this.lastCreatureExitEvent.Invoke(CS$<>8__locals1.creature);
				}
				if (this.creaturesInZone.Count <= 0 && this.itemsInZone.Count <= 0)
				{
					this.lastEntityExitEvent.Invoke(CS$<>8__locals1.creature);
				}
			}
		}

		// Token: 0x06002488 RID: 9352 RVA: 0x000FA2C8 File Offset: 0x000F84C8
		protected void GolemZoneChange(Golem golem, Rigidbody rb, bool enter)
		{
			try
			{
				if (enter != this.golemPartsInZone.Contains(rb))
				{
					if (!enter)
					{
						this.golemPartsInZone.Remove(rb);
					}
					bool flag = this.golemPartsInZone.Count == 0;
					if (enter)
					{
						this.golemPartsInZone.Add(rb);
					}
					if (flag)
					{
						if (enter)
						{
							this.golemEnterEvent.Invoke(golem);
							if (!this.statusOnCreature || this.constantStatus)
							{
								goto IL_176;
							}
							using (Dictionary<StatusData, ValueTuple<float, float?>>.Enumerator enumerator = this.statuses.GetEnumerator())
							{
								while (enumerator.MoveNext())
								{
									KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair = enumerator.Current;
									StatusData statusData;
									ValueTuple<float, float?> valueTuple;
									keyValuePair.Deconstruct(out statusData, out valueTuple);
									ref ValueTuple<float, float?> ptr = valueTuple;
									StatusData status = statusData;
									float? parameter = ptr.Item2;
									golem.Inflict(status, this, float.PositiveInfinity, parameter, this.playStatusEffects);
								}
								goto IL_176;
							}
						}
						this.golemExitEvent.Invoke(golem);
						if (!this.constantStatus)
						{
							foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
							{
								StatusData statusData;
								ValueTuple<float, float?> valueTuple;
								keyValuePair.Deconstruct(out statusData, out valueTuple);
								ValueTuple<float, float?> valueTuple2 = valueTuple;
								StatusData status2 = statusData;
								float duration = valueTuple2.Item1;
								float? parameter2 = valueTuple2.Item2;
								if (duration != 0f && float.IsFinite(duration) && this.statusOnCreature)
								{
									golem.Inflict(status2, this, duration, parameter2, this.playStatusEffects);
								}
								else
								{
									golem.Remove(status2, this);
								}
							}
						}
						IL_176:;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in zone {0} when handling golem zone change: {1}", base.name, e));
			}
		}

		// Token: 0x06002489 RID: 9353 RVA: 0x000FA4B4 File Offset: 0x000F86B4
		public IEnumerator WaitCreatureToLoad(Creature creature, Collider collider, bool enter, bool bodyLoc)
		{
			while (!creature.initialized || !creature.loaded)
			{
				yield return new WaitForEndOfFrame();
			}
			this.CreatureZoneChange(collider, enter, bodyLoc);
			yield break;
		}

		// Token: 0x0600248A RID: 9354 RVA: 0x000FA4E0 File Offset: 0x000F86E0
		protected void ItemZoneChange(Collider collider, bool enter)
		{
			try
			{
				if (!(collider.attachedRigidbody == null))
				{
					Item item;
					if (!collider.attachedRigidbody.TryGetComponent<Item>(out item))
					{
						Rigidbody attachedRigidbody = collider.attachedRigidbody;
						item = ((attachedRigidbody != null) ? attachedRigidbody.GetComponentInParent<Item>() : null);
					}
					if (!(item == null))
					{
						int current;
						int start = this.itemsInZone.TryGetValue(item, out current) ? current : 0;
						if (start != 0 || enter)
						{
							current = (this.itemsInZone[item] = start + (enter ? 1 : -1));
							if ((enter && start == 0) || (!enter && current == 0))
							{
								this.ItemCrossThreshold(item, enter);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in zone {0} when handling item zone change: {1}", base.name, e));
			}
		}

		// Token: 0x0600248B RID: 9355 RVA: 0x000FA5A4 File Offset: 0x000F87A4
		protected void ItemCrossThreshold(Item item, bool enter)
		{
			Zone.<>c__DisplayClass143_0 CS$<>8__locals1 = new Zone.<>c__DisplayClass143_0();
			CS$<>8__locals1.item = item;
			CS$<>8__locals1.<>4__this = this;
			if (enter)
			{
				CS$<>8__locals1.item.OnDespawnEvent += CS$<>8__locals1.<ItemCrossThreshold>g__ItemDespawn|0;
				CS$<>8__locals1.item.InvokeZoneEvent(this, true);
				if (this.statusOnItem && !this.constantStatus)
				{
					foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
					{
						StatusData statusData;
						ValueTuple<float, float?> valueTuple;
						keyValuePair.Deconstruct(out statusData, out valueTuple);
						ref ValueTuple<float, float?> ptr = valueTuple;
						StatusData status = statusData;
						float? parameter = ptr.Item2;
						CS$<>8__locals1.item.Inflict(status, this, float.PositiveInfinity, parameter, this.playStatusEffects);
					}
				}
				if (this.spawnEffect)
				{
					this.SpawnZoneEffect(CS$<>8__locals1.item.transform.position, CS$<>8__locals1.item.physicBody.mass, CS$<>8__locals1.item.physicBody.velocity.magnitude);
				}
				if (this.teleportItem)
				{
					if (CS$<>8__locals1.item.IsHanded())
					{
						for (int i = CS$<>8__locals1.item.handlers.Count - 1; i >= 0; i--)
						{
							CS$<>8__locals1.item.handlers[i].UnGrab(false);
						}
					}
					if (CS$<>8__locals1.item.isTelekinesisGrabbed)
					{
						foreach (Handle handle in CS$<>8__locals1.item.handles)
						{
							handle.ReleaseAllTkHandlers();
						}
					}
					if (this.customTeleportTarget != null)
					{
						CS$<>8__locals1.item.transform.SetPositionAndRotation(this.customTeleportTarget.position, this.customTeleportTarget.rotation);
					}
					else
					{
						CS$<>8__locals1.item.transform.SetPositionAndRotation(PlayerSpawner.current.transform.position, PlayerSpawner.current.transform.rotation);
					}
					if (!this.keepItemVelocity)
					{
						CS$<>8__locals1.item.physicBody.velocity = Vector3.zero;
						CS$<>8__locals1.item.physicBody.angularVelocity = Vector3.zero;
					}
				}
				if (this.despawnItem && !CS$<>8__locals1.item.IsHanded() && !CS$<>8__locals1.item.isTelekinesisGrabbed)
				{
					CS$<>8__locals1.item.fellOutOfBounds = true;
					CS$<>8__locals1.item.Despawn(this.despawnDelay);
				}
				if (this.breakBreakables)
				{
					Breakable breakable = CS$<>8__locals1.item.breakable;
					if (breakable != null)
					{
						breakable.Break();
					}
				}
				if (this.cancelItemVelocity)
				{
					CS$<>8__locals1.item.physicBody.velocity = Vector3.zero;
				}
				if (this.itemVelocityMultOnEnter != 1f)
				{
					CS$<>8__locals1.item.physicBody.velocity *= this.itemVelocityMultOnEnter;
				}
				this.itemEnterEvent.Invoke(CS$<>8__locals1.item);
				if (this.itemsInZone.Count <= 1)
				{
					this.firstItemEnterEvent.Invoke(CS$<>8__locals1.item);
				}
				if (this.itemsInZone.Count <= 1 && this.creaturesInZone.Count <= 0)
				{
					this.firstEntityEnterEvent.Invoke(CS$<>8__locals1.item);
					return;
				}
			}
			else
			{
				CS$<>8__locals1.item.InvokeZoneEvent(this, false);
				if (this.spawnEffect)
				{
					this.SpawnZoneEffect(CS$<>8__locals1.item.transform.position, CS$<>8__locals1.item.physicBody.mass, CS$<>8__locals1.item.physicBody.velocity.magnitude);
				}
				this.itemExitEvent.Invoke(CS$<>8__locals1.item);
				if (!this.constantStatus)
				{
					foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
					{
						StatusData statusData;
						ValueTuple<float, float?> valueTuple;
						keyValuePair.Deconstruct(out statusData, out valueTuple);
						ValueTuple<float, float?> valueTuple2 = valueTuple;
						StatusData status2 = statusData;
						float duration = valueTuple2.Item1;
						float? parameter2 = valueTuple2.Item2;
						if (duration != 0f && float.IsFinite(duration) && this.statusOnItem)
						{
							CS$<>8__locals1.item.Inflict(status2, this, duration, parameter2, this.playStatusEffects);
						}
						else
						{
							CS$<>8__locals1.item.Remove(status2, this);
						}
					}
				}
				this.itemsInZone.Remove(CS$<>8__locals1.item);
				if (this.itemsInZone.Count <= 0)
				{
					this.lastItemExitEvent.Invoke(CS$<>8__locals1.item);
				}
				if (this.itemsInZone.Count <= 0 && this.creaturesInZone.Count <= 0)
				{
					this.lastEntityExitEvent.Invoke(CS$<>8__locals1.item);
				}
			}
		}

		// Token: 0x0600248C RID: 9356 RVA: 0x000FAA74 File Offset: 0x000F8C74
		protected void PhysicBodyZoneChange(PhysicBody pb, Collider collider, bool enter, Zone.PhysicBodyTypes type)
		{
			if (pb == null)
			{
				return;
			}
			try
			{
				RagdollPart part;
				Creature creature;
				if (type == Zone.PhysicBodyTypes.CreatureRoot && pb.gameObject.TryGetComponent<RagdollPart>(out part))
				{
					this.<PhysicBodyZoneChange>g__SetRagdollPhysical|144_0(part.ragdoll);
					type = Zone.PhysicBodyTypes.PlayerPart + (part.ragdoll.creature.isPlayer ? 0 : 2);
				}
				else if (type == Zone.PhysicBodyTypes.CreatureRoot && pb.gameObject.TryGetComponent<Creature>(out creature))
				{
					this.<PhysicBodyZoneChange>g__SetRagdollPhysical|144_0(creature.ragdoll);
					type = (creature.isPlayer ? Zone.PhysicBodyTypes.PlayerRoot : Zone.PhysicBodyTypes.CreatureRoot);
				}
				this.physicBodyTypes[pb] = type;
				int current;
				int start = this.physicBodiesInZone.TryGetValue(pb, out current) ? current : 0;
				if (start != 0 || enter)
				{
					current = (this.physicBodiesInZone[pb] = start + (enter ? 1 : -1));
					if (!enter && current == 0)
					{
						this.physicBodiesInZone.Remove(pb);
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in zone {0} when handling physic body zone change: {1}", base.name, e));
			}
		}

		// Token: 0x0600248D RID: 9357 RVA: 0x000FAB7C File Offset: 0x000F8D7C
		public void SpawnZoneEffect(Vector3 position, float mass, float velocityMagnitude)
		{
			if (!string.IsNullOrEmpty(this.effectID))
			{
				if (this.effectData == null || this.effectData.id != this.effectID)
				{
					this.effectData = Catalog.GetData<EffectData>(this.effectID, true);
				}
				if (this.effectData == null)
				{
					return;
				}
				Vector3 hitPoint = this.mainCollider.ClosestPoint(position);
				Quaternion rotation = this.orientEffectToEntranceXZNormal ? Quaternion.LookRotation(Vector3.ProjectOnPlane(position - base.transform.position, Vector3.up)) : (base.transform.rotation * Quaternion.LookRotation(this.effectOrientation));
				EffectInstance effectInstance = this.effectData.Spawn(hitPoint, rotation, null, null, true, null, false, 1f, 1f, Array.Empty<Type>());
				effectInstance.SetIntensity(this.effectMassVelocityCurve.Evaluate(mass * velocityMagnitude));
				effectInstance.source = this;
				effectInstance.Play(0, false, false);
			}
		}

		// Token: 0x0600248E RID: 9358 RVA: 0x000FAC6C File Offset: 0x000F8E6C
		public void ForceEndStatuses()
		{
			foreach (KeyValuePair<Creature, int> keyValuePair in this.creaturesInZone)
			{
				Creature creature;
				int num;
				keyValuePair.Deconstruct(out creature, out num);
				creature.ClearByHandler(this);
			}
			foreach (KeyValuePair<Item, int> keyValuePair2 in this.itemsInZone)
			{
				int num;
				Item item;
				keyValuePair2.Deconstruct(out item, out num);
				item.ClearByHandler(this);
			}
			if (Golem.local)
			{
				Golem.local.ClearByHandler(this);
			}
		}

		// Token: 0x0600248F RID: 9359 RVA: 0x000FAD34 File Offset: 0x000F8F34
		public void RemoveItem(Item item)
		{
			item.InvokeZoneEvent(this, false);
			this.itemExitEvent.Invoke(item);
			this.itemsInZone.Remove(item);
			if (this.statusOnItem && !this.constantStatus)
			{
				foreach (KeyValuePair<StatusData, ValueTuple<float, float?>> keyValuePair in this.statuses)
				{
					StatusData statusData;
					ValueTuple<float, float?> valueTuple;
					keyValuePair.Deconstruct(out statusData, out valueTuple);
					ValueTuple<float, float?> valueTuple2 = valueTuple;
					StatusData status = statusData;
					float duration = valueTuple2.Item1;
					float? parameter = valueTuple2.Item2;
					if (duration != 0f && float.IsFinite(duration))
					{
						item.Inflict(status, this, duration, parameter, this.playStatusEffects);
					}
					else
					{
						item.Remove(status, this);
					}
				}
			}
			if (this.itemsInZone.Count <= 0)
			{
				this.lastItemExitEvent.Invoke(item);
			}
			if (this.itemsInZone.Count <= 0 && this.creaturesInZone.Count <= 0)
			{
				this.lastEntityExitEvent.Invoke(item);
			}
		}

		// Token: 0x06002491 RID: 9361 RVA: 0x000FB01A File Offset: 0x000F921A
		[CompilerGenerated]
		private void <PhysicBodyZoneChange>g__SetRagdollPhysical|144_0(Ragdoll ragdoll)
		{
			if (ragdoll.creature.isPlayer)
			{
				return;
			}
			if (this.creatureForceMode != Zone.CreatureForceMode.ForceParts)
			{
				return;
			}
			if (ragdoll.IsAnimationEnabled(false))
			{
				ragdoll.SetState(this.physicalCreatureState);
			}
		}

		// Token: 0x040023C4 RID: 9156
		[Tooltip("Causes the Player Exit event to be invoked immediately when the zone is loaded into the level.")]
		public bool invokePlayerExitOnAwake;

		// Token: 0x040023C5 RID: 9157
		[Header("Navigation")]
		[Tooltip("When ticked, Adjusts the speed of the player/NPC when inside the zone.")]
		public bool navSpeedModifier;

		// Token: 0x040023C6 RID: 9158
		[Tooltip("Speed adjustment of the player/NPC when inside the zone")]
		public float runSpeed;

		// Token: 0x040023C7 RID: 9159
		[Header("Kill")]
		[Tooltip("When player enter the zone, the player dies.")]
		public bool killPlayer;

		// Token: 0x040023C8 RID: 9160
		[Tooltip("When NPC enter the zone, the NPC dies.")]
		public bool killNPC;

		// Token: 0x040023C9 RID: 9161
		[Header("Despawn")]
		[Tooltip("When NPC enters the zone, the NPC despawns.")]
		public bool despawnNPC;

		// Token: 0x040023CA RID: 9162
		[Tooltip("When an Item enters the zone, the item despawns.")]
		public bool despawnItem;

		// Token: 0x040023CB RID: 9163
		[Tooltip("Delay of which the NPC/Item despawns once they enter the zone.")]
		public float despawnDelay;

		// Token: 0x040023CC RID: 9164
		[Header("FX")]
		[Tooltip("Spawns Effect when the zone is entered by Player/NPC/Item")]
		public bool spawnEffect;

		// Token: 0x040023CD RID: 9165
		[Tooltip("ID of the effect spawned.")]
		public string effectID;

		// Token: 0x040023CE RID: 9166
		[Tooltip("This curve maps the (mass * velocity) of the interactor at the moment of entry to the intensity of the effect.")]
		public AnimationCurve effectMassVelocityCurve;

		// Token: 0x040023CF RID: 9167
		[Tooltip("The euler rotation of the spawned effect.")]
		public Vector3 effectOrientation = Vector3.forward;

		// Token: 0x040023D0 RID: 9168
		[FormerlySerializedAs("orientToEntranceXZNormal")]
		[Tooltip("Whether to orient the effect to point outwards from the point of exit, projected onto the XZ plane.")]
		public bool orientEffectToEntranceXZNormal;

		// Token: 0x040023D1 RID: 9169
		[Header("Teleport")]
		[Tooltip("Teleports player to the Custom Teleport Target when player enters zone.")]
		public bool teleportPlayer;

		// Token: 0x040023D2 RID: 9170
		[Tooltip("Whether or not to keep the player's velocity when they teleport.")]
		public bool keepPlayerVelocity;

		// Token: 0x040023D3 RID: 9171
		[Tooltip("Teleports item(s) to the Custom Teleport Target when the item(s) enter the zone")]
		public bool teleportItem;

		// Token: 0x040023D4 RID: 9172
		[Tooltip("Whether or not to keep an item's velocity when it teleports.")]
		public bool keepItemVelocity;

		// Token: 0x040023D5 RID: 9173
		[Tooltip("GameObject of which the position is where the player/item teleports to.")]
		public Transform customTeleportTarget;

		// Token: 0x040023D6 RID: 9174
		[Header("Statuses")]
		[Tooltip("If true, inflicts the status effects constantly to anything in the zone.")]
		public bool constantStatus;

		// Token: 0x040023D7 RID: 9175
		[Tooltip("Status Effect IDs to inflict/remove on entry/exit")]
		public List<StatusEntry> statusIDs;

		// Token: 0x040023D8 RID: 9176
		[Tooltip("When NPC/Item is in zone, does it apply a status effect?")]
		public bool playStatusEffects = true;

		// Token: 0x040023D9 RID: 9177
		[Tooltip("Does the status affect Players?")]
		public bool statusOnPlayer;

		// Token: 0x040023DA RID: 9178
		[Tooltip("Does the status affect Creatures?")]
		public bool statusOnCreature;

		// Token: 0x040023DB RID: 9179
		[Tooltip("Does the status affect Items?")]
		public bool statusOnItem;

		// Token: 0x040023DC RID: 9180
		[Tooltip("When enabled, it will disable armor detection on NPCs inside the zone.")]
		public bool disableArmorDetection;

		// Token: 0x040023DD RID: 9181
		[NonSerialized]
		public Dictionary<StatusData, ValueTuple<float, float?>> statuses;

		// Token: 0x040023DE RID: 9182
		[Header("Force")]
		[Tooltip("Does the applied force affect non-creatures?")]
		public bool forceNonCreatures;

		// Token: 0x040023DF RID: 9183
		[Tooltip("Does the applied force affect the player?")]
		public bool forcePlayer;

		// Token: 0x040023E0 RID: 9184
		[Tooltip("How should the zone apply force to NPCs?\n\nNo Force: Applies no force\n\nForce Root: Applies force on the root of the creature\n\nForce Parts: Applies force to all parts of the creature. This will distabilize creatures beforehand")]
		public Zone.CreatureForceMode creatureForceMode;

		// Token: 0x040023E1 RID: 9185
		[Tooltip("Sets the origin point and orientation for forces applied by this zone")]
		public Transform forceTransform;

		// Token: 0x040023E2 RID: 9186
		[Tooltip("Does the force push the object in a linear direction? (X, Y, Z)")]
		public bool linearForceActive;

		// Token: 0x040023E3 RID: 9187
		[Tooltip("What force mode does the linear force apply?")]
		public ForceMode linearForceMode;

		// Token: 0x040023E4 RID: 9188
		[Tooltip("What direction (relative to the force transform) does the linear force push the object?")]
		public Vector3 linearForce;

		// Token: 0x040023E5 RID: 9189
		[Tooltip("Does the force push outwards from the force transform point?")]
		public bool radialForceActive;

		// Token: 0x040023E6 RID: 9190
		[Tooltip("What force mode does the radial force apply?")]
		public ForceMode radialForceMode;

		// Token: 0x040023E7 RID: 9191
		[Tooltip("Output radial force by distance from origin\n\nVertical axis: Force\n\nHorizontal axis: Distance")]
		public AnimationCurve radialForce = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 1f)
		});

		// Token: 0x040023E8 RID: 9192
		[Tooltip("Output radial force gets multiplied by this value")]
		public float radialForceMultiplier = 1f;

		// Token: 0x040023E9 RID: 9193
		[Tooltip("Should downwards force be negated when applying radial force?")]
		public bool noDownwardsForce;

		// Token: 0x040023EA RID: 9194
		[Tooltip("Does the force try to swirl objects around the force transform point?")]
		public bool swirlForceActive;

		// Token: 0x040023EB RID: 9195
		[Tooltip("What force mode does the swirl force force apply?")]
		public ForceMode swirlForceMode;

		// Token: 0x040023EC RID: 9196
		[Tooltip("Output swirl force by distance from origin\n\nVertical axis: Force\n\nHorizontal axis: Distance")]
		public AnimationCurve swirlForce = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 1f)
		});

		// Token: 0x040023ED RID: 9197
		[Tooltip("Output swirl force gets multiplied by this value")]
		public float swirlForceMultiplier = 1f;

		// Token: 0x040023EE RID: 9198
		[Tooltip("Picks a random direction (forwards or backwards) for every object that gets pushed by the swirl force")]
		public bool swirlRandomDirection;

		// Token: 0x040023EF RID: 9199
		[Tooltip("How far objects should be pulled when swirled")]
		public float swirlForceDegrees;

		// Token: 0x040023F0 RID: 9200
		[Tooltip("Determines the axis around which objects are swirled, relative to the force transform.")]
		public Vector3 swirlLocalAxis = Vector3.up;

		// Token: 0x040023F1 RID: 9201
		[Tooltip("Does the force resist an object's velocity?")]
		public bool resistiveForceActive;

		// Token: 0x040023F2 RID: 9202
		[Tooltip("The amount of force to apply opposite to an object's movement")]
		public float resistiveForce;

		// Token: 0x040023F3 RID: 9203
		[Tooltip("What force mode does the resistive force apply?")]
		public ForceMode resistiveForceMode;

		// Token: 0x040023F4 RID: 9204
		[Tooltip("A minimum velocity for the resistive force to activate")]
		public float minimumResistedVelocity;

		// Token: 0x040023F5 RID: 9205
		[Tooltip("Should the resistive force only work in one direction?\n\nIf true, this allows the resistance force direction to update with the force transform direction\n\nIf false, use the direction specified in resistive force direction")]
		public bool resistInForceTransformForward;

		// Token: 0x040023F6 RID: 9206
		[Tooltip("Should the resistive force only work in one direction? Only works if the magnitude of this vector is greater than 0")]
		public Vector3 resistiveForceDirection = Vector3.zero;

		// Token: 0x040023F7 RID: 9207
		[Tooltip("A secondary zone; if an object is in this zone, it doesn't receive force from this zone")]
		public Zone forceExclusionZone;

		// Token: 0x040023F8 RID: 9208
		[Tooltip("Cancel the existing velocity of incoming items")]
		public bool cancelItemVelocity;

		// Token: 0x040023F9 RID: 9209
		[Tooltip("Cancel the existing velocity of incoming NPCs")]
		public bool cancelCreatureVelocity;

		// Token: 0x040023FA RID: 9210
		[Tooltip("Cancel the existing velocity of incoming player")]
		public bool cancelPlayerVelocity;

		// Token: 0x040023FB RID: 9211
		[Tooltip("Multiply the velocity of an item on enter")]
		public float itemVelocityMultOnEnter = 1f;

		// Token: 0x040023FC RID: 9212
		[Tooltip("Multiply the velocity of an NPC on enter")]
		public float creatureVelocityMultOnEnter = 1f;

		// Token: 0x040023FD RID: 9213
		[Tooltip("Multiply the velocity of the player on enter")]
		public float playerVelocityMultOnEnter = 1f;

		// Token: 0x040023FE RID: 9214
		[Tooltip("An additional multiplier on force applied to items")]
		public float itemForceMult = 1f;

		// Token: 0x040023FF RID: 9215
		[Tooltip("An additional multiplier on force applied to NPCs")]
		public float creatureForceMult = 1f;

		// Token: 0x04002400 RID: 9216
		[Tooltip("An additional multiplier on force applied to the player")]
		public float playerForceMult = 1f;

		// Token: 0x04002401 RID: 9217
		[Header("Creature settings")]
		[Tooltip("If enabled, this zone will not work with the player creature.")]
		public bool ignorePlayerCreature;

		// Token: 0x04002402 RID: 9218
		[Tooltip("If enabled, the zone will only react to the root bone of creatures (hip bone).")]
		public bool ignoreNonRootParts = true;

		// Token: 0x04002403 RID: 9219
		[Tooltip("If enabled, this zone will not react to creatures who are idle.")]
		public bool ignoreIdleCreatures;

		// Token: 0x04002404 RID: 9220
		[Tooltip("If enabled, creatures in or touching this zone will have physics culling forced off.")]
		public bool blockPhysicsCulling;

		// Token: 0x04002405 RID: 9221
		public Ragdoll.State physicalCreatureState = Ragdoll.State.Destabilized;

		// Token: 0x04002406 RID: 9222
		[Header("Portals")]
		public List<ZonePortal> portals = new List<ZonePortal>();

		// Token: 0x04002407 RID: 9223
		[Header("Breakables")]
		public bool breakBreakables;

		// Token: 0x04002408 RID: 9224
		[Header("Event")]
		public Zone.ZoneEvent playerEnterEvent = new Zone.ZoneEvent();

		// Token: 0x04002409 RID: 9225
		public Zone.ZoneEvent playerExitEvent = new Zone.ZoneEvent();

		// Token: 0x0400240A RID: 9226
		public Zone.ZoneEvent golemEnterEvent = new Zone.ZoneEvent();

		// Token: 0x0400240B RID: 9227
		public Zone.ZoneEvent golemExitEvent = new Zone.ZoneEvent();

		// Token: 0x0400240C RID: 9228
		public Zone.ZoneEvent creatureEnterEvent = new Zone.ZoneEvent();

		// Token: 0x0400240D RID: 9229
		public Zone.ZoneEvent creatureExitEvent = new Zone.ZoneEvent();

		// Token: 0x0400240E RID: 9230
		public Zone.ZoneEvent firstCreatureEnterEvent = new Zone.ZoneEvent();

		// Token: 0x0400240F RID: 9231
		public Zone.ZoneEvent lastCreatureExitEvent = new Zone.ZoneEvent();

		// Token: 0x04002410 RID: 9232
		public Zone.ZoneEvent itemEnterEvent = new Zone.ZoneEvent();

		// Token: 0x04002411 RID: 9233
		public Zone.ZoneEvent itemExitEvent = new Zone.ZoneEvent();

		// Token: 0x04002412 RID: 9234
		public Zone.ZoneEvent firstItemEnterEvent = new Zone.ZoneEvent();

		// Token: 0x04002413 RID: 9235
		public Zone.ZoneEvent lastItemExitEvent = new Zone.ZoneEvent();

		// Token: 0x04002414 RID: 9236
		public Zone.ZoneEvent firstEntityEnterEvent = new Zone.ZoneEvent();

		// Token: 0x04002415 RID: 9237
		public Zone.ZoneEvent lastEntityExitEvent = new Zone.ZoneEvent();

		// Token: 0x04002416 RID: 9238
		protected Collider mainCollider;

		// Token: 0x04002418 RID: 9240
		private int playerMask;

		// Token: 0x04002419 RID: 9241
		private int golemMask;

		// Token: 0x0400241A RID: 9242
		private int creatureMask;

		// Token: 0x0400241B RID: 9243
		private int itemMask;

		// Token: 0x0400241C RID: 9244
		protected Vector3 orgColliderSize;

		// Token: 0x0400241D RID: 9245
		protected float orgColliderRadius;

		// Token: 0x0400241E RID: 9246
		private EffectData effectData;

		// Token: 0x0400241F RID: 9247
		public int collidersInside;

		// Token: 0x04002421 RID: 9249
		protected int playerCollidersEntered;

		// Token: 0x04002424 RID: 9252
		public HashSet<Rigidbody> golemPartsInZone = new HashSet<Rigidbody>();

		// Token: 0x04002425 RID: 9253
		public Dictionary<PhysicBody, int> physicBodiesInZone = new Dictionary<PhysicBody, int>();

		// Token: 0x04002426 RID: 9254
		private Dictionary<PhysicBody, Zone.PhysicBodyTypes> physicBodyTypes = new Dictionary<PhysicBody, Zone.PhysicBodyTypes>();

		// Token: 0x04002427 RID: 9255
		protected List<PhysicBody> missingBodies = new List<PhysicBody>();

		// Token: 0x020009EC RID: 2540
		[Serializable]
		public class ZoneEvent : UnityEvent<UnityEngine.Object>
		{
		}

		// Token: 0x020009ED RID: 2541
		public enum CreatureForceMode
		{
			// Token: 0x04004669 RID: 18025
			NoForce,
			// Token: 0x0400466A RID: 18026
			ForceRoot,
			// Token: 0x0400466B RID: 18027
			ForceParts
		}

		// Token: 0x020009EE RID: 2542
		// (Invoke) Token: 0x060044F1 RID: 17649
		public delegate void GlobalZoneEvent(Zone zone, UnityEngine.Object obj, Zone.ZoneEventType objectType, bool enter);

		// Token: 0x020009EF RID: 2543
		public enum ZoneEventType
		{
			// Token: 0x0400466D RID: 18029
			Player,
			// Token: 0x0400466E RID: 18030
			Creature,
			// Token: 0x0400466F RID: 18031
			Item,
			// Token: 0x04004670 RID: 18032
			Golem,
			// Token: 0x04004671 RID: 18033
			Other
		}

		// Token: 0x020009F0 RID: 2544
		protected enum PhysicBodyTypes
		{
			// Token: 0x04004673 RID: 18035
			PlayerRoot,
			// Token: 0x04004674 RID: 18036
			PlayerPart,
			// Token: 0x04004675 RID: 18037
			CreatureRoot,
			// Token: 0x04004676 RID: 18038
			CreaturePart,
			// Token: 0x04004677 RID: 18039
			Golem,
			// Token: 0x04004678 RID: 18040
			Other
		}
	}
}
