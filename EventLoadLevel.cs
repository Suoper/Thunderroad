using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002C7 RID: 711
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Levels/EventLoadLevel.html")]
	public class EventLoadLevel : MonoBehaviour
	{
		// Token: 0x06002275 RID: 8821 RVA: 0x000ED884 File Offset: 0x000EBA84
		public void LoadLevel()
		{
			if (this.fadeInDuration > 0f)
			{
				CameraEffects.DoFadeEffect(true, this.fadeInDuration);
				base.Invoke("LoadLevelNow", this.fadeInDuration);
				return;
			}
			this.LoadLevelNow();
		}

		// Token: 0x06002276 RID: 8822 RVA: 0x000ED8B7 File Offset: 0x000EBAB7
		public void LoadLevel(LoadingCamera.State loadingUIState)
		{
			this.loadingUIState = loadingUIState;
			this.LoadLevel();
		}

		// Token: 0x06002277 RID: 8823 RVA: 0x000ED8C6 File Offset: 0x000EBAC6
		public void LoadLevel(string levelId)
		{
			this.levelId = levelId;
			if (this.fadeInDuration > 0f)
			{
				CameraEffects.DoFadeEffect(true, this.fadeInDuration);
				base.Invoke("LoadLevelNow", this.fadeInDuration);
				return;
			}
			this.LoadLevelNow();
		}

		// Token: 0x06002278 RID: 8824 RVA: 0x000ED900 File Offset: 0x000EBB00
		public void LoadLevel(string levelId, LoadingCamera.State loadingUIState)
		{
			this.levelId = levelId;
			this.loadingUIState = loadingUIState;
			this.LoadLevel();
		}

		// Token: 0x06002279 RID: 8825 RVA: 0x000ED918 File Offset: 0x000EBB18
		protected void LoadLevelNow()
		{
			Dictionary<string, string> dicLevelOptions = new Dictionary<string, string>();
			foreach (EventLoadLevel.LevelOption levelOption in this.levelOptions)
			{
				dicLevelOptions.Add(levelOption.name, levelOption.value);
			}
			LevelManager.LoadLevel(this.levelId, this.modeName, dicLevelOptions, this.loadingUIState);
		}

		// Token: 0x04002192 RID: 8594
		public string levelId;

		// Token: 0x04002193 RID: 8595
		public string modeName;

		// Token: 0x04002194 RID: 8596
		public List<EventLoadLevel.LevelOption> levelOptions;

		// Token: 0x04002195 RID: 8597
		public float fadeInDuration = 2f;

		// Token: 0x04002196 RID: 8598
		public LoadingCamera.State loadingUIState = LoadingCamera.State.Enabled;

		// Token: 0x020009A8 RID: 2472
		[Serializable]
		public class LevelOption
		{
			// Token: 0x04004567 RID: 17767
			public string name;

			// Token: 0x04004568 RID: 17768
			public string value;
		}
	}
}
