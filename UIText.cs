using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000395 RID: 917
	public class UIText : MonoBehaviour
	{
		/// <summary>
		/// Invoked when this changes the target text component.
		/// </summary>
		// Token: 0x14000144 RID: 324
		// (add) Token: 0x06002BAA RID: 11178 RVA: 0x00126998 File Offset: 0x00124B98
		// (remove) Token: 0x06002BAB RID: 11179 RVA: 0x001269D0 File Offset: 0x00124BD0
		public event Action TextChanged;

		// Token: 0x06002BAC RID: 11180 RVA: 0x00126A05 File Offset: 0x00124C05
		public List<ValueDropdownItem<string>> GetAllTextGroupID()
		{
			return Catalog.GetTextData().GetDropdownAllTextGroups();
		}

		// Token: 0x170002A1 RID: 673
		// (get) Token: 0x06002BAD RID: 11181 RVA: 0x00126A11 File Offset: 0x00124C11
		// (set) Token: 0x06002BAE RID: 11182 RVA: 0x00126A19 File Offset: 0x00124C19
		public TMP_Text textComponent { get; protected set; }

		// Token: 0x06002BAF RID: 11183 RVA: 0x00126A22 File Offset: 0x00124C22
		private void Awake()
		{
			this.textComponent = base.GetComponentInChildren<TMP_Text>();
			this.language = LocalizationManager.Instance.Language;
			TMP_Text textComponent = this.textComponent;
			this.origFont = ((textComponent != null) ? textComponent.font : null);
		}

		// Token: 0x06002BB0 RID: 11184 RVA: 0x00126A58 File Offset: 0x00124C58
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			if (!GameManager.local)
			{
				base.enabled = false;
				return;
			}
			if (!this.setFromScript || (this.setFromScript && this.wasInitializedFromScript))
			{
				this.Refresh(this.ForceEnglishForNonLatin && !LocalizationManager.Instance.isLatinLanguage);
			}
			if (LocalizationManager.Instance.Language != this.language)
			{
				this.Refresh(this.ForceEnglishForNonLatin && !LocalizationManager.Instance.isLatinLanguage);
				this.language = LocalizationManager.Instance.Language;
			}
		}

		// Token: 0x06002BB1 RID: 11185 RVA: 0x00126B05 File Offset: 0x00124D05
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002BB2 RID: 11186 RVA: 0x00126B18 File Offset: 0x00124D18
		private void OnLanguageChanged(string language)
		{
			this.Refresh(this.ForceEnglishForNonLatin && !LocalizationManager.Instance.isLatinLanguage);
			if (this.ForceLatinFontForNonLatin && this.origFont != null)
			{
				this.textComponent.font = (LocalizationManager.Instance.isLatinLanguage ? this.origFont : LocalizationManager.Instance.NonLatinFont);
			}
		}

		// Token: 0x06002BB3 RID: 11187 RVA: 0x00126B84 File Offset: 0x00124D84
		public virtual void Refresh(bool forceEnglish = false)
		{
			if (this.hasCustomLocalization)
			{
				return;
			}
			if (this.textComponent == null)
			{
				this.textComponent = base.GetComponentInChildren<TMP_Text>();
			}
			if (this.textComponent == null)
			{
				return;
			}
			if (this.textIsItem)
			{
				ItemData itemData;
				if (Catalog.TryGetData<ItemData>(this.text, out itemData, true))
				{
					TextData.Item textItem = LocalizationManager.Instance.GetLocalizedTextItem(itemData.localizationId);
					this.textComponent.text = ((textItem != null) ? textItem.name : itemData.displayName);
					Action textChanged = this.TextChanged;
					if (textChanged != null)
					{
						textChanged();
					}
				}
			}
			else
			{
				this.textComponent.text = LocalizationManager.Instance.TryGetLocalization(this.textGroupId, this.text, null, forceEnglish);
				Action textChanged2 = this.TextChanged;
				if (textChanged2 != null)
				{
					textChanged2();
				}
			}
			if (this.textComponent.text != null && this.forceUpperCase)
			{
				this.textComponent.text = this.textComponent.text.ToUpper();
				Action textChanged3 = this.TextChanged;
				if (textChanged3 == null)
				{
					return;
				}
				textChanged3();
			}
		}

		/// <summary>
		/// Use this method to change this label text from script.
		/// Once this localization data is set, the text label will automatically update on
		/// language change if this text is enable, otherwise it will update when its enabled.
		/// </summary>
		/// <param name="groupId">Id of the text group in the text files</param>
		/// <param name="stringId">Id of the string</param>
		// Token: 0x06002BB4 RID: 11188 RVA: 0x00126C8F File Offset: 0x00124E8F
		public void SetLocalizationIds(string groupId, string stringId)
		{
			this.textGroupId = groupId;
			this.text = stringId;
			this.wasInitializedFromScript = true;
			this.hasCustomLocalization = false;
			this.Refresh(false);
		}

		/// <summary>
		/// Use this method to set localized text that does not come from a TextGroup (ex: Items or Waves)
		/// </summary>
		// Token: 0x06002BB5 RID: 11189 RVA: 0x00126CB4 File Offset: 0x00124EB4
		public void SetLocalizedText(string localizedText)
		{
			this.wasInitializedFromScript = true;
			this.hasCustomLocalization = true;
			if (this.textComponent == null)
			{
				this.textComponent = base.GetComponentInChildren<TMP_Text>();
			}
			if (this.textComponent != null)
			{
				this.textComponent.text = localizedText;
				Action textChanged = this.TextChanged;
				if (textChanged == null)
				{
					return;
				}
				textChanged();
			}
		}

		// Token: 0x06002BB6 RID: 11190 RVA: 0x00126D13 File Offset: 0x00124F13
		public void SetItemLocalizationId(string itemId)
		{
			this.textIsItem = true;
			this.SetLocalizationIds("", itemId);
		}

		// Token: 0x06002BB7 RID: 11191 RVA: 0x00126D28 File Offset: 0x00124F28
		public void SetColor(Color color)
		{
			if (this.textComponent != null)
			{
				this.textComponent.color = color;
			}
		}

		// Token: 0x0400292F RID: 10543
		public string textGroupId;

		// Token: 0x04002930 RID: 10544
		[Tooltip("Enable this flag to have dynamic localized texts set from script. *The group id must be set on the inspector.")]
		public bool setFromScript;

		// Token: 0x04002931 RID: 10545
		[Multiline]
		public string text;

		// Token: 0x04002932 RID: 10546
		public bool forceUpperCase;

		// Token: 0x04002934 RID: 10548
		public bool ForceEnglishForNonLatin;

		// Token: 0x04002935 RID: 10549
		public bool ForceLatinFontForNonLatin;

		// Token: 0x04002936 RID: 10550
		private bool hasCustomLocalization;

		// Token: 0x04002937 RID: 10551
		private bool wasInitializedFromScript;

		// Token: 0x04002938 RID: 10552
		private bool textIsItem;

		// Token: 0x04002939 RID: 10553
		private string language;

		// Token: 0x0400293A RID: 10554
		private TMP_FontAsset origFont;
	}
}
