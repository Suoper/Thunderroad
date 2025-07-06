using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace ThunderRoad
{
	// Token: 0x020002A3 RID: 675
	public class TestEffectManager : MonoBehaviour
	{
		// Token: 0x06001F85 RID: 8069 RVA: 0x000D6770 File Offset: 0x000D4970
		private void Update()
		{
			foreach (TestEffectManager.EffectCollection effectCollection in this.effects)
			{
				effectCollection.CheckIntensityChange();
			}
		}

		// Token: 0x04001EB7 RID: 7863
		public List<TestEffectManager.EffectCollection> effects = new List<TestEffectManager.EffectCollection>();

		// Token: 0x0200093E RID: 2366
		[Serializable]
		public class EffectCollection
		{
			// Token: 0x060042F2 RID: 17138 RVA: 0x0018E4C5 File Offset: 0x0018C6C5
			public void Play_Pause()
			{
				if (this.director == null)
				{
					return;
				}
				if (this.director.state == PlayState.Playing)
				{
					this.director.Pause();
					return;
				}
				this.director.Play();
			}

			// Token: 0x060042F3 RID: 17139 RVA: 0x0018E4FC File Offset: 0x0018C6FC
			public void Stop()
			{
				if (this.director == null)
				{
					return;
				}
				this.director.Stop();
				foreach (TestEffect testEffect in this.effects)
				{
					testEffect.Stop();
				}
			}

			// Token: 0x060042F4 RID: 17140 RVA: 0x0018E568 File Offset: 0x0018C768
			public void CheckIntensityChange()
			{
				if (this.intensity == this.lastIntensity)
				{
					return;
				}
				foreach (TestEffect testEffect in this.effects)
				{
					testEffect.intensity = this.intensity;
					testEffect.OnValidate();
				}
				this.lastIntensity = this.intensity;
			}

			// Token: 0x04004429 RID: 17449
			public string name = "Effect Name";

			// Token: 0x0400442A RID: 17450
			public List<TestEffect> effects = new List<TestEffect>();

			// Token: 0x0400442B RID: 17451
			[Header("Timelines")]
			public PlayableDirector director;

			// Token: 0x0400442C RID: 17452
			[Range(0f, 1f)]
			public float intensity = 1f;

			// Token: 0x0400442D RID: 17453
			private float lastIntensity;
		}
	}
}
