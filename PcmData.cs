using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000330 RID: 816
	[Serializable]
	public class PcmData
	{
		// Token: 0x060025E0 RID: 9696 RVA: 0x00104808 File Offset: 0x00102A08
		public PcmData(AudioClip audioClip)
		{
			this.data = new float[audioClip.samples * audioClip.channels];
			this.buffersize = audioClip.samples * audioClip.channels;
			audioClip.GetData(this.data, 0);
			this.sampleRate = audioClip.frequency;
			this.channelMask = audioClip.channels;
		}

		// Token: 0x060025E1 RID: 9697 RVA: 0x0010486C File Offset: 0x00102A6C
		private void UpdateSamples(float[] sourceData, double sourceFrequency, int sourceChannelCount, int sourceChannel)
		{
			double stepSizePrecise = (sourceFrequency + 1E-06) / (double)OVRHaptics.Config.SampleRateHz;
			if (stepSizePrecise < 1.0)
			{
				return;
			}
			int stepSize = (int)stepSizePrecise;
			double stepSizeError = stepSizePrecise - (double)stepSize;
			double accumulatedStepSizeError = 0.0;
			int length = sourceData.Length;
			int count = 0;
			int capacity = length / sourceChannelCount / stepSize + 1;
			this.samples = new byte[capacity * OVRHaptics.Config.SampleSizeInBytes];
			int i = sourceChannel % sourceChannelCount;
			while (i < length)
			{
				if (OVRHaptics.Config.SampleSizeInBytes == 1)
				{
					if (count >= capacity)
					{
						return;
					}
					if (OVRHaptics.Config.SampleSizeInBytes == 1)
					{
						this.samples[count * OVRHaptics.Config.SampleSizeInBytes] = (byte)(Mathf.Clamp01(Mathf.Abs(sourceData[i])) * 255f);
					}
					count++;
				}
				i += stepSize * sourceChannelCount;
				accumulatedStepSizeError += stepSizeError;
				if ((int)accumulatedStepSizeError > 0)
				{
					i += (int)accumulatedStepSizeError * sourceChannelCount;
					accumulatedStepSizeError -= (double)((int)accumulatedStepSizeError);
				}
			}
		}

		// Token: 0x040025EC RID: 9708
		public float[] data;

		// Token: 0x040025ED RID: 9709
		public byte[] samples;

		// Token: 0x040025EE RID: 9710
		public int buffersize;

		// Token: 0x040025EF RID: 9711
		public int sampleRate;

		// Token: 0x040025F0 RID: 9712
		public int channelMask;
	}
}
