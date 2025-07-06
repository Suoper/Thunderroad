using System;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000293 RID: 659
	[Serializable]
	public class FxBlendCurves
	{
		// Token: 0x06001F0C RID: 7948 RVA: 0x000D3D98 File Offset: 0x000D1F98
		public FxBlendCurves()
		{
			this.intensityCurve = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 1f),
				new Keyframe(1f, 1f)
			});
			this.speedCurve = new AnimationCurve(new Keyframe[]
			{
				new Keyframe(0f, 1f),
				new Keyframe(1f, 1f)
			});
		}

		// Token: 0x06001F0D RID: 7949 RVA: 0x000D3E25 File Offset: 0x000D2025
		public FxBlendCurves(AnimationCurve curve)
		{
			this.intensityCurve = curve;
			this.speedCurve = curve;
		}

		// Token: 0x06001F0E RID: 7950 RVA: 0x000D3E3B File Offset: 0x000D203B
		public bool IsUsed()
		{
			return this.useIntensityCurve || this.useSpeedCurve;
		}

		// Token: 0x06001F0F RID: 7951 RVA: 0x000D3E50 File Offset: 0x000D2050
		public bool TryGetValue(float intensity, float speed, out float value)
		{
			if (this.useIntensityCurve && this.useSpeedCurve)
			{
				switch (this.blendMode)
				{
				case BlendMode.Min:
					value = Mathf.Min(this.intensityCurve.Evaluate(intensity), this.speedCurve.Evaluate(speed));
					return true;
				case BlendMode.Max:
					value = Mathf.Max(this.intensityCurve.Evaluate(intensity), this.speedCurve.Evaluate(speed));
					return true;
				case BlendMode.Average:
					value = Mathf.Lerp(this.intensityCurve.Evaluate(intensity), this.speedCurve.Evaluate(speed), 0.5f);
					return true;
				case BlendMode.Multiply:
					value = this.intensityCurve.Evaluate(intensity) * this.speedCurve.Evaluate(speed);
					return true;
				}
			}
			else
			{
				if (this.useIntensityCurve)
				{
					value = this.intensityCurve.Evaluate(intensity);
					return true;
				}
				if (this.useSpeedCurve)
				{
					value = this.speedCurve.Evaluate(speed);
					return true;
				}
			}
			value = 0f;
			return false;
		}

		// Token: 0x04001E0D RID: 7693
		public bool useIntensityCurve;

		// Token: 0x04001E0E RID: 7694
		public AnimationCurve intensityCurve;

		// Token: 0x04001E0F RID: 7695
		public bool useSpeedCurve;

		// Token: 0x04001E10 RID: 7696
		public AnimationCurve speedCurve;

		// Token: 0x04001E11 RID: 7697
		public BlendMode blendMode;
	}
}
