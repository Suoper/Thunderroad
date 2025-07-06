using System;
using ThunderRoad.Modules;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002C4 RID: 708
	public class DungeonSuccessTrigger : MonoBehaviour
	{
		// Token: 0x06002268 RID: 8808 RVA: 0x000ED64C File Offset: 0x000EB84C
		public void DungeonSuccess()
		{
			this.hasActivated = true;
			LevelInstance levelInstance;
			if (LevelInstancesModule.TryGetCurrentLevelInstance(out levelInstance))
			{
				EventManager.InvokeDungeonSuccess(levelInstance);
				return;
			}
			if (GameModeManager.instance != null && GameModeManager.instance.currentGameMode.name.Equals("CrystalHunt"))
			{
				Debug.LogError("Unable to invoke dungeon success. Could not get current level instance.");
			}
		}

		// Token: 0x04002183 RID: 8579
		private bool hasActivated;
	}
}
