using System;

namespace ThunderRoad
{
	// Token: 0x02000367 RID: 871
	public class RingBuffer<T>
	{
		// Token: 0x0600291A RID: 10522 RVA: 0x0011770B File Offset: 0x0011590B
		public RingBuffer(int length)
		{
			this.buffer = new T[length];
		}

		// Token: 0x0600291B RID: 10523 RVA: 0x00117720 File Offset: 0x00115920
		public void Push(T value)
		{
			this.buffer[this.index] = value;
			this.index++;
			if (this.count < this.length)
			{
				this.count++;
			}
			this.index %= this.length;
		}

		// Token: 0x0600291C RID: 10524 RVA: 0x0011777C File Offset: 0x0011597C
		public T Pop()
		{
			if (this.count == 0)
			{
				return default(T);
			}
			T result = this[this.count - 1];
			this.index--;
			this.count--;
			if (this.index < 0)
			{
				this.index = this.length - this.index;
			}
			return result;
		}

		// Token: 0x0600291D RID: 10525 RVA: 0x001177E1 File Offset: 0x001159E1
		public void Clear()
		{
			this.count = 0;
			this.index = 0;
		}

		// Token: 0x17000275 RID: 629
		public T this[int i]
		{
			get
			{
				return this.buffer[(this.count > this.index) ? (this.length + (this.index - this.count)) : (this.index - this.count)];
			}
		}

		// Token: 0x0400272C RID: 10028
		private T[] buffer;

		// Token: 0x0400272D RID: 10029
		private int length;

		// Token: 0x0400272E RID: 10030
		private int index;

		// Token: 0x0400272F RID: 10031
		public int count;
	}
}
