using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002C6 RID: 710
	public class EventGameMode : MonoBehaviour
	{
		// Token: 0x06002273 RID: 8819 RVA: 0x000ED83D File Offset: 0x000EBA3D
		private void Awake()
		{
			if (GameModeManager.instance.currentGameMode.id == this.gameModeId)
			{
				UnityEvent unityEvent = this.onGameModeEqual;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
				return;
			}
			else
			{
				UnityEvent unityEvent2 = this.onGameModeNotEqual;
				if (unityEvent2 == null)
				{
					return;
				}
				unityEvent2.Invoke();
				return;
			}
		}

		// Token: 0x0400218F RID: 8591
		public string gameModeId;

		// Token: 0x04002190 RID: 8592
		public UnityEvent onGameModeEqual;

		// Token: 0x04002191 RID: 8593
		public UnityEvent onGameModeNotEqual;
	}
}
