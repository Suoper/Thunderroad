using System;
using System.Collections;
using ThunderRoad.Pools;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000286 RID: 646
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/Effects/EffectAudio.html")]
	public class EffectAudio : Effect
	{
		// Token: 0x06001E75 RID: 7797 RVA: 0x000CEE8C File Offset: 0x000CD08C
		private void Awake()
		{
			this.audioSource = base.GetComponent<AudioSource>();
			if (!this.audioSource)
			{
				this.audioSource = base.gameObject.AddComponent<AudioSource>();
			}
			this.audioSource.spatialBlend = 1f;
			this.audioSource.dopplerLevel = 0f;
			this.audioSource.playOnAwake = false;
			this.fadeInMult = 1f;
			this.intensitySmoothingBuffer = new float[0];
			this.speedSmoothingBuffer = new float[0];
		}

		// Token: 0x06001E76 RID: 7798 RVA: 0x000CEF14 File Offset: 0x000CD114
		public override void Play()
		{
			if (!this.audioContainer)
			{
				return;
			}
			if (this.audioSource == null)
			{
				Debug.LogError("Audio Source is missing from EffectAudio " + base.name + "!");
				return;
			}
			if (this.invokeRandomPlay)
			{
				base.CancelInvoke("RandomPlay");
				this.invokeRandomPlay = false;
			}
			if (this.invokeDespawn)
			{
				base.CancelInvoke("Despawn");
				this.invokeDespawn = false;
			}
			base.StopAllCoroutines();
			EffectModuleAudio effectModuleAudio = this.module as EffectModuleAudio;
			if (effectModuleAudio != null)
			{
				this.audioSource.spatialBlend = effectModuleAudio.spatialBlend;
				if (this.containingInstance != null && !this.containingInstance.fromPlayer && effectModuleAudio.globalOnPlayerOnly)
				{
					this.audioSource.spatialBlend = 1f;
				}
				if (this.audioSource.clip == null)
				{
					Debug.LogWarning("No Audioclip set on EffectAudio for [" + this.audioContainer.name + "]. Is this Effect not using pooling");
					AudioClip clip;
					if (!this.audioContainer.TryPickAudioClip(out clip))
					{
						Debug.LogError("Picked audio from audioContainer [" + this.audioContainer.name + "] is null ");
						return;
					}
					this.audioSource.clip = clip;
				}
				float pitch = this.audioSource.pitch;
				if (this.randomPitch)
				{
					pitch = this.pitchCurve.Evaluate(UnityEngine.Random.Range(0f, 1f));
				}
				pitch *= this.globalPitch;
				this.audioSource.pitch = pitch;
				this.fadeInMult = 1f;
				if (this.fadeInTime > 0f)
				{
					base.StartCoroutine(this.AudioFadeIn());
				}
				float delay = this.playDelay;
				if (this.onDynamicMusic)
				{
					if (Catalog.gameData.useDynamicMusic && ThunderBehaviourSingleton<MusicManager>.HasInstance)
					{
						this.audioSource.loop = (this.step == Effect.Step.Loop || (this.step == Effect.Step.Custom && this.loopCustomStep));
						double musicDelay = ThunderBehaviourSingleton<MusicManager>.Instance.GetNextTiming(this.dynamicMusicTiming);
						if (musicDelay > 0.0)
						{
							delay = (float)musicDelay;
							this.audioSource.PlayScheduled(AudioSettings.dspTime + musicDelay);
						}
						else
						{
							delay = 0f;
							this.audioSource.Play();
						}
					}
				}
				else if (this.randomPlay)
				{
					Debug.LogWarning(string.Concat(new string[]
					{
						"EffectAudio ",
						base.gameObject.name,
						" for [",
						this.audioContainer.name,
						"] uses randomPlay. This effect may not be pooled"
					}));
					this.audioSource.loop = false;
					this.RandomPlay();
				}
				else
				{
					this.audioSource.loop = (this.step == Effect.Step.Loop || (this.step == Effect.Step.Custom && this.loopCustomStep));
					if (this.playDelay > 0f)
					{
						this.audioSource.PlayDelayed(this.playDelay);
					}
					else
					{
						this.audioSource.Play();
					}
				}
				if (this.randomTime)
				{
					this.audioSource.time = UnityEngine.Random.Range(0f, this.audioSource.clip.length);
				}
				if (effectModuleAudio.useAudioForHaptic)
				{
					if ((this.hapticDevice & HapticDevice.LeftController) == HapticDevice.LeftController)
					{
						PlayerControl.handLeft.Haptic(this.audioContainer.GetPcmData(this.audioSource.clip), this.hapticClipFallBack, false);
					}
					if ((this.hapticDevice & HapticDevice.RightController) == HapticDevice.RightController)
					{
						PlayerControl.handRight.Haptic(this.audioContainer.GetPcmData(this.audioSource.clip), this.hapticClipFallBack, false);
					}
				}
				if (this.step == Effect.Step.Start || this.step == Effect.Step.End)
				{
					base.Invoke("Despawn", this.audioSource.clip.length + delay + 0.1f);
					this.invokeDespawn = true;
				}
				this.hasNoise = false;
				if (this.doNoise)
				{
					if (this.audioSource.loop)
					{
						AudioSource audioSource = this.audioSource;
						EffectInstance containingInstance = this.containingInstance;
						this.noise = NoiseManager.AddLoopNoise(audioSource, (containingInstance != null) ? containingInstance.source : null);
						if (this.noise != null)
						{
							this.hasNoise = true;
						}
					}
					else
					{
						Vector3 position = base.transform.position;
						float volume = this.audioSource.volume;
						EffectInstance containingInstance2 = this.containingInstance;
						this.noise = NoiseManager.AddNoise(position, volume, (containingInstance2 != null) ? containingInstance2.source : null);
						if (this.noise != null)
						{
							this.hasNoise = true;
						}
					}
				}
				this.playTime = Time.time;
			}
		}

		// Token: 0x170001E6 RID: 486
		// (get) Token: 0x06001E77 RID: 7799 RVA: 0x000CF387 File Offset: 0x000CD587
		// (set) Token: 0x06001E78 RID: 7800 RVA: 0x000CF38F File Offset: 0x000CD58F
		public bool invokeDespawn { get; private set; }

		// Token: 0x06001E79 RID: 7801 RVA: 0x000CF398 File Offset: 0x000CD598
		protected void RandomPlay()
		{
			AudioClip audioClip;
			if (this.audioContainer.TryPickAudioClip(out audioClip))
			{
				this.audioSource.clip = audioClip;
				if (!this.audioSource.isPlaying)
				{
					this.audioSource.Play();
				}
				float randomDelay = UnityEngine.Random.Range(this.randomMinTime, this.randomMaxTime);
				base.Invoke("RandomPlay", randomDelay);
				this.invokeRandomPlay = true;
			}
		}

		// Token: 0x170001E7 RID: 487
		// (get) Token: 0x06001E7A RID: 7802 RVA: 0x000CF3FD File Offset: 0x000CD5FD
		// (set) Token: 0x06001E7B RID: 7803 RVA: 0x000CF405 File Offset: 0x000CD605
		public bool invokeRandomPlay { get; private set; }

		// Token: 0x06001E7C RID: 7804 RVA: 0x000CF410 File Offset: 0x000CD610
		public override void Stop()
		{
			if (this.audioSource != null)
			{
				this.audioSource.Stop();
				if (this.audioSource.loop)
				{
					NoiseManager.RemoveLoopNoise(this.audioSource);
				}
			}
			this.noise = null;
			this.hasNoise = false;
			this.hapticDevice = HapticDevice.None;
		}

		// Token: 0x06001E7D RID: 7805 RVA: 0x000CF464 File Offset: 0x000CD664
		public override void End(bool loopOnly = false)
		{
			if (this.invokeRandomPlay)
			{
				base.CancelInvoke("RandomPlay");
				this.invokeRandomPlay = false;
			}
			if (this.loopFadeDelay > 0f)
			{
				base.StopAllCoroutines();
				base.StartCoroutine(this.AudioFadeOut());
				return;
			}
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				this.Despawn();
			}
		}

		// Token: 0x06001E7E RID: 7806 RVA: 0x000CF4C1 File Offset: 0x000CD6C1
		public override void SetHaptic(HapticDevice hapticDevice, GameData.HapticClip hapticClipFallBack)
		{
			this.hapticDevice = hapticDevice;
			this.hapticClipFallBack = hapticClipFallBack;
		}

		// Token: 0x06001E7F RID: 7807 RVA: 0x000CF4D1 File Offset: 0x000CD6D1
		public override void SetNoise(bool noise)
		{
			this.doNoise = noise;
		}

		// Token: 0x06001E80 RID: 7808 RVA: 0x000CF4DC File Offset: 0x000CD6DC
		public float Smooth(float intensity, ref float[] buffer, ref int sampleIndex, int samples)
		{
			if (buffer.Length != samples)
			{
				Array.Resize<float>(ref buffer, samples);
				sampleIndex = 0;
			}
			float avg = intensity;
			if (buffer.Length != 0)
			{
				buffer[sampleIndex] = intensity;
				int num = sampleIndex + 1;
				sampleIndex = num;
				sampleIndex = num % buffer.Length;
				float sum = 0f;
				for (int i = 0; i < buffer.Length; i++)
				{
					sum += buffer[i];
				}
				avg = sum / (float)buffer.Length;
			}
			return Mathf.Abs(avg);
		}

		// Token: 0x06001E81 RID: 7809 RVA: 0x000CF544 File Offset: 0x000CD744
		public override void SetIntensity(float value, bool loopOnly = false)
		{
			base.SetIntensity(value, loopOnly);
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				EffectModuleAudio effectModuleAudio = this.module as EffectModuleAudio;
				if (((effectModuleAudio != null) ? effectModuleAudio.intensitySmoothingSampleCount : 0) > 0)
				{
					value = this.Smooth(value, ref this.intensitySmoothingBuffer, ref this.intensitySmoothingIndex, (this.module as EffectModuleAudio).intensitySmoothingSampleCount);
				}
				this.effectIntensity = value;
				this.Refresh();
			}
		}

		// Token: 0x06001E82 RID: 7810 RVA: 0x000CF5B4 File Offset: 0x000CD7B4
		public override void SetSpeed(float value, bool loopOnly = false)
		{
			if (!loopOnly || (loopOnly && this.step == Effect.Step.Loop))
			{
				EffectModuleAudio effectModuleAudio = this.module as EffectModuleAudio;
				if (((effectModuleAudio != null) ? effectModuleAudio.speedSmoothingSampleCount : 0) > 0)
				{
					value = this.Smooth(value, ref this.speedSmoothingBuffer, ref this.speedSmoothingIndex, (this.module as EffectModuleAudio).speedSmoothingSampleCount);
				}
				this.effectSpeed = value;
				this.Refresh();
			}
		}

		// Token: 0x06001E83 RID: 7811 RVA: 0x000CF61C File Offset: 0x000CD81C
		public static float CalculateVolume(float globalVolumeDb, bool useVolumeIntensity, bool useVolumeSpeed, BlendMode volumeBlendMode, float intensity, float speed, AnimationCurve volumeIntensityCurve, AnimationCurve volumeSpeedCurve)
		{
			if (volumeIntensityCurve == null)
			{
				volumeIntensityCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 1f),
					new Keyframe(1f, 1f)
				});
			}
			if (volumeSpeedCurve == null)
			{
				volumeSpeedCurve = new AnimationCurve(new Keyframe[]
				{
					new Keyframe(0f, 1f),
					new Keyframe(1f, 1f)
				});
			}
			float linearVolume = EffectAudio.DecibelToLinear(globalVolumeDb);
			if (useVolumeIntensity && useVolumeSpeed)
			{
				switch (volumeBlendMode)
				{
				case BlendMode.Min:
					return Mathf.Min(volumeIntensityCurve.Evaluate(intensity), volumeSpeedCurve.Evaluate(speed)) * linearVolume;
				case BlendMode.Max:
					return Mathf.Max(volumeIntensityCurve.Evaluate(intensity), volumeSpeedCurve.Evaluate(speed)) * linearVolume;
				case BlendMode.Average:
					return Mathf.Lerp(volumeIntensityCurve.Evaluate(intensity), volumeSpeedCurve.Evaluate(speed), 0.5f) * linearVolume;
				case BlendMode.Multiply:
					return volumeIntensityCurve.Evaluate(intensity) * volumeIntensityCurve.Evaluate(speed) * linearVolume;
				}
			}
			else
			{
				if (useVolumeIntensity)
				{
					return volumeIntensityCurve.Evaluate(intensity) * linearVolume;
				}
				if (useVolumeSpeed)
				{
					return volumeSpeedCurve.Evaluate(speed) * linearVolume;
				}
			}
			return 1f;
		}

		// Token: 0x06001E84 RID: 7812 RVA: 0x000CF75C File Offset: 0x000CD95C
		public void Refresh()
		{
			this.audioSource.volume = EffectAudio.CalculateVolume(this.globalVolumeDb, this.useVolumeIntensity, this.useVolumeSpeed, this.volumeBlendMode, this.effectIntensity, this.effectSpeed, this.volumeIntensityCurve, this.volumeSpeedCurve) * this.fadeInMult;
			if (this.linkEffectPitch)
			{
				EffectLink effectLink = this.pitchEffectLink;
				if (effectLink != EffectLink.Intensity)
				{
					if (effectLink == EffectLink.Speed)
					{
						this.audioSource.pitch = this.pitchCurve.Evaluate(this.effectSpeed) * this.globalPitch;
					}
				}
				else
				{
					this.audioSource.pitch = this.pitchCurve.Evaluate(this.effectIntensity) * this.globalPitch;
				}
			}
			if (NoiseManager.isActive && this.hasNoise)
			{
				this.noise.UpdateVolume(this.audioSource.volume);
			}
			if (this.useLowPassFilter)
			{
				EffectLink effectLink = this.lowPassEffectLink;
				float num;
				if (effectLink != EffectLink.Intensity)
				{
					if (effectLink != EffectLink.Speed)
					{
						num = 0f;
					}
					else
					{
						num = this.effectSpeed;
					}
				}
				else
				{
					num = this.effectIntensity;
				}
				float value = num;
				this.lowPassFilter.cutoffFrequency = this.lowPassCutoffFrequencyCurve.Evaluate(value);
				this.lowPassFilter.lowpassResonanceQ = this.lowPassResonanceQCurve.Evaluate(value);
			}
			if (this.useHighPassFilter)
			{
				EffectLink effectLink = this.highPassEffectLink;
				float num;
				if (effectLink != EffectLink.Intensity)
				{
					if (effectLink != EffectLink.Speed)
					{
						num = 0f;
					}
					else
					{
						num = this.effectSpeed;
					}
				}
				else
				{
					num = this.effectIntensity;
				}
				float value2 = num;
				this.highPassFilter.cutoffFrequency = this.highPassCutoffFrequencyCurve.Evaluate(value2);
				this.highPassFilter.highpassResonanceQ = this.highPassResonanceQCurve.Evaluate(value2);
			}
			this.audioSource.reverbZoneMix = this.reverbZoneMix;
			if (this.useReverbFilter)
			{
				EffectLink effectLink = this.reverbEffectLink;
				float num;
				if (effectLink != EffectLink.Intensity)
				{
					if (effectLink != EffectLink.Speed)
					{
						num = 0f;
					}
					else
					{
						num = this.effectSpeed;
					}
				}
				else
				{
					num = this.effectIntensity;
				}
				float value3 = num;
				this.reverbFilter.dryLevel = this.reverbDryLevelCurve.Evaluate(value3);
				this.reverbFilter.dryLevel = this.reverbDryLevelCurve.Evaluate(value3);
			}
		}

		// Token: 0x06001E85 RID: 7813 RVA: 0x000CF969 File Offset: 0x000CDB69
		protected IEnumerator AudioFadeIn()
		{
			this.fadeInMult = 0f;
			while (this.fadeInMult < 1f)
			{
				this.fadeInMult += Time.deltaTime / this.fadeInTime;
				this.Refresh();
				yield return Yielders.EndOfFrame;
			}
			yield break;
		}

		// Token: 0x06001E86 RID: 7814 RVA: 0x000CF978 File Offset: 0x000CDB78
		protected IEnumerator AudioFadeOut()
		{
			while (this.audioSource.volume > 0f)
			{
				this.audioSource.volume -= Time.deltaTime / this.loopFadeDelay;
				yield return Yielders.EndOfFrame;
			}
			this.Despawn();
			yield break;
		}

		// Token: 0x06001E87 RID: 7815 RVA: 0x000CF988 File Offset: 0x000CDB88
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

		// Token: 0x06001E88 RID: 7816 RVA: 0x000CF9B3 File Offset: 0x000CDBB3
		public static float DecibelToLinear(float dB)
		{
			return Mathf.Pow(10f, dB / 20f);
		}

		// Token: 0x06001E89 RID: 7817 RVA: 0x000CF9C8 File Offset: 0x000CDBC8
		public void FullStop()
		{
			this.Stop();
			if (this.invokeRandomPlay)
			{
				base.CancelInvoke("RandomPlay");
				this.invokeRandomPlay = false;
			}
			if (this.invokeDespawn)
			{
				base.CancelInvoke("Despawn");
				this.invokeDespawn = false;
			}
			base.StopAllCoroutines();
		}

		/// <summary>
		/// This is used by the pooling system to sort of fake despawn the effect without returning it to the pool, because we are going to use it again right away
		/// </summary>
		// Token: 0x06001E8A RID: 7818 RVA: 0x000CFA18 File Offset: 0x000CDC18
		public void FakeDespawn()
		{
			try
			{
				this.effectIntensity = (this.effectSpeed = 0f);
				this.hapticDevice = HapticDevice.None;
				this.FullStop();
				if (Application.isPlaying)
				{
					base.InvokeDespawnCallback();
				}
			}
			catch (Exception e)
			{
				Debug.LogError(string.Format("Error in EffectAudio.FakeDespawn: {0}", e));
			}
		}

		// Token: 0x06001E8B RID: 7819 RVA: 0x000CFA78 File Offset: 0x000CDC78
		public override void Despawn()
		{
			this.effectIntensity = (this.effectSpeed = 0f);
			this.hapticDevice = HapticDevice.None;
			this.FullStop();
			if (Application.isPlaying)
			{
				EffectModuleAudio.Despawn(this);
				base.InvokeDespawnCallback();
			}
		}

		// Token: 0x04001CE6 RID: 7398
		public AudioContainer audioContainer;

		// Token: 0x04001CE7 RID: 7399
		public float globalVolumeDb;

		// Token: 0x04001CE8 RID: 7400
		public float globalPitch = 1f;

		// Token: 0x04001CE9 RID: 7401
		public float reverbZoneMix = 1f;

		// Token: 0x04001CEA RID: 7402
		public HapticDevice hapticDevice;

		// Token: 0x04001CEB RID: 7403
		public GameData.HapticClip hapticClipFallBack;

		// Token: 0x04001CEC RID: 7404
		public bool loopCustomStep;

		// Token: 0x04001CED RID: 7405
		public bool doNoise;

		// Token: 0x04001CEE RID: 7406
		protected bool hasNoise;

		// Token: 0x04001CEF RID: 7407
		[NonSerialized]
		public EffectAudioPoolManager poolManager;

		// Token: 0x04001CF0 RID: 7408
		protected NoiseManager.Noise noise;

		// Token: 0x04001CF1 RID: 7409
		public bool useVolumeIntensity;

		// Token: 0x04001CF2 RID: 7410
		public AnimationCurve volumeIntensityCurve;

		// Token: 0x04001CF3 RID: 7411
		public bool useVolumeSpeed;

		// Token: 0x04001CF4 RID: 7412
		public AnimationCurve volumeSpeedCurve;

		// Token: 0x04001CF5 RID: 7413
		public BlendMode volumeBlendMode;

		// Token: 0x04001CF6 RID: 7414
		public float loopFadeDelay;

		// Token: 0x04001CF7 RID: 7415
		public float fadeInTime;

		// Token: 0x04001CF8 RID: 7416
		public EffectLink pitchEffectLink;

		// Token: 0x04001CF9 RID: 7417
		public bool randomPitch;

		// Token: 0x04001CFA RID: 7418
		public bool linkEffectPitch;

		// Token: 0x04001CFB RID: 7419
		public AnimationCurve pitchCurve;

		// Token: 0x04001CFC RID: 7420
		public int intensitySmoothingIndex;

		// Token: 0x04001CFD RID: 7421
		public float[] intensitySmoothingBuffer;

		// Token: 0x04001CFE RID: 7422
		public int speedSmoothingIndex;

		// Token: 0x04001CFF RID: 7423
		public float[] speedSmoothingBuffer;

		// Token: 0x04001D00 RID: 7424
		[NonSerialized]
		public float playTime;

		// Token: 0x04001D01 RID: 7425
		public float playDelay;

		// Token: 0x04001D02 RID: 7426
		[Header("Dynamic Music")]
		public bool onDynamicMusic;

		// Token: 0x04001D03 RID: 7427
		public Music.MusicTransition.TransitionType dynamicMusicTiming;

		// Token: 0x04001D04 RID: 7428
		[Header("Random play")]
		public bool randomPlay;

		// Token: 0x04001D05 RID: 7429
		public float randomMinTime = 2f;

		// Token: 0x04001D06 RID: 7430
		public float randomMaxTime = 5f;

		// Token: 0x04001D07 RID: 7431
		[Header("Random Time")]
		public bool randomTime;

		// Token: 0x04001D08 RID: 7432
		[Header("Low pass filter")]
		public bool useLowPassFilter;

		// Token: 0x04001D09 RID: 7433
		public EffectLink lowPassEffectLink;

		// Token: 0x04001D0A RID: 7434
		public AnimationCurve lowPassCutoffFrequencyCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 22000f),
			new Keyframe(1f, 22000f)
		});

		// Token: 0x04001D0B RID: 7435
		public AnimationCurve lowPassResonanceQCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 1f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001D0C RID: 7436
		[Header("High pass filter")]
		public bool useHighPassFilter;

		// Token: 0x04001D0D RID: 7437
		public EffectLink highPassEffectLink;

		// Token: 0x04001D0E RID: 7438
		public AnimationCurve highPassCutoffFrequencyCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 10f),
			new Keyframe(1f, 10f)
		});

		// Token: 0x04001D0F RID: 7439
		public AnimationCurve highPassResonanceQCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 1f),
			new Keyframe(1f, 1f)
		});

		// Token: 0x04001D10 RID: 7440
		[Header("Reverb filter")]
		public bool useReverbFilter;

		// Token: 0x04001D11 RID: 7441
		public EffectLink reverbEffectLink;

		// Token: 0x04001D12 RID: 7442
		public AnimationCurve reverbDryLevelCurve = new AnimationCurve(new Keyframe[]
		{
			new Keyframe(0f, 0f),
			new Keyframe(1f, 0f)
		});

		// Token: 0x04001D13 RID: 7443
		[NonSerialized]
		public AudioSource audioSource;

		// Token: 0x04001D14 RID: 7444
		[NonSerialized]
		public AudioLowPassFilter lowPassFilter;

		// Token: 0x04001D15 RID: 7445
		[NonSerialized]
		public AudioHighPassFilter highPassFilter;

		// Token: 0x04001D16 RID: 7446
		[NonSerialized]
		public AudioReverbFilter reverbFilter;

		// Token: 0x04001D17 RID: 7447
		protected float fadeInMult = 1f;
	}
}
