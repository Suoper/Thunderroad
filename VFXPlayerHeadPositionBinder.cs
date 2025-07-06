using System;
using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.VFX.Utility;

namespace ThunderRoad
{
	// Token: 0x020002A7 RID: 679
	[VFXBinder("Custom/Player Head Position")]
	internal class VFXPlayerHeadPositionBinder : VFXBinderBase
	{
		// Token: 0x06001F94 RID: 8084 RVA: 0x000D6CD4 File Offset: 0x000D4ED4
		public override bool IsValid(VisualEffect component)
		{
			if (!Application.isPlaying)
			{
				return component.HasVector3(this.targetProperty);
			}
			return Player.local && component.HasVector3(this.targetProperty);
		}

		// Token: 0x06001F95 RID: 8085 RVA: 0x000D6D10 File Offset: 0x000D4F10
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
			component.SetVector3(this.targetProperty, Player.local.head.transform.position);
		}

		// Token: 0x06001F96 RID: 8086 RVA: 0x000D6D73 File Offset: 0x000D4F73
		public override string ToString()
		{
			return string.Format("Player head position : '{0}' -> Player Head", this.targetProperty);
		}

		// Token: 0x04001EC0 RID: 7872
		[VFXPropertyBinding(new string[]
		{
			"UnityEngine.Vector3"
		})]
		public ExposedProperty targetProperty = "Head Position";
	}
}
