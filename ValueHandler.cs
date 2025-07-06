using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000362 RID: 866
	public abstract class ValueHandler<T>
	{
		// Token: 0x14000135 RID: 309
		// (add) Token: 0x06002900 RID: 10496 RVA: 0x001171FC File Offset: 0x001153FC
		// (remove) Token: 0x06002901 RID: 10497 RVA: 0x00117234 File Offset: 0x00115434
		public event ValueHandler<T>.ChangeEvent OnChangeEvent;

		// Token: 0x17000274 RID: 628
		// (get) Token: 0x06002902 RID: 10498 RVA: 0x00117269 File Offset: 0x00115469
		public T Value
		{
			get
			{
				if (!this.initialised)
				{
					this.RefreshInternal();
				}
				return this.value;
			}
		}

		// Token: 0x06002903 RID: 10499 RVA: 0x0011727F File Offset: 0x0011547F
		protected ValueHandler()
		{
			this.value = this.baseValue;
		}

		// Token: 0x06002904 RID: 10500 RVA: 0x0011729E File Offset: 0x0011549E
		protected ValueHandler(T baseValue)
		{
			this.baseValue = baseValue;
			this.value = baseValue;
		}

		// Token: 0x06002905 RID: 10501 RVA: 0x001172C0 File Offset: 0x001154C0
		public bool Add(object handler, T value)
		{
			T existing;
			if (this.handlers.TryGetValue(handler, out existing) && existing.Equals(value))
			{
				return false;
			}
			this.handlers[handler] = value;
			this.RefreshInternal();
			return true;
		}

		// Token: 0x06002906 RID: 10502 RVA: 0x00117308 File Offset: 0x00115508
		public bool Remove(object handler)
		{
			if (!this.handlers.Remove(handler))
			{
				return false;
			}
			this.RefreshInternal();
			return true;
		}

		// Token: 0x06002907 RID: 10503 RVA: 0x00117321 File Offset: 0x00115521
		public bool Clear()
		{
			if (this.handlers.Count == 0)
			{
				return false;
			}
			this.handlers.Clear();
			this.RefreshInternal();
			return true;
		}

		// Token: 0x06002908 RID: 10504 RVA: 0x00117344 File Offset: 0x00115544
		public bool ClearByType<U>()
		{
			List<object> keys = this.handlers.Keys.ToList<object>();
			bool found = false;
			for (int i = 0; i < keys.Count; i++)
			{
				if (keys[i] is U)
				{
					this.handlers.Remove(keys[i]);
					found = true;
				}
			}
			if (found)
			{
				this.RefreshInternal();
			}
			return found;
		}

		// Token: 0x06002909 RID: 10505 RVA: 0x001173A4 File Offset: 0x001155A4
		protected void RefreshInternal()
		{
			this.initialised = true;
			T old = this.Value;
			this.Refresh();
			T t = this.Value;
			if (t != null && !t.Equals(old))
			{
				ValueHandler<T>.ChangeEvent onChangeEvent = this.OnChangeEvent;
				if (onChangeEvent == null)
				{
					return;
				}
				onChangeEvent(old, this.Value);
			}
		}

		// Token: 0x0600290A RID: 10506
		protected abstract void Refresh();

		// Token: 0x0600290B RID: 10507 RVA: 0x0011740C File Offset: 0x0011560C
		public void LogHandlers()
		{
			if (this.handlers.Count == 0)
			{
				Debug.Log(string.Format("{0} has no handlers.", this));
				return;
			}
			List<string> list = new List<string>();
			foreach (KeyValuePair<object, T> keyValuePair in this.handlers)
			{
				object obj;
				T t;
				keyValuePair.Deconstruct(out obj, out t);
				object key = obj;
				T handlerValue = t;
				list.Add(string.Format("{0}: {1}", key, handlerValue));
			}
			Debug.Log(string.Format("Handlers for {0}:\n - {1}", this, string.Join("\n - ", list)));
		}

		// Token: 0x0600290C RID: 10508 RVA: 0x001174C0 File Offset: 0x001156C0
		public static implicit operator T(ValueHandler<T> valueHandler)
		{
			return valueHandler.Value;
		}

		// Token: 0x04002727 RID: 10023
		protected T value;

		// Token: 0x04002728 RID: 10024
		public T baseValue;

		// Token: 0x04002729 RID: 10025
		private bool initialised;

		// Token: 0x0400272B RID: 10027
		public Dictionary<object, T> handlers = new Dictionary<object, T>();

		// Token: 0x02000A5A RID: 2650
		// (Invoke) Token: 0x060045F6 RID: 17910
		public delegate void ChangeEvent(T oldValue, T newValue);
	}
}
