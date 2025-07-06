using System;

namespace ThunderRoad
{
	// Token: 0x0200035E RID: 862
	public static class Filter
	{
		// Token: 0x06002873 RID: 10355 RVA: 0x00114983 File Offset: 0x00112B83
		public static bool Creatures(ThunderEntity entity)
		{
			return entity is Golem || entity is Creature;
		}

		// Token: 0x06002874 RID: 10356 RVA: 0x00114998 File Offset: 0x00112B98
		public static bool LiveCreatures(ThunderEntity entity)
		{
			Golem golem = entity as Golem;
			if (golem != null)
			{
				if (golem.isKilled)
				{
					goto IL_2A;
				}
			}
			else
			{
				Creature creature = entity as Creature;
				if (creature == null || creature.isKilled)
				{
					goto IL_2A;
				}
			}
			return true;
			IL_2A:
			return false;
		}

		// Token: 0x06002875 RID: 10357 RVA: 0x001149D4 File Offset: 0x00112BD4
		public static bool LiveHumanNPCs(ThunderEntity entity)
		{
			Creature creature = entity as Creature;
			return creature != null && !creature.isKilled && !creature.isPlayer;
		}

		// Token: 0x06002876 RID: 10358 RVA: 0x00114A00 File Offset: 0x00112C00
		public static bool LiveNPCs(ThunderEntity entity)
		{
			if (!(entity is Golem))
			{
				Creature creature = entity as Creature;
				if (creature == null || creature.isKilled || creature.isPlayer)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002877 RID: 10359 RVA: 0x00114A38 File Offset: 0x00112C38
		public static bool MetalNPCs(ThunderEntity entity)
		{
			Creature creature = entity as Creature;
			return creature != null && !creature.isKilled && !creature.isPlayer && creature.HasMetal;
		}

		// Token: 0x06002878 RID: 10360 RVA: 0x00114A68 File Offset: 0x00112C68
		public static bool NPCs(ThunderEntity entity)
		{
			if (!(entity is Golem))
			{
				Creature creature = entity as Creature;
				if (creature == null || creature.isPlayer)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06002879 RID: 10361 RVA: 0x00114A96 File Offset: 0x00112C96
		public static bool Items(ThunderEntity entity)
		{
			return entity is Item;
		}

		// Token: 0x0600287A RID: 10362 RVA: 0x00114AA1 File Offset: 0x00112CA1
		public static bool FreeItems(ThunderEntity entity)
		{
			return entity is Item;
		}

		// Token: 0x0600287B RID: 10363 RVA: 0x00114AAC File Offset: 0x00112CAC
		public static bool AllButPlayer(ThunderEntity entity)
		{
			if (!(entity is Item) && !(entity is Golem))
			{
				Creature creature = entity as Creature;
				if (creature == null || creature.isPlayer)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600287C RID: 10364 RVA: 0x00114AE2 File Offset: 0x00112CE2
		public static Func<ThunderEntity, bool> AllBut(ThunderEntity thisEntity)
		{
			return (ThunderEntity entity) => entity != thisEntity;
		}

		// Token: 0x0600287D RID: 10365 RVA: 0x00114AFB File Offset: 0x00112CFB
		public static Func<ThunderEntity, bool> EnemyOf(Creature creature, bool liveOnly = true)
		{
			return delegate(ThunderEntity entity)
			{
				if (!(entity is Golem))
				{
					Creature other = entity as Creature;
					return other != null && other.faction != creature.faction && (!liveOnly || !other.isKilled);
				}
				return true;
			};
		}

		// Token: 0x0600287E RID: 10366 RVA: 0x00114B1B File Offset: 0x00112D1B
		public static Func<ThunderEntity, bool> LiveCreaturesExcept(Creature thisEntity)
		{
			return (ThunderEntity entity) => entity is Golem || (Filter.LiveCreatures(entity) && entity != thisEntity);
		}
	}
}
