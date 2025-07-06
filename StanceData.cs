using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200022D RID: 557
	public abstract class StanceData : CatalogData
	{
		// Token: 0x0600178C RID: 6028
		public abstract IEnumerable<StanceNode> AllStanceNodes();

		// Token: 0x0600178D RID: 6029
		protected abstract StanceData CreateNew();

		// Token: 0x0600178E RID: 6030 RVA: 0x0009DF1D File Offset: 0x0009C11D
		public StanceData GetFilteredClone(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0)
		{
			StanceData stanceData = this.MakeFilteredClone(mainHand, offHand, difficulty);
			stanceData.UpdateAllNodes(false);
			stanceData.PopulateAllNodeLists();
			return stanceData;
		}

		// Token: 0x0600178F RID: 6031 RVA: 0x0009DF38 File Offset: 0x0009C138
		protected virtual StanceData MakeFilteredClone(ItemModuleAI mainHand, ItemModuleAI offHand, int difficulty = 0)
		{
			StanceData stanceData = this.CreateNew();
			stanceData.id = this.id;
			stanceData.sensitiveContent = this.sensitiveContent;
			stanceData.sensitiveFilterBehaviour = this.sensitiveFilterBehaviour;
			stanceData.hashId = this.hashId;
			stanceData.version = this.version;
			stanceData.baseStance = this.baseStance;
			return stanceData;
		}

		// Token: 0x06001790 RID: 6032 RVA: 0x0009DF93 File Offset: 0x0009C193
		protected bool TooDifficult(int threshold, int nodeDifficulty)
		{
			return threshold > 0 && nodeDifficulty > threshold;
		}

		// Token: 0x06001791 RID: 6033 RVA: 0x0009DF9F File Offset: 0x0009C19F
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			this.UpdateAllNodes(false);
		}

		// Token: 0x06001792 RID: 6034 RVA: 0x0009DFAE File Offset: 0x0009C1AE
		public override IEnumerator LoadAddressableAssetsCoroutine()
		{
			yield return this.UpdateAllNodesCoroutine(true);
			yield break;
		}

		// Token: 0x06001793 RID: 6035 RVA: 0x0009DFC0 File Offset: 0x0009C1C0
		public void UpdateAllNodes(bool loadClips)
		{
			List<IEnumerator> list;
			this.UpdateAllNodes(loadClips, out list);
		}

		// Token: 0x06001794 RID: 6036 RVA: 0x0009DFD8 File Offset: 0x0009C1D8
		public void UpdateAllNodes(bool loadClips, out List<IEnumerator> coroutines)
		{
			coroutines = (loadClips ? new List<IEnumerator>() : null);
			using (IEnumerator<StanceNode> enumerator = this.AllStanceNodes().GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					StanceNode node = enumerator.Current;
					if (node != null)
					{
						node.stanceData = this;
						if (loadClips)
						{
							if (node.animationClip)
							{
								Catalog.ReleaseAsset<AnimationClip>(node.animationClip);
							}
							IEnumerator loadAssetCoroutine = Catalog.LoadAssetCoroutine<AnimationClip>(node.address, delegate(AnimationClip value)
							{
								node.animationClip = value;
							}, this.id);
							coroutines.Add(loadAssetCoroutine);
						}
					}
				}
			}
		}

		// Token: 0x06001795 RID: 6037 RVA: 0x0009E09C File Offset: 0x0009C29C
		public IEnumerator UpdateAllNodesCoroutine(bool loadClips)
		{
			List<IEnumerator> coroutines;
			this.UpdateAllNodes(loadClips, out coroutines);
			yield return coroutines.YieldParallel();
			yield break;
		}

		// Token: 0x06001796 RID: 6038 RVA: 0x0009E0B4 File Offset: 0x0009C2B4
		public override void ReleaseAddressableAssets()
		{
			foreach (StanceNode node in this.AllStanceNodes())
			{
				if (node != null)
				{
					if (node.animationClip)
					{
						Catalog.ReleaseAsset<AnimationClip>(node.animationClip);
					}
					node.stanceData = null;
				}
			}
		}

		// Token: 0x06001797 RID: 6039 RVA: 0x0009E11C File Offset: 0x0009C31C
		public void PopulateAllNodeLists()
		{
			foreach (StanceNode stanceNode in this.AllStanceNodes())
			{
				stanceNode.Populate();
			}
		}

		// Token: 0x06001798 RID: 6040
		public abstract IdlePose GetRandomIdle();

		// Token: 0x06001799 RID: 6041
		public abstract IdlePose GetIdleByID(string id);

		// Token: 0x040016FB RID: 5883
		public BrainModuleStance.Stance baseStance = BrainModuleStance.Stance.Melee1H;
	}
}
