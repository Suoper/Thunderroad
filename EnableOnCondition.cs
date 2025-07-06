using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000339 RID: 825
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/EnableOnCondition")]
	[AddComponentMenu("ThunderRoad/Misc/Enable on condition")]
	public class EnableOnCondition : MonoBehaviour
	{
		// Token: 0x06002659 RID: 9817 RVA: 0x0010A614 File Offset: 0x00108814
		protected void Awake()
		{
			switch (this.condition)
			{
			case EnableOnCondition.Condition.OnPlay:
				this.EnableChildren();
				return;
			case EnableOnCondition.Condition.IsBuildRelease:
				if (!Debug.isDebugBuild)
				{
					this.EnableChildren();
					return;
				}
				break;
			case EnableOnCondition.Condition.OnRoomCulled:
				break;
			case EnableOnCondition.Condition.ContentFilterSetting:
				if (!ThunderRoadSettings.current.build.activeContent.HasFlag(this.contentFlag.AsRealFlag()))
				{
					this.EnableChildren();
					return;
				}
				break;
			case EnableOnCondition.Condition.LevelOption:
			{
				string text;
				if (Level.TryGetCurrentLevelOption(this.levelOption, out text))
				{
					this.EnableChildren();
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x0600265A RID: 9818 RVA: 0x0010A6A0 File Offset: 0x001088A0
		private void EnableChildren()
		{
			foreach (object obj in base.transform)
			{
				((Transform)obj).gameObject.SetActive(true);
			}
		}

		// Token: 0x0400261E RID: 9758
		public EnableOnCondition.Condition condition;

		// Token: 0x0400261F RID: 9759
		public BuildSettings.SingleContentFlag contentFlag;

		// Token: 0x04002620 RID: 9760
		public string levelOption = "ENABLE_IF_OPTION_EXISTS";

		// Token: 0x02000A23 RID: 2595
		public enum Condition
		{
			// Token: 0x0400472C RID: 18220
			OnPlay,
			// Token: 0x0400472D RID: 18221
			IsBuildRelease,
			// Token: 0x0400472E RID: 18222
			OnRoomCulled,
			// Token: 0x0400472F RID: 18223
			ContentFilterSetting,
			// Token: 0x04004730 RID: 18224
			LevelOption
		}
	}
}
