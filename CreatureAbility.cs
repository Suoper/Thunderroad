using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000251 RID: 593
	[Serializable]
	public class CreatureAbility : ScriptableObject
	{
		// Token: 0x1700018C RID: 396
		// (get) Token: 0x060019D9 RID: 6617 RVA: 0x000AC3B2 File Offset: 0x000AA5B2
		// (set) Token: 0x060019DA RID: 6618 RVA: 0x000AC3BA File Offset: 0x000AA5BA
		public Creature creature { get; protected set; }

		// Token: 0x060019DB RID: 6619 RVA: 0x000AC3C3 File Offset: 0x000AA5C3
		public virtual void Setup(Creature creature)
		{
			this.creature = creature;
		}

		// Token: 0x040018A7 RID: 6311
		public bool multiHit = true;

		// Token: 0x040018A8 RID: 6312
		public float contactDamage;

		// Token: 0x040018A9 RID: 6313
		public bool breakBreakables;

		// Token: 0x040018AA RID: 6314
		public float contactForce;

		// Token: 0x040018AB RID: 6315
		public bool forceUngrip;
	}
}
