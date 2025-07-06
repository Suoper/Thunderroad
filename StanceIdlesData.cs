using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200022E RID: 558
	public abstract class StanceIdlesData<T> : StanceData where T : IdlePose
	{
		// Token: 0x0600179B RID: 6043 RVA: 0x0009E178 File Offset: 0x0009C378
		protected override StanceData MakeFilteredClone(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0)
		{
			StanceIdlesData<T> newData = (StanceIdlesData<T>)base.MakeFilteredClone(mainHand, offHand, difficulty);
			newData.baseStance = this.baseStance;
			newData.idles = new List<T>();
			for (int i = 0; i < this.idles.Count; i++)
			{
				if (!base.TooDifficult(difficulty, this.idles[i].difficulty) && this.idles[i].CheckValid(mainHand, offHand, difficulty))
				{
					newData.idles.Add(this.idles[i].Copy<T>());
				}
			}
			return newData;
		}

		// Token: 0x0600179C RID: 6044 RVA: 0x0009E21D File Offset: 0x0009C41D
		public override IEnumerable<StanceNode> AllStanceNodes()
		{
			int num;
			for (int i = 0; i < this.idles.Count; i = num + 1)
			{
				yield return this.idles[i];
				num = i;
			}
			yield break;
		}

		// Token: 0x0600179D RID: 6045 RVA: 0x0009E22D File Offset: 0x0009C42D
		public override IdlePose GetRandomIdle()
		{
			return this.idles[UnityEngine.Random.Range(0, this.idles.Count)];
		}

		// Token: 0x0600179E RID: 6046 RVA: 0x0009E250 File Offset: 0x0009C450
		public override IdlePose GetIdleByID(string id)
		{
			for (int i = 0; i < this.idles.Count; i++)
			{
				if (this.idles[i].id == id)
				{
					return this.idles[i];
				}
			}
			return null;
		}

		// Token: 0x040016FC RID: 5884
		public List<T> idles = new List<T>();
	}
}
