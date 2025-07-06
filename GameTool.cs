using System;
using IngameDebugConsole;
using ThunderRoad.Skill.SpellPower;

namespace ThunderRoad
{
	// Token: 0x02000340 RID: 832
	public class GameTool : ThunderBehaviour
	{
		// Token: 0x060026EC RID: 9964 RVA: 0x0010C709 File Offset: 0x0010A909
		private void Start()
		{
			this.slowTime = (Catalog.GetData<SpellPowerData>("SlowTime", true) as SpellPowerSlowTime);
		}

		// Token: 0x060026ED RID: 9965 RVA: 0x0010C721 File Offset: 0x0010A921
		public void ExecuteCommand(string command)
		{
			DebugLogConsole.ExecuteCommand(command);
		}

		// Token: 0x060026EE RID: 9966 RVA: 0x0010C729 File Offset: 0x0010A929
		public void SetSlowMotion(float scale)
		{
			TimeManager.SetSlowMotion(true, scale, this.slowTime.enterCurve, this.slowTime.effectData, true, true);
		}

		// Token: 0x060026EF RID: 9967 RVA: 0x0010C74A File Offset: 0x0010A94A
		public void EndSlowMotion()
		{
			TimeManager.SetSlowMotion(false, this.slowTime.scale, this.slowTime.exitCurve, null, true, true);
		}

		// Token: 0x04002637 RID: 9783
		protected SpellPowerSlowTime slowTime;
	}
}
