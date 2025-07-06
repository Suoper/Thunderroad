using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000314 RID: 788
	[Serializable]
	public class NibbleArray
	{
		// Token: 0x0600257D RID: 9597 RVA: 0x00101184 File Offset: 0x000FF384
		public NibbleArray(int length)
		{
			if (length < 0)
			{
				throw new ArgumentOutOfRangeException("length", "Length cannot be negative.");
			}
			if (length % 2 != 0)
			{
				throw new ArgumentOutOfRangeException("length", "Length cannot be an odd number");
			}
			this.data = new byte[(length + 1) / 2];
		}

		// Token: 0x17000245 RID: 581
		// (get) Token: 0x0600257E RID: 9598 RVA: 0x001011D0 File Offset: 0x000FF3D0
		public int Length
		{
			get
			{
				return this.data.Length * 2;
			}
		}

		// Token: 0x17000246 RID: 582
		public byte this[int index]
		{
			get
			{
				if (index < 0 || index >= this.Length)
				{
					throw new IndexOutOfRangeException("index");
				}
				int byteIndex = index / 2;
				NibbleArray.NibblePosition position = (index % 2 == 0) ? NibbleArray.NibblePosition.Lower : NibbleArray.NibblePosition.Upper;
				if (position == NibbleArray.NibblePosition.Lower)
				{
					return this.GetLowerNibble(byteIndex);
				}
				if (position != NibbleArray.NibblePosition.Upper)
				{
					throw new ArgumentOutOfRangeException("index", string.Format("Invalid nibble position for index {0}.", index));
				}
				return this.GetUpperNibble(byteIndex);
			}
			set
			{
				if (index < 0 || index >= this.Length)
				{
					throw new IndexOutOfRangeException("index");
				}
				if (value > 15)
				{
					throw new ArgumentOutOfRangeException("value", "Nibble value cannot exceed 15.");
				}
				int byteIndex = index / 2;
				NibbleArray.NibblePosition position = (index % 2 == 0) ? NibbleArray.NibblePosition.Lower : NibbleArray.NibblePosition.Upper;
				if (position == NibbleArray.NibblePosition.Lower)
				{
					this.SetLowerNibble(byteIndex, value);
					return;
				}
				if (position != NibbleArray.NibblePosition.Upper)
				{
					throw new ArgumentOutOfRangeException("index", string.Format("Invalid nibble position for index {0}.", index));
				}
				this.SetUpperNibble(byteIndex, value);
			}
		}

		// Token: 0x06002581 RID: 9601 RVA: 0x001012C1 File Offset: 0x000FF4C1
		private byte GetLowerNibble(int index)
		{
			return this.data[index] & 15;
		}

		// Token: 0x06002582 RID: 9602 RVA: 0x001012CF File Offset: 0x000FF4CF
		private byte GetUpperNibble(int index)
		{
			return (byte)((this.data[index] & 240) >> 4);
		}

		// Token: 0x06002583 RID: 9603 RVA: 0x001012E2 File Offset: 0x000FF4E2
		private byte SetNibbles(byte lowerNibble, byte upperNibble)
		{
			return (byte)((int)(lowerNibble & 15) | (int)upperNibble << 4);
		}

		// Token: 0x06002584 RID: 9604 RVA: 0x001012ED File Offset: 0x000FF4ED
		private void SetLowerNibble(int index, byte value)
		{
			if (value > 15)
			{
				throw new ArgumentOutOfRangeException("value", "Lower nibble value cannot exceed 15.");
			}
			this.data[index] = this.SetNibbles(value, this.GetUpperNibble(index));
		}

		// Token: 0x06002585 RID: 9605 RVA: 0x0010131A File Offset: 0x000FF51A
		private void SetUpperNibble(int index, byte value)
		{
			if (value > 15)
			{
				throw new ArgumentOutOfRangeException("value", "Upper nibble value cannot exceed 15.");
			}
			this.data[index] = this.SetNibbles(this.GetLowerNibble(index), value);
		}

		// Token: 0x040024FF RID: 9471
		[SerializeField]
		private byte[] data;

		// Token: 0x02000A11 RID: 2577
		private enum NibblePosition
		{
			// Token: 0x040046F2 RID: 18162
			Lower,
			// Token: 0x040046F3 RID: 18163
			Upper
		}
	}
}
