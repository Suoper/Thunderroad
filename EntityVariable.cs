using System;
using System.Collections.Generic;

namespace ThunderRoad
{
	// Token: 0x0200035C RID: 860
	public class EntityVariable<T> : AbstractEntityVariable
	{
		// Token: 0x06002839 RID: 10297 RVA: 0x00113CA5 File Offset: 0x00111EA5
		public T Set(string name, T value)
		{
			if (this.dictionary == null)
			{
				this.dictionary = new Dictionary<string, T>();
			}
			this.dictionary[name] = value;
			return value;
		}

		// Token: 0x0600283A RID: 10298 RVA: 0x00113CC8 File Offset: 0x00111EC8
		public T Set(string name, Func<T, T> func)
		{
			if (this.dictionary == null)
			{
				this.dictionary = new Dictionary<string, T>();
			}
			T value;
			T newValue = this.TryGetValue(name, out value) ? func(value) : func(default(T));
			this.dictionary[name] = newValue;
			return newValue;
		}

		// Token: 0x0600283B RID: 10299 RVA: 0x00113D1C File Offset: 0x00111F1C
		public T Get(string name)
		{
			if (this.dictionary == null)
			{
				this.dictionary = new Dictionary<string, T>();
			}
			T value;
			if (!this.TryGetValue(name, out value))
			{
				return default(T);
			}
			return value;
		}

		// Token: 0x0600283C RID: 10300 RVA: 0x00113D52 File Offset: 0x00111F52
		public bool TryGetValue(string name, out T value)
		{
			if (this.dictionary == null)
			{
				this.dictionary = new Dictionary<string, T>();
			}
			return this.dictionary.TryGetValue(name, out value);
		}

		// Token: 0x0600283D RID: 10301 RVA: 0x00113D74 File Offset: 0x00111F74
		public bool Clear(string name)
		{
			Dictionary<string, T> dictionary = this.dictionary;
			return dictionary != null && dictionary.Remove(name);
		}

		// Token: 0x04002717 RID: 10007
		private Dictionary<string, T> dictionary;
	}
}
