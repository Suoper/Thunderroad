using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Comparers;

namespace ThunderRoad
{
	// Token: 0x0200036C RID: 876
	public static class Yielders
	{
		// Token: 0x1700027A RID: 634
		// (get) Token: 0x0600296B RID: 10603 RVA: 0x00119DA3 File Offset: 0x00117FA3
		public static WaitForEndOfFrame EndOfFrame
		{
			get
			{
				return Yielders._endOfFrame;
			}
		}

		// Token: 0x1700027B RID: 635
		// (get) Token: 0x0600296C RID: 10604 RVA: 0x00119DAA File Offset: 0x00117FAA
		public static WaitForFixedUpdate FixedUpdate
		{
			get
			{
				return Yielders._fixedUpdate;
			}
		}

		// Token: 0x0600296D RID: 10605 RVA: 0x00119DB4 File Offset: 0x00117FB4
		public static WaitForSeconds ForSeconds(float seconds)
		{
			WaitForSeconds wfs;
			if (!Yielders._timeInterval.TryGetValue(seconds, out wfs))
			{
				wfs = new WaitForSeconds(seconds);
				Yielders._timeInterval.Add(seconds, wfs);
			}
			return wfs;
		}

		// Token: 0x0600296E RID: 10606 RVA: 0x00119DE4 File Offset: 0x00117FE4
		public static WaitForSecondsRealtime ForRealSeconds(float seconds)
		{
			return new WaitForSecondsRealtime(seconds);
		}

		// Token: 0x0600296F RID: 10607 RVA: 0x00119DEC File Offset: 0x00117FEC
		public static IEnumerator YieldParallel(this List<IEnumerator> coroutines)
		{
			Yielders.<>c__DisplayClass10_0 CS$<>8__locals1 = new Yielders.<>c__DisplayClass10_0();
			CS$<>8__locals1.running = 0;
			using (List<IEnumerator>.Enumerator enumerator2 = coroutines.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					IEnumerator enumerator = enumerator2.Current;
					GameManager.local.StartCoroutine(CS$<>8__locals1.<YieldParallel>g__RunAndWait|0(enumerator));
				}
				goto IL_8B;
			}
			IL_74:
			yield return null;
			IL_8B:
			if (CS$<>8__locals1.running <= 0)
			{
				yield break;
			}
			goto IL_74;
		}

		// Token: 0x04002768 RID: 10088
		private static Dictionary<float, WaitForSeconds> _timeInterval = new Dictionary<float, WaitForSeconds>(100, new FloatComparer());

		// Token: 0x04002769 RID: 10089
		private static Dictionary<float, WaitForSecondsRealtime> _realTimeInterval = new Dictionary<float, WaitForSecondsRealtime>(100, new FloatComparer());

		// Token: 0x0400276A RID: 10090
		private static WaitForEndOfFrame _endOfFrame = new WaitForEndOfFrame();

		// Token: 0x0400276B RID: 10091
		private static WaitForFixedUpdate _fixedUpdate = new WaitForFixedUpdate();
	}
}
