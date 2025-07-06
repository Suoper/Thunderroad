using System;
using System.Collections;
using UnityEngine;

namespace ThunderRoad
{
	// Token: 0x02000299 RID: 665
	[HelpURL("https://kospy.github.io/BasSDK/Components/ThunderRoad/FxModuleHaptic")]
	public class FxModuleHaptic : FxModule
	{
		// Token: 0x06001F3C RID: 7996 RVA: 0x000D4FD4 File Offset: 0x000D31D4
		public override void Play()
		{
			if (this.playEvent == FxModuleAudio.PlayEvent.Play)
			{
				this.StartHaptics();
				return;
			}
			if (this.playEvent == FxModuleAudio.PlayEvent.Loop)
			{
				if (this.loopCoroutine != null)
				{
					base.StopCoroutine(this.loopCoroutine);
				}
				this.isLooping = true;
				this.loopCoroutine = base.StartCoroutine(this.LoopHapticCoroutine());
			}
		}

		// Token: 0x06001F3D RID: 7997 RVA: 0x000D5026 File Offset: 0x000D3226
		public override void Stop(bool playStopEffect = true)
		{
			if (this.playEvent == FxModuleAudio.PlayEvent.Stop)
			{
				this.StartHaptics();
				return;
			}
			this.StopHaptics();
		}

		// Token: 0x06001F3E RID: 7998 RVA: 0x000D503E File Offset: 0x000D323E
		public override bool IsPlaying()
		{
			return this.isLooping || Time.time - this.lastPlayTime <= this.clip.duration;
		}

		// Token: 0x06001F3F RID: 7999 RVA: 0x000D5066 File Offset: 0x000D3266
		public override void SetIntensity(float i)
		{
			this.intensity = i;
			this.Refresh();
		}

		// Token: 0x06001F40 RID: 8000 RVA: 0x000D5075 File Offset: 0x000D3275
		public override void SetSpeed(float s)
		{
			this.speed = s;
			this.Refresh();
		}

		// Token: 0x06001F41 RID: 8001 RVA: 0x000D5084 File Offset: 0x000D3284
		protected void Refresh()
		{
			float intensitySampleValue;
			if (this.hapticIntensityCurve.TryGetValue(this.intensity, this.speed, out intensitySampleValue))
			{
				this.intensitySample = intensitySampleValue;
			}
		}

		// Token: 0x06001F42 RID: 8002 RVA: 0x000D50B3 File Offset: 0x000D32B3
		private void StopHaptics()
		{
			this.isLooping = false;
			if (this.loopCoroutine != null)
			{
				base.StopCoroutine(this.loopCoroutine);
			}
		}

		// Token: 0x06001F43 RID: 8003 RVA: 0x000D50D0 File Offset: 0x000D32D0
		private void StartHaptics()
		{
			this.lastPlayTime = Time.time;
			PlayerControl.Hand left;
			PlayerControl.Hand right;
			Handle.HandSide hands = this.GetHands(out left, out right);
			if (hands == Handle.HandSide.Left || hands == Handle.HandSide.Both)
			{
				left.HapticPlayClip(this.clip, 1f);
			}
			if (hands == Handle.HandSide.Right || hands == Handle.HandSide.Both)
			{
				right.HapticPlayClip(this.clip, 1f);
			}
		}

		// Token: 0x06001F44 RID: 8004 RVA: 0x000D5128 File Offset: 0x000D3328
		public void UpdateHandSide(Handle handle, RagdollHand ragdollhand)
		{
			Handle.HandSide side = Handle.HandSide.None;
			if (handle.handlers != null)
			{
				for (int i = 0; i < handle.handlers.Count; i++)
				{
					Side side2 = handle.handlers[i].side;
					if (side2 != Side.Right)
					{
						if (side2 == Side.Left)
						{
							side = ((side == Handle.HandSide.Right || side == Handle.HandSide.Both) ? Handle.HandSide.Both : Handle.HandSide.Left);
						}
					}
					else
					{
						side = ((side == Handle.HandSide.Left || side == Handle.HandSide.Both) ? Handle.HandSide.Both : Handle.HandSide.Right);
					}
				}
			}
			this.UpdateHandSide(side);
		}

		// Token: 0x06001F45 RID: 8005 RVA: 0x000D5191 File Offset: 0x000D3391
		public void UpdateHandSide(Handle.HandSide side)
		{
			this.handSide = side;
		}

		// Token: 0x06001F46 RID: 8006 RVA: 0x000D519A File Offset: 0x000D339A
		protected IEnumerator LoopHapticCoroutine()
		{
			PlayerControl.Hand left;
			PlayerControl.Hand right;
			Handle.HandSide hands = this.GetHands(out left, out right);
			while (this.isLooping)
			{
				this.lastPlayTime = Time.time;
				if (hands == Handle.HandSide.Left || hands == Handle.HandSide.Both)
				{
					left.HapticShort(this.intensitySample, false);
				}
				if (hands == Handle.HandSide.Right || hands == Handle.HandSide.Both)
				{
					right.HapticShort(this.intensitySample, false);
				}
				yield return null;
			}
			yield break;
		}

		// Token: 0x06001F47 RID: 8007 RVA: 0x000D51A9 File Offset: 0x000D33A9
		private Handle.HandSide GetHands(out PlayerControl.Hand left, out PlayerControl.Hand right)
		{
			left = Player.local.GetHand(Side.Left).controlHand;
			right = Player.local.GetHand(Side.Right).controlHand;
			if (!Player.local)
			{
				return Handle.HandSide.None;
			}
			return this.handSide;
		}

		// Token: 0x04001E53 RID: 7763
		[Header("Haptics")]
		public Handle.HandSide handSide = Handle.HandSide.Both;

		// Token: 0x04001E54 RID: 7764
		public FxModuleAudio.PlayEvent playEvent;

		// Token: 0x04001E55 RID: 7765
		public GameData.HapticClip clip;

		// Token: 0x04001E56 RID: 7766
		[Header("Curves")]
		public FxBlendCurves hapticIntensityCurve = new FxBlendCurves();

		// Token: 0x04001E57 RID: 7767
		protected float intensity;

		// Token: 0x04001E58 RID: 7768
		protected float speed;

		// Token: 0x04001E59 RID: 7769
		protected float intensitySample;

		// Token: 0x04001E5A RID: 7770
		protected float lastPlayTime;

		// Token: 0x04001E5B RID: 7771
		protected bool isLooping;

		// Token: 0x04001E5C RID: 7772
		protected Coroutine loopCoroutine;
	}
}
