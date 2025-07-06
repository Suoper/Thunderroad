using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace ThunderRoad
{
	// Token: 0x020001FE RID: 510
	[Serializable]
	public class Music : CatalogData
	{
		// Token: 0x0600161B RID: 5659 RVA: 0x0009875A File Offset: 0x0009695A
		public List<ValueDropdownItem<string>> GetAllMusicGroupID()
		{
			return Catalog.GetDropdownAllID(Category.MusicGroup, "None");
		}

		// Token: 0x0600161C RID: 5660 RVA: 0x00098768 File Offset: 0x00096968
		public override void OnCatalogRefresh()
		{
			if (this.loadedMusicGroup == null)
			{
				this.loadedMusicGroup = new List<MusicGroup>();
			}
			this.loadedMusicGroup.Clear();
			int count = this.groupsToLoad.Count;
			for (int i = 0; i < count; i++)
			{
				this.loadedMusicGroup.Add(Catalog.GetData<MusicGroup>(this.groupsToLoad[i], true));
			}
		}

		// Token: 0x040015BF RID: 5567
		[NonSerialized]
		public List<MusicGroup> loadedMusicGroup;

		// Token: 0x040015C0 RID: 5568
		public float volumeDb;

		// Token: 0x040015C1 RID: 5569
		public List<string> groupsToLoad;

		// Token: 0x040015C2 RID: 5570
		public List<Music.MusicTransition> transitions;

		// Token: 0x040015C3 RID: 5571
		public MusicDynamicModuleMap dynamicModules = new MusicDynamicModuleMap();

		// Token: 0x02000830 RID: 2096
		[Serializable]
		public class MusicTransition
		{
			// Token: 0x06003F40 RID: 16192 RVA: 0x0018667E File Offset: 0x0018487E
			public List<ValueDropdownItem<string>> GetAllMusicGroupID()
			{
				return Catalog.GetDropdownAllID(Category.MusicGroup, "None");
			}

			// Token: 0x040040C1 RID: 16577
			public string sourceGroup;

			// Token: 0x040040C2 RID: 16578
			public string destinationGroup;

			// Token: 0x040040C3 RID: 16579
			public string musicGroup;

			// Token: 0x040040C4 RID: 16580
			public int timeBeforeTransition;

			// Token: 0x040040C5 RID: 16581
			public Music.MusicTransition.TransitionType transitionType;

			// Token: 0x040040C6 RID: 16582
			public int timePreTransition;

			// Token: 0x040040C7 RID: 16583
			public Music.MusicTransition.TransitionType preTransitionType = Music.MusicTransition.TransitionType.Immediate;

			// Token: 0x040040C8 RID: 16584
			public int timeBeforeDestStart;

			// Token: 0x040040C9 RID: 16585
			public Music.MusicTransition.TransitionType transitionDestStartType;

			// Token: 0x02000BD6 RID: 3030
			public enum TransitionType
			{
				// Token: 0x04004D03 RID: 19715
				OnBeat,
				// Token: 0x04004D04 RID: 19716
				OnBar,
				// Token: 0x04004D05 RID: 19717
				Immediate,
				// Token: 0x04004D06 RID: 19718
				OnGrid
			}
		}
	}
}
