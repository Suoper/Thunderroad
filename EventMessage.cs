using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;

namespace ThunderRoad
{
	// Token: 0x020002C8 RID: 712
	public class EventMessage : MonoBehaviour
	{
		// Token: 0x1700021D RID: 541
		// (get) Token: 0x0600227B RID: 8827 RVA: 0x000ED9AE File Offset: 0x000EBBAE
		// (set) Token: 0x0600227C RID: 8828 RVA: 0x000ED9B6 File Offset: 0x000EBBB6
		public DisplayMessage.MessageData MessageData { get; private set; }

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x0600227D RID: 8829 RVA: 0x000ED9BF File Offset: 0x000EBBBF
		// (set) Token: 0x0600227E RID: 8830 RVA: 0x000ED9C7 File Offset: 0x000EBBC7
		public Texture Image { get; private set; }

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x0600227F RID: 8831 RVA: 0x000ED9D0 File Offset: 0x000EBBD0
		// (set) Token: 0x06002280 RID: 8832 RVA: 0x000ED9D8 File Offset: 0x000EBBD8
		public VideoClip VideoClip { get; private set; }

		// Token: 0x06002281 RID: 8833 RVA: 0x000ED9E1 File Offset: 0x000EBBE1
		private void Awake()
		{
			this.MessageData = new DisplayMessage.MessageData(this);
			this.loadMediaAssetsCoroutine = this.LoadMediaAssetsCoroutine();
			base.StartCoroutine(this.loadMediaAssetsCoroutine);
			this.onMessageSkip.AddListener(delegate(DisplayMessage.MessageData data)
			{
				UnityEvent unityEvent = this.messageSkipEvent;
				if (unityEvent == null)
				{
					return;
				}
				unityEvent.Invoke();
			});
		}

		/// <summary>
		/// Load image and/or video assets for this message
		/// </summary>
		// Token: 0x06002282 RID: 8834 RVA: 0x000EDA1F File Offset: 0x000EBC1F
		public IEnumerator LoadMediaAssetsCoroutine()
		{
			if (!string.IsNullOrEmpty(this.imageAddress))
			{
				this.imageAddress = Catalog.GetImagePlatformAddress(this.imageAddress);
				yield return Catalog.LoadAssetCoroutine<Texture>(this.imageAddress, delegate(Texture value)
				{
					this.Image = value;
				}, "EventMessage");
				this.MessageData.image = this.Image;
			}
			if (!string.IsNullOrEmpty(this.videoAddress))
			{
				this.videoAddress = Catalog.GetVideoPlatformAddress(this.videoAddress);
				yield return Catalog.LoadAssetCoroutine<VideoClip>(this.videoAddress, delegate(VideoClip value)
				{
					this.VideoClip = value;
				}, "EventMessage");
				this.MessageData.videoClip = this.VideoClip;
			}
			this.loadMediaAssetsCoroutine = null;
			yield break;
		}

		// Token: 0x06002283 RID: 8835 RVA: 0x000EDA30 File Offset: 0x000EBC30
		public void ShowMessage()
		{
			if (!string.IsNullOrEmpty(this.localizationGroupId) && !string.IsNullOrEmpty(this.localizationStringId))
			{
				string localizedText = LocalizationManager.Instance.GetLocalizedString(this.localizationGroupId, this.localizationStringId, false);
				if (localizedText != null)
				{
					this.text = localizedText;
				}
			}
			this.MessageData.ShowMessageCalled = true;
			if (this.showMessageCoroutine != null)
			{
				base.StopCoroutine(this.showMessageCoroutine);
				this.showMessageCoroutine = null;
			}
			this.showMessageCoroutine = this.ShowMessageCoroutine();
			base.StartCoroutine(this.showMessageCoroutine);
		}

		// Token: 0x06002284 RID: 8836 RVA: 0x000EDAB9 File Offset: 0x000EBCB9
		private IEnumerator ShowMessageCoroutine()
		{
			yield return new WaitForSeconds(this.showDelay);
			this.showMessageCoroutine = null;
			if (Player.local.creature == null)
			{
				yield break;
			}
			((this.customDisplay != null) ? this.customDisplay : DisplayMessage.instance).ShowMessage(this.MessageData);
			yield break;
		}

		/// <summary>
		/// Reset the state of this event message
		/// </summary>
		// Token: 0x06002285 RID: 8837 RVA: 0x000EDAC8 File Offset: 0x000EBCC8
		public void ResetMessage()
		{
			if (this.MessageData.ReleasedMediaAssets)
			{
				if (this.loadMediaAssetsCoroutine != null)
				{
					base.StopCoroutine(this.loadMediaAssetsCoroutine);
					this.loadMediaAssetsCoroutine = null;
				}
				this.loadMediaAssetsCoroutine = this.LoadMediaAssetsCoroutine();
				base.StartCoroutine(this.loadMediaAssetsCoroutine);
			}
			if (this.showMessageCoroutine != null)
			{
				base.StopCoroutine(this.showMessageCoroutine);
				this.showMessageCoroutine = null;
			}
			this.MessageData.ResetData();
		}

		// Token: 0x06002286 RID: 8838 RVA: 0x000EDB3C File Offset: 0x000EBD3C
		public void StopMessage()
		{
			((this.customDisplay != null) ? this.customDisplay : DisplayMessage.instance).StopMessage();
		}

		// Token: 0x04002197 RID: 8599
		public DisplayMessage customDisplay;

		// Token: 0x04002198 RID: 8600
		[TextArea(0, 20)]
		public string text;

		// Token: 0x04002199 RID: 8601
		public string localizationGroupId;

		// Token: 0x0400219A RID: 8602
		public string localizationStringId;

		// Token: 0x0400219B RID: 8603
		public int priority;

		// Token: 0x0400219C RID: 8604
		public float showDelay;

		// Token: 0x0400219D RID: 8605
		public string imageAddress;

		// Token: 0x0400219E RID: 8606
		public string videoAddress;

		// Token: 0x0400219F RID: 8607
		public bool fitVideoHorizontally;

		// Token: 0x040021A0 RID: 8608
		public bool warnPlayer = true;

		// Token: 0x040021A1 RID: 8609
		public MessageAnchorType anchorType = MessageAnchorType.HandLeft;

		// Token: 0x040021A2 RID: 8610
		public Transform anchorTargetTransform;

		// Token: 0x040021A3 RID: 8611
		public bool dismissAutomatically;

		// Token: 0x040021A4 RID: 8612
		public float dismissTime = 2f;

		// Token: 0x040021A5 RID: 8613
		public UnityEvent<DisplayMessage.MessageData> onMessageSkip;

		// Token: 0x040021A6 RID: 8614
		public UnityEvent messageSkipEvent;

		// Token: 0x040021A7 RID: 8615
		[HideInInspector]
		public bool startFromZoneTrigger;

		// Token: 0x040021A8 RID: 8616
		[HideInInspector]
		public bool stopFromZoneTrigger;

		// Token: 0x040021A9 RID: 8617
		[HideInInspector]
		public bool isTutorialMessage;

		// Token: 0x040021AA RID: 8618
		[HideInInspector]
		public bool isSkippable = true;

		// Token: 0x040021AB RID: 8619
		private IEnumerator loadMediaAssetsCoroutine;

		// Token: 0x040021AC RID: 8620
		private IEnumerator showMessageCoroutine;
	}
}
