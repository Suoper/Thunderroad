using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x0200024C RID: 588
	public class ArmorSFX : ThunderBehaviour
	{
		// Token: 0x060018A2 RID: 6306 RVA: 0x000A2A94 File Offset: 0x000A0C94
		private void Awake()
		{
			this.creature = base.GetComponent<Creature>();
			this.creature.OnDataLoaded += this.OnCreatureDataLoaded;
			this.footStep = this.creature.GetComponentInChildren<Footstep>();
			this.footStep.OnStep += this.PlayFootStepArmorFX;
			this.creature.OnThisCreatureAttackEvent += this.OnThisCreatureAttackEvent;
		}

		// Token: 0x060018A3 RID: 6307 RVA: 0x000A2B03 File Offset: 0x000A0D03
		private void OnDestroy()
		{
			this.creature.OnThisCreatureAttackEvent -= this.OnThisCreatureAttackEvent;
			this.footStep.OnStep -= this.PlayFootStepArmorFX;
		}

		// Token: 0x060018A4 RID: 6308 RVA: 0x000A2B33 File Offset: 0x000A0D33
		private void OnCreatureDataLoaded()
		{
			this.CalculateEffectData();
		}

		// Token: 0x060018A5 RID: 6309 RVA: 0x000A2B3C File Offset: 0x000A0D3C
		private void OnThisCreatureAttackEvent(Creature targetCreature, Transform targetTransform, BrainModuleAttack.AttackType type, BrainModuleAttack.AttackStage stage)
		{
			if (stage == BrainModuleAttack.AttackStage.Attack || stage == BrainModuleAttack.AttackStage.WindUp || stage == BrainModuleAttack.AttackStage.FollowThrough)
			{
				float intensity = 1f;
				switch (stage)
				{
				case BrainModuleAttack.AttackStage.WindUp:
					intensity = 1f;
					break;
				case BrainModuleAttack.AttackStage.Attack:
					intensity = 1f;
					break;
				case BrainModuleAttack.AttackStage.FollowThrough:
					intensity = 0.5f;
					break;
				}
				this.PlaySFX(this.creature.transform.position, intensity);
			}
		}

		// Token: 0x060018A6 RID: 6310 RVA: 0x000A2B9F File Offset: 0x000A0D9F
		public void PlayFootStepArmorFX(Vector3 position, Side side, float velocity)
		{
			if (this.effectPrio > 1)
			{
				this.PlaySFX(position, velocity);
			}
		}

		// Token: 0x060018A7 RID: 6311 RVA: 0x000A2BB4 File Offset: 0x000A0DB4
		private void PlaySFX(Vector3 position, float intensity)
		{
			if (!Player.local)
			{
				return;
			}
			if (this.effectPrio < 1)
			{
				return;
			}
			if (this.armorSFXData == null)
			{
				return;
			}
			if (Time.time - this.timeSinceLastPlayed < this.blockTime)
			{
				return;
			}
			EffectInstance effectInstance = EffectInstance.Spawn(this.armorSFXData, position, Quaternion.LookRotation(Vector3.forward), intensity, 0f, null, null, true, null, true, Array.Empty<Type>());
			effectInstance.SetNoise(true);
			effectInstance.source = this.creature;
			effectInstance.Play(0, false, false);
			this.timeSinceLastPlayed = Time.time;
		}

		// Token: 0x060018A8 RID: 6312 RVA: 0x000A2C44 File Offset: 0x000A0E44
		public ItemContent[] GetCorePartContents()
		{
			List<string> coreChannels = new List<string>
			{
				"Torso",
				"Legs",
				"Feet"
			};
			List<ItemContent> contents = new List<ItemContent>();
			List<ContainerContent> contents2 = this.creature.container.contents;
			bool checkDataNotNull = true;
			Func<ItemContent, bool>[] array = new Func<ItemContent, bool>[1];
			array[0] = ((ItemContent content) => content.HasState<ContentStateWorn>());
			foreach (ItemContent content2 in contents2.GetEnumerableContentsOfType(checkDataNotNull, array))
			{
				ItemModuleWardrobe moduleWardrobe;
				ItemModuleWardrobe.CreatureWardrobe creatureWardrobe;
				if (content2.data.TryGetModule<ItemModuleWardrobe>(out moduleWardrobe) && moduleWardrobe.TryGetWardrobe(this.creature, out creatureWardrobe))
				{
					if (content2.data.id == "SantaHat")
					{
						contents.Add(content2);
					}
					else
					{
						for (int i = 0; i < creatureWardrobe.manikinWardrobeData.channels.Length; i++)
						{
							string channel = creatureWardrobe.manikinWardrobeData.channels[i];
							int layer = creatureWardrobe.manikinWardrobeData.layers[i];
							if ((!(channel == "Torso") || layer != 0) && coreChannels.Contains(channel))
							{
								contents.Add(content2);
								break;
							}
						}
					}
				}
			}
			return contents.ToArray();
		}

		// Token: 0x060018A9 RID: 6313 RVA: 0x000A2DA8 File Offset: 0x000A0FA8
		public void CalculateEffectData()
		{
			ItemContent[] corePartContents = this.GetCorePartContents();
			int max = -1;
			string effectID = "";
			this.armorSFXData = null;
			ItemContent[] array = corePartContents;
			for (int i = 0; i < array.Length; i++)
			{
				ItemModuleWardrobe itemModuleWardrobe;
				if (array[i].data.TryGetModule<ItemModuleWardrobe>(out itemModuleWardrobe) && !itemModuleWardrobe.armorSoundEffectID.IsNullOrEmptyOrWhitespace() && itemModuleWardrobe.armorSoundEffectPriority > max)
				{
					max = itemModuleWardrobe.armorSoundEffectPriority;
					effectID = itemModuleWardrobe.armorSoundEffectID;
				}
			}
			this.effectPrio = max;
			if (effectID.IsNullOrEmptyOrWhitespace())
			{
				return;
			}
			this.armorSFXData = Catalog.GetData<EffectData>(effectID, true);
		}

		// Token: 0x040017B2 RID: 6066
		private Creature creature;

		// Token: 0x040017B3 RID: 6067
		private Footstep footStep;

		// Token: 0x040017B4 RID: 6068
		private float timeSinceLastPlayed;

		// Token: 0x040017B5 RID: 6069
		private float blockTime = 0.1f;

		// Token: 0x040017B6 RID: 6070
		private int effectPrio;

		// Token: 0x040017B7 RID: 6071
		private EffectData armorSFXData;
	}
}
