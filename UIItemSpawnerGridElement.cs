using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000381 RID: 897
	public class UIItemSpawnerGridElement : MonoBehaviour
	{
		// Token: 0x17000299 RID: 665
		// (get) Token: 0x06002AD5 RID: 10965 RVA: 0x00121D9E File Offset: 0x0011FF9E
		// (set) Token: 0x06002AD6 RID: 10966 RVA: 0x00121DA6 File Offset: 0x0011FFA6
		public UICustomisableButton Button { get; private set; }

		// Token: 0x06002AD7 RID: 10967 RVA: 0x00121DAF File Offset: 0x0011FFAF
		protected void Awake()
		{
			this.Button = base.GetComponent<UICustomisableButton>();
		}

		// Token: 0x06002AD8 RID: 10968 RVA: 0x00121DBD File Offset: 0x0011FFBD
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
			this.SetLocalizedFields();
		}

		// Token: 0x06002AD9 RID: 10969 RVA: 0x00121DD6 File Offset: 0x0011FFD6
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002ADA RID: 10970 RVA: 0x00121DE9 File Offset: 0x0011FFE9
		private void OnLanguageChanged(string language)
		{
			this.SetLocalizedFields();
		}

		// Token: 0x06002ADB RID: 10971 RVA: 0x00121DF1 File Offset: 0x0011FFF1
		protected virtual void SetLocalizedFields()
		{
		}

		// Token: 0x04002892 RID: 10386
		[SerializeField]
		protected TextMeshProUGUI elementName;

		// Token: 0x04002893 RID: 10387
		[SerializeField]
		protected Image iconSprite;
	}
}
