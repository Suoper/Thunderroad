using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000338 RID: 824
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/DisableOnPlatform.html")]
	[AddComponentMenu("ThunderRoad/Misc/Disable or Strip GameObject relative to platform")]
	public class DisableOnPlatform : MonoBehaviour
	{
		// Token: 0x06002656 RID: 9814 RVA: 0x0010A51C File Offset: 0x0010871C
		protected void Start()
		{
			if (this.platformFilter == DisableOnPlatform.PlatformFilter.OnlyOn)
			{
				if (Common.GetQualityLevel(false) == this.platform && (!this.checkControllerType || PlayerControl.controllerDiagram == this.controllerType))
				{
					base.gameObject.SetActive(false);
					return;
				}
			}
			else if (this.platformFilter == DisableOnPlatform.PlatformFilter.ExcludeOn && (Common.GetQualityLevel(false) != this.platform || (this.checkControllerType && PlayerControl.controllerDiagram != this.controllerType)))
			{
				base.gameObject.SetActive(false);
			}
		}

		// Token: 0x06002657 RID: 9815 RVA: 0x0010A59C File Offset: 0x0010879C
		public bool ConcernQualityLevel(QualityLevel qualityLevel)
		{
			if (this.platformFilter == DisableOnPlatform.PlatformFilter.OnlyOn)
			{
				if (qualityLevel == this.platform && (!this.checkControllerType || PlayerControl.controllerDiagram == this.controllerType))
				{
					return true;
				}
			}
			else if (this.platformFilter == DisableOnPlatform.PlatformFilter.ExcludeOn && (qualityLevel != this.platform || this.checkControllerType || PlayerControl.controllerDiagram == this.controllerType))
			{
				return true;
			}
			return false;
		}

		// Token: 0x04002619 RID: 9753
		public DisableOnPlatform.PlatformFilter platformFilter = DisableOnPlatform.PlatformFilter.OnlyOn;

		// Token: 0x0400261A RID: 9754
		public QualityLevel platform;

		// Token: 0x0400261B RID: 9755
		public PlayerControl.ControllerDiagram controllerType = PlayerControl.ControllerDiagram.WMR;

		// Token: 0x0400261C RID: 9756
		[Tooltip("Check the controller type to strip for as well?")]
		public bool checkControllerType;

		// Token: 0x0400261D RID: 9757
		[Tooltip("Strip can happen when generating platform specific scene or room")]
		public bool allowStrip;

		// Token: 0x02000A22 RID: 2594
		public enum PlatformFilter
		{
			// Token: 0x04004729 RID: 18217
			OnlyOn = 1,
			// Token: 0x0400472A RID: 18218
			ExcludeOn
		}
	}
}
