using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020001E6 RID: 486
	[Serializable]
	public class MaterialData : CatalogData
	{
		// Token: 0x060015B4 RID: 5556 RVA: 0x00097178 File Offset: 0x00095378
		public IEnumerable GetAllIDMapColors()
		{
			ValueDropdownList<int> list = new ValueDropdownList<int>();
			foreach (GameData.IDMapColors idmap in Catalog.gameData.physicMaterialIDMapColours)
			{
				list.Add(new ValueDropdownItem<int>(idmap.label, idmap.id));
			}
			return list;
		}

		// Token: 0x060015B5 RID: 5557 RVA: 0x000971E8 File Offset: 0x000953E8
		public override int GetCurrentVersion()
		{
			return 3;
		}

		// Token: 0x060015B6 RID: 5558 RVA: 0x000971EC File Offset: 0x000953EC
		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			this.idMapColors = new List<Color>();
			foreach (int idMapColorId in this.idMapColorIds)
			{
				Color idMapColor = Catalog.gameData.GetIDMapColor(idMapColorId);
				this.idMapColors.Add(idMapColor);
			}
			this.physicMaterialHash = Animator.StringToHash(this.id + " (Instance)");
			if (this.collisions != null)
			{
				foreach (MaterialData.Collision collision in this.collisions)
				{
					collision.OnCatalogRefresh(this);
				}
			}
			this.defaultCollision = new MaterialData.Collision();
			this.defaultCollision.effects = this.defaultEffects;
			this.defaultCollision.OnCatalogRefresh(this);
		}

		// Token: 0x060015B7 RID: 5559 RVA: 0x000972EC File Offset: 0x000954EC
		public MaterialData.Collision GetCollision(MaterialData materialData)
		{
			int collisionCount = this.collisions.Count;
			for (int i = 0; i < collisionCount; i++)
			{
				MaterialData.Collision collision = this.collisions[i];
				if (collision.targetMaterials.Count == 0)
				{
					return collision;
				}
				int targetMaterialsCount = collision.targetMaterials.Count;
				for (int j = 0; j < targetMaterialsCount; j++)
				{
					if (collision.targetMaterials[j].hashId == materialData.hashId)
					{
						return collision;
					}
				}
			}
			return this.defaultCollision;
		}

		// Token: 0x060015B8 RID: 5560 RVA: 0x0009736C File Offset: 0x0009556C
		public MaterialData.Collision GetCollision(int physicMaterialHash)
		{
			int collisionCount = this.collisions.Count;
			for (int i = 0; i < collisionCount; i++)
			{
				MaterialData.Collision collision = this.collisions[i];
				if (collision.targetMaterials.Count == 0)
				{
					return collision;
				}
				int targetMaterialsCount = collision.targetMaterials.Count;
				for (int j = 0; j < targetMaterialsCount; j++)
				{
					if (collision.targetMaterials[j].physicMaterialHash == physicMaterialHash)
					{
						return collision;
					}
				}
			}
			return this.defaultCollision;
		}

		// Token: 0x060015B9 RID: 5561 RVA: 0x000973E8 File Offset: 0x000955E8
		public static MaterialData GetMaterial(Collider collider)
		{
			int physicMaterialHash = Animator.StringToHash(collider.material.name);
			List<CatalogData> list = Catalog.GetDataList(Category.Material);
			int listCount = list.Count;
			for (int i = 0; i < listCount; i++)
			{
				MaterialData materialData = (MaterialData)list[i];
				if (materialData.physicMaterialHash == physicMaterialHash)
				{
					return materialData;
				}
			}
			return null;
		}

		// Token: 0x060015BA RID: 5562 RVA: 0x00097440 File Offset: 0x00095640
		public static bool TryGetMaterial(int id, out MaterialData materialData)
		{
			List<CatalogData> list = Catalog.GetDataList(Category.Material);
			int listCount = list.Count;
			for (int i = 0; i < listCount; i++)
			{
				MaterialData material = (MaterialData)list[i];
				int count = material.idMapColorIds.Count;
				for (int j = 0; j < count; j++)
				{
					int idMapColor = material.idMapColorIds[j];
					if (id == idMapColor)
					{
						materialData = material;
						return true;
					}
				}
			}
			materialData = null;
			return false;
		}

		// Token: 0x060015BB RID: 5563 RVA: 0x000974B0 File Offset: 0x000956B0
		public static bool TryGetMaterial(Color idMapColor, out MaterialData materialData)
		{
			materialData = MaterialData.GetMaterial(idMapColor);
			return materialData != null;
		}

		// Token: 0x060015BC RID: 5564 RVA: 0x000974C0 File Offset: 0x000956C0
		public static MaterialData GetMaterial(Color idMapColor)
		{
			List<CatalogData> list = Catalog.GetDataList(Category.Material);
			int listCount = list.Count;
			for (int i = 0; i < listCount; i++)
			{
				MaterialData materialData = (MaterialData)list[i];
				int colorsCount = materialData.idMapColors.Count;
				for (int j = 0; j < colorsCount; j++)
				{
					if (materialData.idMapColors[j] == idMapColor)
					{
						return materialData;
					}
				}
			}
			return null;
		}

		// Token: 0x060015BD RID: 5565 RVA: 0x0009752C File Offset: 0x0009572C
		public static bool TryGetMaterials(int sourcePhysicMaterialHash, int targetPhysicMaterialHash, out MaterialData sourceMaterial, out MaterialData targetMaterial)
		{
			sourceMaterial = null;
			targetMaterial = null;
			List<CatalogData> list = Catalog.GetDataList(Category.Material);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				MaterialData materialData = (MaterialData)list[i];
				if (materialData.physicMaterialHash == sourcePhysicMaterialHash)
				{
					sourceMaterial = materialData;
				}
				if (materialData.physicMaterialHash == targetPhysicMaterialHash)
				{
					targetMaterial = materialData;
				}
				if (sourceMaterial != null && targetMaterial != null)
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x04001582 RID: 5506
		public int physicMaterialHash;

		// Token: 0x04001583 RID: 5507
		public int apparelProtectionLevel;

		// Token: 0x04001584 RID: 5508
		[Tooltip("The color of the material in the ID map")]
		[NonSerialized]
		public List<Color> idMapColors = new List<Color>();

		// Token: 0x04001585 RID: 5509
		public List<int> idMapColorIds = new List<int>();

		// Token: 0x04001586 RID: 5510
		public List<EffectBundle> defaultEffects = new List<EffectBundle>();

		// Token: 0x04001587 RID: 5511
		protected MaterialData.Collision defaultCollision;

		// Token: 0x04001588 RID: 5512
		public bool isMetal;

		// Token: 0x04001589 RID: 5513
		public List<MaterialData.Collision> collisions = new List<MaterialData.Collision>();

		// Token: 0x02000828 RID: 2088
		[Serializable]
		public class Collision
		{
			// Token: 0x06003F2E RID: 16174 RVA: 0x0018631B File Offset: 0x0018451B
			public List<ValueDropdownItem<string>> GetAllMaterialID()
			{
				return Catalog.GetDropdownAllID(Category.Material, "Default");
			}

			// Token: 0x06003F2F RID: 16175 RVA: 0x0018632C File Offset: 0x0018452C
			public void OnCatalogRefresh(MaterialData source)
			{
				this.sourceMaterialData = source;
				this.targetMaterials = new List<MaterialData>();
				foreach (string id in this.targetMaterialIds)
				{
					MaterialData materialData = Catalog.GetData<MaterialData>(id, true);
					if (materialData != null)
					{
						this.targetMaterials.Add(materialData);
					}
				}
				for (int i = this.effects.Count - 1; i >= 0; i--)
				{
					this.effects[i].OnCatalogRefresh();
					if (Application.isPlaying && this.effects[i].effectData == null)
					{
						this.effects.RemoveAt(i);
					}
				}
			}

			// Token: 0x040040A4 RID: 16548
			[NonSerialized]
			public MaterialData sourceMaterialData;

			// Token: 0x040040A5 RID: 16549
			public List<string> targetMaterialIds = new List<string>();

			// Token: 0x040040A6 RID: 16550
			[NonSerialized]
			public List<MaterialData> targetMaterials = new List<MaterialData>();

			// Token: 0x040040A7 RID: 16551
			public List<EffectBundle> effects = new List<EffectBundle>();

			// Token: 0x02000BD3 RID: 3027
			public enum Parenting
			{
				// Token: 0x04004CF8 RID: 19704
				None,
				// Token: 0x04004CF9 RID: 19705
				SourceCollider,
				// Token: 0x04004CFA RID: 19706
				TargetCollider
			}

			// Token: 0x02000BD4 RID: 3028
			public enum Align
			{
				// Token: 0x04004CFC RID: 19708
				Normal,
				// Token: 0x04004CFD RID: 19709
				Velocity
			}

			// Token: 0x02000BD5 RID: 3029
			public enum Rotation
			{
				// Token: 0x04004CFF RID: 19711
				SourceCollider,
				// Token: 0x04004D00 RID: 19712
				TargetCollider,
				// Token: 0x04004D01 RID: 19713
				Random
			}
		}
	}
}
