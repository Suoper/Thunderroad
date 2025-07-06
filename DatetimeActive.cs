using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000336 RID: 822
	public class DatetimeActive : MonoBehaviour
	{
		// Token: 0x06002651 RID: 9809 RVA: 0x0010A3C0 File Offset: 0x001085C0
		public bool CheckWithinRange()
		{
			DateTime now = DateTime.Now;
			int currentYear = now.Year;
			DateTime startDate = this.activeStart.ToDateTime(currentYear);
			DateTime endDate = this.activeEnd.ToDateTime(currentYear);
			if (endDate < startDate)
			{
				return now >= startDate || now <= endDate;
			}
			return now >= startDate && now <= endDate;
		}

		// Token: 0x06002652 RID: 9810 RVA: 0x0010A423 File Offset: 0x00108623
		private void Awake()
		{
			if (!this.CheckWithinRange())
			{
				base.gameObject.SetActive(false);
			}
		}

		// Token: 0x04002614 RID: 9748
		[Header("From")]
		public DatetimeActive.SimpleDateTime activeStart = new DatetimeActive.SimpleDateTime(6, 17, 36);

		// Token: 0x04002615 RID: 9749
		[Header("Until")]
		public DatetimeActive.SimpleDateTime activeEnd = new DatetimeActive.SimpleDateTime(10, 28, 36);

		// Token: 0x02000A20 RID: 2592
		[Serializable]
		public class SimpleDateTime
		{
			// Token: 0x170005C8 RID: 1480
			// (get) Token: 0x06004558 RID: 17752 RVA: 0x00195A4C File Offset: 0x00193C4C
			private string hourMinuteString
			{
				get
				{
					ValueTuple<int, int> time = this.GetHourMinute();
					return string.Format("Time: {0}:{1}", time.Item1, (time.Item2 == 0) ? "00" : time.Item2);
				}
			}

			// Token: 0x06004559 RID: 17753 RVA: 0x00195A8F File Offset: 0x00193C8F
			[return: TupleElementNames(new string[]
			{
				"hour",
				"minute"
			})]
			public ValueTuple<int, int> GetHourMinute()
			{
				return new ValueTuple<int, int>(Mathf.FloorToInt((float)(this.time / 2)), 30 * (this.time % 2));
			}

			// Token: 0x0600455A RID: 17754 RVA: 0x00195AB0 File Offset: 0x00193CB0
			public DateTime ToDateTime(int year)
			{
				ValueTuple<int, int> time = this.GetHourMinute();
				return new DateTime(year, this.month, this.day, time.Item1, time.Item2, 0);
			}

			// Token: 0x0600455B RID: 17755 RVA: 0x00195AE3 File Offset: 0x00193CE3
			public SimpleDateTime(int month, int day, int time)
			{
				this.month = month;
				this.day = day;
				this.time = time;
			}

			// Token: 0x0400471F RID: 18207
			public int month;

			// Token: 0x04004720 RID: 18208
			public int day;

			// Token: 0x04004721 RID: 18209
			[Range(0f, 48f)]
			public int time;
		}
	}
}
