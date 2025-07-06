using System;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002EE RID: 750
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Misc/Repeater.html")]
	public class Repeater : MonoBehaviour
	{
		// Token: 0x060023F4 RID: 9204 RVA: 0x000F5E7D File Offset: 0x000F407D
		private void OnEnable()
		{
			base.InvokeRepeating("Repeat", this.startTime, this.interval);
		}

		// Token: 0x060023F5 RID: 9205 RVA: 0x000F5E96 File Offset: 0x000F4096
		private void OnDisable()
		{
			base.CancelInvoke("Repeat");
		}

		// Token: 0x060023F6 RID: 9206 RVA: 0x000F5EA3 File Offset: 0x000F40A3
		private void Repeat()
		{
			UnityEvent unityEvent = this.onRepeat;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x04002320 RID: 8992
		public float startTime;

		// Token: 0x04002321 RID: 8993
		public float interval = 1f;

		// Token: 0x04002322 RID: 8994
		public UnityEvent onRepeat;
	}
}
