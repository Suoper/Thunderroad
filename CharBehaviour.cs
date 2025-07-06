using System;
using System.Collections;
using System.Collections.Generic;
using ThunderRoad.Reveal;
using UnityEngine;
using UnityEngine.Rendering;

namespace ThunderRoad
{
	// Token: 0x02000237 RID: 567
	public class CharBehaviour : ThunderBehaviour
	{
		// Token: 0x17000175 RID: 373
		// (get) Token: 0x060017F7 RID: 6135 RVA: 0x0009FD13 File Offset: 0x0009DF13
		public override ManagedLoops EnabledManagedLoops
		{
			get
			{
				return ManagedLoops.Update;
			}
		}

		// Token: 0x060017F8 RID: 6136 RVA: 0x0009FD18 File Offset: 0x0009DF18
		private void Awake()
		{
			this.creature = base.GetComponent<Creature>();
			this.hairColorsPrimary = this.creature.GetColor(Creature.ColorModifier.Hair);
			this.hairColorsSpecular = this.creature.GetColor(Creature.ColorModifier.HairSecondary);
			this.hairColorsSecondary = this.creature.GetColor(Creature.ColorModifier.HairSpecular);
			for (int i = 0; i < this.creature.revealDecals.Count; i++)
			{
				RevealDecal reveal = this.creature.revealDecals[i];
				this.revealMaterialControllers.Add(reveal.revealMaterialController);
			}
			this.creature.OnDespawnEvent -= this.OnDespawn;
			this.creature.OnDespawnEvent += this.OnDespawn;
			EventManager.onLevelUnload -= this.OnLevelLoad;
			EventManager.onLevelUnload += this.OnLevelLoad;
		}

		// Token: 0x060017F9 RID: 6137 RVA: 0x0009FDF5 File Offset: 0x0009DFF5
		public void Init(StatusDataBurning data)
		{
			this.statusData = data;
			this.blitCoroutine = base.StartCoroutine(this.BlitCoroutine());
		}

		// Token: 0x060017FA RID: 6138 RVA: 0x0009FE10 File Offset: 0x0009E010
		private void OnLevelLoad(LevelData level, LevelData.Mode mode, EventTime time)
		{
			if (time == EventTime.OnStart)
			{
				this.Despawn();
			}
		}

		// Token: 0x060017FB RID: 6139 RVA: 0x0009FE1B File Offset: 0x0009E01B
		private void OnDespawn(EventTime time)
		{
			if (time == EventTime.OnStart)
			{
				this.Despawn();
			}
		}

		// Token: 0x060017FC RID: 6140 RVA: 0x0009FE28 File Offset: 0x0009E028
		public void Despawn()
		{
			this.creature.OnDespawnEvent -= this.OnDespawn;
			EventManager.onLevelUnload -= this.OnLevelLoad;
			base.StopCoroutine(this.blitCoroutine);
			this.blitCoroutine = null;
			this.creature = null;
			this.revealMaterialControllers.Clear();
			this.revealMaterialControllers = null;
			UnityEngine.Object.Destroy(this);
		}

		// Token: 0x060017FD RID: 6141 RVA: 0x0009FE90 File Offset: 0x0009E090
		public void Blit(float amount)
		{
			if (this.blitAmount >= 1f)
			{
				return;
			}
			this.blitAmount += amount;
			Vector4 newColor = CharBehaviour.color * amount;
			RevealMaskProjection.BlitColor(this.revealMaterialControllers, newColor, BlendOp.Add, ColorWriteMask.All);
		}

		// Token: 0x060017FE RID: 6142 RVA: 0x0009FED4 File Offset: 0x0009E0D4
		protected IEnumerator BlitCoroutine()
		{
			yield return RevealMaskProjection.ActivateRevealMaterials(this.revealMaterialControllers, 2);
			while (this.blitAmount < 1f)
			{
				float charAmount;
				if (this.creature.TryGetVariable<float>("CharAmount", out charAmount) && charAmount >= this.blitAmount + this.statusData.charRevealStep)
				{
					this.Blit(this.statusData.charRevealStep);
					this.SetHair(1f - Mathf.Clamp01(charAmount));
				}
				yield return null;
			}
			this.SetHair(0f);
			yield break;
		}

		// Token: 0x060017FF RID: 6143 RVA: 0x0009FEE4 File Offset: 0x0009E0E4
		protected Color MultiplyColorNoAlpha(Color color, float amount)
		{
			float alpha = color.a;
			Color mult = color * amount;
			mult.a = alpha;
			return mult;
		}

		// Token: 0x06001800 RID: 6144 RVA: 0x0009FF0C File Offset: 0x0009E10C
		public void SetHair(float amount)
		{
			this.creature.SetColor(this.MultiplyColorNoAlpha(this.hairColorsPrimary, amount), Creature.ColorModifier.Hair, false);
			this.creature.SetColor(this.MultiplyColorNoAlpha(this.hairColorsSpecular, amount), Creature.ColorModifier.HairSecondary, false);
			this.creature.SetColor(this.MultiplyColorNoAlpha(this.hairColorsSecondary, amount), Creature.ColorModifier.HairSpecular, false);
		}

		// Token: 0x04001730 RID: 5936
		private static Vector4 color = new Vector4(0f, 1f, 0f, 0f);

		// Token: 0x04001731 RID: 5937
		public float blitAmount;

		// Token: 0x04001732 RID: 5938
		private Color hairColorsPrimary;

		// Token: 0x04001733 RID: 5939
		private Color hairColorsSpecular;

		// Token: 0x04001734 RID: 5940
		private Color hairColorsSecondary;

		// Token: 0x04001735 RID: 5941
		public Creature creature;

		// Token: 0x04001736 RID: 5942
		public StatusDataBurning statusData;

		// Token: 0x04001737 RID: 5943
		private Coroutine blitCoroutine;

		// Token: 0x04001738 RID: 5944
		private List<RevealMaterialController> revealMaterialControllers = new List<RevealMaterialController>();
	}
}
