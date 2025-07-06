using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace ThunderRoad
{
	// Token: 0x02000221 RID: 545
	[Serializable]
	public abstract class SkillData : CatalogData, IContainerLoadable<SkillData>
	{
		// Token: 0x060016DB RID: 5851 RVA: 0x00099CD8 File Offset: 0x00097ED8
		public override string SortKey()
		{
			return ((!this.showInTree && !this.isDefaultSkill) ? "AAA" : "") + (8 - (this.isDefaultSkill ? -1 : this.tier)).ToString() + (this.isTierBlocker ? 0 : (8 - (this.isDefaultSkill ? -1 : this.tier))).ToString() + this.id;
		}

		// Token: 0x1700015B RID: 347
		// (get) Token: 0x060016DC RID: 5852 RVA: 0x00099D4D File Offset: 0x00097F4D
		public bool IsCombinedSkill
		{
			get
			{
				return !string.IsNullOrEmpty(this.primarySkillTreeId) && !string.IsNullOrEmpty(this.secondarySkillTreeId) && !this.primarySkillTreeId.Equals(this.secondarySkillTreeId);
			}
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x00099D7F File Offset: 0x00097F7F
		public string GetName()
		{
			return LocalizationManager.Instance.TryGetLocalization("Skills", this.skillTreeDisplayName, null, false);
		}

		// Token: 0x060016DE RID: 5854 RVA: 0x00099D98 File Offset: 0x00097F98
		public string EditorPrefix()
		{
			if (string.IsNullOrEmpty(this.primarySkillTreeId))
			{
				return "";
			}
			string str = (this.allowSkill && this.showInTree) ? "" : "[Disabled] ";
			string format = "{0}{1} ";
			object arg;
			if (!this.IsCombinedSkill)
			{
				arg = this.primarySkillTreeId[0].ToString();
			}
			else
			{
				arg = "X-" + string.Concat<char>(from s in this.primarySkillTreeId[0].ToString() + this.secondarySkillTreeId[0].ToString()
				orderby s
				select s);
			}
			return str + string.Format(format, arg, this.tier + 1);
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x00099E6E File Offset: 0x0009806E
		public string GetDescription()
		{
			return LocalizationManager.Instance.TryGetLocalization("Skills", this.description, null, false);
		}

		// Token: 0x1700015C RID: 348
		// (get) Token: 0x060016E0 RID: 5856 RVA: 0x00099E88 File Offset: 0x00098088
		[JsonIgnore]
		public int Cost
		{
			get
			{
				if (this.isTierBlocker)
				{
					return 0;
				}
				if (this.costOverride >= 0)
				{
					return this.costOverride;
				}
				GameData gameData = Catalog.gameData;
				float cost = (float)((gameData != null) ? gameData.baseSkillCost : 0);
				SkillTreeData skillTreeData;
				if (!this.IsCombinedSkill && this.primarySkillTreeId != null && Catalog.TryGetData<SkillTreeData>(this.primarySkillTreeId, out skillTreeData, true))
				{
					cost *= skillTreeData.costMultiplier;
				}
				GameData gameData2 = Catalog.gameData;
				float f;
				if (!(((gameData2 != null) ? new GameData.ScalingMode?(gameData2.skillCostScalingMode) : null) != GameData.ScalingMode.Exponential))
				{
					float num = cost;
					GameData gameData3 = Catalog.gameData;
					f = num * Mathf.Pow((gameData3 != null) ? gameData3.skillCostMultiplierPerTier : 2f, (float)this.tier);
				}
				else
				{
					f = cost * (float)(this.tier + 1);
				}
				return Mathf.FloorToInt(f);
			}
		}

		// Token: 0x1400007D RID: 125
		// (add) Token: 0x060016E1 RID: 5857 RVA: 0x00099F58 File Offset: 0x00098158
		// (remove) Token: 0x060016E2 RID: 5858 RVA: 0x00099F8C File Offset: 0x0009818C
		public static event SkillData.SkillLoadedEvent OnSkillLoadedEvent;

		// Token: 0x1400007E RID: 126
		// (add) Token: 0x060016E3 RID: 5859 RVA: 0x00099FC0 File Offset: 0x000981C0
		// (remove) Token: 0x060016E4 RID: 5860 RVA: 0x00099FF4 File Offset: 0x000981F4
		public static event SkillData.SkillLoadedEvent OnLateSkillsLoadedEvent;

		// Token: 0x1400007F RID: 127
		// (add) Token: 0x060016E5 RID: 5861 RVA: 0x0009A028 File Offset: 0x00098228
		// (remove) Token: 0x060016E6 RID: 5862 RVA: 0x0009A05C File Offset: 0x0009825C
		public static event SkillData.SkillLoadedEvent OnSkillUnloadedEvent;

		// Token: 0x060016E7 RID: 5863 RVA: 0x0009A090 File Offset: 0x00098290
		public string RemoveTreeAffix(string treeName)
		{
			if (treeName == null)
			{
				return null;
			}
			if (treeName.EndsWith("Tree") && treeName.Length > 4)
			{
				int length = treeName.Length - 4 - 0;
				return treeName.Substring(0, length);
			}
			return treeName;
		}

		// Token: 0x060016E8 RID: 5864 RVA: 0x0009A0D0 File Offset: 0x000982D0
		public string GetCatalogEditorTreeName()
		{
			if (string.Compare(this.primarySkillTreeId, this.secondarySkillTreeId, StringComparison.Ordinal) < 0)
			{
				return this.RemoveTreeAffix(this.primarySkillTreeId) + this.secondarySkillTreeId;
			}
			return this.RemoveTreeAffix(this.secondarySkillTreeId) + this.primarySkillTreeId;
		}

		// Token: 0x060016E9 RID: 5865 RVA: 0x0009A121 File Offset: 0x00098321
		public bool IsOnTree(string treeId)
		{
			return this.primarySkillTreeId == treeId || this.secondarySkillTreeId == treeId;
		}

		// Token: 0x060016EA RID: 5866 RVA: 0x0009A13F File Offset: 0x0009833F
		public string GetSkillTreeKey()
		{
			return SkillData.GetSkillTreeKey(this.primarySkillTreeId, this.secondarySkillTreeId);
		}

		// Token: 0x060016EB RID: 5867 RVA: 0x0009A152 File Offset: 0x00098352
		public static string GetSkillTreeKey(string primaryTreeName, string secondaryTreeName)
		{
			if (string.Compare(primaryTreeName, secondaryTreeName, StringComparison.Ordinal) < 0)
			{
				return primaryTreeName + secondaryTreeName;
			}
			return secondaryTreeName + primaryTreeName;
		}

		// Token: 0x060016EC RID: 5868 RVA: 0x0009A170 File Offset: 0x00098370
		public void GetVideo(Action<VideoClip> onVideoLoaded)
		{
			if (this.video != null)
			{
				this.videoCount++;
				onVideoLoaded(this.video);
			}
			Catalog.LoadAssetAsync<VideoClip>(this.videoAddress, delegate(VideoClip clip)
			{
				if (clip == null)
				{
					return;
				}
				this.videoCount++;
				this.video = clip;
				onVideoLoaded(this.video);
			}, "SkillData: " + this.id + " loading video");
		}

		// Token: 0x060016ED RID: 5869 RVA: 0x0009A1EC File Offset: 0x000983EC
		public void ReleaseVideo()
		{
			this.videoCount--;
			if (this.videoCount <= 0)
			{
				VideoClip clip = this.video;
				this.video = null;
				if (clip != null)
				{
					Catalog.ReleaseAsset<VideoClip>(clip);
				}
			}
		}

		// Token: 0x060016EE RID: 5870 RVA: 0x0009A230 File Offset: 0x00098430
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			if (SkillData.forceInvalidateIconNames || string.IsNullOrEmpty(this.orbIconAddress))
			{
				this.orbIconAddress = "Bas.Ui.SkillTree.Icons[" + this.id + "]";
			}
			this.primarySkillTree = Catalog.GetData<SkillTreeData>(this.primarySkillTreeId, true);
			this.secondarySkillTree = Catalog.GetData<SkillTreeData>(this.secondarySkillTreeId, true);
			if (SkillData.forceOverrideToLocalisedText)
			{
				this.skillTreeDisplayName = "{" + this.id + "Name}";
				this.description = "{" + this.id + "Description}";
			}
			if (SkillData.forceInvalidateTierBoundMesh && this.isTierBlocker)
			{
				this.prefabAddress = "Bas.Item.Misc.SkillOrb";
				this.meshAddress = string.Format("Bas.Mesh.SkillTree.TierCrystal.{0}.T{1}", this.primarySkillTreeId, this.tier + 1);
				this.meshSize = 0.5f;
			}
			if (SkillData.forceOverrideVideoAddress)
			{
				this.videoAddress = "Bas.Video.Skill." + this.id;
			}
			if (this.IsCombinedSkill)
			{
				string treeA;
				string treeB;
				if (string.CompareOrdinal(this.primarySkillTreeId, this.secondarySkillTreeId) <= 0)
				{
					string text = this.primarySkillTreeId;
					string text2 = this.secondarySkillTreeId;
					treeA = text;
					treeB = text2;
				}
				else
				{
					string text3 = this.secondarySkillTreeId;
					string text2 = this.primarySkillTreeId;
					treeA = text3;
					treeB = text2;
				}
				if (SkillData.forceInvalidateButtonNames || string.IsNullOrEmpty(this.buttonEnabledIconAddress))
				{
					this.buttonEnabledIconAddress = string.Concat(new string[]
					{
						this.buttonSpriteSheetAddress,
						"[",
						treeA,
						"_",
						treeB,
						"_ButtonColor]"
					});
				}
				if (SkillData.forceInvalidateButtonNames || string.IsNullOrEmpty(this.buttonDisabledIconAddress))
				{
					this.buttonDisabledIconAddress = string.Concat(new string[]
					{
						this.buttonSpriteSheetAddress,
						"[",
						treeA,
						"_",
						treeB,
						"_Button]"
					});
					return;
				}
			}
			else if (this.primarySkillTree != null)
			{
				if (SkillData.forceInvalidateButtonNames || (string.IsNullOrEmpty(this.buttonEnabledIconAddress) && string.IsNullOrEmpty(this.primarySkillTree.iconEnabledAddress)))
				{
					this.buttonEnabledIconAddress = this.buttonSpriteSheetAddress + "[" + this.primarySkillTreeId + "_ButtonColor]";
				}
				if (SkillData.forceInvalidateButtonNames || (string.IsNullOrEmpty(this.buttonDisabledIconAddress) && string.IsNullOrEmpty(this.primarySkillTree.iconDisabledAddress)))
				{
					this.buttonDisabledIconAddress = this.buttonSpriteSheetAddress + "[" + this.primarySkillTreeId + "_Button]";
				}
			}
		}

		// Token: 0x060016EF RID: 5871 RVA: 0x0009A4B6 File Offset: 0x000986B6
		public void GetOrbIcon(Action<Sprite> callback)
		{
			Catalog.LoadAssetAsync<Sprite>(this.orbIconAddress, callback, this.id);
		}

		// Token: 0x060016F0 RID: 5872 RVA: 0x0009A4CC File Offset: 0x000986CC
		public void GetButtonIcon(bool enabled, Action<Sprite> callback)
		{
			string address = enabled ? this.buttonEnabledIconAddress : this.buttonDisabledIconAddress;
			if (string.IsNullOrEmpty(address) && !this.IsCombinedSkill)
			{
				address = (enabled ? this.primarySkillTree.iconEnabledAddress : this.primarySkillTree.iconDisabledAddress);
			}
			Catalog.LoadAssetAsync<Sprite>(address, callback, this.id);
		}

		/// <summary>
		/// Called when the skill is loaded on a creature, the creature is the one who has the skill
		/// </summary>
		/// <param name="skillData"></param>
		/// <param name="creature"></param>
		// Token: 0x060016F1 RID: 5873 RVA: 0x0009A524 File Offset: 0x00098724
		public virtual void OnSkillLoaded(SkillData skillData, Creature creature)
		{
			SkillData.SkillLoadedEvent onSkillLoadedEvent = SkillData.OnSkillLoadedEvent;
			if (onSkillLoadedEvent == null)
			{
				return;
			}
			onSkillLoadedEvent(skillData, creature);
		}

		/// <summary>
		/// Called when the skill is unloaded from a creature, the creature is the one who has the skill
		/// </summary>
		/// <param name="skillData"></param>
		/// <param name="creature"></param>
		// Token: 0x060016F2 RID: 5874 RVA: 0x0009A537 File Offset: 0x00098737
		public virtual void OnSkillUnloaded(SkillData skillData, Creature creature)
		{
			SkillData.SkillLoadedEvent onSkillUnloadedEvent = SkillData.OnSkillUnloadedEvent;
			if (onSkillUnloadedEvent == null)
			{
				return;
			}
			onSkillUnloadedEvent(skillData, creature);
		}

		// Token: 0x060016F3 RID: 5875 RVA: 0x0009A54A File Offset: 0x0009874A
		public virtual void OnLateSkillsLoaded(SkillData skillData, Creature creature)
		{
			SkillData.SkillLoadedEvent onLateSkillsLoadedEvent = SkillData.OnLateSkillsLoadedEvent;
			if (onLateSkillsLoadedEvent == null)
			{
				return;
			}
			onLateSkillsLoadedEvent(skillData, creature);
		}

		// Token: 0x060016F4 RID: 5876 RVA: 0x0009A55D File Offset: 0x0009875D
		public void OnLoadedFromContainer(Container container)
		{
			throw new NotImplementedException();
		}

		// Token: 0x060016F5 RID: 5877 RVA: 0x0009A564 File Offset: 0x00098764
		public virtual ContainerContent InstanceContent()
		{
			return new SkillContent(this);
		}

		// Token: 0x060016F6 RID: 5878 RVA: 0x0009A56C File Offset: 0x0009876C
		public List<ValueDropdownItem<string>> GetAllHandPoseID()
		{
			return Catalog.GetDropdownAllID(Category.HandPose, "None");
		}

		// Token: 0x060016F7 RID: 5879 RVA: 0x0009A57A File Offset: 0x0009877A
		public List<ValueDropdownItem<string>> GetAllStatusEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Status, "None");
		}

		// Token: 0x060016F8 RID: 5880 RVA: 0x0009A588 File Offset: 0x00098788
		public List<ValueDropdownItem<string>> GetAllSpellID()
		{
			return Catalog.GetDropdownAllID<SpellData>("None");
		}

		// Token: 0x060016F9 RID: 5881 RVA: 0x0009A594 File Offset: 0x00098794
		public List<ValueDropdownItem<string>> GetAllShardsId()
		{
			return Catalog.GetDropdownAllID(Category.Item, "None");
		}

		// Token: 0x060016FA RID: 5882 RVA: 0x0009A5A1 File Offset: 0x000987A1
		public List<ValueDropdownItem<string>> GetAllEffectID()
		{
			return Catalog.GetDropdownAllID(Category.Effect, "None");
		}

		// Token: 0x060016FB RID: 5883 RVA: 0x0009A5AE File Offset: 0x000987AE
		public List<ValueDropdownItem<string>> GetAllSkillID()
		{
			return Catalog.GetDropdownAllID(Category.Skill, "None");
		}

		// Token: 0x060016FC RID: 5884 RVA: 0x0009A5BC File Offset: 0x000987BC
		public List<ValueDropdownItem<string>> GetAllItemID()
		{
			return Catalog.GetDropdownAllID(Category.Item, "None");
		}

		// Token: 0x060016FD RID: 5885 RVA: 0x0009A5C9 File Offset: 0x000987C9
		public List<ValueDropdownItem<string>> GetAllSkillTreeID()
		{
			return Catalog.GetDropdownAllID(Category.SkillTree, "None");
		}

		// Token: 0x060016FE RID: 5886 RVA: 0x0009A5D7 File Offset: 0x000987D7
		public List<ValueDropdownItem<string>> GetAllDamagerID()
		{
			return Catalog.GetDropdownAllID(Category.Damager, "None");
		}

		// Token: 0x060016FF RID: 5887 RVA: 0x0009A5E5 File Offset: 0x000987E5
		public List<ValueDropdownItem<string>> GetAllMaterialID()
		{
			return Catalog.GetDropdownAllID(Category.Material, "None");
		}

		// Token: 0x04001601 RID: 5633
		public string shardId = "Crystal_Small_01_Shard";

		// Token: 0x04001602 RID: 5634
		public string prefabAddress = "Bas.Item.Misc.SkillOrb";

		// Token: 0x04001603 RID: 5635
		public string meshAddress;

		// Token: 0x04001604 RID: 5636
		public float meshSize = 1f;

		// Token: 0x04001605 RID: 5637
		public string orbLinkEffectId = "SkillTreeOrbLink";

		// Token: 0x04001606 RID: 5638
		public int tier;

		// Token: 0x04001607 RID: 5639
		public bool allowSkill = true;

		// Token: 0x04001608 RID: 5640
		public bool forceAllowRefund;

		// Token: 0x04001609 RID: 5641
		public bool showInTree = true;

		// Token: 0x0400160A RID: 5642
		public bool hideInSkillMenu;

		// Token: 0x0400160B RID: 5643
		public string skillTreeDisplayName;

		// Token: 0x0400160C RID: 5644
		public string description;

		// Token: 0x0400160D RID: 5645
		public string imageAddress = "";

		// Token: 0x0400160E RID: 5646
		public string videoAddress = "";

		// Token: 0x0400160F RID: 5647
		public string buttonSpriteSheetAddress = "Bas.Ui.SkillTree.Icons";

		// Token: 0x04001610 RID: 5648
		public string buttonEnabledIconAddress;

		// Token: 0x04001611 RID: 5649
		public string buttonDisabledIconAddress;

		// Token: 0x04001612 RID: 5650
		public string orbIconAddress = "";

		// Token: 0x04001613 RID: 5651
		private int videoCount;

		// Token: 0x04001614 RID: 5652
		public string tutorial;

		// Token: 0x04001615 RID: 5653
		public string tutorialLocalizationId;

		// Token: 0x04001616 RID: 5654
		public string tutorialGoal;

		// Token: 0x04001617 RID: 5655
		public string tutorialGoalLocalizationId;

		// Token: 0x04001618 RID: 5656
		public int costOverride = -1;

		// Token: 0x04001619 RID: 5657
		[FormerlySerializedAs("defaultSkill")]
		public bool isDefaultSkill;

		// Token: 0x0400161A RID: 5658
		[NonSerialized]
		public EffectData orbLinkEffectData;

		// Token: 0x0400161B RID: 5659
		[NonSerialized]
		public Texture image;

		// Token: 0x0400161C RID: 5660
		[NonSerialized]
		public VideoClip video;

		// Token: 0x04001620 RID: 5664
		public string primarySkillTreeId;

		// Token: 0x04001621 RID: 5665
		[NonSerialized]
		public SkillTreeData primarySkillTree;

		// Token: 0x04001622 RID: 5666
		public string secondarySkillTreeId;

		// Token: 0x04001623 RID: 5667
		[NonSerialized]
		public SkillTreeData secondarySkillTree;

		// Token: 0x04001624 RID: 5668
		public bool isTierBlocker;

		// Token: 0x04001625 RID: 5669
		private static bool forceInvalidateIconNames;

		// Token: 0x04001626 RID: 5670
		private static bool forceInvalidateButtonNames;

		// Token: 0x04001627 RID: 5671
		private static bool forceInvalidateTierBoundMesh;

		// Token: 0x04001628 RID: 5672
		private static bool forceOverrideToLocalisedText;

		// Token: 0x04001629 RID: 5673
		private static bool forceOverrideVideoAddress;

		// Token: 0x02000838 RID: 2104
		// (Invoke) Token: 0x06003F5B RID: 16219
		public delegate void SkillLoadedEvent(SkillData skill, Creature creature);
	}
}
