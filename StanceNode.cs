using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000235 RID: 565
	[Serializable]
	public abstract class StanceNode
	{
		// Token: 0x1700016B RID: 363
		// (get) Token: 0x060017CF RID: 6095 RVA: 0x0009EEC9 File Offset: 0x0009D0C9
		protected bool isPlaying
		{
			get
			{
				return Application.isPlaying;
			}
		}

		// Token: 0x1700016C RID: 364
		// (get) Token: 0x060017D0 RID: 6096 RVA: 0x0009EED0 File Offset: 0x0009D0D0
		public virtual bool customID
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700016D RID: 365
		// (get) Token: 0x060017D1 RID: 6097 RVA: 0x0009EED3 File Offset: 0x0009D0D3
		public virtual bool showDifficulty
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700016E RID: 366
		// (get) Token: 0x060017D2 RID: 6098 RVA: 0x0009EED6 File Offset: 0x0009D0D6
		public virtual bool showWeight
		{
			get
			{
				return false;
			}
		}

		// Token: 0x1700016F RID: 367
		// (get) Token: 0x060017D3 RID: 6099 RVA: 0x0009EED9 File Offset: 0x0009D0D9
		protected string prettifiedID
		{
			get
			{
				return Utils.AddSpacesToSentence(this.id, true);
			}
		}

		// Token: 0x060017D4 RID: 6100 RVA: 0x0009EEE7 File Offset: 0x0009D0E7
		public void Populate()
		{
			if (!Application.isPlaying)
			{
				return;
			}
			this.PopulateLists();
			this.populated = true;
		}

		// Token: 0x060017D5 RID: 6101
		protected abstract void PopulateLists();

		// Token: 0x060017D6 RID: 6102
		public abstract StanceNode CreateNew();

		// Token: 0x060017D7 RID: 6103 RVA: 0x0009EF00 File Offset: 0x0009D100
		public virtual T Copy<T>() where T : StanceNode
		{
			StanceNode newNode = this.CreateNew();
			newNode.populated = false;
			newNode.id = this.id;
			newNode.address = this.address;
			newNode.animationSpeed = this.animationSpeed;
			newNode.difficulty = this.difficulty;
			newNode.weight = this.weight;
			if (this.animationClip != null)
			{
				newNode.animationClip = this.animationClip;
			}
			return (T)((object)newNode);
		}

		// Token: 0x0400171C RID: 5916
		[NonSerialized]
		public AnimationClip animationClip;

		// Token: 0x0400171D RID: 5917
		[JsonMergeKey]
		public string id = "[New stance animation]";

		// Token: 0x0400171E RID: 5918
		public string address;

		// Token: 0x0400171F RID: 5919
		public float animationSpeed = 1f;

		// Token: 0x04001720 RID: 5920
		public int difficulty;

		// Token: 0x04001721 RID: 5921
		public bool allowPlayAndMove = true;

		// Token: 0x04001722 RID: 5922
		public float weight;

		// Token: 0x04001723 RID: 5923
		[NonSerialized]
		public StanceData stanceData;

		// Token: 0x04001724 RID: 5924
		[NonSerialized]
		public bool populated;
	}
}
