using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x02000355 RID: 853
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/RandomEventActivator")]
	public class RandomEventActivator : ThunderBehaviour
	{
		// Token: 0x060027EE RID: 10222 RVA: 0x00111DAF File Offset: 0x0010FFAF
		public void ShuffleEventList()
		{
			this.eventList = this.eventList.Shuffle<UnityEvent>();
		}

		// Token: 0x060027EF RID: 10223 RVA: 0x00111DC2 File Offset: 0x0010FFC2
		public void ActivateRandomFromAll()
		{
			this.ActivateRandom(this.eventList.Count, false);
		}

		// Token: 0x060027F0 RID: 10224 RVA: 0x00111DD6 File Offset: 0x0010FFD6
		public void ActivateRandomFromAllMoveToEnd()
		{
			this.ActivateRandom(this.eventList.Count, true);
		}

		// Token: 0x060027F1 RID: 10225 RVA: 0x00111DEA File Offset: 0x0010FFEA
		public void ActivateRandomInRange(int range)
		{
			this.ActivateRandom(range, false);
		}

		// Token: 0x060027F2 RID: 10226 RVA: 0x00111DF4 File Offset: 0x0010FFF4
		public void ActivateRandomInRangeMoveToEnd(int range)
		{
			this.ActivateRandom(range, true);
		}

		// Token: 0x060027F3 RID: 10227 RVA: 0x00111DFE File Offset: 0x0010FFFE
		public void ActivateRandom(int range, bool moveToEnd)
		{
			this.ActivateEventAtIndex(UnityEngine.Random.Range(0, range), moveToEnd);
		}

		// Token: 0x060027F4 RID: 10228 RVA: 0x00111E0E File Offset: 0x0011000E
		public void ActivateEventAtIndex(int index)
		{
			this.ActivateEventAtIndex(index, false);
		}

		// Token: 0x060027F5 RID: 10229 RVA: 0x00111E18 File Offset: 0x00110018
		public void ActivateEventAtIndexMoveToEnd(int index)
		{
			this.ActivateEventAtIndex(index, true);
		}

		// Token: 0x060027F6 RID: 10230 RVA: 0x00111E24 File Offset: 0x00110024
		public void ActivateEventAtIndex(int index, bool moveToEnd)
		{
			UnityEvent toActivate = this.eventList[index];
			if (toActivate != null)
			{
				toActivate.Invoke();
			}
			if (moveToEnd)
			{
				this.eventList.RemoveAt(index);
				this.eventList.Add(toActivate);
			}
		}

		// Token: 0x040026E4 RID: 9956
		public List<UnityEvent> eventList = new List<UnityEvent>();
	}
}
