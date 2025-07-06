using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000394 RID: 916
	public class UIStatIcons : MonoBehaviour
	{
		// Token: 0x06002BA1 RID: 11169 RVA: 0x001268FC File Offset: 0x00124AFC
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.UpdateLocalisation;
		}

		// Token: 0x06002BA2 RID: 11170 RVA: 0x0012690F File Offset: 0x00124B0F
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.UpdateLocalisation;
		}

		// Token: 0x06002BA3 RID: 11171 RVA: 0x00126922 File Offset: 0x00124B22
		private void OnDestroy()
		{
			Catalog.ReleaseAsset<Sprite>(this.filledStarSprite);
			Catalog.ReleaseAsset<Sprite>(this.emptyStarSprite);
		}

		// Token: 0x06002BA4 RID: 11172 RVA: 0x0012693A File Offset: 0x00124B3A
		private void UpdateLocalisation(string _)
		{
			if (this.statName != null)
			{
				this.statName.text = this.stat.GetLocalisedName();
			}
		}

		// Token: 0x06002BA5 RID: 11173 RVA: 0x00126960 File Offset: 0x00124B60
		public IEnumerator Refresh()
		{
			yield return this.LoadAssets();
			if (this.emptyStarSprite == null)
			{
				Debug.LogError("[Inventory Stats]: emptyStarSprite is null");
			}
			if (this.filledStarSprite == null)
			{
				Debug.LogError("[Inventory Stats]: filledStarSprite is null");
			}
			if (this.stat == null)
			{
				yield break;
			}
			if (this.statName != null)
			{
				this.statName.text = this.stat.GetLocalisedName();
			}
			ItemStatInt statInt = this.stat as ItemStatInt;
			if (statInt != null)
			{
				if (!statInt.useStarIcons)
				{
					this.starIconsGameObject.SetActive(false);
					if (this.textValue != null)
					{
						this.textValue.text = statInt.GetValue().ToString();
					}
				}
				else
				{
					if (this.textValue != null)
					{
						this.textValue.text = string.Empty;
					}
					this.starIconsGameObject.SetActive(true);
					int value = statInt.GetValue();
					for (int i = 0; i < this.starIcons.Length; i++)
					{
						this.starIcons[i].sprite = ((i < value) ? this.filledStarSprite : this.emptyStarSprite);
						this.starIcons[i].color = ((i < value) ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.5f));
					}
				}
			}
			else
			{
				this.starIconsGameObject.SetActive(false);
				ItemStatString statString = this.stat as ItemStatString;
				if (statString != null)
				{
					if (this.textValue != null)
					{
						this.textValue.text = statString.GetValue();
					}
				}
				else
				{
					ItemStatFloat statFloat = this.stat as ItemStatFloat;
					if (statFloat != null && this.textValue != null)
					{
						this.textValue.text = statFloat.GetValue().ToString("0.00");
					}
				}
			}
			yield break;
		}

		// Token: 0x06002BA6 RID: 11174 RVA: 0x0012696F File Offset: 0x00124B6F
		private IEnumerator LoadAssets()
		{
			if (this.emptyStarSprite == null && !string.IsNullOrEmpty(this.emptyStarAddress))
			{
				yield return Catalog.LoadAssetCoroutine<Sprite>(this.emptyStarAddress, delegate(Sprite sprite)
				{
					this.emptyStarSprite = sprite;
				}, "UIItemStat");
			}
			if (this.filledStarSprite == null && !string.IsNullOrEmpty(this.filledStarAddress))
			{
				yield return Catalog.LoadAssetCoroutine<Sprite>(this.filledStarAddress, delegate(Sprite sprite)
				{
					this.filledStarSprite = sprite;
				}, "UIItemStat");
			}
			yield break;
		}

		// Token: 0x04002926 RID: 10534
		public string filledStarAddress;

		// Token: 0x04002927 RID: 10535
		public string emptyStarAddress;

		// Token: 0x04002928 RID: 10536
		public IStats stat;

		// Token: 0x04002929 RID: 10537
		public TMP_Text statName;

		// Token: 0x0400292A RID: 10538
		public TMP_Text textValue;

		// Token: 0x0400292B RID: 10539
		public GameObject starIconsGameObject;

		// Token: 0x0400292C RID: 10540
		public Image[] starIcons;

		// Token: 0x0400292D RID: 10541
		private Sprite emptyStarSprite;

		// Token: 0x0400292E RID: 10542
		private Sprite filledStarSprite;
	}
}
