using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002AA RID: 682
	public class AnimationFxPlayer : MonoBehaviour
	{
		// Token: 0x140000EB RID: 235
		// (add) Token: 0x06001FA1 RID: 8097 RVA: 0x000D6F6C File Offset: 0x000D516C
		// (remove) Token: 0x06001FA2 RID: 8098 RVA: 0x000D6FA4 File Offset: 0x000D51A4
		public event AnimationFxPlayer.PlayEvent OnPlayEvent;

		// Token: 0x06001FA3 RID: 8099 RVA: 0x000D6FD9 File Offset: 0x000D51D9
		public void Awake()
		{
			if (this.controllers == null)
			{
				this.controllers = base.GetComponentsInChildren<FxController>();
			}
		}

		// Token: 0x06001FA4 RID: 8100 RVA: 0x000D6FEF File Offset: 0x000D51EF
		public void Play(int index)
		{
			if (index > this.controllers.Length)
			{
				return;
			}
			this.controllers[index].Play();
			AnimationFxPlayer.PlayEvent onPlayEvent = this.OnPlayEvent;
			if (onPlayEvent == null)
			{
				return;
			}
			onPlayEvent(this, index, this.controllers[index]);
		}

		// Token: 0x04001EDB RID: 7899
		public FxController[] controllers;

		// Token: 0x0200093F RID: 2367
		// (Invoke) Token: 0x060042F7 RID: 17143
		public delegate void PlayEvent(AnimationFxPlayer player, int index, FxController controller);
	}
}
