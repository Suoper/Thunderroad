using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace ThunderRoad
{
	// Token: 0x02000352 RID: 850
	[RequireComponent(typeof(PlayableDirector))]
	public class PlayablePlatformBinder : MonoBehaviour
	{
		// Token: 0x060027E7 RID: 10215 RVA: 0x00111A64 File Offset: 0x0010FC64
		private void Awake()
		{
			this.BindDependingOnPlatform();
		}

		// Token: 0x060027E8 RID: 10216 RVA: 0x00111A6C File Offset: 0x0010FC6C
		private void FindTrack()
		{
			PlayableDirector playableDirector = base.GetComponent<PlayableDirector>();
			if (playableDirector != null && playableDirector.playableAsset != null)
			{
				PlayableBinding[] bindings = playableDirector.playableAsset.outputs.ToArray<PlayableBinding>();
				for (int i = 0; i < bindings.Length; i++)
				{
					Debug.Log(bindings[i].streamName);
					if (bindings[i].streamName == this.trackName)
					{
						this.track = bindings[i].sourceObject;
						return;
					}
				}
			}
		}

		// Token: 0x060027E9 RID: 10217 RVA: 0x00111AF4 File Offset: 0x0010FCF4
		public void BindDependingOnPlatform()
		{
			this.FindTrack();
			PlayableDirector playableDirector = base.GetComponent<PlayableDirector>();
			if (Common.GetQualityLevel(false) == QualityLevel.Windows)
			{
				playableDirector.SetGenericBinding(this.track, this.referencePC);
				return;
			}
			if (Common.GetQualityLevel(false) == QualityLevel.Android)
			{
				playableDirector.SetGenericBinding(this.track, this.referenceAndroid);
				return;
			}
			playableDirector.SetGenericBinding(this.track, this.referencePC);
		}

		// Token: 0x040026D7 RID: 9943
		public string trackName;

		// Token: 0x040026D8 RID: 9944
		public UnityEngine.Object referencePC;

		// Token: 0x040026D9 RID: 9945
		public UnityEngine.Object referenceAndroid;

		// Token: 0x040026DA RID: 9946
		private UnityEngine.Object track;
	}
}
