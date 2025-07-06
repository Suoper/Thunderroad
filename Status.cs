using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ThunderRoad.Pools.ThunderRoad.Pools;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200023E RID: 574
	public abstract class Status : IStatus
	{
		// Token: 0x06001832 RID: 6194 RVA: 0x000A0B32 File Offset: 0x0009ED32
		public virtual void Spawn(StatusData data, ThunderEntity entity)
		{
			this.data = data;
			this.entity = entity;
			this.handlers = LazyDictPool<object, ValueTuple<float, object>>.Instance.Get(0);
			this.expiry = 0f;
		}

		// Token: 0x06001833 RID: 6195 RVA: 0x000A0B5E File Offset: 0x0009ED5E
		public virtual void OnCatalogRefresh()
		{
		}

		// Token: 0x06001834 RID: 6196 RVA: 0x000A0B60 File Offset: 0x0009ED60
		public virtual void FirstApply()
		{
			this.startTime = Time.time;
			this.data.InvokeOnFirstApplyEvent(this);
		}

		// Token: 0x06001835 RID: 6197 RVA: 0x000A0B79 File Offset: 0x0009ED79
		public virtual void Apply()
		{
		}

		// Token: 0x06001836 RID: 6198 RVA: 0x000A0B7C File Offset: 0x0009ED7C
		public virtual void PlayEffect()
		{
			if (this.effectInstance != null)
			{
				return;
			}
			StatusData statusData = this.data;
			if (statusData == null || !statusData.SpawnEffect(this.entity, out this.effectInstance))
			{
				return;
			}
			EffectInstance effectInstance = this.effectInstance;
			if (effectInstance != null)
			{
				effectInstance.Play(0, false, false);
			}
			if (this.effectInstance != null)
			{
				Creature creature = this.entity as Creature;
				if (creature != null)
				{
					creature.ragdoll.OnStateChange -= this.OnStateChange;
					creature.ragdoll.OnStateChange += this.OnStateChange;
					return;
				}
			}
		}

		// Token: 0x06001837 RID: 6199 RVA: 0x000A0C10 File Offset: 0x0009EE10
		private void OnStateChange(Ragdoll.State oldState, Ragdoll.State newState, Ragdoll.PhysicStateChange physicsChange, EventTime time)
		{
			try
			{
				Creature creature = this.entity as Creature;
				if (creature != null && physicsChange != Ragdoll.PhysicStateChange.None && newState != Ragdoll.State.Disabled)
				{
					bool isStart = time == EventTime.OnStart;
					if (creature.GetRendererForVFX())
					{
						EffectInstance effectInstance = this.effectInstance;
						if (effectInstance != null)
						{
							effectInstance.SetRenderer(isStart ? null : creature.GetRendererForVFX(), false);
						}
					}
					EffectInstance effectInstance2 = this.effectInstance;
					if (effectInstance2 != null)
					{
						effectInstance2.SetParent(isStart ? null : creature.ragdoll.rootPart.meshBone.transform, false);
					}
				}
			}
			catch (NullReferenceException exception)
			{
				Debug.LogWarning("Warning: Status Effect threw exception during creature state change.");
				Debug.LogException(exception);
			}
		}

		// Token: 0x17000179 RID: 377
		// (get) Token: 0x06001838 RID: 6200 RVA: 0x000A0CB8 File Offset: 0x0009EEB8
		public virtual bool ReapplyOnValueChange
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06001839 RID: 6201 RVA: 0x000A0CBB File Offset: 0x0009EEBB
		public virtual void OnValueChange()
		{
			this.value = this.GetValue();
			if (!this.ReapplyOnValueChange)
			{
				return;
			}
			this.Remove();
			this.Apply();
		}

		// Token: 0x0600183A RID: 6202 RVA: 0x000A0CDE File Offset: 0x0009EEDE
		public virtual void FixedUpdate()
		{
		}

		// Token: 0x0600183B RID: 6203 RVA: 0x000A0CE0 File Offset: 0x0009EEE0
		public virtual void Update()
		{
		}

		// Token: 0x0600183C RID: 6204 RVA: 0x000A0CE2 File Offset: 0x0009EEE2
		public virtual void Remove()
		{
		}

		// Token: 0x0600183D RID: 6205 RVA: 0x000A0CE4 File Offset: 0x0009EEE4
		public virtual void FullRemove()
		{
			if (this.effectInstance != null)
			{
				if (this.effectInstance.isPlaying)
				{
					EffectInstance effect = this.effectInstance;
					this.effectInstance.onEffectFinished += delegate(EffectInstance _)
					{
						effect.Despawn();
					};
					this.effectInstance.End(false, -1f);
				}
				this.effectInstance = null;
			}
			Creature creature = this.entity as Creature;
			if (creature == null)
			{
				return;
			}
			creature.ragdoll.OnStateChange -= this.OnStateChange;
			this.data.InvokeOnFullRemoveEvent(this);
		}

		// Token: 0x0600183E RID: 6206 RVA: 0x000A0D7A File Offset: 0x0009EF7A
		public bool HasHandlers()
		{
			return this.handlers.Count > 0;
		}

		// Token: 0x0600183F RID: 6207 RVA: 0x000A0D8A File Offset: 0x0009EF8A
		public virtual bool RemoveHandler(object handler)
		{
			return this.handlers.Remove(handler);
		}

		// Token: 0x06001840 RID: 6208 RVA: 0x000A0D98 File Offset: 0x0009EF98
		public void ClearHandlers()
		{
			this.handlers.Clear();
		}

		/// <summary>
		/// Combine values from the list of handlers on this status instance.
		/// </summary>
		/// <returns></returns>
		// Token: 0x06001841 RID: 6209 RVA: 0x000A0DA5 File Offset: 0x0009EFA5
		protected virtual object GetValue()
		{
			return null;
		}

		// Token: 0x06001842 RID: 6210 RVA: 0x000A0DA8 File Offset: 0x0009EFA8
		public virtual bool AddHandler(object handler, float duration = float.PositiveInfinity, object parameter = null, bool playEffect = true)
		{
			bool isNew = false;
			ValueTuple<float, object> current;
			bool noChange = this.handlers.TryGetValue(handler, out current) && object.Equals(current.Item2, this.value);
			if (!this.HasHandlers())
			{
				isNew = true;
			}
			if (duration == 0f)
			{
				duration = float.PositiveInfinity;
			}
			switch (this.GetStackType())
			{
			case StackType.Refresh:
			{
				float thisExpiry = Time.time + duration;
				this.handlers[handler] = new ValueTuple<float, object>(thisExpiry, parameter);
				if (float.IsInfinity(this.expiry) || thisExpiry > this.expiry)
				{
					this.expiry = thisExpiry;
				}
				break;
			}
			case StackType.Stack:
				if (this.expiry == 0f)
				{
					this.expiry = Time.time;
				}
				this.expiry += duration;
				this.handlers[handler] = new ValueTuple<float, object>(this.expiry, parameter);
				break;
			case StackType.None:
				if (this.expiry == 0f)
				{
					this.handlers[handler] = new ValueTuple<float, object>(Time.time + duration, parameter);
				}
				else
				{
					this.handlers[handler] = new ValueTuple<float, object>(this.expiry, parameter);
				}
				break;
			case StackType.Infinite:
				this.handlers[handler] = new ValueTuple<float, object>(this.expiry, parameter);
				break;
			}
			if (noChange)
			{
				return false;
			}
			if (isNew)
			{
				this.value = this.GetValue();
				this.FirstApply();
				this.Apply();
				if (playEffect)
				{
					this.PlayEffect();
				}
			}
			else
			{
				this.OnValueChange();
			}
			return true;
		}

		// Token: 0x1700017A RID: 378
		// (get) Token: 0x06001843 RID: 6211 RVA: 0x000A0F29 File Offset: 0x0009F129
		public float Duration
		{
			get
			{
				return this.expiry - Time.time;
			}
		}

		// Token: 0x06001844 RID: 6212 RVA: 0x000A0F38 File Offset: 0x0009F138
		public bool CheckExpired()
		{
			switch (this.GetStackType())
			{
			case StackType.Refresh:
			{
				bool expired = false;
				foreach (KeyValuePair<object, ValueTuple<float, object>> keyValuePair in this.handlers.ToList<KeyValuePair<object, ValueTuple<float, object>>>())
				{
					object obj;
					ValueTuple<float, object> valueTuple;
					keyValuePair.Deconstruct(out obj, out valueTuple);
					ref ValueTuple<float, object> ptr = valueTuple;
					object handler = obj;
					float expiry = ptr.Item1;
					if (Time.time >= expiry)
					{
						this.RemoveHandler(handler);
						expired = true;
					}
				}
				return expired;
			}
			case StackType.Stack:
			case StackType.None:
				if (Time.time >= this.expiry)
				{
					this.ClearHandlers();
					return true;
				}
				break;
			case StackType.Infinite:
				return !this.HasHandlers();
			}
			return false;
		}

		// Token: 0x06001845 RID: 6213 RVA: 0x000A0FFC File Offset: 0x0009F1FC
		public bool Refresh()
		{
			if (this.HasHandlers())
			{
				this.OnValueChange();
				return false;
			}
			return true;
		}

		// Token: 0x06001846 RID: 6214 RVA: 0x000A1011 File Offset: 0x0009F211
		public void Despawn()
		{
			this.Remove();
			this.FullRemove();
			LazyDictPool<object, ValueTuple<float, object>>.Instance.Return(this.handlers);
		}

		/// <summary>
		/// Replay every non-infinite status onto another entity
		/// </summary>
		/// <param name="other">The entity to inflict these statuses upon</param>
		// Token: 0x06001847 RID: 6215 RVA: 0x000A1030 File Offset: 0x0009F230
		public virtual void Transfer(ThunderEntity other)
		{
			foreach (KeyValuePair<object, ValueTuple<float, object>> each in this.handlers)
			{
				object handler = each.Key;
				if (handler != null)
				{
					ValueTuple<float, object> valueTuple = each.Value;
					float thisExpiry = valueTuple.Item1;
					if (thisExpiry != float.PositiveInfinity)
					{
						object parameter = valueTuple.Item2;
						other.Inflict(this.data, handler, thisExpiry - Time.time, parameter, true);
					}
				}
			}
		}

		// Token: 0x06001848 RID: 6216 RVA: 0x000A10C0 File Offset: 0x0009F2C0
		public StackType GetStackType()
		{
			StatusData statusData = this.data;
			if (statusData == null)
			{
				return StackType.Refresh;
			}
			return statusData.stackType;
		}

		// Token: 0x04001743 RID: 5955
		public StatusData data;

		// Token: 0x04001744 RID: 5956
		public ThunderEntity entity;

		// Token: 0x04001745 RID: 5957
		protected EffectInstance effectInstance;

		// Token: 0x04001746 RID: 5958
		[TupleElementNames(new string[]
		{
			"expiry",
			"parameter"
		})]
		protected Dictionary<object, ValueTuple<float, object>> handlers;

		// Token: 0x04001747 RID: 5959
		public object value;

		// Token: 0x04001748 RID: 5960
		public float expiry;

		// Token: 0x04001749 RID: 5961
		public float startTime;
	}
}
