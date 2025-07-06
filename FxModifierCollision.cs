using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000295 RID: 661
	public class FxModifierCollision : ThunderBehaviour
	{
		// Token: 0x06001F1C RID: 7964 RVA: 0x000D4252 File Offset: 0x000D2452
		private void OnValidate()
		{
			if (this.fxController == null)
			{
				this.fxController = base.GetComponent<FxController>();
			}
			if (!base.GetComponent<Rigidbody>())
			{
				Debug.LogErrorFormat(this, "FxModifierCollision must be put on a rigidbody!", Array.Empty<object>());
			}
		}

		// Token: 0x06001F1D RID: 7965 RVA: 0x000D428C File Offset: 0x000D248C
		private void Awake()
		{
			if (this.fxController == null)
			{
				Debug.LogErrorFormat(this, "FxModifierCollision have no fxController set!", Array.Empty<object>());
				base.enabled = false;
			}
			if (!base.GetComponent<Rigidbody>())
			{
				Debug.LogErrorFormat(this, "FxModifierCollision must be put on a rigidbody!", Array.Empty<object>());
				base.enabled = false;
			}
		}

		// Token: 0x06001F1E RID: 7966 RVA: 0x000D42E4 File Offset: 0x000D24E4
		private void OnCollisionEnter(Collision collision)
		{
			if (collision.relativeVelocity.magnitude > this.velocityRange.x && Time.time - this.lastFxTime > this.minDelay)
			{
				ContactPoint contactPoint = collision.GetContact(0);
				if (this.colliders.Contains(contactPoint.thisCollider))
				{
					this.fxController.SetIntensity(Mathf.InverseLerp(this.velocityRange.x, this.velocityRange.y, collision.relativeVelocity.magnitude));
					this.fxController.Play();
					this.lastFxTime = Time.time;
				}
			}
		}

		// Token: 0x04001E1C RID: 7708
		public FxController fxController;

		// Token: 0x04001E1D RID: 7709
		public List<Collider> colliders;

		// Token: 0x04001E1E RID: 7710
		public FxModifierCollision.Link velocityLink;

		// Token: 0x04001E1F RID: 7711
		public Vector2 velocityRange = new Vector2(1f, 12f);

		// Token: 0x04001E20 RID: 7712
		public float minDelay = 0.5f;

		// Token: 0x04001E21 RID: 7713
		private float lastFxTime;

		// Token: 0x02000934 RID: 2356
		public enum Link
		{
			// Token: 0x04004401 RID: 17409
			None,
			// Token: 0x04004402 RID: 17410
			Intensity,
			// Token: 0x04004403 RID: 17411
			Speed
		}
	}
}
