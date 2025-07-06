using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace ThunderRoad
{
	// Token: 0x0200033C RID: 828
	public class FaceCam : MonoBehaviour
	{
		// Token: 0x060026D0 RID: 9936 RVA: 0x0010BF24 File Offset: 0x0010A124
		private void Awake()
		{
			this.cachedTransform = base.transform;
			if (this.fadeRenderer == null)
			{
				this.fadeRenderer = base.GetComponent<Renderer>();
			}
		}

		// Token: 0x060026D1 RID: 9937 RVA: 0x0010BF48 File Offset: 0x0010A148
		private void Update()
		{
			if (this.mainCameraTransform == null)
			{
				if (Camera.main != null)
				{
					this.mainCameraTransform = Camera.main.transform;
				}
			}
			else
			{
				this.cachedTransform.LookAt(2f * this.cachedTransform.position - this.mainCameraTransform.position, this.useWorldUp ? Vector3.up : this.mainCameraTransform.up);
			}
			if (!this.mainCameraTransform || !this.fadeOut || !this.fadeRenderer)
			{
				return;
			}
			float alpha = Mathf.InverseLerp(this.fadeOutDistance.y, this.fadeOutDistance.x, Vector3.Distance(base.transform.position, this.mainCameraTransform.position));
			Renderer renderer = this.fadeRenderer;
			SpriteRenderer sprite = renderer as SpriteRenderer;
			if (sprite != null)
			{
				sprite.color = new Color(sprite.color.r, sprite.color.g, sprite.color.b, alpha);
				return;
			}
			MeshRenderer mesh = renderer as MeshRenderer;
			if (mesh == null)
			{
				return;
			}
			mesh.material.color = new Color(mesh.material.color.r, mesh.material.color.g, mesh.material.color.b, alpha);
		}

		// Token: 0x04002621 RID: 9761
		public bool useWorldUp;

		// Token: 0x04002622 RID: 9762
		public bool fadeOut;

		// Token: 0x04002623 RID: 9763
		public Vector2 fadeOutDistance = new Vector2(0.5f, 1.5f);

		// Token: 0x04002624 RID: 9764
		[FormerlySerializedAs("targetRenderer")]
		[FormerlySerializedAs("renderer")]
		public Renderer fadeRenderer;

		// Token: 0x04002625 RID: 9765
		private Transform mainCameraTransform;

		// Token: 0x04002626 RID: 9766
		private Transform cachedTransform;
	}
}
