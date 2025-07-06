using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000396 RID: 918
	public class UITextMisc : UIText
	{
		// Token: 0x06002BB9 RID: 11193 RVA: 0x00126D4C File Offset: 0x00124F4C
		public override void Refresh(bool forceEnglish = false)
		{
			if (base.textComponent == null)
			{
				base.textComponent = base.GetComponentInChildren<TextMeshProUGUI>();
			}
			if (base.textComponent == null)
			{
				Debug.LogError(this.value.ToString() + " - has no text component");
				return;
			}
			UITextMisc.Value value = this.value;
			if (value != UITextMisc.Value.GameVersion)
			{
				if (value == UITextMisc.Value.DungeonInfo)
				{
					string lengthString;
					int length;
					string dungeonLength;
					if (Level.current.options.TryGetValue(LevelOption.DungeonLength.Get(), out lengthString) && int.TryParse(lengthString, out length))
					{
						dungeonLength = length.ToString();
					}
					else
					{
						dungeonLength = "?";
					}
					base.textComponent.text = LocalizationManager.Instance.TryGetLocalization(this.textGroupId, this.text, new List<string>
					{
						Level.seed.ToString() + " (" + dungeonLength + ")",
						AreaManager.Instance.CurrentTreeSerialized
					}, false);
				}
			}
			else
			{
				base.textComponent.text = LocalizationManager.Instance.TryGetLocalization(this.textGroupId, this.text, new List<string>
				{
					GameSettings.GetVersionString(false)
				}, false);
			}
			if (this.forceUpperCase)
			{
				base.textComponent.text = base.textComponent.text.ToUpper();
			}
		}

		// Token: 0x0400293C RID: 10556
		public UITextMisc.Value value;

		// Token: 0x02000AAC RID: 2732
		public enum Value
		{
			// Token: 0x04004918 RID: 18712
			GameVersion,
			// Token: 0x04004919 RID: 18713
			DungeonInfo
		}
	}
}
