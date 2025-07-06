using System;
using System.Collections.Generic;
using System.Linq;

namespace ThunderRoad
{
	// Token: 0x02000393 RID: 915
	public class UISelectionListButtonsWardrobe : UISelectionListButtons
	{
		// Token: 0x06002B98 RID: 11160 RVA: 0x001262D2 File Offset: 0x001244D2
		private void Start()
		{
			this.LoadValue();
			this.OnUpdateValue(false);
		}

		// Token: 0x06002B99 RID: 11161 RVA: 0x001262E1 File Offset: 0x001244E1
		private void OnEnable()
		{
			EventManager.OnLanguageChanged += this.OnLanguageChanged;
		}

		// Token: 0x06002B9A RID: 11162 RVA: 0x001262F4 File Offset: 0x001244F4
		private void OnDisable()
		{
			EventManager.OnLanguageChanged -= this.OnLanguageChanged;
		}

		// Token: 0x06002B9B RID: 11163 RVA: 0x00126307 File Offset: 0x00124507
		private void OnLanguageChanged(string language)
		{
			this.RefreshDisplay();
		}

		// Token: 0x06002B9C RID: 11164 RVA: 0x0012630F File Offset: 0x0012450F
		protected override void OnValidate()
		{
			base.OnValidate();
			base.name = this.channel + "." + this.layer;
		}

