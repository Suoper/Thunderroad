using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002E0 RID: 736
	public class LoreGazeReceiver : MonoBehaviour
	{
		// Token: 0x0600235F RID: 9055 RVA: 0x000F1C08 File Offset: 0x000EFE08
		protected void LateUpdate()
		{
			if (UpdateManager.frameCount % 60 != 0)
			{
				return;
			}
			if (Vector3.Distance(base.transform.position, Player.local.head.cam.transform.position) <= this.distanceThreshold)
			{
				this.dir = (base.transform.position - Player.local.head.cam.transform.position).normalized;
				if (Vector3.Dot(this.dir, Player.local.head.cam.transform.forward) > this.lookThreshold)
				{
					if (!this.looking)
					{
						this.looking = true;
						base.StartCoroutine(this.BeingLookedAtRoutine());
						return;
					}
				}
				else
				{
					base.StopAllCoroutines();
					this.looking = false;
				}
			}
		}

		// Token: 0x06002360 RID: 9056 RVA: 0x000F1CE1 File Offset: 0x000EFEE1
		private void OnDestroy()
		{
			if (this.item == null)
			{
				return;
			}
			this.item.OnGrabEvent -= this.OnGrab;
		}

		// Token: 0x06002361 RID: 9057 RVA: 0x000F1D04 File Offset: 0x000EFF04
		public void Init(LoreSpawner spawner, Item item, float lookDuration, float lookThreshold, float distanceThreshold)
		{
			if (item == null || spawner == null)
			{
				return;
			}
			this.item = item;
			this.item.OnGrabEvent += this.OnGrab;
			this.loreSpawner = spawner;
			this.lookDuration = lookDuration;
			this.lookThreshold = lookThreshold;
			this.distanceThreshold = distanceThreshold;
		}

		// Token: 0x06002362 RID: 9058 RVA: 0x000F1D54 File Offset: 0x000EFF54
		private IEnumerator BeingLookedAtRoutine()
		{
			float t = 0f;
			while (this.looking && t < this.lookDuration)
			{
				t += Time.deltaTime;
				yield return null;
			}
			if (this.looking)
			{
				this.loreSpawner.MarkAsRead(true);
				UnityEngine.Object.Destroy(this);
			}
			yield break;
		}

		// Token: 0x06002363 RID: 9059 RVA: 0x000F1D63 File Offset: 0x000EFF63
		private void OnGrab(Handle handle, RagdollHand ragdollHand)
		{
			this.loreSpawner.MarkAsRead(true);
			base.StopAllCoroutines();
			UnityEngine.Object.Destroy(this);
		}

		// Token: 0x06002364 RID: 9060 RVA: 0x000F1D80 File Offset: 0x000EFF80
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.cyan;
			Gizmos.DrawRay(new Ray(Player.local.head.cam.transform.position, this.dir));
			Gizmos.color = Color.red;
			Gizmos.DrawRay(new Ray(Player.local.head.cam.transform.position, Player.local.head.cam.transform.forward));
		}

		// Token: 0x06002365 RID: 9061 RVA: 0x000F1E06 File Offset: 0x000F0006
		public void MarkAsRead()
		{
			this.loreSpawner.MarkAsRead(true);
		}

		// Token: 0x04002265 RID: 8805
		public LoreSpawner loreSpawner;

		// Token: 0x04002266 RID: 8806
		public Vector3 dir;

		// Token: 0x04002267 RID: 8807
		private bool looking;

		// Token: 0x04002268 RID: 8808
		[NonSerialized]
		public float distanceThreshold = 1f;

		// Token: 0x04002269 RID: 8809
		[NonSerialized]
		public float lookDuration;

		// Token: 0x0400226A RID: 8810
		[NonSerialized]
		public float lookThreshold;

		// Token: 0x0400226B RID: 8811
		private Item item;
	}
}
