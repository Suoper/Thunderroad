using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000297 RID: 663
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModule.html")]
	public class FxModule : ThunderBehaviour
	{
		// Token: 0x06001F25 RID: 7973 RVA: 0x000D465C File Offset: 0x000D285C
		public virtual void Play()
		{
			if (this.useControllerDirection)
			{
				if (this.controller.direction != Vector3.zero)
				{
					base.transform.rotation = Quaternion.LookRotation(this.controller.direction, this.controller.transform.forward);
					return;
				}
				base.transform.localRotation = Quaternion.identity;
			}
		}

		// Token: 0x06001F26 RID: 7974 RVA: 0x000D46C4 File Offset: 0x000D28C4
		public virtual void SetIntensity(float intensity)
		{
		}

		// Token: 0x06001F27 RID: 7975 RVA: 0x000D46C6 File Offset: 0x000D28C6
		public virtual void SetSpeed(float speed)
		{
		}

		// Token: 0x06001F28 RID: 7976 RVA: 0x000D46C8 File Offset: 0x000D28C8
		public virtual void Stop(bool playStopEffect = true)
		{
		}

		// Token: 0x06001F29 RID: 7977 RVA: 0x000D46CA File Offset: 0x000D28CA
		public virtual bool IsPlaying()
		{
			return false;
		}

		// Token: 0x04001E2D RID: 7725
		public bool useControllerDirection;

		// Token: 0x04001E2E RID: 7726
		[NonSerialized]
		public FxController controller;

		// Token: 0x02000936 RID: 2358
		public enum Link
		{
			// Token: 0x04004409 RID: 17417
			None,
			// Token: 0x0400440A RID: 17418
			Intensity,
			// Token: 0x0400440B RID: 17419
			Speed
		}
	}
}
