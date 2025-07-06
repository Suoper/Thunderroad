using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200035D RID: 861
	public abstract class ThunderEntity : ThunderBehaviour
	{
		// Token: 0x1700026D RID: 621
		// (get) Token: 0x0600283F RID: 10303 RVA: 0x00113D90 File Offset: 0x00111F90
		public bool IsBurning
		{
			get
			{
				Burning statusOfType = this.GetStatusOfType<Burning>();
				return statusOfType != null && statusOfType.isIgnited;
			}
		}

		// Token: 0x06002840 RID: 10304 RVA: 0x00113DA3 File Offset: 0x00111FA3
		public virtual void Load(EntityData data)
		{
			if (this.entityModules == null)
			{
				this.entityModules = new List<EntityModule>();
			}
			if (this.variables == null)
			{
				this.variables = new Dictionary<Type, AbstractEntityVariable>();
			}
			if (data != null)
			{
				data.Load(this);
			}
		}

		// Token: 0x06002841 RID: 10305 RVA: 0x00113DD8 File Offset: 0x00111FD8
		public T GetVariable<T>(string name)
		{
			AbstractEntityVariable dict;
			if (this.variables.TryGetValue(typeof(T), out dict))
			{
				EntityVariable<T> variable = dict as EntityVariable<T>;
				if (variable != null)
				{
					return variable.Get(name);
				}
			}
			return default(T);
		}

		// Token: 0x06002842 RID: 10306 RVA: 0x00113E1C File Offset: 0x0011201C
		public bool TryGetVariable<T>(string name, out T value)
		{
			value = default(T);
			AbstractEntityVariable dict;
			if (this.variables.TryGetValue(typeof(T), out dict))
			{
				EntityVariable<T> variable = dict as EntityVariable<T>;
				if (variable != null)
				{
					return variable.TryGetValue(name, out value);
				}
			}
			return false;
		}

		// Token: 0x06002843 RID: 10307 RVA: 0x00113E60 File Offset: 0x00112060
		public T SetVariable<T>(string name, Func<T, T> func)
		{
			AbstractEntityVariable dict;
			if (!this.variables.TryGetValue(typeof(T), out dict))
			{
				dict = new EntityVariable<T>();
				this.variables[typeof(T)] = dict;
			}
			EntityVariable<T> variable = this.variables[typeof(T)] as EntityVariable<T>;
			if (variable != null)
			{
				return variable.Set(name, func);
			}
			return default(T);
		}

		// Token: 0x06002844 RID: 10308 RVA: 0x00113ED4 File Offset: 0x001120D4
		public T SetVariable<T>(string name, T value)
		{
			AbstractEntityVariable dict;
			if (!this.variables.TryGetValue(typeof(T), out dict))
			{
				dict = new EntityVariable<T>();
				this.variables[typeof(T)] = dict;
			}
			EntityVariable<T> entityVariable = this.variables[typeof(T)] as EntityVariable<T>;
			if (entityVariable != null)
			{
				entityVariable.Set(name, value);
			}
			return value;
		}

		// Token: 0x06002845 RID: 10309 RVA: 0x00113F40 File Offset: 0x00112140
		public void ClearVariable<T>(string name)
		{
			AbstractEntityVariable dict;
			if (!this.variables.TryGetValue(typeof(T), out dict))
			{
				dict = new EntityVariable<T>();
				this.variables[typeof(T)] = dict;
			}
			EntityVariable<T> entityVariable = this.variables[typeof(T)] as EntityVariable<T>;
			if (entityVariable == null)
			{
				return;
			}
			entityVariable.Clear(name);
		}

		// Token: 0x06002846 RID: 10310 RVA: 0x00113FA8 File Offset: 0x001121A8
		public void ClearVariables()
		{
			this.variables.Clear();
		}

		// Token: 0x1700026E RID: 622
		// (get) Token: 0x06002847 RID: 10311 RVA: 0x00113FB8 File Offset: 0x001121B8
		public Vector3 Velocity
		{
			get
			{
				PhysicBody rootPhysicBody = this.RootPhysicBody;
				if (rootPhysicBody == null)
				{
					return default(Vector3);
				}
				return rootPhysicBody.velocity;
			}
		}

		// Token: 0x06002848 RID: 10312 RVA: 0x00113FE0 File Offset: 0x001121E0
		public T GetModule<T>() where T : EntityModule
		{
			for (int i = 0; i < this.entityModules.Count; i++)
			{
				T entityModule = this.entityModules[i] as T;
				if (entityModule != null)
				{
					return entityModule;
				}
			}
			return default(T);
		}

		// Token: 0x06002849 RID: 10313 RVA: 0x00114030 File Offset: 0x00112230
		public bool TryGetModule<T>(out T module) where T : EntityModule
		{
			module = default(T);
			if (this.entityModules == null)
			{
				return false;
			}
			for (int i = 0; i < this.entityModules.Count; i++)
			{
				T entityModule = this.entityModules[i] as T;
				if (entityModule != null)
				{
					module = entityModule;
					return true;
				}
			}
			return false;
		}

		// Token: 0x1700026F RID: 623
		// (get) Token: 0x0600284A RID: 10314 RVA: 0x0011408D File Offset: 0x0011228D
		public List<IStatus> Statuses
		{
			get
			{
				StatusManager instance = StatusManager.Instance;
				if (instance == null)
				{
					return null;
				}
				return instance.GetStatuses(this);
			}
		}

		// Token: 0x0600284B RID: 10315 RVA: 0x001140A0 File Offset: 0x001122A0
		public void TransferStatuses(ThunderEntity other)
		{
			for (int i = 0; i < this.Statuses.Count; i++)
			{
				this.Statuses[i].Transfer(other);
			}
		}

		// Token: 0x17000270 RID: 624
		// (get) Token: 0x0600284C RID: 10316 RVA: 0x001140D8 File Offset: 0x001122D8
		public virtual Vector3 Center
		{
			get
			{
				Vector3 result;
				if (this == null)
				{
					result = Vector3.zero;
				}
				else
				{
					Item item = this as Item;
					Vector3 vector;
					if (item == null)
					{
						Creature creature = this as Creature;
						if (creature == null)
						{
							Golem golem = this as Golem;
							if (golem == null)
							{
								throw new SwitchExpressionException(this);
							}
							vector = golem.RootTransform.position;
						}
						else
						{
							vector = (creature.ragdoll.targetPart ? creature.ragdoll.targetPart : creature.ragdoll.rootPart).transform.position;
						}
					}
					else
					{
						vector = item.transform.TransformPoint(item.GetLocalCenter());
					}
					result = vector;
				}
				return result;
			}
		}

		// Token: 0x17000271 RID: 625
		// (get) Token: 0x0600284D RID: 10317 RVA: 0x00114188 File Offset: 0x00112388
		public virtual Transform RootTransform
		{
			get
			{
				Item item = this as Item;
				Transform transform;
				if (item == null)
				{
					Creature creature = this as Creature;
					if (creature == null)
					{
						Golem golem = this as Golem;
						if (golem == null)
						{
							throw new SwitchExpressionException(this);
						}
						transform = golem.transform;
					}
					else
					{
						transform = creature.ragdoll.rootPart.transform;
					}
				}
				else
				{
					transform = item.transform;
				}
				return transform;
			}
		}

		// Token: 0x17000272 RID: 626
		// (get) Token: 0x0600284E RID: 10318 RVA: 0x001141EC File Offset: 0x001123EC
		public virtual PhysicBody RootPhysicBody
		{
			get
			{
				Item item = this as Item;
				PhysicBody result;
				if (item == null)
				{
					Creature creature = this as Creature;
					if (creature == null)
					{
						if (!(this is Golem))
						{
							throw new SwitchExpressionException(this);
						}
						result = null;
					}
					else
					{
						result = (creature.ragdoll.IsPhysicsEnabled(false) ? creature.ragdoll.rootPart.physicBody : creature.locomotion.physicBody);
					}
				}
				else
				{
					result = item.physicBody;
				}
				return result;
			}
		}

		// Token: 0x0600284F RID: 10319 RVA: 0x0011425C File Offset: 0x0011245C
		protected override void ManagedOnEnable()
		{
			base.ManagedOnEnable();
			ThunderEntity.allEntities.Add(this);
		}

		// Token: 0x06002850 RID: 10320 RVA: 0x0011426F File Offset: 0x0011246F
		protected override void ManagedOnDisable()
		{
			base.ManagedOnDisable();
			ThunderEntity.allEntities.Remove(this);
		}

		// Token: 0x06002851 RID: 10321 RVA: 0x00114283 File Offset: 0x00112483
		public virtual void AddForce(Vector3 force, ForceMode forceMode, CollisionHandler hitHandler = null)
		{
		}

		// Token: 0x06002852 RID: 10322 RVA: 0x00114285 File Offset: 0x00112485
		public virtual void AddRadialForce(float force, Vector3 origin, float upwardsModifier, ForceMode forceMode, CollisionHandler hitHandler = null)
		{
		}

		// Token: 0x06002853 RID: 10323 RVA: 0x00114287 File Offset: 0x00112487
		public virtual void AddExplosionForce(float force, Vector3 origin, float radius, float upwardsModifier, ForceMode forceMode, CollisionHandler hitHandler = null)
		{
		}

		// Token: 0x06002854 RID: 10324 RVA: 0x00114289 File Offset: 0x00112489
		private void Awake()
		{
			this.weakpoints = new List<Transform>();
		}

		// Token: 0x06002855 RID: 10325 RVA: 0x00114296 File Offset: 0x00112496
		protected virtual void Start()
		{
		}

		// Token: 0x06002856 RID: 10326 RVA: 0x00114298 File Offset: 0x00112498
		public virtual void RefreshWeakPoints()
		{
		}

		// Token: 0x06002857 RID: 10327 RVA: 0x0011429C File Offset: 0x0011249C
		public virtual void SetPhysicModifier(object handler, float? gravity = null, float mass = 1f, float drag = -1f)
		{
			Item item = this as Item;
			if (item != null)
			{
				item.SetPhysicModifier(handler, gravity, mass, drag, drag, -1f, null);
				return;
			}
			Creature creature = this as Creature;
			if (creature == null)
			{
				return;
			}
			creature.ragdoll.SetPhysicModifier(handler, gravity, mass, drag, drag, null);
			creature.locomotion.SetPhysicModifier(handler, gravity, mass, drag, -1);
		}

		// Token: 0x06002858 RID: 10328 RVA: 0x001142F8 File Offset: 0x001124F8
		public virtual void RemovePhysicModifier(object handler)
		{
			Item item = this as Item;
			if (item != null)
			{
				item.RemovePhysicModifier(handler);
				return;
			}
			Creature creature = this as Creature;
			if (creature == null)
			{
				return;
			}
			creature.ragdoll.RemovePhysicModifier(handler);
			creature.locomotion.RemovePhysicModifier(handler);
		}

		/// <summary>
		/// Inflict a status effect, providing the StatusData ID
		/// </summary>
		// Token: 0x06002859 RID: 10329 RVA: 0x0011433C File Offset: 0x0011253C
		public virtual void Inflict(string id, object handler, float duration = float.PositiveInfinity, object parameter = null, bool playEffect = true)
		{
			StatusData data = Catalog.GetData<StatusData>(id, true);
			if (data == null || (this.statusImmune && ((!(this is Creature) && !(this is Golem)) || !data.forceAllowOnCreatures)))
			{
				return;
			}
			StatusManager.Instance.Inflict(this, data, handler, duration, parameter, playEffect);
		}

		/// <summary>
		/// Inflict a status effect, providing the StatusData ID
		/// </summary>
		// Token: 0x0600285A RID: 10330 RVA: 0x0011438C File Offset: 0x0011258C
		public virtual void Inflict(StatusData data, object handler, float duration = float.PositiveInfinity, object parameter = null, bool playEffect = true)
		{
			if (data == null || (this.statusImmune && ((!(this is Creature) && !(this is Golem)) || !data.forceAllowOnCreatures)))
			{
				return;
			}
			StatusManager.Instance.Inflict(this, data, handler, duration, parameter, playEffect);
		}

		// Token: 0x0600285B RID: 10331 RVA: 0x001143D4 File Offset: 0x001125D4
		public virtual bool Remove(string id, object handler)
		{
			StatusData data = Catalog.GetData<StatusData>(id, true);
			return data != null && StatusManager.Instance.Remove(this, data, handler);
		}

		/// <summary>
		/// Remove a single handler for a status effect of type S on this entity.
		/// </summary>
		// Token: 0x0600285C RID: 10332 RVA: 0x001143FB File Offset: 0x001125FB
		public virtual bool Remove(StatusData data, object handler)
		{
			return StatusManager.Instance.Remove(this, data, handler);
		}

		// Token: 0x0600285D RID: 10333 RVA: 0x0011440A File Offset: 0x0011260A
		public virtual bool HasAnyStatus()
		{
			return StatusManager.Instance.GetStatuses(this).Count > 0;
		}

		/// <summary>
		/// Does this entity have a particular status?
		/// </summary>
		// Token: 0x0600285E RID: 10334 RVA: 0x0011441F File Offset: 0x0011261F
		public virtual bool HasStatus(string id)
		{
			return StatusManager.Instance.Has(this, id);
		}

		// Token: 0x0600285F RID: 10335 RVA: 0x0011442D File Offset: 0x0011262D
		public virtual bool HasStatus(StatusData data)
		{
			return StatusManager.Instance.Has(this, data);
		}

		/// <summary>
		/// Check and return a particular status on this entity
		/// </summary>
		// Token: 0x06002860 RID: 10336 RVA: 0x0011443C File Offset: 0x0011263C
		public virtual bool TryGetStatus<T>(StatusData data, out T status) where T : class, IStatus
		{
			status = default(T);
			IStatus stat;
			if (StatusManager.Instance.TryGetStatus(this, data, out stat))
			{
				T value = stat as T;
				if (value != null)
				{
					status = value;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06002861 RID: 10337 RVA: 0x00114480 File Offset: 0x00112680
		public virtual T GetStatusOfType<T>() where T : class, IStatus
		{
			foreach (IStatus status in this.Statuses)
			{
				if (status is T)
				{
					return status as T;
				}
			}
			return default(T);
		}

		// Token: 0x06002862 RID: 10338 RVA: 0x001144F0 File Offset: 0x001126F0
		public virtual bool TryGetStatusOfType<T>(out T status) where T : class, IStatus
		{
			foreach (IStatus status2 in this.Statuses)
			{
				T eachAsT = status2 as T;
				if (eachAsT != null)
				{
					status = eachAsT;
					return true;
				}
			}
			status = default(T);
			return false;
		}

		/// <summary>
		/// Remove all status effects of type S on this entity.
		/// </summary>
		// Token: 0x06002863 RID: 10339 RVA: 0x00114564 File Offset: 0x00112764
		public virtual void Clear(string id)
		{
			StatusManager.Instance.Clear(this, id);
		}

		// Token: 0x06002864 RID: 10340 RVA: 0x00114572 File Offset: 0x00112772
		public virtual void ClearByType<T>() where T : Status
		{
			StatusManager.Instance.ClearByType<T>(this);
		}

		// Token: 0x06002865 RID: 10341 RVA: 0x0011457F File Offset: 0x0011277F
		public virtual void Clear(StatusData data)
		{
			StatusManager.Instance.Clear(this, data);
		}

		// Token: 0x06002866 RID: 10342 RVA: 0x0011458D File Offset: 0x0011278D
		public virtual void ClearByHandler(object handler)
		{
			StatusManager.Instance.ClearByHandler(this, handler);
		}

		/// <summary>
		/// Clear all status effects on this entity.
		/// </summary>
		// Token: 0x06002867 RID: 10343 RVA: 0x0011459B File Offset: 0x0011279B
		public virtual void ClearAllStatus()
		{
			StatusManager.Instance.ClearAll(this);
		}

		// Token: 0x06002868 RID: 10344 RVA: 0x001145A8 File Offset: 0x001127A8
		public virtual void Despawn()
		{
			this.ClearAllStatus();
			this.variables = new Dictionary<Type, AbstractEntityVariable>();
			ThunderEntity.allEntities.Remove(this);
		}

		// Token: 0x17000273 RID: 627
		// (get) Token: 0x06002869 RID: 10345 RVA: 0x001145C8 File Offset: 0x001127C8
		public Bounds Bounds
		{
			get
			{
				Creature creature = this as Creature;
				Bounds result;
				if (creature == null)
				{
					Item item = this as Item;
					if (item == null)
					{
						Golem golem = this as Golem;
						if (golem == null)
						{
							throw new ArgumentOutOfRangeException(string.Format("Unknown entity type {0}", base.GetType()));
						}
						result = new Bounds(golem.Center, new Vector3(3f, 4f, 3f));
					}
					else
					{
						result = item.GetWorldBounds();
					}
				}
				else
				{
					result = (creature.ragdoll.IsPhysicsEnabled(false) ? creature.ragdoll.rootPart.GetWorldBounds() : creature.currentLocomotion.capsuleCollider.bounds);
				}
				return result;
			}
		}

		// Token: 0x0600286A RID: 10346 RVA: 0x00114670 File Offset: 0x00112870
		public Vector3 ClosestPoint(Vector3 point)
		{
			Creature creature = this as Creature;
			Vector3 result;
			if (creature == null)
			{
				Item item = this as Item;
				if (item == null)
				{
					Golem golem = this as Golem;
					if (golem == null)
					{
						throw new ArgumentOutOfRangeException(string.Format("Unknown entity type {0}", base.GetType()));
					}
					result = golem.Bounds.ClosestPoint(point);
				}
				else
				{
					result = item.GetWorldBounds().ClosestPoint(point);
				}
			}
			else
			{
				result = creature.ragdoll.rootPart.GetWorldBounds().ClosestPoint(point);
			}
			return result;
		}

		// Token: 0x0600286B RID: 10347 RVA: 0x00114700 File Offset: 0x00112900
		public T GetSpellHandler<T>() where T : SpellHandlerData
		{
			return default(T);
		}

		// Token: 0x0600286C RID: 10348 RVA: 0x00114718 File Offset: 0x00112918
		[return: TupleElementNames(new string[]
		{
			"entity",
			"closestPoint"
		})]
		public static ICollection<ValueTuple<ThunderEntity, Vector3>> InRadiusClosestPoint(Vector3 position, float radius, Func<ThunderEntity, bool> filter = null, [TupleElementNames(new string[]
		{
			"entity",
			"closestPoint"
		})] ICollection<ValueTuple<ThunderEntity, Vector3>> allocList = null)
		{
			if (allocList == null)
			{
				allocList = new List<ValueTuple<ThunderEntity, Vector3>>();
			}
			float sqrRadius = radius * radius;
			for (int i = 0; i < ThunderEntity.allEntities.Count; i++)
			{
				ThunderEntity entity = ThunderEntity.allEntities[i];
				if (filter == null || filter(entity))
				{
					Vector3 closestPoint = entity.ClosestPoint(position);
					if ((position - closestPoint).sqrMagnitude <= sqrRadius)
					{
						allocList.Add(new ValueTuple<ThunderEntity, Vector3>(entity, closestPoint));
					}
				}
			}
			return allocList;
		}

		// Token: 0x0600286D RID: 10349 RVA: 0x0011478C File Offset: 0x0011298C
		public static List<ThunderEntity> InRadiusNaive(Vector3 position, float radius, Func<ThunderEntity, bool> filter = null, List<ThunderEntity> allocList = null)
		{
			if (allocList == null)
			{
				allocList = new List<ThunderEntity>();
			}
			float sqrRadius = radius * radius;
			for (int i = 0; i < ThunderEntity.allEntities.Count; i++)
			{
				ThunderEntity entity = ThunderEntity.allEntities[i];
				if ((filter == null || filter(entity)) && (position - entity.Center).sqrMagnitude <= sqrRadius)
				{
					allocList.Add(entity);
				}
			}
			return allocList;
		}

		// Token: 0x0600286E RID: 10350 RVA: 0x001147F8 File Offset: 0x001129F8
		public static List<ThunderEntity> InRadius(Vector3 position, float radius, Func<ThunderEntity, bool> filter = null, List<ThunderEntity> allocList = null)
		{
			if (allocList == null)
			{
				allocList = new List<ThunderEntity>();
			}
			float sqrRadius = radius * radius;
			for (int i = 0; i < ThunderEntity.allEntities.Count; i++)
			{
				ThunderEntity entity = ThunderEntity.allEntities[i];
				if (filter == null || filter(entity))
				{
					Vector3 closestPoint = entity.ClosestPoint(position);
					if ((position - closestPoint).sqrMagnitude <= sqrRadius)
					{
						allocList.Add(entity);
					}
				}
			}
			return allocList;
		}

		// Token: 0x0600286F RID: 10351 RVA: 0x00114866 File Offset: 0x00112A66
		public static ThunderEntity AimAssist(Vector3 position, Vector3 direction, float maxDistance, float maxAngle, Func<ThunderEntity, bool> filter = null, ThunderEntity ignoredEntity = null)
		{
			return ThunderEntity.AimAssist(new Ray(position, direction), maxDistance, maxAngle, filter, ignoredEntity);
		}

		// Token: 0x06002870 RID: 10352 RVA: 0x0011487C File Offset: 0x00112A7C
		public static ThunderEntity AimAssist(Ray ray, float maxDistance, float maxAngle, Func<ThunderEntity, bool> filter = null, ThunderEntity ignoredEntity = null)
		{
			float sqrMaxDistance = maxDistance * maxDistance;
			float largestRightSqrDistance = float.PositiveInfinity;
			ThunderEntity outputEntity = null;
			for (int i = 0; i < ThunderEntity.allEntities.Count; i++)
			{
				ThunderEntity entity = ThunderEntity.allEntities[i];
				if ((filter == null || filter(entity)) && !(entity == ignoredEntity))
				{
					Item item = entity as Item;
					if (item != null)
					{
						PhysicBody physicBody = item.physicBody;
						if (physicBody != null && physicBody.isKinematic)
						{
							goto IL_D1;
						}
					}
					Vector3 toEntity = entity.Center - ray.origin;
					if (toEntity.sqrMagnitude <= sqrMaxDistance && Vector3.Angle(toEntity, ray.direction) <= maxAngle)
					{
						float distance = toEntity.magnitude;
						float rightSqrDistance = (ray.GetPoint(distance) - entity.ClosestPoint(ray.GetPoint(distance))).sqrMagnitude;
						if (rightSqrDistance < largestRightSqrDistance)
						{
							largestRightSqrDistance = rightSqrDistance;
							outputEntity = entity;
						}
					}
				}
				IL_D1:;
			}
			return outputEntity;
		}

		// Token: 0x04002718 RID: 10008
		public static List<ThunderEntity> allEntities = new List<ThunderEntity>();

		// Token: 0x04002719 RID: 10009
		public List<EntityModule> entityModules;

		// Token: 0x0400271A RID: 10010
		public Dictionary<Type, AbstractEntityVariable> variables;

		// Token: 0x0400271B RID: 10011
		protected bool statusImmune;

		// Token: 0x0400271C RID: 10012
		[NonSerialized]
		public List<Transform> weakpoints;

		// Token: 0x0400271D RID: 10013
		[NonSerialized]
		public WaterHandler waterHandler;
	}
}
