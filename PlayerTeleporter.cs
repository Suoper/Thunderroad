using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002EB RID: 747
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/PlayerTeleporter")]
	public class PlayerTeleporter : MonoBehaviour
	{
		/// <summary>
		/// Teleport the player to the target transform. If use Fading is set to true, fades in a coroutine.
		/// </summary>
		// Token: 0x060023E9 RID: 9193 RVA: 0x000F5B48 File Offset: 0x000F3D48
		public void Teleport()
		{
			if (!Player.local)
			{
				return;
			}
			if (this.useFading)
			{
				base.StartCoroutine(this.FadeAndTeleportPlayer());
				return;
			}
			this.Teleport(this.targetTeleportTransform);
		}

		// Token: 0x060023EA RID: 9194 RVA: 0x000F5B7C File Offset: 0x000F3D7C
		private void Teleport(Transform t)
		{
			if (this.useRelativePosition)
			{
				Vector3 playerLocalPosition = base.transform.InverseTransformPoint(Player.local.transform.position);
				Quaternion playerLocalRotation = base.transform.InverseTransformRotation(Player.local.transform.rotation);
				Player.local.Teleport(this.targetTeleportTransform.TransformPoint(playerLocalPosition), this.targetTeleportTransform.TransformRotation(playerLocalRotation), true, false);
			}
			else
			{
				Player.local.Teleport(t, false, true);
			}
			UnityEvent unityEvent = this.onPlayerTeleport;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		/// <summary>
		/// Fades in, teleport the player, then fades out.
		/// Uses the fadeInDuration and fadeOutDuration fields.
		/// </summary>
		// Token: 0x060023EB RID: 9195 RVA: 0x000F5C0A File Offset: 0x000F3E0A
		private IEnumerator FadeAndTeleportPlayer()
		{
			CameraEffects.DoFadeEffect(true, this.fadeInDuration);
			yield return Yielders.ForSeconds(this.fadeInDuration);
			this.Teleport(this.targetTeleportTransform);
			CameraEffects.DoFadeEffect(false, this.fadeOutDuration);
			yield break;
		}

		// Token: 0x060023EC RID: 9196 RVA: 0x000F5C1C File Offset: 0x000F3E1C
		private void OnDrawGizmos()
		{
			if (!this.targetTeleportTransform)
			{
				return;
			}
			Gizmos.color = new Color(0.05f, 1f, 0.7f);
			Vector3 position = this.targetTeleportTransform.position;
			Gizmos.DrawLine(position, position + Vector3.up * 100f);
			Vector3 forward = this.targetTeleportTransform.forward;
			Gizmos.DrawLine(position, position + forward * 3f);
			Gizmos.DrawLine(position + Vector3.up * 3f, position + forward * 3f);
		}

		// Token: 0x04002313 RID: 8979
		public Transform targetTeleportTransform;

		// Token: 0x04002314 RID: 8980
		public bool useRelativePosition;

		// Token: 0x04002315 RID: 8981
		public bool useFading = true;

		// Token: 0x04002316 RID: 8982
		public float fadeInDuration = 1f;

		// Token: 0x04002317 RID: 8983
		public float fadeOutDuration = 1f;

		// Token: 0x04002318 RID: 8984
		public UnityEvent onPlayerTeleport;
	}
}
