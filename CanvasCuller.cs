using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000375 RID: 885
	[RequireComponent(typeof(Collider))]
	public class CanvasCuller : ThunderBehaviour
	{
		// Token: 0x06002A09 RID: 10761 RVA: 0x0011D654 File Offset: 0x0011B854
		private void Awake()
		{
			this.collider = base.GetComponent<Collider>();
			this.collider.isTrigger = true;
			BoxCollider boxCollider = this.collider as BoxCollider;
			if (boxCollider != null)
			{
				boxCollider.size = new Vector3(this.colliderSize, this.colliderSize, this.colliderSize);
			}
			else
			{
				SphereCollider sphereCollider = this.collider as SphereCollider;
				if (sphereCollider != null)
				{
					sphereCollider.radius = this.colliderSize;
				}
				else
				{
					CapsuleCollider capsuleCollider = this.collider as CapsuleCollider;
					if (capsuleCollider != null)
					{
						capsuleCollider.radius = this.colliderSize;
						capsuleCollider.height = this.colliderSize;
					}
				}
			}
			this.playerMask = 1 << GameManager.GetLayer(LayerName.PlayerLocomotion);
			this.canvasGroup.alpha = 0f;
		}

		// Token: 0x06002A0A RID: 10762 RVA: 0x0011D70D File Offset: 0x0011B90D
		private void OnSpectatorDisabled()
		{
			this.canvasGroup.alpha = (float)(this.playerClose ? 1 : 0);
		}

		// Token: 0x06002A0B RID: 10763 RVA: 0x0011D727 File Offset: 0x0011B927
		private void OnSpectatorEnabled()
		{
			if (Spectator.local.state == Spectator.State.Free)
			{
				this.canvasGroup.alpha = 1f;
				return;
			}
			this.canvasGroup.alpha = (float)(this.playerClose ? 1 : 0);
		}

		// Token: 0x06002A0C RID: 10764 RVA: 0x0011D760 File Offset: 0x0011B960
		protected override void ManagedOnEnable()
		{
			this.canvasGroup.alpha = (float)(this.playerClose ? 1 : 0);
			if (Spectator.local)
			{
				Spectator.local.onSpectatorEnabled += this.OnSpectatorEnabled;
				Spectator.local.onSpectatorDisabled += this.OnSpectatorDisabled;
			}
		}

		// Token: 0x06002A0D RID: 10765 RVA: 0x0011D7C0 File Offset: 0x0011B9C0
		protected override void ManagedOnDisable()
		{
			this.canvasGroup.alpha = 1f;
			if (Spectator.local)
			{
				Spectator.local.onSpectatorEnabled -= this.OnSpectatorEnabled;
				Spectator.local.onSpectatorDisabled -= this.OnSpectatorDisabled;
			}
		}

		// Token: 0x06002A0E RID: 10766 RVA: 0x0011D815 File Offset: 0x0011BA15
		protected bool IsInLayerMask(int layerMask, int layer)
		{
			return layerMask == (layerMask | 1 << layer);
		}

		// Token: 0x06002A0F RID: 10767 RVA: 0x0011D824 File Offset: 0x0011BA24
		private void OnTriggerEnter(Collider other)
		{
			if (!base.enabled)
			{
				this.playerClose = true;
				return;
			}
			if (this.fadeEnterStarted)
			{
				return;
			}
			PhysicBody physicBody = other.GetPhysicBody();
			int objectLayer = (physicBody != null) ? physicBody.gameObject.layer : this.collider.gameObject.layer;
			if (!this.IsInLayerMask(this.playerMask, objectLayer))
			{
				return;
			}
			if (this.fadeCoroutine != null)
			{
				this.fadeExitStarted = false;
				base.StopCoroutine(this.fadeCoroutine);
			}
			this.fadeCoroutine = base.StartCoroutine(this.FadeToAlpha(1f, this.fadeDuration, false));
			this.fadeEnterStarted = true;
			this.playerClose = true;
		}

		// Token: 0x06002A10 RID: 10768 RVA: 0x0011D8C8 File Offset: 0x0011BAC8
		private void OnTriggerExit(Collider other)
		{
			if (!base.enabled)
			{
				this.playerClose = false;
				return;
			}
			if (this.fadeExitStarted)
			{
				return;
			}
			PhysicBody physicBody = other.GetPhysicBody();
			int objectLayer = (physicBody != null) ? physicBody.gameObject.layer : this.collider.gameObject.layer;
			if (!this.IsInLayerMask(this.playerMask, objectLayer))
			{
				return;
			}
			if (this.fadeCoroutine != null)
			{
				this.fadeEnterStarted = false;
				base.StopCoroutine(this.fadeCoroutine);
			}
			this.fadeCoroutine = base.StartCoroutine(this.FadeToAlpha(0f, this.fadeDuration, true));
			this.fadeExitStarted = true;
			this.playerClose = false;
		}

		// Token: 0x06002A11 RID: 10769 RVA: 0x0011D96C File Offset: 0x0011BB6C
		private IEnumerator FadeToAlpha(float targetAlpha, float duration, bool isExit)
		{
			float startAlpha = this.canvasGroup.alpha;
			float time = 0f;
			float fadeTime = duration / 3f;
			while (time < duration)
			{
				time += fadeTime;
				this.canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
				yield return Yielders.ForRealSeconds(fadeTime);
			}
			this.canvasGroup.alpha = targetAlpha;
			if (isExit)
			{
				this.fadeExitStarted = false;
			}
			else
			{
				this.fadeEnterStarted = false;
			}
			yield break;
		}

		// Token: 0x040027DA RID: 10202
		public float fadeDuration = 0.5f;

		// Token: 0x040027DB RID: 10203
		public float colliderSize = 3f;

		// Token: 0x040027DC RID: 10204
		public Collider collider;

		// Token: 0x040027DD RID: 10205
		public CanvasGroup canvasGroup;

		// Token: 0x040027DE RID: 10206
		private int playerMask;

		// Token: 0x040027DF RID: 10207
		private Coroutine fadeCoroutine;

		// Token: 0x040027E0 RID: 10208
		private bool playerClose;

		// Token: 0x040027E1 RID: 10209
		private bool fadeEnterStarted;

		// Token: 0x040027E2 RID: 10210
		private bool fadeExitStarted;
	}
}
