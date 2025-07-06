using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002A8 RID: 680
	[CreateAssetMenu(menuName = "ThunderRoad/Config/Game Settings")]
	public class GameSettings : ScriptableObject
	{
		// Token: 0x170001ED RID: 493
		// (get) Token: 0x06001F98 RID: 8088 RVA: 0x000D6D9D File Offset: 0x000D4F9D
		// (set) Token: 0x06001F99 RID: 8089 RVA: 0x000D6DC5 File Offset: 0x000D4FC5
		public static List<string> loadDefaultFolders
		{
			get
			{
				if (Application.isEditor)
				{
					return ThunderRoadSettings.current.game.editorLoadDefaultFolders;
				}
				return ThunderRoadSettings.current.game.buildLoadDefaultFolders;
			}
			set
			{
				if (Application.isEditor)
				{
					ThunderRoadSettings.current.game.editorLoadDefaultFolders = value;
					return;
				}
				ThunderRoadSettings.current.game.buildLoadDefaultFolders = value;
			}
		}

		// Token: 0x06001F9A RID: 8090 RVA: 0x000D6DEF File Offset: 0x000D4FEF
		public static void LoadAllJson()
		{
			Catalog.EditorLoadAllJson(true, true, true);
		}

		// Token: 0x06001F9B RID: 8091 RVA: 0x000D6DFC File Offset: 0x000D4FFC
		public static string GetVersionString(bool stripBuild = false)
		{
			if (stripBuild || ThunderRoadSettings.current.game.versionBuild == 0)
			{
				return string.Format("{0}.{1}.{2}", ThunderRoadSettings.current.game.versionMajor, ThunderRoadSettings.current.game.versionMinor, ThunderRoadSettings.current.game.versionRevision);
			}
			return string.Format("{0}.{1}.{2}.{3}", new object[]
			{
				ThunderRoadSettings.current.game.versionMajor,
				ThunderRoadSettings.current.game.versionMinor,
				ThunderRoadSettings.current.game.versionRevision,
				ThunderRoadSettings.current.game.versionBuild
			});
		}

		// Token: 0x06001F9C RID: 8092 RVA: 0x000D6ED0 File Offset: 0x000D50D0
		public List<ValueDropdownItem<string>> GetJsonFolders()
		{
			List<ValueDropdownItem<string>> dropdownList = new List<ValueDropdownItem<string>>();
			foreach (string folderName in FileManager.GetFolderNames(FileManager.Type.JSONCatalog, FileManager.Source.Default, ""))
			{
				if (!(folderName == ".git"))
				{
					dropdownList.Add(new ValueDropdownItem<string>(folderName, folderName));
				}
			}
			return dropdownList;
		}

		// Token: 0x04001EC1 RID: 7873
		public int versionMajor;

		// Token: 0x04001EC2 RID: 7874
		public int versionMinor;

		// Token: 0x04001EC3 RID: 7875
		public int versionRevision;

		// Token: 0x04001EC4 RID: 7876
		public int versionBuild;

		// Token: 0x04001EC5 RID: 7877
		public string minModVersion;

		// Token: 0x04001EC6 RID: 7878
		public string productName;

		// Token: 0x04001EC7 RID: 7879
		public string appIdentifier;

		// Token: 0x04001EC8 RID: 7880
		public string exeDescription;

		// Token: 0x04001EC9 RID: 7881
		[Multiline]
		public string androidModReadme;

		// Token: 0x04001ECA RID: 7882
		public Texture2D icon;

		// Token: 0x04001ECB RID: 7883
		public string mainMenuLevelId;

		// Token: 0x04001ECC RID: 7884
		public string defaultGameModeId;

		// Token: 0x04001ECD RID: 7885
		public string pointerAddress = "Ui.Pointer";

		// Token: 0x04001ECE RID: 7886
		public string highlighterAddress = "Ui.Highlighter";

		// Token: 0x04001ECF RID: 7887
		public string optionsMenuAddress = "Ui.OptionsMenu";

		// Token: 0x04001ED0 RID: 7888
		public string loadingBarAddress = "Ui.LoadingBar";

		// Token: 0x04001ED1 RID: 7889
		public Player playerPrefab;

		// Token: 0x04001ED2 RID: 7890
		public Spectator spectatorPrefab;

		// Token: 0x04001ED3 RID: 7891
		public string codecksReportToken;

		// Token: 0x04001ED4 RID: 7892
		public List<string> buildLoadDefaultFolders;

		// Token: 0x04001ED5 RID: 7893
		public List<string> editorLoadDefaultFolders;
	}
}
