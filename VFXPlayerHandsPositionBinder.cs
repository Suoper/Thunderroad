using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace ThunderRoad
{
	// Token: 0x020002A6 RID: 678
	[VFXBinder("Custom/Player Hands Position")]
	internal class VFXPlayerHandsPositionBinder : VFXBinderBase
	{
		// Token: 0x06001F90 RID: 8080 RVA: 0x000D6B9C File Offset: 0x000D4D9C
		public override bool IsValid(VisualEffect component)
		{
			if (!Application.isPlaying)
			{
				return component.HasVector3(this.targetProperty);
			}
			if (!Player.local)
			{
				return false;
			}
			PlayerHand hand = Player.local.GetHand(this.handSide);
			return hand && hand != null && component.HasVector3(this.targetProperty);
		}

		// Token: 0x06001F91 RID: 8081 RVA: 0x000D6C08 File Offset: 0x000D4E08
		public override void UpdateBinding(VisualEffect component)
		{
			if (!Application.isPlaying)
			{
				return;
			}
			if (!Player.local)
			{
				component.SetVector3(this.targetProperty, base.transform.position);
				return;
			}
			PlayerHand hand = Player.local.GetHand(this.handSide);
			if (!hand)
			{
				component.SetVector3(this.targetProperty, base.transform.position);
				return;
			}
			component.SetVector3(this.targetProperty, hand.transform.position);
		}

		// Token: 0x06001F92 RID: 8082 RVA: 0x000D6C98 File Offset: 0x000D4E98
		public override string ToString()
		{
			return string.Format("Player hand postion : '{0}' -> {1}", this.targetProperty, this.handSide);
		}

		// Token: 0x04001EBE RID: 7870
		[VFXPropertyBinding(new string[]
		{
			"UnityEngine.Vector3"
		})]
		public ExposedProperty targetProperty = "Left Hand Position";

		// Token: 0x04001EBF RID: 7871
		public Side handSide = Side.Left;
	}
}
