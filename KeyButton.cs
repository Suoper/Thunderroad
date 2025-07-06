using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x020001D4 RID: 468
	[RequireComponent(typeof(BoxCollider))]
	public class KeyButton : ThunderBehaviour
	{
		// Token: 0x0600151B RID: 5403 RVA: 0x00093940 File Offset: 0x00091B40
		private void OnValidate()
		{
			Canvas foundCanvas;
			if (base.gameObject.TryGetOrAddComponentInChildren(out foundCanvas))
			{
				this.canvas = foundCanvas;
				foundCanvas.name = "Canvas";
				Image image;
				if (!foundCanvas.TryGetComponent<Image>(out image))
				{
					image = this.canvas.gameObject.AddComponent<Image>();
					image.color = Color.clear;
				}
			}
			TextMeshPro foundTextMesh;
			if (base.gameObject.TryGetOrAddComponentInChildren(out foundTextMesh))
			{
				this.textMesh = foundTextMesh;
				this.textMesh.name = "TextMeshPro";
			}
			if (string.IsNullOrEmpty(this.keyId))
			{
				Debug.LogError("Keyboard Key " + base.name + " does not have an ID set. Please set an ID and map it to a valid KeyboardData", this);
			}
		}

		// Token: 0x0400150A RID: 5386
		public string keyId;

		// Token: 0x0400150B RID: 5387
		public Canvas canvas;

		// Token: 0x0400150C RID: 5388
		public TextMeshPro textMesh;
	}
}
