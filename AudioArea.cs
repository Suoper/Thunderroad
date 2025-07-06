using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x020002B8 RID: 696
	[AddComponentMenu("ThunderRoad/Levels/Audio Area")]
	public class AudioArea : MonoBehaviour
	{
		// Token: 0x060021C7 RID: 8647 RVA: 0x000E8D48 File Offset: 0x000E6F48
		private void OnValidate()
		{
			foreach (AudioArea.AudioSourceParam asp in this.audioSources)
			{
				if (asp.cutoffFrequencyCurve.keys.Length == 0)
				{
					if (asp.minMaxVolume.x <= 0f && asp.minMaxVolume.y <= 0f)
					{
						asp.minMaxVolume = new Vector2(0f, 1f);
					}
					asp.ResetcutoffFrequencyCurve();
				}
			}
		}

		// Token: 0x060021C8 RID: 8648 RVA: 0x000E8DE4 File Offset: 0x000E6FE4
		private void Start()
		{
			if (this.setAreaOnstart)
			{
				this.Enter();
			}
		}

		// Token: 0x060021C9 RID: 8649 RVA: 0x000E8DF4 File Offset: 0x000E6FF4
		private void OnEnable()
		{
			AudioArea.all.Add(this);
		}

		// Token: 0x060021CA RID: 8650 RVA: 0x000E8E01 File Offset: 0x000E7001
		private void OnDisable()
		{
			AudioArea.all.Remove(this);
		}

		// Token: 0x060021CB RID: 8651 RVA: 0x000E8E10 File Offset: 0x000E7010
		public void Enter()
		{
			if (Application.isPlaying)
			{
				foreach (AudioArea audioArea in AudioArea.all)
				{
					if (!(audioArea == this))
					{
						audioArea.UpdateIntensity(0f, 1f, true);
					}
				}
				this.UpdateIntensity(1f, 1f, true);
			}
		}

		// Token: 0x060021CC RID: 8652 RVA: 0x000E8E90 File Offset: 0x000E7090
		public void UpdateIntensity(float targetVolumeIntensity, float cutoffFrequencyIntensity, bool instant = false)
		{
			targetVolumeIntensity = Mathf.Clamp01(targetVolumeIntensity);
			cutoffFrequencyIntensity = Mathf.Clamp01(cutoffFrequencyIntensity);
			foreach (AudioArea.AudioSourceParam asp in this.audioSources)
			{
				if (asp.audioSource)
				{
					float targetVolume = Mathf.Lerp(asp.minMaxVolume.x, asp.minMaxVolume.y, targetVolumeIntensity);
					if (instant)
					{
						asp.audioSource.volume = targetVolume;
						asp.targetReached = true;
					}
					else if (asp.audioSource.volume != targetVolume)
					{
						asp.audioSource.volume = Mathf.MoveTowards(asp.audioSource.volume, targetVolume, Time.deltaTime * this.volumeChangeSpeed);
						asp.targetReached = false;
					}
					else
					{
						asp.targetReached = true;
					}
				}
				else
				{
					asp.targetReached = true;
				}
				if (asp.audioLowPassFilter)
				{
					asp.audioLowPassFilter.cutoffFrequency = asp.cutoffFrequencyCurve.Evaluate(cutoffFrequencyIntensity);
				}
			}
		}

		// Token: 0x060021CD RID: 8653 RVA: 0x000E8FA8 File Offset: 0x000E71A8
		public bool IsTargetVolumeReached()
		{
			using (List<AudioArea.AudioSourceParam>.Enumerator enumerator = this.audioSources.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					if (!enumerator.Current.targetReached)
					{
						return false;
					}
				}
			}
			return true;
		}

		// Token: 0x040020AF RID: 8367
		public static List<AudioArea> all = new List<AudioArea>();

		// Token: 0x040020B0 RID: 8368
		public bool setAreaOnstart;

		// Token: 0x040020B1 RID: 8369
		public float volumeChangeSpeed = 0.5f;

		// Token: 0x040020B2 RID: 8370
		public List<AudioArea.AudioSourceParam> audioSources;

		// Token: 0x02000984 RID: 2436
		[Serializable]
		public class AudioSourceParam
		{
			// Token: 0x060043D6 RID: 17366 RVA: 0x0018FE28 File Offset: 0x0018E028
			public void ResetcutoffFrequencyCurve()
			{
				this.cutoffFrequencyCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 100f),
					new Keyframe(1f, 22000f, 30000f, 30000f)
				});
			}

			// Token: 0x040044D2 RID: 17618
			public AudioSource audioSource;

			// Token: 0x040044D3 RID: 17619
			public Vector2 minMaxVolume = new Vector2(0f, 1f);

			// Token: 0x040044D4 RID: 17620
			public AudioLowPassFilter audioLowPassFilter;

			// Token: 0x040044D5 RID: 17621
			public AnimationCurve cutoffFrequencyCurve;

			// Token: 0x040044D6 RID: 17622
			[NonSerialized]
			public bool targetReached;
		}
	}
}
