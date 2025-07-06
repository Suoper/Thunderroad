using System;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThunderRoad
{
	// Token: 0x02000298 RID: 664
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/FxModuleAudio.html")]
	public class FxModuleAudio : FxModule
	{
		// Token: 0x06001F2B RID: 7979 RVA: 0x000D46D5 File Offset: 0x000D28D5
		private void OnDestroy()
		{
			if (this.clipLoadedFromAddressable && this.audioSource.clip)
			{
				Addressables.Release<AudioContainer>(this.audioContainer);
			}
		}

		// Token: 0x06001F2C RID: 7980 RVA: 0x000D46FC File Offset: 0x000D28FC
		private void OnValidate()
		{
			this.audioSource = base.GetComponent<AudioSource>();
		}

		// Token: 0x06001F2D RID: 7981 RVA: 0x000D470A File Offset: 0x000D290A
		protected override void ManagedOnDisable()
		{
			this.wasPlaying = this.audioSource.isPlaying;
		}

		// Token: 0x06001F2E RID: 7982 RVA: 0x000D471D File Offset: 0x000D291D
		protected override void ManagedOnEnable()
		{
			if (this.wasPlaying)
			{
				this.audioSource.Play();
			}
		}

		// Token: 0x06001F2F RID: 7983 RVA: 0x000D4734 File Offset: 0x000D2934
		private void Awake()
		{
			this.audioSource = base.GetComponent<AudioSource>();
			if (!this.audioSource)
			{
				this.audioSource = base.gameObject.AddComponent<AudioSource>();
			}
			this.audioSource.spatialBlend = this.spatialBlend;
			this.audioSource.dopplerLevel = 0f;
			this.audioSource.playOnAwake = false;
			this.audioSource.loop = false;
			if (this.audioSource.outputAudioMixerGroup == null)
			{
				this.audioSource.outputAudioMixerGroup = ThunderRoadSettings.GetAudioMixerGroup(this.audioMixer);
			}
			if (this.lowPassCutoffFrequency.IsUsed() || this.lowPassResonanceQCurve.IsUsed())
			{
				this.lowPassFilter = base.GetComponent<AudioLowPassFilter>();
				if (!this.lowPassFilter)
				{
					this.lowPassFilter = base.gameObject.AddComponent<AudioLowPassFilter>();
				}
			}
			if (this.highPassCutoffFrequency.IsUsed() || this.highPassResonanceQCurve.IsUsed())
			{
				this.highPassFilter = base.GetComponent<AudioHighPassFilter>();
				if (!this.highPassFilter)
				{
					this.highPassFilter = base.gameObject.AddComponent<AudioHighPassFilter>();
				}
			}
			if (this.reverbDryLevel.IsUsed())
			{
				this.reverbFilter = base.GetComponent<AudioReverbFilter>();
				if (!this.reverbFilter)
				{
					this.reverbFilter = base.gameObject.AddComponent<AudioReverbFilter>();
				}
				this.reverbFilter.reverbPreset = AudioReverbPreset.Off;
			}
			if (this.fadeInDuration > 0f)
			{
				this.audioSource.volume = 0f;
			}
			else
			{
				this.audioSource.volume = FxModuleAudio.DecibelToLinear(this.volumeDb);
			}
			if (this.audioContainerAddress != null && this.audioContainerAddress != "")
			{
				Catalog.LoadAssetAsync<AudioContainer>(this.audioContainerAddress, delegate(AudioContainer result)
				{
					this.audioContainer = result;
					this.audioSource.clip = this.audioContainer.PickAudioClip();
					if (this.playOnLoad)
					{
						this.Play();
					}
					this.playOnLoad = false;
					this.clipLoadedFromAddressable = true;
				}, this.audioContainerAddress);
				return;
			}
			if (this.audioContainerReference != null && !string.IsNullOrEmpty(this.audioContainerReference.AssetGUID))
			{
				Catalog.LoadAssetAsync<AudioContainer>(this.audioContainerReference, delegate(AudioContainer result)
				{
					this.audioContainer = result;
					this.audioSource.clip = this.audioContainer.PickAudioClip();
					if (this.playOnLoad)
					{
						this.Play();
					}
					this.playOnLoad = false;
					this.clipLoadedFromAddressable = true;
				}, this.audioContainerReference.AssetGUID);
				return;
			}
			if (this.audioClip)
			{
				this.audioSource.clip = this.audioClip;
				this.playOnLoad = false;
				this.clipLoadedFromAddressable = false;
			}
		}

		// Token: 0x06001F30 RID: 7984 RVA: 0x000D496E File Offset: 0x000D2B6E
		public override bool IsPlaying()
		{
			return this.audioSource.isPlaying;
		}

		// Token: 0x06001F31 RID: 7985 RVA: 0x000D497B File Offset: 0x000D2B7B
		public override void SetIntensity(float intensity)
		{
			this.intensity = intensity;
			this.Refresh();
		}

		// Token: 0x06001F32 RID: 7986 RVA: 0x000D498A File Offset: 0x000D2B8A
		public override void SetSpeed(float speed)
		{
			this.speed = ((speed < 1E-05f) ? 0f : speed);
			this.Refresh();
		}

		// Token: 0x06001F33 RID: 7987 RVA: 0x000D49A8 File Offset: 0x000D2BA8
		public void Refresh()
		{
			this.audioSource.spatialBlend = this.spatialBlend;
			float value;
			if (this.volume.TryGetValue(this.intensity, this.speed, out value))
			{
				this.audioSource.volume = value * FxModuleAudio.DecibelToLinear(this.volumeDb);
			}
			if (this.pitch.TryGetValue(this.randomPitch ? UnityEngine.Random.value : this.intensity, this.speed, out value))
			{
				this.audioSource.pitch = value;
			}
			if (this.lowPassCutoffFrequency.TryGetValue(this.intensity, this.speed, out value))
			{
				this.lowPassFilter.cutoffFrequency = value;
			}
			if (this.lowPassResonanceQCurve.TryGetValue(this.intensity, this.speed, out value))
			{
				this.lowPassFilter.lowpassResonanceQ = value;
			}
			if (this.highPassCutoffFrequency.TryGetValue(this.intensity, this.speed, out value))
			{
				this.highPassFilter.cutoffFrequency = value;
			}
			if (this.highPassCutoffFrequency.TryGetValue(this.intensity, this.speed, out value))
			{
				this.highPassFilter.highpassResonanceQ = value;
			}
			if (this.reverbDryLevel.TryGetValue(this.intensity, this.speed, out value))
			{
				this.reverbFilter.dryLevel = value;
			}
			if (this.hasNoise && NoiseManager.isActive)
			{
				this.noise.UpdateVolume(this.audioSource.volume);
			}
		}

		// Token: 0x06001F34 RID: 7988 RVA: 0x000D4B18 File Offset: 0x000D2D18
		public override void Play()
		{
			if (this.playEvent == FxModuleAudio.PlayEvent.Play || this.playEvent == FxModuleAudio.PlayEvent.Loop)
			{
				if (this.controller == null && this.noiseSource == null)
				{
					this.noiseSource = base.GetComponentInParent<Item>();
					if (this.noiseSource == null)
					{
						this.noiseSource = base.GetComponentInParent<Creature>();
					}
				}
				if (this.audioContainer || this.audioSource.clip)
				{
					if (this.audioContainer)
					{
						AudioClip audioClip = this.audioContainer.PickAudioClip();
						if (this.audioSource.clip != audioClip)
						{
							this.audioSource.clip = audioClip;
						}
					}
					if (this.useRandomTime)
					{
						int randomStartTime = UnityEngine.Random.Range(0, this.audioSource.clip.samples - 1);
						this.audioSource.timeSamples = randomStartTime;
					}
					this.audioSource.loop = (this.playEvent == FxModuleAudio.PlayEvent.Loop);
					if (this.playDelay > 0f)
					{
						this.audioSource.PlayDelayed(this.playDelay);
					}
					else if (this.fadeInDuration > 0f)
					{
						this.audioSource.PlayFade(ref this.fadeCoroutine, this, this.fadeInDuration, this.allowAdditivePlay, null);
					}
					else if (this.allowAdditivePlay)
					{
						this.audioSource.PlayOneShot(this.audioSource.clip);
					}
					else
					{
						this.audioSource.Play();
					}
					this.hasNoise = false;
					if (this.abnormalNoise)
					{
						if (this.playEvent == FxModuleAudio.PlayEvent.Play)
						{
							Vector3 position = base.transform.position;
							float num = this.audioSource.volume;
							FxController controller = this.controller;
							this.noise = NoiseManager.AddNoise(position, num, ((controller != null) ? controller.source : null) ?? this.noiseSource);
						}
						else if (this.playEvent == FxModuleAudio.PlayEvent.Loop)
						{
							AudioSource audioSource = this.audioSource;
							FxController controller2 = this.controller;
							this.noise = NoiseManager.AddLoopNoise(audioSource, ((controller2 != null) ? controller2.source : null) ?? this.noiseSource);
						}
						if (this.noise != null)
						{
							this.hasNoise = true;
							return;
						}
					}
				}
				else
				{
					this.playOnLoad = true;
				}
			}
		}

		// Token: 0x06001F35 RID: 7989 RVA: 0x000D4D2C File Offset: 0x000D2F2C
		public override void Stop(bool playStopEffect = true)
		{
			if (this.playEvent == FxModuleAudio.PlayEvent.Stop)
			{
				if (playStopEffect)
				{
					if (this.audioSource.clip != null)
					{
						this.audioSource.Play();
						if (this.abnormalNoise)
						{
							Vector3 position = base.transform.position;
							float num = this.audioSource.volume;
							FxController controller = this.controller;
							NoiseManager.AddNoise(position, num, (controller != null) ? controller.source : null);
						}
					}
					else
					{
						this.playOnLoad = true;
					}
				}
			}
			else if (this.stopDelay > 0f)
			{
				this.DelayedAction(this.stopDelay, delegate
				{
					this.audioSource.Stop();
				});
			}
			else if (this.fadeOutDuration > 0f)
			{
				this.audioSource.StopFade(ref this.fadeCoroutine, this, this.fadeOutDuration);
			}
			else
			{
				this.audioSource.Stop();
			}
			if (this.hasNoise)
			{
				if (this.playEvent == FxModuleAudio.PlayEvent.Loop)
				{
					NoiseManager.RemoveLoopNoise(this.audioSource);
				}
				this.noise = null;
				this.hasNoise = false;
			}
		}

		// Token: 0x06001F36 RID: 7990 RVA: 0x000D4E30 File Offset: 0x000D3030
		public static float LinearToDecibel(float linear)
		{
			float dB;
			if (linear != 0f)
			{
				dB = 20f * Mathf.Log10(linear);
			}
			else
			{
				dB = -144f;
			}
			return dB;
		}

		// Token: 0x06001F37 RID: 7991 RVA: 0x000D4E5B File Offset: 0x000D305B
		public static float DecibelToLinear(float dB)
		{
			return Mathf.Pow(10f, dB / 20f);
		}

		// Token: 0x04001E2F RID: 7727
		[Header("Audio")]
		public string audioContainerAddress;

		// Token: 0x04001E30 RID: 7728
		public AssetReferenceAudioContainer audioContainerReference;

		// Token: 0x04001E31 RID: 7729
		public AudioClip audioClip;

		// Token: 0x04001E32 RID: 7730
		public bool abnormalNoise;

		// Token: 0x04001E33 RID: 7731
		public float volumeDb;

		// Token: 0x04001E34 RID: 7732
		[Range(0f, 1f)]
		public float spatialBlend = 1f;

		// Token: 0x04001E35 RID: 7733
		public float playDelay;

		// Token: 0x04001E36 RID: 7734
		public float stopDelay;

		// Token: 0x04001E37 RID: 7735
		public float fadeInDuration;

		// Token: 0x04001E38 RID: 7736
		public float fadeOutDuration;

		// Token: 0x04001E39 RID: 7737
		public bool useRandomTime;

		// Token: 0x04001E3A RID: 7738
		public bool allowAdditivePlay;

		// Token: 0x04001E3B RID: 7739
		public FxModuleAudio.PlayEvent playEvent;

		// Token: 0x04001E3C RID: 7740
		public AudioMixerName audioMixer = AudioMixerName.Effect;

		// Token: 0x04001E3D RID: 7741
		[Header("Curves")]
		public FxBlendCurves volume = new FxBlendCurves();

		// Token: 0x04001E3E RID: 7742
		public bool randomPitch;

		// Token: 0x04001E3F RID: 7743
		public FxBlendCurves pitch = new FxBlendCurves();

		// Token: 0x04001E40 RID: 7744
		public FxBlendCurves lowPassCutoffFrequency = new FxBlendCurves(new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 22000f),
			new Keyframe(1f, 22000f)
		}));

		// Token: 0x04001E41 RID: 7745
		public FxBlendCurves lowPassResonanceQCurve = new FxBlendCurves();

		// Token: 0x04001E42 RID: 7746
		public FxBlendCurves highPassCutoffFrequency = new FxBlendCurves(new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 10f),
			new Keyframe(1f, 10f)
		}));

		// Token: 0x04001E43 RID: 7747
		public FxBlendCurves highPassResonanceQCurve = new FxBlendCurves();

		// Token: 0x04001E44 RID: 7748
		public FxBlendCurves reverbDryLevel = new FxBlendCurves();

		// Token: 0x04001E45 RID: 7749
		[NonSerialized]
		public AudioSource audioSource;

		// Token: 0x04001E46 RID: 7750
		protected NoiseManager.Noise noise;

		// Token: 0x04001E47 RID: 7751
		protected bool hasNoise;

		// Token: 0x04001E48 RID: 7752
		protected object noiseSource;

		// Token: 0x04001E49 RID: 7753
		protected AudioLowPassFilter lowPassFilter;

		// Token: 0x04001E4A RID: 7754
		protected AudioHighPassFilter highPassFilter;

		// Token: 0x04001E4B RID: 7755
		protected AudioReverbFilter reverbFilter;

		// Token: 0x04001E4C RID: 7756
		public AudioContainer audioContainer;

		// Token: 0x04001E4D RID: 7757
		protected bool playOnLoad;

		// Token: 0x04001E4E RID: 7758
		protected bool clipLoadedFromAddressable;

		// Token: 0x04001E4F RID: 7759
		protected float intensity;

		// Token: 0x04001E50 RID: 7760
		protected float speed;

		// Token: 0x04001E51 RID: 7761
		protected bool wasPlaying;

		// Token: 0x04001E52 RID: 7762
		protected Coroutine fadeCoroutine;

		// Token: 0x02000937 RID: 2359
		public enum PlayEvent
		{
			// Token: 0x0400440D RID: 17421
			Play,
			// Token: 0x0400440E RID: 17422
			Loop,
			// Token: 0x0400440F RID: 17423
			Stop
		}
	}
}
