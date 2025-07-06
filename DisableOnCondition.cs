using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000337 RID: 823
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/DisableOnCondition.html")]
	[AddComponentMenu("ThunderRoad/Misc/Disable on condition")]
	public class DisableOnCondition : MonoBehaviour
	{
		// Token: 0x06002654 RID: 9812 RVA: 0x0010A464 File Offset: 0x00108664
		protected void OnEnable()
		{
			switch (this.condition)
			{
			case DisableOnCondition.Condition.OnPlay:
				base.gameObject.SetActive(false);
				return;
			case DisableOnCondition.Condition.IsBuildRelease:
				if (!Debug.isDebugBuild)
				{
					base.gameObject.SetActive(false);
					return;
				}
				break;
			case DisableOnCondition.Condition.OnRoomCulled:
				break;
			case DisableOnCondition.Condition.ContentFilterSetting:
				if (!ThunderRoadSettings.current.build.activeContent.HasFlag(this.contentFlag.AsRealFlag()))
				{
					base.gameObject.SetActive(false);
					return;
				}
				break;
			case DisableOnCondition.Condition.LevelOption:
			{
				string text;
				if (Level.TryGetCurrentLevelOption(this.levelOption, out text))
				{
					base.gameObject.SetActive(false);
				}
				break;
			}
			default:
				return;
			}
		}

		// Token: 0x04002616 RID: 9750
		public DisableOnCondition.Condition condition;

		// Token: 0x04002617 RID: 9751
		[Tooltip("Specifies the content flag for the ContentFilterSetting Condition.\n\nSettings such as Blood will make it so this gameObject will be disabled if Gore is disabled (but only if the Condition is ContentFilterSettings).")]
		public BuildSettings.SingleContentFlag contentFlag;

		// Token: 0x04002618 RID: 9752
		[Tooltip("Specifies the levelOption of which this gameObject is disabled if the Condition is set to LevelOption.\n\nDISABLE_IF_OPTION_EXISTS is a placeholder level option, and does not exist in any level.")]
		public string levelOption = "DISABLE_IF_OPTION_EXISTS";

		// Token: 0x02000A21 RID: 2593
		[Tooltip("Specifies the point that this gameObject should be disabled.\n\nOnPlay: Disables when the game is played.\nIsBuildReleased: Disables when the build is ran.\nOnRoomCulled: Disables when the Dungeon room this gameobject resides in is culled.\nContentFilterSetting: Disabled based on the content filter (like gore being disabled).\nLevelOption: Disabled based on what LevelOption is selected in the Level (e.g. Survival).")]
		public enum Condition
		{
			// Token: 0x04004723 RID: 18211
			OnPlay,
			// Token: 0x04004724 RID: 18212
			IsBuildRelease,
			// Token: 0x04004725 RID: 18213
			OnRoomCulled,
			// Token: 0x04004726 RID: 18214
			ContentFilterSetting,
			// Token: 0x04004727 RID: 18215
			LevelOption
		}
	}
}
