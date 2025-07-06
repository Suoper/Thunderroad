using System;
using System.Collections.Generic;
using ThunderRoad.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ThunderRoad
{
	// Token: 0x02000391 RID: 913
	public class UISelectionListButtonsMapSelection : UISelectionListButtons
	{
		// Token: 0x06002B88 RID: 11144 RVA: 0x00125DE6 File Offset: 0x00123FE6
		protected override void Awake()
		{
			base.Awake();
			this.currentValue = 0;
			this.minValue = 0;
			if (this.value != null)
			{
				this.value.text = "";
			}
			this.maxValue = 0;
		}

		// Token: 0x06002B89 RID: 11145 RVA: 0x00125E21 File Offset: 0x00124021
		private void Start()
		{
			this.LoadValue();
			this.OnUpdateValue(false);
		}

		// Token: 0x06002B8A RID: 11146 RVA: 0x00125E30 File Offset: 0x00124030
		private void OnDestroy()
		{
			GameManager.onDevModeActivated = (Action)Delegate.Remove(GameManager.onDevModeActivated, new Action(this.LoadValue));
		}

		// Token: 0x06002B8B RID: 11147 RVA: 0x00125E54 File Offset: 0x00124054
		public override void LoadValue()
		{
			if (this.allLevels.Count == 0)
			{
				LevelInstancesModule levelInstancesModule;
				if (GameModeManager.instance.currentGameMode == null || !GameModeManager.instance.currentGameMode.TryGetModule<LevelInstancesModule>(out levelInstancesModule))
				{
					Debug.Log("Current game mode does not have a valid LevelInstancesModule. Reverting to default");
					levelInstancesModule = new LevelInstancesModule();
				}
				List<LevelInstance> allLevelInstances = levelInstancesModule.LevelInstances;
				for (int i = 0; i < allLevelInstances.Count; i++)
				{
					LevelInstance levelInstance = allLevelInstances[i];
					LevelData levelData = levelInstance.LevelData;
					if ((!levelData.showOnlyDevMode || GameManager.DevMode) && levelData.showInLevelSelection)
					{
						this.allLevels.Add(levelInstance);
					}
				}
			}
			this.levelCount.text = (this.currentValue + 1).ToString() + " / " + this.allLevels.Count.ToString();
			this.maxValue = this.allLevels.Count - 1;
			this.UISelectionLevelMode.gameObject.SetActive(true);
			this.UISelectionLevelMode.UpdateValues(this.allLevels[0]);
			GameManager.onDevModeActivated = (Action)Delegate.Remove(GameManager.onDevModeActivated, new Action(this.LoadValue));
			GameManager.onDevModeActivated = (Action)Delegate.Combine(GameManager.onDevModeActivated, new Action(this.LoadValue));
		}

		// Token: 0x06002B8C RID: 11148 RVA: 0x00125FA1 File Offset: 0x001241A1
		public void OnAnimatorIK(int layerIndex)
		{
		}

		// Token: 0x06002B8D RID: 11149 RVA: 0x00125FA3 File Offset: 0x001241A3
		public LevelInstance GetCurrentLevel()
		{
			return this.allLevels[this.currentValue];
		}

		// Token: 0x06002B8E RID: 11150 RVA: 0x00125FB6 File Offset: 0x001241B6
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
		}

		// Token: 0x06002B8F RID: 11151 RVA: 0x00125FC8 File Offset: 0x001241C8
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			LevelInstance current = this.GetCurrentLevel();
			LevelData levelData = current.LevelData;
			if (this.value != null)
			{
				this.value.text = LocalizationManager.Instance.TryGetLocalization("Default", levelData.name, null, false);
			}
			this.levelDescription.text = LocalizationManager.Instance.TryGetLocalization("Default", levelData.description, null, false);
			this.levelImage.texture = ((levelData != null) ? levelData.mapPreviewImage.texture : null);
			this.levelCount.text = (this.currentValue + 1).ToString() + " / " + this.allLevels.Count.ToString();
			this.UISelectionLevelMode.gameObject.SetActive(true);
			this.UISelectionLevelMode.UpdateValues(current);
		}

		// Token: 0x04002919 RID: 10521
		public RawImage levelImage;

		// Token: 0x0400291A RID: 10522
		public TextMeshProUGUI levelDescription;

		// Token: 0x0400291B RID: 10523
		public TextMeshProUGUI levelCount;

		// Token: 0x0400291C RID: 10524
		public UIMapLevelMode UISelectionLevelMode;

		// Token: 0x0400291D RID: 10525
		private List<LevelInstance> allLevels = new List<LevelInstance>();
	}
}
