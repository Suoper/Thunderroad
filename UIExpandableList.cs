using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace ThunderRoad
{
	// Token: 0x0200037C RID: 892
	[RequireComponent(typeof(UICustomisableButton))]
	[RequireComponent(typeof(TextMeshProUGUI))]
	public class UIExpandableList : MonoBehaviour
	{
		// Token: 0x14000142 RID: 322
		// (add) Token: 0x06002A6E RID: 10862 RVA: 0x0011F600 File Offset: 0x0011D800
		// (remove) Token: 0x06002A6F RID: 10863 RVA: 0x0011F638 File Offset: 0x0011D838
		public event UIExpandableList.ListStateChanged OnListStateChangedEvent;

		// Token: 0x17000292 RID: 658
		// (get) Token: 0x06002A70 RID: 10864 RVA: 0x0011F66D File Offset: 0x0011D86D
		// (set) Token: 0x06002A71 RID: 10865 RVA: 0x0011F675 File Offset: 0x0011D875
		public bool IsListCollapsed { get; private set; }

		// Token: 0x17000293 RID: 659
		// (get) Token: 0x06002A72 RID: 10866 RVA: 0x0011F67E File Offset: 0x0011D87E
		// (set) Token: 0x06002A73 RID: 10867 RVA: 0x0011F686 File Offset: 0x0011D886
		public List<GameObject> Elements { get; private set; }

		// Token: 0x06002A74 RID: 10868 RVA: 0x0011F690 File Offset: 0x0011D890
		public void Init(string category)
		{
			base.gameObject.name = category;
			this.text = base.GetComponent<TextMeshProUGUI>();
			this.text.text = category.ToUpperInvariant();
			this.uiText = base.GetComponent<UIText>();
			this.button = base.GetComponent<UICustomisableButton>();
			this.button.onPointerClick.AddListener(new UnityAction(this.ToggleListVisibility));
			this.Elements = new List<GameObject>();
		}

		// Token: 0x06002A75 RID: 10869 RVA: 0x0011F705 File Offset: 0x0011D905
		public void AddElement(GameObject newElement)
		{
			this.Elements.Add(newElement);
		}

		// Token: 0x06002A76 RID: 10870 RVA: 0x0011F713 File Offset: 0x0011D913
		public void ToggleListVisibility()
		{
			if (this.IsListCollapsed)
			{
				this.ExpandList();
			}
			else
			{
				this.CollapseList();
			}
			UIExpandableList.ListStateChanged onListStateChangedEvent = this.OnListStateChangedEvent;
			if (onListStateChangedEvent == null)
			{
				return;
			}
			onListStateChangedEvent(this);
		}

		// Token: 0x06002A77 RID: 10871 RVA: 0x0011F73C File Offset: 0x0011D93C
		public void CollapseList()
		{
			this.IsListCollapsed = true;
			foreach (GameObject gameObject in this.Elements)
			{
				gameObject.SetActive(false);
			}
		}

		// Token: 0x06002A78 RID: 10872 RVA: 0x0011F794 File Offset: 0x0011D994
		public void ExpandList()
		{
			this.IsListCollapsed = false;
			foreach (GameObject gameObject in this.Elements)
			{
				gameObject.SetActive(true);
			}
		}

		// Token: 0x06002A79 RID: 10873 RVA: 0x0011F7EC File Offset: 0x0011D9EC
		public void SetLocalizationIds(string groupId, string stringId)
		{
			if (this.uiText == null)
			{
				Debug.LogWarning("Trying to setup localization groupId and stringId, but no UIText component was found.");
				return;
			}
			this.uiText.SetLocalizationIds(groupId, stringId);
		}

		// Token: 0x04002835 RID: 10293
		private UICustomisableButton button;

		// Token: 0x04002836 RID: 10294
		private TextMeshProUGUI text;

		// Token: 0x04002837 RID: 10295
		private UIText uiText;

		// Token: 0x02000A92 RID: 2706
		// (Invoke) Token: 0x060046AE RID: 18094
		public delegate void ListStateChanged(UIExpandableList list);
	}
}
