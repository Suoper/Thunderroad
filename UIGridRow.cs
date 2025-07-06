using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x0200037D RID: 893
	public class UIGridRow : MonoBehaviour
	{
		// Token: 0x17000294 RID: 660
		// (get) Token: 0x06002A7B RID: 10875 RVA: 0x0011F81C File Offset: 0x0011DA1C
		// (set) Token: 0x06002A7C RID: 10876 RVA: 0x0011F824 File Offset: 0x0011DA24
		public Canvas Canvas { get; private set; }

		// Token: 0x17000295 RID: 661
		// (get) Token: 0x06002A7D RID: 10877 RVA: 0x0011F82D File Offset: 0x0011DA2D
		// (set) Token: 0x06002A7E RID: 10878 RVA: 0x0011F835 File Offset: 0x0011DA35
		public GraphicRaycaster GraphicRaycaster { get; private set; }

		// Token: 0x17000296 RID: 662
		// (get) Token: 0x06002A7F RID: 10879 RVA: 0x0011F83E File Offset: 0x0011DA3E
		// (set) Token: 0x06002A80 RID: 10880 RVA: 0x0011F846 File Offset: 0x0011DA46
		public List<RaycastTarget> RaycastableGraphics { get; private set; }

		// Token: 0x17000297 RID: 663
		// (get) Token: 0x06002A81 RID: 10881 RVA: 0x0011F84F File Offset: 0x0011DA4F
		// (set) Token: 0x06002A82 RID: 10882 RVA: 0x0011F857 File Offset: 0x0011DA57
		public Transform CachedTransform { get; private set; }

		// Token: 0x06002A83 RID: 10883 RVA: 0x0011F860 File Offset: 0x0011DA60
		private void Awake()
		{
			this.Canvas = base.GetComponent<Canvas>();
			this.GraphicRaycaster = base.GetComponent<GraphicRaycaster>();
			this.CachedTransform = base.transform;
			this.RaycastableGraphics = new List<RaycastTarget>();
		}

		// Token: 0x06002A84 RID: 10884 RVA: 0x0011F894 File Offset: 0x0011DA94
		public void Setup(int columns)
		{
			this.maxColumns = columns;
			this.elements = 0;
			if (this.RaycastableGraphics == null)
			{
				this.RaycastableGraphics = new List<RaycastTarget>();
			}
			this.RaycastableGraphics.Clear();
		}

		// Token: 0x06002A85 RID: 10885 RVA: 0x0011F8CF File Offset: 0x0011DACF
		public bool IsRowFull()
		{
			return this.elements == this.maxColumns;
		}

		/// <summary>
		/// Add a new element to this row
		/// </summary>
		/// <param name="element">Element to add</param>
		/// <returns>Return true if the element was added and return false otherwise.</returns>
		// Token: 0x06002A86 RID: 10886 RVA: 0x0011F8E0 File Offset: 0x0011DAE0
		public void AddElement(GameObject element)
		{
			if (element == null || this.elements == this.maxColumns)
			{
				return;
			}
			element.transform.SetParent(this.CachedTransform);
			this.elements++;
			foreach (RaycastTarget graphic in element.GetComponentsInChildren<RaycastTarget>())
			{
				this.RaycastableGraphics.Add(graphic);
			}
			this.ToggleRaycastTargets(this.componentsEnabled);
		}

		// Token: 0x06002A87 RID: 10887 RVA: 0x0011F958 File Offset: 0x0011DB58
		public void ToggleComponents(bool active)
		{
			if (this.componentsEnabled != active)
			{
				this.componentsEnabled = active;
				if (this.Canvas)
				{
					this.Canvas.enabled = active;
				}
				if (this.GraphicRaycaster)
				{
					this.GraphicRaycaster.enabled = active;
				}
			}
			this.ToggleRaycastTargets(active);
		}

		// Token: 0x06002A88 RID: 10888 RVA: 0x0011F9B0 File Offset: 0x0011DBB0
		public void ToggleRaycastTargets(bool active)
		{
			if (this.RaycastableGraphics.IsNullOrEmpty())
			{
				return;
			}
			int raycastableGraphicsCount = this.RaycastableGraphics.Count;
			for (int i = 0; i < raycastableGraphicsCount; i++)
			{
				this.RaycastableGraphics[i].raycastTarget = active;
			}
		}

		// Token: 0x0400283A RID: 10298
		private int maxColumns;

		// Token: 0x0400283B RID: 10299
		private int elements;

		// Token: 0x0400283C RID: 10300
		private bool componentsEnabled = true;

		// Token: 0x0400283D RID: 10301
		private bool raycastTargetEnabled = true;
	}
}
