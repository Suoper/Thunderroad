using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200035F RID: 863
	public class TriggerDetector : MonoBehaviour
	{
		// Token: 0x14000132 RID: 306
		// (add) Token: 0x0600287F RID: 10367 RVA: 0x00114B34 File Offset: 0x00112D34
		// (remove) Token: 0x06002880 RID: 10368 RVA: 0x00114B6C File Offset: 0x00112D6C
		public event TriggerDetector.TriggerEvent OnTriggerEnterEvent;

		// Token: 0x14000133 RID: 307
		// (add) Token: 0x06002881 RID: 10369 RVA: 0x00114BA4 File Offset: 0x00112DA4
		// (remove) Token: 0x06002882 RID: 10370 RVA: 0x00114BDC File Offset: 0x00112DDC
		public event TriggerDetector.TriggerEvent OnTriggerExitEvent;

		// Token: 0x14000134 RID: 308
		// (add) Token: 0x06002883 RID: 10371 RVA: 0x00114C14 File Offset: 0x00112E14
		// (remove) Token: 0x06002884 RID: 10372 RVA: 0x00114C4C File Offset: 0x00112E4C
		public event TriggerDetector.TriggerEvent OnTriggerStayEvent;

		// Token: 0x06002885 RID: 10373 RVA: 0x00114C81 File Offset: 0x00112E81
		private void OnTriggerEnter(Collider other)
		{
			TriggerDetector.TriggerEvent onTriggerEnterEvent = this.OnTriggerEnterEvent;
			if (onTriggerEnterEvent == null)
			{
				return;
			}
			onTriggerEnterEvent(other);
		}

		// Token: 0x06002886 RID: 10374 RVA: 0x00114C94 File Offset: 0x00112E94
		private void OnTriggerExit(Collider other)
		{
			TriggerDetector.TriggerEvent onTriggerExitEvent = this.OnTriggerExitEvent;
			if (onTriggerExitEvent == null)
			{
				return;
			}
			onTriggerExitEvent(other);
		}

		// Token: 0x06002887 RID: 10375 RVA: 0x00114CA7 File Offset: 0x00112EA7
		private void OnTriggerStay(Collider other)
		{
			TriggerDetector.TriggerEvent onTriggerStayEvent = this.OnTriggerStayEvent;
			if (onTriggerStayEvent == null)
			{
				return;
			}
			onTriggerStayEvent(other);
		}

		// Token: 0x02000A4D RID: 2637
		// (Invoke) Token: 0x060045B9 RID: 17849
		public delegate void TriggerEvent(Collider other);
	}
}
