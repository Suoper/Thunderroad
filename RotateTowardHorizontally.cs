using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002F1 RID: 753
	public class RotateTowardHorizontally : MonoBehaviour
	{
		// Token: 0x06002408 RID: 9224 RVA: 0x000F6780 File Offset: 0x000F4980
		public void OnEnable()
		{
			base.StartCoroutine(this.EnableCoroutine());
		}

		// Token: 0x06002409 RID: 9225 RVA: 0x000F678F File Offset: 0x000F498F
		private IEnumerator EnableCoroutine()
		{
			yield return null;
			if (this.onEnableDelay > 0f)
			{
				yield return new WaitForSeconds(this.onEnableDelay);
			}
			this.UpdateTransform();
			yield break;
		}

		// Token: 0x0600240A RID: 9226 RVA: 0x000F679E File Offset: 0x000F499E
		private void Update()
		{
			if (this.onUpdate)
			{
				this.UpdateTransform();
			}
		}

		// Token: 0x0600240B RID: 9227 RVA: 0x000F67AE File Offset: 0x000F49AE
		public void UpdateTransform()
		{
			base.transform.rotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(this.target.forward, Vector3.up), Vector3.up);
		}

		// Token: 0x04002348 RID: 9032
		public Transform target;

		// Token: 0x04002349 RID: 9033
		public bool onEnable = true;

		// Token: 0x0400234A RID: 9034
		public float onEnableDelay;

		// Token: 0x0400234B RID: 9035
		public bool onUpdate;
	}
}
