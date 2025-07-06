using System;
using ThunderRoad.Skill.SpellPower;

namespace ThunderRoad
{
	// Token: 0x02000274 RID: 628
	public struct SavedSpells
	{
		// Token: 0x06001C85 RID: 7301 RVA: 0x000BF79C File Offset: 0x000BD99C
		public SavedSpells(SpellCastData left, SpellCastData right, SpellTelekinesis tkLeft, SpellTelekinesis tkRight)
		{
			this.left = left;
			this.right = right;
			this.tkLeft = tkLeft;
			this.tkRight = tkRight;
		}

		// Token: 0x04001B5C RID: 7004
		public SpellCastData left;

		// Token: 0x04001B5D RID: 7005
		public SpellCastData right;

		// Token: 0x04001B5E RID: 7006
		public SpellTelekinesis tkLeft;

		// Token: 0x04001B5F RID: 7007
		public SpellTelekinesis tkRight;
	}
}
