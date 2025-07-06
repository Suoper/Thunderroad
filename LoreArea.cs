using System;
using System.Collections.Generic;
using System.Linq;
using ThunderRoad.Modules;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002DF RID: 735
	public class LoreArea : MonoBehaviour
	{
		// Token: 0x06002355 RID: 9045 RVA: 0x000F19C3 File Offset: 0x000EFBC3
		private void Awake()
		{
			EventManager.onLevelLoad += this.HandleLevelLoaded;
		}

		// Token: 0x06002356 RID: 9046 RVA: 0x000F19D6 File Offset: 0x000EFBD6
		private void OnDestroy()
		{
			EventManager.onLevelLoad -= this.HandleLevelLoaded;
		}

		// Token: 0x06002357 RID: 9047 RVA: 0x000F19EC File Offset: 0x000EFBEC
		private void Start()
		{
			foreach (LoreSpawner lore in base.GetComponentsInChildren<LoreSpawner>(true).ToList<LoreSpawner>())
			{
				if (lore.gameObject.activeSelf)
				{
					this.loreSpawners.Add(lore);
				}
			}
			for (int i = this.loreSpawners.Count - 1; i >= 0; i--)
			{
				LoreSpawner loreSpawner = this.loreSpawners[i];
				loreSpawner.loreArea = this;
				if (!loreSpawner.enabled)
				{
					this.loreSpawners.RemoveAt(i);
				}
			}
			if (!(GameModeManager.instance == null) && GameModeManager.instance.currentGameMode != null)
			{
				GameModeManager.instance.currentGameMode.TryGetModule<LoreModule>(out this.loreModule);
			}
		}

		// Token: 0x06002358 RID: 9048 RVA: 0x000F1AC4 File Offset: 0x000EFCC4
		public void HandleLevelLoaded(LevelData levelData, LevelData.Mode mode, EventTime eventTime)
		{
			if (eventTime == EventTime.OnStart)
			{
				this.loreSpawners.Clear();
				return;
			}
			if (eventTime == EventTime.OnEnd)
			{
				UnityEngine.Object.FindObjectOfType<Level>();
				this.SetLevelId(Level.current.data.id);
				this.SetLoreSpawnerRoomId();
			}
		}

		// Token: 0x06002359 RID: 9049 RVA: 0x000F1AFC File Offset: 0x000EFCFC
		public void SetLevelId(string levelId)
		{
			this.levelID = levelId;
			int spawnCount = this.loreSpawners.Count;
			if (!string.IsNullOrEmpty(levelId))
			{
				for (int i = 0; i < spawnCount; i++)
				{
					if (!this.loreSpawners[i].loreConditionOptionalParameters.Contains(this.levelID))
					{
						this.loreSpawners[i].loreConditionOptionalParameters.Add(levelId);
					}
				}
			}
		}

		// Token: 0x0600235A RID: 9050 RVA: 0x000F1B65 File Offset: 0x000EFD65
		public void SetRoomId(string roomId)
		{
			this.roomID = roomId;
			this.SetLoreSpawnerRoomId();
		}

		// Token: 0x0600235B RID: 9051 RVA: 0x000F1B74 File Offset: 0x000EFD74
		private void SetLoreSpawnerRoomId()
		{
			int spawnCount = this.loreSpawners.Count;
			if (!string.IsNullOrEmpty(this.roomID))
			{
				for (int i = 0; i < spawnCount; i++)
				{
					this.loreSpawners[i].loreConditionOptionalParameters.Add(this.roomID);
				}
			}
		}

		// Token: 0x0600235C RID: 9052 RVA: 0x000F1BC2 File Offset: 0x000EFDC2
		public void Debug_MarkAllLoreUnread()
		{
			this.loreModule.Debug_MarkAllLoreUnread();
		}

		// Token: 0x0600235D RID: 9053 RVA: 0x000F1BCF File Offset: 0x000EFDCF
		public void Debug_MarkAllLoreRead()
		{
			this.loreModule.Debug_MarkAllLoreRead();
		}

		// Token: 0x04002261 RID: 8801
		public List<LoreSpawner> loreSpawners = new List<LoreSpawner>();

		// Token: 0x04002262 RID: 8802
		public string levelID = "";

		// Token: 0x04002263 RID: 8803
		public string roomID = "";

		// Token: 0x04002264 RID: 8804
		private LoreModule loreModule;
	}
}
