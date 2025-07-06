using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x020002F2 RID: 754
	[AddComponentMenu("ThunderRoad/Levels/Shaker")]
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/Shaker.html")]
	public class Shaker : MonoBehaviour
	{
		// Token: 0x0600240D RID: 9229 RVA: 0x000F67EC File Offset: 0x000F49EC
		public void End()
		{
			if (this.fadeIntensityCoroutine != null)
			{
				base.StopCoroutine(this.fadeIntensityCoroutine);
			}
			float currentIntensity = this.shakeIntensity;
			this.fadeIntensityCoroutine = this.ProgressiveAction(this.endDuration, delegate(float t)
			{
				this.shakeIntensity = Mathf.Lerp(currentIntensity, 0f, t);
			});
			if (this.audioSource)
			{
				this.audioSource.StopFade(ref this.audioCoroutine, this, this.endDuration);
			}
			foreach (Item item in this.zeroDragItems)
			{
				if (item)
				{
					item.RemovePhysicModifier(this);
				}
			}
			this.endCoroutine = this.DelayedAction(this.endDuration, delegate
			{
				this.shakeIntensity = 0f;
				this.StopAllCoroutines();
				Player.local.headOffsetTransform.localPosition = this.orgPlayerOffsetLocalPosition;
				UnityEvent unityEvent = this.onShakeEnd;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			});
		}

		// Token: 0x0600240E RID: 9230 RVA: 0x000F68D8 File Offset: 0x000F4AD8
		public void Begin()
		{
			if (this.endCoroutine != null)
			{
				base.StopCoroutine(this.endCoroutine);
			}
			if (this.fadeIntensityCoroutine != null)
			{
				base.StopCoroutine(this.fadeIntensityCoroutine);
			}
			float currentIntensity = this.shakeIntensity;
			this.fadeIntensityCoroutine = this.ProgressiveAction(this.startupDuration, delegate(float t)
			{
				this.shakeIntensity = Mathf.Lerp(currentIntensity, 1f, t);
			});
			if (this.audioSource)
			{
				this.audioSource.PlayFade(ref this.audioCoroutine, this, this.startupDuration, false, null);
			}
			this.itemShakeCoroutine = base.StartCoroutine(this.ItemShakeCoroutine());
			foreach (Item item in this.zeroDragItems)
			{
				if (item)
				{
					item.SetPhysicModifier(this, null, 1f, 0f, 0f, -1f, null);
				}
			}
			if (this.playerCameraShake)
			{
				this.playerShakeCoroutine = base.StartCoroutine(this.PlayerShakeCoroutine());
			}
			UnityEvent unityEvent = this.onShakeBegin;
			if (unityEvent == null)
			{
				return;
			}
			unityEvent.Invoke();
		}

		// Token: 0x0600240F RID: 9231 RVA: 0x000F6A14 File Offset: 0x000F4C14
		public Vector3 GetRandomDirectionInCone(Vector3 baseDirection)
		{
			float angle = UnityEngine.Random.Range(0f, this.coneAngle);
			Vector3 randomRotationAxis = Vector3.Cross(baseDirection, UnityEngine.Random.onUnitSphere).normalized;
			return Quaternion.AngleAxis(angle, randomRotationAxis) * baseDirection;
		}

		// Token: 0x06002410 RID: 9232 RVA: 0x000F6A51 File Offset: 0x000F4C51
		private IEnumerator ItemShakeCoroutine()
		{
			for (;;)
			{
				Vector3 force = this.GetRandomDirectionInCone(this.direction) * UnityEngine.Random.Range(this.itemShakeMinMaxForce.x, this.itemShakeMinMaxForce.y);
				if (this.startupDuration > 0f)
				{
					force *= this.shakeIntensity;
				}
				foreach (KeyValuePair<Item, int> itemDic in this.zone.itemsInZone)
				{
					if (!this.IsInItemMagnet(itemDic.Key) && !this.ignoreItems.Contains(itemDic.Key) && !itemDic.Key.isFlying)
					{
						foreach (CollisionHandler collisionHandler in itemDic.Key.collisionHandlers)
						{
							bool isColliding = collisionHandler.isColliding;
						}
						itemDic.Key.physicBody.AddForceAtPosition(force, itemDic.Key.transform.TransformPoint(UnityEngine.Random.insideUnitSphere), ForceMode.VelocityChange);
					}
				}
				yield return new WaitForSeconds(UnityEngine.Random.Range(this.itemShakeInterval.x, this.itemShakeInterval.y));
			}
			yield break;
		}

		// Token: 0x06002411 RID: 9233 RVA: 0x000F6A60 File Offset: 0x000F4C60
		private IEnumerator PlayerShakeCoroutine()
		{
			this.orgPlayerOffsetLocalPosition = Player.local.headOffsetTransform.localPosition;
			for (;;)
			{
				if (this.zone.playerInZone && Player.local.locomotion.isGrounded)
				{
					Vector3 shakeNewPos = this.orgPlayerOffsetLocalPosition + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(this.cameraShakeMinMaxIntensity.x, this.cameraShakeMinMaxIntensity.y);
					if (this.startupDuration > 0f)
					{
						shakeNewPos *= this.shakeIntensity;
					}
					Player.local.headOffsetTransform.localPosition = shakeNewPos;
				}
				yield return new WaitForEndOfFrame();
			}
			yield break;
		}

		// Token: 0x06002412 RID: 9234 RVA: 0x000F6A70 File Offset: 0x000F4C70
		protected bool IsInItemMagnet(Item item)
		{
			foreach (ItemMagnet itemMagnet in this.ignoreItemMagnets)
			{
				using (List<ItemMagnet.CapturedItem>.Enumerator enumerator2 = itemMagnet.capturedItems.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						if (enumerator2.Current.item == item)
						{
							return true;
						}
					}
				}
			}
			return false;
		}

		// Token: 0x06002413 RID: 9235 RVA: 0x000F6B08 File Offset: 0x000F4D08
		public static void ShakePlayer(float duration, float multiplier, AnimationCurve curve, Vector2 minMaxIntensity)
		{
			Shaker.ShakePlayer(duration, multiplier, curve, minMaxIntensity.x, minMaxIntensity.x);
		}

		// Token: 0x06002414 RID: 9236 RVA: 0x000F6B20 File Offset: 0x000F4D20
		public static void ShakePlayer(float duration, float multiplier = 1f, AnimationCurve curve = null, float minIntensity = 0.005f, float maxIntensity = 0.01f)
		{
			Vector3 orgPlayerOffset = Player.local.headOffsetTransform.localPosition;
			Vector2 minMaxIntensity = new Vector2(minIntensity, maxIntensity);
			Player.local.LoopOver(delegate(float value)
			{
				Vector3 shakeNewPos = orgPlayerOffset + UnityEngine.Random.insideUnitSphere * UnityEngine.Random.Range(minMaxIntensity.x, minMaxIntensity.y);
				Vector3 a = shakeNewPos;
				AnimationCurve curve2 = curve;
				shakeNewPos = a * (((curve2 != null) ? curve2.Evaluate(value) : 1f) * multiplier);
				Player.local.headOffsetTransform.localPosition = shakeNewPos;
			}, duration, delegate()
			{
				Player.local.headOffsetTransform.localPosition = orgPlayerOffset;
			}, 0f, true);
		}

		// Token: 0x0400234C RID: 9036
		[Header("Global")]
		[Tooltip("References the zone of which apples shaking to items/players")]
		public Zone zone;

		// Token: 0x0400234D RID: 9037
		[Tooltip("The audio that plays when shaking is active.")]
		public AudioSource audioSource;

		// Token: 0x0400234E RID: 9038
		[Tooltip("How long the startup of the shaking takes.")]
		public float startupDuration = 2f;

		// Token: 0x0400234F RID: 9039
		[Tooltip("How long the ending of the shaking takes.")]
		public float endDuration = 2f;

		// Token: 0x04002350 RID: 9040
		[Header("Items")]
		[Tooltip("The amount of force added to items in shake (X=Minimum, Y=Maximum)")]
		public Vector2 itemShakeMinMaxForce = new Vector2(0.01f, 0.1f);

		// Token: 0x04002351 RID: 9041
		[Tooltip("Time between each individual shake for items (X=Minimum, Y=Maximum)")]
		public Vector2 itemShakeInterval = new Vector2(0.005f, 0.01f);

		// Token: 0x04002352 RID: 9042
		[Tooltip("The maximum random angle (away from the shake axis), along which shake force is applied.")]
		public float coneAngle = 20f;

		// Token: 0x04002353 RID: 9043
		[Tooltip("The axis that the item shakes on.")]
		public Vector3 direction = Vector3.up;

		// Token: 0x04002354 RID: 9044
		[Tooltip("Ignores listed items in the scene.")]
		public List<Item> ignoreItems;

		// Token: 0x04002355 RID: 9045
		[Tooltip("Ignores items inside listed item magnets.")]
		public List<ItemMagnet> ignoreItemMagnets;

		// Token: 0x04002356 RID: 9046
		[Tooltip("List items that will recieve zero drag during shake.")]
		public List<Item> zeroDragItems;

		// Token: 0x04002357 RID: 9047
		[Header("Player")]
		[Tooltip("Does the player camera shake?")]
		public bool playerCameraShake = true;

		// Token: 0x04002358 RID: 9048
		[Tooltip("The intensity of the camera shaking.")]
		public Vector2 cameraShakeMinMaxIntensity = new Vector2(0.005f, 0.01f);

		// Token: 0x04002359 RID: 9049
		[Header("Events")]
		public UnityEvent onShakeBegin;

		// Token: 0x0400235A RID: 9050
		public UnityEvent onShakeEnd;

		// Token: 0x0400235B RID: 9051
		protected Vector3 orgPlayerOffsetLocalPosition;

		// Token: 0x0400235C RID: 9052
		protected float shakeIntensity;

		// Token: 0x0400235D RID: 9053
		protected Coroutine audioCoroutine;

		// Token: 0x0400235E RID: 9054
		protected Coroutine fadeIntensityCoroutine;

		// Token: 0x0400235F RID: 9055
		protected Coroutine itemShakeCoroutine;

		// Token: 0x04002360 RID: 9056
		protected Coroutine playerShakeCoroutine;

		// Token: 0x04002361 RID: 9057
		protected Coroutine endCoroutine;
	}
}
