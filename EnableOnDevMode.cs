using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200033A RID: 826
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/EnableOnDevMode")]
	[AddComponentMenu("ThunderRoad/Misc/Enable on DevMode")]
	public class EnableOnDevMode : MonoBehaviour
	{
		// Token: 0x0600265C RID: 9820 RVA: 0x0010A70F File Offset: 0x0010890F
		private void Awake()
		{
			GameManager.onDevModeActivated = (Action)Delegate.Combine(GameManager.onDevModeActivated, new Action(this.OnDevModeActivated));
		}

		// Token: 0x0600265D RID: 9821 RVA: 0x0010A731 File Offset: 0x00108931
		private void OnEnable()
		{
			if (GameManager.DevMode)
			{
				return;
			}
			base.gameObject.SetActive(false);
		}

		// Token: 0x0600265E RID: 9822 RVA: 0x0010A747 File Offset: 0x00108947
		private void OnDevModeActivated()
		{
			base.gameObject.SetActive(GameManager.DevMode);
		}

		// Token: 0x0600265F RID: 9823 RVA: 0x0010A759 File Offset: 0x00108959
		private void OnDestroy()
		{
			GameManager.onDevModeActivated = (Action)Delegate.Remove(GameManager.onDevModeActivated, new Action(this.OnDevModeActivated));
		}
	}
}