		// Token: 0x06002B9D RID: 11165 RVA: 0x00126334 File Offset: 0x00124534
		public override void Refresh()
		{
			if (!this.characterSelection)
			{
				this.characterSelection = base.GetComponentInParent<CharacterSelection>();
			}
			if (!this.characterSelection.initialized)
			{
				return;
			}
			this.minValue = 0;
			Creature creature = this.characterSelection.GetCharacterCreature();
			CreatureData.EthnicGroup ethnicGroup = this.characterSelection.GetCharacterCreatureEthnicGroup();
			ItemModuleWardrobe currentWardrobe = null;
			foreach (ContainerContent containerContent in creature.container.contents)
			{
				ItemContent itemContent = containerContent as ItemContent;
				ItemModuleWardrobe itemModuleWardrobe;
				ItemModuleWardrobe.CreatureWardrobe creatureWardrobe;
				if (itemContent != null && itemContent.data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe) && itemModuleWardrobe.category == this.category && itemModuleWardrobe.TryGetWardrobe(creature, out creatureWardrobe) && !(creatureWardrobe.manikinWardrobeData == null) && creatureWardrobe.manikinWardrobeData.channels.Contains(this.channel) && creatureWardrobe.manikinWardrobeData.layers.Contains(ItemModuleWardrobe.GetLayer(this.channel, this.layer)))
				{
					currentWardrobe = itemModuleWardrobe;
					break;
				}
			}
			this.wardrobes = new List<ItemModuleWardrobe>();
			bool isHeadPart = this.channel == "Head" && this.layer == "Body";
			foreach (CatalogData catalogData in Catalog.GetDataList(Category.Item))
			{
				ItemData itemData = catalogData as ItemData;
				ItemModuleWardrobe itemModuleWardrobe2;
				ItemModuleWardrobe.CreatureWardrobe creatureWardrobe2;
				if (itemData != null && itemData.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe2) && itemModuleWardrobe2.category == this.category && itemModuleWardrobe2.TryGetWardrobe(creature, out creatureWardrobe2) && !(creatureWardrobe2.manikinWardrobeData == null) && creatureWardrobe2.manikinWardrobeData.channels.Contains(this.channel) && creatureWardrobe2.manikinWardrobeData.layers.Contains(ItemModuleWardrobe.GetLayer(this.channel, this.layer)))
				{
					if (isHeadPart)
					{
						using (List<string>.Enumerator enumerator3 = ethnicGroup.allowedHeadsIDs.GetEnumerator())
						{
							while (enumerator3.MoveNext())
							{
								if (!(enumerator3.Current != itemModuleWardrobe2.itemData.id))
								{
									this.wardrobes.Add(itemModuleWardrobe2);
									break;
								}
							}
							continue;
						}
					}
					this.wardrobes.Add(itemModuleWardrobe2);
				}
			}
			List<ItemModuleWardrobe> list = this.wardrobes;
			if (list != null && list.Count > 0)
			{
				if (currentWardrobe != null && this.wardrobes.Contains(currentWardrobe))
				{
					this.currentValue = (this.allowNothing ? (this.wardrobes.IndexOf(currentWardrobe) + 1) : this.wardrobes.IndexOf(currentWardrobe));
					this.OnUpdateValue(false);
				}
				else
				{
					this.currentValue = 0;
					this.OnUpdateValue(false);
				}
				this.maxValue = (this.allowNothing ? this.wardrobes.Count : (this.wardrobes.Count - 1));
				this.SetInteractable(true);
			}
			else
			{
				this.SetInteractable(false);
			}
			if (!this.allowNothing && this.maxValue == 0)
			{
				base.gameObject.SetActive(false);
			}
			else if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(true);
			}
			base.Refresh();
		}

		// Token: 0x06002B9E RID: 11166 RVA: 0x001266D0 File Offset: 0x001248D0
		public override void OnUpdateValue(bool silent = false)
		{
			base.OnUpdateValue(silent);
			this.RefreshDisplay();
			Creature creature = this.characterSelection.GetCharacterCreature();
			this.characterSelection.FadeCreature(creature);
			if (this.wardrobes != null && this.wardrobes.Count > 0)
			{
				for (int i = 0; i < creature.container.contents.Count; i++)
				{
					ItemContent itemContent = creature.container.contents[i] as ItemContent;
					if (itemContent != null)
					{
						ItemModuleWardrobe itemModuleWardrobe = itemContent.data.GetModule<ItemModuleWardrobe>();
						if (itemModuleWardrobe != null && itemModuleWardrobe.category == this.category)
						{
							ItemModuleWardrobe.CreatureWardrobe creatureWardrobe = itemModuleWardrobe.GetWardrobe(creature);
							if (creatureWardrobe != null && creatureWardrobe.manikinWardrobeData != null && creatureWardrobe.manikinWardrobeData.channels.Contains(this.channel) && creatureWardrobe.manikinWardrobeData.layers.Contains(ItemModuleWardrobe.GetLayer(this.channel, this.layer)))
							{
								creature.container.RemoveContent(creature.container.contents[i]);
							}
						}
					}
				}
				if (this.allowNothing)
				{
					if (this.currentValue != 0)
					{
						ItemContent content = creature.container.AddItemContent(this.wardrobes[this.currentValue - 1].itemData, null, null);
						creature.equipment.EquipWardrobe(content, true);
						return;
					}
				}
				else
				{
					ItemContent content2 = creature.container.AddItemContent(this.wardrobes[this.currentValue].itemData, null, null);
					creature.equipment.EquipWardrobe(content2, true);
				}
			}
		}

		// Token: 0x06002B9F RID: 11167 RVA: 0x0012686C File Offset: 0x00124A6C
		public override void RefreshDisplay()
		{
			base.RefreshDisplay();
			if (this.allowNothing && this.currentValue == 0)
			{
				this.value.text = LocalizationManager.Instance.GetLocalizedString("Default", "None", false).ToUpper();
				return;
			}
			this.value.text = this.currentValue.ToString().ToUpper();
		}

		// Token: 0x04002921 RID: 10529
		public Equipment.WardRobeCategory category = Equipment.WardRobeCategory.Body;

		// Token: 0x04002922 RID: 10530
		public string channel = "Head";

		// Token: 0x04002923 RID: 10531
		public string layer = "Hair";

		// Token: 0x04002924 RID: 10532
		public bool allowNothing = true;

		// Token: 0x04002925 RID: 10533
		protected List<ItemModuleWardrobe> wardrobes;
	}
}
