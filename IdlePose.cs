using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000233 RID: 563
	[Serializable]
	public abstract class IdlePose : StanceNode
	{
		// Token: 0x060017C4 RID: 6084
		public abstract bool CheckValid(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0);

		// Token: 0x060017C5 RID: 6085 RVA: 0x0009EAD9 File Offset: 0x0009CCD9
		public IdlePose GetRandomNext()
		{
			List<IdlePose> list = this.nextIdles;
			if (((list != null) ? list.Count : 0) <= 0)
			{
				return this;
			}
			return this.nextIdles[UnityEngine.Random.Range(0, this.nextIdles.Count)];
		}

		// Token: 0x060017C6 RID: 6086 RVA: 0x0009EB10 File Offset: 0x0009CD10
		protected override void PopulateLists()
		{
			this.nextIdles = new List<IdlePose>();
			foreach (StanceNode stanceNode in this.stanceData.AllStanceNodes())
			{
				IdlePose idlePose = stanceNode as IdlePose;
				if (idlePose != null && idlePose != this)
				{
					this.nextIdles.Add(idlePose);
				}
			}
		}

		// Token: 0x04001714 RID: 5908
		public List<IdlePose> nextIdles;
	}
}
